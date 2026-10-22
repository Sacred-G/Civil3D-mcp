import { spawnSync } from "node:child_process";
import { readFileSync } from "node:fs";

const packageJson = JSON.parse(readFileSync(new URL("../package.json", import.meta.url), "utf8"));
if (packageJson.main !== "build/index.js" || packageJson.bin?.["civil3d-mcp"] !== "./build/index.js") {
  throw new Error("package.json main/bin entrypoints must both target build/index.js.");
}

const packed = process.platform === "win32"
  ? spawnSync(process.env.ComSpec ?? "cmd.exe", ["/d", "/s", "/c", "npm pack --dry-run --json"], { encoding: "utf8" })
  : spawnSync("npm", ["pack", "--dry-run", "--json"], { encoding: "utf8" });
if (packed.status !== 0) {
  process.stderr.write(packed.stderr ?? String(packed.error ?? "npm pack failed"));
  process.exit(packed.status ?? 1);
}

const manifest = JSON.parse(packed.stdout)[0];
const paths = manifest.files.map((file) => file.path);
for (const required of [
  "build/index.js",
  "build/version.js",
  "build/standards/data/civil3d_framework_rules.json",
  "build/standards/data/mdwasd_rules.json",
  "LICENSE",
  "README.md",
]) {
  if (!paths.includes(required)) throw new Error(`Package is missing required runtime file '${required}'.`);
}

const forbidden = paths.filter((path) => /(^|\/)(C_References|bin|obj)(\/|$)|\.(dll|pdb)$/i.test(path));
if (forbidden.length > 0) throw new Error(`Package contains forbidden binary/reference files: ${forbidden.join(", ")}`);
console.log(`Package inspection passed: ${manifest.entryCount} files, no Autodesk binaries.`);
