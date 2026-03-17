using System.Collections;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class CivilObjectUtils
{
  public static string GetHandle(DBObject dbObject)
  {
    return dbObject.Handle.ToString();
  }

  public static string? GetName(object? value)
  {
    if (value == null)
    {
      return null;
    }

    var nameProperty = value.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
    return nameProperty?.GetValue(value)?.ToString();
  }

  public static string? GetStringProperty(object? value, string propertyName)
  {
    if (value == null)
    {
      return null;
    }

    var property = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    return property?.GetValue(value)?.ToString();
  }

  public static T? GetPropertyValue<T>(object? value, string propertyName)
  {
    if (value == null)
    {
      return default;
    }

    var property = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    if (property == null)
    {
      return default;
    }

    var raw = property.GetValue(value);
    if (raw == null)
    {
      return default;
    }

    if (raw is T typed)
    {
      return typed;
    }

    var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
    if (targetType.IsInstanceOfType(raw))
    {
      return (T)(object)raw;
    }

    var converted = Convert.ChangeType(raw, targetType);
    return (T)(object)converted!;
  }

  public static object? InvokeMethod(object? value, string methodName, params object?[] arguments)
  {
    if (value == null)
    {
      return null;
    }

    var methods = value.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
      .Where(m => m.Name == methodName)
      .ToArray();

    foreach (var method in methods)
    {
      var parameters = method.GetParameters();
      if (parameters.Length != arguments.Length)
      {
        continue;
      }

      try
      {
        return method.Invoke(value, arguments);
      }
      catch
      {
      }
    }

    return null;
  }

  public static IEnumerable<ObjectId> ToObjectIds(object? collection)
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

  public static T GetRequiredObject<T>(Transaction transaction, ObjectId objectId, OpenMode openMode) where T : DBObject
  {
    return (T)(transaction.GetObject(objectId, openMode) ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Object {objectId} not found."));
  }

  public static string LinearUnits(Database database)
  {
    return database.Insunits switch
    {
      UnitsValue.Meters => "meters",
      UnitsValue.Feet => "feet",
      UnitsValue.UsSurveyFeet => "feet",
      _ => "other",
    };
  }

  public static string AngularUnits(short aunits)
  {
    return aunits switch
    {
      0 => "degrees",
      1 => "degrees",
      2 => "grads",
      3 => "radians",
      _ => "degrees",
    };
  }

  public static Alignment FindAlignmentByName(CivilDocument civilDoc, Transaction transaction, string name)
  {
    foreach (ObjectId objectId in civilDoc.GetAlignmentIds())
    {
      var alignment = GetRequiredObject<Alignment>(transaction, objectId, OpenMode.ForRead);
      if (string.Equals(alignment.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return alignment;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Alignment '{name}' was not found.");
  }

  public static Profile FindProfileByName(Alignment alignment, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (ObjectId objectId in alignment.GetProfileIds())
    {
      var profile = GetRequiredObject<Profile>(transaction, objectId, openMode);
      if (string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return profile;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile '{name}' was not found on alignment '{alignment.Name}'.");
  }

  public static Surface FindSurfaceByName(CivilDocument civilDoc, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (ObjectId objectId in civilDoc.GetSurfaceIds())
    {
      var surface = GetRequiredObject<Surface>(transaction, objectId, openMode);
      if (string.Equals(surface.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return surface;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Surface '{name}' was not found.");
  }

  public static Corridor FindCorridorByName(CivilDocument civilDoc, Transaction transaction, string name, OpenMode openMode)
  {
    foreach (ObjectId objectId in civilDoc.CorridorCollection)
    {
      var corridor = GetRequiredObject<Corridor>(transaction, objectId, openMode);
      if (string.Equals(corridor.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return corridor;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Corridor '{name}' was not found.");
  }

  public static double? GetDoubleProperty(object? value, string propertyName)
  {
    if (value == null) return null;
    var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    var raw = prop?.GetValue(value);
    if (raw == null) return null;
    try { return Convert.ToDouble(raw); } catch { return null; }
  }

  public static bool? GetBoolProperty(object? value, string propertyName)
  {
    if (value == null) return null;
    var prop = value.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    var raw = prop?.GetValue(value);
    if (raw == null) return null;
    try { return Convert.ToBoolean(raw); } catch { return null; }
  }

  public static string VolumeUnits(Database database)
  {
    return database.Insunits switch
    {
      UnitsValue.Meters => "cubic meters",
      UnitsValue.Feet => "cubic feet",
      UnitsValue.UsSurveyFeet => "cubic feet",
      _ => "cubic units",
    };
  }

  public static void TrySetName(DBObject obj, string name)
  {
    var prop = obj.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
    try { prop?.SetValue(obj, name); } catch { /* ignore */ }
  }

  public static void TrySetLayer(DBObject obj, string layer, Database database, Transaction transaction)
  {
    try
    {
      var layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
      if (layerTable == null) return;
      if (!layerTable.Has(layer))
      {
        var lt = transaction.GetObject(database.LayerTableId, OpenMode.ForWrite) as LayerTable;
        var ltr = new LayerTableRecord { Name = layer };
        lt!.Add(ltr);
        transaction.AddNewlyCreatedDBObject(ltr, true);
      }
      var prop = obj.GetType().GetProperty("Layer", BindingFlags.Public | BindingFlags.Instance);
      prop?.SetValue(obj, layer);
    }
    catch { /* ignore layer errors */ }
  }

  public static Dictionary<string, object?> ToPointData(CogoPoint point)
  {
    return new Dictionary<string, object?>
    {
      ["number"] = point.PointNumber,
      ["name"] = point.PointName,
      ["x"] = point.Location.X,
      ["y"] = point.Location.Y,
      ["z"] = point.Location.Z,
      ["rawDescription"] = point.RawDescription,
      ["fullDescription"] = point.FullDescription,
    };
  }
}
