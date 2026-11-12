using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Civil3DMcpPlugin;

/// <summary>
/// Host-independent drawing fingerprint and diff. Capture lives in
/// CompareCommands (it needs the AutoCAD/Civil 3D API); everything here is
/// plain .NET so the diff can be exercised without Civil 3D.
///
/// Entities are matched by handle (a DWG keeps handles across saves, so two
/// revisions of the same drawing line up). Civil 3D objects are matched by
/// kind + name, which survives copy/paste between drawings.
/// </summary>
public sealed class DrawingFingerprint
{
  public const string SchemaId = "civil3d-mcp/drawing-fingerprint@1";

  public string Schema { get; set; } = SchemaId;
  public string? CapturedAtUtc { get; set; }
  public string? SourcePath { get; set; }
  public string? SourceKind { get; set; }

  // "full" (default; null in older snapshots) or "fast": a drawing with more
  // than CompareCommands.FastEntityThreshold layout entities skips the
  // extents of curves, which their own start, end and length already pin
  // down. Both sides of a comparison
  // are captured at the same detail, or every such entity would differ.
  public string? EntityDetail { get; set; }

  public const string FullDetail = "full";
  public const string FastDetail = "fast";

  [JsonIgnore]
  public bool IsFast => string.Equals(EntityDetail, FastDetail, StringComparison.Ordinal);
  public List<EntityPrint> Entities { get; set; } = new();
  public List<CivilObjectPrint> CivilObjects { get; set; } = new();
  public List<string> Warnings { get; set; } = new();

  private static readonly JsonSerializerOptions SerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
  };

  public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);

  public static DrawingFingerprint FromJson(string json)
  {
    DrawingFingerprint? fingerprint;
    try
    {
      fingerprint = JsonSerializer.Deserialize<DrawingFingerprint>(json, SerializerOptions);
    }
    catch (JsonException exception)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Snapshot is not valid fingerprint JSON: {exception.Message}");
    }

    if (fingerprint == null || !string.Equals(fingerprint.Schema, SchemaId, StringComparison.Ordinal))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"Snapshot schema '{fingerprint?.Schema ?? "(missing)"}' is not supported; expected '{SchemaId}'.");
    }

    fingerprint.Entities ??= new();
    fingerprint.CivilObjects ??= new();
    fingerprint.Warnings ??= new();
    Validate(fingerprint);
    return fingerprint;
  }

  // A schema-tagged file can still carry null rows or null required fields
  // ("entities":[null], "summary":null); reject those as invalid input here
  // instead of letting the diff fail later with an internal error.
  private static void Validate(DrawingFingerprint fingerprint)
  {
    for (var index = 0; index < fingerprint.Entities.Count; index++)
    {
      var entity = fingerprint.Entities[index];
      if (entity == null || entity.Handle == null || entity.Type == null || entity.Hash == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"Snapshot entity {index} is missing its handle, type or hash.");
      }

      entity.Layer ??= string.Empty;
      entity.Space ??= string.Empty;
    }

    for (var index = 0; index < fingerprint.CivilObjects.Count; index++)
    {
      var item = fingerprint.CivilObjects[index];
      if (item == null || item.Kind == null || item.Name == null || item.Hash == null || item.Summary == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"Snapshot Civil 3D object {index} is missing its kind, name, hash or summary.");
      }
    }

    fingerprint.Warnings.RemoveAll(warning => warning == null);
  }

  /// <summary>First 16 hex characters of SHA-256 — ample for change detection.</summary>
  public static string Hash(string text)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(bytes, 0, 8);
  }

  public static string Num(double value) =>
    double.IsFinite(value) ? Math.Round(value, 6).ToString("0.######", CultureInfo.InvariantCulture) : "nan";
}

/// <summary>
/// Order-independent hash of a point set (surface vertices): the same points
/// in any order give the same value, and moving any one point (at 1e-6
/// drawing units) changes it. A wrapping sum of mixed per-point hashes, so it
/// costs one pass and no sort or allocation per point.
/// </summary>
public sealed class PointSetHash
{
  private ulong _sum;

  public long Count { get; private set; }

  public void Add(double x, double y, double z)
  {
    var h = Mix(Quantize(x));
    h = Mix(h ^ Quantize(y));
    h = Mix(h ^ Quantize(z));
    unchecked
    {
      _sum += h;
    }

    Count++;
  }

  public string Value => DrawingFingerprint.Hash($"{Count}:{_sum:x16}");

  private static ulong Quantize(double value) =>
    double.IsFinite(value) ? unchecked((ulong)(long)Math.Round(value * 1e6)) : 0x7FF8_0000_0000_0000UL;

  // SplitMix64 finalizer.
  private static ulong Mix(ulong value)
  {
    unchecked
    {
      value += 0x9E37_79B9_7F4A_7C15UL;
      value = (value ^ (value >> 30)) * 0xBF58_476D_1CE4_E5B9UL;
      value = (value ^ (value >> 27)) * 0x94D0_49BB_1331_11EBUL;
      return value ^ (value >> 31);
    }
  }
}

public sealed class EntityPrint
{
  public string Handle { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Layer { get; set; } = string.Empty;
  public string Space { get; set; } = string.Empty;
  public string Hash { get; set; } = string.Empty;
}

public sealed class CivilObjectPrint
{
  public string Kind { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string? Handle { get; set; }
  public string? Layer { get; set; }
  public string Hash { get; set; } = string.Empty;
  public JsonObject Summary { get; set; } = new();
}

public static class DrawingFingerprintDiff
{
  public static Dictionary<string, object?> Compare(DrawingFingerprint baseline, DrawingFingerprint current, int maxDetails)
  {
    maxDetails = Math.Clamp(maxDetails, 0, 5000);

    var baselineByHandle = IndexByHandle(baseline.Entities);
    var currentByHandle = IndexByHandle(current.Entities);

    var added = new List<EntityPrint>();
    var removed = new List<EntityPrint>();
    var modified = new List<(EntityPrint Before, EntityPrint After)>();
    var unchanged = 0;

    foreach (var (handle, after) in currentByHandle)
    {
      if (!baselineByHandle.TryGetValue(handle, out var before))
      {
        added.Add(after);
      }
      else if (!string.Equals(before.Type, after.Type, StringComparison.Ordinal))
      {
        // A recycled handle on a different object type is a replacement.
        removed.Add(before);
        added.Add(after);
      }
      else if (!string.Equals(before.Hash, after.Hash, StringComparison.Ordinal))
      {
        modified.Add((before, after));
      }
      else
      {
        unchanged++;
      }
    }

    foreach (var (handle, before) in baselineByHandle)
    {
      if (!currentByHandle.ContainsKey(handle))
      {
        removed.Add(before);
      }
    }

    var byType = Group(added, removed, modified, entity => entity.Type);
    var byLayer = Group(added, removed, modified, entity => entity.Layer);

    var layerChanges = modified
      .Where(pair => !string.Equals(pair.Before.Layer, pair.After.Layer, StringComparison.OrdinalIgnoreCase))
      .ToList();

    var totalDetails = added.Count + removed.Count + modified.Count;

    return new Dictionary<string, object?>
    {
      ["baseline"] = Describe(baseline),
      ["current"] = Describe(current),
      ["summary"] = new Dictionary<string, object?>
      {
        ["added"] = added.Count,
        ["removed"] = removed.Count,
        ["modified"] = modified.Count,
        ["unchanged"] = unchanged,
        ["layerChanges"] = layerChanges.Count,
        ["identical"] = totalDetails == 0 && CompareCivil(baseline, current, 0).Changed == 0,
      },
      ["byType"] = byType,
      ["byLayer"] = byLayer,
      ["details"] = new Dictionary<string, object?>
      {
        ["maxDetails"] = maxDetails,
        ["truncated"] = added.Count > maxDetails || removed.Count > maxDetails || modified.Count > maxDetails,
        ["added"] = added.OrderBy(entity => entity.Type).ThenBy(entity => entity.Handle).Take(maxDetails).Select(EntityRow).ToList(),
        ["removed"] = removed.OrderBy(entity => entity.Type).ThenBy(entity => entity.Handle).Take(maxDetails).Select(EntityRow).ToList(),
        ["modified"] = modified
          .OrderBy(pair => pair.After.Type).ThenBy(pair => pair.After.Handle)
          .Take(maxDetails)
          .Select(pair =>
          {
            var row = EntityRow(pair.After);
            if (!string.Equals(pair.Before.Layer, pair.After.Layer, StringComparison.OrdinalIgnoreCase))
            {
              row["previousLayer"] = pair.Before.Layer;
            }

            if (!string.Equals(pair.Before.Space, pair.After.Space, StringComparison.OrdinalIgnoreCase))
            {
              row["previousSpace"] = pair.Before.Space;
            }

            return row;
          })
          .ToList(),
      },
      ["civil"] = CompareCivil(baseline, current, maxDetails).Report,
      ["notes"] = new List<string>
      {
        "Entities are matched by handle: 'added' exists only in the current drawing, 'removed' only in the baseline. Handles line up between revisions of the same DWG; comparing unrelated drawings reports everything as added/removed.",
        "Civil 3D objects are matched by kind and name; their summaries (lengths, PVIs, surface statistics, pipe inverts) are compared field by field.",
      },
    };
  }

  private static Dictionary<string, EntityPrint> IndexByHandle(IEnumerable<EntityPrint> entities)
  {
    var index = new Dictionary<string, EntityPrint>(StringComparer.OrdinalIgnoreCase);
    foreach (var entity in entities)
    {
      if (!string.IsNullOrWhiteSpace(entity.Handle))
      {
        index[entity.Handle] = entity;
      }
    }

    return index;
  }

  private static Dictionary<string, object?> Describe(DrawingFingerprint fingerprint) => new()
  {
    ["sourcePath"] = fingerprint.SourcePath,
    ["sourceKind"] = fingerprint.SourceKind,
    ["entityDetail"] = fingerprint.EntityDetail ?? DrawingFingerprint.FullDetail,
    ["capturedAtUtc"] = fingerprint.CapturedAtUtc,
    ["entityCount"] = fingerprint.Entities.Count,
    ["civilObjectCount"] = fingerprint.CivilObjects.Count,
    ["warnings"] = fingerprint.Warnings,
  };

  private static Dictionary<string, object?> EntityRow(EntityPrint entity) => new()
  {
    ["handle"] = entity.Handle,
    ["type"] = entity.Type,
    ["layer"] = entity.Layer,
    ["space"] = entity.Space,
  };

  private static List<Dictionary<string, object?>> Group(
    List<EntityPrint> added,
    List<EntityPrint> removed,
    List<(EntityPrint Before, EntityPrint After)> modified,
    Func<EntityPrint, string> key)
  {
    var groups = new SortedDictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
    void Bump(string name, int slot)
    {
      if (!groups.TryGetValue(name, out var counts))
      {
        counts = new int[3];
        groups[name] = counts;
      }

      counts[slot]++;
    }

    foreach (var entity in added) Bump(key(entity), 0);
    foreach (var entity in removed) Bump(key(entity), 1);
    foreach (var pair in modified) Bump(key(pair.After), 2);

    return groups
      .Select(group => new Dictionary<string, object?>
      {
        ["name"] = group.Key,
        ["added"] = group.Value[0],
        ["removed"] = group.Value[1],
        ["modified"] = group.Value[2],
      })
      .OrderByDescending(row => (int)row["added"]! + (int)row["removed"]! + (int)row["modified"]!)
      .ThenBy(row => row["name"] as string, StringComparer.OrdinalIgnoreCase)
      .ToList();
  }

  private static (Dictionary<string, object?> Report, int Changed) CompareCivil(DrawingFingerprint baseline, DrawingFingerprint current, int maxDetails)
  {
    static string Key(CivilObjectPrint item) => $"{item.Kind}\u0001{item.Name}";

    var before = new Dictionary<string, CivilObjectPrint>(StringComparer.OrdinalIgnoreCase);
    foreach (var item in baseline.CivilObjects) before[Key(item)] = item;
    var after = new Dictionary<string, CivilObjectPrint>(StringComparer.OrdinalIgnoreCase);
    foreach (var item in current.CivilObjects) after[Key(item)] = item;

    var added = after.Where(pair => !before.ContainsKey(pair.Key)).Select(pair => pair.Value).ToList();
    var removed = before.Where(pair => !after.ContainsKey(pair.Key)).Select(pair => pair.Value).ToList();
    var modified = new List<Dictionary<string, object?>>();
    var unchanged = 0;

    foreach (var (key, now) in after.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
    {
      if (!before.TryGetValue(key, out var then))
      {
        continue;
      }

      if (string.Equals(then.Hash, now.Hash, StringComparison.Ordinal))
      {
        unchanged++;
        continue;
      }

      modified.Add(new Dictionary<string, object?>
      {
        ["kind"] = now.Kind,
        ["name"] = now.Name,
        ["handle"] = now.Handle,
        ["changes"] = DiffSummaries(then.Summary, now.Summary),
      });
    }

    var byKind = baseline.CivilObjects.Select(item => item.Kind)
      .Concat(current.CivilObjects.Select(item => item.Kind))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .OrderBy(kind => kind, StringComparer.OrdinalIgnoreCase)
      .Select(kind => new Dictionary<string, object?>
      {
        ["kind"] = kind,
        ["baseline"] = baseline.CivilObjects.Count(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase)),
        ["current"] = current.CivilObjects.Count(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase)),
        ["added"] = added.Count(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase)),
        ["removed"] = removed.Count(item => string.Equals(item.Kind, kind, StringComparison.OrdinalIgnoreCase)),
        ["modified"] = modified.Count(item => string.Equals(item["kind"] as string, kind, StringComparison.OrdinalIgnoreCase)),
      })
      .ToList();

    var rowLimit = Math.Max(maxDetails, 0);
    var report = new Dictionary<string, object?>
    {
      ["truncated"] = added.Count > rowLimit || removed.Count > rowLimit || modified.Count > rowLimit,
      ["byKind"] = byKind,
      ["added"] = added.Take(rowLimit).Select(item => new Dictionary<string, object?> { ["kind"] = item.Kind, ["name"] = item.Name, ["handle"] = item.Handle, ["summary"] = item.Summary.DeepClone() }).ToList(),
      ["removed"] = removed.Take(rowLimit).Select(item => new Dictionary<string, object?> { ["kind"] = item.Kind, ["name"] = item.Name, ["handle"] = item.Handle }).ToList(),
      ["modified"] = modified.Take(rowLimit).ToList(),
      ["unchanged"] = unchanged,
    };

    return (report, added.Count + removed.Count + modified.Count);
  }

  /// <summary>
  /// Field-level diff of two summaries. Scalars report before/after; arrays of
  /// keyed rows (each row has a "key") report added/removed/changed keys.
  /// </summary>
  internal static Dictionary<string, object?> DiffSummaries(JsonObject before, JsonObject after)
  {
    var changes = new Dictionary<string, object?>();
    var keys = before.Select(pair => pair.Key).Union(after.Select(pair => pair.Key)).OrderBy(key => key, StringComparer.Ordinal);
    foreach (var key in keys)
    {
      before.TryGetPropertyValue(key, out var then);
      after.TryGetPropertyValue(key, out var now);
      var thenText = then?.ToJsonString() ?? "null";
      var nowText = now?.ToJsonString() ?? "null";
      if (string.Equals(thenText, nowText, StringComparison.Ordinal))
      {
        continue;
      }

      if (then is JsonArray thenRows && now is JsonArray nowRows && IsKeyed(thenRows) && IsKeyed(nowRows))
      {
        var thenByKey = KeyRows(thenRows);
        var nowByKey = KeyRows(nowRows);
        var addedKeys = nowByKey.Keys.Where(rowKey => !thenByKey.ContainsKey(rowKey)).OrderBy(rowKey => rowKey, StringComparer.Ordinal).ToList();
        var removedKeys = thenByKey.Keys.Where(rowKey => !nowByKey.ContainsKey(rowKey)).OrderBy(rowKey => rowKey, StringComparer.Ordinal).ToList();
        var changedRows = nowByKey
          .Where(pair => thenByKey.TryGetValue(pair.Key, out var old) && !string.Equals(old.ToJsonString(), pair.Value.ToJsonString(), StringComparison.Ordinal))
          .OrderBy(pair => pair.Key, StringComparer.Ordinal)
          .Take(50)
          .Select(pair => new Dictionary<string, object?>
          {
            ["key"] = pair.Key,
            ["before"] = thenByKey[pair.Key].DeepClone(),
            ["after"] = pair.Value.DeepClone(),
          })
          .ToList();

        changes[key] = new Dictionary<string, object?>
        {
          ["addedCount"] = addedKeys.Count,
          ["removedCount"] = removedKeys.Count,
          ["changedCount"] = nowByKey.Count(pair => thenByKey.TryGetValue(pair.Key, out var old) && !string.Equals(old.ToJsonString(), pair.Value.ToJsonString(), StringComparison.Ordinal)),
          ["added"] = addedKeys.Take(50).ToList(),
          ["removed"] = removedKeys.Take(50).ToList(),
          ["changed"] = changedRows,
        };
        continue;
      }

      changes[key] = new Dictionary<string, object?>
      {
        ["before"] = then?.DeepClone(),
        ["after"] = now?.DeepClone(),
      };
    }

    return changes;
  }

  private static bool IsKeyed(JsonArray rows) =>
    rows.All(row => row is JsonObject obj && obj.TryGetPropertyValue("key", out var key) && key != null);

  private static Dictionary<string, JsonObject> KeyRows(JsonArray rows)
  {
    var result = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
    foreach (var row in rows.OfType<JsonObject>())
    {
      var key = row["key"]!.ToString();
      result[key] = row;
    }

    return result;
  }
}
