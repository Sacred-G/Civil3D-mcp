import { describe, expect, it } from "vitest";
import { spawn, spawnSync } from "node:child_process";
import { copyFileSync, existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

// The 2027 build and deploy scripts are Windows-only. They run under Windows
// PowerShell 5.1, and under pwsh 7 when it is installed.
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const onWindows = process.platform === "win32";
const hasPwsh = onWindows && spawnSync("where.exe", ["pwsh"], { stdio: "ignore" }).status === 0;
const shells = ["powershell", ...(hasPwsh ? ["pwsh"] : [])];

// Windows PowerShell 5.1 started from PowerShell 7 inherits 7's module
// folders; drop PSModulePath for 5.1 so it rebuilds its default.
// Windows variable names are case-insensitive: an extra PATH replaces Path.
function shellEnv(shell: string, extra: NodeJS.ProcessEnv = {}): NodeJS.ProcessEnv {
  const env: NodeJS.ProcessEnv = { ...process.env };
  for (const key of Object.keys(extra)) {
    for (const existing of Object.keys(env)) if (existing.toUpperCase() === key.toUpperCase()) delete env[existing];
    env[key] = extra[key];
  }
  if (shell === "powershell") for (const key of Object.keys(env)) if (key.toUpperCase() === "PSMODULEPATH") delete env[key];
  return env;
}

function run(shell: string, script: string, args: string[], extraEnv: NodeJS.ProcessEnv = {}) {
  const result = spawnSync(shell, ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script, ...args], {
    env: shellEnv(shell, extraEnv),
    encoding: "utf8",
    input: "",
    timeout: 120_000,
    windowsHide: true,
  });
  return { status: result.status, output: `${result.stdout}\n${result.stderr}` };
}

const tempRoots: string[] = [];
function tempDir(label: string) {
  const dir = mkdtempSync(path.join(os.tmpdir(), `c3dmcp-${label}-`));
  tempRoots.push(dir);
  return dir;
}

function cleanup() {
  for (const dir of tempRoots.splice(0)) rmSync(dir, { recursive: true, force: true, maxRetries: 10, retryDelay: 200 });
}

function fakeBuild(root: string, tfm: string, files: Record<string, string>) {
  const dir = path.join(root, "bin", tfm);
  mkdirSync(dir, { recursive: true });
  for (const [name, text] of Object.entries(files)) writeFileSync(path.join(dir, name), text);
  return dir;
}

const installBundle = path.join(repoRoot, "scripts", "install-bundle.ps1");
const gatherRefs = path.join(repoRoot, "scripts", "gather-refs-2027.ps1");
const references = ["accoremgd.dll", "AcDbMgd.dll", "acmgd.dll", "ACA/AecBaseMgd.dll", "C3D/AeccDbMgd.dll", "C3D/AeccPressurePipesMgd.dll"];

describe.skipIf(!onWindows).each(shells)("2027 deploy scripts under %s", (shell) => {
  it("install-bundle replaces the bundle as a whole and leaves no staging folders", () => {
    try {
      const root = tempDir("bundle");
      const bundleRoot = path.join(root, "ApplicationPlugins");
      const first = fakeBuild(path.join(root, "a"), "net10.0-windows", { "Civil3DMcpPlugin.dll": "v1", "Old.dll": "old", "AeccDbMgd.dll": "autodesk" });
      let result = run(shell, installBundle, ["-SourceDir", first, "-BundleRoot", bundleRoot]);
      expect(result.status, result.output).toBe(0);
      const contents = path.join(bundleRoot, "Civil3DMcp.bundle", "Contents");
      expect(readdirSync(contents).sort()).toEqual(["Civil3DMcpPlugin.dll", "Old.dll"]);
      const manifest = readFileSync(path.join(bundleRoot, "Civil3DMcp.bundle", "PackageContents.xml"));
      expect(manifest[0]).toBe(0x3c); // "<": no BOM
      expect(manifest.toString("utf8")).toContain('SeriesMin="R26.0" SeriesMax="R26.0"');

      const second = fakeBuild(path.join(root, "b"), "net10.0-windows", { "Civil3DMcpPlugin.dll": "v2" });
      result = run(shell, installBundle, ["-SourceDir", second, "-BundleRoot", bundleRoot]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(contents)).toEqual(["Civil3DMcpPlugin.dll"]);
      expect(readFileSync(path.join(contents, "Civil3DMcpPlugin.dll"), "utf8")).toBe("v2");
      expect(readdirSync(bundleRoot)).toEqual(["Civil3DMcp.bundle"]);

      result = run(shell, installBundle, ["-BundleRoot", bundleRoot, "-Uninstall"]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(bundleRoot)).toEqual([]);
    } finally {
      cleanup();
    }
  }, 120_000);

  it("install-bundle keeps the previous bundle when the new copy fails", async () => {
    try {
      const root = tempDir("bundle-fail");
      const bundleRoot = path.join(root, "ApplicationPlugins");
      const good = fakeBuild(path.join(root, "a"), "net10.0-windows", { "Civil3DMcpPlugin.dll": "v1" });
      expect(run(shell, installBundle, ["-SourceDir", good, "-BundleRoot", bundleRoot]).status).toBe(0);

      const broken = fakeBuild(path.join(root, "b"), "net10.0-windows", { "Civil3DMcpPlugin.dll": "v2", "Civil3DMcpPlugin.deps.json": "{}" });
      // Hold an exclusive lock on one source file so the copy fails midway.
      const holder = spawn("powershell", ["-NoProfile", "-Command",
        `$f = [System.IO.File]::Open('${path.join(broken, "Civil3DMcpPlugin.deps.json").replace(/'/g, "''")}', 'Open', 'ReadWrite', 'None'); 'locked'; Start-Sleep -Seconds 60`],
        { env: shellEnv("powershell"), stdio: ["ignore", "pipe", "ignore"], windowsHide: true });
      try {
        await new Promise<void>((resolve, reject) => {
          let locked = false;
          holder.stdout.setEncoding("utf8").on("data", (chunk: string) => {
            if (chunk.includes("locked")) {
              locked = true;
              resolve();
            }
          });
          // Only an exit before the lock was taken is a failure.
          holder.once("exit", () => { if (!locked) reject(new Error("lock holder exited before taking the lock")); });
        });
        const result = run(shell, installBundle, ["-SourceDir", broken, "-BundleRoot", bundleRoot]);
        expect(result.status, result.output).not.toBe(0);
        expect(result.output).toContain("the previous bundle (if any) is unchanged");
      } finally {
        // Wait until the holder is gone, so its lock is released before the
        // temp folder is deleted.
        const exited = holder.exitCode !== null || holder.signalCode !== null
          ? Promise.resolve()
          : new Promise<void>((resolve) => holder.once("exit", () => resolve()));
        holder.kill();
        await Promise.race([exited, new Promise<void>((resolve) => setTimeout(resolve, 15_000))]);
      }
      const contents = path.join(bundleRoot, "Civil3DMcp.bundle", "Contents");
      expect(readFileSync(path.join(contents, "Civil3DMcpPlugin.dll"), "utf8")).toBe("v1");
      expect(readdirSync(bundleRoot)).toEqual(["Civil3DMcp.bundle"]);
    } finally {
      cleanup();
    }
  }, 120_000);

  it.each(["..", ".", "..\\..", "a\\b", "a/b", "C:\\x", "x.bundle.", " "])("install-bundle refuses BundleName %j", (name) => {
    try {
      const root = tempDir("bundle-name");
      const bundleRoot = path.join(root, "ApplicationPlugins");
      mkdirSync(bundleRoot, { recursive: true });
      writeFileSync(path.join(root, "sentinel.txt"), "keep");
      writeFileSync(path.join(bundleRoot, "other.txt"), "keep");
      const result = run(shell, installBundle, ["-BundleRoot", bundleRoot, "-BundleName", name, "-Uninstall"]);
      expect(result.status, result.output).not.toBe(0);
      expect(result.output).toContain("must be a plain folder name");
      expect(existsSync(path.join(root, "sentinel.txt"))).toBe(true);
      expect(existsSync(path.join(bundleRoot, "other.txt"))).toBe(true);
    } finally {
      cleanup();
    }
  }, 120_000);

  it("install-bundle cleans up an interrupted run's folders, but keeps a moved-aside bundle when it is the only copy", () => {
    try {
      const root = tempDir("bundle-leftovers");
      const bundleRoot = path.join(root, "ApplicationPlugins");
      const name = "My[1].bundle";
      const orphan = path.join(bundleRoot, `.${name}.old-dead0001`);
      mkdirSync(path.join(orphan, "Contents"), { recursive: true });
      mkdirSync(path.join(bundleRoot, `.${name}.staging-dead0002`), { recursive: true });
      // Another bundle's leftovers are not touched.
      mkdirSync(path.join(bundleRoot, ".Other.bundle.staging-dead0003"), { recursive: true });

      let result = run(shell, installBundle, ["-BundleRoot", bundleRoot, "-BundleName", name, "-Uninstall"]);
      expect(result.status, result.output).toBe(0);
      expect(result.output).toContain("A previous bundle was left at");
      expect(readdirSync(bundleRoot).sort()).toEqual([`.${name}.old-dead0001`, ".Other.bundle.staging-dead0003"]);

      const build = fakeBuild(root, "net10.0-windows", { "Civil3DMcpPlugin.dll": "v1" });
      // The first install that succeeds removes the kept copy.
      result = run(shell, installBundle, ["-SourceDir", build, "-BundleRoot", bundleRoot, "-BundleName", name]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(bundleRoot).sort()).toEqual([".Other.bundle.staging-dead0003", name]);
      result = run(shell, installBundle, ["-SourceDir", build, "-BundleRoot", bundleRoot, "-BundleName", name]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(bundleRoot).sort()).toEqual([".Other.bundle.staging-dead0003", name]);
    } finally {
      cleanup();
    }
  }, 120_000);

  it("install-bundle appends .bundle to a bare name", () => {
    try {
      const root = tempDir("bundle-bare");
      const bundleRoot = path.join(root, "ApplicationPlugins");
      const build = fakeBuild(root, "net8.0-windows", { "Civil3DMcpPlugin.dll": "v1" });
      const result = run(shell, installBundle, ["-SourceDir", build, "-BundleRoot", bundleRoot, "-BundleName", "Mine"]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(bundleRoot)).toEqual(["Mine.bundle"]);
      expect(readFileSync(path.join(bundleRoot, "Mine.bundle", "PackageContents.xml"), "utf8")).toContain('SeriesMin="R25.1"');
    } finally {
      cleanup();
    }
  }, 120_000);

  it("gather-refs copies nothing unless all six references are found", () => {
    try {
      const root = tempDir("refs");
      const acad = path.join(root, "AutoCAD 2027");
      const destination = path.join(root, "C_References");
      for (const reference of references.slice(0, 5)) {
        mkdirSync(path.dirname(path.join(acad, reference)), { recursive: true });
        writeFileSync(path.join(acad, reference), `new ${reference}`);
      }
      mkdirSync(destination);
      writeFileSync(path.join(destination, "accoremgd.dll"), "old 2026");

      let result = run(shell, gatherRefs, ["-AcadRoot", acad, "-Destination", destination]);
      expect(result.status, result.output).not.toBe(0);
      expect(result.output).toContain("AeccPressurePipesMgd.dll");
      expect(result.output).toContain("Nothing was copied");
      expect(readdirSync(destination)).toEqual(["accoremgd.dll"]);
      expect(readFileSync(path.join(destination, "accoremgd.dll"), "utf8")).toBe("old 2026");

      writeFileSync(path.join(acad, references[5]), `new ${references[5]}`);
      result = run(shell, gatherRefs, ["-AcadRoot", acad, "-Destination", destination]);
      expect(result.status, result.output).toBe(0);
      expect(readdirSync(destination)).toHaveLength(6);
      expect(readFileSync(path.join(destination, "accoremgd.dll"), "utf8")).toBe("new accoremgd.dll");

      result = run(shell, gatherRefs, ["-AcadRoot", acad, "-Destination", destination]);
      expect(result.status, result.output).toBe(0);
      expect(result.output.match(/skip {4}/g)).toHaveLength(6);
    } finally {
      cleanup();
    }
  }, 120_000);

  // A fake dotnet on PATH records its calls; the script runs from a copy in a
  // fake repo so the build output check finds a DLL there.
  function fakeRepo(buildExit: number) {
    const root = tempDir("build");
    mkdirSync(path.join(root, "scripts"));
    copyFileSync(path.join(repoRoot, "scripts", "build-2027.ps1"), path.join(root, "scripts", "build-2027.ps1"));
    const output = path.join(root, "Civil3D-MCP-Plugin", "bin", "Release", "net10.0-windows");
    mkdirSync(output, { recursive: true });
    writeFileSync(path.join(output, "Civil3DMcpPlugin.dll"), "built");
    mkdirSync(path.join(root, "C_References"));
    const bin = path.join(root, "fake-bin");
    mkdirSync(bin);
    const log = path.join(root, "dotnet.log");
    writeFileSync(path.join(bin, "dotnet.cmd"), [
      "@echo off",
      `echo %* >> "${log}"`,
      `if "%1"=="build" exit /b ${buildExit}`,
      "exit /b 0",
      "",
    ].join("\r\n"));
    const currentPath = Object.entries(process.env).find(([key]) => key.toUpperCase() === "PATH")?.[1] ?? "";
    const env = { Path: `${bin}${path.delimiter}${currentPath}` };
    return { script: path.join(root, "scripts", "build-2027.ps1"), log, env };
  }

  it("build-2027 restores the default target framework after a failed build and exits non-zero", () => {
    try {
      const repo = fakeRepo(1);
      const result = run(shell, repo.script, [], repo.env);
      expect(result.status, result.output).not.toBe(0);
      expect(result.output).toContain("dotnet build failed with exit code 1");
      const calls = readFileSync(repo.log, "utf8").trim().split(/\r?\n/).map((line) => line.trim());
      expect(calls).toHaveLength(3);
      expect(calls[0]).toMatch(/^restore .*TargetFramework=net10\.0-windows/);
      expect(calls[1]).toMatch(/^build /);
      expect(calls[2]).toMatch(/^restore /);
      expect(calls[2]).not.toContain("TargetFramework");
    } finally {
      cleanup();
    }
  }, 120_000);

  it("build-2027 ends a build without -Install with exit code 0 after restoring the default", () => {
    try {
      const repo = fakeRepo(0);
      const result = run(shell, repo.script, [], repo.env);
      expect(result.status, result.output).toBe(0);
      expect(result.output).toContain("Re-run with -Install");
      const calls = readFileSync(repo.log, "utf8").trim().split(/\r?\n/).map((line) => line.trim());
      expect(calls.map((call) => call.split(" ")[0])).toEqual(["restore", "build", "restore"]);
      // return, not exit: the script must not end the session that ran it.
      expect(readFileSync(path.join(repoRoot, "scripts", "build-2027.ps1"), "utf8")).not.toMatch(/^\s*exit\b/m);
    } finally {
      cleanup();
    }
  }, 120_000);
});
