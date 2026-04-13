using System.Linq;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class WorkflowCommands
{
  public static async Task<object?> CorridorQcReportWorkflowAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var outputPath = PluginRuntime.GetOptionalString(parameters, "outputPath");
    var includeAlignments = PluginRuntime.GetOptionalBool(parameters, "includeAlignments") ?? false;
    var includeProfiles = PluginRuntime.GetOptionalBool(parameters, "includeProfiles") ?? false;
    var includePipeNetworks = PluginRuntime.GetOptionalBool(parameters, "includePipeNetworks") ?? false;
    var includeSurfaces = PluginRuntime.GetOptionalBool(parameters, "includeSurfaces") ?? false;
    var includeLabels = PluginRuntime.GetOptionalBool(parameters, "includeLabels") ?? false;

    var corridorCheck = await RequireDictionary(
      QcCommands.QcCheckCorridorAsync(new JsonObject { ["name"] = corridorName }),
      "qcCheckCorridor");

    var steps = new List<Dictionary<string, object?>>
    {
      WorkflowStep("Run corridor QC check", "qc.check_corridor", "completed", corridorCheck),
    };
    var warnings = new List<string>();
    object? reportResult = null;

    if (!string.IsNullOrWhiteSpace(outputPath))
    {
      reportResult = await RequireDictionary(
        QcCommands.QcReportGenerateAsync(new JsonObject
        {
          ["outputPath"] = outputPath,
          ["includeAlignments"] = includeAlignments,
          ["includeProfiles"] = includeProfiles,
          ["includeCorridors"] = true,
          ["includePipeNetworks"] = includePipeNetworks,
          ["includeSurfaces"] = includeSurfaces,
          ["includeLabels"] = includeLabels,
        }),
        "qcReportGenerate");

      steps.Add(WorkflowStep("Generate consolidated QC report", "qc.generate_report", "completed", reportResult));
    }
    else
    {
      warnings.Add("No outputPath was provided, so consolidated QC report generation was skipped.");
      steps.Add(WorkflowStep("Generate consolidated QC report", "qc.generate_report", "skipped"));
    }

    return WorkflowResult(
      "corridor_qc_report",
      $"Completed corridor QC workflow for '{corridorName}'.",
      steps,
      new Dictionary<string, object?>
      {
        ["corridorCheck"] = corridorCheck,
        ["report"] = reportResult,
      },
      warnings);
  }

  public static async Task<object?> SurfaceComparisonReportWorkflowAsync(JsonObject? parameters)
  {
    var baseSurface = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurface = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var format = PluginRuntime.GetOptionalString(parameters, "format") ?? "summary";

    var baseSurfaceResult = await RequireDictionary(
      SurfaceCommands.GetSurfaceAsync(new JsonObject { ["name"] = baseSurface }),
      "getSurface.base");
    var comparisonSurfaceResult = await RequireDictionary(
      SurfaceCommands.GetSurfaceAsync(new JsonObject { ["name"] = comparisonSurface }),
      "getSurface.comparison");
    var volumeResult = await RequireDictionary(
      SurfaceCommands.ComputeSurfaceVolumeAsync(new JsonObject
      {
        ["baseSurface"] = baseSurface,
        ["comparisonSurface"] = comparisonSurface,
      }),
      "computeSurfaceVolume");
    var reportResult = await RequireDictionary(
      SurfaceCommands.GetSurfaceVolumeReportAsync(new JsonObject
      {
        ["baseSurface"] = baseSurface,
        ["comparisonSurface"] = comparisonSurface,
        ["format"] = format,
      }),
      "getSurfaceVolumeReport");

    var comparison = new Dictionary<string, object?>
    {
      ["baseSurface"] = baseSurfaceResult,
      ["comparisonSurface"] = comparisonSurfaceResult,
      ["volume"] = volumeResult,
    };

    return WorkflowResult(
      "surface_comparison_report",
      $"Completed surface comparison workflow for '{baseSurface}' vs '{comparisonSurface}'.",
      new List<Dictionary<string, object?>>
      {
        WorkflowStep("Run structured surface comparison", "surface.comparison_workflow", "completed", comparison),
        WorkflowStep("Generate surface volume report", "surface.volume_report", "completed", reportResult),
      },
      new Dictionary<string, object?>
      {
        ["comparison"] = comparison,
        ["report"] = reportResult,
      });
  }

  public static async Task<object?> ProjectStartupWorkflowAsync(JsonObject? parameters)
  {
    var templatePath = PluginRuntime.GetOptionalString(parameters, "templatePath");
    var saveAs = PluginRuntime.GetOptionalString(parameters, "saveAs");

    var healthResult = await RequireDictionary(DrawingCommands.GetCivil3DHealthAsync(), "getCivil3DHealth");
    var steps = new List<Dictionary<string, object?>>
    {
      WorkflowStep("Check Civil 3D health", "plugin.health", "completed", healthResult),
    };
    var warnings = new List<string>();
    object? newDrawingResult = null;

    if (!string.IsNullOrWhiteSpace(templatePath))
    {
      newDrawingResult = await RequireDictionary(
        DrawingCommands.NewDrawingAsync(new JsonObject { ["templatePath"] = templatePath }),
        "newDrawing");
      steps.Add(WorkflowStep("Create or open startup drawing from template", "drawing.new", "completed", newDrawingResult));
    }
    else
    {
      warnings.Add("No templatePath was provided, so startup drawing creation was skipped.");
      steps.Add(WorkflowStep("Create or open startup drawing from template", "drawing.new", "skipped"));
    }

    var drawingInfoResult = await RequireDictionary(DrawingCommands.GetDrawingInfoAsync(), "getDrawingInfo");
    var drawingSettingsResult = await RequireDictionary(DrawingCommands.GetDrawingSettingsAsync(), "getDrawingSettings");
    var objectTypesResult = await DrawingCommands.ListCivilObjectTypesAsync();

    steps.Add(WorkflowStep("Inspect drawing info", "drawing.info", "completed", drawingInfoResult));
    steps.Add(WorkflowStep("Inspect drawing settings", "drawing.settings", "completed", drawingSettingsResult));
    steps.Add(WorkflowStep("List Civil 3D object types", "drawing.list_object_types", "completed", objectTypesResult));

    warnings.Add("Project data-shortcut listing is not yet implemented in the native plugin workflow layer, so that step was skipped.");
    steps.Add(WorkflowStep("List project data shortcuts", "project.data_shortcut_list", "skipped"));

    object? saveResult = null;
    if (!string.IsNullOrWhiteSpace(saveAs))
    {
      saveResult = await RequireDictionary(
        DrawingCommands.SaveDrawingAsync(new JsonObject { ["saveAs"] = saveAs }),
        "saveDrawing");
      steps.Add(WorkflowStep("Save startup drawing", "drawing.save", "completed", saveResult));
    }
    else
    {
      steps.Add(WorkflowStep("Save startup drawing", "drawing.save", "skipped"));
    }

    return WorkflowResult(
      "project_startup",
      "Completed project startup and drawing readiness workflow.",
      steps,
      new Dictionary<string, object?>
      {
        ["health"] = healthResult,
        ["newDrawing"] = newDrawingResult,
        ["drawingInfo"] = drawingInfoResult,
        ["drawingSettings"] = drawingSettingsResult,
        ["objectTypes"] = objectTypesResult,
        ["dataShortcuts"] = null,
        ["save"] = saveResult,
      },
      warnings);
  }

  public static async Task<object?> DrawingReadinessAuditWorkflowAsync(JsonObject? parameters)
  {
    var layerPrefix = PluginRuntime.GetOptionalString(parameters, "layerPrefix");
    var checkLineweights = PluginRuntime.GetOptionalBool(parameters, "checkLineweights");
    var checkColors = PluginRuntime.GetOptionalBool(parameters, "checkColors");
    var limit = PluginRuntime.GetOptionalInt(parameters, "limit");

    var healthResult = await RequireDictionary(DrawingCommands.GetCivil3DHealthAsync(), "getCivil3DHealth");
    var drawingInfoResult = await RequireDictionary(DrawingCommands.GetDrawingInfoAsync(), "getDrawingInfo");
    var drawingSettingsResult = await RequireDictionary(DrawingCommands.GetDrawingSettingsAsync(), "getDrawingSettings");
    var objectTypesResult = await DrawingCommands.ListCivilObjectTypesAsync();
    var selectedObjectsResult = await DrawingCommands.GetSelectedCivilObjectsInfoAsync(new JsonObject { ["limit"] = limit });
    var standardsAuditResult = await RequireDictionary(
      QcCommands.QcCheckDrawingStandardsAsync(new JsonObject
      {
        ["layerPrefix"] = layerPrefix,
        ["checkLineweights"] = checkLineweights,
        ["checkColors"] = checkColors,
      }),
      "qcCheckDrawingStandards");

    return WorkflowResult(
      "drawing_readiness_audit",
      "Completed drawing readiness audit workflow.",
      new List<Dictionary<string, object?>>
      {
        WorkflowStep("Check Civil 3D health", "plugin.health", "completed", healthResult),
        WorkflowStep("Inspect drawing info", "drawing.info", "completed", drawingInfoResult),
        WorkflowStep("Inspect drawing settings", "drawing.settings", "completed", drawingSettingsResult),
        WorkflowStep("List Civil 3D object types", "drawing.list_object_types", "completed", objectTypesResult),
        WorkflowStep("Inspect selected Civil 3D objects", "drawing.selected_objects_info", "completed", selectedObjectsResult),
        WorkflowStep("Audit drawing standards", "standards.check_drawing_standards", "completed", standardsAuditResult),
      },
      new Dictionary<string, object?>
      {
        ["health"] = healthResult,
        ["drawingInfo"] = drawingInfoResult,
        ["drawingSettings"] = drawingSettingsResult,
        ["objectTypes"] = objectTypesResult,
        ["selectedObjects"] = selectedObjectsResult,
        ["standardsAudit"] = standardsAuditResult,
      });
  }

  public static async Task<object?> FeatureLineToGradingWorkflowAsync(JsonObject? parameters)
  {
    var featureLineName = PluginRuntime.GetRequiredString(parameters, "featureLineName");
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");
    var groupDescription = PluginRuntime.GetOptionalString(parameters, "groupDescription");
    var createGroup = PluginRuntime.GetOptionalBool(parameters, "createGroup") ?? false;
    var useProjection = PluginRuntime.GetOptionalBool(parameters, "useProjection");
    var criteriaName = PluginRuntime.GetOptionalString(parameters, "criteriaName");
    var side = PluginRuntime.GetOptionalString(parameters, "side");
    var surfaceName = PluginRuntime.GetOptionalString(parameters, "surfaceName");

    var featureLine = await RequireDictionary(
      GradingCommands.GetFeatureLineAsync(new JsonObject { ["name"] = featureLineName }),
      "getFeatureLine");

    var steps = new List<Dictionary<string, object?>>
    {
      WorkflowStep("Inspect source feature line", "grading.feature_line_get", "completed", featureLine),
    };
    var warnings = new List<string>();
    object? groupResult = null;

    if (createGroup)
    {
      groupResult = await RequireDictionary(
        GradingCommands.CreateGradingGroupAsync(new JsonObject
        {
          ["name"] = groupName,
          ["description"] = groupDescription,
          ["useProjection"] = useProjection,
        }),
        "createGradingGroup");
      steps.Add(WorkflowStep("Create grading group", "grading.group_create", "completed", groupResult));
    }
    else
    {
      steps.Add(WorkflowStep("Create grading group", "grading.group_create", "skipped"));
    }

    var gradingResult = await RequireDictionary(
      GradingCommands.CreateGradingAsync(new JsonObject
      {
        ["groupName"] = groupName,
        ["featureLineName"] = featureLineName,
        ["criteriaName"] = criteriaName,
        ["side"] = side,
      }),
      "createGrading");
    steps.Add(WorkflowStep("Create grading from feature line", "grading.create", "completed", gradingResult));

    object? surfaceResult = null;
    if (!string.IsNullOrWhiteSpace(surfaceName))
    {
      surfaceResult = await RequireDictionary(
        GradingCommands.CreateSurfaceFromGradingGroupAsync(new JsonObject
        {
          ["name"] = groupName,
          ["surfaceName"] = surfaceName,
        }),
        "createSurfaceFromGradingGroup");
      steps.Add(WorkflowStep("Create grading surface", "grading.group_surface_create", "completed", surfaceResult));
    }
    else
    {
      warnings.Add("No surfaceName was provided, so grading-surface creation was skipped.");
      steps.Add(WorkflowStep("Create grading surface", "grading.group_surface_create", "skipped"));
    }

    return WorkflowResult(
      "feature_line_to_grading",
      $"Converted feature line '{featureLineName}' into grading in group '{groupName}'.",
      steps,
      new Dictionary<string, object?>
      {
        ["featureLine"] = featureLine,
        ["group"] = groupResult,
        ["grading"] = gradingResult,
        ["surface"] = surfaceResult,
      },
      warnings);
  }

  public static async Task<object?> PlanProductionPublishWorkflowAsync(JsonObject? parameters)
  {
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var sheetSetName = PluginRuntime.GetOptionalString(parameters, "sheetSetName");
    var plotStyleTable = PluginRuntime.GetOptionalString(parameters, "plotStyleTable");
    var paperSize = PluginRuntime.GetOptionalString(parameters, "paperSize");
    var layoutNames = ParseStringArray(parameters, "layoutNames");

    object? publishResult;
    List<Dictionary<string, object?>> steps;
    string summaryTarget;

    if (!string.IsNullOrWhiteSpace(sheetSetName))
    {
      publishResult = await RequireDictionary(
        PlanProductionCommands.ExportSheetSetAsync(new JsonObject
        {
          ["sheetSetName"] = sheetSetName,
          ["outputPath"] = outputPath,
          ["plotStyleTable"] = plotStyleTable,
        }),
        "exportSheetSet");

      steps = new List<Dictionary<string, object?>>
      {
        WorkflowStep("Export sheet set", "plan_production.sheet_set_export", "completed", publishResult),
      };
      summaryTarget = $"sheet set '{sheetSetName}'";
    }
    else
    {
      publishResult = await RequireDictionary(
        PlanProductionCommands.PublishSheetPdfAsync(new JsonObject
        {
          ["layoutNames"] = JsonSerializerHelper.StringArray(layoutNames),
          ["outputPath"] = outputPath,
          ["plotStyleTable"] = plotStyleTable,
          ["paperSize"] = paperSize,
        }),
        "publishSheetPdf");

      steps = new List<Dictionary<string, object?>>
      {
        WorkflowStep("Publish layouts to PDF", "plan_production.sheet_publish_pdf", "completed", publishResult),
      };
      summaryTarget = $"layouts {string.Join(", ", layoutNames)}";
    }

    return WorkflowResult(
      "plan_production_publish",
      $"Completed plan-production publish workflow for {summaryTarget}.",
      steps,
      new Dictionary<string, object?> { ["publish"] = publishResult });
  }

  public static async Task<object?> QcFixAndVerifyWorkflowAsync(JsonObject? parameters)
  {
    var layerPrefix = PluginRuntime.GetOptionalString(parameters, "layerPrefix");
    var checkLineweights = PluginRuntime.GetOptionalBool(parameters, "checkLineweights");
    var checkColors = PluginRuntime.GetOptionalBool(parameters, "checkColors");
    var fixSpaces = PluginRuntime.GetOptionalBool(parameters, "fixSpaces");
    var maxNameLength = PluginRuntime.GetOptionalInt(parameters, "maxNameLength");
    var colorIndex = PluginRuntime.GetOptionalInt(parameters, "colorIndex");
    var lineweight = PluginRuntime.GetOptionalInt(parameters, "lineweight");
    var dryRun = PluginRuntime.GetOptionalBool(parameters, "dryRun");

    var initialCheck = await RequireDictionary(
      QcCommands.QcCheckDrawingStandardsAsync(new JsonObject
      {
        ["layerPrefix"] = layerPrefix,
        ["checkLineweights"] = checkLineweights,
        ["checkColors"] = checkColors,
      }),
      "qcCheckDrawingStandards.initial");
    var fixResult = await RequireDictionary(
      QcCommands.QcFixDrawingStandardsAsync(new JsonObject
      {
        ["layerPrefix"] = layerPrefix,
        ["fixSpaces"] = fixSpaces,
        ["maxNameLength"] = maxNameLength,
        ["colorIndex"] = colorIndex,
        ["lineweight"] = lineweight,
        ["dryRun"] = dryRun,
      }),
      "qcFixDrawingStandards");
    var verificationCheck = await RequireDictionary(
      QcCommands.QcCheckDrawingStandardsAsync(new JsonObject
      {
        ["layerPrefix"] = layerPrefix,
        ["checkLineweights"] = checkLineweights,
        ["checkColors"] = checkColors,
      }),
      "qcCheckDrawingStandards.verification");

    return WorkflowResult(
      "qc_fix_and_verify",
      "Completed drawing-standards fix-and-verify workflow.",
      new List<Dictionary<string, object?>>
      {
        WorkflowStep("Run baseline standards audit", "standards.check_drawing_standards", "completed", initialCheck),
        WorkflowStep("Apply drawing standards fixes", "standards.fix_drawing_standards", "completed", fixResult),
        WorkflowStep("Re-run standards audit", "standards.check_drawing_standards", "completed", verificationCheck),
      },
      new Dictionary<string, object?>
      {
        ["initialCheck"] = initialCheck,
        ["fixes"] = fixResult,
        ["verificationCheck"] = verificationCheck,
      });
  }

  private static async Task<Dictionary<string, object?>> RequireDictionary(Task<object?> task, string context)
  {
    var value = await task;
    if (value is Dictionary<string, object?> dictionary)
    {
      return dictionary;
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Expected '{context}' to return an object result.");
  }

  private static Dictionary<string, object?> WorkflowResult(
    string workflow,
    string summary,
    List<Dictionary<string, object?>> steps,
    Dictionary<string, object?> outputs,
    List<string>? warnings = null)
  {
    warnings ??= new List<string>();
    return new Dictionary<string, object?>
    {
      ["workflow"] = workflow,
      ["status"] = warnings.Count > 0 ? "completed_with_warnings" : "completed",
      ["summary"] = summary,
      ["steps"] = steps,
      ["outputs"] = outputs,
      ["warnings"] = warnings,
    };
  }

  private static Dictionary<string, object?> WorkflowStep(string name, string action, string status, object? result = null)
  {
    var step = new Dictionary<string, object?>
    {
      ["name"] = name,
      ["action"] = action,
      ["status"] = status,
    };

    if (result != null)
    {
      step["result"] = result;
    }

    return step;
  }

  private static List<string> ParseStringArray(JsonObject? parameters, string name)
  {
    if (PluginRuntime.GetParameter(parameters, name) is not JsonArray array)
    {
      return new List<string>();
    }

    return array
      .Select(item => item?.GetValue<string>())
      .Where(value => !string.IsNullOrWhiteSpace(value))
      .Cast<string>()
      .ToList();
  }

  private static class JsonSerializerHelper
  {
    public static JsonArray StringArray(IEnumerable<string> values)
    {
      var array = new JsonArray();
      foreach (var value in values)
      {
        array.Add(value);
      }
      return array;
    }
  }
}
