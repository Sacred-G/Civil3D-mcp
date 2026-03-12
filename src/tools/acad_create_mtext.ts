import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const AcadCreateMTextInputSchema = z.object({
  text: z.string().min(1),
  x: z.number(),
  y: z.number(),
  z: z.number().optional(),
  width: z.number().positive().optional(),
  textHeight: z.number().positive().optional(),
  rotation: z.number().optional(),
  layer: z.string().optional(),
});

const AcadCreateMTextResponseSchema = z.object({
  handle: z.string(),
  text: z.string(),
  x: z.number(),
  y: z.number(),
  z: z.number(),
  textHeight: z.number(),
  rotation: z.number(),
  width: z.number(),
  layer: z.string(),
});

export function registerAcadCreateMTextTool(server: McpServer) {
  server.tool(
    "acad_create_mtext",
    "Creates AutoCAD MText in model space.",
    AcadCreateMTextInputSchema.shape,
    async (args, _extra) => {
      try {
        const parsedArgs = AcadCreateMTextInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createMText", {
            text: parsedArgs.text,
            x: parsedArgs.x,
            y: parsedArgs.y,
            z: parsedArgs.z,
            width: parsedArgs.width,
            textHeight: parsedArgs.textHeight,
            rotation: parsedArgs.rotation,
            layer: parsedArgs.layer,
          });
        });

        const validatedResponse = AcadCreateMTextResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to create MText";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in acad_create_mtext tool:", error);
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
