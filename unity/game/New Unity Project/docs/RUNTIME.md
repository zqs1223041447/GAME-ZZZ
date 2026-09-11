# RUNTIME

模块边界与接口。禁止在代码里另起一套 RNG / 日志 / 内容库。S1 玩法接口以本节为准；代码与本文不一致则同一次改文档。

## 目录

```text
Assets/Runtime/Core/                 Game.Runtime.Core
  GameLog.cs / SeededRng.cs          S0，不得重写
  Gameplay/                          S1 玩法（同一程序集，不是新程序集）
Assets/Runtime/Content/              Game.Runtime.Content（可依赖 Core）
Assets/Tests/EditMode/               Game.Tests.EditMode
Assets/Tests/PlayMode/               Game.Tests.PlayMode（非业务程序集）
Assets/Scenes/Bootstrap.unity        S0，Play 后只做 Seed + Content 加载
Assets/Scenes/Arena.unity            S1 战斗 / 密度场景（PerformanceArena）
Assets/Settings/                     URP 管线资源（非玩法）
ContentData/valid/                   Bootstrap 默认加载
ContentData/invalid/                 非法样本，不自动加载
docs/
开发计划/
```

## 程序集

| 程序集 | 平台 | 依赖 | 职责 |
|---|---|---|---|
| Game.Runtime.Core | 运行时 | 无业务依赖 | S0：SeededRng、GameLog。S1：点地移动、施放流程、三技能、Dummy 群、对象池、Arena、测量。S2：Tag/Stat/Modifier/Trigger、Combat Math、Support/Socket、装备/Craft/天赋/地图锁（仍在 Core/Gameplay，未拆 Combat 程序集） |
| Game.Runtime.Content | 运行时 | Core | ContentId、ContentDatabase、BootstrapRunner |
| Game.Tests.EditMode | Editor | Core、Content、TestRunner | EditMode 测试 |
| Game.Tests.PlayMode | 运行时测试 | Core、TestRunner | PlayMode 测试与密度采样 |

业务程序集仍只有 Core + Content。S2 未拆 `Game.Runtime.Combat`（公式与结算在 `Gameplay/`）。禁止再拆空壳程序集。禁止 `Assembly-CSharp` 里放脚本。禁止 Entities / Netcode。禁止为冲帧数上 Full DOTS / Jobs / Burst。

## SeededRng（Core，S0）

```text
namespace Game.Runtime.Core
class SeededRng
    SeededRng(uint seed)
    uint NextUInt()
    float NextFloat01()   // [0, 1)
```

- 同一 seed 产生同一序列。
- `seed == 0` 时内部状态不得为 0（避免序列卡死在 0）。
- 玩法与内容随机必须走这里，禁止 `UnityEngine.Random`。
- 禁止第二套 RNG。Dummy 布局抖动用 `SeededRng`。

## GameLog（Core，S0）

```text
namespace Game.Runtime.Core
static class GameLog
    void Info(string tag, string message)
    void Warn(string tag, string message)
    void Error(string tag, string message)
```

输出到 Unity Console，格式：`[tag] message`。

S1 音频无资源时：`[Audio] Cast|Impact|Hit|Death`（**S1 historical baseline**）。性能采样：`[Perf] ...`。音频事件（2026-09-07 挂钩）：单一入口 `AudioEvents.Play`（无中间件），5 事件=Cast（施放起手 `ArenaSim.Resolve`）/ Impact（技能命中 `ArenaSim.PlayImpact`）/ Hit（玩家受击上跳沿 `ArenaDirector`，与 Hit 动画同帧）/ Death（玩家进入 `MapState.Dead` `ArenaDirector`，与 Death 动画同处）/ Loot（掉落生成 `SliceSession.DropGear`）；查找契约=`Resources/Audio/<事件名>`（缺资产=静音+限频日志 5s/键的**降级模式**，不报错不卡死；资产存在性以最新 Resource Audit 为当前快照）。**资源路径拼接单一真相源（S3-M2）**：`RuntimeResourcePaths`（PlayerDarkKnight / CombatSfx(key) / Voice(key)）——AudioEvents / VoiceCues / DarkKnightView 均经它取路径；审计期望键集与 REQUIRED/GATED 分类仍为独立 oracle（declared-key parity 测试防未审批增删）。**战斗 SFX 当前已投放（2026-09-07，全 CC0，REQUIRED 资源契约 6/6 PASS）**：Cast/Impact/Hit/Death/Loot 五键各 1 条（来源 80 CC0 RPG SFX / rubberduck / OGA；映射与许可见 `docs/reviews/audio/SFX_SOURCES.md`）；**人声**：独立表 `VoiceCues`（`Resources/Audio/Voice/<键>`，与 5 键 SFX 隔离），Cast=施放喊招（冷却 1.5s）/ Hit=受击呼痛（0.6s）/ Death=倒下（每次 Dead 一次）；清点 609 条 ogg 见 `docs/reviews/audio/DK_VOICE_INVENTORY.md`，**映射仍 GATED**（待导演试听指认，未指认=静音；当前 0/3 present）。

## ContentId + ContentDatabase（Content，S0）

```text
namespace Game.Runtime.Content

readonly struct ContentId
    string Value
    static bool TryParse(string raw, out ContentId id)

sealed class ContentRecord
    ContentId Id
    string DisplayName

sealed class ContentDatabase
    int Count
    bool TryGet(ContentId id, out ContentRecord record)
    static bool TryLoadAll(string directory, out ContentDatabase db, out List<string> errors)

static class ContentPaths
    string ProjectRoot
    string ValidDirectory      // <project>/ContentData/valid
    string InvalidDirectory    // <project>/ContentData/invalid
```

### ID

建议 `domain.kebab-name`，例如 `monster.dummy`。

`TryParse` 失败：空、或缺 `.`、或 `.` 在首尾。

### TryLoadAll

- 只扫该目录顶层 `*.json`，不递归。
- 最小字段：`id`、`displayName`。
- 下列任一情况：整次失败，`db` 为空，`errors` 含文件名（或目录）和原因：
  - 目录不存在
  - JSON 损坏
  - 缺 id
  - id 不含点（或无法 Parse）
  - 缺 displayName
  - 重复 id
- 禁止部分成功却返回 `true`。

### 样本

```text
ContentData/valid/dummy_ok.json
  { "id": "monster.dummy", "displayName": "Dummy" }

ContentData/invalid/dummy_bad.json
  { "displayName": "NoId" }
```

## 场景

- Bootstrap：Play 后用 seed `0xC0FFEE` 建 `SeededRng`，打 Info；加载 `ContentData/valid/`；成功 Info / 失败 Error。**不自动进 Arena**（不改 S0 行为）。
- Arena：S1 唯一可玩场景。Play 后 `ArenaDirector` 运行时搭地面 / 玩家 / Dummy 池 / 弹道池 / 反馈池。这就是 PerformanceArena。

进入 Arena：打开 `Assets/Scenes/Arena.unity`，按 Play。

---

## S1 玩法（Core / Gameplay）

逻辑在 `ArenaSim`（可 EditMode 单测）。`ArenaDirector` 只做输入、视图、采样。Dummy / 弹道 / 命中反馈 **没有** 各自的 `Update()`。禁止每 Buff 一个永驻 GO（S1 无 Buff）。禁止每次命中扫装备树（S1 无装备）。

### 命令

```text
PlayerCommand
    None | Move | Cast | Stop
    SkillId Skill          // Cast 时：Melee / Projectile / Area
    int TargetDummy        // -1 = 无目标
    float AimX, AimZ       // 地面点或目标当前位置
```

Stop：清目的地 / 接近，立刻停步。单击不发 Stop；按住移动松开才发。

### 输入绑定（ArenaDirector）

| 输入 | 命令 |
|---|---|
| 左键单击地面 | Move 到该点；松手后继续走到点 |
| 按住左键地面（≥ 0.04s） | 每帧 Move 到光标在 y=0 的投影；松开发 Stop |
| 左键单击 Dummy（点附近 0.85） | Cast Melee，目标该 Dummy（先接近） |
| 按住左键 Dummy（≥ 0.04s） | 每帧 Cast Melee（接近并按近战规则打）；松开发 Stop |
| Q 点按 | Cast Melee 一次 |
| 按住 Q（≥ 0.08s） | 恢复结束立即再施放（见下） |
| W 点按 / 按住 | 同上，Cast Projectile |
| E 点按 / 按住 | 同上，Cast Area |
| F1 / F2 / F3 | 生成 100 / 200 / 300 Dummy |
| F5 | 对当前 Dummy 数做一帧成本采样并写日志 |

主手感是按住走，不是连点。Q/W/E 优先于左键。

Q/W/E：若鼠标下有 Dummy 则以其为 Target；否则对地面瞄准点施放。近战无目标时原地朝向出招。

**按住 = 恢复结束立即再施放。** 点按一次只放一次。按住时输入层在阈值后每帧再提交同一 Cast，走现有施放流程 / 取消窗口 / 单槽缓冲；禁止另写一套自动放技能。按下当帧（或下一帧）必须进入 Windup 或进入缓冲。

点选不走物理 Raycast 扫 300 Collider：相机射线打 y=0 平面，再在 `DummyCrowd` 里找最近存活者。

### 点地移动 / 朝向 / 接近

- 玩家在 XZ 平面移动，速度 `6.5`，角速度 `720` 度/秒。地面范围 `±38`。
- Move：走向 Aim，到达半径 `0.18` 停。移动中朝向速度方向。
- 与目的地距离 `< 1e-5`：视为到达，**不得**除距离，不得把朝向写成 NaN。
- Cast 时若与目标（Dummy 或地面瞄准点）距离 **大于** 该技能 Range：进入接近，`StopDistance = Range * 0.92`，追上后停步、锁朝向、进入 Windup。
- 接近中 Dummy 移动则每帧更新目的地。目标死亡则清接近。
- Windup / Active 期间不能移动。Recovery 见取消窗口。
- **不**用 NavMeshAgent 做 300 Dummy 寻路，也 **不**用玩家 NavMesh 作为 S1 方案。平面直走。

### 施放流程（三技能同一套）

阶段：`Idle → Windup → Active → Recovery → Idle`。

进入 Active **当帧** 结算一次（弹道为生成一发，飞行在后续 Tick）。

S1 恢复与前摇宁可偏短，第一下必须像自己按出去的。

```text
近战 Melee
    Range 2.4  Windup 0.06  Active 0.08  Recovery 0.10  Cooldown 0.04
    Damage 1
    结算：对 Range 内、相对锁朝向夹角 ≤ 60° 的所有存活 Dummy 各命中一次

弹道 Projectile
    Range 12  Windup 0.08  Active 0.06  Recovery 0.12  Cooldown 0.10
    Damage 1  Speed 18  Radius 0.35  MaxDistance 16
    结算：从对象池取一发，沿锁朝向运动学位移
    禁止 Rigidbody / 默认物理弹道
    与 Dummy 中心距离 < Radius+DummyRadius 则命中并回收；超 MaxDistance 回收

范围 Area
    Range 7  Windup 0.08  Active 0.10  Recovery 0.14  Cooldown 0.14
    Damage 2  Radius 3.2
    结算：以锁定 Aim 为圆心，**一次** 遍历，半径内每个存活 Dummy 命中一次
    禁止每目标一次独立施放流程
```

冷却从 Windup 开始扣。冷却中的技能键 **不入缓冲、不施放**。

### 取消窗口与输入缓冲（代码必须与此一致）

```text
不可取消：Windup、Active。本段必须打完。
取消窗口：Recovery。此阶段点地 / 点敌人 / Q/W/E 立即打断剩余 Recovery 并执行新指令。

输入缓冲：
- 单槽。后到覆盖先到。
- 仅 Windup / Active 期间：合法指令写入缓冲；此期间 Age 冻结（不计超时）。
- 进入 Recovery 或 Idle 的当帧消费缓冲（等于用取消窗口接上缓冲指令）。
- 已处于 Recovery / Idle 时若仍有残留缓冲：Age 累加，超过 0.20s 丢弃。
- 冷却中的技能不入槽。
- Move、Stop、带目标的 Cast、对地 Cast 均可入槽。
```

按住技能时：冷却一结束，后续帧的 Cast 在 Windup/Active 入缓冲，或在 Recovery 取消窗口直接再施放。这就是「按住 = 恢复结束立即再施放」。

### 命中（最小规则，不是 Combat Math）

```text
Dummy 最大生命 = 2
命中：Hp -= 技能固定 Damage
Hp <= 0 → Death，播放死亡占位后 0.40s 回收进对象池
```

禁止：护甲、抗性、暴击、转化、装备扫描、天赋。`docs/COMBAT_MATH.md` 保持未做，不得在此填假公式。

### Dummy 群与 AI LOD

```text
对象池容量 300。F1/F2/F3 清场后按固定 seed 0xC0FFEE 在玩家周围成环生成 100/200/300。
同 seed 同数量 → 同一布局。
简单 AI：近/中距离转向玩家并靠近，停在 1.1 以外（不叠进玩家）。
Dummy 不打玩家。玩家在 Arena 内不扣血。

LOD（禁止 300 只每帧完整 NavMeshAgent）：
- 距玩家 < 16：每帧转向 + 位移
- 16～32：每 3 帧 Tick 一次，用 dt*3 补偿速度，仍转向
- > 32：每 10 帧 Tick 一次，停转向、停靠近
```

Dummy **无** `NavMeshAgent`、**无** 每实例 `Update`、**无** Rigidbody。

### 碰撞（锁定）

```text
Player 层 = 6（Player）
Monster 层 = 7（Monster）
IgnoreLayerCollision(Player, Monster) = true
IgnoreLayerCollision(Monster, Monster) = true
```

- 玩家视图：`Resources/Player/DarkKnight`（Dark Knight 换模，视图层动画 Idle/Run/Attack/Cast/Hit/Death），无预制体时**回退 S1 胶囊**（旧 Eve 回退分支已删除；自制 Eve FBX 已剔除）。模型按**身体包围盒**每帧贴地（`DarkKnightView.UpdateGround`，武器长刀不参与身高/贴地计算），身体净高 ≈1.19。根上留 CapsuleCollider（贴地/墙用，高 1.16），不挡怪。Hit/Death 为纯视图钩子：受击=`Session.HitFlash` 上跳沿（可被下一击打断重播），死亡=`MapState.Dead`（Death 播完冻结末帧），逻辑状态机未扩。
- Dummy 视图：无 Collider、无 Rigidbody、无 CharacterController。
- 命中仍是距离 / 锥 / 弹道半径，不是物理接触。
- 怪-怪：`DummyCrowd.Separate(DummyMinSeparation=1)`，在 `ArenaSim.Tick` 里 `TickAi` 之后调用。距离 < 1 水平各拨一半；`dist==0` 沿 +X 拨开，禁止除零。
- 玩家移动不检测 Dummy 位置，可以走进怪群。

跟随相机默认偏移 `(0, 17, -15)`→`(0, 10.6, -9.3)`（2026-09-07 按贴地后身高 ≈1.19 收一帧，注视距离 22.7→14.1，俯角不变），FOV 42，LookY `0.5`→`0.6`（胸口近似）。玩家视图胶囊约高 `1.16`、半径 `0.32`。Dummy 视图系数 `0.58`（相对旧 1×1 方块）。**不改** 速度 / 攻击距离 / DummyRadius / 分离。命中仍是距离 / 圈 / 弹道。Game 窗口的 Scale 滑条是像素放大，不是提高分辨率。

### 对象池

| 池 | 容量 | 回收 |
|---|---|---|
| Dummy | 300 | 死亡占位结束 SetActive(false)，槽位 Occupied=false |
| Projectile | 32 | 命中或超距 |
| Feedback | 48 | 寿命结束；满员覆盖最旧槽 |

命中与死亡反馈必须走 Feedback 池，禁止每次 `new` 或不回收的 `Instantiate`。

Q / E 必须有「施放」和「命中」两段可见反馈。死亡不能顶替命中闪白。

```text
Q 施放：玩家 Attack 闪白 + 池里短挥击（MeleeSwing）
Q 命中：Dummy 闪白 + 池里 Hit 爆点（致死也要先闪白）
W 施放：池里 Cast 点；弹道本体保留
W 命中：Dummy 闪白 + 池里 Hit 爆点
E 施放：池里地面范围圈 / 色块（Area，半径 = AreaRadius）
E 命中：圈内 Dummy 闪白 + 池里 Hit 爆点（致死也要先闪白）
```

### 占位动画

无骨骼。用缩放 / 变色表达：`Idle / Run / Attack / Cast / Hit / Death`。近战 Windup+Active 用 Attack，弹道与范围用 Cast。Hit 闪白在 Death 缩小期间仍要看得见。

### 音频事件

事件名固定：`Cast`、`Impact`、`Hit`、`Death`。S1 无音频资源：`AudioEvents.Play` 打 `GameLog.Info("Audio", name)`。Cast / Impact 每次记；Hit / Death 每次生成后最多记 16 条，避免 300 只死亡刷屏。

### PerformanceArena

即 Arena 场景。F1/F2/F3 切密度；F5 采样。PlayMode 测试会对 100/200/300 各采一截。

测量钩子：`ArenaPerfHarness` 默认关（仅独立包 `-arenaPerf` 命令行或显式 `StartManual` 启动），`ArenaDirector.BuildIfNeeded` 内的钩子默认无操作；正式游玩路径不得自动开启。

写出 `Logs/s1-perf-arena.txt`，每行含：Dummy 数、采样帧数、主线程帧时间（avg / p95 / max，毫秒）、GC Alloc/帧（字节，托管 `GetAllocatedBytesForCurrentThread` 差值）。

S1 只要求能测量，不要求 1440p / 120FPS / 300 真实实体达标。

### 表现层约束

- 弹道视图禁止挂 `Rigidbody`。
- Dummy 视图关阴影，避免 300 阴影投射。
- 共用一份 URP Lit + `MaterialPropertyBlock`，禁止每 Dummy 一份 Material。

## 禁止

- 重写 S0 的 SeededRng / GameLog / ContentDatabase / ContentId / BootstrapRunner。
- Entities / Netcode / Full DOTS / 为 S1 上 Jobs/Burst。
- 第二套 RNG；`UnityEngine.Random` 当玩法随机源。
- 每个 Buff 一个 GameObject；每个弹道一个 Rigidbody。
- 所有怪物每帧完整寻路 / 每怪一个 NavMeshAgent 作为唯一 AI。
- 每次 Hit 扫描装备和天赋。
- 假写 `COMBAT_MATH.md` 公式。
- 第四个业务程序集；一次加六个空壳程序集。
- 重写 S1 输入 / 对象池 / SeededRng。
- 孔色；每个词缀一个技能脚本；图内改 Build。
- 把命中改成碰到胶囊 / Collider 才算打中。
- 玩家-怪物理互推；怪-怪 Rigidbody 或 CharacterController 互推。
- 运行时加载 Eve.blend；启用 skin 2–8 / 武器战斗。
- 把损坏的自制 FBX 再塞回工程当玩家模型。

---

## S2 玩法（Core / Gameplay）

仍走 `ArenaSim` + `ArenaDirector`。进图不换场景：F6 面板选词缀后进入同一 Arena 平面。

### 内核

```text
Tag, StatId, ModOp, Modifier, ConditionId, EffectId, EventId, Trigger
StatBag     Base/Flat/Increased/More/Override
TriggerSystem   DepthCap=8，每 Trigger 自带 MaxDepth 与 Cooldown
```

热路径不扫装备树：进图时快照，每技能 `CollectSkillMods` 写入 StatBag，Hit 只读 packet。

### Combat Math

见 `docs/COMBAT_MATH.md`。Q/W/E 进入 Active 当帧走 `CombatMath.ResolveHit`。无 Session 时保持 S1 固定伤害（EditMode 旧测试）。

### Support / Socket / Link

无孔色。槽位 Socket 全连成一条 Link：

```text
Weapon 3S → Q（1 Active + 2 Support）
Body   3S → W
Helmet 2S → E（1 Support）
Boots  0S → 只吃词缀
```

7 Support：Burning / Brutal / Focused / Swift / Combustion / **Fork（机制：命中后分裂 2 发，MechanicSkill=弹道）** / **Fire Conversion（火焰转化：50% 物理转火，`ConvertPhysToFire` Flat 0.50，RequiredTags=Attack|Hit|Physical 驱动兼容——近战/弹道可接、范围拒绝，无专用 Combat 分支）**。同一 Support 不能同时装在两条 Link。Support 安装经 `TrySetSupport` 兼容门（写入前拒绝，失败无半写入）；当前数量以最新 Content Audit 为准。

**多连接组（S5 Phase 1+2 已实现，BL-021.A2；权威合同 `docs/reviews/S5/S5_LINK_CONTRACT.md`）**：`ItemInstance.LinkSkill1`（SkillId，None=单连接 legacy，既有数据零迁移）。改挂后 group 0=前部 `SocketCount−2` 孔（映射技能容量=组孔数−1）、group 1=末尾 2 孔（被改挂技能容量恒 1，连接源整体迁移至此，原映射槽对该技能失效）；每技能至多一个有效连接源；2 组资格由 `SocketCount≥3` 推导（当前孔数下=Weapon/Body），资格集合不变。**写路径（S5-WO-03）**：唯一规则所有者=`SliceSession.ValidateLinkGraphPostState`（内核 `ValidateHostState`：可连接技能/孔数≥3/映射槽/禁自改挂/全局唯一源/拆分后两组原子容量）——`TryReassignLink`（假设性写入→校验→非法原子回滚）与 **`TryEquip`（装备/替换提交前同守卫，非法=整次拒绝，装备位与 LinkSkill1/Support 字节不变，禁静默清除/改写/截断）** 都经它，无规则复制；清除改挂=约束放宽恒合法；读路径遇腐败双改挂=fail-closed 忽略改挂回退 legacy（禁 first/last-wins）。**运行时隔离（S5-WO-03 实证）**：改挂技能经组 1 的 Support 等价生效恰一次（数值型 BuildPlayerHit 与机制型 Fork 真实分裂均组 0/组 1 等价），零跨组泄漏（每技能聚合只来自其唯一连接组）。**UI（S5-WO-03，有界呈现）**：技能行显示唯一有效连接源（`LinkSourceLabel`：host+组号+容量；改挂后原默认位不再呈现为生效源，无重复 Skill 行）；角色面板已装备的映射槽 ≥3 孔物品卡底部=「第二连接」配置条（候选过滤=排除自带映射与已被其它装备改挂；不可用候选灰显+原因；写入唯一走 `TryReassignLink`，UI 永不直写 LinkSkill1）；物品 Tooltip 追加连接组划分行（`LinkGroupsText`，3 孔拆分如实呈现 0+1、不暗示免费孔位）；失败均经 LastMessage 给出用户可读原因（host 不承载/超拆分容量/超组 1 容量/已被改挂/自改挂 5 类区分）。兼容判定唯一走 `IsSupportCompatible`+golden oracle（组位无关）；孔颜色/宝石等级/品质/持久化不引入。

### 装备 / 掉落 / Craft

6 装备槽（canonical `EquipSlot`：Weapon/Body/Helmet/Boots + S4 扩展 Gloves/Belt；Gloves/Belt 各 1 孔——仅作孔数展示，**不映射技能孔位**，SupportCapacity 仍只走 Weapon/Body/Helmet）。Ordinary 2 Affix，Rare 3–4。词缀见 `AffixCatalog`（**数量为易变快照，以最新 Content Audit / Production Report 为当前事实，本节不复制计数**；双行 Affix 各自独立掷值，第二值存 `ItemInstance` 第二值，消费路径按 `RowCount` 工作；槽位适用性=单一 applicability truth，AllowedSlots 位掩码决定可出现槽位，不兼容槽位不 roll）。**S5-WO-04 词缀批次（BL-002.A1，权威清单 `docs/reviews/S5/S5_AFFIX_ADMISSION.md`）**：追加 4 条单行词缀（迅疾=AttackSpeed Increased / 铁骨=Armour Increased（**Belt 不可出=GAME-ZZZ 有界适用性决策**）/ 睿智=Intelligence Flat / 坚韧=Strength Flat），全部复用既有 StatId/ModOp 与单一 applicability truth（随机池/定向制作/审计同源，无分支复制），经既有聚合轴消费（AttackSpeed=Recovery 缩放、Armour=乘算、Intelligence/Strength=属性+换算）；count-guard=S5 硬上限 21。击杀用 `LootRng`。随机 Craft（Scrap 洗 Rare）+ 定向 Craft（Etching 写入指定 Affix；物品内已存在同词缀时 deterministic reject，无半写入）。

### 天赋

真实 PoE 树（`PoeTree`，2429 上树节点）。起点免费。普通节点分配走 `SliceSession.TryAllocate`（只读 `TraversalTruth`）；效果兑现走 `RecalcPlayer` / `CollectSkillMods`（只读 `EffectTruth`）。route-only 节点可扣点作路径、贡献 0 modifier。

专精（S6P-WO-03）：静态 `TraversalTruth` 仍为 `SPECIAL_BLOCKED`，不得走普通 `TryAllocate`，也不得作为后续节点的 transit。进入选择器的前置 = 官方 `group` 簇 + 同簇 >=1 已分配 Notable。提交走 `TryAllocateMastery(node, sourceOrdinal)`：显式 choice identity、扣恰好 1 点、所选可兑现 choice 经既有 `PoeStatParser` 生效恰好一次。打开/取消选择器零 gameplay 增量。`R`/`TryRespec` 同时清除分配与选择。禁止 `FirstChoice` / `choices[0]` 默认生效。官方 payload 无 per-choice stable ID，identity fallback = `MasteryNodeId + source ordinal`。

死亡出图 `TryRespec`。

### 地图

1 张 Ash Court。3 词缀 Hearty / Savage / Ash Veil。`Reward = 1 + Σ RewardAdd`，`Stability = 100 - Σ Cost`。进图 `TryEnterMap` → `Snapshot = Capture()`（装备 6 槽索引 + Q/W/E 辅助 + `PassiveHash` FNV1A64 指纹 + Unspent）且 `State=InMap`（`BuildLocked`）。`PassiveHash` 是派生指纹，不是可还原 allocation 的容器。**没有** 32 位 `PassiveMask`（S5U 已迁到 `PassiveHash`）。**没有** clone/存档 seam。同 `SessionSeed` + 同词缀 → 同布局。3 普通（Brute/Stinger/Ashling）+ Elite Warden。

### 输入增量（ArenaDirector）

S1 键保留（按住走、按住 Q/W/E 连发）。S2 UI：顶栏「角色 / 地图 / 制作」与底栏技能格、Support 托盘可全鼠标完成循环。Tab / F6 / F8 仍是加速键。图内 F1/F2/F3 不刷密度 Dummy。

### S2 UI 文案（同一套）

```text
战斗中锁定
图内锁定构筑
清场可改构筑，可出图
出图后可免费重构
死亡出图，已免费重构
```

死亡出图必须 `Alive=true`、施放重置为 Idle，禁止卡在挥击里。稳定度 = 持久完整度 − 词缀消耗；死亡与清场会降低完整度。

**HUD 分轮换皮（Phase 3 开工，2026-09-08）。** 第 1 轮已落（导演批准：正式 UI 开工，本阶段目标=贴图+点击效，声音暂不做）：`SliceSkin` 程序化石质/金边贴图层（确定性合成，零外部资源）+ SliceHud 底栏重排（PoE 式左下双球=生命/法力、Q/W/E 槽石质槽位、辅助条并入底栏 2×4、导航/面板贴皮）+ 点击效（hover 金线点亮 / press 压暗 / 点击金闪约 0.16s）；全局等比缩放（设计空间 1920×1080 基准）。第 2 轮已落（右侧装备抽屉）：`SliceDrawerLayout` 纯几何层（设计空间纯函数）+ SliceHud 常驻抽屉列（右缘 300×326：迷你装备槽 **2×3 六槽**，只读 canonical `EquipSlot` 全六槽=展示顺序 Weapon/Helmet/Body/Gloves/Boots/Belt——S4-P2 起由 2×2 四槽扩为六槽，点击=开 Build 面板）+ Build（760×430）/Craft（560×340）面板右锚迁移（单一渲染器移动而非复制，`s.Panel` 唯一状态，Tab/F6/F8/Escape 路由不变）+ `BlocksWorldInput` 纯函数纳入抽屉吞区。第 3 轮已落（统一 Tooltip）：`SliceTooltipModel` 纯表现模型（装备卡=名称/稀有度/槽位/词缀行 canonical AffixLine；候选 vs 同槽已装备 union 对比 key=(StatId,ModOp) 仅 delta!=0、equipped-only 损失可见、同物品=「已装备」零噪音；Support 卡=描述+逐技能 ✓/× 兼容行+「可装配到当前技能/与当前技能不兼容」全部经 canonical `IsSupportCompatible`）+ `SliceTooltipLayout` 纯几何（pointer 右下默认/右溢翻左/下溢上翻/钳视口）+ 单一渲染器（每帧至多一卡，确定性优先级 面板>抽屉>底栏>顶栏；不可交互、不新增世界吞区）。数据与输入层（按键路由/Tab/F6/F8/QWE/拖拽装 Support）原样未动。**未完备部分**：天赋树重排（第 4 轮=Stage0 禁区待规划审查）——见 `docs/ui/UI_PROPOSAL_POE_D3.md` 分轮建议。

### 对象池

Dummy / Projectile / Feedback 容量与回收规则不变。Fork 从同一弹道池取子弹。新 FeedbackKind：Loot、Ignite。

---

## S6P-DIR-01（2026-09-11）运行时真值增补

### 主动技能域（5 条，ID 只追加不漂移）

```text
None=0, Melee=1, Projectile=2, Area=3, IceSpear=4, Fireball=5, Count=6
```

| 技能 | 键位 | Tag mask | 弹道 | 基础伤害元素 | 特殊 |
|---|---|---|---|---|---|
| 近战 | Q | `Attack\|Melee\|Hit\|Physical` | — | 物理 | 锥形 60° |
| 弹道 | W | `Attack\|Projectile\|Hit\|Physical` | 速 18 / 径 0.35 / 射程 16 | 物理 | 可分裂 |
| 范围 | E | `Spell\|Area\|Hit\|Physical` | — | 物理 | 半径 3.2 |
| 冰矛 | R | `Spell\|Projectile\|Hit\|Physical` | 速 30 / 径 0.28 / 射程 20 | 物理（无冰冷轴，见限制） | **穿透 3** |
| 火球术 | T | `Spell\|Projectile\|Area\|Hit\|Fire` | 速 14 / 径 0.42 / 射程 14 | **纯火焰** | **命中点爆炸 2.6**（排除直接命中目标） |

### 连接组（辅助孔位真值）

`Melee → QSupports`；**`Projectile / IceSpear / Fireball → WSupports`（同一弹道连接组共享孔位）**；`Area → ESupports`。
查重（同一 Support 不得装两处）按**组代表**判定（`SliceSession.ConnectionGroupRep`）。
`MappedSlot`：冰矛/火球术随弹道组映射到胸甲。

### 机制型 Support 的限制表达

`SupportDef.MechanicSkill` 由「技能身份相等」改为**Tag 蕴含**：`Projectile` 表示「只接投射物投送」= 要求 `Tag.Projectile`。
兼容判定唯一入口仍是 `SliceSession.IsSupportCompatible`（golden 矩阵 `5×9` 与 runtime 逐组合对拍）。

### 弹体结算（`ProjectilePool` + `ArenaSim`）

1. 每投射物持有 **已命中位图**（每目标 1 bit）——唯一「同一目标不重复结算」真值，穿透与返程共用。
2. 命中时：若 `PierceLeft > 0` → 递减并**继续飞行**；否则若装配「投射物返回」→ **掉头**（返程自带距离预算，抵达玩家 ≤0.85 即消失）；否则按原规则消失。
3. 飞完射程同理：可返回则掉头，否则消失。
4. `ImpactAreaRadius > 0`（火球术）→ 命中点做一次范围结算（复用 `ApplySkillHit` 路径，排除直接命中目标避免双算）。

### 狙击印记（`SupportId.SnipersMark`）

- 写入唯一入口 `DummyCrowd.ApplyMark`；衰减唯一入口 `DummyCrowd.TickMarks`（`ArenaSim.Tick` 每帧）。
- 施加时机：装配该辅助的投射物**命中且判定为 Hit** 时，给**命中目标**打标（单体）。
- 增伤：目标已带印记时，构造 `HitRequest` 阶段 `MoreDamage *= 1 + SliceRules.SnipersMarkMoreDamage(0.35)`——**在护甲/抗性减免之前**进入同一条 `CombatMath.ResolveHit`，无第二条结算路径。
- 呈现：`EnemyFeedbackState.Mark`（优先级最低：Hit > Ignite > Mark），仅读 `MarkRemain > 0`，零 gameplay 写入。

### 美术来源（呈现层，非 gameplay）

`SlicePoeArt` 从 `Resources/UI/PoE/{Items,Skills,Supports,Vfx}` 载入 poedb 真实美术；
消费点 `SliceHudIcons.ItemIcon / SkillGlyph / SupportGem`、`ArenaDirector.BuildProjectileArt`（贴图球体 + Unlit）。
回退链：poedb → Aria → 程序化合成，任一层缺失不影响可用性。
逐条来源 = `docs/reviews/S6P/S6P_DIR_01_POEDB_SOURCING.md`。
