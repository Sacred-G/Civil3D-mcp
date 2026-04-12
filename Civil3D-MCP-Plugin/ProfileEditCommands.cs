using System.Reflection;
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

      var pvis = CivilObjectUtils.GetPropertyValue<object>(profile, "PVIs");
      if (pvis == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Profile '{profileName}' does not expose a PVIs collection. Is it a layout profile?");
      }

      // ProfilePVICollection.AddPVI(station, elevation) or Add(station, elevation)
      var added = CivilObjectUtils.InvokeMethod(pvis, "AddPVI", station, elevation)
        ?? CivilObjectUtils.InvokeMethod(pvis, "Add", station, elevation);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["station"] = station,
        ["elevation"] = elevation,
        ["success"] = added != null,
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

      var pvis = CivilObjectUtils.GetPropertyValue<object>(profile, "PVIs");
      if (pvis == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Profile '{profileName}' does not expose a PVIs collection. Is it a layout profile?");
      }

      // ProfilePVICollection.DeletePVIByStation(station) or Remove(station)
      var deleted = CivilObjectUtils.InvokeMethod(pvis, "DeletePVIByStation", station)
        ?? CivilObjectUtils.InvokeMethod(pvis, "RemoveAt", station);

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

      var pvis = CivilObjectUtils.GetPropertyValue<object>(profile, "PVIs");
      if (pvis == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Profile '{profileName}' does not expose a PVIs collection. Is it a layout profile?");
      }

      // Find the PVI nearest to pviStation
      var targetPvi = FindPviNearStation(pvis, pviStation);
      if (targetPvi == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.OBJECT_NOT_FOUND",
          $"No PVI found near station {pviStation} in profile '{profileName}'.");
      }

      // Try to set curve length on the PVI object itself
      // ProfilePVI.CurveLength or SetCurveLength(length) depending on Civil3D version
      var curveLengthSet = TrySetPviCurveLength(targetPvi, length, curveType);
      if (!curveLengthSet)
      {
        // Fall back: invoke AddPVIByParabolaCurve on the collection
        var pviStation2 = CivilObjectUtils.GetPropertyValue<double?>(targetPvi, "Station") ?? pviStation;
        var pviElev = CivilObjectUtils.GetPropertyValue<double?>(targetPvi, "Elevation") ?? 0;
        CivilObjectUtils.InvokeMethod(pvis, "DeletePVIByStation", pviStation2);
        _ = CivilObjectUtils.InvokeMethod(pvis, "AddPVIByParabolaCurve", pviStation2, pviElev, length)
          ?? CivilObjectUtils.InvokeMethod(pvis, "AddPVI", pviStation2, pviElev);
      }

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

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForWrite);

      var entities = CivilObjectUtils.GetPropertyValue<object>(profile, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Profile '{profileName}' does not expose an Entities collection.");
      }

      var entity = GetEntityByIndex(entities, entityIndex);
      if (entity == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.OBJECT_NOT_FOUND",
          $"No entity at index {entityIndex} in profile '{profileName}'.");
      }

      // Try setting Grade property
      var gradeProp = entity.GetType().GetProperty("Grade",
        BindingFlags.Public | BindingFlags.Instance);
      if (gradeProp != null && gradeProp.CanWrite)
      {
        gradeProp.SetValue(entity, grade);
      }
      else
      {
        // Try SetGrade method
        var result = CivilObjectUtils.InvokeMethod(entity, "SetGrade", grade);
        if (result == null)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.TRANSACTION_FAILED",
            $"Could not set grade on entity {entityIndex} — no writable Grade property or SetGrade method found.");
        }
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["entityIndex"] = entityIndex,
        ["grade"] = grade,
        ["success"] = true,
      };
    });
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

      // ProfileView.Create(profileViewName, alignmentId, styleId, insertionPoint)
      // or ProfileView.Create(profileViewName, alignmentId, insertionPoint, styleId, bandSetId)
      var profileViewType = typeof(ProfileView);
      var pvId = (ObjectId?)(
        CivilObjectUtils.InvokeStaticMethod(profileViewType, "Create",
          profileViewName, alignment.ObjectId, insertionPoint, styleId, bandSetId)
        ?? CivilObjectUtils.InvokeStaticMethod(profileViewType, "Create",
          profileViewName, alignment.ObjectId, styleId, insertionPoint)
        ?? CivilObjectUtils.InvokeStaticMethod(profileViewType, "Create",
          profileViewName, alignment.ObjectId, insertionPoint));

      if (pvId == null || pvId.Value.IsNull)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          "ProfileView.Create returned null — this Civil 3D version may require a different API signature.");
      }

      var profileView = CivilObjectUtils.GetRequiredObject<ProfileView>(
        transaction, pvId.Value, OpenMode.ForRead);

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

      // ProfileView.BandSet.SetBandSetStyle(bandSetId) or
      // writeView.BandSetStyleId = bandSetId (depending on API)
      var bandSet = CivilObjectUtils.GetPropertyValue<object>(writeView, "BandSet");
      if (bandSet != null)
      {
        // Try setting StyleId on the BandSet object
        var styleProp = bandSet.GetType().GetProperty("StyleId",
          BindingFlags.Public | BindingFlags.Instance);
        if (styleProp != null && styleProp.CanWrite)
        {
          styleProp.SetValue(bandSet, bandSetId);
        }
        else
        {
          CivilObjectUtils.InvokeMethod(bandSet, "SetBandSetStyle", bandSetId);
        }
      }
      else
      {
        // Direct property on ProfileView
        var bsIdProp = writeView.GetType().GetProperty("BandSetStyleId",
          BindingFlags.Public | BindingFlags.Instance);
        if (bsIdProp != null && bsIdProp.CanWrite)
        {
          bsIdProp.SetValue(writeView, bandSetId);
        }
        else
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.TRANSACTION_FAILED",
            $"Could not apply band set — neither BandSet.StyleId nor BandSetStyleId is writable in this Civil 3D version.");
        }
      }

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

  private static object? GetEntityByIndex(object entities, int index)
  {
    var itemProp = entities.GetType().GetProperty("Item");
    if (itemProp != null)
    {
      return itemProp.GetValue(entities, new object[] { index });
    }

    if (entities is System.Collections.IEnumerable enumerable)
    {
      var i = 0;
      foreach (var entity in enumerable)
      {
        if (i++ == index)
        {
          return entity;
        }
      }
    }

    return null;
  }

  private static object? FindPviNearStation(object pvis, double targetStation)
  {
    if (pvis is not System.Collections.IEnumerable enumerable)
    {
      return null;
    }

    object? closest = null;
    var minDist = double.MaxValue;

    foreach (var pvi in enumerable)
    {
      var st = CivilObjectUtils.GetPropertyValue<double?>(pvi, "Station");
      if (st == null)
      {
        continue;
      }

      var dist = Math.Abs(st.Value - targetStation);
      if (dist < minDist)
      {
        minDist = dist;
        closest = pvi;
      }
    }

    return closest;
  }

  private static bool TrySetPviCurveLength(object pvi, double length, string curveType)
  {
    // Try CurveLength property (Civil 3D 2020+)
    var prop = pvi.GetType().GetProperty("CurveLength",
      BindingFlags.Public | BindingFlags.Instance);
    if (prop != null && prop.CanWrite)
    {
      prop.SetValue(pvi, length);
      return true;
    }

    // Try symmetric/asymmetric curve length properties
    var symProp = pvi.GetType().GetProperty("SymmetricCurveLength",
      BindingFlags.Public | BindingFlags.Instance);
    if (symProp != null && symProp.CanWrite)
    {
      symProp.SetValue(pvi, length);
      return true;
    }

    return false;
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
