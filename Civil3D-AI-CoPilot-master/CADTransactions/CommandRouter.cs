using Autodesk.AutoCAD.ApplicationServices;
using Cad_AI_Agent.Models;
using Newtonsoft.Json.Linq;

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
                case "DrawCogoPoints":
                    if (command.Params.Length >= 3)
                    {
                        for (int i = 0; i + 2 < command.Params.Length; i += 3)
                        {
                            CogoPointDrawer.Draw(doc, command.Params[i], command.Params[i + 1], command.Params[i + 2], "AI_Point");
                        }
                    }
                    break;
                case "DrawAlignment":
                    if (command.Params.Length >= 4)
                        AlignmentDrawer.Draw(doc, command.Params, GetStringArg(command, "alignmentName") ?? "AI_Alignment");
                    break;
                case "DrawSurface":
                    if (command.Params.Length >= 9)
                        SurfaceDrawer.Draw(doc, command.Params, GetStringArg(command, "surfaceName"));
                    break;
                case "AddSurfaceBreakline":
                    if (command.Params.Length >= 6)
                        SurfaceDrawer.AddBreakline(doc, command.Params, GetStringArg(command, "surfaceName"));
                    break;
                case "DrawProfile":
                    if (command.Params.Length >= 2 || command.Args != null)
                        ProfileDrawer.Draw(
                            doc,
                            command.Params.Length > 0 ? command.Params[0] : GetDoubleArg(command, "insertX", 0.0),
                            command.Params.Length > 1 ? command.Params[1] : GetDoubleArg(command, "insertY", 0.0),
                            GetStringArg(command, "alignmentName"),
                            GetStringArg(command, "surfaceName"),
                            GetStringArg(command, "profileName"));
                    break;
                case "DrawLayoutProfile":
                    if (command.Params.Length >= 4)
                        LayoutProfileDrawer.Draw(doc, command.Params, GetStringArg(command, "alignmentName"), GetStringArg(command, "profileName"), GetDoubleArgNullable(command, "viewOffsetY"));
                    break;
                case "DrawAutoProfile":
                    double interval = command.Params.Length > 0 ? command.Params[0] : 150.0;
                    AutoProfileDrawer.Draw(doc, interval, GetStringArg(command, "alignmentName"), GetStringArg(command, "sourceProfileName"), GetStringArg(command, "profileName"));
                    break;
                case "DrawCorridor":
                    CorridorDrawer.Draw(
                        doc,
                        GetStringArg(command, "corridorName"),
                        GetStringArg(command, "alignmentName"),
                        GetStringArg(command, "profileName"),
                        GetStringArg(command, "assemblyName"),
                        GetStringArg(command, "surfaceName"),
                        GetDoubleArg(command, "frequency", 10.0));
                    break;
                case "DrawCrossSections":
                    CrossSectionDrawer.Draw(
                        doc,
                        command.Params.Length > 0 ? command.Params[0] : GetDoubleArg(command, "interval", 10.0),
                        GetStringArg(command, "alignmentName"),
                        GetDoubleArg(command, "leftWidth", 20.0),
                        GetDoubleArg(command, "rightWidth", 20.0),
                        GetIntArg(command, "columns", 10),
                        GetDoubleArg(command, "spacingX", 80.0),
                        GetDoubleArg(command, "spacingY", 50.0));
                    break;
                case "DrawSampleLines":
                    if (command.Params.Length >= 3)
                        SampleLineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], GetStringArg(command, "alignmentName"), GetStringArg(command, "groupName"));
                    break;
                case "ExtractSurfaceContours":
                    if (command.Params.Length >= 2)
                        SurfaceDrawer.ExtractContours(doc, command.Params[0], command.Params[1], GetStringArg(command, "surfaceName"));
                    break;
                case "ClearModel":
                    ModelCleanser.Clear(doc);
                    break;
                default:
                    doc.Editor.WriteMessage($"\n[AI Agent] Unknown command: {command.Action}");
                    break;
            }
        }

        private static string GetStringArg(CadCommand command, string name)
        {
            return command.Args?.Value<string>(name);
        }

        private static double GetDoubleArg(CadCommand command, string name, double fallback)
        {
            JToken token = command.Args?[name];
            if (token == null) return fallback;
            return token.Type == JTokenType.Integer || token.Type == JTokenType.Float
                ? token.Value<double>()
                : fallback;
        }

        private static double? GetDoubleArgNullable(CadCommand command, string name)
        {
            JToken token = command.Args?[name];
            if (token == null) return null;
            return token.Type == JTokenType.Integer || token.Type == JTokenType.Float
                ? token.Value<double>()
                : null;
        }

        private static int GetIntArg(CadCommand command, string name, int fallback)
        {
            JToken token = command.Args?[name];
            if (token == null) return fallback;
            return token.Type == JTokenType.Integer
                ? token.Value<int>()
                : fallback;
        }
    }
}