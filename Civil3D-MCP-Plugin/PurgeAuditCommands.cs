using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public static class PurgeAuditCommands
{
  public static Task<object?> PurgeUnusedAsync(JsonObject? parameters)
  {
    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var candidates = new ObjectIdCollection();

      AddNamedRecords<LayerTableRecord>(transaction, database.LayerTableId, candidates);
      AddNamedRecords<LinetypeTableRecord>(transaction, database.LinetypeTableId, candidates);
      AddNamedRecords<TextStyleTableRecord>(transaction, database.TextStyleTableId, candidates);
      AddNamedRecords<DimStyleTableRecord>(transaction, database.DimStyleTableId, candidates);
      AddNamedRecords<RegAppTableRecord>(transaction, database.RegAppTableId, candidates);

      var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
      foreach (ObjectId blockId in blockTable)
      {
        var btr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockId, OpenMode.ForRead);
        if (btr.IsLayout || btr.IsAnonymous || btr.IsFromExternalReference || btr.IsFromOverlayReference)
        {
          continue;
        }

        candidates.Add(blockId);
      }

      var purgedCount = candidates.Count;
      database.Purge(candidates);
      purgedCount = candidates.Count;

      var erased = new List<string>();
      foreach (ObjectId id in candidates)
      {
        var obj = transaction.GetObject(id, OpenMode.ForWrite);
        erased.Add(obj.GetType().Name);
        obj.Erase(true);
      }

      return new Dictionary<string, object?>
      {
        ["purgedCount"] = purgedCount,
        ["purgedTypes"] = erased,
      };
    });
  }

  private static void AddNamedRecords<T>(Transaction transaction, ObjectId tableId, ObjectIdCollection candidates) where T : SymbolTableRecord
  {
    if (transaction.GetObject(tableId, OpenMode.ForRead) is not SymbolTable table)
    {
      return;
    }

    foreach (ObjectId id in table)
    {
      candidates.Add(id);
    }
  }

  public static Task<object?> AuditDrawingAsync(JsonObject? parameters)
  {
    var fixErrors = PluginRuntime.GetOptionalBool(parameters, "fixErrors") ?? true;

    return CivilExecution.ExecuteInCommandContextAsync(async () =>
    {
      var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument
        ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");

      using var documentLock = doc.LockDocument();
      try
      {
        doc.Editor.Command("_.AUDIT", fixErrors ? "_Y" : "_N");
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"AUDIT failed: {ex.Message}");
      }

      await Task.CompletedTask;

      return (object?)new Dictionary<string, object?>
      {
        ["audited"] = true,
        ["fixErrors"] = fixErrors,
      };
    });
  }
}
