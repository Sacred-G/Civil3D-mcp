import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const CorridorStateSchema = z.enum(["built", "out_of_date", "error"]);

const Civil3DCorridorInputShape = {
  action: z.enum(["list", "get", "rebuild", "get_surfaces", "get_feature_lines", "compute_volumes"]),
  name: z.string().optional(),
  corridorSurface: z.string().optional(),
  referenceSurface: z.string().optional(),
};

const Civil3DCorridorInputSchema = z.object(Civil3DCorridorInputShape);

const CorridorListArgsSchema = Civil3DCorridorInputSchema.extend({
  action: z.literal("list"),
});

const CorridorNamedArgsSchema = Civil3DCorridorInputSchema.extend({
  action: z.enum(["get", "rebuild", "get_surfaces", "get_feature_lines"]),
  name: z.string(),
});

const CorridorComputeVolumesArgsSchema = Civil3DCorridorInputSchema.extend({
  action: z.literal("compute_volumes"),
  name: z.string(),
  corridorSurface: z.string(),
  referenceSurface: z.string(),
});

const CorridorSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  baselineCount: z.number(),
  regionCount: z.number(),
  surfaceCount: z.number(),
  state: CorridorStateSchema,
  lastBuildTime: z.string().nullable(),
});

const CorridorListResponseSchema = z.object({
  corridors: z.array(CorridorSummarySchema),
});

const CorridorRegionSchema = z.object({
  name: z.string(),
  assemblyName: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  frequency: z.number(),
});

const CorridorBaselineSchema = z.object({
  name: z.string(),
  alignmentName: z.string(),
  profileName: z.string(),
  regions: z.array(CorridorRegionSchema),
});

const CorridorSurfaceSchema = z.object({
  name: z.string(),
  boundaries: z.array(z.string()),
});

const CorridorDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  style: z.string(),
  layer: z.string(),
  baselines: z.array(CorridorBaselineSchema),
  surfaces: z.array(CorridorSurfaceSchema),
  featureLineCount: z.number(),
  state: CorridorStateSchema,
});

const CorridorSurfacesResponseSchema = z.object({
  corridorName: z.string().optional(),
  surfaces: z.array(CorridorSurfaceSchema),
});

const CorridorFeatureLineSchema = z.record(z.unknown());

const CorridorFeatureLinesResponseSchema = z.object({
  corridorName: z.string().optional(),
  featureLines: z.array(CorridorFeatureLineSchema),
});

const CorridorRebuildResponseSchema = z.object({
  jobId: z.string(),
}).passthrough();

const CorridorVolumesResponseSchema = z.object({
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

export function registerCivil3DCorridorTool(server: McpServer) {
  server.tool(
    "civil3d_corridor",
    "Reads Civil 3D corridor data and controls corridor rebuild and volume operations.",
    Civil3DCorridorInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? CorridorListArgsSchema.parse(args)
            : args.action === "compute_volumes"
              ? CorridorComputeVolumesArgsSchema.parse(args)
              : CorridorNamedArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listCorridors", {});
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getCorridor", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "rebuild") {
            return await appClient.sendCommand("rebuildCorridor", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "get_surfaces") {
            return await appClient.sendCommand("getCorridorSurfaces", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "get_feature_lines") {
            return await appClient.sendCommand("getCorridorFeatureLines", {
              name: parsedArgs.name,
            });
          }

          return await appClient.sendCommand("computeCorridorVolumes", {
            name: parsedArgs.name,
            corridorSurface: parsedArgs.corridorSurface,
            referenceSurface: parsedArgs.referenceSurface,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? CorridorListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? CorridorDetailResponseSchema.parse(response)
              : parsedArgs.action === "rebuild"
                ? CorridorRebuildResponseSchema.parse(response)
              : parsedArgs.action === "get_surfaces"
                ? CorridorSurfacesResponseSchema.parse(response)
                : parsedArgs.action === "get_feature_lines"
                  ? CorridorFeatureLinesResponseSchema.parse(response)
                  : CorridorVolumesResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_corridor action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_corridor tool:", error);
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
