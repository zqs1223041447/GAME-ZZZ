# =====================================================================
# GAME-ZZZ Performance Evidence Snapshotter (S3-M8) — explicit Operator tool.
# Verifier/Operator separation (docs/qa/UNATTENDED_VERIFICATION.md):
#   verify_unattended.ps1  = Verifier: runs/measures/judges, outputs TEMP only,
#                            never writes frozen repo evidence.
#   snapshot_performance_evidence.ps1 = Operator: on explicit invocation only,
#                            copies the CURRENT TEMP PASS result into a frozen
#                            evidence archive. Does not run Unity, does not
#                            modify performance results, does not judge budgets.
#
# Canonical usage (from Unity project root):
#   .\tools\snapshot_performance_evidence.ps1 `
#       -Destination "docs\reviews\s2p\1440p-120-m8-revalidation\gate-a"
#   .\tools\snapshot_performance_evidence.ps1 -SourceRoot <dir> -Destination <dir>
#   .\tools\snapshot_performance_evidence.ps1 -VerifyArchive "docs\reviews\s2p\...\gate-a"
#   .\tools\snapshot_performance_evidence.ps1 -SelfTest   # no Unity, synthetic fixtures
#
# Refusals (exit 1): destination exists (no -Force by design), summary verdict
#   != PASS, environment != PASS, missing/invalid run evidence, evidence vs
#   contract mismatch, uncommitted Runtime/ProjectSettings/contract changes.
# Exit codes: 0 = OK | 1 = refused/failed | 2 = usage/infra error.
# =====================================================================
[CmdletBinding()]
param(
    [string]$Destination = "",
    [string]$SourceRoot = "",
    [string]$ContractPath = "",
    [string]$VerifyArchive = "",
    [switch]$SelfTest
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$DefaultSourceRoot = Join-Path ([System.IO.Path]::GetTempPath()) 'GAME-ZZZ-UnattendedGate'
$DefaultContractPath = Join-Path $ProjectRoot 'docs\qa\PERFORMANCE_GATE.json'

function Read-JsonFile {
    param([string]$Path, [string]$What)
    if (-not (Test-Path -LiteralPath $Path)) { throw "$What not found: $Path" }
    try { return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json }
    catch { throw "$What is not valid JSON ($Path): $($_.Exception.Message)" }
}

# ---------------------------------------------------------------------
# Contract: same required-field rules as the gate (independent small parse).
# ---------------------------------------------------------------------
function Get-ValidatedContract {
    param([string]$Path)
    $c = Read-JsonFile -Path $Path -What 'Performance contract'
    $required = @('schemaVersion', 'resolution', 'fullscreen', 'graphicsApi', 'quality',
        'vSyncCount', 'targetFrameRate', 'densities', 'warmupFrames', 'sampleFrames',
        'castIntervalSeconds', 'frameBudgetMs', 'requiredRuns', 'requireFrameTiming',
        'minAliveRatio', 'hardware')
    foreach ($field in $required) {
        if ($null -eq $c.$field) { return @{ Ok = $false; Reason = "contract missing required field '$field'" } }
    }
    if ([int]$c.schemaVersion -ne 1) { return @{ Ok = $false; Reason = "unsupported contract schemaVersion $($c.schemaVersion)" } }
    $cpu = "$($c.hardware.processorType)".Trim()
    $gpu = "$($c.hardware.graphicsDeviceName)".Trim()
    if (-not $cpu -or $cpu -like '*PENDING_HARDWARE_PROBE*') { return @{ Ok = $false; Reason = 'contract hardware CPU is PENDING_HARDWARE_PROBE (probe + lock hardware first)' } }
    if (-not $gpu -or $gpu -like '*PENDING_HARDWARE_PROBE*') { return @{ Ok = $false; Reason = 'contract hardware GPU is PENDING_HARDWARE_PROBE (probe + lock hardware first)' } }
    if (@($c.densities).Count -eq 0) { return @{ Ok = $false; Reason = 'contract densities empty' } }
    if ([int]$c.requiredRuns -lt 1) { return @{ Ok = $false; Reason = 'contract requiredRuns < 1' } }
    return @{ Ok = $true; Reason = ''; Contract = $c }
}

# ---------------------------------------------------------------------
# Summary preconditions: refuse anything that is not a complete PASS run.
# ---------------------------------------------------------------------
function Test-SummaryPassPreconditions {
    param($Summary, $Contract)
    if ($null -eq $Summary) { return @{ Ok = $false; Reason = 'verification-summary.json missing' } }
    $perf = $Summary.performance
    if ($null -eq $perf) { return @{ Ok = $false; Reason = 'summary has no performance section' } }
    if (-not $perf.requested) { return @{ Ok = $false; Reason = 'performance.requested is false (not a Performance Gate run)' } }
    if ($perf.verdict -ne 'PASS') { return @{ Ok = $false; Reason = "performance verdict is $($perf.verdict) (only PASS evidence may be frozen)" } }
    if ($perf.environmentStatus -ne 'PASS') { return @{ Ok = $false; Reason = "performance environmentStatus is $($perf.environmentStatus)" } }
    if (-not $perf.hardwareMatch) { return @{ Ok = $false; Reason = 'performance.hardwareMatch is false' } }

    # Cross-check summary environment fields against the contract (never trust one verdict string).
    $c = $Contract
    $w = [int]$c.resolution.width; $h = [int]$c.resolution.height
    if ("$($perf.resolution)" -ne "$($w)x$($h)") { return @{ Ok = $false; Reason = "summary resolution $($perf.resolution) != contract $($w)x$($h)" } }
    if ("$($perf.graphicsApi)" -ne "$($c.graphicsApi)") { return @{ Ok = $false; Reason = "summary graphicsApi $($perf.graphicsApi) != contract $($c.graphicsApi)" } }
    if ("$($perf.quality)" -ne "$($c.quality)") { return @{ Ok = $false; Reason = "summary quality $($perf.quality) != contract $($c.quality)" } }
    if ([int]$perf.vsync -ne [int]$c.vSyncCount) { return @{ Ok = $false; Reason = "summary vsync $($perf.vsync) != contract vSyncCount $($c.vSyncCount)" } }
    if ([int]$perf.targetFrameRate -ne [int]$c.targetFrameRate) { return @{ Ok = $false; Reason = "summary targetFrameRate $($perf.targetFrameRate) != contract $($c.targetFrameRate)" } }
    if ([double]$perf.frameBudgetMs -ne [double]$c.frameBudgetMs) { return @{ Ok = $false; Reason = "summary frameBudgetMs $($perf.frameBudgetMs) != contract $($c.frameBudgetMs)" } }

    # Required runs complete: every run, every density, from the CONTRACT (not from what the summary happens to contain).
    $runs = @($perf.runs)
    if ($runs.Count -ne [int]$c.requiredRuns) { return @{ Ok = $false; Reason = "summary has $($runs.Count) runs, contract requires $($c.requiredRuns)" } }
    $densities = @($c.densities)
    for ($i = 0; $i -lt $runs.Count; $i++) {
        $run = $runs[$i]
        if ([int]$run.index -ne ($i + 1)) { return @{ Ok = $false; Reason = "run $($run.index) out of order (expected index $($i + 1))" } }
        if ("$($run.envStatus)" -ne 'PASS') { return @{ Ok = $false; Reason = "run $($run.index) envStatus $($run.envStatus)" } }
        $rd = @($run.densities)
        if ($rd.Count -ne $densities.Count) { return @{ Ok = $false; Reason = "run $($run.index) has $($rd.Count) densities, contract requires $($densities.Count)" } }
        for ($j = 0; $j -lt $rd.Count; $j++) {
            if ([int]$rd[$j].density -ne [int]$densities[$j]) { return @{ Ok = $false; Reason = "run $($run.index) density $($rd[$j].density) != contract $($densities[$j])" } }
            if ("$($rd[$j].status)" -ne 'PASS') { return @{ Ok = $false; Reason = "run $($run.index) density $($rd[$j].density) status $($rd[$j].status)" } }
        }
    }

    # Full gate context: tests/audit/build green, gate green, no issues recorded.
    foreach ($suite in @('editMode', 'playMode')) {
        $s = $Summary.$suite
        if ($null -eq $s -or "$($s.status)" -ne 'PASS' -or [int]$s.failed -ne 0) {
            return @{ Ok = $false; Reason = "$suite is not PASS ($($s.status), failed=$($s.failed))" }
        }
    }
    $audit = $Summary.contentAudit
    if ($null -eq $audit -or -not $audit.completed -or "$($audit.verdict)" -ne 'PASS') {
        return @{ Ok = $false; Reason = "contentAudit not PASS (completed=$($audit.completed), verdict=$($audit.verdict))" }
    }
    $build = $Summary.build
    if ($null -eq $build -or -not $build.requested -or "$($build.status)" -ne 'PASS' -or -not $build.executableExists -or -not $build.dataDirectoryExists) {
        return @{ Ok = $false; Reason = 'build not PASS (Player build evidence incomplete)' }
    }
    $gate = $Summary.gate
    if ($null -eq $gate -or "$($gate.verdict)" -ne 'PASS') { return @{ Ok = $false; Reason = "gate verdict is $($gate.verdict)" } }
    if (@($Summary.issues).Count -gt 0) { return @{ Ok = $false; Reason = "summary records $($Summary.issues.Count) issue(s): $($Summary.issues -join ' | ')" } }
    return @{ Ok = $true; Reason = '' }
}

# ---------------------------------------------------------------------
# Raw evidence headers vs contract (cannot trust the summary alone).
# ---------------------------------------------------------------------
function Test-EvidenceFileAgainstContract {
    param([string]$FilePath, [int]$ExpectedDensity, $Contract)
    if (-not (Test-Path -LiteralPath $FilePath)) { return @{ Ok = $false; Reason = "missing raw evidence file: $([System.IO.Path]::GetFileName($FilePath))" } }
    $lines = @(Get-Content -LiteralPath $FilePath -Encoding UTF8)
    if ($lines.Count -lt 3) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) too short (<3 lines)" } }

    $text = ($lines -join "`n")
    $getMeta = {
        param($pattern)
        $m = [regex]::Match($text, $pattern)
        if ($m.Success) { return $m.Groups[1].Value } else { return $null }
    }

    $density = [int]((& $getMeta 'density=(\d+)'))
    if ($density -ne $ExpectedDensity) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) header density=$density, expected $ExpectedDensity" } }

    $w = [int]$Contract.resolution.width; $h = [int]$Contract.resolution.height
    $resMatch = [regex]::Match($text, 'resolution=(\d+)x(\d+)')
    if (-not $resMatch.Success -or [int]$resMatch.Groups[1].Value -ne $w -or [int]$resMatch.Groups[2].Value -ne $h) {
        return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) resolution != contract $($w)x$($h)" }
    }
    if ([regex]::Match($text, 'editor=(True|False)').Groups[1].Value -ne 'False') {
        return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) editor=True (editor run cannot serve as player evidence)" }
    }
    $dx = & $getMeta 'dx=([A-Za-z0-9]+)'
    if ("$dx" -ne "$($Contract.graphicsApi)") { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) dx=$dx != contract $($Contract.graphicsApi)" } }

    $cpu = ("$(& $getMeta 'hardware_cpu=(.*)')").Trim()
    $gpu = ("$(& $getMeta 'hardware_gpu=(.*)')").Trim()
    if ($cpu -ne ("$($Contract.hardware.processorType)").Trim()) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) hardware_cpu '$cpu' != contract" } }
    if ($gpu -ne ("$($Contract.hardware.graphicsDeviceName)").Trim()) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) hardware_gpu '$gpu' != contract" } }

    $env = [regex]::Match($text, 'perf_env quality=(\S+) vsync=(-?\d+) targetFps=(-?\d+) warmup=(\d+) sample=(\d+) castInterval=([\d.]+)')
    if (-not $env.Success) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) missing perf_env metadata line" } }
    if ($env.Groups[1].Value -ne "$($Contract.quality)") { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) quality $($env.Groups[1].Value) != contract $($Contract.quality)" } }
    if ([int]$env.Groups[2].Value -ne [int]$Contract.vSyncCount) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) vsync $($env.Groups[2].Value) != contract $($Contract.vSyncCount)" } }
    if ([int]$env.Groups[3].Value -ne [int]$Contract.targetFrameRate) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) targetFps $($env.Groups[3].Value) != contract $($Contract.targetFrameRate)" } }
    if ([int]$env.Groups[4].Value -ne [int]$Contract.warmupFrames) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) warmup $($env.Groups[4].Value) != contract $($Contract.warmupFrames)" } }
    if ([int]$env.Groups[5].Value -ne [int]$Contract.sampleFrames) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) sample $($env.Groups[5].Value) != contract $($Contract.sampleFrames)" } }
    $ci = [double]::Parse($env.Groups[6].Value, [System.Globalization.CultureInfo]::InvariantCulture)
    if ([math]::Abs($ci - [double]$Contract.castIntervalSeconds) -gt 1e-9) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) castInterval $ci != contract $($Contract.castIntervalSeconds)" } }

    $csvIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i].StartsWith('dummy_count,')) { $csvIndex = $i; break } }
    if ($csvIndex -lt 0) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) missing CSV header row" } }
    $dataRows = @($lines[($csvIndex + 1)..($lines.Count - 1)] | Where-Object { $_.Trim().Length -gt 0 })
    if ($dataRows.Count -lt 1) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) has no data row" } }
    if ($dataRows[0].Split(',').Count -ne 13) { return @{ Ok = $false; Reason = "$([System.IO.Path]::GetFileName($FilePath)) data row does not have 13 columns" } }
    return @{ Ok = $true; Reason = '' }
}

# ---------------------------------------------------------------------
# Git truth: source commit + refuse uncommitted Runtime/ProjectSettings/contract.
# ---------------------------------------------------------------------
function Get-GitSnapshotTruth {
    param([string]$WorkDir)
    $git = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $git) { return @{ Ok = $false; Reason = 'git not available on PATH'; Commit = '' } }
    $pushed = $false
    try {
        Push-Location -LiteralPath $WorkDir
        $pushed = $true
        $commit = (& git rev-parse HEAD 2>&1 | Out-String).Trim()
        if ($LASTEXITCODE -ne 0 -or $commit -match 'fatal|error') {
            return @{ Ok = $false; Reason = "git rev-parse HEAD failed: $commit"; Commit = '' }
        }
        $porcelain = (& git -c core.quotepath=false status --porcelain 2>&1 | Out-String).TrimEnd()
        if ($LASTEXITCODE -ne 0) {
            return @{ Ok = $false; Reason = "git status failed: $porcelain"; Commit = '' }
        }
        $forbidden = @()
        foreach ($line in ($porcelain -split "`r?`n" | Where-Object { $_.Trim().Length -gt 0 })) {
            $path = $line.Substring(3).Trim('"').Replace('\', '/')
            if ($path -like '*Assets/Runtime/*' -or $path -like '*ProjectSettings/*' -or $path -like '*docs/qa/PERFORMANCE_GATE.json') {
                $forbidden += $path
            }
        }
        if ($forbidden.Count -gt 0) {
            return @{ Ok = $false; Reason = "uncommitted Runtime/ProjectSettings/contract changes (commit first): $($forbidden -join ' | ')"; Commit = '' }
        }
        return @{ Ok = $true; Reason = ''; Commit = $commit }
    }
    finally {
        if ($pushed) { Pop-Location }
    }
}

# ---------------------------------------------------------------------
# Core snapshot: verify + copy + manifest. Returns result object (no exit).
# ---------------------------------------------------------------------
function New-PerformanceSnapshot {
    param(
        [string]$SourceRoot,
        [string]$Destination,
        [string]$ContractPath,
        [switch]$SkipGitCheck
    )
    $result = @{ Ok = $false; Reason = ''; SourceCommit = ''; FilesCopied = 0; ManifestPath = '' }

    if (-not (Test-Path -LiteralPath $SourceRoot)) { $result.Reason = "source root not found: $SourceRoot"; return $result }
    $summaryPath = Join-Path $SourceRoot 'verification-summary.json'
    if (-not (Test-Path -LiteralPath $summaryPath)) { $result.Reason = "verification-summary.json not found in $SourceRoot (not a finished gate run?)"; return $result }
    if (Test-Path -LiteralPath $Destination) { $result.Reason = "destination already exists (refusing to overwrite; frozen evidence is append-by-new-directory): $Destination"; return $result }

    $contractCheck = Get-ValidatedContract -Path $ContractPath
    if (-not $contractCheck.Ok) { $result.Reason = $contractCheck.Reason; return $result }
    $contract = $contractCheck.Contract

    $summary = $null
    try { $summary = Read-JsonFile -Path $summaryPath -What 'verification-summary' }
    catch { $result.Reason = $_.Exception.Message; return $result }
    $pre = Test-SummaryPassPreconditions -Summary $summary -Contract $contract
    if (-not $pre.Ok) { $result.Reason = $pre.Reason; return $result }

    # Raw evidence inventory BEFORE any copying: requiredRuns x (densities + PlayerRun.log).
    $densities = @($contract.densities)
    $runSources = @()
    for ($run = 1; $run -le [int]$contract.requiredRuns; $run++) {
        $runDir = Join-Path $SourceRoot "Performance\Run$run"
        if (-not (Test-Path -LiteralPath $runDir)) { $result.Reason = "missing run directory: Performance\Run$run"; return $result }
        $files = @()
        foreach ($d in $densities) {
            $files += @{ Source = (Join-Path $runDir "$d.txt"); Relative = "Run$run/$d.txt" }
        }
        $files += @{ Source = (Join-Path $runDir 'PlayerRun.log'); Relative = "Run$run/PlayerRun.log" }
        foreach ($f in $files) {
            if (-not (Test-Path -LiteralPath $f.Source)) { $result.Reason = "missing raw evidence: Performance\Run$run\$([System.IO.Path]::GetFileName($f.Source))"; return $result }
            $name = [System.IO.Path]::GetFileNameWithoutExtension($f.Source)
            $d = 0
            if ([int]::TryParse($name, [ref]$d)) {
                $check = Test-EvidenceFileAgainstContract -FilePath $f.Source -ExpectedDensity $d -Contract $contract
                if (-not $check.Ok) { $result.Reason = $check.Reason; return $result }
            }
        }
        $runSources += $files
    }

    $commit = ''
    if (-not $SkipGitCheck) {
        $git = Get-GitSnapshotTruth -WorkDir $ProjectRoot
        if (-not $git.Ok) { $result.Reason = $git.Reason; return $result }
        $commit = $git.Commit
    }

    # Copy: 12 raw files (or requiredRuns x (densities+1)) + summary + contract snapshot. Never Player build artifacts.
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    $copied = @()
    try {
        foreach ($f in $runSources) {
            $destFile = Join-Path $Destination ($f.Relative.Replace('/', '\'))
            New-Item -ItemType Directory -Path (Split-Path -Parent $destFile) -Force | Out-Null
            Copy-Item -LiteralPath $f.Source -Destination $destFile -Force
            $copied += $f.Relative
        }
        Copy-Item -LiteralPath $summaryPath -Destination (Join-Path $Destination 'verification-summary.json') -Force
        $copied += 'verification-summary.json'
        Copy-Item -LiteralPath $ContractPath -Destination (Join-Path $Destination 'PERFORMANCE_GATE.json') -Force
        $copied += 'PERFORMANCE_GATE.json'
    }
    catch {
        $result.Reason = "copy failed: $($_.Exception.Message)"
        return $result
    }

    # Manifest with per-file SHA-256 (computed over the final archive contents).
    $files = @()
    foreach ($rel in $copied) {
        $destFile = Join-Path $Destination ($rel.Replace('/', '\'))
        $hash = (Get-FileHash -LiteralPath $destFile -Algorithm SHA256).Hash.ToLowerInvariant()
        $files += @{ relativePath = $rel; sha256 = $hash; bytes = (Get-Item -LiteralPath $destFile).Length }
    }
    $contractHash = ($files | Where-Object { $_.relativePath -eq 'PERFORMANCE_GATE.json' }).sha256
    $summaryHash = ($files | Where-Object { $_.relativePath -eq 'verification-summary.json' }).sha256
    $manifest = [ordered]@{
        schemaVersion = 1
        sourceCommit = $commit
        contractSha256 = $contractHash
        verificationSummarySha256 = $summaryHash
        expectedRuns = [int]$contract.requiredRuns
        expectedDensities = @($contract.densities)
        performanceVerdict = "$($summary.performance.verdict)"
        environmentStatus = "$($summary.performance.environmentStatus)"
        capturedAtUtc = [DateTime]::UtcNow.ToString('o')
        files = $files
    }
    $manifestPath = Join-Path $Destination 'MANIFEST.json'
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

    $result.Ok = $true
    $result.SourceCommit = $commit
    $result.FilesCopied = $files.Count
    $result.ManifestPath = $manifestPath
    return $result
}

# ---------------------------------------------------------------------
# Archive verification (read-only): manifest/files/sizes/SHA-256/contract.
# ---------------------------------------------------------------------
function Test-EvidenceArchive {
    param([string]$ArchivePath)
    $reasons = @()
    $manifestPath = Join-Path $ArchivePath 'MANIFEST.json'
    if (-not (Test-Path -LiteralPath $manifestPath)) { return @{ Ok = $false; Reasons = @("MANIFEST.json not found in $ArchivePath"); FileCount = 0 } }
    try { $manifest = Read-JsonFile -Path $manifestPath -What 'MANIFEST.json' }
    catch { return @{ Ok = $false; Reasons = @($_.Exception.Message); FileCount = 0 } }
    if ([int]$manifest.schemaVersion -ne 1) { $reasons += "unsupported manifest schemaVersion $($manifest.schemaVersion)" }

    $files = @($manifest.files)
    if ($files.Count -eq 0) { $reasons += 'manifest has no files[] entries' }
    foreach ($f in $files) {
        $destFile = Join-Path $ArchivePath ("$($f.relativePath)".Replace('/', '\'))
        if (-not (Test-Path -LiteralPath $destFile)) { $reasons += "missing archived file: $($f.relativePath)"; continue }
        $item = Get-Item -LiteralPath $destFile
        if ($item.Length -ne [int64]$f.bytes) { $reasons += "size mismatch: $($f.relativePath) (manifest $($f.bytes) != actual $($item.Length))" }
        $hash = (Get-FileHash -LiteralPath $destFile -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($hash -ne "$($f.sha256)".ToLowerInvariant()) { $reasons += "sha256 mismatch: $($f.relativePath)" }
    }
    $contractSnap = Join-Path $ArchivePath 'PERFORMANCE_GATE.json'
    if (Test-Path -LiteralPath $contractSnap) {
        $cc = Get-ValidatedContract -Path $contractSnap
        if (-not $cc.Ok) { $reasons += "contract snapshot invalid: $($cc.Reason)" }
    }
    else { $reasons += 'contract snapshot PERFORMANCE_GATE.json missing' }
    if (-not (Test-Path -LiteralPath (Join-Path $ArchivePath 'verification-summary.json'))) { $reasons += 'verification-summary.json missing' }

    return @{ Ok = ($reasons.Count -eq 0); Reasons = $reasons; FileCount = $files.Count }
}

# ---------------------------------------------------------------------
# SelfTest: synthetic fixtures, no Unity, no real performance run.
# ---------------------------------------------------------------------
function New-SelfTest {
    $fx = Join-Path ([System.IO.Path]::GetTempPath()) 'GAME-ZZZ-PerfSnapshotSelfTest'
    if (Test-Path -LiteralPath $fx) { Remove-Item -LiteralPath $fx -Recurse -Force }
    New-Item -ItemType Directory -Path $fx -Force | Out-Null

    # Synthetic contract (NOT the repo contract): smaller fixture tree.
    $contract = @{
        schemaVersion = 1
        resolution = @{ width = 1280; height = 720 }
        fullscreen = $true
        graphicsApi = 'Direct3D12'
        quality = 'PC'
        vSyncCount = 0
        targetFrameRate = -1
        densities = @(100, 200, 300)
        warmupFrames = 60
        sampleFrames = 600
        castIntervalSeconds = 0.12
        frameBudgetMs = 8.33
        requiredRuns = 2
        requireFrameTiming = $true
        minAliveRatio = 0.95
        hardware = @{ processorType = 'Test CPU X1'; graphicsDeviceName = 'Test GPU X2' }
    }
    $contractPath = Join-Path $fx 'contract.json'
    $contract | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $contractPath -Encoding UTF8

    $evidence = {
        param([int]$density, [string]$cpu, [string]$gpu)
        $alive = $density
        if ($density -eq 300) { $alive = 291 }
        @"
# ArenaPerfHarness, density=$density
# resolution=1280x720 fullscreen=True currentRes=1280x720 editor=False dx=Direct3D12
# hardware_cpu=$cpu
# hardware_gpu=$gpu
# perf_env quality=PC vsync=0 targetFps=-1 warmup=60 sample=600 castInterval=0.12
# density strategy: kill-then-refill (alive kept at nominal via SpawnAt top-up)
dummy_count,alive,frames,main_ms_avg,main_ms_p95,main_ms_p99,main_ms_p999,main_ms_max,gc_alloc_bytes_avg,cpu_ms_avg,gpu_ms_avg,mem_total_mb,frame_timing_ok
$density,$alive,600,1.0,1.5,2.0,2.5,3.0,0.0,1.0,0.5,100.0,true
"@
    }

    function New-FixtureTree {
        param([string]$Root, [string]$Verdict, [string]$EnvStatus, [int[]]$SkipRuns = @(), [string]$DropFile = '', [string]$Cpu = 'Test CPU X1', [string]$Gpu = 'Test GPU X2')
        if (Test-Path -LiteralPath $Root) { Remove-Item -LiteralPath $Root -Recurse -Force }
        New-Item -ItemType Directory -Path $Root -Force | Out-Null
        $perf = @{
            requested = $true; verdict = $Verdict; environmentStatus = $EnvStatus; frameBudgetMs = 8.33
            hardwareMatch = $true; resolution = '1280x720'; graphicsApi = 'Direct3D12'; quality = 'PC'
            vsync = 0; targetFrameRate = -1
            runs = @()
        }
        $runs = @()
        for ($r = 1; $r -le 2; $r++) {
            if ($SkipRuns -contains $r) { continue }
            $ds = @()
            foreach ($d in @(100, 200, 300)) { $ds += @{ density = $d; alive = $d; frames = 600; avg = 1.0; p99 = 2.0; cpuAvg = 1.0; gpuAvg = 0.5; status = 'PASS'; reasons = @() } }
            $runs += @{ index = $r; envStatus = 'PASS'; densities = $ds }
        }
        $perf.runs = $runs
        $summary = @{
            editMode = @{ status = 'PASS'; total = 10; passed = 10; failed = 0; skipped = 0; exitCode = 0; timedOut = $false }
            playMode = @{ status = 'PASS'; total = 3; passed = 3; failed = 0; skipped = 0; exitCode = 0; timedOut = $false }
            contentAudit = @{ exists = $true; fresh = $true; completed = $true; verdict = 'PASS'; failureCount = 0 }
            performance = $perf
            build = @{ requested = $true; status = 'PASS'; executableExists = $true; dataDirectoryExists = $true }
            playerRun = @{ requested = $false; status = 'SKIPPED' }
            gate = @{ verdict = 'PASS' }
            issues = @()
        }
        $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Root 'verification-summary.json') -Encoding UTF8
        foreach ($r in @(1, 2)) {
            if ($SkipRuns -contains $r) { continue }
            $runDir = Join-Path $Root "Performance\Run$r"
            New-Item -ItemType Directory -Path $runDir -Force | Out-Null
            foreach ($d in @(100, 200, 300)) {
                if ($DropFile -eq "Run$r/$d.txt") { continue }
                (& $evidence $d $Cpu $Gpu) | Set-Content -LiteralPath (Join-Path $runDir "$d.txt") -Encoding UTF8
            }
            if ($DropFile -ne "Run$r/PlayerRun.log") { Set-Content -LiteralPath (Join-Path $runDir 'PlayerRun.log') -Value 'PlayerRun log line' -Encoding UTF8 }
        }
    }

    $cases = New-Object System.Collections.Generic.List[string]
    $failed = 0

    # 1) PASS tree -> snapshot OK -> archive verify OK -> tamper -> verify catches.
    $src1 = Join-Path $fx 'src-pass'; $dst1 = Join-Path $fx 'dest-pass'
    New-FixtureTree -Root $src1 -Verdict 'PASS' -EnvStatus 'PASS'
    $r1 = New-PerformanceSnapshot -SourceRoot $src1 -Destination $dst1 -ContractPath $contractPath -SkipGitCheck
    if (-not $r1.Ok) { $cases.Add("FAIL 1a PASS-tree snapshot: $($r1.Reason)"); $failed++ }
    else {
        $v1 = Test-EvidenceArchive -ArchivePath $dst1
        if (-not $v1.Ok) { $cases.Add("FAIL 1b archive verify: $($v1.Reasons -join ' | ')"); $failed++ }
        else {
            Add-Content -LiteralPath (Join-Path $dst1 'Run1\100.txt') -Value 'tamper'
            $v1b = Test-EvidenceArchive -ArchivePath $dst1
            if ($v1b.Ok) { $cases.Add('FAIL 1c tamper NOT detected by archive verify'); $failed++ }
            else { $cases.Add('OK    1  PASS tree snapshot + verify + tamper detection') }
        }
    }

    # 2) verdict FAIL -> refuse.
    $src2 = Join-Path $fx 'src-fail'; New-FixtureTree -Root $src2 -Verdict 'FAIL' -EnvStatus 'PASS'
    $r2 = New-PerformanceSnapshot -SourceRoot $src2 -Destination (Join-Path $fx 'dest-fail') -ContractPath $contractPath -SkipGitCheck
    if ($r2.Ok) { $cases.Add('FAIL 2 FAIL verdict was accepted'); $failed++ } else { $cases.Add("OK    2  refuse FAIL verdict ($($r2.Reason))") }

    # 3) ENV_NOT_MET -> refuse.
    $src3 = Join-Path $fx 'src-env'; New-FixtureTree -Root $src3 -Verdict 'PASS' -EnvStatus 'ENV_NOT_MET'
    $r3 = New-PerformanceSnapshot -SourceRoot $src3 -Destination (Join-Path $fx 'dest-env') -ContractPath $contractPath -SkipGitCheck
    if ($r3.Ok) { $cases.Add('FAIL 3 ENV_NOT_MET was accepted'); $failed++ } else { $cases.Add("OK    3  refuse ENV_NOT_MET ($($r3.Reason))") }

    # 4) missing Run2 -> refuse.
    $src4 = Join-Path $fx 'src-run2'; New-FixtureTree -Root $src4 -Verdict 'PASS' -EnvStatus 'PASS' -SkipRuns @(2)
    $r4 = New-PerformanceSnapshot -SourceRoot $src4 -Destination (Join-Path $fx 'dest-run2') -ContractPath $contractPath -SkipGitCheck
    if ($r4.Ok) { $cases.Add('FAIL 4 missing Run2 was accepted'); $failed++ } else { $cases.Add("OK    4  refuse missing Run2 ($($r4.Reason))") }

    # 5) missing 300.txt -> refuse.
    $src5 = Join-Path $fx 'src-dens'; New-FixtureTree -Root $src5 -Verdict 'PASS' -EnvStatus 'PASS' -DropFile 'Run1/300.txt'
    $r5 = New-PerformanceSnapshot -SourceRoot $src5 -Destination (Join-Path $fx 'dest-dens') -ContractPath $contractPath -SkipGitCheck
    if ($r5.Ok) { $cases.Add('FAIL 5 missing 300.txt was accepted'); $failed++ } else { $cases.Add("OK    5  refuse missing 300.txt ($($r5.Reason))") }

    # 6) missing PlayerRun.log -> refuse.
    $src6 = Join-Path $fx 'src-log'; New-FixtureTree -Root $src6 -Verdict 'PASS' -EnvStatus 'PASS' -DropFile 'Run2/PlayerRun.log'
    $r6 = New-PerformanceSnapshot -SourceRoot $src6 -Destination (Join-Path $fx 'dest-log') -ContractPath $contractPath -SkipGitCheck
    if ($r6.Ok) { $cases.Add('FAIL 6 missing PlayerRun.log was accepted'); $failed++ } else { $cases.Add("OK    6  refuse missing PlayerRun.log ($($r6.Reason))") }

    # 7) existing destination -> refuse.
    $src7 = Join-Path $fx 'src-exists'; New-FixtureTree -Root $src7 -Verdict 'PASS' -EnvStatus 'PASS'
    $dst7 = Join-Path $fx 'dest-exists'
    New-Item -ItemType Directory -Path $dst7 -Force | Out-Null
    $r7 = New-PerformanceSnapshot -SourceRoot $src7 -Destination $dst7 -ContractPath $contractPath -SkipGitCheck
    if ($r7.Ok) { $cases.Add('FAIL 7 existing destination was overwritten'); $failed++ } else { $cases.Add("OK    7  refuse existing destination ($($r7.Reason))") }

    # 8) evidence vs contract mismatch (GPU) -> refuse.
    $src8 = Join-Path $fx 'src-gpu'; New-FixtureTree -Root $src8 -Verdict 'PASS' -EnvStatus 'PASS' -Gpu 'Test GPU WRONG'
    $r8 = New-PerformanceSnapshot -SourceRoot $src8 -Destination (Join-Path $fx 'dest-gpu') -ContractPath $contractPath -SkipGitCheck
    if ($r8.Ok) { $cases.Add('FAIL 8 GPU mismatch was accepted'); $failed++ } else { $cases.Add("OK    8  refuse GPU/contract mismatch ($($r8.Reason))") }

    # 9) issues recorded in summary -> refuse.
    $src9 = Join-Path $fx 'src-issues'; New-FixtureTree -Root $src9 -Verdict 'PASS' -EnvStatus 'PASS'
    $s9 = Get-Content -LiteralPath (Join-Path $src9 'verification-summary.json') -Raw | ConvertFrom-Json
    $s9.issues = @('Performance: something suspicious')
    $s9 | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $src9 'verification-summary.json') -Encoding UTF8
    $r9 = New-PerformanceSnapshot -SourceRoot $src9 -Destination (Join-Path $fx 'dest-issues') -ContractPath $contractPath -SkipGitCheck
    if ($r9.Ok) { $cases.Add('FAIL 9 summary with issues was accepted'); $failed++ } else { $cases.Add("OK    9  refuse summary with issues ($($r9.Reason))") }

    foreach ($line in $cases) { Write-Output $line }
    Write-Output ("SelfTest: {0}/9 PASS" -f (9 - $failed))
    if ($failed -gt 0) { exit 1 }
    exit 0
}

# ---------------------------------------------------------------------
# Mode dispatch
# ---------------------------------------------------------------------
if ($SelfTest) {
    New-SelfTest
    return
}

if ($VerifyArchive -ne '') {
    if (-not [System.IO.Path]::IsPathRooted($VerifyArchive)) { $VerifyArchive = Join-Path $ProjectRoot $VerifyArchive }
    if (-not (Test-Path -LiteralPath $VerifyArchive)) { Write-Output "VerifyArchive: FAIL (archive not found: $VerifyArchive)"; exit 1 }
    $v = Test-EvidenceArchive -ArchivePath $VerifyArchive
    foreach ($r in $v.Reasons) { Write-Output "Archive issue: $r" }
    Write-Output ("VerifyArchive: {0} ({1} manifest files)" -f (@('FAIL', 'OK')[[int]$v.Ok]), $v.FileCount)
    if ($v.Ok) { exit 0 } else { exit 1 }
}

if ($Destination -eq '') {
    Write-Output 'Usage: -Destination <dir> | -VerifyArchive <dir> | -SelfTest (see file header)'; exit 2
}
if (-not [System.IO.Path]::IsPathRooted($Destination)) { $Destination = Join-Path $ProjectRoot $Destination }
if ($SourceRoot -eq '') { $SourceRoot = $DefaultSourceRoot }
if ($ContractPath -eq '') { $ContractPath = $DefaultContractPath }

Write-Output "Snapshot: source=$SourceRoot"
Write-Output "Snapshot: contract=$ContractPath"
$snap = New-PerformanceSnapshot -SourceRoot $SourceRoot -Destination $Destination -ContractPath $ContractPath
if (-not $snap.Ok) {
    Write-Output "Snapshot: REFUSED — $($snap.Reason)"
    exit 1
}
Write-Output "Snapshot: source commit $($snap.SourceCommit)"
Write-Output "Snapshot: copied $($snap.FilesCopied) files -> $Destination"
$v = Test-EvidenceArchive -ArchivePath $Destination
if (-not $v.Ok) { Write-Output "Snapshot: post-copy verify FAILED — $($v.Reasons -join ' | ')"; exit 1 }
Write-Output 'Snapshot: OK (manifest written, archive verified)'
exit 0
