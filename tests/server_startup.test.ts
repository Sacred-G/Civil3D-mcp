import { describe, expect, it } from "vitest";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { InMemoryTransport } from "@modelcontextprotocol/sdk/inMemory.js";
import { registerTools } from "../src/tools/register.js";
import { getToolHandler, listRegisteredToolNames } from "../src/tools/toolHandlerRegistry.js";
import { MIGRATED_DOMAIN_DEFINITIONS, selectManifestExposures } from "../src/tools/toolManifest.js";

describe("MCP server startup", () => {
  it("exposes typed contracts and resources through a real MCP client", async () => {
    const server = new McpServer({ name: "protocol-smoke", version: "test" });
    const client = new Client({ name: "protocol-client", version: "test" });
    await registerTools(server);
    const [clientTransport, serverTransport] = InMemoryTransport.createLinkedPair();

    try {
      await server.connect(serverTransport);
      await client.connect(clientTransport);
      const tools = await client.listTools();
      const health = tools.tools.find((tool) => tool.name === "civil3d_health");
      const drawing = tools.tools.find((tool) => tool.name === "civil3d_drawing");
      const help = tools.tools.find((tool) => tool.name === "civil3d_help");
      expect(tools.tools).toHaveLength(MIGRATED_DOMAIN_DEFINITIONS.length + 5);
      expect(health?.outputSchema).toBeDefined();
      expect(health?.inputSchema.properties).toHaveProperty("idempotencyKey");
      expect(health?.annotations).toMatchObject({ readOnlyHint: true, destructiveHint: false });
      expect(drawing?.outputSchema).toBeDefined();
      expect(drawing?.annotations).toMatchObject({ readOnlyHint: false, destructiveHint: true });
      expect(help?.inputSchema.properties).toHaveProperty("action");

      const resources = await client.listResources();
      expect(resources.resources.map((resource) => resource.uri)).toEqual(expect.arrayContaining([
        "civil3d://catalog/tools",
        "civil3d://docs/safety-and-approval",
        "civil3d://standards/framework",
      ]));
      const catalogResult = await client.readResource({ uri: "civil3d://catalog/tools" });
      expect((catalogResult.contents[0] as { text?: string }).text).toContain("civil3d_alignment");
    } finally {
      await client.close();
      await server.close();
    }
  });

  it("registers the complete manifest against a real McpServer", async () => {
    const server = new McpServer({ name: "startup-smoke", version: "test" });

    await expect(registerTools(server)).resolves.toBeUndefined();

    const publiclyRegistered = Object.keys((server as any)._registeredTools ?? {});
    expect(publiclyRegistered).toHaveLength(MIGRATED_DOMAIN_DEFINITIONS.length + 5);

    const names = listRegisteredToolNames();
    expect(names.length).toBeGreaterThan(200);
    expect(names.filter((name) => name === "civil3d_orchestrate")).toHaveLength(1);
    expect(names).toContain("civil3d_request_approval");
    expect(names).toContain("civil3d_request_plan_approval");
    expect(names).toContain("civil3d_preview_action");
    expect(names).toContain("civil3d_help");

    const resources = Object.keys((server as any)._registeredResources ?? {});
    const resourceTemplates = Object.keys((server as any)._registeredResourceTemplates ?? {});
    expect(resources).toEqual(expect.arrayContaining([
      "civil3d://catalog/tools",
      "civil3d://docs/safety-and-approval",
      "civil3d://standards/framework",
    ]));
    expect(resourceTemplates).toContain("civil3d-generated-report");
    expect(resourceTemplates).toContain("civil3d-help-topic");
    expect(resourceTemplates).toContain("civil3d-help-image");
    expect(resourceTemplates).toContain("civil3d-help-video");
  });

  it("publishes output schemas and policy-derived MCP annotations", async () => {
    const server = new McpServer({ name: "typed-contracts", version: "test" });
    await registerTools(server);

    const registered = (server as any)._registeredTools;
    expect(registered.civil3d_health.outputSchema).toBeDefined();
    expect(registered.civil3d_health.annotations).toMatchObject({
      readOnlyHint: true,
      destructiveHint: false,
      idempotentHint: true,
      openWorldHint: false,
    });
    expect(registered.civil3d_drawing.outputSchema).toBeDefined();
    expect(registered.civil3d_drawing.annotations).toMatchObject({
      readOnlyHint: false,
      destructiveHint: true,
      idempotentHint: false,
      openWorldHint: false,
    });
  });

  it("returns structured content, progress updates, and a report resource link", async () => {
    const server = new McpServer({ name: "structured-results", version: "test" });
    await registerTools(server);
    const handler = getToolHandler("list_tool_capabilities");
    expect(handler).toBeDefined();

    const notifications: unknown[] = [];
    const result = await handler!(
      {},
      {
        _meta: { progressToken: "progress-1" },
        sendNotification: async (notification: unknown) => { notifications.push(notification); },
      },
    );

    expect(result.isError).toBeUndefined();
    expect(result.structuredContent).toMatchObject({
      action: "list_tool_capabilities",
      result: { domains: expect.any(Array), tools: expect.any(Array) },
    });
    expect(notifications).toHaveLength(3);
    expect(notifications).toEqual(expect.arrayContaining([
      expect.objectContaining({
        method: "notifications/progress",
        params: expect.objectContaining({ progressToken: "progress-1", progress: 100 }),
      }),
    ]));

    const resourceLink = result.content.find((item) => item.type === "resource_link") as any;
    expect(resourceLink?.uri).toMatch(/^civil3d:\/\/reports\//);
    const reportTemplate = (server as any)._registeredResourceTemplates["civil3d-generated-report"];
    const reportId = resourceLink.uri.split("/").pop();
    const resourceResult = await reportTemplate.readCallback(
      new URL(resourceLink.uri),
      { reportId },
      {},
    );
    expect(resourceResult.contents[0].text).toContain('"tools"');
  });

  it("serves the manifest-derived tool catalog as an MCP resource", async () => {
    const server = new McpServer({ name: "catalog-resource", version: "test" });
    await registerTools(server);
    const resource = (server as any)._registeredResources["civil3d://catalog/tools"];
    const result = await resource.readCallback(new URL("civil3d://catalog/tools"), {});
    const catalog = JSON.parse(result.contents[0].text);

    expect(catalog.defaultTools).toContain("civil3d_orchestrate");
    expect(catalog.totalCatalogEntries).toBeGreaterThan(200);
    expect(catalog.tools).toEqual(expect.arrayContaining([
      expect.objectContaining({ toolName: "civil3d_alignment" }),
    ]));
  });

  it("exposes one canonical tool per domain plus the orchestrator by default", () => {
    const defaultExposures = MIGRATED_DOMAIN_DEFINITIONS.flatMap((definition) =>
      selectManifestExposures(definition, false),
    );
    const allExposures = MIGRATED_DOMAIN_DEFINITIONS.flatMap((definition) =>
      selectManifestExposures(definition, true),
    );

    expect(defaultExposures).toHaveLength(MIGRATED_DOMAIN_DEFINITIONS.length + 1);
    expect(new Set(defaultExposures.map((exposure) => exposure.toolName)).size)
      .toBe(defaultExposures.length);
    expect(defaultExposures.map((exposure) => exposure.toolName)).toContain("civil3d_orchestrate");
    expect(allExposures.length).toBeGreaterThan(200);
  });

  it("blocks an unapproved protected action before it reaches the plugin", async () => {
    const server = new McpServer({ name: "approval-gate", version: "test" });
    await registerTools(server);

    const handler = getToolHandler("civil3d_drawing");
    expect(handler).toBeDefined();
    const result = await handler!({ action: "save" });

    expect(result.isError).toBe(true);
    expect(result.content[0].text).toContain("Approval required");
  });

  it("previews approval requirements without contacting the plugin", async () => {
    const server = new McpServer({ name: "approval-preview", version: "test" });
    await registerTools(server);

    const handler = getToolHandler("civil3d_preview_action");
    expect(handler).toBeDefined();
    const result = await handler!({
      toolName: "civil3d_drawing",
      action: "save",
      parameters: { action: "save" },
    });

    expect(result.isError).toBeUndefined();
    expect(result.content[0].text).toContain('"approval_required"');
  });
});
