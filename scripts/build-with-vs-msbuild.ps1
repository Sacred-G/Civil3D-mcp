param(
    [string] $Configuration = "Release",
    [string] $Project = "Civil3D-AI-CoPilot-master\Cad AI Agent.csproj"
)

$ErrorActionPreference = 'Stop'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuildPath = $null
if (Test-Path $vswhere) {
    $inst = & $vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath 2>$null
    if ($inst) {
        $candidate = Join-Path $inst 'MSBuild\Current\Bin\MSBuild.exe'
        if (Test-Path $candidate) { $msbuildPath = $candidate }
    }
}

if (-not $msbuildPath) {
    # fallback to msbuild in PATH
    $msbuildPath = (Get-Command msbuild.exe -ErrorAction SilentlyContinue)?.Source
}

if (-not $msbuildPath) {
    Write-Error "MSBuild not found. Open Visual Studio Developer Command Prompt or install MSBuild component."
    exit 1
}

Write-Host "Using MSBuild: $msbuildPath"
& $msbuildPath $Project /t:Rebuild /p:Configuration=$Configuration /p:Platform=x64 /v:m
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE } else { Write-Host "Build succeeded." }
