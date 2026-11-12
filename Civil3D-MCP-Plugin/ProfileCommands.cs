using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class ProfileCommands
{
  public static Task<object?> ListProfilesAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profiles = alignment.GetProfileIds()
        .Cast<ObjectId>()
        .Select(id => CivilObjectUtils.GetRequiredObject<Profile>(transaction, id, OpenMode.ForRead))
        .Select(ToProfileSummary)
        .ToList();

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profiles"] = profiles,
      };
    });
  }

  public static Task<object?> GetProfileAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);
      var entities = ReadProfileEntities(profile);

      return new Dictionary<string, object?>
      {
        ["name"] = profile.Name,
        ["handle"] = CivilObjectUtils.GetHandle(profile),
        ["type"] = MapProfileType(profile.ProfileType.ToString()),
        ["style"] = CivilObjectUtils.GetName(transaction.GetObject(profile.StyleId, OpenMode.ForRead)) ?? string.Empty,
        ["layer"] = profile.Layer,
        ["startStation"] = profile.StartingStation,
        ["endStation"] = profile.EndingStation,
        ["minElevation"] = GetElevationExtents(profile).Min,
        ["maxElevation"] = GetElevationExtents(profile).Max,
        ["entityCount"] = entities.Count,
        ["entities"] = entities,
        ["pviCount"] = CountPvis(profile),
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = CivilObjectUtils.LinearUnits(database),
          ["vertical"] = CivilObjectUtils.LinearUnits(database),
        },
      };
    });
  }

  public static Task<object?> GetProfileElevationAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var station = PluginRuntime.GetRequiredDouble(parameters, "station");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);
      var elevation = ReadProfileElevation(profile, station);
      var grade = ReadProfileGrade(profile, station);

      return new Dictionary<string, object?>
      {
        ["station"] = station,
        ["elevation"] = elevation,
        ["grade"] = grade,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> SampleProfileElevationsAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var interval = PluginRuntime.GetRequiredDouble(parameters, "interval");
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);
      var from = startStation ?? profile.StartingStation;
      var to = endStation ?? profile.EndingStation;
      var samples = new List<Dictionary<string, object?>>();

      for (var station = from; station <= to; station += interval)
      {
        samples.Add(new Dictionary<string, object?>
        {
          ["station"] = station,
          ["elevation"] = ReadProfileElevation(profile, station),
          ["grade"] = ReadProfileGrade(profile, station),
        });
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["startStation"] = from,
        ["endStation"] = to,
        ["interval"] = interval,
        ["samples"] = samples,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> CreateProfileFromSurfaceAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var surfaceName = PluginRuntime.GetRequiredString(parameters, "surfaceName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, surfaceName, OpenMode.ForRead);
      var layerId = LookupUtils.GetLayerId(database, transaction, PluginRuntime.GetOptionalString(parameters, "layer"));
      var styleId = LookupUtils.GetProfileStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var labelSetId = LookupUtils.GetProfileLabelSetId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "labelSet"));
      var profileId = Profile.CreateFromSurface(profileName, alignment.ObjectId, surface.ObjectId, layerId, styleId, labelSetId);
      var profile = CivilObjectUtils.GetRequiredObject<Profile>(transaction, profileId, OpenMode.ForRead);

      return CreatedProfileResult(alignment, profile, transaction);
    });
  }

  public static Task<object?> CreateLayoutProfileAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var layerId = LookupUtils.GetLayerId(database, transaction, PluginRuntime.GetOptionalString(parameters, "layer"));
      var styleId = LookupUtils.GetProfileStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var labelSetId = LookupUtils.GetProfileLabelSetId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "labelSet"));
      var profileId = Profile.CreateByLayout(profileName, alignment.ObjectId, layerId, styleId, labelSetId);
      var profile = CivilObjectUtils.GetRequiredObject<Profile>(transaction, profileId, OpenMode.ForRead);

      return CreatedProfileResult(alignment, profile, transaction);
    });
  }

  /// <summary>
  /// Result of create_layout / create_from_surface. Reports the style and
  /// layer the profile actually got, so a caller can see what was applied.
  /// </summary>
  private static Dictionary<string, object?> CreatedProfileResult(Alignment alignment, Profile profile, Transaction transaction)
  {
    return new Dictionary<string, object?>
    {
      ["alignmentName"] = alignment.Name,
      ["profileName"] = profile.Name,
      ["created"] = true,
      ["handle"] = CivilObjectUtils.GetHandle(profile),
      // A string, as in get and list (empty when the profile has no style).
      ["style"] = profile.StyleId.IsNull
        ? string.Empty
        : CivilObjectUtils.GetName(transaction.GetObject(profile.StyleId, OpenMode.ForRead)) ?? string.Empty,
      ["layer"] = profile.Layer,
    };
  }

  public static Task<object?> DeleteProfileAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForWrite);
      profile.Erase();

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profileName,
        ["deleted"] = true,
      };
    });
  }

  private static Dictionary<string, object?> ToProfileSummary(Profile profile)
  {
    var extents = GetElevationExtents(profile);
    return new Dictionary<string, object?>
    {
      ["name"] = profile.Name,
      ["handle"] = CivilObjectUtils.GetHandle(profile),
      ["type"] = MapProfileType(profile.ProfileType.ToString()),
      ["style"] = string.Empty,
      ["startStation"] = profile.StartingStation,
      ["endStation"] = profile.EndingStation,
      ["minElevation"] = extents.Min,
      ["maxElevation"] = extents.Max,
    };
  }

  private static List<Dictionary<string, object?>> ReadProfileEntities(Profile profile)
  {
    var entities = new List<Dictionary<string, object?>>();
    var index = 0;
    foreach (ProfileEntity entity in profile.Entities)
    {
      entities.Add(new Dictionary<string, object?>
      {
        ["index"] = index++,
        ["type"] = MapProfileEntityType(entity.EntityType.ToString()),
        ["startStation"] = entity.StartStation,
        ["endStation"] = entity.EndStation,
        ["startElevation"] = entity.StartElevation,
        ["endElevation"] = entity.EndElevation,
        ["grade"] = entity is ProfileTangent tangent ? tangent.Grade : null,
        ["length"] = entity.Length,
      });
    }

    return entities;
  }

  private static (double Min, double Max) GetElevationExtents(Profile profile)
  {
    // Civil 3D calculates profile-wide extents, including interior vertical
    // curve extrema and the numeric fallback required for empty layouts.
    return (profile.ElevationMin, profile.ElevationMax);
  }

  private static int CountPvis(Profile profile)
  {
    return profile.PVIs.Count;
  }

  private static double ReadProfileElevation(Profile profile, double station)
  {
    try
    {
      return profile.ElevationAt(station);
    }
    catch (Exception exception)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.API_ERROR",
        $"Could not read elevation on profile '{profile.Name}' at station {station}: {exception.Message}");
    }
  }

  private static double ReadProfileGrade(Profile profile, double station)
  {
    try
    {
      return profile.GradeAt(station);
    }
    catch (Exception exception)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.API_ERROR",
        $"Could not read grade on profile '{profile.Name}' at station {station}: {exception.Message}");
    }
  }

  private static string MapProfileType(string? value)
  {
    var text = value?.ToLowerInvariant() ?? string.Empty;
    if (text.Contains("surface"))
    {
      return "surface";
    }

    if (text.Contains("super"))
    {
      return "superimposed";
    }

    return "layout";
  }

  // Match the ProfileEntityType enum names exactly: a substring test for
  // "asymmetric" also matches "parabolasymmetric" (ParabolaSymmetric), which
  // reported every symmetric curve as asymmetric_parabola.
  private static string MapProfileEntityType(string value)
  {
    return value switch
    {
      "ParabolaSymmetric" => "symmetric_parabola",
      "ParabolaAsymmetric" => "asymmetric_parabola",
      "Circular" => "circular_curve",
      _ => "tangent",
    };
  }
}
