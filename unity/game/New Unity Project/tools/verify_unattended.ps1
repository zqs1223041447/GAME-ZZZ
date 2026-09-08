# =====================================================================
# GAME-ZZZ Unattended Verification Gate (S3-M4 tests + S3-M5 build + S3-M6 player runtime + S3-M7 performance)
# Repository-local canonical verification entry. Four tiers:
#   Quick Gate:            EditMode + PlayMode + Content Audit
#   Build Gate (-IncludeBuild):      Quick + StandaloneWindows64 Player Build
#   Player Runtime Gate (-IncludePlayerRun): Build Gate + launch the just-built player,
#                                            run ArenaPerfHarness (-arenaPerf), verify
#                                            100/200/300 evidence. NOT a performance verdict.
#   Performance Gate (-IncludePerformance): Player Runtime + locked environment
#                                           (docs/qa/PERFORMANCE_GATE.json) + 3 repeated
#                                           runs + hard 8.33ms budget. Only this tier may
#                                           print PerformanceVerdict=PASS.
#
# Canonical usage (from Unity project root):
#   .\tools\verify_unattended.ps1                            # Quick Gate
#   .\tools\verify_unattended.ps1 -IncludeBuild              # Build Gate
#   .\tools\verify_unattended.ps1 -IncludePlayerRun          # Player Runtime Gate (implies -IncludeBuild)
#   .\tools\verify_unattended.ps1 -IncludePerformance        # Performance Gate (implies all tiers)
#   .\tools\verify_unattended.ps1 -UnityPath "G:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe"
#   .\tools\verify_unattended.ps1 -TimeoutMinutes 20 -BuildTimeoutMinutes 30 -PlayerRunTimeoutMinutes 10
#   .\tools\verify_unattended.ps1 -SelfTest                  # no Unity launch, synthetic fixtures
#
# Exit codes: 0 = Gate PASS (Performance tier: only verdict=PASS) |
#             1 = Gate FAIL (tests/audit/build/player-run/performance: FAIL/ENV_NOT_MET/EVIDENCE_INCOMPLETE) |
#             2 = Unity not resolved | 3 = project already open (editor lock)
# The gate verifies only; it never modifies STATUS/ROADMAP/catalogs/tests,
# never runs git commands, and never kills Unity processes it did not start.
# Allowed repo side effects: the formal EditMode run re-persists
# docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md (existing audit contract).
# Player build + player run + performance outputs are ephemeral (temp only), never committed.
# =====================================================================
[CmdletBinding()]
param(
    [string]$UnityPath = "",
    [int]$TimeoutMinutes = 20,
    [int]$BuildTimeoutMinutes = 30,
    [int]$PlayerRunTimeoutMinutes = 10,
    [switch]$IncludeBuild,
    [switch]$IncludePlayerRun,
    [switch]$IncludePerformance,
    [switch]$IncludeArtPerformance,
    [switch]$SelfTest
)

# 隐含链：ArtPerformance ⊃ Performance ⊃ PlayerRun ⊃ Build（无需同时写多个 switch；同时提供亦正常）
if ($IncludeArtPerformance) { $IncludePerformance = $true }
if ($IncludePerformance) { $IncludePlayerRun = $true }
if ($IncludePlayerRun) { $IncludeBuild = $true }

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

# ---------------------------------------------------------------------
# ArenaPerfHarness evidence parsers (S3-M6). Pure functions; -SelfTest fixtures only.
# 文件 schema（ArenaPerfHarness.WriteRow）：
#   L1: # ArenaPerfHarness, density=<n>
#   L2: # resolution=WxH fullscreen=.. currentRes=.. editor=<bool> dx=<api>
#   L3: # density strategy: ...
#   L4: dummy_count,alive,frames,main_ms_avg,main_ms_p95,main_ms_p99,main_ms_p999,main_ms_max,gc_alloc_bytes_avg,cpu_ms_avg,gpu_ms_avg,mem_total_mb,frame_timing_ok
#   L5: 恰一行数据
# ---------------------------------------------------------------------

function Read-HarnessResult {
    param([string]$FilePath, [int]$ExpectedDensity)
    $out = @{ Valid = $false; Reason = ''; Density = $ExpectedDensity; Resolution = ''; Fullscreen = ''; Editor = ''; GraphicsApi = ''
        HardwareCpu = ''; HardwareGpu = ''; Quality = ''; VSync = ''; TargetFps = ''; Warmup = 0; Sample = 0; CastInterval = ''
        Alive = -1; Frames = -1; MainMsAvg = [double]::NaN; MainMsP99 = [double]::NaN; GpuMsAvg = [double]::NaN
        CpuMsAvg = [double]::NaN; GcBytesAvg = [double]::NaN; MemTotalMb = [double]::NaN; FrameTimingOk = ''
        SourcePath = $FilePath }
    if ([string]::IsNullOrEmpty($FilePath) -or -not (Test-Path -LiteralPath $FilePath -PathType Leaf)) {
        $out.SourcePath = ''
        $out.Reason = 'missing file'
        return $out
    }
    $lines = @(Get-Content -LiteralPath $FilePath)
    $header = $null; $meta = $null; $csv = -1; $dataRows = @()
    $hwCpu = ''; $hwGpu = ''; $perfEnv = ''
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($null -eq $header -and $line -match '^#\s+.*density=(\d+)\s*$') { $header = $line; continue }
        if ($null -eq $meta -and $line -match '^#\s+resolution=') { $meta = $line; continue }
        if ($line -match '^#\s+hardware_cpu=(.*)$') { $hwCpu = $Matches[1].Trim(); continue }
        if ($line -match '^#\s+hardware_gpu=(.*)$') { $hwGpu = $Matches[1].Trim(); continue }
        if ($line -match '^#\s+perf_env\s+(.*)$') { $perfEnv = $Matches[1]; continue }
        if ($line -match '^dummy_count,alive,frames,') { $csv = $i; continue }
        if ($csv -ge 0 -and $i -gt $csv -and $line -notmatch '^#' -and $line.Trim().Length -gt 0) { $dataRows += $line }
    }
    $out.HardwareCpu = $hwCpu
    $out.HardwareGpu = $hwGpu
    if ($perfEnv -ne '') {
        $q = [regex]::Match($perfEnv, 'quality=(\S+)')
        $vs = [regex]::Match($perfEnv, 'vsync=(\S+)')
        $tf = [regex]::Match($perfEnv, 'targetFps=(-?\d+)')
        $wu = [regex]::Match($perfEnv, 'warmup=(\d+)')
        $sa = [regex]::Match($perfEnv, 'sample=(\d+)')
        $ci = [regex]::Match($perfEnv, 'castInterval=(\S+)')
        if ($q.Success) { $out.Quality = $q.Groups[1].Value }
        if ($vs.Success) { $out.VSync = $vs.Groups[1].Value }
        if ($tf.Success) { $out.TargetFps = $tf.Groups[1].Value }
        if ($wu.Success) { $out.Warmup = [int]$wu.Groups[1].Value }
        if ($sa.Success) { $out.Sample = [int]$sa.Groups[1].Value }
        if ($ci.Success) { $out.CastInterval = $ci.Groups[1].Value }
    }
    if ($null -eq $header) { $out.Reason = 'unrecognizable header'; return $out }
    $headerDensity = [int]([regex]::Match($header, 'density=(\d+)').Groups[1].Value)
    if ($headerDensity -ne $ExpectedDensity) { $out.Reason = "header density $headerDensity != expected $ExpectedDensity"; return $out }
    if ($null -eq $meta) { $out.Reason = 'metadata line missing'; return $out }
    $res = [regex]::Match($meta, 'resolution=(\d+)x(\d+)')
    if (-not $res.Success -or [int]$res.Groups[1].Value -le 0 -or [int]$res.Groups[2].Value -le 0) {
        $out.Reason = 'invalid resolution'
        return $out
    }
    $out.Resolution = $res.Groups[1].Value + 'x' + $res.Groups[2].Value
    $fs = [regex]::Match($meta, 'fullscreen=(\S+)')
    if ($fs.Success) { $out.Fullscreen = $fs.Groups[1].Value }
    $ed = [regex]::Match($meta, 'editor=(\S+)')
    $out.Editor = $(if ($ed.Success) { $ed.Groups[1].Value } else { '' })
    if ($out.Editor -ne 'False' -and $out.Editor -ne 'false') { $out.Reason = 'editor=True (not a standalone player run)'; return $out }
    $dx = [regex]::Match($meta, 'dx=(\S+)')
    $out.GraphicsApi = $(if ($dx.Success) { $dx.Groups[1].Value } else { '' })
    if ([string]::IsNullOrEmpty($out.GraphicsApi)) { $out.Reason = 'graphics api missing'; return $out }
    if ($csv -lt 0) { $out.Reason = 'csv header missing'; return $out }
    if ($dataRows.Count -ne 1) { $out.Reason = "expected exactly 1 data row, got $($dataRows.Count)"; return $out }
    $f = $dataRows[0].Split(',')
    if ($f.Count -ne 13) { $out.Reason = "csv column count $($f.Count) != 13"; return $out }
    $dummy = 0; $alive = 0; $frames = 0
    if (-not [int]::TryParse($f[0], [ref]$dummy)) { $out.Reason = 'dummy_count not an integer'; return $out }
    if (-not [int]::TryParse($f[1], [ref]$alive)) { $out.Reason = 'alive not an integer'; return $out }
    if (-not [int]::TryParse($f[2], [ref]$frames)) { $out.Reason = 'frames not a positive integer'; return $out }
    if ($dummy -ne $ExpectedDensity) { $out.Reason = "dummy_count $dummy != expected density $ExpectedDensity"; return $out }
    if ($alive -le 0 -or $alive -gt $dummy) { $out.Reason = "alive $alive must be >0 and <=dummy_count $dummy"; return $out }
    if ($frames -le 0) { $out.Reason = "frames $frames must be >0"; return $out }
    $nums = @()
    for ($i = 3; $i -le 10; $i++) {
        $d = [double]0
        if (-not [double]::TryParse($f[$i], [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$d)) {
            $out.Reason = "csv column $i not a parseable number: $($f[$i])"
            return $out
        }
        if ([double]::IsNaN($d) -or [double]::IsInfinity($d)) {
            $out.Reason = "csv column $i is NaN/Infinity: $($f[$i])"
            return $out
        }
        $nums += $d
    }
    $fto = $f[12]
    if ($fto -ne 'true' -and $fto -ne 'false') { $out.Reason = "frame_timing_ok not a bool: $fto"; return $out }
    $out.Alive = $alive
    $out.Frames = $frames
    $out.MainMsAvg = $nums[0]
    $out.MainMsP99 = $nums[2]
    $out.GpuMsAvg = $nums[7]
    $out.CpuMsAvg = $nums[6]
    $out.GcBytesAvg = $nums[5]
    $mem = [double]0
    if (-not [double]::TryParse($f[11], [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$mem)) { $mem = [double]::NaN }
    $out.MemTotalMb = $mem
    $out.FrameTimingOk = $fto
    $out.Valid = $true
    return $out
}

function Test-PlayerRunEvidence {
    param([string]$OutputDir, [int[]]$ExpectedDensities)
    $out = @{ Valid = $false; Reasons = @(); Results = @(); ActualResolution = ''; GraphicsApi = '' }
    if ([string]::IsNullOrEmpty($OutputDir) -or -not (Test-Path -LiteralPath $OutputDir -PathType Container)) {
        $out.Reasons = @("output dir missing: $OutputDir (fallback output elsewhere does not count)")
        return $out
    }
    foreach ($d in $ExpectedDensities) {
        $r = Read-HarnessResult -FilePath (Join-Path $OutputDir "$d.txt") -ExpectedDensity $d
        $out.Results += $r
        if (-not $r.Valid) { $out.Reasons += "density ${d}: $($r.Reason)" }
    }
    $resolutions = @($out.Results | ForEach-Object { $_.Resolution } | Where-Object { $_ -ne '' } | Sort-Object -Unique)
    if ($resolutions.Count -gt 1) { $out.Reasons += "evidence unstable: resolution changed within one player run ($($resolutions -join ' / '))" }
    $apis = @($out.Results | ForEach-Object { $_.GraphicsApi } | Where-Object { $_ -ne '' } | Sort-Object -Unique)
    if ($apis.Count -eq 1) { $out.GraphicsApi = $apis[0] }
    if ($resolutions.Count -eq 1) { $out.ActualResolution = $resolutions[0] }
    $out.Valid = ($out.Reasons.Count -eq 0)
    return $out
}

# ---------------------------------------------------------------------
# Performance contract + pure evaluator（S3-M7）。contract 唯一真相源=docs/qa/PERFORMANCE_GATE.json。
# ---------------------------------------------------------------------

function Get-PerformanceContract {
    param([string]$Path)
    $out = @{ Ok = $false; Contract = $null; Reason = '' }
    if ([string]::IsNullOrEmpty($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        $out.Reason = "contract file missing: $Path"
        return $out
    }
    try { $c = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json } catch {
        $out.Reason = "contract JSON malformed: $($_.Exception.Message)"
        return $out
    }
    foreach ($field in @('resolution', 'fullscreen', 'graphicsApi', 'quality', 'vSyncCount', 'targetFrameRate', 'densities', 'warmupFrames', 'sampleFrames', 'castIntervalSeconds', 'frameBudgetMs', 'requiredRuns', 'requireFrameTiming', 'minAliveRatio', 'hardware')) {
        if ($null -eq $c.$field) { $out.Reason = "contract field missing: $field"; return $out }
    }
    if ("$($c.hardware.processorType)" -match 'PENDING_HARDWARE_PROBE' -or "$($c.hardware.graphicsDeviceName)" -match 'PENDING_HARDWARE_PROBE') {
        $out.Reason = 'contract hardware not locked yet (PENDING_HARDWARE_PROBE; run hardware probe then update docs/qa/PERFORMANCE_GATE.json via formal work order)'
        return $out
    }
    $out.Contract = $c
    $out.Ok = $true
    return $out
}

function Test-PerfEnvironment {
    param($Contract, $Results)
    $out = @{ Status = 'PASS'; Reasons = @(); Resolution = ''; Fullscreen = ''; GraphicsApi = ''; Quality = ''; VSync = ''; TargetFps = ''; HardwareCpu = ''; HardwareGpu = '' }
    $expectedRes = "$($Contract.resolution.width)x$($Contract.resolution.height)"
    $expectedFps = "$($Contract.targetFrameRate)"
    $expectedVs = "$($Contract.vSyncCount)"
    $budgetCi = [double]::Parse("$($Contract.castIntervalSeconds)", [System.Globalization.CultureInfo]::InvariantCulture)
    foreach ($r in $Results) {
        $d = $r.Density
        if ($r.Editor -ne 'False' -and $r.Editor -ne 'false') { $out.Reasons += "density ${d}: editor=$($r.Editor) (must be standalone False)" }
        if ($r.Resolution -ne $expectedRes) { $out.Reasons += "density ${d}: resolution '$($r.Resolution)' != locked '$expectedRes'" }
        $fsOk = ($Contract.fullscreen -and ($r.Fullscreen -eq 'True' -or $r.Fullscreen -eq 'true')) -or
            ((-not $Contract.fullscreen) -and ($r.Fullscreen -eq 'False' -or $r.Fullscreen -eq 'false'))
        if (-not $fsOk) { $out.Reasons += "density ${d}: fullscreen '$($r.Fullscreen)' != locked $($Contract.fullscreen)" }
        if ($r.GraphicsApi -ne $Contract.graphicsApi) { $out.Reasons += "density ${d}: graphics '$($r.GraphicsApi)' != locked '$($Contract.graphicsApi)'" }
        if ($r.Quality -ne $Contract.quality) { $out.Reasons += "density ${d}: quality '$($r.Quality)' != locked '$($Contract.quality)'" }
        if ($r.VSync -ne $expectedVs) { $out.Reasons += "density ${d}: vSync '$($r.VSync)' != locked $expectedVs" }
        if ($r.TargetFps -ne $expectedFps) { $out.Reasons += "density ${d}: targetFps '$($r.TargetFps)' != locked $expectedFps" }
        if ($r.Warmup -ne $Contract.warmupFrames) { $out.Reasons += "density ${d}: warmup $($r.Warmup) != locked $($Contract.warmupFrames)" }
        if ($r.Sample -ne $Contract.sampleFrames) { $out.Reasons += "density ${d}: sample $($r.Sample) != locked $($Contract.sampleFrames)" }
        $ci = [double]0
        $ciOk = [double]::TryParse($r.CastInterval, [System.Globalization.NumberStyles]::Float, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$ci)
        if (-not $ciOk -or [math]::Abs($ci - $budgetCi) -gt 0.0001) { $out.Reasons += "density ${d}: castInterval '$($r.CastInterval)' != locked $($Contract.castIntervalSeconds)" }
        if ($r.HardwareCpu -ne $Contract.hardware.processorType) { $out.Reasons += "density ${d}: CPU '$($r.HardwareCpu)' != locked '$($Contract.hardware.processorType)'" }
        if ($r.HardwareGpu -ne $Contract.hardware.graphicsDeviceName) { $out.Reasons += "density ${d}: GPU '$($r.HardwareGpu)' != locked '$($Contract.hardware.graphicsDeviceName)'" }
    }
    if ($Results.Count -gt 0) {
        $first = $Results[0]
        $out.Resolution = $first.Resolution
        $out.Fullscreen = $first.Fullscreen
        $out.GraphicsApi = $first.GraphicsApi
        $out.Quality = $first.Quality
        $out.VSync = $first.VSync
        $out.TargetFps = $first.TargetFps
        $out.HardwareCpu = $first.HardwareCpu
        $out.HardwareGpu = $first.HardwareGpu
    }
    if ($out.Reasons.Count -gt 0) { $out.Status = 'ENV_NOT_MET' }
    return $out
}

function Test-PerfMetrics {
    param($Contract, $Result)
    $out = @{ Status = 'PASS'; Reasons = @(); TimingAvailable = $true
        Avg = $Result.MainMsAvg; P99 = $Result.MainMsP99; Cpu = $Result.CpuMsAvg; Gpu = $Result.GpuMsAvg
        Alive = $Result.Alive; Frames = $Result.Frames; Density = $Result.Density }
    $d = $Result.Density
    $budget = [double]$Contract.frameBudgetMs
    if ($Result.Frames -ne $Contract.sampleFrames) {
        $out.Reasons += "density ${d}: frames $($Result.Frames) != sampleFrames $($Contract.sampleFrames) (measurement validity)"
    }
    $minAlive = [int][math]::Ceiling($Contract.minAliveRatio * $Result.Density)
    if ($Result.Alive -lt $minAlive) {
        $out.Reasons += "density ${d}: alive $($Result.Alive)/$($Result.Density) < minAliveRatio $($Contract.minAliveRatio) (measurement validity)"
    }
    if ($Result.MainMsAvg -gt $budget) { $out.Reasons += "density ${d}: main_ms_avg $($Result.MainMsAvg) > budget $budget" }
    if ($Result.MainMsP99 -gt $budget) { $out.Reasons += "density ${d}: main_ms_p99 $($Result.MainMsP99) > budget $budget" }
    if ($Contract.requireFrameTiming) {
        if ($Result.FrameTimingOk -ne 'true') {
            $out.TimingAvailable = $false
            $out.Reasons += "density ${d}: frame timing unavailable (frame_timing_ok=$($Result.FrameTimingOk)) -> EVIDENCE_INCOMPLETE"
        }
        elseif ($Result.CpuMsAvg -le 0 -or $Result.CpuMsAvg -gt $budget) {
            $out.Reasons += "density ${d}: cpu_ms_avg $($Result.CpuMsAvg) must be >0 and <= budget $budget"
        }
        elseif ($Result.GpuMsAvg -le 0 -or $Result.GpuMsAvg -gt $budget) {
            $out.Reasons += "density ${d}: gpu_ms_avg $($Result.GpuMsAvg) must be >0 and <= budget $budget"
        }
    }
    if ($out.Reasons.Count -gt 0) {
        $out.Status = 'FAIL'
        if (-not $out.TimingAvailable) { $out.Status = 'EVIDENCE_INCOMPLETE' }
    }
    return $out
}

function Invoke-PerformanceVerdict {
    param($Contract, $RunRecords)
    $out = @{ Verdict = 'PASS'; EnvStatus = 'PASS'; Reasons = @(); Runs = @(); EnvSummaries = @() }
    $envBad = $false; $perfFail = $false; $evidence = $false
    $infraRuns = @($RunRecords | Where-Object { $_.Status -ne 'OK' })
    foreach ($rec in $RunRecords) {
        $i = $rec.Index
        if ($rec.Status -ne 'OK') {
            $perfFail = $true
            $out.Reasons += "run ${i}: INFRA/BAD-EVIDENCE: $($rec.StatusReason)"
            $out.Runs += @{ Index = $i; Env = $null; Metrics = @() }
            continue
        }
        $env = Test-PerfEnvironment -Contract $Contract -Results $rec.Results
        $out.EnvSummaries += $env
        $metrics = @()
        foreach ($r in $rec.Results) {
            $m = Test-PerfMetrics -Contract $Contract -Result $r
            $metrics += $m
            if ($m.Status -eq 'EVIDENCE_INCOMPLETE') { $evidence = $true; $out.Reasons += "run ${i}: " + ($m.Reasons -join ' | ') }
            elseif ($m.Status -ne 'PASS') { $perfFail = $true; $out.Reasons += "run ${i}: " + ($m.Reasons -join ' | ') }
        }
        if ($env.Status -ne 'PASS') {
            $envBad = $true
            $out.Reasons += "run ${i}: ENV_NOT_MET: " + ($env.Reasons -join ' | ')
        }
        $out.Runs += @{ Index = $i; Env = $env; Metrics = $metrics }
    }
    if ($envBad) { $out.EnvStatus = 'ENV_NOT_MET' }
    # 裁决优先级：ENV_NOT_MET（无法评性能）> FAIL（真实测得超预算/无效测量）> EVIDENCE_INCOMPLETE > PASS；全 INFRA -> INFRA
    if ($infraRuns.Count -eq $RunRecords.Count) { $out.Verdict = 'INFRA' }
    elseif ($envBad) { $out.Verdict = 'ENV_NOT_MET' }
    elseif ($perfFail) { $out.Verdict = 'FAIL' }
    elseif ($evidence) { $out.Verdict = 'EVIDENCE_INCOMPLETE' }
    else { $out.Verdict = 'PASS' }
    return $out
}

# ---------------------------------------------------------------------
# S3-P5-ART-R7：Formal Art Performance（第五层）
# 硬件/环境/预算全部继承 PERFORMANCE_GATE.json；本节只描述 presentation workload。
# ---------------------------------------------------------------------

function Get-ArtProfile {
    param([string]$Path)
    $out = @{ Ok = $false; Reason = ''; Profile = $null }
    if ([string]::IsNullOrEmpty($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        $out.Reason = 'ART_PERFORMANCE_PROFILE.json missing'
        return $out
    }
    try { $p = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json }
    catch {
        $out.Reason = 'ART_PERFORMANCE_PROFILE.json not parseable: ' + $_.Exception.Message
        return $out
    }
    $knownSelection = @('DistinctMappedFormalVisuals')
    $knownAssignment = @('RoundRobinByEnemyKind')
    if ($p.schemaVersion -ne 1) { $out.Reason = "unsupported schemaVersion $($p.schemaVersion)"; return $out }
    if ([string]::IsNullOrEmpty($p.profileId)) { $out.Reason = 'profileId missing'; return $out }
    if ($p.baseContract -ne 'PERFORMANCE_GATE.json') { $out.Reason = "baseContract must be PERFORMANCE_GATE.json (got $($p.baseContract))"; return $out }
    if ($knownSelection -notcontains $p.selectionMode) { $out.Reason = "unknown selectionMode $($p.selectionMode)"; return $out }
    if ($knownAssignment -notcontains $p.assignmentMode) { $out.Reason = "unknown assignmentMode $($p.assignmentMode)"; return $out }
    if ($p.gameplayKind -ne 'Dummy') { $out.Reason = "gameplayKind must stay Dummy (got $($p.gameplayKind))"; return $out }
    if ($p.useEnemyVisualPresenter -ne $true) { $out.Reason = 'useEnemyVisualPresenter must be true'; return $out }
    $out.Ok = $true
    $out.Profile = $p
    return $out
}

function Read-ArtMetadata {
    param([string]$FilePath)
    $out = @{ Present = $false; ProfileId = ''; FormalVisuals = ''; VisualInstances = -1; VisualTypeCount = -1
        VisualMix = ''; ResolvedVisuals = ''; RendererInstances = -1; SkinnedRendererInstances = -1
        MaterialSlots = -1; ApproxVertices = -1; ApproxTriangles = -1 }
    if ([string]::IsNullOrEmpty($FilePath) -or -not (Test-Path -LiteralPath $FilePath -PathType Leaf)) { return $out }
    foreach ($line in (Get-Content -LiteralPath $FilePath)) {
        if ($line -match '^#\s+art_profile=(.*)$') { $out.ProfileId = $Matches[1].Trim(); $out.Present = $true; continue }
        if ($line -match '^#\s+formal_visuals=(.*)$') { $out.FormalVisuals = $Matches[1].Trim(); continue }
        if ($line -match '^#\s+visual_instances=(\d+)\s*$') { $out.VisualInstances = [int]$Matches[1]; continue }
        if ($line -match '^#\s+visual_type_count=(\d+)\s*$') { $out.VisualTypeCount = [int]$Matches[1]; continue }
        if ($line -match '^#\s+visual_mix=(.*)$') { $out.VisualMix = $Matches[1].Trim(); continue }
        if ($line -match '^#\s+resolved_visuals=(.*)$') { $out.ResolvedVisuals = $Matches[1].Trim(); continue }
        if ($line -match '^#\s+renderer_instances=(\d+)\s*$') { $out.RendererInstances = [int]$Matches[1]; continue }
        if ($line -match '^#\s+skinned_renderer_instances=(\d+)\s*$') { $out.SkinnedRendererInstances = [int]$Matches[1]; continue }
        if ($line -match '^#\s+material_slots=(\d+)\s*$') { $out.MaterialSlots = [int]$Matches[1]; continue }
        if ($line -match '^#\s+approx_vertices=(\d+)\s*$') { $out.ApproxVertices = [int]$Matches[1]; continue }
        if ($line -match '^#\s+approx_triangles=(\d+)\s*$') { $out.ApproxTriangles = [int]$Matches[1]; continue }
    }
    return $out
}

function Test-ArtEvidence {
    # §34：art 证据契约（formal_visuals=true / profile 匹配 / instances==density / mix 求和==density /
    # resolved 非空 / renderer>0 / skinned>0 / slots>0 / 无 fallback）。通过为 @()，问题为原因列表。
    param($Profile, $Metadata, [int]$Density)
    $reasons = @()
    if (-not $Metadata.Present) { return @('art metadata headers missing (art mode did not run)') }
    if ($Metadata.ProfileId -ne $Profile.profileId) { $reasons += "art_profile '$($Metadata.ProfileId)' != profile '$($Profile.profileId)'" }
    if ($Metadata.FormalVisuals -ne 'true') { $reasons += "formal_visuals='$($Metadata.FormalVisuals)' (load-fail/fallback not allowed in art evidence)" }
    if ($Metadata.VisualInstances -ne $Density) { $reasons += "visual_instances $($Metadata.VisualInstances) != density $Density" }
    if ($Metadata.VisualTypeCount -lt 1) { $reasons += "visual_type_count $($Metadata.VisualTypeCount) < 1 (resolved set empty)" }
    $mixSum = 0
    $mixNames = @()
    if (-not [string]::IsNullOrEmpty($Metadata.VisualMix)) {
        foreach ($kv in $Metadata.VisualMix.Split(';')) {
            $parts = $kv.Split('=')
            if ($parts.Count -eq 2) {
                $n = [int]0
                if ([int]::TryParse($parts[1], [ref]$n)) { $mixSum += $n; $mixNames += $parts[0] }
            }
        }
    }
    if ($mixSum -ne $Density) { $reasons += "visual_mix sum $mixSum != density $Density" }
    if ($mixNames.Count -ne $Metadata.VisualTypeCount) { $reasons += "visual_mix entries $($mixNames.Count) != visual_type_count $($Metadata.VisualTypeCount)" }
    if ([string]::IsNullOrEmpty($Metadata.ResolvedVisuals)) { $reasons += 'resolved_visuals empty' }
    if ($Metadata.RendererInstances -lt 1) { $reasons += "renderer_instances $($Metadata.RendererInstances) < 1" }
    if ($Metadata.SkinnedRendererInstances -lt 1) { $reasons += "skinned_renderer_instances $($Metadata.SkinnedRendererInstances) < 1" }
    if ($Metadata.MaterialSlots -lt 1) { $reasons += "material_slots $($Metadata.MaterialSlots) < 1" }
    return $reasons
}

function Invoke-ArtPerformanceVerdict {
    # 复用 canonical 指标评估器（不复制 8.33ms 逻辑，§33），叠加 art 证据契约校验（§34）。
    # 裁决优先级同 canonical：ENV_NOT_MET > FAIL > EVIDENCE_INCOMPLETE > PASS；全 INFRA -> INFRA。
    param($Contract, $RunRecords, $Profile)
    $metric = Invoke-PerformanceVerdict -Contract $Contract -RunRecords $RunRecords
    $out = @{ Verdict = $metric.Verdict; EnvStatus = $metric.EnvStatus; Reasons = @($metric.Reasons)
        Runs = $metric.Runs; EnvSummaries = $metric.EnvSummaries; ArtReasons = @() }
    $evidenceBad = $false
    $infraCount = 0
    foreach ($rec in $RunRecords) {
        $i = $rec.Index
        if ($rec.Status -ne 'OK') { $infraCount++; continue }
        foreach ($r in $rec.Results) {
            $meta = Read-ArtMetadata -FilePath $r.SourcePath
            $bad = Test-ArtEvidence -Profile $Profile -Metadata $meta -Density $r.Density
            if ($bad.Count -gt 0) {
                $evidenceBad = $true
                foreach ($b in $bad) { $out.Reasons += "run ${i} density $($r.Density): ART-EVIDENCE: $b" }
            }
        }
    }
    if ($evidenceBad) {
        if ($metric.Verdict -eq 'PASS') { $out.Verdict = 'EVIDENCE_INCOMPLETE' }
        elseif ($metric.Verdict -eq 'ENV_NOT_MET') { $out.Verdict = 'ENV_NOT_MET' }
        # FAIL 优先级更高，保持 FAIL
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
    $Data | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding UTF8
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

    # ArenaPerfHarness 证据解析夹具（S3-M6/M7）：schema 与 ArenaPerfHarness.WriteRow 对齐
    $CsvHeader = 'dummy_count,alive,frames,main_ms_avg,main_ms_p95,main_ms_p99,main_ms_p999,main_ms_max,gc_alloc_bytes_avg,cpu_ms_avg,gpu_ms_avg,mem_total_mb,frame_timing_ok'
    function Write-HarnessFixture {
        param([string]$Dir, [int]$Density, [string]$Meta, [string]$Data, [string[]]$ExtraMeta = @())
        $all = "# ArenaPerfHarness, density=$Density`n" + $Meta
        foreach ($x in $ExtraMeta) { $all += "`n" + $x }
        $all += "`n# density strategy: kill-then-refill`n" + $CsvHeader + "`n" + $Data + "`n"
        Set-Content -LiteralPath (Join-Path $Dir "$Density.txt") -Encoding UTF8 -Value $all
    }
    $GoodMeta = '# resolution=1920x1080 fullscreen=True currentRes=1920x1080 editor=False dx=Direct3D12'
    $pr = Join-Path $fx 'playerrun'
    $good = Join-Path $pr 'good'
    New-Item -ItemType Directory -Path $good -Force | Out-Null
    Write-HarnessFixture -Dir $good -Density 100 -Meta $GoodMeta -Data '100,100,600,2.500,2.600,2.700,2.800,3.000,0.0,2.400,0.300,512.0,true'
    Write-HarnessFixture -Dir $good -Density 200 -Meta $GoodMeta -Data '200,200,600,4.100,4.300,4.500,4.700,5.000,0.0,4.000,0.500,512.0,true'
    Write-HarnessFixture -Dir $good -Density 300 -Meta $GoodMeta -Data '300,294,600,6.000,6.400,6.600,6.900,7.500,0.0,5.800,0.800,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $good -ExpectedDensities @(100, 200, 300)
    $ok = $e.Valid -and $e.ActualResolution -eq '1920x1080' -and $e.GraphicsApi -eq 'Direct3D12' -and $e.Results[2].Alive -eq 294
    $results.Add("selftest.playerrun-pass: $(@('FAIL', 'PASS')[[int]$ok])")

    $missing = Join-Path $pr 'missing'
    New-Item -ItemType Directory -Path $missing -Force | Out-Null
    Write-HarnessFixture -Dir $missing -Density 100 -Meta $GoodMeta -Data '100,100,600,2.5,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $missing -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $missing -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'missing file')
    $results.Add("selftest.playerrun-missing-density: $(@('FAIL', 'PASS')[[int]$ok])")

    $mismatch = Join-Path $pr 'mismatch'
    New-Item -ItemType Directory -Path $mismatch -Force | Out-Null
    Write-HarnessFixture -Dir $mismatch -Density 100 -Meta $GoodMeta -Data '200,200,600,2.5,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $mismatch -Density 200 -Meta $GoodMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $mismatch -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $mismatch -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'expected density 100')
    $results.Add("selftest.playerrun-density-mismatch: $(@('FAIL', 'PASS')[[int]$ok])")

    $malformed = Join-Path $pr 'malformed'
    New-Item -ItemType Directory -Path $malformed -Force | Out-Null
    Write-HarnessFixture -Dir $malformed -Density 100 -Meta $GoodMeta -Data '100,100,600'
    Write-HarnessFixture -Dir $malformed -Density 200 -Meta $GoodMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $malformed -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $malformed -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'column count')
    $results.Add("selftest.playerrun-malformed-csv: $(@('FAIL', 'PASS')[[int]$ok])")

    $nan = Join-Path $pr 'nan'
    New-Item -ItemType Directory -Path $nan -Force | Out-Null
    Write-HarnessFixture -Dir $nan -Density 100 -Meta $GoodMeta -Data '100,100,600,NaN,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $nan -Density 200 -Meta $GoodMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $nan -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $nan -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'NaN/Infinity')
    $results.Add("selftest.playerrun-invalid-number: $(@('FAIL', 'PASS')[[int]$ok])")

    $editorTrue = Join-Path $pr 'editortrue'
    New-Item -ItemType Directory -Path $editorTrue -Force | Out-Null
    Write-HarnessFixture -Dir $editorTrue -Density 100 -Meta ($GoodMeta -replace 'editor=False', 'editor=True') -Data '100,100,600,2.5,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $editorTrue -Density 200 -Meta $GoodMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $editorTrue -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $editorTrue -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'editor=True')
    $results.Add("selftest.playerrun-editor-true: $(@('FAIL', 'PASS')[[int]$ok])")

    $zeroAlive = Join-Path $pr 'zeroalive'
    New-Item -ItemType Directory -Path $zeroAlive -Force | Out-Null
    Write-HarnessFixture -Dir $zeroAlive -Density 100 -Meta $GoodMeta -Data '100,0,600,2.5,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $zeroAlive -Density 200 -Meta $GoodMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $zeroAlive -Density 300 -Meta $GoodMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $zeroAlive -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'alive 0')
    $results.Add("selftest.playerrun-zero-alive: $(@('FAIL', 'PASS')[[int]$ok])")

    $badRes = Join-Path $pr 'badres'
    New-Item -ItemType Directory -Path $badRes -Force | Out-Null
    $badMeta = '# resolution=0x0 fullscreen=True currentRes=0x0 editor=False dx=Direct3D12'
    Write-HarnessFixture -Dir $badRes -Density 100 -Meta $badMeta -Data '100,100,600,2.5,2.6,2.7,2.8,3.0,0.0,2.4,0.3,512.0,true'
    Write-HarnessFixture -Dir $badRes -Density 200 -Meta $badMeta -Data '200,200,600,4.1,4.3,4.5,4.7,5.0,0.0,4.0,0.5,512.0,true'
    Write-HarnessFixture -Dir $badRes -Density 300 -Meta $badMeta -Data '300,300,600,6.0,6.4,6.6,6.9,7.5,0.0,5.8,0.8,512.0,true'
    $e = Test-PlayerRunEvidence -OutputDir $badRes -ExpectedDensities @(100, 200, 300)
    $ok = (-not $e.Valid) -and (($e.Reasons -join ' ') -match 'invalid resolution')
    $results.Add("selftest.playerrun-invalid-resolution: $(@('FAIL', 'PASS')[[int]$ok])")

    # Performance contract + evaluator 夹具（S3-M7）：contract 单一来源=docs/qa/PERFORMANCE_GATE.json
    # SelfTest 只校验 schema 完整性（硬件锁定状态由 canonical Gate 检查）；评估夹具硬件用合成值
    $contractFile = Join-Path $ProjectRoot 'docs\qa\PERFORMANCE_GATE.json'
    $CT = $null
    try {
        $CT = Get-Content -LiteralPath $contractFile -Raw | ConvertFrom-Json
        $missingFields = @()
        foreach ($field in @('resolution', 'fullscreen', 'graphicsApi', 'quality', 'vSyncCount', 'targetFrameRate', 'densities', 'warmupFrames', 'sampleFrames', 'castIntervalSeconds', 'frameBudgetMs', 'requiredRuns', 'requireFrameTiming', 'minAliveRatio', 'hardware')) {
            if ($null -eq $CT.$field) { $missingFields += $field }
        }
        if ($missingFields.Count -gt 0) { $CT = $null }
    } catch { $CT = $null }
    $results.Add("selftest.perf-contract-schema: $(@('FAIL', 'PASS')[[int]($null -ne $CT)])")
    if ($null -ne $CT) {
        if ("$($CT.hardware.processorType)" -match 'PENDING_HARDWARE_PROBE') { $CT.hardware.processorType = 'SelfTest CPU (synthetic)' }
        if ("$($CT.hardware.graphicsDeviceName)" -match 'PENDING_HARDWARE_PROBE') { $CT.hardware.graphicsDeviceName = 'SelfTest GPU (synthetic)' }
        function New-PerfResult {
            param($CT, [int]$Density, [string]$Resolution, [string]$Api, [string]$Quality, [string]$VSync, [string]$TFps, [string]$Cpu, [string]$Gpu, [double]$Avg, [double]$P99, [double]$CpuAvg, [double]$GpuAvg, [string]$Fto, [int]$Alive, [int]$Frames)
            return @{ Valid = $true; Density = $Density; Resolution = $Resolution; Fullscreen = 'True'; Editor = 'False'; GraphicsApi = $Api
                HardwareCpu = $Cpu; HardwareGpu = $Gpu; Quality = $Quality; VSync = $VSync; TargetFps = $TFps
                Warmup = $CT.warmupFrames; Sample = $CT.sampleFrames; CastInterval = "$($CT.castIntervalSeconds)"
                Alive = $Alive; Frames = $Frames; MainMsAvg = $Avg; MainMsP99 = $P99; CpuMsAvg = $CpuAvg; GpuMsAvg = $GpuAvg
                GcBytesAvg = 0.0; MemTotalMb = 512.0; FrameTimingOk = $Fto; Reason = '' }
        }
        function New-PerfRun {
            param($CT, [string]$Resolution, [string]$Api, [string]$Quality, [string]$VSync, [string]$TFps, [string]$Cpu, [string]$Gpu, [double]$AvgMul, [double]$P99Mul, [string]$Fto)
            $rs = @()
            foreach ($d in $CT.densities) {
                $rs += New-PerfResult -CT $CT -Density $d -Resolution $Resolution -Api $Api -Quality $Quality -VSync $VSync -TFps $TFps -Cpu $Cpu -Gpu $Gpu -Avg ($CT.frameBudgetMs * $AvgMul) -P99 ($CT.frameBudgetMs * $P99Mul) -CpuAvg ($CT.frameBudgetMs * 0.2) -GpuAvg ($CT.frameBudgetMs * 0.1) -Fto $Fto -Alive ([int][math]::Ceiling($d * $CT.minAliveRatio)) -Frames $CT.sampleFrames
            }
            return $rs
        }
        function New-PerfRecords {
            param($CT, [string]$Resolution, [string]$Api, [string]$Quality, [string]$VSync, [string]$TFps, [string]$Cpu, [string]$Gpu, [double]$AvgMul, [double]$P99Mul, [string]$Fto)
            $recs = @()
            for ($i = 1; $i -le $CT.requiredRuns; $i++) {
                $recs += @{ Index = $i; Status = 'OK'; StatusReason = ''; Results = (New-PerfRun -CT $CT -Resolution $Resolution -Api $Api -Quality $Quality -VSync $VSync -TFps $TFps -Cpu $Cpu -Gpu $Gpu -AvgMul $AvgMul -P99Mul $P99Mul -Fto $Fto) }
            }
            return $recs
        }
        $HW = @{ processorType = "$($CT.hardware.processorType)"; graphicsDeviceName = "$($CT.hardware.graphicsDeviceName)" }
        $okRes = "$($CT.resolution.width)x$($CT.resolution.height)"

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-pass: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'PASS')])")

        $recs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $recs[0].Results[2].MainMsP99 = $CT.frameBudgetMs + 0.01
        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords $recs
        $results.Add("selftest.perf-p99-fail: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        $recs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $recs[1].Results[0].MainMsAvg = $CT.frameBudgetMs + 0.01
        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords $recs
        $results.Add("selftest.perf-avg-fail: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        $recs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $recs[2].Results[1].GpuMsAvg = $CT.frameBudgetMs + 0.01
        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords $recs
        $results.Add("selftest.perf-gpu-fail: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution '1920x1080' -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-resolution-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api 'D3D11' -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-api-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality 'Mobile' -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-quality-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync '1' -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-vsync-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps '60' -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-fps-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu 'Different CPU' -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true')
        $results.Add("selftest.perf-hardware-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'false')
        $results.Add("selftest.perf-timing-unavailable: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'EVIDENCE_INCOMPLETE')])")

        $recs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $d300 = $CT.densities[2]
        $recs[0].Results[2].Alive = [int][math]::Ceiling($d300 * $CT.minAliveRatio) - 1
        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords $recs
        $results.Add("selftest.perf-alive-invalid: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        $recs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $recs[2].Results[1].Frames = $CT.sampleFrames - 1
        $v = Invoke-PerformanceVerdict -Contract $CT -RunRecords $recs
        $results.Add("selftest.perf-frames-invalid: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        # 文件级端到端：硬件/perf_env 行真实捕获
        $perfFileDir = Join-Path $fx 'perffile'
        New-Item -ItemType Directory -Path $perfFileDir -Force | Out-Null
        Write-HarnessFixture -Dir $perfFileDir -Density 100 -Meta $GoodMeta -Data '100,100,600,2.500,2.600,2.700,2.800,3.000,0.0,2.400,0.300,512.0,true' -ExtraMeta @("# hardware_cpu=$($HW.processorType)", "# hardware_gpu=$($HW.graphicsDeviceName)", "# perf_env quality=$($CT.quality) vsync=$($CT.vSyncCount) targetFps=$($CT.targetFrameRate) warmup=$($CT.warmupFrames) sample=$($CT.sampleFrames) castInterval=$($CT.castIntervalSeconds)")
        $r = Read-HarnessResult -FilePath (Join-Path $perfFileDir '100.txt') -ExpectedDensity 100
        $ok = $r.Valid -and $r.HardwareCpu -eq $HW.processorType -and $r.Quality -eq $CT.quality -and $r.VSync -eq "$($CT.vSyncCount)" -and $r.TargetFps -eq "$($CT.targetFrameRate)" -and $r.Warmup -eq $CT.warmupFrames -and $r.Sample -eq $CT.sampleFrames -and $r.CpuMsAvg -gt 0
        $results.Add("selftest.perf-harness-metadata-capture: $(@('FAIL', 'PASS')[[int]$ok])")

        # --- S3-P5-ART-R7：Art Performance 纯合成用例（§35，无需 Unity） ---
        $artProfilePath = Join-Path $ProjectRoot 'docs\qa\ART_PERFORMANCE_PROFILE.json'
        $ap = Get-ArtProfile -Path $artProfilePath
        $results.Add("selftest.art-profile-parse: $(@('FAIL', 'PASS')[[int]$ap.Ok])")
        $results.Add("selftest.art-profile-selection-known: $(@('FAIL', 'PASS')[[int]($ap.Ok -and $ap.Profile.selectionMode -eq 'DistinctMappedFormalVisuals')])")
        $artProfileObj = $ap.Profile

        # resolved visual 排序确定性（文档内声明三套正式视觉的机器可读期望，PowerShell 不复制路径 truth）
        $expectedOrder = @('TrollWarriorVisual', 'FireLionVisual', 'BruceVisual')

        function New-ArtRow {
            param([string]$Dir, [int]$Density, [string]$Avg = '2.500', [string]$P99 = '2.700', [string]$Cpu = '2.400', [string]$Gpu = '0.300',
                [int]$VisualInstances = -1, [string]$Mix = '', [int]$TypeCount = 3, [string]$Formal = 'true',
                [string]$Resolved = 'TrollWarriorVisual;FireLionVisual;BruceVisual', [int]$Renderers = 120, [int]$Skinned = 100, [int]$Slots = 130)
            if ($VisualInstances -lt 0) { $VisualInstances = $Density }
            if ([string]::IsNullOrEmpty($Mix)) {
                # round-robin 期望混合（确定性）
                $a = [int][math]::Ceiling($Density / 3); $b = [int][math]::Floor($Density / 3)
                if ($Density % 3 -eq 0) { $Mix = "TrollWarriorVisual=$b;FireLionVisual=$b;BruceVisual=$b" }
                elseif ($Density % 3 -eq 1) { $Mix = "TrollWarriorVisual=$a;FireLionVisual=$b;BruceVisual=$b" }
                else { $Mix = "TrollWarriorVisual=$a;FireLionVisual=$a;BruceVisual=$b" }
            }
            $artRes = "$($CT.resolution.width)x$($CT.resolution.height)"
            $meta = @(
                "# art_profile=formal-enemy-visual-stress-v1",
                "# formal_visuals=$Formal",
                "# visual_instances=$VisualInstances",
                "# visual_type_count=$TypeCount",
                "# visual_mix=$Mix",
                "# resolved_visuals=$Resolved",
                "# renderer_instances=$Renderers",
                "# skinned_renderer_instances=$Skinned",
                "# material_slots=$Slots",
                "# approx_vertices=1000",
                "# approx_triangles=800"
            )
            Write-HarnessFixture -Dir $Dir -Density $Density -Meta "# resolution=$artRes fullscreen=True currentRes=$artRes editor=False dx=Direct3D12" -Data ("{0},{0},600,$Avg,2.600,$P99,2.800,3.000,0.0,$Cpu,$Gpu,512.0,true" -f $Density) -ExtraMeta ($meta + @("# hardware_cpu=$($HW.processorType)", "# hardware_gpu=$($HW.graphicsDeviceName)", "# perf_env quality=$($CT.quality) vsync=$($CT.vSyncCount) targetFps=$($CT.targetFrameRate) warmup=$($CT.warmupFrames) sample=$($CT.sampleFrames) castInterval=$($CT.castIntervalSeconds)"))
        }

        function New-ArtRecords {
            param([string]$Dir, [string]$Avg = '2.500', [string]$P99 = '2.700', [string]$Cpu = '2.400', [string]$Gpu = '0.300',
                [int]$BadVisualInstances = -1, [string]$MixOverride = '', [string]$FormalOverride = 'true', [string]$ResolvedOverride = 'TrollWarriorVisual;FireLionVisual;BruceVisual')
            $recs = @()
            for ($runIndex = 1; $runIndex -le $CT.requiredRuns; $runIndex++) {
                $runDir = Join-Path $Dir "Run$runIndex"
                New-Item -ItemType Directory -Path $runDir -Force | Out-Null
                $res = @()
                foreach ($density in $CT.densities) {
                    $vi = $BadVisualInstances; $mix = $MixOverride; $formal = $FormalOverride; $resolved = $ResolvedOverride
                    New-ArtRow -Dir $runDir -Density $density -Avg $Avg -P99 $P99 -Cpu $Cpu -Gpu $Gpu -VisualInstances $vi -Mix $mix -Formal $formal -Resolved $resolved
                    $res += Read-HarnessResult -FilePath (Join-Path $runDir "$density.txt") -ExpectedDensity $density
                }
                $recs += @{ Index = $runIndex; Status = 'OK'; StatusReason = ''; Results = $res }
            }
            return $recs
        }

        $artDirFx = Join-Path $fx 'artperf'
        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'valid')
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-3x3-valid: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'PASS')])")

        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'inst299') -BadVisualInstances 299
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-instances-299: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'EVIDENCE_INCOMPLETE')])")

        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'mix299') -MixOverride 'TrollWarriorVisual=100;FireLionVisual=100;BruceVisual=99'
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-mix-sum-299: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'EVIDENCE_INCOMPLETE')])")

        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'fallback') -FormalOverride 'false(load-failed-primitive-fallback)'
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-load-fallback: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'EVIDENCE_INCOMPLETE')])")

        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'avg-fail') -Avg '8.34' -P99 '8.34'
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-avg-over-budget: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        $recs = New-ArtRecords -Dir (Join-Path $artDirFx 'gpu-fail') -Gpu '8.34'
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-gpu-over-budget: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'FAIL')])")

        # canonical PASS + art PASS -> overall PASS；canonical PASS + art FAIL -> overall FAIL（exit 非 0 语义）
        $canonRecs = New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $canonV = Invoke-PerformanceVerdict -Contract $CT -RunRecords $canonRecs
        $artGood = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords (New-ArtRecords -Dir (Join-Path $artDirFx 'combo-good')) -Profile $artProfileObj
        $ok = $canonV.Verdict -eq 'PASS' -and $artGood.Verdict -eq 'PASS'
        $results.Add("selftest.art-combo-both-pass: $(@('FAIL', 'PASS')[[int]$ok])")
        $artBad = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords (New-ArtRecords -Dir (Join-Path $artDirFx 'combo-bad') -Avg '8.34' -P99 '8.34') -Profile $artProfileObj
        $ok = $canonV.Verdict -eq 'PASS' -and $artBad.Verdict -eq 'FAIL'
        $results.Add("selftest.art-combo-art-fail: $(@('FAIL', 'PASS')[[int]$ok])")
        $canonBad = Invoke-PerformanceVerdict -Contract $CT -RunRecords (New-PerfRecords -CT $CT -Resolution $okRes -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 1.2 -P99Mul 1.2 -Fto 'true')
        $ok = $canonBad.Verdict -eq 'FAIL' -and $artGood.Verdict -eq 'PASS'
        $results.Add("selftest.art-combo-canonical-fail: $(@('FAIL', 'PASS')[[int]$ok])")

        # 环境不满足 -> ENV_NOT_MET（分辨率错档）
        $recs = New-PerfRecords -CT $CT -Resolution '1920x1080' -Api $CT.graphicsApi -Quality $CT.quality -VSync "$($CT.vSyncCount)" -TFps "$($CT.targetFrameRate)" -Cpu $HW.processorType -Gpu $HW.graphicsDeviceName -AvgMul 0.3 -P99Mul 0.5 -Fto 'true'
        $v = Invoke-ArtPerformanceVerdict -Contract $CT -RunRecords $recs -Profile $artProfileObj
        $results.Add("selftest.art-env-mismatch: $(@('FAIL', 'PASS')[[int]($v.Verdict -eq 'ENV_NOT_MET')])")

        # resolved 顺序确定性：同一 profile 两次解析一致
        $ap2 = Get-ArtProfile -Path $artProfilePath
        $detOk = $false
        if ($ap.Ok -and $ap2.Ok) {
            $detOk = ($ap.Profile.profileId -eq $ap2.Profile.profileId) -and ($ap.Profile.selectionMode -eq $ap2.Profile.selectionMode)
        }
        $results.Add("selftest.art-resolved-deterministic: $(@('FAIL', 'PASS')[[int]$detOk])")

        # profile parser 拒绝未知 selectionMode
        $badProfileDir = Join-Path $fx 'artprofile-bad'
        New-Item -ItemType Directory -Path $badProfileDir -Force | Out-Null
        $badJson = ($artProfileObj | ConvertTo-Json -Depth 4) -replace 'DistinctMappedFormalVisuals', 'ActualMapMix'
        $badPath = Join-Path $badProfileDir 'ART_PERFORMANCE_PROFILE.json'
        Set-Content -LiteralPath $badPath -Value $badJson -Encoding UTF8
        $apBad = Get-ArtProfile -Path $badPath
        $results.Add("selftest.art-profile-rejects-unknown-selection: $(@('FAIL', 'PASS')[[int](-not $apBad.Ok)])")
    }

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

# 2) Editor lock: fail fast if a live editor holds the project; clean stale locks (no Unity process alive)
if (Test-Path -LiteralPath $LockFile -PathType Leaf) {
    $liveUnity = @(Get-Process -Name Unity -ErrorAction SilentlyContinue)
    if ($liveUnity.Count -gt 0) {
        Write-Output "Gate: FAIL"
        Write-Output "Reason: PROJECT_ALREADY_OPEN"
        Write-Output "Lock file: $LockFile"
        Write-Output "Close the running Unity Editor for this project, then rerun the gate."
        exit 3
    }
    # 崩溃/强杀残留的僵死锁：无 Unity 进程存活时安全移除（不杀任何进程；有进程一律 fail fast）
    Write-Output "Note: stale UnityLockfile with no live Unity process; removing stale lock."
    Remove-Item -LiteralPath $LockFile -Force
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
    $playInfra = $false   # 跳过≠基础设施失败；显式赋值防 $null 绑定 [bool] 参数报错
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

# 7) Player Runtime (Player Runtime Gate only): 启动本轮刚构建的 Player 跑 -arenaPerf
$playerRunStatus = 'SKIPPED'
$playerRunRun = $null
$playerRunEvidence = $null
$playerRunDir = Join-Path $TempRoot 'PlayerRun'
$playerRunLog = Join-Path $TempRoot 'PlayerRun.log'
if ($IncludePerformance) {
    # Performance Gate 层接管：以锁定环境连续 3 次运行替代单次普通 Player Run（不做第 4 次）
    $playerRunStatus = 'SKIPPED'
}
elseif (-not $IncludePlayerRun) {
    # 未请求：不运行，也不谎称已验证 Player Runtime
}
elseif ($buildStatus -ne 'PASS') {
    $playerRunStatus = 'NOT_RUN'
    $Issues.Add('PlayerRun NOT RUN / INFRA BLOCKED (build did not produce a valid player)')
}
else {
    New-Item -ItemType Directory -Path $playerRunDir -Force | Out-Null
    Get-ChildItem -LiteralPath $playerRunDir -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
    $playerRunRun = Invoke-UnityChild -UnityExe $buildExe -ArgumentString "-arenaPerf -arenaPerfOut `"$playerRunDir`" -logFile `"$playerRunLog`"" -TimeoutMin $PlayerRunTimeoutMinutes
    $playerRunEvidence = Test-PlayerRunEvidence -OutputDir $playerRunDir -ExpectedDensities @(100, 200, 300)
    if ($playerRunRun.TimedOut) {
        $playerRunStatus = 'FAIL'
        $Issues.Add('PlayerRun TIMEOUT (gate killed only its own child process)')
    }
    elseif ($playerRunRun.ExitCode -ne 0) {
        $playerRunStatus = 'FAIL'
        $Issues.Add("PlayerRun exit code $($playerRunRun.ExitCode)")
    }
    elseif (-not $playerRunEvidence.Valid) {
        $playerRunStatus = 'FAIL'
        $Issues.Add('PlayerRun evidence invalid: ' + ($playerRunEvidence.Reasons -join ' | '))
    }
    else {
        $playerRunStatus = 'PASS'
    }
}
$playerRunOk = (-not $IncludePlayerRun) -or $IncludePerformance -or ($playerRunStatus -eq 'PASS')

# 8) Performance Gate (-IncludePerformance)：锁定环境 + 同一 Build 连续 3 次运行 + 硬预算
$perfStatus = 'NOT_EVALUATED'
$perfContract = $null
$perfVerdict = $null
$perfEnvSummary = $null
if ($IncludePerformance) {
    $perfDir = Join-Path $TempRoot 'Performance'
    $perfContractPath = Join-Path $ProjectRoot 'docs\qa\PERFORMANCE_GATE.json'
    $c = Get-PerformanceContract -Path $perfContractPath
    if (-not $c.Ok) {
        $perfStatus = 'INFRA'
        $Issues.Add('Performance INFRA: ' + $c.Reason)
    }
    elseif ($buildStatus -ne 'PASS') {
        $perfStatus = 'INFRA'
        $Issues.Add('Performance INFRA: build did not produce a valid player')
    }
    else {
        $perfContract = $c.Contract
        New-Item -ItemType Directory -Path $perfDir -Force | Out-Null
        $perfRunRecords = @()
        for ($runIndex = 1; $runIndex -le [int]$perfContract.requiredRuns; $runIndex++) {
            $runDir = Join-Path $perfDir "Run$runIndex"
            New-Item -ItemType Directory -Path $runDir -Force | Out-Null
            Get-ChildItem -LiteralPath $runDir -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
            $runLog = Join-Path $runDir 'PlayerRun.log'
            $rec = @{ Index = $runIndex; Status = 'OK'; StatusReason = ''; Results = @() }
            $perfRun = Invoke-UnityChild -UnityExe $buildExe -ArgumentString "-screen-width $($perfContract.resolution.width) -screen-height $($perfContract.resolution.height) -screen-fullscreen 1 -screen-quality $($perfContract.quality) -force-d3d12 -arenaPerf -arenaPerfGate -arenaPerfOut `"$runDir`" -logFile `"$runLog`"" -TimeoutMin $PlayerRunTimeoutMinutes
            if ($perfRun.TimedOut) {
                $rec.Status = 'INFRA'
                $rec.StatusReason = 'TIMEOUT (gate killed only its own child process)'
                $Issues.Add("Performance run $runIndex TIMEOUT")
            }
            elseif ($perfRun.ExitCode -ne 0) {
                $rec.Status = 'INFRA'
                $rec.StatusReason = "player exit code $($perfRun.ExitCode)"
                $Issues.Add("Performance run $runIndex player exit $($perfRun.ExitCode)")
            }
            else {
                $runResults = @()
                $badEvidence = @()
                foreach ($density in $perfContract.densities) {
                    $r = Read-HarnessResult -FilePath (Join-Path $runDir "$density.txt") -ExpectedDensity $density
                    $runResults += $r
                    if (-not $r.Valid) { $badEvidence += "density ${density}: $($r.Reason)" }
                }
                if ($badEvidence.Count -gt 0) {
                    $rec.Status = 'INFRA'
                    $rec.StatusReason = 'evidence missing/invalid in the specified output dir (fallback elsewhere does not count): ' + ($badEvidence -join ' | ')
                    $Issues.Add("Performance run $runIndex bad evidence")
                }
                else {
                    $rec.Results = $runResults
                }
            }
            $perfRunRecords += $rec
        }
        $okRecords = @($perfRunRecords | Where-Object { $_.Status -eq 'OK' })
        if ($okRecords.Count -eq 0) {
            $perfStatus = 'INFRA'
        }
        else {
            $perfVerdict = Invoke-PerformanceVerdict -Contract $perfContract -RunRecords $perfRunRecords
            $perfStatus = $perfVerdict.Verdict
            $envOk = @($perfVerdict.EnvSummaries | Where-Object { $_.Status -eq 'PASS' })
            if ($envOk.Count -gt 0) { $perfEnvSummary = $envOk[0] }
            foreach ($reason in $perfVerdict.Reasons) { $Issues.Add("Performance: $reason") }
        }
    }
}
$perfOk = (-not $IncludePerformance) -or ($perfStatus -eq 'PASS')

# 8b) Art Performance Gate（-IncludeArtPerformance）：同一 Build 连续 3 次 -arenaArtVisuals 运行 + 硬预算继承 + art 证据契约
$artStatus = 'NOT_EVALUATED'
$artContract = $null
$artVerdict = $null
$artProfile = $null
$artEnvSummary = $null
if ($IncludeArtPerformance) {
    $artProfilePath = Join-Path $ProjectRoot 'docs\qa\ART_PERFORMANCE_PROFILE.json'
    $ap = Get-ArtProfile -Path $artProfilePath
    if (-not $ap.Ok) {
        $artStatus = 'INFRA'
        $Issues.Add('ArtPerformance INFRA: ' + $ap.Reason)
    }
    elseif ($perfStatus -ne 'PASS') {
        # §6：只有 canonical PerformanceVerdict=PASS 才评估 art 层
        $artStatus = 'NOT_EVALUATED'
        $Issues.Add('ArtPerformance NOT_EVALUATED (canonical performance not PASS: ' + $perfStatus + ')')
    }
    else {
        $artContract = $perfContract
        $artProfile = $ap.Profile
        $artDir = Join-Path $TempRoot 'ArtPerformance'
        New-Item -ItemType Directory -Path $artDir -Force | Out-Null
        $artRunRecords = @()
        for ($runIndex = 1; $runIndex -le [int]$artContract.requiredRuns; $runIndex++) {
            $runDir = Join-Path $artDir "Run$runIndex"
            New-Item -ItemType Directory -Path $runDir -Force | Out-Null
            Get-ChildItem -LiteralPath $runDir -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
            $runLog = Join-Path $runDir 'PlayerRun.log'
            $rec = @{ Index = $runIndex; Status = 'OK'; StatusReason = ''; Results = @() }
            $artRun = Invoke-UnityChild -UnityExe $buildExe -ArgumentString "-screen-width $($artContract.resolution.width) -screen-height $($artContract.resolution.height) -screen-fullscreen 1 -screen-quality $($artContract.quality) -force-d3d12 -arenaPerf -arenaPerfGate -arenaArtVisuals -arenaPerfOut `"$runDir`" -logFile `"$runLog`"" -TimeoutMin $PlayerRunTimeoutMinutes
            if ($artRun.TimedOut) {
                $rec.Status = 'INFRA'
                $rec.StatusReason = 'TIMEOUT (gate killed only its own child process)'
                $Issues.Add("ArtPerformance run $runIndex TIMEOUT")
            }
            elseif ($artRun.ExitCode -ne 0) {
                $rec.Status = 'INFRA'
                $rec.StatusReason = "player exit code $($artRun.ExitCode)"
                $Issues.Add("ArtPerformance run $runIndex player exit $($artRun.ExitCode)")
            }
            else {
                $runResults = @()
                $badEvidence = @()
                foreach ($density in $artContract.densities) {
                    $r = Read-HarnessResult -FilePath (Join-Path $runDir "$density.txt") -ExpectedDensity $density
                    $runResults += $r
                    if (-not $r.Valid) { $badEvidence += "density ${density}: $($r.Reason)" }
                }
                if ($badEvidence.Count -gt 0) {
                    $rec.Status = 'INFRA'
                    $rec.StatusReason = 'evidence missing/invalid in the specified output dir (fallback elsewhere does not count): ' + ($badEvidence -join ' | ')
                    $Issues.Add("ArtPerformance run $runIndex bad evidence")
                }
                else {
                    $rec.Results = $runResults
                }
            }
            $artRunRecords += $rec
        }
        $okRecords = @($artRunRecords | Where-Object { $_.Status -eq 'OK' })
        if ($okRecords.Count -eq 0) {
            $artStatus = 'INFRA'
        }
        else {
            $artVerdict = Invoke-ArtPerformanceVerdict -Contract $artContract -RunRecords $artRunRecords -Profile $artProfile
            $artStatus = $artVerdict.Verdict
            $envOk = @($artVerdict.EnvSummaries | Where-Object { $_.Status -eq 'PASS' })
            if ($envOk.Count -gt 0) { $artEnvSummary = $envOk[0] }
            foreach ($reason in $artVerdict.Reasons) { $Issues.Add("ArtPerformance: $reason") }
        }
    }
}
$artOk = (-not $IncludeArtPerformance) -or ($artStatus -eq 'PASS')

# 9) Finalize once: unified verdict + summary + exit code
$editOk = (-not $editInfra) -and $editResult.Result -eq 'Passed' -and $editResult.Failed -eq 0
$playOk = (-not $playInfra) -and $null -ne $playResult -and $playResult.Result -eq 'Passed' -and $playResult.Failed -eq 0
$gatePass = $editOk -and $playOk -and $auditOk -and $buildOk -and $playerRunOk -and $perfOk -and $artOk

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
$playerRunLine = 'PlayerRun: SKIPPED (use -IncludePlayerRun)'
if ($IncludePerformance) {
    $playerRunLine = 'PlayerRun: SKIPPED (superseded by -IncludePerformance: 3 locked-env runs instead)'
}
elseif ($IncludePlayerRun) {
    if ($playerRunStatus -eq 'PASS') {
        $playerRunLine = "PlayerRun: PASS exit=$($playerRunRun.ExitCode) resolution=$($playerRunEvidence.ActualResolution) densities=100/200/300"
    } elseif ($playerRunStatus -eq 'SKIPPED') {
        $playerRunLine = 'PlayerRun: SKIPPED'
    } else {
        $playerRunLine = "PlayerRun: $playerRunStatus"
    }
}

Write-Output $editLine
Write-Output $playLine
Write-Output $auditLine
Write-Output $buildLine
Write-Output $playerRunLine
if ($IncludePerformance) {
    Write-Output '=== GAME-ZZZ PERFORMANCE GATE ==='
    if ($null -ne $perfEnvSummary) {
        Write-Output "Environment: $($perfEnvSummary.Status)"
        Write-Output "Resolution: $($perfEnvSummary.Resolution)"
        Write-Output "Graphics: $($perfEnvSummary.GraphicsApi)"
        Write-Output "Quality: $($perfEnvSummary.Quality)"
        Write-Output "vSync: $($perfEnvSummary.VSync)"
        Write-Output "TargetFrameRate: $($perfEnvSummary.TargetFps)"
        Write-Output "Hardware: $($perfEnvSummary.HardwareCpu) / $($perfEnvSummary.HardwareGpu)"
    }
    if ($null -ne $perfVerdict) {
        foreach ($runSummary in $perfVerdict.Runs) {
            $i = $runSummary.Index
            if ($null -eq $runSummary.Env) {
                Write-Output "Run${i}: INFRA/BAD-EVIDENCE"
                continue
            }
            $parts = @()
            foreach ($m in $runSummary.Metrics) {
                $parts += ("{0}: avg={1} p99={2} cpu={3} gpu={4} alive={5}/{6} {7}" -f $m.Density,
                    ([math]::Round([double]$m.Avg, 3)), ([math]::Round([double]$m.P99, 3)),
                    ([math]::Round([double]$m.Cpu, 3)), ([math]::Round([double]$m.Gpu, 3)),
                    $m.Alive, $m.Density, $m.Status)
            }
            $envTag = ''
            if ($runSummary.Env.Status -ne 'PASS') { $envTag = ' ENV_NOT_MET' }
            Write-Output ("Run{0}:{1} {2}" -f $i, $envTag, ($parts -join ' | '))
        }
    }
    Write-Output "PerformanceVerdict: $perfStatus"
    Write-Output "FrameBudget: $($perfContract.frameBudgetMs) ms"
}
else {
    Write-Output 'PerformanceVerdict: NOT_EVALUATED'
}
if ($IncludeArtPerformance) {
    Write-Output '=== GAME-ZZZ ART PERFORMANCE GATE ==='
    if ($null -ne $artEnvSummary) {
        Write-Output "Environment: $($artEnvSummary.Status)"
        Write-Output "Resolution: $($artEnvSummary.Resolution)"
        Write-Output "Hardware: $($artEnvSummary.HardwareCpu) / $($artEnvSummary.HardwareGpu)"
    }
    Write-Output "Profile: $(if ($null -ne $artProfile) { $artProfile.profileId } else { 'N/A' })"
    if ($null -ne $artVerdict) {
        foreach ($runSummary in $artVerdict.Runs) {
            $i = $runSummary.Index
            if ($null -eq $runSummary.Env) {
                Write-Output "Run${i}: INFRA/BAD-EVIDENCE"
                continue
            }
            $parts = @()
            foreach ($m in $runSummary.Metrics) {
                $parts += ("{0}: avg={1} p99={2} cpu={3} gpu={4} alive={5}/{6} {7}" -f $m.Density,
                    ([math]::Round([double]$m.Avg, 3)), ([math]::Round([double]$m.P99, 3)),
                    ([math]::Round([double]$m.Cpu, 3)), ([math]::Round([double]$m.Gpu, 3)),
                    $m.Alive, $m.Density, $m.Status)
            }
            $envTag = ''
            if ($runSummary.Env.Status -ne 'PASS') { $envTag = ' ENV_NOT_MET' }
            Write-Output ("Run{0}:{1} {2}" -f $i, $envTag, ($parts -join ' | '))
        }
    }
    Write-Output "ArtPerformanceVerdict: $artStatus"
}
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
    performance = @{ requested = [bool]$IncludePerformance; verdict = $perfStatus; environmentStatus = $(if ($null -ne $perfVerdict) { $perfVerdict.EnvStatus } else { 'NOT_EVALUATED' }); frameBudgetMs = $(if ($null -ne $perfContract) { $perfContract.frameBudgetMs } else { $null }); hardwareMatch = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.Status -eq 'PASS' } else { $false }); resolution = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.Resolution } else { '' }); graphicsApi = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.GraphicsApi } else { '' }); quality = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.Quality } else { '' }); vsync = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.VSync } else { '' }); targetFrameRate = $(if ($null -ne $perfEnvSummary) { $perfEnvSummary.TargetFps } else { '' }); runs = $(if ($null -ne $perfVerdict) { @($perfVerdict.Runs | ForEach-Object { @{ index = $_.Index; envStatus = $(if ($null -ne $_.Env) { $_.Env.Status } else { 'INFRA' }); densities = @($_.Metrics | ForEach-Object { @{ density = $_.Density; alive = $_.Alive; frames = $_.Frames; avg = $_.Avg; p99 = $_.P99; cpuAvg = $_.Cpu; gpuAvg = $_.Gpu; status = $_.Status; reasons = $_.Reasons } }) } }) } else { @() }) }
    build = @{ requested = [bool]$IncludeBuild; status = $buildStatus; unityExitCode = $(if ($null -ne $buildRun) { $buildRun.ExitCode } else { $null }); timedOut = $(if ($null -ne $buildRun) { $buildRun.TimedOut } else { $false }); executablePath = $buildExe; executableExists = $(if ($null -ne $artifact) { $artifact.ExecutableExists } else { $false }); executableBytes = $(if ($null -ne $artifact) { $artifact.ExecutableBytes } else { 0 }); dataDirectoryExists = $(if ($null -ne $artifact) { $artifact.DataDirectoryExists } else { $false }); timeoutMinutes = $BuildTimeoutMinutes; logPath = $buildLog }
    playerRun = @{ requested = [bool]$IncludePlayerRun; status = $playerRunStatus; exitCode = $(if ($null -ne $playerRunRun) { $playerRunRun.ExitCode } else { $null }); timedOut = $(if ($null -ne $playerRunRun) { $playerRunRun.TimedOut } else { $false }); timeoutMinutes = $PlayerRunTimeoutMinutes; logPath = $playerRunLog; outputPath = $playerRunDir; actualResolution = $(if ($null -ne $playerRunEvidence) { $playerRunEvidence.ActualResolution } else { '' }); graphicsApi = $(if ($null -ne $playerRunEvidence) { $playerRunEvidence.GraphicsApi } else { '' }); densities = $(if ($null -ne $playerRunEvidence) { @($playerRunEvidence.Results | ForEach-Object { @{ density = $_.Density; alive = $_.Alive; frames = $_.Frames; mainMsAvg = $_.MainMsAvg; mainMsP99 = $_.MainMsP99; gpuMsAvg = $_.GpuMsAvg; frameTimingAvailable = $_.FrameTimingOk } }) } else { @() }) }
    artPerformance = @{ requested = [bool]$IncludeArtPerformance; verdict = $artStatus; profileId = $(if ($null -ne $artProfile) { $artProfile.profileId } else { '' }); profilePath = $(if ($IncludeArtPerformance) { Join-Path $ProjectRoot 'docs\qa\ART_PERFORMANCE_PROFILE.json' } else { '' }); resolvedVisuals = $(if ($null -ne $artVerdict -and $artVerdict.ArtReasons.Count -eq 0 -and $null -ne $perfVerdict) { 'see evidence headers' } else { '' }); assignmentMode = $(if ($null -ne $artProfile) { $artProfile.assignmentMode } else { '' }); environmentStatus = $(if ($null -ne $artVerdict) { $artVerdict.EnvStatus } else { 'NOT_EVALUATED' }); frameBudgetMs = $(if ($null -ne $artContract) { $artContract.frameBudgetMs } else { $null }); hardwareMatch = $(if ($null -ne $artEnvSummary) { $artEnvSummary.Status -eq 'PASS' } else { $false }); resolution = $(if ($null -ne $artEnvSummary) { $artEnvSummary.Resolution } else { '' }); runs = $(if ($null -ne $artVerdict) { @($artVerdict.Runs | ForEach-Object { @{ index = $_.Index; envStatus = $(if ($null -ne $_.Env) { $_.Env.Status } else { 'INFRA' }); densities = @($_.Metrics | ForEach-Object { @{ density = $_.Density; alive = $_.Alive; frames = $_.Frames; avg = $_.Avg; p99 = $_.P99; cpuAvg = $_.Cpu; gpuAvg = $_.Gpu; status = $_.Status; reasons = $_.Reasons } }) } }) } else { @() }) }
    gate = @{ verdict = $(@('FAIL', 'PASS')[[int]$gatePass]); startedUtc = $gateStartUtc; endedUtc = [DateTime]::UtcNow }
    issues = $Issues
}

if ($gatePass) { exit 0 } else { exit 1 }
