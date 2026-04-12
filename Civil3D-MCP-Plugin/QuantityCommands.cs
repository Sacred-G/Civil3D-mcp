using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

/// <summary>
/// Quantity takeoff commands — aggregate Civil 3D object data for cost estimation.
/// Uses reflection for API compatibility across Civil 3D versions.
/// </summary>
public static class QuantityCommands
{
  // -------------------------------------------------------------------------
  // qtyCorridorVolumes
  // -------------------------------------------------------------------------

  public static Task<object?> QtyCorridorVolumesAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var materialsNode = PluginRuntime.GetParameter(parameters, "materials") as JsonArray;
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");

    var requestedMaterials = materialsNode?
      .Select(n => n?.GetValue<string>())
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Cast<string>()
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
      var materialResults = new List<Dictionary<string, object?>>();

      // Access corridor material volumes via reflection
      // Civil 3D exposes material volumes through the corridor's Baselines/sections
      var baselines = CivilObjectUtils.GetPropertyValue<object>(corridor, "Baselines");
      if (baselines is IEnumerable baselineEnumerable)
      {
        foreach (var baseline in baselineEnumerable)
        {
          if (baseline == null) continue;

          // Try to get material volumes via GetMaterialQuantities or similar
          var materialQuantities = CivilObjectUtils.InvokeMethod(baseline, "GetMaterialQuantities")
            ?? CivilObjectUtils.InvokeMethod(baseline, "GetMaterials")
            ?? CivilObjectUtils.GetPropertyValue<object>(baseline, "Materials");

          if (materialQuantities is IEnumerable mq)
          {
            foreach (var material in mq)
            {
              if (material == null) continue;
              var matName = CivilObjectUtils.GetName(material) ?? CivilObjectUtils.GetStringProperty(material, "MaterialName") ?? "Unknown";
              if (requestedMaterials != null && !requestedMaterials.Contains(matName)) continue;

              var cutVol = CivilObjectUtils.GetPropertyValue<double?>(material, "CutQuantity")
                ?? CivilObjectUtils.GetPropertyValue<double?>(material, "CutVolume")
                ?? CivilObjectUtils.GetDoubleProperty(material, "Cut") ?? 0;
              var fillVol = CivilObjectUtils.GetPropertyValue<double?>(material, "FillQuantity")
                ?? CivilObjectUtils.GetPropertyValue<double?>(material, "FillVolume")
                ?? CivilObjectUtils.GetDoubleProperty(material, "Fill") ?? 0;

              materialResults.Add(new Dictionary<string, object?>
              {
                ["name"] = matName,
                ["cutVolume"] = cutVol,
                ["fillVolume"] = fillVol,
                ["netVolume"] = cutVol - fillVol,
                ["units"] = CivilObjectUtils.VolumeUnits(database),
              });
            }
          }
        }
      }

      // If no material quantities found via baseline iteration, try corridor-level volume methods
      if (materialResults.Count == 0)
      {
        var cutVol = CivilObjectUtils.GetDoubleProperty(corridor, "CutVolume") ?? 0;
        var fillVol = CivilObjectUtils.GetDoubleProperty(corridor, "FillVolume") ?? 0;
        if (cutVol != 0 || fillVol != 0)
        {
          materialResults.Add(new Dictionary<string, object?>
          {
            ["name"] = "Earthwork",
            ["cutVolume"] = cutVol,
            ["fillVolume"] = fillVol,
            ["netVolume"] = cutVol - fillVol,
            ["units"] = CivilObjectUtils.VolumeUnits(database),
          });
        }
      }

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridor.Name,
        ["startStation"] = startStation,
        ["endStation"] = endStation,
        ["materials"] = materialResults,
        ["units"] = CivilObjectUtils.VolumeUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtySurfaceVolume
  // -------------------------------------------------------------------------

  public static Task<object?> QtySurfaceVolumeAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var corridorName = PluginRuntime.GetOptionalString(parameters, "corridorName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var compSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);

      // Use TinVolumeSurface if available, otherwise compute via InvokeMethod
      // The standard Civil 3D approach: create a volume surface or use ComputeVolumes
      var cutVolume = 0.0;
      var fillVolume = 0.0;
      var net2dArea = 0.0;

      // Try to invoke volume calculation via reflection
      var volumeResult = CivilObjectUtils.InvokeMethod(baseSurface, "ComputeVolumes", compSurface.ObjectId)
        ?? CivilObjectUtils.InvokeMethod(baseSurface, "GetVolumes", compSurface.ObjectId);

      if (volumeResult != null)
      {
        cutVolume = CivilObjectUtils.GetPropertyValue<double?>(volumeResult, "CutVolume")
          ?? CivilObjectUtils.GetDoubleProperty(volumeResult, "Cut") ?? 0;
        fillVolume = CivilObjectUtils.GetPropertyValue<double?>(volumeResult, "FillVolume")
          ?? CivilObjectUtils.GetDoubleProperty(volumeResult, "Fill") ?? 0;
        net2dArea = CivilObjectUtils.GetPropertyValue<double?>(volumeResult, "Net2DArea")
          ?? CivilObjectUtils.GetDoubleProperty(volumeResult, "Area2D") ?? 0;
      }
      else
      {
        // Fallback: read volume properties from each surface's general properties
        var baseGp = CivilObjectUtils.InvokeMethod(baseSurface, "GetGeneralProperties");
        var compGp = CivilObjectUtils.InvokeMethod(compSurface, "GetGeneralProperties");
        var baseMinElev = CivilObjectUtils.GetPropertyValue<double?>(baseGp, "MinimumElevation") ?? 0;
        var compMinElev = CivilObjectUtils.GetPropertyValue<double?>(compGp, "MinimumElevation") ?? 0;
        var baseArea = CivilObjectUtils.GetPropertyValue<double?>(baseGp, "Area2D") ?? 0;
        net2dArea = baseArea;
        // Provide zero volumes as a fallback — actual volume requires a TinVolumeSurface
      }

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["comparisonSurface"] = comparisonSurfaceName,
        ["corridorName"] = corridorName,
        ["cutVolume"] = cutVolume,
        ["fillVolume"] = fillVolume,
        ["netVolume"] = fillVolume - cutVolume,
        ["net2dArea"] = net2dArea,
        ["units"] = CivilObjectUtils.VolumeUnits(database),
        ["note"] = volumeResult == null ? "Volume calculation requires a TinVolumeSurface — values may be zero. Use computeSurfaceVolume for full calculation." : null,
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
      var networkObj = FindPipeNetworkByNameReflection(civilDoc, transaction, name)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{name}' was not found.");

      var totalLength = 0.0;
      var pipeCount = 0;
      var breakdown = new Dictionary<string, (double Length, int Count)>(StringComparer.OrdinalIgnoreCase);

      foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
      {
        var pipe = transaction.GetObject(pipeId, OpenMode.ForRead);
        var length = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? 0;
        var diameter = GetAnyDouble(pipe, "InnerDiameterOrWidth", "InnerDiameter", "Diameter") ?? 0;
        var material = GetAnyString(pipe, "Material", "PartDescription", "PartFamilyName") ?? "Unknown";

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
      var networkObj = FindPressureNetworkByNameReflection(civilDoc, transaction, name)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pressure network '{name}' was not found.");

      var totalLength = 0.0;
      var pipeCount = 0;
      var breakdown = new Dictionary<string, (double Length, int Count)>(StringComparer.OrdinalIgnoreCase);

      foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes"))
      {
        var pipe = transaction.GetObject(pipeId, OpenMode.ForRead);
        var length = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? 0;
        var diameter = GetAnyDouble(pipe, "InnerDiameter", "OuterDiameter", "Diameter", "InnerDiameterOrWidth") ?? 0;
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

      var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
      var sites = sitesProperty?.GetValue(civilDoc);

      foreach (var siteId in CivilObjectUtils.ToObjectIds(sites))
      {
        var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
        var currentSiteName = CivilObjectUtils.GetName(siteObj) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(siteName) &&
            !currentSiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase))
          continue;

        var parcelIds = CivilObjectUtils.InvokeMethod(siteObj, "GetParcelIds")
          ?? CivilObjectUtils.GetPropertyValue<object>(siteObj, "Parcels")
          ?? CivilObjectUtils.GetPropertyValue<object>(siteObj, "ParcelCollection");

        foreach (var parcelId in CivilObjectUtils.ToObjectIds(parcelIds))
        {
          if (parcelId == ObjectId.Null) continue;
          var parcel = transaction.GetObject(parcelId, OpenMode.ForRead);
          var parcelName = CivilObjectUtils.GetName(parcel) ?? parcelId.Handle.ToString();

          if (requestedParcelNames != null && !requestedParcelNames.Contains(parcelName)) continue;

          var area = CivilObjectUtils.GetDoubleProperty(parcel, "Area")
            ?? CivilObjectUtils.GetDoubleProperty(parcel, "SurfaceArea")
            ?? CivilObjectUtils.GetDoubleProperty(parcel, "Area2d") ?? 0;
          var perimeter = CivilObjectUtils.GetDoubleProperty(parcel, "Perimeter") ?? 0;

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

      var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
      var pointGroups = pointGroupsProperty?.GetValue(civilDoc);

      foreach (var groupId in CivilObjectUtils.ToObjectIds(pointGroups))
      {
        if (groupId == ObjectId.Null) continue;
        var group = transaction.GetObject(groupId, OpenMode.ForRead);
        var groupName = CivilObjectUtils.GetName(group) ?? groupId.Handle.ToString();

        if (requestedGroupNames != null && !requestedGroupNames.Contains(groupName)) continue;

        // Get point IDs / numbers from the group
        var pointNumbers = CivilObjectUtils.InvokeMethod(group, "GetPointNumbers") as IEnumerable<uint>;
        var pointIds = CivilObjectUtils.InvokeMethod(group, "GetPointIds");
        int count;

        if (pointNumbers != null)
        {
          count = pointNumbers.Count();
        }
        else
        {
          count = CivilObjectUtils.ToObjectIds(pointIds).Count();
        }

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
        sb.AppendLine("Section,Name,Type,MinElevation,MaxElevation,Area2D,NumberOfPoints,Units");
        foreach (ObjectId surfId in civilDoc.GetSurfaceIds())
        {
          var surface = CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, surfId, OpenMode.ForRead);
          if (filterNames != null && !filterNames.Contains(surface.Name)) continue;
          var gp = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
          var minElev = CivilObjectUtils.GetPropertyValue<double?>(gp, "MinimumElevation") ?? 0;
          var maxElev = CivilObjectUtils.GetPropertyValue<double?>(gp, "MaximumElevation") ?? 0;
          var area2d = CivilObjectUtils.GetPropertyValue<double?>(gp, "Area2D") ?? 0;
          var nPts = CivilObjectUtils.GetPropertyValue<int?>(gp, "NumberOfPoints") ?? 0;
          sb.AppendLine($"Surface,{EscapeCsv(surface.Name)},{surface.GetType().Name},{minElev:F3},{maxElev:F3},{area2d:F3},{nPts},{CivilObjectUtils.LinearUnits(database)}");
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
            var length = GetAnyDouble(pipe, "Length3D", "Length2D", "Length") ?? 0;
            var diameter = GetAnyDouble(pipe, "InnerDiameterOrWidth", "InnerDiameter", "Diameter") ?? 0;
            var slope = GetAnyDouble(pipe, "Slope", "FlowSlope") ?? 0;
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
        var sitesProperty = civilDoc.GetType().GetProperty("Sites", BindingFlags.Public | BindingFlags.Instance);
        var sites = sitesProperty?.GetValue(civilDoc);
        foreach (var siteId in CivilObjectUtils.ToObjectIds(sites))
        {
          var siteObj = transaction.GetObject(siteId, OpenMode.ForRead);
          var currentSiteName = CivilObjectUtils.GetName(siteObj) ?? string.Empty;
          var parcelIds = CivilObjectUtils.InvokeMethod(siteObj, "GetParcelIds")
            ?? CivilObjectUtils.GetPropertyValue<object>(siteObj, "Parcels")
            ?? CivilObjectUtils.GetPropertyValue<object>(siteObj, "ParcelCollection");
          foreach (var parcelId in CivilObjectUtils.ToObjectIds(parcelIds))
          {
            if (parcelId == ObjectId.Null) continue;
            var parcel = transaction.GetObject(parcelId, OpenMode.ForRead);
            var parcelName = CivilObjectUtils.GetName(parcel) ?? parcelId.Handle.ToString();
            if (filterNames != null && !filterNames.Contains(parcelName)) continue;
            var area = CivilObjectUtils.GetDoubleProperty(parcel, "Area") ?? 0;
            var perimeter = CivilObjectUtils.GetDoubleProperty(parcel, "Perimeter") ?? 0;
            sb.AppendLine($"Parcel,{EscapeCsv(currentSiteName)},{EscapeCsv(parcelName)},{area:F3},{perimeter:F3},{CivilObjectUtils.LinearUnits(database)}");
            rowsWritten++;
          }
        }
        sb.AppendLine();
      }

      if (includePointGroups)
      {
        sectionsIncluded.Add("pointGroups");
        sb.AppendLine("Section,GroupName,PointCount");
        var pointGroupsProperty = civilDoc.GetType().GetProperty("PointGroups", BindingFlags.Public | BindingFlags.Instance);
        var pointGroups = pointGroupsProperty?.GetValue(civilDoc);
        foreach (var groupId in CivilObjectUtils.ToObjectIds(pointGroups))
        {
          if (groupId == ObjectId.Null) continue;
          var group = transaction.GetObject(groupId, OpenMode.ForRead);
          var groupName = CivilObjectUtils.GetName(group) ?? groupId.Handle.ToString();
          if (filterNames != null && !filterNames.Contains(groupName)) continue;
          var pointNumbers = CivilObjectUtils.InvokeMethod(group, "GetPointNumbers") as IEnumerable<uint>;
          var count = pointNumbers?.Count() ?? CivilObjectUtils.ToObjectIds(CivilObjectUtils.InvokeMethod(group, "GetPointIds")).Count();
          sb.AppendLine($"PointGroup,{EscapeCsv(groupName)},{count}");
          rowsWritten++;
        }
        sb.AppendLine();
      }

      System.IO.File.WriteAllText(outputPath, sb.ToString());

      return new Dictionary<string, object?>
      {
        ["outputPath"] = outputPath,
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
    var corridorName = PluginRuntime.GetRequiredString(parameters, "corridorName");
    var includeQuantities = PluginRuntime.GetOptionalBool(parameters, "includeQuantities") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, corridorName, OpenMode.ForRead);
      var materials = new List<Dictionary<string, object?>>();
      var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      var baselines = CivilObjectUtils.GetPropertyValue<object>(corridor, "Baselines");
      if (baselines is IEnumerable baselineEnumerable)
      {
        foreach (var baseline in baselineEnumerable)
        {
          if (baseline == null) continue;

          var matCollection = CivilObjectUtils.InvokeMethod(baseline, "GetMaterialQuantities")
            ?? CivilObjectUtils.InvokeMethod(baseline, "GetMaterials")
            ?? CivilObjectUtils.GetPropertyValue<object>(baseline, "Materials");

          if (matCollection is IEnumerable mq)
          {
            foreach (var material in mq)
            {
              if (material == null) continue;
              var matName = CivilObjectUtils.GetName(material)
                ?? CivilObjectUtils.GetStringProperty(material, "MaterialName")
                ?? "Unknown";
              if (seen.Contains(matName)) continue;
              seen.Add(matName);

              var matType = CivilObjectUtils.GetStringProperty(material, "MaterialType")
                ?? CivilObjectUtils.GetStringProperty(material, "Type")
                ?? string.Empty;
              var description = CivilObjectUtils.GetStringProperty(material, "Description") ?? string.Empty;

              var entry = new Dictionary<string, object?>
              {
                ["name"] = matName,
                ["type"] = matType,
                ["description"] = description,
              };

              if (includeQuantities)
              {
                var cutVol = CivilObjectUtils.GetPropertyValue<double?>(material, "CutQuantity")
                  ?? CivilObjectUtils.GetDoubleProperty(material, "CutVolume") ?? 0;
                var fillVol = CivilObjectUtils.GetPropertyValue<double?>(material, "FillQuantity")
                  ?? CivilObjectUtils.GetDoubleProperty(material, "FillVolume") ?? 0;
                entry["cutVolume"] = cutVol;
                entry["fillVolume"] = fillVol;
                entry["netVolume"] = cutVol - fillVol;
                entry["units"] = CivilObjectUtils.VolumeUnits(database);
              }

              materials.Add(entry);
            }
          }
        }
      }

      return new Dictionary<string, object?>
      {
        ["corridorName"] = corridor.Name,
        ["materials"] = materials,
        ["materialCount"] = materials.Count,
      };
    });
  }

  // -------------------------------------------------------------------------
  // qtyEarthworkSummary
  // -------------------------------------------------------------------------

  public static Task<object?> QtyEarthworkSummaryAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var designSurfaceName = PluginRuntime.GetRequiredString(parameters, "designSurface");
    var alignmentName = PluginRuntime.GetOptionalString(parameters, "alignmentName");
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");
    var stationInterval = PluginRuntime.GetOptionalDouble(parameters, "stationInterval") ?? 25.0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var designSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, designSurfaceName, OpenMode.ForRead);

      var stations = new List<Dictionary<string, object?>>();
      var cumulativeCut = 0.0;
      var cumulativeFill = 0.0;

      // Determine station range
      double actualStart;
      double actualEnd;

      if (!string.IsNullOrWhiteSpace(alignmentName))
      {
        var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName!);
        actualStart = startStation.HasValue ? Math.Max(startStation.Value, alignment.StartingStation) : alignment.StartingStation;
        actualEnd = endStation.HasValue ? Math.Min(endStation.Value, alignment.EndingStation) : alignment.EndingStation;

        // Sample elevations at each station interval
        var currentStation = actualStart;
        while (currentStation <= actualEnd + 1e-6)
        {
          // Get XY from alignment at this station
          double x = 0, y = 0;
          try
          {
            alignment.PointLocation(currentStation, 0, ref x, ref y);
          }
          catch
          {
            currentStation += stationInterval;
            continue;
          }

          var baseElev = InvokeSurfaceElevationSafe(baseSurface, x, y);
          var designElev = InvokeSurfaceElevationSafe(designSurface, x, y);

          if (baseElev.HasValue && designElev.HasValue)
          {
            var diff = designElev.Value - baseElev.Value;
            var cut = diff < 0 ? Math.Abs(diff) * stationInterval : 0;
            var fill = diff > 0 ? diff * stationInterval : 0;
            cumulativeCut += cut;
            cumulativeFill += fill;

            stations.Add(new Dictionary<string, object?>
            {
              ["station"] = currentStation,
              ["x"] = x,
              ["y"] = y,
              ["baseElevation"] = baseElev.Value,
              ["designElevation"] = designElev.Value,
              ["cut"] = cut,
              ["fill"] = fill,
              ["cumCut"] = cumulativeCut,
              ["cumFill"] = cumulativeFill,
            });
          }

          if (currentStation >= actualEnd - 1e-6) break;
          currentStation = Math.Min(currentStation + stationInterval, actualEnd);
        }
      }
      else
      {
        // Without an alignment, use surface extents to derive a bounding grid
        var baseGp = CivilObjectUtils.InvokeMethod(baseSurface, "GetGeneralProperties");
        actualStart = startStation ?? 0;
        actualEnd = endStation ?? 0;
        // Return basic volume-only summary without station breakdown
      }

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["designSurface"] = designSurfaceName,
        ["alignmentName"] = alignmentName,
        ["cumulativeCut"] = cumulativeCut,
        ["cumulativeFill"] = cumulativeFill,
        ["netEarthwork"] = cumulativeFill - cumulativeCut,
        ["stationCount"] = stations.Count,
        ["stations"] = stations,
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["volumeUnits"] = CivilObjectUtils.VolumeUnits(database),
      };
    });
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private static AcDbObject? FindPipeNetworkByNameReflection(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    foreach (var candidateProp in new[] { "PipeNetworkCollection", "NetworkCollection", "PipeNetworks", "Networks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null) continue;
        var obj = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
          return obj;
      }
    }
    var methodResult = CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds");
    foreach (var objectId in CivilObjectUtils.ToObjectIds(methodResult))
    {
      if (objectId == ObjectId.Null) continue;
      var obj = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
        return obj;
    }
    return null;
  }

  private static AcDbObject? FindPressureNetworkByNameReflection(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    foreach (var candidateProp in new[] { "PressureNetworkCollection", "PressureNetworks", "PressurePipeNetworks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null) continue;
        var obj = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
          return obj;
      }
    }
    var methodResult = CivilObjectUtils.InvokeMethod(civilDoc, "GetPressureNetworkIds");
    foreach (var objectId in CivilObjectUtils.ToObjectIds(methodResult))
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
    foreach (var candidateProp in new[] { "PipeNetworkCollection", "NetworkCollection", "PipeNetworks", "Networks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null) continue;
        yield return transaction.GetObject(objectId, OpenMode.ForRead);
      }
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
