import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const StyleObjectTypeSchema = z.enum([
  "surface",
  "alignment",
  "profile",
  "corridor",
  "pipe",
  "structure",
  "point",
  "section",
  "label",
  "assembly",
]);

const Civil3DStyleInputShape = {
  action: z.enum(["list", "get"]),
  objectType: StyleObjectTypeSchema,
  styleName: z.string().optional(),
};

const Civil3DStyleInputSchema = z.object(Civil3DStyleInputShape);

const StyleListArgsSchema = Civil3DStyleInputSchema.extend({
  action: z.literal("list"),
});

const StyleGetArgsSchema = Civil3DStyleInputSchema.extend({
  action: z.literal("get"),
  styleName: z.string(),
});

const StyleSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  isDefault: z.boolean(),
});

const StyleListResponseSchema = z.object({
  objectType: z.string(),
  styles: z.array(StyleSummarySchema),
});

const StyleDetailResponseSchema = z.object({
  name: z.string().optional(),
  objectType: z.string().optional(),
}).passthrough();

export function registerCivil3DStyleTool(server: McpServer) {
  server.tool(
    "civil3d_style",
    "Lists and inspects Civil 3D styles for supported object types.",
    Civil3DStyleInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? StyleListArgsSchema.parse(args)
            : StyleGetArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listStyles", {
              objectType: parsedArgs.objectType,
            });
          }

          return await appClient.sendCommand("getStyle", {
            objectType: parsedArgs.objectType,
            styleName: parsedArgs.styleName,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? StyleListResponseSchema.parse(response)
            : StyleDetailResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_style action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_style tool:", error);
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
