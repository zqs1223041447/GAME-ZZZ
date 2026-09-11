# S6P-WO-03 Evidence Pack

**Work Order:** S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness  
**Revision / Working-Tree Fingerprint:** 见 `_wo03_fingerprint_entry.txt`（生产树 `fd90468e…` @ `a91e391` dirty=NO；治理入口 `c16d129`）  
**Final fingerprint:** `_wo03_fingerprint_final.txt` = `4f826d08e7bd544b8aa1e815d463d7abb15d9efc6799ffb43e653b33fb77c45c`

## 入口

| 项 | 值 |
|---|---|
| Entry Working-Tree Fingerprint | `fd90468eb7b9ebdc95cb247c5c14d51c02a0c99092154ac0d5516ebaad88fb0a`（clean HEAD `a91e391`） |
| Entry Changed-File Inventory | 治理文档 only（见指纹文件）；产品 mutation 从该点之后开始 |
| Entry EditMode | 475/475 |
| Entry PlayMode | 18/18 |
| Entry Canonical Hash | `FNV1A64:ec1d3ed67d3035d0` |

## Mastery Choice Census

| 项 | 值 |
|---|---|
| Total Masteries | 315 |
| Total Choices | 1863 |
| Fully Supported Choices | 22 |
| Blocked Choices | 1841 |
| Masteries >=1 Supported | 22 |
| Masteries >=2 Supported | 0 |
| Masteries Zero Supported | 293 |
| PassiveSupport Owner Used | YES（`ClassifyLine` / `IsChoiceSelectable`） |
| Second Mastery Support Oracle Found | NONE |

## Identity / Serializer

| 项 | 值 |
|---|---|
| Choice Identity Scheme | `fallback:MasteryNodeId+sourceOrdinal (source choice IDs=NONE)` |
| Source Choice Stable IDs | NONE（payload 只有换行文本） |
| Serializer Stable-Key Scheme | numeric StatId/OpId/nodeId/ordinal |
| Float Canonical Encoding | `ToString("R", InvariantCulture)` |
| Enum Display Names In Hash | NONE（V3 已移除） |
| Duplicate Multiplicity Preserved | YES（ModRow 稳定排序含 tags/condition） |

## Prerequisite

| 项 | 值 |
|---|---|
| Prerequisite Owner | `PassiveSupport.MasteryPrerequisiteMet` |
| Prerequisite Source Relation | official `PoeNode.group` + allocated non-Mastery Notable |
| Same-Cluster Positive | PASS（node 10 / group 741） |
| Unrelated-Cluster Negative | PASS |
| No-Notable Negative | PASS |

## Canonical Mastery Fixture

| 项 | 值 |
|---|---|
| Mastery NodeId | 10 |
| Prerequisite Notable | group 741 最小 Notable（经 domain `TryAllocate` 路径） |
| Selected ChoiceKey | `10:<supported-ordinal>`（`+30 to maximum Life`） |
| Selected semantic tuples | `Life:Flat=30` 恰好一次 |
| Alternative Supported Choice | NONE（§13 fallback） |

## Atomic / Effect

| 项 | 值 |
|---|---|
| Open Selector Point Delta | 0（HUD only） |
| Cancel Point Delta | 0 |
| Success Point Delta | exactly 1 |
| Failed Point Delta | 0 |
| Missing Selection Effect | 0 |
| Selected Choice Count | 1 |
| Selected Effect Count | 1 |
| NonSelected Effect Count | 0 |
| Partial Choice Application | 0 |
| R Reset | allocation+selection+effect 全清 |
| Reselection Rule | R only；无 refund |
| Implicit First Choice Before | 0（04A 已拆） |
| Implicit First Choice After | 0 |

## Topology / Census

| 项 | 值 |
|---|---|
| ALLOCATABLE_SUPPORTED | before 367 / after 367 / delta 0 / reason=EA-4 |
| SPECIAL_PENDING_MASTERY | before 315 / after 315 / delta 0 / reason=EA-4 |
| Reachable | 1985 unchanged |
| \|D\| start-connected supported | 325 unchanged |
| start-disconnected supported | 42 unchanged |
| Mastery transit | NO（EA-2） |

## ProdSim

| 项 | 值 |
|---|---|
| ProdSim Contract Version | V3 |
| Payload marker | `pv\|passive-v2` |
| Predecessor Hash | `FNV1A64:ec1d3ed67d3035d0` |
| Attribution B | serializer cleanup（enum 名移除 + 空 `px|`）⇒ 默认 hash `99f1bfd3f81c4fe6` |
| Attribution A | unselected vs selected node 10 ⇒ payload/hash/Life 均变 |
| UI-Only Hash Invariance | selector 不进 session |
| Cold Process Run1 | 2026-09-11 14:27:08 / 15.7s / exit=0 / contract=V3 / invalid=0 / `FNV1A64:99f1bfd3f81c4fe6` |
| Cold Process Run2 | 2026-09-11 14:27:22 / 14.5s / exit=0 / contract=V3 / invalid=0 / `FNV1A64:99f1bfd3f81c4fe6` |
| Cold Process Run3 | 2026-09-11 14:27:37 / 14.2s / exit=0 / contract=V3 / invalid=0 / `FNV1A64:99f1bfd3f81c4fe6` |
| All Three Exact Match | YES |
| New Canonical Hash | `FNV1A64:99f1bfd3f81c4fe6` |

## Gates

| 项 | 值 |
|---|---|
| EditMode | **489/489** fail=0 skip=0 |
| PlayMode | **19/19** fail=0 skip=0 |
| Content Audit | PASS / fresh |
| Canonical Data Delta | NONE |
| Content Delta | NONE |
| Parser/Stat expansion | NONE |
| Drift | 0 |
| SoT Conflicts | 0 |
| Forbidden Expansion | PASS |

## Changed files（实现期）

- Runtime: `PassiveSupport.cs`, `SliceSession.cs`, `SliceHud.cs`, `PoeTree.cs`（注释）
- Tests: `S6PWo03MasteryTests.cs`+, `PassiveAwareProductionSimulation.cs`, `ProductionSimulator.cs`, census/support/playmode/S3R2
- Docs: RUNTIME.md, capability ledger, WO-03 记录/证据/amendment

## Recommended Next WO

S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity（若 Gate ACCEPT）
