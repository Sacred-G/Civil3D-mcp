import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ─── Shared schemas ──────────────────────────────────────────────────────────

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().default(0),
});

const GenericResponseSchema = z.object({}).passthrough();

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

// ─── Input shapes ────────────────────────────────────────────────────────────

// 1. civil3d_grading_group_list
const GradingGroupListInputShape = {};

// 2. civil3d_grading_group_get
const GradingGroupGetInputShape = {
  name: z.string().describe("Grading group name"),
};

// 3. civil3d_grading_group_create
const GradingGroupCreateInputShape = {
  name: z.string().describe("Name for the new grading group"),
  description: z.string().optional().describe("Description"),
  useProjection: z.boolean().optional().describe("Whether to use projection for grading (default: false)"),
};

// 4. civil3d_grading_group_delete
const GradingGroupDeleteInputShape = {
  name: z.string().describe("Name of the grading group to delete"),
};

// 5. civil3d_grading_group_volume
const GradingGroupVolumeInputShape = {
  name: z.string().describe("Grading group name"),
};

// 6. civil3d_grading_group_surface_create
const GradingGroupSurfaceCreateInputShape = {
  name: z.string().describe("Grading group name"),
  surfaceName: z.string().optional().describe("Name for the created surface (optional)"),
};

// 7. civil3d_grading_list
const GradingListInputShape = {
  groupName: z.string().describe("Grading group name"),
};

// 8. civil3d_grading_get
const GradingGetInputShape = {
  groupName: z.string().describe("Grading group name"),
  handle: z.string().describe("Grading object handle"),
};

// 9. civil3d_grading_create
const GradingCreateInputShape = {
  groupName: z.string().describe("Grading group name to add the grading to"),
  featureLineName: z.string().describe("Name of the feature line to grade from"),
  criteriaName: z.string().optional().describe("Grading criteria name (optional)"),
  side: z.enum(["left", "right", "both"]).optional().describe("Side to grade (default: right)"),
};

// 10. civil3d_grading_delete
const GradingDeleteInputShape = {
  groupName: z.string().describe("Grading group name"),
  handle: z.string().describe("Handle of the grading object to delete"),
};

// 11. civil3d_grading_criteria_list
const GradingCriteriaListInputShape = {};

// 12. civil3d_feature_line_create
const FeatureLineCreateInputShape = {
  points: z.array(Point3DSchema).min(2).describe("Ordered 3D vertices of the feature line (minimum 2)"),
  name: z.string().optional().describe("Name for the feature line (optional)"),
  layer: z.string().optional().describe("AutoCAD layer name (default: 0)"),
};

// ─── Registration ─────────────────────────────────────────────────────────────

export function registerCivil3DGradingTools(server: McpServer) {

  // 1. civil3d_grading_group_list
  server.tool(
    "civil3d_grading_group_list",
    "List all Civil 3D grading groups in the drawing. Returns name, handle, grading count, and validity status.",
    GradingGroupListInputShape,
    async (_args) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listGradingGroups", {});
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_list", error);
      }
    }
  );

  // 2. civil3d_grading_group_get
  server.tool(
    "civil3d_grading_group_get",
    "Get detailed information about a Civil 3D grading group including cut/fill volumes and all contained gradings.",
    GradingGroupGetInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGroupGetInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getGradingGroup", { name: parsed.name });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_get", error);
      }
    }
  );

  // 3. civil3d_grading_group_create
  server.tool(
    "civil3d_grading_group_create",
    "Create a new Civil 3D grading group. Grading groups are containers for grading objects within a site.",
    GradingGroupCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGroupCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createGradingGroup", {
            name: parsed.name,
            description: parsed.description ?? "",
            useProjection: parsed.useProjection ?? false,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_create", error);
      }
    }
  );

  // 4. civil3d_grading_group_delete
  server.tool(
    "civil3d_grading_group_delete",
    "Delete a Civil 3D grading group and all its contained gradings.",
    GradingGroupDeleteInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGroupDeleteInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("deleteGradingGroup", { name: parsed.name });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_delete", error);
      }
    }
  );

  // 5. civil3d_grading_group_volume
  server.tool(
    "civil3d_grading_group_volume",
    "Get cut and fill volume report for a Civil 3D grading group. Returns cut volume, fill volume, and net volume.",
    GradingGroupVolumeInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGroupVolumeInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getGradingGroupVolume", { name: parsed.name });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_volume", error);
      }
    }
  );

  // 6. civil3d_grading_group_surface_create
  server.tool(
    "civil3d_grading_group_surface_create",
    "Create a Civil 3D surface from a grading group. The surface will represent the graded terrain.",
    GradingGroupSurfaceCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGroupSurfaceCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSurfaceFromGradingGroup", {
            name: parsed.name,
            surfaceName: parsed.surfaceName ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_group_surface_create", error);
      }
    }
  );

  // 7. civil3d_grading_list
  server.tool(
    "civil3d_grading_list",
    "List all grading objects within a specific Civil 3D grading group.",
    GradingListInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingListInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listGradings", { groupName: parsed.groupName });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_list", error);
      }
    }
  );

  // 8. civil3d_grading_get
  server.tool(
    "civil3d_grading_get",
    "Get detailed properties of a specific grading object including criteria, side, and volumes.",
    GradingGetInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingGetInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getGrading", {
            groupName: parsed.groupName,
            handle: parsed.handle,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_get", error);
      }
    }
  );

  // 9. civil3d_grading_create
  server.tool(
    "civil3d_grading_create",
    "Create a new Civil 3D grading from a feature line. The grading will project from the feature line using the specified criteria.",
    GradingCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createGrading", {
            groupName: parsed.groupName,
            featureLineName: parsed.featureLineName,
            criteriaName: parsed.criteriaName ?? null,
            side: parsed.side ?? "right",
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_create", error);
      }
    }
  );

  // 10. civil3d_grading_delete
  server.tool(
    "civil3d_grading_delete",
    "Delete a grading object from a Civil 3D grading group by its handle.",
    GradingDeleteInputShape,
    async (args) => {
      try {
        const parsed = z.object(GradingDeleteInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("deleteGrading", {
            groupName: parsed.groupName,
            handle: parsed.handle,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_delete", error);
      }
    }
  );

  // 11. civil3d_grading_criteria_list
  server.tool(
    "civil3d_grading_criteria_list",
    "List all available Civil 3D grading criteria sets and their individual criteria. Use criteria names when creating gradings.",
    GradingCriteriaListInputShape,
    async (_args) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listGradingCriteria", {});
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_grading_criteria_list", error);
      }
    }
  );

  // 12. civil3d_feature_line_create
  server.tool(
    "civil3d_feature_line_create",
    "Create a new Civil 3D feature line from an ordered list of 3D points. Feature lines are used as grading footprints and site design baselines.",
    FeatureLineCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(FeatureLineCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createFeatureLine", {
            points: parsed.points,
            name: parsed.name ?? null,
            layer: parsed.layer ?? "0",
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_feature_line_create", error);
      }
    }
  );
}
