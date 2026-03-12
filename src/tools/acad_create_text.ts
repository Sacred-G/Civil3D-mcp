import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const AcadCreateTextInputSchema = z.object({
  text: z.string().min(1),
  x: z.number(),
  y: z.number(),
  z: z.number().optional(),
  height: z.number().positive().optional(),
  rotation: z.number().optional(),
  layer: z.string().optional(),
});

const AcadCreateTextResponseSchema = z.object({
  handle: z.string(),
  text: z.string(),
  x: z.number(),
  y: z.number(),
  z: z.number(),
  height: z.number(),
  rotation: z.number(),
  layer: z.string(),
});

export function registerAcadCreateTextTool(server: McpServer) {
  server.tool(
    "acad_create_text",
    "Creates AutoCAD DBText in model space.",
    AcadCreateTextInputSchema.shape,
    async (args, _extra) => {
      try {
        const parsedArgs = AcadCreateTextInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createText", {
            text: parsedArgs.text,
            x: parsedArgs.x,
            y: parsedArgs.y,
            z: parsedArgs.z,
            height: parsedArgs.height,
            rotation: parsedArgs.rotation,
            layer: parsedArgs.layer,
          });
        });

        const validatedResponse = AcadCreateTextResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to create text";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in acad_create_text tool:", error);
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
