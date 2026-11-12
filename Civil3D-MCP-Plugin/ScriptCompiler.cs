using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Civil3DMcpPlugin;

/// <summary>
/// Compiles a runCode snippet (C# statements) into an in-memory assembly whose
/// class derives from <c>RunCodeScript</c>; the snippet becomes the body of
/// <c>Run()</c>. Only Roslyn and the BCL are used here, no AutoCAD types, so
/// tests/RunCodeHarness can exercise it outside Civil 3D against the reference
/// assemblies. Nothing here touches the host: callers compile on a worker
/// thread and only run the result on the main thread.
///
/// Methods of this class reference Roslyn types, so the first call loads
/// Roslyn: RoslynResolver must be registered before that (RunCodeCommands).
/// </summary>
internal static class ScriptCompiler
{
  // File name the snippet's lines map to (#line), in diagnostics and stack traces.
  internal const string ScriptPath = "script";
  internal const string BaseClassName = "Civil3DMcpPlugin.RunCodeScript";
  internal const string GuardDiagnosticId = "C3DMCP001";

  // Namespaces every snippet can use without a using directive.
  // Autodesk.AutoCAD.Runtime is left out on purpose: its Exception hides
  // System.Exception in every catch. Type names that two of these namespaces
  // both declare would not compile (CS0104), so DefaultAliases names every one
  // of them (tests/RunCodeHarness checks that none is missing).
  internal static readonly string[] DefaultUsings =
  {
    "System",
    "System.Collections.Generic",
    "System.Diagnostics",
    "System.Globalization",
    "System.IO",
    "System.Linq",
    "System.Text",
    "System.Text.Json",
    "System.Text.Json.Nodes",
    "System.Text.RegularExpressions",
    "System.Threading",
    "System.Threading.Tasks",
    "Autodesk.AutoCAD.ApplicationServices",
    "Autodesk.AutoCAD.Colors",
    "Autodesk.AutoCAD.DatabaseServices",
    "Autodesk.AutoCAD.EditorInput",
    "Autodesk.AutoCAD.Geometry",
    "Autodesk.Civil",
    "Autodesk.Civil.ApplicationServices",
    "Autodesk.Civil.DatabaseServices",
    "Autodesk.Civil.DatabaseServices.Styles",
  };

  // A name both AutoCAD and Civil 3D declare means AutoCAD's (the Civil 3D one
  // gets a Civil prefix), except Surface and Section, which mean Civil 3D's
  // (AutoCAD's get an Ac prefix).
  internal static readonly string[] DefaultAliases =
  {
    "AcException = Autodesk.AutoCAD.Runtime.Exception",
    "ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus",
    "Application = Autodesk.AutoCAD.ApplicationServices.Application",
    "DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject",
    "CivilDBObject = Autodesk.Civil.DatabaseServices.DBObject",
    "Entity = Autodesk.AutoCAD.DatabaseServices.Entity",
    "CivilEntity = Autodesk.Civil.DatabaseServices.Entity",
    "Surface = Autodesk.Civil.DatabaseServices.Surface",
    "AcSurface = Autodesk.AutoCAD.DatabaseServices.Surface",
    "Section = Autodesk.Civil.DatabaseServices.Section",
    "AcSection = Autodesk.AutoCAD.DatabaseServices.Section",
    "SectionViewStyle = Autodesk.Civil.DatabaseServices.Styles.SectionViewStyle",
    "AcSectionViewStyle = Autodesk.AutoCAD.DatabaseServices.SectionViewStyle",
    "Table = Autodesk.AutoCAD.DatabaseServices.Table",
    "CivilTable = Autodesk.Civil.DatabaseServices.Table",
    "TableStyle = Autodesk.AutoCAD.DatabaseServices.TableStyle",
    "CivilTableStyle = Autodesk.Civil.DatabaseServices.Styles.TableStyle",
    "Shape = Autodesk.AutoCAD.DatabaseServices.Shape",
    "CivilShape = Autodesk.Civil.DatabaseServices.Shape",
    "PointCloud = Autodesk.AutoCAD.DatabaseServices.PointCloud",
    "CivilPointCloud = Autodesk.Civil.DatabaseServices.PointCloud",
    "Graph = Autodesk.AutoCAD.DatabaseServices.Graph",
    "CivilGraph = Autodesk.Civil.DatabaseServices.Graph",
    "Group = Autodesk.AutoCAD.DatabaseServices.Group",
    "RegexGroup = System.Text.RegularExpressions.Group",
    "NurbSurface = Autodesk.AutoCAD.DatabaseServices.NurbSurface",
    "GeNurbSurface = Autodesk.AutoCAD.Geometry.NurbSurface",
    "TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager",
    "SystemVariableChangedEventArgs = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventArgs",
    "SystemVariableChangedEventHandler = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventHandler",
    "SystemVariableChangingEventArgs = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangingEventArgs",
    "SystemVariableChangingEventHandler = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangingEventHandler",
    "ThreadState = System.Threading.ThreadState",
    "TimeZone = System.TimeZone",
    "Trace = System.Diagnostics.Trace",
  };

  // A leading "using X;" / "using static X;" / "using A = X;" line of the
  // snippet. "using var x = ...;" and "using (...)" statements do not match.
  private static readonly Regex UsingDirective = new(
    @"^using\s+(?:static\s+)?(?:[A-Za-z_]\w*\s*=\s*)?[A-Za-z_][\w.]*(?:<[^;]*>)?\s*;$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex StackLine = new(@"\bin " + Regex.Escape(ScriptPath) + @":line (\d+)", RegexOptions.Compiled);

  private static readonly ConcurrentDictionary<string, MetadataReference?> ReferenceCache = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// The full source: default usings, the snippet's own leading using
  /// directives (moved up, each mapped back to its line), then the class with
  /// the snippet as Run()'s body, mapped to "script" from line 1.
  /// </summary>
  internal static string BuildSource(string code, string className)
  {
    var lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    var hoisted = new StringBuilder();
    for (var index = 0; index < lines.Length; index++)
    {
      var trimmed = lines[index].Trim();
      if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal))
      {
        continue;
      }

      // A using directive holds no string literal, so a "//" in it starts a comment.
      var comment = trimmed.IndexOf("//", StringComparison.Ordinal);
      if (comment > 0)
      {
        trimmed = trimmed[..comment].TrimEnd();
      }

      if (!UsingDirective.IsMatch(trimmed))
      {
        break;
      }

      hoisted.Append("#line ").Append(index + 1).Append(" \"").Append(ScriptPath).Append("\"\n");
      hoisted.Append(trimmed).Append('\n');
      hoisted.Append("#line default\n");
      lines[index] = string.Empty;
    }

    var source = new StringBuilder();
    foreach (var name in DefaultUsings)
    {
      source.Append("using ").Append(name).Append(";\n");
    }

    foreach (var alias in DefaultAliases)
    {
      source.Append("using ").Append(alias).Append(";\n");
    }

    source.Append(hoisted);
    source.Append("public sealed class ").Append(className).Append(" : global::").Append(BaseClassName).Append('\n');
    source.Append("{\n");
    source.Append("  public override object Run()\n");
    source.Append("  {\n");
    source.Append("#line 1 \"").Append(ScriptPath).Append("\"\n");
    foreach (var line in lines)
    {
      source.Append(line).Append('\n');
    }

    source.Append("#line default\n");
    source.Append("#pragma warning disable CS0162\n");
    source.Append("    return null;\n");
    source.Append("#pragma warning restore CS0162\n");
    source.Append("  }\n");
    source.Append("}\n");
    return source.ToString();
  }

  internal static ScriptCompileResult Compile(string code, string assemblyName, IEnumerable<string> referencePaths, bool writeMode)
  {
    var className = "Script_" + Regex.Replace(assemblyName, @"[^A-Za-z0-9_]", "_");
    var source = BuildSource(code, className);
    var tree = CSharpSyntaxTree.ParseText(
      source,
      new CSharpParseOptions(LanguageVersion.Latest),
      path: assemblyName + ".cs",
      encoding: Encoding.UTF8);

    var references = referencePaths
      .Select(GetReference)
      .Where(reference => reference != null)
      .Cast<MetadataReference>()
      .ToList();

    var options = new CSharpCompilationOptions(
        OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: false,
        nullableContextOptions: NullableContextOptions.Disable,
        platform: Platform.X64)
      // Version unification between the host's assemblies is expected.
      .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
      {
        ["CS1701"] = ReportDiagnostic.Suppress,
        ["CS1702"] = ReportDiagnostic.Suppress,
      });

    var compilation = CSharpCompilation.Create(assemblyName, new[] { tree }, references, options);
    var result = new ScriptCompileResult { TypeName = className, ReferenceCount = references.Count };

    result.Errors.AddRange(FindLineDirectives(tree, source));
    if (result.Errors.Count > 0)
    {
      return result;
    }

    foreach (var diagnostic in compilation.GetDiagnostics())
    {
      AddDiagnostic(result, diagnostic);
    }

    if (result.Errors.Count > 0)
    {
      return result;
    }

    result.Errors.AddRange(FindGuardViolations(compilation, tree, writeMode));
    if (result.Errors.Count > 0)
    {
      return result;
    }

    using var peStream = new MemoryStream();
    using var pdbStream = new MemoryStream();
    var emit = compilation.Emit(peStream, pdbStream, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
    if (!emit.Success)
    {
      foreach (var diagnostic in emit.Diagnostics)
      {
        AddDiagnostic(result, diagnostic);
      }

      return result;
    }

    var compiled = new ScriptCompileResult
    {
      Assembly = peStream.ToArray(),
      Pdb = pdbStream.ToArray(),
      TypeName = className,
      ReferenceCount = references.Count,
    };
    compiled.Warnings.AddRange(result.Warnings);
    return compiled;
  }

  /// <summary>Snippet line of the innermost stack frame in the script, or null.</summary>
  internal static int? FindScriptLine(string? stackTrace)
  {
    if (string.IsNullOrEmpty(stackTrace))
    {
      return null;
    }

    var match = StackLine.Match(stackTrace);
    return match.Success ? int.Parse(match.Groups[1].Value) : null;
  }

  private static void AddDiagnostic(ScriptCompileResult result, Diagnostic diagnostic)
  {
    if (diagnostic.Severity is not (DiagnosticSeverity.Error or DiagnosticSeverity.Warning))
    {
      return;
    }

    var (line, column) = MapToScript(diagnostic.Location);
    var entry = new ScriptDiagnostic(line, column, diagnostic.Id, diagnostic.GetMessage());
    if (diagnostic.Severity == DiagnosticSeverity.Error)
    {
      result.Errors.Add(entry);
    }
    else if (result.Warnings.Count < 20)
    {
      result.Warnings.Add(entry);
    }
  }

  // 1-based line and column in the snippet; 0 when the location is outside it
  // (a missing reference, or the generated wrapper).
  private static (int Line, int Column) MapToScript(Location location)
  {
    if (location == Location.None || !location.IsInSource)
    {
      return (0, 0);
    }

    var span = location.GetMappedLineSpan();
    if (!span.HasMappedPath || span.Path != ScriptPath)
    {
      return (0, 0);
    }

    return (span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1);
  }

  // Callers pass managed assemblies only (RunCodeAssemblies.ReferencePaths
  // filters out native DLLs): one unreadable reference fails the compilation.
  private static MetadataReference? GetReference(string path)
  {
    return ReferenceCache.GetOrAdd(path, static candidate =>
    {
      try
      {
        return MetadataReference.CreateFromFile(candidate);
      }
      catch (Exception)
      {
        return null;
      }
    });
  }

  // ---------------------------------------------------------------------------
  // Guard: refuse API calls that would block Civil 3D, or change the drawing in
  // read mode. It reads the compiled symbols, so aliases and using static are
  // caught, but it is a guard against mistakes, not a sandbox: reflection,
  // dynamic or COM get past it, and the code runs with the user's full rights.
  // ---------------------------------------------------------------------------

  private static readonly HashSet<string> InteractiveEditorMethods = new(StringComparer.Ordinal)
  {
    "GetPoint", "GetCorner", "GetDistance", "GetAngle", "GetString", "GetKeywords", "GetInteger",
    "GetDouble", "GetEntity", "GetNestedEntity", "GetSelection", "GetFileNameForOpen",
    "GetFileNameForSave", "Drag", "Command", "CommandAsync", "StartUserInteraction", "DoPrompt",
  };

  private static readonly string[] CivilWritePrefixes =
  {
    "Create", "Add", "Remove", "Delete", "Rebuild", "Import", "Insert", "Erase", "Rename", "Update",
    "Set", "Reverse", "Join", "Split", "Extend", "Trim", "Promote", "Synchronize", "Lock", "Unlock",
    "Move", "Copy", "Clear", "Reset", "Extract",
  };

  // A snippet's own #line directive would map the code after it away from
  // "script", where the guard does not look (that is how it skips the
  // wrapper), so refuse any inside the snippet's body. Ours sit outside it,
  // and hoisted using lines cannot hold one.
  private static IEnumerable<ScriptDiagnostic> FindLineDirectives(SyntaxTree tree, string source)
  {
    var bodyMarker = "#line 1 \"" + ScriptPath + "\"\n";
    var bodyStart = source.IndexOf(bodyMarker, source.IndexOf("public override object Run()", StringComparison.Ordinal), StringComparison.Ordinal) + bodyMarker.Length;
    var bodyEnd = source.LastIndexOf("#line default\n#pragma warning disable CS0162", StringComparison.Ordinal);
    var firstBodyLine = tree.GetText().Lines.GetLinePosition(bodyStart).Line;
    foreach (var trivia in tree.GetRoot().DescendantTrivia())
    {
      if ((trivia.IsKind(SyntaxKind.LineDirectiveTrivia) || trivia.IsKind(SyntaxKind.LineSpanDirectiveTrivia))
          && trivia.SpanStart >= bodyStart && trivia.SpanStart < bodyEnd)
      {
        var position = tree.GetLineSpan(trivia.Span).StartLinePosition;
        yield return new ScriptDiagnostic(position.Line - firstBodyLine + 1, position.Character + 1, GuardDiagnosticId,
          "#line directives are not allowed in a snippet: they move code away from its script lines, past the guard.");
      }
    }
  }

  private static IEnumerable<ScriptDiagnostic> FindGuardViolations(CSharpCompilation compilation, SyntaxTree tree, bool writeMode)
  {
    var model = compilation.GetSemanticModel(tree);
    var reported = new HashSet<(int, string)>();
    foreach (var node in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
    {
      var (line, column) = MapToScript(node.GetLocation());
      if (line == 0)
      {
        continue;
      }

      var info = model.GetSymbolInfo(node);
      var symbol = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
      if (symbol == null)
      {
        continue;
      }

      var reason = DenyReason(symbol, node, model, writeMode);
      if (reason != null && reported.Add((line, symbol.Name)))
      {
        yield return new ScriptDiagnostic(line, column, GuardDiagnosticId, reason);
      }
    }

    if (writeMode)
    {
      yield break;
    }

    // An OpenMode written as a number, such as (OpenMode)1, names no ForWrite
    // symbol for DenyReason to see. Keyed by member name like the loop above,
    // so OpenMode.ForWrite is reported once.
    foreach (var expression in tree.GetRoot().DescendantNodes().OfType<ExpressionSyntax>())
    {
      var mode = OpenModeConstant(model, expression);
      if (mode == null || mode == "ForRead")
      {
        continue;
      }

      var (line, column) = MapToScript(expression.GetLocation());
      if (line != 0 && reported.Add((line, mode)))
      {
        var display = mode.StartsWith("(", StringComparison.Ordinal) ? mode : "OpenMode." + mode;
        yield return new ScriptDiagnostic(line, column, GuardDiagnosticId,
          $"Read mode opens objects for read only: {display} is not allowed. Use mode 'write' to change the drawing.");
      }
    }
  }

  // The OpenMode member a constant expression stands for ("ForRead",
  // "ForWrite"...), "(OpenMode)n" for a value without a member, or null when
  // the expression is not a constant OpenMode.
  private static string? OpenModeConstant(SemanticModel model, ExpressionSyntax expression)
  {
    var typeInfo = model.GetTypeInfo(expression);
    var type = new[] { typeInfo.Type, typeInfo.ConvertedType }
      .OfType<INamedTypeSymbol>()
      .FirstOrDefault(candidate => candidate.ToDisplayString() == "Autodesk.AutoCAD.DatabaseServices.OpenMode");
    var constant = type == null ? default : model.GetConstantValue(expression);
    if (type == null || !constant.HasValue)
    {
      return null;
    }

    var value = Convert.ToInt64(constant.Value);
    var member = type.GetMembers().OfType<IFieldSymbol>()
      .FirstOrDefault(field => field.HasConstantValue && Convert.ToInt64(field.ConstantValue) == value);
    return member?.Name ?? $"(OpenMode){value}";
  }

  private static string? DenyReason(ISymbol symbol, SimpleNameSyntax node, SemanticModel model, bool writeMode)
  {
    var type = symbol.ContainingType;
    var name = symbol.Name;
    var typeName = type?.ToDisplayString() ?? string.Empty;
    var display = string.IsNullOrEmpty(typeName) ? name : $"{type!.Name}.{name}";

    // Always refused: these wait for a person, open a modal window, or end the process.
    if (symbol is IMethodSymbol)
    {
      if (InheritsFrom(type, "Autodesk.AutoCAD.EditorInput.Editor") && InteractiveEditorMethods.Contains(name))
      {
        return $"{display} waits for user input or needs a running command; runCode runs without user interaction.";
      }

      if (InheritsFrom(type, "Autodesk.AutoCAD.ApplicationServices.Document") && name == "SendStringToExecute")
      {
        return $"{display} queues command-line input that would run after the script, outside its transaction.";
      }

      // ShowDialog and ShowModal of any toolkit (WinForms, WPF, and the
      // Autodesk.AutoCAD.Windows dialogs, which have both), so no list of
      // dialog classes is needed; AdWindows' TaskDialog opens with Show().
      if (name is "ShowDialog" or "ShowModal"
          || (typeName == "Autodesk.Windows.TaskDialog" && name == "Show")
          || (typeName is "Autodesk.AutoCAD.ApplicationServices.Application" or "Autodesk.AutoCAD.ApplicationServices.Core.Application"
              && name is "ShowModalDialog" or "ShowModalWindow" or "ShowAlertDialog" or "Quit")
          || (typeName is "System.Windows.Forms.MessageBox" or "System.Windows.MessageBox" && name == "Show")
          || (typeName == "Microsoft.VisualBasic.Interaction" && name is "InputBox" or "MsgBox"))
      {
        return $"{display} opens a modal window or closes Civil 3D, which would block or end the host.";
      }

      // A nested message loop on the main thread: the run's time budget is
      // never checked until it ends.
      if ((typeName is "System.Windows.Forms.Application" or "System.Windows.Application" && name == "Run")
          || (typeName == "System.Windows.Threading.Dispatcher" && name is "PushFrame" or "Run"))
      {
        return $"{display} runs a message loop that blocks Civil 3D until it ends.";
      }

      if (typeName == "System.Environment" && name is "Exit" or "FailFast")
      {
        return $"{display} ends the Civil 3D process.";
      }
    }

    if (symbol is IEventSymbol && IsHostApi(type))
    {
      return $"Subscribing to {display}: an event handler outlives the run and would fire later, outside any time budget.";
    }

    // TopTransaction hands out the run's transaction without the RunTransaction
    // wrapper, so a Commit or Dispose through it would end the run's transaction.
    if (symbol is IPropertySymbol && name == "TopTransaction"
        && InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.TransactionManager"))
    {
      return $"{display} is the run's own transaction: use Tr (or TransactionFor(db) for another drawing).";
    }

    // The run owns Tr and the side-database transactions. A snippet's own
    // Tr.Commit() would commit the change and then make the run's commit fail,
    // so the run would be reported as rolled back (at run time the wrapper
    // throws too; this says so at compile time, with the line).
    if (symbol is IMethodSymbol && name is "Commit" or "Abort" or "Dispose"
        && InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.Transaction") && IsRunTransaction(model, node))
    {
      return $"{display}: the run owns this transaction (write mode commits it when the snippet returns, read mode never does). Just return.";
    }

    // Write mode rolls back what goes through Tr (and transactions nested in
    // it). An open-close transaction or ObjectId.Open commits on its own, so
    // a failure later in the script would leave those changes behind.
    if (writeMode && symbol is IMethodSymbol
        && ((name == "StartOpenCloseTransaction" && type?.Name == "TransactionManager")
            || (typeName == "Autodesk.AutoCAD.DatabaseServices.ObjectId" && name == "Open")))
    {
      return $"{display}: write mode rolls back only changes made through Tr, and these commit on their own. Use Tr or Open<T>(id, forWrite: true).";
    }

    // A transaction the snippet starts (write: StartTransaction, read:
    // StartOpenCloseTransaction; each mode refuses the other kind) must end on
    // every path. One left open is aborted by the run through a wrapper of its
    // own, and the snippet's wrapper then ends it again from the finalizer
    // thread, a later Civil 3D crash.
    if (symbol is IMethodSymbol && type?.Name == "TransactionManager"
        && name == (writeMode ? "StartTransaction" : "StartOpenCloseTransaction")
        && !(InvocationOf(node) is { } call && IsUsingResource(call)))
    {
      return $"{display}: start it only as 'using var t = db.TransactionManager.{name}();' or 'using (var t = db.TransactionManager.{name}()) {{ ... }}', so it ends on every path; a transaction left open crashes Civil 3D later.";
    }

    // OpenSide hands out a database the run disposes when it ends (after the
    // transaction that holds its objects); the snippet's wrapper does not own
    // it, so a using or Dispose() on it would only detach that wrapper.
    if (DisposesSideDatabase(symbol, node, model))
    {
      return "OpenSide(...): the run disposes side databases when it ends; remove the using or the Dispose() call.";
    }

    if (writeMode)
    {
      return null;
    }

    const string useWrite = " Use mode 'write' to change the drawing.";

    if (symbol is IFieldSymbol && typeName == "Autodesk.AutoCAD.DatabaseServices.OpenMode" && name is "ForWrite" or "ForNotify")
    {
      return $"Read mode opens objects for read only: {display} is not allowed.{useWrite}";
    }

    if (symbol is IMethodSymbol)
    {
      if (name == "StartTransaction" && type != null && type.Name == "TransactionManager")
      {
        return $"{display}: a regular transaction outside a command clears the drawing's undo history. Use Tr, OpenSide/TransactionFor, or 'using var t = db.TransactionManager.StartOpenCloseTransaction();' in read mode.";
      }

      if ((InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.DBObject") && name is "UpgradeOpen" or "UpgradeFromNotify" or "Erase" or "SwapIdWith" or "HandOverTo")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.BlockTableRecord") && name == "AppendEntity")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.SymbolTable") && name == "Add")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.DBDictionary") && name is "SetAt" or "Remove")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.Transaction") && name == "AddNewlyCreatedDBObject")
          || (typeName == "Autodesk.AutoCAD.DatabaseServices.Database" && name is "SaveAs" or "Save" or "Insert" or "WblockCloneObjects"
              or "DeepCloneObjects" or "Audit" or "LoadLineTypeFile" or "UpdateExt" or "AttachXref" or "OverlayXref" or "DetachXref"
              or "BindXrefs" or "ReloadXrefs" or "UnloadXrefs" or "ResolveXrefs" or "AddDBObject" or "EvaluateFields")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.BlockReference") && name == "ExplodeToOwnerSpace")
          || (InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.LayoutManager")
              && name is "CreateLayout" or "DeleteLayout" or "RenameLayout" or "CopyLayout" or "CloneLayout" or "SetCurrentLayoutId")
          || (typeName is "Autodesk.AutoCAD.ApplicationServices.Application" or "Autodesk.AutoCAD.ApplicationServices.Core.Application" && name == "SetSystemVariable"))
      {
        return $"{display} changes a drawing.{useWrite}";
      }

      var ns = type?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
      if (ns.StartsWith("Autodesk.Civil", StringComparison.Ordinal)
          && CivilWritePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
      {
        return $"{display} looks like a Civil 3D change (create/add/update...).{useWrite}";
      }
    }

    // Setters on database-resident objects need ForWrite, which is refused
    // above (and AutoCAD throws eNotOpenForWrite), and setters on objects made
    // in memory are harmless. Database, document, editor and Civil 3D settings
    // properties change the drawing without opening anything.
    if (symbol is IPropertySymbol && IsHostApi(type) && IsAssignmentTarget(node)
        && !InheritsFrom(type, "Autodesk.AutoCAD.DatabaseServices.DBObject")
        && !(typeName == "Autodesk.AutoCAD.DatabaseServices.HostApplicationServices" && name == "WorkingDatabase"))
    {
      return $"Setting {display} changes a drawing or a setting.{useWrite}";
    }

    return null;
  }

  private static bool IsHostApi(INamedTypeSymbol? type)
  {
    var ns = type?.ContainingNamespace?.ToDisplayString() ?? string.Empty;
    return ns.StartsWith("Autodesk.AutoCAD", StringComparison.Ordinal)
      || ns.StartsWith("Autodesk.Civil", StringComparison.Ordinal)
      || ns.StartsWith("Autodesk.Aec", StringComparison.Ordinal);
  }

  // "Tr.Commit()" or "TransactionFor(db).Commit()": the receiver is the
  // script's Tr property or the result of its TransactionFor method.
  private static bool IsRunTransaction(SemanticModel model, SimpleNameSyntax node)
  {
    if (node.Parent is not MemberAccessExpressionSyntax access || access.Name != node)
    {
      return false;
    }

    var receiver = model.GetSymbolInfo(access.Expression).Symbol;
    if (receiver is IPropertySymbol { Name: "Tr" } property && InheritsFrom(property.ContainingType, BaseClassName))
    {
      return true;
    }

    return access.Expression is InvocationExpressionSyntax invocation
      && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "TransactionFor" } method
      && InheritsFrom(method.ContainingType, BaseClassName);
  }

  // The call "M(...)" or "x.M(...)" whose method name is node, or null (a
  // method group, for example).
  private static InvocationExpressionSyntax? InvocationOf(SimpleNameSyntax node)
  {
    SyntaxNode callee = node.Parent is MemberAccessExpressionSyntax access && access.Name == node ? access : node;
    return callee.Parent is InvocationExpressionSyntax invocation && invocation.Expression == callee ? invocation : null;
  }

  // The resource of "using (e)", "using (var v = e)" or "using var v = e;".
  private static bool IsUsingResource(ExpressionSyntax expression)
  {
    if (expression.Parent is UsingStatementSyntax statement && statement.Expression == expression)
    {
      return true;
    }

    return expression.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } }
      && (declaration.Parent is UsingStatementSyntax
          || (declaration.Parent is LocalDeclarationStatementSyntax local && local.UsingKeyword.IsKind(SyntaxKind.UsingKeyword)));
  }

  // A using over OpenSide(...), or over a local initialised directly from it,
  // or Dispose() called on either.
  private static bool DisposesSideDatabase(ISymbol symbol, SimpleNameSyntax node, SemanticModel model)
  {
    if (IsOpenSide(symbol))
    {
      return InvocationOf(node) is { } call && IsUsingResource(call);
    }

    if (symbol is ILocalSymbol && IsUsingResource(node))
    {
      return IsSideDatabase(model, node);
    }

    return symbol is IMethodSymbol { Name: "Dispose" }
      && node.Parent is MemberAccessExpressionSyntax access && access.Name == node
      && IsSideDatabase(model, access.Expression);
  }

  // OpenSide(...) itself, or a local whose declaration initialises it from OpenSide(...).
  private static bool IsSideDatabase(SemanticModel model, ExpressionSyntax expression)
  {
    if (expression is InvocationExpressionSyntax invocation)
    {
      return IsOpenSide(model.GetSymbolInfo(invocation).Symbol);
    }

    return model.GetSymbolInfo(expression).Symbol is ILocalSymbol local
      && local.DeclaringSyntaxReferences.Any(reference =>
        reference.GetSyntax() is VariableDeclaratorSyntax { Initializer.Value: InvocationExpressionSyntax value }
        && IsOpenSide(model.GetSymbolInfo(value).Symbol));
  }

  // RunCodeScript.OpenSide (not a local function of the snippet with that name).
  private static bool IsOpenSide(ISymbol? symbol) =>
    symbol is IMethodSymbol { Name: "OpenSide" } method && method.ContainingType?.ToDisplayString() == BaseClassName;

  // Every way of writing to it: X = v, X += v, X++, --X, (X) = v, X!++,
  // (X, y) = (...) at any tuple depth, and C# 14's Db?.X = v.
  private static bool IsAssignmentTarget(SimpleNameSyntax node)
  {
    SyntaxNode target = node.Parent switch
    {
      MemberAccessExpressionSyntax access when access.Name == node => access,
      MemberBindingExpressionSyntax binding => binding,
      _ => node,
    };

    while (true)
    {
      switch (target.Parent)
      {
        case ParenthesizedExpressionSyntax parenthesized:
          target = parenthesized;
          continue;
        // Db.X!++ compiles and writes Db.X.
        case PostfixUnaryExpressionSyntax suppressed when suppressed.IsKind(SyntaxKind.SuppressNullableWarningExpression):
          target = suppressed;
          continue;
        // A tuple element is written only when the tuple is a deconstruction
        // target; on the right side or as an argument it is a read.
        case ArgumentSyntax { Parent: TupleExpressionSyntax tuple }:
          target = tuple;
          continue;
        case AssignmentExpressionSyntax assignment:
          return assignment.Left == target;
        // C# itself refuses a property here today (CS8186); kept for completeness.
        case ForEachVariableStatementSyntax loop:
          return loop.Variable == target;
        case PrefixUnaryExpressionSyntax prefix:
          return prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression);
        case PostfixUnaryExpressionSyntax postfix:
          return postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression);
        default:
          return false;
      }
    }
  }

  private static bool InheritsFrom(INamedTypeSymbol? type, string fullName)
  {
    for (var current = type; current != null; current = current.BaseType)
    {
      if (current.ToDisplayString() == fullName)
      {
        return true;
      }
    }

    return false;
  }
}

// Kept outside ScriptCompiler so callers can hold results without loading Roslyn.
internal sealed record ScriptDiagnostic(int Line, int Column, string Code, string Message);

internal sealed class ScriptCompileResult
{
  public byte[]? Assembly { get; init; }
  public byte[]? Pdb { get; init; }
  public string TypeName { get; init; } = "";
  public List<ScriptDiagnostic> Errors { get; } = new();
  public List<ScriptDiagnostic> Warnings { get; } = new();
  public int ReferenceCount { get; init; }
  public bool Succeeded => Assembly != null && Errors.Count == 0;
}
