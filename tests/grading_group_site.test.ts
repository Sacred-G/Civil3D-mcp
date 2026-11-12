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

import { GRADING_DOMAIN_DEFINITION } from "../src/tools/domains/gradingDomain.js";

describe("grading group_create site", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
    sendCommandMock.mockResolvedValue({ name: "Pond", site: "Pond Site", siteCreated: true, created: true });
  });

  it("passes site to createGradingGroup when given", async () => {
    await GRADING_DOMAIN_DEFINITION.actions.group_create.execute({
      action: "group_create",
      name: "Pond",
      site: "Grading",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("createGradingGroup", {
      name: "Pond",
      description: "",
      useProjection: false,
      site: "Grading",
    });
  });

  it("sends no site when none is given", async () => {
    await GRADING_DOMAIN_DEFINITION.actions.group_create.execute({ action: "group_create", name: "Pond" });

    expect(sendCommandMock).toHaveBeenCalledWith("createGradingGroup", {
      name: "Pond",
      description: "",
      useProjection: false,
    });
  });

  it("rejects an empty site name", () => {
    const schema = GRADING_DOMAIN_DEFINITION.actions.group_create.inputSchema;
    expect(schema.safeParse({ action: "group_create", name: "Pond", site: "Grading" }).success).toBe(true);
    expect(schema.safeParse({ action: "group_create", name: "Pond", site: "" }).success).toBe(false);
  });

  it("exposes site on the canonical tool and the group_create alias", () => {
    const canonical = GRADING_DOMAIN_DEFINITION.exposures.find((entry) => entry.toolName === "civil3d_grading");
    const alias = GRADING_DOMAIN_DEFINITION.exposures.find((entry) => entry.toolName === "civil3d_grading_group_create");
    expect(canonical?.inputShape).toHaveProperty("site");
    expect(alias?.inputShape).toHaveProperty("site");
    expect(alias?.resolveAction({ name: "Pond", site: "Grading" }).args).toMatchObject({ site: "Grading" });
  });

  it("finds or creates the site in the plugin instead of failing on a drawing with none", () => {
    const source = readFileSync(new URL("../Civil3D-MCP-Plugin/GradingCommands.cs", import.meta.url), "utf8");
    expect(source).toContain("Site.Create(civilDoc, siteParam)");
    expect(source).toContain('Site.Create(civilDoc, name + " Site")');
    expect(source).not.toContain("No site found in the drawing");
    expect(source).toContain('["siteCreated"] = siteCreated');
  });
});
