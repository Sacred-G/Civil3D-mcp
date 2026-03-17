import { createServer, IncomingMessage, ServerResponse } from "node:http";
import { withApplicationConnection } from "./utils/ConnectionManager.js";
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

async function executeLegacyTool(toolName: string, parameters: Record<string, unknown>): Promise<unknown> {
  return await withApplicationConnection(async (appClient) => {
    switch (toolName) {
      case "civil3d_list_alignments":
        return await appClient.sendCommand("listAlignments", {});
      case "civil3d_list_surfaces":
        return await appClient.sendCommand("listSurfaces", {});
      case "civil3d_list_profiles":
        return await appClient.sendCommand("listProfiles", {
          alignmentName: parameters.alignmentName,
        });
      case "civil3d_list_assemblies":
        return await appClient.sendCommand("listAssemblies", {});
      case "civil3d_list_corridors":
        return await appClient.sendCommand("listCorridors", {});
      case "civil3d_alignment_report":
        return await appClient.sendCommand("getAlignment", {
          name: parameters.alignmentName,
        });
      case "civil3d_surface_report":
        return await appClient.sendCommand("getSurface", {
          name: parameters.surfaceName,
        });
      case "civil3d_corridor_summary": {
        const name = parameters.corridorName;
        const corridor = await appClient.sendCommand("getCorridor", { name });
        try {
          const surfaces = await appClient.sendCommand("getCorridorSurfaces", { name });
          return {
            ...corridor,
            surfaces: surfaces?.surfaces ?? corridor?.surfaces ?? [],
          };
        } catch {
          return corridor;
        }
      }
      case "civil3d_health":
        return await appClient.sendCommand("getCivil3DHealth", {});
      default:
        throw new Error(`Unsupported HTTP bridge tool '${toolName}'.`);
    }
  });
}

async function handleHealth(_request: IncomingMessage, response: ServerResponse): Promise<void> {
  try {
    const result = await executeLegacyTool("civil3d_health", {});
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

    const result = await executeLegacyTool(body.tool, parameters);
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
