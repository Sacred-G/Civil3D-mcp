import { ApplicationClientConnection } from "./SocketClient.js";

const CIVIL3D_HOST = process.env.CIVIL3D_HOST ?? "localhost";
const CIVIL3D_PORT = parseInt(process.env.CIVIL3D_PORT ?? "8080", 10);
const CONNECT_TIMEOUT_MS = parseInt(process.env.CIVIL3D_CONNECT_TIMEOUT ?? "5000", 10);

/**
 * Opens a short-lived connection to the Civil 3D plugin, runs the given
 * operation, and tears the connection down afterwards.
 */
export async function withApplicationConnection<T>(
  operation: (client: ApplicationClientConnection) => Promise<T>
): Promise<T> {
  const appClient = new ApplicationClientConnection(CIVIL3D_HOST, CIVIL3D_PORT);

  try {
    if (!appClient.isConnected) {
      await new Promise<void>((resolve, reject) => {
        const onConnect = () => {
          appClient.socket.removeListener("connect", onConnect);
          appClient.socket.removeListener("error", onError);
          resolve();
        };

        const onError = (error: any) => {
          appClient.socket.removeListener("connect", onConnect);
          appClient.socket.removeListener("error", onError);
          reject(new Error(`Failed to connect to Civil 3D plugin at ${CIVIL3D_HOST}:${CIVIL3D_PORT}`));
        };

        appClient.socket.on("connect", onConnect);
        appClient.socket.on("error", onError);

        appClient.connect();

        setTimeout(() => {
          appClient.socket.removeListener("connect", onConnect);
          appClient.socket.removeListener("error", onError);
          reject(new Error(`Connection to Civil 3D plugin timed out after ${CONNECT_TIMEOUT_MS}ms`));
        }, CONNECT_TIMEOUT_MS);
      });
    }

    return await operation(appClient);
  } finally {
    appClient.disconnect();
  }
}
