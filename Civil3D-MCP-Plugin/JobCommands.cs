using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class JobCommands
{
  public static object? GetJobStatus(JsonObject? parameters)
  {
    var jobId = PluginRuntime.GetRequiredString(parameters, "jobId");
    var job = JobRegistry.Get(jobId);
    return ToResponse(job);
  }

  public static object? CancelJob(JsonObject? parameters)
  {
    var jobId = PluginRuntime.GetRequiredString(parameters, "jobId");
    var job = JobRegistry.Cancel(jobId);
    return ToResponse(job);
  }

  private static Dictionary<string, object?> ToResponse(JobRecord job)
  {
    return new Dictionary<string, object?>
    {
      ["jobId"] = job.JobId,
      ["state"] = job.State,
      ["progressPercent"] = job.ProgressPercent,
      ["currentPhase"] = job.CurrentPhase,
      ["estimatedRemainingSeconds"] = job.EstimatedRemainingSeconds,
      ["result"] = job.Result,
    };
  }
}
