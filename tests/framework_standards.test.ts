import { describe, it, expect } from "vitest";
import { lookupFrameworkStandards } from "../src/standards/FrameworkStandardsService.js";

describe("Framework standards lookup", () => {
  it("returns template governance guidance for production style changes", async () => {
    const result = await lookupFrameworkStandards({
      query: "Never edit a style in a production drawing",
      topic: "templates",
      maxResults: 5,
    });

    expect(result.totalRulesLoaded).toBeGreaterThan(0);
    expect(result.matchedCount).toBeGreaterThan(0);
    expect(result.matches.some((match) => match.rule.toLowerCase().includes("production drawing"))).toBe(true);
  });

  it("returns label guidance for label-focused lookups", async () => {
    const result = await lookupFrameworkStandards({
      query: "profile labels proposed existing textstyle",
      topic: "labels",
      maxResults: 5,
    });

    expect(result.matchedCount).toBeGreaterThan(0);
    expect(result.matches.some((match) => match.tags.includes("labels") || match.rule.toLowerCase().includes("label"))).toBe(true);
  });
});
