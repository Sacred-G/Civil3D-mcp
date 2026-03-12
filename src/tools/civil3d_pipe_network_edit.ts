import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Point3DSchema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

const Civil3DPipeNetworkEditInputShape = {
  action: z.enum(["create", "add_pipe", "add_structure"]),
  name: z.string().optional(),
  partsList: z.string().optional(),
  referenceSurface: z.string().optional(),
  referenceAlignment: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  networkName: z.string().optional(),
  startPoint: Point3DSchema.optional(),
  endPoint: Point3DSchema.optional(),
  startStructure: z.string().optional(),
  endStructure: z.string().optional(),
  partName: z.string().optional(),
  diameter: z.number().optional(),
  x: z.number().optional(),
  y: z.number().optional(),
  rimElevation: z.number().optional(),
  sumpDepth: z.number().optional(),
};

const Civil3DPipeNetworkEditInputSchema = z.object(Civil3DPipeNetworkEditInputShape);

const PipeNetworkCreateArgsSchema = Civil3DPipeNetworkEditInputSchema.extend({
  action: z.literal("create"),
  name: z.string(),
  partsList: z.string(),
});

const PipeNetworkAddPipeArgsSchema = Civil3DPipeNetworkEditInputSchema.extend({
  action: z.literal("add_pipe"),
  networkName: z.string(),
  partName: z.string(),
}).superRefine((value, ctx) => {
  if (!value.startPoint && !value.startStructure) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Either startPoint or startStructure is required.",
      path: ["startPoint"],
    });
  }

  if (!value.endPoint && !value.endStructure) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Either endPoint or endStructure is required.",
      path: ["endPoint"],
    });
  }
});

const PipeNetworkAddStructureArgsSchema = Civil3DPipeNetworkEditInputSchema.extend({
  action: z.literal("add_structure"),
  networkName: z.string(),
  x: z.number(),
  y: z.number(),
  partName: z.string(),
});

const GenericPipeNetworkEditResponseSchema = z.object({}).passthrough();

export function registerCivil3DPipeNetworkEditTool(server: McpServer) {
  server.tool(
    "civil3d_pipe_network_edit",
    "Creates and modifies Civil 3D pipe networks, pipes, and structures.",
    Civil3DPipeNetworkEditInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "create"
            ? PipeNetworkCreateArgsSchema.parse(args)
            : args.action === "add_pipe"
              ? PipeNetworkAddPipeArgsSchema.parse(args)
              : PipeNetworkAddStructureArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "create") {
            return await appClient.sendCommand("createPipeNetwork", {
              name: parsedArgs.name,
              partsList: parsedArgs.partsList,
              referenceSurface: parsedArgs.referenceSurface,
              referenceAlignment: parsedArgs.referenceAlignment,
              style: parsedArgs.style,
              layer: parsedArgs.layer,
            });
          }

          if (parsedArgs.action === "add_pipe") {
            return await appClient.sendCommand("addPipeToNetwork", {
              networkName: parsedArgs.networkName,
              startPoint: parsedArgs.startPoint,
              endPoint: parsedArgs.endPoint,
              startStructure: parsedArgs.startStructure,
              endStructure: parsedArgs.endStructure,
              partName: parsedArgs.partName,
              diameter: parsedArgs.diameter,
            });
          }

          return await appClient.sendCommand("addStructureToNetwork", {
            networkName: parsedArgs.networkName,
            x: parsedArgs.x,
            y: parsedArgs.y,
            partName: parsedArgs.partName,
            rimElevation: parsedArgs.rimElevation,
            sumpDepth: parsedArgs.sumpDepth,
          });
        });

        const validatedResponse = GenericPipeNetworkEditResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_pipe_network_edit action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_pipe_network_edit tool:", error);
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
