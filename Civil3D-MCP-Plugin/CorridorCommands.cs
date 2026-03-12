using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class CorridorCommands
{
  public static Task<object?> ListCorridorsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridors = new List<Dictionary<string, object?>>();
      foreach (ObjectId objectId in civilDoc.CorridorCollection)
      {
        var corridor = CivilObjectUtils.GetRequiredObject<Corridor>(transaction, objectId, OpenMode.ForRead);
        corridors.Add(ToCorridorSummary(corridor));
      }

      return new Dictionary<string, object?>
      {
        ["corridors"] = corridors,
      };
    });
  }

  public static Task<object?> GetCorridorAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
      var baselines = new List<Dictionary<string, object?>>();

      foreach (Baseline baseline in corridor.Baselines)
      {
        var regions = new List<Dictionary<string, object?>>();
        foreach (BaselineRegion region in baseline.BaselineRegions)
        {
          regions.Add(new Dictionary<string, object?>
          {
            ["name"] = region.Name,
            ["assemblyName"] = CivilObjectUtils.GetName(transaction.GetObject(region.AssemblyId, OpenMode.ForRead)) ?? string.Empty,
            ["startStation"] = region.StartStation,
            ["endStation"] = region.EndStation,
            ["frequency"] = 0,
          });
        }

        baselines.Add(new Dictionary<string, object?>
        {
          ["name"] = baseline.Name,
          ["alignmentName"] = CivilObjectUtils.GetName(transaction.GetObject(baseline.AlignmentId, OpenMode.ForRead)) ?? string.Empty,
          ["profileName"] = CivilObjectUtils.GetName(transaction.GetObject(baseline.ProfileId, OpenMode.ForRead)) ?? string.Empty,
          ["regions"] = regions,
        });
      }

      var surfaces = ReadCorridorSurfaces(corridor, transaction);
      var featureLines = ReadCorridorFeatureLines(corridor);

      return new Dictionary<string, object?>
      {
        ["name"] = corridor.Name,
        ["handle"] = CivilObjectUtils.GetHandle(corridor),
        ["style"] = CivilObjectUtils.GetName(transaction.GetObject(corridor.StyleId, OpenMode.ForRead)) ?? string.Empty,
        ["layer"] = corridor.Layer,
        ["baselines"] = baselines,
        ["surfaces"] = surfaces,
        ["featureLineCount"] = featureLines.Count,
        ["state"] = GetCorridorState(corridor),
      };
    });
  }

  public static Task<object?> RebuildCorridorAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var job = JobRegistry.Create($"Rebuilding corridor {name}");
      corridor.Rebuild();
      JobRegistry.Complete(job.JobId, new Dictionary<string, object?> { ["corridorName"] = name, ["state"] = GetCorridorState(corridor) });
      return new Dictionary<string, object?>
      {
        ["jobId"] = job.JobId,
      };
    });
  }

  public static Task<object?> GetCorridorSurfacesAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridor.Name,
        ["surfaces"] = ReadCorridorSurfaces(corridor, transaction),
      };
    });
  }

  public static Task<object?> GetCorridorFeatureLinesAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridor.Name,
        ["featureLines"] = ReadCorridorFeatureLines(corridor),
      };
    });
  }

  public static Task<object?> ComputeCorridorVolumesAsync(JsonObject? parameters)
  {
    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "computeCorridorVolumes is not implemented in the initial C# plugin build yet.");
  }

  private static Dictionary<string, object?> ToCorridorSummary(Corridor corridor)
  {
    return new Dictionary<string, object?>
    {
      ["name"] = corridor.Name,
      ["handle"] = CivilObjectUtils.GetHandle(corridor),
      ["baselineCount"] = corridor.Baselines.Count,
      ["regionCount"] = corridor.Baselines.Cast<Baseline>().Sum(baseline => baseline.BaselineRegions.Count),
      ["surfaceCount"] = ReadCorridorSurfaces(corridor, null).Count,
      ["state"] = GetCorridorState(corridor),
      ["lastBuildTime"] = CivilObjectUtils.GetPropertyValue<DateTime?>(corridor, "RebuildDate")?.ToString("O"),
    };
  }

  private static List<Dictionary<string, object?>> ReadCorridorSurfaces(Corridor corridor, Transaction? transaction)
  {
    var result = new List<Dictionary<string, object?>>();
    foreach (var propertyName in new[] { "CorridorSurfaces", "Surfaces" })
    {
      if (CivilObjectUtils.GetPropertyValue<object>(corridor, propertyName) is not IEnumerable enumerable)
      {
        continue;
      }

      foreach (var item in enumerable)
      {
        result.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(item) ?? string.Empty,
          ["boundaries"] = ReadBoundaryNames(item, transaction),
        });
      }

      if (result.Count > 0)
      {
        break;
      }
    }

    return result;
  }

  private static List<Dictionary<string, object?>> ReadCorridorFeatureLines(Corridor corridor)
  {
    var features = new List<Dictionary<string, object?>>();
    foreach (var propertyName in new[] { "FeatureLines", "CodeNameToFeatureLineMap" })
    {
      if (CivilObjectUtils.GetPropertyValue<object>(corridor, propertyName) is not IEnumerable enumerable)
      {
        continue;
      }

      foreach (var item in enumerable)
      {
        features.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(item) ?? item?.ToString(),
        });
      }

      if (features.Count > 0)
      {
        break;
      }
    }

    return features;
  }

  private static List<string> ReadBoundaryNames(object? surface, Transaction? transaction)
  {
    var names = new List<string>();
    if (surface == null)
    {
      return names;
    }

    var boundaries = CivilObjectUtils.GetPropertyValue<object>(surface, "Boundaries") as IEnumerable;
    if (boundaries == null)
    {
      return names;
    }

    foreach (var boundary in boundaries)
    {
      names.Add(CivilObjectUtils.GetName(boundary) ?? boundary?.ToString() ?? string.Empty);
    }

    return names;
  }

  private static string GetCorridorState(Corridor corridor)
  {
    var isOutOfDate = CivilObjectUtils.GetPropertyValue<bool?>(corridor, "IsOutOfDate") ?? false;
    if (isOutOfDate)
    {
      return "out_of_date";
    }

    var hasError = CivilObjectUtils.GetPropertyValue<bool?>(corridor, "HasErrors") ?? false;
    return hasError ? "error" : "built";
  }
}
