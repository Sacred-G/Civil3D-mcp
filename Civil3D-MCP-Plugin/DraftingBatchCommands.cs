using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Text.Json.Nodes;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

// Creates many drafting entities (lines, polylines, text, mtext, mleaders, aligned dimensions,
// block references, layout viewports) in ONE transaction and ONE approval, each with explicit layer, color, linetype,
// lineweight and style. Used to replay a firm drafting standard (layer/style recipes extracted from
// a finished sheet) on a new project without one tool call per entity. All-or-nothing: any invalid
// item aborts the whole batch.
public static class DraftingBatchCommands
{
  private const int MaxEntities = 500;

  public static Task<object?> CreateEntitiesAsync(JsonObject? parameters)
  {
    if (PluginRuntime.GetParameter(parameters, "entities") is not JsonArray items || items.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createEntities requires a non-empty 'entities' array.");
    }

    if (items.Count > MaxEntities)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"createEntities accepts at most {MaxEntities} entities per call; split the batch.");
    }

    var layerDefinitions = PluginRuntime.GetParameter(parameters, "layers") as JsonObject;
    var space = AcadCommands.ParseTargetSpace(parameters);
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layout");

    var hasViewports = items.OfType<JsonObject>().Any(item =>
      string.Equals(PluginRuntime.GetOptionalString(item, "kind")?.Trim(), "viewport", StringComparison.OrdinalIgnoreCase));

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // A new viewport can only be switched on while its layout is the active tab.
      if (hasViewports && space == "paper" && !string.IsNullOrWhiteSpace(layoutName)
        && !LayoutManager.Current.CurrentLayout.Equals(layoutName, StringComparison.OrdinalIgnoreCase))
      {
        LayoutManager.Current.CurrentLayout = layoutName;
      }

      // Annotative MLeaders/dimensions take their scale from CANNOSCALE of the ACTIVE tab: created from a layout tab they
      // get `1" = 1'` only and vanish (or blow up x20 once 1:20 is added) in the sheet viewports. Draft them from the Model tab.
      if (space == "model" && items.OfType<JsonObject>().Any(item =>
        (PluginRuntime.GetOptionalString(item, "kind")?.Trim().ToLowerInvariant()) is "mleader" or "aligned_dimension")
        && !LayoutManager.Current.CurrentLayout.Equals("Model", StringComparison.OrdinalIgnoreCase))
      {
        LayoutManager.Current.CurrentLayout = "Model";
      }

      var (targetSpace, targetLayoutName) = AcadCommands.ResolveTargetSpace(database, transaction, space, layoutName);
      var layersCreated = EnsureLayers(database, transaction, layerDefinitions);
      var created = new List<Dictionary<string, object?>>();

      for (var index = 0; index < items.Count; index++)
      {
        if (items[index] is not JsonObject item)
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"entities[{index}] must be an object.");
        }

        var kind = (PluginRuntime.GetOptionalString(item, "kind") ?? string.Empty).Trim().ToLowerInvariant();
        Entity entity;
        try
        {
          entity = kind switch
          {
            "line" => BuildLine(item),
            "polyline" => BuildPolyline(item),
            "text" => BuildText(database, transaction, item),
            "mtext" => BuildMText(database, transaction, item),
            "mleader" => BuildMLeader(database, transaction, item),
            "aligned_dimension" => BuildAlignedDimension(database, transaction, item),
            "block" => BuildBlock(transaction, database, item),
            "viewport" => space == "paper"
              ? BuildViewport(item)
              : throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "viewport items need space 'paper' and a layout."),
            _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
              "kind must be one of line, polyline, text, mtext, mleader, aligned_dimension, block, viewport."),
          };
        }
        catch (JsonRpcDispatchException ex)
        {
          throw new JsonRpcDispatchException(ex.Code, $"entities[{index}] ({kind}): {ex.Message}");
        }

        using (entity)
        {
          ApplyCommonProperties(database, transaction, entity, item, index);
          targetSpace.AppendEntity(entity);
          transaction.AddNewlyCreatedDBObject(entity, true);
          if (entity is Dimension dimension)
          {
            DimensionViewportCommands.ApplyStyleAnnotative(database, transaction, dimension);
            DimensionViewportCommands.ApplyDimOverrides(dimension, item);
            dimension.RecomputeDimensionBlock(true);
          }

          if (entity is MLeader mleader)
          {
            OrientMLeader(mleader, item);
          }

          var result = new Dictionary<string, object?>
          {
            ["index"] = index,
            ["kind"] = kind,
            ["handle"] = entity.Handle.ToString(),
            ["layer"] = entity.Layer,
          };
          if (entity is Viewport viewport)
          {
            result["on"] = ConfigureViewport(database, viewport, item);
          }

          created.Add(result);
        }
      }

      return new Dictionary<string, object?>
      {
        ["createdCount"] = created.Count,
        ["created"] = created,
        ["layersCreated"] = layersCreated,
        ["space"] = space,
        ["layout"] = targetLayoutName,
      };
    });
  }

  // Moves entities (any space) by a displacement, e.g. to put a template's paper-space notes where the
  // reference sheet has them. All-or-nothing: an unknown handle aborts the move.
  public static Task<object?> MoveEntitiesAsync(JsonObject? parameters)
  {
    if (PluginRuntime.GetParameter(parameters, "handles") is not JsonArray handles || handles.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "moveEntities requires a non-empty 'handles' array.");
    }

    var displacement = new Vector3d(
      PluginRuntime.GetRequiredDouble(parameters, "dx"),
      PluginRuntime.GetRequiredDouble(parameters, "dy"),
      PluginRuntime.GetOptionalDouble(parameters, "dz") ?? 0d);

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var moved = new List<Dictionary<string, object?>>();
      foreach (var node in handles)
      {
        var handleText = node?.GetValue<string>() ?? string.Empty;
        long value;
        try
        {
          value = Convert.ToInt64(handleText, 16);
        }
        catch (System.Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Handle '{handleText}' is not a valid hexadecimal handle.");
        }

        var objectId = database.GetObjectId(false, new Handle(value), 0);
        if (objectId.IsNull || objectId.IsErased)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Entity with handle '{handleText}' was not found.");
        }

        var entity = CivilObjectUtils.GetRequiredObject<Entity>(transaction, objectId, OpenMode.ForWrite);
        entity.TransformBy(Matrix3d.Displacement(displacement));
        moved.Add(new Dictionary<string, object?> { ["handle"] = handleText, ["type"] = entity.GetType().Name, ["layer"] = entity.Layer });
      }

      return new Dictionary<string, object?> { ["movedCount"] = moved.Count, ["moved"] = moved, ["dx"] = displacement.X, ["dy"] = displacement.Y };
    });
  }

  // selectEntities: leaves the entities SELECTED (implied selection = the grips / highlight the user sees, ready for a command) and zooms the
  // model view onto them. Read-only: nothing in the drawing changes. Switches to the Model tab when the active layout is a paper layout.
  public static Task<object?> SelectEntitiesAsync(JsonObject? parameters)
  {
    if (PluginRuntime.GetParameter(parameters, "handles") is not JsonArray handles || handles.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "selectEntities requires a non-empty 'handles' array.");
    }

    var zoom = PluginRuntime.GetOptionalBool(parameters, "zoom") ?? true;
    var margin = Math.Max(1.0, PluginRuntime.GetOptionalDouble(parameters, "zoomMargin") ?? 1.5);
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var ids = new List<ObjectId>();
      var missing = new List<string>();
      Extents3d? box = null;
      foreach (var node in handles)
      {
        var text = node?.GetValue<string>() ?? string.Empty;
        long value;
        try { value = Convert.ToInt64(text, 16); }
        catch (System.Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Handle '{text}' is not a valid hexadecimal handle.");
        }

        var id = database.GetObjectId(false, new Handle(value), 0);
        if (id.IsNull || id.IsErased) { missing.Add(text); continue; }
        ids.Add(id);
        if (transaction.GetObject(id, OpenMode.ForRead) is Entity entity)
        {
          try
          {
            var extents = entity.GeometricExtents;
            if (box.HasValue) { var merged = box.Value; merged.AddExtents(extents); box = merged; } else { box = extents; }
          }
          catch (Autodesk.AutoCAD.Runtime.Exception) { /* entities without extents are still selected */ }
        }
      }

      if (ids.Count == 0)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "None of the handles exist in the active drawing.");
      }

      var editor = doc.Editor;
      var zoomed = false;
      if (zoom && box.HasValue)
      {
        if (Convert.ToInt32(App.GetSystemVariable("TILEMODE")) == 0) App.SetSystemVariable("TILEMODE", 1);   // Model tab
        var view = editor.GetCurrentView();
        // WCS -> DCS (Kean Walmsley's recipe): the Model tab is usually twisted (the street runs horizontally on the sheet)
        var toDcs = Matrix3d.PlaneToWorld(view.ViewDirection);
        toDcs = Matrix3d.Displacement(view.Target - Point3d.Origin) * toDcs;
        toDcs = Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) * toDcs;
        toDcs = toDcs.Inverse();
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
        var lo = box.Value.MinPoint; var hi = box.Value.MaxPoint;
        foreach (var x in new[] { lo.X, hi.X })
          foreach (var y in new[] { lo.Y, hi.Y })
          {
            var p = new Point3d(x, y, lo.Z).TransformBy(toDcs);
            minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X); minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
          }

        var aspect = view.Width / view.Height;
        var width = Math.Max((maxX - minX) * margin, 20d);
        var height = Math.Max((maxY - minY) * margin, 20d);
        if (width / height > aspect) height = width / aspect; else width = height * aspect;
        view.CenterPoint = new Point2d((minX + maxX) / 2, (minY + maxY) / 2);
        view.Width = width;
        view.Height = height;
        editor.SetCurrentView(view);
        zoomed = true;
      }

      editor.SetImpliedSelection(ids.ToArray());
      editor.UpdateScreen();
      return new Dictionary<string, object?>
      {
        ["selected"] = ids.Count,
        ["missing"] = missing,
        ["zoomed"] = zoomed,
        ["extents"] = box.HasValue ? new Dictionary<string, object?> { ["minX"] = box.Value.MinPoint.X, ["minY"] = box.Value.MinPoint.Y, ["maxX"] = box.Value.MaxPoint.X, ["maxY"] = box.Value.MaxPoint.Y } : null,
        ["document"] = doc.Name,
      };
    });
  }

  // Layers named in `layers` are created with those properties when missing; existing layers are
  // left untouched so a project's own layer overrides survive.
  private static List<string> EnsureLayers(Database database, Transaction transaction, JsonObject? definitions)
  {
    var createdNames = new List<string>();
    if (definitions == null)
    {
      return createdNames;
    }

    var layerTable = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForRead);
    foreach (var (name, node) in definitions)
    {
      if (layerTable.Has(name))
      {
        continue;
      }

      var definition = node as JsonObject;
      var layer = new LayerTableRecord { Name = name };
      var colorIndex = PluginRuntime.GetOptionalInt(definition, "colorIndex");
      if (colorIndex.HasValue)
      {
        layer.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex.Value);
      }

      var linetype = PluginRuntime.GetOptionalString(definition, "linetype");
      if (!string.IsNullOrWhiteSpace(linetype))
      {
        layer.LinetypeObjectId = LookupUtils.GetOrLoadLinetypeId(database, transaction, linetype);
      }

      var lineweight = PluginRuntime.GetOptionalInt(definition, "lineweight");
      if (lineweight.HasValue)
      {
        layer.LineWeight = (LineWeight)lineweight.Value;
      }

      var plot = PluginRuntime.GetOptionalBool(definition, "plot");
      if (plot.HasValue)
      {
        layer.IsPlottable = plot.Value;
      }

      layerTable.UpgradeOpen();
      layerTable.Add(layer);
      transaction.AddNewlyCreatedDBObject(layer, true);
      createdNames.Add(name);
    }

    return createdNames;
  }

  private static void ApplyCommonProperties(Database database, Transaction transaction, Entity entity, JsonObject item, int index)
  {
    var layerName = PluginRuntime.GetOptionalString(item, "layer");
    if (!string.IsNullOrWhiteSpace(layerName))
    {
      var layerTable = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForRead);
      if (!layerTable.Has(layerName))
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
          $"entities[{index}]: layer '{layerName}' does not exist. Define it in 'layers' so it is created with the standard's properties.");
      }

      entity.LayerId = layerTable[layerName];
    }

    var colorIndex = PluginRuntime.GetOptionalInt(item, "colorIndex");
    if (colorIndex.HasValue)
    {
      entity.Color = colorIndex.Value switch
      {
        256 => Color.FromColorIndex(ColorMethod.ByLayer, 256),
        0 => Color.FromColorIndex(ColorMethod.ByBlock, 0),
        _ => Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex.Value),
      };
    }

    var linetype = PluginRuntime.GetOptionalString(item, "linetype");
    if (!string.IsNullOrWhiteSpace(linetype))
    {
      entity.LinetypeId = LookupUtils.GetOrLoadLinetypeId(database, transaction, linetype);
    }

    var linetypeScale = PluginRuntime.GetOptionalDouble(item, "linetypeScale");
    if (linetypeScale.HasValue)
    {
      entity.LinetypeScale = linetypeScale.Value;
    }

    var lineweight = PluginRuntime.GetOptionalInt(item, "lineweight");
    if (lineweight.HasValue)
    {
      entity.LineWeight = (LineWeight)lineweight.Value;
    }
  }

  private static List<Point2d> ReadPoints(JsonObject item, int minimum)
  {
    if (PluginRuntime.GetParameter(item, "points") is not JsonArray pointsNode || pointsNode.Count < minimum)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"needs 'points' with at least {minimum} {{x, y}} items.");
    }

    return pointsNode.Select(node => node is JsonObject point
        ? new Point2d(
          point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "point is missing x."),
          point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "point is missing y."))
        : throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "each point must be {x, y}."))
      .ToList();
  }

  private static Entity BuildLine(JsonObject item)
  {
    var points = ReadPoints(item, 2);
    return new Line(new Point3d(points[0].X, points[0].Y, 0), new Point3d(points[1].X, points[1].Y, 0));
  }

  private static Entity BuildPolyline(JsonObject item)
  {
    var points = ReadPoints(item, 2);
    var width = PluginRuntime.GetOptionalDouble(item, "constantWidth") ?? 0d;
    var polyline = new Polyline();
    for (var i = 0; i < points.Count; i++)
    {
      polyline.AddVertexAt(i, points[i], 0, width, width);
    }

    polyline.Closed = PluginRuntime.GetOptionalBool(item, "closed") ?? false;
    return polyline;
  }

  private static Entity BuildText(Database database, Transaction transaction, JsonObject item)
  {
    var text = new DBText
    {
      TextString = PluginRuntime.GetRequiredString(item, "text"),
      Position = new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0),
      Height = PluginRuntime.GetOptionalDouble(item, "height") ?? 2.5d,
      Rotation = PluginRuntime.GetOptionalDouble(item, "rotation") ?? 0d,
    };
    var styleId = ResolveTextStyleId(database, transaction, PluginRuntime.GetOptionalString(item, "textStyle"));
    if (!styleId.IsNull)
    {
      text.TextStyleId = styleId;
    }

    return text;
  }

  private static Entity BuildMText(Database database, Transaction transaction, JsonObject item)
  {
    var mtext = new MText
    {
      Contents = PluginRuntime.GetRequiredString(item, "text"),
      Location = new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0),
      TextHeight = PluginRuntime.GetOptionalDouble(item, "height") ?? 2.5d,
      Rotation = PluginRuntime.GetOptionalDouble(item, "rotation") ?? 0d,
    };
    var styleId = ResolveTextStyleId(database, transaction, PluginRuntime.GetOptionalString(item, "textStyle"));
    if (!styleId.IsNull)
    {
      mtext.TextStyleId = styleId;
    }

    var width = PluginRuntime.GetOptionalDouble(item, "width");
    if (width is > 0)
    {
      mtext.Width = width.Value;
    }

    var attachment = PluginRuntime.GetOptionalInt(item, "attachment");
    if (attachment.HasValue)
    {
      if (attachment.Value is < 1 or > 9)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "attachment must be 1-9 (1 TopLeft, 2 TopCenter, 3 TopRight, 4 MiddleLeft, 5 MiddleCenter, 6 MiddleRight, 7 BottomLeft, 8 BottomCenter, 9 BottomRight).");
      }

      mtext.Attachment = (AttachmentPoint)attachment.Value;
    }

    ApplyMTextMask(mtext, item);
    return mtext;
  }

  // Background mask (MTEXT group 90=3, 45=scale): the fill uses the drawing background, so street/subject labels stay legible
  // over hatches and linework (the X-TOPO asphalt fill otherwise covers them). backgroundScale 1.2 = street labels, 1.0 = subject.
  internal static void ApplyMTextMask(MText mtext, JsonObject item)
  {
    var mask = PluginRuntime.GetOptionalBool(item, "backgroundMask");
    if (mask == null)
    {
      return;
    }

    if (mask.Value)
    {
      mtext.BackgroundFill = true;
      mtext.UseBackgroundColor = true;
      mtext.BackgroundScaleFactor = PluginRuntime.GetOptionalDouble(item, "backgroundScale") ?? 1.2d;
    }
    else
    {
      mtext.BackgroundFill = false;
    }
  }

  // Moves an MLeader's text (arrow tip stays) and re-picks the anchor side like OrientMLeader does at creation:
  // TopLeft + forward dogleg when the text is ahead of the arrow along the reading direction, TopRight + backward otherwise.
  internal static void MoveMLeaderText(MLeader mleader, Point3d textPoint, double? rotation)
  {
    var leaderIndexes = mleader.GetLeaderIndexes();
    if (leaderIndexes.Count == 0)
    {
      mleader.TextLocation = textPoint;
      return;
    }

    var leaderIndex = (int)leaderIndexes[0];
    var lineIndexes = mleader.GetLeaderLineIndexes(leaderIndex);
    var arrow = lineIndexes.Count > 0 ? mleader.GetFirstVertex((int)lineIndexes[0]) : textPoint;
    using var mtext = mleader.MText;
    var angle = rotation ?? mtext.Rotation;
    var along = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
    var forward = (textPoint - arrow).DotProduct(along) >= 0;
    if (rotation.HasValue)
    {
      mleader.TextAngleType = TextAngleType.InsertAngle;
      mtext.Rotation = angle;
    }

    mtext.Attachment = forward ? AttachmentPoint.TopLeft : AttachmentPoint.TopRight;
    mleader.MText = mtext;
    mleader.SetDogleg(leaderIndex, forward ? along : along.Negate());
    // Setting the text or dogleg moves the text to AutoCAD's default landing; put it back last.
    mleader.TextLocation = textPoint;
  }

  private static Entity BuildMLeader(Database database, Transaction transaction, JsonObject item)
  {
    var text = PluginRuntime.GetRequiredString(item, "text");
    var arrowPoint = new Point3d(PluginRuntime.GetRequiredDouble(item, "leaderX"), PluginRuntime.GetRequiredDouble(item, "leaderY"), 0);
    var textPoint = new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0);
    var styleName = PluginRuntime.GetOptionalString(item, "mLeaderStyle");

    var mleader = new MLeader();
    mleader.SetDatabaseDefaults(database);
    var styleDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.MLeaderStyleDictionaryId, OpenMode.ForRead);
    if (!string.IsNullOrWhiteSpace(styleName))
    {
      if (!styleDict.Contains(styleName))
      {
        var available = string.Join(", ", styleDict.Cast<DBDictionaryEntry>().Select(entry => entry.Key));
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"MLeader style '{styleName}' not found. Available: {available}.");
      }

      mleader.MLeaderStyle = styleDict.GetAt(styleName);
    }

    // The text takes the MLeader style's text style (a new MText would otherwise use the drawing's
    // current text style, which draws wider text and wraps differently from the reference sheet).
    var leaderStyle = CivilObjectUtils.GetRequiredObject<MLeaderStyle>(transaction, mleader.MLeaderStyle, OpenMode.ForRead);
    using var mtext = new MText { Contents = text, Location = textPoint, TextStyleId = leaderStyle.TextStyleId };
    mleader.TextStyleId = leaderStyle.TextStyleId;
    var height = PluginRuntime.GetOptionalDouble(item, "height");
    if (height.HasValue)
    {
      mtext.TextHeight = height.Value;
    }

    // Text box width in model units (e.g. 23.3525 = 1.17" at 1"=20'): the text wraps like the reference
    // sheet's leaders instead of running on one line.
    var width = PluginRuntime.GetOptionalDouble(item, "width");
    if (width.HasValue && width.Value > 0)
    {
      mtext.Width = width.Value;
    }

    mleader.MText = mtext;
    // One leader line from the arrow; AutoCAD derives the landing and dogleg from the text location.
    // Adding the text point as a second vertex drew the line back across the text.
    mleader.AddLeaderLine(arrowPoint);
    return mleader;
  }

  // Scale, text angle and dogleg need a database-resident MLeader (eInvalidContext otherwise), so they
  // are applied after AppendEntity. On a twisted sheet viewport the text is rotated to the street
  // angle to read horizontally, and the dogleg follows that direction toward the text side.
  private static void OrientMLeader(MLeader mleader, JsonObject item)
  {
    // Without a rotation the label is horizontal (0): still pick the anchor side from where the text sits relative to the
    // arrow, otherwise every label anchors TopLeft and the leader crosses its own text when the text is left of the arrow.
    var rotation = PluginRuntime.GetOptionalDouble(item, "rotation") ?? 0d;
    var scale = PluginRuntime.GetOptionalDouble(item, "scale");

    var height = PluginRuntime.GetOptionalDouble(item, "height");
    // An annotative MLeader takes its scale from the annotation scale (CANNOSCALE); setting Scale throws eInvalidContext.
    if (scale.HasValue && mleader.Annotative != AnnotativeStates.True)
    {
      mleader.Scale = scale.Value;
    }

    var arrow = new Point3d(PluginRuntime.GetRequiredDouble(item, "leaderX"), PluginRuntime.GetRequiredDouble(item, "leaderY"), 0);
    var textPoint = new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0);
    var along = new Vector3d(Math.Cos(rotation), Math.Sin(rotation), 0);
    // Text on the far side of the arrow along the reading direction: dogleg forward, text hangs from its
    // top-left corner; otherwise dogleg backward and the text hangs from its top-right corner (as drafted by hand).
    var forward = (textPoint - arrow).DotProduct(along) >= 0;

    using var mtext = mleader.MText;
    if (height.HasValue)
    {
      mtext.TextHeight = height.Value;
    }

    mleader.TextAngleType = TextAngleType.InsertAngle;
    mtext.Rotation = rotation;
    mtext.Attachment = forward ? AttachmentPoint.TopLeft : AttachmentPoint.TopRight;

    mleader.MText = mtext;
    var leaderIndexes = mleader.GetLeaderIndexes();
    if (leaderIndexes.Count > 0)
    {
      mleader.SetDogleg((int)leaderIndexes[0], forward ? along : along.Negate());
      // Setting the text or dogleg moves the text to AutoCAD's default landing; put it back last.
      mleader.TextLocation = textPoint;
    }
  }

  private static Entity BuildAlignedDimension(Database database, Transaction transaction, JsonObject item)
  {
    var p1 = new Point3d(PluginRuntime.GetRequiredDouble(item, "x1"), PluginRuntime.GetRequiredDouble(item, "y1"), 0);
    var p2 = new Point3d(PluginRuntime.GetRequiredDouble(item, "x2"), PluginRuntime.GetRequiredDouble(item, "y2"), 0);
    if (p1.DistanceTo(p2) < 1e-9)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "the two dimension points are identical.");
    }

    var dimension = new AlignedDimension();
    dimension.SetDatabaseDefaults(database);
    dimension.XLine1Point = p1;
    dimension.XLine2Point = p2;
    dimension.DimLinePoint = DimensionViewportCommands.ComputeDimLinePoint(
      p1, p2,
      PluginRuntime.GetOptionalDouble(item, "dimLineX"),
      PluginRuntime.GetOptionalDouble(item, "dimLineY"),
      PluginRuntime.GetOptionalDouble(item, "offset") ?? 0d);
    dimension.DimensionStyle = DimensionViewportCommands.ResolveDimStyleId(database, transaction, PluginRuntime.GetOptionalString(item, "dimStyle"));
    dimension.DimensionText = PluginRuntime.GetOptionalString(item, "textOverride") ?? string.Empty;
    return dimension;
  }

  private static Entity BuildBlock(Transaction transaction, Database database, JsonObject item)
  {
    var blockName = PluginRuntime.GetRequiredString(item, "blockName");
    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
    if (!blockTable.Has(blockName))
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
        $"block '{blockName}' is not defined in this drawing. Import it first with acad_insert_block_reference (sourceFilePath).");
    }

    var scale = PluginRuntime.GetOptionalDouble(item, "scale") ?? 1d;
    return new BlockReference(
      new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0),
      blockTable[blockName])
    {
      Rotation = PluginRuntime.GetOptionalDouble(item, "rotation") ?? 0d,
      ScaleFactors = new Scale3d(scale),
    };
  }

  private static Entity BuildViewport(JsonObject item)
  {
    return new Viewport
    {
      CenterPoint = new Point3d(PluginRuntime.GetRequiredDouble(item, "x"), PluginRuntime.GetRequiredDouble(item, "y"), 0),
      Width = PluginRuntime.GetRequiredDouble(item, "width"),
      Height = PluginRuntime.GetRequiredDouble(item, "height"),
    };
  }

  // Plan view centered on (targetX, targetY): ViewCenter 0 puts the target at the viewport center for
  // any twist, the same convention acad_set_viewport_twist uses. Returns whether the viewport is on.
  // A viewport made through the API has NO annotation scale of its own, so annotative dimensions/MLeaders drawn under
  // CANNOSCALE `1" = 20'` are hidden in it (0 dimension words in the plot). Give it the annotation scale that matches its
  // custom scale (`1" = 20'` for 0.05), like a viewport made with MVIEW.
  internal static string? TryApplyViewportAnnotationScale(Database database, Viewport viewport)
  {
    var custom = viewport.CustomScale;
    if (custom <= 0)
    {
      return null;
    }

    // Prefer the standard name `1" = 20'` (with spaces). A drawing can also hold a stray custom `1"=20'` (typed without
    // spaces at the -OBJECTSCALE prompt); matching that one leaves the viewport on a scale no object carries.
    var standard = $"1\" = {Math.Round(1 / custom, 2)}'";
    AnnotationScale? loose = null;
    AnnotationScale? byRatio = null;
    foreach (ObjectContext context in database.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES"))
    {
      if (context is not AnnotationScale candidate)
      {
        continue;
      }

      if (candidate.Name.Equals(standard, StringComparison.Ordinal))
      {
        viewport.AnnotationScale = candidate;
        return candidate.Name;
      }

      if (loose == null && candidate.Name.Replace(" ", string.Empty).Equals(standard.Replace(" ", string.Empty), StringComparison.Ordinal))
      {
        loose = candidate;
      }

      if (byRatio == null && candidate.DrawingUnits > 0 && Math.Abs(candidate.PaperUnits / candidate.DrawingUnits - custom) < 1e-9)
      {
        byRatio = candidate;
      }
    }

    var chosen = loose ?? byRatio;
    if (chosen == null)
    {
      return null;
    }

    viewport.AnnotationScale = chosen;
    return chosen.Name;
  }

  private static bool ConfigureViewport(Database database, Viewport viewport, JsonObject item)
  {
    viewport.ViewDirection = Vector3d.ZAxis;
    viewport.ViewTarget = new Point3d(PluginRuntime.GetRequiredDouble(item, "targetX"), PluginRuntime.GetRequiredDouble(item, "targetY"), 0);
    viewport.ViewCenter = Point2d.Origin;
    viewport.TwistAngle = (PluginRuntime.GetOptionalDouble(item, "twistDegrees") ?? 0d) * Math.PI / 180d;
    viewport.CustomScale = PluginRuntime.GetOptionalDouble(item, "scale") ?? 1d;
    TryApplyViewportAnnotationScale(database, viewport);
    var on = true;
    try
    {
      viewport.On = true;
    }
    catch (Autodesk.AutoCAD.Runtime.Exception)
    {
      on = false;
    }

    viewport.Locked = PluginRuntime.GetOptionalBool(item, "locked") ?? false;
    return on;
  }

  private static ObjectId ResolveTextStyleId(Database database, Transaction transaction, string? styleName)
  {
    if (string.IsNullOrWhiteSpace(styleName))
    {
      return ObjectId.Null;
    }

    var styleTable = CivilObjectUtils.GetRequiredObject<TextStyleTable>(transaction, database.TextStyleTableId, OpenMode.ForRead);
    if (styleTable.Has(styleName))
    {
      return styleTable[styleName];
    }

    var available = new List<string>();
    foreach (ObjectId id in styleTable)
    {
      available.Add(CivilObjectUtils.GetRequiredObject<TextStyleTableRecord>(transaction, id, OpenMode.ForRead).Name);
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"text style '{styleName}' not found. Available: {string.Join(", ", available.Where(n => n.Length > 0))}.");
  }
}
