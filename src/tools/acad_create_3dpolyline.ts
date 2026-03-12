import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

const AcadCreate3dPolylineInputSchema = z.object({
  points: z.array(Point3DSchema).min(2),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
});

const AcadCreate3dPolylineResponseSchema = z.object({
  handle: z.string(),
  vertexCount: z.number(),
  closed: z.boolean(),
  layer: z.string(),
});

export function registerAcadCreate3dPolylineTool(server: McpServer) {
  server.tool(
    "acad_create_3dpolyline",
    "Creates an AutoCAD 3D polyline in model space.",
    AcadCreate3dPolylineInputSchema.shape,
    async (args, _extra) => {
      try {
        const parsedArgs = AcadCreate3dPolylineInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("create3dPolyline", {
            points: parsedArgs.points,
            closed: parsedArgs.closed ? 1 : 0,
            layer: parsedArgs.layer,
          });
        });

        const validatedResponse = AcadCreate3dPolylineResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to create 3D polyline";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in acad_create_3dpolyline tool:", error);
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
