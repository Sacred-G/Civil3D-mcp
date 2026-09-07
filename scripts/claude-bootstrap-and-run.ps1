$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$nodeModulesPath = Join-Path $repoRoot "node_modules"
$buildEntryPath = Join-Path $repoRoot "build/index.js"
$npmCommand = "npm.cmd"
$nodeCommand = "node"

$fallbackNodeDirs = @(
  "$env:ProgramFiles\nodejs",
  "${env:ProgramFiles(x86)}\nodejs",
  "$env:LOCALAPPDATA\Programs\nodejs"
)

if (-not (Get-Command $nodeCommand -ErrorAction SilentlyContinue)) {
  $fallbackDir = $fallbackNodeDirs | Where-Object { Test-Path (Join-Path $_ "node.exe") } | Select-Object -First 1
  if ($fallbackDir) {
    $nodeCommand = Join-Path $fallbackDir "node.exe"
  } else {
    throw "Node.js was not found on PATH."
  }
}

if (-not (Get-Command $npmCommand -ErrorAction SilentlyContinue)) {
  $fallbackDir = $fallbackNodeDirs | Where-Object { Test-Path (Join-Path $_ "npm.cmd") } | Select-Object -First 1
  if ($fallbackDir) {
    $npmCommand = Join-Path $fallbackDir "npm.cmd"
  } else {
    throw "npm was not found on PATH."
  }
}

if (-not (Test-Path $nodeModulesPath)) {
  & $npmCommand install --no-fund --no-audit --silent --prefix $repoRoot *> $null
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to install npm dependencies for civil3d-mcp."
  }
}

if (-not (Test-Path $buildEntryPath)) {
  & $npmCommand run build --prefix $repoRoot *> $null
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to build civil3d-mcp."
  }
}

& $nodeCommand $buildEntryPath
exit $LASTEXITCODE
