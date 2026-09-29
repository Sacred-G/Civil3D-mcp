using Civil3DMcpPlugin;
using Segment = Civil3DMcpPlugin.BridgeMath.Segment;

// Offline checks for the host-independent math behind the bridge-support
// commands. Run with: npm run test:bridge-math

const double Eps = 1e-9;

// --- Square 10 x 10, CCW: area, perimeter, orientation ----------------------
var square = new List<Segment>
{
  new(0, 0, 10, 0, 0),
  new(10, 0, 10, 10, 0),
  new(10, 10, 0, 10, 0),
  new(0, 10, 0, 0, 0),
};
Near(BridgeMath.SignedArea(square), 100, "square area");
Near(BridgeMath.Perimeter(square), 40, "square perimeter");
Near(BridgeMath.SignedArea(square.Select(s => s.Reversed()).Reverse().ToList()), -100, "reversed square is CW");

// --- Semicircle bulge: bulge 1 on a chord of 2 is a half circle of radius 1 ---
var half = new Segment(1, 0, -1, 0, 1); // CCW from (1,0) to (-1,0): bulges to the right of travel = +Y side
var arc = BridgeMath.GetArc(half);
Near(arc.Radius, 1, "semicircle radius");
Near(arc.CenterX, 0, "semicircle centre x");
Near(arc.CenterY, 0, "semicircle centre y");
Near(arc.SweepRadians, Math.PI, "semicircle sweep");
Near(BridgeMath.SegmentLength(half), Math.PI, "semicircle length");

// Full circle as two CCW half circles: area pi, perimeter 2 pi.
var circle = new List<Segment> { new(1, 0, -1, 0, 1), new(-1, 0, 1, 0, 1) };
Near(BridgeMath.SignedArea(circle), Math.PI, "circle area");
Near(BridgeMath.Perimeter(circle), 2 * Math.PI, "circle perimeter");

// Densified circle points stay on the circle; 5 degree steps -> 72 points.
var dense = BridgeMath.Densify(circle, 5 * Math.PI / 180);
Assert(dense.Count == 72, $"circle densified to {dense.Count} points, expected 72");
Assert(dense.All(p => Math.Abs(Math.Sqrt(p.X * p.X + p.Y * p.Y) - 1) < 1e-9), "densified points must lie on the arc");
// The first densified step from (1,0) goes CCW (positive y).
Assert(dense[1].Y > 0, "CCW arc must densify towards +y");

// --- Lot with a curved front (quarter circle bulge) -------------------------
// 0,0 -> 20,0 -> 20,20 -> arc -> 0,20 -> 0,0.
// A 90 degree CCW arc from (20,20) to (0,20) has bulge tan(pi/8), centre (10,10),
// and bows toward +y (right of travel), i.e. out of the lot: area = 400 + circular segment.
var q = Math.Tan(Math.PI / 8);
var lot = new List<Segment>
{
  new(0, 0, 20, 0, 0),
  new(20, 0, 20, 20, 0),
  new(20, 20, 0, 20, q),
  new(0, 20, 0, 0, 0),
};
var r = 20 / Math.Sqrt(2);
var segmentArea = r * r / 2 * (Math.PI / 2 - 1);
Near(BridgeMath.SignedArea(lot), 400 + segmentArea, "positive bulge on a CCW polygon adds area (bows outward)");
var lotArc = BridgeMath.GetArc(lot[2]);
Near(lotArc.Radius, r, "lot arc radius");
Near(lotArc.CenterX, 10, "lot arc centre x");
Near(lotArc.CenterY, 10, "lot arc centre y (centre on the left of travel for CCW)");

// --- Chaining: shuffled and reversed segments reassemble into one loop -------
var shuffled = new List<Segment> { lot[2], lot[0].Reversed(), lot[3], lot[1] };
var chained = BridgeMath.ChainClosed(shuffled, 1e-6);
Assert(chained != null && chained.Count == 4, "shuffled lot should chain");
Near(Math.Abs(BridgeMath.SignedArea(chained!)), 400 + segmentArea, "chained area magnitude preserved");
// The arc arrives reversed, so ChainClosed must flip it back: its bulge sign
// has to be negated with the endpoints or the arc would bow inward.
var arcReversed = new List<Segment> { lot[0], lot[2].Reversed(), lot[3], lot[1] };
var chainedArc = BridgeMath.ChainClosed(arcReversed, 1e-6);
Assert(chainedArc != null && chainedArc.Count == 4, "lot with a reversed arc should chain");
Assert(chainedArc![2] == lot[2], "reversed arc should be restored to its original direction and bulge");
Near(BridgeMath.SignedArea(chainedArc), 400 + segmentArea, "reversed-arc chain keeps the CCW area including the arc segment");
Assert(BridgeMath.ChainClosed(new List<Segment> { lot[0], lot[1], lot[2] }, 1e-6) == null, "open chain must be rejected");
// Zero-length segments (duplicate vertices) are dropped.
var withDup = new List<Segment>(square) { new(0, 0, 0, 0, 0) };
Assert(BridgeMath.ChainClosed(withDup, 1e-6)?.Count == 4, "zero-length segment should be dropped");
// State-plane magnitude tolerance.
var tol = BridgeMath.ChainTolerance(new[] { new Segment(6_000_000, 2_000_000, 6_000_010, 2_000_000, 0) });
Assert(tol > 1e-6 && tol < 1e-2, $"tolerance {tol} out of range");

// --- Point in polygon --------------------------------------------------------
var poly = new List<(double X, double Y)> { (0, 0), (10, 0), (10, 10), (0, 10) };
Assert(BridgeMath.PointInPolygon(5, 5, poly), "centre inside");
Assert(!BridgeMath.PointInPolygon(15, 5, poly), "outside x");
Assert(!BridgeMath.PointInPolygon(5, -1, poly), "outside y");
var concave = new List<(double X, double Y)> { (0, 0), (10, 0), (10, 10), (5, 5), (0, 10) };
Assert(!BridgeMath.PointInPolygon(5, 8, concave), "concave notch is outside");

// --- Stride decimation -------------------------------------------------------
Assert(BridgeMath.StrideIndices(5, 10).SequenceEqual(new[] { 0, 1, 2, 3, 4 }), "no decimation under the cap");
var stride = BridgeMath.StrideIndices(1000, 100);
Assert(stride.Length == 100 && stride[0] == 0 && stride[1] == 10 && stride[^1] == 990, "even stride");
Assert(stride.SequenceEqual(BridgeMath.StrideIndices(1000, 100)), "deterministic");
var odd = BridgeMath.StrideIndices(1_000_003, 100_000);
Assert(odd.Length == 100_000 && odd.Distinct().Count() == 100_000 && odd[^1] < 1_000_003, "large stride unique and in range");
Assert(BridgeMath.StrideIndices(0, 10).Length == 0, "empty input");

// --- Length unit resolution --------------------------------------------------
var usft = BridgeMath.ResolveLengthUnit("USSurveyFeet", "Feet", "UsSurveyFoot");
Assert(usft.LengthUnit == "USSurveyFeet" && usft.LengthUnitSource == "INSUNITS" && usft.UnitsConsistent && usft.Warnings.Count == 0, "US survey feet drawing");
var ift = BridgeMath.ResolveLengthUnit("Feet", "Feet", "InternationalFoot");
Assert(ift.LengthUnit == "Feet" && ift.CivilLengthUnit == "Feet" && ift.UnitsConsistent, "international feet drawing");
var mixed = BridgeMath.ResolveLengthUnit("Feet", "Feet", "UsSurveyFoot");
Assert(mixed.LengthUnit == "Feet" && mixed.CivilLengthUnit == "USSurveyFeet" && !mixed.UnitsConsistent && mixed.Warnings.Count == 1, "INSUNITS feet vs Civil US survey foot is flagged");
var undefinedIns = BridgeMath.ResolveLengthUnit("Undefined", "Feet", "UsSurveyFoot");
Assert(undefinedIns.LengthUnit == "USSurveyFeet" && undefinedIns.LengthUnitSource == "civil3dDrawingSettings", "undefined INSUNITS falls back to Civil settings");
var metric = BridgeMath.ResolveLengthUnit("Meters", "Meters", "InternationalFoot");
Assert(metric.LengthUnit == "Meters" && metric.UnitsConsistent, "metric drawing");
var clash = BridgeMath.ResolveLengthUnit("Meters", "Feet", "InternationalFoot");
Assert(clash.LengthUnit == "Meters" && !clash.UnitsConsistent, "metres vs feet clash flagged");
var unknown = BridgeMath.ResolveLengthUnit("Undefined", null, null);
Assert(unknown.LengthUnit == null && unknown.LengthUnitSource == "unknown", "unknown units");
Near(BridgeMath.MetersPerUnit("USSurveyFeet")!.Value, 1200.0 / 3937.0, "US survey foot");
Near(BridgeMath.MetersPerUnit("Feet")!.Value, 0.3048, "international foot");
Assert(BridgeMath.MetersPerUnit("Undefined") == null, "undefined has no scale");
// The two feet differ by ~2 ppm: ~3.66 m at a 6,000,000 ft easting.
var diff = 6_000_000 * (BridgeMath.MetersPerUnit("USSurveyFeet")!.Value - BridgeMath.MetersPerUnit("Feet")!.Value);
Assert(Math.Abs(diff - 3.6576) < 0.001, $"survey/international foot difference {diff}");

Console.WriteLine("Bridge math harness passed.");
return 0;

static void Near(double actual, double expected, string message)
{
  if (Math.Abs(actual - expected) > Eps * Math.Max(1, Math.Abs(expected)))
  {
    throw new InvalidOperationException($"{message}: expected {expected}, got {actual}");
  }
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
