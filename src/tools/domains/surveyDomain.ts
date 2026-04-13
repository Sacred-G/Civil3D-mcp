import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const GenericSurveyResponseSchema = z.object({}).passthrough();

const canonicalSurveyInputShape = {
  action: z.enum([
    "database_list",
    "database_create",
    "figure_list",
    "figure_get",
    "observation_list",
    "network_adjust",
    "figure_create",
    "landxml_import",
  ]),
  name: z.string().optional(),
  path: z.string().optional(),
  databaseName: z.string().optional(),
  networkName: z.string().optional(),
  observationType: z.enum(["all", "angles", "distances", "directions", "gps"]).optional(),
  method: z.enum(["least_squares", "compass", "transit", "crandall"]).optional(),
  confidenceLevel: z.number().optional(),
  applyAdjustment: z.boolean().optional(),
  figureName: z.string().optional(),
  pointNumbers: z.array(z.number().int().positive()).optional(),
  figureStyle: z.string().optional(),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
  filePath: z.string().optional(),
  importPoints: z.boolean().optional(),
  importAlignments: z.boolean().optional(),
  importSurfaces: z.boolean().optional(),
  coordinateSystemOverride: z.string().optional(),
  duplicatePolicy: z.enum(["skip", "overwrite", "rename"]).optional(),
};

const SurveyDatabaseListArgsSchema = z.object({
  action: z.literal("database_list"),
});

const SurveyDatabaseCreateArgsSchema = z.object({
  action: z.literal("database_create"),
  name: z.string(),
  path: z.string().optional(),
});

const SurveyFigureListArgsSchema = z.object({
  action: z.literal("figure_list"),
  databaseName: z.string().optional(),
});

const SurveyFigureGetArgsSchema = z.object({
  action: z.literal("figure_get"),
  name: z.string(),
  databaseName: z.string().optional(),
});

const SurveyObservationListArgsSchema = z.object({
  action: z.literal("observation_list"),
  databaseName: z.string(),
  networkName: z.string().optional(),
  observationType: z.enum(["all", "angles", "distances", "directions", "gps"]).optional(),
});

const SurveyNetworkAdjustArgsSchema = z.object({
  action: z.literal("network_adjust"),
  databaseName: z.string(),
  networkName: z.string(),
  method: z.enum(["least_squares", "compass", "transit", "crandall"]).optional(),
  confidenceLevel: z.number().min(50).max(99.9).optional(),
  applyAdjustment: z.boolean().optional(),
});

const SurveyFigureCreateArgsSchema = z.object({
  action: z.literal("figure_create"),
  databaseName: z.string(),
  figureName: z.string(),
  pointNumbers: z.array(z.number().int().positive()).min(2),
  figureStyle: z.string().optional(),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
});

const SurveyLandXmlImportArgsSchema = z.object({
  action: z.literal("landxml_import"),
  filePath: z.string(),
  databaseName: z.string(),
  importPoints: z.boolean().optional(),
  importAlignments: z.boolean().optional(),
  importSurfaces: z.boolean().optional(),
  coordinateSystemOverride: z.string().optional(),
  duplicatePolicy: z.enum(["skip", "overwrite", "rename"]).optional(),
});

export const SURVEY_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "survey",
  actions: {
    database_list: {
      action: "database_list",
      inputSchema: SurveyDatabaseListArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listSurveyDatabases"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listSurveyDatabases", {}),
      ),
    },
    database_create: {
      action: "database_create",
      inputSchema: SurveyDatabaseCreateArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createSurveyDatabase"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createSurveyDatabase", {
          name: args.name,
          path: args.path ?? null,
        }),
      ),
    },
    figure_list: {
      action: "figure_list",
      inputSchema: SurveyFigureListArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listSurveyFigures"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listSurveyFigures", {
          databaseName: args.databaseName ?? null,
        }),
      ),
    },
    figure_get: {
      action: "figure_get",
      inputSchema: SurveyFigureGetArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getSurveyFigure"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getSurveyFigure", {
          name: args.name,
          databaseName: args.databaseName ?? null,
        }),
      ),
    },
    observation_list: {
      action: "observation_list",
      inputSchema: SurveyObservationListArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listSurveyObservations"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listSurveyObservations", {
          databaseName: args.databaseName,
          networkName: args.networkName ?? null,
          observationType: args.observationType ?? "all",
        }),
      ),
    },
    network_adjust: {
      action: "network_adjust",
      inputSchema: SurveyNetworkAdjustArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["analyze", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["adjustSurveyNetwork"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("adjustSurveyNetwork", {
          databaseName: args.databaseName,
          networkName: args.networkName,
          method: args.method ?? "least_squares",
          confidenceLevel: args.confidenceLevel ?? 95,
          applyAdjustment: args.applyAdjustment ?? false,
        }),
      ),
    },
    figure_create: {
      action: "figure_create",
      inputSchema: SurveyFigureCreateArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["create"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createSurveyFigure"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("createSurveyFigure", {
          databaseName: args.databaseName,
          figureName: args.figureName,
          pointNumbers: args.pointNumbers,
          figureStyle: args.figureStyle ?? null,
          closed: args.closed ?? false,
          layer: args.layer ?? null,
        }),
      ),
    },
    landxml_import: {
      action: "landxml_import",
      inputSchema: SurveyLandXmlImportArgsSchema,
      responseSchema: GenericSurveyResponseSchema,
      capabilities: ["create", "import", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["importSurveyLandXml"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("importSurveyLandXml", {
          filePath: args.filePath,
          databaseName: args.databaseName,
          importPoints: args.importPoints ?? true,
          importAlignments: args.importAlignments ?? false,
          importSurfaces: args.importSurfaces ?? false,
          coordinateSystemOverride: args.coordinateSystemOverride ?? null,
          duplicatePolicy: args.duplicatePolicy ?? "skip",
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_survey",
      displayName: "Civil 3D Survey",
      description: "Lists, creates, imports, and manages Civil 3D survey databases, figures, observations, and network adjustments through a single domain tool.",
      inputShape: canonicalSurveyInputShape,
      supportedActions: [
        "database_list",
        "database_create",
        "figure_list",
        "figure_get",
        "observation_list",
        "network_adjust",
        "figure_create",
        "landxml_import",
      ],
      resolveAction: (rawArgs) => ({
        action: String(rawArgs.action ?? ""),
        args: rawArgs,
      }),
    },
    {
      toolName: "civil3d_survey_database_list",
      displayName: "Civil 3D Survey Database List",
      description: "Lists Civil 3D survey databases associated with the current drawing.",
      inputShape: {},
      supportedActions: ["database_list"],
      resolveAction: () => ({ action: "database_list", args: { action: "database_list" } }),
    },
    {
      toolName: "civil3d_survey_database_create",
      displayName: "Civil 3D Survey Database Create",
      description: "Creates a new Civil 3D survey database.",
      inputShape: { name: z.string(), path: z.string().optional() },
      supportedActions: ["database_create"],
      resolveAction: (rawArgs) => ({
        action: "database_create",
        args: { action: "database_create", name: rawArgs.name, path: rawArgs.path },
      }),
    },
    {
      toolName: "civil3d_survey_figure_list",
      displayName: "Civil 3D Survey Figure List",
      description: "Lists survey figures in one or more Civil 3D survey databases.",
      inputShape: { databaseName: z.string().optional() },
      supportedActions: ["figure_list"],
      resolveAction: (rawArgs) => ({
        action: "figure_list",
        args: { action: "figure_list", databaseName: rawArgs.databaseName },
      }),
    },
    {
      toolName: "civil3d_survey_figure_get",
      displayName: "Civil 3D Survey Figure Get",
      description: "Gets detailed vertex data for a specific Civil 3D survey figure.",
      inputShape: { name: z.string(), databaseName: z.string().optional() },
      supportedActions: ["figure_get"],
      resolveAction: (rawArgs) => ({
        action: "figure_get",
        args: { action: "figure_get", name: rawArgs.name, databaseName: rawArgs.databaseName },
      }),
    },
    {
      toolName: "civil3d_survey_observation_list",
      displayName: "Civil 3D Survey Observation List",
      description: "Lists raw field-book observations stored in a Civil 3D survey database.",
      inputShape: {
        databaseName: z.string(),
        networkName: z.string().optional(),
        observationType: z.enum(["all", "angles", "distances", "directions", "gps"]).optional(),
      },
      supportedActions: ["observation_list"],
      resolveAction: (rawArgs) => ({
        action: "observation_list",
        args: {
          action: "observation_list",
          databaseName: rawArgs.databaseName,
          networkName: rawArgs.networkName,
          observationType: rawArgs.observationType,
        },
      }),
    },
    {
      toolName: "civil3d_survey_network_adjust",
      displayName: "Civil 3D Survey Network Adjust",
      description: "Performs a traverse or least-squares adjustment on a Civil 3D survey network.",
      inputShape: {
        databaseName: z.string(),
        networkName: z.string(),
        method: z.enum(["least_squares", "compass", "transit", "crandall"]).optional(),
        confidenceLevel: z.number().min(50).max(99.9).optional(),
        applyAdjustment: z.boolean().optional(),
      },
      supportedActions: ["network_adjust"],
      resolveAction: (rawArgs) => ({
        action: "network_adjust",
        args: {
          action: "network_adjust",
          databaseName: rawArgs.databaseName,
          networkName: rawArgs.networkName,
          method: rawArgs.method,
          confidenceLevel: rawArgs.confidenceLevel,
          applyAdjustment: rawArgs.applyAdjustment,
        },
      }),
    },
    {
      toolName: "civil3d_survey_figure_create",
      displayName: "Civil 3D Survey Figure Create",
      description: "Creates a new survey figure by connecting existing survey points in order.",
      inputShape: {
        databaseName: z.string(),
        figureName: z.string(),
        pointNumbers: z.array(z.number().int().positive()).min(2),
        figureStyle: z.string().optional(),
        closed: z.boolean().optional(),
        layer: z.string().optional(),
      },
      supportedActions: ["figure_create"],
      resolveAction: (rawArgs) => ({
        action: "figure_create",
        args: {
          action: "figure_create",
          databaseName: rawArgs.databaseName,
          figureName: rawArgs.figureName,
          pointNumbers: rawArgs.pointNumbers,
          figureStyle: rawArgs.figureStyle,
          closed: rawArgs.closed,
          layer: rawArgs.layer,
        },
      }),
    },
    {
      toolName: "civil3d_survey_landxml_import",
      displayName: "Civil 3D Survey LandXML Import",
      description: "Imports survey data from a LandXML file into a Civil 3D survey database and/or drawing.",
      inputShape: {
        filePath: z.string(),
        databaseName: z.string(),
        importPoints: z.boolean().optional(),
        importAlignments: z.boolean().optional(),
        importSurfaces: z.boolean().optional(),
        coordinateSystemOverride: z.string().optional(),
        duplicatePolicy: z.enum(["skip", "overwrite", "rename"]).optional(),
      },
      supportedActions: ["landxml_import"],
      resolveAction: (rawArgs) => ({
        action: "landxml_import",
        args: {
          action: "landxml_import",
          filePath: rawArgs.filePath,
          databaseName: rawArgs.databaseName,
          importPoints: rawArgs.importPoints,
          importAlignments: rawArgs.importAlignments,
          importSurfaces: rawArgs.importSurfaces,
          coordinateSystemOverride: rawArgs.coordinateSystemOverride,
          duplicatePolicy: rawArgs.duplicatePolicy,
        },
      }),
    },
  ],
};
