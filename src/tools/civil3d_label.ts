import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const LabelPointSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const LabelListObjectTypeSchema = z.enum(["alignment", "profile", "surface", "pipe_network"]);

const Civil3DLabelInputShape = {
  action: z.enum(["list", "add", "list_styles"]),
  objectType: z.string(),
  objectName: z.string().optional(),
  labelType: z.string().optional(),
  labelStyle: z.string().optional(),
  station: z.number().optional(),
  point: LabelPointSchema.optional(),
};

const Civil3DLabelInputSchema = z.object(Civil3DLabelInputShape);

const LabelListArgsSchema = Civil3DLabelInputSchema.extend({
  action: z.literal("list"),
  objectType: LabelListObjectTypeSchema,
  objectName: z.string(),
});

const LabelAddArgsSchema = Civil3DLabelInputSchema.extend({
  action: z.literal("add"),
  objectType: z.string(),
  objectName: z.string(),
  labelType: z.string(),
  labelStyle: z.string().optional(),
  station: z.number().optional(),
  point: LabelPointSchema.optional(),
});

const LabelListStylesArgsSchema = Civil3DLabelInputSchema.extend({
  action: z.literal("list_styles"),
  objectType: z.string(),
});

const GenericLabelSchema = z.object({}).passthrough();

const LabelListResponseSchema = z.object({
  labels: z.array(GenericLabelSchema),
}).passthrough();

const LabelAddResponseSchema = GenericLabelSchema;

const LabelStylesResponseSchema = z.object({
  objectType: z.string().optional(),
  styles: z.array(GenericLabelSchema),
}).passthrough();

export function registerCivil3DLabelTool(server: McpServer) {
  server.tool(
    "civil3d_label",
    "Manages labels on Civil 3D objects.",
    Civil3DLabelInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? LabelListArgsSchema.parse(args)
            : args.action === "add"
              ? LabelAddArgsSchema.parse(args)
              : LabelListStylesArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listLabels", {
              objectType: parsedArgs.objectType,
              objectName: parsedArgs.objectName,
            });
          }

          if (parsedArgs.action === "add") {
            return await appClient.sendCommand("addLabel", {
              objectType: parsedArgs.objectType,
              objectName: parsedArgs.objectName,
              labelType: parsedArgs.labelType,
              labelStyle: parsedArgs.labelStyle,
              station: parsedArgs.station,
              point: parsedArgs.point,
            });
          }

          return await appClient.sendCommand("listLabelStyles", {
            objectType: parsedArgs.objectType,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? LabelListResponseSchema.parse(response)
            : parsedArgs.action === "add"
              ? LabelAddResponseSchema.parse(response)
              : LabelStylesResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_label action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_label tool:", error);
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
