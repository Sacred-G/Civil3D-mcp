import { ApplicationClientConnection, Civil3DRpcError, type SendCommandOptions } from "./SocketClient.js";
import { createLogger } from "./logger.js";
import { currentAbortSignal } from "./requestContext.js";

const log = createLogger("ConnectionManager");

// IPv4 loopback, the only address the plugin binds (see pluginConnectHost).
const CIVIL3D_HOST = process.env.CIVIL3D_HOST ?? "127.0.0.1";
const CIVIL3D_PORT = parseInt(process.env.CIVIL3D_PORT ?? "8080", 10);
const CONNECT_TIMEOUT_MS = parseInt(process.env.CIVIL3D_CONNECT_TIMEOUT ?? "5000", 10);

export interface ApplicationCommandClient {
  sendCommand(command: string, params?: unknown, options?: SendCommandOptions): Promise<any>;
}

/**
 * Runs an operation against the Civil 3D plugin. The native transport accepts
 * exactly one JSON-RPC request per TCP connection, so every sendCommand call
 * receives its own short-lived connection. Composite domain actions may safely
 * issue several sequential commands through the client passed to operation.
 */
export async function withApplicationConnection<T>(
  operation: (client: ApplicationCommandClient) => Promise<T>
): Promise<T> {
  const client: ApplicationCommandClient = {
    sendCommand: async (command, params = {}, options = {}) => await sendSingleCommand(command, params, options),
  };

  return await operation(client);
}

async function sendSingleCommand(command: string, params: unknown, options: SendCommandOptions): Promise<any> {
  const appClient = new ApplicationClientConnection(CIVIL3D_HOST, CIVIL3D_PORT);
  const signal = currentAbortSignal();
  const cancellationError = new Civil3DRpcError(
    `Civil 3D command '${command}' was cancelled by the caller.`,
    "CIVIL3D.CANCELLED",
    -32010,
  );
  const cancel = () => appClient.cancel(cancellationError);

  try {
    if (signal?.aborted) throw cancellationError;
    signal?.addEventListener("abort", cancel, { once: true });
    if (!appClient.isConnected) {
      await new Promise<void>((resolve, reject) => {
        let settled = false;

        const cleanup = () => {
          clearTimeout(timeout);
          appClient.socket.removeListener("connect", onConnect);
          appClient.socket.removeListener("error", onError);
          signal?.removeEventListener("abort", onAbort);
        };

        // A caller that aborts while the TCP connection is still being set up
        // is settled at once. cancel() destroys the socket, which raises
        // neither "connect" nor "error", so without this the request would
        // stay alive until the connect timeout.
        const onAbort = () => {
          if (settled) {
            return;
          }
          settled = true;
          cleanup();
          reject(cancellationError);
        };

        const onConnect = () => {
          if (settled) {
            return;
          }
          settled = true;
          cleanup();
          resolve();
        };

        const onError = (error: Error) => {
          if (settled) {
            return;
          }
          settled = true;
          cleanup();
          log.error("Connection failed", {
            host: CIVIL3D_HOST,
            port: CIVIL3D_PORT,
            error: error.message,
          });
          reject(new Error(`Failed to connect to Civil 3D plugin at ${CIVIL3D_HOST}:${CIVIL3D_PORT}`));
        };

        appClient.socket.on("connect", onConnect);
        appClient.socket.on("error", onError);
        signal?.addEventListener("abort", onAbort, { once: true });

        const timeout = setTimeout(() => {
          if (settled) {
            return;
          }
          settled = true;
          cleanup();
          appClient.socket.destroy();
          log.warn("Connection timed out", { host: CIVIL3D_HOST, port: CIVIL3D_PORT, timeoutMs: CONNECT_TIMEOUT_MS });
          reject(new Error(`Connection to Civil 3D plugin timed out after ${CONNECT_TIMEOUT_MS}ms`));
        }, CONNECT_TIMEOUT_MS);

        if (!appClient.connect()) {
          onError(new Error("Socket connect call failed."));
        }
      });
    }

    return await appClient.sendCommand(command, params, options);
  } catch (error) {
    if (signal?.aborted) throw cancellationError;
    throw error;
  } finally {
    signal?.removeEventListener("abort", cancel);
    appClient.disconnect();
  }
}
