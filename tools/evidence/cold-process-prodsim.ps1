<#
.SYNOPSIS
    三独立 Unity 冷启动进程的 ProdSim 基线校验（S6P-WO-04A 强制前置）。

.DESCRIPTION
    规划 AI 仲裁要求：凡「新 canonical 基线晋升」都必须由 **3 个独立 Unity 冷启动进程** 产出并
    exact match；不得用同一个测试进程内连跑三次冒充（那证明不了 static/初始化顺序污染）。

    本脚本：
      1. 要求编辑器【已关闭】（batch 模式需要独占工程；工程被编辑器占用时 unity test 会拒绝）。
      2. 每次运行前先删掉 report 产物，确保读到的是**本次**新写的，而不是陈旧文件。
      3. 依次跑 N 次 batch EditMode 的 ProductionSimulatorTests，逐次记录时间戳 + deterministicHash。
      4. 三次必须完全一致，否则以非零码退出（调用方应 STOP，不得继续 gameplay 改动）。

.OUTPUTS
    markdown 表格到 stdout；`-OutFile` 可落盘。

.EXAMPLE
    pwsh -NoProfile -File tools/evidence/cold-process-prodsim.ps1 -Runs 3
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = "",
    [string]$ProjectPath = "",
    [int]$Runs = 3,
    [string]$Filter = "ProductionSimulatorTests",
    [string]$OutFile = ""
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = (Resolve-Path (Join-Path $here '..\..')).Path
}
if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = Join-Path $RepoRoot 'unity\game\New Unity Project'
}
$reportPath = Join-Path $ProjectPath 'docs\qa\PRODUCTION_SIMULATION_REPORT.json'
$xmlDir = Join-Path $env:TEMP 'zzz-cold-prodsim'
New-Item -ItemType Directory -Force -Path $xmlDir | Out-Null

function Get-EditorPids {
    @(Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -and $_.MainWindowTitle -like '*Unity*' } |
      ForEach-Object { $_.Id })
}

# ---- 0. 前置：编辑器必须关闭 ----
$pids = Get-EditorPids
if ($pids.Count -gt 0) {
    throw "编辑器仍在运行（PID: $($pids -join ', ')）。冷进程校验要求编辑器先关闭：" +
          "`n  unity close `"$ProjectPath`"`n" +
          "  若 close 无效（挂死态），按 UNATTENDED_STATE.md 的恢复配方处理后重试。"
}

$rows = New-Object System.Collections.Generic.List[object]

for ($i = 1; $i -le $Runs; $i++) {
    # 删掉旧产物：只有本次真的跑了，才读得到文件
    if (Test-Path -LiteralPath $reportPath) { Remove-Item -LiteralPath $reportPath -Force }
    $xml = Join-Path $xmlDir ("run{0}-{1:yyyyMMdd-HHmmss}.xml" -f $i, (Get-Date))

    Write-Host "[cold-run $i/$Runs] 启动 batch EditMode ($Filter) ..."
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    & unity test $ProjectPath --mode EditMode --filter $Filter --output $xml 2>&1 |
        Tee-Object -Variable log | Out-Null
    $sw.Stop()
    $exit = $LASTEXITCODE

    $hash = ''
    $contract = ''
    $invalid = ''
    if (Test-Path -LiteralPath $reportPath) {
        $j = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
        $hash = $j.deterministicHash
        $contract = $j.simulationContractVersion
        $invalid = $j.invalidCount
    }

    $rows.Add([pscustomobject]@{
        Run       = $i
        Time      = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
        Seconds   = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        ExitCode  = $exit
        Contract  = $contract
        Invalid   = $invalid
        Hash      = $hash
    })
    Write-Host ("[cold-run {0}] exit={1} hash={2}" -f $i, $exit, $hash)
}

$hashes = @($rows | ForEach-Object { $_.Hash })
$distinct = @($hashes | Sort-Object -Unique)
$exact = ($distinct.Count -eq 1) -and ($distinct[0] -ne '') -and (@($rows | Where-Object { $_.ExitCode -ne 0 }).Count -eq 0)

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('```text')
$lines.Add("Cold-process ProdSim baseline verification  (runs=$Runs, filter=$Filter)")
$lines.Add("project : $ProjectPath")
$lines.Add("editor closed at start : YES")
foreach ($r in $rows) {
    $lines.Add(("Run{0} | {1} | {2}s | exit={3} | contract={4} | invalid={5} | {6}" -f `
        $r.Run, $r.Time, $r.Seconds, $r.ExitCode, $r.Contract, $r.Invalid, $r.Hash))
}
$lines.Add("Distinct hashes : $($distinct.Count)  [$($distinct -join ', ')]")
$lines.Add("EXACT MATCH     : $(if ($exact) { 'YES' } else { 'NO' })")
$lines.Add('```')
$text = ($lines -join [Environment]::NewLine)
if ($OutFile) { Set-Content -LiteralPath $OutFile -Value $text -Encoding UTF8 }
$text

if (-not $exact) {
    Write-Error "冷进程基线不一致 —— 按 WO-04A 合同必须 STOP，不得继续 gameplay 改动。"
    exit 1
}
exit 0
