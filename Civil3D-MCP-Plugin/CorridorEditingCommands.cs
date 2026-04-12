using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilAssembly = Autodesk.Civil.DatabaseServices.Assembly;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_corridor_editing tools:
///   getCorridorTargetMappings / setCorridorTargetMappings /
///   addCorridorRegion / deleteCorridorRegion.
///
/// Civil 3D API notes:
///   Corridor.Baselines  → Baseline collection
///   Baseline.BaselineRegions → BaselineRegion collection
///   BaselineRegion.AppliedAssemblyId, StartStation, EndStation
///   Target mappings are on SubassemblyTargetInfo objects attached to each region.
/// </summary>
public static class CorridorEditingCommands
{
  // -------------------------------------------------------------------------
  // getCorridorTargetMappings
  // -------------------------------------------------------------------------

  public static Task<object?> GetCorridorTargetMappingsAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var regionIndex = PluginRuntime.GetOptionalInt(parameters, "regionIndex");
    var baselineIndex = PluginRuntime.GetOptionalInt(parameters, "baselineIndex") ?? 0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, corridorName, OpenMode.ForRead);
      var baseline = GetBaseline(corridor, baselineIndex);

      var results = new List<Dictionary<string, object?>>();

      var regions = baseline.BaselineRegions;
      for (var ri = 0; ri < regions.Count; ri++)
      {
        if (regionIndex.HasValue && ri != regionIndex.Value) continue;
        var region = regions[ri];
        var targets = ReadTargetMappings(region, transaction);
        results.Add(new Dictionary<string, object?>
        {
          ["regionIndex"] = ri,
          ["regionName"] = region.Name,
          ["startStation"] = region.StartStation,
          ["endStation"] = region.EndStation,
          ["targets"] = targets,
        });
      }

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridorName,
        ["baselineIndex"] = baselineIndex,
        ["regions"] = results,
      };
    });
  }

  // -------------------------------------------------------------------------
  // setCorridorTargetMappings
  // -------------------------------------------------------------------------

  public static Task<object?> SetCorridorTargetMappingsAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var regionIndex = PluginRuntime.GetOptionalInt(parameters, "regionIndex") ?? 0;
    var baselineIndex = PluginRuntime.GetOptionalInt(parameters, "baselineIndex") ?? 0;
    var targetsNode = PluginRuntime.GetParameter(parameters, "targets") as JsonArray
      ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "targets array is required.");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, corridorName, OpenMode.ForWrite);
      var baseline = GetBaseline(corridor, baselineIndex);

      if (regionIndex < 0 || regionIndex >= baseline.BaselineRegions.Count)
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
          $"Region index {regionIndex} is out of range. Corridor '{corridorName}' baseline {baselineIndex} has {baseline.BaselineRegions.Count} region(s).");

      var region = baseline.BaselineRegions[regionIndex];
      var appliedCount = 0;

      foreach (var targetNode in targetsNode)
      {
        if (targetNode is not JsonObject t) continue;
        var paramName = t["parameterName"]?.GetValue<string>();
        var targetType = t["targetType"]?.GetValue<string>();
        var targetName = t["targetName"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(paramName) || string.IsNullOrWhiteSpace(targetType) || string.IsNullOrWhiteSpace(targetName)) continue;

        // Find the Civil 3D object to use as target
        var targetId = ResolveTargetObjectId(civilDoc, transaction, targetType!, targetName!);
        if (targetId == null)
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
            $"Target object '{targetName}' of type '{targetType}' was not found.");

        // Apply target mapping via reflection on the region's target collection
        var applied = ApplyTargetMapping(region, paramName!, targetType!, targetId.Value);
        if (applied) appliedCount++;
      }

      // Rebuild corridor to apply changes
      corridor.Rebuild();

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridorName,
        ["baselineIndex"] = baselineIndex,
        ["regionIndex"] = regionIndex,
        ["targetsApplied"] = appliedCount,
        ["message"] = $"Applied {appliedCount} target mapping(s) and rebuilt corridor '{corridorName}'.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // addCorridorRegion
  // -------------------------------------------------------------------------

  public static Task<object?> AddCorridorRegionAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var baselineIndex = PluginRuntime.GetOptionalInt(parameters, "baselineIndex") ?? 0;
    var assemblyName = PluginRuntime.GetRequiredString(parameters, "assemblyName");
    var startStation = PluginRuntime.GetRequiredDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetRequiredDouble(parameters, "endStation");
    var frequency = PluginRuntime.GetOptionalDouble(parameters, "frequency") ?? 10.0;
    var frequencyAtCurves = PluginRuntime.GetOptionalDouble(parameters, "frequencyAtCurves") ?? frequency;
    var frequencyAtKneePoints = PluginRuntime.GetOptionalDouble(parameters, "frequencyAtKneePoints") ?? frequency;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, corridorName, OpenMode.ForWrite);
      var baseline = GetBaseline(corridor, baselineIndex);

      // Find the assembly
      ObjectId assemblyId = ObjectId.Null;
      foreach (ObjectId aid in civilDoc.AssemblyCollection)
      {
        var asm = CivilObjectUtils.GetRequiredObject<CivilAssembly>(transaction, aid, OpenMode.ForRead);
        if (string.Equals(asm.Name, assemblyName, StringComparison.OrdinalIgnoreCase))
        {
          assemblyId = aid;
          break;
        }
      }
      if (assemblyId.IsNull)
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Assembly '{assemblyName}' was not found.");

      // Add region via baseline API
      var newRegionIndex = baseline.BaselineRegions.Count;
      var added = CivilObjectUtils.InvokeMethod(baseline, "AddRegion",
        assemblyId, startStation, endStation, frequency, frequencyAtCurves, frequencyAtKneePoints);

      if (added == null)
      {
        // Try alternative API
        added = CivilObjectUtils.InvokeMethod(baseline, "AddRegion", assemblyId, startStation, endStation);
        if (added == null)
          throw new JsonRpcDispatchException("CIVIL3D.API_ERROR",
            "Failed to add corridor region. AddRegion method not found or failed.");
      }

      // Rebuild corridor
      corridor.Rebuild();

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridorName,
        ["baselineIndex"] = baselineIndex,
        ["regionIndex"] = newRegionIndex,
        ["assemblyName"] = assemblyName,
        ["startStation"] = startStation,
        ["endStation"] = endStation,
        ["frequency"] = frequency,
        ["message"] = $"Region added to corridor '{corridorName}' at stations {startStation:F3}–{endStation:F3}.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // deleteCorridorRegion
  // -------------------------------------------------------------------------

  public static Task<object?> DeleteCorridorRegionAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var baselineIndex = PluginRuntime.GetOptionalInt(parameters, "baselineIndex") ?? 0;
    var regionIndex = PluginRuntime.GetRequiredInt(parameters, "regionIndex");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, corridorName, OpenMode.ForWrite);
      var baseline = GetBaseline(corridor, baselineIndex);

      if (regionIndex < 0 || regionIndex >= baseline.BaselineRegions.Count)
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
          $"Region index {regionIndex} is out of range.");

      var region = baseline.BaselineRegions[regionIndex];
      var regionName = region.Name;
      var startStation = region.StartStation;
      var endStation = region.EndStation;

      // Delete via RemoveRegion or direct erase
      var removed = CivilObjectUtils.InvokeMethod(baseline, "RemoveRegion", region);
      if (removed == null)
        removed = CivilObjectUtils.InvokeMethod(baseline, "DeleteRegion", regionIndex);
      if (removed == null)
      {
        // Last resort: erase the region DBObject
        var regionId = CivilObjectUtils.GetPropertyValue<ObjectId>(region, "ObjectId");
        if (regionId != ObjectId.Null)
        {
          var regionWrite = CivilObjectUtils.GetRequiredObject<AcDbObject>(transaction, regionId, OpenMode.ForWrite);
          regionWrite.Erase(true);
        }
      }

      corridor.Rebuild();

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridorName,
        ["baselineIndex"] = baselineIndex,
        ["deletedRegionIndex"] = regionIndex,
        ["deletedRegionName"] = regionName,
        ["deletedStartStation"] = startStation,
        ["deletedEndStation"] = endStation,
        ["message"] = $"Region '{regionName}' deleted from corridor '{corridorName}'.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static Baseline GetBaseline(Corridor corridor, int index)
  {
    if (index < 0 || index >= corridor.Baselines.Count)
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        $"Baseline index {index} is out of range. Corridor '{corridor.Name}' has {corridor.Baselines.Count} baseline(s).");
    return corridor.Baselines[index];
  }

  private static List<Dictionary<string, object?>> ReadTargetMappings(
    BaselineRegion region, Transaction transaction)
  {
    var mappings = new List<Dictionary<string, object?>>();
    var targetInfo = CivilObjectUtils.GetPropertyValue<object>(region, "Targets")
      ?? CivilObjectUtils.InvokeMethod(region, "GetTargets");

    if (targetInfo is System.Collections.IEnumerable targets)
    {
      foreach (var t in targets)
      {
        var paramName = CivilObjectUtils.GetStringProperty(t, "ParameterName")
          ?? CivilObjectUtils.GetStringProperty(t, "Name");
        var targetType = CivilObjectUtils.GetStringProperty(t, "TargetType")
          ?? CivilObjectUtils.GetStringProperty(t, "Type");

        var targetIdProp = t?.GetType().GetProperty("TargetId") ?? t?.GetType().GetProperty("ObjectId");
        string? targetName = null;
        if (targetIdProp?.GetValue(t) is ObjectId tid && !tid.IsNull)
        {
          var targetObj = transaction.GetObject(tid, OpenMode.ForRead);
          targetName = CivilObjectUtils.GetName(targetObj);
        }

        mappings.Add(new Dictionary<string, object?>
        {
          ["parameterName"] = paramName,
          ["targetType"] = targetType,
          ["targetName"] = targetName,
        });
      }
    }

    return mappings;
  }

  private static ObjectId? ResolveTargetObjectId(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction, string targetType, string targetName)
  {
    switch (targetType.ToLowerInvariant())
    {
      case "surface":
        foreach (ObjectId id in civilDoc.GetSurfaceIds())
        {
          var s = CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, id, OpenMode.ForRead);
          if (string.Equals(s.Name, targetName, StringComparison.OrdinalIgnoreCase)) return id;
        }
        break;
      case "alignment":
        foreach (ObjectId id in civilDoc.GetAlignmentIds())
        {
          var a = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, id, OpenMode.ForRead);
          if (string.Equals(a.Name, targetName, StringComparison.OrdinalIgnoreCase)) return id;
        }
        break;
      case "profile":
        foreach (ObjectId aid in civilDoc.GetAlignmentIds())
        {
          var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, aid, OpenMode.ForRead);
          foreach (ObjectId pid in alignment.GetProfileIds())
          {
            var p = CivilObjectUtils.GetRequiredObject<Profile>(transaction, pid, OpenMode.ForRead);
            if (string.Equals(p.Name, targetName, StringComparison.OrdinalIgnoreCase)) return pid;
          }
        }
        break;
    }
    return null;
  }

  private static bool ApplyTargetMapping(BaselineRegion region, string paramName, string targetType, ObjectId targetId)
  {
    // Try via SetTarget or AssignTarget methods
    var result = CivilObjectUtils.InvokeMethod(region, "SetTarget", paramName, targetId);
    if (result != null) return true;

    result = CivilObjectUtils.InvokeMethod(region, "AssignTarget", paramName, targetId);
    if (result != null) return true;

    // Try via Targets collection
    var targets = CivilObjectUtils.GetPropertyValue<object>(region, "Targets");
    if (targets != null)
    {
      var setMethod = targets.GetType().GetMethods()
        .FirstOrDefault(m => m.Name == "SetTarget" || m.Name == "Add" || m.Name == "AssignTarget");
      if (setMethod != null)
      {
        try
        {
          setMethod.Invoke(targets, [paramName, targetId]);
          return true;
        }
        catch { }
      }
    }

    return false;
  }
}
