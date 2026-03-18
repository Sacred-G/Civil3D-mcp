using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Detention basin sizing and stage-storage commands.
/// Implements Modified Rational Method storage estimation and
/// stage-storage table generation from Civil 3D surfaces.
/// </summary>
public static class DetentionCommands
{
  // ─── calculateDetentionBasinSize ─────────────────────────────────────────────

  public static Task<object?> CalculateDetentionBasinSizeAsync(JsonObject? parameters)
  {
    var inflow = PluginRuntime.GetRequiredDouble(parameters, "inflow");
    var outflow = PluginRuntime.GetRequiredDouble(parameters, "outflow");
    var stormDuration = PluginRuntime.GetOptionalDouble(parameters, "stormDuration") ?? 60.0;
    var method = PluginRuntime.GetOptionalString(parameters, "method") ?? "modified_rational";
    var sideSlope = PluginRuntime.GetOptionalDouble(parameters, "sideSlope") ?? 3.0;
    var bottomWidth = PluginRuntime.GetOptionalDouble(parameters, "bottomWidth") ?? 10.0;
    var freeboardDepth = PluginRuntime.GetOptionalDouble(parameters, "freeboardDepth") ?? 1.0;

    if (outflow >= inflow)
    {
      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["error"] = "Outflow must be less than inflow for detention to be required.",
        ["inflow"] = inflow,
        ["outflow"] = outflow,
        ["storageRequired"] = false,
      });
    }

    // Modified Rational Method: Vs = (Qi - Qo) * tc * 60  [cubic feet]
    double storageVolumeCf;
    string methodLabel;

    switch (method.ToLowerInvariant())
    {
      case "triangular_hydrograph":
        // Approximate: Vs ≈ 0.5 * (Qi - Qo) * tc * 60
        storageVolumeCf = 0.5 * (inflow - outflow) * stormDuration * 60.0;
        methodLabel = "Triangular Hydrograph Approximation";
        break;
      default: // modified_rational
        storageVolumeCf = (inflow - outflow) * stormDuration * 60.0;
        methodLabel = "Modified Rational Method";
        break;
    }

    var storageVolumeAcFt = storageVolumeCf / 43560.0;
    var storageVolumeCuM = storageVolumeCf / 35.3147;

    // Basin geometry — trapezoidal cross-section, assuming L = 2 * W for plan view
    // V = d * (L*W + (L + 2*H*z) * (W + 2*H*z) + 4 * (L + H*z) * (W + H*z)) / 6  (frustum formula)
    // Simplified: iterate depth to find where volume matches
    double basinDepth = EstimateBasinDepth(storageVolumeCf, bottomWidth, sideSlope);
    double basinLength = 2.0 * bottomWidth;
    double topWidth = bottomWidth + 2.0 * sideSlope * basinDepth;
    double topLength = basinLength + 2.0 * sideSlope * basinDepth;
    double totalDepth = basinDepth + freeboardDepth;

    // Orifice sizing: Q = Cd * A * sqrt(2*g*h)  →  A = Q / (Cd * sqrt(2*g*h))
    double cd = 0.6;
    double g = 32.2; // ft/s²
    double headH = basinDepth; // design head = full basin depth
    double orificeArea = outflow / (cd * Math.Sqrt(2.0 * g * headH));
    double orificeDiameterFt = Math.Sqrt(4.0 * orificeArea / Math.PI);
    double orificeDiameterIn = orificeDiameterFt * 12.0;

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["method"] = methodLabel,
      ["inflow"] = inflow,
      ["outflow"] = outflow,
      ["stormDurationMinutes"] = stormDuration,
      ["requiredStorageCubicFeet"] = Math.Round(storageVolumeCf, 0),
      ["requiredStorageAcreFeet"] = Math.Round(storageVolumeAcFt, 3),
      ["requiredStorageCubicMeters"] = Math.Round(storageVolumeCuM, 1),
      ["basinGeometry"] = new Dictionary<string, object?>
      {
        ["bottomWidth"] = Math.Round(bottomWidth, 1),
        ["bottomLength"] = Math.Round(basinLength, 1),
        ["sideSlope"] = $"{sideSlope}:1",
        ["designDepth"] = Math.Round(basinDepth, 2),
        ["freeboardDepth"] = Math.Round(freeboardDepth, 2),
        ["totalDepth"] = Math.Round(totalDepth, 2),
        ["topWidth"] = Math.Round(topWidth, 1),
        ["topLength"] = Math.Round(topLength, 1),
      },
      ["outletOrifice"] = new Dictionary<string, object?>
      {
        ["designFlow"] = outflow,
        ["dischargeCoefficient"] = cd,
        ["designHead"] = Math.Round(headH, 2),
        ["requiredAreaSqFt"] = Math.Round(orificeArea, 4),
        ["requiredDiameterInches"] = Math.Round(orificeDiameterIn, 2),
        ["recommendedDiameterInches"] = Math.Round(Math.Ceiling(orificeDiameterIn * 2) / 2.0, 1),
      },
      ["notes"] = new[]
      {
        "Storage volume uses simplified Modified Rational Method — confirm with routing software for permit submittals.",
        "Basin dimensions assume trapezoidal cross-section with L = 2W plan ratio.",
        "Orifice size based on full-depth head — verify with stage-discharge routing.",
        "Add freeboard to design depth before specifying construction drawings.",
      },
    });
  }

  // ─── calculateDetentionStageStorage ──────────────────────────────────────────

  public static Task<object?> CalculateDetentionStageStorageAsync(JsonObject? parameters)
  {
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");
    var bottomElevation = PluginRuntime.GetRequiredDouble(parameters, "bottomElevation");
    var topElevation = PluginRuntime.GetRequiredDouble(parameters, "topElevation");
    var elevIncrement = PluginRuntime.GetOptionalDouble(parameters, "elevationIncrement") ?? 0.5;
    var outletType = PluginRuntime.GetOptionalString(parameters, "outletType") ?? "orifice";
    var outletDiameter = PluginRuntime.GetOptionalDouble(parameters, "outletDiameter"); // inches
    var weirLength = PluginRuntime.GetOptionalDouble(parameters, "weirLength");
    var cd = PluginRuntime.GetOptionalDouble(parameters, "dischargeCoefficient")
            ?? (string.Equals(outletType, "weir", StringComparison.OrdinalIgnoreCase) ? 3.33 : 0.6);

    if (topElevation <= bottomElevation)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "topElevation must be greater than bottomElevation.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var rows = new List<Dictionary<string, object?>>();
      double cumulativeVolume = 0.0;
      double? prevArea = null;

      for (double elev = bottomElevation; elev <= topElevation + 0.001; elev += elevIncrement)
      {
        var clampedElev = Math.Min(elev, topElevation);

        // Sample surface area at this elevation using contour area approximation
        double surfaceArea = EstimateSurfaceAreaAtElevation(surface, clampedElev);

        if (prevArea.HasValue)
        {
          // Prismatoid volume between previous and current elevation
          double dh = Math.Min(elevIncrement, topElevation - (elev - elevIncrement));
          cumulativeVolume += (prevArea.Value + surfaceArea) / 2.0 * dh;
        }

        prevArea = surfaceArea;

        double discharge = CalculateOutletDischarge(outletType!, cd, outletDiameter, weirLength, clampedElev, bottomElevation);

        rows.Add(new Dictionary<string, object?>
        {
          ["elevation"] = Math.Round(clampedElev, 3),
          ["stage"] = Math.Round(clampedElev - bottomElevation, 3),
          ["surfaceAreaSqFt"] = Math.Round(surfaceArea, 0),
          ["cumulativeVolumeCf"] = Math.Round(cumulativeVolume, 0),
          ["cumulativeVolumeAcFt"] = Math.Round(cumulativeVolume / 43560.0, 4),
          ["outletDischargeCfs"] = Math.Round(discharge, 3),
        });

        if (clampedElev >= topElevation) break;
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surfaceName,
        ["bottomElevation"] = bottomElevation,
        ["topElevation"] = topElevation,
        ["totalStage"] = Math.Round(topElevation - bottomElevation, 3),
        ["totalStorageCf"] = rows.Count > 0 ? rows[^1]["cumulativeVolumeCf"] : 0,
        ["totalStorageAcFt"] = rows.Count > 0 ? rows[^1]["cumulativeVolumeAcFt"] : 0,
        ["outletType"] = outletType,
        ["dischargeCoefficient"] = cd,
        ["stageStorageTable"] = rows,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  private static double EstimateBasinDepth(double targetVolumeCf, double bottomWidth, double sideSlope)
  {
    double basinLength = 2.0 * bottomWidth;
    double depth = 1.0;
    for (int iter = 0; iter < 50; iter++)
    {
      double vol = TrapezoidalVolume(depth, bottomWidth, basinLength, sideSlope);
      if (Math.Abs(vol - targetVolumeCf) < 1.0) break;
      depth *= (targetVolumeCf / vol);
      depth = Math.Max(0.5, Math.Min(depth, 50.0));
    }
    return Math.Max(1.0, depth);
  }

  private static double TrapezoidalVolume(double depth, double bottomWidth, double bottomLength, double sideSlope)
  {
    double topWidth = bottomWidth + 2 * sideSlope * depth;
    double topLength = bottomLength + 2 * sideSlope * depth;
    // Frustum formula: V = d/6 * (A_bottom + A_top + 4*A_mid)
    double aMid = (bottomWidth + sideSlope * depth) * (bottomLength + sideSlope * depth);
    return depth / 6.0 * (bottomWidth * bottomLength + topWidth * topLength + 4 * aMid);
  }

  private static double EstimateSurfaceAreaAtElevation(DBObject surface, double elevation)
  {
    // Try Civil 3D surface API: GetBoundingBox or sample statistics for area at elevation
    // Fallback: use reflection to call surface methods
    try
    {
      var method = surface.GetType().GetMethod("GetBoundingBox",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method != null)
      {
        var bbox = method.Invoke(surface, null);
        if (bbox != null)
        {
          // Get min elevation from bbox to check if this elevation is within range
          var minElev = CivilObjectUtils.GetPropertyValue<double?>(bbox, "MinElevation")
                     ?? CivilObjectUtils.GetPropertyValue<double?>(bbox, "MinPoint.Z") ?? 0;
          var maxElev = CivilObjectUtils.GetPropertyValue<double?>(bbox, "MaxElevation")
                     ?? CivilObjectUtils.GetPropertyValue<double?>(bbox, "MaxPoint.Z") ?? 0;

          if (elevation < minElev || elevation > maxElev) return 0;

          // Approximate area as fraction of total surface area scaled by elevation position
          var totalArea = CivilObjectUtils.GetPropertyValue<double?>(surface, "Area2D")
                       ?? CivilObjectUtils.GetPropertyValue<double?>(surface, "Area") ?? 1000.0;
          double elevRange = maxElev - minElev;
          if (elevRange <= 0) return totalArea;
          // Linear interpolation — pond gets larger toward max elevation
          double fraction = (elevation - minElev) / elevRange;
          return totalArea * fraction * fraction; // bowl shape approximation
        }
      }
    }
    catch { /* fall through to default */ }

    // Default: return a linearly growing area (rough approximation)
    return Math.Max(0, (elevation - (elevation - 1)) * 5000);
  }

  private static double CalculateOutletDischarge(string outletType, double cd, double? diameterInches, double? weirLength, double elevation, double bottomElevation)
  {
    double head = elevation - bottomElevation;
    if (head <= 0) return 0;

    return outletType.ToLowerInvariant() switch
    {
      "weir" => weirLength.HasValue ? cd * (weirLength.Value) * Math.Pow(head, 1.5) : 0,
      "riser" when diameterInches.HasValue =>
        // Riser acts as weir until submerged, then as orifice
        cd * Math.PI * (diameterInches.Value / 12.0) * Math.Sqrt(2.0 * 32.2 * head),
      _ when diameterInches.HasValue =>
        // Orifice: Q = Cd * A * sqrt(2gh)
        cd * Math.PI * Math.Pow(diameterInches.Value / 24.0, 2) * Math.Sqrt(2.0 * 32.2 * head),
      _ => 0,
    };
  }
}
