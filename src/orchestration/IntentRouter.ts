import { TOOL_CATALOG, ToolCatalogEntry } from "./ToolCatalog.js";

export interface RouteParams {
  name?: string;
  alignmentName?: string;
  corridorName?: string;
  profileName?: string;
  surfaceName?: string;
  baseSurface?: string;
  comparisonSurface?: string;
  style?: string;
  layer?: string;
  labelSet?: string;
  filePath?: string;
  outputPath?: string;
  gridSpacing?: number;
  designSpeed?: number;
  inflow?: number;
  outflow?: number;
  bottomElevation?: number;
  topElevation?: number;
  payItems?: Array<{
    code: string;
    description: string;
    unit: string;
    unitPrice: number;
  }>;
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

function extractNamedNumber(request: string, fieldLabel: string): number | undefined {
  const expression = new RegExp(`${fieldLabel}\\s*[:=]?\\s*(-?\\d+(?:\\.\\d+)?)`, "i");
  const match = request.match(expression);
  return match ? Number(match[1]) : undefined;
}

function extractPathValue(request: string, fieldLabel: string): string | undefined {
  const named = extractNamedValue(request, fieldLabel);
  if (named) {
    return named.replace(/^['"]|['"]$/g, "").trim();
  }

  const quoted = new RegExp(`${fieldLabel}\\s*[=:]?\\s*["']([^"']+)["']`, "i").exec(request)?.[1]?.trim();
  return quoted;
}

function extractObjectName(request: string, objectType: string): string | undefined {
  const stopPattern = String.raw`(?=\s+(?:(?:on|for|using|with|from)\s+)?(?:alignment|profile|surface|corridor|design\s*speed|grid\s*spacing|spacing|file(?:path)?|output(?:path)?|inflow|outflow|bottom(?:\s+elevation)?|top(?:\s+elevation)?|style|layer|labelset)\b|$)`;
  const directMatch = new RegExp(`${objectType}\\s+(?:named|called)\\s*["']?([^"',;]+?)["']?${stopPattern}`, "i").exec(request)?.[1]?.trim();
  if (directMatch) {
    return directMatch;
  }

  const verbMatch = new RegExp(`(?:get|show|inspect|rebuild|update|refresh|sample|analyze|check|create)\\s+(?:the\\s+)?${objectType}\\s*["']?([^"',;]+?)["']?${stopPattern}`, "i")
    .exec(request)?.[1]?.trim();
  if (verbMatch) {
    return verbMatch;
  }

  const forMatch = new RegExp(`(?:for|on|using)\\s+(?:the\\s+)?${objectType}\\s*["']?([^"',;]+?)["']?${stopPattern}`, "i").exec(request)?.[1]?.trim();
  return forMatch;
}

function extractSurfacePair(request: string) {
  const betweenMatch = /between\s+surfaces?\s+["']?([^"',;]+)["']?\s+and\s+["']?([^"',;]+)["']?/i.exec(request);
  if (betweenMatch) {
    return {
      baseSurface: betweenMatch[1].trim(),
      comparisonSurface: betweenMatch[2].trim(),
    };
  }

  return {
    baseSurface: extractNamedValue(request, "base(?:\\s+surface)?"),
    comparisonSurface: extractNamedValue(request, "comparison(?:\\s+surface)?") ?? extractNamedValue(request, "design(?:\\s+surface)?"),
  };
}

function hasRequiredValue(value: unknown): boolean {
  if (typeof value === "string") {
    return value.trim().length > 0;
  }

  if (typeof value === "number") {
    return Number.isFinite(value);
  }

  if (Array.isArray(value)) {
    return value.length > 0;
  }

  return value != null;
}

function extractParams(request: string): RouteParams {
  const quotedValue = extractQuotedValue(request);
  const alignmentName = extractNamedValue(request, "alignment(?:name)?");
  const profileName = extractNamedValue(request, "profile(?:name)?");
  const surfaceName = extractNamedValue(request, "surface(?:name)?");
  const { baseSurface, comparisonSurface } = extractSurfacePair(request);
  const style = extractNamedValue(request, "style");
  const layer = extractNamedValue(request, "layer");
  const labelSet = extractNamedValue(request, "labelset");
  const filePath = extractPathValue(request, "file(?:path)?");
  const outputPath = extractPathValue(request, "output(?:path)?");
  const gridSpacing = extractNamedNumber(request, "grid\\s*spacing") ?? extractNamedNumber(request, "spacing");
  const designSpeed = extractNamedNumber(request, "design\\s*speed")
    ?? /(?:at|for)\s+(\d+(?:\.\d+)?)\s*(?:kmh|mph|kph)/i.exec(request)?.[1];
  const inflow = extractNamedNumber(request, "inflow");
  const outflow = extractNamedNumber(request, "outflow");
  const bottomElevation = extractNamedNumber(request, "bottom(?:\\s+elevation)?");
  const topElevation = extractNamedNumber(request, "top(?:\\s+elevation)?");
  const namedSurface = extractObjectName(request, "surface");
  const namedAlignment = extractObjectName(request, "alignment");
  const namedProfile = extractObjectName(request, "profile");
  const namedCorridor = extractObjectName(request, "corridor");

  const createSurfaceName = /(?:create|make|add)\s+(?:a\s+|an\s+)?surface\s+(?!from\b)(?:named|called)?\s*([a-z0-9_\-]+)/i.exec(request)?.[1];
  const createSurfaceFromDemName = /(?:create|make|add)\s+(?:a\s+|an\s+)?surface\s+from\s+dem\s+(?:named|called)?\s*([a-z0-9_\-]+)/i.exec(request)?.[1];
  const createLayoutProfileName = /(?:create|make|add)\s+(?:a\s+)?(?:layout|design|proposed)\s+profile\s+(?:named|called)?\s*([a-z0-9_\-]+)(?=\s+(?:on|for|using)\b|$)/i.exec(request)?.[1];
  const profileFromSurfaceName = /(?:profile)\s+(?:named|called)?\s*([a-z0-9_\-]+)/i.exec(request)?.[1];
  const normalizedRequest = normalize(request);
  const resolvedQuotedName = quotedValue ?? undefined;
  const genericObjectName = namedSurface ?? namedAlignment ?? namedCorridor ?? resolvedQuotedName;

  return {
    name: createSurfaceFromDemName
      ?? createSurfaceName
      ?? extractNamedValue(request, "name")
      ?? genericObjectName,
    alignmentName: alignmentName ?? namedAlignment ?? (normalizedRequest.includes("alignment") ? resolvedQuotedName : undefined),
    corridorName: namedCorridor ?? extractNamedValue(request, "corridor(?:name)?") ?? undefined,
    profileName: profileName ?? namedProfile ?? createLayoutProfileName ?? profileFromSurfaceName ?? undefined,
    surfaceName: surfaceName ?? namedSurface ?? (normalizedRequest.includes("surface") ? resolvedQuotedName : undefined),
    baseSurface: baseSurface ?? undefined,
    comparisonSurface: comparisonSurface ?? undefined,
    style: style ?? undefined,
    layer: layer ?? undefined,
    labelSet: labelSet ?? undefined,
    filePath: filePath ?? undefined,
    outputPath: outputPath ?? undefined,
    gridSpacing: gridSpacing ?? undefined,
    designSpeed: typeof designSpeed === "string" ? Number(designSpeed) : designSpeed,
    inflow: inflow ?? undefined,
    outflow: outflow ?? undefined,
    bottomElevation: bottomElevation ?? undefined,
    topElevation: topElevation ?? undefined,
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

  if (entry.domain === "sight_distance" && normalizedRequest.includes("sight distance")) {
    score += 2;
  }

  if (entry.domain === "detention" && normalizedRequest.includes("detention")) {
    score += 2;
  }

  if (entry.domain === "slope_analysis" && normalizedRequest.includes("slope")) {
    score += 2;
  }

  if (entry.domain === "cost_estimation" && (normalizedRequest.includes("cost") || normalizedRequest.includes("pay item") || normalizedRequest.includes("estimate"))) {
    score += 2;
  }

  if (entry.intent === "estimate_material_cost" && (normalizedRequest.includes("material cost") || normalizedRequest.includes("construction cost") || normalizedRequest.includes("cost estimate"))) {
    score += 3;
  }

  if (entry.intent === "export_pay_items" && normalizedRequest.includes("pay item")) {
    score += 2;
  }

  if (entry.domain === "docs" && normalizedRequest.includes("tool")) {
    score += 1;
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
    return !hasRequiredValue(value);
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
