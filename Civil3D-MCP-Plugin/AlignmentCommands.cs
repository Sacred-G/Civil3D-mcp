using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class AlignmentCommands
{
  public static Task<object?> ListAlignmentsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignments = new List<Dictionary<string, object?>>();
      foreach (ObjectId objectId in civilDoc.GetAlignmentIds())
      {
        var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, objectId, OpenMode.ForRead);
        alignments.Add(ToAlignmentSummary(alignment));
      }

      return new Dictionary<string, object?>
      {
        ["alignments"] = alignments,
      };
    });
  }

  public static Task<object?> GetAlignmentAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, name);
      var entities = new List<Dictionary<string, object?>>();
      var entityCollection = CivilObjectUtils.GetPropertyValue<object>(alignment, "Entities");
      if (entityCollection is System.Collections.IEnumerable enumerable)
      {
        var index = 0;
        foreach (var entity in enumerable)
        {
          entities.Add(new Dictionary<string, object?>
          {
            ["index"] = index++,
            ["type"] = MapAlignmentEntityType(CivilObjectUtils.GetStringProperty(entity, "EntityType") ?? entity?.GetType().Name),
            ["startStation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "StartStation") ?? 0,
            ["endStation"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "EndStation") ?? 0,
            ["length"] = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0,
          });
        }
      }

      var dependentProfiles = alignment.GetProfileIds()
        .Cast<ObjectId>()
        .Select(id => CivilObjectUtils.GetRequiredObject<Profile>(transaction, id, OpenMode.ForRead).Name)
        .ToList();

      return new Dictionary<string, object?>
      {
        ["name"] = alignment.Name,
        ["handle"] = CivilObjectUtils.GetHandle(alignment),
        ["type"] = MapAlignmentType(CivilObjectUtils.GetStringProperty(alignment, "AlignmentType") ?? alignment.GetType().Name),
        ["style"] = CivilObjectUtils.GetName(transaction.GetObject(alignment.StyleId, OpenMode.ForRead)) ?? string.Empty,
        ["layer"] = alignment.Layer,
        ["length"] = alignment.Length,
        ["startStation"] = alignment.StartingStation,
        ["endStation"] = alignment.EndingStation,
        ["entityCount"] = entities.Count,
        ["entities"] = entities,
        ["dependentProfiles"] = dependentProfiles,
        ["dependentCorridors"] = new List<string>(),
        ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(alignment, "IsReferenceObject") ?? false,
      };
    });
  }

  public static Task<object?> StationToPointAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var station = PluginRuntime.GetRequiredDouble(parameters, "station");
    var offset = PluginRuntime.GetOptionalDouble(parameters, "offset") ?? 0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, name);
      double x = 0;
      double y = 0;
      alignment.PointLocation(station, offset, ref x, ref y);

      return new Dictionary<string, object?>
      {
        ["x"] = x,
        ["y"] = y,
        ["station"] = station,
        ["offset"] = offset,
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> PointToStationAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, name);
      double station = 0;
      double offset = 0;
      alignment.StationOffset(x, y, ref station, ref offset);

      return new Dictionary<string, object?>
      {
        ["station"] = station,
        ["offset"] = offset,
        ["distanceFromAlignment"] = Math.Abs(offset),
        ["units"] = CivilObjectUtils.LinearUnits(database),
      };
    });
  }

  public static Task<object?> CreateAlignmentAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createAlignment requires at least two points.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var polyline = new Polyline();
      for (var index = 0; index < pointsNode.Count; index++)
      {
        if (pointsNode[index] is not JsonObject point)
        {
          continue;
        }

        polyline.AddVertexAt(index, new Point2d(point["x"]!.GetValue<double>(), point["y"]!.GetValue<double>()), 0, 0, 0);
      }

      var polylineId = modelSpace.AppendEntity(polyline);
      transaction.AddNewlyCreatedDBObject(polyline, true);

      var polylineOptions = new PolylineOptions
      {
        AddCurvesBetweenTangents = true,
        EraseExistingEntities = true,
        PlineId = polylineId,
      };

      var layerId = LookupUtils.GetLayerId(database, transaction, PluginRuntime.GetOptionalString(parameters, "layer"));
      var styleId = LookupUtils.GetAlignmentStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var labelSetId = LookupUtils.GetAlignmentLabelSetId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "labelSet"));
      var siteId = LookupUtils.GetSiteId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "site"));

      var alignmentId = Alignment.Create(civilDoc, polylineOptions, name, siteId, layerId, styleId, labelSetId);
      var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignmentId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["name"] = alignment.Name,
        ["handle"] = CivilObjectUtils.GetHandle(alignment),
        ["created"] = true,
      };
    });
  }

  public static Task<object?> DeleteAlignmentAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, name);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignment.ObjectId, OpenMode.ForWrite);
      writeAlignment.Erase();

      return new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      };
    });
  }

  private static Dictionary<string, object?> ToAlignmentSummary(Alignment alignment)
  {
    return new Dictionary<string, object?>
    {
      ["name"] = alignment.Name,
      ["handle"] = CivilObjectUtils.GetHandle(alignment),
      ["type"] = MapAlignmentType(CivilObjectUtils.GetStringProperty(alignment, "AlignmentType") ?? alignment.GetType().Name),
      ["length"] = alignment.Length,
      ["startStation"] = alignment.StartingStation,
      ["endStation"] = alignment.EndingStation,
      ["site"] = CivilObjectUtils.GetStringProperty(alignment, "SiteName"),
      ["profileCount"] = alignment.GetProfileIds().Count,
      ["isReference"] = CivilObjectUtils.GetPropertyValue<bool?>(alignment, "IsReferenceObject") ?? false,
    };
  }

  private static string MapAlignmentType(string? value)
  {
    var text = value?.ToLowerInvariant() ?? string.Empty;
    if (text.Contains("offset"))
    {
      return "offset";
    }

    if (text.Contains("rail"))
    {
      return "rail";
    }

    if (text.Contains("curb"))
    {
      return "curb_return";
    }

    return "centerline";
  }

  private static string MapAlignmentEntityType(string? value)
  {
    var text = value?.ToLowerInvariant() ?? string.Empty;
    if (text.Contains("spiral"))
    {
      return "spiral";
    }

    if (text.Contains("curve") || text.Contains("arc"))
    {
      return "arc";
    }

    return "line";
  }
}
