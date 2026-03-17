import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_qc_check_alignment ─────────────────────────────────────────

export const QcAlignmentInputSchema = z.object({
  name: z.string().describe("Name of the alignment to check"),
  designSpeed: z.number().positive().optional().describe("Design speed in km/h used to evaluate minimum curve radii and sight distances"),
  checkTangents: z.boolean().optional().describe("Check tangent segments for length and grade violations (default: true)"),
  checkCurves: z.boolean().optional().describe("Check horizontal curve radii and superelevation against design speed (default: true)"),
  checkSpirals: z.boolean().optional().describe("Check spiral transition lengths for adequacy (default: true)"),
});

const QcAlignmentInputShape = QcAlignmentInputSchema.shape;

// ─── 2. civil3d_qc_check_profile ───────────────────────────────────────────

export const QcProfileInputSchema = z.object({
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the profile to check"),
  maxGrade: z.number().positive().optional().describe("Maximum allowable grade in percent (e.g. 8.0 for 8%)"),
  minKValue: z.number().positive().optional().describe("Minimum required K-value for vertical curves"),
});

const QcProfileInputShape = QcProfileInputSchema.shape;

// ─── 3. civil3d_qc_check_corridor ──────────────────────────────────────────

export const QcCorridorInputSchema = z.object({
  name: z.string().describe("Name of the corridor to check"),
});

const QcCorridorInputShape = QcCorridorInputSchema.shape;

// ─── 4. civil3d_qc_check_pipe_network ──────────────────────────────────────

export const QcPipeNetworkInputSchema = z.object({
  name: z.string().describe("Name of the pipe network to check"),
  minCover: z.number().nonnegative().optional().describe("Minimum required cover depth over pipes in drawing units"),
  maxSlope: z.number().positive().optional().describe("Maximum allowable pipe slope in percent"),
  minSlope: z.number().nonnegative().optional().describe("Minimum required pipe slope in percent"),
  minVelocity: z.number().nonnegative().optional().describe("Minimum required flow velocity in m/s (or ft/s)"),
  maxVelocity: z.number().positive().optional().describe("Maximum allowable flow velocity in m/s (or ft/s)"),
});

const QcPipeNetworkInputShape = QcPipeNetworkInputSchema.shape;

// ─── 5. civil3d_qc_check_surface ───────────────────────────────────────────

export const QcSurfaceInputSchema = z.object({
  name: z.string().describe("Name of the surface to check"),
  spikeThreshold: z.number().positive().optional().describe("Maximum allowable elevation delta between adjacent TIN nodes — points exceeding this are flagged as spikes (drawing units)"),
  flatTriangleThreshold: z.number().nonnegative().optional().describe("Maximum slope percentage below which a TIN triangle is considered flat/suspect (e.g. 0.1 for 0.1%)"),
});

const QcSurfaceInputShape = QcSurfaceInputSchema.shape;

// ─── 6. civil3d_qc_check_labels ────────────────────────────────────────────

export const QcLabelsInputSchema = z.object({
  objectType: z
    .enum(["alignment", "profile", "surface", "pipe_network", "all"])
    .optional()
    .describe("Civil 3D object type to check labels for (default: all)"),
  checkMissing: z.boolean().optional().describe("Flag objects that have no labels applied (default: true)"),
  checkStyleViolations: z.boolean().optional().describe("Flag labels whose style does not match the project standard (default: true)"),
});

const QcLabelsInputShape = QcLabelsInputSchema.shape;

// ─── 7. civil3d_qc_report_generate ─────────────────────────────────────────

export const QcReportInputSchema = z.object({
  outputPath: z.string().describe("Full path to write the QC report file (.txt or .csv)"),
  includeAlignments: z.boolean().optional().describe("Include alignment QC results in the report (default: true)"),
  includeProfiles: z.boolean().optional().describe("Include profile QC results in the report (default: true)"),
  includeCorridors: z.boolean().optional().describe("Include corridor QC results in the report (default: true)"),
  includePipeNetworks: z.boolean().optional().describe("Include pipe network QC results in the report (default: true)"),
  includeSurfaces: z.boolean().optional().describe("Include surface QC results in the report (default: true)"),
  includeLabels: z.boolean().optional().describe("Include label QC results in the report (default: true)"),
});

const QcReportInputShape = QcReportInputSchema.shape;

// ─── 8. civil3d_qc_check_drawing_standards ─────────────────────────────────

export const QcDrawingStandardsInputSchema = z.object({
  layerPrefix: z.string().optional().describe("Expected layer prefix (e.g. 'C-'). Layers not starting with this prefix will be flagged."),
  checkLineweights: z.boolean().optional().describe("Verify that object lineweights conform to drawing standard defaults (default: true)"),
  checkColors: z.boolean().optional().describe("Verify that object colors conform to drawing standard defaults (default: true)"),
});

const QcDrawingStandardsInputShape = QcDrawingStandardsInputSchema.shape;

// ─── Registration ───────────────────────────────────────────────────────────

export function registerCivil3DQcTools(server: McpServer) {

  // 1. civil3d_qc_check_alignment
  server.tool(
    "civil3d_qc_check_alignment",
    "Run QC checks on a Civil 3D alignment. Validates tangent lengths, horizontal curve radii, spiral transitions, and design-speed compliance. Returns an array of violation findings.",
    QcAlignmentInputShape,
    async (args) => {
      try {
        const parsed = QcAlignmentInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckAlignment", {
            name: parsed.name,
            designSpeed: parsed.designSpeed ?? null,
            checkTangents: parsed.checkTangents ?? true,
            checkCurves: parsed.checkCurves ?? true,
            checkSpirals: parsed.checkSpirals ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_alignment", error);
      }
    }
  );

  // 2. civil3d_qc_check_profile
  server.tool(
    "civil3d_qc_check_profile",
    "Run QC checks on a Civil 3D profile. Validates maximum grade, minimum K-values for vertical curves, and sight distance requirements. Returns grade, K-value, and sight distance violations.",
    QcProfileInputShape,
    async (args) => {
      try {
        const parsed = QcProfileInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckProfile", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            maxGrade: parsed.maxGrade ?? null,
            minKValue: parsed.minKValue ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_profile", error);
      }
    }
  );

  // 3. civil3d_qc_check_corridor
  server.tool(
    "civil3d_qc_check_corridor",
    "Run QC checks on a Civil 3D corridor. Identifies invalid regions, missing or unresolved targets, assembly gaps, and rebuild errors.",
    QcCorridorInputShape,
    async (args) => {
      try {
        const parsed = QcCorridorInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckCorridor", {
            name: parsed.name,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_corridor", error);
      }
    }
  );

  // 4. civil3d_qc_check_pipe_network
  server.tool(
    "civil3d_qc_check_pipe_network",
    "Run QC checks on a Civil 3D pipe network. Validates cover depth, pipe slope, flow velocity, structure connectivity, and sump depth. Returns violations per pipe and structure.",
    QcPipeNetworkInputShape,
    async (args) => {
      try {
        const parsed = QcPipeNetworkInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckPipeNetwork", {
            name: parsed.name,
            minCover: parsed.minCover ?? null,
            maxSlope: parsed.maxSlope ?? null,
            minSlope: parsed.minSlope ?? null,
            minVelocity: parsed.minVelocity ?? null,
            maxVelocity: parsed.maxVelocity ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_pipe_network", error);
      }
    }
  );

  // 5. civil3d_qc_check_surface
  server.tool(
    "civil3d_qc_check_surface",
    "Run QC checks on a Civil 3D TIN surface. Detects elevation spikes, flat/suspect triangles, crossing breaklines, and data voids. Returns flagged point and triangle locations.",
    QcSurfaceInputShape,
    async (args) => {
      try {
        const parsed = QcSurfaceInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckSurface", {
            name: parsed.name,
            spikeThreshold: parsed.spikeThreshold ?? null,
            flatTriangleThreshold: parsed.flatTriangleThreshold ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_surface", error);
      }
    }
  );

  // 6. civil3d_qc_check_labels
  server.tool(
    "civil3d_qc_check_labels",
    "Check Civil 3D labels across one or all object types for missing labels and style standard violations. Returns a list of objects with label issues.",
    QcLabelsInputShape,
    async (args) => {
      try {
        const parsed = QcLabelsInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckLabels", {
            objectType: parsed.objectType ?? "all",
            checkMissing: parsed.checkMissing ?? true,
            checkStyleViolations: parsed.checkStyleViolations ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_labels", error);
      }
    }
  );

  // 7. civil3d_qc_report_generate
  server.tool(
    "civil3d_qc_report_generate",
    "Run a full QC pass over the active drawing and write a consolidated report to disk (.txt or .csv). Covers alignments, profiles, corridors, pipe networks, surfaces, and labels. Returns the output file path and a summary of total findings.",
    QcReportInputShape,
    async (args) => {
      try {
        const parsed = QcReportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcReportGenerate", {
            outputPath: parsed.outputPath,
            includeAlignments: parsed.includeAlignments ?? true,
            includeProfiles: parsed.includeProfiles ?? true,
            includeCorridors: parsed.includeCorridors ?? true,
            includePipeNetworks: parsed.includePipeNetworks ?? true,
            includeSurfaces: parsed.includeSurfaces ?? true,
            includeLabels: parsed.includeLabels ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_report_generate", error);
      }
    }
  );

  // 8. civil3d_qc_check_drawing_standards
  server.tool(
    "civil3d_qc_check_drawing_standards",
    "Audit the active Civil 3D drawing against CAD standards. Checks layer naming conventions, lineweights, and colors. Returns non-conforming objects grouped by violation type.",
    QcDrawingStandardsInputShape,
    async (args) => {
      try {
        const parsed = QcDrawingStandardsInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qcCheckDrawingStandards", {
            layerPrefix: parsed.layerPrefix ?? null,
            checkLineweights: parsed.checkLineweights ?? true,
            checkColors: parsed.checkColors ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qc_check_drawing_standards", error);
      }
    }
  );
}
