import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

// ─── Input Schema ───────────────────────────────────────────
const TcInputShape = {
  action: z.enum([
    "list_tc_methods",
    "calculate_tc",
    "generate_hydrograph",
  ]),
  // Tc calculation parameters
  method: z.string().optional(),
  flowLength_ft: z.number().positive().optional(),
  elevationDifference_ft: z.number().positive().optional(),
  manningsN: z.number().positive().optional(),
  rainfall2yr24hr_in: z.number().positive().optional(),
  slope_ftPerFt: z.number().positive().optional(),
  slope_percent: z.number().positive().optional(),
  surfaceType: z.enum(["paved", "unpaved"]).optional(),
  hydraulicRadius_ft: z.number().positive().optional(),
  runoffCoefficient: z.number().min(0).max(1).optional(),
  curveNumber: z.number().min(1).max(100).optional(),
  // Hydrograph parameters
  drainageArea_mi2: z.number().positive().optional(),
  runoffDepth_in: z.number().positive().optional(),
  timeOfConcentration_hr: z.number().positive().optional(),
  stormDuration_hr: z.number().positive().optional(),
};

const TcInputSchema = z.object(TcInputShape);
type TcInput = z.infer<typeof TcInputSchema>;

// ─── Action-specific schemas ────────────────────────────────
const ListTcMethodsSchema = TcInputSchema.extend({
  action: z.literal("list_tc_methods"),
});

const CalculateTcSchema = TcInputSchema.extend({
  action: z.literal("calculate_tc"),
  method: z.string(),
});

const GenerateHydrographSchema = TcInputSchema.extend({
  action: z.literal("generate_hydrograph"),
  drainageArea_mi2: z.number().positive(),
  runoffDepth_in: z.number().positive(),
  timeOfConcentration_hr: z.number().positive(),
});

// ─── Registration ───────────────────────────────────────────
export function registerCivil3DTimeOfConcentrationTools(server: McpServer) {
  server.tool(
    "civil3d_time_of_concentration",
    "Calculates Time of Concentration (Tc) using standard methods (Kirpich, TR-55 sheet/shallow/channel, FAA, NRCS Lag) and generates SCS unit hydrographs (triangular or curvilinear). Actions: list_tc_methods, calculate_tc, generate_hydrograph.",
    TcInputShape,
    async (args: TcInput, _extra) => {
      try {
        let parsedArgs: TcInput;
        switch (args.action) {
          case "list_tc_methods":
            parsedArgs = ListTcMethodsSchema.parse(args);
            break;
          case "calculate_tc":
            parsedArgs = CalculateTcSchema.parse(args);
            break;
          case "generate_hydrograph":
            parsedArgs = GenerateHydrographSchema.parse(args);
            break;
          default:
            throw new Error(`Unknown action: ${args.action}`);
        }

        const response = await withApplicationConnection(async (appClient) => {
          if (parsedArgs.action === "list_tc_methods") {
            return await appClient.sendCommand("listTcMethods", {});
          }

          if (parsedArgs.action === "calculate_tc") {
            // Build params, passing through all relevant fields
            const params: Record<string, unknown> = {
              method: parsedArgs.method,
            };
            if (parsedArgs.flowLength_ft !== undefined) params.flowLength_ft = parsedArgs.flowLength_ft;
            if (parsedArgs.elevationDifference_ft !== undefined) params.elevationDifference_ft = parsedArgs.elevationDifference_ft;
            if (parsedArgs.manningsN !== undefined) params.manningsN = parsedArgs.manningsN;
            if (parsedArgs.rainfall2yr24hr_in !== undefined) params.rainfall2yr24hr_in = parsedArgs.rainfall2yr24hr_in;
            if (parsedArgs.slope_ftPerFt !== undefined) params.slope_ftPerFt = parsedArgs.slope_ftPerFt;
            if (parsedArgs.slope_percent !== undefined) params.slope_percent = parsedArgs.slope_percent;
            if (parsedArgs.surfaceType !== undefined) params.surfaceType = parsedArgs.surfaceType;
            if (parsedArgs.hydraulicRadius_ft !== undefined) params.hydraulicRadius_ft = parsedArgs.hydraulicRadius_ft;
            if (parsedArgs.runoffCoefficient !== undefined) params.runoffCoefficient = parsedArgs.runoffCoefficient;
            if (parsedArgs.curveNumber !== undefined) params.curveNumber = parsedArgs.curveNumber;

            return await appClient.sendCommand("calculateTimeOfConcentration", params);
          }

          // generate_hydrograph
          const params: Record<string, unknown> = {
            drainageArea_mi2: parsedArgs.drainageArea_mi2,
            runoffDepth_in: parsedArgs.runoffDepth_in,
            timeOfConcentration_hr: parsedArgs.timeOfConcentration_hr,
          };
          if (parsedArgs.method !== undefined) params.method = parsedArgs.method;
          if (parsedArgs.stormDuration_hr !== undefined) params.stormDuration_hr = parsedArgs.stormDuration_hr;

          return await appClient.sendCommand("generateHydrograph", params);
        });

        return {
          content: [
            {
              type: "text" as const,
              text: JSON.stringify(response, null, 2),
            },
          ],
        };
      } catch (error) {
        let errorMessage = `Failed to execute civil3d_time_of_concentration action '${args.action}'`;
        if (error instanceof Error) {
          errorMessage += `: ${error.message}`;
        } else if (typeof error === "string") {
          errorMessage += `: ${error}`;
        }
        console.error("Error in civil3d_time_of_concentration tool:", error);
        return {
          content: [
            {
              type: "text" as const,
              text: errorMessage,
            },
          ],
          isError: true,
        };
      }
    }
  );
}
