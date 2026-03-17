import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_survey_observation_list ──────────────────────────────────────

export const SurveyObservationListInputSchema = z.object({
  databaseName: z.string().describe("Name of the Civil 3D survey database to query"),
  networkName: z.string().optional().describe("Filter to a specific survey network within the database (omit for all networks)"),
  observationType: z.enum(["all", "angles", "distances", "directions", "gps"]).optional().describe("Filter by observation type (default: all)"),
});

// ─── 2. civil3d_survey_network_adjust ────────────────────────────────────────

export const SurveyNetworkAdjustInputSchema = z.object({
  databaseName: z.string().describe("Name of the Civil 3D survey database containing the network"),
  networkName: z.string().describe("Name of the survey network to adjust"),
  method: z.enum(["least_squares", "compass", "transit", "crandall"]).optional().describe("Adjustment method (default: least_squares). least_squares distributes angular and linear error statistically; compass and transit use classical traverse rules."),
  confidenceLevel: z.number().min(50).max(99.9).optional().describe("Statistical confidence level for error ellipses (default: 95). Only applies to least_squares method."),
  applyAdjustment: z.boolean().optional().describe("Write adjusted coordinates back to the database (default: false — dry run only)"),
});

// ─── 3. civil3d_survey_figure_create ─────────────────────────────────────────

export const SurveyFigureCreateInputSchema = z.object({
  databaseName: z.string().describe("Name of the Civil 3D survey database to add the figure to"),
  figureName: z.string().describe("Name for the new survey figure (must be unique within the database)"),
  pointNumbers: z.array(z.number().int().positive()).min(2).describe("Ordered list of survey point numbers that define the figure linework. Points must already exist in the database."),
  figureStyle: z.string().optional().describe("Survey figure style to apply (uses database default when omitted)"),
  closed: z.boolean().optional().describe("Close the figure by connecting the last point back to the first (default: false)"),
  layer: z.string().optional().describe("AutoCAD layer to place the figure linework on (uses current layer when omitted)"),
});

// ─── 4. civil3d_survey_landxml_import ────────────────────────────────────────

export const SurveyLandXmlImportInputSchema = z.object({
  filePath: z.string().describe("Full path to the LandXML (.xml) file containing survey data to import"),
  databaseName: z.string().describe("Target Civil 3D survey database to import the LandXML data into"),
  importPoints: z.boolean().optional().describe("Import CgPoint elements as COGO points (default: true)"),
  importAlignments: z.boolean().optional().describe("Import Alignment elements as Civil 3D alignments (default: false)"),
  importSurfaces: z.boolean().optional().describe("Import Surface elements as Civil 3D TIN surfaces (default: false)"),
  coordinateSystemOverride: z.string().optional().describe("Apply this coordinate system code to imported data, overriding whatever is defined in the LandXML file"),
  duplicatePolicy: z.enum(["skip", "overwrite", "rename"]).optional().describe("How to handle point number collisions — skip (default), overwrite existing, or auto-rename the imported point"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DSurveyProcessingTools(server: McpServer) {

  // 1. civil3d_survey_observation_list
  server.tool(
    "civil3d_survey_observation_list",
    "List raw field-book observations stored in a Civil 3D survey database. Returns angle, distance, and direction observations grouped by survey network. Use to inspect raw data before running a network adjustment.",
    SurveyObservationListInputSchema.shape,
    async (args) => {
      try {
        const parsed = SurveyObservationListInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listSurveyObservations", {
            databaseName: parsed.databaseName,
            networkName: parsed.networkName ?? null,
            observationType: parsed.observationType ?? "all",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_observation_list", error);
      }
    }
  );

  // 2. civil3d_survey_network_adjust
  server.tool(
    "civil3d_survey_network_adjust",
    "Perform a traverse or least-squares adjustment on a Civil 3D survey network. Distributes angular and linear closure errors across the traverse, optionally writing adjusted coordinates back to the database. Call with applyAdjustment=false first to preview the closure error.",
    SurveyNetworkAdjustInputSchema.shape,
    async (args) => {
      try {
        const parsed = SurveyNetworkAdjustInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("adjustSurveyNetwork", {
            databaseName: parsed.databaseName,
            networkName: parsed.networkName,
            method: parsed.method ?? "least_squares",
            confidenceLevel: parsed.confidenceLevel ?? 95,
            applyAdjustment: parsed.applyAdjustment ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_network_adjust", error);
      }
    }
  );

  // 3. civil3d_survey_figure_create
  server.tool(
    "civil3d_survey_figure_create",
    "Create a new survey figure in a Civil 3D survey database by connecting existing survey points in order. Survey figures represent topographic features observed in the field (e.g., toe of slope, edge of pavement, fence line).",
    SurveyFigureCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = SurveyFigureCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSurveyFigure", {
            databaseName: parsed.databaseName,
            figureName: parsed.figureName,
            pointNumbers: parsed.pointNumbers,
            figureStyle: parsed.figureStyle ?? null,
            closed: parsed.closed ?? false,
            layer: parsed.layer ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_figure_create", error);
      }
    }
  );

  // 4. civil3d_survey_landxml_import
  server.tool(
    "civil3d_survey_landxml_import",
    "Import survey data from a LandXML file into a Civil 3D survey database and/or drawing. Supports importing COGO points, alignments, and TIN surfaces. Handles coordinate system mapping and duplicate point policies.",
    SurveyLandXmlImportInputSchema.shape,
    async (args) => {
      try {
        const parsed = SurveyLandXmlImportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("importSurveyLandXml", {
            filePath: parsed.filePath,
            databaseName: parsed.databaseName,
            importPoints: parsed.importPoints ?? true,
            importAlignments: parsed.importAlignments ?? false,
            importSurfaces: parsed.importSurfaces ?? false,
            coordinateSystemOverride: parsed.coordinateSystemOverride ?? null,
            duplicatePolicy: parsed.duplicatePolicy ?? "skip",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_landxml_import", error);
      }
    }
  );
}
