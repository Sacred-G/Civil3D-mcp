import { z, type ZodRawShape, type ZodTypeAny } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { ToolCapability, ToolCatalogEntry, ToolDomain } from "./toolMetadata.js";

type JsonObject = Record<string, unknown>;

export interface DomainActionDefinition<TArgs = JsonObject> {
  action: string;
  inputSchema: z.ZodType<TArgs>;
  execute: (args: TArgs) => Promise<unknown>;
  responseSchema?: ZodTypeAny;
  capabilities: ToolCapability[];
  requiresActiveDrawing: boolean;
  safeForRetry: boolean;
  pluginMethods?: string[];
}

export interface DomainToolExposure {
  toolName: string;
  displayName: string;
  description: string;
  inputShape: ZodRawShape;
  supportedActions: string[];
  resolveAction: (rawArgs: JsonObject) => { action: string; args: JsonObject };
  capabilities?: ToolCapability[];
  operations?: string[];
  pluginMethods?: string[];
  requiresActiveDrawing?: boolean;
  safeForRetry?: boolean;
  status?: "implemented" | "planned";
}

export interface DomainToolDefinition {
  domain: ToolDomain;
  actions: Record<string, DomainActionDefinition>;
  exposures: DomainToolExposure[];
}

function uniqueStrings(values: Iterable<string | undefined>): string[] | undefined {
  const unique = [...new Set([...values].filter((value): value is string => Boolean(value)))];
  return unique.length > 0 ? unique : undefined;
}

function uniqueCapabilities(values: Iterable<ToolCapability | undefined>): ToolCapability[] {
  return [...new Set([...values].filter((value): value is ToolCapability => Boolean(value)))];
}

function buildToolErrorResult(toolName: string, actionName: string | undefined, error: unknown) {
  const message = error instanceof Error ? error.message : String(error);
  const scopedName = actionName ? `${toolName} action '${actionName}'` : toolName;

  console.error(`Error in ${scopedName}:`, error);

  return {
    content: [
      {
        type: "text" as const,
        text: `${scopedName} failed: ${message}`,
      },
    ],
    isError: true,
  };
}

async function executeExposure(
  definition: DomainToolDefinition,
  exposure: DomainToolExposure,
  rawArgs: JsonObject,
) {
  const resolved = exposure.resolveAction(rawArgs);
  const actionName = resolved.action;

  if (!exposure.supportedActions.includes(actionName)) {
    throw new Error(
      `Unsupported action '${actionName}' for tool '${exposure.toolName}'. ` +
      `Supported actions: ${exposure.supportedActions.join(", ")}.`,
    );
  }

  const actionDefinition = definition.actions[actionName];
  if (!actionDefinition) {
    throw new Error(`Action '${actionName}' is not defined for domain '${definition.domain}'.`);
  }

  const parsedArgs = actionDefinition.inputSchema.parse(resolved.args);
  const response = await actionDefinition.execute(parsedArgs);
  const validatedResponse = actionDefinition.responseSchema
    ? actionDefinition.responseSchema.parse(response)
    : response;

  return {
    content: [
      {
        type: "text" as const,
        text: JSON.stringify(validatedResponse, null, 2),
      },
    ],
  };
}

export function registerDomainTools(server: McpServer, definition: DomainToolDefinition) {
  for (const exposure of definition.exposures) {
    server.tool(
      exposure.toolName,
      exposure.description,
      exposure.inputShape,
      async (rawArgs) => {
        try {
          return await executeExposure(definition, exposure, rawArgs as JsonObject);
        } catch (error) {
          const actionName =
            typeof (rawArgs as JsonObject).action === "string"
              ? String((rawArgs as JsonObject).action)
              : exposure.supportedActions.length === 1
                ? exposure.supportedActions[0]
                : undefined;

          return buildToolErrorResult(exposure.toolName, actionName, error);
        }
      },
    );
  }
}

export function buildDomainToolCatalogEntries(definition: DomainToolDefinition): ToolCatalogEntry[] {
  return definition.exposures.map((exposure) => {
    const supportedActionDefinitions = exposure.supportedActions
      .map((actionName) => definition.actions[actionName])
      .filter((action): action is DomainActionDefinition => Boolean(action));

    return {
      toolName: exposure.toolName,
      displayName: exposure.displayName,
      description: exposure.description,
      domain: definition.domain,
      capabilities: exposure.capabilities ?? uniqueCapabilities(
        supportedActionDefinitions.flatMap((action) => action.capabilities),
      ),
      operations: exposure.operations ?? (exposure.supportedActions.length > 1
        ? exposure.supportedActions
        : undefined),
      pluginMethods: exposure.pluginMethods ?? uniqueStrings(
        supportedActionDefinitions.flatMap((action) => action.pluginMethods ?? []),
      ),
      requiresActiveDrawing: exposure.requiresActiveDrawing
        ?? supportedActionDefinitions.some((action) => action.requiresActiveDrawing),
      safeForRetry: exposure.safeForRetry
        ?? supportedActionDefinitions.every((action) => action.safeForRetry),
      status: exposure.status ?? "implemented",
    };
  });
}
