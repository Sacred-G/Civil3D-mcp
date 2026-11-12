import { readFile, writeFile } from "node:fs/promises";

const packageJson = JSON.parse(await readFile(new URL("../package.json", import.meta.url), "utf8"));
const numericVersion = String(packageJson.version).split("-")[0];
const assemblyVersion = `${numericVersion}.0`.split(".").slice(0, 4).join(".");
const output = `<Project>\n  <!-- Generated from package.json by scripts/sync-version.mjs. -->\n  <PropertyGroup>\n    <Version>${packageJson.version}</Version>\n    <AssemblyVersion>${assemblyVersion}</AssemblyVersion>\n    <FileVersion>${assemblyVersion}</FileVersion>\n    <InformationalVersion>${packageJson.version}</InformationalVersion>\n  </PropertyGroup>\n</Project>\n`;
const target = new URL("../Civil3D-MCP-Plugin/GeneratedVersion.props", import.meta.url);
const manifestTarget = new URL("../packaging/claude-desktop/manifest.json", import.meta.url);
const manifest = JSON.parse(await readFile(manifestTarget, "utf8"));
const manifestOutput = `${JSON.stringify({ ...manifest, version: packageJson.version }, null, 2)}\n`;

if (process.argv.includes("--check")) {
  const current = await readFile(target, "utf8").catch(() => "");
  const currentManifest = await readFile(manifestTarget, "utf8").catch(() => "");
  // Normalize line endings: git's autocrlf checks files out as CRLF on Windows
  // while this script writes LF, which is not staleness.
  const lf = (s) => s.replaceAll("\r\n", "\n");
  if (lf(current) !== output || lf(currentManifest) !== manifestOutput) {
    console.error("Generated version files are stale. Run npm run version:sync.");
    process.exit(1);
  }
  console.log(`Version sources agree on ${packageJson.version}.`);
} else {
  await Promise.all([
    writeFile(target, output, "utf8"),
    writeFile(manifestTarget, manifestOutput, "utf8"),
  ]);
  console.log(`Synchronized plugin and MCPB versions to ${packageJson.version}.`);
}
