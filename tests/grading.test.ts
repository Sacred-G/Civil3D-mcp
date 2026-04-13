import { describe, it, expect } from "vitest";
import { z } from "zod";

// ─── Input schemas (mirrored from civil3d_grading.ts) ──────────────────────

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().default(0),
});

const GradingGroupGetSchema = z.object({ name: z.string() });
const GradingGroupCreateSchema = z.object({
  name: z.string(),
  description: z.string().optional(),
  useProjection: z.boolean().optional(),
});
const GradingGroupDeleteSchema = z.object({ name: z.string() });
const GradingGroupVolumeSchema = z.object({ name: z.string() });
const GradingGroupSurfaceCreateSchema = z.object({
  name: z.string(),
  surfaceName: z.string().optional(),
});
const GradingListSchema = z.object({ groupName: z.string() });
const GradingGetSchema = z.object({
  groupName: z.string(),
  handle: z.string(),
});
const GradingCreateSchema = z.object({
  groupName: z.string(),
  featureLineName: z.string(),
  criteriaName: z.string().optional(),
  side: z.enum(["left", "right", "both"]).optional(),
});
const GradingDeleteSchema = z.object({
  groupName: z.string(),
  handle: z.string(),
});
const FeatureLineGetSchema = z.object({
  name: z.string(),
});
const FeatureLineExportSchema = z.object({
  name: z.string(),
  targetLayer: z.string().optional(),
});
const FeatureLineCreateSchema = z.object({
  points: z.array(Point3DSchema).min(2),
  name: z.string().optional(),
  layer: z.string().optional(),
});

// ─── civil3d_grading_group_create ─────────────────────────────────────────

describe("civil3d_grading_group_create input schema", () => {
  it("accepts minimal valid args", () => {
    expect(GradingGroupCreateSchema.safeParse({ name: "GG-1" }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(GradingGroupCreateSchema.safeParse({
      name: "GG-1",
      description: "Site grading group",
      useProjection: true,
    }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(GradingGroupCreateSchema.safeParse({ description: "test" }).success).toBe(false);
  });
});

// ─── civil3d_grading_group_get ────────────────────────────────────────────

describe("civil3d_grading_group_get input schema", () => {
  it("accepts valid name", () => {
    expect(GradingGroupGetSchema.safeParse({ name: "GG-1" }).success).toBe(true);
  });

  it("rejects empty object", () => {
    expect(GradingGroupGetSchema.safeParse({}).success).toBe(false);
  });
});

// ─── civil3d_grading_group_volume ─────────────────────────────────────────

describe("civil3d_grading_group_volume input schema", () => {
  it("accepts valid name", () => {
    expect(GradingGroupVolumeSchema.safeParse({ name: "GG-1" }).success).toBe(true);
  });
});

// ─── civil3d_grading_group_surface_create ─────────────────────────────────

describe("civil3d_grading_group_surface_create input schema", () => {
  it("accepts minimal args", () => {
    expect(GradingGroupSurfaceCreateSchema.safeParse({ name: "GG-1" }).success).toBe(true);
  });

  it("accepts optional surfaceName", () => {
    expect(GradingGroupSurfaceCreateSchema.safeParse({
      name: "GG-1",
      surfaceName: "Graded Surface",
    }).success).toBe(true);
  });
});

// ─── civil3d_grading_list ─────────────────────────────────────────────────

describe("civil3d_grading_list input schema", () => {
  it("accepts groupName", () => {
    expect(GradingListSchema.safeParse({ groupName: "GG-1" }).success).toBe(true);
  });

  it("rejects empty object", () => {
    expect(GradingListSchema.safeParse({}).success).toBe(false);
  });
});

// ─── civil3d_grading_create ───────────────────────────────────────────────

describe("civil3d_grading_create input schema", () => {
  it("accepts minimal args", () => {
    expect(GradingCreateSchema.safeParse({
      groupName: "GG-1",
      featureLineName: "FL-PAD",
    }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(GradingCreateSchema.safeParse({
      groupName: "GG-1",
      featureLineName: "FL-PAD",
      criteriaName: "Grade to Surface",
      side: "left",
    }).success).toBe(true);
  });

  it("rejects invalid side", () => {
    expect(GradingCreateSchema.safeParse({
      groupName: "GG-1",
      featureLineName: "FL-PAD",
      side: "uphill",
    }).success).toBe(false);
  });
});

// ─── civil3d_grading_delete ───────────────────────────────────────────────

describe("civil3d_grading_delete input schema", () => {
  it("requires both groupName and handle", () => {
    expect(GradingDeleteSchema.safeParse({ groupName: "GG-1", handle: "1A3F" }).success).toBe(true);
    expect(GradingDeleteSchema.safeParse({ groupName: "GG-1" }).success).toBe(false);
    expect(GradingDeleteSchema.safeParse({ handle: "1A3F" }).success).toBe(false);
  });
});

// ─── civil3d_feature_line_create ──────────────────────────────────────────

describe("civil3d_feature_line_create input schema", () => {
  it("accepts two valid 3D points", () => {
    expect(FeatureLineCreateSchema.safeParse({
      points: [
        { x: 100, y: 200, z: 10 },
        { x: 200, y: 300, z: 12 },
      ],
    }).success).toBe(true);
  });

  it("rejects fewer than 2 points", () => {
    expect(FeatureLineCreateSchema.safeParse({
      points: [{ x: 100, y: 200, z: 10 }],
    }).success).toBe(false);
  });

  it("defaults z to 0 when omitted", () => {
    const result = FeatureLineCreateSchema.safeParse({
      points: [
        { x: 100, y: 200 },
        { x: 300, y: 400 },
      ],
    });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.points[0].z).toBe(0);
    }
  });

  it("accepts optional name and layer", () => {
    expect(FeatureLineCreateSchema.safeParse({
      points: [{ x: 0, y: 0, z: 0 }, { x: 10, y: 10, z: 5 }],
      name: "PAD-EDGE",
      layer: "GRADING",
    }).success).toBe(true);
  });
});

// ─── civil3d_feature_line get/export aliases ──────────────────────────────

describe("civil3d_feature_line get/export input schemas", () => {
  it("requires a name to get a feature line", () => {
    expect(FeatureLineGetSchema.safeParse({ name: "PAD-EDGE" }).success).toBe(true);
    expect(FeatureLineGetSchema.safeParse({}).success).toBe(false);
  });

  it("accepts export args with optional targetLayer", () => {
    expect(FeatureLineExportSchema.safeParse({ name: "PAD-EDGE" }).success).toBe(true);
    expect(FeatureLineExportSchema.safeParse({
      name: "PAD-EDGE",
      targetLayer: "C-TOPO-FL",
    }).success).toBe(true);
  });
});
