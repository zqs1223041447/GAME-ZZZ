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

- Attack
- Spell
- Projectile
- Hit
- Physical
- Fire
- Duration

## 音频（已知债，不阻断 S3 最小门）

全部事件（Cast, Impact, Hit, Death, Loot）仅有 `AudioEvents.Play` 日志接线，无音频资产。事件名固定 Cast/Impact/Hit/Death/Loot，接入时按名补资产即可。

## 预制体 / VFX 引用

当前 S2 内容全部为代码表 + 视图层代码（胶囊/换模模型/IMGUI），无预制体 / VFX / 音频路径引用——本类 0 条。

