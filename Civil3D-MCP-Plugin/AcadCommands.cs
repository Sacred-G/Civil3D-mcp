using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class AcadCommands
{
  public static Task<object?> CreatePolylineAsync(JsonObject? parameters)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createPolyline requires at least two points.");
    }

    var closed = PluginRuntime.GetOptionalInt(parameters, "closed") == 1;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

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

        var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
        var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
        polyline.AddVertexAt(index, new Point2d(x, y), 0, 0, 0);
      }

      polyline.Closed = closed;

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        polyline.LayerId = layerId;
      }

      var polylineId = modelSpace.AppendEntity(polyline);
      transaction.AddNewlyCreatedDBObject(polyline, true);

      var created = CivilObjectUtils.GetRequiredObject<Polyline>(transaction, polylineId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["vertexCount"] = created.NumberOfVertices,
        ["closed"] = created.Closed,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> CreateTextAsync(JsonObject? parameters)
  {
    var text = PluginRuntime.GetRequiredString(parameters, "text");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var height = PluginRuntime.GetOptionalDouble(parameters, "height") ?? 2.5d;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var dbText = new DBText
      {
        TextString = text,
        Position = new Point3d(x, y, z),
        Height = height,
        Rotation = rotation,
      };

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        dbText.LayerId = layerId;
      }

      var textId = modelSpace.AppendEntity(dbText);
      transaction.AddNewlyCreatedDBObject(dbText, true);

      var created = CivilObjectUtils.GetRequiredObject<DBText>(transaction, textId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["text"] = created.TextString,
        ["x"] = created.Position.X,
        ["y"] = created.Position.Y,
        ["z"] = created.Position.Z,
        ["height"] = created.Height,
        ["rotation"] = created.Rotation,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> Create3dPolylineAsync(JsonObject? parameters)
  {
    var pointsNode = PluginRuntime.GetParameter(parameters, "points") as JsonArray;
    if (pointsNode == null || pointsNode.Count < 2)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "create3dPolyline requires at least two points.");
    }

    var closed = PluginRuntime.GetOptionalInt(parameters, "closed") == 1;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      var pointCollection = new Point3dCollection();
      for (var index = 0; index < pointsNode.Count; index++)
      {
        if (pointsNode[index] is not JsonObject point)
        {
          continue;
        }

        var x = point["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing x.");
        var y = point["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing y.");
        var z = point["z"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Point is missing z.");
        pointCollection.Add(new Point3d(x, y, z));
      }

      if (pointCollection.Count < 2)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "create3dPolyline requires at least two valid points.");
      }

      using var polyline3d = new Polyline3d(Poly3dType.SimplePoly, pointCollection, closed);

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        polyline3d.LayerId = layerId;
      }

      var polylineId = modelSpace.AppendEntity(polyline3d);
      transaction.AddNewlyCreatedDBObject(polyline3d, true);

      var created = CivilObjectUtils.GetRequiredObject<Polyline3d>(transaction, polylineId, OpenMode.ForRead);
      var vertexCount = 0;
      foreach (ObjectId vertexId in created)
      {
        _ = vertexId;
        vertexCount++;
      }

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["vertexCount"] = vertexCount,
        ["closed"] = created.Closed,
        ["layer"] = created.Layer,
      };
    });
  }

  public static Task<object?> CreateMTextAsync(JsonObject? parameters)
  {
    var text = PluginRuntime.GetRequiredString(parameters, "text");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0d;
    var width = PluginRuntime.GetOptionalDouble(parameters, "width") ?? 0d;
    var textHeight = PluginRuntime.GetOptionalDouble(parameters, "textHeight") ?? 2.5d;
    var rotation = PluginRuntime.GetOptionalDouble(parameters, "rotation") ?? 0d;
    var layerName = PluginRuntime.GetOptionalString(parameters, "layer");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

      using var mtext = new MText
      {
        Contents = text,
        Location = new Point3d(x, y, z),
        TextHeight = textHeight,
        Rotation = rotation,
      };

      if (width > 0)
      {
        mtext.Width = width;
      }

      if (!string.IsNullOrWhiteSpace(layerName))
      {
        var layerId = LookupUtils.GetLayerId(database, transaction, layerName);
        mtext.LayerId = layerId;
      }

      var mtextId = modelSpace.AppendEntity(mtext);
      transaction.AddNewlyCreatedDBObject(mtext, true);

      var created = CivilObjectUtils.GetRequiredObject<MText>(transaction, mtextId, OpenMode.ForRead);

      return new Dictionary<string, object?>
      {
        ["handle"] = CivilObjectUtils.GetHandle(created),
        ["text"] = created.Contents,
        ["x"] = created.Location.X,
        ["y"] = created.Location.Y,
        ["z"] = created.Location.Z,
        ["textHeight"] = created.TextHeight,
        ["rotation"] = created.Rotation,
        ["width"] = created.Width,
        ["layer"] = created.Layer,
      };
    });
  }
}
