import { describe, expect, it } from "vitest";
import { isApprovalRequired } from "../src/tools/approvalPolicy.js";
import {
  COMPARE_DOMAIN_DEFINITION,
  CompareDrawingArgsSchema,
  CompareResponseSchema,
  CompareSnapshotArgsSchema,
  CompareSnapshotDiffArgsSchema,
} from "../src/tools/domains/compareDomain.js";
import { GENERATED_TOOL_CATALOG_ENTRIES } from "../src/tools/toolManifest.js";
import type { DomainToolDefinition } from "../src/tools/domainRuntime.js";

function approvalFor(definition: DomainToolDefinition, toolName: string, action: string): boolean {
  const actionDefinition = definition.actions[action];
  expect(actionDefinition, `${toolName}.${action} is defined`).toBeDefined();
  return isApprovalRequired({
    toolName,
    action,
    capabilities: actionDefinition.capabilities,
    safeForRetry: actionDefinition.safeForRetry,
    requiresActiveDrawing: actionDefinition.requiresActiveDrawing,
  });
}

describe("civil3d_compare domain", () => {
  it("is registered with drawing, snapshot and compare_snapshot", () => {
    const entry = GENERATED_TOOL_CATALOG_ENTRIES.find((item) => item.toolName === "civil3d_compare");
    expect(entry!.domain).toBe("compare");
    expect(entry!.operations).toEqual(["drawing", "snapshot", "compare_snapshot"]);
    expect(entry!.pluginMethods).toEqual(["compareDrawings", "writeDrawingSnapshot", "compareDrawingSnapshot"]);
  });

  it("never mutates the drawing: comparisons are ungated reads, the snapshot file write is a gated export", () => {
    expect(approvalFor(COMPARE_DOMAIN_DEFINITION, "civil3d_compare", "drawing")).toBe(false);
    expect(approvalFor(COMPARE_DOMAIN_DEFINITION, "civil3d_compare", "compare_snapshot")).toBe(false);
    expect(approvalFor(COMPARE_DOMAIN_DEFINITION, "civil3d_compare", "snapshot")).toBe(true);
    for (const action of Object.values(COMPARE_DOMAIN_DEFINITION.actions)) {
      expect(action.capabilities).not.toContain("edit");
      expect(action.capabilities).not.toContain("create");
      expect(action.capabilities).not.toContain("delete");
      expect(action.capabilities).not.toContain("manage");
    }
  });

  it("validates paths and detail limits", () => {
    expect(CompareDrawingArgsSchema.safeParse({ action: "drawing", otherPath: "C:/sub/rev1.dwg", maxDetails: 50 }).success).toBe(true);
    expect(CompareDrawingArgsSchema.safeParse({ action: "drawing", otherPath: "C:/sub/rev1.json" }).success).toBe(false);
    expect(CompareDrawingArgsSchema.safeParse({ action: "drawing", otherPath: "C:/sub/rev1.dwg", maxDetails: 10_000 }).success).toBe(false);
    expect(CompareSnapshotArgsSchema.safeParse({ action: "snapshot", outputPath: "C:/sub/rev1.json" }).success).toBe(true);
    expect(CompareSnapshotArgsSchema.safeParse({ action: "snapshot", outputPath: "C:/sub/rev1.dwg" }).success).toBe(false);
    expect(CompareSnapshotDiffArgsSchema.safeParse({ action: "compare_snapshot", snapshotPath: "C:/sub/rev1.json" }).success).toBe(true);
    expect(CompareSnapshotDiffArgsSchema.safeParse({ action: "compare_snapshot" }).success).toBe(false);
  });

  it("accepts the plugin's diff shape", () => {
    const side = { sourcePath: "C:\\sub\\rev1.dwg", sourceKind: "side_database", capturedAtUtc: "2026-09-26T00:00:00Z", entityCount: 3, civilObjectCount: 1, warnings: [] };
    expect(CompareResponseSchema.safeParse({
      mode: "drawing",
      baseline: side,
      current: { ...side, sourceKind: "active_drawing" },
      summary: { added: 1, removed: 0, modified: 1, unchanged: 2, layerChanges: 1, identical: false },
      byType: [{ name: "LINE", added: 1, removed: 0, modified: 1 }],
      byLayer: [{ name: "C-ROAD", added: 1, removed: 0, modified: 1 }],
      details: {
        maxDetails: 200,
        truncated: false,
        added: [{ handle: "2F", type: "LINE", layer: "C-ROAD", space: "Model" }],
        removed: [],
        modified: [{ handle: "1A", type: "LINE", layer: "C-ROAD", space: "Model", previousLayer: "0" }],
      },
      civil: {
        truncated: false,
        byKind: [{ kind: "alignment", baseline: 1, current: 1, added: 0, removed: 0, modified: 1 }],
        added: [],
        removed: [],
        modified: [{ kind: "alignment", name: "CL", handle: "4A", changes: { length: { before: 500, after: 520 } } }],
        unchanged: 0,
      },
      notes: ["..."],
    }).success).toBe(true);
  });
});
