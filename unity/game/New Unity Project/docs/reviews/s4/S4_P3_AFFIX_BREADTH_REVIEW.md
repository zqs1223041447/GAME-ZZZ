# S4_P3_AFFIX_BREADTH_REVIEW — Itemization Breadth v1（工作令 S4-P3-AFFIX-APPLICABILITY-BREADTH）

**日期**：2026-09-09　**Baseline**：main @ 1900b6a（S4 Phase 2 COMPLETE）→ 本轮 HEAD 见文末
**Verdict**：**PASS — S4 PHASE 3 ITEMIZATION BREADTH COMPLETE**（Affix delta=4；Phase 4 未自行启动）

## 1. Affix Truth Audit（§一，以当前代码为准）

| ID | Affix | Rows | Stats/ModOps | Min/Max | Current Slot Restriction（本轮前） |
|---|---|---|---|---|---|
| 0 | 附加物理 AddPhys | 1 | AddedPhysical/Flat | 3-8 | 无 |
| 1 | 物理伤害 IncPhys | 1 | PhysicalDamage/Increased | 12-28% | 无 |
| 2 | 附加火焰 AddFire | 1 | AddedFire/Flat | 4-10 | 无 |
| 3 | 火焰伤害 IncFire | 1 | FireDamage/Increased | 12-28% | 无 |
| 4 | 生命 Life | 1 | Life/Flat | 12-28 | 无 |
| 5 | 护甲 Armour | 1 | Armour/Flat | 15-45 | 无 |
| 6 | 闪避 Evasion | 1 | Evasion/Flat | 15-45 | 无 |
| 7 | 火焰抗性 FireRes | 1 | FireResistance/Flat | 8-18% | 无 |
| 8 | 命中 Accuracy | 1 | Accuracy/Flat | 20-60 | 无 |
| 9 | 暴击率 Crit | 1 | CritChanceIncreased/Increased | 20-50% | 无 |
| 10 | 灼燃 IgniteFire | 2 | FireDamage/Inc + IgniteChance/Flat | 12-28% / 10-20% | 无 |
| 11 | 锐击 AccCrit | 2 | Accuracy/Flat + CritChanceIncreased/Inc | 20-50 / 15-30% | 无 |
| 12 | 熔铸 PhysFire | 2 | PhysicalDamage/Inc + FireDamage/Inc | 12-28% ×2 | 无 |

- Stable IDs：**有**（AffixId byte enum 0-12=目录位；本轮 13-16 只追加）。
- Rarity/weight：**无 weight 系统**（等概率 unique 选取，沿用现有 deterministic selection；§十四 不新建 weighted pool）。
- Drop roll path=RollItem（DropGear/TryRandomCraft 共用）；Directed Craft=TryDirectedCraft（蚀刻剂写入）；tooltip=AffixLine 通用格式。

## 2. Runtime-consumed Stat Proof（§二）

新词缀 4 条 6 行全部 Stat ∈ `ContentAuditS2Tests.RuntimeConsumedStats`（canonical 白名单，28 项）：
- **AttackSpeed**：`SliceSession.ResolveSkillDef` → `RawIncreased` → Recovery/Cooldown 缩短（SliceSession.cs:827）
- **CritChanceAdded**：`BuildPlayerHit` → `req.AddedCrit` → `CombatMath.CritChance`（:872）
- **Armour**（Increased 新 ModOp 维度）、**Life/Flat、Accuracy/Flat、FireResistance/Flat**：RecalcPlayer/CollectSkillMods/AddItemDefensive 既有消费。
- **StatId delta=0 / ModOp delta=0**（EditMode `AllAffixes_OnlyConsumedStats_NoNewModOp` 钉死）。

## 3. Branch 判定（§三）

**Branch B — No Applicability Exists**（本轮前 AffixDef 无任何 slot 字段/限制 seam）。加入最小字段：
- `AffixDef.AllowedSlots : ushort`（bit i=EquipSlot i）；**0=不限槽**=既有 13 词缀默认语义（向后兼容零改动）。
- 唯一 canonical predicate：**`AffixDef.IsApplicable(EquipSlot)`**（§五）。构造入口=`AffixCatalog.SlotsMask(params EquipSlot[])`（禁手写位运算）。
- Parallel predicates：**0**（Drop/RollItem、TryRandomCraft、TryDirectedCraft、Content Audit、Production Report 全部复用同一 predicate——§五 "禁止各自判一遍" 收口）。

## 4. Backward Compatibility（§四/§二十一）

- `OldAffixes_StableIds_AndAllSlotsEligible_Preserved`：旧 13 ID 位=Id 零漂移、AllowedSlots=0、六槽全可用。
- 旧四槽 eligible 池等价：unrestricted=13 全池（本轮前后同为 13；新 4 条全部 restricted 不进入旧四槽池——`RollItem_RespectsApplicability` 断言旧四槽永不 roll 新专属词缀）。
- Audit stable ID 校验新增：目录位≠Id → 结构/契约失败（防 reorder 漂移）。

## 5. New Affixes（§七-§十一）

| ID | Name | Rows | Stat/ModOp | Min/Max | AllowedSlots | Role |
|---|---|---|---|---|---|---|
| 13 | 迅握 SwiftGrip | 1 | AttackSpeed/Increased | 8-14% | **Gloves** | offensive handling/speed |
| 14 | 锋锐 KeenEdge | 2（hybrid） | CritChanceAdded/Flat + Accuracy/Flat | 3-7% / 15-35 | **Gloves** | precision（与 AccCrit 差异=ModOp 组合：CritAdded 而非 CritIncreased） |
| 15 | 壁垒 Bulwark | 1 | Armour/Increased | 10-22% | **Belt** | survivability/armor（Armour 首个 Increased 维度） |
| 16 | 韧脉 VitalWeave | 2（hybrid） | Life/Flat + FireResistance/Flat | 15-30 / 5-10% | **Belt** | survivability/health+resistance（新 Stat 组合） |

- **为什么是 4 条**：现有 Runtime Stats 恰好支撑 4 条无垃圾组合（2 Gloves+2 Belt 理想形态达成）；未凑第 5 条（§七 上限 4）。
- 禁 Tier/Prefix-Suffix/Weight（§十二/§十三/§十四）：全部未触碰；hybrid 复用既有 2-row 管线（§十一）。
- Gameplay formula 0 delta（§三十）：新词缀只走既有 Modifier/StatBag/CombatMath 管线，无 CombatRules 改动（PlayMode 实证：迅握 equipped → ResolveSkillDef Recovery 缩短）。

## 6. Slot Pools（§十五/§二十/§二十二）

| Slot | eligible | 构成 |
|---|---|---|
| Weapon/Body/Helmet/Boots | **13** | 全部不限槽旧词缀（零缩水） |
| Gloves | **15** | 13 + 迅握 + 锋锐 |
| Belt | **15** | 13 + 壁垒 + 韧脉 |

- Empty pool：**不可能**（任一槽 ≥13；audit 核入结构/契约失败 + `NoAffix_WithZeroApplicableSlots`）。
- Drop：RollItem 构造 slot-eligible 池（canonical catalog 顺序，禁 HashSet 序）后 SeededRng 无重复选取（§十六/§十九）；Gloves 不 roll Belt 专属、反之亦然（EditMode+PlayMode 双实证）。
- Random Craft：TryRandomCraft 经 RollItem 重掷 → **同一 predicate**（先过滤后 roll，非 roll 后 discard）。
- Directed Craft：非法 slot+affix → **deterministic reject**（不消耗蚀刻剂、不半写入；§十七）。
- SeededRng 唯一随机源（§十八）；RNG 取值序列变化=legitimate content-set change（池构造方式变化），如实申报；同 seed 重放逐字段一致（EditMode+smoke 双验证）。

## 7. Generation Smoke（§三十三）

- `GenerationSmoke_HundredsRolls_NoIllegalPair_RepeatDeterministic`：固定 seed 矩阵（40 轮 × 6 槽，Rare/Ordinary 交替）≈ 960 物品 / ~2900 词缀行；**非法对=0**；新 Gloves/Belt 词缀可达（`RollItem_RespectsApplicability` 80-seed 扫描）；同 seed 序列逐字段一致。未做成完整 10k simulator（§四十三，留给 Phase 4）。

## 8. Production Tooling（§二十五/§二十六）

- Production Report 再生：affixes=**17**（15-17 预期带内）；新增 `affixApplicability` 段：total=17 / unrestricted=13 / restricted=4 / bySlot：Weapon 13 / Body 13 / Helmet 13 / Boots 13 / Gloves 15 / Belt 15——全部由 canonical predicate 派生，零手写 expected。
- 其它 canonical counts 不变：skills=3/supports=7/passives=16/enemyKinds=5/mapMods=3/equipmentSlots=6（§三十五 保持项全绿）。
- Verdict FAIL 条件（§二十六）落实于 **Audit**（报告 verdict 继承 audit 真相）：未知 mask bit / 目录位-ID 漂移 / 任一槽空池 → StructuralProblems → Audit FAIL → Report FAIL。Voice/unused tags 继续 non-failure。
- Audit fresh **PASS failures=0**；mask/slot/行校验负向测试 `Audit_Validator_CatchesSyntheticInvalidMask` 证明规则能抓坏合成输入。

## 9. Tests（§三十二/§三十四）

- EditMode **+10**（242/242）：`AffixApplicabilityTests`——旧 13 稳定+全槽保持 / 新词缀行+槽限 / 全词缀只消费白名单 Stat / per-slot 池派生 / 零槽词缀禁 / RollItem 合法+确定性+80 种子新词缀可达 / Directed Craft 拒绝与写入 / 合成坏 mask 校验 / 生成 smoke / Report per-slot 派生。
- PlayMode **+1**（11/11）：`AffixApplicability_SessionLoop_NoIllegalAffix`——真实 RollItem 路径可达迅握→equip 后 recovery 缩短（Modifier 生效）→随机制作后仍合法→Belt 壁垒写入成功、Gloves 拒绝（永不注入）→旧槽武器替换正常→tick 无异常。

## 10. Verification（§三十八-§四十）

- SelfTest **PASS**；EditMode **242/242**；PlayMode **11/11**；Audit **fresh PASS**；Build **PASS win64**；PlayerRuntime **PASS exit=0 @2560×1440 densities=100/200/300**；tracked clean。
- Performance required：**NO**（§三十九——applicability 只运行在 loot/craft/content generation 低频路径；per-frame stat aggregation/combat hot path 零改动；RollItem 的 eligible 池构造在内容生成时一次完成）→ 未跑 canonical Performance。
- Art Gate：**NOT REQUIRED**（enemy visuals/renderers/Animator/ProjectSettings rendering 全 0 diff）。

## 11. Scope（§三十一）

- Affix delta：**+4**（13→17，IDs 13-16 追加）+ applicability metadata（AllowedSlots 字段）。StatId/ModOp/Condition/Effect/Trigger/EquipSlot/Skill/Support/Passive/Enemy/MapMod delta：**0**。UI delta：0（新词缀走既有文本渲染）。Art delta：0。Audio delta：0（Voice 继续 DEFERRED，§四十二）。
- Stage0 全锁延续（§四十一——applicability ≠ deep Craft 解锁）。导演真相全保持。

## 12. Phase Status / Reviewer（§三十六）

- Phase 0/1/2=COMPLETE；**Phase 3=COMPLETE（Itemization Breadth v1）**；Phase 4=NOT STARTED（10k simulator 未自行启动，§四十三）；S4 overall=IN PROGRESS；S3=COMPLETE；Voice=DEFERRED BY DIRECTOR。
- Findings：①Quick Gate 首跑 242/242+11/11 一次全绿（predicate 收口未破坏任何既有契约，含 DirectedCraft/同 seed 重放旧测试）；②历史 CONTENT_AUDIT 冻结报告中的「4 槽全部」措辞属冻结历史不回写，当前快照报告已按新槽位语义再生。
- Fixes：无遗留。
- Final verdict：**PASS — S4 PHASE 3 ITEMIZATION BREADTH COMPLETE**

## 13. Remote Sync

- Commits：本轮 feat/content/test/tooling/docs 数笔（见 git log）；普通 push，未 force；Local HEAD=Remote HEAD（见完成汇报）。
