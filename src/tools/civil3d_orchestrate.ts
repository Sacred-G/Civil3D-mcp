import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";
import { routeIntent } from "../orchestration/IntentRouter.js";
import { getProjectContext } from "../orchestration/ProjectContextService.js";
import { buildWorkflowPlan } from "../orchestration/WorkflowPlanner.js";
import { resolveParamsFromSelection } from "../orchestration/SelectionResolver.js";
import { RouteParams } from "../orchestration/IntentRouter.js";

const Civil3DOrchestrateInputShape = {
  request: z.string().min(1).describe("Natural-language Civil 3D request."),
  execute: z.boolean().optional().describe("When true, execute the selected action if enough parameters are available."),
  name: z.string().optional(),
  alignmentName: z.string().optional(),
  profileName: z.string().optional(),
  surfaceName: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
};

const Civil3DOrchestrateInputSchema = z.object(Civil3DOrchestrateInputShape);

type OrchestrateArgs = z.infer<typeof Civil3DOrchestrateInputSchema>;

function buildMergedParams(args: OrchestrateArgs, routed: ReturnType<typeof routeIntent>) {
  return {
    name: args.name ?? routed.extractedParams.name,
    alignmentName: args.alignmentName ?? routed.extractedParams.alignmentName,
    profileName: args.profileName ?? routed.extractedParams.profileName,
    surfaceName: args.surfaceName ?? routed.extractedParams.surfaceName,
    style: args.style ?? routed.extractedParams.style,
    layer: args.layer ?? routed.extractedParams.layer,
    labelSet: args.labelSet ?? routed.extractedParams.labelSet,
  };
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

  return {
    prerequisitesSatisfied: true,
    suggestions: [],
  };
}

async function executeIntent(intent: ReturnType<typeof routeIntent>, params: RouteParams) {
  switch (intent.match.intent) {
    case "drawing_info":
      return await withApplicationConnection((appClient) => appClient.sendCommand("getDrawingInfo", {}));
    case "list_surfaces":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listSurfaces", {}));
    case "create_surface":
      return await withApplicationConnection((appClient) =>
        appClient.sendCommand("createSurface", {
          name: params.name,
          style: params.style,
          layer: params.layer,
        })
      );
    case "list_alignments":
      return await withApplicationConnection((appClient) => appClient.sendCommand("listAlignments", {}));
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
    case "corridor_prerequisites":
      return buildCorridorAdvice();
  }
}

export function registerCivil3DOrchestrateTool(server: McpServer) {
  server.tool(
    "civil3d_orchestrate",
    "Routes a natural-language Civil 3D request to the best starting tool or action and can execute a small supported set of actions.",
    Civil3DOrchestrateInputShape,
    async (rawArgs) => {
      try {
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
          return typeof value !== "string" || value.trim().length === 0;
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
