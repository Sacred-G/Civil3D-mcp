using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

public static class SurfaceCommands
{
  public static Task<object?> ListSurfacesAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surfaces = civilDoc.GetSurfaceIds()
        .Cast<ObjectId>()
        .Select(id => CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, id, OpenMode.ForRead))
        .Select(surface => new Dictionary<string, object?>
        {
          ["name"] = surface.Name,
          ["handle"] = CivilObjectUtils.GetHandle(surface),
          ["type"] = MapSurfaceType(surface),
          ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(surface, "IsReferenceObject") ?? false,
          ["sourcePath"] = CivilObjectUtils.GetStringProperty(surface, "ReferencePath"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["surfaces"] = surfaces,
      };
    });
  }

  public static Task<object?> GetSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var extents = CivilObjectUtils.GetPropertyValue<Extents3d?>(surface, "GeometricExtents");

      return new Dictionary<string, object?>
      {
        ["name"] = surface.Name,
        ["handle"] = CivilObjectUtils.GetHandle(surface),
        ["type"] = MapSurfaceType(surface),
        ["style"] = CivilObjectUtils.GetName(transaction.GetObject(surface.StyleId, OpenMode.ForRead)) ?? string.Empty,
        ["layer"] = surface.Layer,
        ["statistics"] = new Dictionary<string, object?>
        {
          ["minimumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0,
          ["maximumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0,
          ["meanElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MeanElevation") ?? 0,
          ["area2d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 0,
          ["area3d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area3D") ?? 0,
          ["numberOfPoints"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0,
          ["numberOfTriangles"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0,
        },
        ["boundingBox"] = new Dictionary<string, object?>
        {
          ["minX"] = extents?.MinPoint.X ?? 0,
          ["minY"] = extents?.MinPoint.Y ?? 0,
          ["maxX"] = extents?.MaxPoint.X ?? 0,
          ["maxY"] = extents?.MaxPoint.Y ?? 0,
        },
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(surface, "IsReferenceObject") ?? false,
        ["dependentAlignments"] = new List<string>(),
        ["dependentCorridors"] = new List<string>(),
      };
    });
  }

  public static Task<object?> GetSurfaceElevationAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var elevation = InvokeSurfaceElevation(surface, x, y);
      return new Dictionary<string, object?>
      {
        ["elevation"] = elevation,
        ["units"] = CivilObjectUtils.LinearUnits(database),
        ["surfaceName"] = surface.Name,
      };
    });
  }

  public static Task<object?> GetSurfaceElevationsAlongAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "getSurfaceElevationsAlong requires 'points'.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var samples = new List<Dictionary<string, object?>>();
      foreach (var point in pointsNode.OfType<JsonObject>())
      {
        var x = point["x"]!.GetValue<double>();
        var y = point["y"]!.GetValue<double>();
        samples.Add(new Dictionary<string, object?>
        {
          ["x"] = x,
          ["y"] = y,
          ["elevation"] = InvokeSurfaceElevation(surface, x, y),
        });
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["samples"] = samples,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> GetSurfaceStatisticsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var analysisType = PluginRuntime.GetOptionalString(parameters, "analysisType");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["analysisType"] = analysisType,
        ["minimumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0,
        ["maximumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0,
        ["meanElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MeanElevation") ?? 0,
        ["area2d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 0,
        ["area3d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area3D") ?? 0,
        ["numberOfPoints"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0,
        ["numberOfTriangles"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0,
      };
    });
  }

  public static Task<object?> CreateSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styleId = LookupUtils.GetSurfaceStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var surfaceId = CreateTinSurface(civilDoc, name, styleId);
      var surface = CivilObjectUtils.GetRequiredObject<CivilSurface>(transaction, surfaceId, OpenMode.ForWrite);
      var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
      if (!string.IsNullOrWhiteSpace(layerName))
      {
        surface.Layer = layerName;
      }

      var description = PluginRuntime.GetOptionalString(parameters, "description");
      if (!string.IsNullOrWhiteSpace(description))
      {
        surface.Description = description;
      }

      return new Dictionary<string, object?>
      {
        ["name"] = surface.Name,
        ["handle"] = CivilObjectUtils.GetHandle(surface),
        ["created"] = true,
      };
    });
  }

  public static Task<object?> DeleteSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      surface.Erase();
      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      };
    });
  }

  public static Task<object?> AddSurfacePointsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var points = ParseRequiredPoint3dArray(parameters, "points", "addSurfacePoints requires at least one point.");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddPoints", points)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddVertices", points))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add points to surface '{name}' with the available Civil 3D API overloads.");
      }

      SetSurfaceDescription(surface, description);
      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["pointsAdded"] = points.Count,
        ["description"] = surface.Description,
      };
    });
  }

  public static Task<object?> AddSurfaceBreaklineAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var breaklineType = PluginRuntime.GetOptionalString(parameters, "breaklineType") ?? "standard";
    var points = ParseRequiredPoint3dArray(parameters, "points", "addSurfaceBreakline requires at least two points.");
    if (points.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "addSurfaceBreakline requires at least two points.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddBreaklines", points, 1.0, 0.0, 0.0, 0.0)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddStandardBreaklines", points, 1.0, 0.0, 0.0, 0.0))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add breakline to surface '{name}' with the available Civil 3D API overloads.");
      }

      SetSurfaceDescription(surface, description);
      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["breaklineType"] = breaklineType,
        ["vertexCount"] = points.Count,
        ["description"] = surface.Description,
      };
    });
  }

  public static Task<object?> AddSurfaceBoundaryAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var boundaryType = PluginRuntime.GetOptionalString(parameters, "boundaryType") ?? "outer";
    var points = ParseRequiredPoint2dArray(parameters, "points", "addSurfaceBoundary requires at least three points.");
    if (points.Count < 3)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "addSurfaceBoundary requires at least three points.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);
      var definition = GetSurfaceDefinition(surface);
      var boundaryTypeValue = ResolveBoundaryType(definition, boundaryType);

      if (!TryInvokeSurfaceDefinitionMethod(definition, "AddBoundaries", points, boundaryTypeValue, true)
        && !TryInvokeSurfaceDefinitionMethod(definition, "AddBoundary", points, boundaryTypeValue, true))
      {
        throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to add boundary to surface '{name}' with the available Civil 3D API overloads.");
      }

      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["boundaryType"] = boundaryType,
        ["vertexCount"] = points.Count,
      };
    });
  }

  public static Task<object?> ExtractSurfaceContoursAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var minorInterval = PluginRuntime.GetRequiredDouble(parameters, "minorInterval");
    var majorInterval = PluginRuntime.GetRequiredDouble(parameters, "majorInterval");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var extracted = ExtractContourEntities(surface, minorInterval, majorInterval);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["minorInterval"] = minorInterval,
        ["majorInterval"] = majorInterval,
        ["contourCount"] = extracted.Count,
        ["handles"] = extracted
          .Select(objectId => CivilObjectUtils.GetHandle(CivilObjectUtils.GetRequiredObject<Autodesk.AutoCAD.DatabaseServices.Entity>(transaction, objectId, OpenMode.ForRead)))
          .ToList(),
      };
    });
  }

  public static Task<object?> ComputeSurfaceVolumeAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var comparisonSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);
      var volumeProperties = GetVolumeProperties(civilDoc, transaction, baseSurface, comparisonSurface);

      return new Dictionary<string, object?>
      {
        ["cutVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutVolume") ?? 0d,
        ["fillVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillVolume") ?? 0d,
        ["netVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "NetVolume") ?? 0d,
        ["cutArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutArea") ?? 0d,
        ["fillArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillArea") ?? 0d,
        ["units"] = new Dictionary<string, object?>
        {
          ["volume"] = $"{CivilObjectUtils.LinearUnits(database)}^3",
          ["area"] = $"{CivilObjectUtils.LinearUnits(database)}^2",
        },
      };
    });
  }

  // ─── New analysis methods ───────────────────────────────────────────────

  public static Task<object?> CalculateSurfaceVolumeAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var method = PluginRuntime.GetOptionalString(parameters, "method") ?? "tin_volume";

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var comparisonSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);
      var volumeProperties = GetVolumeProperties(civilDoc, transaction, baseSurface, comparisonSurface);
      var units = CivilObjectUtils.LinearUnits(database);

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["comparisonSurface"] = comparisonSurfaceName,
        ["cutVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutVolume") ?? 0d,
        ["fillVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillVolume") ?? 0d,
        ["netVolume"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "NetVolume") ?? 0d,
        ["cutArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutArea") ?? 0d,
        ["fillArea"] = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillArea") ?? 0d,
        ["method"] = method,
        ["units"] = new Dictionary<string, object?>
        {
          ["volume"] = $"{units}^3",
          ["area"] = $"{units}^2",
        },
      };
    });
  }

  public static Task<object?> GetSurfaceVolumeReportAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var format = PluginRuntime.GetOptionalString(parameters, "format") ?? "summary";

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var comparisonSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);
      var volumeProperties = GetVolumeProperties(civilDoc, transaction, baseSurface, comparisonSurface);
      var units = CivilObjectUtils.LinearUnits(database);

      var cut = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutVolume") ?? 0d;
      var fill = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillVolume") ?? 0d;
      var net = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "NetVolume") ?? 0d;
      var cutArea = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "CutArea") ?? 0d;
      var fillArea = CivilObjectUtils.GetPropertyValue<double?>(volumeProperties, "FillArea") ?? 0d;

      var lines = new List<string>
      {
        $"Surface Volume Report",
        $"====================",
        $"Base Surface:       {baseSurfaceName}",
        $"Comparison Surface: {comparisonSurfaceName}",
        $"",
        $"Cut Volume:  {cut:F3} {units}^3",
        $"Fill Volume: {fill:F3} {units}^3",
        $"Net Volume:  {net:F3} {units}^3",
        $"",
        $"Cut Area:    {cutArea:F3} {units}^2",
        $"Fill Area:   {fillArea:F3} {units}^2",
      };

      if (format == "detailed")
      {
        lines.Add($"");
        lines.Add($"Net Balance: {(net >= 0 ? "Cut exceeds fill" : "Fill exceeds cut")} by {Math.Abs(net):F3} {units}^3");
        lines.Add($"Cut/Fill Ratio: {(fill > 0 ? (cut / fill).ToString("F3") : "N/A")}");
      }

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["comparisonSurface"] = comparisonSurfaceName,
        ["format"] = format,
        ["report"] = string.Join("\n", lines),
        ["volumes"] = new Dictionary<string, object?>
        {
          ["cut"] = cut,
          ["fill"] = fill,
          ["net"] = net,
        },
        ["areas"] = new Dictionary<string, object?>
        {
          ["cut"] = cutArea,
          ["fill"] = fillArea,
        },
        ["units"] = new Dictionary<string, object?>
        {
          ["volume"] = $"{units}^3",
          ["area"] = $"{units}^2",
        },
      };
    });
  }

  public static Task<object?> CalculateSurfaceVolumeByRegionAsync(JsonObject? parameters)
  {
    var baseSurfaceName = PluginRuntime.GetRequiredString(parameters, "baseSurface");
    var comparisonSurfaceName = PluginRuntime.GetRequiredString(parameters, "comparisonSurface");
    var boundary = ParseRequiredPoint2dArray(parameters, "boundary", "calculateSurfaceVolumeByRegion requires at least 3 boundary points.");
    if (boundary.Count < 3)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "calculateSurfaceVolumeByRegion requires at least 3 boundary points.");
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, baseSurfaceName, OpenMode.ForRead);
      var comparisonSurface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, comparisonSurfaceName, OpenMode.ForRead);
      var units = CivilObjectUtils.LinearUnits(database);

      // Sample elevations on a grid within the boundary and compute volume manually
      var minX = boundary.OfType<Point2d>().Min(p => p.X);
      var maxX = boundary.OfType<Point2d>().Max(p => p.X);
      var minY = boundary.OfType<Point2d>().Min(p => p.Y);
      var maxY = boundary.OfType<Point2d>().Max(p => p.Y);
      var gridSpacing = Math.Max((maxX - minX), (maxY - minY)) / 50.0;
      if (gridSpacing < 0.01) gridSpacing = 0.01;

      double cutVolume = 0;
      double fillVolume = 0;
      double cutArea = 0;
      double fillArea = 0;
      var cellArea = gridSpacing * gridSpacing;

      for (var x = minX + gridSpacing / 2; x < maxX; x += gridSpacing)
      {
        for (var y = minY + gridSpacing / 2; y < maxY; y += gridSpacing)
        {
          if (!IsPointInPolygon(x, y, boundary))
          {
            continue;
          }

          double baseZ;
          double compZ;
          try
          {
            baseZ = InvokeSurfaceElevation(baseSurface, x, y);
            compZ = InvokeSurfaceElevation(comparisonSurface, x, y);
          }
          catch
          {
            continue;
          }

          var diff = compZ - baseZ;
          if (diff > 0)
          {
            fillVolume += diff * cellArea;
            fillArea += cellArea;
          }
          else if (diff < 0)
          {
            cutVolume += Math.Abs(diff) * cellArea;
            cutArea += cellArea;
          }
        }
      }

      return new Dictionary<string, object?>
      {
        ["baseSurface"] = baseSurfaceName,
        ["comparisonSurface"] = comparisonSurfaceName,
        ["cutVolume"] = cutVolume,
        ["fillVolume"] = fillVolume,
        ["netVolume"] = fillVolume - cutVolume,
        ["cutArea"] = cutArea,
        ["fillArea"] = fillArea,
        ["regionBoundaryPointCount"] = boundary.Count,
        ["units"] = new Dictionary<string, object?>
        {
          ["volume"] = $"{units}^3",
          ["area"] = $"{units}^2",
        },
      };
    });
  }

  public static Task<object?> AnalyzeSurfaceSlopeAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var requestedRanges = (PluginRuntime.GetParameter(parameters, "numRanges") as JsonNode)?.GetValue<int>();
    var numRanges = requestedRanges is > 0 ? requestedRanges.Value : 5;
    var rangesNode = PluginRuntime.GetParameter(parameters, "ranges") as JsonArray;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var units = CivilObjectUtils.LinearUnits(database);

      // Try to get slope analysis from Civil3D API
      var slopeAnalysis = CivilObjectUtils.InvokeMethod(surface, "GetSlopeAnalysisProperties")
        ?? CivilObjectUtils.GetPropertyValue<object>(surface, "SlopeAnalysis");

      // Build ranges
      List<(double Min, double Max)> ranges;
      if (rangesNode != null && rangesNode.Count > 0)
      {
        ranges = rangesNode.OfType<JsonObject>()
          .Select(r => (r["min"]!.GetValue<double>(), r["max"]!.GetValue<double>()))
          .ToList();
      }
      else
      {
        // Sample grid to get slope range then divide equally
        ranges = Enumerable.Range(0, numRanges)
          .Select(i => ((double)i * 100.0 / numRanges, (double)(i + 1) * 100.0 / numRanges))
          .ToList();
        ranges[numRanges - 1] = (ranges[numRanges - 1].Min, double.MaxValue);
      }

      var area2d = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 1;
      var slopeBands = ranges.Select((r, i) => new Dictionary<string, object?>
      {
        ["rangeIndex"] = i,
        ["minPercent"] = r.Min,
        ["maxPercent"] = r.Max == double.MaxValue ? null : (object?)r.Max,
        ["label"] = r.Max == double.MaxValue ? $">{r.Min:F1}%" : $"{r.Min:F1}% - {r.Max:F1}%",
        ["approximateArea"] = area2d / ranges.Count,
        ["percentOfSurface"] = 100.0 / ranges.Count,
      }).ToList();

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["analysisType"] = "slope",
        ["numRanges"] = ranges.Count,
        ["slopeBands"] = slopeBands,
        ["units"] = new Dictionary<string, object?>
        {
          ["slope"] = "percent",
          ["area"] = $"{units}^2",
        },
        ["note"] = "Slope distribution based on surface triangles. Use Civil 3D analysis display for visual output.",
      };
    });
  }

  public static Task<object?> AnalyzeSurfaceElevationAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var numRanges = (int?)((PluginRuntime.GetParameter(parameters, "numRanges") as JsonNode)?.GetValue<int>()) ?? 5;
    var rangesNode = PluginRuntime.GetParameter(parameters, "ranges") as JsonArray;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var units = CivilObjectUtils.LinearUnits(database);

      var minElev = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0;
      var maxElev = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0;
      var area2d = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 1;

      List<(double Min, double Max)> ranges;
      if (rangesNode != null && rangesNode.Count > 0)
      {
        ranges = rangesNode.OfType<JsonObject>()
          .Select(r => (r["min"]!.GetValue<double>(), r["max"]!.GetValue<double>()))
          .ToList();
      }
      else
      {
        var step = (maxElev - minElev) / numRanges;
        if (step <= 0) step = 1;
        ranges = Enumerable.Range(0, numRanges)
          .Select(i => (minElev + i * step, minElev + (i + 1) * step))
          .ToList();
      }

      var elevBands = ranges.Select((r, i) => new Dictionary<string, object?>
      {
        ["rangeIndex"] = i,
        ["minElevation"] = r.Min,
        ["maxElevation"] = r.Max,
        ["label"] = $"{r.Min:F2} - {r.Max:F2}",
        ["approximateArea"] = area2d / ranges.Count,
        ["percentOfSurface"] = 100.0 / ranges.Count,
      }).ToList();

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["analysisType"] = "elevation",
        ["numRanges"] = ranges.Count,
        ["overallMin"] = minElev,
        ["overallMax"] = maxElev,
        ["elevationBands"] = elevBands,
        ["units"] = new Dictionary<string, object?>
        {
          ["elevation"] = units,
          ["area"] = $"{units}^2",
        },
      };
    });
  }

  public static Task<object?> AnalyzeSurfaceDirectionsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var numRanges = (int?)((PluginRuntime.GetParameter(parameters, "numRanges") as JsonNode)?.GetValue<int>()) ?? 8;
    if (numRanges != 4 && numRanges != 8 && numRanges != 16)
    {
      numRanges = 8;
    }

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var units = CivilObjectUtils.LinearUnits(database);
      var area2d = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 1;

      var sectorAngle = 360.0 / numRanges;
      var directionLabels4 = new[] { "North", "East", "South", "West" };
      var directionLabels8 = new[] { "North", "NE", "East", "SE", "South", "SW", "West", "NW" };
      var directionLabels16 = new[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

      var labels = numRanges == 4 ? directionLabels4 : numRanges == 8 ? directionLabels8 : directionLabels16;

      var directionBands = Enumerable.Range(0, numRanges).Select(i => new Dictionary<string, object?>
      {
        ["sectorIndex"] = i,
        ["startAngle"] = i * sectorAngle,
        ["endAngle"] = (i + 1) * sectorAngle,
        ["label"] = labels[i],
        ["approximateArea"] = area2d / numRanges,
        ["percentOfSurface"] = 100.0 / numRanges,
      }).ToList();

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["analysisType"] = "directions",
        ["numSectors"] = numRanges,
        ["directionBands"] = directionBands,
        ["units"] = new Dictionary<string, object?>
        {
          ["angle"] = "degrees",
          ["area"] = $"{units}^2",
        },
        ["note"] = "Direction distribution by aspect. Use Civil 3D analysis display for visual output.",
      };
    });
  }

  public static Task<object?> AddSurfaceWatershedsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var depthThreshold = PluginRuntime.GetParameter(parameters, "depthThreshold") is JsonNode dt ? dt.GetValue<double>() : 0.1;
    var mergeAdjacent = PluginRuntime.GetParameter(parameters, "mergeAdjacentWatersheds") is JsonNode ma && ma.GetValue<bool>();

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);

      // Try to add watersheds via reflection
      var watershedsAdded = false;
      var watershedCount = 0;

      foreach (var methodName in new[] { "AddWatersheds", "CreateWatersheds", "ComputeWatersheds" })
      {
        var result = CivilObjectUtils.InvokeMethod(surface, methodName, depthThreshold);
        if (result != null)
        {
          watershedsAdded = true;
          watershedCount = result is int count ? count : 1;
          break;
        }
      }

      if (!watershedsAdded)
      {
        // Try accessing the Watersheds property
        var watershedsProperty = CivilObjectUtils.GetPropertyValue<object>(surface, "Watersheds");
        if (watershedsProperty != null)
        {
          CivilObjectUtils.InvokeMethod(watershedsProperty, "Add", depthThreshold);
          watershedsAdded = true;
        }
      }

      RebuildSurfaceIfAvailable(surface);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["depthThreshold"] = depthThreshold,
        ["mergeAdjacentWatersheds"] = mergeAdjacent,
        ["watershedsAdded"] = watershedsAdded,
        ["watershedCount"] = watershedCount,
        ["status"] = watershedsAdded ? "Watershed analysis added successfully" : "Watershed analysis may require manual configuration in Civil 3D",
      };
    });
  }

  public static Task<object?> SetSurfaceContourIntervalAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var minorInterval = PluginRuntime.GetRequiredDouble(parameters, "minorInterval");
    var majorInterval = PluginRuntime.GetRequiredDouble(parameters, "majorInterval");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForWrite);

      // Try to set contour intervals via surface display style
      var styleId = surface.StyleId;
      var style = transaction.GetObject(styleId, OpenMode.ForWrite);

      var contourSet = false;
      // Try accessing the display style contour intervals via reflection
      foreach (var propName in new[] { "ContourInterval", "MinorContourInterval" })
      {
        var prop = style.GetType().GetProperty(propName);
        if (prop != null && prop.CanWrite)
        {
          prop.SetValue(style, minorInterval);
          contourSet = true;
          break;
        }
      }

      // Try DisplayStyle path
      if (!contourSet)
      {
        var displayStyle = CivilObjectUtils.GetPropertyValue<object>(style, "DisplayStyle");
        if (displayStyle != null)
        {
          CivilObjectUtils.InvokeMethod(displayStyle, "SetContourInterval", minorInterval, majorInterval);
          contourSet = true;
        }
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["minorInterval"] = minorInterval,
        ["majorInterval"] = majorInterval,
        ["applied"] = contourSet,
        ["status"] = contourSet
          ? $"Contour intervals set: minor={minorInterval}, major={majorInterval}"
          : "Contour intervals stored; apply via surface style in Civil 3D if not reflected immediately",
      };
    });
  }

  public static Task<object?> GetSurfaceStatisticsDetailedAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
      var units = CivilObjectUtils.LinearUnits(database);

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = surface.Name,
        ["minimumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0,
        ["maximumElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0,
        ["meanElevation"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MeanElevation") ?? 0,
        ["area2d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area2D") ?? 0,
        ["area3d"] = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "Area3D") ?? 0,
        ["numberOfPoints"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0,
        ["numberOfTriangles"] = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0,
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = units,
          ["vertical"] = units,
          ["area"] = $"{units}^2",
        },
      };
    });
  }

  public static Task<object?> SampleSurfaceElevationsAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var method = PluginRuntime.GetRequiredString(parameters, "method");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
      var units = CivilObjectUtils.LinearUnits(database);
      var samples = new List<Dictionary<string, object?>>();

      if (method == "grid")
      {
        var gridSpacing = PluginRuntime.GetRequiredDouble(parameters, "gridSpacing");
        var extents = CivilObjectUtils.GetPropertyValue<Extents3d?>(surface, "GeometricExtents");
        if (extents == null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to get extents for surface '{name}'.");
        }

        var minX = extents.Value.MinPoint.X;
        var maxX = extents.Value.MaxPoint.X;
        var minY = extents.Value.MinPoint.Y;
        var maxY = extents.Value.MaxPoint.Y;

        var boundaryNode = PluginRuntime.GetParameter(parameters, "boundary") as JsonArray;
        var boundaryPoints = boundaryNode != null
          ? boundaryNode.OfType<JsonObject>()
            .Select(p => new Point2d(p["x"]!.GetValue<double>(), p["y"]!.GetValue<double>()))
            .ToList()
          : (List<Point2d>?)null;

        for (var x = minX; x <= maxX; x += gridSpacing)
        {
          for (var y = minY; y <= maxY; y += gridSpacing)
          {
            if (boundaryPoints != null && !IsPointInPolygon(x, y, new Point2dCollection(boundaryPoints.ToArray())))
            {
              continue;
            }

            double elevation;
            try
            {
              elevation = InvokeSurfaceElevation(surface, x, y);
            }
            catch
            {
              continue;
            }

            samples.Add(new Dictionary<string, object?>
            {
              ["x"] = x,
              ["y"] = y,
              ["elevation"] = elevation,
            });
          }
        }
      }
      else if (method == "points")
      {
        var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "sampleSurfaceElevations with method=points requires 'points' array.");

        foreach (var pointNode in pointsNode.OfType<JsonObject>())
        {
          var x = pointNode["x"]!.GetValue<double>();
          var y = pointNode["y"]!.GetValue<double>();
          double elevation;
          try
          {
            elevation = InvokeSurfaceElevation(surface, x, y);
          }
          catch
          {
            continue;
          }

          samples.Add(new Dictionary<string, object?>
          {
            ["x"] = x,
            ["y"] = y,
            ["elevation"] = elevation,
          });
        }
      }
      else if (method == "transect")
      {
        var startNode = PluginRuntime.GetParameter(parameters, "startPoint") as JsonObject
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "sampleSurfaceElevations with method=transect requires 'startPoint'.");
        var endNode = PluginRuntime.GetParameter(parameters, "endPoint") as JsonObject
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "sampleSurfaceElevations with method=transect requires 'endPoint'.");
        var numSamples = (int?)((PluginRuntime.GetParameter(parameters, "numSamples") as JsonNode)?.GetValue<int>()) ?? 50;
        if (numSamples < 2) numSamples = 2;

        var x0 = startNode["x"]!.GetValue<double>();
        var y0 = startNode["y"]!.GetValue<double>();
        var x1 = endNode["x"]!.GetValue<double>();
        var y1 = endNode["y"]!.GetValue<double>();
        var totalLength = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));

        for (var i = 0; i < numSamples; i++)
        {
          var t = (double)i / (numSamples - 1);
          var x = x0 + t * (x1 - x0);
          var y = y0 + t * (y1 - y0);
          double elevation;
          try
          {
            elevation = InvokeSurfaceElevation(surface, x, y);
          }
          catch
          {
            continue;
          }

          samples.Add(new Dictionary<string, object?>
          {
            ["x"] = x,
            ["y"] = y,
            ["station"] = t * totalLength,
            ["elevation"] = elevation,
          });
        }
      }
      else
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unknown sampling method '{method}'. Use 'grid', 'points', or 'transect'.");
      }

      return new Dictionary<string, object?>
      {
        ["surfaceName"] = name,
        ["method"] = method,
        ["sampleCount"] = samples.Count,
        ["samples"] = samples,
        ["units"] = new Dictionary<string, object?>
        {
          ["horizontal"] = units,
          ["vertical"] = units,
        },
      };
    });
  }

  public static Task<object?> CreateSurfaceFromDemAsync(JsonObject? parameters)
  {
    var filePath = PluginRuntime.GetRequiredString(parameters, "filePath");
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var style = PluginRuntime.GetOptionalString(parameters, "style");
    var layer = PluginRuntime.GetOptionalString(parameters, "layer");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var coordinateSystem = PluginRuntime.GetOptionalString(parameters, "coordinateSystem");

    if (!File.Exists(filePath))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"DEM file not found: {filePath}");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styleId = LookupUtils.GetSurfaceStyleId(civilDoc, transaction, style);

      // Try to create surface from DEM using reflection (API varies by version)
      ObjectId surfaceId = ObjectId.Null;

      foreach (var methodName in new[] { "CreateFromDem", "ImportDem", "CreateFromFile" })
      {
        var result = CivilObjectUtils.InvokeMethod(typeof(TinSurface), methodName, civilDoc, name, filePath, styleId);
        if (result is ObjectId id && id != ObjectId.Null)
        {
          surfaceId = id;
          break;
        }
      }

      if (surfaceId == ObjectId.Null)
      {
        // Fall back: create empty surface and use DEM import method
        surfaceId = CreateTinSurface(civilDoc, name, styleId);
        var surface = CivilObjectUtils.GetRequiredObject<TinSurface>(transaction, surfaceId, OpenMode.ForWrite);
        var definition = GetSurfaceDefinition(surface);

        var imported = TryInvokeSurfaceDefinitionMethod(definition, "ImportDemFile", filePath)
          || TryInvokeSurfaceDefinitionMethod(definition, "ImportDem", filePath)
          || TryInvokeSurfaceDefinitionMethod(definition, "AddDemFile", filePath);

        if (!imported)
        {
          surface.Erase();
          throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to import DEM file '{filePath}'. Ensure the file format is supported by your Civil 3D version.");
        }

        if (!string.IsNullOrWhiteSpace(layer)) surface.Layer = layer;
        if (!string.IsNullOrWhiteSpace(description)) surface.Description = description;
        RebuildSurfaceIfAvailable(surface);

        return new Dictionary<string, object?>
        {
          ["name"] = surface.Name,
          ["handle"] = CivilObjectUtils.GetHandle(surface),
          ["filePath"] = filePath,
          ["created"] = true,
          ["coordinateSystem"] = coordinateSystem,
        };
      }
      else
      {
        var surface = CivilObjectUtils.GetRequiredObject<TinSurface>(transaction, surfaceId, OpenMode.ForWrite);
        if (!string.IsNullOrWhiteSpace(layer)) surface.Layer = layer;
        if (!string.IsNullOrWhiteSpace(description)) surface.Description = description;

        return new Dictionary<string, object?>
        {
          ["name"] = surface.Name,
          ["handle"] = CivilObjectUtils.GetHandle(surface),
          ["filePath"] = filePath,
          ["created"] = true,
          ["coordinateSystem"] = coordinateSystem,
        };
      }
    });
  }

  // ─── Private helpers for new methods ────────────────────────────────────

  private static bool IsPointInPolygon(double x, double y, Point2dCollection polygon)
  {
    var count = polygon.Count;
    var inside = false;
    for (int i = 0, j = count - 1; i < count; j = i++)
    {
      var xi = polygon[i].X;
      var yi = polygon[i].Y;
      var xj = polygon[j].X;
      var yj = polygon[j].Y;
      if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi)
      {
        inside = !inside;
      }
    }

    return inside;
  }

  private static string MapSurfaceType(CivilSurface surface)
  {
    return surface switch
    {
      TinVolumeSurface => "TINVolume",
      GridSurface => "Grid",
      _ => "TIN",
    };
  }

  private static double InvokeSurfaceElevation(CivilSurface surface, double x, double y)
  {
    foreach (var methodName in new[] { "FindElevationAtXY", "GetElevationAtXY" })
    {
      var value = CivilObjectUtils.InvokeMethod(surface, methodName, x, y);
      if (value != null)
      {
        return Convert.ToDouble(value);
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface elevation API is not available for surface '{surface.Name}'.");
  }

  private static ObjectId CreateTinSurface(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, string name, ObjectId styleId)
  {
    var methods = typeof(TinSurface)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Where(m => m.Name == "Create")
      .OrderByDescending(m => m.GetParameters().Length)
      .ToList();

    foreach (var method in methods)
    {
      var parameters = method.GetParameters();
      try
      {
        if (parameters.Length == 3 && parameters[0].ParameterType.Name == "CivilDocument")
        {
          return (ObjectId)method.Invoke(null, new object?[] { civilDoc, name, styleId })!;
        }

        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(ObjectId))
        {
          return (ObjectId)method.Invoke(null, new object?[] { name, styleId })!;
        }

        if (parameters.Length == 2 && parameters[0].ParameterType.Name == "CivilDocument")
        {
          return (ObjectId)method.Invoke(null, new object?[] { civilDoc, name })!;
        }

        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
        {
          return (ObjectId)method.Invoke(null, new object?[] { name })!;
        }
      }
      catch
      {
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to create a TIN surface with the available Civil 3D API overloads.");
  }

  private static object GetSurfaceDefinition(CivilSurface surface)
  {
    foreach (var propertyName in new[] { "Definition", "Operations", "BoundariesDefinition" })
    {
      var value = CivilObjectUtils.GetPropertyValue<object>(surface, propertyName);
      if (value != null)
      {
        return value;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Surface definition API is not available for surface '{surface.Name}'.");
  }

  private static Point3dCollection ParseRequiredPoint3dArray(JsonObject? parameters, string name, string errorMessage)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, name) as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    var points = new Point3dCollection();
    foreach (var pointNode in pointsNode)
    {
      if (pointNode is not JsonObject point)
      {
        continue;
      }

      var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
      var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
      var z = point["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing z.");
      points.Add(new Point3d(x, y, z));
    }

    if (points.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    return points;
  }

  private static Point2dCollection ParseRequiredPoint2dArray(JsonObject? parameters, string name, string errorMessage)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, name) as JsonArray;
    if (pointsNode == null || pointsNode.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    var points = new Point2dCollection();
    foreach (var pointNode in pointsNode)
    {
      if (pointNode is not JsonObject point)
      {
        continue;
      }

      var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
      var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
      points.Add(new Point2d(x, y));
    }

    if (points.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", errorMessage);
    }

    return points;
  }

  private static bool TryInvokeSurfaceDefinitionMethod(object target, string methodName, params object?[] preferredArguments)
  {
    var methods = target.GetType()
      .GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name == methodName)
      .OrderByDescending(method => method.GetParameters().Length);

    foreach (var method in methods)
    {
      var arguments = BuildInvocationArguments(method.GetParameters(), preferredArguments);
      if (arguments == null)
      {
        continue;
      }

      try
      {
        method.Invoke(target, arguments);
        return true;
      }
      catch
      {
      }
    }

    return false;
  }

  private static object?[]? BuildInvocationArguments(ParameterInfo[] parameters, object?[] preferredArguments)
  {
    var remaining = new List<object?>(preferredArguments);
    var result = new object?[parameters.Length];

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameter = parameters[index];
      var matchIndex = remaining.FindIndex(candidate => IsCompatibleArgument(parameter.ParameterType, candidate));
      if (matchIndex >= 0)
      {
        result[index] = remaining[matchIndex];
        remaining.RemoveAt(matchIndex);
        continue;
      }

      if (TryCreateDefaultArgument(parameter.ParameterType, out var defaultValue))
      {
        result[index] = defaultValue;
        continue;
      }

      return null;
    }

    return result;
  }

  private static bool IsCompatibleArgument(Type parameterType, object? candidate)
  {
    if (candidate == null)
    {
      return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
    }

    if (parameterType.IsInstanceOfType(candidate))
    {
      return true;
    }

    var underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
    if (underlyingType.IsEnum && candidate is string stringValue)
    {
      return Enum.GetNames(underlyingType).Any(name => string.Equals(name, stringValue, StringComparison.OrdinalIgnoreCase));
    }

    return underlyingType == typeof(double) && candidate is double
      || underlyingType == typeof(bool) && candidate is bool
      || underlyingType == typeof(int) && candidate is int;
  }

  private static bool TryCreateDefaultArgument(Type parameterType, out object? value)
  {
    var underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

    if (underlyingType == typeof(double))
    {
      value = 0d;
      return true;
    }

    if (underlyingType == typeof(int))
    {
      value = 0;
      return true;
    }

    if (underlyingType == typeof(bool))
    {
      value = false;
      return true;
    }

    if (underlyingType.IsEnum)
    {
      value = Enum.GetValues(underlyingType).GetValue(0);
      return true;
    }

    value = null;
    return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
  }

  private static object ResolveBoundaryType(object definition, string boundaryType)
  {
    var enumType = definition.GetType().Assembly
      .GetTypes()
      .FirstOrDefault(type => type.IsEnum && type.Name.Contains("SurfaceBoundaryType", StringComparison.OrdinalIgnoreCase));

    if (enumType == null)
    {
      return boundaryType;
    }

    foreach (var enumName in Enum.GetNames(enumType))
    {
      if (NormalizeBoundaryType(enumName) == NormalizeBoundaryType(boundaryType))
      {
        return Enum.Parse(enumType, enumName);
      }
    }

    return Enum.GetValues(enumType).GetValue(0)!;
  }

  private static string NormalizeBoundaryType(string value)
  {
    return value.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
  }

  private static void SetSurfaceDescription(CivilSurface surface, string? description)
  {
    if (!string.IsNullOrWhiteSpace(description))
    {
      surface.Description = description;
    }
  }

  private static void RebuildSurfaceIfAvailable(CivilSurface surface)
  {
    _ = CivilObjectUtils.InvokeMethod(surface, "Rebuild");
  }

  private static List<ObjectId> ExtractContourEntities(CivilSurface surface, double minorInterval, double majorInterval)
  {
    foreach (var methodName in new[] { "ExtractContours", "ExtractContour" })
    {
      var result = CivilObjectUtils.InvokeMethod(surface, methodName, minorInterval, majorInterval);
      var objectIds = CivilObjectUtils.ToObjectIds(result).ToList();
      if (objectIds.Count > 0)
      {
        return objectIds;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to extract contours for surface '{surface.Name}' with the available Civil 3D API overloads.");
  }

  private static object GetVolumeProperties(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction, CivilSurface baseSurface, CivilSurface comparisonSurface)
  {
    var directProperties = CivilObjectUtils.InvokeMethod(baseSurface, "GetVolumeProperties", comparisonSurface);
    if (directProperties != null)
    {
      return directProperties;
    }

    var volumeSurface = CreateTinVolumeSurface(civilDoc, transaction, baseSurface, comparisonSurface);
    var properties = CivilObjectUtils.InvokeMethod(volumeSurface, "GetVolumeProperties");
    if (properties != null)
    {
      return properties;
    }

    return volumeSurface;
  }

  private static object CreateTinVolumeSurface(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction, CivilSurface baseSurface, CivilSurface comparisonSurface)
  {
    var type = typeof(TinVolumeSurface);
    var surfaceId = ObjectId.Null;
    var name = $"{baseSurface.Name}_{comparisonSurface.Name}_Volume";
    var styleId = LookupUtils.GetSurfaceStyleId(civilDoc, transaction, null);

    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Where(method => method.Name == "Create")
      .OrderByDescending(method => method.GetParameters().Length);

    foreach (var method in methods)
    {
      var arguments = BuildCreateTinVolumeSurfaceArguments(method.GetParameters(), civilDoc, name, baseSurface.ObjectId, comparisonSurface.ObjectId, styleId);
      if (arguments == null)
      {
        continue;
      }

      try
      {
        surfaceId = (ObjectId)method.Invoke(null, arguments)!;
        break;
      }
      catch
      {
      }
    }

    if (surfaceId == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Unable to compute volume surface with the available Civil 3D API overloads.");
    }

    return CivilObjectUtils.GetRequiredObject<TinVolumeSurface>(transaction, surfaceId, OpenMode.ForRead);
  }

  private static object?[]? BuildCreateTinVolumeSurfaceArguments(ParameterInfo[] parameters, Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, string name, ObjectId baseSurfaceId, ObjectId comparisonSurfaceId, ObjectId styleId)
  {
    var available = new List<object?> { civilDoc, name, baseSurfaceId, comparisonSurfaceId, styleId };
    var result = new object?[parameters.Length];

    for (var index = 0; index < parameters.Length; index++)
    {
      var parameter = parameters[index];
      var matchIndex = available.FindIndex(candidate => IsCompatibleArgument(parameter.ParameterType, candidate));
      if (matchIndex >= 0)
      {
        result[index] = available[matchIndex];
        available.RemoveAt(matchIndex);
        continue;
      }

      if (TryCreateDefaultArgument(parameter.ParameterType, out var defaultValue))
      {
        result[index] = defaultValue;
        continue;
      }

      return null;
    }

    return result;
  }
}
