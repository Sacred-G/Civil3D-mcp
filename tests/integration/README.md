# Integration Tests — Civil3D MCP Workflow Fixtures

This directory contains end-to-end workflow integration tests for the Civil3D MCP ecosystem. These tests **do not require a live Civil3D or AutoCAD connection**. Instead, they validate that the AI prompt manager, command parser, and tool router would produce the correct multi-step command sequences for realistic user prompts.

---

## What These Tests Validate

Each test scenario is defined as a JSON fixture in `fixtures/`. A fixture describes:

- **`prompt`** — a realistic user request (what a Civil3D engineer would type)
- **`expectedCommandSequence`** — the ordered list of commands the AI should emit, including action names and parameters
- **`expectedToolCalls`** — which MCP tools (e.g., `civil3d_alignment`, `civil3d_qc`) must be invoked
- **`assertions`** — machine-checkable rules such as:
  - `command_count` — the workflow produces exactly N commands
  - `command_order` — action A must precede action B
  - `parameter_reference` — a specific named value (e.g., alignment name) threads correctly through downstream commands
  - `parameter_value` — a numeric or string value matches what the user specified in the prompt

The test runner (`workflow_fixtures.test.ts`) loads all `.json` files from `fixtures/`, validates their schema, and runs every declared assertion.

---

## Running the Tests

```bash
# From the project root:
pnpm test tests/integration

# Or run the full test suite:
pnpm test
```

The integration tests are picked up automatically by Vitest alongside the unit tests.

---

## Covered Scenarios

| Fixture | Workflow |
|---------|----------|
| `road_design.json` | Create alignment → sample EG profile → design FG profile → build corridor → extract earthwork volumes |
| `water_main.json` | Create pressure network → add pipes → QC validate cover depth → export pipe schedule CSV |
| `qc_report.json` | Check alignment geometry → check profile grades → generate consolidated QC PDF |
| `earthwork_estimate.json` | Create volume surface → calculate cut/fill with swell/shrink → export earthwork CSV |
| `plan_production.json` | Create sheet set → generate plan/profile sheets → add title sheet → publish PDF |

---

## Adding New Scenarios

1. Create a new `.json` file in `fixtures/`. Use the existing fixtures as a template.
2. Required top-level fields:

```json
{
  "id":                       "unique_snake_case_id",
  "title":                    "Human-readable title",
  "description":              "What this workflow tests",
  "prompt":                   "Realistic user prompt (>20 chars)",
  "expectedCommandSequence":  [ ... ],
  "expectedToolCalls":        [ "civil3d_xxx", ... ],
  "assertions":               [ ... ],
  "tags":                     [ "tag1", "tag2" ]
}
```

3. Each command step must have:

```json
{
  "step":        1,
  "action":      "civil3d_alignment",
  "description": "What this step does",
  "params":      { "action": "create", "name": "My Alignment", ... }
}
```

4. Supported assertion types:

| Type | Required Fields | Description |
|------|-----------------|-------------|
| `command_count` | `expected` (number) | Workflow emits exactly N commands |
| `command_order` | `rule` ("A before B") | Action A appears before action B in the sequence |
| `parameter_reference` | `field`, `expectedValue` | A parameter with this key holds this value somewhere in the sequence |
| `parameter_value` | `field`, `expectedValue` | Same as `parameter_reference` — checks any command has the param/value pair |

5. Run `pnpm test tests/integration` to verify your new fixture passes all auto-generated schema and assertion tests.

---

## Design Philosophy

These are **not live tests** — they encode human-expert knowledge about what the correct Civil3D workflow should look like. When an AI model change or prompt engineering change causes the system to produce a different (potentially wrong) command sequence, these fixtures will catch the regression without needing a running AutoCAD instance.

Think of them as golden-path regression tests for the AI layer.
