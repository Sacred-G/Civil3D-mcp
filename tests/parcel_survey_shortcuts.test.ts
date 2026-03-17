import { describe, it, expect } from "vitest";
import {
  ParcelCreateInputSchema,
  ParcelEditInputSchema,
  ParcelLotLineAdjustInputSchema,
  ParcelReportInputSchema,
} from "../src/tools/civil3d_parcel_editing.js";
import {
  SurveyObservationListInputSchema,
  SurveyNetworkAdjustInputSchema,
  SurveyFigureCreateInputSchema,
  SurveyLandXmlImportInputSchema,
} from "../src/tools/civil3d_survey_processing.js";
import {
  DataShortcutCreateInputSchema,
  DataShortcutPromoteInputSchema,
  DataShortcutReferenceInputSchema,
  DataShortcutSyncInputSchema,
} from "../src/tools/civil3d_data_shortcut_mgmt.js";

// ─── civil3d_parcel_create ────────────────────────────────────────────────────

describe("civil3d_parcel_create input schema", () => {
  it("accepts source handle", () => {
    expect(ParcelCreateInputSchema.safeParse({
      siteName: "Subdivision",
      sourceHandle: "1A3F",
    }).success).toBe(true);
  });

  it("accepts vertex point list", () => {
    expect(ParcelCreateInputSchema.safeParse({
      siteName: "Subdivision",
      points: [[0, 0], [100, 0], [100, 100], [0, 100]],
    }).success).toBe(true);
  });

  it("rejects points with fewer than 3 vertices", () => {
    expect(ParcelCreateInputSchema.safeParse({
      siteName: "Subdivision",
      points: [[0, 0], [100, 0]],
    }).success).toBe(false);
  });

  it("rejects missing siteName", () => {
    expect(ParcelCreateInputSchema.safeParse({ sourceHandle: "1A3F" }).success).toBe(false);
  });
});

// ─── civil3d_parcel_edit ──────────────────────────────────────────────────────

describe("civil3d_parcel_edit input schema", () => {
  it("accepts minimal args", () => {
    expect(ParcelEditInputSchema.safeParse({
      siteName: "Subdivision",
      parcelName: "Parcel-1",
      newName: "LOT-1",
    }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(ParcelEditInputSchema.safeParse({
      siteName: "Subdivision",
      parcelName: "Parcel-1",
      newName: "LOT-1",
      style: "Standard",
      areaLabelStyle: "Parcel Area",
      description: "APN 123-456-789",
    }).success).toBe(true);
  });

  it("rejects missing parcelName", () => {
    expect(ParcelEditInputSchema.safeParse({ siteName: "Subdivision" }).success).toBe(false);
  });
});

// ─── civil3d_parcel_lot_line_adjust ──────────────────────────────────────────

describe("civil3d_parcel_lot_line_adjust input schema", () => {
  it("accepts required args", () => {
    expect(ParcelLotLineAdjustInputSchema.safeParse({
      siteName: "Subdivision",
      parcelName: "Parcel-1",
      targetAreaSqFt: 10000,
    }).success).toBe(true);
  });

  it("accepts optional tolerance and handle", () => {
    expect(ParcelLotLineAdjustInputSchema.safeParse({
      siteName: "Subdivision",
      parcelName: "Parcel-1",
      targetAreaSqFt: 10000,
      lotLineHandle: "2B4C",
      tolerance: 0.5,
    }).success).toBe(true);
  });

  it("rejects non-positive target area", () => {
    expect(ParcelLotLineAdjustInputSchema.safeParse({
      siteName: "Subdivision",
      parcelName: "Parcel-1",
      targetAreaSqFt: 0,
    }).success).toBe(false);
  });
});

// ─── civil3d_parcel_report ────────────────────────────────────────────────────

describe("civil3d_parcel_report input schema", () => {
  it("accepts site name only", () => {
    expect(ParcelReportInputSchema.safeParse({ siteName: "Subdivision" }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(ParcelReportInputSchema.safeParse({
      siteName: "Subdivision",
      parcelNames: ["Parcel-1", "Parcel-2"],
      outputPath: "C:/reports/parcels.csv",
      includeCoordinates: true,
      units: "acres",
    }).success).toBe(true);
  });

  it("rejects invalid units", () => {
    expect(ParcelReportInputSchema.safeParse({
      siteName: "Subdivision",
      units: "miles",
    }).success).toBe(false);
  });
});

// ─── civil3d_survey_observation_list ─────────────────────────────────────────

describe("civil3d_survey_observation_list input schema", () => {
  it("accepts required args", () => {
    expect(SurveyObservationListInputSchema.safeParse({ databaseName: "SurveyDB-1" }).success).toBe(true);
  });

  it("accepts all optional fields", () => {
    expect(SurveyObservationListInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      networkName: "Network-A",
      observationType: "angles",
    }).success).toBe(true);
  });

  it("rejects invalid observationType", () => {
    expect(SurveyObservationListInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      observationType: "levels",
    }).success).toBe(false);
  });
});

// ─── civil3d_survey_network_adjust ───────────────────────────────────────────

describe("civil3d_survey_network_adjust input schema", () => {
  it("accepts required args", () => {
    expect(SurveyNetworkAdjustInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      networkName: "Traverse-A",
    }).success).toBe(true);
  });

  it("accepts least_squares with confidence level", () => {
    expect(SurveyNetworkAdjustInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      networkName: "Traverse-A",
      method: "least_squares",
      confidenceLevel: 95,
      applyAdjustment: true,
    }).success).toBe(true);
  });

  it("rejects confidence level out of range", () => {
    expect(SurveyNetworkAdjustInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      networkName: "Traverse-A",
      confidenceLevel: 110,
    }).success).toBe(false);
  });

  it("rejects invalid method", () => {
    expect(SurveyNetworkAdjustInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      networkName: "Traverse-A",
      method: "bowditch",
    }).success).toBe(false);
  });
});

// ─── civil3d_survey_figure_create ────────────────────────────────────────────

describe("civil3d_survey_figure_create input schema", () => {
  it("accepts required args", () => {
    expect(SurveyFigureCreateInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      figureName: "TOE-1",
      pointNumbers: [101, 102, 103],
    }).success).toBe(true);
  });

  it("rejects fewer than 2 points", () => {
    expect(SurveyFigureCreateInputSchema.safeParse({
      databaseName: "SurveyDB-1",
      figureName: "TOE-1",
      pointNumbers: [101],
    }).success).toBe(false);
  });
});

// ─── civil3d_survey_landxml_import ───────────────────────────────────────────

describe("civil3d_survey_landxml_import input schema", () => {
  it("accepts required args", () => {
    expect(SurveyLandXmlImportInputSchema.safeParse({
      filePath: "C:/survey/topo.xml",
      databaseName: "SurveyDB-1",
    }).success).toBe(true);
  });

  it("accepts all optional flags", () => {
    expect(SurveyLandXmlImportInputSchema.safeParse({
      filePath: "C:/survey/topo.xml",
      databaseName: "SurveyDB-1",
      importPoints: true,
      importAlignments: true,
      importSurfaces: false,
      coordinateSystemOverride: "CA83-I",
      duplicatePolicy: "rename",
    }).success).toBe(true);
  });

  it("rejects invalid duplicatePolicy", () => {
    expect(SurveyLandXmlImportInputSchema.safeParse({
      filePath: "C:/survey/topo.xml",
      databaseName: "SurveyDB-1",
      duplicatePolicy: "merge",
    }).success).toBe(false);
  });
});

// ─── civil3d_data_shortcut_create ────────────────────────────────────────────

describe("civil3d_data_shortcut_create input schema", () => {
  it("accepts surface shortcut", () => {
    expect(DataShortcutCreateInputSchema.safeParse({
      objectType: "surface",
      objectName: "Existing Ground",
    }).success).toBe(true);
  });

  it("accepts all object types", () => {
    const types = ["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor", "section_view_group"] as const;
    for (const t of types) {
      expect(DataShortcutCreateInputSchema.safeParse({ objectType: t, objectName: "Obj" }).success).toBe(true);
    }
  });

  it("rejects invalid object type", () => {
    expect(DataShortcutCreateInputSchema.safeParse({
      objectType: "grading",
      objectName: "Grading Group",
    }).success).toBe(false);
  });
});

// ─── civil3d_data_shortcut_promote ───────────────────────────────────────────

describe("civil3d_data_shortcut_promote input schema", () => {
  it("accepts required args", () => {
    expect(DataShortcutPromoteInputSchema.safeParse({
      shortcutName: "Existing Ground",
      shortcutType: "surface",
    }).success).toBe(true);
  });

  it("accepts optional newName", () => {
    expect(DataShortcutPromoteInputSchema.safeParse({
      shortcutName: "Existing Ground",
      shortcutType: "surface",
      newName: "EG-Promoted",
    }).success).toBe(true);
  });
});

// ─── civil3d_data_shortcut_reference ─────────────────────────────────────────

describe("civil3d_data_shortcut_reference input schema", () => {
  it("accepts required args", () => {
    expect(DataShortcutReferenceInputSchema.safeParse({
      projectFolder: "C:/Projects/Subdivision",
      shortcutName: "Existing Ground",
      shortcutType: "surface",
    }).success).toBe(true);
  });

  it("accepts optional layer", () => {
    expect(DataShortcutReferenceInputSchema.safeParse({
      projectFolder: "C:/Projects/Subdivision",
      shortcutName: "Road CL",
      shortcutType: "alignment",
      layer: "C-ROAD",
    }).success).toBe(true);
  });
});

// ─── civil3d_data_shortcut_sync ──────────────────────────────────────────────

describe("civil3d_data_shortcut_sync input schema", () => {
  it("accepts no args (sync all)", () => {
    expect(DataShortcutSyncInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts specific shortcut names", () => {
    expect(DataShortcutSyncInputSchema.safeParse({
      shortcutNames: ["Existing Ground", "Road CL"],
      dryRun: true,
    }).success).toBe(true);
  });
});
