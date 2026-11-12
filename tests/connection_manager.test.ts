import { afterEach, describe, expect, it, vi } from "vitest";
import * as net from "node:net";

afterEach(() => {
  vi.unstubAllEnvs();
  vi.resetModules();
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

  it("passes a per-command timeout through to the connection", async () => {
    // A plugin that reads the request and never answers.
    const server = net.createServer((socket) => socket.resume());

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
      await expect(withApplicationConnection(
        async (client) => await client.sendCommand("runCode", {}, { timeoutMs: 200 }),
      )).rejects.toThrow("Command timed out after 200ms: runCode");
    } finally {
      await new Promise<void>((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
      });
    }
  });
});

describe("plugin host", () => {
  function listen(server: net.Server, port: number, host: string): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      server.once("error", () => resolve(false));
      server.listen(port, host, () => resolve(true));
    });
  }

  function close(server: net.Server): Promise<void> {
    return new Promise<void>((resolve) => server.close(() => resolve()));
  }

  // The plugin binds IPv4 loopback only. Whoever holds [::1] at the plugin's
  // port must never receive a request: it would carry the runCode token.
  it.for([undefined, "localhost", "LocalHost", " LOCALHOST "])(
    "dials CIVIL3D_HOST=%j at 127.0.0.1, never at a listener on [::1]",
    async (host) => {
      let pluginRequests = 0;
      const plugin = net.createServer((socket) => {
        socket.once("data", (data) => {
          pluginRequests += 1;
          const request = JSON.parse(data.toString()) as { id: string };
          socket.end(JSON.stringify({ jsonrpc: "2.0", id: request.id, result: { ok: true } }));
        });
      });
      let decoyConnections = 0;
      const decoy = net.createServer((socket) => {
        decoyConnections += 1;
        socket.destroy();
      });

      expect(await listen(plugin, 0, "127.0.0.1")).toBe(true);
      const port = (plugin.address() as net.AddressInfo).port;
      // Skipped where IPv6 loopback is unavailable or the port is taken there.
      const decoyListening = await listen(decoy, port, "::1");

      try {
        vi.stubEnv("CIVIL3D_HOST", host);
        vi.stubEnv("CIVIL3D_PORT", String(port));
        vi.resetModules();

        const { withApplicationConnection } = await import("../src/utils/ConnectionManager.js");
        const result = await withApplicationConnection(async (client) => await client.sendCommand("runCode", {}));

        expect(result).toEqual({ ok: true });
        expect(pluginRequests).toBe(1);
        if (decoyListening) expect(decoyConnections).toBe(0);
      } finally {
        await close(plugin);
        if (decoyListening) await close(decoy);
      }
    },
  );
});
