你这次的约束很清楚：不采购、不换框架、不等人、不做需要主观肉眼才能判定的工作。 下一段我建议排 5 张 WO，主线是“先让门禁真正覆盖天赋域，再修天赋真相，再做表现优化”。药剂、珠宝、升华、新技能和真正的新 Stat 轴都先排除出无人值守队列。
GAME-ZZZ 下一轮无人值守工作令序列
GAME-ZZZ 下一轮：Passive Truth & Deterministic Build Backbone
总顺序
WO-01 被动域 census + 门禁覆盖审计
 ↓
WO-02 ProdSim 纳入被动构筑真相
 ↓
WO-03 专精显式选择正确化
 ↓
WO-04 现有引擎语义覆盖闭环 + 禁止静默空效果
 ↓
WO-05 天赋树 Overview LOD + 图标显存治理
这个顺序的核心原因是：
先保证“以后改坏天赋，门禁能发现”，然后才继续改天赋。
目前最大的潜在风险是：天赋域已经很大，但当前 ProdSim hash 仍是 
FNV1A64:9a4c9524d0b3e214。如果 ProdSim 没实际覆盖被动分配/效果，那么“hash unchanged”对被动正确性几乎没有保护作用。
S6P-WO-01 — Passive Truth Census & Gate Coverage Audit
为什么现在做
这是所有后续工作的 denominator。
现在同时存在：
2429 主树节点；
315 Mastery；
57 Jewel Socket；
30 special/Timeless 类节点；
558 Ascendancy；
Director 口径 2832；
531 节点当前产出 modifier。
在没明确集合包含关系前，不能继续用“78% 未覆盖”指导代码工作。
同时必须先确认 ProdSim 究竟有没有消费被动状态。
改动面
域层： NONE
绘制层： NONE
测试/工具：
新增 
S6PassiveCensusTests 或同义 test family；
census/report generator；
ProdSim input/output coverage audit。
文档：
passive census；
semantic coverage matrix；
ProdSim surface map。
完成口径
机械要求：
Authoritative node sets reconciled = YES

2429 meaning = explicit
2832 meaning = explicit or UNRESOLVED_WITH_SOURCE
315 overlap = explicit
57 overlap = explicit
30 overlap = explicit
558 overlap = explicit

Every main-tree node classified = YES
Every passive effect line classified = YES
UNKNOWN semantic rows = 0
效果行只能落入：
CONSUMED
BLOCKED_BY_DOMAIN
SPECIAL_INTERACTION
STRUCTURAL
另外必须给出：
ProdSim reads allocated passive state = YES/NO
ProdSim output/hash depends on passive modifier result = YES/NO
ProdSim mastery-sensitive = YES/NO
不能猜。
证据
EditMode
新增：
S6PassiveCensusTests
至少断言：
node IDs 唯一；
分类总数对账；
category overlap 对账；
effect-line classification 无 UNKNOWN；
parser result 与 consumer inventory 可关联。
PlayMode
不要求新增行为测试；现有全部 PASS。
ProdSim
本 WO 零行为变化：
Expected =
FNV1A64:9a4c9524d0b3e214
必须 unchanged。
风险 / 止损线
最大风险是为了把数字对上，擅自解释 
2429/2832。
止损：
源数据不能证明的关系写：
UNRESOLVED_WITH_SOURCE
绝不能靠：
2429 + xxx = 2832
猜。
S6P-WO-02 — Passive-Aware Production Simulation
为什么排第二
这是我认为你漏掉的最高优先级骨架项。
如果下一单开始修 Mastery，而 ProdSim 根本不覆盖被动：
Mastery 改坏也可能 hash 完全不变。
所以先让 deterministic gate 对新天赋域真正敏感。
改动面
域层： gameplay semantics NONE
绘制层： NONE
测试/工具：
Production Simulation scenario；
canonical passive loadout fixture；
hash payload；
deterministic report。
不得为了测试改 Passive runtime。
Canonical scenario 最低覆盖
建立固定、可读、稳定的 passive scenario，至少包含：
1. deterministic starting state
2. deterministic allocated-node ID sequence
3. at least one currently CONSUMED offensive passive
4. at least one currently CONSUMED defensive/stat passive
5. resulting player effective stats
6. allocated node IDs
7. resulting passive modifiers
暂时不要把错误的 Mastery default-first 行为写进 canonical baseline。
Mastery 留 WO-03。
完成口径
必须证明两个 mutation test：
A. 改变 canonical passive allocation
 → ProdSim hash MUST change

B. 恢复 allocation
 → hash MUST recover exactly
这比“跑三次一样”更重要，它证明 hash 确实看得到被动域。
然后跑至少三次：
Run1 == Run2 == Run3
建立新 canonical hash。
哈希规则
这里允许 hash 改变，理由不是 gameplay 变了，而是：
Production Simulation 的 canonical evidence surface 被扩大，终于把 passive build truth 纳入 hash。
因此：
S5 hash 9a4c... = predecessor reference
WO-02 establishes new passive-aware hash
证据
EditMode
S6PassiveProdSimTests
至少：
same state → same hash；
passive allocation delta → different hash；
effective-stat delta → reflected in canonical payload；
unordered collection iteration 不影响结果。
PlayMode
现有全部 PASS。
ProdSim
3× independent exact match。
风险 / 止损线
禁止：
把 UI state 加进 gameplay hash；
hash 整张 2429 节点 JSON；
hash texture/path；
hash dictionary iteration order；
为了得到稳定 hash 固定错误状态。
Hash 只纳入游戏真相。
S6P-WO-03 — Mastery Explicit Selection Correctness
为什么第三
这是当前已知的明确 gameplay correctness defect：
有多个 Mastery choice，但系统默认第一条自动生效。
这比 LOD、地图页、Craft UI 都优先。
改动面
域层
建议新增类似：
MasterySelection
nodeId → selectedEffectId
或项目现有数据风格的等价结构。
Session
allocation；
selection；
reset；
modifier aggregation。
绘制层
点击 Mastery 打开 bounded selection popup；
显示全部候选；
当前选择；
confirm/cancel。
测试
S6MasterySelectionTests
S6MasteryPlayModeTests（若真实输入链值得覆盖）。
完成口径
必须机械证明：
unselected mastery effect = NONE

allocate mastery
→ requires explicit selection

selected mastery
→ exactly one effect active

change selection
→ old effect removed
→ new effect active
→ never both

R reset
→ allocation cleared
→ mastery selection cleared

session reload/rebuild in same run
→ deterministic state
禁止：
selected = choices[0] when missing
UI 验收
不以“弹窗漂亮”为 Gate。
机械验：
popup rect 在 1080p/1440p 屏内；
N 个 choices → N 个 hit rect；
每个 choice ID 与官方数据 ID 一致；
点击坐标→确定性选中对应 effect；
Esc/cancel 不改变状态。
截图只是辅助 evidence。
ProdSim
WO-02 已经 passive-aware。
本 WO 应把一个 deterministic Mastery scenario 纳入 canonical payload。
因此：
预计 hash 合理改变。
完成后：
3× independent run exact match
→ establish new canonical hash
风险 / 止损线
最大的坑：
UI 选择了 B，但 aggregator 仍偷偷用 A。
所以 test 必须从：
click/selection state → effective modifier/stat
端到端验证。
S6P-WO-04 — Supported Passive Semantic Closure & No-Silent-Noop Guard
为什么排在 Mastery 后
现在才适合处理“531/2429 之外还有什么能马上支持”。
但这里不是实现冰冷、闪电、ES、召唤物、药剂等新域。
只处理：
现有 GAME-ZZZ runtime 已经有真实 consumer，但 parser/映射还漏掉的 passive effect。
这是“把已有发动机接完整”，不是“新造发动机”。
改动面
Parser
PoeStatParser
Passive intermediate representation
必要时补：
Stat
Operation
Value
Target
Condition
Tags
但不要做万能脚本系统。
Session/stat aggregation
只允许接已有 consumer。
UI
unsupported state 的 truthfulness。
最重要的新增规则
任何节点如果包含 runtime 无法真实兑现的 effect：
不得静默只吃其中支持的数字。
建议 eligibility：
all gameplay effect lines consumable
 → SUPPORTED

special interaction handled separately
 → SPECIAL

otherwise
 → UNSUPPORTED_CURRENTLY
对 
UNSUPPORTED_CURRENTLY：
明确视觉标记；
hover 显示当前工程未支持；
禁止花点分配。
这是比“继续把覆盖率做漂亮”重要得多的 correctness guard。
否则玩家会花点购买实际没有效果的节点。
完成口径
真正的 Gate 不是：
coverage >= 50%
而是：
Effect lines requiring ONLY already-existing engine semantics:

Parsed = 100%
Runtime-consumed = 100%

Partial semantic application = 0
Condition dropped = 0
Silent ignored allocated effects = 0
Dead new StatIds = 0
其它：
BLOCKED_BY_DOMAIN
可以很多，不算失败。
证据
EditMode
S6PassiveSemanticClosureTests
建议自动从 census fixture 参数化生成：
source text
→ parse
→ expected semantic
→ runtime consumer
并增加：
mixed supported/unsupported node cannot partially apply；
unsupported node allocation rejects without spending point；
exact/full-line parse；
condition cannot disappear。
PlayMode
至少一个真实角色：
allocate supported node
→ player effective stat changes

attempt unsupported node
→ points/state unchanged
ProdSim
因为真实 passive effects 会增加：
hash 预计再次改变。
重新：
3× exact repeat → new baseline
风险 / 止损线
最危险的是：
"10% X while Y"
被实现成：
+10% X always
发现当前 condition engine 不支持：
整条 BLOCKED
而不是删条件。
S6P-WO-05 — Passive Tree Overview LOD & Texture Residency
为什么第五
这项确实应该做，但它是：
高价值可用性工程，不是玩法语义前置。
前四单先确保：
点什么、选什么、算什么都是真的。
然后再解决“整树缩小时看不懂”。
改动面
绘制层
PoeTreeView
SliceHud passive surface
资产加载
PoE atlas/icon resolution/loading path。
测试
S6PassiveLodTests
不改域层。
LOD 合同
按实际屏幕投影大小切，而不是只看 zoom 值。
推荐三档：
LOD0 — Overview
普通节点：
dot
Notable：
larger ring
Keystone：
distinct symbol
Mastery：
distinct mastery marker
Allocated：
强高亮。
关闭：
full 512² icon；
cluster decorative underlay；
-细节 frame。
LOD1 — Mid
notable/mastery/keystone art；
ordinary simplified；
reduced cluster decoration。
LOD2 — Detail
完整：
icon；
frame；
hover；
labels/detail。
完成口径
机械：
At fit-all zoom:
ordinary full icons drawn = 0

Hit-test NodeId:
LOD0 == LOD1 == LOD2

World↔screen transform:
within tolerance

allocated-path identity:
identical across LOD
Headless Chrome 对拍
继续利用已有官方数据 renderer。
固定：
viewport
tree center
zoom
pan
输出：
overview；
mid；
detail。
自动比较：
node centers；
edge endpoints；
visible-class mask；
allocated path；
clipping。
不要求 Chrome 与 Unity 像素级美术一致。
要求 geometry/state oracle 一致。
显存/资源 Gate
先测：
tree closed resident textures
tree overview resident textures
tree detail resident textures
standalone icons loaded
atlas textures loaded
peak texture memory
目标：
LOD0 不因 2429 nodes 加载全部 full icons；
同一 texture 不重复实例化；
优先使用已有官方 atlas/UV；
tree close 后非必要动态资源可释放；
无 per-frame texture allocation。
ProdSim
纯绘制/资源改动：
必须保持 WO-04 建立的新 hash EXACT UNCHANGED。
风险 / 止损线
禁止借显存优化：
改官方节点坐标；
改 node ID；
改 allocation semantics；
改 modifier；
重采样/重写 canonical data。
候选清单重新分类
A. 撑玩法骨架
1. 专精选择交互
现在做，P0/P1。
不是普通 UX，是 correctness。
2. 词条映射
分两类：
现有语义补齐：现在做。
新增轴：现在不做。
冰冷、闪电、ES、格挡、压制、召唤物、图腾、战吼、异常、药剂、充能、吸取、DoT 等如果当前没有 runtime consumer，就是新 gameplay domain，不能靠扩大 parser 自行启动。
3. 药剂系统
玩法骨架，但需导演授权，不进无人值守队列。
槽位存在 ≠ 药剂系统已获授权。
4. 普通珠宝孔
构筑骨架，但属于新 Item/Passive interaction domain，需导演授权。
不进本轮。
5. Ascendancy
玩法骨架，但更晚。
它还需要：
class/ascendancy identity；
point acquisition；
allocation/reset；
special mechanics。
不是“把 558 nodes import 进来”就算完成。
6. Timeless Jewel
高级系统，最晚。
依赖：
普通 Jewel → 稳定 passive semantics → radius/transformation → deterministic seed
现在不做。
B. 可用性/工程质量
整树俯瞰可读性
不是纯装饰。
我会归类：
重要 usability infrastructure
排在 semantic correctness 后、页面 polish 前。
所以本轮 WO-05 做。
512² 图标显存
也不是单纯 polish。
属于：
production scalability
和 LOD 同单处理。
C. 表面打磨
Map 页新版
后置。
Craft 页新版
后置。
Craft 功能已经存在，当前 UI 丑不影响其 gameplay truth。
建议下一个 UI polish mini-cycle 再处理：
Map + Craft + scrollbar skin
Scrollbar skin
最低优先级。
D. 当前明确不做
R/T 空槽
保持空。
正确表现为：
EMPTY / RESERVED
而不是为了填 UI 新造两个 Skill。
新技能属于独立 content authorization。
四个药剂槽
在药剂系统获授权前：
明确表现为：
RESERVED / NO FLASK EQUIPPED
不要让它看起来像有一个可用药剂却点了没反应。
你漏掉、但更应该先做的三件事
① ProdSim 对 Passive 是否真的敏感
这是第一优先。
现在拥有 2429 节点的 gameplay domain 后，deterministic gate 必须真正覆盖它。
否则 FNV1A64 的绿灯是局部绿灯。
② Unsupported Passive 的“静默空效果”问题
比继续扩大 coverage 更严重。
如果玩家可以：
spend point
→ node lights up
→ runtime effect = none
这属于错误产品语义。
所以 WO-04 要建立：
不完整语义不得假装生效。
③ Passive allocation invariants
WO-01 必须顺带 census 当前是否已经严格保证：
valid start
connected allocation
no illegal jump
point accounting
duplicate allocation no-op
reset exact
Mastery prerequisite
如果其中任何一项目前没有 authoritative test，列为 WO-03/04 必补 regression。
这些是玩法骨架，不是树 UI。
哈希策略总表
WO
Gameplay truth
FNV1A64 规则
WO-01 Census
不变
必须保持 
9a4c...
WO-02 ProdSim passive coverage
gameplay 不变，但 hash surface 扩大
允许变化；3×重基线
WO-03 Mastery correctness
改变
预期变化；3×重基线
WO-04 semantic closure
改变
预期变化；3×重基线
WO-05 LOD/memory
不变
必须与 WO-04 baseline 完全一致
每次允许变 hash 的 WO，都必须在 Evidence 中同时给：
Before Hash
After Hash × 3
Exact Repeat = YES
Reason For Change
Changed Canonical Fields
Unexpected Hash Inputs = NONE
如果无人值守只能做三件
不是去做三张最显眼的页面，而是：
第一件
Passive Census + Passive-aware ProdSim
如果必须按 WO 严格算，就是 WO-01→WO-02 连续完成。
这是门禁基础。
第二件
Mastery Explicit Selection
消灭已知错误语义。
第三件
Existing-Domain Semantic Closure + No-Silent-Noop Guard
保证当前声称支持的被动都是真的，未支持的明确拒绝，而不是“亮了但没效果”。
整树 LOD 排第四。
Map/Craft 重皮、scrollbar 排得更后。
当前不进入无人值守队列的事项
Flask System → 需导演玩法授权
Regular Jewel → 需导演玩法授权
Timeless Jewel → 需导演玩法授权 + Jewel 前置
Ascendancy → 需导演玩法/Progression 授权
Cold/Lightning/ES… → 需导演逐域授权
New QWERT Skills → 需导演 Content 授权
Paid Assets → 需导演采购
ORK/TDE → 不在本轮
它们可以出现在 census/dependency report 中，但不得作为阻塞项，也不得自动实现。
当前立即释放的 Primary WO
S6P-WO-01 — Passive Truth Census & Gate Coverage Audit
完成后按上述序列自动进入：
WO-02 → WO-03 → WO-04 → WO-05
但仍坚持“一轮一令”：每张 Evidence Pack 先经过 Gate Review，上一张 ACCEPT 后才释放下一张。
本轮第一令不改 runtime，因此最关键的预期值仍是：
EditMode: ALL PASS
PlayMode: ALL PASS
ProdSim:
FNV1A64:9a4c9524d0b3e214
Drift: 0
Forbidden Expansion: PASS
这里最关键的调整就是：不要再把“词条映射覆盖率”本身当 KPI。 下一阶段应该追求的是“当前声称支持的节点 100% 真生效、当前不支持的节点 0% 假生效”，这个标准才适合后续 AI 无人值守持续扩张。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：你这次的约束很清楚：**不采购、不换框架、不等人、不做需要主观肉眼才能判定的工作。** 下一段我建议排 **5 张 WO**，主线是“先让门禁真正覆盖天赋域，再修天赋真相，再做表现优化”。药剂、珠宝、升华、新技能和真正的新 Stat 轴都先排除出无人值守队列。

:::writing{variant="document" id="73184" title="GAME-ZZZ 下一轮无人值守工作令序列"}
# GAME-ZZZ 下一轮：Passive Truth & Deterministic Build Backbone

## 总顺序

```text
WO-01 被动域 census + 门禁覆盖审计
 ↓
WO-02 ProdSim 纳入被动构筑真相
 ↓
WO-03 专精显式选择正确化
 ↓
WO-04 现有引擎语义覆盖闭环 + 禁止静默空效果
 ↓
WO-05 天赋树 Overview LOD + 图标显存治理
```

这个顺序的核心原因是：

> **先保证“以后改坏天赋，门禁能发现”，然后才继续改天赋。**

目前最大的潜在风险是：天赋域已经很大，但当前 ProdSim hash 仍是 `FNV1A64:9a4c9524d0b3e214`。如果 ProdSim 没实际覆盖被动分配/效果，那么“hash unchanged”对被动正确性几乎没有保护作用。

---

# S6P-WO-01 — Passive Truth Census & Gate Coverage Audit

### 为什么现在做

这是所有后续工作的 denominator。

现在同时存在：

- 2429 主树节点；
- 315 Mastery；
- 57 Jewel Socket；
- 30 special/Timeless 类节点；
- 558 Ascendancy；
- Director 口径 2832；
- 531 节点当前产出 modifier。

在没明确集合包含关系前，不能继续用“78% 未覆盖”指导代码工作。

同时必须先确认 ProdSim **究竟有没有消费被动状态**。

### 改动面

**域层：** NONE 
**绘制层：** NONE 
**测试/工具：**

- 新增 `S6PassiveCensusTests` 或同义 test family；
- census/report generator；
- ProdSim input/output coverage audit。

**文档：**

- passive census；
- semantic coverage matrix；
- ProdSim surface map。

### 完成口径

机械要求：

```text
Authoritative node sets reconciled = YES

2429 meaning = explicit
2832 meaning = explicit or UNRESOLVED_WITH_SOURCE
315 overlap = explicit
57 overlap = explicit
30 overlap = explicit
558 overlap = explicit

Every main-tree node classified = YES
Every passive effect line classified = YES
UNKNOWN semantic rows = 0
```

效果行只能落入：

```text
CONSUMED
BLOCKED_BY_DOMAIN
SPECIAL_INTERACTION
STRUCTURAL
```

另外必须给出：

```text
ProdSim reads allocated passive state = YES/NO
ProdSim output/hash depends on passive modifier result = YES/NO
ProdSim mastery-sensitive = YES/NO
```

不能猜。

### 证据

**EditMode**

新增：

`S6PassiveCensusTests`

至少断言：

- node IDs 唯一；
- 分类总数对账；
- category overlap 对账；
- effect-line classification 无 UNKNOWN；
- parser result 与 consumer inventory 可关联。

**PlayMode**

不要求新增行为测试；现有全部 PASS。

**ProdSim**

本 WO 零行为变化：

```text
Expected =
FNV1A64:9a4c9524d0b3e214
```

必须 unchanged。

### 风险 / 止损线

最大风险是为了把数字对上，擅自解释 `2429/2832`。

**止损：**

源数据不能证明的关系写：

`UNRESOLVED_WITH_SOURCE`

绝不能靠：

`2429 + xxx = 2832`

猜。

---

# S6P-WO-02 — Passive-Aware Production Simulation

### 为什么排第二

这是我认为你漏掉的**最高优先级骨架项**。

如果下一单开始修 Mastery，而 ProdSim 根本不覆盖被动：

> Mastery 改坏也可能 hash 完全不变。

所以先让 deterministic gate 对新天赋域真正敏感。

### 改动面

**域层：** gameplay semantics NONE 
**绘制层：** NONE 
**测试/工具：**

- Production Simulation scenario；
- canonical passive loadout fixture；
- hash payload；
- deterministic report。

不得为了测试改 Passive runtime。

### Canonical scenario 最低覆盖

建立固定、可读、稳定的 passive scenario，至少包含：

```text
1. deterministic starting state
2. deterministic allocated-node ID sequence
3. at least one currently CONSUMED offensive passive
4. at least one currently CONSUMED defensive/stat passive
5. resulting player effective stats
6. allocated node IDs
7. resulting passive modifiers
```

暂时**不要把错误的 Mastery default-first 行为写进 canonical baseline**。

Mastery 留 WO-03。

### 完成口径

必须证明两个 mutation test：

```text
A. 改变 canonical passive allocation
 → ProdSim hash MUST change

B. 恢复 allocation
 → hash MUST recover exactly
```

这比“跑三次一样”更重要，它证明 hash 确实看得到被动域。

然后跑至少三次：

```text
Run1 == Run2 == Run3
```

建立新 canonical hash。

### 哈希规则

这里允许 hash 改变，理由不是 gameplay 变了，而是：

> **Production Simulation 的 canonical evidence surface 被扩大，终于把 passive build truth 纳入 hash。**

因此：

```text
S5 hash 9a4c... = predecessor reference
WO-02 establishes new passive-aware hash
```

### 证据

**EditMode**

`S6PassiveProdSimTests`

至少：

- same state → same hash；
- passive allocation delta → different hash；
- effective-stat delta → reflected in canonical payload；
- unordered collection iteration 不影响结果。

**PlayMode**

现有全部 PASS。

**ProdSim**

3× independent exact match。

### 风险 / 止损线

禁止：

- 把 UI state 加进 gameplay hash；
- hash 整张 2429 节点 JSON；
- hash texture/path；
- hash dictionary iteration order；
- 为了得到稳定 hash 固定错误状态。

Hash 只纳入**游戏真相**。

---

# S6P-WO-03 — Mastery Explicit Selection Correctness

### 为什么第三

这是当前已知的明确 gameplay correctness defect：

> 有多个 Mastery choice，但系统默认第一条自动生效。

这比 LOD、地图页、Craft UI 都优先。

### 改动面

**域层**

建议新增类似：

```text
MasterySelection
nodeId → selectedEffectId
```

或项目现有数据风格的等价结构。

**Session**

- allocation；
- selection；
- reset；
- modifier aggregation。

**绘制层**

- 点击 Mastery 打开 bounded selection popup；
- 显示全部候选；
- 当前选择；
- confirm/cancel。

**测试**

`S6MasterySelectionTests`

`S6MasteryPlayModeTests`（若真实输入链值得覆盖）。

### 完成口径

必须机械证明：

```text
unselected mastery effect = NONE

allocate mastery
→ requires explicit selection

selected mastery
→ exactly one effect active

change selection
→ old effect removed
→ new effect active
→ never both

R reset
→ allocation cleared
→ mastery selection cleared

session reload/rebuild in same run
→ deterministic state
```

禁止：

```text
selected = choices[0] when missing
```

### UI 验收

不以“弹窗漂亮”为 Gate。

机械验：

- popup rect 在 1080p/1440p 屏内；
- N 个 choices → N 个 hit rect；
- 每个 choice ID 与官方数据 ID 一致；
- 点击坐标→确定性选中对应 effect；
- Esc/cancel 不改变状态。

截图只是辅助 evidence。

### ProdSim

WO-02 已经 passive-aware。

本 WO 应把一个 deterministic Mastery scenario 纳入 canonical payload。

因此：

**预计 hash 合理改变。**

完成后：

```text
3× independent run exact match
→ establish new canonical hash
```

### 风险 / 止损线

最大的坑：

> UI 选择了 B，但 aggregator 仍偷偷用 A。

所以 test 必须从：

`click/selection state → effective modifier/stat`

端到端验证。

---

# S6P-WO-04 — Supported Passive Semantic Closure & No-Silent-Noop Guard

### 为什么排在 Mastery 后

现在才适合处理“531/2429 之外还有什么能马上支持”。

但这里**不是实现冰冷、闪电、ES、召唤物、药剂等新域**。

只处理：

> **现有 GAME-ZZZ runtime 已经有真实 consumer，但 parser/映射还漏掉的 passive effect。**

这是“把已有发动机接完整”，不是“新造发动机”。

### 改动面

**Parser**

`PoeStatParser`

**Passive intermediate representation**

必要时补：

```text
Stat
Operation
Value
Target
Condition
Tags
```

但不要做万能脚本系统。

**Session/stat aggregation**

只允许接已有 consumer。

**UI**

unsupported state 的 truthfulness。

### 最重要的新增规则

任何节点如果包含 runtime 无法真实兑现的 effect：

**不得静默只吃其中支持的数字。**

建议 eligibility：

```text
all gameplay effect lines consumable
 → SUPPORTED

special interaction handled separately
 → SPECIAL

otherwise
 → UNSUPPORTED_CURRENTLY
```

对 `UNSUPPORTED_CURRENTLY`：

- 明确视觉标记；
- hover 显示当前工程未支持；
- **禁止花点分配**。

这是比“继续把覆盖率做漂亮”重要得多的 correctness guard。

否则玩家会花点购买实际没有效果的节点。

### 完成口径

真正的 Gate 不是：

`coverage >= 50%`

而是：

```text
Effect lines requiring ONLY already-existing engine semantics:

Parsed = 100%
Runtime-consumed = 100%

Partial semantic application = 0
Condition dropped = 0
Silent ignored allocated effects = 0
Dead new StatIds = 0
```

其它：

```text
BLOCKED_BY_DOMAIN
```

可以很多，不算失败。

### 证据

**EditMode**

`S6PassiveSemanticClosureTests`

建议自动从 census fixture 参数化生成：

```text
source text
→ parse
→ expected semantic
→ runtime consumer
```

并增加：

- mixed supported/unsupported node cannot partially apply；
- unsupported node allocation rejects without spending point；
- exact/full-line parse；
- condition cannot disappear。

**PlayMode**

至少一个真实角色：

```text
allocate supported node
→ player effective stat changes

attempt unsupported node
→ points/state unchanged
```

### ProdSim

因为真实 passive effects 会增加：

**hash 预计再次改变。**

重新：

`3× exact repeat → new baseline`

### 风险 / 止损线

最危险的是：

```text
"10% X while Y"
```

被实现成：

```text
+10% X always
```

发现当前 condition engine 不支持：

**整条 BLOCKED**

而不是删条件。

---

# S6P-WO-05 — Passive Tree Overview LOD & Texture Residency

### 为什么第五

这项确实应该做，但它是：

**高价值可用性工程，不是玩法语义前置。**

前四单先确保：

> 点什么、选什么、算什么都是真的。

然后再解决“整树缩小时看不懂”。

### 改动面

**绘制层**

`PoeTreeView`

`SliceHud` passive surface

**资产加载**

PoE atlas/icon resolution/loading path。

**测试**

`S6PassiveLodTests`

**不改域层。**

### LOD 合同

按**实际屏幕投影大小**切，而不是只看 zoom 值。

推荐三档：

#### LOD0 — Overview

普通节点：

`dot`

Notable：

`larger ring`

Keystone：

`distinct symbol`

Mastery：

`distinct mastery marker`

Allocated：

强高亮。

关闭：

- full 512² icon；
- cluster decorative underlay；
-细节 frame。

#### LOD1 — Mid

- notable/mastery/keystone art；
- ordinary simplified；
- reduced cluster decoration。

#### LOD2 — Detail

完整：

- icon；
- frame；
- hover；
- labels/detail。

### 完成口径

机械：

```text
At fit-all zoom:
ordinary full icons drawn = 0

Hit-test NodeId:
LOD0 == LOD1 == LOD2

World↔screen transform:
within tolerance

allocated-path identity:
identical across LOD
```

### Headless Chrome 对拍

继续利用已有官方数据 renderer。

固定：

```text
viewport
tree center
zoom
pan
```

输出：

- overview；
- mid；
- detail。

自动比较：

- node centers；
- edge endpoints；
- visible-class mask；
- allocated path；
- clipping。

**不要求 Chrome 与 Unity 像素级美术一致。**

要求 geometry/state oracle 一致。

### 显存/资源 Gate

先测：

```text
tree closed resident textures
tree overview resident textures
tree detail resident textures
standalone icons loaded
atlas textures loaded
peak texture memory
```

目标：

- LOD0 不因 2429 nodes 加载全部 full icons；
- 同一 texture 不重复实例化；
- 优先使用已有官方 atlas/UV；
- tree close 后非必要动态资源可释放；
- 无 per-frame texture allocation。

### ProdSim

纯绘制/资源改动：

**必须保持 WO-04 建立的新 hash EXACT UNCHANGED。**

### 风险 / 止损线

禁止借显存优化：

- 改官方节点坐标；
- 改 node ID；
- 改 allocation semantics；
- 改 modifier；
- 重采样/重写 canonical data。

---

# 候选清单重新分类

## A. 撑玩法骨架

### 1. 专精选择交互
**现在做，P0/P1。**

不是普通 UX，是 correctness。

### 2. 词条映射
分两类：

**现有语义补齐：现在做。**

**新增轴：现在不做。**

冰冷、闪电、ES、格挡、压制、召唤物、图腾、战吼、异常、药剂、充能、吸取、DoT 等如果当前没有 runtime consumer，就是**新 gameplay domain**，不能靠扩大 parser 自行启动。

### 3. 药剂系统
**玩法骨架，但需导演授权，不进无人值守队列。**

槽位存在 ≠ 药剂系统已获授权。

### 4. 普通珠宝孔
**构筑骨架，但属于新 Item/Passive interaction domain，需导演授权。**

不进本轮。

### 5. Ascendancy
**玩法骨架，但更晚。**

它还需要：

- class/ascendancy identity；
- point acquisition；
- allocation/reset；
- special mechanics。

不是“把 558 nodes import 进来”就算完成。

### 6. Timeless Jewel
**高级系统，最晚。**

依赖：

`普通 Jewel → 稳定 passive semantics → radius/transformation → deterministic seed`

现在不做。

---

# B. 可用性/工程质量

### 整树俯瞰可读性

不是纯装饰。

我会归类：

**重要 usability infrastructure**

排在 semantic correctness 后、页面 polish 前。

所以本轮 WO-05 做。

### 512² 图标显存

也不是单纯 polish。

属于：

**production scalability**

和 LOD 同单处理。

---

# C. 表面打磨

### Map 页新版

后置。

### Craft 页新版

后置。

Craft 功能已经存在，当前 UI 丑不影响其 gameplay truth。

建议下一个 UI polish mini-cycle 再处理：

```text
Map + Craft + scrollbar skin
```

### Scrollbar skin

最低优先级。

---

# D. 当前明确不做

### R/T 空槽

**保持空。**

正确表现为：

```text
EMPTY / RESERVED
```

而不是为了填 UI 新造两个 Skill。

新技能属于独立 content authorization。

### 四个药剂槽

在药剂系统获授权前：

明确表现为：

```text
RESERVED / NO FLASK EQUIPPED
```

不要让它看起来像有一个可用药剂却点了没反应。

---

# 你漏掉、但更应该先做的三件事

## ① ProdSim 对 Passive 是否真的敏感

这是第一优先。

现在拥有 2429 节点的 gameplay domain 后，deterministic gate 必须真正覆盖它。

否则 FNV1A64 的绿灯是局部绿灯。

---

## ② Unsupported Passive 的“静默空效果”问题

比继续扩大 coverage 更严重。

如果玩家可以：

```text
spend point
→ node lights up
→ runtime effect = none
```

这属于错误产品语义。

所以 WO-04 要建立：

**不完整语义不得假装生效。**

---

## ③ Passive allocation invariants

WO-01 必须顺带 census 当前是否已经严格保证：

```text
valid start
connected allocation
no illegal jump
point accounting
duplicate allocation no-op
reset exact
Mastery prerequisite
```

如果其中任何一项目前没有 authoritative test，列为 WO-03/04 必补 regression。

这些是玩法骨架，不是树 UI。

---

# 哈希策略总表

| WO | Gameplay truth | FNV1A64 规则 |
|---|---|---|
| WO-01 Census | 不变 | **必须保持 `9a4c...`** |
| WO-02 ProdSim passive coverage | gameplay 不变，但 hash surface 扩大 | **允许变化；3×重基线** |
| WO-03 Mastery correctness | 改变 | **预期变化；3×重基线** |
| WO-04 semantic closure | 改变 | **预期变化；3×重基线** |
| WO-05 LOD/memory | 不变 | **必须与 WO-04 baseline 完全一致** |

每次允许变 hash 的 WO，都必须在 Evidence 中同时给：

```text
Before Hash
After Hash × 3
Exact Repeat = YES
Reason For Change
Changed Canonical Fields
Unexpected Hash Inputs = NONE
```

---

# 如果无人值守只能做三件

不是去做三张最显眼的页面，而是：

### 第一件
**Passive Census + Passive-aware ProdSim**

如果必须按 WO 严格算，就是 WO-01→WO-02 连续完成。

这是门禁基础。

### 第二件
**Mastery Explicit Selection**

消灭已知错误语义。

### 第三件
**Existing-Domain Semantic Closure + No-Silent-Noop Guard**

保证当前声称支持的被动都是真的，未支持的明确拒绝，而不是“亮了但没效果”。

整树 LOD 排第四。

Map/Craft 重皮、scrollbar 排得更后。

---

# 当前不进入无人值守队列的事项

```text
Flask System → 需导演玩法授权
Regular Jewel → 需导演玩法授权
Timeless Jewel → 需导演玩法授权 + Jewel 前置
Ascendancy → 需导演玩法/Progression 授权
Cold/Lightning/ES… → 需导演逐域授权
New QWERT Skills → 需导演 Content 授权
Paid Assets → 需导演采购
ORK/TDE → 不在本轮
```

它们可以出现在 census/dependency report 中，但**不得作为阻塞项，也不得自动实现**。

---

# 当前立即释放的 Primary WO

**`S6P-WO-01 — Passive Truth Census & Gate Coverage Audit`**

完成后按上述序列自动进入：

`WO-02 → WO-03 → WO-04 → WO-05`

但仍坚持“一轮一令”：**每张 Evidence Pack 先经过 Gate Review，上一张 ACCEPT 后才释放下一张。**

本轮第一令不改 runtime，因此最关键的预期值仍是：

```text
EditMode: ALL PASS
PlayMode: ALL PASS
ProdSim:
FNV1A64:9a4c9524d0b3e214
Drift: 0
Forbidden Expansion: PASS
```
:::

这里最关键的调整就是：**不要再把“词条映射覆盖率”本身当 KPI。** 下一阶段应该追求的是“当前声称支持的节点 100% 真生效、当前不支持的节点 0% 假生效”，这个标准才适合后续 AI 无人值守持续扩张。
 

 

 

 

 
 
 
 

 日夜切换
 
 
 

 
 
 
 
预设提示词
 

 

 

 
 
 
 
 
 
 
 

 导出对话
 
 
 
 
 

 
 
 
 
 
要求更改
CtrlK
标题 1
要求更改
CtrlK
标题 1
要求更改
CtrlK
标题 1
要求更改
CtrlK
文本