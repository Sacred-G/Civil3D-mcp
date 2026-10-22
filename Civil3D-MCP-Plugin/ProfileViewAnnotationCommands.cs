using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using AcDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using AcEntity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Civil3DMcpPlugin;

// Reads a profile view's presentation (view style, bands, part/station-elevation labels and profile
// label groups) and replays it on another view. Used to rebuild an engineer's profile sheet on a new
// drawing: labels are matched to our parts by plan position, so the two designs only need to share
// coordinates, not object names.
public static class ProfileViewAnnotationCommands
{
  private const double DefaultMatchDistance = 1.0;

  // ─── profileViewStyles ─────────────────────────────────────────────────────

  public static Task<object?> ProfileViewStylesAsync(JsonObject? parameters)
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var styles = civilDoc.Styles;
      var labels = styles.LabelStyles;
      return new Dictionary<string, object?>
      {
        ["profileViewStyles"] = Names(transaction, styles.ProfileViewStyles),
        ["profileViewBandSetStyles"] = Names(transaction, styles.ProfileViewBandSetStyles),
        ["markerStyles"] = Names(transaction, styles.MarkerStyles),
        ["labelStyles"] = new Dictionary<string, object?>
        {
          ["StructureProfileLabel"] = Names(transaction, labels.StructureLabelStyles.LabelStyles),
          ["PipeProfileLabel"] = Names(transaction, labels.PipeLabelStyles.PlanProfileLabelStyles),
          ["PressurePipeProfileLabel"] = Names(transaction, labels.GetPressurePipeLabelStyles().PlanProfileLabelStyles),
          ["PressureFittingProfileLabel"] = Names(transaction, labels.GetPressureFittingLabelStyles().LabelStyles),
          ["PressureAppurtenanceProfileLabel"] = Names(transaction, labels.GetPressureAppurtenanceLabelStyles().LabelStyles),
          ["StationElevationLabel"] = Names(transaction, labels.ProfileViewLabelStyles.StationElevationLabelStyles),
          ["ProfileStationLabelGroup"] = Names(transaction, labels.ProfileLabelStyles.MajorStationLabelStyles),
          ["ProfileMinorStationLabelGroup"] = Names(transaction, labels.ProfileLabelStyles.MinorStationLabelStyles),
        },
      };
    });
  }

  // ─── profileViewAnnotations ────────────────────────────────────────────────

  public static Task<object?> ProfileViewAnnotationsAsync(JsonObject? parameters)
  {
    var profileViewName = PluginRuntime.GetOptionalString(parameters, "profileViewName");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var views = new List<Dictionary<string, object?>>();
      foreach (var view in EnumerateProfileViews(database, transaction))
      {
        if (!string.IsNullOrWhiteSpace(profileViewName)
          && !string.Equals(view.Name, profileViewName, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        views.Add(DescribeAnnotations(view, transaction));
      }

      if (views.Count == 0 && !string.IsNullOrWhiteSpace(profileViewName))
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile view '{profileViewName}' was not found in model space.");
      }

      var result = new Dictionary<string, object?> { ["profileViews"] = views };
      if (string.IsNullOrWhiteSpace(profileViewName))
      {
        result["planLabels"] = DescribePlanLabels(database, transaction);
      }

      return result;
    });
  }

  // GetLabelIds() only covers labels owned by the view (station/elevation, depth); labels on parts
  // drawn in the view come from the part-label lists and from each drawn pressure part.
  private static List<ObjectId> CollectViewLabelIds(ProfileView view, Transaction transaction)
  {
    var labelIds = new List<ObjectId>();
    void Collect(ObjectIdCollection ids)
    {
      foreach (ObjectId id in ids)
      {
        if (!labelIds.Contains(id))
        {
          labelIds.Add(id);
        }
      }
    }

    Collect(view.GetLabelIds());
    Collect(view.GetAvailableStructureProfileLabelIds());
    Collect(view.GetAvailablePipeProfileLabelIds());
    Collect(view.GetAvailableSpanningPipeProfileLabelIds());
    foreach (ObjectId drawnId in view.GetPressureNetworkPartsInGraph())
    {
      if (transaction.GetObject(drawnId, OpenMode.ForRead) is ProfileViewPart drawn)
      {
        Collect(drawn.GetLabelIds());
      }
    }

    return labelIds;
  }

  private static Dictionary<string, object?> DescribeAnnotations(ProfileView view, Transaction transaction)
  {
    var labels = new List<Dictionary<string, object?>>();
    var labelIds = CollectViewLabelIds(view, transaction);

    foreach (var labelId in labelIds)
    {
      if (!labelId.IsErased && transaction.GetObject(labelId, OpenMode.ForRead) is Label label)
      {
        labels.Add(DescribeLabel(label, transaction));
      }
    }

    var groups = new List<Dictionary<string, object?>>();
    var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, view.AlignmentId, OpenMode.ForRead);
    foreach (ObjectId profileId in alignment.GetProfileIds())
    {
      var profile = CivilObjectUtils.GetRequiredObject<Profile>(transaction, profileId, OpenMode.ForRead);
      var groupIds = ProfileLabelGroup.GetAvailableLabelGroupIds(RXObject.GetClass(typeof(ProfileLabelGroup)), view.ObjectId, profileId, true);
      foreach (ObjectId groupId in groupIds)
      {
        if (transaction.GetObject(groupId, OpenMode.ForRead) is not LabelGroup group)
        {
          continue;
        }

        var item = new Dictionary<string, object?>
        {
          ["handle"] = group.Handle.ToString(),
          ["type"] = group.GetType().Name,
          ["style"] = group.StyleName,
          ["profileName"] = profile.Name,
        };
        if (group is ProfileStationLabelGroup stationGroup)
        {
          item["increment"] = stationGroup.Increment;
        }

        groups.Add(item);
      }
    }

    return new Dictionary<string, object?>
    {
      ["profileViewName"] = view.Name,
      ["handle"] = view.Handle.ToString(),
      ["alignmentName"] = alignment.Name,
      ["style"] = view.StyleName,
      ["bands"] = new Dictionary<string, object?>
      {
        ["top"] = DescribeBands(view.Bands.GetTopBandItems(), transaction),
        ["bottom"] = DescribeBands(view.Bands.GetBottomBandItems(), transaction),
      },
      ["labels"] = labels,
      ["labelGroups"] = groups,
    };
  }

  private static List<Dictionary<string, object?>> DescribePlanLabels(Database database, Transaction transaction)
  {
    var labels = new List<Dictionary<string, object?>>();
    foreach (ObjectId id in NoteLabel.GetAvailableLabelIds(database))
    {
      if (!id.IsErased && transaction.GetObject(id, OpenMode.ForRead) is NoteLabel note)
      {
        var item = DescribeLabel(note, transaction);
        // The note's anchor is where it was created; a dragged note keeps the anchor and moves the text.
        var anchor = note.Dragged ? note.LabelLocation - note.DraggedOffset : note.LabelLocation;
        item["anchor"] = XY(anchor);
        labels.Add(item);
      }
    }

    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
    var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
    var stationOffsetClass = RXObject.GetClass(typeof(StationOffsetLabel));
    foreach (ObjectId id in modelSpace)
    {
      if (id.ObjectClass.IsDerivedFrom(stationOffsetClass) && transaction.GetObject(id, OpenMode.ForRead) is StationOffsetLabel stationOffset)
      {
        var item = DescribeLabel(stationOffset, transaction);
        item["alignmentName"] = transaction.GetObject(stationOffset.FeatureId, OpenMode.ForRead) is Alignment alignment ? alignment.Name : null;
        item["location"] = new Dictionary<string, object?> { ["x"] = stationOffset.Location.X, ["y"] = stationOffset.Location.Y };
        labels.Add(item);
      }
    }

    return labels;
  }

  private static List<Dictionary<string, object?>> DescribeBands(ProfileViewBandItemCollection items, Transaction transaction)
  {
    var bands = new List<Dictionary<string, object?>>();
    for (var index = 0; index < items.Count; index++)
    {
      var band = items[index];
      bands.Add(new Dictionary<string, object?>
      {
        ["bandType"] = band.BandType.ToString(),
        ["style"] = NameOf(transaction, band.BandStyleId),
        ["profile1"] = NameOf(transaction, band.Profile1Id),
        ["profile2"] = NameOf(transaction, band.Profile2Id),
        ["majorInterval"] = band.MajorInterval,
        ["minorInterval"] = band.MinorInterval,
        ["gap"] = band.Gap,
      });
    }

    return bands;
  }

  private static Dictionary<string, object?> DescribeLabel(Label label, Transaction transaction)
  {
    var item = new Dictionary<string, object?>
    {
      ["handle"] = label.Handle.ToString(),
      ["type"] = label.GetType().Name,
      ["style"] = label.StyleName,
      ["layer"] = label.Layer,
      ["dragged"] = label.Dragged,
      ["labelLocation"] = XY(label.LabelLocation),
    };

    var feature = ResolveModelPart(transaction, label.FeatureId);
    if (feature != null)
    {
      item["featureType"] = feature.GetType().Name;
      item["featureName"] = CivilObjectUtils.GetName(feature);
      var position = PartPosition(feature);
      if (position.HasValue)
      {
        item["featureX"] = position.Value.X;
        item["featureY"] = position.Value.Y;
      }
    }

    switch (label)
    {
      case PipeProfileLabel pipeLabel:
        item["ratio"] = pipeLabel.Ratio;
        break;
      case PressurePipeProfileLabel pressurePipeLabel:
        item["ratio"] = pressurePipeLabel.Ratio;
        break;
      case PressureFittingProfileLabel fittingLabel:
        item["ratio"] = fittingLabel.Ratio;
        item["direction"] = XY(fittingLabel.Direction);
        break;
      case PressureAppurtenanceProfileLabel appurtenanceLabel:
        item["ratio"] = appurtenanceLabel.Ratio;
        item["direction"] = XY(appurtenanceLabel.Direction);
        break;
      case StationElevationLabel stationLabel:
        item["station"] = stationLabel.Station;
        item["elevation"] = stationLabel.Elevation;
        item["markerStyle"] = NameOf(transaction, stationLabel.AnchorMarkerStyleId);
        break;
      case NoteLabel:
      case StationOffsetLabel:
        item["markerStyle"] = NameOf(transaction, label.AnchorMarkerStyleId);
        break;
    }

    var overrides = new List<Dictionary<string, object?>>();
    var componentIds = label.GetTextComponentIds();
    for (var index = 0; index < componentIds.Count; index++)
    {
      if (label.IsTextComponentOverriden(componentIds[index]))
      {
        overrides.Add(new Dictionary<string, object?> { ["index"] = index, ["text"] = label.GetTextComponentOverride(componentIds[index]) });
      }
    }

    if (overrides.Count > 0)
    {
      item["overrides"] = overrides;
    }

    return item;
  }

  // ─── profileViewApplyAnnotations ───────────────────────────────────────────

  public static Task<object?> ProfileViewApplyAnnotationsAsync(JsonObject? parameters)
  {
    // profileViewName is optional: without it only the plan labels (NoteLabel / StationOffsetLabel, model space) are applied, which is what a
    // Fase 1 sheet needs (EOP / R/W notes and the alignment start/end labels have no profile view).
    var profileViewName = PluginRuntime.GetOptionalString(parameters, "profileViewName");
    var styleName = PluginRuntime.GetOptionalString(parameters, "style");
    var bandSetStyleName = PluginRuntime.GetOptionalString(parameters, "bandSetStyle");
    var clearBands = PluginRuntime.GetOptionalBool(parameters, "clearBands") ?? false;
    var maxDistance = PluginRuntime.GetOptionalDouble(parameters, "maxMatchDistance") ?? DefaultMatchDistance;
    var labelSpecs = PluginRuntime.GetParameter(parameters, "labels") as JsonArray;
    var groupSpecs = PluginRuntime.GetParameter(parameters, "labelGroups") as JsonArray;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      if (string.IsNullOrWhiteSpace(profileViewName))
      {
        if (!string.IsNullOrWhiteSpace(styleName) || !string.IsNullOrWhiteSpace(bandSetStyleName) || clearBands || (groupSpecs?.Count ?? 0) > 0)
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "style, bandSetStyle, clearBands and labelGroups need profileViewName; without it only plan labels (NoteLabel, StationOffsetLabel) are applied.");
        }

        return ApplyPlanLabelsOnly(labelSpecs, database, transaction, civilDoc.Styles, civilDoc.Styles.LabelStyles);
      }

      var found = EnumerateProfileViews(database, transaction)
        .FirstOrDefault(view => string.Equals(view.Name, profileViewName, StringComparison.OrdinalIgnoreCase))
        ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile view '{profileViewName}' was not found in model space.");
      var view = CivilObjectUtils.GetRequiredObject<ProfileView>(transaction, found.ObjectId, OpenMode.ForWrite);
      var styles = civilDoc.Styles;
      var labelStyles = styles.LabelStyles;
      var result = new Dictionary<string, object?> { ["profileViewName"] = view.Name };

      if (!string.IsNullOrWhiteSpace(styleName))
      {
        view.StyleId = RequireStyle(transaction, styleName!, "profile view", styles.ProfileViewStyles);
        result["style"] = view.StyleName;
      }

      if (!string.IsNullOrWhiteSpace(bandSetStyleName))
      {
        view.Bands.ImportBandSetStyle(RequireStyle(transaction, bandSetStyleName!, "profile view band set", styles.ProfileViewBandSetStyles));
        result["bandSetStyle"] = bandSetStyleName;
      }

      if (clearBands)
      {
        var top = view.Bands.GetTopBandItems();
        if (top != null)
        {
          top.RemoveAll();
          view.Bands.SetTopBandItems(top);
        }

        var bottom = view.Bands.GetBottomBandItems();
        if (bottom != null)
        {
          bottom.RemoveAll();
          view.Bands.SetBottomBandItems(bottom);
        }
        result["bandsCleared"] = true;
      }

      var parts = CollectParts(civilDoc, transaction);
      var existingStationLabels = new List<StationElevationLabel>();
      foreach (ObjectId id in view.GetLabelIds())
      {
        if (!id.IsErased && transaction.GetObject(id, OpenMode.ForRead) is StationElevationLabel existing)
        {
          existingStationLabels.Add(existing);
        }
      }

      var planLabels = CollectPlanLabels(database, transaction);
      var labelResults = new List<Dictionary<string, object?>>();
      for (var index = 0; index < (labelSpecs?.Count ?? 0); index++)
      {
        var spec = labelSpecs![index] as JsonObject;
        var entry = new Dictionary<string, object?> { ["index"] = index };
        try
        {
          if (spec == null)
          {
            throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "label spec must be an object.");
          }

          var labelId = FindExistingStationLabel(spec, existingStationLabels);
          if (labelId.IsNull)
          {
            // Plan labels (NoteLabel/StationOffsetLabel) live in model space, not in the view: re-applying the same
            // labels moves/re-texts the existing ones instead of stacking a duplicate on the same anchor.
            labelId = FindExistingPlanLabel(spec, planLabels);
            if (labelId.IsNull)
            {
              // Same for part labels already drawn in this view (re-applying a corrected override edits, not duplicates).
              labelId = FindExistingPartLabel(spec, view, transaction, Math.Min(maxDistance, 0.5));
            }

            if (!labelId.IsNull)
            {
              entry["reused"] = true;
            }
          }
          else
          {
            entry["reused"] = true;
            var reused = CivilObjectUtils.GetRequiredObject<Label>(transaction, labelId, OpenMode.ForWrite);
            var wantedStyle = PluginRuntime.GetRequiredString(spec, "style");
            if (!string.Equals(reused.StyleName, wantedStyle, StringComparison.OrdinalIgnoreCase))
            {
              reused.StyleId = RequireStyle(transaction, wantedStyle, "station elevation label", labelStyles.ProfileViewLabelStyles.StationElevationLabelStyles);
            }
          }

          if (labelId.IsNull)
          {
            labelId = CreateLabel(spec, view, transaction, database, labelStyles, styles, parts, maxDistance, entry);
          }

          ApplyLabelPlacement(spec, labelId, transaction, database);
          entry["handle"] = labelId.Handle.ToString();
        }
        catch (System.Exception ex)
        {
          entry["error"] = ex.Message;
        }

        labelResults.Add(entry);
      }

      var groupResults = new List<Dictionary<string, object?>>();
      var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, view.AlignmentId, OpenMode.ForRead);
      var majorByProfile = new Dictionary<ObjectId, ObjectId>();
      for (var index = 0; index < (groupSpecs?.Count ?? 0); index++)
      {
        var spec = groupSpecs![index] as JsonObject;
        var entry = new Dictionary<string, object?> { ["index"] = index };
        try
        {
          var type = PluginRuntime.GetRequiredString(spec, "type");
          var profileId = FindProfileId(alignment, transaction, PluginRuntime.GetRequiredString(spec, "profileName"));
          var increment = PluginRuntime.GetRequiredDouble(spec, "increment");
          var style = PluginRuntime.GetRequiredString(spec, "style");
          ObjectId groupId;
          if (type == nameof(ProfileMinorStationLabelGroup))
          {
            if (!majorByProfile.TryGetValue(profileId, out var majorId))
            {
              throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "a minor station label group needs a major station group for the same profile earlier in labelGroups.");
            }

            groupId = ProfileMinorStationLabelGroup.Create(RequireStyle(transaction, style, "minor station label", labelStyles.ProfileLabelStyles.MinorStationLabelStyles), majorId, increment);
          }
          else if (type == nameof(ProfileStationLabelGroup))
          {
            groupId = ProfileStationLabelGroup.CreateMajor(view.ObjectId, profileId, RequireStyle(transaction, style, "major station label", labelStyles.ProfileLabelStyles.MajorStationLabelStyles), increment);
            majorByProfile[profileId] = groupId;
          }
          else
          {
            throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"label group type '{type}' is not supported; only ProfileStationLabelGroup and ProfileMinorStationLabelGroup.");
          }

          entry["handle"] = groupId.Handle.ToString();
        }
        catch (System.Exception ex)
        {
          entry["error"] = ex.Message;
        }

        groupResults.Add(entry);
      }

      result["labels"] = labelResults;
      result["labelGroups"] = groupResults;
      result["createdLabels"] = labelResults.Count(entry => entry.ContainsKey("handle"));
      result["failedLabels"] = labelResults.Count(entry => entry.ContainsKey("error"));
      return result;
    });
  }

  private static ObjectId CreateLabel(
    JsonObject spec,
    ProfileView view,
    Transaction transaction,
    Database database,
    LabelStylesRoot labelStyles,
    StylesRoot styles,
    List<(AcEntity Part, Point2d Position)> parts,
    double maxDistance,
    Dictionary<string, object?> entry)
  {
    var type = PluginRuntime.GetRequiredString(spec, "type");
    var style = PluginRuntime.GetRequiredString(spec, "style");
    var ratio = PluginRuntime.GetOptionalDouble(spec, "ratio") ?? 0.5;

    if (type == nameof(NoteLabel) || type == nameof(StationOffsetLabel))
    {
      return CreatePlanLabel(type, spec, style, transaction, labelStyles, styles);
    }

    if (type == nameof(StationElevationLabel))
    {
      var styleId = RequireStyle(transaction, style, "station elevation label", labelStyles.ProfileViewLabelStyles.StationElevationLabelStyles);
      var markerName = PluginRuntime.GetOptionalString(spec, "markerStyle");
      var markerId = string.IsNullOrWhiteSpace(markerName) ? ObjectId.Null : FindStyle(transaction, markerName!, styles.MarkerStyles);
      return StationElevationLabel.Create(view.ObjectId, styleId, markerId,
        PluginRuntime.GetRequiredDouble(spec, "station"), PluginRuntime.GetRequiredDouble(spec, "elevation"));
    }

    var target = new Point2d(PluginRuntime.GetRequiredDouble(spec, "featureX"), PluginRuntime.GetRequiredDouble(spec, "featureY"));
    Func<AcEntity, bool> kind = type switch
    {
      nameof(StructureProfileLabel) => part => part is Structure,
      nameof(PipeProfileLabel) => part => part is Pipe,
      nameof(PressurePipeProfileLabel) => part => part is PressurePipe,
      nameof(PressureFittingProfileLabel) => part => part is PressureFitting,
      nameof(PressureAppurtenanceProfileLabel) => part => part is PressureAppurtenance,
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"label type '{type}' is not supported."),
    };

    var match = parts.Where(candidate => kind(candidate.Part))
      .Select(candidate => (candidate.Part, Distance: candidate.Position.GetDistanceTo(target)))
      .OrderBy(candidate => candidate.Distance)
      .FirstOrDefault();
    if (match.Part == null || match.Distance > maxDistance)
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND",
        $"no {type.Replace("ProfileLabel", string.Empty)} within {maxDistance} ft of ({target.X:F2}, {target.Y:F2}).");
    }

    entry["matchedPart"] = CivilObjectUtils.GetName(match.Part);
    entry["matchDistance"] = Math.Round(match.Distance, 4);
    var direction = ReadVector(spec, "direction");

    // The part id the label API wants is the part as drawn in the view (ProfileViewPart) on some
    // releases and the model part on others; try the drawn copies first, then the model part.
    var candidates = FindProfileViewParts(database, transaction, match.Part.ObjectId).Append(match.Part.ObjectId).ToList();
    System.Exception? last = null;
    foreach (var partId in candidates)
    {
      try
      {
        return type switch
        {
          nameof(StructureProfileLabel) => StructureProfileLabel.Create(view.ObjectId, partId,
            RequireStyle(transaction, style, "structure label", labelStyles.StructureLabelStyles.LabelStyles)),
          nameof(PipeProfileLabel) => PipeProfileLabel.Create(partId, view.ObjectId, ratio,
            RequireStyle(transaction, style, "pipe label", labelStyles.PipeLabelStyles.PlanProfileLabelStyles)),
          nameof(PressurePipeProfileLabel) => PressurePipeProfileLabel.Create(partId, view.ObjectId, ratio,
            RequireStyle(transaction, style, "pressure pipe label", labelStyles.GetPressurePipeLabelStyles().PlanProfileLabelStyles, labelStyles.GetPressurePipeLabelStyles().LabelStyles)),
          nameof(PressureFittingProfileLabel) => PressureFittingProfileLabel.Create(partId, view.ObjectId, ratio, direction,
            RequireStyle(transaction, style, "pressure fitting label", labelStyles.GetPressureFittingLabelStyles().LabelStyles)),
          _ => PressureAppurtenanceProfileLabel.Create(partId, view.ObjectId, ratio, direction,
            RequireStyle(transaction, style, "pressure appurtenance label", labelStyles.GetPressureAppurtenanceLabelStyles().LabelStyles)),
        };
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (System.Exception ex)
      {
        last = ex;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.TRANSACTION_FAILED",
      $"Civil 3D refused the {type} on '{CivilObjectUtils.GetName(match.Part)}' (is the part drawn in this profile view?): {last?.Message}");
  }

  // Dragged labels keep the engineer's position (same grid, so the same model XY); text overrides are
  // replayed by component order because component ids differ between drawings.
  private static ObjectId FindExistingStationLabel(JsonObject spec, List<StationElevationLabel> existing)
  {
    if (PluginRuntime.GetOptionalString(spec, "type") != nameof(StationElevationLabel))
    {
      return ObjectId.Null;
    }

    var station = PluginRuntime.GetRequiredDouble(spec, "station");
    var elevation = PluginRuntime.GetRequiredDouble(spec, "elevation");
    var match = existing.FirstOrDefault(label => Math.Abs(label.Station - station) < 1e-4 && Math.Abs(label.Elevation - elevation) < 1e-4);
    return match?.ObjectId ?? ObjectId.Null;
  }

  // Existing plan labels keyed by what identifies them: NoteLabel by anchor, StationOffsetLabel by alignment + labeled point.
  private static List<(ObjectId Id, string Type, string Style, string? Alignment, Point2d Anchor)> CollectPlanLabels(Database database, Transaction transaction)
  {
    var labels = new List<(ObjectId, string, string, string?, Point2d)>();
    foreach (ObjectId id in NoteLabel.GetAvailableLabelIds(database))
    {
      if (!id.IsErased && transaction.GetObject(id, OpenMode.ForRead) is NoteLabel note)
      {
        var anchor = note.Dragged ? note.LabelLocation - note.DraggedOffset : note.LabelLocation;
        labels.Add((id, nameof(NoteLabel), note.StyleName, null, new Point2d(anchor.X, anchor.Y)));
      }
    }

    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
    var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
    var stationOffsetClass = RXObject.GetClass(typeof(StationOffsetLabel));
    foreach (ObjectId id in modelSpace)
    {
      if (id.ObjectClass.IsDerivedFrom(stationOffsetClass) && transaction.GetObject(id, OpenMode.ForRead) is StationOffsetLabel stationOffset)
      {
        var alignmentName = transaction.GetObject(stationOffset.FeatureId, OpenMode.ForRead) is Alignment alignment ? alignment.Name : null;
        labels.Add((id, nameof(StationOffsetLabel), stationOffset.StyleName, alignmentName, new Point2d(stationOffset.Location.X, stationOffset.Location.Y)));
      }
    }

    return labels;
  }

  private static ObjectId FindExistingPartLabel(JsonObject spec, ProfileView view, Transaction transaction, double tolerance)
  {
    var type = PluginRuntime.GetOptionalString(spec, "type");
    var style = PluginRuntime.GetOptionalString(spec, "style");
    if (type is not (nameof(StructureProfileLabel) or nameof(PipeProfileLabel) or nameof(PressurePipeProfileLabel)
      or nameof(PressureFittingProfileLabel) or nameof(PressureAppurtenanceProfileLabel))
      || PluginRuntime.GetParameter(spec, "featureX") == null || PluginRuntime.GetParameter(spec, "featureY") == null)
    {
      return ObjectId.Null;
    }

    var target = new Point2d(PluginRuntime.GetRequiredDouble(spec, "featureX"), PluginRuntime.GetRequiredDouble(spec, "featureY"));
    var ratio = PluginRuntime.GetOptionalDouble(spec, "ratio");
    foreach (ObjectId id in CollectViewLabelIds(view, transaction))
    {
      if (id.IsErased || transaction.GetObject(id, OpenMode.ForRead) is not Label label
        || label.GetType().Name != type
        || !string.Equals(label.StyleName, style, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      var feature = ResolveModelPart(transaction, label.FeatureId);
      var position = feature == null ? null : PartPosition(feature);
      if (!position.HasValue || position.Value.GetDistanceTo(target) > tolerance)
      {
        continue;
      }

      var labelRatio = label switch
      {
        PipeProfileLabel pipe => pipe.Ratio,
        PressurePipeProfileLabel pressurePipe => pressurePipe.Ratio,
        _ => (double?)null,
      };
      if (ratio.HasValue && labelRatio.HasValue && Math.Abs(ratio.Value - labelRatio.Value) > 0.02)
      {
        continue;
      }

      return id;
    }

    return ObjectId.Null;
  }

  private static ObjectId FindExistingPlanLabel(JsonObject spec, List<(ObjectId Id, string Type, string Style, string? Alignment, Point2d Anchor)> existing)
  {
    var type = PluginRuntime.GetOptionalString(spec, "type");
    var style = PluginRuntime.GetOptionalString(spec, "style");
    Point2d? anchor;
    string? alignmentName = null;
    if (type == nameof(NoteLabel))
    {
      anchor = ReadPoint(spec, "anchor") ?? ReadPoint(spec, "labelLocation");
    }
    else if (type == nameof(StationOffsetLabel))
    {
      anchor = ReadPoint(spec, "location");
      alignmentName = PluginRuntime.GetOptionalString(spec, "alignmentName");
    }
    else
    {
      return ObjectId.Null;
    }

    if (!anchor.HasValue)
    {
      return ObjectId.Null;
    }

    var match = existing.FirstOrDefault(label => label.Type == type
      && string.Equals(label.Style, style, StringComparison.OrdinalIgnoreCase)
      && (alignmentName == null || string.Equals(label.Alignment, alignmentName, StringComparison.OrdinalIgnoreCase))
      && label.Anchor.GetDistanceTo(anchor.Value) < 0.01);
    return match.Id;
  }

  // NoteLabel / StationOffsetLabel: model-space labels with no profile view behind them.
  private static ObjectId CreatePlanLabel(string type, JsonObject spec, string style, Transaction transaction, LabelStylesRoot labelStyles, StylesRoot styles)
  {
    if (type == nameof(NoteLabel))
    {
      var anchor = ReadPoint(spec, "anchor") ?? ReadPoint(spec, "labelLocation")
        ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "a NoteLabel needs anchor {x,y}.");
      var markerName = PluginRuntime.GetOptionalString(spec, "markerStyle");
      var markerId = string.IsNullOrWhiteSpace(markerName) ? ObjectId.Null : FindStyle(transaction, markerName!, styles.MarkerStyles);
      return NoteLabel.Create(new Point3d(anchor.X, anchor.Y, 0),
        RequireStyle(transaction, style, "note label", labelStyles.GeneralNoteLabelStyles), markerId);
    }

    if (type == nameof(StationOffsetLabel))
    {
      var alignmentName = PluginRuntime.GetRequiredString(spec, "alignmentName");
      var alignment = CivilObjectUtils.FindAlignmentByName(CivilApplication.ActiveDocument, transaction, alignmentName);
      var location = ReadPoint(spec, "location")
        ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "a StationOffsetLabel needs location {x,y} (the labeled point).");
      var markerName = PluginRuntime.GetOptionalString(spec, "markerStyle");
      var markerId = string.IsNullOrWhiteSpace(markerName) ? ObjectId.Null : FindStyle(transaction, markerName!, styles.MarkerStyles);
      return StationOffsetLabel.Create(alignment.ObjectId,
        RequireStyle(transaction, style, "station offset label", labelStyles.AlignmentLabelStyles.StationOffsetLabelStyles), markerId, location);
    }

    throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"plan label type '{type}' is not supported; use NoteLabel or StationOffsetLabel.");
  }

  private static object? ApplyPlanLabelsOnly(JsonArray? labelSpecs, Database database, Transaction transaction, StylesRoot styles, LabelStylesRoot labelStyles)
  {
    var planLabels = CollectPlanLabels(database, transaction);
    var labelResults = new List<Dictionary<string, object?>>();
    for (var index = 0; index < (labelSpecs?.Count ?? 0); index++)
    {
      var entry = new Dictionary<string, object?> { ["index"] = index };
      try
      {
        var spec = labelSpecs![index] as JsonObject ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "label spec must be an object.");
        var type = PluginRuntime.GetRequiredString(spec, "type");
        var style = PluginRuntime.GetRequiredString(spec, "style");
        if (type != nameof(NoteLabel) && type != nameof(StationOffsetLabel))
        {
          throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"without profileViewName only NoteLabel and StationOffsetLabel can be applied (got '{type}').");
        }

        // Re-applying the same spec moves / re-texts the existing label instead of stacking a duplicate on its anchor.
        var labelId = FindExistingPlanLabel(spec, planLabels);
        if (labelId.IsNull)
        {
          labelId = CreatePlanLabel(type, spec, style, transaction, labelStyles, styles);
        }
        else
        {
          entry["reused"] = true;
        }

        ApplyLabelPlacement(spec, labelId, transaction, database);
        entry["handle"] = labelId.Handle.ToString();
      }
      catch (System.Exception ex)
      {
        entry["error"] = ex.Message;
      }

      labelResults.Add(entry);
    }

    return new Dictionary<string, object?>
    {
      ["labels"] = labelResults,
      ["createdLabels"] = labelResults.Count(entry => entry.ContainsKey("handle")),
      ["failedLabels"] = labelResults.Count(entry => entry.ContainsKey("error")),
    };
  }

  private static void ApplyLabelPlacement(JsonObject spec, ObjectId labelId, Transaction transaction, Database database)
  {
    var location = ReadPoint(spec, "labelLocation");
    // A labelLocation that differs from the labeled point is a dragged label even when the spec omits
    // "dragged" (a hand-built payload used to drop the position silently, stacking plan labels on their anchor).
    var anchorPoint = ReadPoint(spec, "anchor") ?? ReadPoint(spec, "location");
    var dragged = PluginRuntime.GetOptionalBool(spec, "dragged")
      ?? (location.HasValue && anchorPoint.HasValue && location.Value.GetDistanceTo(anchorPoint.Value) > 1e-6);
    var overrides = PluginRuntime.GetParameter(spec, "overrides") as JsonArray;
    var layer = PluginRuntime.GetOptionalString(spec, "layer");
    if ((!dragged || location == null) && (overrides == null || overrides.Count == 0) && string.IsNullOrWhiteSpace(layer))
    {
      return;
    }

    var label = CivilObjectUtils.GetRequiredObject<Label>(transaction, labelId, OpenMode.ForWrite);
    if (!string.IsNullOrWhiteSpace(layer))
    {
      // Without an explicit layer a new label lands on the current layer (often a no-plot one).
      var layers = CivilObjectUtils.GetRequiredObject<LayerTable>(transaction, database.LayerTableId, OpenMode.ForRead);
      if (!layers.Has(layer))
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Layer '{layer}' does not exist in this drawing.");
      }

      label.Layer = layer;
    }

    if (dragged && location != null)
    {
      label.LabelLocation = new Point3d(location.Value.X, location.Value.Y, 0);
    }

    if (overrides != null)
    {
      var componentIds = label.GetTextComponentIds();
      foreach (var node in overrides.OfType<JsonObject>())
      {
        var index = PluginRuntime.GetOptionalInt(node, "index") ?? -1;
        var text = PluginRuntime.GetOptionalString(node, "text");
        if (index >= 0 && index < componentIds.Count && text != null)
        {
          label.SetTextComponentOverride(componentIds[index], text);
        }
      }
    }
  }

  // ─── Helpers ───────────────────────────────────────────────────────────────

  private static IEnumerable<ProfileView> EnumerateProfileViews(Database database, Transaction transaction)
  {
    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
    var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
    foreach (ObjectId objectId in modelSpace)
    {
      if (transaction.GetObject(objectId, OpenMode.ForRead) is ProfileView view)
      {
        yield return view;
      }
    }
  }

  private static IEnumerable<ObjectId> FindProfileViewParts(Database database, Transaction transaction, ObjectId modelPartId)
  {
    var blockTable = CivilObjectUtils.GetRequiredObject<BlockTable>(transaction, database.BlockTableId, OpenMode.ForRead);
    var modelSpace = CivilObjectUtils.GetRequiredObject<BlockTableRecord>(transaction, blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
    foreach (ObjectId objectId in modelSpace)
    {
      if (objectId.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(ProfileViewPart)))
        && transaction.GetObject(objectId, OpenMode.ForRead) is ProfileViewPart drawn
        && drawn.ModelPartId == modelPartId)
      {
        yield return objectId;
      }
    }
  }

  private static List<(AcEntity Part, Point2d Position)> CollectParts(CivilDocument civilDoc, Transaction transaction)
  {
    var parts = new List<(AcEntity, Point2d)>();
    void Add(ObjectId id)
    {
      if (transaction.GetObject(id, OpenMode.ForRead) is AcEntity part && PartPosition(part) is Point2d position)
      {
        parts.Add((part, position));
      }
    }

    foreach (ObjectId networkId in civilDoc.GetPipeNetworkIds())
    {
      if (transaction.GetObject(networkId, OpenMode.ForRead) is Network network)
      {
        foreach (ObjectId id in network.GetPipeIds()) Add(id);
        foreach (ObjectId id in network.GetStructureIds()) Add(id);
      }
    }

    foreach (ObjectId networkId in civilDoc.GetPressurePipeNetworkIds())
    {
      if (transaction.GetObject(networkId, OpenMode.ForRead) is PressurePipeNetwork network)
      {
        foreach (ObjectId id in network.GetPipeIds()) Add(id);
        foreach (ObjectId id in network.GetFittingIds()) Add(id);
        foreach (ObjectId id in network.GetAppurtenanceIds()) Add(id);
      }
    }

    return parts;
  }

  private static AcDbObject? ResolveModelPart(Transaction transaction, ObjectId featureId)
  {
    if (featureId.IsNull || featureId.IsErased)
    {
      return null;
    }

    var feature = transaction.GetObject(featureId, OpenMode.ForRead);
    if (feature is ProfileViewPart drawn && !drawn.ModelPartId.IsNull)
    {
      return transaction.GetObject(drawn.ModelPartId, OpenMode.ForRead);
    }

    return feature;
  }

  private static Point2d? PartPosition(AcDbObject part) => part switch
  {
    Structure structure => new Point2d(structure.Location.X, structure.Location.Y),
    Pipe pipe => Mid(pipe.StartPoint, pipe.EndPoint),
    PressurePipe pressurePipe => Mid(pressurePipe.StartPoint, pressurePipe.EndPoint),
    PressureFitting fitting => new Point2d(fitting.Location.X, fitting.Location.Y),
    PressureAppurtenance appurtenance => new Point2d(appurtenance.Location.X, appurtenance.Location.Y),
    _ => null,
  };

  private static Point2d Mid(Point3d a, Point3d b) => new((a.X + b.X) / 2d, (a.Y + b.Y) / 2d);

  private static ObjectId FindProfileId(Alignment alignment, Transaction transaction, string profileName)
  {
    foreach (ObjectId profileId in alignment.GetProfileIds())
    {
      if (string.Equals(CivilObjectUtils.GetRequiredObject<Profile>(transaction, profileId, OpenMode.ForRead).Name, profileName, StringComparison.OrdinalIgnoreCase))
      {
        return profileId;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile '{profileName}' was not found on alignment '{alignment.Name}'.");
  }

  private static ObjectId RequireStyle(Transaction transaction, string name, string what, params TreeNodeCollectionBase[] collections)
  {
    foreach (var collection in collections)
    {
      var id = FindStyle(transaction, name, collection);
      if (!id.IsNull)
      {
        return id;
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"The {what} style '{name}' does not exist in this drawing (civil3d_profile_view_styles lists the available ones).");
  }

  private static ObjectId FindStyle(Transaction transaction, string name, TreeNodeCollectionBase collection)
  {
    foreach (var id in StyleIds(collection))
    {
      if (string.Equals(NameOf(transaction, id), name, StringComparison.OrdinalIgnoreCase))
      {
        return id;
      }
    }

    return ObjectId.Null;
  }

  private static IEnumerable<ObjectId> StyleIds(TreeNodeCollectionBase collection)
  {
    var ids = collection is LabelStyleCollection labelStyles ? labelStyles.GetDescendantIds() : collection.ToObjectIds();
    foreach (ObjectId id in ids)
    {
      yield return id;
    }
  }

  private static List<string> Names(Transaction transaction, TreeNodeCollectionBase collection)
  {
    var names = new List<string>();
    foreach (var id in StyleIds(collection))
    {
      if (NameOf(transaction, id) is string name)
      {
        names.Add(name);
      }
    }

    return names;
  }

  private static string? NameOf(Transaction transaction, ObjectId id)
  {
    if (id.IsNull || id.IsErased)
    {
      return null;
    }

    // Typed first: the reflection-based GetName returns null for styles on Civil 3D 2027.
    return transaction.GetObject(id, OpenMode.ForRead) switch
    {
      StyleBase style => style.Name,
      Profile profile => profile.Name,
      var other => CivilObjectUtils.GetName(other),
    };
  }

  private static Dictionary<string, object?> XY(Point3d point) => new() { ["x"] = point.X, ["y"] = point.Y };

  private static Dictionary<string, object?> XY(Vector3d vector) => new() { ["x"] = vector.X, ["y"] = vector.Y };

  private static Point2d? ReadPoint(JsonObject spec, string name)
  {
    if (PluginRuntime.GetParameter(spec, name) is not JsonObject point)
    {
      return null;
    }

    return new Point2d(PluginRuntime.GetRequiredDouble(point, "x"), PluginRuntime.GetRequiredDouble(point, "y"));
  }

  private static Vector3d ReadVector(JsonObject spec, string name)
  {
    var point = ReadPoint(spec, name);
    return point.HasValue ? new Vector3d(point.Value.X, point.Value.Y, 0) : Vector3d.XAxis;
  }
}
