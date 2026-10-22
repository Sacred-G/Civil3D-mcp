using System.Linq;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

// Creates, copies, renames, deletes and lists AutoCAD paper-space layouts. This is plain drawing
// structure (not a Civil3D object), added specifically so a firm's title-block sheet (border, logo,
// revision table) can be duplicated for a new sheet (e.g. C-300 -> C-301) in one fast, in-process
// call. Core Console (LAYOUT COPY / LAYOUT TEMPLATE via a .scr) was tried first and abandoned: once a
// drawing holds more than a couple of Civil3D custom objects (surfaces, profile views with many
// labels, pipe/pressure networks), accoreconsole's headless regen of those objects made even a plain
// layout duplication take many minutes or hang outright. This handler never shells out to Core
// Console; it uses LayoutManager + Database.DeepCloneObjects directly against the live session.
public static class LayoutCommands
{
  public static Task<object?> ListLayoutsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var layouts = new List<Dictionary<string, object?>>();
      foreach (var entry in EnumerateLayouts(database, transaction))
      {
        layouts.Add(new Dictionary<string, object?>
        {
          ["name"] = entry.Layout.LayoutName,
          ["tabOrder"] = entry.Layout.TabOrder,
          ["isModelSpace"] = entry.Layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase),
          ["handle"] = entry.Layout.Handle.ToString(),
          ["isCurrent"] = entry.Layout.LayoutName.Equals(LayoutManager.Current.CurrentLayout, StringComparison.OrdinalIgnoreCase),
        });
      }

      layouts.Sort((a, b) => Convert.ToInt32(a["tabOrder"]).CompareTo(Convert.ToInt32(b["tabOrder"])));
      return new Dictionary<string, object?> { ["layouts"] = layouts };
    });
  }

  public static Task<object?> CreateLayoutAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var doc = App.DocumentManager.MdiActiveDocument
        ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      var database = doc.Database;
      using var documentLock = doc.LockDocument();

      using (var tr = database.TransactionManager.StartTransaction())
      {
        if (FindLayout(database, tr, name).Layout != null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.CONFLICT", $"A layout named '{name}' already exists. Use acad_layout list to see the layouts in this drawing.");
        }
        tr.Commit();
      }

      var newLayoutId = LayoutManager.Current.CreateLayout(name);

      string handle;
      using (var tr = database.TransactionManager.StartTransaction())
      {
        var layout = CivilObjectUtils.GetRequiredObject<Layout>(tr, newLayoutId, OpenMode.ForRead);
        handle = layout.Handle.ToString();
        tr.Commit();
      }

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["name"] = name,
        ["handle"] = handle,
        ["created"] = true,
      });
    });
  }

  public static Task<object?> CopyLayoutAsync(JsonObject? parameters)
  {
    var sourceLayoutName = PluginRuntime.GetRequiredString(parameters, "sourceLayoutName");
    var newName = PluginRuntime.GetRequiredString(parameters, "newName");
    var setCurrent = PluginRuntime.GetOptionalBool(parameters, "setCurrent") ?? true;

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var doc = App.DocumentManager.MdiActiveDocument
        ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      var database = doc.Database;
      using var documentLock = doc.LockDocument();

      ObjectId sourceLayoutId;
      ObjectId sourceBtrId;
      using (var tr = database.TransactionManager.StartTransaction())
      {
        var (sourceLayout, sourceLayoutIdFound, sourceBtrIdFound) = FindLayout(database, tr, sourceLayoutName);
        if (sourceLayout == null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
            $"Layout '{sourceLayoutName}' was not found. Available layouts: {DescribeLayouts(database, tr)}.");
        }
        if (FindLayout(database, tr, newName).Layout != null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.CONFLICT", $"A layout named '{newName}' already exists. Choose a different name or delete it first.");
        }
        sourceLayoutId = sourceLayoutIdFound;
        sourceBtrId = sourceBtrIdFound;
        tr.Commit();
      }

      var newLayoutId = LayoutManager.Current.CreateLayout(newName);

      int entitiesCopied;
      var cleanup = new Dictionary<string, object?>();
      using (var tr = database.TransactionManager.StartTransaction())
      {
        var sourceLayout = CivilObjectUtils.GetRequiredObject<Layout>(tr, sourceLayoutId, OpenMode.ForRead);
        var sourceBtr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(tr, sourceBtrId, OpenMode.ForRead);
        var newLayout = CivilObjectUtils.GetRequiredObject<Layout>(tr, newLayoutId, OpenMode.ForWrite);

        try
        {
          // Page setup only (paper size, plot device/style, plot area): best-effort, never fatal.
          // The title block and border below are what actually has to survive.
          newLayout.CopyFrom(sourceLayout);
        }
        catch
        {
          // ignored: an explicit -PLOT call can always set paper size/plotter later.
        }

        var sourceIds = new ObjectIdCollection();
        foreach (ObjectId id in sourceBtr)
        {
          sourceIds.Add(id);
        }

        entitiesCopied = sourceIds.Count;
        if (sourceIds.Count > 0)
        {
          var idMap = new IdMapping();
          database.DeepCloneObjects(sourceIds, newLayout.BlockTableRecordId, idMap, false);
          var cloned = new List<ObjectId>();
          foreach (IdPair pair in idMap)
          {
            if (pair.IsCloned && pair.IsPrimary)
            {
              cloned.Add(pair.Value);
            }
          }

          cleanup = CleanClonedSheet(tr, cloned, parameters);
        }

        tr.Commit();
      }

      if (setCurrent)
      {
        LayoutManager.Current.CurrentLayout = newName;
      }

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["sourceLayoutName"] = sourceLayoutName,
        ["newName"] = newName,
        ["entitiesCopied"] = entitiesCopied,
        ["cleanup"] = cleanup,
        ["created"] = true,
      });
    });
  }

  // Post-clone edits for "same title block, different sheet" (C-300 -> C-301) so the whole sheet is one call:
  //  excludeLayers / excludeBlockNames / excludeTextContaining / excludeWindow {x1,y1,x2,y2} (paper units, entity centre) erase
  //  clones the new sheet must not carry (plan viewport on VPORT, north arrow, graphic scale, long notes);
  //  replaceText [{find,replace}] edits DBText, MText and block attributes (sheet number, title, "1 OF 3" -> "3 OF 3").
  // Never erased: a viewport on layer "0" — the layout's own sheet viewport; without it the plot is blank.
  private static Dictionary<string, object?> CleanClonedSheet(Transaction tr, List<ObjectId> cloned, JsonObject? parameters)
  {
    var layers = ReadStrings(parameters, "excludeLayers");
    var blocks = ReadStrings(parameters, "excludeBlockNames");
    var texts = ReadStrings(parameters, "excludeTextContaining");
    var window = PluginRuntime.GetParameter(parameters, "excludeWindow") as JsonObject;
    var replacements = new List<(string Find, string Replace)>();
    if (PluginRuntime.GetParameter(parameters, "replaceText") is JsonArray replaceArray)
    {
      foreach (var node in replaceArray.OfType<JsonObject>())
      {
        var find = PluginRuntime.GetRequiredString(node, "find");
        replacements.Add((find, PluginRuntime.GetOptionalString(node, "replace") ?? string.Empty));
      }
    }

    var report = new Dictionary<string, object?>();
    if (layers.Count == 0 && blocks.Count == 0 && texts.Count == 0 && window == null && replacements.Count == 0)
    {
      return report;
    }

    double? x1 = null, y1 = null, x2 = null, y2 = null;
    if (window != null)
    {
      x1 = Math.Min(PluginRuntime.GetRequiredDouble(window, "x1"), PluginRuntime.GetRequiredDouble(window, "x2"));
      x2 = Math.Max(PluginRuntime.GetRequiredDouble(window, "x1"), PluginRuntime.GetRequiredDouble(window, "x2"));
      y1 = Math.Min(PluginRuntime.GetRequiredDouble(window, "y1"), PluginRuntime.GetRequiredDouble(window, "y2"));
      y2 = Math.Max(PluginRuntime.GetRequiredDouble(window, "y1"), PluginRuntime.GetRequiredDouble(window, "y2"));
    }

    int byLayer = 0, byBlock = 0, byText = 0, byWindow = 0, keptViewports = 0;
    var replaced = replacements.ToDictionary(r => r.Find, _ => 0);
    foreach (var id in cloned)
    {
      if (id.IsErased || tr.GetObject(id, OpenMode.ForWrite) is not Entity entity)
      {
        continue;
      }

      if (entity is Viewport && entity.Layer == "0")
      {
        keptViewports++;
        continue;
      }

      string? reason = null;
      if (layers.Contains(entity.Layer))
      {
        reason = "layer";
      }
      else if (entity is BlockReference br && blocks.Contains(BlockName(tr, br)))
      {
        reason = "block";
      }
      else if (texts.Count > 0 && EntityText(entity) is { } content
        && texts.Any(t => content.Contains(t, StringComparison.OrdinalIgnoreCase)))
      {
        reason = "text";
      }
      else if (window != null && CenterInside(entity, x1!.Value, y1!.Value, x2!.Value, y2!.Value))
      {
        reason = "window";
      }

      if (reason != null)
      {
        entity.Erase();
        switch (reason)
        {
          case "layer": byLayer++; break;
          case "block": byBlock++; break;
          case "text": byText++; break;
          default: byWindow++; break;
        }

        continue;
      }

      foreach (var (find, replace) in replacements)
      {
        replaced[find] += ReplaceIn(tr, entity, find, replace);
      }
    }

    report["erasedByLayer"] = byLayer;
    report["erasedByBlock"] = byBlock;
    report["erasedByText"] = byText;
    report["erasedByWindow"] = byWindow;
    report["sheetViewportsKept"] = keptViewports;
    report["replaced"] = replaced.Select(pair => new Dictionary<string, object?> { ["find"] = pair.Key, ["entities"] = pair.Value }).ToList();
    return report;
  }

  private static HashSet<string> ReadStrings(JsonObject? parameters, string name)
  {
    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (PluginRuntime.GetParameter(parameters, name) is JsonArray array)
    {
      foreach (var node in array)
      {
        if (node?.GetValue<string>() is { Length: > 0 } value)
        {
          set.Add(value);
        }
      }
    }

    return set;
  }

  private static string BlockName(Transaction tr, BlockReference br)
  {
    var record = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(tr, br.DynamicBlockTableRecord, OpenMode.ForRead);
    return record.Name;
  }

  private static string? EntityText(Entity entity) => entity switch
  {
    DBText text => text.TextString,
    MText mtext => mtext.Contents,
    _ => null,
  };

  private static bool CenterInside(Entity entity, double x1, double y1, double x2, double y2)
  {
    try
    {
      var extents = entity.GeometricExtents;
      var cx = (extents.MinPoint.X + extents.MaxPoint.X) / 2;
      var cy = (extents.MinPoint.Y + extents.MaxPoint.Y) / 2;
      return cx >= x1 && cx <= x2 && cy >= y1 && cy <= y2;
    }
    catch (Autodesk.AutoCAD.Runtime.Exception)
    {
      return false;
    }
  }

  private static int ReplaceIn(Transaction tr, Entity entity, string find, string replace)
  {
    switch (entity)
    {
      case DBText text when text.TextString.Contains(find, StringComparison.Ordinal):
        text.TextString = text.TextString.Replace(find, replace, StringComparison.Ordinal);
        return 1;
      case MText mtext when mtext.Contents.Contains(find, StringComparison.Ordinal):
        mtext.Contents = mtext.Contents.Replace(find, replace, StringComparison.Ordinal);
        return 1;
      case BlockReference br:
        var changed = 0;
        foreach (ObjectId attributeId in br.AttributeCollection)
        {
          if (tr.GetObject(attributeId, OpenMode.ForWrite) is AttributeReference attribute && attribute.TextString.Contains(find, StringComparison.Ordinal))
          {
            attribute.TextString = attribute.TextString.Replace(find, replace, StringComparison.Ordinal);
            changed++;
          }
        }

        return changed > 0 ? 1 : 0;
      default:
        return 0;
    }
  }

  // Sets each plugin-made viewport's annotation scale from its custom scale (`1" = 20'` for 0.05) — the retroactive fix for
  // viewports created by an older build (they show `1" = 1'` in acad_list_viewports and hide annotative dims/MLeaders in the plot).
  // Skips each layout's own sheet viewport. layout omitted = every layout; viewportHandle narrows to one viewport.
  public static Task<object?> SetViewportAnnotationScaleAsync(JsonObject? parameters)
  {
    var layoutFilter = PluginRuntime.GetOptionalString(parameters, "layout");
    var handleFilter = PluginRuntime.GetOptionalString(parameters, "viewportHandle");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var changed = new List<Dictionary<string, object?>>();
      var unchanged = 0;
      foreach (var (layout, _) in EnumerateLayouts(database, transaction))
      {
        if (layout.LayoutName.Equals("Model", StringComparison.OrdinalIgnoreCase)
          || (!string.IsNullOrWhiteSpace(layoutFilter) && !layout.LayoutName.Equals(layoutFilter, StringComparison.OrdinalIgnoreCase)))
        {
          continue;
        }

        var btr = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, layout.BlockTableRecordId, OpenMode.ForRead);
        foreach (ObjectId id in btr)
        {
          if (transaction.GetObject(id, OpenMode.ForRead) is not Viewport viewport
            || DimensionViewportCommands.IsPaperSpaceViewport(viewport)
            || viewport.Layer == "0"   // a cloned sheet viewport is not detected as paper-space one; layer 0 is never a plan viewport
            || (!string.IsNullOrWhiteSpace(handleFilter) && !viewport.Handle.ToString().Equals(handleFilter, StringComparison.OrdinalIgnoreCase)))
          {
            continue;
          }

          var before = viewport.AnnotationScale?.Name;
          viewport.UpgradeOpen();
          var wasLocked = viewport.Locked;
          if (wasLocked)
          {
            viewport.Locked = false;
          }

          var after = DraftingBatchCommands.TryApplyViewportAnnotationScale(database, viewport);
          if (wasLocked)
          {
            viewport.Locked = true;
          }

          if (after != null && after != before)
          {
            changed.Add(new Dictionary<string, object?> { ["layout"] = layout.LayoutName, ["handle"] = viewport.Handle.ToString(), ["before"] = before, ["after"] = after });
          }
          else
          {
            unchanged++;
          }
        }
      }

      return new Dictionary<string, object?> { ["changed"] = changed, ["unchanged"] = unchanged };
    });
  }

  public static Task<object?> RenameLayoutAsync(JsonObject? parameters)
  {
    var oldName = PluginRuntime.GetRequiredString(parameters, "oldName");
    var newName = PluginRuntime.GetRequiredString(parameters, "newName");

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var doc = App.DocumentManager.MdiActiveDocument
        ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      using var documentLock = doc.LockDocument();

      using (var tr = doc.Database.TransactionManager.StartTransaction())
      {
        if (FindLayout(doc.Database, tr, oldName).Layout == null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
            $"Layout '{oldName}' was not found. Available layouts: {DescribeLayouts(doc.Database, tr)}.");
        }
        if (FindLayout(doc.Database, tr, newName).Layout != null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.CONFLICT", $"A layout named '{newName}' already exists.");
        }
        tr.Commit();
      }

      LayoutManager.Current.RenameLayout(oldName, newName);

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["oldName"] = oldName,
        ["newName"] = newName,
        ["renamed"] = true,
      });
    });
  }

  public static Task<object?> DeleteLayoutAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var doc = App.DocumentManager.MdiActiveDocument
        ?? throw new JsonRpcDispatchException("CIVIL3D.NO_DRAWING", "No active drawing is open in Civil 3D.");
      using var documentLock = doc.LockDocument();

      if (name.Equals("Model", StringComparison.OrdinalIgnoreCase))
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "The Model layout cannot be deleted.");
      }

      using (var tr = doc.Database.TransactionManager.StartTransaction())
      {
        if (FindLayout(doc.Database, tr, name).Layout == null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
            $"Layout '{name}' was not found. Available layouts: {DescribeLayouts(doc.Database, tr)}.");
        }
        tr.Commit();
      }

      LayoutManager.Current.DeleteLayout(name);

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["name"] = name,
        ["deleted"] = true,
      });
    });
  }

  private static IEnumerable<(Layout Layout, ObjectId LayoutId)> EnumerateLayouts(Database database, Transaction transaction)
  {
    var layoutDict = CivilObjectUtils.GetRequiredObject<DBDictionary>(transaction, database.LayoutDictionaryId, OpenMode.ForRead);
    foreach (DBDictionaryEntry entry in layoutDict)
    {
      yield return (CivilObjectUtils.GetRequiredObject<Layout>(transaction, entry.Value, OpenMode.ForRead), entry.Value);
    }
  }

  private static (Layout? Layout, ObjectId LayoutId, ObjectId BlockTableRecordId) FindLayout(Database database, Transaction transaction, string name)
  {
    foreach (var (layout, layoutId) in EnumerateLayouts(database, transaction))
    {
      if (layout.LayoutName.Equals(name, StringComparison.OrdinalIgnoreCase))
      {
        return (layout, layoutId, layout.BlockTableRecordId);
      }
    }

    return (null, ObjectId.Null, ObjectId.Null);
  }

  private static string DescribeLayouts(Database database, Transaction transaction)
  {
    var names = EnumerateLayouts(database, transaction).Select(entry => entry.Layout.LayoutName);
    return string.Join(", ", names);
  }
}
