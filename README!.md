# Civil 3D Desktop MCP — Plugin + Bridge

Connect Claude directly to a **live, open Civil 3D drawing** via a .NET plugin and MCP bridge server. Works exactly like Blender MCP — the plugin runs an HTTP server inside Civil 3D, and the MCP server forwards tool calls to it.

```
Claude ↔ civil3d-mcp-server (Node.js, stdio) ↔ HTTP localhost:8765 ↔ Civil3DMcpPlugin.dll (inside Civil 3D)
```

---

## Two Parts

| Part | Language | What it does |
|---|---|---|
| `Civil3DMcpPlugin/` | C# .NET 4.8 | Loads into Civil 3D, runs HTTP server, executes against full Civil 3D .NET API |
| `civil3d-mcp-server/` | TypeScript (Node.js) | MCP server — bridges Claude to the plugin via JSON over HTTP |

---

## Part 1 — Build the C# Plugin

### Prerequisites
- Visual Studio 2022 (or MSBuild)
- Civil 3D 2024 or 2025 installed
- .NET Framework 4.8 SDK

### Set the install path

Edit `Civil3DMcpPlugin.csproj` or set an environment variable:

```bash
# PowerShell
$env:CivilInstallPath = "C:\Program Files\Autodesk\AutoCAD 2025"
```

### Build

```bash
cd Civil3DMcpPlugin
dotnet restore
dotnet build -c Release
# Output: bin/Release/net48/Civil3DMcpPlugin.dll
```

### Load into Civil 3D

**Option A — NETLOAD (manual, each session):**
1. Open Civil 3D
2. Type `NETLOAD` in the command line
3. Browse to `Civil3DMcpPlugin.dll`
4. The plugin loads and automatically starts the HTTP server on port 8765

**Option B — Autoload (permanent, recommended):**
Add this registry key so Civil 3D loads the plugin on startup:

```
HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD\R25.0\ACAD-6001:409\Applications\Civil3DMcp
  LOADER  = C:\path\to\Civil3DMcpPlugin.dll
  MANAGED = 1
```

Or use the Autodesk `acad.lsp` / trusted paths approach documented in the Civil 3D developer guide.

### AutoCAD Commands

Once loaded, type these in the Civil 3D command line:

| Command | Description |
|---|---|
| `MCPSTART` | Start the HTTP server (auto-starts on load) |
| `MCPSTOP` | Stop the HTTP server |
| `MCPSTATUS` | Show server status and port |

---

## Part 2 — Run the MCP Server

```bash
cd civil3d-mcp-server
npm install
npm run build
npm start
```

### Claude Desktop Configuration

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "civil3d": {
      "command": "node",
      "args": ["/path/to/civil3d-mcp-server/dist/index.js"],
      "env": {
        "CIVIL3D_PLUGIN_HOST": "localhost",
        "CIVIL3D_PLUGIN_PORT": "8765"
      }
    }
  }
}
```

### Claude Code Configuration

```json
{
  "mcpServers": {
    "civil3d": {
      "command": "node",
      "args": ["dist/index.js"],
      "cwd": "/path/to/civil3d-mcp-server"
    }
  }
}
```

---

## How It Works

### Request Flow

```
1. Claude calls tool: c3d_list_alignments()
2. MCP server POSTs to http://localhost:8765/execute:
   { "id": "uuid", "command": "list_alignments", "params": {} }
3. Civil 3D HTTP listener (background thread) receives request
4. Request is placed in RequestQueue (ConcurrentQueue)
5. Civil 3D Application.Idle fires on the main thread
6. IdleHandler dequeues the request, locks the document
7. CommandRouter dispatches to AlignmentHandler.ListAlignments()
8. AlignmentHandler opens a transaction, queries the Civil 3D API
9. Returns { success: true, result: { alignments: [...] } }
10. HTTP listener sends JSON response back to MCP server
11. MCP server returns result to Claude
```

### Thread Safety

Civil 3D (like Blender) requires all database operations on the main thread. The plugin uses:

- **Background thread**: `HttpListener.GetContext()` → accepts HTTP connections
- **`ConcurrentQueue<PendingRequest>`**: safely passes work to the main thread
- **`Application.Idle` event**: fires on the main thread when Civil 3D is idle
- **`TaskCompletionSource`**: lets the background thread `await` the main thread's result
- **`DocumentLock`**: Civil 3D's mechanism for safe document access

---

## Available Tools (20 total)

### Drawing
- `c3d_get_drawing_info` — file path, units, limits, Civil 3D version
- `c3d_get_layer_list` — all layers with visibility, color, lock state
- `c3d_execute_code` — run C# code directly against the Civil 3D API

### Alignments
- `c3d_list_alignments` — all alignments with station range and length
- `c3d_get_alignment_info` — entity list (tangents, arcs, spirals), profiles, design speed
- `c3d_get_alignment_geometry` — XY coordinates at station/offset
- `c3d_create_alignment` — create new alignment from point array

### Profiles
- `c3d_list_profiles` — profiles per alignment with elevation range
- `c3d_get_profile_info` — full PVI list with stations and elevations

### Surfaces
- `c3d_list_surfaces` — all surfaces with elevation stats and area
- `c3d_get_surface_info` — detailed stats including cut/fill volumes
- `c3d_get_surface_elevation` — elevation at any XY coordinate
- `c3d_get_surface_slope` — slope % and aspect at any XY coordinate

### Pipe Networks
- `c3d_list_pipe_networks` — all networks with pipe/structure counts
- `c3d_list_pipes` — pipes with diameter, slope, invert elevations, cover
- `c3d_list_structures` — manholes/inlets with rim and sump elevations

### Corridors
- `c3d_list_corridors` — all corridors
- `c3d_get_corridor_info` — baselines, regions, referenced alignments/profiles

### Parcels
- `c3d_list_parcels` — area (sq ft + acres) and perimeter

### COGO Points
- `c3d_list_point_groups` — all point groups
- `c3d_get_points_in_group` — XYZ coordinates per point

---

## Example Conversations

> "What alignments are in this drawing and how long is each?"
> 
> → `c3d_list_alignments()` → returns names, stations, lengths

> "What's the elevation of the EG surface at X=5000, Y=3200?"
>
> → `c3d_get_surface_elevation(name="EG", x=5000, y=3200)`

> "Show me all the pipes in the storm drain network with their slopes"
>
> → `c3d_list_pipes(network="Storm Drain")`

> "What are the PVIs on the design profile for alignment Mainline?"
>
> → `c3d_get_profile_info(name="Mainline - FG", alignment="Mainline")`

> "Create an alignment called Test Road through these points: (0,0), (500,0), (1000,200)"
>
> → `c3d_create_alignment(name="Test Road", points=[[0,0],[500,0],[1000,200]])`

---

## Code Execution (Advanced)

`c3d_execute_code` runs C# directly in Civil 3D — like Blender's `execute_blender_code`. 

**Requires Roslyn scripting package** — add to `Civil3DMcpPlugin.csproj`:
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Scripting" Version="4.9.2" />
```

Then uncomment the Roslyn evaluator in `DrawingHandler.cs`. The code runs with access to:
- `Db` — `Database` (active drawing)
- `CivilDoc` — `CivilDocument`
- `tr` — open `Transaction`
- All AutoCAD and Civil 3D namespaces

Example:
```csharp
// Count alignments longer than 1000 ft
var count = CivilDoc.GetAlignmentIds()
    .Cast<ObjectId>()
    .Select(id => (Alignment)tr.GetObject(id, OpenMode.ForRead))
    .Count(al => al.Length > 1000);
return count;
```

---

## Troubleshooting

**"Cannot connect to Civil 3D plugin"**
- Civil 3D must be open with the plugin loaded
- Type `MCPSTATUS` in Civil 3D to confirm the server is running
- Check Windows Firewall isn't blocking `localhost:8765`

**"No active document"**
- Open a Civil 3D drawing (.dwg) before making requests

**"Alignment not found"**
- Names are case-insensitive but must match exactly
- Use `c3d_list_alignments` first to see exact names

**Plugin won't load**
- Verify the DLL path in NETLOAD
- Check the DLL targets the correct Civil 3D version
- Civil 3D must be run as the same user (no elevation mismatch)

---

## Civil 3D Version Compatibility

| Civil 3D Version | AutoCAD Version | acad.exe |
|---|---|---|
| 2025 | R26 | `R26.0` |
| 2024 | R25 | `R25.0` |
| 2023 | R24 | `R24.2` |

Update the `CivilInstallPath` in the project file for your version. The API is largely backward-compatible across these versions.
