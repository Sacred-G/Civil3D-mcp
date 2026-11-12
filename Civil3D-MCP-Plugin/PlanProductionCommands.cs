using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for Plan Production / Sheets MCP tools.
///
/// Civil 3D API notes (reflection-based late binding):
///   AeccSheetSet      -- collection of sheets for a plan production workflow
///   AeccSheet         -- individual sheet (layout reference + viewport metadata)
///   AeccPlanProductionHelper -- static factory for plan/profile sheet creation
///
/// The API surface lives in AeccDbMgd.dll. We access it via reflection so the
/// plugin builds without a direct assembly reference and tolerates minor
/// version differences between Civil 3D releases.
///
/// Sheet sets are stored as named objects in the Civil document's
/// database or as a collection on the CivilDocument. Common property/method
/// names tried: SheetSets, SheetSetCollection, GetSheetSetIds(), PlanProductionSheetSets.
///
/// PDF publishing uses Autodesk.AutoCAD.PlottingServices (PlotInfo, PlotEngine,
/// PlotSettings) which is available in all AutoCAD-platform products.
/// </summary>
public static class PlanProductionCommands
{
  // -------------------------------------------------------------------------
  // listSheetSets
  // -------------------------------------------------------------------------

  public static Task<object?> ListSheetSetsAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSets = EnumerateSheetSets(civilDoc, transaction)
        .Select(ss => ToSheetSetSummary(ss, transaction))
        .ToList();

      return new Dictionary<string, object?> { ["sheetSets"] = sheetSets };
    });
  }

  // -------------------------------------------------------------------------
  // getSheetSetInfo
  // -------------------------------------------------------------------------

  public static Task<object?> GetSheetSetInfoAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, name);

      var sheets = GetSheetIds(sheetSet, transaction)
        .Select(id => ToSheetSummary(transaction.GetObject(id, OpenMode.ForRead)))
        .ToList();

      return new Dictionary<string, object?>
      {
        ["name"] = GetName(sheetSet) ?? name,
        ["handle"] = GetHandleString(sheetSet),
        ["description"] = GetAnyString(sheetSet, "Description", "Desc"),
        ["sheets"] = sheets,
      };
    });
  }

  // -------------------------------------------------------------------------
  // createSheetSet
  // -------------------------------------------------------------------------

  public static Task<object?> CreateSheetSetAsync(JsonObject? parameters)
  {
    _ = PluginRuntime.GetRequiredString(parameters, "name");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "Civil 3D 2026 does not document a managed sheet-set creation API. No sheet set was created; use Sheet Set Manager or a reviewed Autodesk Sheet Set API integration.");
  }

  // -------------------------------------------------------------------------
  // addSheet
  // -------------------------------------------------------------------------

  public static Task<object?> AddSheetAsync(JsonObject? parameters)
  {
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var sheetName = PluginRuntime.GetRequiredString(parameters, "sheetName");
    var sheetNumber = PluginRuntime.GetOptionalString(parameters, "sheetNumber");
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layoutName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var sheet = AddSheetToSet(sheetSet, transaction, sheetName, sheetNumber ?? "1", layoutName);

      return new Dictionary<string, object?>
      {
        ["name"] = GetName(sheet) ?? sheetName,
        ["number"] = sheetNumber ?? "1",
        ["handle"] = GetHandleString(sheet),
        ["added"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getSheetProperties
  // -------------------------------------------------------------------------

  public static Task<object?> GetSheetPropertiesAsync(JsonObject? parameters)
  {
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var sheetName = PluginRuntime.GetRequiredString(parameters, "sheetName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var sheet = FindSheetByName(sheetSet, transaction, sheetName);

      return ToSheetDetail(sheet, transaction, doc.Database);
    });
  }

  // -------------------------------------------------------------------------
  // setSheetTitleBlock
  // -------------------------------------------------------------------------

  public static Task<object?> SetSheetTitleBlockAsync(JsonObject? parameters)
  {
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var sheetName = PluginRuntime.GetRequiredString(parameters, "sheetName");
    var titleBlockPath = FileBoundary.ResolveImportPath(
      PluginRuntime.GetRequiredString(parameters, "titleBlockPath"),
      ".dwg", ".dwt");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var sheet = FindSheetByName(sheetSet, transaction, sheetName);

      // Try setting TitleBlockPath, TitleBlock, TemplatePath on the sheet
      if (!TrySetStringProperty(sheet, titleBlockPath, "TitleBlockPath", "TitleBlock", "TemplatePath", "BlockPath"))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Sheet '{sheetName}' does not expose a writable title-block property. No update was made.");
      }

      return new Dictionary<string, object?>
      {
        ["sheetName"] = sheetName,
        ["titleBlock"] = titleBlockPath,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // createPlanProfileSheet
  // -------------------------------------------------------------------------

  public static Task<object?> CreatePlanProfileSheetAsync(JsonObject? parameters)
  {
    _ = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    _ = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "Civil 3D 2026 does not document a managed plan/profile sheet factory. No sheet, view frame, or placeholder layout was created.");
  }

  // -------------------------------------------------------------------------
  // updatePlanProfileSheetAlignment
  // -------------------------------------------------------------------------

  public static Task<object?> UpdatePlanProfileSheetAlignmentAsync(JsonObject? parameters)
  {
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var sheetName = PluginRuntime.GetRequiredString(parameters, "sheetName");
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetOptionalString(parameters, "profileName");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var sheet = FindSheetByName(sheetSet, transaction, sheetName);
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);

      // Open sheet for write if it is a DBObject
      if (sheet is DBObject dbObj)
      {
        dbObj.UpgradeOpen();
        TrySetObjectIdPropertyOnObj(dbObj, alignment.ObjectId, "AlignmentId", "ReferenceAlignmentId");

        if (!string.IsNullOrWhiteSpace(profileName))
        {
          var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName!, OpenMode.ForRead);
          TrySetObjectIdPropertyOnObj(dbObj, profile.ObjectId, "ProfileId", "ReferenceProfileId");
        }
      }
      else
      {
        TrySetObjectIdPropertyOnObj(sheet, alignment.ObjectId, "AlignmentId", "ReferenceAlignmentId");
      }

      return new Dictionary<string, object?>
      {
        ["sheetName"] = sheetName,
        ["alignmentName"] = alignmentName,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // createSheetView
  // -------------------------------------------------------------------------

  public static Task<object?> CreateSheetViewAsync(JsonObject? parameters)
  {
    var layoutName = PluginRuntime.GetRequiredString(parameters, "layoutName");
    var viewName = PluginRuntime.GetOptionalString(parameters, "viewName");
    var centerX = PluginRuntime.GetOptionalDouble(parameters, "centerX") ?? 0.0;
    var centerY = PluginRuntime.GetOptionalDouble(parameters, "centerY") ?? 0.0;
    var width = PluginRuntime.GetOptionalDouble(parameters, "width") ?? 8.0;
    var height = PluginRuntime.GetOptionalDouble(parameters, "height") ?? 6.0;
    var scale = PluginRuntime.GetOptionalDouble(parameters, "scale");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var layout = FindLayoutByName(database, transaction, layoutName);

      // Create viewport in paper space
      var viewport = new Viewport
      {
        CenterPoint = new Point3d(centerX, centerY, 0),
        Width = width,
        Height = height,
      };

      if (scale.HasValue)
      {
        viewport.CustomScale = 1.0 / scale.Value;
      }

      var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
      var layoutBlock = (BlockTableRecord)transaction.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite);

      layoutBlock.AppendEntity(viewport);
      transaction.AddNewlyCreatedDBObject(viewport, true);
      viewport.On = true;

      // If a named view is requested, set it on the viewport
      if (!string.IsNullOrWhiteSpace(viewName))
      {
        TryApplyNamedViewToViewport(database, transaction, viewport, viewName!);
      }

      return new Dictionary<string, object?>
      {
        ["handle"] = viewport.Handle.ToString(),
        ["layoutName"] = layoutName,
        ["scale"] = scale,
        ["created"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // setSheetViewScale
  // -------------------------------------------------------------------------

  public static Task<object?> SetSheetViewScaleAsync(JsonObject? parameters)
  {
    var layoutName = PluginRuntime.GetRequiredString(parameters, "layoutName");
    var viewportHandle = PluginRuntime.GetOptionalString(parameters, "viewportHandle");
    var scale = PluginRuntime.GetRequiredDouble(parameters, "scale");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var layout = FindLayoutByName(database, transaction, layoutName);
      var viewport = FindViewport(database, transaction, layout, viewportHandle);

      viewport.UpgradeOpen();
      viewport.CustomScale = 1.0 / scale;
      viewport.StandardScale = StandardScaleType.CustomScale;

      return new Dictionary<string, object?>
      {
        ["handle"] = viewport.Handle.ToString(),
        ["scale"] = scale,
        ["updated"] = true,
      };
    });
  }

  // -------------------------------------------------------------------------
  // publishSheetPdf
  // -------------------------------------------------------------------------

  public static Task<object?> PublishSheetPdfAsync(JsonObject? parameters)
  {
    var layoutNamesNode = parameters?["layoutNames"] as JsonArray;
    if (layoutNamesNode == null || layoutNamesNode.Count == 0)
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Parameter 'layoutNames' must be a non-empty array.");

    _ = FileBoundary.ResolveExportPath(
      PluginRuntime.GetRequiredString(parameters, "outputPath"),
      PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false,
      ".pdf");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "PDF publishing requires a complete AutoCAD PlotEngine transaction and completion verification. The previous asynchronous PUBLISH fallback could report success before output existed, so no publish was started. Use civil3d_plot action=publish_sheet_set (one multi-sheet PDF) or action=plot_layouts_to_pdf (one PDF per layout), which verify the written output.");
  }

  // -------------------------------------------------------------------------
  // exportSheetSet
  // -------------------------------------------------------------------------

  public static Task<object?> ExportSheetSetAsync(JsonObject? parameters)
  {
    _ = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    _ = FileBoundary.ResolveExportPath(
      PluginRuntime.GetRequiredString(parameters, "outputPath"),
      PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false,
      ".pdf", ".dwf", ".dwfx", ".dst");
    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      "Sheet-set PDF export is unavailable until a typed AutoCAD PlotEngine workflow with output verification is implemented. No export was started. To publish the sheets' layouts as one PDF, use civil3d_plot action=publish_sheet_set with their layoutNames.");
  }

  // =========================================================================
  // Private helpers
  // =========================================================================

  private static IEnumerable<DBObject> EnumerateSheetSets(object civilDoc, Transaction transaction)
  {
    // Try multiple property/method names for the sheet set collection
    foreach (var memberName in new[] { "SheetSets", "SheetSetCollection", "PlanProductionSheetSets" })
    {
      var collection = GetNamedMember(civilDoc, memberName);
      if (collection == null) continue;

      foreach (var id in CivilObjectUtils.ToObjectIds(collection))
      {
        if (id != ObjectId.Null)
          yield return transaction.GetObject(id, OpenMode.ForRead);
      }

      foreach (var item in EnumerateObjects(collection))
      {
        if (item is DBObject dbObj) yield return dbObj;
      }
    }

    // Fallback: search the NamedObjectsDictionary for AeccSheetSet entries
    var database = CivilObjectUtils.GetDatabase(civilDoc);
    var nod = (DBDictionary)transaction.GetObject(
      database.NamedObjectsDictionaryId,
      OpenMode.ForRead);

    foreach (DictionaryEntry entry in nod)
    {
      if (entry.Value is ObjectId oid && oid != ObjectId.Null)
      {
        DBObject? obj = null;
        try { obj = transaction.GetObject(oid, OpenMode.ForRead); } catch (Exception ex) { PluginLog.Swallow("PlanProduction", "read dictionary entry", ex); }
        if (obj != null && obj.GetType().Name.Contains("SheetSet"))
          yield return obj;
      }
    }
  }

  private static IEnumerable<DBObject> EnumerateSheetSets(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction)
  {
    foreach (var memberName in new[] { "SheetSets", "SheetSetCollection", "PlanProductionSheetSets" })
    {
      var collection = GetNamedMember(civilDoc, memberName);
      if (collection == null) continue;

      foreach (var id in CivilObjectUtils.ToObjectIds(collection))
      {
        if (id != ObjectId.Null)
          yield return transaction.GetObject(id, OpenMode.ForRead);
      }

      foreach (var item in EnumerateObjects(collection))
      {
        if (item is DBObject dbObj) yield return dbObj;
      }
    }
  }

  private static DBObject FindSheetSetByName(Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, Transaction transaction, string name)
  {
    foreach (var sheetSet in EnumerateSheetSets(civilDoc, transaction))
    {
      if (string.Equals(GetName(sheetSet), name, StringComparison.OrdinalIgnoreCase))
        return sheetSet;
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Sheet set '{name}' was not found.");
  }

  private static IEnumerable<ObjectId> GetSheetIds(object sheetSet, Transaction transaction)
  {
    // Try GetSheetIds method, then Sheets / SheetIds properties
    var result = CivilObjectUtils.InvokeMethod(sheetSet, "GetSheetIds");
    if (result != null)
    {
      foreach (var id in CivilObjectUtils.ToObjectIds(result))
        if (id != ObjectId.Null) yield return id;
      yield break;
    }

    foreach (var memberName in new[] { "Sheets", "SheetIds", "SheetCollection", "GetSheets" })
    {
      var value = GetNamedMember(sheetSet, memberName)
        ?? CivilObjectUtils.InvokeMethod(sheetSet, memberName);
      if (value == null) continue;

      foreach (var id in CivilObjectUtils.ToObjectIds(value))
        if (id != ObjectId.Null) yield return id;

      foreach (var item in EnumerateObjects(value))
      {
        if (item is DBObject dbObj) yield return dbObj.ObjectId;
      }
    }
  }

  private static DBObject FindSheetByName(DBObject sheetSet, Transaction transaction, string name)
  {
    foreach (var id in GetSheetIds(sheetSet, transaction))
    {
      var sheet = transaction.GetObject(id, OpenMode.ForRead);
      if (string.Equals(GetName(sheet), name, StringComparison.OrdinalIgnoreCase))
        return sheet;
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Sheet '{name}' was not found.");
  }

  private static DBObject AddSheetToSet(
    DBObject sheetSet,
    Transaction transaction,
    string sheetName,
    string sheetNumber,
    string? layoutName)
  {
    // Try AddSheet(name) or Add(name) method on the sheet set
    var addResult = CivilObjectUtils.InvokeMethod(sheetSet, "AddSheet", sheetName, sheetNumber)
      ?? CivilObjectUtils.InvokeMethod(sheetSet, "Add", sheetName);

    if (addResult is DBObject addedSheet)
      return addedSheet;

    if (addResult is ObjectId addedId && addedId != ObjectId.Null)
      return transaction.GetObject(addedId, OpenMode.ForRead);

    throw new JsonRpcDispatchException(
      "CIVIL3D.API_ERROR",
      $"Civil 3D could not add sheet '{sheetName}' to the sheet set. No simulated sheet was created.");
  }

  private static Dictionary<string, object?> ToSheetSetSummary(DBObject sheetSet, Transaction transaction)
  {
    var sheetCount = GetSheetIds(sheetSet, transaction).Count();
    return new Dictionary<string, object?>
    {
      ["name"] = GetName(sheetSet),
      ["handle"] = GetHandleString(sheetSet),
      ["description"] = GetAnyString(sheetSet, "Description", "Desc"),
      ["sheetCount"] = sheetCount,
    };
  }

  private static Dictionary<string, object?> ToSheetSummary(DBObject sheet)
  {
    return new Dictionary<string, object?>
    {
      ["name"] = GetName(sheet),
      ["number"] = GetAnyString(sheet, "Number", "SheetNumber") ?? "",
      ["handle"] = GetHandleString(sheet),
      ["layoutName"] = GetAnyString(sheet, "LayoutName", "Layout"),
    };
  }

  private static Dictionary<string, object?> ToSheetDetail(DBObject sheet, Transaction transaction, Autodesk.AutoCAD.DatabaseServices.Database database)
  {
    double? viewportScale = null;
    var scaleVal = CivilObjectUtils.GetPropertyValue<double?>(sheet, "ViewportScale")
      ?? CivilObjectUtils.GetPropertyValue<double?>(sheet, "Scale");
    if (scaleVal.HasValue && scaleVal.Value > 0) viewportScale = scaleVal.Value;

    string? alignmentName = null;
    var alignmentId = GetFirstObjectId(sheet, "AlignmentId", "ReferenceAlignmentId");
    if (alignmentId != ObjectId.Null)
    {
      try
      {
        var obj = transaction.GetObject(alignmentId, OpenMode.ForRead);
        alignmentName = CivilObjectUtils.GetName(obj);
      }
      catch (Exception ex) { PluginLog.Swallow("PlanProduction", "resolve alignment name", ex); }
    }

    string? profileName = null;
    var profileId = GetFirstObjectId(sheet, "ProfileId", "ReferenceProfileId");
    if (profileId != ObjectId.Null)
    {
      try
      {
        var obj = transaction.GetObject(profileId, OpenMode.ForRead);
        profileName = CivilObjectUtils.GetName(obj);
      }
      catch (Exception ex) { PluginLog.Swallow("PlanProduction", "resolve profile name", ex); }
    }

    return new Dictionary<string, object?>
    {
      ["name"] = GetName(sheet),
      ["number"] = GetAnyString(sheet, "Number", "SheetNumber") ?? "",
      ["handle"] = GetHandleString(sheet),
      ["layoutName"] = GetAnyString(sheet, "LayoutName", "Layout"),
      ["viewportScale"] = viewportScale,
      ["alignmentName"] = alignmentName,
      ["profileName"] = profileName,
      ["titleBlock"] = GetAnyString(sheet, "TitleBlockPath", "TitleBlock", "TemplatePath"),
    };
  }

  private static Layout FindLayoutByName(Autodesk.AutoCAD.DatabaseServices.Database database, Transaction transaction, string layoutName)
  {
    var layoutDict = (DBDictionary)transaction.GetObject(database.LayoutDictionaryId, OpenMode.ForRead);
    foreach (DictionaryEntry entry in layoutDict)
    {
      var name = entry.Key as string;
      if (string.Equals(name, layoutName, StringComparison.OrdinalIgnoreCase))
      {
        if (entry.Value is ObjectId oid)
          return (Layout)transaction.GetObject(oid, OpenMode.ForRead);
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layout '{layoutName}' was not found.");
  }

  private static Viewport FindViewport(
    Autodesk.AutoCAD.DatabaseServices.Database database,
    Transaction transaction,
    Layout layout,
    string? viewportHandle)
  {
    var layoutBlock = (BlockTableRecord)transaction.GetObject(layout.BlockTableRecordId, OpenMode.ForRead);

    Viewport? first = null;
    foreach (ObjectId id in layoutBlock)
    {
      var obj = transaction.GetObject(id, OpenMode.ForRead);
      if (obj is Viewport vp)
      {
        if (!string.IsNullOrWhiteSpace(viewportHandle) &&
            string.Equals(vp.Handle.ToString(), viewportHandle, StringComparison.OrdinalIgnoreCase))
          return vp;
        first ??= vp;
      }
    }

    if (first != null) return first;
    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No viewport found in layout '{layout.LayoutName}'.");
  }

  private static void TryApplyNamedViewToViewport(
    Autodesk.AutoCAD.DatabaseServices.Database database,
    Transaction transaction,
    Viewport viewport,
    string viewName)
  {
    var viewTable = (ViewTable)transaction.GetObject(database.ViewTableId, OpenMode.ForRead);
    if (!viewTable.Has(viewName)) return;

    var viewRecord = (ViewTableRecord)transaction.GetObject(viewTable[viewName], OpenMode.ForRead);
    viewport.ViewCenter = new Point2d(viewRecord.CenterPoint.X, viewRecord.CenterPoint.Y);
    viewport.ViewHeight = viewRecord.Height;
  }

  private static string BuildDsdContent(
    Autodesk.AutoCAD.DatabaseServices.Database database,
    Transaction transaction,
    IList<string> layoutNames,
    string outputPath,
    string? plotStyleTable)
  {
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("[DWF6Sheet:]");

    int i = 1;
    foreach (var layoutName in layoutNames)
    {
      try
      {
        var layout = FindLayoutByName(database, transaction, layoutName);
        sb.AppendLine($"[DWF6Sheet:{layoutName}]");
        sb.AppendLine($"DWG={database.Filename}");
        sb.AppendLine($"Layout={layoutName}");
        sb.AppendLine($"Setup=");
        sb.AppendLine($"OriginalSheetPath={database.Filename}");
        sb.AppendLine($"SheetRecordHandle={layout.Handle}");
        sb.AppendLine($"PlotDevice=DWG To PDF.pc3");
        if (!string.IsNullOrWhiteSpace(plotStyleTable))
          sb.AppendLine($"PlotStyleTable={plotStyleTable}");
        sb.AppendLine($"PlotToFile=1");
        sb.AppendLine($"OutputFile={outputPath}");
        i++;
      }
      catch (Exception ex) { PluginLog.Swallow("PlanProduction", "build DSD layout entry", ex); }
    }

    sb.AppendLine("[Target]");
    sb.AppendLine("Type=6");
    sb.AppendLine($"DWF={outputPath}");

    return sb.ToString();
  }

  // -------------------------------------------------------------------------
  // General reflection helpers (mirror of PressureNetworkCommands pattern)
  // -------------------------------------------------------------------------

  private static string? GetName(object? value) => CivilObjectUtils.GetName(value);

  private static string GetHandleString(object? value)
  {
    if (value is DBObject dbObj) return dbObj.Handle.ToString();
    return CivilObjectUtils.GetStringProperty(value, "Handle") ?? "";
  }

  private static string? GetAnyString(object? value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var v = CivilObjectUtils.GetStringProperty(value, name);
      if (!string.IsNullOrWhiteSpace(v)) return v;
    }
    return null;
  }

  private static object? GetNamedMember(object? value, string memberName)
  {
    return Civil3DCompatibility.GetPropertyValue(value, memberName)
      ?? Civil3DCompatibility.GetFieldValue(value, memberName);
  }

  private static IEnumerable<object> EnumerateObjects(object? collection)
  {
    if (collection is IEnumerable enumerable)
      foreach (var item in enumerable)
        if (item != null) yield return item;
  }

  private static bool TrySetStringProperty(object target, string value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      if (Civil3DCompatibility.TrySetProperty(target, name, value)) return true;
    }

    return false;
  }

  private static void TrySetObjectIdPropertyOnObj(object target, ObjectId objectId, params string[] propertyNames)
  {
    if (objectId == ObjectId.Null) return;
    foreach (var name in propertyNames)
    {
      if (Civil3DCompatibility.TrySetProperty(target, name, objectId)) return;
    }
  }

  private static ObjectId ExtractObjectId(object? result)
  {
    if (result is ObjectId id && id != ObjectId.Null) return id;
    return CivilObjectUtils.GetPropertyValue<ObjectId>(result, "ObjectId");
  }

  private static ObjectId GetFirstObjectId(object target, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var objectId = CivilObjectUtils.GetPropertyValue<ObjectId>(target, propertyName);
      if (objectId != ObjectId.Null)
        return objectId;
    }

    return ObjectId.Null;
  }

  private static void TryInvokeIfPresent(object target, string methodName, params object?[] args)
  {
    Civil3DCompatibility.TryInvokeMethod(target, methodName, out _, args);
  }
}
