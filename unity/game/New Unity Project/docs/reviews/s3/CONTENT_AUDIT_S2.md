# CONTENT_AUDIT_S2

生成：EditMode 测试 `ContentAuditS2Tests`（S3 最小项）。只覆盖 S2 切片：3 Active + 6 Support + 10 词缀 + 16 天赋 + 3 图词缀 + 5 怪。

## 总数

| 类别 | 数量 |
|---|---|
| Active 技能 | 3 |
| Support | 6 |
| 词缀 | 10 |
| 天赋节点 | 16（含 2 Notable + 1 机制烬心） |
| 图词缀 | 3 |
| 怪 | 5（3 普通 + Elite 监守 + 木桩） |
| Modifier 引用 | 25 |

## 缺失 / 非法

| 项 | 条数 | 明细 |
|---|---|---|
| Content ID / StatId 引用缺失 | 0 | — |
| 非法 Tag | 0 | — |
| Effect/Event 引用非法 | 0 | — |
| 天赋链接缺失/单向 | 0 | — |

## 未使用 Tag（已声明、当前内容未引用）

**预留 Tag 不是失败项**：以下 Tag 为已声明预留，当前 S2 切片未引用；S3 内容扩张开启前不得当作「缺实现」去补系统或补技能。

- Attack
- Spell
- Projectile
- Hit
- Physical
- Fire
- Duration

## 音频（已知债，不阻断 S3 最小门）

全部事件（Cast, Impact, Hit, Death, Loot）已挂钩 `AudioEvents.Play`（单一入口，无中间件）：Cast=施放起手、Impact=技能命中、Hit=玩家受击上跳沿、Death=玩家进入 Dead、Loot=掉落生成（SliceSession.DropGear）。查找表 `Resources/Audio/<事件名>` 已预留 5 键，当前无音频资产=静音+限频日志（5s/键）。接入时按名投放资产即可，无需改代码。

## 预制体 / VFX 引用

当前 S2 内容全部为代码表 + 视图层代码（胶囊/换模模型/IMGUI），无预制体 / VFX / 音频路径引用——本类 0 条。

