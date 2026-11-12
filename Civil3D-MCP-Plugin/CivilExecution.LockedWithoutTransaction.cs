using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

namespace Civil3DMcpPlugin;

public static partial class CivilExecution
{
  // Same host gate, command context, drawing-identity check, and cancellation
  // handling as ExecuteCommandSequenceAsync (it is built on it, so fixes to
  // that plumbing apply here too), plus a document lock around the action
  // and WITHOUT an enclosing transaction. Some database operations (xref
  // unload/detach, data-shortcut repair) manage their own internal
  // transactions and must not run nested inside an open top transaction; the
  // callback opens short transactions of its own where it needs to read.
  public static Task<T> ExecuteLockedWithoutTransactionAsync<T>(Func<Document, CivilDocument, Database, T> action)
  {
    return ExecuteCommandSequenceAsync((doc, _) =>
    {
      var civilDoc = CivilApplication.ActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active Civil 3D document is available.");

      using var documentLock = doc.LockDocument();
      return Task.FromResult(action(doc, civilDoc, doc.Database));
    });
  }
}
