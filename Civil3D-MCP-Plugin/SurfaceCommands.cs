using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class SurfaceCommands
{
  public static Task<object?> ListSurfacesAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surfaces = civilDoc.GetSurfaceIds()
        .Cast<ObjectId>()
        .Select(id => CivilObjectUtils.GetRequiredObject<Surface>(transaction, id, OpenMode.ForRead))
        .Select(surface => new Dictionary<string, object?>
        {
          ["name"] = surface.Name,
          ["handle"] = CivilObjectUtils.GetHandle(surface),
          ["type"] = MapSurfaceType(surface),
          ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(surface, "IsReferenceObject") ?? false,
          ["sourcePath"] = CivilObjectUtils.GetStringProperty(surface, "ReferencePath"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["surfaces"] = surfaces,
      };
    });
  }

  public static Task<object?> GetSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var extents = CivilObjectUtils.GetPropertyValue<Extents3d?>(surface, "GeometricExtents");

      return new Dictionary<string, object?>
      {
        ["name"] = surface.Name,
        ["handle"] = CivilObjectUtils.GetHandle(surface),
        ["type"] = MapSurfaceType(surface),
        ["style"] = CivilObjectUtils.GetName(transaction.GetObject(surface.StyleId, OpenMode.ForRead)) ?? string.Empty,
        ["layer"] = surface.Layer,
        ["statistics"] = new Dictionary<string, object?>
        {
          ["minimumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0,
          ["maximumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0,
          ["meanElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MeanElevation") ?? 0,
          ["area2d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 0,
          ["area3d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area3D") ?? 0,
          ["numberOfPoints"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0,
          ["numberOfTriangles"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0,
        },
        ["boundingBox"] = new Dictionary<string, object?>
        {
          ["minX"] = extents?.MinPoint.X ?? 0,
          ["minY"] = extents?.MinPoint.Y ?? 0,
          ["maxX"] = extents?.MaxPoint.X ?? 0,
          ["maxY"] = extents?.MaxPoint.Y ?? 0,
        },
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(surface, "IsReferenceObject") ?? false,
        ["dependentAlignments"] = new List<string>(),
        ["dependentCorridors"] = new List<string>(),
      };
    });
  }

  public static Task<object?> GetSurfaceElevationAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var elevation = InvokeSurfaceElevation(surface, x, y);
      return new Dictionary<string, object?>
      {
        ["elevation"] = elevation,
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["surfaceName"] = surface.Name,
      };
    });
  }

  public static Task<object?> GetSurfaceElevationsAlongAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "getSurfaceElevationsAlong requires 'points'.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var samples = new List<Dictionary<string, object?>>();
      foreach (var point in pointsNode.OfType<JsonObject>())
      {
        var x = point["x"]!.GetValue<double>();
        var y = point["y"]!.GetValue<double>();
        samples.Add(new Dictionary<string, object?>
        {
          ["x"] = x,
          ["y"] = y,
          ["elevation"] = InvokeSurfaceElevation(surface, x, y),
        });
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["samples"] = samples,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> GetSurfaceStatisticsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var analysisType = PluginRuntime.GetOptionalString(parameters, "analysisType");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["analysisType"] = analysisType,
        ["minimumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0,
        ["maximumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0,
        ["meanElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MeanElevation") ?? 0,
        ["area2d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 0,
        ["area3d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area3D") ?? 0,
        ["numberOfPoints"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0,
        ["numberOfTriangles"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0,
      };
    });
  }

  public static Task<object?> CreateSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styleId = LookupUtils.GetSurfaceStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var surfaceId = CreateTinSurface(civilDoc, name, styleId);
      var surface = CivilObjectUtils.GetRequiredObject<Surface>(transaction, surfaceId, OpenMode.ForWrite);
      var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
      if (!string.IsNullOrWhiteSpace(layerName))
      {
        surface.Layer = layerName;
      }

      var description = PluginRuntime.GetOptionalString(parameters, "description");
      if (!string.IsNullOrWhiteSpace(description))
      {
        surface.Description = description;
      }

      return new Dictionary<string, object?>
      {
        ["name"] = surface.Name,
        ["handle"] = CivilObjectUtils.GetHandle(surface),
        ["created"] = true,
      };
    });
  }

  public static Task<object?> DeleteSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      surface.Erase();
      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      };
    });
  }

  private static string MapSurfaceType(Surface surface)
  {
    return surface switch
    {
      TinVolumeSurface => "TINVolume",
      GridSurface => "Grid",
      _ => "TIN",
    };
  }

  private static double InvokeSurfaceElevation(Surface surface, double x, double y)
  {
    foreach (var methodName in new[] { "FindElevationAtXY", "GetElevationAtXY" })
    {
      var value = CivilObjectUtils.InvokeMethod(surface, methodName, x, y);
      if (value != null)
      {
        return Convert.ToDouble(value);
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface elevation API is not available for surface '{surface.Name}'.");
  }

  private static ObjectId CreateTinSurface(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, string name, ObjectId styleId)
  {
    var methods = typeof(TinSurface)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Where(m => m.Name == "Create")
      .OrderByDescending(m => m.GetParameters().Length)
      .ToList();

    foreach (var method in methods)
    {
      var parameters = method.GetParameters();
      try
      {
        if (parameters.Length == 3 && parameters[0].ParameterType.Name == "CivilDocument")
        {
          return (ObjectId)method.Invoke(null, new object?[] { civilDoc, name, styleId })!;
        }

        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(ObjectId))
        {
          return (ObjectId)method.Invoke(null, new object?[] { name, styleId })!;
        }

        if (parameters.Length == 2 && parameters[0].ParameterType.Name == "CivilDocument")
        {
          return (ObjectId)method.Invoke(null, new object?[] { civilDoc, name })!;
        }

        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
        {
          return (ObjectId)method.Invoke(null, new object?[] { name })!;
        }
      }
      catch
      {
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to create a TIN surface with the available Civil 3D API overloads.");
  }
}
