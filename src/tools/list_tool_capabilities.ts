import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { listDomains, listToolCatalog } from "./tool_catalog.js";

const ToolCapabilityListInputShape = {
  domain: z.string().optional(),
  status: z.enum(["implemented", "planned"]).optional(),
};

const ToolCatalogEntrySchema = z.object({
  toolName: z.string(),
  displayName: z.string(),
  description: z.string(),
  domain: z.string(),
  capabilities: z.array(z.string()),
  operations: z.array(z.string()).optional(),
  pluginMethods: z.array(z.string()).optional(),
  requiresActiveDrawing: z.boolean(),
  safeForRetry: z.boolean(),
  status: z.enum(["implemented", "planned"]),
});

const ToolCapabilityListResponseSchema = z.object({
  domains: z.array(z.string()),
  tools: z.array(ToolCatalogEntrySchema),
});

export function registerListToolCapabilitiesTool(server: McpServer) {
  server.tool(
    "list_tool_capabilities",
    "Lists domain and capability metadata for the Civil 3D MCP tool catalog, including planned domains like hydrology.",
    ToolCapabilityListInputShape,
    async (args, _extra) => {
      try {
        const domainFilter = args.domain?.trim().toLowerCase();
        const statusFilter = args.status;

        const tools = listToolCatalog().filter((entry) => {
          const domainMatches = !domainFilter || entry.domain === domainFilter;
          const statusMatches = !statusFilter || entry.status === statusFilter;
          return domainMatches && statusMatches;
        });

        const response = ToolCapabilityListResponseSchema.parse({
          domains: listDomains(),
          tools,
        });

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to list tool capabilities";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in list_tool_capabilities tool:", error);
        return {
          content: [
            {
              type: "text",
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
