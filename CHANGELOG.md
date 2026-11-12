# Changelog

## Unreleased

### Added

- `civil3d_xref` domain: `list` reports each xref's saved and found paths,
  status (`loaded`, `unloaded`, `unreferenced`, `not_found`, `unresolved`,
  `orphaned`), attach or overlay, nesting and parents, and instance count.
  `attach`, `overlay`, `detach`, `reload`, `unload`, `bind` (`bind`/`insert`)
  and `repath` (`absolute`/`relative`) change xrefs and need approval. Every
  caller path goes through `FileBoundary` (import roots, `.dwg` only, must
  exist). Unload and detach run under the document lock with no enclosing
  transaction, through the new
  `CivilExecution.ExecuteLockedWithoutTransactionAsync`. Reload (including
  `repath`'s reload) and bind run the `-XREF` command through the new
  `CivilExecution.ExecuteCommandSequenceAsync`: on Civil 3D 2027,
  `Database.ReloadXrefs` and `BindXrefs` did their work but never returned
  control to the plugin's command context. Verified live against Civil 3D 2027.
- `civil3d_project data_shortcut_references` (read-only) lists the
  data-shortcut references in the current drawing with source drawing, source
  object name/type/handle, where the source is relative to the working folder
  and current project, and `current`/`out_of_date`/`broken`/`source_missing`/`unknown`
  status, all from `Entity.GetReferenceInfo()` and the `IsReference*` flags.
  `data_shortcut_repair` (needs approval) repoints a reference to a new source
  drawing through `DataShortcuts.RepairBrokenDRef`, resolved at runtime from
  `AeccDataShortcutMgd.dll` through `Civil3DCompatibility`, so the build needs
  no seventh reference.
- `civil3d_compare` domain, which never modifies a drawing. `drawing` compares
  the active drawing with another DWG that it reads as a side database with
  `ReadDwgFile`, never opening it as a document. `snapshot` writes a JSON
  fingerprint to the export roots; it is gated as an export and does not
  overwrite by default. `compare_snapshot` diffs against a snapshot. Results
  group added/removed/modified entities by type and layer and add Civil 3D
  object diffs: alignment length and geometry hash, profile PVIs, surface
  statistics, pipe counts and inverts (a pipe whose part defines no inner
  height, and is not circular, reports `null` inverts instead of dropping the
  network from the fingerprint). The host-independent diff
  (`DrawingFingerprint.cs`) has an offline harness,
  `npm run test:compare-diff`.

### Changed

- `data_shortcut_sync` sends `_AeccSynchronizeReferences`, the command name
  found in the Civil 3D 2027 CUIx and `AeccUiBase.arx`, where it used to send
  `SynchronizeReferences`. `data_shortcut_list` fills `sourceFilePath` from
  `GetReferenceInfo()`.
- `data_shortcut_promote` now finds the reference, preselects it, and queues
  `_AeccPromoteReference`, where it used to only return manual steps.

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
