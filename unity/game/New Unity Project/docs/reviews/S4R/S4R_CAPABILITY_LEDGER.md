# S4R_CAPABILITY_LEDGER — 统一能力清单（首次正式建立）

**性质**：高层能力状态地图（BDA 计划 §4.1 定义）。只记录「项目当前能表达什么」的真实状态，不记录未来计划；未来系统一律 Unsupported + 门控标注。细节依赖/测试/覆盖率见 `S4R_MECHANIC_MATRIX.md`。
**建立**：S4R-WO-01（2026-09-09）。**刷新规则**：规划 AI 每次阶段切换前必须刷新；工作 AI 在改变能力的 Work Order 中同步更新。
**本轮 reconciliation 结论**：全部条目与 runtime evidence 一致；**无 UNKNOWN；无「文档声称支持但运行时不支持」反向项；capability delta = NONE（本单未新增/晋升任何能力）**。
**S4R-WO-02 Count Reconciliation（2026-09-09）**：WO-01 Evidence 摘要「Supported（14）」为统计笔误（逐项实为 18），不改变任何行状态（无 promotion/降级）；以本节计数规则为准。

## 计数规则（Count Rule，S4R-WO-02 固定）

- **计数单位 = 状态总览表的一行**（一个 capability/domain 行；同族细分内容如「Mastery/Ascendancy 分属 BDA Phase 3」不拆行，避免双计）。
- **Summary 必须由实际行状态得出**：**Supported = 18；Partial = 4；Unsupported = 5；Blocked = 1；合计 28 行**。
- 禁止为凑总数改变行语义；行状态变更只能由已批准 Work Order 驱动并在「刷新记录」登记。

## Backlog 链接（S4R-WO-02；S4R-WO-03 细化为授权原子）

Partial/Unsupported/Blocked 行的候选扩张已全部进入规范化 Backlog（`S4R_BACKLOG_NORMALIZATION.md`）：Socket→BL-021、Map System→BL-015、Enemy Taxonomy→BL-016、Aura→BL-004、Weapon/Handedness→BL-001、Mastery/Ascendancy→BL-003（大树镜像族内含，不另立 ID）、Persistence→BL-025、Canonical Pipeline→BL-024、Voice→BL-026。Supported 行的合理后续→BL-001/002/021/022/023/028。**候选 ≠ 已批准。**
**授权原子（S4R-WO-03）**：聚合行的导演授权粒度以 `S4R_AUTHORIZATION_ATOMS.md`（57 atoms）为准——批准宽泛 Backlog ID 不得隐式批准独立 gated 子能力（如 BL-003.A1 大树 ≠ BL-003.A2 Mastery ≠ BL-003.A3 Ascendancy）；本 Ledger 各行状态不因 atom 拆分而变化。

状态定义：Supported=有 runtime 行为+测试证据；Partial=部分成立/受明确边界限制；Unsupported=当前 runtime 不存在（含数据不存在）；Blocked=被外部条件卡住（须有 Resume Trigger）。

## 状态总览

| Capability | 状态 | 证据锚点 | 备注 / 门控 |
|---|---|---|---|
| Damage Types（Physical/Fire 子集） | Supported | `COMBAT_MATH.md`（flat/inc/more 公式）+ `CombatMathTests` | ES/多元素=未做（COMBAT_MATH「未做」节如实列出） |
| Conversion（Physical→Fire 单轴） | Supported | `COMBAT_MATH.md` GV-CONV-* + `CombatMathTests` | Support 0.50 + Passive 0.40 共享转换轴聚合 0.90；上限 100%；Gain-as-Extra/多跳=Unsupported |
| Crit | Supported | `COMBAT_MATH.md` GV-CRIT-* | base 150%；DoT tick 不暴击 |
| Accuracy / Evasion | Supported | `COMBAT_MATH.md` GV-HIT-*（5% 下限） | 仅攻击检定；法术 100% |
| Armour | Supported | `COMBAT_MATH.md` GV-ARMOUR-*（POE1 `A/(A+5D)`，90% cap） | POE2 变体不采用（S2 锁定） |
| Resistance（Fire） | Supported | `COMBAT_MATH.md` GV-RES-*（默认 75% 上限，硬顶 90%，允许负抗） | 仅 Fire；多抗=Unsupported |
| DoT：Ignite | Supported | `COMBAT_MATH.md` GV-IGNITE-*（50%/4s，单实例取高 DPS） | Bleed/Poison/Shock=Unsupported（Stage0/后置） |
| Aura / Reservation | **Unsupported** | 无 runtime；BDA 计划 Phase 5（Pride/Anger/Determination/Grace） | **Director-gated 周期范围**；S4R 不授权 |
| Weapon / Handedness / Requirement | **Unsupported** | 当前单武器槽；无武器族/需求模型 | BDA Phase 2 范围，Director-gated |
| Socket / Link | Partial | RUNTIME.md Support/Socket 节（Weapon 3S→Q、Body 3S→W、Helmet 2S→E、Boots 0S；一条 Link；无孔色；Gloves/Belt 各 1 孔仅展示计数、不映射技能孔位——SupportCapacity 只走 Weapon/Body/Helmet） | **S5 跟踪（2026-09-09）**：Multiple Link Groups=**域核+运行时/UI 集成已实现（S5-WO-02 域核+S5-WO-03 集成：共享后置校验器/装备原子守卫/第二连接配置条/技能行源标注/Tooltip 划分行/运行时隔离实证，EditMode 291/291）——Socket/Link 总体保持 Partial**（Socket Color 未支持未授权）；Socket Color=**显式拒绝（导演，S5）**；Gem Level/Quality=**显式拒绝（导演，S5）**；状态仍 Partial，不晋升 |
| Skill Tag | Supported | 9 声明 Tag；golden parity 3/3（`SkillTagGolden` 独立 oracle）；未使用 4 Tag=预留非任务 | 新 Tag=Forbidden（未授权） |
| Support Compatibility | Supported | 单一判定入口 `SliceSession.IsSupportCompatible`（Tag 路径+机制路径）+ golden 21 组合 parity OK + `SupportGateTests` | 16 兼容/5 不兼容；禁止由 Runtime 反向生成 golden |
| Passive（16 节点图） | Supported | Production Report `passiveGraph` 16/20/0 + RUNTIME.md 天赋节（Start 免费、2 Notable、1 机制 Cinder Heart、出图免费重置） | 大树/Mastery/Jewel=Unsupported（Stage0 锁） |
| Mastery | **Unsupported** | 无数据无 runtime | BDA Phase 3 范围，Director-gated |
| Ascendancy | **Unsupported** | 无数据无 runtime | BDA Phase 3 范围，Director-gated |
| Equipment / Affix | Supported | 6 槽全闭环（drop/equip/replace/聚合/快照/tooltip/抽屉 2×3）+ 17 词缀 + 单一 applicability truth + 双行词缀；`SixSlotEquipmentTests`、`AffixApplicabilityTests`、`SliceDrawerTests` | 新槽/新词缀族=Forbidden（未授权）；Ring/Offhand/Amulet=NOT AUTHORIZED |
| Craft（有界） | Supported | 随机 Craft（Scrap 洗 Rare）+ 定向 Craft（Etching 写入；同词缀 deterministic reject）；Production Simulator 覆盖 | Deep Craft/Tier/Corruption=Unsupported + Forbidden |
| Map System（单图） | Partial | 1 图 Ash Court；3 图词缀（Hearty/Savage/Ash Veil）；Stability=100-ΣCost、Reward=1+ΣRewardAdd；进图 BuildSnapshot.Locked | Map Tier/Progression=NOT AUTHORIZED（硬停止） |
| Enemy Taxonomy / AI | Partial | 3 普通（Brute/Stinger/Ashling）+ Elite Warden；AI LOD 三档；分离规则；无 Boss | Boss/BossKind=NOT AUTHORIZED |
| Enemy Visuals | Supported | 4 正式视觉经 `EnemyVisualCatalog`+`EnemyVisualPresenter`+`EnemyVisualFeedback`（反馈 parity，MPB 零 clone；gpuSkinning=1） | 新 Content Batch=NOT AUTHORIZED |
| Audio：战斗 SFX | Supported | 5 键 REQUIRED 10/10 真实加载 PASS；单一入口 `AudioEvents.Play` + `RuntimeResourcePaths` | 中间件（FMOD）=Stage0 锁 |
| Audio：人声 Voice | **Blocked（GATED）** | 3 键映射 0/3 present（缺失=静音+如实记录）；609 条 ogg 无语义名 | **Resume Trigger=导演试听指认**（`DK_VOICE_INVENTORY.md`）；DEFERRED BY DIRECTOR |
| UI（正式 HUD） | Supported | SliceSkin 石质/金边+双球+底栏+抽屉 2×3 六槽+Build/Craft 面板+统一 Tooltip（`SliceTooltipModel`） | 天赋树重排=Stage0 禁区待规划；新面板需工作令 |
| Production Tooling | Supported | Content Audit（failure-safe，Collect→Render→Persist→Assert）+ Production Content Report（machine-readable、确定性、禁止手填）+ 10k Production Simulator（同 seed hash 可复现） | 大规模内容生产=NOT AUTHORIZED（硬停止） |
| Performance Gate Infra | Supported | canonical 5 层门（Quick/Build/PlayerRun/Performance/Art）+ 契约 `docs/qa/PERFORMANCE_GATE.json` 唯一真相源 + 证据快照 Operator（Verifier≠Historian） | 合同/硬件变更需显式工作令 |
| Determinism | Supported | 玩法/内容随机只走 `SeededRng`；sim 10k hash 一致；审计渲染 byte 级确定性 | 禁第二套 RNG/`UnityEngine.Random` |
| Persistence / 存档 | **Unsupported** | 当前设计无跨会话存档（图内 Build 快照锁为会话内） | 如需存档/迁移=Director 决策项（BDA migration-risk 轴） |
| Canonical Data Pipeline | Partial | 工程内版本化数据表（`ContentData/valid/`+`Catalogs.cs`）+ 全成或全败 Import；**PoEDB 全量导入流水线未建立**（BDA Phase 1 范围，Director 未给方向前不启动） | PoEDB=唯一外部来源（禁止替代源混入） |

## 与门控/硬停止的关系（不得写成已批准）

- 所有 **Unsupported + Director-gated** 条目（Aura/Weapon-Handedness/Mastery/Ascendancy/Persistence）与所有 **NOT AUTHORIZED** 扩张方向（新槽位/Ring/Offhand/Amulet/Unique/Boss/Map Tier/Progression Spine/New Content Batch/Voice 解禁）在 S4R 内保持 frozen；其「存在性」只作为 Backlog 分类输入（S4R Phase 1），不构成实施承诺。
- 本 Ledger 建立本身是文档事实（documentation-only），**capability delta = NONE**。
