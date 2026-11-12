import { readFile, writeFile } from "node:fs/promises";
import { TOOL_CATALOG } from "../build/tools/tool_catalog.js";

const pkg = JSON.parse(await readFile(new URL("../package.json", import.meta.url), "utf8"));
const entries = [...TOOL_CATALOG].sort((a, b) =>
  a.domain.localeCompare(b.domain) || a.toolName.localeCompare(b.toolName));
const domains = new Set(entries.map((entry) => entry.domain));
const rows = entries.map((entry) =>
  `| \`${entry.toolName}\` | ${entry.domain} | ${(entry.operations ?? []).join(", ") || "—"} | ${(entry.pluginMethods ?? []).join(", ") || "—"} | ${entry.safeForRetry ? "yes" : "no"} |`);
const output = `# Generated Civil 3D tool reference\n\n` +
  `Generated from the runtime manifest for civil3d-mcp ${pkg.version}. Do not edit by hand.\n\n` +
  `- Catalog entries: ${entries.length}\n- Domains: ${domains.size}\n\n` +
  `| Tool | Domain | Operations | Plugin methods | Safe retry |\n|---|---|---|---|---|\n${rows.join("\n")}\n`;
const target = new URL("../docs/tools.generated.md", import.meta.url);

if (process.argv.includes("--check")) {
  const current = await readFile(target, "utf8").catch(() => "");
  // Compare with normalized line endings: git's autocrlf checks the file out
  // as CRLF on Windows while the generator writes LF, which is not staleness.
  if (current.replaceAll("\r\n", "\n") !== output) {
    console.error("docs/tools.generated.md is stale. Run npm run docs:generate.");
    process.exit(1);
  }
  console.log(`Generated tool reference is current (${entries.length} entries).`);
} else {
  await writeFile(target, output, "utf8");
  console.log(`Generated ${entries.length} tool entries across ${domains.size} domains.`);
}
