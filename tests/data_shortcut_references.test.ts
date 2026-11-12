import { describe, expect, it } from "vitest";
import { z } from "zod";
import { isApprovalRequired } from "../src/tools/approvalPolicy.js";
import {
  DataShortcutReferenceStatusSchema,
  DataShortcutReferencesResponseSchema,
  DataShortcutRepairArgsSchema,
  PROJECT_DOMAIN_DEFINITION,
} from "../src/tools/domains/projectDomain.js";
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

describe("civil3d_project data-shortcut references", () => {
  it("adds references (read) and repair (write) to the canonical project tool", () => {
    const entry = GENERATED_TOOL_CATALOG_ENTRIES.find((item) => item.toolName === "civil3d_project");
    expect(entry!.operations).toEqual(expect.arrayContaining(["data_shortcut_references", "data_shortcut_repair"]));
    expect(entry!.pluginMethods).toEqual(expect.arrayContaining(["listDataShortcutReferences", "repairDataShortcutReference"]));
  });

  it("does not gate the reference inventory but gates repair, promote, and sync", () => {
    expect(approvalFor(PROJECT_DOMAIN_DEFINITION, "civil3d_project", "data_shortcut_references")).toBe(false);
    expect(approvalFor(PROJECT_DOMAIN_DEFINITION, "civil3d_project", "data_shortcut_list")).toBe(false);
    for (const action of ["data_shortcut_repair", "data_shortcut_promote", "data_shortcut_sync"]) {
      expect(approvalFor(PROJECT_DOMAIN_DEFINITION, "civil3d_project", action), action).toBe(true);
    }
  });

  it("validates repair input", () => {
    expect(DataShortcutRepairArgsSchema.safeParse({
      action: "data_shortcut_repair",
      objectType: "surface",
      objectName: "EG",
      sourcePath: "C:/Shortcuts/Proj/Source/EG.dwg",
      autoRepairOther: true,
    }).success).toBe(true);
    expect(DataShortcutRepairArgsSchema.safeParse({
      action: "data_shortcut_repair",
      objectType: "surface",
      objectName: "EG",
      sourcePath: "C:/Shortcuts/Proj/_Shortcuts/Surfaces/EG.xml",
    }).success).toBe(false);
    expect(DataShortcutRepairArgsSchema.safeParse({
      action: "data_shortcut_repair",
      objectType: "parcel",
      objectName: "Lot 1",
      sourcePath: "C:/x.dwg",
    }).success).toBe(false);
  });

  it("accepts the plugin's reference inventory shape", () => {
    expect(DataShortcutReferencesResponseSchema.safeParse({
      workingFolder: "C:\\Civil 3D Projects",
      currentProjectFolder: "Route 9",
      currentProjectPath: "C:\\Civil 3D Projects\\Route 9",
      drawingProjectId: null,
      count: 5,
      statusCounts: { current: 1, out_of_date: 1, source_missing: 1, broken: 1, unknown: 1 },
      references: [
        { objectName: "EG", objectType: "surface", handle: "3F2", layer: "C-TOPO", status: "current", isValid: true, isStale: false, isPartial: false, sourceDrawing: "C:\\Civil 3D Projects\\Route 9\\Source\\EG.dwg", sourceDrawingExists: true, sourceObjectName: "EG", sourceObjectType: "Surface", sourceObjectHandle: "1A2", sourceLocation: "current_project" },
        { objectName: "FG", objectType: "surface", handle: "3F4", layer: "C-TOPO", status: "out_of_date", isValid: true, isStale: true, isPartial: false, sourceDrawing: "C:\\Civil 3D Projects\\Route 8\\Source\\FG.dwg", sourceDrawingExists: true, sourceObjectName: "FG", sourceObjectType: "Surface", sourceObjectHandle: "1A3", sourceLocation: "other_project_in_working_folder" },
        { objectName: "CL", objectType: "alignment", handle: "3F3", layer: "C-ROAD", status: "source_missing", isValid: false, isStale: false, isPartial: false, sourceDrawing: "\\\\old-server\\CL.dwg", sourceDrawingExists: false, sourceObjectName: "CL", sourceObjectType: "Alignment", sourceObjectHandle: "22", sourceLocation: "outside_working_folder" },
        { objectName: "VF", objectType: "view_frame_group", handle: "3F5", layer: "C-ANNO", status: "broken", isValid: false, isStale: false, isPartial: true, sourceDrawing: null, sourceDrawingExists: null, sourceObjectName: null, sourceObjectType: null, sourceObjectHandle: null, sourceLocation: "unknown" },
        { objectName: "Storm", objectType: "pipe_network", handle: "3F6", layer: "C-STRM", status: "unknown", isValid: null, isStale: null, isPartial: false, sourceDrawing: null, sourceDrawingExists: null, sourceObjectName: null, sourceObjectType: null, sourceObjectHandle: null, sourceLocation: "unknown" },
      ],
      notes: [],
    }).success).toBe(true);
    expect(DataShortcutReferenceStatusSchema.options).toEqual(["current", "out_of_date", "broken", "source_missing", "unknown"]);
  });

  it("lets data_shortcut_repair reach view_frame_group through the public tool schema", () => {
    const exposure = PROJECT_DOMAIN_DEFINITION.exposures.find((item) => item.toolName === "civil3d_project")!;
    const publicInput = z.object(exposure.inputShape);
    const args = { action: "data_shortcut_repair", objectType: "view_frame_group", objectName: "VFG - CL", sourcePath: "C:/Shortcuts/Proj/Source/VF.dwg" };
    expect(publicInput.safeParse(args).success).toBe(true);
    expect(DataShortcutRepairArgsSchema.safeParse(args).success).toBe(true);
    expect(publicInput.safeParse({ ...args, objectType: "section_view_group" }).success).toBe(true);
    expect(publicInput.safeParse({ ...args, objectType: "parcel" }).success).toBe(false);
  });
});
