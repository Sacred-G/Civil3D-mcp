using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using AcRuntimeException = Autodesk.AutoCAD.Runtime.Exception;

namespace Civil3DMcpPlugin;

/// <summary>
/// runCode (civil3d_run_code): compiles a C# snippet with Roslyn and runs it
/// inside Civil 3D on the main thread, with the drawing locked and inside a
/// transaction, for work that is too slow over COM or LISP (bulk geometry,
/// side-database reads). Measured 2026-10-11: walking 96,913 objects of a
/// 10 MB background into 547,441 sample points took 1.2 s in-process against
/// about 20 minutes in LISP. Scripts compile inside this already-trusted
/// plugin, so no DLL is written and AutoCAD shows no security prompt.
///
/// Safety: the code runs with the user's full rights; the MCP server always
/// requires an approval token for it, and every script is saved with a summary
/// (RunCodeLog). Read mode (default) runs from an idle tick in an open-close
/// transaction that is never committed, and the compiler refuses the usual
/// write APIs (ScriptCompiler guard; not a sandbox). Write mode runs inside the
/// plugin command C3DMCPHOP in a regular transaction: committed as one undo
/// step when the script returns, rolled back when it throws or runs out of
/// time (changes outside transactions, such as database or system variables,
/// files and Session, are not rolled back). A script cannot be stopped from
/// outside: the time budget only ends it when it calls Check(), and a timeout
/// it catches still ends the run as a timeout.
/// </summary>
public static class RunCodeCommands
{
  private const int MaxCodeChars = 200_000;
  private const int DefaultTimeoutSeconds = 30;
  // Below an older MCP server's fixed 120 s command timeout, with room for
  // compiling and queueing (newer servers wait for the start window too).
  private const int MaxTimeoutSeconds = 90;
  private const int MaxStartWithinSeconds = 300;
  private const int DefaultMaxInlineBytes = 64 * 1024;
  private const int MinInlineBytes = 1024;
  private const int MaxInlineBytes = 1024 * 1024;
  private const int PreviewChars = 2000;
  private static readonly TimeSpan OutputRetention = TimeSpan.FromDays(7);
  private static int _pruned;

  private sealed class RunOutcome
  {
    public RunCodeScript? Script;
    public string? Drawing;
    public long RunMs;
    public byte[]? Result;
    public string? ResultShape;
    public string? ResultError;
    // Set on the main thread when the host action has finished. False after a
    // cancelled request: the request stopped waiting, the script may still run.
    public volatile bool ActionDone;
    // Write mode: set right after the run's transaction was committed.
    public volatile bool Committed;
    // Write mode: the client cancelled after the commit.
    public volatile bool CancelledAfterCommit;
    // Write mode: the script ended the run's transaction itself (not through Tr,
    // which cannot end it, but for example through TopTransaction).
    public volatile bool TransactionEndedByScript;
    // Write mode: transactions the script left open and the run aborted.
    public int LeakedTransactions;
    // 0 waiting, 1 started (main thread, just before the script is created),
    // 2 given up (request thread, cancelled while queued). The first claim
    // wins, so a script never starts after "cancelled before start" was recorded.
    private int _start;

    public bool TryStart() => Interlocked.CompareExchange(ref _start, 1, 0) == 0;

    public bool TryAbandon() => Interlocked.CompareExchange(ref _start, 2, 0) == 0;
  }

  public static async Task<object?> RunCodeAsync(JsonObject? parameters)
  {
    // The only command that runs arbitrary code: callers without this
    // session's token are refused before anything is read or logged.
    RpcAuth.Require(parameters);
    var total = Stopwatch.StartNew();
    var code = PluginRuntime.GetRequiredString(parameters, "code");
    if (code.Length > MaxCodeChars)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'code' is {code.Length:N0} characters; the limit is {MaxCodeChars:N0}.");
    }

    var mode = (PluginRuntime.GetOptionalString(parameters, "mode") ?? "read").Trim().ToLowerInvariant();
    if (mode is not ("read" or "write"))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "'mode' must be 'read' or 'write'.");
    }

    var writeMode = mode == "write";
    var timeoutSeconds = PluginRuntime.GetOptionalInt(parameters, "timeoutSeconds") ?? DefaultTimeoutSeconds;
    if (timeoutSeconds < 1 || timeoutSeconds > MaxTimeoutSeconds)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'timeoutSeconds' must be 1 to {MaxTimeoutSeconds}.");
    }

    // The script starts within this many seconds of the request or not at all
    // (HOST_BUSY), so it never starts after the server stopped waiting. An
    // older server sends none and waits a fixed 120 s: keep budget + window
    // inside that.
    var startWithinSeconds = PluginRuntime.GetOptionalDouble(parameters, "startWithinSeconds") ?? Math.Max(5, 110 - timeoutSeconds);
    if (startWithinSeconds < 1 || startWithinSeconds > MaxStartWithinSeconds)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'startWithinSeconds' must be 1 to {MaxStartWithinSeconds}.");
    }

    var maxInlineBytes = PluginRuntime.GetOptionalInt(parameters, "maxInlineBytes") ?? DefaultMaxInlineBytes;
    if (maxInlineBytes < MinInlineBytes || maxInlineBytes > MaxInlineBytes)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'maxInlineBytes' must be {MinInlineBytes} to {MaxInlineBytes}.");
    }

    var args = parameters?["args"] switch
    {
      null => new JsonObject(),
      JsonObject value => (JsonObject)value.DeepClone(),
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "'args' must be a JSON object."),
    };

    // Log the script before anything runs, so it is on disk even if Civil 3D hangs.
    var runId = $"{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
    var runFolder = RunCodeLog.CreateRunFolder(runId);
    RunCodeLog.WriteText(runFolder, RunCodeLog.ScriptFile, code);
    var summary = new JsonObject
    {
      ["runId"] = runId,
      ["startedUtc"] = DateTimeOffset.UtcNow.ToString("O"),
      ["mode"] = mode,
      ["timeoutSeconds"] = timeoutSeconds,
      ["startWithinSeconds"] = startWithinSeconds,
      ["args"] = args.DeepClone(),
      ["status"] = "started",
    };
    RunCodeLog.WriteSummary(runFolder, summary);
    PluginLog.Info("RunCode", $"Run {runId} ({mode}, {timeoutSeconds} s budget) started; script saved in {runFolder}.");
    if (Interlocked.Exchange(ref _pruned, 1) == 0)
    {
      _ = Task.Run(() => RunCodeLog.PruneOldOutputs(OutputRetention));
    }

    var response = new JsonObject
    {
      ["runId"] = runId,
      ["mode"] = mode,
      ["status"] = "started",
    };
    var warnings = new JsonArray();
    var outcome = new RunOutcome();
    var status = "failed";
    long compileMs = 0;

    try
    {
      RunCodeAssemblies.EnsureRoslynResolver();
      var compileClock = Stopwatch.StartNew();
      ScriptCompileResult compiled;
      try
      {
        compiled = await Task.Run(() => CompileScript(code, "C3dMcpRun_" + runId.Replace('-', '_'), writeMode));
      }
      catch (Exception exception) when (RunCodeAssemblies.IsRoslynLoadFailure(exception))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.RUN_CODE_UNAVAILABLE",
          $"The C# compiler could not be loaded in this Civil 3D ({exception.Message}). The plugin needs Microsoft.CodeAnalysis 4.8 or later, from Civil 3D's own folder or the bundle's roslyn folder.");
      }

      compileMs = compileClock.ElapsedMilliseconds;
      foreach (var warning in compiled.Warnings)
      {
        warnings.Add(DiagnosticJson(warning));
      }

      if (!compiled.Succeeded)
      {
        status = compiled.Errors.Any(error => error.Code == ScriptCompiler.GuardDiagnosticId) ? "rejected" : "compile_error";
        response["errors"] = new JsonArray(compiled.Errors.Select(error => (JsonNode)DiagnosticJson(error)).ToArray());
        return response;
      }

      var (scriptType, context) = RunCodeAssemblies.LoadScript(runId, compiled.Assembly!, compiled.Pdb!, compiled.TypeName);
      try
      {
        status = await ExecuteScriptAsync(scriptType, writeMode, args, TimeSpan.FromSeconds(timeoutSeconds),
          total, TimeSpan.FromSeconds(startWithinSeconds), runFolder, outcome, response);
        if (outcome.CancelledAfterCommit)
        {
          warnings.Add("The request was cancelled after the change was committed: it is in the drawing (one UNDO removes it).");
        }
        else if (outcome.Script != null && !outcome.ActionDone && !outcome.Committed)
        {
          warnings.Add("The request stopped waiting while the script was still running on Civil 3D's main thread; it runs until it returns or calls Check(), and a write is then rolled back.");
        }
        else if (outcome.TransactionEndedByScript)
        {
          warnings.Add("The script ended the run's transaction itself, so what is in the drawing is unknown: check it, and one UNDO reverts the whole run.");
        }
        else if (writeMode && !outcome.Committed && outcome.Script != null)
        {
          warnings.Add("The transaction was rolled back. Changes made outside Tr (database or system variables, files, Session) are not undone.");
        }

        // The run aborts them through wrappers of its own; the script's wrappers
        // stay armed and .NET ends them again later, on its finalizer thread.
        if (outcome.LeakedTransactions > 0)
        {
          warnings.Add($"The script left {outcome.LeakedTransactions} transaction(s) open; the run aborted them, with any changes made through them, but Civil 3D may crash later when .NET cleans them up: save the drawing and restart Civil 3D.");
        }
      }
      finally
      {
        // Collectible: the script's assembly goes away once nothing holds its
        // objects (Session entries of the script's own types keep it alive).
        context.Unload();
      }

      AddOutput(response, warnings, outcome, runFolder, maxInlineBytes);
      AddResult(response, outcome, runFolder, maxInlineBytes);
      if (status == "ok" && outcome.RunMs > timeoutSeconds * 1000L)
      {
        warnings.Add($"The script ran {outcome.RunMs / 1000.0:0.0} s, past its {timeoutSeconds} s budget: call Check() in long loops so the budget can stop it.");
      }

      try
      {
        var files = RunCodeLog.ScriptFiles(runFolder).Select(file => (JsonNode)file).ToArray();
        if (files.Length > 0)
        {
          response["files"] = new JsonArray(files);
        }
      }
      catch (Exception exception)
      {
        warnings.Add($"The files the script wrote could not be listed ({exception.Message}); see {runFolder}.");
      }

      return response;
    }
    catch (JsonRpcDispatchException exception) when (outcome.Script == null)
    {
      // The script never started (start window passed, Civil 3D busy with a
      // command or dialog, no drawing, no compiler): the client gets the
      // error, run.json records why.
      if (exception.Code == "CIVIL3D.HOST_BUSY")
      {
        status = "busy";
      }

      summary["reason"] = exception.Message;
      throw;
    }
    catch (OperationCanceledException)
    {
      // Only the request's cancellation gets here (a started script ends in
      // ExecuteScriptAsync's catch), nearly always while the run waited for
      // Civil 3D; TryAbandon makes sure it then never starts.
      status = "cancelled";
      if (outcome.TryAbandon())
      {
        summary["reason"] = "cancelled before start: the request was cancelled (the MCP client disconnected or gave up waiting) while it waited for Civil 3D; nothing ran.";
      }

      throw;
    }
    finally
    {
      response["status"] = status;
      response["ok"] = status == "ok";
      if (outcome.Drawing != null)
      {
        response["drawing"] = outcome.Drawing;
      }

      response["timing"] = new JsonObject
      {
        ["compileMs"] = compileMs,
        ["runMs"] = outcome.RunMs,
        ["totalMs"] = total.ElapsedMilliseconds,
      };
      if (warnings.Count > 0)
      {
        response["warnings"] = warnings;
      }

      response["logFolder"] = runFolder;
      // "unknown" when the script ended the run's transaction itself and the
      // run did not commit: its changes may or may not be in the drawing.
      var committed = outcome.TransactionEndedByScript && !outcome.Committed
        ? JsonValue.Create("unknown")
        : JsonValue.Create(outcome.Committed);
      if (writeMode)
      {
        response["committed"] = committed?.DeepClone();
      }

      summary["status"] = status;
      summary["drawing"] = outcome.Drawing;
      summary["finishedUtc"] = DateTimeOffset.UtcNow.ToString("O");
      summary["timing"] = response["timing"]!.DeepClone();
      summary["committed"] = committed;
      if (outcome.Script != null && !outcome.ActionDone && !outcome.Committed)
      {
        summary["note"] = "The request ended while the script was still running; a write is rolled back when it returns.";
      }
      foreach (var key in new[] { "errors", "error", "resultShape", "resultBytes", "resultFile", "outputFile", "files" })
      {
        if (response[key] != null)
        {
          summary[key] = response[key]!.DeepClone();
        }
      }

      RunCodeLog.WriteSummary(runFolder, summary);
      PluginLog.Info("RunCode", $"Run {runId} {status} in {total.ElapsedMilliseconds} ms (compile {compileMs} ms, run {outcome.RunMs} ms){(outcome.Drawing != null ? $" on {outcome.Drawing}" : string.Empty)}.");
    }
  }

  // Runs the script on the host's main thread through the plugin's usual
  // routes: read mode from an idle tick (open-close transaction, never
  // committed), write mode inside C3DMCPHOP. Write mode commits here rather
  // than in CivilExecution (write: false, inCommand: true keeps C3DMCPHOP and
  // its regular transaction), so the commit comes after the last checks and
  // outcome.Committed records what really happened. The script starts within
  // startWithin of the request (received) or not at all.
  private static async Task<string> ExecuteScriptAsync(Type scriptType, bool writeMode, JsonObject args, TimeSpan budget,
    Stopwatch received, TimeSpan startWithin, string runFolder, RunOutcome outcome, JsonObject response)
  {
    var token = PluginRuntime.GetCurrentRequestCancellationToken();
    try
    {
      await CivilExecution.ExecuteAsync((doc, civilDoc, database, transaction) =>
      {
        // Past the window the server may have stopped waiting, and an agent
        // told "timed out" could retry while this run still goes ahead. The
        // transaction ExecuteAsync opened is rolled back unused.
        if (received.Elapsed > startWithin)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.HOST_BUSY",
            $"The script waited {received.Elapsed.TotalSeconds:0} s for other Civil 3D work and was not started; nothing changed. Retry when Civil 3D is idle.");
        }

        // The request thread gave up first (cancelled while this was queued)
        // and recorded that nothing ran.
        if (!outcome.TryStart())
        {
          throw new RunCodeCancelledException("The request was cancelled before the script started.");
        }

        var script = (RunCodeScript)Activator.CreateInstance(scriptType)!;
        outcome.Script = script;
        outcome.Drawing = doc.Name;
        script.Attach(doc, database, civilDoc, transaction, writeMode, args, token, budget, runFolder);
        var clock = Stopwatch.StartNew();
        try
        {
          var value = script.Run();
          outcome.RunMs = clock.ElapsedMilliseconds;
          // Serialize now, while the objects in the result are still open.
          try
          {
            outcome.Result = RunCodeJson.Serialize(value, out var shape);
            outcome.ResultShape = shape;
          }
          catch (Exception exception)
          {
            outcome.ResultError = exception.Message;
          }

          // Check() tripped, but the script caught it and returned (or a lazy
          // result timed out while being written): still a timeout or a
          // cancellation, and a write is rolled back.
          if (script.Tripped != null)
          {
            throw script.Tripped;
          }

          if (writeMode)
          {
            // The request may have been cancelled while a long call ran and
            // Check() was never reached: roll back rather than commit a change
            // the client has been told failed.
            if (token.IsCancellationRequested)
            {
              throw new RunCodeCancelledException("The request was cancelled before the script finished; the transaction was rolled back.");
            }

            if (!RestoreRunTransaction(database, transaction, outcome))
            {
              throw new InvalidOperationException("The script committed, aborted or disposed the run's transaction itself (for example through Db.TransactionManager.TopTransaction); its changes may or may not be in the drawing. Return instead of ending it.");
            }

            transaction.Commit();
            outcome.Committed = true;
          }
        }
        catch (Exception)
        {
          outcome.RunMs = clock.ElapsedMilliseconds;
          if (writeMode && !outcome.Committed)
          {
            // Leave only the run's transaction open, so disposing it rolls
            // back everything the script did through Tr.
            try
            {
              RestoreRunTransaction(database, transaction, outcome);
            }
            catch (Exception cleanup)
            {
              PluginLog.Warn("RunCode", "Could not close transactions the script left open.", cleanup);
            }
          }

          // Rethrow: the uncommitted transaction is rolled back when disposed.
          throw;
        }
        finally
        {
          script.ReleaseOwned();
          outcome.ActionDone = true;
        }

        return true;
      }, write: false, inCommand: writeMode);
      return "ok";
    }
    catch (Exception exception) when (outcome.Script != null)
    {
      // Committed, then the client went away before the host finished (side
      // databases still closing, C3DMCPHOP ending): the change is in the
      // drawing, so this is a success, not a cancellation.
      if (outcome.Committed && exception is OperationCanceledException)
      {
        outcome.CancelledAfterCommit = true;
        return "ok";
      }

      response["error"] = ErrorJson(exception);
      // Cancelled means Check() saw the cancelled request, or the host wait
      // gave up on it; an OperationCanceledException of the script's own (an
      // HttpClient timeout, its own token) is a runtime error.
      return exception switch
      {
        RunCodeTimeoutException => "timeout",
        OperationCanceledException when exception is RunCodeCancelledException || token.IsCancellationRequested => "cancelled",
        _ => "runtime_error",
      };
    }
  }

  // Main thread, write mode. Aborts transactions the script started and left
  // open on the drawing, newest first, until the run's own transaction is on
  // top again (Commit only works on the top transaction). Returns false when
  // the run's transaction is no longer active: the script ended it some other
  // way than through Tr (a RunTransaction), for example via TopTransaction.
  // A backstop: the guard allows a nested transaction only as a using resource.
  private static bool RestoreRunTransaction(Database database, Transaction run, RunOutcome outcome)
  {
    // Ended through the run's own object (a script can only reach it by
    // reflection now): Commit and Abort leave AutoDelete false, Dispose zeroes
    // the pointer. Its old address may belong to a newer transaction by now,
    // so it is never compared.
    var ended = run.UnmanagedObject == IntPtr.Zero || !run.AutoDelete;
    var manager = database.TransactionManager;
    for (var guard = 0; guard < 100; guard++)
    {
      var top = manager.TopTransaction;
      if (top == null)
      {
        outcome.TransactionEndedByScript = true;
        return false;
      }

      if (!ended && top.UnmanagedObject == run.UnmanagedObject)
      {
        return true;
      }

      top.Abort();
      outcome.LeakedTransactions++;
    }

    outcome.TransactionEndedByScript = true;
    return false;
  }

  // Not inlined: this is the first method that touches Roslyn types, and the
  // resolver must be registered before the JIT needs them.
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static ScriptCompileResult CompileScript(string code, string assemblyName, bool writeMode)
  {
    return ScriptCompiler.Compile(code, assemblyName, RunCodeAssemblies.ReferencePaths(), writeMode);
  }

  private static void AddOutput(JsonObject response, JsonArray warnings, RunOutcome outcome, string runFolder, int maxInlineBytes)
  {
    var output = outcome.Script?.Output ?? string.Empty;
    if (outcome.Script?.OutputTruncated == true)
    {
      warnings.Add("Log output was cut at 4 M characters; write large output with WriteFile(...).");
    }

    if (output.Length == 0)
    {
      return;
    }

    if (Encoding.UTF8.GetByteCount(output) <= maxInlineBytes)
    {
      response["output"] = output;
      return;
    }

    // The run is over (a write already committed or rolled back): a file that
    // cannot be saved (full disk, folder gone) must not turn the reply into an
    // RPC error, or the caller could rerun a write that is already in the drawing.
    var saved = true;
    try
    {
      response["outputFile"] = RunCodeLog.WriteText(runFolder, RunCodeLog.OutputFile, output);
    }
    catch (Exception exception)
    {
      saved = false;
      warnings.Add($"The full output could not be saved to a file ({exception.Message}); only its start is shown.");
    }

    // Cut by characters, never between the two halves of a surrogate pair.
    var cut = Math.Min(output.Length, maxInlineBytes / 2);
    if (cut > 0 && char.IsHighSurrogate(output[cut - 1]))
    {
      cut--;
    }

    response["output"] = output[..cut] + (saved ? "\n... (cut; full output in outputFile)" : "\n... (cut)");
  }

  private static void AddResult(JsonObject response, RunOutcome outcome, string runFolder, int maxInlineBytes)
  {
    if (outcome.ResultError != null)
    {
      response["resultError"] = $"The return value could not be written as JSON: {outcome.ResultError}";
    }

    if (outcome.Result == null)
    {
      return;
    }

    response["resultShape"] = outcome.ResultShape;
    response["resultBytes"] = outcome.Result.Length;
    if (outcome.Result.Length <= maxInlineBytes)
    {
      response["result"] = JsonNode.Parse(outcome.Result);
      return;
    }

    response["result"] = null;
    try
    {
      response["resultFile"] = RunCodeLog.WriteBytes(runFolder, RunCodeLog.ResultFile, outcome.Result);
    }
    catch (Exception exception)
    {
      // As for the output: the run's outcome still goes back to the caller.
      response["resultError"] = $"The return value ({outcome.Result.Length} bytes) could not be saved to a file ({exception.Message}); only the preview is shown.";
    }

    response["resultPreview"] = Encoding.UTF8.GetString(outcome.Result, 0, Math.Min(outcome.Result.Length, PreviewChars)) + "...";
  }

  private static JsonObject DiagnosticJson(ScriptDiagnostic diagnostic)
  {
    return new JsonObject
    {
      ["line"] = diagnostic.Line,
      ["column"] = diagnostic.Column,
      ["code"] = diagnostic.Code,
      ["message"] = diagnostic.Message,
    };
  }

  private static JsonObject ErrorJson(Exception exception)
  {
    var error = new JsonObject
    {
      ["type"] = exception.GetType().FullName,
      ["message"] = exception.Message,
    };
    var line = ScriptCompiler.FindScriptLine(exception.StackTrace);
    if (line != null)
    {
      error["line"] = line;
    }

    if (exception is AcRuntimeException acadException)
    {
      error["errorStatus"] = acadException.ErrorStatus.ToString();
    }

    if (exception.InnerException != null)
    {
      error["inner"] = $"{exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
    }

    var stack = exception.StackTrace ?? string.Empty;
    error["stack"] = stack.Length > 2000 ? stack[..2000] + "..." : stack;
    return error;
  }
}
