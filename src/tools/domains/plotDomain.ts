import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import type { DomainToolDefinition } from "../domainRuntime.js";

/**
 * civil3d_plot — layout discovery and PDF plotting/publishing.
 *
 * The plugin drives the -PLOT and -PUBLISH commands (BACKGROUNDPLOT=0) rather
 * than the AutoCAD PlotEngine API: live testing on Civil 3D 2027 found the
 * PlotEngine crash-prone from a command context, whereas a command that gets
 * an unexpected answer fails with a message. See PlotCommands.cs for details.
 *
 * Read-only discovery actions are safe to retry and are never approval-gated.
 * Both output actions declare the "export" capability, so the central
 * approval policy requires an approval token before they write a PDF, and
 * every output path is re-validated by the plugin's FileBoundary (export
 * roots, .pdf extension, no junctions, overwrite=false by default).
 */

export const PLOT_JOB_OPERATIONS = {
  plot_layouts_to_pdf: "plot_layouts_to_pdf",
  publish_sheet_set: "publish_sheet_set",
} as const;

const OrientationSchema = z.enum(["portrait", "landscape"]);

const PlotSettingsSummaryShape = {
  device: z.string().nullable(),
  canonicalMediaName: z.string().nullable(),
  paperUnits: z.string(),
  paperWidth: z.number(),
  paperHeight: z.number(),
  orientation: OrientationSchema,
  rotation: z.string(),
  plotType: z.string(),
  plotStyleTable: z.string().nullable(),
};

const LayoutSummarySchema = z.object({
  name: z.string(),
  tabOrder: z.number().int(),
  isModel: z.boolean(),
  isCurrent: z.boolean(),
  pageSetup: z.string().nullable(),
  handle: z.string(),
  ...PlotSettingsSummaryShape,
}).passthrough();

const PageSetupSummarySchema = z.object({
  name: z.string(),
  modelType: z.boolean(),
  ...PlotSettingsSummaryShape,
}).passthrough();

const LayoutPlotResultSchema = z.object({
  layoutName: z.string(),
  status: z.enum(["plotted", "failed", "skipped"]),
  outputPath: z.string().nullable(),
  bytes: z.number().int().nonnegative().nullable(),
  pageCount: z.number().int().nonnegative().nullable(),
  paperSize: z.string(),
  orientation: OrientationSchema,
  plotStyleTable: z.string().nullable(),
  pageSetup: z.string().nullable(),
  durationMs: z.number().nonnegative(),
  error: z.string().nullable(),
}).passthrough();

const PlotLayoutsResultSchema = z.object({
  device: z.string(),
  method: z.string(),
  requested: z.number().int().nonnegative(),
  plotted: z.number().int().nonnegative(),
  failed: z.number().int().nonnegative(),
  skipped: z.number().int().nonnegative(),
  totalBytes: z.number().nonnegative(),
  durationMs: z.number().nonnegative(),
  layouts: z.array(LayoutPlotResultSchema),
  warnings: z.array(z.string()),
}).passthrough();

const PublishResultSchema = z.object({
  outputPath: z.string(),
  bytes: z.number().int().positive(),
  pageCount: z.number().int().nonnegative().nullable(),
  sheetCount: z.number().int().positive(),
  sheets: z.array(z.object({
    index: z.number().int().positive(),
    layoutName: z.string(),
    drawingPath: z.string(),
  })),
  method: z.string(),
  dsdPath: z.string().nullable(),
  durationMs: z.number().nonnegative(),
  published: z.literal(true),
  warnings: z.array(z.string()),
}).passthrough();

// Returned instead of the result when asJob=true (see civil3d_job status).
const JobHandleSchema = z.object({
  jobId: z.string(),
  state: z.enum(["running", "completed", "failed", "cancelled"]),
  operation: z.string(),
}).passthrough();

const ListLayoutsArgsSchema = z.object({
  action: z.literal("list_layouts"),
  includeModel: z.boolean().optional(),
});

const ListPageSetupsArgsSchema = z.object({
  action: z.literal("list_page_setups"),
});

// Plot area: a paper-space layout defaults to "layout" (the sheet at 1:1); the
// Model tab, which has no layout area, defaults to "extents" scaled to fit.
// "window" corners are in the plotted space's own units: paper units on a
// layout, drawing units on the Model tab.
const PlotAreaSchema = z.enum(["layout", "extents", "window"]);

// The plugin treats a whitespace-only override as absent, so reject it here
// instead of silently plotting with the layout's own settings.
const NonBlankStringSchema = z.string().refine((value) => value.trim().length > 0, "must not be blank");

const ListPlottersArgsSchema = z.object({
  action: z.literal("list_plotters"),
  device: NonBlankStringSchema.optional(),
});

export const PlotLayoutsToPdfArgsSchema = z.object({
  action: z.literal("plot_layouts_to_pdf"),
  layoutNames: z.array(z.string().min(1)).min(1).optional(),
  allLayouts: z.boolean().optional(),
  outputDirectory: z.string().min(1).optional(),
  outputPath: z.string().min(1).optional(),
  fileNamePrefix: z.string().optional(),
  pageSetup: NonBlankStringSchema.optional(),
  paperSize: NonBlankStringSchema.optional(),
  plotStyleTable: NonBlankStringSchema.optional(),
  orientation: OrientationSchema.optional(),
  plotArea: PlotAreaSchema.optional(),
  windowCorners: z.array(z.number()).length(4).optional(),
  overwrite: z.boolean().optional(),
  continueOnError: z.boolean().optional(),
  asJob: z.boolean().optional(),
}).superRefine((args, context) => {
  const hasLayouts = (args.layoutNames?.length ?? 0) > 0;
  if (hasLayouts === Boolean(args.allLayouts)) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Provide either a non-empty layoutNames array or allLayouts=true (not both).",
      path: ["layoutNames"],
    });
  }
  if (Boolean(args.outputDirectory) === Boolean(args.outputPath)) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Provide exactly one of outputDirectory or outputPath.",
      path: ["outputDirectory"],
    });
  }
  if (args.outputPath && !/\.pdf$/i.test(args.outputPath)) {
    context.addIssue({ code: z.ZodIssueCode.custom, message: "outputPath must end in .pdf.", path: ["outputPath"] });
  }
  if ((args.plotArea === "window") !== Boolean(args.windowCorners)) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: "plotArea 'window' needs windowCorners [x1, y1, x2, y2], and windowCorners needs plotArea 'window'.",
      path: ["windowCorners"],
    });
  }
  // Any two opposite corners: the plugin sends the lower-left and upper-right
  // corners to -PLOT in that order. A window with no width or height is refused.
  if (args.windowCorners?.length === 4 && (args.windowCorners[0] === args.windowCorners[2] || args.windowCorners[1] === args.windowCorners[3])) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: "windowCorners must be two opposite corners [x1, y1, x2, y2] with distinct x and distinct y values.",
      path: ["windowCorners"],
    });
  }
  if (args.outputPath && (args.allLayouts || (args.layoutNames?.length ?? 0) > 1)) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: "outputPath plots exactly one layout; use outputDirectory for several.",
      path: ["outputPath"],
    });
  }
});

const PublishSheetSchema = z.object({
  layoutName: z.string().min(1),
  drawingPath: z.string().min(1).optional(),
});

export const PublishSheetSetArgsSchema = z.object({
  action: z.literal("publish_sheet_set"),
  outputPath: z.string().min(1).regex(/\.pdf$/i, "outputPath must end in .pdf."),
  layoutNames: z.array(z.string().min(1)).min(1).optional(),
  sheets: z.array(PublishSheetSchema).min(1).optional(),
  overwrite: z.boolean().optional(),
  requireSaved: z.boolean().optional(),
  keepDsd: z.boolean().optional(),
  asJob: z.boolean().optional(),
}).refine((args) => !(args.layoutNames && args.sheets), {
  message: "Provide layoutNames or sheets, not both. Omit both to publish every paper-space layout.",
  path: ["sheets"],
});

type PlotLayoutsToPdfArgs = z.infer<typeof PlotLayoutsToPdfArgsSchema>;
type PublishSheetSetArgs = z.infer<typeof PublishSheetSetArgsSchema>;

export function toPlotLayoutsParameters(args: PlotLayoutsToPdfArgs) {
  return {
    layoutNames: args.layoutNames,
    allLayouts: args.allLayouts ?? false,
    outputDirectory: args.outputDirectory,
    outputPath: args.outputPath,
    fileNamePrefix: args.fileNamePrefix,
    pageSetup: args.pageSetup,
    paperSize: args.paperSize,
    plotStyleTable: args.plotStyleTable,
    orientation: args.orientation,
    plotArea: args.plotArea,
    windowCorners: args.windowCorners,
    overwrite: args.overwrite ?? false,
    continueOnError: args.continueOnError ?? true,
  };
}

export function toPublishSheetSetParameters(args: PublishSheetSetArgs) {
  return {
    outputPath: args.outputPath,
    layoutNames: args.layoutNames,
    sheets: args.sheets,
    overwrite: args.overwrite ?? false,
    requireSaved: args.requireSaved ?? true,
    keepDsd: args.keepDsd ?? false,
  };
}

async function runPlotOperation(
  pluginMethod: string,
  jobOperation: string,
  parameters: Record<string, unknown>,
  asJob: boolean | undefined,
) {
  return await withApplicationConnection(async (appClient) => asJob
    ? await appClient.sendCommand("startJob", { operation: jobOperation, parameters })
    : await appClient.sendCommand(pluginMethod, parameters));
}

export const PLOT_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "plot",
  actions: {
    list_layouts: {
      action: "list_layouts",
      inputSchema: ListLayoutsArgsSchema,
      responseSchema: z.object({
        drawing: z.string(),
        currentLayout: z.string().nullable(),
        count: z.number().int().nonnegative(),
        layouts: z.array(LayoutSummarySchema),
      }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["plotListLayouts"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("plotListLayouts", {
          includeModel: args.includeModel ?? false,
        }),
      ),
    },
    list_page_setups: {
      action: "list_page_setups",
      inputSchema: ListPageSetupsArgsSchema,
      responseSchema: z.object({
        count: z.number().int().nonnegative(),
        pageSetups: z.array(PageSetupSummarySchema),
      }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["plotListPageSetups"],
      execute: async () => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("plotListPageSetups", {}),
      ),
    },
    list_plotters: {
      action: "list_plotters",
      inputSchema: ListPlottersArgsSchema,
      responseSchema: z.object({
        devices: z.array(z.string()),
        plotStyleTables: z.array(z.string()),
        defaultPdfDevice: z.string(),
        pdfDeviceAvailable: z.boolean(),
        device: z.string().nullable(),
        media: z.array(z.object({ canonicalName: z.string(), localeName: z.string() })).nullable(),
      }).passthrough(),
      capabilities: ["query", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["plotListPlotters"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("plotListPlotters", { device: args.device }),
      ),
    },
    plot_layouts_to_pdf: {
      action: "plot_layouts_to_pdf",
      inputSchema: PlotLayoutsToPdfArgsSchema as unknown as z.ZodType<Record<string, unknown>>,
      responseSchema: z.union([PlotLayoutsResultSchema, JobHandleSchema]),
      capabilities: ["export", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["plotLayoutsToPdf", "startJob"],
      execute: async (rawArgs) => {
        const args = rawArgs as PlotLayoutsToPdfArgs;
        return await runPlotOperation(
          "plotLayoutsToPdf",
          PLOT_JOB_OPERATIONS.plot_layouts_to_pdf,
          toPlotLayoutsParameters(args),
          args.asJob,
        );
      },
    },
    publish_sheet_set: {
      action: "publish_sheet_set",
      inputSchema: PublishSheetSetArgsSchema as unknown as z.ZodType<Record<string, unknown>>,
      responseSchema: z.union([PublishResultSchema, JobHandleSchema]),
      capabilities: ["export", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["plotPublishSheetSet", "startJob"],
      execute: async (rawArgs) => {
        const args = rawArgs as PublishSheetSetArgs;
        return await runPlotOperation(
          "plotPublishSheetSet",
          PLOT_JOB_OPERATIONS.publish_sheet_set,
          toPublishSheetSetParameters(args),
          args.asJob,
        );
      },
    },
  },
  exposures: [
    {
      toolName: "civil3d_plot",
      displayName: "Civil 3D Plot",
      description:
        "Lists layouts, page setups, and plot devices, and plots layouts (paper-space, or the Model tab by name) to PDF (one file per layout) or publishes paper-space layouts as one multi-sheet PDF. " +
        "Uses the -PLOT/-PUBLISH commands with BACKGROUNDPLOT=0 and the 'DWG To PDF.pc3' device; output paths must be absolute .pdf paths inside the configured export roots. " +
        "Set asJob=true for long runs and poll civil3d_job action=status.",
      inputShape: {
        action: z.enum([
          "list_layouts",
          "list_page_setups",
          "list_plotters",
          "plot_layouts_to_pdf",
          "publish_sheet_set",
        ]),
        includeModel: z.boolean().optional().describe("list_layouts: include the Model layout."),
        device: z.string().optional().describe("list_plotters: also list paper sizes for this device (for example 'DWG To PDF.pc3')."),
        layoutNames: z.array(z.string()).optional().describe("Layouts to plot/publish, in output order. plot_layouts_to_pdf also accepts 'Model'."),
        allLayouts: z.boolean().optional().describe("plot_layouts_to_pdf: plot every paper-space layout in tab order."),
        sheets: z.array(PublishSheetSchema).optional().describe("publish_sheet_set: explicit sheet list; drawingPath (absolute .dwg inside the import roots) defaults to the active drawing."),
        outputDirectory: z.string().optional().describe("plot_layouts_to_pdf: absolute folder for one PDF per layout."),
        outputPath: z.string().optional().describe("Absolute .pdf path (single layout for plot_layouts_to_pdf; the combined PDF for publish_sheet_set)."),
        fileNamePrefix: z.string().optional().describe("plot_layouts_to_pdf: file name prefix (default '<drawing>-')."),
        pageSetup: z.string().optional().describe("plot_layouts_to_pdf: named paper-space page setup to take paper size, orientation, and plot style from."),
        paperSize: z.string().optional().describe("plot_layouts_to_pdf: paper size name on DWG To PDF.pc3 (overrides the layout/page setup)."),
        plotStyleTable: z.string().optional().describe("plot_layouts_to_pdf: plot style table, for example 'monochrome.ctb'."),
        orientation: OrientationSchema.optional(),
        plotArea: PlotAreaSchema.optional().describe("plot_layouts_to_pdf: 'layout' (paper-space default, 1:1), 'extents', or 'window'. The Model tab plots 'extents' (default) or 'window', scaled to fit and centred."),
        windowCorners: z.array(z.number()).optional().describe("plot_layouts_to_pdf with plotArea='window': [x1, y1, x2, y2], any two opposite corners, in paper units on a layout, drawing units on the Model tab."),
        overwrite: z.boolean().optional().describe("Replace existing PDFs (default false)."),
        continueOnError: z.boolean().optional().describe("plot_layouts_to_pdf: keep plotting after a layout fails (default true)."),
        requireSaved: z.boolean().optional().describe("publish_sheet_set: refuse when the drawing has unsaved changes (default true)."),
        keepDsd: z.boolean().optional().describe("publish_sheet_set: keep the generated .dsd sheet list next to the PDF."),
        asJob: z.boolean().optional().describe("Run the plot/publish as a background job and return a jobId."),
      },
      supportedActions: [
        "list_layouts",
        "list_page_setups",
        "list_plotters",
        "plot_layouts_to_pdf",
        "publish_sheet_set",
      ],
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
  ],
};
