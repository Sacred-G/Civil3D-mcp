using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

/// <summary>
/// Caller check for runCode, the one command that runs arbitrary code. The TCP
/// port is loopback-only but open to every account and integrity level on the
/// machine; the token file is not: it gets a protected DACL that grants only
/// this Windows user access, whatever %LOCALAPPDATA% passes down (another
/// account, such as a sandbox group, may be allowed to read that folder). No
/// mandatory label is set, so a low-integrity process of the same user can
/// still read it. The plugin instance that owns the port writes a new random
/// token on every start; the MCP server reads it before each runCode and
/// sends it as "authToken". Other commands are not
/// gated: none of them runs code, and older servers keep working with them.
/// AutoCAD-free, so tests\RunCodeHarness checks it offline.
/// </summary>
internal static class RpcAuth
{
  internal const string ParameterName = "authToken";

  private static readonly object Sync = new();
  private static string? _token;

  // Fixed, so the server can find it: not plugin.log's CIVIL3D_MCP_LOG_DIR
  // override or its temp fallback (a temp folder may be readable by others).
  // The harness points it at a scratch folder.
  internal static string TokenPath { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Civil3DMcpPlugin", "rpc-token");

  // Called once the TCP server has bound the port. Writes the token through a
  // private temp file and a rename, so the server never reads half of one. On
  // failure (including the permissions) runCode is refused, with the reason,
  // and every other command still works.
  internal static bool Issue()
  {
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    var path = TokenPath;
    var temp = $"{path}.{Guid.NewGuid():N}.tmp";
    try
    {
      if (!Path.IsPathFullyQualified(path))
      {
        throw new IOException("this account has no local application data folder");
      }

      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      WritePrivateFile(temp, token);
      // Same folder, so the rename keeps the file's own DACL.
      File.Move(temp, path, true);
    }
    catch (Exception exception)
    {
      try
      {
        File.Delete(temp);
      }
      catch (Exception)
      {
        // Best effort: a stray temp file next to the token is harmless.
      }

      lock (Sync)
      {
        _token = null;
      }

      PluginLog.Warn("RpcAuth", $"Could not write the runCode token file {path}; runCode is refused until the listener restarts.", exception);
      return false;
    }

    lock (Sync)
    {
      _token = token;
    }

    PluginLog.Info("RpcAuth", $"Wrote the runCode token file {path}.");
    return true;
  }

  // Creates the file with its permissions already set (owner and full control
  // for this user only, nothing inherited), so it is never readable by others,
  // not even between creation and an ACL change. Throws on any failure.
  private static void WritePrivateFile(string path, string token)
  {
    if (!OperatingSystem.IsWindows())
    {
      throw new PlatformNotSupportedException("the token file needs Windows file permissions");
    }

    using var identity = WindowsIdentity.GetCurrent();
    var user = identity.User ?? throw new IOException("this account has no security identifier");
    var security = new FileSecurity();
    security.SetOwner(user);
    security.SetAccessRuleProtection(true, false);
    security.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow));
    using var stream = new FileInfo(path).Create(FileMode.CreateNew, FileSystemRights.FullControl, FileShare.None, 4096, FileOptions.None, security);
    stream.Write(Encoding.ASCII.GetBytes(token));
  }

  // Called when the listener stops. Deletes the file only while it still holds
  // this instance's token: another Civil 3D may own the port and the file now.
  internal static void Revoke()
  {
    string? token;
    lock (Sync)
    {
      token = _token;
      _token = null;
    }

    if (token == null)
    {
      return;
    }

    try
    {
      var path = TokenPath;
      if (File.Exists(path) && string.Equals(File.ReadAllText(path), token, StringComparison.Ordinal))
      {
        File.Delete(path);
      }
    }
    catch (Exception exception)
    {
      PluginLog.Swallow("RpcAuth", "deleting the runCode token file", exception);
    }
  }

  // The first thing runCode does: refuses a missing or wrong token before the
  // script is read or logged. Constant-time comparison; the log line names the
  // reason only (no token, no script).
  internal static void Require(JsonObject? parameters)
  {
    var presented = parameters?[ParameterName] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
    string? expected;
    lock (Sync)
    {
      expected = _token;
    }

    if (expected != null && !string.IsNullOrEmpty(presented)
      && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented), Encoding.UTF8.GetBytes(expected)))
    {
      return;
    }

    if (expected == null)
    {
      PluginLog.Warn("RpcAuth", "Refused runCode: this plugin has no token (its token file could not be written).");
      throw new JsonRpcDispatchException(
        "CIVIL3D.AUTH_REQUIRED",
        $"runCode is unavailable: the Civil 3D plugin could not write its token file ({TokenPath}; the reason is in plugin.log). Fix that, then run C3DMCPSTOP and C3DMCPSTART in Civil 3D (or restart it). Other tools are not affected.");
    }

    PluginLog.Warn("RpcAuth", string.IsNullOrEmpty(presented)
      ? "Refused runCode: the request carried no authToken (an older civil3d-mcp server, or another program)."
      : "Refused runCode: the request's authToken does not match this session's token.");
    throw new JsonRpcDispatchException(
      "CIVIL3D.AUTH_REQUIRED",
      $"runCode was refused: it needs the token this Civil 3D session wrote to {TokenPath}, and the request had none or an old one. The civil3d-mcp server and the Civil 3D plugin must come from the same build: update and rebuild both, restart the MCP server, and retry. Other tools are not affected.");
  }
}
