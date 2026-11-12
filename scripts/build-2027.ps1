<#
.SYNOPSIS
  Builds the Civil 3D MCP plugin against staged 2027 references, and optionally
  deploys it as an auto-loading ApplicationPlugins bundle.

.DESCRIPTION
  The csproj targets net8.0-windows for Civil 3D 2026. Civil 3D 2027 ships
  managed assemblies built against .NET 10, which fail CS1705 against a net8.0
  reference set, so this script overrides the target framework on the command
  line instead of editing the csproj -- the repo default stays valid for 2026.

  Run scripts\gather-refs-2027.ps1 first to stage the six Autodesk references.

  -Install deploys via scripts\install-bundle.ps1. Bundles under
  ApplicationPlugins auto-load, which the APPLOAD Startup Suite cannot do while
  SECURELOAD=1 and TRUSTEDPATHS is empty. The per-user bundle folder is not a
  trusted location, so Civil 3D asks about the unsigned DLL at each start
  (answer Load Once).

.EXAMPLE
  .\scripts\build-2027.ps1 -Install
#>
param(
  [ValidateSet("Release", "Debug")]
  [string] $Configuration = "Release",
  [string] $ReferencesPath,
  [string] $TargetFramework = "net10.0-windows",
  [switch] $Install
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$project  = Join-Path $repoRoot "Civil3D-MCP-Plugin\Civil3DMcpPlugin.csproj"

if (-not $ReferencesPath) { $ReferencesPath = Join-Path $repoRoot "C_References" }

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw "dotnet SDK not found on PATH."
}
if (-not (Test-Path $ReferencesPath)) {
  throw "References not staged at '$ReferencesPath'. Run scripts\gather-refs-2027.ps1 first."
}

Write-Host "Building $Configuration ($TargetFramework) against $ReferencesPath"

# Restore must see the overridden TFM too, or the build fails NETSDK1005
# because project.assets.json has no target for it.
& dotnet restore $project "/p:Civil3DReferencesPath=$ReferencesPath" "/p:TargetFramework=$TargetFramework"
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE." }

& dotnet build $project -c $Configuration --no-restore `
  "/p:Civil3DReferencesPath=$ReferencesPath" "/p:TargetFramework=$TargetFramework" -v minimal
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE." }

# The TFM-overridden restore rewrote the shared obj\project.assets.json with
# net10-only targets, which would break a later default (2026/net8) build run
# with --no-restore. Restore the default TFM back so the working tree is left
# exactly as a 2026 build expects. Best-effort: the 2027 output already exists.
& dotnet restore $project "/p:Civil3DReferencesPath=$ReferencesPath"
if ($LASTEXITCODE -ne 0) {
  Write-Warning "Post-build default-TFM restore failed; run 'dotnet restore' before a 2026 build."
}

$outputDir = Join-Path $repoRoot "Civil3D-MCP-Plugin\bin\$Configuration\$TargetFramework"
$builtDll  = Join-Path $outputDir "Civil3DMcpPlugin.dll"
if (-not (Test-Path $builtDll)) {
  throw "Build reported success but $builtDll is missing."
}

Write-Host ""
Write-Host "Built: $builtDll"

if (-not $Install) {
  Write-Host "Re-run with -Install to deploy it as an auto-loading bundle."
  exit 0
}

# install-bundle.ps1 probes the deployed bundle's file locks itself and refuses
# to half-deploy over a loaded DLL, so no process check is needed here (matching
# acad.exe by name would also block on plain AutoCAD, which never loads this bundle).
& (Join-Path $PSScriptRoot "install-bundle.ps1") -SourceDir $outputDir
if ($LASTEXITCODE -ne 0) { throw "install-bundle.ps1 failed with exit code $LASTEXITCODE." }
