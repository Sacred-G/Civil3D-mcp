// Offline checks of runCode's compiler (Civil3D-MCP-Plugin/ScriptCompiler.cs).
// Build the plugin first, then run on the framework of that Civil 3D release:
//   2026 (net8):  dotnet run --project tests/RunCodeHarness -f net8.0 -- [<refs>] [<Civil3DMcpPlugin.dll>]
//   2027 (net10): dotnet run --project tests/RunCodeHarness -f net10.0 -- <refs> <net10 Civil3DMcpPlugin.dll>
// Defaults: %LOCALAPPDATA%\Civil3D-mcp\refs-2026 and the net8.0-windows Release
// build. Prints SKIPPED (exit 0) when either is missing; exit 1 on a failure.
using Civil3DMcpPlugin;

var repoRoot = FindRepoRoot();
var refsFolder = args.Length > 0
  ? args[0]
  : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Civil3D-mcp", "refs-2026");
var pluginDll = args.Length > 1
  ? args[1]
  : Path.Combine(repoRoot, "Civil3D-MCP-Plugin", "bin", "Release", "net8.0-windows", "Civil3DMcpPlugin.dll");

if (!File.Exists(Path.Combine(refsFolder, "AcDbMgd.dll")) || !File.Exists(pluginDll))
{
  Console.WriteLine($"SKIPPED: needs the Civil 3D references ({refsFolder}) and the built plugin ({pluginDll}).");
  return 0;
}

// The framework this harness runs on (like Civil 3D's), WPF/WinForms for
// acmgd's signatures, the Civil 3D references and the plugin (RunCodeScript).
var frameworkDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
var desktopDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(frameworkDirectory)!)!, "Microsoft.WindowsDesktop.App", Path.GetFileName(frameworkDirectory));
var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
  .Where(path => path.StartsWith(frameworkDirectory, StringComparison.OrdinalIgnoreCase))
  .Concat(Directory.Exists(desktopDirectory) ? Directory.GetFiles(desktopDirectory, "*.dll") : Array.Empty<string>())
  .Concat(Directory.GetFiles(refsFolder, "*.dll"))
  .Append(pluginDll)
  .Where(IsManagedAssembly)
  .ToList();

var failures = 0;
var run = 0;

ScriptCompileResult Compile(string code, bool write = false) =>
  ScriptCompiler.Compile(code, $"HarnessRun_{++run}", references, write);

void Expect(bool condition, string name, ScriptCompileResult? result = null)
{
  if (condition)
  {
    Console.WriteLine($"ok   {name}");
    return;
  }

  failures++;
  Console.WriteLine($"FAIL {name}");
  foreach (var error in result?.Errors ?? new List<ScriptDiagnostic>())
  {
    Console.WriteLine($"       line {error.Line}:{error.Column} {error.Code} {error.Message}");
  }
}

bool HasError(ScriptCompileResult result, string code, int line) =>
  result.Errors.Any(error => error.Code == code && error.Line == line);

// The obstacle-sampling test of 2026-10-11 (Sample.cs) as a runCode script.
var sample = """
  var dwg = (string)Args["dwg"];
  double r = 40;
  var cells = new HashSet<(int, int)>();
  bool Hit(double x, double y) => cells.Contains(((int)(x / r), (int)(y / r)));
  var side = OpenSide(dwg);
  long n = 0, ents = 0;
  var sb = new StringBuilder();
  foreach (var e in ModelSpace<Entity>(side))
  {
    ents++;
    if (e is Hatch || e is Solid || e is RasterImage || e is Wipeout) continue;
    Extents3d ext; try { ext = e.GeometricExtents; } catch (Exception) { continue; }
    if (e is Curve cv)
    {
      double len; try { len = cv.GetDistanceAtParameter(cv.EndParam); } catch (AcException) { continue; }
      for (double s = 0; s <= len; s += 0.8) { var p = cv.GetPointAtDist(s); if (Hit(p.X, p.Y)) { sb.Append(p.X.ToString("F2")).Append(' ').Append(p.Y.ToString("F2")).Append('\n'); n++; } }
    }
  }
  Log($"{ents} objects, {n} points");
  return new { objects = ents, points = n, file = WriteFile("points.txt", sb.ToString()) };
  """;
var sampleResult = Compile(sample);
Expect(sampleResult.Succeeded, "the obstacle-sampling script compiles in read mode", sampleResult);

var civil = Compile("""
  var names = new List<string>();
  foreach (ObjectId id in Civil.GetAlignmentIds()) names.Add(Open<Alignment>(id).Name);
  Surface surface = null;
  CivilEntity civilEntity = null;
  return names;
  """);
Expect(civil.Succeeded, "Civil 3D namespaces and the Entity/Surface aliases resolve", civil);

var syntax = Compile("var a = 1;\nvar b = 2;\nvar c = a + ;\nreturn c;");
Expect(!syntax.Succeeded && syntax.Errors.All(error => error.Line == 3), "a syntax error is reported on its snippet line (3)", syntax);

var semantic = Compile("// comment\nvar x = 1;\n\nx.NoSuchMember();");
Expect(HasError(semantic, "CS1061", 4), "a semantic error is reported on its snippet line (4)", semantic);

var hoisted = Compile("using System.Numerics;\nusing Sb = System.Text.StringBuilder;\nvar v = new Vector2(1, 2);\nreturn new Sb().Append(v.X).ToString();");
Expect(hoisted.Succeeded, "leading using directives are moved out of the method", hoisted);

var commentedUsing = Compile("using System.Numerics; // vectors\nreturn new Vector2(1, 2).X;");
Expect(commentedUsing.Succeeded, "a using line with a trailing comment is moved out too", commentedUsing);

var badUsing = Compile("using System.Text;\nusing Nope.Nowhere;\nreturn 1;");
Expect(HasError(badUsing, "CS0246", 2), "an unknown namespace in a using is reported on its line (2)", badUsing);

var usingStatement = Compile("using var stream = new MemoryStream();\nusing (var other = new MemoryStream()) { }\nreturn stream.Length;");
Expect(usingStatement.Succeeded, "using statements stay in the method", usingStatement);

const string forWrite = "foreach (var e in ModelSpace<Entity>()) { var w = Tr.GetObject(e.ObjectId, OpenMode.ForWrite); }\nreturn null;";
var readWrite = Compile(forWrite);
Expect(HasError(readWrite, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses OpenMode.ForWrite", readWrite);
Expect(Compile(forWrite, write: true).Succeeded, "write mode allows OpenMode.ForWrite");
Expect(readWrite.Errors.Count(error => error.Code == ScriptCompiler.GuardDiagnosticId) == 1, "OpenMode.ForWrite is reported once", readWrite);

foreach (var (label, snippet) in new[] {
  ("(OpenMode)1", "var o = Tr.GetObject(Db.BlockTableId, (OpenMode)1);\nreturn null;"),
  ("((OpenMode)2)", "var o = Tr.GetObject(Db.BlockTableId, ((OpenMode)2));\nreturn null;"),
  ("OpenMode.ForRead + 1", "var o = Tr.GetObject(Db.BlockTableId, OpenMode.ForRead + 1);\nreturn null;") })
{
  var r = Compile(snippet);
  Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, 1) && r.Errors.Count == 1, $"read mode refuses the OpenMode constant {label} (once)", r);
}

Expect(Compile("var a = Tr.GetObject(Db.BlockTableId, (OpenMode)0);\nvar b = Tr.GetObject(Db.BlockTableId, OpenMode.ForRead);\nvar c = Tr.GetObject(Db.BlockTableId, 0);\nreturn a == b && b == c;").Succeeded,
  "read mode allows ForRead given as (OpenMode)0, OpenMode.ForRead or 0");
Expect(Compile("var o = Tr.GetObject(Db.BlockTableId, (OpenMode)1);\nreturn null;", write: true).Succeeded, "write mode allows (OpenMode)1");

var upgrade = Compile("var e = Open<Entity>(ObjectId.Null);\ne.UpgradeOpen();\ne.Erase();");
Expect(HasError(upgrade, ScriptCompiler.GuardDiagnosticId, 2), "read mode refuses UpgradeOpen/Erase", upgrade);

var transaction = Compile("using var t = Db.TransactionManager.StartTransaction();\nreturn 1;");
Expect(HasError(transaction, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses a regular transaction", transaction);
Expect(Compile("using var t = Db.TransactionManager.StartOpenCloseTransaction();\nreturn 1;").Succeeded, "read mode allows an open-close transaction");

var databaseSetter = Compile("Db.Ltscale = 2;");
Expect(HasError(databaseSetter, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses setting a Database property", databaseSetter);
Expect(Compile("Db.Ltscale = 2;", write: true).Succeeded, "write mode allows setting a Database property");

foreach (var (label, snippet) in new[] {
  ("Db.Ltscale++", "Db.Ltscale++;"), ("++Db.Ltscale", "++Db.Ltscale;"), ("Db.Pdmode--", "Db.Pdmode--;"), ("--Db.Pdmode", "--Db.Pdmode;"),
  ("a compound assignment", "Db.Ltscale += 1;"),
  ("a deconstruction", "(Db.Ltscale, Db.Textsize) = (2.0, 3.0);"),
  ("a nested deconstruction", "double x, y; (x, (Db.Ltscale, y)) = (1.0, (2.0, 3.0));"),
  ("a parenthesized target", "(Db.Ltscale) = 2;"), ("a parenthesized ++", "(Db.Ltscale)++;"), ("Db.Ltscale!++", "Db.Ltscale!++;") })
{
  var r = Compile(snippet);
  Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, 1), $"read mode refuses {label} on a Database property", r);
}

var propertyReads = Compile("""
  var n = 0; n++;
  var pl = new Polyline(); pl.Elevation++;
  var t = (Db.Ltscale, 1); var s = Db.Ltscale!;
  double a, b; (a, b) = (Db.Ltscale, Db.Textsize);
  var m = Math.Max(Db.Ltscale, -Db.Textsize);
  foreach (var (u, v) in new[] { (Db.Ltscale, 1.0) }) { }
  return n + a + m;
  """);
Expect(propertyReads.Succeeded, "read mode allows ++ on locals and in-memory entities, and reading a property into a tuple, an argument or a foreach", propertyReads);
Expect(Compile("Db.Ltscale++;", write: true).Succeeded, "write mode allows Db.Ltscale++");

var memoryEntity = Compile("var pl = new Polyline();\npl.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);\npl.Closed = true;\nreturn pl.Area;");
Expect(memoryEntity.Succeeded, "read mode allows building an entity in memory", memoryEntity);

var civilCreate = Compile("Civil.CogoPoints.Add(new Point3d(0, 0, 0), true);");
Expect(HasError(civilCreate, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses a Civil 3D Add", civilCreate);

var append = Compile("var ms = Open<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(Db));\nms.AppendEntity(new Line());");
Expect(HasError(append, ScriptCompiler.GuardDiagnosticId, 2), "read mode refuses AppendEntity", append);

var prompt = Compile("var p = Ed.GetPoint(\"Pick\");\nreturn p.Value;", write: true);
Expect(HasError(prompt, ScriptCompiler.GuardDiagnosticId, 1), "both modes refuse Editor prompts", prompt);

var dialog = Compile("Application.ShowAlertDialog(\"hi\");", write: true);
Expect(HasError(dialog, ScriptCompiler.GuardDiagnosticId, 1), "both modes refuse modal dialogs", dialog);

foreach (var (label, snippet) in new[] {
  ("Form.ShowDialog", "new System.Windows.Forms.Form().ShowDialog();"),
  ("Window.ShowDialog", "new System.Windows.Window().ShowDialog();"),
  ("a WinForms OpenFileDialog", "new System.Windows.Forms.OpenFileDialog().ShowDialog();"),
  ("a WPF OpenFileDialog", "new Microsoft.Win32.OpenFileDialog().ShowDialog();"),
  ("an AutoCAD OpenFileDialog", "new Autodesk.AutoCAD.Windows.OpenFileDialog(\"t\", null, \"dwg\", \"x\", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.NoUrls).ShowDialog();"),
  ("an AutoCAD OpenFileDialog.ShowModal", "new Autodesk.AutoCAD.Windows.OpenFileDialog(\"t\", null, \"dwg\", \"x\", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.NoUrls).ShowModal();"),
  ("an AutoCAD ColorDialog.ShowModal", "new Autodesk.AutoCAD.Windows.ColorDialog().ShowModal();"),
  ("TaskDialog", "System.Windows.Forms.TaskDialog.ShowDialog(new System.Windows.Forms.TaskDialogPage());"),
  ("WinForms Application.Run", "System.Windows.Forms.Application.Run(new System.Windows.Forms.Form());"),
  ("WPF Application.Run", "new System.Windows.Application().Run();"),
  ("Dispatcher.PushFrame", "System.Windows.Threading.Dispatcher.PushFrame(new System.Windows.Threading.DispatcherFrame());"),
  ("Dispatcher.Run", "System.Windows.Threading.Dispatcher.Run();"),
  ("InputBox", "Microsoft.VisualBasic.Interaction.InputBox(\"x\");"),
  ("MsgBox", "Microsoft.VisualBasic.Interaction.MsgBox(\"x\");"),
  ("Editor.DoPrompt", "Ed.DoPrompt(new PromptPointOptions(\"p\"));") })
{
  var r = Compile(snippet, write: true);
  Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, 1), $"both modes refuse {label}", r);
}

// AdWindows.dll (Autodesk.Windows.TaskDialog, a modal task dialog) is not among the
// reference assemblies: a stand-in with the same full name checks the rule, which
// matches by type and method name.
var standIn = Path.Combine(Path.GetTempPath(), $"RunCodeHarness-AdWindows-{Guid.NewGuid():N}.dll");
using (var standInImage = new MemoryStream())
{
  var standInBuild = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("AdWindows",
      new[] { Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("namespace Autodesk.Windows { public class TaskDialog { public string MainInstruction { get; set; } public void Show() { } } }") },
      new[] { Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
      new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary))
    .Emit(standInImage);
  File.WriteAllBytes(standIn, standInImage.ToArray());
  Expect(standInBuild.Success, "the AdWindows TaskDialog stand-in builds");
}

foreach (var write in new[] { false, true })
{
  var r = ScriptCompiler.Compile("new Autodesk.Windows.TaskDialog { MainInstruction = \"Continue?\" }.Show();", $"HarnessRun_{++run}", references.Append(standIn), write);
  Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, 1), $"{(write ? "write" : "read")} mode refuses AdWindows TaskDialog.Show", r);
}

try
{
  File.Delete(standIn);
}
catch (Exception)
{
  // Best effort: the compiler may still hold the file open.
}

var subscribe = Compile("Db.ObjectModified += (s, e) => { };", write: true);
Expect(HasError(subscribe, ScriptCompiler.GuardDiagnosticId, 1), "both modes refuse event subscriptions", subscribe);

var ownCommit = Compile("var x = 1;\nTr.Commit();", write: true);
Expect(HasError(ownCommit, ScriptCompiler.GuardDiagnosticId, 2), "both modes refuse committing the run's Tr", ownCommit);

var sideAbort = Compile("TransactionFor(Db).Abort();");
Expect(HasError(sideAbort, ScriptCompiler.GuardDiagnosticId, 1), "both modes refuse aborting a run transaction", sideAbort);

foreach (var write in new[] { false, true })
{
  var top = Compile("var x = 1;\nvar t = Db.TransactionManager.TopTransaction;\nreturn x;", write);
  Expect(HasError(top, ScriptCompiler.GuardDiagnosticId, 2), $"{(write ? "write" : "read")} mode refuses TopTransaction (the unwrapped run transaction)", top);
}

// A snippet's own #line would move the code after it out of "script", past the guard.
foreach (var (label, snippet, line) in new[] {
  ("#line default", "var x = 1;\n#line default\nDb.Ltscale = 2;\nreturn x;", 2),
  ("#line to another file", "#line 1 \"other\"\nDb.Ltscale = 2;", 1),
  ("#line hidden", "var x = 1;\nvar y = 2;\n#line hidden\nreturn x;", 3) })
{
  var r = Compile(snippet);
  Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, line), $"read mode refuses a snippet's own {label}", r);
}
Expect(Compile("var s = \"#line 5\";\nreturn s;").Succeeded, "a string that mentions #line compiles");
Expect(Compile("using System.Text;\nvar sb = new StringBuilder();\nreturn sb.Length;").Succeeded, "hoisted using lines still compile next to the #line check");

Expect(Compile("using var t = Db.TransactionManager.StartTransaction();\nt.Commit();", write: true).Succeeded, "write mode allows committing a transaction the snippet started");
Expect(Compile("using (var t = Db.TransactionManager.StartTransaction())\n{\n  t.Commit();\n}", write: true).Succeeded, "write mode allows a nested transaction in a using statement");
var bareTransaction = Compile("var t = Db.TransactionManager.StartTransaction();\nt.Commit();", write: true);
Expect(HasError(bareTransaction, ScriptCompiler.GuardDiagnosticId, 1), "write mode refuses a nested transaction outside a using (left open, it crashes Civil 3D later)", bareTransaction);
var bareOpenClose = Compile("var t = Db.TransactionManager.StartOpenCloseTransaction();\nreturn 1;");
Expect(HasError(bareOpenClose, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses an open-close transaction outside a using", bareOpenClose);
Expect(Compile("using (var t = Db.TransactionManager.StartOpenCloseTransaction()) { }\nreturn 1;").Succeeded, "read mode allows an open-close transaction in a using statement");

// The run disposes OpenSide's databases (after the transaction holding their objects).
foreach (var write in new[] { false, true })
{
  var mode = write ? "write" : "read";
  foreach (var (label, snippet, line) in new[] {
    ("using var over OpenSide in a loop", "foreach (var p in new[] { \"a.dwg\" }) { using var side = OpenSide(p); }", 1),
    ("a using statement over OpenSide", "using (var side = OpenSide(\"a.dwg\")) { }", 1),
    ("using (OpenSide(...))", "using (OpenSide(\"a.dwg\")) { }", 1),
    ("Dispose() on a side database", "var side = OpenSide(\"a.dwg\");\nside.Dispose();", 2),
    ("OpenSide(...).Dispose()", "OpenSide(\"a.dwg\").Dispose();", 1),
    ("a using over a side-database local", "var side = OpenSide(\"a.dwg\");\nusing (side) { }", 2) })
  {
    var r = Compile(snippet, write);
    Expect(HasError(r, ScriptCompiler.GuardDiagnosticId, line) && r.Errors.Any(error => error.Message.Contains("the run disposes side databases when it ends")),
      $"{mode} mode refuses {label}", r);
  }

  var sideRead = Compile("var side = OpenSide(\"a.dwg\");\nvar ms = new MemoryStream();\nms.Dispose();\nreturn ModelSpace<Curve>(side).Count();", write);
  Expect(sideRead.Succeeded, $"{mode} mode allows reading a side database and disposing other objects", sideRead);
}

var writeOpenClose = Compile("using var t = Db.TransactionManager.StartOpenCloseTransaction();\nreturn 1;", write: true);
Expect(HasError(writeOpenClose, ScriptCompiler.GuardDiagnosticId, 1), "write mode refuses open-close transactions (they commit outside Tr)", writeOpenClose);

var writeOpen = Compile("var o = Db.BlockTableId.Open(OpenMode.ForRead);", write: true);
Expect(HasError(writeOpen, ScriptCompiler.GuardDiagnosticId, 1), "write mode refuses ObjectId.Open", writeOpen);

var extract = Compile("var s = Open<TinSurface>(ObjectId.Null);\nreturn s.ExtractContours(1.0).Count;");
Expect(HasError(extract, ScriptCompiler.GuardDiagnosticId, 2), "read mode refuses Civil 3D Extract* (adds entities)", extract);

var explode = Compile("Open<BlockReference>(ObjectId.Null).ExplodeToOwnerSpace();");
Expect(HasError(explode, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses ExplodeToOwnerSpace", explode);

var layout = Compile("LayoutManager.Current.CreateLayout(\"X\");");
Expect(HasError(layout, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses LayoutManager changes", layout);

var addObject = Compile("Db.AddDBObject(new Xrecord());");
Expect(HasError(addObject, ScriptCompiler.GuardDiagnosticId, 1), "read mode refuses Database.AddDBObject", addObject);

var returns = Compile("if (Args.Count > 0) return 1;\nreturn 2;");
Expect(returns.Succeeded && returns.Warnings.All(warning => warning.Code != "CS0162"), "a snippet that returns compiles without unreachable-code warnings", returns);

// Every type name declared in two default namespaces must have an alias, or a snippet using it does not
// compile (CS0104): Civil 3D repeats DBObject, Entity, Surface, Section... from AutoCAD.
var probe = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("AliasProbe", null,
  references.Select(path => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path)));
var aliased = new HashSet<string>(ScriptCompiler.DefaultAliases.Select(alias => alias.Split('=')[0].Trim()), StringComparer.Ordinal);
var declaredIn = new Dictionary<string, List<string>>(StringComparer.Ordinal);
foreach (var ns in ScriptCompiler.DefaultUsings)
{
  Microsoft.CodeAnalysis.INamespaceSymbol? symbol = probe.GlobalNamespace;
  foreach (var part in ns.Split('.'))
  {
    symbol = symbol?.GetNamespaceMembers().FirstOrDefault(member => member.Name == part);
  }

  foreach (var type in symbol?.GetTypeMembers().Where(t => t.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public) ?? Enumerable.Empty<Microsoft.CodeAnalysis.INamedTypeSymbol>())
  {
    if (!declaredIn.TryGetValue(type.Name, out var list)) declaredIn[type.Name] = list = new List<string>();
    if (!list.Contains(ns)) list.Add(ns);
  }
}

var unaliased = declaredIn.Where(pair => pair.Value.Count > 1 && !aliased.Contains(pair.Key)).Select(pair => $"{pair.Key} ({string.Join(", ", pair.Value)})").OrderBy(text => text).ToList();
Expect(unaliased.Count == 0, $"every type name repeated across the default namespaces has an alias{(unaliased.Count == 0 ? string.Empty : ": missing " + string.Join("; ", unaliased))}");

var common = Compile("var o = Open<DBObject>(Db.BlockTableId);\nSection s = null; Table t = null; Group g = null; TransactionManager m = Db.TransactionManager;\nreturn o.Handle.ToString();");
Expect(common.Succeeded, "DBObject, Section, Table, Group and TransactionManager resolve without qualification", common);

Expect(ScriptCompiler.FindScriptLine("   at Script_X.Run() in script:line 17\n   at Other() in other.cs:line 3") == 17, "stack traces map back to the snippet line");

var unique = Compile("return 1;");
var again = Compile("return 1;");
Expect(unique.Succeeded && again.Succeeded && unique.TypeName != again.TypeName, "each run gets its own class and assembly name");

RuntimeChecks.Run((condition, name) => Expect(condition, name));
Console.WriteLine(failures == 0 ? $"PASS ({references.Count} references)" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;

static bool IsManagedAssembly(string path)
{
  try
  {
    System.Reflection.AssemblyName.GetAssemblyName(path);
    return true;
  }
  catch (Exception)
  {
    return false;
  }
}

static string FindRepoRoot()
{
  for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
  {
    if (Directory.Exists(Path.Combine(directory.FullName, "Civil3D-MCP-Plugin")))
    {
      return directory.FullName;
    }
  }

  return Directory.GetCurrentDirectory();
}
