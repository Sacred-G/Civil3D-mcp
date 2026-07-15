<div align="center">
  <img src="./image/civil3d_dynamic_mcp_blueprint/dffc9d11-a9c7-4794-bf73-ff1a39c72e3a.jpg" alt="Civil3D-MCP dynamic blueprint" width="420" />

  <h1>Civil3D-MCP Server</h1>

  <p><strong>Connect Claude, Cursor, Cline, and other MCP clients directly to a live Autodesk Civil 3D drawing.</strong></p>
  <p>Node.js MCP server + native Civil 3D plugin + workflow automation for grading, hydrology, plan production, QC, and data shortcuts.</p>

  [![Node.js](https://img.shields.io/badge/Node.js-18%2B-339933?logo=nodedotjs&logoColor=white)](https://nodejs.org)
  [![TypeScript](https://img.shields.io/badge/TypeScript-5.8-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org)
  [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
  [![MCP SDK](https://img.shields.io/badge/MCP%20SDK-1.29-blueviolet)](https://modelcontextprotocol.io)
  [![Civil 3D](https://img.shields.io/badge/Civil%203D-2026-0696D7?logo=autodesk&logoColor=white)](https://www.autodesk.com/products/civil-3d)
  [![Issues](https://img.shields.io/github/issues/Sacred-G/Civil3D-mcp)](https://github.com/Sacred-G/Civil3D-mcp/issues)

  <p>
    <a href="#quick-start">Quick Start</a> •
    <a href="./docs/USER_MANUAL.md">User Manual</a> •
    <a href="#autodesk-help-in-chat">Autodesk Help</a> •
    <a href="#installation">Installation</a> •
    <a href="./docs/DEPLOYMENT.md">Deployment</a> •
    <a href="./docs/tools.generated.md">Generated Tool Reference</a>
  </p>

</div>

---

## What Is This?

**Civil3D-MCP** bridges AI assistants (Claude, Cline, Cursor, etc.) to a **live, open Civil 3D drawing** using the [Model Context Protocol](https://modelcontextprotocol.io). Give Claude your design brief — it reads your drawing, runs calculations, and makes changes in real time.

```text
MCP client  <-- stdio ----------->  civil3d-mcp (Node.js)
HTTP client <-- HTTP :3000 ------>  civil3d-mcp (Node.js)
civil3d-mcp <-- JSON-RPC/TCP :8080 --> Civil3DMcpPlugin.dll --> Civil 3D 2026 API
```

This is the **MCP server** (TypeScript). You also need the **Civil 3D .NET plugin** — see [Installation](#installation).

### What Changed Recently

- Production-ready host execution now includes bounded queues and jobs,
  cancellation, idempotency, structured telemetry, readiness endpoints, and
  observable rotating plugin logs.
- **New:** `civil3d_help` searches the locally installed Autodesk Civil 3D help,
  returns cited topics with screenshots, and surfaces matching tutorial videos in chat.
- The default MCP surface is compact: 34 public tools cover every domain. The
  complete 206-entry catalog can be enabled when a client needs specialized aliases.
- Native workflow handlers cover major QC, grading, hydrology, plan-production,
  and data-shortcut flows instead of relying only on client-side orchestration.
- Claude Code can be registered with a single project-scoped command that
  installs dependencies, builds the Node server, and writes the repo `.mcp.json` entry.
- Claude Desktop now has a self-contained MCPB installer plus a legacy DXT
  compatibility artifact, generated with `npm run package:claude`.
- The release-checked inventory is generated from the runtime manifest at
  [`docs/tools.generated.md`](./docs/tools.generated.md).

---

## Autodesk Help in Chat

Agents can now answer Civil 3D how-to questions from the Autodesk Offline Help
installed on the user's machine. The `civil3d_help` tool auto-discovers Civil 3D
help under Program Files, builds a local search index, and returns:

- cleaned, version-matched Autodesk topic text with a canonical citation;
- useful screenshots and diagrams directly in the chat response;
- the best matching Autodesk tutorial video as a player resource;
- a direct MP4 link when the chat client cannot embed the player.

For example, an agent can search for grading guidance:

```json
{
  "action": "search",
  "query": "how do I create grading criteria?",
  "version": "2026"
}
```

Or request only matching videos:

```json
{
  "action": "search_videos",
  "query": "create grading criteria",
  "maxVideos": 2
}
```

The bundled 2026 catalog currently contains 47 Civil 3D tutorial videos. See the
[Autodesk grading-criteria video](https://help.autodesk.com/videos/hmdXBmbDrmD4RJG46g4xxLXnNrBqHIIC/video.mp4)
for an example. Topic and image retrieval remains local and works without the
Civil 3D drawing plugin; Autodesk-hosted video playback requires internet access.

The first topic query creates a compressed cache under
`%LOCALAPPDATA%\Civil3DMcp\help-index`. Later queries reuse that cache and full
topic retrieval is effectively immediate. See [Local Autodesk help search](#local-autodesk-help-search)
for configuration and additional examples.

---

## Quick Start

If you want the fastest path from clone to a working Claude + Civil 3D setup on Windows:

For Claude Desktop, a release `.mcpb` is the easiest Node-side installation:

1. Open **Settings > Extensions > Advanced settings > Install Extension**.
2. Select `civil3d-mcp-<version>.mcpb` and keep ports `8080` and `3000` unless
   your local setup uses different ports.
3. Open Civil 3D and load the native plugin with `NETLOAD` or the `APPLOAD`
   Startup Suite.
4. Ask Claude to run `civil3d_health`.

The extension installs the Node MCP server and its production dependencies. It
does not install the native Autodesk plugin. Maintainers can build both the
current `.mcpb` and legacy `.dxt` files with:

```powershell
npm install
npm run package:claude
```

For a source-based or Claude Code installation:

1. Clone and build the Node side:

   ```bash
   git clone https://github.com/Sacred-G/Civil3D-mcp.git
   cd Civil3D-mcp
   npm install
   npm run build
   ```

2. Build and `NETLOAD` the Civil 3D plugin:

   ```powershell
   dotnet build .\Civil3D-MCP-Plugin\Civil3DMcpPlugin.csproj -c Release `
     /p:Civil3DReferencesPath="C:\Program Files\Autodesk\AutoCAD 2026\C3D"
   ```

3. Register Claude Code for this repo:

   ```powershell
   npm run claude:add
   ```

4. Open Civil 3D, load the plugin, and start asking for real work:
   - "Audit this drawing before design starts."
   - "Create a grading surface from this feature line."
   - "Delineate the watershed and estimate runoff."
   - "Publish the plan/profile sheets to PDF."

---

## Features

- **Compact default MCP surface** with 34 public tools; all 206 cataloged
  routes remain internally callable or available as opt-in aliases
- **Local Autodesk help in chat** — version-matched topics, screenshots,
  diagrams, citations, and playable Civil 3D tutorial videos
- **Native workflow execution** for corridor QC, surface comparison, project startup, grading conversion, plan publish, data shortcuts, and hydrology pipelines
- **Full road design pipeline** — alignments, profiles, corridors, cross-sections, superelevation
- **Surface analysis** — elevation bands, slope distribution, aspect, watershed, cut/fill volumes
- **Pipe & pressure network** design, validation, and hydraulic analysis
- **Plan production** — existing sheet-set inspection, Plan/Profile updates, views, and PDF export
- **QC checks** — alignment, profile, corridor, surface, pipe network, drawing standards
- **Quantity takeoff** — earthwork, pipe lengths, parcel areas, and CSV export
- **Cost estimation** — pay items, material costs, construction estimates
- **Hydrology** — flow path tracing, watershed delineation, Rational Method runoff, time of concentration
- **Grading** — feature lines, grading groups, grading criteria, surface generation
- **COGO/Survey** — inverse, traverse, curve solve, survey databases, observations, and figure inspection
- **Storm & Sanitary Analysis (SSA)** integration and detention basin sizing
- **Intersection inspection** and corridor target mapping
- **Sight distance** calculations and AASHTO stopping sight distance checks
- **Assembly/subassembly** creation and editing

### Protected action flow

Destructive, file-writing, import/export, and other non-retryable drawing
mutations use a three-step policy flow:

1. Call `civil3d_preview_action` with the target `toolName`, `action`, and the
   exact parameters to validate the operation and learn whether approval is required.
2. If required, call `civil3d_request_approval` with the same values. It issues
   a short-lived, single-use token bound to the active drawing and those exact parameters.
3. Retry the target tool call with `approvalToken`. Changing the drawing,
   action, or parameters invalidates the token.

Set `CIVIL3D_APPROVAL_MODE=disabled` only for isolated local development or
test environments; production deployments should retain the default policy.

### Canonical tools and aliases

Aliases do not unlock extra Civil 3D capabilities. They are narrower convenience
names for actions already available through a canonical domain tool.

| Surface | Exposed tools | How to call an operation |
|---|---:|---|
| Default MCP client | 34 | Call a canonical domain tool and provide `action`. |
| MCP client with aliases enabled | 206 | Use either canonical tools or specialized alias names. |
| HTTP `/execute` and orchestration | 206 | All registered routes remain internally callable in either mode. |

For example, these calls are equivalent:

```json
{
  "tool": "civil3d_surface",
  "parameters": {
    "action": "volume_calculate",
    "baseSurface": "Existing Ground",
    "comparisonSurface": "Finished Grade"
  }
}
```

```json
{
  "tool": "civil3d_surface_volume_calculate",
  "parameters": {
    "baseSurface": "Existing Ground",
    "comparisonSurface": "Finished Grade"
  }
}
```

The compact default is recommended because it gives the model fewer overlapping
tool descriptions while preserving every operation. To expose all specialized
aliases, set this variable in the environment that starts the MCP server, then
restart the client connection:

```powershell
$env:CIVIL3D_ENABLE_TOOL_ALIASES = "true"
npm run start
```

For a client-managed server, put the variable in its MCP configuration:

```json
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "node",
      "args": ["C:/path/to/civil3d-mcp/build/index.js"],
      "env": {
        "CIVIL3D_ENABLE_TOOL_ALIASES": "true"
      }
    }
  }
}
```

Discover operations through the `civil3d://catalog/tools` MCP resource or
[`docs/tools.generated.md`](./docs/tools.generated.md). Canonical rows list
their supported actions; alias rows provide the corresponding convenience name.
Approval requirements are identical for canonical and alias calls.

---

## Architecture

```mermaid
flowchart LR
    Client["MCP Client\n(Claude / Cline / Cursor)"] <-->|"MCP stdio"| Server["civil3d-mcp\nNode.js"]
    HttpClient["Local HTTP Client"] -->|"HTTP :3000"| Server
    Server <-->|"JSON-RPC/TCP :8080"| Plugin

    subgraph Civil3D ["Autodesk Civil 3D"]
        Plugin["Civil3DMcpPlugin.dll\nTCP Listener"]
        Queue["Bounded Host Queue"]
        Context["ExecuteInCommandContextAsync"]
        Safety["Document Lock + Transaction"]
        API["Civil 3D 2026 .NET API"]

        Plugin --> Queue --> Context --> Safety --> API
    end
```

Civil 3D database operations execute inside Autodesk command context under a
bounded, serialized host gate. Mutations additionally use a document lock and a
narrow transaction; callers never mutate a drawing directly from Node.js.

---

## Requirements

| Component | Version |
|---|---|
| Node.js | 18+ |
| Civil 3D | 2026 (live validated) |
| .NET SDK | 8.0 |
| Civil 3D API refs | Licensed local installation or untracked reference directory |
| Visual Studio | 2022 recommended |

---

## Installation

### 1 — Build the MCP Server (TypeScript)

```bash
git clone https://github.com/Sacred-G/Civil3D-mcp.git
cd Civil3D-mcp
npm install
npm run build
```

### 1.5 — One-Line Claude Code Setup

From the repo root on Windows, this project-scoped command adds the MCP server to Claude Code and creates the repo `.mcp.json` entry automatically:

```powershell
claude mcp add --scope project --transport stdio civil3d-mcp -- powershell -NoProfile -ExecutionPolicy Bypass -File "$PWD\scripts\claude-bootstrap-and-run.ps1"
```

The registered launcher will automatically install npm dependencies and build the Node server the first time Claude starts it.

If you want a wrapper that does the install, build, and `claude mcp add` step for you immediately, run:

```powershell
npm run claude:add
```

Useful variants:

```powershell
npm run claude:add:user
npm run claude:print-add
```

### 1.6 — Claude Desktop Extension Package

Current Claude Desktop releases install MCP extensions from `.mcpb` files. The
former DXT format was renamed to MCPB, so the packaging command also emits a
byte-for-byte `.dxt` compatibility copy for older releases:

```powershell
npm run package:claude
```

Generated files are written to `dist/claude-desktop/` and include SHA-256
checksums. Install the `.mcpb` from **Claude Desktop > Settings > Extensions >
Advanced settings > Install Extension**. The package exposes installer fields
for the Civil 3D host and plugin port, alias mode, log level, and the local HTTP
bridge port (default `3000`).

### 2 — Build & Load the C# Plugin

**Prerequisites:** Civil 3D 2026, .NET 8 SDK, and licensed local Civil 3D managed
API assemblies. Autodesk binaries are intentionally not tracked or published by
this repository.

```powershell
$refs = "C:\Program Files\Autodesk\AutoCAD 2026\C3D"
dotnet build .\Civil3D-MCP-Plugin\Civil3DMcpPlugin.csproj -c Release `
  /p:Civil3DReferencesPath="$refs"
# Output: Civil3D-MCP-Plugin\bin\Release\net8.0-windows\Civil3DMcpPlugin.dll
```

An untracked `C_References` directory remains the local fallback when
`Civil3DReferencesPath` is omitted.

At minimum, the configured directory must contain:

```text
accoremgd.dll
AcDbMgd.dll
acmgd.dll
AecBaseMgd.dll
AeccDbMgd.dll
AeccPressurePipesMgd.dll
```

**Load into Civil 3D:**

| Method | Steps |
|---|---|
| **Manual (NETLOAD)** | Open Civil 3D → type `NETLOAD` → browse to `Civil3D-MCP-Plugin.dll` |
| **Autoload (recommended)** | Add registry key — see [DEPLOYMENT.md](./docs/DEPLOYMENT.md) |

Once loaded, three commands are available in the Civil 3D command line:

| Command | Description |
|---|---|
| `C3DMCPSTART` | Start the JSON-RPC server (auto-starts on plugin load) |
| `C3DMCPSTOP` | Stop the JSON-RPC server |
| `C3DMCPSTATUS` | Show server status, port, and queue depth |

### 3 — Configure Your AI Client

**Recommended for Claude Code**

```powershell
npm run claude:add
```

That command registers the bootstrap launcher in Claude Code and keeps the repo self-contained. Use `npm run claude:print-add` if you want to inspect the exact `claude mcp add` command without modifying config.

**Claude Desktop** — install the generated `.mcpb` package. For source-based
development, `claude_desktop_config.json` remains supported:

```json
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "node",
      "args": ["C:/path/to/Civil3D-mcp/build/index.js"]
    }
  }
}
```

**Claude Code** — add via CLI:

```powershell
claude mcp add --scope project --transport stdio civil3d-mcp -- powershell -NoProfile -ExecutionPolicy Bypass -File "$PWD\scripts\claude-bootstrap-and-run.ps1"
```

Restart your client. When you see the **hammer icon**, the MCP connection is live.

> For Docker, npm publishing, environment variables, and alternate deployment paths see **[docs/DEPLOYMENT.md](./docs/DEPLOYMENT.md)**.

---

## Example Conversations

> *"What alignments are in this drawing and how long is each one?"*
> → `civil3d_alignment` → returns names, station ranges, lengths

> *"Run a full QC check on the corridor and give me a report."*
> → `civil3d_workflow` with `action: "corridor_qc_report"`

> *"Trace the flow path from coordinate (5000, 3200) and estimate the peak runoff using the Rational Method."*
> → `civil3d_hydrology` with `action: "watershed_runoff_workflow"`

> *"Publish the existing Plan Production sheet set to PDF."*
> → `civil3d_workflow` with `action: "plan_production_publish"`

> *"Size the storm drain network for a 10-year storm and check all pipe velocities."*
> → `civil3d_pipe` with `action: "size_network"`, then `action: "hydraulic_analysis"`

> *"Calculate cut/fill volumes between the existing ground and proposed surface."*
> → `civil3d_surface` with `action: "volume_calculate"`, then `action: "volume_report"`

---

## Tool Reference (206 catalog entries)

The generated, release-checked inventory is [docs/tools.generated.md](./docs/tools.generated.md).
It includes both canonical tools and specialized aliases. Canonical rows list
one or more operations; alias rows show an em dash in the **Operations** column.

<details>
<summary><strong>Drawing Info & Context (7 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `get_drawing_info` | Retrieves basic information about the active Civil 3D drawing |
| `list_civil_object_types` | Lists major Civil 3D object types present in the current drawing |
| `get_selected_civil_objects_info` | Gets properties of currently selected Civil 3D objects |
| `civil3d_health` | Reports Civil 3D connection and plugin status |
| `civil3d_drawing` | Manages drawing state, document info, save/undo operations |
| `civil3d_job` | Checks status of long-running async operations or requests cancellation |
| `list_tool_capabilities` | Lists domain and capability metadata for the full MCP tool catalog |

</details>

<details>
<summary><strong>Drawing Primitives (6 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `create_cogo_point` | Creates a single COGO point |
| `create_line_segment` | Creates a simple line segment |
| `acad_create_polyline` | Creates an AutoCAD 2D polyline in model space |
| `acad_create_3dpolyline` | Creates an AutoCAD 3D polyline in model space |
| `acad_create_text` | Creates AutoCAD DBText in model space |
| `acad_create_mtext` | Creates AutoCAD MText in model space |

</details>

<details>
<summary><strong>Alignment (9 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_alignment` | Reads alignments, converts stationing, create/delete |
| `civil3d_alignment_report` | Builds structured alignment geometry report |
| `civil3d_alignment_get_station_offset` | Returns station/offset of an XY point relative to an alignment |
| `civil3d_alignment_add_tangent` | Appends a fixed tangent entity to an alignment |
| `civil3d_alignment_add_spiral` | Appends a spiral (transition curve) to an alignment |
| `civil3d_alignment_delete_entity` | Deletes a tangent/curve/spiral entity by index |
| `civil3d_alignment_offset_create` | Creates a new offset alignment at a constant distance |
| `civil3d_alignment_set_station_equation` | Adds a station equation to an alignment |
| `civil3d_alignment_widen_transition` | Creates a variable-offset widening/narrowing transition region |

</details>

<details>
<summary><strong>Profile (10 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_profile` | Reads profiles, create/delete |
| `civil3d_profile_report` | Builds structured profile report with station/elevation sampling |
| `civil3d_profile_get_elevation` | Samples elevation and grade at a given station |
| `civil3d_profile_add_pvi` | Adds a PVI to a layout profile |
| `civil3d_profile_add_curve` | Adds a parabolic vertical curve at an existing PVI |
| `civil3d_profile_delete_pvi` | Deletes the PVI nearest to a specified station |
| `civil3d_profile_set_grade` | Sets the grade of a tangent entity |
| `civil3d_profile_check_k_values` | Validates K-values against AASHTO minimums for design speed |
| `civil3d_profile_view_create` | Creates a profile view at a specified insertion point |
| `civil3d_profile_view_band_set` | Applies a band set style to an existing profile view |

</details>

<details>
<summary><strong>Superelevation (4 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_superelevation_get` | Retrieve superelevation design data for an alignment |
| `civil3d_superelevation_set` | Apply superelevation using AASHTO attainment method |
| `civil3d_superelevation_design_check` | Validate max superelevation rates and attainment lengths |
| `civil3d_superelevation_report` | Generate formatted superelevation report |

</details>

<details>
<summary><strong>Surface (15 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_surface` | Reads surface data, create/delete |
| `civil3d_surface_edit` | Modifies surface data: points, breaklines, boundaries, contours |
| `civil3d_surface_statistics_get` | Comprehensive statistics: elevation range, area, point/triangle count |
| `civil3d_surface_contour_interval_set` | Set minor and major contour display intervals |
| `civil3d_surface_sample_elevations` | Sample elevations at grid points, discrete points, or transect |
| `civil3d_surface_analyze_elevation` | Elevation band distribution (area and percentage per band) |
| `civil3d_surface_analyze_slope` | Slope distribution (area and percentage per slope range) |
| `civil3d_surface_analyze_directions` | Aspect/facing direction breakdown by cardinal sectors |
| `civil3d_surface_volume_calculate` | Calculate cut/fill volumes between two surfaces |
| `civil3d_surface_volume_by_region` | Cut/fill volumes within a polygon region |
| `civil3d_surface_volume_report` | Formatted human-readable cut/fill volume report |
| `civil3d_surface_comparison_workflow` | Structured two-surface comparison with cut/fill volumes |
| `civil3d_surface_create_from_dem` | Create TIN surface from DEM file (.dem, .tif, .asc, .flt) |
| `civil3d_surface_watershed_add` | Add watershed analysis: drainage basins and flow paths |
| `civil3d_surface_drainage_workflow` | Surface drainage workflow: flow path, elevation sampling, runoff estimate |

</details>

<details>
<summary><strong>Corridor (6 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_corridor` | Reads corridor data, rebuild, volume operations |
| `civil3d_corridor_summary` | Builds corridor summary with surfaces and volume analysis |
| `civil3d_corridor_target_mapping_get` | Retrieve subassembly target mappings for a corridor |
| `civil3d_corridor_target_mapping_set` | Set/update subassembly target mappings (surfaces, alignments, profiles) |
| `civil3d_corridor_region_add` | Add a new region to a corridor baseline |
| `civil3d_corridor_region_delete` | Delete a region from a corridor baseline |

</details>

<details>
<summary><strong>Sections & Section Views (6 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_section` | Reads section data, sample line creation |
| `civil3d_section_view_create` | Create section views for a sample line group |
| `civil3d_section_view_list` | List section views in the drawing |
| `civil3d_section_view_update_style` | Update display/band set style on existing section views |
| `civil3d_section_view_group_create` | Create section views using the drawing's draft-placement settings |
| `civil3d_section_view_export` | Export section station, source, offset, and elevation data to CSV |

</details>

<details>
<summary><strong>Intersection Design (2 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_intersection_list` | List all intersections in the drawing |
| `civil3d_intersection_get` | Get detailed properties of an intersection |

</details>

<details>
<summary><strong>Grading & Feature Lines (14 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_grading` | Canonical grading domain tool for groups, gradings, criteria, and feature-line actions |
| `civil3d_feature_line` | Reads feature lines and exports them as 3D polylines |
| `civil3d_feature_line_create` | Create a new feature line from 3D points |
| `civil3d_grading_group_list` | List all grading groups in the drawing |
| `civil3d_grading_group_create` | Create a new grading group |
| `civil3d_grading_group_get` | Get detailed info about a grading group |
| `civil3d_grading_group_delete` | Delete a grading group and all its gradings |
| `civil3d_grading_group_volume` | Get cut/fill volume report for a grading group |
| `civil3d_grading_group_surface_create` | Create a surface from a grading group |
| `civil3d_grading_criteria_list` | List all available grading criteria sets |
| `civil3d_grading_list` | List all grading objects within a grading group |
| `civil3d_grading_create` | Create a new grading from a feature line |
| `civil3d_grading_get` | Get detailed properties of a grading object |
| `civil3d_grading_delete` | Delete a grading object by handle |

</details>

<details>
<summary><strong>Points & Point Groups (6 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_point` | Reads, creates, imports, deletes COGO points and point groups |
| `civil3d_point_export` | Export COGO points to text/CSV (PNEZD, PENZ, XYZD, XYZ, CSV) |
| `civil3d_point_transform` | Transform points by translation, rotation, and/or scale |
| `civil3d_point_group_create` | Create a new point group with filter criteria |
| `civil3d_point_group_update` | Update filter criteria and description of a point group |
| `civil3d_point_group_delete` | Delete a point group (points are NOT deleted) |

</details>

<details>
<summary><strong>COGO & Survey (8 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_cogo_inverse` | Calculate bearing and distance between two coordinate pairs |
| `civil3d_cogo_direction_distance` | Project a point from start coordinate given bearing and distance |
| `civil3d_cogo_curve_solve` | Solve a horizontal curve given any two curve elements |
| `civil3d_cogo_traverse` | Solve a traverse from start point through bearing/distance courses |
| `civil3d_coordinate_system` | Coordinate system info and coordinate transformations |
| `civil3d_survey_database_list` | List all survey databases |
| `civil3d_survey_figure_list` | List all survey figures |
| `civil3d_survey_figure_get` | Get 3D vertex data for a specific survey figure |

</details>

<details>
<summary><strong>Pipe Networks — Gravity (3 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_pipe_catalog` | Lists available pipe parts lists and part names |
| `civil3d_pipe_network` | Reads pipe network data: networks, pipes, structures |
| `civil3d_pipe_network_edit` | Creates and modifies pipe networks, pipes, and structures |

</details>

<details>
<summary><strong>Pressure Networks (15 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_pressure_network_list` | List all pressure networks |
| `civil3d_pressure_network_create` | Create a new pressure network |
| `civil3d_pressure_network_get_info` | Get detailed info about a pressure network |
| `civil3d_pressure_network_delete` | Delete a pressure network and all components |
| `civil3d_pressure_network_assign_parts_list` | Assign a parts list to a network |
| `civil3d_pressure_network_set_cover` | Set minimum cover depth for pipes in a network |
| `civil3d_pressure_network_validate` | Validate a network for cover violations and disconnections |
| `civil3d_pressure_network_export` | Export pressure network data as structured JSON |
| `civil3d_pressure_network_connect` | Connect two pressure networks by merging |
| `civil3d_pressure_pipe_add` | Add a pressure pipe segment |
| `civil3d_pressure_pipe_get_properties` | Get properties of a pressure pipe |
| `civil3d_pressure_pipe_resize` | Change pressure pipe size to a different catalog entry |
| `civil3d_pressure_fitting_add` | Add a pressure fitting (elbow, tee, reducer, cap) |
| `civil3d_pressure_fitting_get_properties` | Get properties of a pressure fitting |
| `civil3d_pressure_appurtenance_add` | Add a pressure appurtenance (valve, hydrant, meter) |

</details>

<details>
<summary><strong>Plan Production / Sheets (11 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_sheet_set_list` | List all Plan Production sheet sets |
| `civil3d_sheet_set_create` | Create a new Plan Production sheet set |
| `civil3d_sheet_set_get_info` | Get detailed info about a sheet set |
| `civil3d_sheet_set_export` | Export all sheets to a multi-page PDF |
| `civil3d_sheet_set_title_block` | Set or update title block template on a sheet |
| `civil3d_sheet_add` | Add a new sheet to an existing sheet set |
| `civil3d_sheet_get_properties` | Get full properties of a specific sheet |
| `civil3d_sheet_publish_pdf` | Publish sheet layouts to a PDF file |
| `civil3d_sheet_view_create` | Create a viewport/view on a sheet layout |
| `civil3d_sheet_view_set_scale` | Update the scale of a viewport |
| `civil3d_plan_profile_sheet_update_alignment` | Update alignment and/or profile on an existing sheet |

</details>

<details>
<summary><strong>QC Checks (8 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_qc_check_alignment` | QC alignment: tangent lengths, curve radii, spirals, design-speed compliance |
| `civil3d_qc_check_profile` | QC profile: max grade, K-values, sight distance requirements |
| `civil3d_qc_check_corridor` | QC corridor: invalid regions, missing targets, assembly gaps, rebuild errors |
| `civil3d_qc_check_pipe_network` | QC pipe network: cover depth, slope, velocity, connectivity |
| `civil3d_qc_check_surface` | QC TIN surface: elevation spikes, flat triangles, crossing breaklines |
| `civil3d_qc_check_labels` | Check labels for missing labels and style standard violations |
| `civil3d_qc_check_drawing_standards` | Audit drawing: layer naming, lineweights, colors |
| `civil3d_qc_report_generate` | Run full QC pass and write consolidated report to disk |

</details>

<details>
<summary><strong>Workflow Automation (13 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_workflow` | Canonical workflow tool for multi-step QC, grading, pipe-design, standards, and publishing |
| `civil3d_workflow_corridor_qc_report` | Run corridor QC and optionally generate a consolidated QC report |
| `civil3d_workflow_grading_surface_volume` | Calculate grading cut/fill volume between base and comparison surfaces |
| `civil3d_workflow_surface_comparison_report` | Run a structured surface comparison and follow it with a volume report |
| `civil3d_workflow_data_shortcut_publish_sync` | Publish a data shortcut and immediately synchronize it |
| `civil3d_workflow_data_shortcut_reference_sync` | Reference a project data shortcut and immediately synchronize it |
| `civil3d_workflow_project_startup` | Check Civil 3D health, inspect drawing readiness, and optionally create/save a startup drawing |
| `civil3d_workflow_project_reference_setup` | Reference one or more project data shortcuts, sync them, and optionally save |
| `civil3d_workflow_drawing_readiness_audit` | Check health, drawing state, selection context, and standards readiness in one audit |
| `civil3d_workflow_feature_line_to_grading` | Convert a feature line into grading and optionally create a grading surface |
| `civil3d_workflow_pipe_network_design` | Size a gravity pipe network and optionally run hydraulic analysis |
| `civil3d_workflow_plan_production_publish` | Publish a sheet set or explicit layout list to PDF output |
| `civil3d_workflow_qc_fix_and_verify` | Audit drawing standards, apply fixes, and verify the result |

</details>

<details>
<summary><strong>Quantity Takeoff (8 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_qty_surface_volume` | Cut/fill volumes between surfaces (with corridor or region scope) |
| `civil3d_qty_earthwork_summary` | Running earthwork cut/fill summary table |
| `civil3d_qty_pipe_network_lengths` | Total pipe lengths for a gravity pipe network |
| `civil3d_qty_pressure_network_lengths` | Total pipe lengths for a pressure network |
| `civil3d_qty_alignment_lengths` | Total length for one or more alignments |
| `civil3d_qty_parcel_areas` | Area, perimeter, and address data for parcels |
| `civil3d_qty_point_count_by_group` | Count COGO points per point group |
| `civil3d_qty_export_to_csv` | Export consolidated quantity takeoff report to CSV |

</details>

<details>
<summary><strong>Hydrology (7 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_hydrology` | Canonical hydrology tool for surface drainage, catchments, Tc, SSA, and multi-step workflows |
| `civil3d_catchment` | Manages catchments and catchment groups, including properties, flow paths, and boundaries |
| `civil3d_time_of_concentration` | Calculates Tc using supported methods and generates hydrographs |
| `civil3d_stm` | Opens interactive STM import/export and Storm and Sanitary Analysis dialogs |
| `civil3d_hydrology_watershed_runoff_workflow` | Runs low-point or outlet-based watershed delineation through runoff estimation |
| `civil3d_hydrology_runoff_detention_workflow` | Runs runoff estimation through detention sizing and optional stage-storage output |
| `civil3d_hydrology_runoff_pipe_workflow` | Runs runoff estimation through gravity pipe HGL and hydraulic analysis |

</details>

<details>
<summary><strong>Pipe Hydraulics (3 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_pipe_network_hgl` | Calculate Hydraulic Grade Line (HGL) for gravity pipe networks |
| `civil3d_pipe_network_hydraulics` | Perform full hydraulic capacity analysis on pipe networks |
| `civil3d_pipe_structure_properties` | Retrieve detailed properties of individual pipe structures |

</details>

<details>
<summary><strong>Pipe Design Automation (2 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_pipe_network_size` | Size gravity-network pipes from Manning full-flow capacity with catalog part selection |
| `civil3d_pipe_profile_view_automation` | Automate gravity-pipe profile-view setup with EG profile creation |

</details>

<details>
<summary><strong>Assembly Creation (3 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_assembly_create` | Create a new Civil 3D assembly at a specified model-space location |
| `civil3d_subassembly_create` | Add a subassembly from the Civil 3D catalog to an existing assembly |
| `civil3d_assembly_edit` | Inspect or modify an existing Civil 3D assembly (list, update, delete subassemblies) |

</details>

<details>
<summary><strong>Sight Distance (2 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_sight_distance_calculate` | Calculate AASHTO stopping, passing, or decision sight distance for design speed |
| `civil3d_stopping_distance_check` | Check stopping sight distance compliance along an alignment at station intervals |

</details>

<details>
<summary><strong>Detention & Stormwater (2 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_detention_basin_size_calculate` | Size a detention basin to reduce peak stormwater runoff |
| `civil3d_detention_stage_storage` | Generate stage-storage-discharge table for a detention basin surface |

</details>

<details>
<summary><strong>Slope Analysis (1 tool)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_slope_geometry_calculate` | Calculate daylight line coordinates and slope geometry for cut/fill sections |

</details>

<details>
<summary><strong>Cost Estimation (2 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_pay_items_export` | Extract Civil 3D quantities and export as structured pay item schedule to CSV/Excel |
| `civil3d_material_cost_estimate` | Generate construction cost estimate by combining quantities with unit prices |

</details>

<details>
<summary><strong>Survey Processing (1 tool)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_survey_observation_list` | List survey observations from a survey database |

</details>

<details>
<summary><strong>Data Shortcuts (4 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_data_shortcut_create` | Create data shortcuts for Civil 3D objects |
| `civil3d_data_shortcut_promote` | Promote data shortcut references to full editable objects |
| `civil3d_data_shortcut_reference` | Reference existing data shortcuts into the current drawing |
| `civil3d_data_shortcut_sync` | Synchronize outdated data shortcut references |

</details>

<details>
<summary><strong>Parcel Editing (4 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_parcel_create` | Create parcels from polylines, feature lines, or vertex lists |
| `civil3d_parcel_edit` | Edit parcel properties (name, style, label style, description) |
| `civil3d_parcel_lot_line_adjust` | Adjust lot lines to achieve a target area |
| `civil3d_parcel_report` | Generate parcel reports with coordinate and unit settings |

</details>

<details>
<summary><strong>Standards & Miscellaneous (7 tools)</strong></summary>

| Tool | Description |
|------|-------------|
| `civil3d_standards_lookup` | Looks up Civil 3D standards, template governance, layer/style guidance, and labeling conventions |
| `civil3d_assembly` | Lists and inspects assemblies and subassemblies |
| `civil3d_label` | Manages labels on Civil 3D objects |
| `civil3d_style` | Lists and inspects Civil 3D styles |
| `civil3d_parcel` | Reads parcel and site data |
| `civil3d_data_shortcut` | Manages data shortcuts: listing, syncing, creating references |
| `civil3d_orchestrate` | Routes natural-language Civil 3D requests to the best tool |

</details>

---

## Civil 3D Version Compatibility

| Civil 3D Version | Status | Notes |
|---|---|---|
| 2023 | Not currently validated | Requires a separate compatibility review and a build against matching licensed assemblies. |
| 2024 | Not currently validated | Requires a separate compatibility review and a build against matching licensed assemblies. |
| 2025 | Not currently validated | Requires a separate compatibility review and a build against matching licensed assemblies. |
| 2026 | Supported and live validated | Set `Civil3DReferencesPath` to the matching licensed 2026 managed assemblies. |

For startup-suite and autoload registry details, use the version-specific guidance in [docs/DEPLOYMENT.md](./docs/DEPLOYMENT.md).

---

## Environment Variables

The Node MCP server reads the following variables at startup. All are optional; defaults are shown in the **Default** column and are suitable for a local Civil 3D plus Copilot install.

### Plugin connection (Node → Civil 3D plugin)

| Variable | Default | Purpose |
|---|---|---|
| `CIVIL3D_HOST` | `localhost` | Host the Civil 3D plugin's JSON-RPC TCP server binds to. |
| `CIVIL3D_PORT` | `8080` | TCP port the plugin listens on (see `PluginRuntime.Port`). |
| `CIVIL3D_CONNECT_TIMEOUT` | `5000` | Milliseconds the server waits for a TCP connection to the plugin before giving up. |
| `CIVIL3D_COMMAND_TIMEOUT` | `120000` | Milliseconds any single JSON-RPC command may run before the Node side rejects the call. Increase for heavy corridor rebuilds. |
| `CIVIL3D_MAX_RESPONSE_BYTES` | `8388608` | Maximum buffered response size accepted from the Civil 3D plugin. |
| `CIVIL3D_LOG_LEVEL` | `info` | One of `debug`, `info`, `warn`, `error`. Logs are written to stderr. |

### MCP tool surface and policy

| Variable | Default | Purpose |
|---|---|---|
| `CIVIL3D_ENABLE_TOOL_ALIASES` | `false` | Set to `true` before the MCP server starts to expose all specialized aliases instead of the compact 34-tool surface. |
| `CIVIL3D_APPROVAL_MODE` | enforced | Set to `disabled` only in an isolated disposable test environment; production should retain parameter-bound approvals. |

### Local Autodesk help search

`civil3d_help` indexes the Autodesk Offline Help already installed with Civil 3D. It can search engineer phrasing such as “balance cut and fill” or “grading optimization,” return a cleaned Markdown topic, and attach the topic's useful screenshots and diagrams as MCP image content and `civil3d://help/images/...` resources. Matching Autodesk tutorial videos are returned as playable `civil3d://help/videos/...` player resources plus direct MP4 links. Decorative navigation icons are excluded. The drawing plugin does not need to be running for help searches.

The first query builds a compressed local index under `%LOCALAPPDATA%\Civil3DMcp\help-index`; later starts reuse it until the Autodesk help files change. By default the server discovers folders named `Offline Help for Civil 3D <year> - <language>\Help` under Program Files and prefers 2026.

| Variable | Default | Purpose |
|---|---|---|
| `CIVIL3D_HELP_ROOT` | Auto-discovered | Optional absolute path to a Civil 3D Offline Help `Help` folder. |
| `CIVIL3D_HELP_VERSION` | `2026` | Preferred installed documentation version. |
| `CIVIL3D_HELP_CACHE_ROOT` | Local AppData | Override the generated search-index directory. |
| `CIVIL3D_ENABLE_HELP_REINDEX` | `false` | Set to `true` to allow the tool's explicit `reindex` action. Automatic cache refresh remains enabled. |
| `CIVIL3D_HELP_MAX_IMAGE_BYTES` | `6291456` | Maximum total bytes of inline image content returned by one topic request. |
| `CIVIL3D_VIDEO_CATALOG` | Bundled 2026 catalog | Optional path to a refreshed JSON catalog produced by `scripts/scrape_autodesk_videos.py`. |

The normal `search` and `get_topic` actions include the best matching video by default. Use `includeVideos: false` to suppress it, `maxVideos` to change the limit, or `search_videos` for a video-only lookup. Autodesk hosts these Civil 3D videos online, so playback requires internet access even though topic and image search remains local.

Example calls:

```json
{ "action": "search", "query": "how do I use grading optimization?", "version": "2026" }
```

```json
{ "action": "get_topic", "id": "<id from search>", "version": "2026", "includeImages": true }
```

```json
{ "action": "search_videos", "query": "create grading criteria", "maxVideos": 2 }
```

### HTTP bridge (Copilot / local HTTP clients → Node)

| Variable | Default | Purpose |
|---|---|---|
| `MCP_HTTP_HOST` | `127.0.0.1` | Interface the HTTP bridge binds to. Keep this on loopback unless you know what you're doing. |
| `MCP_HTTP_PORT` | `3000` | Port the HTTP bridge listens on. |
| `MCP_HTTP_TOKEN` | *(unset)* | Optional shared secret. When set, every request to the bridge must include `Authorization: Bearer <token>` or `X-MCP-Token: <token>`. Leave unset to keep the loopback-only open mode. |
| `MCP_HTTP_MAX_BODY_BYTES` | `1048576` | Maximum accepted `POST /execute` body size in bytes. Requests exceeding this receive `413 Payload Too Large`. |
| `MCP_HTTP_ALLOWED_ORIGINS` | *(unset)* | Comma-separated browser origins allowed to call the bridge. Requests without an `Origin` header, such as native clients, are unaffected. |
| `MCP_HTTP_ALLOWED_HOSTS` | Loopback hostnames | Comma-separated HTTP `Host` names accepted by the bridge. Required with `MCP_HTTP_TOKEN` for non-loopback binds. Ports are ignored during comparison. |

The bridge refuses to start on a non-loopback host unless `MCP_HTTP_TOKEN` is
configured. Docker Compose also requires this token and publishes the bridge on
the host loopback interface by default.

### Plugin filesystem boundaries

Set these variables in the environment that launches Civil 3D. Roots are
semicolon-separated on Windows; every caller-supplied path is canonicalized and
must stay beneath an allowed root. If no roots are configured, the plugin uses
the current user's Documents folder.

| Variable | Default | Purpose |
|---|---|---|
| `CIVIL3D_FILE_ROOTS` | User Documents | Shared fallback roots for imports and exports. |
| `CIVIL3D_IMPORT_ROOTS` | `CIVIL3D_FILE_ROOTS` | Optional import-only roots for templates, DEM files, LandXML, STM, and other source artifacts. |
| `CIVIL3D_EXPORT_ROOTS` | `CIVIL3D_FILE_ROOTS` | Optional export-only roots for drawings and generated reports. |

Import/export tools enforce operation-specific extensions. Generated text and
CSV artifacts are written to a temporary file in the destination directory and
then atomically moved into place. Existing files are rejected unless the tool
call explicitly supplies `overwrite: true`.

### Plugin logging

The native plugin writes a rotating log to
`%LOCALAPPDATA%\Civil3DMcpPlugin\plugin.log` (5 MiB per file, three backups).
Set `CIVIL3D_MCP_LOG_DIR` or `CIVIL3D_MCP_LOG_LEVEL` before launching Civil 3D
to override its directory or minimum level. Plugin health reports
`logFilePath`, `fileLoggingHealthy`, and `fileLoggingError`, so a file-write
failure cannot remain silent.

### HTTP bridge endpoints

| Method | Path | Notes |
|---|---|---|
| `GET` | `/health` | Cheap liveness (bridge status + registered tool count). Does **not** touch the Civil 3D plugin. |
| `GET` | `/health/live` | Explicit alias for cheap bridge liveness. |
| `GET` | `/health?deep=1` | Runs the `civil3d_health` tool, which round-trips to the plugin. Use when you need to know Civil 3D is actually reachable. |
| `GET` | `/health/ready` | Readiness across the bridge, plugin connection, and bounded host queue; returns `503` when unavailable or full. |
| `GET` | `/health/plugin` | Native plugin health and drawing telemetry. |
| `GET` | `/health/queue` | Host queue and background-job health. |
| `GET` | `/health/version` | Package, MCP SDK, and Node dependency versions without probing Civil 3D. |
| `GET` | `/tools` | Full internal route catalog (206 names), independent of compact MCP exposure. |
| `POST` | `/execute` | `{ "tool": "<name>", "parameters": { ... } }` — invokes any registered tool or legacy alias. |

### Example: enable the shared-secret token

```powershell
$env:MCP_HTTP_TOKEN = "change-me-to-a-long-random-string"
npm run start
```

Then from the Copilot or any client:

```http
POST /execute HTTP/1.1
Host: 127.0.0.1:3000
Authorization: Bearer change-me-to-a-long-random-string
Content-Type: application/json

{ "tool": "civil3d_health", "parameters": {} }
```

### Verify the live Civil 3D plugin

With Civil 3D open, a drawing active, and `C3DMCPSTATUS` reporting port 8080,
run this read-only P0 smoke check:

```powershell
npm run test:live-plugin
```

The full opt-in host suite creates disposable drawings and covers query,
concurrency, document switching, create/undo, temporary volume-surface
rollback, delete, job completion, and cancellation. It is guarded to prevent
accidental mutations:

```powershell
$env:CIVIL3D_LIVE_SMOKE_CONFIRM = "DISPOSABLE_DRAWING"
$env:CIVIL3D_LIVE_SMOKE_QC_PATH = "$env:USERPROFILE\Documents\civil3d-mcp-live-qc.txt"
npm run test:live-host
```

To validate the richer contracts through a spawned production stdio MCP client
(typed tools, annotations, progress, resources, health, and drawing context), run:

```powershell
npm run test:live-mcp-client
```

For safe-to-retry actions, clients may provide an `idempotencyKey` (maximum 128
characters). Identical calls share the in-flight or recent result for two
minutes; reusing the key with different parameters returns `CIVIL3D.CONFLICT`.

It calls `getCivil3DHealth` and retrieves drawing context without mutating the
open drawing. Set `CIVIL3D_HOST` and `CIVIL3D_PORT` first if the plugin does not
use the default loopback endpoint.

---

## Troubleshooting

**"Cannot connect to Civil 3D plugin"**
- Civil 3D must be open with a drawing loaded
- Type `C3DMCPSTATUS` in Civil 3D — confirm the server shows as running on port 8080
- Check Windows Firewall is not blocking `localhost:8080`
- If you moved the plugin to a non-default port, set `CIVIL3D_PORT` on the Node side to match

**"No active document"**
- Open a `.dwg` file in Civil 3D before making any tool calls

**"Alignment/Surface not found"**
- Names are case-insensitive but must otherwise match exactly
- Use the corresponding list tool first (e.g. `civil3d_alignment` → `list` action)

**"I cannot see a specialized alias in my MCP client"**
- This is expected with the recommended compact 34-tool surface
- Call the canonical domain tool with its `action`, or set `CIVIL3D_ENABLE_TOOL_ALIASES=true` in the MCP server configuration
- Restart the MCP client connection after changing the environment; changing an unrelated terminal does not update an already-running client-managed server
- Use `civil3d://catalog/tools` or [tools.generated.md](./docs/tools.generated.md) to map aliases to canonical actions

**Plugin won't load**
- Verify the DLL targets the correct Civil 3D version
- Ensure Civil 3D and Visual Studio are running as the same Windows user (no elevation mismatch)
- Check the DLL path passed to `NETLOAD` has no spaces or special characters

---

## Documentation

| Doc | Description |
|---|---|
| [USER_MANUAL.md](./docs/USER_MANUAL.md) | End-user manual, daily workflows, prompt examples, approvals, local Autodesk help, and best practices |
| [DEPLOYMENT.md](./docs/DEPLOYMENT.md) | Full deployment: Docker, npm, env vars, registry autoload |
| [tools.generated.md](./docs/tools.generated.md) | Release-checked canonical and alias catalog generated from the runtime manifest |
| [tools.md](./docs/tools.md) | Extended parameter and workflow reference |
| [CHANGELOG.md](./CHANGELOG.md) | Release history |
| [ROADMAP.md](./ROADMAP.md) | Production-readiness phases and live-validation evidence |

---

## Contributing

Pull requests are welcome. For major changes, open an issue first to discuss what you'd like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/my-tool`)
3. Commit your changes (`git commit -m 'Add civil3d_my_tool'`)
4. Push to the branch (`git push origin feature/my-tool`)
5. Open a Pull Request

---

## License

[MIT](./LICENSE) — Steven Bouldin
