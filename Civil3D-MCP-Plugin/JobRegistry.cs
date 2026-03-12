using System.Collections.Concurrent;

namespace Civil3DMcpPlugin;

public sealed class JobRecord
{
  public required string JobId { get; init; }
  public required string State { get; set; }
  public int? ProgressPercent { get; set; }
  public string? CurrentPhase { get; set; }
  public int? EstimatedRemainingSeconds { get; set; }
  public object? Result { get; set; }
}

public static class JobRegistry
{
  private static readonly ConcurrentDictionary<string, JobRecord> Jobs = new();

  public static JobRecord Create(string currentPhase)
  {
    var record = new JobRecord
    {
      JobId = Guid.NewGuid().ToString("N"),
      State = "running",
      ProgressPercent = 0,
      CurrentPhase = currentPhase,
      EstimatedRemainingSeconds = null,
    };

    Jobs[record.JobId] = record;
    return record;
  }

  public static JobRecord Complete(string jobId, object? result)
  {
    var record = Jobs[jobId];
    record.State = "completed";
    record.ProgressPercent = 100;
    record.EstimatedRemainingSeconds = 0;
    record.Result = result;
    return record;
  }

  public static JobRecord Get(string jobId)
  {
    if (!Jobs.TryGetValue(jobId, out var record))
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Job '{jobId}' was not found.");
    }

    return record;
  }

  public static JobRecord Cancel(string jobId)
  {
    var record = Get(jobId);
    if (record.State == "running")
    {
      record.State = "cancelled";
      record.CurrentPhase = null;
      record.EstimatedRemainingSeconds = null;
    }

    return record;
  }
}
