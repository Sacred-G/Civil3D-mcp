# AI Copilot + MCP Standards-Aware Implementation Plan

## Goal

Implement a production-safe Civil 3D assistant that:

- plans before executing
- uses MCP as the authoritative execution/context layer
- uses `output_clean7` as standards guidance instead of prompt bloat
- asks for clarification when prerequisites are missing

## Current Architecture

### WPF Copilot

Location: `Civil3D-AI-CoPilot-master/`

Key files:

- `APP UI/AIChatPanel.xaml.cs`
- `Services/McpClient.cs`
- `Services/DrawingContextService.cs`
- `Services/CommandExecutorService.cs`
- `Core/AgentPromptManager.cs`

Responsibilities:

- chat UI
- provider selection
- session handling
- drawing context gathering
- command execution and progress display

### MCP Server

Location: `src/`

Key files:

- `src/index.ts`
- `src/httpBridge.ts`
- `src/tools/register.ts`
- `src/tools/civil3d_orchestrate.ts`
- `src/orchestration/IntentRouter.ts`
- `src/orchestration/WorkflowPlanner.ts`
- `src/orchestration/ProjectContextService.ts`
- `src/orchestration/SelectionResolver.ts`

Responsibilities:

- authoritative Civil 3D tool registration
- context lookup from plugin connection
- workflow planning
- intent routing
- deterministic execution for supported actions

## Phase 1: Standards Lookup Foundation

### Deliverables

- `civil3d_standards_lookup` MCP tool
- `FrameworkStandardsService` loader/search service
- tool catalog entry and registration
- HTTP bridge support for standards lookup

### Data Source

Primary source:

- `output_clean7/ace49987-8f85-403b-a9af-9aa393c4f006_prompt_rules.json`

Secondary source for future work:

- `output_clean7/ace49987-8f85-403b-a9af-9aa393c4f006_chunks.json`
- `output_clean7/ace49987-8f85-403b-a9af-9aa393c4f006_agent_knowledge.md`

### Search Strategy

Search by:

- query text
- topic aliases
- tags
- rule/title term overlap
- original extraction score

### Initial Topics

- `templates`
- `styles`
- `layers`
- `labels`
- `plotting`
- `textstyles`
- `proposed_existing`
- `pipe_networks`
- `profile_section`

## Phase 2: Plan-First Orchestration

### Deliverables

- register `civil3d_orchestrate`
- expose orchestrate over HTTP bridge
- WPF copilot calls orchestration before LLM generation
- clarification loop for missing inputs

### Desired Chat Flow

1. User submits message
2. Copilot asks MCP for orchestration plan
3. MCP returns:
   - intent
   - confidence
   - selected tool/action
   - inferred parameters
   - workflow plan
   - missing fields
   - clarification questions
4. Copilot behavior:
   - if missing fields: ask user for clarification and pause execution
   - if supported direct orchestrated action is executable: allow MCP-first execution path
   - otherwise pass orchestration result into the LLM prompt as structured context
5. Execute commands only after plan is sufficiently resolved

## Phase 3: Standards-Aware Guidance

### Deliverables

- standards guidance included in orchestration responses when relevant
- standards lookup callable from chat UI
- assistant recommendations for:
  - template vs drawing-level changes
  - style governance
  - label conventions
  - Proposed vs Existing textstyle defaults
  - layer naming guidance

### High-Value Rules to Surface First

- never edit styles directly in a production drawing
- template is the top level of the hierarchy
- Civil 3D should rely on feature/label styles more than manual layer management
- Proposed and Existing textstyle conventions
- style/layer changes should be made where referencing styles are present

## Required File Changes

### TypeScript MCP Server

- `src/tools/register.ts`
- `src/tools/tool_catalog.ts`
- `src/tools/civil3d_orchestrate.ts`
- `src/httpBridge.ts`
- `src/standards/FrameworkStandardsService.ts` (new)
- `src/tools/civil3d_standards_lookup.ts` (new)

### WPF Copilot

- `Civil3D-AI-CoPilot-master/APP UI/AIChatPanel.xaml.cs`
- `Civil3D-AI-CoPilot-master/Services/McpClient.cs`
- `Civil3D-AI-CoPilot-master/Models/AiModels.cs`

## Immediate Technical Risks

- current HTTP bridge only supports a narrow legacy tool whitelist
- `civil3d_orchestrate` exists but is not registered in `register.ts`
- `McpClient` availability state can diverge from the raw `/health` check in the UI
- direct `civil3d_*` execution through the HTTP bridge is still only partially represented

## Recommended Follow-Up After This Implementation

- add `civil3d_standards_recommend`
- add `civil3d_standards_validate`
- normalize top rules into structured JSON instead of relying only on extracted prose
- expand `civil3d_orchestrate` beyond the currently supported action set
- migrate more chat requests to deterministic MCP execution before LLM fallback
