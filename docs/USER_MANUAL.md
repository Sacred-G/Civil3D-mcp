# Civil 3D MCP User Manual and Best Practices

This manual explains how engineers, designers, technicians, and CAD managers can use Civil3D-MCP safely and effectively with Autodesk Civil 3D 2026.

Civil3D-MCP connects an AI client such as Claude Desktop, Claude Code, Cursor, or another MCP-compatible assistant to the active Civil 3D drawing. It can inspect drawing data, perform supported Civil 3D operations, run multi-step workflows, and search the locally installed Autodesk help—including useful images and matching tutorial videos.

> **Supported host:** Autodesk Civil 3D 2026. Always test drawing-changing workflows on a copy before using them on production files.

## Contents

1. [How the system works](#how-the-system-works)
2. [Installation](#installation)
3. [First connection](#first-connection)
4. [Your first working session](#your-first-working-session)
5. [How to ask for work](#how-to-ask-for-work)
6. [Approvals and protected actions](#approvals-and-protected-actions)
7. [Using Autodesk help, images, and videos](#using-autodesk-help-images-and-videos)
8. [Common Civil 3D workflows](#common-civil-3d-workflows)
9. [Best practices](#best-practices)
10. [Safety and engineering responsibility](#safety-and-engineering-responsibility)
11. [Troubleshooting](#troubleshooting)
12. [Administrator reference](#administrator-reference)
13. [Quick-reference checklist](#quick-reference-checklist)

## How the system works

Civil3D-MCP has three local components:

```text
AI client
   ↕ MCP over stdio
Civil3D-MCP Node.js server
   ↕ JSON-RPC over localhost:8080
Civil3DMcpPlugin.dll inside Civil 3D 2026
   ↕
Active Civil 3D drawing
```

The AI client decides which supported tool to call. The Node.js server validates the request, applies approval and security policy, and sends the operation to the native plugin. The plugin executes drawing work in Civil 3D's host-safe command context and returns a structured result.

The local Autodesk help search is different: it reads installed help files and does not require Civil 3D or a drawing to be open.

### What it can help with

The current release provides a compact default surface of 34 tools backed by 206 catalog entries. Major areas include:

- drawing inspection, settings, save, undo, and redo;
- points, COGO geometry, parcels, and survey inspection;
- surfaces, grading, feature lines, and volumes;
- alignments, profiles, assemblies, corridors, and sections;
- pipe and pressure networks;
- hydrology, catchments, detention, and SSA handoff;
- quantity, cost, QC, standards, and report workflows;
- project setup, data shortcuts, and plan-production operations;
- local Autodesk help with topic citations, images, and videos.

The generated [tool reference](./tools.generated.md) is the source of truth for currently exposed actions. If an operation is not listed, ask the assistant for the nearest supported workflow or for step-by-step Civil 3D UI guidance.

## Installation

You need both the Node.js MCP server and the native Civil 3D plugin.

### Option A: Claude Desktop extension

This is the easiest Node-side installation for most users.

1. Obtain the release file named `civil3d-mcp-<version>.mcpb`.
2. In Claude Desktop, open **Settings > Extensions > Advanced settings**.
3. Select **Install Extension** and choose the `.mcpb` file.
4. Keep the default plugin port `8080` and HTTP bridge port `3000` unless your administrator configured different values.
5. Install and load the native plugin separately as described below.

The extension includes the production Node.js dependencies. It does not install Autodesk Civil 3D or the native plugin.

### Option B: Claude Code from source

From PowerShell in the repository:

```powershell
npm install
npm run build
npm run claude:add
```

The project launcher automatically installs or rebuilds dependencies when needed. For a user-wide Claude Code registration instead of a project-only registration, use:

```powershell
npm run claude:add:user
```

### Build and load the native plugin

Prerequisites:

- Autodesk Civil 3D 2026;
- .NET 8 SDK;
- licensed Civil 3D 2026 managed assemblies from the local installation.

Example build:

```powershell
$refs = "C:\Program Files\Autodesk\AutoCAD 2026\C3D"
dotnet build .\Civil3D-MCP-Plugin\Civil3DMcpPlugin.csproj -c Release `
  /p:Civil3DReferencesPath="$refs"
```

The normal output is:

```text
Civil3D-MCP-Plugin\bin\Release\net8.0-windows\Civil3DMcpPlugin.dll
```

Load it in Civil 3D:

1. Start Civil 3D 2026.
2. Run `NETLOAD` at the command line.
3. Select `Civil3DMcpPlugin.dll`.
4. Run `C3DMCPSTATUS` and confirm that the server is running on port `8080`.
5. For regular use, add the DLL to the `APPLOAD` Startup Suite or follow [DEPLOYMENT.md](./DEPLOYMENT.md) for autoload configuration.

Native plugin commands:

| Command | Purpose |
|---|---|
| `C3DMCPSTART` | Start the local plugin server. It normally starts when the plugin loads. |
| `C3DMCPSTOP` | Stop the local plugin server. |
| `C3DMCPSTATUS` | Show server status, port, and host-queue depth. |

## First connection

Use this checklist before requesting drawing work:

1. Open Civil 3D 2026.
2. Open the intended `.dwg` file.
3. Confirm `C3DMCPSTATUS` reports a running server.
4. Start or reconnect the AI client.
5. Ask: **“Check Civil 3D health and tell me which drawing is active.”**
6. Confirm the returned drawing path and name are correct.
7. Ask for a read-only inventory before making changes.

A useful first prompt is:

> Check the Civil 3D connection, identify the active drawing, report its units and coordinate system, and list the alignments, profiles, surfaces, corridors, and pipe networks. Do not modify anything.

If the assistant cannot reach the plugin, see [Troubleshooting](#troubleshooting).

## Your first working session

The safest session pattern is:

```text
Inspect → Plan → Execute a small step → Verify → Save a checkpoint
```

### 1. Inspect

Start with read-only questions:

- “What surfaces are in this drawing?”
- “List the alignment station ranges and lengths.”
- “Check the corridor for missing targets or rebuild warnings.”
- “Summarize this pipe network without changing it.”

Inspection prevents duplicate objects and catches naming or prerequisite problems early.

### 2. Plan

For work spanning multiple Civil 3D object types, request a plan before execution:

> Plan the workflow for creating a proposed profile and corridor from alignment `CL-01`, surface `EG`, and assembly `Urban Typical`. Identify missing inputs and approval points. Do not make changes yet.

Ask the assistant to separate:

- prerequisites;
- read-only checks;
- drawing mutations;
- validation checkpoints;
- files that will be imported or exported.

### 3. Execute a small step

Prefer one bounded change at a time:

> Create the sampled existing-ground profile for `CL-01` from surface `EG`. Do not create the design profile or corridor yet.

This is easier to review and undo than an oversized instruction containing many unrelated changes.

### 4. Verify

After every mutation, ask for evidence:

- the exact object name and type created or changed;
- relevant counts, station ranges, elevations, or lengths;
- rebuild state and warnings;
- whether Civil 3D accepted the operation;
- a read-back of changed properties.

Example:

> Verify that the new profile belongs to `CL-01`, covers the full alignment station range, and has valid elevations. Report warnings; do not make further changes.

### 5. Save a checkpoint

Save deliberately after verification. For important work, save a new drawing copy instead of immediately overwriting the production drawing.

> Save a checkpoint copy under my approved Documents export folder as `ProjectA_30pct_AI-review.dwg`. Do not overwrite an existing file.

## How to ask for work

You do not need to know internal tool names. Plain language works best when it includes precise engineering context.

### A strong request contains

1. **Intent** — what outcome you want.
2. **Object identity** — exact names where known.
3. **Inputs** — geometry, surfaces, design values, files, or standards.
4. **Constraints** — units, station range, naming rules, tolerances, and no-change boundaries.
5. **Action level** — explain, inspect, plan, or execute.
6. **Verification** — what should be checked afterward.

Example:

> Inspect surface `FG-Phase1` between stations 10+00 and 25+00 along `CL-01`. Calculate cut/fill against `EG`, report the units and net volume, and flag any area where the result appears incomplete. Do not modify either surface.

### Prompt patterns

#### Read-only inspection

> Inspect `[object name]`. Report `[properties or checks]`. Do not modify the drawing.

#### Plan before execution

> Plan how to `[outcome]` using `[inputs]`. Identify prerequisites, missing information, protected actions, and validation steps. Do not execute yet.

#### Bounded execution

> Using `[exact inputs]`, perform `[one operation]`. Preserve `[objects/settings]`. Stop if `[condition]`. Then verify `[success criteria]`.

#### Troubleshooting

> Diagnose why `[object/workflow]` is failing. Inspect first, report the likely cause and evidence, and do not change anything until I approve a fix.

#### UI teaching

> Show me how to `[Civil 3D task]` manually. Give the exact ribbon or Toolspace path, prerequisites, validation checkpoint, and most common mistake.

#### Documentation help

> Search my installed Civil 3D 2026 help for `[topic]`. Summarize the relevant steps, cite the matching topic, and include useful images and the best matching tutorial video.

## Approvals and protected actions

Drawing deletion, destructive changes, file writes, imports, exports, and other non-retryable actions may require approval.

The normal protected flow is:

1. The assistant previews the exact action and parameters.
2. It requests a short-lived approval token when required.
3. It retries the unchanged request with that token.
4. The token is consumed after use.

Approval is bound to:

- the target tool and action;
- the exact parameters;
- the active drawing when the operation depends on one;
- a short expiration period.

If the drawing, output path, object name, or another parameter changes, request a new approval. This prevents a confirmation for one operation from being reused for another.

### Approval best practices

- Read the preview and verify object names, paths, and overwrite settings.
- Reject vague destructive requests; make the scope explicit.
- Do not disable approvals in a production environment.
- Treat `overwrite: true` as an intentional exception, not a default.
- Reconfirm the active drawing after switching document tabs.

## Using Autodesk help, images, and videos

The `civil3d_help` capability searches Autodesk Civil 3D Offline Help installed on the user's computer. It can work even when Civil 3D is closed.

### What it returns

- ranked Autodesk help topics;
- cleaned topic text with source citations;
- useful screenshots and diagrams;
- matching tutorial videos as player resources;
- direct MP4 links when the chat client cannot render the player.

Topic text and local images can be used offline. Autodesk-hosted video playback requires internet access.

### Natural-language examples

- “How do I use Grading Optimization? Use my local 2026 Autodesk help.”
- “Find the official steps for adding a widening to an offset alignment and include the diagrams.”
- “Show me Autodesk help for pressure network cover rules.”
- “Find videos about creating grading criteria.”
- “Explain this profile-view setting and cite the installed Autodesk topic.”

### Index behavior

The first help request builds a compressed search index under:

```text
%LOCALAPPDATA%\Civil3DMcp\help-index
```

Later searches reuse the cache. The index refreshes automatically when the detected Autodesk help files change. If help is stored in a custom location, set `CIVIL3D_HELP_ROOT` to the Offline Help `Help` folder before starting the MCP server.

Manual reindexing is disabled by default. Administrators can enable it with:

```powershell
$env:CIVIL3D_ENABLE_HELP_REINDEX = "true"
```

Then restart the MCP client-managed server and ask the assistant to rebuild the help index.

### Media best practices

- Ask for images only when they clarify a dialog, setting, or workflow.
- Ask for one or two highly relevant videos instead of a long result list.
- If video does not render inline, use the direct MP4 link returned by the assistant.
- Prefer help matching the installed Civil 3D version.
- Use Autodesk help for product procedure, but use project standards for company-specific decisions.

## Common Civil 3D workflows

### Drawing audit

> Audit the active drawing without modifying it. Report units, coordinate system, object counts, broken references, stale rebuild states, obvious standards issues, and the three highest-priority follow-ups.

Best practice: run this at the beginning of a session and before publishing.

### Surface and grading

> Inspect surfaces `EG` and `FG`. Confirm units and boundaries, calculate cut/fill volumes, and identify whether either surface appears stale. Do not rebuild or modify them.

For changes, split the work:

1. inspect feature lines and grading criteria;
2. create or modify the grading object;
3. rebuild and validate the grading surface;
4. calculate volumes;
5. review drainage and tie-in conditions.

Never treat an automated slope result as a geotechnical stability determination. Factor-of-safety analysis requires engineer-approved geometry, soil parameters, groundwater assumptions, and a valid analysis method.

### Alignment, profile, and corridor

> Plan a corridor workflow using alignment `CL-01`, design profile `FG-CL-01`, and assembly `Urban Typical`. Check that each prerequisite exists, inspect target requirements, and stop before creating or rebuilding anything.

Best practice:

1. validate the alignment and stationing;
2. validate existing-ground and design profiles;
3. inspect the assembly and subassemblies;
4. create or edit the corridor in a bounded station range;
5. map targets explicitly;
6. rebuild;
7. check warnings, surfaces, feature lines, and volumes.

### Pipe and pressure networks

> Inspect pipe network `Storm-01`. Report structures, pipe sizes, slopes, connectivity, cover, and any interference or hydraulic warnings. Do not resize anything.

Before automated sizing, provide:

- design flows or the hydrology basis;
- storm frequency and criteria;
- allowable velocity and cover;
- available parts list;
- downstream boundary assumptions;
- units.

Always verify that new pipes are connected to the intended start and end structures.

### Hydrology and detention

> Trace the flow path from coordinate `(5000, 3200)` on surface `EG`, delineate the contributing watershed, calculate its area, and estimate peak runoff using the Rational Method. Show all assumptions and do not create drawing objects.

Do not accept a runoff result without reviewing:

- drainage area and units;
- runoff coefficient or curve-number basis;
- rainfall intensity, duration, and recurrence interval;
- time-of-concentration method;
- outlet and downstream assumptions.

### Sections and quantities

> Inspect the sample-line group for `CL-01`, list its available section sources, and export section point data to an approved CSV path. Do not overwrite an existing file.

Check the generated file for:

- station and offset conventions;
- elevation units;
- correct source surface or corridor;
- complete sample-line coverage;
- explicit warnings about unavailable material quantities.

### QC and standards

> Run read-only QC on alignment `CL-01`, profile `FG-CL-01`, corridor `Road-01`, and surface `FG`. Group findings by severity and cite the object and evidence for every finding. Do not auto-fix.

Review findings before requesting remediation. Apply fixes in small groups and rerun the same checks afterward.

### Plan production and exports

> Inspect the existing sheet set and plan-production objects. Tell me what is ready to publish, what is missing, and which steps require the Civil 3D UI. Do not publish yet.

Use approved export roots, explicit filenames, and `overwrite: false` for the first run. Some Civil 3D operations have no supported managed API and may require manual UI steps; the assistant should say so instead of claiming success.

## Best practices

### Use a copy for AI-assisted changes

- Work on a versioned copy or a controlled branch of the drawing.
- Keep recoverable backups.
- Use clear checkpoint filenames such as `Project_30pct_before-grading.dwg`.
- Do not use experimental automation directly on the only production copy.

### Inspect before creating

Ask for current objects and names before creating new ones. Civil 3D drawings often contain similarly named surfaces, profiles, sites, and networks.

### Provide exact object names

Names are case-insensitive but must otherwise match. Copy names from Prospector or ask the assistant to list candidates before selecting one.

### State units and coordinate system

Do not assume feet versus meters, square feet versus acres, or cubic yards versus cubic meters. Ask the assistant to report the drawing units with every critical calculation.

### Separate engineering assumptions from CAD execution

Provide approved criteria explicitly. The assistant can apply inputs and calculate results, but it should not invent design standards, geotechnical parameters, rainfall data, or regulatory requirements.

### Prefer small, verifiable changes

One successful change with a clear read-back is better than a large opaque workflow. Use station limits and object names to narrow the scope.

### Use read-only diagnosis first

When something is wrong, ask for the cause and evidence before requesting a fix. This avoids compounding a drawing problem.

### Require a verification step

Ask the assistant to read the result back from Civil 3D. A message saying what it intended to do is not proof that Civil 3D accepted the change.

### Keep the compact tool surface

The default 34-tool surface is recommended. It reduces overlapping tool descriptions while preserving supported operations through domain actions. Enable specialized aliases only when a client or integration genuinely needs them.

### Use approved file roots

By default, import and export paths must stay under the user's Documents folder. Administrators can configure separate import and export roots. Avoid broad roots such as an entire system drive.

### Protect the local bridge

Keep the HTTP bridge bound to `127.0.0.1`. If it must bind elsewhere, use a strong `MCP_HTTP_TOKEN`, restrict allowed hosts and origins, and apply normal network controls.

### Record important decisions

For production work, retain:

- the prompt or task description;
- input assumptions and standards;
- approval previews;
- generated reports;
- validation results;
- the drawing version before and after the operation.

## Safety and engineering responsibility

Civil3D-MCP is an engineering productivity tool, not an independent engineer of record.

- A qualified professional remains responsible for design decisions and deliverables.
- Review geometry, units, criteria, calculations, and drawing changes.
- Do not use generated values as regulatory, hydraulic, hydrologic, structural, traffic, or geotechnical approval without appropriate professional validation.
- Follow company CAD standards, QA/QC procedures, contracts, and jurisdictional requirements.
- If the assistant reports that an action is unavailable, use the supported Civil 3D UI workflow or a reviewed integration. Do not ask it to fabricate completion.

## Troubleshooting

### Cannot connect to the Civil 3D plugin

1. Confirm Civil 3D 2026 is open.
2. Open a drawing.
3. Run `C3DMCPSTATUS`.
4. If stopped, run `C3DMCPSTART`.
5. Confirm the Node side uses the same port, normally `8080`.
6. Restart the MCP connection in the AI client.
7. Check local firewall or endpoint-security rules for loopback traffic.

### No active document

Open the intended `.dwg`, make its tab active, and retry. If you switched drawings after approving an action, request a new approval.

### Object not found

Ask the assistant to list objects of that type. Then retry using the exact returned name.

### The client does not show a specialized tool name

This is expected with the compact default surface. Ask in plain language or use the canonical domain tool and its action. See [tools.generated.md](./tools.generated.md) for the current mapping.

### Help search finds no local documentation

1. Confirm Autodesk Civil 3D Offline Help is installed.
2. Ask for help-index status.
3. If help is stored elsewhere, set `CIVIL3D_HELP_ROOT` to its `Help` directory.
4. Restart the MCP server.
5. Enable and run reindex only if automatic refresh does not detect the files.

### Images appear but video does not play

Local images do not require internet access. Autodesk-hosted videos do. Try the returned direct MP4 link, confirm internet access, and check whether the AI client supports embedded media resources.

### Import or export path rejected

Use a path under an allowed root, normally the user's Documents folder. Confirm the extension matches the operation and set `overwrite: true` only when replacement is intentional.

### Approval token rejected

Request a new approval. Tokens are short-lived, single-use, and bound to exact parameters and the active drawing.

### Long operation times out

First confirm Civil 3D is still responsive and inspect job status. For legitimately heavy work, an administrator may increase `CIVIL3D_COMMAND_TIMEOUT`. Do not raise it merely to hide a deadlock or invalid drawing state.

### Plugin log

The native plugin log is normally located at:

```text
%LOCALAPPDATA%\Civil3DMcpPlugin\plugin.log
```

The log rotates at 5 MiB and keeps three backups.

## Administrator reference

### Common environment variables

| Variable | Default | Recommendation |
|---|---|---|
| `CIVIL3D_HOST` | `localhost` | Keep local unless a reviewed architecture requires otherwise. |
| `CIVIL3D_PORT` | `8080` | Must match the native plugin port. |
| `CIVIL3D_COMMAND_TIMEOUT` | `120000` ms | Increase only for verified long-running operations. |
| `CIVIL3D_LOG_LEVEL` | `info` | Use `debug` temporarily while diagnosing. |
| `CIVIL3D_ENABLE_TOOL_ALIASES` | `false` | Keep the compact surface for normal users. |
| `CIVIL3D_APPROVAL_MODE` | enforced | Do not disable in production. |
| `CIVIL3D_HELP_ROOT` | auto-discovered | Set only when Offline Help is in a custom folder. |
| `CIVIL3D_HELP_VERSION` | `2026` | Match the installed product version. |
| `CIVIL3D_FILE_ROOTS` | user Documents | Prefer separate narrow import/export roots. |
| `CIVIL3D_IMPORT_ROOTS` | shared fallback | Restrict to reviewed source-data folders. |
| `CIVIL3D_EXPORT_ROOTS` | shared fallback | Restrict to approved deliverable/report folders. |
| `MCP_HTTP_HOST` | `127.0.0.1` | Keep on loopback. |
| `MCP_HTTP_PORT` | `3000` | Change if another local service owns the port. |
| `MCP_HTTP_TOKEN` | unset | Required for a non-loopback bind; recommended for shared machines. |

Environment changes must be applied to the process that launches the MCP server or Civil 3D, as appropriate. Restart the affected process after changing them.

### Verification commands

From the repository:

```powershell
npm run build
npm test
npm run docs:check
npm run package:inspect
```

With Civil 3D open, the plugin loaded, and a drawing active:

```powershell
npm run test:live-plugin
```

The destructive live-host suite is for disposable drawings only:

```powershell
$env:CIVIL3D_LIVE_SMOKE_CONFIRM = "DISPOSABLE_DRAWING"
$env:CIVIL3D_LIVE_SMOKE_QC_PATH = "$env:USERPROFILE\Documents\civil3d-mcp-live-qc.txt"
npm run test:live-host
```

## Quick-reference checklist

### Start of session

- [ ] Civil 3D 2026 is open.
- [ ] The correct drawing tab is active.
- [ ] `C3DMCPSTATUS` reports the server running.
- [ ] The assistant confirms health and active drawing identity.
- [ ] Units and coordinate system are known.
- [ ] A recoverable drawing copy or checkpoint exists.

### Before a change

- [ ] Exact object names are confirmed.
- [ ] Inputs, units, criteria, and station limits are explicit.
- [ ] The assistant inspected prerequisites.
- [ ] The requested change is small and bounded.
- [ ] Approval preview, path, and overwrite behavior are correct.

### After a change

- [ ] The result was read back from Civil 3D.
- [ ] Rebuild state and warnings were checked.
- [ ] Counts, geometry, units, and calculations were reviewed.
- [ ] A qualified person reviewed engineering implications.
- [ ] The drawing was saved at an intentional checkpoint.

## Additional documentation

- [README](../README.md) — project overview and quick start.
- [Deployment guide](./DEPLOYMENT.md) — detailed installation, autoload, Docker, and environment configuration.
- [Generated tool reference](./tools.generated.md) — current canonical actions and aliases.
- [Extended tool guide](./tools.md) — parameter and workflow details.
