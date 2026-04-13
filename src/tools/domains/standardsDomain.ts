import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import { lookupFrameworkStandards } from "../../standards/FrameworkStandardsService.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

const LabelPointSchema = z.object({
  x: z.number(),
  y: z.number(),
});

const LabelListObjectTypeSchema = z.enum(["alignment", "profile", "surface", "pipe_network"]);
const StyleObjectTypeSchema = z.enum([
  "surface",
  "alignment",
  "profile",
  "corridor",
  "pipe",
  "structure",
  "point",
  "section",
  "label",
  "assembly",
]);

const GenericStandardsResponseSchema = z.object({}).passthrough();

const StyleSummarySchema = z.object({
  name: z.string(),
  handle: z.string(),
  isDefault: z.boolean(),
});

const canonicalStandardsInputShape = {
  action: z.enum([
    "label_list",
    "label_add",
    "label_list_styles",
    "style_list",
    "style_get",
    "lookup",
    "check_labels",
    "check_drawing_standards",
    "fix_drawing_standards",
  ]),
  objectType: z.string().optional(),
  objectName: z.string().optional(),
  labelType: z.string().optional(),
  labelStyle: z.string().optional(),
  station: z.number().optional(),
  point: LabelPointSchema.optional(),
  styleName: z.string().optional(),
  query: z.string().optional(),
  topic: z.string().optional(),
  tags: z.array(z.string()).optional(),
  maxResults: z.number().optional(),
  checkMissing: z.boolean().optional(),
  checkStyleViolations: z.boolean().optional(),
  layerPrefix: z.string().optional(),
  checkLineweights: z.boolean().optional(),
  checkColors: z.boolean().optional(),
  fixSpaces: z.boolean().optional(),
  maxNameLength: z.number().optional(),
  colorIndex: z.number().optional(),
  lineweight: z.number().optional(),
  dryRun: z.boolean().optional(),
};

const LabelListArgsSchema = z.object({
  action: z.literal("label_list"),
  objectType: LabelListObjectTypeSchema,
  objectName: z.string(),
});

const LabelAddArgsSchema = z.object({
  action: z.literal("label_add"),
  objectType: z.string(),
  objectName: z.string(),
  labelType: z.string(),
  labelStyle: z.string().optional(),
  station: z.number().optional(),
  point: LabelPointSchema.optional(),
});

const LabelListStylesArgsSchema = z.object({
  action: z.literal("label_list_styles"),
  objectType: z.string(),
});

const StyleListArgsSchema = z.object({
  action: z.literal("style_list"),
  objectType: StyleObjectTypeSchema,
});

const StyleGetArgsSchema = z.object({
  action: z.literal("style_get"),
  objectType: StyleObjectTypeSchema,
  styleName: z.string(),
});

const StandardsLookupArgsSchema = z.object({
  action: z.literal("lookup"),
  query: z.string().optional(),
  topic: z.string().optional(),
  tags: z.array(z.string()).optional(),
  maxResults: z.number().int().min(1).max(20).optional(),
});

const QcLabelsArgsSchema = z.object({
  action: z.literal("check_labels"),
  objectType: z.enum(["alignment", "profile", "surface", "pipe_network", "all"]).optional(),
  checkMissing: z.boolean().optional(),
  checkStyleViolations: z.boolean().optional(),
});

const QcDrawingStandardsArgsSchema = z.object({
  action: z.literal("check_drawing_standards"),
  layerPrefix: z.string().optional(),
  checkLineweights: z.boolean().optional(),
  checkColors: z.boolean().optional(),
});

const QcFixDrawingStandardsArgsSchema = z.object({
  action: z.literal("fix_drawing_standards"),
  layerPrefix: z.string().optional(),
  fixSpaces: z.boolean().optional(),
  maxNameLength: z.number().int().positive().optional(),
  colorIndex: z.number().int().min(1).max(255).optional(),
  lineweight: z.number().int().optional(),
  dryRun: z.boolean().optional(),
});

export const STANDARDS_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "standards",
  actions: {
    label_list: {
      action: "label_list",
      inputSchema: LabelListArgsSchema,
      responseSchema: z.object({ labels: z.array(GenericStandardsResponseSchema) }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listLabels"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listLabels", {
          objectType: args.objectType,
          objectName: args.objectName,
        }),
      ),
    },
    label_add: {
      action: "label_add",
      inputSchema: LabelAddArgsSchema,
      responseSchema: GenericStandardsResponseSchema,
      capabilities: ["create", "edit"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["addLabel"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("addLabel", {
          objectType: args.objectType,
          objectName: args.objectName,
          labelType: args.labelType,
          labelStyle: args.labelStyle,
          station: args.station,
          point: args.point,
        }),
      ),
    },
    label_list_styles: {
      action: "label_list_styles",
      inputSchema: LabelListStylesArgsSchema,
      responseSchema: z.object({
        objectType: z.string().optional(),
        styles: z.array(GenericStandardsResponseSchema),
      }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listLabelStyles"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listLabelStyles", {
          objectType: args.objectType,
        }),
      ),
    },
    style_list: {
      action: "style_list",
      inputSchema: StyleListArgsSchema,
      responseSchema: z.object({
        objectType: z.string(),
        styles: z.array(StyleSummarySchema),
      }),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["listStyles"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("listStyles", {
          objectType: args.objectType,
        }),
      ),
    },
    style_get: {
      action: "style_get",
      inputSchema: StyleGetArgsSchema,
      responseSchema: z.object({
        name: z.string().optional(),
        objectType: z.string().optional(),
      }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["getStyle"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("getStyle", {
          objectType: args.objectType,
          styleName: args.styleName,
        }),
      ),
    },
    lookup: {
      action: "lookup",
      inputSchema: StandardsLookupArgsSchema,
      responseSchema: GenericStandardsResponseSchema,
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: false,
      safeForRetry: true,
      execute: async (args) => {
        const parsedArgs = StandardsLookupArgsSchema.parse(args);
        return await lookupFrameworkStandards({
          query: parsedArgs.query,
          topic: parsedArgs.topic,
          tags: parsedArgs.tags,
          maxResults: parsedArgs.maxResults,
        });
      },
    },
    check_labels: {
      action: "check_labels",
      inputSchema: QcLabelsArgsSchema,
      responseSchema: GenericStandardsResponseSchema,
      capabilities: ["query", "inspect", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckLabels"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckLabels", {
          objectType: args.objectType ?? "all",
          checkMissing: args.checkMissing ?? true,
          checkStyleViolations: args.checkStyleViolations ?? true,
        }),
      ),
    },
    check_drawing_standards: {
      action: "check_drawing_standards",
      inputSchema: QcDrawingStandardsArgsSchema,
      responseSchema: GenericStandardsResponseSchema,
      capabilities: ["query", "inspect", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["qcCheckDrawingStandards"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcCheckDrawingStandards", {
          layerPrefix: args.layerPrefix ?? null,
          checkLineweights: args.checkLineweights ?? true,
          checkColors: args.checkColors ?? true,
        }),
      ),
    },
    fix_drawing_standards: {
      action: "fix_drawing_standards",
      inputSchema: QcFixDrawingStandardsArgsSchema,
      responseSchema: GenericStandardsResponseSchema,
      capabilities: ["edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["qcFixDrawingStandards"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcFixDrawingStandards", {
          layerPrefix: args.layerPrefix ?? null,
          fixSpaces: args.fixSpaces ?? true,
          maxNameLength: args.maxNameLength ?? 64,
          colorIndex: args.colorIndex ?? null,
          lineweight: args.lineweight ?? null,
          dryRun: args.dryRun ?? false,
        }),
      ),
    },
  },
  exposures: [
    {
      toolName: "civil3d_standards",
      displayName: "Civil 3D Standards",
      description: "Inspects and manages Civil 3D styles, labels, standards guidance, and drawing-standard compliance through a single domain tool.",
      inputShape: canonicalStandardsInputShape,
      supportedActions: [
        "label_list",
        "label_add",
        "label_list_styles",
        "style_list",
        "style_get",
        "lookup",
        "check_labels",
        "check_drawing_standards",
        "fix_drawing_standards",
      ],
      capabilities: ["query", "inspect", "create", "edit", "manage", "analyze"],
      requiresActiveDrawing: false,
      safeForRetry: false,
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_label",
      displayName: "Civil 3D Label",
      description: "Manages labels on Civil 3D objects.",
      inputShape: {
        action: z.enum(["list", "add", "list_styles"]),
        objectType: z.string(),
        objectName: z.string().optional(),
        labelType: z.string().optional(),
        labelStyle: z.string().optional(),
        station: z.number().optional(),
        point: LabelPointSchema.optional(),
      },
      supportedActions: ["label_list", "label_add", "label_list_styles"],
      operations: ["list", "add", "list_styles"],
      resolveAction: (rawArgs) => ({
        action:
          rawArgs.action === "list"
            ? "label_list"
            : rawArgs.action === "add"
              ? "label_add"
              : "label_list_styles",
        args: rawArgs.action === "list"
          ? { action: "label_list", objectType: rawArgs.objectType, objectName: rawArgs.objectName }
          : rawArgs.action === "add"
            ? {
              action: "label_add",
              objectType: rawArgs.objectType,
              objectName: rawArgs.objectName,
              labelType: rawArgs.labelType,
              labelStyle: rawArgs.labelStyle,
              station: rawArgs.station,
              point: rawArgs.point,
            }
            : { action: "label_list_styles", objectType: rawArgs.objectType },
      }),
    },
    {
      toolName: "civil3d_style",
      displayName: "Civil 3D Style",
      description: "Lists and inspects Civil 3D styles for supported object types.",
      inputShape: {
        action: z.enum(["list", "get"]),
        objectType: StyleObjectTypeSchema,
        styleName: z.string().optional(),
      },
      supportedActions: ["style_list", "style_get"],
      operations: ["list", "get"],
      resolveAction: (rawArgs) => ({
        action: rawArgs.action === "list" ? "style_list" : "style_get",
        args: rawArgs.action === "list"
          ? { action: "style_list", objectType: rawArgs.objectType }
          : { action: "style_get", objectType: rawArgs.objectType, styleName: rawArgs.styleName },
      }),
    },
    {
      toolName: "civil3d_standards_lookup",
      displayName: "Civil 3D Standards Lookup",
      description: "Looks up Civil 3D standards, template governance, layer/style guidance, and labeling conventions.",
      inputShape: {
        query: z.string().optional(),
        topic: z.string().optional(),
        tags: z.array(z.string()).optional(),
        maxResults: z.number().int().min(1).max(20).optional(),
      },
      supportedActions: ["lookup"],
      resolveAction: (rawArgs) => ({
        action: "lookup",
        args: {
          action: "lookup",
          query: rawArgs.query,
          topic: rawArgs.topic,
          tags: rawArgs.tags,
          maxResults: rawArgs.maxResults,
        },
      }),
    },
    {
      toolName: "civil3d_qc_check_labels",
      displayName: "Civil 3D QC Check Labels",
      description: "Checks Civil 3D labels for missing labels and style-standard violations.",
      inputShape: {
        objectType: z.enum(["alignment", "profile", "surface", "pipe_network", "all"]).optional(),
        checkMissing: z.boolean().optional(),
        checkStyleViolations: z.boolean().optional(),
      },
      supportedActions: ["check_labels"],
      resolveAction: (rawArgs) => ({
        action: "check_labels",
        args: {
          action: "check_labels",
          objectType: rawArgs.objectType,
          checkMissing: rawArgs.checkMissing,
          checkStyleViolations: rawArgs.checkStyleViolations,
        },
      }),
    },
    {
      toolName: "civil3d_qc_check_drawing_standards",
      displayName: "Civil 3D QC Check Drawing Standards",
      description: "Audits the active drawing against CAD standards for layer naming, lineweights, and colors.",
      inputShape: {
        layerPrefix: z.string().optional(),
        checkLineweights: z.boolean().optional(),
        checkColors: z.boolean().optional(),
      },
      supportedActions: ["check_drawing_standards"],
      resolveAction: (rawArgs) => ({
        action: "check_drawing_standards",
        args: {
          action: "check_drawing_standards",
          layerPrefix: rawArgs.layerPrefix,
          checkLineweights: rawArgs.checkLineweights,
          checkColors: rawArgs.checkColors,
        },
      }),
    },
    {
      toolName: "civil3d_qc_fix_drawing_standards",
      displayName: "Civil 3D QC Fix Drawing Standards",
      description: "Automatically remediates drawing-standard layer issues.",
      inputShape: {
        layerPrefix: z.string().optional(),
        fixSpaces: z.boolean().optional(),
        maxNameLength: z.number().int().positive().optional(),
        colorIndex: z.number().int().min(1).max(255).optional(),
        lineweight: z.number().int().optional(),
        dryRun: z.boolean().optional(),
      },
      supportedActions: ["fix_drawing_standards"],
      resolveAction: (rawArgs) => ({
        action: "fix_drawing_standards",
        args: {
          action: "fix_drawing_standards",
          layerPrefix: rawArgs.layerPrefix,
          fixSpaces: rawArgs.fixSpaces,
          maxNameLength: rawArgs.maxNameLength,
          colorIndex: rawArgs.colorIndex,
          lineweight: rawArgs.lineweight,
          dryRun: rawArgs.dryRun,
        },
      }),
    },
  ],
};
