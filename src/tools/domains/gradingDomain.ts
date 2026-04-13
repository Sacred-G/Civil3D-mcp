import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().optional().default(0),
});

const FeatureLineSummarySchema = z.object({
  name: z.string().optional(),
  handle: z.string().optional(),
  layer: z.string().optional(),
  style: z.string().optional(),
}).passthrough();

const FeatureLineListResponseSchema = z.object({
  featureLines: z.array(FeatureLineSummarySchema),
});

const FeatureLineDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  layer: z.string(),
  style: z.string(),
  length: z.number(),
  vertexCount: z.number(),
  vertices: z.array(Point3DSchema),
  minElevation: z.number(),
  maxElevation: z.number(),
  units: z.string(),
});

const GenericGradingResponseSchema = z.object({}).passthrough();

const canonicalGradingInputShape = {
  action: z.enum([
    "group_list",
    "group_get",
    "group_create",
    "group_delete",
    "group_volume",
    "group_surface_create",
    "list",
    "get",
    "create",
    "delete",
    "criteria_list",
    "feature_line_list",
    "feature_line_get",
    "feature_line_export_as_polyline",
    "feature_line_create",
  ]),
  name: z.string().optional(),
  description: z.string().optional(),
  useProjection: z.boolean().optional(),
  surfaceName: z.string().optional(),
  groupName: z.string().optional(),
  handle: z.string().optional(),
  featureLineName: z.string().optional(),
  criteriaName: z.string().optional(),
  side: z.enum(["left", "right", "both"]).optional(),
  targetLayer: z.string().optional(),
  points: z.array(Point3DSchema).optional(),
  layer: z.string().optional(),
};

const GradingGroupListArgsSchema = z.object({
  action: z.literal("group_list"),
});

const GradingGroupGetArgsSchema = z.object({
  action: z.literal("group_get"),
  name: z.string(),
});

const GradingGroupCreateArgsSchema = z.object({
  action: z.literal("group_create"),
  name: z.string(),
  description: z.string().optional(),
  useProjection: z.boolean().optional(),
});

const GradingGroupDeleteArgsSchema = z.object({
  action: z.literal("group_delete"),
  name: z.string(),
});

const GradingGroupVolumeArgsSchema = z.object({
  action: z.literal("group_volume"),
  name: z.string(),
});

const GradingGroupSurfaceCreateArgsSchema = z.object({
  action: z.literal("group_surface_create"),
  name: z.string(),
  surfaceName: z.string().optional(),
});

const GradingListArgsSchema = z.object({
  action: z.literal("list"),
  groupName: z.string(),
});

const GradingGetArgsSchema = z.object({
  action: z.literal("get"),
  groupName: z.string(),
  handle: z.string(),
});

const GradingCreateArgsSchema = z.object({
  action: z.literal("create"),
  groupName: z.string(),
  featureLineName: z.string(),
  criteriaName: z.string().optional(),
  side: z.enum(["left", "right", "both"]).optional(),
});

const GradingDeleteArgsSchema = z.object({
  action: z.literal("delete"),
  groupName: z.string(),
  handle: z.string(),
});

const GradingCriteriaListArgsSchema = z.object({
  action: z.literal("criteria_list"),
});

const FeatureLineListArgsSchema = z.object({
  action: z.literal("feature_line_list"),
});

const FeatureLineGetArgsSchema = z.object({
  action: z.literal("feature_line_get"),
  name: z.string(),
});

const FeatureLineExportArgsSchema = z.object({
  action: z.literal("feature_line_export_as_polyline"),
  name: z.string(),
  targetLayer: z.string().optional(),
});

const FeatureLineCreateArgsSchema = z.object({
  action: z.literal("feature_line_create"),
  points: z.array(Point3DSchema).min(2),
  name: z.string().optional(),
  layer: z.string().optional(),
});

export const GRADING_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "grading",
  actions: {
    group_list: {
      action: "group_list",
      inputSchema: GradingGroupListArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listGradingGroups"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listGradingGroups", {}),
      ),
    },
    group_get: {
      action: "group_get",
      inputSchema: GradingGroupGetArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getGradingGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getGradingGroup", { name: args.name }),
      ),
    },
    group_create: {
      action: "group_create",
      inputSchema: GradingGroupCreateArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createGradingGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createGradingGroup", {
          name: args.name,
          description: args.description ?? "",
          useProjection: args.useProjection ?? false,
        }),
      ),
    },
    group_delete: {
      action: "group_delete",
      inputSchema: GradingGroupDeleteArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["delete", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deleteGradingGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("deleteGradingGroup", { name: args.name }),
      ),
    },
    group_volume: {
      action: "group_volume",
      inputSchema: GradingGroupVolumeArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getGradingGroupVolume"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getGradingGroupVolume", { name: args.name }),
      ),
    },
    group_surface_create: {
      action: "group_surface_create",
      inputSchema: GradingGroupSurfaceCreateArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["create", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createSurfaceFromGradingGroup"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createSurfaceFromGradingGroup", {
          name: args.name,
          surfaceName: args.surfaceName ?? null,
        }),
      ),
    },
    list: {
      action: "list",
      inputSchema: GradingListArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listGradings"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listGradings", { groupName: args.groupName }),
      ),
    },
    get: {
      action: "get",
      inputSchema: GradingGetArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getGrading"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getGrading", {
          groupName: args.groupName,
          handle: args.handle,
        }),
      ),
    },
    create: {
      action: "create",
      inputSchema: GradingCreateArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createGrading"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createGrading", {
          groupName: args.groupName,
          featureLineName: args.featureLineName,
          criteriaName: args.criteriaName ?? null,
          side: args.side ?? "right",
        }),
      ),
    },
    delete: {
      action: "delete",
      inputSchema: GradingDeleteArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["delete", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deleteGrading"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("deleteGrading", {
          groupName: args.groupName,
          handle: args.handle,
        }),
      ),
    },
    criteria_list: {
      action: "criteria_list",
      inputSchema: GradingCriteriaListArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listGradingCriteria"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listGradingCriteria", {}),
      ),
    },
    feature_line_list: {
      action: "feature_line_list",
      inputSchema: FeatureLineListArgsSchema,
      responseSchema: FeatureLineListResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listFeatureLines"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listFeatureLines", {}),
      ),
    },
    feature_line_get: {
      action: "feature_line_get",
      inputSchema: FeatureLineGetArgsSchema,
      responseSchema: FeatureLineDetailResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getFeatureLine"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getFeatureLine", { name: args.name }),
      ),
    },
    feature_line_export_as_polyline: {
      action: "feature_line_export_as_polyline",
      inputSchema: FeatureLineExportArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["query", "export"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["exportFeatureLineAsPolyline"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("exportFeatureLineAsPolyline", {
          name: args.name,
          targetLayer: args.targetLayer,
        }),
      ),
    },
    feature_line_create: {
      action: "feature_line_create",
      inputSchema: FeatureLineCreateArgsSchema,
      responseSchema: GenericGradingResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createFeatureLine"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createFeatureLine", {
          points: args.points,
          name: args.name ?? null,
          layer: args.layer ?? "0",
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_grading",
      displayName: "Civil 3D Grading",
      description: "Lists, inspects, creates, deletes, and analyzes Civil 3D grading groups, gradings, and feature lines through a single domain tool.",
      inputShape: canonicalGradingInputShape,
      supportedActions: [
        "group_list",
        "group_get",
        "group_create",
        "group_delete",
        "group_volume",
        "group_surface_create",
        "list",
        "get",
        "create",
        "delete",
        "criteria_list",
        "feature_line_list",
        "feature_line_get",
        "feature_line_export_as_polyline",
        "feature_line_create",
      ],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_feature_line",
      displayName: "Civil 3D Feature Line",
      description: "Reads Civil 3D feature lines and supports exporting them as 3D polylines.",
      inputShape: {
        action: z.enum(["list", "get", "export_as_polyline"]),
        name: z.string().optional(),
        targetLayer: z.string().optional(),
      },
      supportedActions: ["feature_line_list", "feature_line_get", "feature_line_export_as_polyline"],
      operations: ["list", "get", "export_as_polyline"],
      resolveAction: (rawArgs) => ({
        action:
          rawArgs.action === "list"
            ? "feature_line_list"
            : rawArgs.action === "get"
              ? "feature_line_get"
              : "feature_line_export_as_polyline",
        args: rawArgs.action === "list"
          ? { action: "feature_line_list" }
          : rawArgs.action === "get"
            ? { action: "feature_line_get", name: rawArgs.name }
            : {
              action: "feature_line_export_as_polyline",
              name: rawArgs.name,
              targetLayer: rawArgs.targetLayer,
            },
      }),
    },
    {
      toolName: "civil3d_grading_group_list",
      displayName: "Civil 3D Grading Group List",
      description: "Lists all Civil 3D grading groups in the drawing.",
      inputShape: {},
      supportedActions: ["group_list"],
      resolveAction: () => ({ action: "group_list", args: { action: "group_list" } }),
    },
    {
      toolName: "civil3d_grading_group_get",
      displayName: "Civil 3D Grading Group Get",
      description: "Gets detailed information about a Civil 3D grading group.",
      inputShape: { name: z.string() },
      supportedActions: ["group_get"],
      resolveAction: (rawArgs) => ({ action: "group_get", args: { action: "group_get", name: rawArgs.name } }),
    },
    {
      toolName: "civil3d_grading_group_create",
      displayName: "Civil 3D Grading Group Create",
      description: "Creates a new Civil 3D grading group.",
      inputShape: {
        name: z.string(),
        description: z.string().optional(),
        useProjection: z.boolean().optional(),
      },
      supportedActions: ["group_create"],
      resolveAction: (rawArgs) => ({
        action: "group_create",
        args: {
          action: "group_create",
          name: rawArgs.name,
          description: rawArgs.description,
          useProjection: rawArgs.useProjection,
        },
      }),
    },
    {
      toolName: "civil3d_grading_group_delete",
      displayName: "Civil 3D Grading Group Delete",
      description: "Deletes a Civil 3D grading group and its gradings.",
      inputShape: { name: z.string() },
      supportedActions: ["group_delete"],
      resolveAction: (rawArgs) => ({ action: "group_delete", args: { action: "group_delete", name: rawArgs.name } }),
    },
    {
      toolName: "civil3d_grading_group_volume",
      displayName: "Civil 3D Grading Group Volume",
      description: "Gets the cut/fill volume report for a Civil 3D grading group.",
      inputShape: { name: z.string() },
      supportedActions: ["group_volume"],
      resolveAction: (rawArgs) => ({ action: "group_volume", args: { action: "group_volume", name: rawArgs.name } }),
    },
    {
      toolName: "civil3d_grading_group_surface_create",
      displayName: "Civil 3D Grading Group Surface Create",
      description: "Creates a Civil 3D surface from a grading group.",
      inputShape: { name: z.string(), surfaceName: z.string().optional() },
      supportedActions: ["group_surface_create"],
      resolveAction: (rawArgs) => ({
        action: "group_surface_create",
        args: {
          action: "group_surface_create",
          name: rawArgs.name,
          surfaceName: rawArgs.surfaceName,
        },
      }),
    },
    {
      toolName: "civil3d_grading_list",
      displayName: "Civil 3D Grading List",
      description: "Lists all grading objects within a specific Civil 3D grading group.",
      inputShape: { groupName: z.string() },
      supportedActions: ["list"],
      resolveAction: (rawArgs) => ({ action: "list", args: { action: "list", groupName: rawArgs.groupName } }),
    },
    {
      toolName: "civil3d_grading_get",
      displayName: "Civil 3D Grading Get",
      description: "Gets detailed properties of a specific grading object.",
      inputShape: { groupName: z.string(), handle: z.string() },
      supportedActions: ["get"],
      resolveAction: (rawArgs) => ({
        action: "get",
        args: {
          action: "get",
          groupName: rawArgs.groupName,
          handle: rawArgs.handle,
        },
      }),
    },
    {
      toolName: "civil3d_grading_create",
      displayName: "Civil 3D Grading Create",
      description: "Creates a new Civil 3D grading from a feature line using the specified criteria.",
      inputShape: {
        groupName: z.string(),
        featureLineName: z.string(),
        criteriaName: z.string().optional(),
        side: z.enum(["left", "right", "both"]).optional(),
      },
      supportedActions: ["create"],
      resolveAction: (rawArgs) => ({
        action: "create",
        args: {
          action: "create",
          groupName: rawArgs.groupName,
          featureLineName: rawArgs.featureLineName,
          criteriaName: rawArgs.criteriaName,
          side: rawArgs.side,
        },
      }),
    },
    {
      toolName: "civil3d_grading_delete",
      displayName: "Civil 3D Grading Delete",
      description: "Deletes a grading object from a Civil 3D grading group by handle.",
      inputShape: { groupName: z.string(), handle: z.string() },
      supportedActions: ["delete"],
      resolveAction: (rawArgs) => ({
        action: "delete",
        args: {
          action: "delete",
          groupName: rawArgs.groupName,
          handle: rawArgs.handle,
        },
      }),
    },
    {
      toolName: "civil3d_grading_criteria_list",
      displayName: "Civil 3D Grading Criteria List",
      description: "Lists the available Civil 3D grading criteria sets and criteria names.",
      inputShape: {},
      supportedActions: ["criteria_list"],
      resolveAction: () => ({ action: "criteria_list", args: { action: "criteria_list" } }),
    },
    {
      toolName: "civil3d_feature_line_create",
      displayName: "Civil 3D Feature Line Create",
      description: "Creates a new Civil 3D feature line from an ordered list of 3D points.",
      inputShape: {
        points: z.array(Point3DSchema).min(2),
        name: z.string().optional(),
        layer: z.string().optional(),
      },
      supportedActions: ["feature_line_create"],
      resolveAction: (rawArgs) => ({
        action: "feature_line_create",
        args: {
          action: "feature_line_create",
          points: rawArgs.points,
          name: rawArgs.name,
          layer: rawArgs.layer,
        },
      }),
    },
  ],
};
