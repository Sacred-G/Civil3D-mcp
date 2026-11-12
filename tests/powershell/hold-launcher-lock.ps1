# Test helper for tests\powershell_scripts.test.ts: holds the install/build lock of
# scripts\claude-bootstrap-and-run.ps1 for the clone that holds -Launcher, the way a running launcher does.
# The lock name comes from the launcher's own $repoKey and $lockName assignments (through the AST), so the
# test also fails if the two drift apart. Writes -ReadyFile once it holds the lock, waits for -ReleaseFile,
# then exits without releasing it, as a launcher killed mid-build would (the lock is then abandoned).
param(
  [Parameter(Mandatory)] [string] $Launcher,
  [Parameter(Mandatory)] [string] $ReadyFile,
  [Parameter(Mandatory)] [string] $ReleaseFile
)
$ErrorActionPreference = 'Stop'
# The launcher's $repoRoot is the parent of its scripts\ folder.
$repoRoot = Split-Path -Parent (Split-Path -Parent $Launcher)
$parseErrors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($Launcher, [ref]$null, [ref]$parseErrors)
if ($parseErrors.Count -gt 0) { throw "The launcher does not parse: $($parseErrors[0].Message)" }
foreach ($s in $ast.EndBlock.Statements) {
  if ($s -is [Management.Automation.Language.AssignmentStatementAst] -and $s.Left.Extent.Text -in '$repoKey', '$lockName') {
    . ([scriptblock]::Create($s.Extent.Text))
  }
}
if (-not $lockName) { throw "No `$lockName assignment in $Launcher" }

$lock = New-Object Threading.Mutex($false, $lockName)
if (-not $lock.WaitOne([TimeSpan]::FromSeconds(30))) { throw "Could not take $lockName" }
[IO.File]::WriteAllText($ReadyFile, $lockName)
$deadline = (Get-Date).AddSeconds(90)
while (-not (Test-Path -LiteralPath $ReleaseFile) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 50 }
exit 0
