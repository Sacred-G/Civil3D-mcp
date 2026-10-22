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

      // Typed overload: none of the name-first signatures exist on Civil 3D 2027, so the old
      // reflection lookup always came back null. The view takes the command-settings defaults for
      // style and band set; explicit ones are applied afterwards.
      var pvId = ProfileView.Create(alignment.ObjectId, insertionPoint);
      if (pvId.IsNull)
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "ProfileView.Create returned a null id.");
      }

      var profileView = CivilObjectUtils.GetRequiredObject<ProfileView>(
        transaction, pvId, OpenMode.ForWrite);
      profileView.Name = profileViewName;

      var styleName = PluginRuntime.GetOptionalString(parameters, "style");
      if (!string.IsNullOrWhiteSpace(styleName))
      {
        var styleId = LookupUtils.GetProfileViewStyleId(civilDoc, transaction, styleName);
        if (!styleId.IsNull)
        {
          profileView.StyleId = styleId;
        }
      }

      var bandSetName = PluginRuntime.GetOptionalString(parameters, "bandSet");
      if (!string.IsNullOrWhiteSpace(bandSetName))
      {
        var bandSetId = LookupUtils.GetProfileViewBandSetId(civilDoc, transaction, bandSetName);
        if (!bandSetId.IsNull)
        {
          profileView.Bands.ImportBandSetStyle(bandSetId);
        }
      }

      // Optional user-specified ranges (e.g. STA -0+20..4+40, elev 0..12) so pipes well below the
      // ground stay inside the grid; otherwise Civil 3D fits the view to the profiles automatically.
      var stationStart = PluginRuntime.GetOptionalDouble(parameters, "stationStart");
      var stationEnd = PluginRuntime.GetOptionalDouble(parameters, "stationEnd");
      if (stationStart.HasValue && stationEnd.HasValue)
      {
        profileView.StationRangeMode = StationRangeType.UserSpecified;
        profileView.StationStart = stationStart.Value;
        profileView.StationEnd = stationEnd.Value;
      }

      var elevationMin = PluginRuntime.GetOptionalDouble(parameters, "elevationMin");
      var elevationMax = PluginRuntime.GetOptionalDouble(parameters, "elevationMax");
      if (elevationMin.HasValue && elevationMax.HasValue)
      {
        profileView.ElevationRangeMode = ElevationRangeType.UserSpecified;
        profileView.ElevationMin = elevationMin.Value;
        profileView.ElevationMax = elevationMax.Value;
      }

      // Changing the ranges shifts the grid away from the insertion point (a -0+20 start moved it
      // 80 ft on Civil 3D 2027); Location is the grid's lower-left corner, so put it back.
      if (stationStart.HasValue || elevationMin.HasValue)
      {
        profileView.Location = insertionPoint;
      }

      return new Dictionary<string, object?>
      {
        ["profileViewName"] = profileView.Name,
        ["handle"] = CivilObjectUtils.GetHandle(profileView),
        ["alignmentName"] = alignment.Name,
        ["insertX"] = insertX,
        ["insertY"] = insertY,
        ["stationStart"] = profileView.StationStart,
        ["stationEnd"] = profileView.StationEnd,
        ["elevationMin"] = profileView.ElevationMin,
        ["elevationMax"] = profileView.ElevationMax,
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

  // ─── profileViewInfo / profileViewSetLocation ─────────────────────────────

  // Station/elevation <-> model XY for a profile view, so sheet annotations (leaders, dims, crossing
  // symbols) can be placed from design values instead of hand-measured coordinates.
  // Without a name it lists every profile view in model space with the same placement summary.
  public static Task<object?> ProfileViewInfoAsync(JsonObject? parameters)
  {
    var profileViewName = PluginRuntime.GetOptionalString(parameters, "profileViewName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      if (string.IsNullOrWhiteSpace(profileViewName))
      {
        var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
        var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        var views = new List<Dictionary<string, object?>>();
        foreach (ObjectId objectId in modelSpace)
        {
          if (transaction.GetObject(objectId, OpenMode.ForRead) is ProfileView view)
          {
            views.Add(DescribeProfileView(view, transaction));
          }
        }

        return new Dictionary<string, object?> { ["profileViews"] = views };
      }

      var profileView = FindProfileViewByName(civilDoc, transaction, profileViewName);
      var result = DescribeProfileView(profileView, transaction);

      var points = new List<Dictionary<string, object?>>();
      if (PluginRuntime.GetParameter(parameters, "points") is JsonArray stationPoints)
      {
        foreach (var node in stationPoints.OfType<JsonObject>())
        {
          var station = PluginRuntime.GetRequiredDouble(node, "station");
          var elevation = PluginRuntime.GetRequiredDouble(node, "elevation");
          var xy = ToXY(profileView, station, elevation);
          points.Add(new Dictionary<string, object?> { ["station"] = station, ["elevation"] = elevation, ["x"] = xy.X, ["y"] = xy.Y });
        }
      }

      var xyPoints = new List<Dictionary<string, object?>>();
      if (PluginRuntime.GetParameter(parameters, "xyPoints") is JsonArray modelPoints)
      {
        foreach (var node in modelPoints.OfType<JsonObject>())
        {
          var x = PluginRuntime.GetRequiredDouble(node, "x");
          var y = PluginRuntime.GetRequiredDouble(node, "y");
          double station = 0, elevation = 0;
          var inside = profileView.FindStationAndElevationAtXY(x, y, ref station, ref elevation);
          xyPoints.Add(new Dictionary<string, object?> { ["x"] = x, ["y"] = y, ["station"] = station, ["elevation"] = elevation, ["insideView"] = inside });
        }
      }

      result["points"] = points;
      result["xyPoints"] = xyPoints;
      return result;
    });
  }

  private static Dictionary<string, object?> DescribeProfileView(ProfileView profileView, Transaction transaction)
  {
    var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, profileView.AlignmentId, OpenMode.ForRead);
    var origin = ToXY(profileView, profileView.StationStart, profileView.ElevationMin);
    var unitStation = ToXY(profileView, profileView.StationStart + 1d, profileView.ElevationMin);
    var unitElevation = ToXY(profileView, profileView.StationStart, profileView.ElevationMin + 1d);
    var station0 = ToXY(profileView, 0d, 0d);

    return new Dictionary<string, object?>
    {
      ["profileViewName"] = profileView.Name,
      ["handle"] = profileView.Handle.ToString(),
      ["alignmentName"] = alignment.Name,
      ["location"] = new Dictionary<string, object?> { ["x"] = profileView.Location.X, ["y"] = profileView.Location.Y },
      ["gridOrigin"] = new Dictionary<string, object?> { ["x"] = origin.X, ["y"] = origin.Y },
      ["station0Elevation0"] = new Dictionary<string, object?> { ["x"] = station0.X, ["y"] = station0.Y },
      ["stationStart"] = profileView.StationStart,
      ["stationEnd"] = profileView.StationEnd,
      ["elevationMin"] = profileView.ElevationMin,
      ["elevationMax"] = profileView.ElevationMax,
      ["xPerStation"] = unitStation.X - origin.X,
      ["yPerElevation"] = unitElevation.Y - origin.Y,
    };
  }

  // Moves a profile view so that (anchorStation, anchorElevation) lands on (targetX, targetY), e.g. to
  // line a rebuilt view up with an existing sheet's viewports and annotations. Parts drawn in the view
  // move with it.
  public static Task<object?> ProfileViewSetLocationAsync(JsonObject? parameters)
  {
    var profileViewName = PluginRuntime.GetRequiredString(parameters, "profileViewName");
    var anchorStation = PluginRuntime.GetRequiredDouble(parameters, "anchorStation");
    var anchorElevation = PluginRuntime.GetRequiredDouble(parameters, "anchorElevation");
    var targetX = PluginRuntime.GetRequiredDouble(parameters, "targetX");
    var targetY = PluginRuntime.GetRequiredDouble(parameters, "targetY");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var found = FindProfileViewByName(civilDoc, transaction, profileViewName);
      var profileView = CivilObjectUtils.GetRequiredObject<ProfileView>(transaction, found.ObjectId, OpenMode.ForWrite);
      var before = ToXY(profileView, anchorStation, anchorElevation);
      var oldLocation = profileView.Location;
      profileView.Location = new Point3d(oldLocation.X + targetX - before.X, oldLocation.Y + targetY - before.Y, oldLocation.Z);
      var after = ToXY(profileView, anchorStation, anchorElevation);

      return new Dictionary<string, object?>
      {
        ["profileViewName"] = profileView.Name,
        ["moved"] = new Dictionary<string, object?> { ["dx"] = targetX - before.X, ["dy"] = targetY - before.Y },
        ["location"] = new Dictionary<string, object?> { ["x"] = profileView.Location.X, ["y"] = profileView.Location.Y },
        ["anchor"] = new Dictionary<string, object?> { ["x"] = after.X, ["y"] = after.Y },
      };
    });
  }

  private static Point2d ToXY(ProfileView profileView, double station, double elevation)
  {
    double x = 0, y = 0;
    profileView.FindXYAtStationAndElevation(station, elevation, ref x, ref y);
    return new Point2d(x, y);
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
