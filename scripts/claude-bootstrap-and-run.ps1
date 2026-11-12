$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$nodeModulesPath = Join-Path $repoRoot "node_modules"
$buildEntryPath = Join-Path $repoRoot "build/index.js"
$npmCommand = "npm.cmd"
$nodeCommand = "node"

# One key per clone (its full path, case-insensitive) names the install/build lock and the npm log, so
# launches of one clone take turns and launches of different clones never share a log.
$repoKey = -join ([Security.Cryptography.SHA256]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($repoRoot.ToUpperInvariant())) |
  Select-Object -First 8 | ForEach-Object { $_.ToString("x2") })
$lockName = "Local\civil3d-mcp-bootstrap-$repoKey"
# stdout carries the MCP protocol, so only node may write to it: npm output goes to this log (started
# afresh by each launch that runs npm), and on failure its last lines go to stderr.
$logPath = Join-Path $env:TEMP "civil3d-mcp-bootstrap-$repoKey.log"
$logStarted = $false
# Stamps written only after a successful install or build. The build stamp sits in build\, which the
# build's prebuild step deletes first, so a failed or killed build leaves none and the next launch builds
# again instead of running a half-built server.
$depsStampPath = Join-Path $nodeModulesPath ".civil3d-mcp-deps"
$buildStampPath = Join-Path $repoRoot "build/.civil3d-mcp-inputs"

if (-not (Get-Command $nodeCommand -ErrorAction SilentlyContinue)) {
  throw "Node.js was not found on PATH."
}

if (-not (Get-Command $npmCommand -ErrorAction SilentlyContinue)) {
  throw "npm was not found on PATH."
}

function Invoke-NpmQuiet {
  # Function scope only: under Windows PowerShell 5.1 a native stderr line (npm's update notice, a
  # warning) would otherwise end the script as a NativeCommandError.
  $ErrorActionPreference = "Continue"
  "== $(Get-Date -Format s) npm $args" | Out-File -LiteralPath $logPath -Append:$script:logStarted -Encoding utf8
  $script:logStarted = $true
  & $npmCommand @args 2>&1 | ForEach-Object { "$_" } | Out-File -LiteralPath $logPath -Append -Encoding utf8
  $LASTEXITCODE
}

function Stop-Bootstrap([string] $What) {
  [Console]::Error.WriteLine("$What. Log: $logPath")
  Get-Content -LiteralPath $logPath -Tail 30 -ErrorAction SilentlyContinue | ForEach-Object { [Console]::Error.WriteLine($_) }
  exit 1
}

function Read-Stamp([string] $Path) {
  if (Test-Path -LiteralPath $Path) { [IO.File]::ReadAllText($Path) } else { "" }
}

function Write-Stamp([string] $Path, [string] $Value) {
  # A missing stamp only costs one more install or build next time: never fail the launch over it.
  try { [IO.File]::WriteAllText($Path, $Value) }
  catch { [Console]::Error.WriteLine("Could not write ${Path}: $($_.Exception.Message)") }
}

# Content hash of the dependency manifests. .NET SHA-256 rather than Get-FileHash: Windows PowerShell 5.1
# started from PowerShell 7 inherits 7's PSModulePath and then cannot load Get-FileHash.
function Get-DependencyHash {
  (@("package.json", "package-lock.json") | ForEach-Object {
    $file = Join-Path $repoRoot $_
    if (Test-Path -LiteralPath $file) {
      [BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash([IO.File]::ReadAllBytes($file))).Replace("-", "")
    } else { "none" }
  }) -join "|"
}

# Every build input with its size and time, so edits, additions, deletions and renames all count.
function Get-BuildInputs {
  $inputs = @(Get-ChildItem (Join-Path $repoRoot "src") -Recurse -File -ErrorAction SilentlyContinue) +
    @("package.json", "package-lock.json", "tsconfig.json", "autodesk_videos.json", "scripts/copy-static-assets.mjs" |
      ForEach-Object { Join-Path $repoRoot $_ } | Where-Object { Test-Path -LiteralPath $_ } | ForEach-Object { Get-Item -LiteralPath $_ })
  ($inputs | ForEach-Object { "{0}|{1}|{2}" -f $_.FullName.Substring($repoRoot.Length), $_.Length, $_.LastWriteTimeUtc.Ticks } | Sort-Object) -join "`n"
}

# Install when there are no dependencies yet or package.json / package-lock.json changed since the last
# install (a pull that adds or bumps one).
function Test-InstallNeeded([string] $Hash) {
  -not (Test-Path -LiteralPath $nodeModulesPath) -or (Read-Stamp $depsStampPath) -cne $Hash
}

# Build when there is no build yet or a build input changed since the last successful build (after a git
# pull, new tools appear without a manual npm run build).
function Test-BuildNeeded([string] $Snapshot) {
  -not (Test-Path -LiteralPath $buildEntryPath) -or (Read-Stamp $buildStampPath) -cne $Snapshot
}

# With fresh stamps the launch goes straight to node. Otherwise launches of this clone take turns (several
# sessions starting their servers at once after a pull): the lock holder installs and builds, and the others
# check the stamps again once they hold the lock, so they find the work done instead of building again.
if ((Test-InstallNeeded (Get-DependencyHash)) -or (Test-BuildNeeded (Get-BuildInputs))) {
  $lock = New-Object Threading.Mutex($false, $lockName)
  $locked = $false
  try {
    # A launcher killed while holding the lock leaves it abandoned; the stamps say what it finished.
    try { $locked = $lock.WaitOne([TimeSpan]::FromMinutes(10)) } catch [Threading.AbandonedMutexException] { $locked = $true }
    if (-not $locked) { Stop-Bootstrap "Timed out after 10 minutes waiting for another civil3d-mcp launcher to install or build" }

    # --no-save: npm would otherwise rewrite the tracked package-lock.json (a dirty clone that blocks the
    # next pull). The stamp is the hash of the committed files, taken before the install.
    $depHash = Get-DependencyHash
    if (Test-InstallNeeded $depHash) {
      if ((Invoke-NpmQuiet install --no-save --no-fund --no-audit --prefix $repoRoot) -ne 0) {
        Stop-Bootstrap "Failed to install npm dependencies for civil3d-mcp"
      }
      Write-Stamp $depsStampPath $depHash
    }

    $buildInputs = Get-BuildInputs
    if (Test-BuildNeeded $buildInputs) {
      if ((Invoke-NpmQuiet run build --prefix $repoRoot) -ne 0) {
        Stop-Bootstrap "Failed to build civil3d-mcp"
      }
      Write-Stamp $buildStampPath $buildInputs
    }
  } finally {
    # Released before node starts: the server runs for the whole session.
    if ($locked) { $lock.ReleaseMutex() }
    $lock.Dispose()
  }
}

& $nodeCommand $buildEntryPath
exit $LASTEXITCODE
