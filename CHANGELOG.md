# Changelog

## Unreleased

### Fixed

- Every plugin request could hang until Civil 3D restarted, timing out after
  120 s each time. Once Civil 3D 2027 stops running
  `DocumentManager.ExecuteInCommandContextAsync` callbacks, it never resumes.
  - It was first seen after a LISP prompt left open over COM.
  - Diagnosed live: in a disconnected Windows session (RDP closed, nobody at
    the keyboard), the first COM `SendCommand` from any client is enough. Any
    client means PowerShell, another add-in, or this plugin. It also happened
    with only this plugin loaded. In a connected session the same commands
    are harmless.

  The plugin no longer uses that route:
  - plain reads run from an `Application.Idle` tick with an
    `OpenCloseTransaction`, so they add no undo steps and keep the undo
    history (a regular transaction under a document lock outside a command
    wipes it);
  - writes, and reads that create scratch objects (volume surfaces) or use the
    pickfirst selection, run inside the plugin's own `C3DMCPHOP` command. It is
    started through COM `SendCommand` only while the editor is quiescent, is
    flagged `UsePickSet` and `Redraw`, and each write is one undoable step;
  - command-line input (undo, redo, Storm and Sanitary, data-shortcut
    commands) is sent through COM `SendCommand` after the operation instead of
    `SendStringToExecute`, whose queued input mixed with the plugin command's;
  - `redoDrawing` sends `MREDO n` (`REDO` takes no count). Undo and redo take
    no document lock, because taking one (even for a read) empties the redo
    stack. Both requests return only after the command has run (or fail with
    `CIVIL3D.HOST_BUSY` if it is still running after 15 s);
  - pay item and cost estimate CSV exports use the plugin command only when
    they compute earthwork (a scratch volume surface); pipe and structure
    quantities alone are a plain read.

  While a command is active for 15 s, requests fail with `CIVIL3D.HOST_BUSY`
  instead of timing out. Requests that need the plugin command or a quiescent
  editor (writes, the reads above, command-line input) also wait for a prompt
  with `CMDACTIVE` 0, such as a LISP `getstring`, and fail the same way. A
  plain read is not held up by such a prompt and runs while it is open. A COM
  call that Civil 3D keeps rejecting is also `CIVIL3D.HOST_BUSY`, and a request
  whose operation fails always ends with that error instead of waiting with
  the plugin's execution gate held. Plugin calls no longer cancel a command or
  prompt the user has open.

  A request cancelled while its operation is already running (client
  disconnect or timeout) still returns at once, but the next request now
  waits until that operation has finished; before, it could start beside it.
  That wait has no 15 s `CIVIL3D.HOST_BUSY` check: a queued request that is
  still waiting when its own timeout expires ends with `CIVIL3D.TIMEOUT`.
  Work that had not started when the request ended never runs, and a
  rejected command-line send is not retried after its request was cancelled
  or had stopped waiting for it. Command-line
  input that Civil 3D rejects or that fails to send now fails the request
  (`CIVIL3D.HOST_BUSY` or `CIVIL3D.COMMAND_FAILED`) instead of being reported
  as `initiated`; a command that opens a dialog is reported once Civil 3D has
  accepted it.
- While a modal dialog was open (e.g. the Drawing Recovery notice at startup)
  requests timed out after 120 s: Civil 3D raises no `Application.Idle` inside
  a modal loop. A watchdog on the timer thread now fails the request with
  `CIVIL3D.HOST_BUSY` after 15 s without an Idle tick.
- With no document open, drawing-dependent requests hung and wedged the
  plugin's execution gate; they now fail fast with `CIVIL3D.NO_DRAWING`, and
  `civil3d_drawing new` works from zero documents. A client that aborts a
  request while its TCP connection is still being set up is now settled at
  once instead of after the connect timeout (`verify-live-plugin.mjs`'s
  no-drawing watchdog relies on this).

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
