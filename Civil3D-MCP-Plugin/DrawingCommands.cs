using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.DatabaseServices;
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

    object response = new Dictionary<string, object?>
    {
      ["connected"] = true,
      ["civil3dVersion"] = App.GetSystemVariable("ACADVER")?.ToString(),
      ["pluginVersion"] = typeof(PluginEntry).Assembly.GetName().Version?.ToString(),
      ["drawingLoaded"] = doc != null,
      ["operationInProgress"] = status.OperationInProgress,
      ["currentOperation"] = status.CurrentOperation,
      ["queueDepth"] = status.QueueDepth,
      ["memoryUsageMb"] = Math.Round(process.PrivateMemorySize64 / 1024d / 1024d, 2),
    };

    return Task.FromResult<object?>(response);
  }

  public static Task<object?> GetDrawingInfoAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var layer = CivilObjectUtils.GetRequiredObject<LayerTableRecord>(transaction, database.Clayer, OpenMode.ForRead);
      var unsavedChanges = Convert.ToInt32(App.GetSystemVariable("DBMOD") ?? 0) != 0;
      var angularUnits = CivilObjectUtils.AngularUnits(Convert.ToInt16(App.GetSystemVariable("AUNITS") ?? 0));

      return new Dictionary<string, object?>
      {
        ["fileName"] = Path.GetFileName(database.Filename),
        ["filePath"] = database.Filename,
        ["coordinateSystem"] = null,
        ["linearUnits"] = CivilObjectUtils.LinearUnits(database),
        ["angularUnits"] = angularUnits,
        ["unsavedChanges"] = unsavedChanges,
        ["objectCounts"] = new Dictionary<string, object?>
        {
          ["surfaces"] = civilDoc.GetSurfaceIds().Count,
          ["alignments"] = civilDoc.GetAlignmentIds().Count,
          ["profiles"] = CountProfiles(civilDoc, transaction),
          ["corridors"] = civilDoc.CorridorCollection.Count,
          ["pipeNetworks"] = 0,
          ["points"] = civilDoc.CogoPoints.Count,
          ["parcels"] = 0,
        },
        ["drawingName"] = doc.Name,
        ["projectName"] = null,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> GetDrawingSettingsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var currentLayer = CivilObjectUtils.GetRequiredObject<LayerTableRecord>(transaction, database.Clayer, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["coordinateSystem"] = null,
        ["coordinateZone"] = null,
        ["datum"] = null,
        ["scaleFactor"] = Convert.ToDouble(App.GetSystemVariable("DIMSCALE") ?? 1d),
        ["elevationReference"] = null,
        ["defaultLayer"] = currentLayer.Name,
        ["defaultStyles"] = new Dictionary<string, object?>
        {
          ["surface"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.SurfaceStyles, transaction),
          ["alignment"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.AlignmentStyles, transaction),
          ["profile"] = LookupUtils.GetFirstStyleName(civilDoc.Styles.ProfileStyles, transaction),
          ["corridor"] = null,
          ["pipeNetwork"] = null,
        },
      };
    });
  }

  public static Task<object?> SaveDrawingAsync(JsonObject? parameters)
  {
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var saveAs = PluginRuntime.GetOptionalString(parameters, "saveAs");
      var targetPath = string.IsNullOrWhiteSpace(saveAs) ? database.Filename : saveAs;
      if (string.IsNullOrWhiteSpace(targetPath))
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "saveDrawing requires 'saveAs' when the drawing has not been saved yet.");
      }

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

    Document? createdDocument = null;
    Exception? capturedException = null;

    await App.DocumentManager.ExecuteInCommandContextAsync(async _ =>
    {
      try
      {
        createdDocument = string.IsNullOrWhiteSpace(templatePath)
          ? App.DocumentManager.Add(string.Empty)
          : (File.Exists(templatePath)
            ? App.DocumentManager.Add(templatePath)
            : throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Template file does not exist: {templatePath}"));

        if (createdDocument != null)
        {
          App.DocumentManager.MdiActiveDocument = createdDocument;
        }
      }
      catch (Exception ex)
      {
        capturedException = ex;
      }

      await Task.CompletedTask;
    }, null);

    if (capturedException != null)
    {
      throw capturedException;
    }

    var activeDoc = App.DocumentManager.MdiActiveDocument;
    if (activeDoc == null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Failed to create a new drawing.");
    }

    return new Dictionary<string, object?>
    {
      ["drawingName"] = activeDoc.Name,
      ["filePath"] = activeDoc.Database.Filename,
      ["templatePath"] = templatePath,
    };
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
    object[] types =
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

    return Task.FromResult<object?>(types);
  }

  public static Task<object?> GetSelectedCivilObjectsInfoAsync(JsonObject? parameters)
  {
    var limit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 100;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
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
    });
  }

  private static int CountProfiles(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction)
  {
    var count = 0;
    foreach (ObjectId objectId in civilDoc.GetAlignmentIds())
    {
      var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, objectId, OpenMode.ForRead);
      count += alignment.GetProfileIds().Count;
    }

    return count;
  }
}
