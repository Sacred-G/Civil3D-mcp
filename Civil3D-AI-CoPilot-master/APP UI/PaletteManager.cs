using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using Cad_AI_Agent.UI;

namespace Cad_AI_Agent.UI
{
    public class PaletteManager
    {
        // We use static variables so that only one window is opened at a time
        private static PaletteSet _chatPalette;
        private static AIChatPanel _chatPanel;

        // New command to display this panel in Civil 3D
        [CommandMethod("AIChat")]
        public void OpenAIChat()
        {
            if (_chatPalette == null)
            {
                // 1. Create the palette. GUID (long code) is required for AutoCAD to remember the window size and position for the next launch.
                _chatPalette = new PaletteSet("AI CAD Agent", new Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D"));

                // 2. Enable docking and auto-hide functions
                _chatPalette.Style = PaletteSetStyles.ShowPropertiesMenu |
                                     PaletteSetStyles.ShowAutoHideButton |
                                     PaletteSetStyles.ShowCloseButton;

                // Limit the minimum size to prevent design distortion
                _chatPalette.MinimumSize = new System.Drawing.Size(300, 500);
                _chatPalette.DockEnabled = DockSides.Left | DockSides.Right;

                // 3. Initialize our WPF UI
                _chatPanel = new AIChatPanel();

                // 4. Add the WPF window to the AutoCAD PaletteSet
                _chatPalette.AddVisual("Chat", _chatPanel);
            }

            // 5. Display on screen
            _chatPalette.Visible = true;
        }
    }
}