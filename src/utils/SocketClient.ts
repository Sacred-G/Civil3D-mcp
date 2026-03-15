import * as net from "net";
import { createLogger } from "./logger.js";

const log = createLogger("SocketClient");
const COMMAND_TIMEOUT_MS = parseInt(process.env.CIVIL3D_COMMAND_TIMEOUT ?? "120000", 10);

export class ApplicationClientConnection {
  host: string;
  port: number;
  socket: net.Socket;
  isConnected: boolean = false;
  responseCallbacks: Map<string, (response: string) => void> = new Map();
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
      this.processBuffer();
    });

    this.socket.on("close", () => {
      this.isConnected = false;
      log.debug("Connection closed");
    });

    this.socket.on("error", (error) => {
      log.error("Connection error", { error: String(error) });
      this.isConnected = false;
    });
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
      this.socket.connect(this.port, this.host);
      return true;
    } catch (error) {
      log.error("Failed to connect", { host: this.host, port: this.port, error: String(error) });
      return false;
    }
  }

  public disconnect(): void {
    this.socket.end();
    this.isConnected = false;
  }

  private generateRequestId(): string {
    return Date.now().toString() + Math.random().toString().substring(2, 8);
  }

  private handleResponse(responseData: string): void {
    try {
      const response = JSON.parse(responseData);
      const requestId = response.id || "default";

      const callback = this.responseCallbacks.get(requestId);
      if (callback) {
        callback(responseData);
        this.responseCallbacks.delete(requestId);
      }
    } catch (error) {
      log.error("Error parsing response", { error: String(error) });
    }
  }

  public sendCommand(command: string, params: any = {}): Promise<any> {
    return new Promise((resolve, reject) => {
      try {
        if (!this.isConnected) {
          this.connect();
        }

        const requestId = this.generateRequestId();

        const commandObj = {
          jsonrpc: "2.0",
          method: command,
          params: params,
          id: requestId,
        };

        this.responseCallbacks.set(requestId, (responseData) => {
          try {
            const response = JSON.parse(responseData);
            if (response.error) {
              reject(
                new Error(response.error.message || "Unknown error from Application")
              );
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
        });

        const commandString = JSON.stringify(commandObj);
        log.debug("Sending command", { method: command, requestId });
        this.socket.write(commandString);

        setTimeout(() => {
          if (this.responseCallbacks.has(requestId)) {
            this.responseCallbacks.delete(requestId);
            log.warn("Command timed out", { method: command, requestId, timeoutMs: COMMAND_TIMEOUT_MS });
            reject(new Error(`Command timed out after ${COMMAND_TIMEOUT_MS}ms: ${command}`));
          }
        }, COMMAND_TIMEOUT_MS);
      } catch (error) {
        reject(error);
      }
    });
  }
}
