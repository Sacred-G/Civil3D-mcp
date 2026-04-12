using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_intersection_* tools.
///
/// Civil 3D API notes:
///   Intersections are accessed via civilDoc.IntersectionCollection.
///   Each Intersection object has MainRoadAlignmentId, IntersectingRoadAlignmentId,
///   CurbReturnParameters, and optionally associated corridors.
///   Intersection.Create() is the factory method for creating new intersections.
/// </summary>
public static class IntersectionCommands
{
  // -------------------------------------------------------------------------
  // listIntersections
  // -------------------------------------------------------------------------

  public static Task<object?> ListIntersectionsAsync(JsonObject? parameters)
  {
    var siteNameFilter = PluginRuntime.GetOptionalString(parameters, "siteName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var intersections = new List<Dictionary<string, object?>>();

      var collection = CivilObjectUtils.GetPropertyValue<object>(civilDoc, "IntersectionCollection");
      if (collection == null)
      {
        return new Dictionary<string, object?> { ["intersections"] = intersections };
      }

      foreach (ObjectId id in CivilObjectUtils.ToObjectIds(collection))
      {
        var inter = transaction.GetObject(id, OpenMode.ForRead);
        if (inter == null) continue;

        var siteName = CivilObjectUtils.GetStringProperty(inter, "SiteName")
          ?? CivilObjectUtils.GetStringProperty(inter, "Site");

        if (!string.IsNullOrWhiteSpace(siteNameFilter) &&
            !string.Equals(siteName, siteNameFilter, StringComparison.OrdinalIgnoreCase))
          continue;

        var ptProp = inter.GetType().GetProperty("Location") ?? inter.GetType().GetProperty("IntersectionPoint");
        Point2d? pt = null;
        if (ptProp != null)
        {
          var rawPt = ptProp.GetValue(inter);
          if (rawPt is Point2d p2) pt = p2;
          else if (rawPt is Point3d p3) pt = new Point2d(p3.X, p3.Y);
        }

        var mainAlignId = GetObjectIdProperty(inter, "MainRoadAlignmentId");
        var secAlignId = GetObjectIdProperty(inter, "IntersectingRoadAlignmentId");

        intersections.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(inter),
          ["handle"] = inter is AcDbObject dbo ? CivilObjectUtils.GetHandle(dbo) : null,
          ["siteName"] = siteName,
          ["intersectionX"] = pt?.X,
          ["intersectionY"] = pt?.Y,
          ["mainRoadAlignment"] = mainAlignId.HasValue
            ? CivilObjectUtils.GetName(transaction.GetObject(mainAlignId.Value, OpenMode.ForRead)) : null,
          ["intersectingRoadAlignment"] = secAlignId.HasValue
            ? CivilObjectUtils.GetName(transaction.GetObject(secAlignId.Value, OpenMode.ForRead)) : null,
        });
      }

      return new Dictionary<string, object?> { ["intersections"] = intersections };
    });
  }

  // -------------------------------------------------------------------------
  // createIntersection
  // -------------------------------------------------------------------------

  public static Task<object?> CreateIntersectionAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var mainRoadAlignment = PluginRuntime.GetRequiredString(parameters, "mainRoadAlignment");
    var mainRoadProfile = PluginRuntime.GetRequiredString(parameters, "mainRoadProfile");
    var intersectingRoadAlignment = PluginRuntime.GetRequiredString(parameters, "intersectingRoadAlignment");
    var intersectingRoadProfile = PluginRuntime.GetRequiredString(parameters, "intersectingRoadProfile");
    var intersectionX = PluginRuntime.GetOptionalDouble(parameters, "intersectionX");
    var intersectionY = PluginRuntime.GetOptionalDouble(parameters, "intersectionY");
    var siteName = PluginRuntime.GetOptionalString(parameters, "siteName");
    var offsetDistance = PluginRuntime.GetOptionalDouble(parameters, "offsetDistance");
    var curveRadius = PluginRuntime.GetOptionalDouble(parameters, "curveRadius");
    var createCorridors = PluginRuntime.GetOptionalBool(parameters, "createCorridors") ?? false;
    var mainRoadAssembly = PluginRuntime.GetOptionalString(parameters, "mainRoadAssembly");
    var intersectingRoadAssembly = PluginRuntime.GetOptionalString(parameters, "intersectingRoadAssembly");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var mainAlign = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, mainRoadAlignment);
      var mainProf = CivilObjectUtils.FindProfileByName(mainAlign, transaction, mainRoadProfile, OpenMode.ForRead);
      var secAlign = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, intersectingRoadAlignment);
      var secProf = CivilObjectUtils.FindProfileByName(secAlign, transaction, intersectingRoadProfile, OpenMode.ForRead);

      // Compute intersection point if not provided
      Point2d intersectionPoint;
      if (intersectionX.HasValue && intersectionY.HasValue)
      {
        intersectionPoint = new Point2d(intersectionX.Value, intersectionY.Value);
      }
      else
      {
        intersectionPoint = ComputeAlignmentIntersection(mainAlign, secAlign)
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
            "Could not compute alignment intersection point. Please supply intersectionX and intersectionY explicitly.");
      }

      // Find site ID
      ObjectId siteId = ObjectId.Null;
      if (!string.IsNullOrWhiteSpace(siteName))
      {
        foreach (ObjectId sid in EnumerateSiteIds(civilDoc))
        {
          var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, sid, OpenMode.ForRead);
          if (string.Equals(site.Name, siteName, StringComparison.OrdinalIgnoreCase))
          {
            siteId = sid;
            break;
          }
        }
        if (siteId.IsNull)
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Site '{siteName}' was not found.");
      }
      else
      {
        siteId = EnumerateSiteIds(civilDoc).FirstOrDefault();
      }

      // Attempt to create via Intersection.Create factory method (Civil 3D API)
      var intersectionType = FindType("Autodesk.Civil.DatabaseServices.Intersection");
      if (intersectionType == null)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR",
          "Intersection type not found. Ensure Autodesk.Civil.DatabaseServices is loaded.");

      // Locate the Create overload
      var createMethods = intersectionType.GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Where(m => m.Name == "Create")
        .ToArray();

      ObjectId newId = ObjectId.Null;
      Exception? lastEx = null;

      foreach (var m in createMethods)
      {
        try
        {
          var parms = m.GetParameters();
          object?[]? args = parms.Length switch
          {
            5 => [name, mainAlign.ObjectId, mainProf.ObjectId, secAlign.ObjectId, secProf.ObjectId],
            6 => [name, mainAlign.ObjectId, mainProf.ObjectId, secAlign.ObjectId, secProf.ObjectId, siteId],
            7 => [name, mainAlign.ObjectId, mainProf.ObjectId, secAlign.ObjectId, secProf.ObjectId, siteId, intersectionPoint],
            _ => null,
          };

          if (args == null) continue;

          var result = m.Invoke(null, args);
          if (result is ObjectId oid && !oid.IsNull)
          {
            newId = oid;
            break;
          }
        }
        catch (Exception ex) { lastEx = ex; }
      }

      if (newId.IsNull)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR",
          $"Failed to create intersection: {lastEx?.Message ?? "No matching Intersection.Create overload found."}");

      var newInter = transaction.GetObject(newId, OpenMode.ForWrite);

      // Apply optional parameters
      if (offsetDistance.HasValue)
      {
        TrySetProperty(newInter, "OffsetDistance", offsetDistance.Value);
        TrySetProperty(newInter, "DefaultOffset", offsetDistance.Value);
      }
      if (curveRadius.HasValue)
      {
        TrySetProperty(newInter, "CurbReturnRadius", curveRadius.Value);
        TrySetProperty(newInter, "DefaultCurbReturnRadius", curveRadius.Value);
      }
      if (!string.IsNullOrWhiteSpace(siteName))
        TrySetProperty(newInter, "SiteName", siteName);

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["handle"] = CivilObjectUtils.GetHandle(newInter as AcDbObject ?? throw new Exception("Object is not a DBObject")),
        ["intersectionX"] = intersectionPoint.X,
        ["intersectionY"] = intersectionPoint.Y,
        ["mainRoadAlignment"] = mainRoadAlignment,
        ["intersectingRoadAlignment"] = intersectingRoadAlignment,
        ["created"] = true,
        ["message"] = $"Intersection '{name}' created successfully.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // getIntersection
  // -------------------------------------------------------------------------

  public static Task<object?> GetIntersectionAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var includeCorridorInfo = PluginRuntime.GetOptionalBool(parameters, "includeCorridorInfo") ?? false;
    var includeCurbReturns = PluginRuntime.GetOptionalBool(parameters, "includeCurbReturns") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var collection = CivilObjectUtils.GetPropertyValue<object>(civilDoc, "IntersectionCollection");
      if (collection == null)
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Intersection '{name}' was not found.");

      foreach (ObjectId id in CivilObjectUtils.ToObjectIds(collection))
      {
        var inter = transaction.GetObject(id, OpenMode.ForRead);
        if (inter == null) continue;
        if (!string.Equals(CivilObjectUtils.GetName(inter), name, StringComparison.OrdinalIgnoreCase)) continue;

        var ptProp = inter.GetType().GetProperty("Location") ?? inter.GetType().GetProperty("IntersectionPoint");
        Point2d? pt = null;
        if (ptProp?.GetValue(inter) is Point2d p2) pt = p2;
        else if (ptProp?.GetValue(inter) is Point3d p3) pt = new Point2d(p3.X, p3.Y);

        var mainAlignId = GetObjectIdProperty(inter, "MainRoadAlignmentId");
        var secAlignId = GetObjectIdProperty(inter, "IntersectingRoadAlignmentId");

        var result = new Dictionary<string, object?>
        {
          ["name"] = name,
          ["handle"] = inter is AcDbObject dbo ? CivilObjectUtils.GetHandle(dbo) : null,
          ["siteName"] = CivilObjectUtils.GetStringProperty(inter, "SiteName"),
          ["intersectionX"] = pt?.X,
          ["intersectionY"] = pt?.Y,
          ["mainRoadAlignment"] = mainAlignId.HasValue
            ? CivilObjectUtils.GetName(transaction.GetObject(mainAlignId.Value, OpenMode.ForRead)) : null,
          ["intersectingRoadAlignment"] = secAlignId.HasValue
            ? CivilObjectUtils.GetName(transaction.GetObject(secAlignId.Value, OpenMode.ForRead)) : null,
          ["offsetDistance"] = CivilObjectUtils.GetDoubleProperty(inter, "OffsetDistance")
            ?? CivilObjectUtils.GetDoubleProperty(inter, "DefaultOffset"),
          ["curbReturnRadius"] = CivilObjectUtils.GetDoubleProperty(inter, "CurbReturnRadius")
            ?? CivilObjectUtils.GetDoubleProperty(inter, "DefaultCurbReturnRadius"),
        };

        if (includeCurbReturns)
        {
          var curbReturns = new List<Dictionary<string, object?>>();
          var curbProp = inter.GetType().GetProperty("CurbReturnParameters")
            ?? inter.GetType().GetProperty("CurbReturns");
          if (curbProp != null)
          {
            var curbCollection = curbProp.GetValue(inter);
            if (curbCollection is System.Collections.IEnumerable curbs)
            {
              foreach (var curb in curbs)
              {
                curbReturns.Add(new Dictionary<string, object?>
                {
                  ["type"] = CivilObjectUtils.GetStringProperty(curb, "CurbReturnType") ?? curb?.GetType().Name,
                  ["radius"] = CivilObjectUtils.GetDoubleProperty(curb, "Radius"),
                  ["offset"] = CivilObjectUtils.GetDoubleProperty(curb, "Offset"),
                });
              }
            }
          }
          result["curbReturns"] = curbReturns;
        }

        if (includeCorridorInfo)
        {
          var corridorIds = CivilObjectUtils.InvokeMethod(inter, "GetCorridorIds") as System.Collections.IEnumerable;
          var corridors = new List<string?>();
          if (corridorIds != null)
          {
            foreach (var cid in corridorIds)
            {
              if (cid is ObjectId oid)
              {
                var corridor = transaction.GetObject(oid, OpenMode.ForRead);
                corridors.Add(CivilObjectUtils.GetName(corridor));
              }
            }
          }
          result["corridors"] = corridors;
        }

        return result;
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Intersection '{name}' was not found.");
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static Point2d? ComputeAlignmentIntersection(Alignment a1, Alignment a2)
  {
    // Sample points along alignment 1 and find closest point on alignment 2
    var step = (a1.EndingStation - a1.StartingStation) / 200.0;
    double bestDist = double.MaxValue;
    Point2d? bestPt = null;

    for (var st = a1.StartingStation; st <= a1.EndingStation; st += step)
    {
      double x = 0, y = 0;
      a1.PointLocation(st, 0, ref x, ref y);

      double closestStation = 0, offset = 0;
      try { a2.StationOffset(x, y, ref closestStation, ref offset); }
      catch { continue; }

      if (Math.Abs(offset) < bestDist)
      {
        bestDist = Math.Abs(offset);
        bestPt = new Point2d(x, y);
      }
    }

    return bestDist < 1.0 ? bestPt : null;
  }

  private static ObjectId? GetObjectIdProperty(object obj, string propertyName)
  {
    var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    if (prop == null) return null;
    var val = prop.GetValue(obj);
    if (val is ObjectId oid) return oid.IsNull ? null : oid;
    return null;
  }

  private static IEnumerable<ObjectId> EnumerateSiteIds(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc)
  {
    foreach (var memberName in new[] { "Sites", "SiteCollection" })
    {
      var prop = civilDoc.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId != ObjectId.Null)
          yield return objectId;
      }
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
    catch { /* ignore */ }
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
