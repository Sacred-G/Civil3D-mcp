import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DCoordinateSystemInputShape = {
  action: z.enum(["info", "transform"]),
  fromSystem: z.enum(["drawing", "geographic"]).optional(),
  toSystem: z.enum(["drawing", "geographic"]).optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  z: z.number().optional(),
};

const Civil3DCoordinateSystemInputSchema = z.object(Civil3DCoordinateSystemInputShape);

const CoordinateSystemInfoArgsSchema = Civil3DCoordinateSystemInputSchema.extend({
  action: z.literal("info"),
});

const CoordinateSystemTransformArgsSchema = Civil3DCoordinateSystemInputSchema.extend({
  action: z.literal("transform"),
  fromSystem: z.enum(["drawing", "geographic"]),
  toSystem: z.enum(["drawing", "geographic"]),
  x: z.number(),
  y: z.number(),
  z: z.number().optional(),
});

const CoordinateSystemInfoResponseSchema = z.object({
  name: z.string().nullable(),
  zone: z.string().nullable(),
  datum: z.string().nullable(),
  projection: z.string().nullable(),
  linearUnits: z.string(),
  centralMeridian: z.number().nullable(),
  falseEasting: z.number().nullable(),
  falseNorthing: z.number().nullable(),
  scaleFactor: z.number().nullable(),
});

const CoordinateSystemTransformResponseSchema = z
  .object({
    x: z.number(),
    y: z.number(),
    z: z.number().optional(),
  })
  .passthrough();

export function registerCivil3DCoordinateSystemTool(server: McpServer) {
  server.tool(
    "civil3d_coordinate_system",
    "Provides coordinate system information and performs coordinate transformations.",
    Civil3DCoordinateSystemInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "info"
            ? CoordinateSystemInfoArgsSchema.parse(args)
            : CoordinateSystemTransformArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "info") {
            return await appClient.sendCommand("getCoordinateSystemInfo", {});
          }

          return await appClient.sendCommand("transformCoordinates", {
            fromSystem: parsedArgs.fromSystem,
            toSystem: parsedArgs.toSystem,
            x: parsedArgs.x,
            y: parsedArgs.y,
            z: parsedArgs.z,
          });
        });

        const validatedResponse =
          parsedArgs.action === "info"
            ? CoordinateSystemInfoResponseSchema.parse(response)
            : CoordinateSystemTransformResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_coordinate_system action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_coordinate_system tool:", error);
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
