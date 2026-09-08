# CONTENT_AUDIT_S3_CLOSEOUT

**当前 Content Audit snapshot（测试再生）**。文件名保留 CLOSEOUT 是因为它源自 S3 Phase 1/2 收口；**BATCH1 / R2 报告才是冻结历史 evidence**。

生成：EditMode 测试 `ContentAuditS2Tests`，failure-safe 顺序 **Collect → Render → Persist → Assert**（S3-M3：报告先于测试断言落盘——测试红 ⇒ 本快照同轮红，不遗留上一轮 PASS）。只覆盖现行切片：3 Active + 7 Support + 13 词缀 + 16 天赋 + 3 图词缀 + 5 怪。

## Verdict

- Audit completed: YES
- Verdict: PASS
- Failure count: 0

## 总数

| 类别 | 数量 |
|---|---|
| Active 技能 | 3 |
| Support | 7 |
| 词缀 | 13 |
| 天赋节点 | 16（含 2 Notable + 1 机制烬心） |
| 图词缀 | 3 |
| 怪 | 5（3 普通 + Elite 监守 + 木桩） |
| Modifier 引用（Support+Passive） | 26 |

内容数量护栏（Collect 阶段核入「结构/契约问题」，baseline 冻结）：Support=7 / 词缀=13 / StatId=28 / ModOp、Tag、Effect、Event、Condition、Skill、图词缀轴全部 +0。

## 资源契约（真实加载验证，非声明文字）

REQUIRED 缺失=审计失败；GATED 缺失=如实记录不失败（导演门控）；「无声明引用」与「有引用但缺资源」是两种状态——VFX 当前 Declared references = 0（Runtime 无任何 VFX 资源路径，Reviewer 已独立扫描复核）→ Result: N/A，与资源缺失不同。

| Logical | Resource Key | Type | Class | Asset Path | Result |
|---|---|---|---|---|---|
| 玩家模型（DarkKnight 预制体） | Player/DarkKnight | GameObject | Required | Assets/Resources/Player/DarkKnight.prefab | PASS |
| 战斗 SFX Cast | Audio/Cast | AudioClip | Required | Assets/Resources/Audio/Cast.ogg | PASS |
| 战斗 SFX Impact | Audio/Impact | AudioClip | Required | Assets/Resources/Audio/Impact.ogg | PASS |
| 战斗 SFX Hit | Audio/Hit | AudioClip | Required | Assets/Resources/Audio/Hit.ogg | PASS |
| 战斗 SFX Death | Audio/Death | AudioClip | Required | Assets/Resources/Audio/Death.ogg | PASS |
| 战斗 SFX Loot | Audio/Loot | AudioClip | Required | Assets/Resources/Audio/Loot.ogg | PASS |
| 敌人视觉预制体（TrollWarriorVisual，Brute 默认视觉） | Enemies/TrollWarriorVisual | GameObject | Required | Assets/Resources/Enemies/TrollWarriorVisual.prefab | PASS |
| 敌人视觉预制体（FireLionVisual，Stinger 默认视觉） | Enemies/FireLionVisual | GameObject | Required | Assets/Resources/Enemies/FireLionVisual.prefab | PASS |
| 敌人视觉预制体（BruceVisual，Warden 默认视觉） | Enemies/BruceVisual | GameObject | Required | Assets/Resources/Enemies/BruceVisual.prefab | PASS |
| 敌人视觉预制体（GargoyleVisual，Ashling 默认视觉） | Enemies/GargoyleVisual | GameObject | Required | Assets/Resources/Enemies/GargoyleVisual.prefab | PASS |
| 人声 Cast | Audio/Voice/Cast | AudioClip | Gated | — | GATED-MISSING |
| 人声 Hit | Audio/Voice/Hit | AudioClip | Gated | — | GATED-MISSING |
| 人声 Death | Audio/Voice/Death | AudioClip | Gated | — | GATED-MISSING |

REQUIRED 通过 10/10；GATED present 0 / missing 3（缺失不失败）。

## Skill Tag Profiles（golden parity + 当前形态规则）

| Skill | Runtime Tags | Golden | Result |
|---|---|---|---|
| Melee | Attack, Melee, Hit, Physical | Attack, Melee, Hit, Physical | ✓ |
| Projectile | Attack, Projectile, Hit, Physical | Attack, Projectile, Hit, Physical | ✓ |
| Area | Spell, Area, Hit, Physical | Spell, Area, Hit, Physical | ✓ |

## Tagged Modifier Reachability（Rule E：死 Tagged Modifier = 0）

| Owner | RequiredTags | 可满足 Skill | 结果 |
|---|---|---|---|
| Support.集中 | Area | 范围 | ✓ |
| Support.集中 | Area | 范围 | ✓ |
| Support.火焰转化 | Attack, Hit, Physical | 近战/弹道 | ✓ |
| Passive.残暴打击 | Melee | 近战 | ✓ |

Tagged Modifier 总数 4，可满足 4，不可满足 0（期望 0）。负向测试 `ContentAuditTagRules_NegativeCases_AreRejected` 证明规则能抓坏合成输入。

## 缺失 / 非法

| 项 | 条数 | 明细 |
|---|---|---|
| 结构/契约问题（目录条目 Name / 数量护栏 / FireConversion 契约 / 图连通 / 技能与怪参数 / MapAffix） | 0 | — |
| StatId/ModOp 引用缺失或运行期未知 | 0 | — |
| 非法 Tag | 0 | — |
| Effect/Event 引用非法 | 0 | — |
| 天赋链接缺失/单向 | 0 | — |
| 词缀行非法（含运行期未知 Stat） | 0 | — |
| Support×技能兼容矩阵违规 | 0 | — |
| Skill Tag 规则/golden parity 违规 | 0 | — |
| 死 Tagged Modifier | 0 | — |
| REQUIRED 资源缺失 | 0 | — |

## Support × 技能兼容矩阵

判定依据：①带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足；②机制路径（SupportDef.MechanicSkill）。**运行时已接入同一契约**：TrySetSupport 写入前调用 SliceSession.IsSupportCompatible 拒绝非法连接；golden 矩阵保持独立 oracle（SupportCompatGolden），Runtime parity 由 SupportGateTests 单独校验。

| Support | Q 近战 | W 弹道 | E 范围 |
|---|---|---|---|
| 燃烧 | ✓ | ✓ | ✓ |
| 残暴 | ✓ | ✓ | ✓ |
| 集中 | ✗ | ✗ | ✓ |
| 迅捷 | ✓ | ✓ | ✓ |
| 燃尽 | ✓ | ✓ | ✓ |
| 分裂 | ✗ | ✓ | ✗ |
| 火焰转化 | ✓ | ✓ | ✗ |

已知不兼容（钉死，不得放宽）：集中×近战、集中×弹道（Tag.Area 仅范围技能满足）；分裂×近战、分裂×范围（ForkProjectiles 只进弹道结算）；火焰转化×范围（RequiredTags=Attack|Hit|Physical，范围=Spell 无 Attack）。

## S3 R2 新增 Support（工作令 S3-R2-FIRE-CONVERSION）

机制型转换 Support：无新 Effect/Trigger/Stat/ModOp/Tag，无 Support 专用 Runtime 分支（Reviewer 零分支审计见 `S3_R2_REVIEW.md`）。

| ID | 名称 | Modifier | RequiredTags | ChangesMechanism | MechanicSkill | Trigger |
|---|---|---|---|---|---|---|
| FireConversion | 火焰转化 | ConvertPhysToFire 固定 0.5 | Attack, Hit, Physical | true | 无（Tag 路径推导） | 无 |

## S3 第一批新增词缀

全部由**已有 StatId/ModOp** 组成。可出现在 4 槽（武器/胸甲/头盔/靴子）掉落池；进两步 Craft（随机制作=废料池重掷、定向制作=蚀刻剂写入列表）。第二行独立掷值，存 `ItemInstance` 第二值。

| ID | 名称 | 行 1（Stat/Op） | 行 2（Stat/Op） | 槽位 | 两步 Craft |
|---|---|---|---|---|---|
| IgniteFire | 灼燃 | FireDamage 提高（{0:0%} 火焰伤害） | IgniteChance 固定（点燃几率 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |
| AccCrit | 锐击 | Accuracy 固定（+{0:0} 命中） | CritChanceIncreased 提高（暴击率 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |
| PhysFire | 熔铸 | PhysicalDamage 提高（{0:0%} 物理伤害） | FireDamage 提高（火焰伤害 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |

## 未使用 Tag（已声明、当前内容未引用）

**预留 Tag 不是失败项**：以下 Tag 为已声明预留，当前切片未引用；后续批次未立令前不得当作「缺实现」去补系统或补技能。

- Spell
- Projectile
- Fire
- Duration

