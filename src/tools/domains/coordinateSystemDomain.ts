import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const InfoArgs = z.object({ action: z.literal("info") });
const TransformArgs = z.object({ action: z.literal("transform"), fromSystem: z.enum(["drawing", "geographic"]), toSystem: z.enum(["drawing", "geographic"]), x: z.number(), y: z.number(), z: z.number().optional() });
const InfoResponseSchema = z.object({ name: z.string().nullable(), zone: z.string().nullable(), datum: z.string().nullable(), projection: z.string().nullable(), linearUnits: z.string(), centralMeridian: z.number().nullable(), falseEasting: z.number().nullable(), falseNorthing: z.number().nullable(), scaleFactor: z.number().nullable() });
const TransformResponseSchema = z.object({ x: z.number(), y: z.number(), z: z.number().optional() }).passthrough();

export const COORDINATE_SYSTEM_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "coordinate_system",
  actions: {
    info: { action: "info", inputSchema: InfoArgs, responseSchema: InfoResponseSchema, capabilities: ["query", "inspect"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["getCoordinateSystemInfo"], execute: async () => await withApplicationConnection(async (appClient) => await appClient.sendCommand("getCoordinateSystemInfo", {})) },
    transform: { action: "transform", inputSchema: TransformArgs, responseSchema: TransformResponseSchema, capabilities: ["query", "analyze"], requiresActiveDrawing: false, safeForRetry: true, pluginMethods: ["transformCoordinates"], execute: async (args) => await withApplicationConnection(async (appClient) => await appClient.sendCommand("transformCoordinates", { fromSystem: args.fromSystem, toSystem: args.toSystem, x: args.x, y: args.y, z: args.z })) },
  },
  exposures: [
    { toolName: "civil3d_coordinate_system", displayName: "Civil 3D Coordinate System", description: "Provides coordinate system information and coordinate transformations.", inputShape: { action: z.enum(["info", "transform"]), fromSystem: z.enum(["drawing", "geographic"]).optional(), toSystem: z.enum(["drawing", "geographic"]).optional(), x: z.number().optional(), y: z.number().optional(), z: z.number().optional() }, supportedActions: ["info", "transform"], resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }) },
  ],
};
