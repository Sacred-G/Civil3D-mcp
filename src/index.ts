import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { registerTools } from "./tools/register.js";
import { startHttpBridge } from "./httpBridge.js";
import { createLogger } from "./utils/logger.js";
import { stripSchemaDialectOnSend } from "./utils/schemaDialect.js";
import { APP_VERSION } from "./version.js";

const log = createLogger("MCP");

const server = new McpServer({
  name: "civil3d-mcp",
  version: APP_VERSION,
});

async function main() {
  await registerTools(server);
  const httpServer = startHttpBridge();

  const transport = stripSchemaDialectOnSend(new StdioServerTransport());
  await server.connect(transport);
  log.info("Civil 3D MCP Server started");

  let shuttingDown = false;
  const shutdown = async (signal: string) => {
    if (shuttingDown) {
      return;
    }
    shuttingDown = true;
    log.info("Shutting down", { signal });

    try {
      await new Promise<void>((resolve) => {
        httpServer.close(() => resolve());
        // Stop accepting new connections but don't let lingering sockets block exit.
        setTimeout(() => resolve(), 2000).unref();
      });
    } catch (error) {
      log.warn("HTTP bridge shutdown failed", { error: String(error) });
    }

    try {
      await server.close();
    } catch (error) {
      log.warn("MCP server close failed", { error: String(error) });
    }

    process.exit(0);
  };

  process.on("SIGINT", () => void shutdown("SIGINT"));
  process.on("SIGTERM", () => void shutdown("SIGTERM"));
}

main().catch((error) => {
  log.error("Error starting Civil 3D MCP Server", { error: String(error) });
  process.exit(1);
});
