import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_qty_corridor_volumes ──────────────────────────────────────

export const QtyCorridorVolumesInputSchema = z.object({
  name: z.string().describe("Corridor name"),
  materials: z.array(z.string()).optional().describe("Specific material names to include (default: all)"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
});

const QtyCorridorVolumesInputShape = {
  name: z.string().describe("Corridor name"),
  materials: z.array(z.string()).optional().describe("Specific material names to include (default: all)"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
};

// ─── 2. civil3d_qty_surface_volume ────────────────────────────────────────

export const QtySurfaceVolumeInputSchema = z.object({
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  corridorName: z.string().optional().describe("Restrict calculation to corridor extents"),
  region: z
    .array(z.object({ x: z.number(), y: z.number() }))
    .optional()
    .describe("Polygon region (min 3 pts)"),
});

const QtySurfaceVolumeInputShape = {
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  corridorName: z.string().optional().describe("Restrict calculation to corridor extents"),
  region: z
    .array(z.object({ x: z.number(), y: z.number() }))
    .optional()
    .describe("Polygon region (min 3 pts)"),
};

// ─── 3. civil3d_qty_pipe_network_lengths ──────────────────────────────────

export const QtyPipeNetworkLengthsInputSchema = z.object({
  name: z.string().describe("Pipe network name"),
  groupBySize: z.boolean().optional(),
  groupByMaterial: z.boolean().optional(),
});

const QtyPipeNetworkLengthsInputShape = {
  name: z.string().describe("Pipe network name"),
  groupBySize: z.boolean().optional(),
  groupByMaterial: z.boolean().optional(),
};

// ─── 4. civil3d_qty_pressure_network_lengths ──────────────────────────────

export const QtyPressureNetworkLengthsInputSchema = z.object({
  name: z.string().describe("Pressure network name"),
  groupBySize: z.boolean().optional(),
  groupByMaterial: z.boolean().optional(),
});

const QtyPressureNetworkLengthsInputShape = {
  name: z.string().describe("Pressure network name"),
  groupBySize: z.boolean().optional(),
  groupByMaterial: z.boolean().optional(),
};

// ─── 5. civil3d_qty_parcel_areas ──────────────────────────────────────────

export const QtyParcelAreasInputSchema = z.object({
  siteName: z.string().optional().describe("Site name to filter parcels (default: all sites)"),
  parcelNames: z.array(z.string()).optional().describe("Specific parcel names (default: all)"),
});

const QtyParcelAreasInputShape = {
  siteName: z.string().optional().describe("Site name to filter parcels (default: all sites)"),
  parcelNames: z.array(z.string()).optional().describe("Specific parcel names (default: all)"),
};

// ─── 6. civil3d_qty_alignment_lengths ─────────────────────────────────────

export const QtyAlignmentLengthsInputSchema = z.object({
  names: z.array(z.string()).optional().describe("Alignment names (default: all)"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
});

const QtyAlignmentLengthsInputShape = {
  names: z.array(z.string()).optional().describe("Alignment names (default: all)"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
};

// ─── 7. civil3d_qty_point_count_by_group ──────────────────────────────────

export const QtyPointCountByGroupInputSchema = z.object({
  groupNames: z.array(z.string()).optional().describe("Point group names (default: all groups)"),
});

const QtyPointCountByGroupInputShape = {
  groupNames: z.array(z.string()).optional().describe("Point group names (default: all groups)"),
};

// ─── 8. civil3d_qty_export_to_csv ─────────────────────────────────────────

export const QtyExportToCsvInputSchema = z.object({
  outputPath: z.string().describe("Full path for the output CSV file"),
  includeCorridorVolumes: z.boolean().optional(),
  includeSurfaceVolumes: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includePressureNetworks: z.boolean().optional(),
  includeParcelAreas: z.boolean().optional(),
  includeAlignmentLengths: z.boolean().optional(),
  corridorName: z.string().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
});

const QtyExportToCsvInputShape = {
  outputPath: z.string().describe("Full path for the output CSV file"),
  includeCorridorVolumes: z.boolean().optional(),
  includeSurfaceVolumes: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includePressureNetworks: z.boolean().optional(),
  includeParcelAreas: z.boolean().optional(),
  includeAlignmentLengths: z.boolean().optional(),
  corridorName: z.string().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
};

// ─── 9. civil3d_qty_material_list_get ─────────────────────────────────────

export const QtyMaterialListGetInputSchema = z.object({
  corridorName: z.string().describe("Corridor name"),
  includeQuantities: z
    .boolean()
    .optional()
    .describe("Include calculated quantities (default: false — list only)"),
});

const QtyMaterialListGetInputShape = {
  corridorName: z.string().describe("Corridor name"),
  includeQuantities: z
    .boolean()
    .optional()
    .describe("Include calculated quantities (default: false — list only)"),
};

// ─── 10. civil3d_qty_earthwork_summary ────────────────────────────────────

export const QtyEarthworkSummaryInputSchema = z.object({
  baseSurface: z.string(),
  designSurface: z.string(),
  alignmentName: z
    .string()
    .optional()
    .describe("Restrict to alignment corridor extents"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  stationInterval: z
    .number()
    .positive()
    .optional()
    .describe("Station interval for running totals (default: 50)"),
});

const QtyEarthworkSummaryInputShape = {
  baseSurface: z.string(),
  designSurface: z.string(),
  alignmentName: z
    .string()
    .optional()
    .describe("Restrict to alignment corridor extents"),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  stationInterval: z
    .number()
    .positive()
    .optional()
    .describe("Station interval for running totals (default: 50)"),
};

// ─── Registration ──────────────────────────────────────────────────────────

export function registerCivil3DQuantityTakeoffTools(server: McpServer) {

  // 1. civil3d_qty_corridor_volumes
  server.tool(
    "civil3d_qty_corridor_volumes",
    "Calculate subassembly material volumes by region for a Civil 3D corridor. Optionally filter by material name and station range.",
    QtyCorridorVolumesInputShape,
    async (args) => {
      try {
        const parsed = QtyCorridorVolumesInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyCorridorVolumes", {
            name: parsed.name,
            materials: parsed.materials ?? null,
            startStation: parsed.startStation ?? null,
            endStation: parsed.endStation ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_corridor_volumes", error);
      }
    }
  );

  // 2. civil3d_qty_surface_volume
  server.tool(
    "civil3d_qty_surface_volume",
    "Calculate cut/fill volumes between two Civil 3D surfaces for quantity takeoff purposes. Optionally restrict the calculation to corridor extents or a polygon region.",
    QtySurfaceVolumeInputShape,
    async (args) => {
      try {
        const parsed = QtySurfaceVolumeInputSchema.parse(args);
        if (parsed.region != null && parsed.region.length < 3) {
          throw new Error("region polygon must contain at least 3 points");
        }
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtySurfaceVolume", {
            baseSurface: parsed.baseSurface,
            comparisonSurface: parsed.comparisonSurface,
            corridorName: parsed.corridorName ?? null,
            region: parsed.region ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_surface_volume", error);
      }
    }
  );

  // 3. civil3d_qty_pipe_network_lengths
  server.tool(
    "civil3d_qty_pipe_network_lengths",
    "Summarize total pipe lengths for a Civil 3D gravity pipe network. Optionally group results by pipe size and/or material.",
    QtyPipeNetworkLengthsInputShape,
    async (args) => {
      try {
        const parsed = QtyPipeNetworkLengthsInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyPipeNetworkLengths", {
            name: parsed.name,
            groupBySize: parsed.groupBySize ?? false,
            groupByMaterial: parsed.groupByMaterial ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_pipe_network_lengths", error);
      }
    }
  );

  // 4. civil3d_qty_pressure_network_lengths
  server.tool(
    "civil3d_qty_pressure_network_lengths",
    "Summarize total pipe lengths for a Civil 3D pressure network. Optionally group results by pipe size and/or material.",
    QtyPressureNetworkLengthsInputShape,
    async (args) => {
      try {
        const parsed = QtyPressureNetworkLengthsInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyPressureNetworkLengths", {
            name: parsed.name,
            groupBySize: parsed.groupBySize ?? false,
            groupByMaterial: parsed.groupByMaterial ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_pressure_network_lengths", error);
      }
    }
  );

  // 5. civil3d_qty_parcel_areas
  server.tool(
    "civil3d_qty_parcel_areas",
    "List area, perimeter, and address data for Civil 3D parcels. Filter by site and/or specific parcel names.",
    QtyParcelAreasInputShape,
    async (args) => {
      try {
        const parsed = QtyParcelAreasInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyParcelAreas", {
            siteName: parsed.siteName ?? null,
            parcelNames: parsed.parcelNames ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_parcel_areas", error);
      }
    }
  );

  // 6. civil3d_qty_alignment_lengths
  server.tool(
    "civil3d_qty_alignment_lengths",
    "Return the total length (and optional station range sub-length) for one or more Civil 3D alignments.",
    QtyAlignmentLengthsInputShape,
    async (args) => {
      try {
        const parsed = QtyAlignmentLengthsInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyAlignmentLengths", {
            names: parsed.names ?? null,
            startStation: parsed.startStation ?? null,
            endStation: parsed.endStation ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_alignment_lengths", error);
      }
    }
  );

  // 7. civil3d_qty_point_count_by_group
  server.tool(
    "civil3d_qty_point_count_by_group",
    "Count COGO points per point group in the Civil 3D drawing. Returns a summary table of group name and point count.",
    QtyPointCountByGroupInputShape,
    async (args) => {
      try {
        const parsed = QtyPointCountByGroupInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyPointCountByGroup", {
            groupNames: parsed.groupNames ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_point_count_by_group", error);
      }
    }
  );

  // 8. civil3d_qty_export_to_csv
  server.tool(
    "civil3d_qty_export_to_csv",
    "Export a consolidated quantity takeoff report to a CSV file. Select which quantity categories to include and supply any required source object names.",
    QtyExportToCsvInputShape,
    async (args) => {
      try {
        const parsed = QtyExportToCsvInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyExportToCsv", {
            outputPath: parsed.outputPath,
            includeCorridorVolumes: parsed.includeCorridorVolumes ?? false,
            includeSurfaceVolumes: parsed.includeSurfaceVolumes ?? false,
            includePipeNetworks: parsed.includePipeNetworks ?? false,
            includePressureNetworks: parsed.includePressureNetworks ?? false,
            includeParcelAreas: parsed.includeParcelAreas ?? false,
            includeAlignmentLengths: parsed.includeAlignmentLengths ?? false,
            corridorName: parsed.corridorName ?? null,
            baseSurface: parsed.baseSurface ?? null,
            comparisonSurface: parsed.comparisonSurface ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_export_to_csv", error);
      }
    }
  );

  // 9. civil3d_qty_material_list_get
  server.tool(
    "civil3d_qty_material_list_get",
    "Retrieve the material list defined on a Civil 3D corridor. Optionally include pre-calculated quantities for each material.",
    QtyMaterialListGetInputShape,
    async (args) => {
      try {
        const parsed = QtyMaterialListGetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyMaterialListGet", {
            corridorName: parsed.corridorName,
            includeQuantities: parsed.includeQuantities ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_material_list_get", error);
      }
    }
  );

  // 10. civil3d_qty_earthwork_summary
  server.tool(
    "civil3d_qty_earthwork_summary",
    "Generate a running earthwork cut/fill summary table between a base and design surface. Optionally scoped to an alignment corridor with configurable station interval.",
    QtyEarthworkSummaryInputShape,
    async (args) => {
      try {
        const parsed = QtyEarthworkSummaryInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("qtyEarthworkSummary", {
            baseSurface: parsed.baseSurface,
            designSurface: parsed.designSurface,
            alignmentName: parsed.alignmentName ?? null,
            startStation: parsed.startStation ?? null,
            endStation: parsed.endStation ?? null,
            stationInterval: parsed.stationInterval ?? 50,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_qty_earthwork_summary", error);
      }
    }
  );
}
