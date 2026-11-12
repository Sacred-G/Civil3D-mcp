import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

// Paths are validated again inside the plugin by FileBoundary (absolute,
// inside CIVIL3D_IMPORT_ROOTS, .dwg only, must exist); the Node schema only
// rejects obviously wrong input early.
const DwgPathSchema = z.string().min(1).regex(/\.dwg$/i, "Xref paths must point to a .dwg file.");
const XrefNameSchema = z.string().min(1).max(255);
const PathTypeSchema = z.enum(["absolute", "relative"]);

export const XrefStatusSchema = z.enum([
  "loaded",
  "unloaded",
  "unreferenced",
  "not_found",
  "unresolved",
  "orphaned",
  "unknown",
]);

export const XrefEntrySchema = z.object({
  name: z.string(),
  handle: z.string().nullish(),
  savedPath: z.string().nullish(),
  foundPath: z.string().nullish(),
  pathType: z.enum(["absolute", "relative", "none"]),
  status: XrefStatusSchema,
  rawStatus: z.string(),
  attachment: z.enum(["attach", "overlay"]).nullish(),
  isNested: z.boolean(),
  parents: z.array(z.string()),
  childCount: z.number().int(),
  instanceCount: z.number().int().nullish(),
});

export const XrefListResponseSchema = z.object({
  hostDrawing: z.string().nullish(),
  hostIsSaved: z.boolean(),
  count: z.number().int(),
  statusCounts: z.record(z.number().int()),
  xrefs: z.array(XrefEntrySchema),
});

const GenericXrefResponseSchema = z.object({}).passthrough();

const PointSchema = z.object({ x: z.number(), y: z.number(), z: z.number().optional() });
const NamesShape = {
  name: XrefNameSchema.optional(),
  names: z.array(XrefNameSchema).min(1).max(100).optional(),
};

function requireNames<T extends { name?: string; names?: string[] }>(value: T, ctx: z.RefinementCtx) {
  if (!value.name && !(value.names && value.names.length > 0)) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: "Provide 'name' or a non-empty 'names' array.", path: ["names"] });
  }
}

export const XrefListArgsSchema = z.object({
  action: z.literal("list"),
  includeNested: z.boolean().optional(),
});

const insertShape = {
  path: DwgPathSchema,
  name: XrefNameSchema.optional(),
  pathType: PathTypeSchema.optional(),
  insert: z.boolean().optional(),
  insertionPoint: PointSchema.optional(),
  scale: z.number().positive().optional(),
  rotation: z.number().optional(),
  layer: z.string().min(1).optional(),
};

export const XrefAttachArgsSchema = z.object({ action: z.literal("attach"), ...insertShape });
export const XrefOverlayArgsSchema = z.object({ action: z.literal("overlay"), ...insertShape });
export const XrefDetachArgsSchema = z.object({ action: z.literal("detach"), ...NamesShape }).superRefine(requireNames);
export const XrefReloadArgsSchema = z.object({ action: z.literal("reload"), ...NamesShape }).superRefine(requireNames);
export const XrefUnloadArgsSchema = z.object({ action: z.literal("unload"), ...NamesShape }).superRefine(requireNames);
export const XrefBindArgsSchema = z.object({
  action: z.literal("bind"),
  ...NamesShape,
  bindType: z.enum(["bind", "insert"]).optional(),
}).superRefine(requireNames);
export const XrefRepathArgsSchema = z.object({
  action: z.literal("repath"),
  name: XrefNameSchema,
  newPath: DwgPathSchema,
  pathType: PathTypeSchema.optional(),
  reload: z.boolean().optional(),
});

const XREF_ACTIONS = ["list", "attach", "overlay", "detach", "reload", "unload", "bind", "repath"] as const;

const canonicalXrefInputShape = {
  action: z.enum(XREF_ACTIONS),
  includeNested: z.boolean().optional(),
  path: z.string().optional(),
  newPath: z.string().optional(),
  name: z.string().optional(),
  names: z.array(z.string()).optional(),
  pathType: PathTypeSchema.optional(),
  insert: z.boolean().optional(),
  insertionPoint: PointSchema.optional(),
  scale: z.number().optional(),
  rotation: z.number().optional(),
  layer: z.string().optional(),
  bindType: z.enum(["bind", "insert"]).optional(),
  reload: z.boolean().optional(),
};

type InsertArgs = z.infer<typeof XrefAttachArgsSchema>;
type NamesArgs = { name?: string; names?: string[] };

function insertParams(args: InsertArgs) {
  return {
    path: args.path,
    name: args.name ?? null,
    pathType: args.pathType ?? "absolute",
    insert: args.insert ?? true,
    insertionPoint: args.insertionPoint ?? null,
    scale: args.scale ?? 1,
    rotation: args.rotation ?? 0,
    layer: args.layer ?? null,
  };
}

function nameParams(args: NamesArgs) {
  return { name: args.name ?? null, names: args.names ?? null };
}

export const XREF_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "xref",
  actions: {
    list: {
      action: "list",
      inputSchema: XrefListArgsSchema,
      responseSchema: XrefListResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listXrefs"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listXrefs", { includeNested: args.includeNested ?? true }),
      ),
    },
    attach: {
      action: "attach",
      inputSchema: XrefAttachArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["create", "import"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["attachXref"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("attachXref", insertParams(args as unknown as InsertArgs)),
      ),
    },
    overlay: {
      action: "overlay",
      inputSchema: XrefOverlayArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["create", "import"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["overlayXref"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("overlayXref", insertParams(args as unknown as InsertArgs)),
      ),
    },
    detach: {
      action: "detach",
      inputSchema: XrefDetachArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["delete", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["detachXrefs"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("detachXrefs", nameParams(args as NamesArgs)),
      ),
    },
    reload: {
      action: "reload",
      inputSchema: XrefReloadArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["reloadXrefs"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("reloadXrefs", nameParams(args as NamesArgs)),
      ),
    },
    unload: {
      action: "unload",
      inputSchema: XrefUnloadArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["unloadXrefs"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("unloadXrefs", nameParams(args as NamesArgs)),
      ),
    },
    bind: {
      action: "bind",
      inputSchema: XrefBindArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["bindXrefs"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("bindXrefs", {
          ...nameParams(args as NamesArgs),
          bindType: args.bindType ?? "bind",
        }),
      ),
    },
    repath: {
      action: "repath",
      inputSchema: XrefRepathArgsSchema,
      responseSchema: GenericXrefResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["repathXref"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("repathXref", {
          name: args.name,
          newPath: args.newPath,
          pathType: args.pathType ?? "absolute",
          reload: args.reload ?? true,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_xref",
      displayName: "Civil 3D Xrefs",
      description:
        "Manages external references (xrefs) in the active drawing: list (saved vs found path, loaded/unloaded/unresolved/orphaned status, attach vs overlay, nesting), attach, overlay, detach, reload, unload, bind (bind or insert), and repath (absolute or relative). All caller paths must be .dwg files inside the plugin's configured import roots. Every action except list requires approval.",
      inputShape: canonicalXrefInputShape,
      supportedActions: [...XREF_ACTIONS],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
  ],
};
