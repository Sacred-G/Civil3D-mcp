import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ─── Input Schema ───────────────────────────────────────────
const CatchmentInputShape = {
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
  // Editable catchment properties
  runoffCoefficient: z.number().min(0).max(1).optional(),
  manningsCoefficient: z.number().positive().optional(),
  curveNumber: z.number().min(0).max(100).optional(),
  description: z.string().optional(),
  newName: z.string().optional(),
};

const CatchmentInputSchema = z.object(CatchmentInputShape);
type CatchmentInput = z.infer<typeof CatchmentInputSchema>;

// ─── Action-specific validation schemas ─────────────────────
const ListCatchmentGroupsSchema = CatchmentInputSchema.extend({
  action: z.literal("list_catchment_groups"),
});

const GetCatchmentGroupSchema = CatchmentInputSchema.extend({
  action: z.literal("get_catchment_group"),
  groupName: z.string(),
});

const ListCatchmentsSchema = CatchmentInputSchema.extend({
  action: z.literal("list_catchments"),
});

const GetCatchmentPropertiesSchema = CatchmentInputSchema.extend({
  action: z.literal("get_catchment_properties"),
  catchmentName: z.string(),
});

const SetCatchmentPropertiesSchema = CatchmentInputSchema.extend({
  action: z.literal("set_catchment_properties"),
  catchmentName: z.string(),
});

const CopyCatchmentToGroupSchema = CatchmentInputSchema.extend({
  action: z.literal("copy_catchment_to_group"),
  catchmentName: z.string(),
  targetGroupName: z.string(),
});

const GetCatchmentFlowPathSchema = CatchmentInputSchema.extend({
  action: z.literal("get_catchment_flow_path"),
  catchmentName: z.string(),
});

const GetCatchmentBoundarySchema = CatchmentInputSchema.extend({
  action: z.literal("get_catchment_boundary"),
  catchmentName: z.string(),
});

// ─── Plugin method mapping ──────────────────────────────────
function actionToPluginMethod(action: string): string {
  switch (action) {
    case "list_catchment_groups": return "listCatchmentGroups";
    case "get_catchment_group": return "getCatchmentGroup";
    case "list_catchments": return "listCatchments";
    case "get_catchment_properties": return "getCatchmentProperties";
    case "set_catchment_properties": return "setCatchmentProperties";
    case "copy_catchment_to_group": return "copyCatchmentToGroup";
    case "get_catchment_flow_path": return "getCatchmentFlowPath";
    case "get_catchment_boundary": return "getCatchmentBoundary";
    default: throw new Error(`Unknown catchment action: ${action}`);
  }
}

// ─── Registration ───────────────────────────────────────────
export function registerCivil3DCatchmentTools(server: McpServer) {
  server.tool(
    "civil3d_catchment",
    "Manages Civil 3D catchments and catchment groups. Actions: list_catchment_groups, get_catchment_group, list_catchments, get_catchment_properties, set_catchment_properties, copy_catchment_to_group, get_catchment_flow_path, get_catchment_boundary.",
    CatchmentInputShape,
    async (args: CatchmentInput, _extra) => {
      try {
        // Validate based on action
        let parsedArgs: CatchmentInput;
        switch (args.action) {
          case "list_catchment_groups":
            parsedArgs = ListCatchmentGroupsSchema.parse(args);
            break;
          case "get_catchment_group":
            parsedArgs = GetCatchmentGroupSchema.parse(args);
            break;
          case "list_catchments":
            parsedArgs = ListCatchmentsSchema.parse(args);
            break;
          case "get_catchment_properties":
            parsedArgs = GetCatchmentPropertiesSchema.parse(args);
            break;
          case "set_catchment_properties":
            parsedArgs = SetCatchmentPropertiesSchema.parse(args);
            break;
          case "copy_catchment_to_group":
            parsedArgs = CopyCatchmentToGroupSchema.parse(args);
            break;
          case "get_catchment_flow_path":
            parsedArgs = GetCatchmentFlowPathSchema.parse(args);
            break;
          case "get_catchment_boundary":
            parsedArgs = GetCatchmentBoundarySchema.parse(args);
            break;
          default:
            throw new Error(`Unknown action: ${args.action}`);
        }

        const pluginMethod = actionToPluginMethod(parsedArgs.action);

        // Build params object (exclude action and undefined values)
        const params: Record<string, unknown> = {};
        if (parsedArgs.groupName !== undefined) params.groupName = parsedArgs.groupName;
        if (parsedArgs.catchmentName !== undefined) params.catchmentName = parsedArgs.catchmentName;
        if (parsedArgs.targetGroupName !== undefined) params.targetGroupName = parsedArgs.targetGroupName;
        if (parsedArgs.runoffCoefficient !== undefined) params.runoffCoefficient = parsedArgs.runoffCoefficient;
        if (parsedArgs.manningsCoefficient !== undefined) params.manningsCoefficient = parsedArgs.manningsCoefficient;
        if (parsedArgs.curveNumber !== undefined) params.curveNumber = parsedArgs.curveNumber;
        if (parsedArgs.description !== undefined) params.description = parsedArgs.description;
        if (parsedArgs.newName !== undefined) params.newName = parsedArgs.newName;

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand(pluginMethod, params);
        });

        return {
          content: [
            {
              type: "text" as const,
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_catchment action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_catchment tool:", error);
        return {
          content: [
            {
              type: "text" as const,
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
