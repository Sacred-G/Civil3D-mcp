import { describe, it, expect } from "vitest";
import { z } from "zod";

// Re-declare the schemas here since they're not exported from the tool file.
// This tests the schema logic independently of the MCP registration.

const Civil3DHydrologyInputSchema = z.object({
  action: z.enum(["list_capabilities", "trace_flow_path", "find_low_point", "estimate_runoff", "delineate_watershed", "calculate_catchment_area"]),
  surfaceName: z.string().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  outletX: z.number().optional(),
  outletY: z.number().optional(),
  stepDistance: z.number().optional(),
  maxSteps: z.number().int().optional(),
  sampleSpacing: z.number().optional(),
  gridSpacing: z.number().optional(),
  searchRadius: z.number().optional(),
  maxDistance: z.number().optional(),
  drainageArea: z.number().optional(),
  runoffCoefficient: z.number().optional(),
  rainfallIntensity: z.number().optional(),
  areaUnits: z.enum(["acres", "hectares"]).optional(),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]).optional(),
});

const HydrologyTraceFlowPathArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("trace_flow_path"),
  surfaceName: z.string(),
  x: z.number(),
  y: z.number(),
  stepDistance: z.number().positive().optional(),
  maxSteps: z.number().int().positive().optional(),
});

const HydrologyEstimateRunoffArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("estimate_runoff"),
  drainageArea: z.number().positive(),
  runoffCoefficient: z.number().min(0).max(1),
  rainfallIntensity: z.number().positive(),
  areaUnits: z.enum(["acres", "hectares"]),
  intensityUnits: z.enum(["in_per_hr", "mm_per_hr"]),
});

const HydrologyDelineateWatershedArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("delineate_watershed"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  gridSpacing: z.number().positive().optional(),
  searchRadius: z.number().positive().optional(),
});

const HydrologyCalculateCatchmentAreaArgsSchema = Civil3DHydrologyInputSchema.extend({
  action: z.literal("calculate_catchment_area"),
  surfaceName: z.string(),
  outletX: z.number(),
  outletY: z.number(),
  sampleSpacing: z.number().positive().optional(),
  maxDistance: z.number().positive().optional(),
});

const HydrologyWatershedResponseSchema = z.object({
  surfaceName: z.string(),
  outletPoint: z.object({ x: z.number(), y: z.number(), elevation: z.number() }),
  gridSpacing: z.number(),
  searchRadius: z.number(),
  contributingPointCount: z.number(),
  boundaryPoints: z.array(z.object({ x: z.number(), y: z.number(), elevation: z.number() })),
  approximateArea: z.number(),
  units: z.object({ horizontal: z.string(), vertical: z.string(), area: z.string() }),
});

const HydrologyCatchmentAreaResponseSchema = z.object({
  surfaceName: z.string(),
  outletPoint: z.object({ x: z.number(), y: z.number(), elevation: z.number() }),
  sampleSpacing: z.number(),
  maxDistance: z.number(),
  contributingCellCount: z.number(),
  catchmentArea: z.number(),
  elevationStatistics: z.object({
    minimum: z.number(),
    maximum: z.number(),
    average: z.number(),
    relief: z.number(),
  }),
  units: z.object({ horizontal: z.string(), vertical: z.string(), area: z.string() }),
});

describe("Hydrology Input Schema Validation", () => {
  describe("trace_flow_path", () => {
    it("accepts valid trace_flow_path args", () => {
      const result = HydrologyTraceFlowPathArgsSchema.safeParse({
        action: "trace_flow_path",
        surfaceName: "EG",
        x: 1000,
        y: 2000,
      });
      expect(result.success).toBe(true);
    });

    it("accepts optional stepDistance and maxSteps", () => {
      const result = HydrologyTraceFlowPathArgsSchema.safeParse({
        action: "trace_flow_path",
        surfaceName: "EG",
        x: 1000,
        y: 2000,
        stepDistance: 5,
        maxSteps: 100,
      });
      expect(result.success).toBe(true);
    });

    it("rejects missing surfaceName", () => {
      const result = HydrologyTraceFlowPathArgsSchema.safeParse({
        action: "trace_flow_path",
        x: 1000,
        y: 2000,
      });
      expect(result.success).toBe(false);
    });

    it("rejects negative stepDistance", () => {
      const result = HydrologyTraceFlowPathArgsSchema.safeParse({
        action: "trace_flow_path",
        surfaceName: "EG",
        x: 1000,
        y: 2000,
        stepDistance: -1,
      });
      expect(result.success).toBe(false);
    });

    it("rejects non-integer maxSteps", () => {
      const result = HydrologyTraceFlowPathArgsSchema.safeParse({
        action: "trace_flow_path",
        surfaceName: "EG",
        x: 1000,
        y: 2000,
        maxSteps: 10.5,
      });
      expect(result.success).toBe(false);
    });
  });

  describe("estimate_runoff", () => {
    it("accepts valid runoff args", () => {
      const result = HydrologyEstimateRunoffArgsSchema.safeParse({
        action: "estimate_runoff",
        drainageArea: 50,
        runoffCoefficient: 0.65,
        rainfallIntensity: 4.2,
        areaUnits: "acres",
        intensityUnits: "in_per_hr",
      });
      expect(result.success).toBe(true);
    });

    it("rejects runoffCoefficient > 1", () => {
      const result = HydrologyEstimateRunoffArgsSchema.safeParse({
        action: "estimate_runoff",
        drainageArea: 50,
        runoffCoefficient: 1.5,
        rainfallIntensity: 4.2,
        areaUnits: "acres",
        intensityUnits: "in_per_hr",
      });
      expect(result.success).toBe(false);
    });

    it("rejects negative drainage area", () => {
      const result = HydrologyEstimateRunoffArgsSchema.safeParse({
        action: "estimate_runoff",
        drainageArea: -10,
        runoffCoefficient: 0.5,
        rainfallIntensity: 4.2,
        areaUnits: "acres",
        intensityUnits: "in_per_hr",
      });
      expect(result.success).toBe(false);
    });

    it("rejects invalid area units", () => {
      const result = HydrologyEstimateRunoffArgsSchema.safeParse({
        action: "estimate_runoff",
        drainageArea: 50,
        runoffCoefficient: 0.5,
        rainfallIntensity: 4.2,
        areaUnits: "square_miles",
        intensityUnits: "in_per_hr",
      });
      expect(result.success).toBe(false);
    });
  });

  describe("delineate_watershed", () => {
    it("accepts valid watershed args", () => {
      const result = HydrologyDelineateWatershedArgsSchema.safeParse({
        action: "delineate_watershed",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
      });
      expect(result.success).toBe(true);
    });

    it("accepts optional gridSpacing and searchRadius", () => {
      const result = HydrologyDelineateWatershedArgsSchema.safeParse({
        action: "delineate_watershed",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
        gridSpacing: 5,
        searchRadius: 200,
      });
      expect(result.success).toBe(true);
    });

    it("rejects negative gridSpacing", () => {
      const result = HydrologyDelineateWatershedArgsSchema.safeParse({
        action: "delineate_watershed",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
        gridSpacing: -5,
      });
      expect(result.success).toBe(false);
    });

    it("rejects zero searchRadius", () => {
      const result = HydrologyDelineateWatershedArgsSchema.safeParse({
        action: "delineate_watershed",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
        searchRadius: 0,
      });
      expect(result.success).toBe(false);
    });

    it("rejects missing outletX", () => {
      const result = HydrologyDelineateWatershedArgsSchema.safeParse({
        action: "delineate_watershed",
        surfaceName: "EG",
        outletY: 600,
      });
      expect(result.success).toBe(false);
    });
  });

  describe("calculate_catchment_area", () => {
    it("accepts valid catchment args", () => {
      const result = HydrologyCalculateCatchmentAreaArgsSchema.safeParse({
        action: "calculate_catchment_area",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
      });
      expect(result.success).toBe(true);
    });

    it("rejects negative sampleSpacing", () => {
      const result = HydrologyCalculateCatchmentAreaArgsSchema.safeParse({
        action: "calculate_catchment_area",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
        sampleSpacing: -10,
      });
      expect(result.success).toBe(false);
    });

    it("rejects zero maxDistance", () => {
      const result = HydrologyCalculateCatchmentAreaArgsSchema.safeParse({
        action: "calculate_catchment_area",
        surfaceName: "EG",
        outletX: 500,
        outletY: 600,
        maxDistance: 0,
      });
      expect(result.success).toBe(false);
    });
  });

  describe("action enum", () => {
    it("rejects unknown action", () => {
      const result = Civil3DHydrologyInputSchema.safeParse({
        action: "unknown_action",
      });
      expect(result.success).toBe(false);
    });

    it("accepts all valid actions", () => {
      const validActions = [
        "list_capabilities",
        "trace_flow_path",
        "find_low_point",
        "estimate_runoff",
        "delineate_watershed",
        "calculate_catchment_area",
      ];
      for (const action of validActions) {
        const result = Civil3DHydrologyInputSchema.safeParse({ action });
        expect(result.success).toBe(true);
      }
    });
  });
});

describe("Hydrology Response Schema Validation", () => {
  it("validates a watershed response", () => {
    const response = {
      surfaceName: "EG",
      outletPoint: { x: 500, y: 600, elevation: 100 },
      gridSpacing: 10,
      searchRadius: 100,
      contributingPointCount: 42,
      boundaryPoints: [
        { x: 450, y: 550, elevation: 105 },
        { x: 550, y: 650, elevation: 110 },
        { x: 450, y: 650, elevation: 108 },
      ],
      approximateArea: 15000,
      units: { horizontal: "Meter", vertical: "Meter", area: "Meter²" },
    };
    expect(HydrologyWatershedResponseSchema.safeParse(response).success).toBe(true);
  });

  it("rejects watershed response missing area", () => {
    const response = {
      surfaceName: "EG",
      outletPoint: { x: 500, y: 600, elevation: 100 },
      gridSpacing: 10,
      searchRadius: 100,
      contributingPointCount: 42,
      boundaryPoints: [],
      // missing approximateArea
      units: { horizontal: "Meter", vertical: "Meter", area: "Meter²" },
    };
    expect(HydrologyWatershedResponseSchema.safeParse(response).success).toBe(false);
  });

  it("validates a catchment area response", () => {
    const response = {
      surfaceName: "EG",
      outletPoint: { x: 500, y: 600, elevation: 100 },
      sampleSpacing: 15,
      maxDistance: 200,
      contributingCellCount: 85,
      catchmentArea: 19125,
      elevationStatistics: {
        minimum: 95,
        maximum: 130,
        average: 112.5,
        relief: 35,
      },
      units: { horizontal: "Meter", vertical: "Meter", area: "Meter²" },
    };
    expect(HydrologyCatchmentAreaResponseSchema.safeParse(response).success).toBe(true);
  });

  it("rejects catchment response with missing elevationStatistics fields", () => {
    const response = {
      surfaceName: "EG",
      outletPoint: { x: 500, y: 600, elevation: 100 },
      sampleSpacing: 15,
      maxDistance: 200,
      contributingCellCount: 85,
      catchmentArea: 19125,
      elevationStatistics: {
        minimum: 95,
        // missing maximum, average, relief
      },
      units: { horizontal: "Meter", vertical: "Meter", area: "Meter²" },
    };
    expect(HydrologyCatchmentAreaResponseSchema.safeParse(response).success).toBe(false);
  });
});
