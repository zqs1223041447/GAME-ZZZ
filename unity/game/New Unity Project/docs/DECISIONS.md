# DECISIONS

S0 锁定。后续阶段不得在未改本文的情况下推翻这些决定。

## S2P 技术收口（2026-09-07）

- S1 手感门：导演复测通过，**正式关闭**（2026-09-07）。
- S2 换模 + 碰撞：**技术放行**（R 审 f22862e；实现 239142e）；贴地修正 ebbf782；外观「小于敌人/看不清」项已关闭。「模型过」仍是审美终审语，不因技术放行而自称。
- Editor 内嵌基线（6e95c1b）成立，仅作对照，**非 120FPS 证据**。
- 独立包 1080p 全屏基线（b152610）成立：100/200/300 主线程 p99 最高 2.508ms，GPU≈0.33ms，均低于 8.33ms；300 档末帧存活 294/300 接受为接近额定（补怪即时生效）。
- 1440p 被本机 1080p 显示器钳制（全屏/窗口两种方式均被钳回），**待有 1440p 硬件再补测**；不宣布 120FPS@1440p。
- 测量工具 `ArenaPerfHarness`：默认关；仅独立包 `-arenaPerf` 命令行或显式调用启动；**正式游玩路径不得自动开启**。
- 非阻断残留：Eve 回退分支已删除（2026-09-07）——`TryMount` 只加载 `Player/DarkKnight`，缺失即回退胶囊并打日志；`EveView/DriveEve` 命名残留**已改名清零（2026-09-07 改名轮）**：`DarkKnightView`/`DriveDarkKnight`，历史 commit/叙述保留曾用名。
- S3 最小底座（2026-09-07）：Art Bible 初稿路径 `docs/art/ART_BIBLE.md`；内容校验挂 EditMode（`ContentAuditS2Tests`），**只覆盖 S2 切片**（3 Active + 6 Support + 10 词缀 + 16 天赋 + 3 图词缀 + 5 怪），报告 `docs/reviews/s3/CONTENT_AUDIT_S2.md`；音频缺失为已知债不阻断。不加新内容。
- 预留 Tag 钉死（2026-09-07）：Attack / Spell / Projectile / Hit / Physical / Fire / Duration 为**已声明预留**，当前 S2 切片未引用；**S3 内容扩张开启前不得当作「缺实现」去补系统或补技能**；「未使用 Tag ≠ 下一工作项」。
- 玩家视图 Hit/Death 动画（2026-09-07）：自已留档的 401 动画库取 2 条——`pdw_01_01_def_shield_dam_00`（受击后仰，0.63s，全段）与 `pdw_01_01_def_shield_break_00`（裁至跪倒段 0.68s，掐掉恢复站立）；控制器加 Hit/Death 两状态（与 Idle/Run/Attack/Cast 同层）。`DriveDarkKnight` 只挂现有信号：`Session.HitFlash` 上跳沿=Hit（下一击重播即可打断），`MapState.Dead`=Death（播完 `animator.speed=0` 定格跪倒末帧，复活出图恢复）。逻辑状态机未扩、不接音频、命中/死亡公式未动。截图 `docs/reviews/images/07_hit.png`、`08_death.png`。
- 相机跟拍收一帧（2026-09-07）：跟拍按贴地后身高 ≈1.19 重取景——偏移 `(0,17,-15)`→`(0,10.6,-9.3)`（注视点距离 22.7→14.1，俯角不变 ≈48.6°），注视高度 `CamLookY 0.5→0.6`（胸口/身体包围盒中心近似，不锁武器尖），FOV 42 不变。人物约占画面高 1/9（改前约 1/24），全身可辨+近怪在框（截图 `docs/reviews/images/09_camera.png`）。只动常量，无平滑、无新镜头系统；相机恒在地面 y=0 上方 11.2，不穿地；死亡冻帧/进出图无新增摆动源。
- 音频事件挂钩·无素材（2026-09-07）：单一入口 `AudioEvents.Play`（不进 FMOD/Wwise，不生产 wav/ogg）；查找表 `Resources/Audio/<事件名>` 预留 5 键（值可空），无资产=静音+限频日志（5s/键，禁每帧刷屏）。触发点：Cast=`ArenaSim.Resolve` 施放起手；Impact=`ArenaSim.PlayImpact` 技能命中；Hit=玩家受击 HitFlash 上跳沿（`ArenaDirector`，与 Hit 动画同帧；原怪物受击挂点已摘除）；Death=玩家进入 `MapState.Dead`（`ArenaDirector`，与 Death 动画同处；原怪物死亡挂点已摘除）；Loot=**掉落生成**（`SliceSession.DropGear`，AddItem 成功即触发——选生成非拾取，当前无独立拾取动作）。语音 ogg 仍不接；正式游玩路径缺音频不报错不卡死。
- 导演门控待输入表（2026-09-07）：ROADMAP 增「导演门控待输入」节（ART_BIBLE 临时色 / 1440p 补测 / 正式 UI / 语音 ogg / EveView 改名 / 怪物独立 EnemyHit 事件 / 音频 clip 投资源）——每项「无输入则不做」，不得当自动任务开工。
- 门状态总览（2026-09-07）：`docs/reviews/STATUS.md` 一页纸——11 门状态+玩家视图/碰撞/测量/音频/校验快照；总览见 STATUS.md。
- 导演决策集·八项（2026-09-07 导演就位逐项裁定，同日细化）：①三色**仅占位**——现阶段颜色随意，后续按技能敲定配色并配贴图+动效（归未来皮肤/技能表现轮）；②1440p 补测**保留后续测试**（维持挂起、不宣布 120@1440p）；③**重做 UI，参考 PoE/暗黑 3，可使用网络免费资源**（先出方案页给导演过目再实现）；④语音 ogg **全部事件接入并设默认冷却**；⑤**批准** EveView/DriveEve 改名轮——**已执行（2026-09-07 改名轮：`DarkKnightView`/`DriveDarkKnight`）**；⑥怪物独立 EnemyHit 事件**维持现状**（用 Impact 语义，不新增）；⑦5 个音频 clip——**网上搜免费资源**，搜不到则等后续补充；⑧**其余问题全部完成后开启 S3**，S3 内容问 GPT 要规划。门控表（ROADMAP）已同步裁定状态。
- 语音 ogg 接入·第一轮（2026-09-07）：独立人声表 `VoiceCues`（**与 AudioEvents 5 键战斗 SFX 完全隔离**，查找 `Resources/Audio/Voice/<键>`）；3 键=Cast（施放喊招，`ArenaSim.Resolve`，冷却 1.5s）/ Hit（受击呼痛，`ArenaDirector` 上跳沿，冷却 0.6s）/ Death（进入 Dead，`ArenaDirector`，每次一次）。**映射为缺口**：包内 609 条 ogg 为数值 ID 无语义名（fx 208 / voice 401，清点见 `docs/reviews/audio/DK_VOICE_INVENTORY.md`），待导演试听 `Desktop/voice_listen/` 候选（20 条，按 CAST/受击/死亡 前缀）指认后投放 `Resources/Audio/Voice/` 即生效；未指认前=静音。不外购、不硬塞语义。
- 5 键战斗 clip 投放·CC0（2026-09-07）：五键全部接入——来源**单一包 80 CC0 RPG SFX**（rubberduck，OpenGameArt，页面 License=CC0）：Cast=spell_fire_04（黑骑士火系）/ Impact=blade_02 / Hit=creature_hurt_01 / Death=creature_die_01 / Loot=item_coins_01，落 `Assets/Resources/Audio/<键>.ogg`（AudioEvents 查找表零逻辑改动，投放即生效）。来源与许可全录 `docs/reviews/audio/SFX_SOURCES.md`。Play 实证：五键 PlayOneShot 逐一 isPlaying=True。包内 fx 208 条留作备选池。

## 工作方式（导演 2026-09-07 追加）

- 本项目全程无人监管，工作 AI 完全自动根据 GPT（协调席）的指示开展工作；同一 AI 轮流担任 I（实现）与 R（审核）。
- 遇到阻塞项：记录下来（写入当前报告/文档），立即转做其它可推进的工作；**不允许停手等待**。
- 每次向 GPT 汇报时，附带上述两条要求。
- S1 手感门：导演 2026-09-07 人工复测通过（按住走、按住连发、Q/E 施放+命中反馈），**手感门正式关闭**（并入上方 S2P 收口条目）。
- S2 玩家模型外观残留：导演复测确认「身高/缩放有问题——人物模型看不清、明显小于敌人模型」，列为待调项（视图层），调后无需重开全包。**已修正（2026-09-07）**：根因是挂载时一次性贴地补偿在动画播放后失准，身体下沉约 0.70 埋入地下（实际可见仅约 0.49）。修正为按身体包围盒（排除武器）每帧贴地（`DarkKnightView.UpdateGround` + `ArenaDirector` 视图更新调用）；修正后身体净高 1.19（普通怪视觉 0.58、精英监守 1.17），同框截图确认清晰可辨。待复核。

## 工程

- 单机 · Windows · Unity 6000.3 · URP（不改 Built-in / HDRP）。
- 开发根目录：`unity/game/New Unity Project/`。
- 业务程序集只允许：`Game.Runtime.Core`、`Game.Runtime.Content`。不创建 Combat / Skill / Items / Maps / AI / Presentation 空壳程序集。
- 测试程序集：`Game.Tests.EditMode`；S1 增加 `Game.Tests.PlayMode`（非业务）。
- 玩法与内容随机禁止 `UnityEngine.Random`。只用 `SeededRng`。
- 接口以 `docs/RUNTIME.md` 为准；代码与文档不一致则同一次改文档。

## S0 范围

做：目录、Asmdef、Bootstrap + 空 Arena、GameLog、SeededRng、顶层 JSON Import/校验、EditMode 测试。

不做：点地移动、技能、伤害、UI、300 怪、DOTS/Entities、Jobs/Burst、玩法内容。

S0 已放行。S0 模块（SeededRng、GameLog、ContentDatabase、ContentId、ContentRecord、ContentPaths、BootstrapRunner）不得重写。

## 内容数据

- 文本数据在工程根 `ContentData/`，不进 `Assets/`。
- Bootstrap 默认只加载 `ContentData/valid/`。
- `ContentData/invalid/` 只给测试和人工复现。
- Import 是全成或全败：任何错误则 `db` 空，禁止部分成功。

## S1 范围

做：点地移动、朝向、攻击距离、接近、同一套施放流程、取消窗口、输入缓冲、近战/弹道/范围各一、Dummy 100/200/300、简单转向、AI LOD、对象池命中/死亡反馈、占位动画、音频事件名、PerformanceArena 测量。

不做：装备、天赋、Craft、Unique、完整 Combat Math、地图词缀、Jobs/Burst/DOTS、Socket、锁 Build。

- S1 玩法进 `Game.Runtime.Core/Gameplay/`，不新增业务程序集。
- 命中 = 固定扣血。Dummy Hp=2。无护甲/抗性/暴击/转化。
- 弹道运动学 + 对象池，禁止默认 Rigidbody。
- Dummy AI 禁止「300 × 每帧完整 NavMeshAgent」作为方案。
- `COMBAT_MATH.md` 保持未做，禁止假写公式。

## S1 放行

S1 技术已放行。输入、对象池、SeededRng、点地移动、Q/W/E 施放流程不得重写。

## S2 范围

做：Tag/Stat/Modifier/Condition/Effect/Event/Trigger 内核；Combat Math 子集；Q/W/E 同一结算；6 Support + Socket/Link（无孔色）；4 装备槽 Ordinary/Rare ≥10 Affix；随机 Craft + 定向 Craft；16 节点天赋（2 Notable + 1 机制）；3 普通 + 1 Elite；1 图 3 词缀 + Stability + Reward + 进图快照锁；从 Arena 进图。

不做：技能库、大天赋树、Unique、完整 Craft 链、孔色、DOTS、S2P 120FPS、大主菜单、第四个业务程序集（Combat 未拆，公式在 Core/Gameplay）。

机制 Support 锁定为 **Fork**（W 弹道命中后分裂成 2）。天赋机制节点锁定为 **Cinder Heart**（40% 物理转火）。DoT 锁定为 Ignite。Conversion 锁定为 Physical → Fire。

图内不可改 Build。死亡出图免费 Respec。随机只走 SeededRng。内容是数据表（`Catalogs.cs`），不在运行时发明新机制。

## S2 UI

导演打回：补可评价界面，不改数值与结算。玩家可见描述一律中文。死亡后不得卡死。稳定度随死亡/清场下降。

**HUD 未完备**：角色背包、技能、装备、天赋 HUD 后补，当前不得当作已经做完。禁止新技能/词缀/天赋/Unique/Atlas/DOTS/S2P/精模。

## 镜头 / 体型（表现）

高速 ARPG 读图：相机拉远，人/怪视图缩小。不改速度、攻击距离、命中、分离。命中仍是距离 / 圈 / 弹道。数值见 `RUNTIME.md` 碰撞节后的相机行。

## 换模

自制 Eve FBX / 预制体 / Mixamo 导出已从工程剔除（蒙皮损坏、双骨架、单位错误，不可用）。玩家暂回 S1 胶囊。换模等导演提供可用成品后再接。不改按住移动、QWE、结算、掉落。不上 HDRP / DOTS。

2026-09-06 已接导演提供的测试成品：黑色沙漠 Dark Knight 包（`unity/game/测试资源，确认后进入项目`）。只取 `Dark Knight.FBX`（176 骨，BDO Biped）+ DDS 转 PNG 贴图 + 4 条动画 FBX。接入方式：Humanoid 重定向（`DarkKnightAvatar`），预制体 `Assets/Resources/Player/DarkKnight.prefab`，由 `EveView.TryMount` 优先加载（缺预制体回退胶囊视图）。动画只做视图层（Idle/Run/Attack/Cast 四状态控制器，`EveView.DriveEve` 驱动），不进逻辑状态机。401 个动画 FBX 中其余留档未导入。语音 ogg 留档后用（S1/S2 只有 Cast/Impact/Hit/Death 四事件）。

## 碰撞（锁定）

- 玩家与怪物无碰撞，互相穿过，不推开。
- 怪物与怪物无物理挤开；只保留约 1 单位最小间隔，防止叠点。
- 地面 / 墙 / 边界仍要站住（平面 `ClampPlane` + 地面 MeshCollider）。
- 命中继续用距离 / 圈 / 弹道。禁止改成碰到胶囊才算打中。
- 玩家与怪不要物理互推：分层忽略（Player / Monster）。
- 怪与怪禁止 Rigidbody 互撞、禁止 CharacterController 互推；在 `DummyCrowd.Separate`（AI 转向之后）距离 < 1 就水平拨开。
- 无碰撞不等于打不中。走进怪群按 QWE 必须仍能命中。
