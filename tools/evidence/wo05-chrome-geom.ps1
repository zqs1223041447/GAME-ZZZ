<#
.SYNOPSIS
  S6P-WO-05 Headless Chrome geometry oracle (existing seam: _poe_src/geom_oracle.html + tree_raw.json).
  Does not npm-install. Uses local Google Chrome.
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = "",
    [string]$Fixture = "G0_1920"
)
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = (Resolve-Path (Join-Path $here '..\..')).Path
}
$template = Join-Path $RepoRoot 'tools\evidence\wo05-geom-oracle.html'
if (-not (Test-Path -LiteralPath $template)) {
    $template = Join-Path $RepoRoot '_poe_src\geom_oracle.html'
}
$treeRaw = Join-Path $RepoRoot '_poe_src\tree_raw.json'
$unityJson = Join-Path $RepoRoot "unity\game\New Unity Project\docs\qa\wo05\$Fixture.unity.json"
$outHtml = Join-Path $env:TEMP "wo05-$Fixture.html"
$outJson = Join-Path $RepoRoot "unity\game\New Unity Project\docs\qa\wo05\$Fixture.chrome.json"

$chrome = @(
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe",
    "C:\Program Files\Google\Chrome\Application\chrome.exe",
    "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $chrome) { throw "Chrome executable not found (existing seam required)." }
if (-not (Test-Path -LiteralPath $template)) { throw "renderer missing: $template" }
if (-not (Test-Path -LiteralPath $treeRaw)) { throw "canonical tree missing: $treeRaw" }
if (-not (Test-Path -LiteralPath $unityJson)) { throw "unity artifact missing (run EditMode WritesUnityGeomArtifacts first): $unityJson" }

$u = Get-Content -LiteralPath $unityJson -Raw | ConvertFrom-Json
$view = @{
    name = $u.name
    zoom = [double]$u.zoom
    pan = @([double]$u.pan[0], [double]$u.pan[1])
    designScale = [double]$u.designScale
    lod = [int]$u.lod
} | ConvertTo-Json -Compress

$html = Get-Content -LiteralPath $template -Raw
$tree = (Get-Content -LiteralPath $treeRaw -Raw).Replace('<', '\u003c')
$html = $html.Replace('__VIEW_JSON__', $view).Replace('__TREE_JSON__', $tree)
Set-Content -LiteralPath $outHtml -Value $html -Encoding UTF8

$uri = ([Uri]$outHtml).AbsoluteUri
$errFile = Join-Path $env:TEMP "wo05-chrome-err.txt"
$oldEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$dump = & $chrome --headless --disable-gpu --virtual-time-budget=4000 --dump-dom $uri 2>$errFile | Out-String
$ErrorActionPreference = $oldEap
if ([string]::IsNullOrWhiteSpace($dump)) { throw "chrome --dump-dom produced no output" }
if ($dump -notmatch '<pre id="out">') { throw "oracle <pre id=out> missing from dump-dom" }
$m = [regex]::Match($dump, '<pre id="out">([\s\S]*?)</pre>')
if (-not $m.Success) { throw "could not parse oracle JSON" }
$text = [System.Net.WebUtility]::HtmlDecode($m.Groups[1].Value.Trim())
Set-Content -LiteralPath $outJson -Value $text -Encoding UTF8

$c = $text | ConvertFrom-Json
$umap = @{}
foreach ($n in $u.nodes) { $umap[[int]$n.id] = $n }
$max = 0.0
$mismatch = 0
$compared = 0
foreach ($n in $c.nodes) {
    $id = [int]$n.id
    if (-not $umap.ContainsKey($id)) { $mismatch++; continue }
    $un = $umap[$id]
    $dx = [math]::Abs([double]$n.sx - [double]$un.sx)
    $dy = [math]::Abs([double]$n.sy - [double]$un.sy)
    $d = [math]::Sqrt($dx*$dx + $dy*$dy)
    if ($d -gt $max) { $max = $d }
    $compared++
}
$uIds = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($n in $u.nodes) { [void]$uIds.Add([int]$n.id) }
$cIds = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($n in $c.nodes) { [void]$cIds.Add([int]$n.id) }
$setMismatch = 0
foreach ($id in $uIds) { if (-not $cIds.Contains($id)) { $setMismatch++ } }
foreach ($id in $cIds) { if (-not $uIds.Contains($id)) { $setMismatch++ } }

Write-Host ("chrome={0}" -f $chrome)
Write-Host ("fixture={0} compared={1} setMismatch={2} maxCentreErrPx={3:N4}" -f $Fixture, $compared, $setMismatch, $max)
if ($setMismatch -ne 0) { throw "visible NodeId set mismatch=$setMismatch" }
if ($max -gt 1.0) { throw "max node-centre error $max > 1px" }
Write-Host "PASS"
