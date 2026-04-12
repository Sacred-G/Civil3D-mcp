import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { lookupFrameworkStandards, StandardsLookupArgs } from "../standards/FrameworkStandardsService.js";

const Civil3DStandardsLookupInputSchema = z.object({
  query: z.string().optional().describe("Natural-language standards question or lookup text."),
  topic: z.string().optional().describe("Optional high-level topic such as templates, styles, layers, labels, plotting, or textstyles."),
  tags: z.array(z.string()).optional().describe("Optional tag filters."),
  maxResults: z.number().int().min(1).max(20).optional().describe("Maximum number of results to return."),
});

export async function executeCivil3DStandardsLookup(rawArgs: StandardsLookupArgs) {
  const args = Civil3DStandardsLookupInputSchema.parse(rawArgs);
  return await lookupFrameworkStandards(args);
}

export function registerCivil3DStandardsLookupTool(server: McpServer) {
  server.tool(
    "civil3d_standards_lookup",
    "Looks up Civil 3D standards, template governance, layer/style guidance, and labeling conventions extracted from the Framework documentation in output_clean7.",
    Civil3DStandardsLookupInputSchema.shape,
    async (rawArgs) => {
      try {
        const response = await executeCivil3DStandardsLookup(rawArgs);
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = "Failed to execute civil3d_standards_lookup";
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_standards_lookup tool:", error);
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
