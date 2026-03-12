using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DMcpPlugin;

public static class SectionCommands
{
  public static Task<object?> ListSampleLineGroupsAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var sampleLineGroups = new List<Dictionary<string, object?>>();
      var groupIds = CivilObjectUtils.InvokeMethod(alignment, "GetSampleLineGroupIds") as ObjectIdCollection;

      if (groupIds != null)
      {
        foreach (ObjectId groupId in groupIds)
        {
          var group = CivilObjectUtils.GetRequiredObject<SampleLineGroup>(transaction, groupId, OpenMode.ForRead);
          var stations = new List<double>();
          foreach (ObjectId sampleLineId in group.GetSampleLineIds())
          {
            var sampleLine = CivilObjectUtils.GetRequiredObject<SampleLine>(transaction, sampleLineId, OpenMode.ForRead);
            stations.Add(sampleLine.Station);
          }

          sampleLineGroups.Add(new Dictionary<string, object?>
          {
            ["name"] = group.Name,
            ["handle"] = CivilObjectUtils.GetHandle(group),
            ["sampleLineCount"] = stations.Count,
            ["stations"] = stations,
          });
        }
      }

      return new Dictionary<string, object?>
      {
        ["sampleLineGroups"] = sampleLineGroups,
      };
    });
  }

  public static Task<object?> CreateSampleLinesAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var groupName = PluginRuntime.GetRequiredString(parameters, "groupName");
    var leftWidth = PluginRuntime.GetRequiredDouble(parameters, "leftWidth");
    var rightWidth = PluginRuntime.GetRequiredDouble(parameters, "rightWidth");
    var interval = PluginRuntime.GetOptionalDouble(parameters, "interval");
    var stationsNode = PluginRuntime.GetParameter(parameters, "stations") as JsonArray;

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var groupId = SampleLineGroup.Create(groupName, alignment.ObjectId);
      var group = CivilObjectUtils.GetRequiredObject<SampleLineGroup>(transaction, groupId, OpenMode.ForWrite);
      var sectionSources = group.GetSectionSources();
      var requestedSurfaces = (PluginRuntime.GetParameter(parameters, "surfaces") as JsonArray)?.Select(node => node?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (SectionSource source in sectionSources)
      {
        var sourceObject = transaction.GetObject(source.SourceId, OpenMode.ForRead);
        var sourceName = CivilObjectUtils.GetName(sourceObject);
        source.IsSampled = requestedSurfaces.Count == 0 || (sourceName != null && requestedSurfaces.Contains(sourceName));
      }

      var stations = new List<double>();
      if (stationsNode != null && stationsNode.Count > 0)
      {
        stations.AddRange(stationsNode.Select(node => node?.GetValue<double>() ?? 0));
      }
      else if (interval.HasValue && interval.Value > 0)
      {
        for (var station = alignment.StartingStation; station <= alignment.EndingStation; station += interval.Value)
        {
          stations.Add(station);
        }
      }
      else
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "createSampleLines requires either stations or interval.");
      }

      var createdStations = new List<double>();
      foreach (var station in stations.Distinct().OrderBy(value => value))
      {
        double x1 = 0;
        double y1 = 0;
        double x2 = 0;
        double y2 = 0;
        alignment.PointLocation(station, -leftWidth, ref x1, ref y1);
        alignment.PointLocation(station, rightWidth, ref x2, ref y2);
        var points = new Point2dCollection
        {
          new(x1, y1),
          new(x2, y2),
        };
        SampleLine.Create($"SL-{station:0.##}", groupId, points);
        createdStations.Add(station);
      }

      return new Dictionary<string, object?>
      {
        ["alignmentName"] = alignment.Name,
        ["sampleLineGroupName"] = group.Name,
        ["created"] = createdStations.Count,
        ["stations"] = createdStations,
      };
    });
  }

  public static Task<object?> GetSectionDataAsync(JsonObject? parameters)
  {
    var alignmentName = PluginRuntime.GetRequiredString(parameters, "alignmentName");
    var sampleLineGroupName = PluginRuntime.GetRequiredString(parameters, "sampleLineGroupName");
    var station = PluginRuntime.GetRequiredDouble(parameters, "station");

    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var alignment = CivilObjectUtils.FindAlignmentByName(civilDoc, transaction, alignmentName);
      var groupIds = CivilObjectUtils.InvokeMethod(alignment, "GetSampleLineGroupIds") as ObjectIdCollection;
      if (groupIds == null)
      {
        throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No sample line groups exist for alignment '{alignmentName}'.");
      }

      foreach (ObjectId groupId in groupIds)
      {
        var group = CivilObjectUtils.GetRequiredObject<SampleLineGroup>(transaction, groupId, OpenMode.ForRead);
        if (!string.Equals(group.Name, sampleLineGroupName, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        var sampleLine = group.GetSampleLineIds()
          .Cast<ObjectId>()
          .Select(id => CivilObjectUtils.GetRequiredObject<SampleLine>(transaction, id, OpenMode.ForRead))
          .FirstOrDefault(line => Math.Abs(line.Station - station) < 0.0001);

        if (sampleLine == null)
        {
          throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Sample line at station {station} was not found.");
        }

        return new Dictionary<string, object?>
        {
          ["station"] = sampleLine.Station,
          ["surfaces"] = new List<object>(),
          ["units"] = new Dictionary<string, object?>
          {
            ["horizontal"] = CivilObjectUtils.LinearUnits(database),
            ["vertical"] = CivilObjectUtils.LinearUnits(database),
          },
        };
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Sample line group '{sampleLineGroupName}' was not found.");
    });
  }
}
