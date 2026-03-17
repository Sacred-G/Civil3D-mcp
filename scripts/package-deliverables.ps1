param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$copilotOutputNet8 = Join-Path $repoRoot "Civil3D-AI-CoPilot-master\bin\$Configuration\net8.0-windows"
$copilotOutputNet48 = Join-Path $repoRoot "Civil3D-AI-CoPilot-master\bin\$Configuration\net48"

# Prefer explicit build outputs for the requested configuration, but fall back to any built output
if (Test-Path $copilotOutputNet8) {
    $copilotOutput = $copilotOutputNet8
} elseif (Test-Path $copilotOutputNet48) {
    $copilotOutput = $copilotOutputNet48
} else {
    # Look for any Cad AI Agent.dll under the copilot bin folders (Release/Debug and multiple TFMs)
    $binRoot = Join-Path $repoRoot "Civil3D-AI-CoPilot-master\bin"
    $found = Get-ChildItem -Path $binRoot -Recurse -Filter 'Cad AI Agent.dll' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $copilotOutput = $found.DirectoryName
    } else {
        # fallback to the expected net8 path for error messaging
        $copilotOutput = $copilotOutputNet8
    }
}
$mcpBuildOutput = Join-Path $repoRoot "build"
$distRoot = Join-Path $repoRoot "dist"
$packageRoot = Join-Path $distRoot "package"
$copilotPackage = Join-Path $packageRoot "copilot-plugin"
$mcpPackage = Join-Path $packageRoot "civil3d-mcp-server"
$zipPath = Join-Path $distRoot "civil3d-package.zip"

if (-not (Test-Path $copilotOutput)) {
    throw "Copilot output folder not found: $copilotOutput"
}

if (-not (Test-Path (Join-Path $copilotOutput "Cad AI Agent.dll"))) {
    throw "Copilot DLL not found. Build the Copilot project first."
}

if (-not (Test-Path $mcpBuildOutput)) {
    throw "MCP server build output not found: $mcpBuildOutput"
}

if (-not (Test-Path (Join-Path $mcpBuildOutput "index.js"))) {
    throw "MCP server build is missing build\index.js. Run npm run build first."
}

if (Test-Path $packageRoot) {
    Remove-Item $packageRoot -Recurse -Force
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

New-Item -ItemType Directory -Path $copilotPackage -Force | Out-Null
New-Item -ItemType Directory -Path $mcpPackage -Force | Out-Null

Copy-Item (Join-Path $copilotOutput "Cad AI Agent.dll") $copilotPackage

$copilotDeps = Join-Path $copilotOutput "Cad AI Agent.deps.json"
if (Test-Path $copilotDeps) {
    Copy-Item $copilotDeps $copilotPackage
}

$copilotPdb = Join-Path $copilotOutput "Cad AI Agent.pdb"
if (Test-Path $copilotPdb) {
    Copy-Item $copilotPdb $copilotPackage
}

Copy-Item $mcpBuildOutput $mcpPackage -Recurse
Copy-Item (Join-Path $repoRoot "package.json") $mcpPackage

$packageLock = Join-Path $repoRoot "package-lock.json"
if (Test-Path $packageLock) {
    Copy-Item $packageLock $mcpPackage
}

$startScriptPath = Join-Path $mcpPackage "run-mcp-server.cmd"
Set-Content -Path $startScriptPath -Value "@echo off`r`nsetlocal`r`nif not exist node_modules (`r`n  echo Installing runtime dependencies...`r`n  call npm install --omit=dev`r`n  if errorlevel 1 exit /b 1`r`n)`r`nnode build\index.js`r`n"

Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -Force

Write-Host "Package created successfully:" -ForegroundColor Green
Write-Host "  Folder: $packageRoot"
Write-Host "  Zip:    $zipPath"
Write-Host ""
Write-Host "Copilot package contents:" -ForegroundColor Cyan
Get-ChildItem $copilotPackage | ForEach-Object { Write-Host "  $($_.Name)" }
Write-Host ""
Write-Host "MCP server package contents:" -ForegroundColor Cyan
Get-ChildItem $mcpPackage | ForEach-Object { Write-Host "  $($_.Name)" }
