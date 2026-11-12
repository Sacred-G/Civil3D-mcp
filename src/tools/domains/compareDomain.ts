import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

// Drawing comparison is its own domain rather than an action on civil3d_qc or
// civil3d_drawing: qc mixes checks with standards remediation (a write) and
// drawing owns document lifecycle (new/save/undo). Keeping compare separate
// lets `drawing` and `snapshot_compare` stay unambiguously read-only, while
// `snapshot` — the only action that writes anything (a JSON file, never the
// drawing) — is classified as an export and gated like every other exporter.

const DwgPathSchema = z.string().min(1).regex(/\.dwg$/i, "otherPath must point to a .dwg file.");
const JsonPathSchema = z.string().min(1).regex(/\.json$/i, "Snapshot paths must end in .json.");
const MaxDetailsSchema = z.number().int().min(0).max(5000);

const EntityRowSchema = z.object({
  handle: z.string(),
  type: z.string(),
  layer: z.string(),
  space: z.string(),
  previousLayer: z.string().optional(),
  previousSpace: z.string().optional(),
});

const GroupRowSchema = z.object({
  name: z.string(),
  added: z.number().int(),
  removed: z.number().int(),
  modified: z.number().int(),
});

const SideSchema = z.object({
  sourcePath: z.string().nullish(),
  sourceKind: z.string().nullish(),
  capturedAtUtc: z.string().nullish(),
  entityCount: z.number().int(),
  civilObjectCount: z.number().int(),
  warnings: z.array(z.string()),
});

export const CompareResponseSchema = z.object({
  mode: z.enum(["drawing", "snapshot"]),
  snapshotPath: z.string().optional(),
  baseline: SideSchema,
  current: SideSchema,
  summary: z.object({
    added: z.number().int(),
    removed: z.number().int(),
    modified: z.number().int(),
    unchanged: z.number().int(),
    layerChanges: z.number().int(),
    identical: z.boolean(),
  }),
  byType: z.array(GroupRowSchema),
  byLayer: z.array(GroupRowSchema),
  details: z.object({
    maxDetails: z.number().int(),
    truncated: z.boolean(),
    added: z.array(EntityRowSchema),
    removed: z.array(EntityRowSchema),
    modified: z.array(EntityRowSchema),
  }),
  civil: z.object({
    truncated: z.boolean(),
    byKind: z.array(z.object({
      kind: z.string(),
      baseline: z.number().int(),
      current: z.number().int(),
      added: z.number().int(),
      removed: z.number().int(),
      modified: z.number().int(),
    })),
    added: z.array(z.object({ kind: z.string(), name: z.string(), handle: z.string().nullish() }).passthrough()),
    removed: z.array(z.object({ kind: z.string(), name: z.string(), handle: z.string().nullish() }).passthrough()),
    modified: z.array(z.object({
      kind: z.string(),
      name: z.string(),
      handle: z.string().nullish(),
      changes: z.record(z.unknown()),
    })),
    unchanged: z.number().int(),
  }),
  notes: z.array(z.string()),
});

export const SnapshotResponseSchema = z.object({
  outputPath: z.string(),
  schema: z.string(),
  capturedAtUtc: z.string().nullish(),
  sourcePath: z.string().nullish(),
  entityCount: z.number().int(),
  civilObjectCount: z.number().int(),
  bytes: z.number().int(),
  contentHash: z.string(),
  warnings: z.array(z.string()),
});

export const CompareDrawingArgsSchema = z.object({
  action: z.literal("drawing"),
  otherPath: DwgPathSchema,
  maxDetails: MaxDetailsSchema.optional(),
  includeCivilSummaries: z.boolean().optional(),
});

export const CompareSnapshotArgsSchema = z.object({
  action: z.literal("snapshot"),
  outputPath: JsonPathSchema,
  overwrite: z.boolean().optional(),
  includeCivilSummaries: z.boolean().optional(),
});

export const CompareSnapshotDiffArgsSchema = z.object({
  action: z.literal("compare_snapshot"),
  snapshotPath: JsonPathSchema,
  maxDetails: MaxDetailsSchema.optional(),
  includeCivilSummaries: z.boolean().optional(),
});

const COMPARE_ACTIONS = ["drawing", "snapshot", "compare_snapshot"] as const;

export const COMPARE_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "compare",
  actions: {
    drawing: {
      action: "drawing",
      inputSchema: CompareDrawingArgsSchema,
      responseSchema: CompareResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["compareDrawings"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("compareDrawings", {
          otherPath: args.otherPath,
          maxDetails: args.maxDetails ?? 200,
          includeCivilSummaries: args.includeCivilSummaries ?? true,
        }),
      ),
    },
    snapshot: {
      action: "snapshot",
      inputSchema: CompareSnapshotArgsSchema,
      responseSchema: SnapshotResponseSchema,
      capabilities: ["query", "export"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["writeDrawingSnapshot"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("writeDrawingSnapshot", {
          outputPath: args.outputPath,
          overwrite: args.overwrite ?? false,
          includeCivilSummaries: args.includeCivilSummaries ?? true,
        }),
      ),
    },
    compare_snapshot: {
      action: "compare_snapshot",
      inputSchema: CompareSnapshotDiffArgsSchema,
      responseSchema: CompareResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["compareDrawingSnapshot"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("compareDrawingSnapshot", {
          snapshotPath: args.snapshotPath,
          maxDetails: args.maxDetails ?? 200,
          includeCivilSummaries: args.includeCivilSummaries ?? true,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_compare",
      displayName: "Civil 3D Compare",
      description:
        "Compares the active drawing without modifying anything. 'drawing' reads another DWG as a side database (never opened as a document) and reports added/removed/modified entities by type and layer plus Civil 3D object changes (alignment length/geometry hash, profile PVIs, surface statistics, pipe counts/inverts). 'snapshot' writes a JSON fingerprint of the current drawing to an allowed export folder (approval required, overwrite defaults to false); 'compare_snapshot' diffs the current drawing against such a snapshot, e.g. 'what changed since the last submittal'. snapshot writes through the plugin's export roots; compare_snapshot reads a snapshot from either the import or the export roots.",
      inputShape: {
        action: z.enum(COMPARE_ACTIONS),
        otherPath: z.string().optional(),
        outputPath: z.string().optional(),
        snapshotPath: z.string().optional(),
        overwrite: z.boolean().optional(),
        maxDetails: MaxDetailsSchema.optional(),
        includeCivilSummaries: z.boolean().optional(),
      },
      supportedActions: [...COMPARE_ACTIONS],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
  ],
};
