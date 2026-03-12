import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DSectionInputShape = {
  action: z.enum(["list_sample_lines", "get_section_data", "create_sample_lines"]),
  alignmentName: z.string().optional(),
  sampleLineGroupName: z.string().optional(),
  station: z.number().optional(),
  groupName: z.string().optional(),
  stations: z.array(z.number()).optional(),
  interval: z.number().optional(),
  leftWidth: z.number().optional(),
  rightWidth: z.number().optional(),
  surfaces: z.array(z.string()).optional(),
};

const Civil3DSectionInputSchema = z.object(Civil3DSectionInputShape);

const SectionListSampleLinesArgsSchema = Civil3DSectionInputSchema.extend({
  action: z.literal("list_sample_lines"),
  alignmentName: z.string(),
});

const SectionListSampleLinesResponseSchema = z.object({
  sampleLineGroups: z.array(z.object({
    name: z.string(),
    handle: z.string(),
    sampleLineCount: z.number(),
    stations: z.array(z.number()),
  })),
});

const SectionGetDataArgsSchema = Civil3DSectionInputSchema.extend({
  action: z.literal("get_section_data"),
  alignmentName: z.string(),
  sampleLineGroupName: z.string(),
  station: z.number(),
});

const SectionCreateSampleLinesArgsSchema = Civil3DSectionInputSchema.extend({
  action: z.literal("create_sample_lines"),
  alignmentName: z.string(),
  groupName: z.string(),
  stations: z.array(z.number()).optional(),
  interval: z.number().optional(),
  leftWidth: z.number(),
  rightWidth: z.number(),
  surfaces: z.array(z.string()),
}).superRefine((value, ctx) => {
  if (!value.stations && value.interval === undefined) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Either stations or interval is required.",
      path: ["stations"],
    });
  }
});

const SectionDataResponseSchema = z.object({
  station: z.number(),
  surfaces: z.array(z.object({
    surfaceName: z.string(),
    offsets: z.array(z.number()),
    elevations: z.array(z.number()),
  })),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
});

const GenericSectionResponseSchema = z.object({}).passthrough();

export function registerCivil3DSectionTool(server: McpServer) {
  server.tool(
    "civil3d_section",
    "Reads Civil 3D section data and supports sample line creation.",
    Civil3DSectionInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list_sample_lines"
            ? SectionListSampleLinesArgsSchema.parse(args)
            : args.action === "get_section_data"
              ? SectionGetDataArgsSchema.parse(args)
              : SectionCreateSampleLinesArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list_sample_lines") {
            return await appClient.sendCommand("listSampleLineGroups", {
              alignmentName: parsedArgs.alignmentName,
            });
          }

          if (parsedArgs.action === "get_section_data") {
            return await appClient.sendCommand("getSectionData", {
              alignmentName: parsedArgs.alignmentName,
              sampleLineGroupName: parsedArgs.sampleLineGroupName,
              station: parsedArgs.station,
            });
          }

          return await appClient.sendCommand("createSampleLines", {
            alignmentName: parsedArgs.alignmentName,
            groupName: parsedArgs.groupName,
            stations: parsedArgs.stations,
            interval: parsedArgs.interval,
            leftWidth: parsedArgs.leftWidth,
            rightWidth: parsedArgs.rightWidth,
            surfaces: parsedArgs.surfaces,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list_sample_lines"
            ? SectionListSampleLinesResponseSchema.parse(response)
            : parsedArgs.action === "get_section_data"
              ? SectionDataResponseSchema.parse(response)
              : GenericSectionResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_section action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_section tool:", error);
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
