using Autodesk.AutoCAD.ApplicationServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

public static partial class CivilExecution
{
  // Runs an async action that drives AutoCAD commands (Editor.CommandAsync)
  // for the active document, serialized behind the same host gate as every
  // other drawing operation. Unlike ExecuteAsync it deliberately opens no
  // transaction and takes no explicit document lock around the action: a
  // command such as -PLOT, -PUBLISH or -XREF must own the document itself,
  // and holding an open transaction across it is unsafe. Callers open short,
  // disposed transactions of their own around any reads they do before or
  // after issuing commands.
  public static async Task<T> ExecuteCommandSequenceAsync<T>(Func<Document, CancellationToken, Task<T>> action)
  {
    return await ExecuteSerializedAsync(async () =>
    {
      // Fail fast with no document: the command-context hop below never fires
      // without one, and the request would hold the host gate indefinitely.
      if (App.DocumentManager.MdiActiveDocument == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      }

      var cancellationToken = PluginRuntime.GetCurrentRequestCancellationToken();
      T? result = default;
      Exception? capturedException = null;

      await App.DocumentManager.ExecuteInCommandContextAsync(async _ =>
      {
        try
        {
          cancellationToken.ThrowIfCancellationRequested();
          var doc = App.DocumentManager.MdiActiveDocument ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
          var expectedDrawingIdentity = PluginRuntime.GetExpectedDrawingIdentity();
          var activeDrawingIdentity = PluginRuntime.GetDrawingIdentity(doc);
          if (!string.IsNullOrWhiteSpace(expectedDrawingIdentity) &&
              !string.Equals(expectedDrawingIdentity, activeDrawingIdentity, StringComparison.OrdinalIgnoreCase))
          {
            throw new JsonRpcDispatchException(
              "CIVIL3D.CONFLICT",
              $"The active drawing changed from '{expectedDrawingIdentity}' to '{activeDrawingIdentity}' while the operation was queued. No commands were run.");
          }

          result = await action(doc, cancellationToken);
        }
        catch (Exception ex)
        {
          capturedException = ex;
        }
      }, null);

      if (capturedException != null)
      {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(capturedException).Throw();
      }

      return result!;
    });
  }
}
