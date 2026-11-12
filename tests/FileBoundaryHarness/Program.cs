using System.Text;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Sockets;
using Civil3DMcpPlugin;

var testRoot = Path.Combine(Path.GetTempPath(), $"civil3d-mcp-file-boundary-{Guid.NewGuid():N}");
var allowedRoot = Path.Combine(testRoot, "allowed");
var outsideRoot = Path.Combine(testRoot, "outside");
var exportOnlyRoot = Path.Combine(testRoot, "export-only");
Directory.CreateDirectory(allowedRoot);
Directory.CreateDirectory(outsideRoot);
Directory.CreateDirectory(exportOnlyRoot);

Environment.SetEnvironmentVariable("CIVIL3D_IMPORT_ROOTS", allowedRoot);
Environment.SetEnvironmentVariable("CIVIL3D_EXPORT_ROOTS", allowedRoot + Path.PathSeparator + exportOnlyRoot);

try
{
  var nestedOutput = Path.Combine(allowedRoot, "reports", "quantities.csv");
  var canonical = FileBoundary.ResolveExportPath(nestedOutput, overwrite: false, ".csv");
  Assert(canonical == Path.GetFullPath(nestedOutput), "Allowed output was not canonicalized.");

  ExpectCode(
    "CIVIL3D.PATH_NOT_ALLOWED",
    () => FileBoundary.ResolveExportPath(Path.Combine(allowedRoot, "..", "outside", "escape.csv"), false, ".csv"));
  ExpectCode(
    "CIVIL3D.FILE_TYPE_NOT_ALLOWED",
    () => FileBoundary.ResolveExportPath(Path.Combine(allowedRoot, "report.exe"), false, ".csv"));

  var writtenPath = FileBoundary.WriteAllTextAtomic(
    nestedOutput, "first", Encoding.UTF8, overwrite: false, ".csv");
  Assert(File.ReadAllText(writtenPath, Encoding.UTF8) == "first", "Atomic write content mismatch.");
  Assert(!Directory.EnumerateFiles(Path.GetDirectoryName(writtenPath)!, ".*.tmp").Any(), "Atomic write left a temp file.");

  ExpectCode(
    "CIVIL3D.CONFLICT",
    () => FileBoundary.WriteAllTextAtomic(nestedOutput, "blocked", Encoding.UTF8, overwrite: false, ".csv"));
  FileBoundary.WriteAllTextAtomic(nestedOutput, "replacement", Encoding.UTF8, overwrite: true, ".csv");
  Assert(File.ReadAllText(writtenPath, Encoding.UTF8) == "replacement", "Explicit overwrite did not replace content.");

  var importPath = Path.Combine(allowedRoot, "terrain.dem");
  File.WriteAllText(importPath, "dem-data");
  Assert(FileBoundary.ResolveImportPath(importPath, ".dem") == Path.GetFullPath(importPath), "Allowed import was rejected.");
  ExpectCode(
    "CIVIL3D.OBJECT_NOT_FOUND",
    () => FileBoundary.ResolveImportPath(Path.Combine(allowedRoot, "missing.dem"), ".dem"));
  ExpectCode(
    "CIVIL3D.PATH_NOT_ALLOWED",
    () => FileBoundary.ResolveImportPath(Path.Combine(outsideRoot, "outside.dem"), ".dem"));

  // Read-back (compare snapshots): import or export roots, must exist.
  var snapshotPath = Path.Combine(allowedRoot, "snapshot.json");
  File.WriteAllText(snapshotPath, "{}");
  Assert(FileBoundary.ResolveReadBackPath(snapshotPath, ".json") == Path.GetFullPath(snapshotPath), "Allowed read-back was rejected.");
  ExpectCode(
    "CIVIL3D.OBJECT_NOT_FOUND",
    () => FileBoundary.ResolveReadBackPath(Path.Combine(allowedRoot, "missing.json"), ".json"));
  ExpectCode(
    "CIVIL3D.PATH_NOT_ALLOWED",
    () => FileBoundary.ResolveReadBackPath(Path.Combine(outsideRoot, "outside.json"), ".json"));
  // A snapshot under an export-only root reads back, though it is no import.
  var exportOnlySnapshot = Path.Combine(exportOnlyRoot, "snapshot.json");
  File.WriteAllText(exportOnlySnapshot, "{}");
  Assert(FileBoundary.ResolveReadBackPath(exportOnlySnapshot, ".json") == Path.GetFullPath(exportOnlySnapshot), "Export-only snapshot read-back was rejected.");
  ExpectCode(
    "CIVIL3D.PATH_NOT_ALLOWED",
    () => FileBoundary.ResolveImportPath(exportOnlySnapshot, ".json"));

  var rpcError = JsonNode.Parse(JsonRpcProtocol.SerializeError(
    JsonValue.Create("request-1"),
    JsonRpcProtocol.NumericErrorCode("CIVIL3D.OBJECT_NOT_FOUND"),
    "CIVIL3D.OBJECT_NOT_FOUND",
    "Surface was not found"))!.AsObject();
  Assert(rpcError["jsonrpc"]!.GetValue<string>() == "2.0", "JSON-RPC version is invalid.");
  Assert(rpcError["error"]!["code"]!.GetValue<int>() == -32004, "JSON-RPC error code is not numeric.");
  Assert(rpcError["error"]!["data"]!["code"]!.GetValue<string>() == "CIVIL3D.OBJECT_NOT_FOUND", "Domain error code was not retained in error.data.code.");

  var completedJob = JobRegistry.Create("Queued", "bulk_qc_report", "request-complete", "drawing-a");
  JobRegistry.Progress(completedJob.JobId, 42, "Working", 10);
  JobRegistry.Complete(completedJob.JobId, new { report = "ok" }, new[] { "review warning" });
  var completed = JobRegistry.Get(completedJob.JobId);
  Assert(completed.State == "completed" && completed.ProgressPercent == 100, "Completed job state was invalid.");
  Assert(completed.DurationMs is >= 0 && completed.RequestId == "request-complete", "Completed job telemetry was missing.");
  Assert(completed.Warnings.Count == 1, "Completed job warnings were not retained.");

  var failedJob = JobRegistry.Create("Queued", "surface_dem_import", "request-fail", "drawing-b");
  JobRegistry.Fail(failedJob.JobId, "Import failed", "validation");
  var failed = JobRegistry.Get(failedJob.JobId);
  Assert(failed.State == "failed" && failed.FailureCategory == "validation", "Failure telemetry was invalid.");

  var cancelledJob = JobRegistry.Create("publish_sheet_pdf");
  JobRegistry.Cancel(cancelledJob.JobId);
  Assert(JobRegistry.Get(cancelledJob.JobId).State == "running" && JobRegistry.Get(cancelledJob.JobId).CancellationRequested, "Cancellation request was not retained while work remained in flight.");
  JobRegistry.AcknowledgeCancellation(cancelledJob.JobId);
  Assert(JobRegistry.Get(cancelledJob.JobId).State == "cancelled", "Cancelled job state was invalid.");

  var stats = JobRegistry.GetStats();
  Assert(stats.Completed >= 1 && stats.Failed >= 1 && stats.Cancelled >= 1, "Job registry statistics were invalid.");
  Assert(stats.Total <= stats.Capacity, "Job registry exceeded its bounded capacity.");

  for (var iteration = 0; iteration < 25; iteration++)
  {
    var raceJob = JobRegistry.Create("Race test");
    await Task.WhenAll(
      Task.Run(() => JobRegistry.Complete(raceJob.JobId, new { ok = true })),
      Task.Run(() => { JobRegistry.Cancel(raceJob.JobId); JobRegistry.AcknowledgeCancellation(raceJob.JobId); }));
    var terminalRaceJob = JobRegistry.Get(raceJob.JobId);
    Assert(terminalRaceJob.State is "completed" or "cancelled", "Racing terminal transitions produced an invalid job state.");
    Assert(terminalRaceJob.CompletedAt.HasValue && terminalRaceJob.DurationMs is >= 0, "Racing terminal transition lost timing telemetry.");
  }

  var capacityRejected = false;
  for (var index = 0; index <= stats.Capacity + 4; index++)
  {
    try
    {
      JobRegistry.Create("Capacity test");
    }
    catch (JsonRpcDispatchException exception) when (exception.Code == "CIVIL3D.HOST_BUSY")
    {
      capacityRejected = true;
      break;
    }
  }
  Assert(capacityRejected, "Job registry did not reject work at its bounded capacity.");
  Assert(JobRegistry.GetStats().Total <= stats.Capacity, "Job registry exceeded capacity under pressure.");

  var disconnectPort = GetFreeTcpPort();
  var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  var handlerCancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  var rpcServer = new RpcTcpServer(disconnectPort, async (_, cancellationToken) =>
  {
    handlerStarted.TrySetResult();
    try
    {
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
      return "{}";
    }
    catch (OperationCanceledException)
    {
      handlerCancelled.TrySetResult();
      throw;
    }
  });
  rpcServer.Start();
  try
  {
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, disconnectPort);
    var requestBytes = Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"method\":\"wait\",\"id\":\"disconnect-test\"}");
    await client.GetStream().WriteAsync(requestBytes);
    await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(3));
    client.Dispose();
    await handlerCancelled.Task.WaitAsync(TimeSpan.FromSeconds(3));
  }
  finally
  {
    rpcServer.Stop();
  }

  var responsePort = GetFreeTcpPort();
  var responseServer = new RpcTcpServer(responsePort, (_, _) => Task.FromResult("{\"ok\":true}"));
  responseServer.Start();
  try
  {
    using var responseClient = new TcpClient();
    await responseClient.ConnectAsync(IPAddress.Loopback, responsePort);
    var requestBytes = Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"method\":\"ping\",\"id\":\"response-test\"}");
    await responseClient.GetStream().WriteAsync(requestBytes);
    var responseBuffer = new byte[128];
    var responseLength = await responseClient.GetStream().ReadAsync(responseBuffer).AsTask().WaitAsync(TimeSpan.FromSeconds(3));
    Assert(Encoding.UTF8.GetString(responseBuffer, 0, responseLength) == "{\"ok\":true}", "RPC response path stalled while stopping the disconnect monitor.");
  }
  finally
  {
    responseServer.Stop();
  }

  Console.WriteLine("P1 disconnect cancellation, P2 filesystem/JSON-RPC, and P4 bounded-job checks passed.");
}
finally
{
  Directory.Delete(testRoot, recursive: true);
}

static void ExpectCode(string expectedCode, Action action)
{
  try
  {
    action();
  }
  catch (JsonRpcDispatchException exception) when (exception.Code == expectedCode)
  {
    return;
  }

  throw new InvalidOperationException($"Expected JsonRpcDispatchException code {expectedCode}.");
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static int GetFreeTcpPort()
{
  var listener = new TcpListener(IPAddress.Loopback, 0);
  listener.Start();
  var port = ((IPEndPoint)listener.LocalEndpoint).Port;
  listener.Stop();
  return port;
}
