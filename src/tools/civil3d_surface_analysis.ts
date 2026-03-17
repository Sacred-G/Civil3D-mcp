import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

// ─── Shared response schemas ───────────────────────────────────────────────

export const SurfaceVolumeResultSchema = z.object({
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  cutVolume: z.number(),
  fillVolume: z.number(),
  netVolume: z.number(),
  cutArea: z.number(),
  fillArea: z.number(),
  method: z.string(),
  units: z.object({
    volume: z.string(),
    area: z.string(),
  }),
});

export const SurfaceStatisticsResultSchema = z.object({
  surfaceName: z.string(),
  minimumElevation: z.number(),
  maximumElevation: z.number(),
  meanElevation: z.number(),
  area2d: z.number(),
  area3d: z.number(),
  numberOfPoints: z.number(),
  numberOfTriangles: z.number(),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
    area: z.string(),
  }),
});

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_surface_volume_calculate ──────────────────────────────────

const SurfaceVolumeCalculateInputShape = {
  baseSurface: z.string().describe("Name of the base (existing ground) surface"),
  comparisonSurface: z.string().describe("Name of the comparison (design) surface"),
  method: z.enum(["tin_volume", "average_end_area", "prismoidal"]).optional().describe("Volume calculation method (default: tin_volume)"),
};

// ─── 2. civil3d_surface_volume_report ─────────────────────────────────────

const SurfaceVolumeReportInputShape = {
  baseSurface: z.string().describe("Name of the base surface"),
  comparisonSurface: z.string().describe("Name of the comparison surface"),
  format: z.enum(["summary", "detailed"]).optional().describe("Report format (default: summary)"),
};

// ─── 3. civil3d_surface_volume_by_region ──────────────────────────────────

const SurfaceVolumeByRegionInputShape = {
  baseSurface: z.string().describe("Name of the base surface"),
  comparisonSurface: z.string().describe("Name of the comparison surface"),
  boundary: z.array(Point2DSchema).min(3).describe("Polygon boundary defining the region (minimum 3 points)"),
};

// ─── 4. civil3d_surface_analyze_slope ─────────────────────────────────────

const SurfaceAnalyzeSlopeInputShape = {
  name: z.string().describe("Surface name"),
  ranges: z.array(z.object({ min: z.number(), max: z.number() })).optional().describe("Custom slope ranges in percent (e.g. [{min:0,max:5},{min:5,max:15}])"),
  numRanges: z.number().int().min(2).max(20).optional().describe("Number of auto-generated equal ranges (used when ranges not provided, default: 5)"),
};

// ─── 5. civil3d_surface_analyze_elevation ─────────────────────────────────

const SurfaceAnalyzeElevationInputShape = {
  name: z.string().describe("Surface name"),
  ranges: z.array(z.object({ min: z.number(), max: z.number() })).optional().describe("Custom elevation ranges in drawing units"),
  numRanges: z.number().int().min(2).max(20).optional().describe("Number of auto-generated equal ranges (default: 5)"),
};

// ─── 6. civil3d_surface_analyze_directions ────────────────────────────────

const SurfaceAnalyzeDirectionsInputShape = {
  name: z.string().describe("Surface name"),
  numRanges: z.number().int().min(4).max(16).optional().describe("Number of directional sectors (4 or 8 or 16, default: 8)"),
};

// ─── 7. civil3d_surface_watershed_add ─────────────────────────────────────

const SurfaceWatershedAddInputShape = {
  name: z.string().describe("Surface name"),
  depthThreshold: z.number().positive().optional().describe("Minimum depression depth to create a watershed (drawing units, default: 0.1)"),
  mergeAdjacentWatersheds: z.boolean().optional().describe("Merge small adjacent watersheds (default: false)"),
};

// ─── 8. civil3d_surface_contour_interval_set ──────────────────────────────

const SurfaceContourIntervalSetInputShape = {
  name: z.string().describe("Surface name"),
  minorInterval: z.number().positive().describe("Minor contour interval in drawing units"),
  majorInterval: z.number().positive().describe("Major contour interval in drawing units"),
};

// ─── 9. civil3d_surface_statistics_get ────────────────────────────────────

const SurfaceStatisticsGetInputShape = {
  name: z.string().describe("Surface name"),
};

// ─── 10. civil3d_surface_sample_elevations ────────────────────────────────

const SurfaceSampleElevationsInputShape = {
  name: z.string().describe("Surface name"),
  method: z.enum(["grid", "points", "transect"]).describe("Sampling method"),
  gridSpacing: z.number().positive().optional().describe("Grid spacing in drawing units (required for method=grid)"),
  points: z.array(Point2DSchema).optional().describe("Sample points (required for method=points)"),
  startPoint: Point2DSchema.optional().describe("Transect start point (required for method=transect)"),
  endPoint: Point2DSchema.optional().describe("Transect end point (required for method=transect)"),
  numSamples: z.number().int().min(2).optional().describe("Number of samples along transect (default: 50)"),
  boundary: z.array(Point2DSchema).optional().describe("Optional boundary polygon to clip grid sampling"),
};

// ─── 11. civil3d_surface_create_from_dem ──────────────────────────────────

const SurfaceCreateFromDemInputShape = {
  filePath: z.string().describe("Full path to the DEM file (.dem, .tif, .asc, etc.)"),
  name: z.string().describe("Name for the created surface"),
  style: z.string().optional().describe("Surface style name"),
  layer: z.string().optional().describe("Layer name for the surface"),
  description: z.string().optional().describe("Surface description"),
  coordinateSystem: z.string().optional().describe("Coordinate system code (e.g. 'UTM83-15')"),
};

// ─── Registration ──────────────────────────────────────────────────────────

export function registerCivil3DSurfaceAnalysisTools(server: McpServer) {

  // 1. civil3d_surface_volume_calculate
  server.tool(
    "civil3d_surface_volume_calculate",
    "Calculate cut/fill volumes between two Civil 3D surfaces. Returns cut volume, fill volume, net volume, and areas.",
    SurfaceVolumeCalculateInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceVolumeCalculateInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateSurfaceVolume", {
            baseSurface: parsed.baseSurface,
            comparisonSurface: parsed.comparisonSurface,
            method: parsed.method ?? "tin_volume",
          });
        });
        const validated = SurfaceVolumeResultSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_volume_calculate", error);
      }
    }
  );

  // 2. civil3d_surface_volume_report
  server.tool(
    "civil3d_surface_volume_report",
    "Generate a formatted volume report comparing two Civil 3D surfaces. Returns a human-readable report with cut/fill summary.",
    SurfaceVolumeReportInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceVolumeReportInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSurfaceVolumeReport", {
            baseSurface: parsed.baseSurface,
            comparisonSurface: parsed.comparisonSurface,
            format: parsed.format ?? "summary",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_volume_report", error);
      }
    }
  );

  // 3. civil3d_surface_volume_by_region
  server.tool(
    "civil3d_surface_volume_by_region",
    "Calculate cut/fill volumes between two surfaces within a specific polygon region boundary.",
    SurfaceVolumeByRegionInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceVolumeByRegionInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateSurfaceVolumeByRegion", {
            baseSurface: parsed.baseSurface,
            comparisonSurface: parsed.comparisonSurface,
            boundary: parsed.boundary,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_volume_by_region", error);
      }
    }
  );

  // 4. civil3d_surface_analyze_slope
  server.tool(
    "civil3d_surface_analyze_slope",
    "Analyze slope distribution across a Civil 3D surface. Returns area and percentage for each slope range.",
    SurfaceAnalyzeSlopeInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceAnalyzeSlopeInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("analyzeSurfaceSlope", {
            name: parsed.name,
            ranges: parsed.ranges ?? null,
            numRanges: parsed.numRanges ?? 5,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_analyze_slope", error);
      }
    }
  );

  // 5. civil3d_surface_analyze_elevation
  server.tool(
    "civil3d_surface_analyze_elevation",
    "Analyze elevation band distribution across a Civil 3D surface. Returns area and percentage for each elevation range.",
    SurfaceAnalyzeElevationInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceAnalyzeElevationInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("analyzeSurfaceElevation", {
            name: parsed.name,
            ranges: parsed.ranges ?? null,
            numRanges: parsed.numRanges ?? 5,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_analyze_elevation", error);
      }
    }
  );

  // 6. civil3d_surface_analyze_directions
  server.tool(
    "civil3d_surface_analyze_directions",
    "Analyze surface aspect/facing direction distribution. Returns area breakdown by cardinal/intercardinal direction sectors.",
    SurfaceAnalyzeDirectionsInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceAnalyzeDirectionsInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("analyzeSurfaceDirections", {
            name: parsed.name,
            numRanges: parsed.numRanges ?? 8,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_analyze_directions", error);
      }
    }
  );

  // 7. civil3d_surface_watershed_add
  server.tool(
    "civil3d_surface_watershed_add",
    "Add watershed analysis to a Civil 3D surface. Identifies drainage basins and flow paths.",
    SurfaceWatershedAddInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceWatershedAddInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("addSurfaceWatersheds", {
            name: parsed.name,
            depthThreshold: parsed.depthThreshold ?? 0.1,
            mergeAdjacentWatersheds: parsed.mergeAdjacentWatersheds ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_watershed_add", error);
      }
    }
  );

  // 8. civil3d_surface_contour_interval_set
  server.tool(
    "civil3d_surface_contour_interval_set",
    "Set the minor and major contour display intervals for a Civil 3D surface.",
    SurfaceContourIntervalSetInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceContourIntervalSetInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("setSurfaceContourInterval", {
            name: parsed.name,
            minorInterval: parsed.minorInterval,
            majorInterval: parsed.majorInterval,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_contour_interval_set", error);
      }
    }
  );

  // 9. civil3d_surface_statistics_get
  server.tool(
    "civil3d_surface_statistics_get",
    "Get comprehensive statistics for a Civil 3D surface including elevation range, area, point count, and triangle count.",
    SurfaceStatisticsGetInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceStatisticsGetInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("getSurfaceStatisticsDetailed", {
            name: parsed.name,
          });
        });
        const validated = SurfaceStatisticsResultSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_statistics_get", error);
      }
    }
  );

  // 10. civil3d_surface_sample_elevations
  server.tool(
    "civil3d_surface_sample_elevations",
    "Sample surface elevations at grid points, discrete points, or along a transect line.",
    SurfaceSampleElevationsInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceSampleElevationsInputShape).parse(args);
        if (parsed.method === "grid" && parsed.gridSpacing == null) {
          throw new Error("gridSpacing is required when method is 'grid'");
        }
        if (parsed.method === "points" && (parsed.points == null || parsed.points.length === 0)) {
          throw new Error("points array is required when method is 'points'");
        }
        if (parsed.method === "transect" && (parsed.startPoint == null || parsed.endPoint == null)) {
          throw new Error("startPoint and endPoint are required when method is 'transect'");
        }
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("sampleSurfaceElevations", {
            name: parsed.name,
            method: parsed.method,
            gridSpacing: parsed.gridSpacing,
            points: parsed.points,
            startPoint: parsed.startPoint,
            endPoint: parsed.endPoint,
            numSamples: parsed.numSamples ?? 50,
            boundary: parsed.boundary,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_sample_elevations", error);
      }
    }
  );

  // 11. civil3d_surface_create_from_dem
  server.tool(
    "civil3d_surface_create_from_dem",
    "Create a Civil 3D TIN surface by importing a DEM file (.dem, .tif, .asc, .flt, etc.).",
    SurfaceCreateFromDemInputShape,
    async (args) => {
      try {
        const parsed = z.object(SurfaceCreateFromDemInputShape).parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createSurfaceFromDem", {
            filePath: parsed.filePath,
            name: parsed.name,
            style: parsed.style,
            layer: parsed.layer,
            description: parsed.description,
            coordinateSystem: parsed.coordinateSystem,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_surface_create_from_dem", error);
      }
    }
  );
}

function errorResult(toolName: string, error: unknown) {
  let message = `Failed to execute ${toolName}`;
  if (error instanceof Error) {
    message += `: ${error.message}`;
  } else if (typeof error === "string") {
    message += `: ${error}`;
  }
  console.error(`Error in ${toolName}:`, error);
  return {
    content: [{ type: "text" as const, text: message }],
    isError: true,
  };
}
