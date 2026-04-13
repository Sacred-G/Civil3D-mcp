import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const GenericAssemblyResponseSchema = z.object({}).passthrough();

const AssemblyParameterValueSchema = z.union([z.number(), z.string(), z.boolean()]);

const AssemblySummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  subassemblyCount: z.number(),
  usedByCorridors: z.array(z.string()),
});

const AssemblyListResponseSchema = z.object({
  assemblies: z.array(AssemblySummarySchema),
});

const SubassemblySchema = z.object({
  name: z.string(),
  side: z.enum(["left", "right", "none"]),
  className: z.string(),
  parameters: z.record(AssemblyParameterValueSchema),
});

const AssemblyDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  style: z.string(),
  subassemblies: z.array(SubassemblySchema),
  usedByCorridors: z.array(z.string()),
});

const canonicalAssemblyInputShape = {
  action: z.enum(["list", "get", "create", "create_subassembly", "edit"]),
  name: z.string().optional(),
  insertionPoint: z.object({
    x: z.number(),
    y: z.number(),
  }).optional(),
  description: z.string().optional(),
  assemblyName: z.string().optional(),
  subassemblyType: z.string().optional(),
  side: z.enum(["Left", "Right", "Both"]).optional(),
  parameters: z.record(AssemblyParameterValueSchema).optional(),
  subassemblyName: z.string().optional(),
  delete: z.boolean().optional(),
};

const AssemblyListArgsSchema = z.object({
  action: z.literal("list"),
});

const AssemblyGetArgsSchema = z.object({
  action: z.literal("get"),
  name: z.string(),
});

const AssemblyCreateArgsSchema = z.object({
  action: z.literal("create"),
  name: z.string(),
  insertionPoint: z.object({
    x: z.number(),
    y: z.number(),
  }).optional(),
  description: z.string().optional(),
});

const AssemblyCreateSubassemblyArgsSchema = z.object({
  action: z.literal("create_subassembly"),
  assemblyName: z.string(),
  subassemblyType: z.string(),
  side: z.enum(["Left", "Right", "Both"]),
  parameters: z.record(AssemblyParameterValueSchema).optional(),
});

const AssemblyEditArgsSchema = z.object({
  action: z.literal("edit"),
  assemblyName: z.string(),
  subassemblyName: z.string().optional(),
  parameters: z.record(AssemblyParameterValueSchema).optional(),
  delete: z.boolean().optional(),
});

export const ASSEMBLY_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "assembly",
  actions: {
    list: {
      action: "list",
      inputSchema: AssemblyListArgsSchema,
      responseSchema: AssemblyListResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listAssemblies"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listAssemblies", {}),
      ),
    },
    get: {
      action: "get",
      inputSchema: AssemblyGetArgsSchema,
      responseSchema: AssemblyDetailResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getAssembly"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getAssembly", {
          name: args.name,
        }),
      ),
    },
    create: {
      action: "create",
      inputSchema: AssemblyCreateArgsSchema,
      responseSchema: GenericAssemblyResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createAssembly"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const insertionPoint = args.insertionPoint as { x: number; y: number } | undefined;
        return await appClient.sendCommand("createAssembly", {
          name: args.name,
          insertX: insertionPoint?.x ?? 0,
          insertY: insertionPoint?.y ?? 0,
          description: args.description ?? "",
        });
      }),
    },
    create_subassembly: {
      action: "create_subassembly",
      inputSchema: AssemblyCreateSubassemblyArgsSchema,
      responseSchema: GenericAssemblyResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createSubassembly"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createSubassembly", {
          assemblyName: args.assemblyName,
          subassemblyType: args.subassemblyType,
          side: args.side,
          parameters: args.parameters ?? {},
        }),
      ),
    },
    edit: {
      action: "edit",
      inputSchema: AssemblyEditArgsSchema,
      responseSchema: GenericAssemblyResponseSchema,
      capabilities: ["inspect", "edit", "delete"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["editAssembly"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("editAssembly", {
          assemblyName: args.assemblyName,
          subassemblyName: args.subassemblyName ?? null,
          parameters: args.parameters ?? null,
          delete: args.delete ?? false,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_assembly",
      displayName: "Civil 3D Assembly",
      description: "Lists, inspects, creates, and edits Civil 3D assemblies and subassemblies through a single domain tool.",
      inputShape: canonicalAssemblyInputShape,
      supportedActions: ["list", "get", "create", "create_subassembly", "edit"],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_assembly_create",
      displayName: "Civil 3D Assembly Create",
      description: "Creates a new Civil 3D assembly at a specified model-space location.",
      inputShape: {
        name: z.string(),
        insertionPoint: z.object({
          x: z.number(),
          y: z.number(),
        }).optional(),
        description: z.string().optional(),
      },
      supportedActions: ["create"],
      resolveAction: (rawArgs) => ({
        action: "create",
        args: {
          action: "create",
          name: rawArgs.name,
          insertionPoint: rawArgs.insertionPoint,
          description: rawArgs.description,
        },
      }),
    },
    {
      toolName: "civil3d_subassembly_create",
      displayName: "Civil 3D Subassembly Create",
      description: "Adds a subassembly from the Civil 3D catalog to an existing assembly.",
      inputShape: {
        assemblyName: z.string(),
        subassemblyType: z.string(),
        side: z.enum(["Left", "Right", "Both"]),
        parameters: z.record(AssemblyParameterValueSchema).optional(),
      },
      supportedActions: ["create_subassembly"],
      resolveAction: (rawArgs) => ({
        action: "create_subassembly",
        args: {
          action: "create_subassembly",
          assemblyName: rawArgs.assemblyName,
          subassemblyType: rawArgs.subassemblyType,
          side: rawArgs.side,
          parameters: rawArgs.parameters,
        },
      }),
    },
    {
      toolName: "civil3d_assembly_edit",
      displayName: "Civil 3D Assembly Edit",
      description: "Inspects or modifies an existing Civil 3D assembly, including subassembly parameter edits and deletion.",
      inputShape: {
        assemblyName: z.string(),
        subassemblyName: z.string().optional(),
        parameters: z.record(AssemblyParameterValueSchema).optional(),
        delete: z.boolean().optional(),
      },
      supportedActions: ["edit"],
      resolveAction: (rawArgs) => ({
        action: "edit",
        args: {
          action: "edit",
          assemblyName: rawArgs.assemblyName,
          subassemblyName: rawArgs.subassemblyName,
          parameters: rawArgs.parameters,
          delete: rawArgs.delete,
        },
      }),
    },
  ],
};
