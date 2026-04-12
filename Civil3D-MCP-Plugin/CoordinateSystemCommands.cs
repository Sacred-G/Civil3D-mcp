using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_coordinate_system tool: getCoordinateSystemInfo / transformCoordinates.
///
/// Civil 3D API notes:
///   civilDoc.Settings.DrawingSettings.UnitZoneSettings exposes CoordinateSystemCode,
///   Zone, Datum, and projection properties for the active drawing's CRS.
///   The Civil 3D CoordinateTransformer (via Autodesk.CSMAP or reflection) performs
///   geographic ↔ drawing coordinate conversion.
/// </summary>
public static class CoordinateSystemCommands
{
  // -------------------------------------------------------------------------
  // getCoordinateSystemInfo
  // -------------------------------------------------------------------------

  public static Task<object?> GetCoordinateSystemInfoAsync()
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Access coordinate system info from drawing settings
      var settings = CivilObjectUtils.GetPropertyValue<object>(civilDoc, "Settings");
      var drawingSettings = CivilObjectUtils.GetPropertyValue<object>(settings, "DrawingSettings");
      var unitZone = CivilObjectUtils.GetPropertyValue<object>(drawingSettings, "UnitZoneSettings");

      string? csCode = null;
      string? zone = null;
      string? datum = null;
      string? projection = null;
      double? centralMeridian = null;
      double? falseEasting = null;
      double? falseNorthing = null;
      double? scaleFactor = null;

      if (unitZone != null)
      {
        csCode = CivilObjectUtils.GetStringProperty(unitZone, "CoordinateSystemCode")
          ?? CivilObjectUtils.GetStringProperty(unitZone, "CsCode")
          ?? CivilObjectUtils.GetStringProperty(unitZone, "ZoneCode");
        zone = CivilObjectUtils.GetStringProperty(unitZone, "Zone")
          ?? CivilObjectUtils.GetStringProperty(unitZone, "ZoneName");
        datum = CivilObjectUtils.GetStringProperty(unitZone, "Datum")
          ?? CivilObjectUtils.GetStringProperty(unitZone, "DatumName");
        projection = CivilObjectUtils.GetStringProperty(unitZone, "Projection")
          ?? CivilObjectUtils.GetStringProperty(unitZone, "ProjectionName");
        centralMeridian = CivilObjectUtils.GetDoubleProperty(unitZone, "CentralMeridian");
        falseEasting = CivilObjectUtils.GetDoubleProperty(unitZone, "FalseEasting");
        falseNorthing = CivilObjectUtils.GetDoubleProperty(unitZone, "FalseNorthing");
        scaleFactor = CivilObjectUtils.GetDoubleProperty(unitZone, "ScaleFactor");
      }

      return new Dictionary<string, object?>
      {
        ["name"] = csCode,
        ["zone"] = zone,
        ["datum"] = datum,
        ["projection"] = projection,
        ["linearUnits"] = CivilObjectUtils.LinearUnits(database),
        ["centralMeridian"] = centralMeridian,
        ["falseEasting"] = falseEasting,
        ["falseNorthing"] = falseNorthing,
        ["scaleFactor"] = scaleFactor,
      };
    });
  }

  // -------------------------------------------------------------------------
  // transformCoordinates
  // -------------------------------------------------------------------------

  public static Task<object?> TransformCoordinatesAsync(JsonObject? parameters)
  {
    var fromSystem = PluginRuntime.GetRequiredString(parameters, "fromSystem");
    var toSystem = PluginRuntime.GetRequiredString(parameters, "toSystem");
    var x = PluginRuntime.GetRequiredDouble(parameters, "x");
    var y = PluginRuntime.GetRequiredDouble(parameters, "y");
    var z = PluginRuntime.GetOptionalDouble(parameters, "z") ?? 0.0;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Civil 3D coordinate transformation via drawing's geographic transform
      // Try to access via reflection since the exact API type varies by version
      var outX = x;
      var outY = y;
      var outZ = z;

      try
      {
        // Attempt to use the Civil 3D coordinate transformer
        // The API varies: some versions use CivilDocument.GetCoordinateTransformInfo()
        // or Autodesk.Civil.CoordinateTransformer
        var transformerType = FindCoordinateTransformerType();
        if (transformerType != null)
        {
          var methods = transformerType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
          var toGeo = methods.FirstOrDefault(m => m.Name.Contains("ToGeographic") || m.Name.Contains("DrawingToGeographic"));
          var toDwg = methods.FirstOrDefault(m => m.Name.Contains("ToDrawing") || m.Name.Contains("GeographicToDrawing"));

          if (fromSystem == "drawing" && toSystem == "geographic" && toGeo != null)
          {
            var args = toGeo.GetParameters().Length == 3
              ? new object[] { x, y, z }
              : new object[] { x, y };
            var result = toGeo.Invoke(null, args);
            if (result != null)
            {
              outX = CivilObjectUtils.GetDoubleProperty(result, "X") ?? outX;
              outY = CivilObjectUtils.GetDoubleProperty(result, "Y") ?? outY;
              outZ = CivilObjectUtils.GetDoubleProperty(result, "Z") ?? outZ;
            }
          }
          else if (fromSystem == "geographic" && toSystem == "drawing" && toDwg != null)
          {
            var args = toDwg.GetParameters().Length == 3
              ? new object[] { x, y, z }
              : new object[] { x, y };
            var result = toDwg.Invoke(null, args);
            if (result != null)
            {
              outX = CivilObjectUtils.GetDoubleProperty(result, "X") ?? outX;
              outY = CivilObjectUtils.GetDoubleProperty(result, "Y") ?? outY;
              outZ = CivilObjectUtils.GetDoubleProperty(result, "Z") ?? outZ;
            }
          }
        }
      }
      catch
      {
        // If the transformer is unavailable, return input coordinates with a note
        return new Dictionary<string, object?>
        {
          ["x"] = x,
          ["y"] = y,
          ["z"] = z,
          ["note"] = "Coordinate transformation is not available in this drawing (no coordinate system assigned or transformer API unavailable).",
        };
      }

      return new Dictionary<string, object?>
      {
        ["x"] = outX,
        ["y"] = outY,
        ["z"] = outZ,
        ["fromSystem"] = fromSystem,
        ["toSystem"] = toSystem,
      };
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static Type? FindCoordinateTransformerType()
  {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
      var t = asm.GetType("Autodesk.Civil.CoordinateTransformer")
        ?? asm.GetType("Autodesk.CSMAP.CsMapCoordinateSystem");
      if (t != null) return t;
    }
    return null;
  }
}
