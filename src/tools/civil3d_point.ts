import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const PointCreateInputSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
  description: z.string().nullable().optional(),
});

const Civil3DPointInputShape = {
  action: z.enum(["list", "get", "create", "list_groups", "import", "delete"]),
  groupName: z.string().optional(),
  limit: z.number().optional(),
  offset: z.number().optional(),
  pointNumber: z.number().optional(),
  points: z.array(PointCreateInputSchema).optional(),
  startNumber: z.number().optional(),
  format: z.enum(["pnezd", "penz", "xyzd", "xyz"]).optional(),
  data: z.string().optional(),
  targetSurface: z.string().optional(),
  pointNumbers: z.array(z.number()).optional(),
};

const Civil3DPointInputSchema = z.object(Civil3DPointInputShape);

const PointListArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("list"),
  groupName: z.string().optional(),
  limit: z.number().optional(),
  offset: z.number().optional(),
});

const PointGetArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("get"),
  pointNumber: z.number(),
});

const PointCreateArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("create"),
  points: z.array(PointCreateInputSchema),
  startNumber: z.number().optional(),
});

const PointListGroupsArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("list_groups"),
});

const PointImportArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("import"),
  format: z.enum(["pnezd", "penz", "xyzd", "xyz"]),
  data: z.string(),
  targetSurface: z.string().optional(),
});

const PointDeleteArgsSchema = Civil3DPointInputSchema.extend({
  action: z.literal("delete"),
  pointNumbers: z.array(z.number()).min(1),
});

const PointSchema = z.object({
  number: z.number(),
  name: z.string().nullable(),
  x: z.number(),
  y: z.number(),
  z: z.number(),
  rawDescription: z.string(),
  fullDescription: z.string(),
});

const PointListResponseSchema = z.object({
  totalCount: z.number(),
  returnedCount: z.number(),
  points: z.array(PointSchema),
  units: z.string(),
});

const PointDetailResponseSchema = PointSchema.passthrough();

const PointGroupSchema = z.object({
  name: z.string(),
  description: z.string().nullable(),
  pointCount: z.number(),
  includePattern: z.string().nullable(),
  excludePattern: z.string().nullable(),
});

const PointGroupsResponseSchema = z.object({
  groups: z.array(PointGroupSchema),
});

const GenericPointMutationResponseSchema = z.object({}).passthrough();

export function registerCivil3DPointTool(server: McpServer) {
  server.tool(
    "civil3d_point",
    "Reads, creates, imports, and deletes Civil 3D COGO points and point groups.",
    Civil3DPointInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? PointListArgsSchema.parse(args)
            : args.action === "get"
              ? PointGetArgsSchema.parse(args)
              : args.action === "create"
                ? PointCreateArgsSchema.parse(args)
              : args.action === "list_groups"
                ? PointListGroupsArgsSchema.parse(args)
                : args.action === "import"
                  ? PointImportArgsSchema.parse(args)
                  : PointDeleteArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listCogoPoints", {
              groupName: parsedArgs.groupName,
              limit: parsedArgs.limit,
              offset: parsedArgs.offset,
            });
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getCogoPoint", {
              pointNumber: parsedArgs.pointNumber,
            });
          }

          if (parsedArgs.action === "create") {
            return await appClient.sendCommand("createCogoPoints", {
              points: parsedArgs.points,
              startNumber: parsedArgs.startNumber,
            });
          }

          if (parsedArgs.action === "list_groups") {
            return await appClient.sendCommand("listPointGroups", {});
          }

          if (parsedArgs.action === "import") {
            return await appClient.sendCommand("importCogoPoints", {
              format: parsedArgs.format,
              data: parsedArgs.data,
              targetSurface: parsedArgs.targetSurface,
            });
          }

          return await appClient.sendCommand("deleteCogoPoints", {
            pointNumbers: parsedArgs.pointNumbers,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? PointListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? PointDetailResponseSchema.parse(response)
              : parsedArgs.action === "list_groups"
                ? PointGroupsResponseSchema.parse(response)
                : GenericPointMutationResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_point action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_point tool:", error);
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
