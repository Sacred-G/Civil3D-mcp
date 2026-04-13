import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const DrawingInfoResponseSchema = z.object({ fileName: z.string().optional(), filePath: z.string().optional(), coordinateSystem: z.string().nullable().optional(), linearUnits: z.enum(["feet", "meters", "other"]).optional(), angularUnits: z.enum(["degrees", "radians", "grads"]).optional(), unsavedChanges: z.boolean().optional(), objectCounts: z.object({ surfaces: z.number().optional(), alignments: z.number().optional(), profiles: z.number().optional(), corridors: z.number().optional(), pipeNetworks: z.number().optional(), points: z.number().optional(), parcels: z.number().optional() }).optional(), drawingName: z.string().optional(), projectName: z.string().optional(), units: z.string().optional() });
const DrawingSettingsResponseSchema = z.object({ coordinateSystem: z.string().nullable().optional(), coordinateZone: z.string().nullable().optional(), datum: z.string().nullable().optional(), scaleFactor: z.number().optional(), elevationReference: z.string().nullable().optional(), defaultLayer: z.string().optional(), defaultStyles: z.object({ surface: z.string().optional(), alignment: z.string().optional(), profile: z.string().optional(), corridor: z.string().optional(), pipeNetwork: z.string().optional() }).optional() });
const SelectedCivilObjectsResponseSchema = z.array(z.object({ handle: z.string(), objectType: z.string(), name: z.string().optional(), description: z.string().optional() }));
const CivilObjectTypesResponseSchema = z.array(z.string());
const GenericResponseSchema = z.object({}).passthrough();

const DrawingInfoArgs = z.object({ action: z.literal("info") });
const DrawingNewArgs = z.object({ action: z.literal("new"), templatePath: z.string().optional() });
const DrawingSaveArgs = z.object({ action: z.literal("save"), saveAs: z.string().optional() });
const DrawingUndoArgs = z.object({ action: z.literal("undo"), steps: z.number().int().min(1).max(10).optional() });
const DrawingRedoArgs = z.object({ action: z.literal("redo"), steps: z.number().int().min(1).max(10).optional() });
const DrawingSettingsArgs = z.object({ action: z.literal("settings") });
const SelectedObjectsArgs = z.object({ action: z.literal("selected_objects_info"), limit: z.number().optional() });
const ObjectTypesArgs = z.object({ action: z.literal("list_object_types") });

export const DRAWING_RUNTIME_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "drawing",
  actions: {
    info: { action: "info", inputSchema: DrawingInfoArgs, responseSchema: DrawingInfoResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["getDrawingInfo"], execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getDrawingInfo", {})) },
    new: { action: "new", inputSchema: DrawingNewArgs, responseSchema: GenericResponseSchema, capabilities: ["create", "manage"], requiresActiveDrawing: false, safeForRetry: false, pluginMethods: ["newDrawing"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("newDrawing", { templatePath: args.templatePath })) },
    save: { action: "save", inputSchema: DrawingSaveArgs, responseSchema: GenericResponseSchema, capabilities: ["edit", "manage"], requiresActiveDrawing: false, safeForRetry: false, pluginMethods: ["saveDrawing"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("saveDrawing", { saveAs: args.saveAs })) },
    undo: { action: "undo", inputSchema: DrawingUndoArgs, responseSchema: GenericResponseSchema, capabilities: ["edit", "manage"], requiresActiveDrawing: true, safeForRetry: false, pluginMethods: ["undoDrawing"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("undoDrawing", { steps: args.steps ?? 1 })) },
    redo: { action: "redo", inputSchema: DrawingRedoArgs, responseSchema: GenericResponseSchema, capabilities: ["edit", "manage"], requiresActiveDrawing: true, safeForRetry: false, pluginMethods: ["redoDrawing"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("redoDrawing", { steps: args.steps ?? 1 })) },
    settings: { action: "settings", inputSchema: DrawingSettingsArgs, responseSchema: DrawingSettingsResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["getDrawingSettings"], execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getDrawingSettings", {})) },
    selected_objects_info: { action: "selected_objects_info", inputSchema: SelectedObjectsArgs, responseSchema: SelectedCivilObjectsResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: true, safeForRetry: true, pluginMethods: ["getSelectedCivilObjectsInfo"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getSelectedCivilObjectsInfo", { limit: args.limit || 100 })) },
    list_object_types: { action: "list_object_types", inputSchema: ObjectTypesArgs, responseSchema: CivilObjectTypesResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["listCivilObjectTypes"], execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("listCivilObjectTypes", {})) },
  },
  exposures: [
    { toolName: "civil3d_drawing", displayName: "Civil 3D Drawing", description: "Reads drawing state, settings, selection context, and document operations through a single domain tool.", inputShape: { action: z.enum(["info", "new", "save", "undo", "redo", "settings", "selected_objects_info", "list_object_types"]), templatePath: z.string().optional(), saveAs: z.string().optional(), steps: z.number().int().min(1).max(10).optional(), limit: z.number().optional() }, supportedActions: ["info", "new", "save", "undo", "redo", "settings", "selected_objects_info", "list_object_types"], resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }) },
    { toolName: "get_drawing_info", displayName: "Get Drawing Info", description: "Retrieves basic information about the active Civil 3D drawing.", inputShape: {}, supportedActions: ["info"], resolveAction: () => ({ action: "info", args: { action: "info" } }) },
    { toolName: "get_selected_civil_objects_info", displayName: "Get Selected Civil Objects Info", description: "Gets basic properties of currently selected Civil 3D objects.", inputShape: { limit: z.number().optional() }, supportedActions: ["selected_objects_info"], resolveAction: (rawArgs) => ({ action: "selected_objects_info", args: { action: "selected_objects_info", ...rawArgs } }) },
    { toolName: "list_civil_object_types", displayName: "List Civil Object Types", description: "Lists major Civil 3D object types available in the current context.", inputShape: {}, supportedActions: ["list_object_types"], resolveAction: () => ({ action: "list_object_types", args: { action: "list_object_types" } }) },
  ],
};
