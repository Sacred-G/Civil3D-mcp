using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class PointCommands
{
  public static Task<object?> ListCogoPointsAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetOptionalString(parameters, "groupName");
    var limit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? int.MaxValue;
    var offset = PluginRuntime.GetOptionalInt(parameters, "offset") ?? 0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      HashSet<uint>? allowedNumbers = null;
      if (!string.IsNullOrWhiteSpace(groupName))
      {
        allowedNumbers = TryGetPointNumbersForGroup(civilDoc, transaction, groupName!);
      }

      var allPoints = EnumeratePoints(civilDoc, transaction)
        .Where(p => allowedNumbers == null || allowedNumbers.Contains(p.PointNumber))
        .OrderBy(p => p.PointNumber)
        .ToList();

      var page = allPoints.Skip(offset).Take(limit).Select(CivilObjectUtils.ToPointData).ToList();

      return new Dictionary<string, object?>
      {
        ["totalCount"] = allPoints.Count,
        ["returnedCount"] = page.Count,
        ["points"] = page,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> GetCogoPointAsync(JsonObject? parameters)
  {
    var pointNumber = (uint)PluginRuntime.GetRequiredInt(parameters, "pointNumber");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var point = FindPointByNumber(civilDoc, transaction, pointNumber, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
      return CivilObjectUtils.ToPointData(point);
    });
  }

  public static Task<object?> CreateCogoPointsAsync(JsonObject? parameters)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createCogoPoints requires a non-empty 'points' array.");
    }

    var startNumber = PluginRuntime.GetOptionalInt(parameters, "startNumber");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var createdNumbers = new List<uint>();
      var collection = civilDoc.CogoPoints;

      for (var index = 0; index < pointsNode.Count; index++)
      {
        if (pointsNode[index] is not JsonObject pointNode)
        {
          continue;
        }

        var x = pointNode["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
        var y = pointNode["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
        var z = pointNode["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing z.");
        var description = pointNode["description"]?.GetValue<string?>() ?? string.Empty;

        var objectId = collection.Add(new Point3d(x, y, z), description, true);
        var point = CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);

        if (startNumber.HasValue)
        {
          point.PointNumber = (uint)(startNumber.Value + index);
        }

        createdNumbers.Add(point.PointNumber);
      }

      return new Dictionary<string, object?>
      {
        ["created"] = createdNumbers.Count,
        ["pointNumbers"] = createdNumbers,
      };
    });
  }

  public static Task<object?> ImportCogoPointsAsync(JsonObject? parameters)
  {
    var format = PluginRuntime.GetRequiredString(parameters, "format");
    var data = PluginRuntime.GetRequiredString(parameters, "data");
    var targetSurface = PluginRuntime.GetOptionalString(parameters, "targetSurface");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var createdNumbers = new List<uint>();
      foreach (var parsed in ParsePointImport(data, format))
      {
        var objectId = civilDoc.CogoPoints.Add(new Point3d(parsed.X, parsed.Y, parsed.Z), parsed.Description, true);
        var point = CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
        createdNumbers.Add(point.PointNumber);
      }

      return new Dictionary<string, object?>
      {
        ["imported"] = createdNumbers.Count,
        ["pointNumbers"] = createdNumbers,
        ["targetSurface"] = targetSurface,
        ["warning"] = string.IsNullOrWhiteSpace(targetSurface) ? null : "targetSurface was accepted but surface population is not yet implemented in this plugin.",
      };
    });
  }

  public static Task<object?> DeleteCogoPointsAsync(JsonObject? parameters)
  {
    var numbersNode = PluginRuntime.GetParameter(parameters, "pointNumbers") as JsonArray;
    if (numbersNode == null || numbersNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "deleteCogoPoints requires 'pointNumbers'.");
    }

    var pointNumbers = numbersNode.Select(node => (uint)(node?.GetValue<int>() ?? 0)).Where(n => n > 0).ToList();

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var deleted = new List<uint>();
      foreach (var pointNumber in pointNumbers)
      {
        var point = FindPointByNumber(civilDoc, transaction, pointNumber, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
        point.Erase();
        deleted.Add(pointNumber);
      }

      return new Dictionary<string, object?>
      {
        ["deleted"] = deleted.Count,
        ["pointNumbers"] = deleted,
      };
    });
  }

  public static Task<object?> ListPointGroupsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groups = new List<Dictionary<string, object?>>();
      var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
      var pointGroups = pointGroupsProperty?.GetValue(civilDoc);

      foreach (var objectId in CivilObjectUtils.ToObjectIds(pointGroups))
      {
        var groupObject = transaction.GetObject(objectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
        if (groupObject == null)
        {
          continue;
        }

        var numbers = CivilObjectUtils.InvokeMethod(groupObject, "GetPointNumbers") as IEnumerable<uint>;
        groups.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(groupObject),
          ["description"] = CivilObjectUtils.GetStringProperty(groupObject, "Description"),
          ["pointCount"] = numbers?.Count() ?? 0,
          ["includePattern"] = CivilObjectUtils.GetStringProperty(groupObject, "IncludeNumbers"),
          ["excludePattern"] = CivilObjectUtils.GetStringProperty(groupObject, "ExcludeNumbers"),
        });
      }

      return new Dictionary<string, object?>
      {
        ["groups"] = groups,
      };
    });
  }

  private static IEnumerable<CogoPoint> EnumeratePoints(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Autodesk.AutoCAD.DatabaseServices.Transaction transaction)
  {
    foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId objectId in civilDoc.CogoPoints)
    {
      yield return CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
    }
  }

  private static CogoPoint FindPointByNumber(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Autodesk.AutoCAD.DatabaseServices.Transaction transaction, uint pointNumber, Autodesk.AutoCAD.DatabaseServices.OpenMode openMode)
  {
    foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId objectId in civilDoc.CogoPoints)
    {
      var point = CivilObjectUtils.GetRequiredObject<CogoPoint>(transaction, objectId, openMode);
      if (point.PointNumber == pointNumber)
      {
        return point;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"COGO point '{pointNumber}' was not found.");
  }

  private static HashSet<uint>? TryGetPointNumbersForGroup(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Autodesk.AutoCAD.DatabaseServices.Transaction transaction, string groupName)
  {
    var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
    var pointGroups = pointGroupsProperty?.GetValue(civilDoc);

    foreach (var objectId in CivilObjectUtils.ToObjectIds(pointGroups))
    {
      var groupObject = transaction.GetObject(objectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
      if (!string.Equals(CivilObjectUtils.GetName(groupObject), groupName, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      var numbers = CivilObjectUtils.InvokeMethod(groupObject, "GetPointNumbers") as IEnumerable<uint>;
      return numbers == null ? null : new HashSet<uint>(numbers);
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Point group '{groupName}' was not found.");
  }

  private static IEnumerable<(double X, double Y, double Z, string Description)> ParsePointImport(string data, string format)
  {
    foreach (var rawLine in data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
    {
      var line = rawLine.Trim();
      if (line.Length == 0)
      {
        continue;
      }

      var tokens = line.Split(new[] { ',', ';', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
      yield return format switch
      {
        "pnezd" when tokens.Length >= 5 => (double.Parse(tokens[1]), double.Parse(tokens[2]), double.Parse(tokens[3]), tokens[4]),
        "penz" when tokens.Length >= 5 => (double.Parse(tokens[1]), double.Parse(tokens[3]), double.Parse(tokens[2]), tokens[4]),
        "xyzd" when tokens.Length >= 4 => (double.Parse(tokens[0]), double.Parse(tokens[1]), double.Parse(tokens[2]), tokens[3]),
        "xyz" when tokens.Length >= 3 => (double.Parse(tokens[0]), double.Parse(tokens[1]), double.Parse(tokens[2]), string.Empty),
        _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unsupported point import line for format '{format}': {line}"),
      };
    }
  }
}
