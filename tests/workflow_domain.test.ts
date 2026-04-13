import { beforeEach, describe, expect, it, vi } from "vitest";

const { sendCommandMock } = vi.hoisted(() => ({
  sendCommandMock: vi.fn(),
}));

vi.mock("../src/utils/ConnectionManager.js", () => ({
  withApplicationConnection: async <T>(
    operation: (client: { sendCommand: typeof sendCommandMock }) => Promise<T>,
  ) => await operation({
    sendCommand: sendCommandMock,
  }),
}));

import { WORKFLOW_DOMAIN_DEFINITION } from "../src/tools/domains/workflowDomain.js";

describe("workflow domain execution", () => {
  beforeEach(() => {
    sendCommandMock.mockReset();
  });

  it("runs corridor QC and skips report generation when no output path is provided", async () => {
    sendCommandMock.mockImplementation(async (commandName: string) => {
      if (commandName === "corridorQcReportWorkflow") {
        return {
          workflow: "corridor_qc_report",
          status: "completed_with_warnings",
          summary: "Completed corridor QC workflow for 'COR-1'.",
          steps: [
            { name: "Run corridor QC check", action: "qc.check_corridor", status: "completed" },
            { name: "Generate consolidated QC report", action: "qc.generate_report", status: "skipped" },
          ],
          outputs: { corridorCheck: { corridorName: "COR-1" }, report: null },
          warnings: ["No outputPath was provided, so consolidated QC report generation was skipped."],
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.corridor_qc_report.execute({
      action: "corridor_qc_report",
      corridorName: "COR-1",
    });

    expect(sendCommandMock).toHaveBeenCalledTimes(1);
    expect(sendCommandMock).toHaveBeenCalledWith("corridorQcReportWorkflow", {
      corridorName: "COR-1",
      outputPath: undefined,
      includeAlignments: undefined,
      includeProfiles: undefined,
      includePipeNetworks: undefined,
      includeSurfaces: undefined,
      includeLabels: undefined,
    });
    expect(result.workflow).toBe("corridor_qc_report");
    expect(result.status).toBe("completed_with_warnings");
    expect(result.steps).toHaveLength(2);
    expect(result.steps[0]).toMatchObject({
      action: "qc.check_corridor",
      status: "completed",
    });
    expect(result.steps[1]).toMatchObject({
      action: "qc.generate_report",
      status: "skipped",
    });
    expect(result.warnings[0]).toContain("outputPath");
    expect(result.outputs.report).toBeNull();
  });

  it("calculates grading surface volume through the surface domain workflow", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "calculateSurfaceVolume") {
        return {
          baseSurface: args.baseSurface,
          comparisonSurface: args.comparisonSurface,
          method: args.method,
          cutVolume: 1250,
          fillVolume: 900,
          netVolume: -350,
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.grading_surface_volume.execute({
      action: "grading_surface_volume",
      baseSurface: "EG",
      comparisonSurface: "FG",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("calculateSurfaceVolume", {
      baseSurface: "EG",
      comparisonSurface: "FG",
      method: "tin_volume",
    });
    expect(result.status).toBe("completed");
    expect(result.steps).toHaveLength(1);
    expect(result.outputs.volume).toMatchObject({
      baseSurface: "EG",
      comparisonSurface: "FG",
      method: "tin_volume",
    });
  });

  it("runs a structured surface comparison and follows it with a report", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "surfaceComparisonReportWorkflow":
          return {
            workflow: "surface_comparison_report",
            status: "completed",
            summary: "Completed surface comparison workflow for 'EG' vs 'FG'.",
            steps: [
              { name: "Run structured surface comparison", action: "surface.comparison_workflow", status: "completed" },
              { name: "Generate surface volume report", action: "surface.volume_report", status: "completed" },
            ],
            outputs: {
              comparison: { baseSurface: { name: "EG" }, comparisonSurface: { name: "FG" }, volume: { netVolume: -200 } },
              report: { format: args.format, netVolume: -200, narrative: "Cut exceeds fill." },
            },
            warnings: [],
          };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.surface_comparison_report.execute({
      action: "surface_comparison_report",
      baseSurface: "EG",
      comparisonSurface: "FG",
      format: "detailed",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("surfaceComparisonReportWorkflow", {
      baseSurface: "EG",
      comparisonSurface: "FG",
      format: "detailed",
    });
    expect(result.status).toBe("completed");
    expect(result.steps).toHaveLength(2);
    expect(result.outputs.report).toMatchObject({ format: "detailed" });
  });

  it("publishes and synchronizes a data shortcut", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "createDataShortcut") {
        return {
          objectName: args.objectName,
          objectType: args.objectType,
          created: true,
        };
      }
      if (commandName === "syncDataShortcuts") {
        return {
          projectFolder: args.projectFolder,
          shortcutNames: args.shortcutNames,
          synced: true,
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.data_shortcut_publish_sync.execute({
      action: "data_shortcut_publish_sync",
      objectType: "surface",
      objectName: "FG",
      projectFolder: "C:/Projects/Roadway",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("createDataShortcut", {
      objectType: "surface",
      objectName: "FG",
      description: null,
      projectFolder: "C:/Projects/Roadway",
    });
    expect(sendCommandMock).toHaveBeenCalledWith("syncDataShortcuts", {
      projectFolder: "C:/Projects/Roadway",
      shortcutNames: ["FG"],
      dryRun: false,
    });
    expect(result.status).toBe("completed");
    expect(result.outputs.shortcutName).toBe("FG");
  });

  it("references and synchronizes a data shortcut", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "referenceDataShortcut") {
        return {
          shortcutName: args.shortcutName,
          referenced: true,
        };
      }
      if (commandName === "syncDataShortcuts") {
        return {
          shortcutNames: args.shortcutNames,
          synced: true,
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.data_shortcut_reference_sync.execute({
      action: "data_shortcut_reference_sync",
      projectFolder: "C:/Projects/Roadway",
      shortcutName: "FG",
      shortcutType: "surface",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("referenceDataShortcut", {
      projectFolder: "C:/Projects/Roadway",
      shortcutName: "FG",
      shortcutType: "surface",
      layer: null,
    });
    expect(sendCommandMock).toHaveBeenCalledWith("syncDataShortcuts", {
      projectFolder: "C:/Projects/Roadway",
      shortcutNames: ["FG"],
      dryRun: false,
    });
    expect(result.status).toBe("completed");
    expect(result.steps.map((step) => step.action)).toEqual([
      "project.data_shortcut_reference",
      "project.data_shortcut_sync",
    ]);
  });

  it("runs project startup readiness and saves when requested", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "projectStartupWorkflow":
          return {
            workflow: "project_startup",
            status: "completed_with_warnings",
            summary: "Completed project startup and drawing readiness workflow.",
            steps: [
              { name: "Check Civil 3D health", action: "plugin.health", status: "completed" },
              { name: "Create or open startup drawing from template", action: "drawing.new", status: "completed" },
              { name: "Inspect drawing info", action: "drawing.info", status: "completed" },
              { name: "Inspect drawing settings", action: "drawing.settings", status: "completed" },
              { name: "List Civil 3D object types", action: "drawing.list_object_types", status: "completed" },
              { name: "List project data shortcuts", action: "project.data_shortcut_list", status: "skipped" },
              { name: "Save startup drawing", action: "drawing.save", status: "completed" },
            ],
            outputs: {
              health: { connected: true },
              save: { saved: true, saveAs: args.saveAs },
            },
            warnings: ["Project data-shortcut listing is not yet implemented in the native plugin workflow layer, so that step was skipped."],
          };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.project_startup.execute({
      action: "project_startup",
      templatePath: "C:/templates/civil3d.dwt",
      saveAs: "C:/projects/startup.dwg",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("projectStartupWorkflow", {
      templatePath: "C:/templates/civil3d.dwt",
      saveAs: "C:/projects/startup.dwg",
    });
    expect(result.status).toBe("completed_with_warnings");
    expect(result.steps.map((step) => step.action)).toEqual([
      "plugin.health",
      "drawing.new",
      "drawing.info",
      "drawing.settings",
      "drawing.list_object_types",
      "project.data_shortcut_list",
      "drawing.save",
    ]);
    expect(result.outputs.health).toMatchObject({ connected: true });
    expect(result.outputs.save).toMatchObject({ saved: true, saveAs: "C:/projects/startup.dwg" });
  });

  it("references multiple project shortcuts, syncs them, and saves the drawing", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "referenceDataShortcut":
          return { shortcutName: args.shortcutName, referenced: true };
        case "syncDataShortcuts":
          return { shortcutNames: args.shortcutNames, synced: true };
        case "listDataShortcuts":
          return { incoming: [{ objectName: "FG", objectType: "surface", sourceFilePath: "C:/p/fg.dwg", isSynced: true, isValid: true }], exportable: [] };
        case "saveDrawing":
          return { saved: true, saveAs: args.saveAs };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.project_reference_setup.execute({
      action: "project_reference_setup",
      references: [
        { projectFolder: "C:/Projects/Roadway", shortcutName: "FG", shortcutType: "surface" },
        { projectFolder: "C:/Projects/Roadway", shortcutName: "CL-1", shortcutType: "alignment" },
      ],
      saveAs: "C:/Projects/Roadway/ref-setup.dwg",
    });

    expect(result.status).toBe("completed");
    expect(result.steps.map((step) => step.action)).toEqual([
      "project.data_shortcut_reference",
      "project.data_shortcut_reference",
      "project.data_shortcut_sync",
      "project.data_shortcut_list",
      "drawing.save",
    ]);
    expect(result.outputs.references).toHaveLength(2);
    expect(result.outputs.save).toMatchObject({ saved: true });
  });

  it("runs a drawing readiness audit across health, drawing state, selection, and standards", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "drawingReadinessAuditWorkflow":
          return {
            workflow: "drawing_readiness_audit",
            status: "completed",
            summary: "Completed drawing readiness audit workflow.",
            steps: [
              { name: "Check Civil 3D health", action: "plugin.health", status: "completed" },
              { name: "Inspect drawing info", action: "drawing.info", status: "completed" },
              { name: "Inspect drawing settings", action: "drawing.settings", status: "completed" },
              { name: "List Civil 3D object types", action: "drawing.list_object_types", status: "completed" },
              { name: "Inspect selected Civil 3D objects", action: "drawing.selected_objects_info", status: "completed" },
              { name: "Audit drawing standards", action: "standards.check_drawing_standards", status: "completed" },
            ],
            outputs: {
              selectedObjects: [{ handle: "1A", objectType: "Alignment", name: "CL-1" }],
              standardsAudit: { violations: 1, layerPrefix: args.layerPrefix },
            },
            warnings: [],
          };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.drawing_readiness_audit.execute({
      action: "drawing_readiness_audit",
      layerPrefix: "C-",
      limit: 10,
    });

    expect(sendCommandMock).toHaveBeenCalledWith("drawingReadinessAuditWorkflow", {
      layerPrefix: "C-",
      checkLineweights: undefined,
      checkColors: undefined,
      limit: 10,
    });
    expect(result.status).toBe("completed");
    expect(result.steps.map((step) => step.action)).toEqual([
      "plugin.health",
      "drawing.info",
      "drawing.settings",
      "drawing.list_object_types",
      "drawing.selected_objects_info",
      "standards.check_drawing_standards",
    ]);
    expect(result.outputs.selectedObjects).toEqual([{ handle: "1A", objectType: "Alignment", name: "CL-1" }]);
    expect(result.outputs.standardsAudit).toMatchObject({ violations: 1 });
  });

  it("creates grading from a feature line and skips surface creation when surfaceName is missing", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "featureLineToGradingWorkflow") {
        return {
          workflow: "feature_line_to_grading",
          status: "completed_with_warnings",
          summary: "Converted feature line 'PAD-EDGE' into grading in group 'GG-1'.",
          steps: [
            { name: "Inspect source feature line", action: "grading.feature_line_get", status: "completed" },
            { name: "Create grading group", action: "grading.group_create", status: "skipped" },
            { name: "Create grading from feature line", action: "grading.create", status: "completed" },
            { name: "Create grading surface", action: "grading.group_surface_create", status: "skipped" },
          ],
          outputs: { surface: null },
          warnings: ["No surfaceName was provided, so grading-surface creation was skipped."],
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.feature_line_to_grading.execute({
      action: "feature_line_to_grading",
      featureLineName: "PAD-EDGE",
      groupName: "GG-1",
      createGroup: false,
    });

    expect(sendCommandMock).toHaveBeenCalledWith("featureLineToGradingWorkflow", {
      featureLineName: "PAD-EDGE",
      groupName: "GG-1",
      groupDescription: undefined,
      createGroup: false,
      useProjection: undefined,
      criteriaName: undefined,
      side: undefined,
      surfaceName: undefined,
    });
    expect(result.status).toBe("completed_with_warnings");
    expect(result.steps).toHaveLength(4);
    expect(result.steps[1]).toMatchObject({ action: "grading.group_create", status: "skipped" });
    expect(result.steps[2]).toMatchObject({ action: "grading.create", status: "completed" });
    expect(result.steps[3]).toMatchObject({ action: "grading.group_surface_create", status: "skipped" });
    expect(result.outputs.surface).toBeNull();
  });

  it("sizes a pipe network and then runs hydraulic analysis", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "getPipeNetwork":
          return {
            name: args.name,
            partsList: "RCP",
            pipes: [
              { name: "P-1", diameter: 12, slope: 1, length: 100 },
            ],
          };
        case "listPipePartsCatalog":
          return {
            partsLists: [
              { name: "RCP", parts: ["12in RCP", "18in RCP", "24in RCP"] },
            ],
          };
        case "analyzePipeNetworkHydraulics":
          return {
            networkName: args.networkName,
            checksPassed: true,
          };
        case "resizePipeInNetwork":
          return { resized: true };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.pipe_network_design.execute({
      action: "pipe_network_design",
      networkName: "STM-1",
      defaultDesignFlow: 4,
      runHydraulicAnalysis: true,
      applyChanges: false,
      manningsN: 0.013,
      minCoverDepth: 2,
      minVelocity: 2,
      maxVelocity: 10,
      minSlope: 0.5,
    });

    expect(sendCommandMock).toHaveBeenCalledWith("getPipeNetwork", { name: "STM-1" });
    expect(sendCommandMock).toHaveBeenCalledWith("listPipePartsCatalog", { partsList: "RCP" });
    expect(sendCommandMock).toHaveBeenCalledWith("analyzePipeNetworkHydraulics", {
      networkName: "STM-1",
      designFlow: 4,
      manningsN: 0.013,
      minCoverDepth: 2,
      minVelocity: 2,
      maxVelocity: 10,
      minSlope: 0.5,
    });
    expect(result.status).toBe("completed");
    expect(result.steps).toHaveLength(2);
    expect(result.outputs.sizing).toBeDefined();
    expect(result.outputs.hydraulicAnalysis).toMatchObject({
      networkName: "STM-1",
      checksPassed: true,
    });
  });

  it("publishes a sheet set through the plan production workflow", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "planProductionPublishWorkflow") {
        return {
          workflow: "plan_production_publish",
          status: "completed",
          summary: "Completed plan-production publish workflow for sheet set 'Road Plans'.",
          steps: [{ name: "Export sheet set", action: "plan_production.sheet_set_export", status: "completed" }],
          outputs: { publish: { sheetSetName: args.sheetSetName, outputPath: args.outputPath, sheetsExported: 12, exported: true } },
          warnings: [],
        };
      }
      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.plan_production_publish.execute({
      action: "plan_production_publish",
      sheetSetName: "Road Plans",
      outputPath: "C:/temp/road-plans.pdf",
    });

    expect(sendCommandMock).toHaveBeenCalledWith("planProductionPublishWorkflow", {
      outputPath: "C:/temp/road-plans.pdf",
      sheetSetName: "Road Plans",
      layoutNames: undefined,
      plotStyleTable: undefined,
      paperSize: undefined,
    });
    expect(result.status).toBe("completed");
    expect(result.steps).toHaveLength(1);
    expect(result.summary).toContain("Road Plans");
  });

  it("audits, fixes, and re-verifies drawing standards", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      if (commandName === "qcFixAndVerifyWorkflow") {
        return {
          workflow: "qc_fix_and_verify",
          status: "completed",
          summary: "Completed drawing-standards fix-and-verify workflow.",
          steps: [
            { name: "Run baseline standards audit", action: "standards.check_drawing_standards", status: "completed" },
            { name: "Apply drawing standards fixes", action: "standards.fix_drawing_standards", status: "completed" },
            { name: "Re-run standards audit", action: "standards.check_drawing_standards", status: "completed" },
          ],
          outputs: {
            initialCheck: { violations: 4, layerPrefix: args.layerPrefix },
            verificationCheck: { violations: 0, layerPrefix: args.layerPrefix },
          },
          warnings: [],
        };
      }

      throw new Error(`Unexpected command: ${commandName}`);
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.qc_fix_and_verify.execute({
      action: "qc_fix_and_verify",
      layerPrefix: "C-",
      fixSpaces: true,
      dryRun: false,
    });

    expect(sendCommandMock).toHaveBeenCalledTimes(1);
    expect(sendCommandMock).toHaveBeenCalledWith("qcFixAndVerifyWorkflow", {
      layerPrefix: "C-",
      checkLineweights: undefined,
      checkColors: undefined,
      fixSpaces: true,
      maxNameLength: undefined,
      colorIndex: undefined,
      lineweight: undefined,
      dryRun: false,
    });
    expect(result.status).toBe("completed");
    expect(result.steps.map((step) => step.action)).toEqual([
      "standards.check_drawing_standards",
      "standards.fix_drawing_standards",
      "standards.check_drawing_standards",
    ]);
    expect(result.outputs.initialCheck).toMatchObject({ violations: 4 });
    expect(result.outputs.verificationCheck).toMatchObject({ violations: 0 });
  });

  it("imports survey data, adjusts a network, creates a figure, and lists resulting figures", async () => {
    sendCommandMock.mockImplementation(async (commandName: string, args: Record<string, unknown>) => {
      switch (commandName) {
        case "importSurveyLandXml":
          return { imported: true, databaseName: args.databaseName };
        case "listSurveyObservations":
          return { count: 12, databaseName: args.databaseName, networkName: args.networkName };
        case "adjustSurveyNetwork":
          return { adjusted: true, method: args.method, networkName: args.networkName };
        case "createSurveyFigure":
          return { created: true, figureName: args.figureName };
        case "listSurveyFigures":
          return { figures: [{ name: "BNDY-1" }] };
        default:
          throw new Error(`Unexpected command: ${commandName}`);
      }
    });

    const result = await WORKFLOW_DOMAIN_DEFINITION.actions.survey_import_adjust_figures.execute({
      action: "survey_import_adjust_figures",
      filePath: "C:/survey/import.xml",
      databaseName: "Survey DB",
      networkName: "Traverse-1",
      method: "least_squares",
      applyAdjustment: true,
      figureName: "BNDY-1",
      pointNumbers: [1, 2, 3],
    });

    expect(result.status).toBe("completed");
    expect(result.steps).toHaveLength(5);
    expect(result.steps.map((step) => step.action)).toEqual([
      "survey.landxml_import",
      "survey.observation_list",
      "survey.network_adjust",
      "survey.figure_create",
      "survey.figure_list",
    ]);
    expect(result.outputs.adjustment).toMatchObject({ adjusted: true });
    expect(result.outputs.figures).toMatchObject({ figures: [{ name: "BNDY-1" }] });
  });
});
