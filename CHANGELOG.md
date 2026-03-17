# Changelog

## v1.1.0 — 2026-03-17

### Summary

Adds 12 new tools across 3 categories (parcel editing, survey processing, data shortcut management) to bring the total MCP tool count to 162.

---

### MCP Server (TypeScript)

**New tools (+12):**

| Category | New Tools | Actions |
|---|---|---|
| Parcel Editing | 4 | create, edit, lot-line-adjust, report |
| Survey Processing | 4 | observation-list, network-adjust, figure-create, landxml-import |
| Data Shortcut Management | 4 | create, promote, reference, sync |

**New files:**
- `src/tools/civil3d_parcel_editing.ts` — parcel CRUD and lot-line adjustment
- `src/tools/civil3d_survey_processing.ts` — survey observation, network adjustment, LandXML import
- `src/tools/civil3d_data_shortcut_mgmt.ts` — data shortcut lifecycle management
- `tests/parcel_survey_shortcuts.test.ts` — 34 schema-level tests

**Deferred:** gravity pipe HGL solver and APS 3D viewer (high complexity, no operational demand).

---

## v1.0.0 — 2026-03-17

### Summary

First stable release of the Civil3D MCP Ecosystem: a 150-tool MCP server for Autodesk Civil 3D paired with an AI CoPilot WPF plugin and a full hydrology analysis engine.

---

### MCP Server (TypeScript)

**150 tools across 16 categories:**

| Category | Tools | Highlights |
|---|---|---|
| Alignment | 22 | Create, edit, stationing, offsets, superelevation, widening |
| Surface | 18 | TIN create/edit, breaklines, contours, volume analysis, sampling |
| Corridor | 12 | Create, rebuild, extract solids, edit frequency, targets |
| Profile | 14 | Surface profiles, layout profiles, PVI edit, design speeds |
| Pipe Networks | 10 | Pipes, structures, network analysis, interference |
| Grading | 8 | Feature lines, grading objects, volume balance |
| COGO / Points | 10 | Import/export, groups, inverse, traverse, intersection |
| Plan Production | 12 | Sheet sets, view frames, match lines, north arrows |
| QC | 18 | Design standard checks, clearance, slope, cross-fall |
| Quantity Takeoff | 10 | Cut/fill volumes, earthwork reports, material summary |
| Hydrology | 5 | Flow paths, low point, watershed delineation, catchment, rational method |
| Sample Lines | 4 | Group creation, section views, material table |
| Data Shortcuts | 4 | Create, reference, synchronize, export |
| Labels / Styles | 4 | Apply label sets, import/export styles |
| Utilities | 9 | Drawing context, layer management, export formats |

**Architecture:**
- MCP protocol over HTTP POST `/execute`
- Tool registry with per-tool Zod validation
- `civil3d_hydrology` tool handles 5 hydrological analysis actions

---

### AI CoPilot Plugin (C#)

**Command routing (all 150 categories):**
- `IsMcpDirectCommand()` — routes `civil3d_*` and `create_cogo_point` to MCP async handler
- `IsHydrologyCommand()` — routes hydrology commands to async MCP → CAD draw pipeline
- `CommandRouter.Execute()` — synchronous CAD transaction handler for 14 draw command types
- `HandleMcpDirectCommandAsync()` — async MCP caller with formatted JObject result display

**Hydrology integration:**
- `HydrologyDrawer.cs` — draws flow paths (cyan), watershed boundaries (yellow), outlet markers (red)
- `McpClient.cs` — typed async methods for all 5 hydrology tools with result model classes
- Sequential workflow support: `lastHydroOutlet` chains `FindLowPoint` → `DelineateWatershed`

**AI providers supported:** Gemini, OpenAI, Anthropic

---

### Integration Tests

- 13 test files, **372 tests**, all passing
- End-to-end workflow tests covering alignment → surface → corridor → profile pipeline
- Hydrology workflow tests covering flow path → low point → watershed → catchment → runoff

---

### Bug Fixes

- **JFS-15**: Fixed structural C# compile error — hydrology methods were accidentally placed outside the `McpClient` class body
- **JFS-15**: Made `ExecuteMcpToolAsync` public (was private) to enable direct MCP command routing

---

### Known Limitations

- Civil 3D API calls require an active drawing session — no headless mode
- Hydrology delineation uses grid sampling (not DEM-optimized); large surfaces may be slow
- C# plugin targets .NET Framework 4.8 (AutoCAD/Civil 3D requirement)
