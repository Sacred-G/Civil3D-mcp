using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Hydraulic analysis commands for gravity pipe networks.
/// Implements Manning's equation, HGL backwater analysis, and structure property queries.
/// </summary>
public static class PipeHydraulicsCommands
{
  // ─── calculatePipeNetworkHgl ────────────────────────────────────────────────

  public static Task<object?> CalculatePipeNetworkHglAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var tailwaterElevation = PluginRuntime.GetOptionalDouble(parameters, "tailwaterElevation");
    var designFlow = PluginRuntime.GetOptionalDouble(parameters, "designFlow");
    var manningsN = PluginRuntime.GetOptionalDouble(parameters, "manningsN") ?? 0.013;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName);
      var pipes = GetPipeData(network, transaction);
      var structures = GetStructureData(network, transaction);

      if (pipes.Count == 0)
      {
        return new Dictionary<string, object?>
        {
          ["networkName"] = networkName,
          ["nodeCount"] = 0,
          ["nodes"] = Array.Empty<object>(),
          ["warning"] = "Network contains no pipes.",
        };
      }

      // Sort pipes upstream to downstream by invert elevation (lowest invert = downstream)
      var sortedPipes = pipes.OrderByDescending(p => p.InvertIn).ToList();
      var outletInvert = sortedPipes.Last().InvertOut;
      var hglAtOutlet = tailwaterElevation ?? outletInvert;

      // Build HGL table — simple backwater walk
      var nodes = new List<Dictionary<string, object?>>();
      var currentHgl = hglAtOutlet;

      for (int i = sortedPipes.Count - 1; i >= 0; i--)
      {
        var pipe = sortedPipes[i];
        double diameter = pipe.Diameter;
        double length = pipe.Length;
        double slope = Math.Abs(pipe.Slope) / 100.0; // convert % to decimal
        double area = Math.PI * diameter * diameter / 4.0;
        double wetPerim = Math.PI * diameter;
        double hydraulicRadius = diameter / 4.0;
        double qFull = (1.0 / manningsN) * area * Math.Pow(hydraulicRadius, 2.0 / 3.0) * Math.Pow(slope, 0.5);

        // Head loss in pipe (Manning's based)
        double headLoss = (manningsN * manningsN * length * (designFlow ?? qFull) * (designFlow ?? qFull))
                          / (Math.Pow(hydraulicRadius, 4.0 / 3.0) * area * area);
        headLoss = Math.Max(0, headLoss);

        var hglAtUpstream = currentHgl + headLoss;
        var isSurcharged = hglAtUpstream > pipe.InvertIn + diameter;

        nodes.Add(new Dictionary<string, object?>
        {
          ["pipeName"] = pipe.Name,
          ["invertIn"] = Math.Round(pipe.InvertIn, 3),
          ["invertOut"] = Math.Round(pipe.InvertOut, 3),
          ["diameter"] = Math.Round(diameter, 3),
          ["length"] = Math.Round(length, 2),
          ["slopePct"] = Math.Round(pipe.Slope, 4),
          ["qFullCfs"] = Math.Round(qFull, 3),
          ["hglDownstream"] = Math.Round(currentHgl, 3),
          ["hglUpstream"] = Math.Round(hglAtUpstream, 3),
          ["headLoss"] = Math.Round(headLoss, 3),
          ["surcharged"] = isSurcharged,
          ["status"] = isSurcharged ? "SURCHARGED" : "FREE_SURFACE",
        });

        currentHgl = hglAtUpstream;
      }

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["tailwaterElevation"] = Math.Round(hglAtOutlet, 3),
        ["manningsN"] = manningsN,
        ["designFlow"] = designFlow,
        ["pipeCount"] = pipes.Count,
        ["surchargedCount"] = nodes.Count(n => (bool)(n["surcharged"] ?? false)),
        ["nodes"] = nodes,
      };
    });
  }

  // ─── analyzePipeNetworkHydraulics ──────────────────────────────────────────

  public static Task<object?> AnalyzePipeNetworkHydraulicsAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var designFlow = PluginRuntime.GetOptionalDouble(parameters, "designFlow");
    var manningsN = PluginRuntime.GetOptionalDouble(parameters, "manningsN") ?? 0.013;
    var minCoverDepth = PluginRuntime.GetOptionalDouble(parameters, "minCoverDepth") ?? 2.0;
    var minVelocity = PluginRuntime.GetOptionalDouble(parameters, "minVelocity") ?? 2.0;
    var maxVelocity = PluginRuntime.GetOptionalDouble(parameters, "maxVelocity") ?? 10.0;
    var minSlope = PluginRuntime.GetOptionalDouble(parameters, "minSlope") ?? 0.5;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName);
      var pipes = GetPipeData(network, transaction);
      var structures = GetStructureData(network, transaction);

      var results = new List<Dictionary<string, object?>>();
      var violations = new List<Dictionary<string, object?>>();

      foreach (var pipe in pipes)
      {
        double diameter = pipe.Diameter;
        double slope = Math.Abs(pipe.Slope) / 100.0;
        double area = Math.PI * diameter * diameter / 4.0;
        double hydraulicRadius = diameter / 4.0;
        double qFull = (1.0 / manningsN) * area * Math.Pow(hydraulicRadius, 2.0 / 3.0) * Math.Pow(slope, 0.5);
        double vFull = qFull / area;
        double froudeNumber = area > 0 ? vFull / Math.Sqrt(9.81 * hydraulicRadius) : 0;

        double actualFlow = designFlow ?? qFull;
        double capacityRatio = qFull > 0 ? actualFlow / qFull : 0;
        double velocity = area > 0 ? actualFlow / area : 0;

        var pipeViolations = new List<string>();
        if (pipe.Slope < minSlope) pipeViolations.Add($"slope {pipe.Slope:F3}% < min {minSlope}%");
        if (velocity < minVelocity) pipeViolations.Add($"velocity {velocity:F2} < min {minVelocity} ft/s");
        if (velocity > maxVelocity) pipeViolations.Add($"velocity {velocity:F2} > max {maxVelocity} ft/s");
        if (capacityRatio > 1.0) pipeViolations.Add($"flow {actualFlow:F2} exceeds capacity {qFull:F2} CFS");

        var pipeResult = new Dictionary<string, object?>
        {
          ["pipeName"] = pipe.Name,
          ["diameter"] = Math.Round(diameter, 3),
          ["length"] = Math.Round(pipe.Length, 2),
          ["slopePct"] = Math.Round(pipe.Slope, 4),
          ["invertIn"] = Math.Round(pipe.InvertIn, 3),
          ["invertOut"] = Math.Round(pipe.InvertOut, 3),
          ["qFullCfs"] = Math.Round(qFull, 3),
          ["designFlowCfs"] = Math.Round(actualFlow, 3),
          ["capacityRatio"] = Math.Round(capacityRatio, 3),
          ["velocityFps"] = Math.Round(velocity, 3),
          ["froudeNumber"] = Math.Round(froudeNumber, 3),
          ["status"] = pipeViolations.Count > 0 ? "FAIL" : "OK",
          ["violations"] = pipeViolations,
        };

        results.Add(pipeResult);
        if (pipeViolations.Count > 0)
        {
          violations.Add(new Dictionary<string, object?>
          {
            ["pipeName"] = pipe.Name,
            ["violations"] = pipeViolations,
          });
        }
      }

      var structureSummary = structures.Select(s => new Dictionary<string, object?>
      {
        ["name"] = s.Name,
        ["rimElevation"] = Math.Round(s.RimElevation, 3),
        ["sumpElevation"] = Math.Round(s.SumpElevation, 3),
        ["depth"] = Math.Round(s.RimElevation - s.SumpElevation, 3),
        ["connectedPipes"] = s.ConnectedPipes,
      }).ToList();

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["manningsN"] = manningsN,
        ["pipeCount"] = pipes.Count,
        ["passCount"] = results.Count(r => (string)(r["status"] ?? "") == "OK"),
        ["failCount"] = violations.Count,
        ["pipes"] = results,
        ["violations"] = violations,
        ["structures"] = structureSummary,
      };
    });
  }

  // ─── getPipeStructureProperties ─────────────────────────────────────────────

  public static Task<object?> GetPipeStructurePropertiesAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var structureName = PluginRuntime.GetRequiredString(parameters, "structureName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var network = FindPipeNetworkByName(civilDoc, transaction, networkName);
      var structure = FindStructureByName(network, transaction, structureName);

      var rimElevation = GetAnyDouble(structure, "RimElevation", "SurfaceElevation", "Elevation") ?? 0.0;
      var sumpDepth = GetAnyDouble(structure, "SumpDepth") ?? 0.0;
      var sumpElevation = GetAnyDouble(structure, "SumpElevation") ?? (rimElevation - sumpDepth);
      var depth = rimElevation - sumpElevation;

      // Get connected pipes and their inverts
      var connectedPipeIds = GetChildObjectIds(structure, "GetConnectedPipeIds", "ConnectedPipeIds", "ConnectedPipes");
      var connectedPipes = new List<Dictionary<string, object?>>();
      foreach (var pipeId in connectedPipeIds)
      {
        if (pipeId == ObjectId.Null) continue;
        var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, pipeId, OpenMode.ForRead);
        var invertIn = GetAnyDouble(pipe, "StartInvertElevation", "InvertIn", "StartInnerElevation") ?? 0.0;
        var invertOut = GetAnyDouble(pipe, "EndInvertElevation", "InvertOut", "EndInnerElevation") ?? 0.0;
        connectedPipes.Add(new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(pipe) ?? pipeId.Handle.ToString(),
          ["invertIn"] = Math.Round(invertIn, 3),
          ["invertOut"] = Math.Round(invertOut, 3),
          ["diameter"] = Math.Round(GetAnyDouble(pipe, "InnerDiameterOrWidth", "InnerDiameter", "Diameter") ?? 0.0, 3),
        });
      }

      var location = GetPointProperty(structure, "Location", "Position", "CenterPoint");

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["structureName"] = structureName,
        ["type"] = GetAnyString(structure, "PartType", "StructureType", "PartDescription") ?? structure.GetType().Name,
        ["x"] = Math.Round(location?.X ?? 0.0, 3),
        ["y"] = Math.Round(location?.Y ?? 0.0, 3),
        ["rimElevation"] = Math.Round(rimElevation, 3),
        ["sumpElevation"] = Math.Round(sumpElevation, 3),
        ["sumpDepth"] = Math.Round(sumpDepth, 3),
        ["totalDepth"] = Math.Round(depth, 3),
        ["barrelCount"] = GetAnyDouble(structure, "BarrelCount", "NumberOfBarrels") ?? 1.0,
        ["connectedPipes"] = connectedPipes,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  private sealed record PipeInfo(string Name, double Diameter, double Length, double Slope, double InvertIn, double InvertOut);
  private sealed record StructureInfo(string Name, double RimElevation, double SumpElevation, List<string> ConnectedPipes);

  private static DBObject FindPipeNetworkByName(object civilDoc, Transaction transaction, string name)
  {
    var candidates = new[]
    {
      CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds"),
      GetNamedMember(civilDoc, "PipeNetworkCollection"),
      GetNamedMember(civilDoc, "NetworkCollection"),
      GetNamedMember(civilDoc, "PipeNetworks"),
      GetNamedMember(civilDoc, "Networks"),
    };

    foreach (var candidate in candidates)
    {
      foreach (var objectId in ToObjectIds(candidate))
      {
        var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(network), name, StringComparison.OrdinalIgnoreCase))
          return network;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{name}' was not found.");
  }

  private static DBObject FindStructureByName(DBObject network, Transaction transaction, string structureName)
  {
    foreach (var objectId in GetChildObjectIds(network, "GetStructureIds", "StructureIds", "Structures", "StructureCollection"))
    {
      var structure = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(structure), structureName, StringComparison.OrdinalIgnoreCase))
        return structure;
    }
    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Structure '{structureName}' was not found.");
  }

  private static List<PipeInfo> GetPipeData(DBObject network, Transaction transaction)
  {
    var results = new List<PipeInfo>();
    foreach (var objectId in GetChildObjectIds(network, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
    {
      var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead);
      var startPt = GetPointProperty(pipe, "StartPoint", "PointAtStart");
      var endPt = GetPointProperty(pipe, "EndPoint", "PointAtEnd");
      var invertIn = GetAnyDouble(pipe, "StartInvertElevation", "InvertIn", "StartInnerElevation") ?? startPt?.Z ?? 0.0;
      var invertOut = GetAnyDouble(pipe, "EndInvertElevation", "InvertOut", "EndInnerElevation") ?? endPt?.Z ?? 0.0;
      var length = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? 0.0;
      var slope = GetAnyDouble(pipe, "Slope", "FlowSlope") ?? (length > 0 ? (invertIn - invertOut) / length * 100 : 0);
      var diameter = GetAnyDouble(pipe, "InnerDiameterOrWidth", "InnerDiameter", "Diameter") ?? 1.0;
      results.Add(new PipeInfo(CivilObjectUtils.GetName(pipe) ?? objectId.Handle.ToString(), diameter, length, slope, invertIn, invertOut));
    }
    return results;
  }

  private static List<StructureInfo> GetStructureData(DBObject network, Transaction transaction)
  {
    var results = new List<StructureInfo>();
    foreach (var objectId in GetChildObjectIds(network, "GetStructureIds", "StructureIds", "Structures", "StructureCollection"))
    {
      var structure = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, OpenMode.ForRead);
      var rim = GetAnyDouble(structure, "RimElevation", "SurfaceElevation", "Elevation") ?? 0.0;
      var sumpDepth = GetAnyDouble(structure, "SumpDepth") ?? 0.0;
      var sump = GetAnyDouble(structure, "SumpElevation") ?? (rim - sumpDepth);
      var pipeIds = GetChildObjectIds(structure, "GetConnectedPipeIds", "ConnectedPipeIds", "ConnectedPipes")
        .Select(id => ResolveObjectName(transaction, id) ?? id.Handle.ToString()).ToList();
      results.Add(new StructureInfo(CivilObjectUtils.GetName(structure) ?? objectId.Handle.ToString(), rim, sump, pipeIds));
    }
    return results;
  }

  private static IEnumerable<ObjectId> GetChildObjectIds(DBObject parent, params string[] memberNames)
  {
    foreach (var name in memberNames)
    {
      var value = GetNamedMember(parent, name);
      if (value != null)
      {
        foreach (var id in ToObjectIds(value))
          yield return id;
        yield break;
      }
    }
  }

  private static IEnumerable<ObjectId> ToObjectIds(object? value)
  {
    if (value == null) yield break;

    if (value is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item is ObjectId id && id != ObjectId.Null)
          yield return id;
        else
        {
          var itemId = GetAnyObjectId(item, "ObjectId", "Id");
          if (itemId != ObjectId.Null) yield return itemId;
        }
      }
    }
  }

  private static object? GetNamedMember(object? obj, string name)
  {
    if (obj == null) return null;
    var type = obj.GetType();
    var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
    if (prop != null) return prop.GetValue(obj);
    var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
    return method?.Invoke(obj, null);
  }

  private static ObjectId GetAnyObjectId(object? obj, params string[] names)
  {
    if (obj == null) return ObjectId.Null;
    foreach (var name in names)
    {
      var val = GetNamedMember(obj, name);
      if (val is ObjectId id && id != ObjectId.Null) return id;
    }
    return ObjectId.Null;
  }

  private static double? GetAnyDouble(DBObject obj, params string[] names)
  {
    foreach (var name in names)
    {
      var val = GetNamedMember(obj, name);
      if (val != null)
      {
        try { return Convert.ToDouble(val); }
        catch { /* try next */ }
      }
    }
    return null;
  }

  private static string? GetAnyString(DBObject obj, params string[] names)
  {
    foreach (var name in names)
    {
      var val = GetNamedMember(obj, name);
      if (val != null) return val.ToString();
    }
    return null;
  }

  private static Point3d? GetPointProperty(DBObject obj, params string[] names)
  {
    foreach (var name in names)
    {
      var val = GetNamedMember(obj, name);
      if (val is Point3d pt) return pt;
    }
    return null;
  }

  private static string? ResolveObjectName(Transaction transaction, ObjectId id)
  {
    if (id == ObjectId.Null) return null;
    try
    {
      var obj = transaction.GetObject(id, OpenMode.ForRead, false, true);
      return CivilObjectUtils.GetName(obj);
    }
    catch { return null; }
  }
}
