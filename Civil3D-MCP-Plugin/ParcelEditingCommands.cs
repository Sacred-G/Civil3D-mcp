using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_parcel_* editing tools.
///
/// Civil 3D API notes:
///   Parcel/site collection members vary by Civil 3D version, so this implementation
///   relies on reflection for site/parcel enumeration and creation fallbacks.
/// </summary>
public static class ParcelEditingCommands
{
  // -------------------------------------------------------------------------
  // listParcelSites
  // -------------------------------------------------------------------------

  public static Task<object?> ListParcelSitesAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var rows = new List<Dictionary<string, object?>>();
      foreach (ObjectId sid in EnumerateSiteIds(civilDoc))
      {
        var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, sid, OpenMode.ForRead);
        rows.Add(new Dictionary<string, object?>
        {
          ["name"] = site.Name,
          ["handle"] = CivilObjectUtils.GetHandle(site),
          ["parcelCount"] = EnumerateParcelIds(site, transaction).Count(),
        });
      }

      return new Dictionary<string, object?>
      {
        ["sites"] = rows,
      };
    });
  }

  // -------------------------------------------------------------------------
  // listParcels
  // -------------------------------------------------------------------------

  public static Task<object?> ListParcelsAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var parcels = new List<Dictionary<string, object?>>();

      foreach (ObjectId pid in EnumerateParcelIds(site, transaction))
      {
        var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, pid, OpenMode.ForRead);
        parcels.Add(new Dictionary<string, object?>
        {
          ["name"] = parcel.Name,
          ["handle"] = CivilObjectUtils.GetHandle(parcel),
          ["number"] = GetParcelNumber(parcel),
          ["area"] = parcel.Area,
          ["perimeter"] = GetParcelPerimeter(parcel),
          ["style"] = CivilObjectUtils.GetStringProperty(parcel.StyleId.IsNull ? null : transaction.GetObject(parcel.StyleId, OpenMode.ForRead), "Name"),
        });
      }

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["parcels"] = parcels,
        ["units"] = new Dictionary<string, object?>
        {
          ["area"] = $"{CivilObjectUtils.LinearUnits(database)}²",
          ["length"] = CivilObjectUtils.LinearUnits(database),
        },
      };
    });
  }

  // -------------------------------------------------------------------------
  // getParcel
  // -------------------------------------------------------------------------

  public static Task<object?> GetParcelAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelName = PluginRuntime.GetRequiredString(parameters, "parcelName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var parcel = FindParcelByName(site, transaction, parcelName, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["name"] = parcel.Name,
        ["handle"] = CivilObjectUtils.GetHandle(parcel),
        ["number"] = GetParcelNumber(parcel),
        ["area"] = parcel.Area,
        ["perimeter"] = GetParcelPerimeter(parcel),
        ["style"] = CivilObjectUtils.GetStringProperty(parcel.StyleId.IsNull ? null : transaction.GetObject(parcel.StyleId, OpenMode.ForRead), "Name"),
      };
    });
  }

  // -------------------------------------------------------------------------
  // createParcel
  // -------------------------------------------------------------------------

  public static Task<object?> CreateParcelAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelName = PluginRuntime.GetOptionalString(parameters, "name");
    var sourceHandle = PluginRuntime.GetOptionalString(parameters, "sourceHandle");
    var style = PluginRuntime.GetOptionalString(parameters, "style");
    var areaLabelStyle = PluginRuntime.GetOptionalString(parameters, "areaLabelStyle");
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);

      ObjectId newParcelId = ObjectId.Null;

      if (!string.IsNullOrWhiteSpace(sourceHandle))
      {
        // Create from existing entity handle
        var handle = new Handle(Convert.ToInt64(sourceHandle, 16));
        if (!database.TryGetObjectId(handle, out var entityId) || entityId.IsNull)
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
            $"Entity with handle '{sourceHandle}' was not found.");

        newParcelId = CreateParcelFromEntity(site, entityId, parcelName, transaction);
      }
      else if (pointsNode != null && pointsNode.Count >= 3)
      {
        // Create from vertex list
        newParcelId = CreateParcelFromPoints(site, pointsNode, parcelName, database, transaction);
      }
      else
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
          "createParcel requires either a sourceHandle (entity handle) or a points array with ≥3 vertices.");
      }

      if (newParcelId.IsNull)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Parcel was not created. Check that the boundary is closed and forms a valid area.");

      var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, newParcelId, OpenMode.ForWrite);

      // Apply optional properties
      if (!string.IsNullOrWhiteSpace(parcelName)) CivilObjectUtils.TrySetName(parcel, parcelName);
      if (!string.IsNullOrWhiteSpace(style)) ApplyStyleToParcel(parcel, style, civilDoc, transaction);
      if (!string.IsNullOrWhiteSpace(areaLabelStyle)) ApplyLabelStyleToParcel(parcel, areaLabelStyle, civilDoc, transaction);

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["name"] = parcel.Name,
        ["handle"] = CivilObjectUtils.GetHandle(parcel),
        ["area"] = parcel.Area,
        ["perimeter"] = GetParcelPerimeter(parcel),
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // editParcel
  // -------------------------------------------------------------------------

  public static Task<object?> EditParcelAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelName = PluginRuntime.GetRequiredString(parameters, "parcelName");
    var newName = PluginRuntime.GetOptionalString(parameters, "newName");
    var style = PluginRuntime.GetOptionalString(parameters, "style");
    var areaLabelStyle = PluginRuntime.GetOptionalString(parameters, "areaLabelStyle");
    var description = PluginRuntime.GetOptionalString(parameters, "description");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var parcel = FindParcelByName(site, transaction, parcelName, OpenMode.ForWrite);

      if (!string.IsNullOrWhiteSpace(newName)) CivilObjectUtils.TrySetName(parcel, newName);
      if (!string.IsNullOrWhiteSpace(style)) ApplyStyleToParcel(parcel, style, civilDoc, transaction);
      if (!string.IsNullOrWhiteSpace(areaLabelStyle)) ApplyLabelStyleToParcel(parcel, areaLabelStyle, civilDoc, transaction);
      if (!string.IsNullOrWhiteSpace(description))
      {
        parcel.Description = description;
      }

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["name"] = parcel.Name,
        ["handle"] = CivilObjectUtils.GetHandle(parcel),
        ["area"] = parcel.Area,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // adjustParcelLotLine
  // -------------------------------------------------------------------------

  public static Task<object?> AdjustParcelLotLineAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelName = PluginRuntime.GetRequiredString(parameters, "parcelName");
    var targetAreaSqFt = PluginRuntime.GetRequiredDouble(parameters, "targetAreaSqFt");
    var lotLineHandle = PluginRuntime.GetOptionalString(parameters, "lotLineHandle");
    var tolerance = PluginRuntime.GetOptionalDouble(parameters, "tolerance") ?? 1.0;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var parcel = FindParcelByName(site, transaction, parcelName, OpenMode.ForRead);

      // Try Civil 3D lot line slide API
      var result = CivilObjectUtils.InvokeMethod(parcel, "AdjustLotLine",
        targetAreaSqFt, tolerance, lotLineHandle);

      if (result == null)
      {
        result = CivilObjectUtils.InvokeMethod(parcel, "SlideAngle",
          targetAreaSqFt, tolerance);
      }

      var actualArea = parcel.Area;
      var converged = Math.Abs(actualArea - TargetAreaToDrawingUnits(targetAreaSqFt, doc)) < tolerance;

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["parcelName"] = parcelName,
        ["targetAreaSqFt"] = targetAreaSqFt,
        ["actualArea"] = actualArea,
        ["converged"] = converged,
        ["message"] = converged
          ? $"Lot line adjusted. Parcel area is now {actualArea:F2} drawing units²."
          : "Lot line adjustment attempted. Verify the result in Civil 3D.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // reportParcels
  // -------------------------------------------------------------------------

  public static Task<object?> ReportParcelsAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelNamesNode = PluginRuntime.GetParameter(parameters, "parcelNames") as JsonArray;
    var outputPath = PluginRuntime.GetOptionalString(parameters, "outputPath");
    var includeCoordinates = PluginRuntime.GetOptionalBool(parameters, "includeCoordinates") ?? false;
    var units = PluginRuntime.GetOptionalString(parameters, "units") ?? "sqft";
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;

    var filterNames = parcelNamesNode?
      .Select(n => n?.GetValue<string>())
      .Where(n => !string.IsNullOrWhiteSpace(n))
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var rows = new List<Dictionary<string, object?>>();

      foreach (ObjectId pid in EnumerateParcelIds(site, transaction))
      {
        var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, pid, OpenMode.ForRead);
        if (filterNames != null && filterNames.Count > 0 && !filterNames.Contains(parcel.Name)) continue;

        var areaInUnits = ConvertArea(parcel.Area, units);

        var row = new Dictionary<string, object?>
        {
          ["name"] = parcel.Name,
          ["handle"] = CivilObjectUtils.GetHandle(parcel),
          ["area"] = areaInUnits,
          ["areaUnits"] = units,
          ["perimeter"] = GetParcelPerimeter(parcel),
          ["style"] = CivilObjectUtils.GetStringProperty(
            parcel.StyleId.IsNull ? null : transaction.GetObject(parcel.StyleId, OpenMode.ForRead), "Name"),
        };

        if (includeCoordinates)
        {
          var vertices = new List<Dictionary<string, object?>>();
          var boundary = CivilObjectUtils.InvokeMethod(parcel, "GetBoundary")
            ?? CivilObjectUtils.InvokeMethod(parcel, "GetVertices");
          if (boundary is System.Collections.IEnumerable pts)
          {
            foreach (var pt in pts)
            {
              double x = CivilObjectUtils.GetDoubleProperty(pt, "X") ?? 0;
              double y = CivilObjectUtils.GetDoubleProperty(pt, "Y") ?? 0;
              vertices.Add(new Dictionary<string, object?> { ["x"] = x, ["y"] = y });
            }
          }
          row["vertices"] = vertices;
        }

        rows.Add(row);
      }

      if (!string.IsNullOrWhiteSpace(outputPath))
      {
        outputPath = WriteCsv(outputPath, rows, includeCoordinates, overwrite);
      }

      return new Dictionary<string, object?>
      {
        ["siteName"] = siteName,
        ["parcelCount"] = rows.Count,
        ["parcels"] = rows,
        ["outputPath"] = outputPath,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getParcelGeometry
  // -------------------------------------------------------------------------

  private const double DefaultMaxArcSegmentAngleDegrees = 5.0;

  /// <summary>
  /// Read-only: the parcel's real boundary through the typed curve API (no
  /// reflection). Returns the true vertices with bulges, the line/arc
  /// segments, and a densified polygon (arcs split so no step sweeps more
  /// than maxArcSegmentAngle degrees) that callers can use directly.
  /// </summary>
  public static Task<object?> GetParcelGeometryAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetRequiredString(parameters, "siteName");
    var parcelName = PluginRuntime.GetRequiredString(parameters, "parcelName");
    var maxArcSegmentAngle = PluginRuntime.GetOptionalDouble(parameters, "maxArcSegmentAngle") ?? DefaultMaxArcSegmentAngleDegrees;
    if (!double.IsFinite(maxArcSegmentAngle) || maxArcSegmentAngle < 0.1 || maxArcSegmentAngle > 90)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "maxArcSegmentAngle must be between 0.1 and 90 degrees.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var site = FindSiteByName(civilDoc, transaction, siteName);
      var parcel = FindParcelByName(site, transaction, parcelName, OpenMode.ForRead);
      var notes = new List<string>();
      var (segments, source) = ReadParcelBoundary(parcel, notes);
      if (segments == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Could not read the boundary of parcel '{parcel.Name}' in site '{site.Name}': {string.Join(" ", notes)}");
      }

      var signedArea = BridgeMath.SignedArea(segments);
      var computedArea = Math.Abs(signedArea);
      var computedPerimeter = BridgeMath.Perimeter(segments);
      var reportedArea = parcel.Area;
      if (reportedArea > 0 && Math.Abs(computedArea - reportedArea) > reportedArea * 0.001)
      {
        notes.Add($"The area computed from the boundary ({computedArea:G10}) differs from the parcel's reported area ({reportedArea:G10}) by more than 0.1%.");
      }

      var densified = BridgeMath.Densify(segments, maxArcSegmentAngle * Math.PI / 180.0);
      Point3d? centroid = null;
      try { centroid = parcel.Centroid; } catch { /* optional */ }

      return new Dictionary<string, object?>
      {
        ["siteName"] = site.Name,
        ["name"] = parcel.Name,
        ["handle"] = CivilObjectUtils.GetHandle(parcel),
        ["number"] = GetParcelNumber(parcel),
        ["vertices"] = densified.Select(p => new Dictionary<string, object?> { ["x"] = p.X, ["y"] = p.Y }).ToList(),
        ["closed"] = true,
        ["orientation"] = signedArea >= 0 ? "ccw" : "cw",
        ["boundaryVertices"] = segments.Select(s => new Dictionary<string, object?> { ["x"] = s.X0, ["y"] = s.Y0, ["bulge"] = s.Bulge }).ToList(),
        ["segments"] = segments.Select(ToSegmentData).ToList(),
        ["hasArcs"] = segments.Any(s => s.IsArc),
        ["maxArcSegmentAngle"] = maxArcSegmentAngle,
        ["area"] = reportedArea,
        ["perimeter"] = computedPerimeter,
        ["computedArea"] = computedArea,
        ["reportedPerimeter"] = GetParcelPerimeter(parcel),
        ["centroid"] = centroid is Point3d c ? new Dictionary<string, object?> { ["x"] = c.X, ["y"] = c.Y } : null,
        ["geometrySource"] = source,
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["lengthUnit"] = DrawingCommands.ResolveLengthUnit(civilDoc, database),
        ["notes"] = notes,
      };
    });
  }

  private static Dictionary<string, object?> ToSegmentData(BridgeMath.Segment segment)
  {
    var data = new Dictionary<string, object?>
    {
      ["type"] = segment.IsArc ? "arc" : "line",
      ["start"] = new Dictionary<string, object?> { ["x"] = segment.X0, ["y"] = segment.Y0 },
      ["end"] = new Dictionary<string, object?> { ["x"] = segment.X1, ["y"] = segment.Y1 },
      ["bulge"] = segment.Bulge,
      ["length"] = BridgeMath.SegmentLength(segment),
    };
    if (segment.IsArc)
    {
      var arc = BridgeMath.GetArc(segment);
      data["center"] = new Dictionary<string, object?> { ["x"] = arc.CenterX, ["y"] = arc.CenterY };
      data["radius"] = arc.Radius;
      data["sweepAngleDeg"] = arc.SweepRadians * 180.0 / Math.PI;
    }

    return data;
  }

  /// <summary>
  /// Tries, in order: the parcel's base curve, the parcel as an AutoCAD curve
  /// (GetGeCurve), and exploding the parcel into lines/arcs. The first source
  /// that yields one closed chain wins; failures are recorded in notes.
  /// </summary>
  private static (List<BridgeMath.Segment>? segments, string source) ReadParcelBoundary(Parcel parcel, List<string> notes)
  {
    try
    {
      var baseCurve = parcel.BaseCurve;
      if (baseCurve != null)
      {
        try
        {
          var chained = ChainOrNote(SegmentsFromCurve(baseCurve), "baseCurve", notes);
          if (chained != null) return (chained, $"baseCurve:{baseCurve.GetType().Name}");
        }
        finally
        {
          // BaseCurve hands back a non-database-resident copy; free it.
          if (baseCurve.ObjectId.IsNull && !baseCurve.IsDisposed) baseCurve.Dispose();
        }
      }
      else
      {
        notes.Add("baseCurve: none.");
      }
    }
    catch (Exception exception)
    {
      notes.Add($"baseCurve: {exception.Message}");
    }

    try
    {
      if ((object)parcel is Curve parcelCurve)
      {
        using var geCurve = parcelCurve.GetGeCurve();
        var chained = ChainOrNote(SegmentsFromGeCurve(geCurve), "geCurve", notes);
        if (chained != null) return (chained, "geCurve");
      }
    }
    catch (Exception exception)
    {
      notes.Add($"geCurve: {exception.Message}");
    }

    var exploded = new DBObjectCollection();
    try
    {
      parcel.Explode(exploded);
      var segments = new List<BridgeMath.Segment>();
      foreach (Autodesk.AutoCAD.DatabaseServices.DBObject item in exploded)
      {
        if (item is Curve curve)
        {
          segments.AddRange(SegmentsFromCurve(curve));
        }
      }

      var chained = ChainOrNote(segments, "explode", notes);
      if (chained != null) return (chained, "explode");
    }
    catch (Exception exception)
    {
      notes.Add($"explode: {exception.Message}");
    }
    finally
    {
      foreach (Autodesk.AutoCAD.DatabaseServices.DBObject item in exploded)
      {
        if (!item.IsDisposed) item.Dispose();
      }
    }

    return (null, "none");
  }

  private static List<BridgeMath.Segment>? ChainOrNote(List<BridgeMath.Segment> segments, string source, List<string> notes)
  {
    var chained = BridgeMath.ChainClosed(segments, BridgeMath.ChainTolerance(segments));
    if (chained == null)
    {
      notes.Add($"{source}: {segments.Count} segment(s) did not form one closed boundary.");
    }

    return chained;
  }

  private static List<BridgeMath.Segment> SegmentsFromCurve(Curve curve)
  {
    if (curve is Polyline polyline)
    {
      var segments = new List<BridgeMath.Segment>();
      var count = polyline.NumberOfVertices;
      var last = polyline.Closed ? count : count - 1;
      for (var i = 0; i < last; i++)
      {
        var p0 = polyline.GetPoint2dAt(i);
        var p1 = polyline.GetPoint2dAt((i + 1) % count);
        segments.Add(new BridgeMath.Segment(p0.X, p0.Y, p1.X, p1.Y, polyline.GetBulgeAt(i)));
      }

      return segments;
    }

    if (curve is Line line)
    {
      return [new BridgeMath.Segment(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y, 0)];
    }

    if (curve is Arc arc)
    {
      var sweep = arc.EndAngle - arc.StartAngle;
      while (sweep <= 0) sweep += 2 * Math.PI;
      var sign = arc.Normal.Z >= 0 ? 1.0 : -1.0;
      return [new BridgeMath.Segment(arc.StartPoint.X, arc.StartPoint.Y, arc.EndPoint.X, arc.EndPoint.Y, BridgeMath.BulgeFromSweep(sign * sweep))];
    }

    using var geCurve = curve.GetGeCurve();
    return SegmentsFromGeCurve(geCurve);
  }

  private static List<BridgeMath.Segment> SegmentsFromGeCurve(Curve3d geCurve)
  {
    var segments = new List<BridgeMath.Segment>();
    switch (geCurve)
    {
      case CompositeCurve3d composite:
        foreach (var child in composite.GetCurves())
        {
          segments.AddRange(SegmentsFromGeCurve(child));
        }

        break;
      case LineSegment3d lineSegment:
        segments.Add(new BridgeMath.Segment(lineSegment.StartPoint.X, lineSegment.StartPoint.Y, lineSegment.EndPoint.X, lineSegment.EndPoint.Y, 0));
        break;
      case CircularArc3d circularArc:
      {
        var sweep = circularArc.EndAngle - circularArc.StartAngle;
        var sign = circularArc.Normal.Z >= 0 ? 1.0 : -1.0;
        var start = circularArc.StartPoint;
        if (sweep >= 2 * Math.PI - 1e-9)
        {
          // Full circle: split into two half circles so both chords are non-zero.
          var center = circularArc.Center;
          var opposite = new Point2d(2 * center.X - start.X, 2 * center.Y - start.Y);
          segments.Add(new BridgeMath.Segment(start.X, start.Y, opposite.X, opposite.Y, sign));
          segments.Add(new BridgeMath.Segment(opposite.X, opposite.Y, start.X, start.Y, sign));
        }
        else
        {
          var end = circularArc.EndPoint;
          segments.Add(new BridgeMath.Segment(start.X, start.Y, end.X, end.Y, BridgeMath.BulgeFromSweep(sign * sweep)));
        }

        break;
      }
      default:
      {
        // Unexpected curve type: approximate with a fine polyline.
        var samples = geCurve.GetSamplePoints(64).Select(sample => sample.Point).ToArray();
        for (var i = 1; i < samples.Length; i++)
        {
          segments.Add(new BridgeMath.Segment(samples[i - 1].X, samples[i - 1].Y, samples[i].X, samples[i].Y, 0));
        }

        break;
      }
    }

    return segments;
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static Site FindSiteByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction, string siteName)
  {
    foreach (ObjectId sid in EnumerateSiteIds(civilDoc))
    {
      var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, sid, OpenMode.ForRead);
      if (string.Equals(site.Name, siteName, StringComparison.OrdinalIgnoreCase)) return site;
    }
    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Site '{siteName}' was not found.");
  }

  private static Parcel FindParcelByName(Site site, Transaction transaction, string parcelName, OpenMode mode)
  {
    foreach (ObjectId pid in EnumerateParcelIds(site, transaction))
    {
      var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, pid, mode);
      if (string.Equals(parcel.Name, parcelName, StringComparison.OrdinalIgnoreCase)) return parcel;
    }
    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Parcel '{parcelName}' was not found in site '{site.Name}'.");
  }

  private static ObjectId CreateParcelFromEntity(Site site, ObjectId entityId, string? name, Transaction transaction)
  {
    // Try Parcel.Create(siteId, entityId) or site.Parcels.Add(entityId)
    var parcelsCollection = GetNamedMember(site, "Parcels") ?? GetNamedMember(site, "ParcelCollection");
    var result = CivilObjectUtils.InvokeMethod(site, "CreateParcelFromEntity", entityId, name ?? "")
      ?? CivilObjectUtils.InvokeMethod(parcelsCollection, "Add", entityId);

    if (result is ObjectId id && !id.IsNull) return id;

    // Try static Parcel.Create
    var parcelType = FindType("Autodesk.Civil.DatabaseServices.Parcel");
    if (parcelType != null)
    {
      var createResult = CivilObjectUtils.InvokeStaticMethod(parcelType, "Create",
        site.ObjectId, entityId, name ?? "");
      if (createResult is ObjectId oid && !oid.IsNull) return oid;
    }

    return ObjectId.Null;
  }

  private static ObjectId CreateParcelFromPoints(Site site, JsonArray pointsNode, string? name,
    Database database, Transaction transaction)
  {
    var pts = new List<Point2d>();
    foreach (var node in pointsNode)
    {
      if (node is JsonArray arr && arr.Count >= 2)
        pts.Add(new Point2d(arr[0]?.GetValue<double>() ?? 0, arr[1]?.GetValue<double>() ?? 0));
    }
    // Close the polygon
    if (pts.Count > 0 && pts[0] != pts[^1])
      pts.Add(pts[0]);

    // Create a temporary polyline and convert to parcel
    var pline = new Polyline();
    for (var index = 0; index < pts.Count; index++)
    {
      pline.AddVertexAt(index, pts[index], 0, 0, 0);
    }

    pline.Closed = true;

    var modelSpaceId = CivilObjectUtils.GetModelSpaceBlockId(database, transaction);
    var btr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, modelSpaceId, OpenMode.ForWrite);
    var tmpId = btr.AppendEntity(pline);
    transaction.AddNewlyCreatedDBObject(pline, true);

    var parcelId = CreateParcelFromEntity(site, tmpId, name, transaction);

    // Erase the temporary polyline if parcel was created from it
    if (!parcelId.IsNull)
      pline.Erase(true);

    return parcelId;
  }

  private static void ApplyStyleToParcel(Parcel parcel, string styleName,
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction)
  {
    parcel.StyleId = LookupUtils.GetParcelStyleId(civilDoc, transaction, styleName);
  }

  private static void ApplyLabelStyleToParcel(Parcel parcel, string labelStyleName,
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction)
  {
    parcel.AreaSelectionLabelStyleId = LookupUtils.GetParcelAreaLabelStyleId(civilDoc, transaction, labelStyleName);
  }

  private static IEnumerable<ObjectId> EnumerateSiteIds(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc)
  {
    foreach (ObjectId objectId in civilDoc.GetSiteIds())
    {
      if (objectId != ObjectId.Null)
        yield return objectId;
    }
  }

  private static IEnumerable<ObjectId> EnumerateParcelIds(Site site, Transaction transaction)
  {
    var documentedIds = new HashSet<ObjectId>();
    foreach (ObjectId objectId in site.GetParcelIds())
    {
      if (objectId != ObjectId.Null && documentedIds.Add(objectId))
        yield return objectId;
    }

    // Compatibility fallbacks are retained for older host variations, but
    // de-duplicate anything already returned by the documented 2026 API.
    foreach (var memberName in new[] { "Parcels", "ParcelCollection", "ParcelIds" })
    {
      var value = GetNamedMember(site, memberName) ?? CivilObjectUtils.InvokeMethod(site, memberName);

      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
      {
        if (objectId != ObjectId.Null && documentedIds.Add(objectId))
          yield return objectId;
      }

      if (value is System.Collections.IEnumerable enumerable)
      {
        foreach (var item in enumerable)
        {
          if (item is Parcel parcel)
          {
            if (documentedIds.Add(parcel.ObjectId)) yield return parcel.ObjectId;
          }
          else if (item is Autodesk.AutoCAD.DatabaseServices.DBObject dbObject)
          {
            if (documentedIds.Add(dbObject.ObjectId)) yield return dbObject.ObjectId;
          }
        }
      }
    }
  }

  private static object? GetNamedMember(object? value, string memberName)
  {
    if (value == null) return null;

    return Civil3DCompatibility.GetPropertyValue(value, memberName)
      ?? Civil3DCompatibility.GetFieldValue(value, memberName);
  }

  private static double? GetParcelPerimeter(Parcel parcel)
  {
    var perimeter = CivilObjectUtils.GetDoubleProperty(parcel, "Perimeter");
    if (perimeter.HasValue)
      return perimeter.Value;

    var boundary = CivilObjectUtils.InvokeMethod(parcel, "GetBoundary")
      ?? CivilObjectUtils.InvokeMethod(parcel, "GetVertices");
    if (boundary is not System.Collections.IEnumerable vertices)
      return null;

    var points = new List<Point2d>();
    foreach (var vertex in vertices)
    {
      var x = CivilObjectUtils.GetDoubleProperty(vertex, "X");
      var y = CivilObjectUtils.GetDoubleProperty(vertex, "Y");
      if (x.HasValue && y.HasValue)
        points.Add(new Point2d(x.Value, y.Value));
    }

    if (points.Count < 2)
      return null;

    double length = 0;
    for (var index = 1; index < points.Count; index++)
    {
      length += points[index - 1].GetDistanceTo(points[index]);
    }

    if (points[0] != points[^1])
      length += points[^1].GetDistanceTo(points[0]);

    return length;
  }

  private static int GetParcelNumber(Parcel parcel)
  {
    return parcel.Number;
  }

  private static double ConvertArea(double areaInDrawingUnits, string units)
  {
    return units.ToLowerInvariant() switch
    {
      "acres" => areaInDrawingUnits / 43560.0,
      "sqm" => areaInDrawingUnits * 0.092903,
      "ha" => areaInDrawingUnits * 0.092903 / 10000.0,
      _ => areaInDrawingUnits, // sqft or default
    };
  }

  private static double TargetAreaToDrawingUnits(double sqFt, Autodesk.AutoCAD.ApplicationServices.Document doc)
  {
    // Drawing units are usually sq ft already for US drawings
    return sqFt;
  }

  private static string WriteCsv(
    string outputPath,
    List<Dictionary<string, object?>> rows,
    bool includeCoordinates,
    bool overwrite)
  {
    var csv = new System.Text.StringBuilder();
    csv.AppendLine("Name,Area,AreaUnits,Perimeter,Style");
    foreach (var row in rows)
    {
      csv.AppendLine(string.Join(",",
        row.GetValueOrDefault("name"), row.GetValueOrDefault("area"),
        row.GetValueOrDefault("areaUnits"), row.GetValueOrDefault("perimeter"),
        row.GetValueOrDefault("style")));
    }

    return FileBoundary.WriteAllTextAtomic(
      outputPath, csv.ToString(), System.Text.Encoding.UTF8, overwrite, ".csv");
  }

  private static Type? FindType(string typeName)
  {
    return Civil3DCompatibility.FindLoadedType(typeName);
  }
}
