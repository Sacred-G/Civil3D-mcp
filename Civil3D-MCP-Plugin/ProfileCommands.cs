using System.Collections;
using System.Reflection;
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
        ["type"] = MapProfileType(CivilObjectUtils.GetStringProperty(profile, "ProfileType") ?? profile.GetType().Name),
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
      var elevation = InvokeProfileDouble(profile, station, "ElevationAt", "GetElevationAt", "ElevationFromStation");
      var grade = InvokeProfileDouble(profile, station, "GradeAt", "GetGradeAt");

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
          ["elevation"] = InvokeProfileDouble(profile, station, "ElevationAt", "GetElevationAt", "ElevationFromStation"),
          ["grade"] = InvokeProfileNullableDouble(profile, station, "GradeAt", "GetGradeAt"),
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

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["created"] = true,
      };
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

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["profileName"] = profile.Name,
        ["created"] = true,
      };
    });
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
      ["type"] = MapProfileType(CivilObjectUtils.GetStringProperty(profile, "ProfileType") ?? profile.GetType().Name),
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
    var collection = CivilObjectUtils.GetPropertyValue<object>(profile, "Entities");
    if (collection is not IEnumerable enumerable)
    {
      return entities;
    }

    var index = 0;
    foreach (var entity in enumerable)
    {
      entities.Add(new Dictionary<string, object?>
      {
        ["index"] = index++,
        ["type"] = MapProfileEntityType(entity?.GetType().Name ?? string.Empty),
        ["startStation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "StartStation") ?? 0,
        ["endStation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "EndStation") ?? 0,
        ["startElevation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "StartElevation") ?? 0,
        ["endElevation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "EndElevation") ?? 0,
        ["grade"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "Grade"),
        ["length"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0,
      });
    }

    return entities;
  }

  private static (double Min, double Max) GetElevationExtents(Profile profile)
  {
    var min = CivilObjectUtils.GetPropertyValue<double?>(profile, "MinimumElevation");
    var max = CivilObjectUtils.GetPropertyValue<double?>(profile, "MaximumElevation");
    if (min.HasValue && max.HasValue)
    {
      return (min.Value, max.Value);
    }

    var elevations = ReadProfileEntities(profile)
      .SelectMany(entity => new[]
      {
        Convert.ToDouble(entity["startElevation"] ?? 0d),
        Convert.ToDouble(entity["endElevation"] ?? 0d),
      })
      .ToList();

    return elevations.Count == 0 ? (0, 0) : (elevations.Min(), elevations.Max());
  }

  private static int CountPvis(Profile profile)
  {
    var pvis = CivilObjectUtils.GetPropertyValue<object>(profile, "PVIs");
    if (pvis == null)
    {
      return 0;
    }

    var countProperty = pvis.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
    return countProperty == null ? 0 : Convert.ToInt32(countProperty.GetValue(pvis) ?? 0);
  }

  private static double InvokeProfileDouble(Profile profile, double station, params string[] methodNames)
  {
    foreach (var methodName in methodNames)
    {
      var value = CivilObjectUtils.InvokeMethod(profile, methodName, station);
      if (value != null)
      {
        return Convert.ToDouble(value);
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"The active Civil 3D version does not expose a supported elevation method for profile '{profile.Name}'.");
  }

  private static double? InvokeProfileNullableDouble(Profile profile, double station, params string[] methodNames)
  {
    foreach (var methodName in methodNames)
    {
      var value = CivilObjectUtils.InvokeMethod(profile, methodName, station);
      if (value != null)
      {
        return Convert.ToDouble(value);
      }
    }

    return null;
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

  private static string MapProfileEntityType(string value)
  {
    var text = value.ToLowerInvariant();
    if (text.Contains("asymmetric"))
    {
      return "asymmetric_parabola";
    }

    if (text.Contains("parabola"))
    {
      return "parabola";
    }

    if (text.Contains("curve"))
    {
      return "circular_curve";
    }

    return "tangent";
  }
}
