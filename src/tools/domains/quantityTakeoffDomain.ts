import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const GenericResponseSchema = z.object({}).passthrough();
const RegionSchema = z.array(z.object({ x: z.number(), y: z.number() }));

const QtyCorridorVolumesArgs = z.object({ action: z.literal("corridor_volumes"), name: z.string(), materials: z.array(z.string()).optional(), startStation: z.number().optional(), endStation: z.number().optional() });
const QtySurfaceVolumeArgs = z.object({ action: z.literal("surface_volume"), baseSurface: z.string(), comparisonSurface: z.string(), corridorName: z.string().optional(), region: RegionSchema.optional() }).superRefine((v, ctx) => {
  if (v.region != null && v.region.length < 3) ctx.addIssue({ code: z.ZodIssueCode.custom, message: "region polygon must contain at least 3 points", path: ["region"] });
});
const QtyPipeNetworkLengthsArgs = z.object({ action: z.literal("pipe_network_lengths"), name: z.string(), groupBySize: z.boolean().optional(), groupByMaterial: z.boolean().optional() });
const QtyPressureNetworkLengthsArgs = z.object({ action: z.literal("pressure_network_lengths"), name: z.string(), groupBySize: z.boolean().optional(), groupByMaterial: z.boolean().optional() });
const QtyParcelAreasArgs = z.object({ action: z.literal("parcel_areas"), siteName: z.string().optional(), parcelNames: z.array(z.string()).optional() });
const QtyAlignmentLengthsArgs = z.object({ action: z.literal("alignment_lengths"), names: z.array(z.string()).optional(), startStation: z.number().optional(), endStation: z.number().optional() });
const QtyPointCountByGroupArgs = z.object({ action: z.literal("point_count_by_group"), groupNames: z.array(z.string()).optional() });
const QtyExportToCsvArgs = z.object({ action: z.literal("export_to_csv"), outputPath: z.string(), includeCorridorVolumes: z.boolean().optional(), includeSurfaceVolumes: z.boolean().optional(), includePipeNetworks: z.boolean().optional(), includePressureNetworks: z.boolean().optional(), includeParcelAreas: z.boolean().optional(), includeAlignmentLengths: z.boolean().optional(), corridorName: z.string().optional(), baseSurface: z.string().optional(), comparisonSurface: z.string().optional() });
const QtyMaterialListGetArgs = z.object({ action: z.literal("material_list_get"), corridorName: z.string(), includeQuantities: z.boolean().optional() });
const QtyEarthworkSummaryArgs = z.object({ action: z.literal("earthwork_summary"), baseSurface: z.string(), designSurface: z.string(), alignmentName: z.string().optional(), startStation: z.number().optional(), endStation: z.number().optional(), stationInterval: z.number().positive().optional() });

const canonicalQuantityTakeoffInputShape = {
  action: z.enum(["corridor_volumes", "surface_volume", "pipe_network_lengths", "pressure_network_lengths", "parcel_areas", "alignment_lengths", "point_count_by_group", "export_to_csv", "material_list_get", "earthwork_summary"]),
  name: z.string().optional(),
  materials: z.array(z.string()).optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
  corridorName: z.string().optional(),
  region: RegionSchema.optional(),
  groupBySize: z.boolean().optional(),
  groupByMaterial: z.boolean().optional(),
  siteName: z.string().optional(),
  parcelNames: z.array(z.string()).optional(),
  names: z.array(z.string()).optional(),
  groupNames: z.array(z.string()).optional(),
  outputPath: z.string().optional(),
  includeCorridorVolumes: z.boolean().optional(),
  includeSurfaceVolumes: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includePressureNetworks: z.boolean().optional(),
  includeParcelAreas: z.boolean().optional(),
  includeAlignmentLengths: z.boolean().optional(),
  includeQuantities: z.boolean().optional(),
  designSurface: z.string().optional(),
  stationInterval: z.number().optional(),
};

export const QUANTITY_TAKEOFF_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "quantity_takeoff",
  actions: {
    corridor_volumes: { action: "corridor_volumes", inputSchema: QtyCorridorVolumesArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyCorridorVolumes"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyCorridorVolumes", { name: args.name, materials: args.materials ?? null, startStation: args.startStation ?? null, endStation: args.endStation ?? null })) },
    surface_volume: { action: "surface_volume", inputSchema: QtySurfaceVolumeArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtySurfaceVolume"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtySurfaceVolume", { baseSurface: args.baseSurface, comparisonSurface: args.comparisonSurface, corridorName: args.corridorName ?? null, region: args.region ?? null })) },
    pipe_network_lengths: { action: "pipe_network_lengths", inputSchema: QtyPipeNetworkLengthsArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyPipeNetworkLengths"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyPipeNetworkLengths", { name: args.name, groupBySize: args.groupBySize ?? false, groupByMaterial: args.groupByMaterial ?? false })) },
    pressure_network_lengths: { action: "pressure_network_lengths", inputSchema: QtyPressureNetworkLengthsArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyPressureNetworkLengths"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyPressureNetworkLengths", { name: args.name, groupBySize: args.groupBySize ?? false, groupByMaterial: args.groupByMaterial ?? false })) },
    parcel_areas: { action: "parcel_areas", inputSchema: QtyParcelAreasArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyParcelAreas"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyParcelAreas", { siteName: args.siteName ?? null, parcelNames: args.parcelNames ?? null })) },
    alignment_lengths: { action: "alignment_lengths", inputSchema: QtyAlignmentLengthsArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyAlignmentLengths"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyAlignmentLengths", { names: args.names ?? null, startStation: args.startStation ?? null, endStation: args.endStation ?? null })) },
    point_count_by_group: { action: "point_count_by_group", inputSchema: QtyPointCountByGroupArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyPointCountByGroup"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyPointCountByGroup", { groupNames: args.groupNames ?? null })) },
    export_to_csv: { action: "export_to_csv", inputSchema: QtyExportToCsvArgs, responseSchema: GenericResponseSchema, capabilities: ["export", "generate"], requiresActiveDrawing: true, safeForRetry: false, pluginMethods: ["qtyExportToCsv"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyExportToCsv", { outputPath: args.outputPath, includeCorridorVolumes: args.includeCorridorVolumes ?? false, includeSurfaceVolumes: args.includeSurfaceVolumes ?? false, includePipeNetworks: args.includePipeNetworks ?? false, includePressureNetworks: args.includePressureNetworks ?? false, includeParcelAreas: args.includeParcelAreas ?? false, includeAlignmentLengths: args.includeAlignmentLengths ?? false, corridorName: args.corridorName ?? null, baseSurface: args.baseSurface ?? null, comparisonSurface: args.comparisonSurface ?? null })) },
    material_list_get: { action: "material_list_get", inputSchema: QtyMaterialListGetArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyMaterialListGet"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyMaterialListGet", { corridorName: args.corridorName, includeQuantities: args.includeQuantities ?? false })) },
    earthwork_summary: { action: "earthwork_summary", inputSchema: QtyEarthworkSummaryArgs, responseSchema: GenericResponseSchema, capabilities: ["query", "analyze", "generate"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["qtyEarthworkSummary"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("qtyEarthworkSummary", { baseSurface: args.baseSurface, designSurface: args.designSurface, alignmentName: args.alignmentName ?? null, startStation: args.startStation ?? null, endStation: args.endStation ?? null, stationInterval: args.stationInterval ?? 50 })) },
  },
  exposures: [
    { toolName: "civil3d_quantity_takeoff", displayName: "Civil 3D Quantity Takeoff", description: "Calculates corridor, surface, network, parcel, alignment, point-group, material-list, earthwork, and export quantity-takeoff operations through a single domain tool.", inputShape: canonicalQuantityTakeoffInputShape, supportedActions: ["corridor_volumes", "surface_volume", "pipe_network_lengths", "pressure_network_lengths", "parcel_areas", "alignment_lengths", "point_count_by_group", "export_to_csv", "material_list_get", "earthwork_summary"], resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }) },
    { toolName: "civil3d_qty_corridor_volumes", displayName: "Civil 3D Quantity Corridor Volumes", description: "Calculates corridor material volumes.", inputShape: { name: z.string(), materials: z.array(z.string()).optional(), startStation: z.number().optional(), endStation: z.number().optional() }, supportedActions: ["corridor_volumes"], resolveAction: (rawArgs) => ({ action: "corridor_volumes", args: { action: "corridor_volumes", ...rawArgs } }) },
    { toolName: "civil3d_qty_surface_volume", displayName: "Civil 3D Quantity Surface Volume", description: "Calculates cut/fill volume between two surfaces.", inputShape: { baseSurface: z.string(), comparisonSurface: z.string(), corridorName: z.string().optional(), region: RegionSchema.optional() }, supportedActions: ["surface_volume"], resolveAction: (rawArgs) => ({ action: "surface_volume", args: { action: "surface_volume", ...rawArgs } }) },
    { toolName: "civil3d_qty_pipe_network_lengths", displayName: "Civil 3D Quantity Pipe Network Lengths", description: "Summarizes gravity pipe-network lengths.", inputShape: { name: z.string(), groupBySize: z.boolean().optional(), groupByMaterial: z.boolean().optional() }, supportedActions: ["pipe_network_lengths"], resolveAction: (rawArgs) => ({ action: "pipe_network_lengths", args: { action: "pipe_network_lengths", ...rawArgs } }) },
    { toolName: "civil3d_qty_pressure_network_lengths", displayName: "Civil 3D Quantity Pressure Network Lengths", description: "Summarizes pressure-network lengths.", inputShape: { name: z.string(), groupBySize: z.boolean().optional(), groupByMaterial: z.boolean().optional() }, supportedActions: ["pressure_network_lengths"], resolveAction: (rawArgs) => ({ action: "pressure_network_lengths", args: { action: "pressure_network_lengths", ...rawArgs } }) },
    { toolName: "civil3d_qty_parcel_areas", displayName: "Civil 3D Quantity Parcel Areas", description: "Lists parcel areas and perimeter data.", inputShape: { siteName: z.string().optional(), parcelNames: z.array(z.string()).optional() }, supportedActions: ["parcel_areas"], resolveAction: (rawArgs) => ({ action: "parcel_areas", args: { action: "parcel_areas", ...rawArgs } }) },
    { toolName: "civil3d_qty_alignment_lengths", displayName: "Civil 3D Quantity Alignment Lengths", description: "Calculates alignment lengths.", inputShape: { names: z.array(z.string()).optional(), startStation: z.number().optional(), endStation: z.number().optional() }, supportedActions: ["alignment_lengths"], resolveAction: (rawArgs) => ({ action: "alignment_lengths", args: { action: "alignment_lengths", ...rawArgs } }) },
    { toolName: "civil3d_qty_point_count_by_group", displayName: "Civil 3D Quantity Point Count By Group", description: "Counts points by point group.", inputShape: { groupNames: z.array(z.string()).optional() }, supportedActions: ["point_count_by_group"], resolveAction: (rawArgs) => ({ action: "point_count_by_group", args: { action: "point_count_by_group", ...rawArgs } }) },
    { toolName: "civil3d_qty_export_to_csv", displayName: "Civil 3D Quantity Export To CSV", description: "Exports a consolidated quantity report to CSV.", inputShape: { outputPath: z.string(), includeCorridorVolumes: z.boolean().optional(), includeSurfaceVolumes: z.boolean().optional(), includePipeNetworks: z.boolean().optional(), includePressureNetworks: z.boolean().optional(), includeParcelAreas: z.boolean().optional(), includeAlignmentLengths: z.boolean().optional(), corridorName: z.string().optional(), baseSurface: z.string().optional(), comparisonSurface: z.string().optional() }, supportedActions: ["export_to_csv"], resolveAction: (rawArgs) => ({ action: "export_to_csv", args: { action: "export_to_csv", ...rawArgs } }) },
    { toolName: "civil3d_qty_material_list_get", displayName: "Civil 3D Quantity Material List Get", description: "Gets a corridor material list and optional quantities.", inputShape: { corridorName: z.string(), includeQuantities: z.boolean().optional() }, supportedActions: ["material_list_get"], resolveAction: (rawArgs) => ({ action: "material_list_get", args: { action: "material_list_get", ...rawArgs } }) },
    { toolName: "civil3d_qty_earthwork_summary", displayName: "Civil 3D Quantity Earthwork Summary", description: "Generates an earthwork summary between surfaces.", inputShape: { baseSurface: z.string(), designSurface: z.string(), alignmentName: z.string().optional(), startStation: z.number().optional(), endStation: z.number().optional(), stationInterval: z.number().positive().optional() }, supportedActions: ["earthwork_summary"], resolveAction: (rawArgs) => ({ action: "earthwork_summary", args: { action: "earthwork_summary", ...rawArgs } }) },
  ],
};
