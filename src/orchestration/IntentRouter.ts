import { TOOL_CATALOG, ToolCatalogEntry } from "./ToolCatalog.js";

export interface RouteParams {
  name?: string;
  alignmentName?: string;
  profileName?: string;
  surfaceName?: string;
  style?: string;
  layer?: string;
  labelSet?: string;
}

export interface RouteResult {
  match: ToolCatalogEntry;
  confidence: number;
  missingFields: string[];
  extractedParams: RouteParams;
  reasoning: string;
}

function normalize(text: string): string {
  return text.toLowerCase().replace(/[^a-z0-9_\s-]/g, " ").replace(/\s+/g, " ").trim();
}

function extractQuotedValue(request: string): string | undefined {
  const quoteMatch = request.match(/["']([^"']+)["']/);
  return quoteMatch?.[1]?.trim();
}

function extractNamedValue(request: string, fieldLabel: string): string | undefined {
  const expression = new RegExp(`${fieldLabel}\\s*[:=]\\s*([^,;]+)`, "i");
  const match = request.match(expression);
  return match?.[1]?.trim();
}

function extractParams(request: string): RouteParams {
  const quotedValue = extractQuotedValue(request);
  const alignmentName = extractNamedValue(request, "alignment(?:name)?");
  const profileName = extractNamedValue(request, "profile(?:name)?");
  const surfaceName = extractNamedValue(request, "surface(?:name)?");
  const style = extractNamedValue(request, "style");
  const layer = extractNamedValue(request, "layer");
  const labelSet = extractNamedValue(request, "labelset");

  const createSurfaceName = /(?:create|make|add)\s+(?:a\s+|an\s+)?surface\s+(?:named|called)?\s*([a-z0-9_\-]+)/i.exec(request)?.[1];
  const profileFromSurfaceName = /(?:profile)\s+(?:named|called)?\s*([a-z0-9_\-]+)/i.exec(request)?.[1];

  return {
    name: createSurfaceName ?? extractNamedValue(request, "name") ?? quotedValue,
    alignmentName: alignmentName ?? undefined,
    profileName: profileName ?? profileFromSurfaceName ?? undefined,
    surfaceName: surfaceName ?? undefined,
    style: style ?? undefined,
    layer: layer ?? undefined,
    labelSet: labelSet ?? undefined,
  };
}

function scoreRequest(normalizedRequest: string, entry: ToolCatalogEntry): number {
  let score = 0;

  for (const keyword of entry.keywords) {
    const normalizedKeyword = normalize(keyword);
    if (normalizedRequest.includes(normalizedKeyword)) {
      score += normalizedKeyword.split(" ").length > 1 ? 4 : 2;
    }
  }

  if (entry.domain === "surface" && normalizedRequest.includes("surface")) {
    score += 2;
  }

  if (entry.domain === "alignment" && normalizedRequest.includes("alignment")) {
    score += 2;
  }

  if (entry.domain === "profile" && normalizedRequest.includes("profile")) {
    score += 2;
  }

  if (entry.domain === "corridor" && normalizedRequest.includes("corridor")) {
    score += 2;
  }

  if (entry.intent === "drawing_info" && (normalizedRequest.includes("drawing") || normalizedRequest.includes("project"))) {
    score += 2;
  }

  return score;
}

export function routeIntent(request: string): RouteResult {
  const normalizedRequest = normalize(request);
  const extractedParams = extractParams(request);

  const ranked = TOOL_CATALOG.map((entry) => ({
    entry,
    score: scoreRequest(normalizedRequest, entry),
  })).sort((left, right) => right.score - left.score);

  const best = ranked[0]?.entry ?? TOOL_CATALOG[0];
  const bestScore = ranked[0]?.score ?? 0;
  const missingFields = best.requiredFields.filter((field) => {
    const value = extractedParams[field as keyof RouteParams];
    return typeof value !== "string" || value.trim().length === 0;
  });

  const confidence = Math.min(1, bestScore / 8);
  const reasoning = `Matched intent '${best.intent}' in domain '${best.domain}' using keyword scoring.`;

  return {
    match: best,
    confidence,
    missingFields,
    extractedParams,
    reasoning,
  };
}
