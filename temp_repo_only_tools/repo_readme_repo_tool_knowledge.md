# Agent Knowledge Extract

## repo.1 Live MCP Tool Coverage: Drawing Info & Context _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Drawing Info & Context

**Tags:** drawing, info, context, mcp, tools

**Priority:** 99

**Summary:** This repo currently exposes 7 live MCP tools in the 'Drawing Info & Context' section. Representative tools include get_drawing_info, list_civil_object_types, get_selected_civil_objects_info, civil3d_health.

**Agent Use:** Use to ground the assistant in the repo's current drawing info & context tool surface instead of relying on PDF-only standards content.

**Rules:**

- get_drawing_info: Retrieves basic information about the active Civil 3D drawing
- list_civil_object_types: Lists major Civil 3D object types present in the current drawing
- get_selected_civil_objects_info: Gets properties of currently selected Civil 3D objects
- civil3d_health: Reports Civil 3D connection and plugin status
- civil3d_drawing: Manages drawing state, document info, save/undo operations
- civil3d_job: Checks status of long-running async operations or requests cancellation
- list_tool_capabilities: Lists domain and capability metadata for the full MCP tool catalog

## repo.2 Live MCP Tool Coverage: Drawing Primitives _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Drawing Primitives

**Tags:** drawing, primitives, mcp, tools

**Priority:** 98

**Summary:** This repo currently exposes 6 live MCP tools in the 'Drawing Primitives' section. Representative tools include create_cogo_point, create_line_segment, acad_create_polyline, acad_create_3dpolyline.

**Agent Use:** Use to ground the assistant in the repo's current drawing primitives tool surface instead of relying on PDF-only standards content.

**Rules:**

- create_cogo_point: Creates a single COGO point
- create_line_segment: Creates a simple line segment
- acad_create_polyline: Creates an AutoCAD 2D polyline in model space
- acad_create_3dpolyline: Creates an AutoCAD 3D polyline in model space
- acad_create_text: Creates AutoCAD DBText in model space
- acad_create_mtext: Creates AutoCAD MText in model space

## repo.3 Live MCP Tool Coverage: Alignment _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Alignment

**Tags:** alignment, mcp, tools

**Priority:** 97

**Summary:** This repo currently exposes 10 live MCP tools in the 'Alignment' section. Representative tools include civil3d_alignment, civil3d_alignment_report, civil3d_alignment_get_station_offset, civil3d_alignment_add_tangent.

**Agent Use:** Use to ground the assistant in the repo's current alignment tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_alignment: Reads alignments, converts stationing, create/delete
- civil3d_alignment_report: Builds structured alignment geometry report
- civil3d_alignment_get_station_offset: Returns station/offset of an XY point relative to an alignment
- civil3d_alignment_add_tangent: Appends a fixed tangent entity to an alignment
- civil3d_alignment_add_curve: Appends a fixed horizontal curve to an alignment
- civil3d_alignment_add_spiral: Appends a spiral (transition curve) to an alignment
- civil3d_alignment_delete_entity: Deletes a tangent/curve/spiral entity by index
- civil3d_alignment_offset_create: Creates a new offset alignment at a constant distance
- civil3d_alignment_set_station_equation: Adds a station equation to an alignment
- civil3d_alignment_widen_transition: Creates a variable-offset widening/narrowing transition region

## repo.4 Live MCP Tool Coverage: Profile _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Profile

**Tags:** profile, mcp, tools

**Priority:** 96

**Summary:** This repo currently exposes 10 live MCP tools in the 'Profile' section. Representative tools include civil3d_profile, civil3d_profile_report, civil3d_profile_get_elevation, civil3d_profile_add_pvi.

**Agent Use:** Use to ground the assistant in the repo's current profile tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_profile: Reads profiles, create/delete
- civil3d_profile_report: Builds structured profile report with station/elevation sampling
- civil3d_profile_get_elevation: Samples elevation and grade at a given station
- civil3d_profile_add_pvi: Adds a PVI to a layout profile
- civil3d_profile_add_curve: Adds a parabolic vertical curve at an existing PVI
- civil3d_profile_delete_pvi: Deletes the PVI nearest to a specified station
- civil3d_profile_set_grade: Sets the grade of a tangent entity
- civil3d_profile_check_k_values: Validates K-values against AASHTO minimums for design speed
- civil3d_profile_view_create: Creates a profile view at a specified insertion point
- civil3d_profile_view_band_set: Applies a band set style to an existing profile view

## repo.5 Live MCP Tool Coverage: Superelevation _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Superelevation

**Tags:** superelevation, mcp, tools

**Priority:** 95

**Summary:** This repo currently exposes 4 live MCP tools in the 'Superelevation' section. Representative tools include civil3d_superelevation_get, civil3d_superelevation_set, civil3d_superelevation_design_check, civil3d_superelevation_report.

**Agent Use:** Use to ground the assistant in the repo's current superelevation tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_superelevation_get: Retrieve superelevation design data for an alignment
- civil3d_superelevation_set: Apply superelevation using AASHTO attainment method
- civil3d_superelevation_design_check: Validate max superelevation rates and attainment lengths
- civil3d_superelevation_report: Generate formatted superelevation report

## repo.6 Live MCP Tool Coverage: Surface _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Surface

**Tags:** surface, mcp, tools

**Priority:** 94

**Summary:** This repo currently exposes 15 live MCP tools in the 'Surface' section. Representative tools include civil3d_surface, civil3d_surface_edit, civil3d_surface_statistics_get, civil3d_surface_contour_interval_set.

**Agent Use:** Use to ground the assistant in the repo's current surface tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_surface: Reads surface data, create/delete
- civil3d_surface_edit: Modifies surface data: points, breaklines, boundaries, contours
- civil3d_surface_statistics_get: Comprehensive statistics: elevation range, area, point/triangle count
- civil3d_surface_contour_interval_set: Set minor and major contour display intervals
- civil3d_surface_sample_elevations: Sample elevations at grid points, discrete points, or transect
- civil3d_surface_analyze_elevation: Elevation band distribution (area and percentage per band)
- civil3d_surface_analyze_slope: Slope distribution (area and percentage per slope range)
- civil3d_surface_analyze_directions: Aspect/facing direction breakdown by cardinal sectors
- civil3d_surface_volume_calculate: Calculate cut/fill volumes between two surfaces
- civil3d_surface_volume_by_region: Cut/fill volumes within a polygon region
- civil3d_surface_volume_report: Formatted human-readable cut/fill volume report
- civil3d_surface_comparison_workflow: Structured two-surface comparison with cut/fill volumes
- civil3d_surface_create_from_dem: Create TIN surface from DEM file (.dem, .tif, .asc, .flt)
- civil3d_surface_watershed_add: Add watershed analysis: drainage basins and flow paths
- civil3d_surface_drainage_workflow: Surface drainage workflow: flow path, elevation sampling, runoff estimate

## repo.7 Live MCP Tool Coverage: Corridor _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Corridor

**Tags:** corridor, mcp, tools

**Priority:** 93

**Summary:** This repo currently exposes 6 live MCP tools in the 'Corridor' section. Representative tools include civil3d_corridor, civil3d_corridor_summary, civil3d_corridor_target_mapping_get, civil3d_corridor_target_mapping_set.

**Agent Use:** Use to ground the assistant in the repo's current corridor tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_corridor: Reads corridor data, rebuild, volume operations
- civil3d_corridor_summary: Builds corridor summary with surfaces and volume analysis
- civil3d_corridor_target_mapping_get: Retrieve subassembly target mappings for a corridor
- civil3d_corridor_target_mapping_set: Set/update subassembly target mappings (surfaces, alignments, profiles)
- civil3d_corridor_region_add: Add a new region to a corridor baseline
- civil3d_corridor_region_delete: Delete a region from a corridor baseline

## repo.8 Live MCP Tool Coverage: Section & Section Views _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Section & Section Views

**Tags:** section, views, mcp, tools

**Priority:** 92

**Summary:** This repo currently exposes 6 live MCP tools in the 'Section & Section Views' section. Representative tools include civil3d_section, civil3d_section_view_create, civil3d_section_view_list, civil3d_section_view_update_style.

**Agent Use:** Use to ground the assistant in the repo's current section & section views tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_section: Reads section data, sample line creation
- civil3d_section_view_create: Create section views for a sample line group
- civil3d_section_view_list: List section views in the drawing
- civil3d_section_view_update_style: Update display/band set style on existing section views
- civil3d_section_view_group_create: Create a multi-row grid layout of section views
- civil3d_section_view_export: Export section data to CSV/text (offsets, elevations, materials)

## repo.9 Live MCP Tool Coverage: Intersection Design _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Intersection Design

**Tags:** intersection, design, mcp, tools

**Priority:** 91

**Summary:** This repo currently exposes 3 live MCP tools in the 'Intersection Design' section. Representative tools include civil3d_intersection_list, civil3d_intersection_create, civil3d_intersection_get.

**Agent Use:** Use to ground the assistant in the repo's current intersection design tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_intersection_list: List all intersections in the drawing
- civil3d_intersection_create: Create an intersection between two road alignments
- civil3d_intersection_get: Get detailed properties of an intersection

## repo.10 Live MCP Tool Coverage: Grading & Feature Lines _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Grading & Feature Lines

**Tags:** grading, feature, lines, mcp, tools

**Priority:** 90

**Summary:** This repo currently exposes 14 live MCP tools in the 'Grading & Feature Lines' section. Representative tools include civil3d_grading, civil3d_feature_line, civil3d_feature_line_create, civil3d_grading_group_list.

**Agent Use:** Use to ground the assistant in the repo's current grading & feature lines tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_grading: Canonical grading domain tool for grading groups, gradings, grading criteria, and feature-line actions
- civil3d_feature_line: Reads feature lines and exports them as 3D polylines
- civil3d_feature_line_create: Create a new feature line from 3D points
- civil3d_grading_group_list: List all grading groups in the drawing
- civil3d_grading_group_create: Create a new grading group
- civil3d_grading_group_get: Get detailed info about a grading group
- civil3d_grading_group_delete: Delete a grading group and all its gradings
- civil3d_grading_group_volume: Get cut/fill volume report for a grading group
- civil3d_grading_group_surface_create: Create a surface from a grading group
- civil3d_grading_criteria_list: List all available grading criteria sets
- civil3d_grading_list: List all grading objects within a grading group
- civil3d_grading_create: Create a new grading from a feature line
- civil3d_grading_get: Get detailed properties of a grading object
- civil3d_grading_delete: Delete a grading object by handle

## repo.11 Live MCP Tool Coverage: Points & Point Groups _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Points & Point Groups

**Tags:** points, point, groups, mcp, tools

**Priority:** 89

**Summary:** This repo currently exposes 6 live MCP tools in the 'Points & Point Groups' section. Representative tools include civil3d_point, civil3d_point_export, civil3d_point_transform, civil3d_point_group_create.

**Agent Use:** Use to ground the assistant in the repo's current points & point groups tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_point: Reads, creates, imports, deletes COGO points and point groups
- civil3d_point_export: Export COGO points to text/CSV (PNEZD, PENZ, XYZD, XYZ, CSV)
- civil3d_point_transform: Transform points by translation, rotation, and/or scale
- civil3d_point_group_create: Create a new point group with filter criteria
- civil3d_point_group_update: Update filter criteria and description of a point group
- civil3d_point_group_delete: Delete a point group (points are NOT deleted)

## repo.12 Live MCP Tool Coverage: COGO & Survey _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: COGO & Survey

**Tags:** cogo, survey, mcp, tools

**Priority:** 88

**Summary:** This repo currently exposes 9 live MCP tools in the 'COGO & Survey' section. Representative tools include civil3d_cogo_inverse, civil3d_cogo_direction_distance, civil3d_cogo_curve_solve, civil3d_cogo_traverse.

**Agent Use:** Use to ground the assistant in the repo's current cogo & survey tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_cogo_inverse: Calculate bearing and distance between two coordinate pairs
- civil3d_cogo_direction_distance: Project a point from start coordinate given bearing and distance
- civil3d_cogo_curve_solve: Solve a horizontal curve given any two curve elements
- civil3d_cogo_traverse: Solve a traverse from start point through bearing/distance courses
- civil3d_coordinate_system: Coordinate system info and coordinate transformations
- civil3d_survey_database_list: List all survey databases
- civil3d_survey_database_create: Create a new survey database
- civil3d_survey_figure_list: List all survey figures
- civil3d_survey_figure_get: Get 3D vertex data for a specific survey figure

## repo.13 Live MCP Tool Coverage: Pipe Networks — Gravity _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Pipe Networks — Gravity

**Tags:** pipe, networks, gravity, mcp, tools

**Priority:** 87

**Summary:** This repo currently exposes 3 live MCP tools in the 'Pipe Networks — Gravity' section. Representative tools include civil3d_pipe_catalog, civil3d_pipe_network, civil3d_pipe_network_edit.

**Agent Use:** Use to ground the assistant in the repo's current pipe networks — gravity tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_pipe_catalog: Lists available pipe parts lists and part names
- civil3d_pipe_network: Reads pipe network data: networks, pipes, structures
- civil3d_pipe_network_edit: Creates and modifies pipe networks, pipes, and structures

## repo.14 Live MCP Tool Coverage: Pressure Networks _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Pressure Networks

**Tags:** pressure, networks, mcp, tools

**Priority:** 86

**Summary:** This repo currently exposes 15 live MCP tools in the 'Pressure Networks' section. Representative tools include civil3d_pressure_network_list, civil3d_pressure_network_create, civil3d_pressure_network_get_info, civil3d_pressure_network_delete.

**Agent Use:** Use to ground the assistant in the repo's current pressure networks tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_pressure_network_list: List all pressure networks
- civil3d_pressure_network_create: Create a new pressure network
- civil3d_pressure_network_get_info: Get detailed info about a pressure network
- civil3d_pressure_network_delete: Delete a pressure network and all components
- civil3d_pressure_network_assign_parts_list: Assign a parts list to a network
- civil3d_pressure_network_set_cover: Set minimum cover depth for pipes in a network
- civil3d_pressure_network_validate: Validate a network for cover violations and disconnections
- civil3d_pressure_network_export: Export pressure network data as structured JSON
- civil3d_pressure_network_connect: Connect two pressure networks by merging
- civil3d_pressure_pipe_add: Add a pressure pipe segment
- civil3d_pressure_pipe_get_properties: Get properties of a pressure pipe
- civil3d_pressure_pipe_resize: Change pressure pipe size to a different catalog entry
- civil3d_pressure_fitting_add: Add a pressure fitting (elbow, tee, reducer, cap)
- civil3d_pressure_fitting_get_properties: Get properties of a pressure fitting
- civil3d_pressure_appurtenance_add: Add a pressure appurtenance (valve, hydrant, meter)

## repo.15 Live MCP Tool Coverage: Plan Production / Sheets _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Plan Production / Sheets

**Tags:** plan, production, sheets, mcp, tools

**Priority:** 85

**Summary:** This repo currently exposes 12 live MCP tools in the 'Plan Production / Sheets' section. Representative tools include civil3d_sheet_set_list, civil3d_sheet_set_create, civil3d_sheet_set_get_info, civil3d_sheet_set_export.

**Agent Use:** Use to ground the assistant in the repo's current plan production / sheets tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_sheet_set_list: List all Plan Production sheet sets
- civil3d_sheet_set_create: Create a new Plan Production sheet set
- civil3d_sheet_set_get_info: Get detailed info about a sheet set
- civil3d_sheet_set_export: Export all sheets to a multi-page PDF
- civil3d_sheet_set_title_block: Set or update title block template on a sheet
- civil3d_sheet_add: Add a new sheet to an existing sheet set
- civil3d_sheet_get_properties: Get full properties of a specific sheet
- civil3d_sheet_publish_pdf: Publish sheet layouts to a PDF file
- civil3d_sheet_view_create: Create a viewport/view on a sheet layout
- civil3d_sheet_view_set_scale: Update the scale of a viewport
- civil3d_plan_profile_sheet_create: Create a Plan/Profile sheet for an alignment and profile
- civil3d_plan_profile_sheet_update_alignment: Update alignment and/or profile on an existing sheet

## repo.16 Live MCP Tool Coverage: QC Checks _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: QC Checks

**Tags:** qc, checks, mcp, tools

**Priority:** 84

**Summary:** This repo currently exposes 8 live MCP tools in the 'QC Checks' section. Representative tools include civil3d_qc_check_alignment, civil3d_qc_check_profile, civil3d_qc_check_corridor, civil3d_qc_check_pipe_network.

**Agent Use:** Use to ground the assistant in the repo's current qc checks tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_qc_check_alignment: QC alignment: tangent lengths, curve radii, spirals, design-speed compliance
- civil3d_qc_check_profile: QC profile: max grade, K-values, sight distance requirements
- civil3d_qc_check_corridor: QC corridor: invalid regions, missing targets, assembly gaps, rebuild errors
- civil3d_qc_check_pipe_network: QC pipe network: cover depth, slope, velocity, connectivity
- civil3d_qc_check_surface: QC TIN surface: elevation spikes, flat triangles, crossing breaklines
- civil3d_qc_check_labels: Check labels for missing labels and style standard violations
- civil3d_qc_check_drawing_standards: Audit drawing: layer naming, lineweights, colors
- civil3d_qc_report_generate: Run full QC pass and write consolidated report to disk

## repo.17 Live MCP Tool Coverage: Workflow Automation _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Workflow Automation

**Tags:** workflow, automation, mcp, tools

**Priority:** 83

**Summary:** This repo currently exposes 6 live MCP tools in the 'Workflow Automation' section. Representative tools include civil3d_workflow, civil3d_workflow_corridor_qc_report, civil3d_workflow_grading_surface_volume, civil3d_workflow_pipe_network_design.

**Agent Use:** Use to ground the assistant in the repo's current workflow automation tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_workflow: Canonical workflow domain tool for multi-step QC, grading, pipe-design, standards, and publishing workflows
- civil3d_workflow_corridor_qc_report: Run corridor QC and optionally generate a consolidated QC report
- civil3d_workflow_grading_surface_volume: Calculate grading cut/fill volume between base and comparison surfaces
- civil3d_workflow_pipe_network_design: Size a gravity pipe network and optionally run hydraulic analysis
- civil3d_workflow_plan_production_publish: Publish a sheet set or explicit layout list to PDF output
- civil3d_workflow_qc_fix_and_verify: Audit drawing standards, apply fixes, and verify the result

## repo.18 Live MCP Tool Coverage: Quantity Takeoff _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Quantity Takeoff

**Tags:** quantity, takeoff, mcp, tools

**Priority:** 82

**Summary:** This repo currently exposes 10 live MCP tools in the 'Quantity Takeoff' section. Representative tools include civil3d_qty_surface_volume, civil3d_qty_earthwork_summary, civil3d_qty_corridor_volumes, civil3d_qty_material_list_get.

**Agent Use:** Use to ground the assistant in the repo's current quantity takeoff tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_qty_surface_volume: Cut/fill volumes between surfaces (with corridor or region scope)
- civil3d_qty_earthwork_summary: Running earthwork cut/fill summary table
- civil3d_qty_corridor_volumes: Subassembly material volumes by region for a corridor
- civil3d_qty_material_list_get: Retrieve material list defined on a corridor
- civil3d_qty_pipe_network_lengths: Total pipe lengths for a gravity pipe network
- civil3d_qty_pressure_network_lengths: Total pipe lengths for a pressure network
- civil3d_qty_alignment_lengths: Total length for one or more alignments
- civil3d_qty_parcel_areas: Area, perimeter, and address data for parcels
- civil3d_qty_point_count_by_group: Count COGO points per point group
- civil3d_qty_export_to_csv: Export consolidated quantity takeoff report to CSV

## repo.19 Live MCP Tool Coverage: Hydrology _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Hydrology

**Tags:** hydrology, mcp, tools

**Priority:** 81

**Summary:** This repo currently exposes 7 live MCP tools in the 'Hydrology' section. Representative tools include civil3d_hydrology, civil3d_catchment, civil3d_time_of_concentration, civil3d_stm.

**Agent Use:** Use to ground the assistant in the repo's current hydrology tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_hydrology: Canonical hydrology domain tool for surface drainage, catchments, Tc, SSA, and multi-step workflows
- civil3d_catchment: Manages catchments and catchment groups, including properties, flow paths, and boundaries
- civil3d_time_of_concentration: Calculates Tc using supported methods and generates hydrographs
- civil3d_stm: Exports/imports STM files and opens Storm and Sanitary Analysis
- civil3d_hydrology_watershed_runoff_workflow: Runs low-point or outlet-based watershed delineation through runoff estimation
- civil3d_hydrology_runoff_detention_workflow: Runs runoff estimation through detention sizing and optional stage-storage output
- civil3d_hydrology_runoff_pipe_workflow: Runs runoff estimation through gravity pipe HGL and hydraulic analysis

## repo.20 Live MCP Tool Coverage: Pipe Hydraulics _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Pipe Hydraulics

**Tags:** pipe, hydraulics, mcp, tools

**Priority:** 80

**Summary:** This repo currently exposes 3 live MCP tools in the 'Pipe Hydraulics' section. Representative tools include civil3d_pipe_network_hgl, civil3d_pipe_network_hydraulics, civil3d_pipe_structure_properties.

**Agent Use:** Use to ground the assistant in the repo's current pipe hydraulics tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_pipe_network_hgl: Calculate Hydraulic Grade Line (HGL) for gravity pipe networks
- civil3d_pipe_network_hydraulics: Perform full hydraulic capacity analysis on pipe networks
- civil3d_pipe_structure_properties: Retrieve detailed properties of individual pipe structures

## repo.21 Live MCP Tool Coverage: Pipe Design Automation _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Pipe Design Automation

**Tags:** pipe, design, automation, mcp, tools

**Priority:** 79

**Summary:** This repo currently exposes 2 live MCP tools in the 'Pipe Design Automation' section. Representative tools include civil3d_pipe_network_size, civil3d_pipe_profile_view_automation.

**Agent Use:** Use to ground the assistant in the repo's current pipe design automation tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_pipe_network_size: Size gravity-network pipes from Manning full-flow capacity with catalog part selection
- civil3d_pipe_profile_view_automation: Automate gravity-pipe profile-view setup with EG profile creation

## repo.22 Live MCP Tool Coverage: Assembly Creation _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Assembly Creation

**Tags:** assembly, creation, mcp, tools

**Priority:** 78

**Summary:** This repo currently exposes 3 live MCP tools in the 'Assembly Creation' section. Representative tools include civil3d_assembly_create, civil3d_subassembly_create, civil3d_assembly_edit.

**Agent Use:** Use to ground the assistant in the repo's current assembly creation tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_assembly_create: Create a new Civil 3D assembly at a specified model-space location
- civil3d_subassembly_create: Add a subassembly from the Civil 3D catalog to an existing assembly
- civil3d_assembly_edit: Inspect or modify an existing Civil 3D assembly (list, update, delete subassemblies)

## repo.23 Live MCP Tool Coverage: Sight Distance _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Sight Distance

**Tags:** sight, distance, mcp, tools

**Priority:** 77

**Summary:** This repo currently exposes 2 live MCP tools in the 'Sight Distance' section. Representative tools include civil3d_sight_distance_calculate, civil3d_stopping_distance_check.

**Agent Use:** Use to ground the assistant in the repo's current sight distance tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_sight_distance_calculate: Calculate AASHTO stopping, passing, or decision sight distance for design speed
- civil3d_stopping_distance_check: Check stopping sight distance compliance along an alignment at station intervals

## repo.24 Live MCP Tool Coverage: Detention & Stormwater _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Detention & Stormwater

**Tags:** detention, stormwater, mcp, tools

**Priority:** 76

**Summary:** This repo currently exposes 2 live MCP tools in the 'Detention & Stormwater' section. Representative tools include civil3d_detention_basin_size_calculate, civil3d_detention_stage_storage.

**Agent Use:** Use to ground the assistant in the repo's current detention & stormwater tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_detention_basin_size_calculate: Size a detention basin to reduce peak stormwater runoff
- civil3d_detention_stage_storage: Generate stage-storage-discharge table for a detention basin surface

## repo.25 Live MCP Tool Coverage: Slope Analysis _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Slope Analysis

**Tags:** slope, analysis, mcp, tools

**Priority:** 75

**Summary:** This repo currently exposes 2 live MCP tools in the 'Slope Analysis' section. Representative tools include civil3d_slope_geometry_calculate, civil3d_slope_stability_check.

**Agent Use:** Use to ground the assistant in the repo's current slope analysis tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_slope_geometry_calculate: Calculate daylight line coordinates and slope geometry for cut/fill sections
- civil3d_slope_stability_check: Evaluate cut and fill slope stability along an alignment

## repo.26 Live MCP Tool Coverage: Cost Estimation _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Cost Estimation

**Tags:** cost, estimation, mcp, tools

**Priority:** 74

**Summary:** This repo currently exposes 2 live MCP tools in the 'Cost Estimation' section. Representative tools include civil3d_pay_items_export, civil3d_material_cost_estimate.

**Agent Use:** Use to ground the assistant in the repo's current cost estimation tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_pay_items_export: Extract Civil 3D quantities and export as structured pay item schedule to CSV/Excel
- civil3d_material_cost_estimate: Generate construction cost estimate by combining quantities with unit prices

## repo.27 Live MCP Tool Coverage: Survey Processing _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Survey Processing

**Tags:** survey, processing, mcp, tools

**Priority:** 73

**Summary:** This repo currently exposes 4 live MCP tools in the 'Survey Processing' section. Representative tools include civil3d_survey_observation_list, civil3d_survey_network_adjust, civil3d_survey_figure_create, civil3d_survey_landxml_import.

**Agent Use:** Use to ground the assistant in the repo's current survey processing tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_survey_observation_list: List survey observations from a survey database
- civil3d_survey_network_adjust: Adjust survey networks using various methods (least squares, compass, transit, crandall)
- civil3d_survey_figure_create: Create survey figures from point numbers
- civil3d_survey_landxml_import: Import survey data from LandXML files

## repo.28 Live MCP Tool Coverage: Data Shortcut Management _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Data Shortcut Management

**Tags:** data, shortcut, management, mcp, tools

**Priority:** 72

**Summary:** This repo currently exposes 4 live MCP tools in the 'Data Shortcut Management' section. Representative tools include civil3d_data_shortcut_create, civil3d_data_shortcut_promote, civil3d_data_shortcut_reference, civil3d_data_shortcut_sync.

**Agent Use:** Use to ground the assistant in the repo's current data shortcut management tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_data_shortcut_create: Create data shortcuts for Civil 3D objects
- civil3d_data_shortcut_promote: Promote data shortcut references to full editable objects
- civil3d_data_shortcut_reference: Reference existing data shortcuts into the current drawing
- civil3d_data_shortcut_sync: Synchronize outdated data shortcut references

## repo.29 Live MCP Tool Coverage: Parcel Editing _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Parcel Editing

**Tags:** parcel, editing, mcp, tools

**Priority:** 71

**Summary:** This repo currently exposes 4 live MCP tools in the 'Parcel Editing' section. Representative tools include civil3d_parcel_create, civil3d_parcel_edit, civil3d_parcel_lot_line_adjust, civil3d_parcel_report.

**Agent Use:** Use to ground the assistant in the repo's current parcel editing tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_parcel_create: Create parcels from polylines, feature lines, or vertex lists
- civil3d_parcel_edit: Edit parcel properties (name, style, label style, description)
- civil3d_parcel_lot_line_adjust: Adjust lot lines to achieve a target area
- civil3d_parcel_report: Generate parcel reports with coordinate and unit settings

## repo.30 Live MCP Tool Coverage: Storm & Sanitary Analysis (SSA) _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Storm & Sanitary Analysis (SSA)

**Tags:** storm, sanitary, analysis, ssa, mcp, tools

**Priority:** 70

**Summary:** This repo currently exposes 1 live MCP tools in the 'Storm & Sanitary Analysis (SSA)' section. Representative tools include civil3d_stm.

**Agent Use:** Use to ground the assistant in the repo's current storm & sanitary analysis (ssa) tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_stm: Manages Storm and Sanitary Analysis workflows including STM file import/export (4 actions)

## repo.31 Live MCP Tool Coverage: Standards Lookup _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Standards Lookup

**Tags:** standards, lookup, mcp, tools

**Priority:** 69

**Summary:** This repo currently exposes 1 live MCP tools in the 'Standards Lookup' section. Representative tools include civil3d_standards_lookup.

**Agent Use:** Use to ground the assistant in the repo's current standards lookup tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_standards_lookup: Looks up Civil 3D standards, template governance, layer/style guidance, and labeling conventions

## repo.32 Live MCP Tool Coverage: Miscellaneous _(pages repo-live)_

**Concept:** Live Civil 3D MCP coverage: Miscellaneous

**Tags:** miscellaneous, mcp, tools

**Priority:** 68

**Summary:** This repo currently exposes 6 live MCP tools in the 'Miscellaneous' section. Representative tools include civil3d_assembly, civil3d_label, civil3d_style, civil3d_parcel.

**Agent Use:** Use to ground the assistant in the repo's current miscellaneous tool surface instead of relying on PDF-only standards content.

**Rules:**

- civil3d_assembly: Lists and inspects assemblies and subassemblies
- civil3d_label: Manages labels on Civil 3D objects
- civil3d_style: Lists and inspects Civil 3D styles
- civil3d_parcel: Reads parcel and site data
- civil3d_data_shortcut: Manages data shortcuts: listing, syncing, creating references
- civil3d_orchestrate: Routes natural-language Civil 3D requests to the best tool
