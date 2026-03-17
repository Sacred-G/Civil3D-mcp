import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import * as net from "net";
import { ApplicationClientConnection } from "../src/utils/SocketClient.js";

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
      await new Promise<void>((resolve) => {
        server.listen(0, "localhost", () => {
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
            error: { code: -32600, message: "Surface not found" },
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

      await expect(
        connClient.sendCommand("getSurface", { name: "NonExistent" })
      ).rejects.toThrow("Surface not found");
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
  });
});
