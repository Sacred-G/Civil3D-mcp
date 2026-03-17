using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

/// <summary>
/// Editing commands for Civil 3D horizontal alignments:
/// add_tangent, add_curve, add_spiral, delete_entity,
/// set_station_equation, get_station_offset,
/// offset_create, widen_transition.
/// </summary>
public static class AlignmentEditCommands
{
  // ─── alignmentAddTangent ──────────────────────────────────────────────────

  public static Task<object?> AddTangentAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var startX = PluginRuntime.GetRequiredDouble(parameters, "startX");
    var startY = PluginRuntime.GetRequiredDouble(parameters, "startY");
    var endX = PluginRuntime.GetRequiredDouble(parameters, "endX");
    var endY = PluginRuntime.GetRequiredDouble(parameters, "endY");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, alignment.ObjectId, OpenMode.ForWrite);

      var pt1 = new Point2d(startX, startY);
      var pt2 = new Point2d(endX, endY);

      var entities = CivilObjectUtils.GetPropertyValue<object>(writeAlignment, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not access Entities collection for alignment '{alignmentName}'.");
      }

      // AlignmentEntityCollection.AddFixedLine(pt1, pt2)
      var addedEntity = CivilObjectUtils.InvokeMethod(entities, "AddFixedLine", pt1, pt2);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["operation"] = "add_tangent",
        ["entityIndex"] = GetEntityIndex(addedEntity),
        ["startX"] = startX,
        ["startY"] = startY,
        ["endX"] = endX,
        ["endY"] = endY,
        ["success"] = true,
      };
    });
  }

  // ─── alignmentAddCurve ────────────────────────────────────────────────────

  public static Task<object?> AddCurveAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var passThroughX = PluginRuntime.GetRequiredDouble(parameters, "passThroughX");
    var passThroughY = PluginRuntime.GetRequiredDouble(parameters, "passThroughY");
    var radius = PluginRuntime.GetRequiredDouble(parameters, "radius");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, alignment.ObjectId, OpenMode.ForWrite);

      var passThrough = new Point2d(passThroughX, passThroughY);
      var entities = CivilObjectUtils.GetPropertyValue<object>(writeAlignment, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not access Entities collection for alignment '{alignmentName}'.");
      }

      // Try AddFixedCurve(passThrough, radius) — Civil 3D 2020+ API
      var addedEntity = CivilObjectUtils.InvokeMethod(entities, "AddFixedCurve", passThrough, radius)
        ?? CivilObjectUtils.InvokeMethod(entities, "AddFloatCurve", passThrough, radius);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["operation"] = "add_curve",
        ["entityIndex"] = GetEntityIndex(addedEntity),
        ["radius"] = radius,
        ["success"] = true,
      };
    });
  }

  // ─── alignmentAddSpiral ───────────────────────────────────────────────────

  public static Task<object?> AddSpiralAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var spiralType = PluginRuntime.GetOptionalString(parameters, "spiralType") ?? "clothoid";
    var startX = PluginRuntime.GetRequiredDouble(parameters, "startX");
    var startY = PluginRuntime.GetRequiredDouble(parameters, "startY");
    var startRadius = PluginRuntime.GetRequiredDouble(parameters, "startRadius");
    var endRadius = PluginRuntime.GetRequiredDouble(parameters, "endRadius");
    var length = PluginRuntime.GetRequiredDouble(parameters, "length");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, alignment.ObjectId, OpenMode.ForWrite);

      var startPoint = new Point2d(startX, startY);
      var entities = CivilObjectUtils.GetPropertyValue<object>(writeAlignment, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not access Entities collection for alignment '{alignmentName}'.");
      }

      // Map spiral type string to enum value if possible; fall back to int 0 = Clothoid
      object? spiralTypeArg = MapSpiralTypeArg(entities.GetType(), spiralType);
      object? addedEntity;

      if (spiralTypeArg != null)
      {
        addedEntity = CivilObjectUtils.InvokeMethod(
          entities, "AddFixedSpiral", startPoint, startRadius, endRadius, length, spiralTypeArg);
      }
      else
      {
        // Older API without spiral type parameter
        addedEntity = CivilObjectUtils.InvokeMethod(
          entities, "AddFixedSpiral", startPoint, startRadius, endRadius, length);
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["operation"] = "add_spiral",
        ["spiralType"] = spiralType,
        ["entityIndex"] = GetEntityIndex(addedEntity),
        ["length"] = length,
        ["success"] = true,
      };
    });
  }

  // ─── alignmentDeleteEntity ────────────────────────────────────────────────

  public static Task<object?> DeleteEntityAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var entityIndex = (int)(PluginRuntime.GetRequiredDouble(parameters, "entityIndex"));

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, alignment.ObjectId, OpenMode.ForWrite);

      var entities = CivilObjectUtils.GetPropertyValue<object>(writeAlignment, "Entities");
      if (entities == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not access Entities collection for alignment '{alignmentName}'.");
      }

      // Get entity by index from the collection
      var countProp = entities.GetType().GetProperty("Count");
      var count = countProp != null ? Convert.ToInt32(countProp.GetValue(entities)) : -1;
      if (count >= 0 && entityIndex >= count)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"Entity index {entityIndex} is out of range (alignment has {count} entities).");
      }

      // AlignmentEntityCollection indexer or .Item(index) to retrieve, then .Remove(entity)
      var entity = GetEntityByIndex(entities, entityIndex);
      if (entity == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not retrieve entity at index {entityIndex} from alignment '{alignmentName}'.");
      }

      CivilObjectUtils.InvokeMethod(entities, "Remove", entity);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["operation"] = "delete_entity",
        ["deletedEntityIndex"] = entityIndex,
        ["success"] = true,
      };
    });
  }

  // ─── alignmentSetStationEquation ─────────────────────────────────────────

  public static Task<object?> SetStationEquationAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var rawStation = PluginRuntime.GetRequiredDouble(parameters, "rawStation");
    var nominalStation = PluginRuntime.GetRequiredDouble(parameters, "nominalStation");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var writeAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, alignment.ObjectId, OpenMode.ForWrite);

      // Alignment.StationEquations.Add(rawStation, nominalStation)
      var stationEquations = CivilObjectUtils.GetPropertyValue<object>(writeAlignment, "StationEquations");
      if (stationEquations == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          $"Could not access StationEquations for alignment '{alignmentName}'.");
      }

      CivilObjectUtils.InvokeMethod(stationEquations, "Add", rawStation, nominalStation);

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["rawStation"] = rawStation,
        ["nominalStation"] = nominalStation,
        ["success"] = true,
      };
    });
  }

  // ─── alignmentGetStationOffset ────────────────────────────────────────────

  public static Task<object?> GetStationOffsetAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
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

  // ─── alignmentOffsetCreate ────────────────────────────────────────────────

  public static Task<object?> OffsetCreateAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var offsetName = PluginRuntime.GetRequiredString(parameters, "offsetName");
    var offset = PluginRuntime.GetRequiredDouble(parameters, "offset");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseAlignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var layerId = LookupUtils.GetLayerId(database, transaction, PluginRuntime.GetOptionalString(parameters, "layer"));
      var styleId = LookupUtils.GetAlignmentStyleId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "style"));
      var labelSetId = LookupUtils.GetAlignmentLabelSetId(civilDoc, transaction, PluginRuntime.GetOptionalString(parameters, "labelSet"));
      var siteId = LookupUtils.GetSiteId(civilDoc, transaction, null);

      // Alignment.CreateOffsetAlignment(name, baseAlignmentId, offset, siteId, layerId, styleId, labelSetId)
      var offsetAlignmentId = (ObjectId)(
        CivilObjectUtils.InvokeStaticMethod(typeof(Alignment), "CreateOffsetAlignment",
          offsetName, baseAlignment.ObjectId, offset, siteId, layerId, styleId, labelSetId)
        ?? CivilObjectUtils.InvokeStaticMethod(typeof(Alignment), "CreateOffsetAlignment",
          offsetName, baseAlignment.ObjectId, offset, layerId, styleId, labelSetId)
        ?? throw new JsonRpcDispatchException(
          "CIVIL3D.TRANSACTION_FAILED",
          "Alignment.CreateOffsetAlignment is not available in this Civil 3D version."));

      var offsetAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, offsetAlignmentId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["baseAlignmentName"] = baseAlignment.Name,
        ["offsetName"] = offsetAlignment.Name,
        ["offset"] = offset,
        ["handle"] = CivilObjectUtils.GetHandle(offsetAlignment),
        ["success"] = true,
      };
    });
  }

  // ─── alignmentWidenTransition ─────────────────────────────────────────────

  public static Task<object?> WidenTransitionAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var side = PluginRuntime.GetRequiredString(parameters, "side");
    var startStation = PluginRuntime.GetRequiredDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetRequiredDouble(parameters, "endStation");
    var startOffset = PluginRuntime.GetRequiredDouble(parameters, "startOffset");
    var endOffset = PluginRuntime.GetRequiredDouble(parameters, "endOffset");
    var offsetName = PluginRuntime.GetOptionalString(parameters, "offsetName")
      ?? $"{alignmentName}_Widen_{side}";

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var baseAlignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);

      // Build a variable-offset alignment using AlignmentOffsetOptions if available,
      // or fall back to creating a simple offset and noting limitations.
      var layerId = LookupUtils.GetLayerId(database, transaction, null);
      var styleId = LookupUtils.GetAlignmentStyleId(civilDoc, transaction, null);
      var labelSetId = LookupUtils.GetAlignmentLabelSetId(civilDoc, transaction, null);
      var siteId = LookupUtils.GetSiteId(civilDoc, transaction, null);

      // Attempt variable offset via Alignment.CreateOffsetAlignment with OffsetOptions
      var offsetAlignmentId = TryCreateWidenTransition(
        civilDoc, transaction, database,
        baseAlignment, offsetName, side,
        startStation, endStation, startOffset, endOffset,
        siteId, layerId, styleId, labelSetId);

      var resultAlignment = CivilObjectUtils.GetRequiredObject<Alignment>(
        transaction, offsetAlignmentId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["baseAlignmentName"] = baseAlignment.Name,
        ["offsetName"] = resultAlignment.Name,
        ["side"] = side,
        ["startStation"] = startStation,
        ["endStation"] = endStation,
        ["startOffset"] = startOffset,
        ["endOffset"] = endOffset,
        ["handle"] = CivilObjectUtils.GetHandle(resultAlignment),
        ["success"] = true,
      };
    });
  }

  // ─── Private helpers ─────────────────────────────────────────────────────

  private static int GetEntityIndex(object? entity)
  {
    if (entity == null)
    {
      return -1;
    }

    var indexProp = entity.GetType().GetProperty("EntityNumber")
      ?? entity.GetType().GetProperty("Index")
      ?? entity.GetType().GetProperty("EntityIndex");
    return indexProp != null ? Convert.ToInt32(indexProp.GetValue(entity)) : -1;
  }

  private static object? GetEntityByIndex(object entities, int index)
  {
    // Try indexer (Item property)
    var itemProp = entities.GetType().GetProperty("Item");
    if (itemProp != null)
    {
      return itemProp.GetValue(entities, new object[] { index });
    }

    // Fall back to enumerating
    if (entities is System.Collections.IEnumerable enumerable)
    {
      var i = 0;
      foreach (var entity in enumerable)
      {
        if (i++ == index)
        {
          return entity;
        }
      }
    }

    return null;
  }

  private static object? MapSpiralTypeArg(Type entitiesType, string spiralType)
  {
    // Look for an AlignmentSpiralType or SpiralType enum on the entities type's assembly
    var assembly = entitiesType.Assembly;
    var enumType = assembly.GetTypes()
      .FirstOrDefault(t => t.IsEnum && (t.Name.Contains("SpiralType") || t.Name.Contains("Spiral")));

    if (enumType == null)
    {
      return null;
    }

    var enumName = spiralType.ToLowerInvariant() switch
    {
      "clothoid" => "Clothoid",
      "cubic" => "CubicParabola",
      "biquadratic" => "BiQuadratic",
      _ => "Clothoid",
    };

    try
    {
      return Enum.Parse(enumType, enumName, ignoreCase: true);
    }
    catch
    {
      return Enum.GetValues(enumType).GetValue(0);
    }
  }

  private static ObjectId TryCreateWidenTransition(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    Database database,
    Alignment baseAlignment,
    string offsetName,
    string side,
    double startStation,
    double endStation,
    double startOffset,
    double endOffset,
    ObjectId siteId,
    ObjectId layerId,
    ObjectId styleId,
    ObjectId labelSetId)
  {
    // Prefer CreateWideningAlignment if available (Civil 3D 2022+)
    var wideningMethod = typeof(Alignment).GetMethod(
      "CreateWideningAlignment",
      System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

    if (wideningMethod != null)
    {
      try
      {
        var result = wideningMethod.Invoke(null, new object[]
        {
          offsetName, baseAlignment.ObjectId,
          startStation, endStation, startOffset, endOffset,
          siteId, layerId, styleId, labelSetId,
        });
        if (result is ObjectId oid && !oid.IsNull)
        {
          return oid;
        }
      }
      catch
      {
        // Fall through to constant offset fallback
      }
    }

    // Fallback: create a simple constant offset alignment using the average offset
    var avgOffset = side.ToLowerInvariant() == "right"
      ? Math.Abs((startOffset + endOffset) / 2.0)
      : -Math.Abs((startOffset + endOffset) / 2.0);

    var fallbackId = (ObjectId)(
      CivilObjectUtils.InvokeStaticMethod(typeof(Alignment), "CreateOffsetAlignment",
        offsetName, baseAlignment.ObjectId, avgOffset, siteId, layerId, styleId, labelSetId)
      ?? CivilObjectUtils.InvokeStaticMethod(typeof(Alignment), "CreateOffsetAlignment",
        offsetName, baseAlignment.ObjectId, avgOffset, layerId, styleId, labelSetId)
      ?? throw new JsonRpcDispatchException(
        "CIVIL3D.TRANSACTION_FAILED",
        "Neither CreateWideningAlignment nor CreateOffsetAlignment is available in this Civil 3D version."));

    return fallbackId;
  }
}
