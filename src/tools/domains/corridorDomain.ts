import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

// ─── Shared schemas ───────────────────────────────────────────────────────────

const GenericCorridorResponseSchema = z.object({}).passthrough();

const CorridorStateSchema = z.enum(["built", "out_of_date", "error"]);

const CorridorSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  baselineCount: z.number(),
  regionCount: z.number(),
  surfaceCount: z.number(),
  state: CorridorStateSchema,
  lastBuildTime: z.string().nullable(),
});

const CorridorListResponseSchema = z.object({
  corridors: z.array(CorridorSummarySchema),
});

const CorridorRegionSchema = z.object({
  name: z.string(),
  assemblyName: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  frequency: z.number(),
});

const CorridorBaselineSchema = z.object({
  name: z.string(),
  alignmentName: z.string(),
  profileName: z.string(),
  regions: z.array(CorridorRegionSchema),
});

const CorridorSurfaceSchema = z.object({
  name: z.string(),
  boundaries: z.array(z.string()),
});

const CorridorDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  style: z.string(),
  layer: z.string(),
  baselines: z.array(CorridorBaselineSchema),
  surfaces: z.array(CorridorSurfaceSchema),
  featureLineCount: z.number(),
  state: CorridorStateSchema,
});

const CorridorSurfacesResponseSchema = z.object({
  corridorName: z.string().optional(),
  surfaces: z.array(CorridorSurfaceSchema),
});

const CorridorFeatureLineSchema = z.record(z.unknown());

const CorridorFeatureLinesResponseSchema = z.object({
  corridorName: z.string().optional(),
  featureLines: z.array(CorridorFeatureLineSchema),
});

const CorridorRebuildResponseSchema = z.object({
  jobId: z.string(),
}).passthrough();

const CorridorVolumesResponseSchema = z.object({
  cutVolume: z.number(),
  fillVolume: z.number(),
  netVolume: z.number(),
  cutArea: z.number(),
  fillArea: z.number(),
  units: z.object({
    volume: z.string(),
    area: z.string(),
  }),
});

const CorridorFullSummaryResponseSchema = z.object({
  corridor: CorridorDetailResponseSchema,
  surfaceInventory: CorridorSurfacesResponseSchema,
  volumeAnalysis: CorridorVolumesResponseSchema.nullable(),
  summary: z.object({
    baselineCount: z.number(),
    regionCount: z.number(),
    corridorSurfaceCount: z.number(),
    featureLineCount: z.number(),
    state: CorridorStateSchema,
    totalRegionLength: z.number(),
    selectedCorridorSurface: z.string().nullable(),
    referenceSurface: z.string().nullable(),
    volumeComputationStatus: z.enum(["computed", "skipped"]),
  }),
});

// ─── Per-action input schemas ─────────────────────────────────────────────────

const CorridorListArgsSchema = z.object({
  action: z.literal("list"),
});

const CorridorGetArgsSchema = z.object({
  action: z.literal("get"),
  name: z.string(),
});

const CorridorRebuildArgsSchema = z.object({
  action: z.literal("rebuild"),
  name: z.string(),
});

const CorridorGetSurfacesArgsSchema = z.object({
  action: z.literal("get_surfaces"),
  name: z.string(),
});

const CorridorGetFeatureLinesArgsSchema = z.object({
  action: z.literal("get_feature_lines"),
  name: z.string(),
});

const CorridorComputeVolumesArgsSchema = z.object({
  action: z.literal("compute_volumes"),
  name: z.string(),
  corridorSurface: z.string(),
  referenceSurface: z.string(),
});

const CorridorSummaryArgsSchema = z.object({
  action: z.literal("summary"),
  name: z.string(),
  corridorSurface: z.string().optional(),
  referenceSurface: z.string().optional(),
});

const CorridorTargetMappingGetArgsSchema = z.object({
  action: z.literal("target_mapping_get"),
  name: z.string(),
  regionIndex: z.number().int().nonnegative().optional(),
  baselineIndex: z.number().int().nonnegative().optional(),
});

const CorridorTargetMappingSetArgsSchema = z.object({
  action: z.literal("target_mapping_set"),
  name: z.string(),
  regionIndex: z.number().int().nonnegative().optional(),
  baselineIndex: z.number().int().nonnegative().optional(),
  targets: z.array(z.object({
    parameterName: z.string(),
    targetType: z.enum(["surface", "alignment", "profile", "polyline"]),
    targetName: z.string(),
  })),
});

const CorridorRegionAddArgsSchema = z.object({
  action: z.literal("region_add"),
  name: z.string(),
  baselineIndex: z.number().int().nonnegative().optional(),
  assemblyName: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  frequency: z.number().positive().optional(),
  frequencyAtCurves: z.number().positive().optional(),
  frequencyAtKneePoints: z.number().positive().optional(),
});

const CorridorRegionDeleteArgsSchema = z.object({
  action: z.literal("region_delete"),
  name: z.string(),
  baselineIndex: z.number().int().nonnegative().optional(),
  regionIndex: z.number().int().nonnegative(),
});

// ─── Canonical input shape ────────────────────────────────────────────────────

const canonicalCorridorInputShape = {
  action: z.enum([
    "list",
    "get",
    "rebuild",
    "get_surfaces",
    "get_feature_lines",
    "compute_volumes",
    "summary",
    "target_mapping_get",
    "target_mapping_set",
    "region_add",
    "region_delete",
  ]),
  name: z.string().optional(),
  corridorSurface: z.string().optional(),
  referenceSurface: z.string().optional(),
  regionIndex: z.number().int().nonnegative().optional(),
  baselineIndex: z.number().int().nonnegative().optional(),
  targets: z.array(z.object({
    parameterName: z.string(),
    targetType: z.enum(["surface", "alignment", "profile", "polyline"]),
    targetName: z.string(),
  })).optional(),
  assemblyName: z.string().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  frequency: z.number().positive().optional(),
  frequencyAtCurves: z.number().positive().optional(),
  frequencyAtKneePoints: z.number().positive().optional(),
};

// ─── Domain definition ────────────────────────────────────────────────────────

export const CORRIDOR_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "corridor",
  actions: {
    list: {
      action: "list",
      inputSchema: CorridorListArgsSchema,
      responseSchema: CorridorListResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listCorridors"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listCorridors", {}),
      ),
    },
    get: {
      action: "get",
      inputSchema: CorridorGetArgsSchema,
      responseSchema: CorridorDetailResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCorridor"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCorridor", {
          name: args.name,
        }),
      ),
    },
    rebuild: {
      action: "rebuild",
      inputSchema: CorridorRebuildArgsSchema,
      responseSchema: CorridorRebuildResponseSchema,
      capabilities: ["manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["rebuildCorridor"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("rebuildCorridor", {
          name: args.name,
        }),
      ),
    },
    get_surfaces: {
      action: "get_surfaces",
      inputSchema: CorridorGetSurfacesArgsSchema,
      responseSchema: CorridorSurfacesResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCorridorSurfaces"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCorridorSurfaces", {
          name: args.name,
        }),
      ),
    },
    get_feature_lines: {
      action: "get_feature_lines",
      inputSchema: CorridorGetFeatureLinesArgsSchema,
      responseSchema: CorridorFeatureLinesResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCorridorFeatureLines"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCorridorFeatureLines", {
          name: args.name,
        }),
      ),
    },
    compute_volumes: {
      action: "compute_volumes",
      inputSchema: CorridorComputeVolumesArgsSchema,
      responseSchema: CorridorVolumesResponseSchema,
      capabilities: ["analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["computeCorridorVolumes"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("computeCorridorVolumes", {
          name: args.name,
          corridorSurface: args.corridorSurface,
          referenceSurface: args.referenceSurface,
        }),
      ),
    },
    summary: {
      action: "summary",
      inputSchema: CorridorSummaryArgsSchema,
      responseSchema: CorridorFullSummaryResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCorridor", "getCorridorSurfaces", "computeCorridorVolumes"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const corridor = CorridorDetailResponseSchema.parse(
          await appClient.sendCommand("getCorridor", { name: args.name }),
        );

        const surfaceInventory = CorridorSurfacesResponseSchema.parse(
          await appClient.sendCommand("getCorridorSurfaces", { name: args.name }),
        );

        const selectedCorridorSurface = (args.corridorSurface as string | undefined)
          ?? (surfaceInventory.surfaces.length === 1 ? surfaceInventory.surfaces[0].name : null);
        const referenceSurface = (args.referenceSurface as string | undefined) ?? null;

        let volumeAnalysis: z.infer<typeof CorridorVolumesResponseSchema> | null = null;
        if (selectedCorridorSurface && referenceSurface) {
          volumeAnalysis = CorridorVolumesResponseSchema.parse(
            await appClient.sendCommand("computeCorridorVolumes", {
              name: args.name,
              corridorSurface: selectedCorridorSurface,
              referenceSurface,
            }),
          );
        }

        const regionCount = corridor.baselines.reduce(
          (total, baseline) => total + baseline.regions.length,
          0,
        );

        const totalRegionLength = corridor.baselines.reduce(
          (baselineTotal, baseline) =>
            baselineTotal + baseline.regions.reduce(
              (regionTotal, region) => regionTotal + (region.endStation - region.startStation),
              0,
            ),
          0,
        );

        return CorridorFullSummaryResponseSchema.parse({
          corridor,
          surfaceInventory,
          volumeAnalysis,
          summary: {
            baselineCount: corridor.baselines.length,
            regionCount,
            corridorSurfaceCount: surfaceInventory.surfaces.length,
            featureLineCount: corridor.featureLineCount,
            state: corridor.state,
            totalRegionLength,
            selectedCorridorSurface,
            referenceSurface,
            volumeComputationStatus: volumeAnalysis ? "computed" : "skipped",
          },
        });
      }),
    },
    target_mapping_get: {
      action: "target_mapping_get",
      inputSchema: CorridorTargetMappingGetArgsSchema,
      responseSchema: GenericCorridorResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getCorridorTargetMappings"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCorridorTargetMappings", {
          corridorName: args.name,
          regionIndex: args.regionIndex ?? null,
          baselineIndex: args.baselineIndex ?? 0,
        }),
      ),
    },
    target_mapping_set: {
      action: "target_mapping_set",
      inputSchema: CorridorTargetMappingSetArgsSchema,
      responseSchema: GenericCorridorResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["setCorridorTargetMappings"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("setCorridorTargetMappings", {
          corridorName: args.name,
          regionIndex: args.regionIndex ?? 0,
          baselineIndex: args.baselineIndex ?? 0,
          targets: args.targets,
        }),
      ),
    },
    region_add: {
      action: "region_add",
      inputSchema: CorridorRegionAddArgsSchema,
      responseSchema: GenericCorridorResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addCorridorRegion"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("addCorridorRegion", {
          corridorName: args.name,
          baselineIndex: args.baselineIndex ?? 0,
          assemblyName: args.assemblyName,
          startStation: args.startStation,
          endStation: args.endStation,
          frequency: args.frequency ?? 10,
          frequencyAtCurves: args.frequencyAtCurves ?? null,
          frequencyAtKneePoints: args.frequencyAtKneePoints ?? null,
        }),
      ),
    },
    region_delete: {
      action: "region_delete",
      inputSchema: CorridorRegionDeleteArgsSchema,
      responseSchema: GenericCorridorResponseSchema,
      capabilities: ["delete", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deleteCorridorRegion"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("deleteCorridorRegion", {
          corridorName: args.name,
          baselineIndex: args.baselineIndex ?? 0,
          regionIndex: args.regionIndex,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_corridor",
      displayName: "Civil 3D Corridor",
      description: "Reads Civil 3D corridor data, controls rebuild, computes volumes, and manages regions and target mappings through a single domain tool.",
      inputShape: canonicalCorridorInputShape,
      supportedActions: [
        "list",
        "get",
        "rebuild",
        "get_surfaces",
        "get_feature_lines",
        "compute_volumes",
        "summary",
        "target_mapping_get",
        "target_mapping_set",
        "region_add",
        "region_delete",
      ],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_corridor_summary",
      displayName: "Civil 3D Corridor Summary",
      description: "Builds a corridor summary by fetching corridor details, corridor surfaces, and optional volume analysis against a reference surface.",
      inputShape: {
        name: z.string(),
        corridorSurface: z.string().optional(),
        referenceSurface: z.string().optional(),
      },
      supportedActions: ["summary"],
      resolveAction: (rawArgs) => ({
        action: "summary",
        args: {
          action: "summary",
          name: rawArgs.name,
          corridorSurface: rawArgs.corridorSurface,
          referenceSurface: rawArgs.referenceSurface,
        },
      }),
    },
    {
      toolName: "civil3d_corridor_target_mapping_get",
      displayName: "Civil 3D Corridor Target Mapping Get",
      description: "Retrieve the current subassembly target mappings for a Civil 3D corridor. Returns all target parameters for each baseline region.",
      inputShape: {
        corridorName: z.string(),
        regionIndex: z.number().int().nonnegative().optional(),
        baselineIndex: z.number().int().nonnegative().optional(),
      },
      supportedActions: ["target_mapping_get"],
      resolveAction: (rawArgs) => ({
        action: "target_mapping_get",
        args: {
          action: "target_mapping_get",
          name: rawArgs.corridorName,
          regionIndex: rawArgs.regionIndex,
          baselineIndex: rawArgs.baselineIndex,
        },
      }),
    },
    {
      toolName: "civil3d_corridor_target_mapping_set",
      displayName: "Civil 3D Corridor Target Mapping Set",
      description: "Set or update subassembly target mappings on a Civil 3D corridor region. Assigns surfaces, alignments, profiles, or polylines as targets for subassembly parameters.",
      inputShape: {
        corridorName: z.string(),
        regionIndex: z.number().int().nonnegative().optional(),
        baselineIndex: z.number().int().nonnegative().optional(),
        targets: z.array(z.object({
          parameterName: z.string(),
          targetType: z.enum(["surface", "alignment", "profile", "polyline"]),
          targetName: z.string(),
        })),
      },
      supportedActions: ["target_mapping_set"],
      resolveAction: (rawArgs) => ({
        action: "target_mapping_set",
        args: {
          action: "target_mapping_set",
          name: rawArgs.corridorName,
          regionIndex: rawArgs.regionIndex,
          baselineIndex: rawArgs.baselineIndex,
          targets: rawArgs.targets,
        },
      }),
    },
    {
      toolName: "civil3d_corridor_region_add",
      displayName: "Civil 3D Corridor Region Add",
      description: "Add a new region to a Civil 3D corridor baseline, defining which assembly applies over a station range and at what sampling frequency.",
      inputShape: {
        corridorName: z.string(),
        baselineIndex: z.number().int().nonnegative().optional(),
        assemblyName: z.string(),
        startStation: z.number(),
        endStation: z.number(),
        frequency: z.number().positive().optional(),
        frequencyAtCurves: z.number().positive().optional(),
        frequencyAtKneePoints: z.number().positive().optional(),
      },
      supportedActions: ["region_add"],
      resolveAction: (rawArgs) => ({
        action: "region_add",
        args: {
          action: "region_add",
          name: rawArgs.corridorName,
          baselineIndex: rawArgs.baselineIndex,
          assemblyName: rawArgs.assemblyName,
          startStation: rawArgs.startStation,
          endStation: rawArgs.endStation,
          frequency: rawArgs.frequency,
          frequencyAtCurves: rawArgs.frequencyAtCurves,
          frequencyAtKneePoints: rawArgs.frequencyAtKneePoints,
        },
      }),
    },
    {
      toolName: "civil3d_corridor_region_delete",
      displayName: "Civil 3D Corridor Region Delete",
      description: "Delete a region from a Civil 3D corridor baseline by its zero-based index. Rebuilds the corridor after deletion.",
      inputShape: {
        corridorName: z.string(),
        baselineIndex: z.number().int().nonnegative().optional(),
        regionIndex: z.number().int().nonnegative(),
      },
      supportedActions: ["region_delete"],
      resolveAction: (rawArgs) => ({
        action: "region_delete",
        args: {
          action: "region_delete",
          name: rawArgs.corridorName,
          baselineIndex: rawArgs.baselineIndex,
          regionIndex: rawArgs.regionIndex,
        },
      }),
    },
  ],
};
