import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const DataShortcutObjectTypeSchema = z.enum([
  "surface",
  "alignment",
  "profile",
  "pipe_network",
  "pressure_network",
  "corridor",
  "section_view_group",
]);

const IncomingDataShortcutSchema = z.object({
  objectName: z.string(),
  objectType: z.enum(["surface", "alignment", "profile", "pipe_network"]),
  sourceFilePath: z.string(),
  isSynced: z.boolean(),
  isValid: z.boolean(),
});

const ExportableDataShortcutSchema = z.object({
  objectName: z.string(),
  objectType: z.string(),
  isExported: z.boolean(),
});

const DataShortcutListResponseSchema = z.object({
  incoming: z.array(IncomingDataShortcutSchema),
  exportable: z.array(ExportableDataShortcutSchema),
});

const GenericProjectResponseSchema = z.object({}).passthrough();

const canonicalProjectInputShape = {
  action: z.enum([
    "data_shortcut_list",
    "data_shortcut_create",
    "data_shortcut_promote",
    "data_shortcut_reference",
    "data_shortcut_sync",
    "data_shortcut_create_reference",
  ]),
  sourceFilePath: z.string().optional(),
  objectName: z.string().optional(),
  objectType: DataShortcutObjectTypeSchema.optional(),
  description: z.string().optional(),
  projectFolder: z.string().optional(),
  shortcutName: z.string().optional(),
  shortcutType: DataShortcutObjectTypeSchema.optional(),
  newName: z.string().optional(),
  layer: z.string().optional(),
  shortcutNames: z.array(z.string()).optional(),
  dryRun: z.boolean().optional(),
};

const DataShortcutListArgsSchema = z.object({
  action: z.literal("data_shortcut_list"),
});

const DataShortcutCreateArgsSchema = z.object({
  action: z.literal("data_shortcut_create"),
  objectType: DataShortcutObjectTypeSchema,
  objectName: z.string(),
  description: z.string().optional(),
  projectFolder: z.string().optional(),
});

const DataShortcutPromoteArgsSchema = z.object({
  action: z.literal("data_shortcut_promote"),
  shortcutName: z.string(),
  shortcutType: z.enum(["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor"]),
  newName: z.string().optional(),
});

const DataShortcutReferenceArgsSchema = z.object({
  action: z.literal("data_shortcut_reference"),
  projectFolder: z.string(),
  shortcutName: z.string(),
  shortcutType: DataShortcutObjectTypeSchema,
  layer: z.string().optional(),
});

const DataShortcutSyncArgsSchema = z.object({
  action: z.literal("data_shortcut_sync"),
  projectFolder: z.string().optional(),
  shortcutNames: z.array(z.string()).optional(),
  dryRun: z.boolean().optional(),
});

const DataShortcutCreateReferenceArgsSchema = z.object({
  action: z.literal("data_shortcut_create_reference"),
  sourceFilePath: z.string(),
  objectName: z.string(),
  objectType: z.enum(["surface", "alignment", "profile", "pipe_network"]),
});

export const PROJECT_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "project",
  actions: {
    data_shortcut_list: {
      action: "data_shortcut_list",
      inputSchema: DataShortcutListArgsSchema,
      responseSchema: DataShortcutListResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listDataShortcuts"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listDataShortcuts", {}),
      ),
    },
    data_shortcut_create: {
      action: "data_shortcut_create",
      inputSchema: DataShortcutCreateArgsSchema,
      responseSchema: GenericProjectResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createDataShortcut"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createDataShortcut", {
          objectType: args.objectType,
          objectName: args.objectName,
          description: args.description ?? null,
          projectFolder: args.projectFolder ?? null,
        }),
      ),
    },
    data_shortcut_promote: {
      action: "data_shortcut_promote",
      inputSchema: DataShortcutPromoteArgsSchema,
      responseSchema: GenericProjectResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["promoteDataShortcut"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("promoteDataShortcut", {
          shortcutName: args.shortcutName,
          shortcutType: args.shortcutType,
          newName: args.newName ?? null,
        }),
      ),
    },
    data_shortcut_reference: {
      action: "data_shortcut_reference",
      inputSchema: DataShortcutReferenceArgsSchema,
      responseSchema: GenericProjectResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["referenceDataShortcut"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("referenceDataShortcut", {
          projectFolder: args.projectFolder,
          shortcutName: args.shortcutName,
          shortcutType: args.shortcutType,
          layer: args.layer ?? null,
        }),
      ),
    },
    data_shortcut_sync: {
      action: "data_shortcut_sync",
      inputSchema: DataShortcutSyncArgsSchema,
      responseSchema: GenericProjectResponseSchema,
      capabilities: ["manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["syncDataShortcuts"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("syncDataShortcuts", {
          projectFolder: args.projectFolder ?? null,
          shortcutNames: args.shortcutNames ?? null,
          dryRun: args.dryRun ?? false,
        }),
      ),
    },
    data_shortcut_create_reference: {
      action: "data_shortcut_create_reference",
      inputSchema: DataShortcutCreateReferenceArgsSchema,
      responseSchema: GenericProjectResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createDataShortcutReference"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createDataShortcutReference", {
          sourceFilePath: args.sourceFilePath,
          objectName: args.objectName,
          objectType: args.objectType,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_project",
      displayName: "Civil 3D Project",
      description: "Manages Civil 3D project collaboration workflows including data-shortcut listing, publishing, referencing, promotion, and synchronization through a single domain tool.",
      inputShape: canonicalProjectInputShape,
      supportedActions: [
        "data_shortcut_list",
        "data_shortcut_create",
        "data_shortcut_promote",
        "data_shortcut_reference",
        "data_shortcut_sync",
        "data_shortcut_create_reference",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_data_shortcut",
      displayName: "Civil 3D Data Shortcut",
      description: "Lists, synchronizes, and creates data-shortcut references.",
      inputShape: {
        action: z.enum(["list", "sync", "create_reference"]),
        sourceFilePath: z.string().optional(),
        objectName: z.string().optional(),
        objectType: z.enum(["surface", "alignment", "profile", "pipe_network"]).optional(),
      },
      supportedActions: ["data_shortcut_list", "data_shortcut_sync", "data_shortcut_create_reference"],
      operations: ["list", "sync", "create_reference"],
      resolveAction: (rawArgs) => ({
        action:
          rawArgs.action === "list"
            ? "data_shortcut_list"
            : rawArgs.action === "sync"
              ? "data_shortcut_sync"
              : "data_shortcut_create_reference",
        args: rawArgs.action === "list"
          ? { action: "data_shortcut_list" }
          : rawArgs.action === "sync"
            ? { action: "data_shortcut_sync" }
            : {
              action: "data_shortcut_create_reference",
              sourceFilePath: rawArgs.sourceFilePath,
              objectName: rawArgs.objectName,
              objectType: rawArgs.objectType,
            },
      }),
    },
    {
      toolName: "civil3d_data_shortcut_create",
      displayName: "Civil 3D Data Shortcut Create",
      description: "Publishes a Civil 3D object as a project data shortcut.",
      inputShape: {
        objectType: DataShortcutObjectTypeSchema,
        objectName: z.string(),
        description: z.string().optional(),
        projectFolder: z.string().optional(),
      },
      supportedActions: ["data_shortcut_create"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_create",
        args: {
          action: "data_shortcut_create",
          objectType: rawArgs.objectType,
          objectName: rawArgs.objectName,
          description: rawArgs.description,
          projectFolder: rawArgs.projectFolder,
        },
      }),
    },
    {
      toolName: "civil3d_data_shortcut_promote",
      displayName: "Civil 3D Data Shortcut Promote",
      description: "Promotes a read-only data shortcut reference to a local editable object.",
      inputShape: {
        shortcutName: z.string(),
        shortcutType: z.enum(["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor"]),
        newName: z.string().optional(),
      },
      supportedActions: ["data_shortcut_promote"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_promote",
        args: {
          action: "data_shortcut_promote",
          shortcutName: rawArgs.shortcutName,
          shortcutType: rawArgs.shortcutType,
          newName: rawArgs.newName,
        },
      }),
    },
    {
      toolName: "civil3d_data_shortcut_reference",
      displayName: "Civil 3D Data Shortcut Reference",
      description: "References an existing project data shortcut into the current drawing.",
      inputShape: {
        projectFolder: z.string(),
        shortcutName: z.string(),
        shortcutType: DataShortcutObjectTypeSchema,
        layer: z.string().optional(),
      },
      supportedActions: ["data_shortcut_reference"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_reference",
        args: {
          action: "data_shortcut_reference",
          projectFolder: rawArgs.projectFolder,
          shortcutName: rawArgs.shortcutName,
          shortcutType: rawArgs.shortcutType,
          layer: rawArgs.layer,
        },
      }),
    },
    {
      toolName: "civil3d_data_shortcut_sync",
      displayName: "Civil 3D Data Shortcut Sync",
      description: "Synchronizes outdated Civil 3D data shortcut references.",
      inputShape: {
        projectFolder: z.string().optional(),
        shortcutNames: z.array(z.string()).optional(),
        dryRun: z.boolean().optional(),
      },
      supportedActions: ["data_shortcut_sync"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_sync",
        args: {
          action: "data_shortcut_sync",
          projectFolder: rawArgs.projectFolder,
          shortcutNames: rawArgs.shortcutNames,
          dryRun: rawArgs.dryRun,
        },
      }),
    },
  ],
};
