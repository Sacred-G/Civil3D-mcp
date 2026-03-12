import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const Civil3DSurfaceInputShape = {
  action: z.enum(["list", "get", "get_elevation", "get_elevation_along", "get_statistics", "create", "delete"]),
  name: z.string().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  points: z.array(Point2DSchema).optional(),
  analysisType: z.enum(["elevations", "slopes", "contours", "watersheds"]).optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  description: z.string().optional(),
};

const Civil3DSurfaceInputSchema = z.object(Civil3DSurfaceInputShape);

const SurfaceListArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("list"),
});

const SurfaceGetArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("get"),
  name: z.string(),
});

const SurfaceGetElevationArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("get_elevation"),
  name: z.string(),
  x: z.number(),
  y: z.number(),
});

const SurfaceGetElevationAlongArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("get_elevation_along"),
  name: z.string(),
  points: z.array(Point2DSchema).min(1),
});

const SurfaceGetStatisticsArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("get_statistics"),
  name: z.string(),
  analysisType: z.enum(["elevations", "slopes", "contours", "watersheds"]).optional(),
});

const SurfaceCreateArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("create"),
  name: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  description: z.string().optional(),
});

const SurfaceDeleteArgsSchema = Civil3DSurfaceInputSchema.extend({
  action: z.literal("delete"),
  name: z.string(),
});

const SurfaceSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["TIN", "Grid", "TINVolume"]),
  isReference: z.boolean(),
  sourcePath: z.string().nullable(),
});

const SurfaceListResponseSchema = z.object({
  surfaces: z.array(SurfaceSummarySchema),
});

const SurfaceDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["TIN", "Grid", "TINVolume"]),
  style: z.string(),
  layer: z.string(),
  statistics: z.object({
    minimumElevation: z.number(),
    maximumElevation: z.number(),
    meanElevation: z.number(),
    area2d: z.number(),
    area3d: z.number(),
    numberOfPoints: z.number(),
    numberOfTriangles: z.number(),
  }),
  boundingBox: z.object({
    minX: z.number(),
    minY: z.number(),
    maxX: z.number(),
    maxY: z.number(),
  }),
  units: z.string(),
  isReference: z.boolean(),
  dependentAlignments: z.array(z.string()),
  dependentCorridors: z.array(z.string()),
});

const SurfaceElevationResponseSchema = z.object({
  elevation: z.number(),
  units: z.string(),
  surfaceName: z.string(),
});

const GenericSurfaceResponseSchema = z.object({}).passthrough();

export function registerCivil3DSurfaceTool(server: McpServer) {
  server.tool(
    "civil3d_surface",
    "Reads Civil 3D surface data, retrieves analysis, and supports create/delete operations.",
    Civil3DSurfaceInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? SurfaceListArgsSchema.parse(args)
            : args.action === "get"
              ? SurfaceGetArgsSchema.parse(args)
              : args.action === "get_elevation"
                ? SurfaceGetElevationArgsSchema.parse(args)
                : args.action === "get_elevation_along"
                  ? SurfaceGetElevationAlongArgsSchema.parse(args)
                  : args.action === "get_statistics"
                    ? SurfaceGetStatisticsArgsSchema.parse(args)
                    : args.action === "create"
                      ? SurfaceCreateArgsSchema.parse(args)
                      : SurfaceDeleteArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listSurfaces", {});
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getSurface", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "get_elevation") {
            return await appClient.sendCommand("getSurfaceElevation", {
              name: parsedArgs.name,
              x: parsedArgs.x,
              y: parsedArgs.y,
            });
          }

          if (parsedArgs.action === "get_elevation_along") {
            return await appClient.sendCommand("getSurfaceElevationsAlong", {
              name: parsedArgs.name,
              points: parsedArgs.points,
            });
          }

          if (parsedArgs.action === "get_statistics") {
            return await appClient.sendCommand("getSurfaceStatistics", {
              name: parsedArgs.name,
              analysisType: parsedArgs.analysisType,
            });
          }

          if (parsedArgs.action === "create") {
            return await appClient.sendCommand("createSurface", {
              name: parsedArgs.name,
              style: parsedArgs.style,
              layer: parsedArgs.layer,
              description: parsedArgs.description,
            });
          }

          return await appClient.sendCommand("deleteSurface", {
            name: parsedArgs.name,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? SurfaceListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? SurfaceDetailResponseSchema.parse(response)
              : parsedArgs.action === "get_elevation"
                ? SurfaceElevationResponseSchema.parse(response)
                : GenericSurfaceResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_surface action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_surface tool:", error);
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
