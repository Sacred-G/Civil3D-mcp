using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_style tool: listStyles / getStyle.
///
/// Civil 3D API notes:
///   civilDoc.Styles returns a StylesRoot object whose properties expose typed
///   style collections: SurfaceStyles, AlignmentStyles, ProfileStyles, etc.
///   Each collection is an IEnumerable of ObjectId; objects are opened via
///   the transaction in the usual way.
/// </summary>
public static class StyleCommands
{
  // -------------------------------------------------------------------------
  // listStyles
  // -------------------------------------------------------------------------

  public static Task<object?> ListStylesAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styles = GetStyleCollection(civilDoc, objectType);

      var styleItems = styles
        .Select(id => transaction.GetObject(id, OpenMode.ForRead))
        .Where(obj => obj != null)
        .Select(obj => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(obj),
          ["handle"] = obj is DBObject dbo ? CivilObjectUtils.GetHandle(dbo) : null,
          ["isDefault"] = CivilObjectUtils.GetBoolProperty(obj, "IsDefault") ?? false,
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["styles"] = styleItems,
      };
    });
  }

  // -------------------------------------------------------------------------
  // getStyle
  // -------------------------------------------------------------------------

  public static Task<object?> GetStyleAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var styleName = PluginRuntime.GetRequiredString(parameters, "styleName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styles = GetStyleCollection(civilDoc, objectType);

      foreach (ObjectId id in styles)
      {
        var obj = transaction.GetObject(id, OpenMode.ForRead);
        if (obj == null) continue;
        var name = CivilObjectUtils.GetName(obj);
        if (!string.Equals(name, styleName, StringComparison.OrdinalIgnoreCase)) continue;

        // Collect all readable primitive properties for detail view
        var details = new Dictionary<string, object?>
        {
          ["name"] = name,
          ["objectType"] = objectType,
          ["handle"] = obj is DBObject dbo ? CivilObjectUtils.GetHandle(dbo) : null,
          ["isDefault"] = CivilObjectUtils.GetBoolProperty(obj, "IsDefault") ?? false,
          ["description"] = CivilObjectUtils.GetStringProperty(obj, "Description"),
        };

        // Append any additional scalar properties via reflection
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
          if (details.ContainsKey(prop.Name)) continue;
          if (!prop.CanRead) continue;
          if (prop.GetIndexParameters().Length > 0) continue;
          var propType = prop.PropertyType;
          if (propType == typeof(string) || propType == typeof(bool) || propType == typeof(bool?)
              || propType == typeof(int) || propType == typeof(int?) || propType == typeof(double) || propType == typeof(double?))
          {
            try
            {
              var value = prop.GetValue(obj);
              // Convert first character to lowercase for camelCase key
              var key = char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
              details[key] = value;
            }
            catch { /* skip inaccessible properties */ }
          }
        }

        return details;
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
        $"Style '{styleName}' was not found for object type '{objectType}'.");
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static IEnumerable<ObjectId> GetStyleCollection(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc, string objectType)
  {
    // civilDoc.Styles is a StylesRoot; each property is a typed style collection.
    // We use reflection to stay forward-compatible with different Civil 3D versions.
    var stylesRoot = CivilObjectUtils.GetPropertyValue<object>(civilDoc, "Styles");
    if (stylesRoot == null)
    {
      return Enumerable.Empty<ObjectId>();
    }

    var collectionPropertyName = objectType.ToLowerInvariant() switch
    {
      "surface"   => "SurfaceStyles",
      "alignment" => "AlignmentStyles",
      "profile"   => "ProfileStyles",
      "corridor"  => "CorridorStyles",
      "pipe"      => "PipeStyles",
      "structure" => "StructureStyles",
      "point"     => "PointStyles",
      "section"   => "SectionStyles",
      "label"     => "LabelStyles",
      "assembly"  => "AssemblyStyles",
      _           => null,
    };

    if (collectionPropertyName == null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        $"Unsupported style object type '{objectType}'. Valid values: surface, alignment, profile, corridor, pipe, structure, point, section, label, assembly.");
    }

    var collectionProp = stylesRoot.GetType()
      .GetProperty(collectionPropertyName, BindingFlags.Public | BindingFlags.Instance);

    var collection = collectionProp?.GetValue(stylesRoot);
    if (collection == null)
    {
      return Enumerable.Empty<ObjectId>();
    }

    return CivilObjectUtils.ToObjectIds(collection);
  }
}
