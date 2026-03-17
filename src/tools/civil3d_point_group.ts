import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const GenericResponseSchema = z.object({}).passthrough();

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

// ─── Input shapes ─────────────────────────────────────────────────────────────

// 1. civil3d_point_group_create
const PointGroupCreateInputShape = {
  name: z.string().describe("Name for the new point group"),
  description: z.string().optional().describe("Description of the point group"),
  includeNumbers: z.string().optional().describe("Point number filter pattern to include (e.g. '1-100,200')"),
  excludeNumbers: z.string().optional().describe("Point number filter pattern to exclude"),
  includeDescriptions: z.string().optional().describe("Raw description filter pattern to include (e.g. 'TOP*,BOT*')"),
};

// 2. civil3d_point_group_update
const PointGroupUpdateInputShape = {
  name: z.string().describe("Name of the point group to update"),
  description: z.string().optional().describe("New description"),
  includeNumbers: z.string().optional().describe("Updated point number include pattern"),
  excludeNumbers: z.string().optional().describe("Updated point number exclude pattern"),
  includeDescriptions: z.string().optional().describe("Updated description include pattern"),
};

// 3. civil3d_point_group_delete
const PointGroupDeleteInputShape = {
  name: z.string().describe("Name of the point group to delete"),
};

// 4. civil3d_point_export
const PointExportInputShape = {
  format: z.enum(["pnezd", "penz", "xyzd", "xyz", "csv"])
    .optional()
    .describe("Export format: pnezd (P,N,E,Z,D), xyzd, xyz, csv with name (default: pnezd)"),
  groupName: z.string().optional().describe("Export only points in this group"),
  pointNumbers: z.array(z.number().int().positive()).optional().describe("Export only these specific point numbers"),
  delimiter: z.string().optional().describe("Field delimiter (default: ,)"),
};

// 5. civil3d_point_transform
const PointTransformInputShape = {
  pointNumbers: z.array(z.number().int().positive()).optional().describe("Point numbers to transform (omit with groupName to transform all)"),
  groupName: z.string().optional().describe("Transform all points in this group"),
  translateX: z.number().optional().describe("Translation in X direction"),
  translateY: z.number().optional().describe("Translation in Y direction"),
  translateZ: z.number().optional().describe("Translation in Z direction (elevation)"),
  rotateRadians: z.number().optional().describe("Rotation angle in radians (around origin)"),
  scaleFactor: z.number().optional().describe("Scale factor (default: 1.0, no scale)"),
};

// ─── Registration ─────────────────────────────────────────────────────────────

export function registerCivil3DPointGroupTools(server: McpServer) {

  // 1. civil3d_point_group_create
  server.tool(
    "civil3d_point_group_create",
    "Create a new Civil 3D point group with optional filter criteria. Point groups organize COGO points for display, export, and surface data operations.",
    PointGroupCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(PointGroupCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createPointGroup", {
            name: parsed.name,
            description: parsed.description ?? "",
            includeNumbers: parsed.includeNumbers ?? null,
            excludeNumbers: parsed.excludeNumbers ?? null,
            includeDescriptions: parsed.includeDescriptions ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_point_group_create", error);
      }
    }
  );

  // 2. civil3d_point_group_update
  server.tool(
    "civil3d_point_group_update",
    "Update filter criteria and description of an existing Civil 3D point group.",
    PointGroupUpdateInputShape,
    async (args) => {
      try {
        const parsed = z.object(PointGroupUpdateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("updatePointGroup", {
            name: parsed.name,
            description: parsed.description ?? null,
            includeNumbers: parsed.includeNumbers ?? null,
            excludeNumbers: parsed.excludeNumbers ?? null,
            includeDescriptions: parsed.includeDescriptions ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_point_group_update", error);
      }
    }
  );

  // 3. civil3d_point_group_delete
  server.tool(
    "civil3d_point_group_delete",
    "Delete a Civil 3D point group. This removes the group definition only — the COGO points themselves are not deleted.",
    PointGroupDeleteInputShape,
    async (args) => {
      try {
        const parsed = z.object(PointGroupDeleteInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("deletePointGroup", { name: parsed.name });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_point_group_delete", error);
      }
    }
  );

  // 4. civil3d_point_export
  server.tool(
    "civil3d_point_export",
    "Export Civil 3D COGO points to a text/CSV format string. Supports PNEZD, PENZ, XYZD, XYZ, and CSV formats. Filter by group or specific point numbers.",
    PointExportInputShape,
    async (args) => {
      try {
        const parsed = z.object(PointExportInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("exportCogoPoints", {
            format: parsed.format ?? "pnezd",
            groupName: parsed.groupName ?? null,
            pointNumbers: parsed.pointNumbers ?? null,
            delimiter: parsed.delimiter ?? ",",
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_point_export", error);
      }
    }
  );

  // 5. civil3d_point_transform
  server.tool(
    "civil3d_point_transform",
    "Transform Civil 3D COGO points by translation, rotation, and/or scale. Apply to specific point numbers, a point group, or all points. Rotation is around the drawing origin.",
    PointTransformInputShape,
    async (args) => {
      try {
        const parsed = z.object(PointTransformInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("transformCogoPoints", {
            pointNumbers: parsed.pointNumbers ?? null,
            groupName: parsed.groupName ?? null,
            translateX: parsed.translateX ?? 0,
            translateY: parsed.translateY ?? 0,
            translateZ: parsed.translateZ ?? 0,
            rotateRadians: parsed.rotateRadians ?? 0,
            scaleFactor: parsed.scaleFactor ?? 1.0,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_point_transform", error);
      }
    }
  );
}
