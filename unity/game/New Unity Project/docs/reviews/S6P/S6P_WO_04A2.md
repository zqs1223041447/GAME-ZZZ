# S6P-WO-04A2 — Passive Traversal / Effect Separation

| 项 | 值 |
|---|---|
| 令号 | **S6P-WO-04A2** |
| 状态 | **CLOSED / ACCEPT** —— Channel A Gate Review（审 `c57f55f`）= **ACCEPT，AC-1..AC-20 = 20 / 20 PASS**；Product rework = NONE；Rebaseline = NONE；Blocking technical follow-up = NONE。原文：`S6P_WO_04A2_GATE_REVIEW.md` |
| 权威 | Channel A（话题 `game-zzz-planning-2` / 会话 `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`，2026-09-11 新建；取代旧会话 `game-zzz-planning`） |
| 合同原文 | `docs/reviews/S6P/S6P_WO_04A2_CONTRACT.md`（20 条 AC + PF-1..PF-6 + §7 可达性门 + §8 冻结 fixture + §14 敏感性 + §16 禁止项 + §18 Evidence 字段 + §19 自动停止条件） |
| 基准 commit | `ce6f85cd23d324ea68a76f98d28663f1258a3c01`（本地 = origin/main，已推送） |
| 前置 | S6P-DIR-01 Gate Addendum COMPLETE |
| 后继 | **S6P-WO-03 — BLOCKED pending Channel A Gate Review ACCEPT**（本令未开始 WO-03 的任何功能） |
| canonical ProdSim | 入口 `FNV1A64:ec1d3ed67d3035d0` ×3 ／ 出口 `FNV1A64:ec1d3ed67d3035d0` ×3 — **未变** |
| 入口指纹 | `6d2c9ae4536a17665851cadb3a72643fba3b34036c4fc65c2216d3e2d0c32b68`（2798 files / 321423157 bytes / dirty=NO / HEAD `ce6f85c`）——`_wo04a2_fingerprint_entry.txt` |
| 出口指纹 | `_wo04a2_fingerprint_final.txt`（覆盖范围与残留差异见 §13） |
| 产物 | `docs/qa/WO_04A2_TRAVERSAL_REPORT.json`（测试再生） |

---

## 1. 本令修的是什么

WO-04A 把「这个节点能不能作为被动树的**路径**」和「这个节点承诺的 gameplay 效果能不能**完整兑现**」
压成了同一个布尔值（`NodeStatus.AllocatableSupported`）。后果：

- 全树从起点只能点亮 **14 / 2429** 个节点（起点邻居只可点 3 个）；
- 367 个「可完整兑现」节点里有 311 个被 route-only 节点隔在孤岛上，玩家永远够不到。

本令把两者拆成两个**互相独立**的真值：

| 维度 | 值 | 判据 |
|---|---|---|
| `EffectTruth` | `FULLY_SUPPORTED` / `UNFULFILLED` / `SPECIAL_PENDING` | 只有 `FULLY_SUPPORTED` 可以贡献 modifier |
| `TraversalTruth` | `TRAVERSABLE` / `SPECIAL_BLOCKED` / `OUT_OF_DOMAIN` | 只有 `TRAVERSABLE` 可以分配、扣点、作为路径 |

**本令不是「提高被动效果支持率」的令**：367 这个数一个没动。

---

## 2. 真值矩阵（实现后的实际值，全部由测试机械核对）

| 节点类别 | EffectTruth | TraversalTruth | 可分配 | 点消耗 | modifier |
|---|---|---|---|---|---|
| 普通、效果可完整兑现 | `FULLY_SUPPORTED` | `TRAVERSABLE` | 是 | 1 | 完整生效 |
| 普通、效果不可完整兑现 | `UNFULFILLED` | `TRAVERSABLE` | **是（本令新增能力）** | 1 | **0（整节点）** |
| 普通、mixed（部分可兑现 + 部分不可） | `UNFULFILLED` | `TRAVERSABLE` | **是** | 1 | **0（整节点，禁止只吃那几条）** |
| 专精 Mastery | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | 否 | 0 | 0 |
| 珠宝孔 Jewel Socket | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | 否 | 0 | 0 |
| 时光珠宝类（`locked != 0`） | `SPECIAL_PENDING` | `SPECIAL_BLOCKED` | 否 | 0 | 0 |
| 域外（403 未上树节点） | `SPECIAL_PENDING` | `OUT_OF_DOMAIN` | 否 | 0 | 0 |

上树 2429 节点的实际交叉表：`367 + 1660 + 402 = 2429`，其中 402 = 87（珠宝孔 + 时光珠宝类）+ 315（专精过渡）。

---

## 3. Preflight（PF-1..PF-6，全部 PASS，逐条有证据）

| 项 | 要求 | 实测 | 结论 |
|---|---|---|---|
| PF-1 | 基准身份 | branch `main`，HEAD `ce6f85c`，工作区 dirty=NO，Unity 6000.3.23f1，无其它未审代码 | PASS |
| PF-2 | 入口基线 | 编译 0 error；EditMode **462/462**；PlayMode **17/17** | PASS |
| PF-3 | 04A 冻结 census | 367 / 1660 / 87 / 315 四项复核一致 | PASS |
| PF-4 | 旧可达性 | 从 `StartNode=2172`、只用 canonical 图边测量：**14 / 2429** | PASS |
| PF-5 | canonical 入口 hash | 冷进程 ×3（11:00:46 / 11:01:00 / 11:01:14，各 exit=0、invalid=0、contract=`pv\|passive-v1`）全部 `FNV1A64:ec1d3ed67d3035d0`，EXACT MATCH | PASS |
| PF-6 | 恢复 fixture 候选 | 候选集 **311** 个（≥1） | PASS |

**PF-5 的取证方式（值得留档）**：入口冷进程基线必须在**生产改动之前**取得。
本轮的做法是把整个改动集 `git stash push -u` 暂存，在干净 HEAD `ce6f85c` 上跑三次独立冷启动，
再 `git stash pop` 复原（复原后 `git status --porcelain` 与暂存前逐字一致，stash 已 drop）。
这条路子不依赖任何历史猜测，也不用回滚提交，可直接复用。
工具：`tools/evidence/cold-process-prodsim.ps1`；输出：`_wo04a2_cold_entry.txt`。

---

## 4. 可达性门（合同 §7）

测量方法：把 canonical 图边（`PoeNode.links`，无向、已去重升序）当作唯一的连通性来源，
禁用坐标 / orbit / group 归属 / 视觉连线。两份**独立实现**互为 oracle：
生产的 `PassiveSupport.ReachableSet()` 与测试侧 `PassiveTraversal04A2.ReferenceReach()`，
测试要求两者**逐节点一致**（不一致即 FAIL）。

| 量 | 值 |
|---|---|
| canonical 节点数 | 2429 |
| canonical 边数（去重无向） | 2787 |
| 无边节点 | 30（全是 `locked != 0` 的时光珠宝类） |
| `TRAVERSABLE` | 2027 |
| `SPECIAL_BLOCKED` | 402 |
| `OUT_OF_DOMAIN` | 0 |
| `\|R_legacy\|`（旧行为参考可达） | **14** |
| `\|R_ref\|`（参考图可达，= 通行维度图） | **1985** |
| `\|R_runtime\|`（生产实现） | **1985**（与 `R_ref` 逐节点一致） |
| `\|D\|`（start-connected ordinary supported） | **325** |
| `\|D ∩ R_runtime\|` | **325** |
| 缺失集 `D - R_runtime` | **`[]`** —— **100.000% PASS** |

**关于 `\|R_ref\| == \|R_runtime\|` 的诚实说明**：按合同 §7.2 / §7.3 的字面定义，
参考图的 transit 集合（「普通、in-domain、删除 SPECIAL_BLOCKED 与 OUT_OF_DOMAIN」）
与生产通行规则的 transit 集合是**同一个集合**，所以两张图同构、可达集必然相同。
因此 AC-15 的 `\|D ∩ R_runtime\|/\|D\| == 1` 在这套定义下是**构造性成立**的；
它真正机械化的价值是「生产 BFS 没写错」+「D 已被穷举」+「`missingSupported` 为空必须逐次复核」。
真正体现本令收益的数字是 **14 → 1985**。若 Channel A 认为参考图应当更严格
（例如把 `R_ref` 定义成「所有 in-domain 节点都可作 transit，包括 special」），
请给出修正定义，本令可另开一轮补测。

---

## 5. 冻结的恢复 fixture（合同 §8，确定性发现后写死）

发现算法完全按 §8.3：候选集 `C = { t ∈ D : t ∉ R_legacy 且 canonical 最短路径上至少含一个 route-only 节点 }`，
邻接一律按 nodeId 升序 BFS，按 `(距离, nodeId)` 升序取唯一一个。

```text
StartNode              : 2172
候选集 |C|             : 311
FrozenTarget04A2       : 183
DistanceFrom2172       : 2
CanonicalPath          : [2172, 71, 183]
PathLength             : 3 nodes (2 hops)
RouteOnlyNodesOnPath   : [71]
FirstRouteOnlyNode     : 71
legacy reachable(183)  : false
runtime reachable(183) : true
```

含义：**node 71**（含 `Mana Regeneration` 等引擎兑现不了的行）在过去是死路；
本令后它是合法路径节点，玩家点它花 1 点、得 0 效果，但**因此够得到真实的可兑现节点 183**。
这正是「恢复」的完整语义：路径花钱、路径不生效、目标真生效。

冻结值写死在 `Assets/Tests/EditMode/S6PWo04A2Tests.cs` 的 `Frozen*` 常量里；
数据未变而 ID / 路径 / 长度发生任何漂移 → `RecoveryFixture_IsFrozen` FAIL。

---

## 6. 效果隔离（合同 §4 / AC-9）

对 route-only 节点（含 mixed 冻结样例 71 / 12 / 255），测试用「绕过分配门直接注入 `Allocated[]`，
再 `RecalcPlayer`」的方式取证：

| 面 | 结果 |
|---|---|
| `ps\|`（已分配身份） | **DIFFERENT** —— 确实进了 selected 集 |
| `pm\|`（实际生效 modifier 语义元组） | **EXACT MATCH** —— 一条都没漏 |
| `pe\|`（玩家有效属性快照） | **EXACT MATCH** |
| `pk\|`（技能侧有效属性快照） | **EXACT MATCH** |

`PassiveSupport.YieldsModifiers(EffectTruth)` 是消费门的**唯一判据**，
`SliceSession.RecalcPlayer` 与 `CollectSkillMods` 各自只读这一个维度；
`Assets/Tests/EditMode/S6PWo04A2Tests.GatesDoNotReadTheDiagnosticProjection` 用**源码扫描**
钉死「两个门都不再引用四值诊断投影 `EvaluateNode(...)`，也不存在 `IsAllocatable`」，
防止「注释写了改了、代码其实没改」。

---

## 7. 负向对照（AC-10..AC-13）

| 对照 | 结果 |
|---|---|
| 专精 node 10（相连性已置成立） | 拒绝，零扣点，`Allocated` 零写入，整份 `StatePayload` 一字不变，`CanAllocate=false`，有稳定原因 |
| 珠宝孔 node 78 | 同上 |
| 时光珠宝类（首个 `locked != 0` 节点） | 同上（其无连线 ⇒ 相连门先拒，且本身非通行） |
| 非相邻普通节点（可通行但不在起点连通域） | 拒绝，原因 `需与已点亮节点相连` |
| 点数不足（`Unspent=0`） | 拒绝，原因 `没有天赋点` |
| Mastery 效果 | 即便注入 `Allocated` 也 0 效果；`BlockedAllocatedCount` 从 0 变 1（invalid-state evidence 仍然有效） |

---

## 8. ProdSim（AC-18 / AC-19）

### 8.1 canonical（未变）

```text
入口冷进程 ×3 : FNV1A64:ec1d3ed67d3035d0（11:00:46 / 11:01:00 / 11:01:14）
出口冷进程 ×3 : FNV1A64:ec1d3ed67d3035d0（10:53:54 / 10:54:08 / 10:54:23）
每次 contract=pv|passive-v1, invalid=0, exit=0; Distinct hashes=1; EXACT MATCH=YES
```

`PassiveAwareProductionSimulation.ModifierTuples` 现在与消费门读**同一个**效果判据
—— 否则 route-only 节点会把它「可识别的那几条」谎报成生效效果，使 `pm|` 泄漏。
该改动对 canonical fixture（559/1795/2034 + 起点）取值不变，**哈希实测未动**，
不构成 rebaseline。

### 8.2 敏感性（§14，State A / State B）

State A = 冻结路径上 `FirstRouteOnlyNode`（71）之前的前缀；State B = A + 71。两次同 seed `20260911u`。

```text
selected-node delta : +1（node 71）
route-only node     : 71
ps : DIFFERENT        （身份必须对 route-only 分配敏感）
pm : EXACT MATCH
pe : EXACT MATCH
pk : EXACT MATCH
```

---

## 9. 04A 冻结 census 未动（AC-16）

```text
FULLY_SUPPORTED 367 ／ UNFULFILLED 1660 ／ SPECIAL_BLOCKED 402（87 + 315）  →  NO SILENT CENSUS UPDATE
```

`NodeStatus` / `EvaluateNode` 保留为**派生的诊断投影**（同一 `Classify` 实现同时产出两个维度与四值视图），
用来让 04A 的 census 数字继续可机械复核；它**不再驱动任何门**。

---

## 10. 为了适配新语义而合法更新的既有测试（非削弱）

| 测试 | 旧断言（04A） | 新断言（04A2） | 理由 |
|---|---|---|---|
| `S6PPassiveSupportPlayModeTests` node 71 | 分配被拒、零扣点、UI 为 Locked | **分配成功**、扣 1 点、进 selected 集、`pm/pe/pk` 零变化、UI 为 Available 且无禁用原因 | 产品规则合法反转：route-only 现在就是合法路径 |
| 同上（新增） | — | `JewelSocket_Reject_LeavesPointsStateAndResultsUnchanged` | 把「非通行必须拒绝」的负对照换到真正非通行的节点上，覆盖没有变空 |
| `S3R2FireConversionTests.SupportAndPassiveConversion_ComposeOnSameAxis` | Avatar of Fire：分配被拒、原因来自支持门 | 效果维度 `UNFULFILLED`、通行维度 `TRAVERSABLE`；分配被拒的原因改为**相连性**；另证注入后 0 modifier | 该节点是「效果未兑现的普通节点」的标准样例；转换轴聚合断言（0.50 + 0.50 = 1.00）原样保留 |
| `PassiveSupportTests.UnsupportedNodeTryAllocateRejectsAtomically` | 拒绝 + 原子零变化 | 改名 `RouteOnlyNodeTryAllocateSucceeds_AndSpendsExactlyOnePoint` | 同上 |
| `PassiveSupportTests.InjectedBlockedNodeContributesZeroModifiers` | `BlockedAllocatedCount == 2` | route-only 注入不算损坏（= 0）；真正的损坏（注入专精）必须 = 1 | `BlockedAllocatedCount` 语义已改为「不可通行却被点亮」 |
| `PassiveSupportTests.UIUsesDomainSupportTruth` | 可分配节点数 = 367 | 可通行 = 2027、不可通行 = 402，且原因只由通行维度产生 | 同一 truth 的两个维度分别核对 |
| `PoeTreeTests.StartNode_IsAllocatedOnReset_...` | blocked 邻居必须被拒 | blocked 邻居必须可作为路径点亮 | 同上 |
| `SliceLoopTests` / `PassiveCensus` 内的「第一个可分配邻居」helper | 取 `IsAllocatable` | 取「效果可完整兑现」(`YieldsModifiers`)，避免把断言建在 route-only 节点上 | 保持原测试的意图不变 |

**删除的测试：0。被静默弱化的断言：0。** 每一处改动都在上表列明，且新断言严格强于旧断言
（旧：拒绝且零变化；新：**允许且零变化** + 身份变化证明）。

---

## 11. 范围审计（AC-20）

| 项 | 结论 |
|---|---|
| 新 `StatId` | **0** |
| 新 `ModOp` | **0** |
| 新 parser 规则 | **0** |
| Mastery selector / choice 应用 | **0**（node 10 仍是 `SPECIAL_PENDING + SPECIAL_BLOCKED`） |
| Jewel 玩法 / 插孔 / 半径 | **0** |
| Timeless 机制 | **0** |
| 升华 / 药剂 / 新技能 / 新元素轴 | **0** |
| 第三连接组 | **0** |
| 存档 / 持久化 / LOD | **0** |
| 采购 / 换框架 / 换数据源 | **0** |
| canonical rebaseline | **0**（入口 = 出口） |
| 用视觉坐标造连通性 | **0**（连通性只用 `PoeNode.links`；两条独立实现互证） |
| 为凑数字重定义 supported | **0**（367 未动；`D` 按合同定义 = 325，且 42 个孤立 supported 已单独登记） |

---

## 12. 已知限制与开放项

1. **L1 — 42 个 supported 节点仍是 start-disconnected supported nodes**（起点非连通 supported 节点；
   `supportedNotStartConnected := 42`，ID 全表见 `docs/qa/WO_04A2_TRAVERSAL_REPORT.json`）。
   注意措辞：它们**不是** `OUT_OF_DOMAIN`（`outOfDomain = 0`）。它们被不可通行的特殊节点
   （珠宝孔 / 时光珠宝类）围住，按合同 §7.2 的 `D` 定义不参与"100% 可达"，但**玩家确实到不了**。
   本令不解（会越界到 Jewel/Timeless 域）——登记为后续 special-interaction topology 工作的输入，
   且不预承诺"实现 Jewel 就一定全部解开"。
2. **L2 — `EffectTruth == UNFULFILLED` 的节点仍会消耗天赋点**。这是合同 §6 的显式规定
   （route-only「normal cost」），不是 bug；但玩家可能觉得「花了点没效果」。
   UI 已如实标注（见下），若导演要求「route-only 免费」，需另开令改产品语义。
3. **L3 — `R_ref` 与 `R_runtime` 按合同字面定义同构**，AC-15 因此构造性成立（见 §4 的诚实说明）。
4. **L4 — 本轮未改 WO-03 遗留的非阻塞 follow-up**（`S3R2FireConversionTests` 的测试名仍在，
   但已按 04A 裁定在本轮顺带把它改成不再误导的注释与断言；如需彻底改名，WO-03 再动）。

### UI 表达（允许范围内的最小改动）

- tooltip：route-only 可点节点显示「可点亮（路径）· 消耗 1 天赋点；该节点当前无法兑现任何效果（0 效果）」；
  已点亮的 route-only 显示「已点亮（路径）· 该节点当前 0 游戏效果……」。**不静默空效果**。
- 配色：可点节点区分「金亮 = 真生效」与「冷灰蓝 = 可点但 0 效果」；已点亮的 route-only 压暗。
- 这些文本/配色只读 domain truth，UI 不自己判定。

---

## 13. 门禁结果（出口）

| 门 | 结果 |
|---|---|
| 编译 | **0 error** |
| EditMode | **475 / 475**（fail=0, skip=0；入口 462 + 本令新增 13） |
| PlayMode | **18 / 18**（fail=0, skip=0；入口 17 + 本令新增 1） |
| ProdSim canonical | 入口 ×3 = 出口 ×3 = `FNV1A64:ec1d3ed67d3035d0`，EXACT MATCH |
| ProdSim 敏感性 | `ps` DIFFERENT；`pm/pe/pk` EXACT MATCH |
| 可达性门 | `\|D\|` = 325，缺失集 `[]`，100.000% |
| 04A census | 367 / 1660 / 87 / 315 **未变** |
| 入口指纹 | `6d2c9ae4536a17665851cadb3a72643fba3b34036c4fc65c2216d3e2d0c32b68`（HEAD `ce6f85c`，dirty=NO） |
| 出口指纹 | `_wo04a2_fingerprint_final.txt` |

### 出口指纹的覆盖范围声明（避免"指纹与状态不唯一对应"）

`_wo04a2_fingerprint_final.txt` 记录的那次测量覆盖的工作区状态是：
**WO-04A2 的全部产品代码、测试、文档与证据文件都已就位**（含本文件与
`S6P_WO_04A2_EVIDENCE_PACK.md`）。
按「清单文件不能包含自己的哈希」这一标准约定，该测量**唯一排除**的是
`_wo04a2_fingerprint_final.txt` 自身（它由该次测量的输出写入）。
指纹输出版面内的 `changed-file inventory` 逐行列出了被测文件清单，故被测状态可完整重建 ——
不存在「测完之后又改了被测内容」的情形。

### 13.1 Gate Review 后的 UI 缺陷修复（主动申报，见 `S6P_WO_04A2_GATE_REVIEW.md` 末节）

Gate Review 判定「route-only 的 UI 明示是产品语义成立所必需的 disclosure」并 ACCEPT 之后，
本轮做**实机视觉取证**时发现该 disclosure 实际上**看不见**：

- 根因：`SliceTooltipModel.TextCard(title, body)` 的第二参数落在**单行 Subtitle 槽**，
  多行字符串在那里会被裁掉 —— 我写的 `body + "\n\n" + state` 只显示出词条，状态行被吃掉。
- 修复（仅 `SliceHud.NodeCard`，呈现层）：节点 tooltip 正文改为**按行拆进 `Body[]`**
  （与既有 `DrawSkillCell` 同一做法，`Body.Length` 参与卡片高度计算）；状态行拆成两行短句
  以适应 `SliceTooltipLayout.BaseW`(360) 的宽度；新增 `AppendLines` 逐行拆分词条。
  顺带把状态行**放在正文第一行**，任何情况下都不会再被词条挤掉。
- 影响面：`Assets/Runtime/Core/Gameplay/SliceHud.cs` 一个文件，不产生 gameplay 语义，
  与分配门 / 消费门 / ProdSim 零接触。
- 重跑门：编译 0 error、EditMode **475/475**、PlayMode **18/18**（数字不变）。
- 取证：`docs/_dirshots/s6pwo04a2/01_tree_route_only_tooltip.png`（route-only：两行披露 + 词条）
  与 `02_tree_effective_tooltip.png`（真生效：仅「可点亮 · 消耗 1 天赋点」）。
  取景方式：`SliceHud.DebugTreeAt(0.9f, <node>)` + `SliceHud.DebugHoverAt(视口中心)`
  + `ScreenCapture.CaptureScreenshot`（IMGUI 不进 `capture_game_view`，必须走 ScreenCapture）。
- 已请 Channel A 确认这处修复是否需要第二次 Gate Review。

---

## 14. 改动文件清单

**运行期（3）**

| 文件 | 改动 |
|---|---|
| `Assets/Runtime/Core/Gameplay/PassiveSupport.cs` | 新增 `EffectTruth` / `TraversalTruth` / `NodeTruth`；`EvaluateTruth` / `IsTraversable` / `YieldsModifiers` / `TraversalReason` / `ReachableSet`；`ClassifyTruth` 成为唯一分类实现，`Project` 派生 04A 四值诊断视图；**删除** `IsAllocatable(NodeStatus)` |
| `Assets/Runtime/Core/Gameplay/SliceSession.cs` | `TryAllocate` / `CanAllocate` 改读通行维度；`RecalcPlayer` / `CollectSkillMods` 改读效果维度；`NodeBlockReason` 语义改为「不可通行的原因」；`BlockedAllocatedCount` 语义改为「不可通行却被点亮」；新增 `NodeTruth(int)` |
| `Assets/Runtime/Core/Gameplay/SliceHud.cs` | tooltip 两维度如实表达；`FrameTint` / `IconTint` 增加 route-only 区分色 |

**测试（11，含 2 个新文件）**

新增：`Assets/Tests/EditMode/PassiveTraversal04A2.cs`（纯计算参考遍历 + fixture 发现 + 产物渲染）、
`Assets/Tests/EditMode/S6PWo04A2Tests.cs`（13 条门禁）。
修改：`PassiveAwareProductionSimulation.cs`、`PassiveCensus.cs`、`PassiveSupportTests.cs`、
`PoeTreeTests.cs`、`S3R2FireConversionTests.cs`、`SliceLoopTests.cs`（EditMode）；
`S6PPassiveSupportPlayModeTests.cs`（PlayMode）。

**文档/证据（6）**：本文件、`S6P_WO_04A2_CONTRACT.md`、`_wo04a2_fingerprint_entry.txt`、
`_wo04a2_fingerprint_final.txt`、`_wo04a2_cold_entry.txt`、`_wo04a2_cold_exit.txt`；
产物 `docs/qa/WO_04A2_TRAVERSAL_REPORT.json`。

---

## 15. 复现命令

```powershell
cd G:\GAME-ZZZ

# 入口冷进程（需先关闭编辑器）—— 干净 HEAD 上跑
unity close "G:\GAME-ZZZ\unity\game\New Unity Project"
pwsh -NoProfile -File tools/evidence/cold-process-prodsim.ps1 -Runs 3

# 全量 EditMode / PlayMode
unity command editor_stop
unity command run_tests --mode EditMode --timeout 2400
unity command run_tests --mode PlayMode --async_tests true
unity command test_status

# 本令门禁单跑
unity command run_tests --mode EditMode --filter S6PWo04A2Tests --timeout 900

# 指纹
pwsh -NoProfile -File tools/evidence/working-tree-fingerprint.ps1
```
