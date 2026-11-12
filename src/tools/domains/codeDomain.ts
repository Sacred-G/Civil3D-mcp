import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import { readPluginToken } from "../../utils/pluginAuth.js";
import { Civil3DRpcError, COMMAND_TIMEOUT_MS } from "../../utils/SocketClient.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

// civil3d_run_code: a C# snippet compiled with Roslyn inside the plugin and run
// on Civil 3D's main thread. It exists for work the fixed tools and COM/LISP
// make too slow (2026-10-11: 96,913 objects sampled into 547,441 points in
// 1.2 s in-process against about 20 minutes in LISP). The code runs with the
// user's full rights, so the action is always approval-gated, even with
// CIVIL3D_APPROVAL_MODE=disabled (approvalPolicy.ts), and the plugin saves
// every script with a summary in its run-code log folder.

export const RUN_CODE_MAX_CODE_CHARS = 200_000;
export const RUN_CODE_MAX_TIMEOUT_SECONDS = 90;
// code + args as JSON; leaves room for the envelope under the plugin's 1 MiB
// request limit (SocketClient MAX_REQUEST_BYTES). Checked here so that
// civil3d_request_approval refuses an oversized call before issuing a token.
export const RUN_CODE_MAX_REQUEST_BYTES = 1_000_000;
// The plugin refuses (HOST_BUSY, nothing changed) a run that has not started
// within this window, so a run either gets its full budget inside the client's
// timeout or does not start at all.
export const RUN_CODE_START_WINDOW_SECONDS = 30;
// Compile, the host hop and the reply, on top of the start window and budget.
const RUN_CODE_REPLY_MARGIN_SECONDS = 30;

export const RunCodeArgsSchema = z.object({
  action: z.literal("run"),
  code: z.string().min(1).max(RUN_CODE_MAX_CODE_CHARS),
  mode: z.enum(["read", "write"]).optional(),
  timeoutSeconds: z.number().int().min(1).max(RUN_CODE_MAX_TIMEOUT_SECONDS).optional(),
  maxInlineBytes: z.number().int().min(1024).max(1024 * 1024).optional(),
  args: z.record(z.unknown()).optional(),
}).superRefine((value, ctx) => {
  const bytes = Buffer.byteLength(JSON.stringify({ code: value.code, args: value.args ?? {} }), "utf8");
  if (bytes > RUN_CODE_MAX_REQUEST_BYTES) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ["args"],
      message: `code and args are ${bytes} bytes as JSON; the limit is ${RUN_CODE_MAX_REQUEST_BYTES}. ` +
        "Put large data in a file and pass its path in args (read it with File.ReadAllText in the script), or split the run.",
    });
  }
});

const DiagnosticSchema = z.object({
  line: z.number().int(),
  column: z.number().int(),
  code: z.string(),
  message: z.string(),
});

export const RunCodeResponseSchema = z.object({
  runId: z.string(),
  mode: z.enum(["read", "write"]),
  status: z.enum(["ok", "compile_error", "rejected", "runtime_error", "timeout", "cancelled", "failed"]),
  ok: z.boolean(),
  drawing: z.string().optional(),
  output: z.string().optional(),
  outputFile: z.string().optional(),
  result: z.unknown().optional(),
  resultShape: z.string().optional(),
  resultBytes: z.number().int().optional(),
  resultFile: z.string().optional(),
  resultPreview: z.string().optional(),
  resultError: z.string().optional(),
  files: z.array(z.string()).optional(),
  errors: z.array(DiagnosticSchema).optional(),
  error: z.object({
    type: z.string(),
    message: z.string(),
    line: z.number().int().optional(),
    errorStatus: z.string().optional(),
    inner: z.string().optional(),
    stack: z.string().optional(),
  }).optional(),
  warnings: z.array(z.union([z.string(), DiagnosticSchema])).optional(),
  timing: z.object({
    compileMs: z.number(),
    runMs: z.number(),
    totalMs: z.number(),
  }),
  logFolder: z.string(),
  // "unknown" when the script ended the run's transaction itself.
  committed: z.union([z.boolean(), z.literal("unknown")]).optional(),
}).passthrough();

export const CODE_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "code",
  actions: {
    run: {
      action: "run",
      inputSchema: RunCodeArgsSchema,
      responseSchema: RunCodeResponseSchema,
      // "manage" + not safe for retry: approval is required (and forced for
      // this tool even when approvals are disabled).
      capabilities: ["query", "analyze", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["runCode"],
      execute: async (parsedArgs) => {
        const args = parsedArgs as z.infer<typeof RunCodeArgsSchema>;
        const timeoutSeconds = args.timeoutSeconds ?? 30;
        // Added here, after approval and idempotency hashed the caller's
        // arguments, so the token never enters a hash or a log.
        const authToken = readPluginToken();
        const timeoutMs = Math.max(
          COMMAND_TIMEOUT_MS,
          (timeoutSeconds + RUN_CODE_START_WINDOW_SECONDS + RUN_CODE_REPLY_MARGIN_SECONDS) * 1000,
        );
        try {
          return await withApplicationConnection(
            async (appClient) => await appClient.sendCommand("runCode", {
              code: args.code,
              mode: args.mode ?? "read",
              timeoutSeconds,
              startWithinSeconds: RUN_CODE_START_WINDOW_SECONDS,
              maxInlineBytes: args.maxInlineBytes ?? 65536,
              args: args.args ?? {},
              ...(authToken ? { authToken } : {}),
            }, { timeoutMs }),
          );
        } catch (error) {
          // No answer, so no runId: the agent must not retry blindly.
          if (error instanceof Civil3DRpcError && error.code === "CIVIL3D.TIMEOUT") {
            throw new Civil3DRpcError(
              `${error.message}. The script may have run: before retrying, open the newest folder in ` +
              "%LOCALAPPDATA%\\Civil3DMcpPlugin\\run-code\\ (next to plugin.log) and read its run.json (status, committed).",
              error.code,
              error.rpcCode,
            );
          }
          throw error;
        }
      },
    },
  },
  exposures: [
    {
      toolName: "civil3d_run_code",
      displayName: "Civil 3D Run Code",
      description:
        "Runs a C# snippet inside Civil 3D (compiled with Roslyn, run on the main thread with the drawing locked), for work that is too slow or impossible with the other tools or COM/LISP: bulk geometry over thousands of objects, reading other DWGs as side databases, anything the AutoCAD/Civil 3D .NET API reaches. " +
        "Always needs approval: call civil3d_request_approval with toolName civil3d_run_code, action 'run' and the same parameters, then retry with approvalToken. The code runs with the user's full rights. " +
        "The snippet is the body of `object Run()`: statements, local functions and leading `using` lines; `return` a value (JSON: points and vectors as [x,y,z], ObjectIds as handles, tuples as arrays, anonymous objects by property). " +
        "In scope: Doc, Db, Ed, Civil (CivilDocument), Tr (the run's transaction), Args (the 'args' object), Log(x), Check(), Open<T>(id, forWrite), ModelSpace<T>(db) (calls Check for each entity), OpenSide(path) (another DWG as a read-only side database; the run disposes it, so no using/Dispose), TransactionFor(db), WriteFile(name, text or lines) (into the run folder), Session (a dictionary kept across runs until Civil 3D closes; plain data only). " +
        "Namespaces: System, Linq, IO, Text, Text.Json, Autodesk.AutoCAD ApplicationServices/DatabaseServices/EditorInput/Geometry/Colors, Autodesk.Civil, Civil ApplicationServices/DatabaseServices/Styles; names both AutoCAD and Civil 3D define mean AutoCAD's (DBObject, Entity, Table, Shape...; Civil 3D's are CivilDBObject, CivilEntity...), except Surface and Section, which are Civil 3D's (AcSurface, AcSection); AcException is AutoCAD's exception. " +
        "A script cannot be stopped from outside: call Check() in long loops, which ends it when timeoutSeconds (1-90, default 30) is used up or the request is cancelled; a timeout the script catches still ends the run as a timeout. " +
        "mode 'read' (default): an open-close transaction, never committed; the compiler refuses OpenMode.ForWrite, UpgradeOpen, Erase, AppendEntity, Civil 3D Create/Add/Update..., regular transactions and drawing-setting setters, including ++/-- and deconstruction (a guard against mistakes, not a sandbox). " +
        "mode 'write': runs inside a plugin command; committed as one undo step when the snippet returns, rolled back when it throws or runs out of time (only changes made through Tr are rolled back: write mode refuses open-close transactions; database or system variables, files and Session are not rolled back). Both modes refuse Editor prompts, modal windows and message loops (any ShowDialog/ShowModal, MessageBox, InputBox, Application.Run), event subscriptions, TransactionManager.TopTransaction and Tr.Commit()/Abort() (the run owns Tr: just return; `using` on Tr does nothing); a nested transaction only as `using var t = ...StartTransaction();` (write) or `...StartOpenCloseTransaction()` (read). " +
        "status is ok, compile_error or rejected (errors with snippet line numbers), runtime_error, timeout or cancelled (error with line); committed is true, false or 'unknown' (the transaction was ended some other way: check the drawing). Output or results larger than maxInlineBytes (default 64 KB) go to outputFile/resultFile with a preview. Every script and its summary are saved in logFolder. " +
        "code plus args: about 1 MB of JSON at most; pass larger data as a file path in args. A run that cannot start within 30 s because Civil 3D is busy with other work fails with HOST_BUSY and changes nothing.",
      inputShape: {
        code: z.string().min(1).max(RUN_CODE_MAX_CODE_CHARS),
        mode: z.enum(["read", "write"]).optional(),
        timeoutSeconds: z.number().int().min(1).max(RUN_CODE_MAX_TIMEOUT_SECONDS).optional(),
        maxInlineBytes: z.number().int().min(1024).max(1024 * 1024).optional(),
        args: z.record(z.unknown()).optional(),
      },
      supportedActions: ["run"],
      resolveAction: (rawArgs) => ({ action: "run", args: { ...rawArgs, action: "run" } }),
    },
  ],
};
