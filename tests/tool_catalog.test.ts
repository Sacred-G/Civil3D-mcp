import { describe, it, expect } from "vitest";
import { TOOL_CATALOG, listToolCatalog, listDomains } from "../src/tools/tool_catalog.js";

describe("Tool Catalog", () => {
  it("should have entries for all registered tools", () => {
    expect(TOOL_CATALOG.length).toBeGreaterThan(0);
  });

  it("should have unique tool names", () => {
    const names = TOOL_CATALOG.map((e) => e.toolName);
    const unique = new Set(names);
    expect(unique.size).toBe(names.length);
  });

  it("every entry should have required fields", () => {
    for (const entry of TOOL_CATALOG) {
      expect(entry.toolName).toBeTruthy();
      expect(entry.displayName).toBeTruthy();
      expect(entry.description).toBeTruthy();
      expect(entry.domain).toBeTruthy();
      expect(entry.capabilities.length).toBeGreaterThan(0);
      expect(typeof entry.requiresActiveDrawing).toBe("boolean");
      expect(typeof entry.safeForRetry).toBe("boolean");
      expect(["implemented", "planned"]).toContain(entry.status);
    }
  });

  it("listToolCatalog returns the full catalog", () => {
    expect(listToolCatalog()).toBe(TOOL_CATALOG);
  });

  it("listDomains returns sorted unique domains", () => {
    const domains = listDomains();
    expect(domains.length).toBeGreaterThan(0);
    // Verify sorted
    const sorted = [...domains].sort();
    expect(domains).toEqual(sorted);
    // Verify unique
    expect(new Set(domains).size).toBe(domains.length);
  });

  it("hydrology tool catalog entry includes new operations", () => {
    const hydrology = TOOL_CATALOG.find((e) => e.toolName === "civil3d_hydrology");
    expect(hydrology).toBeDefined();
    expect(hydrology!.domain).toBe("hydrology");
    expect(hydrology!.status).toBe("implemented");
  });
});
