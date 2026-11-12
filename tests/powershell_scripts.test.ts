import { describe, expect, it } from "vitest";
import { spawn, spawnSync } from "node:child_process";
import { appendFileSync, copyFileSync, existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

// The install and launch scripts are Windows-only and run under Windows PowerShell 5.1; pwsh is checked
// too when it is installed, so neither edition corrupts files or the MCP stdio channel.
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const onWindows = process.platform === "win32";
const hasPwsh = onWindows && spawnSync("where.exe", ["pwsh"], { stdio: "ignore" }).status === 0;
const shells = ["powershell", ...(hasPwsh ? ["pwsh"] : [])];

function runPowerShell(shell: string, script: string, options: { cwd?: string; env?: NodeJS.ProcessEnv } = {}) {
  return spawnSync(shell, ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script], {
    cwd: options.cwd,
    env: options.env,
    encoding: "utf8",
    input: "",
    timeout: 120_000,
    windowsHide: true,
  });
}

// The same, without blocking the event loop, for launches that run side by side; `done` turns true when
// the process has exited.
function startPowerShell(shell: string, args: string[], options: { cwd?: string; env?: NodeJS.ProcessEnv } = {}) {
  const child = spawn(shell, ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", ...args], {
    cwd: options.cwd,
    env: options.env,
    stdio: ["ignore", "pipe", "pipe"],
    windowsHide: true,
  });
  let stdout = "";
  let stderr = "";
  child.stdout.setEncoding("utf8").on("data", (chunk: string) => (stdout += chunk));
  child.stderr.setEncoding("utf8").on("data", (chunk: string) => (stderr += chunk));
  const run = {
    done: false,
    result: new Promise<{ status: number | null; stdout: string; stderr: string }>((resolve, reject) => {
      const timer = setTimeout(() => child.kill(), 110_000);
      child.on("error", reject);
      child.on("close", (status) => {
        clearTimeout(timer);
        run.done = true;
        resolve({ status, stdout, stderr });
      });
    }),
  };
  return run;
}

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

async function waitFor(what: string, check: () => boolean, ms = 60_000) {
  const deadline = Date.now() + ms;
  while (!check()) {
    if (Date.now() > deadline) throw new Error(`Timed out waiting for ${what}`);
    await sleep(50);
  }
}

// A throwaway repo holding a copy of the launcher, with fake npm.cmd and node.cmd first on PATH.
// The fake npm logs its arguments and behaves like install / run build (prebuild deletes build\, then
// tsc emits build\index.js even when it reports errors); the fake node logs that it was started.
// Like npm 11, its install rewrites package-lock.json unless --no-save is passed. Two npm runs at once
// log OVERLAP. With FAKE_NPM_HOLD_BUILD set to a file path, the build writes build-started next to the
// fakes and waits until the file at FAKE_NPM_HOLD_BUILD exists.
function makeLauncherRepo() {
  const temp = mkdtempSync(path.join(os.tmpdir(), "c3dmcp-launcher-"));
  const repo = path.join(temp, "repo");
  const bin = path.join(temp, "bin");
  const logDir = path.join(temp, "temp");
  for (const dir of [path.join(repo, "scripts"), path.join(repo, "src", "tools"), bin, logDir]) mkdirSync(dir, { recursive: true });
  copyFileSync(path.join(repoRoot, "scripts", "claude-bootstrap-and-run.ps1"), path.join(repo, "scripts", "claude-bootstrap-and-run.ps1"));
  const files: Record<string, string> = {
    "package.json": '{ "name": "fake", "version": "1.0.0" }\n',
    "package-lock.json": '{ "lockfileVersion": 3 }\n',
    "tsconfig.json": "{}\n",
    "autodesk_videos.json": "[]\n",
    "scripts/copy-static-assets.mjs": "// copies assets\n",
    "src/index.ts": "export {};\n",
    "src/tools/a.ts": "export const a = 1;\n",
    "src/tools/b.ts": "export const b = 2;\n",
  };
  for (const [name, text] of Object.entries(files)) writeFileSync(path.join(repo, name), text);

  writeFileSync(path.join(bin, "fake-npm.mjs"), `
import { appendFileSync, closeSync, existsSync, mkdirSync, openSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
const args = process.argv.slice(2);
const calls = ${JSON.stringify(path.join(bin, "npm-calls.txt"))};
appendFileSync(calls, args.join(" ") + "\\n");
const running = ${JSON.stringify(path.join(bin, "npm-running"))};
try {
  closeSync(openSync(running, "wx"));
  process.on("exit", () => rmSync(running, { force: true }));
} catch {
  appendFileSync(calls, "OVERLAP\\n");
}
const prefix = args[args.indexOf("--prefix") + 1];
if (process.env.FAKE_NPM_STDERR) process.stderr.write("npm notice New major version of npm available!\\n");
if (args[0] === "install") {
  mkdirSync(path.join(prefix, "node_modules"), { recursive: true });
  if (!args.includes("--no-save")) writeFileSync(path.join(prefix, "package-lock.json"), '{ "lockfileVersion": 3, "rewritten": true }\\n');
  console.log("added 1 package");
} else if (args[0] === "run" && args[1] === "build") {
  rmSync(path.join(prefix, "build"), { recursive: true, force: true });
  mkdirSync(path.join(prefix, "build"), { recursive: true });
  writeFileSync(path.join(prefix, "build", "index.js"), "// fake build\\n");
  const hold = process.env.FAKE_NPM_HOLD_BUILD;
  if (hold) {
    writeFileSync(${JSON.stringify(path.join(bin, "build-started"))}, "");
    const deadline = Date.now() + 90000;
    while (!existsSync(hold) && Date.now() < deadline) await new Promise((resolve) => setTimeout(resolve, 50));
  }
  if (process.env.FAKE_NPM_FAIL_BUILD) {
    console.log("src/tools/a.ts(1,1): error TS2304: Cannot find name 'x'.");
    process.exit(2);
  }
  console.log("built");
}
`);
  writeFileSync(path.join(bin, "npm.cmd"), `@"${process.execPath}" "%~dp0fake-npm.mjs" %*\r\n@exit /b %ERRORLEVEL%\r\n`);
  writeFileSync(path.join(bin, "node.cmd"), `@echo off\r\n>>"%~dp0node-calls.txt" echo %*\r\nexit /b 0\r\n`);

  const env: NodeJS.ProcessEnv = {};
  for (const [key, value] of Object.entries(process.env)) {
    if (!["PATH", "TEMP", "TMP", "CIVIL3D_PORT", "FAKE_NPM_STDERR", "FAKE_NPM_FAIL_BUILD", "FAKE_NPM_HOLD_BUILD"].includes(key.toUpperCase())) env[key] = value;
  }
  const pathKey = Object.keys(process.env).find((key) => key.toUpperCase() === "PATH") ?? "PATH";
  env.Path = `${bin};${process.env[pathKey] ?? ""}`;
  env.TEMP = logDir;
  env.TMP = logDir;

  const take = (file: string) => {
    const full = path.join(bin, file);
    if (!existsSync(full)) return [];
    const lines = readFileSync(full, "utf8").split(/\r?\n/).filter(Boolean);
    rmSync(full);
    return lines;
  };

  const launcher = path.join(repo, "scripts", "claude-bootstrap-and-run.ps1");
  const runs: Array<Promise<unknown>> = [];
  return {
    repo,
    temp,
    bin,
    launcher,
    // One log per clone, named by the launcher's key for that clone.
    get logPath() {
      const logs = readdirSync(logDir).filter((name) => /^civil3d-mcp-bootstrap-[0-9a-f]{16}\.log$/.test(name));
      expect(logs).toHaveLength(1);
      return path.join(logDir, logs[0]);
    },
    launch(shell: string, extraEnv: NodeJS.ProcessEnv = {}) {
      const result = runPowerShell(shell, launcher, { cwd: repo, env: { ...env, ...extraEnv } });
      return { status: result.status, stdout: result.stdout, stderr: result.stderr, npm: take("npm-calls.txt"), node: take("node-calls.txt") };
    },
    start(shell: string, extraEnv: NodeJS.ProcessEnv = {}) {
      return this.track(startPowerShell(shell, [launcher], { cwd: repo, env: { ...env, ...extraEnv } }));
    },
    track<T extends { result: Promise<unknown> }>(run: T) {
      runs.push(run.result);
      return run;
    },
    // Waits for every process from start() or track(), so cleanup finds no file in use after a failure.
    async settle() {
      await Promise.allSettled(runs);
    },
    calls() {
      return { npm: take("npm-calls.txt"), node: take("node-calls.txt") };
    },
    cleanup() {
      rmSync(temp, { recursive: true, force: true });
    },
  };
}

for (const shell of shells) {
  describe.skipIf(!onWindows)(`PowerShell scripts under ${shell}`, () => {
    it("install-trusted.ps1 passes its offline checks", () => {
      const result = runPowerShell(shell, path.join(repoRoot, "tests", "powershell", "install-trusted.check.ps1"));
      expect(result.status, `${result.stdout}\n${result.stderr}`).toBe(0);
      expect(result.stdout).toContain("0 failure(s)");
    }, 120_000);

    it("launcher installs, builds once and starts node, keeping stdout for node only and the lock as committed", () => {
      const fixture = makeLauncherRepo();
      try {
        const lockPath = path.join(fixture.repo, "package-lock.json");
        const committedLock = readFileSync(lockPath, "utf8");
        const first = fixture.launch(shell);
        expect(first.status, first.stderr).toBe(0);
        expect(first.stdout).toBe("");
        expect(first.npm.map((call) => call.split(" ").slice(0, 2).join(" "))).toEqual(["install --no-save", "run build"]);
        expect(first.node).toHaveLength(1);
        expect(first.node[0]).toMatch(/[\\/]build[\\/]index\.js$/);
        expect(readFileSync(fixture.logPath, "utf8")).toContain("built");
        expect(readFileSync(lockPath, "utf8")).toBe(committedLock);

        const second = fixture.launch(shell);
        expect(second.status, second.stderr).toBe(0);
        expect(second.stdout).toBe("");
        expect(second.npm).toEqual([]);
        expect(second.node).toHaveLength(1);
      } finally {
        fixture.cleanup();
      }
    }, 120_000);

    it("launcher reinstalls before rebuilding when package-lock.json or package.json changes", () => {
      const fixture = makeLauncherRepo();
      try {
        expect(fixture.launch(shell).status).toBe(0);
        appendFileSync(path.join(fixture.repo, "package-lock.json"), " \n");
        const afterLock = fixture.launch(shell);
        expect(afterLock.status, afterLock.stderr).toBe(0);
        expect(afterLock.stdout).toBe("");
        expect(afterLock.npm.map((call) => call.split(" ")[0])).toEqual(["install", "run"]);

        writeFileSync(path.join(fixture.repo, "package.json"), '{ "name": "fake", "version": "1.0.1" }\n');
        const afterPackage = fixture.launch(shell);
        expect(afterPackage.npm.map((call) => call.split(" ")[0])).toEqual(["install", "run"]);
        expect(fixture.launch(shell).npm).toEqual([]);
      } finally {
        fixture.cleanup();
      }
    }, 120_000);

    it("launcher rebuilds when a source file is deleted or added, or a copied asset changes", () => {
      const fixture = makeLauncherRepo();
      try {
        expect(fixture.launch(shell).status).toBe(0);
        rmSync(path.join(fixture.repo, "src", "tools", "b.ts"));
        expect(fixture.launch(shell).npm).toEqual([`run build --prefix ${fixture.repo}`]);
        writeFileSync(path.join(fixture.repo, "src", "tools", "c.ts"), "export const c = 3;\n");
        expect(fixture.launch(shell).npm).toEqual([`run build --prefix ${fixture.repo}`]);
        writeFileSync(path.join(fixture.repo, "autodesk_videos.json"), '[{ "id": 1 }]\n');
        expect(fixture.launch(shell).npm).toEqual([`run build --prefix ${fixture.repo}`]);
        expect(fixture.launch(shell).npm).toEqual([]);
      } finally {
        fixture.cleanup();
      }
    }, 120_000);

    it("launcher does not stop when npm writes to stderr", () => {
      const fixture = makeLauncherRepo();
      try {
        const result = fixture.launch(shell, { FAKE_NPM_STDERR: "1" });
        expect(result.status, result.stderr).toBe(0);
        expect(result.stdout).toBe("");
        expect(result.node).toHaveLength(1);
        expect(readFileSync(fixture.logPath, "utf8")).toContain("npm notice New major version");
      } finally {
        fixture.cleanup();
      }
    }, 120_000);

    it("launcher reports a failed build on stderr and builds again on the next launch", () => {
      const fixture = makeLauncherRepo();
      try {
        const failed = fixture.launch(shell, { FAKE_NPM_FAIL_BUILD: "1" });
        expect(failed.status).toBe(1);
        expect(failed.stdout).toBe("");
        expect(failed.node).toEqual([]);
        expect(failed.stderr).toContain("Failed to build civil3d-mcp");
        expect(failed.stderr).toContain(fixture.logPath);
        expect(failed.stderr).toContain("error TS2304");
        // tsc still emitted build\index.js: only the missing stamp makes the next launch build again.
        expect(existsSync(path.join(fixture.repo, "build", "index.js"))).toBe(true);

        const retried = fixture.launch(shell);
        expect(retried.status, retried.stderr).toBe(0);
        expect(retried.npm).toEqual([`run build --prefix ${fixture.repo}`]);
        expect(retried.node).toHaveLength(1);
      } finally {
        fixture.cleanup();
      }
    }, 120_000);

    it("concurrent launches of one clone install and build once, and both start node", async () => {
      const fixture = makeLauncherRepo();
      const release = path.join(fixture.temp, "release-build");
      try {
        const holdEnv = { FAKE_NPM_HOLD_BUILD: release };
        const first = fixture.start(shell, holdEnv);
        await waitFor("the first launcher's build", () => existsSync(path.join(fixture.bin, "build-started")) || first.done);
        expect(first.done, "first launcher exited before building").toBe(false);
        const second = fixture.start(shell, holdEnv);
        // The second finds no build stamp yet, so it waits for the lock instead of building alongside.
        await sleep(3000);
        expect(second.done).toBe(false);
        writeFileSync(release, "");
        for (const run of await Promise.all([first.result, second.result])) {
          expect(run.status, run.stderr).toBe(0);
          expect(run.stdout).toBe("");
        }
        const { npm, node } = fixture.calls();
        expect(npm).toEqual([`install --no-save --no-fund --no-audit --prefix ${fixture.repo}`, `run build --prefix ${fixture.repo}`]);
        expect(node).toHaveLength(2);
      } finally {
        writeFileSync(release, "");
        await fixture.settle();
        fixture.cleanup();
      }
    }, 120_000);

    it("launcher skips the lock when up to date, waits for it otherwise, and takes over an abandoned lock", async () => {
      const fixture = makeLauncherRepo();
      const ready = path.join(fixture.temp, "lock-held");
      const release = path.join(fixture.temp, "release-lock");
      try {
        expect(fixture.launch(shell).status).toBe(0);
        // The helper holds the launcher's lock for this clone, then exits without releasing it, as a launcher
        // killed mid-build would.
        const holder = fixture.track(startPowerShell(shell, [
          path.join(repoRoot, "tests", "powershell", "hold-launcher-lock.ps1"),
          "-Launcher", fixture.launcher, "-ReadyFile", ready, "-ReleaseFile", release,
        ]));
        await waitFor("the helper to hold the lock", () => existsSync(ready) || holder.done);
        expect(holder.done, "helper exited early").toBe(false);

        const upToDate = await fixture.start(shell).result;
        expect(upToDate.status, upToDate.stderr).toBe(0);
        expect(holder.done, "the up-to-date launch waited for the lock").toBe(false);
        expect(fixture.calls()).toEqual({ npm: [], node: [expect.any(String)] });

        writeFileSync(path.join(fixture.repo, "src", "tools", "c.ts"), "export const c = 3;\n");
        const stale = fixture.start(shell);
        await sleep(3000);
        expect(stale.done, "the stale launch did not wait for the lock").toBe(false);
        expect(existsSync(path.join(fixture.bin, "npm-calls.txt"))).toBe(false);
        writeFileSync(release, "");
        const held = await holder.result;
        expect(held.status, held.stderr).toBe(0);
        const rebuilt = await stale.result;
        expect(rebuilt.status, rebuilt.stderr).toBe(0);
        expect(rebuilt.stdout).toBe("");
        expect(fixture.calls()).toEqual({ npm: [`run build --prefix ${fixture.repo}`], node: [expect.any(String)] });
      } finally {
        writeFileSync(release, "");
        await fixture.settle();
        fixture.cleanup();
      }
    }, 120_000);
  });
}
