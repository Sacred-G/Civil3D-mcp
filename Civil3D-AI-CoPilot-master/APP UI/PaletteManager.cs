using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using Cad_AI_Agent.UI; // შემოგვაქვს შენი WPF ფანჯარა

namespace Cad_AI_Agent.UI
{
    public class PaletteManager
    {
        // ვიყენებთ სტატიკურ ცვლადებს, რომ ერთდროულად 10 ფანჯარა არ გაიხსნას
        private static PaletteSet _chatPalette;
        private static AIChatPanel _chatPanel;

        // ახალი ბრძანება, რითიც Civil 3D-ში ამ პანელს გამოვაჩენთ
        [CommandMethod("AIChat")]
        public void OpenAIChat()
        {
            if (_chatPalette == null)
            {
                // 1. ვქმნით პანელს. GUID (გრძელი კოდი) საჭიროა იმისთვის, რომ 
                // ავტოკადმა დაიმახსოვროს ფანჯრის ზომა და პოზიცია შემდეგი ჩართვისთვის.
                _chatPalette = new PaletteSet("AI CAD Agent", new Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D"));

                // 2. ვურთავთ მიმაგრების (Docking) და ავტო-დამალვის ფუნქციებს
                _chatPalette.Style = PaletteSetStyles.ShowPropertiesMenu |
                                     PaletteSetStyles.ShowAutoHideButton |
                                     PaletteSetStyles.ShowCloseButton;

                // ვუზღუდავთ მინიმალურ ზომას, რომ დიზაინი არ დამახინჯდეს
                _chatPalette.MinimumSize = new System.Drawing.Size(300, 500);
                _chatPalette.DockEnabled = DockSides.Left | DockSides.Right;

                // 3. ვაინიციალიზებთ ჩვენს WPF UI-ს
                _chatPanel = new AIChatPanel();

                // 4. ვსვამთ WPF ფანჯარას AutoCAD-ის PaletteSet-ში
                _chatPalette.AddVisual("Chat", _chatPanel);
            }

            // 5. ვაჩენთ ეკრანზე
            _chatPalette.Visible = true;
        }
    }
}