using System.Collections;
using System.Linq;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;
using CivilEntity = Autodesk.Civil.DatabaseServices.Entity;
using DataShortcutKey = Autodesk.Civil.DataShortcuts.DataShortcutKey;

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
      var command = ResolveSynchronizeCommand();
      SendCommand(doc, command);

      var notes = new List<string>
      {
        $"The {command} command was launched in Civil 3D.",
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
        ["command"] = command,
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

    // Civil 3D exposes no managed promote API; _AeccPromoteReference (the
    // Prospector "Promote" command, name verified in the 2027 CUIx) is driven
    // with the reference pre-selected as the pickfirst set. The command runs
    // after this request returns, so the result is "initiated" — confirm with
    // data_shortcut_references that the object is no longer a reference.
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var match = FindReference(
        civilDoc,
        transaction,
        shortcutType,
        shortcutName,
        $"{shortcutType} '{shortcutName}' is already a local object, not a data-shortcut reference.");

      try
      {
        doc.Editor.SetImpliedSelection(new[] { match.Entity.ObjectId });
      }
      catch (Exception)
      {
        // Without a pickfirst set the command simply prompts for the object.
      }

      SendCommand(doc, "_AeccPromoteReference");

      return new Dictionary<string, object?>
      {
        ["shortcutName"] = shortcutName,
        ["shortcutType"] = shortcutType,
        ["handle"] = match.Entity.Handle.ToString(),
        ["newName"] = newName,
        ["status"] = "initiated",
        ["command"] = "_AeccPromoteReference",
        ["notes"] = new List<string>
        {
          "The reference was pre-selected and _AeccPromoteReference was queued; if Civil 3D prompts, select the same object.",
          "Promotion breaks the link to the source drawing and cannot be undone by synchronizing.",
          string.IsNullOrWhiteSpace(newName)
            ? "Verify completion with civil3d_project data_shortcut_references (the object should no longer be listed)."
            : $"Rename to '{newName}' after the command completes; renaming is not applied automatically.",
        },
      };
    });
  }

  // ---------------------------------------------------------------------------
  // Reference inventory, health, and repair (typed Civil 3D API)
  // ---------------------------------------------------------------------------

  public static Task<object?> ListDataShortcutReferencesAsync(JsonObject? parameters)
  {
    var onlyProblems = PluginRuntime.GetOptionalBool(parameters, "onlyProblems") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Civil 3D calls stay on the host thread, inside the gate.
      var projectContext = GetDataShortcutProjectContext();
      var references = FindReferenceCandidates(civilDoc, transaction)
        .Where(candidate => candidate.Entity.IsReferenceObject)
        .Select(candidate => DescribeReference(candidate, projectContext))
        .Where(item => !onlyProblems || (item["status"] as string) != "current")
        .ToList();

      var statusCounts = references
        .GroupBy(item => item["status"]?.ToString() ?? "unknown")
        .ToDictionary(group => group.Key, group => (object?)group.Count());

      return new Dictionary<string, object?>
      {
        ["workingFolder"] = projectContext.WorkingFolder,
        ["currentProjectFolder"] = projectContext.CurrentProject,
        ["currentProjectPath"] = projectContext.CurrentProjectPath,
        ["drawingProjectId"] = SafeGetDrawingProjectId(database),
        ["count"] = references.Count,
        ["statusCounts"] = statusCounts,
        ["references"] = references,
        ["notes"] = new List<string>
        {
          "status: current | out_of_date (source changed; run data_shortcut_sync) | broken (reference invalid; run data_shortcut_repair) | source_missing (source drawing not found; run data_shortcut_repair with a new sourcePath) | unknown (Civil 3D could not report the reference's health; isValid/isStale are null).",
        },
      };
    });
  }

  public static Task<object?> RepairDataShortcutReferenceAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");
    var rawSourcePath = PluginRuntime.GetRequiredString(parameters, "sourcePath");
    var autoRepairOther = PluginRuntime.GetOptionalBool(parameters, "autoRepairOther") ?? false;
    var sourcePath = FileBoundary.ResolveImportPath(rawSourcePath, ".dwg");

    return CivilExecution.ExecuteLockedWithoutTransactionAsync<object?>((doc, civilDoc, database) =>
    {
      // Resolve (and if needed load) the mixed-mode data-shortcut assembly on
      // the host thread, never on the RPC worker thread.
      var dataShortcutsType = ResolveDataShortcutsType()
        ?? throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          "The Civil 3D data-shortcut API (AeccDataShortcutMgd) is not available in this host.");
      var projectContext = GetDataShortcutProjectContext();
      ObjectId referenceId;
      string? previousSource;
      using (var transaction = database.TransactionManager.StartOpenCloseTransaction())
      {
        var candidate = FindReference(
          civilDoc,
          transaction,
          objectType,
          objectName,
          $"{objectType} '{objectName}' is a local object, not a data-shortcut reference.");

        referenceId = candidate.Entity.ObjectId;
        previousSource = SafeReferenceKey(candidate.Entity)?.SourceDrawing;
      }

      if (string.Equals(database.Filename, sourcePath, StringComparison.OrdinalIgnoreCase))
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "A reference cannot be repaired to point at the host drawing itself.");
      }

      object? invokeResult;
      try
      {
        if (!Civil3DCompatibility.TryInvokeStaticMethod(dataShortcutsType, "RepairBrokenDRef", out invokeResult, referenceId, sourcePath, autoRepairOther))
        {
          throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "DataShortcuts.RepairBrokenDRef(ObjectId, string, bool) is not available in this Civil 3D version.");
        }
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Civil 3D rejected the repair: {exception.InnerException?.Message ?? exception.Message}");
      }

      Dictionary<string, object?> after;
      using (var transaction = database.TransactionManager.StartOpenCloseTransaction())
      {
        var repaired = CivilObjectUtils.GetRequiredObject<CivilEntity>(transaction, referenceId, OpenMode.ForRead);
        after = DescribeReference(new ReferenceCandidate(objectType, objectName, repaired), projectContext);
      }

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["objectName"] = objectName,
        ["previousSourceDrawing"] = previousSource,
        ["requestedSourceDrawing"] = sourcePath,
        ["autoRepairOther"] = autoRepairOther,
        ["repaired"] = invokeResult as bool? ?? false,
        ["reference"] = after,
        ["notes"] = new List<string>
        {
          "The source drawing must contain an object with the same name and type; run data_shortcut_sync afterwards if the reference reports out_of_date.",
        },
      };
    });
  }

  private sealed record ReferenceCandidate(string ObjectType, string Name, CivilEntity Entity);

  private sealed record DataShortcutProjectContext(string? WorkingFolder, string? CurrentProject, string? CurrentProjectPath);

  // More than one object can match a type + name (profiles are matched by
  // their own name, and profile names are only unique per alignment, so a
  // local profile and a referenced one can share a name). Prefer a
  // reference among the matches; only when every match is local is the
  // request rejected as "not a reference".
  private static ReferenceCandidate FindReference(
    CivilDocument civilDoc,
    Transaction transaction,
    string objectType,
    string objectName,
    string localOnlyMessage)
  {
    var sawLocal = false;
    foreach (var candidate in FindReferenceCandidates(civilDoc, transaction))
    {
      if (!string.Equals(candidate.ObjectType, objectType, StringComparison.OrdinalIgnoreCase) ||
          !string.Equals(candidate.Name, objectName, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      if (candidate.Entity.IsReferenceObject)
      {
        return candidate;
      }

      sawLocal = true;
    }

    throw sawLocal
      ? new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", localOnlyMessage)
      : new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No {objectType} named '{objectName}' exists in the current drawing.");
  }

  private static IEnumerable<ReferenceCandidate> FindReferenceCandidates(CivilDocument civilDoc, Transaction transaction)
  {
    ReferenceCandidate? Open(ObjectId id, string objectType)
    {
      if (id.IsNull || id.IsErased)
      {
        return null;
      }

      return transaction.GetObject(id, OpenMode.ForRead) is CivilEntity entity
        ? new ReferenceCandidate(objectType, CivilObjectUtils.GetName(entity) ?? entity.Handle.ToString(), entity)
        : null;
    }

    foreach (ObjectId id in civilDoc.GetSurfaceIds())
    {
      if (Open(id, "surface") is { } surface) yield return surface;
    }

    foreach (ObjectId id in civilDoc.GetAlignmentIds())
    {
      if (Open(id, "alignment") is not { } alignmentCandidate) continue;
      yield return alignmentCandidate;
      if (alignmentCandidate.Entity is Alignment alignment)
      {
        foreach (ObjectId profileId in alignment.GetProfileIds())
        {
          if (Open(profileId, "profile") is { } profile) yield return profile;
        }
      }
    }

    foreach (var id in GetPipeNetworkIds(civilDoc))
    {
      if (Open(id, "pipe_network") is { } network) yield return network;
    }

    foreach (var id in GetPressureNetworkIds(civilDoc))
    {
      if (Open(id, "pressure_network") is { } network) yield return network;
    }

    foreach (ObjectId id in civilDoc.CorridorCollection)
    {
      if (Open(id, "corridor") is { } corridor) yield return corridor;
    }

    foreach (ObjectId id in civilDoc.GetViewFrameGroupIds())
    {
      if (Open(id, "view_frame_group") is { } group) yield return group;
    }
  }

  private static Dictionary<string, object?> DescribeReference(ReferenceCandidate candidate, DataShortcutProjectContext projectContext)
  {
    var entity = candidate.Entity;
    var key = SafeReferenceKey(entity);
    // A failed read stays null: an unverifiable reference is reported as
    // "unknown", never as healthy.
    var isValid = SafeBool(() => entity.IsReferenceValid);
    var isStale = SafeBool(() => entity.IsReferenceStale);
    var sourceExisting = SafeBool(() => entity.IsReferencedSourceExisting)
      ?? SafeBool(() => key?.IsSourceDrawingExistent);
    var sourceDrawing = key == null ? null : SafeString(() => key.SourceDrawing);

    var status = sourceExisting == false
      ? "source_missing"
      : isValid == false
        ? "broken"
        : isStale == true
          ? "out_of_date"
          : isValid == true && isStale == false
            ? "current"
            : "unknown";

    return new Dictionary<string, object?>
    {
      ["objectName"] = candidate.Name,
      ["objectType"] = candidate.ObjectType,
      ["handle"] = entity.Handle.ToString(),
      ["layer"] = entity.Layer,
      ["status"] = status,
      ["isValid"] = isValid,
      ["isStale"] = isStale,
      ["isPartial"] = SafeBool(() => entity.IsPartialReferenceObject) ?? false,
      ["sourceDrawing"] = sourceDrawing,
      ["sourceDrawingExists"] = sourceExisting,
      ["sourceObjectName"] = key == null ? null : SafeString(() => key.Name),
      ["sourceObjectType"] = key == null ? null : SafeString(() => key.Type.ToString()),
      ["sourceObjectHandle"] = key == null ? null : SafeString(() => (((ulong)key.HandleHigh << 32) | key.HandleLow).ToString("X")),
      ["sourceLocation"] = ClassifySourceLocation(sourceDrawing, projectContext),
    };
  }

  private static string ClassifySourceLocation(string? sourceDrawing, DataShortcutProjectContext projectContext)
  {
    if (string.IsNullOrWhiteSpace(sourceDrawing))
    {
      return "unknown";
    }

    static bool Under(string path, string? root) =>
      !string.IsNullOrWhiteSpace(root) &&
      path.StartsWith(root.TrimEnd('\\', '/') + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    if (Under(sourceDrawing, projectContext.CurrentProjectPath))
    {
      return "current_project";
    }

    return Under(sourceDrawing, projectContext.WorkingFolder) ? "other_project_in_working_folder" : "outside_working_folder";
  }

  private static DataShortcutKey? SafeReferenceKey(CivilEntity entity)
  {
    try
    {
      return entity.GetReferenceInfo();
    }
    catch (Exception)
    {
      return null;
    }
  }

  private static bool? SafeBool(Func<bool?> read)
  {
    try
    {
      return read();
    }
    catch (Exception)
    {
      return null;
    }
  }

  private static string? SafeString(Func<string?> read)
  {
    try
    {
      return read();
    }
    catch (Exception)
    {
      return null;
    }
  }

  private static Type? ResolveDataShortcutsType()
  {
    return Civil3DCompatibility.FindSiblingAssemblyType(
      typeof(CivilDocument),
      "AeccDataShortcutMgd.dll",
      "Autodesk.Civil.DataShortcuts.DataShortcuts");
  }

  private static DataShortcutProjectContext GetDataShortcutProjectContext()
  {
    var type = ResolveDataShortcutsType();
    if (type == null)
    {
      return new DataShortcutProjectContext(null, null, null);
    }

    string? Invoke(string method)
    {
      try
      {
        return Civil3DCompatibility.TryInvokeStaticMethod(type, method, out var value) ? value as string : null;
      }
      catch (Exception)
      {
        return null;
      }
    }

    var workingFolder = Invoke("GetWorkingFolder");
    var currentProject = Invoke("GetCurrentProjectFolder");
    string? currentProjectPath = null;
    if (!string.IsNullOrWhiteSpace(currentProject))
    {
      currentProjectPath = Path.IsPathFullyQualified(currentProject) || string.IsNullOrWhiteSpace(workingFolder)
        ? currentProject
        : Path.Combine(workingFolder, currentProject);
    }

    return new DataShortcutProjectContext(workingFolder, currentProject, currentProjectPath);
  }

  private static string? SafeGetDrawingProjectId(Database database)
  {
    var type = ResolveDataShortcutsType();
    if (type == null)
    {
      return null;
    }

    try
    {
      return Civil3DCompatibility.TryInvokeStaticMethod(type, "GetAssociateShortcutProjectIdFromDrawing", out var value, database)
        ? value as string
        : null;
    }
    catch (Exception)
    {
      return null;
    }
  }

  private static void RegisterShortcutCandidate(AcDbObject dbObject, string objectType, List<Dictionary<string, object?>> incoming, List<Dictionary<string, object?>> exportable)
  {
    var isReference = GetAnyBool(dbObject, "IsReferenceObject", "IsReference", "IsDrefObject") ?? false;
    var name = CivilObjectUtils.GetName(dbObject) ?? CivilObjectUtils.GetHandle(dbObject);

    if (isReference && IsIncomingSupportedType(objectType))
    {
      // Civil 3D 2027 exposes the source only through Entity.GetReferenceInfo();
      // the legacy property probes stay as a fallback for older hosts.
      var sourceFilePath = (dbObject is CivilEntity civilEntity ? SafeString(() => SafeReferenceKey(civilEntity)?.SourceDrawing) : null)
        ?? GetAnyString(dbObject, "SourceFilePath", "ReferencePath", "SourceDrawingPath", "SourceDrawing", "ShortcutSourceFile")
        ?? string.Empty;
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
    foreach (ObjectId objectId in civilDoc.GetPipeNetworkIds())
    {
      if (objectId != ObjectId.Null)
      {
        yield return objectId;
      }
    }
  }

  private static IEnumerable<ObjectId> GetPressureNetworkIds(CivilDocument civilDoc)
  {
    foreach (ObjectId objectId in civilDoc.GetPressurePipeNetworkIds())
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

  // The synchronize command's global name: AeccSynchronizeReferences in the
  // Civil 3D 2027 CUIx, SynchronizeReferences in older plugin builds. Run the
  // one this Civil 3D defines, so a release that names it differently fails
  // here with a clear message instead of as an unknown command.
  internal static readonly string[] SynchronizeCommandNames = { "AeccSynchronizeReferences", "SynchronizeReferences" };

  private static string ResolveSynchronizeCommand()
  {
    foreach (var name in SynchronizeCommandNames)
    {
      bool defined;
      try
      {
        defined = Autodesk.AutoCAD.Internal.Utils.IsCommandDefined(name);
      }
      catch (Exception)
      {
        // The lookup is an internal API; without it, try the 2027 name.
        return "_" + SynchronizeCommandNames[0];
      }

      if (defined)
      {
        return "_" + name;
      }
    }

    throw new JsonRpcDispatchException(
      "CIVIL3D.COMMAND_FAILED",
      $"This Civil 3D defines none of the data shortcut synchronize commands ({string.Join(", ", SynchronizeCommandNames)}). Synchronize from the Prospector's Data Shortcuts node instead.");
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
