using Newtonsoft.Json.Linq;

namespace Cad_AI_Agent.Models
{
    public class CadCommand
    {
        // Command name, e.g. "DrawLine"
        public string Action { get; set; } = string.Empty;

        // Coordinates: [StartX, StartY, EndX, EndY]
        public double[] Params { get; set; } = System.Array.Empty<double>();
        public JObject? Args { get; set; }
    }
}