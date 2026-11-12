import { afterEach, describe, expect, it, vi } from "vitest";
import * as net from "node:net";

afterEach(() => {
  vi.unstubAllEnvs();
  vi.resetModules();
});

describe("ApplicationClientConnection response limits", () => {
  it("rejects an oversized plugin response before buffering it indefinitely", async () => {
    const server = net.createServer((socket) => {
      socket.once("data", () => {
        socket.write(JSON.stringify({
          jsonrpc: "2.0",
          id: "oversized-response",
          result: { report: "x".repeat(512) },
        }));
      });
    });

    await new Promise<void>((resolve, reject) => {
      server.once("error", reject);
      server.listen(0, "127.0.0.1", resolve);
    });

    try {
      const address = server.address() as net.AddressInfo;
      vi.stubEnv("CIVIL3D_MAX_RESPONSE_BYTES", "64");
      vi.resetModules();

      const { ApplicationClientConnection } = await import("../src/utils/SocketClient.js");
      const client = new ApplicationClientConnection("127.0.0.1", address.port);

      await new Promise<void>((resolve, reject) => {
        client.socket.once("connect", resolve);
        client.socket.once("error", reject);
        client.connect();
      });

      await expect(client.sendCommand("getLargeReport", {})).rejects.toThrow(
        "response exceeds 64 bytes",
      );
      expect(client.responseCallbacks.size).toBe(0);
    } finally {
      await new Promise<void>((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
      });
    }
  });
});

describe("ApplicationClientConnection request limits", () => {
  async function listen(onConnection: (socket: net.Socket) => void): Promise<net.Server> {
    const server = net.createServer(onConnection);
    await new Promise<void>((resolve, reject) => {
      server.once("error", reject);
      server.listen(0, "127.0.0.1", resolve);
    });
    return server;
  }

  async function connectedClient(server: net.Server) {
    const { ApplicationClientConnection } = await import("../src/utils/SocketClient.js");
    const client = new ApplicationClientConnection("127.0.0.1", (server.address() as net.AddressInfo).port);
    await new Promise<void>((resolve, reject) => {
      client.socket.once("connect", resolve);
      client.socket.once("error", reject);
      client.connect();
    });
    return client;
  }

  const closeServer = (server: net.Server) => new Promise<void>((resolve, reject) => {
    server.close((error) => error ? reject(error) : resolve());
  });

  it("refuses a request over the plugin's 1 MiB limit before sending anything", async () => {
    let received = 0;
    let serverSawEnd!: () => void;
    const ended = new Promise<void>((resolve) => { serverSawEnd = resolve; });
    const server = await listen((socket) => {
      socket.on("data", (data) => { received += data.length; });
      socket.on("end", () => serverSawEnd());
    });

    try {
      const { MAX_REQUEST_BYTES } = await import("../src/utils/SocketClient.js");
      expect(MAX_REQUEST_BYTES).toBe(1_048_576);
      const client = await connectedClient(server);

      const error = await client.sendCommand("runCode", { blob: "x".repeat(1_100_000) }).catch((e) => e);
      expect(error.code).toBe("CIVIL3D.REQUEST_TOO_LARGE");
      expect(error.message).toMatch(/at most 1048576 bytes/);
      expect(client.responseCallbacks.size).toBe(0);

      client.disconnect();
      await ended;
      expect(received).toBe(0);
    } finally {
      await closeServer(server);
    }
  });

  it("applies a per-command timeout and keeps the default for other commands", async () => {
    // Reads the request and never answers.
    const server = await listen((socket) => socket.resume());

    try {
      vi.stubEnv("CIVIL3D_COMMAND_TIMEOUT", undefined);
      vi.resetModules();
      const { COMMAND_TIMEOUT_MS } = await import("../src/utils/SocketClient.js");
      expect(COMMAND_TIMEOUT_MS).toBe(120_000);

      const client = await connectedClient(server);
      const error = await client.sendCommand("slowCommand", {}, { timeoutMs: 200 }).catch((e) => e);
      expect(error.message).toBe("Command timed out after 200ms: slowCommand");
      expect(error.code).toBe("CIVIL3D.TIMEOUT");
      expect(client.responseCallbacks.size).toBe(0);
      client.disconnect();
    } finally {
      await closeServer(server);
    }
  });
});
