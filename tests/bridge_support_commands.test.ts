import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";
import { isApprovalRequired } from "../src/tools/approvalPolicy.js";
import type { DomainToolDefinition } from "../src/tools/domainRuntime.js";
import {
  DRAWING_RUNTIME_DOMAIN_DEFINITION,
  DrawingUnitsResponseSchema,
} from "../src/tools/domains/drawingRuntimeDomain.js";
import {
  PARCEL_DOMAIN_DEFINITION,
  ParcelGeometryArgsSchema,
  ParcelGeometryResponseSchema,
} from "../src/tools/domains/parcelDomain.js";
import { PIPE_DOMAIN_DEFINITION } from "../src/tools/domains/pipeDomain.js";
import {
  SURFACE_DOMAIN_DEFINITION,
  SurfaceTinVerticesArgsSchema,
  SurfaceTinVerticesResponseSchema,
  TIN_VERTICES_MAX_POINTS,
} from "../src/tools/domains/surfaceDomain.js";
import { GENERATED_TOOL_CATALOG_ENTRIES } from "../src/tools/toolManifest.js";

// The read-only commands the Civil 3D <-> Revit bridge calls directly on the
// plugin (bypassing this server's approval gate), exposed here as MCP actions.

function pluginSource(fileName: string): string {
  return readFileSync(new URL(`../Civil3D-MCP-Plugin/${fileName}`, import.meta.url), "utf8");
}

function approvalFor(definition: DomainToolDefinition, toolName: string, action: string): boolean {
  const actionDefinition = definition.actions[action];
  expect(actionDefinition, `${toolName}.${action} is defined`).toBeDefined();
  return isApprovalRequired({
    toolName,
    action,
    capabilities: actionDefinition.capabilities,
    safeForRetry: actionDefinition.safeForRetry,
    requiresActiveDrawing: actionDefinition.requiresActiveDrawing,
  });
}

const NEW_ACTIONS: Array<[DomainToolDefinition, string, string, string]> = [
  [SURFACE_DOMAIN_DEFINITION, "civil3d_surface", "get_tin_vertices", "getSurfaceTinVertices"],
  [PARCEL_DOMAIN_DEFINITION, "civil3d_parcel", "get_geometry", "getParcelGeometry"],
  [DRAWING_RUNTIME_DOMAIN_DEFINITION, "civil3d_drawing", "units", "getDrawingUnits"],
];

describe("bridge-support commands", () => {
  it.each(NEW_ACTIONS)("%#: %s.%s is an ungated, retry-safe read on the canonical tool", (definition, toolName, action, method) => {
    const actionDefinition = definition.actions[action];
    expect(actionDefinition.pluginMethods).toEqual([method]);
    expect(actionDefinition.safeForRetry).toBe(true);
    expect(actionDefinition.capabilities).toEqual(["query", "inspect"]);
    expect(approvalFor(definition, toolName, action)).toBe(false);

    const entry = GENERATED_TOOL_CATALOG_ENTRIES.find((item) => item.toolName === toolName);
    expect(entry?.operations).toContain(action);
    expect(entry?.pluginMethods).toContain(method);
  });

  it("dispatches the new plugin methods natively", () => {
    const dispatcher = pluginSource("CommandDispatcher.cs");
    for (const method of ["getSurfaceTinVertices", "getParcelGeometry", "getDrawingUnits"]) {
      expect(dispatcher).toMatch(new RegExp(`"${method}"\\s*=>`));
    }
  });

  describe("getSurfaceTinVertices", () => {
    it("validates name, boundary polygon and the maxPoints cap", () => {
      expect(SurfaceTinVerticesArgsSchema.safeParse({ action: "get_tin_vertices", name: "EG" }).success).toBe(true);
      expect(SurfaceTinVerticesArgsSchema.safeParse({
        action: "get_tin_vertices",
        name: "EG",
        boundary: [{ x: 0, y: 0 }, { x: 10, y: 0 }, { x: 10, y: 10 }],
        maxPoints: TIN_VERTICES_MAX_POINTS,
      }).success).toBe(true);
      expect(SurfaceTinVerticesArgsSchema.safeParse({ action: "get_tin_vertices", name: "" }).success).toBe(false);
      expect(SurfaceTinVerticesArgsSchema.safeParse({ action: "get_tin_vertices", name: "EG", boundary: [{ x: 0, y: 0 }, { x: 1, y: 1 }] }).success).toBe(false);
      expect(SurfaceTinVerticesArgsSchema.safeParse({ action: "get_tin_vertices", name: "EG", maxPoints: TIN_VERTICES_MAX_POINTS + 1 }).success).toBe(false);
      expect(SurfaceTinVerticesArgsSchema.safeParse({ action: "get_tin_vertices", name: "EG", maxPoints: 0 }).success).toBe(false);
    });

    it("matches the plugin cap and the bridge's request size", () => {
      // A bridge client asks for up to 100,000 vertices in one call.
      expect(TIN_VERTICES_MAX_POINTS).toBe(100_000);
      expect(pluginSource("SurfaceCommands.cs")).toMatch(/TinVerticesMaxPoints = 100_000;/);
    });

    it("accepts the plugin response shape the bridge reads", () => {
      const parsed = SurfaceTinVerticesResponseSchema.parse({
        surfaceName: "EG",
        surfaceHandle: "2A1",
        surfaceType: "TIN",
        vertexSource: "tin_visible_triangles",
        vertices: [{ x: 6012345.123457, y: 2012345.987654, z: 101.25 }],
        totalVertexCount: 250_000,
        returnedVertexCount: 1,
        surfaceVertexCount: 260_000,
        truncated: true,
        maxPoints: 1,
        decimation: "stride",
        decimationStride: 250_000,
        boundaryApplied: false,
        coordinateDecimals: 6,
        units: "feet",
        lengthUnit: "USSurveyFeet",
      });
      expect(parsed.vertices[0]).toEqual({ x: 6012345.123457, y: 2012345.987654, z: 101.25 });
      expect(parsed.truncated).toBe(true);
      // Every other required field is valid, so only the surfaceType enum can reject it.
      const volumeResponse = { surfaceName: "EG", surfaceType: "TINVolume", vertices: [], totalVertexCount: 0, returnedVertexCount: 0, truncated: false, maxPoints: 50_000, decimation: "none", boundaryApplied: false };
      expect(SurfaceTinVerticesResponseSchema.safeParse(volumeResponse).success).toBe(false);
      expect(SurfaceTinVerticesResponseSchema.safeParse({ ...volumeResponse, surfaceType: "TIN" }).success).toBe(true);
    });

    it("emits every field the bridge adapter consumes", () => {
      const source = pluginSource("SurfaceCommands.cs");
      for (const key of ["surfaceName", "vertices", "totalVertexCount", "truncated", "units"]) {
        expect(source).toContain(`["${key}"]`);
      }
    });
  });

  describe("getParcelGeometry", () => {
    it("validates site, parcel and densification angle", () => {
      expect(ParcelGeometryArgsSchema.safeParse({ action: "get_geometry", siteName: "Site 1", parcelName: "Lot 1" }).success).toBe(true);
      expect(ParcelGeometryArgsSchema.safeParse({ action: "get_geometry", siteName: "Site 1", parcelName: "Lot 1", maxArcSegmentAngle: 2 }).success).toBe(true);
      expect(ParcelGeometryArgsSchema.safeParse({ action: "get_geometry", siteName: "Site 1" }).success).toBe(false);
      expect(ParcelGeometryArgsSchema.safeParse({ action: "get_geometry", siteName: "Site 1", parcelName: "Lot 1", maxArcSegmentAngle: 180 }).success).toBe(false);
    });

    it("accepts a boundary with an arc and requires at least three vertices", () => {
      const response = {
        siteName: "Site 1",
        name: "Lot 1",
        handle: "3B2",
        number: 1,
        vertices: [{ x: 0, y: 0 }, { x: 20, y: 0 }, { x: 20, y: 20 }, { x: 10, y: 24.142 }, { x: 0, y: 20 }],
        closed: true,
        orientation: "ccw",
        boundaryVertices: [
          { x: 0, y: 0, bulge: 0 },
          { x: 20, y: 0, bulge: 0 },
          { x: 20, y: 20, bulge: 0.41421356 },
          { x: 0, y: 20, bulge: 0 },
        ],
        segments: [
          { type: "line", start: { x: 0, y: 0 }, end: { x: 20, y: 0 }, bulge: 0, length: 20 },
          { type: "arc", start: { x: 20, y: 20 }, end: { x: 0, y: 20 }, bulge: 0.41421356, length: 22.214, center: { x: 10, y: 10 }, radius: 14.142, sweepAngleDeg: 90 },
        ],
        hasArcs: true,
        maxArcSegmentAngle: 5,
        area: 457.08,
        perimeter: 82.214,
        computedArea: 457.08,
        reportedPerimeter: null,
        centroid: { x: 10, y: 11 },
        geometrySource: "baseCurve:Polyline",
        units: "feet",
        lengthUnit: "Feet",
        notes: [],
      };
      expect(ParcelGeometryResponseSchema.safeParse(response).success).toBe(true);
      expect(ParcelGeometryResponseSchema.safeParse({ ...response, vertices: response.vertices.slice(0, 2) }).success).toBe(false);
    });

    it("reads the boundary through the typed curve API, not reflection", () => {
      const source = pluginSource("ParcelEditingCommands.cs");
      const start = source.indexOf("public static Task<object?> GetParcelGeometryAsync");
      const end = source.indexOf("// Helpers", start);
      const body = source.slice(start, end);
      expect(start).toBeGreaterThan(0);
      expect(body).not.toMatch(/InvokeMethod|GetDoubleProperty|GetNamedMember/);
      for (const key of ["name", "vertices", "closed", "units", "area", "perimeter"]) {
        expect(body).toContain(`["${key}"]`);
      }
    });
  });

  describe("getDrawingUnits", () => {
    it("distinguishes US survey feet from international feet", () => {
      const base = {
        insunits: 21,
        insunitsName: "USSurveyFeet",
        lengthUnit: "USSurveyFeet",
        lengthUnitSource: "INSUNITS",
        isUsSurveyFoot: true,
        metersPerUnit: 1200 / 3937,
        mmPerUnit: 1_200_000 / 3937,
        linearUnits: "feet",
        civilLinearUnit: "Feet",
        civilImperialToMetricConversion: "UsSurveyFoot",
        civilLengthUnit: "USSurveyFeet",
        unitsConsistent: true,
        warnings: [],
      };
      expect(DrawingUnitsResponseSchema.safeParse(base).success).toBe(true);
      expect(DrawingUnitsResponseSchema.safeParse({
        ...base,
        insunits: 2,
        insunitsName: "Feet",
        lengthUnit: "Feet",
        isUsSurveyFoot: false,
        metersPerUnit: 0.3048,
        mmPerUnit: 304.8,
        unitsConsistent: false,
        warnings: ["INSUNITS is Feet but the Civil 3D drawing settings use the US survey foot ..."],
      }).success).toBe(true);
      expect(DrawingUnitsResponseSchema.safeParse({ ...base, lengthUnitSource: "guess" }).success).toBe(false);
    });

    it("keeps drawing info's legacy linearUnits and passes the additive lengthUnit through", () => {
      const info = DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.info.responseSchema!;
      const parsed = info.parse({ linearUnits: "feet", lengthUnit: "USSurveyFeet", units: "feet" }) as Record<string, unknown>;
      expect(parsed.linearUnits).toBe("feet");
      expect(parsed.lengthUnit).toBe("USSurveyFeet");
      expect(pluginSource("DrawingCommands.cs")).toContain('["lengthUnit"] = ResolveLengthUnit(civilDoc, database)');
      expect(pluginSource("CoordinateSystemCommands.cs")).toContain('["lengthUnit"]');
    });
  });

  describe("getPipeNetwork pipe geometry", () => {
    it("passes the additive pipe fields through the pipe domain untouched", () => {
      const pipe = {
        name: "P1",
        diameter: 1.5,
        centerlineStartElevation: 100.75,
        centerlineEndElevation: 99.75,
        startPoint: { x: 1000, y: 2000, z: 100.75 },
        endPoint: { x: 1100, y: 2000, z: 99.75 },
        startInvert: 100,
        endInvert: 99,
        innerDiameter: 1.5,
        outerDiameter: 1.8,
      };
      const parsed = PIPE_DOMAIN_DEFINITION.actions.get.responseSchema!.parse({ name: "Storm", pipes: [pipe] }) as { pipes: unknown[] };
      expect(parsed.pipes[0]).toEqual(pipe);
    });

    it("emits the centreline endpoints, inverts and diameters the bridge prefers", () => {
      const source = pluginSource("PipeNetworkCommands.cs");
      const start = source.indexOf("private static Dictionary<string, object?> ToPipeData");
      const body = source.slice(start, source.indexOf("private static Dictionary<string, object?> ToStructureData", start));
      for (const key of ["startPoint", "endPoint", "startInvert", "endInvert", "innerDiameter", "outerDiameter", "diameter", "centerlineStartElevation"]) {
        expect(body).toContain(`["${key}"]`);
      }
      // InnerDiameterOrWidth stands in for a missing InnerHeight only on circular sections.
      expect(body).toContain('string.Equals(shape, "Circular", StringComparison.Ordinal) ? pipe.InnerDiameterOrWidth : null');
      expect(body).not.toMatch(/\?\s*h\s*:\s*pipe\.InnerDiameterOrWidth;/);
    });
  });

  describe("coordinate system code reads", () => {
    it("never looks up the coordinate-system library for the bare code, and tolerates a failed lookup", () => {
      const source = pluginSource("DrawingCommands.cs");
      const codeStart = source.indexOf("private static string? ReadCoordinateSystemCode");
      expect(codeStart).toBeGreaterThan(0);
      const codeEnd = source.indexOf("private static (string? code", codeStart);
      expect(codeEnd).toBeGreaterThan(codeStart);
      const codeBody = source.slice(codeStart, codeEnd);
      expect(codeBody).not.toContain("GetCoordinateSystemByCode");
      const fieldsStart = source.indexOf("private static (string? code, string? zone");
      const fieldsBody = source.slice(fieldsStart);
      expect(fieldsBody).toMatch(/try\s*\{\s*var coordinateSystem = SettingsUnitZone\.GetCoordinateSystemByCode\(code\);/);
    });
  });
});
