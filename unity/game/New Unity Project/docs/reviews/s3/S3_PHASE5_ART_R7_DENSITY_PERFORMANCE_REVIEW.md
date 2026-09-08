# S3-PHASE5-ART-R7 DENSITY PERFORMANCE 复核与收口报告

工作令：S3-P5-ART-R7-FORMAL-VISUAL-DENSITY-GATE（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。Baseline=7569d3e（R6 收口 HEAD）；双 Gate sourceCommit=**321d9b4**（feat+tooling 提交后 clean tracked HEAD，Gate A/B 完全一致）。

## 0. Verdict

**PASS — FORMAL ART DENSITY PERFORMANCE GATE ESTABLISHED**（Gate A 与 Gate B 双 PASS；canonical 与 Art 两层各自独立 PASS，证据已按 M8 惯例冻结入库）。

## 1. Why canonical Dummy gate insufficient（§1）

正式地图渲染 Troll/FireLion/Bruce，但 -IncludePerformance 仍测 EnemyKind.Dummy——现有 PASS 只证明 gameplay/simulation 无回归，不能证明 100–300 个正式 PBR visual 满足 1440p/120 预算。R7 补上该证据缺口。

## 2. 本轮不是性能优化（§2）

只有 measurement + repeatable gate installation。Art 指标一次 FAIL 即冻结扩张、如实记录、不调参（本轮未触发 FAIL 分支）。

## 3. Canonical Gate 隔离（§3/§36/§37）

- `.\tools\verify_unattended.ps1 -IncludePerformance` 行为不变：Dummy workload、`PERFORMANCE_GATE.json` **0 semantic diff**（2560×1440/D3D12/PC/vSync0/targetFps-1/100·200·300/60 warmup/600 sample/0.12 cast/8.33ms/3 runs/locked CPU+GPU 全部未动）；M7/M8 证据解析兼容。
- ArenaPerfHarness base 模式 anti-cheat：density/sample/cast/kill-refill/combat/quality/resolution/HP 全未改；新增逻辑全部受 `-arenaArtVisuals`（benchmark-only）控制；SpawnMap delta=0。

## 4. Art Performance Profile（§7/§8/§26/§27）

- `docs/qa/ART_PERFORMANCE_PROFILE.json`：profileId=formal-enemy-visual-stress-v1、baseContract=PERFORMANCE_GATE.json（硬件/环境/预算全继承）、selectionMode=DistinctMappedFormalVisuals、assignmentMode=RoundRobinByEnemyKind、gameplayKind=Dummy、useEnemyVisualPresenter=true。
- Resolved visuals 由 EnemyVisualCatalog+RuntimeResourcePaths 运行期解析（**不复制资源路径 truth**）：TrollWarriorVisual;FireLionVisual;BruceVisual。catalog 未来增加正式视觉=自动纳入（预期）；selection/assignment/workload 语义变更须正式工作令。
- SelfTest 拒绝未知 selectionMode（ActualMapMix 被拒实证）。

## 5. Art Harness 实现（§9-§22）

- gameplay 实体保持 EnemyKind.Dummy（canonical workload 零变化），presentation 由 `EnemyVisualPresenter`+正式 prefab 叠加（§11：无第二套 Presenter/无模型专用系统；benchmark-only assignment helper 只决定槽位↔视觉）。
- Round-robin：art 槽 j ↔ Dummy 池槽 j 固定 1:1（j%visualCount），mix=100 档 34/33/33、200 档 67/67/66、300 档 100/100/100（§12/§14：instances==density）。
- 预载/池化：300 槽在 warmup 前一次建满（§15/§16/§17），密度切换只 active/inactive；正式 600 frame sample 内零 Resources.Load/Instantiate。
- 真实动画（§20）：Presenter 消费 Dummy 既有 observation（Idle/Run/Attack/Hit/Death）+ R5 feedback（§21：workload 无 Ignite 时如实记录，不人为提高）；root motion off；位置由 canonical Dummy gameplay 驱动（§58/§59：不做相机外藏匿、不挤视锥）。
- 材质完整（§22）：MaterialPropertyBlock/sharedMaterial 不 clone/.material 路径 0；默认地图观察此前已证 0 runtime clone。
- ArenaDirector：art 模式 Dummy 基元视觉隐藏（一实体一视觉身份，§22）。

## 6. Art 证据元数据（§23-§25）

- 每 density 证据头：art_profile/formal_visuals=true/visual_instances/visual_type_count/visual_mix/resolved_visuals/renderer_instances/skinned_renderer_instances/material_slots/approx_vertices/approx_triangles——observation 无独立硬门槛；mix 求和==density 机器校验；resolved 集合明确记录。

## 7. 硬指标与解释边界（§28-§31）

- Art 继承 M7 同一硬预算：avg/p99/cpu/gpu ≤8.33ms、FrameTiming 必须、frames==600、alive≥95%；不 gate main max；GC/memory=观察项。
- 双层 verdict 分别输出（§31），canonical PASS 不覆盖 art FAIL。
- **Harness 边界更新（§54）**：`ART_VISUAL_RENDER_COST_NOT_REPRESENTED_BY_CANONICAL_HARNESS` 关闭，替换为更精确表述：**Canonical gameplay gate 仍为 Dummy；正式美术成本由独立 Art Performance Gate 覆盖（本层）**。

## 8. Gate 结果（双 Gate 同 sourceCommit=321d9b4）

| 层 | Gate A | Gate B |
|---|---|---|
| Canonical Performance | **PASS**（worst avg=1.884ms、worst p99=2.572ms） | **PASS**（worst avg=1.874ms、worst p99=2.596ms） |
| Art Performance | **PASS**（worst avg=6.031ms=72.4%、worst p99=7.483ms=89.8%、worst cpu=6.035ms、worst gpu=0.924ms） | **PASS**（worst avg=6.056ms=72.7%、worst p99=7.628ms=91.6%、worst cpu=6.059ms、worst gpu=0.926ms） |

- 300 档 alive 285-297/300（kill-then-refill canonical 策略，≥95% 达标）。
- 未出现 OOM/崩溃（§56）；GC/memory 趋势仅记录。

## 9. Evidence（§45-§52）

- Operator 扩展：`snapshot_performance_evidence.ps1 -EvidenceKind ArtPerformance`（默认 Performance 完全兼容，M8 -VerifyArchive 不受影响）；SelfTest 12/12（新增 art 正例+fallback 拒收+缺 profile 检测）。
- Gate A/B 各自 TEMP 快照（%TEMP%\GAME-ZZZ-ArtPerf-R7\gate-a|gate-b）→ VerifyArchive OK → 冻结入库 `docs/reviews/s3/art-performance-r7/gate-a|gate-b/`（各 15 files：3 runs×(3 densities+log)+summary+contract+profile+MANIFEST，artProfileSha256 记录于 MANIFEST）。
- 入库归档复核：`VerifyArchive OK (15 manifest files)` ×2。
- M7 1440p evidence 与 M8 revalidation evidence **0 diff**（§52）。
- SUMMARY.md 明确措辞边界（§53）：「正式 Enemy Visual Stress Mix 100/200/300 双 Gate 验证」，不声称「所有未来模型保证 300@120」「当前地图会刷 100 个 Bruce」。

## 10. SelfTest（§35）

- verify_unattended.ps1 SelfTest 新增 15 项 art 纯合成用例全 PASS（3×3 valid/instances 299→EVIDENCE_INCOMPLETE/mix 求和 299/missing fallback→EVIDENCE_INCOMPLETE/avg 8.34→FAIL/p99 8.34→FAIL/gpu>8.33→FAIL/canonical+art 双向组合/env mismatch→ENV_NOT_MET/base -IncludePerformance 不请求 art/resolved 解析确定性/未知 selectionMode 拒绝）。
- snapshot Operator SelfTest 12/12（原 9 项兼容 + art 正例/fallback 拒收/缺 profile 检测）。

## 11. 实际地图 observation（§55）

- R6 期间多次正式地图观察：8 Troll/8 FireLion/1 Bruce/6 Ashling placeholder、Hit/Ignite 反馈正常、runtime material clones=0、无可见卡顿（observation，不替代密度 Gate）。

## 12. 导演四项真相保留（§38）

本轮 0 回滚：Player base HP=9999999 保留、Town/出图后不自动 SpawnDummies(8) 保留、runInBackground=true 保留、Animator 同状态不重播修复保留（护栏：GameplayConstants_Guard/LoopTests 断言链）。

## 13. Scope（§60-§62）

- SpawnMap delta=0；新模型 delta=0（不导 TheTroll/Wolf/Bat/其它 Lion/新 Boss）；UI delta=0（Phase 3 R4 NOT STARTED）；Audio delta=0（Phase 4 GATED）；Content 数量 0 delta。

## 14. DECISIONS（§64）

- 长期规则⑭（最小新增）：Canonical Performance Gate 保持 Dummy gameplay baseline；正式敌人渲染/动画成本由独立 Formal Art Performance Gate 覆盖——Art Gate 继承同一锁定硬件、2560×1440 与 8.33ms 硬预算，并使用当前正式 EnemyVisualCatalog 映射的 distinct visuals 做高密度 stress；Art Performance FAIL 冻结进一步美术/内容扩张，不得通过降质量、减负载或放宽预算在同轮「修绿」。

## 15. Phase 状态（§65/§66）

- Phase 5：仍 **IN PROGRESS**，增加：**Formal Art Density Performance Gate = PASS**（不写 COMPLETE）。
- Ashling 保持 placeholder（不为 benchmark 选模）；第四视觉、Art optimization、LOD/纹理压缩、Phase 3 R4、Audio、Boss gameplay 均未启动（§69）。

## 16. Reviewer Findings / Fixes

- Findings：①verify_unattended SelfTest `$AP`/`$ap` PowerShell 大小写同名覆盖（变量不区分大小写）导致 art-resolved-deterministic 假失败——重命名 `$artProfileObj` 根除；②Snapshot Operator art 头校验需逐文件进行（mix 求和/instances==density 机器校验）——已实现并 SelfTest 覆盖。
- Fixes：如上，无遗留。
- Final verdict：**PASS — FORMAL ART DENSITY PERFORMANCE GATE ESTABLISHED**。

## 17. 提交（§68）

- feat(perf) stress profile+art harness → tooling(qa) art gate+operator+selftest → docs(perf) 本报告+DECISIONS⑭ → evidence/STATUS 按 provenance 习惯独立提交；普通 push 禁 force。
