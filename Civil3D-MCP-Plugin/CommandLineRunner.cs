using Autodesk.AutoCAD.ApplicationServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

/// <summary>
/// Runs a scripted AutoCAD command-line sequence (Editor.CommandAsync) from
/// host work started by <see cref="CivilExecution.ExecuteCommandSequenceAsync{T}"/>
/// and detects a command that was left waiting at a prompt.
/// </summary>
internal static class CommandLineRunner
{
  // A command left waiting for input is reported with this code so callers can
  // stop a batch instead of feeding the next answers into a stale prompt.
  internal const string IncompleteCode = "CIVIL3D.COMMAND_FAILED";

  // Invocations this runner started that have not been seen to finish: a
  // command still at a prompt when its tokens ran out (a cancel was queued) or
  // one whose CommandAsync failed while the command was still active. A normal
  // completion is removed at once, so a PLOT/PUBLISH/XREF the user starts later
  // is never mistaken for a stale request. Only the UI thread runs commands,
  // but the list is locked so a stray caller cannot corrupt it.
  private static readonly List<(Document Doc, string Command)> PendingInvocations = new();

  internal static async Task RunAsync(Document doc, string commandName, params object[] tokens)
  {
    // Editor.CommandAsync completes once its tokens are consumed, even when the
    // command is still waiting for more input (the acedCmdC coroutine model),
    // so a stuck command is detected below and a cancel is queued. That cancel
    // only runs after the host work returns, so check here that no command an
    // earlier request left at a prompt in this document is still there before
    // feeding it this request's answers.
    var stale = FindActiveCommand(doc, PendingCommandsFor(doc));
    if (stale != null)
    {
      var stalePrompt = Convert.ToString(App.GetSystemVariable("LASTPROMPT"));
      QueueCancel(doc);
      throw new JsonRpcDispatchException(
        IncompleteCode,
        $"-{stale} from an earlier request is still waiting at prompt '{stalePrompt}', so -{commandName} was not started. A cancel was queued; retry the request.");
    }

    var invocation = (doc, commandName);
    lock (PendingInvocations)
    {
      PendingInvocations.Add(invocation);
    }

    var stillActive = true;
    try
    {
      await doc.Editor.CommandAsync(tokens);
    }
    finally
    {
      // Forget the invocation once the command is no longer active (normal
      // completion, or a failure that ended it); keep it only while it may
      // still be at a prompt. If another drawing became active, CMDNAMES says
      // nothing about this one: keep the entry, and the next request in this
      // drawing checks it again.
      var active = ActiveCommandNames(doc);
      stillActive = active != null && active.Contains(commandName, StringComparer.OrdinalIgnoreCase);
      if (active != null && !stillActive)
      {
        lock (PendingInvocations)
        {
          PendingInvocations.Remove(invocation);
        }
      }
    }

    // A token the command did not expect (renamed prompt in a future release,
    // unexpected paper-size dialog, ...) leaves the command waiting for input.
    // Detect that instead of reporting success, record the pending prompt for
    // diagnosis, and queue a cancel so the editor is usable again.
    if (stillActive)
    {
      var lastPrompt = Convert.ToString(App.GetSystemVariable("LASTPROMPT"));
      QueueCancel(doc);
      throw new JsonRpcDispatchException(
        IncompleteCode,
        $"-{commandName} did not complete; it was waiting at prompt '{lastPrompt}'. A cancel was queued. The prompt chain may differ in this Civil 3D release.");
    }
  }

  // Commands this runner left unfinished in <paramref name="doc"/>. Entries
  // whose command is no longer active (the queued cancel ran) are dropped, and
  // so are entries for documents that have been closed. Entries are only
  // dropped as finished while <paramref name="doc"/> is the active drawing,
  // because CMDNAMES describes the active drawing only.
  private static string[] PendingCommandsFor(Document doc)
  {
    var active = ActiveCommandNames(doc);
    var open = App.DocumentManager.Cast<Document>().ToList();
    lock (PendingInvocations)
    {
      PendingInvocations.RemoveAll(entry =>
        !open.Contains(entry.Doc)
        || (active != null && ReferenceEquals(entry.Doc, doc) && !active.Contains(entry.Command, StringComparer.OrdinalIgnoreCase)));
      return PendingInvocations
        .Where(entry => ReferenceEquals(entry.Doc, doc))
        .Select(entry => entry.Command)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    }
  }

  // The commands active in <paramref name="doc"/>, or null when it is not the
  // active drawing (CMDNAMES is only known for the active drawing).
  private static string[]? ActiveCommandNames(Document doc)
  {
    if (!ReferenceEquals(App.DocumentManager.MdiActiveDocument, doc))
    {
      return null;
    }

    var activeCommands = Convert.ToString(App.GetSystemVariable("CMDNAMES")) ?? string.Empty;
    return activeCommands
      .Split('\'')
      .Select(name => name.TrimStart('-', '_', '.'))
      .Where(name => name.Length > 0)
      .ToArray();
  }

  private static string? FindActiveCommand(Document doc, IReadOnlyCollection<string> commandNames)
  {
    if (commandNames.Count == 0)
    {
      return null;
    }

    return ActiveCommandNames(doc)
      ?.FirstOrDefault(name => commandNames.Contains(name, StringComparer.OrdinalIgnoreCase));
  }

  private static void QueueCancel(Document doc)
  {
    try
    {
      doc.SendStringToExecute("\x03\x03", true, false, false);
    }
    catch
    {
      // Best effort; the caller still reports the stuck command.
    }
  }
}
