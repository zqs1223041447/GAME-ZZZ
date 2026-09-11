# 无人值守模式 · 运行状态

> 本文件是无人值守循环的**持久记忆**。每次醒来先读本文件，再决定做什么。
> 上下文丢失后，一切以本文件 + 仓库工作令为准。

> ## ✅ 冷进程基线：无互斥锁（2026-09-11 11:0x 释放并归档）
>
> **互斥锁已释放，编辑器处于打开状态，可以正常 `unity open` / `unity command` / `unity test`。**
> 之前那段"⛔ 冷进程校验进行中，禁止启动编辑器"的警告属 WO-04A 入口阶段，已过期并删除；
> 其历史记录保留在下方。

> ### 冷进程基线记录（历史，按时间倒序）
>
> **S6P-WO-04A2 出口 Final Gate（2026-09-11 10:53–10:54）** —— 编辑器关闭、三独立冷启动：
>
> ```
> Run1 | 10:53:54 | 15.5s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run2 | 10:54:08 | 14.0s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run3 | 10:54:23 | 14.3s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Distinct hashes : 1   EXACT MATCH : YES
> ```
>
> **S6P-WO-04A2 入口 preflight（2026-09-11 11:00–11:01）** —— 用 `git stash push -u` 在干净 HEAD
> `ce6f85c` 上真实取得（这是 04A 那个 entry-fingerprint 缺口的正确补法，见 `S6P_WO_04A2.md` §3）：
>
> ```
> Run1 | 11:00:46 | 16.7s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run2 | 11:01:00 | 14.2s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run3 | 11:01:14 | 14.2s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Distinct hashes : 1   EXACT MATCH : YES
> ```
>
> **S6P-WO-04A 入口 preflight（2026-09-11 04:51–04:52）**：
>
> ```
> Run1 | 04:51:40 | 15.8s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run2 | 04:51:54 | 14.2s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Run3 | 04:52:08 | 14.1s | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
> Distinct hashes : 1   EXACT MATCH : YES
> ```
>
> 工具：`tools/evidence/cold-process-prodsim.ps1`。

## 模式

| 项 | 值 |
|---|---|
| 状态 | **ON** |
| 开启时间 | 2026-09-11 |
| 开启依据 | 导演指令「本次执行完成，询问GPT请求获得下一步工作规划。并进入无人值守模式。」 |
| 周期 | **S6P — Passive Truth & Deterministic Build Backbone** |
| 计划正文 | `docs/reviews/S6P/S6P_PLAN.md` |
| 规划 AI 原文 | `docs/reviews/S6P/S6P_PLANNER_REPLY_WO_RELEASE.md` |
| 规划渠道 | **Channel A `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`**（2026-09-11 新建；旧会话 `game-zzz-planning` 已停用） |

## 当前令

| 项 | 值 |
|---|---|
| 令号 | **S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness** |
| 状态 | **REWORK / AC-22 已修，待最小 Gate Review** |
| 授权来源 | Channel A 于 WO-04A2 Gate Review ACCEPT 时 **RELEASED / EXECUTE NOW**；合同 = `S6P_WO_04A_GATE_REVIEW_AND_WO_03_CONTRACT.md` §4–§19（叠加原 `S6P_WO_02_GATE_REVIEW_AND_WO_03_CONTRACT.md` 37 AC） |
| 前置 | `S6P_WO_03_PREFLIGHT.md` = PASS（22 个专精有可兑现 choice ≠ 0，不触发 STOP）；04A2 约束全部继承 |
| 入口指纹（生产树） | 干净 HEAD `a91e391` dirty=NO = `fd90468eb7b9ebdc95cb247c5c14d51c02a0c99092154ac0d5516ebaad88fb0a`（2814 files）。治理文档落盘后 working-tree = `bf400b251f2f1c33f37d46a2e633d562138d647f0085556ef2bc82e41298523a`（dirty=YES，仅 Markdown）。明细 `_wo03_fingerprint_entry.txt`。历史 preflight `a85a5e2e…`（HEAD `64b614f`）仅作 04A 收口态档案，**不得**再当本轮 mutation 入口 |
| 入口基线 | EditMode **475/475**、PlayMode **18/18**、ProdSim `FNV1A64:ec1d3ed67d3035d0`（04A2 出口冷进程 ×3 EXACT） |
| 冻结 fixture | **node 10 = Life Mastery**（group 741 / 1 Notable / 唯一可兑现 choice `+30 to maximum Life` ⇒ `Life:Flat=30`）；§13 fallback = 未选择 vs 显式选择必须改 gameplay/hash（全树无 >=2 可兑现 choice 的专精） |
| 实现顺序 | ① prerequisite owner（官方簇 + 同簇 >=1 已分配 Notable）→ ② choice support 只调 04A truth → ③ selection identity = MasteryNodeId + 权威 ordinal → ④ 原子提交/取消零增量 + 恰好 1 点 + 恰好生效一次 → ⑤ UI selector → ⑥ ProdSim V3（`pv\|passive-v2`）→ ⑦ 新 baseline 三独立冷进程 → ⑧ Evidence Pack |
| 硬约束 | TraversalTruth/EffectTruth 分离不可回退；route-only 不得改回“分配被拒”；Mastery 在正式选择提交前仍 SPECIAL_BLOCKED；Jewel/Timeless 不属本令；04A 367/315 若移动必须 before/after/delta/reason；禁 FirstChoice/choices[0]；禁第二套 Mastery oracle；禁 refund/存档/新玩法域；禁静默更新 14→1985 / `\|D\|`=325 / 42 个 start-disconnected supported nodes |
| 规划渠道 | Channel A = `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`（旧 `game-zzz-planning` 已停用） |

## 队列（WO-03 进行中）

| 项 | 值 |
|---|---|
| 队列 | **S6P-WO-03（进行中） → WO-04B → [WO-04C 条件] → WO-05** |
| 上一令 | **S6P-WO-04A2 = CLOSED / ACCEPT**（含 Post-Gate UI Addendum ACCEPT）；记录见 `S6P_WO_04A2.md` / `_GATE_REVIEW.md` |
| WO-03 必须继承的 04A2 事实 | ① `TraversalTruth` / `EffectTruth` 分离**不可回退**；② route-only 不得被重新当成"分配被拒"；③ Mastery 在 WO-03 正式选择提交之前仍为 `SPECIAL_PENDING + SPECIAL_BLOCKED`；④ Jewel / Timeless **不属于 WO-03 职权**；⑤ 04A 的 367 / 315 冻结数字若因 WO-03 合法移动，必须给出 before / after / delta / reason，禁静默更新 |
| WO-03 新约束（由 04A2 产生） | ① 04A2 的 14→1985 与 `\|D\|`=325 是新的基线事实，WO-03 若使其移动必须显式更新并说明；② **42 个 start-disconnected supported nodes**（不是 `OUT_OF_DOMAIN`）是后续 special-interaction topology 工作的输入，不得在本轮偷偷解，也不得预承诺"实现 Jewel 就全部解开"；③ `BlockedAllocatedCount` 语义已改为「不可通行却被点亮」，WO-03 不得再把它当「不可兑现」用；④ `TraversalTruth`/`EffectTruth` 分离**不可回退**，route-only 不得被重新当成"分配被拒" |

## 上一令（S6P-DIR-01 — 已实现、已提交入库 `ce6f85c`、已推送）

| 项 | 值 |
|---|---|
| 令号 | **S6P-DIR-01 — Director Supplemental Gameplay & Presentation Intervention**（原名误记为 WO-05，已按规划 AI 裁定改名 + 旧名存根） |
| 状态 | **IMPLEMENTED / 已提交入库**（commit `ce6f85c`）；当轮门：EditMode 462/462、PlayMode 17/17、ProdSim `FNV1A64:ec1d3ed67d3035d0` 未变 |
| 授权来源 | 导演 2026-09-11 补充要求 4/5/6（原话见 `docs/reviews/S6P/S6P_DIR_01.md` §1）；同批要求 1/2/3 见 `docs/reviews/S6P/S6P_DIRECTOR_FEEDBACK_2026_09_11.md` |
| 记录 | 实现记录 `S6P_DIR_01.md`；美术来源台账 `S6P_DIR_01_POEDB_SOURCING.md`；证据包 `S6P_DIR_01_EVIDENCE_PACK.md`；取证截图 `docs/_dirshots/s6pwo05/` |
| 交付要点 | 30 个 poedb 资源（物品图 12 / 技能宝石图 5 / 辅助宝石图 9 / 特效图 4）+ 两条主动技能（冰矛 R / 火球术 T）+ 两条机制型辅助（投射物返回 / 狙击印记）+ 四项机制（穿透 / 返回 / 命中点爆炸 / 单体印记增伤） |
| 关键设计裁定 | 冰矛/火球术**共享弹道连接组**（不新增连接数组、不动 canonical payload）⇒ ProdSim 基线不动；理由与代价见 `S6P_DIR_01.md` §4.3 |
| 已知限制 | L1 引擎无冰冷伤害轴（冰矛走物理轴）；L2 poedb 无粒子系统（弹体=贴图球体）；L3 共享连接组；L4 印记仅单体增伤 —— 全部登记在证据包 §7 |
| 规划 AI 裁定（2026-09-11） | ① 可达性冲突 = **批准「通行/生效分离」**，但**另立 S6P-WO-04A2 先做**（本令不并入）；② 共享连接组 = **暂时接受**（附 5 条不变量：`MaxLinkGroups=2` / 无 `LinkSkill2` / 无第三组 / 不增容量 / 既有 G0/G1 真值不变），**不得并入 WO-03**；③ 冰冷轴暂缓且**明确不属 WO-03 职权**，未来加 Cold axis 时单独重基线；④ 新四条技能/辅助 = **明确的导演授权内容增量**，不得写成「无内容变化」；ProdSim `ec1d…` 只证明**旧 canonical 场景未破**，**不**证明新轴已被覆盖 |
| 未做（越界项） | 不新增 StatId/EffectId/EventId/ModOp/Tag/Affix；不碰 Curse/Flask/Jewel 运行时；不改既有 Support 1..7 的身份与语义 |

## 历史：04A2 要点（已由正式合同 `S6P_WO_04A2_CONTRACT.md` 取代，保留作对账用）

| 项 | 值 |
|---|---|
| 队列 | **S6P-DIR-01 Gate Addendum ＋ S6P-WO-04A2 → WO-03 → WO-04B → [WO-04C 条件] → WO-05**（WO-05 仍指 Passive Overview LOD & Texture Residency，未开工） |
| 下一令 | **S6P-WO-04A2**（**已执行完毕，待 Gate Review**；合同原文见 `S6P_WO_04A2_CONTRACT.md`） |
| 04A2 目标 | 把节点的两个问题拆开：① 能不能作为树路径？② gameplay promise 能不能完整兑现？不得共用一个布尔值 |
| 04A2 真值 | `EffectTruth` = FULLY_SUPPORTED / UNFULFILLED / SPECIAL_PENDING；`TraversalTruth` = TRAVERSABLE / SPECIAL_BLOCKED / OUT_OF_DOMAIN（由现有 `PassiveSupport` 单一 owner 暴露，不另建 oracle） |
| 04A2 硬约束 | 普通上树节点：fully supported → TRAVERSABLE+FULLY_SUPPORTED；BLOCKED/mixed → TRAVERSABLE+**UNFULFILLED（零 modifier）**；Mastery/Jewel/Timeless → SPECIAL_BLOCKED（WO-03 才解）。mixed node 仍**整节点零效果**（禁 4 条偷偷生效）。04A 消费门保留（即使经 traversal 合法进入 Allocated，`RecalcPlayer`/`CollectSkillMods` 贡献恒 0） |
| 04A2 可达性门 | 从 `StartNode=2172` 出发：TRAVERSABLE 当图边、SPECIAL_BLOCKED 不可过；要求 **start-connected 的普通 supported 节点 100% 可达**（不得只用「14 变多了」当 PASS）；至少冻结一个此前被阻断、现经 route-only 路径可达的**真实 supported 节点**（route-only 可花点、route-only 零效果、最终目标真实生效） |
| 04A2 负向对照 | Mastery = reject、Jewel Socket = reject、Timeless/special = reject，且**零扣点** |
| 04A2 ProdSim | 入口/期望出口均 `FNV1A64:ec1d3ed67d3035d0`；新增敏感性测试证「分配 route-only 节点 → `ps\|` 身份变化，`pm/pe/pk` gameplay 结果不变」；若正式 hash 变化 → **STOP / 调查 / 禁止自行 rebaseline** |
| 04A2 禁止 | 新 StatId/ModOp/parser 规则、Mastery selector、Jewel/Timeless 机制、元素伤害轴、第三连接组、存档、LOD |

## 后续令（04A2 ACCEPT 之后回到 WO-03）

| 项 | 值 |
|---|---|
| 令号 | **S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness** |
| 状态 | **RELEASED / EXECUTE NOW**（Channel A 于 WO-04A2 Gate Review ACCEPT 时正式放行）。前置：只读 preflight 已 PASS 并落库，产品实现尚未开始 |
| 授权来源 | `docs/reviews/S6P/S6P_WO_04A_GATE_REVIEW_AND_WO_03_CONTRACT.md`（04A Gate Review = ACCEPT WITH FOLLOW-UP + WO-03 完整合同 §4–§19） |
| 前置记录 | `docs/reviews/S6P/S6P_WO_03_PREFLIGHT.md`；entry fingerprint `a85a5e2e23ca5acfae2a96b41a61d12cfdd59fd9fd9ec8a77ab1b43f4a12c926`（13765 files，HEAD `64b614f`） |
| 入口基线 | EditMode 426/426、PlayMode 17/17、ProdSim `FNV1A64:ec1d3ed67d3035d0` |
| 前置结论 | 315 专精 / 1863 choice：**全可兑现 22**、blocked 1841；专精 >=1 可兑现 = **22**（≠0 ⇒ **允许继续**，不触发 `PRECONDITION BLOCKED_BY_EXISTING_DOMAIN`）；>=2 可兑现 = **0** ⇒ 按 §13 fallback 用「未选择 vs 显式选择」做 hash 敏感性 |
| 冻结 fixture 候选 | **node 10 = Life Mastery**（group 741 / 组内 1 Notable / 唯一可兑现 choice `+30 to maximum Life` ⇒ `Life:Flat=30`） |
| 下一步（实现顺序） | ① prerequisite owner（PassiveSupport，同簇 >=1 已分配 Notable，禁 GUI 几何/名称/序）→ ② choice support truth 只调 04A truth → ③ selection identity（`MasteryNodeId + 权威 ordinal`）→ ④ 原子提交/取消零增量 + 恰好 1 点 + 恰好生效一次 → ⑤ UI selector（entry count == source count、1920/2560 边界、blocked 可见不可提交）→ ⑥ ProdSim **V3**（`pv\|passive-v2` + stable-key/invariant-float 清理 + 双因归因）→ ⑦ 新 baseline 由 3 个独立冷进程 `H1==H2==H3` 才能晋升 → ⑧ Evidence Pack（含 §18 增补字段） |
| 实现期硬约束 | 04A 冻结数字（367/315）会随 WO-03 移动，**必须显式更新并说明**，不得静默改；禁恢复 `FirstChoice`/`choices[0]`；禁第二套 Mastery support oracle 或 evaluator；禁 refund economy/存档/新玩法域 |
| 非阻塞 follow-up（04A 裁定，非返工） | WO-03 动到附近代码时把 `S3R2FireConversionTests.SupportAndPassiveConversion_ComposeOnSameAxis` 改成不误导的名称（如 `SupportAndParsedPassiveConversion_ComposeOnSameAxis`） |



## 已收口

| 令 | 结论 |
|---|---|
| S6P-WO-01 | **Gate Review = ACCEPT（无阻塞 follow-up）**。记录：`docs/reviews/S6P/S6P_WO_01.md`；产物：`docs/qa/PASSIVE_CENSUS_REPORT.json`。规划 AI 裁定：STRUCTURAL 582 行接受（但立规矩：STRUCTURAL 的判定依据必须是"该行语义角色不是 gameplay effect"，**不能简化为"含括号就是 STRUCTURAL"**）；编辑器两次挂死接受为环境 runbook evidence，不算产品 defect |
| S6P-WO-02 | **Gate Review = ACCEPT（31/31 AC，Follow-up = NONE）**。记录：`docs/reviews/S6P/S6P_WO_02.md`；原文：`docs/reviews/S6P/S6P_WO_02_GATE_REVIEW_AND_WO_03_CONTRACT.md`；Evidence：`docs/reviews/S6P/S6P_WO_02_EVIDENCE_PACK.md`。产物：`Assets/Tests/EditMode/PassiveAwareProductionSimulation.cs`(+Tests)、`ProductionSimulator.cs`(schema V2)、`docs/qa/PRODUCTION_SIMULATION_REPORT.json`(V2)。新 canonical hash **`FNV1A64:ec1d3ed67d3035d0`**×3 exact；`9a4c…` 降级 predecessor。规划 AI 额外锁定：`pe|` 带 raw 分量（base/flat/increased/more）接受、`pk|` 技能侧快照与 `pe|` 并存接受、allocation event 不进哈希接受 |

| 队列（2026-09-11 仲裁改序后的唯一 RELEASED 队列） | 状态 |
|---|---|
| S6P-WO-02 Passive-Aware Production Simulation | ✅ Gate ACCEPT（31/31） |
| **S6P-WO-04A Passive Support Truth Gate & Silent-Zero Elimination** | **已授权，进行中** |
| S6P-WO-03 Mastery Explicit Selection & Allocation Correctness | **QUEUE-DEFERRED**（合同保留、非失败非取消；04A ACCEPT 后重新放行，且必须消费 04A 的 support truth，不得自建第二套 oracle） |
| S6P-WO-04B Passive Build Identity / Snapshot / Lock Parity | 等 WO-03（只审计仓库**已存在**的 mask/定宽标识/snapshot/clone/map-entry capture/build lock/serializer/reset；**不得顺手发明存档系统**） |
| [S6P-WO-04C Existing-Consumer Semantic Closure] | CONDITIONAL：若 04A census 证明存在「runtime consumer 已存在、但 parser 尚未接上」的 effect families 才插，否则跳到 WO-05 |
| S6P-DIR-01 Passive Overview LOD & Texture Residency | 最后 |

**一轮一令**：每张 Evidence Pack 必须先发回 **Channel A** 做 Gate Review，上一单 ACCEPT 后才动下一单。
Evidence Pack 字段清单见 `docs/reviews/S6P/S6P_WO_01_GATE_REVIEW_AND_WO_02_CONTRACT.md` §25，
04A 专用字段清单见 `S6P_ORDER_ARBITRATION_AND_WO_04A_CONTRACT.md` §30。

## 04A 执行设计（已探明，照此做，别重新摸索）

### 唯一 support truth 放哪

放**运行期**（`Assets/Runtime/Core/Gameplay/PassiveSupport.cs`），因为 `SliceSession.TryAllocate` 与 UI 都必须消费它。
**不得**让分类逻辑只活在测试程序集里（现状 `PassiveCensus` 在 `Game.Tests.EditMode`）——
04A 要把行分类器**下沉到 runtime**，让 census 反过来消费它，做到"只有一个 owner"。

### 关键：CONSUMED 的消费者清单**已经存在**，直接复用，不要新建第二份

`Assets/Tests/EditMode/ContentAuditS2Tests.cs` 有 `internal static readonly StatId[] RuntimeConsumedStats`
（注释注明已被 `ProductionContentReport` 复用，即工程本来就有"不建第二份 truth"的纪律）：

- 面板/防御（`RecalcPlayer` → `PlayerStats`）：Life, Mana, Strength, Dexterity, Intelligence, Armour, Evasion, Accuracy, FireResistance, MaxFireResistance
- 攻击结算（`BuildPlayerHit` → `HitRequest` → `CombatMath.ResolveHit`）：Damage, PhysicalDamage, FireDamage, MoreDamage, MorePhysical, MoreFire, AddedPhysical, AddedFire, ConvertPhysToFire, CritChanceBase, CritChanceAdded, CritChanceIncreased, CritMultiAdded, IgniteChance
- 技能形态（`ResolveSkillDef`）：AreaRadiusMore, AreaDamageMore, AttackSpeed
- 机制（`ResolveProjectile` → `ForkCount`）：Fork

**已核对：`PoeStatParser` 能产出的每一个 StatId 都在这张表里**（含 `AreaRadiusMore` —— 它确实在
`SliceSession.ResolveSkillDef` 第 873 行被 `_RawMore` 消费，不是死映射）。

### 因此 04A 的严格化"不会翻动 canonical fixture"

把 `CONSUMED` 从"parser 认了"收紧为"parser + 真消费者"，**一条都不翻**：
- 607 条 CONSUMED 保持不变 → 424 个 SUPPORTED 节点保持不变
- canonical fixture（NodeId `559,1795,2034,2172`，全是 supported non-Mastery）**仍然可分配**
- ⇒ **出口 hash 应仍为 `ec1d3ed67d3035d0`**，与合同预期一致

### 04A 真正的行为变更只有两条

1. **2005 个 `BLOCKED_CURRENTLY` 节点从"可点但只吃半截"变成"整节点原子拒绝"**（加点 0 消耗、状态零变化）。
2. **315 个专精在 WO-03 前不可分配**，且**隐式 choice[0] 效果归零**
   （现状：`Catalogs.cs` 的 `PassiveCatalog.Build` 对 Mastery 用 `FirstChoice(src)` 当 Mods —— 这就是要拆掉的那条路径）。

### 行分类规则（结构性判定的依据要写在注释里）

- `STRUCTURAL`：行首就是 `(` ——这是官方数据标记"注释/提示文本"的格式约定，
  **不是**"含括号就算"。`+20% increased Damage (some note)` 行首是 `+`，**不得**判成 STRUCTURAL（要写回归测试钉死）。
- `CONSUMED`：`PoeStatParser` 命中 **且** 产出的 StatId 全在 `RuntimeConsumedStats` 里。
- `BLOCKED_BY_DOMAIN`：命中缺失轴关键词表（沿用 WO-01 那张 70 轴表）。
- `SPECIAL_INTERACTION`：无数字且无已知轴 —— 但按合同 §12，**没有现存 runtime handler 的 special 一律按 blocked 处理**，
  当前至少包含 Mastery / Jewel Socket / Timeless-special。

### 分阶段（每阶段都要能编译 + 全绿，避免留下半成品）

1. runtime 下沉 `PassiveSupport`（分类器 + 节点资格），census 改为消费它 —— 行为不变。
2. `TryAllocate` 支持门 + 消费门（注入的 blocked 节点贡献零 modifier）；UI 读同一 truth。
3. 专精过渡态：不可分配 + `FirstChoice` 默认效果断开。
4. 新增测试族（合同 §24 那 ~19 条）+ PlayMode 正/负对照。
5. 源数据 provenance（`tree_raw.json` SHA-256，只入证据不入 hash）+ Evidence Pack + **出口冷进程 ×3**。

### 出口门（必须做）

```powershell
unity close "G:\GAME-ZZZ\unity\game\New Unity Project"     # 冷进程需要独占
pwsh -NoProfile -File tools/evidence/cold-process-prodsim.ps1 -Runs 3
```
三次都必须 `FNV1A64:ec1d3ed67d3035d0`；变了就 STOP 调查，**不得直接重基线**。

## 双规划渠道治理（规划 AI 已锁定，别自作主张）

| 渠道 | 会话 | 角色 |
|---|---|---|
| **Channel A** | **`game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`**（2026-09-11 新建） | **仲裁 + 可执行队列权威**（唯一 RELEASED 队列） |
| （历史）Channel A 旧会话 | `game-zzz-planning` / `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` | **已停用** —— 按导演 2026-09-11 指令「新开一个会话，不要再使用旧的规划 AI 会话」，不再向其发令 |
| **Channel B** | `game-zzz-plan-v2` / `6aa31495-638c-83e8-bf15-f9952101fc6c` | **独立第二意见**；输出默认 = `PROPOSED / NOT YET EXECUTABLE` |

### 仓库对规划 AI 的可见性（2026-09-11 起）

- 远端 = **公开** GitHub `https://github.com/zqs1223041447/GAME-ZZZ`，`main` 已与本地同步
  （`git push` 于 2026-09-11 完成：`64eb9fe..ce6f85c`）。
- **新规则：每次向规划 AI 发令前，先 `git push`，让它能直接读仓库真值**，而不是只靠消息里的现状摘要。
- 若某轮只提交未推送，必须在发令消息里显式声明「远端落后本地 N 个提交」，否则规划 AI 会读到过期代码。

- 冲突时：**STOP → 交 Channel A 仲裁 → 只保留一份 RELEASED 队列**。
- **不采用**「两个渠道并行发令、我自己择优执行」——无人值守最大的敌人是同时存在两份都自称权威的未来状态。
- 例外：**导演明确点名**某条 Channel B 工作令「立即执行」时，导演指令优先；执行后由 Channel A 依真实仓库状态重新对账。

## 证据要求（自 WO-04A 起强制）

1. **Working-Tree Fingerprint + Changed-File Inventory**（工具已建）：
   `pwsh -NoProfile -File tools/evidence/working-tree-fingerprint.ps1`
   内容寻址 SHA-256；Gate 与 Evidence 必须唯一对应**同一** working-tree state。
2. **三独立 Unity 冷启动进程**：凡「新 canonical 基线晋升」都必须由 3 个独立冷进程产出并 exact match；
   不得用同进程内连跑三次冒充。04A 的入口 preflight 与出口 Final Gate 都要做。
3. **PoE 源数据 provenance**：记录 `tree_raw.json` 的来源/版本与 **SHA-256**；checksum 只作证据，**不进 gameplay hash**。
4. **门禁判据**：`ALL DISCOVERED tests PASS / fail = 0 / skip = 0（除非显式说明）`
   + `predecessor contract tests missing = 0` + `deleted/weakened without explanation = 0`。
   **不再把具体测试总数（394/409/…）当固定分母 oracle**，但数字仍要报告作 evidence accounting。

## 硬约束（违反即停）

1. **不加新玩法域**：药剂 / 珠宝 / 升华 / Timeless Jewel / 新技能 / 新 Stat 轴
   （冰冷、闪电、ES、格挡、压制、召唤物、图腾、战吼、异常、充能、吸取、DoT）
   一律**不做**——需导演玩法授权。只能出现在 census / dependency report 里，不得作为阻塞项。
2. **不采购、不换框架**：不引入 ORK / TopDown Engine / 其他第三方框架；不买资产。
3. **不做只能靠人眼验收的事**：每项必须能用门禁自证。
4. **不臆造**：源数据证明不了的关系写 `UNRESOLVED_WITH_SOURCE`，禁止凑数。
5. **不把覆盖率当 KPI**：判据是「声称支持的 100% 真生效，不支持的 0% 假生效」。
6. **不做破坏性操作**：不 `git reset --hard`、不删分支、不强推、不动 `.git`。
7. **不静默空效果**：不允许**静默**的"点亮了但没效果"。唯一合法例外是 **S6P-WO-04A2 明确授权、
   且在 UI 上明示**的 route-only 节点（`EffectTruth=UNFULFILLED` + `TraversalTruth=TRAVERSABLE`）：
   它可合法消耗天赋点作为路径、贡献恒 0 modifier，**前提是玩家在 tooltip 上能看到"该节点当前无法
   兑现任何效果（0 效果）"**。除它之外，任何"点亮了但没效果"的路径仍是错误产品语义。
   不允许再新增第二类静默零效果；也不允许把 route-only 重新当成"分配被拒"（那会退回 14/2429）。

## 门禁命令（怎么验）

```powershell
# 编辑器常驻时可用（工程被编辑器占用时 unity test 会拒绝，必须走 command 管线）
cd G:\GAME-ZZZ

# ⚠️ 跑 EditMode 之前必须先停 Play：编辑器处于 Play 模式时 run_tests 会无限挂住（不报错不返回）
unity command editor_stop

# 全量 EditMode
unity command run_tests --mode EditMode --timeout 1200

# 单测过滤
unity command run_tests --mode EditMode --filter <TestClassName> --timeout 600

# PlayMode（必须异步）
unity command run_tests --mode PlayMode --async_tests true
# 等 30~60 秒后读 Temp\pipeline_test_status.json（务必核对文件时间戳是本次运行，别读成上一次的陈旧结果）

# ProdSim 基线（hash 打在 EditMode 的控制台日志里）
unity command run_tests --mode EditMode --filter ProductionSimulatorTests --timeout 600
unity command console        # 从结果里正则取 [ProductionSimulation] hash=... invalid=...

# 实机取证
unity command editor_play / editor_stop
unity command eval --code '...'
unity command capture_game_view --source screen --width 2560 --height 1440 --save_path "docs/<...>.png"
```

### 编辑器挂死的识别与恢复（2026-09-11 实际踩过）

- **症状**：pipeline 全部命令超时（连 `editor_stop`、`test_status` 都超时），进程 CPU 不再增长，
  `%LOCALAPPDATA%\Unity\Editor\Editor.log` 被 `IPCStream (hubIPCService): IPC stream failed to write (Timed out)` 刷屏。
- **真因**：强杀编辑器后，下次启动停在模态框 **"Recovering Scene Backups"**（用
  `Get-Process Unity | Select MainWindowTitle` 可确认），该框不响应 pipeline。此时 `unity close` 无效（返回 `closed=false method=none`）。
- **恢复配方**：
  ```powershell
  Get-Process | Where-Object { $_.ProcessName -match '^Unity' } | Stop-Process -Force
  Remove-Item "...\Temp\__Backupscenes" -Recurse -Force
  Remove-Item "...\Temp\UnityLockfile" -Force
  unity open "G:\GAME-ZZZ\unity\game\New Unity Project" --no-banner   # 用 background + timeout 0 启动，别让超时把 Job 杀了
  ```
- **不要**用 `unity close` 之外的方式强杀正在正常工作的编辑器；确实要强杀时，杀完必须清 `__Backupscenes` 再重开。

**截图落盘注意**：`capture_game_view --save_path` 只接受工程内相对 `Assets/` 的路径且拒绝 `..`，
所以会落在 `Assets/docs/...`——**每次必须立刻移动到 `docs/...` 并删掉 `Assets/docs`**，
否则会在 Assets 下留一堆被 Unity 导入的 stderr 资产。

## 每令收尾必做

1. 跑全量 EditMode + PlayMode，记录数字。
2. 记录 ProdSim hash（本令允许变化的，给 Before / After×3 / Exact Repeat / Reason / Changed Fields / Unexpected Inputs=None）。
3. 写工作令记录到 `docs/reviews/S6P/<令号>.md`。
4. 把 Evidence Pack 发回规划 AI 会话做 Gate Review（`--topic game-zzz-planning`）。
5. 把 Gate Review 结论与本文件状态一起更新（含 `开发计划/规划AI会话.md` 追加一行）。
6. 只有上一令 ACCEPT，才把下一令状态改为「进行中」。

## 停止条件

- 规划 AI 判 `REJECT-REWORK` 且两次返工仍不通过 → **停**，留给导演。
- 遇到需要导演授权的事（新玩法域、采购、换框架、破坏性操作）→ **跳过该项并记录**，不阻塞其余工作。
- 门禁连续两次因同一原因失败且无法定位 → **停**，写清现象与已排除项。
- 队列走完（WO-05 收口）→ 把 Evidence 发回规划 AI 请求下一周期，等回复；**不要自行开新周期**。

## 进度日志

| 时间 | 事件 |
|---|---|
| 2026-09-11 | 模式开启；S6P 计划落库；WO-01 开始 |
| 2026-09-11 | 编辑器挂死（Recovering Scene Backups 模态框），已定位真因并恢复；恢复配方已写入本文件 |
| 2026-09-11 | **WO-01 执行完毕**：节点集合对账写死（2832 = 2429 上树 + 403 未上树）；效果行 5676 行四分类 **UNKNOWN=0**；ProdSim 审计结论 **NOT_PASSIVE_SENSITIVE**（哈希只看物品键+循环序号）；**2005 个节点属 UNSUPPORTED_CURRENTLY**（可点亮但含引擎兑现不了的效果行）；专精分配规则不存在。门：EditMode 394/394、PlayMode 14/14、ProdSim `9a4c…` UNCHANGED。Evidence Pack 已发回规划 AI |
| 2026-09-11 | **WO-01 Gate Review = ACCEPT（无阻塞 follow-up）**，规划 AI 同步下发 **S6P-WO-02 完整合同（31 条 AC）**。WO-02 已置为「已授权，待开工」 |
| 2026-09-11 | 心跳执行 **WO-02 完毕**：passive-aware canonical 证据面（`pv\|passive-v1`，fixture NodeId `559,1795,2034,2172`，6 条 modifier / 2 进攻 + 4 防御属性），敏感性 A/B/C 全过、顺序无关、新 canonical hash **`ec1d3ed67d3035d0`**；`9a4c…` 降为 predecessor。**Gate Review = ACCEPT（31/31 AC）**。发现并修正上一轮偏差：WO-02 曾说"已回传"但实际没发出 Evidence Pack，本轮补发 |
| 2026-09-11 | 导演指令「新开 GPT 渠道 + 给仓库地址 + 问工作计划 + 无人值守自动执行」。已开 **Channel B**（`game-zzz-plan-v2`），交给它公开仓库地址并声明远端落后本地 31 个提交。Channel B 给出顺序 `WO-02 → WO-04A → WO-03 → WO-04B → WO-05`，与原队列冲突 |
| 2026-09-11 | 把冲突提交 **Channel A 仲裁**。**裁决 = 改序采纳**：`WO-04A → WO-03 → WO-04B → [WO-04C 条件] → WO-05`。WO-03 标为 QUEUE-DEFERRED（合同保留）。同时锁定：专精过渡态授权、04A 出口 hash 必须不变、WO-02 不追溯降级但**此后基线晋升须三独立冷进程**、canonical serializer 协议、测试分母规则、源数据 provenance、双渠道治理（A 为队列权威，B 为第二意见）。**S6P-WO-04A 正式 RELEASED** |
| 2026-09-11 | 已建 `tools/evidence/working-tree-fingerprint.ps1`（内容寻址 SHA-256 + 分区变更清单），实测可用；清理了 `Assets/docs.meta` 残骸。下一动作 = 04A 强制前置：三独立冷进程基线校验 |
| 2026-09-11 | **04A 入口冷进程基线校验通过**：关编辑器 → 三次独立 batch EditMode 冷启动（各 ~14–16s，exit=0，contract=`pv\|passive-v1`，invalid=0）全部 `FNV1A64:ec1d3ed67d3035d0`，EXACT MATCH。工具 `tools/evidence/cold-process-prodsim.ps1`。编辑器已重开 |
| 2026-09-11 | **04A 设计已探明并写入本文件**（见「04A 执行设计」节）：唯一 support truth 下沉 runtime；CONSUMED 复用既有 `ContentAuditS2Tests.RuntimeConsumedStats`（已核对：parser 能产出的 StatId 全在其内，含被 `ResolveSkillDef:873` 真实消费的 `AreaRadiusMore`）⇒ **严格化一条都不翻，出口 hash 应仍为 `ec1d3ed67d3035d0`**；04A 真正的行为变更只有「2005 个 blocked 节点整节点原子拒绝」与「315 个专精过渡态不可分配 + 隐式 choice[0] 归零」 |
| 2026-09-11 | **WO-02 执行完毕**：ProdSim 从 loot→craft→equip 扩到 +passive allocation→modifier→effective stat；冻结 canonical fixture（起点 2172 + 559/1795/2034，全 SUPPORTED、专精=0、unsupported=0）；哈希负载新增 `pv|/ps|/pm|/pe|/pk|`；敏感性 A/B/C/D 全证；新 canonical hash **`ec1d3ed67d3035d0`**×3。门：EditMode 409/409、PlayMode 14/14、Content Audit fresh PASS、census `PASSIVE_SENSITIVE` |
| 2026-09-11 | **WO-02 收尾核验（本轮心跳）**：最终代码状态重跑全量门 —— EditMode 409/409、PlayMode 14/14、ProdSim×3 `ec1d3ed67d3035d0`（04:35:42/49/55）、Content Audit PASS fresh；复核 WO-02 时间窗内 `Assets/Runtime` 零 .cs 改动（Delta=NONE 成立）。发现上一轮**只写了工作令记录、并未真正发出 Evidence Pack**（Temp 无 WO-02 prompt、会话日志停在 01:06），本轮补发 |
| 2026-09-11 | **WO-02 Gate Review = ACCEPT（31/31 AC，Follow-up = NONE）**。规划 AI 正式确立新 baseline `ec1d3ed67d3035d0`，`9a4c…` 仅作 predecessor；**释放 S6P-WO-03（完整合同：37 AC + 22 必做测试）**。WO-03 已置为「已授权，待开工」 |
| 2026-09-11 | 本批导演插单向规划 AI（Channel A）回报：要求对 ① 树可达性冲突 ② 共享连接组 ③ 冰冷轴缺位 给出裁决，并请确认队列。**已收到裁定**：① 批准「通行/生效分离」但**另立 S6P-WO-04A2 先做**；② 共享连接组**暂时接受**（5 条不变量，不得并入 WO-03）；③ 冰冷轴暂缓且**不属 WO-03 职权**；④ **本批改记 S6P-DIR-01**（WO-05 编号保留给 Passive LOD）；⑤ 需补 Gate Addendum 6 项。全部落库 `S6P_DIR_01_GATE_ADDENDUM.md`；文档改名 + 旧名存根完成 |
| 2026-09-11 | Gate Addendum 补齐：怪物**攻击**动画实机轨迹（`req=Attack` ↔ state hash 1080829965 / `req=Idle` ↔ 2081823275；攻击采样间 `Bip01_R_Forearm` 世界位移最大 0.4；采样序列 Attack→Idle 交替，`AttackExecutions` 30→33）；四项新机制的分域测试清单；Multi-Link 5 条不变量逐条取证（同组共享=`AreSame` 钉死，非第三组）；30 项资产 SHA-256 清单 + **PROTOTYPE / REFERENCE ONLY / NOT PRODUCTION-ADMITTED** 授权状态；内容增量与 ProdSim 覆盖面声明（`ec1d…` 只证旧场景未破，不证新轴已覆盖） |
| 2026-09-11 | 导演补充要求 1/2/3 落盘并验收：背包快捷键开关（`I`）/ 天赋树连线改真实节点坐标+簇底衬派生几何+默认缩放 0.45 / Troll 系 5 条 clip 打开 Loop Time。门：EditMode **434/434**、PlayMode **17/17**、ProdSim 未变。记录 `S6P_DIRECTOR_FEEDBACK_2026_09_11.md`；取证 `docs/_dirshots/01..05` |
| 2026-09-11 | 发现并登记**产品级真值冲突**：04A fail-closed 支持门使全树可达节点仅 **14/2429**（起点可点 3 个），而「可完整兑现」节点有 367 个（其中 353 个被孤岛隔开）。**未自行放宽**（属设计决策）：登记为需导演/Channel A 裁决，建议方案 A「通行/生效分离」 |
| 2026-09-11 | 导演补充要求 4/5/6 落盘：poedb 美术摄取 33 项（`tools/poedb/fetch_art.py`，逐条来源入台账）；新增 `SkillId.IceSpear=4/Fireball=5`、`SupportId.ReturningProjectiles=8/SnipersMark=9`；机制=穿透/返回/命中点爆炸/单体印记增伤；呈现=物品图+宝石图+弹体贴图球体。门：EditMode **462/462**、PlayMode **17/17**、ProdSim `ec1d3ed67d3035d0` **未变**。证据包 `S6P_DIR_01_EVIDENCE_PACK.md`；取证 `docs/_dirshots/s6pwo05/01..04` |
| 2026-09-11 | 弹体形态四轮实测取舍留档（广告牌不抠图=深色方块 / 抠图=方晕 / 加色=饱和方片 / **贴图球体 Unlit=通过**），写入 `S6P_DIR_01_POEDB_SOURCING.md` §4.2，避免后续重复踩坑 |
| 2026-09-11 | **WO-04A Gate Review = ACCEPT WITH FOLLOW-UP（技术/产品 42 / 42 AC PASS，Blocking Follow-up = NONE，Evidence Exception ×1）**；**S6P-WO-03 正式 RE-RELEASED / EXECUTE NOW**（完整合同 §4–§19）。三条裁定：① **57 珠宝孔封锁 = 接受**（属 §12/AC-11 授权范围，`424 − 57 = 367` 是合理 truth refinement，不构成实现 Jewel 域）；② **S3R2 测试适配 = 接受，不判削弱**（旧 E2E 路径被产品规则合法关闭；三层互补证据成立）+ 非阻塞整理要求（WO-03 顺手改测试名，**非 04A 返工**）；③ **入口指纹缺失 = `EVIDENCE_EXCEPTION_ACCEPTED`**，不得事后伪造，规则升级为「entry fingerprint → production mutation → tests → final fingerprint」。04A 证据：`S6P_WO_04A_EVIDENCE_PACK.md`；裁定原文：`S6P_WO_04A_GATE_REVIEW_AND_WO_03_CONTRACT.md` |
| 2026-09-11 | **WO-03 强制前置（只读）执行完毕 = PREFLIGHT PASS**，产品实现尚未开始。entry fingerprint `a85a5e2e…`（13765 files，HEAD `64b614f`）先落库；用 04A 唯一 support truth 全量 census 315 个专精 / **1863** 条 choice：**全可兑现 22**、blocked 1841（域 1833 + 无 handler 特殊 8）；**专精 >=1 可兑现 = 22 ≠ 0 ⇒ 不触发 STOP，允许继续完整 WO-03**；>=2 可兑现 = **0** ⇒ 按 §13 fallback 用「未选择 vs 显式选择」证 hash 敏感性。全部 315 个专精在官方簇内都有 >=1 个 Notable（prerequisite 规则可满足，非死规则）。冻结 fixture 候选 = **node 10 Life Mastery**（group 741，唯一可兑现 choice `+30 to maximum Life` ⇒ `Life:Flat=30`）。记录：`docs/reviews/S6P/S6P_WO_03_PREFLIGHT.md`。实现期耦合点已登记：04A 冻结数字 367/315 会随 WO-03 移动，必须显式更新并说明 |
| 2026-09-11 | **S6P-WO-04A 实施完毕**。开工前先按导演指令把状态同步发回 Channel A（`--topic game-zzz-planning`），规划 AI 回「**立即开工**」并给 7 条 Execution Amendment（runtime 必须拥有 consumer 清单 / mixed node 完整零增量取证 / 消费门必须绕开分配门取证 / 专精连 baked Mods 一起拆 / supported 正对照 / 出口 hash 不得变 / Evidence hygiene），原文落库 `docs/reviews/S6P/S6P_WO_04A_PLANNER_AMENDMENT.md`。实现：新增 runtime 唯一 support truth `PassiveSupport.cs`（行四分类 + 节点资格 + consumer 清单 + 稳定原因），`TryAllocate` 支持门、`RecalcPlayer`/`CollectSkillMods` 消费门、`NodeBlockReason`/`BlockedAllocatedCount` UI 与 invalid-state 接口、专精不再烘焙 `FirstChoice`、census 反转为消费 runtime（V2 + masteryPending）。门：EditMode **426/426**、PlayMode **17/17**、ProdSim `ec1d3ed67d3035d0` **不变**（出口冷进程 ×3 EXACT，07:58–07:59）、Content Audit PASS。真相：支持 **367** / 阻塞 **1660** / 无 handler 特殊 **87**（57 珠宝孔 + 30 时光珠宝类，**第三处未事前预估的行为变化**，合同 §12 明确要求）/ 专精过渡 **315**；效果行与 WO-01 完全一致（607/4475/12/582，UNKNOWN=0），严格化 CONSUMED **一条都没翻**。证据：`docs/reviews/S6P/S6P_WO_04A.md` + `..._EVIDENCE_PACK.md` + `_wo04a_fingerprint_gate.txt` / `_wo04a_fingerprint_final.txt` / `_wo04a_cold_exit.txt`。**缺口**：入口 working-tree fingerprint 未捕获（已在证据包登记；新规则：改动前先落指纹） |
| 2026-09-11 | 导演指令「提交 git，然后继续执行原本工作计划，和规划 AI 一起执行；**新开一个会话**，不要再用旧的规划 AI 会话，把仓库地址和现状发给他们」。已执行：① 提交 `ce6f85c`（S6P 结转 + S6P-DIR-01；2020 文件 / 51.1 MB；`.gitignore` 新增本地暂存与外部素材目录，含 1.0GB 无引用的 `Assets/RAEL STUDIOS`）；② 导演追加要求「先推送远端再让 GPT 读仓库」→ `git push` 成功（`64eb9fe..ce6f85c`），远端已与本地同步；③ **新开 Channel A 会话** `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`，发出仓库地址 + 完整现状，收到 **S6P-WO-04A2 完整合同**（落库 `S6P_WO_04A2_CONTRACT.md`） |
| 2026-09-11 | **S6P-WO-04A2 执行完毕（Ready for Gate Review）**。入口先落指纹（`6d2c9ae4…`，HEAD `ce6f85c`，dirty=NO），再按 PF-1..PF-6 全过：EditMode 462/462、PlayMode 17/17、04A census 367/1660/87/315、旧可达性 14/2429、PF-5 用 `git stash push -u` 在干净 HEAD 上跑冷进程 ×3 = `ec1d3ed67d3035d0` EXACT、候选集 311。实现：`PassiveSupport` 拆出 `EffectTruth` / `TraversalTruth`（`ClassifyTruth` 唯一分类，`Project` 派生 04A 四值诊断视图），删除共用布尔 `IsAllocatable`；分配门只读通行维度、消费门只读效果维度；新增 `ReachableSet()`。全树可达 **14 → 1985**；`\|D\|`=325 的 start-connected supported **100% 可达**（缺失集 `[]`）；冻结 fixture target **183** / 路径 `[2172, 71, 183]` / 首个 route-only **71**。门：编译 0 error、EditMode **475/475**、PlayMode **18/18**、ProdSim 入口/出口冷进程 ×3 均 `ec1d3ed67d3035d0`（**未 rebaseline**）；敏感性 `ps` DIFFERENT、`pm/pe/pk` EXACT MATCH。证据：`S6P_WO_04A2.md` + `S6P_WO_04A2_EVIDENCE_PACK.md` + `docs/qa/WO_04A2_TRAVERSAL_REPORT.json` + `_wo04a2_cold_entry/exit.txt`。**诚实登记**：42 个 supported 节点仍在起点连通域外（被特殊节点围住，ID 全表已列）；`R_ref` 与 `R_runtime` 按合同字面定义同构 ⇒ AC-15 构造性成立（已在证据包 §F 说明，请 Channel A 确认是否要求更严格的参考图） |
| 2026-09-11 | **S6P-WO-03 维持 BLOCKED**：未实现任何 Mastery 显式选择功能（22 个可兑现 choice 一个未动），未碰 Jewel/Timeless/升华/药剂/新技能/新 Stat 轴。Evidence Pack 待发回 Channel A 做 Gate Review（`--topic game-zzz-planning-2`） |
| 2026-09-11 | 导演指令「继续」。进程曾在 push-then-planner 步骤遇到 429；醒来后核对：远端已与 `a91e391` 同步、WO-04A2 已 CLOSED/ACCEPT、WO-03 已 RELEASED。入口指纹按当前 HEAD `a91e391` 重取（`fd90468e…`，dirty=NO）；旧 preflight 指纹 `a85a5e2e…`（HEAD `64b614f`）降为档案。按「先 push 再让规划 AI 读仓库」把治理文档推远端后，向 Channel A `game-zzz-planning-2` 通报开工并开始 WO-03 产品实现 |
| 2026-09-11 | **S6P-WO-03 产品实现完毕，Ready for Gate Review**。Channel A 开工裁定 = EXECUTE NOW + EA-1..EA-4。实现：prerequisite / 显式选择 / `TryAllocateMastery` 原子提交 / 选择器 UI / ProdSim V3。EA-2 专精不是 transit；EA-3 合法选择不算 corruption；EA-4 census 367/315 未动；拓扑 1985/325/42 未动。门：EditMode **489/489**、PlayMode **19/19**、Content Audit PASS、冷进程 ×3 `FNV1A64:99f1bfd3f81c4fe6` EXACT（contract=V3）。hash 归因：B=serializer cleanup（默认场景）；A=node 10 未选 vs +30 Life。证据：`S6P_WO_03.md` + `S6P_WO_03_EVIDENCE_PACK.md` + `_wo03_cold_exit.txt`。下一步=把 Evidence Pack 发回 Channel A |
| 2026-09-11 | **WO-03 Gate = REJECT-REWORK（36 PASS / AC-22 FAIL）**。AC-22：测试恒真自比较 + 22px Clip 未证明长文本包含 + helper 与 renderer 不是同一权威。AC-28 新 hash 暂不晋升。已修：`BuildMasterySelectorLayout` 单一权威、换行高度纯函数、open/cancel 零 mutation、PlayMode integration。返工后门：EditMode **490/490**、PlayMode **20/20**。Evidence Delta：`S6P_WO_03_REWORK_AC22.md` |
