using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(Civil3DMcpPlugin.PluginEntry))]
[assembly: CommandClass(typeof(Civil3DMcpPlugin.PluginEntry))]

namespace Civil3DMcpPlugin;

public sealed class PluginEntry : IExtensionApplication
{
  public void Initialize()
  {
    try
    {
      PluginRuntime.StartServer();
      PluginLog.Info("PluginEntry", $"Civil3D MCP plugin initialized on port {PluginRuntime.Port}. Log file: {PluginLog.LogFilePath}");
      WriteMessage("Civil3D MCP plugin initialized.");
    }
    catch (System.Exception ex)
    {
      PluginLog.Error("PluginEntry", "Plugin failed to initialize", ex);
      WriteMessage($"Civil3D MCP plugin failed to initialize: {ex.Message}");
    }
  }

  public void Terminate()
  {
    try
    {
      PluginRuntime.StopServer();
      PluginLog.Info("PluginEntry", "Civil3D MCP plugin terminated cleanly.");
    }
    catch (System.Exception ex)
    {
      PluginLog.Error("PluginEntry", "Error during plugin termination", ex);
    }
  }

  [CommandMethod("C3DMCPSTART")]
  public void StartCommand()
  {
    PluginRuntime.StartServer();
    WriteMessage($"Civil3D MCP listener started on port {PluginRuntime.Port}.");
  }

  [CommandMethod("C3DMCPSTOP")]
  public void StopCommand()
  {
    PluginRuntime.StopServer();
    WriteMessage("Civil3D MCP listener stopped.");
  }

  [CommandMethod("C3DMCPSTATUS")]
  public void StatusCommand()
  {
    var status = PluginRuntime.GetStatus();
    WriteMessage($"Civil3D MCP listener running: {status.IsRunning}; pending: {status.QueueDepth}; active: {status.OperationInProgress}; current: {status.CurrentOperation ?? "<none>"}");
  }

  // Runs host work queued by CivilExecution (writes and reads that need a
  // command; see CivilExecution.RunOnHostAsync). Started over COM by the
  // plugin itself; typing it runs nothing when no work waits. UsePickSet lets
  // work read the pickfirst selection (Editor.SelectImplied returns an error
  // outside such a command); Redraw keeps that selection and its grips after
  // the command.
  [CommandMethod(CivilExecution.PluginCommandName, CommandFlags.Modal | CommandFlags.NoHistory | CommandFlags.NoMultiple | CommandFlags.UsePickSet | CommandFlags.Redraw)]
  public static async void HostHopCommand()
  {
    try
    {
      await CivilExecution.RunPluginCommandWorkAsync();
    }
    catch (System.Exception ex)
    {
      PluginLog.Error("PluginEntry", $"{CivilExecution.PluginCommandName} failed", ex);
    }
  }

  private static void WriteMessage(string message)
  {
    var doc = App.DocumentManager.MdiActiveDocument;
    doc?.Editor.WriteMessage($"\n{message}");
  }
}
