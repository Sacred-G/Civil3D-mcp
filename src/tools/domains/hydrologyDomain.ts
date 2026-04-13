import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const AreaUnitsSchema = z.enum(["acres", "hectares"]);
const IntensityUnitsSchema = z.enum(["in_per_hr", "mm_per_hr"]);
const DetentionMethodSchema = z.enum(["modified_rational", "triangular_hydrograph"]);
const OutletTypeSchema = z.enum(["orifice", "weir", "riser"]);

const GenericHydrologyResponseSchema = z.object({}).passthrough();

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

const canonicalHydrologyInputShape = {
  action: z.enum([
    "list_capabilities",
    "trace_flow_path",
    "find_low_point",
    "estimate_runoff",
    "delineate_watershed",
    "calculate_catchment_area",
    "list_catchment_groups",
    "get_catchment_group",
    "list_catchments",
    "get_catchment_properties",
    "set_catchment_properties",
    "copy_catchment_to_group",
    "get_catchment_flow_path",
    "get_catchment_boundary",
    "list_tc_methods",
    "calculate_tc",
    "generate_hydrograph",
    "list_ssa_capabilities",
    "export_stm",
    "import_stm",
    "open_storm_sanitary_analysis",
    "watershed_runoff_workflow",
    "runoff_detention_workflow",
    "runoff_pipe_workflow",
  ]),
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
  areaUnits: AreaUnitsSchema.optional(),
  intensityUnits: IntensityUnitsSchema.optional(),
  groupName: z.string().optional(),
  catchmentName: z.string().optional(),
  targetGroupName: z.string().optional(),
  manningsCoefficient: z.number().optional(),
  curveNumber: z.number().optional(),
  description: z.string().optional(),
  newName: z.string().optional(),
  method: z.string().optional(),
  flowLength_ft: z.number().optional(),
  elevationDifference_ft: z.number().optional(),
  manningsN: z.number().optional(),
  rainfall2yr24hr_in: z.number().optional(),
  slope_ftPerFt: z.number().optional(),
  slope_percent: z.number().optional(),
  surfaceType: z.enum(["paved", "unpaved"]).optional(),
  hydraulicRadius_ft: z.number().optional(),
  drainageArea_mi2: z.number().optional(),
  runoffDepth_in: z.number().optional(),
  timeOfConcentration_hr: z.number().optional(),
  stormDuration_hr: z.number().optional(),
  filePath: z.string().optional(),
  findLowPointSampleSpacing: z.number().optional(),
  catchmentSampleSpacing: z.number().optional(),
  runoffAreaUnits: AreaUnitsSchema.optional(),
  allowableOutflow: z.number().optional(),
  stormDuration: z.number().optional(),
  detentionMethod: DetentionMethodSchema.optional(),
  sideSlope: z.number().optional(),
  bottomWidth: z.number().optional(),
  freeboardDepth: z.number().optional(),
  basinSurfaceName: z.string().optional(),
  bottomElevation: z.number().optional(),
  topElevation: z.number().optional(),
  elevationIncrement: z.number().optional(),
  outletType: OutletTypeSchema.optional(),
  outletDiameter: z.number().optional(),
  weirLength: z.number().optional(),
  dischargeCoefficient: z.number().optional(),
  networkName: z.string().optional(),
  designFlowMultiplier: z.number().optional(),
  tailwaterElevation: z.number().optional(),
  minCoverDepth: z.number().optional(),
  minVelocity: z.number().optional(),
  maxVelocity: z.number().optional(),
  minSlope: z.number().optional(),
};

const HydrologyListCapabilitiesArgsSchema = z.object({
  action: z.literal("list_capabilities"),
});

const HydrologyTraceFlowPathArgsSchema = z.object({
  action: z.literal("trace_flow_path"),
  surfaceName: z.string(),
  x: z.number(),
  y: z.number(),
  stepDistance: z.number().positive().optional(),
  maxSteps: z.number().int().positive().optional(),
});

const HydrologyFindLowPointArgsSchema = z.object({
  action: z.literal("find_low_point"),
  surfaceName: z.string(),
  sampleSpacing: z.number().positive().optional(),
});

const HydrologyEstimateRunoffArgsSchema = z.object({
  action: z.literal("estimate_runoff"),
  drainageArea: z.number().positive(),
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  areaUnits: AreaUnitsSchema,
  intensityUnits: IntensityUnitsSchema,
});

const HydrologyDelineateWatershedArgsSchema = z.object({
  action: z.literal("delineate_watershed"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  gridSpacing: z.number().positive().optional(),
  searchRadius: z.number().positive().optional(),
});

const HydrologyCalculateCatchmentAreaArgsSchema = z.object({
  action: z.literal("calculate_catchment_area"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  sampleSpacing: z.number().positive().optional(),
  maxDistance: z.number().positive().optional(),
});

const ListCatchmentGroupsArgsSchema = z.object({
  action: z.literal("list_catchment_groups"),
});

const GetCatchmentGroupArgsSchema = z.object({
  action: z.literal("get_catchment_group"),
  groupName: z.string(),
});

const ListCatchmentsArgsSchema = z.object({
  action: z.literal("list_catchments"),
});

const GetCatchmentPropertiesArgsSchema = z.object({
  action: z.literal("get_catchment_properties"),
  catchmentName: z.string(),
});

const SetCatchmentPropertiesArgsSchema = z.object({
  action: z.literal("set_catchment_properties"),
  catchmentName: z.string(),
  runoffCoefficient: z.number().min(0).max(1).optional(),
  manningsCoefficient: z.number().positive().optional(),
  curveNumber: z.number().min(0).max(100).optional(),
  description: z.string().optional(),
  newName: z.string().optional(),
});

const CopyCatchmentToGroupArgsSchema = z.object({
  action: z.literal("copy_catchment_to_group"),
  catchmentName: z.string(),
  targetGroupName: z.string(),
});

const GetCatchmentFlowPathArgsSchema = z.object({
  action: z.literal("get_catchment_flow_path"),
  catchmentName: z.string(),
});

const GetCatchmentBoundaryArgsSchema = z.object({
  action: z.literal("get_catchment_boundary"),
  catchmentName: z.string(),
});

const ListTcMethodsArgsSchema = z.object({
  action: z.literal("list_tc_methods"),
});

const CalculateTcArgsSchema = z.object({
  action: z.literal("calculate_tc"),
  method: z.string(),
  flowLength_ft: z.number().positive().optional(),
  elevationDifference_ft: z.number().positive().optional(),
  manningsN: z.number().positive().optional(),
  rainfall2yr24hr_in: z.number().positive().optional(),
  slope_ftPerFt: z.number().positive().optional(),
  slope_percent: z.number().positive().optional(),
  surfaceType: z.enum(["paved", "unpaved"]).optional(),
  hydraulicRadius_ft: z.number().positive().optional(),
  runoffCoefficient: z.number().min(0).max(1).optional(),
  curveNumber: z.number().min(1).max(100).optional(),
});

const GenerateHydrographArgsSchema = z.object({
  action: z.literal("generate_hydrograph"),
  drainageArea_mi2: z.number().positive(),
  runoffDepth_in: z.number().positive(),
  timeOfConcentration_hr: z.number().positive(),
  method: z.string().optional(),
  stormDuration_hr: z.number().positive().optional(),
});

const ListSsaCapabilitiesArgsSchema = z.object({
  action: z.literal("list_ssa_capabilities"),
});

const ExportStmArgsSchema = z.object({
  action: z.literal("export_stm"),
  filePath: z.string().optional(),
});

const ImportStmArgsSchema = z.object({
  action: z.literal("import_stm"),
  filePath: z.string().optional(),
});

const OpenStormSanitaryAnalysisArgsSchema = z.object({
  action: z.literal("open_storm_sanitary_analysis"),
  filePath: z.string().optional(),
});

const WatershedRunoffWorkflowArgsSchema = z.object({
  action: z.literal("watershed_runoff_workflow"),
  surfaceName: z.string(),
  outletX: z.number().optional(),
  outletY: z.number().optional(),
  findLowPointSampleSpacing: z.number().positive().optional(),
  gridSpacing: z.number().positive().optional(),
  searchRadius: z.number().positive().optional(),
  catchmentSampleSpacing: z.number().positive().optional(),
  maxDistance: z.number().positive().optional(),
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  intensityUnits: IntensityUnitsSchema,
  runoffAreaUnits: AreaUnitsSchema.optional(),
}).superRefine((value, ctx) => {
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

const RunoffDetentionWorkflowArgsSchema = z.object({
  action: z.literal("runoff_detention_workflow"),
  drainageArea: z.number().positive(),
  areaUnits: AreaUnitsSchema,
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  intensityUnits: IntensityUnitsSchema,
  allowableOutflow: z.number().positive(),
  stormDuration: z.number().positive().optional(),
  detentionMethod: DetentionMethodSchema.optional(),
  sideSlope: z.number().positive().optional(),
  bottomWidth: z.number().nonnegative().optional(),
  freeboardDepth: z.number().nonnegative().optional(),
  basinSurfaceName: z.string().optional(),
  bottomElevation: z.number().optional(),
  topElevation: z.number().optional(),
  elevationIncrement: z.number().positive().optional(),
  outletType: OutletTypeSchema.optional(),
  outletDiameter: z.number().positive().optional(),
  weirLength: z.number().positive().optional(),
  dischargeCoefficient: z.number().positive().optional(),
}).superRefine((value, ctx) => {
  const stageFieldsProvided = [value.basinSurfaceName, value.bottomElevation, value.topElevation]
    .filter((field) => field != null)
    .length;
  if (stageFieldsProvided > 0 && stageFieldsProvided < 3) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Provide basinSurfaceName, bottomElevation, and topElevation together to generate stage-storage output.",
      path: ["basinSurfaceName"],
    });
  }
});

const RunoffPipeWorkflowArgsSchema = z.object({
  action: z.literal("runoff_pipe_workflow"),
  networkName: z.string(),
  drainageArea: z.number().positive(),
  areaUnits: AreaUnitsSchema,
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  intensityUnits: IntensityUnitsSchema,
  designFlowMultiplier: z.number().positive().optional(),
  tailwaterElevation: z.number().optional(),
  manningsN: z.number().positive().optional(),
  minCoverDepth: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
});

export const HYDROLOGY_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "hydrology",
  actions: {
    list_capabilities: {
      action: "list_capabilities",
      inputSchema: HydrologyListCapabilitiesArgsSchema,
      responseSchema: HydrologyCapabilitiesResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listHydrologyCapabilities"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listHydrologyCapabilities", {}),
      ),
    },
    trace_flow_path: {
      action: "trace_flow_path",
      inputSchema: HydrologyTraceFlowPathArgsSchema,
      responseSchema: HydrologyFlowPathResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["traceHydrologyFlowPath"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("traceHydrologyFlowPath", {
          surfaceName: args.surfaceName,
          x: args.x,
          y: args.y,
          stepDistance: args.stepDistance,
          maxSteps: args.maxSteps,
        }),
      ),
    },
    find_low_point: {
      action: "find_low_point",
      inputSchema: HydrologyFindLowPointArgsSchema,
      responseSchema: HydrologyLowPointResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["findHydrologyLowPoint"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("findHydrologyLowPoint", {
          surfaceName: args.surfaceName,
          sampleSpacing: args.sampleSpacing,
        }),
      ),
    },
    estimate_runoff: {
      action: "estimate_runoff",
      inputSchema: HydrologyEstimateRunoffArgsSchema,
      responseSchema: HydrologyRunoffResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["estimateHydrologyRunoff"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("estimateHydrologyRunoff", {
          drainageArea: args.drainageArea,
          runoffCoefficient: args.runoffCoefficient,
          rainfallIntensity: args.rainfallIntensity,
          areaUnits: args.areaUnits,
          intensityUnits: args.intensityUnits,
        }),
      ),
    },
    delineate_watershed: {
      action: "delineate_watershed",
      inputSchema: HydrologyDelineateWatershedArgsSchema,
      responseSchema: HydrologyWatershedResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["delineateWatershed"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("delineateWatershed", {
          surfaceName: args.surfaceName,
          outletX: args.outletX,
          outletY: args.outletY,
          gridSpacing: args.gridSpacing,
          searchRadius: args.searchRadius,
        }),
      ),
    },
    calculate_catchment_area: {
      action: "calculate_catchment_area",
      inputSchema: HydrologyCalculateCatchmentAreaArgsSchema,
      responseSchema: HydrologyCatchmentAreaResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["calculateCatchmentArea"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("calculateCatchmentArea", {
          surfaceName: args.surfaceName,
          outletX: args.outletX,
          outletY: args.outletY,
          sampleSpacing: args.sampleSpacing,
          maxDistance: args.maxDistance,
        }),
      ),
    },
    list_catchment_groups: {
      action: "list_catchment_groups",
      inputSchema: ListCatchmentGroupsArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listCatchmentGroups"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listCatchmentGroups", {}),
      ),
    },
    get_catchment_group: {
      action: "get_catchment_group",
      inputSchema: GetCatchmentGroupArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCatchmentGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCatchmentGroup", { groupName: args.groupName }),
      ),
    },
    list_catchments: {
      action: "list_catchments",
      inputSchema: ListCatchmentsArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listCatchments"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listCatchments", {}),
      ),
    },
    get_catchment_properties: {
      action: "get_catchment_properties",
      inputSchema: GetCatchmentPropertiesArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCatchmentProperties"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCatchmentProperties", { catchmentName: args.catchmentName }),
      ),
    },
    set_catchment_properties: {
      action: "set_catchment_properties",
      inputSchema: SetCatchmentPropertiesArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["setCatchmentProperties"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("setCatchmentProperties", {
          catchmentName: args.catchmentName,
          runoffCoefficient: args.runoffCoefficient,
          manningsCoefficient: args.manningsCoefficient,
          curveNumber: args.curveNumber,
          description: args.description,
          newName: args.newName,
        }),
      ),
    },
    copy_catchment_to_group: {
      action: "copy_catchment_to_group",
      inputSchema: CopyCatchmentToGroupArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["copyCatchmentToGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("copyCatchmentToGroup", {
          catchmentName: args.catchmentName,
          targetGroupName: args.targetGroupName,
        }),
      ),
    },
    get_catchment_flow_path: {
      action: "get_catchment_flow_path",
      inputSchema: GetCatchmentFlowPathArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCatchmentFlowPath"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCatchmentFlowPath", { catchmentName: args.catchmentName }),
      ),
    },
    get_catchment_boundary: {
      action: "get_catchment_boundary",
      inputSchema: GetCatchmentBoundaryArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCatchmentBoundary"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCatchmentBoundary", { catchmentName: args.catchmentName }),
      ),
    },
    list_tc_methods: {
      action: "list_tc_methods",
      inputSchema: ListTcMethodsArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["listTcMethods"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listTcMethods", {}),
      ),
    },
    calculate_tc: {
      action: "calculate_tc",
      inputSchema: CalculateTcArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["calculateTimeOfConcentration"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("calculateTimeOfConcentration", {
          method: args.method,
          flowLength_ft: args.flowLength_ft,
          elevationDifference_ft: args.elevationDifference_ft,
          manningsN: args.manningsN,
          rainfall2yr24hr_in: args.rainfall2yr24hr_in,
          slope_ftPerFt: args.slope_ftPerFt,
          slope_percent: args.slope_percent,
          surfaceType: args.surfaceType,
          hydraulicRadius_ft: args.hydraulicRadius_ft,
          runoffCoefficient: args.runoffCoefficient,
          curveNumber: args.curveNumber,
        }),
      ),
    },
    generate_hydrograph: {
      action: "generate_hydrograph",
      inputSchema: GenerateHydrographArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["generateHydrograph"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("generateHydrograph", {
          drainageArea_mi2: args.drainageArea_mi2,
          runoffDepth_in: args.runoffDepth_in,
          timeOfConcentration_hr: args.timeOfConcentration_hr,
          method: args.method,
          stormDuration_hr: args.stormDuration_hr,
        }),
      ),
    },
    list_ssa_capabilities: {
      action: "list_ssa_capabilities",
      inputSchema: ListSsaCapabilitiesArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listSsaCapabilities"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listSsaCapabilities", {}),
      ),
    },
    export_stm: {
      action: "export_stm",
      inputSchema: ExportStmArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["export"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["exportStm"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("exportStm", { filePath: args.filePath }),
      ),
    },
    import_stm: {
      action: "import_stm",
      inputSchema: ImportStmArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["import"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["importStm"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("importStm", { filePath: args.filePath }),
      ),
    },
    open_storm_sanitary_analysis: {
      action: "open_storm_sanitary_analysis",
      inputSchema: OpenStormSanitaryAnalysisArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["openStormSanitaryAnalysis"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("openStormSanitaryAnalysis", {}),
      ),
    },
    watershed_runoff_workflow: {
      action: "watershed_runoff_workflow",
      inputSchema: WatershedRunoffWorkflowArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["watershedRunoffWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("watershedRunoffWorkflow", {
          surfaceName: args.surfaceName,
          outletX: args.outletX,
          outletY: args.outletY,
          findLowPointSampleSpacing: args.findLowPointSampleSpacing,
          gridSpacing: args.gridSpacing,
          searchRadius: args.searchRadius,
          catchmentSampleSpacing: args.catchmentSampleSpacing,
          maxDistance: args.maxDistance,
          runoffCoefficient: args.runoffCoefficient,
          rainfallIntensity: args.rainfallIntensity,
          intensityUnits: args.intensityUnits,
          runoffAreaUnits: args.runoffAreaUnits,
        }),
      ),
    },
    runoff_detention_workflow: {
      action: "runoff_detention_workflow",
      inputSchema: RunoffDetentionWorkflowArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["runoffDetentionWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("runoffDetentionWorkflow", {
          drainageArea: args.drainageArea,
          areaUnits: args.areaUnits,
          runoffCoefficient: args.runoffCoefficient,
          rainfallIntensity: args.rainfallIntensity,
          intensityUnits: args.intensityUnits,
          allowableOutflow: args.allowableOutflow,
          stormDuration: args.stormDuration,
          detentionMethod: args.detentionMethod,
          sideSlope: args.sideSlope,
          bottomWidth: args.bottomWidth,
          freeboardDepth: args.freeboardDepth,
          basinSurfaceName: args.basinSurfaceName,
          bottomElevation: args.bottomElevation,
          topElevation: args.topElevation,
          elevationIncrement: args.elevationIncrement,
          outletType: args.outletType,
          outletDiameter: args.outletDiameter,
          weirLength: args.weirLength,
          dischargeCoefficient: args.dischargeCoefficient,
        }),
      ),
    },
    runoff_pipe_workflow: {
      action: "runoff_pipe_workflow",
      inputSchema: RunoffPipeWorkflowArgsSchema,
      responseSchema: GenericHydrologyResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["runoffPipeWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("runoffPipeWorkflow", {
          networkName: args.networkName,
          drainageArea: args.drainageArea,
          areaUnits: args.areaUnits,
          runoffCoefficient: args.runoffCoefficient,
          rainfallIntensity: args.rainfallIntensity,
          intensityUnits: args.intensityUnits,
          designFlowMultiplier: args.designFlowMultiplier,
          tailwaterElevation: args.tailwaterElevation,
          manningsN: args.manningsN,
          minCoverDepth: args.minCoverDepth,
          minVelocity: args.minVelocity,
          maxVelocity: args.maxVelocity,
          minSlope: args.minSlope,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_hydrology",
      displayName: "Civil 3D Hydrology",
      description: "Provides hydrology, catchment, time-of-concentration, SSA, and drainage workflow operations through a single domain tool.",
      inputShape: canonicalHydrologyInputShape,
      supportedActions: [
        "list_capabilities",
        "trace_flow_path",
        "find_low_point",
        "estimate_runoff",
        "delineate_watershed",
        "calculate_catchment_area",
        "list_catchment_groups",
        "get_catchment_group",
        "list_catchments",
        "get_catchment_properties",
        "set_catchment_properties",
        "copy_catchment_to_group",
        "get_catchment_flow_path",
        "get_catchment_boundary",
        "list_tc_methods",
        "calculate_tc",
        "generate_hydrograph",
        "list_ssa_capabilities",
        "export_stm",
        "import_stm",
        "open_storm_sanitary_analysis",
        "watershed_runoff_workflow",
        "runoff_detention_workflow",
        "runoff_pipe_workflow",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_catchment",
      displayName: "Civil 3D Catchment Management",
      description: "Manages Civil 3D catchments and catchment groups including properties, flow paths, and boundaries.",
      inputShape: {
        action: z.enum([
          "list_catchment_groups",
          "get_catchment_group",
          "list_catchments",
          "get_catchment_properties",
          "set_catchment_properties",
          "copy_catchment_to_group",
          "get_catchment_flow_path",
          "get_catchment_boundary",
        ]),
        groupName: z.string().optional(),
        catchmentName: z.string().optional(),
        targetGroupName: z.string().optional(),
        runoffCoefficient: z.number().min(0).max(1).optional(),
        manningsCoefficient: z.number().positive().optional(),
        curveNumber: z.number().min(0).max(100).optional(),
        description: z.string().optional(),
        newName: z.string().optional(),
      },
      supportedActions: [
        "list_catchment_groups",
        "get_catchment_group",
        "list_catchments",
        "get_catchment_properties",
        "set_catchment_properties",
        "copy_catchment_to_group",
        "get_catchment_flow_path",
        "get_catchment_boundary",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_time_of_concentration",
      displayName: "Civil 3D Time of Concentration & Hydrograph",
      description: "Calculates time of concentration using standard methods and generates hydrographs.",
      inputShape: {
        action: z.enum(["list_tc_methods", "calculate_tc", "generate_hydrograph"]),
        method: z.string().optional(),
        flowLength_ft: z.number().positive().optional(),
        elevationDifference_ft: z.number().positive().optional(),
        manningsN: z.number().positive().optional(),
        rainfall2yr24hr_in: z.number().positive().optional(),
        slope_ftPerFt: z.number().positive().optional(),
        slope_percent: z.number().positive().optional(),
        surfaceType: z.enum(["paved", "unpaved"]).optional(),
        hydraulicRadius_ft: z.number().positive().optional(),
        runoffCoefficient: z.number().min(0).max(1).optional(),
        curveNumber: z.number().min(1).max(100).optional(),
        drainageArea_mi2: z.number().positive().optional(),
        runoffDepth_in: z.number().positive().optional(),
        timeOfConcentration_hr: z.number().positive().optional(),
        stormDuration_hr: z.number().positive().optional(),
      },
      supportedActions: ["list_tc_methods", "calculate_tc", "generate_hydrograph"],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_stm",
      displayName: "Civil 3D Storm & Sanitary Analysis (STM)",
      description: "Manages STM export/import and Storm and Sanitary Analysis launch workflows.",
      inputShape: {
        action: z.enum(["list_ssa_capabilities", "export_stm", "import_stm", "open_storm_sanitary_analysis"]),
        filePath: z.string().optional(),
      },
      supportedActions: ["list_ssa_capabilities", "export_stm", "import_stm", "open_storm_sanitary_analysis"],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_hydrology_watershed_runoff_workflow",
      displayName: "Civil 3D Hydrology Watershed Runoff Workflow",
      description: "Builds a complete watershed-to-runoff analysis by locating or using an outlet, delineating the watershed, calculating catchment area, converting area units, and estimating Rational Method runoff.",
      inputShape: {
        surfaceName: z.string(),
        outletX: z.number().optional(),
        outletY: z.number().optional(),
        findLowPointSampleSpacing: z.number().positive().optional(),
        gridSpacing: z.number().positive().optional(),
        searchRadius: z.number().positive().optional(),
        catchmentSampleSpacing: z.number().positive().optional(),
        maxDistance: z.number().positive().optional(),
        runoffCoefficient: z.number().min(0).max(1),
        rainfallIntensity: z.number().positive(),
        intensityUnits: IntensityUnitsSchema,
        runoffAreaUnits: AreaUnitsSchema.optional(),
      },
      supportedActions: ["watershed_runoff_workflow"],
      resolveAction: (rawArgs) => ({ action: "watershed_runoff_workflow", args: { action: "watershed_runoff_workflow", ...rawArgs } }),
    },
    {
      toolName: "civil3d_hydrology_runoff_detention_workflow",
      displayName: "Civil 3D Hydrology Runoff Detention Workflow",
      description: "Builds a runoff-to-detention workflow by estimating Rational Method runoff, sizing a detention basin, and optionally generating a stage-storage-discharge table.",
      inputShape: {
        drainageArea: z.number().positive(),
        areaUnits: AreaUnitsSchema,
        runoffCoefficient: z.number().min(0).max(1),
        rainfallIntensity: z.number().positive(),
        intensityUnits: IntensityUnitsSchema,
        allowableOutflow: z.number().positive(),
        stormDuration: z.number().positive().optional(),
        detentionMethod: DetentionMethodSchema.optional(),
        sideSlope: z.number().positive().optional(),
        bottomWidth: z.number().nonnegative().optional(),
        freeboardDepth: z.number().nonnegative().optional(),
        basinSurfaceName: z.string().optional(),
        bottomElevation: z.number().optional(),
        topElevation: z.number().optional(),
        elevationIncrement: z.number().positive().optional(),
        outletType: OutletTypeSchema.optional(),
        outletDiameter: z.number().positive().optional(),
        weirLength: z.number().positive().optional(),
        dischargeCoefficient: z.number().positive().optional(),
      },
      supportedActions: ["runoff_detention_workflow"],
      resolveAction: (rawArgs) => ({ action: "runoff_detention_workflow", args: { action: "runoff_detention_workflow", ...rawArgs } }),
    },
    {
      toolName: "civil3d_hydrology_runoff_pipe_workflow",
      displayName: "Civil 3D Hydrology Runoff Pipe Workflow",
      description: "Builds a runoff-to-pipe-network workflow by estimating Rational Method runoff, then performing HGL and hydraulic capacity analysis on a gravity pipe network.",
      inputShape: {
        networkName: z.string(),
        drainageArea: z.number().positive(),
        areaUnits: AreaUnitsSchema,
        runoffCoefficient: z.number().min(0).max(1),
        rainfallIntensity: z.number().positive(),
        intensityUnits: IntensityUnitsSchema,
        designFlowMultiplier: z.number().positive().optional(),
        tailwaterElevation: z.number().optional(),
        manningsN: z.number().positive().optional(),
        minCoverDepth: z.number().nonnegative().optional(),
        minVelocity: z.number().nonnegative().optional(),
        maxVelocity: z.number().positive().optional(),
        minSlope: z.number().nonnegative().optional(),
      },
      supportedActions: ["runoff_pipe_workflow"],
      resolveAction: (rawArgs) => ({ action: "runoff_pipe_workflow", args: { action: "runoff_pipe_workflow", ...rawArgs } }),
    },
  ],
};
