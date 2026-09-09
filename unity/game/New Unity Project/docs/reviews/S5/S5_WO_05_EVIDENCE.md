# S5_WO_05_EVIDENCE — Evidence Pack（S5-WO-05 Build & Interaction Validation）

**Work Order**: S5-WO-05 — Build & Interaction Validation（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 WO-04 Gate Review=ACCEPT/Follow-up NONE 放行 Phase 4）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ da4ca87

## Changed Files

| 类别 | 文件 |
|---|---|
| Changed Runtime Product Files | **NONE**（WO-05 §3 政策遵守——零生产行为改动） |
| Changed Test Files | `Assets/Tests/EditMode/S5BuildInteractionTests.cs`（新建 12 项验证测试；唯一代码变更=测试） |
| Changed Markdown | `S4R_REFERENCE_BUILD_CANDIDATES.md`（UPDATE REQUIRED：§S5-WO-05 Validation Records 三候选逐条记录+池假设审计记录）、`S4R_CAPABILITY_LEDGER.md`（evidence linkage only）、`S4R_MECHANIC_MATRIX.md`（candidate coverage evidence only）、`S5_WO_04_EVIDENCE.md`（Gate Review 结果落库）、`规划AI会话.md` |
| Changed Assets/Scenes/Prefabs: **NONE** | `S5_PLAN.md`/`S5_LINK_CONTRACT.md`/`S5_AFFIX_ADMISSION.md`/`RUNTIME.md`/`COMBAT_MATH.md`：**VERIFY ONLY 零改动**；`DECISIONS.md`：零新增（无新产品决策需要记录） |

## Entry Baseline

EditMode Before: 302/302；PlayMode Before: 11/11；Affix Count: 21。

## Validation Scenarios

**Validation Scenarios Total: 12 项测试（V1-V6 矩阵 + 词缀交互矩阵 4 + 有意义选择 1），覆盖 3/3 canonical skills + 4/4 新词缀。**

### RC-M Scenario（烈刃转火）

RC-M Exact Supports: Q[0]=火焰转化（ConvertPhysToFire Flat 0.50）+ Q[1]=残暴（MorePhysical More 0.40）——golden 合法。
RC-M Affixes: 坚韧（Tenacity，手套，Strength/Flat 12）。
RC-M Link Mode: legacy 单连接（组0 双 Support 位）。
RC-M Result: **PASS**（转化 0.5 + MorePhys 1.4 + Strength 32；词缀/连接正交实证）。

### RC-P Scenario（分裂弹幕）

RC-P Exact Supports: 组0 位=Fork+Faster（legacy 容 2）；组1 位=恰 Fork（组 1 容 1=容量真相如实）。
RC-P Affixes: 迅疾（SwiftBreeze，武器，AttackSpeed/Increased 0.16）。
RC-P Link Mode: 双位验证（legacy 组0 / 武器改挂弹道组1），同 seed 9u。
RC-P Result: **PASS**（ForkSpawns==2 双位等价=真实 runtime 分裂非数据模型断言）。

### RC-A Scenario（灰烬领域）

RC-A Exact Supports: V6=Concentrated（Area 专属 ✓，legacy 头盔位）；V2B=Combustion（Area ✓，经武器组1）——候选「二选一」两变体独立验证，取舍仍留正式锁定阶段。
RC-A Affixes: V6=坚韧（武器）+迅疾（身体）；V2B=无词缀（隔离观测）。
RC-A Link Mode: V6=武器改挂弹道双连接在场；V2B=Area 经组1。
RC-A Result: **PASS**（AreaDamageMore 0.4 仅进 Area 聚合 [RawMore=1.4 含中性 1]；跨组零泄漏）。

**候选描述 vs 实际执行配置的区分**：逐条记录于 `S4R_REFERENCE_BUILD_CANDIDATES.md` §S5-WO-05 Validation Records（candidate description 保持原文；exact configuration actually exercised 如上）；兼容规则零修改强制（Concentrated 仅 Area/Fork 仅弹道按 golden 真相照用）。

**Canonical Skills Covered: 3/3**（Melee=V1/RC-M；Projectile=V1/V2/V3/V4/RC-P；Area=V1/V2B/V6/RC-A）。

## 验证矩阵对账（WO-05 §4 V1-V6）

- **V1 Legacy Single-Link（3/3 skills）**：三技能各一真实负载（含 S5 词缀在场）——映射源照常生效、legacy 容量 2/2/1 不变、Support 照常执行（MorePhys 1.4）、新词缀不改变连接源解析（`LinkSkill1=None`+组0 标注+无组1 呈现）。
- **V2 Valid Two-Link ×2**：配置 A（武器组1 弹道+残暴 / 胸甲组1 范围+迅捷=**组0/组1 不同 Support 身份**）；配置 B（武器组1 范围+燃尽）——两配置唯一源成立、容量=域值、各组 Support 生效恰一次、零跨组继承、源标注/Tooltip 与 runtime 一致。
- **V3 Stat-Support Cross-Position（含 S5 词缀构筑）**：坚韧在场，残暴组0/组1 双位 MorePhys=1.4 等价；词缀不改变 Support 所有权/兼容（Strength 32 与连接模式无关）。
- **V4 Mechanic-Support Cross-Position（RC-P 覆盖）**：Fork 真实分裂双位 ForkSpawns==2（ArenaSim 执行，非模型断言）。
- **V5 Invalid Third Group**：第二物品再挂同技能=确定性拒绝（「已被其它装备改挂」）、零状态损坏、无 LinkSkill2/group2 状态（反射断言）。
- **V6 Cross-Group Isolation Under Affixes**：双连接+S5 词缀负载——组1 Support 不影响 host 组0（MorePhys=1）、Area 专属 Support 只服务 Area、词缀走全局/装备 stat 路径（攻速进技能聚合/坚韧进玩家面板）、**词缀不成为连接组成员机制**；当前绑定预判可用+约束冲突候选确定性拒绝（正交）。

## 词缀交互矩阵（§5，AC-08/09/10）

- **SwiftBreeze Interaction**：改挂不重复/不抑制（Recovery 与 RawIncreased(AttackSpeed)=0.16 改挂前后逐位相等；组1 技能同吃全局词缀轴=既有全局语义非组机制）。
- **Ironhide Interaction**：合法非 Belt 槽（胸甲）同基面 ×1.22 照常；**Belt 负面在多连接负载下保持为真**（定向拒绝+零消耗）。
- **Insight Interaction**：Intelligence 32 + 既有 Intelligence→Mana 换算贯通（MaxMana=基线+12×0.5）。
- **Tenacity Interaction**：Strength 32 + 既有 Strength→Life 换算贯通（MaxLife=基线+12×0.5）。
- **Ironhide Belt Negative**: PASS（AC-09）。
- **Affix-Link Orthogonality**（AC-10）：词缀不选组/不改归属/不绕容量/不改兼容/不复制 Support 效果（V3/V6+矩阵测试实证）。

## 有意义选择证据（§7）

**≥3 个可区分合法负载场景**（`MeaningfulChoice` 测试）：①legacy 近战 转化+残暴+坚韧（Conversion 身份，容 2）②拆分双连接 Fork 机制身份（容 2→0+1 真实取舍）③双组不同 Support 身份（残暴/迅捷跨两组）——三场景（技能/连接模式/Support 身份）两两不同，仅用当前已支持机制。**零**"balanced/optimal/meta/endgame-viable/Reference Build certified" 声明。

## 池假设审计（§8，AC-13）

- **Stale Assumptions Found: 0**（grep 全测试目录证据：无「当前目录/池==17」断言）。
- **Historical Assertions Retained（with rationale）**: `S2BaselineAffixCount=10`（S2 历史基线）；`S5AffixBreadthTests` 内 17=stable-ID 常量（SwiftBreeze ID=17）/"Before 17+4=21"计数语义/旧 ID 稳定域 0-16 扫描；其余 17 命中=无关（17k 顶点/meta guid/文档 §17/0.017 缩放）。
- **Test-Only Corrections**: 无需（WO-04 已更新两处护栏为 21；随机断言均为池基数无关的存在性断言）。

## Reference Build Status（§2/§12）

**Reference Build Status**: RC-M/RC-P/RC-A 全部保持 **CANDIDATE / NOT LOCKED**（逐条记录已入 `S4R_REFERENCE_BUILD_CANDIDATES.md`；未改名 Locked/Certified；未声明最优构筑/未选 RC-A 变体取舍/未立 DPS/通关目标）。
**Formal Reference Build Coverage Changed: NO**（Ledger 覆盖字段仍 N/A）。

## Gate Results

| 门 | 结果 |
|---|---|
| EditMode | **PASS 314/314**（302 基线 +12 S5BuildInteractionTests；前置 302 零删除/零削弱） |
| PlayMode | **PASS 11/11** |
| Content Audit | **PASS / fresh=YES / failures=0**（Affix count=21；dead/unconsumed=0 delta；无第 5 词缀） |
| Quick Gate | **PASS**（Gate: PASS） |
| Performance Gate | NOT_RE_RUN（§13：产品 runtime 未变，Phase 4 不要求；完整闭口留给 Phase 5） |
| Production Simulation | NOT_RE_RUN（§13：完整确定性 Production Simulation+性能闭口=Phase 5 保留项） |

门迭代记录（全部测试自身问题，产品代码零改动——Runtime Product Delta=NONE 保持）：①`BuildPlayerHit` 缺 `default(Dummy)` 实参（编译期捕获）；②V2B/V6 starter 词缀污染（灼燃 IgniteChance 前项/新池词条）→ 清空装备+全新物品隔离；③RawMore 语义=中性 1+和（与 WO-03 F1 同源先例对齐）；④V6 正交断言前提错误→修正为「当前绑定预判可用+约束冲突候选确定性拒绝」（预判拒绝正是正确域语义）。

## Delta 声明

- **Capability Delta: NONE**；**Mechanic Support Delta: NONE**
- **Runtime Product Delta: NONE**（§3 政策遵守；Defects Found=NONE——未发现 BL-021.A2/BL-002.A1 缺陷）
- **Canonical Data Delta: NONE**；**Content Delta: NONE**；**Authorization Delta: NONE**
- **Affix Count: 21**；**Link Groups Max: 2**
- **Socket Color Introduced: NO**；**Gem Level/Quality Introduced: NO**；**New Affix Family: NO**；**Prefix/Suffix: NO**；**Tier/ModGroup: NO**（§9 负面表面全过——组合场景零依赖未支持能力，无 fixture 伪造）

## Drift / Source-of-Truth Conflicts / Forbidden Expansion

**Drift: 0**；**Source-of-Truth Conflicts: 0**；**Forbidden Expansion Audit: PASS**（零第 5 词缀/零新族/零 P-S/Tier/ModGroup/零权重平衡系统/零孔色/零宝石成长/零第三组/零新 Skill/Support/槽位/零 Unique/Aura/Curse/Flask/Jewel/Trigger/Progression/Map Tier/Boss/Atlas/Endgame/Deep Craft/Persistence/Voice/BL-024/Content Batch/Content Factory/零"为未来构筑"生产重构）。

## Open Questions（供规划 AI）

1. 无阻塞问题。RC-A 的 Concentrated/Combustion 取舍保持开放（两变体均已独立验证），留正式锁定阶段决策。
2. V6 揭示一个交互语义事实（非缺陷）：在「武器改挂弹道+残暴组1+胸甲 3 孔」配置下，把胸甲改挂近战的预判=确定性拒绝（残暴将溢出胸甲组0）——共享校验器的假设态校验把这类「换绑会破坏现有放置」的转换也拦住了。这与「合法移交」语义一致（移交后唯一源仍恰一且容量合法才放行），如你认为该场景应放行（自动迁移 Support=禁，故我们认为拒绝是唯一合规解），请明示。

## Recommended Next WO

**S5-WO-06 — Production Closure**（全 S5 收口门：全测试/Audit/零死声明/golden/词缀可达/**3 次等价 Production Simulation 同 hash**/canonical 锁定硬件性能/-IncludeArtProduction/生产视觉分辨率/Drift=0/Forbidden PASS）——由你按 Gate Review 结果下发。

## Gate Review 结果（2026-09-09 规划 AI 回文，落库登记）

**S5-WO-05 = ACCEPT，Follow-up: NONE**（Gate 表 28 项全 PASS；EditMode 314/314/PlayMode 11/11/Audit fresh/Affix 21/Quick Gate PASS；RC 状态保持=CANDIDATE/NOT LOCKED；Formal Reference Build Coverage=UNCHANGED；stale pool assumptions=0）。两 Open Questions 裁定：①**RC-A Concentrated/Combustion 继续保持开放**——两变体均已独立成立；WO-06 不得选定任一为正式答案、不得声称平衡、不得立 DPS/通关/最优标准、不得晋升 Formal Reference Build Coverage（Production Closure 只验证生产稳定性，不承担产品 Build 取舍）；②**V6 换绑拒绝=接受，正确行为非缺陷**——换绑后终态会使既有组容量溢出时，共享 validator 必须在 mutation 前拒绝；禁自动迁移/截断/删除/重排 Support（atomic-capacity contract）；**「合法移交」仅适用于终态唯一源且全部容量约束合法的情形**——V6 确定性拒绝与合法 host handoff 完全一致。**S5-WO-06 已放行（最终生产闭口轮；完成后 STOP implementation，下一步=S5 Director Final Gate）。**
