using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.Settings;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_coordinate_system tool: getCoordinateSystemInfo / setCoordinateSystem / transformCoordinates.
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
      var unitZone = civilDoc.Settings.DrawingSettings.UnitZoneSettings;
      var csCode = unitZone.CoordinateSystemCode;
      string? zone = null;
      string? datum = null;
      string? projection = null;

      if (!string.IsNullOrWhiteSpace(csCode))
      {
        var coordinateSystem = SettingsUnitZone.GetCoordinateSystemByCode(csCode);
        zone = coordinateSystem.Category;
        datum = coordinateSystem.Datum;
        projection = coordinateSystem.Projection;
      }

      return new Dictionary<string, object?>
      {
        ["name"] = csCode,
        ["zone"] = zone,
        ["datum"] = datum,
        ["projection"] = projection,
        ["linearUnits"] = CivilObjectUtils.LinearUnits(database),
        ["centralMeridian"] = null,
        ["falseEasting"] = null,
        ["falseNorthing"] = null,
        ["scaleFactor"] = null,
      };
    });
  }

  // -------------------------------------------------------------------------
  // setCoordinateSystem
  // -------------------------------------------------------------------------

  public static Task<object?> SetCoordinateSystemAsync(JsonObject? parameters)
  {
    var code = PluginRuntime.GetRequiredString(parameters, "code").Trim();
    if (code.Length == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Coordinate system code must not be empty.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Validate before assigning so an unknown code fails loudly instead of
      // leaving the caller believing the drawing was georeferenced.
      // (SettingsUnitZone.IsValidCoordinateSystemCode exists but is internal.)
      var knownCode = SettingsUnitZone.GetAllCodes()
        .FirstOrDefault(candidate => string.Equals(candidate, code, StringComparison.OrdinalIgnoreCase));
      if (knownCode == null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          $"'{code}' is not a coordinate system code known to this Civil 3D installation. The drawing coordinate system was not changed.");
      }

      var unitZone = civilDoc.Settings.DrawingSettings.UnitZoneSettings;
      var previousCode = unitZone.CoordinateSystemCode;
      unitZone.CoordinateSystemCode = knownCode;

      var appliedCode = unitZone.CoordinateSystemCode;
      if (!string.Equals(appliedCode, knownCode, StringComparison.OrdinalIgnoreCase))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Civil 3D did not apply coordinate system '{knownCode}' (the drawing still reports '{appliedCode}').");
      }

      var coordinateSystem = SettingsUnitZone.GetCoordinateSystemByCode(appliedCode);
      return new Dictionary<string, object?>
      {
        ["code"] = appliedCode,
        ["previousCode"] = string.IsNullOrWhiteSpace(previousCode) ? null : previousCode,
        ["description"] = coordinateSystem.Description,
        ["zone"] = coordinateSystem.Category,
        ["datum"] = coordinateSystem.Datum,
        ["projection"] = coordinateSystem.Projection,
        ["unit"] = coordinateSystem.Unit,
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
      if (string.Equals(fromSystem, toSystem, StringComparison.OrdinalIgnoreCase))
      {
        return new Dictionary<string, object?>
        {
          ["x"] = x,
          ["y"] = y,
          ["z"] = z,
          ["fromSystem"] = fromSystem,
          ["toSystem"] = toSystem,
          ["identityTransform"] = true,
        };
      }

      try
      {
        // Attempt to use the Civil 3D coordinate transformer
        // The API varies: some versions use CivilDocument.GetCoordinateTransformInfo()
        // or Autodesk.Civil.CoordinateTransformer
        var transformerType = FindCoordinateTransformerType();
        if (transformerType == null)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.API_ERROR",
            "This Civil 3D installation does not expose a compatible coordinate-transform service. No coordinates were returned.");
        }

        var methodNames = fromSystem == "drawing" && toSystem == "geographic"
          ? new[] { "ToGeographic", "DrawingToGeographic" }
          : fromSystem == "geographic" && toSystem == "drawing"
            ? new[] { "ToDrawing", "GeographicToDrawing" }
            : Array.Empty<string>();

        if (methodNames.Length == 0)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.API_ERROR",
            $"Coordinate transform '{fromSystem}' to '{toSystem}' is not supported. No coordinates were returned.");
        }

        object? result = null;
        foreach (var methodName in methodNames)
        {
          result = Civil3DCompatibility.InvokeStaticMethod(transformerType, methodName, x, y, z)
            ?? Civil3DCompatibility.InvokeStaticMethod(transformerType, methodName, x, y);
          if (result != null)
          {
            break;
          }
        }

        if (result == null)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.API_ERROR",
            "The coordinate transformer returned no result. No coordinates were returned.");
        }

        var outX = CivilObjectUtils.GetDoubleProperty(result, "X");
        var outY = CivilObjectUtils.GetDoubleProperty(result, "Y");
        var outZ = CivilObjectUtils.GetDoubleProperty(result, "Z") ?? z;
        if (outX == null || outY == null)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.API_ERROR",
            "The coordinate transformer returned an unsupported result type. No coordinates were returned.");
        }

        return new Dictionary<string, object?>
        {
          ["x"] = outX.Value,
          ["y"] = outY.Value,
          ["z"] = outZ,
          ["fromSystem"] = fromSystem,
          ["toSystem"] = toSystem,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception exception)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.API_ERROR",
          $"Coordinate transformation failed: {exception.Message}. No coordinates were returned.");
      }
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static Type? FindCoordinateTransformerType()
  {
    return Civil3DCompatibility.FindLoadedType(
      "Autodesk.Civil.CoordinateTransformer",
      "Autodesk.CSMAP.CsMapCoordinateSystem");
  }
}
