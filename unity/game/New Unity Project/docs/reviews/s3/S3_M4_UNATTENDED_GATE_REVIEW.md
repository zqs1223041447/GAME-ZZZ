# S3_M4_UNATTENDED_GATE_REVIEW（S3 维护轮 M4 独立复核）

日期：2026-09-08。工作令：`S3-M4-UNATTENDED-VERIFY-GATE`。Phase R（A15 Reviewer）：以真实 diff + 实际运行复核。Baseline HEAD=ddb9ace。

## Baseline HEAD / Before verification model

- Baseline HEAD=ddb9ace；EditMode 113/113、PlayMode 3/3（pipeline run_tests）；Content Audit PASS（M3 failure-safe）。
- Before：完整验证流程依赖外部会话工具（pipeline package CLI `unity command run_tests/test_status`）+ 运行中的编辑器 + 上一位 AI 的工具知识；仓库内没有可重复、带机器退出码的标准 Gate 入口。A15「执行测试」职责未 Repository-as-Memory 化。

## Canonical command

`unity/game/New Unity Project/tools/verify_unattended.ps1`（仓库内、Assets 外，Unity 项目根自动由脚本自身位置解析——`$PSScriptRoot` 父目录，无任何机器路径硬编码）。参数：`-UnityPath` / `-TimeoutMinutes`（默认 20）/ `-SelfTest`。

## Unity resolution strategy（实测）

显式 `-UnityPath` → env `UNITY_EDITOR` → `ProjectVersion.txt` 精确版本（6000.3.23f1）→ Hub 安装根搜索（`%ProgramFiles%\Unity\Hub\Editor`、`%ProgramFiles(x86)%`、`%LOCALAPPDATA%\Programs\Unity\Hub\Editor` 默认根 + `%APPDATA%\UnityHub\secondaryInstallPath.json` 记录的自定义根）逐根匹配 `<版本>\Editor\Unity.exe`；找不到立即 exit 2（打印所需版本与已查策略，不交互、不下载）。本机实测：默认根为空，经 **secondaryInstallPath.json（Hub 标准机制，记录 G:\Unity\Hub\Editor）命中**——证明自定义安装位置的机器也能自动解析，脚本无本机硬编码。

## Editor lock behavior（实测）

`Temp/UnityLockfile` 存在 → fail fast（Reason: PROJECT_ALREADY_OPEN，exit 3）。实测时编辑器（PID 45828）全程存活，Gate 未杀任何进程；随后执行 AI 正常关闭编辑器（`unity close` graceful），锁释放后 Gate 才进入套件阶段。

## Timeout behavior（机制实测）

每套件 `Start-Process -PassThru` 只持自己启动的 child 句柄；`WaitForExit(TimeoutMinutes*60000)` 超时→`Kill()`→`WaitForExit()` 回收。合成进程实测：2 秒超时返回 False（2015ms）→ Kill → HasExited=True。无广域 `Stop-Process Unity`，无其它进程触碰。

## EditMode actual result（canonical Gate ×3）

| Run | Exit | EditMode | PlayMode | ContentAudit |
|---|---|---|---|---|
| 1 | 0 | PASS 113/113, failed 0, skipped 0 | PASS 3/3, failed 0, skipped 0 | PASS, fresh=YES, failures=0 |
| 2 | 0 | PASS 113/113 | PASS 3/3 | PASS, fresh=YES |
| 3（归一化复现实验） | 0 | PASS 113/113 | PASS 3/3 | PASS, fresh=YES |

## Content Audit freshness result / verdict

freshness=before/after 双记录 `LastWriteTimeUtc`，判据 `afterUtc ≥ gateStartUtc`（run1 实测 after=18:20:58Z ≥ start=18:20:22Z）；hash 不变是预期（deterministic PASS snapshot byte-equivalent）。verdict 解析三行：`Audit completed: YES` / `Verdict: PASS` / `Failure count: 0`，任一不满足 Gate FAIL。**旧 PASS snapshot 无法骗过 Gate**（freshness + verdict 双检查；SelfTest freshness 夹具证明陈旧文件判不 fresh）。

## SelfTest result

9/9 PASS，exit 0：pass-xml / failed-xml / missing-xml / malformed-xml / missing-result-node / audit-pass-text / audit-fail-text / audit-incomplete-text / audit-freshness。

## Invalid Unity path negative result

`-UnityPath "Z:\nope\Unity.exe"` → 立即 FAIL（UNITY_NOT_FOUND，exit 2，打印 Required Unity version: 6000.3.23f1 与已查策略）；不 hang、不修改仓库。

## Double-run reproducibility

Canonical Gate 连续 3 次全 PASS exit 0；EditMode/PlayMode totals 恒 113/3（XML 动态解析，非硬编码）；`CONTENT_AUDIT_S3_CLOSEOUT.md` 三次 sha256 两两一致（0707e2b82f2ff555af9d416875eb124e15fb5b334dd94af31de31e3a0d25be66）。

## Repository pollution check / ProjectSettings normalization

三次 Gate 后仓库无任何 XML/log/JSON/临时产物（全部留在 %TEMP%）。唯一额外变化：`ProjectSettings/ProjectSettings.asset` 的 `scriptingDefineSymbols.Standalone` 被批量实例确定性移除孤儿 define `SENTIS_ANALYTICS_ENABLED`。复核结论：

- 该 define 于 S0 初始推送（d5beb7f）入库；manifest 无 sentis/ml 包、全工程（Assets+Packages）**零引用**——纯孤儿符号。
- 复现实验：还原文件 → 再跑 Gate → 再次被清理（每次批量运行必发生）；同文件中同样无引用的自定义 define `APP_UI_EDITOR_ONLY` 始终保留——证明清理是选择性的（只清包属 analytics 孤儿），终态稳定。
- 处置：一次性归一化提交（移除该孤儿 define，独立 chore commit 说明原因）。此后 canonical Gate 每次运行仓库零污染；不采用 .gitignore 掩盖，不在 Gate 内做还原（Gate 不得修改仓库）。

## Reviewer 必答（新 AI 视角：刚 clone 仓库、只知道 PowerShell）

- Q1 不依赖历史聊天能否找到 canonical verification command？**YES**——`docs/qa/UNATTENDED_VERIFICATION.md`（QA 契约）+ STATUS「校验」节均指向 `tools/verify_unattended.ps1`。
- Q2 是否不依赖 pipeline run_tests？**YES**——脚本直接以 Unity Test Framework 官方 batch 方式（`-batchmode -runTests -testPlatform -testResults -logFile`）启动 Unity.exe，实际运行验证（6000.3.23f1）。
- Q3 EditMode 红但 PlayMode 能跑时是否仍收集 PlayMode 结果？**YES**——跳过条件仅为基础设施失败（timeout/缺畸形 XML/exit-XML 失配），测试失败（XML Failed）不触发跳过（代码结构复核）。
- Q4 旧 PASS Content Audit 能否骗过 Gate？**NO**——freshness（本轮重新落盘）+ verdict（YES/PASS/0）双检查，缺一即 FAIL。
- Q5 Unity hang 会不会永久卡住无人值守工作？**NO**——套件级超时（默认 20 分钟），超时杀自己 child 并记 TIMEOUT→Gate FAIL（机制实测）。
- Q6 Project 已被 Editor 打开会不会乱杀进程？**NO**——fail fast exit 3，实测编辑器存活。
- Q7 测试数量变化是否必须改脚本？**NO**——totals 从 XML 动态解析（SelfTest 用 2/2 夹具与实际 113/3 双重证明）。

## PowerShell 安全必查

- 路径全部正确 quote：ArgumentList 用内嵌引号构造，含空格的 `New Unity Project` 三次实际运行通过（路径本身就是验证样例）。
- child process exit 正确读取：WaitForExit 后读 `$p.ExitCode`（并要求与 XML 一致，防「exit 0 即绿」）。
- timeout 只 kill 自己启动的 process（-PassThru 句柄）；无危险广域 `Stop-Process Unity`。
- 不删除仓库目录；temp cleanup 只作用于 `%TEMP%\GAME-ZZZ-UnattendedGate` 自身目录。
- Gate 不跑 git、不改 STATUS/ROADMAP/Catalog/测试/代码（只验证）。

## 失败收集结构（Reviewer 必查）

`run Edit → record → run Play if infrastructure still permits → record → check Audit → record → finalize once`：统一出口单次判定（$gatePass = editOk && playOk && auditOk），EditMode 一红不会提前 exit 导致 PlayMode 无证据。

## Runtime delta / Content delta / 性能

Runtime gameplay 文件修改=0（git 复核：本轮仅新增 tools 脚本 + docs + ProjectSettings 归一化）；hot path delta=0；new Update=0；new MonoBehaviour=0；新 Package=0。内容全轴 delta=0。Phase 3/4/5 未启动。无需 Arena。

## Remaining risks

- 低：Gate 假定项目锁=`Temp/UnityLockfile`（本机 Unity 6000.3 实测成立）；若未来 Unity 改锁机制需按实际版本调整（工作令三十三预案已记录）。
- 低：批处理模式每次启动会重新导入项目（首次冷启动可能较慢）；-TimeoutMinutes 默认 20 分钟已按无人值守合理化，必要时可调参。
- 低：`-nographics` 未加（工作令明确默认不加）；PlayMode 含真实 Runtime smoke，保持与现有环境一致。

## Verdict

**PASS**
