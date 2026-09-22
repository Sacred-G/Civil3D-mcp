using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Editing commands for Civil 3D vertical profiles and profile views:
/// add_pvi, delete_pvi, add_curve, set_grade, get_elevation,
/// check_k_values, profile_view_create, profile_view_band_set.
/// </summary>
public static class ProfileEditCommands
{
  // ─── profileAddPvi ────────────────────────────────────────────────────────

  public static Task<object?> AddPviAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var station = PluginRuntime.GetRequiredDouble(parameters, "station");
    var elevation = PluginRuntime.GetRequiredDouble(parameters, "elevation");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForWrite);

      profile.PVIs.AddPVI(station, elevation);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["station"] = station,
        ["elevation"] = elevation,
        ["success"] = true,
      };
    });
  }

  // ─── profileDeletePvi ─────────────────────────────────────────────────────

  public static Task<object?> DeletePviAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var station = PluginRuntime.GetRequiredDouble(parameters, "station");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForWrite);

      var targetPvi = FindPviNearStation(profile.PVIs, station)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No PVI found near station {station} in profile '{profileName}'.");
      profile.PVIs.RemoveAt(targetPvi.RawStation, targetPvi.Elevation);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["station"] = station,
        ["success"] = true,
      };
    });
  }

  // ─── profileAddCurve ──────────────────────────────────────────────────────

  public static Task<object?> AddCurveAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var pviStation = PluginRuntime.GetRequiredDouble(parameters, "pviStation");
    var length = PluginRuntime.GetRequiredDouble(parameters, "length");
    var curveType = PluginRuntime.GetOptionalString(parameters, "curveType") ?? "symmetric_parabola";

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForWrite);

      if (!string.Equals(curveType, "symmetric_parabola", StringComparison.OrdinalIgnoreCase))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Curve type '{curveType}' is not implemented. Civil 3D's typed API path currently supports symmetric_parabola only.");
      }

      var targetPvi = FindPviNearStation(profile.PVIs, pviStation);
      if (targetPvi == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.OBJECT_NOT_FOUND",
          $"No PVI found near station {pviStation} in profile '{profileName}'.");
      }

      profile.Entities.AddFreeSymmetricParabolaByPVIAndCurveLength(targetPvi, length);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["pviStation"] = pviStation,
        ["curveLength"] = length,
        ["curveType"] = curveType,
        ["success"] = true,
      };
    });
  }

  // ─── profileSetGrade ──────────────────────────────────────────────────────

  public static Task<object?> SetGradeAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var entityIndex = (int)(PluginRuntime.GetRequiredDouble(parameters, "entityIndex"));
    var grade = PluginRuntime.GetRequiredDouble(parameters, "grade");

    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      $"Cannot set grade {grade} on profile entity {entityIndex} in '{profileName}': ProfileTangent.Grade is read-only in the Civil 3D 2026 .NET API. " +
      "Edit the adjoining PVIs instead.");
  }

  // ─── profileGetElevation ──────────────────────────────────────────────────

  /// <summary>
  /// Delegates to the same underlying implementation as
  /// ProfileCommands.GetProfileElevationAsync but is exposed as a
  /// dedicated tool per JFS-10 requirements.
  /// </summary>
  public static Task<object?> GetElevationAsync(JsonObject? parameters)
  {
    return ProfileCommands.GetProfileElevationAsync(parameters);
  }

  // ─── profileCheckKValues ──────────────────────────────────────────────────

  public static Task<object?> CheckKValuesAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);

      var entities = CivilObjectUtils.GetPropertyValue<object>(profile, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Profile '{profileName}' does not expose an Entities collection.");
      }

      // AASHTO minimum K values table (metric km/h → K_sag, K_crest)
      // Source: AASHTO Green Book 2011 Table 3-36 / 3-37
      var kTable = BuildAashtoKTable();
      var (kSagMin, kCrestMin) = LookupKValues(kTable, designSpeed);

      var results = new List<Dictionary<string, object?>>();
      var index = 0;
      foreach (var entity in (System.Collections.IEnumerable)entities)
      {
        var entityType = entity?.GetType().Name ?? string.Empty;
        var isCurve = entityType.ToLowerInvariant().Contains("parabola")
          || entityType.ToLowerInvariant().Contains("curve");
        if (!isCurve)
        {
          index++;
          continue;
        }

        var curveLength = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0;
        var gradeIn = CivilObjectUtils.GetPropertyValue<double?>(entity, "GradeIn")
          ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "StartGrade") ?? 0;
        var gradeOut = CivilObjectUtils.GetPropertyValue<double?>(entity, "GradeOut")
          ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "EndGrade") ?? 0;
        var algebraicDiff = Math.Abs(gradeOut - gradeIn);
        var kValue = algebraicDiff > 1e-10 ? curveLength / algebraicDiff : double.PositiveInfinity;

        var isSag = gradeOut > gradeIn;
        var requiredK = isSag ? kSagMin : kCrestMin;
        var passes = kValue >= requiredK || double.IsPositiveInfinity(kValue);

        results.Add(new Dictionary<string, object?>
        {
          ["entityIndex"] = index,
          ["curveType"] = isSag ? "sag" : "crest",
          ["curveLength"] = curveLength,
          ["gradeIn"] = gradeIn,
          ["gradeOut"] = gradeOut,
          ["algebraicDifference"] = algebraicDiff,
          ["kValue"] = double.IsPositiveInfinity(kValue) ? null : (object?)kValue,
          ["requiredK"] = requiredK,
          ["passes"] = passes,
        });
        index++;
      }

      var allPass = results.All(r => (bool)(r["passes"] ?? false));
      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["designSpeed"] = designSpeed,
        ["kSagMinimum"] = kSagMin,
        ["kCrestMinimum"] = kCrestMin,
        ["curves"] = results,
        ["allPass"] = allPass,
        ["summary"] = allPass
          ? $"All {results.Count} vertical curve(s) meet minimum K values for {designSpeed} design speed."
          : $"{results.Count(r => !(bool)(r["passes"] ?? false))} of {results.Count} curve(s) fail minimum K value requirements.",
      };
    });
  }

  // ─── profileViewCreate ────────────────────────────────────────────────────

  public static Task<object?> ProfileViewCreateAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileViewName = PluginRuntime.GetRequiredString(parameters, "profileViewName");
    var insertX = PluginRuntime.GetRequiredDouble(parameters, "insertX");
    var insertY = PluginRuntime.GetRequiredDouble(parameters, "insertY");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var insertionPoint = new Point3d(insertX, insertY, 0);

      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(
        transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(
        transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      var styleId = LookupUtils.GetProfileViewStyleId(
        civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var bandSetId = LookupUtils.GetProfileViewBandSetId(
        civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "bandSet"));

      // Every ProfileView.Create overload begins with either the alignment's
      // ObjectId or a CivilDocument -- none of them takes the view name first.
      // The previous reflection attempt probed three name-first signatures, so
      // it never matched anything and always reported "returned null". Several
      // of the real overloads also return ObjectIdCollection rather than
      // ObjectId, which a blind (ObjectId?) cast would have discarded anyway.
      //
      // Call the typed API instead: the five-argument form when we resolved
      // both a style and a band set, otherwise the minimal form plus a rename,
      // with whichever of the two was supplied applied on its own.
      var both = !styleId.IsNull && !bandSetId.IsNull;
      var pvId = both
        ? ProfileView.Create(alignment.ObjectId, insertionPoint, profileViewName, bandSetId, styleId)
        : ProfileView.Create(alignment.ObjectId, insertionPoint);

      if (pvId.IsNull)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"ProfileView.Create did not produce a profile view for alignment '{alignmentName}'.");
      }

      var profileView = CivilObjectUtils.GetRequiredObject<ProfileView>(
        transaction, pvId, both ? OpenMode.ForRead : OpenMode.ForWrite);
      if (!both)
      {
        profileView.Name = profileViewName;
        if (!styleId.IsNull)
        {
          profileView.StyleId = styleId;
        }
        if (!bandSetId.IsNull)
        {
          profileView.Bands.ImportBandSetStyle(bandSetId);
        }
      }

      return new Dictionary<string, object?>
      {
        ["profileViewName"] = profileView.Name,
        ["handle"] = CivilObjectUtils.GetHandle(profileView),
        ["alignmentName"] = alignment.Name,
        ["insertX"] = insertX,
        ["insertY"] = insertY,
        ["success"] = true,
      };
    });
  }

  // ─── profileViewBandSet ───────────────────────────────────────────────────

  public static Task<object?> ProfileViewBandSetAsync(JsonObject? parameters)
  {
    var profileViewName = PluginRuntime.GetRequiredString(parameters, "profileViewName");
    var bandSetName = PluginRuntime.GetRequiredString(parameters, "bandSetName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var profileView = FindProfileViewByName(civilDoc, transaction, profileViewName);
      var writeView = CivilObjectUtils.GetRequiredObject<ProfileView>(
        transaction, profileView.ObjectId, OpenMode.ForWrite);

      var bandSetId = LookupUtils.GetProfileViewBandSetId(civilDoc, transaction, bandSetName);

      writeView.Bands.ImportBandSetStyle(bandSetId);

      return new Dictionary<string, object?>
      {
        ["profileViewName"] = profileView.Name,
        ["bandSetName"] = bandSetName,
        ["success"] = true,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────

  private static ProfileView FindProfileViewByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    // Profile views live in model space; enumerate all ProfileView objects
    var database = CivilObjectUtils.GetDatabase(civilDoc);
    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(
      transaction, database.BlockTableId, OpenMode.ForRead);
    var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(
      transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

    foreach (ObjectId objectId in modelSpace)
    {
      var obj = transaction.GetObject(objectId, OpenMode.ForRead);
      if (obj is ProfileView pv
        && string.Equals(pv.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return pv;
      }
    }

    throw new JsonRpcDispatchException(
      "CIVIL3D.OBJECT_NOT_FOUND",
      $"Profile view '{name}' was not found in model space.");
  }

  private static ProfilePVI? FindPviNearStation(ProfilePVICollection pvis, double targetStation)
  {
    ProfilePVI? closest = null;
    var minDist = double.MaxValue;

    foreach (ProfilePVI pvi in pvis)
    {
      var dist = Math.Abs(pvi.RawStation - targetStation);
      if (dist < minDist)
      {
        minDist = dist;
        closest = pvi;
      }
    }

    return closest;
  }

  /// <summary>
  /// AASHTO minimum K values (metric, km/h).
  /// Returns (K_sag_min, K_crest_min).
  /// Source: AASHTO A Policy on Geometric Design of Highways and Streets, 2011.
  /// </summary>
  private static List<(double speed, double kSag, double kCrest)> BuildAashtoKTable() =>
  [
    (30, 3, 1),
    (40, 7, 2),
    (50, 9, 4),
    (60, 11, 6),
    (70, 14, 10),
    (80, 19, 17),
    (90, 24, 29),
    (100, 30, 44),
    (110, 37, 60),
    (120, 46, 84),
    (130, 57, 114),
  ];

  private static (double kSag, double kCrest) LookupKValues(
    List<(double speed, double kSag, double kCrest)> table,
    double designSpeed)
  {
    // Find exact match first
    var exact = table.FirstOrDefault(t => Math.Abs(t.speed - designSpeed) < 0.5);
    if (exact != default)
    {
      return (exact.kSag, exact.kCrest);
    }

    // Interpolate between nearest values
    var lower = table.LastOrDefault(t => t.speed <= designSpeed);
    var upper = table.FirstOrDefault(t => t.speed > designSpeed);

    if (lower == default)
    {
      return (table[0].kSag, table[0].kCrest);
    }

    if (upper == default)
    {
      return (table[^1].kSag, table[^1].kCrest);
    }

    var ratio = (designSpeed - lower.speed) / (upper.speed - lower.speed);
    return (
      lower.kSag + ratio * (upper.kSag - lower.kSag),
      lower.kCrest + ratio * (upper.kCrest - lower.kCrest));
  }
}
