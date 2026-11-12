import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";
import { isApprovalRequired } from "../src/tools/approvalPolicy.js";
import { buildExposureAnnotations } from "../src/tools/domainRuntime.js";
import {
  XREF_DOMAIN_DEFINITION,
  XrefAttachArgsSchema,
  XrefBindArgsSchema,
  XrefDetachArgsSchema,
  XrefListResponseSchema,
  XrefRepathArgsSchema,
} from "../src/tools/domains/xrefDomain.js";
import { GENERATED_TOOL_CATALOG_ENTRIES, findManifestAction } from "../src/tools/toolManifest.js";
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

describe("civil3d_xref domain", () => {
  it("is registered with every action on the canonical tool", () => {
    const entry = GENERATED_TOOL_CATALOG_ENTRIES.find((item) => item.toolName === "civil3d_xref");
    expect(entry).toBeDefined();
    expect(entry!.domain).toBe("xref");
    expect(entry!.operations).toEqual(["list", "attach", "overlay", "detach", "reload", "unload", "bind", "repath"]);
    expect(entry!.pluginMethods).toEqual(expect.arrayContaining([
      "listXrefs", "attachXref", "overlayXref", "detachXrefs", "reloadXrefs", "unloadXrefs", "bindXrefs", "repathXref",
    ]));
    expect(findManifestAction("civil3d_xref", "repath")).toBeDefined();
  });

  it("does not gate list but gates every mutation", () => {
    expect(approvalFor(XREF_DOMAIN_DEFINITION, "civil3d_xref", "list")).toBe(false);
    for (const action of ["attach", "overlay", "detach", "reload", "unload", "bind", "repath"]) {
      expect(approvalFor(XREF_DOMAIN_DEFINITION, "civil3d_xref", action), action).toBe(true);
    }
    expect(XREF_DOMAIN_DEFINITION.actions.list.safeForRetry).toBe(true);
    expect(XREF_DOMAIN_DEFINITION.actions.detach.capabilities).toContain("delete");
    expect(XREF_DOMAIN_DEFINITION.actions.attach.capabilities).toContain("import");
  });

  it("annotates the tool as mutating and destructive", () => {
    const annotations = buildExposureAnnotations(XREF_DOMAIN_DEFINITION, XREF_DOMAIN_DEFINITION.exposures[0]);
    expect(annotations.readOnlyHint).toBe(false);
    expect(annotations.destructiveHint).toBe(true);
    expect(annotations.idempotentHint).toBe(false);
  });

  it("validates attach/overlay input", () => {
    expect(XrefAttachArgsSchema.safeParse({ action: "attach", path: "C:/proj/base.dwg" }).success).toBe(true);
    expect(XrefAttachArgsSchema.safeParse({
      action: "attach",
      path: "C:/proj/base.DWG",
      name: "BASE",
      pathType: "relative",
      insertionPoint: { x: 10, y: 20 },
      scale: 2,
      rotation: 90,
      layer: "X-BASE",
    }).success).toBe(true);
    expect(XrefAttachArgsSchema.safeParse({ action: "attach", path: "C:/proj/base.dxf" }).success).toBe(false);
    expect(XrefAttachArgsSchema.safeParse({ action: "attach", path: "C:/proj/base.dwg", scale: 0 }).success).toBe(false);
    expect(XrefAttachArgsSchema.safeParse({ action: "attach", path: "C:/proj/base.dwg", pathType: "none" }).success).toBe(false);
  });

  it("requires a name or names for batch operations and validates bindType", () => {
    expect(XrefDetachArgsSchema.safeParse({ action: "detach" }).success).toBe(false);
    expect(XrefDetachArgsSchema.safeParse({ action: "detach", names: [] }).success).toBe(false);
    expect(XrefDetachArgsSchema.safeParse({ action: "detach", name: "BASE" }).success).toBe(true);
    expect(XrefBindArgsSchema.safeParse({ action: "bind", names: ["A", "B"], bindType: "insert" }).success).toBe(true);
    expect(XrefBindArgsSchema.safeParse({ action: "bind", names: ["A"], bindType: "explode" }).success).toBe(false);
  });

  it("validates repath input", () => {
    expect(XrefRepathArgsSchema.safeParse({ action: "repath", name: "BASE", newPath: "D:/new/base.dwg", pathType: "relative" }).success).toBe(true);
    expect(XrefRepathArgsSchema.safeParse({ action: "repath", name: "BASE" }).success).toBe(false);
    expect(XrefRepathArgsSchema.safeParse({ action: "repath", name: "BASE", newPath: "D:/new/base.txt" }).success).toBe(false);
  });

  it("accepts the plugin's list response shape including orphaned and unresolved xrefs", () => {
    expect(XrefListResponseSchema.safeParse({
      hostDrawing: "C:\\proj\\sheet.dwg",
      hostIsSaved: true,
      count: 3,
      statusCounts: { loaded: 1, not_found: 1, orphaned: 1 },
      xrefs: [
        { name: "BASE", handle: "2A1", savedPath: ".\\base.dwg", foundPath: "C:\\proj\\base.dwg", pathType: "relative", status: "loaded", rawStatus: "Resolved", attachment: "attach", isNested: false, parents: ["<host>"], childCount: 1, instanceCount: 1 },
        { name: "TOPO", handle: "2A2", savedPath: "C:\\old\\topo.dwg", foundPath: null, pathType: "absolute", status: "not_found", rawStatus: "FileNotFound", attachment: "overlay", isNested: false, parents: ["<host>"], childCount: 0, instanceCount: 1 },
        { name: "UTIL", handle: null, savedPath: null, foundPath: null, pathType: "none", status: "orphaned", rawStatus: "Unresolved", attachment: null, isNested: true, parents: ["BASE"], childCount: 0, instanceCount: null },
      ],
    }).success).toBe(true);
  });
});

describe("civil3d_xref command-line runner", () => {
  it("remembers only unfinished commands, per document, so a later interactive XREF is not cancelled", () => {
    const runner = readFileSync(new URL("../Civil3D-MCP-Plugin/CommandLineRunner.cs", import.meta.url), "utf8");
    const run = runner.slice(runner.indexOf("internal static async Task RunAsync"));
    const command = run.indexOf("await doc.Editor.CommandAsync(tokens)");
    const staleCheck = run.indexOf("FindActiveCommand(doc, PendingCommandsFor(doc))");
    expect(staleCheck).toBeGreaterThan(-1);
    expect(staleCheck).toBeLessThan(command);
    expect(run.indexOf("PendingInvocations.Remove(invocation)")).toBeGreaterThan(command);
    expect(runner).not.toContain("HashSet<string> DrivenCommands");
    expect(runner).toContain("ReferenceEquals(entry.Doc, doc)");
    // CMDNAMES describes only the active drawing, so pending state for
    // another drawing is never updated from it.
    expect(runner).toContain("ReferenceEquals(App.DocumentManager.MdiActiveDocument, doc)");
  });
});
