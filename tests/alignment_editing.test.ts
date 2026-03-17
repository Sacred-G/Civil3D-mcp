import { describe, it, expect } from "vitest";
import { z } from "zod";

// ─── Input schemas (mirrored from civil3d_alignment_editing.ts) ──────────────

const AlignmentAddTangentSchema = z.object({
  alignmentName: z.string(),
  startX: z.number(),
  startY: z.number(),
  endX: z.number(),
  endY: z.number(),
});

const AlignmentAddCurveSchema = z.object({
  alignmentName: z.string(),
  passThroughX: z.number(),
  passThroughY: z.number(),
  radius: z.number().positive(),
});

const AlignmentAddSpiralSchema = z.object({
  alignmentName: z.string(),
  spiralType: z.enum(["clothoid", "cubic", "biquadratic"]).optional().default("clothoid"),
  startX: z.number(),
  startY: z.number(),
  startRadius: z.number(),
  endRadius: z.number(),
  length: z.number().positive(),
});

const AlignmentDeleteEntitySchema = z.object({
  alignmentName: z.string(),
  entityIndex: z.number().int().min(0),
});

const AlignmentSetStationEquationSchema = z.object({
  alignmentName: z.string(),
  rawStation: z.number(),
  nominalStation: z.number(),
});

const AlignmentGetStationOffsetSchema = z.object({
  alignmentName: z.string(),
  x: z.number(),
  y: z.number(),
});

const AlignmentOffsetCreateSchema = z.object({
  alignmentName: z.string(),
  offsetName: z.string(),
  offset: z.number(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const AlignmentWidenTransitionSchema = z.object({
  alignmentName: z.string(),
  side: z.enum(["left", "right"]),
  startStation: z.number(),
  endStation: z.number(),
  startOffset: z.number(),
  endOffset: z.number(),
  offsetName: z.string().optional(),
});

// ─── Input schemas (mirrored from civil3d_profile_editing.ts) ────────────────

const ProfileAddPviSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
  elevation: z.number(),
});

const ProfileDeletePviSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
});

const ProfileAddCurveSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  pviStation: z.number(),
  length: z.number().positive(),
  curveType: z.enum(["symmetric_parabola", "asymmetric_parabola"])
    .optional()
    .default("symmetric_parabola"),
});

const ProfileGetElevationSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
});

// ─── civil3d_alignment_add_tangent ───────────────────────────────────────────

describe("civil3d_alignment_add_tangent input schema", () => {
  it("accepts valid args", () => {
    expect(
      AlignmentAddTangentSchema.safeParse({
        alignmentName: "Main Road CL",
        startX: 1000,
        startY: 2000,
        endX: 1500,
        endY: 2000,
      }).success,
    ).toBe(true);
  });

  it("rejects missing alignmentName", () => {
    expect(
      AlignmentAddTangentSchema.safeParse({
        startX: 1000,
        startY: 2000,
        endX: 1500,
        endY: 2000,
      }).success,
    ).toBe(false);
  });

  it("rejects non-numeric coordinates", () => {
    expect(
      AlignmentAddTangentSchema.safeParse({
        alignmentName: "Road",
        startX: "abc",
        startY: 0,
        endX: 1,
        endY: 0,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_add_curve ─────────────────────────────────────────────

describe("civil3d_alignment_add_curve input schema", () => {
  it("accepts valid args", () => {
    expect(
      AlignmentAddCurveSchema.safeParse({
        alignmentName: "Road",
        passThroughX: 1250,
        passThroughY: 2050,
        radius: 300,
      }).success,
    ).toBe(true);
  });

  it("rejects zero radius", () => {
    expect(
      AlignmentAddCurveSchema.safeParse({
        alignmentName: "Road",
        passThroughX: 1250,
        passThroughY: 2050,
        radius: 0,
      }).success,
    ).toBe(false);
  });

  it("rejects negative radius", () => {
    expect(
      AlignmentAddCurveSchema.safeParse({
        alignmentName: "Road",
        passThroughX: 1250,
        passThroughY: 2050,
        radius: -100,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_add_spiral ────────────────────────────────────────────

describe("civil3d_alignment_add_spiral input schema", () => {
  it("defaults spiralType to clothoid", () => {
    const result = AlignmentAddSpiralSchema.parse({
      alignmentName: "Road",
      startX: 1000,
      startY: 2000,
      startRadius: 0,
      endRadius: 300,
      length: 80,
    });
    expect(result.spiralType).toBe("clothoid");
  });

  it("accepts cubic spiral type", () => {
    expect(
      AlignmentAddSpiralSchema.safeParse({
        alignmentName: "Road",
        spiralType: "cubic",
        startX: 1000,
        startY: 2000,
        startRadius: 0,
        endRadius: 300,
        length: 80,
      }).success,
    ).toBe(true);
  });

  it("rejects unknown spiral type", () => {
    expect(
      AlignmentAddSpiralSchema.safeParse({
        alignmentName: "Road",
        spiralType: "bloch",
        startX: 1000,
        startY: 2000,
        startRadius: 0,
        endRadius: 300,
        length: 80,
      }).success,
    ).toBe(false);
  });

  it("rejects zero length", () => {
    expect(
      AlignmentAddSpiralSchema.safeParse({
        alignmentName: "Road",
        startX: 1000,
        startY: 2000,
        startRadius: 0,
        endRadius: 300,
        length: 0,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_delete_entity ─────────────────────────────────────────

describe("civil3d_alignment_delete_entity input schema", () => {
  it("accepts valid index 0", () => {
    expect(
      AlignmentDeleteEntitySchema.safeParse({
        alignmentName: "Road",
        entityIndex: 0,
      }).success,
    ).toBe(true);
  });

  it("rejects negative index", () => {
    expect(
      AlignmentDeleteEntitySchema.safeParse({
        alignmentName: "Road",
        entityIndex: -1,
      }).success,
    ).toBe(false);
  });

  it("rejects float index", () => {
    expect(
      AlignmentDeleteEntitySchema.safeParse({
        alignmentName: "Road",
        entityIndex: 1.5,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_set_station_equation ──────────────────────────────────

describe("civil3d_alignment_set_station_equation input schema", () => {
  it("accepts valid equation", () => {
    expect(
      AlignmentSetStationEquationSchema.safeParse({
        alignmentName: "Road",
        rawStation: 5280,
        nominalStation: 0,
      }).success,
    ).toBe(true);
  });

  it("accepts negative nominal station (back station)", () => {
    expect(
      AlignmentSetStationEquationSchema.safeParse({
        alignmentName: "Road",
        rawStation: 1000,
        nominalStation: -200,
      }).success,
    ).toBe(true);
  });
});

// ─── civil3d_alignment_get_station_offset ────────────────────────────────────

describe("civil3d_alignment_get_station_offset input schema", () => {
  it("accepts valid point", () => {
    expect(
      AlignmentGetStationOffsetSchema.safeParse({
        alignmentName: "Road",
        x: 1234.5,
        y: 5678.9,
      }).success,
    ).toBe(true);
  });

  it("accepts origin point", () => {
    expect(
      AlignmentGetStationOffsetSchema.safeParse({
        alignmentName: "Road",
        x: 0,
        y: 0,
      }).success,
    ).toBe(true);
  });

  it("rejects missing y", () => {
    expect(
      AlignmentGetStationOffsetSchema.safeParse({
        alignmentName: "Road",
        x: 100,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_offset_create ─────────────────────────────────────────

describe("civil3d_alignment_offset_create input schema", () => {
  it("accepts positive offset (right side)", () => {
    expect(
      AlignmentOffsetCreateSchema.safeParse({
        alignmentName: "Road CL",
        offsetName: "Road EP Right",
        offset: 3.65,
      }).success,
    ).toBe(true);
  });

  it("accepts negative offset (left side)", () => {
    expect(
      AlignmentOffsetCreateSchema.safeParse({
        alignmentName: "Road CL",
        offsetName: "Road EP Left",
        offset: -3.65,
      }).success,
    ).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(
      AlignmentOffsetCreateSchema.safeParse({
        alignmentName: "Road CL",
        offsetName: "Road EP Right",
        offset: 3.65,
        style: "Offset Alignment",
        layer: "C-ROAD-EDGE",
        labelSet: "Standard",
      }).success,
    ).toBe(true);
  });

  it("rejects missing offset", () => {
    expect(
      AlignmentOffsetCreateSchema.safeParse({
        alignmentName: "Road",
        offsetName: "Offset",
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_alignment_widen_transition ──────────────────────────────────────

describe("civil3d_alignment_widen_transition input schema", () => {
  it("accepts valid right-side widening", () => {
    expect(
      AlignmentWidenTransitionSchema.safeParse({
        alignmentName: "Road CL",
        side: "right",
        startStation: 100,
        endStation: 150,
        startOffset: 3.65,
        endOffset: 7.3,
      }).success,
    ).toBe(true);
  });

  it("accepts left side", () => {
    expect(
      AlignmentWidenTransitionSchema.safeParse({
        alignmentName: "Road CL",
        side: "left",
        startStation: 200,
        endStation: 250,
        startOffset: 3.65,
        endOffset: 7.3,
        offsetName: "Turn Lane",
      }).success,
    ).toBe(true);
  });

  it("rejects invalid side", () => {
    expect(
      AlignmentWidenTransitionSchema.safeParse({
        alignmentName: "Road CL",
        side: "center",
        startStation: 100,
        endStation: 150,
        startOffset: 3.65,
        endOffset: 7.3,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_profile_add_pvi ─────────────────────────────────────────────────

describe("civil3d_profile_add_pvi input schema", () => {
  it("accepts valid PVI args", () => {
    expect(
      ProfileAddPviSchema.safeParse({
        alignmentName: "Road",
        profileName: "Proposed FG",
        station: 250.0,
        elevation: 152.45,
      }).success,
    ).toBe(true);
  });

  it("accepts zero elevation", () => {
    expect(
      ProfileAddPviSchema.safeParse({
        alignmentName: "Road",
        profileName: "Proposed FG",
        station: 0,
        elevation: 0,
      }).success,
    ).toBe(true);
  });

  it("rejects missing profileName", () => {
    expect(
      ProfileAddPviSchema.safeParse({
        alignmentName: "Road",
        station: 250,
        elevation: 152,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_profile_delete_pvi ──────────────────────────────────────────────

describe("civil3d_profile_delete_pvi input schema", () => {
  it("accepts valid args", () => {
    expect(
      ProfileDeletePviSchema.safeParse({
        alignmentName: "Road",
        profileName: "Proposed FG",
        station: 250,
      }).success,
    ).toBe(true);
  });
});

// ─── civil3d_profile_add_curve ───────────────────────────────────────────────

describe("civil3d_profile_add_curve input schema", () => {
  it("defaults to symmetric_parabola", () => {
    const result = ProfileAddCurveSchema.parse({
      alignmentName: "Road",
      profileName: "FG",
      pviStation: 250,
      length: 100,
    });
    expect(result.curveType).toBe("symmetric_parabola");
  });

  it("accepts asymmetric_parabola", () => {
    expect(
      ProfileAddCurveSchema.safeParse({
        alignmentName: "Road",
        profileName: "FG",
        pviStation: 250,
        length: 100,
        curveType: "asymmetric_parabola",
      }).success,
    ).toBe(true);
  });

  it("rejects zero curve length", () => {
    expect(
      ProfileAddCurveSchema.safeParse({
        alignmentName: "Road",
        profileName: "FG",
        pviStation: 250,
        length: 0,
      }).success,
    ).toBe(false);
  });
});

// ─── civil3d_profile_get_elevation ───────────────────────────────────────────

describe("civil3d_profile_get_elevation input schema", () => {
  it("accepts valid args", () => {
    expect(
      ProfileGetElevationSchema.safeParse({
        alignmentName: "Road",
        profileName: "EG",
        station: 500.0,
      }).success,
    ).toBe(true);
  });

  it("rejects missing station", () => {
    expect(
      ProfileGetElevationSchema.safeParse({
        alignmentName: "Road",
        profileName: "EG",
      }).success,
    ).toBe(false);
  });
});
