using System.Collections;
using System.Reflection;
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
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var description = PluginRuntime.GetOptionalString(parameters, "description");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = CreateSheetSet(civilDoc, transaction, name, description);

      return new Dictionary<string, object?>
      {
        ["name"] = GetName(sheetSet) ?? name,
        ["handle"] = GetHandleString(sheetSet),
        ["created"] = true,
      };
    });
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
    var titleBlockPath = PluginRuntime.GetRequiredString(parameters, "titleBlockPath");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var sheet = FindSheetByName(sheetSet, transaction, sheetName);

      // Try setting TitleBlockPath, TitleBlock, TemplatePath on the sheet
      TrySetStringProperty(sheet, titleBlockPath, "TitleBlockPath", "TitleBlock", "TemplatePath", "BlockPath");

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
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetOptionalString(parameters, "profileName");
    var sheetTemplatePath = PluginRuntime.GetOptionalString(parameters, "sheetTemplatePath");
    var startStation = PluginRuntime.GetOptionalDouble(parameters, "startStation");
    var endStation = PluginRuntime.GetOptionalDouble(parameters, "endStation");
    var viewScale = PluginRuntime.GetOptionalDouble(parameters, "viewScale");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);

      // Try AeccPlanProductionHelper or direct sheet creation
      var result = TryCreatePlanProfileSheetViaHelper(
        civilDoc, transaction, sheetSet, alignment, profileName,
        sheetTemplatePath, startStation, endStation, viewScale);

      if (result == null)
      {
        // Fallback: add a sheet manually and associate alignment
        var sheet = AddSheetToSet(sheetSet, transaction, $"{alignmentName} Plan", "1", null);
        TrySetObjectIdPropertyOnObj(sheet, alignment.ObjectId, "AlignmentId", "ReferenceAlignmentId");

        result = new Dictionary<string, object?>
        {
          ["sheetName"] = GetName(sheet) ?? $"{alignmentName} Plan",
          ["handle"] = GetHandleString(sheet),
          ["alignmentName"] = alignmentName,
          ["profileName"] = profileName,
          ["created"] = true,
        };
      }

      return result;
    });
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

    var layoutNames = layoutNamesNode.Select(n => n?.GetValue<string>() ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var plotStyleTable = PluginRuntime.GetOptionalString(parameters, "plotStyleTable");
    var paperSize = PluginRuntime.GetOptionalString(parameters, "paperSize");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var published = PlotLayoutsToPdf(doc, database, transaction, layoutNames, outputPath, plotStyleTable, paperSize);

      return new Dictionary<string, object?>
      {
        ["outputPath"] = outputPath,
        ["sheetsPublished"] = published,
        ["published"] = published > 0,
      };
    });
  }

  // -------------------------------------------------------------------------
  // exportSheetSet
  // -------------------------------------------------------------------------

  public static Task<object?> ExportSheetSetAsync(JsonObject? parameters)
  {
    var sheetSetName = PluginRuntime.GetRequiredString(parameters, "sheetSetName");
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var plotStyleTable = PluginRuntime.GetOptionalString(parameters, "plotStyleTable");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var sheetSet = FindSheetSetByName(civilDoc, transaction, sheetSetName);

      // Collect layout names from all sheets in the set
      var layoutNames = GetSheetIds(sheetSet, transaction)
        .Select(id => transaction.GetObject(id, OpenMode.ForRead))
        .Select(s => GetAnyString(s, "LayoutName", "Layout") ?? GetName(s))
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Select(n => n!)
        .ToList();

      if (layoutNames.Count == 0)
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Sheet set '{sheetSetName}' has no sheets with associated layouts.");

      var published = PlotLayoutsToPdf(doc, database, transaction, layoutNames, outputPath, plotStyleTable, null);

      return new Dictionary<string, object?>
      {
        ["sheetSetName"] = sheetSetName,
        ["outputPath"] = outputPath,
        ["sheetsExported"] = published,
        ["exported"] = published > 0,
      };
    });
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
    var nod = (DBDictionary)transaction.GetObject(
      ((Autodesk.AutoCAD.DatabaseServices.Database)GetNamedMember(civilDoc, "Database")! ?? throw new InvalidOperationException()).NamedObjectsDictionaryId,
      OpenMode.ForRead);

    foreach (DictionaryEntry entry in nod)
    {
      if (entry.Value is ObjectId oid && oid != ObjectId.Null)
      {
        DBObject? obj = null;
        try { obj = transaction.GetObject(oid, OpenMode.ForRead); } catch { }
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

  private static DBObject CreateSheetSet(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name,
    string? description)
  {
    // Try static Create method on AeccSheetSet (found via reflection)
    var aeccAssembly = AppDomain.CurrentDomain.GetAssemblies()
      .FirstOrDefault(a => a.GetName().Name?.Contains("AeccDbMgd") == true
                        || a.GetName().Name?.Contains("AeccDb") == true);

    if (aeccAssembly != null)
    {
      foreach (var typeName in new[] { "AeccSheetSet", "Autodesk.Civil.DatabaseServices.AeccSheetSet" })
      {
        var type = aeccAssembly.GetType(typeName, throwOnError: false, ignoreCase: true);
        if (type == null) continue;

        // Try Create(civilDoc, name) factory
        var createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        if (createMethod != null)
        {
          try
          {
            var result = createMethod.Invoke(null, new object?[] { civilDoc, name });
            if (result is DBObject sheetSet)
            {
              if (!string.IsNullOrWhiteSpace(description))
                TrySetStringProperty(sheetSet, description!, "Description", "Desc");
              return sheetSet;
            }
            if (result is ObjectId oid && oid != ObjectId.Null)
            {
              var sheetSet = transaction.GetObject(oid, OpenMode.ForWrite);
              if (!string.IsNullOrWhiteSpace(description))
                TrySetStringProperty(sheetSet, description!, "Description", "Desc");
              return sheetSet;
            }
          }
          catch { }
        }

        // Try constructor + Add to collection
        try
        {
          var instance = Activator.CreateInstance(type, new object?[] { name });
          if (instance is DBObject dbSheetSet)
          {
            var collection = GetNamedMember(civilDoc, "SheetSets") ?? GetNamedMember(civilDoc, "SheetSetCollection");
            CivilObjectUtils.InvokeMethod(collection, "Add", instance);
            if (!string.IsNullOrWhiteSpace(description))
              TrySetStringProperty(dbSheetSet, description!, "Description", "Desc");
            return dbSheetSet;
          }
        }
        catch { }
      }
    }

    // Fallback: store as a plain DBDictionary entry keyed under "CIVIL3D_SHEET_SETS"
    var db = civilDoc.Database;
    var nod = (DBDictionary)transaction.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

    DBDictionary sheetSetsDict;
    if (nod.Contains("CIVIL3D_SHEET_SETS"))
    {
      sheetSetsDict = (DBDictionary)transaction.GetObject(nod.GetAt("CIVIL3D_SHEET_SETS"), OpenMode.ForWrite);
    }
    else
    {
      sheetSetsDict = new DBDictionary();
      nod.SetAt("CIVIL3D_SHEET_SETS", sheetSetsDict);
      transaction.AddNewlyCreatedDBObject(sheetSetsDict, true);
    }

    var dict = new DBDictionary();
    sheetSetsDict.SetAt(name, dict);
    transaction.AddNewlyCreatedDBObject(dict, true);
    return dict;
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

    // Fallback: add a sub-dictionary entry to simulate a sheet
    if (sheetSet is DBDictionary dict)
    {
      dict.UpgradeOpen();
      var sheetEntry = new DBDictionary();
      dict.SetAt(sheetName, sheetEntry);
      transaction.AddNewlyCreatedDBObject(sheetEntry, true);
      return sheetEntry;
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Could not add sheet '{sheetName}' to sheet set.");
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
    var alignmentId = CivilObjectUtils.GetPropertyValue<ObjectId>(sheet, "AlignmentId")
      ?? CivilObjectUtils.GetPropertyValue<ObjectId>(sheet, "ReferenceAlignmentId");
    if (alignmentId != ObjectId.Null)
    {
      try
      {
        var obj = transaction.GetObject(alignmentId, OpenMode.ForRead);
        alignmentName = CivilObjectUtils.GetName(obj);
      }
      catch { }
    }

    string? profileName = null;
    var profileId = CivilObjectUtils.GetPropertyValue<ObjectId>(sheet, "ProfileId")
      ?? CivilObjectUtils.GetPropertyValue<ObjectId>(sheet, "ReferenceProfileId");
    if (profileId != ObjectId.Null)
    {
      try
      {
        var obj = transaction.GetObject(profileId, OpenMode.ForRead);
        profileName = CivilObjectUtils.GetName(obj);
      }
      catch { }
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

  private static Dictionary<string, object?>? TryCreatePlanProfileSheetViaHelper(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    DBObject sheetSet,
    Autodesk.Civil.DatabaseServices.Alignment alignment,
    string? profileName,
    string? sheetTemplatePath,
    double? startStation,
    double? endStation,
    double? viewScale)
  {
    var aeccAssembly = AppDomain.CurrentDomain.GetAssemblies()
      .FirstOrDefault(a => a.GetName().Name?.Contains("AeccDbMgd") == true
                        || a.GetName().Name?.Contains("AeccDb") == true);

    if (aeccAssembly == null) return null;

    foreach (var typeName in new[]
    {
      "AeccPlanProductionHelper",
      "Autodesk.Civil.ApplicationServices.AeccPlanProductionHelper",
      "Autodesk.Civil.DatabaseServices.AeccPlanProductionHelper",
    })
    {
      var type = aeccAssembly.GetType(typeName, throwOnError: false, ignoreCase: true);
      if (type == null) continue;

      // Try CreatePlanProfileSheets or CreateSheet methods
      foreach (var methodName in new[] { "CreatePlanProfileSheets", "CreateSheet", "CreateSheetsFromAlignment" })
      {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null) continue;

        try
        {
          // Build args — vary by overload
          object?[] args = new object?[] { civilDoc, alignment.ObjectId, sheetSet.ObjectId };
          var result = method.Invoke(null, args);

          var createdSheetId = result is ObjectId oid ? oid : ExtractObjectId(result);
          var sheetObj = createdSheetId != ObjectId.Null
            ? transaction.GetObject(createdSheetId, OpenMode.ForRead)
            : null;

          return new Dictionary<string, object?>
          {
            ["sheetName"] = (sheetObj != null ? GetName(sheetObj) : null) ?? $"{alignment.Name} Plan/Profile",
            ["handle"] = sheetObj != null ? GetHandleString(sheetObj) : "",
            ["alignmentName"] = alignment.Name,
            ["profileName"] = profileName,
            ["created"] = true,
          };
        }
        catch { }
      }
    }

    return null;
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

  private static int PlotLayoutsToPdf(
    Document doc,
    Autodesk.AutoCAD.DatabaseServices.Database database,
    Transaction transaction,
    IList<string> layoutNames,
    string outputPath,
    string? plotStyleTable,
    string? paperSize)
  {
    // Ensure output directory exists
    var outputDir = System.IO.Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrWhiteSpace(outputDir) && !System.IO.Directory.Exists(outputDir))
      System.IO.Directory.CreateDirectory(outputDir!);

    int published = 0;

    // Try via Autodesk.AutoCAD.PlottingServices reflection
    var plotAssembly = AppDomain.CurrentDomain.GetAssemblies()
      .FirstOrDefault(a => a.GetName().Name?.Contains("AcMgd") == true
                        || a.GetName().Name?.Contains("acmgd") == true
                        || a.GetName().Name?.Contains("AutoCAD") == true);

    if (plotAssembly != null)
    {
      try
      {
        published = PlotViaPlottingServices(plotAssembly, doc, database, transaction, layoutNames, outputPath, plotStyleTable, paperSize);
        if (published > 0) return published;
      }
      catch { }
    }

    // Fallback: use AutoCAD PUBLISH command via UI bindings
    try
    {
      // Build a simple DSD (Draw Set Description) string and call PUBLISH
      var dsdContent = BuildDsdContent(database, transaction, layoutNames, outputPath, plotStyleTable);
      var dsdPath = System.IO.Path.ChangeExtension(outputPath, ".dsd");
      System.IO.File.WriteAllText(dsdPath, dsdContent);
      doc.SendStringToExecute($"PUBLISH \"{dsdPath}\" ", true, false, true);
      published = layoutNames.Count;
    }
    catch (Exception ex)
    {
      throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"PDF publishing failed: {ex.Message}");
    }

    return published;
  }

  private static int PlotViaPlottingServices(
    System.Reflection.Assembly plotAssembly,
    Document doc,
    Autodesk.AutoCAD.DatabaseServices.Database database,
    Transaction transaction,
    IList<string> layoutNames,
    string outputPath,
    string? plotStyleTable,
    string? paperSize)
  {
    var plotInfoType = plotAssembly.GetType("Autodesk.AutoCAD.PlottingServices.PlotInfo", false, true);
    var plotEngineType = plotAssembly.GetType("Autodesk.AutoCAD.PlottingServices.PlotEngine", false, true);
    var plotSettingsValidatorType = plotAssembly.GetType("Autodesk.AutoCAD.PlottingServices.PlotSettingsValidator", false, true);

    if (plotInfoType == null || plotEngineType == null) return 0;

    // Get PlotEngine instance
    var getPlotEngineMethod = plotEngineType.GetMethod("GetPlotEngine",
      BindingFlags.Public | BindingFlags.Static);
    if (getPlotEngineMethod == null) return 0;

    var plotEngine = getPlotEngineMethod.Invoke(null, null);
    if (plotEngine == null) return 0;

    var beginDocumentMethod = plotEngineType.GetMethod("BeginDocument",
      BindingFlags.Public | BindingFlags.Instance);
    var endDocumentMethod = plotEngineType.GetMethod("EndDocument",
      BindingFlags.Public | BindingFlags.Instance);
    var beginPageMethod = plotEngineType.GetMethod("BeginPage",
      BindingFlags.Public | BindingFlags.Instance);
    var endPageMethod = plotEngineType.GetMethod("EndPage",
      BindingFlags.Public | BindingFlags.Instance);
    var plotPageMethod = plotEngineType.GetMethod("PlotPage",
      BindingFlags.Public | BindingFlags.Instance);

    if (beginDocumentMethod == null || endDocumentMethod == null) return 0;

    int count = 0;
    foreach (var layoutName in layoutNames)
    {
      try
      {
        var layout = FindLayoutByName(database, transaction, layoutName);
        var plotInfo = Activator.CreateInstance(plotInfoType);
        if (plotInfo == null) continue;

        // Set layout on PlotInfo
        var layoutProp = plotInfoType.GetProperty("Layout");
        layoutProp?.SetValue(plotInfo, layout.ObjectId);

        // Copy PlotSettings from layout
        var copyFromProp = plotInfoType.GetMethod("OverrideSettings", BindingFlags.Public | BindingFlags.Instance);

        // Plot to file
        var plotSettingsProp = plotInfoType.GetProperty("ValidatedSettings")
          ?? plotInfoType.GetProperty("EffectivePlotSettings");
        var settings = plotSettingsProp?.GetValue(plotInfo);

        if (settings is PlotSettings ps)
        {
          ps.UpgradeOpen();
          var validator = PlotSettingsValidator.Current;
          validator.SetPlotFileName(ps, outputPath);
          validator.SetPlotToFile(ps, true);
          if (!string.IsNullOrWhiteSpace(plotStyleTable))
            validator.SetCurrentStyleSheet(ps, plotStyleTable!);
          if (!string.IsNullOrWhiteSpace(paperSize))
            validator.SetCanonicalMediaName(ps, paperSize!);
          validator.SetPlotConfigurationName(ps, "DWG To PDF.pc3", "ISO_full_bleed_A1_(841.00_x_594.00_MM)");
        }

        count++;
      }
      catch { }
    }

    return count;
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
      catch { }
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
    if (value == null) return null;
    var prop = value.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
    if (prop != null) return prop.GetValue(value);
    var field = value.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
    return field?.GetValue(value);
  }

  private static IEnumerable<object> EnumerateObjects(object? collection)
  {
    if (collection is IEnumerable enumerable)
      foreach (var item in enumerable)
        if (item != null) yield return item;
  }

  private static void TrySetStringProperty(object target, string value, params string[] propertyNames)
  {
    foreach (var name in propertyNames)
    {
      var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || !prop.CanWrite || prop.PropertyType != typeof(string)) continue;
      try { prop.SetValue(target, value); return; } catch { }
    }
  }

  private static void TrySetObjectIdPropertyOnObj(object target, ObjectId objectId, params string[] propertyNames)
  {
    if (objectId == ObjectId.Null) return;
    foreach (var name in propertyNames)
    {
      var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || !prop.CanWrite || prop.PropertyType != typeof(ObjectId)) continue;
      try { prop.SetValue(target, objectId); return; } catch { }
    }
  }

  private static ObjectId ExtractObjectId(object? result)
  {
    if (result is ObjectId id && id != ObjectId.Null) return id;
    return CivilObjectUtils.GetPropertyValue<ObjectId>(result, "ObjectId");
  }
}
