import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

const Civil3DSurfaceEditInputShape = {
  action: z.enum(["add_points", "add_breakline", "add_boundary", "extract_contours", "compute_volume"]),
  name: z.string().optional(),
  points: z.array(z.union([Point3DSchema, Point2DSchema])).optional(),
  description: z.string().optional(),
  breaklineType: z.enum(["standard", "wall", "proximity"]).optional(),
  boundaryType: z.enum(["show", "hide", "outer", "data_clip"]).optional(),
  minorInterval: z.number().optional(),
  majorInterval: z.number().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
};

const Civil3DSurfaceEditInputSchema = z.object(Civil3DSurfaceEditInputShape);

const SurfaceAddPointsArgsSchema = Civil3DSurfaceEditInputSchema.extend({
  action: z.literal("add_points"),
  name: z.string(),
  points: z.array(Point3DSchema),
  description: z.string().optional(),
});

const SurfaceAddBreaklineArgsSchema = Civil3DSurfaceEditInputSchema.extend({
  action: z.literal("add_breakline"),
  name: z.string(),
  breaklineType: z.enum(["standard", "wall", "proximity"]),
  points: z.array(Point3DSchema),
  description: z.string().optional(),
});

const SurfaceAddBoundaryArgsSchema = Civil3DSurfaceEditInputSchema.extend({
  action: z.literal("add_boundary"),
  name: z.string(),
  boundaryType: z.enum(["show", "hide", "outer", "data_clip"]),
  points: z.array(Point2DSchema),
});

const SurfaceExtractContoursArgsSchema = Civil3DSurfaceEditInputSchema.extend({
  action: z.literal("extract_contours"),
  name: z.string(),
  minorInterval: z.number(),
  majorInterval: z.number(),
});

const SurfaceComputeVolumeArgsSchema = Civil3DSurfaceEditInputSchema.extend({
  action: z.literal("compute_volume"),
  baseSurface: z.string(),
  comparisonSurface: z.string(),
});

const GenericSurfaceEditResponseSchema = z.object({}).passthrough();

const SurfaceVolumeResponseSchema = z.object({
  cutVolume: z.number(),
  fillVolume: z.number(),
  netVolume: z.number(),
  cutArea: z.number(),
  fillArea: z.number(),
  units: z.object({
    volume: z.string(),
    area: z.string(),
  }),
});

export function registerCivil3DSurfaceEditTool(server: McpServer) {
  server.tool(
    "civil3d_surface_edit",
    "Modifies Civil 3D surface data by adding points, breaklines, boundaries, extracting contours, and computing volumes.",
    Civil3DSurfaceEditInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "add_points"
            ? SurfaceAddPointsArgsSchema.parse(args)
            : args.action === "add_breakline"
              ? SurfaceAddBreaklineArgsSchema.parse(args)
              : args.action === "add_boundary"
                ? SurfaceAddBoundaryArgsSchema.parse(args)
                : args.action === "extract_contours"
                  ? SurfaceExtractContoursArgsSchema.parse(args)
                  : SurfaceComputeVolumeArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "add_points") {
            return await appClient.sendCommand("addSurfacePoints", {
              name: parsedArgs.name,
              points: parsedArgs.points,
              description: parsedArgs.description,
            });
          }

          if (parsedArgs.action === "add_breakline") {
            return await appClient.sendCommand("addSurfaceBreakline", {
              name: parsedArgs.name,
              breaklineType: parsedArgs.breaklineType,
              points: parsedArgs.points,
              description: parsedArgs.description,
            });
          }

          if (parsedArgs.action === "add_boundary") {
            return await appClient.sendCommand("addSurfaceBoundary", {
              name: parsedArgs.name,
              boundaryType: parsedArgs.boundaryType,
              points: parsedArgs.points,
            });
          }

          if (parsedArgs.action === "extract_contours") {
            return await appClient.sendCommand("extractSurfaceContours", {
              name: parsedArgs.name,
              minorInterval: parsedArgs.minorInterval,
              majorInterval: parsedArgs.majorInterval,
            });
          }

          return await appClient.sendCommand("computeSurfaceVolume", {
            baseSurface: parsedArgs.baseSurface,
            comparisonSurface: parsedArgs.comparisonSurface,
          });
        });

        const validatedResponse =
          parsedArgs.action === "compute_volume"
            ? SurfaceVolumeResponseSchema.parse(response)
            : GenericSurfaceEditResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_surface_edit action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_surface_edit tool:", error);
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
