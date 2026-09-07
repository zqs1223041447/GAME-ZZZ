# S3-M8 PERF EVIDENCE INTEGRITY — 复核报告

工作令：S3-M8-PERF-EVIDENCE-INTEGRITY（M7 Closeout Integrity — Repository Truth + Performance Evidence Provenance）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**PASS**（Q1-Q8 全达标；内容 delta=0；性能 workload/threshold/contract 零改动；两个 M8 canonical Gate 全 PASS 且原始证据 provenance 完整冻结）。

## 1. Baseline

- Baseline=3be0d7a（M7 STATUS 回填，M7 收口后 HEAD）。
- 进入本轮时事实：S2P 1080p=已证；S2P 1440p/120=CLOSED（M7 locked hardware）；M7 Performance Gate=PASS；S3 Phase 1/2=COMPLETE；Phase 3/4/5=GATED/NOT STARTED。

## 2. M7 Historical Truth（不改写、不否认）

- M7 当时：Gate A PASS、Gate B PASS、S2P 1440p/120 CLOSED（锁定硬件）。
- M7 冻结包 `docs/reviews/s2p/1440p-120-m7/`（SUMMARY+契约快照+gate-b 原始 12 文件）零字节改动。
- M7 历史边界如实保留：Gate A 原始 txt/log 为 ephemeral（已不存在），完整数字存于 SUMMARY/复核文档；**M8 不补造 M7 Gate A 原件**（伪造=零容忍）。

## 3. Stale Current-State Findings（定向搜索，词表=1440p/120/以后再说/挂起/不补测/不宣布/NOT CLOSED/NOT_EVALUATED/IncludePerformance）

| 文件 | 发现 | 处置 |
|---|---|---|
| STATUS.md 测量段 | 「不宣布 120FPS@1440p；1440p 补测挂门控表」与 S2P 行 CLOSED 自相矛盾 | **Current→修正**（3a577d3）：CLOSED+边界声明 |
| STATUS.md 校验段 M6 行 | 「1440p/120 维持未结案、性能未判定」被读成当前态 | 加「（M6 当时状态；后由 M7 CLOSED）」，历史不改写 |
| S3_PLAN.md 阶段 6 行 | 「导演提供 1440p 环境（当前挂起「以后再说」）」失效 | **Current→修正**：已完成 CLOSED 不再是待开启项+历史演进保留 |
| S3_PLAN.md 阶段 7 | 「维持挂起、不补测、不宣布」失效 | **Current→修正**：COMPLETE/CLOSED（M7 locked hardware）+仍非 Phase 3-5 自动前置+锁定硬件边界 |
| ROADMAP.md S2P 行/checklist/导演门控表 | 三处「1440p 被钳制待补/未宣布/保留后续测试」失效 | **Current→修正**（历史裁定保留+条件变化说明） |
| UNATTENDED_VERIFICATION.md | 头部演进缺 M7；canonical command 块只有 3 行 | 补 M7（四层齐备）+第四行 `-IncludePerformance` |
| S2P_CLOSEOUT.md 阻塞段/结论段 | 「未结案/不宣布」为 M5/M6 时代文本 | 加「当时状态」标注，指向 Final Closeout |
| S3_PHASE2_CLOSEOUT.md | 「1440p 补测：硬件挂起」 | 加「2026-09-07 当时状态」标注 |
| DECISIONS.md 旧条目（11/19/21 行等）、M4-M7 复核、PERFORMANCE_BUDGET 语义表、QA 四层表 NOT_EVALUATED 列 | **Historical** | 保留不动 |

## 4. Docs Fixes（提交 3a577d3，7 文件）

STATUS（测量段+M6 标注）、S3_PLAN（Phase 6/7）、ROADMAP（3 处）、UNATTENDED_VERIFICATION（头部+命令块+新增 Performance Evidence Capture 节：固定顺序 run→confirm PASS→snapshot→next gate（next gate 清 temp）→VerifyArchive；命令块四行齐备）、S2P_CLOSEOUT、S3_PHASE2_CLOSEOUT、DECISIONS（长期规则⑨ Verifier≠Historian）。M8 **不新增第五层 Gate**。

## 5. M7 Evidence Provenance Gap（如实记录）

M7 Gate A raw=temp 契约下被下一次运行清理（只剩抄表数字）；Gate B raw 完整冻结。该不对称是**历史事实**，M7 冻结包照旧保留。M8 修复的是**未来**：新增可重复操作链（Gate→立即快照→下一 Gate→VerifyArchive），本轮已按该链完整执行两次。

## 6. Snapshot Tool（tools/snapshot_performance_evidence.ps1，提交 ead815c）

- 职责：显式 Operator。只在明确调用时把当前 temp 的 **PASS** 结果复制为冻结归档；不运行 Unity、不修改性能结果、不判定新预算；**不隶属 verify_unattended.ps1**。
- 拒收矩阵：非 PASS verdict（FAIL/ENV_NOT_MET/EVIDENCE_INCOMPLETE/INFRA/NOT_EVALUATED）；缺 run/缺密度/缺 PlayerRun.log；证据头与 contract 逐字段不匹配（density/resolution/editor=False/dx/hardware_cpu+gpu/quality/vsync/targetFps/warmup/sample/castInterval/13 列）；destination 已存在（**无 -Force**，append-by-new-directory）；working tree 有 Assets/Runtime、ProjectSettings、docs/qa/PERFORMANCE_GATE.json 未提交修改（docs/tools 脏允许）。
- MANIFEST.json：schemaVersion/sourceCommit/contractSha256/verificationSummarySha256/expectedRuns/expectedDensities/performanceVerdict/environmentStatus/capturedAtUtc（仅记录）/files[]（relativePath+sha256+bytes）。
- 复制范围：3 Run ×（100/200/300.txt+PlayerRun.log）+verification-summary.json+PERFORMANCE_GATE.json=14 文件；**永不复制 Player 构建产物**（exe/Data/pdb 不入库）。
- `-VerifyArchive`：只读，存在性+bytes+SHA-256+契约快照完整；SelfTest 1c 篡改夹具证明可检出。
- 职责分离落成规则：DECISIONS 长期规则⑨ Verifier≠Historian。

## 7. SelfTest（Snapshot Tool）

**9/9 PASS**（exit=0）：①合成 PASS 树→快照+归档校验+篡改检测；②FAIL verdict 拒；③ENV_NOT_MET 拒；④缺 Run2 拒；⑤缺 300.txt 拒；⑥缺缺 PlayerRun.log 拒；⑦destination 已存在拒；⑧GPU/契约不匹配拒；⑨summary 含 issues 拒。另有真机预检：M7 Gate B temp 证据 14 文件快照+VerifyArchive 全 OK（临时目录，未入库）。

## 8. Phase-A Source HEAD 与振荡事件（如实记录）

- Phase A 提交链：92a71a9（chore：define 移除，源自 Quick 门实测清理）→ ead815c（tooling：snapshotter）→ 3a577d3（docs truth）→ **Evidence HEAD=a11aa87（chore：define 终态收敛回含 define）**。
- **ProjectSettings define 振荡矩阵实测更新**：Quick 门（纯测试实例）确定性移除 `SENTIS_ANALYTICS_ENABLED`；Performance/Build 门（构建实例）确定性回写。单一 HEAD 态无法同时满足两类门零污染——M5 b79c77c 同型矩阵重现，**3de5d24（submitAnalytics=0）的钉死假设本轮被实测证伪**（Library 机器级同意状态漂移）。
- Gate A 第一次尝试（HEAD=3a577d3，define 已移除态）：门 PASS 但终态 ProjectSettings 被构建实例回写弄脏，快照 Operator **按契约拒收**——该尝试作废未冻结，数字不进入任何证据包。
- 收敛决策：M8 跑的是 Performance 门，其**终态=含 define**（测试段移除被构建段回写抵消），Evidence HEAD=a11aa87（含 define）。重跑后 Gate A/B 终态树均干净（实测）。
- 该 define=孤儿符号（71601fd：manifest 无 sentis/ml 包、全工程零引用），gameplay 影响=0。长期修复=移除 com.unity.ai.inference 间接依赖（M5 复核遗留建议）——**超出 M8 范围，列为建议后续任务**。

## 9. M8 Gate A（HEAD=a11aa87，exit=0，PerformanceVerdict=PASS）

- Environment=PASS（2560×1440 fullscreen/D3D12/PC/vSync=0/targetFps=-1，CPU/GPU=锁定值）。
- 9/9 密度×Run 全 PASS。worst p99=4.319ms（Run3/100 尖峰，预算 51.9%）；worst avg=1.671ms；alive 96.7%-100%；FrameTiming 9/9。
- **Archive**：`docs/reviews/s2p/1440p-120-m8-revalidation/gate-a/`（14 文件=MANIFEST.json+PERFORMANCE_GATE.json+verification-summary.json+Run1-3×4 原始文件）；**VerifyArchive=OK（14 manifest files）**。

## 10. M8 Gate B（同一 HEAD=a11aa87，exit=0，PerformanceVerdict=PASS）

- Environment=PASS；9/9 全 PASS。worst p99=4.380ms（Run2/100 尖峰，预算 52.6%）；worst avg=1.678ms；alive 96.7%-97.0%（300 档）与满员；FrameTiming 9/9。
- **Archive**：`.../gate-b/` 同结构 14 文件；**VerifyArchive=OK**。

## 11. Same-HEAD Proof

- gate-a MANIFEST.sourceCommit = gate-b MANIFEST.sourceCommit = `a11aa8737669e188f453345b18414855c36d0955` = Phase-A 干净 HEAD（实测两 JSON 逐字节比对）。
- 契约未变：live `docs/qa/PERFORMANCE_GATE.json` 与 M7 冻结快照 blob SHA 一致（ec958d3206fc4ab16acd1effbf969d8eeb573752）；M8 每归档内契约快照 SHA-256 记录于 MANIFEST。

## 12. Current Performance Verdict

- **M8 revalidation=PASS**：18/18 测量在预算内（worst avg=1.678ms=20.1%、worst p99=4.380ms=52.6%、cpu/gpu≤1.678/0.580、alive≥96.7%、FrameTiming 18/18）。
- **S2P 1440p/120 = CLOSED（M7 locked hardware）维持不变**；M8 不重定义预算、不降阈值、不新增目标（workload Densities=100/200/300、Warmup=60、Sample=600、Cast=0.12 全部未动）。
- 与 M7 数字差异（worst p99 2.388→4.380）=自然波动（同为尖峰型 p99，均 <52% 预算），不构成退化信号；M8 按工作令三十二不比较 exact 值。

## 13. Anti-Cheat Diff（3be0d7a..a11aa87 实测）

- `Assets/Runtime` diff=**EMPTY（0 文件）**；`ArenaPerfHarness.cs` diff=EMPTY；`docs/qa/PERFORMANCE_GATE.json` diff=EMPTY；`docs/qa/PERFORMANCE_BUDGET.md` diff=EMPTY。
- 变更文件全集=7 docs + 1 新 tool（+ProjectSettings define 净零往返）。Densities/Warmup/Sample/CastInterval/Quality/vSync/targetFps/budget/minAliveRatio/locked CPU+GPU 全部未变。
- Gameplay 0 files modified；Scene/Prefab/ContentData 0。

## 14. Content Delta

Skill=0 Support=0 Affix=0 Passive=0 Enemy=0 Resource=0 Scene=0 Stat=0 Tag=0 Effect/Event/Condition=0。Phase 3/4/5 未启动；新 Content Batch 未开始。

## 15. Repository Pollution

- 允许新增/修改：snapshot tool、docs（truth+QA+DECISIONS+STATUS+S3_PLAN+ROADMAP+S2P/S3_PHASE2 标注）、M8 证据归档、M8 复核——与工作令四十三允许清单一致。
- Temp 未误入库；Player Build 产物未入库；非项目未跟踪项（素材清单类 4 项）未动。
- ProjectSettings define 往返（92a71a9→a11aa87）=净零，事件见第 8 节，如实记录。

## 16. Remaining Risks

- 中：define 振荡矩阵（Quick=移除/Build=回写）——Quick 门在含 define HEAD 下终态脏；后续维护轮若需 Quick 零污染须处理（根治=移除 com.unity.ai.inference 间接依赖，另行工作令）。
- 低：snapshot Operator 的拒绝路径依赖 git 可用（git 缺失=拒收，INFRA 语义，安全侧）。
- 低：M8 尖峰 p99（4.319/4.380）仍远低于预算；如未来常态化逼近 8.33 需按 PERFORMANCE_BUDGET 语义处理。

## 17. Q1-Q8 必查

- **Q1** 新 AI 看 STATUS 会同时读到 CLOSED 与「不宣布 1440p/仍挂起」吗？**NO**（测量段已修为 CLOSED+边界；M6/M5/M6 时代文本全部带「当时状态」标注）。
- **Q2** 新 AI 看 S3_PLAN 会认为 1440p 仍等导演环境吗？**NO**（Phase 6 行=CLOSED 不再待开启；Phase 7=COMPLETE/CLOSED+历史演进保留）。
- **Q3** 新 AI 看 QA 首页能直接看到 Performance canonical command 吗？**YES**（命令块四行，第四行 `-IncludePerformance` 带注释）。
- **Q4** Gate A 跑完后、Gate B 清 temp 前有完整 raw snapshot 吗？**YES**（gate-a 14 文件冻结于 Gate B 启动前，VerifyArchive OK）。
- **Q5** snapshot 工具会把 FAIL evidence 存成 PASS archive 吗？**NO**（SelfTest 2/3/8/9 全拒）。
- **Q6** archive 被手工修改后 manifest verification 抓得出吗？**YES**（SelfTest 1c 篡改夹具=SHA-256/bytes 失配检出）。
- **Q7** verify_unattended 会自己偷偷写 repo evidence 吗？**NO**（Verifier 只输出 temp；冻结写入唯一入口=显式 snapshot Operator；脚本头注释+QA 文档+DECISIONS 规则⑨三重钉死）。
- **Q8** M8 改变了性能 workload/threshold/contract 吗？**NO**（13 节 diff 实测全 EMPTY；契约 blob SHA=M7 快照）。

## 18. 复核结论

Repository Truth 修正完成且无一历史改写；证据链可重复（Gate→快照→VerifyArchive）且本轮已实证两次；双 Gate 同 HEAD PASS；内容与性能契约零改动。**Verdict=PASS**。
