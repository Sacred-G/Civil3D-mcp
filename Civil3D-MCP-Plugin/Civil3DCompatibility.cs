using System.Collections.Concurrent;
using System.Reflection;

namespace Civil3DMcpPlugin;

/// <summary>
/// Narrow compatibility boundary for Civil 3D members that are unavailable in
/// the referenced managed API or vary between supported host versions.
/// Documented Autodesk members should be called directly instead.
/// </summary>
internal static class Civil3DCompatibility
{
  internal readonly record struct ParameterShape(Type Type);
  private sealed record CachedProperty(PropertyInfo? Value);
  private sealed record CachedMethods(MethodInfo[] Values);
  private sealed record CachedField(FieldInfo? Value);
  private sealed record CachedType(Type? Value);

  private readonly record struct PropertyKey(Type Type, string Name, bool IsStatic);
  private readonly record struct MethodKey(Type Type, string Name, bool IsStatic, int ArgumentCount);
  private readonly record struct MethodFamilyKey(Type Type, string Name, bool IsStatic);
  private readonly record struct LoadedStaticMethodKey(string Name, Type FirstParameterType, int ArgumentCount);

  private static readonly ConcurrentDictionary<PropertyKey, CachedProperty> PropertyCache = new();
  private static readonly ConcurrentDictionary<PropertyKey, CachedField> FieldCache = new();
  private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ScalarPropertyCache = new();
  private static readonly ConcurrentDictionary<MethodKey, CachedMethods> MethodCache = new();
  private static readonly ConcurrentDictionary<MethodFamilyKey, CachedMethods> MethodFamilyCache = new();
  private static readonly ConcurrentDictionary<LoadedStaticMethodKey, CachedMethods> LoadedStaticMethodCache = new();
  private static readonly ConcurrentDictionary<string, CachedType> TypeCache = new(StringComparer.Ordinal);

  public static T? GetPropertyValue<T>(object? target, string propertyName)
  {
    var raw = GetPropertyValue(target, propertyName);
    if (raw == null)
    {
      return default;
    }

    if (raw is T typed)
    {
      return typed;
    }

    try
    {
      var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
      if (targetType.IsInstanceOfType(raw))
      {
        return (T)(object)raw;
      }

      return (T)(object)Convert.ChangeType(raw, targetType)!;
    }
    catch
    {
      return default;
    }
  }

  public static object? GetPropertyValue(object? target, string propertyName)
  {
    if (target == null)
    {
      return null;
    }

    var property = ResolveProperty(target.GetType(), propertyName, isStatic: false);
    if (property == null)
    {
      return null;
    }

    try
    {
      return property.GetValue(target);
    }
    catch
    {
      return null;
    }
  }

  public static object? GetIndexedPropertyValue(object? target, string propertyName, params object?[] indexes)
  {
    if (target == null)
    {
      return null;
    }

    var property = ResolveProperty(target.GetType(), propertyName, isStatic: false);
    try
    {
      return property?.GetValue(target, indexes);
    }
    catch
    {
      return null;
    }
  }

  public static object? GetFieldValue(object? target, string fieldName)
  {
    if (target == null)
    {
      return null;
    }

    var type = target.GetType();
    var key = new PropertyKey(type, fieldName, IsStatic: false);
    var field = FieldCache.GetOrAdd(key, static item =>
      new CachedField(item.Type.GetField(item.Name, BindingFlags.Public | BindingFlags.Instance))).Value;
    try
    {
      return field?.GetValue(target);
    }
    catch
    {
      return null;
    }
  }

  public static bool TrySetProperty(object? target, string propertyName, object? value)
  {
    if (target == null)
    {
      return false;
    }

    var property = ResolveProperty(target.GetType(), propertyName, isStatic: false);
    if (property?.CanWrite != true)
    {
      return false;
    }

    try
    {
      var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
      var converted = value == null || targetType.IsInstanceOfType(value)
        ? value
        : targetType.IsEnum && value is string enumText
          ? Enum.Parse(targetType, enumText, ignoreCase: true)
          : Convert.ChangeType(value, targetType);
      property.SetValue(target, converted);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static IReadOnlyDictionary<string, object?> GetReadableScalarProperties(object target)
  {
    var properties = ScalarPropertyCache.GetOrAdd(target.GetType(), static type =>
      type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
        .Where(property =>
        {
          var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
          return propertyType == typeof(string)
            || propertyType == typeof(bool)
            || propertyType == typeof(int)
            || propertyType == typeof(double);
        })
        .ToArray());

    var values = new Dictionary<string, object?>(StringComparer.Ordinal);
    foreach (var property in properties)
    {
      try
      {
        values[property.Name] = property.GetValue(target);
      }
      catch
      {
        // Some Autodesk wrappers throw for state-dependent getters. Omit them.
      }
    }

    return values;
  }

  public static object? InvokeMethod(object? target, string methodName, params object?[] arguments)
  {
    if (target == null)
    {
      return null;
    }

    TryInvokeCandidates(target.GetType(), target, methodName, isStatic: false, arguments, out var result);
    return result;
  }

  public static bool TryInvokeMethod(object? target, string methodName, out object? result, params object?[] arguments)
  {
    result = null;
    return target != null
      && TryInvokeCandidates(target.GetType(), target, methodName, isStatic: false, arguments, out result);
  }

  public static object? InvokeStaticMethod(Type type, string methodName, params object?[] arguments)
  {
    TryInvokeCandidates(type, null, methodName, isStatic: true, arguments, out var result);
    return result;
  }

  public static bool TryInvokeStaticMethod(Type type, string methodName, out object? result, params object?[] arguments)
  {
    return TryInvokeCandidates(type, null, methodName, isStatic: true, arguments, out result);
  }

  public static bool TryInvokeStaticOverloads(
    Type type,
    string methodName,
    Func<ParameterShape[], object?[]?> argumentBuilder,
    out object? result,
    out object?[]? invokedArguments)
  {
    result = null;
    invokedArguments = null;
    var key = new MethodFamilyKey(type, methodName, IsStatic: true);
    var methods = MethodFamilyCache.GetOrAdd(key, static item =>
      new CachedMethods(item.Type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(method => method.Name == item.Name)
        .OrderBy(method => method.GetParameters().Length)
        .ToArray())).Values;

    foreach (var method in methods)
    {
      var shapes = method.GetParameters()
        .Select(parameter => new ParameterShape(
          parameter.ParameterType.IsByRef
            ? parameter.ParameterType.GetElementType()!
            : parameter.ParameterType))
        .ToArray();
      var arguments = argumentBuilder(shapes);
      if (arguments == null)
      {
        continue;
      }

      try
      {
        result = method.Invoke(null, arguments);
        invokedArguments = arguments;
        return true;
      }
      catch (ArgumentException)
      {
      }
      catch (TargetParameterCountException)
      {
      }
      catch (TargetInvocationException)
      {
        // The runtime-selected overload rejected its synthesized defaults.
        // Continue so another compatible Civil 3D overload can be attempted.
      }
    }

    return false;
  }

  public static bool TryInvokeLoadedStaticMethod(
    string methodName,
    Type firstParameterType,
    out object? result,
    params object?[] arguments)
  {
    result = null;
    var key = new LoadedStaticMethodKey(methodName, firstParameterType, arguments.Length);
    var methods = LoadedStaticMethodCache.GetOrAdd(key, static item =>
    {
      var matches = new List<MethodInfo>();
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] types;
        try
        {
          types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
          types = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
        }
        catch
        {
          continue;
        }

        foreach (var type in types)
        {
          matches.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == item.Name)
            .Where(method => method.GetParameters().Length == item.ArgumentCount)
            .Where(method => method.GetParameters().Length > 0
              && method.GetParameters()[0].ParameterType.IsAssignableFrom(item.FirstParameterType)));
        }
      }
      return new CachedMethods(matches.ToArray());
    }).Values;

    foreach (var method in methods)
    {
      try
      {
        result = method.Invoke(null, arguments);
        return true;
      }
      catch (ArgumentException)
      {
      }
      catch (TargetParameterCountException)
      {
      }
    }
    return false;
  }

  /// <summary>
  /// Resolves a type from a Civil 3D managed assembly that ships next to a
  /// referenced anchor assembly but is not itself a build reference (for
  /// example AeccDataShortcutMgd.dll beside AeccDbMgd.dll). Already-loaded
  /// assemblies win; otherwise the sibling file is loaded from the anchor's
  /// install folder. Returns null when the file or type is absent.
  /// </summary>
  public static Type? FindSiblingAssemblyType(Type anchorType, string assemblyFileName, string fullTypeName)
  {
    var loaded = FindLoadedType(fullTypeName);
    if (loaded != null)
    {
      return loaded;
    }

    try
    {
      var directory = Path.GetDirectoryName(anchorType.Assembly.Location);
      if (string.IsNullOrWhiteSpace(directory))
      {
        return null;
      }

      var candidate = Path.Combine(directory, assemblyFileName);
      if (!File.Exists(candidate))
      {
        return null;
      }

      var assembly = Assembly.LoadFrom(candidate);
      var type = assembly.GetType(fullTypeName, throwOnError: false, ignoreCase: false);
      if (type != null)
      {
        TypeCache[fullTypeName] = new CachedType(type);
      }

      return type;
    }
    catch
    {
      return null;
    }
  }

  public static Type? FindLoadedType(params string[] fullNames)
  {
    foreach (var fullName in fullNames)
    {
      var cached = TypeCache.GetOrAdd(fullName, static candidateName =>
      {
        var assemblyQualifiedType = Type.GetType(candidateName, throwOnError: false, ignoreCase: false);
        if (assemblyQualifiedType != null)
        {
          return new CachedType(assemblyQualifiedType);
        }

        var commaIndex = candidateName.IndexOf(',');
        var normalizedName = commaIndex >= 0
          ? candidateName[..commaIndex].Trim()
          : candidateName;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
          var type = assembly.GetType(normalizedName, throwOnError: false, ignoreCase: false);
          if (type != null)
          {
            return new CachedType(type);
          }
        }

        return new CachedType(null);
      });
      if (cached.Value != null)
      {
        return cached.Value;
      }
    }

    return null;
  }

  private static PropertyInfo? ResolveProperty(Type type, string propertyName, bool isStatic)
  {
    var key = new PropertyKey(type, propertyName, isStatic);
    return PropertyCache.GetOrAdd(key, static item =>
    {
      var flags = BindingFlags.Public | (item.IsStatic ? BindingFlags.Static : BindingFlags.Instance);
      return new CachedProperty(item.Type.GetProperty(item.Name, flags));
    }).Value;
  }

  private static bool TryInvokeCandidates(
    Type type,
    object? target,
    string methodName,
    bool isStatic,
    object?[] arguments,
    out object? result)
  {
    result = null;
    var key = new MethodKey(type, methodName, isStatic, arguments.Length);
    var methods = MethodCache.GetOrAdd(key, static item =>
    {
      var flags = BindingFlags.Public | (item.IsStatic ? BindingFlags.Static : BindingFlags.Instance);
      return new CachedMethods(item.Type
        .GetMethods(flags)
        .Where(method => method.Name == item.Name && method.GetParameters().Length == item.ArgumentCount)
        .ToArray());
    }).Values;

    foreach (var method in methods)
    {
      try
      {
        result = method.Invoke(target, arguments);
        return true;
      }
      catch (ArgumentException)
      {
        // Try the next overload. Invocation exceptions from a compatible
        // overload are allowed to propagate to the command's explicit error path.
      }
      catch (TargetParameterCountException)
      {
      }
    }

    return false;
  }
}
