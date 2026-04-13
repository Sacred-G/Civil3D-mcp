import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { routeIntent, type RouteParams, type RouteResult } from "../orchestration/IntentRouter.js";
import { findToolCatalogEntry } from "../orchestration/ToolCatalog.js";
import { getProjectContext } from "../orchestration/ProjectContextService.js";
import { buildWorkflowPlan } from "../orchestration/WorkflowPlanner.js";
import { resolveParamsFromSelection } from "../orchestration/SelectionResolver.js";
import { executeRegisteredTool } from "./toolHandlerRegistry.js";

const Civil3DOrchestrateInputShape = {
  request: z.string().min(1).optional().describe("Natural-language Civil 3D request."),
  execute: z.boolean().optional().describe("When true, execute the selected action if enough parameters are available."),
  toolName: z.string().optional().describe("Exact registered tool name to plan or execute through the orchestrator."),
  toolAction: z.string().optional().describe("Optional exact action when targeting a multi-action tool directly."),
  toolParameters: z.record(z.unknown()).optional().describe("Optional exact tool parameters when targeting a tool directly."),
  name: z.string().optional(),
  alignmentName: z.string().optional(),
  corridorName: z.string().optional(),
  profileName: z.string().optional(),
  surfaceName: z.string().optional(),
  projectFolder: z.string().optional(),
  shortcutName: z.string().optional(),
  shortcutType: z.string().optional(),
  templatePath: z.string().optional(),
  saveAs: z.string().optional(),
  limit: z.number().optional(),
  layerPrefix: z.string().optional(),
  networkName: z.string().optional(),
  pipeName: z.string().optional(),
  structureName: z.string().optional(),
  fittingName: z.string().optional(),
  partName: z.string().optional(),
  partsList: z.string().optional(),
  targetType: z.string().optional(),
  targetName: z.string().optional(),
  targetNetwork: z.string().optional(),
  sourceNetwork: z.string().optional(),
  newPartName: z.string().optional(),
  startPoint: z.object({ x: z.number(), y: z.number(), z: z.number().optional() }).optional(),
  endPoint: z.object({ x: z.number(), y: z.number(), z: z.number().optional() }).optional(),
  position: z.object({ x: z.number(), y: z.number(), z: z.number().optional() }).optional(),
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
  minCoverDepth: z.number().optional(),
  maxCoverDepth: z.number().optional(),
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

const ROUTE_PARAM_KEYS: Array<keyof RouteParams> = [
  "name",
  "alignmentName",
  "corridorName",
  "profileName",
  "surfaceName",
  "projectFolder",
  "shortcutName",
  "shortcutType",
  "templatePath",
  "saveAs",
  "limit",
  "layerPrefix",
  "networkName",
  "pipeName",
  "structureName",
  "fittingName",
  "partName",
  "partsList",
  "targetType",
  "targetName",
  "targetNetwork",
  "sourceNetwork",
  "newPartName",
  "startPoint",
  "endPoint",
  "position",
  "baseSurface",
  "comparisonSurface",
  "style",
  "layer",
  "labelSet",
  "filePath",
  "outputPath",
  "gridSpacing",
  "designSpeed",
  "inflow",
  "outflow",
  "bottomElevation",
  "topElevation",
  "minCoverDepth",
  "maxCoverDepth",
  "payItems",
];

function pickRouteParams(
  source?: Record<string, unknown>,
): Partial<RouteParams> {
  if (!source) {
    return {};
  }

  return Object.fromEntries(
    ROUTE_PARAM_KEYS
      .filter((key) => source[key] !== undefined)
      .map((key) => [key, source[key]]),
  ) as Partial<RouteParams>;
}

function buildMergedParams(args: OrchestrateArgs, routed: ReturnType<typeof routeIntent>) {
  return {
    name: args.name ?? routed.extractedParams.name,
    alignmentName: args.alignmentName ?? routed.extractedParams.alignmentName,
    corridorName: args.corridorName ?? routed.extractedParams.corridorName,
    profileName: args.profileName ?? routed.extractedParams.profileName,
    surfaceName: args.surfaceName ?? routed.extractedParams.surfaceName,
    projectFolder: args.projectFolder ?? routed.extractedParams.projectFolder,
    shortcutName: args.shortcutName ?? routed.extractedParams.shortcutName,
    shortcutType: args.shortcutType ?? routed.extractedParams.shortcutType,
    templatePath: args.templatePath ?? routed.extractedParams.templatePath,
    saveAs: args.saveAs ?? routed.extractedParams.saveAs,
    limit: args.limit ?? routed.extractedParams.limit,
    layerPrefix: args.layerPrefix ?? routed.extractedParams.layerPrefix,
    networkName: args.networkName ?? routed.extractedParams.networkName,
    pipeName: args.pipeName ?? routed.extractedParams.pipeName,
    structureName: args.structureName ?? routed.extractedParams.structureName,
    fittingName: args.fittingName ?? routed.extractedParams.fittingName,
    partName: args.partName ?? routed.extractedParams.partName,
    partsList: args.partsList ?? routed.extractedParams.partsList,
    targetType: args.targetType ?? routed.extractedParams.targetType,
    targetName: args.targetName ?? routed.extractedParams.targetName,
    targetNetwork: args.targetNetwork ?? routed.extractedParams.targetNetwork,
    sourceNetwork: args.sourceNetwork ?? routed.extractedParams.sourceNetwork,
    newPartName: args.newPartName ?? routed.extractedParams.newPartName,
    startPoint: args.startPoint ?? routed.extractedParams.startPoint,
    endPoint: args.endPoint ?? routed.extractedParams.endPoint,
    position: args.position ?? routed.extractedParams.position,
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
    minCoverDepth: args.minCoverDepth ?? routed.extractedParams.minCoverDepth,
    maxCoverDepth: args.maxCoverDepth ?? routed.extractedParams.maxCoverDepth,
    payItems: args.payItems ?? routed.extractedParams.payItems,
  };
}

function buildDirectParams(args: OrchestrateArgs): RouteParams {
  const extractedRequestParams = args.request ? routeIntent(args.request).extractedParams : {};

  return {
    ...extractedRequestParams,
    name: args.name ?? extractedRequestParams.name,
    alignmentName: args.alignmentName ?? extractedRequestParams.alignmentName,
    corridorName: args.corridorName ?? extractedRequestParams.corridorName,
    profileName: args.profileName ?? extractedRequestParams.profileName,
    surfaceName: args.surfaceName ?? extractedRequestParams.surfaceName,
    projectFolder: args.projectFolder ?? extractedRequestParams.projectFolder,
    shortcutName: args.shortcutName ?? extractedRequestParams.shortcutName,
    shortcutType: args.shortcutType ?? extractedRequestParams.shortcutType,
    templatePath: args.templatePath ?? extractedRequestParams.templatePath,
    saveAs: args.saveAs ?? extractedRequestParams.saveAs,
    limit: args.limit ?? extractedRequestParams.limit,
    layerPrefix: args.layerPrefix ?? extractedRequestParams.layerPrefix,
    networkName: args.networkName ?? extractedRequestParams.networkName,
    pipeName: args.pipeName ?? extractedRequestParams.pipeName,
    structureName: args.structureName ?? extractedRequestParams.structureName,
    fittingName: args.fittingName ?? extractedRequestParams.fittingName,
    partName: args.partName ?? extractedRequestParams.partName,
    partsList: args.partsList ?? extractedRequestParams.partsList,
    targetType: args.targetType ?? extractedRequestParams.targetType,
    targetName: args.targetName ?? extractedRequestParams.targetName,
    targetNetwork: args.targetNetwork ?? extractedRequestParams.targetNetwork,
    sourceNetwork: args.sourceNetwork ?? extractedRequestParams.sourceNetwork,
    newPartName: args.newPartName ?? extractedRequestParams.newPartName,
    startPoint: args.startPoint ?? extractedRequestParams.startPoint,
    endPoint: args.endPoint ?? extractedRequestParams.endPoint,
    position: args.position ?? extractedRequestParams.position,
    baseSurface: args.baseSurface ?? extractedRequestParams.baseSurface,
    comparisonSurface: args.comparisonSurface ?? extractedRequestParams.comparisonSurface,
    style: args.style ?? extractedRequestParams.style,
    layer: args.layer ?? extractedRequestParams.layer,
    labelSet: args.labelSet ?? extractedRequestParams.labelSet,
    filePath: args.filePath ?? extractedRequestParams.filePath,
    outputPath: args.outputPath ?? extractedRequestParams.outputPath,
    gridSpacing: args.gridSpacing ?? extractedRequestParams.gridSpacing,
    designSpeed: args.designSpeed ?? extractedRequestParams.designSpeed,
    inflow: args.inflow ?? extractedRequestParams.inflow,
    outflow: args.outflow ?? extractedRequestParams.outflow,
    bottomElevation: args.bottomElevation ?? extractedRequestParams.bottomElevation,
    topElevation: args.topElevation ?? extractedRequestParams.topElevation,
    minCoverDepth: args.minCoverDepth ?? extractedRequestParams.minCoverDepth,
    maxCoverDepth: args.maxCoverDepth ?? extractedRequestParams.maxCoverDepth,
    payItems: args.payItems ?? extractedRequestParams.payItems,
    ...pickRouteParams(args.toolParameters),
  };
}

function buildDirectRoute(args: OrchestrateArgs): RouteResult {
  const requestedToolName = args.toolName;
  if (!requestedToolName) {
    throw new Error("toolName is required for direct tool orchestration.");
  }

  const requestedAction =
    args.toolAction
    ?? (typeof args.toolParameters?.action === "string"
      ? String(args.toolParameters.action)
      : undefined);

  const match = findToolCatalogEntry(requestedToolName, requestedAction);
  if (!match) {
    throw new Error(
      requestedAction
        ? `Tool '${requestedToolName}' with action '${requestedAction}' was not found in the tool catalog.`
        : `Tool '${requestedToolName}' was not found in the tool catalog.`,
    );
  }

  return {
    match,
    confidence: 1,
    missingFields: [],
    extractedParams: {},
    reasoning: requestedAction
      ? `Used exact tool override for '${requestedToolName}' action '${requestedAction}'.`
      : `Used exact tool override for '${requestedToolName}'.`,
  };
}

function findMissingRequiredFields(
  requiredFields: string[],
  params: Record<string, unknown> | RouteParams,
) {
  const paramRecord = params as Record<string, unknown>;
  return requiredFields.filter((field) => !hasRequiredValue(paramRecord[field]));
}

function buildDirectExecutionContext(args: OrchestrateArgs) {
  const routed = buildDirectRoute(args);
  const params = buildDirectParams(args);
  const exactParameters = buildExactToolParameters(args, params, routed.match.action);
  const missingFields = findMissingRequiredFields(routed.match.requiredFields, exactParameters);

  return {
    routed,
    params,
    exactParameters,
    missingFields,
  };
}

function buildExactToolParameters(
  args: OrchestrateArgs,
  params: RouteParams,
  selectedAction: string,
): Record<string, unknown> {
  const parameterObject = args.toolParameters ?? {};
  const explicitAction =
    args.toolAction
    ?? (typeof parameterObject.action === "string" ? String(parameterObject.action) : undefined)
    ?? (selectedAction !== "execute" ? selectedAction : undefined);

  const exactParameters = {
    ...(explicitAction ? { action: explicitAction } : {}),
    ...params,
    ...parameterObject,
  };

  return Object.fromEntries(
    Object.entries(exactParameters).filter(([, value]) => value !== undefined),
  );
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
  if (intent.match.intent === "corridor_prerequisites") {
    return buildCorridorAdvice();
  }

  const toolArgs = intent.match.buildToolArgs(params as unknown as Record<string, unknown>);
  return await executeRegisteredToolDirectly(intent.match.toolName, toolArgs);
}

async function executeRegisteredToolDirectly(
  toolName: string,
  parameters: Record<string, unknown>,
) {
  return await executeRegisteredTool(toolName, parameters);
}

export async function executeToolCallViaOrchestrator(
  toolName: string,
  parameters: Record<string, unknown>,
) {
  const directExecution = buildDirectExecutionContext({
    toolName,
    toolAction: typeof parameters.action === "string"
      ? String(parameters.action)
      : undefined,
    toolParameters: parameters,
    ...pickRouteParams(parameters),
  });

  if (directExecution.missingFields.length > 0) {
    throw new Error(
      `Missing required fields: ${directExecution.missingFields.join(", ")}`,
    );
  }

  return await executeRegisteredToolDirectly(
    directExecution.routed.match.toolName,
    directExecution.exactParameters,
  );
}

export async function executeCivil3DOrchestrate(rawArgs: OrchestrateArgs) {
  const args = Civil3DOrchestrateInputSchema.parse(rawArgs);
  if (!args.request && !args.toolName) {
    throw new Error("civil3d_orchestrate requires either a request or a toolName.");
  }

  const directExecution = args.toolName ? buildDirectExecutionContext(args) : null;
  const routed = directExecution?.routed ?? routeIntent(args.request as string);
  const projectContext = await getProjectContext();
  const mergedParams = directExecution?.params ?? buildMergedParams(args, routed);
  const selectionResolution = resolveParamsFromSelection(mergedParams, projectContext);
  const params = selectionResolution.resolvedParams;
  const contextAdvice = buildContextAwareAdvice(routed, projectContext);
  const workflowPlan = buildWorkflowPlan(routed, params, projectContext);
  const missingFields = findMissingRequiredFields(
    routed.match.requiredFields,
    directExecution?.exactParameters ?? params,
  );

  const response: Record<string, unknown> = {
    request: args.request ?? `direct:${args.toolName}`,
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
      response.result = directExecution
        ? await executeRegisteredToolDirectly(
          routed.match.toolName,
          directExecution.exactParameters,
        )
        : await executeIntent(routed, params);
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
    "Routes a natural-language Civil 3D request to the best starting tool or action and executes routed work through the registered MCP tool surface.",
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
