using System.Text.Json.Nodes;
using Civil3DMcpPlugin;

// Offline checks for the host-independent drawing fingerprint diff used by
// civil3d_compare. Run with: npm run test:compare-diff

var baseline = new DrawingFingerprint
{
  CapturedAtUtc = "2026-09-01T00:00:00.0000000Z",
  SourcePath = @"C:\work\submittal-1.dwg",
  SourceKind = "side_database",
  Entities =
  {
    Entity("1A", "LINE", "C-ROAD", "a1"),
    Entity("1B", "LWPOLYLINE", "C-ROAD", "b1"),
    Entity("1C", "TEXT", "C-ANNO", "c1"),
    Entity("1D", "CIRCLE", "C-UTIL", "d1"),
    Entity("1E", "LINE", "C-ROAD", "e1"),
  },
  CivilObjects =
  {
    Civil("alignment", "Main St", "h1", new JsonObject { ["length"] = 500.0, ["geometryHash"] = "AAAA" }),
    Civil("profile", "Main St/FG", "p1", new JsonObject
    {
      ["pviCount"] = 2,
      ["pvis"] = new JsonArray(Pvi("0", 100.0), Pvi("500", 105.0)),
    }),
    Civil("surface", "EG", "s1", new JsonObject { ["numberOfPoints"] = 100, ["maximumElevation"] = 120.0 }),
    Civil("pipe_network", "Storm", "n1", new JsonObject
    {
      ["pipeCount"] = 1,
      ["pipes"] = new JsonArray(new JsonObject { ["key"] = "P1", ["startInvert"] = 95.0, ["endInvert"] = 94.0 }),
    }),
  },
};

var current = new DrawingFingerprint
{
  CapturedAtUtc = "2026-09-20T00:00:00.0000000Z",
  SourcePath = @"C:\work\design.dwg",
  SourceKind = "active_drawing",
  Entities =
  {
    Entity("1A", "LINE", "C-ROAD", "a1"),            // unchanged
    Entity("1B", "LWPOLYLINE", "C-ROAD", "b2"),      // modified geometry
    Entity("1C", "TEXT", "C-ANNO-NEW", "c2"),        // modified + layer change
    Entity("1E", "CIRCLE", "C-ROAD", "e2"),          // handle recycled with new type => removed + added
    Entity("2F", "INSERT", "C-UTIL", "f1"),          // added
    // 1D removed
  },
  CivilObjects =
  {
    Civil("alignment", "Main St", "h1", new JsonObject { ["length"] = 520.0, ["geometryHash"] = "BBBB" }),
    Civil("profile", "Main St/FG", "p1", new JsonObject
    {
      ["pviCount"] = 3,
      ["pvis"] = new JsonArray(Pvi("0", 100.0), Pvi("250", 103.0), Pvi("500", 106.0)),
    }),
    Civil("surface", "EG", "s1", new JsonObject { ["numberOfPoints"] = 100, ["maximumElevation"] = 120.0 }),
    Civil("surface", "FG", "s2", new JsonObject { ["numberOfPoints"] = 40 }),
  },
};

var result = DrawingFingerprintDiff.Compare(baseline, current, maxDetails: 10);
var summary = (Dictionary<string, object?>)result["summary"]!;
Assert((int)summary["added"]! == 2, $"added: {summary["added"]}");
Assert((int)summary["removed"]! == 2, $"removed: {summary["removed"]}");
Assert((int)summary["modified"]! == 2, $"modified: {summary["modified"]}");
Assert((int)summary["unchanged"]! == 1, $"unchanged: {summary["unchanged"]}");
Assert((int)summary["layerChanges"]! == 1, $"layerChanges: {summary["layerChanges"]}");
Assert((bool)summary["identical"]! == false, "identical should be false");

var details = (Dictionary<string, object?>)result["details"]!;
var modifiedRows = (List<Dictionary<string, object?>>)details["modified"]!;
Assert(modifiedRows.Any(row => (string)row["handle"]! == "1C" && (string)row["previousLayer"]! == "C-ANNO"), "layer change not reported");

var byType = (List<Dictionary<string, object?>>)result["byType"]!;
var lineRow = byType.Single(row => (string)row["name"]! == "LINE");
Assert((int)lineRow["removed"]! == 1 && (int)lineRow["added"]! == 0, "LINE group wrong");

var civil = (Dictionary<string, object?>)result["civil"]!;
var civilModified = (List<Dictionary<string, object?>>)civil["modified"]!;
Assert(civilModified.Count == 2, $"civil modified: {civilModified.Count}");
var alignmentChanges = (Dictionary<string, object?>)civilModified.Single(row => (string)row["kind"]! == "alignment")["changes"]!;
Assert(alignmentChanges.ContainsKey("length") && alignmentChanges.ContainsKey("geometryHash"), "alignment field diff missing");
var profileChanges = (Dictionary<string, object?>)civilModified.Single(row => (string)row["kind"]! == "profile")["changes"]!;
var pviDiff = (Dictionary<string, object?>)profileChanges["pvis"]!;
Assert((int)pviDiff["addedCount"]! == 1 && (int)pviDiff["changedCount"]! == 1 && (int)pviDiff["removedCount"]! == 0, "keyed PVI diff wrong");
Assert(((List<Dictionary<string, object?>>)civil["added"]!).Single()["name"] as string == "FG", "added surface missing");
Assert(((List<Dictionary<string, object?>>)civil["removed"]!).Single()["name"] as string == "Storm", "removed network missing");
Assert((int)civil["unchanged"]! == 1, "EG should be unchanged");

// Round trip through JSON (snapshot files) and self-compare is identical.
var roundTripped = DrawingFingerprint.FromJson(current.ToJson());
var self = DrawingFingerprintDiff.Compare(roundTripped, current, 10);
Assert((bool)((Dictionary<string, object?>)self["summary"]!)["identical"]!, "round-tripped snapshot should compare identical");
// The diff only looks at hashes, so also require the serialized form itself
// to survive the round trip (summaries, handles, layers, warnings).
Assert(roundTripped.ToJson() == current.ToJson(), "snapshot JSON should round-trip unchanged");

// The capture detail level survives a snapshot round trip; older snapshots
// without it read as full detail.
var fastPrint = new DrawingFingerprint { EntityDetail = DrawingFingerprint.FastDetail };
Assert(DrawingFingerprint.FromJson(fastPrint.ToJson()).IsFast, "fast detail should round-trip");
Assert(!DrawingFingerprint.FromJson(new DrawingFingerprint().ToJson()).IsFast, "a snapshot without entityDetail is full detail");

// Surface vertex hash: order-independent, sensitive to any one moved vertex.
var points = new[] { (1.0, 2.0, 3.0), (4.5, 5.25, 6.125), (7_000_000.123456, 2_500_000.5, 101.25), (0.0, 0.0, 0.0) };
string HashOf(IEnumerable<(double X, double Y, double Z)> set)
{
  var hash = new PointSetHash();
  foreach (var (x, y, z) in set)
  {
    hash.Add(x, y, z);
  }

  return hash.Value;
}

var forward = HashOf(points);
Assert(forward == HashOf(points.Reverse()), "vertex hash should not depend on order");
Assert(forward != HashOf(points.Select((p, i) => i == 2 ? (p.Item1, p.Item2, p.Item3 + 0.001) : p)), "a re-elevated vertex should change the hash");
Assert(forward != HashOf(points.Select((p, i) => i == 1 ? (p.Item2, p.Item1, p.Item3) : p)), "swapped x/y should change the hash");
Assert(forward != HashOf(points.Take(3)), "a removed vertex should change the hash");
// Two vertices trading elevations keep count, min, max and mean; the hash still changes.
var traded = new[] { (1.0, 2.0, 6.125), (4.5, 5.25, 3.0), points[2], points[3] };
Assert(forward != HashOf(traded), "vertices trading elevations should change the hash");

// maxDetails truncates rows but never counts.
var truncated = DrawingFingerprintDiff.Compare(baseline, current, 0);
var truncatedDetails = (Dictionary<string, object?>)truncated["details"]!;
Assert((bool)truncatedDetails["truncated"]! && ((List<Dictionary<string, object?>>)truncatedDetails["added"]!).Count == 0, "truncation wrong");
Assert((int)((Dictionary<string, object?>)truncated["summary"]!)["added"]! == 2, "counts must not truncate");
var truncatedCivil = (Dictionary<string, object?>)truncated["civil"]!;
Assert((bool)truncatedCivil["truncated"]! && ((List<Dictionary<string, object?>>)truncatedCivil["modified"]!).Count == 0, "civil truncation not reported");
Assert(!(bool)civil["truncated"]!, "civil diff under maxDetails should not be truncated");

// Schema-tagged but structurally malformed snapshots are invalid input, not internal errors.
const string schemaTag = "\"schema\":\"civil3d-mcp/drawing-fingerprint@1\"";
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("{" + schemaTag + ",\"entities\":[null]}"));
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("{" + schemaTag + ",\"entities\":[{\"handle\":\"1A\",\"type\":null,\"hash\":\"x\"}]}"));
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("{" + schemaTag + ",\"civilObjects\":[{\"kind\":\"surface\",\"name\":\"EG\",\"hash\":\"x\",\"summary\":null}]}"));
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("{" + schemaTag + ",\"civilObjects\":[null]}"));
var lenient = DrawingFingerprint.FromJson("{" + schemaTag + ",\"entities\":[{\"handle\":\"1A\",\"type\":\"LINE\",\"hash\":\"x\",\"layer\":null}]}");
DrawingFingerprintDiff.Compare(lenient, current, 10);

// Schema guard.
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("{\"schema\":\"something-else\"}"));
ExpectCode("CIVIL3D.INVALID_INPUT", () => DrawingFingerprint.FromJson("not json"));

Console.WriteLine("Drawing compare diff harness passed.");
return 0;

static EntityPrint Entity(string handle, string type, string layer, string hash) =>
  new() { Handle = handle, Type = type, Layer = layer, Space = "Model", Hash = hash };

static CivilObjectPrint Civil(string kind, string name, string handle, JsonObject summary) =>
  new() { Kind = kind, Name = name, Handle = handle, Hash = DrawingFingerprint.Hash(summary.ToJsonString()), Summary = summary };

static JsonObject Pvi(string key, double elevation) =>
  new() { ["key"] = key, ["station"] = double.Parse(key), ["elevation"] = elevation };

static void ExpectCode(string expectedCode, Action action)
{
  try
  {
    action();
  }
  catch (JsonRpcDispatchException exception) when (exception.Code == expectedCode)
  {
    return;
  }

  throw new InvalidOperationException($"Expected JsonRpcDispatchException code {expectedCode}.");
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
