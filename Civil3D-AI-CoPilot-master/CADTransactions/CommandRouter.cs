using Autodesk.AutoCAD.ApplicationServices;
using Cad_AI_Agent.Models;
using Newtonsoft.Json.Linq;
using System;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CommandRouter
    {
        public static void Execute(Document doc, CadCommand command, ExecutionProgressReporter progress = null)
        {
            try
            {
                switch (command.Action)
                {
                    case "DrawLine":
                        progress?.ReportRunning(command.Action, "Creating line geometry...", $"From ({command.Params[0]:F2}, {command.Params[1]:F2}) to ({command.Params[2]:F2}, {command.Params[3]:F2})");
                        if (command.Params.Length >= 4)
                        {
                            LineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], command.Params[3]);
                            progress?.ReportSuccess(command.Action, "Line created successfully", $"Length: {Math.Sqrt(Math.Pow(command.Params[2]-command.Params[0], 2) + Math.Pow(command.Params[3]-command.Params[1], 2)):F2} units");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for line", "Expected: X1, Y1, X2, Y2");
                        }
                        break;

                    case "DrawCircle":
                        progress?.ReportRunning(command.Action, "Creating circle...", $"Center: ({command.Params[0]:F2}, {command.Params[1]:F2}), Radius: {command.Params[2]:F2}");
                        if (command.Params.Length >= 3)
                        {
                            CircleDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2]);
                            progress?.ReportSuccess(command.Action, "Circle created successfully", $"Area: {Math.PI * command.Params[2] * command.Params[2]:F2} sq units");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for circle", "Expected: X, Y, Radius");
                        }
                        break;

                    case "DrawCogoPoint":
                        progress?.ReportRunning(command.Action, "Creating COGO point...", $"Location: ({command.Params[0]:F2}, {command.Params[1]:F2}, {command.Params[2]:F2})");
                        if (command.Params.Length >= 3)
                        {
                            CogoPointDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], "AI_Point");
                            progress?.ReportSuccess(command.Action, "COGO point created", $"Elevation: {command.Params[2]:F2}");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters", "Expected: X, Y, Z");
                        }
                        break;

                    case "DrawCogoPoints":
                        int pointCount = command.Params.Length / 3;
                        progress?.ReportRunning(command.Action, $"Creating {pointCount} COGO points...", "Processing batch point creation");
                        if (command.Params.Length >= 3)
                        {
                            for (int i = 0; i + 2 < command.Params.Length; i += 3)
                            {
                                CogoPointDrawer.Draw(doc, command.Params[i], command.Params[i + 1], command.Params[i + 2], "AI_Point");
                            }
                            progress?.ReportSuccess(command.Action, $"{pointCount} COGO points created", "All points added to the drawing");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for points", "Expected: groups of X, Y, Z");
                        }
                        break;

                    case "DrawAlignment":
                        string alignmentName = GetStringArg(command, "alignmentName") ?? "AI_Alignment";
                        int piCount = command.Params.Length / 2;
                        progress?.ReportRunning(command.Action, $"Creating alignment '{alignmentName}'...", $"Processing {piCount} PI points");
                        if (command.Params.Length >= 4)
                        {
                            AlignmentDrawer.Draw(doc, command.Params, alignmentName);
                            progress?.ReportSuccess(command.Action, $"Alignment '{alignmentName}' created", $"Total length with curves will be calculated by Civil 3D");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient PI points", "Minimum 2 points (4 values) required");
                        }
                        break;

                    case "DrawSurface":
                        string surfaceName = GetStringArg(command, "surfaceName") ?? "AI_Surface";
                        int pointCount3d = command.Params.Length / 3;
                        progress?.ReportRunning(command.Action, $"Creating TIN surface '{surfaceName}'...", $"Building from {pointCount3d} points");
                        if (command.Params.Length >= 9)
                        {
                            SurfaceDrawer.Draw(doc, command.Params, surfaceName);
                            progress?.ReportSuccess(command.Action, $"Surface '{surfaceName}' created", "TIN surface is now available for profile/corridor operations");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient surface points", "Minimum 3 points (9 values) required");
                        }
                        break;

                    case "AddSurfaceBreakline":
                        string targetSurface = GetStringArg(command, "surfaceName");
                        progress?.ReportRunning(command.Action, "Adding breakline to surface...", targetSurface != null ? $"Target: {targetSurface}" : "Using first active surface");
                        if (command.Params.Length >= 6)
                        {
                            SurfaceDrawer.AddBreakline(doc, command.Params, targetSurface);
                            progress?.ReportSuccess(command.Action, "Breakline added", "Surface will be rebuilt with breakline constraints");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient breakline points", "Minimum 2 points (6 values) required");
                        }
                        break;

                    case "DrawProfile":
                        string profileAlignment = GetStringArg(command, "alignmentName");
                        string profileSurface = GetStringArg(command, "surfaceName");
                        string profileName = GetStringArg(command, "profileName") ?? "AI_Profile";
                        progress?.ReportRunning(command.Action, $"Creating surface profile '{profileName}'...", $"Alignment: {profileAlignment ?? "first"}, Surface: {profileSurface ?? "first"}");
                        if (command.Params.Length >= 2 || command.Args != null)
                        {
                            ProfileDrawer.Draw(
                                doc,
                                command.Params.Length > 0 ? command.Params[0] : GetDoubleArg(command, "insertX", 0.0),
                                command.Params.Length > 1 ? command.Params[1] : GetDoubleArg(command, "insertY", 0.0),
                                profileAlignment,
                                profileSurface,
                                profileName);
                            progress?.ReportSuccess(command.Action, $"Profile '{profileName}' created", "Surface profile with profile view added");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Missing profile parameters", "Expected: insertX, insertY or Args");
                        }
                        break;

                    case "DrawLayoutProfile":
                        string layoutAlignment = GetStringArg(command, "alignmentName");
                        string layoutProfileName = GetStringArg(command, "profileName") ?? "AI_LayoutProfile";
                        int pviCount = command.Params.Length / 2;
                        progress?.ReportRunning(command.Action, $"Creating layout profile '{layoutProfileName}'...", $"Processing {pviCount} PVI points");
                        if (command.Params.Length >= 4)
                        {
                            LayoutProfileDrawer.Draw(doc, command.Params, layoutAlignment, layoutProfileName, GetDoubleArgNullable(command, "viewOffsetY"));
                            progress?.ReportSuccess(command.Action, $"Layout profile '{layoutProfileName}' created", "Design profile with tangents and profile view");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient PVI data", "Minimum 2 PVI points required");
                        }
                        break;

                    case "DrawAutoProfile":
                        double interval = command.Params.Length > 0 ? command.Params[0] : 150.0;
                        string autoAlignment = GetStringArg(command, "alignmentName");
                        string sourceProfile = GetStringArg(command, "sourceProfileName");
                        string autoProfileName = GetStringArg(command, "profileName") ?? "AI_AutoProfile";
                        progress?.ReportRunning(command.Action, $"Generating auto-profile '{autoProfileName}'...", $"Sampling every {interval:F0} units from {sourceProfile ?? "EG profile"}");
                        AutoProfileDrawer.Draw(doc, interval, autoAlignment, sourceProfile, autoProfileName);
                        progress?.ReportSuccess(command.Action, $"Auto-profile '{autoProfileName}' generated", $"Best-fit design profile created with {interval:F0} interval sampling");
                        break;

                    case "DrawCorridor":
                        string corridorName = GetStringArg(command, "corridorName") ?? "AI_Corridor";
                        string corrAlignment = GetStringArg(command, "alignmentName");
                        string corrProfile = GetStringArg(command, "profileName");
                        string corrAssembly = GetStringArg(command, "assemblyName");
                        string corrSurface = GetStringArg(command, "surfaceName");
                        double frequency = GetDoubleArg(command, "frequency", 10.0);
                        progress?.ReportRunning(command.Action, $"Building corridor '{corridorName}'...", $"Alignment: {corrAlignment ?? "first"}, Profile: {corrProfile ?? "first FG"}, Assembly: {corrAssembly ?? "first"}");
                        CorridorDrawer.Draw(
                            doc,
                            corridorName,
                            corrAlignment,
                            corrProfile,
                            corrAssembly,
                            corrSurface,
                            frequency);
                        progress?.ReportSuccess(command.Action, $"Corridor '{corridorName}' built", $"Stations every {frequency:F1} units, surface targets applied");
                        break;

                    case "DrawCrossSections":
                        string sectionAlignment = GetStringArg(command, "alignmentName");
                        double sectionInterval = command.Params.Length > 0 ? command.Params[0] : GetDoubleArg(command, "interval", 10.0);
                        double leftWidth = GetDoubleArg(command, "leftWidth", 20.0);
                        double rightWidth = GetDoubleArg(command, "rightWidth", 20.0);
                        progress?.ReportRunning(command.Action, "Creating cross-sections...", $"Every {sectionInterval:F1} units, {leftWidth:F1}L/{rightWidth:F1}R width");
                        CrossSectionDrawer.Draw(
                            doc,
                            sectionInterval,
                            sectionAlignment,
                            leftWidth,
                            rightWidth,
                            GetIntArg(command, "columns", 10),
                            GetDoubleArg(command, "spacingX", 80.0),
                            GetDoubleArg(command, "spacingY", 50.0));
                        progress?.ReportSuccess(command.Action, "Cross-sections created", "Sample lines and section views generated");
                        break;

                    case "DrawSampleLines":
                        string sampleAlignment = GetStringArg(command, "alignmentName");
                        string groupName = GetStringArg(command, "groupName") ?? "AI_SampleLines";
                        progress?.ReportRunning(command.Action, $"Creating sample line group '{groupName}'...", $"Interval: {command.Params[0]:F1}, Widths: {command.Params[1]:F1}L/{command.Params[2]:F1}R");
                        if (command.Params.Length >= 3)
                        {
                            SampleLineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], sampleAlignment, groupName);
                            progress?.ReportSuccess(command.Action, $"Sample lines '{groupName}' created", "Ready for cross-section generation");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters", "Expected: interval, leftWidth, rightWidth");
                        }
                        break;

                    case "ExtractSurfaceContours":
                        string contourSurface = GetStringArg(command, "surfaceName");
                        progress?.ReportRunning(command.Action, "Extracting surface contours...", $"Major: {command.Params[0]:F1}, Minor: {command.Params[1]:F1}" + (contourSurface != null ? $", Surface: {contourSurface}" : ""));
                        if (command.Params.Length >= 2)
                        {
                            SurfaceDrawer.ExtractContours(doc, command.Params[0], command.Params[1], contourSurface);
                            progress?.ReportSuccess(command.Action, "Contours extracted", $"Major {command.Params[0]:F1}' / Minor {command.Params[1]:F1}' contour polylines created");
                        }
                        else
                        {
                            progress?.ReportSkipped(command.Action, "Missing contour intervals", "Expected: majorInterval, minorInterval");
                        }
                        break;

                    case "ClearModel":
                        progress?.ReportRunning(command.Action, "Clearing model...", "Removing corridors, alignments, and dependent objects");
                        ModelCleanser.Clear(doc);
                        progress?.ReportSuccess(command.Action, "Model cleared", "All AI-generated objects removed");
                        break;

                    default:
                        progress?.ReportError(command.Action, $"Unknown command: {command.Action}", "Command not recognized by the AI agent");
                        doc.Editor.WriteMessage($"\n[AI Agent] Unknown command: {command.Action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                progress?.ReportError(command.Action, $"Error executing {command.Action}", ex.Message);
                throw;
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