import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_intersection_list ───────────────────────────────────────────

export const IntersectionListInputSchema = z.object({
  siteName: z.string().optional().describe("Filter intersections by site name (omit for all intersections in the drawing)"),
});

// ─── 2. civil3d_intersection_create ─────────────────────────────────────────

export const IntersectionCreateInputSchema = z.object({
  name: z.string().describe("Name for the new intersection"),
  mainRoadAlignment: z.string().describe("Name of the main (primary) road alignment"),
  mainRoadProfile: z.string().describe("Name of the finished ground profile on the main road alignment"),
  intersectingRoadAlignment: z.string().describe("Name of the intersecting (secondary) road alignment"),
  intersectingRoadProfile: z.string().describe("Name of the finished ground profile on the intersecting road alignment"),
  intersectionPoint: z.tuple([z.number(), z.number()]).optional().describe("X, Y coordinates of the intersection point. If omitted, Civil 3D will compute it from the alignment geometries."),
  siteName: z.string().optional().describe("Name of the site to add the intersection to (omit to use the default site)"),
  offsetDistance: z.number().nonnegative().optional().describe("Standard offset distance from the intersection centerline to the edge of pavement in drawing units"),
  curveRadius: z.number().nonnegative().optional().describe("Curb return radius at intersection corners in drawing units"),
  createCorridors: z.boolean().optional().describe("Automatically create corridor(s) for the intersection (default: false)"),
  mainRoadAssembly: z.string().optional().describe("Assembly name for the main road corridor lanes (required if createCorridors is true)"),
  intersectingRoadAssembly: z.string().optional().describe("Assembly name for the intersecting road corridor lanes (required if createCorridors is true)"),
});

// ─── 3. civil3d_intersection_get ────────────────────────────────────────────

export const IntersectionGetInputSchema = z.object({
  name: z.string().describe("Name of the intersection to retrieve"),
  includeCorridorInfo: z.boolean().optional().describe("Include associated corridor information in the response (default: false)"),
  includeCurbReturns: z.boolean().optional().describe("Include curb return alignment and profile details (default: false)"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DIntersectionTools(server: McpServer) {

  // 1. civil3d_intersection_list
  server.tool(
    "civil3d_intersection_list",
    "List all Civil 3D intersections in the active drawing. Returns name, main road alignment, intersecting road alignment, intersection point coordinates, and site membership for each intersection.",
    IntersectionListInputSchema.shape,
    async (args) => {
      try {
        const parsed = IntersectionListInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listIntersections", {
            siteName: parsed.siteName ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_intersection_list", error);
      }
    }
  );

  // 2. civil3d_intersection_create
  server.tool(
    "civil3d_intersection_create",
    "Create a Civil 3D intersection between two road alignments. Computes or uses the supplied intersection point, applies curb return radii, and optionally creates corridor(s) for the intersection legs. Both alignments must have finished ground profiles.",
    IntersectionCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = IntersectionCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createIntersection", {
            name: parsed.name,
            mainRoadAlignment: parsed.mainRoadAlignment,
            mainRoadProfile: parsed.mainRoadProfile,
            intersectingRoadAlignment: parsed.intersectingRoadAlignment,
            intersectingRoadProfile: parsed.intersectingRoadProfile,
            intersectionX: parsed.intersectionPoint ? parsed.intersectionPoint[0] : null,
            intersectionY: parsed.intersectionPoint ? parsed.intersectionPoint[1] : null,
            siteName: parsed.siteName ?? null,
            offsetDistance: parsed.offsetDistance ?? null,
            curveRadius: parsed.curveRadius ?? null,
            createCorridors: parsed.createCorridors ?? false,
            mainRoadAssembly: parsed.mainRoadAssembly ?? null,
            intersectingRoadAssembly: parsed.intersectingRoadAssembly ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_intersection_create", error);
      }
    }
  );

  // 3. civil3d_intersection_get
  server.tool(
    "civil3d_intersection_get",
    "Get detailed properties of a Civil 3D intersection. Returns the main and intersecting road alignments, intersection point, offset distances, curb return configurations, and optionally corridor and curb return details.",
    IntersectionGetInputSchema.shape,
    async (args) => {
      try {
        const parsed = IntersectionGetInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getIntersection", {
            name: parsed.name,
            includeCorridorInfo: parsed.includeCorridorInfo ?? false,
            includeCurbReturns: parsed.includeCurbReturns ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_intersection_get", error);
      }
    }
  );
}
