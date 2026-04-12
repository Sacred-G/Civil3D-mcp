using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for Civil 3D Point Group management and point export/transform.
///
/// Civil 3D API notes (reflection-based late binding):
///   AeccPointGroup  -- manages a filtered/ordered set of COGO points
///
/// Accessed via reflection to avoid a hard dependency on the exact AeccDbMgd version.
/// </summary>
public static class PointGroupCommands
{
  // -------------------------------------------------------------------------
  // createPointGroup
  // -------------------------------------------------------------------------

  public static Task<object?> CreatePointGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description") ?? string.Empty;
    var includeNumbers = PluginRuntime.GetOptionalString(parameters, "includeNumbers");
    var excludeNumbers = PluginRuntime.GetOptionalString(parameters, "excludeNumbers");
    var includeDescriptions = PluginRuntime.GetOptionalString(parameters, "includeDescriptions");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
      var pointGroups = pointGroupsProperty?.GetValue(civilDoc);

      var addMethod = pointGroups?.GetType().GetMethod("Add", new[] { typeof(string) });
      if (addMethod == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "PointGroups.Add(name) method not found.");
      }

      var newGroupId = (ObjectId)(addMethod.Invoke(pointGroups, new object[] { name })
        ?? throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Failed to create point group."));

      var newGroup = transaction.GetObject(newGroupId, OpenMode.ForWrite);

      // Set optional properties
      TrySetProperty(newGroup, "Description", description);
      if (includeNumbers != null) TrySetProperty(newGroup, "IncludeNumbers", includeNumbers);
      if (excludeNumbers != null) TrySetProperty(newGroup, "ExcludeNumbers", excludeNumbers);
      if (includeDescriptions != null) TrySetProperty(newGroup, "IncludeDescriptions", includeDescriptions);

      // Force update
      CivilObjectUtils.InvokeMethod(newGroup, "Update");

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["handle"] = CivilObjectUtils.GetHandle(newGroup),
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // updatePointGroup
  // -------------------------------------------------------------------------

  public static Task<object?> UpdatePointGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var includeNumbers = PluginRuntime.GetOptionalString(parameters, "includeNumbers");
    var excludeNumbers = PluginRuntime.GetOptionalString(parameters, "excludeNumbers");
    var includeDescriptions = PluginRuntime.GetOptionalString(parameters, "includeDescriptions");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindPointGroupByName(civilDoc, transaction, name, OpenMode.ForWrite);

      if (description != null) TrySetProperty(group, "Description", description);
      if (includeNumbers != null) TrySetProperty(group, "IncludeNumbers", includeNumbers);
      if (excludeNumbers != null) TrySetProperty(group, "ExcludeNumbers", excludeNumbers);
      if (includeDescriptions != null) TrySetProperty(group, "IncludeDescriptions", includeDescriptions);

      CivilObjectUtils.InvokeMethod(group, "Update");

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // deletePointGroup
  // -------------------------------------------------------------------------

  public static Task<object?> DeletePointGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindPointGroupByName(civilDoc, transaction, name, OpenMode.ForWrite);
      group.Erase();

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // exportCogoPoints
  // -------------------------------------------------------------------------

  public static Task<object?> ExportCogoPointsAsync(JsonObject? parameters)
  {
    var format = PluginRuntime.GetOptionalString(parameters, "format") ?? "pnezd";
    var groupName = PluginRuntime.GetOptionalString(parameters, "groupName");
    var numbersNode = PluginRuntime.GetParameter(parameters, "pointNumbers") as JsonArray;
    var delimiter = PluginRuntime.GetOptionalString(parameters, "delimiter") ?? ",";

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      HashSet<uint>? allowedNumbers = null;

      if (!string.IsNullOrWhiteSpace(groupName))
      {
        var group = FindPointGroupByName(civilDoc, transaction, groupName!, OpenMode.ForRead);
        var numbers = CivilObjectUtils.InvokeMethod(group, "GetPointNumbers") as IEnumerable<uint>;
        allowedNumbers = numbers == null ? new HashSet<uint>() : new HashSet<uint>(numbers);
      }

      if (numbersNode != null && numbersNode.Count > 0)
      {
        var explicit_ = new HashSet<uint>(numbersNode.Select(n => (uint)(n?.GetValue<int>() ?? 0)).Where(n => n > 0));
        allowedNumbers = allowedNumbers == null ? explicit_ : new HashSet<uint>(allowedNumbers.Intersect(explicit_));
      }

      var sb = new StringBuilder();
      var exportedCount = 0;

      foreach (ObjectId objectId in civilDoc.CogoPoints)
      {
        var point = CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, OpenMode.ForRead);
        if (allowedNumbers != null && !allowedNumbers.Contains(point.PointNumber)) continue;

        var line = format switch
        {
          "pnezd" => $"{point.PointNumber}{delimiter}{point.Location.X}{delimiter}{point.Location.Y}{delimiter}{point.Location.Z}{delimiter}{point.RawDescription}",
          "penz" => $"{point.PointNumber}{delimiter}{point.Location.X}{delimiter}{point.Location.Z}{delimiter}{point.Location.Y}{delimiter}{point.RawDescription}",
          "xyzd" => $"{point.Location.X}{delimiter}{point.Location.Y}{delimiter}{point.Location.Z}{delimiter}{point.RawDescription}",
          "xyz" => $"{point.Location.X}{delimiter}{point.Location.Y}{delimiter}{point.Location.Z}",
          "csv" => $"{point.PointNumber}{delimiter}{point.PointName}{delimiter}{point.Location.X}{delimiter}{point.Location.Y}{delimiter}{point.Location.Z}{delimiter}{point.RawDescription}",
          _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unsupported export format: {format}"),
        };

        sb.AppendLine(line);
        exportedCount++;
      }

      return new Dictionary<string, object?>
      {
        ["format"] = format,
        ["exportedCount"] = exportedCount,
        ["data"] = sb.ToString(),
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // transformCogoPoints
  // -------------------------------------------------------------------------

  public static Task<object?> TransformCogoPointsAsync(JsonObject? parameters)
  {
    var numbersNode = PluginRuntime.GetParameter(parameters, "pointNumbers") as JsonArray;
    var translateX = PluginRuntime.GetOptionalDouble(parameters, "translateX") ?? 0;
    var translateY = PluginRuntime.GetOptionalDouble(parameters, "translateY") ?? 0;
    var translateZ = PluginRuntime.GetOptionalDouble(parameters, "translateZ") ?? 0;
    var rotateRadians = PluginRuntime.GetOptionalDouble(parameters, "rotateRadians") ?? 0;
    var scaleFactor = PluginRuntime.GetOptionalDouble(parameters, "scaleFactor") ?? 1.0;
    var groupName = PluginRuntime.GetOptionalString(parameters, "groupName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      HashSet<uint>? targetNumbers = null;

      if (numbersNode != null && numbersNode.Count > 0)
      {
        targetNumbers = new HashSet<uint>(numbersNode.Select(n => (uint)(n?.GetValue<int>() ?? 0)).Where(n => n > 0));
      }
      else if (!string.IsNullOrWhiteSpace(groupName))
      {
        var group = FindPointGroupByName(civilDoc, transaction, groupName!, OpenMode.ForRead);
        var numbers = CivilObjectUtils.InvokeMethod(group, "GetPointNumbers") as IEnumerable<uint>;
        targetNumbers = numbers == null ? new HashSet<uint>() : new HashSet<uint>(numbers);
      }

      var transformedCount = 0;
      var sinRot = Math.Sin(rotateRadians);
      var cosRot = Math.Cos(rotateRadians);

      foreach (ObjectId objectId in civilDoc.CogoPoints)
      {
        var point = CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, OpenMode.ForRead);
        if (targetNumbers != null && !targetNumbers.Contains(point.PointNumber)) continue;

        var writablePoint = transaction.GetObject(objectId, OpenMode.ForWrite) as CogoPoint;
        if (writablePoint == null) continue;

        var x = writablePoint.Location.X;
        var y = writablePoint.Location.Y;
        var z = writablePoint.Location.Z;

        // Scale around origin
        x *= scaleFactor;
        y *= scaleFactor;

        // Rotate around origin
        if (rotateRadians != 0)
        {
          var rx = x * cosRot - y * sinRot;
          var ry = x * sinRot + y * cosRot;
          x = rx;
          y = ry;
        }

        // Translate
        x += translateX;
        y += translateY;
        z += translateZ;

        if (TrySetPointCoordinate(writablePoint, x, y, z))
        {
          transformedCount++;
        }
      }

      return new Dictionary<string, object?>
      {
        ["transformedCount"] = transformedCount,
        ["translateX"] = translateX,
        ["translateY"] = translateY,
        ["translateZ"] = translateZ,
        ["rotateRadians"] = rotateRadians,
        ["scaleFactor"] = scaleFactor,
      };
    });
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private static AcDbObject FindPointGroupByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name,
    OpenMode mode)
  {
    var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
    var pointGroups = pointGroupsProperty?.GetValue(civilDoc);

    foreach (var objectId in CivilObjectUtils.ToObjectIds(pointGroups))
    {
      var groupObject = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(groupObject), name, StringComparison.OrdinalIgnoreCase))
      {
        return mode == OpenMode.ForWrite
          ? transaction.GetObject(objectId, OpenMode.ForWrite)
          : groupObject;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Point group '{name}' was not found.");
  }

  private static void TrySetProperty(AcDbObject obj, string propertyName, object value)
  {
    var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    try { prop?.SetValue(obj, value); } catch { /* ignore */ }
  }

  private static bool TrySetPointCoordinate(CogoPoint point, double x, double y, double z)
  {
    if (TrySetCoordinateProperty(point, x, "Easting", "X")
      & TrySetCoordinateProperty(point, y, "Northing", "Y")
      & TrySetCoordinateProperty(point, z, "Elevation", "Z"))
    {
      return true;
    }

    return CivilObjectUtils.InvokeMethod(point, "MoveTo", new Point3d(x, y, z)) != null
      || CivilObjectUtils.InvokeMethod(point, "SetLocation", new Point3d(x, y, z)) != null;
  }

  private static bool TrySetCoordinateProperty(CogoPoint point, double value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var prop = point.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || !prop.CanWrite) continue;

      try
      {
        prop.SetValue(point, value);
        return true;
      }
      catch
      {
      }
    }

    return false;
  }
}
