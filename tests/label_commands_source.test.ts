import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";

// No AutoCAD-free harness reaches LabelCommands, so these checks read the
// source. They guard the label_add fixes that were verified against the Civil
// 3D 2026 API but cannot run here. Line endings are normalized because git's
// autocrlf checks the file out as CRLF on Windows.
const source = readFileSync(
  fileURLToPath(new URL("../Civil3D-MCP-Plugin/LabelCommands.cs", import.meta.url)),
  "utf8",
).replaceAll("\r\n", "\n");

// Text from `marker` to the brace that closes the first block after it.
// Braces inside C# interpolated strings are balanced, so counting works.
function blockAfter(marker: string): string {
  const start = source.indexOf(marker);
  expect(start, `marker not found: ${marker}`).toBeGreaterThanOrEqual(0);
  const open = source.indexOf("{", start + marker.length);
  let depth = 0;
  for (let i = open; i < source.length; i++) {
    if (source[i] === "{") depth++;
    if (source[i] === "}" && --depth === 0) return source.slice(start, i + 1);
  }
  throw new Error(`unbalanced block after ${marker}`);
}

const pipeBranch = () => blockAfter('if (string.Equals(objectType, "pipe", StringComparison.OrdinalIgnoreCase))\n');
const structureBranch = () => blockAfter('if (string.Equals(objectType, "structure", StringComparison.OrdinalIgnoreCase))\n');

describe("label_add source guards (LabelCommands.cs)", () => {
  it("returns every pipe and structure label, not only the first", () => {
    expect(source).not.toMatch(/Create(Pipe|Structure)Labels\([^;]*\)\[0\]/);
    for (const branch of [pipeBranch(), structureBranch()]) {
      expect(branch).toContain('["handle"] = ResolveHandle(transaction, labelIds[0])');
      expect(branch).toContain('["handles"] = labelIds.Select(');
      expect(branch).toContain('["created"] = labelIds.Count');
      expect(branch).toContain('["skipped"] = skipped');
    }
  });

  it("labels every profile view that shows the part, with the part's ProfileViewPart", () => {
    // Civil 3D 2026 keeps one ProfileViewPart per part, shared by all its views,
    // with placeholder extents (live 2026-10-11): matching views by extents
    // labelled only the first view.
    expect(source).not.toContain(".GeometricExtents");
    const profile = blockAfter("private static IReadOnlyList<ObjectId> CreateProfileLabels(");
    expect(profile).toContain("part.GetProfileViewsDisplayingMe()");
    expect(profile).toContain("candidates.Add(part.ProfileViewPartId)");
    expect(profile).toMatch(/foreach \(ObjectId profileViewId in profileViewIds\)/);
    expect(profile).toContain("skipped++");
    // The model-space scan is a fallback after a refused attempt, and a list.
    expect(profile.indexOf("FindProfileViewParts(")).toBeGreaterThan(profile.indexOf("TryCreateProfileLabel("));
    expect(blockAfter("private static List<ObjectId> FindProfileViewParts(")).not.toMatch(/\byield\b/);
    for (const creator of ["CreatePipeLabels(", "CreateStructureLabels("]) {
      expect(blockAfter(`private static IReadOnlyList<ObjectId> ${creator}`)).toContain("CreateProfileLabels(transaction, ");
    }
  });

  it("reports the alignment's label group count after label_set", () => {
    const labelSet = blockAfter('string.Equals(labelType, "label_set", StringComparison.OrdinalIgnoreCase))\n');
    expect(labelSet).toContain("ImportLabelSet(labelSetId)");
    expect(labelSet).toContain('["labelGroups"] = alignment.GetAlignmentLabelGroupIds().Count');
  });

  it("labels the arcs inside compound curves as well as simple arcs", () => {
    const curves = blockAfter("private static IReadOnlyList<ObjectId> CreateAlignmentCurveLabels(");
    expect(curves).toContain("is Autodesk.Civil.DatabaseServices.AlignmentArc arc");
    expect(curves).toContain("entity.SubEntityCount");
    expect(curves).toContain("is Autodesk.Civil.DatabaseServices.AlignmentSubEntityArc subArc");
  });
});
