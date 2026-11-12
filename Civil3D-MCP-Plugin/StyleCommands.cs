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

        foreach (var property in Civil3DCompatibility.GetReadableScalarProperties(obj))
        {
          if (details.ContainsKey(property.Key)) continue;
          var key = char.ToLowerInvariant(property.Key[0]) + property.Key[1..];
          details[key] = property.Value;
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
    return objectType.ToLowerInvariant() switch
    {
      "surface" => civilDoc.Styles.SurfaceStyles.Cast<ObjectId>(),
      "alignment" => civilDoc.Styles.AlignmentStyles.Cast<ObjectId>(),
      "profile" => civilDoc.Styles.ProfileStyles.Cast<ObjectId>(),
      "corridor" => civilDoc.Styles.CorridorStyles.Cast<ObjectId>(),
      "pipe" => civilDoc.Styles.PipeStyles.Cast<ObjectId>(),
      "structure" => civilDoc.Styles.StructureStyles.Cast<ObjectId>(),
      "point" => civilDoc.Styles.PointStyles.Cast<ObjectId>(),
      "section" => civilDoc.Styles.SectionStyles.Cast<ObjectId>(),
      "assembly" => civilDoc.Styles.AssemblyStyles.Cast<ObjectId>(),
      "profile_view" => civilDoc.Styles.ProfileViewStyles.Cast<ObjectId>(),
      "profile_view_band_set" => civilDoc.Styles.ProfileViewBandSetStyles.Cast<ObjectId>(),
      "section_view" => civilDoc.Styles.SectionViewStyles.Cast<ObjectId>(),
      "section_view_band_set" => civilDoc.Styles.SectionViewBandSetStyles.Cast<ObjectId>(),
      "label" => throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        "'label' is a hierarchy of label-style collections, not one enumerable style collection. Use the civil3d_label tool for label-style operations."),
      _ => throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"Unsupported style object type '{objectType}'. Valid values: surface, alignment, profile, corridor, pipe, structure, point, section, assembly, profile_view, profile_view_band_set, section_view, section_view_band_set."),
    };
  }
}
