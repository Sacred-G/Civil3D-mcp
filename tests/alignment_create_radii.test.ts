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
