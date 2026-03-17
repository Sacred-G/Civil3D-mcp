import { describe, it, expect } from "vitest";
import {
  QtyCorridorVolumesInputSchema,
  QtySurfaceVolumeInputSchema,
  QtyPipeNetworkLengthsInputSchema,
  QtyPressureNetworkLengthsInputSchema,
  QtyParcelAreasInputSchema,
  QtyAlignmentLengthsInputSchema,
  QtyPointCountByGroupInputSchema,
  QtyExportToCsvInputSchema,
  QtyMaterialListGetInputSchema,
  QtyEarthworkSummaryInputSchema,
} from "../src/tools/civil3d_quantity_takeoff.js";

// ─── civil3d_qty_corridor_volumes ──────────────────────────────────────────

describe("civil3d_qty_corridor_volumes input schema", () => {
  it("accepts name only", () => {
    expect(QtyCorridorVolumesInputSchema.safeParse({ name: "Road Corridor" }).success).toBe(true);
  });

  it("accepts full args with station range and materials", () => {
    expect(QtyCorridorVolumesInputSchema.safeParse({
      name: "Road Corridor",
      materials: ["Aggregate Base", "Asphalt"],
      startStation: 0,
      endStation: 500,
    }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(QtyCorridorVolumesInputSchema.safeParse({ startStation: 0 }).success).toBe(false);
  });

  it("rejects non-array materials", () => {
    expect(QtyCorridorVolumesInputSchema.safeParse({
      name: "Road Corridor",
      materials: "Asphalt",
    }).success).toBe(false);
  });
});

// ─── civil3d_qty_surface_volume ────────────────────────────────────────────

describe("civil3d_qty_surface_volume input schema", () => {
  it("accepts minimal args", () => {
    expect(QtySurfaceVolumeInputSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
    }).success).toBe(true);
  });

  it("accepts with corridorName", () => {
    expect(QtySurfaceVolumeInputSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
      corridorName: "Road Corridor",
    }).success).toBe(true);
  });

  it("accepts with polygon region", () => {
    expect(QtySurfaceVolumeInputSchema.safeParse({
      baseSurface: "EG",
      comparisonSurface: "FG",
      region: [{ x: 0, y: 0 }, { x: 100, y: 0 }, { x: 100, y: 100 }],
    }).success).toBe(true);
  });

  it("rejects missing baseSurface", () => {
    expect(QtySurfaceVolumeInputSchema.safeParse({ comparisonSurface: "FG" }).success).toBe(false);
  });

  it("rejects missing comparisonSurface", () => {
    expect(QtySurfaceVolumeInputSchema.safeParse({ baseSurface: "EG" }).success).toBe(false);
  });
});

// ─── civil3d_qty_pipe_network_lengths ──────────────────────────────────────

describe("civil3d_qty_pipe_network_lengths input schema", () => {
  it("accepts name only", () => {
    expect(QtyPipeNetworkLengthsInputSchema.safeParse({ name: "Storm" }).success).toBe(true);
  });

  it("accepts groupBy flags", () => {
    expect(QtyPipeNetworkLengthsInputSchema.safeParse({
      name: "Storm",
      groupBySize: true,
      groupByMaterial: false,
    }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(QtyPipeNetworkLengthsInputSchema.safeParse({ groupBySize: true }).success).toBe(false);
  });
});

// ─── civil3d_qty_pressure_network_lengths ──────────────────────────────────

describe("civil3d_qty_pressure_network_lengths input schema", () => {
  it("accepts name only", () => {
    expect(QtyPressureNetworkLengthsInputSchema.safeParse({ name: "Water Main" }).success).toBe(true);
  });

  it("accepts groupBy flags", () => {
    expect(QtyPressureNetworkLengthsInputSchema.safeParse({
      name: "Water Main",
      groupBySize: false,
      groupByMaterial: true,
    }).success).toBe(true);
  });

  it("rejects missing name", () => {
    expect(QtyPressureNetworkLengthsInputSchema.safeParse({}).success).toBe(false);
  });
});

// ─── civil3d_qty_parcel_areas ──────────────────────────────────────────────

describe("civil3d_qty_parcel_areas input schema", () => {
  it("accepts empty args (all parcels)", () => {
    expect(QtyParcelAreasInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts siteName filter", () => {
    expect(QtyParcelAreasInputSchema.safeParse({ siteName: "Site 1" }).success).toBe(true);
  });

  it("accepts parcelNames filter", () => {
    expect(QtyParcelAreasInputSchema.safeParse({
      parcelNames: ["Parcel 1", "Parcel 2"],
    }).success).toBe(true);
  });

  it("rejects non-array parcelNames", () => {
    expect(QtyParcelAreasInputSchema.safeParse({ parcelNames: "Parcel 1" }).success).toBe(false);
  });
});

// ─── civil3d_qty_alignment_lengths ─────────────────────────────────────────

describe("civil3d_qty_alignment_lengths input schema", () => {
  it("accepts empty args (all alignments)", () => {
    expect(QtyAlignmentLengthsInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts specific alignment names", () => {
    expect(QtyAlignmentLengthsInputSchema.safeParse({
      names: ["Main St", "Oak Ave"],
    }).success).toBe(true);
  });

  it("accepts station range", () => {
    expect(QtyAlignmentLengthsInputSchema.safeParse({
      startStation: 0,
      endStation: 1000,
    }).success).toBe(true);
  });
});

// ─── civil3d_qty_point_count_by_group ──────────────────────────────────────

describe("civil3d_qty_point_count_by_group input schema", () => {
  it("accepts empty args (all groups)", () => {
    expect(QtyPointCountByGroupInputSchema.safeParse({}).success).toBe(true);
  });

  it("accepts specific group names", () => {
    expect(QtyPointCountByGroupInputSchema.safeParse({
      groupNames: ["Topo", "Utilities"],
    }).success).toBe(true);
  });

  it("rejects non-array groupNames", () => {
    expect(QtyPointCountByGroupInputSchema.safeParse({ groupNames: "Topo" }).success).toBe(false);
  });
});

// ─── civil3d_qty_export_to_csv ─────────────────────────────────────────────

describe("civil3d_qty_export_to_csv input schema", () => {
  it("accepts outputPath only", () => {
    expect(QtyExportToCsvInputSchema.safeParse({ outputPath: "C:/output/quantities.csv" }).success).toBe(true);
  });

  it("accepts all include flags", () => {
    expect(QtyExportToCsvInputSchema.safeParse({
      outputPath: "C:/output/quantities.csv",
      includeCorridorVolumes: true,
      includeSurfaceVolumes: true,
      includePipeNetworks: false,
      includePressureNetworks: false,
      includeParcelAreas: true,
      includeAlignmentLengths: true,
      corridorName: "Road Corridor",
      baseSurface: "EG",
      comparisonSurface: "FG",
    }).success).toBe(true);
  });

  it("rejects missing outputPath", () => {
    expect(QtyExportToCsvInputSchema.safeParse({ includeCorridorVolumes: true }).success).toBe(false);
  });
});

// ─── civil3d_qty_material_list_get ─────────────────────────────────────────

describe("civil3d_qty_material_list_get input schema", () => {
  it("accepts corridorName only", () => {
    expect(QtyMaterialListGetInputSchema.safeParse({ corridorName: "Road Corridor" }).success).toBe(true);
  });

  it("accepts includeQuantities flag", () => {
    expect(QtyMaterialListGetInputSchema.safeParse({
      corridorName: "Road Corridor",
      includeQuantities: true,
    }).success).toBe(true);
  });

  it("rejects missing corridorName", () => {
    expect(QtyMaterialListGetInputSchema.safeParse({ includeQuantities: false }).success).toBe(false);
  });
});

// ─── civil3d_qty_earthwork_summary ─────────────────────────────────────────

describe("civil3d_qty_earthwork_summary input schema", () => {
  it("accepts minimal args", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({
      baseSurface: "EG",
      designSurface: "FG",
    }).success).toBe(true);
  });

  it("accepts full args", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({
      baseSurface: "EG",
      designSurface: "FG",
      alignmentName: "Main Street",
      startStation: 0,
      endStation: 1000,
      stationInterval: 25,
    }).success).toBe(true);
  });

  it("rejects zero stationInterval", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({
      baseSurface: "EG",
      designSurface: "FG",
      stationInterval: 0,
    }).success).toBe(false);
  });

  it("rejects negative stationInterval", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({
      baseSurface: "EG",
      designSurface: "FG",
      stationInterval: -10,
    }).success).toBe(false);
  });

  it("rejects missing baseSurface", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({ designSurface: "FG" }).success).toBe(false);
  });

  it("rejects missing designSurface", () => {
    expect(QtyEarthworkSummaryInputSchema.safeParse({ baseSurface: "EG" }).success).toBe(false);
  });
});
