# Deployment Guide

This guide covers every method for deploying the civil3d-mcp server: Claude
Desktop extension packaging, local development, npm package publishing, Docker,
and Civil 3D plugin installation.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Claude Desktop Extension (MCPB / DXT)](#claude-desktop-extension-mcpb--dxt)
3. [Local Installation (clone → running)](#local-installation)
4. [npm Package Publishing](#npm-package-publishing)
5. [Civil 3D Plugin Installation](#civil-3d-plugin-installation)
6. [Environment Variables](#environment-variables)
7. [Connecting to Claude Desktop](#connecting-to-claude-desktop)
8. [Docker Deployment](#docker-deployment)
9. [Verifying the Connection](#verifying-the-connection)

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Node.js | 18 or later | Required to run the MCP server |
| npm | 8 or later | Comes with Node.js |
| Autodesk Civil 3D | 2026 | Supported and live-validated plugin host |
| .NET 10 SDK | 10.0 | Required to build the C# plugin |
| Docker (optional) | 20+ | Only needed for container deployment |

---

## Claude Desktop Extension (MCPB / DXT)

Anthropic renamed the former Desktop Extension (`.dxt`) format to MCP Bundle
(`.mcpb`). Current Claude Desktop releases should install the `.mcpb`; the build
also creates an identical `.dxt` file for older releases.

### Build the installers

```powershell
npm install
npm run package:claude
```

The command compiles the TypeScript server, stages production-only Node.js
dependencies, validates the manifest with the official MCPB CLI, and writes:

```text
dist/claude-desktop/
├── civil3d-mcp-<version>.mcpb
├── civil3d-mcp-<version>.dxt
└── SHA256SUMS.txt
```

Revalidate existing artifacts without rebuilding them:

```powershell
npm run package:claude:validate
```

### Install in Claude Desktop

1. Open **Settings > Extensions**.
2. Open **Advanced settings** and select **Install Extension**.
3. Select `civil3d-mcp-<version>.mcpb`.
4. For a standard local setup, keep Civil 3D at `localhost:8080` and the HTTP
   bridge at `127.0.0.1:3000`.
5. Open Civil 3D with `Civil3DMcpPlugin.dll` loaded, then ask Claude to run
   `civil3d_health`.

The extension is self-contained for the Node.js MCP server. Claude Desktop
cannot install or load an Autodesk .NET plugin, so the Civil 3D plugin remains a
separate prerequisite. Do not run a second standalone copy of the Node server
on the same HTTP port while the extension is enabled.

---

## Local Installation

### 1. Clone and install

```bash
git clone https://github.com/Sacred-G/Civil3D-mcp.git
cd civil3d-mcp
npm install
```

### 2. Build

```bash
npm run build
```

This compiles TypeScript to `build/` using the config in `tsconfig.json`.
The entry point is `build/index.js`.

### 3. Configure Claude Desktop

Edit `claude_desktop_config.json`:

- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "node",
      "args": ["C:/path/to/civil3d-mcp/build/index.js"]
    }
  }
}
```

Restart Claude Desktop. A hammer icon in the toolbar confirms the MCP server connected successfully.

### Claude Code one-line project setup

From the repo root on Windows, you can register the MCP server into Claude Code and create the project `.mcp.json` in one command:

```powershell
claude mcp add --scope project --transport stdio civil3d-mcp -- powershell -NoProfile -ExecutionPolicy Bypass -File "$PWD\scripts\claude-bootstrap-and-run.ps1"
```

The launcher script installs npm dependencies and builds the Node server automatically the first time Claude starts it.

If you want the repo to perform the initial install/build and then register Claude Code for you, use:

```powershell
npm run claude:add
```

### 4. Run standalone (without Claude Desktop)

```bash
node build/index.js
```

The process reads from `stdin` and writes to `stdout` (MCP stdio transport). It
also starts an HTTP bridge on `127.0.0.1:3000` for local HTTP clients. Node.js
connects separately to the Civil 3D plugin's JSON-RPC TCP listener on port 8080.

---

## npm Package Publishing

The package is configured for npm publishing as-is. The `files` field in `package.json` ensures only the compiled output ships.

### Current configuration

```json
{
  "name": "civil3d-mcp",
  "version": "1.2.1",
  "main": "build/index.js",
  "bin": {
    "civil3d-mcp": "./build/index.js"
  },
  "files": ["build"]
}
```

### Steps to publish

```bash
# Bump version
npm version patch   # or minor / major

# Build
npm run build

# Dry-run to verify what gets published
npm pack --dry-run

# Publish
npm publish
```

After publishing, users can run the server with `npx`:

```bash
# In claude_desktop_config.json:
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "npx",
      "args": ["-y", "civil3d-mcp"]
    }
  }
}
```

---

## Civil 3D Plugin Installation

The C# plugin (`Civil3D-MCP-Plugin/`) runs inside Civil 3D and acts as the bridge between the MCP server and the Civil 3D API.

### Build the plugin

```powershell
# Requires .NET 10 SDK and licensed Civil 3D 2026 managed references
cd Civil3D-MCP-Plugin
dotnet build -c Release /p:Civil3DReferencesPath="C:\Program Files\Autodesk\AutoCAD 2026\C3D"
```

The output DLL is in `Civil3D-MCP-Plugin/bin/Release/net10.0-windows/`.

### Load into Civil 3D

1. Open Autodesk Civil 3D.
2. At the command prompt, type `NETLOAD` and press Enter.
3. Browse to `Civil3D-MCP-Plugin/bin/Release/net10.0-windows/Civil3DMcpPlugin.dll`.
4. Click **Open**. The plugin registers its RPC server and starts listening.

> **Auto-load on startup:** Add the plugin to the `APPLOAD` startup suite (Tools → Load Application → Startup Suite) so it loads automatically when Civil 3D opens.

### Civil 3D reference DLLs

Autodesk assemblies are not tracked, published, or copied to the plugin output.
Pass a licensed local reference directory with `Civil3DReferencesPath`. An
untracked `C_References/` directory remains the local fallback and must include:

```
C_References/
├── accoremgd.dll
├── AcDbMgd.dll
├── Acmgd.dll
├── AecBaseMgd.dll
├── AeccDbMgd.dll
└── AeccPressurePipesMgd.dll
```

Use assemblies from the matching Civil 3D 2026 installation or licensed SDK.

---

## Environment Variables

All variables are optional; defaults work for a standard local setup.

| Variable | Default | Description |
|---|---|---|
| `CIVIL3D_HOST` | `localhost` | Host where Civil 3D plugin RPC server is listening |
| `CIVIL3D_PORT` | `8080` | TCP port for the Civil 3D plugin RPC server |
| `CIVIL3D_CONNECT_TIMEOUT` | `5000` | Connection timeout in milliseconds |
| `CIVIL3D_COMMAND_TIMEOUT` | `120000` | Timeout for individual command execution (ms) |
| `CIVIL3D_MAX_RESPONSE_BYTES` | `8388608` | Maximum plugin response buffered by Node.js |
| `CIVIL3D_ENABLE_TOOL_ALIASES` | `false` | Expose specialized drawing aliases instead of the compact 34-tool surface |
| `CIVIL3D_APPROVAL_MODE` | enforced | Set to `disabled` only for isolated disposable testing |
| `CIVIL3D_HELP_ROOT` | Auto-discovered | Optional path to an Autodesk Civil 3D Offline Help `Help` folder |
| `CIVIL3D_HELP_VERSION` | `2026` | Preferred installed offline-help version |
| `CIVIL3D_HELP_CACHE_ROOT` | Local AppData | Generated offline-help index directory |
| `CIVIL3D_ENABLE_HELP_REINDEX` | `false` | Allow the `civil3d_help` explicit `reindex` action |
| `CIVIL3D_HELP_MAX_IMAGE_BYTES` | `6291456` | Maximum inline help-image bytes per topic response |
| `CIVIL3D_VIDEO_CATALOG` | Bundled 2026 catalog | Optional refreshed Autodesk video-catalog JSON path |
| `MCP_HTTP_PORT` | `3000` | HTTP bridge port for local HTTP clients |
| `MCP_HTTP_HOST` | `127.0.0.1` | HTTP bridge bind address |
| `MCP_HTTP_TOKEN` | *(unset)* | Shared secret required for non-loopback binds |
| `MCP_HTTP_MAX_BODY_BYTES` | `1048576` | Maximum accepted `/execute` request body |
| `MCP_HTTP_ALLOWED_HOSTS` | Loopback hostnames | Comma-separated accepted HTTP Host names; required for non-loopback binds |
| `MCP_HTTP_ALLOWED_ORIGINS` | *(unset)* | Comma-separated browser origins; wildcards are rejected |
| `CIVIL3D_FILE_ROOTS` | User Documents | Semicolon-separated fallback roots for plugin imports and exports |
| `CIVIL3D_IMPORT_ROOTS` | `CIVIL3D_FILE_ROOTS` | Optional import-only roots |
| `CIVIL3D_EXPORT_ROOTS` | `CIVIL3D_FILE_ROOTS` | Optional export-only roots |
| `CIVIL3D_LOG_LEVEL` | `info` | Log verbosity: `debug`, `info`, `warn`, `error` |
| `CIVIL3D_MCP_LOG_DIR` | Local AppData | Native plugin rotating-log directory; set before Civil 3D starts |
| `CIVIL3D_MCP_LOG_LEVEL` | `info` | Native plugin log threshold; set before Civil 3D starts |

`CIVIL3D_MCP_LOG_DIR`, `CIVIL3D_MCP_LOG_LEVEL`, and plugin filesystem-root
variables must be set in the environment that launches Civil 3D. The remaining
connection, MCP, and HTTP variables belong to the Node.js server environment.

### Setting env vars for Claude Desktop

```json
{
  "mcpServers": {
    "civil3d-mcp": {
      "command": "node",
      "args": ["C:/path/to/civil3d-mcp/build/index.js"],
      "env": {
        "CIVIL3D_HOST": "localhost",
        "CIVIL3D_PORT": "8080",
        "CIVIL3D_ENABLE_TOOL_ALIASES": "true",
        "CIVIL3D_LOG_LEVEL": "debug"
      }
    }
  }
}
```

---

## Connecting to Claude Desktop

End-to-end flow once everything is running:

1. **Civil 3D** opens with the plugin loaded — plugin starts RPC server on port `8080`.
2. **MCP server** (`node build/index.js`) starts — connects to Civil 3D on `localhost:8080`, starts HTTP bridge on `127.0.0.1:3000`.
3. **Claude Desktop** connects to the MCP server via stdio.
4. You ask Claude to do something in Civil 3D — Claude calls the MCP tool — MCP server forwards to Civil 3D plugin — result returns to Claude.

### Checklist

- [ ] Civil 3D is open with the plugin loaded (`NETLOAD` or auto-load)
- [ ] `node build/index.js` process is running (or Claude Desktop launched it)
- [ ] Hammer icon visible in Claude Desktop toolbar
- [ ] Test: ask Claude "run civil3d_health" — should return plugin status

---

## Docker Deployment

See [`Dockerfile`](../Dockerfile) and [`docker-compose.yml`](../docker-compose.yml) at the repo root.

### Production build

```bash
docker build -t civil3d-mcp .
docker run --rm -it \
  -e CIVIL3D_HOST=host.docker.internal \
  -e CIVIL3D_PORT=8080 \
  civil3d-mcp
```

> **Note:** When running in Docker, Civil 3D must be on the host machine. Use `host.docker.internal` (Docker Desktop) to reach it from inside the container. The MCP server still communicates via stdio, so use Docker's `-i` flag or pipe stdio from your MCP client.

### Local development with hot-reload

```bash
docker compose up
```

This mounts `src/` into the container and runs `tsc --watch` so changes recompile automatically.

---

## Verifying the Connection

### Quick smoke test

With the MCP server running (via Claude Desktop or standalone), call the health check tool:

```
civil3d_health
```

Expected response when Civil 3D is connected:
```json
{
  "connected": true,
  "pluginVersion": "1.2.1.0",
  "drawingLoaded": true,
  "queueDepth": 0,
  "queueCapacity": 64,
  "fileLoggingHealthy": true
}
```

The MCP call returns a structured connection error when Civil 3D or the plugin
is unavailable. For automation, `GET http://127.0.0.1:3000/health/ready`
returns HTTP 503 until the bridge, plugin, and host queue are ready.

### HTTP bridge test

The MCP server also exposes a loopback REST endpoint for local integrations. Test it directly:

```bash
curl -X POST http://127.0.0.1:3000/execute \
  -H "Content-Type: application/json" \
  -d '{"tool": "civil3d_health", "parameters": {}}'
```

### Log output

Set `CIVIL3D_LOG_LEVEL=debug` to see connection and command traffic in the MCP server process output.
