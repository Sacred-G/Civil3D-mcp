# Deployment Guide

This guide covers every method for deploying the civil3d-mcp server: local development, npm package publishing, Docker container, and Civil 3D plugin installation.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Installation (clone → running)](#local-installation)
3. [npm Package Publishing](#npm-package-publishing)
4. [Civil 3D Plugin Installation](#civil-3d-plugin-installation)
5. [Environment Variables](#environment-variables)
6. [Connecting to Claude Desktop](#connecting-to-claude-desktop)
7. [Docker Deployment](#docker-deployment)
8. [Verifying the Connection](#verifying-the-connection)

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Node.js | 18 or later | Required to run the MCP server |
| npm | 8 or later | Comes with Node.js |
| Autodesk Civil 3D | 2023–2026 | Required for the C# plugin |
| .NET 8 SDK | 8.0 | Required to build the C# plugin |
| Docker (optional) | 20+ | Only needed for container deployment |

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

### 4. Run standalone (without Claude Desktop)

```bash
node build/index.js
```

The process reads from `stdin` and writes to `stdout` (MCP stdio transport). It also starts an HTTP bridge on `127.0.0.1:3000` for the Civil 3D plugin to call.

---

## npm Package Publishing

The package is configured for npm publishing as-is. The `files` field in `package.json` ensures only the compiled output ships.

### Current configuration

```json
{
  "name": "civil3d-mcp",
  "version": "1.0.0",
  "main": "index.js",
  "bin": {
    "civil3d-mcp": "./build/index.js"
  },
  "files": ["build"],
  "publishConfig": {
    "access": "public"
  }
}
```

> **Note:** `publishConfig.access` is not currently in `package.json`. Add it before your first publish if publishing to a public npm registry.

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

```bash
# Requires .NET 8 SDK and Civil 3D reference DLLs in C_References/
cd Civil3D-MCP-Plugin
dotnet build -c Release
```

The output DLL is in `Civil3D-MCP-Plugin/bin/Release/net8.0-windows/`.

### Load into Civil 3D

1. Open Autodesk Civil 3D.
2. At the command prompt, type `NETLOAD` and press Enter.
3. Browse to `Civil3D-MCP-Plugin/bin/Release/net8.0-windows/Civil3DMcpPlugin.dll`.
4. Click **Open**. The plugin registers its RPC server and starts listening.

> **Auto-load on startup:** Add the plugin to the `APPLOAD` startup suite (Tools → Load Application → Startup Suite) so it loads automatically when Civil 3D opens.

### Civil 3D reference DLLs

The project references Civil 3D API assemblies from `C_References/`. These are not redistributed and must be copied from a local Civil 3D installation:

```
C_References/
├── accoremgd.dll
├── AcDbMgd.dll
├── Acmgd.dll
├── AecBaseMgd.dll
├── AeccDbMgd.dll
└── (others as needed)
```

Copy from: `C:\Program Files\Autodesk\AutoCAD 202x\`

---

## Environment Variables

All variables are optional; defaults work for a standard local setup.

| Variable | Default | Description |
|---|---|---|
| `CIVIL3D_HOST` | `localhost` | Host where Civil 3D plugin RPC server is listening |
| `CIVIL3D_PORT` | `8080` | TCP port for the Civil 3D plugin RPC server |
| `CIVIL3D_CONNECT_TIMEOUT` | `5000` | Connection timeout in milliseconds |
| `CIVIL3D_COMMAND_TIMEOUT` | `120000` | Timeout for individual command execution (ms) |
| `MCP_HTTP_PORT` | `3000` | HTTP bridge port (used by the AI CoPilot plugin) |
| `MCP_HTTP_HOST` | `127.0.0.1` | HTTP bridge bind address |
| `CIVIL3D_LOG_LEVEL` | `info` | Log verbosity: `debug`, `info`, `warn`, `error` |

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
  "status": "connected",
  "plugin": "Civil3DMcpPlugin",
  "civil3dVersion": "2025"
}
```

Expected response when Civil 3D is not running:
```json
{
  "status": "disconnected",
  "error": "Connection refused"
}
```

### HTTP bridge test

The MCP server also exposes a REST endpoint for the AI CoPilot plugin. Test it directly:

```bash
curl -X POST http://127.0.0.1:3000/execute \
  -H "Content-Type: application/json" \
  -d '{"tool": "civil3d_health", "parameters": {}}'
```

### Log output

Set `CIVIL3D_LOG_LEVEL=debug` to see connection and command traffic in the MCP server process output.
