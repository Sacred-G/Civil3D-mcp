import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DAlignmentReportInputShape = {
  alignmentName: z.string(),
  interval: z.number().positive().optional(),
  offset: z.number().optional(),
  maximumSamples: z.number().int().positive().max(200).optional(),
};

const Civil3DAlignmentReportInputSchema = z.object(Civil3DAlignmentReportInputShape);

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

const AlignmentReportStationSampleSchema = z.object({
  station: z.number(),
  x: z.number(),
  y: z.number(),
  offset: z.number(),
  units: z.string(),
});

const AlignmentReportResponseSchema = z.object({
  alignment: AlignmentDetailResponseSchema,
  summary: z.object({
    interval: z.number(),
    sampleCount: z.number(),
    startStation: z.number(),
    endStation: z.number(),
    length: z.number(),
    entityCount: z.number(),
    entityTypeBreakdown: z.object({
      line: z.number(),
      arc: z.number(),
      spiral: z.number(),
    }),
    dependentProfileCount: z.number(),
    dependentCorridorCount: z.number(),
  }),
  stationSamples: z.array(AlignmentReportStationSampleSchema),
});

function buildStationSequence(startStation: number, endStation: number, interval: number, maximumSamples: number) {
  const stations: number[] = [];
  const length = Math.max(0, endStation - startStation);
  const estimatedSamples = Math.floor(length / interval) + 2;

  if (estimatedSamples > maximumSamples) {
    throw new Error(
      `Alignment report would require ${estimatedSamples} samples, which exceeds the maximum of ${maximumSamples}. Increase the interval or maximumSamples.`
    );
  }

  let currentStation = startStation;
  while (currentStation < endStation) {
    stations.push(Number(currentStation.toFixed(6)));
    currentStation += interval;
  }

  if (stations.length === 0 || stations[stations.length - 1] !== endStation) {
    stations.push(Number(endStation.toFixed(6)));
  }

  return stations;
}

export function registerCivil3DAlignmentReportTool(server: McpServer) {
  server.tool(
    "civil3d_alignment_report",
    "Builds an alignment report by fetching alignment geometry and sampling station locations along the alignment.",
    Civil3DAlignmentReportInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs = Civil3DAlignmentReportInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          const alignment = AlignmentDetailResponseSchema.parse(
            await appClient.sendCommand("getAlignment", {
              name: parsedArgs.alignmentName,
            })
          );

          const interval = parsedArgs.interval ?? Math.max((alignment.endStation - alignment.startStation) / 20, 25);
          const maximumSamples = parsedArgs.maximumSamples ?? 100;
          const stations = buildStationSequence(alignment.startStation, alignment.endStation, interval, maximumSamples);

          const stationSamples: z.infer<typeof AlignmentStationToPointResponseSchema>[] = [];
          for (const station of stations) {
            stationSamples.push(
              AlignmentStationToPointResponseSchema.parse(
                await appClient.sendCommand("alignmentStationToPoint", {
                  name: parsedArgs.alignmentName,
                  station,
                  offset: parsedArgs.offset ?? 0,
                })
              )
            );
          }

          const entityTypeBreakdown = alignment.entities.reduce(
            (accumulator, entity) => {
              accumulator[entity.type] += 1;
              return accumulator;
            },
            { line: 0, arc: 0, spiral: 0 }
          );

          return AlignmentReportResponseSchema.parse({
            alignment,
            summary: {
              interval,
              sampleCount: stationSamples.length,
              startStation: alignment.startStation,
              endStation: alignment.endStation,
              length: alignment.length,
              entityCount: alignment.entityCount,
              entityTypeBreakdown,
              dependentProfileCount: alignment.dependentProfiles.length,
              dependentCorridorCount: alignment.dependentCorridors.length,
            },
            stationSamples,
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
        let errorMessage = "Failed to execute civil3d_alignment_report";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_alignment_report tool:", error);
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
