import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DProfileInputShape = {
  action: z.enum(["list", "get", "get_elevation", "sample_elevations", "create_from_surface", "create_layout", "delete"]),
  alignmentName: z.string().optional(),
  profileName: z.string().optional(),
  station: z.number().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  interval: z.number().optional(),
  surfaceName: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
};

const Civil3DProfileInputSchema = z.object(Civil3DProfileInputShape);

const ProfileListArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("list"),
  alignmentName: z.string(),
});

const ProfileGetArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("get"),
  alignmentName: z.string(),
  profileName: z.string(),
});

const ProfileGetElevationArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("get_elevation"),
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
});

const ProfileSampleElevationsArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("sample_elevations"),
  alignmentName: z.string(),
  profileName: z.string(),
  interval: z.number(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
});

const ProfileCreateFromSurfaceArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("create_from_surface"),
  alignmentName: z.string(),
  profileName: z.string(),
  surfaceName: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const ProfileCreateLayoutArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("create_layout"),
  alignmentName: z.string(),
  profileName: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const ProfileDeleteArgsSchema = Civil3DProfileInputSchema.extend({
  action: z.literal("delete"),
  alignmentName: z.string(),
  profileName: z.string(),
});

const ProfileSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["surface", "layout", "superimposed"]),
  style: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  minElevation: z.number(),
  maxElevation: z.number(),
});

const ProfileListResponseSchema = z.object({
  alignmentName: z.string(),
  profiles: z.array(ProfileSummarySchema),
});

const ProfileEntitySchema = z.object({
  index: z.number(),
  type: z.enum(["tangent", "circular_curve", "parabola", "asymmetric_parabola"]),
  startStation: z.number(),
  endStation: z.number(),
  startElevation: z.number(),
  endElevation: z.number(),
  grade: z.number().nullable(),
  length: z.number(),
});

const ProfileDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  style: z.string(),
  layer: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  minElevation: z.number(),
  maxElevation: z.number(),
  entityCount: z.number(),
  entities: z.array(ProfileEntitySchema),
  pviCount: z.number(),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
});

const ProfileElevationResponseSchema = z.object({
  station: z.number(),
  elevation: z.number(),
  grade: z.number(),
  units: z.string(),
});

const ProfileSamplePointSchema = z.object({
  station: z.number(),
  elevation: z.number(),
  grade: z.number().nullable(),
});

const ProfileSampleElevationsResponseSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  interval: z.number(),
  samples: z.array(ProfileSamplePointSchema),
  units: z.string(),
});

const GenericProfileResponseSchema = z.object({}).passthrough();

export function registerCivil3DProfileTool(server: McpServer) {
  server.tool(
    "civil3d_profile",
    "Reads Civil 3D vertical profiles and supports creation and deletion of profiles.",
    Civil3DProfileInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? ProfileListArgsSchema.parse(args)
            : args.action === "get"
              ? ProfileGetArgsSchema.parse(args)
              : args.action === "get_elevation"
                ? ProfileGetElevationArgsSchema.parse(args)
                : args.action === "sample_elevations"
                  ? ProfileSampleElevationsArgsSchema.parse(args)
                  : args.action === "create_from_surface"
                    ? ProfileCreateFromSurfaceArgsSchema.parse(args)
                    : args.action === "create_layout"
                      ? ProfileCreateLayoutArgsSchema.parse(args)
                      : ProfileDeleteArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listProfiles", {
              alignmentName: parsedArgs.alignmentName,
            });
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getProfile", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
            });
          }

          if (parsedArgs.action === "get_elevation") {
            return await appClient.sendCommand("getProfileElevation", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
              station: parsedArgs.station,
            });
          }

          if (parsedArgs.action === "sample_elevations") {
            return await appClient.sendCommand("sampleProfileElevations", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
              startStation: parsedArgs.startStation,
              endStation: parsedArgs.endStation,
              interval: parsedArgs.interval,
            });
          }

          if (parsedArgs.action === "create_from_surface") {
            return await appClient.sendCommand("createProfileFromSurface", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
              surfaceName: parsedArgs.surfaceName,
              style: parsedArgs.style,
              layer: parsedArgs.layer,
              labelSet: parsedArgs.labelSet,
            });
          }

          if (parsedArgs.action === "create_layout") {
            return await appClient.sendCommand("createLayoutProfile", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
              style: parsedArgs.style,
              layer: parsedArgs.layer,
              labelSet: parsedArgs.labelSet,
            });
          }

          return await appClient.sendCommand("deleteProfile", {
            alignmentName: parsedArgs.alignmentName,
            profileName: parsedArgs.profileName,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? ProfileListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? ProfileDetailResponseSchema.parse(response)
              : parsedArgs.action === "get_elevation"
                ? ProfileElevationResponseSchema.parse(response)
                : parsedArgs.action === "sample_elevations"
                  ? ProfileSampleElevationsResponseSchema.parse(response)
                  : GenericProfileResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_profile action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_profile tool:", error);
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
