import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_superelevation_get ──────────────────────────────────────────

export const SuperelevationGetInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment to retrieve superelevation data from"),
  includeRawData: z.boolean().optional().describe("Include the raw superelevation table with station-by-station values (default: false)"),
});

// ─── 2. civil3d_superelevation_set ──────────────────────────────────────────

export const SuperelevationSetInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment to apply superelevation to"),
  designSpeed: z.number().positive().describe("Design speed in km/h (or mph if drawing units are imperial)"),
  normalCrownSlope: z.number().describe("Normal crown slope in percent (e.g., -2.0 for 2% cross-fall). Negative = slopes away from centerline."),
  attainmentMethod: z
    .enum(["AASHTO_2001", "AASHTO_2011", "manual"])
    .optional()
    .describe("Superelevation attainment method to use. Defaults to AASHTO_2011."),
  pivotPoint: z.enum(["centerline", "inside_edge", "outside_edge"]).optional().describe("Pivot point for superelevation rotation (default: centerline)"),
});

// ─── 3. civil3d_superelevation_design_check ─────────────────────────────────

export const SuperelevationDesignCheckInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment to check superelevation design"),
  designSpeed: z.number().positive().describe("Design speed in km/h used to evaluate maximum superelevation rate"),
  maxSuperelevation: z.number().positive().optional().describe("Maximum allowable superelevation rate in percent (e.g., 8.0 for 8%). Defaults to AASHTO limits for design speed."),
  checkAttainmentLength: z.boolean().optional().describe("Check that superelevation runoff and tangent runout lengths meet AASHTO minimums (default: true)"),
  checkRunoffLength: z.boolean().optional().describe("Check that superelevation runoff length is within allowable range (default: true)"),
});

// ─── 4. civil3d_superelevation_report ───────────────────────────────────────

export const SuperelevationReportInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment to generate the superelevation report for"),
  outputPath: z.string().optional().describe("Full file path to write the report to (.txt or .csv). Omit to return the report as a string."),
  includeRunoffTable: z.boolean().optional().describe("Include the detailed superelevation runoff/runout length table (default: true)"),
  includeViolations: z.boolean().optional().describe("Highlight stations that violate design criteria in the report (default: true)"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DSuperelevationTools(server: McpServer) {

  // 1. civil3d_superelevation_get
  server.tool(
    "civil3d_superelevation_get",
    "Retrieve the superelevation design data for a Civil 3D alignment. Returns design speed, attainment method, pivot point, and optionally the full station-by-station superelevation table.",
    SuperelevationGetInputSchema.shape,
    async (args) => {
      try {
        const parsed = SuperelevationGetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSuperelevation", {
            alignmentName: parsed.alignmentName,
            includeRawData: parsed.includeRawData ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_superelevation_get", error);
      }
    }
  );

  // 2. civil3d_superelevation_set
  server.tool(
    "civil3d_superelevation_set",
    "Apply superelevation design to a Civil 3D alignment using a specified design speed and AASHTO attainment method. Sets normal crown slope, pivot point, and calculates transition lengths automatically.",
    SuperelevationSetInputSchema.shape,
    async (args) => {
      try {
        const parsed = SuperelevationSetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setSuperelevation", {
            alignmentName: parsed.alignmentName,
            designSpeed: parsed.designSpeed,
            normalCrownSlope: parsed.normalCrownSlope,
            attainmentMethod: parsed.attainmentMethod ?? "AASHTO_2011",
            pivotPoint: parsed.pivotPoint ?? "centerline",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_superelevation_set", error);
      }
    }
  );

  // 3. civil3d_superelevation_design_check
  server.tool(
    "civil3d_superelevation_design_check",
    "Run a design check on a Civil 3D alignment's superelevation. Validates maximum superelevation rates and AASHTO attainment length requirements for the specified design speed. Returns pass/fail results with specific station violations.",
    SuperelevationDesignCheckInputSchema.shape,
    async (args) => {
      try {
        const parsed = SuperelevationDesignCheckInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("checkSuperelevationDesign", {
            alignmentName: parsed.alignmentName,
            designSpeed: parsed.designSpeed,
            maxSuperelevation: parsed.maxSuperelevation ?? null,
            checkAttainmentLength: parsed.checkAttainmentLength ?? true,
            checkRunoffLength: parsed.checkRunoffLength ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_superelevation_design_check", error);
      }
    }
  );

  // 4. civil3d_superelevation_report
  server.tool(
    "civil3d_superelevation_report",
    "Generate a formatted superelevation report for a Civil 3D alignment. Includes attainment method, design speed, normal crown slope, transition lengths, and optional violation highlights. Writes to file or returns as string.",
    SuperelevationReportInputSchema.shape,
    async (args) => {
      try {
        const parsed = SuperelevationReportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("generateSuperelevationReport", {
            alignmentName: parsed.alignmentName,
            outputPath: parsed.outputPath ?? null,
            includeRunoffTable: parsed.includeRunoffTable ?? true,
            includeViolations: parsed.includeViolations ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_superelevation_report", error);
      }
    }
  );
}
