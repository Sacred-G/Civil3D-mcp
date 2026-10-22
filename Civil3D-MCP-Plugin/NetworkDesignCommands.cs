using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

// Typed helpers for designing pipe networks on plan/profile sheets: the parts catalogs (gravity parts
// lists and pressure part lists, with the exact names/descriptions the add-part tools match on) and
// drawing a whole network into a profile view.
public static class NetworkDesignCommands
{
  public static Task<object?> ListNetworkCatalogAsync(JsonObject? parameters)
  {
    return CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var gravity = new List<Dictionary<string, object?>>();
      foreach (ObjectId listId in civilDoc.Styles.PartsListSet)
      {
        if (transaction.GetObject(listId, OpenMode.ForRead) is not PartsList partsList)
        {
          continue;
        }

        gravity.Add(new Dictionary<string, object?>
        {
          ["name"] = partsList.Name,
          ["pipes"] = FamilySizes(transaction, partsList, DomainType.Pipe),
          ["structures"] = FamilySizes(transaction, partsList, DomainType.Structure),
        });
      }

      var pressure = new List<Dictionary<string, object?>>();
      foreach (ObjectId listId in civilDoc.Styles.GetPressurePartLists())
      {
        if (transaction.GetObject(listId, OpenMode.ForRead) is not PressurePartList partList)
        {
          continue;
        }

        var byType = new Dictionary<string, object?>();
        foreach (var partType in new[] { PressurePartType.PressurePipe, PressurePartType.Elbow, PressurePartType.Tee, PressurePartType.Plug, PressurePartType.Cap, PressurePartType.Valve, PressurePartType.Hydrant })
        {
          var descriptions = new List<string>();
          try
          {
            foreach (var part in partList.GetParts(partType))
            {
              if (part.IsValid && !string.IsNullOrWhiteSpace(part.Description))
              {
                descriptions.Add(part.Description);
              }
            }
          }
          catch
          {
            // a part list without this type throws on some releases; report it as empty
          }

          byType[partType.ToString()] = descriptions.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(d => d).ToList();
        }

        pressure.Add(new Dictionary<string, object?> { ["name"] = partList.Name, ["parts"] = byType });
      }

      return new Dictionary<string, object?> { ["gravityPartsLists"] = gravity, ["pressurePartLists"] = pressure };
    });
  }

  private static List<Dictionary<string, object?>> FamilySizes(Transaction transaction, PartsList partsList, DomainType domain)
  {
    var families = new List<Dictionary<string, object?>>();
    foreach (ObjectId familyId in partsList.GetPartFamilyIdsByDomain(domain))
    {
      if (transaction.GetObject(familyId, OpenMode.ForRead) is not PartFamily family)
      {
        continue;
      }

      var sizes = new List<string>();
      for (var i = 0; i < family.PartSizeCount; i++)
      {
        var size = transaction.GetObject(family[i], OpenMode.ForRead);
        var sizeName = CivilObjectUtils.GetName(size);
        if (!string.IsNullOrWhiteSpace(sizeName))
        {
          sizes.Add(sizeName!);
        }
      }

      families.Add(new Dictionary<string, object?> { ["family"] = family.Name, ["sizes"] = sizes });
    }

    return families;
  }

  // Draws every part of a gravity or pressure network into a profile view (Part.AddToProfileView /
  // PressurePart.AddToProfileView). Parts already shown in that view are skipped.
  public static Task<object?> AddNetworkToProfileViewAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var profileViewName = PluginRuntime.GetRequiredString(parameters, "profileViewName");
    // Optional: draw only these parts (e.g. one crossing stub in another street's profile).
    var partNames = (PluginRuntime.GetParameter(parameters, "partNames") as JsonArray)?
      .Select(node => node?.GetValue<string>())
      .Where(name => !string.IsNullOrWhiteSpace(name))
      .Select(name => name!)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var profileViewId = FindProfileViewId(civilDoc, transaction, profileViewName);
      var added = 0;
      var failed = new List<string>();
      var matchedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      void AddPart(ObjectId partId)
      {
        var part = transaction.GetObject(partId, OpenMode.ForRead);
        if (partNames is { Count: > 0 })
        {
          var name = part switch { Part gravityPart => gravityPart.Name, PressurePart pressurePart => pressurePart.Name, _ => null };
          if (name == null || !partNames.Contains(name))
          {
            return;
          }

          matchedNames.Add(name);
        }

        part.UpgradeOpen();
        try
        {
          switch (part)
          {
            case Part gravityPart:
              gravityPart.AddToProfileView(profileViewId);
              added++;
              break;
            case PressurePart pressurePart:
              pressurePart.AddToProfileView(profileViewId);
              added++;
              break;
          }
        }
        catch (Exception ex)
        {
          failed.Add($"{partId.Handle}: {ex.Message}");
        }
      }

      string networkType;
      var gravity = FindGravityNetwork(civilDoc, transaction, networkName);
      if (gravity != null)
      {
        networkType = "gravity";
        foreach (ObjectId id in gravity.GetPipeIds()) AddPart(id);
        foreach (ObjectId id in gravity.GetStructureIds()) AddPart(id);
      }
      else
      {
        var pressure = FindPressureNetwork(civilDoc, transaction, networkName)
          ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No gravity or pressure network named '{networkName}'.");
        networkType = "pressure";
        foreach (ObjectId id in pressure.GetPipeIds()) AddPart(id);
        foreach (ObjectId id in pressure.GetFittingIds()) AddPart(id);
        foreach (ObjectId id in pressure.GetAppurtenanceIds()) AddPart(id);
      }

      if (partNames is { Count: > 0 })
      {
        foreach (var missing in partNames.Where(name => !matchedNames.Contains(name)))
        {
          failed.Add($"{missing}: no part with that name in network '{networkName}'");
        }
      }

      return new Dictionary<string, object?>
      {
        ["networkName"] = networkName,
        ["networkType"] = networkType,
        ["profileViewName"] = profileViewName,
        ["partsAdded"] = added,
        ["failed"] = failed,
      };
    });
  }

  // Sets a network part's Description (what pipe labels print as <[Description]>, e.g. the as-built
  // "EXIST 8\" DIP WATER MAIN" on a crossing drawn with a stand-in part) and/or renames it.
  public static Task<object?> SetPartPropertiesAsync(JsonObject? parameters)
  {
    var networkName = PluginRuntime.GetRequiredString(parameters, "networkName");
    var partName = PluginRuntime.GetRequiredString(parameters, "partName");
    var description = PluginRuntime.GetOptionalString(parameters, "description");
    var newName = PluginRuntime.GetOptionalString(parameters, "newName");
    if (description == null && string.IsNullOrWhiteSpace(newName))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Pass description and/or newName.");
    }

    return CivilExecution.WriteAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var ids = new List<ObjectId>();
      var gravity = FindGravityNetwork(civilDoc, transaction, networkName);
      if (gravity != null)
      {
        foreach (ObjectId id in gravity.GetPipeIds()) ids.Add(id);
        foreach (ObjectId id in gravity.GetStructureIds()) ids.Add(id);
      }
      else
      {
        var pressure = FindPressureNetwork(civilDoc, transaction, networkName)
          ?? throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No gravity or pressure network named '{networkName}'.");
        foreach (ObjectId id in pressure.GetPipeIds()) ids.Add(id);
        foreach (ObjectId id in pressure.GetFittingIds()) ids.Add(id);
        foreach (ObjectId id in pressure.GetAppurtenanceIds()) ids.Add(id);
      }

      foreach (var id in ids)
      {
        if (transaction.GetObject(id, OpenMode.ForRead) is Autodesk.Civil.DatabaseServices.Entity part
          && string.Equals(part.Name, partName, StringComparison.OrdinalIgnoreCase))
        {
          part.UpgradeOpen();
          if (description != null)
          {
            part.Description = description;
          }

          if (!string.IsNullOrWhiteSpace(newName))
          {
            part.Name = newName;
          }

          return new Dictionary<string, object?>
          {
            ["networkName"] = networkName,
            ["name"] = part.Name,
            ["description"] = part.Description,
            ["type"] = part.GetType().Name,
            ["handle"] = part.Handle.ToString(),
          };
        }
      }

      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"No part named '{partName}' in network '{networkName}'.");
    });
  }

  private static Network? FindGravityNetwork(CivilDocument civilDoc, Transaction transaction, string name)
  {
    foreach (ObjectId id in civilDoc.GetPipeNetworkIds())
    {
      if (transaction.GetObject(id, OpenMode.ForRead) is Network network && string.Equals(network.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return network;
      }
    }

    return null;
  }

  private static PressurePipeNetwork? FindPressureNetwork(CivilDocument civilDoc, Transaction transaction, string name)
  {
    foreach (ObjectId id in civilDoc.GetPressurePipeNetworkIds())
    {
      if (transaction.GetObject(id, OpenMode.ForRead) is PressurePipeNetwork network && string.Equals(network.Name, name, StringComparison.OrdinalIgnoreCase))
      {
        return network;
      }
    }

    return null;
  }

  internal static ObjectId FindProfileViewId(CivilDocument civilDoc, Transaction transaction, string profileViewName)
  {
    foreach (ObjectId alignmentId in civilDoc.GetAlignmentIds())
    {
      var alignment = CivilObjectUtils.GetRequiredObject<Alignment>(transaction, alignmentId, OpenMode.ForRead);
      foreach (ObjectId viewId in alignment.GetProfileViewIds())
      {
        var view = CivilObjectUtils.GetRequiredObject<ProfileView>(transaction, viewId, OpenMode.ForRead);
        if (string.Equals(view.Name, profileViewName, StringComparison.OrdinalIgnoreCase))
        {
          return viewId;
        }
      }
    }

    throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Profile view '{profileViewName}' was not found.");
  }
}
