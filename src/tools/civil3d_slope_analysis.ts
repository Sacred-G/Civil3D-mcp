import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_slope_geometry_calculate ────────────────────────────────────

export const SlopeGeometryInputSchema = z.object({
  alignmentName: z.string().optional().describe("Name of the alignment to calculate daylight lines along. If omitted, uses the first alignment in the drawing."),
  profileName: z.string().optional().describe("Name of the design profile. Defaults to the first finished-grade profile on the alignment."),
  surfaceName: z.string().optional().describe("Name of the existing ground surface to daylight to. Defaults to the first surface."),
  cutSlopeRatio: z.number().positive().optional().describe("Cut slope as H:V ratio (e.g. 2 for 2:1 cut). Default: 2.0"),
  fillSlopeRatio: z.number().positive().optional().describe("Fill slope as H:V ratio (e.g. 3 for 3:1 fill). Default: 3.0"),
  benchWidth: z.number().nonnegative().optional().describe("Bench width in drawing units at each bench interval (0 = no benching). Default: 0"),
  benchHeightInterval: z.number().positive().optional().describe("Vertical height interval between benches in drawing units (default: 20). Only used when benchWidth > 0."),
  stationStart: z.number().optional().describe("Start station for daylight line calculation"),
  stationEnd: z.number().optional().describe("End station for daylight line calculation"),
  stationInterval: z.number().positive().optional().describe("Station interval for sampling in drawing units (default: 10)"),
  roadwayWidth: z.number().nonnegative().optional().describe("Half-width of the roadway template (subgrade) in drawing units used as the catch-point offset baseline"),
});

const SlopeGeometryInputShape = SlopeGeometryInputSchema.shape;

// ─── 2. civil3d_slope_stability_check ───────────────────────────────────────

export const SlopeStabilityCheckInputSchema = z.object({
  alignmentName: z.string().optional().describe("Name of the alignment to check slopes along"),
  surfaceName: z.string().optional().describe("Name of the terrain surface"),
  maxCutSlopeRatio: z.number().positive().optional().describe("Maximum allowable cut slope as H:V (e.g. 1.5 for 1.5:1). Steeper slopes will be flagged. Default: 1.5"),
  maxFillSlopeRatio: z.number().positive().optional().describe("Maximum allowable fill slope as H:V (e.g. 2 for 2:1). Default: 2.0"),
  maxCutHeight: z.number().positive().optional().describe("Maximum allowable cut height in drawing units before benching is required (default: 30)"),
  maxFillHeight: z.number().positive().optional().describe("Maximum allowable fill height in drawing units before additional analysis is required (default: 40)"),
  stationInterval: z.number().positive().optional().describe("Station interval for checking in drawing units (default: 25)"),
  soilType: z.enum(["granular", "cohesive", "rock", "mixed"]).optional().describe("Predominant soil type — affects slope stability assessment. Default: 'granular'"),
});

const SlopeStabilityCheckInputShape = SlopeStabilityCheckInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DSlopeAnalysisTools(server: McpServer) {

  // 1. civil3d_slope_geometry_calculate
  server.tool(
    "civil3d_slope_geometry_calculate",
    "Calculate daylight line coordinates and slope geometry for cut and fill sections along a Civil 3D alignment. Uses the design profile elevation above/below the existing ground surface to determine catch-point locations for standard cut and fill slopes (e.g. 2:1 cut, 3:1 fill). Supports benched slopes for tall cuts. Returns a station-by-station table with catch-point offsets, slope heights, and cut/fill flag for each side. Use this to validate grading limits before building a corridor.",
    SlopeGeometryInputShape,
    async (args) => {
      try {
        const parsed = SlopeGeometryInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateSlopeGeometry", {
            alignmentName: parsed.alignmentName ?? null,
            profileName: parsed.profileName ?? null,
            surfaceName: parsed.surfaceName ?? null,
            cutSlopeRatio: parsed.cutSlopeRatio ?? 2.0,
            fillSlopeRatio: parsed.fillSlopeRatio ?? 3.0,
            benchWidth: parsed.benchWidth ?? 0,
            benchHeightInterval: parsed.benchHeightInterval ?? 20,
            stationStart: parsed.stationStart ?? null,
            stationEnd: parsed.stationEnd ?? null,
            stationInterval: parsed.stationInterval ?? 10,
            roadwayWidth: parsed.roadwayWidth ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_slope_geometry_calculate", error);
      }
    }
  );

  // 2. civil3d_slope_stability_check
  server.tool(
    "civil3d_slope_stability_check",
    "Evaluate cut and fill slope stability along a Civil 3D alignment. Checks corridor or grading slopes against user-specified maximum allowable ratios and heights, flags stations where slopes exceed limits, and identifies locations where benching may be required. Reports cut height, fill height, and actual slope ratio at each station. Flags are categorized as: OK, WARN (approaching limit), or FAIL (exceeds limit). Use as a geotechnical pre-check before finalizing grading design.",
    SlopeStabilityCheckInputShape,
    async (args) => {
      try {
        const parsed = SlopeStabilityCheckInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("checkSlopeStability", {
            alignmentName: parsed.alignmentName ?? null,
            surfaceName: parsed.surfaceName ?? null,
            maxCutSlopeRatio: parsed.maxCutSlopeRatio ?? 1.5,
            maxFillSlopeRatio: parsed.maxFillSlopeRatio ?? 2.0,
            maxCutHeight: parsed.maxCutHeight ?? 30,
            maxFillHeight: parsed.maxFillHeight ?? 40,
            stationInterval: parsed.stationInterval ?? 25,
            soilType: parsed.soilType ?? "granular",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_slope_stability_check", error);
      }
    }
  );
}
