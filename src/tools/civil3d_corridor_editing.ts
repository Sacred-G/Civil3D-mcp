import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_corridor_target_mapping_get ─────────────────────────────────

export const CorridorTargetMappingGetInputSchema = z.object({
  corridorName: z.string().describe("Name of the corridor to retrieve target mappings for"),
  regionIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline region to get targets for (omit for all regions)"),
  baselineIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline (omit for the first baseline)"),
});

// ─── 2. civil3d_corridor_target_mapping_set ─────────────────────────────────

export const CorridorTargetMappingSetInputSchema = z.object({
  corridorName: z.string().describe("Name of the corridor to update target mappings on"),
  regionIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline region to update (default: 0)"),
  baselineIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline (default: 0)"),
  targets: z.array(z.object({
    parameterName: z.string().describe("Name of the subassembly target parameter (e.g., 'Right Lane Width', 'Daylight Surface')"),
    targetType: z.enum(["surface", "alignment", "profile", "polyline"]).describe("Type of Civil 3D object being set as the target"),
    targetName: z.string().describe("Name of the target object (surface, alignment, profile, or polyline)"),
  })).describe("Array of target assignments to apply to the corridor region"),
});

// ─── 3. civil3d_corridor_region_add ─────────────────────────────────────────

export const CorridorRegionAddInputSchema = z.object({
  corridorName: z.string().describe("Name of the corridor to add a region to"),
  baselineIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline to add the region to (default: 0)"),
  assemblyName: z.string().describe("Name of the assembly to use for the new region"),
  startStation: z.number().describe("Start station of the new region along the baseline alignment"),
  endStation: z.number().describe("End station of the new region along the baseline alignment"),
  frequency: z.number().positive().optional().describe("Station frequency (sampling interval) for the new region in drawing units (default: 10)"),
  frequencyAtCurves: z.number().positive().optional().describe("Station frequency at horizontal curves for the new region (default: same as frequency)"),
  frequencyAtKneePoints: z.number().positive().optional().describe("Station frequency at vertical curve high/low points (default: same as frequency)"),
});

// ─── 4. civil3d_corridor_region_delete ──────────────────────────────────────

export const CorridorRegionDeleteInputSchema = z.object({
  corridorName: z.string().describe("Name of the corridor to delete a region from"),
  baselineIndex: z.number().int().nonnegative().optional().describe("Zero-based index of the baseline (default: 0)"),
  regionIndex: z.number().int().nonnegative().describe("Zero-based index of the region to delete within the specified baseline"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DCorridorEditingTools(server: McpServer) {

  // 1. civil3d_corridor_target_mapping_get
  server.tool(
    "civil3d_corridor_target_mapping_get",
    "Retrieve the current subassembly target mappings for a Civil 3D corridor. Returns all target parameters for each baseline region — including daylight surfaces, width targets, slope targets, and alignment/profile targets.",
    CorridorTargetMappingGetInputSchema.shape,
    async (args) => {
      try {
        const parsed = CorridorTargetMappingGetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getCorridorTargetMappings", {
            corridorName: parsed.corridorName,
            regionIndex: parsed.regionIndex ?? null,
            baselineIndex: parsed.baselineIndex ?? 0,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_corridor_target_mapping_get", error);
      }
    }
  );

  // 2. civil3d_corridor_target_mapping_set
  server.tool(
    "civil3d_corridor_target_mapping_set",
    "Set or update subassembly target mappings on a Civil 3D corridor region. Assigns surfaces, alignments, profiles, or polylines as targets for subassembly parameters (e.g., daylight surface, lane width target). Rebuilds the corridor after applying.",
    CorridorTargetMappingSetInputSchema.shape,
    async (args) => {
      try {
        const parsed = CorridorTargetMappingSetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setCorridorTargetMappings", {
            corridorName: parsed.corridorName,
            regionIndex: parsed.regionIndex ?? 0,
            baselineIndex: parsed.baselineIndex ?? 0,
            targets: parsed.targets,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_corridor_target_mapping_set", error);
      }
    }
  );

  // 3. civil3d_corridor_region_add
  server.tool(
    "civil3d_corridor_region_add",
    "Add a new region to a Civil 3D corridor baseline. Regions define which assembly applies over a station range and at what sampling frequency. Use to apply different assemblies in different parts of the corridor (e.g., full section on tangent, reduced section in cut).",
    CorridorRegionAddInputSchema.shape,
    async (args) => {
      try {
        const parsed = CorridorRegionAddInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addCorridorRegion", {
            corridorName: parsed.corridorName,
            baselineIndex: parsed.baselineIndex ?? 0,
            assemblyName: parsed.assemblyName,
            startStation: parsed.startStation,
            endStation: parsed.endStation,
            frequency: parsed.frequency ?? 10,
            frequencyAtCurves: parsed.frequencyAtCurves ?? null,
            frequencyAtKneePoints: parsed.frequencyAtKneePoints ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_corridor_region_add", error);
      }
    }
  );

  // 4. civil3d_corridor_region_delete
  server.tool(
    "civil3d_corridor_region_delete",
    "Delete a region from a Civil 3D corridor baseline by its zero-based index. Rebuilds the corridor after deletion.",
    CorridorRegionDeleteInputSchema.shape,
    async (args) => {
      try {
        const parsed = CorridorRegionDeleteInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("deleteCorridorRegion", {
            corridorName: parsed.corridorName,
            baselineIndex: parsed.baselineIndex ?? 0,
            regionIndex: parsed.regionIndex,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_corridor_region_delete", error);
      }
    }
  );
}
