/**
 * Fase 1 audit: is the open drawing "existing conditions only"? (firm rule, see the civil3d-mcp-workflows skill,
 * references/c300-water-sewer-plan.md section "Fase 1: definicion de terminado".) Read-only: composes plugin queries only.
 *
 * Kept free of MCP/zod imports so it can be unit-tested with a fake `send`.
 */
import { isPropText, stripMText } from "./fase1PropNotes.js";

export type Fase1Level = "OK" | "WARN" | "FAIL";

export interface Fase1Check {
  level: Fase1Level;
  what: string;
  detail: string;
}

export interface Fase1AuditOptions {
  /** Style every alignment must use. */
  alignmentStyle?: string;
  /** Layer that carries the EG surface boundary polygon; must be frozen when a surface exists. */
  boundaryLayer?: string;
  /** Layouts a Fase 1 drawing may have. */
  allowedLayouts?: string[];
}

export type PluginSend = (method: string, params: Record<string, unknown>) => Promise<unknown>;

type Loose = Record<string, unknown>;

const asArray = (value: unknown): Loose[] => (Array.isArray(value) ? (value as Loose[]) : []);

export async function runFase1Audit(send: PluginSend, options: Fase1AuditOptions = {}): Promise<Fase1Check[]> {
  const wantedStyle = options.alignmentStyle ?? "BCC - ALIGNMENT";
  const boundaryLayer = options.boundaryLayer ?? "C-TINN-BNDY";
  const allowedLayouts = options.allowedLayouts ?? ["Model", "C-300"];
  const checks: Fase1Check[] = [];
  const add = (level: Fase1Level, what: string, detail = "") => checks.push({ level, what, detail });

  const call = async (method: string, params: Record<string, unknown> = {}): Promise<{ ok: true; value: Loose } | { ok: false; error: string }> => {
    try {
      return { ok: true, value: ((await send(method, params)) ?? {}) as Loose };
    } catch (error) {
      return { ok: false, error: error instanceof Error ? error.message : String(error) };
    }
  };

  // 1. layouts
  const layouts = await call("listLayouts");
  if (!layouts.ok) {
    add("WARN", "layouts", layouts.error);
  } else {
    const names = asArray(layouts.value.layouts).map((l) => String(l.name));
    const extra = names.filter((n) => !allowedLayouts.includes(n));
    const missing = allowedLayouts.filter((n) => n !== "Model" && !names.includes(n));
    if (missing.length) {
      add("FAIL", "layouts", `missing ${missing.join(", ")} (found: ${names.join(", ")})`);
    } else if (extra.length) {
      add("FAIL", "layouts", `extra layouts ${extra.join(", ")} (fase 1 = ${allowedLayouts.join(" + ")} only) -> acad_layout delete_layout`);
    } else {
      add("OK", "layouts", names.join(", "));
    }
  }

  // 2. PROP / PROPOSED wording
  const text = await call("listTextEntities", { space: "all", limit: 500 });
  if (!text.ok) {
    add("WARN", "text scan", text.error);
  } else {
    const entities = asArray(text.value.entities);
    const model = entities.filter((e) => e.space === "model" && isPropText(e.text));
    const paper = entities.filter((e) => e.space === "paper" && isPropText(e.text));
    if (model.length) {
      add("FAIL", "PROP text in Model", `${model.length} item(s): ${model.slice(0, 12).map((e) => `${e.handle}:${stripMText(e.text).slice(0, 40)}`).join(" | ")}${model.length > 12 ? " ..." : ""}`);
    } else {
      add("OK", "PROP text in Model", "none");
    }
    if (paper.length) {
      add("FAIL", "PROP/PROPOSED in paper space", `${paper.length} note MText(s) (${paper.map((e) => `${e.layout}:${e.handle}`).join(", ")}) -> civil3d_workflow_fase1_build { expectedDocument: "<this DWG name>" } fixes both in one call (rewrites the on-sheet MD-WASD notes, erases the off-sheet template notes, saves)`);
    } else {
      add("OK", "PROP/PROPOSED in paper space", "none");
    }
  }

  // 3. design objects
  const pressure = await call("listPressureNetworks");
  if (!pressure.ok) add("WARN", "pressure networks", pressure.error);
  else if (asArray(pressure.value.networks).length) add("FAIL", "pressure networks", `${asArray(pressure.value.networks).map((n) => n.name).join(", ")} -> civil3d_pressure_network_delete`);
  else add("OK", "pressure networks", "none");

  const views = await call("profileViewInfo");
  if (!views.ok) add("WARN", "profile views", views.error);
  else if (asArray(views.value.profileViews).length) add("FAIL", "profile views", `${asArray(views.value.profileViews).map((v) => v.profileViewName).join(" | ")} -> acad_erase_entities`);
  else add("OK", "profile views", "none");

  const gravity = await call("listPipeNetworks");
  if (!gravity.ok) add("WARN", "gravity pipe networks", `cannot list (${gravity.error}); an empty shell may remain -> civil3d_pipe_network_delete {name}`);
  else if (asArray(gravity.value.networks).length) add("FAIL", "gravity pipe networks", `${asArray(gravity.value.networks).map((n) => n.name).join(", ")} -> civil3d_pipe_network_delete`);
  else add("OK", "gravity pipe networks", "none");

  // 4. alignment style
  const alignments = await call("listAlignments");
  if (!alignments.ok) {
    add("WARN", "alignments", alignments.error);
  } else {
    const list = asArray(alignments.value.alignments);
    for (const alignment of list) {
      const detail = await call("getAlignment", { name: alignment.name });
      const style = detail.ok ? String(detail.value.style ?? "?") : "?";
      if (style.toLowerCase() === wantedStyle.toLowerCase()) {
        add("OK", `alignment ${alignment.name}`, `style ${style}`);
      } else {
        add("FAIL", `alignment ${alignment.name}`, `style "${style}" != "${wantedStyle}" -> civil3d_alignment set_style {name:"${alignment.name}", style:"${wantedStyle}"}`);
      }
    }
    if (!list.length) add("WARN", "alignments", "none in the drawing");
  }

  // 5. EG surface boundary hidden (live layer state, needs the listLayers plugin method)
  const surfaces = await call("listSurfaces");
  const surfaceList = surfaces.ok ? asArray(surfaces.value.surfaces) : [];
  if (!surfaceList.length) {
    add("OK", "surface boundary", "no surface in the drawing");
  } else {
    const layer = await call("listLayers", { name: boundaryLayer, includeXref: false });
    if (!layer.ok) {
      add("WARN", "surface boundary", `surface(s) ${surfaceList.map((s) => s.name).join(", ")} present but the layer reader is unavailable (${layer.error}); check that ${boundaryLayer} is frozen`);
    } else {
      const found = asArray(layer.value.layers)[0];
      if (!found) add("WARN", "surface boundary", `layer ${boundaryLayer} not found although surface(s) exist`);
      else if (found.isFrozen === true || found.isOff === true) add("OK", "surface boundary", `${boundaryLayer} is ${found.isFrozen ? "frozen" : "off"} (live)`);
      else add("FAIL", "surface boundary", `${boundaryLayer} is visible: the green EG boundary shows -> acad_create_or_update_layer {name:"${boundaryLayer}", frozen:true}`);
    }
  }

  // 6. dimension text detached from its own line (WARN, not Fase-1-specific): catches the
  // acad_create_aligned_dimension short-dimension bug found 2026-09-28 -- AutoCAD's own DIMFIT
  // auto-placement can eject a short dimension's text several feet from its line (a 5 ft U.E./PL
  // corner dim at 1:20 scale is the case that surfaced it). Mirrors scripts/fase1-audit.mjs check 6
  // so the native tool (subagents, no Bash) and the standalone script never drift apart again.
  const dims = await call("listDimensions", { layer: "C-ANNO", space: "model", limit: 500 });
  if (!dims.ok) {
    add("WARN", "dimension text position", dims.error);
  } else {
    const distToSegment = (px: number, py: number, ax: number, ay: number, bx: number, by: number): number => {
      const dx = bx - ax;
      const dy = by - ay;
      const len2 = dx * dx + dy * dy;
      const t = len2 > 1e-9 ? Math.max(0, Math.min(1, ((px - ax) * dx + (py - ay) * dy) / len2)) : 0;
      const cx = ax + t * dx;
      const cy = ay + t * dy;
      return Math.hypot(px - cx, py - cy);
    };
    const entities = asArray(dims.value.entities);
    const detached: { handle: string; dist: string; lineLen: string }[] = [];
    for (const d of entities) {
      const line1 = Array.isArray(d.xLine1Point) ? (d.xLine1Point as number[]) : undefined;
      const line2 = Array.isArray(d.xLine2Point) ? (d.xLine2Point as number[]) : undefined;
      const textX = typeof d.textX === "number" ? d.textX : undefined;
      const textY = typeof d.textY === "number" ? d.textY : undefined;
      if (!line1 || !line2 || textX === undefined || textY === undefined) continue;
      const [ax, ay] = line1;
      const [bx, by] = line2;
      const lineLen = Math.hypot(bx - ax, by - ay);
      const dist = distToSegment(textX, textY, ax, ay, bx, by);
      if (dist > Math.max(3, lineLen)) {
        detached.push({ handle: String(d.handle), dist: dist.toFixed(2), lineLen: lineLen.toFixed(2) });
      }
    }
    if (detached.length) {
      add(
        "WARN",
        "dimension text position",
        `${detached.length} dimension(s) with text far from their line: ${detached.map((x) => `${x.handle} (${x.dist} ft away, line ${x.lineLen} ft)`).join(", ")} -> recreate with acad_create_aligned_dimension passing explicit dimLineX/dimLineY (not just offset), or reposition in place with acad_update_text_content {handle, x, y}`,
      );
    } else {
      add("OK", "dimension text position", `${entities.length} C-ANNO dimension(s), none detached`);
    }
  }

  // 7. survey dimensions hidden (WARN): an xref's own DIM layer left visible prints its dims on top of the sheet's C-ANNO dims
  // (VILLA ONE 2026-10-01: X-TOPO|DIM, 13 R/W 25.00' dims, some upside down, doubled every C-ANNO dim -- in the guide too).
  // Mirrors scripts/fase1-audit.mjs check 7.
  const ownDims = dims.ok ? asArray(dims.value.entities).length : 0;
  const xrefDimLayers = await call("listLayers", { namePattern: "*|DIM", includeXref: true });
  if (!xrefDimLayers.ok) {
    add("WARN", "survey dimensions hidden", xrefDimLayers.error);
  } else {
    const visible = asArray(xrefDimLayers.value.layers).filter((l) => l.isFrozen !== true && l.isOff !== true).map((l) => String(l.name));
    if (visible.length && ownDims > 0) {
      add("WARN", "survey dimensions hidden", `${visible.join(", ")} visible while the sheet has ${ownDims} C-ANNO dimension(s): they print doubled -> ${visible.map((n) => `acad_create_or_update_layer {name:"${n}", frozen:true}`).join("; ")} (fase1_build does it with freezeLayers)`);
    } else {
      add("OK", "survey dimensions hidden", visible.length ? `${visible.join(", ")} visible but the sheet has no C-ANNO dimensions of its own` : "no xref DIM layer visible");
    }
  }

  return checks;
}

export function summarizeFase1(checks: Fase1Check[]): { fail: number; warn: number; ok: number; summary: string } {
  const fail = checks.filter((c) => c.level === "FAIL").length;
  const warn = checks.filter((c) => c.level === "WARN").length;
  const ok = checks.filter((c) => c.level === "OK").length;
  return {
    fail,
    warn,
    ok,
    summary: fail ? `Fase 1 audit: ${fail} FAIL, ${warn} WARN, ${ok} OK` : `Fase 1 audit: no FAIL (${warn} WARN, ${ok} OK)`,
  };
}
