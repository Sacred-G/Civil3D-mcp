using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class LayerXrefCommands
{
  public static Task<object?> AttachXrefAsync(JsonObject? parameters)
  {
    var filePath = PluginRuntime.GetRequiredString(parameters, "filePath");
    var overlay = PluginRuntime.GetOptionalBool(parameters, "overlay") ?? true;
    var xrefName = PluginRuntime.GetOptionalString(parameters, "xrefName");
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");
    var x = PluginRuntime.GetOptionalDouble(parameters, "x") ?? 0d;
    var y = PluginRuntime.GetOptionalDouble(parameters, "y") ?? 0d;
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var scale = PluginRuntime.GetOptionalDouble(parameters, "scale") ?? 1d;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;

    if (!File.Exists(filePath))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Xref source file '{filePath}' does not exist.");
    }

    var blockName = string.IsNullOrWhiteSpace(xrefName)
      ? Path.GetFileNameWithoutExtension(filePath)
      : xrefName;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      ObjectId xrefBtrId;
      try
      {
        xrefBtrId = overlay
          ? database.OverlayXref(filePath, blockName)
          : database.AttachXref(filePath, blockName);
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Failed to {(overlay ? "overlay" : "attach")} xref '{filePath}': {ex.Message}");
      }

      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var xrefRef = new BlockReference(new Point3d(x, y, z), xrefBtrId)
      {
        ScaleFactors = new Scale3d(scale, scale, scale),
        Rotation = rotation,
      };

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        xrefRef.LayerId = layerId;
      }

      var xrefRefId = modelSpace.AppendEntity(xrefRef);
      transaction.AddNewlyCreatedDBObject(xrefRef, true);

      var xrefBtr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, xrefBtrId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(xrefRef),
        ["xrefName"] = xrefBtr.Name,
        ["filePath"] = filePath,
        ["overlay"] = overlay,
        ["x"] = xrefRef.Position.X,
        ["y"] = xrefRef.Position.Y,
        ["z"] = xrefRef.Position.Z,
        ["layer"] = xrefRef.Layer,
        ["xrefId"] = xrefRefId.Handle.ToString(),
      };
    });
  }

  public static Task<object?> CreateOrUpdateLayerAsync(JsonObject? parameters)
  {
    var layerName = PluginRuntime.GetRequiredString(parameters, "name");
    var colorIndex = PluginRuntime.GetOptionalInt(parameters, "colorIndex");
    var linetype = PluginRuntime.GetOptionalString(parameters, "linetype");
    var lineweight = PluginRuntime.GetOptionalInt(parameters, "lineweight");
    var plot = PluginRuntime.GetOptionalBool(parameters, "plot");
    var frozen = PluginRuntime.GetOptionalBool(parameters, "frozen");
    var locked = PluginRuntime.GetOptionalBool(parameters, "locked");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var layerTable = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForRead);

      LayerTableRecord layer;
      bool created;
      if (layerTable.Has(layerName))
      {
        layer = CivilObjectUtils.GetRequiredObject<LayerTableRecord>(transaction, layerTable[layerName], OpenMode.ForWrite);
        created = false;
      }
      else
      {
        var layerTableWrite = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForWrite);
        layer = new LayerTableRecord { Name = layerName };
        layerTableWrite.Add(layer);
        transaction.AddNewlyCreatedDBObject(layer, true);
        created = true;
      }

      if (colorIndex.HasValue)
      {
        layer.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex.Value);
      }

      if (!string.IsNullOrWhiteSpace(linetype))
      {
        layer.LinetypeObjectId = LookupUtils.GetOrLoadLinetypeId(database, transaction, linetype);
      }

      if (lineweight.HasValue)
      {
        layer.LineWeight = (LineWeight)lineweight.Value;
      }

      if (plot.HasValue)
      {
        layer.IsPlottable = plot.Value;
      }

      if (frozen.HasValue)
      {
        layer.IsFrozen = frozen.Value;
      }

      if (locked.HasValue)
      {
        layer.IsLocked = locked.Value;
      }

      return new Dictionary<string, object?>
      {
        ["name"] = layer.Name,
        ["created"] = created,
        ["colorIndex"] = layer.Color.ColorMethod == ColorMethod.ByAci ? (int)layer.Color.ColorIndex : (int?)null,
        ["linetype"] = LookupUtils.GetLinetypeName(transaction, layer.LinetypeObjectId),
        ["lineweight"] = (int)layer.LineWeight,
        ["plot"] = layer.IsPlottable,
        ["frozen"] = layer.IsFrozen,
        ["locked"] = layer.IsLocked,
      };
    });
  }
}
