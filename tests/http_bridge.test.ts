import { afterAll, afterEach, beforeAll, describe, expect, it, vi } from "vitest";
import type { AddressInfo } from "node:net";
import type { Server } from "node:http";

// ─── Hoisted mocks ────────────────────────────────────────────────────────────

const {
  hasToolHandlerMock,
  executeRegisteredToolMock,
  listRegisteredToolNamesMock,
  executeOrchestratorMock,
} = vi.hoisted(() => ({
  hasToolHandlerMock: vi.fn<(name: string) => boolean>(),
  executeRegisteredToolMock: vi.fn<(name: string, params: Record<string, unknown>) => Promise<unknown>>(),
  listRegisteredToolNamesMock: vi.fn<() => string[]>(() => []),
  executeOrchestratorMock: vi.fn<(name: string, params: Record<string, unknown>) => Promise<unknown>>(),
}));

vi.mock("../src/tools/toolHandlerRegistry.js", () => ({
  hasToolHandler: hasToolHandlerMock,
  executeRegisteredTool: executeRegisteredToolMock,
  listRegisteredToolNames: listRegisteredToolNamesMock,
}));

vi.mock("../src/tools/civil3d_orchestrate.js", () => ({
  executeToolCallViaOrchestrator: executeOrchestratorMock,
}));

import { startHttpBridge } from "../src/httpBridge.js";
import { Civil3DRpcError } from "../src/utils/SocketClient.js";
import { currentAbortSignal } from "../src/utils/requestContext.js";

// ─── Test helpers ─────────────────────────────────────────────────────────────

interface BridgeHandle {
  server: Server;
  baseUrl: string;
  close: () => Promise<void>;
}

async function launchBridge(options: Parameters<typeof startHttpBridge>[0] = {}): Promise<BridgeHandle> {
  const server = startHttpBridge({ port: 0, host: "127.0.0.1", ...options });

  await new Promise<void>((resolve, reject) => {
    server.once("listening", () => resolve());
    server.once("error", reject);
  });

  const address = server.address() as AddressInfo;
  return {
    server,
    baseUrl: `http://127.0.0.1:${address.port}`,
    close: () =>
      new Promise<void>((resolve, reject) => {
        server.close((err) => (err ? reject(err) : resolve()));
      }),
  };
}

interface BridgeResponse {
  status: number;
  body: unknown;
}

async function request(
  method: "GET" | "POST" | "OPTIONS",
  url: string,
  init: { headers?: Record<string, string>; body?: string } = {},
): Promise<BridgeResponse> {
  const response = await fetch(url, {
    method,
    headers: init.headers,
    body: init.body,
  });

  const text = await response.text();
  let body: unknown;
  try {
    body = text.length > 0 ? JSON.parse(text) : null;
  } catch {
    body = text;
  }

  return { status: response.status, body };
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe("httpBridge", () => {
  let handle: BridgeHandle | null = null;

  afterEach(async () => {
    if (handle) {
      await handle.close();
      handle = null;
    }
    hasToolHandlerMock.mockReset();
    executeRegisteredToolMock.mockReset();
    listRegisteredToolNamesMock.mockReset();
    listRegisteredToolNamesMock.mockReturnValue([]);
    executeOrchestratorMock.mockReset();
  });

  describe("GET /health", () => {
    it("returns cheap liveness without probing the plugin", async () => {
      listRegisteredToolNamesMock.mockReturnValue(["civil3d_alignment", "civil3d_surface"]);
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/health`);

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ bridge: "ok", registeredTools: 2 });
      // cheap health MUST NOT hit the registered tool dispatcher
      expect(executeRegisteredToolMock).not.toHaveBeenCalled();
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it("exposes dependency versions without probing the plugin", async () => {
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/health/version`);

      expect(res.status).toBe(200);
      expect(res.body).toMatchObject({
        bridge: "ok",
        versions: { application: "1.2.1", mcpSdk: expect.any(String), node: expect.any(String) },
      });
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it("fails readiness when the plugin is unavailable", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_health");
      executeRegisteredToolMock.mockRejectedValue(new Error("Plugin not running"));
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/health/ready`);

      expect(res.status).toBe(503);
      expect(res.body).toMatchObject({ ready: false, connected: false });
    });

    it("fails readiness and queue health when the host queue is full", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_health");
      executeRegisteredToolMock.mockResolvedValue({ connected: true, queueDepth: 64, queueCapacity: 64 });
      handle = await launchBridge();

      const readiness = await request("GET", `${handle.baseUrl}/health/ready`);
      const queue = await request("GET", `${handle.baseUrl}/health/queue`);

      expect(readiness.status).toBe(503);
      expect(readiness.body).toMatchObject({ ready: false, queue: { healthy: false } });
      expect(queue.status).toBe(503);
      expect(queue.body).toMatchObject({ healthy: false, queueDepth: 64, queueCapacity: 64 });
    });

    it("?deep=1 routes through the registered civil3d_health handler", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_health");
      executeRegisteredToolMock.mockResolvedValue({ running: true, pluginRunning: true });
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/health?deep=1`);

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ running: true, pluginRunning: true });
      expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_health", {});
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it("?deep=1 reports 503 when the plugin probe fails", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_health");
      executeRegisteredToolMock.mockRejectedValue(new Error("Plugin not running"));
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/health?deep=1`);

      expect(res.status).toBe(503);
      expect(res.body).toEqual({
        connected: false,
        error: { code: "CIVIL3D.UNAVAILABLE", message: "Plugin not running" },
      });
    });
  });

  describe("GET /tools", () => {
    it("lists every registered tool name", async () => {
      listRegisteredToolNamesMock.mockReturnValue(["civil3d_alignment", "civil3d_surface"]);
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/tools`);

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ count: 2, tools: ["civil3d_alignment", "civil3d_surface"] });
    });
  });

  describe("POST /execute", () => {
    it("routes registered tools through their authoritative MCP handler", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_alignment");
      executeRegisteredToolMock.mockResolvedValue({ alignments: [{ name: "Mainline" }] });
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "civil3d_alignment", parameters: { action: "list" } }),
      });

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ alignments: [{ name: "Mainline" }] });
      expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_alignment", { action: "list" });
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it("routes civil3d_orchestrate calls through executeRegisteredTool directly", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_orchestrate");
      executeRegisteredToolMock.mockResolvedValue({ selectedTool: "civil3d_alignment" });
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "civil3d_orchestrate", parameters: { request: "list alignments" } }),
      });

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ selectedTool: "civil3d_alignment" });
      expect(executeRegisteredToolMock).toHaveBeenCalledWith("civil3d_orchestrate", { request: "list alignments" });
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it.each(["civil3d_preview_action", "civil3d_request_approval", "civil3d_request_plan_approval"])(
      "routes the %s broker through its registered handler directly",
      async (toolName) => {
        hasToolHandlerMock.mockImplementation((name) => name === toolName);
        executeRegisteredToolMock.mockResolvedValue({ approvalToken: "approval-1" });
        handle = await launchBridge();
        const parameters = {
          toolName: "civil3d_drawing",
          action: "new",
          parameters: { action: "new" },
        };

        const res = await request("POST", `${handle.baseUrl}/execute`, {
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ tool: toolName, parameters }),
        });

        expect(res.status).toBe(200);
        expect(executeRegisteredToolMock).toHaveBeenCalledWith(toolName, parameters);
        expect(executeOrchestratorMock).not.toHaveBeenCalled();
      },
    );

    it("returns 400 when the body is missing a tool name", async () => {
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ parameters: { foo: "bar" } }),
      });

      expect(res.status).toBe(400);
      expect(res.body).toEqual({
        error: {
          code: "CIVIL3D.INVALID_INPUT",
          message: expect.stringContaining("tool"),
        },
      });
    });

    it("returns 400 when the body is not valid JSON", async () => {
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: "{not json",
      });

      expect(res.status).toBe(400);
      expect(res.body).toEqual({
        error: {
          code: "CIVIL3D.INVALID_JSON",
          message: expect.stringMatching(/Invalid JSON body/i),
        },
      });
    });

    it("maps a not-found handler failure to 404", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_alignment");
      executeRegisteredToolMock.mockRejectedValue(new Error("Alignment 'Foo' not found"));
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "civil3d_alignment", parameters: { action: "get", name: "Foo" } }),
      });

      expect(res.status).toBe(404);
      expect(res.body).toEqual({
        error: { code: "CIVIL3D.OBJECT_NOT_FOUND", message: "Alignment 'Foo' not found" },
      });
    });

    it("maps missing required fields to a validation response", async () => {
      hasToolHandlerMock.mockReturnValue(true);
      executeRegisteredToolMock.mockRejectedValue(new Error("Missing required fields: name"));
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "civil3d_surface", parameters: { action: "get" } }),
      });

      expect(res.status).toBe(400);
      expect(res.body).toEqual({
        error: { code: "CIVIL3D.INVALID_INPUT", message: "Missing required fields: name" },
      });
    });

    it("rejects bodies that exceed maxBodyBytes with 413", async () => {
      hasToolHandlerMock.mockReturnValue(true);
      executeOrchestratorMock.mockResolvedValue({ ok: true });
      handle = await launchBridge({ maxBodyBytes: 64 });

      const oversizedPayload = JSON.stringify({
        tool: "civil3d_alignment",
        parameters: { giant: "x".repeat(200) },
      });

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: oversizedPayload,
      });

      expect(res.status).toBe(413);
      expect(res.body).toEqual({
        error: {
          code: "CIVIL3D.REQUEST_TOO_LARGE",
          message: expect.stringContaining("exceeds 64 bytes"),
        },
      });
      expect(executeOrchestratorMock).not.toHaveBeenCalled();
    });

    it("returns 404 when the tool is neither registered nor legacy", async () => {
      hasToolHandlerMock.mockReturnValue(false);
      listRegisteredToolNamesMock.mockReturnValue([]);
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "does_not_exist", parameters: {} }),
      });

      expect(res.status).toBe(404);
      expect(res.body).toEqual({
        error: {
          code: "CIVIL3D.METHOD_NOT_FOUND",
          message: expect.stringContaining("not registered"),
        },
      });
    });

    it("maps Civil 3D domain errors to stable HTTP status and error codes", async () => {
      hasToolHandlerMock.mockReturnValue(true);
      executeRegisteredToolMock.mockRejectedValue(
        new Civil3DRpcError("Surface 'Missing' was not found", "CIVIL3D.OBJECT_NOT_FOUND", -32004),
      );
      handle = await launchBridge();

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tool: "civil3d_surface", parameters: { action: "get", name: "Missing" } }),
      });

      expect(res.status).toBe(404);
      expect(res.body).toEqual({
        error: { code: "CIVIL3D.OBJECT_NOT_FOUND", message: "Surface 'Missing' was not found" },
      });
    });

    it("aborts downstream work when the HTTP caller disconnects", async () => {
      hasToolHandlerMock.mockReturnValue(true);
      let observeAbort!: () => void;
      const aborted = new Promise<void>((resolve) => { observeAbort = resolve; });
      executeRegisteredToolMock.mockImplementation(async () => await new Promise((_, reject) => {
        const signal = currentAbortSignal();
        expect(signal).toBeDefined();
        signal!.addEventListener("abort", () => {
          observeAbort();
          reject(new Civil3DRpcError("Caller disconnected", "CIVIL3D.CANCELLED", -32010));
        }, { once: true });
      }));
      handle = await launchBridge();
      const controller = new AbortController();
      const pending = fetch(`${handle.baseUrl}/execute`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Connection: "close" },
        body: JSON.stringify({ tool: "civil3d_health", parameters: {} }),
        signal: controller.signal,
      });
      setTimeout(() => controller.abort(), 25);

      await expect(pending).rejects.toThrow();
      await expect(aborted).resolves.toBeUndefined();
    });
  });

  describe("authorization", () => {
    it("refuses a non-loopback bind without authentication", () => {
      expect(() => startHttpBridge({ host: "0.0.0.0", port: 0, authToken: "" }))
        .toThrow(/MCP_HTTP_TOKEN is required/i);
    });

    it("refuses a non-loopback bind without an explicit Host allowlist", () => {
      expect(() => startHttpBridge({
        host: "0.0.0.0",
        port: 0,
        authToken: "secret-token",
        allowedHosts: [],
      })).toThrow(/MCP_HTTP_ALLOWED_HOSTS is required/i);
    });

    it("accepts requests with the correct Authorization bearer token", async () => {
      hasToolHandlerMock.mockImplementation((name) => name === "civil3d_orchestrate");
      executeRegisteredToolMock.mockResolvedValue({ ok: true });
      handle = await launchBridge({ authToken: "secret-token" });

      const res = await request("POST", `${handle.baseUrl}/execute`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer secret-token",
        },
        body: JSON.stringify({ tool: "civil3d_orchestrate", parameters: {} }),
      });

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ ok: true });
    });

    it("accepts the X-MCP-Token header as an alternative to Authorization", async () => {
      listRegisteredToolNamesMock.mockReturnValue(["civil3d_alignment"]);
      handle = await launchBridge({ authToken: "secret-token" });

      const res = await request("GET", `${handle.baseUrl}/tools`, {
        headers: { "X-MCP-Token": "secret-token" },
      });

      expect(res.status).toBe(200);
      expect(res.body).toEqual({ count: 1, tools: ["civil3d_alignment"] });
    });

    it("rejects requests missing the token with 401", async () => {
      handle = await launchBridge({ authToken: "secret-token" });

      const res = await request("GET", `${handle.baseUrl}/tools`);

      expect(res.status).toBe(401);
      expect(res.body).toEqual({ error: { code: "CIVIL3D.AUTH_REQUIRED", message: "Unauthorized" } });
    });

    it("rejects requests with an incorrect token with 401", async () => {
      handle = await launchBridge({ authToken: "secret-token" });

      const res = await request("GET", `${handle.baseUrl}/tools`, {
        headers: { Authorization: "Bearer wrong-token" },
      });

      expect(res.status).toBe(401);
      expect(res.body).toEqual({ error: { code: "CIVIL3D.AUTH_REQUIRED", message: "Unauthorized" } });
    });

    it("does not enforce auth when authToken is empty (default dev mode)", async () => {
      listRegisteredToolNamesMock.mockReturnValue([]);
      handle = await launchBridge({ authToken: "" });

      const res = await request("GET", `${handle.baseUrl}/tools`);

      expect(res.status).toBe(200);
    });
  });

  describe("CORS preflight", () => {
    it("responds to OPTIONS with 204 and the expected CORS headers", async () => {
      handle = await launchBridge();

      const response = await fetch(`${handle.baseUrl}/execute`, {
        method: "OPTIONS",
      });

      expect(response.status).toBe(204);
      expect(response.headers.get("access-control-allow-methods")).toContain("POST");
      expect(response.headers.get("access-control-allow-headers") ?? "").toContain("Authorization");
    });

    it("rejects browser origins that are not allowlisted", async () => {
      handle = await launchBridge({ allowedOrigins: ["https://trusted.example"] });

      const response = await fetch(`${handle.baseUrl}/tools`, {
        headers: { Origin: "https://untrusted.example" },
      });

      expect(response.status).toBe(403);
      expect(response.headers.get("access-control-allow-origin")).toBeNull();
    });

    it("echoes an explicitly allowlisted browser origin", async () => {
      handle = await launchBridge({ allowedOrigins: ["https://trusted.example"] });

      const response = await fetch(`${handle.baseUrl}/tools`, {
        headers: { Origin: "https://trusted.example" },
      });

      expect(response.status).toBe(200);
      expect(response.headers.get("access-control-allow-origin"))
        .toBe("https://trusted.example");
    });
  });

  describe("Host validation", () => {
    it("rejects a Host header outside the allowlist", async () => {
      handle = await launchBridge({ allowedHosts: ["trusted.local"] });

      const res = await request("GET", `${handle.baseUrl}/tools`, {
        headers: { Host: "evil.local" },
      });

      expect(res.status).toBe(403);
      expect(res.body).toEqual({ error: { code: "CIVIL3D.FORBIDDEN", message: "Host is not allowed" } });
    });

    it("accepts the explicitly allowlisted binding Host", async () => {
      handle = await launchBridge({ allowedHosts: ["127.0.0.1"] });

      const res = await request("GET", `${handle.baseUrl}/tools`);

      expect(res.status).toBe(200);
    });

    it("rejects wildcard Host and Origin configuration", () => {
      expect(() => startHttpBridge({ port: 0, allowedHosts: ["*"] })).toThrow(/Wildcard/i);
      expect(() => startHttpBridge({ port: 0, allowedOrigins: ["*"] })).toThrow(/Wildcard/i);
    });
  });

  describe("unknown routes", () => {
    it("returns 404 for unknown paths", async () => {
      handle = await launchBridge();

      const res = await request("GET", `${handle.baseUrl}/not-a-real-route`);

      expect(res.status).toBe(404);
      expect(res.body).toEqual({ error: { code: "CIVIL3D.OBJECT_NOT_FOUND", message: "Not found" } });
    });
  });
});
