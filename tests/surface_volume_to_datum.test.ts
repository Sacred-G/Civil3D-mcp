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

import { SURFACE_DOMAIN_DEFINITION } from "../src/tools/domains/surfaceDomain.js";

const square = [
  { x: 0, y: 0 },
  { x: 100, y: 0 },
  { x: 100, y: 100 },
  { x: 0, y: 100 },
];

describe("surface domain volume_to_datum", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
    sendCommandMock.mockResolvedValue({ name: "EG", datum: 750, cutVolume: 0, fillVolume: 1200, netVolume: 1200 });
  });

  it("routes to calculateSurfaceVolumeToDatum", async () => {
    const result = await SURFACE_DOMAIN_DEFINITION.actions.volume_to_datum.execute({
      action: "volume_to_datum",
      name: "EG",
      datum: 750,
      boundary: square,
    });

    expect(sendCommandMock).toHaveBeenCalledWith("calculateSurfaceVolumeToDatum", {
      name: "EG",
      datum: 750,
      boundary: square,
    });
    expect(result).toMatchObject({ fillVolume: 1200 });
  });

  it("requires a datum and a boundary of at least three points", () => {
    const schema = SURFACE_DOMAIN_DEFINITION.actions.volume_to_datum.inputSchema;
    expect(schema.safeParse({ action: "volume_to_datum", name: "EG", datum: 750, boundary: square }).success).toBe(true);
    expect(schema.safeParse({ action: "volume_to_datum", name: "EG", boundary: square }).success).toBe(false);
    expect(schema.safeParse({ action: "volume_to_datum", name: "EG", datum: 750, boundary: square.slice(0, 2) }).success).toBe(false);
  });

  it("is listed on the civil3d_surface tool", () => {
    const tool = SURFACE_DOMAIN_DEFINITION.exposures.find((entry) => entry.toolName === "civil3d_surface");
    expect(tool?.supportedActions).toContain("volume_to_datum");
  });
});
