import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";

describe("Logger", () => {
  const originalEnv = process.env.CIVIL3D_LOG_LEVEL;

  beforeEach(() => {
    vi.spyOn(console, "error").mockImplementation(() => {});
  });

  afterEach(() => {
    vi.restoreAllMocks();
    if (originalEnv !== undefined) {
      process.env.CIVIL3D_LOG_LEVEL = originalEnv;
    } else {
      delete process.env.CIVIL3D_LOG_LEVEL;
    }
  });

  it("should log info messages by default", async () => {
    delete process.env.CIVIL3D_LOG_LEVEL;
    // Re-import to pick up env change
    const { createLogger } = await import("../src/utils/logger.js");
    const log = createLogger("Test");
    log.info("hello");
    expect(console.error).toHaveBeenCalledTimes(1);
    const msg = (console.error as any).mock.calls[0][0];
    expect(msg).toContain("[INFO]");
    expect(msg).toContain("[Test]");
    expect(msg).toContain("hello");
  });

  it("should include data in log output", async () => {
    const { createLogger } = await import("../src/utils/logger.js");
    const log = createLogger("Test");
    log.info("with data", { key: "value" });
    const msg = (console.error as any).mock.calls[0][0];
    expect(msg).toContain('"key":"value"');
  });

  it("should include ISO timestamp", async () => {
    const { createLogger } = await import("../src/utils/logger.js");
    const log = createLogger("Test");
    log.info("timestamped");
    const msg = (console.error as any).mock.calls[0][0];
    // ISO 8601 pattern
    expect(msg).toMatch(/\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/);
  });

  it("should log errors regardless of level", async () => {
    const { createLogger } = await import("../src/utils/logger.js");
    const log = createLogger("Test");
    log.error("something broke");
    expect(console.error).toHaveBeenCalled();
    const msg = (console.error as any).mock.calls[0][0];
    expect(msg).toContain("[ERROR]");
  });
});
