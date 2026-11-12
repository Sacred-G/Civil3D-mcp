import { describe, expect, it } from "vitest";
import { DRAWING_RUNTIME_DOMAIN_DEFINITION } from "../src/tools/domains/drawingRuntimeDomain.js";

describe("drawing runtime response contracts", () => {
  it("accepts a standalone drawing with no Civil 3D project", () => {
    const responseSchema = DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.info.responseSchema;

    expect(responseSchema.safeParse({
      drawingName: "Drawing1.dwg",
      projectName: null,
      units: "feet",
    }).success).toBe(true);
  });

  it("accepts settings for a plain AutoCAD drawing where every default style is null", () => {
    // A drawing with no Civil 3D content has no styles to report, so
    // LookupUtils.GetFirstStyleName returns null for each entry.
    const responseSchema = DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.settings.responseSchema;

    expect(responseSchema.safeParse({
      coordinateSystem: null,
      coordinateZone: null,
      datum: null,
      scaleFactor: 1,
      elevationReference: null,
      defaultLayer: "0",
      defaultStyles: { surface: null, alignment: null, profile: null, corridor: null, pipeNetwork: null },
    }).success).toBe(true);
  });

  it("accepts settings for a styled Civil 3D drawing where corridor is null", () => {
    // DrawingCommands.cs reports defaultStyles.corridor as a hardcoded null,
    // so this holds even when every other style resolves to a name.
    const responseSchema = DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.settings.responseSchema;

    expect(responseSchema.safeParse({
      coordinateSystem: "USVA-NF",
      scaleFactor: 1,
      defaultLayer: "C-TOPO",
      defaultStyles: { surface: "Contours 2' and 10'", alignment: "Proposed", profile: "Design", corridor: null, pipeNetwork: "Storm" },
    }).success).toBe(true);
  });

  it("rejects a null defaultStyles object: the plugin always sends one", () => {
    // Only the five style names are nullable (DrawingCommands.cs always
    // builds the defaultStyles dictionary).
    const responseSchema = DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.settings.responseSchema;

    expect(responseSchema.safeParse({ scaleFactor: 1, defaultLayer: "0", defaultStyles: null }).success).toBe(false);
  });
});
