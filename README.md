# Civil3D MCP Server

An AI-powered Model Context Protocol (MCP) server for Autodesk Civil 3D 2026, enabling natural language interaction with Civil 3D projects through any MCP-compatible client.

Built by [Steven Bouldin](https://linkedin.com/in/steven-bouldin)

---

## Overview

Civil3D MCP bridges the gap between AI assistants and Autodesk Civil 3D by exposing 55+ command tools via the [Model Context Protocol](https://modelcontextprotocol.io). Engineers and designers can query project data, create and modify civil engineering elements, and automate repetitive tasks—all through conversational AI.

### Architecture

The system uses a **two-layer architecture**:

```
┌─────────────────────────────────────────────────────────┐
│                    MCP Client                           │
│          (Claude Desktop, Cline, etc.)                  │
└──────────────┬──────────────────────────────────────────┘
               │
┌──────────────▼──────────────────────────────────────────┐
│              civil3d-mcp (Node.js)                      │
│         MCP Server — 55+ Tool Definitions               │
│         Exposes Civil 3D operations as AI tools          │
└──────────────┬──────────────────────────────────────────┘
               │ Socket
┌──────────────▼──────────────────────────────────────────┐
│         Civil 3D + MCP Plugin (C#)                      │
│    ┌──────────────────────────────────────────┐         │
│    │  civil3d-mcp-plugin                      │         │
│    │  SocketService ↔ CommandSet ↔ Execute     │         │
│    │  CommandManager ← Command Projects       │         │
│    └──────────────────────────────────────────┘         │
└─────────────────────────────────────────────────────────┘
```

**Layer 1 — MCP Server (this repo):** A Node.js/TypeScript server that implements the MCP protocol, defining tools that map to Civil 3D operations. Communicates with the Civil 3D plugin over a local socket connection.

**Layer 2 — Civil 3D Plugin:** A C# plugin loaded inside Civil 3D that receives commands from the MCP server, executes them against the Civil 3D API, and returns results.

---

## Features

- **Query project data** — Retrieve drawing info, surface data, alignments, profiles, corridors, parcels, pipe networks, and more
- **Create & modify elements** — Points, alignments, profiles, surfaces, feature lines, pipe networks, polylines, text, and labels
- **Surface editing** — Add points, breaklines, boundaries; extract contours; compute volumes
- **Style & label management** — List, inspect, and apply Civil 3D styles and labels
- **Coordinate systems** — Query coordinate system info and perform transformations
- **Data shortcuts** — List, sync, and create data shortcut references
- **Drawing management** — Save, undo/redo, create from template
- **Async job tracking** — Monitor long-running operations with status checks and cancellation

---

## Supported Tools

### Core

| Tool | Description |
|------|-------------|
| `civil3d_health` | Connection and plugin status check |
| `civil3d_drawing` | Drawing management (info, settings, save, undo/redo, new from template) |
| `civil3d_job` | Async operation status and cancellation |
| `get_drawing_info` | Active drawing information |
| `list_civil_object_types` | Available Civil 3D object types in the current drawing |
| `get_selected_civil_objects_info` | Properties of currently selected objects |

### Points & Geometry

| Tool | Description |
|------|-------------|
| `civil3d_point` | COGO point and point group CRUD operations |
| `create_cogo_point` | Create a single COGO point |
| `create_line_segment` | Create a line segment |
| `acad_create_polyline` | Create a 2D polyline |
| `acad_create_3dpolyline` | Create a 3D polyline |

### Horizontal & Vertical Design

| Tool | Description |
|------|-------------|
| `civil3d_alignment` | Horizontal alignment read, create, delete, and station conversion |
| `civil3d_profile` | Vertical profile read, create, and delete |
| `civil3d_corridor` | Corridor data, rebuild, and volume operations |
| `civil3d_section` | Section data and sample line creation |
| `civil3d_assembly` | Assembly and subassembly inspection |

### Surfaces

| Tool | Description |
|------|-------------|
| `civil3d_surface` | Surface data read, create, and delete |
| `civil3d_surface_edit` | Add points/breaklines/boundaries, extract contours, compute volumes |

### Site & Utilities

| Tool | Description |
|------|-------------|
| `civil3d_parcel` | Parcel and site data |
| `civil3d_pipe_network` | Pipe network data, interference checks |
| `civil3d_pipe_network_edit` | Create and modify pipe networks, pipes, and structures |
| `civil3d_feature_line` | Feature line data and 3D polyline export |

### Annotation & Coordinate Systems

| Tool | Description |
|------|-------------|
| `civil3d_label` | Label management on Civil 3D objects |
| `civil3d_style` | Style listing and inspection |
| `civil3d_coordinate_system` | Coordinate system info and transformations |
| `civil3d_data_shortcut` | Data shortcut management |
| `acad_create_text` | Create DBText |
| `acad_create_mtext` | Create MText |

---

## Requirements

- **Node.js** 18+
- **Autodesk Civil 3D 2026** with the MCP plugin installed
- An MCP-compatible client (Claude Desktop, Cline, VS Code + Continue, etc.)

---

## Installation

### 1. Clone & Build

```bash
git clone https://github.com/Sacred-G/Civil3D-mcp.git
cd Civil3D-mcp
npm install
npm run build
```

### 2. Install the Civil 3D Plugin

Load the C# plugin into Civil 3D. See the [`Civil3D-MCP-Plugin`](./Civil3D-MCP-Plugin) directory for build instructions and the plugin DLL.

### 3. Configure Your MCP Client

**Claude Desktop**

Open `Settings → Developer → Edit Config → claude_desktop_config.json` and add:

```json
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "node",
      "args": ["C:\\path\\to\\Civil3D-mcp\\build\\index.js"]
    }
  }
}
```

Restart Claude Desktop. The hammer icon confirms a successful connection.

**Other MCP Clients**

Point your client's MCP server configuration to the built `index.js` using the `node` command, following the same pattern above.

---

## Project Structure

```
Civil3D-mcp/
├── src/                        # TypeScript MCP server source
├── Civil3D-MCP-Plugin/         # C# plugin for Civil 3D
├── Civil3D-AI-CoPilot-master/  # AI CoPilot integration
├── templates/                  # Drawing templates
├── assets/                     # Screenshots and media
├── tools.md                    # Detailed tool documentation
├── package.json
└── tsconfig.json
```

---

## Usage Examples

Once connected, you can interact with Civil 3D through natural language:

> "Show me all the surfaces in the current drawing"

> "Create a COGO point at coordinates 1000, 2000, elevation 350"

> "What alignments exist in this project? Give me the station range for the main alignment."

> "Add breaklines to the existing ground surface from the selected feature lines"

> "Check for pipe network interference between the storm and sanitary systems"

---

## Tech Stack

- **TypeScript / Node.js** — MCP server implementation
- **C# / .NET** — Civil 3D plugin (AutoCAD/Civil 3D API)
- **Model Context Protocol** — AI tool interface standard

---

## License

[MIT](./LICENSE)

---

## Author

**Steven Bouldin**
- LinkedIn: [linkedin.com/in/steven-bouldin](https://linkedin.com/in/steven-bouldin)
- GitHub: [github.com/Sacred-G](https://github.com/Sacred-G)
- Web: [stevenbouldin.com](https://stevenbouldin.com)
