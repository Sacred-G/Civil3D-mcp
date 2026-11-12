import { mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";

const { sendCommandMock } = vi.hoisted(() => ({
  sendCommandMock: vi.fn(),
}));

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async <T>(
    operation: (client: { sendCommand: typeof sendCommandMock }) => Promise<T>,
  ) => await operation({
    sendCommand: sendCommandMock,
  }),
}));

import {
  approvalPolicy,
  ApprovalPolicyService,
  ApprovalRequiredError,
  isApprovalRequired,
} from "../src/tools/approvalPolicy.js";
import { registerApprovalTool } from "../src/tools/approvalTool.js";
import { buildExposureAnnotations, captureDomainToolHandlers } from "../src/tools/domainRuntime.js";
import {
  CODE_DOMAIN_DEFINITION,
  RUN_CODE_START_WINDOW_SECONDS,
  RunCodeArgsSchema,
  RunCodeResponseSchema,
} from "../src/tools/domains/codeDomain.js";
import { getToolHandler } from "../src/tools/toolHandlerRegistry.js";
import { GENERATED_TOOL_CATALOG_ENTRIES, findManifestAction } from "../src/tools/toolManifest.js";
import { Civil3DRpcError, COMMAND_TIMEOUT_MS } from "../src/utils/SocketClient.js";

const runTarget = {
  toolName: "civil3d_run_code",
  action: "run",
  capabilities: CODE_DOMAIN_DEFINITION.actions.run.capabilities,
  safeForRetry: CODE_DOMAIN_DEFINITION.actions.run.safeForRetry,
  requiresActiveDrawing: true,
};

describe("civil3d_run_code domain", () => {
  const savedApprovalMode = process.env.CIVIL3D_APPROVAL_MODE;
  afterEach(() => {
    if (savedApprovalMode === undefined) delete process.env.CIVIL3D_APPROVAL_MODE;
    else process.env.CIVIL3D_APPROVAL_MODE = savedApprovalMode;
  });

  it("is registered as one tool that calls the plugin's runCode", () => {
    const entry = GENERATED_TOOL_CATALOG_ENTRIES.find((item) => item.toolName === "civil3d_run_code");
    expect(entry).toBeDefined();
    expect(entry!.domain).toBe("code");
    expect(entry!.pluginMethods).toEqual(["runCode"]);
    expect(entry!.safeForRetry).toBe(false);
    expect(findManifestAction("civil3d_run_code", "run")).toBeDefined();
  });

  it("always requires approval, also when approvals are disabled", () => {
    expect(isApprovalRequired(runTarget)).toBe(true);
    process.env.CIVIL3D_APPROVAL_MODE = "disabled";
    expect(isApprovalRequired(runTarget)).toBe(true);
    // Other tools still honour the switch.
    expect(isApprovalRequired({ ...runTarget, toolName: "civil3d_xref", action: "detach" })).toBe(false);
  });

  it("refuses to run without a token, also when approvals are disabled", async () => {
    process.env.CIVIL3D_APPROVAL_MODE = "disabled";
    const policy = new ApprovalPolicyService(async () => "drawing-a");
    const parameters = { code: "return 1;" };
    await expect(policy.enforce(runTarget, parameters)).rejects.toBeInstanceOf(ApprovalRequiredError);

    const receipt = await policy.requestApproval(runTarget, parameters);
    await expect(policy.enforce(runTarget, { ...parameters, approvalToken: receipt.approvalToken })).resolves.toBeUndefined();
  });

  it("binds the approval to the exact code", async () => {
    const policy = new ApprovalPolicyService(async () => "drawing-a");
    const receipt = await policy.requestApproval(runTarget, { code: "return 1;" });
    await expect(policy.enforce(runTarget, { code: "return 2;", approvalToken: receipt.approvalToken }))
      .rejects.toThrow(/does not match/);
  });

  it("is marked destructive and not read-only for MCP clients", () => {
    const annotations = buildExposureAnnotations(CODE_DOMAIN_DEFINITION, CODE_DOMAIN_DEFINITION.exposures[0]);
    expect(annotations.readOnlyHint).toBe(false);
    expect(annotations.destructiveHint).toBe(true);
    expect(annotations.idempotentHint).toBe(false);
  });

  it("validates the arguments", () => {
    const valid = { action: "run", code: "return Db.Filename;" };
    expect(RunCodeArgsSchema.safeParse(valid).success).toBe(true);
    expect(RunCodeArgsSchema.safeParse({ ...valid, mode: "write", timeoutSeconds: 90, args: { dwg: "C:/bg.dwg" } }).success).toBe(true);
    expect(RunCodeArgsSchema.safeParse({ ...valid, code: "" }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, mode: "admin" }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, timeoutSeconds: 0 }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, timeoutSeconds: 91 }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, maxInlineBytes: 100 }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, args: ["not", "an", "object"] }).success).toBe(false);
    expect(RunCodeArgsSchema.safeParse({ ...valid, code: "x".repeat(200_001) }).success).toBe(false);
  });

  it("refuses code and args over about 1 MB of JSON, counted in UTF-8 bytes", () => {
    const valid = { action: "run", code: "return Args.Count;" };
    expect(RunCodeArgsSchema.safeParse({ ...valid, code: "x".repeat(199_000) }).success).toBe(true);
    // Under the 200k character limit, but escaped as \u0001 (6 bytes each) in JSON.
    expect(RunCodeArgsSchema.safeParse({ ...valid, code: "\u0001".repeat(180_000) }).success).toBe(false);

    const points = Array.from({ length: 30_000 }, (_, i) => [512345.123456 + i, 941234.987654 + i, 12.3456]);
    const result = RunCodeArgsSchema.safeParse({ ...valid, args: { pts: points } });
    expect(result.success).toBe(false);
    const issue = result.success ? undefined : result.error.issues[0];
    expect(issue?.path).toEqual(["args"]);
    expect(issue?.message).toMatch(/bytes as JSON; the limit is 1000000/);
    expect(issue?.message).toMatch(/file/);
  });

  it("issues no approval token for an oversized call", async () => {
    sendCommandMock.mockReset();
    registerApprovalTool({ registerTool: () => undefined } as unknown as McpServer);
    const requestApproval = getToolHandler("civil3d_request_approval")!;
    const result = await requestApproval({
      toolName: "civil3d_run_code",
      action: "run",
      parameters: { code: "return 1;", args: { blob: "x".repeat(1_000_001) } },
    });
    expect(result.isError).toBe(true);
    expect(result.content[0].text).toMatch(/bytes as JSON/);
    expect(result.structuredContent).toBeUndefined();
    expect(sendCommandMock).not.toHaveBeenCalled();
  });

  it("resolves calls without an action to 'run'", () => {
    const resolved = CODE_DOMAIN_DEFINITION.exposures[0].resolveAction({ code: "return 1;", mode: "read" });
    expect(resolved.action).toBe("run");
    expect(RunCodeArgsSchema.parse(resolved.args).code).toBe("return 1;");
  });

  it("accepts the plugin's response shapes", () => {
    const base = { runId: "20261011-120000-abcd1234", mode: "read", logFolder: "C:/log/run-code/20261011-120000-abcd1234", timing: { compileMs: 180, runMs: 1200, totalMs: 1450 } };
    expect(RunCodeResponseSchema.safeParse({
      ...base, status: "ok", ok: true, drawing: "Job.dwg", output: "96913 objects\n",
      result: { objects: 96913, points: 547441 }, resultShape: "<>f__AnonymousType0`2", resultBytes: 37,
    }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({
      ...base, status: "ok", ok: true, result: null, resultFile: "C:/log/run-code/x/result.json",
      resultBytes: 8_400_000, resultPreview: "[[1.00,2.00],...", resultShape: "array of 547441", files: ["C:/log/run-code/x/points.txt"],
    }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({
      ...base, status: "rejected", ok: false,
      errors: [{ line: 3, column: 40, code: "C3DMCP001", message: "Read mode opens objects for read only" }],
      warnings: [{ line: 1, column: 1, code: "CS0105", message: "duplicate using" }, "ran past its budget"],
    }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({
      ...base, status: "timeout", ok: false,
      error: { type: "Civil3DMcpPlugin.RunCodeTimeoutException", message: "budget used up", line: 12, stack: "at Run() in script:line 12" },
    }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({ ...base, status: "exploded", ok: false }).success).toBe(false);
  });

  it("accepts committed true, false and 'unknown'", () => {
    const base = { runId: "r", mode: "write", status: "runtime_error", ok: false, logFolder: "C:/log/run-code/r", timing: { compileMs: 1, runMs: 1, totalMs: 2 } };
    expect(RunCodeResponseSchema.safeParse({ ...base, committed: true }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({ ...base, committed: false }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({ ...base, committed: "unknown" }).success).toBe(true);
    expect(RunCodeResponseSchema.safeParse({ ...base, committed: "maybe" }).success).toBe(false);
  });
});

describe("civil3d_run_code execution", () => {
  const okResponse = {
    runId: "20261011-120000-abcd1234",
    mode: "read",
    status: "ok",
    ok: true,
    result: 1,
    logFolder: "C:/log/run-code/20261011-120000-abcd1234",
    timing: { compileMs: 1, runMs: 1, totalMs: 2 },
  };
  const execute = CODE_DOMAIN_DEFINITION.actions.run.execute;
  let tokenDir: string;

  beforeEach(() => {
    tokenDir = mkdtempSync(join(tmpdir(), "c3d-rpc-token-"));
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", join(tokenDir, "rpc-token"));
    sendCommandMock.mockReset();
  });

  afterEach(() => {
    vi.unstubAllEnvs();
    rmSync(tokenDir, { recursive: true, force: true });
  });

  it("sends the session token, the start window and a timeout that covers the run", async () => {
    writeFileSync(join(tokenDir, "rpc-token"), "a1b2c3\r\n");
    sendCommandMock.mockResolvedValue(okResponse);

    await execute(RunCodeArgsSchema.parse({ action: "run", code: "return 1;", timeoutSeconds: 90 }));

    expect(RUN_CODE_START_WINDOW_SECONDS).toBe(30);
    expect(sendCommandMock).toHaveBeenCalledWith("runCode", {
      code: "return 1;",
      mode: "read",
      timeoutSeconds: 90,
      startWithinSeconds: 30,
      maxInlineBytes: 65536,
      args: {},
      authToken: "a1b2c3",
    }, { timeoutMs: Math.max(COMMAND_TIMEOUT_MS, 150_000) });
    expect(sendCommandMock.mock.calls[0][2].timeoutMs).toBeGreaterThanOrEqual(150_000);
  });

  it("keeps the command timeout for short runs and omits a missing token", async () => {
    sendCommandMock.mockResolvedValue(okResponse);

    await execute(RunCodeArgsSchema.parse({ action: "run", code: "return 1;" }));

    const [, params, options] = sendCommandMock.mock.calls[0];
    expect(params.timeoutSeconds).toBe(30);
    expect(params.startWithinSeconds).toBe(30);
    expect(params).not.toHaveProperty("authToken");
    expect(options).toEqual({ timeoutMs: Math.max(COMMAND_TIMEOUT_MS, 90_000) });
  });

  it("approves the caller's arguments, not the token added at execution", async () => {
    writeFileSync(join(tokenDir, "rpc-token"), "session-token");
    sendCommandMock.mockImplementation(async (method: string) => {
      if (method === "getCivil3DHealth") return { drawingLoaded: true };
      if (method === "getDrawingInfo") return { name: "Job.dwg" };
      if (method === "runCode") return okResponse;
      throw new Error(`unexpected ${method}`);
    });
    captureDomainToolHandlers(CODE_DOMAIN_DEFINITION);
    const rawArgs = { code: "return 1;", mode: "read", args: { n: 1 } };
    const receipt = await approvalPolicy.requestApproval(runTarget, rawArgs);

    const result = await getToolHandler("civil3d_run_code")!({ ...rawArgs, approvalToken: receipt.approvalToken });

    expect(result.isError).toBeUndefined();
    const runCall = sendCommandMock.mock.calls.find(([method]) => method === "runCode")!;
    expect(runCall[1].authToken).toBe("session-token");
    expect(result.content[0].text).not.toContain("session-token");
  });

  it.each([
    ["CIVIL3D.AUTH_REQUIRED", -32043, "runCode needs this Civil 3D session's token (%LOCALAPPDATA%\\Civil3DMcpPlugin\\rpc-token); update the civil3d-mcp server to match the plugin."],
    ["CIVIL3D.HOST_BUSY", -32029, "The script waited 31 s for other Civil 3D work and was not started; nothing changed. Retry when Civil 3D is idle."],
  ])("passes the plugin's %s error through unchanged", async (code, rpcCode, message) => {
    sendCommandMock.mockRejectedValue(new Civil3DRpcError(message, code, rpcCode));

    const error = await execute(RunCodeArgsSchema.parse({ action: "run", code: "return 1;" })).catch((e) => e);

    expect(error).toBeInstanceOf(Civil3DRpcError);
    expect(error.code).toBe(code);
    expect(error.rpcCode).toBe(rpcCode);
    expect(error.message).toBe(message);
  });

  it("tells the agent where the run's record is when the call times out", async () => {
    sendCommandMock.mockRejectedValue(new Civil3DRpcError("Command timed out after 150000ms: runCode", "CIVIL3D.TIMEOUT", -32008));

    const error = await execute(RunCodeArgsSchema.parse({ action: "run", code: "return 1;", timeoutSeconds: 90 })).catch((e) => e);

    expect(error.code).toBe("CIVIL3D.TIMEOUT");
    expect(error.message).toContain("Command timed out after 150000ms: runCode");
    expect(error.message).toContain("%LOCALAPPDATA%\\Civil3DMcpPlugin\\run-code\\");
    expect(error.message).toContain("run.json");
  });
});
