import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DPipeNetworkInputShape = {
  action: z.enum(["list", "get", "get_pipe", "get_structure", "check_interference"]),
  name: z.string().optional(),
  networkName: z.string().optional(),
  pipeName: z.string().optional(),
  structureName: z.string().optional(),
  targetType: z.enum(["surface", "pipe_network"]).optional(),
  targetName: z.string().optional(),
};

const Civil3DPipeNetworkInputSchema = z.object(Civil3DPipeNetworkInputShape);

const PipeNetworkListArgsSchema = Civil3DPipeNetworkInputSchema.extend({
  action: z.literal("list"),
});

const PipeNetworkGetArgsSchema = Civil3DPipeNetworkInputSchema.extend({
  action: z.literal("get"),
  name: z.string(),
});

const PipeNetworkGetPipeArgsSchema = Civil3DPipeNetworkInputSchema.extend({
  action: z.literal("get_pipe"),
  networkName: z.string(),
  pipeName: z.string(),
});

const PipeNetworkGetStructureArgsSchema = Civil3DPipeNetworkInputSchema.extend({
  action: z.literal("get_structure"),
  networkName: z.string(),
  structureName: z.string(),
});

const PipeNetworkCheckInterferenceArgsSchema = Civil3DPipeNetworkInputSchema.extend({
  action: z.literal("check_interference"),
  networkName: z.string(),
  targetType: z.enum(["surface", "pipe_network"]),
  targetName: z.string(),
});

const PipeNetworkSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  pipeCount: z.number(),
  structureCount: z.number(),
  surface: z.string().nullable(),
});

const PipeSchema = z.object({
  name: z.string(),
  handle: z.string(),
  startStructure: z.string().nullable(),
  endStructure: z.string().nullable(),
  length: z.number(),
  diameter: z.number(),
  slope: z.number(),
  material: z.string(),
  invertIn: z.number(),
  invertOut: z.number(),
});

const StructureSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  rimElevation: z.number(),
  sumpElevation: z.number(),
  x: z.number(),
  y: z.number(),
  connectedPipes: z.array(z.string()),
});

const PipeNetworkListResponseSchema = z.object({
  networks: z.array(PipeNetworkSummarySchema),
});

const PipeNetworkDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  style: z.string(),
  referenceSurface: z.string().nullable(),
  referenceAlignment: z.string().nullable(),
  pipes: z.array(PipeSchema),
  structures: z.array(StructureSchema),
});

const PipeDetailResponseSchema = PipeSchema;
const StructureDetailResponseSchema = StructureSchema;

const InterferenceSchema = z.object({
  objectA: z.string(),
  objectB: z.string(),
  location: z.object({
    x: z.number(),
    y: z.number(),
    z: z.number(),
  }),
  clearance: z.number(),
  conflictType: z.enum(["crossing", "proximity", "cover_violation"]),
});

const PipeNetworkInterferenceResponseSchema = z.object({
  interferences: z.array(InterferenceSchema),
  totalConflicts: z.number(),
});

export function registerCivil3DPipeNetworkTool(server: McpServer) {
  server.tool(
    "civil3d_pipe_network",
    "Reads Civil 3D pipe network data including networks, pipes, structures, and interference checks.",
    Civil3DPipeNetworkInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? PipeNetworkListArgsSchema.parse(args)
            : args.action === "get"
              ? PipeNetworkGetArgsSchema.parse(args)
              : args.action === "get_pipe"
                ? PipeNetworkGetPipeArgsSchema.parse(args)
                : args.action === "get_structure"
                  ? PipeNetworkGetStructureArgsSchema.parse(args)
                  : PipeNetworkCheckInterferenceArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listPipeNetworks", {});
          }

          if (parsedArgs.action === "get") {
            return await appClient.sendCommand("getPipeNetwork", {
              name: parsedArgs.name,
            });
          }

          if (parsedArgs.action === "get_pipe") {
            return await appClient.sendCommand("getPipe", {
              networkName: parsedArgs.networkName,
              pipeName: parsedArgs.pipeName,
            });
          }

          if (parsedArgs.action === "get_structure") {
            return await appClient.sendCommand("getStructure", {
              networkName: parsedArgs.networkName,
              structureName: parsedArgs.structureName,
            });
          }

          return await appClient.sendCommand("checkPipeNetworkInterference", {
            networkName: parsedArgs.networkName,
            targetType: parsedArgs.targetType,
            targetName: parsedArgs.targetName,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? PipeNetworkListResponseSchema.parse(response)
            : parsedArgs.action === "get"
              ? PipeNetworkDetailResponseSchema.parse(response)
              : parsedArgs.action === "get_pipe"
                ? PipeDetailResponseSchema.parse(response)
                : parsedArgs.action === "get_structure"
                  ? StructureDetailResponseSchema.parse(response)
                  : PipeNetworkInterferenceResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_pipe_network action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_pipe_network tool:", error);
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
