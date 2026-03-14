import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DSurfaceDrainageWorkflowInputShape = {
  surfaceName: z.string(),
  x: z.number(),
  y: z.number(),
  stepDistance: z.number().positive().optional(),
  maxSteps: z.number().int().positive().optional(),
  drainageArea: z.number().positive(),
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  areaUnits: z.enum(["acres", "hectares"]),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]),
};

const Civil3DSurfaceDrainageWorkflowInputSchema = z.object(Civil3DSurfaceDrainageWorkflowInputShape);

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

const FlowPathPointSchema = z.object({
  x: z.number(),
  y: z.number(),
  elevation: z.number(),
});

const HydrologyFlowPathResponseSchema = z.object({
  surfaceName: z.string(),
  status: z.enum(["complete", "stopped_flat", "stopped_local_minimum", "max_steps_reached"]),
  stepDistance: z.number(),
  stepCount: z.number(),
  totalDistance: z.number(),
  dropElevation: z.number(),
  startPoint: FlowPathPointSchema,
  endPoint: FlowPathPointSchema,
  points: z.array(FlowPathPointSchema),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
});

const SurfaceElevationAlongResponseSchema = z.object({
  surfaceName: z.string(),
  samples: z.array(FlowPathPointSchema),
  units: z.string(),
});

const HydrologyRunoffResponseSchema = z.object({
  method: z.literal("rational"),
  drainageArea: z.number(),
  runoffCoefficient: z.number(),
  rainfallIntensity: z.number(),
  areaUnits: z.enum(["acres", "hectares"]),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]),
  runoffRate: z.object({
    cfs: z.number(),
    cubicMetersPerSecond: z.number(),
  }),
  normalizedInputs: z.object({
    drainageAreaAcres: z.number(),
    drainageAreaHectares: z.number(),
    rainfallIntensityInPerHr: z.number(),
    rainfallIntensityMmPerHr: z.number(),
  }),
});

const DrainageElevationProfileSchema = z.object({
  surfaceName: z.string(),
  sampleCount: z.number(),
  startElevation: z.number(),
  endElevation: z.number(),
  minimumElevation: z.number(),
  maximumElevation: z.number(),
  totalDrop: z.number(),
  units: z.string(),
  samples: z.array(FlowPathPointSchema),
});

const SurfaceDrainageWorkflowResponseSchema = z.object({
  surface: SurfaceDetailResponseSchema,
  flowPath: HydrologyFlowPathResponseSchema,
  elevationProfile: DrainageElevationProfileSchema,
  runoffEstimate: HydrologyRunoffResponseSchema,
});

export function registerCivil3DSurfaceDrainageWorkflowTool(server: McpServer) {
  server.tool(
    "civil3d_surface_drainage_workflow",
    "Runs a surface drainage workflow by fetching a surface, tracing a flow path, sampling elevations along that path, and estimating runoff.",
    Civil3DSurfaceDrainageWorkflowInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs = Civil3DSurfaceDrainageWorkflowInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          const surface = SurfaceDetailResponseSchema.parse(
            await appClient.sendCommand("getSurface", {
              name: parsedArgs.surfaceName,
            })
          );

          const flowPath = HydrologyFlowPathResponseSchema.parse(
            await appClient.sendCommand("traceHydrologyFlowPath", {
              surfaceName: parsedArgs.surfaceName,
              x: parsedArgs.x,
              y: parsedArgs.y,
              stepDistance: parsedArgs.stepDistance,
              maxSteps: parsedArgs.maxSteps,
            })
          );

          const sampledElevations = SurfaceElevationAlongResponseSchema.parse(
            await appClient.sendCommand("getSurfaceElevationsAlong", {
              name: parsedArgs.surfaceName,
              points: flowPath.points.map((point) => ({
                x: point.x,
                y: point.y,
              })),
            })
          );

          const runoffEstimate = HydrologyRunoffResponseSchema.parse(
            await appClient.sendCommand("estimateHydrologyRunoff", {
              drainageArea: parsedArgs.drainageArea,
              runoffCoefficient: parsedArgs.runoffCoefficient,
              rainfallIntensity: parsedArgs.rainfallIntensity,
              areaUnits: parsedArgs.areaUnits,
              intensityUnits: parsedArgs.intensityUnits,
            })
          );

          const elevations = sampledElevations.samples.map((sample) => sample.elevation);
          const startElevation = elevations[0] ?? 0;
          const endElevation = elevations[elevations.length - 1] ?? 0;

          return SurfaceDrainageWorkflowResponseSchema.parse({
            surface,
            flowPath,
            elevationProfile: {
              surfaceName: sampledElevations.surfaceName,
              sampleCount: sampledElevations.samples.length,
              startElevation,
              endElevation,
              minimumElevation: Math.min(...elevations),
              maximumElevation: Math.max(...elevations),
              totalDrop: startElevation - endElevation,
              units: sampledElevations.units,
              samples: sampledElevations.samples,
            },
            runoffEstimate,
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
        let errorMessage = "Failed to execute civil3d_surface_drainage_workflow";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_surface_drainage_workflow tool:", error);
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
