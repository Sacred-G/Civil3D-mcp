using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcRuntimeException = Autodesk.AutoCAD.Runtime.Exception;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

/// <summary>
/// External reference (xref) management for the active drawing.
///
/// Reads use the xref graph inside a read-only transaction. Mutations that the
/// AutoCAD database performs with its own internal transactions (unload,
/// detach) run under the document lock WITHOUT an enclosing transaction via
/// <see cref="CivilExecution.ExecuteLockedWithoutTransactionAsync{T}"/>. Reload
/// (including repath's reload) and bind go through the -XREF command instead,
/// because Database.ReloadXrefs / BindXrefs never return control to the
/// command context (verified live on Civil 3D 2027).
/// Every caller-supplied path passes <see cref="FileBoundary.ResolveImportPath"/>
/// (canonicalized, inside CIVIL3D_IMPORT_ROOTS, .dwg only, must exist).
/// </summary>
public static class XrefCommands
{
  public static Task<object?> ListXrefsAsync(JsonObject? parameters)
  {
    var includeNested = PluginRuntime.GetOptionalBool(parameters, "includeNested") ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var xrefs = DescribeXrefs(database, transaction)
        .Where(item => includeNested || !(item["isNested"] as bool? ?? false))
        .ToList();

      var statusCounts = xrefs
        .GroupBy(item => item["status"]?.ToString() ?? "unknown")
        .ToDictionary(group => group.Key, group => (object?)group.Count());

      return new Dictionary<string, object?>
      {
        ["hostDrawing"] = database.Filename,
        ["hostIsSaved"] = doc.IsNamedDrawing,
        ["count"] = xrefs.Count,
        ["statusCounts"] = statusCounts,
        ["xrefs"] = xrefs,
      };
    });
  }

  public static Task<object?> AttachXrefAsync(JsonObject? parameters)
  {
    return InsertXrefAsync(parameters, overlay: false);
  }

  public static Task<object?> OverlayXrefAsync(JsonObject? parameters)
  {
    return InsertXrefAsync(parameters, overlay: true);
  }

  public static Task<object?> DetachXrefAsync(JsonObject? parameters)
  {
    var names = RequireNames(parameters);

    return CivilExecution.ExecuteLockedWithoutTransactionAsync<object?>((doc, civilDoc, database) =>
    {
      var targets = ResolveXrefTargets(database, names);
      var results = new List<Dictionary<string, object?>>();
      foreach (var target in targets)
      {
        results.Add(RunPerXref(target.Name, () => database.DetachXref(target.Id), "detached"));
      }

      return BuildBatchResult("detach", results);
    });
  }

  public static Task<object?> ReloadXrefsAsync(JsonObject? parameters)
  {
    var names = RequireNames(parameters);
    return ReloadViaCommandLineAsync(names, "reload");
  }

  // Database.ReloadXrefs from the plugin's command context reloads the xref but
  // never returns control (live on Civil 3D 2027: the xref showed "loaded" while
  // the request hung until the 120 s timeout, every time). Reload through the
  // -XREF command instead, on the command-sequence path.
  private static Task<object?> ReloadViaCommandLineAsync(List<string> names, string operation, Action<Dictionary<string, object?>>? decorate = null)
  {
    return CivilExecution.ExecuteCommandSequenceAsync<object?>(async (doc, cancellationToken) =>
    {
      List<XrefTarget> targets;
      using (doc.LockDocument())
      {
        targets = ResolveXrefTargets(doc.Database, names);
      }

      cancellationToken.ThrowIfCancellationRequested();
      await CommandLineRunner.RunAsync(doc, "XREF", "_.-XREF", "_Reload", string.Join(",", targets.Select(target => target.Name)));

      using (doc.LockDocument())
      {
        var status = BuildStatusResult(operation, doc.Database, targets);
        decorate?.Invoke(status);
        return (object?)status;
      }
    });
  }

  public static Task<object?> UnloadXrefsAsync(JsonObject? parameters)
  {
    var names = RequireNames(parameters);

    return CivilExecution.ExecuteLockedWithoutTransactionAsync<object?>((doc, civilDoc, database) =>
    {
      var targets = ResolveXrefTargets(database, names);
      RunBatch(() => database.UnloadXrefs(ToCollection(targets)), "unload");
      return BuildStatusResult("unload", database, targets);
    });
  }

  public static Task<object?> BindXrefsAsync(JsonObject? parameters)
  {
    var names = RequireNames(parameters);
    var bindType = (PluginRuntime.GetOptionalString(parameters, "bindType") ?? "bind").Trim().ToLowerInvariant();
    if (bindType is not ("bind" or "insert"))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "bindType must be 'bind' (keeps xref-dependent symbol prefixes) or 'insert' (merges symbols like INSERT).");
    }

    // Like ReloadXrefs, Database.BindXrefs completes but never returns control to
    // the command context (live on Civil 3D 2027), so bind runs through -XREF.
    return CivilExecution.ExecuteCommandSequenceAsync<object?>(async (doc, cancellationToken) =>
    {
      var database = doc.Database;
      List<XrefTarget> targets;
      using (doc.LockDocument())
      using (var transaction = database.TransactionManager.StartOpenCloseTransaction())
      {
        targets = ResolveXrefTargets(database, names);
        foreach (var target in targets)
        {
          var btr = (BlockTableRecord)transaction.GetObject(target.Id, OpenMode.ForRead);
          if (btr.XrefStatus != XrefStatus.Resolved)
          {
            throw new JsonRpcDispatchException(
              "CIVIL3D.CONFLICT",
              $"Xref '{target.Name}' is {MapStatus(btr.XrefStatus)}; only loaded (resolved) xrefs can be bound. Reload or repath it first.");
          }
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      var previousBindType = App.GetSystemVariable("BINDTYPE");
      try
      {
        App.SetSystemVariable("BINDTYPE", bindType == "insert" ? (short)1 : (short)0);
        await CommandLineRunner.RunAsync(doc, "XREF", "_.-XREF", "_Bind", string.Join(",", targets.Select(target => target.Name)));
      }
      finally
      {
        App.SetSystemVariable("BINDTYPE", previousBindType);
      }

      return (object?)new Dictionary<string, object?>
      {
        ["operation"] = "bind",
        ["bindType"] = bindType,
        ["bound"] = targets.Select(target => target.Name).ToList(),
        ["notes"] = new List<string>
        {
          bindType == "insert"
            ? "Insert-bind merged the xref's layers/styles into the host symbol tables without the 'xref$0$' prefix."
            : "Bind converted the xref into a block; its dependent symbols were renamed with the '<xref>$0$' prefix.",
        },
      };
    });
  }

  public static Task<object?> RepathXrefAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var rawPath = PluginRuntime.GetRequiredString(parameters, "newPath");
    var pathType = ParsePathType(parameters);
    var reload = PluginRuntime.GetOptionalBool(parameters, "reload") ?? true;
    var newPath = FileBoundary.ResolveImportPath(rawPath, ".dwg");

    return RepathThenMaybeReloadAsync(name, newPath, pathType, reload);
  }

  private static async Task<object?> RepathThenMaybeReloadAsync(string name, string newPath, string pathType, bool reload)
  {
    // Phase 1: change the stored path (no reload; see ReloadViaCommandLineAsync).
    var repathed = await CivilExecution.ExecuteLockedWithoutTransactionAsync((doc, civilDoc, database) =>
    {
      var target = ResolveXrefTargets(database, new List<string> { name }).Single();
      RejectSelfReference(database, newPath);
      var storedPath = BuildStoredPath(doc, database, newPath, pathType);

      string? previousPath;
      using (var transaction = database.TransactionManager.StartTransaction())
      {
        var btr = (BlockTableRecord)transaction.GetObject(target.Id, OpenMode.ForWrite);
        previousPath = btr.PathName;
        btr.PathName = storedPath;
        transaction.Commit();
      }

      return (target, previousPath, storedPath, status: BuildStatusResult("repath", database, new List<XrefTarget> { target }));
    });

    void Decorate(Dictionary<string, object?> status)
    {
      status["previousPath"] = repathed.previousPath;
      status["storedPath"] = repathed.storedPath;
      status["resolvedTarget"] = newPath;
      status["pathType"] = pathType;
      status["reloaded"] = reload;
    }

    if (!reload)
    {
      Decorate(repathed.status);
      return repathed.status;
    }

    // Phase 2: reload through -XREF.
    return await ReloadViaCommandLineAsync(new List<string> { repathed.target.Name }, "repath", Decorate);
  }

  // ---------------------------------------------------------------------------
  // Implementation
  // ---------------------------------------------------------------------------

  private static Task<object?> InsertXrefAsync(JsonObject? parameters, bool overlay)
  {
    var rawPath = PluginRuntime.GetRequiredString(parameters, "path");
    var path = FileBoundary.ResolveImportPath(rawPath, ".dwg");
    var name = PluginRuntime.GetOptionalString(parameters, "name");
    if (string.IsNullOrWhiteSpace(name))
    {
      name = Path.GetFileNameWithoutExtension(path);
    }

    try
    {
      SymbolUtilityServices.ValidateSymbolName(name, false);
    }
    catch (AcRuntimeException)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'{name}' is not a valid block/xref name.");
    }

    var pathType = ParsePathType(parameters);
    var insert = PluginRuntime.GetOptionalBool(parameters, "insert") ?? true;
    var position = ParsePoint(parameters, "insertionPoint");
    var scale = PluginRuntime.GetOptionalDouble(parameters, "scale") ?? 1.0;
    if (scale <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "scale must be greater than zero.");
    }

    var rotationDegrees = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0.0;
    var layer = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      RejectSelfReference(database, path);
      var storedPath = BuildStoredPath(doc, database, path, pathType);

      var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
      if (blockTable.Has(name))
      {
        throw new JsonRpcDispatchException("CIVIL3D.CONFLICT", $"A block or xref named '{name}' already exists in this drawing.");
      }

      ObjectId xrefId;
      try
      {
        xrefId = overlay ? database.OverlayXref(path, name) : database.AttachXref(path, name);
      }
      catch (AcRuntimeException exception)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Civil 3D could not {(overlay ? "overlay" : "attach")} '{path}': {exception.Message}");
      }

      var btr = (BlockTableRecord)transaction.GetObject(xrefId, OpenMode.ForWrite);
      if (!string.Equals(btr.PathName, storedPath, StringComparison.OrdinalIgnoreCase))
      {
        btr.PathName = storedPath;
      }

      string? referenceHandle = null;
      if (insert)
      {
        var modelSpace = (BlockTableRecord)transaction.GetObject(
          SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
        var reference = new BlockReference(position, xrefId)
        {
          ScaleFactors = new Scale3d(scale),
          Rotation = rotationDegrees * Math.PI / 180.0,
        };

        if (!string.IsNullOrWhiteSpace(layer))
        {
          var layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
          if (!layerTable.Has(layer))
          {
            throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layer '{layer}' does not exist in this drawing.");
          }

          reference.Layer = layer;
        }

        modelSpace.AppendEntity(reference);
        transaction.AddNewlyCreatedDBObject(reference, true);
        referenceHandle = reference.Handle.ToString();
      }

      return new Dictionary<string, object?>
      {
        ["operation"] = overlay ? "overlay" : "attach",
        ["name"] = name,
        ["resolvedPath"] = path,
        ["storedPath"] = storedPath,
        ["pathType"] = pathType,
        ["attachment"] = overlay ? "overlay" : "attach",
        ["blockTableRecordHandle"] = btr.Handle.ToString(),
        ["inserted"] = insert,
        ["referenceHandle"] = referenceHandle,
        ["insertionPoint"] = insert ? new Dictionary<string, object?> { ["x"] = position.X, ["y"] = position.Y, ["z"] = position.Z } : null,
        ["scale"] = insert ? scale : null,
        ["rotation"] = insert ? rotationDegrees : null,
        ["layer"] = insert ? layer : null,
        ["status"] = MapStatus(btr.XrefStatus),
      };
    });
  }

  internal static List<Dictionary<string, object?>> DescribeXrefs(Database database, Transaction transaction)
  {
    var results = new List<Dictionary<string, object?>>();
    // Dispose on this (host) thread rather than leaving the native graph to a
    // GC finalizer thread.
    using var graph = database.GetHostDwgXrefGraph(true);
    var loadedByName = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
    var nodes = new List<XrefGraphNode>();
    // GraphNode.In() hands back a plain GraphNode wrapper, not the typed
    // XrefGraphNode, so parents are resolved by native pointer identity.
    var nodesByPointer = new Dictionary<IntPtr, XrefGraphNode>();

    for (var index = 0; index < graph.NumNodes; index++)
    {
      var node = graph.GetXrefNode(index);
      if (node == null)
      {
        continue;
      }

      nodesByPointer[node.UnmanagedObject] = node;
      if (node.BlockTableRecordId.IsNull)
      {
        continue; // the host drawing itself
      }

      nodes.Add(node);
      loadedByName[node.Name] = node.XrefStatus == XrefStatus.Resolved;
    }

    foreach (var node in nodes)
    {
      var parents = new List<string>();
      var hostIsParent = false;
      for (var parentIndex = 0; parentIndex < node.NumIn; parentIndex++)
      {
        var incoming = node.In(parentIndex);
        if (incoming == null || !nodesByPointer.TryGetValue(incoming.UnmanagedObject, out var parent))
        {
          continue;
        }

        if (parent.BlockTableRecordId.IsNull)
        {
          hostIsParent = true;
        }
        else
        {
          parents.Add(parent.Name);
        }
      }

      var isNested = node.IsNested;
      var orphaned = isNested && !hostIsParent && parents.Count > 0 &&
        parents.All(parentName => !(loadedByName.TryGetValue(parentName, out var loaded) && loaded));

      string? savedPath = null;
      string? attachment = null;
      int? instanceCount = null;
      string? handle = null;
      if (node.BlockTableRecordId.Database == database)
      {
        try
        {
          var btr = (BlockTableRecord)transaction.GetObject(node.BlockTableRecordId, OpenMode.ForRead);
          savedPath = btr.PathName;
          attachment = btr.IsFromOverlayReference ? "overlay" : "attach";
          instanceCount = btr.GetBlockReferenceIds(true, false).Count;
          handle = btr.Handle.ToString();
        }
        catch (Exception exception) when (exception is not JsonRpcDispatchException)
        {
          // Nested xref records can be transient; report what the graph knows.
        }
      }

      var foundPath = ResolveFoundPath(node, savedPath, database);
      var status = orphaned ? "orphaned" : MapStatus(node.XrefStatus);

      results.Add(new Dictionary<string, object?>
      {
        ["name"] = node.Name,
        ["handle"] = handle,
        ["savedPath"] = savedPath,
        ["foundPath"] = foundPath,
        ["pathType"] = ClassifyPath(savedPath),
        ["status"] = status,
        ["rawStatus"] = node.XrefStatus.ToString(),
        ["attachment"] = attachment,
        ["isNested"] = isNested,
        ["parents"] = hostIsParent ? parents.Prepend("<host>").ToList() : parents,
        ["childCount"] = node.NumOut,
        ["instanceCount"] = instanceCount,
      });
    }

    return results;
  }

  private static string? ResolveFoundPath(XrefGraphNode node, string? savedPath, Database hostDatabase)
  {
    try
    {
      if (node.XrefStatus == XrefStatus.Resolved && node.Database != null && !string.IsNullOrWhiteSpace(node.Database.Filename))
      {
        return node.Database.Filename;
      }
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
    }

    if (string.IsNullOrWhiteSpace(savedPath))
    {
      return null;
    }

    try
    {
      var found = HostApplicationServices.Current.FindFile(savedPath, hostDatabase, FindFileHint.XRefDrawing);
      return string.IsNullOrWhiteSpace(found) ? null : found;
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      return null;
    }
  }

  internal static string MapStatus(XrefStatus status) => status switch
  {
    XrefStatus.Resolved => "loaded",
    XrefStatus.Unloaded => "unloaded",
    XrefStatus.Unreferenced => "unreferenced",
    XrefStatus.FileNotFound => "not_found",
    XrefStatus.Unresolved => "unresolved",
    _ => "unknown",
  };

  internal static string ClassifyPath(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return "none";
    }

    if (Path.IsPathFullyQualified(path))
    {
      return "absolute";
    }

    // AutoCAD saves a same-folder relative xref as ".\base.dwg"; a bare
    // "base.dwg" is its "No path" option (found through the host folder and
    // the support search paths), reported here as "none".
    return path.Contains('\\') || path.Contains('/') || path.StartsWith('.') ? "relative" : "none";
  }

  private static string ParsePathType(JsonObject? parameters)
  {
    var pathType = (PluginRuntime.GetOptionalString(parameters, "pathType") ?? "absolute").Trim().ToLowerInvariant();
    if (pathType is not ("absolute" or "relative"))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "pathType must be 'absolute' or 'relative'.");
    }

    return pathType;
  }

  private static string BuildStoredPath(Document doc, Database database, string canonicalPath, string pathType)
  {
    if (pathType == "absolute")
    {
      return canonicalPath;
    }

    if (!doc.IsNamedDrawing || string.IsNullOrWhiteSpace(database.Filename))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        "A relative xref path needs a saved host drawing. Save the drawing first or use pathType 'absolute'.");
    }

    var hostDirectory = Path.GetDirectoryName(Path.GetFullPath(database.Filename))
      ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Unable to determine the host drawing folder.");
    var relative = Path.GetRelativePath(hostDirectory, canonicalPath);
    if (Path.IsPathRooted(relative))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        "The xref is on a different drive/share than the host drawing, so a relative path is impossible. Use pathType 'absolute'.");
    }

    return relative.StartsWith("..", StringComparison.Ordinal) ? relative : $".{Path.DirectorySeparatorChar}{relative}";
  }

  private static void RejectSelfReference(Database database, string canonicalPath)
  {
    if (!string.IsNullOrWhiteSpace(database.Filename) &&
        string.Equals(Path.GetFullPath(database.Filename), canonicalPath, StringComparison.OrdinalIgnoreCase))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "A drawing cannot reference itself as an xref.");
    }
  }

  private static Point3d ParsePoint(JsonObject? parameters, string name)
  {
    if (PluginRuntime.GetParameter(parameters, name) is not JsonObject point)
    {
      return Point3d.Origin;
    }

    double Read(string key) => point.TryGetPropertyValue(key, out var value) && value != null ? value.GetValue<double>() : 0.0;
    return new Point3d(Read("x"), Read("y"), Read("z"));
  }

  private static List<string> RequireNames(JsonObject? parameters)
  {
    var names = new List<string>();
    if (PluginRuntime.GetParameter(parameters, "names") is JsonArray array)
    {
      names.AddRange(array
        .Select(item => item?.GetValue<string>())
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Cast<string>());
    }

    var single = PluginRuntime.GetOptionalString(parameters, "name");
    if (!string.IsNullOrWhiteSpace(single))
    {
      names.Add(single);
    }

    names = names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    if (names.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Provide 'name' or a non-empty 'names' array of xref names.");
    }

    return names;
  }

  private sealed record XrefTarget(string Name, ObjectId Id);

  private static List<XrefTarget> ResolveXrefTargets(Database database, List<string> names)
  {
    var targets = new List<XrefTarget>();
    using var transaction = database.TransactionManager.StartOpenCloseTransaction();
    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
    foreach (var name in names)
    {
      if (!blockTable.Has(name))
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Xref '{name}' was not found in this drawing.");
      }

      var id = blockTable[name];
      var btr = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
      if (!btr.IsFromExternalReference)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'{name}' is a block definition, not an xref.");
      }

      targets.Add(new XrefTarget(btr.Name, id));
    }

    return targets;
  }

  private static ObjectIdCollection ToCollection(IEnumerable<XrefTarget> targets)
  {
    var collection = new ObjectIdCollection();
    foreach (var target in targets)
    {
      collection.Add(target.Id);
    }

    return collection;
  }

  private static void RunBatch(Action action, string operation)
  {
    try
    {
      action();
    }
    catch (AcRuntimeException exception)
    {
      throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Xref {operation} failed: {exception.Message}");
    }
  }

  private static Dictionary<string, object?> RunPerXref(string name, Action action, string successStatus)
  {
    try
    {
      action();
      return new Dictionary<string, object?> { ["name"] = name, ["status"] = successStatus };
    }
    catch (AcRuntimeException exception)
    {
      return new Dictionary<string, object?> { ["name"] = name, ["status"] = "failed", ["error"] = exception.Message };
    }
  }

  private static Dictionary<string, object?> BuildBatchResult(string operation, List<Dictionary<string, object?>> results)
  {
    return new Dictionary<string, object?>
    {
      ["operation"] = operation,
      ["succeeded"] = results.Count(item => (item["status"] as string) != "failed"),
      ["failed"] = results.Count(item => (item["status"] as string) == "failed"),
      ["results"] = results,
    };
  }

  private static Dictionary<string, object?> BuildStatusResult(string operation, Database database, List<XrefTarget> targets)
  {
    var wanted = targets.Select(target => target.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
    List<Dictionary<string, object?>> described;
    using (var transaction = database.TransactionManager.StartOpenCloseTransaction())
    {
      described = DescribeXrefs(database, transaction)
        .Where(item => wanted.Contains(item["name"]?.ToString() ?? string.Empty))
        .ToList();
    }

    return new Dictionary<string, object?>
    {
      ["operation"] = operation,
      ["xrefs"] = described,
    };
  }
}
