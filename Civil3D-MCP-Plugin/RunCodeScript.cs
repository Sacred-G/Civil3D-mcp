using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using AcRuntimeException = Autodesk.AutoCAD.Runtime.Exception;

namespace Civil3DMcpPlugin;

/// <summary>
/// Base class of every runCode script (civil3d_run_code). The snippet becomes
/// the body of <see cref="Run"/>, so these members are in scope by name:
/// Doc, Db, Ed, Civil, Tr, Args, Log(...), Check(), Open&lt;T&gt;(id),
/// ModelSpace&lt;T&gt;(), OpenSide(path), TransactionFor(db), WriteFile(...),
/// RunFolder, Session.
///
/// The script runs on Civil 3D's main thread and cannot be stopped from
/// outside: call Check() in long loops (ModelSpace() does it for every entity)
/// so the time budget and a cancelled request end it.
/// </summary>
public abstract class RunCodeScript
{
  private const int MaxOutputChars = 4 * 1024 * 1024;

  private readonly StringBuilder _output = new();
  // Log runs on the main thread; Output can be read from the request's thread
  // when a cancelled request stops waiting while the script still runs.
  private readonly object _outputLock = new();
  // Disposed after the run, newest first: for each side database the script's
  // wrapper (detached only), its transaction, then the database.
  private readonly List<IDisposable> _owned = new();
  private readonly Dictionary<Database, Transaction> _transactions = new();
  private Stopwatch _clock = new();
  private TimeSpan _budget;
  // A script may point HostApplicationServices.WorkingDatabase at a side
  // database; it is set back before side databases are disposed.
  private Database? _savedWorkingDatabase;

  /// <summary>
  /// Kept across runs until Civil 3D closes: cache plain data between runs
  /// (arrays, lists, numbers, strings), for example geometry read from a
  /// background drawing. Never store DBObjects, ObjectIds of side databases or
  /// Databases: those are closed or disposed when the run ends.
  /// </summary>
  public static ConcurrentDictionary<string, object?> Session { get; } = new(StringComparer.OrdinalIgnoreCase);

  public Document Doc { get; private set; } = null!;
  public Database Db { get; private set; } = null!;
  public Editor Ed { get; private set; } = null!;
  public CivilDocument Civil { get; private set; } = null!;

  /// <summary>
  /// The run's transaction on <see cref="Db"/>. Read mode: an open-close
  /// transaction that is never committed. Write mode: a regular transaction,
  /// committed when Run returns (one Civil 3D undo step) and rolled back when
  /// it throws or runs out of time. Only changes made through Tr (and transactions
  /// nested in it) are rolled back: database and system variables, files and
  /// Session are not. Open-close transactions are refused in write mode, and
  /// a transaction the script starts must be the resource of a using.
  /// The run owns it: Commit() and Abort() throw, and Dispose() does nothing
  /// (RunTransaction).
  /// </summary>
  public Transaction Tr { get; private set; } = null!;

  public bool WriteMode { get; private set; }

  /// <summary>The tool call's "args" object (empty when none was given).</summary>
  public JsonObject Args { get; private set; } = new();

  public CancellationToken Token { get; private set; }

  /// <summary>This run's folder (script, summary, results); WriteFile writes here.</summary>
  public string RunFolder { get; private set; } = string.Empty;

  public TimeSpan Elapsed => _clock.Elapsed;

  public TimeSpan Budget => _budget;

  internal bool OutputTruncated { get; private set; }

  internal string Output
  {
    get
    {
      lock (_outputLock)
      {
        return _output.ToString();
      }
    }
  }

  /// <summary>
  /// The timeout or cancellation Check() threw, kept even when the script
  /// caught it and went on: the run still ends as timed out or cancelled, and
  /// a write is rolled back.
  /// </summary>
  internal Exception? Tripped { get; private set; }

  public abstract object? Run();

  internal void Attach(Document doc, Database db, CivilDocument civil, Transaction transaction, bool writeMode,
    JsonObject args, CancellationToken token, TimeSpan budget, string runFolder)
  {
    Doc = doc;
    Db = db;
    Ed = doc.Editor;
    Civil = civil;
    Tr = new RunTransaction(transaction);
    WriteMode = writeMode;
    Args = args;
    Token = token;
    RunFolder = runFolder;
    _budget = budget;
    _transactions[db] = Tr;
    _savedWorkingDatabase = HostApplicationServices.WorkingDatabase;
    _clock = Stopwatch.StartNew();
  }

  /// <summary>Appends a line to the run's output (returned as "output").</summary>
  public void Log(object? value)
  {
    var text = value?.ToString();
    lock (_outputLock)
    {
      if (RunCodeOutput.AppendLine(_output, text, MaxOutputChars))
      {
        OutputTruncated = true;
      }
    }
  }

  /// <summary>
  /// Ends the script when its time budget is used up or the request was
  /// cancelled. Call it in every long loop: nothing else can stop a script.
  /// </summary>
  public void Check()
  {
    if (Tripped != null)
    {
      throw Tripped;
    }

    if (Token.IsCancellationRequested)
    {
      throw Tripped = new RunCodeCancelledException("The request was cancelled (the MCP client disconnected or gave up waiting).");
    }

    if (_clock.Elapsed > _budget)
    {
      throw Tripped = new RunCodeTimeoutException($"The script used up its time budget of {_budget.TotalSeconds:0} s.");
    }
  }

  /// <summary>
  /// Opens an object through the run's transaction for its database (the
  /// drawing's or a side database's; an attached xref's objects through the
  /// drawing's, read only). forWrite needs mode 'write'.
  /// </summary>
  public T Open<T>(ObjectId id, bool forWrite = false) where T : DBObject
  {
    if (forWrite && !WriteMode)
    {
      throw new InvalidOperationException("Open(id, forWrite: true) needs mode 'write'.");
    }

    // Objects of an attached xref (its dependent block definitions and what is
    // in them) report the xref's database, which has no transaction of its own
    // in the run; the drawing's transaction opens them, for read only.
    var db = id.Database;
    Transaction transaction;
    if (db is { IsDisposed: false } && !_transactions.ContainsKey(db))
    {
      if (forWrite)
      {
        throw new InvalidOperationException($"Object {id.Handle} belongs to an attached xref (or another database this run did not open): it can be read, not opened for write.");
      }

      transaction = Tr;
    }
    else
    {
      transaction = TransactionFor(db);
    }

    var obj = transaction.GetObject(id, forWrite ? OpenMode.ForWrite : OpenMode.ForRead);
    return obj as T ?? throw new InvalidCastException($"Object {id.Handle} is a {obj.GetType().Name}, not a {typeof(T).Name}.");
  }

  /// <summary>
  /// Every entity of type T in model space (of the drawing, or of a side
  /// database), opened for read. Calls Check() for each entity.
  /// </summary>
  public IEnumerable<T> ModelSpace<T>(Database? db = null) where T : Autodesk.AutoCAD.DatabaseServices.Entity
  {
    db ??= Db;
    var transaction = TransactionFor(db);
    var table = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
    var modelSpace = (BlockTableRecord)transaction.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForRead);
    foreach (ObjectId id in modelSpace)
    {
      Check();
      if (transaction.GetObject(id, OpenMode.ForRead) is T entity)
      {
        yield return entity;
      }
    }
  }

  /// <summary>
  /// Reads another DWG as a side database (never opened in the editor; the
  /// file is not locked after this returns) with a transaction of its own
  /// (TransactionFor). Disposed when the run ends: the script gets a wrapper
  /// that does not own the database, so a using or Dispose() on it (refused
  /// at compile time) cannot free it while the run's transaction still holds
  /// its objects. Changes stay in memory unless the script saves the database
  /// somewhere, which needs mode 'write'.
  /// </summary>
  public Database OpenSide(string path)
  {
    var fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
      throw new FileNotFoundException($"Drawing not found: {fullPath}", fullPath);
    }

    var side = new Database(false, true);
    try
    {
      try
      {
        side.ReadDwgFile(fullPath, FileOpenMode.OpenForReadAndAllShare, true, string.Empty);
      }
      catch (AcRuntimeException)
      {
        // A drawing open in this session is write-locked; fall back to the
        // most permissive read share AutoCAD offers.
        side.ReadDwgFile(fullPath, FileOpenMode.OpenTryForReadShare, true, string.Empty);
      }

      side.CloseInput(true);
    }
    catch
    {
      side.Dispose();
      throw;
    }

    // The owning wrapper, freed by ReleaseOwned after the transaction that
    // holds its objects.
    _owned.Add(side);
    var transaction = side.TransactionManager.StartOpenCloseTransaction();
    // The real one is disposed by ReleaseOwned; the script only gets the wrapper.
    _owned.Add(transaction);
    // Keyed by the owning wrapper, which the script never sees, so the key
    // cannot be disposed; Equals/GetHashCode follow the native database, so
    // the script's wrapper and id.Database find the same entry.
    _transactions[side] = new RunTransaction(transaction);
    // The script's wrapper: autoDelete false, like the ones id.Database hands
    // out, so its Dispose() only detaches it and never deletes the database.
    // ReleaseOwned detaches it too, so one kept past the run holds no pointer.
    var alias = (Database)Autodesk.AutoCAD.Runtime.DisposableWrapper.Create(typeof(Database), side.UnmanagedObject, false);
    _owned.Add(alias);
    return alias;
  }

  /// <summary>The run's transaction for the drawing (Tr) or for a database from OpenSide.</summary>
  public Transaction TransactionFor(Database db)
  {
    // A disposed wrapper has no pointer, and 2027's Database.GetHashCode reads
    // the native database: never hash one (address 0).
    if (db is { IsDisposed: true })
    {
      throw new InvalidOperationException("That database was disposed, or comes from a run that has ended: use one from this run's OpenSide, and do not dispose it (the run disposes side databases when it ends).");
    }

    return _transactions.TryGetValue(db, out var transaction)
      ? transaction
      : throw new InvalidOperationException("That database has no transaction in this run: use Db or a database returned by OpenSide.");
  }

  /// <summary>Writes a text file into RunFolder (a file name, no folders) and returns its full path.</summary>
  public string WriteFile(string fileName, string contents) => RunCodeLog.WriteScriptFile(RunFolder, fileName, contents, null);

  /// <summary>Writes lines into a file in RunFolder and returns its full path.</summary>
  public string WriteFile(string fileName, IEnumerable<string> lines) => RunCodeLog.WriteScriptFile(RunFolder, fileName, null, lines);

  internal void ReleaseOwned()
  {
    // Never leave a disposed side database as the working database.
    try
    {
      if (_savedWorkingDatabase != null && HostApplicationServices.WorkingDatabase != _savedWorkingDatabase)
      {
        HostApplicationServices.WorkingDatabase = _savedWorkingDatabase;
      }
    }
    catch (Exception exception)
    {
      PluginLog.Swallow("RunCode", "restoring the working database", exception);
    }

    // Tr and the side transactions as the script sees them: detached before
    // the real ones end, so a wrapper kept past the run never reaches freed
    // memory. The run itself only uses the real transactions from here on.
    foreach (var wrapper in _transactions.Values.OfType<RunTransaction>())
    {
      try
      {
        wrapper.Release();
      }
      catch (Exception exception)
      {
        PluginLog.Swallow("RunCode", "releasing a transaction wrapper", exception);
      }
    }

    for (var index = _owned.Count - 1; index >= 0; index--)
    {
      try
      {
        _owned[index].Dispose();
      }
      catch (Exception exception)
      {
        PluginLog.Swallow("RunCode", "disposing a side database", exception);
      }
    }

    _owned.Clear();
    _transactions.Clear();
  }
}

/// <summary>
/// Tr and TransactionFor(...) as the script sees them. Every call goes to the
/// run's transaction, but Commit() and Abort() throw and Dispose() does
/// nothing, so an alias (var tx = Tr; tx.Commit(); or using var tx = Tr;) can
/// neither end it early nor leave a freed transaction that the next call would
/// dereference (a Civil 3D crash). The run alone commits, aborts and disposes
/// the real one. Members not overridden here (GetAllObjects, the RXObject
/// members) work on the same native transaction, exactly as on the real object;
/// OpenCloseTransaction does not override them either. When the run ends,
/// Release() zeroes this wrapper's pointer, so those members then fail with a
/// NullReferenceException instead of reading the freed transaction.
/// </summary>
internal sealed class RunTransaction : Transaction
{
  private readonly Transaction _run;
  private volatile bool _released;

  // autoDelete false: this wrapper never ends or frees the native transaction.
  internal RunTransaction(Transaction run) : base(run.UnmanagedObject, false)
  {
    _run = run;
  }

  // A wrapper kept past its run (in Session, or by a task still running)
  // must not reach the disposed transaction: that is a native crash.
  private Transaction Run => _released || _run.IsDisposed
    ? throw new InvalidOperationException("This transaction belongs to a run that has ended; use the current run's Tr.")
    : _run;

  // Main thread, when the run ends (ReleaseOwned). The base Dispose with
  // AutoDelete false only detaches (pointer zeroed, nothing freed): the
  // override below stays a no-op for the script.
  internal void Release()
  {
    _released = true;
    base.Dispose(true);
  }

  public override DBObject GetObject(ObjectId id, OpenMode mode) => Run.GetObject(id, mode);

  public override DBObject GetObject(ObjectId id, OpenMode mode, bool openErased) => Run.GetObject(id, mode, openErased);

  public override DBObject GetObject(ObjectId id, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer) =>
    Run.GetObject(id, mode, openErased, forceOpenOnLockedLayer);

  public override void AddNewlyCreatedDBObject(DBObject obj, bool add) => Run.AddNewlyCreatedDBObject(obj, add);

  public override int NumberOfOpenedObjects => Run.NumberOfOpenedObjects;

  public override Autodesk.AutoCAD.DatabaseServices.TransactionManager TransactionManager => Run.TransactionManager;

  public override void Commit() =>
    throw new InvalidOperationException("The run owns Tr (and TransactionFor's transactions): write mode commits Tr when the script returns; remove Commit().");

  public override void Abort() =>
    throw new InvalidOperationException("The run owns Tr (and TransactionFor's transactions): throw an exception to roll back; remove Abort().");

  protected override void Dispose(bool disposing)
  {
    // Nothing to do: the run disposes the real transaction.
  }
}

/// <summary>Thrown by Check() when the script's time budget is used up.</summary>
public sealed class RunCodeTimeoutException : Exception
{
  public RunCodeTimeoutException(string message) : base(message)
  {
  }
}

/// <summary>Thrown by Check() when the request was cancelled.</summary>
public sealed class RunCodeCancelledException : OperationCanceledException
{
  public RunCodeCancelledException(string message) : base(message)
  {
  }
}
