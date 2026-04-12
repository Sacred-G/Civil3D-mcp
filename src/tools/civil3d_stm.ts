import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ─── Input Schema ───────────────────────────────────────────
const StmInputShape = {
  action: z.enum([
    "list_ssa_capabilities",
    "export_stm",
    "import_stm",
    "open_storm_sanitary_analysis",
  ]),
  filePath: z.string().optional(),
};

const StmInputSchema = z.object(StmInputShape);
type StmInput = z.infer<typeof StmInputSchema>;

// ─── Action-specific schemas ────────────────────────────────
const ListSsaCapabilitiesSchema = StmInputSchema.extend({
  action: z.literal("list_ssa_capabilities"),
});

const ExportStmSchema = StmInputSchema.extend({
  action: z.literal("export_stm"),
});

const ImportStmSchema = StmInputSchema.extend({
  action: z.literal("import_stm"),
});

const OpenSsaSchema = StmInputSchema.extend({
  action: z.literal("open_storm_sanitary_analysis"),
});

// ─── Plugin method mapping ──────────────────────────────────
function actionToPluginMethod(action: string): string {
  switch (action) {
    case "list_ssa_capabilities": return "listSsaCapabilities";
    case "export_stm": return "exportStm";
    case "import_stm": return "importStm";
    case "open_storm_sanitary_analysis": return "openStormSanitaryAnalysis";
    default: throw new Error(`Unknown STM action: ${action}`);
  }
}

// ─── Registration ───────────────────────────────────────────
export function registerCivil3DStmTools(server: McpServer) {
  server.tool(
    "civil3d_stm",
    "Manages Storm and Sanitary Analysis (SSA) workflows including STM file import/export and gravity network analysis. Actions: list_ssa_capabilities, export_stm, import_stm, open_storm_sanitary_analysis. Note: export/import commands open dialogs requiring user interaction.",
    StmInputShape,
    async (args: StmInput, _extra) => {
      try {
        let parsedArgs: StmInput;
        switch (args.action) {
          case "list_ssa_capabilities":
            parsedArgs = ListSsaCapabilitiesSchema.parse(args);
            break;
          case "export_stm":
            parsedArgs = ExportStmSchema.parse(args);
            break;
          case "import_stm":
            parsedArgs = ImportStmSchema.parse(args);
            break;
          case "open_storm_sanitary_analysis":
            parsedArgs = OpenSsaSchema.parse(args);
            break;
          default:
            throw new Error(`Unknown action: ${args.action}`);
        }

        const pluginMethod = actionToPluginMethod(parsedArgs.action);

        const params: Record<string, unknown> = {};
        if (parsedArgs.filePath !== undefined) params.filePath = parsedArgs.filePath;

        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand(pluginMethod, params);
        });

        return {
          content: [
            {
              type: "text" as const,
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_stm action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_stm tool:", error);
        return {
          content: [
            {
              type: "text" as const,
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
