import { mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { pluginTokenPath, readPluginToken } from "../src/utils/pluginAuth.js";

describe("readPluginToken", () => {
  let dir: string;

  beforeEach(() => {
    dir = mkdtempSync(join(tmpdir(), "c3d-plugin-auth-"));
  });

  afterEach(() => {
    vi.unstubAllEnvs();
    rmSync(dir, { recursive: true, force: true });
  });

  it("reads and trims the token from CIVIL3D_RPC_TOKEN_FILE", () => {
    const file = join(dir, "rpc-token");
    writeFileSync(file, "  0123456789ABCDEF\r\n");
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", file);

    expect(readPluginToken()).toBe("0123456789ABCDEF");
  });

  it("reads the file again on every call, because Civil 3D writes a new token on each start", () => {
    const file = join(dir, "rpc-token");
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", file);
    writeFileSync(file, "first");
    expect(readPluginToken()).toBe("first");
    writeFileSync(file, "second");
    expect(readPluginToken()).toBe("second");
  });

  it("returns undefined when the file is missing or empty", () => {
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", join(dir, "missing"));
    expect(readPluginToken()).toBeUndefined();

    const empty = join(dir, "empty");
    writeFileSync(empty, " \n");
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", empty);
    expect(readPluginToken()).toBeUndefined();
  });

  it("defaults to the plugin's fixed path under LOCALAPPDATA", () => {
    vi.stubEnv("CIVIL3D_RPC_TOKEN_FILE", undefined);
    vi.stubEnv("LOCALAPPDATA", dir);

    expect(pluginTokenPath()).toBe(join(dir, "Civil3DMcpPlugin", "rpc-token"));
  });
});
