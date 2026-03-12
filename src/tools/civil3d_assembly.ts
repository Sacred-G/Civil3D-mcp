import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DAssemblyInputShape = {
  action: z.enum(["list", "get"]),
  name: z.string().optional(),
};

const Civil3DAssemblyInputSchema = z.object(Civil3DAssemblyInputShape);

const AssemblyListArgsSchema = Civil3DAssemblyInputSchema.extend({
  action: z.literal("list"),
});

const AssemblyGetArgsSchema = Civil3DAssemblyInputSchema.extend({
  action: z.literal("get"),
  name: z.string(),
});

const AssemblySummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  subassemblyCount: z.number(),
  usedByCorridors: z.array(z.string()),
});

const AssemblyListResponseSchema = z.object({
  assemblies: z.array(AssemblySummarySchema),
});

const AssemblyParameterValueSchema = z.union([z.number(), z.string(), z.boolean()]);

const SubassemblySchema = z.object({
  name: z.string(),
  side: z.enum(["left", "right", "none"]),
  className: z.string(),
  parameters: z.record(AssemblyParameterValueSchema),
});

const AssemblyDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  style: z.string(),
  subassemblies: z.array(SubassemblySchema),
  usedByCorridors: z.array(z.string()),
});

export function registerCivil3DAssemblyTool(server: McpServer) {
  server.tool(
    "civil3d_assembly",
    "Lists and inspects Civil 3D assemblies and their subassemblies.",
    Civil3DAssemblyInputShape,
    async (args, _extra) => {
      try {
        const parsedArgs =
          args.action === "list"
            ? AssemblyListArgsSchema.parse(args)
            : AssemblyGetArgsSchema.parse(args);

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list") {
            return await appClient.sendCommand("listAssemblies", {});
          }

          return await appClient.sendCommand("getAssembly", {
            name: parsedArgs.name,
          });
        });

        const validatedResponse =
          parsedArgs.action === "list"
            ? AssemblyListResponseSchema.parse(response)
            : AssemblyDetailResponseSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_assembly action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_assembly tool:", error);
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
