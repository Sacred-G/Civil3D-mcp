using System.Reflection;
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
        TrySetProperty(parcel, "Description", description);
        TrySetProperty(parcel, "TaxId", description);
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
        WriteCsv(outputPath, rows, includeCoordinates);
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
    var stylesRoot = CivilObjectUtils.GetPropertyValue<object>(civilDoc, "Styles");
    var parcelStyles = CivilObjectUtils.GetPropertyValue<object>(stylesRoot, "ParcelStyles");
    if (parcelStyles == null) return;
    foreach (ObjectId sid in CivilObjectUtils.ToObjectIds(parcelStyles))
    {
      var style = transaction.GetObject(sid, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(style), styleName, StringComparison.OrdinalIgnoreCase))
      {
        TrySetProperty(parcel, "StyleId", sid);
        break;
      }
    }
  }

  private static void ApplyLabelStyleToParcel(Parcel parcel, string labelStyleName,
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction)
  {
    // Try via AreaLabelStyleId or similar property
    TrySetProperty(parcel, "AreaLabelStyleName", labelStyleName);
  }

  private static IEnumerable<ObjectId> EnumerateSiteIds(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc)
  {
    foreach (var memberName in new[] { "Sites", "SiteCollection" })
    {
      var collection = GetNamedMember(civilDoc, memberName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId != ObjectId.Null)
          yield return objectId;
      }
    }
  }

  private static IEnumerable<ObjectId> EnumerateParcelIds(Site site, Transaction transaction)
  {
    foreach (var memberName in new[] { "Parcels", "ParcelCollection", "ParcelIds" })
    {
      var value = GetNamedMember(site, memberName) ?? CivilObjectUtils.InvokeMethod(site, memberName);

      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
      {
        if (objectId != ObjectId.Null)
          yield return objectId;
      }

      if (value is System.Collections.IEnumerable enumerable)
      {
        foreach (var item in enumerable)
        {
          if (item is Parcel parcel)
          {
            yield return parcel.ObjectId;
          }
          else if (item is Autodesk.AutoCAD.DatabaseServices.DBObject dbObject)
          {
            yield return dbObject.ObjectId;
          }
        }
      }
    }
  }

  private static object? GetNamedMember(object? value, string memberName)
  {
    if (value == null) return null;

    var property = value.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
    if (property != null) return property.GetValue(value);

    var field = value.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
    return field?.GetValue(value);
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

  private static void WriteCsv(string outputPath, List<Dictionary<string, object?>> rows, bool includeCoordinates)
  {
    using var sw = new System.IO.StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
    sw.WriteLine("Name,Area,AreaUnits,Perimeter,Style");
    foreach (var row in rows)
    {
      sw.WriteLine(string.Join(",",
        row.GetValueOrDefault("name"), row.GetValueOrDefault("area"),
        row.GetValueOrDefault("areaUnits"), row.GetValueOrDefault("perimeter"),
        row.GetValueOrDefault("style")));
    }
  }

  private static void TrySetProperty(object obj, string propertyName, object? value)
  {
    try
    {
      var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      if (prop != null && prop.CanWrite)
        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
    }
    catch { }
  }

  private static Type? FindType(string typeName)
  {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
      var t = asm.GetType(typeName);
      if (t != null) return t;
    }
    return null;
  }
}
