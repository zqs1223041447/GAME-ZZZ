# S4R_MECHANIC_MATRIX — Mechanic Support Matrix（首次正式建立）

**性质**：按机制族追踪实现状态（BDA 计划 §4.2 Passive / §4.3 Support + 系统机制族）。数据已导入 ≠ Runtime Supported；未支持机制的实现优先级由 Reference Build Coverage Gap 驱动——**当前 Reference Build Targets 尚未锁定（BDA Phase 1 范围），故 Reference Build Coverage 一律 N/A，不构成 Gap 判断**。
**建立**：S4R-WO-01（2026-09-09）。**本轮 reconciliation 结论**：与 runtime/test evidence 一致；无未经批准的 mechanic 晋级；**mechanic-support delta = NONE**。

状态：Supported / Partial / Unsupported / Blocked（同 Capability Ledger 定义）。

## 1. Passive 机制族

| 机制 | 状态 | 依赖系统 | 测试 | 受影响内容 | Blocked 原因 / Resume Trigger |
|---|---|---|---|---|---|
| Passive 节点图（16/20/0 断连） | Supported | PassiveCatalog、StatBag | Production Report `passiveGraph`（ProductionContentReportTests 再生）+ 图连通 audit 失败类别 | 全部 16 节点 | — |
| 免费节点 Start / 出图免费重置（TryRespec） | Supported | SliceSession、BuildSnapshot | Build/Respec 契约测试（EditMode） | Start 节点 | — |
| Notable：Brutal Strikes / Pyre | Supported | StatBag、CombatMath | CombatMathTests + 消费 Stat 白名单（unconsumedDeclaredStats=0） | 2 Notable | — |
| 机制节点：Cinder Heart（40% 物转火） | Supported | 转换轴（与 Support FireConversion 共享） | GV-CONV 系列 + 聚合测试（0.50+0.40→0.90） | 1 节点 | — |
| 大天赋树 / 完整 Passive Tree 镜像 | Unsupported | PoEDB Import Pipeline（未建立） | — | — | Director 未给方向；Resume=BDA Phase 1 canonical pipeline 建立后按 Coverage Gap 调度 |
| Mastery / Ascendancy | Unsupported | 同上 | — | — | 同上；S4R 内 Forbidden |

## 2. Support 机制族

| 机制 | 状态 | 依赖系统 | 测试 | 受影响内容 | Blocked 原因 / Resume Trigger |
|---|---|---|---|---|---|
| Stat 类 Support（Burning/Brutal/Focused/Swift/Combustion） | Supported | StatBag、CollectSkillMods | 消费 Stat 白名单（27/27 有读取点）+ golden parity | 5 Support | — |
| 机制 Support：Fork（弹道分裂 2） | Supported | 弹道对象池、MechanicSkill=弹道 | 兼容门（分裂×近战/范围必须拒绝）+ golden | 1 Support | — |
| 机制 Support：Fire Conversion（50% 物转火） | Supported | 转换轴；Runtime 零专用分支（战斗代码 0 条 if/switch/case） | GV-CONV 系列 + Tag 兼容（近战/弹道可接、范围拒绝）+ golden | 1 Support | — |
| Support 兼容门（写入前判定，失败无半写入） | Supported | `SliceSession.IsSupportCompatible`（Tag 路径 + 机制路径） | `SupportGateTests`（Runtime 判定与 golden 全 21 组合一致） | 全部 Support×3 技能 | — |
| golden 兼容 oracle（人工钉死，禁反向生成） | Supported | `SupportCompatGolden` | parity 测试（3×7=21 组合） | — | — |
| Support Level 1–20 / Quality 0–20 | Unsupported | Gem 数据模型（未建） | — | — | BDA Phase 4 范围；Director 未给方向 |
| 全量 Support Canonical 导入（PoEDB） | Unsupported | PoEDB Import Pipeline（未建立） | — | — | 同上 |

## 3. 装备 / 词缀 / Craft 机制族

| 机制 | 状态 | 依赖系统 | 测试 | 受影响内容 | Blocked 原因 / Resume Trigger |
|---|---|---|---|---|---|
| 6 装备槽全闭环（drop/equip/replace/聚合/快照/tooltip/抽屉 2×3） | Supported | EquipSlot canonical、ItemInstance、Build snapshot | `SixSlotEquipmentTests`（9）+ `SliceDrawerTests` + PlayMode 六槽 loop | 6 槽全部 | — |
| 词缀槽位适用性（单一 applicability truth：AllowedSlots 位掩码） | Supported | `AffixDef.AllowedSlots`（S4-P3 收口 predicate） | `AffixApplicabilityTests`（10）+ bySlot 13×4+15+15 audit | 17 词缀 | — |
| 双行组合词缀（第二值独立掷值/归零语义） | Supported | `ItemInstance.SecondValue*`、`RowCount` | 双范围 roll + 第二值残留归零回归测试 | 3 组合词缀 | — |
| 随机 Craft（Scrap 洗 Rare） | Supported | LootRng、SeededRng | Production Simulator 覆盖（10k cycles） | 4 原槽+2 新槽 | — |
| 定向 Craft（Etching 写入 + 同词缀 duplicate deterministic reject） | Supported | applicability predicate | duplicate guard 回归测试（S4-P4 +1） | 同上 | — |
| 掉落可达性（6 槽全 reachable、17 词缀全可达） | Supported | Drop 路径 | Production Simulator（invalidCount=0 + hash exact match） | — | — |
| 10 槽 / Weapon 族 / Handedness / Prefix-Suffix Tier / Item Level / Base Items | Unsupported | 未建 | — | — | BDA Phase 2 范围；Director 未给方向（Ring/Offhand/Amulet=NOT AUTHORIZED） |

> **S5 授权跟踪（2026-09-09，仅 tracking 不晋升）**：Multiple Link Groups（BL-021.A2）=**AUTHORIZED NOT IMPLEMENTED**（合同 `docs/reviews/S5/S5_LINK_CONTRACT.md`；S5 Phase 1 起实现）；Bounded Affix Breadth（BL-002.A1）=**AUTHORIZED NOT IMPLEMENTED**（清单 `S5_AFFIX_ADMISSION.md`，N=4；S5 Phase 3 起）；Socket Color（BL-021.A1）与 Gem Level/Quality（BL-012.A1）=**导演显式拒绝**（负向验收条件贯穿 S5）。上表各机制状态不因授权变化。

## 4. 战斗 / 地图 / 表现 / 基础设施机制族

| 机制 | 状态 | 依赖系统 | 测试 | 受影响内容 | Blocked 原因 / Resume Trigger |
|---|---|---|---|---|---|
| Combat Math 子集（stat/inc/more、Acc/Eva、Crit、Armour、Fire Res、单轴转换、Ignite、完整 9 步命中顺序） | Supported | `CombatMath.ResolveHit`、StatBag | `CombatMathTests`（golden 向量 GV-*） | 全部技能/词缀/Passive | — |
| 地图词缀 + Stability/Reward + 进图快照锁 | Supported | MapAffix、BuildSnapshot.Locked | EditMode 契约测试 + PlayMode | 1 图 3 词缀 | Map Tier=NOT AUTHORIZED（硬停止） |
| 敌人 AI LOD + 分离 + 命中规则（距离/圈/弹道） | Supported | DummyCrowd、ArenaSim | PlayMode 密度采样（100/200/300） | 3 普通+1 Elite | Boss=NOT AUTHORIZED |
| 正式敌人视觉管线（Catalog/Presenter/Feedback parity/MPB） | Supported | `EnemyVisualCatalog` 等（表现层零 gameplay 写） | 反射护栏 + `AshlingVisualContractTests` + Art Gate | 4 正式视觉 | 新 Content Batch=NOT AUTHORIZED |
| 战斗 SFX（5 键） | Supported | `AudioEvents.Play`+`RuntimeResourcePaths` | 资源契约 REQUIRED 真实加载（10/10） | 5 键 | — |
| 人声 VoiceCues（3 键） | **Blocked（GATED）** | `VoiceCues`+`RuntimeResourcePaths.Voice` | 资源契约 GATED 如实记录（0/3 present） | 3 键 | **Resume Trigger=导演试听指认**（候选清单 `DK_VOICE_INVENTORY.md`）；DEFERRED BY DIRECTOR |
| UI 正式层（皮肤/双球/底栏/抽屉/面板/Tooltip） | Supported | SliceSkin/SliceHud/SliceDrawerLayout/SliceTooltip* | 几何契约测试（`SliceDrawerTests` 等） | 全 HUD | 天赋树重排=Stage0 禁区（待规划审查） |
| Production Audit / Report / Simulator | Supported | failure-safe 审计管线 | `ProductionContentReportTests`、`ContentAuditFailsafeTests`、`ProductionSimulatorTests` | 机器可读真相 | — |
| canonical 验证门（5 层） | Supported | `tools/verify_unattended.ps1`（零仓库污染；只验证不修复） | `-SelfTest` 22+ 项；本轮 Quick Gate PASS | — | — |
| 性能合同（锁定硬件 2560×1440/D3D12/8.33ms） | Supported | `docs/qa/PERFORMANCE_GATE.json` 唯一真相源 | S4 Final 双 PASS（引用，不重跑） | — | 合同/硬件变更需显式工作令 |

## 5. 数据已导入但未实现的机制（防「假绿」声明）

- **无**。当前工程内数据表中的全部 Stat 均有运行期读取点（`unconsumedDeclaredStats=0`），4 条 Tagged Modifier 全部可达（死 Tagged Modifier=0），3 技能 golden parity 3/3。PoEDB 级全量数据尚未导入（流水线未建立），不存在「已导入未实现」库存。

## 6. Backlog ID Linkage（S4R-WO-02；S4R-WO-03 atom 细化）

未支持/部分支持机制的候选扩张已规范化入 Backlog（`S4R_BACKLOG_NORMALIZATION.md`，候选 ≠ 已批准）：Passive 大树镜像/Mastery/Ascendancy→BL-003（+BL-024 pipeline 前置）；Support Level/Quality→BL-022（授权粒度归 BL-012.A1）；全量 Support 导入→BL-023；孔色/多 Link→BL-021；10 槽/Weapon 族/Tier/Base Items→BL-001/002；Craft Deepening→BL-013（+BL-014 sim）；Map Tier→BL-015（+BL-017 sim）；Boss→BL-016；Aura→BL-004；Voice→BL-026（BLOCKED，Resume=导演指认）；Death 截断→BL-030。聚合行的授权粒度以 `S4R_AUTHORIZATION_ATOMS.md`（57 atoms）为准。

## 7. Candidate Reference Build Coverage Mapping（非权威；AC-08）

- 正式 **Reference Build Coverage 字段维持 N/A**（Targets 未锁定，不构成 Gap 判断；不得从 N/A 晋级 PASS/SUPPORTED）。
- 候选层映射见 `S4R_REFERENCE_BUILD_CANDIDATES.md`（RC-M/RC-P/RC-A，覆盖全部 3 个 canonical Skill，仅用 Supported mechanics，unsupported requirement=NONE，CANDIDATE / NOT LOCKED）。本表与正式 Locked Targets 严格分区：候选映射的任何后续变化不追溯改写本 Matrix 各族状态。
