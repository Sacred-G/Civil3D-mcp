using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.IO;
using Cad_AI_Agent.UI;

namespace Cad_AI_Agent.UI
{
    public class PaletteManager
    {
        // We use static variables so that only one window is opened at a time
        private static PaletteSet? _chatPalette;
        private static AIChatPanel? _chatPanel;

        // New command to display this panel in Civil 3D
        [CommandMethod("AIChat")]
        public void OpenAIChat()
        {
            try
            {
                if (_chatPalette == null)
                {
                    _chatPalette = new PaletteSet("AI CAD Agent", new Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D"));
                    _chatPalette.Style = PaletteSetStyles.ShowPropertiesMenu |
                                         PaletteSetStyles.ShowAutoHideButton |
                                         PaletteSetStyles.ShowCloseButton;
                    _chatPalette.MinimumSize = new System.Drawing.Size(300, 500);
                    _chatPalette.DockEnabled = DockSides.Left | DockSides.Right;
                    _chatPanel = new AIChatPanel();
                    _chatPalette.AddVisual("Chat", _chatPanel);
                }

                _chatPalette.Visible = true;
            }
            catch (System.Exception ex)
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                string message = $"[AI Agent] Failed to open AIChat: {ex}\n";
                doc?.Editor.WriteMessage("\n" + message);

                try
                {
                    string logPath = Path.Combine(Path.GetTempPath(), "CadAiAgent_Error.log");
                    File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " " + message + Environment.NewLine);
                }
                catch { }
                _chatPanel = null;
                _chatPalette = null;
            }
        }
    }
}