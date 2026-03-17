import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_data_shortcut_create ─────────────────────────────────────────

export const DataShortcutCreateInputSchema = z.object({
  objectType: z.enum(["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor", "section_view_group"]).describe("Type of Civil 3D object to publish as a data shortcut"),
  objectName: z.string().describe("Name of the Civil 3D object to publish"),
  description: z.string().optional().describe("Description to embed in the data shortcut XML (visible to referencing drawings)"),
  projectFolder: z.string().optional().describe("Full path to the Civil 3D project folder (data shortcuts root). If omitted, uses the active project folder set in the drawing."),
});

// ─── 2. civil3d_data_shortcut_promote ────────────────────────────────────────

export const DataShortcutPromoteInputSchema = z.object({
  shortcutName: z.string().describe("Name of the data shortcut reference to promote to a full, editable Civil 3D object"),
  shortcutType: z.enum(["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor"]).describe("Type of the data shortcut"),
  newName: z.string().optional().describe("Name to give the promoted object (defaults to the shortcut name)"),
});

// ─── 3. civil3d_data_shortcut_reference ──────────────────────────────────────

export const DataShortcutReferenceInputSchema = z.object({
  projectFolder: z.string().describe("Full path to the Civil 3D project folder that contains the data shortcuts to reference"),
  shortcutName: z.string().describe("Name of the data shortcut to reference into the current drawing"),
  shortcutType: z.enum(["surface", "alignment", "profile", "pipe_network", "pressure_network", "corridor", "section_view_group"]).describe("Type of the data shortcut"),
  layer: z.string().optional().describe("AutoCAD layer to place the referenced object's display representation on (uses shortcut default when omitted)"),
});

// ─── 4. civil3d_data_shortcut_sync ───────────────────────────────────────────

export const DataShortcutSyncInputSchema = z.object({
  projectFolder: z.string().optional().describe("Full path to the Civil 3D project folder to sync against. Omit to use the active project folder."),
  shortcutNames: z.array(z.string()).optional().describe("List of specific shortcut names to synchronize. Omit to sync all outdated references in the drawing."),
  dryRun: z.boolean().optional().describe("Report which references are out of date without applying updates (default: false)"),
});

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DDataShortcutMgmtTools(server: McpServer) {

  // 1. civil3d_data_shortcut_create
  server.tool(
    "civil3d_data_shortcut_create",
    "Publish a Civil 3D object (surface, alignment, profile, corridor, pipe network, or section view group) as a data shortcut so other drawings in the project can reference it without copying data. Writes a shortcut XML entry to the project's Data Shortcuts folder.",
    DataShortcutCreateInputSchema.shape,
    async (args) => {
      try {
        const parsed = DataShortcutCreateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("createDataShortcut", {
            objectType: parsed.objectType,
            objectName: parsed.objectName,
            description: parsed.description ?? null,
            projectFolder: parsed.projectFolder ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_data_shortcut_create", error);
      }
    }
  );

  // 2. civil3d_data_shortcut_promote
  server.tool(
    "civil3d_data_shortcut_promote",
    "Promote a read-only data shortcut reference in the current drawing to a full, locally-editable Civil 3D object. The promoted object is independent of the source drawing and can be modified freely.",
    DataShortcutPromoteInputSchema.shape,
    async (args) => {
      try {
        const parsed = DataShortcutPromoteInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("promoteDataShortcut", {
            shortcutName: parsed.shortcutName,
            shortcutType: parsed.shortcutType,
            newName: parsed.newName ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_data_shortcut_promote", error);
      }
    }
  );

  // 3. civil3d_data_shortcut_reference
  server.tool(
    "civil3d_data_shortcut_reference",
    "Reference an existing Civil 3D data shortcut from a project folder into the current drawing. Creates a live, read-only link to the source object that updates automatically when the source changes and you synchronize.",
    DataShortcutReferenceInputSchema.shape,
    async (args) => {
      try {
        const parsed = DataShortcutReferenceInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("referenceDataShortcut", {
            projectFolder: parsed.projectFolder,
            shortcutName: parsed.shortcutName,
            shortcutType: parsed.shortcutType,
            layer: parsed.layer ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_data_shortcut_reference", error);
      }
    }
  );

  // 4. civil3d_data_shortcut_sync
  server.tool(
    "civil3d_data_shortcut_sync",
    "Synchronize outdated Civil 3D data shortcut references in the current drawing against their source objects. Detects which referenced objects have changed since last sync and updates them. Use dryRun=true to audit without modifying the drawing.",
    DataShortcutSyncInputSchema.shape,
    async (args) => {
      try {
        const parsed = DataShortcutSyncInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("syncDataShortcuts", {
            projectFolder: parsed.projectFolder ?? null,
            shortcutNames: parsed.shortcutNames ?? null,
            dryRun: parsed.dryRun ?? false,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_data_shortcut_sync", error);
      }
    }
  );
}
