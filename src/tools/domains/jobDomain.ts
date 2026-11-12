import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const JobOperationSchema = z.enum([
  "publish_sheet_pdf",
  "surface_dem_import",
  "bulk_qc_report",
  "plot_layouts_to_pdf",
  "publish_sheet_set",
]);

const JobStatusArgs = z.object({ action: z.literal("status"), jobId: z.string().min(1) });
const JobCancelArgs = z.object({ action: z.literal("cancel"), jobId: z.string().min(1) });
const JobStartArgs = z.object({
  action: z.literal("start"),
  operation: JobOperationSchema,
  parameters: z.record(z.unknown()),
});

const JobResponseSchema = z.object({
  jobId: z.string(),
  state: z.enum(["running", "completed", "failed", "cancelled"]),
  operation: z.string(),
  progressPercent: z.number().nullable().optional(),
  currentPhase: z.string().nullable().optional(),
  estimatedRemainingSeconds: z.number().nullable().optional(),
  result: z.unknown().nullable().optional(),
  createdAt: z.string().or(z.date()).optional(),
  completedAt: z.string().or(z.date()).nullable().optional(),
  durationMs: z.number().nullable().optional(),
  requestId: z.string().nullable().optional(),
  drawingIdentity: z.string().nullable().optional(),
  failureCategory: z.string().nullable().optional(),
  warnings: z.array(z.string()).optional(),
  registry: z.object({
    total: z.number().int().nonnegative(),
    running: z.number().int().nonnegative(),
    capacity: z.number().int().positive(),
    terminalRetentionMinutes: z.number().int().positive(),
  }).optional(),
}).passthrough();

export const JOB_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "job",
  actions: {
    start: {
      action: "start",
      inputSchema: JobStartArgs,
      responseSchema: JobResponseSchema,
      capabilities: ["manage", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["startJob"],
      execute: async (args) => await withApplicationConnection(async (appClient) =>
        await appClient.sendCommand("startJob", {
          operation: args.operation,
          parameters: args.parameters,
        })),
    },
    status: {
      action: "status",
      inputSchema: JobStatusArgs,
      responseSchema: JobResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["getJobStatus"],
      execute: async (args) => await withApplicationConnection(async (appClient) =>
        await appClient.sendCommand("getJobStatus", { jobId: args.jobId })),
    },
    cancel: {
      action: "cancel",
      inputSchema: JobCancelArgs,
      responseSchema: JobResponseSchema,
      capabilities: ["manage"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["cancelJob"],
      execute: async (args) => await withApplicationConnection(async (appClient) =>
        await appClient.sendCommand("cancelJob", { jobId: args.jobId })),
    },
  },
  exposures: [{
    toolName: "civil3d_job",
    displayName: "Civil 3D Job",
    description: "Starts approved long-running publish, DEM import, or bulk-QC jobs and checks or cancels existing Civil 3D jobs. Corridor rebuild actions already return jobs directly.",
    inputShape: {
      action: z.enum(["start", "status", "cancel"]),
      jobId: z.string().optional(),
      operation: JobOperationSchema.optional(),
      parameters: z.record(z.unknown()).optional(),
    },
    supportedActions: ["start", "status", "cancel"],
    resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
  }],
};
