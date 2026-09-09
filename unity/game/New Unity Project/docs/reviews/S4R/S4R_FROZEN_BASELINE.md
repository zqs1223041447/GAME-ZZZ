# S4R_FROZEN_BASELINE — S4 冻结基线快照（Baseline Reconciliation 产物）

**性质**：baseline assertion（对已存在事实的可追溯冻结），不是新 capability。任何条目如需变更必须走后续已批准 Work Order。
**建立**：S4R-WO-01（2026-09-09，工作 AI）。**来源**：`docs/reviews/STATUS.md`（2026-09-09 S4 收口态）+ `docs/qa/CONTENT_PRODUCTION_REPORT.json`（EditMode 测试再生）+ `docs/reviews/s4/S4_PLAN.md` + `docs/reviews/s4/S4_OVERALL_CLOSEOUT_REVIEW.md`。

## 1. 冻结事实表（claim → authoritative reference → evidence）

| # | 事实 | Authoritative / Evidence |
|---|---|---|
| 1 | Equip Slots = 6（Weapon/Body/Helmet/Boots + S4 新增 Gloves/Belt） | RUNTIME.md 装备节（本轮对齐）+ Production Report `counts.equipmentSlots=6`；S4-P2 复核 `docs/reviews/s4/S4_P2_EQUIPMENT_BREADTH_REVIEW.md` |
| 2 | Affixes = 17（13 不限槽 + 4 条 Gloves/Belt 专属；全部复用已有 Stat/ModOp） | Production Report `counts.affixes=17` + `affixApplicability`（unrestricted 13 / restricted 4 / bySlot 13×4+15+15）；S4-P3 复核 `S4_P3_AFFIX_BREADTH_REVIEW.md` |
| 3 | 槽位适用性 = 单一 applicability truth（AllowedSlots 位掩码 predicate；Drop/Craft/Audit/Report 全复用） | RUNTIME.md 装备节 + `AffixApplicabilityTests`；S4-P3 复核 |
| 4 | Canonical Skills = 3（Melee / Projectile / Area） | Production Report `counts.skills=3` + RUNTIME.md 施放流程节 |
| 5 | Canonical Supports = 7（Burning/Brutal/Focused/Swift/Combustion/Fork/FireConversion） | Production Report `counts.supports=7` + RUNTIME.md Support 节（含两个机制 Support 事实） |
| 6 | Support 兼容 = 21 组合中 16 兼容 / 5 不兼容，golden oracle parity OK | Production Report `supportCompatibility` + `SupportGateTests`（golden=独立 oracle，禁反向生成） |
| 7 | Passive Graph = 16 节点 / 20 边 / 0 断连；Start 免费；2 Notable（Brutal Strikes、Pyre）+ 1 机制（Cinder Heart 40% 物转火）；出图免费重置 | Production Report `passiveGraph` + RUNTIME.md 天赋节 |
| 8 | 玩家视觉 = DarkKnight（预制体缩放 0.24，Humanoid 重定向；贴地净高 ≈1.19；六态动画） | RUNTIME.md 玩家视图 + STATUS 玩家视图表 |
| 9 | 正式敌人视觉 = 4（Brute=Troll / Stinger=FireLion / Ashling=Gargoyle / Warden=Bruce；fallback=0/clones=0；非 benchmark EnemyKind 全覆盖） | Production Report `enemyVisuals` + Art Gate 证据（`docs/reviews/s3/art-performance-r9-gargoyle/`、S4 Final Recertification） |
| 10 | 战斗 SFX = 5 键全投放（Cast/Impact/Hit/Death/Loot，全 CC0；REQUIRED 10/10 真实加载 PASS） | Production Report `resources.requiredPresent=10/requiredMissing=0` + `docs/reviews/audio/SFX_SOURCES.md` |
| 11 | 人声 = 3 键映射 GATED（0/3 present，缺失=静音；待导演试听指认；DEFERRED BY DIRECTOR） | Production Report `resources.gatedMissing=3` + `docs/reviews/audio/DK_VOICE_INVENTORY.md` |
| 12 | S4 Final Canonical Performance：worst avg=1.903ms（22.8%）/ worst p99=4.840ms（58.1%）；CPU 1.903 / GPU 0.581；alive 97.7-98.3% | S4 Final Gate `-IncludeArtPerformance` @ b3eb9fe；STATUS 门状态表 |
| 13 | S4 Final Art Performance：worst avg=4.085ms（49.0%）/ worst p99=6.671ms（80.1%）/ GPU worst 1.281；resolvedVisuals=4 | 同上 |
| 14 | Production Simulation = 10k cycles invalid=0 + 同 seed hash exact match（FNV1A64:a1f075f251ec1070） | `docs/qa/PRODUCTION_SIMULATION_REPORT.json`（工具再生，禁止手填）+ `S4_P4_PRODUCTION_SIMULATION_REVIEW.md` |
| 15 | 测试基线 = EditMode 246/246、PlayMode 11/11 | 2026-09-09 S4R-WO-01 Quick Gate 复确认 PASS（`.\tools\verify_unattended.ps1` exit 0）；历史各轮 STATUS「最近一次绿灯」表 |
| 16 | 门状态 = S0/S1/S1手感/S2/S2技术/S2P 1080p/S2P 1440p-120 全放行或 CLOSED；S3=COMPLETE（Voice DEFERRED）；S4=COMPLETE | `docs/reviews/STATUS.md` 门状态表 + `docs/ROADMAP.md` 阶段表 |
| 17 | Canonical Data 来源 = 工程内版本化数据表（`ContentData/valid/` + `Catalogs.cs`）；**PoEDB 全量导入流水线未建立**（属 BDA Phase 1 范围，非当前事实） | RUNTIME.md 目录/ContentDatabase 节 + BDA 计划 §14 Phase 1 Scope |
| 18 | 确定性 = 玩法/内容随机只走 `SeededRng`；禁止 `UnityEngine.Random`；sim 可复现 | RUNTIME.md SeededRng 节 + DECISIONS.md 工程节 |

## 2. Hard-Stop Registry（冻结；只有导演明示才能变更）

| 硬停止 | 语义 | 来源 |
|---|---|---|
| S4 完成后硬停止 | 不启动 S5 / Progression Spine / Map Tier / Boss / Unique / Ring / Offhand / Amulet / 新 Content Batch / Voice——等待导演/协调席下一产品方向 | STATUS 门状态表（S4 行）+ S4_PLAN §8「完成 S4 后不自动启动 S5」 |
| Voice DEFERRED BY DIRECTOR | Phase 4 人声被导演延期排除出完成范围；S3/S4 均不改 VoiceCues / voice 资源 | STATUS 门状态表（S3/S4 行）+ S4_PLAN §7 |
| Stage0 锁 | 大天赋树 / Unique library / Atlas / full-deep Craft / DOTS / HDRP / FMOD / character customization 继续锁，每项需导演单独明确解禁 | S4_PLAN §1（延续） |
| Performance Gate 冻结语义 | 正式敌人美术/内容扩张受 Art Gate 与 Canonical Gate 冻结约束；FAIL 时因果修复，不降质修绿不放宽预算 | DECISIONS 长期规则⑭⑯ + COMBAT/PERF 契约 |
| 导演门控待输入表 | ROADMAP「导演门控待输入」节各项「无输入则不做」 | ROADMAP 门控表 |

## 3. Forbidden Expansion Registry（S4R 周期内 FORBIDDEN / NOT AUTHORIZED）

S5 gameplay implementation；Progression Spine；Map Tier；Boss implementation/content；Unique；Ring；Offhand；Amulet；New Content Batch；Voice activation/release；Curse；Flask；Jewel；Atlas；New Class；Deep Craft；未批准 Endgame；未授权新 EquipSlot；未授权新 Skill/Support；未授权新 Affix family；未授权新 progression currency/resource；dormant future gameplay system；PoEDB 替代数据源；「清理/refactor/为未来准备」名义的新接口语义、新 gameplay 状态、新存档字段、新掉落/装备/progression hooks、inactive-but-functional future systems。

（与 `S4R_PLAN.md` §5 同源；此处为 registry 形式，供逐单 Forbidden Expansion Audit 对照。）

## 4. 已知限制（如实保留，不构成 FAIL）

- Death 视觉受 0.40s 回收截断（non-blocking known limitation，S3 Phase 5 收口记录）。
- 人声 3 键 0/3 present（GATED，非缺陷；导演未指认）。
- 未使用 Tag 4 个（Spell/Projectile/Fire/Duration）=已声明预留，不是任务（DECISIONS 预留 Tag 钉死）。
- Harness 代表性：canonical Performance Gate 为 Dummy baseline（正式美术成本由 Art Gate 覆盖——设计如此，长期规则⑭）。
- 1440p/120 结论只适用锁定硬件（Ryzen 7 5700X3D / RTX 5070）+ canonical 契约。
