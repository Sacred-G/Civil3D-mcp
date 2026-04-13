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
    expect(hydrology!.operations).toContain("calculate_tc");
    expect(hydrology!.operations).toContain("export_stm");
    expect(hydrology!.operations).toContain("watershed_runoff_workflow");
  });

  it("hydrology family compatibility tools now resolve to the hydrology domain", () => {
    const catchment = TOOL_CATALOG.find((e) => e.toolName === "civil3d_catchment");
    const tc = TOOL_CATALOG.find((e) => e.toolName === "civil3d_time_of_concentration");
    const stm = TOOL_CATALOG.find((e) => e.toolName === "civil3d_stm");

    expect(catchment).toBeDefined();
    expect(catchment!.domain).toBe("hydrology");
    expect(tc).toBeDefined();
    expect(tc!.domain).toBe("hydrology");
    expect(stm).toBeDefined();
    expect(stm!.domain).toBe("hydrology");
  });

  it("includes canonical standalone domain entries for migrated analysis and takeoff tools", () => {
    const requiredCanonicalTools = [
      "civil3d_workflow",
      "civil3d_quantity_takeoff",
      "civil3d_superelevation",
      "civil3d_intersection",
      "civil3d_sight_distance",
      "civil3d_detention",
      "civil3d_slope_analysis",
      "civil3d_cost_estimation",
      "civil3d_geometry",
      "civil3d_drawing",
      "civil3d_coordinate_system",
      "civil3d_job",
      "civil3d_docs",
      "civil3d_health",
    ];

    for (const toolName of requiredCanonicalTools) {
      const entry = TOOL_CATALOG.find((e) => e.toolName === toolName);
      expect(entry, `${toolName} should be present in TOOL_CATALOG`).toBeDefined();
      expect(entry!.status).toBe("implemented");
    }
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
      "civil3d_parcel",
      "civil3d_parcel_edit",
      "civil3d_parcel_lot_line_adjust",
      "civil3d_parcel_report",
      "civil3d_survey",
      "civil3d_survey_database_create",
      "civil3d_survey_figure_list",
      "civil3d_pressure_network_list",
      "civil3d_pipe_network_size",
      "civil3d_pipe_profile_view_automation",
      "civil3d_data_shortcut_create",
      "civil3d_project",
      "civil3d_data_shortcut",
      "civil3d_data_shortcut_sync",
      "civil3d_standards",
      "civil3d_standards_lookup",
      "civil3d_qc_check_drawing_standards",
      "civil3d_qc",
      "civil3d_qc_report_generate",
      "civil3d_workflow_corridor_qc_report",
      "civil3d_workflow_grading_surface_volume",
      "civil3d_workflow_surface_comparison_report",
      "civil3d_workflow_data_shortcut_publish_sync",
      "civil3d_workflow_data_shortcut_reference_sync",
      "civil3d_workflow_project_startup",
      "civil3d_workflow_project_reference_setup",
      "civil3d_workflow_drawing_readiness_audit",
      "civil3d_workflow_feature_line_to_grading",
      "civil3d_workflow_pipe_network_design",
      "civil3d_workflow_plan_production_publish",
      "civil3d_workflow_qc_fix_and_verify",
      "civil3d_workflow_survey_import_adjust_figures",
      "civil3d_intersection_list",
      "civil3d_superelevation_get",
      "civil3d_qc_check_alignment",
      "civil3d_qc_fix_drawing_standards",
      "civil3d_qty_corridor_volumes",
      "civil3d_survey_observation_list",
      "civil3d_plan_production",
      "civil3d_sheet_set_list",
      "civil3d_surface_statistics_get",
      "civil3d_surface_volume_calculate",
      "civil3d_grading",
      "civil3d_grading_group_create",
      "civil3d_feature_line",
      "civil3d_feature_line_create",
      "civil3d_sight_distance_calculate",
      "civil3d_detention_basin_size_calculate",
      "civil3d_slope_geometry_calculate",
      "civil3d_material_cost_estimate",
      "civil3d_quantity_takeoff",
      "civil3d_superelevation",
      "civil3d_intersection",
      "civil3d_sight_distance",
      "civil3d_detention",
      "civil3d_slope_analysis",
      "civil3d_cost_estimation",
      "civil3d_geometry",
      "get_drawing_info",
      "get_selected_civil_objects_info",
      "civil3d_coordinate_system",
      "civil3d_job",
      "list_civil_object_types",
      "civil3d_orchestrate",
      "civil3d_cogo_inverse",
      "civil3d_cogo_curve_solve",
      "create_line_segment",
      "acad_create_polyline",
      "acad_create_text",
      "acad_create_3dpolyline",
      "acad_create_mtext",
    ];

    for (const toolName of requiredTools) {
      const entry = TOOL_CATALOG.find((e) => e.toolName === toolName);
      expect(entry, `${toolName} should be present in TOOL_CATALOG`).toBeDefined();
      expect(entry!.status).toBe("implemented");
    }
  });
});
