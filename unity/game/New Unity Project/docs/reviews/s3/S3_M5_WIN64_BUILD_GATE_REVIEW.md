# S3_M5_WIN64_BUILD_GATE_REVIEW（S3 维护轮 M5 独立复核）

日期：2026-09-08。工作令：`S3-M5-WIN64-PLAYER-BUILD-GATE`。Phase R（A15 Reviewer）：以真实 diff + 实际运行复核。Baseline HEAD=387e508。

## Baseline HEAD / Before verification model

- Baseline HEAD=387e508（M4 回填）；EditMode 113/113、PlayMode 3/3（M4 canonical Gate）；Content Audit PASS。
- Before：仓库能证明「编辑器测试全绿」，但**不能证明「当前 HEAD 能产出 Windows Player」**；Build Settings 场景顺序（ArenaPerfHarness `SceneManager.LoadScene(1)` 隐藏依赖）无任何自动锁定。

## Quick Gate / Full Gate command

- Quick：`.\tools\verify_unattended.ps1`（默认行为不变：EditMode+PlayMode+Audit）。
- Full：`.\tools\verify_unattended.ps1 -IncludeBuild`（= Quick + StandaloneWindows64 Player Build），另有 `-BuildTimeoutMinutes`（默认 30）。

## Unity version / Build command / Build target

- Unity 6000.3.23f1（ProjectVersion.txt；复用 M4 解析链）。
- 构建命令（实测有效）：`-batchmode -quit -projectPath "<root>" -buildTarget win64 -buildWindows64Player "<temp>\PlayerBuild\GAME-ZZZ.exe" -logFile "<temp>\PlayerBuild.log"`。含空格的 `New Unity Project` 路径全程正确 quote（全部实测运行通过）。
- Build target：win64（StandaloneWindows64）。

## Build settings scene contract（已锁定）

`BuildSettingsContractTests` 2 测（EditorBuildSettings API，非 YAML 解析）：Index 0=Assets/Scenes/Bootstrap.unity（启用+资产存在）、Index 1=Assets/Scenes/Arena.unity（启用+资产存在，`ArenaPerfHarness.LoadScene(1)` 契约）；启用场景路径无重复、Bootstrap/Arena 各恰一次、无「启用但资产缺失」条目。本轮不改 Harness 的 index 契约（锁现状；变更需另行工作令同步处理——记录于 Remaining risks）。

## Full Gate 实际运行结果

| Run | Exit | EditMode | PlayMode | Audit | PlayerBuild |
|---|---|---|---|---|---|
| 1 | 0 | PASS 115/115, failed 0 | PASS 3/3 | PASS, fresh=YES, failures=0 | PASS win64（exe 667136 bytes，Data 162 文件） |
| 2 | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（同上） |
| 3 | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（同上，**幂等性证明**：4 个被归一化设置资产前后哈希逐一相同） |
| 4（归一化收口提交后） | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（**零 tracked diff**） |
| 5（根因修复提交后） | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（零新增 mutation） |
| 6（重复性确认） | 0 | PASS 115/115 | PASS 3/3 | PASS, fresh=YES | PASS win64（零新增 mutation） |

- 构建日志保留完整（`%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild.log`），含稳定成功标记 `Build Finished, Result: Success.` 与 `Exiting batchmode successfully now!`（第三证存在，未做成脆弱 regex——核心判据=进程+产物双证）。
- exe 字节数三次构建完全一致（667136）；按工作令二十四不比较 exe hash（不要求 byte-identical）。

## Artifact validation

双证据：Unity 进程正常完成（exit 0，timeout=false）+ `GAME-ZZZ.exe` 存在非空（667136 bytes）+ `GAME-ZZZ_Data/` 存在非空（162 文件）。exit=0 而产物无效 → FAIL（SelfTest 夹具覆盖零字节/缺失/空目录）。未硬编码后端产物（GameAssembly.dll/MonoBleedingEdge 等不作为判据；实际产物含 MonoBleedingEdge/D3D12/Burst 调试目录仅作记录）。

## SelfTest build-validator result

14/14 PASS，exit 0（原 9 项 + 新 5 项构件夹具：artifact-pass / missing-exe / zero-byte-exe / missing-data / empty-data）。不启动 Unity、不引入 Pester。

## Summary JSON / 控制台摘要

- Quick：`build.requested=false, status=SKIPPED`，控制台明确 `PlayerBuild: SKIPPED (use -IncludeBuild)`（实测，不谎称已验证 Player）。
- Full：`build` 字段含 requested/status/unityExitCode/executablePath/executableExists/executableBytes/dataDirectoryExists/timeoutMinutes/logPath（实测完整）。仅存 temp，不入仓库。

## Repository pollution result / ProjectSettings mutation audit

**审计过程（工作令二十五/二十六全程执行）**：
1. 构建前基线：tracked diff=0（还原后确认）。
2. Full Gate 1/2 后：发现 4 个设置资产被批量实例归一化——`ProjectSettings.asset`（SENTIS_ANALYTICS_ENABLED 回写）、`UniversalRenderPipelineGlobalSettings.asset`（m_RuntimeSettings 注册表 16 rid 填充）、`PC_RPAsset.asset`（URP 关键字预过滤模式重推导）、`DefaultVolumeProfile.asset`（新 schema 字段 worldOffset/filter+2 个失效组件引用清理）。EditorBuildSettings.asset / GraphicsSettings / Packages **未被触碰** ✓。
3. 根因：`-quit` 构建实例会保存批量加载期间引擎派生的归一化（测试实例不保存——M4 纯测试门零污染的原因）。
4. 幂等性实证（Run 3）：归一化状态落盘后，4 文件哈希前后逐一相同 → 单次收敛。
5. 处置：按工作令二十五「确认正确且应成为 canonical 的变更才允许独立记录」提交归一化状态（b79c77c）→ Run 4 零 tracked diff。
6. **振荡发现**：Quick Gate（纯测试实例）证明 define 被反向清理——`com.unity.ai.inference`（间接依赖包）`AnalyticsDefineManager` 按 `EditorAnalytics.enabled` 加删 define，该机器级同意值跨实例类型不稳定（编辑器/构建=真、批量测试=假）→ 振荡矩阵：HEAD 含 define 时 Quick 脏 / HEAD 无 define 时 Full 脏，任一状态都无法双 Gate 同时零污染（b79c77c 的含 define 态被 Quick 证伪）。
7. **根因修复**：`submitAnalytics: 1→0`（项目级分析提交退出 → `EditorAnalytics.enabled` 恒 false → 包管理器 shouldEnable 恒 false → define 恒保持缺席，与 M4 移除方向一致，关闭 b79c77c 暂定态）。收敛实证：修复后 Quick×1 + Full×2 全部零新增 mutation（唯一 diff=该单行提交本身）。3de5d24。
8. 终态：**Quick 与 Full 两级 Gate 全部零仓库污染**（exe/Data/pdb/log/XML/JSON 全在 temp；git 实证）。

## Runtime delta / Content delta / 性能

- Runtime gameplay 文件修改=0（git 复核：本轮仅 tools 脚本 + 新 EditMode 测试 + docs + 2 个 ProjectSettings canonical 修复）。CombatMath/SliceSession/ArenaSim/**ArenaPerfHarness**/Skill/Loot/Craft/Support/Audio/UI 零触碰。
- Content delta 全 0（场景内容/build scene 计数=2 不变；内容护栏全绿；新增 EditMode 测试不是游戏内容）。
- 新 assembly=0；新 Package=0；Phase 3/4/5 未启动。本轮不建立性能数字（Build truth 先行，Player-run/Performance truth 留给规划 AI 决定的后续轮）。

## Reviewer 必答 Q1-Q7

- Q1 Edit/Play 全绿但 Player build 失败时 Full Gate 会红吗？**YES**——`$gatePass = editOk && playOk && auditOk && buildOk`，build FAIL → exit 1。
- Q2 Build 成功但 EditMode 红时 Full Gate 会绿吗？**NO**——editOk 是必要条件，Build 成功不覆盖测试失败。
- Q3 Build artifact 不存在但 Unity exit=0 时会绿吗？**NO**——双证据判定（进程+产物）；SelfTest 缺 exe/零字节/缺 Data/空 Data 夹具证明 validator 拒绝。
- Q4 构建输出会污染 repo 吗？**NO**——输出全在 `%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild\`；Run 4/5/6 提交后 git 实证零 tracked diff。
- Q5 PlayerBuild timeout 会永久卡住无人值守 AI 吗？**NO**——`-BuildTimeoutMinutes`（默认 30）+ M4 同款只杀自己 child 的 WaitForExit/Kill 机制。
- Q6 场景顺序被改成 Arena=0/Bootstrap=1 时测试会红吗？**YES**——`BuildScenes_Index0IsBootstrap_Index1IsArena` 逐索引精确断言路径（相等断言，换序必红）。
- Q7 默认 Quick Gate 会谎称已验证 Player 吗？**NO**——实测输出 `PlayerBuild: SKIPPED (use -IncludeBuild)`，summary JSON `status=SKIPPED, requested=false`。

## Remaining risks

- 低：`ArenaPerfHarness.LoadScene(1)` index 依赖为既有技术债（已用测试锁住现状；场景结构变更须同步 Harness，另行工作令）。
- 低：`com.unity.ai.inference` 为间接依赖包（本项目无 ML 内容，纯包级携带）；其 define 机制已通过 submitAnalytics=0 根因稳定；未来移除该间接依赖可另行评估（收益=依赖图更瘦）。
- 低：`submitAnalytics: 0` 关闭项目分析遥测同意（机器级/项目级设置）；如未来需要 Unity 分析服务需显式重开（届时 define 振荡问题会回来，需同步重估）。
- 低：Player Build 产物不要求 byte-identical（工作令二十四）；本轮三次构建字节数一致属观测事实，不作为契约。

## Verdict

**PASS**
