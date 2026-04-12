import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";
import { routeIntent } from "../orchestration/IntentRouter.js";
import { getProjectContext } from "../orchestration/ProjectContextService.js";
import { buildWorkflowPlan } from "../orchestration/WorkflowPlanner.js";
import { resolveParamsFromSelection } from "../orchestration/SelectionResolver.js";
import { RouteParams } from "../orchestration/IntentRouter.js";
import { listDomains, listToolCatalog } from "./tool_catalog.js";

const Civil3DOrchestrateInputShape = {
  request: z.string().min(1).describe("Natural-language Civil 3D request."),
  execute: z.boolean().optional().describe("When true, execute the selected action if enough parameters are available."),
  name: z.string().optional(),
  alignmentName: z.string().optional(),
  corridorName: z.string().optional(),
  profileName: z.string().optional(),
  surfaceName: z.string().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
  filePath: z.string().optional(),
  outputPath: z.string().optional(),
  gridSpacing: z.number().optional(),
  designSpeed: z.number().optional(),
  inflow: z.number().optional(),
  outflow: z.number().optional(),
  bottomElevation: z.number().optional(),
  topElevation: z.number().optional(),
  payItems: z.array(
    z.object({
      code: z.string(),
      description: z.string(),
      unit: z.string(),
      unitPrice: z.number().nonnegative(),
    })
  ).optional(),
};

const Civil3DOrchestrateInputSchema = z.object(Civil3DOrchestrateInputShape);

export type OrchestrateArgs = z.infer<typeof Civil3DOrchestrateInputSchema>;

function buildMergedParams(args: OrchestrateArgs, routed: ReturnType<typeof routeIntent>) {
  return {
    name: args.name ?? routed.extractedParams.name,
    alignmentName: args.alignmentName ?? routed.extractedParams.alignmentName,
    corridorName: args.corridorName ?? routed.extractedParams.corridorName,
    profileName: args.profileName ?? routed.extractedParams.profileName,
    surfaceName: args.surfaceName ?? routed.extractedParams.surfaceName,
    baseSurface: args.baseSurface ?? routed.extractedParams.baseSurface,
    comparisonSurface: args.comparisonSurface ?? routed.extractedParams.comparisonSurface,
    style: args.style ?? routed.extractedParams.style,
    layer: args.layer ?? routed.extractedParams.layer,
    labelSet: args.labelSet ?? routed.extractedParams.labelSet,
    filePath: args.filePath ?? routed.extractedParams.filePath,
    outputPath: args.outputPath ?? routed.extractedParams.outputPath,
    gridSpacing: args.gridSpacing ?? routed.extractedParams.gridSpacing,
    designSpeed: args.designSpeed ?? routed.extractedParams.designSpeed,
    inflow: args.inflow ?? routed.extractedParams.inflow,
    outflow: args.outflow ?? routed.extractedParams.outflow,
    bottomElevation: args.bottomElevation ?? routed.extractedParams.bottomElevation,
    topElevation: args.topElevation ?? routed.extractedParams.topElevation,
    payItems: args.payItems ?? routed.extractedParams.payItems,
  };
}

function hasRequiredValue(value: unknown): boolean {
  if (typeof value === "string") {
    return value.trim().length > 0;
  }

  if (typeof value === "number") {
    return Number.isFinite(value);
  }

  if (Array.isArray(value)) {
    return value.length > 0;
  }

  return value != null;
}

function buildCorridorAdvice() {
  return {
    recommendedSequence: [
      "Create or identify an alignment.",
      "Create or identify an existing ground surface.",
      "Create an EG profile from that alignment and surface.",
      "Create a design profile or layout profile.",
      "Choose an assembly before corridor creation.",
    ],
    requiredObjects: ["alignment", "surface", "profile", "assembly"],
  };
}

function buildContextAwareAdvice(intent: ReturnType<typeof routeIntent>, projectContext: Awaited<ReturnType<typeof getProjectContext>>) {
  const drawingInfo = (projectContext.drawingInfo ?? {}) as {
    objectCounts?: {
      surfaces?: number;
      alignments?: number;
      profiles?: number;
      corridors?: number;
    };
  };

  const objectCounts = drawingInfo.objectCounts ?? {};
  const hasSurfaces = (objectCounts.surfaces ?? 0) > 0;
  const hasAlignments = (objectCounts.alignments ?? 0) > 0;
  const hasProfiles = (objectCounts.profiles ?? 0) > 0;

  if (intent.match.intent === "create_profile_from_surface") {
    return {
      prerequisitesSatisfied: hasSurfaces && hasAlignments,
      suggestions: [
        !hasAlignments ? "No alignments detected. Create or identify an alignment before creating a profile from surface." : null,
        !hasSurfaces ? "No surfaces detected. Create or identify a surface before creating a profile from surface." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "list_profiles" || intent.match.intent === "create_layout_profile") {
    return {
      prerequisitesSatisfied: hasAlignments,
      suggestions: [
        !hasAlignments ? "No alignments detected. Select or identify an alignment before working with profiles." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "corridor_prerequisites") {
    return {
      prerequisitesSatisfied: hasAlignments && hasSurfaces && hasProfiles,
      suggestions: [
        !hasAlignments ? "No alignments detected. Corridor creation usually starts with an alignment." : null,
        !hasSurfaces ? "No surfaces detected. Existing ground is commonly needed for corridor targeting and profile generation." : null,
        !hasProfiles ? "No profiles detected. A design or existing-ground profile is typically needed before creating a corridor." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "get_surface_statistics" || intent.match.intent === "analyze_surface_slope" || intent.match.intent === "calculate_surface_volume") {
    return {
      prerequisitesSatisfied: hasSurfaces,
      suggestions: [
        !hasSurfaces ? "No surfaces detected. Create or identify the required Civil 3D surfaces before running this analysis." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "generate_surface_volume_report" || intent.match.intent === "sample_surface_elevations") {
    return {
      prerequisitesSatisfied: hasSurfaces,
      suggestions: [
        !hasSurfaces ? "No surfaces detected. Create or identify the required Civil 3D surface before running this request." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "check_stopping_distance") {
    return {
      prerequisitesSatisfied: hasAlignments && hasProfiles,
      suggestions: [
        !hasAlignments ? "No alignments detected. Select or identify an alignment before running stopping sight distance checks." : null,
        !hasProfiles ? "No profiles detected. Select or identify a profile before running stopping sight distance checks." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "generate_detention_stage_storage") {
    return {
      prerequisitesSatisfied: hasSurfaces,
      suggestions: [
        !hasSurfaces ? "No surfaces detected. A basin surface is required before generating a detention stage-storage table." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "calculate_slope_geometry" || intent.match.intent === "check_slope_stability") {
    return {
      prerequisitesSatisfied: hasAlignments,
      suggestions: [
        !hasAlignments ? "No alignments detected. Select or identify an alignment before running slope analysis." : null,
      ].filter(Boolean),
    };
  }

  if (intent.match.intent === "export_pay_items" || intent.match.intent === "estimate_material_cost") {
    return {
      prerequisitesSatisfied: true,
      suggestions: [],
    };
  }

  return {
    prerequisitesSatisfied: true,
    suggestions: [],
  };
}

async function executeIntent(intent: ReturnType<typeof routeIntent>, params: RouteParams) {
  switch (intent.match.intent) {
    case "drawing_info":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getDrawingInfo", {}));
    case "list_tool_capabilities":
      return {
        domains: listDomains(),
        tools: listToolCatalog(),
      };
    case "list_surfaces":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listSurfaces", {}));
    case "get_surface":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getSurface", {
        name: params.name,
      }));
    case "create_surface":
      return await withApplicationConnection((appClient) =>
        appClient.sendCommand("createSurface", {
          name: params.name,
          style: params.style,
          layer: params.layer,
        })
      );
    case "get_surface_statistics":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getSurfaceStatisticsDetailed", {
        name: params.name,
      }));
    case "analyze_surface_slope":
      return await withApplicationConnection((appClient) => appClient.sendCommand("analyzeSurfaceSlope", {
        name: params.name,
        ranges: null,
        numRanges: 5,
      }));
    case "calculate_surface_volume":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateSurfaceVolume", {
        baseSurface: params.baseSurface,
        comparisonSurface: params.comparisonSurface,
        method: "tin_volume",
      }));
    case "generate_surface_volume_report":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getSurfaceVolumeReport", {
        baseSurface: params.baseSurface,
        comparisonSurface: params.comparisonSurface,
        format: "summary",
      }));
    case "sample_surface_elevations":
      return await withApplicationConnection((appClient) => appClient.sendCommand("sampleSurfaceElevations", {
        name: params.name,
        method: "grid",
        gridSpacing: params.gridSpacing,
        points: undefined,
        startPoint: undefined,
        endPoint: undefined,
        numSamples: 50,
        boundary: undefined,
      }));
    case "create_surface_from_dem":
      return await withApplicationConnection((appClient) => appClient.sendCommand("createSurfaceFromDem", {
        filePath: params.filePath,
        name: params.name,
        style: params.style,
        layer: params.layer,
        description: undefined,
        coordinateSystem: undefined,
      }));
    case "list_alignments":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listAlignments", {}));
    case "get_alignment":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getAlignment", {
        name: params.name,
      }));
    case "list_profiles":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listProfiles", {
        alignmentName: params.alignmentName,
      }));
    case "create_profile_from_surface":
      return await withApplicationConnection((appClient) =>
        appClient.sendCommand("createProfileFromSurface", {
          alignmentName: params.alignmentName,
          profileName: params.profileName,
          surfaceName: params.surfaceName,
          style: params.style,
          layer: params.layer,
          labelSet: params.labelSet,
        })
      );
    case "create_layout_profile":
      return await withApplicationConnection((appClient) =>
        appClient.sendCommand("createLayoutProfile", {
          alignmentName: params.alignmentName,
          profileName: params.profileName,
          style: params.style,
          layer: params.layer,
          labelSet: params.labelSet,
        })
      );
    case "calculate_sight_distance":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateSightDistance", {
        designSpeed: params.designSpeed,
        speedUnits: "kmh",
        sightDistanceType: "stopping",
        grade: 0,
        frictionCoefficient: null,
        perceptionReactionTime: 2.5,
        standard: "AASHTO",
        alignmentName: params.alignmentName ?? null,
        profileName: params.profileName ?? null,
        checkStation: null,
      }));
    case "check_stopping_distance":
      return await withApplicationConnection((appClient) => appClient.sendCommand("checkStoppingDistance", {
        alignmentName: params.alignmentName,
        profileName: params.profileName,
        designSpeed: params.designSpeed,
        stationStart: null,
        stationEnd: null,
        stationInterval: 25,
        standard: "AASHTO",
      }));
    case "calculate_detention_basin_size":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateDetentionBasinSize", {
        inflow: params.inflow,
        outflow: params.outflow,
        stormDuration: 60,
        method: "modified_rational",
        sideSlope: 3.0,
        bottomWidth: 10.0,
        freeboardDepth: 1.0,
        surfaceName: params.surfaceName ?? null,
      }));
    case "generate_detention_stage_storage":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateDetentionStageStorage", {
        surfaceName: params.surfaceName,
        bottomElevation: params.bottomElevation,
        topElevation: params.topElevation,
        elevationIncrement: 0.5,
        outletType: "orifice",
        outletDiameter: null,
        weirLength: null,
        dischargeCoefficient: null,
      }));
    case "calculate_slope_geometry":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateSlopeGeometry", {
        alignmentName: params.alignmentName ?? null,
        profileName: params.profileName ?? null,
        surfaceName: params.surfaceName ?? null,
        cutSlopeRatio: 2.0,
        fillSlopeRatio: 3.0,
        benchWidth: 0,
        benchHeightInterval: 20,
        stationStart: null,
        stationEnd: null,
        stationInterval: 10,
        roadwayWidth: null,
      }));
    case "check_slope_stability":
      return await withApplicationConnection((appClient) => appClient.sendCommand("checkSlopeStability", {
        alignmentName: params.alignmentName ?? null,
        surfaceName: params.surfaceName ?? null,
        maxCutSlopeRatio: 1.5,
        maxFillSlopeRatio: 2.0,
        maxCutHeight: 30,
        maxFillHeight: 40,
        stationInterval: 25,
        soilType: "granular",
      }));
    case "export_pay_items":
      return await withApplicationConnection((appClient) => appClient.sendCommand("exportPayItems", {
        outputPath: params.outputPath,
        corridorName: params.corridorName ?? null,
        baseSurface: params.baseSurface ?? null,
        designSurface: params.comparisonSurface ?? null,
        alignmentName: params.alignmentName ?? null,
        payItems: params.payItems ?? [],
        includeEarthwork: true,
        includeCorridorMaterials: true,
        includePipeLengths: true,
        includeStructureCounts: true,
      }));
    case "estimate_material_cost":
      return await withApplicationConnection((appClient) => appClient.sendCommand("calculateMaterialCostEstimate", {
        corridorName: params.corridorName ?? null,
        baseSurface: params.baseSurface ?? null,
        designSurface: params.comparisonSurface ?? null,
        alignmentName: params.alignmentName ?? null,
        contingencyPercent: 0,
        mobilizationPercent: 5,
        payItems: params.payItems,
        outputPath: params.outputPath ?? null,
      }));
    case "list_corridors":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listCorridors", {}));
    case "get_corridor":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getCorridor", {
        name: params.name,
      }));
    case "rebuild_corridor":
      return await withApplicationConnection((appClient) => appClient.sendCommand("rebuildCorridor", {
        name: params.name,
      }));
    case "corridor_prerequisites":
      return buildCorridorAdvice();
  }
}

export async function executeCivil3DOrchestrate(rawArgs: OrchestrateArgs) {
  const args = Civil3DOrchestrateInputSchema.parse(rawArgs);
  const routed = routeIntent(args.request);
  const projectContext = await getProjectContext();
  const mergedParams = buildMergedParams(args, routed);
  const selectionResolution = resolveParamsFromSelection(mergedParams, projectContext);
  const params = selectionResolution.resolvedParams;
  const contextAdvice = buildContextAwareAdvice(routed, projectContext);
  const workflowPlan = buildWorkflowPlan(routed, params, projectContext);
  const missingFields = routed.match.requiredFields.filter((field) => {
    const value = params[field as keyof typeof params];
    return !hasRequiredValue(value);
  });

  const response: Record<string, unknown> = {
    request: args.request,
    selectedIntent: routed.match.intent,
    selectedTool: routed.match.toolName,
    selectedAction: routed.match.action,
    confidence: routed.confidence,
    reasoning: routed.reasoning,
    params,
    inferredFromSelection: selectionResolution.inferredFromSelection,
    projectContext,
    contextAdvice,
    workflowPlan,
    missingFields,
    canExecute: missingFields.length === 0,
  };

  if (args.execute === true) {
    if (missingFields.length > 0) {
      response.status = "needs_input";
      response.message = `Missing required fields: ${missingFields.join(", ")}`;
      response.clarificationQuestions = workflowPlan.clarificationQuestions;
    } else {
      response.status = "executed";
      response.result = await executeIntent(routed, params);
    }
  } else {
    response.status = missingFields.length > 0 ? "planned_needs_input" : "planned";
    response.clarificationQuestions = workflowPlan.clarificationQuestions;
  }

  return response;
}

export function registerCivil3DOrchestrateTool(server: McpServer) {
  server.tool(
    "civil3d_orchestrate",
    "Routes a natural-language Civil 3D request to the best starting tool or action and can execute a small supported set of actions.",
    Civil3DOrchestrateInputShape,
    async (rawArgs) => {
      try {
        const response = await executeCivil3DOrchestrate(rawArgs);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to execute civil3d_orchestrate";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_orchestrate tool:", error);
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
