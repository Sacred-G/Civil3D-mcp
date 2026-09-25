import { describe, expect, it } from "vitest";
import Ajv2020 from "ajv/dist/2020.js";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { InMemoryTransport } from "@modelcontextprotocol/sdk/inMemory.js";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { registerTools } from "../src/tools/register.js";
import { stripSchemaDialectFromToolList, stripSchemaDialectOnSend, toDraft2020ToolSchema } from "../src/utils/schemaDialect.js";

describe("tool schema dialect", () => {
  it("drops $schema only from tool list results", () => {
    const listResult = {
      jsonrpc: "2.0" as const,
      id: 1,
      result: {
        tools: [
          {
            name: "t",
            inputSchema: { $schema: "http://json-schema.org/draft-07/schema#", type: "object" },
            outputSchema: { $schema: "http://json-schema.org/draft-07/schema#", type: "object" },
          },
        ],
      },
    };
    const cleaned = stripSchemaDialectFromToolList(listResult) as typeof listResult;
    expect(cleaned.result.tools[0].inputSchema).toEqual({ type: "object" });
    expect(cleaned.result.tools[0].outputSchema).toEqual({ type: "object" });

    const other = { jsonrpc: "2.0" as const, id: 2, result: { $schema: "x" } };
    expect(stripSchemaDialectFromToolList(other)).toBe(other);
  });

  it("converts draft-07 tuples to prefixItems and follows refs into them", () => {
    const tuple = { type: "array", items: [{ type: "number" }, { type: "number" }], minItems: 2, maxItems: 2 };
    expect(
      toDraft2020ToolSchema({
        type: "object",
        properties: {
          a: { type: "array", items: tuple },
          b: { $ref: "#/properties/a/items/items/0" },
          items: { type: "string" },
        },
      }),
    ).toEqual({
      type: "object",
      properties: {
        a: {
          type: "array",
          items: { type: "array", prefixItems: [{ type: "number" }, { type: "number" }], minItems: 2, maxItems: 2 },
        },
        b: { $ref: "#/properties/a/items/prefixItems/0" },
        items: { type: "string" },
      },
    });
  });

  it("serves every registered tool with schemas a 2020-12 validator accepts", async () => {
    const server = new McpServer({ name: "schema-dialect-test", version: "0.0.0" });
    await registerTools(server);

    const [clientTransport, serverTransport] = InMemoryTransport.createLinkedPair();
    await server.connect(stripSchemaDialectOnSend(serverTransport));
    const client = new Client({ name: "schema-dialect-client", version: "0.0.0" });
    await client.connect(clientTransport);

    try {
      const { tools } = await client.listTools();
      expect(tools.length).toBeGreaterThan(0);

      const ajv = new Ajv2020({ strict: false });
      for (const tool of tools) {
        for (const schema of [tool.inputSchema, tool.outputSchema]) {
          if (!schema) continue;
          expect(schema, tool.name).not.toHaveProperty("$schema");
          expect(() => ajv.compile(schema), tool.name).not.toThrow();
        }
      }
    } finally {
      await client.close();
      await server.close();
    }
  });
});
