S6P-WO-04B CONTRACT
S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity
S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity
Authority: Channel A / 
game-zzz-planning-2
Status: 
RELEASED / EXECUTE NOW
Base: 
main@88e8202
Canonical ProdSim: 
V3 / FNV1A64:99f1bfd3f81c4fe6
Scope: 仅审计/最小修复仓库已经存在的 
mask / fixed-width identity / allocation identity / snapshot / clone / map-entry capture / build lock / serializer / reset。
明确禁止: 发明 Save / Persistence。
当前 
BuildSnapshot 只包含 
Locked、六个装备槽字段、
Q0/Q1/W0/W1/E0、
PassiveHash、
Unspent；当前生产 passive state 为 
Allocated[] + MasteryChoice[]。
PassiveHash 由 
Capture() 对升序 allocated NodeId 做 FNV1A64，并在该节点存在显式 Mastery choice 时继续混入 
choiceOrdinal + 1。
1. Mission
本令只回答以下问题：
已有 passive build 在 live allocation identity、定宽 fingerprint、map-entry snapshot、build lock、V3 serializer 与 reset 各表面之间，是否保持一致、确定、无截断、无遗失、无锁绕过？
不是本令目标：
新 SaveGame；
session persistence；
runtime serialize/deserialize；
restore from snapshot；
新 clone subsystem；
新 passive gameplay；
新 serializer version。
BuildSnapshot.PassiveHash 是摘要/fingerprint，不是能够无损重建 2429 节点 allocation 的容器。不得因为 snapshot 没有 NodeId 列表，就把本令扩成存档系统。
2. 强制入口门
任何产品 mutation 前记录：
HEAD / branch；
git status --porcelain；
working-tree fingerprint；
Unity version；
discovered EditMode / PlayMode manifest；
Content Audit；
当前 V3 canonical hash。
入口必须：
Compile = 0 error
EditMode = ALL PASS / fail=0 / skip=0
PlayMode = ALL PASS / fail=0 / skip=0
Content Audit = PASS / fresh
ProdSim = V3
Hash = FNV1A64:99f1bfd3f81c4fe6
入口 canonical 至少实跑 1 次。
若入口 hash 不是 
99f1bfd3f81c4fe6：
STOP。不得实施，不得自行 rebaseline。
3. 扫描分类枚举
本令所有命中必须使用且只能使用下列四类：
Classification
精确定义
SAFE_CURRENT
当前 production surface 存在，并满足本合同；只需机械证明，不需为它发明新机制
MIGRATED
历史风险真实存在过，但当前 production 已被既有工作令迁移；必须给旧→新 source 证据，不能当现役 defect 修第二遍
NOT_APPLICABLE_WITH_SOURCE
本合同要求审计的 surface 在当前 runtime 中不存在；必须给机械搜索/source evidence，禁止为了 checklist 新建
UNRESOLVED_WITH_SOURCE
当前 production surface 确实存在且与本合同不一致；必须给 source + 最小修复；出口不得残留此分类
出口要求：
UNRESOLVED_WITH_SOURCE = 0。
不得使用：
UNKNOWN；
ASSUMED；
NOT CHECKED；
“看起来没问题”。
4. 强制扫描面
开工时生成 
WO_04B_BUILD_IDENTITY_SCAN，至少扫描：
PassiveMask
PassiveHash
BuildSnapshot
Allocated
MasteryChoice
Capture
Clone
Copy
MemberwiseClone
Serialize
Deserialize
File.Write
FileStream
PlayerPrefs
TryEnterMap
ExitMap
BuildLocked
Snapshot.Locked
TryAllocate
TryAllocateMastery
TryRespec
TryEquip
TrySetSupport
TryReassignLink
TryRandomCraft
TryDirectedCraft
ResetTown
StatePayload
ps|
px|
每条结果记录：
symbol/path
classification
authority / derived / metadata / evidence-only
reader(s)
writer(s)
disposition
5. 已冻结的预期分类
5.1 
PassiveMask
当前 production 不存在 
PassiveMask。
历史 
BuildSnapshot.PassiveMask : int 的 ≤32 节点风险已由 S5U-WO-04 迁移为 
PassiveHash : long。
因此本令预期：
PassiveMask = MIGRATED
验收不是“重新实现 PassiveMask”，而是证明：
production symbol = 0；
没有任何 32-bit passive allocation mask 回流；
历史证据能定位迁移关系。
发现现役 
PassiveMask / 
1 << nodeId / 等价 32-bit allocation identity：
STOP / UNRESOLVED_WITH_SOURCE。
5.2 
PassiveHash
现役：
BuildSnapshot.PassiveHash : long
分类：
SAFE_CURRENT
DERIVED_FINGERPRINT
NOT EXACT IDENTITY AUTHORITY
Capture() 当前对 
Allocated[] 按 NodeId 升序迭代；每个 allocated node 混入 NodeId；若该节点 
MasteryChoice >= 0，继续混入 
choiceOrdinal + 1。
不得声称：
PassiveHash equality => mathematically collision-free exact equality
本令只要求：
同一合法 build → 同一 fingerprint；
冻结的 identity mutation → fingerprint 有敏感性；
不使用 fingerprint 反向恢复 allocation。
6. Canonical live allocation identity
唯一 live authority 保持：
Allocated[nodeId]
+
MasteryChoice[nodeId]
其中：
Allocated=true 决定 NodeId 是否属于 passive allocation identity；
普通节点 
MasteryChoice=-1；
已显式选择的 Mastery 使用 WO-03 冻结的 authoritative source ordinal；
WO-03 当前数据无 per-choice stable ID，因此 identity 为：
MasteryNodeId + sourceOrdinal；
StartNode 保持既有 runtime 语义。
V3 serializer 已分别输出排序后的 
ps| allocated NodeIds 与 
px| explicit Mastery choices。
PassiveHash、
BuildSnapshot、ProdSim payload 都只能是这个 live truth 的派生消费者，不得反向拥有 allocation。
7. 冻结 fixtures
所有 fixture 必须通过真实 domain/public API 构造；禁止直接写数组来制造正向证据。
F0 — Fresh baseline
同 seed 新建 session。
冻结并记录：
StartNode；
sorted allocated IDs；
all MasteryChoice values；
Unspent；
PassiveHash（通过真实 map-entry capture 获得）；
V3 
ps| / px|。
F1 — Route-only identity
固定：
NodeId = 71
通过合法路径分配。
必须证明：
Allocated[71]=true；
Node 71 出现在 allocation identity；
snapshot 
PassiveHash 相对 F0 改变；
V3 
ps| 改变；
route-only gameplay effect 仍为 0。
不得因为它“0 效果”而从 identity/snapshot fingerprint 消失。
F2 — WO-03 Mastery identity
固定：
Mastery NodeId = 10
Group = 741
Choice text = "+30 to maximum Life"
Effect = Life:Flat=30
prerequisite 使用既有 WO-03 规则：
group 741 中按 authoritative source 得到的最小 non-Mastery Notable，经真实 allocation path 分配。
在本令 Preflight 中机械求出并冻结：
PrerequisiteNotableNodeId = <exact>
SupportedChoiceOrdinal = <exact source ordinal>
ChoiceKey = 10:<ordinal>
不得猜 ordinal，不得用 GUI index。
WO-03 已冻结 node 10 / group 741 / 
+30 to maximum Life fixture。
成功 explicit selection 后必须证明：
Allocated[10]=true；
MasteryChoice[10]=SupportedChoiceOrdinal；
exact identity 包含 
10:<ordinal>；
PassiveHash 相对未选择状态变化；
V3 
ps| 包含 10；
V3 
px| 包含 
10:<ordinal>；
Life +30 恰好一次。
F3 — Map-entry capture
以包含 F1 + F2 的合法 build 进入地图。
在 
TryEnterMap 前冻结 live state；成功调用后冻结 snapshot。
当前代码顺序为：
Snapshot = Capture()
Snapshot.Locked = true
State = InMap
必须逐字段比对 snapshot 与进入前 live build。
F4 — Reset
从含：
ordinary allocation；
route-only 71；
explicit Mastery 10；
的合法状态执行现有 reset surface。
分别审计：
TryRespec
ResetTown
death-triggered respec
不得新建第四种 reset。
8. BuildSnapshot 精确定义
现役字段只能按真实 schema 审计：
Locked
WeaponId
BodyId
HelmetId
BootsId
GlovesId
BeltId
Q0
Q1
W0
W1
E0
PassiveHash
Unspent
不得在本令 AC/Evidence 中臆造：
AllocatedNodeIds；
MasteryChoices；
Inventory；
SaveVersion；
SerializedBytes；
file path；
restore token。
现有六个 
*Id 在 
Capture() 中实际直接取自 
Equipped[0..5]，因此 Evidence 必须按实际语义记录它们；不得因为字段名含 
Id 就擅自称为 
ItemInstance.Id。
Snapshot 的合同身份是：
MAP-ENTRY CAPTURE SUMMARY
不是：
SAVE STATE
RESTORE STATE
EXACT PASSIVE LIST
CLONE SOURCE
9. Map-entry capture contract
成功 
TryEnterMap 必须：
在 mutation 前 live build 已冻结；
Capture() 读取该 build；
Snapshot 字段与进入前 live 值一致；
Snapshot.Locked=true；
State=InMap；
BuildLocked=true；
InMap 内 snapshot 除现有状态生命周期允许项外不得漂移。
失败 
TryEnterMap：
Snapshot delta = 0
State delta = 0
当前 
OnMap 为 
InMap || Cleared，所以 Cleared 状态再次 
TryEnterMap 必须继续拒绝并要求先 Exit。
10. Clear / Exit lifecycle
当前清场行为：
State = Cleared
Snapshot.Locked = false
同时：
BuildLocked = (State == InMap)
所以 Cleared 不是 build-locked 状态。
本令不得改变这个产品语义。
必须证明：
State
BuildLocked
Snapshot.Locked
Town
false
false/default
InMap
true
true
Cleared
false
false
Dead
false
false
正常 
ExitMap 和 death 
ExitMap 均会令 
Snapshot.Locked=false；death path 随后走既有 
TryRespec。
11. Build-lock authority
唯一 permission authority：
SliceSession.BuildLocked
当前定义：
State == MapState.InMap
Snapshot.Locked 是 capture/lifecycle metadata。
禁止让 
Snapshot.Locked 成为第二套写权限 oracle。
必须 repo-wide 搜索所有 
Snapshot.Locked consumer，并证明：
production mutation permission readers = 0
除非发现现有代码违约，此时分类 
UNRESOLVED_WITH_SOURCE 并做最小修复。
12. InMap build-mutation matrix
本令必须扫描所有现役 build-mutating public/domain API；至少当前已知：
TryEquip
TrySetSupport
TryReassignLink
TryAllocate
TryAllocateMastery
TryRespec
TryRandomCraft
TryDirectedCraft
这些入口当前均存在 
BuildLocked 门，包括普通 passive、Mastery、respec、装备/support/link 与 craft。
如果机械扫描发现其它同类 build mutation，必须加入矩阵；不得只测上述固定清单后忽略新命中。
InMap 对每个 discovered mutator：
result = reject
live build delta = 0
Allocated delta = 0
MasteryChoice delta = 0
Unspent delta = 0
equipment delta = 0
support/link delta = 0
inventory craft delta = 0
Snapshot delta = 0
V3 identity delta = 0
只比较该 API 实际拥有的字段；禁止为不相关 API 制造无意义断言。
13. Clone audit
当前只读侦察结论：
runtime clone seam = NONE
本令必须用 repo scan 重新证明：
no passive/build 
Clone；
no relevant 
MemberwiseClone；
no build deep/shallow-copy API。
若仍不存在：
Classification = NOT_APPLICABLE_WITH_SOURCE
Implementation = NONE
AC = PASS
绝对禁止为了完成“clone 审计”新增 Clone API。
若 scan 意外发现既有 clone/copy seam，则必须改标：
UNRESOLVED_WITH_SOURCE
并回到该既有 surface 做 parity audit，不得自行扩大成通用 clone 系统。
14. Save / Persistence audit
当前只读侦察结论：
runtime serialize/deserialize/File.Write persistence seam = NONE
必须机械扫描并给 source evidence。
若仍不存在：
Save/Persistence = NOT_APPLICABLE_WITH_SOURCE
这就是 PASS。
不得新增：
SaveGame；
save slot；
load；
JSON persistence；
PlayerPrefs build state；
File.Write；
FileStream save；
migration schema；
checkpoint；
restore-from-BuildSnapshot。
15. Serializer contract
本令中的 “serializer” 仅指已有 ProdSim V3 canonical evidence serializer，不是 runtime persistence。
V3 当前 passive identity：
ps| = sorted allocated NodeIds
px| = allocated Mastery explicit choice identity
并继续包含：
pm|
pe|
pk|
StatePayload 明确按稳定顺序输出 
ps| 和 
px|。
必须证明：
Ordinary / route-only
allocated route-only：
ps| changes
px| unchanged
route-only own effect does not leak to pm/pe/pk
Explicit Mastery
node 10 selected：
ps| contains 10
px| contains 10:<sourceOrdinal>
Life gameplay reflects +30 exactly once
Snapshot isolation
ProdSim V3：
MUST NOT READ BuildSnapshot
MUST NOT READ PassiveHash
MUST NOT READ Snapshot.Locked
本令不允许通过把 snapshot 数据塞入 V3 来“实现 parity”。
16. Reset contract
TryRespec
现有行为必须保持：
InMap：reject；
非锁定状态：清除 StartNode 之外所有 allocated；
对被清节点清 
MasteryChoice=-1；
StartNode 保持 allocated；
refund 等于实际清除的 allocated non-start count；
route-only 71 清除；
Mastery 10 allocation + explicit choice 清除；
+30 Life 消失；
derived player state 重算。
当前实现确实对每个被清 allocated node 同步清 
MasteryChoice。
ResetTown
必须证明：
allocation 回到既有 fresh/reset 规则；
MasteryChoice 全部 
-1；
Unspent / TotalPoints 回既有 initial rules；
Snapshot = default；
State = Town；
build-relevant state 与同 seed fresh session 一致。
ResetTown 当前明确清 
Allocated[]、
MasteryChoice[] 并将 
Snapshot=default。
Death reset
现有：
ExitMap(death=true)
→ State=Dead
→ Snapshot.Locked=false
→ TryRespec
必须证明 Mastery choice 无残留。
17. Acceptance Criteria
AC
可观测验收判据
AC-01
Entry fingerprint 完整；Compile 0；全部 discovered EditMode/PlayMode PASS；Content Audit fresh；entry V3 hash=
99f1bfd3f81c4fe6
AC-02
WO_04B_BUILD_IDENTITY_SCAN 覆盖合同 §4 全部关键词/表面
AC-03
所有 scan row 只使用四类枚举；出口 
UNRESOLVED_WITH_SOURCE=0
AC-04
当前 production 
PassiveMask 命中=0；历史 
PassiveMask 正确分类 
MIGRATED
AC-05
没有现役 32-bit passive allocation mask / ≤32 节点 authoritative identity
AC-06
PassiveHash 正确分类 
SAFE_CURRENT / DERIVED_FINGERPRINT，不作为 exact authority
AC-07
exact live passive identity owner 保持 
Allocated[] + MasteryChoice[]
AC-08
route-only node 71 合法进入 exact allocation identity
AC-09
F1 分配 71 后 
PassiveHash 改变，但 71 自身 gameplay effect 仍为 0
AC-10
WO-03 fixture node 10 / group 741 / 
+30 maximum Life 按真实 source ordinal 冻结
AC-11
selected node 10 的 exact identity 同时包含 NodeId 10 与 authoritative source ordinal
AC-12
selected node 10 后 
PassiveHash 对该 identity mutation 敏感
AC-13
BuildSnapshot schema 与合同列出的 13 个现役字段完全一致；Evidence 不臆造字段
AC-14
六个装备 
*Id 字段按真实 
Equipped[] capture 语义记录，不误称其它 ID 类型
AC-15
Snapshot 明确是 map-entry capture summary；不是 Save/Restore/exact allocation list
AC-16
Snapshot 没有 allocated NodeId list 被记录为已知设计事实，而不是要求新增字段的 defect
AC-17
成功 
TryEnterMap 的 Snapshot 所有现役字段与进入前 live state 一致
AC-18
成功入图后 
Snapshot.Locked=true、
State=InMap、
BuildLocked=true
AC-19
failed 
TryEnterMap 对 Snapshot/State/build identity 零变化
AC-20
Cleared 时保持 
State=Cleared、
BuildLocked=false、
Snapshot.Locked=false
AC-21
Cleared 未 Exit 时重新 
TryEnterMap reject，且不重抓 Snapshot
AC-22
Exit 后 snapshot lock metadata 正确解锁，不改变既有 state-machine 产品语义
AC-23
BuildLocked 是唯一 build mutation lock authority
AC-24
production mutation path 读取 
Snapshot.Locked 作为 permission oracle 的数量=0
AC-25
所有 discovered build mutators 均进入 InMap lock matrix，无遗漏
AC-26
TryAllocate 在 InMap 原子 reject，allocation/points/snapshot 零变化
AC-27
TryAllocateMastery 在 InMap 原子 reject，MasteryChoice/Life/points/snapshot 零变化
AC-28
TryRespec 在 InMap 原子 reject
AC-29
equip / support / link-reassign 在 InMap 全部原子 reject
AC-30
random / directed craft 在 InMap 全部原子 reject，resources/item contents 零变化
AC-31
任何扫描中新发现的 build mutator 同样满足 InMap reject + zero-delta
AC-32
runtime clone/copy seam 若仍不存在，则 
NOT_APPLICABLE_WITH_SOURCE + no implementation
AC-33
runtime Save/Load/Persistence/File.Write seam 若仍不存在，则 
NOT_APPLICABLE_WITH_SOURCE + no implementation
AC-34
ProdSim V3 不读取 
BuildSnapshot / 
PassiveHash / 
Snapshot.Locked
AC-35
V3 `ps
` 与 exact allocated NodeId set 一致，稳定数值排序
AC-36
V3 `px
` 与 allocated Mastery explicit choices 一一对应
AC-37
route-only fixture：`ps
sensitive；其自身 effect 不进入pm/pe/pk`
AC-38
Mastery fixture：`ps
/px
` identity 与 +30 Life gameplay 均正确
AC-39
TryRespec 清普通、route-only、Mastery allocation/selection，无 Mastery residue
AC-40
ResetTown 清 MasteryChoice、恢复既有 start/reset state、Snapshot=default
AC-41
death-triggered respec 后 Mastery allocation/choice/effect 无残留
AC-42
same-seed 
fresh session 与 
dirty → ResetTown 的本令 build-relevant state EXACT MATCH
AC-43
passive topology invariants 不变：reachable=1985、`
D
=325`、start-disconnected supported=42
AC-44
passive truth census 未未经授权移动：367 / 315 保持
AC-45
Canonical Data / Content / Parser / StatId / ModOp delta = NONE
AC-46
predecessor tests 无静默删除/禁用/弱化
AC-47
Final Compile 0；全部 discovered EditMode/PlayMode PASS；fail=0；skip=0；Content Audit PASS/fresh
AC-48
final cold ProdSim ×3 全部 
FNV1A64:99f1bfd3f81c4fe6，contract=V3，invalid=0
AC-49
Forbidden Expansion PASS，尤其 Save/Persistence/Clone invention = NONE
AC-50
Evidence Pack 全字段完整；04B 只标 
READY_FOR_GATE_REVIEW，不得自行启动 successor
AC-01..AC-50 全部 PASS 才可请求 ACCEPT。
18. 必做测试
至少提供以下等价机械测试；测试名可按项目命名规范调整：
PassiveMask_CurrentProductionSymbol_IsAbsent
PassiveHash_RouteOnly71_IsIdentitySensitive
PassiveHash_Mastery10Choice_IsIdentitySensitive

BuildSnapshot_SchemaMatchesCurrentContract
BuildSnapshot_CapturesCurrentLiveBuild
BuildSnapshot_EquipmentIdsMatchEquippedIndices
FailedMapEntry_DoesNotReplaceSnapshot
Cleared_UnlocksSnapshotMetadataAndBuildLock
Cleared_CannotReenterUntilExit

BuildLock_AllDiscoveredMutatorsRejectInMapAtomically
BuildLock_MasteryCommitRejectsWithoutChoiceOrLifeDelta
BuildLock_RespecRejectsWithoutPassiveDelta

ProdSim_DoesNotConsumeBuildSnapshot
Serializer_RouteOnlyIdentityWithoutEffect
Serializer_MasteryNodeAndChoiceParity

Respec_ClearsRouteOnlyAndMasteryChoice
ResetTown_ClearsSnapshotAndMasteryChoice
ResetTown_SameSeed_EqualsFreshBuildRelevantState
DeathRespec_ClearsMasteryIdentity
Clone / Save/Persistence 不存在时不要造 runtime test seam；以 scan artifact + source evidence 为验收证据。
19. ProdSim hash policy
本令不允许 canonical hash 变化。
入口/出口正式 baseline：
V3
FNV1A64:99f1bfd3f81c4fe6
最终必须三个独立冷进程：
H1 == H2 == H3 == FNV1A64:99f1bfd3f81c4fe6
原因：
ProdSim 当前不读 BuildSnapshot；
04B 不获准修改 V3 semantic payload；
snapshot/fingerprint/lock audit 不应改变 canonical gameplay truth；
本令不是 V4 / serializer expansion / rebaseline work order。
若 hash 发生任何变化
立即：
STOP
NO REBASELINE
NO VERSION BUMP
NO SUCCESSOR
Evidence 必须给：
expected hash；
actual hash；
first changed section；
pv/ps/px/pm/pe/pk exact diff；
triggering code diff；
minimal reproduction。
不得用“snapshot 修复导致的合理变化”自行放行。
20. Evidence Pack 必填字段
EP-A — Identity / Entry
Work Order
Base commit
Implementation commit
Entry/final git status
Entry/final working-tree fingerprint
Unity version
Changed files
Entry test manifest
EP-B — Scan classification
完整 
WO_04B_BUILD_IDENTITY_SCAN：
symbol/path
classification
source evidence
authority type
readers
writers
disposition
汇总：
SAFE_CURRENT = N
MIGRATED = N
NOT_APPLICABLE_WITH_SOURCE = N
UNRESOLVED_WITH_SOURCE = 0
EP-C — PassiveMask / fixed-width audit
必须明确：
Current PassiveMask = ABSENT
Historical PassiveMask = MIGRATED
Current PassiveHash = long / FNV1A64 / DERIVED_FINGERPRINT
Exact allocation authority = Allocated[] + MasteryChoice[]
并给扫描证据。
EP-D — Frozen fixture table
对 F0/F1/F2 至少记录：
sorted allocated NodeIds
Mastery NodeId
Mastery source ordinal
ChoiceKey
Unspent
PassiveHash
ps|
px|
Life / relevant gameplay result
F2 必须写出实际冻结的：
PrerequisiteNotableNodeId
SupportedChoiceOrdinal
EP-E — Snapshot schema
逐字段：
field
actual source expression
capture-time expected
actual
match?
semantic note
只能包含当前真实字段。
特别标明：
Weapon/Body/Helmet/Boots/Gloves/Belt Id
= Equipped[] capture value
EP-F — Map lifecycle
提供至少：
Town → EnterMap → InMap
InMap → Cleared
Cleared → failed re-enter
Cleared → ExitMap → Town
InMap → death Exit → Dead
逐步记录：
State
OnMap
BuildLocked
Snapshot.Locked
PassiveHash
Unspent
EP-G — Build-lock mutation matrix
每个 discovered mutator：
API
precondition
Town result
InMap result
return/error
live-state delta
Snapshot delta
resource delta
InMap 必须全为：
REJECT + ZERO DELTA
EP-H — Clone / Persistence absence
分别提供 scan：
Clone/Copy/MemberwiseClone
Serialize/Deserialize
File.Write/FileStream
PlayerPrefs build persistence
若无：
Classification = NOT_APPLICABLE_WITH_SOURCE
Product implementation added = NO
EP-I — Serializer parity
至少提供 F0/F1/F2：
ps|
px|
pm|
pe| digest/full
pk| digest/full
Snapshot.PassiveHash
并明确写：
ProdSim reads BuildSnapshot = NO
Snapshot PassiveHash == ProdSim canonical hash = NOT A REQUIREMENT
EP-J — Reset
TryRespec：
before allocated IDs
before MasteryChoice
before Unspent
after allocated IDs
after MasteryChoice
after Unspent
refund expected/actual
ResetTown：
fresh vs reset exact build-relevant comparison
Snapshot default = YES
death reset：
Mastery residue = 0
EP-K — Regression / scope
Compile
EditMode pass/fail/skip
PlayMode pass/fail/skip
Content Audit
predecessor missing tests
predecessor weakened tests
Canonical Data Delta
Content Delta
Parser Delta
StatId Delta
ModOp Delta
Topology 1985/325/42
Census 367/315
EP-L — Final ProdSim
三个真实冷进程：
Run1 timestamp / hash / contract / invalid
Run2 timestamp / hash / contract / invalid
Run3 timestamp / hash / contract / invalid
必须：
Distinct hashes = 1
Exact expected match = YES
Hash = FNV1A64:99f1bfd3f81c4fe6
Contract = V3
invalid = 0
EP-M — Forbidden expansion audit
逐项：
SaveGame NONE
Load NONE
Persistence NONE
File serialization NONE
PlayerPrefs build save NONE
New Clone API NONE
V3 schema bump NONE
Rebaseline NONE
New StatId NONE
New ModOp NONE
Parser expansion NONE
Jewel/Timeless NONE
Ascendancy NONE
Flask NONE
New skill axis NONE
Third connection group NONE
LOD NONE
EP-N — Governance handoff
最终只允许：
S6P-WO-04B = READY_FOR_GATE_REVIEW
successor = BLOCKED_PENDING_CHANNEL_A_GATE_REVIEW
不得自行判断/启动 04C 或 WO-05。
21. 禁止项
本令明确禁止：
恢复或新增 
PassiveMask；
新 bitmask allocation authority；
把 
PassiveHash 提升为 exact identity authority；
用 hash reconstruct passive allocation；
声称 FNV1A64 collision-free；
给 BuildSnapshot 增加 NodeId list 仅为了把它变成 save object；
SaveGame；
LoadGame；
runtime serialize/deserialize persistence；
File.Write / FileStream save；
PlayerPrefs build state；
save slot；
restore system；
migration schema；
cloud persistence；
为完成 checklist 新增 clone/copy subsystem；
用 
Snapshot.Locked 替代 
BuildLocked；
改变 Town/InMap/Cleared/Dead 的既有 lock 产品语义；
改 WO-03 Mastery prerequisite / choice identity / effect semantics；
FirstChoice / 
choices[0]；
Jewel；
Timeless；
Ascendancy；
Flask；
新技能；
新 StatId；
新 ModOp；
parser expansion；
第三连接组；
LOD；
Canonical Data/content mutation；
V3 contract/schema bump；
把 Snapshot/PassiveHash 加进 V3 payload；
ProdSim rebaseline。
允许的 runtime 修改仅限：
对扫描证明确实为 
UNRESOLVED_WITH_SOURCE 的现役表面做最小 parity repair。
22. Automatic STOP
任一成立立即停止：
entry ProdSim ≠ 
99f1bfd3f81c4fe6；
production 中重新发现现役 
PassiveMask 或等价 ≤32 authoritative passive mask；
有 fixed-width hash/mask 被用作 exact allocation authority；
需要新增 Save/Persistence 才能完成；
需要新增 Clone subsystem 才能完成；
Snapshot 被现有 production 当 restore/save source，而修复会扩张产品域；
Snapshot.Locked 与 
BuildLocked 形成竞争权限 owner；
InMap 任一 build mutation 可以成功；
InMap reject 后发生任何 half-mutation；
route-only 71 从 allocation identity 消失；
WO-03 Mastery choice identity 在 fingerprint/serializer parity 中丢失；
reset 后残留 Mastery choice；
ProdSim 被改成读取 BuildSnapshot/PassiveHash；
topology 
1985 / 325 / 42 漂移；
census 
367 / 315 漂移；
Canonical Data / Content / Parser / StatId / ModOp 发生非预期变化；
final ProdSim 任一 cold run hash 改变；
需要 V3 bump；
需要 canonical rebaseline；
predecessor test 被静默删除/禁用/弱化；
出口仍存在 
UNRESOLVED_WITH_SOURCE；
任何禁止域被触及。
STOP 后：
S6P-WO-04B = BLOCKED / NEEDS CHANNEL A ARBITRATION
提交：
expected；
actual；
source；
minimal reproduction；
first divergence；
affected tests/files。
不得继续 successor。
23. 完成定义
本令只有在以下同时成立时完成：
现役 PassiveMask = NONE
历史 32-bit risk = MIGRATED

exact passive identity
 = Allocated[] + explicit MasteryChoice[]

PassiveHash
 = deterministic derived fingerprint
 != exact/reconstructable identity

BuildSnapshot
 = honest map-entry summary
 != Save/Persistence

Clone
 = 当前不存在则机械 N/A，不发明

BuildLocked
 = 唯一写权限 owner
 = State == InMap

V3 serializer
 = live truth 的独立 deterministic projection
 不读取 Snapshot

Reset
 = ordinary / route-only / Mastery choice 无残留

ProdSim
 = FNV1A64:99f1bfd3f81c4fe6 ×3 EXACT
完成后只提交 S6P-WO-04B Evidence Pack 做 Channel A Gate Review。
来源
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：S6P-WO-04B CONTRACT

:::writing{variant="document" id="80421" title="S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity"}
# S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity

**Authority:** Channel A / `game-zzz-planning-2` 
**Status:** `RELEASED / EXECUTE NOW` 
**Base:** `main@88e8202` 
**Canonical ProdSim:** `V3 / FNV1A64:99f1bfd3f81c4fe6` 
**Scope:** 仅审计/最小修复仓库已经存在的 `mask / fixed-width identity / allocation identity / snapshot / clone / map-entry capture / build lock / serializer / reset`。 
**明确禁止:** 发明 Save / Persistence。

当前 `BuildSnapshot` 只包含 `Locked`、六个装备槽字段、`Q0/Q1/W0/W1/E0`、`PassiveHash`、`Unspent`；当前生产 passive state 为 `Allocated[] + MasteryChoice[]`。`PassiveHash` 由 `Capture()` 对升序 allocated NodeId 做 FNV1A64，并在该节点存在显式 Mastery choice 时继续混入 `choiceOrdinal + 1`。citeturn634960view0turn501661view0

---

## 1. Mission

本令只回答以下问题：

> 已有 passive build 在 live allocation identity、定宽 fingerprint、map-entry snapshot、build lock、V3 serializer 与 reset 各表面之间，是否保持一致、确定、无截断、无遗失、无锁绕过？

不是本令目标：

- 新 SaveGame；
- session persistence；
- runtime serialize/deserialize；
- restore from snapshot；
- 新 clone subsystem；
- 新 passive gameplay；
- 新 serializer version。

`BuildSnapshot.PassiveHash` 是**摘要/fingerprint**，不是能够无损重建 2429 节点 allocation 的容器。不得因为 snapshot 没有 NodeId 列表，就把本令扩成存档系统。

---

# 2. 强制入口门

任何产品 mutation 前记录：

- HEAD / branch；
- `git status --porcelain`；
- working-tree fingerprint；
- Unity version；
- discovered EditMode / PlayMode manifest；
- Content Audit；
- 当前 V3 canonical hash。

入口必须：

```text
Compile = 0 error
EditMode = ALL PASS / fail=0 / skip=0
PlayMode = ALL PASS / fail=0 / skip=0
Content Audit = PASS / fresh
ProdSim = V3
Hash = FNV1A64:99f1bfd3f81c4fe6
```

入口 canonical 至少实跑 1 次。

若入口 hash 不是 `99f1bfd3f81c4fe6`：

**STOP。不得实施，不得自行 rebaseline。**

---

# 3. 扫描分类枚举

本令所有命中必须使用且只能使用下列四类：

| Classification | 精确定义 |
|---|---|
| `SAFE_CURRENT` | 当前 production surface 存在，并满足本合同；只需机械证明，不需为它发明新机制 |
| `MIGRATED` | 历史风险真实存在过，但当前 production 已被既有工作令迁移；必须给旧→新 source 证据，不能当现役 defect 修第二遍 |
| `NOT_APPLICABLE_WITH_SOURCE` | 本合同要求审计的 surface 在当前 runtime 中不存在；必须给机械搜索/source evidence，禁止为了 checklist 新建 |
| `UNRESOLVED_WITH_SOURCE` | 当前 production surface 确实存在且与本合同不一致；必须给 source + 最小修复；出口不得残留此分类 |

**出口要求：`UNRESOLVED_WITH_SOURCE = 0`。**

不得使用：

- UNKNOWN；
- ASSUMED；
- NOT CHECKED；
- “看起来没问题”。

---

# 4. 强制扫描面

开工时生成 `WO_04B_BUILD_IDENTITY_SCAN`，至少扫描：

```text
PassiveMask
PassiveHash
BuildSnapshot
Allocated
MasteryChoice
Capture
Clone
Copy
MemberwiseClone
Serialize
Deserialize
File.Write
FileStream
PlayerPrefs
TryEnterMap
ExitMap
BuildLocked
Snapshot.Locked
TryAllocate
TryAllocateMastery
TryRespec
TryEquip
TrySetSupport
TryReassignLink
TryRandomCraft
TryDirectedCraft
ResetTown
StatePayload
ps|
px|
```

每条结果记录：

```text
symbol/path
classification
authority / derived / metadata / evidence-only
reader(s)
writer(s)
disposition
```

---

# 5. 已冻结的预期分类

## 5.1 `PassiveMask`

当前 production **不存在 `PassiveMask`**。

历史 `BuildSnapshot.PassiveMask : int` 的 ≤32 节点风险已由 S5U-WO-04 迁移为 `PassiveHash : long`。

因此本令预期：

```text
PassiveMask = MIGRATED
```

验收不是“重新实现 PassiveMask”，而是证明：

- production symbol = 0；
- 没有任何 32-bit passive allocation mask 回流；
- 历史证据能定位迁移关系。

发现现役 `PassiveMask` / `1 << nodeId` / 等价 32-bit allocation identity：

**STOP / UNRESOLVED_WITH_SOURCE。**

---

## 5.2 `PassiveHash`

现役：

```text
BuildSnapshot.PassiveHash : long
```

分类：

```text
SAFE_CURRENT
DERIVED_FINGERPRINT
NOT EXACT IDENTITY AUTHORITY
```

`Capture()` 当前对 `Allocated[]` 按 NodeId 升序迭代；每个 allocated node 混入 NodeId；若该节点 `MasteryChoice >= 0`，继续混入 `choiceOrdinal + 1`。citeturn501661view0

不得声称：

```text
PassiveHash equality => mathematically collision-free exact equality
```

本令只要求：

- 同一合法 build → 同一 fingerprint；
- 冻结的 identity mutation → fingerprint 有敏感性；
- 不使用 fingerprint 反向恢复 allocation。

---

# 6. Canonical live allocation identity

唯一 live authority 保持：

```text
Allocated[nodeId]
+
MasteryChoice[nodeId]
```

其中：

- `Allocated=true` 决定 NodeId 是否属于 passive allocation identity；
- 普通节点 `MasteryChoice=-1`；
- 已显式选择的 Mastery 使用 WO-03 冻结的 authoritative source ordinal；
- WO-03 当前数据无 per-choice stable ID，因此 identity 为：
 `MasteryNodeId + sourceOrdinal`；
- StartNode 保持既有 runtime 语义。

V3 serializer 已分别输出排序后的 `ps|` allocated NodeIds 与 `px|` explicit Mastery choices。citeturn501661view2turn501661view3

`PassiveHash`、`BuildSnapshot`、ProdSim payload 都只能是这个 live truth 的**派生消费者**，不得反向拥有 allocation。

---

# 7. 冻结 fixtures

所有 fixture 必须通过真实 domain/public API 构造；禁止直接写数组来制造正向证据。

## F0 — Fresh baseline

同 seed 新建 session。

冻结并记录：

- StartNode；
- sorted allocated IDs；
- all MasteryChoice values；
- Unspent；
- PassiveHash（通过真实 map-entry capture 获得）；
- V3 `ps| / px|`。

---

## F1 — Route-only identity

固定：

```text
NodeId = 71
```

通过合法路径分配。

必须证明：

- `Allocated[71]=true`；
- Node 71 出现在 allocation identity；
- snapshot `PassiveHash` 相对 F0 改变；
- V3 `ps|` 改变；
- route-only gameplay effect 仍为 0。

不得因为它“0 效果”而从 identity/snapshot fingerprint 消失。

---

## F2 — WO-03 Mastery identity

固定：

```text
Mastery NodeId = 10
Group = 741
Choice text = "+30 to maximum Life"
Effect = Life:Flat=30
```

prerequisite 使用既有 WO-03 规则：

> group 741 中按 authoritative source 得到的最小 non-Mastery Notable，经真实 allocation path 分配。

在本令 Preflight 中机械求出并冻结：

```text
PrerequisiteNotableNodeId = <exact>
SupportedChoiceOrdinal = <exact source ordinal>
ChoiceKey = 10:<ordinal>
```

不得猜 ordinal，不得用 GUI index。

WO-03 已冻结 node 10 / group 741 / `+30 to maximum Life` fixture。citeturn425612view0

成功 explicit selection 后必须证明：

- `Allocated[10]=true`；
- `MasteryChoice[10]=SupportedChoiceOrdinal`；
- exact identity 包含 `10:<ordinal>`；
- `PassiveHash` 相对未选择状态变化；
- V3 `ps|` 包含 10；
- V3 `px|` 包含 `10:<ordinal>`；
- Life +30 恰好一次。

---

## F3 — Map-entry capture

以包含 F1 + F2 的合法 build 进入地图。

在 `TryEnterMap` 前冻结 live state；成功调用后冻结 snapshot。

当前代码顺序为：

```text
Snapshot = Capture()
Snapshot.Locked = true
State = InMap
```

citeturn451353view1

必须逐字段比对 snapshot 与进入前 live build。

---

## F4 — Reset

从含：

- ordinary allocation；
- route-only 71；
- explicit Mastery 10；

的合法状态执行现有 reset surface。

分别审计：

- `TryRespec`
- `ResetTown`
- death-triggered respec

不得新建第四种 reset。

---

# 8. BuildSnapshot 精确定义

现役字段只能按真实 schema 审计：

```text
Locked
WeaponId
BodyId
HelmetId
BootsId
GlovesId
BeltId
Q0
Q1
W0
W1
E0
PassiveHash
Unspent
```

不得在本令 AC/Evidence 中臆造：

- AllocatedNodeIds；
- MasteryChoices；
- Inventory；
- SaveVersion；
- SerializedBytes；
- file path；
- restore token。

现有六个 `*Id` 在 `Capture()` 中实际直接取自 `Equipped[0..5]`，因此 Evidence 必须按**实际语义**记录它们；不得因为字段名含 `Id` 就擅自称为 `ItemInstance.Id`。citeturn501661view0

Snapshot 的合同身份是：

```text
MAP-ENTRY CAPTURE SUMMARY
```

不是：

```text
SAVE STATE
RESTORE STATE
EXACT PASSIVE LIST
CLONE SOURCE
```

---

# 9. Map-entry capture contract

成功 `TryEnterMap` 必须：

1. 在 mutation 前 live build 已冻结；
2. `Capture()` 读取该 build；
3. Snapshot 字段与进入前 live 值一致；
4. `Snapshot.Locked=true`；
5. `State=InMap`；
6. `BuildLocked=true`；
7. InMap 内 snapshot 除现有状态生命周期允许项外不得漂移。

失败 `TryEnterMap`：

```text
Snapshot delta = 0
State delta = 0
```

当前 `OnMap` 为 `InMap || Cleared`，所以 Cleared 状态再次 `TryEnterMap` 必须继续拒绝并要求先 Exit。citeturn501661view1

---

# 10. Clear / Exit lifecycle

当前清场行为：

```text
State = Cleared
Snapshot.Locked = false
```

同时：

```text
BuildLocked = (State == InMap)
```

所以 Cleared 不是 build-locked 状态。citeturn501661view1

本令**不得改变这个产品语义**。

必须证明：

| State | BuildLocked | Snapshot.Locked |
|---|---:|---:|
| Town | false | false/default |
| InMap | true | true |
| Cleared | false | false |
| Dead | false | false |

正常 `ExitMap` 和 death `ExitMap` 均会令 `Snapshot.Locked=false`；death path 随后走既有 `TryRespec`。citeturn451353view2turn451353view3

---

# 11. Build-lock authority

唯一 permission authority：

```text
SliceSession.BuildLocked
```

当前定义：

```text
State == MapState.InMap
```

citeturn634960view0

`Snapshot.Locked` 是 capture/lifecycle metadata。

**禁止让 `Snapshot.Locked` 成为第二套写权限 oracle。**

必须 repo-wide 搜索所有 `Snapshot.Locked` consumer，并证明：

```text
production mutation permission readers = 0
```

除非发现现有代码违约，此时分类 `UNRESOLVED_WITH_SOURCE` 并做最小修复。

---

# 12. InMap build-mutation matrix

本令必须扫描所有现役 build-mutating public/domain API；至少当前已知：

```text
TryEquip
TrySetSupport
TryReassignLink
TryAllocate
TryAllocateMastery
TryRespec
TryRandomCraft
TryDirectedCraft
```

这些入口当前均存在 `BuildLocked` 门，包括普通 passive、Mastery、respec、装备/support/link 与 craft。citeturn282609view1turn282609view0turn451353view4turn451353view5

如果机械扫描发现其它同类 build mutation，必须加入矩阵；不得只测上述固定清单后忽略新命中。

InMap 对每个 discovered mutator：

```text
result = reject
live build delta = 0
Allocated delta = 0
MasteryChoice delta = 0
Unspent delta = 0
equipment delta = 0
support/link delta = 0
inventory craft delta = 0
Snapshot delta = 0
V3 identity delta = 0
```

只比较该 API 实际拥有的字段；禁止为不相关 API 制造无意义断言。

---

# 13. Clone audit

当前只读侦察结论：

```text
runtime clone seam = NONE
```

本令必须用 repo scan 重新证明：

- no passive/build `Clone`；
- no relevant `MemberwiseClone`；
- no build deep/shallow-copy API。

若仍不存在：

```text
Classification = NOT_APPLICABLE_WITH_SOURCE
Implementation = NONE
AC = PASS
```

**绝对禁止为了完成“clone 审计”新增 Clone API。**

若 scan 意外发现既有 clone/copy seam，则必须改标：

```text
UNRESOLVED_WITH_SOURCE
```

并回到该既有 surface 做 parity audit，不得自行扩大成通用 clone 系统。

---

# 14. Save / Persistence audit

当前只读侦察结论：

```text
runtime serialize/deserialize/File.Write persistence seam = NONE
```

必须机械扫描并给 source evidence。

若仍不存在：

```text
Save/Persistence = NOT_APPLICABLE_WITH_SOURCE
```

这就是 PASS。

不得新增：

- SaveGame；
- save slot；
- load；
- JSON persistence；
- PlayerPrefs build state；
- File.Write；
- FileStream save；
- migration schema；
- checkpoint；
- restore-from-BuildSnapshot。

---

# 15. Serializer contract

本令中的 “serializer” **仅指已有 ProdSim V3 canonical evidence serializer**，不是 runtime persistence。

V3 当前 passive identity：

```text
ps| = sorted allocated NodeIds
px| = allocated Mastery explicit choice identity
```

并继续包含：

```text
pm|
pe|
pk|
```

`StatePayload` 明确按稳定顺序输出 `ps|` 和 `px|`。citeturn501661view2turn501661view3

必须证明：

### Ordinary / route-only

allocated route-only：

```text
ps| changes
px| unchanged
route-only own effect does not leak to pm/pe/pk
```

### Explicit Mastery

node 10 selected：

```text
ps| contains 10
px| contains 10:<sourceOrdinal>
Life gameplay reflects +30 exactly once
```

### Snapshot isolation

ProdSim V3：

```text
MUST NOT READ BuildSnapshot
MUST NOT READ PassiveHash
MUST NOT READ Snapshot.Locked
```

本令不允许通过把 snapshot 数据塞入 V3 来“实现 parity”。

---

# 16. Reset contract

## `TryRespec`

现有行为必须保持：

- InMap：reject；
- 非锁定状态：清除 StartNode 之外所有 allocated；
- 对被清节点清 `MasteryChoice=-1`；
- StartNode 保持 allocated；
- refund 等于实际清除的 allocated non-start count；
- route-only 71 清除；
- Mastery 10 allocation + explicit choice 清除；
- +30 Life 消失；
- derived player state 重算。

当前实现确实对每个被清 allocated node 同步清 `MasteryChoice`。citeturn451353view3

## `ResetTown`

必须证明：

- allocation 回到既有 fresh/reset 规则；
- MasteryChoice 全部 `-1`；
- Unspent / TotalPoints 回既有 initial rules；
- Snapshot = default；
- State = Town；
- build-relevant state 与同 seed fresh session 一致。

`ResetTown` 当前明确清 `Allocated[]`、`MasteryChoice[]` 并将 `Snapshot=default`。citeturn634960view0

## Death reset

现有：

```text
ExitMap(death=true)
→ State=Dead
→ Snapshot.Locked=false
→ TryRespec
```

必须证明 Mastery choice 无残留。citeturn451353view2

---

# 17. Acceptance Criteria

| AC | 可观测验收判据 |
|---|---|
| **AC-01** | Entry fingerprint 完整；Compile 0；全部 discovered EditMode/PlayMode PASS；Content Audit fresh；entry V3 hash=`99f1bfd3f81c4fe6` |
| **AC-02** | `WO_04B_BUILD_IDENTITY_SCAN` 覆盖合同 §4 全部关键词/表面 |
| **AC-03** | 所有 scan row 只使用四类枚举；出口 `UNRESOLVED_WITH_SOURCE=0` |
| **AC-04** | 当前 production `PassiveMask` 命中=0；历史 `PassiveMask` 正确分类 `MIGRATED` |
| **AC-05** | 没有现役 32-bit passive allocation mask / ≤32 节点 authoritative identity |
| **AC-06** | `PassiveHash` 正确分类 `SAFE_CURRENT / DERIVED_FINGERPRINT`，不作为 exact authority |
| **AC-07** | exact live passive identity owner 保持 `Allocated[] + MasteryChoice[]` |
| **AC-08** | route-only node 71 合法进入 exact allocation identity |
| **AC-09** | F1 分配 71 后 `PassiveHash` 改变，但 71 自身 gameplay effect 仍为 0 |
| **AC-10** | WO-03 fixture node 10 / group 741 / `+30 maximum Life` 按真实 source ordinal 冻结 |
| **AC-11** | selected node 10 的 exact identity 同时包含 NodeId 10 与 authoritative source ordinal |
| **AC-12** | selected node 10 后 `PassiveHash` 对该 identity mutation 敏感 |
| **AC-13** | `BuildSnapshot` schema 与合同列出的 13 个现役字段完全一致；Evidence 不臆造字段 |
| **AC-14** | 六个装备 `*Id` 字段按真实 `Equipped[]` capture 语义记录，不误称其它 ID 类型 |
| **AC-15** | Snapshot 明确是 map-entry capture summary；不是 Save/Restore/exact allocation list |
| **AC-16** | Snapshot 没有 allocated NodeId list 被记录为已知设计事实，而不是要求新增字段的 defect |
| **AC-17** | 成功 `TryEnterMap` 的 Snapshot 所有现役字段与进入前 live state 一致 |
| **AC-18** | 成功入图后 `Snapshot.Locked=true`、`State=InMap`、`BuildLocked=true` |
| **AC-19** | failed `TryEnterMap` 对 Snapshot/State/build identity 零变化 |
| **AC-20** | Cleared 时保持 `State=Cleared`、`BuildLocked=false`、`Snapshot.Locked=false` |
| **AC-21** | Cleared 未 Exit 时重新 `TryEnterMap` reject，且不重抓 Snapshot |
| **AC-22** | Exit 后 snapshot lock metadata 正确解锁，不改变既有 state-machine 产品语义 |
| **AC-23** | `BuildLocked` 是唯一 build mutation lock authority |
| **AC-24** | production mutation path 读取 `Snapshot.Locked` 作为 permission oracle 的数量=0 |
| **AC-25** | 所有 discovered build mutators 均进入 InMap lock matrix，无遗漏 |
| **AC-26** | `TryAllocate` 在 InMap 原子 reject，allocation/points/snapshot 零变化 |
| **AC-27** | `TryAllocateMastery` 在 InMap 原子 reject，MasteryChoice/Life/points/snapshot 零变化 |
| **AC-28** | `TryRespec` 在 InMap 原子 reject |
| **AC-29** | equip / support / link-reassign 在 InMap 全部原子 reject |
| **AC-30** | random / directed craft 在 InMap 全部原子 reject，resources/item contents 零变化 |
| **AC-31** | 任何扫描中新发现的 build mutator 同样满足 InMap reject + zero-delta |
| **AC-32** | runtime clone/copy seam 若仍不存在，则 `NOT_APPLICABLE_WITH_SOURCE` + no implementation |
| **AC-33** | runtime Save/Load/Persistence/File.Write seam 若仍不存在，则 `NOT_APPLICABLE_WITH_SOURCE` + no implementation |
| **AC-34** | ProdSim V3 不读取 `BuildSnapshot` / `PassiveHash` / `Snapshot.Locked` |
| **AC-35** | V3 `ps|` 与 exact allocated NodeId set 一致，稳定数值排序 |
| **AC-36** | V3 `px|` 与 allocated Mastery explicit choices 一一对应 |
| **AC-37** | route-only fixture：`ps|` sensitive；其自身 effect 不进入 `pm/pe/pk` |
| **AC-38** | Mastery fixture：`ps|/px|` identity 与 +30 Life gameplay 均正确 |
| **AC-39** | `TryRespec` 清普通、route-only、Mastery allocation/selection，无 Mastery residue |
| **AC-40** | `ResetTown` 清 MasteryChoice、恢复既有 start/reset state、Snapshot=default |
| **AC-41** | death-triggered respec 后 Mastery allocation/choice/effect 无残留 |
| **AC-42** | same-seed `fresh session` 与 `dirty → ResetTown` 的本令 build-relevant state EXACT MATCH |
| **AC-43** | passive topology invariants 不变：reachable=1985、`|D|=325`、start-disconnected supported=42 |
| **AC-44** | passive truth census 未未经授权移动：367 / 315 保持 |
| **AC-45** | Canonical Data / Content / Parser / StatId / ModOp delta = NONE |
| **AC-46** | predecessor tests 无静默删除/禁用/弱化 |
| **AC-47** | Final Compile 0；全部 discovered EditMode/PlayMode PASS；fail=0；skip=0；Content Audit PASS/fresh |
| **AC-48** | final cold ProdSim ×3 全部 `FNV1A64:99f1bfd3f81c4fe6`，contract=V3，invalid=0 |
| **AC-49** | Forbidden Expansion PASS，尤其 Save/Persistence/Clone invention = NONE |
| **AC-50** | Evidence Pack 全字段完整；04B 只标 `READY_FOR_GATE_REVIEW`，不得自行启动 successor |

**AC-01..AC-50 全部 PASS 才可请求 ACCEPT。**

---

# 18. 必做测试

至少提供以下等价机械测试；测试名可按项目命名规范调整：

```text
PassiveMask_CurrentProductionSymbol_IsAbsent
PassiveHash_RouteOnly71_IsIdentitySensitive
PassiveHash_Mastery10Choice_IsIdentitySensitive

BuildSnapshot_SchemaMatchesCurrentContract
BuildSnapshot_CapturesCurrentLiveBuild
BuildSnapshot_EquipmentIdsMatchEquippedIndices
FailedMapEntry_DoesNotReplaceSnapshot
Cleared_UnlocksSnapshotMetadataAndBuildLock
Cleared_CannotReenterUntilExit

BuildLock_AllDiscoveredMutatorsRejectInMapAtomically
BuildLock_MasteryCommitRejectsWithoutChoiceOrLifeDelta
BuildLock_RespecRejectsWithoutPassiveDelta

ProdSim_DoesNotConsumeBuildSnapshot
Serializer_RouteOnlyIdentityWithoutEffect
Serializer_MasteryNodeAndChoiceParity

Respec_ClearsRouteOnlyAndMasteryChoice
ResetTown_ClearsSnapshotAndMasteryChoice
ResetTown_SameSeed_EqualsFreshBuildRelevantState
DeathRespec_ClearsMasteryIdentity
```

Clone / Save/Persistence 不存在时**不要造 runtime test seam**；以 scan artifact + source evidence 为验收证据。

---

# 19. ProdSim hash policy

**本令不允许 canonical hash 变化。**

入口/出口正式 baseline：

```text
V3
FNV1A64:99f1bfd3f81c4fe6
```

最终必须三个独立冷进程：

```text
H1 == H2 == H3 == FNV1A64:99f1bfd3f81c4fe6
```

原因：

- ProdSim 当前不读 BuildSnapshot；
- 04B 不获准修改 V3 semantic payload；
- snapshot/fingerprint/lock audit 不应改变 canonical gameplay truth；
- 本令不是 V4 / serializer expansion / rebaseline work order。

### 若 hash 发生任何变化

立即：

```text
STOP
NO REBASELINE
NO VERSION BUMP
NO SUCCESSOR
```

Evidence 必须给：

- expected hash；
- actual hash；
- first changed section；
- `pv/ps/px/pm/pe/pk` exact diff；
- triggering code diff；
- minimal reproduction。

不得用“snapshot 修复导致的合理变化”自行放行。

---

# 20. Evidence Pack 必填字段

## EP-A — Identity / Entry

```text
Work Order
Base commit
Implementation commit
Entry/final git status
Entry/final working-tree fingerprint
Unity version
Changed files
Entry test manifest
```

---

## EP-B — Scan classification

完整 `WO_04B_BUILD_IDENTITY_SCAN`：

```text
symbol/path
classification
source evidence
authority type
readers
writers
disposition
```

汇总：

```text
SAFE_CURRENT = N
MIGRATED = N
NOT_APPLICABLE_WITH_SOURCE = N
UNRESOLVED_WITH_SOURCE = 0
```

---

## EP-C — PassiveMask / fixed-width audit

必须明确：

```text
Current PassiveMask = ABSENT
Historical PassiveMask = MIGRATED
Current PassiveHash = long / FNV1A64 / DERIVED_FINGERPRINT
Exact allocation authority = Allocated[] + MasteryChoice[]
```

并给扫描证据。

---

## EP-D — Frozen fixture table

对 F0/F1/F2 至少记录：

```text
sorted allocated NodeIds
Mastery NodeId
Mastery source ordinal
ChoiceKey
Unspent
PassiveHash
ps|
px|
Life / relevant gameplay result
```

F2 必须写出实际冻结的：

```text
PrerequisiteNotableNodeId
SupportedChoiceOrdinal
```

---

## EP-E — Snapshot schema

逐字段：

```text
field
actual source expression
capture-time expected
actual
match?
semantic note
```

只能包含当前真实字段。

特别标明：

```text
Weapon/Body/Helmet/Boots/Gloves/Belt Id
= Equipped[] capture value
```

---

## EP-F — Map lifecycle

提供至少：

```text
Town → EnterMap → InMap
InMap → Cleared
Cleared → failed re-enter
Cleared → ExitMap → Town
InMap → death Exit → Dead
```

逐步记录：

```text
State
OnMap
BuildLocked
Snapshot.Locked
PassiveHash
Unspent
```

---

## EP-G — Build-lock mutation matrix

每个 discovered mutator：

```text
API
precondition
Town result
InMap result
return/error
live-state delta
Snapshot delta
resource delta
```

InMap 必须全为：

```text
REJECT + ZERO DELTA
```

---

## EP-H — Clone / Persistence absence

分别提供 scan：

```text
Clone/Copy/MemberwiseClone
Serialize/Deserialize
File.Write/FileStream
PlayerPrefs build persistence
```

若无：

```text
Classification = NOT_APPLICABLE_WITH_SOURCE
Product implementation added = NO
```

---

## EP-I — Serializer parity

至少提供 F0/F1/F2：

```text
ps|
px|
pm|
pe| digest/full
pk| digest/full
Snapshot.PassiveHash
```

并明确写：

```text
ProdSim reads BuildSnapshot = NO
Snapshot PassiveHash == ProdSim canonical hash = NOT A REQUIREMENT
```

---

## EP-J — Reset

`TryRespec`：

```text
before allocated IDs
before MasteryChoice
before Unspent
after allocated IDs
after MasteryChoice
after Unspent
refund expected/actual
```

`ResetTown`：

```text
fresh vs reset exact build-relevant comparison
Snapshot default = YES
```

death reset：

```text
Mastery residue = 0
```

---

## EP-K — Regression / scope

```text
Compile
EditMode pass/fail/skip
PlayMode pass/fail/skip
Content Audit
predecessor missing tests
predecessor weakened tests
Canonical Data Delta
Content Delta
Parser Delta
StatId Delta
ModOp Delta
Topology 1985/325/42
Census 367/315
```

---

## EP-L — Final ProdSim

三个真实冷进程：

```text
Run1 timestamp / hash / contract / invalid
Run2 timestamp / hash / contract / invalid
Run3 timestamp / hash / contract / invalid
```

必须：

```text
Distinct hashes = 1
Exact expected match = YES
Hash = FNV1A64:99f1bfd3f81c4fe6
Contract = V3
invalid = 0
```

---

## EP-M — Forbidden expansion audit

逐项：

```text
SaveGame NONE
Load NONE
Persistence NONE
File serialization NONE
PlayerPrefs build save NONE
New Clone API NONE
V3 schema bump NONE
Rebaseline NONE
New StatId NONE
New ModOp NONE
Parser expansion NONE
Jewel/Timeless NONE
Ascendancy NONE
Flask NONE
New skill axis NONE
Third connection group NONE
LOD NONE
```

---

## EP-N — Governance handoff

最终只允许：

```text
S6P-WO-04B = READY_FOR_GATE_REVIEW
successor = BLOCKED_PENDING_CHANNEL_A_GATE_REVIEW
```

不得自行判断/启动 04C 或 WO-05。

---

# 21. 禁止项

本令明确禁止：

- 恢复或新增 `PassiveMask`；
- 新 bitmask allocation authority；
- 把 `PassiveHash` 提升为 exact identity authority；
- 用 hash reconstruct passive allocation；
- 声称 FNV1A64 collision-free；
- 给 BuildSnapshot 增加 NodeId list 仅为了把它变成 save object；
- SaveGame；
- LoadGame；
- runtime serialize/deserialize persistence；
- File.Write / FileStream save；
- PlayerPrefs build state；
- save slot；
- restore system；
- migration schema；
- cloud persistence；
- 为完成 checklist 新增 clone/copy subsystem；
- 用 `Snapshot.Locked` 替代 `BuildLocked`；
- 改变 Town/InMap/Cleared/Dead 的既有 lock 产品语义；
- 改 WO-03 Mastery prerequisite / choice identity / effect semantics；
- FirstChoice / `choices[0]`；
- Jewel；
- Timeless；
- Ascendancy；
- Flask；
- 新技能；
- 新 StatId；
- 新 ModOp；
- parser expansion；
- 第三连接组；
- LOD；
- Canonical Data/content mutation；
- V3 contract/schema bump；
- 把 Snapshot/PassiveHash 加进 V3 payload；
- ProdSim rebaseline。

允许的 runtime 修改仅限：

> 对扫描证明确实为 `UNRESOLVED_WITH_SOURCE` 的现役表面做**最小 parity repair**。

---

# 22. Automatic STOP

任一成立立即停止：

1. entry ProdSim ≠ `99f1bfd3f81c4fe6`；
2. production 中重新发现现役 `PassiveMask` 或等价 ≤32 authoritative passive mask；
3. 有 fixed-width hash/mask 被用作 exact allocation authority；
4. 需要新增 Save/Persistence 才能完成；
5. 需要新增 Clone subsystem 才能完成；
6. Snapshot 被现有 production 当 restore/save source，而修复会扩张产品域；
7. `Snapshot.Locked` 与 `BuildLocked` 形成竞争权限 owner；
8. InMap 任一 build mutation 可以成功；
9. InMap reject 后发生任何 half-mutation；
10. route-only 71 从 allocation identity 消失；
11. WO-03 Mastery choice identity 在 fingerprint/serializer parity 中丢失；
12. reset 后残留 Mastery choice；
13. ProdSim 被改成读取 BuildSnapshot/PassiveHash；
14. topology `1985 / 325 / 42` 漂移；
15. census `367 / 315` 漂移；
16. Canonical Data / Content / Parser / StatId / ModOp 发生非预期变化；
17. final ProdSim 任一 cold run hash 改变；
18. 需要 V3 bump；
19. 需要 canonical rebaseline；
20. predecessor test 被静默删除/禁用/弱化；
21. 出口仍存在 `UNRESOLVED_WITH_SOURCE`；
22. 任何禁止域被触及。

STOP 后：

```text
S6P-WO-04B = BLOCKED / NEEDS CHANNEL A ARBITRATION
```

提交：

- expected；
- actual；
- source；
- minimal reproduction；
- first divergence；
- affected tests/files。

不得继续 successor。

---

# 23. 完成定义

本令只有在以下同时成立时完成：

```text
现役 PassiveMask = NONE
历史 32-bit risk = MIGRATED

exact passive identity
 = Allocated[] + explicit MasteryChoice[]

PassiveHash
 = deterministic derived fingerprint
 != exact/reconstructable identity

BuildSnapshot
 = honest map-entry summary
 != Save/Persistence

Clone
 = 当前不存在则机械 N/A，不发明

BuildLocked
 = 唯一写权限 owner
 = State == InMap

V3 serializer
 = live truth 的独立 deterministic projection
 不读取 Snapshot

Reset
 = ordinary / route-only / Mastery choice 无残留

ProdSim
 = FNV1A64:99f1bfd3f81c4fe6 ×3 EXACT
```

完成后只提交 **S6P-WO-04B Evidence Pack** 做 Channel A Gate Review。
:::
 

 

 

 

 
 
 
 

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