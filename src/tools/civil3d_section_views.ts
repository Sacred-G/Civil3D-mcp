import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_section_view_create ─────────────────────────────────────────

export const SectionViewCreateInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment whose sample lines define the sections"),
  sampleLineGroupName: z.string().describe("Name of the sample line group to create views for"),
  insertionPoint: z.tuple([z.number(), z.number()]).describe("X, Y coordinates for the origin of the first section view in model space"),
  style: z.string().optional().describe("Section view style name to apply (omit to use drawing default)"),
  bandSetStyle: z.string().optional().describe("Band set style name to apply to the section views"),
  leftOffset: z.number().nonnegative().optional().describe("Left width offset from centerline to display in the view (drawing units)"),
  rightOffset: z.number().nonnegative().optional().describe("Right width offset from centerline to display in the view (drawing units)"),
  stationStart: z.number().optional().describe("Start station to limit which sections are created (omit for all sections)"),
  stationEnd: z.number().optional().describe("End station to limit which sections are created (omit for all sections)"),
  rows: z.number().int().positive().optional().describe("Number of rows to arrange section views in (default: 1)"),
  gapBetweenViews: z.number().nonnegative().optional().describe("Gap between adjacent section view boxes in drawing units (default: 10)"),
});

// ─── 2. civil3d_section_view_list ───────────────────────────────────────────

export const SectionViewListInputSchema = z.object({
  alignmentName: z.string().optional().describe("Filter section views by alignment name (omit for all section views in the drawing)"),
  sampleLineGroupName: z.string().optional().describe("Filter by sample line group name"),
});

// ─── 3. civil3d_section_view_update_style ───────────────────────────────────

export const SectionViewUpdateStyleInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment whose section views to update"),
  sampleLineGroupName: z.string().describe("Name of the sample line group whose section views to update"),
  style: z.string().optional().describe("New section view style to apply"),
  bandSetStyle: z.string().optional().describe("New band set style to apply"),
  applyToAll: z.boolean().optional().describe("Apply the style change to all section views in the group (default: true)"),
});

// ─── 4. civil3d_section_view_group_create ───────────────────────────────────

export const SectionViewGroupCreateInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment whose sample lines define the sections"),
  sampleLineGroupName: z.string().describe("Name of the sample line group to create section views for"),
  insertionPoint: z.tuple([z.number(), z.number()]).describe("X, Y coordinates for the origin of the section view group in model space"),
  style: z.string().optional().describe("Section view style name to apply"),
  plotStyle: z.string().optional().describe("Plot style table to use for plotting the section views"),
  rows: z.number().int().positive().optional().describe("Number of rows to arrange section views in (default: auto)"),
  columns: z.number().int().positive().optional().describe("Number of columns to arrange section views in (default: auto)"),
  gapX: z.number().nonnegative().optional().describe("Horizontal gap between section view boxes in drawing units"),
  gapY: z.number().nonnegative().optional().describe("Vertical gap between section view boxes in drawing units"),
});

// ─── 5. civil3d_section_view_export ─────────────────────────────────────────

export const SectionViewExportInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment whose section views to export"),
  sampleLineGroupName: z.string().describe("Name of the sample line group whose section views to export"),
  outputPath: z.string().describe("Full file path for the exported section data (.csv or .txt)"),
  includeElevations: z.boolean().optional().describe("Include surface elevation data at each offset (default: true)"),
  includeMaterials: z.boolean().optional().describe("Include corridor material data per section (default: false)"),
  stationStart: z.number().optional().describe("Start station to filter export range (omit for all stations)"),
  stationEnd: z.number().optional().describe("End station to filter export range (omit for all stations)"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DSectionViewTools(server: McpServer) {

  // 1. civil3d_section_view_create
  server.tool(
    "civil3d_section_view_create",
    "Create Civil 3D section views for a sample line group. Places section view boxes in model space at the specified insertion point. Optionally applies a style, band set, and constrains to a station range.",
    SectionViewCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = SectionViewCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSectionViews", {
            alignmentName: parsed.alignmentName,
            sampleLineGroupName: parsed.sampleLineGroupName,
            insertionX: parsed.insertionPoint[0],
            insertionY: parsed.insertionPoint[1],
            style: parsed.style ?? null,
            bandSetStyle: parsed.bandSetStyle ?? null,
            leftOffset: parsed.leftOffset ?? null,
            rightOffset: parsed.rightOffset ?? null,
            stationStart: parsed.stationStart ?? null,
            stationEnd: parsed.stationEnd ?? null,
            rows: parsed.rows ?? 1,
            gapBetweenViews: parsed.gapBetweenViews ?? 10,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_section_view_create", error);
      }
    }
  );

  // 2. civil3d_section_view_list
  server.tool(
    "civil3d_section_view_list",
    "List Civil 3D section views in the active drawing. Optionally filter by alignment name and/or sample line group. Returns view handles, station ranges, and bounding box extents for each view.",
    SectionViewListInputSchema.shape,
    async (args) => {
      try {
        const parsed = SectionViewListInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listSectionViews", {
            alignmentName: parsed.alignmentName ?? null,
            sampleLineGroupName: parsed.sampleLineGroupName ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_section_view_list", error);
      }
    }
  );

  // 3. civil3d_section_view_update_style
  server.tool(
    "civil3d_section_view_update_style",
    "Update the display style and/or band set style on existing Civil 3D section views for a sample line group. Use to batch-apply a new style after initial creation.",
    SectionViewUpdateStyleInputSchema.shape,
    async (args) => {
      try {
        const parsed = SectionViewUpdateStyleInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("updateSectionViewStyles", {
            alignmentName: parsed.alignmentName,
            sampleLineGroupName: parsed.sampleLineGroupName,
            style: parsed.style ?? null,
            bandSetStyle: parsed.bandSetStyle ?? null,
            applyToAll: parsed.applyToAll ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_section_view_update_style", error);
      }
    }
  );

  // 4. civil3d_section_view_group_create
  server.tool(
    "civil3d_section_view_group_create",
    "Create a Civil 3D section view group — a multi-row grid layout of section views for all stations in a sample line group. Supports custom row/column counts, spacing, and style assignment.",
    SectionViewGroupCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = SectionViewGroupCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSectionViewGroup", {
            alignmentName: parsed.alignmentName,
            sampleLineGroupName: parsed.sampleLineGroupName,
            insertionX: parsed.insertionPoint[0],
            insertionY: parsed.insertionPoint[1],
            style: parsed.style ?? null,
            plotStyle: parsed.plotStyle ?? null,
            rows: parsed.rows ?? null,
            columns: parsed.columns ?? null,
            gapX: parsed.gapX ?? null,
            gapY: parsed.gapY ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_section_view_group_create", error);
      }
    }
  );

  // 5. civil3d_section_view_export
  server.tool(
    "civil3d_section_view_export",
    "Export Civil 3D section data to a CSV or text file. Includes station, offset, and surface elevation data per section. Optionally includes corridor material breakdowns.",
    SectionViewExportInputSchema.shape,
    async (args) => {
      try {
        const parsed = SectionViewExportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("exportSectionData", {
            alignmentName: parsed.alignmentName,
            sampleLineGroupName: parsed.sampleLineGroupName,
            outputPath: parsed.outputPath,
            includeElevations: parsed.includeElevations ?? true,
            includeMaterials: parsed.includeMaterials ?? false,
            stationStart: parsed.stationStart ?? null,
            stationEnd: parsed.stationEnd ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_section_view_export", error);
      }
    }
  );
}
