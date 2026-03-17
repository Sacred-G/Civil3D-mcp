using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for Civil 3D COGO (Coordinate Geometry) calculations and Survey tools.
///
/// These tools perform geometry math that does not require an active drawing
/// (inverse, direction-distance, traverse, curve solve) as well as reflection-
/// based access to Survey databases and figures.
///
/// Civil 3D API notes:
///   AeccSurveyDocument  -- root survey object, accessed via reflection from CivilDocument
///   AeccSurveyDatabase  -- a named survey database (survey network container)
///   AeccSurveyFigure    -- a figure (polyline-like set of survey shots)
/// </summary>
public static class CogoCommands
{
  // -------------------------------------------------------------------------
  // cogoInverse  -- bearing + distance between two points
  // -------------------------------------------------------------------------

  public static Task<object?> CogoInverseAsync(JsonObject? parameters)
  {
    var x1 = PluginRuntime.GetRequiredDouble(parameters, "x1");
    var y1 = PluginRuntime.GetRequiredDouble(parameters, "y1");
    var x2 = PluginRuntime.GetRequiredDouble(parameters, "x2");
    var y2 = PluginRuntime.GetRequiredDouble(parameters, "y2");

    return Task.FromResult<object?>(ComputeInverse(x1, y1, x2, y2));
  }

  // -------------------------------------------------------------------------
  // cogoDirectionDistance  -- project a point given bearing and distance
  // -------------------------------------------------------------------------

  public static Task<object?> CogoDirectionDistanceAsync(JsonObject? parameters)
  {
    var fromX = PluginRuntime.GetRequiredDouble(parameters, "fromX");
    var fromY = PluginRuntime.GetRequiredDouble(parameters, "fromY");
    var bearingDegrees = PluginRuntime.GetRequiredDouble(parameters, "bearingDegrees");
    var distance = PluginRuntime.GetRequiredDouble(parameters, "distance");
    var fromZ = PluginRuntime.GetOptionalDouble(parameters, "fromZ") ?? 0;
    var slope = PluginRuntime.GetOptionalDouble(parameters, "slope"); // optional percent slope

    var bearingRadians = BearingToRadians(bearingDegrees);
    var dx = Math.Sin(bearingRadians) * distance;
    var dy = Math.Cos(bearingRadians) * distance;
    var dz = slope.HasValue ? distance * (slope.Value / 100.0) : 0;

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["fromX"] = fromX,
      ["fromY"] = fromY,
      ["fromZ"] = fromZ,
      ["toX"] = fromX + dx,
      ["toY"] = fromY + dy,
      ["toZ"] = fromZ + dz,
      ["bearingDegrees"] = bearingDegrees,
      ["distance"] = distance,
    });
  }

  // -------------------------------------------------------------------------
  // cogoTraverse  -- solve a series of bearing/distance courses
  // -------------------------------------------------------------------------

  public static Task<object?> CogoTraverseAsync(JsonObject? parameters)
  {
    var startX = PluginRuntime.GetRequiredDouble(parameters, "startX");
    var startY = PluginRuntime.GetRequiredDouble(parameters, "startY");
    var startZ = PluginRuntime.GetOptionalDouble(parameters, "startZ") ?? 0;
    var coursesNode = PluginRuntime.GetParameter(parameters, "courses") as JsonArray;
    var isClosed = PluginRuntime.GetOptionalBool(parameters, "isClosed") ?? false;

    if (coursesNode == null || coursesNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "cogoTraverse requires at least one course.");
    }

    var points = new List<Dictionary<string, object?>>
    {
      new() { ["x"] = startX, ["y"] = startY, ["z"] = startZ, ["station"] = 0.0 },
    };

    var currentX = startX;
    var currentY = startY;
    var currentZ = startZ;
    var totalDist = 0.0;

    foreach (var courseNode in coursesNode)
    {
      if (courseNode is not JsonObject course)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Each course must be a JSON object.");
      }

      var bearing = course["bearingDegrees"]?.GetValue<double>()
        ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Course missing bearingDegrees.");
      var dist = course["distance"]?.GetValue<double>()
        ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Course missing distance.");
      var slope = course["slope"]?.GetValue<double?>() ?? null;
      var description = course["description"]?.GetValue<string?>() ?? string.Empty;

      var bearingRad = BearingToRadians(bearing);
      currentX += Math.Sin(bearingRad) * dist;
      currentY += Math.Cos(bearingRad) * dist;
      if (slope.HasValue) currentZ += dist * (slope.Value / 100.0);
      totalDist += dist;

      points.Add(new Dictionary<string, object?>
      {
        ["x"] = currentX,
        ["y"] = currentY,
        ["z"] = currentZ,
        ["station"] = totalDist,
        ["description"] = description,
      });
    }

    // Closure error (for closed traverse)
    double? closureError = null;
    double? closureBearing = null;
    double? closurePrecision = null;

    if (isClosed)
    {
      var errorX = currentX - startX;
      var errorY = currentY - startY;
      closureError = Math.Sqrt(errorX * errorX + errorY * errorY);
      if (closureError > 0)
      {
        closureBearing = RadiansToBearing(Math.Atan2(errorX, errorY));
      }
      closurePrecision = totalDist > 0 && closureError > 0
        ? totalDist / closureError.Value
        : double.PositiveInfinity;
    }

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["startPoint"] = new Dictionary<string, double> { ["x"] = startX, ["y"] = startY, ["z"] = startZ },
      ["endPoint"] = new Dictionary<string, double> { ["x"] = currentX, ["y"] = currentY, ["z"] = currentZ },
      ["totalLength"] = totalDist,
      ["courseCount"] = coursesNode.Count,
      ["points"] = points,
      ["isClosed"] = isClosed,
      ["closureError"] = closureError,
      ["closureBearingDegrees"] = closureBearing,
      ["closurePrecision"] = closurePrecision == double.PositiveInfinity ? null : (object?)closurePrecision,
    });
  }

  // -------------------------------------------------------------------------
  // cogoCurveSolve  -- solve horizontal curve from two of: radius/delta/length/tangent/chord
  // -------------------------------------------------------------------------

  public static Task<object?> CogoCurveSolveAsync(JsonObject? parameters)
  {
    var radius = PluginRuntime.GetOptionalDouble(parameters, "radius");
    var deltaDegreesInput = PluginRuntime.GetOptionalDouble(parameters, "deltaDegrees");
    var length = PluginRuntime.GetOptionalDouble(parameters, "length");
    var tangent = PluginRuntime.GetOptionalDouble(parameters, "tangent");
    var chord = PluginRuntime.GetOptionalDouble(parameters, "chord");

    // Need at least two elements to solve
    double R, delta;

    if (radius.HasValue && deltaDegreesInput.HasValue)
    {
      R = radius.Value;
      delta = deltaDegreesInput.Value * Math.PI / 180.0;
    }
    else if (radius.HasValue && length.HasValue)
    {
      R = radius.Value;
      delta = length.Value / R;
    }
    else if (radius.HasValue && tangent.HasValue)
    {
      R = radius.Value;
      delta = 2 * Math.Atan(tangent.Value / R);
    }
    else if (radius.HasValue && chord.HasValue)
    {
      R = radius.Value;
      delta = 2 * Math.Asin(chord.Value / (2 * R));
    }
    else if (length.HasValue && tangent.HasValue)
    {
      // Iterative solve: L/2 = T => not closed form; approximate
      // T = R*tan(d/2), L = R*d => R = L/d, T = (L/d)*tan(d/2)
      // Solve numerically via Newton
      delta = SolveDeltaFromLengthAndTangent(length.Value, tangent.Value);
      R = length.Value / delta;
    }
    else
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        "cogoCurveSolve requires at least two of: radius, deltaDegrees, length, tangent, chord.");
    }

    var L = R * delta;
    var T = R * Math.Tan(delta / 2);
    var C = 2 * R * Math.Sin(delta / 2);
    var E = R * (1 / Math.Cos(delta / 2) - 1);
    var M = R * (1 - Math.Cos(delta / 2));

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["radius"] = R,
      ["deltaDegrees"] = delta * 180.0 / Math.PI,
      ["length"] = L,
      ["tangent"] = T,
      ["chord"] = C,
      ["externalDistance"] = E,
      ["middleOrdinate"] = M,
      ["degree"] = 5729.578 / R, // degree of curve (arc definition)
    });
  }

  // -------------------------------------------------------------------------
  // listSurveyDatabases
  // -------------------------------------------------------------------------

  public static Task<object?> ListSurveyDatabasesAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surveyDoc = GetSurveyDocument(civilDoc);
      if (surveyDoc == null)
      {
        return new Dictionary<string, object?> { ["databases"] = new List<object>(), ["note"] = "Survey not initialized in this drawing." };
      }

      var dbsProp = surveyDoc.GetType().GetProperty("Databases", BindingFlags.Public | BindingFlags.Instance)
        ?? surveyDoc.GetType().GetProperty("SurveyDatabases", BindingFlags.Public | BindingFlags.Instance);
      var dbs = dbsProp?.GetValue(surveyDoc);

      var result = new List<Dictionary<string, object?>>();
      foreach (var item in AsEnumerable(dbs))
      {
        result.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(item) ?? CivilObjectUtils.GetStringProperty(item, "DatabaseName"),
          ["path"] = CivilObjectUtils.GetStringProperty(item, "DatabasePath") ?? CivilObjectUtils.GetStringProperty(item, "Path"),
        });
      }

      return new Dictionary<string, object?> { ["databases"] = result };
    });
  }

  // -------------------------------------------------------------------------
  // createSurveyDatabase
  // -------------------------------------------------------------------------

  public static Task<object?> CreateSurveyDatabaseAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var path = PluginRuntime.GetOptionalString(parameters, "path");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surveyDoc = GetSurveyDocument(civilDoc);
      if (surveyDoc == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Survey API not available in this drawing.");
      }

      var dbsProp = surveyDoc.GetType().GetProperty("Databases", BindingFlags.Public | BindingFlags.Instance)
        ?? surveyDoc.GetType().GetProperty("SurveyDatabases", BindingFlags.Public | BindingFlags.Instance);
      var dbs = dbsProp?.GetValue(surveyDoc);

      var addMethod = dbs?.GetType().GetMethods()
        .FirstOrDefault(m => m.Name == "Add" || m.Name == "Create");

      if (addMethod == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "SurveyDatabases.Add/Create method not found.");
      }

      var args = addMethod.GetParameters().Length switch
      {
        2 when path != null => new object[] { name, path },
        _ => new object[] { name },
      };

      addMethod.Invoke(dbs, args);

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["path"] = path,
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // listSurveyFigures
  // -------------------------------------------------------------------------

  public static Task<object?> ListSurveyFiguresAsync(JsonObject? parameters)
  {
    var databaseName = PluginRuntime.GetOptionalString(parameters, "databaseName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var figures = new List<Dictionary<string, object?>>();
      var surveyDoc = GetSurveyDocument(civilDoc);
      if (surveyDoc == null)
      {
        return new Dictionary<string, object?> { ["figures"] = figures, ["note"] = "Survey not initialized." };
      }

      var dbs = GetSurveyDatabases(surveyDoc);
      foreach (var db in dbs)
      {
        var dbName = CivilObjectUtils.GetName(db) ?? CivilObjectUtils.GetStringProperty(db, "DatabaseName") ?? string.Empty;
        if (databaseName != null && !string.Equals(dbName, databaseName, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        var figuresProp = db.GetType().GetProperty("Figures", BindingFlags.Public | BindingFlags.Instance)
          ?? db.GetType().GetProperty("SurveyFigures", BindingFlags.Public | BindingFlags.Instance);
        var figs = figuresProp?.GetValue(db);

        foreach (var fig in AsEnumerable(figs))
        {
          figures.Add(new Dictionary<string, object?>
          {
            ["name"] = CivilObjectUtils.GetName(fig),
            ["databaseName"] = dbName,
            ["vertexCount"] = CivilObjectUtils.GetPropertyValue<int?>(fig, "VertexCount"),
            ["isClosed"] = CivilObjectUtils.GetBoolProperty(fig, "IsClosed"),
            ["layer"] = CivilObjectUtils.GetStringProperty(fig, "Layer"),
          });
        }
      }

      return new Dictionary<string, object?> { ["figures"] = figures };
    });
  }

  // -------------------------------------------------------------------------
  // getSurveyFigure
  // -------------------------------------------------------------------------

  public static Task<object?> GetSurveyFigureAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var databaseName = PluginRuntime.GetOptionalString(parameters, "databaseName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surveyDoc = GetSurveyDocument(civilDoc);
      if (surveyDoc == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Survey not initialized in this drawing.");
      }

      var dbs = GetSurveyDatabases(surveyDoc);
      foreach (var db in dbs)
      {
        var dbName = CivilObjectUtils.GetName(db) ?? string.Empty;
        if (databaseName != null && !string.Equals(dbName, databaseName, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        var figuresProp = db.GetType().GetProperty("Figures", BindingFlags.Public | BindingFlags.Instance)
          ?? db.GetType().GetProperty("SurveyFigures", BindingFlags.Public | BindingFlags.Instance);
        var figs = figuresProp?.GetValue(db);

        foreach (var fig in AsEnumerable(figs))
        {
          if (!string.Equals(CivilObjectUtils.GetName(fig), name, StringComparison.OrdinalIgnoreCase))
          {
            continue;
          }

          // Extract vertices
          var vertices = new List<Dictionary<string, double>>();
          var verticesMethod = fig.GetType().GetMethod("GetVertices", BindingFlags.Public | BindingFlags.Instance);
          var vertexCollection = verticesMethod?.Invoke(fig, Array.Empty<object>());
          foreach (var vtx in AsEnumerable(vertexCollection))
          {
            var px = CivilObjectUtils.GetDoubleProperty(vtx, "X") ?? CivilObjectUtils.GetDoubleProperty(vtx, "Easting") ?? 0;
            var py = CivilObjectUtils.GetDoubleProperty(vtx, "Y") ?? CivilObjectUtils.GetDoubleProperty(vtx, "Northing") ?? 0;
            var pz = CivilObjectUtils.GetDoubleProperty(vtx, "Z") ?? CivilObjectUtils.GetDoubleProperty(vtx, "Elevation") ?? 0;
            vertices.Add(new Dictionary<string, double> { ["x"] = px, ["y"] = py, ["z"] = pz });
          }

          return new Dictionary<string, object?>
          {
            ["name"] = CivilObjectUtils.GetName(fig),
            ["databaseName"] = dbName,
            ["isClosed"] = CivilObjectUtils.GetBoolProperty(fig, "IsClosed"),
            ["layer"] = CivilObjectUtils.GetStringProperty(fig, "Layer"),
            ["vertexCount"] = vertices.Count,
            ["vertices"] = vertices,
          };
        }
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Survey figure '{name}' not found.");
    });
  }

  // -------------------------------------------------------------------------
  // Pure math helpers (no drawing context needed)
  // -------------------------------------------------------------------------

  private static Dictionary<string, object?> ComputeInverse(double x1, double y1, double x2, double y2)
  {
    var dx = x2 - x1;
    var dy = y2 - y1;
    var distance = Math.Sqrt(dx * dx + dy * dy);
    var bearingDeg = distance > 0 ? RadiansToBearing(Math.Atan2(dx, dy)) : 0;

    return new Dictionary<string, object?>
    {
      ["fromX"] = x1,
      ["fromY"] = y1,
      ["toX"] = x2,
      ["toY"] = y2,
      ["distance"] = distance,
      ["bearingDegrees"] = bearingDeg,
      ["bearingDms"] = DecimalToDms(bearingDeg),
      ["deltaX"] = dx,
      ["deltaY"] = dy,
    };
  }

  private static double BearingToRadians(double bearingDegrees)
  {
    // Bearing is measured clockwise from North (Y axis)
    // Normalize to [0, 360)
    var normalized = ((bearingDegrees % 360) + 360) % 360;
    return normalized * Math.PI / 180.0;
  }

  private static double RadiansToBearing(double atan2Result)
  {
    // atan2(dx, dy) gives angle from North, but we want clockwise
    var deg = atan2Result * 180.0 / Math.PI;
    return ((deg % 360) + 360) % 360;
  }

  private static string DecimalToDms(double degrees)
  {
    var d = (int)degrees;
    var mFrac = (degrees - d) * 60;
    var m = (int)mFrac;
    var s = (mFrac - m) * 60;
    return $"{d:D3}°{m:D2}'{s:00.0}\"";
  }

  private static double SolveDeltaFromLengthAndTangent(double L, double T, int maxIter = 50)
  {
    // T = R*tan(d/2) and L = R*d => R = L/d
    // So T = (L/d)*tan(d/2). Solve for d.
    var delta = L / (L / 2); // initial guess: d ~ 1 radian
    for (var i = 0; i < maxIter; i++)
    {
      var f = (L / delta) * Math.Tan(delta / 2) - T;
      var fPrime = -(L / (delta * delta)) * Math.Tan(delta / 2) + (L / delta) * 0.5 / (Math.Cos(delta / 2) * Math.Cos(delta / 2));
      if (Math.Abs(fPrime) < 1e-12) break;
      var next = delta - f / fPrime;
      if (next <= 0 || next > 2 * Math.PI) break;
      if (Math.Abs(next - delta) < 1e-9) { delta = next; break; }
      delta = next;
    }
    return delta;
  }

  // -------------------------------------------------------------------------
  // Survey API helpers
  // -------------------------------------------------------------------------

  private static object? GetSurveyDocument(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc)
  {
    // Try SurveyDocument property
    var surveyProp = civilDoc.GetType().GetProperty("SurveyDocument", BindingFlags.Public | BindingFlags.Instance);
    return surveyProp?.GetValue(civilDoc);
  }

  private static IEnumerable<object> GetSurveyDatabases(object surveyDoc)
  {
    var dbsProp = surveyDoc.GetType().GetProperty("Databases", BindingFlags.Public | BindingFlags.Instance)
      ?? surveyDoc.GetType().GetProperty("SurveyDatabases", BindingFlags.Public | BindingFlags.Instance);
    return AsEnumerable(dbsProp?.GetValue(surveyDoc));
  }

  private static IEnumerable<object> AsEnumerable(object? collection)
  {
    if (collection is System.Collections.IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item != null) yield return item;
      }
    }
  }
}
