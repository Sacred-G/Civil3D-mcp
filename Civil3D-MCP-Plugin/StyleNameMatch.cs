namespace Civil3DMcpPlugin;

/// <summary>
/// Host-independent name matching behind the LookupUtils style lookups, kept
/// free of Civil 3D types so tests/StyleLookupHarness can exercise it.
/// </summary>
public static class StyleNameMatch
{
  private const int MaxListedNames = 30;

  /// <summary>
  /// Index of the style to use: with no requested name, the first style (0),
  /// or -1 when there are none; with a name, an exact (ordinal) match first,
  /// then a case-insensitive match, ignoring surrounding whitespace; -1 when
  /// nothing matches. An unknown name never falls back to another style.
  /// </summary>
  public static int FindIndex(IReadOnlyList<string?> names, string? requested)
  {
    if (string.IsNullOrWhiteSpace(requested))
    {
      return names.Count > 0 ? 0 : -1;
    }

    var wanted = requested.Trim();
    for (var i = 0; i < names.Count; i++)
    {
      if (string.Equals(names[i]?.Trim(), wanted, StringComparison.Ordinal))
      {
        return i;
      }
    }

    for (var i = 0; i < names.Count; i++)
    {
      if (string.Equals(names[i]?.Trim(), wanted, StringComparison.OrdinalIgnoreCase))
      {
        return i;
      }
    }

    return -1;
  }

  /// <summary>The INVALID_INPUT message for a requested name that does not exist.</summary>
  public static string DescribeMissing(string kind, string requested, IReadOnlyList<string?> names)
  {
    var available = names
      .Where(name => !string.IsNullOrWhiteSpace(name))
      .Select(name => name!)
      .ToList();

    if (available.Count == 0)
    {
      return $"{kind} '{requested.Trim()}' was not found: the drawing has no {Plural(kind)}.";
    }

    var listed = string.Join(", ", available.Take(MaxListedNames).Select(name => $"'{name}'"));
    var more = available.Count > MaxListedNames ? $" (and {available.Count - MaxListedNames} more)" : string.Empty;
    return $"{kind} '{requested.Trim()}' was not found. Available: {listed}{more}.";
  }

  private static string Plural(string kind)
  {
    var lower = kind.ToLowerInvariant();
    return lower.EndsWith("s", StringComparison.Ordinal) ? lower : lower + "s";
  }
}
