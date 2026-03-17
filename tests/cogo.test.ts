import { describe, it, expect } from "vitest";
import { z } from "zod";

// ─── Input schemas (mirrored from civil3d_cogo.ts and civil3d_point_group.ts) ─

// Point group schemas
const PointGroupCreateSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  includeNumbers: z.string().optional(),
  excludeNumbers: z.string().optional(),
  includeDescriptions: z.string().optional(),
});

const PointGroupUpdateSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  includeNumbers: z.string().optional(),
  excludeNumbers: z.string().optional(),
  includeDescriptions: z.string().optional(),
});

const PointGroupDeleteSchema = z.object({ name: z.string() });

const PointExportSchema = z.object({
  format: z.enum(["pnezd", "penz", "xyzd", "xyz", "csv"]).optional(),
  groupName: z.string().optional(),
  pointNumbers: z.array(z.number().int().positive()).optional(),
  delimiter: z.string().optional(),
});

const PointTransformSchema = z.object({
  pointNumbers: z.array(z.number().int().positive()).optional(),
  groupName: z.string().optional(),
  translateX: z.number().optional(),
  translateY: z.number().optional(),
  translateZ: z.number().optional(),
  rotateRadians: z.number().optional(),
  scaleFactor: z.number().optional(),
});

// COGO schemas
const CogoInverseSchema = z.object({
  x1: z.number(),
  y1: z.number(),
  x2: z.number(),
  y2: z.number(),
});

const CogoDirectionDistanceSchema = z.object({
  fromX: z.number(),
  fromY: z.number(),
  fromZ: z.number().optional(),
  bearingDegrees: z.number(),
  distance: z.number().positive(),
  slope: z.number().optional(),
});

const CourseSchema = z.object({
  bearingDegrees: z.number(),
  distance: z.number().positive(),
  slope: z.number().optional(),
  description: z.string().optional(),
});

const CogoTraverseSchema = z.object({
  startX: z.number(),
  startY: z.number(),
  startZ: z.number().optional(),
  courses: z.array(CourseSchema).min(1),
  isClosed: z.boolean().optional(),
});

const CogoCurveSolveSchema = z.object({
  radius: z.number().positive().optional(),
  deltaDegrees: z.number().positive().optional(),
  length: z.number().positive().optional(),
  tangent: z.number().positive().optional(),
  chord: z.number().positive().optional(),
});

const SurveyDatabaseCreateSchema = z.object({
  name: z.string(),
  path: z.string().optional(),
});

const SurveyFigureListSchema = z.object({
  databaseName: z.string().optional(),
});

const SurveyFigureGetSchema = z.object({
  name: z.string(),
  databaseName: z.string().optional(),
});

// ─── Point group tests ────────────────────────────────────────────────────────

describe("civil3d_point_group_create input schema", () => {
  it("accepts minimal args (name only)", () => {
    expect(PointGroupCreateSchema.safeParse({ name: "Topo Points" }).success).toBe(true);
  });

  it("accepts all optional filter fields", () => {
    expect(PointGroupCreateSchema.safeParse({
      name: "Spot Shots",
      description: "Field topo shots",
      includeNumbers: "1-500",
      excludeNumbers: "101-110",
      includeDescriptions: "TOPO*",
    }).success).toBe(true);
  });

  it("rejects empty object", () => {
    expect(PointGroupCreateSchema.safeParse({}).success).toBe(false);
  });
});

describe("civil3d_point_group_update input schema", () => {
  it("accepts name only (all fields optional except name)", () => {
    expect(PointGroupUpdateSchema.safeParse({ name: "Topo Points" }).success).toBe(true);
  });

  it("accepts all updatable fields", () => {
    expect(PointGroupUpdateSchema.safeParse({
      name: "Topo Points",
      includeNumbers: "1-600",
      description: "Updated",
    }).success).toBe(true);
  });
});

describe("civil3d_point_export input schema", () => {
  it("accepts empty args (export all with defaults)", () => {
    expect(PointExportSchema.safeParse({}).success).toBe(true);
  });

  it("accepts all format options", () => {
    for (const format of ["pnezd", "penz", "xyzd", "xyz", "csv"]) {
      expect(PointExportSchema.safeParse({ format }).success).toBe(true);
    }
  });

  it("accepts groupName filter", () => {
    expect(PointExportSchema.safeParse({ groupName: "Topo Points", format: "xyzd" }).success).toBe(true);
  });

  it("accepts pointNumbers filter", () => {
    expect(PointExportSchema.safeParse({ pointNumbers: [1, 2, 3, 4, 5] }).success).toBe(true);
  });
});

describe("civil3d_point_transform input schema", () => {
  it("accepts empty args (no-op transform)", () => {
    expect(PointTransformSchema.safeParse({}).success).toBe(true);
  });

  it("accepts translation only", () => {
    expect(PointTransformSchema.safeParse({
      translateX: 100,
      translateY: 200,
    }).success).toBe(true);
  });

  it("accepts full transform", () => {
    expect(PointTransformSchema.safeParse({
      pointNumbers: [1, 2, 3],
      translateX: 5000,
      translateY: 10000,
      translateZ: 0,
      rotateRadians: 0.0174533,
      scaleFactor: 1.0,
    }).success).toBe(true);
  });
});

// ─── COGO inverse tests ───────────────────────────────────────────────────────

describe("civil3d_cogo_inverse input schema", () => {
  it("accepts two coordinate pairs", () => {
    expect(CogoInverseSchema.safeParse({
      x1: 1000, y1: 2000, x2: 1300, y2: 2400,
    }).success).toBe(true);
  });

  it("rejects missing coordinates", () => {
    expect(CogoInverseSchema.safeParse({ x1: 1000, y1: 2000, x2: 1300 }).success).toBe(false);
  });
});

// ─── COGO direction-distance tests ───────────────────────────────────────────

describe("civil3d_cogo_direction_distance input schema", () => {
  it("accepts minimal args", () => {
    expect(CogoDirectionDistanceSchema.safeParse({
      fromX: 1000,
      fromY: 2000,
      bearingDegrees: 45.0,
      distance: 100,
    }).success).toBe(true);
  });

  it("accepts optional fromZ and slope", () => {
    expect(CogoDirectionDistanceSchema.safeParse({
      fromX: 0,
      fromY: 0,
      fromZ: 100,
      bearingDegrees: 90,
      distance: 200,
      slope: 5.0,
    }).success).toBe(true);
  });

  it("rejects non-positive distance", () => {
    expect(CogoDirectionDistanceSchema.safeParse({
      fromX: 0, fromY: 0, bearingDegrees: 0, distance: 0,
    }).success).toBe(false);
  });
});

// ─── COGO traverse tests ──────────────────────────────────────────────────────

describe("civil3d_cogo_traverse input schema", () => {
  it("accepts single course", () => {
    expect(CogoTraverseSchema.safeParse({
      startX: 0,
      startY: 0,
      courses: [{ bearingDegrees: 45, distance: 100 }],
    }).success).toBe(true);
  });

  it("accepts closed traverse with multiple courses", () => {
    expect(CogoTraverseSchema.safeParse({
      startX: 1000,
      startY: 2000,
      startZ: 500,
      courses: [
        { bearingDegrees: 0, distance: 100, description: "N" },
        { bearingDegrees: 90, distance: 100, description: "E" },
        { bearingDegrees: 180, distance: 100, description: "S" },
        { bearingDegrees: 270, distance: 100, description: "W" },
      ],
      isClosed: true,
    }).success).toBe(true);
  });

  it("rejects empty courses array", () => {
    expect(CogoTraverseSchema.safeParse({
      startX: 0, startY: 0, courses: [],
    }).success).toBe(false);
  });
});

// ─── COGO curve solve tests ───────────────────────────────────────────────────

describe("civil3d_cogo_curve_solve input schema", () => {
  it("accepts radius + delta", () => {
    expect(CogoCurveSolveSchema.safeParse({ radius: 200, deltaDegrees: 30 }).success).toBe(true);
  });

  it("accepts radius + length", () => {
    expect(CogoCurveSolveSchema.safeParse({ radius: 500, length: 100 }).success).toBe(true);
  });

  it("accepts all elements (over-constrained but valid input)", () => {
    expect(CogoCurveSolveSchema.safeParse({
      radius: 200,
      deltaDegrees: 30,
      length: 104.72,
      tangent: 53.59,
      chord: 103.53,
    }).success).toBe(true);
  });

  it("accepts empty args (handled at runtime level)", () => {
    // Schema allows empty input; the C# handler validates that 2 elements are provided
    expect(CogoCurveSolveSchema.safeParse({}).success).toBe(true);
  });
});

// ─── Survey database / figure tests ──────────────────────────────────────────

describe("civil3d_survey_database_create input schema", () => {
  it("accepts name only", () => {
    expect(SurveyDatabaseCreateSchema.safeParse({ name: "Survey DB 1" }).success).toBe(true);
  });

  it("accepts name and path", () => {
    expect(SurveyDatabaseCreateSchema.safeParse({
      name: "Survey DB 1",
      path: "C:/Projects/survey.sdb",
    }).success).toBe(true);
  });

  it("rejects empty object", () => {
    expect(SurveyDatabaseCreateSchema.safeParse({}).success).toBe(false);
  });
});

describe("civil3d_survey_figure_list input schema", () => {
  it("accepts empty args (list all databases)", () => {
    expect(SurveyFigureListSchema.safeParse({}).success).toBe(true);
  });

  it("accepts databaseName filter", () => {
    expect(SurveyFigureListSchema.safeParse({ databaseName: "Survey DB 1" }).success).toBe(true);
  });
});

describe("civil3d_survey_figure_get input schema", () => {
  it("accepts name only", () => {
    expect(SurveyFigureGetSchema.safeParse({ name: "TOE-SLOPE" }).success).toBe(true);
  });

  it("accepts name and databaseName", () => {
    expect(SurveyFigureGetSchema.safeParse({
      name: "TOE-SLOPE",
      databaseName: "Survey DB 1",
    }).success).toBe(true);
  });

  it("rejects empty object", () => {
    expect(SurveyFigureGetSchema.safeParse({}).success).toBe(false);
  });
});
