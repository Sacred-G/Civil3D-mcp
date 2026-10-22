import type { Transport } from "@modelcontextprotocol/sdk/shared/transport.js";
import type { JSONRPCMessage } from "@modelcontextprotocol/sdk/types.js";

type JsonObject = Record<string, unknown>;

function isObject(value: unknown): value is JsonObject {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function rewriteRef(ref: string): string {
  return ref.replace(/\/items\/(\d+)(?=\/|$)/g, "/prefixItems/$1").replace(/\/additionalItems(?=\/|$)/g, "/items");
}

function rewriteSchema(node: unknown): unknown {
  if (Array.isArray(node)) {
    return node.map(rewriteSchema);
  }
  if (!isObject(node)) {
    return node;
  }

  const tupleForm = Array.isArray(node.items);
  const out: JsonObject = {};
  for (const [key, value] of Object.entries(node)) {
    if (key === "$schema") {
      continue;
    }
    if (key === "$ref" && typeof value === "string") {
      out.$ref = rewriteRef(value);
    } else if (tupleForm && key === "items") {
      out.prefixItems = rewriteSchema(value);
    } else if (tupleForm && key === "additionalItems") {
      out.items = rewriteSchema(value);
    } else {
      out[key] = rewriteSchema(value);
    }
  }
  return out;
}

/**
 * Converts a draft-07 tool schema, as produced by the SDK's Zod converter, to
 * JSON Schema 2020-12. Newer MCP clients validate tool schemas with a
 * 2020-12-only validator and reject any tool that declares another dialect.
 * Besides dropping "$schema", tuple schemas (z.tuple) must move from the
 * draft-07 array form of "items" to "prefixItems", or the schema is invalid.
 */
export function toDraft2020ToolSchema(schema: unknown): unknown {
  return rewriteSchema(schema);
}

export function stripSchemaDialectFromToolList(message: JSONRPCMessage): JSONRPCMessage {
  if (!("result" in message) || !isObject(message.result)) {
    return message;
  }
  const tools = message.result.tools;
  if (!Array.isArray(tools)) {
    return message;
  }

  return {
    ...message,
    result: {
      ...message.result,
      tools: tools.map((tool) => {
        if (!isObject(tool)) {
          return tool;
        }
        const cleaned: JsonObject = { ...tool, inputSchema: toDraft2020ToolSchema(tool.inputSchema) };
        if ("outputSchema" in tool) {
          cleaned.outputSchema = toDraft2020ToolSchema(tool.outputSchema);
        }
        return cleaned;
      }),
    },
  } as JSONRPCMessage;
}

export function stripSchemaDialectOnSend<T extends Transport>(transport: T): T {
  const send = transport.send.bind(transport);
  transport.send = (message, options) => send(stripSchemaDialectFromToolList(message), options);
  return transport;
}
