import { readFileSync } from "node:fs";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { sendCommandMock } = vi.hoisted(() => ({
  sendCommandMock: vi.fn(),
}));

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async <T>(
    operation: (client: { sendCommand: typeof sendCommandMock }) => Promise<T>,
  ) => await operation({
    sendCommand: sendCommandMock,
  }),
}));

import { ALIGNMENT_DOMAIN_DEFINITION } from "../src/tools/domains/alignmentDomain.js";

const points = [
  { x: 0, y: 0 },
  { x: 500, y: 0 },
  { x: 900, y: 300 },
  { x: 1400, y: 300 },
];

describe("alignment create radii", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
    sendCommandMock.mockResolvedValue({ name: "CL", curvesAdded: 1, curvesSkipped: 1, created: true });
  });

  it("passes per-PI radii to the plugin", async () => {
    const create = ALIGNMENT_DOMAIN_DEFINITION.actions.create;
    const args = create.inputSchema.parse({ action: "create", name: "CL", points, radii: [400, null] });
    await create.execute(args);

    expect(sendCommandMock).toHaveBeenCalledWith("createAlignment", expect.objectContaining({
      name: "CL",
      points,
      radii: [400, null],
      curveRadius: undefined,
    }));
  });

  it("rejects radii together with curveRadius, and empty or non-positive radii", () => {
    const schema = ALIGNMENT_DOMAIN_DEFINITION.actions.create.inputSchema;
    expect(schema.safeParse({ action: "create", name: "CL", points, radii: [400, 300] }).success).toBe(true);
    expect(schema.safeParse({ action: "create", name: "CL", points, radii: [400], curveRadius: 300 }).success).toBe(false);
    expect(schema.safeParse({ action: "create", name: "CL", points, radii: [] }).success).toBe(false);
    expect(schema.safeParse({ action: "create", name: "CL", points, radii: [0] }).success).toBe(false);
    expect(schema.safeParse({ action: "create", name: "CL", points, radii: [-50] }).success).toBe(false);
  });
});

describe("alignment create curve reporting", () => {
  const normalize = (text: string) => text.replace(/\s+/g, " ");
  const source = normalize(readFileSync(new URL("../Civil3D-MCP-Plugin/AlignmentCommands.cs", import.meta.url), "utf8"));

  it("counts curves Civil 3D fitted itself in curvesAdded and warns when the default fitted them", () => {
    expect(source).toContain('["curvesAdded"] = curvesAutoFitted + curvesAdded');
    expect(source).toContain('["curvesAutoFitted"] = curvesAutoFitted');
    expect(source).toContain('["curveCount"] = CountArcs(alignment)');
    expect(source).toMatch(/if \(curvesAutoFitted > 0 && addCurvesGiven == null\)/);
  });

  it("does not count arcs that were already in a traced polyline", () => {
    expect(source).toContain("source.GetBulgeAt(segment) != 0");
    expect(source).toMatch(/CountArcs\([^;]*\) - sourceArcs/);
  });

  it("documents the addCurves default on the canonical tool", () => {
    const canonical = ALIGNMENT_DOMAIN_DEFINITION.exposures.find((item) => item.toolName === "civil3d_alignment");
    expect(canonical!.inputShape.addCurves.description).toMatch(/Defaults to true for points/);
  });
});
