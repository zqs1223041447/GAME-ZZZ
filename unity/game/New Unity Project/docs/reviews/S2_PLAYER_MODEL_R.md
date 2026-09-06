# R 审阅包：S2 玩家模型 + 碰撞（换模门）

日期：2026-09-07　|　撰写：I 席（Grok 会话）　|　依据：DECISIONS.md / RUNTIME.md / 交接第 7、9 节

---

## 0. 封面

- 门：S2 玩家模型 + 碰撞
- 申请：**放行 / 打回**（R 只按第 7 节格式回复）
- 偏差：导演提供 Dark Knight 成品包（BDO 黑暗骑士），取代自制 Eve 路线；Eve 自制路线维持剔除
- 未改：输入、QWE、结算、掉落；程序集仍 `Game.Runtime.Core` + `Game.Runtime.Content`
- I 自测：换模完成、碰撞按 RUNTIME.md 落地、PlayerMotor 零距离已护住（复核方法见第 2、3 节）
- 请 R 只审下方阻断项；文案债不作为本门阻断（见第 6 节）

---

## 1. 资源与接入（全部为工程内路径）

| 项 | 值 |
|---|---|
| 源模型 | `unity/game/测试资源，确认后进入项目/Dark Knight/Dark Knight.FBX`（176 骨 BDO 3ds Max Biped；FBX 内含 armor/weapon 分件，仅用合并版） |
| 源模型排除项 | `.max`（不可用）、`.blend`（RUNTIME.md 禁止运行时加载，未入工程） |
| 贴图 | 源包 56 张 DDS 批量转 PNG（含 opacity 透明贴图：用 opacity 蒙版烘出 `cloak_t.png`/`lower_t.png` 的 alpha 通道）；输出 `Assets/Art/Player/DarkKnight/Textures/`（58 PNG = 56 转换 + 2 烘焙） |
| 材质 | 22 个 URP/Lit 材质，`Assets/Art/Player/DarkKnight/Materials/`；3 个透明材质（cloak_opacity、lower、sho_lower_opacity） |
| Avatar | `DarkKnightAvatar`，Humanoid 自动映射成功（isHuman=True, isValid=True）；未映射骨为武器挂点/相机/表情骨，走 Biped 原生骨骼跟随 |
| 预制体 | `Assets/Resources/Player/DarkKnight.prefab`（Animator + 控制器 + avatar，applyRootMotion=false） |
| 挂载 | `Assets/Runtime/Core/Gameplay/EveView.cs` → `TryMount`：优先加载 `Resources/Player/DarkKnight`，无预制体回退 `Player/Eve`，再无则胶囊视图 |
| 动画 | 从 401 个动画 FBX 中抽 4 条入工程：Idle=`pdw_00_00_stand_idle_a_00_0001`（15.07s 循环）、Run=`pdw_01_01_move_run_f_00`（0.90s 循环）、Attack=`pdw_05_01_att_skill_comboatt_01`、Cast=`pdw_05_01_att_normal_magic_r_00`；目录 `Assets/Art/Player/DarkKnight/Animations/`；控制器 `Assets/Art/Player/DarkKnight/DarkKnight.controller`（状态名 Idle/Run/Attack/Cast）；全部设 Humanoid 重定向 |
| 动画驱动 | `ArenaDirector.DriveEve` 仅按视图 AnimState 调 `animator.Play(...)`，不进逻辑状态机 |
| 语音 | 源包 `sound/`（voice_1 200 + voice_2 201 + fx 208 个 ogg）**未入工程**；S1/S2 音频仍只有 Cast/Impact/Hit/Death 四事件 |
| Eve 剔除证明 | `EveView` 无 `.blend` 加载路径（仅 `Resources.Load<GameObject>` 预制体）；`Assets/` 内无 .blend/.max 文件（`Assets/Art/` 仅上列 DarkKnight 目录） |

### 自测核对方法（R 可复跑）

- 贴合：Play 后 Editor.log 出现 `[Arena] Eve fitted height 1.16 from 4.755 scale x0.24`（原始世界高 4.755，均匀缩放 0.24 至 1.16，与胶囊碰撞体高 1.16 一致）与 `[Arena] DarkKnight mounted under Player.`
- 骨骼健康：Play 后对 11 个 SkinnedMeshRenderer 逐一检查 `bones[i] != null`、`rootBone != null`、`isVisible`——本次实测全部通过（11/11）。复跑命令（pipeline eval）：对挂载后的 `Player/DarkKnight` 遍历 `GetComponentsInChildren<SkinnedMeshRenderer>()` 输出 bonesOk。
- 四态重定向：编辑器 `AnimationMode.SampleAnimationClip` 逐姿势采样截图，见 `docs/reviews/images/01_idle.png`、`02_run.png`、`03_attack.png`、`04_cast.png`（蒙皮无拉伸、透明斗篷正确、刀贴图正常）；全景见 `05_arena_wide.png`。

---

## 2. 碰撞（本门核心，逐条对照 RUNTIME.md「碰撞（锁定）」）

每条格式：锁定句 → 实现位置 → 如何复验。

1. **玩家与怪无碰撞，互相穿过，不推开**
   - 实现：`Assets/Runtime/Core/Gameplay/CollisionLayers.cs` → `Apply()`：`Physics.IgnoreLayerCollision(player, monster, true)`（Player=6 / Monster=7，层名缺失时用 fallback 常量）。
   - 复验：Play 后 `Physics.GetIgnoreLayerCollision(6, 7)` 预期 `true`；进怪堆走一圈不挡路。
2. **怪与怪无物理挤开，只保留约 1 单位最小间隔**
   - 实现：`CombatTypes.cs:163` `DummyMinSeparation = 1f`；`ArenaSim.cs:77` `Dummies.Separate(CombatRules.DummyMinSeparation)` 在 `TickAi` 之后每 Tick 调用；`DummyCrowd.cs:342` `Separate(float minDist)` 水平拨开，dist==0 沿 +X 拨开防除零。
   - 复验：F2/F3 生成怪群静置数秒，观察无叠点、无刚体弹开。
3. **命中仍是距离 / 圈 / 弹道，未改成碰到胶囊才算打中**
   - 实现：近战锥形/弹道半径/范围圈结算统一入口 `CombatMath.ResolveHit`（`CombatMath.cs:181`），调用点 `ArenaSim.cs:257/349/390`；packet 只读，未改。
   - 复验：走进怪群中心按 Q/W/E，命中数正常（换模前后数值一致，Damage 规则未动）。
4. **地面 / 墙 / 边界仍站住**
   - 实现：玩家根 `CapsuleCollider`（`ArenaDirector.BuildPlayer`，height=1.16/radius=0.32）+ 地面 MeshCollider + 平面 Clamp 不变；层忽略只作用于 Player↔Monster、Monster↔Monster，未涉及 Ground 层。
   - 复验：走到 ±38 边界与墙，不穿地、不出界。
5. **无刚体互推**
   - 实现：玩家与怪均无 Rigidbody；弹道为运动学位移（对象池），无默认物理弹道。
   - 复验：Hierarchy 过滤 `Rigidbody`，场景内玩家/怪数量为 0。

**R 打回标准（任一即阻断）**：层忽略缺失、Separate 未进 Tick、命中改成靠身体相撞、地面穿模。

---

## 3. PlayerMotor

- **零距离防护**：`Assets/Runtime/Core/Gameplay/PlayerMotor.cs` `Tick()`：`dist < CombatRules.DistEpsilon(=1e-5)` 直接返回（`CombatTypes.cs:153`），`dx/dist` 仅在 dist≥epsilon 分支执行——不存在除零 NaN 朝向。
- 目标点重合 / 静止 / 贴墙滑动均先走该分支，不会产生 NaN。
- 朝向一致性：逻辑朝向 `YawDeg` 由 `CombatMathUtil.MoveTowardsAngleDeg` 驱动；`ArenaDirector` 每帧 `_playerView.SetPositionAndRotation(pos, Euler(0, YawDeg, 0))`，模型作为 Player 根的子物体跟随同一 Yaw——逻辑朝向与模型朝向一致，无单独模型转向逻辑。

---

## 4. 零改动声明

本轮全部变更 = 下列清单（其余为空）：

- **代码（仅 1 个文件、仅视图层）**：`Assets/Runtime/Core/Gameplay/EveView.cs`——新增 `ResourcesNameDarkKnight` 常量与 `TryMount` 优先加载逻辑、挂载物命名 `DarkKnight`。无其他 .cs 变更。
- **文档**：`docs/DECISIONS.md`（换模条款追加 Dark Knight 接入记录）、`docs/RUNTIME.md`（玩家视图行）。
- **资产（新增）**：`Assets/Art/Player/DarkKnight/**`（FBX、58 PNG、22 mat、4 动画 FBX、控制器）、`Assets/Resources/Player/DarkKnight.prefab`。

逐条：输入未重写；QWE 结算未改；掉落 / Craft / 天赋 / 图内锁未改；未拆 Combat 程序集；动画未进逻辑状态机；未接武器、饰品、换装、语音。命中 packet 未触碰（见第 2 节第 3 条）。

预制体运行时变更字段（挂载时由 EveView 施加，非预制体静态值）：`localScale 1→0.24`（贴合 1.16）、遮挡/阴影 ShadowCastingMode.Off、SMR updateWhenOffscreen=true、剔除子物体上 Rigidbody/CharacterController/Collider（模型包本身无这些组件，防御性清理）。

---

## 5. R 与导演同一套 5 步验收

1. 打开 `Assets/Scenes/Arena.unity`，Play。应看见 **Dark Knight**，不是胶囊。（回退胶囊 = 本门未过。）
2. 走进怪堆能穿过；Q/W/E 仍能打中（身高 1.16 后近战锥 / 弹道 / 范围圈不得打空或打进地里）。
3. 怪堆静置数秒，不会永久叠成一个点。
4. 按住走、按住 QWE 仍可用（S1 手感曾打回，本门至少不能回退）。
5. 身高、朝向、Idle/Run/Attack/Cast 是否可接受（截图见 `docs/reviews/images/`）。

## 6. 已知非阻断（请勿据此打回）

- 语音未接（事件仍打 `[Audio]` 日志）。
- 仅四条动画，无 Hit / Death / Turn 动画（死亡仍是缩放占位表现）。
- 代码名仍叫 `EveView / DriveEve`，资源是 Dark Knight（历史命名，未重命名以免扩大 diff）。
- S1 手感未单独关门（本门不替它关）。
- Q/E 命中仍可能扫描 `CollectSkillMods`（交接已定 S2 体量非事故）。
- 旧 `-nographics` 采样不能当 GPU 成绩（S2P 门处理）。
- 文案四项（Cleared-F6 面板、Rare Rare、HUD Q Q、Combustion 截断）已在 S2 UI 返工中修复，不列入本门阻断。
- aiguillette（肩饰绳）无专属贴图，暂借用 sho 漫反射（视觉无异常，后续可换）。

## 7. 请 R 只按此格式回复

```text
结论：放行 / 打回
阻断项：（编号 + 现象 + 复现 + 违反的 RUNTIME/交接原句）
非阻断观察：（可选，I 不得当本轮范围）
```

打回把阻断项原样送回 I。R 不改代码，不开 S2P。

---
---

# 附：给导演的最短纸条（不写技术表）

1. 进 Arena 应看到**黑骑（Dark Knight）**，不是胶囊。
2. 走进怪堆应穿过，技能仍打中。
3. 怪不应粘成一个点。
4. 按住移动和 QWE 应与换模前一样能用。
5. 看身高和朝向是否别扭；不接受只指出要调的一项（缩放 / 相机 / 轴心 / 动画）。

回复「**模型过**」或「**调 X**」。
