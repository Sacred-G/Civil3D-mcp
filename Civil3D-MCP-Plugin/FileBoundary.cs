using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Civil3DMcpPlugin;

/// <summary>
/// Authoritative filesystem boundary for caller-supplied import and export paths.
/// Paths are canonicalized, restricted to configured roots, and checked against
/// a command-specific extension allowlist before Civil 3D or System.IO sees them.
/// </summary>
internal static class FileBoundary
{
  private const string SharedRootsVariable = "CIVIL3D_FILE_ROOTS";
  private const string ImportRootsVariable = "CIVIL3D_IMPORT_ROOTS";
  private const string ExportRootsVariable = "CIVIL3D_EXPORT_ROOTS";
  private const uint FileFlagBackupSemantics = 0x02000000;
  private const uint FileFlagOpenReparsePoint = 0x00200000;
  private const int FileAttributeTagInfoClass = 9;
  private const uint FileReadAttributes = 0x0080;
  private const uint GenericRead = 0x80000000;
  private const int ErrorSharingViolation = 32;
  private const int ErrorFileNotFound = 2;
  private const int ErrorPathNotFound = 3;

  private static readonly Lazy<string[]> ImportRoots = new(() => LoadRoots(ImportRootsVariable));
  private static readonly Lazy<string[]> ExportRoots = new(() => LoadRoots(ExportRootsVariable));

  public static string ResolveImportPath(string rawPath, params string[] allowedExtensions)
  {
    var path = ResolvePath(rawPath, ImportRoots.Value, "import", allowedExtensions);
    if (!File.Exists(path))
    {
      throw new JsonRpcDispatchException("CIVIL3D.OBJECT_NOT_FOUND", $"Import file was not found: {path}");
    }

    return path;
  }

  public static string ResolveExportPath(
    string rawPath,
    bool overwrite,
    params string[] allowedExtensions)
  {
    var path = ResolvePath(rawPath, ExportRoots.Value, "export", allowedExtensions);
    if (File.Exists(path) && !overwrite)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.CONFLICT",
        $"Output file already exists: {path}. Set overwrite=true to replace it explicitly.");
    }

    return path;
  }

  public static string WriteAllTextAtomic(
    string rawPath,
    string content,
    Encoding encoding,
    bool overwrite,
    params string[] allowedExtensions)
  {
    var path = ResolveExportPath(rawPath, overwrite, allowedExtensions);
    var directory = Path.GetDirectoryName(path)
      ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Output path must include a directory.");
    using var directoryLock = LockExportDirectoryChain(directory);

    var tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
    try
    {
      File.WriteAllText(tempPath, content, encoding);
      File.Move(tempPath, path, overwrite);
      return path;
    }
    catch (IOException) when (!overwrite && File.Exists(path))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.CONFLICT",
        $"Output file already exists: {path}. Set overwrite=true to replace it explicitly.");
    }
    catch (JsonRpcDispatchException)
    {
      throw;
    }
    catch (Exception exception)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to write output file '{path}': {exception.Message}");
    }
    finally
    {
      try
      {
        if (File.Exists(tempPath))
        {
          File.Delete(tempPath);
        }
      }
      catch
      {
        // Preserve the original operation result. Stale temp files use a
        // hidden, collision-resistant name and can be removed later.
      }
    }
  }

  /// <summary>
  /// Re-checks a file this plugin just wrote for an external reader (the
  /// publish DSD, whose DWF= line tells -PUBLISH where to write) and pins it
  /// for that reader. The entry is opened without following links, must be a
  /// regular file with a single hard link, and must hold exactly
  /// <paramref name="expectedContent"/>; otherwise CIVIL3D.PATH_NOT_ALLOWED.
  /// The returned handle shares read access only, so while it is held nobody
  /// can write, delete, rename or replace the file. Dispose it after the reader
  /// has finished.
  /// </summary>
  public static IDisposable HoldVerifiedFile(string resolvedPath, string expectedContent, Encoding encoding)
  {
    var path = Path.GetFullPath(resolvedPath);
    var stream = OpenCheckedOutput(path, "it was not used")
      ?? throw new JsonRpcDispatchException(
        "CIVIL3D.PATH_NOT_ALLOWED",
        $"'{path}' was removed by another process before it was used.");
    try
    {
      var expected = encoding.GetBytes(expectedContent);
      var matches = stream.Length == expected.Length;
      if (matches)
      {
        var actual = new byte[expected.Length];
        stream.ReadExactly(actual);
        matches = actual.AsSpan().SequenceEqual(expected);
      }

      if (!matches)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.PATH_NOT_ALLOWED",
          $"'{path}' was changed by another process after it was written; it was not used.");
      }

      return stream;
    }
    catch
    {
      stream.Dispose();
      throw;
    }
  }

  private static string ResolvePath(
    string rawPath,
    IReadOnlyCollection<string> roots,
    string operation,
    IReadOnlyCollection<string> allowedExtensions)
  {
    if (string.IsNullOrWhiteSpace(rawPath))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"A non-empty {operation} path is required.");
    }

    if (!Path.IsPathFullyQualified(rawPath))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_INPUT",
        $"The {operation} path must be absolute: {rawPath}");
    }

    string canonicalPath;
    try
    {
      canonicalPath = Path.GetFullPath(rawPath);
    }
    catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Invalid {operation} path: {exception.Message}");
    }

    var matchedRoot = roots.FirstOrDefault(root => IsWithinRoot(canonicalPath, root));
    if (matchedRoot == null)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.PATH_NOT_ALLOWED",
        $"The {operation} path is outside the configured roots: {canonicalPath}");
    }

    RejectReparsePointTraversal(canonicalPath, matchedRoot);

    var allowed = allowedExtensions
      .Select(NormalizeExtension)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var extension = Path.GetExtension(canonicalPath);
    if (allowed.Count > 0 && !allowed.Contains(extension))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_TYPE_NOT_ALLOWED",
        $"Extension '{extension}' is not allowed for this operation. Allowed extensions: {string.Join(", ", allowed.Order())}.");
    }

    return canonicalPath;
  }

  private static string[] LoadRoots(string operationVariable)
  {
    var configured = Environment.GetEnvironmentVariable(operationVariable);
    if (string.IsNullOrWhiteSpace(configured))
    {
      configured = Environment.GetEnvironmentVariable(SharedRootsVariable);
    }

    var roots = SplitRoots(configured).ToList();
    if (roots.Count == 0)
    {
      var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      if (!string.IsNullOrWhiteSpace(documents))
      {
        roots.Add(documents);
      }
    }

    return roots
      .Select(Path.GetFullPath)
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToArray();
  }

  private static IEnumerable<string> SplitRoots(string? configured)
  {
    if (string.IsNullOrWhiteSpace(configured))
    {
      yield break;
    }

    foreach (var root in configured.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
      if (!Path.IsPathFullyQualified(root))
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.INVALID_CONFIGURATION",
          $"Configured filesystem root must be absolute: {root}");
      }

      yield return root;
    }
  }

  private static bool IsWithinRoot(string path, string root)
  {
    var relative = Path.GetRelativePath(root, path);
    return !Path.IsPathRooted(relative)
      && !string.Equals(relative, "..", StringComparison.Ordinal)
      && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
      && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
  }

  private static void RejectReparsePointTraversal(string path, string root)
  {
    var relative = Path.GetRelativePath(root, path);
    if (relative == ".")
    {
      return;
    }

    var current = root;
    foreach (var segment in relative.Split(
      [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
      StringSplitOptions.RemoveEmptyEntries))
    {
      current = Path.Combine(current, segment);
      if (!Directory.Exists(current) && !File.Exists(current))
      {
        break;
      }

      try
      {
        if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.PATH_NOT_ALLOWED",
            $"Filesystem links and junctions are not allowed in caller-supplied paths: {current}");
        }
      }
      catch (JsonRpcDispatchException)
      {
        throw;
      }
      catch (Exception exception)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.FILE_IO_ERROR",
          $"Unable to validate filesystem path '{current}': {exception.Message}");
      }
    }
  }

  /// <summary>
  /// Starts an output written by an external writer (the AutoCAD plotter,
  /// -PLOT / -PUBLISH) to an export path already resolved by
  /// <see cref="ResolveExportPath"/>. Call it immediately before starting the
  /// writer, point the writer at <see cref="ExternalWriterOutput.FinalPath"/>,
  /// and call <see cref="ExternalWriterOutput.Commit{T}"/> when it finishes.
  ///
  /// Why the writer gets the final path. An earlier version had the plotter
  /// write to a hidden temporary name and renamed it afterwards. That closed
  /// the link race below completely, but the plotter reports the name it
  /// wrote to the device's "open in viewer when done" option, so the viewer
  /// opened the temporary name after it had been renamed away ("file not
  /// found"). The plotter therefore writes the final name, protected like this:
  ///
  /// 1. The directory chain from the export root down is created and locked
  ///    for the whole write (no segment can be renamed, deleted or swapped for
  ///    a junction while the lock is held), so the final path stays inside the
  ///    export roots.
  /// 2. The final name itself must be absent or a regular file. A symbolic
  ///    link, junction or directory there is refused (PATH_NOT_ALLOWED /
  ///    CONFLICT) and left untouched.
  /// 3. An existing regular file is refused unless overwrite is set; with
  ///    overwrite it is renamed (never followed) to a backup name beside it,
  ///    and the name is checked to be absent again. The plotter then creates a
  ///    new file, which is the only case the -PLOT / -PUBLISH prompt chains
  ///    have been verified for (they were established with any existing PDF
  ///    deleted first). A placeholder file held open at the final name
  ///    would keep the name from being replaced during the write, but whether
  ///    the PDF driver then prompts, or needs to delete/rename the target, is
  ///    unverified, so no placeholder is used.
  /// 4. After the writer finishes, Commit opens the final name once, without
  ///    following links, and requires a regular file with a single hard link
  ///    before anything reads it. The content check then reads that same open
  ///    handle, never the name again, and the handle denies writers and
  ///    delete/rename until the check is done, so the entry cannot be swapped
  ///    for a link between the check and the read. A symbolic link or an extra
  ///    hard link planted there during the write is removed (the link, never
  ///    its target) and the call fails with PATH_NOT_ALLOWED.
  ///
  /// Trade-off: between step 3 and the plotter opening the file, a process
  /// with write access to the export folder could still plant a link at the
  /// final name; step 4 detects and removes it and fails the call, but a write
  /// that already went through the link cannot be undone. (Creating a
  /// symbolic link also needs the symlink privilege; a junction cannot stand
  /// in for a file.)
  ///
  /// A failed or unverified write never destroys the previous file: disposing
  /// without a successful Commit removes whatever the writer left at the final
  /// name and renames the backup back. Commit deletes the backup.
  /// </summary>
  public static ExternalWriterOutput BeginExternalWrite(string resolvedPath, bool overwrite)
  {
    var finalPath = Path.GetFullPath(resolvedPath);
    var directory = Path.GetDirectoryName(finalPath)
      ?? throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Output path must include a directory.");
    var directoryLock = LockExportDirectoryChain(directory);
    string? backupPath = null;
    try
    {
      var attributes = GetEntryAttributes(finalPath);
      if (attributes is FileAttributes existing)
      {
        if ((existing & FileAttributes.ReparsePoint) != 0)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.PATH_NOT_ALLOWED",
            $"Filesystem links and junctions are not allowed in caller-supplied paths: {finalPath}");
        }

        if ((existing & FileAttributes.Directory) != 0)
        {
          throw new JsonRpcDispatchException("CIVIL3D.CONFLICT", $"Output path is an existing directory: {finalPath}");
        }

        if (!overwrite)
        {
          throw new JsonRpcDispatchException(
            "CIVIL3D.CONFLICT",
            $"Output file already exists: {finalPath}. Set overwrite=true to replace it explicitly.");
        }

        backupPath = Path.Combine(directory, $".{Path.GetFileName(finalPath)}.{Guid.NewGuid():N}.mcp-bak");
        try
        {
          // A rename moves the directory entry itself; it never follows a link.
          File.Move(finalPath, backupPath);
        }
        catch (Exception exception)
        {
          backupPath = null;
          throw new JsonRpcDispatchException(
            "CIVIL3D.FILE_IO_ERROR",
            $"Could not replace '{finalPath}' (is it open in another program?): {exception.Message}");
        }
      }

      if (GetEntryAttributes(finalPath) != null)
      {
        throw new JsonRpcDispatchException(
          "CIVIL3D.CONFLICT",
          $"Output file '{finalPath}' was re-created by another process before the write started.");
      }

      return new ExternalWriterOutput(finalPath, backupPath, directoryLock);
    }
    catch
    {
      if (backupPath != null)
      {
        TryRestoreBackup(finalPath, backupPath);
      }

      directoryLock.Dispose();
      throw;
    }
  }

  internal sealed class ExternalWriterOutput : IDisposable
  {
    private IDisposable? _directoryLock;
    private bool _committed;

    internal ExternalWriterOutput(string finalPath, string? backupPath, IDisposable directoryLock)
    {
      FinalPath = finalPath;
      BackupPath = backupPath;
      _directoryLock = directoryLock;
    }

    /// <summary>The path the external writer must write.</summary>
    public string FinalPath { get; }

    /// <summary>Where the replaced file waits until Commit (null when there was none).</summary>
    public string? BackupPath { get; }

    /// <summary>
    /// Verifies the writer's output is a regular, singly-linked file at the
    /// final name, runs <paramref name="verifyContent"/> on it, and only then
    /// discards the backup of the replaced file. <paramref name="verifyContent"/>
    /// gets the final path (for messages) and a read-only stream over the
    /// handle that was checked, or null when nothing was written; it must read
    /// the stream, not reopen the path. Any failure leaves the output to
    /// Dispose, which removes it and restores the backup.
    /// </summary>
    public T Commit<T>(Func<string, FileStream?, T> verifyContent)
    {
      if (_directoryLock == null)
      {
        throw new ObjectDisposedException(nameof(ExternalWriterOutput));
      }

      T result;
      using (var output = OpenCheckedOutput(FinalPath, "it was removed"))
      {
        // A missing file is reported by verifyContent (for example "no PDF was written").
        result = verifyContent(FinalPath, output);

        // The handle denies delete/rename, so it is still the entry at the
        // final name; confirm no hard link was added while it was read.
        if (output != null)
        {
          RequireSingleRegularFile(output.SafeFileHandle, FinalPath, "it was removed");
        }
      }

      _committed = true;
      if (BackupPath != null)
      {
        try
        {
          File.Delete(BackupPath);
        }
        catch (Exception exception)
        {
          PluginLog.Warn("FileBoundary", $"Could not delete the replaced file's backup '{BackupPath}': {exception.Message}");
        }
      }

      return result;
    }

    public void Dispose()
    {
      if (_directoryLock == null)
      {
        return;
      }

      try
      {
        if (!_committed)
        {
          // Everything at the final name now was created during this write
          // (it was absent when the write started), so it is removed; a link
          // is removed itself, never its target.
          var removed = TryRemoveEntry(FinalPath);
          if (BackupPath != null)
          {
            if (removed)
            {
              TryRestoreBackup(FinalPath, BackupPath);
            }
            else
            {
              PluginLog.Warn("FileBoundary", $"The failed output at '{FinalPath}' could not be removed; the previous file was kept at '{BackupPath}'.");
            }
          }
        }
      }
      finally
      {
        _directoryLock.Dispose();
        _directoryLock = null;
      }
    }
  }

  /// <summary>The entry's own attributes (a link is not followed), or null when nothing is there.</summary>
  private static FileAttributes? GetEntryAttributes(string path)
  {
    try
    {
      return File.GetAttributes(path);
    }
    catch (FileNotFoundException)
    {
      return null;
    }
    catch (DirectoryNotFoundException)
    {
      return null;
    }
    catch (Exception exception)
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to validate filesystem path '{path}': {exception.Message}");
    }
  }

  /// <summary>
  /// Opens the entry at <paramref name="path"/> itself (a link is not
  /// followed) for reading, sharing read access only, so while the stream is
  /// open nobody can write, delete, rename or replace the entry. Returns null
  /// when nothing is there; throws PATH_NOT_ALLOWED unless it is a regular file
  /// with a single hard link.
  /// </summary>
  private static FileStream? OpenCheckedOutput(string path, string outcome)
  {
    var handle = CreateFileW(
      path,
      GenericRead | FileReadAttributes,
      FileShare.Read,
      IntPtr.Zero,
      FileMode.Open,
      FileFlagBackupSemantics | FileFlagOpenReparsePoint,
      IntPtr.Zero);
    if (handle.IsInvalid)
    {
      var error = Marshal.GetLastWin32Error();
      handle.Dispose();
      if (error is ErrorFileNotFound or ErrorPathNotFound)
      {
        return null;
      }

      var hint = error == ErrorSharingViolation ? " (is it still open in another program?)" : string.Empty;
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to open output file '{path}'{hint}: {new Win32Exception(error).Message}");
    }

    try
    {
      RequireSingleRegularFile(handle, path, outcome);
      return new FileStream(handle, FileAccess.Read);
    }
    catch
    {
      handle.Dispose();
      throw;
    }
  }

  private static void RequireSingleRegularFile(SafeFileHandle handle, string path, string outcome)
  {
    if (!GetFileInformationByHandle(handle, out var information))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to inspect output file '{path}': {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
    }

    var attributes = (FileAttributes)information.FileAttributes;
    var links = information.NumberOfLinks;
    if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0 || links != 1)
    {
      var kind = (attributes & FileAttributes.ReparsePoint) != 0
        ? "a filesystem link"
        : (attributes & FileAttributes.Directory) != 0 ? "a directory" : $"a file with {links} hard links";
      throw new JsonRpcDispatchException(
        "CIVIL3D.PATH_NOT_ALLOWED",
        $"The output at '{path}' is {kind}, not the regular file that was written; {outcome}.");
    }
  }

  /// <summary>Removes the entry at <paramref name="path"/> (a link itself, never its target). True when nothing is left.</summary>
  private static bool TryRemoveEntry(string path)
  {
    try
    {
      var attributes = GetEntryAttributes(path);
      if (attributes == null)
      {
        return true;
      }

      if ((attributes.Value & FileAttributes.Directory) != 0)
      {
        if ((attributes.Value & FileAttributes.ReparsePoint) == 0)
        {
          // A real directory was not created by a file writer; leave it.
          return false;
        }

        // Non-recursive delete of a junction / directory link removes the link only.
        Directory.Delete(path, recursive: false);
      }
      else
      {
        File.Delete(path);
      }

      return GetEntryAttributes(path) == null;
    }
    catch (Exception exception)
    {
      PluginLog.Warn("FileBoundary", $"Could not remove '{path}': {exception.Message}");
      return false;
    }
  }

  private static void TryRestoreBackup(string finalPath, string backupPath)
  {
    try
    {
      File.Move(backupPath, finalPath);
    }
    catch (Exception exception)
    {
      PluginLog.Warn("FileBoundary", $"Could not restore '{finalPath}' from its backup; the previous file was kept at '{backupPath}': {exception.Message}");
    }
  }

  private static string NormalizeExtension(string extension) =>
    extension.StartsWith('.') ? extension : $".{extension}";

  private static DirectoryChainLock LockExportDirectoryChain(string directory)
  {
    var canonicalDirectory = Path.GetFullPath(directory);
    var matchedRoot = ExportRoots.Value
      .Where(root => IsWithinRoot(canonicalDirectory, root))
      .OrderByDescending(root => root.Length)
      .FirstOrDefault()
      ?? throw new JsonRpcDispatchException(
        "CIVIL3D.PATH_NOT_ALLOWED",
        $"The export path is outside the configured roots: {canonicalDirectory}");

    if (!Directory.Exists(matchedRoot))
    {
      throw new JsonRpcDispatchException(
        "CIVIL3D.INVALID_CONFIGURATION",
        $"Configured export root does not exist: {matchedRoot}");
    }

    var handles = new List<SafeFileHandle>();
    try
    {
      var current = Path.GetFullPath(matchedRoot);
      handles.Add(OpenLockedDirectory(current));
      var relative = Path.GetRelativePath(current, canonicalDirectory);
      if (relative != ".")
      {
        foreach (var segment in relative.Split(
          [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
          StringSplitOptions.RemoveEmptyEntries))
        {
          var next = Path.Combine(current, segment);
          if (!Directory.Exists(next))
          {
            // The parent handle excludes FILE_SHARE_DELETE, so it cannot be
            // swapped for a junction between creation and the next handle open.
            Directory.CreateDirectory(next);
          }
          handles.Add(OpenLockedDirectory(next));
          current = next;
        }
      }
      return new DirectoryChainLock(handles);
    }
    catch
    {
      foreach (var handle in handles) handle.Dispose();
      throw;
    }
  }

  private static SafeFileHandle OpenLockedDirectory(string path)
  {
    var handle = CreateFileW(
      path,
      0,
      FileShare.ReadWrite,
      IntPtr.Zero,
      FileMode.Open,
      FileFlagBackupSemantics | FileFlagOpenReparsePoint,
      IntPtr.Zero);
    if (handle.IsInvalid)
    {
      var error = new Win32Exception(Marshal.GetLastWin32Error());
      handle.Dispose();
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to lock filesystem directory '{path}': {error.Message}");
    }

    if (!GetFileInformationByHandleEx(
      handle,
      FileAttributeTagInfoClass,
      out var information,
      (uint)Marshal.SizeOf<FileAttributeTagInfo>()))
    {
      var error = new Win32Exception(Marshal.GetLastWin32Error());
      handle.Dispose();
      throw new JsonRpcDispatchException(
        "CIVIL3D.FILE_IO_ERROR",
        $"Unable to inspect filesystem directory '{path}': {error.Message}");
    }

    if ((information.FileAttributes & FileAttributes.ReparsePoint) != 0)
    {
      handle.Dispose();
      throw new JsonRpcDispatchException(
        "CIVIL3D.PATH_NOT_ALLOWED",
        $"Filesystem links and junctions are not allowed in caller-supplied paths: {path}");
    }
    return handle;
  }

  private sealed class DirectoryChainLock(List<SafeFileHandle> handles) : IDisposable
  {
    public void Dispose()
    {
      for (var index = handles.Count - 1; index >= 0; index--)
        handles[index].Dispose();
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct FileAttributeTagInfo
  {
    public FileAttributes FileAttributes;
    public uint ReparseTag;
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  private static extern SafeFileHandle CreateFileW(
    string fileName,
    uint desiredAccess,
    FileShare shareMode,
    IntPtr securityAttributes,
    FileMode creationDisposition,
    uint flagsAndAttributes,
    IntPtr templateFile);

  [StructLayout(LayoutKind.Sequential)]
  private struct ByHandleFileInformation
  {
    public uint FileAttributes;
    public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
    public uint VolumeSerialNumber;
    public uint FileSizeHigh;
    public uint FileSizeLow;
    public uint NumberOfLinks;
    public uint FileIndexHigh;
    public uint FileIndexLow;
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetFileInformationByHandle(SafeFileHandle file, out ByHandleFileInformation fileInformation);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool GetFileInformationByHandleEx(
    SafeFileHandle file,
    int fileInformationClass,
    out FileAttributeTagInfo fileInformation,
    uint bufferSize);
}
