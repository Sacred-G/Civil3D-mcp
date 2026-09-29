# Changelog

## Unreleased

### Added

- Read-only geometry reads, written to support a Civil 3D-Revit bridge. None
  of these is approval-gated (`query`/`inspect`, `safeForRetry`):
  - `getSurfaceTinVertices` (`civil3d_surface get_tin_vertices`): the vertices
    of a TIN surface's visible triangles (or a grid surface's visible points),
    optionally clipped to a plan polygon. Over `maxPoints` (default 50,000,
    hard cap 100,000) they are decimated deterministically: sorted by X then Y,
    with an even stride. Coordinates are rounded to 6 decimals to keep the
    largest response near 6 MB. TIN volume surfaces are rejected clearly.
  - `getParcelGeometry` (`civil3d_parcel get_geometry`): real parcel boundary
    from the typed curve API (base curve, then `GetGeCurve`, then `Explode`)
    with line/arc segments, bulges, a densified polygon, perimeter and area
    computed including arcs. Parcels with holes return the outer loop only.
    `reportParcels` still uses its reflection path.
  - `getDrawingUnits` (`civil3d_drawing units`): raw INSUNITS and its name, a
    `lengthUnit` that keeps `USSurveyFeet` distinct from `Feet`, metres per
    unit, the Civil 3D drawing unit settings (Feet/Meters and the
    imperial-to-metric foot), angular units, and a consistency warning when
    INSUNITS and Civil 3D disagree.
  - `getPipeNetwork`/`getPipe` pipes gain `startPoint`, `endPoint`,
    `startInvert`, `endInvert`, `startCrown`, `endCrown`, `innerDiameter`,
    `outerDiameter`, `innerHeight`, `outerHeight`, `wallThickness`,
    `crossSectionalShape` and `length2d` (additive; inverts are centreline
    minus inner height / 2).
  - `getDrawingInfo` and `getCoordinateSystemInfo` gain an additive
    `lengthUnit`; `linearUnits` keeps its old values.
  - Host-independent `BridgeMath.cs` (arcs, bulge areas, chaining,
    point-in-polygon, stride decimation, unit resolution) with an offline
    harness, `npm run test:bridge-math`. Verified live against Civil 3D 2027.

### Fixed

- `getPipeNetwork` (`civil3d_pipe get`) failed with CivilException "Retrieve
  attribute failed" on networks whose style, parts list, or reference
  surface/alignment attributes are unset (seen on the Civil 3D 2027 Pipe
  Networks-3 tutorial drawing), although each pipe and structure read fine on
  its own. Those network attributes, and part-family-specific pipe attributes,
  are now read defensively and reported as `null`.
- The host executor rethrew captured exceptions with `throw ex`, which dropped
  the original stack from the plugin log; it now uses `ExceptionDispatchInfo`.

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
