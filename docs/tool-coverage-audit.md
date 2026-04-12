# Civil3D MCP Tool Coverage Audit

**Last Updated**: 2026-03-20
**Author**: Founding Engineer + Cascade audit refresh
**V3 Plan**: `ULTIMATE-CIVIL3D-MCP-PLAN-V3.md`
**Branch**: `main`

---

## Summary

| Metric | Value |
|--------|-------|
| V3 Planned Tools | 169+ across 8 planned microservices |
| Registered MCP Tools | **186** |
| Cataloged Tools (`tool_catalog.ts`) | **186** |
| Coverage vs V3 Count | ~110% by raw tool count |
| Runtime Architecture | Single TypeScript MCP server + Civil3D plugin bridge |

## Audit Corrections Applied

- **Registered missing tool**: `civil3d_pipe_catalog` existed and was plugin-backed, but was not exposed in `src/tools/register.ts`.
- **Backfilled 12 missing catalog entries** in `src/tools/tool_catalog.ts`:
  - `civil3d_assembly_create`
  - `civil3d_subassembly_create`
  - `civil3d_assembly_edit`
  - `civil3d_cogo_inverse`
  - `civil3d_cogo_direction_distance`
  - `civil3d_cogo_traverse`
  - `civil3d_cogo_curve_solve`
  - `civil3d_pipe_catalog`
  - `civil3d_survey_database_list`
  - `civil3d_survey_database_create`
  - `civil3d_survey_figure_list`
  - `civil3d_survey_figure_get`
- **Removed stale gap claims**: alignment editing, profile editing, QC, quantity takeoff, section views, superelevation, intersections, parcels, and survey-processing are no longer “missing”.
- **Verified real backend gaps** against `Civil3D-MCP-Plugin` instead of relying on plan-era assumptions.
- **Closed previously documented gaps**:
  - Multi-turn Copilot tool-use loop
  - Pressure network drawing context
  - Gravity pipe sizing automation
  - Gravity pipe profile-view automation
  - Standards auto-remediation for layer-based drawing standards

---

## Architecture Note

The V3 plan describes an 8-microservice ecosystem. What is actually implemented is a **single TypeScript MCP server** in `src/`, connected to Civil 3D through the C# plugin bridge in `Civil3D-MCP-Plugin/` via `httpBridge.ts`.

The **AI Copilot** (`Civil3D-AI-CoPilot-master/`) is a WPF palette running inside Civil 3D that communicates with the MCP server via HTTP. It has two execution paths:

1. **Primary**: Copilot → HTTP POST `/execute` → `httpBridge.ts` → `toolHandlerRegistry` → MCP tool handlers → plugin TCP socket
2. **Fallback**: Copilot → `PluginCommandClient.cs` → direct TCP to plugin on port 8080 (when MCP server unavailable)
3. **External AI**: Claude Desktop / Windsurf / Cascade → MCP stdio protocol → registered tools (unchanged)

That architecture is the current source of truth. Tool coverage should be audited from:

- **Runtime registration**: `src/tools/register.ts`
- **Catalog/documentation**: `src/tools/tool_catalog.ts`
- **Backend command support**: `Civil3D-MCP-Plugin/CommandDispatcher.cs`
- **AI Copilot prompt**: `Civil3D-AI-CoPilot-master/Core/AgentPromptManager.cs`
- **HTTP bridge passthrough**: `src/httpBridge.ts` + `src/tools/toolHandlerRegistry.ts`

---

## Current Coverage Snapshot

### Fully or Broadly Implemented Areas

- **Surface / terrain**
  - Surface CRUD, drainage workflow, comparison workflow, volume analysis, slope/elevation/aspect analysis, contour control, statistics, DEM import, elevation sampling.

- **Alignment / profile design**
  - Alignment and profile base tools, reports, editing tools, offset/widening, station equations, station/elevation sampling, K-value checks, profile views, superelevation, and intersections.

- **Corridor / section / assembly**
  - Corridor inspection and summary, corridor target mapping, corridor region editing, section tools, section views, assembly inspection, assembly creation/editing, and feature-line support.

- **Grading**
  - Grading groups, grading creation/deletion, grading criteria lookup, grading group surfaces, and feature-line creation.

- **Points / survey / COGO**
  - COGO point operations, point groups, point export/transform, COGO inverse/direction-distance/traverse/curve solve, survey databases, survey figures, observation listing, network adjustment, and LandXML import.

- **Parcels / data shortcuts**
  - Parcel query/create/edit/lot-line adjustment/report and data shortcut create/promote/reference/sync.

- **Pipes / utilities**
  - Gravity pipe network query/edit, pipe parts catalog, HGL calculation, hydraulic analysis, and structure properties.
  - Gravity pipe automation now includes `civil3d_pipe_network_size` and `civil3d_pipe_profile_view_automation`.
  - Pressure network suite is implemented with create/delete/connect/export/validate and component-level add/resize/get operations.

- **Documentation / QC / orchestration**
  - Drawing, label, style, health, coordinate system, object info, tool capability listing, standards lookup, job inspection, orchestrate planning, QC checks, quantity takeoff, and standards auto-remediation tools.

- **Hydrology / catchment / stormwater**
  - Surface-based flow tracing, watershed delineation, Rational Method runoff, watershed-to-runoff/detention/pipe workflows.
  - Catchment group management: list groups, list/get/edit catchment properties (runoff coefficient, Manning's n, curve number, Tc), copy between groups, retrieve flow paths and boundaries.
  - Time of Concentration: Kirpich, TR-55 (sheet/shallow/channel), FAA, NRCS Lag methods. SCS triangular and curvilinear unit hydrograph generation.
  - Storm & Sanitary Analysis (STM): export/import Hydraflow .STM files, open gravity network analysis dialog.

- **Extended analysis domains**
  - Sight distance, detention sizing, slope analysis, and cost estimation are implemented.

- **AutoCAD primitive helpers**
  - `acad_create_polyline`, `acad_create_3dpolyline`, `acad_create_text`, `acad_create_mtext`, `create_line_segment`.

---

## Verified Remaining Gaps

These are the real gaps after checking the TypeScript server, C# plugin backend, and AI Copilot integration.

| Gap Area | Status | Notes |
|----------|--------|-------|
| APS / 3D viewer integration | Not implemented | No corresponding backend commands were found in `Civil3D-MCP-Plugin`. |
| Rich workflow packs | Partial | `civil3d_orchestrate` still provides planning and limited execution rather than a broad library of end-to-end, domain-specific workflow packs. |
| Full storm-drain synthesis | Partial | Pipe sizing and profile-view setup now exist, but there is still no single end-to-end gravity-network design synthesis tool covering layout, sizing, checks, and deliverables in one call. |

---

## Important Audit Notes

- **Raw count alone is not enough**. The previous audit overstated and understated coverage in different places because it mixed plan assumptions with implementation reality.
- **`tool_catalog.ts` must stay synchronized** with actual `server.tool(...)` registrations, or `list_tool_capabilities` becomes misleading.
- **Do not claim a tool is implemented unless both are true**:
  - it is registered in `src/tools/register.ts`
  - it is backed by a real plugin command or internal implementation

---

## Copilot-Specific Fixes Applied (2026-03-20)

### Critical: HTTP Bridge Passthrough (Phase 1)
The HTTP bridge (`httpBridge.ts`) previously only supported ~11 hardcoded tool names. All other 168+ tools threw `Unsupported HTTP bridge tool`. Fixed by:
- Creating `toolHandlerRegistry.ts` — captures every `server.tool()` handler during registration
- Modifying `register.ts` — intercepts `server.tool()` to populate the registry
- Rewriting `httpBridge.ts` — looks up handlers from registry first, falls back to legacy endpoints
- Adding `GET /tools` endpoint for runtime tool discovery

### Phase 2: Direct Plugin TCP Client
- Created `PluginCommandClient.cs` — speaks JSON-RPC 2.0 directly to the C# plugin on port 8080
- Integrated into `CommandExecutorService.cs` as a fallback when MCP HTTP server is unavailable
- Wired into `AIChatPanel.xaml.cs` initialization flow

### AgentPromptManager Updates
- Added 24 missing tools to SUPPORTED COMMANDS (intersection, parcel editing, data shortcuts, survey processing, section views, corridor editing, standards lookup)
- Added multi-turn result-loop rules and `ContinueWithResults` response contract
- Added new tool coverage for gravity pipe sizing/profile-view automation and drawing-standards remediation
- Fixed section counts: CORRIDOR 2→7, SECTION 1→8

### DrawingContextService Expansion
- Added pipe networks, pressure networks, sites/parcels, point groups to the drawing context summary
- Previously only had: alignments, surfaces, assemblies, corridors

### Copilot Multi-Turn Loop
- `AIChatPanel.xaml.cs` now supports iterative command execution with tool-result feedback
- `CommandExecutorService.cs` now returns structured per-command execution results
- AI providers now accept conversation history and the JSON contract includes `ContinueWithResults`

### New Backend and MCP Tools
- Added plugin method `resizePipeInNetwork`
- Added plugin method `qcFixDrawingStandards`
- Added MCP tool `civil3d_pipe_network_size`
- Added MCP tool `civil3d_pipe_profile_view_automation`
- Added MCP tool `civil3d_qc_fix_drawing_standards`

---

## Recommendation

Use this priority order for future work:

1. **APS / 3D viewer integration**
   - Do not add TypeScript wrappers until an actual backend/API integration exists.

2. **Richer workflow packs**
   - Add more single-call workflow tools for full corridor, utility, and QC delivery sequences where multi-step orchestration remains repetitive.

3. **Broader standards remediation**
   - Current remediation focuses on layer-based drawing standards. Style, label, and object-level standards repair remains future work.

4. **Keep audit automation-friendly**
   - Treat `register.ts` and `tool_catalog.ts` as the canonical implementation surfaces for future audits.
   - Keep `AgentPromptManager.cs` SUPPORTED COMMANDS synchronized with the tool catalog.
