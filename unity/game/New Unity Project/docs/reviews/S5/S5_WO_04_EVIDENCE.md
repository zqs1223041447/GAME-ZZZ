# S5_WO_04_EVIDENCE — Evidence Pack（S5-WO-04 Bounded Affix Breadth）

**Work Order**: S5-WO-04 — Bounded Affix Breadth（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 WO-03 Gate Review=ACCEPT/Follow-up NONE 放行 Phase 3；「执行授权已生效：可立即开工」）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ d711906

## Changed Files

| 类别 | 文件 |
|---|---|
| Changed Runtime Files | `Assets/Runtime/Core/Gameplay/Catalogs.cs`（恰 AffixId 枚举 17-20 追加 + AffixCatalog 4 条 def；其余零改动） |
| Changed Test Files | `Assets/Tests/EditMode/S5AffixBreadthTests.cs`（新建 11 项）；`Assets/Tests/EditMode/ContentAuditS2Tests.cs`（count-guard 17→21 按裁定扩展）；`Assets/Tests/EditMode/AffixApplicabilityTests.cs`（计数护栏 15/17→21/21） |
| Changed Markdown | `S5_AFFIX_ADMISSION.md`（implemented 状态行）、`S5_SCOPE_LEDGER.md`（BL-002.A1=IMPLEMENTED）、`DECISIONS.md`（WO-04 落地语义+WO-03 两裁定回填）、`RUNTIME.md`（S5 词缀批次段）、`S4R_CAPABILITY_LEDGER.md`（Equipment/Affix 行 S5 跟踪=21，无新 family）、`S4R_MECHANIC_MATRIX.md`（S5 跟踪块更新）、`S5_WO_03_EVIDENCE.md`（Gate Review 结果落库）、`规划AI会话.md` |
| Changed Assets/Scenes/Prefabs: **NONE** | COMBAT_MATH.md：**VERIFY ONLY 零公式改动（未触碰）** |

## Manifest Landed（AC-01/02/03 精确清单/计数/语义）

| # | AffixId | 名 | Stat / Op | Range | Applicability | 备注 |
|---|---|---|---|---|---|---|
| 17 | `SwiftBreeze` | 迅疾 | AttackSpeed / Increased | 0.10–0.16 | 不限槽（AllowedSlots=0） | 单行 |
| 18 | `Ironhide` | 铁骨 | Armour / Increased | 0.10–0.22 | Weapon/Body/Helmet/Gloves/Boots；**Belt 排除** | 单行；排除=GAME-ZZZ 有界适用性决策（溯源/适用性分离表述保持） |
| 19 | `Insight` | 睿智 | Intelligence / Flat | 6–12 | 不限槽 | 单行 |
| 20 | `Tenacity` | 坚韧 | Strength / Flat | 6–12 | 不限槽 | 单行；候选 20 权威=坚韧，旧开阔（AreaRadiusMore/More）已废弃不恢复 |

- **Affix Count Before: 17 / Added: 4 / After: 21**（`AffixId.Count==21` + `AffixCatalog.Count==21` 断言）；Stable ID=位值（17-20），旧 0-16 逐位断言零漂移；四条全单行（`SecondValue*` 路径零触碰）。
- **Count Guards Updated**（AC-12）：①`ContentAuditS2Tests` 结构护栏 `> 10+3+4(=17)` → `> 10+3+4+4(=21)`（护栏注释引用本清单与 BL-002.A1；语义不放宽——新上限仍硬上限）②`AffixApplicabilityTests` `≤17` → `≤21` 且下限 `≥15` → `≥21`（不得缩水）③Production Report 计数=canonical 派生自动更新（报告由测试再生，禁手填）。**保留的 17** 均为非当前目录计数语义（S2BaselineAffixCount=10 基线/历史文档事实），Evidence 已核对无冒充。

## Applicability Truth Owner（AC-05）

- **唯一 owner=`AffixDef.IsApplicable(slot)`（AllowedSlots 位掩码）**；铁骨 AllowedSlots=SlotsMask(Weapon,Body,Helmet,Gloves,Boots)。随机池（RollItem eligible 枚举）/定向制作（TryDirectedCraft applicability 门）/审计/报告全部同源调用——**零 production rule duplication**（测试断言「每槽池中 18 的存在性==IsApplicable」逐槽对拍）。
- **Ironbone Belt Rejection**（AC-06）：`IsApplicable(Belt)=false` + 其余五槽 true；定向制作 Belt 拒绝（「铁骨 不能出现在腰带」）**不消耗蚀刻剂、无半写入**（资源与词缀快照断言）；Belt 随机池恒不含 18（AC-08 Invalid-Slot Reachability：200 seeds 全绿断言）。

## Generation/Craft Path（AC-07/10）

- **Reachability 迅疾/铁骨/睿智/坚韧**：①定向制作路径——四条各在合法槽全新物品上写入成功，掷值∈[Min,Max]（确定性）；②随机生成路径——SeededRng 有界 sweep（Body 400 seeds，Rare）四条全部出现（无 test-only fallback/无强制命中 API/无权重修补）。
- **Invalid-Slot Reachability**：Belt 200 seeds 掉落恒不含铁骨。
- **Duplicate Craft Regression**：既有 duplicate rejection 不变（「已在该装备上」，不消耗）；Rarity≥3=Rare 既有语义不变；定向目录枚举自动纳入新词条（Craft 面板=AffixCatalog.Count 迭代，无硬编码清单）；**不新增 craft operation**。
- **WO-03 语义保持**：whole-item random reroll 的 LinkSkill1 legacy reset 未重新设计（保持 WO-03 已接受语义）。

## Runtime Consumption（AC-09 零死声明）

| 词条 | 既有消费轴 | 证据（清空装备隔离后单因子） |
|---|---|---|
| 迅疾 | AttackSpeed 聚合 → `ResolveSkillDef.Recovery ×= 1/(1+speed)` | Recovery = baseline/1.16（±0.02% 精度断言） |
| 铁骨 | Armour 聚合 → `CombineStat(base+flat, inc, more)` | 同基面换装对拍：armour1 = armour0×1.22（其它槽词缀噪声被双测点差分隔离） |
| 睿智 | Intelligence Flat → 属性+`ManaPerInt` 换算 | `PlayerStats.Get(Intelligence)==32`（20 基础+12）+ 会话属性一致 |
| 坚韧 | Strength Flat → 属性+`LifePerStr` 换算 | `PlayerStats.Get(Strength)==32` + 会话属性一致 |

- **New stat semantic: NONE / New modifier semantic: NONE**；Dead declared modifier increase: **0**（Content Audit 全量词缀行校验 PASS：StatId 合法/ModOp≤Override/Max≥Min/RowCount-Format2 一致）；Unconsumed declared stat increase: **0**（四 stat 均在 `RuntimeConsumedStats` canonical 白名单）。

## Generic Tooltip/Item Presentation（AC-11）

- 通用路径零改动：`AffixLine`（Format 渲染「16% 攻击速度」等，占位符零残留）→ `SliceTooltipModel.ItemCard.Body` 自动包含新词条行；**无 entry-specific UI branch**（无新 UI 代码文件）。

## Multi-Link Regression（AC-13 冻结回归）

- WO-02/WO-03 前置 291 项（14 域核+31 集成+246 历史回归）**零删除/零削弱全 PASS**；golden Group0=**21/21**、Group1=**21/21** 维持；新增联动测试：新词缀物品（铁骨+迅疾）参与改挂路径——组0 容量 0/组1 容量 1/组1 Support 装配全部照常。

## Gate Results

| 门 | 结果 |
|---|---|
| EditMode | **PASS 302/302**（291 基线 +11 S5AffixBreadthTests，对账：清单语义 1/铁骨守卫 1/定向可达 1/随机 sweep+非法槽 1/运行时消费 4/craft 回归 1/通用呈现 1/Multi-Link 联动 1；零删除/零削弱） |
| PlayMode | **PASS 11/11** |
| Content Audit | **PASS / fresh=YES / failures=0**（count-guard 更新后审计结构绿） |
| Quick Gate | **PASS**（Gate: PASS；性能门不重跑——恰为有界目录条目+零 hot-loop 架构改动+零性能 drift 迹象；最终复验按计划 Phase 5） |

首跑迭代记录（全部为**测试自身问题**，产品代码零回退）：①CS0266（`.Affix0` 字段类型直取→改 `AffixIdAt(0)`）②腰带初始未装备 `Inventory[-1]` 越界（同 WO-02 手套/腰带先例——自建物品）③④新词缀入池后 starter roll 可能含新词条+蚀刻剂初始仅 1（清空装备隔离+资源补足）——均为测试前提修正，非 drift/scope/削弱。

## Capability State / Mechanic State

- **Equipment/Affix breadth 扩至 21 条内容**（Ledger/Matrix 已记录）；**不创建新 capability family、无 mechanic promotion**（四条全部复用既有 Supported mechanic/stat path）；Socket/Link 保持 Partial（Multi-Link 集成已实现、Socket Color 未授权）；无其它晋升。

## Delta 声明

- **Capability Delta: NONE**（无新 family）
- **Mechanic Support Delta: NONE**
- **Runtime Delta**: BL-002.A1 bounded Affix integration only（Catalogs.cs 恰 4 def+枚举；guard 测试更新）
- **Canonical Data Delta: NONE**（无新 canonical-data schema/source/pipeline；四条沿用 Phase 0 已审核人工 PoEDB provenance）
- **Content Delta: +4 approved Affix entries**（恰锁定清单，无第 5 条）
- **Authorization Delta: NONE**
- **Affix Count: 21**（17+4）；**New Affix Family Introduced: NO**；**Prefix/Suffix Introduced: NO**；**Tier/ModGroup Introduced: NO**；**Socket Color Introduced: NO**；**Gem Level/Quality Introduced: NO**；**Link Group Count Changed: NO**；**BL-024 Required: NO**

## Drift / Source-of-Truth Conflicts

**0 / 0**（门迭代=测试自身修正，产品代码零回退；文档全部同轮同步）

## Forbidden Expansion Audit

**PASS**——零第 5 词缀/零 manifest 变更/零新族/零新 Stat/ModOp 语义/零 P-S/Tier/ModGroup/零新槽/Ring/Offhand/Amulet/Unique/零新 Skill/Support/零孔色/零宝石成长/零第三组/零 Link 合同改动/零持久化/零 Progression/Endgame/Content Batch/BL-024 pipeline/零 test-only production fallback/零特殊权重修补。

## Open Questions（供规划 AI）

1. 无阻塞问题。铁骨 Belt 排除已按「GAME-ZZZ 有界适用性决策」表述落库（溯源/适用性分离），如需进一步措辞调整请明示。
2. starter 掉落池现已包含四条新词缀（同 applicability truth 自动纳入）——既有测试零改动全绿（确定性 seed 下未命中新词条的历史断言不受影响）；后续若有测试依赖「池=17」的具体概率假设，将在其工作令内显式更新。

## Recommended Next WO

**S5-WO-05 — Build & Interaction Validation**（联合验证 legacy link/valid multi-link/invalid third-link/cross-group isolation/四条新词缀 slot/application/reachability/新词缀×Link 构筑交互；RC-M/RC-P/RC-A 作 validation scenarios 不自动晋升 Locked Reference Builds）——由你按 Gate Review 结果下发。
