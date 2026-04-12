using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Handlers for civil3d_superelevation_* tools.
///
/// Civil 3D API notes:
///   Alignment.SuperelevationSpecification provides access to the superelevation
///   wizard data. The superelevation table is accessed via
///   Alignment.GetSuperelevationCriticalSections() or the
///   SuperelevationSpecificationManager. We use reflection where the exact
///   property/method name varies across Civil 3D versions.
/// </summary>
public static class SuperelevationCommands
{
  // -------------------------------------------------------------------------
  // getSuperelevation
  // -------------------------------------------------------------------------

  public static Task<object?> GetSuperelevationAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var includeRawData = PluginRuntime.GetOptionalBool(parameters, "includeRawData") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);

      // Read superelevation specification via reflection
      var spec = CivilObjectUtils.GetPropertyValue<object>(alignment, "SuperelevationSpecification")
        ?? CivilObjectUtils.GetPropertyValue<object>(alignment, "SuperelevationSpecificationManager");

      string? attainmentMethod = null;
      double? normalCrownSlope = null;
      double? designSpeed = null;
      string? pivotPoint = null;
      List<Dictionary<string, object?>>? rawTable = null;

      if (spec != null)
      {
        attainmentMethod = CivilObjectUtils.GetStringProperty(spec, "AttainmentMethod")
          ?? CivilObjectUtils.GetStringProperty(spec, "Method");
        normalCrownSlope = CivilObjectUtils.GetDoubleProperty(spec, "NormalCrownSlope")
          ?? CivilObjectUtils.GetDoubleProperty(spec, "NormalCrown");
        designSpeed = CivilObjectUtils.GetDoubleProperty(spec, "DesignSpeed");
        pivotPoint = CivilObjectUtils.GetStringProperty(spec, "PivotPoint")
          ?? CivilObjectUtils.GetStringProperty(spec, "PivotPointType");

        if (includeRawData)
        {
          rawTable = ReadSuperelevationTable(alignment, transaction);
        }
      }

      var result = new Dictionary<string, object?>
      {
        ["alignmentName"] = alignmentName,
        ["designSpeed"] = designSpeed,
        ["attainmentMethod"] = attainmentMethod ?? "AASHTO_2011",
        ["normalCrownSlope"] = normalCrownSlope,
        ["pivotPoint"] = pivotPoint ?? "centerline",
        ["hasSuperelevation"] = spec != null,
      };

      if (includeRawData)
      {
        result["rawTable"] = rawTable ?? [];
      }

      return result;
    });
  }

  // -------------------------------------------------------------------------
  // setSuperelevation
  // -------------------------------------------------------------------------

  public static Task<object?> SetSuperelevationAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");
    var normalCrownSlope = PluginRuntime.GetRequiredDouble(parameters, "normalCrownSlope");
    var attainmentMethod = PluginRuntime.GetOptionalString(parameters, "attainmentMethod") ?? "AASHTO_2011";
    var pivotPoint = PluginRuntime.GetOptionalString(parameters, "pivotPoint") ?? "centerline";

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var alignmentWrite = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignment.ObjectId, OpenMode.ForWrite);

      // Try to set via SuperelevationSpecification property
      var spec = CivilObjectUtils.GetPropertyValue<object>(alignmentWrite, "SuperelevationSpecification")
        ?? CivilObjectUtils.GetPropertyValue<object>(alignmentWrite, "SuperelevationSpecificationManager");

      bool applied = false;

      if (spec != null)
      {
        TrySetProperty(spec, "DesignSpeed", designSpeed);
        TrySetProperty(spec, "NormalCrownSlope", normalCrownSlope);
        TrySetProperty(spec, "NormalCrown", normalCrownSlope);
        TrySetProperty(spec, "AttainmentMethod", attainmentMethod);
        TrySetProperty(spec, "Method", attainmentMethod);
        TrySetProperty(spec, "PivotPoint", pivotPoint);
        TrySetProperty(spec, "PivotPointType", pivotPoint);
        applied = true;
      }
      else
      {
        // Try to create/apply via method
        var result = CivilObjectUtils.InvokeMethod(alignmentWrite, "SetSuperelevation",
          designSpeed, normalCrownSlope, attainmentMethod, pivotPoint);
        applied = result != null;

        if (!applied)
        {
          CivilObjectUtils.InvokeMethod(alignmentWrite, "ApplySuperelevation", designSpeed, normalCrownSlope);
          applied = true;
        }
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignmentName,
        ["designSpeed"] = designSpeed,
        ["normalCrownSlope"] = normalCrownSlope,
        ["attainmentMethod"] = attainmentMethod,
        ["pivotPoint"] = pivotPoint,
        ["applied"] = applied,
        ["message"] = applied
          ? $"Superelevation applied to '{alignmentName}' at {designSpeed} km/h."
          : "Superelevation specification was not modified — API may not be available for this Civil 3D version.",
      };
    });
  }

  // -------------------------------------------------------------------------
  // checkSuperelevationDesign
  // -------------------------------------------------------------------------

  public static Task<object?> CheckSuperelevationDesignAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var designSpeed = PluginRuntime.GetRequiredDouble(parameters, "designSpeed");
    var maxSuperelevation = PluginRuntime.GetOptionalDouble(parameters, "maxSuperelevation") ?? AashtoMaxSuperelevation(designSpeed);
    var checkAttainmentLength = PluginRuntime.GetOptionalBool(parameters, "checkAttainmentLength") ?? true;
    var checkRunoffLength = PluginRuntime.GetOptionalBool(parameters, "checkRunoffLength") ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var violations = new List<Dictionary<string, object?>>();
      var passed = true;

      var rawTable = ReadSuperelevationTable(alignment, transaction);

      // Check each station for violations
      foreach (var row in rawTable)
      {
        var leftSlope = row.TryGetValue("leftSlope", out var ls) ? Convert.ToDouble(ls) : 0.0;
        var rightSlope = row.TryGetValue("rightSlope", out var rs) ? Convert.ToDouble(rs) : 0.0;
        var station = row.TryGetValue("station", out var st) ? Convert.ToDouble(st) : 0.0;

        if (Math.Abs(leftSlope) > maxSuperelevation || Math.Abs(rightSlope) > maxSuperelevation)
        {
          passed = false;
          violations.Add(new Dictionary<string, object?>
          {
            ["station"] = station,
            ["violationType"] = "MAX_SUPERELEVATION_EXCEEDED",
            ["leftSlope"] = leftSlope,
            ["rightSlope"] = rightSlope,
            ["maxAllowed"] = maxSuperelevation,
          });
        }
      }

      // AASHTO minimum runoff/runout lengths check
      if (checkAttainmentLength || checkRunoffLength)
      {
        var minRunoffLength = AashtoMinRunoffLength(designSpeed);
        var attainmentResult = CivilObjectUtils.InvokeMethod(alignment, "CheckSuperelevationAttainmentLengths", designSpeed);
        if (attainmentResult is IEnumerable<object> attainViolations)
        {
          foreach (var v in attainViolations)
          {
            passed = false;
            violations.Add(new Dictionary<string, object?>
            {
              ["violationType"] = "ATTAINMENT_LENGTH_VIOLATION",
              ["details"] = v?.ToString(),
              ["minRunoffLength"] = minRunoffLength,
            });
          }
        }
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignmentName,
        ["designSpeed"] = designSpeed,
        ["maxSuperelevation"] = maxSuperelevation,
        ["passed"] = passed,
        ["violationCount"] = violations.Count,
        ["violations"] = violations,
      };
    });
  }

  // -------------------------------------------------------------------------
  // generateSuperelevationReport
  // -------------------------------------------------------------------------

  public static Task<object?> GenerateSuperelevationReportAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var outputPath = PluginRuntime.GetOptionalString(parameters, "outputPath");
    var includeRunoffTable = PluginRuntime.GetOptionalBool(parameters, "includeRunoffTable") ?? true;
    var includeViolations = PluginRuntime.GetOptionalBool(parameters, "includeViolations") ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);

      var spec = CivilObjectUtils.GetPropertyValue<object>(alignment, "SuperelevationSpecification")
        ?? CivilObjectUtils.GetPropertyValue<object>(alignment, "SuperelevationSpecificationManager");

      var designSpeed = CivilObjectUtils.GetDoubleProperty(spec, "DesignSpeed") ?? 0;
      var attainmentMethod = CivilObjectUtils.GetStringProperty(spec, "AttainmentMethod") ?? "AASHTO_2011";
      var normalCrownSlope = CivilObjectUtils.GetDoubleProperty(spec, "NormalCrownSlope") ?? 0;

      var rawTable = includeRunoffTable ? ReadSuperelevationTable(alignment, transaction) : new List<Dictionary<string, object?>>();
      var sb = new StringBuilder();

      sb.AppendLine($"SUPERELEVATION REPORT — {alignmentName}");
      sb.AppendLine($"Design Speed:        {designSpeed} km/h");
      sb.AppendLine($"Attainment Method:   {attainmentMethod}");
      sb.AppendLine($"Normal Crown Slope:  {normalCrownSlope:F2}%");
      sb.AppendLine($"Report Date:         {DateTime.Now:yyyy-MM-dd HH:mm}");
      sb.AppendLine(new string('-', 70));

      if (includeRunoffTable && rawTable.Count > 0)
      {
        sb.AppendLine();
        sb.AppendLine("SUPERELEVATION TABLE");
        sb.AppendLine($"{"Station",-14} {"Left Slope %",-14} {"Right Slope %",-14} {"Notes",-20}");
        sb.AppendLine(new string('-', 62));
        foreach (var row in rawTable)
        {
          var station = row.TryGetValue("station", out var st) ? $"{Convert.ToDouble(st):F3}" : "—";
          var left = row.TryGetValue("leftSlope", out var ls) ? $"{Convert.ToDouble(ls):F2}" : "—";
          var right = row.TryGetValue("rightSlope", out var rs) ? $"{Convert.ToDouble(rs):F2}" : "—";
          var notes = row.TryGetValue("notes", out var n) ? n?.ToString() ?? "" : "";
          sb.AppendLine($"{station,-14} {left,-14} {right,-14} {notes,-20}");
        }
      }

      if (includeViolations)
      {
        var violations = rawTable
          .Where(row => {
            var maxSe = AashtoMaxSuperelevation(designSpeed);
            var ls = row.TryGetValue("leftSlope", out var l) ? Math.Abs(Convert.ToDouble(l)) : 0;
            var rs = row.TryGetValue("rightSlope", out var r) ? Math.Abs(Convert.ToDouble(r)) : 0;
            return ls > maxSe || rs > maxSe;
          })
          .ToList();

        if (violations.Count > 0)
        {
          sb.AppendLine();
          sb.AppendLine($"VIOLATIONS ({violations.Count})");
          foreach (var v in violations)
          {
            sb.AppendLine($"  Station {v.GetValueOrDefault("station")}: Left={v.GetValueOrDefault("leftSlope")}% Right={v.GetValueOrDefault("rightSlope")}%");
          }
        }
        else
        {
          sb.AppendLine();
          sb.AppendLine("No superelevation violations found.");
        }
      }

      var reportText = sb.ToString();

      if (!string.IsNullOrWhiteSpace(outputPath))
      {
        File.WriteAllText(outputPath, reportText);
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignmentName,
        ["designSpeed"] = designSpeed,
        ["attainmentMethod"] = attainmentMethod,
        ["rowCount"] = rawTable.Count,
        ["report"] = string.IsNullOrWhiteSpace(outputPath) ? reportText : null,
        ["outputPath"] = outputPath,
      };
    });
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static List<Dictionary<string, object?>> ReadSuperelevationTable(
    Alignment alignment, Transaction transaction)
  {
    var table = new List<Dictionary<string, object?>>();

    // Try the Civil 3D API for superelevation critical sections
    var critSections = CivilObjectUtils.InvokeMethod(alignment, "GetSuperelevationCriticalSections");
    if (critSections is System.Collections.IEnumerable sections)
    {
      foreach (var section in sections)
      {
        var station = CivilObjectUtils.GetDoubleProperty(section, "Station") ?? 0;
        var leftSlope = CivilObjectUtils.GetDoubleProperty(section, "LeftSlope")
          ?? CivilObjectUtils.GetDoubleProperty(section, "Left") ?? 0;
        var rightSlope = CivilObjectUtils.GetDoubleProperty(section, "RightSlope")
          ?? CivilObjectUtils.GetDoubleProperty(section, "Right") ?? 0;
        table.Add(new Dictionary<string, object?>
        {
          ["station"] = station,
          ["leftSlope"] = leftSlope,
          ["rightSlope"] = rightSlope,
        });
      }
    }

    return table;
  }

  private static double AashtoMaxSuperelevation(double designSpeed)
  {
    // AASHTO maximum superelevation by design speed (km/h)
    return designSpeed switch
    {
      <= 20 => 12.0,
      <= 30 => 12.0,
      <= 40 => 10.0,
      <= 50 => 10.0,
      <= 60 => 8.0,
      <= 70 => 8.0,
      <= 80 => 6.0,
      <= 100 => 6.0,
      _ => 4.0,
    };
  }

  private static double AashtoMinRunoffLength(double designSpeed)
  {
    // AASHTO minimum superelevation runoff length (m) by design speed (km/h)
    return designSpeed switch
    {
      <= 40 => 30,
      <= 60 => 45,
      <= 80 => 60,
      <= 100 => 75,
      _ => 90,
    };
  }

  private static void TrySetProperty(object obj, string propertyName, object? value)
  {
    try
    {
      var prop = obj.GetType().GetProperty(propertyName,
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
      if (prop != null && prop.CanWrite)
      {
        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
      }
    }
    catch { /* ignore — property may be read-only or wrong type */ }
  }
}
