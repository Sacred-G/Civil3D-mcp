using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// AASHTO sight distance calculation commands.
/// Implements stopping (SSD), passing (PSD), and decision (DSD) sight distance
/// per AASHTO Green Book formulas. Can optionally check against alignment K-values.
/// </summary>
public static class SightDistanceCommands
{
  // ─── AASHTO constants ────────────────────────────────────────────────────────

  // AASHTO Table 3-1: friction coefficients by design speed (km/h)
  private static readonly (double SpeedKmh, double f)[] AashtoFriction =
  [
    (30, 0.40), (40, 0.38), (50, 0.35), (60, 0.33),
    (70, 0.31), (80, 0.30), (90, 0.30), (100, 0.29),
    (110, 0.28), (120, 0.28), (130, 0.27),
  ];

  // AASHTO minimum K-values for crest curves by design speed (km/h)
  private static readonly (double SpeedKmh, double Kcrest, double Ksag)[] AashtoKValues =
  [
    (30, 1, 3), (40, 2, 6), (50, 4, 9), (60, 7, 12),
    (70, 12, 17), (80, 19, 23), (90, 30, 30), (100, 43, 38),
    (110, 60, 45), (120, 79, 55), (130, 104, 63),
  ];

  // ─── calculateSightDistance ──────────────────────────────────────────────────

  public static Task<object?> CalculateSightDistanceAsync(JsonObject? parameters)
  {
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");
    var speedUnits = PluginRuntime.GetOptionalString(parameters, "speedUnits") ?? "kmh";
    var sightType = PluginRuntime.GetOptionalString(parameters, "sightDistanceType") ?? "stopping";
    var grade = PluginRuntime.GetOptionalDouble(parameters, "grade") ?? 0.0;
    var prt = PluginRuntime.GetOptionalDouble(parameters, "perceptionReactionTime") ?? 2.5;
    var userFriction = PluginRuntime.GetOptionalDouble(parameters, "frictionCoefficient");
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetOptionalString(parameters, "profileName");
    var checkStation = PluginRuntime.GetOptionalDouble(parameters, "checkStation");

    // Normalise to km/h
    var speedKmh = string.Equals(speedUnits, "mph", StringComparison.OrdinalIgnoreCase)
      ? designSpeed * 1.60934
      : designSpeed;

    var friction = userFriction ?? InterpolateFriction(speedKmh);
    var gradeDecimal = grade / 100.0;

    // ── SSD (AASHTO Green Book Equation 3-1) ────────────────────────────────
    // SSD = 0.278 * V * t + V² / (254 * (f ± g))    [result in metres]
    var ssdMetres = 0.278 * speedKmh * prt + (speedKmh * speedKmh) / (254.0 * (friction + gradeDecimal));
    var ssdFeet = ssdMetres * 3.28084;

    // Min K-value for crest curve (AASHTO):  K = L / A,  SSD ≈ √(SSD² / 658) for H1=1.08m, H2=0.60m
    var (kcrest, ksag) = InterpolateKValues(speedKmh);

    double requiredSightDistance;
    string sightTypeLabel;
    switch (sightType.ToLowerInvariant())
    {
      case "passing":
      case "psd":
        // PSD ≈ 6 * SSD (rough rule of thumb; AASHTO Table 3-4)
        requiredSightDistance = ssdMetres * 6.0;
        sightTypeLabel = "Passing Sight Distance (PSD)";
        break;
      case "decision":
      case "dsd":
        // DSD ≈ 3.5 * SSD for complex situations (AASHTO Table 3-3)
        requiredSightDistance = ssdMetres * 3.5;
        sightTypeLabel = "Decision Sight Distance (DSD)";
        break;
      default:
        requiredSightDistance = ssdMetres;
        sightTypeLabel = "Stopping Sight Distance (SSD)";
        break;
    }

    var result = new Dictionary<string, object?>
    {
      ["designSpeedKmh"] = Math.Round(speedKmh, 1),
      ["designSpeedMph"] = Math.Round(speedKmh / 1.60934, 1),
      ["sightDistanceType"] = sightTypeLabel,
      ["grade"] = grade,
      ["frictionCoefficient"] = Math.Round(friction, 3),
      ["perceptionReactionTime"] = prt,
      ["requiredSightDistanceMetres"] = Math.Round(requiredSightDistance, 1),
      ["requiredSightDistanceFeet"] = Math.Round(requiredSightDistance * 3.28084, 1),
      ["minimumKValueCrest"] = kcrest,
      ["minimumKValueSag"] = ksag,
      ["standard"] = "AASHTO Green Book",
    };

    // If alignment + profile provided, check at the requested station
    if (!string.IsNullOrWhiteSpace(alignmentName) && !string.IsNullOrWhiteSpace(profileName) && checkStation.HasValue)
    {
      return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
      {
        try
        {
          var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName!);
          var profileId = FindProfileId(alignment, transaction, profileName!);
          double? kValueAtStation = profileId != ObjectId.Null
            ? GetKValueAtStation(transaction, profileId, checkStation.Value)
            : null;

          result["stationChecked"] = checkStation;
          result["kValueAtStation"] = kValueAtStation.HasValue ? Math.Round(kValueAtStation.Value, 1) : (object?)"N/A";
          result["kValueCompliant"] = kValueAtStation.HasValue && kValueAtStation.Value >= kcrest;
        }
        catch
        {
          result["stationCheckError"] = "Could not access profile K-values at requested station.";
        }
        return (object?)result;
      });
    }

    return Task.FromResult<object?>(result);
  }

  // ─── checkStoppingDistance ───────────────────────────────────────────────────

  public static Task<object?> CheckStoppingDistanceAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");
    var stationStart = PluginRuntime.GetOptionalDouble(parameters, "stationStart");
    var stationEnd = PluginRuntime.GetOptionalDouble(parameters, "stationEnd");
    var stationInterval = PluginRuntime.GetOptionalDouble(parameters, "stationInterval") ?? 25.0;

    var speedKmh = designSpeed;
    var friction = InterpolateFriction(speedKmh);
    var ssdMetres = 0.278 * speedKmh * 2.5 + (speedKmh * speedKmh) / (254.0 * friction);
    var (kcrestMin, _) = InterpolateKValues(speedKmh);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var alignmentLength = CivilObjectUtils.GetPropertyValue<double?>(alignment, "Length") ?? 0.0;
      var startSta = stationStart ?? CivilObjectUtils.GetPropertyValue<double?>(alignment, "StartingStation") ?? 0.0;
      var endSta = stationEnd ?? (startSta + alignmentLength);

      var profileId = FindProfileId(alignment, transaction, profileName);
      var violations = new List<Dictionary<string, object?>>();
      var checkedCount = 0;

      for (double sta = startSta; sta <= endSta; sta += stationInterval)
      {
        checkedCount++;
        var kValue = profileId != ObjectId.Null ? GetKValueAtStation(transaction, profileId, sta) : null;
        if (kValue.HasValue && kValue.Value < kcrestMin)
        {
          violations.Add(new Dictionary<string, object?>
          {
            ["station"] = Math.Round(sta, 2),
            ["kValue"] = Math.Round(kValue.Value, 1),
            ["kValueRequired"] = kcrestMin,
            ["deficiency"] = Math.Round(kcrestMin - kValue.Value, 1),
            ["severity"] = kValue.Value < kcrestMin * 0.75 ? "FAIL" : "WARN",
          });
        }
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignmentName,
        ["profileName"] = profileName,
        ["designSpeedKmh"] = speedKmh,
        ["requiredSsdMetres"] = Math.Round(ssdMetres, 1),
        ["minimumKValueCrest"] = kcrestMin,
        ["stationsChecked"] = checkedCount,
        ["violationCount"] = violations.Count,
        ["compliant"] = violations.Count == 0,
        ["violations"] = violations,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  private static double InterpolateFriction(double speedKmh)
  {
    if (speedKmh <= AashtoFriction[0].SpeedKmh) return AashtoFriction[0].f;
    if (speedKmh >= AashtoFriction[^1].SpeedKmh) return AashtoFriction[^1].f;
    for (int i = 0; i < AashtoFriction.Length - 1; i++)
    {
      var (s1, f1) = AashtoFriction[i];
      var (s2, f2) = AashtoFriction[i + 1];
      if (speedKmh >= s1 && speedKmh <= s2)
      {
        double t = (speedKmh - s1) / (s2 - s1);
        return f1 + t * (f2 - f1);
      }
    }
    return 0.35;
  }

  private static (double Kcrest, double Ksag) InterpolateKValues(double speedKmh)
  {
    if (speedKmh <= AashtoKValues[0].SpeedKmh) return (AashtoKValues[0].Kcrest, AashtoKValues[0].Ksag);
    if (speedKmh >= AashtoKValues[^1].SpeedKmh) return (AashtoKValues[^1].Kcrest, AashtoKValues[^1].Ksag);
    for (int i = 0; i < AashtoKValues.Length - 1; i++)
    {
      var (s1, kc1, ks1) = AashtoKValues[i];
      var (s2, kc2, ks2) = AashtoKValues[i + 1];
      if (speedKmh >= s1 && speedKmh <= s2)
      {
        double t = (speedKmh - s1) / (s2 - s1);
        return (kc1 + t * (kc2 - kc1), ks1 + t * (ks2 - ks1));
      }
    }
    return (19, 23);
  }

  private static ObjectId FindProfileId(DBObject alignment, Transaction transaction, string profileName)
  {
    var profileIds = CivilObjectUtils.InvokeMethod(alignment, "GetProfileIds") as IEnumerable;
    if (profileIds == null) return ObjectId.Null;
    foreach (var item in profileIds)
    {
      if (item is ObjectId id && id != ObjectId.Null)
      {
        var profile = transaction.GetObject(id, OpenMode.ForRead, false, true);
        if (string.Equals(CivilObjectUtils.GetName(profile), profileName, StringComparison.OrdinalIgnoreCase))
          return id;
      }
    }
    return ObjectId.Null;
  }

  private static double? GetKValueAtStation(Transaction transaction, ObjectId profileId, double station)
  {
    try
    {
      var profile = transaction.GetObject(profileId, OpenMode.ForRead, false, true);
      var entities = CivilObjectUtils.GetPropertyValue<object>(profile, "Entities");
      if (entities is not IEnumerable enumerable) return null;

      foreach (var entity in enumerable)
      {
        if (entity == null) continue;
        var startSta = CivilObjectUtils.GetPropertyValue<double?>(entity, "StartStation") ?? 0;
        var endSta = CivilObjectUtils.GetPropertyValue<double?>(entity, "EndStation") ?? 0;
        if (station < startSta || station > endSta) continue;

        var entityType = entity.GetType().Name;
        if (entityType.IndexOf("curve", StringComparison.OrdinalIgnoreCase) < 0) continue;

        var kValue = CivilObjectUtils.GetPropertyValue<double?>(entity, "K")
                  ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "KValue");
        if (kValue.HasValue && kValue.Value > 0) return kValue.Value;

        // Calculate K from length and grade change
        var length = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0;
        var gradeIn = CivilObjectUtils.GetPropertyValue<double?>(entity, "GradeIn")
                   ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "StartGrade") ?? 0;
        var gradeOut = CivilObjectUtils.GetPropertyValue<double?>(entity, "GradeOut")
                    ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "EndGrade") ?? 0;
        var a = Math.Abs(gradeOut - gradeIn);
        if (a > 0.001 && length > 0) return length / a;
      }
      return null;
    }
    catch { return null; }
  }
}
