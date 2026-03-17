import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_parcel_create ────────────────────────────────────────────────

export const ParcelCreateInputSchema = z.object({
  siteName: z.string().describe("Name of the site to add the parcel to"),
  name: z.string().optional().describe("Display name for the new parcel (auto-generated if omitted)"),
  sourceHandle: z.string().optional().describe("AutoCAD entity handle of a closed polyline or feature line to convert into a parcel boundary"),
  points: z.array(z.tuple([z.number(), z.number()])).min(3).optional().describe("Ordered list of [X, Y] vertices that define the parcel boundary (closed automatically). Use when no sourceHandle is available."),
  style: z.string().optional().describe("Parcel style name to apply (uses site default when omitted)"),
  areaLabelStyle: z.string().optional().describe("Area label style name to apply to the parcel interior (omit to suppress labels)"),
});

// ─── 2. civil3d_parcel_edit ──────────────────────────────────────────────────

export const ParcelEditInputSchema = z.object({
  siteName: z.string().describe("Name of the site that owns the parcel"),
  parcelName: z.string().describe("Current name of the parcel to edit"),
  newName: z.string().optional().describe("Rename the parcel to this value"),
  style: z.string().optional().describe("New parcel style to apply"),
  areaLabelStyle: z.string().optional().describe("New area label style to apply"),
  description: z.string().optional().describe("Textual description or tax parcel ID to attach as parcel metadata"),
});

// ─── 3. civil3d_parcel_lot_line_adjust ───────────────────────────────────────

export const ParcelLotLineAdjustInputSchema = z.object({
  siteName: z.string().describe("Name of the site containing the parcels to adjust"),
  parcelName: z.string().describe("Name of the parcel whose lot line will be moved"),
  targetAreaSqFt: z.number().positive().describe("Target parcel area in square feet. The lot line will be translated until this area is achieved."),
  lotLineHandle: z.string().optional().describe("AutoCAD entity handle of the specific lot line segment to adjust (resolves ambiguity when multiple shared segments exist)"),
  tolerance: z.number().positive().optional().describe("Area convergence tolerance in square feet (default 1.0). Adjustment stops when |computed - target| < tolerance."),
});

// ─── 4. civil3d_parcel_report ────────────────────────────────────────────────

export const ParcelReportInputSchema = z.object({
  siteName: z.string().describe("Name of the site to report on"),
  parcelNames: z.array(z.string()).optional().describe("List of specific parcel names to include. Omit to report all parcels in the site."),
  outputPath: z.string().optional().describe("Full file path for a CSV export of the parcel report (e.g. C:/output/parcels.csv). Omit to return data inline."),
  includeCoordinates: z.boolean().optional().describe("Include vertex coordinate listing for each parcel boundary (default: false)"),
  units: z.enum(["sqft", "acres", "sqm", "ha"]).optional().describe("Area unit for the report (default: sqft)"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DParcelEditingTools(server: McpServer) {

  // 1. civil3d_parcel_create
  server.tool(
    "civil3d_parcel_create",
    "Create a new Civil 3D parcel in a site by converting a closed polyline or feature line entity (via its handle), or by providing a vertex list. Optionally applies a parcel style and area label style.",
    ParcelCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = ParcelCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createParcel", {
            siteName: parsed.siteName,
            name: parsed.name ?? null,
            sourceHandle: parsed.sourceHandle ?? null,
            points: parsed.points ?? null,
            style: parsed.style ?? null,
            areaLabelStyle: parsed.areaLabelStyle ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_parcel_create", error);
      }
    }
  );

  // 2. civil3d_parcel_edit
  server.tool(
    "civil3d_parcel_edit",
    "Edit properties of an existing Civil 3D parcel — rename it, change its display style, update its area label style, or attach a description. Does not modify parcel geometry.",
    ParcelEditInputSchema.shape,
    async (args) => {
      try {
        const parsed = ParcelEditInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("editParcel", {
            siteName: parsed.siteName,
            parcelName: parsed.parcelName,
            newName: parsed.newName ?? null,
            style: parsed.style ?? null,
            areaLabelStyle: parsed.areaLabelStyle ?? null,
            description: parsed.description ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_parcel_edit", error);
      }
    }
  );

  // 3. civil3d_parcel_lot_line_adjust
  server.tool(
    "civil3d_parcel_lot_line_adjust",
    "Adjust a parcel lot line to reach a target area. Civil 3D translates the specified shared lot-line segment until the parcel area converges to the target within tolerance. Useful for lot-area balancing in subdivision design.",
    ParcelLotLineAdjustInputSchema.shape,
    async (args) => {
      try {
        const parsed = ParcelLotLineAdjustInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("adjustParcelLotLine", {
            siteName: parsed.siteName,
            parcelName: parsed.parcelName,
            targetAreaSqFt: parsed.targetAreaSqFt,
            lotLineHandle: parsed.lotLineHandle ?? null,
            tolerance: parsed.tolerance ?? 1.0,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_parcel_lot_line_adjust", error);
      }
    }
  );

  // 4. civil3d_parcel_report
  server.tool(
    "civil3d_parcel_report",
    "Generate a parcel area and dimension report for one or more parcels in a Civil 3D site. Returns parcel number, name, area, perimeter, and optional vertex coordinates. Can export to CSV.",
    ParcelReportInputSchema.shape,
    async (args) => {
      try {
        const parsed = ParcelReportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("reportParcels", {
            siteName: parsed.siteName,
            parcelNames: parsed.parcelNames ?? null,
            outputPath: parsed.outputPath ?? null,
            includeCoordinates: parsed.includeCoordinates ?? false,
            units: parsed.units ?? "sqft",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_parcel_report", error);
      }
    }
  );
}
