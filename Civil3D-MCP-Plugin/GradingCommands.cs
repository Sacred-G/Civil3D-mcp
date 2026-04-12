using System.Reflection;
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

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
      var sites = sitesProperty?.GetValue(civilDoc);
      var siteIds = CivilObjectUtils.ToObjectIds(sites);
      var firstSiteId = siteIds.FirstOrDefault();

      if (firstSiteId == ObjectId.Null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "No site found in the drawing. Create a site first before adding a grading group.");
      }

      var siteObj = transaction.GetObject(firstSiteId, OpenMode.ForRead);
      var gradingGroupsProperty = siteObj.GetType().GetProperty("GradingGroups", BindingFlags.Public | BindingFlags.Instance);
      var gradingGroups = gradingGroupsProperty?.GetValue(siteObj);

      // Try Add(name, useProjection) or Add(name)
      object? newGroupId = null;
      var addMethods = gradingGroups?.GetType().GetMethods().Where(m => m.Name == "Add").ToArray();
      if (addMethods != null)
      {
        var addWithProjection = addMethods.FirstOrDefault(m => m.GetParameters().Length == 2);
        var addSimple = addMethods.FirstOrDefault(m => m.GetParameters().Length == 1);

        newGroupId = addWithProjection != null
          ? addWithProjection.Invoke(gradingGroups, new object[] { name, useProjection })
          : addSimple?.Invoke(gradingGroups, new object[] { name });
      }

      if (newGroupId == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Failed to create grading group — Add method not found.");
      }

      var newGroupObjectId = (ObjectId)newGroupId;
      var newGroup = transaction.GetObject(newGroupObjectId, OpenMode.ForWrite);

      var descriptionProp = newGroup.GetType().GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
      descriptionProp?.SetValue(newGroup, description);

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["handle"] = CivilObjectUtils.GetHandle(newGroup),
        ["description"] = description,
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

      var cutVolume = CivilObjectUtils.GetDoubleProperty(group, "CutVolume") ?? 0;
      var fillVolume = CivilObjectUtils.GetDoubleProperty(group, "FillVolume") ?? 0;
      var netVolume = cutVolume - fillVolume;

      return new Dictionary<string, object?>
      {
        ["groupName"] = name,
        ["cutVolume"] = cutVolume,
        ["fillVolume"] = fillVolume,
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
      var createMethod = group.GetType().GetMethods()
        .Where(m => m.Name == "CreateSurface")
        .OrderByDescending(m => m.GetParameters().Length)
        .FirstOrDefault();

      if (createMethod == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "CreateSurface method not found on grading group.");
      }

      object? resultId;
      if (createMethod.GetParameters().Length >= 1 && surfaceName != null)
      {
        resultId = createMethod.Invoke(group, new object[] { surfaceName });
      }
      else
      {
        resultId = createMethod.Invoke(group, Array.Empty<object>());
      }

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
      var addMethod = group.GetType().GetMethods()
        .FirstOrDefault(m => m.Name == "AddGrading" || m.Name == "CreateGrading");

      if (addMethod == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "AddGrading/CreateGrading method not found on grading group.");
      }

      var featureLineId = featureLine.ObjectId;
      object? gradingId;

      if (addMethod.GetParameters().Length >= 3 && criteriaName != null)
      {
        var criteriaId = FindGradingCriteriaId(civilDoc, transaction, criteriaName);
        gradingId = addMethod.Invoke(group, new object[] { featureLineId, criteriaId, side });
      }
      else if (addMethod.GetParameters().Length >= 2)
      {
        gradingId = addMethod.Invoke(group, new object[] { featureLineId, side });
      }
      else
      {
        gradingId = addMethod.Invoke(group, new object[] { featureLineId });
      }

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
      var criteriaSetsProp = civilDoc.GetType().GetProperty("GradingCriteriaSets", BindingFlags.Public | BindingFlags.Instance);
      var criteriaSets = criteriaSetsProp?.GetValue(civilDoc);

      foreach (var setId in CivilObjectUtils.ToObjectIds(criteriaSets))
      {
        var setObj = transaction.GetObject(setId, OpenMode.ForRead);
        var setName = CivilObjectUtils.GetName(setObj) ?? string.Empty;

        var criteriaIdsProperty = setObj.GetType().GetProperty("CriteriaIds", BindingFlags.Public | BindingFlags.Instance)
          ?? setObj.GetType().GetProperty("Criteria", BindingFlags.Public | BindingFlags.Instance);
        var criteriaIds = criteriaIdsProperty?.GetValue(setObj);

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
      var z = pt["z"]?.GetValue<double>() ?? 0.0;
      return new Point3d(x, y, z);
    }).ToList();

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Get or find a site to add the feature line to
      var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
      var sites = sitesProperty?.GetValue(civilDoc);
      var siteIds = CivilObjectUtils.ToObjectIds(sites);
      var firstSiteId = siteIds.FirstOrDefault();

      // Build a Point3dCollection for the feature line
      var pointCollection = new Point3dCollection();
      foreach (var pt in points)
      {
        pointCollection.Add(pt);
      }

      // Use the static FeatureLine.Create method (if available) or direct constructor
      var featureLineType = FindType("Autodesk.Civil.DatabaseServices.FeatureLine");
      object? newId = null;

      if (featureLineType != null)
      {
        var createMethod = featureLineType.GetMethods(BindingFlags.Static | BindingFlags.Public)
          .FirstOrDefault(m => m.Name == "Create" && m.GetParameters().Length >= 2);

        if (createMethod != null)
        {
          var modelSpaceId = database.CurrentSpaceId;
          var siteArg = firstSiteId == ObjectId.Null ? (object)ObjectId.Null : firstSiteId;
          newId = createMethod.Invoke(null, new object[] { modelSpaceId, siteArg, pointCollection });
        }
      }

      if (newId == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "FeatureLine.Create method not found. Ensure Civil3D API is available.");
      }

      var newObjectId = (ObjectId)newId;
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
    var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
    var sites = sitesProperty?.GetValue(civilDoc);

    foreach (var siteId in CivilObjectUtils.ToObjectIds(sites))
    {
      var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
      var gradingGroupsProp = siteObj.GetType().GetProperty("GradingGroups", BindingFlags.Public | BindingFlags.Instance);
      var gradingGroups = gradingGroupsProp?.GetValue(siteObj);

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
    var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
    var sites = sitesProperty?.GetValue(civilDoc);

    foreach (var siteId in CivilObjectUtils.ToObjectIds(sites))
    {
      var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
      var gradingGroupsProp = siteObj.GetType().GetProperty("GradingGroups", BindingFlags.Public | BindingFlags.Instance);
      var gradingGroups = gradingGroupsProp?.GetValue(siteObj);

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
    var gradingIdsProp = group.GetType().GetProperty("GradingIds", BindingFlags.Public | BindingFlags.Instance)
      ?? group.GetType().GetProperty("Gradings", BindingFlags.Public | BindingFlags.Instance);
    var gradingIds = gradingIdsProp?.GetValue(group);

    foreach (var id in CivilObjectUtils.ToObjectIds(gradingIds))
    {
      yield return transaction.GetObject(id, OpenMode.ForRead);
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
    var criteriaSetsProp = civilDoc.GetType().GetProperty("GradingCriteriaSets", BindingFlags.Public | BindingFlags.Instance);
    var criteriaSets = criteriaSetsProp?.GetValue(civilDoc);

    foreach (var setId in CivilObjectUtils.ToObjectIds(criteriaSets))
    {
      var setObj = transaction.GetObject(setId, OpenMode.ForRead);
      var criteriaIdsProperty = setObj.GetType().GetProperty("CriteriaIds", BindingFlags.Public | BindingFlags.Instance)
        ?? setObj.GetType().GetProperty("Criteria", BindingFlags.Public | BindingFlags.Instance);
      var criteriaIds = criteriaIdsProperty?.GetValue(setObj);

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
    var gradingIds = group.GetType().GetProperty("GradingIds", BindingFlags.Public | BindingFlags.Instance)
      ?? group.GetType().GetProperty("Gradings", BindingFlags.Public | BindingFlags.Instance);
    var ids = gradingIds?.GetValue(group);
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

    var cutVolume = CivilObjectUtils.GetDoubleProperty(group, "CutVolume") ?? 0;
    var fillVolume = CivilObjectUtils.GetDoubleProperty(group, "FillVolume") ?? 0;

    return new Dictionary<string, object?>
    {
      ["name"] = CivilObjectUtils.GetName(group),
      ["handle"] = CivilObjectUtils.GetHandle(group),
      ["description"] = CivilObjectUtils.GetStringProperty(group, "Description"),
      ["gradingCount"] = gradings.Count,
      ["cutVolume"] = cutVolume,
      ["fillVolume"] = fillVolume,
      ["netVolume"] = cutVolume - fillVolume,
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

  private static Type? FindType(string fullName)
  {
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      var t = assembly.GetType(fullName);
      if (t != null) return t;
    }
    return null;
  }
}
