import { afterEach, describe, expect, it, vi } from "vitest";
import { EventEmitter } from "node:events";
import * as net from "node:net";

afterEach(() => {
  vi.doUnmock("../src/utils/SocketClient.js");
  vi.unstubAllEnvs();
  vi.resetModules();
});

describe("withApplicationConnection cancellation", () => {
  it("settles a request aborted while the connection is still being set up", async () => {
    // A connection that never connects: neither "connect" nor "error" fires.
    vi.doMock("../src/utils/SocketClient.js", async (importOriginal) => {
      const actual = await importOriginal<typeof import("../src/utils/SocketClient.js")>();
      class NeverConnects {
        socket = new EventEmitter();
        isConnected = false;
        connect() { return true; }
        cancel() { this.isConnected = false; }
        disconnect() {}
        async sendCommand() { throw new Error("not connected"); }
      }
      return { ...actual, ApplicationClientConnection: NeverConnects };
    });
    vi.stubEnv("CIVIL3D_CONNECT_TIMEOUT", "60000");
    vi.resetModules();

    const { withApplicationConnection } = await import("../src/utils/ConnectionManager.js");
    const { createRequestId, runWithRequestId } = await import("../src/utils/requestContext.js");
    const abort = new AbortController();
    const startedAt = Date.now();
    const pending = runWithRequestId(
      createRequestId(),
      () => withApplicationConnection(async (client) => client.sendCommand("getDrawingInfo", {})),
      abort.signal,
    );
    setTimeout(() => abort.abort(), 20);

    await expect(pending).rejects.toMatchObject({ code: "CIVIL3D.CANCELLED" });
    expect(Date.now() - startedAt).toBeLessThan(2000);
  });
});

describe("withApplicationConnection", () => {
  it("uses a fresh TCP connection for every command in a composite action", async () => {
    let connectionCount = 0;
    const server = net.createServer((socket) => {
      connectionCount += 1;
      socket.once("data", (data) => {
        const request = JSON.parse(data.toString()) as { id: string; method: string };
        socket.end(JSON.stringify({
          jsonrpc: "2.0",
          id: request.id,
          result: { method: request.method },
        }));
      });
    });

    await new Promise<void>((resolve, reject) => {
      server.once("error", reject);
      server.listen(0, "127.0.0.1", resolve);
    });

    try {
      const address = server.address() as net.AddressInfo;
      vi.stubEnv("CIVIL3D_HOST", "127.0.0.1");
      vi.stubEnv("CIVIL3D_PORT", String(address.port));
      vi.resetModules();

      const { withApplicationConnection } = await import("../src/utils/ConnectionManager.js");
      const results = await withApplicationConnection(async (client) => [
        await client.sendCommand("first", {}),
        await client.sendCommand("second", {}),
      ]);

      expect(results).toEqual([{ method: "first" }, { method: "second" }]);
      expect(connectionCount).toBe(2);
    } finally {
      await new Promise<void>((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
      });
    }
  });
});
