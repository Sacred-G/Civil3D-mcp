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

// 1. civil3d_cogo_inverse
const CogoInverseInputShape = {
  x1: z.number().describe("Easting (X) of start point"),
  y1: z.number().describe("Northing (Y) of start point"),
  x2: z.number().describe("Easting (X) of end point"),
  y2: z.number().describe("Northing (Y) of end point"),
};

// 2. civil3d_cogo_direction_distance
const CogoDirectionDistanceInputShape = {
  fromX: z.number().describe("Starting easting (X)"),
  fromY: z.number().describe("Starting northing (Y)"),
  fromZ: z.number().optional().describe("Starting elevation (Z, default: 0)"),
  bearingDegrees: z.number().describe("Bearing in decimal degrees (clockwise from North)"),
  distance: z.number().positive().describe("Horizontal distance"),
  slope: z.number().optional().describe("Percent slope (optional, applied to calculate dZ)"),
};

// 3. civil3d_cogo_traverse
const CourseSchema = z.object({
  bearingDegrees: z.number().describe("Bearing in decimal degrees (clockwise from North)"),
  distance: z.number().positive().describe("Course length"),
  slope: z.number().optional().describe("Percent slope (optional)"),
  description: z.string().optional().describe("Course description"),
});

const CogoTraverseInputShape = {
  startX: z.number().describe("Starting easting (X)"),
  startY: z.number().describe("Starting northing (Y)"),
  startZ: z.number().optional().describe("Starting elevation (default: 0)"),
  courses: z.array(CourseSchema).min(1).describe("List of bearing/distance courses"),
  isClosed: z.boolean().optional().describe("Whether to compute closure error (default: false)"),
};

// 4. civil3d_cogo_curve_solve
const CogoCurveSolveInputShape = {
  radius: z.number().positive().optional().describe("Curve radius"),
  deltaDegrees: z.number().positive().optional().describe("Central angle (delta) in decimal degrees"),
  length: z.number().positive().optional().describe("Arc length"),
  tangent: z.number().positive().optional().describe("Tangent length"),
  chord: z.number().positive().optional().describe("Chord length"),
};

// 5. civil3d_survey_database_list
const SurveyDatabaseListInputShape = {};

// 6. civil3d_survey_database_create
const SurveyDatabaseCreateInputShape = {
  name: z.string().describe("Name for the new survey database"),
  path: z.string().optional().describe("File system path for the database (optional)"),
};

// 7. civil3d_survey_figure_list
const SurveyFigureListInputShape = {
  databaseName: z.string().optional().describe("Filter to a specific survey database name"),
};

// 8. civil3d_survey_figure_get
const SurveyFigureGetInputShape = {
  name: z.string().describe("Survey figure name"),
  databaseName: z.string().optional().describe("Survey database name (optional, searches all if omitted)"),
};

// ─── Registration ─────────────────────────────────────────────────────────────

export function registerCivil3DCogoTools(server: McpServer) {

  // 1. civil3d_cogo_inverse
  server.tool(
    "civil3d_cogo_inverse",
    "Calculate the bearing and distance (inverse) between two 2D coordinate pairs. Returns bearing in decimal degrees (clockwise from North) and the horizontal distance.",
    CogoInverseInputShape,
    async (args) => {
      try {
        const parsed = z.object(CogoInverseInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("cogoInverse", {
            x1: parsed.x1,
            y1: parsed.y1,
            x2: parsed.x2,
            y2: parsed.y2,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_cogo_inverse", error);
      }
    }
  );

  // 2. civil3d_cogo_direction_distance
  server.tool(
    "civil3d_cogo_direction_distance",
    "Project a new point from a starting coordinate given a bearing (clockwise from North) and horizontal distance. Returns the resulting X, Y, Z coordinates.",
    CogoDirectionDistanceInputShape,
    async (args) => {
      try {
        const parsed = z.object(CogoDirectionDistanceInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("cogoDirectionDistance", {
            fromX: parsed.fromX,
            fromY: parsed.fromY,
            fromZ: parsed.fromZ ?? 0,
            bearingDegrees: parsed.bearingDegrees,
            distance: parsed.distance,
            slope: parsed.slope ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_cogo_direction_distance", error);
      }
    }
  );

  // 3. civil3d_cogo_traverse
  server.tool(
    "civil3d_cogo_traverse",
    "Solve a traverse from a starting point through a series of bearing/distance courses. Returns all computed traverse points and optionally closure error for closed traverses.",
    CogoTraverseInputShape,
    async (args) => {
      try {
        const parsed = z.object(CogoTraverseInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("cogoTraverse", {
            startX: parsed.startX,
            startY: parsed.startY,
            startZ: parsed.startZ ?? 0,
            courses: parsed.courses,
            isClosed: parsed.isClosed ?? false,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_cogo_traverse", error);
      }
    }
  );

  // 4. civil3d_cogo_curve_solve
  server.tool(
    "civil3d_cogo_curve_solve",
    "Solve a horizontal curve given any two curve elements (radius, delta, arc length, tangent, or chord). Returns all five curve elements plus degree of curve.",
    CogoCurveSolveInputShape,
    async (args) => {
      try {
        const parsed = z.object(CogoCurveSolveInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("cogoCurveSolve", {
            radius: parsed.radius ?? null,
            deltaDegrees: parsed.deltaDegrees ?? null,
            length: parsed.length ?? null,
            tangent: parsed.tangent ?? null,
            chord: parsed.chord ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_cogo_curve_solve", error);
      }
    }
  );

  // 5. civil3d_survey_database_list
  server.tool(
    "civil3d_survey_database_list",
    "List all Civil 3D survey databases associated with the current drawing. Survey databases store raw survey observations, figures, and networks.",
    SurveyDatabaseListInputShape,
    async (_args) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listSurveyDatabases", {});
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_database_list", error);
      }
    }
  );

  // 6. civil3d_survey_database_create
  server.tool(
    "civil3d_survey_database_create",
    "Create a new Civil 3D survey database to store survey observations and figures.",
    SurveyDatabaseCreateInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurveyDatabaseCreateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSurveyDatabase", {
            name: parsed.name,
            path: parsed.path ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_database_create", error);
      }
    }
  );

  // 7. civil3d_survey_figure_list
  server.tool(
    "civil3d_survey_figure_list",
    "List all survey figures in Civil 3D survey databases. Survey figures are linework derived from field observations (e.g., toe of slope, top of curb).",
    SurveyFigureListInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurveyFigureListInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("listSurveyFigures", {
            databaseName: parsed.databaseName ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_figure_list", error);
      }
    }
  );

  // 8. civil3d_survey_figure_get
  server.tool(
    "civil3d_survey_figure_get",
    "Get detailed vertex data for a specific Civil 3D survey figure. Returns all 3D vertices along the figure.",
    SurveyFigureGetInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurveyFigureGetInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSurveyFigure", {
            name: parsed.name,
            databaseName: parsed.databaseName ?? null,
          });
        });
        return { content: [{ type: "text", text: JSON.stringify(GenericResponseSchema.parse(response), null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_survey_figure_get", error);
      }
    }
  );
}
