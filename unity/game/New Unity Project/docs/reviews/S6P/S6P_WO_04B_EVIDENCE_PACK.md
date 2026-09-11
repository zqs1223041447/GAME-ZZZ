# S6P-WO-04B Evidence Pack

**Work Order:** S6P-WO-04B  
**Base commit:** `88e8202`  
**Status:** READY_FOR_GATE_REVIEW（successor BLOCKED pending Channel A）

## EP-A Entry

| 项 | 值 |
|---|---|
| Entry fingerprint | `f9a798382c26efcff0978d5fd1499c9b950184ccfc093bdabfd494d8d2f0ff9c` dirty=NO @ `88e8202` |
| Entry EditMode/PlayMode | 490/490 · 20/20（WO-03 ACCEPT 态） |
| Entry ProdSim | `FNV1A64:99f1bfd3f81c4fe6` V3 |

## EP-B Scan

见 `docs/qa/WO_04B_BUILD_IDENTITY_SCAN.md`

| SAFE_CURRENT | MIGRATED | N/A | UNRESOLVED |
|---|---|---|---|
| 多数现役表面 | PassiveMask | Clone / Save / File persist | **0** |

## EP-C Mask

- Current PassiveMask = ABSENT  
- Historical PassiveMask = MIGRATED (S5U-WO-04 → PassiveHash long)  
- Exact allocation authority = `Allocated[]` + `MasteryChoice[]`  
- PassiveHash = derived fingerprint, not reconstructable identity  

## EP-D Fixtures

| | allocated | mastery | notes |
|---|---|---|---|
| F0 | start 2172 | none | fresh seed 11 |
| F1 | +71 | none | route-only, hash 变, effect 0 |
| F2 | +path to 1006 + node 10 | `10:2` Life:Flat=30 | Notable=1006 Blood Drinker, ordinal=2 |

## EP-E Snapshot schema

14 fields only: Locked, 6× Equipped index, Q0/Q1/W0/W1/E0, PassiveHash, Unspent.  
`*Id` = `Equipped[]` 槽位索引，不是 ItemInstance.Id。  
**不是 SAVE/RESTORE。**

## EP-F Lifecycle

Town: BuildLocked=false. EnterMap: InMap + Locked=true. Cleared: BuildLocked=false, Snapshot.Locked=false, OnMap=true, re-enter rejected. Exit→Town. Death Exit→Dead + TryRespec.

## EP-G Lock matrix

InMap: TryEquip / TrySetSupport / TryReassignLink / TryAllocate / TryAllocateMastery / TryRespec / TryRandomCraft / TryDirectedCraft = REJECT + ZERO DELTA. Permission owner = `BuildLocked` (`State==InMap`) only.

## EP-H Clone / Persistence

NOT_APPLICABLE_WITH_SOURCE. 未新增 API。

## EP-I Serializer

ProdSim reads BuildSnapshot = **NO**. Snapshot PassiveHash == ProdSim hash = **NOT A REQUIREMENT**. F1 `ps|` contains 71; F2 `px|10:2`.

## EP-J Reset

TryRespec / ResetTown / death：allocation 仅 start；MasteryChoice 全 -1。

## EP-K Gates

EditMode **506/506**. PlayMode **21/21**. Content Audit PASS. Topology/census 未授权移动. Predecessor 未删。

## EP-L ProdSim

同进程 hash `FNV1A64:99f1bfd3f81c4fe6` UNCHANGED。

冷进程 ×3 EXACT MATCH YES：

```
Run1 | 2026-09-11 15:38:31 | 15.6s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run2 | 2026-09-11 15:38:45 | 14.3s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run3 | 2026-09-11 15:39:00 | 14.4s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
```


## EP-M Forbidden

Save/Load/Persistence/Clone invention/V3 bump/rebaseline/Jewel/Timeless/FirstChoice/LOD = NONE.

## EP-N Handoff

`S6P-WO-04B = READY_FOR_GATE_REVIEW`  
`successor = BLOCKED_PENDING_CHANNEL_A_GATE_REVIEW`
