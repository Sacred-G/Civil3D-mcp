using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// QC check commands — read-only analysis tools that inspect Civil 3D objects
/// and return structured findings. Uses reflection for API compatibility.
/// </summary>
public static class QcCommands
{
  // -------------------------------------------------------------------------
  // qcCheckAlignment
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckAlignmentAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var designSpeed = PluginRuntime.GetOptionalDouble(parameters, "designSpeed");
    var checkTangents = PluginRuntime.GetOptionalBool(parameters, "checkTangents") ?? true;
    var checkCurves = PluginRuntime.GetOptionalBool(parameters, "checkCurves") ?? true;
    var checkSpirals = PluginRuntime.GetOptionalBool(parameters, "checkSpirals") ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, name);
        var findings = new List<Dictionary<string, object?>>();

        var entityCollection = CivilObjectUtils.GetPropertyValue<object>(alignment, "Entities");
        if (entityCollection is IEnumerable enumerable)
        {
          var index = 0;
          foreach (var entity in enumerable)
          {
            if (entity == null) { index++; continue; }

            var entityType = CivilObjectUtils.GetStringProperty(entity, "EntityType") ?? entity.GetType().Name;
            var length = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0;
            var startStation = CivilObjectUtils.GetPropertyValue<double?>(entity, "StartStation") ?? 0;
            var endStation = CivilObjectUtils.GetPropertyValue<double?>(entity, "EndStation") ?? 0;
            var isTangent = entityType.IndexOf("tangent", StringComparison.OrdinalIgnoreCase) >= 0
                         || entityType.IndexOf("line", StringComparison.OrdinalIgnoreCase) >= 0;
            var isCurve = entityType.IndexOf("curve", StringComparison.OrdinalIgnoreCase) >= 0
                       || entityType.IndexOf("arc", StringComparison.OrdinalIgnoreCase) >= 0;
            var isSpiral = entityType.IndexOf("spiral", StringComparison.OrdinalIgnoreCase) >= 0;

            if (checkTangents && isTangent)
            {
              if (length <= 0)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "error",
                  ["type"] = "tangent_zero_length",
                  ["entityIndex"] = index,
                  ["entityType"] = entityType,
                  ["startStation"] = startStation,
                  ["endStation"] = endStation,
                  ["message"] = $"Tangent at index {index} (sta {startStation:F2}–{endStation:F2}) has zero or negative length.",
                });
              }
            }

            if (checkCurves && isCurve)
            {
              var radius = CivilObjectUtils.GetPropertyValue<double?>(entity, "Radius") ?? 0;
              if (radius <= 0)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "error",
                  ["type"] = "curve_invalid_radius",
                  ["entityIndex"] = index,
                  ["entityType"] = entityType,
                  ["startStation"] = startStation,
                  ["endStation"] = endStation,
                  ["radius"] = radius,
                  ["message"] = $"Curve at index {index} (sta {startStation:F2}–{endStation:F2}) has invalid radius {radius:F2}.",
                });
              }
              else if (designSpeed.HasValue)
              {
                // AASHTO minimum radius approximation: R_min ≈ V² / (127 * (e + f))
                // Using a simplified check: R_min ≈ V² / 15 for a rough check
                var minRadius = (designSpeed.Value * designSpeed.Value) / 15.0;
                if (radius < minRadius)
                {
                  findings.Add(new Dictionary<string, object?>
                  {
                    ["severity"] = "warning",
                    ["type"] = "curve_radius_below_design_minimum",
                    ["entityIndex"] = index,
                    ["entityType"] = entityType,
                    ["startStation"] = startStation,
                    ["endStation"] = endStation,
                    ["radius"] = radius,
                    ["minimumRadius"] = minRadius,
                    ["designSpeed"] = designSpeed,
                    ["message"] = $"Curve at index {index} (sta {startStation:F2}–{endStation:F2}) has radius {radius:F2}, below minimum {minRadius:F2} for design speed {designSpeed} km/h.",
                  });
                }
              }

              if (length <= 0)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "error",
                  ["type"] = "curve_zero_length",
                  ["entityIndex"] = index,
                  ["entityType"] = entityType,
                  ["startStation"] = startStation,
                  ["endStation"] = endStation,
                  ["message"] = $"Curve at index {index} (sta {startStation:F2}–{endStation:F2}) has zero or negative length.",
                });
              }
            }

            if (checkSpirals && isSpiral)
            {
              var aValue = CivilObjectUtils.GetPropertyValue<double?>(entity, "A")
                ?? CivilObjectUtils.GetPropertyValue<double?>(entity, "SpiralParameter")
                ?? 0;
              if (aValue <= 0)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "warning",
                  ["type"] = "spiral_invalid_parameter",
                  ["entityIndex"] = index,
                  ["entityType"] = entityType,
                  ["startStation"] = startStation,
                  ["endStation"] = endStation,
                  ["spiralParameter"] = aValue,
                  ["message"] = $"Spiral at index {index} (sta {startStation:F2}–{endStation:F2}) has invalid A-parameter {aValue:F4}.",
                });
              }

              if (length <= 0)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "error",
                  ["type"] = "spiral_zero_length",
                  ["entityIndex"] = index,
                  ["entityType"] = entityType,
                  ["startStation"] = startStation,
                  ["endStation"] = endStation,
                  ["message"] = $"Spiral at index {index} (sta {startStation:F2}–{endStation:F2}) has zero or negative length.",
                });
              }
            }

            index++;
          }
        }

        return new Dictionary<string, object?>
        {
          ["alignmentName"] = alignment.Name,
          ["length"] = alignment.Length,
          ["startStation"] = alignment.StartingStation,
          ["endStation"] = alignment.EndingStation,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking alignment '{name}': {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckProfile
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckProfileAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var profileName = PluginRuntime.GetRequiredString(parameters, "profileName");
    var maxGrade = PluginRuntime.GetOptionalDouble(parameters, "maxGrade");
    var minKValue = PluginRuntime.GetOptionalDouble(parameters, "minKValue");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
        var profile = CivilObjectUtils.FindProfileByName(alignment, transaction, profileName, OpenMode.ForRead);
        var findings = new List<Dictionary<string, object?>>();

        // Iterate PVIs
        var pviCollection = CivilObjectUtils.GetPropertyValue<object>(profile, "PVIs");
        if (pviCollection is IEnumerable pviEnumerable)
        {
          var pviList = pviEnumerable.Cast<object?>().Where(p => p != null).ToList();
          for (var i = 0; i < pviList.Count; i++)
          {
            var pvi = pviList[i];
            var station = CivilObjectUtils.GetPropertyValue<double?>(pvi, "Station") ?? 0;
            var elevation = CivilObjectUtils.GetPropertyValue<double?>(pvi, "Elevation") ?? 0;
            var gradeIn = CivilObjectUtils.GetPropertyValue<double?>(pvi, "GradeIn") ?? 0;
            var gradeOut = CivilObjectUtils.GetPropertyValue<double?>(pvi, "GradeOut") ?? 0;
            var kValue = CivilObjectUtils.GetPropertyValue<double?>(pvi, "KValue")
              ?? CivilObjectUtils.GetPropertyValue<double?>(pvi, "K")
              ?? 0;
            var curveLength = CivilObjectUtils.GetPropertyValue<double?>(pvi, "CurveLength") ?? 0;

            if (maxGrade.HasValue)
            {
              var absGradeIn = Math.Abs(gradeIn);
              var absGradeOut = Math.Abs(gradeOut);
              if (absGradeIn > maxGrade.Value)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "warning",
                  ["type"] = "profile_grade_exceeds_maximum",
                  ["pviIndex"] = i,
                  ["station"] = station,
                  ["gradeIn"] = gradeIn,
                  ["maxGrade"] = maxGrade,
                  ["message"] = $"PVI {i} at station {station:F2}: incoming grade {gradeIn:P2} exceeds maximum {maxGrade:P2}.",
                });
              }
              if (absGradeOut > maxGrade.Value)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "warning",
                  ["type"] = "profile_grade_exceeds_maximum",
                  ["pviIndex"] = i,
                  ["station"] = station,
                  ["gradeOut"] = gradeOut,
                  ["maxGrade"] = maxGrade,
                  ["message"] = $"PVI {i} at station {station:F2}: outgoing grade {gradeOut:P2} exceeds maximum {maxGrade:P2}.",
                });
              }
            }

            if (minKValue.HasValue && curveLength > 0)
            {
              if (kValue < minKValue.Value)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "warning",
                  ["type"] = "profile_k_value_below_minimum",
                  ["pviIndex"] = i,
                  ["station"] = station,
                  ["kValue"] = kValue,
                  ["minKValue"] = minKValue,
                  ["message"] = $"PVI {i} at station {station:F2}: K-value {kValue:F2} is below minimum {minKValue:F2}.",
                });
              }
            }
          }
        }

        return new Dictionary<string, object?>
        {
          ["alignmentName"] = alignmentName,
          ["profileName"] = profileName,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking profile '{profileName}': {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckCorridor
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckCorridorAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var corridor = CivilObjectUtils.FindCorridorByName(civilDoc, transaction, name, OpenMode.ForRead);
        var findings = new List<Dictionary<string, object?>>();

        // Check overall corridor validity via reflection
        var isValid = CivilObjectUtils.GetBoolProperty(corridor, "IsValid") ?? true;
        if (!isValid)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "error",
            ["type"] = "corridor_invalid",
            ["corridorName"] = corridor.Name,
            ["message"] = $"Corridor '{corridor.Name}' reports IsValid = false. A rebuild may be required.",
          });
        }

        // Iterate baselines and regions
        var baselines = CivilObjectUtils.GetPropertyValue<object>(corridor, "Baselines");
        if (baselines is IEnumerable baselineEnumerable)
        {
          var baselineIndex = 0;
          foreach (var baseline in baselineEnumerable)
          {
            if (baseline == null) { baselineIndex++; continue; }

            var baselineName = CivilObjectUtils.GetName(baseline) ?? $"Baseline[{baselineIndex}]";
            var regions = CivilObjectUtils.GetPropertyValue<object>(baseline, "BaselineRegions");
            if (regions is IEnumerable regionEnumerable)
            {
              var regionIndex = 0;
              foreach (var region in regionEnumerable)
              {
                if (region == null) { regionIndex++; continue; }

                var regionName = CivilObjectUtils.GetName(region) ?? $"Region[{regionIndex}]";
                var startStation = CivilObjectUtils.GetPropertyValue<double?>(region, "StartStation") ?? 0;
                var endStation = CivilObjectUtils.GetPropertyValue<double?>(region, "EndStation") ?? 0;

                // Zero-length or inverted region
                if (endStation <= startStation)
                {
                  findings.Add(new Dictionary<string, object?>
                  {
                    ["severity"] = "error",
                    ["type"] = "corridor_region_zero_length",
                    ["baseline"] = baselineName,
                    ["region"] = regionName,
                    ["startStation"] = startStation,
                    ["endStation"] = endStation,
                    ["message"] = $"Baseline '{baselineName}', region '{regionName}': end station {endStation:F2} <= start station {startStation:F2}.",
                  });
                }

                // Check assembly assignment
                var assemblyId = CivilObjectUtils.GetPropertyValue<ObjectId?>(region, "AssemblyId");
                if (assemblyId == null || assemblyId == ObjectId.Null)
                {
                  findings.Add(new Dictionary<string, object?>
                  {
                    ["severity"] = "warning",
                    ["type"] = "corridor_region_no_assembly",
                    ["baseline"] = baselineName,
                    ["region"] = regionName,
                    ["startStation"] = startStation,
                    ["endStation"] = endStation,
                    ["message"] = $"Baseline '{baselineName}', region '{regionName}': no assembly assigned.",
                  });
                }

                regionIndex++;
              }
            }

            baselineIndex++;
          }
        }

        return new Dictionary<string, object?>
        {
          ["corridorName"] = corridor.Name,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking corridor '{name}': {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckPipeNetwork
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckPipeNetworkAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var minCover = PluginRuntime.GetOptionalDouble(parameters, "minCover");
    var maxSlope = PluginRuntime.GetOptionalDouble(parameters, "maxSlope");
    var minSlope = PluginRuntime.GetOptionalDouble(parameters, "minSlope");
    var minVelocity = PluginRuntime.GetOptionalDouble(parameters, "minVelocity");
    var maxVelocity = PluginRuntime.GetOptionalDouble(parameters, "maxVelocity");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        // Find the pipe network via reflection (same pattern as PipeNetworkCommands)
        var networkObj = FindPipeNetworkByNameReflection(civilDoc, transaction, name)
          ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{name}' was not found.");

        var findings = new List<Dictionary<string, object?>>();

        // Iterate pipes
        foreach (var pipeId in GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection"))
        {
          var pipe = transaction.GetObject(pipeId, OpenMode.ForRead);
          var pipeName = CivilObjectUtils.GetName(pipe) ?? pipeId.Handle.ToString();

          var slope = GetAnyDouble(pipe, "Slope", "FlowSlope") ?? 0;
          var absSlope = Math.Abs(slope);

          if (maxSlope.HasValue && absSlope > maxSlope.Value)
          {
            findings.Add(new Dictionary<string, object?>
            {
              ["severity"] = "warning",
              ["type"] = "pipe_slope_exceeds_maximum",
              ["pipeName"] = pipeName,
              ["slope"] = slope,
              ["maxSlope"] = maxSlope,
              ["message"] = $"Pipe '{pipeName}': slope {slope:P3} exceeds maximum {maxSlope:P3}.",
            });
          }

          if (minSlope.HasValue && absSlope < minSlope.Value)
          {
            findings.Add(new Dictionary<string, object?>
            {
              ["severity"] = "warning",
              ["type"] = "pipe_slope_below_minimum",
              ["pipeName"] = pipeName,
              ["slope"] = slope,
              ["minSlope"] = minSlope,
              ["message"] = $"Pipe '{pipeName}': slope {slope:P3} is below minimum {minSlope:P3}.",
            });
          }

          if (minCover.HasValue)
          {
            var coverDepth = GetAnyDouble(pipe, "CoverDepth", "MinimumCover", "CoverMin") ?? 0;
            if (coverDepth < minCover.Value)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "warning",
                ["type"] = "pipe_cover_below_minimum",
                ["pipeName"] = pipeName,
                ["coverDepth"] = coverDepth,
                ["minCover"] = minCover,
                ["message"] = $"Pipe '{pipeName}': cover depth {coverDepth:F3} is below minimum {minCover:F3}.",
              });
            }
          }

          if (minVelocity.HasValue || maxVelocity.HasValue)
          {
            var velocity = GetAnyDouble(pipe, "Velocity", "FlowVelocity") ?? 0;
            if (minVelocity.HasValue && velocity < minVelocity.Value && velocity > 0)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "warning",
                ["type"] = "pipe_velocity_below_minimum",
                ["pipeName"] = pipeName,
                ["velocity"] = velocity,
                ["minVelocity"] = minVelocity,
                ["message"] = $"Pipe '{pipeName}': velocity {velocity:F3} is below minimum {minVelocity:F3}.",
              });
            }
            if (maxVelocity.HasValue && velocity > maxVelocity.Value)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "warning",
                ["type"] = "pipe_velocity_exceeds_maximum",
                ["pipeName"] = pipeName,
                ["velocity"] = velocity,
                ["maxVelocity"] = maxVelocity,
                ["message"] = $"Pipe '{pipeName}': velocity {velocity:F3} exceeds maximum {maxVelocity:F3}.",
              });
            }
          }
        }

        return new Dictionary<string, object?>
        {
          ["networkName"] = name,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking pipe network '{name}': {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckSurface
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckSurfaceAsync(JsonObject? parameters)
  {
    var name = PluginRuntime.GetRequiredString(parameters, "name");
    var spikeThreshold = PluginRuntime.GetOptionalDouble(parameters, "spikeThreshold") ?? 10.0;
    var flatTriangleThreshold = PluginRuntime.GetOptionalDouble(parameters, "flatTriangleThreshold") ?? 0.01;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var surface = CivilObjectUtils.FindSurfaceByName(civilDoc, transaction, name, OpenMode.ForRead);
        var findings = new List<Dictionary<string, object?>>();

        var generalProperties = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
        var minElevation = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MinimumElevation") ?? 0;
        var maxElevation = CivilObjectUtils.GetPropertyValue<double?>(generalProperties, "MaximumElevation") ?? 0;
        var numberOfPoints = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfPoints") ?? 0;
        var numberOfTriangles = CivilObjectUtils.GetPropertyValue<int?>(generalProperties, "NumberOfTriangles") ?? 0;

        var elevationRange = maxElevation - minElevation;
        if (elevationRange > spikeThreshold * 10)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "warning",
            ["type"] = "surface_large_elevation_range",
            ["surfaceName"] = name,
            ["elevationRange"] = elevationRange,
            ["minElevation"] = minElevation,
            ["maxElevation"] = maxElevation,
            ["message"] = $"Surface '{name}': elevation range {elevationRange:F2} may indicate spike data (min={minElevation:F2}, max={maxElevation:F2}).",
          });
        }

        if (numberOfPoints == 0)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "error",
            ["type"] = "surface_no_points",
            ["surfaceName"] = name,
            ["message"] = $"Surface '{name}' has no data points.",
          });
        }

        if (numberOfTriangles == 0 && numberOfPoints > 2)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "error",
            ["type"] = "surface_no_triangles",
            ["surfaceName"] = name,
            ["numberOfPoints"] = numberOfPoints,
            ["message"] = $"Surface '{name}' has {numberOfPoints} points but no triangles — surface may need rebuilding.",
          });
        }

        // Check for flat surface (zero elevation range with points present)
        if (numberOfPoints > 2 && numberOfTriangles > 0 && elevationRange < flatTriangleThreshold)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "warning",
            ["type"] = "surface_flat_or_constant_elevation",
            ["surfaceName"] = name,
            ["elevationRange"] = elevationRange,
            ["flatTriangleThreshold"] = flatTriangleThreshold,
            ["message"] = $"Surface '{name}': elevation range {elevationRange:F4} is below flat-triangle threshold {flatTriangleThreshold:F4}. All triangles may be flat.",
          });
        }

        // Check for datum issues (elevations near zero when they shouldn't be)
        if (numberOfPoints > 0 && Math.Abs(minElevation) < 0.001 && Math.Abs(maxElevation) < 0.001)
        {
          findings.Add(new Dictionary<string, object?>
          {
            ["severity"] = "warning",
            ["type"] = "surface_possible_datum_issue",
            ["surfaceName"] = name,
            ["message"] = $"Surface '{name}': all elevations near zero — possible datum/units issue.",
          });
        }

        return new Dictionary<string, object?>
        {
          ["surfaceName"] = name,
          ["numberOfPoints"] = numberOfPoints,
          ["numberOfTriangles"] = numberOfTriangles,
          ["minElevation"] = minElevation,
          ["maxElevation"] = maxElevation,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking surface '{name}': {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckLabels
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckLabelsAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetOptionalString(parameters, "objectType");
    var checkMissing = PluginRuntime.GetOptionalBool(parameters, "checkMissing") ?? true;
    var checkStyleViolations = PluginRuntime.GetOptionalBool(parameters, "checkStyleViolations") ?? true;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var findings = new List<Dictionary<string, object?>>();

        // Check alignments for missing labels
        if (checkMissing && (string.IsNullOrWhiteSpace(objectType) || objectType!.Equals("alignment", StringComparison.OrdinalIgnoreCase)))
        {
          foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
          {
            var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignId, OpenMode.ForRead);
            var labelSetId = CivilObjectUtils.GetPropertyValue<ObjectId?>(alignment, "LabelSetId");
            if (labelSetId == null || labelSetId == ObjectId.Null)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "info",
                ["type"] = "alignment_no_label_set",
                ["objectName"] = alignment.Name,
                ["objectType"] = "alignment",
                ["message"] = $"Alignment '{alignment.Name}' has no label set assigned.",
              });
            }

            if (checkStyleViolations)
            {
              var styleId = CivilObjectUtils.GetPropertyValue<ObjectId?>(alignment, "StyleId");
              if (styleId == null || styleId == ObjectId.Null)
              {
                findings.Add(new Dictionary<string, object?>
                {
                  ["severity"] = "warning",
                  ["type"] = "alignment_no_style",
                  ["objectName"] = alignment.Name,
                  ["objectType"] = "alignment",
                  ["message"] = $"Alignment '{alignment.Name}' has no style assigned.",
                });
              }
            }
          }
        }

        // Check surfaces for missing style
        if (checkStyleViolations && (string.IsNullOrWhiteSpace(objectType) || objectType!.Equals("surface", StringComparison.OrdinalIgnoreCase)))
        {
          foreach (ObjectId surfaceId in civilDoc.GetSurfaceIds())
          {
            var surface = CivilObjectUtils.GetRequiredObject<Surface>(transaction, surfaceId, OpenMode.ForRead);
            if (surface.StyleId == ObjectId.Null)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "warning",
                ["type"] = "surface_no_style",
                ["objectName"] = surface.Name,
                ["objectType"] = "surface",
                ["message"] = $"Surface '{surface.Name}' has no style assigned.",
              });
            }
          }
        }

        // Check corridors for missing style
        if (checkStyleViolations && (string.IsNullOrWhiteSpace(objectType) || objectType!.Equals("corridor", StringComparison.OrdinalIgnoreCase)))
        {
          foreach (ObjectId corridorId in civilDoc.CorridorCollection)
          {
            var corridor = CivilObjectUtils.GetRequiredObject<Corridor>(transaction, corridorId, OpenMode.ForRead);
            if (corridor.StyleId == ObjectId.Null)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "warning",
                ["type"] = "corridor_no_style",
                ["objectName"] = corridor.Name,
                ["objectType"] = "corridor",
                ["message"] = $"Corridor '{corridor.Name}' has no style assigned.",
              });
            }
          }
        }

        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType ?? "all",
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking labels: {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcReportGenerate
  // -------------------------------------------------------------------------

  public static Task<object?> QcReportGenerateAsync(JsonObject? parameters)
  {
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var includeAlignments = PluginRuntime.GetOptionalBool(parameters, "includeAlignments") ?? true;
    var includeProfiles = PluginRuntime.GetOptionalBool(parameters, "includeProfiles") ?? false;
    var includeCorridors = PluginRuntime.GetOptionalBool(parameters, "includeCorridors") ?? true;
    var includePipeNetworks = PluginRuntime.GetOptionalBool(parameters, "includePipeNetworks") ?? true;
    var includeSurfaces = PluginRuntime.GetOptionalBool(parameters, "includeSurfaces") ?? true;
    var includeLabels = PluginRuntime.GetOptionalBool(parameters, "includeLabels") ?? true;
    var includeDrawingStandards = PluginRuntime.GetOptionalBool(parameters, "includeDrawingStandards") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var sb = new StringBuilder();
        sb.AppendLine("=======================================================");
        sb.AppendLine("  CIVIL 3D QC REPORT");
        sb.AppendLine($"  Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("=======================================================");
        sb.AppendLine();

        var totalViolations = 0;
        var checksRun = 0;
        var sectionsIncluded = new List<string>();

        // Alignments
        if (includeAlignments)
        {
          checksRun++;
          sectionsIncluded.Add("alignments");
          sb.AppendLine("--- ALIGNMENT CHECKS ---");
          foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
          {
            var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignId, OpenMode.ForRead);
            var entityCollection = CivilObjectUtils.GetPropertyValue<object>(alignment, "Entities");
            var entityViolations = 0;
            if (entityCollection is IEnumerable enumerable)
            {
              var index = 0;
              foreach (var entity in enumerable)
              {
                if (entity == null) { index++; continue; }
                var length = CivilObjectUtils.GetPropertyValue<double?>(entity, "Length") ?? 0;
                if (length <= 0)
                {
                  sb.AppendLine($"  [WARN] {alignment.Name} entity[{index}]: zero/negative length");
                  entityViolations++;
                }
                index++;
              }
            }
            if (entityViolations == 0)
              sb.AppendLine($"  [OK] {alignment.Name} (length={alignment.Length:F2})");
            totalViolations += entityViolations;
          }
          sb.AppendLine();
        }

        // Surfaces
        if (includeSurfaces)
        {
          checksRun++;
          sectionsIncluded.Add("surfaces");
          sb.AppendLine("--- SURFACE CHECKS ---");
          foreach (ObjectId surfId in civilDoc.GetSurfaceIds())
          {
            var surface = CivilObjectUtils.GetRequiredObject<Surface>(transaction, surfId, OpenMode.ForRead);
            var gp = CivilObjectUtils.InvokeMethod(surface, "GetGeneralProperties");
            var npts = CivilObjectUtils.GetPropertyValue<int?>(gp, "NumberOfPoints") ?? 0;
            var ntri = CivilObjectUtils.GetPropertyValue<int?>(gp, "NumberOfTriangles") ?? 0;
            if (npts == 0)
            {
              sb.AppendLine($"  [ERROR] {surface.Name}: no data points");
              totalViolations++;
            }
            else if (ntri == 0)
            {
              sb.AppendLine($"  [ERROR] {surface.Name}: {npts} pts but no triangles — rebuild required");
              totalViolations++;
            }
            else
            {
              sb.AppendLine($"  [OK] {surface.Name} (pts={npts}, tri={ntri})");
            }
          }
          sb.AppendLine();
        }

        // Corridors
        if (includeCorridors)
        {
          checksRun++;
          sectionsIncluded.Add("corridors");
          sb.AppendLine("--- CORRIDOR CHECKS ---");
          foreach (ObjectId corrId in civilDoc.CorridorCollection)
          {
            var corridor = CivilObjectUtils.GetRequiredObject<Corridor>(transaction, corrId, OpenMode.ForRead);
            var isValid = CivilObjectUtils.GetBoolProperty(corridor, "IsValid") ?? true;
            if (!isValid)
            {
              sb.AppendLine($"  [ERROR] {corridor.Name}: IsValid=false — rebuild required");
              totalViolations++;
            }
            else
            {
              sb.AppendLine($"  [OK] {corridor.Name}");
            }
          }
          sb.AppendLine();
        }

        // Pipe Networks
        if (includePipeNetworks)
        {
          checksRun++;
          sectionsIncluded.Add("pipeNetworks");
          sb.AppendLine("--- PIPE NETWORK CHECKS ---");
          foreach (var networkObj in EnumerateAllPipeNetworks(civilDoc, transaction))
          {
            var networkName = CivilObjectUtils.GetName(networkObj) ?? "unknown";
            var pipeIds = GetChildObjectIds(networkObj, "GetPipeIds", "PipeIds", "Pipes", "PipeCollection").ToList();
            sb.AppendLine($"  [OK] {networkName} ({pipeIds.Count} pipes)");
          }
          sb.AppendLine();
        }

        // Labels
        if (includeLabels)
        {
          checksRun++;
          sectionsIncluded.Add("labels");
          sb.AppendLine("--- LABEL / STYLE CHECKS ---");
          foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
          {
            var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignId, OpenMode.ForRead);
            if (alignment.StyleId == ObjectId.Null)
            {
              sb.AppendLine($"  [WARN] Alignment '{alignment.Name}': no style");
              totalViolations++;
            }
          }
          sb.AppendLine();
        }

        // Drawing standards
        if (includeDrawingStandards)
        {
          checksRun++;
          sectionsIncluded.Add("drawingStandards");
          sb.AppendLine("--- DRAWING STANDARDS ---");
          var lt = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
          if (lt != null)
          {
            foreach (ObjectId layerId in lt)
            {
              var ltr = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
              if (ltr == null) continue;
              if (ltr.Name.Length > 64)
              {
                sb.AppendLine($"  [WARN] Layer '{ltr.Name}': name exceeds 64 characters");
                totalViolations++;
              }
            }
          }
          sb.AppendLine();
        }

        sb.AppendLine("=======================================================");
        sb.AppendLine($"  SUMMARY: {totalViolations} violation(s) across {checksRun} check(s)");
        sb.AppendLine("=======================================================");

        System.IO.File.WriteAllText(outputPath, sb.ToString());

        return new Dictionary<string, object?>
        {
          ["outputPath"] = outputPath,
          ["checksRun"] = checksRun,
          ["totalViolations"] = totalViolations,
          ["sectionsIncluded"] = sectionsIncluded,
          ["reportGenerated"] = true,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error generating QC report: {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // qcCheckDrawingStandards
  // -------------------------------------------------------------------------

  public static Task<object?> QcCheckDrawingStandardsAsync(JsonObject? parameters)
  {
    var layerPrefix = PluginRuntime.GetOptionalString(parameters, "layerPrefix");
    var checkLineweights = PluginRuntime.GetOptionalBool(parameters, "checkLineweights") ?? false;
    var checkColors = PluginRuntime.GetOptionalBool(parameters, "checkColors") ?? false;

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      try
      {
        var findings = new List<Dictionary<string, object?>>();

        var lt = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
        if (lt == null)
        {
          return new Dictionary<string, object?>
          {
            ["findings"] = findings,
            ["totalViolations"] = 0,
          };
        }

        foreach (ObjectId layerId in lt)
        {
          var ltr = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
          if (ltr == null) continue;

          var layerName = ltr.Name;

          // Skip "0" and "Defpoints" reserved layers
          if (layerName == "0" || layerName.Equals("Defpoints", StringComparison.OrdinalIgnoreCase))
            continue;

          // Prefix check
          if (!string.IsNullOrWhiteSpace(layerPrefix))
          {
            if (!layerName.StartsWith(layerPrefix, StringComparison.OrdinalIgnoreCase))
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "info",
                ["type"] = "layer_prefix_violation",
                ["layerName"] = layerName,
                ["expectedPrefix"] = layerPrefix,
                ["message"] = $"Layer '{layerName}' does not start with required prefix '{layerPrefix}'.",
              });
            }
          }

          // Name length check
          if (layerName.Length > 64)
          {
            findings.Add(new Dictionary<string, object?>
            {
              ["severity"] = "warning",
              ["type"] = "layer_name_too_long",
              ["layerName"] = layerName,
              ["nameLength"] = layerName.Length,
              ["message"] = $"Layer '{layerName}' has a name that exceeds 64 characters.",
            });
          }

          // Check for spaces in layer name (common standard violation)
          if (layerName.Contains(' '))
          {
            findings.Add(new Dictionary<string, object?>
            {
              ["severity"] = "info",
              ["type"] = "layer_name_contains_space",
              ["layerName"] = layerName,
              ["message"] = $"Layer '{layerName}' contains spaces — may cause compatibility issues.",
            });
          }

          // Lineweight check
          if (checkLineweights)
          {
            var lineweight = ltr.LineWeight;
            if (lineweight == LineWeight.ByLayer)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "info",
                ["type"] = "layer_lineweight_by_layer",
                ["layerName"] = layerName,
                ["message"] = $"Layer '{layerName}': lineweight set to ByLayer — consider explicit assignment for standards compliance.",
              });
            }
          }

          // Color check
          if (checkColors)
          {
            var color = ltr.Color;
            if (color.IsByLayer || color.IsByBlock)
            {
              findings.Add(new Dictionary<string, object?>
              {
                ["severity"] = "info",
                ["type"] = "layer_color_by_layer_or_block",
                ["layerName"] = layerName,
                ["colorName"] = color.ColorNameForDisplay,
                ["message"] = $"Layer '{layerName}': color is ByLayer/ByBlock — may not meet drawing standard requirements.",
              });
            }
          }

          // Frozen/off layer check
          if (ltr.IsFrozen)
          {
            findings.Add(new Dictionary<string, object?>
            {
              ["severity"] = "info",
              ["type"] = "layer_frozen",
              ["layerName"] = layerName,
              ["message"] = $"Layer '{layerName}' is frozen.",
            });
          }
        }

        return new Dictionary<string, object?>
        {
          ["layerPrefix"] = layerPrefix,
          ["findings"] = findings,
          ["totalViolations"] = findings.Count,
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new JsonRpcDispatchException("CIVIL3D.QC_ERROR", $"Error checking drawing standards: {ex.Message}");
      }
    });
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private static DBObject? FindPipeNetworkByNameReflection(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction,
    string name)
  {
    foreach (var candidateProp in new[] { "PipeNetworkCollection", "NetworkCollection", "PipeNetworks", "Networks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null) continue;
        var obj = transaction.GetObject(objectId, OpenMode.ForRead);
        if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
          return obj;
      }
    }

    // Try GetPipeNetworkIds method
    var methodResult = CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds");
    foreach (var objectId in CivilObjectUtils.ToObjectIds(methodResult))
    {
      if (objectId == ObjectId.Null) continue;
      var obj = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(obj), name, StringComparison.OrdinalIgnoreCase))
        return obj;
    }

    return null;
  }

  private static IEnumerable<DBObject> EnumerateAllPipeNetworks(
    Autodesk.Civil.ApplicationServices.CivilDocument civilDoc,
    Transaction transaction)
  {
    foreach (var candidateProp in new[] { "PipeNetworkCollection", "NetworkCollection", "PipeNetworks", "Networks" })
    {
      var prop = civilDoc.GetType().GetProperty(candidateProp, BindingFlags.Public | BindingFlags.Instance);
      var collection = prop?.GetValue(civilDoc);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(collection))
      {
        if (objectId == ObjectId.Null) continue;
        yield return transaction.GetObject(objectId, OpenMode.ForRead);
      }
    }
  }

  private static IEnumerable<ObjectId> GetChildObjectIds(DBObject owner, params string[] memberNames)
  {
    foreach (var memberName in memberNames)
    {
      var methodResult = CivilObjectUtils.InvokeMethod(owner, memberName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(methodResult))
      {
        if (objectId != ObjectId.Null) yield return objectId;
      }

      var propVal = CivilObjectUtils.GetPropertyValue<object>(owner, memberName);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(propVal))
      {
        if (objectId != ObjectId.Null) yield return objectId;
      }
    }
  }

  private static double? GetAnyDouble(object? value, params string[] propertyNames)
  {
    foreach (var propertyName in propertyNames)
    {
      var v = CivilObjectUtils.GetPropertyValue<double?>(value, propertyName);
      if (v.HasValue) return v.Value;
    }
    return null;
  }
}
