# Domain Tool Migration Roadmap

## Purpose

This document is the implementation playbook for finishing the migration from a large flat MCP tool surface to a production-ready domain-action architecture.

This is not a product spec. It is an engineering guide for future work:

- what the target architecture is
- what has already been migrated
- what order to migrate the remaining domains in
- how to implement each migration
- how to add new capabilities without reintroducing API sprawl

## Current State

Phases 0, 1, and 2 are complete. Fourteen domains are fully migrated.

Implemented foundation:

- Shared metadata types in `src/tools/toolMetadata.ts`
- Shared domain runtime in `src/tools/domainRuntime.ts`
- Manifest-driven registration in `src/tools/toolManifest.ts`
- `src/tools/register.ts` wired to call `registerManifestTools(server)` first; all legacy registrations for migrated domains removed
- `src/tools/tool_catalog.ts` merges generated catalog entries via `mergeCatalogEntries`
- 441 tests passing in `tests/domain_manifest.test.ts` and full suite

Current migrated domains (in `src/tools/domains/`):

| Domain file | Canonical tool | Actions | Aliases replaced |
|---|---|---|---|
| `alignmentDomain.ts` | `civil3d_alignment` | 15 | `civil3d_alignment_report`, `civil3d_alignment_add_tangent`, `civil3d_alignment_add_curve`, `civil3d_alignment_add_spiral`, `civil3d_alignment_delete_entity`, `civil3d_alignment_set_station_equation`, `civil3d_alignment_get_station_offset`, `civil3d_alignment_offset_create`, `civil3d_alignment_widen_transition` |
| `surfaceDomain.ts` | `civil3d_surface` | 24 | `civil3d_surface_edit`, `civil3d_surface_volume_calculate`, `civil3d_surface_volume_report`, `civil3d_surface_volume_by_region`, `civil3d_surface_analyze_slope`, `civil3d_surface_analyze_elevation`, `civil3d_surface_analyze_directions`, `civil3d_surface_watershed_add`, `civil3d_surface_contour_interval_set`, `civil3d_surface_statistics_get`, `civil3d_surface_sample_elevations`, `civil3d_surface_create_from_dem`, `civil3d_surface_comparison_workflow`, `civil3d_surface_drainage_workflow` |
| `profileDomain.ts` | `civil3d_profile` | 15 | `civil3d_profile_report`, `civil3d_profile_add_pvi`, `civil3d_profile_delete_pvi`, `civil3d_profile_add_curve`, `civil3d_profile_set_grade`, `civil3d_profile_get_elevation`, `civil3d_profile_check_k_values`, `civil3d_profile_view_create`, `civil3d_profile_view_band_set` |
| `corridorDomain.ts` | `civil3d_corridor` | 11 | `civil3d_corridor_summary`, `civil3d_corridor_target_mapping_get`, `civil3d_corridor_target_mapping_set`, `civil3d_corridor_region_add`, `civil3d_corridor_region_delete` |
| `sectionDomain.ts` | `civil3d_section` | 8 | `civil3d_section_view_create`, `civil3d_section_view_list`, `civil3d_section_view_update_style`, `civil3d_section_view_group_create`, `civil3d_section_view_export` |
| `pipeDomain.ts` | `civil3d_pipe` | 29 | `civil3d_pipe_network`, `civil3d_pipe_network_edit`, `civil3d_pipe_catalog`, `civil3d_pipe_network_hgl_calculate`, `civil3d_pipe_hydraulic_analysis`, `civil3d_pipe_structure_properties`, `civil3d_pipe_network_size`, `civil3d_pipe_profile_view_automation`, `civil3d_pressure_network_list`, `civil3d_pressure_network_get_info`, `civil3d_pressure_network_create`, `civil3d_pressure_network_delete`, `civil3d_pressure_network_assign_parts_list`, `civil3d_pressure_network_set_cover`, `civil3d_pressure_network_validate`, `civil3d_pressure_network_export`, `civil3d_pressure_network_connect`, `civil3d_pressure_pipe_add`, `civil3d_pressure_pipe_get_properties`, `civil3d_pressure_pipe_resize`, `civil3d_pressure_fitting_add`, `civil3d_pressure_fitting_get_properties`, `civil3d_pressure_appurtenance_add` |
| `assemblyDomain.ts` | `civil3d_assembly` | 5 | `civil3d_assembly_create`, `civil3d_subassembly_create`, `civil3d_assembly_edit` |
| `pointDomain.ts` | `civil3d_point` | 11 | `create_cogo_point`, `civil3d_point_group_create`, `civil3d_point_group_update`, `civil3d_point_group_delete`, `civil3d_point_export`, `civil3d_point_transform` |
| `gradingDomain.ts` | `civil3d_grading` | 15 | `civil3d_feature_line`, `civil3d_grading_group_list`, `civil3d_grading_group_get`, `civil3d_grading_group_create`, `civil3d_grading_group_delete`, `civil3d_grading_group_volume`, `civil3d_grading_group_surface_create`, `civil3d_grading_list`, `civil3d_grading_get`, `civil3d_grading_create`, `civil3d_grading_delete`, `civil3d_grading_criteria_list`, `civil3d_feature_line_create` |
| `parcelDomain.ts` | `civil3d_parcel` | 7 | `civil3d_parcel_create`, `civil3d_parcel_edit`, `civil3d_parcel_lot_line_adjust`, `civil3d_parcel_report` |
| `surveyDomain.ts` | `civil3d_survey` | 8 | `civil3d_survey_database_list`, `civil3d_survey_database_create`, `civil3d_survey_figure_list`, `civil3d_survey_figure_get`, `civil3d_survey_observation_list`, `civil3d_survey_network_adjust`, `civil3d_survey_figure_create`, `civil3d_survey_landxml_import` |
| `planProductionDomain.ts` | `civil3d_plan_production` | 12 | `civil3d_sheet_set_list`, `civil3d_sheet_set_get_info`, `civil3d_sheet_set_create`, `civil3d_sheet_add`, `civil3d_sheet_get_properties`, `civil3d_sheet_set_title_block`, `civil3d_plan_profile_sheet_create`, `civil3d_plan_profile_sheet_update_alignment`, `civil3d_sheet_view_create`, `civil3d_sheet_view_set_scale`, `civil3d_sheet_publish_pdf`, `civil3d_sheet_set_export` |
| `projectDomain.ts` | `civil3d_project` | 6 | `civil3d_data_shortcut`, `civil3d_data_shortcut_create`, `civil3d_data_shortcut_promote`, `civil3d_data_shortcut_reference`, `civil3d_data_shortcut_sync` |
| `standardsDomain.ts` | `civil3d_standards` | 9 | `civil3d_label`, `civil3d_style`, `civil3d_standards_lookup`, `civil3d_qc_check_labels`, `civil3d_qc_check_drawing_standards`, `civil3d_qc_fix_drawing_standards` |
| `qcDomain.ts` | `civil3d_qc` | 6 | `civil3d_qc_check_alignment`, `civil3d_qc_check_profile`, `civil3d_qc_check_corridor`, `civil3d_qc_check_pipe_network`, `civil3d_qc_check_surface`, `civil3d_qc_report_generate` |

Legacy flat files removed from registration (still exist on disk as reference):

- `civil3d_alignment.ts`, `civil3d_alignment_report.ts`, `civil3d_alignment_editing.ts`
- `civil3d_surface.ts`, `civil3d_surface_edit.ts`, `civil3d_surface_analysis.ts`, `civil3d_surface_comparison_workflow.ts`, `civil3d_surface_drainage_workflow.ts`
- `civil3d_profile.ts`, `civil3d_profile_report.ts`, `civil3d_profile_editing.ts`
- `civil3d_corridor.ts`, `civil3d_corridor_summary.ts`, `civil3d_corridor_editing.ts`
- `civil3d_section.ts`, `civil3d_section_views.ts`

These files are no longer registered and can be deleted when convenient. Do not edit them.

## North Star Architecture

The end state is a stable public MCP surface made of canonical domain tools, with optional compatibility aliases for older granular names.

Target model:

1. One canonical tool per major domain.
2. Each domain tool exposes multiple `action` values.
3. Each action has:
   - a dedicated input schema
   - an optional response schema
   - safety metadata
   - plugin method metadata
4. Old granular tool names stay available as alias exposures until explicitly retired.
5. The manifest is the source of truth for:
   - registration
   - tool metadata
   - catalog generation
   - compatibility exposures

## Architecture Rules

These rules are non-negotiable for future work.

### Rule 1: New Features Belong in a Domain

Do not add new top-level MCP tools by default.

When adding a new capability:

- prefer a new `action` on an existing canonical domain tool
- only add a new top-level tool if it represents a brand-new domain and there is no coherent home for it

### Rule 2: Manifest First

Every migrated domain must be defined through:

- a domain definition file under `src/tools/domains/`
- the shared runtime in `src/tools/domainRuntime.ts`
- manifest registration via `src/tools/toolManifest.ts`

### Rule 3: Compatibility Is a Feature

Do not break existing clients unless there is a deliberate versioned migration.

If an old granular tool exists and is already being used:

- keep it alive as an alias exposure
- map it into the canonical domain action
- keep its schema compatible unless there is a very good reason not to

### Rule 4: Catalog Must Be Derived

For migrated families, `src/tools/tool_catalog.ts` must not become a second manual truth source.

The catalog should be:

- generated from the manifest for migrated domains
- merged with the base catalog for yet-to-be-migrated domains

### Rule 5: Safety Metadata Must Travel With the Action

Every action definition must declare:

- `capabilities`
- `requiresActiveDrawing`
- `safeForRetry`
- `pluginMethods` when applicable

This keeps registration, orchestration, and future approval logic consistent.

## Proposed Final Domain Set

This is the recommended end-state domain layout. It is intentionally opinionated and optimized for long-term growth rather than preserving every current category as a separate public tool.

### Core domains

- `civil3d_system`
  - health, drawing, jobs, coordinate system, selection/context helpers
- `civil3d_alignment`
- `civil3d_profile`
- `civil3d_surface`
- `civil3d_corridor`
- `civil3d_section`
- `civil3d_pipe`
- `civil3d_hydrology`
- `civil3d_grading`
- `civil3d_point`
- `civil3d_survey`
- `civil3d_parcel`
- `civil3d_assembly`
- `civil3d_plan_production`
- `civil3d_standards`
- `civil3d_project`
  - data shortcuts, collaboration/project-level object management
- `civil3d_docs`
  - capabilities lookup, orchestration/planning helpers, standards/doc retrieval if kept separate from `standards`

### Consolidation guidance

Use these merges unless a strong reason emerges not to.

- `health` + `drawing` + `job` + `coordinate_system` -> `system`
- `alignment_report` + alignment editing actions -> `alignment`
- `profile_report` + profile editing actions -> `profile`
- `surface_edit` + `surface_analysis` + comparison/drainage workflows -> `surface`
- `corridor_summary` + corridor editing -> `corridor`
- `section_views` -> `section`
- gravity pipe + pressure networks + hydraulics + design automation -> `pipe`
- `hydrology` + `catchment` + `time_of_concentration` + `stm` + detention + sight distance + slope analysis
  - recommended split:
    - keep drainage/hydrology in `hydrology`
    - keep roadway geometry checks such as sight distance and slope geometry either in `alignment/profile` or a later `roadway_analysis` subdomain if it becomes large enough
- `feature_line` + grading tools -> `grading`
- point groups and point operations -> `point`
- survey processing and COGO-heavy survey flows -> `survey`
- QC + style + label + standards lookup
  - recommended split:
    - put governance and design rules in `standards`
    - keep label/style manipulation either in `standards` or fold into object domains if usage stays light
- quantity takeoff + cost estimation -> either:
  - keep separate internal categories, but expose as actions under `project` or `docs`, or
  - create a future `civil3d_estimation` domain if it grows large enough

## Migration Order

This is the recommended execution order for the rest of the implementation.

### Phase 0: Foundation

Status: complete

- shared runtime
- manifest registration
- catalog merge strategy
- alignment migrated
- surface migrated

### Phase 1: Profile

Status: complete

Priority: highest

Why next:

- already follows an action-based shape
- tightly coupled to alignment
- easy win after the alignment migration

Scope:

- migrate `civil3d_profile.ts`
- fold in `civil3d_profile_report.ts`
- fold in `civil3d_profile_editing.ts`
- expose compatibility aliases for the current granular profile tools

Exit criteria:

- `civil3d_profile` becomes the canonical entry point
- profile report/editing actions are in the canonical domain definition
- old profile granular names remain as aliases

### Phase 2: Corridor and Section

Status: complete

Priority: high

Why here:

- corridor workflows depend on alignment/profile/surface
- section tools are downstream of corridor and surface

Scope:

- migrate corridor base + summary + editing tools into `corridor`
- migrate section base + section views into `section`

Exit criteria:

- one canonical corridor domain tool
- one canonical section domain tool
- orchestration/catalog continue to point at valid tool names

### Phase 3: Pipe

Priority: high

Why here:

- this is one of the largest remaining public tool surfaces
- current pipe-related tools are split across many files
- future expansion pressure is high here

Scope:

- gravity pipe tools
- pressure network tools
- hydraulics tools
- pipe design automation tools
- pipe catalog helpers

Exit criteria:

- `civil3d_pipe` becomes the single canonical pipe domain tool
- granular pressure-network tool names remain as aliases until formally deprecated

### Phase 4: Grading, Point, Survey, Parcel, Assembly

Priority: medium

Scope:

- grading + feature line -> `grading`
- point + point groups -> `point`
- COGO and survey-processing review:
  - keep pure geometry utilities where they best fit
  - migrate survey-specific behavior to `survey`
- parcel tools -> `parcel`
- assembly creation + assembly tools -> `assembly`

### Phase 5: Plan Production, Standards, Project, Docs

Priority: medium

Scope:

- plan production -> `plan_production`
- standards/qc/label/style review -> `standards`
- data shortcuts/project-level behaviors -> `project`
- `list_tool_capabilities`, orchestration helper surfaces, and similar utilities -> `docs`

### Phase 6: Hydrology and Analysis Cleanup

Priority: medium

This phase is partly architectural cleanup, partly taxonomy cleanup.

Scope:

- decide final placement for:
  - hydrology
  - catchment
  - time of concentration
  - detention
  - STM
  - sight distance
  - slope analysis
- migrate them into the chosen long-term domain layout

This phase should happen after the more obvious object/workflow domains are stable.

## Detailed Instructions for Migrating One Domain

Use this exact process for every future migration.

### Step 1: Inventory the Current Surface

For the domain you are migrating:

- list all current tool files
- list all currently registered tool names
- list all catalog entries
- list all orchestration references
- list all tests that mention those tool names or schemas

Artifacts to inspect:

- `src/tools/register.ts`
- `src/tools/tool_catalog.ts`
- `src/orchestration/ToolCatalog.ts`
- `src/orchestration/WorkflowPlanner.ts`
- `tests/`

### Step 2: Define the Canonical Action Set

Write down the final action list for the domain before editing code.

For each action decide:

- canonical action name
- whether it is query/create/edit/delete/analyze/manage/generate/etc.
- whether it mutates the drawing
- whether it is safe for retry
- which plugin command(s) it calls
- whether it needs a compatibility alias

### Step 3: Create the Domain Definition

Add or update a file under `src/tools/domains/`.

The domain file must contain:

- all action schemas
- all response schemas that belong with the domain
- one `DomainToolDefinition`
- one canonical exposure for the main domain tool
- compatibility exposures for old tool names that should continue to work

### Step 4: Register Through the Manifest

Update `src/tools/toolManifest.ts`:

- import the domain definition
- add it to `MIGRATED_DOMAIN_DEFINITIONS`

After that, the domain should register through the manifest automatically.

### Step 5: Remove Direct Registration for Migrated Files

Once the domain is working through the manifest:

- remove direct registration imports and calls from `src/tools/register.ts`
- do not leave both paths active

This avoids double registration and ambiguity.

### Step 6: Merge Catalog Ownership

Make sure the canonical domain entries and alias entries come from the generated catalog, not stale manual entries.

If the base catalog still contains older versions of those entries, the generated versions should override them.

### Step 7: Lock the Migration with Tests

Add or update tests for:

- generated manifest entries
- canonical domain action coverage
- compatibility alias coverage
- any important schema shifts

At minimum:

- build must pass
- full test suite must pass

### Step 8: Clean Up Legacy Code

After a domain is stable, do one of these:

1. Delete the old files if they are no longer needed.
2. Keep them temporarily, but add a clear note that they are legacy references only.
3. Convert them into thin wrappers around the new domain implementation if that helps preserve import stability.

Preferred end state:

- one active domain file
- no duplicate business logic in old files

## Instructions for Adding a New Action

Once a domain is migrated, use this procedure for every new feature.

1. Choose the canonical domain first.
2. Add a new action schema to the domain definition file.
3. Add the execution handler.
4. Add response validation if practical.
5. Add metadata:
   - capabilities
   - requiresActiveDrawing
   - safeForRetry
   - pluginMethods
6. If the feature replaces or mirrors an old granular tool name, add a compatibility exposure.
7. Update tests.
8. Do not add direct manual registration in `register.ts`.

## Naming Conventions

Use these naming rules consistently.

### Canonical tool names

- `civil3d_<domain>`

Examples:

- `civil3d_alignment`
- `civil3d_surface`
- `civil3d_profile`
- `civil3d_pipe`

### Action names

- use lowercase snake_case
- prefer verbs or verb phrases
- keep naming consistent across domains

Examples:

- `list`
- `get`
- `create`
- `delete`
- `report`
- `sample_elevations`
- `create_from_dem`
- `widen_transition`

### Compatibility exposure names

- preserve the currently shipped tool names exactly
- do not silently rename old granular tools unless you are intentionally breaking compatibility

## Definition of Done for a Migrated Domain

A domain migration is only done when all of the following are true:

- the canonical domain definition exists under `src/tools/domains/`
- registration happens through the manifest
- direct legacy registration is removed from `src/tools/register.ts`
- the generated catalog owns the migrated entries
- compatibility aliases exist for still-supported old tool names
- tests cover the migration
- build passes
- test suite passes
- future contributors can clearly see which file is the canonical place to extend

## Technical Debt To Pay Down After Each Phase

These cleanup tasks should happen continuously, not all at the end.

### Remove legacy duplicates

After each domain is stable:

- remove or shrink the old implementation files
- avoid leaving multiple active sources of truth

### Centralize shared schemas when it becomes worthwhile

Only extract shared schemas when they are reused in a meaningful way.

Do not over-abstract too early, but do pull common response or geometry schemas into shared helpers once duplication becomes obvious.

### Revisit orchestration mappings

`src/orchestration/ToolCatalog.ts` still points at specific tool names/actions.

As more domains migrate:

- update those mappings to prefer canonical domain tools
- keep intent routing aligned with the new public API shape

### Improve alias visibility

In the future it may be worth extending manifest metadata to mark exposures as:

- canonical
- compatibility alias
- deprecated alias

That will make catalog output and future cleanup easier.

## Risks and Guardrails

### Risk: Split-brain ownership

Problem:

- contributors keep editing old files instead of the canonical domain file

Guardrail:

- remove direct registration for migrated files
- add comments or deprecate legacy files quickly

### Risk: API drift between catalog and runtime

Problem:

- metadata says one thing, runtime does another

Guardrail:

- keep migrated catalog data generated from the manifest

### Risk: Silent client breakage

Problem:

- old tool names disappear during cleanup

Guardrail:

- preserve compatibility exposures until there is an explicit removal plan

### Risk: New features regress into flat tool sprawl

Problem:

- future work adds more one-off top-level tools

Guardrail:

- require all new features to start with “which canonical domain does this belong to?”

## Suggested Next Moves

Completed: alignment, surface, profile, corridor, section, pipe, assembly, point, grading, parcel, survey, plan production, project, standards, and qc are migrated.

Recommended immediate order:

1. Migrate `hydrology` and analysis cleanup (Phase 6)
2. Delete all legacy flat files from disk once each migrated domain has settled

The major non-docs tool surfaces in the roadmap are now in the manifest-driven domain architecture. Remaining work is mainly taxonomy cleanup, hydrology placement, and deleting legacy flat files.

## Quick Start for the Next Session

If you are picking this up later, do this first:

1. Read:
   - `src/tools/domainRuntime.ts`
   - `src/tools/toolManifest.ts`
   - `src/tools/domains/alignmentDomain.ts` (reference for Number() coercion/defaulting inside a composite report action)
   - `src/tools/domains/surfaceDomain.ts` (reference for a large action set with many compatibility aliases and composite workflow actions)
   - `src/tools/domains/profileDomain.ts` (reference for report-style aggregation with bounded sample-count guardrails)
   - `src/tools/domains/corridorDomain.ts` (reference for parameter remapping in resolveAction and plugin null/default normalization)
   - `src/tools/domains/sectionDomain.ts` (reference for tuple cast/unpacking when adapting domain-shaped inputs to plugin parameters)
   - this roadmap
2. The next recommended work is cleanup and hydrology placement. Files to inventory:
   - `src/tools/civil3d_hydrology.ts`
   - `src/tools/civil3d_hydrology_workflows.ts`
   - `src/tools/civil3d_catchment.ts`
   - `src/tools/civil3d_time_of_concentration.ts`
   - `src/tools/civil3d_stm.ts`
3. `src/tools/register.ts` is already wired — `registerManifestTools(server)` is called first.
   Do NOT add new registrations to register.ts. Add the new domain to `toolManifest.ts` only.
4. Define the canonical action list before touching code.
5. Run:
   - `npm run build`
   - `npm test`

## Final Principle

The point of this migration is not just fewer files or fewer tool names.

The point is to create a public API and an internal implementation pattern that can keep growing for a long time without becoming harder to reason about every month.

If a future change makes it easier to add a feature but harder to understand where that feature belongs, it is probably the wrong change.
