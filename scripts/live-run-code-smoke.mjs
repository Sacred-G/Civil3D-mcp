// Live smoke test of civil3d_run_code through the real MCP server (build/index.js)
// and the plugin in a running Civil 3D. Run it against a SCRATCH drawing: the
// write cases add and remove a text object, and one undo is sent.
//
//   npm run build
//   node scripts/live-run-code-smoke.mjs --expect-drawing <name> [--side <drawing.dwg>]
//
// --expect-drawing: the write cases run only when the active drawing's name contains
//   this text (guards against testing on a real job by accident); without it, or
//   --allow-writes, only the read cases run.
// --side: a DWG read as a side database (never opened, never changed) for the
//   OpenSide and bulk-sampling cases; skipped when not given.
import { existsSync, readFileSync } from "node:fs";
import net from "node:net";
import { parseArgs } from "node:util";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";
import { pluginTokenPath, readPluginToken } from "../build/utils/pluginAuth.js";

// Strict: an unknown option or a missing value is an error, never a silently disabled guard.
const { values: options } = parseArgs({
  options: {
    side: { type: "string" },
    "expect-drawing": { type: "string" },
    "allow-writes": { type: "boolean", default: false },
  },
  strict: true,
});
const sidePath = options.side;
const expectDrawing = options["expect-drawing"];

const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["./build/index.js"],
  cwd: process.cwd(),
  env: { ...process.env, MCP_HTTP_HOST: "127.0.0.1", MCP_HTTP_PORT: "0" },
  stderr: "pipe",
});
const client = new Client({ name: "civil3d-run-code-smoke", version: "1.0.0" });
const results = [];
let failures = 0;

function record(name, passed, detail) {
  results.push({ name, passed, ...(detail === undefined ? {} : { detail }) });
  if (!passed) failures++;
  console.error(`${passed ? "ok  " : "FAIL"} ${name}${detail === undefined ? "" : ` :: ${JSON.stringify(detail).slice(0, 400)}`}`);
}

async function call(name, args, timeout = 180_000) {
  return await client.callTool({ name, arguments: args }, undefined, { timeout, maxTotalTimeout: timeout });
}

function text(result) {
  return (result.content ?? []).map((item) => item.text ?? "").join("\n");
}

async function approve(toolName, action, parameters) {
  const approval = await call("civil3d_request_approval", { toolName, action, parameters });
  if (approval.isError) throw new Error(`approval failed: ${text(approval)}`);
  return approval.structuredContent.approvalToken;
}

// One approved run; returns the plugin's response object (structuredContent.result).
async function run(parameters) {
  const approvalToken = await approve("civil3d_run_code", "run", parameters);
  const response = await call("civil3d_run_code", { ...parameters, approvalToken });
  if (response.isError) throw new Error(`civil3d_run_code failed: ${text(response)}`);
  return response.structuredContent.result;
}

// One JSON-RPC request straight to the plugin's port, bypassing the MCP server
// (and its approval): for the token and start-window cases. The socket is not
// half-closed before the answer, because the plugin cancels a request whose
// client closes.
function rawRpc(method, params, timeoutMs = 120_000) {
  return new Promise((resolve, reject) => {
    const socket = net.createConnection({ host: "127.0.0.1", port: Number(process.env.CIVIL3D_PORT ?? 8080) });
    let data = "";
    let settled = false;
    const finish = (fn, value) => {
      if (settled) return;
      settled = true;
      clearTimeout(timer);
      socket.destroy();
      fn(value);
    };
    const timer = setTimeout(() => finish(reject, new Error(`raw ${method} timed out`)), timeoutMs);
    socket.setEncoding("utf8");
    socket.on("connect", () => socket.write(JSON.stringify({ jsonrpc: "2.0", id: `smoke-${process.pid}-${method}`, method, params })));
    socket.on("data", (chunk) => {
      data += chunk;
      try { finish(resolve, JSON.parse(data)); } catch { /* not complete yet */ }
    });
    socket.on("error", (error) => finish(reject, error));
    socket.on("close", () => finish(reject, new Error(`raw ${method}: connection closed without a complete answer (${data.length} chars)`)));
  });
}

async function check(name, fn) {
  try {
    await fn();
  } catch (error) {
    record(name, false, error instanceof Error ? error.message : String(error));
  }
}

try {
  await client.connect(transport);
  const tools = await client.listTools();
  const tool = tools.tools.find((item) => item.name === "civil3d_run_code");
  record("the server lists civil3d_run_code as destructive", Boolean(tool && tool.annotations?.destructiveHint === true));

  const info = await run({ code: "return new { name = Doc.Name, dbmod = Convert.ToInt32(Application.GetSystemVariable(\"DBMOD\")) };" });
  const drawingName = info.result?.name ?? "";
  record("read: basic run returns the active drawing", info.status === "ok" && drawingName.length > 0, { drawing: drawingName, timing: info.timing });
  if (expectDrawing && !drawingName.toLowerCase().includes(expectDrawing.toLowerCase())) {
    throw new Error(`Active drawing '${drawingName}' does not contain '${expectDrawing}'; stopping before any write.`);
  }

  await check("approval: refused without a token", async () => {
    const response = await call("civil3d_run_code", { code: "return 1;" });
    record("approval: refused without a token", response.isError === true && /approval required/i.test(text(response)), text(response).slice(0, 160));
  });

  await check("approval: a token for other code is refused", async () => {
    const token = await approve("civil3d_run_code", "run", { code: "return 1;" });
    const response = await call("civil3d_run_code", { code: "return 2;", approvalToken: token });
    record("approval: a token for other code is refused", response.isError === true && /does not match/i.test(text(response)), text(response).slice(0, 160));
  });

  await check("read: counts model space and logs", async () => {
    const response = await run({ code: "var n = ModelSpace<Entity>().Count();\nLog($\"{n} entities\");\nreturn n;" });
    record("read: counts model space and logs", response.status === "ok" && typeof response.result === "number" && /entities/.test(response.output ?? ""), { result: response.result, timing: response.timing });
  });

  await check("read: adds no change to the drawing (DBMOD unchanged)", async () => {
    const before = await run({ code: "return Convert.ToInt32(Application.GetSystemVariable(\"DBMOD\"));" });
    await run({ code: "foreach (var e in ModelSpace<Entity>()) { var x = e.Layer; }\nreturn 0;" });
    const after = await run({ code: "return Convert.ToInt32(Application.GetSystemVariable(\"DBMOD\"));" });
    record("read: adds no change to the drawing (DBMOD unchanged)", before.result === after.result, { before: before.result, after: after.result });
  });

  await check("compile error with its line", async () => {
    const response = await run({ code: "var a = 1;\nvar b = a + ;\nreturn b;" });
    record("compile error with its line", response.status === "compile_error" && response.errors?.[0]?.line === 2, response.errors);
  });

  await check("read mode refuses OpenMode.ForWrite", async () => {
    const response = await run({ code: "foreach (var e in ModelSpace<Entity>()) Tr.GetObject(e.ObjectId, OpenMode.ForWrite);\nreturn 0;" });
    record("read mode refuses OpenMode.ForWrite", response.status === "rejected" && response.errors?.[0]?.code === "C3DMCP001", response.errors);
  });

  await check("read mode refuses Db.Ltscale++", async () => {
    const response = await run({ code: "Db.Ltscale++;\nreturn 0;" });
    record("read mode refuses Db.Ltscale++", response.status === "rejected" && response.errors?.[0]?.code === "C3DMCP001", response.errors);
  });

  await check("using on Tr does nothing: Tr still works after it", async () => {
    const response = await run({ code: "{ using var tx = Tr; }\nreturn Open<DBObject>(Db.BlockTableId).Handle.ToString();" });
    record("using on Tr does nothing: Tr still works after it", response.status === "ok" && typeof response.result === "string", { status: response.status, result: response.result, error: response.error });
  });

  await check("a Tr kept in Session throws in a later run (no crash)", async () => {
    await run({ code: "Session[\"smoke-tr\"] = Tr;\nreturn 1;" });
    const later = await run({ code: "return ((Transaction)Session[\"smoke-tr\"]).NumberOfOpenedObjects;" });
    const after = await run({ code: "Session.TryRemove(\"smoke-tr\", out _);\nreturn 7;" });
    record("a Tr kept in Session throws in a later run (no crash)", later.status === "runtime_error" && after.status === "ok" && after.result === 7,
      { later: later.status, error: later.error?.message, after: after.status });
  });

  await check("read: a nested open-close transaction in a using works", async () => {
    const response = await run({ code: "using var t = Db.TransactionManager.StartOpenCloseTransaction();\nvar bt = (BlockTable)t.GetObject(Db.BlockTableId, OpenMode.ForRead);\nreturn bt.Has(BlockTableRecord.ModelSpace);" });
    record("read: a nested open-close transaction in a using works", response.status === "ok" && response.result === true, { status: response.status, error: response.error });
  });

  await check("the plugin wrote this session's rpc-token", async () => {
    const path = pluginTokenPath();
    const token = existsSync(path) ? readFileSync(path, "utf8").trim() : "";
    record("the plugin wrote this session's rpc-token", /^[0-9A-F]{64}$/i.test(token), { path, length: token.length });
  });

  await check("runCode without the token is refused (raw TCP)", async () => {
    const missing = await rawRpc("runCode", { code: "return 1;" });
    const wrong = await rawRpc("runCode", { code: "return 1;", authToken: "0".repeat(64) });
    record("runCode without the token is refused (raw TCP)",
      missing.error?.data?.code === "CIVIL3D.AUTH_REQUIRED" && wrong.error?.data?.code === "CIVIL3D.AUTH_REQUIRED",
      { missing: missing.error?.data?.code ?? missing.result, wrong: wrong.error?.data?.code ?? wrong.result });
  });

  await check("start window: a run that cannot start in time fails HOST_BUSY", async () => {
    const long = run({ code: "while (Elapsed.TotalSeconds < 12) Check();\nreturn 1;", timeoutSeconds: 30 });
    await new Promise((resolve) => setTimeout(resolve, 3000));
    const busy = await rawRpc("runCode", { code: "return 1;", mode: "read", timeoutSeconds: 10, startWithinSeconds: 3, authToken: readPluginToken() });
    const first = await long;
    record("start window: a run that cannot start in time fails HOST_BUSY",
      busy.error?.data?.code === "CIVIL3D.HOST_BUSY" && first.status === "ok",
      { busy: busy.error ? `${busy.error.data?.code}: ${busy.error.message}` : busy.result, first: first.status });
  });

  await check("runtime error with its line", async () => {
    const response = await run({ code: "var a = 1;\nthrow new InvalidOperationException(\"boom\");" });
    record("runtime error with its line", response.status === "runtime_error" && response.error?.line === 2 && response.error?.message === "boom", response.error);
  });

  await check("timeout through Check()", async () => {
    const response = await run({ code: "while (true) Check();", timeoutSeconds: 3 });
    const runMs = response.timing?.runMs ?? 0;
    record("timeout through Check()", response.status === "timeout" && runMs >= 2900 && runMs < 10_000, { status: response.status, timing: response.timing });
  });

  await check("Civil 3D still answers after the timeout", async () => {
    const response = await run({ code: "return 7;" });
    record("Civil 3D still answers after the timeout", response.status === "ok" && response.result === 7);
  });

  await check("large result goes to a file", async () => {
    const response = await run({ code: "return Enumerable.Range(0, 200000).Select(i => (i * 0.5, i * 0.25)).ToList();" });
    record("large result goes to a file", response.status === "ok" && Boolean(response.resultFile) && response.resultBytes > 65_536 && response.result === null,
      { resultFile: response.resultFile, resultBytes: response.resultBytes, resultShape: response.resultShape });
  });

  await check("WriteFile output is listed", async () => {
    const response = await run({ code: "return WriteFile(\"points.txt\", new[] { \"1 2\", \"3 4\" });" });
    record("WriteFile output is listed", response.status === "ok" && (response.files ?? []).some((file) => file.endsWith("points.txt")), response.files);
  });

  await check("AutoCAD values become compact JSON", async () => {
    const response = await run({ code: "return new { p = new Point3d(1, 2, 3), id = Db.BlockTableId, ext = new Extents3d(new Point3d(0,0,0), new Point3d(1,1,0)), t = (1, \"a\") };" });
    const r = response.result ?? {};
    record("AutoCAD values become compact JSON", response.status === "ok" && Array.isArray(r.p) && r.p[2] === 3 && typeof r.id === "string" && Array.isArray(r.ext?.min) && Array.isArray(r.t), r);
  });

  await check("dictionaries keyed by ObjectId use handles", async () => {
    const response = await run({ code: "return ModelSpace<Entity>().Take(2).ToDictionary(e => e.ObjectId, e => e.Layer);" });
    const keys = Object.keys(response.result ?? {});
    record("dictionaries keyed by ObjectId use handles", response.status === "ok" && !response.resultError && keys.length > 0 && keys.every((key) => /^[0-9A-F]+$/i.test(key)), { result: response.result, resultError: response.resultError });
  });

  await check("Session keeps data across runs", async () => {
    await run({ code: "Session[\"smoke\"] = 42;\nreturn null;" });
    const response = await run({ code: "return Session[\"smoke\"];" });
    record("Session keeps data across runs", response.status === "ok" && response.result === 42, response.result);
  });

  if (sidePath) {
    await check("side database: bulk curve sampling", async () => {
      const code = [
        "var side = OpenSide((string)Args[\"dwg\"]);",
        "long objects = 0, curves = 0, points = 0;",
        "foreach (var e in ModelSpace<Entity>(side))",
        "{",
        "  objects++;",
        "  if (e is not Curve c) continue;",
        "  double length; try { length = c.GetDistanceAtParameter(c.EndParam); } catch (Exception) { continue; }",
        "  curves++;",
        "  for (double s = 0; s <= length; s += 0.8) { try { c.GetPointAtDist(s); points++; } catch (Exception) { } }",
        "}",
        "return new { objects, curves, points, seconds = Elapsed.TotalSeconds };",
      ].join("\n");
      const response = await run({ code, args: { dwg: sidePath }, timeoutSeconds: 90 });
      record("side database: bulk curve sampling", response.status === "ok" && response.result?.objects > 0, { result: response.result, timing: response.timing, error: response.error });
    });
    await check("the working database is restored after a failing run", async () => {
      const failing = await run({
        code: "var side = OpenSide((string)Args[\"dwg\"]);\nHostApplicationServices.WorkingDatabase = side;\nthrow new InvalidOperationException(\"fail with the side database as working database\");",
        args: { dwg: sidePath },
      });
      const after = await run({ code: "return HostApplicationServices.WorkingDatabase == Db;" });
      record("the working database is restored after a failing run", failing.status === "runtime_error" && after.status === "ok" && after.result === true, { failing: failing.status, restored: after.result });
    });
    await check("a side database kept in Session throws in a later run (no crash)", async () => {
      await run({ code: "var side = OpenSide((string)Args[\"dwg\"]);\nSession[\"smoke-side\"] = side;\nreturn ModelSpace<Entity>(side).Take(10).Count();", args: { dwg: sidePath } });
      const later = await run({ code: "return TransactionFor((Database)Session[\"smoke-side\"]).NumberOfOpenedObjects;" });
      const after = await run({ code: "Session.TryRemove(\"smoke-side\", out _);\nreturn 7;" });
      record("a side database kept in Session throws in a later run (no crash)", later.status === "runtime_error" && after.status === "ok" && after.result === 7,
        { later: later.status, error: later.error?.message, after: after.status });
    });
  } else {
    record("side database: skipped (no --side)", true);
  }

  // Writes: add a tagged text, check it, undo once, check it is gone.
  const tag = `RUNCODE-SMOKE-${process.pid}`;
  const countTagged = `return ModelSpace<DBText>().Count(t => t.TextString == (string)Args["tag"]);`;
  const writesAllowed = Boolean(expectDrawing) || options["allow-writes"];
  if (!writesAllowed) {
    record("write cases: skipped (pass --expect-drawing <scratch drawing name>, or --allow-writes)", true);
  }

  if (writesAllowed) {
  await check("write: adds an object, one undo removes it", async () => {
    const add = await run({
      mode: "write",
      args: { tag },
      code: [
        "var ms = Open<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(Db), forWrite: true);",
        "var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "ms.AppendEntity(text);",
        "Tr.AddNewlyCreatedDBObject(text, true);",
        "return text.Handle.ToString();",
      ].join("\n"),
    });
    const afterAdd = await run({ code: countTagged, args: { tag } });
    // Undo only what this case committed: an undo after a rolled-back add would revert the
    // drawing's previous undo group instead.
    let undo = { isError: true, content: [{ text: "not sent: the add did not commit" }] };
    let afterUndo = { result: afterAdd.result };
    if (add.status === "ok" && add.committed === true && afterAdd.result === 1) {
      const undoToken = await approve("civil3d_drawing", "undo", { action: "undo", steps: 1 });
      undo = await call("civil3d_drawing", { action: "undo", steps: 1, approvalToken: undoToken });
      await new Promise((resolve) => setTimeout(resolve, 1500));
      afterUndo = await run({ code: countTagged, args: { tag } });
    }
    record("write: adds an object, one undo removes it",
      add.status === "ok" && add.committed === true && afterAdd.result === 1 && !undo.isError && afterUndo.result === 0,
      { add: add.status, committed: add.committed, afterAdd: afterAdd.result, undo: undo.isError ? text(undo) : "ok", afterUndo: afterUndo.result });
  });

  await check("write: an exception rolls the change back", async () => {
    const failed = await run({
      mode: "write",
      args: { tag: `${tag}-ROLLBACK` },
      code: [
        "var ms = Open<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(Db), forWrite: true);",
        "var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "ms.AppendEntity(text);",
        "Tr.AddNewlyCreatedDBObject(text, true);",
        "throw new InvalidOperationException(\"roll back\");",
      ].join("\n"),
    });
    const after = await run({ code: countTagged, args: { tag: `${tag}-ROLLBACK` } });
    record("write: an exception rolls the change back", failed.status === "runtime_error" && after.result === 0, { failed: failed.status, after: after.result });
  });

  await check("write: a timeout the script catches is still rolled back", async () => {
    const swallowed = await run({
      mode: "write",
      timeoutSeconds: 2,
      args: { tag: `${tag}-SWALLOWED` },
      code: [
        "var ms = Open<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(Db), forWrite: true);",
        "var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "ms.AppendEntity(text);",
        "Tr.AddNewlyCreatedDBObject(text, true);",
        "try { while (true) Check(); } catch (Exception) { }",
        "return \"caught it\";",
      ].join("\n"),
    });
    const after = await run({ code: countTagged, args: { tag: `${tag}-SWALLOWED` } });
    record("write: a timeout the script catches is still rolled back", swallowed.status === "timeout" && after.result === 0, { status: swallowed.status, after: after.result, warnings: swallowed.warnings });
  });

  await check("write: Commit through an alias of Tr is refused and rolled back", async () => {
    const aliased = await run({
      mode: "write",
      args: { tag: `${tag}-ALIAS` },
      code: [
        "var tx = Tr;",
        "var ms = Open<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(Db), forWrite: true);",
        "var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "ms.AppendEntity(text);",
        "tx.AddNewlyCreatedDBObject(text, true);",
        "tx.Commit();",
        "return 1;",
      ].join("\n"),
    });
    const after = await run({ code: countTagged, args: { tag: `${tag}-ALIAS` } });
    record("write: Commit through an alias of Tr is refused and rolled back",
      aliased.status === "runtime_error" && aliased.committed === false && /owns Tr/.test(aliased.error?.message ?? "") && after.result === 0,
      { status: aliased.status, committed: aliased.committed, error: aliased.error?.message, after: after.result });
  });

  await check("write: a nested transaction in a using commits with the run", async () => {
    const nested = await run({
      mode: "write",
      args: { tag: `${tag}-NESTED` },
      code: [
        "using (var t = Db.TransactionManager.StartTransaction())",
        "{",
        "  var ms = (BlockTableRecord)t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Db), OpenMode.ForWrite);",
        "  var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "  ms.AppendEntity(text);",
        "  t.AddNewlyCreatedDBObject(text, true);",
        "  t.Commit();",
        "}",
        "throw new InvalidOperationException(\"roll back the outer run\");",
      ].join("\n"),
    });
    const after = await run({ code: countTagged, args: { tag: `${tag}-NESTED` } });
    // The nested commit lands in the run's transaction, so the run's rollback removes it too.
    // Only the script's own last line may fail: an earlier error would also leave nothing behind.
    record("write: a nested transaction in a using commits with the run",
      nested.status === "runtime_error" && nested.error?.message === "roll back the outer run" && nested.committed === false && after.result === 0,
      { status: nested.status, error: nested.error?.message, committed: nested.committed, after: after.result, warnings: nested.warnings });
  });

  await check("write: using var tx = Tr commits as usual, one undo removes it", async () => {
    const add = await run({
      mode: "write",
      args: { tag: `${tag}-USING` },
      code: [
        "using var tx = Tr;",
        "var ms = (BlockTableRecord)tx.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Db), OpenMode.ForWrite);",
        "var text = new DBText { TextString = (string)Args[\"tag\"], Position = new Point3d(-1e6, -1e6, 0), Height = 1 };",
        "ms.AppendEntity(text);",
        "tx.AddNewlyCreatedDBObject(text, true);",
        "return text.Handle.ToString();",
      ].join("\n"),
    });
    const afterAdd = await run({ code: countTagged, args: { tag: `${tag}-USING` } });
    let undo = { isError: true, content: [{ text: "not sent: the add did not commit" }] };
    let afterUndo = { result: afterAdd.result };
    if (add.status === "ok" && add.committed === true && afterAdd.result === 1) {
      const undoToken = await approve("civil3d_drawing", "undo", { action: "undo", steps: 1 });
      undo = await call("civil3d_drawing", { action: "undo", steps: 1, approvalToken: undoToken });
      await new Promise((resolve) => setTimeout(resolve, 1500));
      afterUndo = await run({ code: countTagged, args: { tag: `${tag}-USING` } });
    }
    record("write: using var tx = Tr commits as usual, one undo removes it",
      add.status === "ok" && add.committed === true && afterAdd.result === 1 && !undo.isError && afterUndo.result === 0,
      { add: add.status, committed: add.committed, error: add.error?.message, afterAdd: afterAdd.result, undo: undo.isError ? text(undo) : "ok", afterUndo: afterUndo.result });
  });

  }

  console.log(JSON.stringify({ status: failures === 0 ? "passed" : "failed", failures, drawing: drawingName, results }, null, 2));
  process.exitCode = failures === 0 ? 0 : 1;
} catch (error) {
  console.error(`Live run-code smoke stopped: ${error instanceof Error ? error.message : String(error)}`);
  console.log(JSON.stringify({ status: "stopped", failures, results }, null, 2));
  process.exitCode = 1;
} finally {
  // Closing the stdio transport can leave a promise that never settles once
  // the server exits; Node would then end with code 13 (unsettled top-level
  // await) over the real result, so bound it and exit explicitly.
  await Promise.race([
    client.close().catch(() => undefined),
    new Promise((resolve) => setTimeout(resolve, 5000)),
  ]);
}
process.exit(process.exitCode ?? 0);
