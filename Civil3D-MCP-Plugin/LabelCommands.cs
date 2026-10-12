using System.Collections;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DMcpPlugin;

public static class LabelCommands
{
  public static Task<object?> ListLabelStylesAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styles = GetLabelStyleCollection(civilDoc, objectType);
      var styleItems = styles
        .Select(objectId => transaction.GetObject(objectId, OpenMode.ForRead))
        .Where(style => style != null)
        .Select(style => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(style),
          ["handle"] = style is DBObject dbObject ? CivilObjectUtils.GetHandle(dbObject) : null,
          ["description"] = CivilObjectUtils.GetStringProperty(style, "Description"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["styles"] = styleItems,
      };
    });
  }

  public static Task<object?> ListLabelsAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var target = ResolveTargetObject(civilDoc, transaction, objectType, objectName, OpenMode.ForRead);
      var labels = GetLabelIds(target)
        .Select(objectId => transaction.GetObject(objectId, OpenMode.ForRead))
        .Where(label => label != null)
        .Select(label => new Dictionary<string, object?>
        {
          ["name"] = CivilObjectUtils.GetName(label),
          ["handle"] = label is DBObject dbObject ? CivilObjectUtils.GetHandle(dbObject) : null,
          ["type"] = label?.GetType().Name,
          ["style"] = ResolveObjectName(transaction, GetObjectId(label, "StyleId")),
          ["text"] = CivilObjectUtils.GetStringProperty(label, "TextOverride") ?? CivilObjectUtils.GetStringProperty(label, "Text"),
        })
        .ToList();

      return new Dictionary<string, object?>
      {
        ["objectType"] = objectType,
        ["objectName"] = objectName,
        ["labels"] = labels,
      };
    });
  }

  public static Task<object?> AddLabelAsync(JsonObject? parameters)
  {
    var objectType = PluginRuntime.GetRequiredString(parameters, "objectType");
    var objectName = PluginRuntime.GetRequiredString(parameters, "objectName");
    var labelType = PluginRuntime.GetRequiredString(parameters, "labelType");
    var labelStyle = PluginRuntime.GetOptionalString(parameters, "labelStyle");

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var target = ResolveTargetObject(civilDoc, transaction, objectType, objectName, OpenMode.ForWrite);

      if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "label_set", StringComparison.OrdinalIgnoreCase))
      {
        var labelSetId = LookupUtils.GetAlignmentLabelSetId(civilDoc, transaction, labelStyle);
        if (labelSetId == ObjectId.Null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Alignment label set '{labelStyle}' was not found.");
        }

        // Alignment has no LabelSetId property; the old reflection setter always
        // failed with "does not expose a writable label set property".
        // ImportLabelSet only needs the alignment's id: it erases the existing
        // label groups and creates new ones as separate objects, without
        // write-checking the alignment (IL-verified, live-tested on 2026), so
        // the read-open alignment is enough. It ignores the native result, so
        // the group count is returned to make a silent no-op visible.
        var alignment = (Autodesk.Civil.DatabaseServices.Alignment)target;
        alignment.ImportLabelSet(labelSetId);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, labelSetId),
          ["applied"] = true,
          ["labelGroups"] = alignment.GetAlignmentLabelGroupIds().Count,
        };
      }

      if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "curves", StringComparison.OrdinalIgnoreCase))
      {
        var styleId = FindStyleInCollection(transaction, civilDoc.Styles.LabelStyles.AlignmentLabelStyles.CurveLabelStyles, labelStyle, "alignment curve label");
        var handles = CreateAlignmentCurveLabels((Autodesk.Civil.DatabaseServices.Alignment)target, styleId)
          .Select(labelId => ResolveHandle(transaction, labelId))
          .ToList();
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["handles"] = handles,
          ["created"] = handles.Count,
        };
      }

      if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "station", StringComparison.OrdinalIgnoreCase))
      {
        var station = PluginRuntime.GetOptionalDouble(parameters, "station")
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Alignment station labels require 'station'.");
        // Deliberately NOT FindLabelStyleId here: for "alignment" that returns
        // an alignment label SET style, and StationOffsetLabel.Create rejects
        // it with "Wrong type objectId". A station label needs a station-offset
        // LABEL style, which lives in its own collection.
        var styleId = FindStationOffsetLabelStyleId(civilDoc, transaction, labelStyle);
        var markerStyleId = FindFirstMarkerStyleId(civilDoc);
        var labelId = CreateAlignmentStationLabel(target, styleId, markerStyleId, station);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["station"] = station,
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "label_set", StringComparison.OrdinalIgnoreCase))
      {
        // Verified on Civil 3D 2026 (2026-10-11): only Alignment has ImportLabelSet;
        // Profile and ProfileView expose no label-set member at all.
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_INPUT",
          "Profile label sets cannot be assigned through the Civil 3D .NET API (only alignments have ImportLabelSet). Set them in Profile View Properties > Profiles > Labels, or add profile station labels.");
      }

      if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase) && string.Equals(labelType, "station", StringComparison.OrdinalIgnoreCase))
      {
        var station = PluginRuntime.GetOptionalDouble(parameters, "station")
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Profile station labels require 'station'.");
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateProfileStationLabel(target, styleId, station);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["station"] = station,
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "surface", StringComparison.OrdinalIgnoreCase) && (string.Equals(labelType, "spot_elevation", StringComparison.OrdinalIgnoreCase) || string.Equals(labelType, "spot", StringComparison.OrdinalIgnoreCase)))
      {
        var pointNode = PluginRuntime.GetParameter(parameters, "point") as JsonObject
          ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require 'point'.");
        var point = new Point2d(
          pointNode["x"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require point.x."),
          pointNode["y"]?.GetValue<double>() ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Surface spot labels require point.y.")
        );
        var styleId = FindLabelStyleId(civilDoc, transaction, objectType, labelStyle);
        var labelId = CreateSurfaceSpotLabel(target, styleId, point);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["point"] = new Dictionary<string, object?>
          {
            ["x"] = point.X,
            ["y"] = point.Y,
          },
          ["handle"] = ResolveHandle(transaction, labelId),
          ["created"] = true,
        };
      }

      if (string.Equals(objectType, "pipe", StringComparison.OrdinalIgnoreCase))
      {
        var styleId = FindStyleInCollection(transaction, civilDoc.Styles.LabelStyles.PipeLabelStyles.PlanProfileLabelStyles, labelStyle, "pipe label");
        var labelIds = CreatePipeLabels(transaction, (Autodesk.Civil.DatabaseServices.Pipe)target, styleId, labelType, out var skipped);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["handle"] = ResolveHandle(transaction, labelIds[0]),
          ["handles"] = labelIds.Select(labelId => ResolveHandle(transaction, labelId)).ToList(),
          ["created"] = labelIds.Count,
          ["skipped"] = skipped,
        };
      }

      if (string.Equals(objectType, "structure", StringComparison.OrdinalIgnoreCase))
      {
        var styleId = FindStyleInCollection(transaction, civilDoc.Styles.LabelStyles.StructureLabelStyles.LabelStyles, labelStyle, "structure label");
        // A structure shown in several profile views (a matchline) gets one
        // label per view; return them all so none is left untracked.
        var labelIds = CreateStructureLabels(transaction, (Autodesk.Civil.DatabaseServices.Structure)target, styleId, labelType, out var skipped);
        return new Dictionary<string, object?>
        {
          ["objectType"] = objectType,
          ["objectName"] = objectName,
          ["labelType"] = labelType,
          ["labelStyle"] = ResolveObjectName(transaction, styleId),
          ["handle"] = ResolveHandle(transaction, labelIds[0]),
          ["handles"] = labelIds.Select(labelId => ResolveHandle(transaction, labelId)).ToList(),
          ["created"] = labelIds.Count,
          ["skipped"] = skipped,
        };
      }

      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"addLabel currently supports alignment 'label_set', alignment/profile 'station', alignment 'curves', surface 'spot_elevation', and pipe/structure 'plan' or 'profile' labels. Requested objectType='{objectType}', labelType='{labelType}'."
      );
    });
  }

  private static DBObject ResolveTargetObject(object civilDoc, Transaction transaction, string objectType, string objectName, OpenMode openMode)
  {
    if (string.Equals(objectType, "alignment", StringComparison.OrdinalIgnoreCase))
    {
      // Always opened for read, whatever openMode says: no label path changes
      // the alignment object itself. A branch that does must call UpgradeOpen.
      return CivilObjectUtils.FindAlignmentByName((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc, transaction, objectName);
    }

    if (string.Equals(objectType, "profile", StringComparison.OrdinalIgnoreCase))
    {
      foreach (ObjectId alignmentId in ((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc).GetAlignmentIds())
      {
        var alignment = CivilObjectUtils.GetRequiredObject<Autodesk.Civil.DatabaseServices.Alignment>(transaction, alignmentId, OpenMode.ForRead);
        foreach (ObjectId profileId in alignment.GetProfileIds())
        {
          var profile = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, profileId, openMode);
          if (string.Equals(CivilObjectUtils.GetName(profile), objectName, StringComparison.OrdinalIgnoreCase))
          {
            return profile;
          }
        }
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile '{objectName}' was not found.");
    }

    if (string.Equals(objectType, "surface", StringComparison.OrdinalIgnoreCase))
    {
      return CivilObjectUtils.FindSurfaceByName((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc, transaction, objectName, openMode);
    }

    if (string.Equals(objectType, "pipe", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeObject(civilDoc, transaction, objectName, openMode, true);
    }

    if (string.Equals(objectType, "structure", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeObject(civilDoc, transaction, objectName, openMode, false);
    }

    if (string.Equals(objectType, "pipe_network", StringComparison.OrdinalIgnoreCase))
    {
      return ResolvePipeNetwork(civilDoc, transaction, objectName, openMode);
    }

    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Unsupported label objectType '{objectType}'.");
  }

  private static DBObject ResolvePipeNetwork(object civilDoc, Transaction transaction, string objectName, OpenMode openMode)
  {
    foreach (var objectId in GetPipeNetworkIds(civilDoc))
    {
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
      if (string.Equals(CivilObjectUtils.GetName(network), objectName, StringComparison.OrdinalIgnoreCase))
      {
        return network;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Pipe network '{objectName}' was not found.");
  }

  private static DBObject ResolvePipeObject(object civilDoc, Transaction transaction, string objectName, OpenMode openMode, bool pipe)
  {
    foreach (var networkId in GetPipeNetworkIds(civilDoc))
    {
      var network = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, networkId, OpenMode.ForRead);
      foreach (var objectId in GetPipeRelatedObjectIds(network, pipe))
      {
        var dbObject = CivilObjectUtils.GetRequiredObject<DBObject>(transaction, objectId, openMode);
        if (string.Equals(CivilObjectUtils.GetName(dbObject), objectName, StringComparison.OrdinalIgnoreCase))
        {
          return dbObject;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"{(pipe ? "Pipe" : "Structure")} '{objectName}' was not found.");
  }

  private static IEnumerable<ObjectId> GetPipeNetworkIds(object civilDoc)
  {
    var candidates = new[]
    {
      CivilObjectUtils.InvokeMethod(civilDoc, "GetPipeNetworkIds"),
      GetNamedMemberValue(civilDoc, "PipeNetworkCollection"),
      GetNamedMemberValue(civilDoc, "NetworkCollection"),
      GetNamedMemberValue(civilDoc, "PipeNetworks"),
      GetNamedMemberValue(civilDoc, "Networks"),
    };

    foreach (var candidate in candidates)
    {
      foreach (var objectId in CivilObjectUtils.ToObjectIds(candidate))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static IEnumerable<ObjectId> GetLabelStyleCollection(object civilDoc, string objectType)
  {
    var styles = GetNamedMemberValue(civilDoc, "Styles") ?? throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", "Civil 3D styles collection is not available.");
    var labelSetStyles = GetNamedMemberValue(styles, "LabelSetStyles");

    object? collection = objectType.ToLowerInvariant() switch
    {
      "alignment" => GetNamedMemberValue(labelSetStyles, "AlignmentLabelSetStyles") ?? GetNamedMemberValue(styles, "AlignmentLabelStyles"),
      "profile" => GetNamedMemberValue(labelSetStyles, "ProfileLabelSetStyles") ?? GetNamedMemberValue(styles, "ProfileLabelStyles"),
      "surface" => GetNamedMemberValue(styles, "SurfaceLabelStyles"),
      "pipe_network" => GetNamedMemberValue(styles, "PipeNetworkLabelStyles") ?? GetNamedMemberValue(styles, "PipeLabelStyles"),
      // Pipe and structure label styles live under Styles.LabelStyles; the
      // Styles-level names probed before do not exist, so both came back empty.
      "pipe" => ((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc).Styles.LabelStyles.PipeLabelStyles.PlanProfileLabelStyles,
      "structure" => ((Autodesk.Civil.ApplicationServices.CivilDocument)civilDoc).Styles.LabelStyles.StructureLabelStyles.LabelStyles,
      _ => null,
    };

    if (collection == null)
    {
      return Array.Empty<ObjectId>();
    }

    return CivilObjectUtils.ToObjectIds(collection).Where(objectId => objectId != ObjectId.Null).ToList();
  }

  /// <summary>
  /// First available marker style from civilDoc.Styles.MarkerStyles.
  /// StationOffsetLabel.Create rejects ObjectId.Null for the marker style.
  /// </summary>
  private static ObjectId FindFirstMarkerStyleId(object civilDoc)
  {
    var styles = Civil3DCompatibility.GetPropertyValue(civilDoc, "Styles");
    var markerStyles = Civil3DCompatibility.GetPropertyValue(styles, "MarkerStyles");

    if (markerStyles is IEnumerable enumerable)
    {
      foreach (var item in enumerable)
      {
        if (item is ObjectId objectId && objectId != ObjectId.Null)
        {
          return objectId;
        }
      }
    }

    throw new JsonRpcDispatchException(
      "CIVIL3D.OBJECT_NOT_FOUND",
      "This drawing has no marker style, which a station label requires.");
  }

  /// <summary>
  /// Resolves a station-offset label style from
  /// civilDoc.Styles.LabelStyles.AlignmentLabelStyles.StationOffsetLabelStyles.
  /// Returns the style matching <paramref name="labelStyle"/> when one is
  /// named (an unknown name is rejected with the available names, rather than
  /// silently replaced), otherwise the first available style. These styles are
  /// not the alignment label SET styles that label_list_styles returns for
  /// "alignment".
  /// </summary>
  private static ObjectId FindStationOffsetLabelStyleId(
    object civilDoc, Transaction transaction, string? labelStyle)
  {
    var styles = Civil3DCompatibility.GetPropertyValue(civilDoc, "Styles");
    var labelStyles = Civil3DCompatibility.GetPropertyValue(styles, "LabelStyles");
    var alignmentLabelStyles = Civil3DCompatibility.GetPropertyValue(labelStyles, "AlignmentLabelStyles");
    var collection = Civil3DCompatibility.GetPropertyValue(alignmentLabelStyles, "StationOffsetLabelStyles");

    if (collection is not IEnumerable enumerable)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.OBJECT_NOT_FOUND",
        "Could not reach the alignment station-offset label styles in this drawing.");
    }

    var fallback = ObjectId.Null;
    var available = new List<string>();
    foreach (var item in enumerable)
    {
      if (item is not ObjectId objectId || objectId == ObjectId.Null)
      {
        continue;
      }

      if (fallback == ObjectId.Null)
      {
        fallback = objectId;
      }

      if (string.IsNullOrWhiteSpace(labelStyle))
      {
        continue;
      }

      var name = CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead)) ?? string.Empty;
      if (string.Equals(name, labelStyle, StringComparison.OrdinalIgnoreCase))
      {
        return objectId;
      }
      if (!string.IsNullOrEmpty(name))
      {
        available.Add(name);
      }
    }

    if (fallback == ObjectId.Null)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.OBJECT_NOT_FOUND",
        "This drawing has no alignment station-offset label style to apply.");
    }

    if (!string.IsNullOrWhiteSpace(labelStyle))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.OBJECT_NOT_FOUND",
        $"No alignment station-offset label style named '{labelStyle}'. Available: {string.Join(", ", available)}.");
    }

    return fallback;
  }

  private static ObjectId FindLabelStyleId(object civilDoc, Transaction transaction, string objectType, string? labelStyle)
  {
    var fallback = ObjectId.Null;

    foreach (var objectId in GetLabelStyleCollection(civilDoc, objectType))
    {
      if (objectId == ObjectId.Null)
      {
        continue;
      }

      if (fallback == ObjectId.Null)
      {
        fallback = objectId;
      }

      if (string.IsNullOrWhiteSpace(labelStyle))
      {
        continue;
      }

      var style = transaction.GetObject(objectId, OpenMode.ForRead);
      if (string.Equals(CivilObjectUtils.GetName(style), labelStyle, StringComparison.OrdinalIgnoreCase))
      {
        return objectId;
      }
    }

    return fallback;
  }

  private static ObjectId CreateSurfaceSpotLabel(DBObject surface, ObjectId styleId, Point2d point)
  {
    return TryCreateLabel(
      new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.SurfaceSpotElevationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.SurfaceSpotElevationLabel, AeccDbMgd",
      },
      "surface spot elevation",
      parameters => BuildSurfaceSpotLabelArguments(parameters, surface.ObjectId, styleId, point)
    );
  }

  private static ObjectId CreateProfileStationLabel(DBObject profile, ObjectId styleId, double station)
  {
    return TryCreateLabel(
      new[]
      {
        "Autodesk.Civil.DatabaseServices.Labels.ProfileStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.ProfileStationLabel, AeccDbMgd",
        "Autodesk.Civil.DatabaseServices.Labels.ProfileStationElevationLabel, AeccDbMgd",
      },
      "profile station",
      parameters => BuildProfileStationLabelArguments(parameters, profile.ObjectId, styleId, station)
    );
  }

  private static ObjectId FindStyleInCollection(Transaction transaction, IEnumerable collection, string? styleName, string description)
  {
    var fallback = ObjectId.Null;
    var available = new List<string>();
    foreach (var item in collection)
    {
      if (item is not ObjectId objectId || objectId == ObjectId.Null)
      {
        continue;
      }

      if (fallback == ObjectId.Null)
      {
        fallback = objectId;
      }

      var name = CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead)) ?? string.Empty;
      available.Add(name);
      if (!string.IsNullOrWhiteSpace(styleName) && string.Equals(name, styleName, StringComparison.OrdinalIgnoreCase))
      {
        return objectId;
      }
    }

    if (!string.IsNullOrWhiteSpace(styleName))
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"The {description} style '{styleName}' was not found. Available: {string.Join(", ", available)}.");
    }

    if (fallback == ObjectId.Null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"This drawing has no {description} style.");
    }

    return fallback;
  }

  private static IReadOnlyList<ObjectId> CreateAlignmentCurveLabels(Autodesk.Civil.DatabaseServices.Alignment alignment, ObjectId styleId)
  {
    var labelIds = new List<ObjectId>();
    foreach (Autodesk.Civil.DatabaseServices.AlignmentEntity entity in alignment.Entities)
    {
      if (entity is Autodesk.Civil.DatabaseServices.AlignmentArc arc)
      {
        labelIds.Add(Autodesk.Civil.DatabaseServices.AlignmentCurveLabel.Create(arc, styleId));
        continue;
      }

      // Compound curves (spiral-curve-spiral, reverse and compound arcs, ...)
      // are their own entity types and hold their arcs as sub-entities, which
      // AlignmentCurveLabel.Create has an overload for.
      for (var i = 0; i < entity.SubEntityCount; i++)
      {
        if (entity[i] is Autodesk.Civil.DatabaseServices.AlignmentSubEntityArc subArc)
        {
          labelIds.Add(Autodesk.Civil.DatabaseServices.AlignmentCurveLabel.Create(subArc, styleId));
        }
      }
    }

    if (labelIds.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Alignment '{alignment.Name}' has no arcs to label (neither simple arcs nor arcs inside compound curves).");
    }

    return labelIds;
  }

  // "plan" labels the pipe in plan at its midpoint; "profile" labels it in every
  // profile view that displays it. The type names probed before (PipePlanLabel,
  // Labels.PipeLabel) do not exist, so "profile" silently produced a PLAN label
  // with the default style.
  private static IReadOnlyList<ObjectId> CreatePipeLabels(Transaction transaction, Autodesk.Civil.DatabaseServices.Pipe pipe, ObjectId styleId, string labelType, out int skipped)
  {
    skipped = 0;
    if (string.Equals(labelType, "profile", StringComparison.OrdinalIgnoreCase))
    {
      return CreateProfileLabels(transaction, pipe, "Pipe",
        (partId, profileViewId) => Autodesk.Civil.DatabaseServices.PipeProfileLabel.Create(partId, profileViewId, 0.5, styleId), out skipped);
    }

    if (string.Equals(labelType, "plan", StringComparison.OrdinalIgnoreCase))
    {
      return new[] { Autodesk.Civil.DatabaseServices.PipeLabel.Create(pipe.ObjectId, 0.5, styleId) };
    }

    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Pipe labelType must be 'plan' or 'profile', not '{labelType}'.");
  }

  // Profile labels hang off the part's ProfileViewPart (the part as drawn in
  // profile views), not the pipe or structure itself: passing the model part
  // fails with "An ObjectId of type ProfileViewPart is expected". Civil 3D 2026
  // keeps ONE ProfileViewPart per part, shared by every profile view that shows
  // it, with placeholder extents ((0,0)-(1,0), seen live 2026-10-11 with two
  // views), so views cannot be told apart by extents. Each profile view in
  // GetProfileViewsDisplayingMe is therefore labelled with Part.ProfileViewPartId;
  // when a view refuses it (Create checks that the ProfileViewPart belongs to the
  // view, as it would in a release that keeps one per view), the part's other
  // ProfileViewParts in model space are tried. A view that accepts none of them
  // is skipped and counted.
  private static IReadOnlyList<ObjectId> CreateProfileLabels(
    Transaction transaction, Autodesk.Civil.DatabaseServices.Part part, string kind,
    Func<ObjectId, ObjectId, ObjectId> create, out int skipped)
  {
    skipped = 0;
    var profileViewIds = part.GetProfileViewsDisplayingMe();
    if (profileViewIds.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"{kind} '{part.Name}' is not drawn in any profile view, so it has no profile to label.");
    }

    var candidates = new List<ObjectId>();
    if (!part.ProfileViewPartId.IsNull)
    {
      candidates.Add(part.ProfileViewPartId);
    }

    var scanned = false;
    string? lastError = null;
    var labelIds = new List<ObjectId>();
    foreach (ObjectId profileViewId in profileViewIds)
    {
      var labelId = TryCreateProfileLabel(candidates, profileViewId, create, ref lastError);
      if (labelId.IsNull && !scanned)
      {
        // Scanned only after an attempt failed, and finished (a list) before the
        // next label is created: model space is never appended to mid-enumeration.
        scanned = true;
        var more = FindProfileViewParts(transaction, part).Where(id => !candidates.Contains(id)).ToList();
        candidates.AddRange(more);
        labelId = TryCreateProfileLabel(more, profileViewId, create, ref lastError);
      }

      if (labelId.IsNull)
      {
        skipped++;
        continue;
      }

      labelIds.Add(labelId);
    }

    if (labelIds.Count == 0)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", candidates.Count == 0
        ? $"Civil 3D lists {profileViewIds.Count} profile view(s) displaying {kind.ToLowerInvariant()} '{part.Name}', but it has no profile view part, so no profile label was created."
        : $"{kind} '{part.Name}' is drawn in {profileViewIds.Count} profile view(s), but Civil 3D accepted none of its {candidates.Count} profile view part(s) for them, so no profile label was created{(lastError == null ? "." : $" (last error: {lastError}).")}");
    }

    return labelIds;
  }

  private static ObjectId TryCreateProfileLabel(
    IEnumerable<ObjectId> candidates, ObjectId profileViewId, Func<ObjectId, ObjectId, ObjectId> create, ref string? lastError)
  {
    foreach (var partId in candidates)
    {
      try
      {
        return create(partId, profileViewId);
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
        lastError = exception.Message;
      }
    }

    return ObjectId.Null;
  }

  // Every ProfileViewPart in model space that draws this part.
  private static List<ObjectId> FindProfileViewParts(Transaction transaction, Autodesk.Civil.DatabaseServices.Part part)
  {
    var parts = new List<ObjectId>();
    var modelSpace = (BlockTableRecord)transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(part.Database), OpenMode.ForRead);
    var partClass = Autodesk.AutoCAD.Runtime.RXObject.GetClass(typeof(Autodesk.Civil.DatabaseServices.ProfileViewPart));
    foreach (ObjectId entityId in modelSpace)
    {
      if (entityId.ObjectClass.IsDerivedFrom(partClass)
        && ((Autodesk.Civil.DatabaseServices.ProfileViewPart)transaction.GetObject(entityId, OpenMode.ForRead)).ModelPartId == part.ObjectId)
      {
        parts.Add(entityId);
      }
    }

    return parts;
  }

  private static IReadOnlyList<ObjectId> CreateStructureLabels(Transaction transaction, Autodesk.Civil.DatabaseServices.Structure structure, ObjectId styleId, string labelType, out int skipped)
  {
    skipped = 0;
    if (string.Equals(labelType, "profile", StringComparison.OrdinalIgnoreCase))
    {
      return CreateProfileLabels(transaction, structure, "Structure",
        (partId, profileViewId) => Autodesk.Civil.DatabaseServices.StructureProfileLabel.Create(profileViewId, partId, styleId), out skipped);
    }

    if (string.Equals(labelType, "plan", StringComparison.OrdinalIgnoreCase))
    {
      return new[] { Autodesk.Civil.DatabaseServices.StructureLabel.Create(structure.ObjectId, styleId, structure.Position) };
    }

    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Structure labelType must be 'plan' or 'profile', not '{labelType}'.");
  }

  private static ObjectId CreateAlignmentStationLabel(
    DBObject alignment, ObjectId styleId, ObjectId markerStyleId, double station)
  {
    // None of the three type names this used to probe for exists in AeccDbMgd.
    // There is no AlignmentStationLabel, no Labels.AlignmentStationLabel and no
    // Labels.AlignmentMajorStationLabel, so FindLoadedType returned null for
    // every candidate and the call always ended in "Unable to create a
    // alignment station label with the available Civil 3D API overloads."
    //
    // A single label at one chosen station is a StationOffsetLabel placed at
    // zero offset. (The interval-based alternative, AlignmentStationLabelGroup
    // .Create(styleId, alignmentId, increment), labels the whole alignment at a
    // spacing and so does not match this tool's "station" parameter.)
    if (alignment is not Autodesk.Civil.DatabaseServices.Alignment typedAlignment)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        "Station labels can only be added to an alignment.");
    }

    if (station < typedAlignment.StartingStation || station > typedAlignment.EndingStation)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"Station {station} is outside alignment '{typedAlignment.Name}' " +
        $"({typedAlignment.StartingStation} to {typedAlignment.EndingStation}).");
    }

    // PointLocation returns void and reports the position through ref
    // parameters; it does not return a point.
    double easting = 0.0;
    double northing = 0.0;
    typedAlignment.PointLocation(station, 0.0, ref easting, ref northing);

    // A null marker style is rejected with "Value does not fall within the
    // expected range" -- the overload needs a real marker style, so the caller
    // resolves one from civilDoc.Styles.MarkerStyles.
    return Autodesk.Civil.DatabaseServices.StationOffsetLabel.Create(
      typedAlignment.ObjectId, styleId, markerStyleId, new Point2d(easting, northing));
  }

  private static ObjectId TryCreateLabel(
    IEnumerable<string> candidateTypeNames,
    string labelDescription,
    Func<Civil3DCompatibility.ParameterShape[], object?[]?> argumentBuilder)
  {
    foreach (var typeName in candidateTypeNames)
    {
      var type = Civil3DCompatibility.FindLoadedType(typeName);
      if (type == null)
      {
        continue;
      }

      if (Civil3DCompatibility.TryInvokeStaticOverloads(
        type,
        "Create",
        argumentBuilder,
        out var result,
        out var args))
      {
        var objectId = ExtractCreatedObjectId(result, args ?? Array.Empty<object?>());
        if (objectId != ObjectId.Null)
        {
          return objectId;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED", $"Unable to create a {labelDescription} label with the available Civil 3D API overloads.");
  }

  private static object?[]? BuildSurfaceSpotLabelArguments(Civil3DCompatibility.ParameterShape[] parameters, ObjectId surfaceId, ObjectId styleId, Point2d point)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { surfaceId, styleId });
    var points2d = new Queue<Point2d>(new[] { point });
    var doubles = new Queue<double>(new[] { point.X, point.Y, 0.0, 0.0 });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].Type;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = points2d.Count > 0 ? points2d.Dequeue() : point;
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = new Point3d(point.X, point.Y, 0.0);
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static object?[]? BuildProfileStationLabelArguments(Civil3DCompatibility.ParameterShape[] parameters, ObjectId profileId, ObjectId styleId, double station)
  {
    var args = new object?[parameters.Length];
    var objectIds = new Queue<ObjectId>(new[] { profileId, styleId });
    var doubles = new Queue<double>(new[] { station, 0.0, 0.0 });

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameterType = parameters[i].Type;

      if (parameterType == typeof(ObjectId))
      {
        args[i] = objectIds.Count > 0 ? objectIds.Dequeue() : ObjectId.Null;
        continue;
      }

      if (parameterType == typeof(double))
      {
        args[i] = doubles.Count > 0 ? doubles.Dequeue() : 0.0;
        continue;
      }

      if (parameterType == typeof(Point2d))
      {
        args[i] = new Point2d(0.0, 0.0);
        continue;
      }

      if (parameterType == typeof(Point3d))
      {
        args[i] = Point3d.Origin;
        continue;
      }

      if (parameterType == typeof(bool))
      {
        args[i] = false;
        continue;
      }

      if (parameterType == typeof(int))
      {
        args[i] = 0;
        continue;
      }

      if (parameterType == typeof(string))
      {
        args[i] = string.Empty;
        continue;
      }

      return null;
    }

    return args;
  }

  private static ObjectId ExtractCreatedObjectId(object? result, object?[] args)
  {
    if (result is ObjectId objectId && objectId != ObjectId.Null)
    {
      return objectId;
    }

    var resultId = CivilObjectUtils.GetPropertyValue<ObjectId>(result, "ObjectId");
    if (resultId != ObjectId.Null)
    {
      return resultId;
    }

    foreach (var arg in args)
    {
      var argId = CivilObjectUtils.GetPropertyValue<ObjectId>(arg, "ObjectId");
      if (argId != ObjectId.Null)
      {
        return argId;
      }
    }

    return ObjectId.Null;
  }

  private static string? ResolveHandle(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null)
    {
      return null;
    }

    try
    {
      var dbObject = transaction.GetObject(objectId, OpenMode.ForRead) as DBObject;
      return dbObject == null ? null : CivilObjectUtils.GetHandle(dbObject);
    }
    catch
    {
      return null;
    }
  }

  private static IEnumerable<ObjectId> GetPipeRelatedObjectIds(DBObject network, bool pipe)
  {
    var members = pipe
      ? new[] { "GetPipeIds", "PipeIds", "Pipes", "PipeCollection" }
      : new[] { "GetStructureIds", "StructureIds", "Structures", "StructureCollection" };

    foreach (var name in members)
    {
      var value = CivilObjectUtils.InvokeMethod(network, name) ?? GetNamedMemberValue(network, name);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static IEnumerable<ObjectId> GetLabelIds(object target)
  {
    foreach (var name in new[] { "GetLabelIds", "LabelIds", "Labels" })
    {
      var value = CivilObjectUtils.InvokeMethod(target, name) ?? GetNamedMemberValue(target, name);
      foreach (var objectId in CivilObjectUtils.ToObjectIds(value))
      {
        if (objectId != ObjectId.Null)
        {
          yield return objectId;
        }
      }
    }
  }

  private static object? GetNamedMemberValue(object? value, string memberName)
  {
    if (value == null)
    {
      return null;
    }

    return Civil3DCompatibility.GetPropertyValue(value, memberName)
      ?? Civil3DCompatibility.GetFieldValue(value, memberName);
  }

  private static ObjectId GetObjectId(object? value, string propertyName)
  {
    return CivilObjectUtils.GetPropertyValue<ObjectId>(value, propertyName);
  }

  private static string? ResolveObjectName(Transaction transaction, ObjectId objectId)
  {
    if (objectId == ObjectId.Null)
    {
      return null;
    }

    try
    {
      return CivilObjectUtils.GetName(transaction.GetObject(objectId, OpenMode.ForRead));
    }
    catch
    {
      return null;
    }
  }

}
