import { z } from "zod";
import { withApplicationConnection } from "../../utils/ConnectionManager.js";
import { DRAWING_RUNTIME_DOMAIN_DEFINITION } from "./drawingRuntimeDomain.js";
import type { DomainToolDefinition } from "../domainRuntime.js";
import { PIPE_DOMAIN_DEFINITION } from "./pipeDomain.js";
import { PROJECT_DOMAIN_DEFINITION } from "./projectDomain.js";
import { SURFACE_DOMAIN_DEFINITION } from "./surfaceDomain.js";
import { SURVEY_DOMAIN_DEFINITION } from "./surveyDomain.js";

const SurfaceVolumeMethodSchema = z.enum(["tin_volume", "average_end_area", "prismoidal"]);
const WorkflowDataShortcutObjectTypeSchema = z.enum([
  "surface",
  "alignment",
  "profile",
  "pipe_network",
  "pressure_network",
  "corridor",
  "section_view_group",
]);
const WorkflowSurveyAdjustMethodSchema = z.enum(["least_squares", "compass", "transit", "crandall"]);
const WorkflowDuplicatePolicySchema = z.enum(["skip", "overwrite", "rename"]);
const WorkflowVolumeReportFormatSchema = z.enum(["summary", "detailed"]);
const WorkflowGradingSideSchema = z.enum(["left", "right", "both"]);
const WorkflowShortcutReferenceSchema = z.object({
  projectFolder: z.string(),
  shortcutName: z.string(),
  shortcutType: WorkflowDataShortcutObjectTypeSchema,
  layer: z.string().optional(),
});

const PipeFlowSchema = z.object({
  pipeName: z.string(),
  designFlow: z.number().positive(),
});

const WorkflowStepSchema = z.object({
  name: z.string(),
  action: z.string(),
  status: z.enum(["completed", "skipped"]),
  result: z.unknown().optional(),
});

const WorkflowResponseSchema = z.object({
  workflow: z.string(),
  status: z.enum(["completed", "completed_with_warnings"]),
  summary: z.string(),
  steps: z.array(WorkflowStepSchema),
  outputs: z.record(z.string(), z.unknown()),
  warnings: z.array(z.string()),
});

function buildWorkflowResult(
  workflow: string,
  summary: string,
  steps: Array<z.infer<typeof WorkflowStepSchema>>,
  outputs: Record<string, unknown>,
  warnings: string[] = [],
) {
  return {
    workflow,
    status: warnings.length > 0 ? "completed_with_warnings" as const : "completed" as const,
    summary,
    steps,
    outputs,
    warnings,
  };
}

const canonicalWorkflowInputShape = {
  action: z.enum([
    "corridor_qc_report",
    "grading_surface_volume",
    "surface_comparison_report",
    "data_shortcut_publish_sync",
    "data_shortcut_reference_sync",
    "project_startup",
    "project_reference_setup",
    "drawing_readiness_audit",
    "feature_line_to_grading",
    "pipe_network_design",
    "plan_production_publish",
    "qc_fix_and_verify",
    "survey_import_adjust_figures",
  ]),
  corridorName: z.string().optional(),
  outputPath: z.string().optional(),
  includeAlignments: z.boolean().optional(),
  includeProfiles: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includeSurfaces: z.boolean().optional(),
  includeLabels: z.boolean().optional(),
  baseSurface: z.string().optional(),
  comparisonSurface: z.string().optional(),
  method: SurfaceVolumeMethodSchema.optional(),
  format: WorkflowVolumeReportFormatSchema.optional(),
  objectType: WorkflowDataShortcutObjectTypeSchema.optional(),
  objectName: z.string().optional(),
  shortcutName: z.string().optional(),
  shortcutType: WorkflowDataShortcutObjectTypeSchema.optional(),
  references: z.array(WorkflowShortcutReferenceSchema).optional(),
  projectFolder: z.string().optional(),
  description: z.string().optional(),
  templatePath: z.string().optional(),
  saveAs: z.string().optional(),
  limit: z.number().optional(),
  featureLineName: z.string().optional(),
  groupName: z.string().optional(),
  groupDescription: z.string().optional(),
  createGroup: z.boolean().optional(),
  useProjection: z.boolean().optional(),
  criteriaName: z.string().optional(),
  side: WorkflowGradingSideSchema.optional(),
  networkName: z.string().optional(),
  partsList: z.string().optional(),
  defaultDesignFlow: z.number().positive().optional(),
  perPipeDesignFlows: z.array(PipeFlowSchema).optional(),
  manningsN: z.number().positive().optional(),
  targetVelocityMin: z.number().positive().optional(),
  targetVelocityMax: z.number().positive().optional(),
  applyChanges: z.boolean().optional(),
  runHydraulicAnalysis: z.boolean().optional(),
  designFlow: z.number().positive().optional(),
  minCoverDepth: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
  tailwaterElevation: z.number().optional(),
  sheetSetName: z.string().optional(),
  layoutNames: z.array(z.string()).optional(),
  plotStyleTable: z.string().optional(),
  paperSize: z.string().optional(),
  layerPrefix: z.string().optional(),
  checkLineweights: z.boolean().optional(),
  checkColors: z.boolean().optional(),
  fixSpaces: z.boolean().optional(),
  maxNameLength: z.number().int().positive().optional(),
  colorIndex: z.number().int().min(1).max(255).optional(),
  lineweight: z.number().int().optional(),
  dryRun: z.boolean().optional(),
  databaseName: z.string().optional(),
  filePath: z.string().optional(),
  confidenceLevel: z.number().optional(),
  applyAdjustment: z.boolean().optional(),
  figureName: z.string().optional(),
  pointNumbers: z.array(z.number().int().positive()).optional(),
  figureStyle: z.string().optional(),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
  importPoints: z.boolean().optional(),
  importAlignments: z.boolean().optional(),
  importSurfaces: z.boolean().optional(),
  coordinateSystemOverride: z.string().optional(),
  duplicatePolicy: WorkflowDuplicatePolicySchema.optional(),
};

const CorridorQcReportArgsSchema = z.object({
  action: z.literal("corridor_qc_report"),
  corridorName: z.string(),
  outputPath: z.string().optional(),
  includeAlignments: z.boolean().optional(),
  includeProfiles: z.boolean().optional(),
  includePipeNetworks: z.boolean().optional(),
  includeSurfaces: z.boolean().optional(),
  includeLabels: z.boolean().optional(),
});

const GradingSurfaceVolumeArgsSchema = z.object({
  action: z.literal("grading_surface_volume"),
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  method: SurfaceVolumeMethodSchema.optional(),
});

const SurfaceComparisonReportArgsSchema = z.object({
  action: z.literal("surface_comparison_report"),
  baseSurface: z.string(),
  comparisonSurface: z.string(),
  format: WorkflowVolumeReportFormatSchema.optional(),
});

const DataShortcutPublishSyncArgsSchema = z.object({
  action: z.literal("data_shortcut_publish_sync"),
  objectType: WorkflowDataShortcutObjectTypeSchema,
  objectName: z.string(),
  shortcutName: z.string().optional(),
  description: z.string().optional(),
  projectFolder: z.string().optional(),
  dryRun: z.boolean().optional(),
});

const DataShortcutReferenceSyncArgsSchema = z.object({
  action: z.literal("data_shortcut_reference_sync"),
  projectFolder: z.string(),
  shortcutName: z.string(),
  shortcutType: WorkflowDataShortcutObjectTypeSchema,
  layer: z.string().optional(),
  dryRun: z.boolean().optional(),
});

const ProjectStartupArgsSchema = z.object({
  action: z.literal("project_startup"),
  templatePath: z.string().optional(),
  saveAs: z.string().optional(),
});

const ProjectReferenceSetupArgsSchema = z.object({
  action: z.literal("project_reference_setup"),
  references: z.array(WorkflowShortcutReferenceSchema).min(1),
  dryRun: z.boolean().optional(),
  saveAs: z.string().optional(),
});

const DrawingReadinessAuditArgsSchema = z.object({
  action: z.literal("drawing_readiness_audit"),
  layerPrefix: z.string().optional(),
  checkLineweights: z.boolean().optional(),
  checkColors: z.boolean().optional(),
  limit: z.number().int().positive().optional(),
});

const FeatureLineToGradingArgsSchema = z.object({
  action: z.literal("feature_line_to_grading"),
  featureLineName: z.string(),
  groupName: z.string(),
  groupDescription: z.string().optional(),
  createGroup: z.boolean().optional(),
  useProjection: z.boolean().optional(),
  criteriaName: z.string().optional(),
  side: WorkflowGradingSideSchema.optional(),
  surfaceName: z.string().optional(),
});

const PipeNetworkDesignArgsSchema = z.object({
  action: z.literal("pipe_network_design"),
  networkName: z.string(),
  partsList: z.string().optional(),
  defaultDesignFlow: z.number().positive().optional(),
  perPipeDesignFlows: z.array(PipeFlowSchema).optional(),
  manningsN: z.number().positive().optional(),
  targetVelocityMin: z.number().positive().optional(),
  targetVelocityMax: z.number().positive().optional(),
  applyChanges: z.boolean().optional(),
  runHydraulicAnalysis: z.boolean().optional(),
  designFlow: z.number().positive().optional(),
  minCoverDepth: z.number().nonnegative().optional(),
  minVelocity: z.number().nonnegative().optional(),
  maxVelocity: z.number().positive().optional(),
  minSlope: z.number().nonnegative().optional(),
});

const PlanProductionPublishArgsSchema = z.object({
  action: z.literal("plan_production_publish"),
  outputPath: z.string(),
  sheetSetName: z.string().optional(),
  layoutNames: z.array(z.string()).min(1).optional(),
  plotStyleTable: z.string().optional(),
  paperSize: z.string().optional(),
}).superRefine((value, ctx) => {
  if (!value.sheetSetName && (!value.layoutNames || value.layoutNames.length === 0)) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Either sheetSetName or layoutNames is required.",
      path: ["sheetSetName"],
    });
  }
});

const QcFixAndVerifyArgsSchema = z.object({
  action: z.literal("qc_fix_and_verify"),
  layerPrefix: z.string().optional(),
  checkLineweights: z.boolean().optional(),
  checkColors: z.boolean().optional(),
  fixSpaces: z.boolean().optional(),
  maxNameLength: z.number().int().positive().optional(),
  colorIndex: z.number().int().min(1).max(255).optional(),
  lineweight: z.number().int().optional(),
  dryRun: z.boolean().optional(),
});

const SurveyImportAdjustFiguresArgsSchema = z.object({
  action: z.literal("survey_import_adjust_figures"),
  filePath: z.string(),
  databaseName: z.string(),
  networkName: z.string().optional(),
  method: WorkflowSurveyAdjustMethodSchema.optional(),
  confidenceLevel: z.number().min(50).max(99.9).optional(),
  applyAdjustment: z.boolean().optional(),
  figureName: z.string().optional(),
  pointNumbers: z.array(z.number().int().positive()).min(2).optional(),
  figureStyle: z.string().optional(),
  closed: z.boolean().optional(),
  layer: z.string().optional(),
  importPoints: z.boolean().optional(),
  importAlignments: z.boolean().optional(),
  importSurfaces: z.boolean().optional(),
  coordinateSystemOverride: z.string().optional(),
  duplicatePolicy: WorkflowDuplicatePolicySchema.optional(),
}).superRefine((value, ctx) => {
  const hasFigureName = Boolean(value.figureName);
  const hasPointNumbers = Boolean(value.pointNumbers && value.pointNumbers.length > 0);

  if (hasFigureName !== hasPointNumbers) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "figureName and pointNumbers must be provided together.",
      path: hasFigureName ? ["pointNumbers"] : ["figureName"],
    });
  }
});

export const WORKFLOW_DOMAIN_DEFINITION: DomainToolDefinition = {
  domain: "workflow",
  actions: {
    corridor_qc_report: {
      action: "corridor_qc_report",
      inputSchema: CorridorQcReportArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["corridorQcReportWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("corridorQcReportWorkflow", {
          corridorName: args.corridorName,
          outputPath: args.outputPath,
          includeAlignments: args.includeAlignments,
          includeProfiles: args.includeProfiles,
          includePipeNetworks: args.includePipeNetworks,
          includeSurfaces: args.includeSurfaces,
          includeLabels: args.includeLabels,
        }),
      ),
    },
    grading_surface_volume: {
      action: "grading_surface_volume",
      inputSchema: GradingSurfaceVolumeArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["calculateSurfaceVolume"],
      execute: async (args) => {
        const volumeResult = await SURFACE_DOMAIN_DEFINITION.actions.volume_calculate.execute({
          action: "volume_calculate",
          baseSurface: args.baseSurface,
          comparisonSurface: args.comparisonSurface,
          method: args.method,
        });

        return buildWorkflowResult(
          "grading_surface_volume",
          `Calculated grading/earthwork volume between '${args.baseSurface}' and '${args.comparisonSurface}'.`,
          [
            {
              name: "Calculate surface-to-surface volume",
              action: "surface.volume_calculate",
              status: "completed",
              result: volumeResult,
            },
          ],
          { volume: volumeResult },
        );
      },
    },
    surface_comparison_report: {
      action: "surface_comparison_report",
      inputSchema: SurfaceComparisonReportArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "analyze", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["surfaceComparisonReportWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("surfaceComparisonReportWorkflow", {
          baseSurface: args.baseSurface,
          comparisonSurface: args.comparisonSurface,
          format: args.format,
        }),
      ),
    },
    data_shortcut_publish_sync: {
      action: "data_shortcut_publish_sync",
      inputSchema: DataShortcutPublishSyncArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["createDataShortcut", "syncDataShortcuts"],
      execute: async (args) => {
        const publishResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_create.execute({
          action: "data_shortcut_create",
          objectType: args.objectType,
          objectName: args.objectName,
          description: args.description,
          projectFolder: args.projectFolder,
        });
        const shortcutName = args.shortcutName ?? args.objectName;
        const syncResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_sync.execute({
          action: "data_shortcut_sync",
          projectFolder: args.projectFolder,
          shortcutNames: [shortcutName],
          dryRun: args.dryRun ?? false,
        });

        return buildWorkflowResult(
          "data_shortcut_publish_sync",
          `Published and synchronized data shortcut '${shortcutName}'.`,
          [
            {
              name: "Publish data shortcut",
              action: "project.data_shortcut_create",
              status: "completed",
              result: publishResult,
            },
            {
              name: "Synchronize published shortcut",
              action: "project.data_shortcut_sync",
              status: "completed",
              result: syncResult,
            },
          ],
          {
            publish: publishResult,
            sync: syncResult,
            shortcutName,
          },
        );
      },
    },
    data_shortcut_reference_sync: {
      action: "data_shortcut_reference_sync",
      inputSchema: DataShortcutReferenceSyncArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["create", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["referenceDataShortcut", "syncDataShortcuts"],
      execute: async (args) => {
        const referenceResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_reference.execute({
          action: "data_shortcut_reference",
          projectFolder: args.projectFolder,
          shortcutName: args.shortcutName,
          shortcutType: args.shortcutType,
          layer: args.layer,
        });
        const syncResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_sync.execute({
          action: "data_shortcut_sync",
          projectFolder: args.projectFolder,
          shortcutNames: [args.shortcutName],
          dryRun: args.dryRun ?? false,
        });

        return buildWorkflowResult(
          "data_shortcut_reference_sync",
          `Referenced and synchronized data shortcut '${args.shortcutName}'.`,
          [
            {
              name: "Reference project data shortcut",
              action: "project.data_shortcut_reference",
              status: "completed",
              result: referenceResult,
            },
            {
              name: "Synchronize referenced shortcut",
              action: "project.data_shortcut_sync",
              status: "completed",
              result: syncResult,
            },
          ],
          {
            reference: referenceResult,
            sync: syncResult,
          },
        );
      },
    },
    project_startup: {
      action: "project_startup",
      inputSchema: ProjectStartupArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "inspect", "create", "manage"],
      requiresActiveDrawing: false,
      safeForRetry: false,
      pluginMethods: ["projectStartupWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("projectStartupWorkflow", {
          templatePath: args.templatePath,
          saveAs: args.saveAs,
        }),
      ),
    },
    project_reference_setup: {
      action: "project_reference_setup",
      inputSchema: ProjectReferenceSetupArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["create", "manage", "inspect"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["referenceDataShortcut", "syncDataShortcuts", "listDataShortcuts", "saveDrawing"],
      execute: async (args) => {
        const referenceResults: unknown[] = [];
        const steps: Array<z.infer<typeof WorkflowStepSchema>> = [];
        const references = args.references as Array<z.infer<typeof WorkflowShortcutReferenceSchema>>;

        for (const reference of references) {
          const result = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_reference.execute({
            action: "data_shortcut_reference",
            projectFolder: reference.projectFolder,
            shortcutName: reference.shortcutName,
            shortcutType: reference.shortcutType,
            layer: reference.layer,
          });
          referenceResults.push(result);
          steps.push({
            name: `Reference data shortcut '${reference.shortcutName}'`,
            action: "project.data_shortcut_reference",
            status: "completed",
            result,
          });
        }

        const syncProjectFolder = references[0].projectFolder;
        const syncResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_sync.execute({
          action: "data_shortcut_sync",
          projectFolder: syncProjectFolder,
          shortcutNames: references.map((reference) => reference.shortcutName),
          dryRun: args.dryRun ?? false,
        });
        steps.push({
          name: "Synchronize referenced shortcuts",
          action: "project.data_shortcut_sync",
          status: "completed",
          result: syncResult,
        });

        const dataShortcutsResult = await PROJECT_DOMAIN_DEFINITION.actions.data_shortcut_list.execute({
          action: "data_shortcut_list",
        });
        steps.push({
          name: "List data shortcuts after setup",
          action: "project.data_shortcut_list",
          status: "completed",
          result: dataShortcutsResult,
        });

        let saveResult: unknown = null;
        if (args.saveAs) {
          saveResult = await DRAWING_RUNTIME_DOMAIN_DEFINITION.actions.save.execute({
            action: "save",
            saveAs: args.saveAs,
          });
          steps.push({
            name: "Save drawing after reference setup",
            action: "drawing.save",
            status: "completed",
            result: saveResult,
          });
        } else {
          steps.push({
            name: "Save drawing after reference setup",
            action: "drawing.save",
            status: "skipped",
          });
        }

        return buildWorkflowResult(
          "project_reference_setup",
          `Completed project reference setup for ${references.length} data shortcut(s).`,
          steps,
          {
            references: referenceResults,
            sync: syncResult,
            dataShortcuts: dataShortcutsResult,
            save: saveResult,
          },
        );
      },
    },
    drawing_readiness_audit: {
      action: "drawing_readiness_audit",
      inputSchema: DrawingReadinessAuditArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "inspect", "analyze"],
      requiresActiveDrawing: true,
      safeForRetry: true,
      pluginMethods: ["drawingReadinessAuditWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("drawingReadinessAuditWorkflow", {
          layerPrefix: args.layerPrefix,
          checkLineweights: args.checkLineweights,
          checkColors: args.checkColors,
          limit: args.limit,
        }),
      ),
    },
    feature_line_to_grading: {
      action: "feature_line_to_grading",
      inputSchema: FeatureLineToGradingArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "create", "edit", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["featureLineToGradingWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("featureLineToGradingWorkflow", {
          featureLineName: args.featureLineName,
          groupName: args.groupName,
          groupDescription: args.groupDescription,
          createGroup: args.createGroup,
          useProjection: args.useProjection,
          criteriaName: args.criteriaName,
          side: args.side,
          surfaceName: args.surfaceName,
        }),
      ),
    },
    pipe_network_design: {
      action: "pipe_network_design",
      inputSchema: PipeNetworkDesignArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["analyze", "edit", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: [
        "getPipeNetwork",
        "listPipePartsCatalog",
        "resizePipeInNetwork",
        "analyzePipeNetworkHydraulics",
      ],
      execute: async (args) => {
        const sizingResult = await PIPE_DOMAIN_DEFINITION.actions.size_network.execute({
          action: "size_network",
          networkName: args.networkName,
          partsList: args.partsList,
          defaultDesignFlow: args.defaultDesignFlow,
          perPipeDesignFlows: args.perPipeDesignFlows,
          manningsN: args.manningsN,
          targetVelocityMin: args.targetVelocityMin,
          targetVelocityMax: args.targetVelocityMax,
          applyChanges: args.applyChanges ?? false,
        });

        const steps: Array<z.infer<typeof WorkflowStepSchema>> = [
          {
            name: "Size gravity pipe network",
            action: "pipe.size_network",
            status: "completed",
            result: sizingResult,
          },
        ];
        const warnings: string[] = [];
        let hydraulicAnalysisResult: unknown;

        if (args.runHydraulicAnalysis ?? true) {
          hydraulicAnalysisResult = await PIPE_DOMAIN_DEFINITION.actions.hydraulic_analysis.execute({
            action: "hydraulic_analysis",
            networkName: args.networkName,
            designFlow: args.designFlow ?? args.defaultDesignFlow,
            manningsN: args.manningsN,
            minCoverDepth: args.minCoverDepth,
            minVelocity: args.minVelocity,
            maxVelocity: args.maxVelocity,
            minSlope: args.minSlope,
          });

          steps.push({
            name: "Run hydraulic analysis",
            action: "pipe.hydraulic_analysis",
            status: "completed",
            result: hydraulicAnalysisResult,
          });
        } else {
          warnings.push("Hydraulic analysis was skipped because runHydraulicAnalysis was false.");
          steps.push({
            name: "Run hydraulic analysis",
            action: "pipe.hydraulic_analysis",
            status: "skipped",
          });
        }

        return buildWorkflowResult(
          "pipe_network_design",
          `Completed pipe-network sizing workflow for '${args.networkName}'.`,
          steps,
          {
            sizing: sizingResult,
            hydraulicAnalysis: hydraulicAnalysisResult ?? null,
          },
          warnings,
        );
      },
    },
    plan_production_publish: {
      action: "plan_production_publish",
      inputSchema: PlanProductionPublishArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["export", "generate"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["planProductionPublishWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("planProductionPublishWorkflow", {
          outputPath: args.outputPath,
          sheetSetName: args.sheetSetName,
          layoutNames: args.layoutNames,
          plotStyleTable: args.plotStyleTable,
          paperSize: args.paperSize,
        }),
      ),
    },
    qc_fix_and_verify: {
      action: "qc_fix_and_verify",
      inputSchema: QcFixAndVerifyArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["query", "analyze", "edit", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["qcFixAndVerifyWorkflow"],
      execute: async (args) => await withApplicationConnection(
        async (appClient) => await appClient.sendCommand("qcFixAndVerifyWorkflow", {
          layerPrefix: args.layerPrefix,
          checkLineweights: args.checkLineweights,
          checkColors: args.checkColors,
          fixSpaces: args.fixSpaces,
          maxNameLength: args.maxNameLength,
          colorIndex: args.colorIndex,
          lineweight: args.lineweight,
          dryRun: args.dryRun,
        }),
      ),
    },
    survey_import_adjust_figures: {
      action: "survey_import_adjust_figures",
      inputSchema: SurveyImportAdjustFiguresArgsSchema,
      responseSchema: WorkflowResponseSchema,
      capabilities: ["create", "import", "analyze", "manage"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      pluginMethods: ["importSurveyLandXml", "listSurveyObservations", "adjustSurveyNetwork", "createSurveyFigure", "listSurveyFigures"],
      execute: async (args) => {
        const importResult = await SURVEY_DOMAIN_DEFINITION.actions.landxml_import.execute({
          action: "landxml_import",
          filePath: args.filePath,
          databaseName: args.databaseName,
          importPoints: args.importPoints,
          importAlignments: args.importAlignments,
          importSurfaces: args.importSurfaces,
          coordinateSystemOverride: args.coordinateSystemOverride,
          duplicatePolicy: args.duplicatePolicy,
        });

        const steps: Array<z.infer<typeof WorkflowStepSchema>> = [
          {
            name: "Import survey LandXML",
            action: "survey.landxml_import",
            status: "completed",
            result: importResult,
          },
        ];
        const warnings: string[] = [];
        let observationsResult: unknown = null;
        let adjustmentResult: unknown = null;
        let figureCreateResult: unknown = null;

        if (args.networkName) {
          observationsResult = await SURVEY_DOMAIN_DEFINITION.actions.observation_list.execute({
            action: "observation_list",
            databaseName: args.databaseName,
            networkName: args.networkName,
            observationType: "all",
          });
          steps.push({
            name: "List imported survey observations",
            action: "survey.observation_list",
            status: "completed",
            result: observationsResult,
          });

          adjustmentResult = await SURVEY_DOMAIN_DEFINITION.actions.network_adjust.execute({
            action: "network_adjust",
            databaseName: args.databaseName,
            networkName: args.networkName,
            method: args.method,
            confidenceLevel: args.confidenceLevel,
            applyAdjustment: args.applyAdjustment,
          });
          steps.push({
            name: "Adjust survey network",
            action: "survey.network_adjust",
            status: "completed",
            result: adjustmentResult,
          });
        } else {
          warnings.push("No networkName was provided, so observation review and network adjustment were skipped.");
          steps.push({
            name: "List imported survey observations",
            action: "survey.observation_list",
            status: "skipped",
          });
          steps.push({
            name: "Adjust survey network",
            action: "survey.network_adjust",
            status: "skipped",
          });
        }

        if (args.figureName && args.pointNumbers) {
          figureCreateResult = await SURVEY_DOMAIN_DEFINITION.actions.figure_create.execute({
            action: "figure_create",
            databaseName: args.databaseName,
            figureName: args.figureName,
            pointNumbers: args.pointNumbers,
            figureStyle: args.figureStyle,
            closed: args.closed,
            layer: args.layer,
          });
          steps.push({
            name: "Create survey figure",
            action: "survey.figure_create",
            status: "completed",
            result: figureCreateResult,
          });
        } else {
          warnings.push("No figureName/pointNumbers pair was provided, so survey-figure creation was skipped.");
          steps.push({
            name: "Create survey figure",
            action: "survey.figure_create",
            status: "skipped",
          });
        }

        const figureListResult = await SURVEY_DOMAIN_DEFINITION.actions.figure_list.execute({
          action: "figure_list",
          databaseName: args.databaseName,
        });
        steps.push({
          name: "List survey figures",
          action: "survey.figure_list",
          status: "completed",
          result: figureListResult,
        });

        return buildWorkflowResult(
          "survey_import_adjust_figures",
          `Completed survey import workflow for database '${args.databaseName}'.`,
          steps,
          {
            import: importResult,
            observations: observationsResult,
            adjustment: adjustmentResult,
            createdFigure: figureCreateResult,
            figures: figureListResult,
          },
          warnings,
        );
      },
    },
  },
  exposures: [
    {
      toolName: "civil3d_workflow",
      displayName: "Civil 3D Workflow",
      description: "Runs multi-step Civil 3D workflows that compose existing QC, grading, surface, project, survey, pipe-design, standards, and plan-production operations through a single domain tool.",
      inputShape: canonicalWorkflowInputShape,
      supportedActions: [
        "corridor_qc_report",
        "grading_surface_volume",
        "surface_comparison_report",
        "data_shortcut_publish_sync",
        "data_shortcut_reference_sync",
        "project_startup",
        "project_reference_setup",
        "drawing_readiness_audit",
        "feature_line_to_grading",
        "pipe_network_design",
        "plan_production_publish",
        "qc_fix_and_verify",
        "survey_import_adjust_figures",
      ],
      capabilities: ["query", "analyze", "generate", "edit", "manage", "export", "create", "import"],
      requiresActiveDrawing: true,
      safeForRetry: false,
      resolveAction: (rawArgs) => ({ action: String(rawArgs.action ?? ""), args: rawArgs }),
    },
    {
      toolName: "civil3d_workflow_corridor_qc_report",
      displayName: "Civil 3D Workflow Corridor QC Report",
      description: "Runs corridor QC and optionally generates a consolidated QC report file.",
      inputShape: {
        corridorName: z.string(),
        outputPath: z.string().optional(),
        includeAlignments: z.boolean().optional(),
        includeProfiles: z.boolean().optional(),
        includePipeNetworks: z.boolean().optional(),
        includeSurfaces: z.boolean().optional(),
        includeLabels: z.boolean().optional(),
      },
      supportedActions: ["corridor_qc_report"],
      resolveAction: (rawArgs) => ({
        action: "corridor_qc_report",
        args: {
          action: "corridor_qc_report",
          corridorName: rawArgs.corridorName,
          outputPath: rawArgs.outputPath,
          includeAlignments: rawArgs.includeAlignments,
          includeProfiles: rawArgs.includeProfiles,
          includePipeNetworks: rawArgs.includePipeNetworks,
          includeSurfaces: rawArgs.includeSurfaces,
          includeLabels: rawArgs.includeLabels,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_grading_surface_volume",
      displayName: "Civil 3D Workflow Grading Surface Volume",
      description: "Calculates grading surface cut/fill volume between an existing and proposed surface.",
      inputShape: {
        baseSurface: z.string(),
        comparisonSurface: z.string(),
        method: SurfaceVolumeMethodSchema.optional(),
      },
      supportedActions: ["grading_surface_volume"],
      resolveAction: (rawArgs) => ({
        action: "grading_surface_volume",
        args: {
          action: "grading_surface_volume",
          baseSurface: rawArgs.baseSurface,
          comparisonSurface: rawArgs.comparisonSurface,
          method: rawArgs.method,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_surface_comparison_report",
      displayName: "Civil 3D Workflow Surface Comparison Report",
      description: "Runs a structured surface comparison and follows it with a volume report.",
      inputShape: {
        baseSurface: z.string(),
        comparisonSurface: z.string(),
        format: WorkflowVolumeReportFormatSchema.optional(),
      },
      supportedActions: ["surface_comparison_report"],
      resolveAction: (rawArgs) => ({
        action: "surface_comparison_report",
        args: {
          action: "surface_comparison_report",
          baseSurface: rawArgs.baseSurface,
          comparisonSurface: rawArgs.comparisonSurface,
          format: rawArgs.format,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_data_shortcut_publish_sync",
      displayName: "Civil 3D Workflow Data Shortcut Publish Sync",
      description: "Publishes a data shortcut for a Civil 3D object and immediately synchronizes it.",
      inputShape: {
        objectType: WorkflowDataShortcutObjectTypeSchema,
        objectName: z.string(),
        shortcutName: z.string().optional(),
        description: z.string().optional(),
        projectFolder: z.string().optional(),
        dryRun: z.boolean().optional(),
      },
      supportedActions: ["data_shortcut_publish_sync"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_publish_sync",
        args: {
          action: "data_shortcut_publish_sync",
          objectType: rawArgs.objectType,
          objectName: rawArgs.objectName,
          shortcutName: rawArgs.shortcutName,
          description: rawArgs.description,
          projectFolder: rawArgs.projectFolder,
          dryRun: rawArgs.dryRun,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_data_shortcut_reference_sync",
      displayName: "Civil 3D Workflow Data Shortcut Reference Sync",
      description: "References a project data shortcut into the current drawing and immediately synchronizes it.",
      inputShape: {
        projectFolder: z.string(),
        shortcutName: z.string(),
        shortcutType: WorkflowDataShortcutObjectTypeSchema,
        layer: z.string().optional(),
        dryRun: z.boolean().optional(),
      },
      supportedActions: ["data_shortcut_reference_sync"],
      resolveAction: (rawArgs) => ({
        action: "data_shortcut_reference_sync",
        args: {
          action: "data_shortcut_reference_sync",
          projectFolder: rawArgs.projectFolder,
          shortcutName: rawArgs.shortcutName,
          shortcutType: rawArgs.shortcutType,
          layer: rawArgs.layer,
          dryRun: rawArgs.dryRun,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_project_startup",
      displayName: "Civil 3D Workflow Project Startup",
      description: "Checks plugin health, optionally creates a startup drawing, inspects drawing readiness, lists data shortcuts, and can save the startup drawing.",
      inputShape: {
        templatePath: z.string().optional(),
        saveAs: z.string().optional(),
      },
      supportedActions: ["project_startup"],
      requiresActiveDrawing: false,
      resolveAction: (rawArgs) => ({
        action: "project_startup",
        args: {
          action: "project_startup",
          templatePath: rawArgs.templatePath,
          saveAs: rawArgs.saveAs,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_project_reference_setup",
      displayName: "Civil 3D Workflow Project Reference Setup",
      description: "References one or more project data shortcuts, synchronizes them, reviews the resulting shortcut state, and can save the drawing.",
      inputShape: {
        references: z.array(WorkflowShortcutReferenceSchema).min(1),
        dryRun: z.boolean().optional(),
        saveAs: z.string().optional(),
      },
      supportedActions: ["project_reference_setup"],
      resolveAction: (rawArgs) => ({
        action: "project_reference_setup",
        args: {
          action: "project_reference_setup",
          references: rawArgs.references,
          dryRun: rawArgs.dryRun,
          saveAs: rawArgs.saveAs,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_drawing_readiness_audit",
      displayName: "Civil 3D Workflow Drawing Readiness Audit",
      description: "Checks plugin health, drawing metadata, settings, object types, current selection, and drawing standards in one readiness audit.",
      inputShape: {
        layerPrefix: z.string().optional(),
        checkLineweights: z.boolean().optional(),
        checkColors: z.boolean().optional(),
        limit: z.number().int().positive().optional(),
      },
      supportedActions: ["drawing_readiness_audit"],
      resolveAction: (rawArgs) => ({
        action: "drawing_readiness_audit",
        args: {
          action: "drawing_readiness_audit",
          layerPrefix: rawArgs.layerPrefix,
          checkLineweights: rawArgs.checkLineweights,
          checkColors: rawArgs.checkColors,
          limit: rawArgs.limit,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_feature_line_to_grading",
      displayName: "Civil 3D Workflow Feature Line To Grading",
      description: "Inspects a feature line, optionally creates a grading group, builds grading from it, and can generate a grading surface.",
      inputShape: {
        featureLineName: z.string(),
        groupName: z.string(),
        groupDescription: z.string().optional(),
        createGroup: z.boolean().optional(),
        useProjection: z.boolean().optional(),
        criteriaName: z.string().optional(),
        side: WorkflowGradingSideSchema.optional(),
        surfaceName: z.string().optional(),
      },
      supportedActions: ["feature_line_to_grading"],
      resolveAction: (rawArgs) => ({
        action: "feature_line_to_grading",
        args: {
          action: "feature_line_to_grading",
          featureLineName: rawArgs.featureLineName,
          groupName: rawArgs.groupName,
          groupDescription: rawArgs.groupDescription,
          createGroup: rawArgs.createGroup,
          useProjection: rawArgs.useProjection,
          criteriaName: rawArgs.criteriaName,
          side: rawArgs.side,
          surfaceName: rawArgs.surfaceName,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_pipe_network_design",
      displayName: "Civil 3D Workflow Pipe Network Design",
      description: "Sizes a gravity pipe network and optionally follows it with a hydraulic analysis pass.",
      inputShape: {
        networkName: z.string(),
        partsList: z.string().optional(),
        defaultDesignFlow: z.number().positive().optional(),
        perPipeDesignFlows: z.array(PipeFlowSchema).optional(),
        manningsN: z.number().positive().optional(),
        targetVelocityMin: z.number().positive().optional(),
        targetVelocityMax: z.number().positive().optional(),
        applyChanges: z.boolean().optional(),
        runHydraulicAnalysis: z.boolean().optional(),
        designFlow: z.number().positive().optional(),
        minCoverDepth: z.number().nonnegative().optional(),
        minVelocity: z.number().nonnegative().optional(),
        maxVelocity: z.number().positive().optional(),
        minSlope: z.number().nonnegative().optional(),
      },
      supportedActions: ["pipe_network_design"],
      resolveAction: (rawArgs) => ({
        action: "pipe_network_design",
        args: {
          action: "pipe_network_design",
          networkName: rawArgs.networkName,
          partsList: rawArgs.partsList,
          defaultDesignFlow: rawArgs.defaultDesignFlow,
          perPipeDesignFlows: rawArgs.perPipeDesignFlows,
          manningsN: rawArgs.manningsN,
          targetVelocityMin: rawArgs.targetVelocityMin,
          targetVelocityMax: rawArgs.targetVelocityMax,
          applyChanges: rawArgs.applyChanges,
          runHydraulicAnalysis: rawArgs.runHydraulicAnalysis,
          designFlow: rawArgs.designFlow,
          minCoverDepth: rawArgs.minCoverDepth,
          minVelocity: rawArgs.minVelocity,
          maxVelocity: rawArgs.maxVelocity,
          minSlope: rawArgs.minSlope,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_plan_production_publish",
      displayName: "Civil 3D Workflow Plan Production Publish",
      description: "Publishes either a named sheet set or an explicit list of layouts to a PDF output.",
      inputShape: {
        outputPath: z.string(),
        sheetSetName: z.string().optional(),
        layoutNames: z.array(z.string()).optional(),
        plotStyleTable: z.string().optional(),
        paperSize: z.string().optional(),
      },
      supportedActions: ["plan_production_publish"],
      resolveAction: (rawArgs) => ({
        action: "plan_production_publish",
        args: {
          action: "plan_production_publish",
          outputPath: rawArgs.outputPath,
          sheetSetName: rawArgs.sheetSetName,
          layoutNames: rawArgs.layoutNames,
          plotStyleTable: rawArgs.plotStyleTable,
          paperSize: rawArgs.paperSize,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_qc_fix_and_verify",
      displayName: "Civil 3D Workflow QC Fix And Verify",
      description: "Audits drawing standards, applies fixes, and re-runs the audit to verify compliance.",
      inputShape: {
        layerPrefix: z.string().optional(),
        checkLineweights: z.boolean().optional(),
        checkColors: z.boolean().optional(),
        fixSpaces: z.boolean().optional(),
        maxNameLength: z.number().int().positive().optional(),
        colorIndex: z.number().int().min(1).max(255).optional(),
        lineweight: z.number().int().optional(),
        dryRun: z.boolean().optional(),
      },
      supportedActions: ["qc_fix_and_verify"],
      resolveAction: (rawArgs) => ({
        action: "qc_fix_and_verify",
        args: {
          action: "qc_fix_and_verify",
          layerPrefix: rawArgs.layerPrefix,
          checkLineweights: rawArgs.checkLineweights,
          checkColors: rawArgs.checkColors,
          fixSpaces: rawArgs.fixSpaces,
          maxNameLength: rawArgs.maxNameLength,
          colorIndex: rawArgs.colorIndex,
          lineweight: rawArgs.lineweight,
          dryRun: rawArgs.dryRun,
        },
      }),
    },
    {
      toolName: "civil3d_workflow_survey_import_adjust_figures",
      displayName: "Civil 3D Workflow Survey Import Adjust Figures",
      description: "Imports survey LandXML, optionally adjusts a network, optionally creates a figure, and lists resulting survey figures.",
      inputShape: {
        filePath: z.string(),
        databaseName: z.string(),
        networkName: z.string().optional(),
        method: WorkflowSurveyAdjustMethodSchema.optional(),
        confidenceLevel: z.number().min(50).max(99.9).optional(),
        applyAdjustment: z.boolean().optional(),
        figureName: z.string().optional(),
        pointNumbers: z.array(z.number().int().positive()).min(2).optional(),
        figureStyle: z.string().optional(),
        closed: z.boolean().optional(),
        layer: z.string().optional(),
        importPoints: z.boolean().optional(),
        importAlignments: z.boolean().optional(),
        importSurfaces: z.boolean().optional(),
        coordinateSystemOverride: z.string().optional(),
        duplicatePolicy: WorkflowDuplicatePolicySchema.optional(),
      },
      supportedActions: ["survey_import_adjust_figures"],
      resolveAction: (rawArgs) => ({
        action: "survey_import_adjust_figures",
        args: {
          action: "survey_import_adjust_figures",
          filePath: rawArgs.filePath,
          databaseName: rawArgs.databaseName,
          networkName: rawArgs.networkName,
          method: rawArgs.method,
          confidenceLevel: rawArgs.confidenceLevel,
          applyAdjustment: rawArgs.applyAdjustment,
          figureName: rawArgs.figureName,
          pointNumbers: rawArgs.pointNumbers,
          figureStyle: rawArgs.figureStyle,
          closed: rawArgs.closed,
          layer: rawArgs.layer,
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
