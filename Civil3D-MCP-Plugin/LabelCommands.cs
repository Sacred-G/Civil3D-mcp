using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

public static class LabelCommands
{
  public static Task<object?> ListLabelStylesAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styles = GetLabelStyleCollection(civilDoc, objectType);
      var styleItems = styles
        .Select(objectId => transaction.GetObject(objectId, OpenMode.ForRead))
        .Where(style => style != null)
        .Select(style => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(style),
          ["handle"] = style is DBObject dbObject ? CivilObjectUtils.GetHandle(dbObject) : null,
          ["description"] = CivilObjectUtils.GetStringProperty(style, "Description"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["styles"] = styleItems,
      };
    });
  }

  public static Task<object?> ListLabelsAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var target = ResolveTargetObject(civilDoc, transaction, objectType, objectName, OpenMode.ForRead);
      var labels = GetLabelIds(target)
        .Select(objectId => transaction.GetObject(objectId, OpenMode.ForRead))
        .Where(label => label != null)
        .Select(label => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(label),
          ["handle"] = label is DBObject dbObject ? CivilObjectUtils.GetHandle(dbObject) : null,
          ["type"] = label?.GetType().Name,
          ["style"] = ResolveObjectName(transaction, GetObjectId(label, "StyleId")),
          ["text"] = CivilObjectUtils.GetStringProperty(label, "TextOverride") ?? CivilObjectUtils.GetStringProperty(label, "Text"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["objectName"] = objectName,
        ["labels"] = labels,
      };
    });
  }

  public static Task<object?> AddLabelAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");
    var labelType = PluginRuntime.GetRequiredString(parameters, "labelType");
    var labelStyle = PluginRuntime.GetOptionalString(parameters, "labelStyle");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var target = ResolveTargetObject(civilDoc, transaction, objectType, objectName, OpenMode.ForWrite);

      if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "label_set", StringComparison.OrdinalIgnoreCase))
      {
        var labelSetId = LookupUtils.GetAlignmentLabelSetId(civilDoc, transaction, labelStyle);
        ApplyObjectIdProperty(target, labelSetId, "LabelSetId");
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, labelSetId),
          ["applied"] = true,
        };
      }

      if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "station", StringComparison.OrdinalIgnoreCase))
      {
        var station = PluginRuntime.GetOptionalDouble(parameters, "station")
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Alignment station labels require 'station'.");
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateAlignmentStationLabel(target, styleId, station);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["station"] = station,
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "label_set", StringComparison.OrdinalIgnoreCase))
      {
        var labelSetId = LookupUtils.GetProfileLabelSetId(civilDoc, transaction, labelStyle);
        ApplyObjectIdProperty(target, labelSetId, "LabelSetId");
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, labelSetId),
          ["applied"] = true,
        };
      }

      if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "station", StringComparison.OrdinalIgnoreCase))
      {
        var station = PluginRuntime.GetOptionalDouble(parameters, "station")
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Profile station labels require 'station'.");
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateProfileStationLabel(target, styleId, station);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["station"] = station,
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "surface", StringComparison.OrdinalIgnoreCase) && (string.Equals(labelType, "spot_elevation", StringComparison.OrdinalIgnoreCase) || string.Equals(labelType, "spot", StringComparison.OrdinalIgnoreCase)))
      {
        var pointNode = PluginRuntime.GetParameter(parameters, "point") as JsonObject
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require 'point'.");
        var point = new Point2d(
          pointNode["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require point.x."),
          pointNode["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require point.y.")
        );
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateSurfaceSpotLabel(target, styleId, point);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["point"] = new Dictionary<string, object?>
          {
            ["x"] = point.X,
            ["y"] = point.Y,
          },
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "pipe", StringComparison.OrdinalIgnoreCase))
      {
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreatePipeLabel(target, styleId, labelType);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "structure", StringComparison.OrdinalIgnoreCase))
      {
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateStructureLabel(target, styleId, labelType);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"addLabel currently supports alignment/profile 'label_set', alignment/profile 'station', surface 'spot_elevation', and basic pipe/structure labels. Requested objectType='{objectType}', labelType='{labelType}'."
      );
    });
  }

  private static DBObject ResolveTargetObject(object civilDoc, Transaction transaction, string objectType, string objectName, OpenMode openMode)
  {
    if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase))
    {
      return CivilObjectUtils.FindAlignmentByName((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc, transaction, objectName);
    }

    if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase))
    {
      foreach (ObjectId alignmentId in ((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc).GetAlignmentIds())
      {
        var alignment = CivilObjectUtils.GetRequiredObject<Autodesk.Civil.DatabaseServices.Alignment>(transaction, alignmentId, OpenMode.ForRead);
        foreach (ObjectId profileId in alignment.GetProfileIds())
        {
          var profile = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, profileId, openMode);
          if (string.Equals(CivilObjectUtils.GetName(profile), objectName, StringComparison.OrdinalIgnoreCase))
          {
            return profile;
          }
        }
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile '{objectName}' was not found.");
    }

    if (string.Equals(objectType, "surface", StringComparison.OrdinalIgnoreCase))
    {
      return CivilObjectUtils.FindSurfaceByName((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc, transaction, objectName, openMode);
    }

    if (string.Equals(objectType, "pipe", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeObject(civilDoc, transaction, objectName, openMode, true);
    }

    if (string.Equals(objectType, "structure", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeObject(civilDoc, transaction, objectName, openMode, false);
    }

    if (string.Equals(objectType, "pipe_network", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeNetwork(civilDoc, transaction, objectName, openMode);
    }

    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unsupported label objectType '{objectType}'.");
  }

  private static DBObject ResolvePipeNetwork(object civilDoc, Transaction transaction, string objectName, OpenMode openMode)
  {
    foreach (var objectId in GetPipeNetworkIds(civilDoc))
    {
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
      if (string.Equals(CivilObjectUtils.GetName(network), objectName, StringComparison.OrdinalIgnoreCase))
      {
        return network;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{objectName}' was not found.");
  }

  private static DBObject ResolvePipeObject(object civilDoc, Transaction transaction, string objectName, OpenMode openMode, bool pipe)
  {
    foreach (var networkId in GetPipeNetworkIds(civilDoc))
    {
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, networkId, OpenMode.ForRead);
      foreach (var objectId in GetPipeRelatedObjectIds(network, pipe))
      {
        var dbObject = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
        if (string.Equals(CivilObjectUtils.GetName(dbObject), objectName, StringComparison.OrdinalIgnoreCase))
        {
          return dbObject;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"{(pipe ? "Pipe" : "Structure")} '{objectName}' was not found.");
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
      foreach (var objectId in CivilObjectUtils.ToObjectIds(candidate))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static IEnumerable<ObjectId> GetLabelStyleCollection(object civilDoc, string objectType)
  {
    var styles = GetNamedMemberValue(civilDoc, "Styles") ?? throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Civil 3D styles collection is not available.");
    var labelSetStyles = GetNamedMemberValue(styles, "LabelSetStyles");

    object? collection = objectType.ToLowerInvariant() switch
    {
      "alignment" => GetNamedMemberValue(labelSetStyles, "AlignmentLabelSetStyles") ?? GetNamedMemberValue(styles, "AlignmentLabelStyles"),
      "profile" => GetNamedMemberValue(labelSetStyles, "ProfileLabelSetStyles") ?? GetNamedMemberValue(styles, "ProfileLabelStyles"),
      "surface" => GetNamedMemberValue(styles, "SurfaceLabelStyles"),
      "pipe_network" => GetNamedMemberValue(styles, "PipeNetworkLabelStyles") ?? GetNamedMemberValue(styles, "PipeLabelStyles"),
      "pipe" => GetNamedMemberValue(styles, "PipeLabelStyles") ?? GetNamedMemberValue(styles, "PipeNetworkLabelStyles"),
      "structure" => GetNamedMemberValue(styles, "StructureLabelStyles") ?? GetNamedMemberValue(styles, "PipeNetworkLabelStyles"),
      _ => null,
    };

    if (collection == null)
    {
      return Array.Empty<ObjectId>();
    }

    return CivilObjectUtils.ToObjectIds(collection).Where(objectId => objectId != ObjectId.Null).ToList();
  }

  private static ObjectId FindLabelStyleId(object civilDoc, Transaction transaction, string objectType, string? labelStyle)
  {
    var fallback = ObjectId.Null;

    foreach (var objectId in GetLabelStyleCollection(civilDoc, objectType))
    {
      if (objectId == ObjectId.Null)
      {
        continue;
      }

      if (fallback == ObjectId.Null)
      {
        fallback = objectId;
      }

      if (string.IsNullOrWhiteSpace(labelStyle))
      {
        continue;
      }

      var style = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(style), labelStyle, StringComparison.OrdinalIgnoreCase))
      {
        return objectId;
      }
    }

    return fallback;
  }

  private static ObjectId CreateSurfaceSpotLabel(DBObject surface, ObjectId styleId, Point2d point)
  {
    return TryCreateLabel(
      new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.SurfaceSpotElevationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.SurfaceSpotElevationLabel, AeccDbMgd",
      },
      "surface spot elevation",
      parameters => BuildSurfaceSpotLabelArguments(parameters, surface.ObjectId, styleId, point)
    );
  }

  private static ObjectId CreateProfileStationLabel(DBObject profile, ObjectId styleId, double station)
  {
    return TryCreateLabel(
      new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.ProfileStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.ProfileStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.Labels.ProfileStationElevationLabel, AeccDbMgd",
      },
      "profile station",
      parameters => BuildProfileStationLabelArguments(parameters, profile.ObjectId, styleId, station)
    );
  }

  private static ObjectId CreatePipeLabel(DBObject pipe, ObjectId styleId, string labelType)
  {
    var candidateTypes = string.Equals(labelType, "plan", StringComparison.OrdinalIgnoreCase)
      ? new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.PipePlanLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.PipePlanLabel, AeccDbMgd",
      }
      : new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.PipeLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.PipeLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.Labels.PipePlanLabel, AeccDbMgd",
      };

    return TryCreateLabel(
      candidateTypes,
      "pipe",
      parameters => BuildSingleObjectLabelArguments(parameters, pipe.ObjectId, styleId)
    );
  }

  private static ObjectId CreateStructureLabel(DBObject structure, ObjectId styleId, string labelType)
  {
    var candidateTypes = string.Equals(labelType, "plan", StringComparison.OrdinalIgnoreCase)
      ? new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.StructurePlanLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.StructurePlanLabel, AeccDbMgd",
      }
      : new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.StructureLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.StructureLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.Labels.StructurePlanLabel, AeccDbMgd",
      };

    return TryCreateLabel(
      candidateTypes,
      "structure",
      parameters => BuildSingleObjectLabelArguments(parameters, structure.ObjectId, styleId)
    );
  }

  private static ObjectId CreateAlignmentStationLabel(DBObject alignment, ObjectId styleId, double station)
  {
    return TryCreateLabel(
      new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.AlignmentStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.AlignmentStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.Labels.AlignmentMajorStationLabel, AeccDbMgd",
      },
      "alignment station",
      parameters => BuildAlignmentStationLabelArguments(parameters, alignment.ObjectId, styleId, station)
    );
  }

  private static ObjectId TryCreateLabel(IEnumerable<string> candidateTypeNames, string labelDescription, Func<ParameterInfo[], object?[]?> argumentBuilder)
  {
    foreach (var typeName in candidateTypeNames)
    {
      var type = Type.GetType(typeName, false);
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
        var args = argumentBuilder(method.GetParameters());
        if (args == null)
        {
          continue;
        }

        try
        {
          var result = method.Invoke(null, args);
          var objectId = ExtractCreatedObjectId(result, args);
          if (objectId != ObjectId.Null)
          {
            return objectId;
          }
        }
        catch
        {
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to create a {labelDescription} label with the available Civil 3D API overloads.");
  }

  private static object?[]? BuildSurfaceSpotLabelArguments(ParameterInfo[] parameters, ObjectId surfaceId, ObjectId styleId, Point2d point)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { surfaceId, styleId });
    var points2d = new Queue<Point2d>(new[] { point });
    var doubles = new Queue<double>(new[] { point.X, point.Y, 0.0, 0.0 });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = points2d.Count > 0 ? points2d.Dequeue() : point;
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = new Point3d(point.X, point.Y, 0.0);
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static object?[]? BuildProfileStationLabelArguments(ParameterInfo[] parameters, ObjectId profileId, ObjectId styleId, double station)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { profileId, styleId });
    var doubles = new Queue<double>(new[] { station, 0.0, 0.0 });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = new Point2d(0.0, 0.0);
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = Point3d.Origin;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static object?[]? BuildSingleObjectLabelArguments(ParameterInfo[] parameters, ObjectId objectId, ObjectId styleId)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { objectId, styleId });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = new Point2d(0.0, 0.0);
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = Point3d.Origin;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = 0.0;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static object?[]? BuildAlignmentStationLabelArguments(ParameterInfo[] parameters, ObjectId alignmentId, ObjectId styleId, double station)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { alignmentId, styleId });
    var doubles = new Queue<double>(new[] { station, 0.0, 0.0 });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = new Point2d(0.0, 0.0);
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = Point3d.Origin;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static ObjectId ExtractCreatedObjectId(object? result, object?[] args)
  {
    if (result is ObjectId objectId && objectId != ObjectId.Null)
    {
      return objectId;
    }

    var resultId = CivilObjectUtils.GetPropertyValue<ObjectId>(result, "ObjectId");
    if (resultId != ObjectId.Null)
    {
      return resultId;
    }

    foreach (var arg in args)
    {
      var argId = CivilObjectUtils.GetPropertyValue<ObjectId>(arg, "ObjectId");
      if (argId != ObjectId.Null)
      {
        return argId;
      }
    }

    return ObjectId.Null;
  }

  private static string? ResolveHandle(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null)
    {
      return null;
    }

    try
    {
      var dbObject = transaction.GetObject(objectId, OpenMode.ForRead) as DBObject;
      return dbObject == null ? null : CivilObjectUtils.GetHandle(dbObject);
    }
    catch
    {
      return null;
    }
  }

  private static IEnumerable<ObjectId> GetPipeRelatedObjectIds(DBObject network, bool pipe)
  {
    var members = pipe
      ? new[] { "GetPipeIds", "PipeIds", "Pipes", "PipeCollection" }
      : new[] { "GetStructureIds", "StructureIds", "Structures", "StructureCollection" };

    foreach (var name in members)
    {
      var value = CivilObjectUtils.InvokeMethod(network, name) ?? GetNamedMemberValue(network, name);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static IEnumerable<ObjectId> GetLabelIds(object target)
  {
    foreach (var name in new[] { "GetLabelIds", "LabelIds", "Labels" })
    {
      var value = CivilObjectUtils.InvokeMethod(target, name) ?? GetNamedMemberValue(target, name);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
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

  private static ObjectId GetObjectId(object? value, string propertyName)
  {
    return CivilObjectUtils.GetPropertyValue<ObjectId>(value, propertyName);
  }

  private static string? ResolveObjectName(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null)
    {
      return null;
    }

    try
    {
      return CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead));
    }
    catch
    {
      return null;
    }
  }

  private static void ApplyObjectIdProperty(object target, ObjectId objectId, params string[] propertyNames)
  {
    if (objectId == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "Requested label style or label set was not found.");
    }

    foreach (var propertyName in propertyNames)
    {
      var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (property == null || !property.CanWrite || property.PropertyType != typeof(ObjectId))
      {
        continue;
      }

      property.SetValue(target, objectId);
      return;
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Object '{CivilObjectUtils.GetName(target)}' does not expose a writable label set property.");
  }
}
