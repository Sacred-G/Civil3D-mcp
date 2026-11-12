using System.Text;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Civil3DMcpPlugin;

var testRoot = Path.Combine(Path.GetTempPath(), $"civil3d-mcp-file-boundary-{Guid.NewGuid():N}");
var allowedRoot = Path.Combine(testRoot, "allowed");
var outsideRoot = Path.Combine(testRoot, "outside");
Directory.CreateDirectory(allowedRoot);
Directory.CreateDirectory(outsideRoot);

Environment.SetEnvironmentVariable("CIVIL3D_IMPORT_ROOTS", allowedRoot);
Environment.SetEnvironmentVariable("CIVIL3D_EXPORT_ROOTS", allowedRoot);

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

  // External writers (the AutoCAD plotter) write the FINAL path themselves, so
  // a device's "open in viewer when done" opens the real file. The directory
  // chain is created and locked, the final name must be absent (an existing
  // file is moved to a backup, overwrite only), and Commit checks the result
  // is a regular, singly-linked file before the content check runs.
  var plotDirectory = Path.Combine(allowedRoot, "plots", "sheets");
  var plotOutput = FileBoundary.ResolveExportPath(
    Path.Combine(plotDirectory, "C-101.pdf"), overwrite: false, ".pdf");
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: false))
  {
    Assert(Directory.Exists(plotDirectory), "Plot output directory was not created.");
    Assert(external.FinalPath == plotOutput, "The writer is not pointed at the final path.");
    Assert(external.BackupPath == null, "A backup was made although nothing existed.");
    Assert(!File.Exists(plotOutput), "The final name was pre-created.");

    // The lock must stop any segment of the chain being renamed or deleted
    // (and so swapped for a junction) while the external writer runs.
    var chainBlocked = false;
    try
    {
      Directory.Move(Path.Combine(allowedRoot, "plots"), Path.Combine(allowedRoot, "plots-moved"));
    }
    catch (IOException)
    {
      chainBlocked = true;
    }
    catch (UnauthorizedAccessException)
    {
      chainBlocked = true;
    }
    Assert(chainBlocked, "The locked directory chain could be renamed while the writer lock was held.");

    // Nested boundary writes (the publish DSD) still work under the lock, and
    // the DSD is re-checked and pinned while -PUBLISH reads it: other readers
    // can read it, nobody can rewrite, delete or replace it.
    var dsdPath = Path.Combine(plotDirectory, "C-101.mcp-publish.dsd");
    var dsdContent = $"[Target]\r\nDWF={external.FinalPath}\r\n";
    var elsewhere = "DWF=" + Path.Combine(outsideRoot, "elsewhere.pdf");
    FileBoundary.WriteAllTextAtomic(dsdPath, dsdContent, new UTF8Encoding(false), overwrite: true, ".dsd");
    using (FileBoundary.HoldVerifiedFile(dsdPath, dsdContent, new UTF8Encoding(false)))
    {
      Assert(File.ReadAllText(dsdPath) == dsdContent, "A held DSD could not be read by its reader.");
      ExpectBlocked(() => File.WriteAllText(dsdPath, elsewhere), "A held DSD could be rewritten.");
      ExpectBlocked(() => File.Delete(dsdPath), "A held DSD could be deleted.");
      var swap = Path.Combine(plotDirectory, "swap.dsd");
      File.WriteAllText(swap, elsewhere);
      ExpectBlocked(() => File.Move(swap, dsdPath, overwrite: true), "A held DSD could be replaced by a rename.");
      File.Delete(swap);
    }
    // A DSD changed after it was written (DWF= pointed elsewhere) is refused.
    File.WriteAllText(dsdPath, dsdContent.Replace(external.FinalPath, Path.Combine(outsideRoot, "escape.pdf")), new UTF8Encoding(false));
    ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => FileBoundary.HoldVerifiedFile(dsdPath, dsdContent, new UTF8Encoding(false)).Dispose());
    File.Delete(dsdPath);
    ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => FileBoundary.HoldVerifiedFile(dsdPath, dsdContent, new UTF8Encoding(false)).Dispose());

    File.WriteAllText(external.FinalPath, "%PDF-1.7");
    var verified = external.Commit((path, stream) =>
    {
      Assert(path == external.FinalPath && stream != null, "Commit did not hand the checked output to the content check.");
      // The content check reads the handle Commit checked. While it runs the
      // final entry cannot be deleted, renamed, replaced or rewritten, so it
      // cannot be swapped for a link between the check and the read.
      ExpectBlocked(() => File.Delete(path), "The checked output could be deleted during verification.");
      ExpectBlocked(() => File.Move(path, path + ".moved"), "The checked output could be renamed during verification.");
      var planted = Path.Combine(plotDirectory, "planted.pdf");
      File.WriteAllText(planted, "planted");
      ExpectBlocked(() => File.Move(planted, path, overwrite: true), "The checked output could be replaced during verification.");
      File.Delete(planted);
      ExpectBlocked(() => File.WriteAllText(path, "rewritten"), "The checked output could be rewritten during verification.");
      return new StreamReader(stream!).ReadToEnd();
    });
    Assert(verified == "%PDF-1.7", "Commit did not hand the final file's content to the content check.");
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7", "A committed output was not kept at the final name.");
  AssertNoStaging(plotDirectory, "a committed write");
  ExpectCode(
    "CIVIL3D.CONFLICT",
    () => FileBoundary.ResolveExportPath(plotOutput, overwrite: false, ".pdf"));

  // An existing file is refused without overwrite, before the writer runs.
  ExpectCode("CIVIL3D.CONFLICT", () => FileBoundary.BeginExternalWrite(plotOutput, overwrite: false).Dispose());
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7", "A refused write touched the existing file.");

  // With overwrite the old file waits in a backup while the writer creates a new one.
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: true))
  {
    Assert(!File.Exists(plotOutput), "The final name was not cleared for the writer.");
    Assert(external.BackupPath != null && File.ReadAllText(external.BackupPath) == "%PDF-1.7", "The replaced file was not backed up.");
    File.WriteAllText(external.FinalPath, "%PDF-1.7 replaced");
    external.Commit((path, _) => path);
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7 replaced", "An overwrite commit did not keep the new file.");
  AssertNoStaging(plotDirectory, "an overwrite commit");

  // A failed plot never destroys the previous file: a partial output is removed
  // and the backup restored, whether the writer wrote nothing, wrote a partial
  // file, or the content check rejected the result.
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: true))
  {
    // The writer failed before creating anything.
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7 replaced", "A write that produced nothing lost the previous file.");
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: true))
  {
    File.WriteAllText(external.FinalPath, "%PDF-partial");
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7 replaced", "An uncommitted partial output replaced the previous file.");
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: true))
  {
    File.WriteAllText(external.FinalPath, string.Empty);
    ExpectCode("CIVIL3D.API_ERROR", () => external.Commit<int>((_, _) => throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "empty")));
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7 replaced", "A rejected output replaced the previous file.");
  AssertNoStaging(plotDirectory, "failed writes");

  // A fresh output that fails leaves nothing behind.
  var freshOutput = FileBoundary.ResolveExportPath(Path.Combine(plotDirectory, "C-102.pdf"), overwrite: false, ".pdf");
  using (var external = FileBoundary.BeginExternalWrite(freshOutput, overwrite: false))
  {
    File.WriteAllText(external.FinalPath, "%PDF-partial");
  }
  Assert(!File.Exists(freshOutput), "A failed fresh write left its partial output.");

  // A hard link planted at the final name while the writer runs (a link is
  // not a reparse point) is detected before anything reads it, removed, and the
  // previous file restored. The file it pointed to is not deleted. (The write
  // through it is the documented residual window; the call fails.)
  var outsideTarget = Path.Combine(outsideRoot, "victim.pdf");
  File.WriteAllText(outsideTarget, "outside");
  using (var external = FileBoundary.BeginExternalWrite(plotOutput, overwrite: true))
  {
    Assert(CreateHardLinkW(external.FinalPath, outsideTarget, IntPtr.Zero), "Could not create the test hard link.");
    var contentRead = false;
    ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => external.Commit((_, _) => contentRead = true));
    Assert(!contentRead, "The content check ran on a hard-linked output.");
  }
  Assert(File.ReadAllText(plotOutput) == "%PDF-1.7 replaced", "The previous file was not restored after a planted hard link.");
  Assert(File.Exists(outsideTarget), "Removing the planted hard link deleted its target.");

  // A junction at the final name (no privilege needed) is refused up front and left alone.
  var junctionOutput = Path.Combine(plotDirectory, "C-103.pdf");
  if (CreateJunction(junctionOutput, outsideRoot))
  {
    ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => FileBoundary.BeginExternalWrite(junctionOutput, overwrite: true).Dispose());
    Assert(Directory.Exists(junctionOutput) && Directory.Exists(outsideRoot), "A refused junction or its target was removed.");
    Directory.Delete(junctionOutput, recursive: false);
  }
  else
  {
    Console.WriteLine("Skipped the junction check: mklink /J is unavailable.");
  }

  // A symbolic link at the final name is refused up front, and one planted
  // during the write is detected and removed (needs the symlink privilege or
  // Developer Mode; skipped otherwise).
  var symlinkOutput = Path.Combine(plotDirectory, "C-104.pdf");
  if (TryCreateFileSymlink(symlinkOutput, outsideTarget))
  {
    ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => FileBoundary.BeginExternalWrite(symlinkOutput, overwrite: true).Dispose());
    File.Delete(symlinkOutput);
    using (var external = FileBoundary.BeginExternalWrite(symlinkOutput, overwrite: false))
    {
      File.CreateSymbolicLink(external.FinalPath, outsideTarget);
      ExpectCode("CIVIL3D.PATH_NOT_ALLOWED", () => external.Commit((path, _) => path));
    }
    Assert(!File.Exists(symlinkOutput) && new FileInfo(symlinkOutput).LinkTarget == null, "A planted symbolic link was not removed.");
    Assert(File.ReadAllText(outsideTarget) == "outside", "Removing the planted symbolic link touched its target.");
  }
  else
  {
    Console.WriteLine("Skipped the symbolic-link checks: creating symbolic links needs the privilege or Developer Mode.");
  }
  ExpectCode(
    "CIVIL3D.FILE_TYPE_NOT_ALLOWED",
    () => FileBoundary.ResolveExportPath(Path.Combine(allowedRoot, "plots", "C-101.dwg"), false, ".pdf"));

  var importPath = Path.Combine(allowedRoot, "terrain.dem");
  File.WriteAllText(importPath, "dem-data");
  Assert(FileBoundary.ResolveImportPath(importPath, ".dem") == Path.GetFullPath(importPath), "Allowed import was rejected.");
  ExpectCode(
    "CIVIL3D.OBJECT_NOT_FOUND",
    () => FileBoundary.ResolveImportPath(Path.Combine(allowedRoot, "missing.dem"), ".dem"));
  ExpectCode(
    "CIVIL3D.PATH_NOT_ALLOWED",
    () => FileBoundary.ResolveImportPath(Path.Combine(outsideRoot, "outside.dem"), ".dem"));

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

static void ExpectBlocked(Action action, string message)
{
  try
  {
    action();
  }
  catch (IOException)
  {
    return;
  }
  catch (UnauthorizedAccessException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertNoStaging(string directory, string context)
{
  var leftovers = Directory.EnumerateFiles(directory, ".*").ToList();
  if (leftovers.Count > 0)
  {
    throw new InvalidOperationException($"After {context}, staging files were left behind: {string.Join(", ", leftovers.Select(Path.GetFileName))}");
  }
}

static bool CreateJunction(string junctionPath, string targetDirectory)
{
  try
  {
    using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
      FileName = "cmd.exe",
      ArgumentList = { "/c", "mklink", "/J", junctionPath, targetDirectory },
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true,
    })!;
    process.WaitForExit(10000);
    return process.ExitCode == 0 && Directory.Exists(junctionPath);
  }
  catch
  {
    return false;
  }
}

static bool TryCreateFileSymlink(string linkPath, string targetPath)
{
  try
  {
    File.CreateSymbolicLink(linkPath, targetPath);
    return true;
  }
  catch (IOException)
  {
    return false;
  }
  catch (UnauthorizedAccessException)
  {
    return false;
  }
}

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool CreateHardLinkW(string fileName, string existingFileName, IntPtr securityAttributes);

static int GetFreeTcpPort()
{
  var listener = new TcpListener(IPAddress.Loopback, 0);
  listener.Start();
  var port = ((IPEndPoint)listener.LocalEndpoint).Port;
  listener.Stop();
  return port;
}
