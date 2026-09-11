# WO_04B_BUILD_IDENTITY_SCAN

| symbol/path | classification | authority | readers | writers | disposition |
|---|---|---|---|---|---|
| `PassiveMask` | **MIGRATED** | historical int ≤32 | none in production | none | S5U-WO-04 → `PassiveHash`; production symbol=0 |
| `BuildSnapshot.PassiveHash` | **SAFE_CURRENT** | DERIVED_FINGERPRINT, not exact identity | map-entry snapshot | `Capture()` | FNV1A64 of allocated NodeId + (choiceOrdinal+1) |
| `Allocated[]` | **SAFE_CURRENT** | exact live identity | allocate/consume/UI/ProdSim | TryAllocate/Mastery/Respec/ResetTown | bool[PassiveCount]=2429 |
| `MasteryChoice[]` | **SAFE_CURRENT** | exact live identity | consume/hash/UI | TryAllocateMastery/Respec/ResetTown | int[], -1=none |
| `BuildSnapshot` schema | **SAFE_CURRENT** | MAP-ENTRY CAPTURE SUMMARY | TryEnterMap consumers | Capture + Locked flags | 14 public fields; no NodeId list; not Save |
| `Capture()` | **SAFE_CURRENT** | derived | TryEnterMap | live Equipped/supports/hash/Unspent | does not persist files |
| Clone / MemberwiseClone / Copy API | **NOT_APPLICABLE_WITH_SOURCE** | — | — | — | Runtime Gameplay 无 clone seam |
| Serialize/Deserialize/PlayerPrefs | **NOT_APPLICABLE_WITH_SOURCE** | — | — | — | 无 session persistence |
| `File.WriteAllText` | **NOT_APPLICABLE_WITH_SOURCE** | perf only | ArenaPerfHarness, PerfSampler | perf reports | 非 build 存档 |
| `TryEnterMap` / `ExitMap` / `BuildLocked` | **SAFE_CURRENT** | lock permission = `State==InMap` | all mutators | State | Snapshot.Locked is metadata only |
| TryAllocate / TryAllocateMastery / TryRespec / TryEquip / TrySetSupport / TryReassignLink / TryRandomCraft / TryDirectedCraft | **SAFE_CURRENT** | BuildLocked gate | UI/tests | session | InMap = reject + zero delta |
| `ResetTown` / death TryRespec | **SAFE_CURRENT** | reset | session | Allocated/MasteryChoice/Snapshot | no residue |
| ProdSim `ps\|`/`px\|` | **SAFE_CURRENT** | live projection | ProductionSimulator | Allocated + MasteryChoice | 不读 BuildSnapshot |

**UNRESOLVED_WITH_SOURCE = 0**

Frozen F2: PrerequisiteNotableNodeId=**1006** (Blood Drinker, group 741); SupportedChoiceOrdinal=**2**; ChoiceKey=`10:2`.
