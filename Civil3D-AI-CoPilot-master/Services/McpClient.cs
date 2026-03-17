using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cad_AI_Agent.Services
{
    /// <summary>
    /// Client for communicating with the Civil 3D MCP server to query drawing data.
    /// </summary>
    public class McpClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private bool _isAvailable;

        public McpClient(string baseUrl = "http://localhost:3000")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _isAvailable = false;
        }

        /// <summary>
        /// Check if MCP server is available and responding.
        /// </summary>
        public async Task<bool> CheckAvailabilityAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                _isAvailable = response.IsSuccessStatusCode;
                return _isAvailable;
            }
            catch
            {
                _isAvailable = false;
                return false;
            }
        }

        public bool IsAvailable => _isAvailable;

        /// <summary>
        /// Execute an MCP tool and return the result.
        /// </summary>
        private async Task<JObject> ExecuteToolAsync(string toolName, object? parameters = null)
        {
            if (!_isAvailable)
            {
                throw new InvalidOperationException("MCP server is not available. Call CheckAvailabilityAsync first.");
            }

            try
            {
                var request = new
                {
                    tool = toolName,
                    parameters = parameters ?? new { }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/execute", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"MCP tool '{toolName}' failed: {responseString}");
                }

                return JObject.Parse(responseString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing MCP tool '{toolName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// List all alignments in the active drawing.
        /// </summary>
        public async Task<List<AlignmentInfo>> ListAlignmentsAsync()
        {
            var result = await ExecuteToolAsync("civil3d_list_alignments");
            var alignments = new List<AlignmentInfo>();

            if (result["alignments"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    alignments.Add(new AlignmentInfo
                    {
                        Name = item["name"]?.ToString() ?? string.Empty,
                        Length = item["length"]?.Value<double>() ?? 0,
                        StartStation = item["startStation"]?.Value<double>() ?? 0,
                        EndStation = item["endStation"]?.Value<double>() ?? 0
                    });
                }
            }

            return alignments;
        }

        /// <summary>
        /// List all surfaces in the active drawing.
        /// </summary>
        public async Task<List<SurfaceInfo>> ListSurfacesAsync()
        {
            var result = await ExecuteToolAsync("civil3d_list_surfaces");
            var surfaces = new List<SurfaceInfo>();

            if (result["surfaces"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    surfaces.Add(new SurfaceInfo
                    {
                        Name = item["name"]?.ToString() ?? string.Empty,
                        Type = item["type"]?.ToString() ?? string.Empty,
                        MinElevation = item["minElevation"]?.Value<double>(),
                        MaxElevation = item["maxElevation"]?.Value<double>()
                    });
                }
            }

            return surfaces;
        }

        /// <summary>
        /// List all profiles for a specific alignment.
        /// </summary>
        public async Task<List<ProfileInfo>> ListProfilesAsync(string alignmentName)
        {
            var result = await ExecuteToolAsync("civil3d_list_profiles", new { alignmentName });
            var profiles = new List<ProfileInfo>();

            if (result["profiles"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    profiles.Add(new ProfileInfo
                    {
                        Name = item["name"]?.ToString() ?? string.Empty,
                        Type = item["type"]?.ToString() ?? string.Empty,
                        AlignmentName = alignmentName
                    });
                }
            }

            return profiles;
        }

        /// <summary>
        /// List all assemblies in the active drawing.
        /// </summary>
        public async Task<List<AssemblyInfo>> ListAssembliesAsync()
        {
            var result = await ExecuteToolAsync("civil3d_list_assemblies");
            var assemblies = new List<AssemblyInfo>();

            if (result["assemblies"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    assemblies.Add(new AssemblyInfo
                    {
                        Name = item["name"]?.ToString() ?? string.Empty
                    });
                }
            }

            return assemblies;
        }

        /// <summary>
        /// List all corridors in the active drawing.
        /// </summary>
        public async Task<List<CorridorInfo>> ListCorridorsAsync()
        {
            var result = await ExecuteToolAsync("civil3d_list_corridors");
            var corridors = new List<CorridorInfo>();

            if (result["corridors"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    corridors.Add(new CorridorInfo
                    {
                        Name = item["name"]?.ToString() ?? string.Empty,
                        AlignmentName = item["alignmentName"]?.ToString() ?? string.Empty,
                        ProfileName = item["profileName"]?.ToString() ?? string.Empty
                    });
                }
            }

            return corridors;
        }

        /// <summary>
        /// Get detailed information about a specific alignment.
        /// </summary>
        public async Task<JObject> GetAlignmentDetailsAsync(string alignmentName)
        {
            return await ExecuteToolAsync("civil3d_alignment_report", new { alignmentName });
        }

        /// <summary>
        /// Get detailed information about a specific surface.
        /// </summary>
        public async Task<JObject> GetSurfaceDetailsAsync(string surfaceName)
        {
            return await ExecuteToolAsync("civil3d_surface_report", new { surfaceName });
        }

        /// <summary>
        /// Get detailed information about a specific corridor.
        /// </summary>
        public async Task<JObject> GetCorridorDetailsAsync(string corridorName)
        {
            return await ExecuteToolAsync("civil3d_corridor_summary", new { corridorName });
        }

        /// <summary>
        /// Query drawing context - returns summary of all available objects.
        /// </summary>
        public async Task<DrawingContext> GetDrawingContextAsync()
        {
            var context = new DrawingContext();

            try
            {
                // Run queries in parallel for speed
                var alignmentsTask = ListAlignmentsAsync();
                var surfacesTask = ListSurfacesAsync();
                var assembliesTask = ListAssembliesAsync();
                var corridorsTask = ListCorridorsAsync();

                await Task.WhenAll(alignmentsTask, surfacesTask, assembliesTask, corridorsTask);

                context.Alignments = await alignmentsTask;
                context.Surfaces = await surfacesTask;
                context.Assemblies = await assembliesTask;
                context.Corridors = await corridorsTask;
            }
            catch (Exception ex)
            {
                context.Error = ex.Message;
            }

            return context;
        }

        /// <summary>
        /// Execute any MCP tool by name and return the raw result as JObject.
        /// Used for routing AI-requested civil3d_* commands directly to MCP.
        /// </summary>
        public async Task<JObject> ExecuteMcpToolAsync(string toolName, JObject? args = null)
        {
            return await ExecuteToolAsync(toolName, args);
        }

    #region Hydrology Methods

    /// <summary>
    /// Trace a downhill flow path from a point on a surface using steepest descent.
    /// </summary>
    public async Task<HydrologyFlowPathResult> TraceFlowPathAsync(
        string surfaceName, double x, double y,
        double? stepDistance = null, int? maxSteps = null)
    {
        var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            ["action"] = "trace_flow_path",
            ["surfaceName"] = surfaceName,
            ["x"] = x,
            ["y"] = y
        };
        if (stepDistance.HasValue) parameters["stepDistance"] = stepDistance.Value;
        if (maxSteps.HasValue) parameters["maxSteps"] = maxSteps.Value;

        var result = await ExecuteToolAsync("civil3d_hydrology", parameters);
        return ParseHydrologyFlowPath(result);
    }

    /// <summary>
    /// Find the lowest elevation point on a surface by grid sampling.
    /// </summary>
    public async Task<HydrologyLowPointResult> FindLowPointAsync(
        string surfaceName, double? sampleSpacing = null)
    {
        var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            ["action"] = "find_low_point",
            ["surfaceName"] = surfaceName
        };
        if (sampleSpacing.HasValue) parameters["sampleSpacing"] = sampleSpacing.Value;

        var result = await ExecuteToolAsync("civil3d_hydrology", parameters);
        return ParseHydrologyLowPoint(result);
    }

    /// <summary>
    /// Delineate a watershed boundary for an outlet point.
    /// </summary>
    public async Task<HydrologyWatershedResult> DelineateWatershedAsync(
        string surfaceName, double outletX, double outletY,
        double? gridSpacing = null, double? searchRadius = null)
    {
        var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            ["action"] = "delineate_watershed",
            ["surfaceName"] = surfaceName,
            ["outletX"] = outletX,
            ["outletY"] = outletY
        };
        if (gridSpacing.HasValue) parameters["gridSpacing"] = gridSpacing.Value;
        if (searchRadius.HasValue) parameters["searchRadius"] = searchRadius.Value;

        var result = await ExecuteToolAsync("civil3d_hydrology", parameters);
        return ParseHydrologyWatershed(result);
    }

    /// <summary>
    /// Calculate catchment area draining to an outlet point.
    /// </summary>
    public async Task<HydrologyCatchmentResult> CalculateCatchmentAreaAsync(
        string surfaceName, double outletX, double outletY,
        double? sampleSpacing = null, double? maxDistance = null)
    {
        var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            ["action"] = "calculate_catchment_area",
            ["surfaceName"] = surfaceName,
            ["outletX"] = outletX,
            ["outletY"] = outletY
        };
        if (sampleSpacing.HasValue) parameters["sampleSpacing"] = sampleSpacing.Value;
        if (maxDistance.HasValue) parameters["maxDistance"] = maxDistance.Value;

        var result = await ExecuteToolAsync("civil3d_hydrology", parameters);
        return ParseHydrologyCatchment(result);
    }

    /// <summary>
    /// Estimate peak runoff using the Rational Method (Q = CiA).
    /// </summary>
    public async Task<HydrologyRunoffResult> EstimateRunoffAsync(
        double drainageArea, double runoffCoefficient, double rainfallIntensity,
        string areaUnits = "acres", string intensityUnits = "in_per_hr")
    {
        var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            ["action"] = "estimate_runoff",
            ["drainageArea"] = drainageArea,
            ["runoffCoefficient"] = runoffCoefficient,
            ["rainfallIntensity"] = rainfallIntensity,
            ["areaUnits"] = areaUnits,
            ["intensityUnits"] = intensityUnits
        };

        var result = await ExecuteToolAsync("civil3d_hydrology", parameters);
        return ParseHydrologyRunoff(result);
    }

    private HydrologyFlowPathResult ParseHydrologyFlowPath(JObject data)
    {
        var result = new HydrologyFlowPathResult
        {
            SurfaceName = data["surfaceName"]?.ToString() ?? string.Empty,
            Status = data["status"]?.ToString() ?? string.Empty,
            StepCount = data["stepCount"]?.Value<int>() ?? 0,
            TotalDistance = data["totalDistance"]?.Value<double>() ?? 0,
            DropElevation = data["dropElevation"]?.Value<double>() ?? 0
        };

        if (data["points"] is JArray pts)
        {
            foreach (var pt in pts)
            {
                result.Points.Add((
                    pt["x"]?.Value<double>() ?? 0,
                    pt["y"]?.Value<double>() ?? 0,
                    pt["elevation"]?.Value<double>() ?? 0
                ));
            }
        }
        return result;
    }

    private HydrologyLowPointResult ParseHydrologyLowPoint(JObject data)
    {
        var lp = data["lowPoint"];
        return new HydrologyLowPointResult
        {
            SurfaceName = data["surfaceName"]?.ToString() ?? string.Empty,
            X = lp?["x"]?.Value<double>() ?? 0,
            Y = lp?["y"]?.Value<double>() ?? 0,
            Elevation = lp?["elevation"]?.Value<double>() ?? 0,
            SampledPointCount = data["sampledPointCount"]?.Value<int>() ?? 0
        };
    }

    private HydrologyWatershedResult ParseHydrologyWatershed(JObject data)
    {
        var result = new HydrologyWatershedResult
        {
            SurfaceName = data["surfaceName"]?.ToString() ?? string.Empty,
            ContributingPointCount = data["contributingPointCount"]?.Value<int>() ?? 0,
            ApproximateArea = data["approximateArea"]?.Value<double>() ?? 0
        };

        var op = data["outletPoint"];
        if (op != null)
        {
            result.OutletX = op["x"]?.Value<double>() ?? 0;
            result.OutletY = op["y"]?.Value<double>() ?? 0;
            result.OutletElevation = op["elevation"]?.Value<double>() ?? 0;
        }

        if (data["boundaryPoints"] is JArray bps)
        {
            foreach (var bp in bps)
            {
                result.BoundaryPoints.Add((
                    bp["x"]?.Value<double>() ?? 0,
                    bp["y"]?.Value<double>() ?? 0
                ));
            }
        }
        return result;
    }

    private HydrologyCatchmentResult ParseHydrologyCatchment(JObject data)
    {
        var result = new HydrologyCatchmentResult
        {
            SurfaceName = data["surfaceName"]?.ToString() ?? string.Empty,
            CatchmentArea = data["catchmentArea"]?.Value<double>() ?? 0,
            ContributingCellCount = data["contributingCellCount"]?.Value<int>() ?? 0
        };

        var op = data["outletPoint"];
        if (op != null)
        {
            result.OutletX = op["x"]?.Value<double>() ?? 0;
            result.OutletY = op["y"]?.Value<double>() ?? 0;
            result.OutletElevation = op["elevation"]?.Value<double>() ?? 0;
        }

        var stats = data["elevationStatistics"];
        if (stats != null)
        {
            result.ElevMin = stats["minimum"]?.Value<double>() ?? 0;
            result.ElevMax = stats["maximum"]?.Value<double>() ?? 0;
            result.ElevAverage = stats["average"]?.Value<double>() ?? 0;
            result.Relief = stats["relief"]?.Value<double>() ?? 0;
        }
        return result;
    }

    private HydrologyRunoffResult ParseHydrologyRunoff(JObject data)
    {
        var rr = data["runoffRate"];
        return new HydrologyRunoffResult
        {
            DrainageArea = data["drainageArea"]?.Value<double>() ?? 0,
            RunoffCoefficient = data["runoffCoefficient"]?.Value<double>() ?? 0,
            RainfallIntensity = data["rainfallIntensity"]?.Value<double>() ?? 0,
            AreaUnits = data["areaUnits"]?.ToString() ?? "acres",
            IntensityUnits = data["intensityUnits"]?.ToString() ?? "in_per_hr",
            RunoffCfs = rr?["cfs"]?.Value<double>() ?? 0,
            RunoffCubicMetersPerSecond = rr?["cubicMetersPerSecond"]?.Value<double>() ?? 0
        };
    }

    #endregion

    } // end class McpClient

    #region Data Models

    public class AlignmentInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Length { get; set; }
        public double StartStation { get; set; }
        public double EndStation { get; set; }
    }

    public class SurfaceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }
    }

    public class ProfileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string AlignmentName { get; set; } = string.Empty;
    }

    public class AssemblyInfo
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CorridorInfo
    {
        public string Name { get; set; } = string.Empty;
        public string AlignmentName { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
    }

    public class DrawingContext
    {
        public List<AlignmentInfo> Alignments { get; set; } = new List<AlignmentInfo>();
        public List<SurfaceInfo> Surfaces { get; set; } = new List<SurfaceInfo>();
        public List<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();
        public List<CorridorInfo> Corridors { get; set; } = new List<CorridorInfo>();
        public string? Error { get; set; }

        public bool HasAlignments => Alignments?.Count > 0;
        public bool HasSurfaces => Surfaces?.Count > 0;
        public bool HasAssemblies => Assemblies?.Count > 0;
        public bool HasCorridors => Corridors?.Count > 0;
        public bool HasPipeNetworks => PipeNetworks?.Count > 0;

        public List<string> PipeNetworks { get; set; } = new List<string>();

        public string ToSummary()
        {
            if (!string.IsNullOrEmpty(Error))
            {
                return $"Error querying drawing: {Error}";
            }

            var summary = new StringBuilder();
            summary.AppendLine("Drawing Context:");
            summary.AppendLine($"- Alignments: {Alignments.Count}");
            if (Alignments.Count > 0)
            {
                foreach (var al in Alignments)
                {
                    summary.AppendLine($"  • {al.Name} (Length: {al.Length:F2})");
                }
            }

            summary.AppendLine($"- Surfaces: {Surfaces.Count}");
            if (Surfaces.Count > 0)
            {
                foreach (var surf in Surfaces)
                {
                    summary.AppendLine($"  • {surf.Name} ({surf.Type})");
                }
            }

            summary.AppendLine($"- Assemblies: {Assemblies.Count}");
            if (Assemblies.Count > 0)
            {
                foreach (var asm in Assemblies)
                {
                    summary.AppendLine($"  • {asm.Name}");
                }
            }

            summary.AppendLine($"- Corridors: {Corridors.Count}");
            if (Corridors.Count > 0)
            {
                foreach (var corr in Corridors)
                {
                    summary.AppendLine($"  • {corr.Name}");
                }
            }

            return summary.ToString();
        }
    }

    #endregion

    #region Hydrology Data Models

    public class HydrologyFlowPathResult
    {
        public string SurfaceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int StepCount { get; set; }
        public double TotalDistance { get; set; }
        public double DropElevation { get; set; }
        public List<(double X, double Y, double Elevation)> Points { get; set; } = new();
    }

    public class HydrologyLowPointResult
    {
        public string SurfaceName { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Elevation { get; set; }
        public int SampledPointCount { get; set; }
    }

    public class HydrologyWatershedResult
    {
        public string SurfaceName { get; set; } = string.Empty;
        public double OutletX { get; set; }
        public double OutletY { get; set; }
        public double OutletElevation { get; set; }
        public int ContributingPointCount { get; set; }
        public double ApproximateArea { get; set; }
        public List<(double X, double Y)> BoundaryPoints { get; set; } = new();
    }

    public class HydrologyCatchmentResult
    {
        public string SurfaceName { get; set; } = string.Empty;
        public double OutletX { get; set; }
        public double OutletY { get; set; }
        public double OutletElevation { get; set; }
        public double CatchmentArea { get; set; }
        public int ContributingCellCount { get; set; }
        public double ElevMin { get; set; }
        public double ElevMax { get; set; }
        public double ElevAverage { get; set; }
        public double Relief { get; set; }
    }

    public class HydrologyRunoffResult
    {
        public double DrainageArea { get; set; }
        public double RunoffCoefficient { get; set; }
        public double RainfallIntensity { get; set; }
        public string AreaUnits { get; set; } = "acres";
        public string IntensityUnits { get; set; } = "in_per_hr";
        public double RunoffCfs { get; set; }
        public double RunoffCubicMetersPerSecond { get; set; }
    }

    #endregion
}
