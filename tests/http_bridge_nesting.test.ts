import { afterEach, beforeAll, describe, expect, it, vi } from "vitest";
import type { AddressInfo } from "node:net";
import type { Server } from "node:http";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";

// The real tool registry behind the real bridge; only the plugin connection is
// faked, so nothing reaches Civil 3D.
const { pluginCalls } = vi.hoisted(() => ({ pluginCalls: [] as string[] }));

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async <T>(
    operation: (client: { sendCommand: (method: string, params?: unknown) => Promise<unknown> }) => Promise<T>,
  ) => await operation({
    sendCommand: async (method: string) => {
      pluginCalls.push(method);
      if (method === "getProjectContext") return { drawingInfo: { name: "Job.dwg" }, objectTypes: [], selectedObjects: [] };
      if (method === "getCivil3DHealth") return { drawingLoaded: true };
      if (method === "getDrawingInfo") return { name: "Job.dwg" };
      if (method === "runCode") {
        return { runId: "r", mode: "read", status: "ok", ok: true, logFolder: "x", timing: { compileMs: 0, runMs: 0, totalMs: 0 } };
      }
      throw new Error(`unexpected plugin method ${method}`);
    },
  }),
}));

import { registerTools } from "../src/tools/register.js";
import { approvalPolicy } from "../src/tools/approvalPolicy.js";
import { startHttpBridge } from "../src/httpBridge.js";

const TOKEN = "secret-token";

// civil3d_orchestrate -> civil3d_docs (orchestrate) -> civil3d_run_code: the
// first hop names only civil3d_docs.
function nestedRunCode(approvalToken?: string) {
  return {
    toolName: "civil3d_docs",
    toolAction: "orchestrate",
    execute: true,
    toolParameters: {
      action: "orchestrate",
      toolName: "civil3d_run_code",
      execute: true,
      toolParameters: { code: "return 1;", ...(approvalToken ? { approvalToken } : {}) },
    },
  };
}

async function mintRunCodeApproval(): Promise<string> {
  const receipt = await approvalPolicy.requestApproval(
    { toolName: "civil3d_run_code", action: "run", capabilities: ["query", "analyze", "manage"], safeForRetry: false, requiresActiveDrawing: true },
    { code: "return 1;" },
  );
  return receipt.approvalToken;
}

describe("HTTP bridge gate with the real tool registry", () => {
  let server: Server | undefined;

  beforeAll(async () => {
    await registerTools(new McpServer({ name: "bridge-nesting", version: "test" }));
  });

  afterEach(async () => {
    pluginCalls.length = 0;
    const closing = server;
    server = undefined;
    if (closing) await new Promise<void>((resolve) => closing.close(() => resolve()));
  });

  async function post(authToken: string, tool: string, parameters: Record<string, unknown>) {
    server = startHttpBridge({ port: 0, host: "127.0.0.1", authToken });
    await new Promise<void>((resolve, reject) => {
      server!.once("listening", () => resolve());
      server!.once("error", reject);
    });
    const { port } = server.address() as AddressInfo;
    const response = await fetch(`http://127.0.0.1:${port}/execute`, {
      method: "POST",
      headers: { "Content-Type": "application/json", ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}) },
      body: JSON.stringify({ tool, parameters }),
    });
    return { status: response.status, body: await response.json() as { error?: { code: string; message: string } } };
  }

  it("refuses orchestrate -> docs -> run_code without a token, before anything reaches the plugin", async () => {
    const res = await post("", "civil3d_orchestrate", nestedRunCode());

    expect(res.status).toBe(403);
    expect(res.body.error?.code).toBe("CIVIL3D.FORBIDDEN");
    expect(res.body.error?.message).toMatch(/civil3d_run_code is not available over the HTTP bridge without authentication/);
    expect(pluginCalls).toEqual([]);
  });

  it("refuses docs -> docs -> run_code without a token", async () => {
    const res = await post("", "civil3d_docs", { action: "orchestrate", ...nestedRunCode() });

    expect(res.status).toBe(403);
    expect(pluginCalls).toEqual([]);
  });

  it("refuses the nested call without a token even when it carries a valid approval token", async () => {
    const approvalToken = await mintRunCodeApproval();
    pluginCalls.length = 0;

    const res = await post("", "civil3d_orchestrate", nestedRunCode(approvalToken));

    expect(res.status).toBe(403);
    expect(pluginCalls).not.toContain("runCode");
  });

  it("lets the same nested call through with the bridge token: it reaches run_code", async () => {
    const unapproved = await post(TOKEN, "civil3d_orchestrate", nestedRunCode());
    expect(unapproved.status).toBe(409);
    expect(unapproved.body.error?.message).toMatch(/Approval required for 'civil3d_run_code'/);
    await new Promise<void>((resolve) => server!.close(() => resolve()));
    server = undefined;

    const approvalToken = await mintRunCodeApproval();
    const approved = await post(TOKEN, "civil3d_orchestrate", nestedRunCode(approvalToken));
    expect(approved.status).toBe(200);
    expect(pluginCalls.filter((method) => method === "runCode")).toHaveLength(1);
  });

  it("still runs nested orchestration of other tools without a token", async () => {
    const res = await post("", "civil3d_orchestrate", {
      toolName: "civil3d_docs",
      toolAction: "orchestrate",
      execute: true,
      toolParameters: {
        action: "orchestrate",
        toolName: "civil3d_docs",
        toolAction: "list_tool_capabilities",
        execute: true,
        toolParameters: { action: "list_tool_capabilities", domain: "alignment" },
      },
    });

    expect(res.status).toBe(200);
    expect(res.body).toMatchObject({
      status: "executed",
      result: { status: "executed", selectedTool: "civil3d_docs", selectedAction: "list_tool_capabilities" },
    });
    expect(pluginCalls).not.toContain("runCode");
  });
});
