# CONTENT_AUDIT_S3_BATCH1

生成：EditMode 测试 `ContentAuditS2Tests`（S3 第一批扩展，导演口令「开 S3」）。只覆盖现行切片：3 Active + 6 Support + 13 词缀（S2 基线 10 + 本批 3 组合系）+ 16 天赋 + 3 图词缀 + 5 怪。

## 总数

| 类别 | 数量 |
|---|---|
| Active 技能 | 3 |
| Support | 6 |
| 词缀 | 13（S2 基线 10 + S3 第一批 3） |
| 天赋节点 | 16（含 2 Notable + 1 机制烬心） |
| 图词缀 | 3 |
| 怪 | 5（3 普通 + Elite 监守 + 木桩） |
| Modifier 引用（Support+Passive） | 25 |

## 缺失 / 非法

| 项 | 条数 | 明细 |
|---|---|---|
| StatId/ModOp 引用缺失或运行期未知 | 0 | — |
| 非法 Tag | 0 | — |
| Effect/Event 引用非法 | 0 | — |
| 天赋链接缺失/单向 | 0 | — |
| 词缀行非法（含运行期未知 Stat） | 0 | — |
| Support×技能兼容矩阵违规 | 0 | — |

## Support × 技能兼容矩阵（本批新增校验）

判定依据：①带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足（StatBag 对不满足是静默跳过=隐形无效）；②机制路径（分裂）只接入弹道结算。**运行时已接入同一契约**：TrySetSupport 写入前调用 SliceSession.IsSupportCompatible 拒绝非法连接；golden 矩阵保持独立 oracle（SupportCompatGolden），Runtime parity 由 SupportGateTests 单独校验。

| Support | Q 近战 | W 弹道 | E 范围 |
|---|---|---|---|
| 燃烧 | ✓ | ✓ | ✓ |
| 残暴 | ✓ | ✓ | ✓ |
| 集中 | ✗ | ✗ | ✓ |
| 迅捷 | ✓ | ✓ | ✓ |
| 燃尽 | ✓ | ✓ | ✓ |
| 分裂 | ✗ | ✓ | ✗ |

已知不兼容（钉死，不得放宽）：集中×近战、集中×弹道（Tag.Area 仅范围技能满足）；分裂×近战、分裂×范围（ForkProjectiles 只进弹道结算）。

## 本批新增词缀（S3 第一批）

全部由**已有 StatId/ModOp** 组成（新增 Stat/ModOp/Tag/Effect/Event = 0）。可出现在 4 槽（武器/胸甲/头盔/靴子）掉落池；进两步 Craft（随机制作=废料池重掷、定向制作=蚀刻剂写入列表）。第二行独立掷值，存 `ItemInstance` 第二值。

| ID | 名称 | 行 1（Stat/Op） | 行 2（Stat/Op） | 槽位 | 两步 Craft |
|---|---|---|---|---|---|
| IgniteFire | 灼燃 | FireDamage 提高（{0:0%} 火焰伤害） | IgniteChance 固定（点燃几率 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |
| AccCrit | 锐击 | Accuracy 固定（+{0:0} 命中） | CritChanceIncreased 提高（暴击率 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |
| PhysFire | 熔铸 | PhysicalDamage 提高（{0:0%} 物理伤害） | FireDamage 提高（火焰伤害 {0:0%}） | 4 槽全部 | 随机池 + 定向列表 |

## 未使用 Tag（已声明、当前内容未引用）

**预留 Tag 不是失败项**：以下 Tag 为已声明预留，当前切片未引用；S3 后续批次未立令前不得当作「缺实现」去补系统或补技能。

- Attack
- Spell
- Projectile
- Hit
- Physical
- Fire
- Duration

## 音频（已知债，不阻断）

全部事件（Cast, Impact, Hit, Death, Loot）已挂钩 `AudioEvents.Play`（单一入口，无中间件）。查找表 `Resources/Audio/<事件名>` 5 键已投放 CC0 clip；语音 ogg 走 `VoiceCues` 待导演指认，与本审计无关。

## 预制体 / VFX 引用

当前切片内容全部为代码表 + 视图层代码（胶囊/换模模型/IMGUI），无预制体 / VFX / 音频路径引用——本类 0 条。

