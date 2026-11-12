namespace Civil3DMcpPlugin;

/// <summary>
/// Host-independent vertical-curve K-value math behind civil3d_profile
/// check_k_values. Plain .NET only, so it can be exercised offline by
/// tests/VerticalCurveMathHarness without Civil 3D.
///
/// Civil 3D reports profile grades as decimal ratios (0.028 = 2.8 %). K is
/// defined with the algebraic grade difference A in PERCENT:
/// K = L / A, A = 100 * |g2 - g1|.
/// </summary>
public static class VerticalCurveMath
{
  public const string Mph = "mph";
  public const string Kmh = "km/h";

  /// <summary>One AASHTO stopping-sight-distance design row: minimum K for crest and sag curves.</summary>
  public sealed record KRow(double Speed, double KCrest, double KSag);

  /// <summary>
  /// US customary: design speed in mph, K in ft/%. AASHTO Green Book stopping
  /// sight distance design K (crest: Table 3-34; sag: Table 3-36), from 15 mph
  /// like the tables themselves.
  /// </summary>
  public static readonly IReadOnlyList<KRow> MphTable =
  [
    new(15, 3, 10),
    new(20, 7, 17),
    new(25, 12, 26),
    new(30, 19, 37),
    new(35, 29, 49),
    new(40, 44, 64),
    new(45, 61, 79),
    new(50, 84, 96),
    new(55, 114, 115),
    new(60, 151, 136),
    new(65, 193, 157),
    new(70, 247, 181),
    new(75, 312, 206),
    new(80, 384, 231),
  ];

  /// <summary>Metric: design speed in km/h, K in m/%. Same AASHTO tables, metric edition.</summary>
  public static readonly IReadOnlyList<KRow> KmhTable =
  [
    new(20, 1, 3),
    new(30, 2, 6),
    new(40, 4, 9),
    new(50, 7, 13),
    new(60, 11, 18),
    new(70, 17, 23),
    new(80, 26, 30),
    new(90, 39, 38),
    new(100, 52, 45),
    new(110, 74, 55),
    new(120, 95, 63),
    new(130, 124, 73),
  ];

  public sealed record KLookup(KRow Row, bool ExactMatch, string SpeedUnits, string KUnits, string? Note);

  /// <summary>Canonical "mph" / "km/h", null when not given; ArgumentException for anything else.</summary>
  public static string? NormalizeSpeedUnits(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    return value.Trim().ToLowerInvariant() switch
    {
      "mph" => Mph,
      "km/h" or "kmh" or "kph" or "kmph" => Kmh,
      _ => throw new ArgumentException($"speedUnits must be \"mph\" or \"km/h\", not '{value}'."),
    };
  }

  /// <summary>Metres per drawing length unit, keyed by the AutoCAD UnitsValue name; null for other units.</summary>
  public static double? MetersPerUnit(string? unitName) => unitName switch
  {
    "Inches" => 0.0254,
    "Feet" => 0.3048,
    "Yards" => 0.9144,
    "Miles" => 1609.344,
    "Millimeters" => 0.001,
    "Centimeters" => 0.01,
    "Decimeters" => 0.1,
    "Meters" => 1.0,
    "Kilometers" => 1000.0,
    "USSurveyFeet" => 1200.0 / 3937.0,
    "USSurveyInch" => 100.0 / 3937.0,
    "USSurveyYard" => 3600.0 / 3937.0,
    "USSurveyMile" => 6_336_000.0 / 3937.0,
    _ => null,
  };

  /// <summary>
  /// The drawing's length unit: INSUNITS when it names a known length unit,
  /// otherwise the Civil 3D drawing units ("Feet" — "USSurveyFeet" when the
  /// imperial-to-metric conversion is the US survey foot — or "Meters"),
  /// otherwise null.
  /// </summary>
  public static string? ResolveLengthUnit(string? insunitsName, string? civilDrawingUnits, string? civilImperialToMetric)
  {
    if (MetersPerUnit(insunitsName) != null)
    {
      return insunitsName;
    }

    return civilDrawingUnits switch
    {
      "Meters" => "Meters",
      "Feet" => civilImperialToMetric == "UsSurveyFoot" ? "USSurveyFeet" : "Feet",
      _ => null,
    };
  }

  /// <summary>
  /// The speed unit implied by a drawing length unit (AutoCAD UnitsValue names):
  /// imperial lengths → mph, metric → km/h, unknown → null.
  /// </summary>
  public static string? DefaultSpeedUnitsForLengthUnit(string? lengthUnit) => lengthUnit switch
  {
    "Feet" or "USSurveyFeet" or "Inches" or "USSurveyInch" or "Yards" or "USSurveyYard" or "Miles" or "USSurveyMile" => Mph,
    "Meters" or "Millimeters" or "Centimeters" or "Decimeters" or "Kilometers" => Kmh,
    _ => null,
  };

  public static string KUnitsFor(string speedUnits) => speedUnits == Mph ? "ft/%" : "m/%";

  /// <summary>
  /// The design row for a speed. An exact row is used when one exists;
  /// otherwise the next HIGHER speed row (conservative). A speed within
  /// SpeedTolerance of a row is that row. Speeds outside the table throw
  /// ArgumentOutOfRangeException; speed units other than the canonical
  /// <see cref="Mph"/> / <see cref="Kmh"/> (see NormalizeSpeedUnits) throw
  /// ArgumentException instead of silently using the km/h table.
  /// </summary>
  public static KLookup LookupRow(string speedUnits, double designSpeed)
  {
    var table = speedUnits switch
    {
      Mph => MphTable,
      Kmh => KmhTable,
      _ => throw new ArgumentException($"speedUnits must be \"{Mph}\" or \"{Kmh}\", not '{speedUnits}'.", nameof(speedUnits)),
    };
    if (double.IsNaN(designSpeed) || designSpeed < table[0].Speed - SpeedTolerance || designSpeed > table[^1].Speed + SpeedTolerance)
    {
      throw new ArgumentOutOfRangeException(
        nameof(designSpeed),
        $"Design speed {designSpeed} {speedUnits} is outside the AASHTO K table ({table[0].Speed}-{table[^1].Speed} {speedUnits}).");
    }

    foreach (var row in table)
    {
      // Same expressions as the range check, so a speed it admits just above
      // the last row (or just below the first) is matched to that row here.
      if (designSpeed >= row.Speed - SpeedTolerance && designSpeed <= row.Speed + SpeedTolerance)
      {
        return new KLookup(row, true, speedUnits, KUnitsFor(speedUnits), null);
      }

      if (row.Speed > designSpeed)
      {
        return new KLookup(
          row,
          false,
          speedUnits,
          KUnitsFor(speedUnits),
          $"{designSpeed} {speedUnits} is between table rows; the next higher row ({row.Speed} {speedUnits}) was used (conservative).");
      }
    }

    // Unreachable: the range check admits at most the last row's speed plus
    // SpeedTolerance, and that speed matches the last row as an exact row above.
    throw new InvalidOperationException($"No AASHTO K row for {designSpeed} {speedUnits}.");
  }

  /// <summary>How close a design speed must be to a table row to count as that row.</summary>
  public const double SpeedTolerance = 1e-9;

  /// <summary>Algebraic grade difference A in percent from decimal grades.</summary>
  public static double GradeDifferencePercent(double gradeIn, double gradeOut) => 100.0 * Math.Abs(gradeOut - gradeIn);

  /// <summary>K = L / A (A in percent). Null only when the grades are equal (no curve needed, K is infinite); a very flat curve still gets its finite K.</summary>
  public static double? ComputeK(double curveLength, double gradeIn, double gradeOut)
  {
    var a = GradeDifferencePercent(gradeIn, gradeOut);
    return a > 0 ? curveLength / a : null;
  }

  /// <summary>Sag when the grade increases through the curve (g2 &gt; g1), crest otherwise.</summary>
  public static bool IsSag(double gradeIn, double gradeOut) => gradeOut > gradeIn;

  /// <summary>
  /// Converts a length in drawing units to the table's length unit (feet for
  /// mph, metres for km/h). With an unknown drawing unit the length is taken
  /// to already be in the table's unit.
  /// </summary>
  public static double LengthToTableUnits(double length, double? metersPerDrawingUnit, string speedUnits)
  {
    if (metersPerDrawingUnit is not double factor || factor <= 0)
    {
      return length;
    }

    var meters = length * factor;
    return speedUnits == Mph ? meters / 0.3048 : meters;
  }
}
