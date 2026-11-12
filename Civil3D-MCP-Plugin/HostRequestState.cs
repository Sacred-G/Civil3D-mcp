namespace Civil3DMcpPlugin;

// Who owns one host request's work on Civil 3D's main thread, and when the
// host has finished with it. No AutoCAD types, so tests/HostGateHarness runs
// it offline.
//
// Two decisions are made once each, whichever side gets there first:
//  - start: the Idle handler starts the host work (TryStart), or the request
//    ends before that (TryAbandonBeforeStart: busy watcher, no-Idle watchdog,
//    cancellation). Abandoned before start means nothing ran or will run.
//  - operation: the drawing action runs (TryClaimOperation, in the Idle tick
//    or in C3DMCPHOP), or it is stopped before it runs (TryAbandonOperation:
//    cancellation, or C3DMCPHOP did not run it).
// Settled completes when the host is done with the request: it was abandoned
// before start, or the started host work called HostFinished (after the
// action, and any command-line input it queued, has finished). HostGate keeps
// the next request out until then, even when this request's caller has
// already stopped waiting.
internal sealed class HostRequestState
{
  private const int Waiting = 0;
  private const int Taken = 1;
  private const int Abandoned = 2;

  private int _start;
  private int _operation;
  private readonly TaskCompletionSource<bool> _operationDone = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource<bool> _settled = new(TaskCreationOptions.RunContinuationsAsynchronously);

  // Completes when the action has finished (faulted with its exception), or
  // is cancelled when it was stopped before it ran.
  public Task OperationDone => _operationDone.Task;

  public Task Settled => _settled.Task;

  public bool IsWaiting => Volatile.Read(ref _start) == Waiting;

  public bool OperationClaimed => Volatile.Read(ref _operation) == Taken;

  // Idle handler: true when it may start the host work.
  public bool TryStart() => Interlocked.CompareExchange(ref _start, Taken, Waiting) == Waiting;

  // True when the request ended before its host work started.
  public bool TryAbandonBeforeStart()
  {
    if (Interlocked.CompareExchange(ref _start, Abandoned, Waiting) != Waiting)
    {
      return false;
    }

    Interlocked.CompareExchange(ref _operation, Abandoned, Waiting);
    _operationDone.TrySetCanceled();
    _settled.TrySetResult(true);
    return true;
  }

  // True when the action may run; at most once per request.
  public bool TryClaimOperation() => Interlocked.CompareExchange(ref _operation, Taken, Waiting) == Waiting;

  // True when the action was stopped before it ran. The host work that
  // started still calls HostFinished, so the request settles when it ends.
  public bool TryAbandonOperation()
  {
    if (Interlocked.CompareExchange(ref _operation, Abandoned, Waiting) != Waiting)
    {
      return false;
    }

    _operationDone.TrySetCanceled();
    return true;
  }

  public void OperationFinished(Exception? exception)
  {
    if (exception == null)
    {
      _operationDone.TrySetResult(true);
    }
    else
    {
      _operationDone.TrySetException(exception);
    }
  }

  // The started host work has nothing left to do for this request.
  public void HostFinished() => _settled.TrySetResult(true);

  // The request has ended (finished, failed or cancelled): whatever has not
  // started yet never will. Work already running is left to finish.
  public void End()
  {
    if (!TryAbandonBeforeStart())
    {
      TryAbandonOperation();
    }
  }
}

// Runs one host request at a time. A request whose caller stops waiting
// (cancelled, or the request failed while its action still runs) returns at
// once, but the gate opens for the next request only when the host has
// settled that request, so two requests never use the drawing together.
internal sealed class HostGate
{
  private readonly SemaphoreSlim _gate = new(1, 1);

  // onEntered runs once the request holds the gate; onReleased when the gate
  // opens again (possibly later, from another thread); onDeferred when the
  // request returns while its host work is still running.
  public async Task<T> RunAsync<T>(
    Func<HostRequestState, Task<T>> action,
    CancellationToken cancellationToken,
    Action? onEntered = null,
    Action? onReleased = null,
    Action? onDeferred = null)
  {
    await _gate.WaitAsync(cancellationToken);
    var state = new HostRequestState();
    try
    {
      onEntered?.Invoke();
      cancellationToken.ThrowIfCancellationRequested();
      return await action(state);
    }
    finally
    {
      state.End();
      if (state.Settled.IsCompleted)
      {
        Release(onReleased);
      }
      else
      {
        onDeferred?.Invoke();
        _ = state.Settled.ContinueWith(_ => Release(onReleased), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
      }
    }
  }

  private void Release(Action? onReleased)
  {
    try
    {
      onReleased?.Invoke();
    }
    finally
    {
      _gate.Release();
    }
  }
}
