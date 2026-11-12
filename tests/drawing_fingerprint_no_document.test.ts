import { afterEach, describe, expect, it, vi } from "vitest";
import * as net from "node:net";

afterEach(() => {
  vi.unstubAllEnvs();
  vi.resetModules();
});

// Simulates a plugin with Civil 3D running but zero documents open. The
// ungated health endpoint answers; getDrawingInfo is never answered, which is
// how older plugin builds behave in that state (the request wedges the host
// execution gate and Node only finds out at CIVIL3D_COMMAND_TIMEOUT).
function createNoDocumentPluginServer(receivedMethods: string[]): net.Server {
  return net.createServer((socket) => {
    socket.once("data", (data) => {
      const request = JSON.parse(data.toString()) as { id: string; method: string };
      receivedMethods.push(request.method);
      if (request.method === "getCivil3DHealth") {
        socket.end(JSON.stringify({
          jsonrpc: "2.0",
          id: request.id,
          result: {
            connected: true,
            drawingLoaded: false,
            operationInProgress: false,
            queueDepth: 0,
          },
        }));
        return;
      }
      // Never answer anything else; leave the socket open like a wedged host.
    });
  });
}

describe("drawing fingerprint with no document open", () => {
  it("fails fast with CIVIL3D.NO_DRAWING without ever calling getDrawingInfo", async () => {
    const receivedMethods: string[] = [];
    const server = createNoDocumentPluginServer(receivedMethods);

    await new Promise<void>((resolve, reject) => {
      server.once("error", reject);
      server.listen(0, "127.0.0.1", resolve);
    });

    try {
      const address = server.address() as net.AddressInfo;
      vi.stubEnv("CIVIL3D_HOST", "127.0.0.1");
      vi.stubEnv("CIVIL3D_PORT", String(address.port));
      vi.stubEnv("CIVIL3D_COMMAND_TIMEOUT", "2000");
      vi.resetModules();

      const { getActiveDrawingFingerprint, ApprovalPolicyService } = await import("../src/tools/approvalPolicy.js");

      const startedAt = Date.now();
      await expect(getActiveDrawingFingerprint()).rejects.toMatchObject({ code: "CIVIL3D.NO_DRAWING" });
      expect(Date.now() - startedAt).toBeLessThan(1000);
      expect(receivedMethods).toEqual(["getCivil3DHealth"]);
      expect(receivedMethods).not.toContain("getDrawingInfo");

      const policy = new ApprovalPolicyService();
      const target = {
        toolName: "civil3d_drawing",
        action: "new",
        capabilities: ["create", "manage"] as const,
        safeForRetry: false,
        requiresActiveDrawing: false,
      };
      const receipt = await policy.requestApproval(target, { action: "new" });
      expect(receipt.drawingFingerprint).toBe("no-active-drawing");
      expect(receivedMethods).not.toContain("getDrawingInfo");
    } finally {
      await new Promise<void>((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
      });
    }
  });
});
