import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

const OptionalPoint3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().optional().default(0),
});

const PipeFlowSchema = z.object({
  pipeName: z.string(),
  designFlow: z.number().positive(),
});

const GenericPipeResponseSchema = z.object({}).passthrough();

const canonicalPipeInputShape = {
  action: z.enum([
    "list",
    "get",
    "get_pipe",
    "get_structure",
    "check_interference",
    "create",
    "add_pipe",
    "add_structure",
    "catalog_list",
    "calculate_hgl",
    "hydraulic_analysis",
    "get_structure_properties",
    "size_network",
    "automate_profile_view",
    "list_pressure_networks",
    "get_pressure_network",
    "create_pressure_network",
    "delete_pressure_network",
    "assign_pressure_parts_list",
    "set_pressure_cover",
    "validate_pressure_network",
    "export_pressure_network",
    "connect_pressure_networks",
    "add_pressure_pipe",
    "get_pressure_pipe_properties",
    "resize_pressure_pipe",
    "add_pressure_fitting",
    "get_pressure_fitting_properties",
    "add_pressure_appurtenance",
  ]),
  name: z.string().optional(),
  networkName: z.string().optional(),
  pipeName: z.string().optional(),
  structureName: z.string().optional(),
  fittingName: z.string().optional(),
  targetType: z.enum(["surface", "pipe_network"]).optional(),
  targetName: z.string().optional(),
  partsList: z.string().optional(),
  referenceSurface: z.string().optional(),
  referenceAlignment: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  startPoint: Point3DSchema.optional(),
  endPoint: Point3DSchema.optional(),
  startStructure: z.string().optional(),
  endStructure: z.string().optional(),
  partName: z.string().optional(),
  diameter: z.number().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  rimElevation: z.number().optional(),
  sumpDepth: z.number().optional(),
  tailwaterElevation: z.number().optional(),
  designFlow: z.number().optional(),
  manningsN: z.number().positive().optional(),
  minCoverDepth: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
  defaultDesignFlow: z.number().positive().optional(),
  perPipeDesignFlows: z.array(PipeFlowSchema).optional(),
  targetVelocityMin: z.number().positive().optional(),
  targetVelocityMax: z.number().positive().optional(),
  applyChanges: z.boolean().optional(),
  profileViewName: z.string().optional(),
  insertX: z.number().optional(),
  insertY: z.number().optional(),
  alignmentName: z.string().optional(),
  surfaceName: z.string().optional(),
  existingProfileName: z.string().optional(),
  surfaceProfileName: z.string().optional(),
  createSurfaceProfileIfMissing: z.boolean().optional(),
  bandSet: z.string().optional(),
  maxCoverDepth: z.number().optional(),
  includeCoordinates: z.boolean().optional(),
  targetNetwork: z.string().optional(),
  sourceNetwork: z.string().optional(),
  newPartName: z.string().optional(),
  newDiameter: z.number().optional(),
  position: OptionalPoint3DSchema.optional(),
  rotation: z.number().optional(),
  onPipeName: z.string().optional(),
};

const PipeListArgsSchema = z.object({ action: z.literal("list") });
const PipeGetArgsSchema = z.object({ action: z.literal("get"), name: z.string() });
const PipeGetPipeArgsSchema = z.object({ action: z.literal("get_pipe"), networkName: z.string(), pipeName: z.string() });
const PipeGetStructureArgsSchema = z.object({ action: z.literal("get_structure"), networkName: z.string(), structureName: z.string() });
const PipeCheckInterferenceArgsSchema = z.object({
  action: z.literal("check_interference"),
  networkName: z.string(),
  targetType: z.enum(["surface", "pipe_network"]),
  targetName: z.string(),
});
const PipeCreateArgsSchema = z.object({
  action: z.literal("create"),
  name: z.string(),
  partsList: z.string(),
  referenceSurface: z.string().optional(),
  referenceAlignment: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
});
const PipeAddPipeArgsSchema = z.object({
  action: z.literal("add_pipe"),
  networkName: z.string(),
  startPoint: Point3DSchema.optional(),
  endPoint: Point3DSchema.optional(),
  startStructure: z.string().optional(),
  endStructure: z.string().optional(),
  partName: z.string(),
  diameter: z.number().optional(),
}).superRefine((value, ctx) => {
  if (!value.startPoint && !value.startStructure) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: "Either startPoint or startStructure is required.", path: ["startPoint"] });
  }
  if (!value.endPoint && !value.endStructure) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: "Either endPoint or endStructure is required.", path: ["endPoint"] });
  }
});
const PipeAddStructureArgsSchema = z.object({
  action: z.literal("add_structure"),
  networkName: z.string(),
  x: z.number(),
  y: z.number(),
  partName: z.string(),
  rimElevation: z.number().optional(),
  sumpDepth: z.number().optional(),
});
const PipeCatalogListArgsSchema = z.object({ action: z.literal("catalog_list"), partsList: z.string().optional() });
const PipeCalculateHglArgsSchema = z.object({
  action: z.literal("calculate_hgl"),
  networkName: z.string(),
  tailwaterElevation: z.number().optional(),
  designFlow: z.number().optional(),
  manningsN: z.number().positive().optional(),
});
const PipeHydraulicAnalysisArgsSchema = z.object({
  action: z.literal("hydraulic_analysis"),
  networkName: z.string(),
  designFlow: z.number().optional(),
  manningsN: z.number().positive().optional(),
  minCoverDepth: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
});
const PipeStructurePropertiesArgsSchema = z.object({
  action: z.literal("get_structure_properties"),
  networkName: z.string(),
  structureName: z.string(),
});
const PipeSizeNetworkArgsSchema = z.object({
  action: z.literal("size_network"),
  networkName: z.string(),
  partsList: z.string().optional(),
  defaultDesignFlow: z.number().positive().optional(),
  perPipeDesignFlows: z.array(PipeFlowSchema).optional(),
  manningsN: z.number().positive().optional().default(0.013),
  targetVelocityMin: z.number().positive().optional().default(2.0),
  targetVelocityMax: z.number().positive().optional().default(10.0),
  applyChanges: z.boolean().optional().default(false),
});
const PipeAutomateProfileViewArgsSchema = z.object({
  action: z.literal("automate_profile_view"),
  networkName: z.string(),
  profileViewName: z.string(),
  insertX: z.number(),
  insertY: z.number(),
  alignmentName: z.string().optional(),
  surfaceName: z.string().optional(),
  existingProfileName: z.string().optional(),
  surfaceProfileName: z.string().optional(),
  createSurfaceProfileIfMissing: z.boolean().optional().default(true),
  style: z.string().optional(),
  bandSet: z.string().optional(),
});
const PipeListPressureNetworksArgsSchema = z.object({ action: z.literal("list_pressure_networks") });
const PipeGetPressureNetworkArgsSchema = z.object({ action: z.literal("get_pressure_network"), name: z.string() });
const PipeCreatePressureNetworkArgsSchema = z.object({
  action: z.literal("create_pressure_network"),
  name: z.string(),
  partsList: z.string(),
  layer: z.string().optional(),
  referenceAlignment: z.string().optional(),
  referenceSurface: z.string().optional(),
});
const PipeDeletePressureNetworkArgsSchema = z.object({ action: z.literal("delete_pressure_network"), name: z.string() });
const PipeAssignPressurePartsListArgsSchema = z.object({
  action: z.literal("assign_pressure_parts_list"),
  networkName: z.string(),
  partsList: z.string(),
});
const PipeSetPressureCoverArgsSchema = z.object({
  action: z.literal("set_pressure_cover"),
  networkName: z.string(),
  minCoverDepth: z.number(),
  maxCoverDepth: z.number().optional(),
});
const PipeValidatePressureNetworkArgsSchema = z.object({ action: z.literal("validate_pressure_network"), networkName: z.string() });
const PipeExportPressureNetworkArgsSchema = z.object({
  action: z.literal("export_pressure_network"),
  networkName: z.string(),
  includeCoordinates: z.boolean().optional().default(true),
});
const PipeConnectPressureNetworksArgsSchema = z.object({
  action: z.literal("connect_pressure_networks"),
  targetNetwork: z.string(),
  sourceNetwork: z.string(),
});
const PipeAddPressurePipeArgsSchema = z.object({
  action: z.literal("add_pressure_pipe"),
  networkName: z.string(),
  partName: z.string(),
  startPoint: OptionalPoint3DSchema,
  endPoint: OptionalPoint3DSchema,
  diameter: z.number().optional(),
});
const PipeGetPressurePipePropertiesArgsSchema = z.object({
  action: z.literal("get_pressure_pipe_properties"),
  networkName: z.string(),
  pipeName: z.string(),
});
const PipeResizePressurePipeArgsSchema = z.object({
  action: z.literal("resize_pressure_pipe"),
  networkName: z.string(),
  pipeName: z.string(),
  newPartName: z.string(),
  newDiameter: z.number().optional(),
});
const PipeAddPressureFittingArgsSchema = z.object({
  action: z.literal("add_pressure_fitting"),
  networkName: z.string(),
  partName: z.string(),
  position: OptionalPoint3DSchema,
  rotation: z.number().optional(),
});
const PipeGetPressureFittingPropertiesArgsSchema = z.object({
  action: z.literal("get_pressure_fitting_properties"),
  networkName: z.string(),
  fittingName: z.string(),
});
const PipeAddPressureAppurtenanceArgsSchema = z.object({
  action: z.literal("add_pressure_appurtenance"),
  networkName: z.string(),
  partName: z.string(),
  position: OptionalPoint3DSchema,
  rotation: z.number().optional(),
  onPipeName: z.string().optional(),
});

type PipeNetworkDetail = {
  name: string;
  partsList?: string;
  referenceAlignment?: string;
  referenceSurface?: string;
  pipes: Array<{ name: string; diameter: number; slope: number; length: number }>;
};

type PartsCatalogResponse = {
  partsLists: Array<{ name: string | null; parts: string[] }>;
};

function parsePartDiameter(partName: string): number | null {
  const match = partName.match(/(\d+(?:\.\d+)?)/);
  if (!match) return null;
  const value = Number(match[1]);
  return Number.isFinite(value) ? value : null;
}

function computeFullFlowCapacity(diameter: number, slopePct: number, manningsN: number): number {
  const slope = Math.max(Math.abs(slopePct) / 100, 1e-6);
  const area = Math.PI * diameter * diameter / 4;
  const hydraulicRadius = diameter / 4;
  return (1 / manningsN) * area * Math.pow(hydraulicRadius, 2 / 3) * Math.sqrt(slope);
}

function computeVelocity(flow: number, diameter: number): number {
  const area = Math.PI * diameter * diameter / 4;
  return area > 0 ? flow / area : 0;
}

function solveRequiredDiameter(flow: number, slopePct: number, manningsN: number): number {
  let low = 0.01;
  let high = 100;
  while (computeFullFlowCapacity(high, slopePct, manningsN) < flow && high < 1_000_000) high *= 2;
  for (let i = 0; i < 60; i++) {
    const mid = (low + high) / 2;
    const capacity = computeFullFlowCapacity(mid, slopePct, manningsN);
    if (capacity >= flow) high = mid;
    else low = mid;
  }
  return high;
}

function chooseBestPart(partNames: string[], requiredDiameter: number, flow: number, velocityMin: number, velocityMax: number) {
  const candidates = partNames
    .map((name) => ({ name, diameter: parsePartDiameter(name) }))
    .filter((candidate): candidate is { name: string; diameter: number } => candidate.diameter != null)
    .sort((a, b) => a.diameter - b.diameter);

  const preferred = candidates.find((candidate) => {
    if (candidate.diameter < requiredDiameter) return false;
    const velocity = computeVelocity(flow, candidate.diameter);
    return velocity >= velocityMin && velocity <= velocityMax;
  });

  return preferred ?? candidates.find((candidate) => candidate.diameter >= requiredDiameter) ?? candidates[candidates.length - 1] ?? null;
}

export const PIPE_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "pipe",
  actions: {
    list: {
      action: "list",
      inputSchema: PipeListArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listPipeNetworks"],
      execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("listPipeNetworks", {})),
    },
    get: {
      action: "get",
      inputSchema: PipeGetArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPipeNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPipeNetwork", { name: args.name })),
    },
    get_pipe: {
      action: "get_pipe",
      inputSchema: PipeGetPipeArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPipe"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPipe", { networkName: args.networkName, pipeName: args.pipeName })),
    },
    get_structure: {
      action: "get_structure",
      inputSchema: PipeGetStructureArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getStructure"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getStructure", { networkName: args.networkName, structureName: args.structureName })),
    },
    check_interference: {
      action: "check_interference",
      inputSchema: PipeCheckInterferenceArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["checkPipeNetworkInterference"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("checkPipeNetworkInterference", {
        networkName: args.networkName,
        targetType: args.targetType,
        targetName: args.targetName,
      })),
    },
    create: {
      action: "create",
      inputSchema: PipeCreateArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createPipeNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("createPipeNetwork", {
        name: args.name,
        partsList: args.partsList,
        referenceSurface: args.referenceSurface,
        referenceAlignment: args.referenceAlignment,
        style: args.style,
        layer: args.layer,
      })),
    },
    add_pipe: {
      action: "add_pipe",
      inputSchema: PipeAddPipeArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addPipeToNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("addPipeToNetwork", {
        networkName: args.networkName,
        startPoint: args.startPoint,
        endPoint: args.endPoint,
        startStructure: args.startStructure,
        endStructure: args.endStructure,
        partName: args.partName,
        diameter: args.diameter,
      })),
    },
    add_structure: {
      action: "add_structure",
      inputSchema: PipeAddStructureArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addStructureToNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("addStructureToNetwork", {
        networkName: args.networkName,
        x: args.x,
        y: args.y,
        partName: args.partName,
        rimElevation: args.rimElevation,
        sumpDepth: args.sumpDepth,
      })),
    },
    catalog_list: {
      action: "catalog_list",
      inputSchema: PipeCatalogListArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listPipePartsCatalog"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("listPipePartsCatalog", { partsList: args.partsList })),
    },
    calculate_hgl: {
      action: "calculate_hgl",
      inputSchema: PipeCalculateHglArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["calculatePipeNetworkHgl"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("calculatePipeNetworkHgl", {
        networkName: args.networkName,
        tailwaterElevation: args.tailwaterElevation ?? null,
        designFlow: args.designFlow ?? null,
        manningsN: args.manningsN ?? 0.013,
      })),
    },
    hydraulic_analysis: {
      action: "hydraulic_analysis",
      inputSchema: PipeHydraulicAnalysisArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["analyzePipeNetworkHydraulics"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("analyzePipeNetworkHydraulics", {
        networkName: args.networkName,
        designFlow: args.designFlow ?? null,
        manningsN: args.manningsN ?? 0.013,
        minCoverDepth: args.minCoverDepth ?? 2.0,
        minVelocity: args.minVelocity ?? 2.0,
        maxVelocity: args.maxVelocity ?? 10.0,
        minSlope: args.minSlope ?? 0.5,
      })),
    },
    get_structure_properties: {
      action: "get_structure_properties",
      inputSchema: PipeStructurePropertiesArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPipeStructureProperties"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPipeStructureProperties", {
        networkName: args.networkName,
        structureName: args.structureName,
      })),
    },
    size_network: {
      action: "size_network",
      inputSchema: PipeSizeNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["analyze", "edit", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["getPipeNetwork", "listPipePartsCatalog", "resizePipeInNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const network = await appClient.sendCommand("getPipeNetwork", { name: args.networkName }) as PipeNetworkDetail;
        const partsListName = args.partsList ?? network.partsList;
        if (!partsListName) throw new Error("No parts list was provided and the pipe network does not expose one.");

        const catalog = await appClient.sendCommand("listPipePartsCatalog", { partsList: partsListName }) as PartsCatalogResponse;
        const parts = catalog.partsLists.find((item) => item.name === partsListName)?.parts ?? [];
        if (parts.length === 0) throw new Error(`Parts list '${partsListName}' does not contain any pipe parts.`);

        const perPipeDesignFlows = (args.perPipeDesignFlows ?? []) as Array<z.infer<typeof PipeFlowSchema>>;
        const flowMap = new Map(perPipeDesignFlows.map((item) => [item.pipeName.toLowerCase(), item.designFlow]));
        const recommendations: Array<Record<string, unknown>> = [];
        let appliedCount = 0;

        for (const pipe of network.pipes) {
          const designFlow = flowMap.get(pipe.name.toLowerCase()) ?? args.defaultDesignFlow;
          if (!designFlow) {
            recommendations.push({ pipeName: pipe.name, currentDiameter: pipe.diameter, status: "skipped", reason: "No design flow was supplied for this pipe." });
            continue;
          }

          const resolvedDesignFlow = Number(designFlow);
          const requiredDiameter = solveRequiredDiameter(resolvedDesignFlow, pipe.slope, Number(args.manningsN));
          const selectedPart = chooseBestPart(parts, requiredDiameter, resolvedDesignFlow, Number(args.targetVelocityMin), Number(args.targetVelocityMax));
          if (!selectedPart) {
            recommendations.push({ pipeName: pipe.name, currentDiameter: pipe.diameter, status: "skipped", reason: "No catalog part could be parsed into a numeric diameter." });
            continue;
          }

          const selectedVelocity = computeVelocity(resolvedDesignFlow, selectedPart.diameter);
          const shouldApply = args.applyChanges && (selectedPart.name !== pipe.name || Math.abs(selectedPart.diameter - pipe.diameter) > 1e-6);

          if (shouldApply) {
            await appClient.sendCommand("resizePipeInNetwork", {
              networkName: args.networkName,
              pipeName: pipe.name,
              newPartName: selectedPart.name,
              newDiameter: selectedPart.diameter,
            });
            appliedCount++;
          }

          recommendations.push({
            pipeName: pipe.name,
            currentDiameter: pipe.diameter,
            designFlow: resolvedDesignFlow,
            requiredDiameter: Number(requiredDiameter.toFixed(3)),
            selectedPart: selectedPart.name,
            selectedDiameter: selectedPart.diameter,
            selectedVelocity: Number(selectedVelocity.toFixed(3)),
            applied: shouldApply,
            status: "ok",
          });
        }

        return { networkName: args.networkName, partsList: partsListName, applyChanges: args.applyChanges, appliedCount, recommendations };
      }),
    },
    automate_profile_view: {
      action: "automate_profile_view",
      inputSchema: PipeAutomateProfileViewArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "manage", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["getPipeNetwork", "listProfiles", "createProfileFromSurface", "profileViewCreate"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const network = await appClient.sendCommand("getPipeNetwork", { name: args.networkName }) as PipeNetworkDetail;
        const alignmentName = args.alignmentName ?? network.referenceAlignment;
        if (!alignmentName) throw new Error("The pipe network does not expose a reference alignment. Provide 'alignmentName' explicitly.");

        const surfaceName = args.surfaceName ?? network.referenceSurface;
        const profileName = String(args.existingProfileName ?? args.surfaceProfileName ?? `EG_${alignmentName}`);
        const profileList = await appClient.sendCommand("listProfiles", { alignmentName }) as { profiles?: Array<{ name: string }> };
        const existingNames = new Set((profileList.profiles ?? []).map((profile) => profile.name.toLowerCase()));

        if (!existingNames.has(profileName.toLowerCase())) {
          if (!args.createSurfaceProfileIfMissing) throw new Error(`Profile '${profileName}' does not exist and automatic creation is disabled.`);
          if (!surfaceName) throw new Error("No surface was supplied or found on the pipe network, so the EG profile cannot be created.");
          await appClient.sendCommand("createProfileFromSurface", { alignmentName, profileName, surfaceName });
        }

        const profileView = await appClient.sendCommand("profileViewCreate", {
          alignmentName,
          profileViewName: args.profileViewName,
          insertX: args.insertX,
          insertY: args.insertY,
          style: args.style,
          bandSet: args.bandSet,
        });

        return { networkName: args.networkName, alignmentName, surfaceName: surfaceName ?? null, profileName, profileView };
      }),
    },
    list_pressure_networks: {
      action: "list_pressure_networks",
      inputSchema: PipeListPressureNetworksArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listPressureNetworks"],
      execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("listPressureNetworks", {})),
    },
    get_pressure_network: {
      action: "get_pressure_network",
      inputSchema: PipeGetPressureNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPressureNetworkInfo"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPressureNetworkInfo", { name: args.name })),
    },
    create_pressure_network: {
      action: "create_pressure_network",
      inputSchema: PipeCreatePressureNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createPressureNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("createPressureNetwork", {
        name: args.name,
        partsList: args.partsList,
        layer: args.layer,
        referenceAlignment: args.referenceAlignment,
        referenceSurface: args.referenceSurface,
      })),
    },
    delete_pressure_network: {
      action: "delete_pressure_network",
      inputSchema: PipeDeletePressureNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["delete"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deletePressureNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("deletePressureNetwork", { name: args.name })),
    },
    assign_pressure_parts_list: {
      action: "assign_pressure_parts_list",
      inputSchema: PipeAssignPressurePartsListArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["assignPressurePartsList"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("assignPressurePartsList", {
        networkName: args.networkName,
        partsList: args.partsList,
      })),
    },
    set_pressure_cover: {
      action: "set_pressure_cover",
      inputSchema: PipeSetPressureCoverArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["setPressureNetworkCover"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("setPressureNetworkCover", {
        networkName: args.networkName,
        minCoverDepth: args.minCoverDepth,
        maxCoverDepth: args.maxCoverDepth,
      })),
    },
    validate_pressure_network: {
      action: "validate_pressure_network",
      inputSchema: PipeValidatePressureNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "analyze", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["validatePressureNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("validatePressureNetwork", { networkName: args.networkName })),
    },
    export_pressure_network: {
      action: "export_pressure_network",
      inputSchema: PipeExportPressureNetworkArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "export"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["exportPressureNetwork"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("exportPressureNetwork", {
        networkName: args.networkName,
        includeCoordinates: args.includeCoordinates,
      })),
    },
    connect_pressure_networks: {
      action: "connect_pressure_networks",
      inputSchema: PipeConnectPressureNetworksArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["connectPressureNetworks"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("connectPressureNetworks", {
        targetNetwork: args.targetNetwork,
        sourceNetwork: args.sourceNetwork,
      })),
    },
    add_pressure_pipe: {
      action: "add_pressure_pipe",
      inputSchema: PipeAddPressurePipeArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addPressurePipe"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("addPressurePipe", {
        networkName: args.networkName,
        partName: args.partName,
        startPoint: args.startPoint,
        endPoint: args.endPoint,
        diameter: args.diameter,
      })),
    },
    get_pressure_pipe_properties: {
      action: "get_pressure_pipe_properties",
      inputSchema: PipeGetPressurePipePropertiesArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPressurePipeProperties"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPressurePipeProperties", {
        networkName: args.networkName,
        pipeName: args.pipeName,
      })),
    },
    resize_pressure_pipe: {
      action: "resize_pressure_pipe",
      inputSchema: PipeResizePressurePipeArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["resizePressurePipe"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("resizePressurePipe", {
        networkName: args.networkName,
        pipeName: args.pipeName,
        newPartName: args.newPartName,
        newDiameter: args.newDiameter,
      })),
    },
    add_pressure_fitting: {
      action: "add_pressure_fitting",
      inputSchema: PipeAddPressureFittingArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addPressureFitting"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("addPressureFitting", {
        networkName: args.networkName,
        partName: args.partName,
        position: args.position,
        rotation: args.rotation,
      })),
    },
    get_pressure_fitting_properties: {
      action: "get_pressure_fitting_properties",
      inputSchema: PipeGetPressureFittingPropertiesArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getPressureFittingProperties"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getPressureFittingProperties", {
        networkName: args.networkName,
        fittingName: args.fittingName,
      })),
    },
    add_pressure_appurtenance: {
      action: "add_pressure_appurtenance",
      inputSchema: PipeAddPressureAppurtenanceArgsSchema,
      responseSchema: GenericPipeResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addPressureAppurtenance"],
      execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("addPressureAppurtenance", {
        networkName: args.networkName,
        partName: args.partName,
        position: args.position,
        rotation: args.rotation,
        onPipeName: args.onPipeName,
      })),
    },
  },
  exposures: [
    {
      toolName: "civil3d_pipe",
      displayName: "Civil 3D Pipe",
      description: "Reads, analyzes, designs, and manages Civil 3D gravity and pressure pipe systems through a single domain tool.",
      inputShape: canonicalPipeInputShape,
      supportedActions: [
        "list", "get", "get_pipe", "get_structure", "check_interference", "create", "add_pipe", "add_structure",
        "catalog_list", "calculate_hgl", "hydraulic_analysis", "get_structure_properties", "size_network", "automate_profile_view",
        "list_pressure_networks", "get_pressure_network", "create_pressure_network", "delete_pressure_network",
        "assign_pressure_parts_list", "set_pressure_cover", "validate_pressure_network", "export_pressure_network",
        "connect_pressure_networks", "add_pressure_pipe", "get_pressure_pipe_properties", "resize_pressure_pipe",
        "add_pressure_fitting", "get_pressure_fitting_properties", "add_pressure_appurtenance",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_pipe_network",
      displayName: "Civil 3D Pipe Network",
      description: "Reads Civil 3D pipe network data including networks, pipes, structures, and interference checks.",
      inputShape: {
        action: z.enum(["list", "get", "get_pipe", "get_structure", "check_interference"]),
        name: z.string().optional(),
        networkName: z.string().optional(),
        pipeName: z.string().optional(),
        structureName: z.string().optional(),
        targetType: z.enum(["surface", "pipe_network"]).optional(),
        targetName: z.string().optional(),
      },
      supportedActions: ["list", "get", "get_pipe", "get_structure", "check_interference"],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_pipe_network_edit",
      displayName: "Civil 3D Pipe Network Edit",
      description: "Creates and modifies Civil 3D pipe networks, pipes, and structures.",
      inputShape: {
        action: z.enum(["create", "add_pipe", "add_structure"]),
        name: z.string().optional(),
        partsList: z.string().optional(),
        referenceSurface: z.string().optional(),
        referenceAlignment: z.string().optional(),
        style: z.string().optional(),
        layer: z.string().optional(),
        networkName: z.string().optional(),
        startPoint: Point3DSchema.optional(),
        endPoint: Point3DSchema.optional(),
        startStructure: z.string().optional(),
        endStructure: z.string().optional(),
        partName: z.string().optional(),
        diameter: z.number().optional(),
        x: z.number().optional(),
        y: z.number().optional(),
        rimElevation: z.number().optional(),
        sumpDepth: z.number().optional(),
      },
      supportedActions: ["create", "add_pipe", "add_structure"],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_pipe_catalog",
      displayName: "Civil 3D Pipe Catalog",
      description: "Lists available Civil 3D pipe parts lists and part names to help choose valid inputs for pipe network creation and editing tools.",
      inputShape: { partsList: z.string().optional() },
      supportedActions: ["catalog_list"],
      resolveAction: (rawArgs) => ({ action: "catalog_list", args: { action: "catalog_list", partsList: rawArgs.partsList } }),
    },
    {
      toolName: "civil3d_pipe_network_hgl_calculate",
      displayName: "Civil 3D Pipe Network HGL Calculate",
      description: "Calculates hydraulic grade line and energy grade line values for a gravity pipe network.",
      inputShape: { networkName: z.string(), tailwaterElevation: z.number().optional(), designFlow: z.number().optional(), manningsN: z.number().positive().optional() },
      supportedActions: ["calculate_hgl"],
      resolveAction: (rawArgs) => ({ action: "calculate_hgl", args: { action: "calculate_hgl", networkName: rawArgs.networkName, tailwaterElevation: rawArgs.tailwaterElevation, designFlow: rawArgs.designFlow, manningsN: rawArgs.manningsN } }),
    },
    {
      toolName: "civil3d_pipe_hydraulic_analysis",
      displayName: "Civil 3D Pipe Hydraulic Analysis",
      description: "Runs hydraulic capacity analysis on a gravity pipe network using Manning-based checks.",
      inputShape: { networkName: z.string(), designFlow: z.number().optional(), manningsN: z.number().positive().optional(), minCoverDepth: z.number().nonnegative().optional(), minVelocity: z.number().nonnegative().optional(), maxVelocity: z.number().positive().optional(), minSlope: z.number().nonnegative().optional() },
      supportedActions: ["hydraulic_analysis"],
      resolveAction: (rawArgs) => ({ action: "hydraulic_analysis", args: { action: "hydraulic_analysis", networkName: rawArgs.networkName, designFlow: rawArgs.designFlow, manningsN: rawArgs.manningsN, minCoverDepth: rawArgs.minCoverDepth, minVelocity: rawArgs.minVelocity, maxVelocity: rawArgs.maxVelocity, minSlope: rawArgs.minSlope } }),
    },
    {
      toolName: "civil3d_pipe_structure_properties",
      displayName: "Civil 3D Pipe Structure Properties",
      description: "Retrieves detailed properties for a structure in a gravity pipe network.",
      inputShape: { networkName: z.string(), structureName: z.string() },
      supportedActions: ["get_structure_properties"],
      resolveAction: (rawArgs) => ({ action: "get_structure_properties", args: { action: "get_structure_properties", networkName: rawArgs.networkName, structureName: rawArgs.structureName } }),
    },
    {
      toolName: "civil3d_pipe_network_size",
      displayName: "Civil 3D Pipe Network Size",
      description: "Sizes gravity-network pipes from Manning full-flow capacity, chooses matching catalog parts, and optionally applies the selected sizes back to the drawing.",
      inputShape: { networkName: z.string(), partsList: z.string().optional(), defaultDesignFlow: z.number().positive().optional(), perPipeDesignFlows: z.array(PipeFlowSchema).optional(), manningsN: z.number().positive().optional().default(0.013), targetVelocityMin: z.number().positive().optional().default(2), targetVelocityMax: z.number().positive().optional().default(10), applyChanges: z.boolean().optional().default(false) },
      supportedActions: ["size_network"],
      resolveAction: (rawArgs) => ({ action: "size_network", args: { action: "size_network", networkName: rawArgs.networkName, partsList: rawArgs.partsList, defaultDesignFlow: rawArgs.defaultDesignFlow, perPipeDesignFlows: rawArgs.perPipeDesignFlows, manningsN: rawArgs.manningsN, targetVelocityMin: rawArgs.targetVelocityMin, targetVelocityMax: rawArgs.targetVelocityMax, applyChanges: rawArgs.applyChanges } }),
    },
    {
      toolName: "civil3d_pipe_profile_view_automation",
      displayName: "Civil 3D Pipe Profile View Automation",
      description: "Automates a gravity-pipe profile-view setup by resolving the network alignment/surface, creating an EG profile if needed, and creating the profile view with optional style and band set.",
      inputShape: { networkName: z.string(), profileViewName: z.string(), insertX: z.number(), insertY: z.number(), alignmentName: z.string().optional(), surfaceName: z.string().optional(), existingProfileName: z.string().optional(), surfaceProfileName: z.string().optional(), createSurfaceProfileIfMissing: z.boolean().optional().default(true), style: z.string().optional(), bandSet: z.string().optional() },
      supportedActions: ["automate_profile_view"],
      resolveAction: (rawArgs) => ({ action: "automate_profile_view", args: { action: "automate_profile_view", networkName: rawArgs.networkName, profileViewName: rawArgs.profileViewName, insertX: rawArgs.insertX, insertY: rawArgs.insertY, alignmentName: rawArgs.alignmentName, surfaceName: rawArgs.surfaceName, existingProfileName: rawArgs.existingProfileName, surfaceProfileName: rawArgs.surfaceProfileName, createSurfaceProfileIfMissing: rawArgs.createSurfaceProfileIfMissing, style: rawArgs.style, bandSet: rawArgs.bandSet } }),
    },
    {
      toolName: "civil3d_pressure_network_list",
      displayName: "Civil 3D Pressure Network List",
      description: "Lists all pressure networks in the active Civil 3D drawing with summary counts for pipes, fittings, and appurtenances.",
      inputShape: {},
      supportedActions: ["list_pressure_networks"],
      resolveAction: () => ({ action: "list_pressure_networks", args: { action: "list_pressure_networks" } }),
    },
    {
      toolName: "civil3d_pressure_network_get_info",
      displayName: "Civil 3D Pressure Network Get Info",
      description: "Gets detailed information about a pressure network including its pipes, fittings, and appurtenances.",
      inputShape: { name: z.string() },
      supportedActions: ["get_pressure_network"],
      resolveAction: (rawArgs) => ({ action: "get_pressure_network", args: { action: "get_pressure_network", name: rawArgs.name } }),
    },
    {
      toolName: "civil3d_pressure_network_create",
      displayName: "Civil 3D Pressure Network Create",
      description: "Creates a new pressure network in the active Civil 3D drawing.",
      inputShape: { name: z.string(), partsList: z.string(), layer: z.string().optional(), referenceAlignment: z.string().optional(), referenceSurface: z.string().optional() },
      supportedActions: ["create_pressure_network"],
      resolveAction: (rawArgs) => ({ action: "create_pressure_network", args: { action: "create_pressure_network", name: rawArgs.name, partsList: rawArgs.partsList, layer: rawArgs.layer, referenceAlignment: rawArgs.referenceAlignment, referenceSurface: rawArgs.referenceSurface } }),
    },
    {
      toolName: "civil3d_pressure_network_delete",
      displayName: "Civil 3D Pressure Network Delete",
      description: "Deletes a pressure network and all its components from the drawing.",
      inputShape: { name: z.string() },
      supportedActions: ["delete_pressure_network"],
      resolveAction: (rawArgs) => ({ action: "delete_pressure_network", args: { action: "delete_pressure_network", name: rawArgs.name } }),
    },
    {
      toolName: "civil3d_pressure_network_assign_parts_list",
      displayName: "Civil 3D Pressure Network Assign Parts List",
      description: "Assigns a pressure parts list to an existing pressure network.",
      inputShape: { networkName: z.string(), partsList: z.string() },
      supportedActions: ["assign_pressure_parts_list"],
      resolveAction: (rawArgs) => ({ action: "assign_pressure_parts_list", args: { action: "assign_pressure_parts_list", networkName: rawArgs.networkName, partsList: rawArgs.partsList } }),
    },
    {
      toolName: "civil3d_pressure_network_set_cover",
      displayName: "Civil 3D Pressure Network Set Cover",
      description: "Sets minimum and optional maximum cover requirements for a pressure network.",
      inputShape: { networkName: z.string(), minCoverDepth: z.number(), maxCoverDepth: z.number().optional() },
      supportedActions: ["set_pressure_cover"],
      resolveAction: (rawArgs) => ({ action: "set_pressure_cover", args: { action: "set_pressure_cover", networkName: rawArgs.networkName, minCoverDepth: rawArgs.minCoverDepth, maxCoverDepth: rawArgs.maxCoverDepth } }),
    },
    {
      toolName: "civil3d_pressure_network_validate",
      displayName: "Civil 3D Pressure Network Validate",
      description: "Validates a pressure network for cover violations, disconnected components, and parts mismatches.",
      inputShape: { networkName: z.string() },
      supportedActions: ["validate_pressure_network"],
      resolveAction: (rawArgs) => ({ action: "validate_pressure_network", args: { action: "validate_pressure_network", networkName: rawArgs.networkName } }),
    },
    {
      toolName: "civil3d_pressure_network_export",
      displayName: "Civil 3D Pressure Network Export",
      description: "Exports a pressure network as structured data including pipes, fittings, and appurtenances.",
      inputShape: { networkName: z.string(), includeCoordinates: z.boolean().optional().default(true) },
      supportedActions: ["export_pressure_network"],
      resolveAction: (rawArgs) => ({ action: "export_pressure_network", args: { action: "export_pressure_network", networkName: rawArgs.networkName, includeCoordinates: rawArgs.includeCoordinates } }),
    },
    {
      toolName: "civil3d_pressure_network_connect",
      displayName: "Civil 3D Pressure Network Connect",
      description: "Connects two pressure networks by merging the source network into the target network.",
      inputShape: { targetNetwork: z.string(), sourceNetwork: z.string() },
      supportedActions: ["connect_pressure_networks"],
      resolveAction: (rawArgs) => ({ action: "connect_pressure_networks", args: { action: "connect_pressure_networks", targetNetwork: rawArgs.targetNetwork, sourceNetwork: rawArgs.sourceNetwork } }),
    },
    {
      toolName: "civil3d_pressure_pipe_add",
      displayName: "Civil 3D Pressure Pipe Add",
      description: "Adds a pressure pipe segment to an existing pressure network.",
      inputShape: { networkName: z.string(), partName: z.string(), startPoint: OptionalPoint3DSchema, endPoint: OptionalPoint3DSchema, diameter: z.number().optional() },
      supportedActions: ["add_pressure_pipe"],
      resolveAction: (rawArgs) => ({ action: "add_pressure_pipe", args: { action: "add_pressure_pipe", networkName: rawArgs.networkName, partName: rawArgs.partName, startPoint: rawArgs.startPoint, endPoint: rawArgs.endPoint, diameter: rawArgs.diameter } }),
    },
    {
      toolName: "civil3d_pressure_pipe_get_properties",
      displayName: "Civil 3D Pressure Pipe Get Properties",
      description: "Gets detailed properties of a specific pressure pipe including diameter, length, material, and cover depth.",
      inputShape: { networkName: z.string(), pipeName: z.string() },
      supportedActions: ["get_pressure_pipe_properties"],
      resolveAction: (rawArgs) => ({ action: "get_pressure_pipe_properties", args: { action: "get_pressure_pipe_properties", networkName: rawArgs.networkName, pipeName: rawArgs.pipeName } }),
    },
    {
      toolName: "civil3d_pressure_pipe_resize",
      displayName: "Civil 3D Pressure Pipe Resize",
      description: "Changes the part and optional diameter of an existing pressure pipe.",
      inputShape: { networkName: z.string(), pipeName: z.string(), newPartName: z.string(), newDiameter: z.number().optional() },
      supportedActions: ["resize_pressure_pipe"],
      resolveAction: (rawArgs) => ({ action: "resize_pressure_pipe", args: { action: "resize_pressure_pipe", networkName: rawArgs.networkName, pipeName: rawArgs.pipeName, newPartName: rawArgs.newPartName, newDiameter: rawArgs.newDiameter } }),
    },
    {
      toolName: "civil3d_pressure_fitting_add",
      displayName: "Civil 3D Pressure Fitting Add",
      description: "Adds a pressure fitting such as an elbow, tee, reducer, or cap to a pressure network.",
      inputShape: { networkName: z.string(), partName: z.string(), position: OptionalPoint3DSchema, rotation: z.number().optional() },
      supportedActions: ["add_pressure_fitting"],
      resolveAction: (rawArgs) => ({ action: "add_pressure_fitting", args: { action: "add_pressure_fitting", networkName: rawArgs.networkName, partName: rawArgs.partName, position: rawArgs.position, rotation: rawArgs.rotation } }),
    },
    {
      toolName: "civil3d_pressure_fitting_get_properties",
      displayName: "Civil 3D Pressure Fitting Get Properties",
      description: "Gets detailed properties of a pressure fitting including type, location, and part size.",
      inputShape: { networkName: z.string(), fittingName: z.string() },
      supportedActions: ["get_pressure_fitting_properties"],
      resolveAction: (rawArgs) => ({ action: "get_pressure_fitting_properties", args: { action: "get_pressure_fitting_properties", networkName: rawArgs.networkName, fittingName: rawArgs.fittingName } }),
    },
    {
      toolName: "civil3d_pressure_appurtenance_add",
      displayName: "Civil 3D Pressure Appurtenance Add",
      description: "Adds a pressure appurtenance such as a valve, hydrant, or meter to a pressure network.",
      inputShape: { networkName: z.string(), partName: z.string(), position: OptionalPoint3DSchema, rotation: z.number().optional(), onPipeName: z.string().optional() },
      supportedActions: ["add_pressure_appurtenance"],
      resolveAction: (rawArgs) => ({ action: "add_pressure_appurtenance", args: { action: "add_pressure_appurtenance", networkName: rawArgs.networkName, partName: rawArgs.partName, position: rawArgs.position, rotation: rawArgs.rotation, onPipeName: rawArgs.onPipeName } }),
    },
  ],
};
