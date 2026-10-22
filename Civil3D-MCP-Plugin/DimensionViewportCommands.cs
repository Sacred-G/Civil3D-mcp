using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

// Dimensions (e.g. road right-of-way widths on plan sheets) and layout viewport twist
// (the API equivalent of MSPACE > DVIEW > TWist, used to turn the frontage street horizontal).
public static class DimensionViewportCommands
{
  public static Task<object?> ListDimensionsAsync(JsonObject? parameters)
  {
    var layerFilter = PluginRuntime.GetOptionalString(parameters, "layer");
    var spaceFilter = (PluginRuntime.GetOptionalString(parameters, "space") ?? "all").ToLowerInvariant();
    var limit = Math.Clamp(PluginRuntime.GetOptionalInt(parameters, "limit") ?? 200, 1, 500);
    var nearX = PluginRuntime.GetOptionalDouble(parameters, "nearX");
    var nearY = PluginRuntime.GetOptionalDouble(parameters, "nearY");
    var nearRadius = PluginRuntime.GetOptionalDouble(parameters, "nearRadius");
    var nearPoint = nearX.HasValue && nearY.HasValue ? new Point2d(nearX.Value, nearY.Value) : (Point2d?)null;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);

      foreach (DBDictionaryEntry layoutEntry in layoutDict)
      {
        var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
        var isModelSpace = layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase);
        if ((spaceFilter == "model" && !isModelSpace) || (spaceFilter == "paper" && isModelSpace))
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

          if (transaction.GetObject(objectId, OpenMode.ForRead) is not Dimension dimension)
          {
            continue;
          }

          if (!string.IsNullOrWhiteSpace(layerFilter) && !string.Equals(dimension.Layer, layerFilter, StringComparison.OrdinalIgnoreCase))
          {
            continue;
          }

          var entry = BuildDimensionEntry(dimension, layout.LayoutName, isModelSpace);
          if (nearPoint.HasValue && nearRadius.HasValue)
          {
            var anchor = entry["xLine1Point"] is double[] p1 && entry["xLine2Point"] is double[] p2
              ? new Point2d((p1[0] + p2[0]) / 2, (p1[1] + p2[1]) / 2)
              : new Point2d(dimension.TextPosition.X, dimension.TextPosition.Y);
            if (anchor.GetDistanceTo(nearPoint.Value) > nearRadius.Value)
            {
              continue;
            }
          }

          results.Add(entry);
        }
      }

      return new Dictionary<string, object?> { ["entities"] = results };
    });
  }

  public static Task<object?> CreateAlignedDimensionAsync(JsonObject? parameters)
  {
    var x1 = PluginRuntime.GetRequiredDouble(parameters, "x1");
    var y1 = PluginRuntime.GetRequiredDouble(parameters, "y1");
    var x2 = PluginRuntime.GetRequiredDouble(parameters, "x2");
    var y2 = PluginRuntime.GetRequiredDouble(parameters, "y2");
    var dimLineX = PluginRuntime.GetOptionalDouble(parameters, "dimLineX");
    var dimLineY = PluginRuntime.GetOptionalDouble(parameters, "dimLineY");
    var offset = PluginRuntime.GetOptionalDouble(parameters, "offset") ?? 0d;
    var dimStyleName = PluginRuntime.GetOptionalString(parameters, "dimStyle");
    var textOverride = PluginRuntime.GetOptionalString(parameters, "textOverride");
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var space = AcadCommands.ParseTargetSpace(parameters);
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layout");

    var p1 = new Point3d(x1, y1, 0);
    var p2 = new Point3d(x2, y2, 0);
    if (p1.DistanceTo(p2) < 1e-9)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "The two dimension points are identical.");
    }

    var dimLinePoint = ComputeDimLinePoint(p1, p2, dimLineX, dimLineY, offset);
    // A caller that names an explicit dimLineX/dimLineY (rather than just an offset) wants the text
    // fixed there. RecomputeDimensionBlock(true) below always re-runs AutoCAD's own DIMFIT placement,
    // which for a short dimension (e.g. a 5 ft easement callout at 1:20) can eject the text several
    // feet away from the line whenever it doesn't fit between the extension lines. Recompute(false)
    // keeps the point we set instead. Bug found 2026-09-28 (VILLA ONE FASE 1 easement dims 12235/12248).
    var hasExplicitDimLine = dimLineX.HasValue && dimLineY.HasValue;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var (targetSpace, targetLayoutName) = AcadCommands.ResolveTargetSpace(database, transaction, space, layoutName);
      var dimStyleId = ResolveDimStyleId(database, transaction, dimStyleName);

      using var dimension = new AlignedDimension();
      dimension.SetDatabaseDefaults(database);
      dimension.XLine1Point = p1;
      dimension.XLine2Point = p2;
      dimension.DimLinePoint = dimLinePoint;
      dimension.DimensionStyle = dimStyleId;
      dimension.DimensionText = textOverride ?? string.Empty;
      if (hasExplicitDimLine)
      {
        dimension.TextPosition = dimLinePoint;
        dimension.UsingDefaultTextPosition = false;
      }

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        dimension.LayerId = LookupUtils.GetLayerId(database, transaction, layerName);
      }

      var dimensionId = targetSpace.AppendEntity(dimension);
      transaction.AddNewlyCreatedDBObject(dimension, true);
      ApplyStyleAnnotative(database, transaction, dimension);
      ApplyDimOverrides(dimension, parameters);
      dimension.RecomputeDimensionBlock(!hasExplicitDimLine);
      if (hasExplicitDimLine)
      {
        dimension.TextPosition = dimLinePoint;
      }

      var created = CivilObjectUtils.GetRequiredObject<Dimension>(transaction, dimensionId, OpenMode.ForRead);
      var entry = BuildDimensionEntry(created, targetLayoutName, space == "model");
      return entry;
    });
  }

  // Per-dimension overrides a reference sheet's dimensions carry (e.g. DIMTAD 4 + DIMTXTDIRECTION on so
  // the text reads upright on a twisted sheet). Applied to the database-resident dimension.
  // Written as the DSTYLE xdata AutoCAD itself stores (only these codes). Setting Dimtad/Dimtxtdirection
  // through the properties also froze every other variable of an annotative dimension as an override,
  // at the wrong scale (DIMTXT 0.005 instead of the style's value).
  internal static void ApplyDimOverrides(Dimension dimension, JsonObject? item)
  {
    var tad = PluginRuntime.GetOptionalInt(item, "dimTad");
    var direction = PluginRuntime.GetOptionalBool(item, "dimTxtDirection");
    if (!tad.HasValue && !direction.HasValue)
    {
      return;
    }

    var values = new List<TypedValue>
    {
      new((int)DxfCode.ExtendedDataRegAppName, "ACAD"),
      new((int)DxfCode.ExtendedDataAsciiString, "DSTYLE"),
      new((int)DxfCode.ExtendedDataControlString, "{"),
    };
    if (tad.HasValue)
    {
      values.Add(new((int)DxfCode.ExtendedDataInteger16, (short)77));
      values.Add(new((int)DxfCode.ExtendedDataInteger16, (short)tad.Value));
    }

    if (direction.HasValue)
    {
      values.Add(new((int)DxfCode.ExtendedDataInteger16, (short)294));
      values.Add(new((int)DxfCode.ExtendedDataInteger16, (short)(direction.Value ? 1 : 0)));
    }

    values.Add(new((int)DxfCode.ExtendedDataControlString, "}"));
    using var buffer = new ResultBuffer(values.ToArray());
    dimension.XData = buffer;
  }

  // A dimension created through the API does not inherit the annotative flag of its style: with an annotative
  // style (e.g. BCC-1.0) it would draw at scale 1 (0.1 ft text, invisible on a 1"=20' sheet). Make it
  // annotative and give it the current annotation scale (CANNOSCALE), as the DIMLINEAR command does.
  internal static void ApplyStyleAnnotative(Database database, Transaction transaction, Dimension dimension)
  {
    if (transaction.GetObject(dimension.DimensionStyle, OpenMode.ForRead) is not DimStyleTableRecord style
      || style.Annotative != AnnotativeStates.True)
    {
      return;
    }

    dimension.Annotative = AnnotativeStates.True;
    var scale = database.Cannoscale;
    if (scale == null)
    {
      return;
    }

    if (!dimension.HasContext(scale))
    {
      dimension.AddContext(scale);
    }

    // Keep only the current annotation scale: a stray extra scale (e.g. 1" = 1') makes the dimension draw
    // at that scale in any viewport set to it (0.1 ft text on the profile sheet).
    var scales = database.ObjectContextManager?.GetContextCollection("ACDB_ANNOTATIONSCALES");
    if (scales == null)
    {
      return;
    }

    foreach (ObjectContext context in scales)
    {
      if (context.Name != scale.Name && dimension.HasContext(context))
      {
        dimension.RemoveContext(context);
      }
    }
  }

  public static Task<object?> ListViewportsAsync(JsonObject? parameters)
  {
    var layoutFilter = PluginRuntime.GetOptionalString(parameters, "layout");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var results = new List<Dictionary<string, object?>>();
      foreach (var (layout, viewport) in EnumerateViewports(database, transaction, layoutFilter))
      {
        results.Add(BuildViewportEntry(viewport, layout.LayoutName));
      }

      return new Dictionary<string, object?> { ["viewports"] = results };
    });
  }

  public static Task<object?> SetViewportTwistAsync(JsonObject? parameters)
  {
    var layoutName = PluginRuntime.GetRequiredString(parameters, "layout");
    var viewportHandle = PluginRuntime.GetOptionalString(parameters, "viewportHandle");
    var twistDegrees = PluginRuntime.GetOptionalDouble(parameters, "twistDegrees");
    var streetAngleDegrees = PluginRuntime.GetOptionalDouble(parameters, "streetAngleDegrees");
    var centerX = PluginRuntime.GetOptionalDouble(parameters, "centerX");
    var centerY = PluginRuntime.GetOptionalDouble(parameters, "centerY");

    if (twistDegrees.HasValue == streetAngleDegrees.HasValue)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Pass exactly one of twistDegrees or streetAngleDegrees.");
    }

    if (centerX.HasValue != centerY.HasValue)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "centerX and centerY must be given together.");
    }

    // A street at angle θ reads horizontal (left to right) once the view is twisted by 360° − θ.
    var twistRadians = NormalizeAngle((twistDegrees ?? (360d - streetAngleDegrees!.Value)) * Math.PI / 180d);

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      if (string.Equals(layoutName, "Model", StringComparison.OrdinalIgnoreCase))
      {
        return SetModelViewTwist(doc, twistRadians, centerX, centerY);
      }

      var candidates = EnumerateViewports(database, transaction, layoutName).ToList();
      if (candidates.Count == 0)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layout '{layoutName}' has no viewports. Use listViewports to see layouts.");
      }

      Viewport target;
      if (!string.IsNullOrWhiteSpace(viewportHandle))
      {
        target = candidates.Select(c => c.Viewport).FirstOrDefault(v => string.Equals(v.Handle.ToString(), viewportHandle, StringComparison.OrdinalIgnoreCase))
          ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Viewport '{viewportHandle}' not found in layout '{layoutName}'.");
      }
      else
      {
        // Default to the largest model viewport (skip the layout's own paper-space viewport).
        target = candidates.Select(c => c.Viewport).Where(v => !IsPaperSpaceViewport(v)).OrderByDescending(v => v.Width * v.Height).FirstOrDefault()
          ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layout '{layoutName}' has no model viewport.");
      }

      target.UpgradeOpen();
      var before = BuildViewportEntry(target, layoutName);
      var center = centerX.HasValue ? new Point2d(centerX.Value, centerY!.Value) : ModelCenter(target);

      var wasLocked = target.Locked;
      target.Locked = false;
      target.TwistAngle = twistRadians;
      target.ViewTarget = new Point3d(center.X, center.Y, target.ViewTarget.Z);
      target.ViewCenter = Point2d.Origin;
      target.Locked = wasLocked;

      return new Dictionary<string, object?>
      {
        ["before"] = before,
        ["after"] = BuildViewportEntry(target, layoutName),
      };
    });
  }

  // Without an explicit dimension-line point, place the line `offset` units to the left of p1->p2
  // (negative = right). offset 0 keeps the dimension line on the measured points.
  internal static Point3d ComputeDimLinePoint(Point3d p1, Point3d p2, double? dimLineX, double? dimLineY, double offset)
  {
    if (dimLineX.HasValue && dimLineY.HasValue)
    {
      return new Point3d(dimLineX.Value, dimLineY.Value, 0);
    }

    var direction = (p2 - p1).GetNormal();
    var left = new Vector3d(-direction.Y, direction.X, 0);
    return new Point3d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, 0) + left * offset;
  }

  internal static ObjectId ResolveDimStyleId(Database database, Transaction transaction, string? dimStyleName)
  {
    if (string.IsNullOrWhiteSpace(dimStyleName))
    {
      return database.Dimstyle;
    }

    var dimStyleTable = CivilObjectUtils.GetRequiredObject<DimStyleTable>(transaction, database.DimStyleTableId, OpenMode.ForRead);
    if (dimStyleTable.Has(dimStyleName))
    {
      return dimStyleTable[dimStyleName];
    }

    var available = new List<string>();
    foreach (ObjectId id in dimStyleTable)
    {
      available.Add(CivilObjectUtils.GetRequiredObject<DimStyleTableRecord>(transaction, id, OpenMode.ForRead).Name);
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Dimension style '{dimStyleName}' not found. Available: {string.Join(", ", available)}.");
  }

  private static IEnumerable<(Layout Layout, Viewport Viewport)> EnumerateViewports(Database database, Transaction transaction, string? layoutFilter)
  {
    var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);
    foreach (DBDictionaryEntry layoutEntry in layoutDict)
    {
      var layout = CivilObjectUtils.GetRequiredObject<Layout>(transaction, layoutEntry.Value, OpenMode.ForRead);
      if (layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      if (!string.IsNullOrWhiteSpace(layoutFilter) && !layout.LayoutName.Equals(layoutFilter, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      var spaceBlock = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
      foreach (ObjectId objectId in spaceBlock)
      {
        if (transaction.GetObject(objectId, OpenMode.ForRead) is Viewport viewport)
        {
          yield return (layout, viewport);
        }
      }
    }
  }

  // The layout's own "sheet" viewport shows paper space 1:1 (its view equals its frame).
  internal static bool IsPaperSpaceViewport(Viewport viewport) =>
    Math.Abs(viewport.ViewHeight - viewport.Height) < 1e-6 &&
    viewport.ViewCenter.GetDistanceTo(new Point2d(viewport.CenterPoint.X, viewport.CenterPoint.Y)) < 1e-6;

  // Plan view: DCS = R(twist)·(WCS − target), so WCS = target + R(−twist)·ViewCenter.
  private static Point2d ModelCenter(Viewport viewport)
  {
    var c = viewport.ViewCenter;
    var t = -viewport.TwistAngle;
    return new Point2d(
      viewport.ViewTarget.X + c.X * Math.Cos(t) - c.Y * Math.Sin(t),
      viewport.ViewTarget.Y + c.X * Math.Sin(t) + c.Y * Math.Cos(t));
  }

  // Model tab: twist the current model view (DVIEW TWist in model space) so the street reads
  // horizontally while drafting, and turn the crosshairs with it (SNAPANG = street angle). The
  // drawing opens on the Model tab afterwards, as a reference sheet saved from model space does.
  private static Dictionary<string, object?> SetModelViewTwist(
    Autodesk.AutoCAD.ApplicationServices.Document doc, double twistRadians, double? centerX, double? centerY)
  {
    if (!LayoutManager.Current.CurrentLayout.Equals("Model", StringComparison.OrdinalIgnoreCase))
    {
      LayoutManager.Current.CurrentLayout = "Model";
    }

    var editor = doc.Editor;
    using var view = editor.GetCurrentView();
    var beforeTwist = view.ViewTwist;
    var c = view.CenterPoint;
    var t = -view.ViewTwist;
    var currentCenter = new Point2d(
      view.Target.X + c.X * Math.Cos(t) - c.Y * Math.Sin(t),
      view.Target.Y + c.X * Math.Sin(t) + c.Y * Math.Cos(t));
    var center = centerX.HasValue ? new Point2d(centerX.Value, centerY!.Value) : currentCenter;

    view.ViewDirection = Vector3d.ZAxis;
    view.Target = new Point3d(center.X, center.Y, 0);
    view.CenterPoint = Point2d.Origin;
    view.ViewTwist = twistRadians;
    editor.SetCurrentView(view);

    var snapAngle = NormalizeAngle(2 * Math.PI - twistRadians);
    Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("SNAPANG", snapAngle);

    return new Dictionary<string, object?>
    {
      ["layout"] = "Model",
      ["beforeTwistDegrees"] = beforeTwist * 180d / Math.PI,
      ["twistDegrees"] = twistRadians * 180d / Math.PI,
      ["snapAngleDegrees"] = snapAngle * 180d / Math.PI,
      ["modelCenterX"] = center.X,
      ["modelCenterY"] = center.Y,
      ["viewHeight"] = view.Height,
    };
  }

  private static double NormalizeAngle(double radians)
  {
    var full = 2 * Math.PI;
    var value = radians % full;
    return value < 0 ? value + full : value;
  }

  private static string? ViewportAnnotationScaleName(Viewport viewport)
  {
    try
    {
      return viewport.AnnotationScale?.Name;
    }
    catch (Autodesk.AutoCAD.Runtime.Exception)
    {
      return null;
    }
  }

  private static Dictionary<string, object?> BuildViewportEntry(Viewport viewport, string layoutName)
  {
    var paperSpace = IsPaperSpaceViewport(viewport);
    var scale = viewport.ViewHeight > 0 ? viewport.Height / viewport.ViewHeight : 0d;
    var modelCenter = paperSpace ? (Point2d?)null : ModelCenter(viewport);
    return new Dictionary<string, object?>
    {
      ["handle"] = viewport.Handle.ToString(),
      ["layout"] = layoutName,
      ["isPaperSpaceViewport"] = paperSpace,
      ["layer"] = viewport.Layer,
      ["centerX"] = viewport.CenterPoint.X,
      ["centerY"] = viewport.CenterPoint.Y,
      ["width"] = viewport.Width,
      ["height"] = viewport.Height,
      ["viewHeight"] = viewport.ViewHeight,
      ["customScale"] = scale,
      ["scaleLabel"] = !paperSpace && scale > 0 ? $"1\"={Math.Round(1 / scale, 2)}'" : null,
      // null = the viewport has no annotation scale of its own: annotative dims/MLeaders will not show in it (QC check).
      ["annotationScale"] = paperSpace ? null : ViewportAnnotationScaleName(viewport),
      ["twistRadians"] = viewport.TwistAngle,
      ["twistDegrees"] = Math.Round(viewport.TwistAngle * 180d / Math.PI, 4),
      ["modelCenterX"] = modelCenter?.X,
      ["modelCenterY"] = modelCenter?.Y,
      ["locked"] = viewport.Locked,
    };
  }

  private static Dictionary<string, object?> BuildDimensionEntry(Dimension dimension, string layoutName, bool isModelSpace)
  {
    double[]? xLine1 = null, xLine2 = null, dimLine = null;
    double? rotation = null;
    switch (dimension)
    {
      case AlignedDimension aligned:
        xLine1 = new[] { aligned.XLine1Point.X, aligned.XLine1Point.Y };
        xLine2 = new[] { aligned.XLine2Point.X, aligned.XLine2Point.Y };
        dimLine = new[] { aligned.DimLinePoint.X, aligned.DimLinePoint.Y };
        break;
      case RotatedDimension rotated:
        xLine1 = new[] { rotated.XLine1Point.X, rotated.XLine1Point.Y };
        xLine2 = new[] { rotated.XLine2Point.X, rotated.XLine2Point.Y };
        dimLine = new[] { rotated.DimLinePoint.X, rotated.DimLinePoint.Y };
        rotation = rotated.Rotation;
        break;
    }

    double? measurement = null;
    try { measurement = dimension.Measurement; } catch { measurement = null; }

    return new Dictionary<string, object?>
    {
      ["handle"] = dimension.Handle.ToString(),
      ["dimensionType"] = dimension.GetType().Name,
      ["layer"] = dimension.Layer,
      ["dimStyle"] = dimension.DimensionStyleName,
      ["measurement"] = measurement,
      ["textOverride"] = dimension.DimensionText,
      ["textX"] = dimension.TextPosition.X,
      ["textY"] = dimension.TextPosition.Y,
      ["xLine1Point"] = xLine1,
      ["xLine2Point"] = xLine2,
      ["dimLinePoint"] = dimLine,
      ["rotation"] = rotation,
      ["space"] = isModelSpace ? "model" : "paper",
      ["layout"] = layoutName,
    };
  }
}
