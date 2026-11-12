import * as net from "net";
import { createLogger } from "./logger.js";
import { createRpcRequestId } from "./requestContext.js";

const log = createLogger("SocketClient");
export const COMMAND_TIMEOUT_MS = parseInt(process.env.CIVIL3D_COMMAND_TIMEOUT ?? "120000", 10);
const MAX_RESPONSE_BYTES = parseInt(process.env.CIVIL3D_MAX_RESPONSE_BYTES ?? "8388608", 10);
// = RpcTcpServer.MaxRequestBytes. The plugin drops a larger request without an
// answer, which the caller would only see as a closed connection.
export const MAX_REQUEST_BYTES = 1024 * 1024;

export interface SendCommandOptions {
  /** Overrides CIVIL3D_COMMAND_TIMEOUT for this command. */
  timeoutMs?: number;
}

interface PendingRequest {
  resolve: (response: string) => void;
  reject: (error: Error) => void;
  timeout: NodeJS.Timeout;
}

/**
 * The address to dial for a configured plugin host. The plugin listens on IPv4
 * loopback only (RpcTcpServer binds IPAddress.Loopback), while Node resolves
 * "localhost" (and an empty host) to ::1 first: any local account can listen on
 * [::1] at the plugin's port and would receive the runCode token. So
 * "localhost" is dialed as 127.0.0.1.
 */
export function pluginConnectHost(host: string): string {
  const normalized = host.trim().toLowerCase();
  return normalized === "" || normalized === "localhost" ? "127.0.0.1" : host;
}

export class Civil3DRpcError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly rpcCode: number,
  ) {
    super(message);
    this.name = "Civil3DRpcError";
  }
}

export class ApplicationClientConnection {
  host: string;
  port: number;
  socket: net.Socket;
  isConnected: boolean = false;
  responseCallbacks: Map<string, PendingRequest> = new Map();
  buffer: string = "";

  constructor(host: string, port: number) {
    this.host = host;
    this.port = port;
    this.socket = new net.Socket();
    this.setupSocketListeners();
  }

  private setupSocketListeners(): void {
    this.socket.on("connect", () => {
      this.isConnected = true;
      log.info("Connected", { host: this.host, port: this.port });
    });

    this.socket.on("data", (data) => {
      this.buffer += data.toString();
      if (Buffer.byteLength(this.buffer, "utf8") > MAX_RESPONSE_BYTES) {
        const error = new Error(`Civil 3D response exceeds ${MAX_RESPONSE_BYTES} bytes.`);
        this.rejectPendingRequests(error);
        this.buffer = "";
        this.socket.destroy(error);
        return;
      }
      this.processBuffer();
    });

    this.socket.on("close", () => {
      this.isConnected = false;
      this.rejectPendingRequests(new Error("Civil 3D plugin connection closed before the command completed."));
      log.debug("Connection closed");
    });

    this.socket.on("error", (error) => {
      log.error("Connection error", { error: String(error) });
      this.isConnected = false;
      this.rejectPendingRequests(new Error(`Civil 3D plugin connection failed: ${error.message}`));
    });
  }

  private rejectPendingRequests(error: Error): void {
    for (const pending of this.responseCallbacks.values()) {
      clearTimeout(pending.timeout);
      pending.reject(error);
    }
    this.responseCallbacks.clear();
  }

  private processBuffer(): void {
    try {
      JSON.parse(this.buffer);
      this.handleResponse(this.buffer);
      this.buffer = "";
    } catch {
      // Incomplete JSON — wait for more data
    }
  }

  public connect(): boolean {
    if (this.isConnected) {
      return true;
    }

    try {
      this.socket.connect({ port: this.port, host: pluginConnectHost(this.host) });
      return true;
    } catch (error) {
      log.error("Failed to connect", { host: this.host, port: this.port, error: String(error) });
      return false;
    }
  }

  public disconnect(): void {
    this.rejectPendingRequests(new Error("Civil 3D plugin connection was closed by the client."));
    this.socket.end();
    this.isConnected = false;
  }

  public cancel(error = new Civil3DRpcError(
    "Civil 3D request was cancelled by the caller.",
    "CIVIL3D.CANCELLED",
    -32010,
  )): void {
    this.rejectPendingRequests(error);
    this.socket.destroy();
    this.isConnected = false;
  }

  private generateRequestId(): string {
    return createRpcRequestId();
  }

  private handleResponse(responseData: string): void {
    try {
      const response = JSON.parse(responseData);
      const requestId = response.id || "default";

      const pending = this.responseCallbacks.get(requestId);
      if (pending) {
        clearTimeout(pending.timeout);
        this.responseCallbacks.delete(requestId);
        pending.resolve(responseData);
      }
    } catch (error) {
      log.error("Error parsing response", { error: String(error) });
    }
  }

  public sendCommand(command: string, params: any = {}, options: SendCommandOptions = {}): Promise<any> {
    return new Promise((resolve, reject) => {
      const requestId = this.generateRequestId();

      const commandObj = {
        jsonrpc: "2.0",
        method: command,
        params,
        id: requestId,
      };
      const commandString = JSON.stringify(commandObj);

      // Refuse before anything is sent or registered (see MAX_REQUEST_BYTES).
      const requestBytes = Buffer.byteLength(commandString, "utf8");
      if (requestBytes > MAX_REQUEST_BYTES) {
        reject(new Civil3DRpcError(
          `Civil 3D request '${command}' is ${requestBytes} bytes; the plugin accepts at most ${MAX_REQUEST_BYTES} bytes per call. Split the data or pass a file path.`,
          "CIVIL3D.REQUEST_TOO_LARGE",
          -32602,
        ));
        return;
      }

      if (!this.isConnected && !this.connect()) {
        reject(new Error(`Failed to connect to Civil 3D plugin at ${this.host}:${this.port}.`));
        return;
      }

      const timeoutMs = options.timeoutMs ?? COMMAND_TIMEOUT_MS;
      const timeout = setTimeout(() => {
        const pending = this.responseCallbacks.get(requestId);
        if (pending) {
          this.responseCallbacks.delete(requestId);
          log.warn("Command timed out", { method: command, requestId, timeoutMs });
          pending.reject(new Civil3DRpcError(
            `Command timed out after ${timeoutMs}ms: ${command}`,
            "CIVIL3D.TIMEOUT",
            -32008,
          ));
        }
      }, timeoutMs);

      this.responseCallbacks.set(requestId, {
        timeout,
        reject,
        resolve: (responseData) => {
          try {
            const response = JSON.parse(responseData);
            if (response.jsonrpc !== "2.0" || response.id !== requestId) {
              reject(new Error("Invalid JSON-RPC 2.0 response envelope from Civil 3D plugin."));
              return;
            }

            const hasResult = Object.prototype.hasOwnProperty.call(response, "result");
            const hasError = Object.prototype.hasOwnProperty.call(response, "error");
            if (hasResult === hasError) {
              reject(new Error("Invalid JSON-RPC 2.0 response: expected exactly one of result or error."));
              return;
            }

            if (response.error) {
              if (typeof response.error.code !== "number"
                || typeof response.error.message !== "string"
                || typeof response.error.data?.code !== "string") {
                reject(new Error("Invalid JSON-RPC 2.0 error response from Civil 3D plugin."));
                return;
              }
              const domainCode = typeof response.error.data?.code === "string"
                ? response.error.data.code
                : "CIVIL3D.UNKNOWN_ERROR";
              const rpcCode = typeof response.error.code === "number"
                ? response.error.code
                : -32000;
              reject(new Civil3DRpcError(
                response.error.message || "Unknown error from Civil 3D plugin",
                domainCode,
                rpcCode,
              ));
            } else {
              resolve(response.result);
            }
          } catch (error) {
            if (error instanceof Error) {
              reject(new Error(`Failed to parse response: ${error.message}`));
            } else {
              reject(new Error(`Failed to parse response: ${String(error)}`));
            }
          }
        },
      });

      try {
        log.debug("Sending command", { method: command, requestId });
        this.socket.write(commandString);
      } catch (error) {
        clearTimeout(timeout);
        this.responseCallbacks.delete(requestId);
        reject(error);
      }
    });
  }
}
