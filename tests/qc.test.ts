import { describe, it, expect } from "vitest";
import {
  QcAlignmentInputSchema,
  QcProfileInputSchema,
  QcCorridorInputSchema,
  QcPipeNetworkInputSchema,
  QcSurfaceInputSchema,
  QcLabelsInputSchema,
  QcReportInputSchema,
  QcDrawingStandardsInputSchema,
} from "../src/tools/civil3d_qc.js";

// ─── civil3d_qc_check_alignment ────────────────────────────────────────────

describe("civil3d_qc_check_alignment input schema", () => {
  it("accepts minimal args (name only)", () => {
    expect(QcAlignmentInputSchema.safeParse({ name: "Main Street" }).success).toBe(true);
  });

  it("accepts full args with design speed and flags", () => {
    expect(QcAlignmentInputSchema.safeParse({
      name: "Main Street",
      designSpeed: 60,
      checkTangents: true,
      checkCurves: true,
      checkSpirals: false,
    }).success).toBe(true);
  });

  it("rejects negative designSpeed", () => {
    expect(QcAlignmentInputSchema.safeParse({ name: "Main Street", designSpeed: -30 }).success).toBe(false);
  });

  it("rejects zero designSpeed", () => {
    expect(QcAlignmentInputSchema.safeParse({ name: "Main Street", designSpeed: 0 }).success).toBe(false);
  });

  it("rejects missing name", () => {
    expect(QcAlignmentInputSchema.safeParse({ designSpeed: 60 }).success).toBe(false);
  });

  it("rejects non-boolean checkTangents", () => {
    expect(QcAlignmentInputSchema.safeParse({ name: "Main Street", checkTangents: "yes" }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_profile ──────────────────────────────────────────────

describe("civil3d_qc_check_profile input schema", () => {
  it("accepts minimal args", () => {
    expect(QcProfileInputSchema.safeParse({
      alignmentName: "Main Street",
      profileName: "FG",
    }).success).toBe(true);
  });

  it("accepts full args", () => {
    expect(QcProfileInputSchema.safeParse({
      alignmentName: "Main Street",
      profileName: "FG",
      maxGrade: 8.0,
      minKValue: 30,
    }).success).toBe(true);
  });

  it("rejects zero maxGrade", () => {
    expect(QcProfileInputSchema.safeParse({
      alignmentName: "Main Street",
      profileName: "FG",
      maxGrade: 0,
    }).success).toBe(false);
  });

  it("rejects negative minKValue", () => {
    expect(QcProfileInputSchema.safeParse({
      alignmentName: "Main Street",
      profileName: "FG",
      minKValue: -1,
    }).success).toBe(false);
  });

  it("rejects missing profileName", () => {
    expect(QcProfileInputSchema.safeParse({ alignmentName: "Main Street" }).success).toBe(false);
  });

  it("rejects missing alignmentName", () => {
    expect(QcProfileInputSchema.safeParse({ profileName: "FG" }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_corridor ─────────────────────────────────────────────

describe("civil3d_qc_check_corridor input schema", () => {
  it("accepts corridor name", () => {
    expect(QcCorridorInputSchema.safeParse({ name: "Road Corridor" }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(QcCorridorInputSchema.safeParse({}).success).toBe(false);
  });

  it("rejects non-string name", () => {
    expect(QcCorridorInputSchema.safeParse({ name: 42 }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_pipe_network ─────────────────────────────────────────

describe("civil3d_qc_check_pipe_network input schema", () => {
  it("accepts name only", () => {
    expect(QcPipeNetworkInputSchema.safeParse({ name: "Storm Network" }).success).toBe(true);
  });

  it("accepts full thresholds", () => {
    expect(QcPipeNetworkInputSchema.safeParse({
      name: "Storm Network",
      minCover: 1.5,
      maxSlope: 15.0,
      minSlope: 0.5,
      minVelocity: 0.6,
      maxVelocity: 3.0,
    }).success).toBe(true);
  });

  it("rejects negative minCover", () => {
    expect(QcPipeNetworkInputSchema.safeParse({ name: "Storm Network", minCover: -1 }).success).toBe(false);
  });

  it("rejects zero maxSlope", () => {
    expect(QcPipeNetworkInputSchema.safeParse({ name: "Storm Network", maxSlope: 0 }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_surface ──────────────────────────────────────────────

describe("civil3d_qc_check_surface input schema", () => {
  it("accepts name only", () => {
    expect(QcSurfaceInputSchema.safeParse({ name: "EG" }).success).toBe(true);
  });

  it("accepts spike and flat triangle thresholds", () => {
    expect(QcSurfaceInputSchema.safeParse({
      name: "EG",
      spikeThreshold: 5.0,
      flatTriangleThreshold: 0.01,
    }).success).toBe(true);
  });

  it("rejects zero spikeThreshold", () => {
    expect(QcSurfaceInputSchema.safeParse({ name: "EG", spikeThreshold: 0 }).success).toBe(false);
  });

  it("rejects negative flatTriangleThreshold", () => {
    expect(QcSurfaceInputSchema.safeParse({ name: "EG", flatTriangleThreshold: -0.1 }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_labels ───────────────────────────────────────────────

describe("civil3d_qc_check_labels input schema", () => {
  it("accepts empty args (all defaults)", () => {
    expect(QcLabelsInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts all objectType values", () => {
    for (const t of ["alignment", "profile", "surface", "pipe_network", "all"]) {
      expect(QcLabelsInputSchema.safeParse({ objectType: t }).success).toBe(true);
    }
  });

  it("rejects unknown objectType", () => {
    expect(QcLabelsInputSchema.safeParse({ objectType: "corridor" }).success).toBe(false);
  });
});

// ─── civil3d_qc_report_generate ────────────────────────────────────────────

describe("civil3d_qc_report_generate input schema", () => {
  it("accepts outputPath only", () => {
    expect(QcReportInputSchema.safeParse({ outputPath: "C:/reports/qc.txt" }).success).toBe(true);
  });

  it("accepts all include flags", () => {
    expect(QcReportInputSchema.safeParse({
      outputPath: "C:/reports/qc.csv",
      includeAlignments: true,
      includeProfiles: false,
      includeCorridors: true,
      includePipeNetworks: true,
      includeSurfaces: false,
      includeLabels: true,
    }).success).toBe(true);
  });

  it("rejects missing outputPath", () => {
    expect(QcReportInputSchema.safeParse({ includeAlignments: true }).success).toBe(false);
  });
});

// ─── civil3d_qc_check_drawing_standards ───────────────────────────────────

describe("civil3d_qc_check_drawing_standards input schema", () => {
  it("accepts empty args", () => {
    expect(QcDrawingStandardsInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts layer prefix and check flags", () => {
    expect(QcDrawingStandardsInputSchema.safeParse({
      layerPrefix: "C-",
      checkLineweights: true,
      checkColors: false,
    }).success).toBe(true);
  });

  it("rejects non-boolean checkLineweights", () => {
    expect(QcDrawingStandardsInputSchema.safeParse({ checkLineweights: "true" }).success).toBe(false);
  });
});
