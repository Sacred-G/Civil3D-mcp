// Offline checks of runCode's AutoCAD-free runtime helpers, called from
// Program.cs: the Log cap (RunCodeOutput.cs) and the runCode token (RpcAuth.cs).
// RunTransaction (RunCodeScript.cs) needs AutoCAD at runtime, so it is only
// compiled here (with the plugin build) and checked live in Civil 3D.
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;
using Civil3DMcpPlugin;

internal static class RuntimeChecks
{
  internal static void Run(Action<bool, string> expect)
  {
    CheckOutput(expect);
    CheckAuth(expect);
  }

  private static void CheckOutput(Action<bool, string> expect)
  {
    var full = new StringBuilder();
    expect(RunCodeOutput.AppendLine(full, new string('x', 50), 10) && full.Length == 10 && full[^1] == '\n',
      "Log cuts one oversized line to the room left (cap 10) and reports the cut");
    expect(RunCodeOutput.AppendLine(full, "more", 10) && full.Length == 10, "Log refuses further lines at the cap");

    var fits = new StringBuilder();
    expect(!RunCodeOutput.AppendLine(fits, "abc", 10) && fits.ToString() == "abc\n", "a line that fits is appended with its newline");
    expect(!RunCodeOutput.AppendLine(fits, null, 10) && fits.ToString() == "abc\n\n", "Log(null) appends an empty line");

    var exact = new StringBuilder();
    expect(!RunCodeOutput.AppendLine(exact, "abc", 4) && exact.Length == 4, "a line that fills the cap exactly is not cut");
    expect(RunCodeOutput.AppendLine(exact, string.Empty, 4) && exact.Length == 4, "at the cap even an empty line is refused");

    // Room 3 for "a" + U+1F600 (a surrogate pair): keep "a", drop the whole pair.
    var pair = new StringBuilder("1234567");
    var pairCut = RunCodeOutput.AppendLine(pair, "a\U0001F600", 10);
    var text = pair.ToString();
    expect(pairCut && text == "1234567a\n" && !char.IsHighSurrogate(text[^2]), "a cut never leaves half of a surrogate pair");
  }

  private static void CheckAuth(Action<bool, string> expect)
  {
    var folder = Path.Combine(Path.GetTempPath(), "RunCodeHarness-" + Guid.NewGuid().ToString("N"));
    var defaultPath = RpcAuth.TokenPath;
    try
    {
      RpcAuth.TokenPath = Path.Combine(folder, "rpc-token");
      expect(RpcAuth.Issue() && File.Exists(RpcAuth.TokenPath), "Issue writes the token file, creating its folder");
      var token = File.ReadAllText(RpcAuth.TokenPath);
      expect(token.Length == 64 && token.All(Uri.IsHexDigit), "the token is 64 hex characters");
      expect(Directory.GetFiles(folder).Length == 1, "Issue leaves no temp file behind");
      if (OperatingSystem.IsWindows())
      {
        expect(IsPrivate(RpcAuth.TokenPath), "the token file has a protected DACL with one allow rule, for this user (also its owner)");
      }

      expect(Accepts(token), "Require accepts this session's token");
      expect(Refused(null) && Refused(new JsonObject()), "Require refuses a call without authToken");
      expect(Refused(Token(string.Empty)), "Require refuses an empty authToken");
      expect(Refused(new JsonObject { [RpcAuth.ParameterName] = 42 }), "Require refuses an authToken that is not a string");
      var wrong = token[..^1] + (token[^1] == '0' ? '1' : '0');
      expect(Refused(Token(wrong)), "Require refuses a wrong authToken");
      expect(PluginLog.Warnings.Count >= 4 && PluginLog.Warnings.All(line => !line.Contains(token) && !line.Contains(wrong)),
        "each refusal is logged, without the token");

      RpcAuth.Issue();
      var second = File.ReadAllText(RpcAuth.TokenPath);
      expect(second != token && Refused(Token(token)) && Accepts(second), "a restart issues a new token and refuses the old one");

      File.WriteAllText(RpcAuth.TokenPath, "another instance");
      RpcAuth.Revoke();
      expect(File.ReadAllText(RpcAuth.TokenPath) == "another instance", "Revoke keeps a token file another instance wrote");
      expect(Refused(Token(second)), "after Revoke runCode is refused");

      // Replaces the file written above with the folder's inherited permissions.
      RpcAuth.Issue();
      if (OperatingSystem.IsWindows())
      {
        expect(IsPrivate(RpcAuth.TokenPath), "a token file that replaces one with inherited permissions is private too");
      }

      RpcAuth.Revoke();
      expect(!File.Exists(RpcAuth.TokenPath), "Revoke deletes its own token file");

      RpcAuth.TokenPath = "relative-rpc-token";
      expect(!RpcAuth.Issue() && Refused(Token(second), "could not write its token file"),
        "a token file that cannot be written refuses runCode and says why");
    }
    finally
    {
      RpcAuth.Revoke();
      RpcAuth.TokenPath = defaultPath;
      try
      {
        Directory.Delete(folder, true);
      }
      catch (IOException)
      {
      }
    }
  }

  [SupportedOSPlatform("windows")]
  private static bool IsPrivate(string path)
  {
    using var identity = WindowsIdentity.GetCurrent();
    var security = new FileInfo(path).GetAccessControl();
    var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().ToList();
    return security.AreAccessRulesProtected
      && Equals(security.GetOwner(typeof(SecurityIdentifier)), identity.User)
      && rules.Count == 1
      && rules[0].AccessControlType == AccessControlType.Allow
      && Equals(rules[0].IdentityReference, identity.User);
  }

  private static JsonObject Token(string token) => new() { [RpcAuth.ParameterName] = token };

  private static bool Accepts(string token)
  {
    try
    {
      RpcAuth.Require(Token(token));
      return true;
    }
    catch (JsonRpcDispatchException)
    {
      return false;
    }
  }

  private static bool Refused(JsonObject? parameters, string because = "Other tools are not affected")
  {
    try
    {
      RpcAuth.Require(parameters);
      return false;
    }
    catch (JsonRpcDispatchException exception)
    {
      return exception.Code == "CIVIL3D.AUTH_REQUIRED" && exception.Message.Contains(because);
    }
  }
}

namespace Civil3DMcpPlugin
{
  // Stand-ins for the plugin types RpcAuth.cs uses: the real ones live in a
  // file that needs AutoCAD (PluginRuntime.cs) or write the real plugin.log.
  internal sealed class JsonRpcDispatchException : Exception
  {
    public JsonRpcDispatchException(string code, string message) : base(message)
    {
      Code = code;
    }

    public string Code { get; }
  }

  internal static class PluginLog
  {
    internal static readonly List<string> Warnings = new();

    public static void Info(string component, string message, Exception? exception = null)
    {
    }

    public static void Warn(string component, string message, Exception? exception = null) => Warnings.Add(message);

    public static void Swallow(string component, string operation, Exception exception)
    {
    }
  }
}
