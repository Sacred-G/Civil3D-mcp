using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

public static class CivilExecution
{
  private static readonly SemaphoreSlim HostExecutionGate = new(1, 1);

  // The AutoCAD main-thread context, captured in PluginEntry.Initialize. With no drawing open (Start tab)
  // DocumentManager.ExecuteInCommandContextAsync never runs its callback, so application-level work
  // (new drawing, list/switch documents) is posted here instead.
  private static SynchronizationContext? _hostContext;

  // Only a real UI context (WinForms/WPF) marshals to the main thread; the base SynchronizationContext
  // posts to the thread pool and must never be used for AutoCAD calls.
  internal static void CaptureHostContext()
  {
    var current = SynchronizationContext.Current;
    if (_hostContext != null || current == null || current.GetType() == typeof(SynchronizationContext))
    {
      return;
    }

    _hostContext = current;
    _hostThreadId = Environment.CurrentManagedThreadId;
    PluginLog.Info("CivilExecution", $"Main-thread context captured ({current.GetType().FullName}, thread {_hostThreadId}).");
  }

  private static int _hostThreadId;

  private static bool HasActiveDocument => App.DocumentManager.MdiActiveDocument != null;

  // Runs body on the host: in the active document's command context, or on the main thread when no
  // drawing is open (allowed only for application-level work). A request cancelled before the host
  // starts it is abandoned — the body never runs later — so one stuck call cannot wedge the queue.
  private static async Task RunOnHostAsync(Func<Task> body, bool requiresDocument)
  {
    if (!HasActiveDocument && (requiresDocument || _hostContext == null))
    {
      throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING",
        "No drawing is open in Civil 3D (Start tab). Open or create a drawing first.");
    }

    var state = 0;   // 0 pending, 1 abandoned, 2 started
    var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    async Task Guarded()
    {
      if (Interlocked.CompareExchange(ref state, 2, 0) != 0)
      {
        return;
      }

      CaptureHostContext();   // running on the main thread: remember its context for the no-drawing case
      try
      {
        await body();
        completion.TrySetResult();
      }
      catch (Exception ex)
      {
        completion.TrySetException(ex);
      }
    }

    if (HasActiveDocument)
    {
      async Task Submit()
      {
        try
        {
          await App.DocumentManager.ExecuteInCommandContextAsync(async _ => await Guarded(), null);
        }
        catch (Exception ex)
        {
          completion.TrySetException(ex);
        }
      }

      _ = Submit();
    }
    else
    {
      _hostContext!.Post(async _ =>
      {
        // Safety net: never touch AutoCAD off the main thread.
        if (Environment.CurrentManagedThreadId != _hostThreadId)
        {
          _hostContext = null;
          if (Interlocked.CompareExchange(ref state, 1, 0) == 0)
          {
            PluginLog.Warn("CivilExecution", "The captured context did not run on the main thread; disabled.");
            completion.TrySetException(new JsonRpcDispatchException("CIVIL3D.NO_DRAWING",
              "No drawing is open in Civil 3D (Start tab). Open or create a drawing first."));
          }

          return;
        }

        await Guarded();
      }, null);
    }

    var cancellationToken = PluginRuntime.GetCurrentRequestCancellationToken();
    using (cancellationToken.Register(() =>
    {
      if (Interlocked.CompareExchange(ref state, 1, 0) == 0)
      {
        completion.TrySetCanceled(cancellationToken);
      }
    }))
    {
      await completion.Task;
    }
  }

  public static async Task<T> ExecuteAsync<T>(Func<Document, CivilDocument, Database, Transaction, T> action, bool write)
  {
    return await ExecuteSerializedAsync(async () =>
    {
      T? result = default;
      Exception? capturedException = null;

      await RunOnHostAsync(async () =>
      {
        try
        {
          var doc = App.DocumentManager.MdiActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
          var expectedDrawingIdentity = PluginRuntime.GetExpectedDrawingIdentity();
          var activeDrawingIdentity = PluginRuntime.GetDrawingIdentity(doc);
          if (!string.IsNullOrWhiteSpace(expectedDrawingIdentity) &&
              !string.Equals(expectedDrawingIdentity, activeDrawingIdentity, StringComparison.OrdinalIgnoreCase))
          {
            throw new JsonRpcDispatchException(
              "CIVIL3D.CONFLICT",
              $"The active drawing changed from '{expectedDrawingIdentity}' to '{activeDrawingIdentity}' while the operation was queued. No drawing changes were made.");
          }
          var civilDoc = CivilApplication.ActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active Civil 3D document is available.");
          var database = doc.Database;

          using var documentLock = doc.LockDocument();
          using var transaction = database.TransactionManager.StartTransaction();

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

        await Task.CompletedTask;
      }, requiresDocument: true);

      if (capturedException != null)
      {
        throw capturedException;
      }

      return result!;
    });
  }

  public static async Task<T> ExecuteInCommandContextAsync<T>(Func<Task<T>> action)
  {
    return await ExecuteSerializedAsync(async () =>
    {
      T? result = default;
      Exception? capturedException = null;

      await RunOnHostAsync(async () =>
      {
        try
        {
          result = await action();
        }
        catch (Exception ex)
        {
          capturedException = ex;
        }
      }, requiresDocument: false);

      if (capturedException != null)
      {
        throw capturedException;
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

  private static async Task<T> ExecuteSerializedAsync<T>(Func<Task<T>> action)
  {
    var cancellationToken = PluginRuntime.GetCurrentRequestCancellationToken();
    PluginRuntime.QueueHostOperation();
    var started = false;

    try
    {
      await HostExecutionGate.WaitAsync(cancellationToken);
      started = true;
      PluginRuntime.StartHostOperation();
      cancellationToken.ThrowIfCancellationRequested();
      return await action();
    }
    finally
    {
      if (started)
      {
        PluginRuntime.CompleteHostOperation();
        HostExecutionGate.Release();
      }
      else
      {
        PluginRuntime.CancelQueuedHostOperation();
      }
    }
  }
}
