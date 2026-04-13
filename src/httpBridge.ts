import { createServer, IncomingMessage, ServerResponse } from "node:http";
import {
  hasToolHandler,
  executeRegisteredTool,
  listRegisteredToolNames,
} from "./tools/toolHandlerRegistry.js";
import { executeToolCallViaOrchestrator } from "./tools/civil3d_orchestrate.js";
import { createLogger } from "./utils/logger.js";

const log = createLogger("HttpBridge");
const HTTP_PORT = parseInt(process.env.MCP_HTTP_PORT ?? "3000", 10);
const HTTP_HOST = process.env.MCP_HTTP_HOST ?? "127.0.0.1";

type ExecuteRequest = {
  tool?: string;
  parameters?: Record<string, unknown>;
};

async function readJsonBody(request: IncomingMessage): Promise<ExecuteRequest> {
  const chunks: Buffer[] = [];

  for await (const chunk of request) {
    chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk));
  }

  const raw = Buffer.concat(chunks).toString("utf8").trim();
  if (!raw) {
    return {};
  }

  return JSON.parse(raw) as ExecuteRequest;
}

function writeJson(response: ServerResponse, statusCode: number, payload: unknown): void {
  response.statusCode = statusCode;
  response.setHeader("Content-Type", "application/json; charset=utf-8");
  response.setHeader("Access-Control-Allow-Origin", "*");
  response.setHeader("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
  response.setHeader("Access-Control-Allow-Headers", "Content-Type");
  response.end(JSON.stringify(payload));
}

/**
 * Legacy convenience endpoints used by the Copilot's McpClient.cs for
 * drawing-context queries. These tool names do NOT correspond to registered
 * MCP tools — they are synthetic shortcuts that map directly to C# plugin commands.
 */
const LEGACY_TOOL_NAMES = new Set([
  "civil3d_list_alignments",
  "civil3d_list_surfaces",
  "civil3d_list_profiles",
  "civil3d_list_assemblies",
  "civil3d_list_corridors",
  "civil3d_alignment_report",
  "civil3d_surface_report",
]);

async function executeLegacyTool(toolName: string, parameters: Record<string, unknown>): Promise<unknown> {
  switch (toolName) {
    case "civil3d_list_alignments":
      return await executeToolCallViaOrchestrator("civil3d_alignment", { action: "list" });
    case "civil3d_list_surfaces":
      return await executeToolCallViaOrchestrator("civil3d_surface", { action: "list" });
    case "civil3d_list_profiles":
      return await executeToolCallViaOrchestrator("civil3d_profile", {
        action: "list",
        alignmentName: parameters.alignmentName,
      });
    case "civil3d_list_assemblies":
      return await executeToolCallViaOrchestrator("civil3d_assembly", { action: "list" });
    case "civil3d_list_corridors":
      return await executeToolCallViaOrchestrator("civil3d_corridor", { action: "list" });
    case "civil3d_alignment_report":
      return await executeToolCallViaOrchestrator("civil3d_alignment_report", {
        alignmentName: parameters.alignmentName,
      });
    case "civil3d_surface_report":
      return await executeToolCallViaOrchestrator("civil3d_surface", {
        action: "get",
        name: parameters.surfaceName,
      });
    default:
      throw new Error(`Unknown legacy tool '${toolName}'.`);
  }
}

/**
 * Execute a tool by name. Resolution order:
 *   1. Registered MCP tool handlers (all 180+ tools)
 *   2. Legacy synthetic endpoints (drawing-context convenience queries)
 *   3. Error
 */
async function executeBridgeTool(toolName: string, parameters: Record<string, unknown>): Promise<unknown> {
  // 1. Check the global tool handler registry (populated during registerTools)
  if (hasToolHandler(toolName)) {
    if (toolName === "civil3d_orchestrate") {
      return await executeRegisteredTool(toolName, parameters);
    }

    return await executeToolCallViaOrchestrator(toolName, parameters);
  }

  // 2. Legacy convenience endpoints for Copilot drawing-context queries
  if (LEGACY_TOOL_NAMES.has(toolName)) {
    return await executeLegacyTool(toolName, parameters);
  }

  // 3. Not found
  const registeredCount = listRegisteredToolNames().length;
  throw new Error(
    `Tool '${toolName}' is not registered (${registeredCount} tools available) ` +
    `and is not a legacy bridge endpoint. Check the tool name.`
  );
}

async function handleHealth(_request: IncomingMessage, response: ServerResponse): Promise<void> {
  try {
    // civil3d_health is a registered MCP tool — route through the main dispatcher
    const result = await executeBridgeTool("civil3d_health", {});
    writeJson(response, 200, result);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    writeJson(response, 503, {
      connected: false,
      error: message,
    });
  }
}

async function handleExecute(request: IncomingMessage, response: ServerResponse): Promise<void> {
  try {
    const body = await readJsonBody(request);
    if (!body.tool || typeof body.tool !== "string") {
      writeJson(response, 400, { error: "Request body must include a string 'tool' property." });
      return;
    }

    const parameters = body.parameters && typeof body.parameters === "object"
      ? body.parameters
      : {};

    const result = await executeBridgeTool(body.tool, parameters);
    writeJson(response, 200, result);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    writeJson(response, 500, { error: message });
  }
}

export function startHttpBridge() {
  const server = createServer(async (request, response) => {
    try {
      const method = request.method ?? "GET";
      const url = request.url ?? "/";

      if (method === "OPTIONS") {
        writeJson(response, 204, {});
        return;
      }

      if (method === "GET" && url === "/health") {
        await handleHealth(request, response);
        return;
      }

      if (method === "GET" && url === "/tools") {
        const tools = listRegisteredToolNames();
        writeJson(response, 200, { count: tools.length, tools });
        return;
      }

      if (method === "POST" && url === "/execute") {
        await handleExecute(request, response);
        return;
      }

      writeJson(response, 404, { error: "Not found" });
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      writeJson(response, 500, { error: message });
    }
  });

  server.listen(HTTP_PORT, HTTP_HOST, () => {
    log.info("HTTP MCP bridge started", { host: HTTP_HOST, port: HTTP_PORT });
  });

  server.on("error", (error) => {
    log.error("HTTP MCP bridge failed", { error: String(error) });
  });

  return server;
}
