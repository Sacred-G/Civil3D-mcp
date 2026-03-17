import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DHydrologyInputShape = {
  action: z.enum(["list_capabilities", "trace_flow_path", "find_low_point", "estimate_runoff", "delineate_watershed", "calculate_catchment_area"]),
  surfaceName: z.string().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  outletX: z.number().optional(),
  outletY: z.number().optional(),
  stepDistance: z.number().optional(),
  maxSteps: z.number().int().optional(),
  sampleSpacing: z.number().optional(),
  gridSpacing: z.number().optional(),
  searchRadius: z.number().optional(),
  maxDistance: z.number().optional(),
  drainageArea: z.number().optional(),
  runoffCoefficient: z.number().optional(),
  rainfallIntensity: z.number().optional(),
  areaUnits: z.enum(["acres", "hectares"]).optional(),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]).optional(),
};

const Civil3DHydrologyInputSchema = z.object(Civil3DHydrologyInputShape);

const HydrologyListCapabilitiesArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("list_capabilities"),
});

const HydrologyTraceFlowPathArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("trace_flow_path"),
  surfaceName: z.string(),
  x: z.number(),
  y: z.number(),
  stepDistance: z.number().positive().optional(),
  maxSteps: z.number().int().positive().optional(),
});

const HydrologyFindLowPointArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("find_low_point"),
  surfaceName: z.string(),
  sampleSpacing: z.number().positive().optional(),
});

const HydrologyEstimateRunoffArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("estimate_runoff"),
  drainageArea: z.number().positive(),
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  areaUnits: z.enum(["acres", "hectares"]),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]),
});

const HydrologyDelineateWatershedArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("delineate_watershed"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  gridSpacing: z.number().positive().optional(),
  searchRadius: z.number().positive().optional(),
});

const HydrologyCalculateCatchmentAreaArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("calculate_catchment_area"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  sampleSpacing: z.number().positive().optional(),
  maxDistance: z.number().positive().optional(),
});

const HydrologyCapabilitiesResponseSchema = z.object({
  domain: z.literal("hydrology"),
  operations: z.array(z.object({
    name: z.string(),
    status: z.enum(["implemented", "planned"]),
    description: z.string(),
  })),
  notes: z.array(z.string()),
});

const HydrologyFlowPathResponseSchema = z.object({
  surfaceName: z.string(),
  status: z.enum(["complete", "stopped_flat", "stopped_local_minimum", "max_steps_reached"]),
  stepDistance: z.number(),
  stepCount: z.number(),
  totalDistance: z.number(),
  dropElevation: z.number(),
  startPoint: z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  }),
  endPoint: z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  }),
  points: z.array(z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  })),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
});

const HydrologyLowPointResponseSchema = z.object({
  surfaceName: z.string(),
  sampleSpacing: z.number(),
  sampledPointCount: z.number(),
  lowPoint: z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  }),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
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

const HydrologyWatershedResponseSchema = z.object({
  surfaceName: z.string(),
  outletPoint: z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  }),
  gridSpacing: z.number(),
  searchRadius: z.number(),
  contributingPointCount: z.number(),
  boundaryPoints: z.array(z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  })),
  approximateArea: z.number(),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
    area: z.string(),
  }),
});

const HydrologyCatchmentAreaResponseSchema = z.object({
  surfaceName: z.string(),
  outletPoint: z.object({
    x: z.number(),
    y: z.number(),
    elevation: z.number(),
  }),
  sampleSpacing: z.number(),
  maxDistance: z.number(),
  contributingCellCount: z.number(),
  catchmentArea: z.number(),
  elevationStatistics: z.object({
    minimum: z.number(),
    maximum: z.number(),
    average: z.number(),
    relief: z.number(),
  }),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
    area: z.string(),
  }),
});

export function registerCivil3DHydrologyTool(server: McpServer) {
  server.tool(
    "civil3d_hydrology",
    "Provides hydrology-oriented analysis helpers, including capability discovery and surface-based flow path tracing.",
    Civil3DHydrologyInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list_capabilities"
            ? HydrologyListCapabilitiesArgsSchema.parse(args)
            : args.action === "trace_flow_path"
              ? HydrologyTraceFlowPathArgsSchema.parse(args)
              : args.action === "find_low_point"
                ? HydrologyFindLowPointArgsSchema.parse(args)
                : args.action === "estimate_runoff"
                  ? HydrologyEstimateRunoffArgsSchema.parse(args)
                  : args.action === "delineate_watershed"
                    ? HydrologyDelineateWatershedArgsSchema.parse(args)
                    : HydrologyCalculateCatchmentAreaArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list_capabilities") {
            return await appClient.sendCommand("listHydrologyCapabilities", {});
          }

          if (parsedArgs.action === "trace_flow_path") {
            return await appClient.sendCommand("traceHydrologyFlowPath", {
              surfaceName: parsedArgs.surfaceName,
              x: parsedArgs.x,
              y: parsedArgs.y,
              stepDistance: parsedArgs.stepDistance,
              maxSteps: parsedArgs.maxSteps,
            });
          }

          if (parsedArgs.action === "find_low_point") {
            return await appClient.sendCommand("findHydrologyLowPoint", {
              surfaceName: parsedArgs.surfaceName,
              sampleSpacing: parsedArgs.sampleSpacing,
            });
          }

          if (parsedArgs.action === "estimate_runoff") {
            return await appClient.sendCommand("estimateHydrologyRunoff", {
              drainageArea: parsedArgs.drainageArea,
              runoffCoefficient: parsedArgs.runoffCoefficient,
              rainfallIntensity: parsedArgs.rainfallIntensity,
              areaUnits: parsedArgs.areaUnits,
              intensityUnits: parsedArgs.intensityUnits,
            });
          }

          if (parsedArgs.action === "delineate_watershed") {
            return await appClient.sendCommand("delineateWatershed", {
              surfaceName: parsedArgs.surfaceName,
              outletX: parsedArgs.outletX,
              outletY: parsedArgs.outletY,
              gridSpacing: parsedArgs.gridSpacing,
              searchRadius: parsedArgs.searchRadius,
            });
          }

          return await appClient.sendCommand("calculateCatchmentArea", {
            surfaceName: parsedArgs.surfaceName,
            outletX: parsedArgs.outletX,
            outletY: parsedArgs.outletY,
            sampleSpacing: parsedArgs.sampleSpacing,
            maxDistance: parsedArgs.maxDistance,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list_capabilities"
            ? HydrologyCapabilitiesResponseSchema.parse(response)
            : parsedArgs.action === "trace_flow_path"
              ? HydrologyFlowPathResponseSchema.parse(response)
              : parsedArgs.action === "find_low_point"
                ? HydrologyLowPointResponseSchema.parse(response)
                : parsedArgs.action === "estimate_runoff"
                  ? HydrologyRunoffResponseSchema.parse(response)
                  : parsedArgs.action === "delineate_watershed"
                    ? HydrologyWatershedResponseSchema.parse(response)
                    : HydrologyCatchmentAreaResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_hydrology action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_hydrology tool:", error);
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
