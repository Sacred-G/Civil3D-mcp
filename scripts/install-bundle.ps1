<#
.SYNOPSIS
  Installs the plugin as an Autodesk ApplicationPlugins bundle (auto-load).

.DESCRIPTION
  The APPLOAD Startup Suite refuses folders that are not in TRUSTEDPATHS when
  SECURELOAD=1, which is the default and which reports failure only as a bare
  "Error" dialog. Bundles under ApplicationPlugins are Autodesk's supported
  deployment path and load at startup without TRUSTEDPATHS or the Startup Suite.

  Installs per-user under %APPDATA%, so no elevation is required. That folder is
  not a trusted location: Civil 3D 2026 shows "Security - Unsigned Executable
  File" for Civil3DMcpPlugin.dll at every start (verified 2026-10-11); answer
  Load Once.

  The new bundle is built in a sibling folder and swapped in only when it is
  complete, so a failed install leaves the previous bundle as it was, and
  files the new build no longer has do not linger in Contents\.

.PARAMETER BundleName
  The bundle folder's name under BundleRoot: a plain folder name, no path.
  ".bundle" is appended when missing.

.PARAMETER Uninstall
  Remove the bundle instead of installing it.
#>
param(
  [string] $SourceDir,
  [string] $BundleRoot = (Join-Path $env:APPDATA "Autodesk\ApplicationPlugins"),
  [string] $BundleName = "Civil3DMcp.bundle",
  [switch] $Uninstall
)

$ErrorActionPreference = 'Stop'

# BundleName is joined under BundleRoot and the result is deleted recursively
# on uninstall and on reinstall, so it must name one folder directly inside
# BundleRoot: no separators, drive, '.' or '..', or characters Windows drops.
if ([string]::IsNullOrWhiteSpace($BundleName) -or
    $BundleName.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0 -or
    $BundleName.IndexOfAny([char[]]@('\', '/', ':')) -ge 0 -or
    $BundleName -eq '.' -or $BundleName -eq '..' -or
    $BundleName.EndsWith('.') -or $BundleName.EndsWith(' ') -or $BundleName.StartsWith(' ')) {
  throw "BundleName '$BundleName' must be a plain folder name such as Civil3DMcp.bundle, with no path."
}
if (-not $BundleName.EndsWith('.bundle', [System.StringComparison]::OrdinalIgnoreCase)) {
  $BundleName = "$BundleName.bundle"
}
if ($BundleName -ieq '.bundle') {
  throw "BundleName '$BundleName' needs a name before .bundle."
}

$repoRoot  = Split-Path -Parent $PSScriptRoot
$bundleDir = Join-Path $BundleRoot $BundleName

# A running Civil 3D holds a lock on the loaded bundle DLL; deleting or copying
# over it fails partway and leaves a partial or mixed-version bundle. Probe the
# actual file locks rather than matching acad.exe by name: plain AutoCAD uses
# the same executable, and it does not load this Civil 3D bundle.
function Get-LockedBundleFile([string] $dir) {
  if (-not (Test-Path -LiteralPath $dir)) { return $null }
  foreach ($f in Get-ChildItem -LiteralPath $dir -Recurse -File -Filter *.dll) {
    $fs = $null
    try {
      $fs = [System.IO.File]::Open($f.FullName, 'Open', 'ReadWrite', 'None')
    } catch [System.IO.IOException] {
      return $f.FullName
    } catch [System.UnauthorizedAccessException] {
      # Access denied reads the same as locked for our purposes: we cannot
      # safely replace the file, so refuse with the friendly message.
      return $f.FullName
    } finally {
      if ($fs) { $fs.Dispose() }
    }
  }
  return $null
}

$locked = Get-LockedBundleFile $bundleDir
if ($locked) {
  throw "'$locked' is loaded by a running Civil 3D. Close it completely, then re-run."
}

# Folders an interrupted earlier run left behind. Their names do not end in
# .bundle, so AutoCAD never loads them. Literal prefixes, not -like: a name
# may contain [ or ]. A moved-aside previous bundle (.old-) is deleted only
# while the bundle itself is in place; otherwise it may be the only copy, so
# it is kept and reported.
$stagingPrefix = ".$BundleName.staging-"
$oldPrefix     = ".$BundleName.old-"
foreach ($leftover in @(Get-ChildItem -LiteralPath $BundleRoot -Directory -Force -ErrorAction SilentlyContinue)) {
  if ($leftover.Name.StartsWith($stagingPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    Remove-Item -LiteralPath $leftover.FullName -Recurse -Force -ErrorAction SilentlyContinue
  } elseif ($leftover.Name.StartsWith($oldPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    if (Test-Path -LiteralPath $bundleDir) {
      Remove-Item -LiteralPath $leftover.FullName -Recurse -Force -ErrorAction SilentlyContinue
    } else {
      Write-Warning "A previous bundle was left at $($leftover.FullName) by an interrupted install; rename it to $BundleName to restore it, or delete it."
    }
  }
}

if ($Uninstall) {
  if (Test-Path -LiteralPath $bundleDir) {
    Remove-Item -LiteralPath $bundleDir -Recurse -Force
    Write-Host "Removed $bundleDir"
  } else {
    Write-Host "Nothing to remove at $bundleDir"
  }
  Write-Host "Restart Civil 3D for the change to take effect."
  # return, not exit: run with -File or .\install-bundle.ps1 both end the
  # same way, and a dot-sourced run does not close the session.
  return
}

if (-not $SourceDir) {
  $SourceDir = Join-Path $repoRoot "Civil3D-MCP-Plugin\bin\Release\net10.0-windows"
}
$dll = Join-Path $SourceDir "Civil3DMcpPlugin.dll"
if (-not (Test-Path -LiteralPath $dll)) {
  throw "Plugin not built at '$dll'. Run scripts\build-2027.ps1 first."
}

# The bundle's RuntimeRequirements series must match the Civil 3D release the
# DLL was compiled against, or AutoCAD silently never loads it. The build's
# target framework (last path segment of the output dir) tells us which:
# net8.0-windows is the 2026 build (R25.1), net10.0-windows the 2027 build (R26.0).
$tfm = Split-Path -Leaf $SourceDir
$series = switch ($tfm) {
  "net8.0-windows"  { "R25.1" }
  "net10.0-windows" { "R26.0" }
  default { throw "Cannot infer the Civil 3D series from '$tfm'. Pass a build output dir named by target framework (net8.0-windows or net10.0-windows)." }
}

$appVersion = (Get-Item -LiteralPath $dll).VersionInfo.FileVersion
if (-not $appVersion) { $appVersion = "1.0.0.0" }

# Stable GUIDs so reinstalls upgrade in place rather than registering a duplicate.
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<ApplicationPackage
  SchemaVersion="1.0"
  AppVersion="$appVersion"
  Author="Sacred-G"
  ProductCode="{3F7C1A94-6E2D-4B85-9C13-5A8E0D2F7B41}"
  UpgradeCode="{8B24E5D7-1C3F-4A69-B0E2-7D9146C8F3A5}"
  Name="Civil 3D MCP Plugin"
  PreferNewestAcross="AppData|ProgramFiles"
  >
  <CompanyDetails Name="Sacred-G" Url="https://github.com/Sacred-G/Civil3D-mcp" Email="" />
  <RuntimeRequirements Platform="Civil3D" SeriesMin="$series" SeriesMax="$series" OS="Win64" SupportPath="./Contents" />
  <Components>
    <ComponentEntry AppName="Civil3DMcp" ModuleName="./Contents/Civil3DMcpPlugin.dll"
                    LoadOnAutoCADStartup="true" LoadOnRequest="false" AppDescription="Civil 3D MCP JSON-RPC bridge">
    </ComponentEntry>
  </Components>
  <DisplayInAppManager>true</DisplayInAppManager>
</ApplicationPackage>
"@

# Build the complete bundle next to the real one first.
$suffix  = [guid]::NewGuid().ToString("N").Substring(0, 8)
$staging = Join-Path $BundleRoot ".$BundleName.staging-$suffix"
$old     = Join-Path $BundleRoot ".$BundleName.old-$suffix"

New-Item -ItemType Directory -Path $BundleRoot -Force | Out-Null
$rollbackError = $null
try {
  $contents = Join-Path $staging "Contents"
  New-Item -ItemType Directory -Path $contents -Force | Out-Null

  # The six Autodesk references are Private=false in the csproj, so they never
  # land in bin\; deny-list them anyway so a hand-copied set can't be shipped.
  $autodeskRefs = @("accoremgd.dll", "AcDbMgd.dll", "acmgd.dll", "AecBaseMgd.dll", "AeccDbMgd.dll", "AeccPressurePipesMgd.dll")
  $copied = 0
  foreach ($f in Get-ChildItem -LiteralPath $SourceDir -File) {
    if ($autodeskRefs -contains $f.Name) { continue }
    Copy-Item -LiteralPath $f.FullName -Destination (Join-Path $contents $f.Name) -Force
    $copied++
  }

  # PackageContents.xml must be UTF-8 without BOM or AutoCAD ignores the bundle.
  [System.IO.File]::WriteAllText(
    (Join-Path $staging "PackageContents.xml"),
    $xml,
    (New-Object System.Text.UTF8Encoding($false)))

  # Swap: move the current bundle aside, move the new one in, then delete the
  # old one. Folder renames in one parent are atomic, and a failed second
  # rename puts the old bundle back.
  $hadBundle = Test-Path -LiteralPath $bundleDir
  if ($hadBundle) { [System.IO.Directory]::Move($bundleDir, $old) }
  try {
    [System.IO.Directory]::Move($staging, $bundleDir)
  } catch {
    if ($hadBundle) {
      try {
        [System.IO.Directory]::Move($old, $bundleDir)
      } catch {
        $rollbackError = $_.Exception.Message
      }
    }
    throw
  }
} catch {
  if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue }
  if ($rollbackError) {
    throw "Install failed and the previous bundle could not be moved back: it is at $old. Rename it to $BundleName to restore it. $($_.Exception.Message) ($rollbackError)"
  }
  throw "Install failed; the previous bundle (if any) is unchanged at $bundleDir. $($_.Exception.Message)"
}

if (Test-Path -LiteralPath $old) {
  Remove-Item -LiteralPath $old -Recurse -Force -ErrorAction SilentlyContinue
  if (Test-Path -LiteralPath $old) { Write-Warning "Could not delete the previous bundle at $old; delete it by hand." }
}

Write-Host "Installed bundle: $bundleDir"
Write-Host "  PackageContents.xml  (AppVersion $appVersion)"
Write-Host "  Contents\            ($copied file(s))"
Write-Host ""
Write-Host "Close Civil 3D COMPLETELY and reopen it. The plugin auto-loads (no NETLOAD,"
Write-Host "no Startup Suite); answer Load Once if Civil 3D asks about Civil3DMcpPlugin.dll."
Write-Host "Verify with C3DMCPSTART (echoes the port)."
