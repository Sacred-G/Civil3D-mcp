import { GENERATED_TOOL_CATALOG_ENTRIES } from "./toolManifest.js";
import type { ToolCatalogEntry } from "./toolMetadata.js";

export const TOOL_CATALOG: ToolCatalogEntry[] = [...GENERATED_TOOL_CATALOG_ENTRIES];

export function listToolCatalog() {
  return TOOL_CATALOG;
}

export function listDomains() {
  return [...new Set(TOOL_CATALOG.map((entry) => entry.domain))].sort();
}
