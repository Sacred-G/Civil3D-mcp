import { readFileSync } from "node:fs";
import { homedir } from "node:os";
import { join } from "node:path";

// The plugin writes a new random token to this file each time its RPC server
// starts and requires it for runCode, so a process running as another account
// cannot run code in this user's Civil 3D through the loopback port.
export function pluginTokenPath(): string {
  return process.env.CIVIL3D_RPC_TOKEN_FILE
    ?? join(process.env.LOCALAPPDATA ?? join(homedir(), "AppData", "Local"), "Civil3DMcpPlugin", "rpc-token");
}

/**
 * The current Civil 3D session's runCode token, or undefined when the file is
 * missing (Civil 3D not running, or a plugin build without tokens). Read on
 * every call: the token changes whenever Civil 3D restarts. Never log it.
 */
export function readPluginToken(): string | undefined {
  try {
    return readFileSync(pluginTokenPath(), "utf8").trim() || undefined;
  } catch {
    return undefined;
  }
}
