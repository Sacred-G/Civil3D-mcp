import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const Civil3DJobInputShape = {
  action: z.enum(["status", "cancel"]),
  jobId: z.string(),
};

const Civil3DJobResultSchema = z.object({
  jobId: z.string(),
  state: z.enum(["running", "completed", "failed", "cancelled"]),
  progressPercent: z.number().nullable().optional(),
  currentPhase: z.string().nullable().optional(),
  estimatedRemainingSeconds: z.number().nullable().optional(),
  result: z.unknown().nullable().optional(),
});

export function registerCivil3DJobTool(server: McpServer) {
  server.tool(
    "civil3d_job",
    "Checks the status of long-running asynchronous Civil 3D operations or requests cancellation.",
    Civil3DJobInputShape,
    async (args, _extra) => {
      try {
        const response = await withApplicationConnection(async (appClient) => {
          if (args.action === "status") {
            return await appClient.sendCommand("getJobStatus", {
              jobId: args.jobId,
            });
          }

          return await appClient.sendCommand("cancelJob", {
            jobId: args.jobId,
          });
        });

        const validatedResponse = Civil3DJobResultSchema.parse(response);

        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(validatedResponse, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_job action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_job tool:", error);
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
