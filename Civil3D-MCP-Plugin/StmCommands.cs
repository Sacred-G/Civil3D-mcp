using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

/// <summary>
/// Backend commands for STM (Storm and Sanitary Analysis) import/export.
/// Uses SendStringToExecute for the AutoCAD command-line SSA commands.
/// These commands invoke Civil 3D's built-in Storm and Sanitary Analysis
/// dialog workflows via command line.
/// </summary>
public static class StmCommands
{
  // ──────────────────────────────────────────────
  //  Export to STM (Hydraflow Storm Sewers file)
  // ──────────────────────────────────────────────
  public static Task<object?> ExportStmAsync(JsonObject? parameters)
  {
    var filePath = PluginRuntime.GetOptionalString(parameters, "filePath");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // The ExportSSA command opens the Storm and Sanitary Analysis export dialog.
      // If a filePath is provided, we note it for the user; the dialog will still appear.
      var commandString = "ExportSSA\n";

      try
      {
        doc.SendStringToExecute(commandString, true, false, false);
      }
      catch (System.Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.COMMAND_FAILED",
          $"Failed to execute ExportSSA command: {ex.Message}");
      }

      return new Dictionary<string, object?>
      {
        ["command"] = "ExportSSA",
        ["status"] = "initiated",
        ["message"] = filePath != null
          ? $"ExportSSA dialog opened. Save the .STM file to: {filePath}"
          : "ExportSSA dialog opened. Choose a location to save the .STM file.",
        ["notes"] = new List<string>
        {
          "The ExportSSA command opens a dialog for exporting pipe network and catchment data to a Hydraflow Storm Sewers (.STM) file.",
          "The export includes pipe sizes, invert elevations, structure data, and catchment parameters.",
          "After export, the .STM file can be opened in Hydraflow Storm Sewers for detailed analysis.",
        },
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Import from STM
  // ──────────────────────────────────────────────
  public static Task<object?> ImportStmAsync(JsonObject? parameters)
  {
    var filePath = PluginRuntime.GetOptionalString(parameters, "filePath");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // The ImportSSA command opens the Storm and Sanitary Analysis import dialog.
      var commandString = "ImportSSA\n";

      try
      {
        doc.SendStringToExecute(commandString, true, false, false);
      }
      catch (System.Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.COMMAND_FAILED",
          $"Failed to execute ImportSSA command: {ex.Message}");
      }

      return new Dictionary<string, object?>
      {
        ["command"] = "ImportSSA",
        ["status"] = "initiated",
        ["message"] = filePath != null
          ? $"ImportSSA dialog opened. Import the .STM file from: {filePath}"
          : "ImportSSA dialog opened. Select the .STM file to import.",
        ["notes"] = new List<string>
        {
          "The ImportSSA command opens a dialog for importing Hydraflow Storm Sewers (.STM) analysis results back into Civil 3D.",
          "Imported data updates pipe network sizes, invert elevations, and catchment parameters.",
          "Part matching settings should be configured before importing to ensure correct pipe/structure mapping.",
        },
      };
    });
  }

  // ──────────────────────────────────────────────
  //  Open Storm and Sanitary Analysis
  // ──────────────────────────────────────────────
  public static Task<object?> OpenStormSanitaryAnalysisAsync(JsonObject? parameters)
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var commandString = "AnalyzeGravityNetwork\n";

      try
      {
        doc.SendStringToExecute(commandString, true, false, false);
      }
      catch (System.Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.COMMAND_FAILED",
          $"Failed to open Storm and Sanitary Analysis: {ex.Message}");
      }

      return new Dictionary<string, object?>
      {
        ["command"] = "AnalyzeGravityNetwork",
        ["status"] = "initiated",
        ["message"] = "Storm and Sanitary Analysis dialog opened for gravity network analysis.",
        ["notes"] = new List<string>
        {
          "Use this to perform detailed hydraulic analysis of gravity pipe networks.",
          "The analysis includes HGL/EGL calculations, capacity checks, and flow routing.",
          "Results can be exported to .STM format for further analysis.",
        },
      };
    });
  }

  // ──────────────────────────────────────────────
  //  List SSA Capabilities
  // ──────────────────────────────────────────────
  public static Task<object?> ListSsaCapabilitiesAsync()
  {
    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["domain"] = "storm_sanitary_analysis",
      ["operations"] = new List<Dictionary<string, object?>>
      {
        new()
        {
          ["name"] = "export_stm",
          ["command"] = "ExportSSA",
          ["status"] = "implemented",
          ["description"] = "Exports pipe network and catchment data to Hydraflow Storm Sewers (.STM) file format.",
          ["dialogRequired"] = true,
        },
        new()
        {
          ["name"] = "import_stm",
          ["command"] = "ImportSSA",
          ["status"] = "implemented",
          ["description"] = "Imports Storm and Sanitary Analysis results from .STM file back into Civil 3D.",
          ["dialogRequired"] = true,
        },
        new()
        {
          ["name"] = "open_ssa",
          ["command"] = "AnalyzeGravityNetwork",
          ["status"] = "implemented",
          ["description"] = "Opens the Storm and Sanitary Analysis dialog for gravity network analysis.",
          ["dialogRequired"] = true,
        },
      },
      ["notes"] = new List<string>
      {
        "STM import/export commands open dialogs that require user interaction for file selection.",
        "For fully automated STM workflows, consider using UI automation (Playwright/MCPControl) to handle dialog interactions.",
        "Part matching settings should be configured before importing STM data.",
      },
    });
  }
}
