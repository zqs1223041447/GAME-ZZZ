# S6P-DIR-01 · poedb 美术接入 + 冰矛/火球术 + 投射物返回/狙击印记

| 项 | 值 |
|---|---|
| 工作令 | **S6P-DIR-01**（导演插入令，2026-09-11） |
| 权威来源 | 导演 2026-09-11 补充要求（本文件 §1 逐条引用原话） |
| 前置 | S6P-WO-04A（planner ACCEPT WITH FOLLOW-UP，基线 EditMode 434 / PlayMode 17） |
| 状态 | 见 `S6P_DIR_01_EVIDENCE_PACK.md` |
| 关联 | `S6P_DIRECTOR_FEEDBACK_2026_09_11.md`（同批导演补充要求的 1/2/3 三条 + 真值冲突登记） |

## 1. 需求（导演原话，逐条不改写）

| # | 原话 | 本轮处置 |
|---|---|---|
| 1 | 「背包做成快捷键开关式」 | 已实现（`I` 开关 + 面板「关闭 I」按钮 + 底栏宽度随开关联动）。证据见 `S6P_DIRECTOR_FEEDBACK_2026_09_11.md` |
| 2 | 「天赋树……我没法点击或者说人眼看他们不是通过线连接起来的」 | 已实现（连线改由真实节点坐标绘制、簇底衬由成员节点几何派生、默认缩放 0.45 居中起点）。同上文件 |
| 3 | 「怪物的移动似乎没有动画、攻击也没有」 | 已定位并修复（Troll 系 5 条 clip 未勾 Loop Time → Run/Walk 冻在末帧）。同上文件 |
| 4 | 「装备图标现在是纯2D，我需你从POEDB.TW上扒取装备图标来用」 | **本文件 §3** |
| 5 | 「技能特效顺别也从POEDB.TW上扒取，制作冰茅和火球术」 | **本文件 §3/§4** |
| 6 | 「辅助技能做一个投射物返回和狙击印记的效果试试」 | **本文件 §5** |

## 2. 交付概览

| 层 | 内容 |
|---|---|
| 素材 | 30 个 poedb 资源落盘（12 物品图 / 5 技能宝石图 / 9 辅助宝石图 / 4 特效图），逐条来源见 `S6P_DIR_01_POEDB_SOURCING.md` |
| 运行时新增 | `SlicePoeArt`（poedb 美术加载层，含 `DeclaredKeys` 单一真相） |
| 技能 | `SkillId.IceSpear = 4`（冰矛）、`SkillId.Fireball = 5`（火球术）；键位 R / T |
| 辅助 | `SupportId.ReturningProjectiles = 8`（投射物返回）、`SupportId.SnipersMark = 9`（狙击印记） |
| 机制 | 投射物**穿透**、投射物**返回**、命中点**爆炸**、单体**印记**（增伤） |
| 测试 | 新增 26 条（`S6PDir01Tests`）+ 既有钉死计数按显式理由上抬 |

## 3. 装备图标与技能图标（需求 4 / 5 前半）

- 分档：`EquipSlot × Rarity`（6 槽 × 普通/稀有 = 12 张），落到 `Resources/UI/PoE/Items/`。
- 消费点（唯一）：`SliceHudIcons.ItemIcon(slot, rarity)` —— 背包网格格心、装备卡图标、装备卡空槽回退三类位置共用它。
  优先级：**poedb 物品图 → Aria 通用槽位图 → 程序化合形**。任一层缺失都不影响可用性（`SlicePoeArt.Get` 返回 null，不抛）。
- **等比缩放**：PoE 物品图不是正方形（单手剑 1×3 = 78×234、腰带 2×1 = 156×78），塞进方形格会拉伸变形；
  绘制端新增 `SliceHud.FitAspect`：按纹理原始宽高比在格内居中摆放（实测最宽/最高项在格内都完整不裁）。
- 技能槽图标：`SliceHudIcons.SkillGlyph(skill)`，同一优先级；宝石图**自带配色**，故槽位不再整体染色（仅按选中/悬停做轻明度差），否则宝石本色被压暗。
- 辅助宝石托盘：`SliceHudIcons.SupportGem(id)`，此前托盘只有文字，现在每格带 PoE 宝石图 + 名称。

**边界（不夸大）**：poedb CDN 只发布 `.webp` 静态美术（同路径 `.png` 一律 403），且它是**图标级美术**，
不是粒子系统。因此「技能特效」的兑现口径是：**弹体贴图 / 槽位图标取自 poedb 美术**，
而飞行、拖尾、命中爆炸的**粒子行为仍由引擎自绘**。这一点在 `S6P_DIR_01_POEDB_SOURCING.md` §0 里明写。

**弹体形态**：平面广告牌（poedb 图标是不透明方图）实测会出现方晕/饱和方片，故最终采用**贴图球体 + Unlit 着色**
（无方晕、美术本色），冰矛再叠一层非等比拉长做「细长锐利」；四轮取舍留档在 sourcing §4.2。
冰矛/火球两项特效图在摄取期做过**唯一的派生化处理**（亮度软阈值抠除深色背景，单点可撤）。

## 4. 冰矛与火球术（需求 5 后半）

### 4.1 身份（稳定数值 ID，只追加不漂移）

```
SkillId: None=0, Melee=1, Projectile=2, Area=3, IceSpear=4, Fireball=5, Count=6
```

### 4.2 定义

| 技能 | Tag mask | 定义要点 | 键位 |
|---|---|---|---|
| 冰矛 | `Spell \| Projectile \| Hit \| Physical` | 弹速 30（基础弹道 18）、弹径 0.28（基础 0.35）、射程 20、**穿透 3** | R |
| 火球术 | `Spell \| Projectile \| Area \| Hit \| Fire` | **纯火焰基础伤害**（`BaseDamageIsFire`，物理分量为 0）、弹速 14、**命中点爆炸半径 2.6**、可点燃 | T |

- 元素语义由 `SkillDef.BaseDamageIsFire` 决定，不再「所有投射物都是物理基底 + 附加火焰」。
  火球术因此不会伪造一份物理伤害。
- 火球术带 `Area` Tag ⇒ 自动吃到 `AreaRadiusMore` / `AreaDamageMore`，也自动与「集中」兼容（其爆炸确为范围分量）。
  `ResolveSkillDef` 里原先硬编码 `id == SkillId.Area` 才缩放半径，已泛化为「任何带半径的技能」，否则火球术的爆炸会静默忽略范围词条。
- 冰矛与基础弹道同列？**不**。冰矛是法术弹道（无 `Attack` Tag），所以命中判定走法术必中路径，
  与吃命中/闪避的攻击弹道区分开。

### 4.3 连接组（重要设计决定）

冰矛/火球术**不新增连接数组**，而是与「弹道」共享同一连接组（`WSupports`）：

- `SupportsOf(IceSpear) == SupportsOf(Fireball) == WSupports`；`MappedSlot` 两者都映射到胸甲。
- 理由一（语义）：PoE 里同一连线上的技能共享辅助宝石；本工程三者的「弹道连接组」就是这一语义。
- 理由二（不变量）：`BuildSnapshot` / ProdSim canonical payload 只含 `W0/W1`，**不新增字段 ⇒ canonical hash 不受影响**。
  若为两条新技能各建一套连接数组，必须扩 payload，进而移动已被 planner 接受的基线 `FNV1A64:ec1d3ed67d3035d0`——那是 WO-03 的职权，不应由本令顺带触发。
- 连带修正：查重（「同一 Support 不许装两处」）原按技能身份比对，共享数组后会把同组自己判成占用 ⇒
  改为按**组代表**（`ConnectionGroupRep`）比对。这是共享语义下的必要修正，不是放宽。

### 4.4 机制型 Support 的限制表达

原 `IsSupportCompatible` 用「技能身份相等」表达机制限制（`MechanicSkill != skill` ⇒ 拒），
且把技能枚举写死为 1..3。现改为**Tag 蕴含**：`MechanicSkill = Projectile` 表示「只接投射物投送」
⇒ 凡带 `Tag.Projectile` 的技能都满足。这样新增投射物技能不需要再改兼容判定，也避免留下第二个技能清单。

## 5. 投射物返回与狙击印记（需求 6）

### 5.1 投射物返回（`SupportId.ReturningProjectiles`）

PoE 语义：**命中目标后，或飞完射程后，投射物掉头返回你**；返程可以再命中（**同一敌人不会被同一投射物命中两次**）。

实现（唯一落点 = `ProjectilePool`）：

- `Projectile.CanReturn/Returning`：装配支持时置位。命中或 `Traveled >= MaxDistance` 时不再消失，而是掉头；
  掉头不是无限制折返——返程自带距离预算（= 掉头瞬间到玩家的距离），到达玩家（≤0.85）即消失。
- 「不重复结算」的唯一真值 = `ProjectilePool._hits` 位图（每投射物 × 每目标 1 bit）。
  这是**穿透与返回共用**的同一条不重复规则，不写第二份判据。
- 无玩家上下文（`sim == null`）时按原规则消失——纯 `ProjectilePool` 单元用法不受影响。

### 5.2 狙击印记（`SupportId.SnipersMark`）

PoE 语义：对单体施加印记，被印记的敌人受到的**投射物**伤害提高。

实现（全部复用既有结算路径，无新 Effect / 新 Stat）：

- `Dummy.MarkRemain`：唯一写入点 = `DummyCrowd.ApplyMark`，唯一衰减点 = `DummyCrowd.TickMarks`（在 `ArenaSim.Tick` 中每帧调用）。
- 施加：支持的投射物**命中时**给命中目标打上印记（单体，旁观者不打）。
- 增伤：若目标**已被印记**，在构造 `HitRequest` 时 `MoreDamage *= 1 + SliceRules.SnipersMarkMoreDamage`，
  即**在护甲/抗性减免之前**进入同一条 `CombatMath.ResolveHit`。不新增第二条伤害结算路径。
- 数值唯一真相源 = `SliceRules.SnipersMarkMoreDamage = 0.35f` / `SnipersMarkDuration = 8f`；
  `SupportDef.Desc` 的「35%」由该常量派生，防止文案与数值两处漂移。

### 5.3 两条 Support 的目录契约

| 项 | 值 |
|---|---|
| `ChangesMechanism` | 均为 `true` |
| `MechanicSkill` | 均为 `SkillId.Projectile`（经 Tag 蕴含判定） |
| `TriggerEffect` | 均 `None`（不为机制型支持新增 EffectId） |
| `Mods` | 均为**空数组**——纯机制，不虚构 Stat 轴（避免「挂了个没作用的数值」） |
| 兼容 | 弹道/冰矛/火球术 ✓；近战/范围 ✗（golden 与 runtime 逐组合对拍） |

## 6. 兼容矩阵与钉死计数的显式上抬

矩阵由「3 技能 × 7 支持」扩为「5 技能 × 9 支持」= 45 组合，逐条人工钉死在 `SupportCompatGolden`（含负例：
火焰转化不接冰矛/火球术，集中不接冰矛）。同步上抬的钉死值及理由：

| 文件 | 旧 → 新 | 理由 |
|---|---|---|
| `SupportCatalogInvariantTests` | `SupportId.Count` 8→10；`SupportCatalog.Count` 7→9 | +2 机制型 Support（导演令） |
| `S3R2FireConversionTests` | 总数 7→9；矩阵 `3×7`→`5×9` 断言改为派生式 | 同上；火焰转化自身定义断言逐条不变 |
| `ContentAuditS2Tests` | Support 护栏 `基线+1`→`基线+3`；Skill 护栏 `Area=3`→`Fireball=5`；技能枚举改用 `SkillTagGolden.All` | 内容轴有界扩容，护栏改为「当前真实值」仍保持有界 |
| `ProductionContentReportTests` | 只改断言文案（计数已由 audit 派生） | `data.SkillCount = audit.ActiveSkillCount`，无需硬编码 |

**未动**：`StatId.Count = 28`、`ModOp`、`Tag`、`EffectId`、`EventId`、`ConditionId`、`AffixId.Count`、图词缀 —— 全部保持原钉死值。

## 7. 已知限制（不隐藏）

| # | 限制 | 原因 | 影响面 | 解除线索 |
|---|---|---|---|---|
| L1 | **引擎无冰冷/闪电伤害轴**，冰矛的基础伤害落在物理轴 | `PoeStatParser` 注释即写明「引擎无冰冷/闪电轴：全元素抗性只记火焰，不虚构」；新增 `StatId.ColdDamage/AddedCold` 会让 `PassiveAwareProductionSimulation.StatSnapshot`（按 StatId 全序哈希）变化 ⇒ 移动 planner 已接受的 ProdSim 基线 | 冰矛的**数值**是物理观感、**辨识**由美术/颜色/穿透机制承载；被动「冰霜伤害」词条不存在 | 由 WO-03 的 ProdSim V3 重新基线，或在专门冻结令下追加冷/电轴 |
| L2 | poedb 不发布粒子系统，特效只有图标级美术可用 | 站点资源性质（见 sourcing §0）；平面广告牌实测有方晕 | 弹体=贴图球体（Unlit）+ 引擎自绘粒子；非 PoE 原版粒子 | 若需要真粒子资产，需另找来源或自制 |
| L3 | 冰矛/火球术与弹道**共享**连接组（辅助孔位同源） | 见 §4.3（避免移动 canonical payload） | 三者显示同一连接徽章与同一组支持孔 | 若要求各自独立孔位，需连同 payload/基线一起做 |
| L4 | 狙击印记只实现「单体 + 增伤」两要素 | 计划弹道分裂/回旋等其余要素需新增弹道改写机制 | 印记不会把投射物拉向目标 | 后续机制令 |

## 8. 不做（本轮明确越界项）

- 不新增 `StatId` / `EffectId` / `EventId` / `ModOp` / `Tag` / `Affix`；
- 不新增地图词缀、不新增职业/升华、不触碰 Curse/Flask/Jewel 运行时、不做深度制作；
- 不改任何既有 Support（1..7）的身份、数值或兼容语义；
- 不修改 ProdSim canonical contract / payload 字段集；
- 不改天赋树任何 truth（本令只动 UI 呈现与怪物动画导入设置，见 `S6P_DIRECTOR_FEEDBACK_2026_09_11.md`）。
