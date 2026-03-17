import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { withApplicationConnection } from "../utils/ConnectionManager.js";

const GenericResponseSchema = z.object({}).passthrough();

function errorResult(toolName: string, error: unknown) {
  const msg = error instanceof Error ? error.message : String(error);
  console.error(`Error in ${toolName}:`, error);
  return {
    content: [{ type: "text" as const, text: `${toolName} failed: ${msg}` }],
    isError: true,
  };
}

// ─── 9. civil3d_profile_add_pvi ──────────────────────────────────────────────

const ProfileAddPviInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the layout profile"),
  station: z.number().describe("Station of the new PVI"),
  elevation: z.number().describe("Elevation of the new PVI"),
};

// ─── 10. civil3d_profile_delete_pvi ──────────────────────────────────────────

const ProfileDeletePviInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the layout profile"),
  station: z
    .number()
    .describe("Station of the PVI to delete (nearest PVI within tolerance is used)"),
};

// ─── 11. civil3d_profile_add_curve ───────────────────────────────────────────

const ProfileAddCurveInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the layout profile"),
  pviStation: z.number().describe("Station of the PVI where the curve will be placed"),
  length: z.number().positive().describe("Vertical curve length"),
  curveType: z
    .enum(["symmetric_parabola", "asymmetric_parabola"])
    .optional()
    .default("symmetric_parabola")
    .describe("Vertical curve type (default: symmetric_parabola)"),
};

// ─── 12. civil3d_profile_set_grade ───────────────────────────────────────────

const ProfileSetGradeInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the layout profile"),
  entityIndex: z
    .number()
    .int()
    .min(0)
    .describe("Zero-based index of the tangent entity"),
  grade: z
    .number()
    .describe("Target grade as a decimal fraction (e.g. 0.02 = 2%)"),
};

// ─── 13. civil3d_profile_get_elevation ───────────────────────────────────────

const ProfileGetElevationInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the profile"),
  station: z.number().describe("Station at which to sample the elevation"),
};

// ─── 14. civil3d_profile_check_k_values ──────────────────────────────────────

const ProfileCheckKValuesInputShape = {
  alignmentName: z.string().describe("Name of the parent alignment"),
  profileName: z.string().describe("Name of the layout profile to check"),
  designSpeed: z
    .number()
    .positive()
    .describe("Design speed in km/h (or mph if drawing is in imperial units)"),
};

// ─── 15. civil3d_profile_view_create ─────────────────────────────────────────

const ProfileViewCreateInputShape = {
  alignmentName: z.string().describe("Name of the alignment whose profiles will be displayed"),
  profileViewName: z.string().describe("Name for the new profile view"),
  insertX: z.number().describe("X coordinate of the profile view insertion point"),
  insertY: z.number().describe("Y coordinate of the profile view insertion point"),
  style: z
    .string()
    .optional()
    .describe("Profile view style name (optional — uses default if omitted)"),
  bandSet: z
    .string()
    .optional()
    .describe("Band set name to apply (optional)"),
};

// ─── 16. civil3d_profile_view_band_set ───────────────────────────────────────

const ProfileViewBandSetInputShape = {
  profileViewName: z.string().describe("Name of the profile view to modify"),
  bandSetName: z.string().describe("Name of the band set style to apply"),
};

// ─── Registration ─────────────────────────────────────────────────────────────

export function registerCivil3DProfileEditingTools(server: McpServer) {
  // 9. add_pvi
  server.tool(
    "civil3d_profile_add_pvi",
    "Adds a PVI (Point of Vertical Intersection) to a Civil 3D layout profile at the specified station and elevation.",
    ProfileAddPviInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileAddPviInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileAddPvi", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            station: parsed.station,
            elevation: parsed.elevation,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_add_pvi", error);
      }
    },
  );

  // 10. delete_pvi
  server.tool(
    "civil3d_profile_delete_pvi",
    "Deletes the PVI nearest to the specified station from a Civil 3D layout profile.",
    ProfileDeletePviInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileDeletePviInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileDeletePvi", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            station: parsed.station,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_delete_pvi", error);
      }
    },
  );

  // 11. add_curve
  server.tool(
    "civil3d_profile_add_curve",
    "Adds a symmetric or asymmetric parabolic vertical curve at an existing PVI in a Civil 3D layout profile.",
    ProfileAddCurveInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileAddCurveInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileAddCurve", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            pviStation: parsed.pviStation,
            length: parsed.length,
            curveType: parsed.curveType,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_add_curve", error);
      }
    },
  );

  // 12. set_grade
  server.tool(
    "civil3d_profile_set_grade",
    "Sets the grade (slope) of a tangent entity in a Civil 3D layout profile. Grade is expressed as a decimal fraction (0.02 = 2%).",
    ProfileSetGradeInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileSetGradeInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileSetGrade", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            entityIndex: parsed.entityIndex,
            grade: parsed.grade,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_set_grade", error);
      }
    },
  );

  // 13. get_elevation
  server.tool(
    "civil3d_profile_get_elevation",
    "Samples the elevation and instantaneous grade of a Civil 3D profile at a given station. High-value tool for AI geometric reasoning.",
    ProfileGetElevationInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileGetElevationInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileGetElevation", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            station: parsed.station,
          }),
        );
        const result = z
          .object({
            station: z.number(),
            elevation: z.number(),
            grade: z.number(),
            units: z.string(),
          })
          .parse(response);
        return {
          content: [{ type: "text", text: JSON.stringify(result, null, 2) }],
        };
      } catch (error) {
        return errorResult("civil3d_profile_get_elevation", error);
      }
    },
  );

  // 14. check_k_values
  server.tool(
    "civil3d_profile_check_k_values",
    "Validates the K value of every vertical curve in a Civil 3D layout profile against AASHTO minimum K values for the specified design speed. Returns a pass/fail report per curve.",
    ProfileCheckKValuesInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileCheckKValuesInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileCheckKValues", {
            alignmentName: parsed.alignmentName,
            profileName: parsed.profileName,
            designSpeed: parsed.designSpeed,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_check_k_values", error);
      }
    },
  );

  // 15. profile_view_create
  server.tool(
    "civil3d_profile_view_create",
    "Creates a Civil 3D profile view at the specified insertion point in model space. Optionally applies a style and band set.",
    ProfileViewCreateInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileViewCreateInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileViewCreate", {
            alignmentName: parsed.alignmentName,
            profileViewName: parsed.profileViewName,
            insertX: parsed.insertX,
            insertY: parsed.insertY,
            style: parsed.style,
            bandSet: parsed.bandSet,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_view_create", error);
      }
    },
  );

  // 16. profile_view_band_set
  server.tool(
    "civil3d_profile_view_band_set",
    "Applies a band set style to an existing Civil 3D profile view, updating the data bands displayed above and below the grid.",
    ProfileViewBandSetInputShape,
    async (args, _extra) => {
      try {
        const parsed = z.object(ProfileViewBandSetInputShape).parse(args);
        const response = await withApplicationConnection((appClient) =>
          appClient.sendCommand("profileViewBandSet", {
            profileViewName: parsed.profileViewName,
            bandSetName: parsed.bandSetName,
          }),
        );
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(
                GenericResponseSchema.parse(response),
                null,
                2,
              ),
            },
          ],
        };
      } catch (error) {
        return errorResult("civil3d_profile_view_band_set", error);
      }
    },
  );
}
