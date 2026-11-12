import { readFileSync } from "node:fs";
import { homedir } from "node:os";
import { join } from "node:path";
import { isIP } from "node:net";
import { pluginConnectHost } from "./SocketClient.js";

// The plugin writes a new random token to this file each time its RPC server
// starts and requires it for runCode, so a process running as another account
// cannot run code in this user's Civil 3D through the loopback port.
export function pluginTokenPath(): string {
  // || rather than ??: an empty variable falls back to the default too.
  return process.env.CIVIL3D_RPC_TOKEN_FILE
    || join(process.env.LOCALAPPDATA || join(homedir(), "AppData", "Local"), "Civil3DMcpPlugin", "rpc-token");
}

/**
 * The current Civil 3D session's runCode token, or undefined when the file is
 * missing or empty (Civil 3D not running, or a plugin build without tokens).
 * Read on every call: the token changes whenever Civil 3D restarts. Any other
 * read error (permissions, a bad path) is thrown, so it is not mistaken for a
 * plugin without tokens. Never log the token.
 */
export function readPluginToken(): string | undefined {
  let text: string;
  try {
    text = readFileSync(pluginTokenPath(), "utf8");
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === "ENOENT") return undefined;
    throw new Error(`Could not read the Civil 3D plugin token file ${pluginTokenPath()}: ${(error as Error).message}`);
  }
  return text.trim() || undefined;
}

/**
 * True when the configured plugin host is a loopback address, the only place
 * the runCode token may be sent (the plugin itself listens on IPv4 loopback).
 */
export function isLoopbackPluginHost(host: string): boolean {
  const dialed = pluginConnectHost(host).trim().toLowerCase().replace(/^\[|\]$/g, "");
  if (isIP(dialed) === 4) return dialed.startsWith("127.");
  if (isIP(dialed) === 6) return dialed === "::1";
  return false;
}
