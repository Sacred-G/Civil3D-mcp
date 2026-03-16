import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DPipeCatalogInputShape = {
  partsList: z.string().optional(),
};

const PipePartsListSchema = z.object({
  name: z.string().nullable(),
  handle: z.string().nullable(),
  parts: z.array(z.string()),
});

const PipeCatalogResponseSchema = z.object({
  partsLists: z.array(PipePartsListSchema),
});

export function registerCivil3DPipeCatalogTool(server: McpServer) {
  server.tool(
    "civil3d_pipe_catalog",
    "Lists available Civil 3D pipe parts lists and part names to help choose valid inputs for pipe network creation and editing tools.",
    Civil3DPipeCatalogInputShape,
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listPipePartsCatalog", {
            partsList: args.partsList,
          });
        });

        const validatedResponse = PipeCatalogResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to list Civil 3D pipe parts catalog";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_pipe_catalog tool:", error);
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
