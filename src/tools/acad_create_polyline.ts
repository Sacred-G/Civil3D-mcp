import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const AcadCreatePolylineInputSchema = z.object({
  points: z.array(Point2DSchema).min(2),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
});

const AcadCreatePolylineResponseSchema = z.object({
  handle: z.string(),
  vertexCount: z.number(),
  closed: z.boolean(),
  layer: z.string(),
});

export function registerAcadCreatePolylineTool(server: McpServer) {
  server.tool(
    "acad_create_polyline",
    "Creates an AutoCAD 2D polyline in model space.",
    AcadCreatePolylineInputSchema.shape,
    async (args, _extra) => {
      try {
        const parsedArgs = AcadCreatePolylineInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createPolyline", {
            points: parsedArgs.points,
            closed: parsedArgs.closed ? 1 : 0,
            layer: parsedArgs.layer,
          });
        });

        const validatedResponse = AcadCreatePolylineResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to create polyline";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in acad_create_polyline tool:", error);
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
