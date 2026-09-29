namespace Civil3DMcpPlugin;

/// <summary>
/// Host-independent geometry and unit helpers behind the read-only commands
/// the Civil 3D / Revit bridge relies on (getSurfaceTinVertices,
/// getParcelGeometry, getDrawingUnits). Plain .NET only, so the math can be
/// exercised offline by tests/BridgeMathHarness without Civil 3D.
/// </summary>
public static class BridgeMath
{
  /// <summary>
  /// A plan boundary segment from (X0,Y0) to (X1,Y1). Bulge follows the
  /// AutoCAD polyline convention: tan(sweep/4), positive for a
  /// counter-clockwise arc, 0 for a straight line.
  /// </summary>
  public readonly record struct Segment(double X0, double Y0, double X1, double Y1, double Bulge)
  {
    public bool IsArc => Math.Abs(Bulge) > 1e-12;

    public double ChordLength => Math.Sqrt((X1 - X0) * (X1 - X0) + (Y1 - Y0) * (Y1 - Y0));

    public Segment Reversed() => new(X1, Y1, X0, Y0, -Bulge);
  }

  public readonly record struct ArcInfo(double CenterX, double CenterY, double Radius, double SweepRadians);

  // -------------------------------------------------------------------------
  // Arcs
  // -------------------------------------------------------------------------

  /// <summary>Bulge for a signed sweep angle (radians, positive = CCW).</summary>
  public static double BulgeFromSweep(double sweepRadians) => Math.Tan(sweepRadians / 4.0);

  /// <summary>Centre, radius and signed sweep of an arc segment.</summary>
  public static ArcInfo GetArc(Segment segment)
  {
    var chord = segment.ChordLength;
    var bulge = segment.Bulge;
    var sweep = 4.0 * Math.Atan(bulge);
    var radius = chord * (1.0 + bulge * bulge) / (4.0 * Math.Abs(bulge));
    // Signed distance from the chord midpoint to the centre, measured along
    // the chord's left normal. Positive bulge (CCW) puts the centre on the left
    // for arcs under a semicircle.
    var offset = chord * (1.0 - bulge * bulge) / (4.0 * bulge);
    var midX = (segment.X0 + segment.X1) / 2.0;
    var midY = (segment.Y0 + segment.Y1) / 2.0;
    var nx = -(segment.Y1 - segment.Y0) / chord;
    var ny = (segment.X1 - segment.X0) / chord;
    return new ArcInfo(midX + nx * offset, midY + ny * offset, radius, sweep);
  }

  public static double SegmentLength(Segment segment)
  {
    if (!segment.IsArc) return segment.ChordLength;
    var arc = GetArc(segment);
    return arc.Radius * Math.Abs(arc.SweepRadians);
  }

  public static double Perimeter(IReadOnlyList<Segment> segments) => segments.Sum(SegmentLength);

  /// <summary>
  /// Signed area (positive = counter-clockwise) of a closed chain of segments,
  /// including the circular segments that arcs add or remove.
  /// </summary>
  public static double SignedArea(IReadOnlyList<Segment> segments)
  {
    double area = 0;
    foreach (var s in segments)
    {
      area += (s.X0 * s.Y1 - s.X1 * s.Y0) / 2.0;
      if (s.IsArc)
      {
        var arc = GetArc(s);
        var theta = Math.Abs(arc.SweepRadians);
        var circularSegment = arc.Radius * arc.Radius / 2.0 * (theta - Math.Sin(theta));
        area += Math.Sign(s.Bulge) * circularSegment;
      }
    }

    return area;
  }

  /// <summary>
  /// Polygon vertices for a closed chain: each segment's start point, plus
  /// intermediate points on arcs so that no densified step sweeps more than
  /// <paramref name="maxSegmentAngleRadians"/>. The closing point is not repeated.
  /// </summary>
  public static List<(double X, double Y)> Densify(IReadOnlyList<Segment> segments, double maxSegmentAngleRadians)
  {
    var points = new List<(double X, double Y)>();
    foreach (var s in segments)
    {
      points.Add((s.X0, s.Y0));
      if (!s.IsArc) continue;
      var arc = GetArc(s);
      var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(arc.SweepRadians) / maxSegmentAngleRadians - 1e-9));
      var startAngle = Math.Atan2(s.Y0 - arc.CenterY, s.X0 - arc.CenterX);
      for (var k = 1; k < steps; k++)
      {
        var angle = startAngle + arc.SweepRadians * k / steps;
        points.Add((arc.CenterX + arc.Radius * Math.Cos(angle), arc.CenterY + arc.Radius * Math.Sin(angle)));
      }
    }

    return points;
  }

  /// <summary>
  /// Orders segments into one closed chain, reversing any that run backwards.
  /// Returns null when the segments do not form a single closed loop within
  /// <paramref name="tolerance"/>.
  /// </summary>
  public static List<Segment>? ChainClosed(IReadOnlyList<Segment> segments, double tolerance)
  {
    var usable = segments.Where(s => s.ChordLength > tolerance).ToList();
    if (usable.Count < 2) return null;

    var chain = new List<Segment> { usable[0] };
    usable.RemoveAt(0);
    while (usable.Count > 0)
    {
      var tail = chain[^1];
      var index = usable.FindIndex(s => Near(s.X0, s.Y0, tail.X1, tail.Y1, tolerance));
      if (index >= 0)
      {
        chain.Add(usable[index]);
      }
      else
      {
        index = usable.FindIndex(s => Near(s.X1, s.Y1, tail.X1, tail.Y1, tolerance));
        if (index < 0) return null;
        chain.Add(usable[index].Reversed());
      }

      usable.RemoveAt(index);
    }

    var first = chain[0];
    var last = chain[^1];
    return Near(first.X0, first.Y0, last.X1, last.Y1, tolerance) ? chain : null;
  }

  private static bool Near(double x0, double y0, double x1, double y1, double tolerance) =>
    Math.Abs(x0 - x1) <= tolerance && Math.Abs(y0 - y1) <= tolerance;

  /// <summary>A length tolerance scaled to the coordinates' magnitude.</summary>
  public static double ChainTolerance(IEnumerable<Segment> segments)
  {
    double magnitude = 1;
    foreach (var s in segments)
    {
      magnitude = Math.Max(magnitude, Math.Max(Math.Max(Math.Abs(s.X0), Math.Abs(s.Y0)), Math.Max(Math.Abs(s.X1), Math.Abs(s.Y1))));
    }

    return magnitude * 1e-9 + 1e-6;
  }

  // -------------------------------------------------------------------------
  // Point filtering and decimation
  // -------------------------------------------------------------------------

  /// <summary>Even-odd point-in-polygon test (boundary points may go either way).</summary>
  public static bool PointInPolygon(double x, double y, IReadOnlyList<(double X, double Y)> polygon)
  {
    var inside = false;
    for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
    {
      var (xi, yi) = polygon[i];
      var (xj, yj) = polygon[j];
      if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi)
      {
        inside = !inside;
      }
    }

    return inside;
  }

  /// <summary>
  /// Deterministic stride decimation: <paramref name="max"/> indices spread
  /// evenly over [0, total), always including 0. Returns every index when
  /// total &lt;= max.
  /// </summary>
  public static int[] StrideIndices(int total, int max)
  {
    if (total <= 0 || max <= 0) return Array.Empty<int>();
    if (total <= max) return Enumerable.Range(0, total).ToArray();
    var indices = new int[max];
    for (var i = 0; i < max; i++)
    {
      indices[i] = (int)((long)i * total / max);
    }

    return indices;
  }

  // -------------------------------------------------------------------------
  // Units
  // -------------------------------------------------------------------------

  /// <summary>Metres per drawing unit, keyed by the AutoCAD UnitsValue name.</summary>
  public static double? MetersPerUnit(string? unitName) => unitName switch
  {
    "Inches" => 0.0254,
    "Feet" => 0.3048,
    "Miles" => 1609.344,
    "Millimeters" => 0.001,
    "Centimeters" => 0.01,
    "Meters" => 1.0,
    "Kilometers" => 1000.0,
    "MicroInches" => 0.0254e-6,
    "Mils" => 0.0254e-3,
    "Yards" => 0.9144,
    "Angstroms" => 1e-10,
    "Nanometers" => 1e-9,
    "Microns" => 1e-6,
    "Decimeters" => 0.1,
    "Dekameters" => 10.0,
    "Hectometers" => 100.0,
    "Gigameters" => 1e9,
    "Astronomical" => 149_597_870_700.0,
    "LightYears" => 9_460_730_472_580_800.0,
    "Parsecs" => 30_856_775_814_913_673.0,
    "USSurveyFeet" => 1200.0 / 3937.0,
    "USSurveyInch" => 100.0 / 3937.0,
    "USSurveyYard" => 3600.0 / 3937.0,
    "USSurveyMile" => 6_336_000.0 / 3937.0,
    _ => null,
  };

  public sealed record LengthUnitResolution(
    string? LengthUnit,
    string LengthUnitSource,
    string? CivilLengthUnit,
    bool UnitsConsistent,
    IReadOnlyList<string> Warnings);

  /// <summary>
  /// Resolves the drawing's length unit. INSUNITS is authoritative (it is what
  /// Revit and AutoCAD read when the DWG is linked); Civil 3D's drawing
  /// settings (Feet/Meters plus the imperial-to-metric foot definition) are
  /// used when INSUNITS is Undefined and are compared for consistency.
  /// </summary>
  /// <param name="insunitsName">AutoCAD UnitsValue name, e.g. "Feet", "USSurveyFeet", "Meters", "Undefined".</param>
  /// <param name="civilDrawingUnits">Civil 3D DrawingUnitType name ("Feet" or "Meters"), or null.</param>
  /// <param name="civilImperialToMetric">Civil 3D ImperialToMetricConversionType name ("InternationalFoot" or "UsSurveyFoot"), or null.</param>
  public static LengthUnitResolution ResolveLengthUnit(string? insunitsName, string? civilDrawingUnits, string? civilImperialToMetric)
  {
    var warnings = new List<string>();
    string? civilLengthUnit = civilDrawingUnits switch
    {
      "Meters" => "Meters",
      "Feet" => civilImperialToMetric == "UsSurveyFoot" ? "USSurveyFeet" : "Feet",
      _ => null,
    };

    var insunitsKnown = !string.IsNullOrWhiteSpace(insunitsName) && insunitsName != "Undefined" && MetersPerUnit(insunitsName) != null;
    string? lengthUnit;
    string source;
    if (insunitsKnown)
    {
      lengthUnit = insunitsName;
      source = "INSUNITS";
    }
    else if (civilLengthUnit != null)
    {
      lengthUnit = civilLengthUnit;
      source = "civil3dDrawingSettings";
      warnings.Add($"INSUNITS is '{insunitsName ?? "unknown"}'; the length unit was taken from the Civil 3D drawing settings ({civilLengthUnit}).");
    }
    else
    {
      lengthUnit = null;
      source = "unknown";
      warnings.Add($"INSUNITS is '{insunitsName ?? "unknown"}' and the Civil 3D drawing units are unavailable; the drawing's length unit is unknown.");
    }

    var consistent = true;
    if (insunitsKnown && civilLengthUnit != null && !string.Equals(insunitsName, civilLengthUnit, StringComparison.Ordinal))
    {
      consistent = false;
      var bothFeet = IsFoot(insunitsName) && IsFoot(civilLengthUnit);
      warnings.Add(bothFeet
        ? $"INSUNITS is {insunitsName} but the Civil 3D drawing settings use the {(civilLengthUnit == "USSurveyFeet" ? "US survey foot" : "international foot")} for imperial-to-metric conversion. " +
          "The coordinate numbers are the same; converting them to metres differs by 2 ppm (about 3.7 m at a 6,000,000 ft easting). Decide which foot the project uses before converting."
        : $"INSUNITS is {insunitsName} but the Civil 3D drawing units are {civilLengthUnit}.");
    }

    return new LengthUnitResolution(lengthUnit, source, civilLengthUnit, consistent, warnings);
  }

  private static bool IsFoot(string? unit) => unit is "Feet" or "USSurveyFeet";
}
