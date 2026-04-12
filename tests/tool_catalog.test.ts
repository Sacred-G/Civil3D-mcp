import { describe, it, expect } from "vitest";
import { TOOL_CATALOG, listToolCatalog, listDomains } from "../src/tools/tool_catalog.js";

describe("Tool Catalog", () => {
  it("should have entries for all registered tools", () => {
    expect(TOOL_CATALOG.length).toBeGreaterThan(0);
  });

  it("should have unique tool names", () => {
    const names = TOOL_CATALOG.map((e) => e.toolName);
    const unique = new Set(names);
    expect(unique.size).toBe(names.length);
  });

  it("every entry should have required fields", () => {
    for (const entry of TOOL_CATALOG) {
      expect(entry.toolName).toBeTruthy();
      expect(entry.displayName).toBeTruthy();
      expect(entry.description).toBeTruthy();
      expect(entry.domain).toBeTruthy();
      expect(entry.capabilities.length).toBeGreaterThan(0);
      expect(typeof entry.requiresActiveDrawing).toBe("boolean");
      expect(typeof entry.safeForRetry).toBe("boolean");
      expect(["implemented", "planned"]).toContain(entry.status);
    }
  });

  it("listToolCatalog returns the full catalog", () => {
    expect(listToolCatalog()).toBe(TOOL_CATALOG);
  });

  it("listDomains returns sorted unique domains", () => {
    const domains = listDomains();
    expect(domains.length).toBeGreaterThan(0);
    // Verify sorted
    const sorted = [...domains].sort();
    expect(domains).toEqual(sorted);
    // Verify unique
    expect(new Set(domains).size).toBe(domains.length);
  });

  it("hydrology tool catalog entry includes new operations", () => {
    const hydrology = TOOL_CATALOG.find((e) => e.toolName === "civil3d_hydrology");
    expect(hydrology).toBeDefined();
    expect(hydrology!.domain).toBe("hydrology");
    expect(hydrology!.status).toBe("implemented");
  });

  it("includes newly added entries across major sections", () => {
    const requiredTools = [
      "list_tool_capabilities",
      "civil3d_alignment_add_tangent",
      "civil3d_profile_add_pvi",
      "civil3d_corridor_target_mapping_get",
      "civil3d_section_view_create",
      "civil3d_point_group_create",
      "civil3d_parcel_create",
      "civil3d_pressure_network_list",
      "civil3d_pipe_network_size",
      "civil3d_pipe_profile_view_automation",
      "civil3d_data_shortcut_create",
      "civil3d_intersection_list",
      "civil3d_superelevation_get",
      "civil3d_qc_check_alignment",
      "civil3d_qc_fix_drawing_standards",
      "civil3d_qty_corridor_volumes",
      "civil3d_survey_observation_list",
      "civil3d_sheet_set_list",
      "civil3d_surface_statistics_get",
      "civil3d_surface_volume_calculate",
      "civil3d_sight_distance_calculate",
      "civil3d_detention_basin_size_calculate",
      "civil3d_slope_geometry_calculate",
      "civil3d_material_cost_estimate",
    ];

    for (const toolName of requiredTools) {
      const entry = TOOL_CATALOG.find((e) => e.toolName === toolName);
      expect(entry, `${toolName} should be present in TOOL_CATALOG`).toBeDefined();
      expect(entry!.status).toBe("implemented");
    }
  });
});
