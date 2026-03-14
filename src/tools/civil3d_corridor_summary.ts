import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DCorridorSummaryInputShape = {
  name: z.string(),
  corridorSurface: z.string().optional(),
  referenceSurface: z.string().optional(),
};

const Civil3DCorridorSummaryInputSchema = z.object(Civil3DCorridorSummaryInputShape);

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
  state: z.enum(["built", "out_of_date", "error"]),
});

const CorridorSurfacesResponseSchema = z.object({
  corridorName: z.string().optional(),
  surfaces: z.array(CorridorSurfaceSchema),
});

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

const CorridorSummaryResponseSchema = z.object({
  corridor: CorridorDetailResponseSchema,
  surfaceInventory: CorridorSurfacesResponseSchema,
  volumeAnalysis: CorridorVolumesResponseSchema.nullable(),
  summary: z.object({
    baselineCount: z.number(),
    regionCount: z.number(),
    corridorSurfaceCount: z.number(),
    featureLineCount: z.number(),
    state: z.enum(["built", "out_of_date", "error"]),
    totalRegionLength: z.number(),
    selectedCorridorSurface: z.string().nullable(),
    referenceSurface: z.string().nullable(),
    volumeComputationStatus: z.enum(["computed", "skipped"]),
  }),
});

export function registerCivil3DCorridorSummaryTool(server: McpServer) {
  server.tool(
    "civil3d_corridor_summary",
    "Builds a corridor summary by fetching corridor details, corridor surfaces, and optional volume analysis against a reference surface.",
    Civil3DCorridorSummaryInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs = Civil3DCorridorSummaryInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          const corridor = CorridorDetailResponseSchema.parse(
            await appClient.sendCommand("getCorridor", {
              name: parsedArgs.name,
            })
          );

          const surfaceInventory = CorridorSurfacesResponseSchema.parse(
            await appClient.sendCommand("getCorridorSurfaces", {
              name: parsedArgs.name,
            })
          );

          const selectedCorridorSurface = parsedArgs.corridorSurface
            ?? (surfaceInventory.surfaces.length === 1 ? surfaceInventory.surfaces[0].name : null);
          const referenceSurface = parsedArgs.referenceSurface ?? null;

          let volumeAnalysis: z.infer<typeof CorridorVolumesResponseSchema> | null = null;
          if (selectedCorridorSurface && referenceSurface) {
            volumeAnalysis = CorridorVolumesResponseSchema.parse(
              await appClient.sendCommand("computeCorridorVolumes", {
                name: parsedArgs.name,
                corridorSurface: selectedCorridorSurface,
                referenceSurface,
              })
            );
          }

          const regionCount = corridor.baselines.reduce(
            (total, baseline) => total + baseline.regions.length,
            0
          );

          const totalRegionLength = corridor.baselines.reduce(
            (baselineTotal, baseline) => baselineTotal + baseline.regions.reduce(
              (regionTotal, region) => regionTotal + (region.endStation - region.startStation),
              0
            ),
            0
          );

          return CorridorSummaryResponseSchema.parse({
            corridor,
            surfaceInventory,
            volumeAnalysis,
            summary: {
              baselineCount: corridor.baselines.length,
              regionCount,
              corridorSurfaceCount: surfaceInventory.surfaces.length,
              featureLineCount: corridor.featureLineCount,
              state: corridor.state,
              totalRegionLength,
              selectedCorridorSurface,
              referenceSurface,
              volumeComputationStatus: volumeAnalysis ? "computed" : "skipped",
            },
          });
        });

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to execute civil3d_corridor_summary";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_corridor_summary tool:", error);
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
