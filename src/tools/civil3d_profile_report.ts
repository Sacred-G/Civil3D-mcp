import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DProfileReportInputShape = {
  alignmentName: z.string(),
  profileName: z.string(),
  interval: z.number().positive().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  maximumSamples: z.number().int().positive().max(400).optional(),
};

const Civil3DProfileReportInputSchema = z.object(Civil3DProfileReportInputShape);

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

const ProfileReportResponseSchema = z.object({
  profile: ProfileDetailResponseSchema,
  samples: ProfileSampleElevationsResponseSchema,
  summary: z.object({
    sampledStationCount: z.number(),
    startStation: z.number(),
    endStation: z.number(),
    interval: z.number(),
    minimumElevation: z.number(),
    maximumElevation: z.number(),
    elevationRange: z.number(),
    averageGrade: z.number().nullable(),
    totalAscendingLength: z.number(),
    totalDescendingLength: z.number(),
    tangentCount: z.number(),
    curveCount: z.number(),
    pviCount: z.number(),
  }),
});

function estimateSampleCount(startStation: number, endStation: number, interval: number) {
  const length = Math.max(0, endStation - startStation);
  return Math.floor(length / interval) + 2;
}

export function registerCivil3DProfileReportTool(server: McpServer) {
  server.tool(
    "civil3d_profile_report",
    "Builds a structured profile report by fetching profile detail and sampling elevations along the profile range.",
    Civil3DProfileReportInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs = Civil3DProfileReportInputSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          const profile = ProfileDetailResponseSchema.parse(
            await appClient.sendCommand("getProfile", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
            })
          );

          const startStation = parsedArgs.startStation ?? profile.startStation;
          const endStation = parsedArgs.endStation ?? profile.endStation;
          const interval = parsedArgs.interval ?? Math.max((endStation - startStation) / 20, 25);
          const maximumSamples = parsedArgs.maximumSamples ?? 150;
          const sampleCount = estimateSampleCount(startStation, endStation, interval);

          if (sampleCount > maximumSamples) {
            throw new Error(
              `Profile report would require ${sampleCount} samples, which exceeds the maximum of ${maximumSamples}. Increase the interval or maximumSamples.`
            );
          }

          const samples = ProfileSampleElevationsResponseSchema.parse(
            await appClient.sendCommand("sampleProfileElevations", {
              alignmentName: parsedArgs.alignmentName,
              profileName: parsedArgs.profileName,
              startStation,
              endStation,
              interval,
            })
          );

          const elevations = samples.samples.map((sample) => sample.elevation);
          const grades = samples.samples
            .map((sample) => sample.grade)
            .filter((grade): grade is number => grade !== null);

          const totalAscendingLength = profile.entities
            .filter((entity) => (entity.grade ?? 0) > 0)
            .reduce((sum, entity) => sum + entity.length, 0);
          const totalDescendingLength = profile.entities
            .filter((entity) => (entity.grade ?? 0) < 0)
            .reduce((sum, entity) => sum + entity.length, 0);
          const tangentCount = profile.entities.filter((entity) => entity.type === "tangent").length;
          const curveCount = profile.entities.filter((entity) => entity.type !== "tangent").length;
          const averageGrade = grades.length > 0
            ? grades.reduce((sum, grade) => sum + grade, 0) / grades.length
            : null;

          return ProfileReportResponseSchema.parse({
            profile,
            samples,
            summary: {
              sampledStationCount: samples.samples.length,
              startStation: samples.startStation,
              endStation: samples.endStation,
              interval: samples.interval,
              minimumElevation: Math.min(...elevations),
              maximumElevation: Math.max(...elevations),
              elevationRange: Math.max(...elevations) - Math.min(...elevations),
              averageGrade,
              totalAscendingLength,
              totalDescendingLength,
              tangentCount,
              curveCount,
              pviCount: profile.pviCount,
            },
          });
        });

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to execute civil3d_profile_report";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_profile_report tool:", error);
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
