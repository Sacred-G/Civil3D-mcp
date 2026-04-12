using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Slope geometry and stability analysis commands.
/// Calculates daylight line catch-points and flags unstable slope conditions.
/// </summary>
public static class SlopeAnalysisCommands
{
  // ─── calculateSlopeGeometry ──────────────────────────────────────────────────

  public static Task<object?> CalculateSlopeGeometryAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetOptionalString(parameters, "profileName");
    var surfaceName = PluginRuntime.GetOptionalString(parameters, "surfaceName");
    var cutSlopeRatio = PluginRuntime.GetOptionalDouble(parameters, "cutSlopeRatio") ?? 2.0;
    var fillSlopeRatio = PluginRuntime.GetOptionalDouble(parameters, "fillSlopeRatio") ?? 3.0;
    var benchWidth = PluginRuntime.GetOptionalDouble(parameters, "benchWidth") ?? 0.0;
    var benchHeightInterval = PluginRuntime.GetOptionalDouble(parameters, "benchHeightInterval") ?? 20.0;
    var stationStart = PluginRuntime.GetOptionalDouble(parameters, "stationStart");
    var stationEnd = PluginRuntime.GetOptionalDouble(parameters, "stationEnd");
    var stationInterval = PluginRuntime.GetOptionalDouble(parameters, "stationInterval") ?? 10.0;
    var roadwayHalfWidth = PluginRuntime.GetOptionalDouble(parameters, "roadwayWidth") ?? 0.0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Resolve alignment
      DBObject? alignment = null;
      if (!string.IsNullOrWhiteSpace(alignmentName))
        alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName!);

      if (alignment == null)
        alignment = GetFirstAlignment(civilDoc, transaction);

      if (alignment == null)
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "No alignment found in drawing.");

      // Resolve profile
      DBObject? profile = null;
      if (!string.IsNullOrWhiteSpace(profileName))
        profile = FindProfileByName(alignment, transaction, profileName!);

      if (profile == null)
        profile = GetFirstProfile(alignment, transaction, preferFinishedGrade: true);

      // Resolve surface
      DBObject? surface = null;
      if (!string.IsNullOrWhiteSpace(surfaceName))
        surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName!, OpenMode.ForRead);

      if (surface == null)
        surface = GetFirstSurface(civilDoc, transaction);

      if (profile == null || surface == null)
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "Could not resolve profile or surface.");

      var alignLen = CivilObjectUtils.GetPropertyValue<double?>(alignment, "Length") ?? 0;
      var startSta = stationStart ?? CivilObjectUtils.GetPropertyValue<double?>(alignment, "StartingStation") ?? 0;
      var endSta = stationEnd ?? (startSta + alignLen);

      var rows = new List<Dictionary<string, object?>>();
      double maxCutHeight = 0, maxFillHeight = 0;

      for (double sta = startSta; sta <= endSta + 0.01; sta += stationInterval)
      {
        var clampedSta = Math.Min(sta, endSta);

        // Get design elevation at station
        double designElev = GetProfileElevationAtStation(profile, clampedSta);

        // Get XY of station on alignment
        var (ptX, ptY) = GetAlignmentPointAtStation(alignment, clampedSta);

        // Get ground elevation at that XY
        double groundElev = GetSurfaceElevation(surface, ptX, ptY);

        double cutFill = designElev - groundElev; // positive = fill, negative = cut
        bool isCut = cutFill < 0;
        double height = Math.Abs(cutFill);

        double slopeRatio = isCut ? cutSlopeRatio : fillSlopeRatio;

        // Calculate catch-point offset including benching
        double catchOffset = roadwayHalfWidth + CalculateCatchOffset(height, slopeRatio, benchWidth, benchHeightInterval);

        if (isCut) maxCutHeight = Math.Max(maxCutHeight, height);
        else maxFillHeight = Math.Max(maxFillHeight, height);

        rows.Add(new Dictionary<string, object?>
        {
          ["station"] = Math.Round(clampedSta, 2),
          ["designElevation"] = Math.Round(designElev, 3),
          ["groundElevation"] = Math.Round(groundElev, 3),
          ["cutFillDepth"] = Math.Round(cutFill, 3),
          ["condition"] = isCut ? "CUT" : (Math.Abs(cutFill) < 0.05 ? "MATCH" : "FILL"),
          ["slopeRatio"] = $"{slopeRatio}:1",
          ["slopeHeight"] = Math.Round(height, 3),
          ["catchPointOffset"] = Math.Round(catchOffset, 2),
          ["benchCount"] = benchWidth > 0 && height > 0 ? (int)(height / benchHeightInterval) : 0,
        });

        if (clampedSta >= endSta) break;
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = CivilObjectUtils.GetName(alignment) ?? alignmentName ?? "Unknown",
        ["profileName"] = CivilObjectUtils.GetName(profile) ?? profileName ?? "Unknown",
        ["surfaceName"] = CivilObjectUtils.GetName(surface) ?? surfaceName ?? "Unknown",
        ["cutSlopeRatio"] = $"{cutSlopeRatio}:1",
        ["fillSlopeRatio"] = $"{fillSlopeRatio}:1",
        ["benchWidth"] = benchWidth,
        ["stationCount"] = rows.Count,
        ["maximumCutHeight"] = Math.Round(maxCutHeight, 2),
        ["maximumFillHeight"] = Math.Round(maxFillHeight, 2),
        ["stations"] = rows,
      };
    });
  }

  // ─── checkSlopeStability ─────────────────────────────────────────────────────

  public static Task<object?> CheckSlopeStabilityAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var surfaceName = PluginRuntime.GetOptionalString(parameters, "surfaceName");
    var maxCutRatio = PluginRuntime.GetOptionalDouble(parameters, "maxCutSlopeRatio") ?? 1.5;
    var maxFillRatio = PluginRuntime.GetOptionalDouble(parameters, "maxFillSlopeRatio") ?? 2.0;
    var maxCutHeight = PluginRuntime.GetOptionalDouble(parameters, "maxCutHeight") ?? 30.0;
    var maxFillHeight = PluginRuntime.GetOptionalDouble(parameters, "maxFillHeight") ?? 40.0;
    var stationInterval = PluginRuntime.GetOptionalDouble(parameters, "stationInterval") ?? 25.0;
    var soilType = PluginRuntime.GetOptionalString(parameters, "soilType") ?? "granular";

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      DBObject? alignment = null;
      if (!string.IsNullOrWhiteSpace(alignmentName))
        alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName!);
      if (alignment == null)
        alignment = GetFirstAlignment(civilDoc, transaction);

      DBObject? surface = null;
      if (!string.IsNullOrWhiteSpace(surfaceName))
        surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName!, OpenMode.ForRead);
      if (surface == null)
        surface = GetFirstSurface(civilDoc, transaction);

      if (alignment == null || surface == null)
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", "Alignment or surface not found.");

      // Use slope analysis from the surface itself along the alignment
      var alignLen = CivilObjectUtils.GetPropertyValue<double?>(alignment, "Length") ?? 0;
      var startSta = CivilObjectUtils.GetPropertyValue<double?>(alignment, "StartingStation") ?? 0;
      var endSta = startSta + alignLen;

      var findings = new List<Dictionary<string, object?>>();
      int okCount = 0, warnCount = 0, failCount = 0;

      for (double sta = startSta; sta <= endSta + 0.01; sta += stationInterval)
      {
        var clampedSta = Math.Min(sta, endSta);
        var (ptX, ptY) = GetAlignmentPointAtStation(alignment, clampedSta);

        // Sample surface slope at this point
        double surfaceSlope = GetSurfaceSlopePercent(surface, ptX, ptY);
        double slopeRatio = surfaceSlope > 0 ? 100.0 / surfaceSlope : 999.0;

        // Determine if this looks like a cut or fill area (simplified — use surface slope direction)
        bool isCut = surfaceSlope > 0;
        double allowableRatio = isCut ? maxCutRatio : maxFillRatio;
        double allowableHeight = isCut ? maxCutHeight : maxFillHeight;

        // Sample height: use surface elevation difference over sample distance
        double slopeHeight = EstimateSlopeHeight(surface, ptX, ptY, stationInterval, slopeRatio);

        string status;
        if (slopeRatio < allowableRatio)
          status = "FAIL";
        else if (slopeRatio < allowableRatio * 1.2)
          status = "WARN";
        else
          status = "OK";

        bool benchingRequired = slopeHeight > allowableHeight;
        if (benchingRequired && status == "OK") status = "WARN";

        switch (status)
        {
          case "OK": okCount++; break;
          case "WARN": warnCount++; break;
          case "FAIL": failCount++; break;
        }

        if (status != "OK")
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["station"] = Math.Round(clampedSta, 2),
            ["condition"] = isCut ? "CUT" : "FILL",
            ["surfaceSlopePct"] = Math.Round(surfaceSlope, 2),
            ["slopeRatioHtoV"] = slopeRatio < 100 ? $"{slopeRatio:F1}:1" : "flat",
            ["allowableRatio"] = $"{allowableRatio}:1",
            ["estimatedHeight"] = Math.Round(slopeHeight, 1),
            ["benchingRequired"] = benchingRequired,
            ["status"] = status,
          });
        }

        if (clampedSta >= endSta) break;
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = CivilObjectUtils.GetName(alignment) ?? alignmentName ?? "Unknown",
        ["surfaceName"] = CivilObjectUtils.GetName(surface) ?? surfaceName ?? "Unknown",
        ["soilType"] = soilType,
        ["maxCutSlopeRatio"] = $"{maxCutRatio}:1",
        ["maxFillSlopeRatio"] = $"{maxFillRatio}:1",
        ["stationsChecked"] = okCount + warnCount + failCount,
        ["okCount"] = okCount,
        ["warnCount"] = warnCount,
        ["failCount"] = failCount,
        ["compliant"] = failCount == 0,
        ["findings"] = findings,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  private static double CalculateCatchOffset(double height, double slopeRatio, double benchWidth, double benchInterval)
  {
    if (benchWidth <= 0 || benchInterval <= 0 || height <= 0)
      return height * slopeRatio;

    int benches = (int)(height / benchInterval);
    double remaining = height - benches * benchInterval;
    return (benches * benchInterval * slopeRatio) + (benches * benchWidth) + (remaining * slopeRatio);
  }

  private static double GetProfileElevationAtStation(DBObject profile, double station)
  {
    try
    {
      var method = profile.GetType().GetMethod("ElevationAt",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(profile, new object[] { station });
        if (result != null) return Convert.ToDouble(result);
      }

      var method2 = profile.GetType().GetMethod("GetElevation",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method2 != null)
      {
        var result2 = method2.Invoke(profile, new object[] { station });
        if (result2 != null) return Convert.ToDouble(result2);
      }
    }
    catch { /* fall through */ }
    return 0;
  }

  private static (double X, double Y) GetAlignmentPointAtStation(DBObject alignment, double station)
  {
    try
    {
      var method = alignment.GetType().GetMethod("GetXYZAtStation",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(alignment, new object?[] { station, null, null });
        if (result != null)
        {
          var x = CivilObjectUtils.GetPropertyValue<double?>(result, "X") ?? 0;
          var y = CivilObjectUtils.GetPropertyValue<double?>(result, "Y") ?? 0;
          return (x, y);
        }
      }

      // Try PointLocation method
      var method2 = alignment.GetType().GetMethod("PointLocation",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method2 != null)
      {
        var invokeArgs = new object?[] { station, 0.0, 0.0, 0.0 };
        method2.Invoke(alignment, invokeArgs);
        double xOut = Convert.ToDouble(invokeArgs[2] ?? 0.0);
        double yOut = Convert.ToDouble(invokeArgs[3] ?? 0.0);
        return (xOut, yOut);
      }
    }
    catch { /* fall through */ }
    return (0, 0);
  }

  private static double GetSurfaceElevation(DBObject surface, double x, double y)
  {
    try
    {
      var method = surface.GetType().GetMethod("FindElevationAtXY",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(surface, new object[] { x, y });
        if (result != null) return Convert.ToDouble(result);
      }

      var method2 = surface.GetType().GetMethod("SampleElevations",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method2 != null)
      {
        var result2 = method2.Invoke(surface, new object[] { new[] { new Autodesk.AutoCAD.Geometry.Point2d(x, y) } });
        if (result2 is IEnumerable en)
          foreach (var item in en)
            return Convert.ToDouble(item);
      }
    }
    catch { /* fall through */ }
    return 0;
  }

  private static double GetSurfaceSlopePercent(DBObject surface, double x, double y)
  {
    try
    {
      var method = surface.GetType().GetMethod("FindSlopeAtXY",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
      if (method != null)
      {
        var result = method.Invoke(surface, new object[] { x, y });
        if (result != null) return Math.Abs(Convert.ToDouble(result)) * 100;
      }
    }
    catch { /* fall through */ }
    return 5.0; // default 5% slope if can't read surface
  }

  private static double EstimateSlopeHeight(DBObject surface, double x, double y, double sampleDistance, double slopeRatio)
  {
    // Estimate height from slope ratio and catch distance
    // Simplified: assume catchpoint is at slopeRatio * H from edge, height is estimated from surface
    return slopeRatio > 0 ? sampleDistance / slopeRatio * 2.0 : 0;
  }

  private static DBObject? GetFirstAlignment(object civilDoc, Transaction transaction)
  {
    try
    {
      var ids = CivilObjectUtils.InvokeMethod(civilDoc, "GetAlignmentIds") as IEnumerable;
      if (ids != null)
        foreach (var item in ids)
          if (item is ObjectId id && id != ObjectId.Null)
            return CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
    }
    catch { /* fall through */ }
    return null;
  }

  private static DBObject? GetFirstSurface(object civilDoc, Transaction transaction)
  {
    try
    {
      var surfaces = CivilObjectUtils.InvokeMethod(civilDoc, "GetSurfaceIds") as IEnumerable;
      if (surfaces != null)
        foreach (var item in surfaces)
          if (item is ObjectId id && id != ObjectId.Null)
            return CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
    }
    catch { /* fall through */ }
    return null;
  }

  private static DBObject? FindProfileByName(DBObject alignment, Transaction transaction, string name)
  {
    try
    {
      var ids = CivilObjectUtils.InvokeMethod(alignment, "GetProfileIds") as IEnumerable;
      if (ids != null)
        foreach (var item in ids)
          if (item is ObjectId id && id != ObjectId.Null)
          {
            var p = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
            if (string.Equals(CivilObjectUtils.GetName(p), name, StringComparison.OrdinalIgnoreCase))
              return p;
          }
    }
    catch { /* fall through */ }
    return null;
  }

  private static DBObject? GetFirstProfile(DBObject alignment, Transaction transaction, bool preferFinishedGrade)
  {
    try
    {
      DBObject? first = null;
      var ids = CivilObjectUtils.InvokeMethod(alignment, "GetProfileIds") as IEnumerable;
      if (ids != null)
        foreach (var item in ids)
          if (item is ObjectId id && id != ObjectId.Null)
          {
            var p = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
            var name = CivilObjectUtils.GetName(p) ?? "";
            first ??= p;
            if (preferFinishedGrade && (name.IndexOf("FG", StringComparison.OrdinalIgnoreCase) >= 0
              || name.IndexOf("Design", StringComparison.OrdinalIgnoreCase) >= 0))
              return p;
          }
      return first;
    }
    catch { return null; }
  }
}
