import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const JobArgs = z.object({ action: z.enum(["status", "cancel"]), jobId: z.string() });
const JobResponseSchema = z.object({ jobId: z.string(), state: z.enum(["running", "completed", "failed", "cancelled"]), progressPercent: z.number().nullable().optional(), currentPhase: z.string().nullable().optional(), estimatedRemainingSeconds: z.number().nullable().optional(), result: z.unknown().nullable().optional() });

export const JOB_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "job",
  actions: {
    status: { action: "status", inputSchema: JobArgs.extend({ action: z.literal("status") }), responseSchema: JobResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["getJobStatus"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getJobStatus", { jobId: args.jobId })) },
    cancel: { action: "cancel", inputSchema: JobArgs.extend({ action: z.literal("cancel") }), responseSchema: JobResponseSchema, capabilities: ["edit", "manage"], requiresActiveDrawing: false, safeForRetry: false, pluginMethods: ["cancelJob"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("cancelJob", { jobId: args.jobId })) },
  },
  exposures: [
    { toolName: "civil3d_job", displayName: "Civil 3D Job", description: "Checks job status or requests cancellation for long-running Civil 3D operations.", inputShape: { action: z.enum(["status", "cancel"]), jobId: z.string() }, supportedActions: ["status", "cancel"], resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }) },
  ],
};
