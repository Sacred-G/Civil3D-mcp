import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DHealthResponseSchema = z.object({
  connected: z.boolean(),
  civil3dVersion: z.string().optional(),
  pluginVersion: z.string().optional(),
  drawingLoaded: z.boolean(),
  operationInProgress: z.boolean(),
  currentOperation: z.string().nullable(),
  queueDepth: z.number(),
  memoryUsageMb: z.number(),
});

export function registerCivil3DHealthTool(server: McpServer) {
  server.tool(
    "civil3d_health",
    "Reports the status of the Civil 3D connection and plugin.",
    {},
    async (_args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getCivil3DHealth", {});
        });

        const validatedResponse = Civil3DHealthResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to get Civil 3D health status";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_health tool:", error);
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
