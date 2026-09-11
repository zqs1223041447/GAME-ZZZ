# S6P-WO-04A2 — Passive Traversal / Effect Separation · 规划 AI 执行合同（原文）

| 项 | 值 |
|---|---|
| 令号 | S6P-WO-04A2 |
| 状态 | RELEASED / EXECUTE NOW |
| 权威 | Channel A |
| 渠道 | 话题 \game-zzz-planning-2\ / 会话 aa368c5-7478-83ea-a2d3-95003ee4e6ab\（**新会话**，2026-09-11 建立，取代旧会话 \game-zzz-planning\ / aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d\） |
| 基准真值 | 本地 HEAD \ce6f85c\（已推送 origin/main） |
| 前置 | S6P-DIR-01 Gate Addendum COMPLETE |
| 后继 | S6P-WO-03 — **BLOCKED pending Channel A Gate Review ACCEPT** |
| canonical ProdSim | \FNV1A64:ec1d3ed67d3035d0\ |
| 页面 | https://leopard-x.memofun.net/c/6aa368c5-7478-83ea-a2d3-95003ee4e6ab |

> 以下为规划 AI 回复原文（未经改写，仅作落库）。执行记录见 \S6P_WO_04A2.md\；入口指纹见 \_wo04a2_fingerprint_entry.txt\。
> 回复中的排版偶有换行错位（镜像站渲染所致），语义不受影响。

---

确认。Channel A 当前唯一 RELEASED 队列不变：
S6P-WO-04A2 → S6P-WO-03 → S6P-WO-04B → [S6P-WO-04C 条件触发] → S6P-WO-05
不新增前置工作令。WO-04A2 内置强制只读 Preflight；WO-03 在 WO-04A2 Evidence Pack 经我 Gate Review = ACCEPT 之前不得启动。
下面是正式执行合同。
S6P-WO-04A2 — Passive Traversal / Effect Separation 执行合同
S6P-WO-04A2 — Passive Traversal / Effect Separation
Status: 
RELEASED / EXECUTE NOW
Authority: Channel A
Execution mode: Unattended / 一轮一令
Base truth: 2026-09-11 local HEAD 
ce6f85c
Predecessor: 
S6P-DIR-01 Gate Addendum COMPLETE
Successor: 
S6P-WO-03 — BLOCKED pending Channel A Gate Review ACCEPT
Canonical ProdSim: 
FNV1A64:ec1d3ed67d3035d0
1. Mission
本令只解决一个架构错误：
“节点能否作为被动树路径”与“节点承诺的 gameplay effect 是否已完整兑现”必须成为两个互相独立的真值。
不得再以「effect supported」布尔值同时决定 traversal 与 modifier application。
完成后：
普通、当前未兑现的节点可以作为合法路径节点；
它们可以正常花费被动点、进入已选择节点集合、继续连接后续节点；
但必须产生 0 gameplay modifier；
mixed node 仍执行 whole-node fail-closed：一个 modifier 都不得漏出；
Mastery / Jewel Socket / Timeless/special 不得借此解锁；
04A 的 effect consumption gate 不得削弱；
WO-03 的 Mastery explicit selection 职权不得提前实现。
本令不是「增加被动效果支持率」工作令。
2. 强制 Preflight Gate
任何生产代码修改之前，先执行本节。全部 PASS 才准继续。
PF-1 — Base identity
记录：
git HEAD；
working tree 状态；
Unity version；
当前分支。
期望基准为 
ce6f85c，或仅包含本令治理/合同落库而无其它产品代码变化的直接后继。
若发现其它工作令的未审代码、不可解释 dirty state 或并行产品修改：
STOP。不得自行吸收。
PF-2 — Existing gates
重新验证当前基线：
Compile: 
0 error
EditMode: 
462/462
PlayMode: 
17/17
此处是入口基准。实施后允许因为新增测试使测试总数增加，不要求最终仍等于 462/17；但所有原有测试必须继续 PASS。
PF-3 — 04A frozen census
必须复核并记录：
fully supported = 367
blocked = 1660
no-handler special = 87
mastery transition = 315
本令不得改变这四个 04A 冻结数字。
任一不符：
STOP。不得“顺手修 census”。
PF-4 — Legacy reachability
用当前、修改前的 traversal 行为复核：
StartNode = 
2172
reachable = 14 / 2429
必须使用当前真实 passive-tree graph，不得通过屏幕坐标、orbit、cluster geometry 或其它视觉信息推导边。
无法重现 
14 / 2429：
STOP。
PF-5 — Canonical ProdSim entry
冷进程执行 canonical ProdSim ×3。
三次都必须：
FNV1A64:ec1d3ed67d3035d0
且 EXACT MATCH。
任何一次不一致：
STOP；不得实施；不得 rebaseline。
PF-6 — Route-only recovery fixture discovery
按 §7 的确定性算法寻找至少一个满足条件的真实 supported target：
修改前不能从 
2172 到达；
在「普通节点允许 traversal、special 仍阻断」的参考图中可以到达；
确定性最短路径至少包含一个 
EffectTruth=UNFULFILLED 的普通节点。
候选集合为空：
STOP。
不得为了让本令 PASS 而把 Mastery/Jewel/Timeless 改成 traversable。
3. Truth ownership
唯一 owner 继续是：
Assets/Runtime/Core/Gameplay/PassiveSupport.cs
允许在该 owner 内新增/整理枚举、结果结构或查询 API，但不得建立第二个 truth oracle。
逻辑上必须暴露两个独立维度：
EffectTruth
 FULLY_SUPPORTED
 UNFULFILLED
 SPECIAL_PENDING

TraversalTruth
 TRAVERSABLE
 SPECIAL_BLOCKED
 OUT_OF_DOMAIN
可以用一个不可变 result/record 同时返回二者，但：
不得再存在一个 
IsSupported / 
CanAllocate / 等价 bool 同时承担二者语义。
UI、allocator、census、tests、ProdSim 等消费者必须消费 
PassiveSupport 的 canonical truth，而不是复制 parser/节点类型判断建立自己的 oracle。
4. EffectTruth 精确定义
FULLY_SUPPORTED
表示该普通节点当前承诺的 gameplay effect 已由 04A runtime truth 判定为完整可兑现。
要求：
节点全部 effect 均在现有 runtime 支持域；
不含 unsupported remainder；
不含 mixed partially-supported 状态；
allocation 后允许现有 effect consumption path 应用其完整 modifier 集。
这是 effect 能力真值，不是 connectivity 真值。
UNFULFILLED
表示普通节点存在当前 runtime 无法完整兑现的 gameplay promise。
包括：
完全 unsupported 普通节点；
mixed node；
其它已经由 04A 判定为 blocked 的普通节点。
语义必须是：
节点可以用于 traversal，但本节点的 gameplay modifier 集合整体为空。
尤其 mixed node：
即使其中 4 条 modifier 有 3 条可支持、1 条不支持，也必须：
EffectTruth = UNFULFILLED
并且：
applied modifiers = 0
禁止只应用那 3 条已支持 modifier。
SPECIAL_PENDING
表示该节点属于需要其它专门机制才能正确兑现的特殊语义，本令不对其 gameplay effect 作普通节点式兑现。
它不是“部分支持”，也不是“未来一定会实现”的承诺。
Mastery / Jewel / Timeless/special 在本令下均属于此类。
5. TraversalTruth 精确定义
TRAVERSABLE
节点属于当前普通 passive-tree traversal 域。
当满足现有正常 allocation 前提时，它：
可以被分配；
花费现有普通节点应花费的被动点；
写入 selected-passive identity；
可以成为后续邻接节点的合法路径；
traversal 不取决于 
EffectTruth 是 
FULLY_SUPPORTED 还是 
UNFULFILLED。
因此：
TRAVERSABLE + FULLY_SUPPORTED
是普通有效节点。
而：
TRAVERSABLE + UNFULFILLED
就是本令所谓 route-only node。
SPECIAL_BLOCKED
节点位于 passive tree 数据中，但其 traversal/allocation 需要本令禁止涉及的特殊机制。
本令至少包括：
Mastery；
Jewel Socket；
Timeless / recognized special node。
行为：
不可分配；
不可充当 graph transit；
不得写入 selected set；
不得扣被动点；
不得产生 effect。
OUT_OF_DOMAIN
仅表示已有结构事实认定该节点不属于当前普通 passive allocation domain。
例如已有 runtime 本来就排除的 Ascendancy/domain-specific 节点，可继续属于此值。
严格约束：
不得因为节点 effect 不支持，就把普通节点标成 OUT_OF_DOMAIN。
普通 canonical passive node 若既非现有 out-of-domain，也非明确 special，必须是 
TRAVERSABLE。
本令不得发明新的 out-of-domain 规则来规避 reachability gate。
6. Mandatory truth matrix
Node kind
EffectTruth
TraversalTruth
Allocation
Point delta
Gameplay modifiers
Ordinary fully supported
FULLY_SUPPORTED
TRAVERSABLE
allow under normal rules
normal cost
full existing effect
Ordinary fully unsupported/blocked
UNFULFILLED
TRAVERSABLE
allow under normal rules
normal cost
0
Ordinary mixed node
UNFULFILLED
TRAVERSABLE
allow under normal rules
normal cost
0, whole node
Mastery
SPECIAL_PENDING
SPECIAL_BLOCKED
reject
0
0
Jewel Socket
SPECIAL_PENDING
SPECIAL_BLOCKED
reject
0
0
Timeless / recognized special
SPECIAL_PENDING
SPECIAL_BLOCKED
reject
0
0
Existing structural out-of-domain node
SPECIAL_PENDING
OUT_OF_DOMAIN
reject
0
0
StartNode=2172 是 reachability 的固定 seed。不得因为本令改变其现有起始节点/初始分配语义；reachability 测量始终从它开始。
7. Reachability Gate
7.1 Canonical edges
唯一允许的 graph edge：
passive-tree canonical 数据中、当前 allocation system 所消费的真实 node-to-node connection。
对 connectivity 测量将连接归一化为无向 node pair。
严禁使用：
screen/world coordinates；
orbit relation；
group/cluster membership；
visual line renderer；
几何距离；
“看起来相邻”推断。
7.2 Reference graph
定义 
G_ref：
seed 为 node 
2172；
包含所有当前普通、in-domain passive nodes；
普通节点无论 EffectTruth 为何都可作为 vertex；
删除 
SPECIAL_BLOCKED；
删除 
OUT_OF_DOMAIN；
edges 只取 §7.1 canonical edges。
从 
2172 在 
G_ref 上 BFS，得到：
R_ref
定义 supported denominator：
D =
所有同时满足：
1. node ∈ R_ref
2. ordinary node
3. EffectTruth == FULLY_SUPPORTED
的节点集合
这就是：
start-connected ordinary supported nodes
的唯一正式定义。
7.3 Runtime graph
用生产 
PassiveSupport.TraversalTruth 建 
G_runtime：
只有：
TraversalTruth == TRAVERSABLE
的节点可作为 transit vertex，另包含固定 seed 
2172。
从 
2172 BFS 得：
R_runtime
验收公式：
|D ∩ R_runtime| / |D| == 1.000000
并且：
D - R_runtime == ∅
即：
100% start-connected ordinary supported nodes 可达。
99.x%、比 14 大很多、绝大部分恢复，都不是 PASS。
8. Frozen route-only fixture
由于当前本地节点数据未包含在本合同输入中，不允许凭空猜一个 target ID。
因此 fixture 必须在 PF-6 自动、确定性地产生，然后在本令内永久冻结。
8.1 Legacy reference reach
定义：
R_legacy
为从 
2172 出发、使用旧行为语义构造的参考 reach：
ordinary 
FULLY_SUPPORTED 可以作为 transit；
ordinary 
UNFULFILLED 不可作为 transit；
special/out-of-domain 不可 transit。
生产代码不必保留旧实现；测试可用纯 reference traversal 计算此集合。
8.2 Candidate target
候选：
C = {
 t |
 t ∈ D
 AND t ∉ R_legacy
 AND shortest_path_G_ref(2172, t)
 至少包含一个 ordinary UNFULFILLED 节点
}
必须 
|C| >= 1。
8.3 Deterministic selection
所有 BFS neighbor 按 
nodeId ascending 遍历。
从 
C 中按以下 key 升序选择唯一 target：
(shortestDistanceFrom2172, nodeId)
得到：
FrozenTarget04A2
再使用同一 ordered BFS 规则得到唯一 canonical shortest path：
P = [2172, ..., FrozenTarget04A2]
立即冻结并写入 Evidence Pack / fixture test：
exact target node ID；
exact ordered path node IDs；
path length；
path 中全部 
UNFULFILLED + TRAVERSABLE node IDs；
第一个 route-only node ID。
后续测试不得每次“动态换一个容易 PASS 的 target”。
数据未变而 fixture ID/path 发生变化：
FAIL。
8.4 Required fixture proof
必须同时证明：
FrozenTarget04A2 ∉ R_legacy
FrozenTarget04A2 ∈ R_runtime
且 frozen path 上至少一个 node：
EffectTruth == UNFULFILLED
TraversalTruth == TRAVERSABLE
9. Allocation semantics
route-only node 不是免费桥，也不是 ghost edge。
对于普通：
UNFULFILLED + TRAVERSABLE
节点：
必须遵守现有 adjacency/connectivity rule；
必须遵守现有 passive-point affordability；
成功 allocation 后扣正常 passive-point cost；
必须进入 selected passive set；
必须能解锁其相邻的其它 
TRAVERSABLE 节点；
必须产生 0 modifiers；
deallocation/respec 若现有系统已有语义，则保持现有行为；本令不得另造 respec 系统。
Traversal separation 不得成为 bypass：
非邻接普通节点仍不能凭空分配；
点数不足仍不能分配；
blocked special 仍不能分配。
10. 04A consumption gate preservation
04A 的 fail-closed gameplay consumption gate 必须原样保留。
Effect application 必须等价于：
EffectTruth == FULLY_SUPPORTED
 => consume existing complete modifier payload

anything else
 => zero modifiers
禁止：
traversal 成功后绕过 effect gate；
直接从 raw parser result 应用 modifier；
mixed node 的 supported subset 泄漏；
UI/allocator 自行判断“这一条 modifier 看起来支持，所以应用”。
本令修改的是 path permission，不是 effect permission。
11. Required tests
测试名字可适应现有项目风格，但语义覆盖不得缺失。
Positive controls
T-P01 — OrdinarySupportedTruth
真实普通 fully-supported fixture：
EffectTruth == FULLY_SUPPORTED
TraversalTruth == TRAVERSABLE
T-P02 — OrdinaryBlockedBecomesRouteOnly
真实普通 blocked fixture：
EffectTruth == UNFULFILLED
TraversalTruth == TRAVERSABLE
并验证它可以在满足正常邻接/点数条件时被 allocation。
T-P03 — RouteOnlyCostsPoint
成功分配一个 route-only node：
selected set 增加该 node；
passive points 减少正常 cost；
modifier set 不变。
T-P04 — RouteOnlyUnlocksTraversal
使用真实 path 证明：
route-only node → downstream traversable node
确实形成合法 connectivity。
T-P05 — MixedWholeNodeZero
至少一个真实 mixed fixture：
UNFULFILLED + TRAVERSABLE
allocation 成功；
即使其中存在原本可识别 modifier，最终应用 modifier 数仍为 0。
T-P06 — FrozenRecoveryFixture
使用 §8 冻结的 exact fixture：
legacy reference 下 target unreachable；
新 traversal 下 target reachable；
exact frozen path 经过 route-only node。
T-P07 — Reachability100Percent
计算 §7：
denominator 
|D|；
reached 
|D ∩ R_runtime|；
missing IDs。
必须：
missing == []
ratio == 100%
T-P08 — SupportedEffectUnaffected
普通 fully-supported fixture 在改造前后的 gameplay modifier 结果一致。
不得因为 traversal 重构改变其效果数值。
Negative controls
T-N01 — MasteryRejectZeroCost
真实 Mastery fixture：
SPECIAL_PENDING
SPECIAL_BLOCKED
allocation reject；
selected set unchanged；
passive points delta = 
0；
modifiers delta = 
0。
node 
10 Life Mastery 可继续作为已登记的 Mastery fixture 候选；若当前数据确认仍为该真实 Mastery，则优先使用它。
T-N02 — JewelSocketRejectZeroCost
真实 Jewel Socket：
reject；
zero point deduction；
zero selection mutation；
zero effect。
T-N03 — TimelessSpecialRejectZeroCost
真实 Timeless/recognized special：
同上。
T-N04 — OutOfDomainRejectZeroCost
若当前 canonical dataset 中存在已有 out-of-domain fixture：
reject；
zero points；
zero selected-state mutation。
若数据集中不存在，不得虚构 fixture；Evidence Pack 明确记为 
N/A — no canonical fixture，并用 classification unit test 覆盖分支。
T-N05 — NonAdjacentCannotBypass
一个普通 
TRAVERSABLE 节点若尚未满足既有 adjacency requirement：
allocation 仍必须 reject。
本令不能把“TRAVERSABLE”解释成“任意地点都能点”。
T-N06 — InsufficientPointsCannotBypass
点数不足：
allocation reject。
T-N07 — MixedNoPartialLeak
对 mixed node 明确断言：
任何 individually-supported modifier 均不得出现在 runtime modifier output。
12. Census invariants
WO-04A2 不得改变 effect-support census。
出口仍须报告原 04A 冻结值：
supported 367
blocked 1660
no-handler special 87
mastery transition 315
同时新增或输出 
EffectTruth × TraversalTruth cross-tab 供审计。
不得为了得到漂亮 traversal 数字而：
把 blocked 改成 supported；
把普通 blocked 改成 special；
从 census 隐藏节点；
改 parser；
修改支持判定。
367 / 315 的第一次合法变化只能来自后续明确获批、且显式说明 census delta 的工作令；尤其 WO-03 对 Mastery 的变化不得提前落入本令。
13. ProdSim Gate
13.1 Canonical exit
实施完成后冷进程 canonical ProdSim ×3。
三次必须全部：
FNV1A64:ec1d3ed67d3035d0
EXACT MATCH。
若 canonical hash 发生任何变化
立即：
STOP / INVESTIGATE
必须：
不修改 expected hash；
不生成新 canonical baseline；
不用“这是预期变化”自行放行；
不继续 WO-03；
Evidence Pack 报告首次 divergence；
至少区分 
ps| / pm / pe / pk 哪一 section 首先不同。
只有 Channel A 可决定是否 rebaseline。
14. Mandatory ProdSim sensitivity proof
除 canonical run 外，必须加入一个非 canonical sensitivity test。
使用 §8 frozen path 的：
FirstRouteOnlyNode
记为 
r。
构造两个完全确定的 simulation states：
State A
从 
2172 合法分配 frozen path 上 
r 之前的相同 prefix，不分配 
r。
State B
与 A 完全相同，但再合法分配 
r。
除该 allocation 外：
seed 相同；
equipment 相同；
skill/support state 相同；
enemy state 相同；
frame/tick schedule 相同；
其它 passive selection 相同。
必须证明：
ps|(A) != ps|(B)
即 passive-selection identity 对 route-only allocation 敏感。
同时必须证明：
pm(A) == pm(B)
pe(A) == pe(B)
pk(A) == pk(B)
要求 EXACT MATCH，而非近似数值相等。
意义：
route-only node 必须改变“我点了哪些被动”的身份，但不得改变 gameplay outcome。
若 
pm/pe/pk 任意变化：
FAIL。
如果 
ps| 不变化：
FAIL。
该 sensitivity scenario 的 hash 不得被写成新的 canonical baseline。
15. Acceptance Criteria
AC-1 — Preflight baseline
PF-1..PF-6 全部 PASS，有原始证据。
AC-2 — Two-dimensional truth exists
runtime canonical owner 明确暴露 
EffectTruth 与 
TraversalTruth 两个独立维度。
AC-3 — Single owner
二者均由现有 
PassiveSupport owner 产出；无第二套 parser/classifier/oracle。
AC-4 — No shared boolean authority
不存在一个 support bool 同时决定 traversal 与 effect application。
AC-5 — Supported ordinary mapping
真实普通 supported node = 
FULLY_SUPPORTED + TRAVERSABLE。
AC-6 — Blocked ordinary mapping
真实普通 blocked node = 
UNFULFILLED + TRAVERSABLE。
AC-7 — Mixed mapping
真实 mixed node = 
UNFULFILLED + TRAVERSABLE，并验证 whole-node zero modifier。
AC-8 — Route-only allocation
合法 route-only allocation 成功、正常扣点、进入 selected set、可继续连接。
AC-9 — Effect gate preserved
任何 
EffectTruth != FULLY_SUPPORTED 节点产生零 modifier。
AC-10 — Mastery remains blocked
Mastery = 
SPECIAL_PENDING + SPECIAL_BLOCKED，reject + zero point deduction。
AC-11 — Jewel remains blocked
Jewel Socket = 
SPECIAL_PENDING + SPECIAL_BLOCKED，reject + zero point deduction。
AC-12 — Timeless/special remains blocked
Timeless/recognized special = 
SPECIAL_PENDING + SPECIAL_BLOCKED，reject + zero point deduction。
AC-13 — No traversal bypass
Non-adjacent 与 insufficient-point negative controls 继续 reject。
AC-14 — Frozen recovery fixture
存在并冻结一个真实 supported target，其 legacy unreachable、新 runtime reachable，且 path 确实经过 route-only node。
AC-15 — Reachability gate
从 
StartNode=2172，按 §7 定义：
start-connected ordinary supported nodes = 100% reachable。
Missing set 必须为空。
AC-16 — Frozen 04A census
367 / 1660 / 87 / 315 四个数字均不变。
AC-17 — Regression suites
Compile 
0 error；全部 EditMode PASS；全部 PlayMode PASS；所有原有测试无 regression。
AC-18 — Canonical ProdSim
出口 cold ×3 均为：
FNV1A64:ec1d3ed67d3035d0
AC-19 — ProdSim sensitivity
route-only allocation：
ps| 必须变化；
pm/pe/pk 必须全部 EXACT MATCH。
AC-20 — Scope discipline
无禁止域变化、无隐式 rebaseline、无 WO-03 functionality 偷跑。
AC-1..AC-20 必须全部满足才可请求 ACCEPT。
不存在“19/20 基本通过”。
16. Explicit prohibitions
本令明确禁止：
新 
StatId；
新 
ModOp；
新 parser rule；
扩张 modifier vocabulary；
新元素伤害轴；
Mastery selector；
Mastery choice application；
Jewel Socket gameplay；
Jewel insertion；
Jewel radius；
Timeless mechanism；
Ascendancy gameplay；
Flask gameplay；
新 skill；
第三 projectile/skill connection group；
save/load schema；
persistence；
LOD；
framework replacement；
package/library purchase；
asset purchase；
数据源替换；
canonical ProdSim rebaseline；
利用视觉坐标制造 tree connectivity；
为提高数字而重新定义 supported；
对 mixed node 应用部分 modifier。
允许新增的仅限：
traversal/effect truth separation 所需 runtime type/API；
allocator 对新 
TraversalTruth 的消费；
census/test/evidence 所需观测；
必要的 UI 状态表达，但不得引入新玩法或新选择机制。
17. WO-03 职权边界
WO-04A2 对所有 Mastery 的最终立场只有：
EffectTruth = SPECIAL_PENDING
TraversalTruth = SPECIAL_BLOCKED
WO-04A2 只准识别并拒绝 Mastery。
以下事项专属于 
S6P-WO-03，本令禁止实现：
Mastery explicit selection；
Mastery effect/choice picker；
“未选择”与“已选择某 choice”状态区别；
Mastery choice ID/state；
Mastery choice 对 gameplay modifier 的应用；
Mastery selection 对 
ps| identity/hash 的语义；
node 10 Life Mastery 的 explicit-choice 产品实现；
根据已选 choice 将某 Mastery 从 pending 转为可兑现；
因 WO-03 实现造成的 
315 mastery-transition census 更新；
WO-03 已冻结的 hash sensitivity fallback：
未选择 vs 显式选择。
即使当前已有 22 个 mastery choice 可完整兑现，也不得在 WO-04A2 直接应用。
这 22 个 choice 只是 WO-03 的前置事实，不是本令的授权。
Jewel / Timeless 也不会因为 WO-03 而自动获得授权。
18. Evidence Pack — Mandatory fields
Evidence Pack 至少必须包含以下内容，缺项不得提交 Gate Review。
A. Identity
Work Order: 
S6P-WO-04A2
status: 
READY FOR GATE REVIEW
base commit
implementation commit
git status
Unity version
execution timestamp
changed-file list
不得自行写 
ACCEPTED。
B. Preflight
原样记录：
Compile result
EditMode 
462/462
PlayMode 
17/17
04A census 
367 / 1660 / 87 / 315
legacy reachability 
14 / 2429
ProdSim cold entry hashes ×3
PF-6 candidate count
C. Truth model
列出：
EffectTruth API；
TraversalTruth API；
canonical owner；
consumer locations；
mandatory truth matrix；
EffectTruth × TraversalTruth complete count cross-tab。
D. Oracle audit
说明所有 traversal/effect consumers 如何从 
PassiveSupport 获取 truth。
附搜索结果证明没有新增平行 parser/classifier。
若保留旧 helper，必须说明它为什么不是 authoritative oracle。
E. Frozen fixture
必须给出精确：
StartNode: 2172
TargetNode: <exact id>
Path: [2172, ..., target]
PathLength: <n>
RouteOnlyNodesOnPath: [...]
FirstRouteOnlyNode: <id>
并给：
legacy reachable(target): false
new reachable(target): true
F. Reachability measurement
报告：
canonical node count；
canonical edge count；
ordinary in-domain node count；
special-blocked count；
out-of-domain count；
|R_legacy|；
|R_ref|；
|D|；
|D ∩ R_runtime|；
percentage；
missing node IDs。
验收时 missing 必须：
[]
G. Effect isolation
至少给出：
ordinary supported fixture；
ordinary blocked route-only fixture；
mixed fixture；
before/after modifier outputs；
mixed node zero-modifier proof。
H. Negative controls
分别记录：
Mastery node ID；
Jewel Socket node ID；
Timeless/special node ID；
optional out-of-domain fixture；
每项必须列：
allocation result
selected-set before/after
passive points before/after
modifier output before/after
special reject 的 points delta 必须为 
0。
I. Test evidence
提供所有新增 test names 和结果，以及完整：
Compile result
EditMode total/pass/fail
PlayMode total/pass/fail
不得以“覆盖率提升”作为完成依据。
J. ProdSim canonical
列出 exit cold-process ×3 的三个实际 hashes。
必须全部：
FNV1A64:ec1d3ed67d3035d0
K. ProdSim sensitivity
给出 State A / State B：
exact selected-node delta；
route-only node ID；
ps| A/B value 或 digest；
pm comparison；
pe comparison；
pk comparison。
必须明确写：
ps: DIFFERENT
pm: EXACT MATCH
pe: EXACT MATCH
pk: EXACT MATCH
L. Census preservation
再次列出出口：
367 / 1660 / 87 / 315
并明确：
NO SILENT CENSUS UPDATE
M. Scope audit
声明并提供 diff/search 证据：
no new StatId
no new ModOp
no parser expansion
no Mastery selector
no Jewel gameplay
no Timeless gameplay
no Ascendancy gameplay
no new skill
no new elemental axis
no third connection group
no persistence/save
no LOD
no framework/package/asset purchase
no rebaseline
N. Governance handoff
更新 
开发计划/UNATTENDED_STATE.md，内容必须反映：
S6P-WO-04A2 = READY_FOR_GATE_REVIEW
S6P-WO-03 = BLOCKED_PENDING_CHANNEL_A_ACCEPT
不得自动开始 WO-03。
19. Automatic STOP conditions
出现以下任一情形立即停止产品修改，保留失败证据并提交 Channel A，不自行找替代方案：
入口 census 不等于 
367 / 1660 / 87 / 315；
legacy reachability 不能重现 
14 / 2429；
canonical entry hash 不是 
ec1d3ed67d3035d0；
找不到真实 route-only recovery fixture；
必须修改 parser/StatId/ModOp 才能完成；
必须使 Mastery/Jewel/Timeless traversable 才能达到 reachability gate；
route-only node 出现任何 gameplay modifier；
mixed node 泄漏任意部分 modifier；
Mastery/Jewel/Timeless reject 时发生扣点或 selected-state mutation；
§7 reachability 
< 100%；
04A census 任一数字变化；
canonical ProdSim exit hash 任一变化；
sensitivity 
ps| 不变化；
sensitivity 
pm/pe/pk 任一变化；
发现 traversal 与 effect 仍实际依赖同一个 authority bool；
需要越过本令禁止域才能“修好”。
STOP 后：
不 rebaseline；
不扩大范围；
不继续 WO-03；
Evidence Pack 标 
BLOCKED / NEEDS ARBITRATION；
报告最小复现、实际值、期望值及 first divergence。
20. Completion rule
本令完成的判据不是“树更能点了”，而是同时证明：
普通节点的路径资格
 与
普通节点的 effect 兑现资格
 已真正分离

+

所有 start-connected ordinary supported nodes
 100% reachable

+

所有 ordinary unsupported/mixed route-only nodes
 0 gameplay effect

+

Mastery/Jewel/Timeless
 仍然 fail-closed

+

canonical ProdSim
 完全不变
完成实现后只提交 S6P-WO-04A2 Evidence Pack 给 Channel A。
在 Channel A 返回：
Gate Review = ACCEPT
之前，S6P-WO-03 不得执行。
这份令里唯一需要在执行时“生成后冻结”的东西，是那个真实 recovery target 的具体 ID/path；选择算法已经封死，因此不会给执行器留下人工挑样本或 cherry-pick 的空间。除此之外，没有必要在 WO-04A2 前再插任何令。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：确认。**Channel A 当前唯一 RELEASED 队列不变：**

**S6P-WO-04A2 → S6P-WO-03 → S6P-WO-04B → [S6P-WO-04C 条件触发] → S6P-WO-05**

不新增前置工作令。WO-04A2 内置强制只读 Preflight；WO-03 在 WO-04A2 Evidence Pack 经我 Gate Review = ACCEPT 之前不得启动。

下面是正式执行合同。

:::writing{variant="document" id="58341" title="S6P-WO-04A2 — Passive Traversal / Effect Separation 执行合同"}
# S6P-WO-04A2 — Passive Traversal / Effect Separation

**Status:** `RELEASED / EXECUTE NOW` 
**Authority:** Channel A 
**Execution mode:** Unattended / 一轮一令 
**Base truth:** 2026-09-11 local HEAD `ce6f85c` 
**Predecessor:** `S6P-DIR-01 Gate Addendum` COMPLETE 
**Successor:** `S6P-WO-03` — BLOCKED pending Channel A Gate Review ACCEPT 
**Canonical ProdSim:** `FNV1A64:ec1d3ed67d3035d0`

---

## 1. Mission

本令只解决一个架构错误：

> **“节点能否作为被动树路径”与“节点承诺的 gameplay effect 是否已完整兑现”必须成为两个互相独立的真值。**

不得再以「effect supported」布尔值同时决定 traversal 与 modifier application。

完成后：

- 普通、当前未兑现的节点可以作为合法路径节点；
- 它们可以正常花费被动点、进入已选择节点集合、继续连接后续节点；
- 但必须产生 **0 gameplay modifier**；
- mixed node 仍执行 whole-node fail-closed：**一个 modifier 都不得漏出**；
- Mastery / Jewel Socket / Timeless/special 不得借此解锁；
- 04A 的 effect consumption gate 不得削弱；
- WO-03 的 Mastery explicit selection 职权不得提前实现。

本令不是「增加被动效果支持率」工作令。

---

# 2. 强制 Preflight Gate

任何生产代码修改之前，先执行本节。全部 PASS 才准继续。

### PF-1 — Base identity

记录：

- git HEAD；
- working tree 状态；
- Unity version；
- 当前分支。

期望基准为 `ce6f85c`，或仅包含本令治理/合同落库而无其它产品代码变化的直接后继。

若发现其它工作令的未审代码、不可解释 dirty state 或并行产品修改：

**STOP。不得自行吸收。**

### PF-2 — Existing gates

重新验证当前基线：

- Compile: `0 error`
- EditMode: `462/462`
- PlayMode: `17/17`

此处是入口基准。实施后允许因为新增测试使测试总数增加，不要求最终仍等于 462/17；但所有原有测试必须继续 PASS。

### PF-3 — 04A frozen census

必须复核并记录：

- fully supported = **367**
- blocked = **1660**
- no-handler special = **87**
- mastery transition = **315**

本令不得改变这四个 04A 冻结数字。

任一不符：

**STOP。不得“顺手修 census”。**

### PF-4 — Legacy reachability

用当前、修改前的 traversal 行为复核：

- StartNode = `2172`
- reachable = **14 / 2429**

必须使用当前真实 passive-tree graph，不得通过屏幕坐标、orbit、cluster geometry 或其它视觉信息推导边。

无法重现 `14 / 2429`：

**STOP。**

### PF-5 — Canonical ProdSim entry

冷进程执行 canonical ProdSim ×3。

三次都必须：

`FNV1A64:ec1d3ed67d3035d0`

且 EXACT MATCH。

任何一次不一致：

**STOP；不得实施；不得 rebaseline。**

### PF-6 — Route-only recovery fixture discovery

按 §7 的确定性算法寻找至少一个满足条件的真实 supported target：

1. 修改前不能从 `2172` 到达；
2. 在「普通节点允许 traversal、special 仍阻断」的参考图中可以到达；
3. 确定性最短路径至少包含一个 `EffectTruth=UNFULFILLED` 的普通节点。

候选集合为空：

**STOP。**

不得为了让本令 PASS 而把 Mastery/Jewel/Timeless 改成 traversable。

---

# 3. Truth ownership

唯一 owner 继续是：

`Assets/Runtime/Core/Gameplay/PassiveSupport.cs`

允许在该 owner 内新增/整理枚举、结果结构或查询 API，但不得建立第二个 truth oracle。

逻辑上必须暴露两个独立维度：

```text
EffectTruth
 FULLY_SUPPORTED
 UNFULFILLED
 SPECIAL_PENDING

TraversalTruth
 TRAVERSABLE
 SPECIAL_BLOCKED
 OUT_OF_DOMAIN
```

可以用一个不可变 result/record 同时返回二者，但：

**不得再存在一个 `IsSupported` / `CanAllocate` / 等价 bool 同时承担二者语义。**

UI、allocator、census、tests、ProdSim 等消费者必须消费 `PassiveSupport` 的 canonical truth，而不是复制 parser/节点类型判断建立自己的 oracle。

---

# 4. EffectTruth 精确定义

## `FULLY_SUPPORTED`

表示该普通节点当前承诺的 gameplay effect 已由 04A runtime truth 判定为**完整可兑现**。

要求：

- 节点全部 effect 均在现有 runtime 支持域；
- 不含 unsupported remainder；
- 不含 mixed partially-supported 状态；
- allocation 后允许现有 effect consumption path 应用其完整 modifier 集。

这是 **effect 能力真值**，不是 connectivity 真值。

## `UNFULFILLED`

表示普通节点存在当前 runtime 无法完整兑现的 gameplay promise。

包括：

- 完全 unsupported 普通节点；
- **mixed node**；
- 其它已经由 04A 判定为 blocked 的普通节点。

语义必须是：

> 节点可以用于 traversal，但本节点的 gameplay modifier 集合整体为空。

尤其 mixed node：

即使其中 4 条 modifier 有 3 条可支持、1 条不支持，也必须：

`EffectTruth = UNFULFILLED`

并且：

`applied modifiers = 0`

禁止只应用那 3 条已支持 modifier。

## `SPECIAL_PENDING`

表示该节点属于需要其它专门机制才能正确兑现的特殊语义，本令不对其 gameplay effect 作普通节点式兑现。

它不是“部分支持”，也不是“未来一定会实现”的承诺。

Mastery / Jewel / Timeless/special 在本令下均属于此类。

---

# 5. TraversalTruth 精确定义

## `TRAVERSABLE`

节点属于当前普通 passive-tree traversal 域。

当满足现有正常 allocation 前提时，它：

- 可以被分配；
- 花费现有普通节点应花费的被动点；
- 写入 selected-passive identity；
- 可以成为后续邻接节点的合法路径；
- traversal 不取决于 `EffectTruth` 是 `FULLY_SUPPORTED` 还是 `UNFULFILLED`。

因此：

```text
TRAVERSABLE + FULLY_SUPPORTED
```

是普通有效节点。

而：

```text
TRAVERSABLE + UNFULFILLED
```

就是本令所谓 **route-only node**。

## `SPECIAL_BLOCKED`

节点位于 passive tree 数据中，但其 traversal/allocation 需要本令禁止涉及的特殊机制。

本令至少包括：

- Mastery；
- Jewel Socket；
- Timeless / recognized special node。

行为：

- 不可分配；
- 不可充当 graph transit；
- 不得写入 selected set；
- 不得扣被动点；
- 不得产生 effect。

## `OUT_OF_DOMAIN`

仅表示已有结构事实认定该节点不属于当前普通 passive allocation domain。

例如已有 runtime 本来就排除的 Ascendancy/domain-specific 节点，可继续属于此值。

严格约束：

> **不得因为节点 effect 不支持，就把普通节点标成 OUT_OF_DOMAIN。**

普通 canonical passive node 若既非现有 out-of-domain，也非明确 special，必须是 `TRAVERSABLE`。

本令不得发明新的 out-of-domain 规则来规避 reachability gate。

---

# 6. Mandatory truth matrix

| Node kind | EffectTruth | TraversalTruth | Allocation | Point delta | Gameplay modifiers |
|---|---|---|---|---:|---:|
| Ordinary fully supported | `FULLY_SUPPORTED` | `TRAVERSABLE` | allow under normal rules | normal cost | full existing effect |
| Ordinary fully unsupported/blocked | `UNFULFILLED` | `TRAVERSABLE` | allow under normal rules | normal cost | **0** |
| Ordinary mixed node | `UNFULFILLED` | `TRAVERSABLE` | allow under normal rules | normal cost | **0, whole node** |
| Mastery | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | reject | **0** | **0** |
| Jewel Socket | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | reject | **0** | **0** |
| Timeless / recognized special | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | reject | **0** | **0** |
| Existing structural out-of-domain node | `SPECIAL_PENDING` | `OUT_OF_DOMAIN` | reject | **0** | **0** |

`StartNode=2172` 是 reachability 的固定 seed。不得因为本令改变其现有起始节点/初始分配语义；reachability 测量始终从它开始。

---

# 7. Reachability Gate

## 7.1 Canonical edges

唯一允许的 graph edge：

> passive-tree canonical 数据中、当前 allocation system 所消费的真实 node-to-node connection。

对 connectivity 测量将连接归一化为无向 node pair。

严禁使用：

- screen/world coordinates；
- orbit relation；
- group/cluster membership；
- visual line renderer；
- 几何距离；
- “看起来相邻”推断。

## 7.2 Reference graph

定义 `G_ref`：

- seed 为 node `2172`；
- 包含所有当前普通、in-domain passive nodes；
- 普通节点无论 EffectTruth 为何都可作为 vertex；
- 删除 `SPECIAL_BLOCKED`；
- 删除 `OUT_OF_DOMAIN`；
- edges 只取 §7.1 canonical edges。

从 `2172` 在 `G_ref` 上 BFS，得到：

`R_ref`

定义 supported denominator：

```text
D =
所有同时满足：
1. node ∈ R_ref
2. ordinary node
3. EffectTruth == FULLY_SUPPORTED
的节点集合
```

这就是：

**start-connected ordinary supported nodes**

的唯一正式定义。

## 7.3 Runtime graph

用生产 `PassiveSupport.TraversalTruth` 建 `G_runtime`：

只有：

`TraversalTruth == TRAVERSABLE`

的节点可作为 transit vertex，另包含固定 seed `2172`。

从 `2172` BFS 得：

`R_runtime`

验收公式：

```text
|D ∩ R_runtime| / |D| == 1.000000
```

并且：

```text
D - R_runtime == ∅
```

即：

**100% start-connected ordinary supported nodes 可达。**

`99.x%`、比 14 大很多、绝大部分恢复，都不是 PASS。

---

# 8. Frozen route-only fixture

由于当前本地节点数据未包含在本合同输入中，不允许凭空猜一个 target ID。

因此 fixture 必须在 PF-6 自动、确定性地产生，然后在本令内永久冻结。

## 8.1 Legacy reference reach

定义：

`R_legacy`

为从 `2172` 出发、使用旧行为语义构造的参考 reach：

- ordinary `FULLY_SUPPORTED` 可以作为 transit；
- ordinary `UNFULFILLED` 不可作为 transit；
- special/out-of-domain 不可 transit。

生产代码不必保留旧实现；测试可用纯 reference traversal 计算此集合。

## 8.2 Candidate target

候选：

```text
C = {
 t |
 t ∈ D
 AND t ∉ R_legacy
 AND shortest_path_G_ref(2172, t)
 至少包含一个 ordinary UNFULFILLED 节点
}
```

必须 `|C| >= 1`。

## 8.3 Deterministic selection

所有 BFS neighbor 按 `nodeId ascending` 遍历。

从 `C` 中按以下 key 升序选择唯一 target：

```text
(shortestDistanceFrom2172, nodeId)
```

得到：

`FrozenTarget04A2`

再使用同一 ordered BFS 规则得到唯一 canonical shortest path：

```text
P = [2172, ..., FrozenTarget04A2]
```

立即冻结并写入 Evidence Pack / fixture test：

- exact target node ID；
- exact ordered path node IDs；
- path length；
- path 中全部 `UNFULFILLED + TRAVERSABLE` node IDs；
- 第一个 route-only node ID。

后续测试不得每次“动态换一个容易 PASS 的 target”。

数据未变而 fixture ID/path 发生变化：

**FAIL。**

## 8.4 Required fixture proof

必须同时证明：

```text
FrozenTarget04A2 ∉ R_legacy
FrozenTarget04A2 ∈ R_runtime
```

且 frozen path 上至少一个 node：

```text
EffectTruth == UNFULFILLED
TraversalTruth == TRAVERSABLE
```

---

# 9. Allocation semantics

route-only node 不是免费桥，也不是 ghost edge。

对于普通：

`UNFULFILLED + TRAVERSABLE`

节点：

1. 必须遵守现有 adjacency/connectivity rule；
2. 必须遵守现有 passive-point affordability；
3. 成功 allocation 后扣正常 passive-point cost；
4. 必须进入 selected passive set；
5. 必须能解锁其相邻的其它 `TRAVERSABLE` 节点；
6. 必须产生 **0 modifiers**；
7. deallocation/respec 若现有系统已有语义，则保持现有行为；本令不得另造 respec 系统。

Traversal separation **不得**成为 bypass：

- 非邻接普通节点仍不能凭空分配；
- 点数不足仍不能分配；
- blocked special 仍不能分配。

---

# 10. 04A consumption gate preservation

04A 的 fail-closed gameplay consumption gate 必须原样保留。

Effect application 必须等价于：

```text
EffectTruth == FULLY_SUPPORTED
 => consume existing complete modifier payload

anything else
 => zero modifiers
```

禁止：

- traversal 成功后绕过 effect gate；
- 直接从 raw parser result 应用 modifier；
- mixed node 的 supported subset 泄漏；
- UI/allocator 自行判断“这一条 modifier 看起来支持，所以应用”。

本令修改的是 **path permission**，不是 **effect permission**。

---

# 11. Required tests

测试名字可适应现有项目风格，但语义覆盖不得缺失。

## Positive controls

### T-P01 — OrdinarySupportedTruth

真实普通 fully-supported fixture：

```text
EffectTruth == FULLY_SUPPORTED
TraversalTruth == TRAVERSABLE
```

### T-P02 — OrdinaryBlockedBecomesRouteOnly

真实普通 blocked fixture：

```text
EffectTruth == UNFULFILLED
TraversalTruth == TRAVERSABLE
```

并验证它可以在满足正常邻接/点数条件时被 allocation。

### T-P03 — RouteOnlyCostsPoint

成功分配一个 route-only node：

- selected set 增加该 node；
- passive points 减少正常 cost；
- modifier set 不变。

### T-P04 — RouteOnlyUnlocksTraversal

使用真实 path 证明：

`route-only node → downstream traversable node`

确实形成合法 connectivity。

### T-P05 — MixedWholeNodeZero

至少一个真实 mixed fixture：

- `UNFULFILLED + TRAVERSABLE`
- allocation 成功；
- 即使其中存在原本可识别 modifier，最终应用 modifier 数仍为 **0**。

### T-P06 — FrozenRecoveryFixture

使用 §8 冻结的 exact fixture：

- legacy reference 下 target unreachable；
- 新 traversal 下 target reachable；
- exact frozen path 经过 route-only node。

### T-P07 — Reachability100Percent

计算 §7：

- denominator `|D|`；
- reached `|D ∩ R_runtime|`；
- missing IDs。

必须：

```text
missing == []
ratio == 100%
```

### T-P08 — SupportedEffectUnaffected

普通 fully-supported fixture 在改造前后的 gameplay modifier 结果一致。

不得因为 traversal 重构改变其效果数值。

---

## Negative controls

### T-N01 — MasteryRejectZeroCost

真实 Mastery fixture：

- `SPECIAL_PENDING`
- `SPECIAL_BLOCKED`
- allocation reject；
- selected set unchanged；
- passive points delta = `0`；
- modifiers delta = `0`。

node `10 Life Mastery` 可继续作为已登记的 Mastery fixture 候选；若当前数据确认仍为该真实 Mastery，则优先使用它。

### T-N02 — JewelSocketRejectZeroCost

真实 Jewel Socket：

- reject；
- zero point deduction；
- zero selection mutation；
- zero effect。

### T-N03 — TimelessSpecialRejectZeroCost

真实 Timeless/recognized special：

同上。

### T-N04 — OutOfDomainRejectZeroCost

若当前 canonical dataset 中存在已有 out-of-domain fixture：

- reject；
- zero points；
- zero selected-state mutation。

若数据集中不存在，不得虚构 fixture；Evidence Pack 明确记为 `N/A — no canonical fixture`，并用 classification unit test 覆盖分支。

### T-N05 — NonAdjacentCannotBypass

一个普通 `TRAVERSABLE` 节点若尚未满足既有 adjacency requirement：

allocation 仍必须 reject。

本令不能把“TRAVERSABLE”解释成“任意地点都能点”。

### T-N06 — InsufficientPointsCannotBypass

点数不足：

allocation reject。

### T-N07 — MixedNoPartialLeak

对 mixed node 明确断言：

任何 individually-supported modifier 均不得出现在 runtime modifier output。

---

# 12. Census invariants

WO-04A2 不得改变 effect-support census。

出口仍须报告原 04A 冻结值：

```text
supported 367
blocked 1660
no-handler special 87
mastery transition 315
```

同时新增或输出 `EffectTruth × TraversalTruth` cross-tab 供审计。

不得为了得到漂亮 traversal 数字而：

- 把 blocked 改成 supported；
- 把普通 blocked 改成 special；
- 从 census 隐藏节点；
- 改 parser；
- 修改支持判定。

**367 / 315 的第一次合法变化只能来自后续明确获批、且显式说明 census delta 的工作令；尤其 WO-03 对 Mastery 的变化不得提前落入本令。**

---

# 13. ProdSim Gate

## 13.1 Canonical exit

实施完成后冷进程 canonical ProdSim ×3。

三次必须全部：

`FNV1A64:ec1d3ed67d3035d0`

EXACT MATCH。

### 若 canonical hash 发生任何变化

立即：

**STOP / INVESTIGATE**

必须：

- 不修改 expected hash；
- 不生成新 canonical baseline；
- 不用“这是预期变化”自行放行；
- 不继续 WO-03；
- Evidence Pack 报告首次 divergence；
- 至少区分 `ps| / pm / pe / pk` 哪一 section 首先不同。

只有 Channel A 可决定是否 rebaseline。

---

# 14. Mandatory ProdSim sensitivity proof

除 canonical run 外，必须加入一个**非 canonical sensitivity test**。

使用 §8 frozen path 的：

`FirstRouteOnlyNode`

记为 `r`。

构造两个完全确定的 simulation states：

### State A

从 `2172` 合法分配 frozen path 上 `r` 之前的相同 prefix，不分配 `r`。

### State B

与 A 完全相同，但再合法分配 `r`。

除该 allocation 外：

- seed 相同；
- equipment 相同；
- skill/support state 相同；
- enemy state 相同；
- frame/tick schedule 相同；
- 其它 passive selection 相同。

必须证明：

```text
ps|(A) != ps|(B)
```

即 passive-selection identity 对 route-only allocation **敏感**。

同时必须证明：

```text
pm(A) == pm(B)
pe(A) == pe(B)
pk(A) == pk(B)
```

要求 EXACT MATCH，而非近似数值相等。

意义：

> route-only node 必须改变“我点了哪些被动”的身份，但不得改变 gameplay outcome。

若 `pm/pe/pk` 任意变化：

**FAIL。**

如果 `ps|` 不变化：

**FAIL。**

该 sensitivity scenario 的 hash 不得被写成新的 canonical baseline。

---

# 15. Acceptance Criteria

### AC-1 — Preflight baseline
PF-1..PF-6 全部 PASS，有原始证据。

### AC-2 — Two-dimensional truth exists
runtime canonical owner 明确暴露 `EffectTruth` 与 `TraversalTruth` 两个独立维度。

### AC-3 — Single owner
二者均由现有 `PassiveSupport` owner 产出；无第二套 parser/classifier/oracle。

### AC-4 — No shared boolean authority
不存在一个 support bool 同时决定 traversal 与 effect application。

### AC-5 — Supported ordinary mapping
真实普通 supported node = `FULLY_SUPPORTED + TRAVERSABLE`。

### AC-6 — Blocked ordinary mapping
真实普通 blocked node = `UNFULFILLED + TRAVERSABLE`。

### AC-7 — Mixed mapping
真实 mixed node = `UNFULFILLED + TRAVERSABLE`，并验证 whole-node zero modifier。

### AC-8 — Route-only allocation
合法 route-only allocation 成功、正常扣点、进入 selected set、可继续连接。

### AC-9 — Effect gate preserved
任何 `EffectTruth != FULLY_SUPPORTED` 节点产生零 modifier。

### AC-10 — Mastery remains blocked
Mastery = `SPECIAL_PENDING + SPECIAL_BLOCKED`，reject + zero point deduction。

### AC-11 — Jewel remains blocked
Jewel Socket = `SPECIAL_PENDING + SPECIAL_BLOCKED`，reject + zero point deduction。

### AC-12 — Timeless/special remains blocked
Timeless/recognized special = `SPECIAL_PENDING + SPECIAL_BLOCKED`，reject + zero point deduction。

### AC-13 — No traversal bypass
Non-adjacent 与 insufficient-point negative controls 继续 reject。

### AC-14 — Frozen recovery fixture
存在并冻结一个真实 supported target，其 legacy unreachable、新 runtime reachable，且 path 确实经过 route-only node。

### AC-15 — Reachability gate
从 `StartNode=2172`，按 §7 定义：

**start-connected ordinary supported nodes = 100% reachable。**

Missing set 必须为空。

### AC-16 — Frozen 04A census
`367 / 1660 / 87 / 315` 四个数字均不变。

### AC-17 — Regression suites
Compile `0 error`；全部 EditMode PASS；全部 PlayMode PASS；所有原有测试无 regression。

### AC-18 — Canonical ProdSim
出口 cold ×3 均为：

`FNV1A64:ec1d3ed67d3035d0`

### AC-19 — ProdSim sensitivity
route-only allocation：

- `ps|` 必须变化；
- `pm/pe/pk` 必须全部 EXACT MATCH。

### AC-20 — Scope discipline
无禁止域变化、无隐式 rebaseline、无 WO-03 functionality 偷跑。

**AC-1..AC-20 必须全部满足才可请求 ACCEPT。**

不存在“19/20 基本通过”。

---

# 16. Explicit prohibitions

本令明确禁止：

- 新 `StatId`；
- 新 `ModOp`；
- 新 parser rule；
- 扩张 modifier vocabulary；
- 新元素伤害轴；
- Mastery selector；
- Mastery choice application；
- Jewel Socket gameplay；
- Jewel insertion；
- Jewel radius；
- Timeless mechanism；
- Ascendancy gameplay；
- Flask gameplay；
- 新 skill；
- 第三 projectile/skill connection group；
- save/load schema；
- persistence；
- LOD；
- framework replacement；
- package/library purchase；
- asset purchase；
- 数据源替换；
- canonical ProdSim rebaseline；
- 利用视觉坐标制造 tree connectivity；
- 为提高数字而重新定义 supported；
- 对 mixed node 应用部分 modifier。

允许新增的仅限：

- traversal/effect truth separation 所需 runtime type/API；
- allocator 对新 `TraversalTruth` 的消费；
- census/test/evidence 所需观测；
- 必要的 UI 状态表达，但不得引入新玩法或新选择机制。

---

# 17. WO-03 职权边界

WO-04A2 对所有 Mastery 的最终立场只有：

```text
EffectTruth = SPECIAL_PENDING
TraversalTruth = SPECIAL_BLOCKED
```

WO-04A2 **只准识别并拒绝 Mastery**。

以下事项专属于 `S6P-WO-03`，本令禁止实现：

1. Mastery explicit selection；
2. Mastery effect/choice picker；
3. “未选择”与“已选择某 choice”状态区别；
4. Mastery choice ID/state；
5. Mastery choice 对 gameplay modifier 的应用；
6. Mastery selection 对 `ps|` identity/hash 的语义；
7. node 10 Life Mastery 的 explicit-choice 产品实现；
8. 根据已选 choice 将某 Mastery 从 pending 转为可兑现；
9. 因 WO-03 实现造成的 `315` mastery-transition census 更新；
10. WO-03 已冻结的 hash sensitivity fallback：`未选择 vs 显式选择`。

即使当前已有 **22** 个 mastery choice 可完整兑现，也不得在 WO-04A2 直接应用。

这 22 个 choice 只是 WO-03 的前置事实，不是本令的授权。

Jewel / Timeless 也不会因为 WO-03 而自动获得授权。

---

# 18. Evidence Pack — Mandatory fields

Evidence Pack 至少必须包含以下内容，缺项不得提交 Gate Review。

## A. Identity

- Work Order: `S6P-WO-04A2`
- status: `READY FOR GATE REVIEW`
- base commit
- implementation commit
- git status
- Unity version
- execution timestamp
- changed-file list

不得自行写 `ACCEPTED`。

## B. Preflight

原样记录：

- Compile result
- EditMode `462/462`
- PlayMode `17/17`
- 04A census `367 / 1660 / 87 / 315`
- legacy reachability `14 / 2429`
- ProdSim cold entry hashes ×3
- PF-6 candidate count

## C. Truth model

列出：

- `EffectTruth` API；
- `TraversalTruth` API；
- canonical owner；
- consumer locations；
- mandatory truth matrix；
- `EffectTruth × TraversalTruth` complete count cross-tab。

## D. Oracle audit

说明所有 traversal/effect consumers 如何从 `PassiveSupport` 获取 truth。

附搜索结果证明没有新增平行 parser/classifier。

若保留旧 helper，必须说明它为什么不是 authoritative oracle。

## E. Frozen fixture

必须给出精确：

```text
StartNode: 2172
TargetNode: <exact id>
Path: [2172, ..., target]
PathLength: <n>
RouteOnlyNodesOnPath: [...]
FirstRouteOnlyNode: <id>
```

并给：

```text
legacy reachable(target): false
new reachable(target): true
```

## F. Reachability measurement

报告：

- canonical node count；
- canonical edge count；
- ordinary in-domain node count；
- special-blocked count；
- out-of-domain count；
- `|R_legacy|`；
- `|R_ref|`；
- `|D|`；
- `|D ∩ R_runtime|`；
- percentage；
- missing node IDs。

验收时 missing 必须：

`[]`

## G. Effect isolation

至少给出：

- ordinary supported fixture；
- ordinary blocked route-only fixture；
- mixed fixture；
- before/after modifier outputs；
- mixed node zero-modifier proof。

## H. Negative controls

分别记录：

- Mastery node ID；
- Jewel Socket node ID；
- Timeless/special node ID；
- optional out-of-domain fixture；

每项必须列：

```text
allocation result
selected-set before/after
passive points before/after
modifier output before/after
```

special reject 的 points delta 必须为 `0`。

## I. Test evidence

提供所有新增 test names 和结果，以及完整：

- Compile result
- EditMode total/pass/fail
- PlayMode total/pass/fail

不得以“覆盖率提升”作为完成依据。

## J. ProdSim canonical

列出 exit cold-process ×3 的三个实际 hashes。

必须全部：

`FNV1A64:ec1d3ed67d3035d0`

## K. ProdSim sensitivity

给出 State A / State B：

- exact selected-node delta；
- route-only node ID；
- `ps|` A/B value 或 digest；
- `pm` comparison；
- `pe` comparison；
- `pk` comparison。

必须明确写：

```text
ps: DIFFERENT
pm: EXACT MATCH
pe: EXACT MATCH
pk: EXACT MATCH
```

## L. Census preservation

再次列出出口：

```text
367 / 1660 / 87 / 315
```

并明确：

`NO SILENT CENSUS UPDATE`

## M. Scope audit

声明并提供 diff/search 证据：

- no new StatId
- no new ModOp
- no parser expansion
- no Mastery selector
- no Jewel gameplay
- no Timeless gameplay
- no Ascendancy gameplay
- no new skill
- no new elemental axis
- no third connection group
- no persistence/save
- no LOD
- no framework/package/asset purchase
- no rebaseline

## N. Governance handoff

更新 `开发计划/UNATTENDED_STATE.md`，内容必须反映：

```text
S6P-WO-04A2 = READY_FOR_GATE_REVIEW
S6P-WO-03 = BLOCKED_PENDING_CHANNEL_A_ACCEPT
```

不得自动开始 WO-03。

---

# 19. Automatic STOP conditions

出现以下任一情形立即停止产品修改，保留失败证据并提交 Channel A，不自行找替代方案：

1. 入口 census 不等于 `367 / 1660 / 87 / 315`；
2. legacy reachability 不能重现 `14 / 2429`；
3. canonical entry hash 不是 `ec1d3ed67d3035d0`；
4. 找不到真实 route-only recovery fixture；
5. 必须修改 parser/StatId/ModOp 才能完成；
6. 必须使 Mastery/Jewel/Timeless traversable 才能达到 reachability gate；
7. route-only node 出现任何 gameplay modifier；
8. mixed node 泄漏任意部分 modifier；
9. Mastery/Jewel/Timeless reject 时发生扣点或 selected-state mutation；
10. §7 reachability `< 100%`；
11. 04A census 任一数字变化；
12. canonical ProdSim exit hash 任一变化；
13. sensitivity `ps|` 不变化；
14. sensitivity `pm/pe/pk` 任一变化；
15. 发现 traversal 与 effect 仍实际依赖同一个 authority bool；
16. 需要越过本令禁止域才能“修好”。

STOP 后：

- 不 rebaseline；
- 不扩大范围；
- 不继续 WO-03；
- Evidence Pack 标 `BLOCKED / NEEDS ARBITRATION`；
- 报告最小复现、实际值、期望值及 first divergence。

---

# 20. Completion rule

本令完成的判据不是“树更能点了”，而是同时证明：

```text
普通节点的路径资格
 与
普通节点的 effect 兑现资格
 已真正分离

+

所有 start-connected ordinary supported nodes
 100% reachable

+

所有 ordinary unsupported/mixed route-only nodes
 0 gameplay effect

+

Mastery/Jewel/Timeless
 仍然 fail-closed

+

canonical ProdSim
 完全不变
```

完成实现后只提交 **S6P-WO-04A2 Evidence Pack** 给 Channel A。

在 Channel A 返回：

`Gate Review = ACCEPT`

之前，**S6P-WO-03 不得执行。**
:::

这份令里唯一需要在执行时“生成后冻结”的东西，是那个真实 recovery target 的具体 ID/path；选择算法已经封死，因此不会给执行器留下人工挑样本或 cherry-pick 的空间。除此之外，没有必要在 WO-04A2 前再插任何令。

 

 

 

 
 
 
 

 日夜切换
 
 
 

 
 
 
 
预设提示词
 

 

 

 
 
 
 
 
 
 
 

 导出对话
 
 
 
 
 

 
 
 
 
 
要求更改
CtrlK
文本