import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const GenericQcResponseSchema = z.object({}).passthrough();

const canonicalQcInputShape = {
  action: z.enum([
    "check_alignment",
    "check_profile",
    "check_corridor",
    "check_pipe_network",
    "check_surface",
    "generate_report",
  ]),
  name: z.string().optional(),
  designSpeed: z.number().optional(),
  checkTangents: z.boolean().optional(),
  checkCurves: z.boolean().optional(),
  checkSpirals: z.boolean().optional(),
  alignmentName: z.string().optional(),
  profileName: z.string().optional(),
  maxGrade: z.number().optional(),
  minKValue: z.number().optional(),
  minCover: z.number().optional(),
  maxSlope: z.number().optional(),
  minSlope: z.number().optional(),
  minVelocity: z.number().optional(),
  maxVelocity: z.number().optional(),
  spikeThreshold: z.number().optional(),
  flatTriangleThreshold: z.number().optional(),
  outputPath: z.string().optional(),
  includeAlignments: z.boolean().optional(),
  includeProfiles: z.boolean().optional(),
  includeCorridors: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includeSurfaces: z.boolean().optional(),
  includeLabels: z.boolean().optional(),
};

const QcAlignmentArgsSchema = z.object({
  action: z.literal("check_alignment"),
  name: z.string(),
  designSpeed: z.number().positive().optional(),
  checkTangents: z.boolean().optional(),
  checkCurves: z.boolean().optional(),
  checkSpirals: z.boolean().optional(),
});

const QcProfileArgsSchema = z.object({
  action: z.literal("check_profile"),
  alignmentName: z.string(),
  profileName: z.string(),
  maxGrade: z.number().positive().optional(),
  minKValue: z.number().positive().optional(),
});

const QcCorridorArgsSchema = z.object({
  action: z.literal("check_corridor"),
  name: z.string(),
});

const QcPipeNetworkArgsSchema = z.object({
  action: z.literal("check_pipe_network"),
  name: z.string(),
  minCover: z.number().nonnegative().optional(),
  maxSlope: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
});

const QcSurfaceArgsSchema = z.object({
  action: z.literal("check_surface"),
  name: z.string(),
  spikeThreshold: z.number().positive().optional(),
  flatTriangleThreshold: z.number().nonnegative().optional(),
});

const QcReportArgsSchema = z.object({
  action: z.literal("generate_report"),
  outputPath: z.string(),
  includeAlignments: z.boolean().optional(),
  includeProfiles: z.boolean().optional(),
  includeCorridors: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includeSurfaces: z.boolean().optional(),
  includeLabels: z.boolean().optional(),
});

export const QC_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "qc",
  actions: {
    check_alignment: {
      action: "check_alignment",
      inputSchema: QcAlignmentArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckAlignment"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckAlignment", {
          name: args.name,
          designSpeed: args.designSpeed ?? null,
          checkTangents: args.checkTangents ?? true,
          checkCurves: args.checkCurves ?? true,
          checkSpirals: args.checkSpirals ?? true,
        }),
      ),
    },
    check_profile: {
      action: "check_profile",
      inputSchema: QcProfileArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckProfile"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckProfile", {
          alignmentName: args.alignmentName,
          profileName: args.profileName,
          maxGrade: args.maxGrade ?? null,
          minKValue: args.minKValue ?? null,
        }),
      ),
    },
    check_corridor: {
      action: "check_corridor",
      inputSchema: QcCorridorArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckCorridor"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckCorridor", { name: args.name }),
      ),
    },
    check_pipe_network: {
      action: "check_pipe_network",
      inputSchema: QcPipeNetworkArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckPipeNetwork"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckPipeNetwork", {
          name: args.name,
          minCover: args.minCover ?? null,
          maxSlope: args.maxSlope ?? null,
          minSlope: args.minSlope ?? null,
          minVelocity: args.minVelocity ?? null,
          maxVelocity: args.maxVelocity ?? null,
        }),
      ),
    },
    check_surface: {
      action: "check_surface",
      inputSchema: QcSurfaceArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckSurface"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckSurface", {
          name: args.name,
          spikeThreshold: args.spikeThreshold ?? null,
          flatTriangleThreshold: args.flatTriangleThreshold ?? null,
        }),
      ),
    },
    generate_report: {
      action: "generate_report",
      inputSchema: QcReportArgsSchema,
      responseSchema: GenericQcResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["qcReportGenerate"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcReportGenerate", {
          outputPath: args.outputPath,
          includeAlignments: args.includeAlignments ?? true,
          includeProfiles: args.includeProfiles ?? true,
          includeCorridors: args.includeCorridors ?? true,
          includePipeNetworks: args.includePipeNetworks ?? true,
          includeSurfaces: args.includeSurfaces ?? true,
          includeLabels: args.includeLabels ?? true,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_qc",
      displayName: "Civil 3D QC",
      description: "Runs Civil 3D quality-control checks for alignments, profiles, corridors, pipe networks, surfaces, and consolidated QC reporting through a single domain tool.",
      inputShape: canonicalQcInputShape,
      supportedActions: [
        "check_alignment",
        "check_profile",
        "check_corridor",
        "check_pipe_network",
        "check_surface",
        "generate_report",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_qc_check_alignment",
      displayName: "Civil 3D QC Check Alignment",
      description: "Runs QC checks on a Civil 3D alignment.",
      inputShape: {
        name: z.string(),
        designSpeed: z.number().positive().optional(),
        checkTangents: z.boolean().optional(),
        checkCurves: z.boolean().optional(),
        checkSpirals: z.boolean().optional(),
      },
      supportedActions: ["check_alignment"],
      resolveAction: (rawArgs) => ({
        action: "check_alignment",
        args: {
          action: "check_alignment",
          name: rawArgs.name,
          designSpeed: rawArgs.designSpeed,
          checkTangents: rawArgs.checkTangents,
          checkCurves: rawArgs.checkCurves,
          checkSpirals: rawArgs.checkSpirals,
        },
      }),
    },
    {
      toolName: "civil3d_qc_check_profile",
      displayName: "Civil 3D QC Check Profile",
      description: "Runs QC checks on a Civil 3D profile.",
      inputShape: {
        alignmentName: z.string(),
        profileName: z.string(),
        maxGrade: z.number().positive().optional(),
        minKValue: z.number().positive().optional(),
      },
      supportedActions: ["check_profile"],
      resolveAction: (rawArgs) => ({
        action: "check_profile",
        args: {
          action: "check_profile",
          alignmentName: rawArgs.alignmentName,
          profileName: rawArgs.profileName,
          maxGrade: rawArgs.maxGrade,
          minKValue: rawArgs.minKValue,
        },
      }),
    },
    {
      toolName: "civil3d_qc_check_corridor",
      displayName: "Civil 3D QC Check Corridor",
      description: "Runs QC checks on a Civil 3D corridor.",
      inputShape: { name: z.string() },
      supportedActions: ["check_corridor"],
      resolveAction: (rawArgs) => ({
        action: "check_corridor",
        args: { action: "check_corridor", name: rawArgs.name },
      }),
    },
    {
      toolName: "civil3d_qc_check_pipe_network",
      displayName: "Civil 3D QC Check Pipe Network",
      description: "Runs QC checks on a Civil 3D pipe network.",
      inputShape: {
        name: z.string(),
        minCover: z.number().nonnegative().optional(),
        maxSlope: z.number().positive().optional(),
        minSlope: z.number().nonnegative().optional(),
        minVelocity: z.number().nonnegative().optional(),
        maxVelocity: z.number().positive().optional(),
      },
      supportedActions: ["check_pipe_network"],
      resolveAction: (rawArgs) => ({
        action: "check_pipe_network",
        args: {
          action: "check_pipe_network",
          name: rawArgs.name,
          minCover: rawArgs.minCover,
          maxSlope: rawArgs.maxSlope,
          minSlope: rawArgs.minSlope,
          minVelocity: rawArgs.minVelocity,
          maxVelocity: rawArgs.maxVelocity,
        },
      }),
    },
    {
      toolName: "civil3d_qc_check_surface",
      displayName: "Civil 3D QC Check Surface",
      description: "Runs QC checks on a Civil 3D TIN surface.",
      inputShape: {
        name: z.string(),
        spikeThreshold: z.number().positive().optional(),
        flatTriangleThreshold: z.number().nonnegative().optional(),
      },
      supportedActions: ["check_surface"],
      resolveAction: (rawArgs) => ({
        action: "check_surface",
        args: {
          action: "check_surface",
          name: rawArgs.name,
          spikeThreshold: rawArgs.spikeThreshold,
          flatTriangleThreshold: rawArgs.flatTriangleThreshold,
        },
      }),
    },
    {
      toolName: "civil3d_qc_report_generate",
      displayName: "Civil 3D QC Report Generate",
      description: "Runs a full QC pass over the active drawing and writes a consolidated report to disk.",
      inputShape: {
        outputPath: z.string(),
        includeAlignments: z.boolean().optional(),
        includeProfiles: z.boolean().optional(),
        includeCorridors: z.boolean().optional(),
        includePipeNetworks: z.boolean().optional(),
        includeSurfaces: z.boolean().optional(),
        includeLabels: z.boolean().optional(),
      },
      supportedActions: ["generate_report"],
      resolveAction: (rawArgs) => ({
        action: "generate_report",
        args: {
          action: "generate_report",
          outputPath: rawArgs.outputPath,
          includeAlignments: rawArgs.includeAlignments,
          includeProfiles: rawArgs.includeProfiles,
          includeCorridors: rawArgs.includeCorridors,
          includePipeNetworks: rawArgs.includePipeNetworks,
          includeSurfaces: rawArgs.includeSurfaces,
          includeLabels: rawArgs.includeLabels,
        },
      }),
    },
  ],
};
