import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_pipe_network_hgl_calculate ──────────────────────────────────

export const PipeHglInputSchema = z.object({
  networkName: z.string().describe("Name of the gravity pipe network to analyze"),
  tailwaterElevation: z.number().optional().describe("Tailwater elevation at the outlet structure in drawing units (defaults to outlet invert elevation if omitted)"),
  designFlow: z.number().optional().describe("Design flow rate in CFS to use for HGL computation (if not provided, uses full-pipe capacity)"),
  manningsN: z.number().positive().optional().describe("Manning's n roughness coefficient override for all pipes (default: 0.013 for concrete)"),
});

const PipeHglInputShape = PipeHglInputSchema.shape;

// ─── 2. civil3d_pipe_hydraulic_analysis ─────────────────────────────────────

export const PipeHydraulicAnalysisInputSchema = z.object({
  networkName: z.string().describe("Name of the gravity pipe network to analyze"),
  designFlow: z.number().optional().describe("Design flow in CFS for capacity analysis. If omitted, full-pipe capacity is reported."),
  manningsN: z.number().positive().optional().describe("Manning's n roughness coefficient (default: 0.013 for concrete, 0.024 for corrugated metal)"),
  minCoverDepth: z.number().nonnegative().optional().describe("Minimum required cover depth in drawing units (default: 2.0)"),
  minVelocity: z.number().nonnegative().optional().describe("Minimum acceptable flow velocity in ft/s or m/s (default: 2.0)"),
  maxVelocity: z.number().positive().optional().describe("Maximum acceptable flow velocity in ft/s or m/s (default: 10.0)"),
  minSlope: z.number().nonnegative().optional().describe("Minimum acceptable pipe slope in percent (default: 0.5%)"),
});

const PipeHydraulicAnalysisInputShape = PipeHydraulicAnalysisInputSchema.shape;

// ─── 3. civil3d_pipe_structure_properties ───────────────────────────────────

export const PipeStructurePropertiesInputSchema = z.object({
  networkName: z.string().describe("Name of the pipe network containing the structure"),
  structureName: z.string().describe("Name of the structure to inspect"),
});

const PipeStructurePropertiesInputShape = PipeStructurePropertiesInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DPipeHydraulicsTools(server: McpServer) {

  // 1. civil3d_pipe_network_hgl_calculate
  server.tool(
    "civil3d_pipe_network_hgl_calculate",
    "Calculate the Hydraulic Grade Line (HGL) and Energy Grade Line (EGL) for a Civil 3D gravity pipe network. Performs backwater analysis from the outlet upstream, reporting HGL elevation, pipe full/partial flow status, and surcharged structures at each node. Use before completing any stormwater drainage design.",
    PipeHglInputShape,
    async (args) => {
      try {
        const parsed = PipeHglInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculatePipeNetworkHgl", {
            networkName: parsed.networkName,
            tailwaterElevation: parsed.tailwaterElevation ?? null,
            designFlow: parsed.designFlow ?? null,
            manningsN: parsed.manningsN ?? 0.013,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pipe_network_hgl_calculate", error);
      }
    }
  );

  // 2. civil3d_pipe_hydraulic_analysis
  server.tool(
    "civil3d_pipe_hydraulic_analysis",
    "Run a full hydraulic capacity analysis on a Civil 3D gravity pipe network using Manning's equation. Reports pipe capacity (CFS), design flow vs capacity ratio, flow velocity, Froude number, and flags cover violations, low-velocity sedimentation risk, and high-velocity erosion risk for every pipe. Returns a per-pipe and per-structure summary.",
    PipeHydraulicAnalysisInputShape,
    async (args) => {
      try {
        const parsed = PipeHydraulicAnalysisInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("analyzePipeNetworkHydraulics", {
            networkName: parsed.networkName,
            designFlow: parsed.designFlow ?? null,
            manningsN: parsed.manningsN ?? 0.013,
            minCoverDepth: parsed.minCoverDepth ?? 2.0,
            minVelocity: parsed.minVelocity ?? 2.0,
            maxVelocity: parsed.maxVelocity ?? 10.0,
            minSlope: parsed.minSlope ?? 0.5,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pipe_hydraulic_analysis", error);
      }
    }
  );

  // 3. civil3d_pipe_structure_properties
  server.tool(
    "civil3d_pipe_structure_properties",
    "Retrieve detailed properties of a single structure (manhole, inlet, headwall, junction) in a Civil 3D gravity pipe network. Returns rim elevation, sump elevation, barrel count, connected pipe inverts, structure depth, and hydraulic head loss coefficient.",
    PipeStructurePropertiesInputShape,
    async (args) => {
      try {
        const parsed = PipeStructurePropertiesInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getPipeStructureProperties", {
            networkName: parsed.networkName,
            structureName: parsed.structureName,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pipe_structure_properties", error);
      }
    }
  );
}
