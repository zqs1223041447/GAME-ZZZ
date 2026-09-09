# S5_LINK_CONTRACT — Multi-Link 权威合同（S5-WO-01 锁定；AC-02/03/04/05/06/07/08/09 交付物）

**性质**：BL-021.A2（多连接组）的唯一权威实现合同。本文由 S5-WO-01 锁定；S5-WO-02 起 runtime 实现必须逐条遵守，不得在本文件之外发明产品行为。**状态=域核+运行时/UI 集成已实现（S5-WO-02 域核+S5-WO-03 集成；实现证据=`S5_WO_02_EVIDENCE.md`/`S5_WO_03_EVIDENCE.md`；RUNTIME.md 已同步）**。
**MaxLinkGroupsPerEligibleItem = 2**（唯一权威数值；其它文档只可引用本句）。

## 1. 当前真相源指认（合同基线；全部为既有代码事实，本单零改动）

| 项 | 真相源 | 现状事实 |
|---|---|---|
| 每槽孔数（不 roll） | `Catalogs.SliceRules`：`WeaponSockets=3 / BodySockets=3 / HelmetSockets=2 / BootsSockets=0 / GlovesSockets=1 / BeltSockets=1` | `RollItem` 由调用方传入；`GiveStarterItems`/掉落按槽常量传 |
| 连接资格（Support 容量>0 的槽） | `SliceSession.SupportCapacity(SkillId)`：硬映射 `Melee→Weapon / Projectile→Body / Area→Helmet`；`cap = item.SocketCount − 1`（首个孔=技能宝石位，其余=Support 位），上限受会话 Support 数组长度钳制 | Gloves/Belt 1 孔与 Boots 0 孔 → 容量 0（`GlovesSockets/BeltSockets` 注释：「不映射任何技能孔位——SupportCapacity 仍只走 Weapon/Body/Helmet」） |
| Support 存储 | 会话数组 `QSupports/WSupports/ESupports`（按技能），`ClampArr` 按容量清空；物品本身只存 `SocketCount` | 物品→技能绑定当前为隐式（槽位硬映射） |
| Support 兼容 | 单一入口 `SliceSession.IsSupportCompatible` + 独立 golden oracle `SupportCompatGolden`（3×7=21 组合，16 兼容/5 不兼容；禁反向生成） | `TrySetSupport` 写入前判定，失败无半写入 |
| 顺序 | 数组索引即顺序（支持位 0..cap−1）；技能→槽位硬映射 | 确定性 |
| runtime 消费路径 | `SupportCapacity → ClampSupportsToSockets → CollectSkillMods`（进图快照聚合，热路径不扫装备树） | 既有契约不变 |
| UI 表示路径 | `SliceHud`（技能行 Support 托盘 `s.SupportCapacity(skill)`）+ 物品 Tooltip「N孔」 | Phase 2 仅最小扩展 |

## 2. 锁定的多连接组行为

1. **Group 0（legacy）**：物品当前隐式连接=其映射技能的连接（Weapon→Melee / Body→Projectile / Area→Helmet）。无 group 1 时，行为与现状**逐位等价**（容量=SocketCount−1；兼容/顺序/UI 全部不变）。
2. **Group 1（可选改挂）**：具备 2 组资格的物品可**改挂**一个第二技能的连接。绑定规则：`LinkSkill1 ∈ {Melee, Projectile, Area} \ {该物品映射技能}`（禁止与 group 0 重复绑定同一技能）。
3. **改挂语义（唯一 link 原则）**：被改挂技能 X 的连接**恰好一处**——由 hosting item 的 group 1 提供；X 原映射槽对 X 的隐式连接即失效（该槽的 group 0 继续服务其自己的映射技能，行为不变）。一个技能在全图**至多一个连接源**（映射槽 或 恰一个 group 1）；对已绑定技能的第二次改挂请求=确定性拒绝。
4. **容量规则（不变）**：每组容量 = 该组孔数 − 1（首孔=技能宝石位）。组划分确定性：**group 1 固定占用物品末尾 2 孔（1 技能位+1 Support 位）；group 0 = 其余前部孔**。故 2 组资格要求 `SocketCount ≥ 3`（当前孔数下=Weapon/Body；Helmet 2S/Boots 0S/Gloves·Belt 1S 自然不满足——**资格集合不变**：可连接槽仍为 Weapon/Body/Helmet，2 组子集由孔数≥3 推导，无新增带连接槽位）。启用 group 1 为**显式选择**：不做=legacy 完整容量（Weapon/Body 2 Support）；做=group 0 剩 (SocketCount−2)−1 个 Support 位（3S 物品=0 Support）+group 1 固定 1 Support 位——这是「双连接 vs 大单连接」的**真实取舍**。
5. **第三组拒绝**：表示层只有 group 0（隐式）+ group 1（可选）两个槽位；任何第三组请求确定性拒绝（AC/测试矩阵场景 3）。
6. **隔离**：每组独立经 `IsSupportCompatible` 评估（同一入口、同一 golden oracle）；Support/效果不得跨组泄漏——改挂语义下每技能恰一个连接源，结构性排除泄漏。
7. **确定性顺序**：组身份=固定索引（0=legacy 隐式组、1=改挂组）；组内 Support 顺序=数组索引顺序；组划分固定（末尾 2 孔）。
8. **不引入**：孔颜色（无字段/匹配/UI/需求）；宝石等级/品质（无字段/缩放）；持久化/存档；新增带孔或带连接槽位；新增 Skill/Support。

### 2a. S5-WO-01 评审裁定补丁（2026-09-09 规划 AI Gate Review=ACCEPT WITH FOLLOW-UP；代码前强制收口）

- **改挂语义（已批准）**：整体迁移（reassignment）而非叠加（not stacking）。任一技能的有效连接源数 ≤1：group 1 改挂后该技能默认隐式连接源失效、Support 列表跟随技能身份（不复制到新集合）、清除改挂即恢复默认源。
- **必须拒绝的写入**：①把 host 自带 group 0 技能再挂为其 group 1（自改挂）；②同一技能被两个已装备物品同时改挂；③任何会产生双有效连接源的状态。**禁止 first/last-wins 式裁决**；腐败构造态 fail-closed（读路径检测到重复改挂=忽略改挂、回退映射槽 legacy）。
- **缺组 parity（明确条款）**：`LinkSkill1 == None` ⇒ group 0 拥有该物品完整孔集；legacy 容量与行为逐位不变。
- **改挂生效前提**：物品连接资格不变（Weapon/Body/Helmet）+ `SocketCount ≥ 3`；划分=group 1 末尾 2 孔 / group 0 其余前部孔；两组共用既有公式 `容量 = 组孔数 − 1`。
- **总容量取舍（有意设计，非回归）**：启用 group 1 消耗第二个技能宝石位。3 孔例：legacy=group 0 3 孔→2 Support；拆分=group 0 1 孔→0 Support + group 1 2 孔→1 Support。
- **原子容量校验（写入前）**：写入/变更 `LinkSkill1` 前校验拆分后两组容量——host 映射技能既有 Support 数 ≤ 拆分后 group 0 容量；被改挂技能既有 Support 数 ≤ group 1 容量（1）。任一溢出 ⇒ **原子拒绝**（不消耗、不半写入）；**禁止**静默截断、自动移除、自动迁移、重排凑容。
- **生命周期**：Assign=激活 group 1+抑制被改挂技能默认源+保留其 Support 列表+套用 group 1 容量；Change（A→B）=单次校验事务（无外部可见双源中间态）；Clear（→None）=移除 group 1+恢复被改挂技能默认源+host group 0 恢复完整孔集。

### 2b. S5-WO-03 集成补记（2026-09-09；Phase-2 Integration Guard 落地语义，权威语义以 DECISIONS.md 同名条目为登记）

- **唯一规则所有者**：有效连接源图后置校验=`SliceSession.ValidateLinkGraphPostState`（内核 `ValidateHostState`：可连接技能/孔数≥3/映射槽资格/禁自改挂/全局唯一源/拆分后两组原子容量）。`TryReassignLink`（假设性写入→校验→非法原子回滚；清除=约束放宽恒合法）与 **`TryEquip`（装备/替换提交前同守卫）** 都经它——装备路径与改挂路径禁止规则副本。
- **装备变更守卫**：任何装备操作不得产生改挂 API 自身会拒绝的后置状态；非法转换=原子拒绝（装备位与 LinkSkill1/Support 字节不变），禁 first/last-wins、禁静默清除/改写 LinkSkill1、禁静默截断/迁移 Support。同槽同技能配置 host 交接=合法移交（旧 host 离图入包休眠）。
- **UI 有界呈现/配置**：资格呈现=`SecondaryLinkConfigurable`（映射槽∧SocketCount≥3，与域同源）；「第二连接」配置条仅在已装备合格物品卡；候选=`RebindCandidates`（排除自带映射与已被其它装备改挂者）；可用性预判=`PreviewReassignError`（与写入前校验同一内核，零写入）；技能行唯一有效源标注（`LinkSourceLabel`，改挂后原默认位不再呈现为生效源，无重复 Skill 行）；Tooltip 划分行（`LinkGroupsText`，3 孔拆分如实 0+1）；UI 永不直写 LinkSkill1；失败经 LastMessage 5 类可读原因，无内部异常文本。
- **运行时隔离**：改挂技能经组 1 的 Support 等价生效恰一次（数值型 BuildPlayerHit 与机制型 Fork 真实分裂双证明）；零跨组泄漏。golden 21 组合组位无关维持。

## 3. Data-Model Decision（AC-08：唯一选定表示）

**选定表示：`ItemInstance` 追加单个字段 `LinkSkill1`（SkillId），其余全部由既有字段+确定性规则派生。**

- `SkillId.None = 0`（`CombatTypes.cs`）⇒ `default(ItemInstance).LinkSkill1 = None` = 单组 legacy——**既有数据/既有构造零迁移**（AC-05 legacy mapping：现有连接≡group 0，语义零变化）。
- 权威数据所有者：`ItemInstance`（库存物品本体）；会话层只在读路径消费（`SupportCapacity` 增加「改挂查询」分支）。
- 合法基数：`LinkSkill1=None`（0 组外挂，legacy）或 `LinkSkill1∈{技能}\{映射技能}` 且 `SocketCount≥3`（1 个外挂组）；总计组数∈{0,1} 外挂 ⇒ 每物品组数∈{1,2}。非法态处理：写入前校验（映射技能重复=拒绝；SocketCount<3=拒绝；技能已被其它物品 group 1 绑定=拒绝），失败无半写入（与 `TrySetSupport` 同语义）。
- 组内 Support 表示：**复用既有会话 per-skill Support 数组**（改挂只改容量来源与容量值，不新增数组/存档字段）；group 1 容量恒=1（2 孔−1）。
- Unity 序列化兼容：`ItemInstance` 为纯 C# struct（会话 List 持有，非 MonoBehaviour 序列化字段），新增 byte 字段无 prefab/scene 资产迁移；`SkillId.None=0` 保证旧数据/存档无（当前无存档）兼容。
- runtime 读路径：`SupportCapacity(skill)` 判定顺序：①存在已装备物品 `LinkSkill1==skill` → cap=1；②否则按映射槽 legacy `SocketCount−1`。`ClampArr`/`CollectSkillMods` 不变。
- UI 读路径：技能行显示连接来源物品（映射物品或「改挂→<物品名>」）与 Support 位；物品 Tooltip 显示「N孔（2 连接组）」；仅此最小扩展，无颜色/等级元素。

**被否决的备选（tradeoff 记录，非权威）**：①任意组划分/玩家拖孔分配——交互与确定性成本高，非「有界基数扩展」；②容量叠加（一个技能吃两个组）——违反隔离规则（同一技能两组供能=跨组泄漏语义）；③每槽新增 LinkGroup 数组序列化——超出「不做通用 Socket 重写」边界。

## 4. 12 场景测试矩阵（AC-09；S5-WO-02 起逐条落地为测试）

| # | 场景 | 期望 |
|---|---|---|
| 1 | legacy 1 组（无 LinkSkill1，全部 3 技能按映射槽） | 与现状逐位等价（容量/兼容/UI） |
| 2 | 合法 2 组（3S Weapon 改挂 Projectile） | Weapon group 0=Melee 1 孔 0 Support；group 1=Projectile 1 Support；Body 对 Projectile 的隐式连接失效 |
| 3 | 第三组请求（LinkSkill1 已设后再挂；或 SocketCount<3 请求） | 确定性拒绝，无半写入 |
| 4 | group 0 合法 + group 1 合法（两组 Support 均过兼容门） | 双双生效，互不影响 |
| 5 | group 0 非法 Support 配对（如 Fork→Area 等 golden 不兼容组合） | `IsSupportCompatible` 拒绝，失败无半写入 |
| 6 | group 1 非法 Support 配对（同一 golden 不兼容组合落在改挂组） | 拒绝；组位不改变兼容语义 |
| 7 | 无跨组兼容泄漏（group 0 Support 不影响 group 1 技能，反之亦然） | 各技能 Support 集只来自其唯一连接组 |
| 8 | 确定性顺序（同 seed 同操作 → 同组划分/同组内 Support 序） | 精确可复现 |
| 9 | 资格不变（Helmet/Boots/Gloves/Belt 行为与现状一致；不可挂 2 组） | SupportCapacity/UI 全部与 legacy 等价 |
| 10 | 每组容量规则不变（容量=组孔数−1；group 1 恒 1） | 全场景成立 |
| 11 | 零孔色依赖（无颜色字段/匹配/展示/需求路径） | 合同护栏断言通过 |
| 12 | 零等级/品质依赖（无 level/quality 字段/缩放） | 合同护栏断言通过 |

**Golden oracle 复用策略**：`SupportCompatGolden`（21 组合）不变、不复制；兼容判定仍唯一走 `IsSupportCompatible`；S5-WO-02/03 的测试把既有 golden 用例在 group 0 与 group 1 两个位置各跑一遍（位置无关性证明），golden 文件零改动。

## 5. 禁区护栏（AC-07；NB-5 断言清单）

实现与其测试必须能捕获以下意外引入并以失败告终：①任何孔颜色字段/匹配/展示；②任何宝石等级/品质字段或缩放；③第三连接组（表示与校验两层）；④新增带连接槽位（资格集合变化）；⑤未批准词缀语义（新 Stat/ModOp 越界、新词缀族——与 `S5_AFFIX_ADMISSION.md` 联动）。

## 6. 明确不做（S5 全周期）

Socket colors；color matching；Gem Level；Gem Quality；Persistence；新增带孔槽位；新增 Skill/Support（本合同节 2.8 与 `S5_PLAN.md` §4/§5 同源）。
