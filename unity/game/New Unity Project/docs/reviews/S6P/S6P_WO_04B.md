# S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity

**状态：** READY_FOR_GATE_REVIEW  
**合同：** `S6P_WO_04B_CONTRACT.md`  
**入口：** HEAD `88e8202` / fingerprint `f9a79838…` dirty=NO  
**ProdSim：** 期望且实测保持 `FNV1A64:99f1bfd3f81c4fe6`（V3）

## 做了什么

1. 机械扫描现役 identity 表面，分类见 `docs/qa/WO_04B_BUILD_IDENTITY_SCAN.md`。`UNRESOLVED=0`。
2. 现役 **没有** `PassiveMask`（MIGRATED）。exact identity = `Allocated[]` + `MasteryChoice[]`。`PassiveHash` 只是 FNV1A64 指纹。
3. **没有** 给 `BuildSnapshot` 加 NodeId 列表（合同禁止把它变成 save object）。
4. 公开 live 投影：`CanonicalAllocatedIds` / `CanonicalMasterySelections` / `ComputePassiveHash`（测试与 Capture 共用同一 hash 算法）。
5. 契约测试覆盖 F0/F1(71)/F2(10:2)/lock 全 mutator/Cleared 再进图拒绝/ResetTown/death respec/ProdSim 不读 snapshot。
6. Clone / Persistence = N/A，未发明。

## 没做什么

Save/Load、clone 子系统、V3 bump、rebaseline、Jewel/Timeless、改 Mastery 语义。

## 门

| 项 | 值 |
|---|---|
| EditMode | **506/506** |
| PlayMode | **21/21** |
| Content Audit | PASS（EditMode 再生） |
| ProdSim 同进程 | `FNV1A64:99f1bfd3f81c4fe6` UNCHANGED |
| 冷进程 ×3 | `FNV1A64:99f1bfd3f81c4fe6` EXACT MATCH（15:38–15:39） |
| Topology | 1985 / 325 / 42 未测漂移（04B 测试未改 PassiveSupport 分类） |
| Census | 367 / 315 未改 |

## F2 冻结

- Notable **1006** Blood Drinker（group 741 最小 Notable）
- Ordinal **2** = `+30 to maximum Life`；ChoiceKey `10:2`
