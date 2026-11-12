using System.Collections;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class LookupUtils
{
  /// <summary>
  /// Resolves the layer an object should be created on. No name means the
  /// current layer. A named layer that does not exist yet is CREATED (it used
  /// to fall back silently to the current layer, which put e.g. a profile
  /// requested on C-ROAD-DES onto layer 0). Every caller runs inside a
  /// CivilExecution.WriteAsync transaction, so the new layer is committed with
  /// the object that uses it. Do not call it from a read-only path: it would
  /// add a layer there.
  /// </summary>
  public static ObjectId GetLayerId(Database database, Transaction transaction, string? layerName)
  {
    if (string.IsNullOrWhiteSpace(layerName))
    {
      return database.Clayer;
    }

    var name = layerName.Trim();
    var layerTable = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForRead);
    if (layerTable.Has(name))
    {
      return layerTable[name];
    }

    try
    {
      SymbolUtilityServices.ValidateSymbolName(name, false);
    }
    catch (Autodesk.AutoCAD.Runtime.Exception)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'{name}' is not a valid layer name.");
    }

    layerTable.UpgradeOpen();
    var record = new LayerTableRecord { Name = name };
    var layerId = layerTable.Add(record);
    transaction.AddNewlyCreatedDBObject(record, true);
    return layerId;
  }

  public static ObjectId GetSiteId(CivilDocument civilDoc, Transaction transaction, string? siteName)
  {
    if (string.IsNullOrWhiteSpace(siteName))
    {
      return ObjectId.Null;
    }

    foreach (ObjectId objectId in civilDoc.GetSiteIds())
    {
      var site = CivilObjectUtils.GetRequiredObject<Site>(transaction, objectId, OpenMode.ForRead);
      if (string.Equals(site.Name, siteName, StringComparison.OrdinalIgnoreCase))
      {
        return objectId;
      }
    }

    return ObjectId.Null;
  }

  // Style lookups. With no name each returns the drawing's first style of that
  // kind (ObjectId.Null only when the drawing has none), except where noted.
  // A name that does not exist is CIVIL3D.INVALID_INPUT listing the available
  // names; it used to fall back silently to the first style (for profiles in
  // the NCS template, "Existing Ground Profile"). Matching is exact first, then
  // case-insensitive. Names are read via CivilObjectUtils.GetName, which on
  // Civil 3D 2027 returned null for every style (StyleBase hides the Name
  // getter), so before that fix no named lookup ever matched.

  public static ObjectId GetAlignmentStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.AlignmentStyles, transaction, styleName, "Alignment style");
  }

  public static ObjectId GetProfileStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.ProfileStyles, transaction, styleName, "Profile style");
  }

  public static ObjectId GetSurfaceStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.SurfaceStyles, transaction, styleName, "Surface style");
  }

  public static ObjectId GetAlignmentLabelSetId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles, transaction, styleName, "Alignment label set");
  }

  public static ObjectId GetProfileLabelSetId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles, transaction, styleName, "Profile label set");
  }

  public static ObjectId GetProfileViewStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.ProfileViewStyles, transaction, styleName, "Profile view style");
  }

  /// <summary>
  /// The named profile view band set. Band sets live on StylesRoot, not on
  /// LabelSetStylesRoot (the old reflective lookup there always returned
  /// Null). With no name this returns ObjectId.Null unless
  /// <paramref name="fallbackToFirst"/> is set.
  /// </summary>
  public static ObjectId GetProfileViewBandSetId(CivilDocument civilDoc, Transaction transaction, string? bandSetName, bool fallbackToFirst = false)
  {
    if (string.IsNullOrWhiteSpace(bandSetName) && !fallbackToFirst)
    {
      return ObjectId.Null;
    }

    return GetStyleId(civilDoc.Styles.ProfileViewBandSetStyles, transaction, bandSetName, "Profile view band set");
  }

  public static ObjectId GetParcelStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.ParcelStyles, transaction, styleName, "Parcel style");
  }

  public static ObjectId GetParcelAreaLabelStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.LabelStyles.ParcelLabelStyles.AreaLabelStyles, transaction, styleName, "Parcel area label style");
  }

  public static ObjectId GetSectionViewStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return GetStyleId(civilDoc.Styles.SectionViewStyles, transaction, styleName, "Section view style");
  }

  /// <summary>With no name, ObjectId.Null (Civil 3D's default).</summary>
  public static ObjectId GetSectionViewBandSetId(CivilDocument civilDoc, Transaction transaction, string? bandSetName)
  {
    return string.IsNullOrWhiteSpace(bandSetName)
      ? ObjectId.Null
      : GetStyleId(civilDoc.Styles.SectionViewBandSetStyles, transaction, bandSetName, "Section view band set");
  }

  /// <summary>With no name, ObjectId.Null (Civil 3D's default).</summary>
  public static ObjectId GetGroupPlotStyleId(CivilDocument civilDoc, Transaction transaction, string? styleName)
  {
    return string.IsNullOrWhiteSpace(styleName)
      ? ObjectId.Null
      : GetStyleId(civilDoc.Styles.GroupPlotStyles, transaction, styleName, "Group plot style");
  }

  public static string? GetFirstStyleName(object? collection, Transaction transaction)
  {
    foreach (var objectId in EnumerateObjectIds(collection))
    {
      if (objectId == ObjectId.Null)
      {
        continue;
      }

      var style = transaction.GetObject(objectId, OpenMode.ForRead);
      return CivilObjectUtils.GetName(style);
    }

    return null;
  }

  private static ObjectId GetStyleId(object collection, Transaction transaction, string? styleName, string kind)
  {
    var ids = EnumerateObjectIds(collection).Where(objectId => !objectId.IsNull).ToList();
    if (string.IsNullOrWhiteSpace(styleName))
    {
      return ids.Count > 0 ? ids[0] : ObjectId.Null;
    }

    // Open styles one at a time and stop at an exact (ordinal) match, so the
    // usual lookup does not read every style. Only without an exact match is
    // the whole list needed: for the case-insensitive match, and for the
    // error message listing the available names. A style that cannot be
    // opened (erased or stale id) is skipped, not fatal.
    var wanted = styleName.Trim();
    var names = new List<string?>(ids.Count);
    foreach (var objectId in ids)
    {
      var name = TryReadName(transaction, objectId);
      if (string.Equals(name?.Trim(), wanted, StringComparison.Ordinal))
      {
        return objectId;
      }

      names.Add(name);
    }

    var index = StyleNameMatch.FindIndex(names, styleName);
    if (index < 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", StyleNameMatch.DescribeMissing(kind, styleName, names));
    }

    return ids[index];
  }

  private static string? TryReadName(Transaction transaction, ObjectId objectId)
  {
    if (objectId.IsErased || !objectId.IsValid)
    {
      return null;
    }

    try
    {
      return CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead));
    }
    catch (Autodesk.AutoCAD.Runtime.Exception exception) when (IsUnreadableStyle(exception.ErrorStatus))
    {
      // Only a style that is gone (erased, or a stale id) is skipped. Lock,
      // transaction and database errors propagate to the normal error path
      // instead of being reported as an unknown style name.
      return null;
    }
  }

  private static bool IsUnreadableStyle(Autodesk.AutoCAD.Runtime.ErrorStatus status) =>
    status is Autodesk.AutoCAD.Runtime.ErrorStatus.WasErased
      or Autodesk.AutoCAD.Runtime.ErrorStatus.PermanentlyErased
      or Autodesk.AutoCAD.Runtime.ErrorStatus.NullObjectId
      or Autodesk.AutoCAD.Runtime.ErrorStatus.InvalidObjectId
      or Autodesk.AutoCAD.Runtime.ErrorStatus.UnknownHandle;

  private static IEnumerable<ObjectId> EnumerateObjectIds(object? collection)
  {
    if (collection is ObjectIdCollection objectIds)
    {
      foreach (ObjectId objectId in objectIds)
      {
        yield return objectId;
      }

      yield break;
    }

    if (collection is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item is ObjectId objectId)
        {
          yield return objectId;
        }
      }
    }
  }
}
