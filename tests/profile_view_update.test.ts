import { readFileSync } from "node:fs";
import { beforeEach, describe, expect, it, vi } from "vitest";

const sendCommand = vi.fn();

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async (fn: (client: { sendCommand: typeof sendCommand }) => unknown) => await fn({ sendCommand }),
}));

const {
  PROFILE_DOMAIN_DEFINITION,
  ProfileViewCreateArgsSchema,
  ProfileViewUpdateArgsSchema,
} = await import("../src/tools/domains/profileDomain.js");

const update = { action: "view_update", profileViewName: "RW-R1 Wall Profile" };
const create = { action: "view_create", alignmentName: "RW-R1", profileViewName: "PV", insertX: 0, insertY: 0 };

describe("civil3d_profile view_update schema", () => {
  beforeEach(() => {
    sendCommand.mockReset().mockResolvedValue({ success: true });
  });

  it("accepts ranges, auto ranges, a style and vertical exaggeration", () => {
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, stationStart: -15, stationEnd: 422.15 }).success).toBe(true);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, elevationMin: 105, elevationMax: 155 }).success).toBe(true);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, stationRange: "auto", elevationRange: "auto" }).success).toBe(true);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, style: "Profile View", verticalExaggeration: 5 }).success).toBe(true);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, stationEnd: 300 }).success).toBe(true);
  });

  it("rejects empty or contradictory ranges, a non-positive exaggeration and an update with nothing to change", () => {
    expect(ProfileViewUpdateArgsSchema.safeParse(update).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, stationStart: 100, stationEnd: 100 }).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, elevationMin: 155, elevationMax: 105 }).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, stationRange: "auto", stationStart: 0 }).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, elevationRange: "auto", elevationMax: 150 }).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...update, verticalExaggeration: 0 }).success).toBe(false);
  });

  it("reports a contradictory range through the MCP tool as CIVIL3D.INVALID_INPUT, without calling the plugin", async () => {
    const { captureDomainToolHandlers } = await import("../src/tools/domainRuntime.js");
    const { executeRegisteredTool } = await import("../src/tools/toolHandlerRegistry.js");
    captureDomainToolHandlers(PROFILE_DOMAIN_DEFINITION);
    for (const args of [
      { ...update, stationStart: 100, stationEnd: 50 },
      { ...update, elevationMin: 155, elevationMax: 105 },
      update,
    ]) {
      const error = await executeRegisteredTool("civil3d_profile", args).then(() => undefined, (reason) => reason);
      expect(error, JSON.stringify(args)).toMatchObject({ code: "CIVIL3D.INVALID_INPUT", rpcCode: -32602 });
    }
    expect(sendCommand).not.toHaveBeenCalled();
  });

  it("forwards every setting to profileViewUpdate", async () => {
    const args = { ...update, style: "Profile View", layer: "C-PROF-VIEW", stationStart: -15, stationEnd: 422.15,
      elevationMin: 105, elevationMax: 155, verticalExaggeration: 5 };
    await PROFILE_DOMAIN_DEFINITION.actions.view_update.execute(ProfileViewUpdateArgsSchema.parse(args) as never);
    expect(sendCommand).toHaveBeenCalledWith("profileViewUpdate", {
      profileViewName: "RW-R1 Wall Profile",
      style: "Profile View",
      layer: "C-PROF-VIEW",
      stationStart: -15,
      stationEnd: 422.15,
      stationRange: undefined,
      elevationMin: 105,
      elevationMax: 155,
      elevationRange: undefined,
      verticalExaggeration: 5,
    });
  });

  it("is exposed on civil3d_profile and as civil3d_profile_view_update", () => {
    const canonical = PROFILE_DOMAIN_DEFINITION.exposures.find((item) => item.toolName === "civil3d_profile");
    expect(canonical!.supportedActions).toContain("view_update");
    expect(canonical!.inputShape).toHaveProperty("verticalExaggeration");
    const standalone = PROFILE_DOMAIN_DEFINITION.exposures.find((item) => item.toolName === "civil3d_profile_view_update");
    expect(standalone).toBeDefined();
    const resolved = standalone!.resolveAction({ profileViewName: "PV", elevationMin: 100, elevationMax: 120 });
    expect(resolved.args).toMatchObject({ action: "view_update", profileViewName: "PV", elevationMin: 100, elevationMax: 120 });
  });
});

describe("civil3d_profile view_create ranges", () => {
  beforeEach(() => {
    sendCommand.mockReset().mockResolvedValue({ success: true });
  });

  it("validates and forwards ranges and vertical exaggeration", async () => {
    expect(ProfileViewCreateArgsSchema.safeParse({ ...create, elevationMin: 120, elevationMax: 110 }).success).toBe(false);
    const args = ProfileViewCreateArgsSchema.parse({ ...create, stationStart: -15, stationEnd: 422.15, elevationMin: 105, elevationMax: 155, verticalExaggeration: 5 });
    await PROFILE_DOMAIN_DEFINITION.actions.view_create.execute(args as never);
    expect(sendCommand).toHaveBeenCalledWith("profileViewCreate", expect.objectContaining({
      stationStart: -15, stationEnd: 422.15, elevationMin: 105, elevationMax: 155, verticalExaggeration: 5,
    }));
  });

  it("maps ranges through the standalone civil3d_profile_view_create tool", () => {
    const exposure = PROFILE_DOMAIN_DEFINITION.exposures.find((item) => item.toolName === "civil3d_profile_view_create");
    expect(exposure!.inputShape).toHaveProperty("elevationMax");
    expect(exposure!.resolveAction({ ...create, verticalExaggeration: 10 }).args).toMatchObject({ verticalExaggeration: 10 });
  });
});

describe("profileViewUpdate plugin guards", () => {
  const normalize = (text: string) => text.replace(/\s+/g, " ");
  const source = normalize(readFileSync(new URL("../Civil3D-MCP-Plugin/ProfileEditCommands.cs", import.meta.url), "utf8"));
  const dispatcher = normalize(readFileSync(new URL("../Civil3D-MCP-Plugin/CommandDispatcher.cs", import.meta.url), "utf8"));

  it("is dispatched and runs as a write", () => {
    expect(dispatcher).toContain('"profileViewUpdate" => ProfileEditCommands.ProfileViewUpdateAsync(parameters)');
    const body = source.slice(source.indexOf("public static Task<object?> ProfileViewUpdateAsync"));
    expect(body).toMatch(/^public static Task<object\?> ProfileViewUpdateAsync[^]*?CivilExecution\.WriteAsync/);
  });

  it("switches ranges to user-specified and reports views that share the exaggerated style", () => {
    expect(source).toContain("profileView.StationRangeMode = StationRangeType.UserSpecified;");
    expect(source).toContain("profileView.ElevationRangeMode = ElevationRangeType.UserSpecified;");
    expect(source).toContain("style.GraphStyle.VerticalExaggeration = exaggeration");
    expect(source).toContain('result["styleSharedWith"] = styleSharedWith;');
  });

  it("applies the same settings when a view is created", () => {
    const createBody = source.slice(
      source.indexOf("public static Task<object?> ProfileViewCreateAsync"),
      source.indexOf("private static bool ApplyToProfileView"));
    expect(createBody).toContain("ProfileViewSettings.Read(parameters, includeStyle: false)");
    expect(createBody).toContain("ApplyViewSettings(civilDoc, transaction, profileView, settings, warnings)");
  });
});

describe("civil3d_style view styles", () => {
  it("lists profile and section view styles and band sets", async () => {
    const { STANDARDS_DOMAIN_DEFINITION } = await import("../src/tools/domains/standardsDomain.js");
    const schema = STANDARDS_DOMAIN_DEFINITION.actions.style_list.inputSchema;
    for (const objectType of ["profile_view", "profile_view_band_set", "section_view", "section_view_band_set"]) {
      expect(schema.safeParse({ action: "style_list", objectType }).success, objectType).toBe(true);
    }
    const plugin = readFileSync(new URL("../Civil3D-MCP-Plugin/StyleCommands.cs", import.meta.url), "utf8");
    expect(plugin).toContain('"profile_view" => civilDoc.Styles.ProfileViewStyles');
    expect(plugin).toContain('"profile_view_band_set" => civilDoc.Styles.ProfileViewBandSetStyles');
    expect(plugin).toContain('"section_view" => civilDoc.Styles.SectionViewStyles');
    expect(plugin).toContain('"section_view_band_set" => civilDoc.Styles.SectionViewBandSetStyles');
  });
});

describe("view_update range modes", () => {
  it("takes only 'auto' as a range mode", () => {
    const base = { action: "view_update", profileViewName: "PV" };
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...base, stationRange: "user" }).success).toBe(false);
    expect(ProfileViewUpdateArgsSchema.safeParse({ ...base, elevationRange: "user" }).success).toBe(false);
  });
});
