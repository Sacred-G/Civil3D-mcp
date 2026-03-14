using Newtonsoft.Json.Linq;

namespace Cad_AI_Agent.Models
{
    public class CadCommand
    {
        // ბრძანების სახელი, მაგალითად: "DrawLine"
        public string Action { get; set; } = string.Empty;

        // კოორდინატები: [StartX, StartY, EndX, EndY]
        public double[] Params { get; set; } = System.Array.Empty<double>();
        public JObject? Args { get; set; }
    }
}