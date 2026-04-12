using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

public static class HydrologyCommands
{
  public static Task<object?> ListHydrologyCapabilitiesAsync()
  {
    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["domain"] = "hydrology",
      ["operations"] = new List<Dictionary<string, object?>>()
      {
        new()
        {
          ["name"] = "list_capabilities",
          ["status"] = "implemented",
          ["description"] = "Lists currently supported hydrology-domain operations in the plugin.",
        },
        new()
        {
          ["name"] = "trace_flow_path",
          ["status"] = "implemented",
          ["description"] = "Traces a downhill flow path over a surface by iteratively following the steepest sampled descent.",
        },
        new()
        {
          ["name"] = "find_low_point",
          ["status"] = "implemented",
          ["description"] = "Approximates the lowest point on a surface by sampling its extents on a regular grid.",
        },
        new()
        {
          ["name"] = "delineate_watershed",
          ["status"] = "implemented",
          ["description"] = "Delineates a watershed boundary by tracing flow paths from perimeter points to determine contributing area to an outlet point.",
        },
        new()
        {
          ["name"] = "calculate_catchment_area",
          ["status"] = "implemented",
          ["description"] = "Calculates the catchment area for a given outlet point by analyzing surface drainage patterns.",
        },
        new()
        {
          ["name"] = "estimate_runoff",
          ["status"] = "implemented",
          ["description"] = "Estimates peak runoff with the Rational Method using user-provided drainage area, runoff coefficient, and rainfall intensity.",
        },
      },
      ["notes"] = new List<string>
      {
        "This MVP focuses on surface-based flow tracing and capability discovery.",
        "Runoff estimation uses the Rational Method with user-supplied inputs and does not automatically delineate drainage areas.",
        "Watershed delineation requires additional validated Civil 3D or drainage-specific workflows.",
      },
    });
  }

  public static Task<object?> TraceFlowPathAsync(JsonObject? parameters)
  {
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var stepDistance = PluginRuntime.GetOptionalDouble(parameters, "stepDistance") ?? 5d;
    var maxSteps = PluginRuntime.GetOptionalInt(parameters, "maxSteps") ?? 100;

    if (stepDistance <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "traceHydrologyFlowPath requires a positive stepDistance.");
    }

    if (maxSteps <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "traceHydrologyFlowPath requires maxSteps greater than zero.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var points = new List<Dictionary<string, object?>>();
      var currentX = x;
      var currentY = y;
      var currentElevation = GetSurfaceElevation(surface, currentX, currentY);
      var startElevation = currentElevation;
      points.Add(ToHydrologyPoint(currentX, currentY, currentElevation));

      var status = "complete";
      var stepCount = 0;

      for (var stepIndex = 0; stepIndex < maxSteps; stepIndex++)
      {
        var nextPoint = FindSteepestDescentPoint(surface, currentX, currentY, currentElevation, stepDistance);
        if (nextPoint == null)
        {
          status = stepIndex == 0 ? "stopped_flat" : "stopped_local_minimum";
          break;
        }

        currentX = nextPoint.X;
        currentY = nextPoint.Y;
        currentElevation = nextPoint.Elevation;
        points.Add(ToHydrologyPoint(currentX, currentY, currentElevation));
        stepCount++;

        if (stepIndex == maxSteps - 1)
        {
          status = "max_steps_reached";
        }
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["status"] = status,
        ["stepDistance"] = stepDistance,
        ["stepCount"] = stepCount,
        ["totalDistance"] = stepCount * stepDistance,
        ["dropElevation"] = startElevation - currentElevation,
        ["startPoint"] = points.First(),
        ["endPoint"] = points.Last(),
        ["points"] = points,
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = CivilObjectUtils.LinearUnits(database),
          ["vertical"] = CivilObjectUtils.LinearUnits(database),
        },
      };
    });
  }

  public static Task<object?> FindLowPointAsync(JsonObject? parameters)
  {
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");
    var sampleSpacing = PluginRuntime.GetOptionalDouble(parameters, "sampleSpacing") ?? 25d;

    if (sampleSpacing <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "findHydrologyLowPoint requires a positive sampleSpacing.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var extents = CivilObjectUtils.GetPropertyValue<Extents3d?>(surface, "GeometricExtents");
      if (!extents.HasValue)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface extents are not available for surface '{surface.Name}'.");
      }

      FlowPoint? bestPoint = null;
      var sampledPointCount = 0;

      for (var sampleX = extents.Value.MinPoint.X; sampleX <= extents.Value.MaxPoint.X; sampleX += sampleSpacing)
      {
        for (var sampleY = extents.Value.MinPoint.Y; sampleY <= extents.Value.MaxPoint.Y; sampleY += sampleSpacing)
        {
          double elevation;
          try
          {
            elevation = GetSurfaceElevation(surface, sampleX, sampleY);
          }
          catch
          {
            continue;
          }

          sampledPointCount++;
          if (bestPoint == null || elevation < bestPoint.Elevation)
          {
            bestPoint = new FlowPoint(sampleX, sampleY, elevation);
          }
        }
      }

      if (bestPoint == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to sample a valid low point on surface '{surface.Name}'.");
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["sampleSpacing"] = sampleSpacing,
        ["sampledPointCount"] = sampledPointCount,
        ["lowPoint"] = ToHydrologyPoint(bestPoint.X, bestPoint.Y, bestPoint.Elevation),
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = CivilObjectUtils.LinearUnits(database),
          ["vertical"] = CivilObjectUtils.LinearUnits(database),
        },
      };
    });
  }

  public static Task<object?> EstimateRunoffAsync(JsonObject? parameters)
  {
    var drainageArea = PluginRuntime.GetRequiredDouble(parameters, "drainageArea");
    var runoffCoefficient = PluginRuntime.GetRequiredDouble(parameters, "runoffCoefficient");
    var rainfallIntensity = PluginRuntime.GetRequiredDouble(parameters, "rainfallIntensity");
    var areaUnits = PluginRuntime.GetRequiredString(parameters, "areaUnits");
    var intensityUnits = PluginRuntime.GetRequiredString(parameters, "intensityUnits");

    if (drainageArea <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires a positive drainageArea.");
    }

    if (runoffCoefficient < 0 || runoffCoefficient > 1)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires runoffCoefficient between 0 and 1.");
    }

    if (rainfallIntensity <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires a positive rainfallIntensity.");
    }

    var drainageAreaAcres = areaUnits switch
    {
      "acres" => drainageArea,
      "hectares" => drainageArea * 2.471053814671653,
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires areaUnits to be 'acres' or 'hectares'."),
    };

    var drainageAreaHectares = areaUnits switch
    {
      "acres" => drainageArea * 0.40468564224,
      "hectares" => drainageArea,
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires areaUnits to be 'acres' or 'hectares'."),
    };

    var rainfallIntensityInPerHr = intensityUnits switch
    {
      "in_per_hr" => rainfallIntensity,
      "mm_per_hr" => rainfallIntensity / 25.4,
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires intensityUnits to be 'in_per_hr' or 'mm_per_hr'."),
    };

    var rainfallIntensityMmPerHr = intensityUnits switch
    {
      "in_per_hr" => rainfallIntensity * 25.4,
      "mm_per_hr" => rainfallIntensity,
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "estimateHydrologyRunoff requires intensityUnits to be 'in_per_hr' or 'mm_per_hr'."),
    };

    var runoffRateCfs = runoffCoefficient * rainfallIntensityInPerHr * drainageAreaAcres;
    var runoffRateCubicMetersPerSecond = runoffCoefficient * rainfallIntensityMmPerHr * drainageAreaHectares / 360d;

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["method"] = "rational",
      ["drainageArea"] = drainageArea,
      ["runoffCoefficient"] = runoffCoefficient,
      ["rainfallIntensity"] = rainfallIntensity,
      ["areaUnits"] = areaUnits,
      ["intensityUnits"] = intensityUnits,
      ["runoffRate"] = new Dictionary<string, object?>
      {
        ["cfs"] = runoffRateCfs,
        ["cubicMetersPerSecond"] = runoffRateCubicMetersPerSecond,
      },
      ["normalizedInputs"] = new Dictionary<string, object?>
      {
        ["drainageAreaAcres"] = drainageAreaAcres,
        ["drainageAreaHectares"] = drainageAreaHectares,
        ["rainfallIntensityInPerHr"] = rainfallIntensityInPerHr,
        ["rainfallIntensityMmPerHr"] = rainfallIntensityMmPerHr,
      },
    });
  }

  private static Dictionary<string, object?> ToHydrologyPoint(double x, double y, double elevation)
  {
    return new Dictionary<string, object?>
    {
      ["x"] = x,
      ["y"] = y,
      ["elevation"] = elevation,
    };
  }

  private static FlowPoint? FindSteepestDescentPoint(CivilSurface surface, double x, double y, double currentElevation, double stepDistance)
  {
    FlowPoint? bestPoint = null;
    double[] directions = [0, 45, 90, 135, 180, 225, 270, 315];

    foreach (var angleDegrees in directions)
    {
      var angleRadians = Math.PI * angleDegrees / 180d;
      var candidateX = x + Math.Cos(angleRadians) * stepDistance;
      var candidateY = y + Math.Sin(angleRadians) * stepDistance;
      double candidateElevation;

      try
      {
        candidateElevation = GetSurfaceElevation(surface, candidateX, candidateY);
      }
      catch
      {
        continue;
      }

      if (candidateElevation >= currentElevation - 1e-6)
      {
        continue;
      }

      if (bestPoint == null || candidateElevation < bestPoint.Elevation)
      {
        bestPoint = new FlowPoint(candidateX, candidateY, candidateElevation);
      }
    }

    return bestPoint;
  }

  private static double GetSurfaceElevation(CivilSurface surface, double x, double y)
  {
    foreach (var methodName in new[] { "FindElevationAtXY", "GetElevationAtXY" })
    {
      var value = CivilObjectUtils.InvokeMethod(surface, methodName, x, y);
      if (value != null)
      {
        return Convert.ToDouble(value);
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface elevation API is not available for surface '{surface.Name}'.");
  }

  private sealed record FlowPoint(double X, double Y, double Elevation);

  public static Task<object?> DelineateWatershedAsync(JsonObject? parameters)
  {
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");
    var outletX = PluginRuntime.GetRequiredDouble(parameters, "outletX");
    var outletY = PluginRuntime.GetRequiredDouble(parameters, "outletY");
    var gridSpacing = PluginRuntime.GetOptionalDouble(parameters, "gridSpacing") ?? 10d;
    var searchRadius = PluginRuntime.GetOptionalDouble(parameters, "searchRadius") ?? 100d;

    if (gridSpacing <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "delineateWatershed requires a positive gridSpacing.");
    }

    if (searchRadius <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "delineateWatershed requires a positive searchRadius.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var outletElevation = GetSurfaceElevation(surface, outletX, outletY);
      
      var watershedPoints = new List<Dictionary<string, object?>>();
      var contributingPoints = new HashSet<(double x, double y)>();
      var testedPoints = new HashSet<(double x, double y)>();
      
      var minX = outletX - searchRadius;
      var maxX = outletX + searchRadius;
      var minY = outletY - searchRadius;
      var maxY = outletY + searchRadius;

      for (var testX = minX; testX <= maxX; testX += gridSpacing)
      {
        for (var testY = minY; testY <= maxY; testY += gridSpacing)
        {
          var key = (Math.Round(testX / gridSpacing) * gridSpacing, Math.Round(testY / gridSpacing) * gridSpacing);
          if (testedPoints.Contains(key))
          {
            continue;
          }
          testedPoints.Add(key);

          double testElevation;
          try
          {
            testElevation = GetSurfaceElevation(surface, testX, testY);
          }
          catch
          {
            continue;
          }

          var flowPath = TraceFlowToOutlet(surface, testX, testY, outletX, outletY, gridSpacing * 0.5, 500);
          if (flowPath.ReachesOutlet)
          {
            contributingPoints.Add((testX, testY));
          }
        }
      }

      foreach (var point in contributingPoints)
      {
        double elevation;
        try
        {
          elevation = GetSurfaceElevation(surface, point.x, point.y);
        }
        catch
        {
          elevation = 0;
        }
        watershedPoints.Add(ToHydrologyPoint(point.x, point.y, elevation));
      }

      var boundaryPoints = ComputeConvexHull(contributingPoints.ToList());
      var boundaryCoordinates = boundaryPoints.Select(p => ToHydrologyPoint(p.x, p.y, GetSurfaceElevation(surface, p.x, p.y))).ToList();

      var area = CalculatePolygonArea(boundaryPoints);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["outletPoint"] = ToHydrologyPoint(outletX, outletY, outletElevation),
        ["gridSpacing"] = gridSpacing,
        ["searchRadius"] = searchRadius,
        ["contributingPointCount"] = contributingPoints.Count,
        ["boundaryPoints"] = boundaryCoordinates,
        ["approximateArea"] = area,
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = CivilObjectUtils.LinearUnits(database),
          ["vertical"] = CivilObjectUtils.LinearUnits(database),
          ["area"] = $"{CivilObjectUtils.LinearUnits(database)}²",
        },
      };
    });
  }

  public static Task<object?> CalculateCatchmentAreaAsync(JsonObject? parameters)
  {
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");
    var outletX = PluginRuntime.GetRequiredDouble(parameters, "outletX");
    var outletY = PluginRuntime.GetRequiredDouble(parameters, "outletY");
    var sampleSpacing = PluginRuntime.GetOptionalDouble(parameters, "sampleSpacing") ?? 15d;
    var maxDistance = PluginRuntime.GetOptionalDouble(parameters, "maxDistance") ?? 200d;

    if (sampleSpacing <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "calculateCatchmentArea requires a positive sampleSpacing.");
    }

    if (maxDistance <= 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "calculateCatchmentArea requires a positive maxDistance.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var outletElevation = GetSurfaceElevation(surface, outletX, outletY);

      var contributingCells = new List<(double x, double y, double elevation)>();
      var minX = outletX - maxDistance;
      var maxX = outletX + maxDistance;
      var minY = outletY - maxDistance;
      var maxY = outletY + maxDistance;

      for (var sampleX = minX; sampleX <= maxX; sampleX += sampleSpacing)
      {
        for (var sampleY = minY; sampleY <= maxY; sampleY += sampleSpacing)
        {
          double elevation;
          try
          {
            elevation = GetSurfaceElevation(surface, sampleX, sampleY);
          }
          catch
          {
            continue;
          }

          var flowResult = TraceFlowToOutlet(surface, sampleX, sampleY, outletX, outletY, sampleSpacing * 0.5, 300);
          if (flowResult.ReachesOutlet)
          {
            contributingCells.Add((sampleX, sampleY, elevation));
          }
        }
      }

      var cellArea = sampleSpacing * sampleSpacing;
      var totalArea = contributingCells.Count * cellArea;

      var elevations = contributingCells.Select(c => c.elevation).ToList();
      var avgElevation = elevations.Any() ? elevations.Average() : outletElevation;
      var maxElevation = elevations.Any() ? elevations.Max() : outletElevation;
      var minElevation = elevations.Any() ? elevations.Min() : outletElevation;

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["outletPoint"] = ToHydrologyPoint(outletX, outletY, outletElevation),
        ["sampleSpacing"] = sampleSpacing,
        ["maxDistance"] = maxDistance,
        ["contributingCellCount"] = contributingCells.Count,
        ["catchmentArea"] = totalArea,
        ["elevationStatistics"] = new Dictionary<string, object?>
        {
          ["minimum"] = minElevation,
          ["maximum"] = maxElevation,
          ["average"] = avgElevation,
          ["relief"] = maxElevation - minElevation,
        },
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = CivilObjectUtils.LinearUnits(database),
          ["vertical"] = CivilObjectUtils.LinearUnits(database),
          ["area"] = $"{CivilObjectUtils.LinearUnits(database)}²",
        },
      };
    });
  }

  private static (bool ReachesOutlet, int Steps) TraceFlowToOutlet(CivilSurface surface, double startX, double startY, double outletX, double outletY, double stepDistance, int maxSteps)
  {
    var currentX = startX;
    var currentY = startY;
    var tolerance = stepDistance * 2;

    for (var step = 0; step < maxSteps; step++)
    {
      var distanceToOutlet = Math.Sqrt(Math.Pow(currentX - outletX, 2) + Math.Pow(currentY - outletY, 2));
      if (distanceToOutlet <= tolerance)
      {
        return (true, step);
      }

      double currentElevation;
      try
      {
        currentElevation = GetSurfaceElevation(surface, currentX, currentY);
      }
      catch
      {
        return (false, step);
      }

      var nextPoint = FindSteepestDescentPoint(surface, currentX, currentY, currentElevation, stepDistance);
      if (nextPoint == null)
      {
        return (false, step);
      }

      currentX = nextPoint.X;
      currentY = nextPoint.Y;
    }

    return (false, maxSteps);
  }

  private static List<(double x, double y)> ComputeConvexHull(List<(double x, double y)> points)
  {
    if (points.Count < 3)
    {
      return points;
    }

    var sorted = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
    var lower = new List<(double x, double y)>();
    
    foreach (var p in sorted)
    {
      while (lower.Count >= 2 && CrossProduct(lower[^2], lower[^1], p) <= 0)
      {
        lower.RemoveAt(lower.Count - 1);
      }
      lower.Add(p);
    }

    var upper = new List<(double x, double y)>();
    for (var i = sorted.Count - 1; i >= 0; i--)
    {
      var p = sorted[i];
      while (upper.Count >= 2 && CrossProduct(upper[^2], upper[^1], p) <= 0)
      {
        upper.RemoveAt(upper.Count - 1);
      }
      upper.Add(p);
    }

    lower.RemoveAt(lower.Count - 1);
    upper.RemoveAt(upper.Count - 1);
    lower.AddRange(upper);
    
    return lower;
  }

  private static double CrossProduct((double x, double y) o, (double x, double y) a, (double x, double y) b)
  {
    return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
  }

  private static double CalculatePolygonArea(List<(double x, double y)> points)
  {
    if (points.Count < 3)
    {
      return 0;
    }

    var area = 0d;
    for (var i = 0; i < points.Count; i++)
    {
      var j = (i + 1) % points.Count;
      area += points[i].x * points[j].y;
      area -= points[j].x * points[i].y;
    }
    
    return Math.Abs(area) / 2d;
  }
}
