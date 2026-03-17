import { describe, it, expect } from "vitest";
import { z } from "zod";
import { SurfaceVolumeResultSchema, SurfaceStatisticsResultSchema } from "../src/tools/civil3d_surface_analysis.js";

// ─── Schema definitions mirrored from the tool file ────────────────────────

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const SurfaceVolumeCalculateSchema = z.object({
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  method: z.enum(["tin_volume", "average_end_area", "prismoidal"]).optional(),
});

const SurfaceVolumeByRegionSchema = z.object({
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  boundary: z.array(Point2DSchema).min(3),
});

const SurfaceStatisticsGetSchema = z.object({
  name: z.string(),
});

const SurfaceSampleElevationsSchema = z.object({
  name: z.string(),
  method: z.enum(["grid", "points", "transect"]),
  gridSpacing: z.number().positive().optional(),
  points: z.array(Point2DSchema).optional(),
  startPoint: Point2DSchema.optional(),
  endPoint: Point2DSchema.optional(),
  numSamples: z.number().int().min(2).optional(),
  boundary: z.array(Point2DSchema).optional(),
});

const SurfaceAnalyzeSlopeSchema = z.object({
  name: z.string(),
  ranges: z.array(z.object({ min: z.number(), max: z.number() })).optional(),
  numRanges: z.number().int().min(2).max(20).optional(),
});

const SurfaceContourIntervalSetSchema = z.object({
  name: z.string(),
  minorInterval: z.number().positive(),
  majorInterval: z.number().positive(),
});

const SurfaceCreateFromDemSchema = z.object({
  filePath: z.string(),
  name: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  description: z.string().optional(),
  coordinateSystem: z.string().optional(),
});

// ─── civil3d_surface_volume_calculate ──────────────────────────────────────

describe("civil3d_surface_volume_calculate input schema", () => {
  it("accepts minimal valid args", () => {
    const result = SurfaceVolumeCalculateSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
    });
    expect(result.success).toBe(true);
  });

  it("accepts all method options", () => {
    for (const method of ["tin_volume", "average_end_area", "prismoidal"]) {
      const result = SurfaceVolumeCalculateSchema.safeParse({
        baseSurface: "EG",
        comparisonSurface: "FG",
        method,
      });
      expect(result.success).toBe(true);
    }
  });

  it("rejects invalid method", () => {
    const result = SurfaceVolumeCalculateSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
      method: "cross_section",
    });
    expect(result.success).toBe(false);
  });

  it("rejects missing baseSurface", () => {
    const result = SurfaceVolumeCalculateSchema.safeParse({
      comparisonSurface: "FG",
    });
    expect(result.success).toBe(false);
  });

  it("rejects missing comparisonSurface", () => {
    const result = SurfaceVolumeCalculateSchema.safeParse({
      baseSurface: "EG",
    });
    expect(result.success).toBe(false);
  });
});

describe("SurfaceVolumeResultSchema response validation", () => {
  const validResult = {
    baseSurface: "EG",
    comparisonSurface: "FG",
    cutVolume: 5200.5,
    fillVolume: 3100.25,
    netVolume: -2100.25,
    cutArea: 10000.0,
    fillArea: 8500.0,
    method: "tin_volume",
    units: { volume: "Foot^3", area: "Foot^2" },
  };

  it("validates a correct volume result", () => {
    expect(SurfaceVolumeResultSchema.safeParse(validResult).success).toBe(true);
  });

  it("rejects missing cutVolume", () => {
    const { cutVolume, ...rest } = validResult;
    expect(SurfaceVolumeResultSchema.safeParse(rest).success).toBe(false);
  });

  it("rejects missing units", () => {
    const { units, ...rest } = validResult;
    expect(SurfaceVolumeResultSchema.safeParse(rest).success).toBe(false);
  });

  it("rejects non-numeric fillVolume", () => {
    const result = SurfaceVolumeResultSchema.safeParse({ ...validResult, fillVolume: "lots" });
    expect(result.success).toBe(false);
  });

  it("accepts zero volumes (flat surfaces)", () => {
    const result = SurfaceVolumeResultSchema.safeParse({
      ...validResult,
      cutVolume: 0,
      fillVolume: 0,
      netVolume: 0,
    });
    expect(result.success).toBe(true);
  });

  it("accepts negative netVolume (fill-dominant)", () => {
    const result = SurfaceVolumeResultSchema.safeParse({
      ...validResult,
      netVolume: -1500.0,
    });
    expect(result.success).toBe(true);
  });
});

// ─── civil3d_surface_statistics_get ────────────────────────────────────────

describe("civil3d_surface_statistics_get input schema", () => {
  it("accepts a valid surface name", () => {
    expect(SurfaceStatisticsGetSchema.safeParse({ name: "EG" }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(SurfaceStatisticsGetSchema.safeParse({}).success).toBe(false);
  });

  it("rejects non-string name", () => {
    expect(SurfaceStatisticsGetSchema.safeParse({ name: 42 }).success).toBe(false);
  });
});

describe("SurfaceStatisticsResultSchema response validation", () => {
  const validStats = {
    surfaceName: "EG",
    minimumElevation: 95.5,
    maximumElevation: 145.2,
    meanElevation: 120.35,
    area2d: 50000.0,
    area3d: 52000.0,
    numberOfPoints: 1250,
    numberOfTriangles: 2490,
    units: {
      horizontal: "Foot",
      vertical: "Foot",
      area: "Foot^2",
    },
  };

  it("validates a correct statistics result", () => {
    expect(SurfaceStatisticsResultSchema.safeParse(validStats).success).toBe(true);
  });

  it("rejects missing surfaceName", () => {
    const { surfaceName, ...rest } = validStats;
    expect(SurfaceStatisticsResultSchema.safeParse(rest).success).toBe(false);
  });

  it("rejects missing units", () => {
    const { units, ...rest } = validStats;
    expect(SurfaceStatisticsResultSchema.safeParse(rest).success).toBe(false);
  });

  it("rejects non-numeric numberOfPoints", () => {
    expect(SurfaceStatisticsResultSchema.safeParse({ ...validStats, numberOfPoints: "many" }).success).toBe(false);
  });

  it("accepts zero-point surface", () => {
    const result = SurfaceStatisticsResultSchema.safeParse({
      ...validStats,
      numberOfPoints: 0,
      numberOfTriangles: 0,
      area2d: 0,
      area3d: 0,
    });
    expect(result.success).toBe(true);
  });

  it("validates units structure", () => {
    const result = SurfaceStatisticsResultSchema.safeParse({
      ...validStats,
      units: { horizontal: "Meter", vertical: "Meter", area: "Meter^2" },
    });
    expect(result.success).toBe(true);
  });
});

// ─── civil3d_surface_volume_by_region ──────────────────────────────────────

describe("civil3d_surface_volume_by_region input schema", () => {
  const validBoundary = [
    { x: 0, y: 0 },
    { x: 100, y: 0 },
    { x: 100, y: 100 },
    { x: 0, y: 100 },
  ];

  it("accepts valid region args", () => {
    expect(SurfaceVolumeByRegionSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
      boundary: validBoundary,
    }).success).toBe(true);
  });

  it("rejects boundary with fewer than 3 points", () => {
    expect(SurfaceVolumeByRegionSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
      boundary: [{ x: 0, y: 0 }, { x: 100, y: 0 }],
    }).success).toBe(false);
  });

  it("rejects missing boundary", () => {
    expect(SurfaceVolumeByRegionSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
    }).success).toBe(false);
  });
});

// ─── civil3d_surface_sample_elevations ─────────────────────────────────────

describe("civil3d_surface_sample_elevations input schema", () => {
  it("accepts grid method with spacing", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "grid",
      gridSpacing: 10,
    }).success).toBe(true);
  });

  it("accepts points method with points array", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "points",
      points: [{ x: 100, y: 200 }, { x: 150, y: 250 }],
    }).success).toBe(true);
  });

  it("accepts transect method with start/end", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "transect",
      startPoint: { x: 0, y: 0 },
      endPoint: { x: 500, y: 0 },
      numSamples: 25,
    }).success).toBe(true);
  });

  it("rejects unknown method", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "random",
    }).success).toBe(false);
  });

  it("rejects negative gridSpacing", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "grid",
      gridSpacing: -5,
    }).success).toBe(false);
  });

  it("rejects numSamples < 2", () => {
    expect(SurfaceSampleElevationsSchema.safeParse({
      name: "EG",
      method: "transect",
      startPoint: { x: 0, y: 0 },
      endPoint: { x: 500, y: 0 },
      numSamples: 1,
    }).success).toBe(false);
  });
});

// ─── civil3d_surface_analyze_slope ─────────────────────────────────────────

describe("civil3d_surface_analyze_slope input schema", () => {
  it("accepts name-only (uses defaults)", () => {
    expect(SurfaceAnalyzeSlopeSchema.safeParse({ name: "EG" }).success).toBe(true);
  });

  it("accepts custom ranges", () => {
    expect(SurfaceAnalyzeSlopeSchema.safeParse({
      name: "EG",
      ranges: [{ min: 0, max: 5 }, { min: 5, max: 15 }, { min: 15, max: 100 }],
    }).success).toBe(true);
  });

  it("accepts numRanges", () => {
    expect(SurfaceAnalyzeSlopeSchema.safeParse({ name: "EG", numRanges: 10 }).success).toBe(true);
  });

  it("rejects numRanges = 1", () => {
    expect(SurfaceAnalyzeSlopeSchema.safeParse({ name: "EG", numRanges: 1 }).success).toBe(false);
  });

  it("rejects numRanges > 20", () => {
    expect(SurfaceAnalyzeSlopeSchema.safeParse({ name: "EG", numRanges: 25 }).success).toBe(false);
  });
});

// ─── civil3d_surface_contour_interval_set ──────────────────────────────────

describe("civil3d_surface_contour_interval_set input schema", () => {
  it("accepts valid intervals", () => {
    expect(SurfaceContourIntervalSetSchema.safeParse({
      name: "EG",
      minorInterval: 1.0,
      majorInterval: 5.0,
    }).success).toBe(true);
  });

  it("rejects zero minorInterval", () => {
    expect(SurfaceContourIntervalSetSchema.safeParse({
      name: "EG",
      minorInterval: 0,
      majorInterval: 5.0,
    }).success).toBe(false);
  });

  it("rejects negative majorInterval", () => {
    expect(SurfaceContourIntervalSetSchema.safeParse({
      name: "EG",
      minorInterval: 1.0,
      majorInterval: -5.0,
    }).success).toBe(false);
  });

  it("rejects missing name", () => {
    expect(SurfaceContourIntervalSetSchema.safeParse({
      minorInterval: 1.0,
      majorInterval: 5.0,
    }).success).toBe(false);
  });
});

// ─── civil3d_surface_create_from_dem ───────────────────────────────────────

describe("civil3d_surface_create_from_dem input schema", () => {
  it("accepts minimal args", () => {
    expect(SurfaceCreateFromDemSchema.safeParse({
      filePath: "C:/data/terrain.dem",
      name: "DEM Surface",
    }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(SurfaceCreateFromDemSchema.safeParse({
      filePath: "C:/data/terrain.tif",
      name: "DEM Surface",
      style: "Standard",
      layer: "C-TOPO",
      description: "Imported from LiDAR",
      coordinateSystem: "UTM83-15",
    }).success).toBe(true);
  });

  it("rejects missing filePath", () => {
    expect(SurfaceCreateFromDemSchema.safeParse({ name: "DEM Surface" }).success).toBe(false);
  });

  it("rejects missing name", () => {
    expect(SurfaceCreateFromDemSchema.safeParse({ filePath: "C:/data/terrain.dem" }).success).toBe(false);
  });
});
