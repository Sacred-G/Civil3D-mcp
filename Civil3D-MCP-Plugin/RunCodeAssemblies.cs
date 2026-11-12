using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DMcpPlugin;

/// <summary>
/// Assembly loading for runCode: the compiler (Roslyn), the references a
/// script compiles against, and each compiled script in its own collectible
/// load context. The only runCode file that uses System.Reflection (exempt in
/// tests/reflection_boundary.test.ts); it never reflects over Civil 3D APIs.
/// </summary>
internal static class RunCodeAssemblies
{
  private static readonly ConcurrentDictionary<string, bool> ManagedByPath = new(StringComparer.OrdinalIgnoreCase);
  private static int _resolverRegistered;

  /// <summary>
  /// The plugin compiles against Microsoft.CodeAnalysis 4.8 and does not ship
  /// it next to its DLL. When the default load context cannot find it, Civil
  /// 3D's own copy (AutoCAD folder; 2026 has 4.10) is used if it is at least
  /// the requested version, so Civil 3D and the plugin share one copy;
  /// otherwise the bundle's roslyn\ fallback. Loading a second, newer copy
  /// first could break Civil 3D features that need theirs. Must run before
  /// the first call into ScriptCompiler.
  /// </summary>
  internal static void EnsureRoslynResolver()
  {
    if (Interlocked.Exchange(ref _resolverRegistered, 1) == 0)
    {
      AssemblyLoadContext.Default.Resolving += ResolveRoslyn;
    }
  }

  private static Assembly? ResolveRoslyn(AssemblyLoadContext context, AssemblyName requested)
  {
    if (requested.Name is not ("Microsoft.CodeAnalysis" or "Microsoft.CodeAnalysis.CSharp"))
    {
      return null;
    }

    var folders = new[]
    {
      Path.GetDirectoryName(typeof(Database).Assembly.Location),
      Path.Combine(Path.GetDirectoryName(typeof(RunCodeAssemblies).Assembly.Location) ?? string.Empty, "roslyn"),
    };
    foreach (var folder in folders)
    {
      if (string.IsNullOrEmpty(folder))
      {
        continue;
      }

      var path = Path.Combine(folder, requested.Name + ".dll");
      try
      {
        if (File.Exists(path) && (requested.Version == null || AssemblyName.GetAssemblyName(path).Version >= requested.Version))
        {
          PluginLog.Info("RunCode", $"Loading {requested.Name} {requested.Version} from {path}.");
          return context.LoadFromAssemblyPath(path);
        }
      }
      catch (Exception exception)
      {
        PluginLog.Warn("RunCode", $"Could not load {path}.", exception);
      }
    }

    return null;
  }

  internal static bool IsRoslynLoadFailure(Exception exception)
  {
    for (var current = exception; current != null; current = current.InnerException)
    {
      if (current is FileNotFoundException or FileLoadException or TypeLoadException or BadImageFormatException
          && current.Message.Contains("Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
    }

    return false;
  }

  // The .NET shared framework (Microsoft.NETCore.App and WindowsDesktop.App)
  // from the trusted platform assemblies; not AutoCAD's own folder, which also
  // holds assemblies Roslyn cannot read.
  private static readonly Lazy<string[]> FrameworkReferences = new(() =>
  {
    var frameworkDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    var sharedRoot = Path.GetDirectoryName(Path.GetDirectoryName(frameworkDirectory)!)!;
    var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
    var candidates = string.IsNullOrEmpty(trusted)
      ? Directory.EnumerateFiles(frameworkDirectory, "*.dll")
      : trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
    return candidates
      .Where(path => path.StartsWith(sharedRoot, StringComparison.OrdinalIgnoreCase))
      .ToArray();
  });

  /// <summary>What a script compiles against, one path per assembly name.</summary>
  internal static List<string> ReferencePaths()
  {
    var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    void Add(string? path)
    {
      if (!string.IsNullOrEmpty(path) && File.Exists(path) && IsManagedAssembly(path))
      {
        byName.TryAdd(Path.GetFileNameWithoutExtension(path), path);
      }
    }

    // What is loaded now, first: the exact copies Civil 3D runs (AutoCAD,
    // Civil 3D, this plugin, the framework parts in use). Earlier scripts are
    // collectible and loaded from memory, without a location.
    foreach (var assembly in LoadedSharedAssemblies())
    {
      try
      {
        Add(assembly.Location);
      }
      catch (NotSupportedException)
      {
      }
    }

    foreach (var path in FrameworkReferences.Value)
    {
      Add(path);
    }

    // The Civil 3D API assemblies a script may use before anything loaded them.
    var acadDirectory = Path.GetDirectoryName(typeof(Database).Assembly.Location)!;
    foreach (var name in new[] { "accoremgd", "acdbmgd", "acmgd", "AecBaseMgd", "AeccDbMgd", "AeccPressurePipesMgd" })
    {
      Add(Path.Combine(acadDirectory, name + ".dll"));
      Add(Path.Combine(acadDirectory, "C3D", name + ".dll"));
    }

    return byName.Values.ToList();
  }

  // The framework folders also hold native DLLs, and one unreadable reference
  // fails the whole compilation.
  private static bool IsManagedAssembly(string path)
  {
    return ManagedByPath.GetOrAdd(path, static candidate =>
    {
      try
      {
        AssemblyName.GetAssemblyName(candidate);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    });
  }

  /// <summary>
  /// Loads a compiled script into a new collectible context and returns its
  /// class. Unload the context after the run.
  /// </summary>
  internal static (Type ScriptType, AssemblyLoadContext Context) LoadScript(string runId, byte[] assembly, byte[] symbols, string typeName)
  {
    var context = new ScriptLoadContext(runId);
    try
    {
      using var assemblyStream = new MemoryStream(assembly);
      using var symbolStream = new MemoryStream(symbols);
      var type = context.LoadFromStream(assemblyStream, symbolStream).GetType(typeName, throwOnError: true)!;
      return (type, context);
    }
    catch
    {
      context.Unload();
      throw;
    }
  }

  private static IEnumerable<Assembly> LoadedSharedAssemblies()
  {
    return AppDomain.CurrentDomain.GetAssemblies()
      .Where(assembly => !assembly.IsDynamic && AssemblyLoadContext.GetLoadContext(assembly)?.IsCollectible != true);
  }

  // References resolve to the copies Civil 3D already loaded; one the script
  // compiled against but nothing has loaded yet (a Civil 3D API assembly in
  // the C3D folder, say) is loaded into the default context from the path it
  // compiled against, so it is shared with Civil 3D and the plugin.
  private sealed class ScriptLoadContext : AssemblyLoadContext
  {
    public ScriptLoadContext(string runId) : base("C3dMcpRun_" + runId, isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
      var plugin = typeof(RunCodeScript).Assembly;
      if (string.Equals(assemblyName.Name, plugin.GetName().Name, StringComparison.OrdinalIgnoreCase))
      {
        return plugin;
      }

      var loaded = LoadedSharedAssemblies()
        .FirstOrDefault(candidate => string.Equals(candidate.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
      if (loaded != null)
      {
        return loaded;
      }

      var path = ReferencePaths()
        .FirstOrDefault(candidate => string.Equals(Path.GetFileNameWithoutExtension(candidate), assemblyName.Name, StringComparison.OrdinalIgnoreCase));
      try
      {
        return path == null ? null : Default.LoadFromAssemblyPath(path);
      }
      catch (Exception exception)
      {
        PluginLog.Warn("RunCode", $"Could not load {path} for a script.", exception);
        return null;
      }
    }
  }
}
