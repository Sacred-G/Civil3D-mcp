import { readdirSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";
import { callArguments, enclosingBlocks, enclosingMethod, maskCSharp, maskCSharpParts, matchingClose, methodBody, squash } from "./csharp_source.js";

const pluginDirectory = fileURLToPath(new URL("../Civil3D-MCP-Plugin/", import.meta.url));
const source = (name: string) => readFileSync(`${pluginDirectory}/${name}`, "utf8");
// Code only: comments and literal contents blanked, so neither can satisfy or
// trip a check.
const code = (name: string) => maskCSharp(source(name));
const pluginFiles = readdirSync(pluginDirectory).filter((name) => name.endsWith(".cs"));

// Asserts that the patterns occur in `text` in this order.
function expectInOrder(text: string, patterns: RegExp[], label: string) {
  let from = 0;
  for (const pattern of patterns) {
    const flags = pattern.flags.includes("g") ? pattern.flags : `${pattern.flags}g`;
    const search = new RegExp(pattern.source, flags);
    search.lastIndex = from;
    const match = search.exec(text);
    expect(match, `${label}: ${pattern} after index ${from}`).not.toBeNull();
    from = match!.index + match![0].length;
  }
}

// The body of the block that the pattern's first match opens (the match ends
// just before or at its "{").
function blockAfter(text: string, pattern: RegExp, label: string): string {
  const match = pattern.exec(text);
  expect(match, `${label}: ${pattern}`).not.toBeNull();
  const open = text.indexOf("{", match!.index + match![0].length - 1);
  return text.slice(open + 1, matchingClose(text, open));
}

// Literal text (comments blanked) of the block found by following the
// patterns in order through a file's masked code: each pattern is searched
// after the previous match, and the last one opens the block. Checks a
// message where it is thrown, so a copy in a comment or elsewhere does not
// count.
function literalsInBlock(name: string, patterns: RegExp[], label: string): string {
  const parts = maskCSharpParts(source(name));
  let from = 0;
  let end = 0;
  for (const pattern of patterns) {
    const search = new RegExp(pattern.source, pattern.flags.includes("g") ? pattern.flags : `${pattern.flags}g`);
    search.lastIndex = from;
    const match = search.exec(parts.code);
    expect(match, `${label}: ${pattern} after index ${from}`).not.toBeNull();
    end = match!.index + match![0].length;
    from = end;
  }
  const open = parts.code.indexOf("{", end - 1);
  return squash(parts.withoutComments.slice(open + 1, matchingClose(parts.code, open)));
}

describe("C# source masking used by these guards", () => {
  it("blanks comments and literal contents but keeps code and interpolation holes", () => {
    const sample = [
      'var a = "{ QueueCommandLine( }"; // SendCommand(',
      "var b = '}';",
      'var c = $"x {QueueCommandLine(y)} {{ z";',
      'var d = @"a ""}"" b";',
      "/* { */ Run(() => { Foo(); });",
    ].join("\n");
    const masked = maskCSharp(sample);
    expect(masked).toHaveLength(sample.length);
    expect(masked.match(/QueueCommandLine\(/g)?.length).toBe(1);
    expect(masked).not.toContain("SendCommand(");
    const open = masked.indexOf("{", masked.indexOf("Run("));
    expect(masked.slice(open, matchingClose(masked, open) + 1)).toBe("{ Foo(); }");
  });

  it("keeps code after a verbatim string that holds a quote, and masks raw strings", () => {
    const sample = 'var q = @""""; Run(1);\nvar r = """ { QueueCommandLine( """; Run(2);';
    const masked = maskCSharp(sample);
    expect(masked.match(/Run\(\d\)/g)).toEqual(["Run(1)", "Run(2)"]);
    expect(masked).not.toContain("QueueCommandLine(");
    expect(maskCSharpParts(sample).withoutComments).toBe(sample);
  });

  it("finds the parameters of constructors with an initializer and of constrained generics", () => {
    const sample = [
      "class A : B {",
      "  public A(int first, string second) : base(first) { Mark(); }",
      "  T Get<T>(Func<int, T> make, int count) where T : class { Mark(); }",
      "}",
    ].join("\n");
    const masked = maskCSharp(sample);
    const marks = [...masked.matchAll(/Mark\(\)/g)].map((match) => enclosingMethod(masked, match.index!));
    expect(marks).toEqual([
      { name: "A", parameters: ["first", "second"] },
      { name: "Get", parameters: ["make", "count"] },
    ]);
  });
});

// Civil 3D 2027 stops running ExecuteInCommandContextAsync callbacks for the
// rest of the session once any COM client has run SendCommand (verified live;
// see CivilExecution.RunOnHostAsync). Host work therefore runs from an
// Application.Idle tick (reads) or inside the plugin's C3DMCPHOP command.
describe("host routes", () => {
  const execution = code("CivilExecution.cs");
  const runOnHost = methodBody(execution, /private static async Task RunOnHostAsync\s*\(/);
  const idleHandler = blockAfter(runOnHost, /idle\s*=\s*async\s*\(\s*_\s*,\s*_\s*\)\s*=>\s*\{/, "Idle handler");

  it("never uses the command-context hop", () => {
    for (const name of pluginFiles) {
      expect(code(name), name).not.toMatch(/DocumentManager\s*\.\s*ExecuteInCommandContextAsync/);
    }
  });

  it("reads from Application.Idle with an OpenCloseTransaction, which keeps the undo history", () => {
    expect(execution).toMatch(/route\s*==\s*HostRoute\.Idle\s*\?\s*database\.TransactionManager\.StartOpenCloseTransaction\(\)\s*:\s*database\.TransactionManager\.StartTransaction\(\)/);
    expect(execution).toMatch(/needsCommand:\s*write\s*\|\|\s*inCommand\b/);
    expect(idleHandler).toMatch(/if\s*\(\s*needsCommand\s*\)\s*\{\s*await\s+StartPluginCommandAsync\(\s*\(\)\s*=>\s*RunOnce\(\s*HostRoute\.PluginCommand\s*\)/);
    expect(idleHandler).toMatch(/else\s*\{\s*await\s+RunOnce\(\s*HostRoute\.Idle\s*\)/);
    // The route starts from an Idle tick: RunOnHostAsync subscribes its
    // handler before it waits, and the handler unsubscribes itself and takes
    // the start before it runs the work.
    expectInOrder(runOnHost, [/CoreApp\.Idle\s*\+=\s*idle\s*;/, /await\s+AwaitHostContextAsync\(\s*done\.Task\s*,\s*cancellationToken\s*\)/], "RunOnHostAsync");
    expectInOrder(idleHandler, [/CoreApp\.Idle\s*-=\s*idle\s*;\s*if\s*\(\s*!\s*hostRequest\.TryStart\(\)\s*\)\s*\{\s*return\s*;\s*\}/, /await\s+RunOnce\(/], "Idle handler");
  });

  it("ends the request with the operation's exception even after the operation claimed it", () => {
    const failed = blockAfter(idleHandler.slice(idleHandler.search(/hostRequest\.TryStart\(\)/)), /catch\s*\(\s*Exception\s+ex\s*\)\s*\{/, "Idle handler catch");
    expectInOrder(failed, [/hostRequest\.TryAbandonOperation\(\)\s*;/, /done\.TrySetException\(\s*ex\s*\)\s*;/], "Idle handler catch");
    expect(failed).not.toMatch(/\breturn\b/);
  });

  // The behaviour is exercised by tests/HostGateHarness (npm run
  // test:host-gate); these pin that CivilExecution uses it that way.
  it("keeps the next request out until a cancelled request's host work has finished", () => {
    const serialized = methodBody(execution, /private static async Task<T> ExecuteSerializedAsync<T>\s*\(/);
    expect(serialized).toMatch(/await\s+HostExecutionGate\.RunAsync\(/);
    expect(execution).toMatch(/private static readonly HostGate HostExecutionGate\s*=\s*new\(\)\s*;/);
    expect(execution).not.toMatch(/SemaphoreSlim|HostExecutionGate\.Release\(/);

    // RunOnHostAsync ends the request in a finally: what has not started
    // never will.
    const ending = blockAfter(runOnHost.slice(runOnHost.search(/await\s+AwaitHostContextAsync\(\s*done\.Task/)), /finally\s*\{/, "RunOnHostAsync finally");
    expect(ending).toMatch(/hostRequest\.End\(\)\s*;/);
    expect(ending).not.toMatch(/TrySetCanceled|HostFinished/);

    // The started Idle handler settles the request only after an action that
    // claimed it has finished.
    const settle = blockAfter(idleHandler.slice(idleHandler.search(/hostRequest\.TryStart\(\)/)), /finally\s*\{/, "Idle handler finally");
    expectInOrder(settle, [/if\s*\(\s*hostRequest\.OperationClaimed\s*\)/, /await\s+hostRequest\.OperationDone\s*;/, /hostRequest\.HostFinished\(\)\s*;/], "Idle handler finally");
    // Queued command-line input is sent, unless the request was cancelled,
    // before the request settles.
    expectInOrder(idleHandler, [
      /await\s+hostRequest\.OperationDone\s*;/,
      /if\s*\(\s*cancellationToken\.IsCancellationRequested\s*\)/,
      /else\s*\{\s*await\s+SendCommandLineAsync\(/,
      /hostRequest\.HostFinished\(\)\s*;\s*done\.TrySetResult\(\s*true\s*\)/,
    ], "Idle handler");

    // The busy watcher and the no-Idle watchdog only end a request that has
    // not started.
    const endBeforeStart = blockAfter(runOnHost, /void\s+EndBeforeStart\s*\(\s*Exception\s+reason\s*\)\s*\{/, "EndBeforeStart");
    expect(endBeforeStart).toMatch(/if\s*\(\s*hostRequest\.TryAbandonBeforeStart\(\)\s*\)\s*\{[^}]*done\.TrySetException\(\s*reason\s*\)/);
    expect(runOnHost.match(/done\.TrySetException\(/g)?.length).toBe(2);

    // C3DMCPHOP: work the command did not run is stopped before it is dropped.
    const start = methodBody(execution, /private static async Task StartPluginCommandAsync\s*\(/);
    expect(start.match(/hostRequest\.TryAbandonOperation\(\)\s*;\s*if\s*\(\s*!\s*hostRequest\.OperationClaimed\s*\)\s*\{\s*DropQueuedWork\(\s*work\s*\)/g)?.length).toBe(2);

    // newDrawing's application-context hop: a cancellation ends only a
    // callback that has not started, and the callback settles the request.
    const application = methodBody(execution, /private static Task ExecuteInApplicationContextAsync\s*\(/);
    expectInOrder(application, [/hostRequest\.TryStart\(\)/, /hostRequest\.TryClaimOperation\(\)/, /await\s+callback\(/, /hostRequest\.HostFinished\(\)/], "ExecuteInApplicationContextAsync");
    expect(application).toMatch(/cancellationToken\.Register\(\s*\(\)\s*=>\s*\{\s*if\s*\(\s*hostRequest\.TryAbandonBeforeStart\(\)\s*\)/);
  });

  it("waits for undo and redo to finish and reports a COM call still rejected as HOST_BUSY", () => {
    const run = methodBody(execution, /public static async Task<T> RunCommandLineAsync<T>\s*\(/);
    expect(run).toMatch(/QueueCommandLine\(\s*input\s*,\s*waitForCompletion:\s*true\s*\)/);
    const send = methodBody(execution, /private static async Task SendCommandLineAsync\s*\(/);
    expect(send).toMatch(/if\s*\(\s*wait\s*\)\s*\{\s*await\s+Task\.WhenAny\(\s*call\s*,\s*Task\.Delay\(\s*HostBusyTimeout\s*\)\s*\)/);
    const retry = methodBody(execution, /private static void SendCommandWithRetry\s*\(/);
    const rejected = blockAfter(retry, /catch\s*\(\s*COMException\s+ex\s*\)\s*when\s*\(\s*IsCallRejected\(\s*ex\s*\)\s*\)\s*\{/, "rejected call");
    const deadline = blockAfter(rejected, /if\s*\(\s*DateTime\.UtcNow\s*>=\s*deadline\s*\)\s*\{/, "retry deadline");
    expect(deadline).toMatch(/throw\s+new\s+JsonRpcDispatchException\(/);
    // The code and message are literals: read them inside that block.
    const deadlineLiterals = literalsInBlock("CivilExecution.cs", [
      /private static void SendCommandWithRetry\s*\(/,
      /catch\s*\(\s*COMException\s+ex\s*\)\s*when\s*\(\s*IsCallRejected\(\s*ex\s*\)\s*\)\s*\{/,
      /if\s*\(\s*DateTime\.UtcNow\s*>=\s*deadline\s*\)\s*\{/,
    ], "retry deadline");
    expect(deadlineLiterals).toContain('throw new JsonRpcDispatchException( "CIVIL3D.HOST_BUSY", $"Civil 3D kept rejecting');
    // A cancelled request, or one that stopped waiting, never retries.
    expectInOrder(rejected, [/progress\?\.Rejected\(\)/, /cancellationToken\.ThrowIfCancellationRequested\(\)/, /if\s*\(\s*progress\?\.IsDetached\s*==\s*true\s*\)\s*\{\s*throw\b/, /if\s*\(\s*DateTime\.UtcNow\s*>=\s*deadline\s*\)/, /Thread\.Sleep\(/], "rejected call");
    expect(send.match(/progress\.Detach\(\)\s*;/g)?.length).toBe(2);
    expect(send).toMatch(/SendCommandWithRetry\(\s*acadDocument\s*,\s*text\s*,\s*cancellationToken\s*,\s*progress\s*\)/);
  });

  // A send that never got through must fail the request, not be reported as
  // started: the request keeps waiting while Civil 3D still rejects the call,
  // and stops early only once Civil 3D has accepted it (a dialog is open).
  it("fails the request when queued command-line input cannot be sent", () => {
    const send = methodBody(execution, /private static async Task SendCommandLineAsync\s*\(/);
    expect(send).toMatch(/while\s*\(\s*!\s*call\.IsCompleted\s*&&\s*!\s*progress\.AcceptedFor\(\s*DialogGrace\s*\)\s*\)/);
    expect(send).toMatch(/catch\s*\(\s*Exception\s+ex\s*\)\s*when\s*\(\s*ex\s+is\s+not\s+JsonRpcDispatchException\s+and\s+not\s+OperationCanceledException\s*\)\s*\{\s*throw\s+new\s+JsonRpcDispatchException\(/);
    const retry = methodBody(execution, /private static void SendCommandWithRetry\s*\(/);
    expectInOrder(retry, [/progress\?\.Attempting\(\)/, /InvokeComMethod\(\s*acadDocument\s*,/, /progress\?\.Rejected\(\)/], "SendCommandWithRetry");

    // QueueCommandLine only appends to a list, so a try/catch around it is
    // dead code that suggests a failure would be reported there.
    const offenders: string[] = [];
    for (const name of pluginFiles.filter((file) => file !== "CivilExecution.cs")) {
      const masked = code(name);
      for (const match of masked.matchAll(/\bQueueCommandLine\s*\(/g)) {
        const insideTry = enclosingBlocks(masked, match.index!).some((block) => block.header === "try");
        if (insideTry) offenders.push(`${name}:${masked.slice(0, match.index!).split("\n").length}`);
      }
    }
    expect(offenders).toEqual([]);
  });

  it("uses the plugin command for cost quantities only when earthwork creates a scratch volume surface", () => {
    const cost = code("CostEstimationCommands.cs");
    const route = methodBody(cost, /private static Task<object\?> ReadQuantitiesAsync\s*\(/);
    expect(route).toMatch(/return\s+computesEarthwork\s*\?\s*CivilExecution\.ReadInCommandAsync\(\s*action\s*\)\s*:\s*CivilExecution\.ReadAsync\(\s*action\s*\)\s*;/);
    const calls = [...cost.matchAll(/\bReadQuantitiesAsync\s*\(\s*computesEarthwork\s*,/g)];
    expect(calls).toHaveLength(2);
    for (const call of calls) {
      // The flag is set from the same condition that guards the earthwork
      // branch inside the delegate.
      const before = cost.slice(0, call.index!);
      const flag = /var\s+computesEarthwork\s*=\s*([^;]+);[^;]*$/.exec(before.slice(before.lastIndexOf("var computesEarthwork")));
      expect(flag, "computesEarthwork assignment").not.toBeNull();
      const args = callArguments(cost, cost.indexOf("(", call.index!));
      const delegate = args[1].text;
      const branch = /if\s*\(([^{]+)\)\s*\{/.exec(delegate);
      expect(branch, "earthwork branch").not.toBeNull();
      const guard = squash(branch![1]);
      expect([squash(flag![1]), "computesEarthwork"]).toContain(guard);
    }
  });

  it("runs writes inside the plugin's own command", () => {
    const retryLiterals = literalsInBlock("CivilExecution.cs", [/private static void SendCommandWithRetry\s*\([^)]*\)\s*\{/], "SendCommandWithRetry");
    expect(retryLiterals).toContain('Civil3DCompatibility.InvokeComMethod(acadDocument, "SendCommand", text)');
    // The C3DMCPHOP send stops retrying when the request is cancelled.
    expect(methodBody(execution, /private static async Task StartPluginCommandAsync\s*\(/)).toMatch(/SendCommandWithRetry\(\s*acadDocument\s*,\s*"\s*"\s*\+\s*PluginCommandName\s*\+\s*"\s*"\s*,\s*cancellationToken\s*\)/);
    const entry = code("PluginEntry.cs");
    expect(entry).toMatch(/\[\s*CommandMethod\(\s*CivilExecution\.PluginCommandName\b/);
    // async void and awaited, so Editor.CommandAsync can run inside the command.
    const command = methodBody(entry, /public static async void HostHopCommand\s*\(\s*\)/);
    expect(command).toMatch(/await\s+CivilExecution\.RunPluginCommandWorkAsync\(\s*\)/);
  });

  it("fails fast while Civil 3D is busy instead of waiting for the request timeout", () => {
    expect(execution).toMatch(/HostBusyTimeout\s*=\s*TimeSpan\.FromSeconds\(\s*15\s*\)/);
    // Busy watcher: a command, or for C3DMCPHOP work a non-quiescent editor,
    // for HostBusyTimeout ends the request with HOST_BUSY.
    expect(idleHandler).toMatch(/Editor\.IsQuiescent\s*==\s*false/);
    const busy = blockAfter(idleHandler, /if\s*\(\s*busy\s*\)\s*\{/, "busy branch");
    const busyTimeout = blockAfter(busy, /if\s*\(\s*now\s*-\s*busySince\.Value\s*>=\s*HostBusyTimeout\s*\)\s*\{/, "busy timeout");
    expect(busyTimeout).toMatch(/EndBeforeStart\(\s*new\s+JsonRpcDispatchException\(/);
    expect(busy).toMatch(/\breturn\s*;\s*$/);
    // No-Idle watchdog (a modal dialog), on the nudge timer.
    const nudge = blockAfter(runOnHost, /new\s+Timer\(\s*_\s*=>\s*\{/, "nudge timer");
    const watchdog = blockAfter(nudge, /if\s*\(\s*sinceIdle\s*>=\s*HostBusyTimeout\s*\)\s*\{/, "watchdog");
    expect(watchdog).toMatch(/EndBeforeStart\(\s*new\s+JsonRpcDispatchException\(/);
    // The messages, where they are raised.
    const run = /private static async Task RunOnHostAsync\s*\(/;
    expect(literalsInBlock("CivilExecution.cs", [run, /idle\s*=\s*async/, /if\s*\(\s*busy\s*\)/, /if\s*\(\s*now\s*-\s*busySince\.Value\s*>=\s*HostBusyTimeout\s*\)\s*\{/], "busy timeout"))
      .toContain('EndBeforeStart(new JsonRpcDispatchException( "CIVIL3D.HOST_BUSY", "Civil 3D has a command or prompt in progress.');
    expect(literalsInBlock("CivilExecution.cs", [run, /new\s+Timer\(/, /if\s*\(\s*sinceIdle\s*>=\s*HostBusyTimeout\s*\)\s*\{/], "watchdog"))
      .toContain('EndBeforeStart(new JsonRpcDispatchException( "CIVIL3D.HOST_BUSY", "Civil 3D is not processing requests: a dialog is open');
  });

  // An OpenCloseTransaction is not rolled back on abort, and Idle ticks run
  // outside any command, so a plain read must not create scratch objects, use
  // the pickfirst selection or run commands. Those use ReadInCommandAsync or
  // ReadThenCommandLineAsync, and command-line input goes through
  // QueueCommandLine (sent over COM after the operation), never
  // SendStringToExecute, whose input mixes with C3DMCPHOP's.
  it("keeps scratch objects, pickfirst and command-line input out of plain reads", () => {
    const markers = {
      scratch: ["GetVolumeProperties(", "ComputeVolumeBetweenSurfaces(", "GetEarthworkVolumes(", "TinVolumeSurface.Create(", ".Create("],
      pickfirst: ["SetImpliedSelection(", "SelectImplied(", "BuildSelectedCivilObjects("],
      commandLine: ["QueueCommandLine(", "SendCommand("],
      writes: ["UpgradeOpen(", "OpenMode.ForWrite", ".Commit("],
    };
    type Group = keyof typeof markers;
    const forbidden: Record<string, Group[]> = {
      ReadAsync: ["scratch", "pickfirst", "commandLine", "writes"],
      ReadThenCommandLineAsync: ["scratch", "pickfirst", "writes"],
      ReadInCommandAsync: ["writes"],
    };
    const offenders: string[] = [];
    const unscanned: string[] = [];
    const counts: Record<string, number> = {};
    const check = (where: string, body: string, groups: Iterable<Group>) => {
      const bad = [...groups].flatMap((group) => markers[group].filter((marker) => body.replace(/\s+/g, "").includes(marker)));
      if (bad.length > 0) offenders.push(`${where}: ${bad.join(", ")}`);
    };

    for (const name of pluginFiles.filter((file) => file !== "CivilExecution.cs")) {
      const masked = code(name);
      // A delegate passed through a parameter of a helper (CostEstimation's
      // ReadQuantitiesAsync): the groups forbidden on any route it may take.
      const viaParameter = new Map<string, { method: string; position: number; kinds: Set<string>; groups: Set<Group> }>();
      for (const match of masked.matchAll(/CivilExecution\.(ReadAsync|ReadInCommandAsync|ReadThenCommandLineAsync)\s*(?:<[^()]*>)?\s*\(/g)) {
        const kind = match[1];
        counts[kind] = (counts[kind] ?? 0) + 1;
        const open = match.index! + match[0].length - 1;
        const argument = callArguments(masked, open)[0]?.text ?? "";
        if (argument.includes("=>")) {
          check(`${name} ${kind}`, argument, forbidden[kind]);
          continue;
        }
        const identifier = argument.trim();
        const method = enclosingMethod(masked, match.index!);
        const position = method?.parameters.indexOf(identifier) ?? -1;
        if (!method || position < 0) {
          unscanned.push(`${name} ${kind}(${identifier})`);
          continue;
        }
        const key = `${method.name}#${position}`;
        const entry = viaParameter.get(key) ?? { method: method.name, position, kinds: new Set<string>(), groups: new Set<Group>() };
        entry.kinds.add(kind);
        forbidden[kind].forEach((group) => entry.groups.add(group));
        viaParameter.set(key, entry);
      }

      for (const { method, position, kinds, groups } of viaParameter.values()) {
        // Scratch objects are allowed only when the helper also routes the
        // delegate to ReadInCommandAsync, on a condition pinned by the cost
        // quantities test above.
        const allowed = new Set(groups);
        if (kinds.has("ReadInCommandAsync")) allowed.delete("scratch");
        let callers = 0;
        for (const call of masked.matchAll(new RegExp(`(?<![\\w.])${method}\\s*\\(`, "g"))) {
          const open = call.index! + call[0].length - 1;
          const after = masked.slice(matchingClose(masked, open) + 1);
          if (/^\s*\{/.test(after)) continue; // the declaration
          const argument = callArguments(masked, open)[position]?.text ?? "";
          if (!argument.includes("=>")) {
            unscanned.push(`${name} ${method}(... ${argument.trim()} ...)`);
            continue;
          }
          callers++;
          check(`${name} ${method}`, argument, allowed);
        }
        if (callers === 0) unscanned.push(`${name} ${method}: no callers found`);
      }

      if (/SendStringToExecute\s*\(/.test(masked)) offenders.push(`${name}: SendStringToExecute`);
    }
    expect(offenders).toEqual([]);
    expect(unscanned).toEqual([]);
    expect(counts.ReadAsync).toBeGreaterThan(100);
    expect(counts.ReadInCommandAsync).toBeGreaterThan(0);
    expect(counts.ReadThenCommandLineAsync).toBeGreaterThan(0);
  });
});
