using System.Collections.Concurrent;
using System.Diagnostics;
using Civil3DMcpPlugin;

// Offline checks of HostRequestState and HostGate, the part of
// CivilExecution that decides when the next host request may use the
// drawing. SimulatedHost below drives them the way CivilExecution's
// RunOnHostAsync does (an Idle tick on one main thread, optionally a
// C3DMCPHOP command run later on the same thread), without AutoCAD.
//
// The property under test: a request whose caller has stopped waiting
// (cancelled) returns its error at once, but the next request does not start
// until the first one's action has finished or provably never started.

var failures = 0;
var checks = 0;

void Expect(bool condition, string name)
{
  checks++;
  if (condition)
  {
    Console.WriteLine($"PASS {name}");
  }
  else
  {
    failures++;
    Console.WriteLine($"FAIL {name}");
  }
}

static async Task<bool> CompletesWithin(Task task, int milliseconds) =>
  await Task.WhenAny(task, Task.Delay(milliseconds)) == task;

static async Task<Exception?> Fault(Task task)
{
  try
  {
    await task;
    return null;
  }
  catch (Exception ex)
  {
    return ex;
  }
}

// 1. Cancelled before the Idle tick: nothing ran, the gate opens at once.
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  var unblock = main.Block();
  using var cts = new CancellationTokenSource();
  var ran = false;
  var first = host.Request(() => { ran = true; return Task.CompletedTask; }, viaCommand: false, cts.Token);
  await Task.Delay(50);
  cts.Cancel();
  Expect(await CompletesWithin(Fault(first), 1000) && first.IsCanceled, "cancelled before the Idle tick: the caller gets the cancellation at once");
  var second = host.Request(() => Task.CompletedTask, viaCommand: false, CancellationToken.None);
  Expect(host.EnteredCount == 2 || await WaitFor(() => host.EnteredCount == 2, 1000), "cancelled before the Idle tick: the gate opens for the next request at once");
  unblock();
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, "cancelled before the Idle tick: the next request runs");
  await main.Drain();
  Expect(!ran, "cancelled before the Idle tick: the action never runs, even when the tick comes later");
}

// 2. Cancelled while an async action runs on the main thread (C3DMCPHOP work
//    awaiting Editor.CommandAsync, or run_code): the caller returns at once,
//    the next request waits until the action has finished.
foreach (var viaCommand in new[] { false, true })
{
  var route = viaCommand ? "C3DMCPHOP" : "Idle";
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  using var cts = new CancellationTokenSource();
  var actionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  var finishAction = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  var firstFinished = false;
  var secondSawFirstFinished = (bool?)null;
  var first = host.Request(async () =>
  {
    actionStarted.SetResult();
    await finishAction.Task;
    firstFinished = true;
  }, viaCommand, cts.Token);
  await actionStarted.Task;
  cts.Cancel();
  Expect(await CompletesWithin(Fault(first), 1000) && first.IsCanceled, $"{route}: cancelled during an async action, the caller gets the cancellation at once");
  var second = host.Request(() => { secondSawFirstFinished = firstFinished; return Task.CompletedTask; }, viaCommand, CancellationToken.None);
  await Task.Delay(300);
  Expect(host.EnteredCount == 1 && !second.IsCompleted, $"{route}: the next request waits while the cancelled action still runs");
  finishAction.SetResult();
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, $"{route}: the next request runs once the cancelled action has finished");
  Expect(secondSawFirstFinished == true, $"{route}: the next request's action starts only after the cancelled one ended");
  Expect(host.MaxConcurrentActions == 1, $"{route}: never two actions at once");
}

// 3. Cancelled while a synchronous action blocks the main thread (a long
//    read, a write's transaction).
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  using var cts = new CancellationTokenSource();
  using var actionStarted = new ManualResetEventSlim();
  using var finishAction = new ManualResetEventSlim();
  var first = host.Request(() =>
  {
    actionStarted.Set();
    finishAction.Wait();
    return Task.CompletedTask;
  }, viaCommand: false, cts.Token);
  actionStarted.Wait();
  cts.Cancel();
  Expect(await CompletesWithin(Fault(first), 1000) && first.IsCanceled, "synchronous action: the caller gets the cancellation while the main thread is still busy");
  var second = host.Request(() => Task.CompletedTask, viaCommand: false, CancellationToken.None);
  await Task.Delay(300);
  Expect(host.EnteredCount == 1, "synchronous action: the gate stays closed while it runs");
  finishAction.Set();
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, "synchronous action: the next request runs after it");
}

// 4. C3DMCPHOP route, cancelled after the Idle tick sent the command but
//    before the command ran the work: the work never runs, and the gate opens
//    when the host is done (the command came and went), not before.
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  using var cts = new CancellationTokenSource();
  var commandSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  var runCommand = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
  host.BeforeCommand = async () =>
  {
    commandSent.TrySetResult();
    await runCommand.Task;
  };
  var ran = false;
  var first = host.Request(() => { ran = true; return Task.CompletedTask; }, viaCommand: true, cts.Token);
  await commandSent.Task;
  cts.Cancel();
  Expect(await CompletesWithin(Fault(first), 1000) && first.IsCanceled, "C3DMCPHOP pending: the caller gets the cancellation at once");
  host.BeforeCommand = null;
  var second = host.Request(() => Task.CompletedTask, viaCommand: true, CancellationToken.None);
  await Task.Delay(300);
  Expect(host.EnteredCount == 1, "C3DMCPHOP pending: the gate stays closed until the sent command has come and gone");
  runCommand.SetResult();
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, "C3DMCPHOP pending: the next request runs afterwards");
  Expect(!ran, "C3DMCPHOP pending: the cancelled work never runs");
}

// 5. The busy watcher or the no-Idle watchdog (a timer thread) ends the
//    request before the Idle tick: HOST_BUSY at once, nothing runs later.
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  var unblock = main.Block();
  var ran = false;
  host.Watchdog = state => state.TryAbandonBeforeStart();
  var first = host.Request(() => { ran = true; return Task.CompletedTask; }, viaCommand: false, CancellationToken.None);
  var error = await Fault(first);
  Expect(error is TimeoutException, "watchdog: the request fails with its busy error");
  host.Watchdog = null;
  var second = host.Request(() => Task.CompletedTask, viaCommand: false, CancellationToken.None);
  Expect(await WaitFor(() => host.EnteredCount == 2, 1000), "watchdog: the gate opens at once (nothing started)");
  unblock();
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, "watchdog: the next request runs");
  await main.Drain();
  Expect(!ran, "watchdog: the abandoned action never runs");
}

// 6. The action faults: the caller gets its exception and the gate opens.
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  var first = host.Request(() => throw new InvalidOperationException("boom"), viaCommand: true, CancellationToken.None);
  var error = await Fault(first);
  Expect(error is InvalidOperationException { Message: "boom" }, "faulted action: the caller gets the action's exception");
  var second = host.Request(() => Task.CompletedTask, viaCommand: true, CancellationToken.None);
  Expect(await CompletesWithin(second, 2000) && second.IsCompletedSuccessfully, "faulted action: the gate opens for the next request");
}

// 7. The one-time decisions under contention: exactly one side wins each.
{
  var startRaces = 0;
  var claimRaces = 0;
  var settledEarly = 0;
  for (var i = 0; i < 20000; i++)
  {
    var state = new HostRequestState();
    var results = new bool[2];
    Parallel.Invoke(() => results[0] = state.TryStart(), () => results[1] = state.TryAbandonBeforeStart());
    if (results[0] == results[1]) startRaces++;
    if (results[0])
    {
      if (state.Settled.IsCompleted) settledEarly++;
      var claims = new bool[2];
      Parallel.Invoke(() => claims[0] = state.TryClaimOperation(), () => claims[1] = state.TryAbandonOperation());
      if (claims[0] == claims[1]) claimRaces++;
      if (state.Settled.IsCompleted) settledEarly++;
      state.HostFinished();
    }
    else if (!state.Settled.IsCompleted || state.TryClaimOperation())
    {
      settledEarly++;
    }
  }

  Expect(startRaces == 0, "start vs abandon: exactly one wins every time");
  Expect(claimRaces == 0, "claim vs abandon: exactly one wins every time");
  Expect(settledEarly == 0, "settled exactly when nothing started, or when started work finished");
}

// 8. Many requests with random cancellation: never two actions at once, and
//    every request ends.
{
  using var main = new MainThread();
  var gate = new HostGate();
  var host = new SimulatedHost(main, gate);
  var random = new Random(7);
  var requests = new List<Task>();
  var stopwatch = Stopwatch.StartNew();
  for (var i = 0; i < 300; i++)
  {
    var cts = new CancellationTokenSource();
    var viaCommand = random.Next(2) == 0;
    var work = random.Next(4);
    requests.Add(host.Request(async () =>
    {
      if (work == 1) Thread.Sleep(1);
      if (work == 2) await Task.Delay(2);
      if (work == 3) await Task.Yield();
    }, viaCommand, cts.Token));
    cts.CancelAfter(random.Next(0, 6));
  }

  var all = Task.WhenAll(requests.Select(Fault));
  Expect(await CompletesWithin(all, 30000), "stress: every request ends");
  Expect(host.MaxConcurrentActions <= 1, $"stress: never two actions at once (max {host.MaxConcurrentActions}, {stopwatch.ElapsedMilliseconds} ms)");
}

Console.WriteLine($"{checks - failures}/{checks} host gate checks passed.");
return failures == 0 ? 0 : 1;

static async Task<bool> WaitFor(Func<bool> condition, int milliseconds)
{
  var deadline = DateTime.UtcNow.AddMilliseconds(milliseconds);
  while (DateTime.UtcNow < deadline)
  {
    if (condition()) return true;
    await Task.Delay(10);
  }

  return condition();
}

// Runs posted work one item at a time on its own thread, with a
// SynchronizationContext that brings awaits back to it, like Civil 3D's main
// thread.
sealed class MainThread : IDisposable
{
  private readonly BlockingCollection<Action> _queue = new();
  private readonly Thread _thread;

  public MainThread()
  {
    _thread = new Thread(() =>
    {
      SynchronizationContext.SetSynchronizationContext(new Context(this));
      foreach (var item in _queue.GetConsumingEnumerable())
      {
        item();
      }
    }) { IsBackground = true };
    _thread.Start();
  }

  public void Post(Action action) => _queue.Add(action);

  public void Post(Func<Task> action) => _queue.Add(() => _ = action());

  // Keeps the main thread busy (a modal dialog) until the returned action is
  // called.
  public Action Block()
  {
    var release = new ManualResetEventSlim();
    Post(() => release.Wait());
    return release.Set;
  }

  public async Task Drain()
  {
    for (var i = 0; i < 5; i++)
    {
      var tick = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      Post(() => tick.SetResult());
      await tick.Task;
      await Task.Delay(20);
    }
  }

  public void Dispose() => _queue.CompleteAdding();

  private sealed class Context : SynchronizationContext
  {
    private readonly MainThread _owner;

    public Context(MainThread owner) => _owner = owner;

    public override void Post(SendOrPostCallback d, object? state) => _owner.Post(() => d(state));
  }
}

// The request flow of CivilExecution.ExecuteSerializedAsync + RunOnHostAsync
// with the AutoCAD parts replaced: the Idle tick is an item on MainThread,
// and C3DMCPHOP (viaCommand) is a later item that runs the action.
sealed class SimulatedHost
{
  private readonly MainThread _main;
  private readonly HostGate _gate;
  private int _entered;
  private int _active;
  private int _maxActive;

  public SimulatedHost(MainThread main, HostGate gate)
  {
    _main = main;
    _gate = gate;
  }

  public int EnteredCount => Volatile.Read(ref _entered);

  public int MaxConcurrentActions => Volatile.Read(ref _maxActive);

  // Runs on MainThread before C3DMCPHOP runs the work.
  public Func<Task>? BeforeCommand { get; set; }

  // Called from another thread before the Idle tick; true ends the request
  // with a busy error (the busy watcher or the no-Idle watchdog).
  public Func<HostRequestState, bool>? Watchdog { get; set; }

  public Task Request(Func<Task> action, bool viaCommand, CancellationToken cancellationToken) =>
    _gate.RunAsync<bool>(
      async state =>
      {
        await RunOnHost(state, action, viaCommand, cancellationToken);
        return true;
      },
      cancellationToken,
      onEntered: () => Interlocked.Increment(ref _entered));

  private async Task RunOnHost(HostRequestState state, Func<Task> action, bool viaCommand, CancellationToken cancellationToken)
  {
    var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var beforeCommand = BeforeCommand;

    async Task RunOnce()
    {
      if (!state.TryClaimOperation())
      {
        return;
      }

      var active = Interlocked.Increment(ref _active);
      InterlockedMax(ref _maxActive, active);
      try
      {
        await action();
        state.OperationFinished(null);
      }
      catch (Exception ex)
      {
        state.OperationFinished(ex);
      }
      finally
      {
        Interlocked.Decrement(ref _active);
      }
    }

    // The Idle tick.
    _main.Post(async () =>
    {
      if (!state.IsWaiting || !state.TryStart())
      {
        return;
      }

      try
      {
        if (viaCommand)
        {
          // StartPluginCommandAsync: the command runs later on the main
          // thread; SendCommand returns once it has.
          var sent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
          _main.Post(async () =>
          {
            if (beforeCommand != null) await beforeCommand();
            var work = RunOnce();
            sent.SetResult();
            await work;
          });
          await sent.Task;
          state.TryAbandonOperation();
          if (!state.OperationClaimed)
          {
            throw new InvalidOperationException("C3DMCPHOP did not run the work.");
          }
        }
        else
        {
          await RunOnce();
        }

        await state.OperationDone;
        state.HostFinished();
        done.TrySetResult(true);
      }
      catch (Exception ex)
      {
        state.TryAbandonOperation();
        if (!state.OperationClaimed || state.OperationDone.IsCompleted)
        {
          state.HostFinished();
        }

        done.TrySetException(ex);
      }
      finally
      {
        if (state.OperationClaimed)
        {
          try
          {
            await state.OperationDone;
          }
          catch
          {
          }
        }

        state.HostFinished();
      }
    });

    if (Watchdog is { } watchdog && watchdog(state))
    {
      done.TrySetException(new TimeoutException("HOST_BUSY"));
    }

    try
    {
      // AwaitHostContextAsync.
      var completed = await Task.WhenAny(done.Task, Task.Delay(Timeout.Infinite, cancellationToken));
      if (completed != done.Task)
      {
        cancellationToken.ThrowIfCancellationRequested();
      }

      await done.Task;
    }
    finally
    {
      state.End();
    }
  }

  private static void InterlockedMax(ref int target, int value)
  {
    int current;
    while ((current = Volatile.Read(ref target)) < value && Interlocked.CompareExchange(ref target, value, current) != current)
    {
    }
  }
}
