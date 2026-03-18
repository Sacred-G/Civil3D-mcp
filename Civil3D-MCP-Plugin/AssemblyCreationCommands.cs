using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Assembly and subassembly creation/editing commands.
/// Uses reflection for Civil 3D API compatibility.
/// </summary>
public static class AssemblyCreationCommands
{
  // ─── createAssembly ─────────────────────────────────────────────────────────

  public static Task<object?> CreateAssemblyAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var insertX = PluginRuntime.GetOptionalDouble(parameters, "insertX") ?? 0.0;
    var insertY = PluginRuntime.GetOptionalDouble(parameters, "insertY") ?? 0.0;
    var description = PluginRuntime.GetOptionalString(parameters, "description") ?? string.Empty;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var insertPoint = new Point3d(insertX, insertY, 0);
      var assemblyId = CreateAssembly(civilDoc, database, transaction, name, insertPoint, description);

      if (assemblyId == ObjectId.Null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Failed to create assembly '{name}'.");
      }

      var assembly = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, assemblyId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["name"] = CivilObjectUtils.GetName(assembly) ?? name,
        ["handle"] = CivilObjectUtils.GetHandle(assembly),
        ["insertX"] = insertX,
        ["insertY"] = insertY,
        ["created"] = true,
      };
    });
  }

  // ─── createSubassembly ──────────────────────────────────────────────────────

  public static Task<object?> CreateSubassemblyAsync(JsonObject? parameters)
  {
    var assemblyName = PluginRuntime.GetRequiredString(parameters, "assemblyName");
    var subassemblyType = PluginRuntime.GetRequiredString(parameters, "subassemblyType");
    var side = PluginRuntime.GetOptionalString(parameters, "side") ?? "Right";

    // Collect all parameters as a dictionary for property injection
    var subParams = parameters?["parameters"] as JsonObject;
    var paramDict = new Dictionary<string, object?>();
    if (subParams != null)
    {
      foreach (var kv in subParams)
      {
        paramDict[kv.Key] = kv.Value?.GetValue<object>();
      }
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var assembly = FindAssemblyByName(civilDoc, transaction, assemblyName, OpenMode.ForWrite);
      var subassemblyId = AddSubassembly(civilDoc, database, transaction, assembly, subassemblyType, side, paramDict);

      if (subassemblyId == ObjectId.Null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Failed to add subassembly '{subassemblyType}' to assembly '{assemblyName}'.");
      }

      var sub = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, subassemblyId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["assemblyName"] = assemblyName,
        ["subassemblyName"] = CivilObjectUtils.GetName(sub) ?? subassemblyType,
        ["subassemblyType"] = subassemblyType,
        ["side"] = side,
        ["handle"] = CivilObjectUtils.GetHandle(sub),
        ["added"] = true,
      };
    });
  }

  // ─── editAssembly ───────────────────────────────────────────────────────────

  public static Task<object?> EditAssemblyAsync(JsonObject? parameters)
  {
    var assemblyName = PluginRuntime.GetRequiredString(parameters, "assemblyName");
    var subassemblyName = PluginRuntime.GetOptionalString(parameters, "subassemblyName");
    var deleteSubassembly = PluginRuntime.GetOptionalBool(parameters, "delete") ?? false;

    var editParams = parameters?["parameters"] as JsonObject;
    var paramDict = new Dictionary<string, object?>();
    if (editParams != null)
    {
      foreach (var kv in editParams)
      {
        paramDict[kv.Key] = kv.Value?.GetValue<object>();
      }
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var openMode = (deleteSubassembly || paramDict.Count > 0) ? OpenMode.ForWrite : OpenMode.ForRead;
      var assembly = FindAssemblyByName(civilDoc, transaction, assemblyName, openMode);
      var subassemblyIds = GetSubassemblyIds(assembly).ToList();

      // List mode — no subassembly name provided
      if (string.IsNullOrWhiteSpace(subassemblyName))
      {
        var subs = subassemblyIds.Select(id =>
        {
          var sub = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
          return new Dictionary<string, object?>
          {
            ["name"] = CivilObjectUtils.GetName(sub) ?? id.Handle.ToString(),
            ["handle"] = CivilObjectUtils.GetHandle(sub),
            ["type"] = sub.GetType().Name,
          };
        }).ToList();

        return new Dictionary<string, object?>
        {
          ["assemblyName"] = assemblyName,
          ["subassemblyCount"] = subs.Count,
          ["subassemblies"] = subs,
        };
      }

      // Find the target subassembly
      DBObject? targetSub = null;
      foreach (var id in subassemblyIds)
      {
        var sub = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, openMode);
        if (string.Equals(CivilObjectUtils.GetName(sub), subassemblyName, StringComparison.OrdinalIgnoreCase))
        {
          targetSub = sub;
          break;
        }
      }

      if (targetSub == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Subassembly '{subassemblyName}' not found in assembly '{assemblyName}'.");
      }

      if (deleteSubassembly)
      {
        targetSub.Erase();
        return new Dictionary<string, object?>
        {
          ["assemblyName"] = assemblyName,
          ["subassemblyName"] = subassemblyName,
          ["deleted"] = true,
        };
      }

      // Apply parameter updates via reflection
      var updatedParams = new List<string>();
      foreach (var kv in paramDict)
      {
        var prop = targetSub.GetType().GetProperty(kv.Key,
          BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.CanWrite && kv.Value != null)
        {
          try
          {
            var converted = Convert.ChangeType(kv.Value, prop.PropertyType);
            prop.SetValue(targetSub, converted);
            updatedParams.Add(kv.Key);
          }
          catch { /* skip params we can't set */ }
        }
      }

      return new Dictionary<string, object?>
      {
        ["assemblyName"] = assemblyName,
        ["subassemblyName"] = subassemblyName,
        ["updatedParameters"] = updatedParams,
        ["updated"] = updatedParams.Count > 0,
        ["note"] = updatedParams.Count == 0 ? "No properties updated — rebuild corridor to apply any parameter changes." : "Parameters updated. Rebuild corridor to apply changes.",
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  private static DBObject FindAssemblyByName(object civilDoc, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (var assemblyTypeName in new[]
    {
      "Autodesk.Civil.DatabaseServices.Assembly",
      "Autodesk.Civil.DatabaseServices.AssemblyAECC",
    })
    {
      var type = Type.GetType($"{assemblyTypeName}, AeccDbMgd", false);
      if (type == null) continue;

      var collectionProp = civilDoc.GetType().GetProperty("AssemblyCollection",
        BindingFlags.Public | BindingFlags.Instance);
      var collection = collectionProp?.GetValue(civilDoc);
      if (collection is IEnumerable enumerable)
      {
        foreach (var item in enumerable)
        {
          if (item is ObjectId id)
          {
            var obj = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, openMode);
            if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
              return obj;
          }
        }
      }
    }

    // Fallback: scan model space block for assembly objects
    var db = transaction.GetObject(
      Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument!.Database.CurrentSpaceId,
      OpenMode.ForRead) as BlockTableRecord;

    if (db != null)
    {
      foreach (ObjectId id in db)
      {
        try
        {
          var obj = transaction.GetObject(id, openMode, false, true);
          if (obj?.GetType().Name.IndexOf("Assembly", StringComparison.OrdinalIgnoreCase) >= 0
              && string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
          {
            return obj;
          }
        }
        catch { /* skip */ }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Assembly '{name}' was not found in the drawing.");
  }

  private static ObjectId CreateAssembly(object civilDoc, Database database, Transaction transaction, string name, Point3d insertPoint, string description)
  {
    foreach (var assemblyTypeName in new[]
    {
      "Autodesk.Civil.DatabaseServices.Assembly",
      "Autodesk.Civil.DatabaseServices.AssemblyAECC",
    })
    {
      var type = Type.GetType($"{assemblyTypeName}, AeccDbMgd", false);
      if (type == null) continue;

      var createMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name == "Create")
        .OrderBy(m => m.GetParameters().Length)
        .ToList();

      foreach (var method in createMethods)
      {
        try
        {
          var mParams = method.GetParameters();
          object?[] args;

          if (mParams.Length == 2 && mParams[0].ParameterType == typeof(string))
            args = [name, insertPoint];
          else if (mParams.Length == 3)
            args = [name, insertPoint, description];
          else
            continue;

          var result = method.Invoke(null, args);
          if (result is ObjectId id && id != ObjectId.Null)
            return id;
        }
        catch { /* try next overload */ }
      }
    }

    return ObjectId.Null;
  }

  private static ObjectId AddSubassembly(object civilDoc, Database database, Transaction transaction, DBObject assembly, string subassemblyType, string side, Dictionary<string, object?> parameters)
  {
    // Try to find the subassembly catalog type and invoke its Create/Add method
    foreach (var nsPrefix in new[] { "Autodesk.Civil.DatabaseServices.", "Autodesk.Civil.DatabaseServices.Styles." })
    {
      var type = Type.GetType($"{nsPrefix}{subassemblyType}, AeccDbMgd", false)
               ?? Type.GetType($"{nsPrefix}{subassemblyType}, AeccDbCoreMgd", false);

      if (type == null) continue;

      var createMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name == "Create" || m.Name == "Add")
        .OrderBy(m => m.GetParameters().Length)
        .ToList();

      foreach (var method in createMethods)
      {
        try
        {
          var mParams = method.GetParameters();
          object?[] args;

          if (mParams.Length == 1)
            args = [assembly.ObjectId];
          else if (mParams.Length == 2)
            args = [assembly.ObjectId, side];
          else
            continue;

          var result = method.Invoke(null, args);
          if (result is ObjectId id && id != ObjectId.Null)
          {
            ApplySubassemblyParameters(transaction, id, parameters);
            return id;
          }
        }
        catch { /* try next */ }
      }
    }

    // Fallback: try via assembly's AddSubassembly method
    var addMethod = assembly.GetType().GetMethod("AddSubassembly",
      BindingFlags.Public | BindingFlags.Instance);
    if (addMethod != null)
    {
      try
      {
        var result = addMethod.Invoke(assembly, new object?[] { subassemblyType });
        if (result is ObjectId id && id != ObjectId.Null)
        {
          ApplySubassemblyParameters(transaction, id, parameters);
          return id;
        }
      }
      catch { /* fall through */ }
    }

    return ObjectId.Null;
  }

  private static void ApplySubassemblyParameters(Transaction transaction, ObjectId subId, Dictionary<string, object?> parameters)
  {
    if (parameters.Count == 0) return;
    try
    {
      var sub = transaction.GetObject(subId, OpenMode.ForWrite, false, true);
      foreach (var kv in parameters)
      {
        var prop = sub?.GetType().GetProperty(kv.Key,
          BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.CanWrite && kv.Value != null)
        {
          try
          {
            var converted = Convert.ChangeType(kv.Value, prop.PropertyType);
            prop.SetValue(sub, converted);
          }
          catch { /* skip */ }
        }
      }
    }
    catch { /* skip */ }
  }

  private static IEnumerable<ObjectId> GetSubassemblyIds(DBObject assembly)
  {
    foreach (var name in new[] { "GetSubassemblyIds", "SubassemblyIds", "Subassemblies", "SubassemblyCollection" })
    {
      var method = assembly.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
      if (method != null)
      {
        var result = method.Invoke(assembly, null);
        if (result is IEnumerable enumerable)
        {
          foreach (var item in enumerable)
          {
            if (item is ObjectId id && id != ObjectId.Null) yield return id;
          }
          yield break;
        }
      }

      var prop = assembly.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
      if (prop != null)
      {
        var val = prop.GetValue(assembly);
        if (val is IEnumerable enumerable2)
        {
          foreach (var item in enumerable2)
          {
            if (item is ObjectId id && id != ObjectId.Null) yield return id;
          }
          yield break;
        }
      }
    }
  }
}
