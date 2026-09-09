# S4_OVERALL_CLOSEOUT_REVIEW — Production Scale & Itemization Breadth 总收口（工作令 S4-P5-INTEGRATED-CLOSEOUT）

**日期**：2026-09-09　**Candidate HEAD**：5ec04bb（clean）
**Verdict**：**S4 REMAINS IN PROGRESS — ENV BLOCKER: LOCKED-HARDWARE DISPLAY MODE NOT MET（1920×1080 虚拟显示 ≠ 合同锁定 2560×1440）**
（Branch B——工作令 §四十二。非项目回归：Gate 1/2/3 全 PASS + Simulation hash exact match；仅 Gate 4/Art Gate 因执行环境显示模式无法满足锁定合同而 ENV_NOT_MET。不得放宽合同（规则⑦/§四十四）；需导演/协调席裁定：恢复 2560×1440 显示模式，或按规则⑦走正式合同变更工作令。）

## 1. Baseline & S4 Goal（§一/§二）

- 候选 HEAD=5ec04bb（从 repo 当前 HEAD 核实）；S4 目标=Production Scale & Itemization Breadth（`S4_PLAN.md`）。
- Milestone 真相核实：Phase 0/1/2/3/4=COMPLETE（各自评审在 `docs/reviews/s4/`）；Phase 5=本轮。

## 2. Canonical Content Snapshot（§三/§四）

- Skills=3 / Supports=7 / Affixes=**17** / Passives=16 / EnemyKinds=5 / MapMods=3 / **EquipSlots=6**（Weapon/Body/Helmet/Boots/**Gloves/Belt**）——全部以 Catalog/enum 现数据为准（Production Report 再生，未手填）。
- S4 新增范围核对：Phase 2=+Gloves/Belt；Phase 3=+SwiftGrip/KeenEdge/Bulwark/VitalWeave；Phase 4=+Production Simulator+DirectedCraft duplicate guard。**意外 scope creep：无**。

## 3. Stable Contracts（§五/§六）

- EquipSlot：旧四槽 numeric ID=0-3 保持、Gloves=4/Belt=5 只追加、Count=6；未重排。
- Affix：旧 13 IDs/order 未漂移；新 4 只追加；applicability 唯一 canonical predicate=`AffixDef.IsApplicable(EquipSlot)`。

## 4. Eligibility Single Truth Audit（§七）

全仓确认同一 eligibility truth 消费者：Drop（RollItem）/ Random Craft（TryRandomCraft→RollItem）/ Directed Craft（TryDirectedCraft）/ Content Audit（ValidateAffixSlots+池派生）/ Production Report（affixApplicability 派生）/ Production Simulation（Validate 复用 predicate）。**平行 slot-affix predicate：0**。

## 5. Directed Craft Contract（§八）

Phase 4 起正式规则（canonical baseline）：物品已含同 Affix → deterministic reject + 物品不变 + 零资源消耗 + 零半写入；slot applicability reject 同 fail-safe。现存测试覆盖：`DirectedCraft_RejectsDuplicateAffix_WithoutConsuming` + `DirectedCraft_RejectsIllegalCombination_Deterministic`（未造重复测试）。

## 6. Six-slot Full Loop（§九/§十/§十一）

- 六槽全进入：generation/drop（OnKill→DropGear 6 槽可达种子扫描+10k sim 槽分布 634-671/槽）/craft（同槽重掷+定向 reject）/equip/replace（聚合正反双向断言）/modifier aggregation（RecalcPlayer+CollectSkillMods）/Build snapshot（6 槽字段）/Tooltip+Compare（canonical key=(int)slot）/drawer UI（2×3 DisplayOrder）。
- 旧四槽回归：行为零变化（pool=13 等价/抽屉几何断言/snapshot 字段原名原序/测试全绿）。
- Gloves 池=13+迅握+锋锐（Bulwark/VitalWeave 不可 roll）；Belt 池=13+壁垒+韧脉（迅握/锋锐不可 roll）；旧四槽不获得 4 条 restricted——测试+10k sim 双实证。

## 7. Existing Stats Only（§十二）

StatId/ModOp/Condition/Effect/Trigger delta=0；4 条新词缀全部由 Runtime-consumed Stats 组成（白名单+消费点实证）。

## 8. Production Tooling（§十三/§十四/§十九）

- **Content Production Report**：Closeout candidate HEAD 重新再生（EditMode 测试）；deterministic ✓；**verdict PASS**；equipmentSlots=6 / affixes=17 / restricted=4 / unrestricted=13 / 六槽池 13×4+15+15 / unconsumedDeclaredStats=0 / oracle parity OK / passive disconnected=0 / REQUIRED 10/10 / formal visuals 4/4 / Voice deferred 不失败。与 committed 版本 **byte 级一致**（无 stale/drift）。
- **Production Simulation Report**：candidate HEAD 重新再生；PASS；seed=20260909 / iterations=10,000 / 全部指标同 P4（§十/§十六 全绿）。
- 未手改任何报告；两报告职责分离未合并。

## 9. Production Simulation Final Replay（§十五/§十六/§十七/§十八）

- Closeout candidate HEAD 重跑完整 10,000-cycle simulation（canonical seed=20260909）。
- Expected contract：iterations=10000 ✓ / 六槽全达 ✓ / **17 Affixes 全部 reachable**（分布 116-1388）✓ / invalid items=0 ✓ / invalid slot-affix=0 ✓ / invalid rows=0 ✓ / nonfinite=0 ✓ / out-of-range=0 ✓ / stale second value=0 ✓ / illegal duplicates=0 ✓（duplicate guard 后）/ unconsumed generated Stats=0 ✓ / exceptions=0 ✓ / 全部操作类 >0 ✓。
- **Deterministic hash：FNV1A64:a1f075f251ec1070 = P4 hash exact match**（本轮零 production semantics change，§十七 合规；未刷新 expected）。

## 10. Gates（§二十八-§三十）

| Gate | 结果 |
|---|---|
| Gate 1 SelfTest | **PASS**（全项含 art 23 项） |
| Gate 2/3 `-IncludePlayerRun` | **全 PASS**：EditMode **246/246** / PlayMode **11/11** / Content Audit **fresh PASS** / Content Production Report **PASS** / Production Simulation Report **PASS** / Build **PASS win64** / PlayerRuntime **PASS exit=0** / tracked clean |
| Gate 4 Canonical Performance | **ENV_NOT_MET**（见 §11） |
| Art Gate | **NOT_EVALUATED**（canonical 未 PASS 则 Art 不评估，工具语义） |

## 11. ENV BLOCKER（§四十四：先判定，不放宽）

- 现象：`-IncludeArtPerformance` 3 次 run 全部 **ENV_NOT_MET**——player 以 `-screen-width 2560 -screen-height 1440 -screen-fullscreen 1` 启动（gate 按合同正确请求 1440p），实测 resolution=**1920×1080** ≠ 锁定 2560×1440。
- 根因判定：**执行环境显示模式变更，非 measurement issue、非项目 regression**。机器当前活动显示链=GameViewer/OrayIdd 远程虚拟显示（1920×1080@144；RTX 5070 输出同 1920×1080@165）——R9 时代（2026-09-08）机器为真实 2560×1440 显示。虚拟显示不接受 1440p 全屏切换 → Unity 回落 1080p。
- 顺带记录（非指标问题）：性能余量本身健康——18 次测量 avg 1.47-1.88ms / p99 2.27-2.67ms / gpu ~0.35ms（预算 8.33），alive 293-295/300（≥95% 阈值内）；但**分辨率不满足锁定合同即 ENV_NOT_MET，不得据此放行**（规则⑦/规则⑥）。
- 已尝试（记录）：PowerShell ChangeDisplaySettings/EnumDisplayDevices 恢复 2560×1440——本会话（远程虚拟显示链）下枚举/切换不可用，未改动导演显示环境。
- 解除路径（需导演/协调席裁定，执行端不自行裁定）：①导演将显示恢复 2560×1440（R9 状态）后重跑 `-IncludeArtPerformance`；或 ②按规则⑦走正式合同变更（显式更新锁定环境）。**不得放宽预算/density/contract/workload。**

## 12. S3 Regression Boundary（§二十三）

Formal UI 仍 COMPLETE（六槽 Drawer=S4 bounded extension，Phase 3 未重开）；Formal Art 仍 COMPLETE（四正式视觉不变，P5 轮零美术 diff）。

## 13. Director Truths / Voice / Stage0 / Known Limitations（§二十四-§二十七）

- Player HP=9999999 ✓ / Town auto dummies removed ✓ / runInBackground ✓ / Animator same-state replay fix ✓ / licensing 非 Gate ✓ / 无人值守工作流 ✓——全保持。
- Voice：**DEFERRED BY DIRECTOR**（零触碰；不实现/不标 blocker）。
- Stage0 全锁；S4 未开启 Ring/Offhand/Amulet/Tier/Prefix-Suffix/Boss/Progression Spine（均非 blocker）。
- Known limitations：S3 已接受项全保留（Death 截断/单击闪/极窄窗边角）；本轮无新增。

## 14. Evidence（§三十五/§三十六/§三十七）

- **R7 VerifyArchive：OK（15 files）× gate-a/gate-b**；**R9 VerifyArchive：OK（15 files）× gate-a/gate-b**（旧 evidence 零修改）。
- S4 final evidence：本轮 Art Gate 因 ENV blocker 未产出 PASS 证据——**不冻结非 PASS 快照**（Verifier≠Historian，规则⑨）；解除 blocker 后按 §三十六 记录 sourceCommit/verdicts/worst avg·p99/CPU/GPU/resolved visuals。
- Gate 1/2/3 证据引用本轮 gate 输出（`PerformanceVerdict=NOT_EVALUATED`、非 PASS 不冒充）。

## 15. Content/Gameplay/UI/Art/Audio Delta（§四十五-§四十七）

- 全 **0**（Closeout 轮零代码 diff；git tracked 工作树干净；reports 由测试再生 byte 一致）。

## 16. STATUS / PLAN / Hard Stop（§四十八/§四十九）

- STATUS：**S4 仍 IN PROGRESS**（blocker 如 §11 如实记录）；Phase 5=IN PROGRESS（blocked）；Phase 0-4=COMPLETE。
- `S4_PLAN.md` 回填 milestone 状态（目标定义不改）。
- **硬停止**：S4 PASS 前不启动 S5/Progression Spine/Map Tier/Boss/Unique/Ring/Offhand/Amulet/新 Content Batch/Voice。

## 17. Reviewer

- Findings：①Gate 1/2/3 全绿一次通过（Closeout candidate HEAD 与 P4 提交一致，零回归）；②两报告再生与 committed byte 级一致（§十七 hash exact match 达成）；③唯一 blocker=环境显示模式（非项目缺陷），已按 §四十二 Branch B 如实呈现。
- Fixes：无（本轮零代码 diff——§五十 合规：仅 docs 提交）。
- Final verdict：**S4 REMAINS IN PROGRESS — ENV BLOCKER: LOCKED-HARDWARE DISPLAY MODE NOT MET（待导演恢复 2560×1440 或正式合同变更令）**

## 18. Remote Sync

- Push：是（docs 提交，普通 push 未 force）；Local HEAD=Remote HEAD（见完成汇报）。
