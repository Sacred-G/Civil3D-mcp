using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

public static class PipeNetworkCommands
{
  public static Task<object?> ListPipeNetworksAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networks = EnumeratePipeNetworks(civilDoc, transaction, OpenMode.ForRead)
        .Select(network => ToPipeNetworkSummary(network, transaction))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["networks"] = networks,
      };
    });
  }

  public static Task<object?> GetPipeNetworkAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, name, OpenMode.ForRead);
      return ToPipeNetworkDetail(network, transaction);
    });
  }

  public static Task<object?> GetPipeAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var pipeName = PluginRuntime.GetRequiredString(parameters, "pipeName");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      var pipe = FindPipeByName(network, transaction, pipeName, OpenMode.ForRead);
      return ToPipeData(pipe, transaction);
    });
  }

  public static Task<object?> GetStructureAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var structureName = PluginRuntime.GetRequiredString(parameters, "structureName");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      var structure = FindStructureByName(network, transaction, structureName, OpenMode.ForRead);
      return ToStructureData(structure, transaction);
    });
  }

  public static Task<object?> CheckPipeNetworkInterferenceAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var targetType = PluginRuntime.GetRequiredString(parameters, "targetType");
    var targetName = PluginRuntime.GetRequiredString(parameters, "targetName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      _ = FindPipeNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      if (string.Equals(targetType, "surface", StringComparison.OrdinalIgnoreCase))
      {
        _ = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, targetName, OpenMode.ForRead);
      }
      else if (string.Equals(targetType, "pipe_network", StringComparison.OrdinalIgnoreCase))
      {
        _ = FindPipeNetworkByName(civilDoc, transaction, targetName, OpenMode.ForRead);
      }
      else
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unsupported targetType '{targetType}'.");
      }

      return new Dictionary<string, object?>
      {
        ["interferences"] = Array.Empty<object>(),
        ["totalConflicts"] = 0,
      };
    });
  }

  public static Task<object?> CreatePipeNetworkAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var partsListName = PluginRuntime.GetRequiredString(parameters, "partsList");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networkId = CreatePipeNetwork(civilDoc, name);
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, networkId, OpenMode.ForWrite);

      var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
      if (!string.IsNullOrWhiteSpace(layerName) && network is Entity entity)
      {
        entity.Layer = layerName;
      }

      var partsListId = FindPartsListId(civilDoc, transaction, partsListName);
      TrySetObjectIdProperty(network, partsListId, "PartsListId");

      var styleName = PluginRuntime.GetOptionalString(parameters, "style");
      if (!string.IsNullOrWhiteSpace(styleName))
      {
        var styleId = FindStyleId(civilDoc, transaction, styleName!, "PipeNetworkStyles", "NetworkStyles");
        if (styleId != ObjectId.Null)
        {
          TrySetObjectIdProperty(network, styleId, "StyleId");
        }
      }

      var referenceSurface = PluginRuntime.GetOptionalString(parameters, "referenceSurface");
      if (!string.IsNullOrWhiteSpace(referenceSurface))
      {
        var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, referenceSurface!, OpenMode.ForRead);
        TrySetObjectIdProperty(network, surface.ObjectId, "ReferenceSurfaceId", "SurfaceId");
      }

      var referenceAlignment = PluginRuntime.GetOptionalString(parameters, "referenceAlignment");
      if (!string.IsNullOrWhiteSpace(referenceAlignment))
      {
        var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, referenceAlignment!);
        TrySetObjectIdProperty(network, alignment.ObjectId, "ReferenceAlignmentId", "AlignmentId");
      }

      return new Dictionary<string, object?>
      {
        ["name"] = CivilObjectUtils.GetName(network) ?? name,
        ["handle"] = CivilObjectUtils.GetHandle(network),
        ["partsList"] = ResolveObjectName(transaction, partsListId),
        ["created"] = true,
      };
    });
  }

  public static Task<object?> AddStructureToNetworkAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var rimElevation = PluginRuntime.GetOptionalDouble(parameters, "rimElevation") ?? 0.0;
    var sumpDepth = PluginRuntime.GetOptionalDouble(parameters, "sumpDepth") ?? 0.0;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var location = new Point3d(x, y, rimElevation);
      var createdStructureId = AddStructureToNetwork(network, transaction, location, partName, rimElevation, sumpDepth);
      var structure = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, createdStructureId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["networkName"] = CivilObjectUtils.GetName(network) ?? networkName,
        ["structure"] = ToStructureData(structure, transaction),
        ["added"] = true,
      };
    });
  }

  public static Task<object?> AddPipeToNetworkAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var diameter = PluginRuntime.GetOptionalDouble(parameters, "diameter");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var startStructureName = PluginRuntime.GetOptionalString(parameters, "startStructure");
      var endStructureName = PluginRuntime.GetOptionalString(parameters, "endStructure");
      var startStructureId = !string.IsNullOrWhiteSpace(startStructureName)
        ? FindStructureByName(network, transaction, startStructureName!, OpenMode.ForRead).ObjectId
        : ObjectId.Null;
      var endStructureId = !string.IsNullOrWhiteSpace(endStructureName)
        ? FindStructureByName(network, transaction, endStructureName!, OpenMode.ForRead).ObjectId
        : ObjectId.Null;

      var startPoint = ReadPoint(parameters, "startPoint");
      var endPoint = ReadPoint(parameters, "endPoint");
      var createdPipeId = AddPipeToNetwork(network, transaction, partName, diameter, startPoint, endPoint, startStructureId, endStructureId);
      var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, createdPipeId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["networkName"] = CivilObjectUtils.GetName(network) ?? networkName,
        ["pipe"] = ToPipeData(pipe, transaction),
        ["added"] = true,
      };
    });
  }

  public static Task<object?> ListPipePartsCatalogAsync(JsonObject? parameters)
  {
    var partsListName = PluginRuntime.GetOptionalString(parameters, "partsList");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var partsLists = EnumeratePartsLists(civilDoc, transaction)
        .Where(partsList => string.IsNullOrWhiteSpace(partsListName) || string.Equals(CivilObjectUtils.GetName(partsList), partsListName, StringComparison.OrdinalIgnoreCase))
        .Select(partsList => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(partsList),
          ["handle"] = partsList is DBObject dbObject ? CivilObjectUtils.GetHandle(dbObject) : null,
          ["parts"] = EnumeratePartNames(partsList).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(name => name).ToList(),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["partsLists"] = partsLists,
      };
    });
  }

  public static int CountPipeNetworks(object civilDoc)
  {
    return GetPipeNetworkIds(civilDoc).Count();
  }

  public static string? GetFirstPipeNetworkStyleName(object civilDoc, Transaction transaction)
  {
    var stylesProperty = civilDoc.GetType().GetProperty("Styles", BindingFlags.Public | BindingFlags.Instance);
    var styles = stylesProperty?.GetValue(civilDoc);
    var collection = GetNamedMemberValue(styles, "PipeNetworkStyles") ?? GetNamedMemberValue(styles, "NetworkStyles");
    return LookupUtils.GetFirstStyleName(collection, transaction);
  }

  private static IEnumerable<DBObject> EnumeratePipeNetworks(object civilDoc, Transaction transaction, OpenMode openMode)
  {
    foreach (var objectId in GetPipeNetworkIds(civilDoc))
    {
      yield return CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
    }
  }

  private static IEnumerable<ObjectId> GetPipeNetworkIds(object civilDoc)
  {
    var candidates = new[]
    {
      CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds"),
      GetNamedMemberValue(civilDoc, "PipeNetworkCollection"),
      GetNamedMemberValue(civilDoc, "NetworkCollection"),
      GetNamedMemberValue(civilDoc, "PipeNetworks"),
      GetNamedMemberValue(civilDoc, "Networks"),
    };

    foreach (var candidate in candidates)
    {
      foreach (var objectId in ToObjectIdsFlexible(candidate))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static IEnumerable<ObjectId> ToObjectIdsFlexible(object? value)
  {
    foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
    {
      yield return objectId;
    }

    if (value is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item is ObjectId objectId && objectId != ObjectId.Null)
        {
          yield return objectId;
          continue;
        }

        var itemObjectId = GetAnyObjectId(item, "ObjectId", "Id");
        if (itemObjectId != ObjectId.Null)
        {
          yield return itemObjectId;
        }
      }
    }
  }

  private static DBObject FindPipeNetworkByName(object civilDoc, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (var network in EnumeratePipeNetworks(civilDoc, transaction, openMode))
    {
      if (string.Equals(CivilObjectUtils.GetName(network), name, StringComparison.OrdinalIgnoreCase))
      {
        return network;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{name}' was not found.");
  }

  private static DBObject FindPipeByName(DBObject network, Transaction transaction, string pipeName, OpenMode openMode)
  {
    foreach (var objectId in GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
    {
      var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
      if (string.Equals(CivilObjectUtils.GetName(pipe), pipeName, StringComparison.OrdinalIgnoreCase))
      {
        return pipe;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe '{pipeName}' was not found in network '{CivilObjectUtils.GetName(network)}'.");
  }

  private static DBObject FindStructureByName(DBObject network, Transaction transaction, string structureName, OpenMode openMode)
  {
    foreach (var objectId in GetChildObjectIds(network, "GetStructureIds", "StructureIds", "Structures", "StructureCollection"))
    {
      var structure = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
      if (string.Equals(CivilObjectUtils.GetName(structure), structureName, StringComparison.OrdinalIgnoreCase))
      {
        return structure;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Structure '{structureName}' was not found in network '{CivilObjectUtils.GetName(network)}'.");
  }

  private static Dictionary<string, object?> ToPipeNetworkSummary(DBObject network, Transaction transaction)
  {
    var pipes = GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection").ToList();
    var structures = GetChildObjectIds(network, "GetStructureIds", "StructureIds", "Structures", "StructureCollection").ToList();

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(network) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(network),
      ["pipeCount"] = pipes.Count,
      ["structureCount"] = structures.Count,
      ["surface"] = ResolveObjectName(transaction, GetAnyObjectId(network, "ReferenceSurfaceId", "SurfaceId")),
    };
  }

  private static Dictionary<string, object?> ToPipeNetworkDetail(DBObject network, Transaction transaction)
  {
    var pipes = GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection")
      .Select(objectId => ToPipeData(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead), transaction))
      .ToList();
    var structures = GetChildObjectIds(network, "GetStructureIds", "StructureIds", "Structures", "StructureCollection")
      .Select(objectId => ToStructureData(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead), transaction))
      .ToList();

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(network) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(network),
      ["style"] = ResolveObjectName(transaction, GetAnyObjectId(network, "StyleId")) ?? string.Empty,
      ["referenceSurface"] = ResolveObjectName(transaction, GetAnyObjectId(network, "ReferenceSurfaceId", "SurfaceId")),
      ["referenceAlignment"] = ResolveObjectName(transaction, GetAnyObjectId(network, "ReferenceAlignmentId", "AlignmentId")),
      ["pipes"] = pipes,
      ["structures"] = structures,
    };
  }

  private static Dictionary<string, object?> ToPipeData(DBObject pipe, Transaction transaction)
  {
    var startPoint = GetPointProperty(pipe, "StartPoint", "PointAtStart");
    var endPoint = GetPointProperty(pipe, "EndPoint", "PointAtEnd");
    var startInvert = GetAnyDouble(pipe, "StartInvertElevation", "InvertIn", "StartInnerElevation") ?? startPoint?.Z ?? 0.0;
    var endInvert = GetAnyDouble(pipe, "EndInvertElevation", "InvertOut", "EndInnerElevation") ?? endPoint?.Z ?? 0.0;
    var material = GetAnyString(pipe, "Material", "PartDescription", "PartFamilyName") ?? string.Empty;

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(pipe) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(pipe),
      ["startStructure"] = ResolveObjectName(transaction, GetAnyObjectId(pipe, "StartStructureId")),
      ["endStructure"] = ResolveObjectName(transaction, GetAnyObjectId(pipe, "EndStructureId")),
      ["length"] = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? Distance(startPoint, endPoint),
      ["diameter"] = GetAnyDouble(pipe, "InnerDiameterOrWidth", "InnerDiameter", "Diameter") ?? 0.0,
      ["slope"] = GetAnyDouble(pipe, "Slope", "FlowSlope") ?? CalculateSlope(startPoint, endPoint),
      ["material"] = material,
      ["invertIn"] = startInvert,
      ["invertOut"] = endInvert,
    };
  }

  private static Dictionary<string, object?> ToStructureData(DBObject structure, Transaction transaction)
  {
    var location = GetPointProperty(structure, "Location", "Position", "CenterPoint");
    var connectedPipeIds = GetChildObjectIds(structure, "GetConnectedPipeIds", "ConnectedPipeIds", "ConnectedPipes");

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(structure) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(structure),
      ["type"] = GetAnyString(structure, "PartType", "StructureType", "PartDescription") ?? structure.GetType().Name,
      ["rimElevation"] = GetAnyDouble(structure, "RimElevation", "SurfaceElevation", "Elevation") ?? location?.Z ?? 0.0,
      ["sumpElevation"] = GetAnyDouble(structure, "SumpElevation") ?? ((GetAnyDouble(structure, "RimElevation", "SurfaceElevation", "Elevation") ?? location?.Z ?? 0.0) - (GetAnyDouble(structure, "SumpDepth") ?? 0.0)),
      ["x"] = location?.X ?? 0.0,
      ["y"] = location?.Y ?? 0.0,
      ["connectedPipes"] = connectedPipeIds.Select(objectId => ResolveObjectName(transaction, objectId) ?? objectId.Handle.ToString()).ToList(),
    };
  }

  private static ObjectId CreatePipeNetwork(object civilDoc, string name)
  {
    foreach (var typeName in new[]
    {
      "Autodesk.Civil.DatabaseServices.Network",
      "Autodesk.Civil.DatabaseServices.PipeNetwork",
    })
    {
      var type = Type.GetType($"{typeName}, AeccDbMgd", false);
      if (type == null)
      {
        continue;
      }

      var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(method => method.Name == "Create")
        .OrderBy(method => method.GetParameters().Length)
        .ToList();

      foreach (var method in methods)
      {
        var args = BuildCreateNetworkArguments(method.GetParameters(), civilDoc, name);
        if (args == null)
        {
          continue;
        }

        try
        {
          var result = method.Invoke(null, args);
          if (result is ObjectId objectId && objectId != ObjectId.Null)
          {
            return objectId;
          }
        }
        catch
        {
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to create a pipe network with the available Civil 3D API overloads.");
  }

  private static object?[]? BuildCreateNetworkArguments(ParameterInfo[] parameters, object civilDoc, string name)
  {
    var args = new object?[parameters.Length];

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameter = parameters[index];
      var parameterType = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType;

      if (parameterType.Name == "CivilDocument")
      {
        args[index] = civilDoc;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[index] = name;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[index] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[index] = 0;
        continue;
      }

      if (parameterType == typeof(ObjectId))
      {
        args[index] = ObjectId.Null;
        continue;
      }

      return null;
    }

    return args;
  }

  private static ObjectId AddStructureToNetwork(DBObject network, Transaction transaction, Point3d location, string partName, double rimElevation, double sumpDepth)
  {
    var partId = FindPartIdForNetwork(network, transaction, partName, true);
    var methods = network.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name.Contains("AddStructure", StringComparison.OrdinalIgnoreCase))
      .OrderBy(method => method.GetParameters().Length)
      .ToList();

    foreach (var method in methods)
    {
      var args = BuildAddStructureArguments(method.GetParameters(), partId, location, rimElevation, sumpDepth);
      if (args == null)
      {
        continue;
      }

      try
      {
        var result = method.Invoke(network, args);
        var createdId = ExtractObjectId(result, args);
        if (createdId != ObjectId.Null)
        {
          return createdId;
        }
      }
      catch
      {
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add structure '{partName}' to network '{CivilObjectUtils.GetName(network)}'.");
  }

  private static object?[]? BuildAddStructureArguments(ParameterInfo[] parameters, ObjectId partId, Point3d location, double rimElevation, double sumpDepth)
  {
    var args = new object?[parameters.Length];
    var doubleIndex = 0;

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameterType = parameters[index].ParameterType.IsByRef ? parameters[index].ParameterType.GetElementType()! : parameters[index].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        args[index] = partId;
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[index] = location;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[index] = doubleIndex++ == 0 ? rimElevation : sumpDepth;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[index] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[index] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[index] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static ObjectId AddPipeToNetwork(DBObject network, Transaction transaction, string partName, double? diameter, Point3d? startPoint, Point3d? endPoint, ObjectId startStructureId, ObjectId endStructureId)
  {
    var partId = FindPartIdForNetwork(network, transaction, partName, false);
    var methods = network.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name.Contains("AddPipe", StringComparison.OrdinalIgnoreCase))
      .OrderBy(method => method.GetParameters().Length)
      .ToList();

    foreach (var method in methods)
    {
      var args = BuildAddPipeArguments(method.GetParameters(), partId, diameter, startPoint, endPoint, startStructureId, endStructureId);
      if (args == null)
      {
        continue;
      }

      try
      {
        var result = method.Invoke(network, args);
        var createdId = ExtractObjectId(result, args);
        if (createdId != ObjectId.Null)
        {
          return createdId;
        }
      }
      catch
      {
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add pipe '{partName}' to network '{CivilObjectUtils.GetName(network)}'.");
  }

  private static object?[]? BuildAddPipeArguments(ParameterInfo[] parameters, ObjectId partId, double? diameter, Point3d? startPoint, Point3d? endPoint, ObjectId startStructureId, ObjectId endStructureId)
  {
    var args = new object?[parameters.Length];
    var points = new Queue<Point3d>(new[] { startPoint ?? Point3d.Origin, endPoint ?? Point3d.Origin });
    var structures = new Queue<ObjectId>(new[] { startStructureId, endStructureId });
    var doubles = new Queue<double>(new[] { diameter ?? 0.0, 0.0, 0.0 });

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameterType = parameters[index].ParameterType.IsByRef ? parameters[index].ParameterType.GetElementType()! : parameters[index].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        if (partId != ObjectId.Null)
        {
          args[index] = partId;
          partId = ObjectId.Null;
          continue;
        }

        args[index] = structures.Count > 0 ? structures.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[index] = points.Count > 0 ? points.Dequeue() : Point3d.Origin;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[index] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[index] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[index] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[index] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static ObjectId ExtractObjectId(object? result, object?[] args)
  {
    if (result is ObjectId objectId && objectId != ObjectId.Null)
    {
      return objectId;
    }

    var resultId = GetAnyObjectId(result, "ObjectId", "Id");
    if (resultId != ObjectId.Null)
    {
      return resultId;
    }

    foreach (var arg in args)
    {
      var argId = GetAnyObjectId(arg, "ObjectId", "Id");
      if (argId != ObjectId.Null)
      {
        return argId;
      }
    }

    return ObjectId.Null;
  }

  private static IEnumerable<ObjectId> GetChildObjectIds(object owner, params string[] memberNames)
  {
    foreach (var memberName in memberNames)
    {
      object? memberValue = CivilObjectUtils.InvokeMethod(owner, memberName) ?? GetNamedMemberValue(owner, memberName);
      foreach (var objectId in ToObjectIdsFlexible(memberValue))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static object? GetNamedMemberValue(object? value, string memberName)
  {
    if (value == null)
    {
      return null;
    }

    var property = value.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
    if (property != null)
    {
      return property.GetValue(value);
    }

    var field = value.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
    return field?.GetValue(value);
  }

  private static string? ResolveObjectName(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null)
    {
      return null;
    }

    try
    {
      var dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
      return CivilObjectUtils.GetName(dbObject);
    }
    catch
    {
      return null;
    }
  }

  private static ObjectId GetAnyObjectId(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var objectId = CivilObjectUtils.GetPropertyValue<ObjectId>(value, propertyName);
      if (objectId != ObjectId.Null)
      {
        return objectId;
      }
    }

    return ObjectId.Null;
  }

  private static double? GetAnyDouble(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var propertyValue = CivilObjectUtils.GetPropertyValue<double?>(value, propertyName);
      if (propertyValue.HasValue)
      {
        return propertyValue.Value;
      }
    }

    return null;
  }

  private static string? GetAnyString(object? value, params string[] propertyNames)
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

  private static Point3d? GetPointProperty(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var propertyValue = CivilObjectUtils.GetPropertyValue<Point3d?>(value, propertyName);
      if (propertyValue.HasValue)
      {
        return propertyValue.Value;
      }
    }

    return null;
  }

  private static double Distance(Point3d? startPoint, Point3d? endPoint)
  {
    if (!startPoint.HasValue || !endPoint.HasValue)
    {
      return 0.0;
    }

    return startPoint.Value.DistanceTo(endPoint.Value);
  }

  private static double CalculateSlope(Point3d? startPoint, Point3d? endPoint)
  {
    if (!startPoint.HasValue || !endPoint.HasValue)
    {
      return 0.0;
    }

    var horizontal = Math.Sqrt(Math.Pow(endPoint.Value.X - startPoint.Value.X, 2) + Math.Pow(endPoint.Value.Y - startPoint.Value.Y, 2));
    if (horizontal <= 1.0e-9)
    {
      return 0.0;
    }

    return (endPoint.Value.Z - startPoint.Value.Z) / horizontal;
  }

  private static Point3d? ReadPoint(JsonObject? parameters, string parameterName)
  {
    if (PluginRuntime.GetParameter(parameters, parameterName) is not JsonObject pointNode)
    {
      return null;
    }

    var x = pointNode["x"]?.GetValue<double>() ?? 0.0;
    var y = pointNode["y"]?.GetValue<double>() ?? 0.0;
    var z = pointNode["z"]?.GetValue<double>() ?? 0.0;
    return new Point3d(x, y, z);
  }

  private static void TrySetObjectIdProperty(object target, ObjectId objectId, params string[] propertyNames)
  {
    if (objectId == ObjectId.Null)
    {
      return;
    }

    foreach (var propertyName in propertyNames)
    {
      var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (property == null || !property.CanWrite || property.PropertyType != typeof(ObjectId))
      {
        continue;
      }

      try
      {
        property.SetValue(target, objectId);
        return;
      }
      catch
      {
      }
    }
  }

  private static ObjectId FindStyleId(object civilDoc, Transaction transaction, string styleName, params string[] collectionNames)
  {
    var styles = GetNamedMemberValue(civilDoc, "Styles");
    if (styles == null)
    {
      return ObjectId.Null;
    }

    foreach (var collectionName in collectionNames)
    {
      var collection = GetNamedMemberValue(styles, collectionName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null)
        {
          continue;
        }

        var style = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(style), styleName, StringComparison.OrdinalIgnoreCase))
        {
          return objectId;
        }
      }
    }

    return ObjectId.Null;
  }

  private static ObjectId FindPartsListId(object civilDoc, Transaction transaction, string partsListName)
  {
    foreach (var partsList in EnumeratePartsLists(civilDoc, transaction))
    {
      if (string.Equals(CivilObjectUtils.GetName(partsList), partsListName, StringComparison.OrdinalIgnoreCase) && partsList is DBObject dbObject)
      {
        return dbObject.ObjectId;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Parts list '{partsListName}' was not found.");
  }

  private static IEnumerable<object> EnumeratePartsLists(object civilDoc, Transaction transaction)
  {
    var styles = GetNamedMemberValue(civilDoc, "Styles");
    if (styles == null)
    {
      yield break;
    }

    foreach (var collectionName in new[] { "PartsListSet", "PartsLists", "PartsListCollection", "PartsListStyles" })
    {
      var collection = GetNamedMemberValue(styles, collectionName) ?? GetNamedMemberValue(civilDoc, collectionName);
      foreach (var objectId in ToObjectIdsFlexible(collection))
      {
        yield return transaction.GetObject(objectId, OpenMode.ForRead)!;
      }
    }
  }

  private static IEnumerable<string> EnumeratePartNames(object partsList)
  {
    foreach (var collectionName in new[] { "PartFamilies", "PipeFamilies", "StructureFamilies", "PartFamilySet" })
    {
      var collection = GetNamedMemberValue(partsList, collectionName);
      foreach (var item in EnumerateNamedObjects(collection))
      {
        var familyName = CivilObjectUtils.GetName(item);
        if (!string.IsNullOrWhiteSpace(familyName))
        {
          yield return familyName!;
        }

        foreach (var child in EnumerateNamedObjects(GetNamedMemberValue(item, "PartSizeFilter") ?? GetNamedMemberValue(item, "PartSizes") ?? GetNamedMemberValue(item, "SizeDataRecords")))
        {
          var childName = CivilObjectUtils.GetName(child) ?? CivilObjectUtils.GetStringProperty(child, "Description");
          if (!string.IsNullOrWhiteSpace(childName))
          {
            yield return childName!;
          }
        }
      }
    }
  }

  private static IEnumerable<object> EnumerateNamedObjects(object? collection)
  {
    if (collection is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item == null)
        {
          continue;
        }

        yield return item;
      }
    }
  }

  private static ObjectId FindPartIdForNetwork(DBObject network, Transaction transaction, string partName, bool structurePart)
  {
    var partsListId = GetAnyObjectId(network, "PartsListId");
    if (partsListId == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{CivilObjectUtils.GetName(network)}' does not have a parts list assigned.");
    }

    var partsList = transaction.GetObject(partsListId, OpenMode.ForRead) ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "The network parts list could not be opened.");
    var preferredCollections = structurePart
      ? new[] { "StructureFamilies", "PartFamilies", "PartFamilySet" }
      : new[] { "PipeFamilies", "PartFamilies", "PartFamilySet" };

    foreach (var collectionName in preferredCollections)
    {
      var collection = GetNamedMemberValue(partsList, collectionName);
      foreach (var item in EnumerateNamedObjects(collection))
      {
        var objectId = GetAnyObjectId(item, "ObjectId", "Id");
        var name = CivilObjectUtils.GetName(item) ?? CivilObjectUtils.GetStringProperty(item, "Description");
        if (objectId != ObjectId.Null && string.Equals(name, partName, StringComparison.OrdinalIgnoreCase))
        {
          return objectId;
        }

        foreach (var child in EnumerateNamedObjects(GetNamedMemberValue(item, "PartSizeFilter") ?? GetNamedMemberValue(item, "PartSizes") ?? GetNamedMemberValue(item, "SizeDataRecords")))
        {
          var childId = GetAnyObjectId(child, "ObjectId", "Id");
          var childName = CivilObjectUtils.GetName(child) ?? CivilObjectUtils.GetStringProperty(child, "Description");
          if (childId != ObjectId.Null && string.Equals(childName, partName, StringComparison.OrdinalIgnoreCase))
          {
            return childId;
          }
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Part '{partName}' was not found in the parts list for network '{CivilObjectUtils.GetName(network)}'.");
  }
}
