using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class AcadCommands
{
  public static Task<object?> CreatePolylineAsync(JsonObject? parameters)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createPolyline requires at least two points.");
    }

    var closed = PluginRuntime.GetOptionalInt(parameters, "closed") == 1;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var polyline = new Polyline();
      for (var index = 0; index < pointsNode.Count; index++)
      {
        if (pointsNode[index] is not JsonObject point)
        {
          continue;
        }

        var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
        var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
        polyline.AddVertexAt(index, new Point2d(x, y), 0, 0, 0);
      }

      polyline.Closed = closed;

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        polyline.LayerId = layerId;
      }

      var polylineId = modelSpace.AppendEntity(polyline);
      transaction.AddNewlyCreatedDBObject(polyline, true);

      var created = CivilObjectUtils.GetRequiredObject<Polyline>(transaction, polylineId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["vertexCount"] = created.NumberOfVertices,
        ["closed"] = created.Closed,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> CreateTextAsync(JsonObject? parameters)
  {
    var text = PluginRuntime.GetRequiredString(parameters, "text");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var space = ParseTargetSpace(parameters);
    var height = PluginRuntime.GetOptionalDouble(parameters, "height") ?? DefaultTextHeight(space);
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layout");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var (targetSpace, targetLayoutName) = ResolveTargetSpace(database, transaction, space, layoutName);

      using var dbText = new DBText
      {
        TextString = text,
        Position = new Point3d(x, y, z),
        Height = height,
        Rotation = rotation,
      };

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        dbText.LayerId = layerId;
      }

      var textId = targetSpace.AppendEntity(dbText);
      transaction.AddNewlyCreatedDBObject(dbText, true);

      var created = CivilObjectUtils.GetRequiredObject<DBText>(transaction, textId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["text"] = created.TextString,
        ["x"] = created.Position.X,
        ["y"] = created.Position.Y,
        ["z"] = created.Position.Z,
        ["height"] = created.Height,
        ["rotation"] = created.Rotation,
        ["layer"] = created.Layer,
        ["space"] = space,
        ["layout"] = targetLayoutName,
      };
    });
  }

  private static string ParseTargetSpace(JsonObject? parameters)
  {
    var space = PluginRuntime.GetOptionalString(parameters, "space")?.Trim().ToLowerInvariant() ?? "model";
    if (space != "model" && space != "paper")
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"space must be 'model' or 'paper', got '{space}'.");
    }

    return space;
  }

  // Model-space text keeps the 2.5 drawing-unit default; on a sheet that would be 2.5 inches tall,
  // so paper space defaults to 0.1 (a typical plotted note height).
  private static double DefaultTextHeight(string space) => space == "paper" ? 0.1d : 2.5d;

  // Resolves the block table record new entities are appended to. For paper space, uses the named
  // layout, or the current layout when it is a paper layout; otherwise fails listing available layouts.
  private static (BlockTableRecord Space, string LayoutName) ResolveTargetSpace(Database database, Transaction transaction, string space, string? layoutName)
  {
    if (space == "model")
    {
      if (!string.IsNullOrWhiteSpace(layoutName) && !layoutName.Equals("Model", StringComparison.OrdinalIgnoreCase))
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"layout '{layoutName}' was given with space 'model'. Use space 'paper' to write into a layout.");
      }

      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      return (CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite), "Model");
    }

    var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);
    var paperLayouts = new List<Layout>();
    foreach (DBDictionaryEntry layoutEntry in layoutDict)
    {
      var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
      if (!layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase))
      {
        paperLayouts.Add(layout);
      }
    }

    var available = string.Join(", ", paperLayouts.Select(layout => layout.LayoutName));
    var targetName = string.IsNullOrWhiteSpace(layoutName) ? LayoutManager.Current.CurrentLayout : layoutName;
    if (targetName.Equals("Model", StringComparison.OrdinalIgnoreCase))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"space 'paper' needs a layout because the active tab is Model. Available layouts: {available}.");
    }

    var target = paperLayouts.FirstOrDefault(layout => layout.LayoutName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
      ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layout '{targetName}' was not found. Available layouts: {available}.");

    return (CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, target.BlockTableRecordId, OpenMode.ForWrite), target.LayoutName);
  }

  public static Task<object?> Create3dPolylineAsync(JsonObject? parameters)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "create3dPolyline requires at least two points.");
    }

    var closed = PluginRuntime.GetOptionalInt(parameters, "closed") == 1;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      var pointCollection = new Point3dCollection();
      for (var index = 0; index < pointsNode.Count; index++)
      {
        if (pointsNode[index] is not JsonObject point)
        {
          continue;
        }

        var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
        var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
        var z = point["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing z.");
        pointCollection.Add(new Point3d(x, y, z));
      }

      if (pointCollection.Count < 2)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "create3dPolyline requires at least two valid points.");
      }

      using var polyline3d = new Polyline3d(Poly3dType.SimplePoly, pointCollection, closed);

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        polyline3d.LayerId = layerId;
      }

      var polylineId = modelSpace.AppendEntity(polyline3d);
      transaction.AddNewlyCreatedDBObject(polyline3d, true);

      var created = CivilObjectUtils.GetRequiredObject<Polyline3d>(transaction, polylineId, OpenMode.ForRead);
      var vertexCount = 0;
      foreach (ObjectId vertexId in created)
      {
        _ = vertexId;
        vertexCount++;
      }

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["vertexCount"] = vertexCount,
        ["closed"] = created.Closed,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> CreateLineSegmentAsync(JsonObject? parameters)
  {
    var startX = PluginRuntime.GetRequiredDouble(parameters, "startX");
    var startY = PluginRuntime.GetRequiredDouble(parameters, "startY");
    var startZ = PluginRuntime.GetOptionalDouble(parameters, "startZ") ?? 0d;
    var endX = PluginRuntime.GetRequiredDouble(parameters, "endX");
    var endY = PluginRuntime.GetRequiredDouble(parameters, "endY");
    var endZ = PluginRuntime.GetOptionalDouble(parameters, "endZ") ?? 0d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var line = new Line(new Point3d(startX, startY, startZ), new Point3d(endX, endY, endZ));

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        line.LayerId = layerId;
      }

      var lineId = modelSpace.AppendEntity(line);
      transaction.AddNewlyCreatedDBObject(line, true);

      var created = CivilObjectUtils.GetRequiredObject<Line>(transaction, lineId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["lineId"] = CivilObjectUtils.GetHandle(created),
        ["startX"] = created.StartPoint.X,
        ["startY"] = created.StartPoint.Y,
        ["startZ"] = created.StartPoint.Z,
        ["endX"] = created.EndPoint.X,
        ["endY"] = created.EndPoint.Y,
        ["endZ"] = created.EndPoint.Z,
        ["length"] = created.Length,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> CreateMTextAsync(JsonObject? parameters)
  {
    var text = PluginRuntime.GetRequiredString(parameters, "text");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var width = PluginRuntime.GetOptionalDouble(parameters, "width") ?? 0d;
    var space = ParseTargetSpace(parameters);
    var textHeight = PluginRuntime.GetOptionalDouble(parameters, "textHeight") ?? DefaultTextHeight(space);
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layout");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var (targetSpace, targetLayoutName) = ResolveTargetSpace(database, transaction, space, layoutName);

      using var mtext = new MText
      {
        Contents = text,
        Location = new Point3d(x, y, z),
        TextHeight = textHeight,
        Rotation = rotation,
      };

      if (width > 0)
      {
        mtext.Width = width;
      }

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        mtext.LayerId = layerId;
      }

      var mtextId = targetSpace.AppendEntity(mtext);
      transaction.AddNewlyCreatedDBObject(mtext, true);

      var created = CivilObjectUtils.GetRequiredObject<MText>(transaction, mtextId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["text"] = created.Contents,
        ["x"] = created.Location.X,
        ["y"] = created.Location.Y,
        ["z"] = created.Location.Z,
        ["textHeight"] = created.TextHeight,
        ["rotation"] = created.Rotation,
        ["width"] = created.Width,
        ["layer"] = created.Layer,
        ["space"] = space,
        ["layout"] = targetLayoutName,
      };
    });
  }

  public static Task<object?> CreateMLeaderAsync(JsonObject? parameters)
  {
    var text = PluginRuntime.GetRequiredString(parameters, "text");
    var leaderX = PluginRuntime.GetRequiredDouble(parameters, "leaderPointX");
    var leaderY = PluginRuntime.GetRequiredDouble(parameters, "leaderPointY");
    var leaderZ = PluginRuntime.GetOptionalDouble(parameters, "leaderPointZ") ?? 0d;
    var textX = PluginRuntime.GetRequiredDouble(parameters, "textPointX");
    var textY = PluginRuntime.GetRequiredDouble(parameters, "textPointY");
    var textZ = PluginRuntime.GetOptionalDouble(parameters, "textPointZ") ?? 0d;
    var textHeight = PluginRuntime.GetOptionalDouble(parameters, "textHeight") ?? 0.125d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var styleName = PluginRuntime.GetOptionalString(parameters, "mLeaderStyle");
    var space = ParseTargetSpace(parameters);
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layout");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var (targetSpace, targetLayoutName) = ResolveTargetSpace(database, transaction, space, layoutName);

      var leaderPoint = new Point3d(leaderX, leaderY, leaderZ);
      var textPoint = new Point3d(textX, textY, textZ);

      using var mleader = new MLeader();
      mleader.SetDatabaseDefaults(database);

      var styleDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.MLeaderStyleDictionaryId, OpenMode.ForRead);
      if (!string.IsNullOrWhiteSpace(styleName) && styleDict.Contains(styleName))
      {
        mleader.MLeaderStyle = styleDict.GetAt(styleName);
      }
      else if (styleDict.Contains("Standard"))
      {
        mleader.MLeaderStyle = styleDict.GetAt("Standard");
      }

      using var mtext = new MText
      {
        Contents = text,
        Location = textPoint,
        TextHeight = textHeight,
      };
      mleader.MText = mtext;

      var leaderIndex = mleader.AddLeaderLine(leaderPoint);
      mleader.AddLastVertex(leaderIndex, textPoint);

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        mleader.LayerId = layerId;
      }

      var mleaderId = targetSpace.AppendEntity(mleader);
      transaction.AddNewlyCreatedDBObject(mleader, true);

      var created = CivilObjectUtils.GetRequiredObject<MLeader>(transaction, mleaderId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["text"] = text,
        ["leaderX"] = leaderX,
        ["leaderY"] = leaderY,
        ["textX"] = textX,
        ["textY"] = textY,
        ["layer"] = created.Layer,
        ["space"] = space,
        ["layout"] = targetLayoutName,
      };
    });
  }

  public static Task<object?> ListTextEntitiesAsync(JsonObject? parameters)
  {
    var containsFilter = PluginRuntime.GetOptionalString(parameters, "contains");
    var layerFilter = PluginRuntime.GetOptionalString(parameters, "layer");
    var requestedLimit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 200;
    var limit = Math.Clamp(requestedLimit, 1, 500);
    var spaceFilter = (PluginRuntime.GetOptionalString(parameters, "space") ?? "all").ToLowerInvariant();

    var typesNode = PluginRuntime.GetParameter(parameters, "entityTypes") as JsonArray;
    var allowedTypes = typesNode != null && typesNode.Count > 0
      ? typesNode.Select(node => node?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToHashSet(StringComparer.OrdinalIgnoreCase)
      : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DBText", "MText", "MLeader" };

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);

      foreach (DBDictionaryEntry layoutEntry in layoutDict)
      {
        if (results.Count >= limit)
        {
          break;
        }

        var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
        var isModelSpace = layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase);
        if (spaceFilter == "model" && !isModelSpace)
        {
          continue;
        }

        if (spaceFilter == "paper" && isModelSpace)
        {
          continue;
        }

        var spaceBlock = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
        foreach (ObjectId objectId in spaceBlock)
        {
          if (results.Count >= limit)
          {
            break;
          }

          var dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
          var entry = dbObject switch
          {
            DBText dbText when allowedTypes.Contains("DBText") => BuildTextEntry(dbText, layout.LayoutName, isModelSpace),
            MText mText when allowedTypes.Contains("MText") => BuildTextEntry(mText, layout.LayoutName, isModelSpace),
            MLeader mLeader when allowedTypes.Contains("MLeader") => BuildTextEntry(mLeader, layout.LayoutName, isModelSpace),
            _ => null,
          };

          if (entry == null || !MatchesFilters((string?)entry["text"], (string?)entry["layer"], containsFilter, layerFilter))
          {
            continue;
          }

          results.Add(entry);
        }
      }

      return new Dictionary<string, object?>
      {
        ["entities"] = results,
      };
    });
  }

  public static Task<object?> ListPolylineEntitiesAsync(JsonObject? parameters)
  {
    var layerFilter = PluginRuntime.GetOptionalString(parameters, "layer");
    var requestedLimit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 200;
    var limit = Math.Clamp(requestedLimit, 1, 500);
    var spaceFilter = (PluginRuntime.GetOptionalString(parameters, "space") ?? "all").ToLowerInvariant();
    var colorIndexFilter = PluginRuntime.GetOptionalInt(parameters, "colorIndex");

    var typesNode = PluginRuntime.GetParameter(parameters, "entityTypes") as JsonArray;
    var allowedTypes = typesNode != null && typesNode.Count > 0
      ? typesNode.Select(node => node?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToHashSet(StringComparer.OrdinalIgnoreCase)
      : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Polyline", "Polyline2d", "Polyline3d", "Spline" };

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);

      foreach (DBDictionaryEntry layoutEntry in layoutDict)
      {
        if (results.Count >= limit)
        {
          break;
        }

        var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
        var isModelSpace = layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase);
        if (spaceFilter == "model" && !isModelSpace)
        {
          continue;
        }

        if (spaceFilter == "paper" && isModelSpace)
        {
          continue;
        }

        var spaceBlock = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
        foreach (ObjectId objectId in spaceBlock)
        {
          if (results.Count >= limit)
          {
            break;
          }

          var dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
          var entityTypeName = dbObject switch
          {
            Polyline => "Polyline",
            Polyline2d => "Polyline2d",
            Polyline3d => "Polyline3d",
            Spline => "Spline",
            _ => null,
          };

          if (entityTypeName == null || !allowedTypes.Contains(entityTypeName) || dbObject is not Entity entity)
          {
            continue;
          }

          if (!string.IsNullOrWhiteSpace(layerFilter) && !string.Equals(entity.Layer, layerFilter, StringComparison.OrdinalIgnoreCase))
          {
            continue;
          }

          if (colorIndexFilter.HasValue && entity.ColorIndex != colorIndexFilter.Value)
          {
            continue;
          }

          results.Add(BuildPolylineEntry(entity, entityTypeName, layout.LayoutName, isModelSpace));
        }
      }

      return new Dictionary<string, object?>
      {
        ["entities"] = results,
      };
    });
  }

  private static Dictionary<string, object?> BuildPolylineEntry(Entity entity, string entityTypeName, string layoutName, bool isModelSpace)
  {
    Extents3d? bounds = null;
    try { bounds = entity.GeometricExtents; } catch { bounds = null; }

    return new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(entity),
      ["entityType"] = entityTypeName,
      ["layer"] = entity.Layer,
      ["colorIndex"] = entity.ColorIndex,
      ["trueColorName"] = GetTrueColorName(entity.Color),
      ["closed"] = CivilObjectUtils.GetBoolProperty(entity, "Closed"),
      ["minX"] = bounds?.MinPoint.X,
      ["minY"] = bounds?.MinPoint.Y,
      ["maxX"] = bounds?.MaxPoint.X,
      ["maxY"] = bounds?.MaxPoint.Y,
      ["centerX"] = bounds.HasValue ? (bounds.Value.MinPoint.X + bounds.Value.MaxPoint.X) / 2 : (double?)null,
      ["centerY"] = bounds.HasValue ? (bounds.Value.MinPoint.Y + bounds.Value.MaxPoint.Y) / 2 : (double?)null,
      ["space"] = isModelSpace ? "model" : "paper",
      ["layout"] = layoutName,
    };
  }

  public static Task<object?> ListShapeEntitiesAsync(JsonObject? parameters)
  {
    var layerFilter = PluginRuntime.GetOptionalString(parameters, "layer");
    var requestedLimit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 200;
    var limit = Math.Clamp(requestedLimit, 1, 500);
    var spaceFilter = (PluginRuntime.GetOptionalString(parameters, "space") ?? "all").ToLowerInvariant();
    var colorIndexFilter = PluginRuntime.GetOptionalInt(parameters, "colorIndex");
    var nearX = PluginRuntime.GetOptionalDouble(parameters, "nearX");
    var nearY = PluginRuntime.GetOptionalDouble(parameters, "nearY");
    var nearRadius = PluginRuntime.GetOptionalDouble(parameters, "nearRadius");

    var typesNode = PluginRuntime.GetParameter(parameters, "entityTypes") as JsonArray;
    var allowedTypes = typesNode != null && typesNode.Count > 0
      ? typesNode.Select(node => node?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToHashSet(StringComparer.OrdinalIgnoreCase)
      : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Line", "Circle", "Arc", "Ellipse", "Solid", "Hatch", "Point" };

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);

      foreach (DBDictionaryEntry layoutEntry in layoutDict)
      {
        if (results.Count >= limit)
        {
          break;
        }

        var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
        var isModelSpace = layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase);
        if (spaceFilter == "model" && !isModelSpace)
        {
          continue;
        }

        if (spaceFilter == "paper" && isModelSpace)
        {
          continue;
        }

        var spaceBlock = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
        foreach (ObjectId objectId in spaceBlock)
        {
          if (results.Count >= limit)
          {
            break;
          }

          var dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
          var entityTypeName = dbObject switch
          {
            Line => "Line",
            Circle => "Circle",
            Arc => "Arc",
            Ellipse => "Ellipse",
            Solid => "Solid",
            Hatch => "Hatch",
            DBPoint => "Point",
            _ => null,
          };

          if (entityTypeName == null || !allowedTypes.Contains(entityTypeName) || dbObject is not Entity entity)
          {
            continue;
          }

          if (!string.IsNullOrWhiteSpace(layerFilter) && !string.Equals(entity.Layer, layerFilter, StringComparison.OrdinalIgnoreCase))
          {
            continue;
          }

          if (colorIndexFilter.HasValue && entity.ColorIndex != colorIndexFilter.Value)
          {
            continue;
          }

          var entry = BuildShapeEntry(entity, entityTypeName, layout.LayoutName, isModelSpace);

          if (nearX.HasValue && nearY.HasValue && nearRadius.HasValue)
          {
            if (entry["centerX"] is not double cx || entry["centerY"] is not double cy)
            {
              continue;
            }

            var dx = cx - nearX.Value;
            var dy = cy - nearY.Value;
            if (Math.Sqrt(dx * dx + dy * dy) > nearRadius.Value)
            {
              continue;
            }
          }

          results.Add(entry);
        }
      }

      return new Dictionary<string, object?>
      {
        ["entities"] = results,
      };
    });
  }

  private static Dictionary<string, object?> BuildShapeEntry(Entity entity, string entityTypeName, string layoutName, bool isModelSpace)
  {
    Extents3d? bounds = null;
    try { bounds = entity.GeometricExtents; } catch { bounds = null; }

    var entry = new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(entity),
      ["entityType"] = entityTypeName,
      ["layer"] = entity.Layer,
      ["colorIndex"] = entity.ColorIndex,
      ["trueColorName"] = GetTrueColorName(entity.Color),
      ["minX"] = bounds?.MinPoint.X,
      ["minY"] = bounds?.MinPoint.Y,
      ["maxX"] = bounds?.MaxPoint.X,
      ["maxY"] = bounds?.MaxPoint.Y,
      ["centerX"] = bounds.HasValue ? (bounds.Value.MinPoint.X + bounds.Value.MaxPoint.X) / 2 : (double?)null,
      ["centerY"] = bounds.HasValue ? (bounds.Value.MinPoint.Y + bounds.Value.MaxPoint.Y) / 2 : (double?)null,
      ["space"] = isModelSpace ? "model" : "paper",
      ["layout"] = layoutName,
    };

    switch (entity)
    {
      case Line line:
        entry["startX"] = line.StartPoint.X;
        entry["startY"] = line.StartPoint.Y;
        entry["startZ"] = line.StartPoint.Z;
        entry["endX"] = line.EndPoint.X;
        entry["endY"] = line.EndPoint.Y;
        entry["endZ"] = line.EndPoint.Z;
        entry["length"] = line.Length;
        break;

      case Circle circle:
        entry["centerX"] = circle.Center.X;
        entry["centerY"] = circle.Center.Y;
        entry["centerZ"] = circle.Center.Z;
        entry["radius"] = circle.Radius;
        break;

      case Arc arc:
        entry["centerX"] = arc.Center.X;
        entry["centerY"] = arc.Center.Y;
        entry["centerZ"] = arc.Center.Z;
        entry["radius"] = arc.Radius;
        entry["startAngle"] = arc.StartAngle;
        entry["endAngle"] = arc.EndAngle;
        break;

      case Ellipse ellipse:
        entry["centerX"] = ellipse.Center.X;
        entry["centerY"] = ellipse.Center.Y;
        entry["centerZ"] = ellipse.Center.Z;
        entry["majorRadius"] = ellipse.MajorRadius;
        entry["minorRadius"] = ellipse.MinorRadius;
        break;

      case DBPoint point:
        entry["x"] = point.Position.X;
        entry["y"] = point.Position.Y;
        entry["z"] = point.Position.Z;
        entry["centerX"] = point.Position.X;
        entry["centerY"] = point.Position.Y;
        break;

      case Hatch hatch:
        entry["patternName"] = hatch.PatternName;
        entry["numberOfLoops"] = hatch.NumberOfLoops;
        break;

      case Solid solid:
        var corners = new List<Dictionary<string, object?>>();
        for (short index = 0; index < 4; index++)
        {
          var point = solid.GetPointAt(index);
          corners.Add(new Dictionary<string, object?> { ["x"] = point.X, ["y"] = point.Y, ["z"] = point.Z });
        }

        entry["corners"] = corners;
        break;
    }

    return entry;
  }

  private static Dictionary<string, object?> BuildTextEntry(DBText dbText, string layoutName, bool isModelSpace) => new()
  {
    ["handle"] = CivilObjectUtils.GetHandle(dbText),
    ["entityType"] = "DBText",
    ["text"] = dbText.TextString,
    ["x"] = dbText.Position.X,
    ["y"] = dbText.Position.Y,
    ["z"] = dbText.Position.Z,
    ["height"] = dbText.Height,
    ["rotation"] = dbText.Rotation,
    ["layer"] = dbText.Layer,
    ["colorIndex"] = dbText.ColorIndex,
    ["trueColorName"] = GetTrueColorName(dbText.Color),
    ["space"] = isModelSpace ? "model" : "paper",
    ["layout"] = layoutName,
  };

  private static Dictionary<string, object?> BuildTextEntry(MText mText, string layoutName, bool isModelSpace) => new()
  {
    ["handle"] = CivilObjectUtils.GetHandle(mText),
    ["entityType"] = "MText",
    ["text"] = mText.Contents,
    ["x"] = mText.Location.X,
    ["y"] = mText.Location.Y,
    ["z"] = mText.Location.Z,
    ["textHeight"] = mText.TextHeight,
    ["rotation"] = mText.Rotation,
    ["width"] = mText.Width,
    ["layer"] = mText.Layer,
    ["colorIndex"] = mText.ColorIndex,
    ["trueColorName"] = GetTrueColorName(mText.Color),
    ["space"] = isModelSpace ? "model" : "paper",
    ["layout"] = layoutName,
  };

  private static Dictionary<string, object?> BuildTextEntry(MLeader mLeader, string layoutName, bool isModelSpace)
  {
    var location = mLeader.MText?.Location;
    return new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(mLeader),
      ["entityType"] = "MLeader",
      ["text"] = mLeader.MText?.Contents,
      ["x"] = location?.X,
      ["y"] = location?.Y,
      ["z"] = location?.Z,
      ["layer"] = mLeader.Layer,
      ["colorIndex"] = mLeader.ColorIndex,
      ["trueColorName"] = GetTrueColorName(mLeader.Color),
      ["space"] = isModelSpace ? "model" : "paper",
      ["layout"] = layoutName,
    };
  }

  private static string? GetTrueColorName(Color color)
  {
    return color.ColorMethod == ColorMethod.ByColor ? color.ColorNameForDisplay : null;
  }

  private static bool MatchesFilters(string? text, string? layer, string? containsFilter, string? layerFilter)
  {
    if (!string.IsNullOrWhiteSpace(containsFilter) &&
        (text == null || text.IndexOf(containsFilter, StringComparison.OrdinalIgnoreCase) < 0))
    {
      return false;
    }

    if (!string.IsNullOrWhiteSpace(layerFilter) &&
        !string.Equals(layer, layerFilter, StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    return true;
  }

  public static Task<object?> UpdateTextContentAsync(JsonObject? parameters)
  {
    var handleValue = PluginRuntime.GetRequiredString(parameters, "handle");
    var newText = PluginRuntime.GetOptionalString(parameters, "text");
    var newX = PluginRuntime.GetOptionalDouble(parameters, "x");
    var newY = PluginRuntime.GetOptionalDouble(parameters, "y");
    var newZ = PluginRuntime.GetOptionalDouble(parameters, "z");
    var newHeight = PluginRuntime.GetOptionalDouble(parameters, "height");
    var newRotation = PluginRuntime.GetOptionalDouble(parameters, "rotation");

    if (newText == null && newHeight == null && newRotation == null && (newX == null || newY == null))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "updateTextContent requires 'text', 'height', 'rotation', or both 'x' and 'y'.");
    }

    long handleNumber;
    try
    {
      handleNumber = Convert.ToInt64(handleValue, 16);
    }
    catch (Exception ex) when (ex is FormatException or OverflowException)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Handle '{handleValue}' is not a valid hexadecimal handle.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var entityHandle = new Handle(handleNumber);
      var objectId = database.GetObjectId(false, entityHandle, 0);
      if (objectId.IsNull)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Entity with handle '{handleValue}' was not found.");
      }

      var entity = CivilObjectUtils.GetRequiredObject<Entity>(transaction, objectId, OpenMode.ForWrite);

      switch (entity)
      {
        case DBText dbText:
          if (newText != null)
          {
            dbText.TextString = newText;
          }

          if (newX is double dtx && newY is double dty)
          {
            dbText.Position = new Point3d(dtx, dty, newZ ?? dbText.Position.Z);
          }

          if (newHeight is double dth)
          {
            dbText.Height = dth;
          }

          if (newRotation is double dtr)
          {
            dbText.Rotation = dtr;
          }

          return new Dictionary<string, object?>
          {
            ["handle"] = handleValue,
            ["entityType"] = "DBText",
            ["text"] = dbText.TextString,
            ["x"] = dbText.Position.X,
            ["y"] = dbText.Position.Y,
            ["z"] = dbText.Position.Z,
            ["height"] = dbText.Height,
            ["rotation"] = dbText.Rotation,
            ["layer"] = dbText.Layer,
          };

        case MText mText:
          if (newText != null)
          {
            mText.Contents = newText;
          }

          if (newX is double mtx && newY is double mty)
          {
            mText.Location = new Point3d(mtx, mty, newZ ?? mText.Location.Z);
          }

          if (newHeight is double mth)
          {
            mText.TextHeight = mth;
          }

          if (newRotation is double mtr)
          {
            mText.Rotation = mtr;
          }

          return new Dictionary<string, object?>
          {
            ["handle"] = handleValue,
            ["entityType"] = "MText",
            ["text"] = mText.Contents,
            ["x"] = mText.Location.X,
            ["y"] = mText.Location.Y,
            ["z"] = mText.Location.Z,
            ["textHeight"] = mText.TextHeight,
            ["rotation"] = mText.Rotation,
            ["layer"] = mText.Layer,
          };

        case MLeader mLeader:
          if (newX != null || newY != null || newRotation != null)
          {
            throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "MLeader entities only support 'text' and 'height' updates, not position or rotation (leader vertices require dedicated leader APIs).");
          }

          var existingMText = mLeader.MText ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"MLeader '{handleValue}' has no editable text content.");
          if (newText != null)
          {
            existingMText.Contents = newText;
          }

          if (newHeight is double lh)
          {
            existingMText.TextHeight = lh;
          }

          mLeader.MText = existingMText;

          return new Dictionary<string, object?>
          {
            ["handle"] = handleValue,
            ["entityType"] = "MLeader",
            ["text"] = mLeader.MText?.Contents,
            ["x"] = mLeader.MText?.Location.X,
            ["y"] = mLeader.MText?.Location.Y,
            ["z"] = mLeader.MText?.Location.Z,
            ["layer"] = mLeader.Layer,
          };

        default:
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Entity type '{entity.GetType().Name}' does not support text updates. Supported types: DBText, MText, MLeader.");
      }
    });
  }

  public static Task<object?> ListBlockReferencesAsync(JsonObject? parameters)
  {
    var containsFilter = PluginRuntime.GetOptionalString(parameters, "contains");
    var layerFilter = PluginRuntime.GetOptionalString(parameters, "layer");
    var requestedLimit = PluginRuntime.GetOptionalInt(parameters, "limit") ?? 200;
    var limit = Math.Clamp(requestedLimit, 1, 500);
    var spaceFilter = (PluginRuntime.GetOptionalString(parameters, "space") ?? "all").ToLowerInvariant();
    var includeAttributes = PluginRuntime.GetOptionalInt(parameters, "includeAttributes") != 0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);

      foreach (DBDictionaryEntry layoutEntry in layoutDict)
      {
        if (results.Count >= limit)
        {
          break;
        }

        var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
        var isModelSpace = layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase);
        if (spaceFilter == "model" && !isModelSpace)
        {
          continue;
        }

        if (spaceFilter == "paper" && isModelSpace)
        {
          continue;
        }

        var spaceBlock = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
        foreach (ObjectId objectId in spaceBlock)
        {
          if (results.Count >= limit)
          {
            break;
          }

          if (transaction.GetObject(objectId, OpenMode.ForRead) is not BlockReference blockRef)
          {
            continue;
          }

          var effectiveName = GetEffectiveBlockName(blockRef, transaction);

          if (!string.IsNullOrWhiteSpace(containsFilter) && effectiveName.IndexOf(containsFilter, StringComparison.OrdinalIgnoreCase) < 0)
          {
            continue;
          }

          if (!string.IsNullOrWhiteSpace(layerFilter) && !string.Equals(blockRef.Layer, layerFilter, StringComparison.OrdinalIgnoreCase))
          {
            continue;
          }

          results.Add(BuildBlockEntry(blockRef, effectiveName, layout.LayoutName, isModelSpace, transaction, includeAttributes));
        }
      }

      return new Dictionary<string, object?>
      {
        ["entities"] = results,
      };
    });
  }

  private static string GetEffectiveBlockName(BlockReference blockRef, Transaction transaction)
  {
    var btrId = blockRef.IsDynamicBlock ? blockRef.DynamicBlockTableRecord : blockRef.BlockTableRecord;
    var btr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, btrId, OpenMode.ForRead);
    return btr.Name;
  }

  private static Dictionary<string, object?> BuildBlockEntry(BlockReference blockRef, string effectiveName, string layoutName, bool isModelSpace, Transaction transaction, bool includeAttributes)
  {
    var entry = new Dictionary<string, object?>
    {
      ["handle"] = CivilObjectUtils.GetHandle(blockRef),
      ["blockName"] = effectiveName,
      ["isDynamicBlock"] = blockRef.IsDynamicBlock,
      ["x"] = blockRef.Position.X,
      ["y"] = blockRef.Position.Y,
      ["z"] = blockRef.Position.Z,
      ["rotation"] = blockRef.Rotation,
      ["scaleX"] = blockRef.ScaleFactors.X,
      ["scaleY"] = blockRef.ScaleFactors.Y,
      ["scaleZ"] = blockRef.ScaleFactors.Z,
      ["layer"] = blockRef.Layer,
      ["colorIndex"] = blockRef.ColorIndex,
      ["space"] = isModelSpace ? "model" : "paper",
      ["layout"] = layoutName,
    };

    if (includeAttributes && blockRef.AttributeCollection.Count > 0)
    {
      var attributes = new List<Dictionary<string, object?>>();
      foreach (ObjectId attributeId in blockRef.AttributeCollection)
      {
        if (transaction.GetObject(attributeId, OpenMode.ForRead) is not AttributeReference attributeRef)
        {
          continue;
        }

        attributes.Add(new Dictionary<string, object?>
        {
          ["tag"] = attributeRef.Tag,
          ["value"] = attributeRef.TextString,
        });
      }

      entry["attributes"] = attributes;
    }

    return entry;
  }

  public static Task<object?> UpdateBlockReferenceAsync(JsonObject? parameters)
  {
    var handleValue = PluginRuntime.GetRequiredString(parameters, "handle");
    var newBlockName = PluginRuntime.GetOptionalString(parameters, "blockName");
    var newX = PluginRuntime.GetOptionalDouble(parameters, "x");
    var newY = PluginRuntime.GetOptionalDouble(parameters, "y");
    var newZ = PluginRuntime.GetOptionalDouble(parameters, "z");
    var newRotation = PluginRuntime.GetOptionalDouble(parameters, "rotation");
    var newScaleX = PluginRuntime.GetOptionalDouble(parameters, "scaleX");
    var newScaleY = PluginRuntime.GetOptionalDouble(parameters, "scaleY");
    var newScaleZ = PluginRuntime.GetOptionalDouble(parameters, "scaleZ");
    var attributesNode = PluginRuntime.GetParameter(parameters, "attributes") as JsonArray;

    if (newBlockName == null && newRotation == null && attributesNode == null &&
        newScaleX == null && newScaleY == null && newScaleZ == null && (newX == null || newY == null))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "updateBlockReference requires at least one of 'blockName', 'rotation', 'scaleX'/'scaleY'/'scaleZ', 'attributes', or both 'x' and 'y'.");
    }

    long handleNumber;
    try
    {
      handleNumber = Convert.ToInt64(handleValue, 16);
    }
    catch (Exception ex) when (ex is FormatException or OverflowException)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Handle '{handleValue}' is not a valid hexadecimal handle.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var entityHandle = new Handle(handleNumber);
      var objectId = database.GetObjectId(false, entityHandle, 0);
      if (objectId.IsNull)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Entity with handle '{handleValue}' was not found.");
      }

      if (transaction.GetObject(objectId, OpenMode.ForWrite) is not BlockReference blockRef)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Entity '{handleValue}' is not a block reference.");
      }

      if (!string.IsNullOrWhiteSpace(newBlockName))
      {
        var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
        if (!blockTable.Has(newBlockName))
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Block definition '{newBlockName}' does not exist in this drawing. Insert/import it first.");
        }

        blockRef.BlockTableRecord = blockTable[newBlockName];
      }

      if (newX is double bx && newY is double by)
      {
        blockRef.Position = new Point3d(bx, by, newZ ?? blockRef.Position.Z);
      }

      if (newRotation is double brot)
      {
        blockRef.Rotation = brot;
      }

      if (newScaleX.HasValue || newScaleY.HasValue || newScaleZ.HasValue)
      {
        var current = blockRef.ScaleFactors;
        blockRef.ScaleFactors = new Scale3d(newScaleX ?? current.X, newScaleY ?? current.Y, newScaleZ ?? current.Z);
      }

      if (attributesNode != null)
      {
        foreach (var node in attributesNode)
        {
          if (node is not JsonObject attributeUpdate)
          {
            continue;
          }

          var tag = attributeUpdate["tag"]?.GetValue<string>();
          var value = attributeUpdate["value"]?.GetValue<string>();
          if (string.IsNullOrWhiteSpace(tag) || value == null)
          {
            continue;
          }

          foreach (ObjectId attributeId in blockRef.AttributeCollection)
          {
            if (transaction.GetObject(attributeId, OpenMode.ForWrite) is not AttributeReference attributeRef)
            {
              continue;
            }

            if (string.Equals(attributeRef.Tag, tag, StringComparison.OrdinalIgnoreCase))
            {
              attributeRef.TextString = value;
              break;
            }
          }
        }
      }

      var effectiveName = GetEffectiveBlockName(blockRef, transaction);

      return new Dictionary<string, object?>
      {
        ["handle"] = handleValue,
        ["blockName"] = effectiveName,
        ["x"] = blockRef.Position.X,
        ["y"] = blockRef.Position.Y,
        ["z"] = blockRef.Position.Z,
        ["rotation"] = blockRef.Rotation,
        ["scaleX"] = blockRef.ScaleFactors.X,
        ["scaleY"] = blockRef.ScaleFactors.Y,
        ["scaleZ"] = blockRef.ScaleFactors.Z,
        ["layer"] = blockRef.Layer,
      };
    });
  }

  public static Task<object?> InsertBlockReferenceAsync(JsonObject? parameters)
  {
    var blockName = PluginRuntime.GetRequiredString(parameters, "blockName");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;
    var scaleX = PluginRuntime.GetOptionalDouble(parameters, "scaleX") ?? 1d;
    var scaleY = PluginRuntime.GetOptionalDouble(parameters, "scaleY") ?? 1d;
    var scaleZ = PluginRuntime.GetOptionalDouble(parameters, "scaleZ") ?? 1d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var sourceFilePath = PluginRuntime.GetOptionalString(parameters, "sourceFilePath");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);

      if (!blockTable.Has(blockName))
      {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Block definition '{blockName}' does not exist in this drawing. Provide 'sourceFilePath' to import it from an external drawing.");
        }

        if (!File.Exists(sourceFilePath))
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Source file '{sourceFilePath}' does not exist.");
        }

        using var sourceDb = new Database(false, true);
        try
        {
          sourceDb.ReadDwgFile(sourceFilePath, FileOpenMode.OpenForReadAndAllShare, true, null);
        }
        catch (Exception ex)
        {
          throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Failed to read source drawing '{sourceFilePath}': {ex.Message}");
        }

        ObjectId sourceBlockId;
        using (var sourceTransaction = sourceDb.TransactionManager.StartTransaction())
        {
          var sourceBlockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(sourceTransaction, sourceDb.BlockTableId, OpenMode.ForRead);
          if (!sourceBlockTable.Has(blockName))
          {
            throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Block '{blockName}' was not found in '{sourceFilePath}'.");
          }

          sourceBlockId = sourceBlockTable[blockName];
          sourceTransaction.Commit();
        }

        try
        {
          var idsToClone = new ObjectIdCollection { sourceBlockId };
          var mapping = new IdMapping();
          database.WblockCloneObjects(idsToClone, database.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
        }
        catch (Exception ex)
        {
          throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Failed to import block '{blockName}' from '{sourceFilePath}': {ex.Message}");
        }

        blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
        if (!blockTable.Has(blockName))
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Block '{blockName}' was not found after importing from '{sourceFilePath}'.");
        }
      }

      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var blockRef = new BlockReference(new Point3d(x, y, z), blockTable[blockName])
      {
        Rotation = rotation,
        ScaleFactors = new Scale3d(scaleX, scaleY, scaleZ),
      };

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        blockRef.LayerId = layerId;
      }

      var blockRefId = modelSpace.AppendEntity(blockRef);
      transaction.AddNewlyCreatedDBObject(blockRef, true);

      var created = CivilObjectUtils.GetRequiredObject<BlockReference>(transaction, blockRefId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["blockName"] = blockName,
        ["x"] = created.Position.X,
        ["y"] = created.Position.Y,
        ["z"] = created.Position.Z,
        ["rotation"] = created.Rotation,
        ["scaleX"] = created.ScaleFactors.X,
        ["scaleY"] = created.ScaleFactors.Y,
        ["scaleZ"] = created.ScaleFactors.Z,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> EraseEntityAsync(JsonObject? parameters)
  {
    var handleValue = PluginRuntime.GetRequiredString(parameters, "handle");

    long handleNumber;
    try
    {
      handleNumber = Convert.ToInt64(handleValue, 16);
    }
    catch (Exception ex) when (ex is FormatException or OverflowException)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Handle '{handleValue}' is not a valid hexadecimal handle.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var entityHandle = new Handle(handleNumber);
      var objectId = database.GetObjectId(false, entityHandle, 0);
      if (objectId.IsNull)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Entity with handle '{handleValue}' was not found.");
      }

      var entity = CivilObjectUtils.GetRequiredObject<Entity>(transaction, objectId, OpenMode.ForWrite);
      var entityType = entity.GetType().Name;
      var layerName = entity.Layer;

      entity.Erase();

      return new Dictionary<string, object?>
      {
        ["handle"] = handleValue,
        ["entityType"] = entityType,
        ["layer"] = layerName,
        ["erased"] = true,
      };
    });
  }
}
