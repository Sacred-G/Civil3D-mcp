import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const AreaUnitsSchema = z.enum(["acres", "hectares"]);
const IntensityUnitsSchema = z.enum(["in_per_hr", "mm_per_hr"]);
const DetentionMethodSchema = z.enum(["modified_rational", "triangular_hydrograph"]);
const OutletTypeSchema = z.enum(["orifice", "weir", "riser"]);

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

const HydrologyRunoffResponseSchema = z.object({
  method: z.literal("rational"),
  drainageArea: z.number(),
  runoffCoefficient: z.number(),
  rainfallIntensity: z.number(),
  areaUnits: AreaUnitsSchema,
  intensityUnits: IntensityUnitsSchema,
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

const GenericResponseSchema = z.object({}).passthrough();

const WatershedRunoffWorkflowInputShape = {
  surfaceName: z.string().describe("Surface used for watershed delineation and catchment analysis."),
  outletX: z.number().optional().describe("Optional outlet X coordinate. If omitted with outletY, the workflow finds the low point automatically."),
  outletY: z.number().optional().describe("Optional outlet Y coordinate. If omitted with outletX, the workflow finds the low point automatically."),
  findLowPointSampleSpacing: z.number().positive().optional().describe("Sampling spacing when automatically locating the outlet low point. Default: 25."),
  gridSpacing: z.number().positive().optional().describe("Grid spacing for watershed delineation. Default: 10."),
  searchRadius: z.number().positive().optional().describe("Search radius for watershed delineation. Default: 100."),
  catchmentSampleSpacing: z.number().positive().optional().describe("Sample spacing for catchment area calculation. Default: 15."),
  maxDistance: z.number().positive().optional().describe("Maximum distance for catchment area calculation. Default: 200."),
  runoffCoefficient: z.number().min(0).max(1).describe("Runoff coefficient used for Rational Method runoff estimation."),
  rainfallIntensity: z.number().positive().describe("Rainfall intensity used for runoff estimation."),
  intensityUnits: IntensityUnitsSchema.describe("Rainfall intensity units for the runoff calculation."),
  runoffAreaUnits: AreaUnitsSchema.optional().describe("Optional target area units for runoff calculation. Defaults to acres for feet-based drawings and hectares for meter-based drawings."),
};

const WatershedRunoffWorkflowInputSchema = z.object(WatershedRunoffWorkflowInputShape).superRefine((value, ctx) => {
  const hasOutletX = value.outletX != null;
  const hasOutletY = value.outletY != null;
  if (hasOutletX !== hasOutletY) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Provide both outletX and outletY, or omit both to auto-detect the low point.",
      path: hasOutletX ? ["outletY"] : ["outletX"],
    });
  }
});

type WatershedRunoffWorkflowInput = z.infer<typeof WatershedRunoffWorkflowInputSchema>;

const RunoffDetentionWorkflowInputShape = {
  drainageArea: z.number().positive().describe("Drainage area for runoff estimation."),
  areaUnits: AreaUnitsSchema.describe("Area units for the provided drainage area."),
  runoffCoefficient: z.number().min(0).max(1).describe("Runoff coefficient used for Rational Method runoff estimation."),
  rainfallIntensity: z.number().positive().describe("Rainfall intensity used for runoff estimation."),
  intensityUnits: IntensityUnitsSchema.describe("Rainfall intensity units for the runoff calculation."),
  allowableOutflow: z.number().positive().describe("Maximum allowable outflow used for detention sizing."),
  stormDuration: z.number().positive().optional().describe("Design storm duration in minutes. Default: 60."),
  detentionMethod: DetentionMethodSchema.optional().describe("Detention volume method. Defaults to modified_rational."),
  sideSlope: z.number().positive().optional().describe("Interior basin side slope ratio H:V. Default: 3."),
  bottomWidth: z.number().nonnegative().optional().describe("Minimum basin bottom width. Default: 10."),
  freeboardDepth: z.number().nonnegative().optional().describe("Freeboard above design water surface. Default: 1."),
  basinSurfaceName: z.string().optional().describe("Optional basin surface for stage-storage generation."),
  bottomElevation: z.number().optional().describe("Bottom elevation for optional stage-storage generation."),
  topElevation: z.number().optional().describe("Top elevation for optional stage-storage generation."),
  elevationIncrement: z.number().positive().optional().describe("Elevation increment for optional stage-storage generation. Default: 0.5."),
  outletType: OutletTypeSchema.optional().describe("Outlet type for optional stage-storage generation. Default: orifice."),
  outletDiameter: z.number().positive().optional().describe("Outlet diameter for optional stage-storage generation when using orifice or riser."),
  weirLength: z.number().positive().optional().describe("Weir length for optional stage-storage generation when using a weir outlet."),
  dischargeCoefficient: z.number().positive().optional().describe("Outlet discharge coefficient override for optional stage-storage generation."),
};

const RunoffDetentionWorkflowInputSchema = z.object(RunoffDetentionWorkflowInputShape).superRefine((value, ctx) => {
  const stageFieldsProvided = [value.basinSurfaceName, value.bottomElevation, value.topElevation].filter((field) => field != null).length;
  if (stageFieldsProvided > 0 && stageFieldsProvided < 3) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Provide basinSurfaceName, bottomElevation, and topElevation together to generate stage-storage output.",
      path: ["basinSurfaceName"],
    });
  }
});

type RunoffDetentionWorkflowInput = z.infer<typeof RunoffDetentionWorkflowInputSchema>;

const RunoffPipeWorkflowInputShape = {
  networkName: z.string().describe("Pipe network to analyze."),
  drainageArea: z.number().positive().describe("Drainage area for runoff estimation."),
  areaUnits: AreaUnitsSchema.describe("Area units for the provided drainage area."),
  runoffCoefficient: z.number().min(0).max(1).describe("Runoff coefficient used for Rational Method runoff estimation."),
  rainfallIntensity: z.number().positive().describe("Rainfall intensity used for runoff estimation."),
  intensityUnits: IntensityUnitsSchema.describe("Rainfall intensity units for the runoff calculation."),
  designFlowMultiplier: z.number().positive().optional().describe("Optional safety factor applied to the estimated runoff before pipe analysis. Default: 1.0."),
  tailwaterElevation: z.number().optional().describe("Optional tailwater elevation at the outlet structure."),
  manningsN: z.number().positive().optional().describe("Optional Manning's n override. Default: 0.013."),
  minCoverDepth: z.number().nonnegative().optional().describe("Minimum acceptable cover depth. Default: 2.0."),
  minVelocity: z.number().nonnegative().optional().describe("Minimum acceptable velocity. Default: 2.0."),
  maxVelocity: z.number().positive().optional().describe("Maximum acceptable velocity. Default: 10.0."),
  minSlope: z.number().nonnegative().optional().describe("Minimum acceptable slope in percent. Default: 0.5."),
};

const RunoffPipeWorkflowInputSchema = z.object(RunoffPipeWorkflowInputShape);

type RunoffPipeWorkflowInput = z.infer<typeof RunoffPipeWorkflowInputSchema>;

type ResolvedOutlet = {
  x: number;
  y: number;
  elevation: number | null;
  source: "user_provided" | "auto_low_point";
  sampleSpacing: number | null;
  sampledPointCount: number | null;
};

function normalizeHorizontalUnits(units: string) {
  return units.trim().toLowerCase();
}

function convertCatchmentAreaForRunoff(catchmentArea: number, horizontalUnits: string, requestedAreaUnits?: z.infer<typeof AreaUnitsSchema>) {
  const normalizedUnits = normalizeHorizontalUnits(horizontalUnits);
  const targetAreaUnits = requestedAreaUnits ?? (normalizedUnits === "meters" ? "hectares" : normalizedUnits === "feet" ? "acres" : null);

  if (targetAreaUnits == null) {
    throw new Error(`Unsupported drawing linear units '${horizontalUnits}' for automatic runoff area conversion. Use a feet- or meters-based drawing.`);
  }

  if (normalizedUnits === "feet") {
    const drainageAreaAcres = catchmentArea / 43560.0;
    const drainageAreaHectares = catchmentArea * 0.09290304 / 10000.0;
    return {
      horizontalUnits,
      sourceArea: catchmentArea,
      sourceAreaUnits: "square_feet",
      convertedDrainageArea: targetAreaUnits === "acres" ? drainageAreaAcres : drainageAreaHectares,
      convertedAreaUnits: targetAreaUnits,
      drainageAreaAcres,
      drainageAreaHectares,
    };
  }

  if (normalizedUnits === "meters") {
    const drainageAreaAcres = catchmentArea / 4046.8564224;
    const drainageAreaHectares = catchmentArea / 10000.0;
    return {
      horizontalUnits,
      sourceArea: catchmentArea,
      sourceAreaUnits: "square_meters",
      convertedDrainageArea: targetAreaUnits === "acres" ? drainageAreaAcres : drainageAreaHectares,
      convertedAreaUnits: targetAreaUnits,
      drainageAreaAcres,
      drainageAreaHectares,
    };
  }

  throw new Error(`Unsupported drawing linear units '${horizontalUnits}' for automatic runoff area conversion. Use a feet- or meters-based drawing.`);
}

export function registerCivil3DHydrologyWorkflowTools(server: McpServer) {
  server.tool(
    "civil3d_hydrology_watershed_runoff_workflow",
    "Runs a complete watershed-to-runoff workflow by optionally finding the low point outlet, delineating the watershed, calculating catchment area, converting the computed area to acres/hectares, and estimating Rational Method runoff.",
    WatershedRunoffWorkflowInputShape,
    async (args: WatershedRunoffWorkflowInput) => {
      try {
        const parsed = WatershedRunoffWorkflowInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          let resolvedOutlet: ResolvedOutlet;

          if (parsed.outletX != null && parsed.outletY != null) {
            resolvedOutlet = {
              x: parsed.outletX,
              y: parsed.outletY,
              elevation: null,
              source: "user_provided",
              sampleSpacing: null,
              sampledPointCount: null,
            };
          } else {
            const lowPoint = HydrologyLowPointResponseSchema.parse(
              await appClient.sendCommand("findHydrologyLowPoint", {
                surfaceName: parsed.surfaceName,
                sampleSpacing: parsed.findLowPointSampleSpacing ?? 25,
              })
            );

            resolvedOutlet = {
              x: lowPoint.lowPoint.x,
              y: lowPoint.lowPoint.y,
              elevation: lowPoint.lowPoint.elevation,
              source: "auto_low_point",
              sampleSpacing: lowPoint.sampleSpacing,
              sampledPointCount: lowPoint.sampledPointCount,
            };
          }

          const watershed = HydrologyWatershedResponseSchema.parse(
            await appClient.sendCommand("delineateWatershed", {
              surfaceName: parsed.surfaceName,
              outletX: resolvedOutlet.x,
              outletY: resolvedOutlet.y,
              gridSpacing: parsed.gridSpacing ?? 10,
              searchRadius: parsed.searchRadius ?? 100,
            })
          );

          const catchment = HydrologyCatchmentAreaResponseSchema.parse(
            await appClient.sendCommand("calculateCatchmentArea", {
              surfaceName: parsed.surfaceName,
              outletX: resolvedOutlet.x,
              outletY: resolvedOutlet.y,
              sampleSpacing: parsed.catchmentSampleSpacing ?? 15,
              maxDistance: parsed.maxDistance ?? 200,
            })
          );

          const drainageArea = convertCatchmentAreaForRunoff(
            catchment.catchmentArea,
            catchment.units.horizontal,
            parsed.runoffAreaUnits
          );

          const runoff = HydrologyRunoffResponseSchema.parse(
            await appClient.sendCommand("estimateHydrologyRunoff", {
              drainageArea: drainageArea.convertedDrainageArea,
              runoffCoefficient: parsed.runoffCoefficient,
              rainfallIntensity: parsed.rainfallIntensity,
              areaUnits: drainageArea.convertedAreaUnits,
              intensityUnits: parsed.intensityUnits,
            })
          );

          return {
            workflow: "watershed_to_runoff",
            surfaceName: parsed.surfaceName,
            outlet: resolvedOutlet,
            watershed,
            catchment,
            derivedDrainageArea: drainageArea,
            runoffEstimate: runoff,
          };
        });

        return { content: [{ type: "text", text: JSON.stringify(response, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_hydrology_watershed_runoff_workflow", error);
      }
    }
  );

  server.tool(
    "civil3d_hydrology_runoff_detention_workflow",
    "Runs a runoff-to-detention workflow by estimating Rational Method runoff, sizing a detention basin, and optionally generating a stage-storage-discharge table when basin surface elevations are provided.",
    RunoffDetentionWorkflowInputShape,
    async (args: RunoffDetentionWorkflowInput) => {
      try {
        const parsed = RunoffDetentionWorkflowInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          const runoff = HydrologyRunoffResponseSchema.parse(
            await appClient.sendCommand("estimateHydrologyRunoff", {
              drainageArea: parsed.drainageArea,
              runoffCoefficient: parsed.runoffCoefficient,
              rainfallIntensity: parsed.rainfallIntensity,
              areaUnits: parsed.areaUnits,
              intensityUnits: parsed.intensityUnits,
            })
          );

          const detentionSizing = GenericResponseSchema.parse(
            await appClient.sendCommand("calculateDetentionBasinSize", {
              inflow: runoff.runoffRate.cfs,
              outflow: parsed.allowableOutflow,
              stormDuration: parsed.stormDuration ?? 60,
              method: parsed.detentionMethod ?? "modified_rational",
              sideSlope: parsed.sideSlope ?? 3.0,
              bottomWidth: parsed.bottomWidth ?? 10.0,
              freeboardDepth: parsed.freeboardDepth ?? 1.0,
              surfaceName: parsed.basinSurfaceName ?? null,
            })
          );

          const stageStorage = parsed.basinSurfaceName != null && parsed.bottomElevation != null && parsed.topElevation != null
            ? GenericResponseSchema.parse(
                await appClient.sendCommand("calculateDetentionStageStorage", {
                  surfaceName: parsed.basinSurfaceName,
                  bottomElevation: parsed.bottomElevation,
                  topElevation: parsed.topElevation,
                  elevationIncrement: parsed.elevationIncrement ?? 0.5,
                  outletType: parsed.outletType ?? "orifice",
                  outletDiameter: parsed.outletDiameter ?? null,
                  weirLength: parsed.weirLength ?? null,
                  dischargeCoefficient: parsed.dischargeCoefficient ?? null,
                })
              )
            : null;

          return {
            workflow: "runoff_to_detention",
            inputHydrology: {
              drainageArea: parsed.drainageArea,
              areaUnits: parsed.areaUnits,
              runoffCoefficient: parsed.runoffCoefficient,
              rainfallIntensity: parsed.rainfallIntensity,
              intensityUnits: parsed.intensityUnits,
            },
            runoffEstimate: runoff,
            detentionSizing,
            stageStorage,
          };
        });

        return { content: [{ type: "text", text: JSON.stringify(response, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_hydrology_runoff_detention_workflow", error);
      }
    }
  );

  server.tool(
    "civil3d_hydrology_runoff_pipe_workflow",
    "Runs a runoff-to-pipe-network workflow by estimating Rational Method runoff, applying an optional design-flow multiplier, then executing HGL and hydraulic capacity analysis for a gravity pipe network.",
    RunoffPipeWorkflowInputShape,
    async (args: RunoffPipeWorkflowInput) => {
      try {
        const parsed = RunoffPipeWorkflowInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          const runoff = HydrologyRunoffResponseSchema.parse(
            await appClient.sendCommand("estimateHydrologyRunoff", {
              drainageArea: parsed.drainageArea,
              runoffCoefficient: parsed.runoffCoefficient,
              rainfallIntensity: parsed.rainfallIntensity,
              areaUnits: parsed.areaUnits,
              intensityUnits: parsed.intensityUnits,
            })
          );

          const designFlowCfs = runoff.runoffRate.cfs * (parsed.designFlowMultiplier ?? 1.0);

          const hglAnalysis = GenericResponseSchema.parse(
            await appClient.sendCommand("calculatePipeNetworkHgl", {
              networkName: parsed.networkName,
              tailwaterElevation: parsed.tailwaterElevation ?? null,
              designFlow: designFlowCfs,
              manningsN: parsed.manningsN ?? 0.013,
            })
          );

          const hydraulicAnalysis = GenericResponseSchema.parse(
            await appClient.sendCommand("analyzePipeNetworkHydraulics", {
              networkName: parsed.networkName,
              designFlow: designFlowCfs,
              manningsN: parsed.manningsN ?? 0.013,
              minCoverDepth: parsed.minCoverDepth ?? 2.0,
              minVelocity: parsed.minVelocity ?? 2.0,
              maxVelocity: parsed.maxVelocity ?? 10.0,
              minSlope: parsed.minSlope ?? 0.5,
            })
          );

          return {
            workflow: "runoff_to_pipe_network",
            networkName: parsed.networkName,
            inputHydrology: {
              drainageArea: parsed.drainageArea,
              areaUnits: parsed.areaUnits,
              runoffCoefficient: parsed.runoffCoefficient,
              rainfallIntensity: parsed.rainfallIntensity,
              intensityUnits: parsed.intensityUnits,
            },
            runoffEstimate: runoff,
            designFlowCfs,
            hglAnalysis,
            hydraulicAnalysis,
          };
        });

        return { content: [{ type: "text", text: JSON.stringify(response, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_hydrology_runoff_pipe_workflow", error);
      }
    }
  );
}
