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
        private async Task<JObject> ExecuteToolAsync(string toolName, object parameters = null)
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
                        Name = item["name"]?.ToString(),
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
                        Name = item["name"]?.ToString(),
                        Type = item["type"]?.ToString(),
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
                        Name = item["name"]?.ToString(),
                        Type = item["type"]?.ToString(),
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
                        Name = item["name"]?.ToString()
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
                        Name = item["name"]?.ToString(),
                        AlignmentName = item["alignmentName"]?.ToString(),
                        ProfileName = item["profileName"]?.ToString()
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
    }

    #region Data Models

    public class AlignmentInfo
    {
        public string Name { get; set; }
        public double Length { get; set; }
        public double StartStation { get; set; }
        public double EndStation { get; set; }
    }

    public class SurfaceInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double? MinElevation { get; set; }
        public double? MaxElevation { get; set; }
    }

    public class ProfileInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string AlignmentName { get; set; }
    }

    public class AssemblyInfo
    {
        public string Name { get; set; }
    }

    public class CorridorInfo
    {
        public string Name { get; set; }
        public string AlignmentName { get; set; }
        public string ProfileName { get; set; }
    }

    public class DrawingContext
    {
        public List<AlignmentInfo> Alignments { get; set; } = new List<AlignmentInfo>();
        public List<SurfaceInfo> Surfaces { get; set; } = new List<SurfaceInfo>();
        public List<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();
        public List<CorridorInfo> Corridors { get; set; } = new List<CorridorInfo>();
        public string Error { get; set; }

        public bool HasAlignments => Alignments?.Count > 0;
        public bool HasSurfaces => Surfaces?.Count > 0;
        public bool HasAssemblies => Assemblies?.Count > 0;
        public bool HasCorridors => Corridors?.Count > 0;

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
}
