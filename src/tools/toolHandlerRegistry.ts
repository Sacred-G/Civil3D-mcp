/**
 * Global registry that captures MCP tool handlers during server.tool() registration.
 * The HTTP bridge uses this registry to invoke any registered tool without going
 * through the MCP stdio protocol, enabling the AI Copilot (which connects via HTTP)
 * to call all 180+ tools.
 *
 * How it works:
 *   1. registerTools() in register.ts intercepts server.tool() calls
 *   2. Each handler is stored here by tool name
 *   3. httpBridge.ts looks up handlers from this registry
 *   4. The handler is called with the same params the MCP protocol would provide
 *   5. The MCP CallToolResult is unwrapped to extract the JSON payload
 */

/** MCP CallToolResult shape returned by every server.tool() handler */
interface McpCallToolResult {
  content: Array<{ type: string; text?: string }>;
  isError?: boolean;
}

type ToolHandler = (
  args: Record<string, unknown>,
  extra?: unknown
) => Promise<McpCallToolResult>;

const _handlers = new Map<string, ToolHandler>();

/**
 * Store a tool handler during registration.
 * Called by the server.tool() interceptor in register.ts.
 */
export function captureToolHandler(name: string, handler: ToolHandler): void {
  _handlers.set(name, handler);
}

/**
 * Retrieve a previously captured tool handler by name.
 */
export function getToolHandler(name: string): ToolHandler | undefined {
  return _handlers.get(name);
}

/**
 * Check whether a handler exists for the given tool name.
 */
export function hasToolHandler(name: string): boolean {
  return _handlers.has(name);
}

/**
 * Return all registered tool names (sorted).
 */
export function listRegisteredToolNames(): string[] {
  return [..._handlers.keys()].sort();
}

/**
 * Execute a registered tool and unwrap the MCP CallToolResult into a plain
 * JSON-serialisable object suitable for HTTP responses.
 *
 * Throws if the tool is not registered or if the handler signals an error.
 */
export async function executeRegisteredTool(
  toolName: string,
  parameters: Record<string, unknown>
): Promise<unknown> {
  const handler = _handlers.get(toolName);
  if (!handler) {
    throw new Error(
      `Tool '${toolName}' is not registered. Available: ${listRegisteredToolNames().length} tools.`
    );
  }

  const mcpResult = await handler(parameters, {});

  // If the tool signalled an error, throw so the caller can set the HTTP status
  if (mcpResult?.isError) {
    const errorText =
      mcpResult.content?.[0]?.text ?? `Tool '${toolName}' returned an error`;
    throw new Error(errorText);
  }

  // Unwrap the MCP content envelope → raw JSON payload
  const text = mcpResult?.content?.[0]?.text;
  if (text) {
    try {
      return JSON.parse(text);
    } catch {
      // Not JSON – return as a message string
      return { message: text };
    }
  }

  return mcpResult;
}
