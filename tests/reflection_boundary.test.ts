import { readFileSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";

const pluginDirectory = fileURLToPath(new URL("../Civil3D-MCP-Plugin/", import.meta.url));
const directReflectionPattern = /System\.Reflection|BindingFlags|\bType\.GetType|\.GetType\(\)\.Get(?:Property|Properties|Method|Methods|Field|Fields)|typeof\([^)]*\)\.Get(?:Method|Methods|Property)|AppDomain\.CurrentDomain\.GetAssemblies/;

function countDirectReflectionLines(source: string): number {
  return source.split(/\r?\n/).filter((line) => directReflectionPattern.test(line)).length;
}

describe("Civil 3D reflection boundary", () => {
  it("prohibits direct reflection outside the centralized compatibility boundary", () => {
    const violations: string[] = [];
    for (const fileName of readdirSync(pluginDirectory).filter((name) => name.endsWith(".cs"))) {
      if (fileName === "Civil3DCompatibility.cs") continue;
      // runCode loads the compiler and each compiled script (assembly load
      // contexts); it never reflects over Civil 3D APIs.
      if (fileName === "RunCodeAssemblies.cs") continue;
      const source = readFileSync(`${pluginDirectory}/${fileName}`, "utf8");
      const count = countDirectReflectionLines(source);
      if (count > 0) {
        violations.push(`${fileName}: ${count} direct reflection markers`);
      }
    }

    expect(violations).toEqual([]);
  });

  it("keeps the common object utility free of direct reflection", () => {
    const source = readFileSync(`${pluginDirectory}/CivilObjectUtils.cs`, "utf8");
    expect(countDirectReflectionLines(source)).toBe(0);
  });
});
