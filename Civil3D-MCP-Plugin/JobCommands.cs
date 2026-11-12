using System.Collections;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class JobCommands
{
  private static readonly IReadOnlyDictionary<string, string> LongRunningOperations =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      ["publish_sheet_pdf"] = "publishSheetPdf",
      ["surface_dem_import"] = "createSurfaceFromDem",
      ["bulk_qc_report"] = "qcReportGenerate",
      ["plot_layouts_to_pdf"] = "plotLayoutsToPdf",
      ["publish_sheet_set"] = "plotPublishSheetSet",
    };

  public static Task<object?> StartJobAsync(JsonObject? parameters)
  {
    var operation = PluginRuntime.GetRequiredString(parameters, "operation");
    if (!LongRunningOperations.TryGetValue(operation, out var pluginMethod))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"Unsupported job operation '{operation}'. Supported operations: {string.Join(", ", LongRunningOperations.Keys)}. Corridor rebuilds already return jobs through civil3d_corridor.");
    }

    var operationParameters = parameters?["parameters"] as JsonObject;
    operationParameters = operationParameters?.DeepClone() as JsonObject ?? new JsonObject();
    var requestId = PluginRuntime.GetCurrentRequestId();
    var job = JobRegistry.Create(
      $"Queued {operation}",
      operation,
      requestId,
      PluginRuntime.GetActiveDrawingIdentity());
    job.CancellationSource = new CancellationTokenSource();
    var cancellationToken = job.CancellationSource.Token;

    _ = Task.Run(async () =>
    {
      try
      {
        await PluginRuntime.RunWithRequestContextAsync(
          pluginMethod,
          $"{requestId ?? "job"}:job:{job.JobId}",
          cancellationToken,
          job.DrawingIdentity,
          async () =>
          {
            // Keep the queued state observable long enough for a caller to
            // issue an immediate cooperative cancellation after StartJob.
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            JobRegistry.Progress(job.JobId, 10, $"Starting {operation}", null);
            PluginLog.Info("Job", $"Starting {operation} (job {job.JobId}, request {requestId ?? "none"}).");
            var result = await CommandDispatcher.DispatchAsync(pluginMethod, operationParameters, cancellationToken);
            var warnings = ExtractWarnings(result).ToList();
            if (cancellationToken.IsCancellationRequested)
            {
              warnings.Add("Cancellation was requested after non-interruptible Civil 3D host work began; the committed result is reported as completed.");
            }
            JobRegistry.Complete(job.JobId, result, warnings);
            PluginLog.Info("Job", $"Completed {operation} (job {job.JobId}).");
            return result;
          });
      }
      catch (OperationCanceledException)
      {
        JobRegistry.AcknowledgeCancellation(job.JobId);
        PluginLog.Info("Job", $"Cancelled {operation} (job {job.JobId}).");
      }
      catch (JsonRpcDispatchException ex)
      {
        JobRegistry.Fail(job.JobId, ex.Message, ex.Code);
        PluginLog.Info("Job", $"Failed {operation} (job {job.JobId}) [{ex.Code}]: {ex.Message}");
      }
      catch (Exception ex)
      {
        JobRegistry.Fail(job.JobId, ex.Message, ex.GetType().Name);
        PluginLog.Error("Job", $"Unhandled failure in {operation} (job {job.JobId}).", ex);
      }
      finally
      {
        try
        {
          job.CancellationSource?.Dispose();
        }
        catch (ObjectDisposedException)
        {
          // Worker and cancellation raced; the source is already released.
        }
        job.CancellationSource = null;
      }
    });

    return Task.FromResult<object?>(ToResponse(job));
  }

  public static object? GetJobStatus(JsonObject? parameters)
  {
    var jobId = PluginRuntime.GetRequiredString(parameters, "jobId");
    return ToResponse(JobRegistry.Get(jobId));
  }

  public static object? CancelJob(JsonObject? parameters)
  {
    var jobId = PluginRuntime.GetRequiredString(parameters, "jobId");
    return ToResponse(JobRegistry.Cancel(jobId));
  }

  private static IEnumerable<string> ExtractWarnings(object? result)
  {
    if (result is not IDictionary<string, object?> dictionary
      || !dictionary.TryGetValue("warnings", out var warningValue)
      || warningValue is not IEnumerable warnings
      || warningValue is string)
    {
      return [];
    }

    return warnings.Cast<object?>()
      .Select(value => value?.ToString())
      .Where(value => !string.IsNullOrWhiteSpace(value))
      .Cast<string>()
      .ToArray();
  }

  private static Dictionary<string, object?> ToResponse(JobRecord job)
  {
    var stats = JobRegistry.GetStats();
    return new Dictionary<string, object?>
    {
      ["jobId"] = job.JobId,
      ["state"] = job.State,
      ["operation"] = job.Operation,
      ["progressPercent"] = job.ProgressPercent,
      ["currentPhase"] = job.CurrentPhase,
      ["estimatedRemainingSeconds"] = job.EstimatedRemainingSeconds,
      ["result"] = job.Result,
      ["createdAt"] = job.CreatedAt,
      ["completedAt"] = job.CompletedAt,
      ["durationMs"] = job.DurationMs,
      ["requestId"] = job.RequestId,
      ["drawingIdentity"] = job.DrawingIdentity,
      ["failureCategory"] = job.FailureCategory,
      ["cancellationRequested"] = job.CancellationRequested,
      ["warnings"] = job.Warnings.ToArray(),
      ["registry"] = new Dictionary<string, object?>
      {
        ["total"] = stats.Total,
        ["running"] = stats.Running,
        ["capacity"] = stats.Capacity,
        ["terminalRetentionMinutes"] = stats.TerminalRetentionMinutes,
      },
    };
  }
}
