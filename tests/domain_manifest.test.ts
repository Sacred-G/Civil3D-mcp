import { describe, expect, it } from "vitest";
import { GENERATED_TOOL_CATALOG_ENTRIES, MIGRATED_DOMAIN_DEFINITIONS } from "../src/tools/toolManifest.js";
import { TOOL_CATALOG } from "../src/tools/tool_catalog.js";

describe("domain manifest migration", () => {
  it("registers unique exposure names across migrated domains", () => {
    const toolNames = MIGRATED_DOMAIN_DEFINITIONS.flatMap((definition) =>
      definition.exposures.map((exposure) => exposure.toolName),
    );

    expect(new Set(toolNames).size).toBe(toolNames.length);
  });

  it("generates expanded canonical alignment operations", () => {
    const alignment = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_alignment");

    expect(alignment).toBeDefined();
    expect(alignment!.operations).toContain("report");
    expect(alignment!.operations).toContain("add_tangent");
    expect(alignment!.operations).toContain("widen_transition");
    expect(alignment!.safeForRetry).toBe(false);
  });

  it("generates expanded canonical surface operations and legacy surface_edit exposure", () => {
    const surface = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_surface");
    const surfaceEdit = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_surface_edit");

    expect(surface).toBeDefined();
    expect(surface!.operations).toContain("comparison_workflow");
    expect(surface!.operations).toContain("drainage_workflow");
    expect(surface!.operations).toContain("volume_calculate");
    expect(surfaceEdit).toBeDefined();
    expect(surfaceEdit!.operations).toContain("compute_volume");
  });

  it("generates canonical profile domain with all actions and compatibility aliases", () => {
    const profile = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_profile");
    const profileReport = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_profile_report");
    const profileAddPvi = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_profile_add_pvi");
    const profileCheckK = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_profile_check_k_values");
    const profileViewCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((entry) => entry.toolName === "civil3d_profile_view_create");

    expect(profile).toBeDefined();
    expect(profile!.operations).toContain("report");
    expect(profile!.operations).toContain("add_pvi");
    expect(profile!.operations).toContain("check_k_values");
    expect(profile!.operations).toContain("view_create");
    expect(profile!.operations).toContain("view_band_set");
    expect(profile!.safeForRetry).toBe(false);

    expect(profileReport).toBeDefined();
    // single-action exposures have no operations list — the tool name IS the operation
    expect(profileReport!.operations).toBeUndefined();

    expect(profileAddPvi).toBeDefined();
    expect(profileCheckK).toBeDefined();
    expect(profileViewCreate).toBeDefined();
  });

  it("generates canonical corridor domain with summary and editing action aliases", () => {
    const corridor = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_corridor");
    const corridorSummary = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_corridor_summary");
    const targetGet = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_corridor_target_mapping_get");
    const regionAdd = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_corridor_region_add");

    expect(corridor).toBeDefined();
    expect(corridor!.operations).toContain("summary");
    expect(corridor!.operations).toContain("target_mapping_get");
    expect(corridor!.operations).toContain("target_mapping_set");
    expect(corridor!.operations).toContain("region_add");
    expect(corridor!.operations).toContain("region_delete");
    expect(corridor!.safeForRetry).toBe(false);

    expect(corridorSummary).toBeDefined();
    expect(corridorSummary!.operations).toBeUndefined();

    expect(targetGet).toBeDefined();
    expect(regionAdd).toBeDefined();
  });

  it("generates canonical section domain with view action aliases", () => {
    const section = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_section");
    const viewCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_section_view_create");
    const viewExport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_section_view_export");

    expect(section).toBeDefined();
    expect(section!.operations).toContain("list_sample_lines");
    expect(section!.operations).toContain("create_sample_lines");
    expect(section!.operations).toContain("view_create");
    expect(section!.operations).toContain("view_group_create");
    expect(section!.operations).toContain("view_export");

    expect(viewCreate).toBeDefined();
    expect(viewExport).toBeDefined();
  });

  it("generates canonical pipe domain with gravity, pressure, and automation aliases", () => {
    const pipe = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_pipe");
    const pipeNetwork = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_pipe_network");
    const pressureList = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_pressure_network_list");
    const pipeSize = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_pipe_network_size");
    const pipeProfileView = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_pipe_profile_view_automation");

    expect(pipe).toBeDefined();
    expect(pipe!.operations).toContain("list");
    expect(pipe!.operations).toContain("catalog_list");
    expect(pipe!.operations).toContain("calculate_hgl");
    expect(pipe!.operations).toContain("size_network");
    expect(pipe!.operations).toContain("create_pressure_network");
    expect(pipe!.operations).toContain("add_pressure_pipe");
    expect(pipe!.safeForRetry).toBe(false);

    expect(pipeNetwork).toBeDefined();
    expect(pressureList).toBeDefined();
    expect(pipeSize).toBeDefined();
    expect(pipeProfileView).toBeDefined();
  });

  it("generates canonical assembly domain with creation and edit aliases", () => {
    const assembly = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_assembly");
    const assemblyCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_assembly_create");
    const subassemblyCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_subassembly_create");
    const assemblyEdit = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_assembly_edit");

    expect(assembly).toBeDefined();
    expect(assembly!.operations).toContain("list");
    expect(assembly!.operations).toContain("get");
    expect(assembly!.operations).toContain("create");
    expect(assembly!.operations).toContain("create_subassembly");
    expect(assembly!.operations).toContain("edit");
    expect(assembly!.safeForRetry).toBe(false);

    expect(assemblyCreate).toBeDefined();
    expect(subassemblyCreate).toBeDefined();
    expect(assemblyEdit).toBeDefined();
  });

  it("generates canonical point domain with point-group and export aliases", () => {
    const point = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_point");
    const createCogoPoint = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "create_cogo_point");
    const pointGroupCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_point_group_create");
    const pointExport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_point_export");
    const pointTransform = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_point_transform");

    expect(point).toBeDefined();
    expect(point!.operations).toContain("list");
    expect(point!.operations).toContain("create");
    expect(point!.operations).toContain("group_create");
    expect(point!.operations).toContain("group_update");
    expect(point!.operations).toContain("export");
    expect(point!.operations).toContain("transform");
    expect(point!.safeForRetry).toBe(false);

    expect(createCogoPoint).toBeDefined();
    expect(pointGroupCreate).toBeDefined();
    expect(pointExport).toBeDefined();
    expect(pointTransform).toBeDefined();
  });

  it("generates canonical grading domain with grading-group and feature-line aliases", () => {
    const grading = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_grading");
    const featureLine = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_feature_line");
    const gradingGroupCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_grading_group_create");
    const gradingCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_grading_create");
    const featureLineCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_feature_line_create");

    expect(grading).toBeDefined();
    expect(grading!.operations).toContain("group_list");
    expect(grading!.operations).toContain("group_create");
    expect(grading!.operations).toContain("criteria_list");
    expect(grading!.operations).toContain("feature_line_list");
    expect(grading!.operations).toContain("feature_line_create");
    expect(grading!.safeForRetry).toBe(false);

    expect(featureLine).toBeDefined();
    expect(featureLine!.operations).toContain("list");
    expect(featureLine!.operations).toContain("export_as_polyline");

    expect(gradingGroupCreate).toBeDefined();
    expect(gradingCreate).toBeDefined();
    expect(featureLineCreate).toBeDefined();
  });

  it("generates canonical parcel domain with editing and report aliases", () => {
    const parcel = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_parcel");
    const parcelCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_parcel_create");
    const parcelEdit = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_parcel_edit");
    const parcelAdjust = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_parcel_lot_line_adjust");
    const parcelReport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_parcel_report");

    expect(parcel).toBeDefined();
    expect(parcel!.operations).toContain("list_sites");
    expect(parcel!.operations).toContain("create");
    expect(parcel!.operations).toContain("edit");
    expect(parcel!.operations).toContain("lot_line_adjust");
    expect(parcel!.operations).toContain("report");
    expect(parcel!.safeForRetry).toBe(false);

    expect(parcelCreate).toBeDefined();
    expect(parcelEdit).toBeDefined();
    expect(parcelAdjust).toBeDefined();
    expect(parcelReport).toBeDefined();
  });

  it("generates canonical survey domain with database, figure, and processing aliases", () => {
    const survey = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey");
    const databaseCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey_database_create");
    const figureList = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey_figure_list");
    const observationList = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey_observation_list");
    const networkAdjust = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey_network_adjust");
    const landxmlImport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_survey_landxml_import");

    expect(survey).toBeDefined();
    expect(survey!.operations).toContain("database_list");
    expect(survey!.operations).toContain("database_create");
    expect(survey!.operations).toContain("figure_list");
    expect(survey!.operations).toContain("observation_list");
    expect(survey!.operations).toContain("network_adjust");
    expect(survey!.operations).toContain("landxml_import");
    expect(survey!.safeForRetry).toBe(false);

    expect(databaseCreate).toBeDefined();
    expect(figureList).toBeDefined();
    expect(observationList).toBeDefined();
    expect(networkAdjust).toBeDefined();
    expect(landxmlImport).toBeDefined();
  });

  it("generates canonical plan production domain with sheet and publish aliases", () => {
    const planProduction = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_plan_production");
    const sheetSetCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_sheet_set_create");
    const sheetAdd = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_sheet_add");
    const planProfileCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_plan_profile_sheet_create");
    const sheetPublish = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_sheet_publish_pdf");
    const sheetSetExport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_sheet_set_export");

    expect(planProduction).toBeDefined();
    expect(planProduction!.operations).toContain("sheet_set_list");
    expect(planProduction!.operations).toContain("sheet_set_create");
    expect(planProduction!.operations).toContain("sheet_add");
    expect(planProduction!.operations).toContain("plan_profile_sheet_create");
    expect(planProduction!.operations).toContain("sheet_publish_pdf");
    expect(planProduction!.operations).toContain("sheet_set_export");
    expect(planProduction!.safeForRetry).toBe(false);

    expect(sheetSetCreate).toBeDefined();
    expect(sheetAdd).toBeDefined();
    expect(planProfileCreate).toBeDefined();
    expect(sheetPublish).toBeDefined();
    expect(sheetSetExport).toBeDefined();
  });

  it("generates canonical workflow domain with composite delivery and qc aliases", () => {
    const workflow = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow");
    const corridorQcReport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_corridor_qc_report");
    const gradingSurfaceVolume = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_grading_surface_volume");
    const surfaceComparisonReport = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_surface_comparison_report");
    const dataShortcutPublishSync = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_data_shortcut_publish_sync");
    const dataShortcutReferenceSync = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_data_shortcut_reference_sync");
    const projectStartup = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_project_startup");
    const projectReferenceSetup = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_project_reference_setup");
    const drawingReadinessAudit = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_drawing_readiness_audit");
    const featureLineToGrading = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_feature_line_to_grading");
    const pipeNetworkDesign = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_pipe_network_design");
    const planProductionPublish = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_plan_production_publish");
    const qcFixAndVerify = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_qc_fix_and_verify");
    const surveyImportAdjustFigures = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_workflow_survey_import_adjust_figures");

    expect(workflow).toBeDefined();
    expect(workflow!.domain).toBe("workflow");
    expect(workflow!.operations).toContain("corridor_qc_report");
    expect(workflow!.operations).toContain("grading_surface_volume");
    expect(workflow!.operations).toContain("surface_comparison_report");
    expect(workflow!.operations).toContain("data_shortcut_publish_sync");
    expect(workflow!.operations).toContain("data_shortcut_reference_sync");
    expect(workflow!.operations).toContain("project_startup");
    expect(workflow!.operations).toContain("project_reference_setup");
    expect(workflow!.operations).toContain("drawing_readiness_audit");
    expect(workflow!.operations).toContain("feature_line_to_grading");
    expect(workflow!.operations).toContain("pipe_network_design");
    expect(workflow!.operations).toContain("plan_production_publish");
    expect(workflow!.operations).toContain("qc_fix_and_verify");
    expect(workflow!.operations).toContain("survey_import_adjust_figures");
    expect(workflow!.safeForRetry).toBe(false);

    expect(corridorQcReport).toBeDefined();
    expect(gradingSurfaceVolume).toBeDefined();
    expect(surfaceComparisonReport).toBeDefined();
    expect(dataShortcutPublishSync).toBeDefined();
    expect(dataShortcutReferenceSync).toBeDefined();
    expect(projectStartup).toBeDefined();
    expect(projectReferenceSetup).toBeDefined();
    expect(drawingReadinessAudit).toBeDefined();
    expect(featureLineToGrading).toBeDefined();
    expect(pipeNetworkDesign).toBeDefined();
    expect(planProductionPublish).toBeDefined();
    expect(qcFixAndVerify).toBeDefined();
    expect(surveyImportAdjustFigures).toBeDefined();
  });

  it("generates canonical project domain with data-shortcut aliases", () => {
    const project = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_project");
    const dataShortcut = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_data_shortcut");
    const dataShortcutCreate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_data_shortcut_create");
    const dataShortcutPromote = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_data_shortcut_promote");
    const dataShortcutReference = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_data_shortcut_reference");
    const dataShortcutSync = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_data_shortcut_sync");

    expect(project).toBeDefined();
    expect(project!.operations).toContain("data_shortcut_list");
    expect(project!.operations).toContain("data_shortcut_create");
    expect(project!.operations).toContain("data_shortcut_reference");
    expect(project!.operations).toContain("data_shortcut_sync");
    expect(project!.safeForRetry).toBe(false);

    expect(dataShortcut).toBeDefined();
    expect(dataShortcut!.operations).toContain("list");
    expect(dataShortcut!.operations).toContain("create_reference");
    expect(dataShortcutCreate).toBeDefined();
    expect(dataShortcutPromote).toBeDefined();
    expect(dataShortcutReference).toBeDefined();
    expect(dataShortcutSync).toBeDefined();
  });

  it("generates canonical standards domain with label, style, lookup, and standards-qc aliases", () => {
    const standards = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_standards");
    const label = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_label");
    const style = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_style");
    const lookup = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_standards_lookup");
    const qcLabels = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_check_labels");
    const qcFix = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_fix_drawing_standards");

    expect(standards).toBeDefined();
    expect(standards!.operations).toContain("label_list");
    expect(standards!.operations).toContain("style_get");
    expect(standards!.operations).toContain("lookup");
    expect(standards!.operations).toContain("check_drawing_standards");
    expect(standards!.operations).toContain("fix_drawing_standards");
    expect(standards!.safeForRetry).toBe(false);

    expect(label).toBeDefined();
    expect(style).toBeDefined();
    expect(lookup).toBeDefined();
    expect(qcLabels).toBeDefined();
    expect(qcFix).toBeDefined();
  });

  it("generates canonical qc domain with alignment/profile/corridor/pipe/surface/report aliases", () => {
    const qc = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc");
    const checkAlignment = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_check_alignment");
    const checkProfile = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_check_profile");
    const checkPipe = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_check_pipe_network");
    const report = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_qc_report_generate");

    expect(qc).toBeDefined();
    expect(qc!.operations).toContain("check_alignment");
    expect(qc!.operations).toContain("check_profile");
    expect(qc!.operations).toContain("check_corridor");
    expect(qc!.operations).toContain("check_pipe_network");
    expect(qc!.operations).toContain("check_surface");
    expect(qc!.operations).toContain("generate_report");
    expect(qc!.safeForRetry).toBe(false);

    expect(checkAlignment).toBeDefined();
    expect(checkProfile).toBeDefined();
    expect(checkPipe).toBeDefined();
    expect(report).toBeDefined();
  });

  it("generates canonical hydrology domain with catchment, tc, stm, and workflow aliases", () => {
    const hydrology = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_hydrology");
    const catchment = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_catchment");
    const tc = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_time_of_concentration");
    const stm = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_stm");
    const watershedWorkflow = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_hydrology_watershed_runoff_workflow");

    expect(hydrology).toBeDefined();
    expect(hydrology!.operations).toContain("estimate_runoff");
    expect(hydrology!.operations).toContain("set_catchment_properties");
    expect(hydrology!.operations).toContain("calculate_tc");
    expect(hydrology!.operations).toContain("open_storm_sanitary_analysis");
    expect(hydrology!.operations).toContain("watershed_runoff_workflow");
    expect(hydrology!.safeForRetry).toBe(false);

    expect(catchment).toBeDefined();
    expect(catchment!.domain).toBe("hydrology");
    expect(tc).toBeDefined();
    expect(tc!.domain).toBe("hydrology");
    expect(stm).toBeDefined();
    expect(stm!.domain).toBe("hydrology");
    expect(watershedWorkflow).toBeDefined();
  });

  it("generates canonical standalone domains for quantity, superelevation, intersection, sight distance, detention, slope, and cost", () => {
    const quantity = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_quantity_takeoff");
    const superelevation = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_superelevation");
    const intersection = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_intersection");
    const sightDistance = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_sight_distance");
    const detention = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_detention");
    const slope = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_slope_analysis");
    const cost = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_cost_estimation");

    expect(quantity).toBeDefined();
    expect(quantity!.operations).toContain("corridor_volumes");
    expect(quantity!.operations).toContain("earthwork_summary");
    expect(quantity!.safeForRetry).toBe(false);

    expect(superelevation).toBeDefined();
    expect(superelevation!.operations).toContain("design_check");
    expect(intersection).toBeDefined();
    expect(intersection!.operations).toContain("create");
    expect(sightDistance).toBeDefined();
    expect(sightDistance!.operations).toContain("stopping_distance_check");
    expect(detention).toBeDefined();
    expect(detention!.operations).toContain("stage_storage");
    expect(slope).toBeDefined();
    expect(slope!.operations).toContain("stability_check");
    expect(cost).toBeDefined();
    expect(cost!.operations).toContain("material_cost_estimate");
  });

  it("generates canonical geometry, drawing, coordinate system, and job domains", () => {
    const geometry = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_geometry");
    const drawing = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_drawing");
    const coordinateSystem = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_coordinate_system");
    const job = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_job");

    expect(geometry).toBeDefined();
    expect(geometry!.operations).toContain("cogo_inverse");
    expect(geometry!.operations).toContain("create_mtext");
    expect(geometry!.safeForRetry).toBe(false);

    expect(drawing).toBeDefined();
    expect(drawing!.operations).toContain("info");
    expect(drawing!.operations).toContain("selected_objects_info");
    expect(drawing!.operations).toContain("list_object_types");

    expect(coordinateSystem).toBeDefined();
    expect(coordinateSystem!.operations).toContain("transform");

    expect(job).toBeDefined();
    expect(job!.operations).toContain("cancel");
  });

  it("generates plugin and docs helper domains including orchestrate", () => {
    const plugin = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_health");
    const docs = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_docs");
    const capabilities = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "list_tool_capabilities");
    const orchestrate = GENERATED_TOOL_CATALOG_ENTRIES.find((e) => e.toolName === "civil3d_orchestrate");

    expect(plugin).toBeDefined();
    expect(plugin!.domain).toBe("plugin");
    expect(docs).toBeDefined();
    expect(docs!.operations).toContain("list_tool_capabilities");
    expect(docs!.operations).toContain("orchestrate");
    expect(capabilities).toBeDefined();
    expect(capabilities!.domain).toBe("docs");
    expect(orchestrate).toBeDefined();
    expect(orchestrate!.domain).toBe("docs");
  });

  it("merges generated entries back into the main tool catalog", () => {
    const alignment = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_alignment");
    const surface = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_surface");
    const profile = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_profile");
    const pipe = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_pipe");
    const assembly = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_assembly");
    const point = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_point");
    const grading = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_grading");
    const parcel = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_parcel");
    const survey = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_survey");
    const planProduction = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_plan_production");
    const project = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_project");
    const standards = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_standards");
    const qc = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_qc");
    const hydrology = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_hydrology");
    const catchment = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_catchment");
    const stm = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_stm");
    const quantity = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_quantity_takeoff");
    const superelevation = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_superelevation");
    const intersection = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_intersection");
    const sightDistance = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_sight_distance");
    const detention = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_detention");
    const slope = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_slope_analysis");
    const cost = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_cost_estimation");
    const geometry = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_geometry");
    const drawing = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_drawing");
    const coordinateSystem = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_coordinate_system");
    const job = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_job");
    const plugin = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_health");
    const docs = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_docs");
    const orchestrate = TOOL_CATALOG.find((entry) => entry.toolName === "civil3d_orchestrate");

    expect(alignment).toBeDefined();
    expect(alignment!.description).toContain("single domain tool");
    expect(surface).toBeDefined();
    expect(surface!.operations).toContain("sample_elevations");
    expect(surface!.operations).toContain("create_from_dem");
    expect(profile).toBeDefined();
    expect(profile!.operations).toContain("report");
    expect(profile!.operations).toContain("view_create");
    expect(pipe).toBeDefined();
    expect(pipe!.operations).toContain("list_pressure_networks");
    expect(pipe!.operations).toContain("automate_profile_view");
    expect(assembly).toBeDefined();
    expect(assembly!.operations).toContain("create_subassembly");
    expect(assembly!.operations).toContain("edit");
    expect(point).toBeDefined();
    expect(point!.operations).toContain("group_create");
    expect(point!.operations).toContain("transform");
    expect(grading).toBeDefined();
    expect(grading!.operations).toContain("group_create");
    expect(grading!.operations).toContain("feature_line_create");
    expect(parcel).toBeDefined();
    expect(parcel!.operations).toContain("create");
    expect(parcel!.operations).toContain("report");
    expect(survey).toBeDefined();
    expect(survey!.operations).toContain("database_create");
    expect(survey!.operations).toContain("network_adjust");
    expect(planProduction).toBeDefined();
    expect(planProduction!.operations).toContain("sheet_set_create");
    expect(planProduction!.operations).toContain("sheet_set_export");
    expect(project).toBeDefined();
    expect(project!.operations).toContain("data_shortcut_create");
    expect(project!.operations).toContain("data_shortcut_sync");
    expect(standards).toBeDefined();
    expect(standards!.operations).toContain("lookup");
    expect(standards!.operations).toContain("fix_drawing_standards");
    expect(qc).toBeDefined();
    expect(qc!.operations).toContain("check_alignment");
    expect(qc!.operations).toContain("generate_report");
    expect(hydrology).toBeDefined();
    expect(hydrology!.operations).toContain("calculate_tc");
    expect(hydrology!.operations).toContain("export_stm");
    expect(catchment).toBeDefined();
    expect(catchment!.domain).toBe("hydrology");
    expect(stm).toBeDefined();
    expect(stm!.domain).toBe("hydrology");
    expect(quantity).toBeDefined();
    expect(quantity!.operations).toContain("export_to_csv");
    expect(superelevation).toBeDefined();
    expect(superelevation!.operations).toContain("report");
    expect(intersection).toBeDefined();
    expect(intersection!.operations).toContain("get");
    expect(sightDistance).toBeDefined();
    expect(sightDistance!.operations).toContain("calculate");
    expect(detention).toBeDefined();
    expect(detention!.operations).toContain("basin_size_calculate");
    expect(slope).toBeDefined();
    expect(slope!.operations).toContain("geometry_calculate");
    expect(cost).toBeDefined();
    expect(cost!.operations).toContain("pay_items_export");
    expect(geometry).toBeDefined();
    expect(geometry!.operations).toContain("create_polyline");
    expect(drawing).toBeDefined();
    expect(drawing!.operations).toContain("list_object_types");
    expect(coordinateSystem).toBeDefined();
    expect(coordinateSystem!.operations).toContain("info");
    expect(job).toBeDefined();
    expect(job!.operations).toContain("status");
    expect(plugin).toBeDefined();
    expect(plugin!.domain).toBe("plugin");
    expect(docs).toBeDefined();
    expect(docs!.operations).toContain("orchestrate");
    expect(orchestrate).toBeDefined();
    expect(orchestrate!.domain).toBe("docs");
  });
});
