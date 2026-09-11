Gate Review：
ACCEPT。无阻塞 follow-up。正式释放 
S6P-WO-02 — Passive-Aware Production Simulation。
WO-01 做得很扎实：它不仅完成 census，还证明了一个关键事实——现有 
FNV1A64:9a4c9524d0b3e214 对 Passive/Mastery 零敏感。因此 WO-02 不应调整方向，反而现在比计划阶段更有必要。
Gate
裁决
3390 总节点对账
PASS
558 Ascendancy / 2832 non-Ascendancy
PASS
2429 上树 + 403 无坐标
PASS
Director 2832 口径解释
PASS — 已无算术歧义
上树节点分类
PASS
5676 行效果/结构分类
PASS
UNKNOWN
PASS — 0
Parser 单一判定来源
PASS
2005 unsupported 节点量化
PASS — discovery，不是本 WO failure
ProdSim sensitivity audit
PASS — 明确判定 NOT_PASSIVE_SENSITIVE
Allocation invariants census
PASS
Mastery prerequisite
明确缺失，进入 WO-03，不阻塞 WO-01
Runtime/Product Delta
NONE
EditMode
394/394 PASS
PlayMode
14/14 PASS
ProdSim predecessor hash
UNCHANGED / PASS
Drift
0
Forbidden Expansion
PASS

STRUCTURAL 582 行裁定
接受，不要求重跑。
你的理解是正确的：如果这些括号行本身只是解释、提示、结构性注释，而不是可独立施加的 modifier/effect，就不应该为了满足“每条效果行分类”而硬塞进 
BLOCKED_BY_DOMAIN。
但从现在起锁一条规则：

STRUCTURAL 的判定依据必须是“该行的语义角色不是 gameplay effect”，不能简化成“只要有括号就是 STRUCTURAL”。

例如未来遇到：
+X while Y (……)
其中 
while Y 是 gameplay condition，那么整条仍然属于效果语义，不能因为含括号就逃进 STRUCTURAL。
按你当前 evidence，这不构成 WO-01 follow-up。
编辑器两次运行陷阱也接受为环境/无人值守 runbook evidence，不是产品 defect。后续 WO 可以把 
ensure not in Play Mode → test 与 backup-recovery 检测固化进 runner，但不要在 WO-02 借机大改 Unity tooling。
S6P-WO-02 — Passive-Aware Production Simulation
S6P-WO-02 — Passive-Aware Production Simulation
Cycle: S6P — Passive Truth & Build Backbone
Phase: Gate Infrastructure
Primary: Extend deterministic Production Simulation so Passive gameplay truth is authoritative evidence
Gameplay Behavior Authority: 
NONE
Passive Runtime Semantic Authority: 
NONE
Test/Simulation Tool Authority: 
YES
Canonical Data Authority: 
NONE
1. Objective
把 Production Simulation 从当前：
loot
craft
equip
扩展为：
loot
craft
equip
passive allocation/state
passive modifier result
effective gameplay stat result
使以后任何真实 Passive/Mastery gameplay truth 变化都能被 canonical FNV1A64 evidence 捕获。
本 WO 不修 Passive。
特别禁止在本单：
修 Mastery；
禁止 2005 unsupported 节点；
扩 
PoeStatParser；
新增 StatId；
实现任何新被动轴。
本单只修门禁盲区。
2. Entry Baseline
EditMode = 394/394
PlayMode = 14/14

Passive tree on-tree nodes = 2429
Current CONSUMED effect lines = 607
Current supported nodes = 424

Current ProdSim =
FNV1A64:9a4c9524d0b3e214

Current ProdSim Passive Sensitivity =
NONE
旧 hash 从本单开始成为：
PREDECESSOR REFERENCE
不是 WO-02 expected final hash。
3. Canonical Passive Scenario
Production Simulation 增加一个固定、可解释、完全使用当前已支持语义的 passive scenario。
Fixture 必须：
Mastery nodes = 0
Unsupported nodes = 0
Special-interaction nodes = 0
至少选择一条从合法 class/start 出发的、当前真实可分配的 deterministic path，并最终覆盖：
>= 1 offensive CONSUMED effect
>= 1 defensive / attribute / resource CONSUMED effect
如果现有树拓扑无法在一个合理短路径同时满足，可使用两个明确独立的 canonical passive sub-scenarios。
不得为了测试便利绕过：
start rule；
connected allocation；
point accounting。
Fixture ID 选择
Implementer 可从 WO-01 census 中选择实际官方 NodeId，但选择后必须在：
PASSIVE_PRODSIM_CONTRACT
中显式冻结 NodeId。
不得每次通过：
find first currently supported node
动态选择。
否则以后 parser coverage 变化会无故改变 canonical fixture。
4. Production Simulation Event Surface
建议增加稳定事件族，命名可按现有代码风格调整。
例如：
pa|<nodeId>|<result>|<remainingPoints>
表示 canonical allocation event。
最终增加 passive state summary，例如：
ps|<sortedAllocatedNodeIds>
以及 canonicalized modifier/stat output。
核心要求不是字符串名字，而是 semantic coverage。
5. Hash 中必须包含什么
最终 canonical FNV payload 至少包含：
A. Allocated passive identity
所有 canonical allocated NodeId。
序列化必须稳定：
numeric NodeId ascending
或另一个明确唯一的 canonical ordering。
不得依赖：
Dictionary iteration；
HashSet enumeration；
source JSON object ordering。
B. Effective passive modifier truth
对于 canonical allocated nodes 实际进入 runtime 的 modifier，序列化 canonical semantic tuple，例如：
StatId
ModOp
Value
Target/Scope if currently meaningful
按照明确稳定排序。
不要 hash 原始中文/英文说明文本。
C. Effective gameplay stat result
至少包含 canonical scenario 被动实际影响到的最终 gameplay stats。
优先方案：
如果现有 PlayerStats/StatId 可以安全 deterministic enumerate，则使用：
StatId numeric order
→ effective value
否则只冻结 canonical gameplay-stat subset，但必须解释为什么没有全枚举。
6. Hash 中明确禁止包含什么
不得因为“Passive 数据很大”就 hash 整个数据源。
禁止进入 canonical gameplay hash：
node coordinates
zoom
pan
icon path
texture
sprite UV
cluster visual
tooltip text
localized strings
GUI state
hover
selected UI node
window size
screen resolution
asset timestamps
file timestamps
dictionary iteration order
raw tree_raw.json bytes
all 2429 node definitions merely because they exist
ProdSim 证明的是游戏结果，不是资源包是否字节相同。
7. Passive Sensitivity Proof — Mandatory
只跑三次同 hash 不足以通过本 WO。
必须先证明 hash 真的对 Passive 敏感。
Sensitivity A — Allocation Identity
Canonical baseline：
Fixture A
→ Hash H1
在测试隔离环境中，合法地去掉或改变一个 canonical supported allocation：
Fixture B
→ Hash H2
必须：
H1 != H2
Sensitivity B — Runtime Effect
必须证明 passive 的实际 modifier/stat result变化也进入 hash。
优先做法：
使用另一组合法 supported allocation，使最终 effective stat 不同：
EffectiveStat A != EffectiveStat B
→ Hash A != Hash B
不要通过修改 production catalog 数值制造测试。
Sensitivity C — Recovery
恢复 canonical fixture：
Fixture A again
→ exact H1
必须证明没有隐藏 state pollution。
8. Ordering Invariance Proof
Passive hash 不能对无意义的集合迭代顺序敏感。
测试必须证明：
相同 final allocation set / modifier set，以不同输入枚举顺序交给 canonical serializer：
Hash == Hash
如果 allocation 操作顺序本身被设计为 simulation event evidence，可以记录固定 event order；
但最终 semantic-state serialization 仍必须 canonicalized。
9. Existing Allocation Invariants Must Be Exercised
WO-01 已发现现有：
ValidStart = TRUE
ConnectedAllocation = TRUE
NoIllegalJump = TRUE
PointAccounting = TRUE
DuplicateAllocationNoop = TRUE
ResetExact = TRUE
MasteryPrerequisite = FALSE
WO-02 不改变这些事实。
Canonical scenario 只能通过现有合法 allocation API 建立，不能直接写内部 allocated container 绕过 domain。
Mastery 本单不得进入 fixture。
10. Mastery Explicitly Deferred
当前 Mastery：
315 nodes
choices presented
implicit first-choice runtime behavior exists
Mastery prerequisite missing
所以 WO-02 canonical fixture 明确：
Mastery Coverage = EXCLUDED_BY_CONTRACT
原因：
不能把一个已知即将在 WO-03 修正的错误语义冻结为新的 canonical oracle。
WO-03 完成后才把 Mastery selection truth 纳入 Production Simulation。
11. Unsupported Nodes Explicitly Deferred
当前：
SUPPORTED = 424
UNSUPPORTED_CURRENTLY = 2005
WO-02 只使用 SUPPORTED 节点。
不能为了提高 ProdSim coverage：
点 unsupported node；
部分应用 unsupported node；
添加临时 consumer；
修改 parser。
2005 节点仍是 WO-04 的目标面。
12. Expected Product Delta
必须：
Gameplay Delta = NONE
Passive Runtime Delta = NONE
Canonical Data Delta = NONE
Content Delta = NONE
UI Delta = NONE
允许：
Production Simulation Tool Delta = YES
Canonical Evidence Surface Delta = YES
这是 hash 改变的唯一合法原因。
13. FNV Baseline Policy
旧值：
FNV1A64:9a4c9524d0b3e214
本 WO 预计改变。
不得把旧值写成 expected final oracle。
正确流程：
old S5/S6P-WO-01 hash
 ↓
implement passive-aware canonical surface
 ↓
sensitivity proof
 ↓
fresh Production Simulation run #1
 ↓
fresh run #2
 ↓
fresh run #3
 ↓
H1 == H2 == H3
 ↓
establish S6P passive-aware canonical hash
14. Three Independent Production Simulation Runs
最终至少 3 次真正独立运行。
必须报告：
Run1 timestamp
Run1 hash

Run2 timestamp
Run2 hash

Run3 timestamp
Run3 hash
要求：
Run1 == Run2 == Run3
不得：
复制 report；
读取旧 hash 冒充新跑；
同一个进程内仅重复读取缓存结果。
15. Report Schema
PRODUCTION_SIMULATION_REPORT.json 或对应 canonical report 应能明确看出 Passive 已纳入。
建议至少增加类似字段：
passiveScenarioVersion
allocatedPassiveNodeCount
allocatedPassiveNodeIds
passiveModifierCount
passiveEffectiveStatSnapshot
passiveSensitive = true
命名按现有 schema 风格调整。
不要把整棵树塞入 report。
16. Version Marker
建议 canonical hash payload 增加显式 schema marker，例如：
pv|passive-v1
或：
simulationContractVersion = ...
目的是区分：
“数据结果变了”
与：
“canonical hash evidence surface 本身升级了”
不要靠注释隐式表达。
17. Required EditMode Tests
新增 test family 建议：
PassiveAwareProductionSimulationTests
至少覆盖：
1. CanonicalFixtureUsesOnlySupportedNonMasteryNodes
2. CanonicalFixtureAllocatesThroughDomainAPI
3. PassiveAllocatedNodeIdsEnterHash
4. PassiveEffectiveModifiersEnterHash
5. PassiveEffectiveStatsEnterHash
6. PassiveAllocationMutationChangesHash
7. PassiveStatMutationViaLegalAlternateAllocationChangesHash
8. RestoringCanonicalFixtureRestoresExactHash
9. PassiveCollectionOrderingDoesNotChangeHash
10. UIAndTreeGeometryDoNotEnterHash
11. MasteryExcludedFromWO02CanonicalFixture
12. UnsupportedNodesExcludedFromWO02CanonicalFixture
13. SameScenarioProducesExactSamePayload
测试名可调整，但 coverage 不能少。
18. PlayMode
至少保留：
14/14 predecessor PASS
若 Production Simulator 可合理从 PlayMode 走真实 session，可增加一个 bounded integration test：
canonical passive allocation
→ effective player stat
→ simulation snapshot agrees
如果现有架构明显属于 EditMode deterministic simulation，不为了增加 PlayMode 数字强行复制测试。
19. Content Audit
仍要求：
PASS
fresh = YES
预计：
Affix Count = unchanged
Skill/Support content = unchanged
Passive source data = unchanged
20. Headless Chrome
本 WO 不涉及绘制。
因此：
Headless Chrome Screenshot Gate = NOT REQUIRED
不要为了“每令都有截图”做无价值截图。
这也是可自证原则的一部分：只使用与改动面相关的 evidence。
21. Unattended Execution Hygiene
结合 WO-01 环境发现，runner 在执行 Gate 前应保证：
Editor Play Mode = FALSE
如果检测到 Play Mode：
先执行 stop，再进入 EditMode tests。
如出现：
IPCStream timeout
Recovering Scene Backups modal
允许使用已验证恢复路径恢复 Unity 环境。
但：
不修改 gameplay/runtime；
不把环境恢复 commit 到产品；
Evidence 中披露发生次数与恢复方式。
这不是 WO-02 产品 AC。
22. Acceptance Criteria
AC-01 Gameplay Delta = NONE
AC-02 Passive Runtime Delta = NONE
AC-03 Canonical Data Delta = NONE
AC-04 Content Delta = NONE

AC-05 Production Simulator has explicit passive scenario
AC-06 Canonical fixture uses only SUPPORTED nodes
AC-07 Canonical fixture contains zero Mastery nodes
AC-08 Canonical fixture contains zero unsupported nodes
AC-09 Allocations use existing domain allocation path

AC-10 Allocated node identity enters canonical hash
AC-11 Effective passive modifiers enter canonical hash
AC-12 Effective affected gameplay stats enter canonical hash

AC-13 Legal passive allocation mutation changes hash
AC-14 Legal effective-stat mutation changes hash
AC-15 Restoring canonical fixture restores exact original new hash

AC-16 Collection/enumeration ordering cannot change hash
AC-17 UI/tree geometry cannot change hash
AC-18 Raw tree asset bytes are not indiscriminately hashed

AC-19 Passive sensitivity report = TRUE
AC-20 Mastery sensitivity remains explicitly deferred to WO-03

AC-21 Three fresh independent ProdSim runs executed
AC-22 Three final hashes exact-match
AC-23 New passive-aware canonical hash established
AC-24 Old 9a4c... hash retained only as predecessor reference

AC-25 EditMode all PASS
AC-26 PlayMode all PASS
AC-27 Content Audit PASS/fresh
AC-28 no predecessor test deleted/weakened

AC-29 Drift = 0
AC-30 Source-of-Truth Conflicts = 0
AC-31 Forbidden Expansion Audit = PASS
23. Forbidden Expansion
本单禁止实现：
Mastery choice
Mastery prerequisite
unsupported-node rejection
PoeStatParser expansion
new StatId
Cold
Lightning
Energy Shield
Block
Suppression
Minion
Totem
Warcry
Ailment
Flask
Charge
Leech
DoT
Jewel
Timeless Jewel
Ascendancy
new Skill
new Support
new Affix
Map/Craft UI work
Passive LOD
texture-memory optimization
ORK/TDE/vendor framework
这些都不是“为了让 ProdSim 更完整”就能顺便做的事情。
24. Expected Markdown / Evidence Updates
更新相应 S6P 文档，至少记录：
WO-01 ProdSim audit = NOT_PASSIVE_SENSITIVE
WO-02 canonical evidence surface change
frozen passive fixture NodeIds
passive hash payload schema
predecessor hash
new hash
3-run exact-repeat evidence
sensitivity evidence
Mastery explicitly deferred
unsupported nodes explicitly deferred
不要修改产品 roadmap authorization。
25. Evidence Pack
回传：
Work Order:
S6P-WO-02 — Passive-Aware Production Simulation

Revision/Commit:

Changed Runtime Gameplay Files:
Changed Passive Runtime Files:
Changed Production Simulation Files:
Changed Test Files:
Changed Markdown:
Changed Canonical Data:

Entry EditMode:
Entry PlayMode:
Predecessor ProdSim Hash:

Canonical Passive Scenario:
Scenario Version:
Starting Node:
Allocated Node IDs:
Allocated Node Count:
Mastery Nodes In Fixture:
Unsupported Nodes In Fixture:

Offensive Passive Evidence:
Defensive/Attribute Passive Evidence:

Passive Allocation API Used:

Hash Payload Added:
Allocated Identity Included:
Modifier Semantics Included:
Effective Stats Included:
UI/Geometry Excluded:
Raw Tree Bytes Excluded:

Sensitivity Allocation Baseline Hash:
Sensitivity Allocation Mutated Hash:
Allocation Mutation Changed Hash:

Sensitivity Stat Baseline:
Sensitivity Stat Mutated:
Stat Mutation Changed Hash:

Restored Hash:
Restore Exact Match:

Ordering Invariance:
Same-State Payload Exact Match:

Production Simulation Run1:
Production Simulation Run2:
Production Simulation Run3:
All Three Exact Match:

New Passive-Aware Canonical Hash:
Predecessor Hash:
Reason For Hash Change:

Passive Sensitive:
Mastery Sensitive:
Mastery Deferred:

EditMode:
PlayMode:
Content Audit:
Content Audit Fresh:

Gameplay Delta:
Passive Runtime Delta:
Canonical Data Delta:
Content Delta:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Environment Incidents:

Open Questions:
Recommended Next WO:
Expected critical fields：
Mastery Nodes In Fixture = 0
Unsupported Nodes In Fixture = 0

Passive Sensitive = YES
Mastery Sensitive = NO / DEFERRED

Allocation Mutation Changed Hash = YES
Stat Mutation Changed Hash = YES
Restore Exact Match = YES

All Three Exact Match = YES

Gameplay Delta = NONE
Passive Runtime Delta = NONE
Canonical Data Delta = NONE
Content Delta = NONE

Drift = 0
Source-of-Truth Conflicts = 0
Forbidden Expansion Audit = PASS
26. Expected Next
如果 WO-02 ACCEPT：
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
那一单才第一次获得 Passive gameplay behavior authority，并且会要求：
implicit first-choice = eliminated；
explicit selected mastery effect；
prerequisite；
reset；
aggregation；
Passive-aware ProdSim sensitivity；
新 hash 3×重基线。
执行授权生效：
S6P-WO-02 可立即开始。
另外，WO-01 暴露出的 
2005 / 2429 不是“马上把 2005 个全禁掉”的授权；先把门禁变成 Passive-sensitive，再修 Mastery，最后由 WO-04 一次性建立“支持节点真生效 / 不支持节点不得假生效”的统一规则。这个顺序保持不变。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：**Gate Review：`ACCEPT`。无阻塞 follow-up。正式释放 `S6P-WO-02 — Passive-Aware Production Simulation`。**

WO-01 做得很扎实：它不仅完成 census，还证明了一个关键事实——现有 `FNV1A64:9a4c9524d0b3e214` 对 Passive/Mastery **零敏感**。因此 WO-02 不应调整方向，反而现在比计划阶段更有必要。

| Gate | 裁决 |
|---|---|
| 3390 总节点对账 | **PASS** |
| 558 Ascendancy / 2832 non-Ascendancy | **PASS** |
| 2429 上树 + 403 无坐标 | **PASS** |
| Director 2832 口径解释 | **PASS — 已无算术歧义** |
| 上树节点分类 | **PASS** |
| 5676 行效果/结构分类 | **PASS** |
| UNKNOWN | **PASS — 0** |
| Parser 单一判定来源 | **PASS** |
| 2005 unsupported 节点量化 | **PASS — discovery，不是本 WO failure** |
| ProdSim sensitivity audit | **PASS — 明确判定 NOT_PASSIVE_SENSITIVE** |
| Allocation invariants census | **PASS** |
| Mastery prerequisite | **明确缺失，进入 WO-03，不阻塞 WO-01** |
| Runtime/Product Delta | **NONE** |
| EditMode | **394/394 PASS** |
| PlayMode | **14/14 PASS** |
| ProdSim predecessor hash | **UNCHANGED / PASS** |
| Drift | **0** |
| Forbidden Expansion | **PASS** |

### `STRUCTURAL` 582 行裁定

**接受，不要求重跑。**

你的理解是正确的：如果这些括号行本身只是解释、提示、结构性注释，而不是可独立施加的 modifier/effect，就不应该为了满足“每条效果行分类”而硬塞进 `BLOCKED_BY_DOMAIN`。

但从现在起锁一条规则：

> `STRUCTURAL` 的判定依据必须是“该行的语义角色不是 gameplay effect”，**不能简化成“只要有括号就是 STRUCTURAL”**。

例如未来遇到：

`+X while Y (……)`

其中 `while Y` 是 gameplay condition，那么整条仍然属于效果语义，不能因为含括号就逃进 STRUCTURAL。

按你当前 evidence，这不构成 WO-01 follow-up。

编辑器两次运行陷阱也接受为**环境/无人值守 runbook evidence**，不是产品 defect。后续 WO 可以把 `ensure not in Play Mode → test` 与 backup-recovery 检测固化进 runner，但不要在 WO-02 借机大改 Unity tooling。

:::writing{variant="document" id="62591" title="S6P-WO-02 — Passive-Aware Production Simulation"}
# S6P-WO-02 — Passive-Aware Production Simulation

**Cycle:** S6P — Passive Truth & Build Backbone 
**Phase:** Gate Infrastructure 
**Primary:** Extend deterministic Production Simulation so Passive gameplay truth is authoritative evidence 
**Gameplay Behavior Authority:** `NONE` 
**Passive Runtime Semantic Authority:** `NONE` 
**Test/Simulation Tool Authority:** `YES` 
**Canonical Data Authority:** `NONE`

## 1. Objective

把 Production Simulation 从当前：

```text
loot
craft
equip
```

扩展为：

```text
loot
craft
equip
passive allocation/state
passive modifier result
effective gameplay stat result
```

使以后任何真实 Passive/Mastery gameplay truth 变化都能被 canonical FNV1A64 evidence 捕获。

本 WO **不修 Passive**。

特别禁止在本单：

- 修 Mastery；
- 禁止 2005 unsupported 节点；
- 扩 `PoeStatParser`；
- 新增 StatId；
- 实现任何新被动轴。

本单只修**门禁盲区**。

---

# 2. Entry Baseline

```text
EditMode = 394/394
PlayMode = 14/14

Passive tree on-tree nodes = 2429
Current CONSUMED effect lines = 607
Current supported nodes = 424

Current ProdSim =
FNV1A64:9a4c9524d0b3e214

Current ProdSim Passive Sensitivity =
NONE
```

旧 hash 从本单开始成为：

`PREDECESSOR REFERENCE`

不是 WO-02 expected final hash。

---

# 3. Canonical Passive Scenario

Production Simulation 增加一个**固定、可解释、完全使用当前已支持语义**的 passive scenario。

Fixture 必须：

```text
Mastery nodes = 0
Unsupported nodes = 0
Special-interaction nodes = 0
```

至少选择一条从合法 class/start 出发的、当前真实可分配的 deterministic path，并最终覆盖：

```text
>= 1 offensive CONSUMED effect
>= 1 defensive / attribute / resource CONSUMED effect
```

如果现有树拓扑无法在一个合理短路径同时满足，可使用两个明确独立的 canonical passive sub-scenarios。

不得为了测试便利绕过：

- start rule；
- connected allocation；
- point accounting。

## Fixture ID 选择

Implementer 可从 WO-01 census 中选择实际官方 NodeId，但选择后必须在：

`PASSIVE_PRODSIM_CONTRACT`

中**显式冻结 NodeId**。

不得每次通过：

`find first currently supported node`

动态选择。

否则以后 parser coverage 变化会无故改变 canonical fixture。

---

# 4. Production Simulation Event Surface

建议增加稳定事件族，命名可按现有代码风格调整。

例如：

```text
pa|<nodeId>|<result>|<remainingPoints>
```

表示 canonical allocation event。

最终增加 passive state summary，例如：

```text
ps|<sortedAllocatedNodeIds>
```

以及 canonicalized modifier/stat output。

核心要求不是字符串名字，而是 semantic coverage。

---

# 5. Hash 中必须包含什么

最终 canonical FNV payload 至少包含：

### A. Allocated passive identity

所有 canonical allocated NodeId。

序列化必须稳定：

```text
numeric NodeId ascending
```

或另一个明确唯一的 canonical ordering。

不得依赖：

- Dictionary iteration；
- HashSet enumeration；
- source JSON object ordering。

### B. Effective passive modifier truth

对于 canonical allocated nodes 实际进入 runtime 的 modifier，序列化 canonical semantic tuple，例如：

```text
StatId
ModOp
Value
Target/Scope if currently meaningful
```

按照明确稳定排序。

不要 hash 原始中文/英文说明文本。

### C. Effective gameplay stat result

至少包含 canonical scenario 被动实际影响到的最终 gameplay stats。

优先方案：

如果现有 PlayerStats/StatId 可以安全 deterministic enumerate，则使用：

```text
StatId numeric order
→ effective value
```

否则只冻结 canonical gameplay-stat subset，但必须解释为什么没有全枚举。

---

# 6. Hash 中明确禁止包含什么

不得因为“Passive 数据很大”就 hash 整个数据源。

禁止进入 canonical gameplay hash：

```text
node coordinates
zoom
pan
icon path
texture
sprite UV
cluster visual
tooltip text
localized strings
GUI state
hover
selected UI node
window size
screen resolution
asset timestamps
file timestamps
dictionary iteration order
raw tree_raw.json bytes
all 2429 node definitions merely because they exist
```

ProdSim 证明的是**游戏结果**，不是资源包是否字节相同。

---

# 7. Passive Sensitivity Proof — Mandatory

只跑三次同 hash **不足以通过本 WO**。

必须先证明 hash 真的对 Passive 敏感。

## Sensitivity A — Allocation Identity

Canonical baseline：

```text
Fixture A
→ Hash H1
```

在测试隔离环境中，合法地去掉或改变一个 canonical supported allocation：

```text
Fixture B
→ Hash H2
```

必须：

```text
H1 != H2
```

## Sensitivity B — Runtime Effect

必须证明 passive 的**实际 modifier/stat result**变化也进入 hash。

优先做法：

使用另一组合法 supported allocation，使最终 effective stat 不同：

```text
EffectiveStat A != EffectiveStat B
→ Hash A != Hash B
```

不要通过修改 production catalog 数值制造测试。

## Sensitivity C — Recovery

恢复 canonical fixture：

```text
Fixture A again
→ exact H1
```

必须证明没有隐藏 state pollution。

---

# 8. Ordering Invariance Proof

Passive hash 不能对无意义的集合迭代顺序敏感。

测试必须证明：

相同 final allocation set / modifier set，以不同输入枚举顺序交给 canonical serializer：

```text
Hash == Hash
```

如果 allocation **操作顺序本身**被设计为 simulation event evidence，可以记录固定 event order；

但最终 semantic-state serialization 仍必须 canonicalized。

---

# 9. Existing Allocation Invariants Must Be Exercised

WO-01 已发现现有：

```text
ValidStart = TRUE
ConnectedAllocation = TRUE
NoIllegalJump = TRUE
PointAccounting = TRUE
DuplicateAllocationNoop = TRUE
ResetExact = TRUE
MasteryPrerequisite = FALSE
```

WO-02 不改变这些事实。

Canonical scenario 只能通过现有合法 allocation API 建立，不能直接写内部 allocated container 绕过 domain。

Mastery 本单不得进入 fixture。

---

# 10. Mastery Explicitly Deferred

当前 Mastery：

```text
315 nodes
choices presented
implicit first-choice runtime behavior exists
Mastery prerequisite missing
```

所以 WO-02 canonical fixture 明确：

```text
Mastery Coverage = EXCLUDED_BY_CONTRACT
```

原因：

**不能把一个已知即将在 WO-03 修正的错误语义冻结为新的 canonical oracle。**

WO-03 完成后才把 Mastery selection truth 纳入 Production Simulation。

---

# 11. Unsupported Nodes Explicitly Deferred

当前：

```text
SUPPORTED = 424
UNSUPPORTED_CURRENTLY = 2005
```

WO-02 只使用 SUPPORTED 节点。

不能为了提高 ProdSim coverage：

- 点 unsupported node；
- 部分应用 unsupported node；
- 添加临时 consumer；
- 修改 parser。

2005 节点仍是 WO-04 的目标面。

---

# 12. Expected Product Delta

必须：

```text
Gameplay Delta = NONE
Passive Runtime Delta = NONE
Canonical Data Delta = NONE
Content Delta = NONE
UI Delta = NONE
```

允许：

```text
Production Simulation Tool Delta = YES
Canonical Evidence Surface Delta = YES
```

这是 hash 改变的唯一合法原因。

---

# 13. FNV Baseline Policy

旧值：

```text
FNV1A64:9a4c9524d0b3e214
```

本 WO **预计改变**。

不得把旧值写成 expected final oracle。

正确流程：

```text
old S5/S6P-WO-01 hash
 ↓
implement passive-aware canonical surface
 ↓
sensitivity proof
 ↓
fresh Production Simulation run #1
 ↓
fresh run #2
 ↓
fresh run #3
 ↓
H1 == H2 == H3
 ↓
establish S6P passive-aware canonical hash
```

---

# 14. Three Independent Production Simulation Runs

最终至少 3 次真正独立运行。

必须报告：

```text
Run1 timestamp
Run1 hash

Run2 timestamp
Run2 hash

Run3 timestamp
Run3 hash
```

要求：

```text
Run1 == Run2 == Run3
```

不得：

- 复制 report；
- 读取旧 hash 冒充新跑；
- 同一个进程内仅重复读取缓存结果。

---

# 15. Report Schema

`PRODUCTION_SIMULATION_REPORT.json` 或对应 canonical report 应能明确看出 Passive 已纳入。

建议至少增加类似字段：

```text
passiveScenarioVersion
allocatedPassiveNodeCount
allocatedPassiveNodeIds
passiveModifierCount
passiveEffectiveStatSnapshot
passiveSensitive = true
```

命名按现有 schema 风格调整。

不要把整棵树塞入 report。

---

# 16. Version Marker

建议 canonical hash payload 增加显式 schema marker，例如：

```text
pv|passive-v1
```

或：

```text
simulationContractVersion = ...
```

目的是区分：

```text
“数据结果变了”
```

与：

```text
“canonical hash evidence surface 本身升级了”
```

不要靠注释隐式表达。

---

# 17. Required EditMode Tests

新增 test family 建议：

`PassiveAwareProductionSimulationTests`

至少覆盖：

```text
1. CanonicalFixtureUsesOnlySupportedNonMasteryNodes
2. CanonicalFixtureAllocatesThroughDomainAPI
3. PassiveAllocatedNodeIdsEnterHash
4. PassiveEffectiveModifiersEnterHash
5. PassiveEffectiveStatsEnterHash
6. PassiveAllocationMutationChangesHash
7. PassiveStatMutationViaLegalAlternateAllocationChangesHash
8. RestoringCanonicalFixtureRestoresExactHash
9. PassiveCollectionOrderingDoesNotChangeHash
10. UIAndTreeGeometryDoNotEnterHash
11. MasteryExcludedFromWO02CanonicalFixture
12. UnsupportedNodesExcludedFromWO02CanonicalFixture
13. SameScenarioProducesExactSamePayload
```

测试名可调整，但 coverage 不能少。

---

# 18. PlayMode

至少保留：

```text
14/14 predecessor PASS
```

若 Production Simulator 可合理从 PlayMode 走真实 session，可增加一个 bounded integration test：

```text
canonical passive allocation
→ effective player stat
→ simulation snapshot agrees
```

如果现有架构明显属于 EditMode deterministic simulation，不为了增加 PlayMode 数字强行复制测试。

---

# 19. Content Audit

仍要求：

```text
PASS
fresh = YES
```

预计：

```text
Affix Count = unchanged
Skill/Support content = unchanged
Passive source data = unchanged
```

---

# 20. Headless Chrome

本 WO 不涉及绘制。

因此：

```text
Headless Chrome Screenshot Gate = NOT REQUIRED
```

不要为了“每令都有截图”做无价值截图。

这也是可自证原则的一部分：只使用与改动面相关的 evidence。

---

# 21. Unattended Execution Hygiene

结合 WO-01 环境发现，runner 在执行 Gate 前应保证：

```text
Editor Play Mode = FALSE
```

如果检测到 Play Mode：

先执行 stop，再进入 EditMode tests。

如出现：

```text
IPCStream timeout
Recovering Scene Backups modal
```

允许使用已验证恢复路径恢复 Unity 环境。

但：

- 不修改 gameplay/runtime；
- 不把环境恢复 commit 到产品；
- Evidence 中披露发生次数与恢复方式。

这不是 WO-02 产品 AC。

---

# 22. Acceptance Criteria

```text
AC-01 Gameplay Delta = NONE
AC-02 Passive Runtime Delta = NONE
AC-03 Canonical Data Delta = NONE
AC-04 Content Delta = NONE

AC-05 Production Simulator has explicit passive scenario
AC-06 Canonical fixture uses only SUPPORTED nodes
AC-07 Canonical fixture contains zero Mastery nodes
AC-08 Canonical fixture contains zero unsupported nodes
AC-09 Allocations use existing domain allocation path

AC-10 Allocated node identity enters canonical hash
AC-11 Effective passive modifiers enter canonical hash
AC-12 Effective affected gameplay stats enter canonical hash

AC-13 Legal passive allocation mutation changes hash
AC-14 Legal effective-stat mutation changes hash
AC-15 Restoring canonical fixture restores exact original new hash

AC-16 Collection/enumeration ordering cannot change hash
AC-17 UI/tree geometry cannot change hash
AC-18 Raw tree asset bytes are not indiscriminately hashed

AC-19 Passive sensitivity report = TRUE
AC-20 Mastery sensitivity remains explicitly deferred to WO-03

AC-21 Three fresh independent ProdSim runs executed
AC-22 Three final hashes exact-match
AC-23 New passive-aware canonical hash established
AC-24 Old 9a4c... hash retained only as predecessor reference

AC-25 EditMode all PASS
AC-26 PlayMode all PASS
AC-27 Content Audit PASS/fresh
AC-28 no predecessor test deleted/weakened

AC-29 Drift = 0
AC-30 Source-of-Truth Conflicts = 0
AC-31 Forbidden Expansion Audit = PASS
```

---

# 23. Forbidden Expansion

本单禁止实现：

```text
Mastery choice
Mastery prerequisite
unsupported-node rejection
PoeStatParser expansion
new StatId
Cold
Lightning
Energy Shield
Block
Suppression
Minion
Totem
Warcry
Ailment
Flask
Charge
Leech
DoT
Jewel
Timeless Jewel
Ascendancy
new Skill
new Support
new Affix
Map/Craft UI work
Passive LOD
texture-memory optimization
ORK/TDE/vendor framework
```

这些都不是“为了让 ProdSim 更完整”就能顺便做的事情。

---

# 24. Expected Markdown / Evidence Updates

更新相应 S6P 文档，至少记录：

```text
WO-01 ProdSim audit = NOT_PASSIVE_SENSITIVE
WO-02 canonical evidence surface change
frozen passive fixture NodeIds
passive hash payload schema
predecessor hash
new hash
3-run exact-repeat evidence
sensitivity evidence
Mastery explicitly deferred
unsupported nodes explicitly deferred
```

不要修改产品 roadmap authorization。

---

# 25. Evidence Pack

回传：

```text
Work Order:
S6P-WO-02 — Passive-Aware Production Simulation

Revision/Commit:

Changed Runtime Gameplay Files:
Changed Passive Runtime Files:
Changed Production Simulation Files:
Changed Test Files:
Changed Markdown:
Changed Canonical Data:

Entry EditMode:
Entry PlayMode:
Predecessor ProdSim Hash:

Canonical Passive Scenario:
Scenario Version:
Starting Node:
Allocated Node IDs:
Allocated Node Count:
Mastery Nodes In Fixture:
Unsupported Nodes In Fixture:

Offensive Passive Evidence:
Defensive/Attribute Passive Evidence:

Passive Allocation API Used:

Hash Payload Added:
Allocated Identity Included:
Modifier Semantics Included:
Effective Stats Included:
UI/Geometry Excluded:
Raw Tree Bytes Excluded:

Sensitivity Allocation Baseline Hash:
Sensitivity Allocation Mutated Hash:
Allocation Mutation Changed Hash:

Sensitivity Stat Baseline:
Sensitivity Stat Mutated:
Stat Mutation Changed Hash:

Restored Hash:
Restore Exact Match:

Ordering Invariance:
Same-State Payload Exact Match:

Production Simulation Run1:
Production Simulation Run2:
Production Simulation Run3:
All Three Exact Match:

New Passive-Aware Canonical Hash:
Predecessor Hash:
Reason For Hash Change:

Passive Sensitive:
Mastery Sensitive:
Mastery Deferred:

EditMode:
PlayMode:
Content Audit:
Content Audit Fresh:

Gameplay Delta:
Passive Runtime Delta:
Canonical Data Delta:
Content Delta:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Environment Incidents:

Open Questions:
Recommended Next WO:
```

Expected critical fields：

```text
Mastery Nodes In Fixture = 0
Unsupported Nodes In Fixture = 0

Passive Sensitive = YES
Mastery Sensitive = NO / DEFERRED

Allocation Mutation Changed Hash = YES
Stat Mutation Changed Hash = YES
Restore Exact Match = YES

All Three Exact Match = YES

Gameplay Delta = NONE
Passive Runtime Delta = NONE
Canonical Data Delta = NONE
Content Delta = NONE

Drift = 0
Source-of-Truth Conflicts = 0
Forbidden Expansion Audit = PASS
```

---

# 26. Expected Next

如果 WO-02 ACCEPT：

`S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`

那一单才第一次获得 Passive gameplay behavior authority，并且会要求：

- implicit first-choice = eliminated；
- explicit selected mastery effect；
- prerequisite；
- reset；
- aggregation；
- Passive-aware ProdSim sensitivity；
- 新 hash 3×重基线。
:::

**执行授权生效：`S6P-WO-02` 可立即开始。**

另外，WO-01 暴露出的 `2005 / 2429` 不是“马上把 2005 个全禁掉”的授权；先把门禁变成 Passive-sensitive，再修 Mastery，最后由 WO-04 一次性建立“支持节点真生效 / 不支持节点不得假生效”的统一规则。这个顺序保持不变。
 

 

 

 

 
 
 
 

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
项目符号列表