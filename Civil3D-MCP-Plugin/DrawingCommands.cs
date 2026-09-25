using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.Settings;
using System.Text.Json.Nodes;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

public static class DrawingCommands
{
  public static Task<object?> GetCivil3DHealthAsync()
  {
    var status = PluginRuntime.GetStatus();
    var doc = App.DocumentManager.MdiActiveDocument;
    var process = System.Diagnostics.Process.GetCurrentProcess();
    var jobs = JobRegistry.GetStats();

    object response = new Dictionary<string, object?>
    {
      ["connected"] = true,
      ["civil3dVersion"] = App.GetSystemVariable("ACADVER")?.ToString(),
      ["pluginVersion"] = typeof(PluginEntry).Assembly.GetName().Version?.ToString(),
      ["drawingLoaded"] = doc != null,
      ["operationInProgress"] = status.OperationInProgress,
      ["currentOperation"] = status.CurrentOperation,
      ["queueDepth"] = status.QueueDepth,
      ["queueCapacity"] = status.QueueCapacity,
      ["currentOperationStartedAtUnixMs"] = status.CurrentOperationStartedAtUnixMs,
      ["currentRequestId"] = status.CurrentRequestId,
      ["currentOperationDurationMs"] = status.CurrentOperationStartedAtUnixMs is long startedAt
        ? Math.Max(0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt)
        : null,
      ["memoryUsageMb"] = Math.Round(process.PrivateMemorySize64 / 1024d / 1024d, 2),
      ["logFilePath"] = PluginLog.LogFilePath,
      ["fileLoggingHealthy"] = PluginLog.IsFileLoggingHealthy,
      ["fileLoggingError"] = PluginLog.LastFileError,
      ["jobs"] = new Dictionary<string, object?>
      {
        ["total"] = jobs.Total,
        ["running"] = jobs.Running,
        ["completed"] = jobs.Completed,
        ["failed"] = jobs.Failed,
        ["cancelled"] = jobs.Cancelled,
        ["capacity"] = jobs.Capacity,
        ["terminalRetentionMinutes"] = jobs.TerminalRetentionMinutes,
      },
    };

    return Task.FromResult<object?>(response);
  }

  public static Task<object?> GetDrawingInfoAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
      BuildDrawingInfo(doc, civilDoc, database, transaction));
  }

  public static Task<object?> GetProjectContextAsync(JsonObject? parameters)
  {
    var requestedLimit = PluginRuntime.GetOptionalInt(parameters, "selectedObjectLimit") ?? 25;
    var selectedObjectLimit = Math.Clamp(requestedLimit, 1, 100);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
      new Dictionary<string, object?>
      {
        ["drawingInfo"] = BuildDrawingInfo(doc, civilDoc, database, transaction),
        ["objectTypes"] = BuildCivilObjectTypes(),
        ["selectedObjects"] = BuildSelectedCivilObjects(doc, transaction, selectedObjectLimit),
      });
  }

  public static Task<object?> GetDrawingSettingsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var currentLayer = CivilObjectUtils.GetRequiredObject<LayerTableRecord>(transaction, database.Clayer, OpenMode.ForRead);
      var coords = ReadCoordinateSystemFields(civilDoc);

      return new Dictionary<string, object?>
      {
        ["coordinateSystem"] = coords.code,
        ["coordinateZone"] = coords.zone,
        ["datum"] = coords.datum,
        ["scaleFactor"] = Convert.ToDouble(App.GetSystemVariable("DIMSCALE") ?? 1d),
        ["elevationReference"] = coords.verticalDatum,
        ["defaultLayer"] = currentLayer.Name,
        ["defaultStyles"] = new Dictionary<string, object?>
        {
          ["surface"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.SurfaceStyles, transaction),
          ["alignment"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.AlignmentStyles, transaction),
          ["profile"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.ProfileStyles, transaction),
          ["corridor"] = null,
          ["pipeNetwork"] = PipeNetworkCommands.GetFirstPipeNetworkStyleName(civilDoc, transaction),
        },
      };
    });
  }

  public static Task<object?> SaveDrawingAsync(JsonObject? parameters)
  {
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var saveAs = PluginRuntime.GetOptionalString(parameters, "saveAs");
      var targetPath = string.IsNullOrWhiteSpace(saveAs) ? database.Filename : saveAs;
      if (string.IsNullOrWhiteSpace(targetPath))
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "saveDrawing requires 'saveAs' when the drawing has not been saved yet.");
      }

      // Saving the active drawing is not an export conflict: its current file
      // necessarily exists. Boundary/overwrite checks apply only to Save As.
      targetPath = string.IsNullOrWhiteSpace(saveAs)
        ? Path.GetFullPath(targetPath)
        : FileBoundary.ResolveExportPath(targetPath, overwrite, ".dwg");
      database.SaveAs(targetPath, true, DwgVersion.Current, database.SecurityParameters);

      return new Dictionary<string, object?>
      {
        ["saved"] = true,
        ["filePath"] = targetPath,
      };
    });
  }

  public static async Task<object?> NewDrawingAsync(JsonObject? parameters)
  {
    var templatePath = PluginRuntime.GetOptionalString(parameters, "templatePath");
    if (!string.IsNullOrWhiteSpace(templatePath))
    {
      templatePath = FileBoundary.ResolveImportPath(templatePath, ".dwt", ".dwg");
    }

    return await CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var createdDocument = string.IsNullOrWhiteSpace(templatePath)
        ? App.DocumentManager.Add(string.Empty)
        : App.DocumentManager.Add(templatePath);

      App.DocumentManager.MdiActiveDocument = createdDocument;
      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["drawingName"] = createdDocument.Name,
        ["filePath"] = createdDocument.Database.Filename,
        ["templatePath"] = templatePath,
      });
    });
  }

  public static Task<object?> ListOpenDocumentsAsync()
  {
    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var activeDocument = App.DocumentManager.MdiActiveDocument;
      var documents = new List<Dictionary<string, object?>>();

      foreach (Document document in App.DocumentManager)
      {
        documents.Add(new Dictionary<string, object?>
        {
          ["name"] = document.Name,
          ["filePath"] = document.Database?.Filename,
          ["isActive"] = ReferenceEquals(document, activeDocument),
        });
      }

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["documents"] = documents,
      });
    });
  }

  public static Task<object?> SetActiveDocumentAsync(JsonObject? parameters)
  {
    var match = PluginRuntime.GetRequiredString(parameters, "match");

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      Document? target = null;
      foreach (Document document in App.DocumentManager)
      {
        var name = document.Name ?? string.Empty;
        var filePath = document.Database?.Filename ?? string.Empty;
        if (name.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0 ||
            filePath.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0)
        {
          target = document;
          break;
        }
      }

      if (target == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No open document matches '{match}'. Use listOpenDocuments to see what's currently open.");
      }

      App.DocumentManager.MdiActiveDocument = target;

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["name"] = target.Name,
        ["filePath"] = target.Database?.Filename,
        ["activated"] = true,
      });
    });
  }

  public static Task<object?> UndoDrawingAsync(JsonObject? parameters)
  {
    var steps = PluginRuntime.GetOptionalInt(parameters, "steps") ?? 1;
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      doc.SendStringToExecute($"_.UNDO {steps} ", true, false, false);
      return new Dictionary<string, object?>
      {
        ["steps"] = steps,
        ["action"] = "undo",
      };
    });
  }

  public static Task<object?> RedoDrawingAsync(JsonObject? parameters)
  {
    var steps = PluginRuntime.GetOptionalInt(parameters, "steps") ?? 1;
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      doc.SendStringToExecute($"_.REDO {steps} ", true, false, false);
      return new Dictionary<string, object?>
      {
        ["steps"] = steps,
        ["action"] = "redo",
      };
    });
  }

  public static Task<object?> ListCivilObjectTypesAsync()
  {
    return Task.FromResult<object?>(BuildCivilObjectTypes());
  }

  public static Task<object?> GetSelectedCivilObjectsInfoAsync(JsonObject? parameters)
  {
    var limit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 100;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
      BuildSelectedCivilObjects(doc, transaction, limit));
  }

  private static Dictionary<string, object?> BuildDrawingInfo(
    Document doc,
    CivilDocument civilDoc,
    Database database,
    Transaction transaction)
  {
    var unsavedChanges = Convert.ToInt32(App.GetSystemVariable("DBMOD") ?? 0) != 0;
    var angularUnits = CivilObjectUtils.AngularUnits(Convert.ToInt16(App.GetSystemVariable("AUNITS") ?? 0));
    var coordinateSystemCode = ReadCoordinateSystemCode(civilDoc);

    return new Dictionary<string, object?>
    {
      ["fileName"] = Path.GetFileName(database.Filename),
      ["filePath"] = database.Filename,
      ["coordinateSystem"] = coordinateSystemCode,
      ["linearUnits"] = CivilObjectUtils.LinearUnits(database),
      ["angularUnits"] = angularUnits,
      ["unsavedChanges"] = unsavedChanges,
      ["objectCounts"] = new Dictionary<string, object?>
      {
        ["surfaces"] = civilDoc.GetSurfaceIds().Count,
        ["alignments"] = civilDoc.GetAlignmentIds().Count,
        ["profiles"] = CountProfiles(civilDoc, transaction),
        ["corridors"] = civilDoc.CorridorCollection.Count,
        ["pipeNetworks"] = PipeNetworkCommands.CountPipeNetworks(civilDoc),
        ["points"] = civilDoc.CogoPoints.Count,
        ["parcels"] = CountParcels(civilDoc, transaction),
      },
      ["drawingName"] = doc.Name,
      ["projectName"] = null,
      ["units"] = CivilObjectUtils.LinearUnits(database),
    };
  }

  private static object[] BuildCivilObjectTypes() =>
  [
    "Alignment",
    "Surface",
    "Profile",
    "Corridor",
    "CogoPoint",
    "SampleLineGroup",
    "FeatureLine",
    "Parcel",
    "PipeNetwork",
  ];

  private static List<Dictionary<string, object?>> BuildSelectedCivilObjects(
    Document doc,
    Transaction transaction,
    int limit)
  {
    var result = new List<Dictionary<string, object?>>();
    var selection = doc.Editor.SelectImplied();
    if (selection.Status != PromptStatus.OK || selection.Value == null)
    {
      return result;
    }

    foreach (var objectId in selection.Value.GetObjectIds().Take(limit))
    {
      var dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
      if (dbObject == null)
      {
        continue;
      }

      result.Add(new Dictionary<string, object?>
      {
        ["handle"] = dbObject.Handle.ToString(),
        ["objectType"] = dbObject.GetType().Name,
        ["name"] = CivilObjectUtils.GetName(dbObject),
        ["description"] = CivilObjectUtils.GetStringProperty(dbObject, "Description"),
      });
    }

    return result;
  }

  private static int CountProfiles(CivilDocument civilDoc, Transaction transaction)
  {
    var count = 0;
    foreach (ObjectId objectId in civilDoc.GetAlignmentIds())
    {
      var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, objectId, OpenMode.ForRead);
      count += alignment.GetProfileIds().Count;
    }

    return count;
  }

  private static int CountParcels(CivilDocument civilDoc, Transaction transaction)
  {
    var count = 0;

    foreach (ObjectId siteId in civilDoc.GetSiteIds())
    {
      var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, siteId, OpenMode.ForRead);
      count += site.GetParcelIds().Count;
    }

    return count;
  }

  // Single-field coordinate system code (for getDrawingInfo).
  private static string? ReadCoordinateSystemCode(CivilDocument civilDoc)
  {
    return ReadCoordinateSystemFields(civilDoc).code;
  }

  private static (string? code, string? zone, string? datum, string? verticalDatum) ReadCoordinateSystemFields(CivilDocument civilDoc)
  {
    var unitZone = civilDoc.Settings.DrawingSettings.UnitZoneSettings;
    var code = unitZone.CoordinateSystemCode;
    if (string.IsNullOrWhiteSpace(code))
    {
      return (null, null, null, null);
    }

    var coordinateSystem = SettingsUnitZone.GetCoordinateSystemByCode(code);
    return (code, coordinateSystem.Category, coordinateSystem.Datum, null);
  }
}
