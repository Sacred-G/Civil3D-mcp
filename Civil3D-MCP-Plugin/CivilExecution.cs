using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Civil3DMcpPlugin;

public static class CivilExecution
{
  // One host request at a time. A cancelled request returns at once, but the
  // gate stays closed until its host work has finished (see HostGate).
  private static readonly HostGate HostExecutionGate = new();

  // Where an operation runs. Nothing uses DocumentManager
  // .ExecuteInCommandContextAsync any more: on Civil 3D 2027 it stops running
  // callbacks for the rest of the session as soon as any COM client (another
  // process, or this plugin) has run a command with SendCommand (see
  // RunOnHostAsync). Reads run from an Application.Idle tick; everything that
  // changes the drawing, drives commands, or creates scratch objects runs
  // inside the plugin's own C3DMCPHOP command.
  private enum HostRoute
  {
    Idle,
    PluginCommand,
  }

  public static async Task<T> ExecuteAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action, bool write, bool inCommand = false, bool commandLine = false)
  {
    return await ExecuteSerializedAsync(async (hostRequest, cancellationToken) =>
    {
      // With zero documents open there is no command context, so the
      // ExecuteInCommandContextAsync callback below never runs and the
      // NO_DRAWING check inside it is unreachable. Hanging here would hold
      // HostExecutionGate forever and wedge every later host operation, so
      // fail fast before the hop. The inner check stays because the document
      // can still close while this request waits in the queue.
      if (App.DocumentManager.MdiActiveDocument == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      }

      T? result = default;
      Exception? capturedException = null;

      // Read on the request's thread: the expected identity is an AsyncLocal,
      // and the Idle and plugin-command routes run on the UI thread's
      // execution context, where it would read empty and skip the check.
      var expectedDrawingIdentity = PluginRuntime.GetExpectedDrawingIdentity();

      var hostTask = RunOnHostAsync(route =>
      {
        try
        {
          cancellationToken.ThrowIfCancellationRequested();
          var doc = RequireExpectedDocument(expectedDrawingIdentity, "No drawing changes were made.");
          var civilDoc = CivilApplication.ActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active Civil 3D document is available.");
          var database = doc.Database;

          using var documentLock = doc.LockDocument();
          // Outside a command (the Idle route), a document lock plus a regular
          // Transaction wipes the drawing's undo history even when it is only
          // read and aborted; an OpenCloseTransaction does not. Only plain reads
          // take the Idle route. Writes run in a command (PluginCommand) so they
          // are one undoable step, and so do reads that create scratch objects:
          // aborting a regular Transaction removes them again, aborting an
          // OpenCloseTransaction does not.
          using var transaction = route == HostRoute.Idle
            ? database.TransactionManager.StartOpenCloseTransaction()
            : database.TransactionManager.StartTransaction();

          result = action(doc, civilDoc, database, transaction);

          if (write)
          {
            transaction.Commit();
          }
        }
        catch (Exception ex)
        {
          capturedException = ex;
        }

        return Task.CompletedTask;
      }, needsCommand: write || inCommand, needsQuietEditor: commandLine, hostRequest, cancellationToken);

      await AwaitHostContextAsync(hostTask, cancellationToken);

      if (capturedException != null)
      {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(capturedException).Throw();
      }

      return result!;
    });
  }

  public static async Task<T> ExecuteInCommandContextAsync<T>(Func<Task<T>> action)
  {
    return await ExecuteSerializedAsync(async (hostRequest, cancellationToken) =>
    {
      T? result = default;
      Exception? capturedException = null;

      Func<object, Task> callback = async _ =>
      {
        try
        {
          cancellationToken.ThrowIfCancellationRequested();
          result = await action();
        }
        catch (Exception ex)
        {
          capturedException = ex;
        }
      };

      // Always run application-context work from the Idle hop, never from a
      // command context. The only caller is newDrawing, whose DocumentManager
      // .Add is an application-context API that needs no command context, and
      // choosing the hop by MdiActiveDocument was racy: if the last document
      // closed between the check and the hop, the command-context callback
      // would never run and the request would hang holding the gate.
      var hostTask = ExecuteInApplicationContextAsync(callback, hostRequest, cancellationToken);

      await AwaitHostContextAsync(hostTask, cancellationToken);

      if (capturedException != null)
      {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(capturedException).Throw();
      }

      return result!;
    });
  }

  public static Task<T> ReadAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action)
  {
    return ExecuteAsync(action, false);
  }

  public static Task<T> WriteAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action)
  {
    return ExecuteAsync(action, true);
  }

  // A read that needs a command around it: it creates scratch objects that
  // must disappear when the transaction is aborted (TinVolumeSurface.Create
  // for volumes), or it uses the pickfirst selection (Editor.SelectImplied
  // and SetImpliedSelection only work inside a command flagged UsePickSet).
  // The transaction is a regular one and is never committed. It may also
  // queue command-line input with QueueCommandLine.
  public static Task<T> ReadInCommandAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action)
  {
    return ExecuteAsync(action, false, inCommand: true);
  }

  // A plain read that then runs AutoCAD commands by queueing command-line
  // input with QueueCommandLine (undo, redo, and commands that open their own
  // dialogs). Waits for a quiescent editor so the input is not typed into a
  // prompt.
  public static Task<T> ReadThenCommandLineAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action)
  {
    return ExecuteAsync(action, false, commandLine: true);
  }

  // Runs AutoCAD command-line input and nothing else: no document lock and no
  // transaction, because on the Idle route even a read under a lock empties
  // the redo stack (seen live: UNDO, one plugin read, then MREDO said "Nothing
  // to redo"). Used by undo and redo. Waits for a quiescent editor.
  public static async Task<T> RunCommandLineAsync<T>(string input, T result)
  {
    return await ExecuteSerializedAsync(async (hostRequest, cancellationToken) =>
    {
      if (App.DocumentManager.MdiActiveDocument == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      }

      var expectedDrawingIdentity = PluginRuntime.GetExpectedDrawingIdentity();
      var hostTask = RunOnHostAsync(_ =>
      {
        cancellationToken.ThrowIfCancellationRequested();
        RequireExpectedDocument(expectedDrawingIdentity, "No commands were run.");
        // UNDO / MREDO never open a dialog, so the request waits until they
        // have run instead of reporting them done while they still execute.
        QueueCommandLine(input, waitForCompletion: true);
        return Task.CompletedTask;
      }, needsCommand: false, needsQuietEditor: true, hostRequest, cancellationToken);

      await AwaitHostContextAsync(hostTask, cancellationToken);
      return result;
    });
  }

  // Command-line input requested by the running host operation; sent through
  // COM SendCommand once the operation (and C3DMCPHOP) has finished. Only
  // touched on the main thread, one host operation at a time.
  private static readonly List<(string Text, bool Wait)> PendingCommandLine = new();

  // Main thread, inside a host operation: run this command-line input (for
  // example "_.UNDO 2 ") right after the operation. Document.SendStringToExecute
  // cannot be used: queued input mixes with C3DMCPHOP's COM input (seen live:
  // UNDO received "C3DMCPHOP" as its answer instead of the count).
  // waitForCompletion: the request waits until the input has run (up to
  // HostBusyTimeout). Without it the request stops waiting after a few
  // seconds, for commands that open a dialog and keep it open.
  public static void QueueCommandLine(string input, bool waitForCompletion = false)
  {
    // COM SendCommand needs a trailing space (or Enter) to submit the input.
    PendingCommandLine.Add((input.TrimEnd('\r', '\n', ' ') + " ", waitForCompletion));
  }

  private static Document RequireExpectedDocument(string? expectedDrawingIdentity, string noChanges)
  {
    var doc = App.DocumentManager.MdiActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
    var activeDrawingIdentity = PluginRuntime.GetDrawingIdentity(doc);
    if (!string.IsNullOrWhiteSpace(expectedDrawingIdentity) &&
        !string.Equals(expectedDrawingIdentity, activeDrawingIdentity, StringComparison.OrdinalIgnoreCase))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.CONFLICT",
        $"The active drawing changed from '{expectedDrawingIdentity}' to '{activeDrawingIdentity}' while the operation was queued. {noChanges}");
    }

    return doc;
  }

  // How long a running command, a prompt (for work that needs a command) or a
  // modal dialog may hold the host before the request fails with HOST_BUSY.
  private static readonly TimeSpan HostBusyTimeout = TimeSpan.FromSeconds(15);

  // Runs the operation on the host's main thread exactly once, from an
  // Application.Idle tick:
  //  - reads (needsCommand false) run in the Idle tick itself (application
  //    context, with their own document lock);
  //  - everything else is queued for the plugin's C3DMCPHOP command, which the
  //    Idle tick starts with COM SendCommand from a background thread.
  //
  // Why not DocumentManager.ExecuteInCommandContextAsync, the usual route:
  // verified on Civil 3D 2027 (2026-10-11, diagnostic build, Windows session
  // disconnected, only this plugin loaded), the first COM SendCommand from any
  // client stops it for the rest of the session. That includes PowerShell,
  // other plugins' automation, and this plugin's own C3DMCPHOP start. Queued
  // callbacks never start their EXECUTEFUNCTION command, Application.Idle keeps
  // firing, CMDACTIVE is 0, the editor is quiescent, and nothing in or outside
  // the process revives it (SendStringToExecute, PostCommandPrompt, more COM
  // commands, WM_NULL, focus, injected mouse and key input). The earlier
  // design detected that state after 5 s and switched to these routes; using
  // them from the start removes the stall and the dependency. It also stops
  // plugin calls from cancelling a command or prompt the user has open, which
  // the command-context hop did.
  //
  // The watcher fails fast with CIVIL3D.HOST_BUSY instead of letting the
  // request time out after 120 s:
  //  - while a command is active (CMDACTIVE != 0) for HostBusyTimeout, or, for
  //    work that needs C3DMCPHOP, while the editor is not quiescent (a LISP
  //    prompt waits with CMDACTIVE 0; the command name must not be typed into
  //    it);
  //  - while no Idle tick arrives at all for HostBusyTimeout (a modal dialog).
  // The busy checks, the no-Idle watchdog and the claim flag come from
  // jorgegarcias60/Civil3D-mcp#1 and #2.
  //
  // hostRequest decides, once each, whether the host work starts and whether
  // the action runs, and settles when the host is done with the request. A
  // cancelled caller stops waiting at once, but a started action always runs
  // to its end before the request settles, and HostExecutionGate stays closed
  // until then.
  private static async Task RunOnHostAsync(Func<HostRoute, Task> operation, bool needsCommand, bool needsQuietEditor, HostRequestState hostRequest, CancellationToken cancellationToken)
  {
    var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    async Task RunOnce(HostRoute route)
    {
      if (!hostRequest.TryClaimOperation())
      {
        return;
      }

      PendingCommandLine.Clear();
      try
      {
        await operation(route);
        hostRequest.OperationFinished(null);
      }
      catch (Exception ex)
      {
        hostRequest.OperationFinished(ex);
      }
    }

    DateTime? busySince = null;
    // Last Idle tick (UTC ticks), read by the timer thread: no tick at all for
    // HostBusyTimeout means the host is inside a modal dialog or a long
    // operation.
    long lastIdleTicks = DateTime.UtcNow.Ticks;
    EventHandler? idle = null;

    // Ends the request before its host work started: nothing ran.
    void EndBeforeStart(Exception reason)
    {
      if (hostRequest.TryAbandonBeforeStart())
      {
        CoreApp.Idle -= idle;
        done.TrySetException(reason);
      }
    }

    idle = async (_, _) =>
    {
      // async void: an exception escaping this handler would take down the
      // host, so every failure ends the request instead.
      try
      {
        Interlocked.Exchange(ref lastIdleTicks, DateTime.UtcNow.Ticks);
        if (!hostRequest.IsWaiting)
        {
          CoreApp.Idle -= idle;
          return;
        }

        var now = DateTime.UtcNow;
        var busy = Convert.ToInt32(CoreApp.GetSystemVariable("CMDACTIVE")) != 0
          || ((needsCommand || needsQuietEditor) && App.DocumentManager.MdiActiveDocument?.Editor.IsQuiescent == false);
        if (busy)
        {
          busySince ??= now;
          if (now - busySince.Value >= HostBusyTimeout)
          {
            EndBeforeStart(new JsonRpcDispatchException(
              "CIVIL3D.HOST_BUSY",
              "Civil 3D has a command or prompt in progress. Finish it or press Esc in Civil 3D, then retry. No drawing changes were made."));
          }

          return;
        }
      }
      catch (Exception ex)
      {
        EndBeforeStart(ex);
        return;
      }

      // Started or abandoned exactly once: the watchdog and a cancellation
      // on other threads race this.
      CoreApp.Idle -= idle;
      if (!hostRequest.TryStart())
      {
        return;
      }

      try
      {
        // The COM document must be fetched on the main thread.
        var acadDocument = App.DocumentManager.MdiActiveDocument?.GetAcadDocument();
        if (needsCommand)
        {
          await StartPluginCommandAsync(() => RunOnce(HostRoute.PluginCommand), hostRequest, cancellationToken);
        }
        else
        {
          await RunOnce(HostRoute.Idle);
        }

        // Async work in C3DMCPHOP may still be running after SendCommand
        // returned; wait for the operation, then send any
        // command-line input it queued.
        await hostRequest.OperationDone;
        if (PendingCommandLine.Count > 0 && acadDocument != null)
        {
          var input = PendingCommandLine.ToList();
          PendingCommandLine.Clear();
          if (cancellationToken.IsCancellationRequested)
          {
            PluginLog.Info("Host", $"The request was cancelled; command-line input not sent: {string.Join(", ", input.Select(entry => entry.Text.Trim()))}.");
          }
          else
          {
            await SendCommandLineAsync(acadDocument, input, cancellationToken);
          }
        }

        // Settle before completing, so the request that resumes finds the
        // gate free to release.
        hostRequest.HostFinished();
        done.TrySetResult(true);
      }
      catch (Exception ex)
      {
        // Also when the operation had already claimed the request (it ran and
        // faulted, for example with CONFLICT from RequireExpectedDocument):
        // the request must end with this exception. An action that has not
        // run yet never will.
        hostRequest.TryAbandonOperation();
        if (!hostRequest.OperationClaimed || hostRequest.OperationDone.IsCompleted)
        {
          hostRequest.HostFinished();
        }

        done.TrySetException(ex);
      }
      finally
      {
        // An action that started (async work in C3DMCPHOP) is waited for even
        // when the request already ended, so the next request cannot run
        // beside it.
        if (hostRequest.OperationClaimed)
        {
          try
          {
            await hostRequest.OperationDone;
          }
          catch
          {
            // Reported through done above.
          }
        }

        hostRequest.HostFinished();
      }
    };
    CoreApp.Idle += idle;

    // Idle is raised when the host's message queue empties; a background
    // Civil 3D with nothing to process may not raise it again. Post a no-op
    // message to the main window while the request waits so the watcher keeps
    // getting ticks. PostMessage is safe from any thread.
    IntPtr mainWindow;
    using (var host = System.Diagnostics.Process.GetCurrentProcess())
    {
      mainWindow = host.MainWindowHandle;
    }
    using var nudge = new Timer(_ =>
    {
      if (!hostRequest.IsWaiting)
      {
        return;
      }

      var sinceIdle = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - Interlocked.Read(ref lastIdleTicks));
      if (sinceIdle >= HostBusyTimeout)
      {
        EndBeforeStart(new JsonRpcDispatchException(
          "CIVIL3D.HOST_BUSY",
          "Civil 3D is not processing requests: a dialog is open or a long operation is running. Close the dialog or wait, then retry. No drawing changes were made."));
        return;
      }

      if (mainWindow != IntPtr.Zero)
      {
        PostMessage(mainWindow, 0 /* WM_NULL */, IntPtr.Zero, IntPtr.Zero);
      }
    }, null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));

    try
    {
      await AwaitHostContextAsync(done.Task, cancellationToken);
    }
    finally
    {
      // Request finished, failed or was cancelled: work that has not started
      // never will (an Idle handler still waiting for queued C3DMCPHOP work
      // stops waiting), while an action already running finishes first; the
      // request settles, and the gate opens, only then.
      hostRequest.End();
      CoreApp.Idle -= idle;
    }
  }

  [DllImport("user32.dll")]
  private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

  // ---------------------------------------------------------------------------
  // Plugin-command route
  // ---------------------------------------------------------------------------

  // Work waiting for the next C3DMCPHOP run. Each item runs once, inside the
  // command, where the document is locked by the command and a regular
  // Transaction is recorded as one undo step.
  private static readonly ConcurrentQueue<Func<Task>> PluginCommandWork = new();

  internal const string PluginCommandName = "C3DMCPHOP";

  // Awaited by PluginEntry's C3DMCPHOP command method (async void, on the main
  // thread in command context), so the command stays active while the work
  // awaits Editor.CommandAsync.
  internal static async Task RunPluginCommandWorkAsync()
  {
    while (PluginCommandWork.TryDequeue(out var work))
    {
      try
      {
        await work();
      }
      catch (Exception ex)
      {
        PluginLog.Error("Host", "C3DMCPHOP work failed outside its own error handling.", ex);
      }
    }
  }

  // Main thread (an Idle tick). Queues the work and starts C3DMCPHOP with COM
  // SendCommand from a background thread: AutoCAD marshals that call to the
  // main thread like an external client's and runs it at once, also in a
  // disconnected Windows session with nobody at the keyboard. Completes when SendCommand
  // has returned; the work itself reports through the request's own task.
  private static async Task StartPluginCommandAsync(Func<Task> work, HostRequestState hostRequest, CancellationToken cancellationToken)
  {
    var doc = App.DocumentManager.MdiActiveDocument
      ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
    // The COM document must be fetched on the main thread.
    var acadDocument = doc.GetAcadDocument();
    PluginCommandWork.Enqueue(work);

    try
    {
      await Task.Run(() => SendCommandWithRetry(acadDocument, "_" + PluginCommandName + " ", cancellationToken));
    }
    catch
    {
      // The send failed: the work must not run later from a stray
      // C3DMCPHOP, unless it already started.
      hostRequest.TryAbandonOperation();
      if (!hostRequest.OperationClaimed)
      {
        DropQueuedWork(work);
      }

      throw;
    }

    // SendCommand returned without the command running the work (or the
    // request was cancelled before it ran): stop it from ever running, drop
    // it from the queue, and report instead of waiting for the request
    // timeout.
    hostRequest.TryAbandonOperation();
    if (!hostRequest.OperationClaimed)
    {
      DropQueuedWork(work);
      throw new JsonRpcDispatchException(
        "CIVIL3D.HOST_BUSY",
        $"Civil 3D did not run the {PluginCommandName} command (another command or a dialog may be active). No drawing changes were made.");
    }
  }

  // Sends queued command-line input, one entry at a time, with COM
  // SendCommand from a background thread. SendCommand returns once AutoCAD
  // has consumed the input. A command that opens a dialog or stops at a
  // prompt keeps it from returning; once Civil 3D has accepted the call and
  // kept it running for a few seconds, the request completes anyway, so the
  // host gate is not held while the user works in the dialog. While Civil 3D
  // still rejects the call (SendCommandWithRetry retrying) the request keeps
  // waiting, so a send that never got through fails the request (HOST_BUSY
  // or the COM error) instead of being reported as started.
  // Input queued with waitForCompletion (UNDO, MREDO) is waited for up to
  // HostBusyTimeout; if it is still running then, the request fails with
  // HOST_BUSY instead of reporting it done.
  // A cancelled request stops the retries of a rejected call.
  private static async Task SendCommandLineAsync(object acadDocument, IReadOnlyList<(string Text, bool Wait)> input, CancellationToken cancellationToken)
  {
    foreach (var (text, wait) in input)
    {
      var progress = new SendProgress();
      var call = Task.Run(() => SendCommandWithRetry(acadDocument, text, cancellationToken, progress));
      if (wait)
      {
        await Task.WhenAny(call, Task.Delay(HostBusyTimeout));
      }
      else
      {
        while (!call.IsCompleted && !progress.AcceptedFor(DialogGrace))
        {
          await Task.WhenAny(call, Task.Delay(100));
        }
      }

      if (!call.IsCompleted && wait)
      {
        progress.Detach();
        _ = call.ContinueWith(
          t => PluginLog.Warn("Host", $"Command-line input '{text.Trim()}' failed.", t.Exception),
          CancellationToken.None,
          TaskContinuationOptions.OnlyOnFaulted,
          TaskScheduler.Default);
        throw new JsonRpcDispatchException(
          "CIVIL3D.HOST_BUSY",
          $"'{text.Trim()}' did not finish within {HostBusyTimeout.TotalSeconds:0} s and may still be running. Check Civil 3D before retrying.");
      }

      if (!call.IsCompleted)
      {
        // Civil 3D has kept the accepted call running for DialogGrace (a
        // dialog). Should it still be rejected later, it is not retried.
        progress.Detach();
        _ = call.ContinueWith(
          t => PluginLog.Warn("Host", $"Command-line input '{text.Trim()}' failed.", t.Exception),
          CancellationToken.None,
          TaskContinuationOptions.OnlyOnFaulted,
          TaskScheduler.Default);
        PluginLog.Info("Host", $"Command-line input '{text.Trim()}' is still running (a dialog or prompt); not waiting for it.");
        return;
      }

      try
      {
        await call;
      }
      catch (Exception ex) when (ex is not JsonRpcDispatchException and not OperationCanceledException)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.COMMAND_FAILED",
          $"Civil 3D did not accept '{text.Trim()}': {ex.Message}");
      }
    }
  }

  private static readonly TimeSpan CallRejectedRetry = TimeSpan.FromSeconds(10);

  // How long command-line input without waitForCompletion may keep a call
  // that Civil 3D accepted running (a dialog) before the request completes.
  private static readonly TimeSpan DialogGrace = TimeSpan.FromSeconds(3);

  // Whether the current SendCommand attempt is in Civil 3D's hands (not
  // rejected) and since when, written by the sending thread; and whether the
  // request has stopped waiting for the call (then it is not retried).
  private sealed class SendProgress
  {
    private long _attemptStartedTicks;
    private int _detached;

    public bool IsDetached => Volatile.Read(ref _detached) != 0;

    public void Detach() => Interlocked.Exchange(ref _detached, 1);

    public void Attempting() => Interlocked.Exchange(ref _attemptStartedTicks, DateTime.UtcNow.Ticks);

    public void Rejected() => Interlocked.Exchange(ref _attemptStartedTicks, 0);

    public bool AcceptedFor(TimeSpan duration)
    {
      var started = Interlocked.Read(ref _attemptStartedTicks);
      return started != 0 && DateTime.UtcNow.Ticks - started >= duration.Ticks;
    }
  }

  // Retries a call Civil 3D rejects (busy) for up to CallRejectedRetry, but
  // never after the request was cancelled or has stopped waiting for this
  // call (progress detached): a later retry would run the command after the
  // caller was told it did not, or was told something else.
  private static void SendCommandWithRetry(object acadDocument, string text, CancellationToken cancellationToken, SendProgress? progress = null)
  {
    var deadline = DateTime.UtcNow + CallRejectedRetry;
    while (true)
    {
      try
      {
        progress?.Attempting();
        Civil3DCompatibility.InvokeComMethod(acadDocument, "SendCommand", text);
        return;
      }
      catch (COMException ex) when (IsCallRejected(ex))
      {
        progress?.Rejected();
        cancellationToken.ThrowIfCancellationRequested();
        if (progress?.IsDetached == true)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.HOST_BUSY",
            $"Civil 3D rejected '{text.Trim()}' after the request had stopped waiting for it; it was not sent again.");
        }

        if (DateTime.UtcNow >= deadline)
        {
          // Still busy after the retries: the designed HOST_BUSY, not a raw
          // RPC_E_CALL_REJECTED / RPC_E_SERVERCALL_RETRYLATER.
          throw new JsonRpcDispatchException(
            "CIVIL3D.HOST_BUSY",
            $"Civil 3D kept rejecting '{text.Trim()}' for {CallRejectedRetry.TotalSeconds:0} s (another command or a dialog may be active). Finish it or press Esc in Civil 3D, then retry.");
        }

        Thread.Sleep(200);
      }
    }
  }

  private static bool IsCallRejected(COMException ex) =>
    ex.HResult is unchecked((int)0x80010001) /* RPC_E_CALL_REJECTED */ or unchecked((int)0x8001010A) /* RPC_E_SERVERCALL_RETRYLATER */;

  private static void DropQueuedWork(Func<Task> work)
  {
    var keep = new List<Func<Task>>();
    while (PluginCommandWork.TryDequeue(out var queued))
    {
      if (!ReferenceEquals(queued, work)) keep.Add(queued);
    }

    foreach (var queued in keep)
    {
      PluginCommandWork.Enqueue(queued);
    }
  }

  // Runs the callback on the host's main thread via a one-shot Application.Idle
  // handler and completes when the async callback has finished, propagating any
  // exception it throws.
  //
  // Verified live on Civil 3D 2027 with zero documents open:
  // DocumentManager.ExecuteInApplicationContext never invoked its callback in
  // that state (the request timed out after 120 s with no document created),
  // whereas Application.Idle keeps firing while only the Start tab is showing.
  // Idle runs on the main thread, which is the context DocumentManager.Add needs.
  // hostRequest settles when the callback has finished, so a request
  // cancelled while the callback runs keeps HostExecutionGate closed until
  // then.
  private static Task ExecuteInApplicationContextAsync(Func<object, Task> callback, HostRequestState hostRequest, CancellationToken cancellationToken)
  {
    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    EventHandler? handler = null;
    handler = async (_, _) =>
    {
      // One-shot: detach before running so a slow callback cannot be re-entered
      // by the next idle tick.
      CoreApp.Idle -= handler;
      if (!hostRequest.TryStart())
      {
        return;
      }

      Exception? failure = null;
      var ran = hostRequest.TryClaimOperation();
      if (ran)
      {
        try
        {
          await callback(null!);
        }
        catch (Exception ex)
        {
          failure = ex;
        }
      }

      // Settle before completing, so the request that resumes finds the gate
      // free to release.
      hostRequest.OperationFinished(failure);
      hostRequest.HostFinished();
      if (!ran)
      {
        completion.TrySetCanceled();
      }
      else if (failure != null)
      {
        completion.TrySetException(failure);
      }
      else
      {
        completion.TrySetResult(true);
      }
    };
    CoreApp.Idle += handler;

    // A request cancelled before the next idle tick (client disconnect while
    // Civil 3D is busy) must not leave its handler subscribed; otherwise
    // abandoned handlers accumulate until Idle next fires. Unsubscribe and
    // complete as cancelled so the awaiting request observes it promptly.
    // Once the callback has started it runs to its end.
    var cancellation = cancellationToken.Register(() =>
    {
      if (hostRequest.TryAbandonBeforeStart())
      {
        CoreApp.Idle -= handler;
        completion.TrySetCanceled(cancellationToken);
      }
    });
    _ = completion.Task.ContinueWith(_ => cancellation.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    return completion.Task;
  }

  // Waits for a host-context hop without ignoring request cancellation. A
  // client disconnect cancels the request token (RpcTcpServer monitors the
  // socket); without this, a hop whose callback never fires would keep
  // the client waiting after it has already gone away. Work that has not
  // started by then never starts (HostRequestState.End); work already
  // running finishes, and HostExecutionGate stays closed until it has. Its
  // result is discarded.
  private static async Task AwaitHostContextAsync(Task hostTask, CancellationToken cancellationToken)
  {
    if (hostTask.IsCompleted || !cancellationToken.CanBeCanceled)
    {
      await hostTask;
      return;
    }

    using var abandon = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var abandonTask = Task.Delay(Timeout.Infinite, abandon.Token);
    var completed = await Task.WhenAny(hostTask, abandonTask);
    if (completed != hostTask)
    {
      cancellationToken.ThrowIfCancellationRequested();
    }

    // Release the abandon registration so it does not outlive the request.
    abandon.Cancel();
    await hostTask;
  }

  private static async Task<T> ExecuteSerializedAsync<T>(Func<HostRequestState, CancellationToken, Task<T>> action)
  {
    var cancellationToken = PluginRuntime.GetCurrentRequestCancellationToken();
    var operationName = PluginRuntime.GetCurrentRequestOperation();
    PluginRuntime.QueueHostOperation();
    var started = false;

    try
    {
      // The gate opens for the next request when this request's host work
      // has settled, which can be after this returns: a cancelled request
      // returns its error at once while an action that already started on the
      // main thread runs to its end. The operation then still counts as
      // active in getCivil3DHealth.
      return await HostExecutionGate.RunAsync(
        hostRequest => action(hostRequest, cancellationToken),
        cancellationToken,
        onEntered: () =>
        {
          started = true;
          PluginRuntime.StartHostOperation();
        },
        onReleased: PluginRuntime.CompleteHostOperation,
        onDeferred: () => PluginLog.Warn("Host", $"'{operationName}' ended while its work was still running in Civil 3D; the next request waits until that work finishes."));
    }
    finally
    {
      if (!started)
      {
        PluginRuntime.CancelQueuedHostOperation();
      }
    }
  }
}
