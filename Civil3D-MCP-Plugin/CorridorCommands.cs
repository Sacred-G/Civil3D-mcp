using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
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

    // Create the job up front so the caller gets a jobId immediately — they
    // poll civil3d_job(status, jobId) until the background rebuild finishes.
    var requestId = PluginRuntime.GetCurrentRequestId();
    var job = JobRegistry.Create(
      $"Rebuilding corridor {name}",
      "corridor_rebuild",
      requestId,
      PluginRuntime.GetActiveDrawingIdentity());
    job.CancellationSource = new CancellationTokenSource();
    var cancellationToken = job.CancellationSource.Token;

    // Dispatch the rebuild to the Civil 3D command thread via
    // CivilExecution.WriteAsync, but do NOT await it here so the JSON-RPC
    // handler returns immediately and the HTTP/TCP command timeout does not
    // apply to long-running rebuilds.
    _ = Task.Run(async () =>
    {
      try
      {
        await PluginRuntime.RunWithRequestContextAsync(
          "rebuildCorridor",
          $"{requestId ?? "job"}:job:{job.JobId}",
          cancellationToken,
          job.DrawingIdentity,
          async () =>
          {
            cancellationToken.ThrowIfCancellationRequested();
            JobRegistry.Progress(job.JobId, 10, $"Scheduling rebuild for corridor {name}", null);

            var result = await CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
            {
              cancellationToken.ThrowIfCancellationRequested();

              var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForWrite);

              // Corridor.Rebuild() is synchronous and exposes no cancellation
              // handle. Abort before entering and after it returns.
              JobRegistry.Progress(job.JobId, 40, $"Rebuilding corridor {name}", null);
              corridor.Rebuild();
              cancellationToken.ThrowIfCancellationRequested();

              return new Dictionary<string, object?>
              {
                ["corridorName"] = name,
                ["state"] = GetCorridorState(corridor),
              };
            });

            JobRegistry.Complete(job.JobId, result);
            PluginLog.Info("Corridor", $"Rebuild completed for corridor '{name}' (job {job.JobId}).");
            return result;
          });
      }
      catch (OperationCanceledException)
      {
        JobRegistry.AcknowledgeCancellation(job.JobId);
        PluginLog.Info("Corridor", $"Rebuild cancelled for corridor '{name}' (job {job.JobId}).");
      }
      catch (Exception ex)
      {
        PluginLog.Error("Corridor", $"Rebuild failed for corridor '{name}' (job {job.JobId}).", ex);
        try
        {
          JobRegistry.Fail(job.JobId, ex.Message);
        }
        catch (Exception failEx)
        {
          PluginLog.Error("Corridor", $"Unable to mark job {job.JobId} as failed.", failEx);
        }
        job.CancellationSource = null;
      }
      finally
      {
        // Release the cancellation source once the worker has exited.
        try
        {
          job.CancellationSource?.Dispose();
        }
        catch (ObjectDisposedException)
        {
          // Already disposed (e.g. by Cancel/Dispose race); safe to ignore.
        }
      }
    }, CancellationToken.None);

    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["jobId"] = job.JobId,
      ["state"] = "running",
      ["message"] = $"Corridor '{name}' rebuild queued. Poll civil3d_job with action='status' to track progress.",
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
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var corridorSurfaceName = PluginRuntime.GetRequiredString(parameters, "corridorSurface");
    var referenceSurfaceName = PluginRuntime.GetRequiredString(parameters, "referenceSurface");

    return CivilExecution.ReadInCommandAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
      var corridorSurface = FindCorridorSurfaceByName(corridor, corridorSurfaceName, transaction);
      var referenceSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, referenceSurfaceName, OpenMode.ForRead);
      return ComputeVolumeBetweenSurfaces(database, transaction, corridorSurface, referenceSurface);
    });
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
    var corridorSurfaces = corridor.CorridorSurfaces;
    return corridorSurfaces.Cast<CorridorSurface>()
      .Select(surface => new Dictionary<string, object?>
      {
        ["name"] = surface.Name,
        ["surfaceId"] = surface.SurfaceId == ObjectId.Null ? null : surface.SurfaceId.Handle.ToString(),
        ["boundaries"] = surface.Boundaries.Cast<CorridorSurfaceBoundary>().Select(boundary => boundary.Name).ToList(),
      })
      .ToList();
  }

  private static List<Dictionary<string, object?>> ReadCorridorFeatureLines(Corridor corridor)
  {
    var featureLines = corridor.FeatureLineCodeInfos;
    return featureLines.Cast<FeatureLineCodeInfo>()
      .Select(feature => new Dictionary<string, object?>
      {
        ["name"] = feature.CodeName,
      })
      .ToList();
  }

  private static string GetCorridorState(Corridor corridor) => corridor.IsOutOfDate ? "out_of_date" : "built";

  private static Autodesk.Civil.DatabaseServices.Surface FindCorridorSurfaceByName(Corridor corridor, string name, Transaction transaction)
  {
    var definition = corridor.CorridorSurfaces.Cast<CorridorSurface>()
      .FirstOrDefault(surface => string.Equals(surface.Name, name, StringComparison.OrdinalIgnoreCase))
      ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Corridor surface '{name}' was not found on corridor '{corridor.Name}'.");

    if (definition.SurfaceId == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Corridor surface '{name}' has no built Civil 3D surface ObjectId.");
    }

    return CivilObjectUtils.GetRequiredObject<Autodesk.Civil.DatabaseServices.Surface>(transaction, definition.SurfaceId, OpenMode.ForRead);
  }

  private static Dictionary<string, object?> ComputeVolumeBetweenSurfaces(
    Database database,
    Transaction transaction,
    Autodesk.Civil.DatabaseServices.Surface baseSurface,
    Autodesk.Civil.DatabaseServices.Surface comparisonSurface)
  {
    var temporaryName = $"MCP_TMP_CORRIDOR_VOLUME_{Guid.NewGuid():N}";
    var volumeSurfaceId = TinVolumeSurface.Create(temporaryName, baseSurface.ObjectId, comparisonSurface.ObjectId);
    var volumeSurface = CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, volumeSurfaceId, OpenMode.ForRead);
    var properties = volumeSurface.GetVolumeProperties();
    return new Dictionary<string, object?>
    {
      ["cutVolume"] = properties.UnadjustedCutVolume,
      ["fillVolume"] = properties.UnadjustedFillVolume,
      ["netVolume"] = properties.UnadjustedNetVolume,
      ["units"] = new Dictionary<string, object?>
      {
        ["volume"] = CivilObjectUtils.VolumeUnits(database),
      },
      ["source"] = "TinVolumeSurface.GetVolumeProperties",
    };
  }
}
