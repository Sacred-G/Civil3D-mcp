# Changelog

## Unreleased

### Fixed

- `civil3d_drawing` `settings` failed response validation on every drawing:
  the plugin reports `defaultStyles.corridor` as `null` unconditionally (Civil
  3D exposes no corridor style collection) and the schema rejected `null`. The
  five `defaultStyles` fields are now `string | null`, with regression tests.
- `docs:check` and `version:check` reported generated files as stale on any
  Windows clone with `core.autocrlf=true`; comparisons now ignore CRLF/LF.

### Added

- Civil 3D 2027 build path: `scripts/gather-refs-2027.ps1` stages the six
  managed references (spread across three folders in a 2027 install) and
  `scripts/build-2027.ps1` builds with a `net10.0-windows` override — Civil 3D
  2027's assemblies target .NET 10 — without changing the 2026 default.
- `scripts/install-bundle.ps1` deploys the plugin as an ApplicationPlugins
  bundle, which auto-loads under the default `SECURELOAD=1` policy that rejects
  Startup Suite entries outside `TRUSTEDPATHS`. The per-user bundle folder is
  not a trusted location, so Civil 3D 2026 asks about the unsigned
  `Civil3DMcpPlugin.dll` at every start (answer Load Once).
- `scripts/install-trusted.ps1`: installs the plugin into a trusted folder
  (`-Destination`, which must be in `TRUSTEDPATHS`; default
  `C:\CAD-Tools\plugins\Civil3DMcp`, one `R<series>` folder per release) and
  NETLOADs it from a
  marked `acaddoc.lsp` block, so Civil 3D no longer asks "Security - Unsigned
  Executable File" at every start as it does for the `%APPDATA%` bundle; removes
  the bundle; `-Uninstall` (with the install's `-SourceDir`) reverses. The plugin then loads when a drawing opens.
  An `acaddoc.lsp` further down the support path, which the new file would
  shadow, is loaded first (so its `S::STARTUP` is chained, not lost) and stays
  chained when the installer runs again, also after user code was added to the
  file or while that file is unreachable (only `-Uninstall` drops it, with a
  warning when user code keeps the file). Every lock (a changed build, the old
  bundle) is checked before anything is changed; `-Uninstall` leaves
  `acaddoc.lsp` files without our block untouched; ANSI files keep their
  encoding under PowerShell 7 too.
- `claude-bootstrap-and-run.ps1` runs `npm install --no-save` (the committed
  `package-lock.json` stays as it is) when `package.json` or
  `package-lock.json` changed since the last install, and rebuilds when any
  build input changed (source files, including deletions and renames, the
  package files, `tsconfig.json`, the copied assets), not only when there is no
  build: after a `git pull`, new tools appear without a manual build. A failed
  or interrupted build is rebuilt at the next start instead of running half a
  build. Concurrent starts of one clone take turns through a named mutex and
  reuse the first one's install and build; a start with nothing to do never
  waits. npm output goes to `%TEMP%\civil3d-mcp-bootstrap-<clone key>.log`
  (its tail to stderr on failure), never to stdout, and an npm notice on
  stderr no longer stops the start under Windows PowerShell 5.1.
- `gather-refs-2027.ps1` hashes with .NET SHA-256 instead of `Get-FileHash`,
  which Windows PowerShell 5.1 cannot load when started from PowerShell 7
  (it inherits 7's `PSModulePath`).

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
