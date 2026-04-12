using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Civil3DMcpPlugin;

/// <summary>
/// Backend commands for Civil 3D catchment management.
/// Uses the verified Autodesk.Civil.DatabaseServices.Catchment class from AeccDbMgd.dll.
/// CatchmentGroup collection access uses reflection for forward-compatibility with 2023+ API.
/// </summary>
public static class CatchmentCommands
{
  // ──────────────────────────────────────────────
  //  List Catchment Groups
  // ──────────────────────────────────────────────
  public static Task<object?> ListCatchmentGroupsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groupIds = GetCatchmentGroupIds(civilDoc, database);
      var groups = new List<Dictionary<string, object?>>();

      foreach (var groupId in groupIds)
      {
        var groupObj = transaction.GetObject(groupId, OpenMode.ForRead);
        if (groupObj == null) continue;

        var name = CivilObjectUtils.GetName(groupObj) ?? "(unnamed)";
        var handle = CivilObjectUtils.GetHandle(groupObj);

        // Enumerate catchments in this group
        var catchmentIds = GetCatchmentIdsInGroup(groupObj);
        var catchmentNames = new List<string>();
        foreach (var cId in catchmentIds)
        {
          var cObj = transaction.GetObject(cId, OpenMode.ForRead);
          var cName = CivilObjectUtils.GetName(cObj);
          if (cName != null) catchmentNames.Add(cName);
        }

        groups.Add(new Dictionary<string, object?>
        {
          ["name"] = name,
          ["handle"] = handle,
          ["catchmentCount"] = catchmentNames.Count,
          ["catchmentNames"] = catchmentNames,
        });
      }

      return new Dictionary<string, object?>
      {
        ["groups"] = groups,
        ["totalGroups"] = groups.Count,
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Get Catchment Group (detailed)
  // ──────────────────────────────────────────────
  public static Task<object?> GetCatchmentGroupAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groupId = FindCatchmentGroupByName(civilDoc, database, transaction, groupName);
      var groupObj = transaction.GetObject(groupId, OpenMode.ForRead);
      var catchmentIds = GetCatchmentIdsInGroup(groupObj);

      var catchments = new List<Dictionary<string, object?>>();
      foreach (var cId in catchmentIds)
      {
        var catchment = CivilObjectUtils.GetRequiredObject<Catchment>(transaction, cId, OpenMode.ForRead);
        catchments.Add(ToCatchmentSummary(catchment));
      }

      return new Dictionary<string, object?>
      {
        ["groupName"] = CivilObjectUtils.GetName(groupObj),
        ["handle"] = CivilObjectUtils.GetHandle(groupObj!),
        ["catchments"] = catchments,
        ["catchmentCount"] = catchments.Count,
      };
    });
  }

  // ──────────────────────────────────────────────
  //  List All Catchments (across all groups)
  // ──────────────────────────────────────────────
  public static Task<object?> ListCatchmentsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groupIds = GetCatchmentGroupIds(civilDoc, database);
      var allCatchments = new List<Dictionary<string, object?>>();

      foreach (var groupId in groupIds)
      {
        var groupObj = transaction.GetObject(groupId, OpenMode.ForRead);
        var groupName = CivilObjectUtils.GetName(groupObj) ?? "(unnamed)";
        var catchmentIds = GetCatchmentIdsInGroup(groupObj);

        foreach (var cId in catchmentIds)
        {
          var catchment = CivilObjectUtils.GetRequiredObject<Catchment>(transaction, cId, OpenMode.ForRead);
          var summary = ToCatchmentSummary(catchment);
          summary["groupName"] = groupName;
          allCatchments.Add(summary);
        }
      }

      return new Dictionary<string, object?>
      {
        ["catchments"] = allCatchments,
        ["totalCatchments"] = allCatchments.Count,
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Get Catchment Properties (detailed)
  // ──────────────────────────────────────────────
  public static Task<object?> GetCatchmentPropertiesAsync(JsonObject? parameters)
  {
    var catchmentName = PluginRuntime.GetRequiredString(parameters, "catchmentName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var catchment = FindCatchmentByName(civilDoc, database, transaction, catchmentName, OpenMode.ForRead);
      return ToCatchmentDetail(catchment, transaction);
    });
  }

  // ──────────────────────────────────────────────
  //  Set Catchment Properties (edit)
  // ──────────────────────────────────────────────
  public static Task<object?> SetCatchmentPropertiesAsync(JsonObject? parameters)
  {
    var catchmentName = PluginRuntime.GetRequiredString(parameters, "catchmentName");

    // Optional editable properties
    var runoffCoefficient = PluginRuntime.GetOptionalDouble(parameters, "runoffCoefficient");
    var manningsCoefficient = PluginRuntime.GetOptionalDouble(parameters, "manningsCoefficient");
    var curveNumber = PluginRuntime.GetOptionalDouble(parameters, "curveNumber");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var newName = PluginRuntime.GetOptionalString(parameters, "newName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var catchment = FindCatchmentByName(civilDoc, database, transaction, catchmentName, OpenMode.ForWrite);
      var changes = new List<string>();

      if (runoffCoefficient.HasValue)
      {
        catchment.RunoffCoefficient = runoffCoefficient.Value;
        changes.Add($"RunoffCoefficient={runoffCoefficient.Value}");
      }

      if (manningsCoefficient.HasValue)
      {
        TrySetDeprecatedProperty(catchment, manningsCoefficient.Value, "ManningsCoefficient");
        changes.Add($"ManningsCoefficient={manningsCoefficient.Value}");
      }

      if (curveNumber.HasValue)
      {
        TrySetDeprecatedProperty(catchment, (int)Math.Round(curveNumber.Value), "CompositeCurveNumber", "CurveNumber");
        changes.Add($"CurveNumber={curveNumber.Value}");
      }

      if (!string.IsNullOrWhiteSpace(description))
      {
        catchment.Description = description;
        changes.Add($"Description={description}");
      }

      if (!string.IsNullOrWhiteSpace(newName))
      {
        catchment.Name = newName;
        changes.Add($"Name={newName}");
      }

      if (changes.Count == 0)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
          "setCatchmentProperties requires at least one property to change (runoffCoefficient, manningsCoefficient, curveNumber, description, newName).");
      }

      return new Dictionary<string, object?>
      {
        ["catchmentName"] = catchment.Name,
        ["handle"] = CivilObjectUtils.GetHandle(catchment),
        ["changes"] = changes,
        ["updatedProperties"] = ToCatchmentSummary(catchment),
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Copy Catchment To Group
  // ──────────────────────────────────────────────
  public static Task<object?> CopyCatchmentToGroupAsync(JsonObject? parameters)
  {
    var catchmentName = PluginRuntime.GetRequiredString(parameters, "catchmentName");
    var targetGroupName = PluginRuntime.GetRequiredString(parameters, "targetGroupName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var catchment = FindCatchmentByName(civilDoc, database, transaction, catchmentName, OpenMode.ForRead);
      var targetGroupId = FindCatchmentGroupByName(civilDoc, database, transaction, targetGroupName);

      // Catchment.CreateCopy is a static method: Catchment.CreateCopy(ObjectId catchmentId, ObjectId groupId)
      var newCatchmentId = Catchment.CreateCopy(catchment.ObjectId, targetGroupId);
      var newCatchment = CivilObjectUtils.GetRequiredObject<Catchment>(transaction, newCatchmentId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["sourceCatchment"] = catchmentName,
        ["targetGroup"] = targetGroupName,
        ["newCatchmentName"] = newCatchment.Name,
        ["newCatchmentHandle"] = CivilObjectUtils.GetHandle(newCatchment),
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Get Catchment Flow Path
  // ──────────────────────────────────────────────
  public static Task<object?> GetCatchmentFlowPathAsync(JsonObject? parameters)
  {
    var catchmentName = PluginRuntime.GetRequiredString(parameters, "catchmentName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var catchment = FindCatchmentByName(civilDoc, database, transaction, catchmentName, OpenMode.ForRead);

      var flowPath = CivilObjectUtils.GetPropertyValue<object>(catchment, "FlowPath")
        ?? CivilObjectUtils.InvokeMethod(catchment, "GetFlowPath");
      if (flowPath == null)
      {
        return new Dictionary<string, object?>
        {
          ["catchmentName"] = catchmentName,
          ["hasFlowPath"] = false,
          ["points"] = new List<object>(),
        };
      }

      // Extract flow path points via reflection since FlowPath might be an entity with geometry
      var points = new List<Dictionary<string, double>>();
      try
      {
        // Try to get points from the flow path object
        var pointsProp = flowPath.GetType().GetProperty("Points", BindingFlags.Public | BindingFlags.Instance);
        if (pointsProp != null)
        {
          var pointsCollection = pointsProp.GetValue(flowPath);
          if (pointsCollection is Point3dCollection pt3dCol)
          {
            foreach (Point3d pt in pt3dCol)
            {
              points.Add(new Dictionary<string, double>
              {
                ["x"] = pt.X,
                ["y"] = pt.Y,
                ["z"] = pt.Z,
              });
            }
          }
        }
      }
      catch
      {
        // Flow path geometry extraction failed — return empty
      }

      return new Dictionary<string, object?>
      {
        ["catchmentName"] = catchmentName,
        ["hasFlowPath"] = true,
        ["pointCount"] = points.Count,
        ["points"] = points,
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Get Catchment Boundary
  // ──────────────────────────────────────────────
  public static Task<object?> GetCatchmentBoundaryAsync(JsonObject? parameters)
  {
    var catchmentName = PluginRuntime.GetRequiredString(parameters, "catchmentName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var catchment = FindCatchmentByName(civilDoc, database, transaction, catchmentName, OpenMode.ForRead);

      var points2d = new List<Dictionary<string, double>>();
      try
      {
        var boundary2d = catchment.BoundaryPolyline2d;
        if (boundary2d != null)
        {
          foreach (Point2d pt in boundary2d)
          {
            points2d.Add(new Dictionary<string, double>
            {
              ["x"] = pt.X,
              ["y"] = pt.Y,
            });
          }
        }
      }
      catch
      {
        // Boundary extraction failed
      }

      var points3d = new List<Dictionary<string, double>>();
      try
      {
        var boundary3d = catchment.BoundaryPolyline3d;
        if (boundary3d != null)
        {
          foreach (Point3d pt in boundary3d)
          {
            points3d.Add(new Dictionary<string, double>
            {
              ["x"] = pt.X,
              ["y"] = pt.Y,
              ["z"] = pt.Z,
            });
          }
        }
      }
      catch
      {
        // 3D boundary extraction failed
      }

      return new Dictionary<string, object?>
      {
        ["catchmentName"] = catchmentName,
        ["boundary2dPointCount"] = points2d.Count,
        ["boundary2dPoints"] = points2d,
        ["boundary3dPointCount"] = points3d.Count,
        ["boundary3dPoints"] = points3d,
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Helpers
  // ──────────────────────────────────────────────

  /// <summary>
  /// Gets catchment group ObjectIds from CivilDocument.GetCatchmentGroups() via reflection
  /// for forward-compatibility. Falls back to Database extension method.
  /// </summary>
  private static List<ObjectId> GetCatchmentGroupIds(object civilDoc, Database database)
  {
    var ids = new List<ObjectId>();

    // Try CivilDocument.GetCatchmentGroups() (added in 2023 API)
    try
    {
      var method = civilDoc.GetType().GetMethod("GetCatchmentGroups", BindingFlags.Public | BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(civilDoc, null);
        if (result != null)
        {
          foreach (var id in CivilObjectUtils.ToObjectIds(result))
          {
            ids.Add(id);
          }
        }
      }
    }
    catch
    {
      // Method not available in this version
    }

    if (ids.Count > 0) return ids;

    // Fallback: try Database.GetCivilCatchmentGroups() extension method
    try
    {
      var extensionTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a =>
        {
          try { return a.GetTypes(); }
          catch { return Array.Empty<Type>(); }
        })
        .Where(t => t.IsClass && t.IsAbstract && t.IsSealed); // static classes

      foreach (var type in extensionTypes)
      {
        var extMethod = type.GetMethod("GetCivilCatchmentGroups",
          BindingFlags.Public | BindingFlags.Static,
          null,
          new[] { typeof(Database) },
          null);

        if (extMethod != null)
        {
          var result = extMethod.Invoke(null, new object[] { database });
          if (result != null)
          {
            foreach (var id in CivilObjectUtils.ToObjectIds(result))
            {
              ids.Add(id);
            }
          }
          break;
        }
      }
    }
    catch
    {
      // Extension method not available
    }

    return ids;
  }

  /// <summary>
  /// Gets catchment ObjectIds from a group object. Group typically has a method or
  /// property to enumerate its child catchment IDs.
  /// </summary>
  private static List<ObjectId> GetCatchmentIdsInGroup(AcDbObject? groupObj)
  {
    if (groupObj == null) return new List<ObjectId>();

    var ids = new List<ObjectId>();

    // Try GetCatchmentIds() method
    try
    {
      var method = groupObj.GetType().GetMethod("GetCatchmentIds", BindingFlags.Public | BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(groupObj, null);
        if (result != null)
        {
          foreach (var id in CivilObjectUtils.ToObjectIds(result))
          {
            ids.Add(id);
          }
        }
      }
    }
    catch { /* method not available */ }

    if (ids.Count > 0) return ids;

    // Try CatchmentIds property
    try
    {
      var prop = groupObj.GetType().GetProperty("CatchmentIds", BindingFlags.Public | BindingFlags.Instance);
      if (prop != null)
      {
        var result = prop.GetValue(groupObj);
        if (result != null)
        {
          foreach (var id in CivilObjectUtils.ToObjectIds(result))
          {
            ids.Add(id);
          }
        }
      }
    }
    catch { /* property not available */ }

    return ids;
  }

  private static ObjectId FindCatchmentGroupByName(object civilDoc, Database database, Transaction transaction, string name)
  {
    var groupIds = GetCatchmentGroupIds(civilDoc, database);
    foreach (var gId in groupIds)
    {
      var groupObj = transaction.GetObject(gId, OpenMode.ForRead);
      var gName = CivilObjectUtils.GetName(groupObj);
      if (string.Equals(gName, name, StringComparison.OrdinalIgnoreCase))
      {
        return gId;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Catchment group '{name}' was not found.");
  }

  private static Catchment FindCatchmentByName(object civilDoc, Database database, Transaction transaction, string name, OpenMode openMode)
  {
    var groupIds = GetCatchmentGroupIds(civilDoc, database);
    foreach (var gId in groupIds)
    {
      var groupObj = transaction.GetObject(gId, OpenMode.ForRead);
      var catchmentIds = GetCatchmentIdsInGroup(groupObj);

      foreach (var cId in catchmentIds)
      {
        var catchment = CivilObjectUtils.GetRequiredObject<Catchment>(transaction, cId, openMode);
        if (string.Equals(catchment.Name, name, StringComparison.OrdinalIgnoreCase))
        {
          return catchment;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Catchment '{name}' was not found in any group.");
  }

  private static Dictionary<string, object?> ToCatchmentSummary(Catchment catchment)
  {
    return new Dictionary<string, object?>
    {
      ["name"] = catchment.Name,
      ["handle"] = CivilObjectUtils.GetHandle(catchment),
      ["area2d"] = SafeGet(() => catchment.Area2d),
      ["perimeter2d"] = SafeGet(() => catchment.Perimeter2d),
      ["runoffCoefficient"] = SafeGet(() => catchment.RunoffCoefficient),
      ["curveNumber"] = SafeGetDouble(catchment, "CompositeCurveNumber", "CurveNumber"),
      ["timeOfConcentration"] = SafeGet(() => catchment.TimeOfConcentration),
      ["referenceSurfaceName"] = SafeGetStr(() => catchment.ReferenceSurfaceName),
    };
  }

  private static Dictionary<string, object?> ToCatchmentDetail(Catchment catchment, Transaction transaction)
  {
    var detail = ToCatchmentSummary(catchment);

    // Additional detailed properties
    detail["description"] = SafeGetStr(() => catchment.Description);
    detail["manningsCoefficient"] = SafeGetDouble(catchment, "ManningsCoefficient");
    detail["imperviousArea"] = SafeGet(() => catchment.ImperviousArea);
    detail["unconnectedImperviousArea"] = SafeGetDouble(catchment, "UnconnectedImperviousArea");
    detail["hydrologicalSoilGroup"] = SafeGetEnumText(catchment, "HydrologicalSoilGroup");
    detail["hydrologicallyMostDistantLength"] = SafeGetDouble(catchment, "FlowPathLength", "HydrologicallyMostDistantLength");
    detail["antecedentWetness"] = SafeGetEnumText(catchment, "AntecedentWetness");
    detail["exclusionary"] = SafeGet(() => catchment.Exclusionary);
    detail["referencePipeNetworkName"] = SafeGetStr(() => catchment.ReferencePipeNetworkName);
    detail["referencePipeNetworkStructureName"] = SafeGetStringProperty(catchment, "ReferenceDischargeObjectName", "ReferencePipeNetworkStructureName");
    detail["timeOfConcentrationCalculationMethod"] = SafeGetEnumText(catchment, "TimeOfConcentrationCalculationMethod");

    // Discharge point
    try
    {
      var dp = catchment.DischargePoint;
      detail["dischargePoint"] = new Dictionary<string, double>
      {
        ["x"] = dp.X,
        ["y"] = dp.Y,
        ["z"] = dp.Z,
      };
    }
    catch
    {
      detail["dischargePoint"] = null;
    }

    // Hydrologically most distant point
    try
    {
      var hmdp = catchment.HydrologicallyMostDistantPoint;
      detail["hydrologicallyMostDistantPoint"] = new Dictionary<string, double>
      {
        ["x"] = hmdp.X,
        ["y"] = hmdp.Y,
        ["z"] = hmdp.Z,
      };
    }
    catch
    {
      detail["hydrologicallyMostDistantPoint"] = null;
    }

    // Containing group name
    try
    {
      var groupId = catchment.ContainingGroupId;
      if (!groupId.IsNull)
      {
        var groupObj = transaction.GetObject(groupId, OpenMode.ForRead);
        detail["containingGroupName"] = CivilObjectUtils.GetName(groupObj);
      }
    }
    catch
    {
      detail["containingGroupName"] = null;
    }

    return detail;
  }

  private static double? SafeGet(Func<double> getter)
  {
    try { return getter(); }
    catch { return null; }
  }

  private static bool? SafeGet(Func<bool> getter)
  {
    try { return getter(); }
    catch { return null; }
  }

  private static string? SafeGetStr(Func<string?> getter)
  {
    try { return getter(); }
    catch { return null; }
  }

  private static double? SafeGetDouble(object target, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var value = CivilObjectUtils.GetDoubleProperty(target, propertyName);
      if (value.HasValue)
        return value.Value;
    }

    return null;
  }

  private static string? SafeGetStringProperty(object target, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var value = CivilObjectUtils.GetStringProperty(target, propertyName);
      if (!string.IsNullOrWhiteSpace(value))
        return value;
    }

    return null;
  }

  private static string? SafeGetEnumText(object target, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (property == null) continue;

      try
      {
        var value = property.GetValue(target);
        if (value != null)
          return value.ToString();
      }
      catch
      {
      }
    }

    return null;
  }

  private static void TrySetDeprecatedProperty(object target, object value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (property == null || !property.CanWrite) continue;

      try
      {
        var coerced = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(target, coerced);
        return;
      }
      catch
      {
      }
    }
  }
}
