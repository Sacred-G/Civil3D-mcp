using Civil3DMcpPlugin;
using V = Civil3DMcpPlugin.VerticalCurveMath;

// Offline checks for the K-value math behind civil3d_profile check_k_values.
// Run with: npm run test:vertical-curve-math

const double Eps = 1e-9;

// --- K uses A in percent -----------------------------------------------------
// The live bug: a 280 ft curve between -1.4 % and +1.4 % reported K ~ 9,980
// because A was taken as the decimal 0.028. K must be 280 / 2.8 = 100.
Near(V.GradeDifferencePercent(-0.014, 0.014), 2.8, "A in percent");
Near(V.ComputeK(280, -0.014, 0.014)!.Value, 100, "K = L / A(%)");
Near(V.ComputeK(280, 0.014, -0.014)!.Value, 100, "K uses |A|");
Assert(V.ComputeK(280, 0.02, 0.02) == null, "equal grades have no finite K");
Near(V.ComputeK(280, 0, 1e-12)!.Value, 280 / (100 * 1e-12), "a very flat but valid curve keeps its finite K");

// --- Sag / crest by sign of g2 - g1 -----------------------------------------
Assert(V.IsSag(-0.014, 0.014), "grade increasing is a sag");
Assert(!V.IsSag(0.03, -0.01), "grade decreasing is a crest");
Assert(!V.IsSag(0.02, 0.01), "flattening upgrade is a crest");
Assert(V.IsSag(-0.03, -0.01), "flattening downgrade is a sag");

// --- mph table: exact rows ---------------------------------------------------
var mph50 = V.LookupRow(V.Mph, 50);
Assert(mph50.ExactMatch && mph50.Note == null, "50 mph is an exact row");
Near(mph50.Row.KCrest, 84, "50 mph crest K");
Near(mph50.Row.KSag, 96, "50 mph sag K");
Assert(mph50.KUnits == "ft/%", "mph K units");
Near(V.LookupRow(V.Mph, 30).Row.KSag, 37, "30 mph sag K");
Near(V.LookupRow(V.Mph, 80).Row.KCrest, 384, "80 mph crest K");
Near(V.LookupRow(V.Mph, 55).Row.KCrest, 114, "55 mph crest K");
Near(V.LookupRow(V.Mph, 55).Row.KSag, 115, "55 mph sag K");

// --- Between rows: next higher row (conservative) ----------------------------
var mph52 = V.LookupRow(V.Mph, 52);
Assert(!mph52.ExactMatch && mph52.Note != null, "52 mph is between rows and says so");
Near(mph52.Row.Speed, 55, "52 mph uses the 55 mph row");
Near(mph52.Row.KCrest, 114, "52 mph crest K from 55 mph row");
Near(V.LookupRow(V.Mph, 30.5).Row.Speed, 35, "30.5 mph uses 35 mph");

// --- km/h table -------------------------------------------------------------
var kmh50 = V.LookupRow(V.Kmh, 50);
Assert(kmh50.ExactMatch && kmh50.KUnits == "m/%", "50 km/h exact, K in m/%");
Near(kmh50.Row.KCrest, 7, "50 km/h crest K");
Near(kmh50.Row.KSag, 13, "50 km/h sag K");
Near(V.LookupRow(V.Kmh, 20).Row.KSag, 3, "20 km/h sag K");
Near(V.LookupRow(V.Kmh, 130).Row.KCrest, 124, "130 km/h crest K");
Near(V.LookupRow(V.Kmh, 95).Row.Speed, 100, "95 km/h uses 100 km/h");

// --- Low speeds: the mph table starts at 15 mph, like AASHTO's ----------------
Near(V.LookupRow(V.Mph, 15).Row.KCrest, 3, "15 mph crest K");
Near(V.LookupRow(V.Mph, 15).Row.KSag, 10, "15 mph sag K");
Near(V.LookupRow(V.Mph, 20).Row.KCrest, 7, "20 mph crest K");
Near(V.LookupRow(V.Mph, 20).Row.KSag, 17, "20 mph sag K");
var mph25 = V.LookupRow(V.Mph, 25);
Assert(mph25.ExactMatch, "25 mph is an exact row");
Near(mph25.Row.KCrest, 12, "25 mph crest K");
Near(mph25.Row.KSag, 26, "25 mph sag K");
Near(V.LookupRow(V.Mph, 27).Row.Speed, 30, "27 mph uses 30 mph");

// --- Within the tolerance of a row is that row --------------------------------
Assert(V.LookupRow(V.Mph, 80 + V.SpeedTolerance).ExactMatch, "80 mph + tolerance is the 80 mph row");
Assert(V.LookupRow(V.Kmh, 20 - V.SpeedTolerance).ExactMatch, "20 km/h - tolerance is the 20 km/h row");

// --- Only the canonical speed units select a table ---------------------------
Throws<ArgumentException>(() => V.LookupRow("MPH", 50), "non-canonical 'MPH' is refused, not read as km/h");
Throws<ArgumentException>(() => V.LookupRow("knots", 50), "unknown speed units are refused");

// --- Outside the table -------------------------------------------------------
Throws<ArgumentOutOfRangeException>(() => V.LookupRow(V.Mph, 10), "10 mph is below the table");
Throws<ArgumentOutOfRangeException>(() => V.LookupRow(V.Mph, 85), "85 mph is above the table");
Throws<ArgumentOutOfRangeException>(() => V.LookupRow(V.Kmh, 10), "10 km/h is below the table");
Throws<ArgumentOutOfRangeException>(() => V.LookupRow(V.Kmh, 140), "140 km/h is above the table");

// --- Speed units -------------------------------------------------------------
Assert(V.NormalizeSpeedUnits(null) == null, "no speed units");
Assert(V.NormalizeSpeedUnits("MPH") == V.Mph, "mph normalised");
Assert(V.NormalizeSpeedUnits("km/h") == V.Kmh, "km/h normalised");
Assert(V.NormalizeSpeedUnits("kph") == V.Kmh, "kph alias");
Throws<ArgumentException>(() => V.NormalizeSpeedUnits("knots"), "unknown speed unit");
Assert(V.DefaultSpeedUnitsForLengthUnit("Feet") == V.Mph, "feet -> mph");
Assert(V.DefaultSpeedUnitsForLengthUnit("USSurveyFeet") == V.Mph, "US survey feet -> mph");
Assert(V.DefaultSpeedUnitsForLengthUnit("Meters") == V.Kmh, "meters -> km/h");
Assert(V.DefaultSpeedUnitsForLengthUnit(null) == null, "unknown -> none");
Assert(V.DefaultSpeedUnitsForLengthUnit("Undefined") == null, "undefined -> none");

// --- Drawing length unit -----------------------------------------------------
Assert(V.ResolveLengthUnit("Feet", "Meters", null) == "Feet", "INSUNITS wins when it names a length unit");
Assert(V.ResolveLengthUnit("Undefined", "Feet", "UsSurveyFoot") == "USSurveyFeet", "unitless INSUNITS uses Civil 3D units (US survey foot)");
Assert(V.ResolveLengthUnit("Undefined", "Feet", "InternationalFoot") == "Feet", "unitless INSUNITS uses Civil 3D units (international foot)");
Assert(V.ResolveLengthUnit(null, "Meters", null) == "Meters", "no INSUNITS uses Civil 3D meters");
Assert(V.ResolveLengthUnit("Undefined", null, null) == null, "nothing known -> null");
Near(V.MetersPerUnit("USSurveyFeet")!.Value, 1200.0 / 3937.0, "US survey foot factor");
Assert(V.MetersPerUnit("Parsecs") == null, "non-civil unit has no factor");

// --- Lengths converted to the table's unit -----------------------------------
Near(V.LengthToTableUnits(280, V.MetersPerUnit("Feet"), V.Mph), 280, "feet drawing, mph table");
Near(V.LengthToTableUnits(100, V.MetersPerUnit("Meters"), V.Kmh), 100, "metre drawing, km/h table");
Near(V.LengthToTableUnits(100, V.MetersPerUnit("Meters"), V.Mph), 100 / 0.3048, "metre drawing, mph table");
Near(V.LengthToTableUnits(328.084, V.MetersPerUnit("Feet"), V.Kmh), 328.084 * 0.3048, "feet drawing, km/h table");
Near(V.LengthToTableUnits(280, null, V.Mph), 280, "unknown unit passes through");

// --- The live case end to end ------------------------------------------------
// 280 ft sag, -1.4 % -> +1.4 %, 50 mph: K = 100 >= 96 required -> passes.
var k = V.ComputeK(V.LengthToTableUnits(280, V.MetersPerUnit("Feet"), V.Mph), -0.014, 0.014)!.Value;
var required = V.IsSag(-0.014, 0.014) ? mph50.Row.KSag : mph50.Row.KCrest;
Assert(k >= required && required == 96, $"live case K {k} vs required {required}");
// The same curve as a crest at 50 mph needs 84 and also passes; at 55 mph (114) it fails.
Assert(k < V.LookupRow(V.Mph, 55).Row.KCrest, "fails a 55 mph crest");

Console.WriteLine("Vertical curve math harness passed.");

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

static void Throws<T>(Action action, string message) where T : Exception
{
  try
  {
    action();
  }
  catch (T)
  {
    return;
  }

  throw new InvalidOperationException($"{message}: expected {typeof(T).Name}");
}
