param(
  [ValidateSet("project", "user", "local")]
  [string]$Scope = "project",
  [string]$ServerName = "civil3d-mcp",
  [switch]$PrintOnly
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$launcherPath = Join-Path $PSScriptRoot "claude-bootstrap-and-run.ps1"
$nodeModulesPath = Join-Path $repoRoot "node_modules"
$buildEntryPath = Join-Path $repoRoot "build/index.js"
$npmCommand = "npm.cmd"
$claudeCommand = "claude"
$powershellExe = "powershell"

if (-not (Test-Path $launcherPath)) {
  throw "Launcher script not found at '$launcherPath'."
}

$quotedLauncherPath = '"' + $launcherPath + '"'
$commandText = "claude mcp add --scope $Scope --transport stdio $ServerName -- $powershellExe -NoProfile -ExecutionPolicy Bypass -File $quotedLauncherPath"

if ($PrintOnly) {
  Write-Output $commandText
  exit 0
}

if (-not (Get-Command $claudeCommand -ErrorAction SilentlyContinue)) {
  throw "Claude Code CLI ('claude') was not found on PATH."
}

if (-not (Get-Command $npmCommand -ErrorAction SilentlyContinue)) {
  throw "npm was not found on PATH."
}

if (-not (Test-Path $nodeModulesPath)) {
  & $npmCommand install --no-fund --no-audit --silent --prefix $repoRoot
  if ($LASTEXITCODE -ne 0) {
    throw "npm install failed."
  }
}

if (-not (Test-Path $buildEntryPath)) {
  & $npmCommand run build --prefix $repoRoot
  if ($LASTEXITCODE -ne 0) {
    throw "npm run build failed."
  }
}

& $claudeCommand mcp add --scope $Scope --transport stdio $ServerName -- $powershellExe -NoProfile -ExecutionPolicy Bypass -File $launcherPath
if ($LASTEXITCODE -ne 0) {
  throw "claude mcp add failed."
}

Write-Output "Claude MCP server '$ServerName' added with scope '$Scope'."
