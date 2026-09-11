# S6P-WO-04A2 — Evidence Pack

**Work Order**: `S6P-WO-04A2 — Passive Traversal / Effect Separation`
**Status**: `READY FOR GATE REVIEW`（未自行写 ACCEPTED）
**Authority**: Channel A
**Base commit**: `ce6f85cd23d324ea68a76f98d28663f1258a3c01`（= origin/main）
**Implementation commit**: **`c57f55f`**（"S6P-WO-04A2: Passive Traversal / Effect Separation"；已推送 origin/main）
**HEAD at gate review**: `c57f55f`（Gate Review 按 commit-pinned 内容审阅）
**Unity**: 6000.3.23f1（URP，Windows）
**Execution**: 2026-09-11 10:35–11:05 (+08)
**Contract**: `docs/reviews/S6P/S6P_WO_04A2_CONTRACT.md`
**Record**: `docs/reviews/S6P/S6P_WO_04A2.md`
**Gate Review**: `docs/reviews/S6P/S6P_WO_04A2_GATE_REVIEW.md`（**ACCEPT，AC-1..AC-20 = 20/20 PASS**）

> 提交后补记（Gate Review 第一节要求）：本 Pack 首次成文时工作区尚未提交（HEAD `ce6f85c`，dirty）。
> 该 dirty content state 随后被提交为 **`c57f55f`**，Gate Review 审的就是该提交。
> 历史 final fingerprint `2dd76339fec2741e7bbd79df0b2bf9fac4b21ca31c837ca431bdd56956e2cfbd`
> **原样保留**：它准确描述的是"提交前、全部 04A2 内容就位的 dirty content state"，不是笔误。

---

## A. Identity

```text
branch                 : main
HEAD                   : ce6f85cd23d324ea68a76f98d28663f1258a3c01
working tree           : DIRTY（本令改动；未提交，等待 Gate Review）
entry fingerprint      : 6d2c9ae4536a17665851cadb3a72643fba3b34036c4fc65c2216d3e2d0c32b68
                         2798 files / 321423157 bytes / dirty=NO / HEAD ce6f85c
exit  fingerprint      : 见 docs/reviews/S6P/_wo04a2_fingerprint_final.txt
```

changed-file list（完整差分清单）

```text
[Assets/Runtime] 3
    M Assets/Runtime/Core/Gameplay/PassiveSupport.cs
    M Assets/Runtime/Core/Gameplay/SliceHud.cs
    M Assets/Runtime/Core/Gameplay/SliceSession.cs
[Assets/Tests] 11（含 2 个新文件）
    ?? Assets/Tests/EditMode/PassiveTraversal04A2.cs (+ .meta)
    ?? Assets/Tests/EditMode/S6PWo04A2Tests.cs (+ .meta)
    M  Assets/Tests/EditMode/PassiveAwareProductionSimulation.cs
    M  Assets/Tests/EditMode/PassiveCensus.cs
    M  Assets/Tests/EditMode/PassiveSupportTests.cs
    M  Assets/Tests/EditMode/PoeTreeTests.cs
    M  Assets/Tests/EditMode/S3R2FireConversionTests.cs
    M  Assets/Tests/EditMode/SliceLoopTests.cs
    M  Assets/Tests/PlayMode/S6PPassiveSupportPlayModeTests.cs
[docs] 6 + 1 artifact
    ?? docs/qa/WO_04A2_TRAVERSAL_REPORT.json
    ?? docs/reviews/S6P/S6P_WO_04A2_CONTRACT.md
    ?? docs/reviews/S6P/S6P_WO_04A2.md
    ?? docs/reviews/S6P/S6P_WO_04A2_EVIDENCE_PACK.md
    ?? docs/reviews/S6P/_wo04a2_fingerprint_entry.txt
    ?? docs/reviews/S6P/_wo04a2_cold_entry.txt
    ?? docs/reviews/S6P/_wo04a2_cold_exit.txt
```

**产品代码面**：仅 3 个 runtime 文件；无内容数据、无 canonical 数据、无配置改动。

---

## B. Preflight（原样记录）

```text
Compile            : 0 error
EditMode (entry)   : 462 / 462, fail=0, skip=0
PlayMode (entry)   : 17 / 17, fail=0, skip=0
04A census (entry) : FULLY_SUPPORTED 367 / BLOCKED 1660 / SPECIAL 87 / MASTERY 315
legacy reachability: 14 / 2429（只用 canonical 图边，从 StartNode=2172）
ProdSim cold entry : ×3 → FNV1A64:ec1d3ed67d3035d0
                     Run1 2026-09-11 11:00:46 | 16.7s | exit=0 | contract=pv|passive-v1 | invalid=0
                     Run2 2026-09-11 11:01:00 | 14.2s | exit=0 | contract=pv|passive-v1 | invalid=0
                     Run3 2026-09-11 11:01:14 | 14.2s | exit=0 | contract=pv|passive-v1 | invalid=0
                     Distinct hashes = 1 ; EXACT MATCH = YES
PF-6 candidate count: 311
```

PF-5 的取证手法（值得复用）：把改动集 `git stash push -u` 暂存 → 在干净 HEAD `ce6f85c` 上跑
冷进程 ×3 → `git stash pop` 复原。复原后 `git status --porcelain` 与暂存前**逐字一致**，stash 已 drop。
证据：`_wo04a2_cold_entry.txt`。

---

## C. Truth model

**canonical owner**：`Assets/Runtime/Core/Gameplay/PassiveSupport.cs`（唯一；无第二套 parser/classifier/oracle）

```csharp
public enum EffectTruth    : byte { FullySupported = 0, Unfulfilled = 1, SpecialPending = 2 }
public enum TraversalTruth : byte { Traversable = 0, SpecialBlocked = 1, OutOfDomain = 2 }
public struct NodeTruth { public EffectTruth Effect; public TraversalTruth Traversal; }

PassiveSupport.EvaluateTruth(int)      -> NodeTruth      // canonical
PassiveSupport.EvaluateTruth(PoeNode)  -> NodeTruth
PassiveSupport.IsTraversable(TraversalTruth) -> bool     // 分配门唯一判据
PassiveSupport.YieldsModifiers(EffectTruth)  -> bool     // 消费门唯一判据
PassiveSupport.TraversalReason(int)    -> string|null    // 不可通行时的稳定原因
PassiveSupport.ReachableSet()          -> bool[]         // 可达性真值（BFS，邻接升序）
PassiveSupport.EvaluateNode(int)/(PoeNode) -> NodeStatus // 04A 四值**诊断投影**（派生；不驱动任何门）
```

**consumer 位置**（全部读上面的 canonical API，无人自建第二套）：

| 消费者 | 读什么 |
|---|---|
| `SliceSession.TryAllocate` | `EvaluateTruth(...).Traversal` → `IsTraversable` |
| `SliceSession.CanAllocate` | 同上 |
| `SliceSession.NodeBlockReason` | `PassiveSupport.TraversalReason` |
| `SliceSession.BlockedAllocatedCount` | `EvaluateTruth(...).Traversal` |
| `SliceSession.RecalcPlayer`（消费门） | `EvaluateTruth(...).Effect` → `YieldsModifiers` |
| `SliceSession.CollectSkillMods`（消费门） | 同上 |
| `SliceSession.NodeTruth(int)` | 透传，供 UI/测试 |
| `SliceHud` tooltip / tint | `NodeTruth`（经 `SliceSession.NodeTruth`） |
| `PassiveCensus` | `EvaluateTruth(...).Effect`（挑 supported 样例）＋ `EvaluateNode`（census 记账） |
| `PassiveAwareProductionSimulation.ModifierTuples` | `EvaluateTruth(...).Effect`（与消费门同判据） |
| `PassiveTraversal04A2`（测试） | `EvaluateTruth`（独立参考遍历） |

**mandatory truth matrix（实测）**

| 节点类别 | Effect | Traversal | 计数 |
|---|---|---|---|
| 普通、可完整兑现 | `FULLY_SUPPORTED` | `TRAVERSABLE` | **367** |
| 普通、不可完整兑现（含 mixed） | `UNFULFILLED` | `TRAVERSABLE` | **1660** |
| 专精 + 珠宝孔 + 时光珠宝类 | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | **402**（315 + 57 + 30） |
| 域外 | `SPECIAL_PENDING` | `OUT_OF_DOMAIN` | 0（上树域内） |

`EffectTruth × TraversalTruth` 完整交叉表（3×3，其余格全 0）：

```text
                     TRAVERSABLE   SPECIAL_BLOCKED   OUT_OF_DOMAIN
FULLY_SUPPORTED          367              0                0
UNFULFILLED             1660              0                0
SPECIAL_PENDING            0            402                0
```

由 `S6PWo04A2Tests.TruthMatrix_MatchesContract_And04ACensusPreserved` 机械核对。

---

## D. Oracle audit

- 唯一分类实现：`PassiveSupport.ClassifyTruth(PoeNode)`；四值视图 `Project(n, truth)` 由它派生
  （`EvaluateNode` 与 `EvaluateTruth` 共用同一缓存条目，不可能分叉）。
- **没有任何门读四值投影**：`S6PWo04A2Tests.GatesDoNotReadTheDiagnosticProjection` 直接读
  `Assets/Runtime/Core/Gameplay/SliceSession.cs` 源码并断言：
  - `EvaluateNode(` 出现次数 = 0
  - `IsAllocatable` 出现次数 = 0
  - `YieldsModifiers` 存在（消费门）
  - `IsTraversable` 存在（通行门）
- `S6PWo04A2Tests.TruthOwner_ExposesTwoIndependentDimensions_AndNoSharedBooleanAuthority` 用反射断言
  `IsAllocatable` 已从公开 API 消失（旧 04A 的共用布尔 authority），两个维度判据独立存在。
- 保留的旧 helper：`NodeStatus` / `EvaluateNode` —— 保留原因：让 04A 冻结 census（367/1660/87/315）
  继续可机械复核；它**不是**任何门的判据（上条源码扫描即证）。

---

## E. Frozen fixture

```text
StartNode                : 2172
TargetNode               : 183
Path                     : [2172, 71, 183]
PathLength               : 3 nodes / 2 hops
RouteOnlyNodesOnPath     : [71]
FirstRouteOnlyNode       : 71
CandidateCount |C|       : 311
legacy reachable(target) : false
new reachable(target)    : true
```

发现算法（§8.3，写死在测试里）：`C = { t ∈ D : t ∉ R_legacy ∧ canonical 最短路径含 ≥1 个 route-only 节点 }`；
邻接一律 nodeId 升序 BFS；按 `(距离, nodeId)` 升序取唯一 target。
冻结断言：`S6PWo04A2Tests.RecoveryFixture_IsFrozen`（ID / 距离 / 路径 / route-only 集合 / 首个 route-only
逐字比对，漂移即 FAIL）。分配可行性：`FrozenPath_IsAllocatableThroughDomainApi`（全程经生产 API，扣点精确）。

---

## F. Reachability measurement

```text
canonical node count        : 2429
canonical edge count        : 2787（去重无向）
edgeless nodes              : 30（全部 locked != 0）
ordinary in-domain (TRAVERSABLE) : 2027
special-blocked             : 402
out-of-domain               : 0
|R_legacy|                  : 14
|R_ref|                     : 1985
|R_runtime|                 : 1985
|D|  (start-connected ordinary supported) : 325
|D ∩ R_runtime|             : 325
percentage                  : 100.000%
missing node IDs            : []
```

- `R_runtime` 由生产 `PassiveSupport.ReachableSet()` 产出，`R_ref` 由测试侧独立实现
  `PassiveTraversal04A2.ReferenceReach()` 产出 —— 测试要求两者**逐节点一致**（`ReachabilityGate_IsNonTrivial_AndProductionMatchesReference`）。
- **诚实说明**：按合同 §7.2 / §7.3 的字面定义，`G_ref` 与 `G_runtime` 的 transit 集合是同一个集合，
  因此两者同构、AC-15 的等式**构造性成立**。其机械化价值 = 「生产 BFS 无 bug」+「D 已穷举」+「missing 每次复核」。
  本令真正的收益数字是 **14 → 1985**。若 Channel A 要求更严格的参考图定义，请给出修正，可另开一轮补测。
- **分母之外的诚实登记**：`D` 之外的 42 个 `FULLY_SUPPORTED` 节点（**start-disconnected supported nodes**，
  即"起点非连通 supported 节点"，**不是** `OUT_OF_DOMAIN`——`outOfDomain = 0`）已单独列出并钉死：
  `supportedNotStartConnected = 42`，ID 全表见 `docs/qa/WO_04A2_TRAVERSAL_REPORT.json`；
  测试 `SupportedNodesOutsideStartConnectedRegion_AreRegistered` 防它被静默吞掉。
  本令与 WO-03 均不得触碰这 42 个；它们是未来明确获批的 special-interaction topology 工作的输入，
  且**不得预承诺"实现 Jewel 就一定会解开全部 42 个"**（哪些属 Jewel、哪些由 locked/Timeless
  或其它结构原因控制，须待相应域正式开令后按源图重测）。

---

## G. Effect isolation

| fixture | Effect / Traversal | 注入 `Allocated[]` 后 |
|---|---|---|
| 普通 supported（559 / 1795 / 2034） | `FULLY_SUPPORTED` / `TRAVERSABLE` | `pm/pe/pk` **真的变化**（正对照） |
| route-only mixed（71 / 12 / 255） | `UNFULFILLED` / `TRAVERSABLE` | `ps|` 变化；`pm|/pe|/pk|` **EXACT MATCH** |
| 专精 10 / 珠宝孔 78 / locked 节点 | `SPECIAL_PENDING` / `SPECIAL_BLOCKED` | 分配被拒；注入后仍 0 效果，且 `BlockedAllocatedCount` +1 |

before/after 取值由 `PassiveAwareProductionSimulation.StatePayload` 的四段（`ps| / pm| / pe| / pk|`）逐段比对，
不是「看起来没变」。mixed 节点 zero-modifier proof 见 `MixedNodes_AreRouteOnly_AndContributeNothing`
与 `PassiveSupportTests.InjectedBlockedNodeContributesZeroModifiers`。

---

## H. Negative controls

| 对照节点 | 分配结果 | selected 集 | 天赋点 | modifier 输出 |
|---|---|---|---|---|
| Mastery node **10**（相连性已置成立） | **reject**（`专精暂未开放显式选择`） | 零写入 | **Δ=0** | 无变化（整份 `StatePayload` 一字不变） |
| Jewel Socket node **78**（相连性已置成立） | **reject**（`该节点需要当前尚未实现的特殊交互`） | 零写入 | **Δ=0** | 无变化 |
| Timeless/special（首个 `locked != 0` 节点） | **reject** | 零写入 | **Δ=0** | 无变化 |
| optional out-of-domain | 索引域外，`EvaluateTruth` 返回 `SPECIAL_PENDING / OUT_OF_DOMAIN`；分配门拒绝 | 零写入 | **Δ=0** | 无变化 |
| 非相邻普通节点（可通行但不在起点连通域） | reject（`需与已点亮节点相连`） | 零写入 | **Δ=0** | 无变化 |
| `Unspent = 0` | reject（`没有天赋点`） | 零写入 | **Δ=0** | 无变化 |

special reject 的 points delta 全部为 `0`。

---

## I. Test evidence

```text
Compile   : 0 error
EditMode  : total 475 / passed 475 / failed 0 / skipped 0
PlayMode  : total 18  / passed 18  / failed 0 / skipped 0
```

本令新增测试（13 条，`Game.Tests.EditMode.S6PWo04A2Tests`，全部 Passed）：

```text
TruthOwner_ExposesTwoIndependentDimensions_AndNoSharedBooleanAuthority
GatesDoNotReadTheDiagnosticProjection
TruthMatrix_MatchesContract_And04ACensusPreserved
MixedNodes_AreRouteOnly_AndContributeNothing
ReachabilityGate_SupportedDenominatorIsFullyReachable
ReachabilityGate_IsNonTrivial_AndProductionMatchesReference
RecoveryFixture_IsFrozen
FrozenPath_IsAllocatableThroughDomainApi
SpecialNodes_Reject_WithZeroPointAndZeroStateMutation
TraversalNegativeControls_NonAdjacentAndNoPoints
ProdSimSensitivity_RouteOnlyAllocationChangesIdentityOnly
SupportedNodesOutsideStartConnectedRegion_AreRegistered
WritesTraversalArtifact
```

PlayMode 新增/改写（`Game.Tests.PlayMode.S6PPassiveSupportPlayModeTests`，全部 Passed）：

```text
RouteOnlyNode_Allocate_SpendsPoint_ButYieldsNoEffect        （取代 04A 的 UnsupportedNode_Reject_...）
JewelSocket_Reject_LeavesPointsStateAndResultsUnchanged      （新增：把负对照换到真正非通行的节点）
SupportedNode_Allocate_ChangesActualPlayerAndSkillResult     （原样保留，正对照）
Mastery_Unavailable_And_NoEffect                             （原样保留）
```

同时跑通的既有回归（节选，均为 PASS）：`ProductionSimulatorTests` 4/4、`PassiveAwareProductionSimulationTests`、
`PassiveCensusTests`、`PassiveSupportTests`、`PoeTreeTests`、`S3R2FireConversionTests`、
`ContentAuditS2Tests`、`SupportCatalogInvariantTests`、`SupportGateTests`、`SliceLoopTests`、`S5U*`。
**未通过验收依据「覆盖率提升」** —— 判据是上表 20 条 AC 逐条可观测。

---

## J. ProdSim canonical

```text
entry cold ×3 : Run1 11:00:46 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
                Run2 11:01:00 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
                Run3 11:01:14 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
exit  cold ×3 : Run1 10:53:54 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
                Run2 10:54:08 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
                Run3 10:54:23 | exit=0 | contract=pv|passive-v1 | invalid=0 | FNV1A64:ec1d3ed67d3035d0
Distinct hashes : 1 ; EXACT MATCH : YES（入口与出口同为 FNV1A64:ec1d3ed67d3035d0）
```

入口证据 `_wo04a2_cold_entry.txt`；出口证据 `_wo04a2_cold_exit.txt`。
**未 rebaseline**。`PassiveAwareProductionSimulation.ModifierTuples` 改为与消费门同判据，
对 canonical fixture 取值不变 —— 哈希实测未动，即为该改动的证明。

---

## K. ProdSim sensitivity

```text
scenario            : 冻结路径 [2172, 71, 183]；State A = 71 之前的前缀（空，仅起点）；State B = A + 71
seed                : both 20260911u（equipment / skill / support / enemy / tick schedule 全同）
exact selected delta: +1 node → 71
route-only node     : 71
ps : DIFFERENT
pm : EXACT MATCH
pe : EXACT MATCH
pk : EXACT MATCH
```

该 sensitivity scenario **未被写成新的 canonical baseline**。
测试：`ProdSimSensitivity_RouteOnlyAllocationChangesIdentityOnly`。

---

## L. Census preservation

```text
FULLY_SUPPORTED 367 ／ UNFULFILLED 1660 ／ SPECIAL_BLOCKED 402（87 + 315）
NO SILENT CENSUS UPDATE
```

`TruthMatrix_MatchesContract_And04ACensusPreserved` + `PassiveSupportTests.EveryOnTreeNodeHasDeterministicSupportTruth`
双重钉死；`PassiveCensus` 的节点资格仍委托同一 owner。

---

## M. Scope audit

```text
no new StatId                 : ✔（git diff 无新增 StatId 引用；AffixApplicabilityTests 词表护栏仍 PASS）
no new ModOp                  : ✔
no parser expansion           : ✔（PoeStatParser.cs 零改动）
no Mastery selector           : ✔（node 10 仍 SPECIAL_PENDING / SPECIAL_BLOCKED）
no Mastery choice application : ✔
no Jewel gameplay/insertion/radius : ✔（57 珠宝孔仍 SPECIAL_BLOCKED）
no Timeless mechanism         : ✔（30 个 locked 节点仍 SPECIAL_BLOCKED）
no Ascendancy / Flask gameplay: ✔
no new skill                  : ✔
no new elemental axis         : ✔
no third connection group     : ✔
no persistence / save         : ✔
no LOD                        : ✔
no framework / package / asset purchase : ✔
no canonical ProdSim rebaseline : ✔（入口 = 出口 = ec1d3ed67d3035d0）
no visual-coordinate connectivity : ✔（连通性只用 PoeNode.links；两份独立实现互证）
no supported redefinition to inflate numbers : ✔（367 未动）
no partial modifier on mixed node : ✔（§G）
```

新增内容仅限：通行/效果两个维度的 runtime 类型与查询、allocator 对新通行维度的消费、
census/test/evidence 所需观测、以及**不引入新玩法/新选择机制**的 UI 状态表达（tooltip 文案 + 区分配色）。

---

## N. Governance handoff

`开发计划/UNATTENDED_STATE.md` 已更新为：

```text
S6P-WO-04A2 = READY_FOR_GATE_REVIEW
S6P-WO-03   = BLOCKED_PENDING_CHANNEL_A_ACCEPT
```

**未自动开始 WO-03**（未实现 Mastery explicit selection、未建 choice ID/状态、未把任何专精从
`SPECIAL_PENDING` 转成可兑现；22 个可兑现 choice 一个未动）。
Channel A 记录：本轮规划渠道已切换到**新会话** `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`
（按导演指令不复用旧会话 `6aa0dbbf-…`）。

---

## 附：STOP 条件自检（合同 §19）

| # | 条件 | 实际 |
|---|---|---|
| 1 | 入口 census ≠ 367/1660/87/315 | 一致 → 未触发 |
| 2 | 旧可达性不重现 14/2429 | 重现 14 → 未触发 |
| 3 | canonical 入口 hash ≠ ec1d3ed67d3035d0 | 相等（×3） → 未触发 |
| 4 | 找不到真实 route-only recovery fixture | 找到 183（候选 311） → 未触发 |
| 5 | 必须改 parser/StatId/ModOp 才能完成 | 未改 → 未触发 |
| 6 | 必须让 Mastery/Jewel/Timeless 可通行才能达标 | 未让 → 未触发 |
| 7 | route-only 出现 gameplay modifier | 0 → 未触发 |
| 8 | mixed 泄漏部分 modifier | 0 → 未触发 |
| 9 | special reject 时扣点或写入 selected | Δ=0 → 未触发 |
| 10 | §7 可达性 < 100% | 100.000% → 未触发 |
| 11 | 04A census 任一数字变化 | 未变 → 未触发 |
| 12 | canonical ProdSim 出口 hash 变化 | 未变 → 未触发 |
| 13 | sensitivity `ps\|` 不变化 | DIFFERENT → 未触发 |
| 14 | sensitivity `pm/pe/pk` 任一变化 | EXACT MATCH → 未触发 |
| 15 | 通行与效果仍实际依赖同一 authority bool | 已分离 + 源码扫描取证 → 未触发 |
| 16 | 需越过禁止域才能「修好」 | 未越过 → 未触发 |
