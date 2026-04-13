import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const HealthResponseSchema = z.object({
  connected: z.boolean(),
  civil3dVersion: z.string().optional(),
  pluginVersion: z.string().optional(),
  drawingLoaded: z.boolean(),
  operationInProgress: z.boolean(),
  currentOperation: z.string().nullable(),
  queueDepth: z.number(),
  memoryUsageMb: z.number(),
});

export const PLUGIN_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "plugin",
  actions: {
    health: {
      action: "health",
      inputSchema: z.object({ action: z.literal("health") }),
      responseSchema: HealthResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      pluginMethods: ["getCivil3DHealth"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getCivil3DHealth", {}),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_health",
      displayName: "Civil 3D Health",
      description: "Reports the status of the Civil 3D connection and plugin.",
      inputShape: {},
      supportedActions: ["health"],
      resolveAction: () => ({ action: "health", args: { action: "health" } }),
    },
  ],
};
