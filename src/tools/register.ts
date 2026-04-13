import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { captureToolHandler } from "./toolHandlerRegistry.js";
import { registerManifestTools } from "./toolManifest.js";

export async function registerTools(server: McpServer) {
  // Intercept server.tool() to capture every handler in the global registry.
  // This allows the HTTP bridge to invoke any registered tool by name,
  // without maintaining a separate hardcoded switch statement.
  const originalTool = server.tool.bind(server);
  (server as any).tool = function (...args: unknown[]) {
    if (args.length >= 2) {
      const name = args[0];
      const handler = args[args.length - 1];
      if (typeof name === "string" && typeof handler === "function") {
        captureToolHandler(name, handler as Parameters<typeof captureToolHandler>[1]);
      }
    }
    return (originalTool as Function).apply(server, args);
  };

  // Manifest-driven domains: alignment, surface, profile, corridor, section, pipe, assembly, point, grading, parcel, survey, plan_production, project, standards, qc, hydrology, quantity_takeoff, sight_distance, detention, slope_analysis, cost_estimation, geometry, drawing, coordinate_system, job, plugin, docs.
  // Add new migrated domains to toolManifest.ts, not here.
  registerManifestTools(server);
}
