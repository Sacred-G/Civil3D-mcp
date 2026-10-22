using Autodesk.AutoCAD.Geometry;
using System.Text.Json.Nodes;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

/// <summary>
/// Reads AutoCAD / Civil 3D system variables (any name) and writes a short ALLOWLIST of display and drafting variables
/// (e.g. LABELOVERRIDEGLYPHS, ANNOALLVISIBLE, CANNOSCALE). Anything else is refused: this is not a general settings editor,
/// and variables that decide what code or files AutoCAD trusts are never on the list.
/// </summary>
public static class SystemVariableCommands
{
  // name -> why it is settable. Add a variable here only when it is a pure display/drafting setting.
  private static readonly Dictionary<string, string> WritableNames = new(StringComparer.OrdinalIgnoreCase)
  {
    ["LABELOVERRIDEGLYPHS"] = "Civil 3D: show (1) or hide (0) the orange 'i' glyph on labels with overridden text. Display only, never plotted.",
    ["ANNOALLVISIBLE"] = "Show (1) or hide (0) annotative objects that do not support the current annotation scale.",
    ["ANNOAUTOSCALE"] = "Add annotation scales automatically when the annotation scale changes.",
    ["CANNOSCALE"] = "Current annotation scale name, e.g. '1\" = 20''. Copy the exact spelling from acad_list_viewports (annotationScale).",
    ["LWDISPLAY"] = "Show (1) or hide (0) lineweights on screen.",
    ["PDMODE"] = "Point marker style.",
    ["PDSIZE"] = "Point marker size.",
    ["LTSCALE"] = "Global linetype scale.",
    ["PSLTSCALE"] = "Paper-space linetype scaling (0/1).",
    ["MSLTSCALE"] = "Scale linetypes by the annotation scale in model space (0/1).",
    ["SELECTIONPREVIEW"] = "Selection preview highlight (0-3).",
    ["HIGHLIGHT"] = "Highlight selected objects (0/1).",
  };

  public static Task<object?> GetSystemVariableAsync(JsonObject? parameters)
  {
    var name = NormalizeName(PluginRuntime.GetRequiredString(parameters, "name"));

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var value = ReadOrThrow(name);
      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["name"] = name,
        ["value"] = ToJsonValue(value),
        ["type"] = TypeLabel(value),
        ["writable"] = WritableNames.ContainsKey(name),
      });
    });
  }

  public static Task<object?> SetSystemVariableAsync(JsonObject? parameters)
  {
    var name = NormalizeName(PluginRuntime.GetRequiredString(parameters, "name"));
    var rawValue = parameters?["value"];
    var regen = PluginRuntime.GetOptionalBool(parameters, "regen") ?? false;

    if (rawValue is not JsonValue jsonValue)
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "'value' is required and must be a number, string or boolean.");
    if (!WritableNames.ContainsKey(name))
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        $"'{name}' is not on the list of variables this tool may change ({string.Join(", ", WritableNames.Keys.OrderBy(k => k))}). " +
        "Reading any variable is allowed (get_system_variable); anything else must be changed by the user in Civil 3D.");

    return CivilExecution.ExecuteInCommandContextAsync<object?>(() =>
    {
      var previous = ReadOrThrow(name);
      var converted = ConvertLike(previous, jsonValue, name);

      try
      {
        App.SetSystemVariable(name, converted);
      }
      catch (Autodesk.AutoCAD.Runtime.Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR",
          $"AutoCAD refused to set '{name}' to '{converted}' ({ex.ErrorStatus}). The value may be out of range.");
      }

      var current = ReadOrThrow(name);
      var regenerated = false;
      if (regen)
      {
        var doc = App.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
          doc.Editor.Regen();
          regenerated = true;
        }
      }

      return Task.FromResult<object?>(new Dictionary<string, object?>
      {
        ["name"] = name,
        ["previousValue"] = ToJsonValue(previous),
        ["value"] = ToJsonValue(current),
        ["type"] = TypeLabel(current),
        ["changed"] = !Equals(previous, current),
        ["regenerated"] = regenerated,
      });
    });
  }

  private static string NormalizeName(string name)
  {
    var trimmed = name.Trim();
    if (trimmed.Length == 0 || trimmed.Length > 64 || trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '$')))
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'{name}' is not a valid system variable name.");
    return trimmed.ToUpperInvariant();
  }

  private static object ReadOrThrow(string name)
  {
    try
    {
      return App.GetSystemVariable(name)
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"System variable '{name}' returned no value.");
    }
    catch (Autodesk.AutoCAD.Runtime.Exception)
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
        $"System variable '{name}' does not exist in this Civil 3D session. Check the spelling (type the first letters at the command line to see the autocomplete list).");
    }
  }

  // Values must reach SetSystemVariable with the exact CLR type AutoCAD reports (short/int/double/string).
  private static object ConvertLike(object previous, JsonValue value, string name)
  {
    switch (previous)
    {
      case string:
        return value.TryGetValue<string>(out var s) ? s : value.ToString();
      case short:
      case int:
      case double:
        double number;
        if (value.TryGetValue<bool>(out var flag)) number = flag ? 1 : 0;
        else if (value.TryGetValue<double>(out var d)) number = d;
        else if (value.TryGetValue<string>(out var text) && double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)) number = parsed;
        else throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"'{name}' takes a number; got '{value}'.");
        if (previous is short) return Convert.ToInt16(Math.Round(number));
        if (previous is int) return Convert.ToInt32(Math.Round(number));
        return number;
      default:
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
          $"'{name}' holds a {TypeLabel(previous)} value, which this tool does not set. Only number and string variables are supported.");
    }
  }

  private static object? ToJsonValue(object value) => value switch
  {
    Point3d p => new[] { p.X, p.Y, p.Z },
    Point2d p => new[] { p.X, p.Y },
    short or int or long or double or string or bool => value,
    _ => value.ToString(),
  };

  private static string TypeLabel(object value) => value switch
  {
    short => "short",
    int => "int",
    double => "double",
    string => "string",
    Point3d => "point3d",
    Point2d => "point2d",
    _ => value.GetType().Name,
  };
}
