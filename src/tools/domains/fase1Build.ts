/**
 * Fase 1 build: assembles a "C-300 water & sewer plan, existing conditions only" sheet from a
 * pre-computed spec (see the civil3d-mcp-workflows skill, scripts/c300-build-spec.mjs and
 * references/c300-water-sewer-plan.md section 7 "Pipeline automatizado") in ONE MCP tool call
 * instead of the ~20 individual calls the section describes -- same idea as fase1Audit.ts (which
 * turned 8 reads into 1 call), applied to the write side.
 *
 * This orchestrates existing plugin RPC methods only (newDrawing, saveDrawing, listOpenDocuments,
 * attachXref, createAlignment, insertBlockReference, createEntities, setViewportTwist,
 * listTextEntities, updateTextContent, eraseEntities) -- no new C# code needed. What it deliberately
 * does NOT do: read X-TOPO, query the Property Appraiser, transcribe as-builts, or decide what the
 * entities should be -- that stays in c300-build-spec.mjs / pa-lookup.mjs / poc-extract.mjs (external
 * data, not Civil 3D mutations). This tool takes their OUTPUT (a spec) and draws it
 * (scripts/fase1-build-payload.mjs turns the spec into this tool's payload).
 *
 * Safety (validated live 2026-09-28 with the guide and the real FASE 1 open in the same session):
 * every plugin call acts on the ACTIVE document, so nothing is written until the active document
 * matches `expectedDocument` (or `saveAs`), and the match is checked again right before saving.
 *
 * Kept free of MCP/zod imports so it can be unit-tested with a fake `send`, same as fase1Audit.ts.
 */
import { DEFAULT_SHEET, isOffSheet, isPropText, saysPropWord, stripPropNotes, type SheetExtents } from "./fase1PropNotes.js";

export type Fase1BuildStatus = "OK" | "FAIL" | "SKIPPED";

export interface Fase1BuildStep {
  name: string;
  status: Fase1BuildStatus;
  detail: string;
}

export interface Fase1BuildXref {
  filePath: string;
  layer?: string;
  overlay?: boolean;
  xrefName?: string;
  x?: number;
  y?: number;
  z?: number;
  scale?: number;
  rotation?: number;
}

export interface Fase1BuildAlignment {
  name: string;
  points: { x: number; y: number }[];
  type?: "centerline" | "offset";
  site?: string;
  style?: string;
  layer?: string;
  labelSet?: string;
}

export interface Fase1BuildTwist {
  layout: string;
  viewportHandle?: string;
  twistDegrees?: number;
  streetAngleDegrees?: number;
  centerX?: number;
  centerY?: number;
}

export interface Fase1BuildClImport {
  blockName: string;
  sourceFilePath: string;
}

export interface Fase1BuildTitleReplacement {
  layout: string;
  /** Substring used to find the ONE text entity to edit on that layout (e.g. a fragment only that field has). */
  contains: string;
  find: string;
  replace: string;
}

export type Fase1BuildEntity = Record<string, unknown> & { kind: string };

export interface Fase1BuildOptions {
  templatePath?: string;
  saveAs?: string;
  overwrite?: boolean;
  /**
   * The drawing this build must write to: its file name (e.g. "VILLA ONE @XEREFT FASE 1.dwg") or full path. Matched
   * case-insensitively against the ACTIVE document -- a bare name against the file name only, a path against the
   * whole path. Defaults to `saveAs`. With neither, the tool refuses to write anything.
   */
  expectedDocument?: string;
  xrefs?: Fase1BuildXref[];
  alignment?: Fase1BuildAlignment;
  clImport?: Fase1BuildClImport;
  /** More block definitions to import from another DWG, same as clImport (e.g. "EXIST ARROW" / "FH" from the package C-300 for the utility labels). */
  blockImports?: Fase1BuildClImport[];
  entities?: Fase1BuildEntity[];
  /**
   * Native Civil 3D plan labels (no profile view): { type: "NoteLabel", style, anchor:{x,y}, labelLocation:{x,y}, layer? } for the EOP / R/W notes and
   * { type: "StationOffsetLabel", style, alignmentName, location:{x,y}, labelLocation, layer?, markerStyle?, overrides:[{index,text}] } for the alignment
   * start/end notes. Applied right after the entity batch through profileViewApplyAnnotations WITHOUT profileViewName (the styles must exist in the template).
   */
  planLabels?: Array<Record<string, unknown>>;
  /**
   * Same length and order as planLabels: the MLeader (create_entities shape) that stands in for each native label. Used ONLY for the labels the plugin
   * could not create (typically a template without the label style): they are drawn as MLeaders instead of aborting the build half-way.
   */
  planLabelsFallback?: Fase1BuildEntity[];
  layers?: Record<string, unknown>;
  /** Xref layers to freeze after attaching the xrefs (e.g. "X-TOPO|DIM": the survey's own R/W dims that duplicate the sheet's). */
  freezeLayers?: string[];
  entitySpace?: "model" | "paper";
  entityLayout?: string;
  twists?: Fase1BuildTwist[];
  titleBlock?: Fase1BuildTitleReplacement[];
  /** Fase 1 has no PROP/PROPOSED wording: rewrite the on-sheet MD-WASD notes, erase off-sheet PROP notes. Default true. */
  stripPropNotes?: boolean;
  /** Paper extents of the sheet used to tell on-sheet from off-sheet notes. Default ARCH D 36x24. */
  sheet?: SheetExtents;
  /** Final civil3d_drawing save. Default true. Set false to review before saving yourself. */
  save?: boolean;
}

export type PluginSend = (method: string, params: Record<string, unknown>) => Promise<unknown>;

type Loose = Record<string, unknown>;

interface OpenDocument {
  name?: string;
  filePath?: string | null;
  isActive?: boolean;
}

interface TextEntity {
  handle: string;
  text: string;
  x?: number;
  y?: number;
  /** Drawn extents (plugin build 2026-09-28+). */
  minX?: number;
  minY?: number;
  maxX?: number;
  maxY?: number;
  layout?: string;
  space?: string;
  entityType?: string;
}

const normalizePath = (value: string): string => value.replace(/\//g, "\\").toLowerCase();
const fileName = (value: string): string => {
  const parts = value.split(/[\\/]/);
  return parts[parts.length - 1] || value;
};

/** Does the active document match what the caller said it should be? (bare name -> file name only; path -> full path) */
export function documentMatches(active: OpenDocument, expected: string): boolean {
  const target = normalizePath(expected.trim());
  if (!target) return false;
  const activePath = normalizePath(String(active.filePath || active.name || ""));
  if (/[\\]/.test(target)) {
    return activePath === target || activePath.endsWith(target);
  }
  return normalizePath(fileName(activePath)).includes(target);
}

export async function runFase1Build(send: PluginSend, options: Fase1BuildOptions): Promise<Fase1BuildStep[]> {
  const steps: Fase1BuildStep[] = [];
  let aborted = false;
  const add = (name: string, status: Fase1BuildStatus, detail = "") => steps.push({ name, status, detail });
  const fail = (name: string, detail: string) => {
    aborted = true;
    add(name, "FAIL", detail);
  };

  const call = async (name: string, method: string, params: Loose): Promise<Loose | undefined> => {
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      return undefined;
    }
    try {
      const value = (await send(method, params)) as Loose | undefined;
      add(name, "OK", summarize(method, value));
      return value;
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
      return undefined;
    }
  };

  const expected = options.expectedDocument ?? options.saveAs;
  const checkActiveDocument = async (name: string): Promise<void> => {
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      return;
    }
    if (!expected) {
      fail(
        name,
        "refusing to write: pass expectedDocument (the target DWG's file name or path) or templatePath+saveAs -- every step acts on the ACTIVE document",
      );
      return;
    }
    try {
      const listed = (await send("listOpenDocuments", {})) as { documents?: OpenDocument[] } | undefined;
      const active = listed?.documents?.find((d) => d.isActive);
      if (!active) {
        fail(name, "no active document in Civil 3D");
        return;
      }
      const activePath = String(active.filePath || active.name || "");
      if (!documentMatches(active, expected)) {
        fail(name, `active document is "${activePath}", expected "${expected}" -- switch with acad_set_active_document first; nothing was written`);
        return;
      }
      add(name, "OK", activePath);
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
    }
  };

  const checkNewDrawingActive = async (drawingName: string): Promise<void> => {
    const name = "check new drawing is active";
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      return;
    }
    try {
      const listed = (await send("listOpenDocuments", {})) as { documents?: OpenDocument[] } | undefined;
      const active = listed?.documents?.find((d) => d.isActive);
      const activeName = fileName(String(active?.name || ""));
      if (activeName.toLowerCase() !== fileName(drawingName).toLowerCase()) {
        fail(name, `the new drawing "${drawingName}" is not the active document (active: "${active?.filePath || active?.name || "none"}") -- nothing saved`);
        return;
      }
      add(name, "OK", drawingName);
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
    }
  };

  // 1-2: open the template and give it a real path (mirrors the recipe's steps 5-6: civil3d_drawing
  // new templatePath=... -> save saveAs=... overwrite:true -- there is no "open a file" RPC).
  if (options.templatePath) {
    const created = await call("open template", "newDrawing", { templatePath: options.templatePath });
    // "save as" renames whatever is ACTIVE: make sure that is the drawing just created, not the guide / a delivered file.
    if (created && typeof created.drawingName === "string") {
      await checkNewDrawingActive(created.drawingName);
    }
  }
  if (options.saveAs) {
    await call("save as", "saveDrawing", { saveAs: options.saveAs, overwrite: options.overwrite ?? false });
  }

  // Guard: nothing below may touch a drawing other than the target (the guide / a delivered FASE 1 can be open too).
  await checkActiveDocument("check active document");

  // 3: xrefs (step 7 -- Overlay, the firm's hard rule; default true here too)
  for (const xref of options.xrefs ?? []) {
    await call(`xref ${fileName(xref.filePath)}`, "attachXref", {
      filePath: xref.filePath,
      overlay: xref.overlay ?? true,
      xrefName: xref.xrefName,
      layer: xref.layer,
      x: xref.x,
      y: xref.y,
      z: xref.z,
      scale: xref.scale,
      rotation: xref.rotation,
    });
  }

  // 3b: hide xref layers that print duplicated on the sheet (VILLA ONE 2026-10-01: the survey's own R/W dims on X-TOPO|DIM,
  // some upside down, doubled every C-ANNO 25.00' dim). Runs after the xrefs so their layers exist; a layer the survey does not
  // have is fine (nothing to hide) -- createOrUpdateLayer would otherwise try to CREATE "X-TOPO|..." and fail.
  for (const layerName of options.freezeLayers ?? []) {
    const name = `freeze ${layerName}`;
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      continue;
    }
    try {
      const listed = (await send("listLayers", { name: layerName, includeXref: true })) as { layers?: Loose[] } | undefined;
      const found = (listed?.layers ?? []).find((l) => String(l.name).toLowerCase() === layerName.toLowerCase());
      if (!found) add(name, "OK", "not in this drawing (nothing to hide)");
      else if (found.isFrozen === true) add(name, "OK", "already frozen");
      else {
        await send("createOrUpdateLayer", { name: layerName, frozen: true });
        add(name, "OK", "frozen");
      }
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
    }
  }

  // 4: alignment (step 8 -- pass style/labelSet up front so a separate set_style call isn't needed)
  if (options.alignment) {
    await call(`alignment ${options.alignment.name}`, "createAlignment", {
      name: options.alignment.name,
      points: options.alignment.points,
      type: options.alignment.type,
      site: options.alignment.site,
      style: options.alignment.style,
      layer: options.alignment.layer,
      labelSet: options.alignment.labelSet,
    });
  }

  // 5: import the block DEFINITION for the first matching block entity (step 9 -- e.g. "_cl" from
  // X-TOPO), then let the batch below place any further copies (create_entities needs the block to
  // already be defined in the drawing; it has no sourceFilePath of its own).
  let entities = options.entities ?? [];
  const imports = [...(options.clImport ? [options.clImport] : []), ...(options.blockImports ?? [])];
  for (const imp of imports) {
    const idx = entities.findIndex((e) => e.kind === "block" && e.blockName === imp.blockName);
    const label = `import block "${imp.blockName}"`;
    if (idx >= 0) {
      const first = entities[idx];
      // the first copy is placed by insertBlockReference, which drops a block on the CURRENT layer when its own layer does not exist
      // yet (createEntities makes `layers` only later): define that layer first when the payload describes it
      const layerDef = typeof first.layer === "string" ? (options.layers?.[first.layer] as Loose | undefined) : undefined;
      if (layerDef) await call(`layer ${String(first.layer)}`, "createOrUpdateLayer", { name: first.layer, ...layerDef });
      await call(label, "insertBlockReference", {
        blockName: imp.blockName,
        sourceFilePath: imp.sourceFilePath,
        x: first.x,
        y: first.y,
        z: first.z,
        rotation: first.rotation,
        scaleX: first.scale ?? first.scaleX,
        scaleY: first.scale ?? first.scaleY,
        scaleZ: first.scale ?? first.scaleZ,
        layer: first.layer,
      });
      entities = [...entities.slice(0, idx), ...entities.slice(idx + 1)];
    } else {
      add(label, "SKIPPED", "no block entity with that blockName in the batch");
    }
  }

  // 6: everything else in one batch (step 10)
  if (entities.length) {
    await call(`create ${entities.length} entities`, "createEntities", {
      entities,
      layers: options.layers,
      space: options.entitySpace,
      layout: options.entityLayout,
    });
  }

  // 6b: native plan labels (EOP / EXIST R/W notes + alignment start/end station-offset labels). One call. A label the plugin could not create (e.g. the
  // template lacks its style) is drawn as its MLeader stand-in when planLabelsFallback is given (WARN); otherwise the step fails.
  if (options.planLabels?.length && !aborted) {
    const total = options.planLabels.length;
    const name = `apply ${total} plan labels`;
    let result: Loose | undefined;
    let problem: string | undefined;
    let failedIdx: number[] = [];
    try {
      result = (await send("profileViewApplyAnnotations", { labels: options.planLabels })) as Loose | undefined;
      const entries = Array.isArray(result?.labels) ? (result!.labels as Loose[]) : [];
      failedIdx = entries.filter((l) => typeof l.error === "string").map((l) => Number(l.index));
      if (failedIdx.length) problem = `first: #${failedIdx[0]} ${String(entries.find((l) => typeof l.error === "string")?.error ?? "unknown error")}`;
    } catch (error) {
      problem = error instanceof Error ? error.message : String(error);
      failedIdx = options.planLabels.map((_, i) => i);
    }
    if (!problem) {
      add(name, "OK", summarize("profileViewApplyAnnotations", result));
    } else {
      const stand = failedIdx.map((i) => options.planLabelsFallback?.[i]).filter((e): e is Fase1BuildEntity => !!e);
      if (stand.length === failedIdx.length && stand.length > 0) {
        try {
          await send("createEntities", { entities: stand, layers: options.layers, space: options.entitySpace, layout: options.entityLayout });
          add(name, "OK", `WARN: ${failedIdx.length} of ${total} native labels failed (${problem}); drew ${stand.length} MLeader stand-in(s) instead`);
        } catch (error) {
          fail(`plan labels: ${failedIdx.length} of ${total} failed`, `${problem}; MLeader fallback also failed: ${error instanceof Error ? error.message : String(error)}`);
        }
      } else {
        fail(`plan labels: ${failedIdx.length} of ${total} failed`, problem);
      }
    }
  } else if (options.planLabels?.length) {
    add(`apply ${options.planLabels.length} plan labels`, "SKIPPED", "an earlier step failed");
  }

  // 7: viewport + Model-tab twist (steps 11 and 11b -- pass both in `twists`; without viewportHandle
  // the plugin picks the layout's largest model viewport)
  for (const twist of options.twists ?? []) {
    await call(`twist ${twist.layout}`, "setViewportTwist", {
      layout: twist.layout,
      viewportHandle: twist.viewportHandle,
      twistDegrees: twist.twistDegrees,
      streetAngleDegrees: twist.streetAngleDegrees,
      centerX: twist.centerX,
      centerY: twist.centerY,
    });
  }

  // 8: title block substring replacements (step 12 -- find the ONE text on that layout containing a
  // known fragment, replace only the changed substring, preserving every other MText code untouched).
  // listTextEntities has no layout filter of its own, so the layout is filtered here.
  for (const tb of options.titleBlock ?? []) {
    const name = `title block: "${tb.find}" -> "${tb.replace}"`;
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      continue;
    }
    try {
      const found = (await send("listTextEntities", { space: "paper", contains: tb.contains, limit: 500 })) as
        | { entities?: TextEntity[] }
        | undefined;
      const onLayout = (found?.entities ?? []).filter((e) => (e.layout ?? "").toLowerCase() === tb.layout.toLowerCase());
      if (!onLayout.length) {
        fail(name, `no text containing "${tb.contains}" found on layout "${tb.layout}"`);
        continue;
      }
      if (onLayout.length > 1) {
        fail(name, `"${tb.contains}" matches ${onLayout.length} texts on layout "${tb.layout}" (${onLayout.map((e) => e.handle).join(", ")}) -- use a fragment only one field has`);
        continue;
      }
      const entity = onLayout[0];
      if (!entity.text.includes(tb.find)) {
        fail(name, `handle ${entity.handle} (matched by "${tb.contains}") does not contain "${tb.find}" -- read it and pass the exact substring`);
        continue;
      }
      const newText = entity.text.split(tb.find).join(tb.replace);
      await send("updateTextContent", { handle: entity.handle, text: newText });
      add(name, "OK", `handle ${entity.handle}`);
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
    }
  }

  // 9: Fase 1 = no PROP/PROPOSED on the sheet (the template's MD-WASD notes talk about proposed work).
  if (options.stripPropNotes !== false) {
    await stripPropNotesStep();
  }

  // 10: save (step 13) -- re-check the target first: a dialog or a user click could have switched documents mid-build.
  if (options.save !== false) {
    await checkActiveDocument("check active document before save");
    await call("save", "saveDrawing", { overwrite: true });
  }

  return steps;

  async function stripPropNotesStep(): Promise<void> {
    const name = "Fase 1 notes (no PROP/PROPOSED)";
    if (aborted) {
      add(name, "SKIPPED", "an earlier step failed");
      return;
    }
    try {
      const sheet = options.sheet ?? DEFAULT_SHEET;
      const found = (await send("listTextEntities", { space: "paper", contains: "PROP", limit: 500 })) as
        | { entities?: TextEntity[] }
        | undefined;
      const props = (found?.entities ?? []).filter((e) => saysPropWord(e.text));
      const offSheet = props.filter((e) => isOffSheet(Number(e.x ?? 0), Number(e.y ?? 0), sheet));
      // On the sheet only DESIGN wording counts: the standard existing-facility notes (ALLOWED_PROPOSED_PHRASES) stay.
      const onSheet = props.filter((e) => !offSheet.includes(e) && isPropText(e.text));
      if (!offSheet.length && !onSheet.length) {
        add(name, "OK", "none");
        return;
      }
      // Decide everything BEFORE writing: one unrecognized on-sheet note stops the step with nothing changed.
      const rewrites: { handle: string; text: string; removed: number }[] = [];
      for (const e of onSheet) {
        const result = stripPropNotes(e.text);
        if (!result.ok) {
          fail(name, `${e.layout ?? "paper"}:${e.handle} is on the sheet and says PROP/PROPOSED: ${result.reason}; nothing changed`);
          return;
        }
        rewrites.push({ handle: e.handle, text: result.text, removed: result.removed.length });
      }
      if (offSheet.length) {
        await send("eraseEntities", { handles: offSheet.map((e) => e.handle) });
      }
      for (const r of rewrites) {
        await send("updateTextContent", { handle: r.handle, text: r.text });
      }
      const shifts = rewrites.length ? await shiftOrphans(onSheet) : [];
      const parts = [
        offSheet.length ? `erased ${offSheet.length} off-sheet (${offSheet.map((e) => e.handle).join(", ")})` : "",
        rewrites.length ? `rewrote ${rewrites.map((r) => `${r.handle} (-${r.removed} item(s))`).join(", ")}` : "",
        ...shifts,
      ].filter(Boolean);
      add(name, "OK", parts.join("; "));
    } catch (error) {
      fail(name, error instanceof Error ? error.message : String(error));
    }
  }

  /**
   * A shortened note leaves behind whatever sat on it (VILLA ONE: the vector glyphs of "(NOT PART OF M-WASD NOTES NOR
   * APPROVAL)" under its "PROJECT SPECIFIC NOTES" header, 43 polylines + 2 arcs on PDF_Geometry, moved by hand +7.6222).
   * The note keeps its top, so its bottom (and that header) moves up by dy = newMinY - oldMinY; everything lying ENTIRELY
   * inside the band the note vacated, [oldMinX, oldMaxX] x [oldMinY, newMinY] on the same layout, moves by the same dy.
   */
  async function shiftOrphans(notes: TextEntity[]): Promise<string[]> {
    const tol = 0.05;
    const hasBox = (e: Loose) => [e.minX, e.minY, e.maxX, e.maxY].every((v) => typeof v === "number");
    if (!notes.every((n) => hasBox(n as unknown as Loose))) {
      return ["orphan shift NOT computed (plugin without text extents: redeploy the DLL) -> python scripts/fase1-notes-shift.py"];
    }
    const after = (await send("listTextEntities", { space: "paper", limit: 500 })) as { entities?: (TextEntity & Loose)[] } | undefined;
    const polylines = (await send("listPolylineEntities", { space: "paper", limit: 500 })) as { entities?: Loose[] } | undefined;
    const shapes = (await send("listShapeEntities", { space: "paper", limit: 500 })) as { entities?: Loose[] } | undefined;
    const noteHandles = new Set(notes.map((n) => n.handle));
    const out: string[] = [];
    for (const note of notes as (TextEntity & Loose)[]) {
      const now = (after?.entities ?? []).find((e) => e.handle === note.handle);
      if (!now || !hasBox(now)) {
        out.push(`orphan shift for ${note.handle} NOT computed (no extents after the rewrite) -> python scripts/fase1-notes-shift.py`);
        continue;
      }
      const dy = Number(now.minY) - Number(note.minY);
      if (dy < 0.01) {
        // bottom-attached MText keeps its bottom (nothing orphaned) -- but say so: a silent skip hid a bad extents read once
        out.push(`${note.handle} bottom did not move up (dy ${fmt(dy)}); nothing shifted -- check the plot`);
        continue;
      }
      const band = { x1: Number(note.minX) - tol, x2: Number(note.maxX) + tol, y1: Number(note.minY) - tol, y2: Number(now.minY) + tol };
      const inside = (e: Loose) =>
        hasBox(e) && (String(e.layout ?? "") === String(note.layout ?? "") || !note.layout) &&
        Number(e.minX) >= band.x1 && Number(e.maxX) <= band.x2 && Number(e.minY) >= band.y1 && Number(e.maxY) <= band.y2;
      const candidates = [...(polylines?.entities ?? []), ...(shapes?.entities ?? []), ...(after?.entities ?? []).filter((e) => !noteHandles.has(e.handle))];
      const orphans = candidates.filter(inside).map((e) => String(e.handle));
      if (!orphans.length) {
        out.push(`${note.handle} bottom moved up ${fmt(dy)}; nothing left behind`);
        continue;
      }
      await send("moveEntities", { handles: orphans, dx: 0, dy });
      const truncated = [polylines, shapes].some((r) => (r?.entities ?? []).length >= 500) ? " (a listing hit its 500 limit: check the plot)" : "";
      out.push(`moved ${orphans.length} entit(ies) left under ${note.handle} up ${fmt(dy)}${truncated}`);
    }
    return out;
  }
}

export function summarizeFase1Build(steps: Fase1BuildStep[]): { fail: number; skipped: number; ok: number; summary: string } {
  const fail = steps.filter((s) => s.status === "FAIL").length;
  const skipped = steps.filter((s) => s.status === "SKIPPED").length;
  const ok = steps.filter((s) => s.status === "OK").length;
  return {
    fail,
    skipped,
    ok,
    summary: fail
      ? `Fase 1 build: stopped after ${ok} step(s) OK, 1 FAIL (${skipped} step(s) skipped after it)`
      : `Fase 1 build: ${ok} step(s) OK`,
  };
}

const fmt = (value: unknown, digits = 4): string => (typeof value === "number" ? String(Number(value.toFixed(digits))) : "?");

function summarize(method: string, value: Loose | undefined): string {
  if (value == null) return "";
  if (method === "setViewportTwist") {
    // Layout viewport: { before, after } entries of acad_list_viewports. Model tab: flat twist/snap fields.
    const after = value.after as Loose | undefined;
    const before = value.before as Loose | undefined;
    if (after && typeof after.twistDegrees === "number") {
      return `viewport ${String(after.handle ?? "?")}: twist ${fmt(before?.twistDegrees)}° -> ${fmt(after.twistDegrees)}°, ` +
        `center ${fmt(after.modelCenterX)}, ${fmt(after.modelCenterY)}, scale ${String(after.annotationScale ?? after.scaleLabel ?? "?")}`;
    }
    if (typeof value.twistDegrees === "number") {
      return `twist ${fmt(value.beforeTwistDegrees)}° -> ${fmt(value.twistDegrees)}°, snap ${fmt(value.snapAngleDegrees)}°, ` +
        `center ${fmt(value.modelCenterX)}, ${fmt(value.modelCenterY)}`;
    }
  }
  if (typeof value.handle === "string") return `handle ${value.handle}`;
  if (typeof value.name === "string") return String(value.name);
  if (typeof value.createdCount === "number") return `${value.createdCount} created`;
  const text = JSON.stringify(value);
  return text.length > 200 ? `${text.slice(0, 200)}…` : text;
}
