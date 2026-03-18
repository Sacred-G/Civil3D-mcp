using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cad_AI_Agent.Services
{
    /// <summary>
    /// HTTP client for a Playwright MCP server.
    /// Expects the same /health + /execute protocol used by the Civil 3D MCP server.
    /// Default URL: http://localhost:3001 (Playwright MCP default port).
    /// </summary>
    public class PlaywrightMcpClient
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private bool _isAvailable;

        public PlaywrightMcpClient(string baseUrl = "http://localhost:3001")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _isAvailable = false;
        }

        public bool IsAvailable => _isAvailable;

        public void UpdateBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            _isAvailable = false;
        }

        /// <summary>Check if the Playwright MCP server is reachable.</summary>
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

        /// <summary>
        /// Execute any Playwright MCP tool by name and return the raw result as JObject.
        /// Tool names follow Playwright MCP conventions: browser_navigate, browser_screenshot, etc.
        /// </summary>
        public async Task<JObject> ExecuteToolAsync(string toolName, JObject? args = null)
        {
            if (!_isAvailable)
                throw new InvalidOperationException("Playwright MCP server is not available.");

            var request = new
            {
                tool = toolName,
                parameters = (object?)args ?? new { }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/execute", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Playwright MCP tool '{toolName}' failed ({(int)response.StatusCode}): {body}");

            return JObject.Parse(body);
        }
    }
}
