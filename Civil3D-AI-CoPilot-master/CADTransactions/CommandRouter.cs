using Autodesk.AutoCAD.ApplicationServices;
using Cad_AI_Agent.Models;
using Newtonsoft.Json.Linq;
using System;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CommandRouter
    {
        // Minimum parameter counts for each command type
        private const int MinLineParams = 4;        // X1, Y1, X2, Y2
        private const int MinCircleParams = 3;       // X, Y, Radius
        private const int MinPointParams = 3;        // X, Y, Z
        private const int MinAlignmentParams = 4;    // At least 2 PI points
        private const int MinSurfaceParams = 9;      // At least 3 XYZ points
        private const int MinBreaklineParams = 6;    // At least 2 XYZ points
        private const int MinProfileParams = 2;      // InsertX, InsertY
        private const int MinPviParams = 4;          // At least 2 PVI points
        private const int MinContourParams = 2;      // Major, Minor intervals
        private const int MinSampleLineParams = 3;   // Interval, LeftWidth, RightWidth

        // Default object name prefixes
        private const string DefaultAlignmentName = "AI_Alignment";
        private const string DefaultSurfaceName = "AI_Surface";
        private const string DefaultProfileName = "AI_Profile";
        private const string DefaultLayoutProfileName = "AI_LayoutProfile";
        private const string DefaultAutoProfileName = "AI_AutoProfile";
        private const string DefaultCorridorName = "AI_Corridor";
        private const string DefaultSampleGroupName = "AI_SampleLines";

        // Default numeric parameters
        private const double DefaultAutoProfileInterval = 150.0;
        private const double DefaultCorridorFrequency = 10.0;
        private const double DefaultSectionInterval = 10.0;
        private const double DefaultSectionWidth = 20.0;
        private const int DefaultSectionColumns = 10;
        private const double DefaultSectionSpacingX = 80.0;
        private const double DefaultSectionSpacingY = 50.0;

        public static void Execute(Document doc, CadCommand command, ExecutionProgressReporter progress = null)
        {
            if (command == null)
            {
                progress?.ReportError("null", "No command provided", "Command object was null");
                return;
            }

            if (string.IsNullOrWhiteSpace(command.Action))
            {
                progress?.ReportError("unknown", "Empty command action", "Command action was null or empty");
                return;
            }

            try
            {
                switch (command.Action)
                {
                    case "DrawLine":
                        if (command.Params.Length < MinLineParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for line", "Expected: X1, Y1, X2, Y2");
                            break;
                        }
                        progress?.ReportRunning(command.Action, "Creating line geometry...", $"From ({command.Params[0]:F2}, {command.Params[1]:F2}) to ({command.Params[2]:F2}, {command.Params[3]:F2})");
                        LineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], command.Params[3]);
                        progress?.ReportSuccess(command.Action, "Line created successfully", $"Length: {Math.Sqrt(Math.Pow(command.Params[2]-command.Params[0], 2) + Math.Pow(command.Params[3]-command.Params[1], 2)):F2} units");
                        break;

                    case "DrawCircle":
                        if (command.Params.Length < MinCircleParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for circle", "Expected: X, Y, Radius");
                            break;
                        }
                        if (command.Params[2] <= 0)
                        {
                            progress?.ReportSkipped(command.Action, "Invalid circle radius", "Radius must be positive");
                            break;
                        }
                        progress?.ReportRunning(command.Action, "Creating circle...", $"Center: ({command.Params[0]:F2}, {command.Params[1]:F2}), Radius: {command.Params[2]:F2}");
                        CircleDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2]);
                        progress?.ReportSuccess(command.Action, "Circle created successfully", $"Area: {Math.PI * command.Params[2] * command.Params[2]:F2} sq units");
                        break;

                    case "DrawCogoPoint":
                        if (command.Params.Length < MinPointParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters", "Expected: X, Y, Z");
                            break;
                        }
                        progress?.ReportRunning(command.Action, "Creating COGO point...", $"Location: ({command.Params[0]:F2}, {command.Params[1]:F2}, {command.Params[2]:F2})");
                        CogoPointDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], "AI_Point");
                        progress?.ReportSuccess(command.Action, "COGO point created", $"Elevation: {command.Params[2]:F2}");
                        break;

                    case "DrawCogoPoints":
                        if (command.Params.Length < MinPointParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters for points", "Expected: groups of X, Y, Z");
                            break;
                        }
                        int pointCount = command.Params.Length / 3;
                        progress?.ReportRunning(command.Action, $"Creating {pointCount} COGO points...", "Processing batch point creation");
                        for (int i = 0; i + 2 < command.Params.Length; i += 3)
                        {
                            CogoPointDrawer.Draw(doc, command.Params[i], command.Params[i + 1], command.Params[i + 2], "AI_Point");
                        }
                        progress?.ReportSuccess(command.Action, $"{pointCount} COGO points created", "All points added to the drawing");
                        break;

                    case "DrawAlignment":
                        string alignmentName = GetStringArg(command, "alignmentName") ?? DefaultAlignmentName;
                        int piCount = command.Params.Length / 2;
                        progress?.ReportRunning(command.Action, $"Creating alignment '{alignmentName}'...", $"Processing {piCount} PI points");
                        if (command.Params.Length < MinAlignmentParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient PI points", "Minimum 2 points (4 values) required");
                            break;
                        }
                        AlignmentDrawer.Draw(doc, command.Params, alignmentName);
                        progress?.ReportSuccess(command.Action, $"Alignment '{alignmentName}' created", "Total length with curves will be calculated by Civil 3D");
                        break;

                    case "DrawSurface":
                        string surfaceName = GetStringArg(command, "surfaceName") ?? DefaultSurfaceName;
                        int pointCount3d = command.Params.Length / 3;
                        progress?.ReportRunning(command.Action, $"Creating TIN surface '{surfaceName}'...", $"Building from {pointCount3d} points");
                        if (command.Params.Length < MinSurfaceParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient surface points", "Minimum 3 points (9 values) required");
                            break;
                        }
                        SurfaceDrawer.Draw(doc, command.Params, surfaceName);
                        progress?.ReportSuccess(command.Action, $"Surface '{surfaceName}' created", "TIN surface is now available for profile/corridor operations");
                        break;

                    case "AddSurfaceBreakline":
                        string targetSurface = GetStringArg(command, "surfaceName");
                        progress?.ReportRunning(command.Action, "Adding breakline to surface...", targetSurface != null ? $"Target: {targetSurface}" : "Using first active surface");
                        if (command.Params.Length < MinBreaklineParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient breakline points", "Minimum 2 points (6 values) required");
                            break;
                        }
                        SurfaceDrawer.AddBreakline(doc, command.Params, targetSurface);
                        progress?.ReportSuccess(command.Action, "Breakline added", "Surface will be rebuilt with breakline constraints");
                        break;

                    case "DrawProfile":
                        string profileAlignment = GetStringArg(command, "alignmentName");
                        string profileSurface = GetStringArg(command, "surfaceName");
                        string profileName = GetStringArg(command, "profileName") ?? DefaultProfileName;
                        progress?.ReportRunning(command.Action, $"Creating surface profile '{profileName}'...", $"Alignment: {profileAlignment ?? "first"}, Surface: {profileSurface ?? "first"}");
                        if (command.Params.Length >= MinProfileParams || command.Args != null)
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
                        string layoutProfileName = GetStringArg(command, "profileName") ?? DefaultLayoutProfileName;
                        int pviCount = command.Params.Length / 2;
                        progress?.ReportRunning(command.Action, $"Creating layout profile '{layoutProfileName}'...", $"Processing {pviCount} PVI points");
                        if (command.Params.Length < MinPviParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient PVI data", "Minimum 2 PVI points required");
                            break;
                        }
                        LayoutProfileDrawer.Draw(doc, command.Params, layoutAlignment, layoutProfileName, GetDoubleArgNullable(command, "viewOffsetY"));
                        progress?.ReportSuccess(command.Action, $"Layout profile '{layoutProfileName}' created", "Design profile with tangents and profile view");
                        break;

                    case "DrawAutoProfile":
                        double interval = command.Params.Length > 0 ? command.Params[0] : DefaultAutoProfileInterval;
                        string autoAlignment = GetStringArg(command, "alignmentName");
                        string sourceProfile = GetStringArg(command, "sourceProfileName");
                        string autoProfileName = GetStringArg(command, "profileName") ?? DefaultAutoProfileName;
                        progress?.ReportRunning(command.Action, $"Generating auto-profile '{autoProfileName}'...", $"Sampling every {interval:F0} units from {sourceProfile ?? "EG profile"}");
                        AutoProfileDrawer.Draw(doc, interval, autoAlignment, sourceProfile, autoProfileName);
                        progress?.ReportSuccess(command.Action, $"Auto-profile '{autoProfileName}' generated", $"Best-fit design profile created with {interval:F0} interval sampling");
                        break;

                    case "DrawCorridor":
                        string corridorName = GetStringArg(command, "corridorName") ?? DefaultCorridorName;
                        string corrAlignment = GetStringArg(command, "alignmentName");
                        string corrProfile = GetStringArg(command, "profileName");
                        string corrAssembly = GetStringArg(command, "assemblyName");
                        string corrSurface = GetStringArg(command, "surfaceName");
                        double frequency = GetDoubleArg(command, "frequency", DefaultCorridorFrequency);
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
                        double sectionInterval = command.Params.Length > 0 ? command.Params[0] : GetDoubleArg(command, "interval", DefaultSectionInterval);
                        double leftWidth = GetDoubleArg(command, "leftWidth", DefaultSectionWidth);
                        double rightWidth = GetDoubleArg(command, "rightWidth", DefaultSectionWidth);
                        progress?.ReportRunning(command.Action, "Creating cross-sections...", $"Every {sectionInterval:F1} units, {leftWidth:F1}L/{rightWidth:F1}R width");
                        CrossSectionDrawer.Draw(
                            doc,
                            sectionInterval,
                            sectionAlignment,
                            leftWidth,
                            rightWidth,
                            GetIntArg(command, "columns", DefaultSectionColumns),
                            GetDoubleArg(command, "spacingX", DefaultSectionSpacingX),
                            GetDoubleArg(command, "spacingY", DefaultSectionSpacingY));
                        progress?.ReportSuccess(command.Action, "Cross-sections created", "Sample lines and section views generated");
                        break;

                    case "DrawSampleLines":
                        string sampleAlignment = GetStringArg(command, "alignmentName");
                        string groupName = GetStringArg(command, "groupName") ?? DefaultSampleGroupName;
                        progress?.ReportRunning(command.Action, $"Creating sample line group '{groupName}'...", $"Interval: {command.Params[0]:F1}, Widths: {command.Params[1]:F1}L/{command.Params[2]:F1}R");
                        if (command.Params.Length < MinSampleLineParams)
                        {
                            progress?.ReportSkipped(command.Action, "Insufficient parameters", "Expected: interval, leftWidth, rightWidth");
                            break;
                        }
                        SampleLineDrawer.Draw(doc, command.Params[0], command.Params[1], command.Params[2], sampleAlignment, groupName);
                        progress?.ReportSuccess(command.Action, $"Sample lines '{groupName}' created", "Ready for cross-section generation");
                        break;

                    case "ExtractSurfaceContours":
                        string contourSurface = GetStringArg(command, "surfaceName");
                        progress?.ReportRunning(command.Action, "Extracting surface contours...", $"Major: {command.Params[0]:F1}, Minor: {command.Params[1]:F1}" + (contourSurface != null ? $", Surface: {contourSurface}" : ""));
                        if (command.Params.Length < MinContourParams)
                        {
                            progress?.ReportSkipped(command.Action, "Missing contour intervals", "Expected: majorInterval, minorInterval");
                            break;
                        }
                        if (command.Params[0] <= 0 || command.Params[1] <= 0)
                        {
                            progress?.ReportSkipped(command.Action, "Invalid contour intervals", "Intervals must be positive values");
                            break;
                        }
                        SurfaceDrawer.ExtractContours(doc, command.Params[0], command.Params[1], contourSurface);
                        progress?.ReportSuccess(command.Action, "Contours extracted", $"Major {command.Params[0]:F1}' / Minor {command.Params[1]:F1}' contour polylines created");
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