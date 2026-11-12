import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import * as net from "net";
import { ApplicationClientConnection, Civil3DRpcError, pluginConnectHost } from "../src/utils/SocketClient.js";

describe("ApplicationClientConnection", () => {
  let client: ApplicationClientConnection;

  beforeEach(() => {
    client = new ApplicationClientConnection("localhost", 9999);
  });

  afterEach(() => {
    try {
      client.disconnect();
    } catch {
      // ignore cleanup errors
    }
  });

  it("dials localhost (any case) and an empty host as 127.0.0.1, other hosts unchanged", () => {
    expect(pluginConnectHost("localhost")).toBe("127.0.0.1");
    expect(pluginConnectHost(" LocalHost ")).toBe("127.0.0.1");
    expect(pluginConnectHost("")).toBe("127.0.0.1");
    expect(pluginConnectHost("127.0.0.1")).toBe("127.0.0.1");
    expect(pluginConnectHost("::1")).toBe("::1");
    expect(pluginConnectHost("host.docker.internal")).toBe("host.docker.internal");
  });

  it("should initialize with correct host and port", () => {
    expect(client.host).toBe("localhost");
    expect(client.port).toBe(9999);
    expect(client.isConnected).toBe(false);
  });

  it("should have an empty response callbacks map initially", () => {
    expect(client.responseCallbacks.size).toBe(0);
  });

  it("should have an empty buffer initially", () => {
    expect(client.buffer).toBe("");
  });

  describe("with a mock TCP server", () => {
    let server: net.Server;
    let serverPort: number;

    beforeEach(async () => {
      server = net.createServer();
      // IPv4 loopback only, like the plugin; the clients below dial "localhost".
      await new Promise<void>((resolve) => {
        server.listen(0, "127.0.0.1", () => {
          const addr = server.address() as net.AddressInfo;
          serverPort = addr.port;
          resolve();
        });
      });
    });

    afterEach(async () => {
      client.disconnect();
      await new Promise<void>((resolve) => {
        server.close(() => resolve());
      });
    });

    it("should connect to a server", async () => {
      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });
      expect(connClient.isConnected).toBe(true);
      connClient.disconnect();
    });

    it("should send a JSON-RPC command and receive a response", async () => {
      // Set up server to echo back a successful response
      server.on("connection", (socket) => {
        socket.on("data", (data) => {
          const request = JSON.parse(data.toString());
          const response = {
            jsonrpc: "2.0",
            id: request.id,
            result: { status: "ok", name: request.params.name },
          };
          socket.write(JSON.stringify(response));
        });
      });

      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      const result = await connClient.sendCommand("testMethod", { name: "TestSurface" });
      expect(result).toEqual({ status: "ok", name: "TestSurface" });
      connClient.disconnect();
    });

    it("should reject on JSON-RPC error response", async () => {
      server.on("connection", (socket) => {
        socket.on("data", (data) => {
          const request = JSON.parse(data.toString());
          const response = {
            jsonrpc: "2.0",
            id: request.id,
            error: {
              code: -32004,
              message: "Surface not found",
              data: { code: "CIVIL3D.OBJECT_NOT_FOUND" },
            },
          };
          socket.write(JSON.stringify(response));
        });
      });

      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      const error = await connClient.sendCommand("getSurface", { name: "NonExistent" })
        .then(() => undefined, (reason) => reason);
      expect(error).toBeInstanceOf(Civil3DRpcError);
      expect(error).toMatchObject({
        message: "Surface not found",
        code: "CIVIL3D.OBJECT_NOT_FOUND",
        rpcCode: -32004,
      });
      connClient.disconnect();
    });

    it("should handle chunked responses", async () => {
      server.on("connection", (socket) => {
        socket.on("data", (data) => {
          const request = JSON.parse(data.toString());
          const response = JSON.stringify({
            jsonrpc: "2.0",
            id: request.id,
            result: { elevation: 123.45 },
          });
          // Send in two chunks
          const mid = Math.floor(response.length / 2);
          socket.write(response.substring(0, mid));
          setTimeout(() => {
            socket.write(response.substring(mid));
          }, 50);
        });
      });

      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      const result = await connClient.sendCommand("getSurfaceElevation", { x: 100, y: 200 });
      expect(result).toEqual({ elevation: 123.45 });
      connClient.disconnect();
    });

    it("rejects malformed JSON-RPC error envelopes", async () => {
      server.on("connection", (socket) => {
        socket.on("data", (data) => {
          const request = JSON.parse(data.toString());
          socket.write(JSON.stringify({
            jsonrpc: "2.0",
            id: request.id,
            error: { code: "CIVIL3D.OBJECT_NOT_FOUND", message: "Wrong code type" },
          }));
        });
      });

      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      await expect(connClient.sendCommand("getSurface", { name: "Missing" }))
        .rejects.toThrow("Invalid JSON-RPC 2.0 error response");
      connClient.disconnect();
    });

    it("should reject pending commands when the plugin connection closes", async () => {
      server.on("connection", (socket) => {
        socket.once("data", () => socket.destroy());
      });

      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", () => resolve());
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      await expect(connClient.sendCommand("interruptedCommand", {})).rejects.toThrow(
        "connection closed",
      );
      expect(connClient.responseCallbacks.size).toBe(0);
      connClient.disconnect();
    });

    it("rejects pending commands with the stable cancellation code", async () => {
      const requestReceived = new Promise<void>((resolve) => {
        server.on("connection", (socket) => socket.once("data", () => resolve()));
      });
      const connClient = new ApplicationClientConnection("localhost", serverPort);
      await new Promise<void>((resolve, reject) => {
        connClient.socket.on("connect", resolve);
        connClient.socket.on("error", reject);
        connClient.connect();
      });

      const pending = connClient.sendCommand("cancelMe", {});
      await requestReceived;
      connClient.cancel();

      await expect(pending).rejects.toMatchObject({
        code: "CIVIL3D.CANCELLED",
        rpcCode: -32010,
      });
    });
  });
});
