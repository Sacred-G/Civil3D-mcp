using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using TinVolumeSurface = Autodesk.Civil.DatabaseServices.TinVolumeSurface;

namespace Civil3DMcpPlugin;

/// <summary>
/// Pay item export and construction cost estimation commands.
/// Pulls quantities from Civil 3D (earthwork, corridor layers, pipes, structures)
/// and combines them with user-supplied unit prices to generate cost estimates.
/// </summary>
public static class CostEstimationCommands
{
  // ─── exportPayItems ──────────────────────────────────────────────────────────

  public static Task<object?> ExportPayItemsAsync(JsonObject? parameters)
  {
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var corridorName = PluginRuntime.GetOptionalString(parameters, "corridorName");
    var baseSurface = PluginRuntime.GetOptionalString(parameters, "baseSurface");
    var designSurface = PluginRuntime.GetOptionalString(parameters, "designSurface");
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var includeEarthwork = PluginRuntime.GetOptionalBool(parameters, "includeEarthwork") ?? true;
    var includeCorridorMaterials = PluginRuntime.GetOptionalBool(parameters, "includeCorridorMaterials") ?? true;
    var includePipeLengths = PluginRuntime.GetOptionalBool(parameters, "includePipeLengths") ?? true;
    var includeStructureCounts = PluginRuntime.GetOptionalBool(parameters, "includeStructureCounts") ?? true;
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;

    // Parse pay items from parameters
    var payItemsNode = parameters?["payItems"] as JsonArray;
    var payItems = ParsePayItems(payItemsNode);

    var computesEarthwork = includeEarthwork && !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface);
    return ReadQuantitiesAsync(computesEarthwork, (doc, civilDoc, database, transaction) =>
    {
      var quantityLines = new List<Dictionary<string, object?>>();

      // Earthwork volumes
      if (includeEarthwork && !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface))
      {
        var (cutCy, fillCy) = GetEarthworkVolumes(civilDoc, database, transaction, baseSurface!, designSurface!);
        quantityLines.Add(MakeLine("203.01", "Unclassified Excavation (Cut)", "CY", Math.Round(cutCy, 1), payItems));
        quantityLines.Add(MakeLine("203.02", "Embankment (Fill)", "CY", Math.Round(fillCy, 1), payItems));
      }

      // Civil 3D 2026 exposes QTO totals through SampleLineGroup material lists,
      // not through a Corridor.GetMaterialVolumes member.
      if (includeCorridorMaterials && !string.IsNullOrWhiteSpace(corridorName))
      {
        throw CorridorMaterialCapabilityError(corridorName!);
      }

      // Pipe network lengths
      if (includePipeLengths)
      {
        var pipeNetworks = GetAllPipeNetworkLengths(civilDoc, transaction);
        foreach (var (netName, lengthLf) in pipeNetworks)
        {
          quantityLines.Add(MakeLine(string.Empty, $"Pipe Network '{netName}' — Total Length", "LF", Math.Round(lengthLf, 1), payItems));
        }
      }

      // Structure counts
      if (includeStructureCounts)
      {
        var structureCounts = GetStructureCounts(civilDoc, transaction);
        foreach (var (typeName, count) in structureCounts)
        {
          quantityLines.Add(MakeLine(string.Empty, typeName, "EA", count, payItems));
        }
      }

      // Write CSV
      var csvContent = BuildCsv(quantityLines, payItems.Count > 0);
      var canonicalOutputPath = FileBoundary.WriteAllTextAtomic(
        outputPath, csvContent, Encoding.UTF8, overwrite, ".csv");

      return new Dictionary<string, object?>
      {
        ["outputPath"] = canonicalOutputPath,
        ["lineItemCount"] = quantityLines.Count,
        ["hasPricing"] = payItems.Count > 0,
        ["quantityLines"] = quantityLines,
        ["exported"] = true,
      };
    });
  }

  // ─── calculateMaterialCostEstimate ───────────────────────────────────────────

  public static Task<object?> CalculateMaterialCostEstimateAsync(JsonObject? parameters)
  {
    var corridorName = PluginRuntime.GetOptionalString(parameters, "corridorName");
    var baseSurface = PluginRuntime.GetOptionalString(parameters, "baseSurface");
    var designSurface = PluginRuntime.GetOptionalString(parameters, "designSurface");
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var contingencyPct = PluginRuntime.GetOptionalDouble(parameters, "contingencyPercent") ?? 0.0;
    var mobilizationPct = PluginRuntime.GetOptionalDouble(parameters, "mobilizationPercent") ?? 5.0;
    var outputPath = PluginRuntime.GetOptionalString(parameters, "outputPath");
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;

    var payItemsNode = parameters?["payItems"] as JsonArray;
    var payItems = ParsePayItems(payItemsNode);

    if (payItems.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "At least one pay item with a unit price is required to generate a cost estimate.");
    }

    var computesEarthwork = !string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface);
    return ReadQuantitiesAsync(computesEarthwork, (doc, civilDoc, database, transaction) =>
    {
      var lineItems = new List<Dictionary<string, object?>>();
      double subtotal = 0;

      // Earthwork
      if (!string.IsNullOrWhiteSpace(baseSurface) && !string.IsNullOrWhiteSpace(designSurface))
      {
        var (cutCy, fillCy) = GetEarthworkVolumes(civilDoc, database, transaction, baseSurface!, designSurface!);
        AddCostLine(lineItems, payItems, "203.01", "Unclassified Excavation (Cut)", "CY", Math.Round(cutCy, 1), ref subtotal);
        AddCostLine(lineItems, payItems, "203.02", "Embankment (Fill)", "CY", Math.Round(fillCy, 1), ref subtotal);
      }

      // Corridor materials
      if (!string.IsNullOrWhiteSpace(corridorName))
      {
        throw CorridorMaterialCapabilityError(corridorName!);
      }

      // Pipe lengths
      var pipeNetworks = GetAllPipeNetworkLengths(civilDoc, transaction);
      foreach (var (netName, lengthLf) in pipeNetworks)
      {
        AddCostLine(lineItems, payItems, string.Empty, $"Pipe Network '{netName}'", "LF", Math.Round(lengthLf, 1), ref subtotal);
      }

      // Structure counts
      var structs = GetStructureCounts(civilDoc, transaction);
      foreach (var (typeName, count) in structs)
      {
        AddCostLine(lineItems, payItems, string.Empty, typeName, "EA", count, ref subtotal);
      }

      double mobilizationCost = subtotal * mobilizationPct / 100.0;
      double contingencyCost = (subtotal + mobilizationCost) * contingencyPct / 100.0;
      double total = subtotal + mobilizationCost + contingencyCost;

      var result = new Dictionary<string, object?>
      {
        ["lineItems"] = lineItems,
        ["subtotal"] = Math.Round(subtotal, 2),
        ["mobilizationPercent"] = mobilizationPct,
        ["mobilizationCost"] = Math.Round(mobilizationCost, 2),
        ["contingencyPercent"] = contingencyPct,
        ["contingencyCost"] = Math.Round(contingencyCost, 2),
        ["totalEstimatedCost"] = Math.Round(total, 2),
        ["totalFormatted"] = $"${total:N2}",
        ["note"] = "Estimate is preliminary. Field verification of quantities required for bid documents.",
      };

      if (!string.IsNullOrWhiteSpace(outputPath))
      {
        var csv = BuildCostCsv(lineItems, subtotal, mobilizationCost, contingencyCost, total);
        result["outputPath"] = FileBoundary.WriteAllTextAtomic(
          outputPath!, csv, Encoding.UTF8, overwrite, ".csv");
        result["exported"] = true;
      }

      return (object?)result;
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────────

  // Earthwork volumes create a scratch TinVolumeSurface, which must disappear
  // with the aborted transaction, so only then does the read need the
  // C3DMCPHOP command (and a quiescent editor). Pipe and structure quantities
  // alone are a plain read on the Idle route.
  private static Task<object?> ReadQuantitiesAsync(
    bool computesEarthwork,
    Func<Autodesk.AutoCAD.ApplicationServices.Document, Autodesk.Civil.ApplicationServices.CivilDocument, Database, Transaction, object?> action)
  {
    return computesEarthwork
      ? CivilExecution.ReadInCommandAsync(action)
      : CivilExecution.ReadAsync(action);
  }

  private sealed record PayItem(string Code, string Description, string Unit, double UnitPrice);

  private static List<PayItem> ParsePayItems(JsonArray? node)
  {
    var items = new List<PayItem>();
    if (node == null) return items;
    foreach (var item in node)
    {
      if (item is not JsonObject obj) continue;
      var code = obj["code"]?.GetValue<string>() ?? string.Empty;
      var desc = obj["description"]?.GetValue<string>() ?? string.Empty;
      var unit = obj["unit"]?.GetValue<string>() ?? string.Empty;
      double price = 0;
      try { price = obj["unitPrice"]?.GetValue<double>() ?? 0; } catch { }
      if (!string.IsNullOrWhiteSpace(desc))
        items.Add(new PayItem(code, desc, unit, price));
    }
    return items;
  }

  private static Dictionary<string, object?> MakeLine(string code, string description, string unit, double quantity, List<PayItem> payItems)
  {
    var match = payItems.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase)
      || string.Equals(p.Description, description, StringComparison.OrdinalIgnoreCase));
    return new Dictionary<string, object?>
    {
      ["payItemCode"] = !string.IsNullOrEmpty(code) ? code : (match?.Code ?? string.Empty),
      ["description"] = description,
      ["unit"] = unit,
      ["quantity"] = quantity,
      ["unitPrice"] = match?.UnitPrice,
      ["totalCost"] = match != null ? Math.Round(match.UnitPrice * quantity, 2) : (object?)null,
    };
  }

  private static void AddCostLine(List<Dictionary<string, object?>> lineItems, List<PayItem> payItems,
    string code, string description, string unit, double quantity, ref double subtotal)
  {
    var match = payItems.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase)
      || description.IndexOf(p.Description, StringComparison.OrdinalIgnoreCase) >= 0
      || p.Description.IndexOf(description, StringComparison.OrdinalIgnoreCase) >= 0);

    var lineTotal = match != null ? match.UnitPrice * quantity : 0;
    subtotal += lineTotal;

    lineItems.Add(new Dictionary<string, object?>
    {
      ["payItemCode"] = !string.IsNullOrEmpty(code) ? code : (match?.Code ?? string.Empty),
      ["description"] = description,
      ["unit"] = unit,
      ["quantity"] = quantity,
      ["unitPrice"] = match?.UnitPrice ?? 0,
      ["lineTotal"] = Math.Round(lineTotal, 2),
    });
  }

  private static (double CutCy, double FillCy) GetEarthworkVolumes(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Database database, Transaction transaction, string baseSurface, string designSurface)
  {
    var baseSurf = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurface, OpenMode.ForRead);
    var desSurf = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, designSurface, OpenMode.ForRead);
    var temporaryName = $"MCP_TMP_COST_VOLUME_{Guid.NewGuid():N}";
    var volumeSurfaceId = TinVolumeSurface.Create(temporaryName, baseSurf.ObjectId, desSurf.ObjectId);
    var volumeSurface = CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, volumeSurfaceId, OpenMode.ForRead);
    var properties = volumeSurface.GetVolumeProperties();
    var cubicUnitsToCubicYards = database.Insunits == UnitsValue.Meters
      ? 1.3079506193
      : database.Insunits == UnitsValue.Feet || database.Insunits.ToString().Contains("Survey", StringComparison.OrdinalIgnoreCase)
        ? 1.0 / 27.0
        : throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Cannot convert drawing units '{database.Insunits}' to cubic yards for cost estimation.");

    return (
      properties.UnadjustedCutVolume * cubicUnitsToCubicYards,
      properties.UnadjustedFillVolume * cubicUnitsToCubicYards);
  }

  private static JsonRpcDispatchException CorridorMaterialCapabilityError(string corridorName)
  {
    return new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      $"Corridor material quantities for '{corridorName}' cannot be read from Corridor in the Civil 3D 2026 .NET API. " +
      "Civil 3D exposes material totals through SampleLineGroup.GetTotalVolumeResultDataForMaterialList(Guid); " +
      "supply a sample-line-group/material-list mapping in a dedicated QTO workflow instead.");
  }

  private static List<(string NetworkName, double LengthLf)> GetAllPipeNetworkLengths(object civilDoc, Transaction transaction)
  {
    var results = new List<(string, double)>();
    try
    {
      var candidates = new object?[]
      {
        CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds"),
      };

      foreach (var candidate in candidates)
      {
        if (candidate is not IEnumerable enumerable) continue;
        foreach (var item in enumerable)
        {
          if (item is not ObjectId id || id == ObjectId.Null) continue;
          var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, id, OpenMode.ForRead);
          var networkName = CivilObjectUtils.GetName(network) ?? "Unknown";
          double totalLength = 0;

          // Sum all pipe lengths
          var pipeIds = CivilObjectUtils.InvokeMethod(network, "GetPipeIds") as IEnumerable;
          if (pipeIds != null)
          {
            foreach (var pItem in pipeIds)
            {
              if (pItem is not ObjectId pId || pId == ObjectId.Null) continue;
              var pipe = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, pId, OpenMode.ForRead);
              var length = CivilObjectUtils.GetPropertyValue<double?>(pipe, "Length3D")
                        ?? CivilObjectUtils.GetPropertyValue<double?>(pipe, "Length2D")
                        ?? CivilObjectUtils.GetPropertyValue<double?>(pipe, "Length") ?? 0;
              totalLength += length;
            }
          }

          if (totalLength > 0) results.Add((networkName, totalLength));
        }
      }
    }
    catch { /* skip */ }
    return results;
  }

  private static List<(string TypeName, double Count)> GetStructureCounts(object civilDoc, Transaction transaction)
  {
    var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    try
    {
      var networkIds = CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds") as IEnumerable;
      if (networkIds == null) return [];

      foreach (var netItem in networkIds)
      {
        if (netItem is not ObjectId netId || netId == ObjectId.Null) continue;
        var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, netId, OpenMode.ForRead);

        var structIds = CivilObjectUtils.InvokeMethod(network, "GetStructureIds") as IEnumerable;
        if (structIds == null) continue;

        foreach (var sItem in structIds)
        {
          if (sItem is not ObjectId sId || sId == ObjectId.Null) continue;
          var structure = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, sId, OpenMode.ForRead);
          var typeName = CivilObjectUtils.GetStringProperty(structure, "PartType")
                      ?? CivilObjectUtils.GetStringProperty(structure, "PartDescription")
                      ?? "Structure";
          counts[typeName] = counts.GetValueOrDefault(typeName) + 1;
        }
      }
    }
    catch { /* skip */ }

    return counts.Select(kv => ($"{kv.Key} (Structure)", (double)kv.Value)).ToList();
  }

  private static string BuildCsv(List<Dictionary<string, object?>> lines, bool hasPricing)
  {
    var sb = new StringBuilder();
    sb.AppendLine(hasPricing
      ? "Pay Item Code,Description,Unit,Quantity,Unit Price,Total Cost"
      : "Pay Item Code,Description,Unit,Quantity");

    foreach (var line in lines)
    {
      if (hasPricing)
        sb.AppendLine($"\"{line["payItemCode"]}\",\"{line["description"]}\",\"{line["unit"]}\",{line["quantity"]},{line["unitPrice"] ?? ""},{ line["totalCost"] ?? ""}");
      else
        sb.AppendLine($"\"{line["payItemCode"]}\",\"{line["description"]}\",\"{line["unit"]}\",{line["quantity"]}");
    }
    return sb.ToString();
  }

  private static string BuildCostCsv(List<Dictionary<string, object?>> lineItems, double subtotal, double mob, double contingency, double total)
  {
    var sb = new StringBuilder();
    sb.AppendLine("Pay Item Code,Description,Unit,Quantity,Unit Price,Line Total");
    foreach (var item in lineItems)
      sb.AppendLine($"\"{item["payItemCode"]}\",\"{item["description"]}\",\"{item["unit"]}\",{item["quantity"]},{item["unitPrice"]},{item["lineTotal"]}");
    sb.AppendLine();
    sb.AppendLine($",,,,Subtotal,{subtotal:F2}");
    sb.AppendLine($",,,,Mobilization,{mob:F2}");
    sb.AppendLine($",,,,Contingency,{contingency:F2}");
    sb.AppendLine($",,,,TOTAL,{total:F2}");
    return sb.ToString();
  }
}
