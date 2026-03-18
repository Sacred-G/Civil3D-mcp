import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_assembly_create ─────────────────────────────────────────────

export const AssemblyCreateInputSchema = z.object({
  name: z.string().describe("Name for the new assembly"),
  insertionPoint: z.object({
    x: z.number(),
    y: z.number(),
  }).optional().describe("Model space XY coordinate to insert the assembly marker (defaults to 0,0)"),
  description: z.string().optional().describe("Optional description for the assembly"),
});

const AssemblyCreateInputShape = AssemblyCreateInputSchema.shape;

// ─── 2. civil3d_subassembly_create ──────────────────────────────────────────

export const SubassemblyCreateInputSchema = z.object({
  assemblyName: z.string().describe("Name of the existing assembly to add the subassembly to"),
  subassemblyType: z.string().describe("Subassembly catalog type to add. Common types: 'LaneOutsideSuper', 'LaneInsideSuper', 'BasicLane', 'BasicShoulder', 'BasicSideSlopeCutDitch', 'BasicGuardrail', 'DaylightStandard', 'DaylightBench', 'LinkSlopeToSurface', 'LinkWidthAndSlope'"),
  side: z.enum(["Left", "Right", "Both"]).describe("Side of the baseline to place the subassembly"),
  parameters: z.record(z.union([z.string(), z.number(), z.boolean()])).optional().describe("Subassembly parameter overrides as key-value pairs. Common params: { width: 12, slope: -0.02, depth: 0.5 }"),
});

const SubassemblyCreateInputShape = SubassemblyCreateInputSchema.shape;

// ─── 3. civil3d_assembly_edit ───────────────────────────────────────────────

export const AssemblyEditInputSchema = z.object({
  assemblyName: z.string().describe("Name of the assembly to modify"),
  subassemblyName: z.string().optional().describe("Name of a specific subassembly to edit. If omitted, lists all subassemblies."),
  parameters: z.record(z.union([z.string(), z.number(), z.boolean()])).optional().describe("Parameter values to update on the target subassembly as key-value pairs"),
  delete: z.boolean().optional().describe("If true and subassemblyName is provided, delete that subassembly from the assembly"),
});

const AssemblyEditInputShape = AssemblyEditInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DAssemblyCreationTools(server: McpServer) {

  // 1. civil3d_assembly_create
  server.tool(
    "civil3d_assembly_create",
    "Create a new Civil 3D assembly at a specified model-space location. An assembly is a container for subassemblies that defines the cross-section geometry used by a corridor. After creating an assembly, use civil3d_subassembly_create to add lane, shoulder, ditch, and daylight subassemblies to it.",
    AssemblyCreateInputShape,
    async (args) => {
      try {
        const parsed = AssemblyCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createAssembly", {
            name: parsed.name,
            insertX: parsed.insertionPoint?.x ?? 0,
            insertY: parsed.insertionPoint?.y ?? 0,
            description: parsed.description ?? "",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_assembly_create", error);
      }
    }
  );

  // 2. civil3d_subassembly_create
  server.tool(
    "civil3d_subassembly_create",
    "Add a subassembly from the Civil 3D catalog to an existing assembly. Subassemblies define individual cross-section components: travel lanes, shoulders, ditches, slopes, and daylight links. Common types: BasicLane (travel lane), BasicShoulder (paved shoulder), BasicSideSlopeCutDitch (cut ditch with 1:1 or 2:1 slope), DaylightStandard (standard daylight to a surface), LinkWidthAndSlope (parametric link). Parameters control width, slope, depth, and geometry.",
    SubassemblyCreateInputShape,
    async (args) => {
      try {
        const parsed = SubassemblyCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSubassembly", {
            assemblyName: parsed.assemblyName,
            subassemblyType: parsed.subassemblyType,
            side: parsed.side,
            parameters: parsed.parameters ?? {},
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_subassembly_create", error);
      }
    }
  );

  // 3. civil3d_assembly_edit
  server.tool(
    "civil3d_assembly_edit",
    "Inspect or modify an existing Civil 3D assembly. Can list all subassemblies with their current parameters, update specific parameter values on a subassembly (e.g., change lane width from 12 to 14 ft), or delete a subassembly from the assembly. After editing, the corridor must be rebuilt with civil3d_corridor (action: rebuild) to apply changes.",
    AssemblyEditInputShape,
    async (args) => {
      try {
        const parsed = AssemblyEditInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("editAssembly", {
            assemblyName: parsed.assemblyName,
            subassemblyName: parsed.subassemblyName ?? null,
            parameters: parsed.parameters ?? null,
            delete: parsed.delete ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_assembly_edit", error);
      }
    }
  );
}
