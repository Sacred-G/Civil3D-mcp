# Changelog

## Unreleased

### Fixed

- Style lookups by name never matched on Civil 3D 2027, so a requested style
  was silently replaced by the drawing's first one: `create_layout` with style
  "Design Profile" got "Existing Ground Profile", and `view_create` with style
  "Profile View" warned the style was missing. Civil 3D 2027's `StyleBase`
  declares a set-only `Name` that hides the readable base-class `Name`, so the
  reflective name read returned null for every style, label set and band set.
  Names are now read through the documented `DBObject.Name`, and the
  reflection boundary resolves a readable (or writable) declaration when a
  derived class hides one. A style, label set or band set name that does not
  exist is now `CIVIL3D.INVALID_INPUT` listing the available names instead of
  a silent substitution. No name still means the drawing's first one, except
  the section view band set and group plot style lookups, which pass Civil
  3D's default (a null id), and the profile view band set lookup, which uses
  the first band set only where the caller asks for it (`view_create` does).
  `create_layout` / `create_from_surface` now report the style and layer
  applied. Affects every `LookupUtils` style lookup (alignment, profile,
  surface, alignment/profile label set, profile view style and band set,
  parcel style, parcel area label style, section view style and band set,
  group plot style) and the name reads in the label and pipe-network style
  lookups.
- `civil3d_profile view_create` always failed on Civil 3D 2027 ("ProfileView.Create
  returned null"): the plugin probed `ProfileView.Create` by reflection with
  argument orders that do not exist. It now calls the typed
  `Create(alignmentId, insertPosition, name, bandSetId, styleId)` overload,
  falling back to the drawing's first profile view style and band set when none
  is named. When the drawing has only one of the two, the view is created with
  Civil 3D's defaults, the one that exists is applied to it, and a warning
  names the one that was missing; a named style or band set that cannot be
  applied, or a view that cannot be given the requested name, fails with
  `CIVIL3D.INVALID_INPUT` and nothing is created. New optional `layer`; the
  result reports name, handle, layer, style and band set.
- The profile view band-set lookup read `ProfileViewBandSetStyles` from the
  label-set styles root, where it does not exist, so it always returned a null
  id (`view_band_set` imported nothing). It now reads `Styles.ProfileViewBandSetStyles`.
- A requested layer that did not exist was silently replaced by the current
  layer (e.g. `create_layout layer:"C-ROAD-DES"` landed on layer 0). Missing
  layers are now created; an invalid layer name is `CIVIL3D.INVALID_INPUT`.
  Applies to every create path that takes `layer` (profiles, alignments,
  offset alignments, polylines, text, lines, profile views).
- `civil3d_profile check_k_values` computed K with A as a decimal grade
  difference (0.028) instead of percent (2.8) — a 280 ft curve reported
  K ≈ 9,980 — and always used a metric table, so 50 (mph) required sag 9 /
  crest 4. K is now L / (100·|g2−g1|), checked against the AASHTO
  stopping-sight-distance design K tables in mph (ft/%, 15-80 mph) or km/h
  (m/%, 20-130 km/h). New
  optional `speedUnits` (`mph` | `km/h`) defaults from the drawing's length
  unit; speeds between rows use the next higher row (and say so); speeds
  outside the table are `CIVIL3D.INVALID_INPUT`. Per curve it now returns
  grades in percent, A in percent, K, required K, start/end and PVI station.
  A profile with no vertical curves reports `allPass: false` with a warning,
  since nothing was checked.
- `civil3d_profile get` reported symmetric parabolas as `asymmetric_parabola`
  ("parabolasymmetric" contains "asymmetric"); they are now
  `symmetric_parabola`. The response schema still accepts the old `parabola`.

## v1.2.1 — 2026-07-14

### Production readiness

- Added a self-contained Claude Desktop MCPB package, a byte-identical legacy
  DXT compatibility artifact, installer configuration, checksum generation, and
  official manifest validation through `npm run package:claude`.
- Added a compact 33-tool default MCP surface while keeping 216 manifest-backed
  canonical and alias routes available internally; set
  `CIVIL3D_ENABLE_TOOL_ALIASES=true` to expose the full 218-tool MCP surface.
- Added first-class MCP output schemas, structured content, annotations,
  resources, progress notifications, and report-resource retrieval.
- Added parameter-bound, drawing-bound approval receipts, bounded serialized
  host execution, disconnect cancellation, idempotency, and stable errors.
- Added bounded background jobs for publishing, imports, corridor rebuilds, and
  bulk QC, with terminal retention and cancellation telemetry.
- Added liveness, readiness, plugin, queue, and version endpoints on the local
  HTTP bridge at port 3000.
- Added rotating native plugin logs with file-health and last-error telemetry.
- Added CI, generated tool documentation, manifest/dispatcher parity, package
  inspection, startup smoke, and opt-in live Civil 3D host validation.
- Removed tracked Autodesk assemblies; Release builds resolve licensed Civil 3D
  2026 references through `Civil3DReferencesPath`.

### Live validation

- Validated plugin 1.2.1.0 against Civil 3D 2026 on port 8080.
- Validated the spawned stdio MCP client, HTTP registration and routing,
  approvals, concurrency, document switching, create/undo/delete, temporary
  TIN-volume rollback, job cancellation/completion, and response limits.

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
