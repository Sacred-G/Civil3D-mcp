namespace Cad_AI_Agent.Models
{
    public class CadCommand
    {
        // ბრძანების სახელი, მაგალითად: "DrawLine"
        public string Action { get; set; }

        // კოორდინატები: [StartX, StartY, EndX, EndY]
        public double[] Params { get; set; }
    }
}