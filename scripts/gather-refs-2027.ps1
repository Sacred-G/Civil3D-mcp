<#
.SYNOPSIS
  Stages the six Civil 3D managed reference assemblies into C_References/.

.DESCRIPTION
  Civil3DMcpPlugin.csproj expects all six references in one directory
  ($(Civil3DReferencesPath), default ..\C_References). A Civil 3D 2027 install
  spreads them across three folders, so the README's single-folder guidance
  does not apply:

      accoremgd / acdbmgd / acmgd   -> <AcadRoot>\
      AecBaseMgd                    -> <AcadRoot>\ACA\
      AeccDbMgd / AeccPressurePipes -> <AcadRoot>\C3D\

  C_References/ is gitignored; these are licensed Autodesk assemblies and must
  never be committed.
#>
param(
  [string] $AcadRoot = "C:\Program Files\Autodesk\AutoCAD 2027",
  [string] $Destination,
  [switch] $Force
)

$ErrorActionPreference = 'Stop'

# SHA-256 through .NET rather than Get-FileHash: Windows PowerShell 5.1 started from a
# PowerShell 7 prompt inherits 7's PSModulePath and cannot load the module Get-FileHash lives in.
function Get-Sha256([string] $Path) {
  $stream = [System.IO.File]::OpenRead($Path)
  try { [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($stream)) }
  finally { $stream.Dispose() }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $Destination) { $Destination = Join-Path $repoRoot "C_References" }

if (-not (Test-Path $AcadRoot)) {
  throw "Civil 3D install not found at '$AcadRoot'. Pass -AcadRoot <path>."
}

# Canonical file name -> subdirectory of $AcadRoot to search.
$required = [ordered]@{
  "accoremgd.dll"              = ""
  "AcDbMgd.dll"                = ""
  "acmgd.dll"                  = ""
  "AecBaseMgd.dll"             = "ACA"
  "AeccDbMgd.dll"              = "C3D"
  "AeccPressurePipesMgd.dll"   = "C3D"
}

if (-not (Test-Path $Destination)) {
  New-Item -ItemType Directory -Path $Destination | Out-Null
  Write-Host "Created $Destination"
}

$missing = @()
foreach ($name in $required.Keys) {
  $subDir = $required[$name]
  $searchDir = if ($subDir) { Join-Path $AcadRoot $subDir } else { $AcadRoot }
  $target = Join-Path $Destination $name

  # NTFS is case-insensitive, but match defensively so casing drift in the
  # install (acdbmgd.dll vs AcDbMgd.dll) still resolves.
  $source = Get-ChildItem -Path $searchDir -Filter $name -File -ErrorAction SilentlyContinue |
            Select-Object -First 1
  if (-not $source) {
    Write-Warning "  MISSING $name (looked in $searchDir)"
    $missing += $name
    continue
  }

  # Never trust an existing copy blindly: C_References is also the documented
  # staging folder for 2026 references, so a stale set left there would feed
  # the 2027 build the wrong assemblies. Skip only a byte-identical copy;
  # refresh anything else.
  if ((Test-Path $target) -and -not $Force) {
    # A locked or unreadable file (antivirus scan, concurrent build) must not
    # abort the run half-staged under ErrorActionPreference=Stop; treat an
    # unhashable file as different and let the copy below decide.
    $identical = $false
    try {
      $identical = (Get-Sha256 $target) -eq (Get-Sha256 $source.FullName)
    } catch {
      Write-Warning "  could not hash $name ($($_.Exception.Message)); refreshing"
    }
    if ($identical) {
      Write-Host "  skip    $name (already staged, matches source)"
      continue
    }
    Write-Host "  refresh $name (staged copy differs from source)"
  }

  Copy-Item -Path $source.FullName -Destination $target -Force
  $version = (Get-Item $target).VersionInfo.FileVersion
  Write-Host ("  copied  {0,-26} <- {1}  [v{2}]" -f $name, $source.DirectoryName, $version)
}

if ($missing.Count -gt 0) {
  throw "Could not locate: $($missing -join ', '). Check the Civil 3D install is complete."
}

Write-Host ""
Write-Host "All six references staged in $Destination"
