import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DDrawingInputShape = {
  action: z.enum(["info", "new", "save", "undo", "redo", "settings"]),
  templatePath: z.string().optional(),
  saveAs: z.string().optional(),
  steps: z.number().int().min(1).max(10).optional(),
};

const Civil3DDrawingInputSchema = z.object(Civil3DDrawingInputShape);

const DrawingInfoArgsSchema = Civil3DDrawingInputSchema.extend({
  action: z.literal("info"),
});

const DrawingNewArgsSchema = Civil3DDrawingInputSchema.extend({
  action: z.literal("new"),
  templatePath: z.string().optional(),
});

const DrawingSaveArgsSchema = Civil3DDrawingInputSchema.extend({
  action: z.literal("save"),
  saveAs: z.string().optional(),
});

const DrawingUndoRedoArgsSchema = Civil3DDrawingInputSchema.extend({
  action: z.enum(["undo", "redo"]),
  steps: z.number().int().min(1).max(10).optional(),
});

const DrawingSettingsArgsSchema = Civil3DDrawingInputSchema.extend({
  action: z.literal("settings"),
});

const DrawingInfoResponseSchema = z.object({
  fileName: z.string().optional(),
  filePath: z.string().optional(),
  coordinateSystem: z.string().nullable().optional(),
  linearUnits: z.enum(["feet", "meters", "other"]).optional(),
  angularUnits: z.enum(["degrees", "radians", "grads"]).optional(),
  unsavedChanges: z.boolean().optional(),
  objectCounts: z
    .object({
      surfaces: z.number().optional(),
      alignments: z.number().optional(),
      profiles: z.number().optional(),
      corridors: z.number().optional(),
      pipeNetworks: z.number().optional(),
      points: z.number().optional(),
      parcels: z.number().optional(),
    })
    .optional(),
  drawingName: z.string().optional(),
  projectName: z.string().optional(),
  units: z.string().optional(),
});

const DrawingSettingsResponseSchema = z.object({
  coordinateSystem: z.string().nullable().optional(),
  coordinateZone: z.string().nullable().optional(),
  datum: z.string().nullable().optional(),
  scaleFactor: z.number().optional(),
  elevationReference: z.string().nullable().optional(),
  defaultLayer: z.string().optional(),
  defaultStyles: z
    .object({
      surface: z.string().optional(),
      alignment: z.string().optional(),
      profile: z.string().optional(),
      corridor: z.string().optional(),
      pipeNetwork: z.string().optional(),
    })
    .optional(),
});

const GenericDrawingResponseSchema = z.object({}).passthrough();

export function registerCivil3DDrawingTool(server: McpServer) {
  server.tool(
    "civil3d_drawing",
    "Manages the active drawing state, provides document-level information, and controls save/undo operations.",
    Civil3DDrawingInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "info"
            ? DrawingInfoArgsSchema.parse(args)
            : args.action === "new"
              ? DrawingNewArgsSchema.parse(args)
            : args.action === "save"
              ? DrawingSaveArgsSchema.parse(args)
              : args.action === "undo" || args.action === "redo"
                ? DrawingUndoRedoArgsSchema.parse(args)
                : DrawingSettingsArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "info") {
            return await appClient.sendCommand("getDrawingInfo", {});
          }

          if (parsedArgs.action === "new") {
            return await appClient.sendCommand("newDrawing", {
              templatePath: parsedArgs.templatePath,
            });
          }

          if (parsedArgs.action === "save") {
            return await appClient.sendCommand("saveDrawing", {
              saveAs: parsedArgs.saveAs,
            });
          }

          if (parsedArgs.action === "undo") {
            return await appClient.sendCommand("undoDrawing", {
              steps: parsedArgs.steps ?? 1,
            });
          }

          if (parsedArgs.action === "redo") {
            return await appClient.sendCommand("redoDrawing", {
              steps: parsedArgs.steps ?? 1,
            });
          }

          return await appClient.sendCommand("getDrawingSettings", {});
        });

        const validatedResponse =
          parsedArgs.action === "info"
            ? DrawingInfoResponseSchema.parse(response)
            : parsedArgs.action === "settings"
              ? DrawingSettingsResponseSchema.parse(response)
              : GenericDrawingResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_drawing action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_drawing tool:", error);
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
