using System.Collections;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

/// <summary>
/// Quantity takeoff commands — aggregate Civil 3D object data for cost estimation.
/// Uses typed Civil 3D APIs wherever the 2026 API exposes the required data.
/// </summary>
public static class QuantityCommands
{
  // -------------------------------------------------------------------------
  // qtyCorridorVolumes
  // -------------------------------------------------------------------------

  public static Task<object?> QtyCorridorVolumesAsync(JsonObject? parameters)
  {
    _ = PluginRuntime.GetRequiredString(parameters, "name");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "Corridor material quantities are exposed through SampleLineGroup QTO material lists, not Corridor baseline properties. A sample-line group and material-list identifier are required; no zero quantities were returned.");
  }

  // -------------------------------------------------------------------------
  // qtySurfaceVolume
  // -------------------------------------------------------------------------

  public static Task<object?> QtySurfaceVolumeAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var corridorName = PluginRuntime.GetOptionalString(parameters, "corridorName");

    return CivilExecution.ReadInCommandAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var compSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);

      var temporaryName = $"MCP_TMP_VOLUME_{Guid.NewGuid():N}";
      var volumeSurfaceId = TinVolumeSurface.Create(temporaryName, baseSurface.ObjectId, compSurface.ObjectId);
      var volumeSurface = CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, volumeSurfaceId, OpenMode.ForRead);
      var volumeProperties = volumeSurface.GetVolumeProperties();

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["comparisonSurface"] = comparisonSurfaceName,
        ["corridorName"] = corridorName,
        ["cutVolume"] = volumeProperties.UnadjustedCutVolume,
        ["fillVolume"] = volumeProperties.UnadjustedFillVolume,
        ["netVolume"] = volumeProperties.UnadjustedNetVolume,
        ["net2dArea"] = null,
        ["units"] = CivilObjectUtils.VolumeUnits(database),
        ["note"] = "Volumes are exact Civil 3D TinVolumeSurface results. The managed volume-properties API does not expose a net 2D area.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyPipeNetworkLengths
  // -------------------------------------------------------------------------

  public static Task<object?> QtyPipeNetworkLengthsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var groupBySize = PluginRuntime.GetOptionalBool(parameters, "groupBySize") ?? false;
    var groupByMaterial = PluginRuntime.GetOptionalBool(parameters, "groupByMaterial") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networkObj = FindPipeNetworkByName(civilDoc, transaction, name)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{name}' was not found.");

      var totalLength = 0.0;
      var pipeCount = 0;
      var breakdown = new Dictionary<string, (double Length, int Count)>(StringComparer.OrdinalIgnoreCase);

      foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
      {
        var pipe = CivilObjectUtils.GetRequiredObject<Pipe>(transaction, pipeId, OpenMode.ForRead);
        var length = pipe.Length3D;
        var diameter = pipe.InnerDiameterOrWidth;
        var material = pipe.Material;

        totalLength += length;
        pipeCount++;

        // Build breakdown key
        var key = "All";
        if (groupBySize && groupByMaterial)
          key = $"{material} Ø{diameter:F0}mm";
        else if (groupBySize)
          key = $"Ø{diameter:F0}mm";
        else if (groupByMaterial)
          key = material;

        if (breakdown.TryGetValue(key, out var existing))
          breakdown[key] = (existing.Length + length, existing.Count + 1);
        else
          breakdown[key] = (length, 1);
      }

      var breakdownList = breakdown
        .Select(kvp => new Dictionary<string, object?>
        {
          ["key"] = kvp.Key,
          ["length"] = kvp.Value.Length,
          ["count"] = kvp.Value.Count,
        })
        .OrderByDescending(d => d["length"])
        .ToList();

      return new Dictionary<string, object?>
      {
        ["networkName"] = name,
        ["totalLength"] = totalLength,
        ["pipeCount"] = pipeCount,
        ["breakdown"] = breakdownList,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyPressureNetworkLengths
  // -------------------------------------------------------------------------

  public static Task<object?> QtyPressureNetworkLengthsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var groupBySize = PluginRuntime.GetOptionalBool(parameters, "groupBySize") ?? false;
    var groupByMaterial = PluginRuntime.GetOptionalBool(parameters, "groupByMaterial") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var networkObj = FindPressureNetworkByName(civilDoc, transaction, name)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pressure network '{name}' was not found.");

      var totalLength = 0.0;
      var pipeCount = 0;
      var breakdown = new Dictionary<string, (double Length, int Count)>(StringComparer.OrdinalIgnoreCase);

      foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes"))
      {
        var pipe = transaction.GetObject(pipeId, OpenMode.ForRead);
        var length = GetRequiredEngineeringDouble(pipe, $"pressure pipe '{CivilObjectUtils.GetName(pipe)}' length", "Length3D", "Length2D", "Length");
        var diameter = GetRequiredEngineeringDouble(pipe, $"pressure pipe '{CivilObjectUtils.GetName(pipe)}' diameter", "InnerDiameter", "OuterDiameter", "Diameter", "InnerDiameterOrWidth");
        var material = GetAnyString(pipe, "Material", "PipeFamily", "PartFamilyName", "PartDescription") ?? "Unknown";

        totalLength += length;
        pipeCount++;

        var key = "All";
        if (groupBySize && groupByMaterial)
          key = $"{material} Ø{diameter:F0}mm";
        else if (groupBySize)
          key = $"Ø{diameter:F0}mm";
        else if (groupByMaterial)
          key = material;

        if (breakdown.TryGetValue(key, out var existing))
          breakdown[key] = (existing.Length + length, existing.Count + 1);
        else
          breakdown[key] = (length, 1);
      }

      var breakdownList = breakdown
        .Select(kvp => new Dictionary<string, object?>
        {
          ["key"] = kvp.Key,
          ["length"] = kvp.Value.Length,
          ["count"] = kvp.Value.Count,
        })
        .OrderByDescending(d => d["length"])
        .ToList();

      return new Dictionary<string, object?>
      {
        ["networkName"] = name,
        ["totalLength"] = totalLength,
        ["pipeCount"] = pipeCount,
        ["breakdown"] = breakdownList,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyParcelAreas
  // -------------------------------------------------------------------------

  public static Task<object?> QtyParcelAreasAsync(JsonObject? parameters)
  {
    var siteName = PluginRuntime.GetOptionalString(parameters, "siteName");
    var parcelNamesNode = PluginRuntime.GetParameter(parameters, "parcelNames") as JsonArray;
    var requestedParcelNames = parcelNamesNode?
      .Select(n => n?.GetValue<string>())
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Cast<string>()
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var parcelResults = new List<Dictionary<string, object?>>();
      var totalArea = 0.0;

      foreach (ObjectId siteId in civilDoc.GetSiteIds())
      {
        var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, siteId, OpenMode.ForRead);
        var currentSiteName = site.Name;

        if (!string.IsNullOrWhiteSpace(siteName) &&
            !currentSiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase))
          continue;

        foreach (ObjectId parcelId in site.GetParcelIds())
        {
          if (parcelId == ObjectId.Null) continue;
          var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, parcelId, OpenMode.ForRead);
          var parcelName = parcel.Name;

          if (requestedParcelNames != null && !requestedParcelNames.Contains(parcelName)) continue;

          var area = parcel.Area;
          var perimeter = CivilObjectUtils.GetDoubleProperty(parcel, "Perimeter");

          totalArea += area;
          parcelResults.Add(new Dictionary<string, object?>
          {
            ["name"] = parcelName,
            ["area"] = area,
            ["perimeter"] = perimeter,
            ["siteName"] = currentSiteName,
          });
        }
      }

      return new Dictionary<string, object?>
      {
        ["parcels"] = parcelResults,
        ["totalArea"] = totalArea,
        ["parcelCount"] = parcelResults.Count,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyAlignmentLengths
  // -------------------------------------------------------------------------

  public static Task<object?> QtyAlignmentLengthsAsync(JsonObject? parameters)
  {
    var namesNode = PluginRuntime.GetParameter(parameters, "names") as JsonArray;
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");

    var requestedNames = namesNode?
      .Select(n => n?.GetValue<string>())
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Cast<string>()
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignmentResults = new List<Dictionary<string, object?>>();
      var totalLength = 0.0;

      foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
      {
        var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignId, OpenMode.ForRead);

        if (requestedNames != null && !requestedNames.Contains(alignment.Name)) continue;

        // Clamp station range
        var start = startStation.HasValue ? Math.Max(startStation.Value, alignment.StartingStation) : alignment.StartingStation;
        var end = endStation.HasValue ? Math.Min(endStation.Value, alignment.EndingStation) : alignment.EndingStation;
        var segmentLength = Math.Max(0, end - start);

        totalLength += segmentLength;
        alignmentResults.Add(new Dictionary<string, object?>
        {
          ["name"] = alignment.Name,
          ["length"] = segmentLength,
          ["totalLength"] = alignment.Length,
          ["startStation"] = start,
          ["endStation"] = end,
        });
      }

      return new Dictionary<string, object?>
      {
        ["alignments"] = alignmentResults,
        ["totalLength"] = totalLength,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyPointCountByGroup
  // -------------------------------------------------------------------------

  public static Task<object?> QtyPointCountByGroupAsync(JsonObject? parameters)
  {
    var groupNamesNode = PluginRuntime.GetParameter(parameters, "groupNames") as JsonArray;
    var requestedGroupNames = groupNamesNode?
      .Select(n => n?.GetValue<string>())
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Cast<string>()
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var groupResults = new List<Dictionary<string, object?>>();
      var totalPoints = 0;

      foreach (ObjectId groupId in civilDoc.PointGroups)
      {
        if (groupId == ObjectId.Null) continue;
        var group = CivilObjectUtils.GetRequiredObject<PointGroup>(transaction, groupId, OpenMode.ForRead);
        var groupName = group.Name;

        if (requestedGroupNames != null && !requestedGroupNames.Contains(groupName)) continue;

        var count = group.GetPointNumbers().Length;

        totalPoints += count;
        groupResults.Add(new Dictionary<string, object?>
        {
          ["name"] = groupName,
          ["count"] = count,
        });
      }

      return new Dictionary<string, object?>
      {
        ["groups"] = groupResults,
        ["totalPoints"] = totalPoints,
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyExportToCsv
  // -------------------------------------------------------------------------

  public static Task<object?> QtyExportToCsvAsync(JsonObject? parameters)
  {
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;
    var includeAlignments = PluginRuntime.GetOptionalBool(parameters, "includeAlignments") ?? true;
    var includeSurfaces = PluginRuntime.GetOptionalBool(parameters, "includeSurfaces") ?? false;
    var includePipeNetworks = PluginRuntime.GetOptionalBool(parameters, "includePipeNetworks") ?? false;
    var includeParcelAreas = PluginRuntime.GetOptionalBool(parameters, "includeParcelAreas") ?? false;
    var includePointGroups = PluginRuntime.GetOptionalBool(parameters, "includePointGroups") ?? false;
    var filterNamesNode = PluginRuntime.GetParameter(parameters, "filterNames") as JsonArray;
    var filterNames = filterNamesNode?
      .Select(n => n?.GetValue<string>())
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Cast<string>()
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sb = new StringBuilder();
      var rowsWritten = 0;
      var sectionsIncluded = new List<string>();

      if (includeAlignments)
      {
        sectionsIncluded.Add("alignments");
        sb.AppendLine("Section,Name,Length,StartStation,EndStation,Units");
        foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
        {
          var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignId, OpenMode.ForRead);
          if (filterNames != null && !filterNames.Contains(alignment.Name)) continue;
          sb.AppendLine($"Alignment,{EscapeCsv(alignment.Name)},{alignment.Length:F3},{alignment.StartingStation:F3},{alignment.EndingStation:F3},{CivilObjectUtils.LinearUnits(database)}");
          rowsWritten++;
        }
        sb.AppendLine();
      }

      if (includeSurfaces)
      {
        sectionsIncluded.Add("surfaces");
        sb.AppendLine("Section,Name,Type,MinElevation,MaxElevation,NumberOfPoints,Units");
        foreach (ObjectId surfId in civilDoc.GetSurfaceIds())
        {
          var surface = CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, surfId, OpenMode.ForRead);
          if (filterNames != null && !filterNames.Contains(surface.Name)) continue;
          var gp = surface.GetGeneralProperties();
          sb.AppendLine($"Surface,{EscapeCsv(surface.Name)},{surface.GetType().Name},{gp.MinimumElevation:F3},{gp.MaximumElevation:F3},{gp.NumberOfPoints},{CivilObjectUtils.LinearUnits(database)}");
          rowsWritten++;
        }
        sb.AppendLine();
      }

      if (includePipeNetworks)
      {
        sectionsIncluded.Add("pipeNetworks");
        sb.AppendLine("Section,NetworkName,PipeName,Length,Diameter,Slope,Material,Units");
        foreach (var networkObj in EnumerateAllPipeNetworks(civilDoc, transaction))
        {
          var networkName = CivilObjectUtils.GetName(networkObj) ?? "unknown";
          if (filterNames != null && !filterNames.Contains(networkName)) continue;
          foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
          {
            var pipe = transaction.GetObject(pipeId, OpenMode.ForRead);
            var pipeName = CivilObjectUtils.GetName(pipe) ?? pipeId.Handle.ToString();
            var length = GetRequiredEngineeringDouble(pipe, $"pipe '{pipeName}' length", "Length3D", "Length2D", "Length");
            var diameter = GetRequiredEngineeringDouble(pipe, $"pipe '{pipeName}' diameter", "InnerDiameterOrWidth", "InnerDiameter", "Diameter");
            var slope = GetRequiredEngineeringDouble(pipe, $"pipe '{pipeName}' slope", "Slope", "FlowSlope");
            var material = GetAnyString(pipe, "Material", "PartDescription", "PartFamilyName") ?? string.Empty;
            sb.AppendLine($"PipeNetwork,{EscapeCsv(networkName)},{EscapeCsv(pipeName)},{length:F3},{diameter:F3},{slope:F6},{EscapeCsv(material)},{CivilObjectUtils.LinearUnits(database)}");
            rowsWritten++;
          }
        }
        sb.AppendLine();
      }

      if (includeParcelAreas)
      {
        sectionsIncluded.Add("parcelAreas");
        sb.AppendLine("Section,SiteName,ParcelName,Area,Perimeter,Units");
        foreach (ObjectId siteId in civilDoc.GetSiteIds())
        {
          var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, siteId, OpenMode.ForRead);
          var currentSiteName = site.Name;
          foreach (ObjectId parcelId in site.GetParcelIds())
          {
            if (parcelId == ObjectId.Null) continue;
            var parcel = CivilObjectUtils.GetRequiredObject<Parcel>(transaction, parcelId, OpenMode.ForRead);
            var parcelName = parcel.Name;
            if (filterNames != null && !filterNames.Contains(parcelName)) continue;
            var area = parcel.Area;
            var perimeter = CivilObjectUtils.GetDoubleProperty(parcel, "Perimeter");
            var perimeterText = perimeter.HasValue ? perimeter.Value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
            sb.AppendLine($"Parcel,{EscapeCsv(currentSiteName)},{EscapeCsv(parcelName)},{area:F3},{perimeterText},{CivilObjectUtils.LinearUnits(database)}");
            rowsWritten++;
          }
        }
        sb.AppendLine();
      }

      if (includePointGroups)
      {
        sectionsIncluded.Add("pointGroups");
        sb.AppendLine("Section,GroupName,PointCount");
        foreach (ObjectId groupId in civilDoc.PointGroups)
        {
          if (groupId == ObjectId.Null) continue;
          var group = CivilObjectUtils.GetRequiredObject<PointGroup>(transaction, groupId, OpenMode.ForRead);
          var groupName = group.Name;
          if (filterNames != null && !filterNames.Contains(groupName)) continue;
          var count = group.GetPointNumbers().Length;
          sb.AppendLine($"PointGroup,{EscapeCsv(groupName)},{count}");
          rowsWritten++;
        }
        sb.AppendLine();
      }

      var canonicalOutputPath = FileBoundary.WriteAllTextAtomic(
        outputPath, sb.ToString(), Encoding.UTF8, overwrite, ".csv");

      return new Dictionary<string, object?>
      {
        ["outputPath"] = canonicalOutputPath,
        ["rowsWritten"] = rowsWritten,
        ["sectionsIncluded"] = sectionsIncluded,
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyMaterialListGet
  // -------------------------------------------------------------------------

  public static Task<object?> QtyMaterialListGetAsync(JsonObject? parameters)
  {
    _ = PluginRuntime.GetRequiredString(parameters, "corridorName");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "Civil 3D 2026 QTO material lists belong to SampleLineGroup objects. Corridor baselines do not expose GetMaterials/GetMaterialQuantities; no empty or zero-valued material list was fabricated.");
  }

  // -------------------------------------------------------------------------
  // qtyEarthworkSummary
  // -------------------------------------------------------------------------

  public static Task<object?> QtyEarthworkSummaryAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var designSurfaceName = PluginRuntime.GetRequiredString(parameters, "designSurface");
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");
    if (startStation.HasValue || endStation.HasValue || !string.IsNullOrWhiteSpace(PluginRuntime.GetOptionalString(parameters, "alignmentName")))
      throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", "Station-clipped earthwork requires sample lines/QTO. The managed TinVolumeSurface API computes whole-surface volume only; no station-volume approximation was used.");

    return CivilExecution.ReadInCommandAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var designSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, designSurfaceName, OpenMode.ForRead);
      var volumeId = TinVolumeSurface.Create($"MCP_TMP_EARTHWORK_{Guid.NewGuid():N}", baseSurface.ObjectId, designSurface.ObjectId);
      var volumeSurface = CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, volumeId, OpenMode.ForRead);
      var volumes = volumeSurface.GetVolumeProperties();

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["designSurface"] = designSurfaceName,
        ["cumulativeCut"] = volumes.UnadjustedCutVolume,
        ["cumulativeFill"] = volumes.UnadjustedFillVolume,
        ["netEarthwork"] = volumes.UnadjustedNetVolume,
        ["stationCount"] = 0,
        ["stations"] = Array.Empty<object>(),
        ["volumeUnits"] = CivilObjectUtils.VolumeUnits(database),
        ["note"] = "Exact whole-surface TinVolumeSurface quantities; station breakdown is not exposed by this API path.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private static AcDbObject? FindPipeNetworkByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    foreach (ObjectId objectId in civilDoc.GetPipeNetworkIds())
    {
      if (objectId == ObjectId.Null) continue;
      var obj = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
        return obj;
    }
    return null;
  }

  private static AcDbObject? FindPressureNetworkByName(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    foreach (ObjectId objectId in civilDoc.GetPressurePipeNetworkIds())
    {
      if (objectId == ObjectId.Null) continue;
      var obj = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
        return obj;
    }
    return null;
  }

  private static IEnumerable<AcDbObject> EnumerateAllPipeNetworks(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction)
  {
    foreach (ObjectId objectId in civilDoc.GetPipeNetworkIds())
    {
      if (objectId == ObjectId.Null) continue;
      yield return transaction.GetObject(objectId, OpenMode.ForRead);
    }
  }

  private static IEnumerable<ObjectId> GetChildObjectIds(AcDbObject owner, params string[] memberNames)
  {
    foreach (var memberName in memberNames)
    {
      var methodResult = CivilObjectUtils.InvokeMethod(owner, memberName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(methodResult))
      {
        if (objectId != ObjectId.Null) yield return objectId;
      }
      var propVal = CivilObjectUtils.GetPropertyValue<object>(owner, memberName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(propVal))
      {
        if (objectId != ObjectId.Null) yield return objectId;
      }
    }
  }

  private static double? GetAnyDouble(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var v = CivilObjectUtils.GetPropertyValue<double?>(value, propertyName);
      if (v.HasValue) return v.Value;
    }
    return null;
  }

  private static double GetRequiredEngineeringDouble(object? value, string valueDescription, params string[] propertyNames)
  {
    var result = GetAnyDouble(value, propertyNames);
    if (result.HasValue) return result.Value;
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      $"Could not read {valueDescription} from the managed API. Zero was not substituted.");
  }

  private static string? GetAnyString(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var v = CivilObjectUtils.GetStringProperty(value, propertyName);
      if (!string.IsNullOrWhiteSpace(v)) return v;
    }
    return null;
  }

  private static double? InvokeSurfaceElevationSafe(CivilSurface surface, double x, double y)
  {
    try
    {
      var result = surface.FindElevationAtXY(x, y);
      return result;
    }
    catch
    {
      // Point may be outside surface boundary
      return null;
    }
  }

  private static string EscapeCsv(string value)
  {
    if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
      return $"\"{value.Replace("\"", "\"\"")}\"";
    return value;
  }
}
