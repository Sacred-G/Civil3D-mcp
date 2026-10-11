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

  it("finishes the profile view part scan before creating labels", () => {
    const scan = blockAfter("private static List<(ObjectId PartId, ObjectId ProfileViewId)> FindProfileViewParts(");
    expect(scan).not.toMatch(/\byield\b/);
    // Iterating the scan directly would create labels mid-enumeration.
    expect(source).not.toMatch(/\bin\s+FindProfileViewParts\((?![^\n]*\.ToList\(\))/);
    for (const creator of ["CreatePipeLabels(", "CreateStructureLabels("]) {
      const body = blockAfter(`private static IReadOnlyList<ObjectId> ${creator}`);
      const scanAt = body.indexOf("FindProfileViewParts(");
      const createAt = body.search(/(PipeProfileLabel|StructureProfileLabel)\.Create\(/);
      expect(scanAt).toBeGreaterThanOrEqual(0);
      expect(createAt).toBeGreaterThan(scanAt);
      expect(body).toContain("throw NoProfileViewPartError(");
    }
  });

  it("skips a part that lies in no profile view instead of pairing it with the first", () => {
    const scan = blockAfter("private static List<(ObjectId PartId, ObjectId ProfileViewId)> FindProfileViewParts(");
    expect(scan).not.toContain("var profileViewId = profileViewIds[0];");
    expect(scan).toContain("skipped++");
    // GeometricExtents throws on null extents; it is only read through the guard.
    const guard = blockAfter("private static bool TryGetExtents(");
    expect(source.split(".GeometricExtents").length - 1).toBe(1);
    expect(guard).toContain(".GeometricExtents");
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
