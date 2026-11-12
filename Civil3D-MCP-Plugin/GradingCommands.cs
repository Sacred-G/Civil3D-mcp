using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for Civil 3D Grading MCP tools.
///
/// Civil 3D API notes (reflection-based late binding):
///   AeccGradingGroup   -- container for one or more gradings, owns a surface
///   AeccGrading        -- a single grading object (criteria + target)
///   AeccGradingCriteria / AeccGradingCriteriaSet -- style/criteria definitions
///
/// We access the Grading API via reflection so the plugin builds without a
/// direct AeccDbMgd.dll reference and tolerates minor version differences.
/// </summary>
public static class GradingCommands
{
  // -------------------------------------------------------------------------
  // listGradingGroups
  // -------------------------------------------------------------------------

  public static Task<object?> ListGradingGroupsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groups = EnumerateGradingGroups(civilDoc, transaction)
        .Select(g => ToGradingGroupSummary(g, transaction))
        .ToList();

      return new Dictionary<string, object?> { ["groups"] = groups };
    });
  }

  // -------------------------------------------------------------------------
  // getGradingGroup
  // -------------------------------------------------------------------------

  public static Task<object?> GetGradingGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, name, OpenMode.ForRead);
      return ToGradingGroupDetail(group, transaction);
    });
  }

  // -------------------------------------------------------------------------
  // createGradingGroup
  // -------------------------------------------------------------------------

  public static Task<object?> CreateGradingGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description") ?? string.Empty;
    var useProjection = PluginRuntime.GetOptionalBool(parameters, "useProjection") ?? false;
    // Optional: name the site the group is created in. If it does not exist yet, a site with that
    // name is created and used, so a fresh drawing with no sites does not need a separate
    // create-site step.
    var siteParam = PluginRuntime.GetOptionalString(parameters, "site");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var siteId = ObjectId.Null;
      var siteCreated = false;

      if (!string.IsNullOrWhiteSpace(siteParam))
      {
        // A requested site: found by name, or created when it does not exist yet.
        siteId = LookupUtils.GetSiteId(civilDoc, transaction, siteParam);
        if (siteId == ObjectId.Null)
        {
          siteId = Autodesk.Civil.DatabaseServices.Site.Create(civilDoc, siteParam);
          siteCreated = true;
        }
      }
      else
      {
        // No site requested: the first one, or a new one when the drawing has none.
        siteId = civilDoc.GetSiteIds().Cast<ObjectId>().FirstOrDefault();
        if (siteId == ObjectId.Null)
        {
          siteId = Autodesk.Civil.DatabaseServices.Site.Create(civilDoc, name + " Site");
          siteCreated = true;
        }
      }

      var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
      var gradingGroups = Civil3DCompatibility.GetPropertyValue(siteObj, "GradingGroups");

      // Try Add(name, useProjection) or Add(name)
      object? newGroupId = null;
      if (!Civil3DCompatibility.TryInvokeMethod(gradingGroups, "Add", out newGroupId, name, useProjection))
        Civil3DCompatibility.TryInvokeMethod(gradingGroups, "Add", out newGroupId, name);

      if (newGroupId == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Failed to create grading group — Add method not found.");
      }

      var newGroupObjectId = (ObjectId)newGroupId;
      var newGroup = transaction.GetObject(newGroupObjectId, OpenMode.ForWrite);

      Civil3DCompatibility.TrySetProperty(newGroup, "Description", description);

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["handle"] = CivilObjectUtils.GetHandle(newGroup),
        ["description"] = description,
        ["site"] = CivilObjectUtils.GetName(siteObj),
        ["siteCreated"] = siteCreated,
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // deleteGradingGroup
  // -------------------------------------------------------------------------

  public static Task<object?> DeleteGradingGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, name, OpenMode.ForWrite);
      group.Erase();

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getGradingGroupVolume
  // -------------------------------------------------------------------------

  public static Task<object?> GetGradingGroupVolumeAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, name, OpenMode.ForRead);

      var cutVolume = CivilObjectUtils.GetDoubleProperty(group, "CutVolume");
      var fillVolume = CivilObjectUtils.GetDoubleProperty(group, "FillVolume");
      if (!cutVolume.HasValue || !fillVolume.HasValue)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Grading group '{name}' does not expose readable cut/fill volumes. Zero volumes were not substituted.");
      var netVolume = cutVolume.Value - fillVolume.Value;

      return new Dictionary<string, object?>
      {
        ["groupName"] = name,
        ["cutVolume"] = cutVolume.Value,
        ["fillVolume"] = fillVolume.Value,
        ["netVolume"] = netVolume,
        ["units"] = new Dictionary<string, string> { ["volume"] = CivilObjectUtils.VolumeUnits(database) },
      };
    });
  }

  // -------------------------------------------------------------------------
  // createSurfaceFromGradingGroup
  // -------------------------------------------------------------------------

  public static Task<object?> CreateSurfaceFromGradingGroupAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var surfaceName = PluginRuntime.GetOptionalString(parameters, "surfaceName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, name, OpenMode.ForWrite);

      // CreateSurface() or CreateSurface(surfaceName) depending on API version
      object? resultId;
      var invoked = surfaceName != null
        ? Civil3DCompatibility.TryInvokeMethod(group, "CreateSurface", out resultId, surfaceName)
        : Civil3DCompatibility.TryInvokeMethod(group, "CreateSurface", out resultId);
      if (!invoked)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "CreateSurface method not found on grading group.");

      return new Dictionary<string, object?>
      {
        ["groupName"] = name,
        ["surfaceCreated"] = true,
        ["surfaceObjectId"] = resultId?.ToString(),
      };
    });
  }

  // -------------------------------------------------------------------------
  // listGradings
  // -------------------------------------------------------------------------

  public static Task<object?> ListGradingsAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, groupName, OpenMode.ForRead);
      var gradings = GetGradingsFromGroup(group, transaction)
        .Select(g => ToGradingSummary(g))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["groupName"] = groupName,
        ["gradings"] = gradings,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getGrading
  // -------------------------------------------------------------------------

  public static Task<object?> GetGradingAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");
    var handle = PluginRuntime.GetRequiredString(parameters, "handle");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, groupName, OpenMode.ForRead);
      var grading = GetGradingsFromGroup(group, transaction)
        .FirstOrDefault(g => CivilObjectUtils.GetHandle(g) == handle)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Grading with handle '{handle}' not found in group '{groupName}'.");

      return ToGradingDetail(grading);
    });
  }

  // -------------------------------------------------------------------------
  // createGrading
  // -------------------------------------------------------------------------

  public static Task<object?> CreateGradingAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");
    var featureLineName = PluginRuntime.GetRequiredString(parameters, "featureLineName");
    var criteriaName = PluginRuntime.GetOptionalString(parameters, "criteriaName");
    var side = PluginRuntime.GetOptionalString(parameters, "side") ?? "right"; // left | right | both

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, groupName, OpenMode.ForWrite);

      // Find the feature line by name
      var featureLine = FindFeatureLineByName(civilDoc, transaction, featureLineName, database)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Feature line '{featureLineName}' not found.");

      // Try to invoke AddGrading on the group
      var featureLineId = featureLine.ObjectId;
      object? gradingId;
      var invoked = criteriaName != null
        ? Civil3DCompatibility.TryInvokeMethod(group, "AddGrading", out gradingId, featureLineId, FindGradingCriteriaId(civilDoc, transaction, criteriaName), side)
          || Civil3DCompatibility.TryInvokeMethod(group, "CreateGrading", out gradingId, featureLineId, FindGradingCriteriaId(civilDoc, transaction, criteriaName), side)
        : Civil3DCompatibility.TryInvokeMethod(group, "AddGrading", out gradingId, featureLineId, side)
          || Civil3DCompatibility.TryInvokeMethod(group, "CreateGrading", out gradingId, featureLineId, side);
      if (!invoked)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "AddGrading/CreateGrading method not found on grading group.");

      return new Dictionary<string, object?>
      {
        ["groupName"] = groupName,
        ["featureLineName"] = featureLineName,
        ["gradingHandle"] = gradingId?.ToString(),
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // deleteGrading
  // -------------------------------------------------------------------------

  public static Task<object?> DeleteGradingAsync(JsonObject? parameters)
  {
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");
    var handle = PluginRuntime.GetRequiredString(parameters, "handle");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var group = FindGradingGroupByName(civilDoc, transaction, groupName, OpenMode.ForRead);
      var grading = GetGradingsFromGroup(group, transaction)
        .FirstOrDefault(g => CivilObjectUtils.GetHandle(g) == handle)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Grading with handle '{handle}' not found in group '{groupName}'.");

      // Open for write and erase
      var writableGrading = transaction.GetObject(grading.ObjectId, OpenMode.ForWrite);
      writableGrading.Erase();

      return new Dictionary<string, object?>
      {
        ["handle"] = handle,
        ["deleted"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // listGradingCriteria
  // -------------------------------------------------------------------------

  public static Task<object?> ListGradingCriteriaAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var criteriaList = new List<Dictionary<string, object?>>();

      // Try GradingCriteriaSets property path
      var criteriaSets = Civil3DCompatibility.GetPropertyValue(civilDoc, "GradingCriteriaSets");

      foreach (var setId in CivilObjectUtils.ToObjectIds(criteriaSets))
      {
        var setObj = transaction.GetObject(setId, OpenMode.ForRead);
        var setName = CivilObjectUtils.GetName(setObj) ?? string.Empty;

        var criteriaIds = Civil3DCompatibility.GetPropertyValue(setObj, "CriteriaIds")
          ?? Civil3DCompatibility.GetPropertyValue(setObj, "Criteria");

        foreach (var criteriaId in CivilObjectUtils.ToObjectIds(criteriaIds))
        {
          var criteriaObj = transaction.GetObject(criteriaId, OpenMode.ForRead);
          criteriaList.Add(new Dictionary<string, object?>
          {
            ["setName"] = setName,
            ["name"] = CivilObjectUtils.GetName(criteriaObj),
            ["handle"] = CivilObjectUtils.GetHandle(criteriaObj),
            ["description"] = CivilObjectUtils.GetStringProperty(criteriaObj, "Description"),
          });
        }
      }

      return new Dictionary<string, object?> { ["criteriaList"] = criteriaList };
    });
  }

  // -------------------------------------------------------------------------
  // listFeatureLines
  // -------------------------------------------------------------------------

  public static Task<object?> ListFeatureLinesAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var featureLines = EnumerateFeatureLines(database, transaction)
        .Select(featureLine => ToFeatureLineSummary(featureLine, transaction))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["featureLines"] = featureLines,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getFeatureLine
  // -------------------------------------------------------------------------

  public static Task<object?> GetFeatureLineAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var featureLine = FindFeatureLineByName(civilDoc, transaction, name, database)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Feature line '{name}' not found.");

      return ToFeatureLineDetail(featureLine, database, transaction);
    });
  }

  // -------------------------------------------------------------------------
  // exportFeatureLineAsPolyline
  // -------------------------------------------------------------------------

  public static Task<object?> ExportFeatureLineAsPolylineAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var targetLayer = PluginRuntime.GetOptionalString(parameters, "targetLayer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var featureLine = FindFeatureLineByName(civilDoc, transaction, name, database)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Feature line '{name}' not found.");

      var vertices = GetFeatureLineVertices(featureLine);
      if (vertices.Count < 2)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Feature line '{name}' does not expose enough vertices to export.");
      }

      var modelSpaceId = CivilObjectUtils.GetModelSpaceBlockId(database, transaction);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, modelSpaceId, OpenMode.ForWrite);
      var polylinePoints = new Point3dCollection();
      foreach (var vertex in vertices)
      {
        polylinePoints.Add(vertex);
      }

      using var polyline3d = new Polyline3d(Poly3dType.SimplePoly, polylinePoints, false);
      var polylineId = modelSpace.AppendEntity(polyline3d);
      transaction.AddNewlyCreatedDBObject(polyline3d, true);

      var resolvedLayer = targetLayer
        ?? CivilObjectUtils.GetStringProperty(featureLine, "Layer")
        ?? "0";
      CivilObjectUtils.TrySetLayer(polyline3d, resolvedLayer, database, transaction);

      var createdPolyline = CivilObjectUtils.GetRequiredObject<Polyline3d>(transaction, polylineId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["sourceFeatureLineName"] = CivilObjectUtils.GetName(featureLine) ?? name,
        ["sourceFeatureLineHandle"] = CivilObjectUtils.GetHandle(featureLine),
        ["polylineHandle"] = CivilObjectUtils.GetHandle(createdPolyline),
        ["targetLayer"] = resolvedLayer,
        ["vertexCount"] = vertices.Count,
        ["exported"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // createFeatureLine
  // -------------------------------------------------------------------------

  public static Task<object?> CreateFeatureLineAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetOptionalString(parameters, "name");
    var layer = PluginRuntime.GetOptionalString(parameters, "layer") ?? "0";
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;

    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createFeatureLine requires at least 2 points.");
    }

    var points = pointsNode.Select(node =>
    {
      if (node is not JsonObject pt)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Each point must be a JSON object with x, y, z.");
      }
      var x = pt["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point missing x.");
      var y = pt["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point missing y.");
      var z = pt["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Feature-line point missing z; elevation 0 will not be assumed.");
      return new Point3d(x, y, z);
    }).ToList();

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Get or find a site to add the feature line to
      var siteIds = civilDoc.GetSiteIds().Cast<ObjectId>();
      var firstSiteId = siteIds.FirstOrDefault();

      // Build a Point3dCollection for the feature line
      var pointCollection = new Point3dCollection();
      foreach (var pt in points)
      {
        pointCollection.Add(pt);
      }

      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, database.CurrentSpaceId, OpenMode.ForWrite);
      using var sourcePolyline = new Polyline3d(Poly3dType.SimplePoly, pointCollection, false);
      var sourceId = modelSpace.AppendEntity(sourcePolyline);
      transaction.AddNewlyCreatedDBObject(sourcePolyline, true);
      var newObjectId = firstSiteId.IsNull
        ? Autodesk.Civil.DatabaseServices.FeatureLine.Create(name, sourceId)
        : Autodesk.Civil.DatabaseServices.FeatureLine.Create(name, sourceId, firstSiteId);
      sourcePolyline.Erase();
      var fl = transaction.GetObject(newObjectId, OpenMode.ForWrite);

      // Set name and layer
      if (!string.IsNullOrWhiteSpace(name))
      {
        CivilObjectUtils.TrySetName(fl, name);
      }
      CivilObjectUtils.TrySetLayer(fl, layer, database, transaction);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(fl),
        ["name"] = CivilObjectUtils.GetName(fl) ?? name,
        ["layer"] = layer,
        ["pointCount"] = points.Count,
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private static IEnumerable<DBObject> EnumerateGradingGroups(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction)
  {
    foreach (ObjectId siteId in civilDoc.GetSiteIds())
    {
      var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
      var gradingGroups = Civil3DCompatibility.GetPropertyValue(siteObj, "GradingGroups");

      foreach (var groupId in CivilObjectUtils.ToObjectIds(gradingGroups))
      {
        yield return transaction.GetObject(groupId, OpenMode.ForRead);
      }
    }
  }

  private static DBObject FindGradingGroupByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name,
    OpenMode mode)
  {
    foreach (ObjectId siteId in civilDoc.GetSiteIds())
    {
      var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
      var gradingGroups = Civil3DCompatibility.GetPropertyValue(siteObj, "GradingGroups");

      foreach (var groupId in CivilObjectUtils.ToObjectIds(gradingGroups))
      {
        var groupObj = transaction.GetObject(groupId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(groupObj), name, StringComparison.OrdinalIgnoreCase))
        {
          return mode == OpenMode.ForWrite
            ? transaction.GetObject(groupId, OpenMode.ForWrite)
            : groupObj;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Grading group '{name}' was not found.");
  }

  private static IEnumerable<DBObject> GetGradingsFromGroup(DBObject group, Transaction transaction)
  {
    var gradingIds = Civil3DCompatibility.GetPropertyValue(group, "GradingIds")
      ?? Civil3DCompatibility.GetPropertyValue(group, "Gradings");

    foreach (var id in CivilObjectUtils.ToObjectIds(gradingIds))
    {
      yield return transaction.GetObject(id, OpenMode.ForRead);
    }
  }

  private static IEnumerable<DBObject> EnumerateFeatureLines(
    Database database,
    Transaction transaction)
  {
    var modelSpaceId = CivilObjectUtils.GetModelSpaceBlockId(database, transaction);
    var modelSpace = transaction.GetObject(modelSpaceId, OpenMode.ForRead) as BlockTableRecord;
    if (modelSpace == null)
    {
      yield break;
    }

    foreach (ObjectId id in modelSpace)
    {
      var obj = transaction.GetObject(id, OpenMode.ForRead) as DBObject;
      if (obj?.GetType().FullName == "Autodesk.Civil.DatabaseServices.FeatureLine")
      {
        yield return obj;
      }
    }
  }

  private static DBObject? FindFeatureLineByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name,
    Database database)
  {
    // Feature lines are AutoCAD entities in model space — iterate BlockTableRecord
    var modelSpaceId = CivilObjectUtils.GetModelSpaceBlockId(database, transaction);
    var btr = transaction.GetObject(modelSpaceId, OpenMode.ForRead) as BlockTableRecord;
    if (btr == null) return null;

    var featureLineTypeName = "Autodesk.Civil.DatabaseServices.FeatureLine";
    foreach (ObjectId id in btr)
    {
      var obj = transaction.GetObject(id, OpenMode.ForRead);
      if (obj.GetType().FullName == featureLineTypeName)
      {
        var objName = CivilObjectUtils.GetName(obj);
        if (string.Equals(objName, name, StringComparison.OrdinalIgnoreCase))
        {
          return obj;
        }
      }
    }
    return null;
  }

  private static ObjectId FindGradingCriteriaId(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string criteriaName)
  {
    var criteriaSets = Civil3DCompatibility.GetPropertyValue(civilDoc, "GradingCriteriaSets");

    foreach (var setId in CivilObjectUtils.ToObjectIds(criteriaSets))
    {
      var setObj = transaction.GetObject(setId, OpenMode.ForRead);
      var criteriaIds = Civil3DCompatibility.GetPropertyValue(setObj, "CriteriaIds")
        ?? Civil3DCompatibility.GetPropertyValue(setObj, "Criteria");

      foreach (var criteriaId in CivilObjectUtils.ToObjectIds(criteriaIds))
      {
        var criteriaObj = transaction.GetObject(criteriaId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(criteriaObj), criteriaName, StringComparison.OrdinalIgnoreCase))
        {
          return criteriaId;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Grading criteria '{criteriaName}' not found.");
  }

  private static Dictionary<string, object?> ToGradingGroupSummary(DBObject group, Transaction transaction)
  {
    var ids = Civil3DCompatibility.GetPropertyValue(group, "GradingIds")
      ?? Civil3DCompatibility.GetPropertyValue(group, "Gradings");
    var count = CivilObjectUtils.ToObjectIds(ids).Count();

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(group),
      ["handle"] = CivilObjectUtils.GetHandle(group),
      ["description"] = CivilObjectUtils.GetStringProperty(group, "Description"),
      ["gradingCount"] = count,
      ["isValid"] = CivilObjectUtils.GetBoolProperty(group, "IsValid"),
    };
  }

  private static Dictionary<string, object?> ToGradingGroupDetail(DBObject group, Transaction transaction)
  {
    var gradings = GetGradingsFromGroup(group, transaction)
      .Select(g => ToGradingSummary(g))
      .ToList();

    var cutVolume = CivilObjectUtils.GetDoubleProperty(group, "CutVolume");
    var fillVolume = CivilObjectUtils.GetDoubleProperty(group, "FillVolume");

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(group),
      ["handle"] = CivilObjectUtils.GetHandle(group),
      ["description"] = CivilObjectUtils.GetStringProperty(group, "Description"),
      ["gradingCount"] = gradings.Count,
      ["cutVolume"] = cutVolume,
      ["fillVolume"] = fillVolume,
      ["netVolume"] = cutVolume.HasValue && fillVolume.HasValue ? cutVolume.Value - fillVolume.Value : null,
      ["isValid"] = CivilObjectUtils.GetBoolProperty(group, "IsValid"),
      ["gradings"] = gradings,
    };
  }

  private static Dictionary<string, object?> ToGradingSummary(DBObject grading)
  {
    return new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(grading),
      ["name"] = CivilObjectUtils.GetName(grading),
      ["criteriaName"] = CivilObjectUtils.GetStringProperty(grading, "CriteriaName"),
      ["isValid"] = CivilObjectUtils.GetBoolProperty(grading, "IsValid"),
    };
  }

  private static Dictionary<string, object?> ToFeatureLineSummary(DBObject featureLine, Transaction transaction)
  {
    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(featureLine),
      ["handle"] = CivilObjectUtils.GetHandle(featureLine),
      ["layer"] = CivilObjectUtils.GetStringProperty(featureLine, "Layer"),
      ["style"] = GetFeatureLineStyleName(featureLine, transaction),
    };
  }

  private static Dictionary<string, object?> ToFeatureLineDetail(
    DBObject featureLine,
    Database database,
    Transaction transaction)
  {
    var vertices = GetFeatureLineVertices(featureLine);
    var minElevation = vertices.Count > 0 ? vertices.Min(point => point.Z) : 0d;
    var maxElevation = vertices.Count > 0 ? vertices.Max(point => point.Z) : 0d;

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(featureLine) ?? string.Empty,
      ["handle"] = CivilObjectUtils.GetHandle(featureLine),
      ["layer"] = CivilObjectUtils.GetStringProperty(featureLine, "Layer") ?? string.Empty,
      ["style"] = GetFeatureLineStyleName(featureLine, transaction) ?? string.Empty,
      ["length"] = GetFeatureLineLength(featureLine),
      ["vertexCount"] = vertices.Count,
      ["vertices"] = vertices.Select(ToPointData).ToList(),
      ["minElevation"] = minElevation,
      ["maxElevation"] = maxElevation,
      ["units"] = CivilObjectUtils.LinearUnits(database),
    };
  }

  private static Dictionary<string, object?> ToGradingDetail(DBObject grading)
  {
    return new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(grading),
      ["name"] = CivilObjectUtils.GetName(grading),
      ["criteriaName"] = CivilObjectUtils.GetStringProperty(grading, "CriteriaName"),
      ["side"] = CivilObjectUtils.GetStringProperty(grading, "Side"),
      ["isValid"] = CivilObjectUtils.GetBoolProperty(grading, "IsValid"),
      ["cutVolume"] = CivilObjectUtils.GetDoubleProperty(grading, "CutVolume"),
      ["fillVolume"] = CivilObjectUtils.GetDoubleProperty(grading, "FillVolume"),
    };
  }

  private static string? GetFeatureLineStyleName(DBObject featureLine, Transaction transaction)
  {
    try
    {
      var styleName = CivilObjectUtils.GetStringProperty(featureLine, "StyleName");
      if (!string.IsNullOrWhiteSpace(styleName))
      {
        return styleName;
      }

      var styleIdObject = CivilObjectUtils.GetPropertyValue<object>(featureLine, "StyleId");
      if (styleIdObject is not ObjectId styleId || styleId == ObjectId.Null)
      {
        return null;
      }

      var style = transaction.GetObject(styleId, OpenMode.ForRead);
      return CivilObjectUtils.GetName(style);
    }
    catch
    {
      return null;
    }
  }

  private static double GetFeatureLineLength(DBObject featureLine)
  {
    foreach (var propertyName in new[] { "Length3D", "Length2D", "Length" })
    {
      var value = CivilObjectUtils.GetDoubleProperty(featureLine, propertyName);
      if (value.HasValue)
      {
        return value.Value;
      }
    }

    var vertices = GetFeatureLineVertices(featureLine);
    if (vertices.Count < 2)
    {
      return 0d;
    }

    var totalLength = 0d;
    for (var index = 1; index < vertices.Count; index++)
    {
      totalLength += vertices[index - 1].DistanceTo(vertices[index]);
    }

    return totalLength;
  }

  private static List<Point3d> GetFeatureLineVertices(DBObject featureLine)
  {
    if (featureLine is not Autodesk.Civil.DatabaseServices.FeatureLine typedFeatureLine)
      return new List<Point3d>();
    return DeduplicateSequentialPoints(
      typedFeatureLine.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints).Cast<Point3d>());
  }

  private static List<Point3d> DeduplicateSequentialPoints(IEnumerable<Point3d> points)
  {
    var result = new List<Point3d>();
    foreach (var point in points)
    {
      if (result.Count == 0 || !point.IsEqualTo(result[^1], new Tolerance(1e-8, 1e-8)))
      {
        result.Add(point);
      }
    }

    return result;
  }

  private static Dictionary<string, object?> ToPointData(Point3d point)
  {
    return new Dictionary<string, object?>
    {
      ["x"] = point.X,
      ["y"] = point.Y,
      ["z"] = point.Z,
    };
  }

}
