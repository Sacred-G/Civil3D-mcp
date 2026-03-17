/**
 * End-to-end workflow integration tests
 *
 * These tests validate structured JSON fixture files that describe multi-step
 * AI workflows. They do NOT require a live Civil3D/AutoCAD connection.
 * Instead, they verify that each fixture is well-formed and that the
 * command sequences satisfy the declared ordering and parameter assertions.
 *
 * Each fixture represents a realistic user prompt → command sequence scenario
 * that the AI prompt manager, command parser, and tool router should produce.
 */

import { describe, it, expect } from "vitest";
import { readFileSync, readdirSync } from "fs";
import { join } from "path";
import { fileURLToPath } from "url";
import { dirname } from "path";

// ─── Types ────────────────────────────────────────────────────────────────────

interface CommandStep {
  step: number;
  action: string;
  description: string;
  params: Record<string, unknown>;
}

type AssertionType =
  | "command_count"
  | "command_order"
  | "parameter_reference"
  | "parameter_value";

interface Assertion {
  type: AssertionType;
  description: string;
  expected?: number | string | boolean;
  rule?: string;       // "actionA before actionB"
  field?: string;
  expectedValue?: unknown;
}

interface WorkflowFixture {
  id: string;
  title: string;
  description: string;
  prompt: string;
  expectedCommandSequence: CommandStep[];
  expectedToolCalls: string[];
  assertions: Assertion[];
  tags: string[];
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function loadFixtures(): WorkflowFixture[] {
  const __filename = fileURLToPath(import.meta.url);
  const __dirname = dirname(__filename);
  const fixturesDir = join(__dirname, "fixtures");

  return readdirSync(fixturesDir)
    .filter((f) => f.endsWith(".json"))
    .map((f) => {
      const raw = readFileSync(join(fixturesDir, f), "utf-8");
      return JSON.parse(raw) as WorkflowFixture;
    });
}

function collectAllParamValues(
  commands: CommandStep[],
  field: string
): unknown[] {
  return commands
    .map((cmd) => cmd.params[field])
    .filter((v) => v !== undefined);
}

function getActionIndices(commands: CommandStep[], action: string): number[] {
  return commands
    .map((cmd, i) => ({ cmd, i }))
    .filter(({ cmd }) => cmd.action === action || cmd.params["action"] === action)
    .map(({ i }) => i);
}

// ─── Fixture Schema Validation ───────────────────────────────────────────────

describe("Workflow fixture schema validation", () => {
  const fixtures = loadFixtures();

  it("should load at least 5 fixture files", () => {
    expect(fixtures.length).toBeGreaterThanOrEqual(5);
  });

  for (const fixture of fixtures) {
    describe(`Fixture: ${fixture.id}`, () => {
      it("has required top-level fields", () => {
        expect(fixture.id).toBeTruthy();
        expect(fixture.title).toBeTruthy();
        expect(fixture.description).toBeTruthy();
        expect(fixture.prompt).toBeTruthy();
        expect(Array.isArray(fixture.expectedCommandSequence)).toBe(true);
        expect(Array.isArray(fixture.expectedToolCalls)).toBe(true);
        expect(Array.isArray(fixture.assertions)).toBe(true);
        expect(Array.isArray(fixture.tags)).toBe(true);
      });

      it("has a non-empty prompt (simulates realistic user input)", () => {
        expect(fixture.prompt.length).toBeGreaterThan(20);
      });

      it("has at least 3 commands in the sequence", () => {
        expect(fixture.expectedCommandSequence.length).toBeGreaterThanOrEqual(3);
      });

      it("has at least 1 expected tool call", () => {
        expect(fixture.expectedToolCalls.length).toBeGreaterThanOrEqual(1);
      });

      it("has at least 1 assertion", () => {
        expect(fixture.assertions.length).toBeGreaterThanOrEqual(1);
      });

      it("command steps are numbered sequentially starting at 1", () => {
        const steps = fixture.expectedCommandSequence.map((c) => c.step);
        steps.forEach((step, idx) => {
          expect(step).toBe(idx + 1);
        });
      });

      it("each command step has action, description, and params", () => {
        for (const cmd of fixture.expectedCommandSequence) {
          expect(cmd.action).toBeTruthy();
          expect(cmd.description).toBeTruthy();
          expect(typeof cmd.params).toBe("object");
        }
      });

      it("all expected tool calls are referenced in the command sequence", () => {
        const commandActions = fixture.expectedCommandSequence.map(
          (c) => c.action
        );
        for (const toolCall of fixture.expectedToolCalls) {
          const referenced = commandActions.some((a) => a === toolCall);
          expect(
            referenced,
            `Tool '${toolCall}' listed in expectedToolCalls but not found in any command action`
          ).toBe(true);
        }
      });
    });
  }
});

// ─── Assertion Runner ─────────────────────────────────────────────────────────

describe("Workflow assertion validation", () => {
  const fixtures = loadFixtures();

  for (const fixture of fixtures) {
    describe(`Assertions: ${fixture.id}`, () => {
      const commands = fixture.expectedCommandSequence;

      for (const assertion of fixture.assertions) {
        it(assertion.description, () => {
          switch (assertion.type) {
            case "command_count": {
              expect(commands.length).toBe(assertion.expected as number);
              break;
            }

            case "command_order": {
              // rule format: "actionA before actionB"
              const rule = assertion.rule!;
              const match = rule.match(/^(.+?)\s+before\s+(.+?)$/);
              expect(
                match,
                `Assertion rule '${rule}' does not match 'X before Y' format`
              ).not.toBeNull();

              const [, actionA, actionB] = match!;
              const indicesA = getActionIndices(commands, actionA.trim());
              const indicesB = getActionIndices(commands, actionB.trim());

              expect(
                indicesA.length,
                `Action '${actionA}' not found in command sequence`
              ).toBeGreaterThan(0);
              expect(
                indicesB.length,
                `Action '${actionB}' not found in command sequence`
              ).toBeGreaterThan(0);

              const lastA = Math.max(...indicesA);
              const firstB = Math.min(...indicesB);
              expect(
                lastA,
                `'${actionA}' (index ${lastA}) must appear before '${actionB}' (index ${firstB})`
              ).toBeLessThan(firstB);
              break;
            }

            case "parameter_reference": {
              const values = collectAllParamValues(commands, assertion.field!);
              expect(
                values,
                `No command has a '${assertion.field}' parameter`
              ).not.toHaveLength(0);
              expect(values).toContain(assertion.expectedValue);
              break;
            }

            case "parameter_value": {
              const values = collectAllParamValues(commands, assertion.field!);
              expect(
                values,
                `No command has a '${assertion.field}' parameter`
              ).not.toHaveLength(0);
              expect(values).toContain(assertion.expectedValue);
              break;
            }

            default:
              throw new Error(`Unknown assertion type: ${(assertion as Assertion).type}`);
          }
        });
      }
    });
  }
});

// ─── Cross-fixture Sanity Checks ──────────────────────────────────────────────

describe("Cross-fixture sanity checks", () => {
  const fixtures = loadFixtures();

  it("all fixture ids are unique", () => {
    const ids = fixtures.map((f) => f.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it("covers the 5 required workflow scenarios", () => {
    const tags = new Set(fixtures.flatMap((f) => f.tags));
    expect(tags.has("road") || tags.has("alignment")).toBe(true);
    expect(tags.has("water-main") || tags.has("pressure-network")).toBe(true);
    expect(tags.has("qc")).toBe(true);
    expect(tags.has("earthwork") || tags.has("volume")).toBe(true);
    expect(tags.has("plan-production") || tags.has("sheets")).toBe(true);
  });

  it("each fixture references at least one civil3d_ tool", () => {
    for (const fixture of fixtures) {
      const civil3dTools = fixture.expectedToolCalls.filter((t) =>
        t.startsWith("civil3d_")
      );
      expect(
        civil3dTools.length,
        `Fixture '${fixture.id}' should reference at least one civil3d_ tool`
      ).toBeGreaterThan(0);
    }
  });
});
