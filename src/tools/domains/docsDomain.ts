import { z } from "zod";
import type { DomainToolDefinition } from "../domainRuntime.js";

const ToolCapabilityListResponseSchema = z.object({
  domains: z.array(z.string()),
  tools: z.array(z.object({
    toolName: z.string(),
    displayName: z.string(),
    description: z.string(),
    domain: z.string(),
    capabilities: z.array(z.string()),
    operations: z.array(z.string()).optional(),
    pluginMethods: z.array(z.string()).optional(),
    requiresActiveDrawing: z.boolean(),
    safeForRetry: z.boolean(),
    status: z.enum(["implemented", "planned"]),
  })),
});

const ListToolCapabilitiesArgsSchema = z.object({
  action: z.literal("list_tool_capabilities"),
  domain: z.string().optional(),
  status: z.enum(["implemented", "planned"]).optional(),
});

const OrchestrateArgsSchema = z.object({
  action: z.literal("orchestrate"),
  request: z.string().min(1).optional(),
  execute: z.boolean().optional(),
  toolName: z.string().optional(),
  toolAction: z.string().optional(),
  toolParameters: z.record(z.unknown()).optional(),
  name: z.string().optional(),
  alignmentName: z.string().optional(),
  corridorName: z.string().optional(),
  profileName: z.string().optional(),
  surfaceName: z.string().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
  filePath: z.string().optional(),
  outputPath: z.string().optional(),
  gridSpacing: z.number().optional(),
  designSpeed: z.number().optional(),
  inflow: z.number().optional(),
  outflow: z.number().optional(),
  bottomElevation: z.number().optional(),
  topElevation: z.number().optional(),
  payItems: z.array(z.object({
    code: z.string(),
    description: z.string(),
    unit: z.string(),
    unitPrice: z.number().nonnegative(),
  })).optional(),
});

const canonicalDocsInputShape = {
  action: z.enum(["list_tool_capabilities", "orchestrate"]),
  domain: z.string().optional(),
  status: z.enum(["implemented", "planned"]).optional(),
  request: z.string().min(1).optional(),
  execute: z.boolean().optional(),
  toolName: z.string().optional(),
  toolAction: z.string().optional(),
  toolParameters: z.record(z.unknown()).optional(),
  name: z.string().optional(),
  alignmentName: z.string().optional(),
  corridorName: z.string().optional(),
  profileName: z.string().optional(),
  surfaceName: z.string().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
  filePath: z.string().optional(),
  outputPath: z.string().optional(),
  gridSpacing: z.number().optional(),
  designSpeed: z.number().optional(),
  inflow: z.number().optional(),
  outflow: z.number().optional(),
  bottomElevation: z.number().optional(),
  topElevation: z.number().optional(),
  payItems: z.array(z.object({
    code: z.string(),
    description: z.string(),
    unit: z.string(),
    unitPrice: z.number().nonnegative(),
  })).optional(),
};

export const DOCS_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "docs",
  actions: {
    list_tool_capabilities: {
      action: "list_tool_capabilities",
      inputSchema: ListToolCapabilitiesArgsSchema,
      responseSchema: ToolCapabilityListResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      execute: async (args) => {
        const parsedArgs = args as z.infer<typeof ListToolCapabilitiesArgsSchema>;
        const { listDomains, listToolCatalog } = await import("../tool_catalog.js");
        const domainFilter = parsedArgs.domain?.trim().toLowerCase();
        const statusFilter = parsedArgs.status;
        return ToolCapabilityListResponseSchema.parse({
          domains: listDomains(),
          tools: listToolCatalog().filter((entry) => {
            const domainMatches = !domainFilter || entry.domain === domainFilter;
            const statusMatches = !statusFilter || entry.status === statusFilter;
            return domainMatches && statusMatches;
          }),
        });
      },
    },
    orchestrate: {
      action: "orchestrate",
      inputSchema: OrchestrateArgsSchema,
      responseSchema: z.object({}).passthrough(),
      capabilities: ["query", "inspect", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      execute: async (args) => {
        const parsedArgs = args as z.infer<typeof OrchestrateArgsSchema>;
        const { executeCivil3DOrchestrate } = await import("../civil3d_orchestrate.js");
        return await executeCivil3DOrchestrate({
          request: parsedArgs.request,
          execute: parsedArgs.execute,
          toolName: parsedArgs.toolName,
          toolAction: parsedArgs.toolAction,
          toolParameters: parsedArgs.toolParameters,
          name: parsedArgs.name,
          alignmentName: parsedArgs.alignmentName,
          corridorName: parsedArgs.corridorName,
          profileName: parsedArgs.profileName,
          surfaceName: parsedArgs.surfaceName,
          baseSurface: parsedArgs.baseSurface,
          comparisonSurface: parsedArgs.comparisonSurface,
          style: parsedArgs.style,
          layer: parsedArgs.layer,
          labelSet: parsedArgs.labelSet,
          filePath: parsedArgs.filePath,
          outputPath: parsedArgs.outputPath,
          gridSpacing: parsedArgs.gridSpacing,
          designSpeed: parsedArgs.designSpeed,
          inflow: parsedArgs.inflow,
          outflow: parsedArgs.outflow,
          bottomElevation: parsedArgs.bottomElevation,
          topElevation: parsedArgs.topElevation,
          payItems: parsedArgs.payItems,
        });
      },
    },
  },
  exposures: [
    {
      toolName: "civil3d_docs",
      displayName: "Civil 3D Docs",
      description: "Lists tool capabilities and routes natural-language orchestration requests through a single docs/orchestration tool.",
      inputShape: canonicalDocsInputShape,
      supportedActions: ["list_tool_capabilities", "orchestrate"],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "list_tool_capabilities",
      displayName: "List Tool Capabilities",
      description: "Lists domain and capability metadata for the Civil 3D MCP tool catalog, including implemented and planned tools.",
      inputShape: {
        domain: z.string().optional(),
        status: z.enum(["implemented", "planned"]).optional(),
      },
      supportedActions: ["list_tool_capabilities"],
      resolveAction: (rawArgs) => ({ action: "list_tool_capabilities", args: { action: "list_tool_capabilities", ...rawArgs } }),
    },
    {
      toolName: "civil3d_orchestrate",
      displayName: "Civil 3D Orchestrate",
      description: "Routes a natural-language Civil 3D request to the best starting tool or action and executes routed work through the registered MCP tool surface.",
      inputShape: {
        request: z.string().min(1).optional(),
        execute: z.boolean().optional(),
        toolName: z.string().optional(),
        toolAction: z.string().optional(),
        toolParameters: z.record(z.unknown()).optional(),
        name: z.string().optional(),
        alignmentName: z.string().optional(),
        corridorName: z.string().optional(),
        profileName: z.string().optional(),
        surfaceName: z.string().optional(),
        baseSurface: z.string().optional(),
        comparisonSurface: z.string().optional(),
        style: z.string().optional(),
        layer: z.string().optional(),
        labelSet: z.string().optional(),
        filePath: z.string().optional(),
        outputPath: z.string().optional(),
        gridSpacing: z.number().optional(),
        designSpeed: z.number().optional(),
        inflow: z.number().optional(),
        outflow: z.number().optional(),
        bottomElevation: z.number().optional(),
        topElevation: z.number().optional(),
        payItems: z.array(z.object({
          code: z.string(),
          description: z.string(),
          unit: z.string(),
          unitPrice: z.number().nonnegative(),
        })).optional(),
      },
      supportedActions: ["orchestrate"],
      resolveAction: (rawArgs) => ({ action: "orchestrate", args: { action: "orchestrate", ...rawArgs } }),
    },
  ],
};
