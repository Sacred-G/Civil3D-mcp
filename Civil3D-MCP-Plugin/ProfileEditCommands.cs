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

  /// <summary>The drawing's length unit from INSUNITS, else the Civil 3D drawing units; null when neither is known.</summary>
  private static string? ResolveDrawingLengthUnit(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Database database)
  {
    string? civilDrawingUnits = null;
    string? imperialToMetric = null;
    try
    {
      var unitZone = civilDoc.Settings.DrawingSettings.UnitZoneSettings;
      civilDrawingUnits = unitZone.DrawingUnits.ToString();
      imperialToMetric = unitZone.ImperialToMetricConversion.ToString();
    }
    catch (System.Exception)
    {
      // Fall back to INSUNITS alone.
    }

    return VerticalCurveMath.ResolveLengthUnit(database.Insunits.ToString(), civilDrawingUnits, imperialToMetric);
  }

  public static Task<object?> CheckKValuesAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");
    string? requestedSpeedUnits;
    try
    {
      requestedSpeedUnits = VerticalCurveMath.NormalizeSpeedUnits(PluginRuntime.GetOptionalString(parameters, "speedUnits"));
    }
    catch (ArgumentException ex)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", ex.Message);
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);

      var warnings = new List<string>();
      var lengthUnit = ResolveDrawingLengthUnit(civilDoc, database);
      var metersPerUnit = VerticalCurveMath.MetersPerUnit(lengthUnit);
      var speedUnits = requestedSpeedUnits ?? VerticalCurveMath.DefaultSpeedUnitsForLengthUnit(lengthUnit);
      var speedUnitsSource = requestedSpeedUnits != null ? "parameter" : "drawingUnits";
      if (speedUnits == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"The drawing's length unit ('{lengthUnit ?? "unknown"}') does not imply a speed unit; pass speedUnits \"mph\" or \"km/h\".");
      }

      if (metersPerUnit == null)
      {
        warnings.Add($"The drawing's length unit is unknown; curve lengths were taken to be in {(speedUnits == VerticalCurveMath.Mph ? "feet" : "meters")}.");
      }

      VerticalCurveMath.KLookup lookup;
      try
      {
        lookup = VerticalCurveMath.LookupRow(speedUnits, designSpeed);
      }
      catch (ArgumentException ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", ex.Message.Split(Environment.NewLine)[0]);
      }

      if (lookup.Note != null)
      {
        warnings.Add(lookup.Note);
      }

      var results = new List<Dictionary<string, object?>>();
      var index = 0;
      foreach (ProfileEntity entity in profile.Entities)
      {
        var currentIndex = index++;
        double gradeIn, gradeOut, pviStation;
        string entityType;
        switch (entity)
        {
          case ProfileParabolaSymmetric symmetric:
            (gradeIn, gradeOut, pviStation, entityType) = (symmetric.GradeIn, symmetric.GradeOut, symmetric.PVIStation, "symmetric_parabola");
            break;
          case ProfileParabolaAsymmetric asymmetric:
            (gradeIn, gradeOut, pviStation, entityType) = (asymmetric.GradeIn, asymmetric.GradeOut, asymmetric.PVIStation, "asymmetric_parabola");
            break;
          case ProfileCircular circular:
            (gradeIn, gradeOut, pviStation, entityType) = (circular.GradeIn, circular.GradeOut, circular.PVIStation, "circular_curve");
            break;
          default:
            continue;
        }

        var curveLength = entity.Length;
        var tableLength = VerticalCurveMath.LengthToTableUnits(curveLength, metersPerUnit, speedUnits);
        var aPercent = VerticalCurveMath.GradeDifferencePercent(gradeIn, gradeOut);
        var kValue = VerticalCurveMath.ComputeK(tableLength, gradeIn, gradeOut);
        var isSag = VerticalCurveMath.IsSag(gradeIn, gradeOut);
        var requiredK = isSag ? lookup.Row.KSag : lookup.Row.KCrest;
        var passes = kValue == null || kValue.Value >= requiredK;

        results.Add(new Dictionary<string, object?>
        {
          ["entityIndex"] = currentIndex,
          ["entityType"] = entityType,
          ["curveType"] = isSag ? "sag" : "crest",
          ["startStation"] = entity.StartStation,
          ["endStation"] = entity.EndStation,
          ["pviStation"] = pviStation,
          ["curveLength"] = curveLength,
          ["gradeIn"] = gradeIn,
          ["gradeOut"] = gradeOut,
          ["gradeInPercent"] = gradeIn * 100.0,
          ["gradeOutPercent"] = gradeOut * 100.0,
          ["algebraicDifferencePercent"] = aPercent,
          ["kValue"] = kValue,
          ["requiredK"] = requiredK,
          ["passes"] = passes,
        });
      }

      var failing = results.Count(r => !(bool)(r["passes"] ?? false));
      // No curves means nothing was checked, which is not a pass.
      var allPass = results.Count > 0 && failing == 0;
      if (results.Count == 0)
      {
        warnings.Add($"Profile '{profile.Name}' has no vertical curves, so no K values were checked.");
      }
      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["designSpeed"] = designSpeed,
        ["speedUnits"] = speedUnits,
        ["speedUnitsSource"] = speedUnitsSource,
        ["drawingLengthUnit"] = lengthUnit,
        ["kUnits"] = lookup.KUnits,
        ["gradeUnits"] = "gradeIn/gradeOut are decimal ratios; *Percent fields and A are percent; K = L / A",
        ["tableRow"] = new Dictionary<string, object?>
        {
          ["designSpeed"] = lookup.Row.Speed,
          ["kCrest"] = lookup.Row.KCrest,
          ["kSag"] = lookup.Row.KSag,
          ["exactMatch"] = lookup.ExactMatch,
          ["source"] = "AASHTO Green Book stopping sight distance design K (crest Table 3-34, sag Table 3-36)",
        },
        ["kSagMinimum"] = lookup.Row.KSag,
        ["kCrestMinimum"] = lookup.Row.KCrest,
        ["curves"] = results,
        ["allPass"] = allPass,
        ["warnings"] = warnings,
        ["summary"] = results.Count == 0
          ? $"Profile '{profile.Name}' has no vertical curves; no K values were checked."
          : allPass
          ? $"All {results.Count} vertical curve(s) meet the minimum K for {designSpeed} {speedUnits} (table row {lookup.Row.Speed} {speedUnits}: crest {lookup.Row.KCrest}, sag {lookup.Row.KSag} {lookup.KUnits})."
          : $"{failing} of {results.Count} vertical curve(s) are below the minimum K for {designSpeed} {speedUnits} (table row {lookup.Row.Speed} {speedUnits}: crest {lookup.Row.KCrest}, sag {lookup.Row.KSag} {lookup.KUnits}).",
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
    var requestedStyle = PluginRuntime.GetOptionalString(parameters, "style");
    var requestedBandSet = PluginRuntime.GetOptionalString(parameters, "bandSet");
    var requestedLayer = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var insertionPoint = new Point3d(insertX, insertY, 0);

      // Resolve the layer first so an invalid name fails before anything is created.
      var layerId = string.IsNullOrWhiteSpace(requestedLayer)
        ? ObjectId.Null
        : LookupUtils.GetLayerId(database, transaction, requestedLayer);

      // With no name, both lookups use the drawing's first style / band set,
      // so Create never gets a Null id unless the drawing has none at all. A
      // name that does not exist is CIVIL3D.INVALID_INPUT (listing the
      // available names) rather than a silent substitution.
      var styleId = LookupUtils.GetProfileViewStyleId(civilDoc, transaction, requestedStyle);
      var bandSetId = LookupUtils.GetProfileViewBandSetId(civilDoc, transaction, requestedBandSet, fallbackToFirst: true);
      var styleName = NameOf(transaction, styleId);
      var bandSetName = NameOf(transaction, bandSetId);

      var warnings = new List<string>();

      ObjectId pvId;
      var createdWithBoth = !styleId.IsNull && !bandSetId.IsNull;
      if (createdWithBoth)
      {
        // Civil 3D 2027 (verified from AeccDbMgd metadata):
        // static ObjectId Create(ObjectId alignmentId, Point3d insertPosition,
        //   string profileViewName, ObjectId profileViewBandSetId, ObjectId profileViewStyleId)
        try
        {
          pvId = ProfileView.Create(alignment.ObjectId, insertionPoint, profileViewName, bandSetId, styleId);
        }
        catch (System.ArgumentException ex)
        {
          // Civil 3D rejects the name here (for example "Duplicate ProfileView
          // name.") before anything is created.
          throw new JsonRpcDispatchException(
            "CIVIL3D.INVALID_INPUT",
            $"Civil 3D could not create the profile view '{profileViewName}': {ex.Message}");
        }
      }
      else
      {
        // The drawing lacks a style or a band set. Every 2027 Create overload
        // that takes a style also takes a band set (AeccDbMgd metadata), so the
        // view is created with Civil 3D's defaults and whichever of the two the
        // drawing does have is applied to it below.
        pvId = ProfileView.Create(alignment.ObjectId, insertionPoint);
      }

      if (pvId.IsNull)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "ProfileView.Create did not return a profile view.");
      }

      var profileView = CivilObjectUtils.GetRequiredObject<ProfileView>(transaction, pvId, OpenMode.ForWrite);
      if (!createdWithBoth)
      {
        if (!styleId.IsNull)
        {
          ApplyToProfileView(() => profileView.StyleId = styleId, "profile view style", styleName, requestedStyle, warnings);
        }
        if (!bandSetId.IsNull)
        {
          if (!ApplyToProfileView(() => profileView.Bands.ImportBandSetStyle(bandSetId), "profile view band set", bandSetName, requestedBandSet, warnings))
          {
            // Civil 3D's default band set stayed on the view; don't report the
            // drawing's band set as applied.
            bandSetName = null;
          }
        }

        var missing = new List<string>();
        if (styleId.IsNull) missing.Add("profile view style");
        if (bandSetId.IsNull) missing.Add("profile view band set");
        warnings.Add($"The drawing has no {string.Join(" and no ", missing)}; Civil 3D's default {string.Join(" and ", missing)} {(missing.Count == 1 ? "was" : "were")} used.");
      }

      if (!string.Equals(profileView.Name, profileViewName, StringComparison.Ordinal))
      {
        try
        {
          profileView.Name = profileViewName;
        }
        catch (System.Exception ex)
        {
          // A view under another name is not what was asked for; failing here
          // rolls the whole create back.
          throw new JsonRpcDispatchException(
            "CIVIL3D.INVALID_INPUT",
            $"Civil 3D could not name the profile view '{profileViewName}': {ex.Message}");
        }
      }

      if (!layerId.IsNull)
      {
        profileView.LayerId = layerId;
      }

      return new Dictionary<string, object?>
      {
        ["profileViewName"] = profileView.Name,
        ["name"] = profileView.Name,
        ["handle"] = CivilObjectUtils.GetHandle(profileView),
        ["alignmentName"] = alignment.Name,
        ["layer"] = profileView.Layer,
        ["style"] = NameOf(transaction, profileView.StyleId) ?? styleName,
        ["bandSet"] = bandSetName,
        ["insertX"] = insertX,
        ["insertY"] = insertY,
        ["warnings"] = warnings,
        ["success"] = true,
      };
    });
  }

  // Applies a style or band set to a new profile view. One the caller named
  // must be applied or the call fails (CIVIL3D.INVALID_INPUT, which rolls the
  // create back); the drawing's first one, used when no name was given, is
  // reported in a warning when it cannot be applied. Returns whether it was
  // applied.
  private static bool ApplyToProfileView(Action apply, string kind, string? name, string? requestedName, List<string> warnings)
  {
    try
    {
      apply();
      return true;
    }
    catch (System.Exception ex) when (ex is not JsonRpcDispatchException)
    {
      if (!string.IsNullOrWhiteSpace(requestedName))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"The requested {kind} '{name ?? requestedName}' could not be applied to the new profile view: {ex.Message}");
      }

      warnings.Add($"The drawing's {kind} '{name}' could not be applied; Civil 3D's default was used: {ex.Message}");
      return false;
    }
  }

  private static string? NameOf(Transaction transaction, ObjectId objectId)
  {
    if (objectId.IsNull)
    {
      return null;
    }

    return CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead));
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
}
