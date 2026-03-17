import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DSurfaceComparisonWorkflowInputShape = {
  baseSurface: z.string(),
  comparisonSurface: z.string(),
};

const Civil3DSurfaceComparisonWorkflowInputSchema = z.object(Civil3DSurfaceComparisonWorkflowInputShape);

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

const SurfaceComparisonWorkflowResponseSchema = z.object({
  baseSurface: SurfaceDetailResponseSchema,
  comparisonSurface: SurfaceDetailResponseSchema,
  volumeAnalysis: SurfaceVolumeResponseSchema,
  summary: z.object({
    baseSurfaceName: z.string(),
    comparisonSurfaceName: z.string(),
    elevationDeltaMean: z.number(),
    elevationDeltaMin: z.number(),
    elevationDeltaMax: z.number(),
    area2dDelta: z.number(),
    area3dDelta: z.number(),
    pointCountDelta: z.number(),
    triangleCountDelta: z.number(),
    netVolumeDirection: z.enum(["cut", "fill", "balanced"]),
  }),
});

export function registerCivil3DSurfaceComparisonWorkflowTool(server: McpServer) {
  server.tool(
    "civil3d_surface_comparison_workflow",
    "Builds a structured surface comparison by fetching two surfaces and computing cut/fill volume differences between them.",
    Civil3DSurfaceComparisonWorkflowInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs = Civil3DSurfaceComparisonWorkflowInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          const baseSurface = SurfaceDetailResponseSchema.parse(
            await appClient.sendCommand("getSurface", {
              name: parsedArgs.baseSurface,
            })
          );

          const comparisonSurface = SurfaceDetailResponseSchema.parse(
            await appClient.sendCommand("getSurface", {
              name: parsedArgs.comparisonSurface,
            })
          );

          const volumeAnalysis = SurfaceVolumeResponseSchema.parse(
            await appClient.sendCommand("computeSurfaceVolume", {
              baseSurface: parsedArgs.baseSurface,
              comparisonSurface: parsedArgs.comparisonSurface,
            })
          );

          const netVolumeDirection = volumeAnalysis.netVolume > 0
            ? "fill"
            : volumeAnalysis.netVolume < 0
              ? "cut"
              : "balanced";

          return SurfaceComparisonWorkflowResponseSchema.parse({
            baseSurface,
            comparisonSurface,
            volumeAnalysis,
            summary: {
              baseSurfaceName: baseSurface.name,
              comparisonSurfaceName: comparisonSurface.name,
              elevationDeltaMean: comparisonSurface.statistics.meanElevation - baseSurface.statistics.meanElevation,
              elevationDeltaMin: comparisonSurface.statistics.minimumElevation - baseSurface.statistics.minimumElevation,
              elevationDeltaMax: comparisonSurface.statistics.maximumElevation - baseSurface.statistics.maximumElevation,
              area2dDelta: comparisonSurface.statistics.area2d - baseSurface.statistics.area2d,
              area3dDelta: comparisonSurface.statistics.area3d - baseSurface.statistics.area3d,
              pointCountDelta: comparisonSurface.statistics.numberOfPoints - baseSurface.statistics.numberOfPoints,
              triangleCountDelta: comparisonSurface.statistics.numberOfTriangles - baseSurface.statistics.numberOfTriangles,
              netVolumeDirection,
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
        let errorMessage = "Failed to execute civil3d_surface_comparison_workflow";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_surface_comparison_workflow tool:", error);
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
