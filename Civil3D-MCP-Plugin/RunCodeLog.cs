using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

/// <summary>
/// Every file runCode writes, all inside its own folder next to plugin.log
/// (run-code\&lt;run id&gt;\): the script and its summary, large output and
/// results, and the files scripts write with WriteFile. Like PluginLog it
/// stays outside FileBoundary's import/export roots, because these are the
/// plugin's own logs; every path is checked to be inside run-code\.
/// </summary>
internal static class RunCodeLog
{
  internal const string ScriptFile = "script.cs";
  internal const string SummaryFile = "run.json";
  internal const string ResultFile = "result.json";
  internal const string OutputFile = "output.txt";

  // Kept when old run outputs are pruned.
  private static readonly string[] KeptFiles = { ScriptFile, SummaryFile };
  // Written by the plugin; a script's WriteFile may not use these names.
  private static readonly string[] ReservedFiles = { ScriptFile, SummaryFile, ResultFile, OutputFile };
  private static readonly UTF8Encoding Utf8NoBom = new(false);
  private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

  internal static string Root => Path.Combine(Path.GetDirectoryName(PluginLog.LogFilePath)!, "run-code");

  internal static string CreateRunFolder(string runId)
  {
    var folder = Inside(Path.Combine(Root, runId));
    Directory.CreateDirectory(folder);
    return folder;
  }

  internal static string WriteText(string runFolder, string fileName, string text)
  {
    var path = FilePath(runFolder, fileName);
    File.WriteAllText(path, text, Utf8NoBom);
    return path;
  }

  internal static string WriteBytes(string runFolder, string fileName, byte[] bytes)
  {
    var path = FilePath(runFolder, fileName);
    File.WriteAllBytes(path, bytes);
    return path;
  }

  // A file a script writes with WriteFile(name, ...).
  internal static string WriteScriptFile(string runFolder, string fileName, string? text, IEnumerable<string>? lines)
  {
    if (ReservedFiles.Contains(Path.GetFileName(fileName), StringComparer.OrdinalIgnoreCase))
    {
      throw new ArgumentException($"WriteFile cannot use '{fileName}': the plugin writes that file.");
    }

    var path = FilePath(runFolder, fileName);
    if (lines != null)
    {
      File.WriteAllLines(path, lines, Utf8NoBom);
    }
    else
    {
      File.WriteAllText(path, text ?? string.Empty, Utf8NoBom);
    }

    return path;
  }

  internal static void WriteSummary(string runFolder, JsonObject summary)
  {
    try
    {
      WriteText(runFolder, SummaryFile, summary.ToJsonString(Indented));
    }
    catch (Exception exception)
    {
      PluginLog.Warn("RunCode", $"Could not write the run summary in {runFolder}.", exception);
    }
  }

  // Files the script wrote with WriteFile (not the plugin's own).
  internal static IEnumerable<string> ScriptFiles(string runFolder)
  {
    return Directory.EnumerateFiles(runFolder)
      .Where(file => !ReservedFiles.Contains(Path.GetFileName(file), StringComparer.OrdinalIgnoreCase));
  }

  // Results and files written by scripts older than the retention are
  // deleted; every script.cs and run.json is kept.
  internal static void PruneOldOutputs(TimeSpan retention)
  {
    try
    {
      if (!Directory.Exists(Root))
      {
        return;
      }

      var cutoff = DateTime.UtcNow - retention;
      foreach (var directory in Directory.EnumerateDirectories(Root))
      {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
          if (!KeptFiles.Contains(Path.GetFileName(file), StringComparer.OrdinalIgnoreCase) && File.GetLastWriteTimeUtc(file) < cutoff)
          {
            File.Delete(Inside(file));
          }
        }
      }
    }
    catch (Exception exception)
    {
      PluginLog.Swallow("RunCode", "pruning old run outputs", exception);
    }
  }

  private static string FilePath(string runFolder, string fileName)
  {
    var name = Path.GetFileName(fileName);
    if (string.IsNullOrWhiteSpace(name) || name != fileName)
    {
      throw new ArgumentException($"Expected a plain file name (no folders), got '{fileName}'.");
    }

    return Inside(Path.Combine(runFolder, name));
  }

  private static string Inside(string path)
  {
    var full = Path.GetFullPath(path);
    var root = Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
    {
      throw new InvalidOperationException($"runCode writes only inside {root}; refused {full}.");
    }

    return full;
  }
}
