using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

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
          ["name"] = "watershed",
          ["status"] = "planned",
          ["description"] = "Future watershed delineation support.",
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

  private static FlowPoint? FindSteepestDescentPoint(Surface surface, double x, double y, double currentElevation, double stepDistance)
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

  private static double GetSurfaceElevation(Surface surface, double x, double y)
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
}
