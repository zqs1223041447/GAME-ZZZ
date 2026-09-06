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

S1 音频无资源时：`[Audio] Cast|Impact|Hit|Death`。性能采样：`[Perf] ...`。

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

- 玩家视图：S1 胶囊（自制 Eve FBX 已剔除）。根上留 CapsuleCollider（贴地/墙用），不挡怪。
- Dummy 视图：无 Collider、无 Rigidbody、无 CharacterController。
- 命中仍是距离 / 锥 / 弹道半径，不是物理接触。
- 怪-怪：`DummyCrowd.Separate(DummyMinSeparation=1)`，在 `ArenaSim.Tick` 里 `TickAi` 之后调用。距离 < 1 水平各拨一半；`dist==0` 沿 +X 拨开，禁止除零。
- 玩家移动不检测 Dummy 位置，可以走进怪群。

跟随相机默认偏移 `(0, 17, -15)`，FOV 42，LookY `0.5`（与旧 `(0, 8.5, -7.5)` 同俯角，约 2 倍距离）。玩家视图胶囊约高 `1.16`、半径 `0.32`。Dummy 视图系数 `0.58`（相对旧 1×1 方块）。**不改** 速度 / 攻击距离 / DummyRadius / 分离。命中仍是距离 / 圈 / 弹道。Game 窗口的 Scale 滑条是像素放大，不是提高分辨率。

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

6 Support：Burning / Brutal / Focused / Swift / Combustion / **Fork（机制：命中后分裂 2 发）**。同一 Support 不能同时装在两条 Link。

### 装备 / 掉落 / Craft

4 槽。Ordinary 2 Affix，Rare 3–4。10 Affix 见 `AffixCatalog`。击杀用 `LootRng`。随机 Craft（Scrap 洗 Rare）+ 定向 Craft（Etching 写入指定 Affix）。

### 天赋

16 节点。Start 免费。2 Notable：Brutal Strikes、Pyre。1 机制：Cinder Heart（40% 物转火）。死亡出图 `TryRespec`。

### 地图

1 张 Ash Court。3 词缀 Hearty / Savage / Ash Veil。`Reward = 1 + Σ RewardAdd`，`Stability = 100 - Σ Cost`。进图 `BuildSnapshot.Locked=true`。同 `SessionSeed` + 同词缀 → 同布局。3 普通（Brute/Stinger/Ashling）+ Elite Warden。

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

**HUD 未完备。** 角色背包、技能栏、装备、天赋的正式 HUD 后补，当前底栏/面板只是占位，不得当作完成。

### 对象池

Dummy / Projectile / Feedback 容量与回收规则不变。Fork 从同一弹道池取子弹。新 FeedbackKind：Loot、Ignite。
