# Offline checks for scripts\install-trusted.ps1, run by tests\powershell_scripts.test.ts (or by hand:
# powershell -NoProfile -ExecutionPolicy Bypass -File tests\powershell\install-trusted.check.ps1).
# Everything is written under a temp folder with a temp APPDATA. Civil 3D and the real profile are never
# changed; the full-script cases only read this profile's support path from HKCU (Find-OtherAcaddoc).
# Prints PASS/FAIL lines and exits with the number of failures.
$ErrorActionPreference = 'Stop'
$installer = (Resolve-Path (Join-Path $PSScriptRoot '..\..\scripts\install-trusted.ps1')).Path
$work = Join-Path ([IO.Path]::GetTempPath()) ('c3dmcp-install-check-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null
$failures = 0

function Check([string] $name, [bool] $ok, [string] $detail = '') {
  if ($ok) { "PASS $name" } else { "FAIL $name $detail"; $script:failures++ }
}
function Hex([byte[]] $bytes) { ($bytes | ForEach-Object { '{0:X2}' -f $_ }) -join ' ' }
function New-CaseFile([string] $name) {
  $dir = Join-Path $work "unit\$name"
  New-Item -ItemType Directory -Force $dir | Out-Null
  Join-Path $dir 'acaddoc.lsp'
}
function Count([string] $text, [string] $part) { ([regex]::Matches($text, [regex]::Escape($part))).Count }

$acp = [int](Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Nls\CodePage').ACP
$cafe = "; caf$([char]0xE9)`r`n"
$oldTime = [datetime]::new(2020, 1, 2, 3, 4, 5, [DateTimeKind]::Utc)
$savedAppData = $env:APPDATA

try {
  # --- Unit cases: the installer's own functions, loaded through the AST, with Find-OtherAcaddoc stubbed.
  $parseErrors = $null
  $ast = [Management.Automation.Language.Parser]::ParseFile($installer, [ref]$null, [ref]$parseErrors)
  Check 'installer parses' ($parseErrors.Count -eq 0) (($parseErrors | ForEach-Object { $_.Message }) -join '; ')
  $top = $ast.EndBlock.Statements
  foreach ($s in $top) {
    if ($s -is [Management.Automation.Language.AssignmentStatementAst] -and $s.Left.Extent.Text -in '$beginMarker', '$endMarker') {
      . ([scriptblock]::Create($s.Extent.Text))
    }
    if ($s -is [Management.Automation.Language.FunctionDefinitionAst] -and $s.Name -in 'Read-TextFile', 'Set-AcaddocBlock') {
      . ([scriptblock]::Create($s.Extent.Text))
    }
  }
  $fakeOther = $null
  function Find-OtherAcaddoc([string] $exclude) { $script:fakeOther }
  $root = 'C:/Fake/plugins/Civil3DMcp'
  foreach ($s in $top) {
    if ($s -is [Management.Automation.Language.AssignmentStatementAst] -and $s.Left.Extent.Text -eq '$block') {
      . ([scriptblock]::Create($s.Extent.Text))
    }
  }
  Check 'block loaded from the installer' ($block -and $block.StartsWith($beginMarker) -and $block.Contains('(cond'))

  # ANSI (BOM-less, not valid UTF-8): read and written back in the system code page, in both editions.
  $ansiBytes = [Text.Encoding]::GetEncoding($acp).GetBytes($cafe)
  $validUtf8 = $true
  try { (New-Object Text.UTF8Encoding($false, $true)).GetString($ansiBytes) | Out-Null } catch { $validUtf8 = $false }
  if ($validUtf8) {
    "SKIP ANSI cases: '; cafe' with an accent is valid UTF-8 in code page $acp"
  } else {
    $f = New-CaseFile 'ansi'
    [IO.File]::WriteAllBytes($f, $ansiBytes)
    Check 'ANSI fallback is the system code page' ((Read-TextFile $f).Encoding.CodePage -eq $acp) "got $((Read-TextFile $f).Encoding.CodePage), ACP $acp"
    Set-AcaddocBlock $f $block | Out-Null
    $after = [IO.File]::ReadAllBytes($f)
    Check 'ANSI file keeps its bytes' ((Hex $after[0..($ansiBytes.Length - 1)]) -eq (Hex $ansiBytes)) (Hex $after[0..15])
    Check 'ANSI file gets the block' ([Text.Encoding]::GetEncoding($acp).GetString($after).Contains($beginMarker))
    Check 'ANSI file backed up before the first change' ((Test-Path "$f.c3dmcp-backup") -and (Hex ([IO.File]::ReadAllBytes("$f.c3dmcp-backup"))) -eq (Hex $ansiBytes))
  }

  # UTF-8 with BOM keeps its BOM; BOM-less UTF-8 stays BOM-less; UTF-16LE keeps FF FE.
  $utf8 = [Text.Encoding]::UTF8.GetBytes($cafe)
  $f = New-CaseFile 'utf8bom'
  [IO.File]::WriteAllBytes($f, [byte[]](@(0xEF, 0xBB, 0xBF) + $utf8))
  Set-AcaddocBlock $f $block | Out-Null
  $after = [IO.File]::ReadAllBytes($f)
  Check 'UTF-8 BOM file keeps BOM and text' ((Hex $after[0..(2 + $utf8.Length)]) -eq (Hex ([byte[]](@(0xEF, 0xBB, 0xBF) + $utf8)))) (Hex $after[0..15])
  Check 'UTF-8 BOM file gets the block once' ((Count ([Text.Encoding]::UTF8.GetString($after)) $beginMarker) -eq 1)

  $f = New-CaseFile 'utf8'
  [IO.File]::WriteAllBytes($f, $utf8)
  Set-AcaddocBlock $f $block | Out-Null
  $after = [IO.File]::ReadAllBytes($f)
  Check 'BOM-less UTF-8 file stays BOM-less UTF-8' ((Hex $after[0..($utf8.Length - 1)]) -eq (Hex $utf8)) (Hex $after[0..15])

  $utf16 = [Text.Encoding]::Unicode.GetBytes($cafe)
  $f = New-CaseFile 'utf16'
  [IO.File]::WriteAllBytes($f, [byte[]](@(0xFF, 0xFE) + $utf16))
  Set-AcaddocBlock $f $block | Out-Null
  $after = [IO.File]::ReadAllBytes($f)
  Check 'UTF-16LE file keeps BOM and text' ((Hex $after[0..(1 + $utf16.Length)]) -eq (Hex ([byte[]](@(0xFF, 0xFE) + $utf16)))) (Hex $after[0..15])
  Check 'UTF-16LE file gets the block' ([Text.Encoding]::Unicode.GetString($after, 2, $after.Length - 2).Contains($beginMarker))

  # Uninstall on a file that never held our block: not rewritten, not re-encoded, not backed up.
  $f = New-CaseFile 'foreign'
  [IO.File]::WriteAllBytes($f, [byte[]](@(0x3B, 0x20, 0x63, 0x61, 0x66, 0xE9, 0x0D, 0x0A)))
  [IO.File]::SetLastWriteTimeUtc($f, $oldTime)
  Set-AcaddocBlock $f '' | Out-Null
  Check 'uninstall leaves a foreign file byte-identical' ((Hex ([IO.File]::ReadAllBytes($f))) -eq '3B 20 63 61 66 E9 0D 0A')
  Check 'uninstall leaves a foreign file unwritten' ([IO.File]::GetLastWriteTimeUtc($f) -eq $oldTime)
  Check 'uninstall backs up nothing' (-not (Test-Path "$f.c3dmcp-backup"))
  $f = New-CaseFile 'missing'
  Set-AcaddocBlock $f '' | Out-Null
  Check 'uninstall creates no file' (-not (Test-Path $f))

  # A hidden acaddoc.lsp is loaded before the (cond that wraps S::STARTUP, and the line survives a re-run.
  $fakeOther = 'C:\Fake\Support\acaddoc.lsp'
  $f = New-CaseFile 'hidden'
  foreach ($run in 1, 2) {
    Set-AcaddocBlock $f $block | Out-Null
    $text = [IO.File]::ReadAllText($f)
    $load = "(vl-catch-all-apply 'load (list `"C:/Fake/Support/acaddoc.lsp`"))"
    Check "run ${run}: hidden acaddoc.lsp loaded exactly once" ((Count $text $load) -eq 1)
    Check "run ${run}: hidden acaddoc.lsp loaded before (cond" ($text.IndexOf($load) -ge 0 -and $text.IndexOf($load) -lt $text.IndexOf('(cond'))
    Check "run ${run}: one block" ((Count $text $beginMarker) -eq 1 -and (Count $text $endMarker) -eq 1)
  }
  Set-AcaddocBlock $f '' | Out-Null
  Check 'uninstall deletes a file that held only our block' (-not (Test-Path $f))

  # A user's existing file never shadowed anything new: no load line, on the first run or a re-run.
  $f = New-CaseFile 'userfile'
  [IO.File]::WriteAllText($f, "(princ `"user`")`r`n")
  foreach ($run in 1, 2) {
    Set-AcaddocBlock $f $block | Out-Null
    $text = [IO.File]::ReadAllText($f)
    Check "run ${run}: user file gets no load line" (-not $text.Contains("'load (list"))
    Check "run ${run}: user file keeps its code and one block" ($text.StartsWith('(princ "user")') -and (Count $text $beginMarker) -eq 1)
  }

  # The user adds code to the file the install created: re-runs keep the load line, with the hidden file in
  # reach or not, and -Uninstall keeps the user's code without it and warns that the file is hidden again.
  $load = "(vl-catch-all-apply 'load (list `"C:/Fake/Support/acaddoc.lsp`"))"
  $fakeOther = 'C:\Fake\Support\acaddoc.lsp'
  $f = New-CaseFile 'userappend'
  Set-AcaddocBlock $f $block | Out-Null
  [IO.File]::AppendAllText($f, "(load `"wsp-core.lsp`")`r`n")
  foreach ($case in @{ Name = 'in reach'; Other = 'C:\Fake\Support\acaddoc.lsp' }, @{ Name = 'out of reach'; Other = $null }) {
    $fakeOther = $case.Other
    Set-AcaddocBlock $f $block | Out-Null
    $text = [IO.File]::ReadAllText($f)
    Check "user code added, hidden file $($case.Name): load line kept once" ((Count $text $load) -eq 1)
    Check "user code added, hidden file $($case.Name): load line before (cond" ($text.IndexOf($load) -ge 0 -and $text.IndexOf($load) -lt $text.IndexOf('(cond'))
    Check "user code added, hidden file $($case.Name): user code and one block" ($text.Contains('(load "wsp-core.lsp")') -and (Count $text $beginMarker) -eq 1)
  }
  $out = @(Set-AcaddocBlock $f '' 3>&1)
  $text = [IO.File]::ReadAllText($f)
  Check 'uninstall keeps the user code, without our block or the load line' ($text.Contains('(load "wsp-core.lsp")') -and -not $text.Contains($beginMarker) -and -not $text.Contains("'load (list"))
  $warnings = @($out | Where-Object { $_ -is [Management.Automation.WarningRecord] })
  Check 'uninstall warns that the hidden file is hidden again' ($warnings.Count -eq 1 -and $warnings[0].Message.Contains('C:/Fake/Support/acaddoc.lsp')) (($out | ForEach-Object { "$_" }) -join ' | ')

  # The hidden file out of reach on a re-run (a share off VPN): the load line stays; with the file back
  # (any case) there is still one; a newly found file is added next to it; -Uninstall of a file that is
  # only ours deletes it without a warning (nothing is hidden any more).
  $f = New-CaseFile 'offline'
  $fakeOther = 'C:\Fake\Support\acaddoc.lsp'
  Set-AcaddocBlock $f $block | Out-Null
  $fakeOther = $null
  Set-AcaddocBlock $f $block | Out-Null
  Check 'hidden file out of reach: load line kept' ((Count ([IO.File]::ReadAllText($f)) $load) -eq 1)
  $fakeOther = 'c:\fake\support\ACADDOC.lsp'
  Set-AcaddocBlock $f $block | Out-Null
  $text = [IO.File]::ReadAllText($f)
  Check 'hidden file back: still one load line' ((Count $text "'load (list") -eq 1 -and (Count $text $load) -eq 1)
  $fakeOther = 'D:\Other\Support\acaddoc.lsp'
  Set-AcaddocBlock $f $block | Out-Null
  $text = [IO.File]::ReadAllText($f)
  $load2 = "(vl-catch-all-apply 'load (list `"D:/Other/Support/acaddoc.lsp`"))"
  Check 'another hidden file found: both loaded once, before (cond' ((Count $text $load) -eq 1 -and (Count $text $load2) -eq 1 -and $text.IndexOf($load2) -lt $text.IndexOf('(cond'))
  Check 'another hidden file found: one block' ((Count $text $beginMarker) -eq 1)
  $out = @(Set-AcaddocBlock $f '' 3>&1)
  Check 'uninstall deletes a file that held only our block, without a warning' ((-not (Test-Path $f)) -and -not ($out | Where-Object { $_ -is [Management.Automation.WarningRecord] }))
  $fakeOther = $null

  # --- Full-script cases with a temp APPDATA: every lock is checked before the first change.
  $appData = Join-Path $work 'appdata'
  $support = Join-Path $appData 'Autodesk\C3D 2026\enu\Support'
  $bundle = Join-Path $appData 'Autodesk\ApplicationPlugins\Civil3DMcp.bundle'
  $bundleDll = Join-Path $bundle 'Contents\Civil3DMcpPlugin.dll'
  $src = Join-Path $work 'src\net8.0-windows'
  $dest = Join-Path $work 'dest'
  $target = Join-Path $dest 'R25.1'
  $acaddoc = Join-Path $support 'acaddoc.lsp'
  foreach ($d in $support, (Split-Path $bundleDll), $src) { New-Item -ItemType Directory -Force $d | Out-Null }
  [IO.File]::WriteAllBytes($bundleDll, [byte[]](1, 2, 3))
  [IO.File]::WriteAllBytes((Join-Path $src 'Civil3DMcpPlugin.dll'), [byte[]](4, 5, 6))
  $env:APPDATA = $appData

  # A loaded DLL can be read (hashed) but not opened for writing: hold it the same way.
  $threw = $null
  $handle = [IO.File]::Open($bundleDll, 'Open', 'Read', 'Read')
  try { & $installer -SourceDir $src -Destination $dest | Out-Null } catch { $threw = $_.Exception.Message } finally { $handle.Dispose() }
  Check 'locked bundle: install refused' ($threw -and $threw.Contains('Civil3DMcp.bundle')) "threw: $threw"
  Check 'locked bundle: acaddoc.lsp not written' (-not (Test-Path $acaddoc))
  Check 'locked bundle: nothing copied' (-not (Test-Path $target))
  Check 'locked bundle: bundle kept' (Test-Path $bundleDll)

  $out = & $installer -SourceDir $src -Destination $dest
  Check 'unlocked: acaddoc.lsp holds the block' ((Test-Path $acaddoc) -and [IO.File]::ReadAllText($acaddoc).Contains($beginMarker)) ($out -join ' | ')
  Check 'unlocked: bundle removed' (-not (Test-Path $bundle))
  Check 'unlocked: DLL installed' ((Test-Path "$target\Civil3DMcpPlugin.dll") -and (Get-FileHash "$target\Civil3DMcpPlugin.dll").Hash -eq (Get-FileHash "$src\Civil3DMcpPlugin.dll").Hash)

  # A changed build while the installed DLL is loaded: refused before acaddoc.lsp or the bundle is touched.
  Remove-Item $acaddoc
  New-Item -ItemType Directory -Force (Split-Path $bundleDll) | Out-Null
  [IO.File]::WriteAllBytes($bundleDll, [byte[]](1, 2, 3))
  [IO.File]::WriteAllBytes((Join-Path $src 'Civil3DMcpPlugin.dll'), [byte[]](7, 8, 9))
  $threw = $null
  $handle = [IO.File]::Open("$target\Civil3DMcpPlugin.dll", 'Open', 'Read', 'Read')
  try { & $installer -SourceDir $src -Destination $dest | Out-Null } catch { $threw = $_.Exception.Message } finally { $handle.Dispose() }
  Check 'locked target: install refused' ($threw -and $threw.Contains('build changed')) "threw: $threw"
  Check 'locked target: acaddoc.lsp not written' (-not (Test-Path $acaddoc))
  Check 'locked target: bundle kept' (Test-Path $bundleDll)
  Check 'locked target: old DLL kept' ((Hex ([IO.File]::ReadAllBytes("$target\Civil3DMcpPlugin.dll"))) -eq '04 05 06')

  # Uninstall (a child process: the script ends with exit) after a second language folder appeared with
  # someone else's acaddoc.lsp: ours is removed, theirs is left byte-for-byte and unwritten.
  & $installer -SourceDir $src -Destination $dest | Out-Null
  $esp = Join-Path $appData 'Autodesk\C3D 2026\esp\Support'
  New-Item -ItemType Directory -Force $esp | Out-Null
  $foreign = Join-Path $esp 'acaddoc.lsp'
  [IO.File]::WriteAllBytes($foreign, [byte[]](@(0x3B, 0x20, 0x63, 0x61, 0x66, 0xE9, 0x0D, 0x0A)))
  [IO.File]::SetLastWriteTimeUtc($foreign, $oldTime)
  $exe = (Get-Process -Id $PID).Path
  $out = & $exe -NoProfile -ExecutionPolicy Bypass -File $installer -Uninstall -SourceDir $src -Destination $dest
  Check 'uninstall exits 0' ($LASTEXITCODE -eq 0) ($out -join ' | ')
  Check 'uninstall removes our file' (-not (Test-Path $acaddoc))
  Check 'uninstall removes the plugin folder' (-not (Test-Path $target))
  Check 'uninstall leaves the foreign file' ((Hex ([IO.File]::ReadAllBytes($foreign))) -eq '3B 20 63 61 66 E9 0D 0A' -and [IO.File]::GetLastWriteTimeUtc($foreign) -eq $oldTime)
  Check 'uninstall backs up no foreign file' (-not (Test-Path "$foreign.c3dmcp-backup"))
} catch {
  "FAIL unexpected error: $($_.Exception.Message) at $($_.InvocationInfo.PositionMessage)"
  $failures++
} finally {
  $env:APPDATA = $savedAppData
  Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
}
"$failures failure(s)"
exit $failures
