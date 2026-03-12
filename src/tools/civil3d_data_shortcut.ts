import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const DataShortcutObjectTypeSchema = z.enum(["surface", "alignment", "profile", "pipe_network"]);

const Civil3DDataShortcutInputShape = {
  action: z.enum(["list", "sync", "create_reference"]),
  sourceFilePath: z.string().optional(),
  objectName: z.string().optional(),
  objectType: DataShortcutObjectTypeSchema.optional(),
};

const Civil3DDataShortcutInputSchema = z.object(Civil3DDataShortcutInputShape);

const DataShortcutListArgsSchema = Civil3DDataShortcutInputSchema.extend({
  action: z.literal("list"),
});

const DataShortcutSyncArgsSchema = Civil3DDataShortcutInputSchema.extend({
  action: z.literal("sync"),
});

const DataShortcutCreateReferenceArgsSchema = Civil3DDataShortcutInputSchema.extend({
  action: z.literal("create_reference"),
  sourceFilePath: z.string(),
  objectName: z.string(),
  objectType: DataShortcutObjectTypeSchema,
});

const IncomingDataShortcutSchema = z.object({
  objectName: z.string(),
  objectType: DataShortcutObjectTypeSchema,
  sourceFilePath: z.string(),
  isSynced: z.boolean(),
  isValid: z.boolean(),
});

const ExportableDataShortcutSchema = z.object({
  objectName: z.string(),
  objectType: z.string(),
  isExported: z.boolean(),
});

const DataShortcutListResponseSchema = z.object({
  incoming: z.array(IncomingDataShortcutSchema),
  exportable: z.array(ExportableDataShortcutSchema),
});

const GenericDataShortcutResponseSchema = z.object({}).passthrough();

export function registerCivil3DDataShortcutTool(server: McpServer) {
  server.tool(
    "civil3d_data_shortcut",
    "Manages Civil 3D data shortcuts including listing, syncing, and creating references.",
    Civil3DDataShortcutInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? DataShortcutListArgsSchema.parse(args)
            : args.action === "sync"
              ? DataShortcutSyncArgsSchema.parse(args)
              : DataShortcutCreateReferenceArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listDataShortcuts", {});
          }

          if (parsedArgs.action === "sync") {
            return await appClient.sendCommand("syncDataShortcuts", {});
          }

          return await appClient.sendCommand("createDataShortcutReference", {
            sourceFilePath: parsedArgs.sourceFilePath,
            objectName: parsedArgs.objectName,
            objectType: parsedArgs.objectType,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? DataShortcutListResponseSchema.parse(response)
            : GenericDataShortcutResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_data_shortcut action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_data_shortcut tool:", error);
        return {
          content: [
            {
              type: "text",
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
