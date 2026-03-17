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

  public static Task<object?> AddSurfacePointsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var points = ParseRequiredPoint3dArray(parameters, "points", "addSurfacePoints requires at least one point.");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddPoints", points)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddVertices", points))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add points to surface '{name}' with the available Civil 3D API overloads.");
      }

      SetSurfaceDescription(surface, description);
      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["pointsAdded"] = points.Count,
        ["description"] = surface.Description,
      };
    });
  }

  public static Task<object?> AddSurfaceBreaklineAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var breaklineType = PluginRuntime.GetOptionalString(parameters, "breaklineType") ?? "standard";
    var points = ParseRequiredPoint3dArray(parameters, "points", "addSurfaceBreakline requires at least two points.");
    if (points.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "addSurfaceBreakline requires at least two points.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddBreaklines", points, 1.0, 0.0, 0.0, 0.0)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddStandardBreaklines", points, 1.0, 0.0, 0.0, 0.0))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add breakline to surface '{name}' with the available Civil 3D API overloads.");
      }

      SetSurfaceDescription(surface, description);
      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["breaklineType"] = breaklineType,
        ["vertexCount"] = points.Count,
        ["description"] = surface.Description,
      };
    });
  }

  public static Task<object?> AddSurfaceBoundaryAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var boundaryType = PluginRuntime.GetOptionalString(parameters, "boundaryType") ?? "outer";
    var points = ParseRequiredPoint2dArray(parameters, "points", "addSurfaceBoundary requires at least three points.");
    if (points.Count < 3)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "addSurfaceBoundary requires at least three points.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);
      var boundaryTypeValue = ResolveBoundaryType(definition, boundaryType);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddBoundaries", points, boundaryTypeValue, true)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddBoundary", points, boundaryTypeValue, true))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add boundary to surface '{name}' with the available Civil 3D API overloads.");
      }

      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["boundaryType"] = boundaryType,
        ["vertexCount"] = points.Count,
      };
    });
  }

  public static Task<object?> ExtractSurfaceContoursAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var minorInterval = PluginRuntime.GetRequiredDouble(parameters, "minorInterval");
    var majorInterval = PluginRuntime.GetRequiredDouble(parameters, "majorInterval");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var extracted = ExtractContourEntities(surface, minorInterval, majorInterval);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["minorInterval"] = minorInterval,
        ["majorInterval"] = majorInterval,
        ["contourCount"] = extracted.Count,
        ["handles"] = extracted
          .Select(objectId => CivilObjectUtils.GetHandle(CivilObjectUtils.GetRequiredObject<Entity>(transaction, objectId, OpenMode.ForRead)))
          .ToList(),
      };
    });
  }

  public static Task<object?> ComputeSurfaceVolumeAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var comparisonSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);
      var volumeProperties = GetVolumeProperties(civilDoc, transaction, baseSurface, comparisonSurface);

      return new Dictionary<string, object?>
      {
        ["cutVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutVolume") ?? 0d,
        ["fillVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillVolume") ?? 0d,
        ["netVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "NetVolume") ?? 0d,
        ["cutArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutArea") ?? 0d,
        ["fillArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillArea") ?? 0d,
        ["units"] = new Dictionary<string, object?>
        {
          ["volume"] = $"{CivilObjectUtils.LinearUnits(database)}^3",
          ["area"] = $"{CivilObjectUtils.LinearUnits(database)}^2",
        },
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

  private static object GetSurfaceDefinition(Surface surface)
  {
    foreach (var propertyName in new[] { "Definition", "Operations", "BoundariesDefinition" })
    {
      var value = CivilObjectUtils.GetPropertyValue<object>(surface, propertyName);
      if (value != null)
      {
        return value;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface definition API is not available for surface '{surface.Name}'.");
  }

  private static Point3dCollection ParseRequiredPoint3dArray(JsonObject? parameters, string name, string errorMessage)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, name) as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    var points = new Point3dCollection();
    foreach (var pointNode in pointsNode)
    {
      if (pointNode is not JsonObject point)
      {
        continue;
      }

      var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
      var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
      var z = point["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing z.");
      points.Add(new Point3d(x, y, z));
    }

    if (points.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    return points;
  }

  private static Point2dCollection ParseRequiredPoint2dArray(JsonObject? parameters, string name, string errorMessage)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, name) as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    var points = new Point2dCollection();
    foreach (var pointNode in pointsNode)
    {
      if (pointNode is not JsonObject point)
      {
        continue;
      }

      var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
      var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
      points.Add(new Point2d(x, y));
    }

    if (points.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    return points;
  }

  private static bool TryInvokeSurfaceDefinitionMethod(object target, string methodName, params object?[] preferredArguments)
  {
    var methods = target.GetType()
      .GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name == methodName)
      .OrderByDescending(method => method.GetParameters().Length);

    foreach (var method in methods)
    {
      var arguments = BuildInvocationArguments(method.GetParameters(), preferredArguments);
      if (arguments == null)
      {
        continue;
      }

      try
      {
        method.Invoke(target, arguments);
        return true;
      }
      catch
      {
      }
    }

    return false;
  }

  private static object?[]? BuildInvocationArguments(ParameterInfo[] parameters, object?[] preferredArguments)
  {
    var remaining = new List<object?>(preferredArguments);
    var result = new object?[parameters.Length];

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameter = parameters[index];
      var matchIndex = remaining.FindIndex(candidate => IsCompatibleArgument(parameter.ParameterType, candidate));
      if (matchIndex >= 0)
      {
        result[index] = remaining[matchIndex];
        remaining.RemoveAt(matchIndex);
        continue;
      }

      if (TryCreateDefaultArgument(parameter.ParameterType, out var defaultValue))
      {
        result[index] = defaultValue;
        continue;
      }

      return null;
    }

    return result;
  }

  private static bool IsCompatibleArgument(Type parameterType, object? candidate)
  {
    if (candidate == null)
    {
      return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
    }

    if (parameterType.IsInstanceOfType(candidate))
    {
      return true;
    }

    var underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
    if (underlyingType.IsEnum && candidate is string stringValue)
    {
      return Enum.GetNames(underlyingType).Any(name => string.Equals(name, stringValue, StringComparison.OrdinalIgnoreCase));
    }

    return underlyingType == typeof(double) && candidate is double
      || underlyingType == typeof(bool) && candidate is bool
      || underlyingType == typeof(int) && candidate is int;
  }

  private static bool TryCreateDefaultArgument(Type parameterType, out object? value)
  {
    var underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

    if (underlyingType == typeof(double))
    {
      value = 0d;
      return true;
    }

    if (underlyingType == typeof(int))
    {
      value = 0;
      return true;
    }

    if (underlyingType == typeof(bool))
    {
      value = false;
      return true;
    }

    if (underlyingType.IsEnum)
    {
      value = Enum.GetValues(underlyingType).GetValue(0);
      return true;
    }

    value = null;
    return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
  }

  private static object ResolveBoundaryType(object definition, string boundaryType)
  {
    var enumType = definition.GetType().Assembly
      .GetTypes()
      .FirstOrDefault(type => type.IsEnum && type.Name.Contains("SurfaceBoundaryType", StringComparison.OrdinalIgnoreCase));

    if (enumType == null)
    {
      return boundaryType;
    }

    foreach (var enumName in Enum.GetNames(enumType))
    {
      if (NormalizeBoundaryType(enumName) == NormalizeBoundaryType(boundaryType))
      {
        return Enum.Parse(enumType, enumName);
      }
    }

    return Enum.GetValues(enumType).GetValue(0)!;
  }

  private static string NormalizeBoundaryType(string value)
  {
    return value.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
  }

  private static void SetSurfaceDescription(Surface surface, string? description)
  {
    if (!string.IsNullOrWhiteSpace(description))
    {
      surface.Description = description;
    }
  }

  private static void RebuildSurfaceIfAvailable(Surface surface)
  {
    _ = CivilObjectUtils.InvokeMethod(surface, "Rebuild");
  }

  private static List<ObjectId> ExtractContourEntities(Surface surface, double minorInterval, double majorInterval)
  {
    foreach (var methodName in new[] { "ExtractContours", "ExtractContour" })
    {
      var result = CivilObjectUtils.InvokeMethod(surface, methodName, minorInterval, majorInterval);
      var objectIds = CivilObjectUtils.ToObjectIds(result).ToList();
      if (objectIds.Count > 0)
      {
        return objectIds;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to extract contours for surface '{surface.Name}' with the available Civil 3D API overloads.");
  }

  private static object GetVolumeProperties(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction, Surface baseSurface, Surface comparisonSurface)
  {
    var directProperties = CivilObjectUtils.InvokeMethod(baseSurface, "GetVolumeProperties", comparisonSurface);
    if (directProperties != null)
    {
      return directProperties;
    }

    var volumeSurface = CreateTinVolumeSurface(civilDoc, transaction, baseSurface, comparisonSurface);
    var properties = CivilObjectUtils.InvokeMethod(volumeSurface, "GetVolumeProperties");
    if (properties != null)
    {
      return properties;
    }

    return volumeSurface;
  }

  private static object CreateTinVolumeSurface(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction, Surface baseSurface, Surface comparisonSurface)
  {
    var type = typeof(TinVolumeSurface);
    var surfaceId = ObjectId.Null;
    var name = $"{baseSurface.Name}_{comparisonSurface.Name}_Volume";
    var styleId = LookupUtils.GetSurfaceStyleId(civilDoc, transaction, null);

    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Where(method => method.Name == "Create")
      .OrderByDescending(method => method.GetParameters().Length);

    foreach (var method in methods)
    {
      var arguments = BuildCreateTinVolumeSurfaceArguments(method.GetParameters(), civilDoc, name, baseSurface.ObjectId, comparisonSurface.ObjectId, styleId);
      if (arguments == null)
      {
        continue;
      }

      try
      {
        surfaceId = (ObjectId)method.Invoke(null, arguments)!;
        break;
      }
      catch
      {
      }
    }

    if (surfaceId == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to compute volume surface with the available Civil 3D API overloads.");
    }

    return CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, surfaceId, OpenMode.ForRead);
  }

  private static object?[]? BuildCreateTinVolumeSurfaceArguments(ParameterInfo[] parameters, Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, string name, ObjectId baseSurfaceId, ObjectId comparisonSurfaceId, ObjectId styleId)
  {
    var available = new List<object?> { civilDoc, name, baseSurfaceId, comparisonSurfaceId, styleId };
    var result = new object?[parameters.Length];

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameter = parameters[index];
      var matchIndex = available.FindIndex(candidate => IsCompatibleArgument(parameter.ParameterType, candidate));
      if (matchIndex >= 0)
      {
        result[index] = available[matchIndex];
        available.RemoveAt(matchIndex);
        continue;
      }

      if (TryCreateDefaultArgument(parameter.ParameterType, out var defaultValue))
      {
        result[index] = defaultValue;
        continue;
      }

      return null;
    }

    return result;
  }
}
