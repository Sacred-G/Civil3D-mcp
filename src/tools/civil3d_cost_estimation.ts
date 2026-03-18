import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

const PayItemSchema = z.object({
  code: z.string().describe("Pay item code (e.g. '203.01', 'A-2010')"),
  description: z.string().describe("Pay item description (e.g. 'Unclassified Excavation')"),
  unit: z.string().describe("Unit of measure (CY, LF, SY, EA, TON, LS)"),
  unitPrice: z.number().nonnegative().describe("Unit price in dollars"),
});

// ─── 1. civil3d_pay_items_export ────────────────────────────────────────────

export const PayItemsExportInputSchema = z.object({
  outputPath: z.string().describe("Full file path to write the pay item export (.csv or .xlsx)"),
  corridorName: z.string().optional().describe("Corridor to extract material quantities from"),
  baseSurface: z.string().optional().describe("Base (existing ground) surface name for earthwork volumes"),
  designSurface: z.string().optional().describe("Design surface name for earthwork volumes"),
  alignmentName: z.string().optional().describe("Alignment name to scope pipe network and corridor quantities"),
  payItems: z.array(PayItemSchema).optional().describe("List of pay item definitions with codes, units, and unit prices to populate the export. If omitted, uses quantities only without pricing."),
  includeEarthwork: z.boolean().optional().describe("Include cut/fill earthwork quantities (default: true)"),
  includeCorridorMaterials: z.boolean().optional().describe("Include corridor material layer quantities (default: true)"),
  includePipeLengths: z.boolean().optional().describe("Include gravity and pressure pipe lengths (default: true)"),
  includeStructureCounts: z.boolean().optional().describe("Include structure (manhole, inlet, junction box) counts (default: true)"),
});

const PayItemsExportInputShape = PayItemsExportInputSchema.shape;

// ─── 2. civil3d_material_cost_estimate ──────────────────────────────────────

export const MaterialCostEstimateInputSchema = z.object({
  corridorName: z.string().optional().describe("Corridor to extract material volumes from"),
  baseSurface: z.string().optional().describe("Existing ground surface name"),
  designSurface: z.string().optional().describe("Design surface name"),
  alignmentName: z.string().optional().describe("Alignment to scope cost estimate to"),
  contingencyPercent: z.number().nonnegative().optional().describe("Contingency percentage to add to total (e.g. 10 for 10%). Default: 0"),
  mobilizationPercent: z.number().nonnegative().optional().describe("Mobilization as a percentage of construction cost (default: 5%)"),
  payItems: z.array(PayItemSchema).describe("Pay item definitions with unit prices. Required to generate a cost estimate. Each item must have a code, description, unit, and unit price."),
  outputPath: z.string().optional().describe("Optional file path to export the cost estimate to CSV"),
});

const MaterialCostEstimateInputShape = MaterialCostEstimateInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DCostEstimationTools(server: McpServer) {

  // 1. civil3d_pay_items_export
  server.tool(
    "civil3d_pay_items_export",
    "Extract Civil 3D quantities (earthwork volumes, corridor material layers, pipe network lengths, structure counts) and export them as a structured pay item schedule to CSV or Excel. Optionally accepts a list of pay item codes with unit prices to produce a priced bill of quantities. Use after civil3d_qty_earthwork_summary and civil3d_qty_corridor_volumes to build the complete quantity takeoff for bid preparation.",
    PayItemsExportInputShape,
    async (args) => {
      try {
        const parsed = PayItemsExportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("exportPayItems", {
            outputPath: parsed.outputPath,
            corridorName: parsed.corridorName ?? null,
            baseSurface: parsed.baseSurface ?? null,
            designSurface: parsed.designSurface ?? null,
            alignmentName: parsed.alignmentName ?? null,
            payItems: parsed.payItems ?? [],
            includeEarthwork: parsed.includeEarthwork ?? true,
            includeCorridorMaterials: parsed.includeCorridorMaterials ?? true,
            includePipeLengths: parsed.includePipeLengths ?? true,
            includeStructureCounts: parsed.includeStructureCounts ?? true,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_pay_items_export", error);
      }
    }
  );

  // 2. civil3d_material_cost_estimate
  server.tool(
    "civil3d_material_cost_estimate",
    "Generate a construction cost estimate by combining Civil 3D quantities with user-provided unit prices. Extracts earthwork volumes, corridor layer volumes (subbase, base, pavement), and pipe network quantities, then multiplies each by the supplied unit price per pay item code. Returns a line-item cost breakdown, subtotal, contingency, mobilization, and total estimated construction cost. Optionally exports to CSV for submittal.",
    MaterialCostEstimateInputShape,
    async (args) => {
      try {
        const parsed = MaterialCostEstimateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateMaterialCostEstimate", {
            corridorName: parsed.corridorName ?? null,
            baseSurface: parsed.baseSurface ?? null,
            designSurface: parsed.designSurface ?? null,
            alignmentName: parsed.alignmentName ?? null,
            contingencyPercent: parsed.contingencyPercent ?? 0,
            mobilizationPercent: parsed.mobilizationPercent ?? 5,
            payItems: parsed.payItems,
            outputPath: parsed.outputPath ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_material_cost_estimate", error);
      }
    }
  );
}
