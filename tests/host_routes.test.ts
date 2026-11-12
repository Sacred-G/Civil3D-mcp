import { readdirSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";

const pluginDirectory = fileURLToPath(new URL("../Civil3D-MCP-Plugin/", import.meta.url));
const source = (name: string) => readFileSync(`${pluginDirectory}/${name}`, "utf8");
const withoutComments = (code: string) => code.replace(/\/\/[^\n]*/g, "");
const pluginFiles = readdirSync(pluginDirectory).filter((name) => name.endsWith(".cs"));

// Body of the lambda passed to the call that starts at `start`.
function lambdaBody(code: string, start: number): string {
  const arrow = code.indexOf("=>", start);
  let k = arrow + 2;
  while (/\s/.test(code[k])) k++;
  if (code[k] !== "{") return code.slice(k, code.indexOf(";", k));
  let depth = 0;
  for (let j = k; j < code.length; j++) {
    if (code[j] === "{") depth++;
    if (code[j] === "}" && --depth === 0) return code.slice(k, j);
  }
  return code.slice(k);
}

// Civil 3D 2027 stops running ExecuteInCommandContextAsync callbacks for the
// rest of the session once any COM client has run SendCommand (verified live;
// see CivilExecution.RunOnHostAsync). Host work therefore runs from an
// Application.Idle tick (reads) or inside the plugin's C3DMCPHOP command.
describe("host routes", () => {
  const execution = withoutComments(source("CivilExecution.cs"));

  it("never uses the command-context hop", () => {
    for (const name of pluginFiles) {
      expect(withoutComments(source(name)), name).not.toMatch(/DocumentManager\s*\.\s*ExecuteInCommandContextAsync/);
    }
  });

  it("reads from Application.Idle with an OpenCloseTransaction, which keeps the undo history", () => {
    expect(execution).toMatch(/route == HostRoute\.Idle\s*\?\s*database\.TransactionManager\.StartOpenCloseTransaction\(\)/);
    expect(execution).toContain("needsCommand: write || inCommand");
    expect(execution).toMatch(/if \(needsCommand\)\s*\{\s*await StartPluginCommandAsync\(/);
    // The route starts from an Idle tick: RunOnHostAsync subscribes its
    // handler, and the handler unsubscribes itself before it runs the work.
    const run = execution.slice(execution.indexOf("private static async Task RunOnHostAsync"));
    expect(run).toContain("CoreApp.Idle += idle;");
    expect(run.indexOf("CoreApp.Idle += idle;")).toBeLessThan(run.indexOf("await AwaitHostContextAsync(done.Task, cancellationToken)"));
    expect(run).toMatch(/CoreApp\.Idle -= idle;\s*Interlocked\.Exchange\(ref started, 1\);/);
  });

  it("ends the request with the operation's exception even after the operation claimed it", () => {
    const run = execution.slice(execution.indexOf("private static async Task RunOnHostAsync"));
    expect(run).toMatch(/catch \(Exception ex\)\s*\{\s*CoreApp\.Idle -= idle;\s*Interlocked\.Exchange\(ref claimed, 1\);\s*done\.TrySetException\(ex\);/);
  });

  it("waits for undo and redo to finish and reports a COM call still rejected as HOST_BUSY", () => {
    expect(execution).toContain("QueueCommandLine(input, waitForCompletion: true)");
    expect(execution).toContain("Task.Delay(wait ? HostBusyTimeout : TimeSpan.FromSeconds(3))");
    const retry = execution.slice(execution.indexOf("private static void SendCommandWithRetry"));
    expect(retry).toMatch(/catch \(COMException ex\) when \(IsCallRejected\(ex\)\)\s*\{\s*if \(DateTime\.UtcNow >= deadline\)\s*\{\s*throw new JsonRpcDispatchException\(\s*"CIVIL3D\.HOST_BUSY"/);
  });

  it("uses the plugin command for cost quantities only when earthwork creates a scratch volume surface", () => {
    const cost = withoutComments(source("CostEstimationCommands.cs"));
    expect(cost).toMatch(/computesEarthwork\s*\?\s*CivilExecution\.ReadInCommandAsync\(action\)\s*:\s*CivilExecution\.ReadAsync\(action\)/);
    expect(cost.match(/return ReadQuantitiesAsync\(computesEarthwork,/g)?.length).toBe(2);
    expect(cost).toContain("var computesEarthwork = includeEarthwork && !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface);");
    expect(cost).toContain("if (includeEarthwork && !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface))");
    expect(cost).toContain("var computesEarthwork = !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface);");
    expect(cost).toContain("if (!string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface))");
  });

  it("runs writes inside the plugin's own command", () => {
    expect(execution).toContain('Civil3DCompatibility.InvokeComMethod(acadDocument, "SendCommand"');
    const entry = source("PluginEntry.cs");
    expect(entry).toContain("[CommandMethod(CivilExecution.PluginCommandName");
    // async void and awaited, so Editor.CommandAsync can run inside the command.
    expect(entry).toMatch(/public static async void HostHopCommand\(\)[\s\S]*await CivilExecution\.RunPluginCommandWorkAsync\(\)/);
  });

  it("fails fast while Civil 3D is busy instead of waiting for the request timeout", () => {
    expect(execution).toContain("HostBusyTimeout = TimeSpan.FromSeconds(15)");
    expect(execution).toContain('"CIVIL3D.HOST_BUSY"');
    expect(execution).toContain("Editor.IsQuiescent == false");
    expect(execution).toContain("Interlocked.Exchange(ref claimed, 1)");
  });

  // An OpenCloseTransaction is not rolled back on abort, and Idle ticks run
  // outside any command, so a plain read must not create scratch objects, use
  // the pickfirst selection or run commands. Those use ReadInCommandAsync or
  // ReadThenCommandLineAsync, and command-line input goes through
  // QueueCommandLine (sent over COM after the operation), never
  // SendStringToExecute, whose input mixes with C3DMCPHOP's.
  it("keeps scratch objects, pickfirst and command-line input out of plain reads", () => {
    const scratch = ["GetVolumeProperties(", "ComputeVolumeBetweenSurfaces(", "GetEarthworkVolumes(", "TinVolumeSurface.Create("];
    const pickfirst = ["SetImpliedSelection(", "SelectImplied(", "BuildSelectedCivilObjects("];
    const commandLine = ["QueueCommandLine(", "SendCommand("];
    const writes = ["UpgradeOpen(", "OpenMode.ForWrite", ".Commit("];
    const offenders: string[] = [];
    const counts: Record<string, number> = {};
    for (const name of pluginFiles.filter((file) => file !== "CivilExecution.cs")) {
      const code = withoutComments(source(name));
      for (const match of code.matchAll(/CivilExecution\.(ReadAsync|ReadInCommandAsync|ReadThenCommandLineAsync)</g)) {
        const kind = match[1];
        counts[kind] = (counts[kind] ?? 0) + 1;
        const body = lambdaBody(code, match.index!);
        const has = (markers: string[]) => markers.filter((marker) => body.includes(marker));
        const bad =
          kind === "ReadAsync" ? [...has(scratch), ...has(pickfirst), ...has(commandLine), ...has(writes)]
          : kind === "ReadThenCommandLineAsync" ? [...has(scratch), ...has(pickfirst), ...has(writes)]
          : has(writes);
        if (bad.length > 0) offenders.push(`${name} ${kind}: ${bad.join(", ")}`);
      }
      const sendString = code.match(/SendStringToExecute\(/g)?.length ?? 0;
      if (sendString > 0) offenders.push(`${name}: SendStringToExecute`);
    }
    expect(offenders).toEqual([]);
    expect(counts.ReadInCommandAsync).toBeGreaterThan(0);
  });
});
