using System;
using Autodesk.AutoCAD.Runtime;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(Cad_AI_Agent.PluginEntry))]
[assembly: CommandClass(typeof(Cad_AI_Agent.PluginEntry))]
[assembly: CommandClass(typeof(Cad_AI_Agent.UI.PaletteManager))]

namespace Cad_AI_Agent
{
    public sealed class PluginEntry : IExtensionApplication
    {
        public void Initialize()
        {
            try
            {
                WriteMessage("[AI Agent] Plugin initialized. Run AIChat to open the palette.");
            }
            catch (System.Exception ex)
            {
                WriteMessage($"[AI Agent] Initialization error: {ex.Message}");
            }
        }

        public void Terminate()
        {
        }

        [CommandMethod("AIAGENTSTATUS")]
        public void ShowStatus()
        {
            WriteMessage("[AI Agent] Loaded. Run AIChat to open the palette.");
        }

        private static void WriteMessage(string message)
        {
            var doc = App.DocumentManager.MdiActiveDocument;
            doc?.Editor.WriteMessage($"\n{message}");
        }
    }
}
