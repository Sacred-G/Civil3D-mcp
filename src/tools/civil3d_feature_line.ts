import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DFeatureLineInputShape = {
  action: z.enum(["list", "get", "export_as_polyline"]),
  name: z.string().optional(),
  targetLayer: z.string().optional(),
};

const Civil3DFeatureLineInputSchema = z.object(Civil3DFeatureLineInputShape);

const FeatureLineListArgsSchema = Civil3DFeatureLineInputSchema.extend({
  action: z.literal("list"),
});

const FeatureLineGetArgsSchema = Civil3DFeatureLineInputSchema.extend({
  action: z.literal("get"),
  name: z.string(),
});

const FeatureLineExportArgsSchema = Civil3DFeatureLineInputSchema.extend({
  action: z.literal("export_as_polyline"),
  name: z.string(),
  targetLayer: z.string().optional(),
});

const FeatureLineSummarySchema = z
  .object({
    name: z.string().optional(),
    handle: z.string().optional(),
    layer: z.string().optional(),
    style: z.string().optional(),
  })
  .passthrough();

const FeatureLineListResponseSchema = z.object({
  featureLines: z.array(FeatureLineSummarySchema),
});

const FeatureLineVertexSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

const FeatureLineDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  layer: z.string(),
  style: z.string(),
  length: z.number(),
  vertexCount: z.number(),
  vertices: z.array(FeatureLineVertexSchema),
  minElevation: z.number(),
  maxElevation: z.number(),
  units: z.string(),
});

const GenericFeatureLineResponseSchema = z.object({}).passthrough();

export function registerCivil3DFeatureLineTool(server: McpServer) {
  server.tool(
    "civil3d_feature_line",
    "Reads Civil 3D feature lines and supports exporting them as 3D polylines.",
    Civil3DFeatureLineInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? FeatureLineListArgsSchema.parse(args)
            : args.action === "get"
              ? FeatureLineGetArgsSchema.parse(args)
              : FeatureLineExportArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listFeatureLines", {});
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getFeatureLine", {
              name: parsedArgs.name,
            });
          }

          return await appClient.sendCommand("exportFeatureLineAsPolyline", {
            name: parsedArgs.name,
            targetLayer: parsedArgs.targetLayer,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? FeatureLineListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? FeatureLineDetailResponseSchema.parse(response)
              : GenericFeatureLineResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_feature_line action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_feature_line tool:", error);
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
