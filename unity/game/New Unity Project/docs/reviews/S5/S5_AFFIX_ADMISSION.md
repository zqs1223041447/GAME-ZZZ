# S5_AFFIX_ADMISSION — 有界词缀广度准入清单（S5-WO-01 锁定；WO-01 评审后按裁定修订；AC-10/11/12 交付物）

**性质**：BL-002.A1 的唯一权威词缀清单。**Locked N = 4**（1≤4≤6 ✓）；计划目录数 = **17 + 4 = 21**。N 事后变更需新规划 AI 工作令。
**准入规则（S5_PLAN §4.1 逐条满足）**：唯一外部 canonical 源=PoEDB（人工溯源，不触发 BL-024）；复用已支持 runtime stat/modifier 语义；走现有单一 applicability truth（`AffixDef.IsApplicable` + `AllowedSlots` 位掩码）；现有装备槽语义；经现有生成路径可达（随机掉落/洗炼池=IsApplicable 自动过滤；定向制作=目录枚举 + applicability + duplicate guard——已核 `TryDirectedCraft` 按目录枚举，无硬编码清单，新条目自动可达）。禁止：新词缀族/新 ModOp 语义/Prefix-Suffix/Tier/ModGroup/新槽/新资源。

## 锁定候选（4 条；Stable ID 追加 17-20，旧 ID 零漂移；全部单行词缀）

| # | 候选 ID | 命名 | Stat / ModOp | Min–Max | 槽位（AllowedSlots） | PoEDB canonical 溯源 | 复用的既有语义路径 | 构筑用途 |
|---|---|---|---|---|---|---|---|---|
| 1 | `AffixId.SwiftBreeze`(17) | 迅疾 | `AttackSpeed` / `Increased` | 0.10–0.16 | 不限槽（mask=0） | PoE 攻击速度increased 词缀族（武器/手套上 "of Fury" 类本地攻速词缀） | 与「迅握」同 stat/op 轴（迅握=Gloves 专属 0.08–0.14）；AttackSpeed 为已消费 stat | RC-P（弹幕攻速）/RC-M 通用进攻轴 |
| 2 | `AffixId.Ironhide`(18) | 铁骨 | `Armour` / `Increased` | 0.10–0.22 | Weapon/Body/Helmet/Boots/Gloves（**排除 Belt**） | PoE "% increased Armour" 本地护甲词缀族（**溯源仅证词条语义**=护甲%提高；「Belt 不可出」非 PoEDB 事实，而是 GAME-ZZZ 有界适用性决策——两者已按 S5-WO-01 评审要求分开表述） | 与「壁垒」同 stat/op 轴（壁垒=Belt 专属）；Armour Increased 已消费（壁垒在用） | 防御构筑轴（非 Belt 槽获得 %甲） |
| 3 | `AffixId.Insight`(19) | 睿智 | `Intelligence` / `Flat` | 6–12 | 不限槽（mask=0） | PoE "+X to Intelligence" 属性词缀族 | Intelligence 为已消费 stat（`ManaPerInt=0.5` 法力换算）；属性轴首次进入词缀池 | 资源/属性构筑轴 |
| 4 | `AffixId.Tenacity`(20) | 坚韧 | `Strength` / `Flat` | 6–12 | 不限槽（mask=0） | PoE "+X to Strength" 属性词缀族（**exact flat 语义匹配**——替换原「开阔 AreaRadiusMore/More」候选：规划 AI WO-01 评审裁定 PoE AoE 词缀为 increased 语义、与内部 More 轴不构成已证映射，按 Option B 替换为精确匹配候选） | Strength 为已消费 stat（`LifePerStr=0.5` 生命换算）；属性轴（第二轴） | 生存/属性构筑轴（RC-M/防御向） |

**原候选 4「开阔（AreaRadiusMore/More）」替换记录**：规划 AI WO-01 Gate Review 指出溯源表述（increased AoE）与实现运算（More）存在未证映射——按裁定 Option B 处理，替换为「坚韧 Strength/Flat」（exact-semantic-match）。N=4 不变；总量 21 不变；约束（无新族/无 Prefix-Suffix/无 Tier-ModGroup/无 BL-024）全部保持。原候选留档于本节，若未来出现「more AoE」PoEDB 实证可另行立令评估。

**逐条准入自查**：无新词缀族 ✓（四条全部复用既有 stat+op 组合语义）；无 Prefix/Suffix 语义 ✓；无 Tier/ModGroup 语义 ✓；单一 applicability truth ✓（AllowedSlots/IsApplicable）；生成/制作可达 ✓（随机池自动 + 定向制作目录枚举自动）；无 BL-024 ✓（人工溯源）；不产生死声明 stat ✓（四 stat 全部已在 `runtimeCoverage.consumedStats`）；稳定 ID ✓（追加 17-20 且位=Id；旧 ID 只追加不漂移）；`SkillId/StatId/ModOp` 零新增 ✓。

## Phase 3 实现登记事项（本单不动代码；实现工作令必须执行）

1. **Stable ID 追加**：`AffixId` 枚举追加 17-20 并更新 `Count=21`；`AffixCatalog` 在位 17-20 追加四条 def（位=Id，防「stable ID 漂移」审计断言）。
2. **计数护栏同步更新**：`ContentAuditS2Tests` 既有断言「`AffixId.Count > S2BaselineAffixCount + 3 + 4` → 结构问题（S4-P3 上限 17）」必须扩展为 S5 界（+4，总上限 21），护栏注释引用本清单与导演原子批准（BL-002.A1）；旧断言直接沿用会导致 Phase 3 审计结构红——此为**预期护栏更新**，不是放宽语义（新上限仍为硬上限，越界照样红）。
3. 词缀行校验（StatId 越界/运行期未知 Stat/ModOp ≤Override/Max≥Min/RowCount-Format2 一致）全部自动覆盖新条目，无需改校验逻辑。
4. 双行（组合词缀）本批不使用（四条全部单行）；`SecondValue*` 路径不触碰。

## 未入选候选（NB-3 溯源筛选记录；留后续批次，不入 N）

- 暴击基础率通用词缀（会侵蚀「锋锐」Gloves 专属生态位——留观）；
- 敏捷/力量 Flat 属性词缀（属性轴第二批再扩）；
- 物理伤害 More%、火抗% 增收变体（避免首批过度膨胀）；
- 双行组合新变体（首批保持单行，降低第二值路径风险）。
