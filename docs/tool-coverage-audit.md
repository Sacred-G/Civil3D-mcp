# Civil3D MCP Tool Coverage Audit

**Last Updated**: 2026-03-17
**Author**: Founding Engineer (Paperclip JFS-13)
**V3 Plan**: `ULTIMATE-CIVIL3D-MCP-PLAN-V3.md`
**Branch**: `main`

---

## Summary

| Metric | Value |
|--------|-------|
| V3 Planned Tools | 169+ across 8 microservices |
| Actually Implemented | **150 MCP tools** in a single server |
| Coverage | ~89% of planned tool count |
| Architecture | Single MCP server (pragmatic, intentional) |

### JFS-13 Additions (2026-03-17) — +16 tools (134 → 150)
| Category | Tools Added |
|----------|-------------|
| Section Views | `civil3d_section_view_create`, `civil3d_section_view_list`, `civil3d_section_view_update_style`, `civil3d_section_view_group_create`, `civil3d_section_view_export` |
| Superelevation | `civil3d_superelevation_get`, `civil3d_superelevation_set`, `civil3d_superelevation_design_check`, `civil3d_superelevation_report` |
| Corridor Editing | `civil3d_corridor_target_mapping_get`, `civil3d_corridor_target_mapping_set`, `civil3d_corridor_region_add`, `civil3d_corridor_region_delete` |
| Intersection Design | `civil3d_intersection_list`, `civil3d_intersection_create`, `civil3d_intersection_get` |

---

## Architecture Note

The V3 plan describes an 8-microservice ecosystem. What is actually built is a **single TypeScript MCP server** (`src/`) connecting directly to Civil3D via a socket bridge (`httpBridge.ts`). This is the right approach at current scale — do not migrate to microservices without a concrete operational reason.

---

## Implemented Tools (100 Total)

### Core Design — Surfaces (15 / 15 planned)
| Tool | Status |
|------|--------|
| `civil3d_surface` | ✅ Implemented |
| `civil3d_surface_edit` | ✅ Implemented |
| `civil3d_surface_comparison_workflow` | ✅ Implemented |
| `civil3d_surface_drainage_workflow` | ✅ Implemented |
| `civil3d_surface_volume_calculate` | ✅ Implemented (JFS-7) |
| `civil3d_surface_volume_report` | ✅ Implemented (JFS-7) |
| `civil3d_surface_volume_by_region` | ✅ Implemented (JFS-7) |
| `civil3d_surface_analyze_slope` | ✅ Implemented (JFS-7) |
| `civil3d_surface_analyze_elevation` | ✅ Implemented (JFS-7) |
| `civil3d_surface_analyze_directions` | ✅ Implemented (JFS-7) |
| `civil3d_surface_watershed_add` | ✅ Implemented (JFS-7) |
| `civil3d_surface_contour_interval_set` | ✅ Implemented (JFS-7) |
| `civil3d_surface_statistics_get` | ✅ Implemented (JFS-7) |
| `civil3d_surface_sample_elevations` | ✅ Implemented (JFS-7) |
| `civil3d_surface_create_from_dem` | ✅ Implemented (JFS-7) |

### Core Design — Alignments (2 / 10 planned)
| Tool | Status |
|------|--------|
| `civil3d_alignment` | ✅ Implemented |
| `civil3d_alignment_report` | ✅ Implemented |
| Alignment edit, offset, design check, superelevation, widenings, station equations, label, import/export | ❌ Missing (8 tools) |

### Core Design — Profiles (2 / 10 planned)
| Tool | Status |
|------|--------|
| `civil3d_profile` | ✅ Implemented |
| `civil3d_profile_report` | ✅ Implemented |
| Profile edit, design check, label, existing ground, superimpose, grade break, vertical curves, import/export | ❌ Missing (8 tools) |

### Core Design — Corridors / Sections (5 / ~10 planned)
| Tool | Status |
|------|--------|
| `civil3d_corridor` | ✅ Implemented |
| `civil3d_corridor_summary` | ✅ Implemented |
| `civil3d_section` | ✅ Implemented |
| `civil3d_assembly` | ✅ Implemented |
| `civil3d_feature_line` | ✅ Implemented |
| Section views, sample lines, corridor targets, section labels, corridor solids | ❌ Missing |

### Core Design — Grading (12 / 12 planned) ✅ NEW JFS-9
| Tool | Status |
|------|--------|
| `civil3d_grading_group_list` | ✅ Implemented (JFS-9) |
| `civil3d_grading_group_get` | ✅ Implemented (JFS-9) |
| `civil3d_grading_group_create` | ✅ Implemented (JFS-9) |
| `civil3d_grading_group_delete` | ✅ Implemented (JFS-9) |
| `civil3d_grading_group_volume` | ✅ Implemented (JFS-9) |
| `civil3d_grading_group_surface_create` | ✅ Implemented (JFS-9) |
| `civil3d_grading_list` | ✅ Implemented (JFS-9) |
| `civil3d_grading_get` | ✅ Implemented (JFS-9) |
| `civil3d_grading_create` | ✅ Implemented (JFS-9) |
| `civil3d_grading_delete` | ✅ Implemented (JFS-9) |
| `civil3d_grading_criteria_list` | ✅ Implemented (JFS-9) |
| `civil3d_feature_line_create` | ✅ Implemented (JFS-9) |

### Infrastructure — Pipe Networks (3 / 25 planned)
| Tool | Status |
|------|--------|
| `civil3d_pipe_network` | ✅ Implemented |
| `civil3d_pipe_network_edit` | ✅ Implemented |
| `civil3d_pipe_catalog` | ✅ Implemented |
| Gravity pipe analysis, pipe sizing, storm drain design, network labels, profile views | ❌ Missing |

### Infrastructure — Pressure Networks (15 / 15 planned) ✅ Complete (JFS-8)
| Tool | Status |
|------|--------|
| `civil3d_pressure_network_list` | ✅ Implemented |
| `civil3d_pressure_network_info` | ✅ Implemented |
| `civil3d_pressure_network_create` | ✅ Implemented |
| `civil3d_pressure_network_delete` | ✅ Implemented |
| `civil3d_pressure_network_assign_parts_list` | ✅ Implemented |
| `civil3d_pressure_network_set_cover` | ✅ Implemented |
| `civil3d_pressure_network_validate` | ✅ Implemented |
| `civil3d_pressure_network_export` | ✅ Implemented |
| `civil3d_pressure_network_connect` | ✅ Implemented |
| `civil3d_pressure_pipe_add` | ✅ Implemented |
| `civil3d_pressure_pipe_properties` | ✅ Implemented |
| `civil3d_pressure_pipe_resize` | ✅ Implemented |
| `civil3d_pressure_fitting_add` | ✅ Implemented |
| `civil3d_pressure_fitting_properties` | ✅ Implemented |
| `civil3d_pressure_appurtenance_add` | ✅ Implemented |

### Infrastructure — Hydrology (1 / 8 planned)
| Tool | Status |
|------|--------|
| `civil3d_hydrology` | ✅ Implemented |
| Watershed analysis, runoff calcs, grading tools (7 tools) | ❌ Missing |

### Survey & Data — COGO Points (6 / 10 planned)
| Tool | Status |
|------|--------|
| `civil3d_point` (list/get/create/list_groups/import/delete) | ✅ Implemented |
| `create_cogo_point` | ✅ Implemented |
| Survey processing (4 tools), traverse solve | ❌ Missing |

### Survey & Data — Point Groups (5 / 5 planned) ✅ NEW JFS-9
| Tool | Status |
|------|--------|
| `civil3d_point_group_create` | ✅ Implemented (JFS-9) |
| `civil3d_point_group_update` | ✅ Implemented (JFS-9) |
| `civil3d_point_group_delete` | ✅ Implemented (JFS-9) |
| `civil3d_point_export` | ✅ Implemented (JFS-9) |
| `civil3d_point_transform` | ✅ Implemented (JFS-9) |

### Survey & Data — COGO Calculations (4 / 8 planned) ✅ NEW JFS-9
| Tool | Status |
|------|--------|
| `civil3d_cogo_inverse` | ✅ Implemented (JFS-9) |
| `civil3d_cogo_direction_distance` | ✅ Implemented (JFS-9) |
| `civil3d_cogo_traverse` | ✅ Implemented (JFS-9) |
| `civil3d_cogo_curve_solve` | ✅ Implemented (JFS-9) |
| COGO lot fit, import survey data, figures from traverse | ❌ Missing (4 tools) |

### Survey & Data — Survey Databases & Figures (4 / 8 planned) ✅ NEW JFS-9
| Tool | Status |
|------|--------|
| `civil3d_survey_database_list` | ✅ Implemented (JFS-9) |
| `civil3d_survey_database_create` | ✅ Implemented (JFS-9) |
| `civil3d_survey_figure_list` | ✅ Implemented (JFS-9) |
| `civil3d_survey_figure_get` | ✅ Implemented (JFS-9) |
| Survey network processing, observation editing, LandXML import | ❌ Missing (4 tools) |

### Survey & Data — Parcels (1 / 5 planned)
| Tool | Status |
|------|--------|
| `civil3d_parcel` | ✅ Implemented |
| Parcel create/edit, lot line adjust, report | ❌ Missing |

### Survey & Data — Data Shortcuts (1 / 5 planned)
| Tool | Status |
|------|--------|
| `civil3d_data_shortcut` | ✅ Implemented |
| Create shortcut, promote shortcut, reference shortcut, sync | ❌ Missing |

### Documentation & QC (3 / 35 planned)
| Tool | Status |
|------|--------|
| `civil3d_drawing` | ✅ Implemented |
| `civil3d_label` | ✅ Implemented |
| `civil3d_style` | ✅ Implemented |
| QC checks (8 tools), reporting (10 tools), standards compliance (7 tools), quantity takeoff (7 tools) | ❌ Missing (32 tools) |

### Plan Production / Sheets (12 / 12 planned) ✅ Complete (JFS-8)
| Tool | Status |
|------|--------|
| `civil3d_sheet_set_list` | ✅ Implemented |
| `civil3d_sheet_set_info` | ✅ Implemented |
| `civil3d_sheet_set_create` | ✅ Implemented |
| `civil3d_sheet_add` | ✅ Implemented |
| `civil3d_sheet_properties` | ✅ Implemented |
| `civil3d_sheet_title_block_set` | ✅ Implemented |
| `civil3d_plan_profile_sheet_create` | ✅ Implemented |
| `civil3d_plan_profile_sheet_update` | ✅ Implemented |
| `civil3d_sheet_view_create` | ✅ Implemented |
| `civil3d_sheet_view_scale_set` | ✅ Implemented |
| `civil3d_sheet_publish_pdf` | ✅ Implemented |
| `civil3d_sheet_set_export` | ✅ Implemented |

### Workflow & Coordination (4 / 15 planned)
| Tool | Status |
|------|--------|
| `civil3d_orchestrate` | ✅ Implemented |
| `civil3d_job` | ✅ Implemented |
| `civil3d_surface_comparison_workflow` | ✅ (counted above) |
| `civil3d_surface_drainage_workflow` | ✅ (counted above) |
| Multi-step corridor workflow, pipe design workflow, QC workflow, export workflow | ❌ Missing |

### AutoCAD Primitives (5 — bonus, not in V3 plan)
| Tool | Status |
|------|--------|
| `acad_create_polyline` | ✅ Implemented |
| `acad_create_3dpolyline` | ✅ Implemented |
| `acad_create_text` | ✅ Implemented |
| `acad_create_mtext` | ✅ Implemented |
| `create_line_segment` | ✅ Implemented |

### Utility / Info (7 — partial V3 coverage)
| Tool | Status |
|------|--------|
| `civil3d_health` | ✅ Implemented |
| `civil3d_coordinate_system` | ✅ Implemented |
| `get_drawing_info` | ✅ Implemented |
| `get_selected_civil_objects_info` | ✅ Implemented |
| `list_civil_object_types` | ✅ Implemented |
| `list_tool_capabilities` | ✅ Implemented |
| `civil3d_pipe_catalog` | ✅ Implemented |

---

## Remaining Gaps (highest ROI first)

| Domain | V3 Count | Implemented | Remaining |
|--------|----------|-------------|-----------|
| Alignment (edit/label/design check) | 10 | 10 | 0 ✅ JFS-10 |
| Profile (edit/label/design check) | 10 | 10 | 0 ✅ JFS-10 |
| QC checks | 8 | 8 | 0 ✅ JFS-11 |
| Quantity takeoff / reporting | 10+ | 10 | 0 ✅ JFS-11 |
| Section views / sample lines | 5 | 5 | 0 ✅ JFS-13 |
| Superelevation | 4 | 4 | 0 ✅ JFS-13 |
| Corridor target editing | 4 | 4 | 0 ✅ JFS-13 |
| Intersection design | 3 | 3 | 0 ✅ JFS-13 |
| Survey processing (observations, networks) | 8 | 4 | 4 |
| Standards compliance labels | 7 | 1 | 6 |
| 3D viewer / APS integration | 6 | 0 | 6 |
| Gravity pipe analysis / sizing | 22 | 3 | 19 |

---

## Recommendation

Target 150+ is achieved. Remaining high-ROI gaps:

1. **Gravity pipe analysis / sizing** — storm drain design, HGL computation (high complexity, deferred)
2. **Survey observation/network processing** — raw survey import, network adjustment
3. **3D viewer / APS integration** — deferred until operational need
