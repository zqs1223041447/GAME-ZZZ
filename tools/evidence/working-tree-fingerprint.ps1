<#
.SYNOPSIS
    生成 Evidence 用的「工作区指纹 + 变更文件清单」。

.DESCRIPTION
    规划 AI 自 S6P-WO-03 起要求：Gate 与 Evidence 必须唯一对应**同一个 working-tree state**。
    所以每份 Evidence Pack 都要带上本脚本输出的这一段。

    指纹是**内容寻址**的（对每个文件的 SHA-256 再整体哈希），不是 mtime 也不是 diff 文本，
    因此：同一份工作区重复跑必然同值；只要有一个字节变了就必然变值。

    文件集合 = `git ls-files`（已跟踪） ∪ `git ls-files --others --exclude-standard`（未跟踪但未被忽略）。
    被 .gitignore 忽略的（Library/ Temp/ 等）天然排除。

.OUTPUTS
    默认把一段 markdown（可直接粘进 Evidence Pack）打到 stdout。
    加 -Json 输出机器可读 JSON；加 -OutFile <路径> 同时落盘。

.EXAMPLE
    pwsh -File tools/evidence/working-tree-fingerprint.ps1
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = "",
    [switch]$Json,
    [string]$OutFile = ""
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $here = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = (Resolve-Path (Join-Path $here '..\..')).Path
}
Push-Location $RepoRoot
try {
    $tracked = @(& git ls-files)
    $untracked = @(& git ls-files --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) { throw "git ls-files 失败（$RepoRoot 是 git 仓库吗？）" }

    $all = @($tracked + $untracked) | Where-Object { $_ } | Sort-Object -Unique

    $sha = [System.Security.Cryptography.SHA256]::Create()
    $manifest = New-Object System.Collections.Generic.List[object]
    $totalBytes = 0L

    foreach ($rel in $all) {
        $full = Join-Path $RepoRoot $rel
        if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { continue }
        $fi = Get-Item -LiteralPath $full
        $totalBytes += $fi.Length
        $stream = [System.IO.File]::OpenRead($full)
        try { $hash = $sha.ComputeHash($stream) } finally { $stream.Dispose() }
        $manifest.Add([pscustomobject]@{
            Path   = ($rel -replace '\\', '/')
            Bytes  = $fi.Length
            Sha256 = ([BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
        })
    }

    # 整体指纹：对 "path<TAB>sha256" 行按路径序拼接后再哈希一次（与文件系统枚举顺序无关）
    $sb = New-Object System.Text.StringBuilder
    foreach ($m in $manifest) { [void]$sb.Append($m.Path).Append("`t").Append($m.Sha256).Append("`n") }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($sb.ToString())
    $overall = ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace '-', '').ToLowerInvariant()

    # 变更清单：porcelain，按顶层区归类
    $status = @(& git status --porcelain)
    $areas = [ordered]@{}
    foreach ($line in $status) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $code = $line.Substring(0, 2).Trim()
        $path = $line.Substring(3).Trim().Trim('"')
        $area =
            if ($path -like 'unity/*/Assets/Runtime/*') { 'Assets/Runtime' }
            elseif ($path -like 'unity/*/Assets/Tests/*') { 'Assets/Tests' }
            elseif ($path -like 'unity/*/Assets/*') { 'Assets/other' }
            elseif ($path -like 'unity/*/docs/*') { 'docs' }
            elseif ($path -like '开发计划/*') { '开发计划' }
            elseif ($path -like '_poe_src/*') { '_poe_src' }
            else { 'other' }
        if (-not $areas.Contains($area)) { $areas[$area] = New-Object System.Collections.Generic.List[string] }
        $areas[$area].Add("$code $path")
    }

    $head = (& git rev-parse HEAD).Trim()
    $branch = (& git rev-parse --abbrev-ref HEAD).Trim()
    $dirty = if ($status.Count -gt 0) { 'YES' } else { 'NO' }

    if ($Json) {
        $out = [pscustomobject]@{
            repoRoot        = $RepoRoot
            branch          = $branch
            head            = $head
            fileCount       = $manifest.Count
            totalBytes      = $totalBytes
            workingTreeHash = $overall
            dirty           = $dirty
            changedAreas    = $areas
        } | ConvertTo-Json -Depth 6
        if ($OutFile) { Set-Content -LiteralPath $OutFile -Value $out -Encoding UTF8 }
        $out
    }
    else {
        $lines = New-Object System.Collections.Generic.List[string]
        $lines.Add('```text')
        $lines.Add("Working-Tree Fingerprint (content-addressed, SHA-256)")
        $lines.Add("branch                : $branch")
        $lines.Add("HEAD                  : $head")
        $lines.Add("tracked+untracked files: $($manifest.Count)")
        $lines.Add("total bytes           : $totalBytes")
        $lines.Add("WORKING-TREE HASH     : $overall")
        $lines.Add("dirty                 : $dirty")
        $lines.Add('--- changed-file inventory (git status --porcelain, by area) ---')
        if ($areas.Count -eq 0) { $lines.Add('(clean)') }
        foreach ($k in $areas.Keys) {
            $lines.Add("[$k] $($areas[$k].Count)")
            foreach ($f in ($areas[$k] | Sort-Object)) { $lines.Add("    $f") }
        }
        $lines.Add('```')
        $text = ($lines -join [Environment]::NewLine)
        if ($OutFile) { Set-Content -LiteralPath $OutFile -Value $text -Encoding UTF8 }
        $text
    }
}
finally { Pop-Location }
