import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_detention_basin_size_calculate ──────────────────────────────

export const DetentionBasinSizeInputSchema = z.object({
  inflow: z.number().positive().describe("Peak inflow rate into the detention basin in CFS (from Rational Method or other hydrology calculation)"),
  outflow: z.number().positive().describe("Maximum allowable outflow rate from the detention basin in CFS (pre-development or permitted discharge rate)"),
  stormDuration: z.number().positive().optional().describe("Design storm duration in minutes (default: 60 min). Used with the Modified Rational Method for storage volume."),
  method: z.enum(["modified_rational", "triangular_hydrograph", "scs_curve_number"]).optional().describe("Volume estimation method: 'modified_rational' (default), 'triangular_hydrograph', or 'scs_curve_number'"),
  sideSlope: z.number().positive().optional().describe("Interior basin side slope as H:V ratio (e.g. 3 for 3:1). Default: 3.0"),
  bottomWidth: z.number().nonnegative().optional().describe("Minimum basin bottom width in feet/meters (default: 10.0)"),
  freeboardDepth: z.number().nonnegative().optional().describe("Freeboard depth above design water surface in feet/meters (default: 1.0)"),
  surfaceName: z.string().optional().describe("Optional existing Civil 3D surface to use for stage-storage computation instead of a prismatic basin assumption"),
});

const DetentionBasinSizeInputShape = DetentionBasinSizeInputSchema.shape;

// ─── 2. civil3d_detention_stage_storage ─────────────────────────────────────

export const DetentionStageStorageInputSchema = z.object({
  surfaceName: z.string().describe("Civil 3D surface representing the detention basin grading or existing topography"),
  bottomElevation: z.number().describe("Lowest elevation of the basin (bottom of basin) in drawing units"),
  topElevation: z.number().describe("Maximum water surface elevation (top of active storage) in drawing units"),
  elevationIncrement: z.number().positive().optional().describe("Elevation increment for stage-storage table in drawing units (default: 0.5)"),
  outletType: z.enum(["orifice", "weir", "riser"]).optional().describe("Primary outlet structure type for stage-discharge calculation (default: orifice)"),
  outletDiameter: z.number().positive().optional().describe("Outlet orifice or riser diameter in inches (used for orifice/riser outlet-discharge rating)"),
  weirLength: z.number().positive().optional().describe("Emergency spillway weir length in feet (used for weir outlet-discharge rating)"),
  dischargeCoefficient: z.number().positive().optional().describe("Outlet discharge coefficient. Default: 0.6 for orifice, 3.33 for broad-crested weir"),
});

const DetentionStageStorageInputShape = DetentionStageStorageInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DDetentionTools(server: McpServer) {

  // 1. civil3d_detention_basin_size_calculate
  server.tool(
    "civil3d_detention_basin_size_calculate",
    "Size a detention basin to reduce peak stormwater runoff. Given a peak inflow rate (from Rational Method Q=CiA or SCS CN), a target maximum outflow rate, and a design storm duration, calculates the required storage volume using the Modified Rational Method or triangular hydrograph approximation. Returns required volume, estimated basin dimensions (length, width, depth), outlet orifice size, and freeboard recommendation. Use after HydrologyEstimateRunoff to complete the stormwater design workflow.",
    DetentionBasinSizeInputShape,
    async (args) => {
      try {
        const parsed = DetentionBasinSizeInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateDetentionBasinSize", {
            inflow: parsed.inflow,
            outflow: parsed.outflow,
            stormDuration: parsed.stormDuration ?? 60,
            method: parsed.method ?? "modified_rational",
            sideSlope: parsed.sideSlope ?? 3.0,
            bottomWidth: parsed.bottomWidth ?? 10.0,
            freeboardDepth: parsed.freeboardDepth ?? 1.0,
            surfaceName: parsed.surfaceName ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_detention_basin_size_calculate", error);
      }
    }
  );

  // 2. civil3d_detention_stage_storage
  server.tool(
    "civil3d_detention_stage_storage",
    "Generate a stage-storage-discharge table for a Civil 3D detention basin surface. Samples the surface at a given elevation increment between the bottom and top elevations to compute cumulative storage volume at each stage. Optionally computes an outlet discharge rating curve using orifice or weir equations. Output is a table suitable for routing computations or regulatory submittals.",
    DetentionStageStorageInputShape,
    async (args) => {
      try {
        const parsed = DetentionStageStorageInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateDetentionStageStorage", {
            surfaceName: parsed.surfaceName,
            bottomElevation: parsed.bottomElevation,
            topElevation: parsed.topElevation,
            elevationIncrement: parsed.elevationIncrement ?? 0.5,
            outletType: parsed.outletType ?? "orifice",
            outletDiameter: parsed.outletDiameter ?? null,
            weirLength: parsed.weirLength ?? null,
            dischargeCoefficient: parsed.dischargeCoefficient ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_detention_stage_storage", error);
      }
    }
  );
}
