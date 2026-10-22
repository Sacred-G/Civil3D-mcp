import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";
import { isPropText } from "../src/tools/domains/fase1PropNotes.js";
import { documentMatches, runFase1Build, summarizeFase1Build, type PluginSend } from "../src/tools/domains/fase1Build.js";

type Call = { method: string; params: Record<string, unknown> };

const CF80 = readFileSync(new URL("./fixtures/md-wasd-notes-cf80.txt", import.meta.url), "utf8");
const TARGET = "C:\\Proj\\VILLA ONE\\PROJECT FASE 1.dwg";
const GUIDE = "C:\\Proj\\VILLA ONE\\C-300_GUIA_COMO_DEBE_QUEDAR.dwg";

const openDocs = (activePath: string) => ({
  documents: [
    { name: GUIDE, filePath: GUIDE, isActive: activePath === GUIDE },
    { name: activePath, filePath: activePath, isActive: activePath !== GUIDE },
  ],
});

type Responder = unknown | Error | ((params: Record<string, unknown>) => unknown);

function fakePlugin(responses: Record<string, Responder>, log: Call[]): PluginSend {
  return async (method, params) => {
    log.push({ method, params });
    const value = responses[method];
    if (value instanceof Error) throw value;
    if (value === undefined) throw new Error(`unexpected plugin call ${method}`);
    return typeof value === "function" ? (value as (p: Record<string, unknown>) => unknown)(params) : value;
  };
}

const titleText = { handle: "T1", text: "C-300  WATER AND SEWER PLAN  1\"=20'", layout: "C-300", space: "paper", x: 30, y: 6 };

// listTextEntities answers the title-block lookup (contains != "PROP") and the PROP-notes scan (contains == "PROP").
const listText = (propNotes: unknown[] = []) => (params: Record<string, unknown>) =>
  params.contains === "PROP" ? { entities: propNotes } : { entities: [titleText].filter((e) => e.text.includes(String(params.contains))) };

const happyResponses: Record<string, Responder> = {
  newDrawing: { name: "Drawing2.dwg" },
  saveDrawing: { saved: true },
  listOpenDocuments: openDocs(TARGET),
  attachXref: { xrefName: "X-TOPO" },
  createAlignment: { name: "SW 118TH AVE" },
  insertBlockReference: { handle: "AAA1" },
  createEntities: { createdCount: 2 },
  setViewportTwist: {
    before: { handle: "CDF1", twistDegrees: 359.11 },
    after: { handle: "CDF1", twistDegrees: 268.4891, modelCenterX: 859789.1908, modelCenterY: 444700.6855, annotationScale: "1\" = 20'" },
  },
  listTextEntities: listText(),
  updateTextContent: { handle: "T1" },
  eraseEntities: { erased: 3 },
};

// A Civil 3D session where newDrawing opens "Drawing2.dwg" (active, still named after the template) and "save as" gives it TARGET.
function templateSession(responses: Record<string, Responder>): Record<string, Responder> {
  let savedAs = false;
  return {
    ...responses,
    newDrawing: { drawingName: "Drawing2.dwg", filePath: "C:\\Proj\\_template.dwg" },
    saveDrawing: (p: Record<string, unknown>) => {
      if (p.saveAs) savedAs = true;
      return { saved: true };
    },
    listOpenDocuments: () =>
      savedAs
        ? openDocs(TARGET)
        : { documents: [{ name: GUIDE, filePath: GUIDE, isActive: false }, { name: "Drawing2.dwg", filePath: "C:\\Proj\\_template.dwg", isActive: true }] },
  };
}

const mutating = ["attachXref", "createAlignment", "insertBlockReference", "createEntities", "setViewportTwist", "updateTextContent", "eraseEntities"];

describe("fase1 build", () => {
  it("runs every step in order and reports OK for a clean pipeline", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin(templateSession(happyResponses), log), {
      templatePath: "C-300 template.dwg",
      saveAs: TARGET,
      overwrite: true,
      xrefs: [{ filePath: "X-TOPO.dwg" }, { filePath: "X-UTIL.dwg" }],
      alignment: { name: "SW 118TH AVE", points: [{ x: 0, y: 0 }, { x: 440, y: 0 }], style: "BCC - ALIGNMENT", labelSet: "Major and Minor only" },
      entities: [{ kind: "mtext", text: "SW 118TH AVENUE" }],
      twists: [{ layout: "C-300", streetAngleDegrees: 91.51, centerX: 100, centerY: 200 }],
      titleBlock: [{ layout: "C-300", contains: "1\"=20'", find: "1\"=20'", replace: "AS SHOWN" }],
    });

    expect(summarizeFase1Build(steps).fail).toBe(0);
    expect(steps.every((s) => s.status === "OK")).toBe(true);
    expect(log.map((c) => c.method)).toEqual([
      "newDrawing",
      "listOpenDocuments", // the new drawing is the active one
      "saveDrawing", // save as
      "listOpenDocuments", // guard: saveAs is the expected document
      "attachXref",
      "attachXref",
      "createAlignment",
      "createEntities",
      "setViewportTwist",
      "listTextEntities", // title block
      "updateTextContent",
      "listTextEntities", // PROP notes scan (none)
      "listOpenDocuments", // guard again before saving
      "saveDrawing", // final save
    ]);
  });

  it("refuses to write when the active document is not the expected one (e.g. the guide is focused)", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin({ ...happyResponses, listOpenDocuments: openDocs(GUIDE) }, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      xrefs: [{ filePath: "X-TOPO.dwg" }],
      entities: [{ kind: "mtext", text: "x" }],
    });

    const guard = steps.find((s) => s.name === "check active document")!;
    expect(guard.status).toBe("FAIL");
    expect(guard.detail).toContain("C-300_GUIA_COMO_DEBE_QUEDAR.dwg");
    expect(log.some((c) => mutating.includes(c.method) || c.method === "saveDrawing")).toBe(false);
    expect(steps.find((s) => s.name === "save")!.status).toBe("SKIPPED");
  });

  it("does not 'save as' when the new drawing did not become the active document", async () => {
    const log: Call[] = [];
    const session = templateSession(happyResponses);
    const steps = await runFase1Build(fakePlugin({ ...session, listOpenDocuments: openDocs(GUIDE) }, log), {
      templatePath: "C-300 template.dwg",
      saveAs: TARGET,
      entities: [{ kind: "mtext", text: "x" }],
    });
    expect(steps.find((s) => s.name === "check new drawing is active")!.status).toBe("FAIL");
    expect(log.some((c) => c.method === "saveDrawing" || mutating.includes(c.method))).toBe(false);
  });

  it("refuses to write when neither expectedDocument nor saveAs says which drawing is the target", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin(happyResponses, log), { entities: [{ kind: "mtext", text: "x" }] });
    expect(steps[0]).toMatchObject({ name: "check active document", status: "FAIL" });
    expect(log).toHaveLength(0);
  });

  it("matches a bare file name against the file name only, never against a folder that happens to contain it", () => {
    expect(documentMatches({ filePath: TARGET }, "PROJECT FASE 1.dwg")).toBe(true);
    expect(documentMatches({ filePath: TARGET }, "project fase 1")).toBe(true);
    expect(documentMatches({ filePath: GUIDE }, "VILLA ONE")).toBe(false); // folder name, not the file
    expect(documentMatches({ filePath: TARGET }, "C:/Proj/VILLA ONE/PROJECT FASE 1.dwg")).toBe(true);
    expect(documentMatches({ filePath: GUIDE }, TARGET)).toBe(false);
  });

  it("checks the document again before saving and does not save a drawing that lost focus mid-build", async () => {
    const log: Call[] = [];
    let calls = 0;
    const steps = await runFase1Build(
      fakePlugin({ ...happyResponses, listOpenDocuments: () => (++calls === 1 ? openDocs(TARGET) : openDocs(GUIDE)) }, log),
      { expectedDocument: "PROJECT FASE 1.dwg", entities: [{ kind: "mtext", text: "x" }] },
    );
    expect(steps.find((s) => s.name === "check active document before save")!.status).toBe("FAIL");
    expect(log.filter((c) => c.method === "saveDrawing")).toHaveLength(0);
  });

  it("stops at the first failing step and marks the rest skipped, without undoing what already ran", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(
      fakePlugin(templateSession({ ...happyResponses, attachXref: new Error("file not found") }), log),
      {
        templatePath: "C-300 template.dwg",
        saveAs: TARGET,
        xrefs: [{ filePath: "X-TOPO.dwg" }],
        alignment: { name: "SW 118TH AVE", points: [{ x: 0, y: 0 }, { x: 440, y: 0 }] },
        entities: [{ kind: "mtext", text: "SW 118TH AVENUE" }],
      },
    );

    const totals = summarizeFase1Build(steps);
    expect(totals.fail).toBe(1);
    expect(steps.find((s) => s.name.startsWith("xref"))!.status).toBe("FAIL");
    expect(steps.find((s) => s.name.startsWith("alignment"))!.status).toBe("SKIPPED");
    expect(steps.find((s) => s.name.includes("entities"))!.status).toBe("SKIPPED");
    expect(steps.find((s) => s.name.startsWith("Fase 1 notes"))!.status).toBe("SKIPPED");
    expect(steps.find((s) => s.name === "save")!.status).toBe("SKIPPED");
    // newDrawing and the initial save DID run and are not retried or reversed
    expect(log.filter((c) => c.method === "newDrawing")).toHaveLength(1);
  });

  it("imports the block definition for the first matching block entity and excludes it from the batch", async () => {
    const log: Call[] = [];
    await runFase1Build(fakePlugin(happyResponses, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      clImport: { blockName: "_cl", sourceFilePath: "X-TOPO.dwg" },
      entities: [
        { kind: "block", blockName: "_cl", x: 10, y: 20, rotation: 0, scale: 30, layer: "TEXT" },
        { kind: "block", blockName: "_cl", x: 50, y: 60, rotation: 0, scale: 30, layer: "TEXT" },
        { kind: "mtext", text: "SW 118TH AVENUE" },
      ],
      save: false,
    });

    const importCall = log.find((c) => c.method === "insertBlockReference");
    expect(importCall).toBeDefined();
    expect(importCall!.params).toMatchObject({ blockName: "_cl", sourceFilePath: "X-TOPO.dwg", x: 10, y: 20 });

    const batchCall = log.find((c) => c.method === "createEntities");
    const batchEntities = batchCall!.params.entities as Array<Record<string, unknown>>;
    expect(batchEntities).toHaveLength(2); // the second _cl copy + the mtext, not the imported one
    expect(batchEntities.filter((e) => e.kind === "block")).toHaveLength(1);
  });

  it("imports every blockImports definition (first copy each), creating the block's layer first when the payload describes it", async () => {
    const log: Call[] = [];
    await runFase1Build(fakePlugin({ ...happyResponses, createOrUpdateLayer: { created: true } }, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      clImport: { blockName: "_cl", sourceFilePath: "X-TOPO.dwg" },
      blockImports: [
        { blockName: "EXIST ARROW", sourceFilePath: "PACKAGE.dwg" },
        { blockName: "FH", sourceFilePath: "PACKAGE.dwg" },
      ],
      layers: { "C-FH-EXIST": { colorIndex: 8, linetype: "Continuous", lineweight: -3, plot: true } },
      entities: [
        { kind: "block", blockName: "_cl", x: 1, y: 2, layer: "TEXT" },
        { kind: "block", blockName: "EXIST ARROW", x: 3, y: 4, layer: "C-ANNO" },
        { kind: "block", blockName: "EXIST ARROW", x: 5, y: 6, layer: "C-ANNO" },
        { kind: "block", blockName: "FH", x: 7, y: 8, layer: "C-FH-EXIST" },
        { kind: "mtext", text: "x" },
      ],
      save: false,
    });

    const imports = log.filter((c) => c.method === "insertBlockReference");
    expect(imports.map((c) => c.params.blockName)).toEqual(["_cl", "EXIST ARROW", "FH"]);
    expect(imports[2].params).toMatchObject({ sourceFilePath: "PACKAGE.dwg", x: 7, y: 8 });
    // the FH layer is defined BEFORE the FH import lands on it; C-ANNO has no definition in the payload, so no layer call for it
    const layerCalls = log.filter((c) => c.method === "createOrUpdateLayer");
    expect(layerCalls.map((c) => c.params.name)).toEqual(["C-FH-EXIST"]);
    expect(log.indexOf(layerCalls[0])).toBeLessThan(log.indexOf(imports[2]));
    const batch = log.find((c) => c.method === "createEntities")!.params.entities as Array<Record<string, unknown>>;
    expect(batch).toHaveLength(2); // the second EXIST ARROW + the mtext
  });

  it("applies native plan labels right after the entity batch, with no profileViewName, and fails the step when a label fails", async () => {
    const labels = [
      { type: "NoteLabel", style: "EOP", anchor: { x: 1, y: 2 }, labelLocation: { x: 3, y: 4 }, layer: "C-ANNO" },
      { type: "StationOffsetLabel", style: "ALGN START", alignmentName: "SW 118TH AVE", location: { x: 5, y: 6 }, layer: "C-ROAD-TEXT" },
    ];
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin({ ...happyResponses, profileViewApplyAnnotations: { createdLabels: 2, failedLabels: 0, labels: [] } }, log), {
      expectedDocument: "PROJECT FASE 1.dwg", entities: [{ kind: "mtext", text: "x" }], planLabels: labels, save: false,
    });
    const apply = log.find((c) => c.method === "profileViewApplyAnnotations")!;
    expect(apply.params).toEqual({ labels });
    expect("profileViewName" in apply.params).toBe(false);
    expect(log.indexOf(apply)).toBeGreaterThan(log.findIndex((c) => c.method === "createEntities"));
    expect(steps.find((s) => s.name === "apply 2 plan labels")!.status).toBe("OK");

    const failLog: Call[] = [];
    const failed = await runFase1Build(fakePlugin({ ...happyResponses, profileViewApplyAnnotations: { createdLabels: 1, failedLabels: 1, labels: [{ index: 0 }, { index: 1, error: "style 'ALGN START' not found" }] } }, failLog), {
      expectedDocument: "PROJECT FASE 1.dwg", planLabels: labels, save: false,
    });
    const bad = failed.find((s) => s.status === "FAIL")!;
    expect(bad.name).toContain("plan labels: 1 of 2 failed");
    expect(bad.detail ?? "").toContain("ALGN START");
  });

  it("draws MLeader stand-ins for the plan labels the plugin could not create (template without the style) instead of aborting", async () => {
    const labels = [
      { type: "NoteLabel", style: "EOP", anchor: { x: 1, y: 2 } },
      { type: "NoteLabel", style: "RW", anchor: { x: 3, y: 4 } },
    ];
    const fallback = [{ kind: "mleader", text: "EOP", leaderX: 1, leaderY: 2 }, { kind: "mleader", text: "EXIST R/W", leaderX: 3, leaderY: 4 }];
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin({ ...happyResponses, profileViewApplyAnnotations: { createdLabels: 1, failedLabels: 1, labels: [{ index: 0, handle: "AB" }, { index: 1, error: "style 'RW' not found" }] } }, log), {
      expectedDocument: "PROJECT FASE 1.dwg", planLabels: labels, planLabelsFallback: fallback as never, save: false,
    });
    const batches = log.filter((c) => c.method === "createEntities");
    expect(batches).toHaveLength(1);
    expect(batches[0].params.entities).toEqual([fallback[1]]); // only the label that failed
    const step = steps.find((s) => s.name === "apply 2 plan labels")!;
    expect(step.status).toBe("OK");
    expect(step.detail).toContain("WARN");
    expect(steps.some((s) => s.status === "FAIL")).toBe(false);
  });

  it("skips the block import when no matching block entity is in the batch, and still builds the rest", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(fakePlugin(happyResponses, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      clImport: { blockName: "_cl", sourceFilePath: "X-TOPO.dwg" },
      entities: [{ kind: "mtext", text: "SW 118TH AVENUE" }],
      save: false,
    });

    expect(steps.find((s) => s.name.includes("import block"))!.status).toBe("SKIPPED");
    expect(log.some((c) => c.method === "insertBlockReference")).toBe(false);
    expect(log.some((c) => c.method === "createEntities")).toBe(true);
  });

  it("edits only the matched substring of the title-block text on THAT layout and fails clearly otherwise", async () => {
    const log: Call[] = [];
    const ok = await runFase1Build(fakePlugin(happyResponses, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      titleBlock: [{ layout: "C-300", contains: "WATER AND SEWER", find: "1\"=20'", replace: "AS SHOWN" }],
      save: false,
    });
    expect(ok.every((s) => s.status === "OK")).toBe(true);
    const update = log.find((c) => c.method === "updateTextContent")!;
    expect(update.params.text).toBe("C-300  WATER AND SEWER PLAN  AS SHOWN");

    const notFound = await runFase1Build(fakePlugin(happyResponses, []), {
      expectedDocument: "PROJECT FASE 1.dwg",
      titleBlock: [{ layout: "C-300", contains: "WATER AND SEWER", find: "NOT PRESENT", replace: "X" }],
      save: false,
    });
    expect(notFound.find((s) => s.name.includes("title block"))!.status).toBe("FAIL");

    // the text lives on C-300: asking for C-301 must not edit it (listTextEntities itself has no layout filter)
    const log3: Call[] = [];
    const otherLayout = await runFase1Build(fakePlugin(happyResponses, log3), {
      expectedDocument: "PROJECT FASE 1.dwg",
      titleBlock: [{ layout: "C-301", contains: "WATER AND SEWER", find: "1\"=20'", replace: "AS SHOWN" }],
      save: false,
    });
    expect(otherLayout.find((s) => s.name.includes("title block"))!.status).toBe("FAIL");
    expect(log3.some((c) => c.method === "updateTextContent")).toBe(false);

    // two texts on the layout match the fragment -> ambiguous, nothing edited
    const log4: Call[] = [];
    const twin = { ...titleText, handle: "T2" };
    const ambiguous = await runFase1Build(
      fakePlugin({ ...happyResponses, listTextEntities: (p: Record<string, unknown>) => (p.contains === "PROP" ? { entities: [] } : { entities: [titleText, twin] }) }, log4),
      { expectedDocument: "PROJECT FASE 1.dwg", titleBlock: [{ layout: "C-300", contains: "WATER", find: "1\"=20'", replace: "AS SHOWN" }], save: false },
    );
    expect(ambiguous.find((s) => s.name.includes("title block"))!.detail).toContain("T1, T2");
    expect(log4.some((c) => c.method === "updateTextContent")).toBe(false);
  });

  it("removes PROP/PROPOSED from the sheet: rewrites the on-sheet MD-WASD notes and erases the off-sheet template notes", async () => {
    const log: Call[] = [];
    const propNotes = [
      { handle: "CF80", text: CF80, layout: "C-300", space: "paper", x: 25.66, y: 17.923 },
      { handle: "CF57", text: "...FOR THE SCHEDULING OF LICENSED OPERATOR TO BE PRESENT FOR PROPOSED ACTIVITY.}", layout: "C-300", space: "paper", x: 48.315, y: 16.292 },
      { handle: "CF6E", text: "{\\LFOR ALL PROJECTS WHERE REMOVAL OF UTILITY LINES IS PROPOSED}", layout: "C-300", space: "paper", x: 63.855, y: 16.29 },
      { handle: "SUBJ", text: "SUBJECT PROPERTY", layout: "C-300", space: "paper", x: 10, y: 10 }, // not PROP as a word
    ];
    const steps = await runFase1Build(fakePlugin({ ...happyResponses, listTextEntities: listText(propNotes) }, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
    });

    const notes = steps.find((s) => s.name.startsWith("Fase 1 notes"))!;
    expect(notes.status).toBe("OK");
    expect(log.find((c) => c.method === "eraseEntities")!.params.handles).toEqual(["CF57", "CF6E"]);
    const rewrite = log.find((c) => c.method === "updateTextContent")!;
    expect(rewrite.params.handle).toBe("CF80");
    // design wording is gone; the two existing-facility notes stay (ALLOWED_PROPOSED_PHRASES)
    expect(isPropText(rewrite.params.text)).toBe(false);
    expect(String(rewrite.params.text)).toContain("THE FOLLOWING ACTIVITIES ON EXISTING WATER SERVICES");
    expect(log.at(-1)!.method).toBe("saveDrawing");
  });

  it("moves what the shortened notes left behind (VILLA ONE: the '(NOT PART OF M-WASD NOTES…)' glyphs) by the distance the notes' bottom moved up", async () => {
    const log: Call[] = [];
    let rewritten = false;
    const cf80Before = { handle: "CF80", text: CF80, layout: "C-300", space: "paper", x: 25.66, y: 17.923, minX: 25.6, minY: 6.4, maxX: 32.0, maxY: 17.95 };
    const cf80After = { ...cf80Before, text: "(rewritten)", minY: 14.0222 }; // top fixed, bottom up 7.6222
    const steps = await runFase1Build(
      fakePlugin(
        {
          ...happyResponses,
          listTextEntities: (p: Record<string, unknown>) =>
            p.contains === "PROP" ? { entities: [cf80Before] } : { entities: [rewritten ? cf80After : cf80Before, { handle: "D4F2", text: "AGR. NO.", layout: "C-300", minX: 32.4, minY: 0.8, maxX: 36.3, maxY: 1.0 }] },
          updateTextContent: () => { rewritten = true; return { handle: "CF80" }; },
          listPolylineEntities: {
            entities: [
              { handle: "D25C", layout: "C-300", minX: 25.62, minY: 7.39, maxX: 26.1, maxY: 7.56 }, // glyph: moves
              { handle: "D286", layout: "C-300", minX: 28.2, minY: 7.4, maxX: 28.6, maxY: 7.55 }, // glyph: moves
              { handle: "FRAME", layout: "C-300", minX: 25.5, minY: 0.2, maxX: 25.5, maxY: 23.6 }, // sheet frame crossing the band: stays
              { handle: "OTHER", layout: "C-301", minX: 26, minY: 8, maxX: 27, maxY: 8.2 }, // another layout: stays
            ],
          },
          listShapeEntities: { entities: [{ handle: "D25B", layout: "C-300", minX: 25.6, minY: 7.41, maxX: 25.7, maxY: 7.5 }] }, // arc glyph: moves
          moveEntities: { movedCount: 3 },
        },
        log,
      ),
      { expectedDocument: "PROJECT FASE 1.dwg", save: false },
    );

    const move = log.find((c) => c.method === "moveEntities")!;
    expect(move.params.handles).toEqual(["D25C", "D286", "D25B"]);
    expect(move.params.dx).toBe(0);
    expect(move.params.dy as number).toBeCloseTo(7.6222, 4);
    expect(steps.find((s) => s.name.startsWith("Fase 1 notes"))!.detail).toContain("moved 3 entit(ies) left under CF80 up 7.6222");
  });

  it("says so (instead of guessing) when the plugin gives no text extents to measure the shift", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(
      fakePlugin({ ...happyResponses, listTextEntities: listText([{ handle: "CF80", text: CF80, layout: "C-300", x: 25.66, y: 17.923 }]) }, log),
      { expectedDocument: "PROJECT FASE 1.dwg", save: false },
    );
    expect(steps.find((s) => s.name.startsWith("Fase 1 notes"))!.detail).toContain("fase1-notes-shift.py");
    expect(log.some((c) => c.method === "moveEntities")).toBe(false);
  });

  it("freezes the requested xref layers right after the xrefs (only when present and visible)", async () => {
    const log: Call[] = [];
    const state: Record<string, boolean> = { "X-TOPO|DIM": false, "X-TOPO|ELEVATIONS": true };
    const steps = await runFase1Build(
      fakePlugin(
        {
          ...happyResponses,
          listLayers: (p: Record<string, unknown>) => ({ layers: p.name in state ? [{ name: p.name, isFrozen: state[String(p.name)] }] : [] }),
          createOrUpdateLayer: (p: Record<string, unknown>) => { state[String(p.name)] = Boolean(p.frozen); return { name: p.name, frozen: p.frozen }; },
        },
        log,
      ),
      {
        expectedDocument: "PROJECT FASE 1.dwg",
        xrefs: [{ filePath: "C:\\Proj\\X-TOPO.dwg" }],
        freezeLayers: ["X-TOPO|DIM", "X-TOPO|ELEVATIONS", "X-TOPO|NOT_IN_THIS_SURVEY"],
        stripPropNotes: false,
        save: false,
      },
    );
    const byName = (n: string) => steps.find((s) => s.name === `freeze ${n}`)!;
    expect(byName("X-TOPO|DIM").detail).toBe("frozen");
    expect(byName("X-TOPO|ELEVATIONS").detail).toBe("already frozen");
    expect(byName("X-TOPO|NOT_IN_THIS_SURVEY").detail).toContain("nothing to hide");
    expect(steps.every((s) => s.status === "OK")).toBe(true);
    // after the xref, and only one write (the visible layer)
    const order = log.map((c) => c.method);
    expect(order.indexOf("attachXref")).toBeLessThan(order.indexOf("listLayers"));
    expect(log.filter((c) => c.method === "createOrUpdateLayer").map((c) => c.params.name)).toEqual(["X-TOPO|DIM"]);
  });

  it("reports (never silently skips) a rewrite whose bottom did not move up (2026-09-28 live run: bad MText extents)", async () => {
    const log: Call[] = [];
    // What the first live run read for CF80 before and after the rewrite: a 0.46" box that did not change.
    const cf80 = { handle: "CF80", text: CF80, layout: "C-300", space: "paper", x: 25.66, y: 17.923, minX: 25.66, minY: 17.4617, maxX: 32.02, maxY: 17.923 };
    const steps = await runFase1Build(
      fakePlugin(
        {
          ...happyResponses,
          listTextEntities: (p: Record<string, unknown>) => ({ entities: [p.contains === "PROP" ? cf80 : { ...cf80, text: "(rewritten)" }] }),
          listPolylineEntities: { entities: [] },
          listShapeEntities: { entities: [] },
        },
        log,
      ),
      { expectedDocument: "PROJECT FASE 1.dwg", save: false },
    );
    expect(steps.find((s) => s.name.startsWith("Fase 1 notes"))!.detail).toContain("CF80 bottom did not move up");
    expect(log.some((c) => c.method === "moveEntities")).toBe(false);
  });

  it("stops without changing anything when an on-sheet PROP text is not the MD-WASD notes", async () => {
    const log: Call[] = [];
    const steps = await runFase1Build(
      fakePlugin({ ...happyResponses, listTextEntities: listText([{ handle: "X1", text: "PROPOSED 8\" WM", layout: "C-300", x: 10, y: 10 }]) }, log),
      { expectedDocument: "PROJECT FASE 1.dwg" },
    );
    expect(steps.find((s) => s.name.startsWith("Fase 1 notes"))!.status).toBe("FAIL");
    expect(log.some((c) => c.method === "eraseEntities" || c.method === "updateTextContent" || c.method === "saveDrawing")).toBe(false);
  });

  it("skips the PROP-notes step with stripPropNotes:false", async () => {
    const log: Call[] = [];
    await runFase1Build(fakePlugin(happyResponses, log), { expectedDocument: "PROJECT FASE 1.dwg", stripPropNotes: false, save: false });
    expect(log.some((c) => c.method === "listTextEntities")).toBe(false);
  });

  it("reports the twist actually applied (not a truncated 'before' dump)", async () => {
    const steps = await runFase1Build(
      fakePlugin(
        {
          ...happyResponses,
          setViewportTwist: (p: Record<string, unknown>) =>
            p.layout === "Model"
              ? { layout: "Model", beforeTwistDegrees: 0, twistDegrees: 268.4891, snapAngleDegrees: 91.5109, modelCenterX: 859788.5785, modelCenterY: 444723.8975 }
              : (happyResponses.setViewportTwist as unknown),
        },
        [],
      ),
      {
        expectedDocument: "PROJECT FASE 1.dwg",
        twists: [{ layout: "C-300", streetAngleDegrees: 91.5109 }, { layout: "Model", streetAngleDegrees: 91.5109 }],
        save: false,
      },
    );
    expect(steps.find((s) => s.name === "twist C-300")!.detail).toBe("viewport CDF1: twist 359.11° -> 268.4891°, center 859789.1908, 444700.6855, scale 1\" = 20'");
    expect(steps.find((s) => s.name === "twist Model")!.detail).toContain("-> 268.4891°, snap 91.5109°");
  });

  it("honours save:false by not issuing a final save", async () => {
    const log: Call[] = [];
    await runFase1Build(fakePlugin(happyResponses, log), {
      expectedDocument: "PROJECT FASE 1.dwg",
      entities: [{ kind: "mtext", text: "x" }],
      save: false,
    });
    expect(log.filter((c) => c.method === "saveDrawing")).toHaveLength(0);
  });
});
