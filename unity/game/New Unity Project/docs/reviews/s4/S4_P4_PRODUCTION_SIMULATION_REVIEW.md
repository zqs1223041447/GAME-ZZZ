# S4_P4_PRODUCTION_SIMULATION_REVIEW — Production Simulation / Scale Proof（S4_PLAN §八；协调席 2026-09-09 工作指令）

**日期**：2026-09-09　**Baseline**：main @ 2ec290d（S4 Phase 3 COMPLETE）→ 本轮 HEAD 见文末
**Verdict**：**PASS — S4 PHASE 4 PRODUCTION SIMULATION COMPLETE**（10,000 cycles invalid=0 + deterministic hash 一致；Phase 5 未自行启动）

## 1. Simulator 架构（§八：扩现有工具，禁止再建一套）

- `ProductionSimulator`（Game.Tests.EditMode，纯 tooling/测试层）：**驱动真实 Drop/Craft/Equip 路径**——RollItem（DropGear/TryRandomCraft 共用生成路径）、TryRandomCraft、TryDirectedCraft、TryEquip、RecalcPlayer——**零第二套生成引擎、零平行 truth**。
- 10,000 cycles = 250 会话 × 40 cycle（会话=SeedTown 派生种子 + LootRng；decision rng 独立流；库存 24 上限天然不溢出）；cycle 类别=drop 40% / randomCraft 25% / directedCraft 20% / equip 15%（decision SeededRng 决定，全程 SeededRng 唯一随机源）。

## 2. Run 结果（seed=20260909，docs/qa/PRODUCTION_SIMULATION_REPORT.json 工具再生）

| 项 | 值 |
|---|---|
| iterations | **10,000** |
| cycle 分布 | drop 3891 / randomCraft 2505 / directedCraft 2002 / equip 1602 |
| slot 分布 | Weapon 640 / Body 671 / Helmet 638 / Boots 634 / **Gloves 643 / Belt 665**（六槽全 reachable） |
| rarity 分布 | ordinary 1992 / rare 1899 |
| affix 分布 | 旧 13 条各 ~1290-1390；**新 4 条：SwiftGrip 130 / KeenEdge 116 / Bulwark 118 / VitalWeave 124**（大样本可达性实证） |
| **invalidCount** | **0** |
| rejectedDirectedCraft | 433（=物品内 duplicate guard 预期 fail-safe 生效次数，信息项非失败；每次拒绝后物品经复检仍合法） |
| **deterministicHash** | FNV1A64:a1f075f251ec1070 |
| repeatHashMatch | **true**（同 seed 重跑 hash/distribution 逐项一致） |

## 3. 逐项验证清单（§八 minimum）

| 要求 | 结果 |
|---|---|
| deterministic replay | ✓ 同 seed 双跑 hash/分布逐项一致（`Simulation_SameSeed_HashIdentical`） |
| no NaN | ✓ 聚合 MaxLife/MaxMana + 全部词缀值 finite 校验，0 违规 |
| no invalid slot | ✓ 槽位 ∈ [0,6) 且孔数=槽位契约，0 违规 |
| no incompatible affix | ✓ 唯一 predicate IsApplicable 校验，0 非法对 |
| no stale second-value data | ✓ 单行词缀第二值必须为 0、双行第二值 ∈ [Min2,Max2]，0 违规 |
| no impossible item | ✓ 词缀数 ∈ [0,4]、rarity/socket/base 合法，0 违规 |
| no invalid duplicate/conflict | ✓ 物品内词缀 ID 唯一——**发现并修复既有缺口**（见 §4） |
| craft result always valid | ✓ TryRandomCraft 产物保持 Rare+同槽+全部合法 |
| all generated affix Stats consumed | ✓ 全部行 Stat ∈ RuntimeConsumedStats 白名单 |
| fixed seed output reproducible | ✓ FNV-1a 64 hash 一致；异 seed hash 不同（sanity） |

## 4. 发现项与修复（Reviewer）

- **发现**：首跑 10k cycles 报 505 例「物品内重复词缀」——全部来自 `TryDirectedCraft`：定向制作可向已含同词缀的装备重复写入（S2 时代既有语义，此前无任何测试/审计覆盖此维度）。
- **修复**：`TryDirectedCraft` 新增 duplicate guard——物品内已存在同词缀 → **deterministic reject**（不消耗蚀刻剂、不半写入），与 applicability reject 同一语义（不强行添加/不自动替换/不消耗后失败）。
- **回归影响**：S3 既有测试 `DirectedCraft_SingleRowOverHybrid_NoSecondValueResidue` 的夹具物品 slot0 与覆写词缀同为 Life——触发新 guard；按新契约调整夹具（slot0 改 IncPhys），**覆写末槽+第二值归零语义原样保留并继续被该测试证明**。
- **新增回归测试**：`DirectedCraft_RejectsDuplicateAffix_WithoutConsuming`（重复拒绝/零消耗/物品内该词缀仍唯一）。
- Fixes：如上，无遗留。

## 5. Verification（S4_PLAN §五 Gate 框架）

- SelfTest **PASS**；EditMode **246/246**（+3：10k 全绿/同 seed 一致/异 seed 不同 + duplicate reject 回归）；PlayMode **11/11**（零改动，纯 tooling 轮）；Audit **fresh PASS**；Build **PASS win64**；PlayerRuntime **PASS exit=0**（densities=100/200/300；本轮回放分辨率 1920×1080，PlayerRun 契约全 PASS）；tracked clean。
- Performance required：**NO**——模拟器纯测试层（Game.Tests.EditMode），runtime 产品代码仅 TryDirectedCraft 增加 O(4) 词缀扫描（配置路径，非热路径）→ 未跑 canonical Performance。
- Art Gate：**N/A**（visuals/renderers/Animator/ProjectSettings 0 diff）。

## 6. Scope（§三十一 语义延续：本阶段不再加玩法）

- Content delta：**0**（无新 Affix/EquipSlot/Skill/Support/Passive/Enemy/MapMod）；Gameplay delta：TryDirectedCraft duplicate guard（fail-safe 收口，非行为扩张——正常制作流程不可见）；UI/Art/Audio delta：0；Voice 继续 DEFERRED；Stage0 全锁。

## 7. Phase Status

- Phase 0/1/2/3/**4**=**COMPLETE**；Phase 5（S4 Integrated Closeout：四门+Art Gate 全跑回归）=NOT STARTED 待协调席令；S4 overall=IN PROGRESS；S3=COMPLETE；Voice=DEFERRED BY DIRECTOR。

## 8. Remote Sync

- Commits：本轮 3 笔（simulator+tests / duplicate guard 修复 / docs）；普通 push，未 force；Local HEAD=Remote HEAD（见完成汇报）。
