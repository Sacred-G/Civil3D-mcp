// Runs a C# file through civil3d_run_code (the real MCP server, build/index.js, with the approval step)
// and prints the plugin's response as JSON. For people and scripts outside an MCP client.
//
//   npm run build
//   node scripts/run-code-file.mjs <script.cs> [--mode read|write] [--timeout <seconds>]
//        [--args '<json object>'] [--args-file <file.json>] [--max-inline <bytes>]
import { readFileSync } from "node:fs";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const argv = process.argv.slice(2);
const option = (name) => {
  const index = argv.indexOf(name);
  return index >= 0 ? argv[index + 1] : undefined;
};
const file = argv.find((arg, index) => !arg.startsWith("--") && (index === 0 || !argv[index - 1].startsWith("--")));
if (!file) {
  console.error("usage: node scripts/run-code-file.mjs <script.cs> [--mode read|write] [--timeout s] [--args json] [--args-file f.json] [--max-inline bytes]");
  process.exit(2);
}

// Windows PowerShell 5.1 writes UTF-8 files with a byte-order mark; JSON.parse rejects it.
const readText = (path) => readFileSync(path, "utf8").replace(/^﻿/, "");
const parameters = { code: readText(file) };
if (option("--mode")) parameters.mode = option("--mode");
if (option("--timeout")) parameters.timeoutSeconds = Number(option("--timeout"));
if (option("--max-inline")) parameters.maxInlineBytes = Number(option("--max-inline"));
if (option("--args")) parameters.args = JSON.parse(option("--args"));
if (option("--args-file")) parameters.args = JSON.parse(readText(option("--args-file")));

const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["./build/index.js"],
  cwd: process.cwd(),
  env: { ...process.env, MCP_HTTP_HOST: "127.0.0.1", MCP_HTTP_PORT: "0" },
  stderr: "pipe",
});
const client = new Client({ name: "civil3d-run-code-file", version: "1.0.0" });
const options = { timeout: 180_000, maxTotalTimeout: 180_000 };
const text = (result) => (result.content ?? []).map((item) => item.text ?? "").join("\n");

try {
  await client.connect(transport);
  const approval = await client.callTool({
    name: "civil3d_request_approval",
    arguments: { toolName: "civil3d_run_code", action: "run", parameters },
  }, undefined, options);
  if (approval.isError) throw new Error(text(approval));
  const response = await client.callTool({
    name: "civil3d_run_code",
    arguments: { ...parameters, approvalToken: approval.structuredContent.approvalToken },
  }, undefined, options);
  if (response.isError) throw new Error(text(response));
  const result = response.structuredContent.result;
  console.log(JSON.stringify(result, null, 2));
  process.exitCode = result.ok ? 0 : 1;
} catch (error) {
  console.error(`run-code-file failed: ${error instanceof Error ? error.message : String(error)}`);
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
