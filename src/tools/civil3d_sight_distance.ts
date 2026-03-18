import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return { content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }], isError: true };
}

const GenericResponseSchema = z.object({}).passthrough();

// ─── 1. civil3d_sight_distance_calculate ────────────────────────────────────

export const SightDistanceCalculateInputSchema = z.object({
  designSpeed: z.number().positive().describe("Design speed in km/h (e.g. 80 for 80 km/h) or mph — specify units via speedUnits"),
  speedUnits: z.enum(["kmh", "mph"]).optional().describe("Speed units: 'kmh' (default) or 'mph'"),
  sightDistanceType: z.enum(["stopping", "passing", "decision"]).describe("Type of sight distance: 'stopping' (SSD), 'passing' (PSD), or 'decision' (DSD)"),
  grade: z.number().optional().describe("Longitudinal grade in percent at the point of interest (positive = upgrade, negative = downgrade). Used to adjust SSD for grade. Default: 0%"),
  frictionCoefficient: z.number().positive().optional().describe("Longitudinal friction coefficient (AASHTO Table 3-1). If omitted, the AASHTO default for the design speed is used."),
  perceptionReactionTime: z.number().positive().optional().describe("Perception-reaction time in seconds (default: 2.5 s per AASHTO)"),
  standard: z.enum(["AASHTO", "FHWA", "HCM"]).optional().describe("Design standard to apply (default: AASHTO Green Book)"),
  alignmentName: z.string().optional().describe("If provided, check SSD against the available sight distance along this alignment"),
  profileName: z.string().optional().describe("Profile to use when checking sight distance along the alignment"),
  checkStation: z.number().optional().describe("Specific station (in drawing units) to evaluate sight distance at"),
});

const SightDistanceCalculateInputShape = SightDistanceCalculateInputSchema.shape;

// ─── 2. civil3d_stopping_distance_check ─────────────────────────────────────

export const StoppingDistanceCheckInputSchema = z.object({
  alignmentName: z.string().describe("Name of the alignment to check stopping sight distance along"),
  profileName: z.string().describe("Name of the design profile on the alignment"),
  designSpeed: z.number().positive().describe("Design speed in km/h"),
  stationStart: z.number().optional().describe("Start station for the check range (defaults to alignment start)"),
  stationEnd: z.number().optional().describe("End station for the check range (defaults to alignment end)"),
  stationInterval: z.number().positive().optional().describe("Station interval for checking (default: 25 m or 50 ft)"),
  standard: z.enum(["AASHTO", "FHWA"]).optional().describe("Design standard (default: AASHTO)"),
});

const StoppingDistanceCheckInputShape = StoppingDistanceCheckInputSchema.shape;

// ─── Registration ────────────────────────────────────────────────────────────

export function registerCivil3DSightDistanceTools(server: McpServer) {

  // 1. civil3d_sight_distance_calculate
  server.tool(
    "civil3d_sight_distance_calculate",
    "Calculate AASHTO stopping sight distance (SSD), passing sight distance (PSD), or decision sight distance (DSD) for a given design speed and grade. Returns required sight distance in feet and meters, the minimum K-value for vertical curves, and the corresponding crest/sag curve parameters. Optionally checks the available sight distance at a specific station on a Civil 3D alignment/profile.",
    SightDistanceCalculateInputShape,
    async (args) => {
      try {
        const parsed = SightDistanceCalculateInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("calculateSightDistance", {
            designSpeed: parsed.designSpeed,
            speedUnits: parsed.speedUnits ?? "kmh",
            sightDistanceType: parsed.sightDistanceType,
            grade: parsed.grade ?? 0,
            frictionCoefficient: parsed.frictionCoefficient ?? null,
            perceptionReactionTime: parsed.perceptionReactionTime ?? 2.5,
            standard: parsed.standard ?? "AASHTO",
            alignmentName: parsed.alignmentName ?? null,
            profileName: parsed.profileName ?? null,
            checkStation: parsed.checkStation ?? null,
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_sight_distance_calculate", error);
      }
    }
  );

  // 2. civil3d_stopping_distance_check
  server.tool(
    "civil3d_stopping_distance_check",
    "Check stopping sight distance (SSD) compliance along an entire Civil 3D alignment and profile at a specified station interval. Returns a table of stations where the available SSD (based on crest/sag vertical curve K-values and horizontal curve geometry) falls below the AASHTO required SSD for the design speed. Flags non-compliant stations with the deficiency amount.",
    StoppingDistanceCheckInputShape,
    async (args) => {
      try {
        const parsed = StoppingDistanceCheckInputSchema.parse(args);
        const response = await withApplicationConnection(async (appClient) => {
          return await appClient.sendCommand("checkStoppingDistance", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            designSpeed: parsed.designSpeed,
            stationStart: parsed.stationStart ?? null,
            stationEnd: parsed.stationEnd ?? null,
            stationInterval: parsed.stationInterval ?? 25,
            standard: parsed.standard ?? "AASHTO",
          });
        });
        const validated = GenericResponseSchema.parse(response);
        return { content: [{ type: "text", text: JSON.stringify(validated, null, 2) }] };
      } catch (error) {
        return errorResult("civil3d_stopping_distance_check", error);
      }
    }
  );
}
