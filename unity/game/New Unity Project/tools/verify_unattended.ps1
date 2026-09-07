# =====================================================================
# GAME-ZZZ Unattended Verification Gate (S3-M4-UNATTENDED-VERIFY-GATE)
# Repository-local canonical verification entry. One command runs:
#   Unity resolution -> EditMode suite -> PlayMode suite ->
#   Content Audit freshness + verdict -> XML parsing -> summary -> exit code
#
# Canonical usage (from Unity project root):
#   .\tools\verify_unattended.ps1                            # Quick Gate: EditMode + PlayMode + Audit
#   .\tools\verify_unattended.ps1 -IncludeBuild              # Full Gate: Quick + StandaloneWindows64 Player Build
#   .\tools\verify_unattended.ps1 -UnityPath "G:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
#   .\tools\verify_unattended.ps1 -TimeoutMinutes 20 -BuildTimeoutMinutes 30
#   .\tools\verify_unattended.ps1 -SelfTest                  # no Unity launch, synthetic fixtures
#
# Exit codes: 0 = Gate PASS | 1 = Gate FAIL (tests/audit/player build/timeout) |
#             2 = Unity not resolved | 3 = project already open (editor lock)
# The gate verifies only; it never modifies STATUS/ROADMAP/catalogs/tests,
# never runs git commands, and never kills Unity processes it did not start.
# Allowed repo side effects: the formal EditMode run re-persists
# docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md (existing audit contract).
# Player build output is ephemeral (temp only), never committed to the repo.
# =====================================================================
[CmdletBinding()]
param(
    [string]$UnityPath = "",
    [int]$TimeoutMinutes = 20,
    [int]$BuildTimeoutMinutes = 30,
    [switch]$IncludeBuild,
    [switch]$SelfTest
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$AuditPath = Join-Path $ProjectRoot 'docs\reviews\s3\CONTENT_AUDIT_S3_CLOSEOUT.md'
$ProjectVersionFile = Join-Path $ProjectRoot 'ProjectSettings\ProjectVersion.txt'
$LockFile = Join-Path $ProjectRoot 'Temp\UnityLockfile'
$TempRoot = Join-Path ([System.IO.Path]::GetTempPath()) 'GAME-ZZZ-UnattendedGate'
$Issues = New-Object System.Collections.Generic.List[string]

# ---------------------------------------------------------------------
# Parsers (kept pure so -SelfTest can exercise them without Unity)
# ---------------------------------------------------------------------

function Read-TestXml {
    param([string]$XmlPath)
    if ([string]::IsNullOrEmpty($XmlPath) -or -not (Test-Path -LiteralPath $XmlPath -PathType Leaf)) { return $null }
    try {
        $doc = New-Object System.Xml.XmlDocument
        $doc.Load($XmlPath)
    } catch { return $null }
    $root = $doc.DocumentElement
    if ($null -eq $root -or $root.Name -ne 'test-run') { return $null }
    $raw = @{
        total   = $root.GetAttribute('total')
        passed  = $root.GetAttribute('passed')
        failed  = $root.GetAttribute('failed')
        skipped = $root.GetAttribute('skipped')
        result  = $root.GetAttribute('result')
    }
    if ([string]::IsNullOrEmpty($raw.result)) { return $null }
    $n = @{}
    foreach ($k in @('total', 'passed', 'failed', 'skipped')) {
        $v = 0
        if (-not [int]::TryParse($raw[$k], [ref]$v)) { return $null }
        $n[$k] = $v
    }
    return @{
        Total = $n['total']; Passed = $n['passed']; Failed = $n['failed']; Skipped = $n['skipped']
        Result = $raw.result
    }
}

function Read-AuditVerdict {
    param([string]$Text)
    $out = @{ Exists = $false; Completed = ''; Verdict = ''; FailureCount = -1 }
    if ([string]::IsNullOrEmpty($Text)) { return $out }
    $out.Exists = $true
    $completed = [regex]::Match($Text, '- Audit completed:\s*(\w+)')
    $verdict = [regex]::Match($Text, '- Verdict:\s*(\w+)')
    $count = [regex]::Match($Text, '- Failure count:\s*(\d+)')
    if ($completed.Success) { $out.Completed = $completed.Groups[1].Value.ToUpperInvariant() }
    if ($verdict.Success) { $out.Verdict = $verdict.Groups[1].Value.ToUpperInvariant() }
    if ($count.Success) { $out.FailureCount = [int]$count.Groups[1].Value }
    return $out
}

function Test-AuditFresh {
    param([DateTime]$BeforeUtc, [DateTime]$AfterUtc, [DateTime]$GateStartUtc)
    return $AfterUtc -ge $GateStartUtc
}

function Get-RequiredUnityVersion {
    if (-not (Test-Path -LiteralPath $ProjectVersionFile -PathType Leaf)) { return $null }
    foreach ($line in (Get-Content -LiteralPath $ProjectVersionFile)) {
        $m = [regex]::Match($line, 'm_EditorVersion:\s*(\S+)')
        if ($m.Success) { return $m.Groups[1].Value }
    }
    return $null
}

function Resolve-UnityEditorPath {
    param([string]$Explicit)
    $tried = New-Object System.Collections.Generic.List[string]

    if (-not [string]::IsNullOrWhiteSpace($Explicit)) {
        $tried.Add("explicit -UnityPath: $Explicit")
        if (Test-Path -LiteralPath $Explicit -PathType Leaf) {
            return @{ Path = (Resolve-Path -LiteralPath $Explicit).Path; Source = 'explicit -UnityPath'; Tried = $tried }
        }
        return $null
    }

    $envPath = $env:UNITY_EDITOR
    if (-not [string]::IsNullOrWhiteSpace($envPath)) {
        $tried.Add("env UNITY_EDITOR: $envPath")
        if (Test-Path -LiteralPath $envPath -PathType Leaf) {
            return @{ Path = (Resolve-Path -LiteralPath $envPath).Path; Source = 'env UNITY_EDITOR'; Tried = $tried }
        }
    }

    $version = Get-RequiredUnityVersion
    if ([string]::IsNullOrEmpty($version)) {
        $tried.Add("ProjectVersion.txt unreadable: $ProjectVersionFile")
        return $null
    }
    $tried.Add("version from ProjectVersion.txt: $version")

    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($base in @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:LOCALAPPDATA)) {
        if (-not [string]::IsNullOrEmpty($base)) { $roots.Add((Join-Path $base 'Unity\Hub\Editor')) }
    }
    # Unity Hub secondary install root (documented Hub mechanism for custom install locations)
    $secondary = Join-Path $env:APPDATA 'UnityHub\secondaryInstallPath.json'
    if (Test-Path -LiteralPath $secondary -PathType Leaf) {
        try {
            $custom = (Get-Content -LiteralPath $secondary -Raw).Trim().Trim('"')
            if (-not [string]::IsNullOrWhiteSpace($custom)) {
                $roots.Add($custom)
                $tried.Add("hub secondaryInstallPath.json: $custom")
            }
        } catch { $tried.Add("hub secondaryInstallPath.json unreadable") }
    }

    foreach ($rootDir in $roots) {
        $candidate = Join-Path $rootDir ("$version\Editor\Unity.exe")
        $tried.Add("hub root search: $candidate")
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return @{ Path = (Resolve-Path -LiteralPath $candidate).Path; Source = "hub install root: $rootDir"; Tried = $tried }
        }
    }
    return $null
}

function Invoke-UnityChild {
    param([string]$UnityExe, [string]$ArgumentString, [int]$TimeoutMin)
    $p = Start-Process -FilePath $UnityExe -ArgumentList $ArgumentString -PassThru
    $exited = $p.WaitForExit($TimeoutMin * 60 * 1000)
    if (-not $exited) {
        try { $p.Kill() } catch { }
        try { $p.WaitForExit() | Out-Null } catch { }
        return @{ ExitCode = $null; TimedOut = $true; ArgumentString = $ArgumentString }
    }
    return @{ ExitCode = $p.ExitCode; TimedOut = $false; ArgumentString = $ArgumentString }
}

function Invoke-TestSuite {
    param([string]$UnityExe, [string]$Platform, [string]$ResultsXml, [string]$Log, [int]$TimeoutMin)
    $argString = "-batchmode -projectPath `"$ProjectRoot`" -runTests -testPlatform $Platform -testResults `"$ResultsXml`" -logFile `"$Log`""
    return Invoke-UnityChild -UnityExe $UnityExe -ArgumentString $argString -TimeoutMin $TimeoutMin
}

function Test-PlayerArtifact {
    param([string]$ExecutablePath)
    $out = @{ ExecutableExists = $false; ExecutableBytes = 0; DataDirectoryExists = $false; DataFileCount = 0; DataDirectoryPath = $null }
    if ([string]::IsNullOrEmpty($ExecutablePath) -or -not (Test-Path -LiteralPath $ExecutablePath -PathType Leaf)) { return $out }
    $out.ExecutableExists = $true
    $out.ExecutableBytes = (Get-Item -LiteralPath $ExecutablePath).Length
    $dataDir = Join-Path (Split-Path -Parent $ExecutablePath) (([System.IO.Path]::GetFileNameWithoutExtension($ExecutablePath)) + '_Data')
    $out.DataDirectoryPath = $dataDir
    if (Test-Path -LiteralPath $dataDir -PathType Container) {
        $out.DataDirectoryExists = $true
        $out.DataFileCount = @((Get-ChildItem -LiteralPath $dataDir -Recurse -File -ErrorAction SilentlyContinue)).Count
    }
    return $out
}

function Get-SuiteSummary {
    param($Result, $Run, [bool]$Ok, [bool]$Infra)
    $status = 'FAIL'
    if ($Infra) { $status = 'INFRA' }
    elseif ($Ok) { $status = 'PASS' }
    $out = @{ status = $status; total = -1; passed = -1; failed = -1; skipped = -1; exitCode = $null; timedOut = $false }
    if ($null -ne $Run) { $out.exitCode = $Run.ExitCode; $out.timedOut = $Run.TimedOut }
    if ($null -ne $Result) {
        $out.total = $Result.Total; $out.passed = $Result.Passed
        $out.failed = $Result.Failed; $out.skipped = $Result.Skipped
    }
    return $out
}

function Write-SummaryJson {
    param([string]$Path, [hashtable]$Data)
    $Data | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $Path -Encoding UTF8
}

# ---------------------------------------------------------------------
# SelfTest: synthetic fixtures only, no Unity launch
# ---------------------------------------------------------------------
if ($SelfTest) {
    $fx = Join-Path $TempRoot 'selftest'
    if (Test-Path -LiteralPath $fx) { Remove-Item -LiteralPath $fx -Recurse -Force }
    New-Item -ItemType Directory -Path $fx -Force | Out-Null
    $results = New-Object System.Collections.Generic.List[string]

    $passXml = Join-Path $fx 'pass.xml'
    Set-Content -LiteralPath $passXml -Encoding UTF8 -Value '<test-run total="2" passed="2" failed="0" skipped="0" result="Passed" />'
    $r = Read-TestXml $passXml
    $ok = ($null -ne $r -and $r.Result -eq 'Passed' -and $r.Failed -eq 0)
    $results.Add("selftest.pass-xml: $(@('FAIL', 'PASS')[[int]$ok])")

    $failXml = Join-Path $fx 'fail.xml'
    Set-Content -LiteralPath $failXml -Encoding UTF8 -Value '<test-run total="2" passed="1" failed="1" skipped="0" result="Failed" />'
    $r = Read-TestXml $failXml
    $ok = ($null -ne $r -and $r.Failed -eq 1 -and $r.Result -eq 'Failed')
    $results.Add("selftest.failed-xml: $(@('FAIL', 'PASS')[[int]$ok])")

    $r = Read-TestXml (Join-Path $fx 'does_not_exist.xml')
    $results.Add("selftest.missing-xml: $(@('FAIL', 'PASS')[[int]($null -eq $r)])")

    $badXml = Join-Path $fx 'malformed.xml'
    Set-Content -LiteralPath $badXml -Encoding UTF8 -Value '<test-run total="1" passed="0" failed'
    $r = Read-TestXml $badXml
    $results.Add("selftest.malformed-xml: $(@('FAIL', 'PASS')[[int]($null -eq $r)])")

    $wrongRoot = Join-Path $fx 'wrongroot.xml'
    Set-Content -LiteralPath $wrongRoot -Encoding UTF8 -Value '<not-a-test-run total="1" result="Passed" />'
    $r = Read-TestXml $wrongRoot
    $results.Add("selftest.missing-result-node: $(@('FAIL', 'PASS')[[int]($null -eq $r)])")

    $auditPass = Read-AuditVerdict ("- Audit completed: YES`n- Verdict: PASS`n- Failure count: 0")
    $ok = ($auditPass.Completed -eq 'YES' -and $auditPass.Verdict -eq 'PASS' -and $auditPass.FailureCount -eq 0)
    $results.Add("selftest.audit-pass-text: $(@('FAIL', 'PASS')[[int]$ok])")

    $auditFail = Read-AuditVerdict ("- Audit completed: YES`n- Verdict: FAIL`n- Failure count: 2")
    $ok = ($auditFail.Verdict -eq 'FAIL' -and $auditFail.FailureCount -eq 2)
    $results.Add("selftest.audit-fail-text: $(@('FAIL', 'PASS')[[int]$ok])")

    $auditNo = Read-AuditVerdict ("- Audit completed: NO`n- Verdict: FAIL`n- Execution error: NullReferenceException")
    $ok = ($auditNo.Completed -eq 'NO' -and $auditNo.Verdict -eq 'FAIL')
    $results.Add("selftest.audit-incomplete-text: $(@('FAIL', 'PASS')[[int]$ok])")

    $b = [DateTime]::Parse('2026-09-08T10:00:00Z').ToUniversalTime()
    $s = [DateTime]::Parse('2026-09-08T10:05:00Z').ToUniversalTime()
    $a = [DateTime]::Parse('2026-09-08T10:07:00Z').ToUniversalTime()
    $ok = (Test-AuditFresh -BeforeUtc $b -AfterUtc $a -GateStartUtc $s) -and -not (Test-AuditFresh -BeforeUtc $b -AfterUtc $b -GateStartUtc $s)
    $results.Add("selftest.audit-freshness: $(@('FAIL', 'PASS')[[int]$ok])")

    # Build artifact validator (S3-M5)：有效=非空 exe + 非空 _Data 目录
    $art = Join-Path $fx 'artifact'
    $okDir = Join-Path $art 'ok'
    New-Item -ItemType Directory -Path (Join-Path $okDir 'GAME-ZZZ_Data') -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $okDir 'GAME-ZZZ.exe') -Value 'MZ'
    Set-Content -LiteralPath (Join-Path $okDir 'GAME-ZZZ_Data\level0') -Value 'x'
    $v = Test-PlayerArtifact -ExecutablePath (Join-Path $okDir 'GAME-ZZZ.exe')
    $ok = $v.ExecutableExists -and $v.ExecutableBytes -gt 0 -and $v.DataDirectoryExists -and $v.DataFileCount -gt 0
    $results.Add("selftest.artifact-pass: $(@('FAIL', 'PASS')[[int]$ok])")

    $miss = Join-Path $art 'missingexe'
    New-Item -ItemType Directory -Path (Join-Path $miss 'GAME-ZZZ_Data') -Force | Out-Null
    $v = Test-PlayerArtifact -ExecutablePath (Join-Path $miss 'GAME-ZZZ.exe')
    $ok = -not ($v.ExecutableExists -and $v.ExecutableBytes -gt 0 -and $v.DataDirectoryExists -and $v.DataFileCount -gt 0)
    $results.Add("selftest.artifact-missing-exe: $(@('FAIL', 'PASS')[[int]$ok])")

    $zero = Join-Path $art 'zeroexe'
    New-Item -ItemType Directory -Path (Join-Path $zero 'GAME-ZZZ_Data') -Force | Out-Null
    [System.IO.File]::WriteAllBytes((Join-Path $zero 'GAME-ZZZ.exe'), @())
    Set-Content -LiteralPath (Join-Path $zero 'GAME-ZZZ_Data\level0') -Value 'x'
    $v = Test-PlayerArtifact -ExecutablePath (Join-Path $zero 'GAME-ZZZ.exe')
    $ok = -not ($v.ExecutableExists -and $v.ExecutableBytes -gt 0 -and $v.DataDirectoryExists -and $v.DataFileCount -gt 0)
    $results.Add("selftest.artifact-zero-byte-exe: $(@('FAIL', 'PASS')[[int]$ok])")

    $nodata = Join-Path $art 'nodata'
    New-Item -ItemType Directory -Path $nodata -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $nodata 'GAME-ZZZ.exe') -Value 'MZ'
    $v = Test-PlayerArtifact -ExecutablePath (Join-Path $nodata 'GAME-ZZZ.exe')
    $ok = -not ($v.ExecutableExists -and $v.ExecutableBytes -gt 0 -and $v.DataDirectoryExists -and $v.DataFileCount -gt 0)
    $results.Add("selftest.artifact-missing-data: $(@('FAIL', 'PASS')[[int]$ok])")

    $emptydata = Join-Path $art 'emptydata'
    New-Item -ItemType Directory -Path (Join-Path $emptydata 'GAME-ZZZ_Data') -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $emptydata 'GAME-ZZZ.exe') -Value 'MZ'
    $v = Test-PlayerArtifact -ExecutablePath (Join-Path $emptydata 'GAME-ZZZ.exe')
    $ok = -not ($v.ExecutableExists -and $v.ExecutableBytes -gt 0 -and $v.DataDirectoryExists -and $v.DataFileCount -gt 0)
    $results.Add("selftest.artifact-empty-data: $(@('FAIL', 'PASS')[[int]$ok])")

    $allPass = $true
    foreach ($line in $results) {
        Write-Output $line
        if ($line -notmatch ': PASS$') { $allPass = $false }
    }
    Write-Output "=== GAME-ZZZ UNATTENDED GATE SELF-TEST ==="
    Write-Output "SelfTest: $(@('FAIL', 'PASS')[[int]$allPass])"
    if ($allPass) { exit 0 } else { exit 1 }
}

# ---------------------------------------------------------------------
# Canonical gate
# ---------------------------------------------------------------------
if (Test-Path -LiteralPath $TempRoot) { Remove-Item -LiteralPath $TempRoot -Recurse -Force }
New-Item -ItemType Directory -Path $TempRoot -Force | Out-Null
$gateStartUtc = [DateTime]::UtcNow
$editResult = $null
$playResult = $null
$editRun = $null
$playRun = $null
$auditBeforeUtc = $null
if (Test-Path -LiteralPath $AuditPath -PathType Leaf) {
    $auditBeforeUtc = (Get-Item -LiteralPath $AuditPath).LastWriteTimeUtc
}

# 1) Resolve Unity editor (fail fast, no interactive prompt, no download)
$resolved = Resolve-UnityEditorPath -Explicit $UnityPath
if ($null -eq $resolved) {
    $version = Get-RequiredUnityVersion
    Write-Output "=== GAME-ZZZ UNATTENDED GATE ==="
    Write-Output "Gate: FAIL"
    Write-Output "Reason: UNITY_NOT_FOUND"
    Write-Output "Required Unity version: $(@('unknown', $version)[[int](-not [string]::IsNullOrEmpty($version))])"
    Write-Output "Resolution strategies checked:"
    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) {
        Write-Output "  - explicit -UnityPath: file not found"
    } else {
        Write-Output "  - env UNITY_EDITOR: unset or file not found"
        Write-Output "  - Unity Hub install roots (defaults + secondaryInstallPath.json) for exact version"
        Write-Output "  - ProjectVersion.txt: $ProjectVersionFile"
    }
    exit 2
}
$unityExe = $resolved.Path
Write-Output "=== GAME-ZZZ UNATTENDED GATE ==="
Write-Output "Unity: $unityExe (source: $($resolved.Source))"

# 2) Editor lock: fail fast, never kill a running editor
if (Test-Path -LiteralPath $LockFile -PathType Leaf) {
    Write-Output "Gate: FAIL"
    Write-Output "Reason: PROJECT_ALREADY_OPEN"
    Write-Output "Lock file: $LockFile"
    Write-Output "Close the running Unity Editor for this project, then rerun the gate."
    exit 3
}

# 3) EditMode suite
$editXml = Join-Path $TempRoot 'EditModeResults.xml'
$editLog = Join-Path $TempRoot 'EditMode.log'
$editRun = Invoke-TestSuite -UnityExe $unityExe -Platform 'EditMode' -ResultsXml $editXml -Log $editLog -TimeoutMin $TimeoutMinutes
$editResult = Read-TestXml $editXml
$editInfra = $editRun.TimedOut -or $null -eq $editResult -or $editRun.ExitCode -ne 0
if ($editRun.TimedOut) { $Issues.Add('EditMode TIMEOUT (gate killed only its own child process)') }
elseif ($null -eq $editResult) { $Issues.Add('EditMode XML missing/malformed/missing result node') }
elseif ($editRun.ExitCode -ne 0) { $Issues.Add("EditMode exit code $($editRun.ExitCode) with XML $($editResult.Result) (exit/XML mismatch)") }

# 4) PlayMode suite (run when infrastructure still permits; never fabricate evidence)
$playSkipped = $false
if ($editInfra) {
    $playSkipped = $true
    $Issues.Add('PlayMode SKIPPED (Edit suite infrastructure failure; PlayMode result not fabricated)')
} else {
    $playXml = Join-Path $TempRoot 'PlayModeResults.xml'
    $playLog = Join-Path $TempRoot 'PlayMode.log'
    $playRun = Invoke-TestSuite -UnityExe $unityExe -Platform 'PlayMode' -ResultsXml $playXml -Log $playLog -TimeoutMin $TimeoutMinutes
    $playResult = Read-TestXml $playXml
    $playInfra = $playRun.TimedOut -or $null -eq $playResult -or $playRun.ExitCode -ne 0
    if ($playRun.TimedOut) { $Issues.Add('PlayMode TIMEOUT (gate killed only its own child process)') }
    elseif ($null -eq $playResult) { $Issues.Add('PlayMode XML missing/malformed/missing result node') }
    elseif ($playRun.ExitCode -ne 0) { $Issues.Add("PlayMode exit code $($playRun.ExitCode) with XML $($playResult.Result) (exit/XML mismatch)") }
}

# 5) Content Audit: freshness (re-persisted this round) + verdict
$auditAfterUtc = $null
$audit = Read-AuditVerdict ''
$auditExists = Test-Path -LiteralPath $AuditPath -PathType Leaf
if ($auditExists) {
    $auditAfterUtc = (Get-Item -LiteralPath $AuditPath).LastWriteTimeUtc
    $audit = Read-AuditVerdict (Get-Content -LiteralPath $AuditPath -Raw)
}
$fresh = $auditExists -and $null -ne $auditAfterUtc -and (Test-AuditFresh -BeforeUtc $auditBeforeUtc -AfterUtc $auditAfterUtc -GateStartUtc $gateStartUtc)
$auditOk = $auditExists -and $fresh -and $audit.Completed -eq 'YES' -and $audit.Verdict -eq 'PASS' -and $audit.FailureCount -eq 0
if (-not $auditExists) { $Issues.Add("Content Audit file missing: $AuditPath") }
elseif (-not $fresh) { $Issues.Add('Content Audit not re-persisted during this gate run (stale snapshot)') }
elseif ($audit.Completed -ne 'YES') { $Issues.Add("Content Audit completed=$($audit.Completed)") }
elseif ($audit.Verdict -ne 'PASS') { $Issues.Add("Content Audit verdict=$($audit.Verdict)") }
elseif ($audit.FailureCount -ne 0) { $Issues.Add("Content Audit failure count=$($audit.FailureCount)") }

# 6) Player build (Full Gate only; reuses resolved Unity, lock, timeout, temp, summary infra)
$buildStatus = 'SKIPPED'
$buildRun = $null
$artifact = $null
$buildExe = Join-Path $TempRoot 'PlayerBuild\GAME-ZZZ.exe'
$buildLog = Join-Path $TempRoot 'PlayerBuild.log'
if (-not $IncludeBuild) {
    # Quick Gate：不构建，也不谎称已验证 Player
} elseif ($editInfra -or $playInfra) {
    $buildStatus = 'NOT_RUN'
    $Issues.Add('PlayerBuild NOT RUN / INFRA BLOCKED (suite infrastructure failure; no fake build PASS)')
} else {
    New-Item -ItemType Directory -Path (Split-Path -Parent $buildExe) -Force | Out-Null
    $buildRun = Invoke-UnityChild -UnityExe $unityExe -ArgumentString "-batchmode -quit -projectPath `"$ProjectRoot`" -buildTarget win64 -buildWindows64Player `"$buildExe`" -logFile `"$buildLog`"" -TimeoutMin $BuildTimeoutMinutes
    $artifact = Test-PlayerArtifact -ExecutablePath $buildExe
    if ($buildRun.TimedOut) {
        $buildStatus = 'FAIL'
        $Issues.Add('PlayerBuild TIMEOUT (gate killed only its own child process)')
    }
    elseif ($buildRun.ExitCode -ne 0) {
        $buildStatus = 'FAIL'
        $Issues.Add("PlayerBuild Unity exit code $($buildRun.ExitCode)")
    }
    elseif (-not ($artifact.ExecutableExists -and $artifact.ExecutableBytes -gt 0 -and $artifact.DataDirectoryExists -and $artifact.DataFileCount -gt 0)) {
        $buildStatus = 'FAIL'
        $Issues.Add('PlayerBuild artifact invalid (exe missing/empty or Data folder missing/empty)')
    }
    else {
        $buildStatus = 'PASS'
    }
}
$buildOk = (-not $IncludeBuild) -or ($buildStatus -eq 'PASS')

# 7) Finalize once: unified verdict + summary + exit code
$editOk = (-not $editInfra) -and $editResult.Result -eq 'Passed' -and $editResult.Failed -eq 0
$playOk = (-not $playInfra) -and $null -ne $playResult -and $playResult.Result -eq 'Passed' -and $playResult.Failed -eq 0
$gatePass = $editOk -and $playOk -and $auditOk -and $buildOk

$editLine = 'EditMode: N/A'
if ($null -ne $editResult) {
    $editLine = "EditMode: $(@('FAIL', 'PASS')[[int]$editOk]) $($editResult.Passed)/$($editResult.Total), failed $($editResult.Failed), skipped $($editResult.Skipped)"
}
$playLine = 'PlayMode: N/A (skipped)'
if ($null -ne $playResult) {
    $playLine = "PlayMode: $(@('FAIL', 'PASS')[[int]$playOk]) $($playResult.Passed)/$($playResult.Total), failed $($playResult.Failed), skipped $($playResult.Skipped)"
}
$playSummary = Get-SuiteSummary -Result $playResult -Run $playRun -Ok $playOk -Infra $playInfra
if ($playSkipped -and $null -eq $playRun) { $playSummary.status = 'SKIPPED' }
$auditLine = "ContentAudit: $(@('FAIL', 'PASS')[[int]$auditOk]), fresh=$(@('NO', 'YES')[[int]$fresh]), failures=$($audit.FailureCount)"
$buildLine = 'PlayerBuild: SKIPPED (use -IncludeBuild)'
if ($IncludeBuild) {
    $buildLine = "PlayerBuild: $buildStatus win64"
    if ($null -ne $artifact -and $artifact.ExecutableExists) {
        $buildLine += " (GAME-ZZZ.exe $($artifact.ExecutableBytes) bytes, Data files: $($artifact.DataFileCount))"
    }
}

Write-Output $editLine
Write-Output $playLine
Write-Output $auditLine
Write-Output $buildLine
foreach ($issue in $Issues) { Write-Output "Issue: $issue" }
Write-Output "Gate: $(@('FAIL', 'PASS')[[int]$gatePass])"
Write-Output "Artifacts: $TempRoot"

Write-SummaryJson -Path (Join-Path $TempRoot 'verification-summary.json') -Data @{
    unityVersion = (Get-RequiredUnityVersion)
    unityPath = $unityExe
    unityResolutionSource = $resolved.Source
    projectRoot = $ProjectRoot
    editMode = (Get-SuiteSummary -Result $editResult -Run $editRun -Ok $editOk -Infra $editInfra)
    playMode = $playSummary
    contentAudit = @{ exists = $auditExists; fresh = $fresh; completed = $audit.Completed; verdict = $audit.Verdict; failureCount = $audit.FailureCount; beforeUtc = $auditBeforeUtc; afterUtc = $auditAfterUtc }
    build = @{ requested = [bool]$IncludeBuild; status = $buildStatus; unityExitCode = $(if ($null -ne $buildRun) { $buildRun.ExitCode } else { $null }); timedOut = $(if ($null -ne $buildRun) { $buildRun.TimedOut } else { $false }); executablePath = $buildExe; executableExists = $(if ($null -ne $artifact) { $artifact.ExecutableExists } else { $false }); executableBytes = $(if ($null -ne $artifact) { $artifact.ExecutableBytes } else { 0 }); dataDirectoryExists = $(if ($null -ne $artifact) { $artifact.DataDirectoryExists } else { $false }); timeoutMinutes = $BuildTimeoutMinutes; logPath = $buildLog }
    gate = @{ verdict = $(@('FAIL', 'PASS')[[int]$gatePass]); startedUtc = $gateStartUtc; endedUtc = [DateTime]::UtcNow }
    issues = $Issues
}

if ($gatePass) { exit 0 } else { exit 1 }
