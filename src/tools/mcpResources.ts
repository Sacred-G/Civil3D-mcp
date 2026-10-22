import { McpServer, ResourceTemplate } from "@modelcontextprotocol/sdk/server/mcp.js";
import { lookupFrameworkStandards } from "../standards/FrameworkStandardsService.js";
import { MIGRATED_DOMAIN_DEFINITIONS, selectManifestExposures } from "./toolManifest.js";
import { TOOL_CATALOG } from "./tool_catalog.js";
import { getReportResource, listReportResources } from "./reportResourceStore.js";

const SAFETY_GUIDANCE = `# Civil 3D MCP safety and execution

All drawing work follows one authoritative path:

MCP or HTTP caller -> schema validation -> approval policy -> host queue -> Civil 3D execution -> structured result

- Query and inspection actions are read-only and retry-safe when their tool annotations say so.
- Destructive or ambiguous actions require preview and a short-lived, drawing-bound approval token.
- A known ordered sequence can be approved in one call with civil3d_request_plan_approval: one token per step, bound to the active document (not its contents), executed in order with exact parameters.
- Drawing mutations execute in the Civil 3D host under command context, document lock, and a narrow transaction.
- The Node MCP layer plans, validates, brokers, and reports results; it does not mutate drawings directly.
`;

export function registerMcpResources(server: McpServer): void {
  server.registerResource(
    "civil3d-tool-catalog",
    "civil3d://catalog/tools",
    {
      title: "Civil 3D Tool Catalog",
      description: "Complete manifest-derived Civil 3D tool and action metadata.",
      mimeType: "application/json",
    },
    async (uri) => {
      const includeAliases = process.env.CIVIL3D_ENABLE_TOOL_ALIASES?.trim().toLowerCase() === "true";
      const defaultTools = MIGRATED_DOMAIN_DEFINITIONS.flatMap((definition) =>
        selectManifestExposures(definition, includeAliases).map((exposure) => exposure.toolName)
      );
      return {
        contents: [{
          uri: uri.href,
          mimeType: "application/json",
          text: JSON.stringify({
            defaultTools,
            totalCatalogEntries: TOOL_CATALOG.length,
            tools: TOOL_CATALOG,
          }, null, 2),
        }],
      };
    },
  );

  server.registerResource(
    "civil3d-safety-guidance",
    "civil3d://docs/safety-and-approval",
    {
      title: "Civil 3D Safety and Approval Guidance",
      description: "Host-safety, approval, and authoritative execution-path guidance.",
      mimeType: "text/markdown",
    },
    async (uri) => ({
      contents: [{ uri: uri.href, mimeType: "text/markdown", text: SAFETY_GUIDANCE }],
    }),
  );

  server.registerResource(
    "civil3d-framework-standards",
    "civil3d://standards/framework",
    {
      title: "Civil 3D Framework Standards",
      description: "Searchable framework standards guidance bundled with the server.",
      mimeType: "application/json",
    },
    async (uri) => ({
      contents: [{
        uri: uri.href,
        mimeType: "application/json",
        text: JSON.stringify(await lookupFrameworkStandards({ maxResults: 20 }), null, 2),
      }],
    }),
  );

  server.registerResource(
    "civil3d-generated-report",
    new ResourceTemplate("civil3d://reports/{reportId}", {
      list: async () => ({
        resources: listReportResources().map((report) => ({
          name: report.name,
          uri: report.uri,
          description: "Retained structured output from a Civil 3D report or export action.",
          mimeType: "application/json",
          size: report.size,
        })),
      }),
      complete: {
        reportId: async () => listReportResources().map((report) => report.id),
      },
    }),
    {
      title: "Civil 3D Generated Report",
      description: "Bounded, short-lived structured results from report and export actions.",
      mimeType: "application/json",
    },
    async (uri, variables) => {
      const reportId = String(variables.reportId ?? "");
      const report = getReportResource(reportId);
      if (!report) {
        throw new Error(`Civil 3D report resource '${reportId}' was not found or has expired.`);
      }
      return {
        contents: [{
          uri: uri.href,
          mimeType: "application/json",
          text: report.text,
        }],
      };
    },
  );
}
