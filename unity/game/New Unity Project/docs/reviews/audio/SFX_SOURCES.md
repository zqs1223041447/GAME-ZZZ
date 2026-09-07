# SFX_SOURCES（5 键战斗 clip 来源与许可）

日期：2026-09-07。**全部 CC0**（Creative Commons Public Domain Zero 1.0），可商用、免署名（自愿署名见下）。

来源包：**80 CC0 RPG SFX**（作者 rubberduck，OpenGameArt）
- 页面：https://opengameart.org/content/80-cc0-rpg-sfx （页面 License 字段：CC0）
- 包直链：https://opengameart.org/sites/default/files/80-CC0-RPG-SFX_0.zip
- 许可文本：https://creativecommons.org/publicdomain/zero/1.0/legalcode

| AudioEvents 键 | 源文件 | Resources 路径 | 时长 | 许可 |
|---|---|---|---|---|
| Cast | spell_fire_04.ogg | Assets/Resources/Audio/Cast.ogg | 1.59s | CC0（rubberduck） |
| Impact | blade_02.ogg | Assets/Resources/Audio/Impact.ogg | 0.31s | CC0（rubberduck） |
| Hit | creature_hurt_01.ogg | Assets/Resources/Audio/Hit.ogg | 0.62s | CC0（rubberduck） |
| Death | creature_die_01.ogg | Assets/Resources/Audio/Death.ogg | 1.06s | CC0（rubberduck） |
| Loot | item_coins_01.ogg | Assets/Resources/Audio/Loot.ogg | 0.41s | CC0（rubberduck） |

选型说明：黑骑士火系技能→Cast 取 fire 系；Impact 取刀剑命中；Hit 取受伤呼痛；Death 取死亡哀声；Loot 取金币。未用 Kenney（OGA 单包已覆盖五键，单一来源许可更干净）。

包内 fx 208 条（DK_VOICE_INVENTORY.md）保留作后续备选池（数值 ID 无语义名，换装/替换时由导演试听指认）。

实机验证（Play，2026-09-07）：五键 PlayOneShot 逐一触发，AudioCue 音源 isPlaying=True 全部实证；VoiceCues 表空=无 VoiceCue 实例（人声零回归）。
