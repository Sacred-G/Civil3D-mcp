import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

// ─── Shared schemas ───────────────────────────────────────────────────────────

const GenericProfileResponseSchema = z.object({}).passthrough();

const ProfileSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.enum(["surface", "layout", "superimposed"]),
  style: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  minElevation: z.number(),
  maxElevation: z.number(),
});

const ProfileListResponseSchema = z.object({
  alignmentName: z.string(),
  profiles: z.array(ProfileSummarySchema),
});

const ProfileEntitySchema = z.object({
  index: z.number(),
  type: z.enum(["tangent", "circular_curve", "parabola", "asymmetric_parabola"]),
  startStation: z.number(),
  endStation: z.number(),
  startElevation: z.number(),
  endElevation: z.number(),
  grade: z.number().nullable(),
  length: z.number(),
});

const ProfileDetailResponseSchema = z.object({
  name: z.string(),
  handle: z.string(),
  type: z.string(),
  style: z.string(),
  layer: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  minElevation: z.number(),
  maxElevation: z.number(),
  entityCount: z.number(),
  entities: z.array(ProfileEntitySchema),
  pviCount: z.number(),
  units: z.object({
    horizontal: z.string(),
    vertical: z.string(),
  }),
});

const ProfileElevationResponseSchema = z.object({
  station: z.number(),
  elevation: z.number(),
  grade: z.number(),
  units: z.string(),
});

const ProfileSamplePointSchema = z.object({
  station: z.number(),
  elevation: z.number(),
  grade: z.number().nullable(),
});

const ProfileSampleElevationsResponseSchema = z.object({
  alignmentName: z.string(),
  profileName: z.string(),
  startStation: z.number(),
  endStation: z.number(),
  interval: z.number(),
  samples: z.array(ProfileSamplePointSchema),
  units: z.string(),
});

const ProfileReportResponseSchema = z.object({
  profile: ProfileDetailResponseSchema,
  samples: ProfileSampleElevationsResponseSchema,
  summary: z.object({
    sampledStationCount: z.number(),
    startStation: z.number(),
    endStation: z.number(),
    interval: z.number(),
    minimumElevation: z.number(),
    maximumElevation: z.number(),
    elevationRange: z.number(),
    averageGrade: z.number().nullable(),
    totalAscendingLength: z.number(),
    totalDescendingLength: z.number(),
    tangentCount: z.number(),
    curveCount: z.number(),
    pviCount: z.number(),
  }),
});

// ─── Per-action input schemas ─────────────────────────────────────────────────

const ProfileListArgsSchema = z.object({
  action: z.literal("list"),
  alignmentName: z.string(),
});

const ProfileGetArgsSchema = z.object({
  action: z.literal("get"),
  alignmentName: z.string(),
  profileName: z.string(),
});

const ProfileGetElevationArgsSchema = z.object({
  action: z.literal("get_elevation"),
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
});

const ProfileSampleElevationsArgsSchema = z.object({
  action: z.literal("sample_elevations"),
  alignmentName: z.string(),
  profileName: z.string(),
  interval: z.number(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
});

const ProfileCreateFromSurfaceArgsSchema = z.object({
  action: z.literal("create_from_surface"),
  alignmentName: z.string(),
  profileName: z.string(),
  surfaceName: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const ProfileCreateLayoutArgsSchema = z.object({
  action: z.literal("create_layout"),
  alignmentName: z.string(),
  profileName: z.string(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
});

const ProfileDeleteArgsSchema = z.object({
  action: z.literal("delete"),
  alignmentName: z.string(),
  profileName: z.string(),
});

const ProfileReportArgsSchema = z.object({
  action: z.literal("report"),
  alignmentName: z.string(),
  profileName: z.string(),
  interval: z.number().positive().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  maximumSamples: z.number().int().positive().max(400).optional(),
});

const ProfileAddPviArgsSchema = z.object({
  action: z.literal("add_pvi"),
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
  elevation: z.number(),
});

const ProfileDeletePviArgsSchema = z.object({
  action: z.literal("delete_pvi"),
  alignmentName: z.string(),
  profileName: z.string(),
  station: z.number(),
});

const ProfileAddCurveArgsSchema = z.object({
  action: z.literal("add_curve"),
  alignmentName: z.string(),
  profileName: z.string(),
  pviStation: z.number(),
  length: z.number().positive(),
  curveType: z.enum(["symmetric_parabola", "asymmetric_parabola"]).optional().default("symmetric_parabola"),
});

const ProfileSetGradeArgsSchema = z.object({
  action: z.literal("set_grade"),
  alignmentName: z.string(),
  profileName: z.string(),
  entityIndex: z.number().int().min(0),
  grade: z.number(),
});

const ProfileCheckKValuesArgsSchema = z.object({
  action: z.literal("check_k_values"),
  alignmentName: z.string(),
  profileName: z.string(),
  designSpeed: z.number().positive(),
});

const ProfileViewCreateArgsSchema = z.object({
  action: z.literal("view_create"),
  alignmentName: z.string(),
  profileViewName: z.string(),
  insertX: z.number(),
  insertY: z.number(),
  style: z.string().optional(),
  bandSet: z.string().optional(),
});

const ProfileViewBandSetArgsSchema = z.object({
  action: z.literal("view_band_set"),
  profileViewName: z.string(),
  bandSetName: z.string(),
});

// ─── Canonical input shape (union of all action fields) ───────────────────────

const canonicalProfileInputShape = {
  action: z.enum([
    "list",
    "get",
    "get_elevation",
    "sample_elevations",
    "create_from_surface",
    "create_layout",
    "delete",
    "report",
    "add_pvi",
    "delete_pvi",
    "add_curve",
    "set_grade",
    "check_k_values",
    "view_create",
    "view_band_set",
  ]),
  alignmentName: z.string().optional(),
  profileName: z.string().optional(),
  profileViewName: z.string().optional(),
  station: z.number().optional(),
  elevation: z.number().optional(),
  startStation: z.number().optional(),
  endStation: z.number().optional(),
  interval: z.number().optional(),
  maximumSamples: z.number().int().positive().max(400).optional(),
  surfaceName: z.string().optional(),
  style: z.string().optional(),
  layer: z.string().optional(),
  labelSet: z.string().optional(),
  pviStation: z.number().optional(),
  length: z.number().positive().optional(),
  curveType: z.enum(["symmetric_parabola", "asymmetric_parabola"]).optional(),
  entityIndex: z.number().int().min(0).optional(),
  grade: z.number().optional(),
  designSpeed: z.number().positive().optional(),
  insertX: z.number().optional(),
  insertY: z.number().optional(),
  bandSet: z.string().optional(),
  bandSetName: z.string().optional(),
};

// ─── Report helper ────────────────────────────────────────────────────────────

function estimateSampleCount(startStation: number, endStation: number, interval: number) {
  const length = Math.max(0, endStation - startStation);
  return Math.floor(length / interval) + 2;
}

// ─── Domain definition ────────────────────────────────────────────────────────

export const PROFILE_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "profile",
  actions: {
    list: {
      action: "list",
      inputSchema: ProfileListArgsSchema,
      responseSchema: ProfileListResponseSchema,
      capabilities: ["query"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listProfiles"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listProfiles", {
          alignmentName: args.alignmentName,
        }),
      ),
    },
    get: {
      action: "get",
      inputSchema: ProfileGetArgsSchema,
      responseSchema: ProfileDetailResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getProfile"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getProfile", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
        }),
      ),
    },
    get_elevation: {
      action: "get_elevation",
      inputSchema: ProfileGetElevationArgsSchema,
      responseSchema: ProfileElevationResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getProfileElevation"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getProfileElevation", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          station: args.station,
        }),
      ),
    },
    sample_elevations: {
      action: "sample_elevations",
      inputSchema: ProfileSampleElevationsArgsSchema,
      responseSchema: ProfileSampleElevationsResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["sampleProfileElevations"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("sampleProfileElevations", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          startStation: args.startStation,
          endStation: args.endStation,
          interval: args.interval,
        }),
      ),
    },
    create_from_surface: {
      action: "create_from_surface",
      inputSchema: ProfileCreateFromSurfaceArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createProfileFromSurface"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createProfileFromSurface", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          surfaceName: args.surfaceName,
          style: args.style,
          layer: args.layer,
          labelSet: args.labelSet,
        }),
      ),
    },
    create_layout: {
      action: "create_layout",
      inputSchema: ProfileCreateLayoutArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createLayoutProfile"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createLayoutProfile", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          style: args.style,
          layer: args.layer,
          labelSet: args.labelSet,
        }),
      ),
    },
    delete: {
      action: "delete",
      inputSchema: ProfileDeleteArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["delete"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["deleteProfile"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("deleteProfile", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
        }),
      ),
    },
    report: {
      action: "report",
      inputSchema: ProfileReportArgsSchema,
      responseSchema: ProfileReportResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getProfile", "sampleProfileElevations"],
      execute: async (args) => await withApplicationConnection(async (appClient) => {
        const profile = ProfileDetailResponseSchema.parse(
          await appClient.sendCommand("getProfile", {
            alignmentName: args.alignmentName,
            profileName: args.profileName,
          }),
        );

        const startStation = Number(args.startStation ?? profile.startStation);
        const endStation = Number(args.endStation ?? profile.endStation);
        const interval = Number(args.interval ?? Math.max((endStation - startStation) / 20, 25));
        const maximumSamples = Number(args.maximumSamples ?? 150);
        const sampleCount = estimateSampleCount(startStation, endStation, interval);

        if (sampleCount > maximumSamples) {
          throw new Error(
            `Profile report would require ${sampleCount} samples, which exceeds the maximum of ${maximumSamples}. ` +
            "Increase the interval or maximumSamples.",
          );
        }

        const samples = ProfileSampleElevationsResponseSchema.parse(
          await appClient.sendCommand("sampleProfileElevations", {
            alignmentName: args.alignmentName,
            profileName: args.profileName,
            startStation,
            endStation,
            interval,
          }),
        );

        const elevations = samples.samples.map((s) => s.elevation);
        const grades = samples.samples
          .map((s) => s.grade)
          .filter((g): g is number => g !== null);

        const totalAscendingLength = profile.entities
          .filter((e) => (e.grade ?? 0) > 0)
          .reduce((sum, e) => sum + e.length, 0);
        const totalDescendingLength = profile.entities
          .filter((e) => (e.grade ?? 0) < 0)
          .reduce((sum, e) => sum + e.length, 0);
        const tangentCount = profile.entities.filter((e) => e.type === "tangent").length;
        const curveCount = profile.entities.filter((e) => e.type !== "tangent").length;
        const averageGrade = grades.length > 0
          ? grades.reduce((sum, g) => sum + g, 0) / grades.length
          : null;

        return ProfileReportResponseSchema.parse({
          profile,
          samples,
          summary: {
            sampledStationCount: samples.samples.length,
            startStation: samples.startStation,
            endStation: samples.endStation,
            interval: samples.interval,
            minimumElevation: Math.min(...elevations),
            maximumElevation: Math.max(...elevations),
            elevationRange: Math.max(...elevations) - Math.min(...elevations),
            averageGrade,
            totalAscendingLength,
            totalDescendingLength,
            tangentCount,
            curveCount,
            pviCount: profile.pviCount,
          },
        });
      }),
    },
    add_pvi: {
      action: "add_pvi",
      inputSchema: ProfileAddPviArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileAddPvi"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileAddPvi", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          station: args.station,
          elevation: args.elevation,
        }),
      ),
    },
    delete_pvi: {
      action: "delete_pvi",
      inputSchema: ProfileDeletePviArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["delete", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileDeletePvi"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileDeletePvi", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          station: args.station,
        }),
      ),
    },
    add_curve: {
      action: "add_curve",
      inputSchema: ProfileAddCurveArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileAddCurve"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileAddCurve", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          pviStation: args.pviStation,
          length: args.length,
          curveType: args.curveType,
        }),
      ),
    },
    set_grade: {
      action: "set_grade",
      inputSchema: ProfileSetGradeArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileSetGrade"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileSetGrade", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          entityIndex: args.entityIndex,
          grade: args.grade,
        }),
      ),
    },
    check_k_values: {
      action: "check_k_values",
      inputSchema: ProfileCheckKValuesArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["analyze", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["profileCheckKValues"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileCheckKValues", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          designSpeed: args.designSpeed,
        }),
      ),
    },
    view_create: {
      action: "view_create",
      inputSchema: ProfileViewCreateArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileViewCreate"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileViewCreate", {
          alignmentName: args.alignmentName,
          profileViewName: args.profileViewName,
          insertX: args.insertX,
          insertY: args.insertY,
          style: args.style,
          bandSet: args.bandSet,
        }),
      ),
    },
    view_band_set: {
      action: "view_band_set",
      inputSchema: ProfileViewBandSetArgsSchema,
      responseSchema: GenericProfileResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["profileViewBandSet"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("profileViewBandSet", {
          profileViewName: args.profileViewName,
          bandSetName: args.bandSetName,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_profile",
      displayName: "Civil 3D Profile",
      description: "Reads Civil 3D vertical profiles, reports geometry, and manages creation, editing, and deletion through a single domain tool.",
      inputShape: canonicalProfileInputShape,
      supportedActions: [
        "list",
        "get",
        "get_elevation",
        "sample_elevations",
        "create_from_surface",
        "create_layout",
        "delete",
        "report",
        "add_pvi",
        "delete_pvi",
        "add_curve",
        "set_grade",
        "check_k_values",
        "view_create",
        "view_band_set",
      ],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_profile_report",
      displayName: "Civil 3D Profile Report",
      description: "Builds a structured profile report by fetching profile detail and sampling elevations along the profile range.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        interval: z.number().positive().optional(),
        startStation: z.number().optional(),
        endStation: z.number().optional(),
        maximumSamples: z.number().int().positive().max(400).optional(),
      },
      supportedActions: ["report"],
      resolveAction: (rawArgs) => ({
        action: "report",
        args: {
          action: "report",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          interval: rawArgs.interval,
          startStation: rawArgs.startStation,
          endStation: rawArgs.endStation,
          maximumSamples: rawArgs.maximumSamples,
        },
      }),
    },
    {
      toolName: "civil3d_profile_add_pvi",
      displayName: "Civil 3D Profile Add PVI",
      description: "Adds a PVI (Point of Vertical Intersection) to a Civil 3D layout profile at the specified station and elevation.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        station: z.number(),
        elevation: z.number(),
      },
      supportedActions: ["add_pvi"],
      resolveAction: (rawArgs) => ({
        action: "add_pvi",
        args: {
          action: "add_pvi",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          station: rawArgs.station,
          elevation: rawArgs.elevation,
        },
      }),
    },
    {
      toolName: "civil3d_profile_delete_pvi",
      displayName: "Civil 3D Profile Delete PVI",
      description: "Deletes the PVI nearest to the specified station from a Civil 3D layout profile.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        station: z.number(),
      },
      supportedActions: ["delete_pvi"],
      resolveAction: (rawArgs) => ({
        action: "delete_pvi",
        args: {
          action: "delete_pvi",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          station: rawArgs.station,
        },
      }),
    },
    {
      toolName: "civil3d_profile_add_curve",
      displayName: "Civil 3D Profile Add Curve",
      description: "Adds a symmetric or asymmetric parabolic vertical curve at an existing PVI in a Civil 3D layout profile.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        pviStation: z.number(),
        length: z.number().positive(),
        curveType: z.enum(["symmetric_parabola", "asymmetric_parabola"]).optional().default("symmetric_parabola"),
      },
      supportedActions: ["add_curve"],
      resolveAction: (rawArgs) => ({
        action: "add_curve",
        args: {
          action: "add_curve",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          pviStation: rawArgs.pviStation,
          length: rawArgs.length,
          curveType: rawArgs.curveType,
        },
      }),
    },
    {
      toolName: "civil3d_profile_set_grade",
      displayName: "Civil 3D Profile Set Grade",
      description: "Sets the grade (slope) of a tangent entity in a Civil 3D layout profile. Grade is expressed as a decimal fraction (0.02 = 2%).",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        entityIndex: z.number().int().min(0),
        grade: z.number(),
      },
      supportedActions: ["set_grade"],
      resolveAction: (rawArgs) => ({
        action: "set_grade",
        args: {
          action: "set_grade",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          entityIndex: rawArgs.entityIndex,
          grade: rawArgs.grade,
        },
      }),
    },
    {
      toolName: "civil3d_profile_get_elevation",
      displayName: "Civil 3D Profile Get Elevation",
      description: "Samples the elevation and instantaneous grade of a Civil 3D profile at a given station.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        station: z.number(),
      },
      supportedActions: ["get_elevation"],
      resolveAction: (rawArgs) => ({
        action: "get_elevation",
        args: {
          action: "get_elevation",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          station: rawArgs.station,
        },
      }),
    },
    {
      toolName: "civil3d_profile_check_k_values",
      displayName: "Civil 3D Profile Check K Values",
      description: "Validates the K value of every vertical curve in a Civil 3D layout profile against AASHTO minimum K values for the specified design speed. Returns a pass/fail report per curve.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        designSpeed: z.number().positive(),
      },
      supportedActions: ["check_k_values"],
      resolveAction: (rawArgs) => ({
        action: "check_k_values",
        args: {
          action: "check_k_values",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          designSpeed: rawArgs.designSpeed,
        },
      }),
    },
    {
      toolName: "civil3d_profile_view_create",
      displayName: "Civil 3D Profile View Create",
      description: "Creates a Civil 3D profile view at the specified insertion point in model space. Optionally applies a style and band set.",
      inputShape: {
        alignmentName: z.string(),
        profileViewName: z.string(),
        insertX: z.number(),
        insertY: z.number(),
        style: z.string().optional(),
        bandSet: z.string().optional(),
      },
      supportedActions: ["view_create"],
      resolveAction: (rawArgs) => ({
        action: "view_create",
        args: {
          action: "view_create",
          alignmentName: rawArgs.alignmentName,
          profileViewName: rawArgs.profileViewName,
          insertX: rawArgs.insertX,
          insertY: rawArgs.insertY,
          style: rawArgs.style,
          bandSet: rawArgs.bandSet,
        },
      }),
    },
    {
      toolName: "civil3d_profile_view_band_set",
      displayName: "Civil 3D Profile View Band Set",
      description: "Applies a band set style to an existing Civil 3D profile view, updating the data bands displayed above and below the grid.",
      inputShape: {
        profileViewName: z.string(),
        bandSetName: z.string(),
      },
      supportedActions: ["view_band_set"],
      resolveAction: (rawArgs) => ({
        action: "view_band_set",
        args: {
          action: "view_band_set",
          profileViewName: rawArgs.profileViewName,
          bandSetName: rawArgs.bandSetName,
        },
      }),
    },
  ],
};
