using Autodesk.AutoCAD.ApplicationServices;
using Cad_AI_Agent.Models;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CommandRouter
    {
        public static void Execute(Document doc, CadCommand command)
        {
            switch (command.Action)
            {
                case "DrawLine":
                    if (command.Params.Length >= 4)
                        LineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], command.Params[3]);
                    break;
                case "DrawCircle":
                    if (command.Params.Length >= 3)
                        CircleDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2]);
                    break;
                case "DrawCogoPoint":
                    if (command.Params.Length >= 3)
                        CogoPointDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], "AI_Point");
                    break;
                case "DrawAlignment":
                    if (command.Params.Length >= 4) // მინიმუმ 2 წერტილი სჭირდება (X1, Y1, X2, Y2)
                        AlignmentDrawer.Draw(doc, command.Params);
                    break;
                case "DrawProfile":
                    if (command.Params.Length >= 2) // გჭირდება მხოლოდ 2 კოორდინატი პროფილის დასასმელად (InsertX, InsertY)
                        ProfileDrawer.Draw(doc, command.Params[0], command.Params[1]);
                    break;
                case "DrawLayoutProfile":
                    if (command.Params.Length >= 4) // მინიმუმ 2 PVI გვჭირდება (Station1, Elev1, Station2, Elev2)
                        LayoutProfileDrawer.Draw(doc, command.Params);
                    break;
                case "DrawAutoProfile":
                    // თუ პარამეტრი მოგვაწოდა, ვიყენებთ მას (ბიჯს), თუ არა - 150 მეტრს
                    double interval = command.Params.Length > 0 ? command.Params[0] : 150.0;
                    AutoProfileDrawer.Draw(doc);
                    break;
                case "DrawCorridor":
                    CorridorDrawer.Draw(doc);
                    break;
                case "DrawCrossSections":
                    CrossSectionDrawer.Draw(doc, 10.0); // 10მ ბიჯი
                    break;
                case "ClearModel":
                    ModelCleanser.Clear(doc);
                    break;
                default:
                    doc.Editor.WriteMessage($"\n[AI Agent] Unknown command: {command.Action}");
                    break;
            }
        }
    }
}