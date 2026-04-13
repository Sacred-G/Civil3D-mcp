using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

public static class DataShortcutCommands
{
  public static Task<object?> ListDataShortcutsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var incoming = new List<Dictionary<string, object?>>();
      var exportable = new List<Dictionary<string, object?>>();

      foreach (ObjectId objectId in civilDoc.GetSurfaceIds())
      {
        var surface = CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, objectId, OpenMode.ForRead);
        RegisterShortcutCandidate(surface, "surface", incoming, exportable);
      }

      foreach (ObjectId objectId in civilDoc.GetAlignmentIds())
      {
        var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, objectId, OpenMode.ForRead);
        RegisterShortcutCandidate(alignment, "alignment", incoming, exportable);

        foreach (ObjectId profileId in alignment.GetProfileIds())
        {
          var profile = CivilObjectUtils.GetRequiredObject<Profile>(transaction, profileId, OpenMode.ForRead);
          RegisterShortcutCandidate(profile, "profile", incoming, exportable);
        }
      }

      foreach (var objectId in GetPipeNetworkIds(civilDoc))
      {
        var network = CivilObjectUtils.GetRequiredObject<AcDbObject>(transaction, objectId, OpenMode.ForRead);
        RegisterShortcutCandidate(network, "pipe_network", incoming, exportable);
      }

      foreach (var objectId in GetPressureNetworkIds(civilDoc))
      {
        var network = CivilObjectUtils.GetRequiredObject<AcDbObject>(transaction, objectId, OpenMode.ForRead);
        RegisterShortcutCandidate(network, "pressure_network", incoming, exportable);
      }

      foreach (ObjectId objectId in civilDoc.CorridorCollection)
      {
        var corridor = CivilObjectUtils.GetRequiredObject<Corridor>(transaction, objectId, OpenMode.ForRead);
        RegisterShortcutCandidate(corridor, "corridor", incoming, exportable);
      }

      return new Dictionary<string, object?>
      {
        ["incoming"] = incoming,
        ["exportable"] = exportable,
      };
    });
  }

  public static Task<object?> CreateDataShortcutAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var projectFolder = PluginRuntime.GetOptionalString(parameters, "projectFolder");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      SendCommand(doc, "CreateDataShortcuts");

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["objectName"] = objectName,
        ["description"] = description,
        ["projectFolder"] = projectFolder,
        ["status"] = "initiated",
        ["dialogRequired"] = true,
        ["command"] = "CreateDataShortcuts",
        ["notes"] = new List<string>
        {
          "The Create Data Shortcuts dialog was opened in Civil 3D.",
          "Civil 3D still requires user confirmation in the dialog to choose which eligible objects will be published.",
          !string.IsNullOrWhiteSpace(projectFolder)
            ? $"Ensure the active Data Shortcuts project folder is set to '{projectFolder}' before completing the dialog."
            : "Ensure the active Data Shortcuts project folder is set correctly before completing the dialog.",
        },
      };
    });
  }

  public static Task<object?> ReferenceDataShortcutAsync(JsonObject? parameters)
  {
    var projectFolder = PluginRuntime.GetRequiredString(parameters, "projectFolder");
    var shortcutName = PluginRuntime.GetRequiredString(parameters, "shortcutName");
    var shortcutType = PluginRuntime.GetRequiredString(parameters, "shortcutType");
    var layer = PluginRuntime.GetOptionalString(parameters, "layer");

    var commandName = shortcutType.ToLowerInvariant() switch
    {
      "surface" => "CreateSurfaceReference",
      "alignment" => "CreateAlignmentReference",
      "profile" => "CreateProfileReference",
      "pipe_network" => "CreateNetworkReference",
      "corridor" => "CreateCorridorReference",
      _ => null,
    };

    if (commandName == null)
    {
      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["projectFolder"] = projectFolder,
        ["shortcutName"] = shortcutName,
        ["shortcutType"] = shortcutType,
        ["layer"] = layer,
        ["status"] = "manual_step_required",
        ["dialogRequired"] = true,
        ["notes"] = new List<string>
        {
          $"Automatic command launch is not yet mapped for shortcut type '{shortcutType}'.",
          "Use the Data Shortcuts node in Toolspace or the Data Shortcut Manager dialog to create the reference manually.",
        },
      });
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      SendCommand(doc, commandName);

      return new Dictionary<string, object?>
      {
        ["projectFolder"] = projectFolder,
        ["shortcutName"] = shortcutName,
        ["shortcutType"] = shortcutType,
        ["layer"] = layer,
        ["status"] = "initiated",
        ["dialogRequired"] = true,
        ["command"] = commandName,
        ["notes"] = new List<string>
        {
          $"The {commandName} command was launched in Civil 3D.",
          $"Civil 3D still requires the user to pick the '{shortcutName}' shortcut from the active project.",
          $"Ensure the active Data Shortcuts project folder matches '{projectFolder}' before completing the dialog.",
        },
      };
    });
  }

  public static async Task<object?> SyncDataShortcutsAsync(JsonObject? parameters)
  {
    var projectFolder = PluginRuntime.GetOptionalString(parameters, "projectFolder");
    var shortcutNames = ParseStringArray(parameters, "shortcutNames");
    var dryRun = PluginRuntime.GetOptionalBool(parameters, "dryRun") ?? false;

    var inventory = await RequireDictionary(ListDataShortcutsAsync(), "listDataShortcuts");
    var incoming = inventory["incoming"] as List<Dictionary<string, object?>> ?? new List<Dictionary<string, object?>>();
    var staleReferences = incoming
      .Where(item => !(item["isSynced"] as bool? ?? false))
      .Where(item => shortcutNames.Count == 0 || shortcutNames.Contains(item["objectName"]?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
      .ToList();

    if (dryRun)
    {
      return new Dictionary<string, object?>
      {
        ["projectFolder"] = projectFolder,
        ["shortcutNames"] = shortcutNames,
        ["dryRun"] = true,
        ["staleReferenceCount"] = staleReferences.Count,
        ["wouldSynchronize"] = staleReferences,
      };
    }

    return await CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      SendCommand(doc, "SynchronizeReferences");

      var notes = new List<string>
      {
        "The SynchronizeReferences command was launched in Civil 3D.",
      };

      if (!string.IsNullOrWhiteSpace(projectFolder))
      {
        notes.Add($"The requested project folder was '{projectFolder}'. Civil 3D sync scope still depends on the active Data Shortcuts project.");
      }

      if (shortcutNames.Count > 0)
      {
        notes.Add("Civil 3D's manual synchronize command applies to the active drawing's references; shortcutNames were treated as a filter for reporting, not as a guaranteed command-line scope.");
      }

      return new Dictionary<string, object?>
      {
        ["projectFolder"] = projectFolder,
        ["shortcutNames"] = shortcutNames,
        ["dryRun"] = false,
        ["staleReferenceCount"] = staleReferences.Count,
        ["targetReferences"] = staleReferences,
        ["status"] = "initiated",
        ["dialogRequired"] = false,
        ["command"] = "SynchronizeReferences",
        ["notes"] = notes,
      };
    });
  }

  public static Task<object?> CreateDataShortcutReferenceAsync(JsonObject? parameters)
  {
    var sourceFilePath = PluginRuntime.GetRequiredString(parameters, "sourceFilePath");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["sourceFilePath"] = sourceFilePath,
      ["objectName"] = objectName,
      ["objectType"] = objectType,
      ["status"] = "manual_step_required",
      ["dialogRequired"] = true,
      ["notes"] = new List<string>
      {
        "Direct source-file-based data shortcut reference creation is not fully automatable in the current native plugin.",
        "Use the active project Data Shortcuts tree or the object-specific Create Reference command after confirming the correct project folder.",
      },
    });
  }

  public static Task<object?> PromoteDataShortcutAsync(JsonObject? parameters)
  {
    var shortcutName = PluginRuntime.GetRequiredString(parameters, "shortcutName");
    var shortcutType = PluginRuntime.GetRequiredString(parameters, "shortcutType");
    var newName = PluginRuntime.GetOptionalString(parameters, "newName");

    if (shortcutType.Equals("corridor", StringComparison.OrdinalIgnoreCase))
    {
      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["shortcutName"] = shortcutName,
        ["shortcutType"] = shortcutType,
        ["newName"] = newName,
        ["status"] = "unsupported",
        ["notes"] = new List<string>
        {
          "Corridor references cannot be promoted in Civil 3D.",
        },
      });
    }

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["shortcutName"] = shortcutName,
      ["shortcutType"] = shortcutType,
      ["newName"] = newName,
      ["status"] = "manual_step_required",
      ["dialogRequired"] = true,
      ["notes"] = new List<string>
      {
        "Promotion currently requires selecting the reference in Toolspace and using Promote.",
        "The native plugin reports the request and current limits rather than pretending promotion was completed automatically.",
      },
    });
  }

  private static void RegisterShortcutCandidate(AcDbObject dbObject, string objectType, List<Dictionary<string, object?>> incoming, List<Dictionary<string, object?>> exportable)
  {
    var isReference = GetAnyBool(dbObject, "IsReferenceObject", "IsReference", "IsDrefObject") ?? false;
    var name = CivilObjectUtils.GetName(dbObject) ?? CivilObjectUtils.GetHandle(dbObject);

    if (isReference && IsIncomingSupportedType(objectType))
    {
      var sourceFilePath = GetAnyString(dbObject, "SourceFilePath", "ReferencePath", "SourceDrawingPath", "SourceDrawing", "ShortcutSourceFile") ?? string.Empty;
      var isValid = GetAnyBool(dbObject, "IsReferenceValid", "ReferenceIsValid", "IsValid") ?? true;
      var isStale = GetAnyBool(dbObject, "IsReferenceStale", "ReferenceIsStale", "IsOutOfDate", "IsReferenceOutOfDate") ?? false;

      incoming.Add(new Dictionary<string, object?>
      {
        ["objectName"] = name,
        ["objectType"] = objectType,
        ["sourceFilePath"] = sourceFilePath,
        ["isSynced"] = !isStale,
        ["isValid"] = isValid,
      });
      return;
    }

    var isExported = GetAnyBool(dbObject, "IsExportedToDataShortcuts", "IsPublishedAsDataShortcut", "IsSharedAsShortcut") ?? false;
    exportable.Add(new Dictionary<string, object?>
    {
      ["objectName"] = name,
      ["objectType"] = objectType,
      ["isExported"] = isExported,
    });
  }

  private static bool IsIncomingSupportedType(string objectType)
  {
    return objectType is "surface" or "alignment" or "profile" or "pipe_network";
  }

  private static IEnumerable<ObjectId> GetPipeNetworkIds(CivilDocument civilDoc)
  {
    foreach (var candidateProp in new[] { "PipeNetworkCollection", "NetworkCollection", "PipeNetworks", "Networks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }

    foreach (var objectId in CivilObjectUtils.ToObjectIds(CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds")))
    {
      if (objectId != ObjectId.Null)
      {
        yield return objectId;
      }
    }
  }

  private static IEnumerable<ObjectId> GetPressureNetworkIds(CivilDocument civilDoc)
  {
    foreach (var candidateProp in new[] { "PressureNetworkCollection", "PressureNetworks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }

    foreach (var objectId in CivilObjectUtils.ToObjectIds(CivilObjectUtils.InvokeMethod(civilDoc, "GetPressureNetworkIds")))
    {
      if (objectId != ObjectId.Null)
      {
        yield return objectId;
      }
    }
  }

  private static bool? GetAnyBool(object value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var propertyValue = CivilObjectUtils.GetBoolProperty(value, propertyName);
      if (propertyValue.HasValue)
      {
        return propertyValue.Value;
      }
    }

    return null;
  }

  private static string? GetAnyString(object value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var propertyValue = CivilObjectUtils.GetStringProperty(value, propertyName);
      if (!string.IsNullOrWhiteSpace(propertyValue))
      {
        return propertyValue;
      }
    }

    return null;
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

  private static void SendCommand(Document doc, string commandName)
  {
    try
    {
      doc.SendStringToExecute($"{commandName}\n", true, false, false);
    }
    catch (Exception ex)
    {
      throw new JsonRpcDispatchException("CIVIL3D.COMMAND_FAILED", $"Failed to execute {commandName}: {ex.Message}");
    }
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
}
