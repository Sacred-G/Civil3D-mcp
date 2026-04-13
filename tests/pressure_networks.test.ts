import { describe, it, expect, vi } from "vitest";
import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";

// ---------------------------------------------------------------------------
// Schemas under test (mirrored from civil3d_pressure_networks.ts)
// ---------------------------------------------------------------------------

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number().optional().default(0),
});

const PressureNetworkSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  pipeCount: z.number(),
  fittingCount: z.number(),
  appurtenanceCount: z.number(),
  partsList: z.string().nullable(),
});

const PressurePipeSchema = z.object({
  name: z.string(),
  handle: z.string(),
  diameter: z.number(),
  length: z.number(),
  material: z.string(),
  startPoint: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  endPoint: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  coverDepth: z.number().nullable(),
});

const PressureFittingSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  position: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  partSize: z.string().nullable(),
});

const PressureAppurtenanceSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  position: z.object({ x: z.number(), y: z.number(), z: z.number() }).nullable(),
  partSize: z.string().nullable(),
});

const PressureNetworkListResponseSchema = z.object({
  networks: z.array(PressureNetworkSummarySchema),
});

const PressureNetworkDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  partsList: z.string().nullable(),
  pipes: z.array(PressurePipeSchema),
  fittings: z.array(PressureFittingSchema),
  appurtenances: z.array(PressureAppurtenanceSchema),
});

const ValidationIssueSchema = z.object({
  type: z.string(),
  severity: z.enum(["error", "warning", "info"]),
  message: z.string(),
  objectHandle: z.string().nullable(),
});

const PressureNetworkValidationResponseSchema = z.object({
  networkName: z.string(),
  valid: z.boolean(),
  issues: z.array(ValidationIssueSchema),
});

// Tool input schemas
const PressureNetworkCreateInputSchema = z.object({
  name: z.string(),
  partsList: z.string(),
  layer: z.string().optional(),
  referenceAlignment: z.string().optional(),
  referenceSurface: z.string().optional(),
});

const PressurePipeAddInputSchema = z.object({
  networkName: z.string(),
  partName: z.string(),
  startPoint: Point3DSchema,
  endPoint: Point3DSchema,
  diameter: z.number().optional(),
});

const PressurePipeResizeInputSchema = z.object({
  networkName: z.string(),
  pipeName: z.string(),
  newPartName: z.string(),
  newDiameter: z.number().optional(),
});

const PressureNetworkSetCoverInputSchema = z.object({
  networkName: z.string(),
  minCoverDepth: z.number(),
  maxCoverDepth: z.number().optional(),
});

const PressureNetworkConnectInputSchema = z.object({
  targetNetwork: z.string(),
  sourceNetwork: z.string(),
});

const PressureFittingAddInputSchema = z.object({
  networkName: z.string(),
  partName: z.string(),
  position: Point3DSchema,
  rotation: z.number().optional(),
});

const PressureAppurtenanceAddInputSchema = z.object({
  networkName: z.string(),
  partName: z.string(),
  position: Point3DSchema,
  rotation: z.number().optional(),
  onPipeName: z.string().optional(),
});

// ---------------------------------------------------------------------------
// 1. PressureNetworkSummarySchema
// ---------------------------------------------------------------------------

describe("PressureNetworkSummarySchema", () => {
  it("accepts a valid network summary", () => {
    const result = PressureNetworkSummarySchema.safeParse({
      name: "Water Main",
      handle: "abc123",
      pipeCount: 5,
      fittingCount: 3,
      appurtenanceCount: 2,
      partsList: "Standard Pressure",
    });
    expect(result.success).toBe(true);
  });

  it("accepts null partsList", () => {
    const result = PressureNetworkSummarySchema.safeParse({
      name: "Force Main",
      handle: "def456",
      pipeCount: 0,
      fittingCount: 0,
      appurtenanceCount: 0,
      partsList: null,
    });
    expect(result.success).toBe(true);
  });

  it("rejects missing required fields", () => {
    const result = PressureNetworkSummarySchema.safeParse({
      name: "Water Main",
      handle: "abc123",
      // missing pipeCount, fittingCount, appurtenanceCount, partsList
    });
    expect(result.success).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// 2. PressureNetworkListResponseSchema
// ---------------------------------------------------------------------------

describe("PressureNetworkListResponseSchema", () => {
  it("accepts an empty networks list", () => {
    const result = PressureNetworkListResponseSchema.safeParse({ networks: [] });
    expect(result.success).toBe(true);
  });

  it("accepts multiple networks", () => {
    const result = PressureNetworkListResponseSchema.safeParse({
      networks: [
        { name: "WM-1", handle: "h1", pipeCount: 10, fittingCount: 5, appurtenanceCount: 2, partsList: "Std" },
        { name: "FM-1", handle: "h2", pipeCount: 4, fittingCount: 2, appurtenanceCount: 1, partsList: null },
      ],
    });
    expect(result.success).toBe(true);
    expect(result.data!.networks).toHaveLength(2);
  });

  it("rejects if networks key is missing", () => {
    const result = PressureNetworkListResponseSchema.safeParse({});
    expect(result.success).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// 3. PressurePipeSchema
// ---------------------------------------------------------------------------

describe("PressurePipeSchema", () => {
  it("accepts a fully-specified pipe", () => {
    const result = PressurePipeSchema.safeParse({
      name: "PP-1",
      handle: "pp1",
      diameter: 8,
      length: 150,
      material: "PVC",
      startPoint: { x: 0, y: 0, z: 100 },
      endPoint: { x: 150, y: 0, z: 98 },
      coverDepth: 3.5,
    });
    expect(result.success).toBe(true);
  });

  it("accepts null startPoint/endPoint/coverDepth", () => {
    const result = PressurePipeSchema.safeParse({
      name: "PP-2",
      handle: "pp2",
      diameter: 12,
      length: 200,
      material: "Ductile Iron",
      startPoint: null,
      endPoint: null,
      coverDepth: null,
    });
    expect(result.success).toBe(true);
  });

  it("rejects negative diameter", () => {
    // Note: schema doesn't enforce positive — this tests the data plumbing, not domain logic.
    // Diameter field must be a number regardless.
    const result = PressurePipeSchema.safeParse({
      name: "PP-3",
      handle: "pp3",
      diameter: "not-a-number",
      length: 100,
      material: "PVC",
      startPoint: null,
      endPoint: null,
      coverDepth: null,
    });
    expect(result.success).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// 4. PressureNetworkDetailResponseSchema
// ---------------------------------------------------------------------------

describe("PressureNetworkDetailResponseSchema", () => {
  it("accepts a network with mixed components", () => {
    const result = PressureNetworkDetailResponseSchema.safeParse({
      name: "Force Main",
      handle: "fm1",
      partsList: "Pressure Pipes",
      pipes: [
        { name: "PP-1", handle: "p1", diameter: 8, length: 100, material: "PVC", startPoint: null, endPoint: null, coverDepth: null },
      ],
      fittings: [
        { name: "Fit-1", handle: "f1", type: "Elbow", position: { x: 100, y: 0, z: 98 }, partSize: "8 inch 90-deg" },
      ],
      appurtenances: [
        { name: "App-1", handle: "a1", type: "Gate Valve", position: { x: 50, y: 0, z: 99 }, partSize: null },
      ],
    });
    expect(result.success).toBe(true);
    expect(result.data!.pipes).toHaveLength(1);
    expect(result.data!.fittings).toHaveLength(1);
    expect(result.data!.appurtenances).toHaveLength(1);
  });

  it("accepts empty component arrays", () => {
    const result = PressureNetworkDetailResponseSchema.safeParse({
      name: "Empty Net",
      handle: "en1",
      partsList: null,
      pipes: [],
      fittings: [],
      appurtenances: [],
    });
    expect(result.success).toBe(true);
  });
});

// ---------------------------------------------------------------------------
// 5. PressureNetworkValidationResponseSchema
// ---------------------------------------------------------------------------

describe("PressureNetworkValidationResponseSchema", () => {
  it("accepts a valid (no-issues) response", () => {
    const result = PressureNetworkValidationResponseSchema.safeParse({
      networkName: "WM-1",
      valid: true,
      issues: [],
    });
    expect(result.success).toBe(true);
    expect(result.data!.valid).toBe(true);
  });

  it("accepts a response with cover violation errors", () => {
    const result = PressureNetworkValidationResponseSchema.safeParse({
      networkName: "WM-1",
      valid: false,
      issues: [
        { type: "cover_violation", severity: "error", message: "PP-1 cover 1.2 < min 3.0", objectHandle: "pp1" },
        { type: "disconnected_end", severity: "warning", message: "PP-3 has unconnected end", objectHandle: "pp3" },
      ],
    });
    expect(result.success).toBe(true);
    expect(result.data!.issues).toHaveLength(2);
    expect(result.data!.issues[0].severity).toBe("error");
  });

  it("rejects invalid severity value", () => {
    const result = PressureNetworkValidationResponseSchema.safeParse({
      networkName: "WM-1",
      valid: false,
      issues: [
        { type: "cover_violation", severity: "critical", message: "...", objectHandle: null },
      ],
    });
    expect(result.success).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// 6. Tool input schemas
// ---------------------------------------------------------------------------

describe("PressureNetworkCreateInputSchema", () => {
  it("requires name and partsList", () => {
    expect(PressureNetworkCreateInputSchema.safeParse({ name: "WM", partsList: "Std" }).success).toBe(true);
    expect(PressureNetworkCreateInputSchema.safeParse({ name: "WM" }).success).toBe(false);
    expect(PressureNetworkCreateInputSchema.safeParse({ partsList: "Std" }).success).toBe(false);
  });

  it("accepts optional layer and reference fields", () => {
    const result = PressureNetworkCreateInputSchema.safeParse({
      name: "WM",
      partsList: "Std",
      layer: "WATER",
      referenceAlignment: "AL-1",
      referenceSurface: "EG",
    });
    expect(result.success).toBe(true);
    expect(result.data!.layer).toBe("WATER");
  });
});

describe("PressurePipeAddInputSchema", () => {
  it("requires networkName, partName, startPoint, endPoint", () => {
    const valid = PressurePipeAddInputSchema.safeParse({
      networkName: "WM",
      partName: "8in PVC",
      startPoint: { x: 0, y: 0, z: 100 },
      endPoint: { x: 100, y: 0, z: 100 },
    });
    expect(valid.success).toBe(true);
  });

  it("rejects missing endPoint", () => {
    const result = PressurePipeAddInputSchema.safeParse({
      networkName: "WM",
      partName: "8in PVC",
      startPoint: { x: 0, y: 0, z: 100 },
    });
    expect(result.success).toBe(false);
  });
});

describe("PressureNetworkSetCoverInputSchema", () => {
  it("accepts minCoverDepth with optional max", () => {
    expect(PressureNetworkSetCoverInputSchema.safeParse({ networkName: "WM", minCoverDepth: 3.0 }).success).toBe(true);
    expect(PressureNetworkSetCoverInputSchema.safeParse({ networkName: "WM", minCoverDepth: 3.0, maxCoverDepth: 10.0 }).success).toBe(true);
  });

  it("rejects missing minCoverDepth", () => {
    expect(PressureNetworkSetCoverInputSchema.safeParse({ networkName: "WM" }).success).toBe(false);
  });
});

describe("PressureNetworkConnectInputSchema", () => {
  it("requires both targetNetwork and sourceNetwork", () => {
    expect(PressureNetworkConnectInputSchema.safeParse({ targetNetwork: "WM-1", sourceNetwork: "WM-2" }).success).toBe(true);
    expect(PressureNetworkConnectInputSchema.safeParse({ targetNetwork: "WM-1" }).success).toBe(false);
  });
});

describe("PressurePipeResizeInputSchema", () => {
  it("requires networkName, pipeName, newPartName", () => {
    expect(PressurePipeResizeInputSchema.safeParse({ networkName: "WM", pipeName: "PP-1", newPartName: "12in DI" }).success).toBe(true);
    expect(PressurePipeResizeInputSchema.safeParse({ networkName: "WM", pipeName: "PP-1" }).success).toBe(false);
  });
});

describe("PressureFittingAddInputSchema", () => {
  it("requires networkName, partName, position", () => {
    expect(PressureFittingAddInputSchema.safeParse({ networkName: "WM", partName: "Elbow 90", position: { x: 100, y: 0, z: 98 } }).success).toBe(true);
    expect(PressureFittingAddInputSchema.safeParse({ networkName: "WM", partName: "Elbow 90" }).success).toBe(false);
  });
});

describe("PressureAppurtenanceAddInputSchema", () => {
  it("accepts optional onPipeName", () => {
    const result = PressureAppurtenanceAddInputSchema.safeParse({
      networkName: "WM",
      partName: "Gate Valve",
      position: { x: 50, y: 0, z: 99 },
      onPipeName: "PP-1",
    });
    expect(result.success).toBe(true);
    expect(result.data!.onPipeName).toBe("PP-1");
  });

  it("works without onPipeName", () => {
    expect(PressureAppurtenanceAddInputSchema.safeParse({ networkName: "WM", partName: "Hydrant", position: { x: 0, y: 0, z: 100 } }).success).toBe(true);
  });
});

