import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point2DSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const Civil3DAlignmentInputShape = {
  action: z.enum(["list", "get", "station_to_point", "point_to_station", "create", "delete"]),
  name: z.string().optional(),
  station: z.number().optional(),
  offset: z.number().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  points: z.array(Point2DSchema).optional(),
  type: z.enum(["centerline", "offset"]).optional(),
  site: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
};

const Civil3DAlignmentInputSchema = z.object(Civil3DAlignmentInputShape);

const AlignmentListArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("list"),
});

const AlignmentGetArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("get"),
  name: z.string(),
});

const AlignmentStationToPointArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("station_to_point"),
  name: z.string(),
  station: z.number(),
  offset: z.number().optional(),
});

const AlignmentPointToStationArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("point_to_station"),
  name: z.string(),
  x: z.number(),
  y: z.number(),
});

const AlignmentCreateArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("create"),
  name: z.string(),
  points: z.array(Point2DSchema).min(2),
  type: z.enum(["centerline", "offset"]).optional(),
  site: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const AlignmentDeleteArgsSchema = Civil3DAlignmentInputSchema.extend({
  action: z.literal("delete"),
  name: z.string(),
});

const AlignmentSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["centerline", "offset", "curb_return", "rail"]),
  length: z.number(),
  startStation: z.number(),
  endStation: z.number(),
  site: z.string().nullable(),
  profileCount: z.number(),
  isReference: z.boolean(),
});

const AlignmentListResponseSchema = z.object({
  alignments: z.array(AlignmentSummarySchema),
});

const AlignmentEntitySchema = z.object({
  index: z.number(),
  type: z.enum(["line", "arc", "spiral"]),
  startStation: z.number(),
  endStation: z.number(),
  length: z.number(),
});

const AlignmentDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  style: z.string(),
  layer: z.string(),
  length: z.number(),
  startStation: z.number(),
  endStation: z.number(),
  entityCount: z.number(),
  entities: z.array(AlignmentEntitySchema),
  dependentProfiles: z.array(z.string()),
  dependentCorridors: z.array(z.string()),
  isReference: z.boolean(),
});

const AlignmentStationToPointResponseSchema = z.object({
  x: z.number(),
  y: z.number(),
  station: z.number(),
  offset: z.number(),
  units: z.string(),
});

const AlignmentPointToStationResponseSchema = z.object({
  station: z.number(),
  offset: z.number(),
  distanceFromAlignment: z.number(),
  units: z.string(),
});

const GenericAlignmentResponseSchema = z.object({}).passthrough();

export function registerCivil3DAlignmentTool(server: McpServer) {
  server.tool(
    "civil3d_alignment",
    "Reads Civil 3D horizontal alignments, converts stationing, and supports create/delete operations.",
    Civil3DAlignmentInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? AlignmentListArgsSchema.parse(args)
            : args.action === "get"
              ? AlignmentGetArgsSchema.parse(args)
              : args.action === "station_to_point"
                ? AlignmentStationToPointArgsSchema.parse(args)
                : args.action === "point_to_station"
                  ? AlignmentPointToStationArgsSchema.parse(args)
                  : args.action === "create"
                    ? AlignmentCreateArgsSchema.parse(args)
                    : AlignmentDeleteArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listAlignments", {});
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getAlignment", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "station_to_point") {
            return await appClient.sendCommand("alignmentStationToPoint", {
              name: parsedArgs.name,
              station: parsedArgs.station,
              offset: parsedArgs.offset ?? 0,
            });
          }

          if (parsedArgs.action === "point_to_station") {
            return await appClient.sendCommand("alignmentPointToStation", {
              name: parsedArgs.name,
              x: parsedArgs.x,
              y: parsedArgs.y,
            });
          }

          if (parsedArgs.action === "create") {
            return await appClient.sendCommand("createAlignment", {
              name: parsedArgs.name,
              points: parsedArgs.points,
              type: parsedArgs.type,
              site: parsedArgs.site,
              style: parsedArgs.style,
              layer: parsedArgs.layer,
              labelSet: parsedArgs.labelSet,
            });
          }

          return await appClient.sendCommand("deleteAlignment", {
            name: parsedArgs.name,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? AlignmentListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? AlignmentDetailResponseSchema.parse(response)
              : parsedArgs.action === "station_to_point"
                ? AlignmentStationToPointResponseSchema.parse(response)
                : parsedArgs.action === "point_to_station"
                  ? AlignmentPointToStationResponseSchema.parse(response)
                  : GenericAlignmentResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_alignment action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_alignment tool:", error);
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
