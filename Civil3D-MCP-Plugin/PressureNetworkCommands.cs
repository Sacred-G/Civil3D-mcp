using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for pressure network MCP tools.
///
/// Civil 3D API notes (reflection-based late binding):
///   AeccPressureNetwork   -- top-level network object
///   AeccPressurePipe      -- pipe segment (InnerDiameter, Length3D, Material, CoverDepth)
///   AeccPressureFitting   -- fitting (elbow, tee, reducer, cap)
///   AeccPressureAppurtenance -- valve, hydrant, meter, ARV, etc.
///
/// The API surface lives in AeccDbMgd.dll. We access it via reflection so the
/// plugin builds without a direct assembly reference and tolerates minor
/// version differences between Civil 3D releases.
/// </summary>
public static class PressureNetworkCommands
{
  // -------------------------------------------------------------------------
  // listPressureNetworks
  // -------------------------------------------------------------------------

  public static Task<object?> ListPressureNetworksAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networks = EnumeratePressureNetworks(civilDoc, transaction, OpenMode.ForRead)
        .Select(net => ToPressureNetworkSummary(net, transaction))
        .ToList();

      return new Dictionary<string, object?> { ["networks"] = networks };
    });
  }

  // -------------------------------------------------------------------------
  // getPressureNetworkInfo
  // -------------------------------------------------------------------------

  public static Task<object?> GetPressureNetworkInfoAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, name, OpenMode.ForRead);

      var pipes = GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes")
        .Select(id => ToPressurePipeData(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead)))
        .ToList();

      var fittings = GetChildObjectIds(network, "GetFittingIds", "FittingIds", "Fittings")
        .Select(id => ToPressureFittingData(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead)))
        .ToList();

      var appurtenances = GetChildObjectIds(network, "GetAppurtenanceIds", "AppurtenanceIds", "Appurtenances")
        .Select(id => ToPressureAppurtenanceData(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead)))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["name"] = CivilObjectUtils.GetName(network) ?? name,
        ["handle"] = CivilObjectUtils.GetHandle(network),
        ["partsList"] = ResolveObjectName(transaction, GetAnyObjectId(network, "PartsListId", "CatalogId")),
        ["pipes"] = pipes,
        ["fittings"] = fittings,
        ["appurtenances"] = appurtenances,
      };
    });
  }

  // -------------------------------------------------------------------------
  // createPressureNetwork
  // -------------------------------------------------------------------------

  public static Task<object?> CreatePressureNetworkAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var partsListName = PluginRuntime.GetRequiredString(parameters, "partsList");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networkId = CreatePressureNetwork(civilDoc, name);
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, networkId, OpenMode.ForWrite);

      // Layer
      var layer = PluginRuntime.GetOptionalString(parameters, "layer");
      if (!string.IsNullOrWhiteSpace(layer) && network is Entity entity)
      {
        entity.Layer = layer;
      }

      // Parts list
      var partsListId = FindPressurePartsListId(civilDoc, transaction, partsListName);
      TrySetObjectIdProperty(network, partsListId, "PartsListId", "CatalogId");

      // Reference surface
      var referenceSurface = PluginRuntime.GetOptionalString(parameters, "referenceSurface");
      if (!string.IsNullOrWhiteSpace(referenceSurface))
      {
        var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, referenceSurface!, OpenMode.ForRead);
        TrySetObjectIdProperty(network, surface.ObjectId, "ReferenceSurfaceId", "SurfaceId");
      }

      // Reference alignment
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
        ["partsList"] = partsListName,
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // deletePressureNetwork
  // -------------------------------------------------------------------------

  public static Task<object?> DeletePressureNetworkAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, name, OpenMode.ForWrite);
      network.Erase();
      return new Dictionary<string, object?> { ["deleted"] = true, ["name"] = name };
    });
  }

  // -------------------------------------------------------------------------
  // assignPressurePartsList
  // -------------------------------------------------------------------------

  public static Task<object?> AssignPressurePartsListAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partsListName = PluginRuntime.GetRequiredString(parameters, "partsList");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var partsListId = FindPressurePartsListId(civilDoc, transaction, partsListName);
      TrySetObjectIdProperty(network, partsListId, "PartsListId", "CatalogId");

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["partsList"] = partsListName,
        ["assigned"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // setPressureNetworkCover
  // -------------------------------------------------------------------------

  public static Task<object?> SetPressureNetworkCoverAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var minCover = PluginRuntime.GetRequiredDouble(parameters, "minCoverDepth");
    var maxCover = PluginRuntime.GetOptionalDouble(parameters, "maxCoverDepth");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      TrySetDoubleProperty(network, minCover, "MinimumCoverDepth", "MinCoverDepth", "CoverDepthMin");
      if (maxCover.HasValue)
      {
        TrySetDoubleProperty(network, maxCover.Value, "MaximumCoverDepth", "MaxCoverDepth", "CoverDepthMax");
      }

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["minCoverDepth"] = minCover,
        ["maxCoverDepth"] = maxCover,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // validatePressureNetwork
  // -------------------------------------------------------------------------

  public static Task<object?> ValidatePressureNetworkAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      var issues = new List<Dictionary<string, object?>>();

      // Cover depth check
      var minCover = GetAnyDouble(network, "MinimumCoverDepth", "MinCoverDepth", "CoverDepthMin") ?? 0.0;
      foreach (var pipeId in GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes"))
      {
        var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, pipeId, OpenMode.ForRead);
        var cover = GetAnyDouble(pipe, "CoverDepth", "Cover", "MinCover");
        if (cover.HasValue && minCover > 0 && cover.Value < minCover)
        {
          issues.Add(new Dictionary<string, object?>
          {
            ["type"] = "cover_violation",
            ["severity"] = "error",
            ["message"] = $"Pipe '{CivilObjectUtils.GetName(pipe)}' has cover {cover.Value:F3} < minimum {minCover:F3}",
            ["objectHandle"] = CivilObjectUtils.GetHandle(pipe),
          });
        }
      }

      // Disconnected pipe ends (stub check)
      var fittingIds = GetChildObjectIds(network, "GetFittingIds", "FittingIds", "Fittings").ToHashSet();
      var appIds = GetChildObjectIds(network, "GetAppurtenanceIds", "AppurtenanceIds", "Appurtenances").ToHashSet();
      var connectedIds = fittingIds.Union(appIds).ToHashSet();

      var valid = issues.All(i => (string?)i["severity"] != "error");
      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["valid"] = valid,
        ["issues"] = issues,
      };
    });
  }

  // -------------------------------------------------------------------------
  // exportPressureNetwork
  // -------------------------------------------------------------------------

  public static Task<object?> ExportPressureNetworkAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var includeCoordinates = parameters?["includeCoordinates"]?.GetValue<bool>() ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);

      var pipes = GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes")
        .Select(id => ExportPressurePipe(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead), includeCoordinates))
        .ToList();

      var fittings = GetChildObjectIds(network, "GetFittingIds", "FittingIds", "Fittings")
        .Select(id => ExportPressureFitting(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead), includeCoordinates))
        .ToList();

      var appurtenances = GetChildObjectIds(network, "GetAppurtenanceIds", "AppurtenanceIds", "Appurtenances")
        .Select(id => ExportPressureAppurtenance(CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead), includeCoordinates))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["handle"] = CivilObjectUtils.GetHandle(network),
        ["partsList"] = ResolveObjectName(transaction, GetAnyObjectId(network, "PartsListId", "CatalogId")),
        ["pipes"] = pipes,
        ["fittings"] = fittings,
        ["appurtenances"] = appurtenances,
        ["exportedAt"] = DateTime.UtcNow.ToString("O"),
      };
    });
  }

  // -------------------------------------------------------------------------
  // connectPressureNetworks
  // -------------------------------------------------------------------------

  public static Task<object?> ConnectPressureNetworksAsync(JsonObject? parameters)
  {
    var targetName = PluginRuntime.GetRequiredString(parameters, "targetNetwork");
    var sourceName = PluginRuntime.GetRequiredString(parameters, "sourceNetwork");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var target = FindPressureNetworkByName(civilDoc, transaction, targetName, OpenMode.ForWrite);
      var source = FindPressureNetworkByName(civilDoc, transaction, sourceName, OpenMode.ForWrite);

      // Attempt native merge via reflection first
      var merged = TryMergeNetworks(target, source);
      if (!merged)
      {
        // Fallback: re-parent all source components to target by changing their network ID property
        foreach (var pipeId in GetChildObjectIds(source, "GetPipeIds", "PipeIds", "Pipes"))
        {
          var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, pipeId, OpenMode.ForWrite);
          TrySetObjectIdProperty(pipe, target.ObjectId, "NetworkId", "PressureNetworkId");
        }
        foreach (var fittingId in GetChildObjectIds(source, "GetFittingIds", "FittingIds", "Fittings"))
        {
          var fitting = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, fittingId, OpenMode.ForWrite);
          TrySetObjectIdProperty(fitting, target.ObjectId, "NetworkId", "PressureNetworkId");
        }
        foreach (var appId in GetChildObjectIds(source, "GetAppurtenanceIds", "AppurtenanceIds", "Appurtenances"))
        {
          var app = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, appId, OpenMode.ForWrite);
          TrySetObjectIdProperty(app, target.ObjectId, "NetworkId", "PressureNetworkId");
        }
        source.Erase();
      }

      return new Dictionary<string, object?>
      {
        ["targetNetwork"] = targetName,
        ["sourceNetwork"] = sourceName,
        ["connected"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // addPressurePipe
  // -------------------------------------------------------------------------

  public static Task<object?> AddPressurePipeAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var startPoint = ReadPoint(parameters, "startPoint") ?? Point3d.Origin;
    var endPoint = ReadPoint(parameters, "endPoint") ?? new Point3d(1, 0, 0);
    var diameter = PluginRuntime.GetOptionalDouble(parameters, "diameter");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var createdId = AddPressurePipeToNetwork(network, transaction, partName, startPoint, endPoint, diameter);
      var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, createdId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["pipe"] = ToPressurePipeData(pipe),
        ["added"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getPressurePipeProperties
  // -------------------------------------------------------------------------

  public static Task<object?> GetPressurePipePropertiesAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var pipeName = PluginRuntime.GetRequiredString(parameters, "pipeName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      var pipe = FindPressureComponentByName(network, transaction, pipeName, "GetPipeIds", "PipeIds", "Pipes");
      return ToPressurePipeData(pipe);
    });
  }

  // -------------------------------------------------------------------------
  // resizePressurePipe
  // -------------------------------------------------------------------------

  public static Task<object?> ResizePressurePipeAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var pipeName = PluginRuntime.GetRequiredString(parameters, "pipeName");
    var newPartName = PluginRuntime.GetRequiredString(parameters, "newPartName");
    var newDiameter = PluginRuntime.GetOptionalDouble(parameters, "newDiameter");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var pipe = FindPressureComponentByName(network, transaction, pipeName, "GetPipeIds", "PipeIds", "Pipes");
      pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, pipe.ObjectId, OpenMode.ForWrite);

      // Try setting part via reflection
      var partsListId = GetAnyObjectId(network, "PartsListId", "CatalogId");
      if (partsListId != ObjectId.Null)
      {
        var partId = FindPressurePartId(transaction, partsListId, newPartName);
        TrySetObjectIdProperty(pipe, partId, "PartId", "PartFamilyId");
      }

      if (newDiameter.HasValue)
      {
        TrySetDoubleProperty(pipe, newDiameter.Value, "InnerDiameter", "Diameter", "InnerDiameterOrWidth");
      }

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["pipeName"] = pipeName,
        ["newPartName"] = newPartName,
        ["resized"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // addPressureFitting
  // -------------------------------------------------------------------------

  public static Task<object?> AddPressureFittingAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var position = ReadPoint(parameters, "position") ?? Point3d.Origin;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0.0;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);
      var createdId = AddPressureComponentToNetwork(network, transaction, partName, position, rotation, isFitting: true);
      var fitting = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, createdId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["fitting"] = ToPressureFittingData(fitting),
        ["added"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getPressureFittingProperties
  // -------------------------------------------------------------------------

  public static Task<object?> GetPressureFittingPropertiesAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var fittingName = PluginRuntime.GetRequiredString(parameters, "fittingName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForRead);
      var fitting = FindPressureComponentByName(network, transaction, fittingName, "GetFittingIds", "FittingIds", "Fittings");
      return ToPressureFittingData(fitting);
    });
  }

  // -------------------------------------------------------------------------
  // addPressureAppurtenance
  // -------------------------------------------------------------------------

  public static Task<object?> AddPressureAppurtenanceAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var position = ReadPoint(parameters, "position") ?? Point3d.Origin;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0.0;
    var onPipeName = PluginRuntime.GetOptionalString(parameters, "onPipeName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPressureNetworkByName(civilDoc, transaction, networkName, OpenMode.ForWrite);

      // If onPipeName specified, find the pipe and snap position to its midpoint
      if (!string.IsNullOrWhiteSpace(onPipeName))
      {
        try
        {
          var pipe = FindPressureComponentByName(network, transaction, onPipeName!, "GetPipeIds", "PipeIds", "Pipes");
          var sp = GetPointProperty(pipe, "StartPoint", "PointAtStart");
          var ep = GetPointProperty(pipe, "EndPoint", "PointAtEnd");
          if (sp.HasValue && ep.HasValue)
          {
            position = new Point3d(
              (sp.Value.X + ep.Value.X) / 2,
              (sp.Value.Y + ep.Value.Y) / 2,
              (sp.Value.Z + ep.Value.Z) / 2
            );
          }
        }
        catch
        {
          // Non-fatal - use provided position
        }
      }

      var createdId = AddPressureComponentToNetwork(network, transaction, partName, position, rotation, isFitting: false);
      var app = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, createdId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["appurtenance"] = ToPressureAppurtenanceData(app),
        ["added"] = true,
      };
    });
  }

  // =========================================================================
  // Private helpers
  // =========================================================================

  private static IEnumerable<DBObject> EnumeratePressureNetworks(object civilDoc, Transaction transaction, OpenMode openMode)
  {
    foreach (var objectId in GetPressureNetworkIds(civilDoc))
    {
      yield return CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
    }
  }

  private static IEnumerable<ObjectId> GetPressureNetworkIds(object civilDoc)
  {
    var candidates = new object?[]
    {
      CivilObjectUtils.InvokeMethod(civilDoc, "GetPressureNetworkIds"),
      GetNamedMember(civilDoc, "PressureNetworkCollection"),
      GetNamedMember(civilDoc, "PressureNetworks"),
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

  private static DBObject FindPressureNetworkByName(object civilDoc, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (var network in EnumeratePressureNetworks(civilDoc, transaction, OpenMode.ForRead))
    {
      if (!string.Equals(CivilObjectUtils.GetName(network), name, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      if (openMode == OpenMode.ForWrite)
      {
        return CivilObjectUtils.GetRequiredObject<DBObject>(transaction, network.ObjectId, OpenMode.ForWrite);
      }

      return network;
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pressure network '{name}' was not found.");
  }

  private static DBObject FindPressureComponentByName(
    DBObject network,
    Transaction transaction,
    string componentName,
    params string[] idMethods)
  {
    foreach (var objectId in GetChildObjectIds(network, idMethods))
    {
      var component = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(component), componentName, StringComparison.OrdinalIgnoreCase))
      {
        return component;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
      $"Component '{componentName}' was not found in network '{CivilObjectUtils.GetName(network)}'.");
  }

  private static Dictionary<string, object?> ToPressureNetworkSummary(DBObject network, Transaction transaction)
  {
    var pipeIds = GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes").ToList();
    var fittingIds = GetChildObjectIds(network, "GetFittingIds", "FittingIds", "Fittings").ToList();
    var appIds = GetChildObjectIds(network, "GetAppurtenanceIds", "AppurtenanceIds", "Appurtenances").ToList();

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(network) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(network),
      ["pipeCount"] = pipeIds.Count,
      ["fittingCount"] = fittingIds.Count,
      ["appurtenanceCount"] = appIds.Count,
      ["partsList"] = ResolveObjectName(transaction, GetAnyObjectId(network, "PartsListId", "CatalogId")),
    };
  }

  private static Dictionary<string, object?> ToPressurePipeData(DBObject pipe)
  {
    var sp = GetPointProperty(pipe, "StartPoint", "PointAtStart");
    var ep = GetPointProperty(pipe, "EndPoint", "PointAtEnd");

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(pipe) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(pipe),
      ["diameter"] = GetAnyDouble(pipe, "InnerDiameter", "InnerDiameterOrWidth", "Diameter") ?? 0.0,
      ["length"] = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? 0.0,
      ["material"] = GetAnyString(pipe, "Material", "PartFamilyName", "PartDescription") ?? string.Empty,
      ["startPoint"] = sp.HasValue ? new Dictionary<string, object?> { ["x"] = sp.Value.X, ["y"] = sp.Value.Y, ["z"] = sp.Value.Z } : null,
      ["endPoint"] = ep.HasValue ? new Dictionary<string, object?> { ["x"] = ep.Value.X, ["y"] = ep.Value.Y, ["z"] = ep.Value.Z } : null,
      ["coverDepth"] = GetAnyDouble(pipe, "CoverDepth", "Cover", "MinCover"),
    };
  }

  private static Dictionary<string, object?> ToPressureFittingData(DBObject fitting)
  {
    var pos = GetPointProperty(fitting, "Position", "Location", "CenterPoint");

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(fitting) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(fitting),
      ["type"] = GetAnyString(fitting, "FittingType", "PartType", "PartDescription") ?? fitting.GetType().Name,
      ["position"] = pos.HasValue ? new Dictionary<string, object?> { ["x"] = pos.Value.X, ["y"] = pos.Value.Y, ["z"] = pos.Value.Z } : null,
      ["partSize"] = GetAnyString(fitting, "PartSize", "SizeName", "PartFamilyName"),
    };
  }

  private static Dictionary<string, object?> ToPressureAppurtenanceData(DBObject app)
  {
    var pos = GetPointProperty(app, "Position", "Location", "CenterPoint");

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(app) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(app),
      ["type"] = GetAnyString(app, "AppurtenanceType", "PartType", "PartDescription") ?? app.GetType().Name,
      ["position"] = pos.HasValue ? new Dictionary<string, object?> { ["x"] = pos.Value.X, ["y"] = pos.Value.Y, ["z"] = pos.Value.Z } : null,
      ["partSize"] = GetAnyString(app, "PartSize", "SizeName", "PartFamilyName"),
    };
  }

  private static Dictionary<string, object?> ExportPressurePipe(DBObject pipe, bool includeCoords)
  {
    var data = ToPressurePipeData(pipe);
    if (!includeCoords)
    {
      data.Remove("startPoint");
      data.Remove("endPoint");
    }

    return data;
  }

  private static Dictionary<string, object?> ExportPressureFitting(DBObject fitting, bool includeCoords)
  {
    var data = ToPressureFittingData(fitting);
    if (!includeCoords)
    {
      data.Remove("position");
    }

    return data;
  }

  private static Dictionary<string, object?> ExportPressureAppurtenance(DBObject app, bool includeCoords)
  {
    var data = ToPressureAppurtenanceData(app);
    if (!includeCoords)
    {
      data.Remove("position");
    }

    return data;
  }

  private static ObjectId CreatePressureNetwork(object civilDoc, string name)
  {
    foreach (var typeName in new[]
    {
      "Autodesk.Civil.DatabaseServices.PressureNetwork",
      "Autodesk.Civil.DatabaseServices.AeccPressureNetwork",
    })
    {
      var type = Type.GetType($"{typeName}, AeccDbMgd", false)
        ?? AppDomain.CurrentDomain.GetAssemblies()
             .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
             .FirstOrDefault(t => t.FullName == typeName);

      if (type == null)
      {
        continue;
      }

      foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name == "Create")
        .OrderBy(m => m.GetParameters().Length))
      {
        var args = BuildCreateNetworkArgs(method.GetParameters(), civilDoc, name);
        if (args == null)
        {
          continue;
        }

        try
        {
          var result = method.Invoke(null, args);
          if (result is ObjectId id && id != ObjectId.Null)
          {
            return id;
          }
        }
        catch
        {
          // Try next overload
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to create a pressure network — no matching Civil 3D API overload found.");
  }

  private static object?[]? BuildCreateNetworkArgs(ParameterInfo[] parameters, object civilDoc, string name)
  {
    var args = new object?[parameters.Length];
    for (var i = 0; i < parameters.Length; i++)
    {
      var paramType = parameters[i].ParameterType.IsByRef
        ? parameters[i].ParameterType.GetElementType()!
        : parameters[i].ParameterType;

      if (paramType.Name == "CivilDocument") { args[i] = civilDoc; continue; }
      if (paramType == typeof(string)) { args[i] = name; continue; }
      if (paramType == typeof(bool)) { args[i] = false; continue; }
      if (paramType == typeof(int)) { args[i] = 0; continue; }
      if (paramType == typeof(ObjectId)) { args[i] = ObjectId.Null; continue; }

      return null;
    }

    return args;
  }

  private static ObjectId AddPressurePipeToNetwork(DBObject network, Transaction transaction, string partName, Point3d startPoint, Point3d endPoint, double? diameter)
  {
    var partsListId = GetAnyObjectId(network, "PartsListId", "CatalogId");
    var partId = partsListId != ObjectId.Null ? FindPressurePartId(transaction, partsListId, partName) : ObjectId.Null;

    foreach (var method in network.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(m => m.Name.Contains("AddPipe", StringComparison.OrdinalIgnoreCase))
      .OrderBy(m => m.GetParameters().Length))
    {
      var args = BuildAddPipeArgs(method.GetParameters(), partId, startPoint, endPoint, diameter ?? 0.0);
      if (args == null) continue;

      try
      {
        var result = method.Invoke(network, args);
        var id = ExtractObjectId(result, args);
        if (id != ObjectId.Null) return id;
      }
      catch { }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add pressure pipe '{partName}' to network.");
  }

  private static object?[]? BuildAddPipeArgs(ParameterInfo[] parameters, ObjectId partId, Point3d start, Point3d end, double diameter)
  {
    var args = new object?[parameters.Length];
    var points = new Queue<Point3d>(new[] { start, end });
    var doubles = new Queue<double>(new[] { diameter, 0.0, 0.0 });
    var usedPart = false;

    for (var i = 0; i < parameters.Length; i++)
    {
      var pType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (pType == typeof(ObjectId))
      {
        if (!usedPart && partId != ObjectId.Null) { args[i] = partId; usedPart = true; }
        else { args[i] = ObjectId.Null; }
        continue;
      }

      if (pType == typeof(Point3d)) { args[i] = points.Count > 0 ? points.Dequeue() : Point3d.Origin; continue; }
      if (pType == typeof(double)) { args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0; continue; }
      if (pType == typeof(bool)) { args[i] = false; continue; }
      if (pType == typeof(int)) { args[i] = 0; continue; }
      if (pType == typeof(string)) { args[i] = string.Empty; continue; }

      return null;
    }

    return args;
  }

  private static ObjectId AddPressureComponentToNetwork(DBObject network, Transaction transaction, string partName, Point3d position, double rotation, bool isFitting)
  {
    var partsListId = GetAnyObjectId(network, "PartsListId", "CatalogId");
    var partId = partsListId != ObjectId.Null ? FindPressurePartId(transaction, partsListId, partName) : ObjectId.Null;
    var methodToken = isFitting ? "AddFitting" : "AddAppurtenance";

    foreach (var method in network.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(m => m.Name.Contains(methodToken, StringComparison.OrdinalIgnoreCase))
      .OrderBy(m => m.GetParameters().Length))
    {
      var args = BuildAddComponentArgs(method.GetParameters(), partId, position, rotation);
      if (args == null) continue;

      try
      {
        var result = method.Invoke(network, args);
        var id = ExtractObjectId(result, args);
        if (id != ObjectId.Null) return id;
      }
      catch { }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add '{partName}' to pressure network.");
  }

  private static object?[]? BuildAddComponentArgs(ParameterInfo[] parameters, ObjectId partId, Point3d position, double rotation)
  {
    var args = new object?[parameters.Length];
    var usedPart = false;
    var usedPos = false;

    for (var i = 0; i < parameters.Length; i++)
    {
      var pType = parameters[i].ParameterType.IsByRef ? parameters[i].ParameterType.GetElementType()! : parameters[i].ParameterType;

      if (pType == typeof(ObjectId))
      {
        if (!usedPart && partId != ObjectId.Null) { args[i] = partId; usedPart = true; }
        else { args[i] = ObjectId.Null; }
        continue;
      }

      if (pType == typeof(Point3d)) { args[i] = position; usedPos = true; continue; }
      if (pType == typeof(double)) { args[i] = rotation; continue; }
      if (pType == typeof(bool)) { args[i] = false; continue; }
      if (pType == typeof(int)) { args[i] = 0; continue; }
      if (pType == typeof(string)) { args[i] = string.Empty; continue; }

      return null;
    }

    _ = usedPos;
    return args;
  }

  private static bool TryMergeNetworks(DBObject target, DBObject source)
  {
    foreach (var methodName in new[] { "MergeNetwork", "Merge", "AddNetwork" })
    {
      var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
      if (method == null) continue;

      try
      {
        method.Invoke(target, new object[] { source.ObjectId });
        return true;
      }
      catch { }
    }

    return false;
  }

  private static ObjectId FindPressurePartsListId(object civilDoc, Transaction transaction, string partsListName)
  {
    foreach (var collectionName in new[] { "PressurePartsLists", "PresPartsLists", "PressurePipePartsList" })
    {
      var styles = GetNamedMember(civilDoc, "Styles");
      var collection = GetNamedMember(styles, collectionName) ?? GetNamedMember(civilDoc, collectionName);
      foreach (var objectId in ToObjectIdsFlexible(collection))
      {
        var obj = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(obj), partsListName, StringComparison.OrdinalIgnoreCase))
        {
          return objectId;
        }
      }
    }

    // Fallback: look in any parts list collection
    foreach (var collectionName in new[] { "PartsListSet", "PartsLists", "PartsListCollection", "PartsListStyles" })
    {
      var styles = GetNamedMember(civilDoc, "Styles");
      var collection = GetNamedMember(styles, collectionName) ?? GetNamedMember(civilDoc, collectionName);
      foreach (var objectId in ToObjectIdsFlexible(collection))
      {
        var obj = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(obj), partsListName, StringComparison.OrdinalIgnoreCase))
        {
          return objectId;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pressure parts list '{partsListName}' was not found.");
  }

  private static ObjectId FindPressurePartId(Transaction transaction, ObjectId partsListId, string partName)
  {
    var partsList = transaction.GetObject(partsListId, OpenMode.ForRead);

    foreach (var collectionName in new[] { "PipeFamilies", "FittingFamilies", "AppurtenanceFamilies", "PartFamilies", "PartFamilySet" })
    {
      var collection = GetNamedMember(partsList, collectionName);
      foreach (var family in EnumerateObjects(collection))
      {
        var familyId = GetAnyObjectId(family, "ObjectId", "Id");
        if (string.Equals(CivilObjectUtils.GetName(family), partName, StringComparison.OrdinalIgnoreCase) && familyId != ObjectId.Null)
        {
          return familyId;
        }

        foreach (var size in EnumerateObjects(GetNamedMember(family, "PartSizes") ?? GetNamedMember(family, "SizeDataRecords") ?? GetNamedMember(family, "PartSizeFilter")))
        {
          var sizeId = GetAnyObjectId(size, "ObjectId", "Id");
          var sizeName = CivilObjectUtils.GetName(size) ?? CivilObjectUtils.GetStringProperty(size, "Description");
          if (sizeId != ObjectId.Null && string.Equals(sizeName, partName, StringComparison.OrdinalIgnoreCase))
          {
            return sizeId;
          }
        }
      }
    }

    return ObjectId.Null;
  }

  private static IEnumerable<ObjectId> GetChildObjectIds(object owner, params string[] memberNames)
  {
    foreach (var memberName in memberNames)
    {
      var value = CivilObjectUtils.InvokeMethod(owner, memberName) ?? GetNamedMember(owner, memberName);
      foreach (var id in ToObjectIdsFlexible(value))
      {
        if (id != ObjectId.Null) yield return id;
      }
    }
  }

  private static IEnumerable<ObjectId> ToObjectIdsFlexible(object? value)
  {
    foreach (var id in CivilObjectUtils.ToObjectIds(value))
    {
      yield return id;
    }

    if (value is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item is ObjectId oid && oid != ObjectId.Null) { yield return oid; continue; }
        var itemId = GetAnyObjectId(item, "ObjectId", "Id");
        if (itemId != ObjectId.Null) yield return itemId;
      }
    }
  }

  private static IEnumerable<object> EnumerateObjects(object? collection)
  {
    if (collection is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item != null) yield return item;
      }
    }
  }

  private static object? GetNamedMember(object? value, string memberName)
  {
    if (value == null) return null;
    var prop = value.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
    if (prop != null) return prop.GetValue(value);
    var field = value.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
    return field?.GetValue(value);
  }

  private static string? ResolveObjectName(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null) return null;
    try { return CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead)); }
    catch { return null; }
  }

  private static ObjectId GetAnyObjectId(object? value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var id = CivilObjectUtils.GetPropertyValue<ObjectId>(value, name);
      if (id != ObjectId.Null) return id;
    }

    return ObjectId.Null;
  }

  private static double? GetAnyDouble(object? value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var v = CivilObjectUtils.GetPropertyValue<double?>(value, name);
      if (v.HasValue) return v.Value;
    }

    return null;
  }

  private static string? GetAnyString(object? value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var v = CivilObjectUtils.GetStringProperty(value, name);
      if (!string.IsNullOrWhiteSpace(v)) return v;
    }

    return null;
  }

  private static Point3d? GetPointProperty(object? value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var v = CivilObjectUtils.GetPropertyValue<Point3d?>(value, name);
      if (v.HasValue) return v.Value;
    }

    return null;
  }

  private static void TrySetObjectIdProperty(object target, ObjectId objectId, params string[] propertyNames)
  {
    if (objectId == ObjectId.Null) return;
    foreach (var name in propertyNames)
    {
      var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || !prop.CanWrite || prop.PropertyType != typeof(ObjectId)) continue;
      try { prop.SetValue(target, objectId); return; } catch { }
    }
  }

  private static void TrySetDoubleProperty(object target, double value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || !prop.CanWrite) continue;
      if (prop.PropertyType != typeof(double) && prop.PropertyType != typeof(double?)) continue;
      try { prop.SetValue(target, value); return; } catch { }
    }
  }

  private static ObjectId ExtractObjectId(object? result, object?[] args)
  {
    if (result is ObjectId id && id != ObjectId.Null) return id;
    var rid = GetAnyObjectId(result, "ObjectId", "Id");
    if (rid != ObjectId.Null) return rid;
    foreach (var arg in args)
    {
      var aid = GetAnyObjectId(arg, "ObjectId", "Id");
      if (aid != ObjectId.Null) return aid;
    }

    return ObjectId.Null;
  }

  private static Point3d? ReadPoint(JsonObject? parameters, string paramName)
  {
    if (PluginRuntime.GetParameter(parameters, paramName) is not JsonObject node) return null;
    return new Point3d(
      node["x"]?.GetValue<double>() ?? 0.0,
      node["y"]?.GetValue<double>() ?? 0.0,
      node["z"]?.GetValue<double>() ?? 0.0
    );
  }
}
