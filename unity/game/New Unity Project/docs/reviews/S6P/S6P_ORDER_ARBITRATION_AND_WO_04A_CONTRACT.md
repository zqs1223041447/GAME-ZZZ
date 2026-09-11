裁决：改序。采用 
WO-04A → WO-03 → WO-04B → WO-05 为主序列。 原已释放的 WO-03 不作废、也不算失败，而是标记为 
QUEUE-DEFERRED / NOT STARTED；先建立唯一 Passive support truth，再让 Mastery 消费这份 truth。第二渠道这次提出的依赖关系判断是成立的。
同时我加两项修正：WO-02 保持 ACCEPT，不追溯降级；从现在起 canonical baseline 的最终确认升级为 3 个独立 Unity 冷启动进程。另外，原 WO-04 里“现有 consumer 已存在但 parser 仍漏映射”的工作不丢失，后续根据 04A census 决定是否插一个条件性的 
WO-04C。
S6P 执行顺序仲裁与 S6P-WO-04A
S6P Planning Arbitration
1. 执行顺序裁决
旧队列：
WO-02
→ WO-03 Mastery
→ WO-04 Semantic Closure
→ WO-05 LOD
改为：
WO-02 ACCEPTED
 ↓
WO-04A Passive Support Truth Gate & Silent-Zero Elimination
 ↓
WO-03 Mastery Explicit Selection & Allocation Correctness
 ↓
WO-04B Passive Build Identity / Snapshot / Lock Parity
 ↓
[WO-04C Existing-Consumer Semantic Closure — CONDITIONAL]
 ↓
WO-05 Passive Overview LOD & Texture Residency
原 WO-03 状态
S6P-WO-03:
RELEASED PREVIOUSLY
→ QUEUE-DEFERRED BEFORE EXECUTION
→ NOT FAILED
→ NOT CANCELLED
04A ACCEPT 后重新放行，合同主体保留，但必须使用 04A 建立的 support truth，不得自己建立第二套 Mastery-choice compatibility oracle。
2. 为什么改序
第二渠道最关键的一点是正确的：
“能不能完整兑现”是“能不能选择”的前置真相。
现在已经知道：
On-tree nodes = 2429
SUPPORTED = 424
UNSUPPORTED_CURRENTLY = 2005
Mastery = 315
如果直接做 WO-03，Mastery selector 必须自行回答：
这个 choice 是否完整支持？
随后原 WO-04 又必须回答：
这个 passive/node 是否完整支持？
这极易形成：
MasterySupportEvaluator
PassiveSupportEvaluator
两套近似但不完全相同的 oracle。
正确依赖应是：
source passive semantics
 ↓
ONE Passive Support Truth
 ↓
normal-node allocation gate
 ↓
Mastery choice gate
 ↓
UI
所以 support truth 必须先于 Mastery interaction。
3. Mastery 过渡态裁定
授权在 WO-04A 中临时把 Mastery 设为不可分配。
这是明确的：
Gameplay Delta = YES
但属于 bounded correctness/safety correction。
在 WO-03 完成前：
Mastery node
→ SPECIAL_PENDING_MASTERY
→ cannot allocate
→ point cost = 0
→ implicit choice[0] effect = 0
允许查看其官方信息，但不得获得 gameplay effect。
这比继续保持：
click
→ spend point
→ silently choice[0]
更符合当前已知真相。
对 ProdSim hash 的影响
预期不变。
WO-02 canonical fixture 明确：
Mastery nodes = 0
Unsupported nodes = 0
04A 只会改变这些 fixture 没有触及的非法/未支持 allocation 路径。
因此 04A Exit expected：
FNV1A64:ec1d3ed67d3035d0
如果改变：
STOP
→ investigate unexpected canonical truth drift
不得直接重新基线。
4. WO-02 三次独立运行裁定
对既有 WO-02
仍然 ACCEPT。无 retroactive follow-up。
原合同要求的是：
三次 fresh execution；
实际重新运行 simulator；
非缓存；
非复制报告；
不得只在同一次计算结果上重复读取。
它没有明确写：
Unity Editor process 必须退出重启三次。
因此当前三次独立 
run_tests invocation 在合同原文下有效。
WO-02 状态不变
S6P-WO-02 = ACCEPT
Canonical =
FNV1A64:ec1d3ed67d3035d0
从现在开始提高标准
第二渠道指出的 static/process contamination 风险成立。
以后凡是：
NEW CANONICAL HASH PROMOTION
最终 closure 必须使用：
Unity Process #1 → hash
Unity Process #2 → hash
Unity Process #3 → hash
三个独立 Unity 冷启动进程。
优先使用现有可自动化的：
Unity -batchmode -runTests ...
或项目等价 runner。
04A 的额外 preflight
在任何 gameplay mutation 前，先用新 runner 对当前 WO-02 baseline 做一次：
Cold Process 1 = ec1d...
Cold Process 2 = ec1d...
Cold Process 3 = ec1d...
如果不一致：
停止 04A 产品修改。
这样既验证旧 baseline，也无需单开 
WO-02-F1。
5. Canonical serializer 附加裁定
第二渠道提出的协议风险成立，但不追溯否决 WO-02。
从现在锁定：
Gameplay hash 的 authority 必须是 stable semantic identity
禁止未来依赖：
Enum.ToString()
localized display name
human-readable stat name
GUI name
dictionary ordering
culture-specific float ToString()
作为 canonical identity。
WO-02 当前冗余名称
如果 
pm/pe/pk 中同时有：
numeric StatId + readable enum/name
现状可以暂时保留，不要求 04A 为此改变 hash。
但是在 WO-03 本来就要升级 
passive-v2 / V3 并重新基线 时：
hash segment 改为 stable numeric ID / explicit canonical token；
readable names 移到 report diagnostics；
float encoding 固定 invariant/canonical representation；
duplicate modifiers 保留 multiplicity；
tags/condition identity 明确 canonical sort/encoding。
这样不会额外制造一次纯协议 rebaseline。
6. Test-count 规则修正
接受第二渠道意见。
以后 Gate 不再使用：
expected exactly 409 tests
作为产品门。
正确规则：
all discovered tests PASS
fail = 0
skip = 0 unless explicitly justified
all predecessor contract tests still present
no unexplained deletion/weakening
具体：
409/409
422/422
...
仍需报告，作为 evidence accounting，但不是固定分母 oracle。
7. PoE source provenance
04A 开始追加：
tree_raw.json source/version provenance
tree_raw.json checksum
推荐 SHA-256。
它属于：
evidence provenance
明确不进入 gameplay FNV hash。
S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination
Execution Order: immediately after accepted WO-02
Runtime Gameplay Authority: 
YES — passive allocatability/support gate only
Mastery Feature Authority: 
NO — temporary blocking only
Parser Expansion: 
NO
New StatId / New Gameplay Axis: 
NO
Canonical Data Mutation: 
NO
8. Objective
建立整个 Passive domain 唯一的：
Can this complete passive gameplay promise be fulfilled
by GAME-ZZZ's currently implemented runtime?
权威判定。
并消灭：
spend passive point
→ node lights up
→ only supported subset applies
→ unsupported lines silently do nothing
这种状态。
核心 invariant：
节点只要存在一个不能完整兑现的 gameplay effect，整个节点当前不可分配。
9. Support Truth Architecture
建立一个 runtime/domain-owned authority。
名称可按工程风格，例如：
PassiveSupportTruth
PassiveEligibility
PassiveSemanticSupport
但只能有一个规则 owner。
必须提供至少两类查询：
EvaluateNode(node)
EvaluateChoice(masteryChoice)
后者只是给未来 WO-03 消费 support truth。
04A 不实现 Mastery selection。
10. Line Truth 与 Node Truth 分离
沿用 WO-01 line classification：
CONSUMED
BLOCKED_BY_DOMAIN
SPECIAL_INTERACTION
STRUCTURAL
STRUCTURAL
不属于 gameplay promise。
因此：
STRUCTURAL alone
≠ unsupported
括号提示、解释行等不因存在而阻止分配。
但不能使用：
contains '('
作为判定规则。
必须根据 semantic classification。
11. CONSUMED 的严格定义
从 04A 起：
CONSUMED 不再仅表示：
PoeStatParser recognized it
而必须表示：
source line
→ authoritative parser
→ semantic tuple
→ existing runtime consumer
→ effective gameplay result path exists
例如应能证明不同消费链：
AttackSpeed / PhysicalDamage
→ actual Skill context consumer

Life / Strength / Dexterity
→ actual player/global result consumer
不得在 test layer 重建第二套 modifier evaluator。
正确链只能是：
source
→ production runtime truth
→ canonical/read-only projection
→ tests/FNV
12. SPECIAL_INTERACTION
若 effect/node 属于 special interaction：
只有存在：
explicit current runtime handler
+
behavior evidence
才可视为当前可兑现。
否则：
BLOCKED_SPECIAL_INTERACTION
当前至少：
Mastery
Jewel Socket
Timeless/special node
不能因为画出来了就算 supported。
13. Node Allocatability Rule
对于普通 player-allocatable node：
all gameplay effect lines = CONSUMED
AND
all required special interactions handled
→ ALLOCATABLE_SUPPORTED
只要存在：
>=1 BLOCKED_BY_DOMAIN
或：
unhandled SPECIAL_INTERACTION
则：
BLOCKED_CURRENTLY
14. Mixed Node — Mandatory Fail Closed
这是 04A 的核心。
例如：
Line 1 = CONSUMED
Line 2 = CONSUMED
Line 3 = BLOCKED_BY_DOMAIN
正确结果：
whole node = BLOCKED_CURRENTLY
不得：
apply line1 + line2
ignore line3
Evidence 必须找至少一个真实 mixed node 做冻结 regression。
若 authoritative dataset 中客观没有 mixed node，报告 count=0，再以 unit-level semantic fixture 补算法测试；不得伪造它存在。
15. TryAllocate Gate
SliceSession.TryAllocate 或当前唯一 allocation authority 必须调用相同 support truth。
Unsupported node：
TryAllocate
→ deterministic reject
→ point delta = 0
→ allocated-set delta = 0
→ modifier delta = 0
→ player effective-result delta = 0
→ skill effective-result delta = 0
必须提供稳定 rejection reason。
16. Defensive Consumption Gate
除了 allocation gate，还必须防止 corrupted/test-injected state 产生半效果。
如果 allocated collection 中出现一个当前：
BLOCKED_CURRENTLY
节点：
它必须：
contribute zero gameplay modifiers
并由适合现有架构的 invalid-state evidence 暴露。
不得继续部分消费其中 CONSUMED lines。
不要求新 Persistence/migration。
17. Mastery Transitional Rule
在 WO-03 前所有 Mastery：
SPECIAL_PENDING_MASTERY
点击/分配
reject
point delta = 0
Effect
implicit choice[0] = DISABLED
Mastery gameplay modifier = 0
UI
读取相同 domain truth，显示：
currently unavailable / pending explicit mastery selection
或等价稳定原因。
不得维护：
masteryBlockedNodeIds[]
这样的第二份 GUI truth。
18. 403 无坐标节点
继续：
SOURCE/CENSUS ACCOUNTING ONLY
不得进入：
player allocation eligibility denominator
不把它们视为 2429 上树支持率失败。
19. Missing 70 Domains
继续：
dependency report only
不得自动实现：
Cold；
Lightning；
ES；
Block；
Suppression；
Minion；
Totem；
Warcry；
Ailment；
Flask；
Charge；
Leech；
DoT；
etc.
这些节点只保持：
BLOCKED_CURRENTLY
这不阻塞 04A PASS。
20. UI Truth
Passive UI 中：
CanClick
Disabled
Reason
全部读取 domain authority。
禁止：
UI-specific unsupported list
UI re-runs parser itself
UI checks node text independently
视觉只展示真相，不定义真相。
21. Provenance
Evidence 必须记录：
Passive source file:
tree_raw.json

Source provenance/version:
...

SHA-256:
...
Checksum 不进 FNV gameplay hash。
22. Cold-Process Entry Verification
必须发生在第一行 production gameplay mutation 之前。
运行：
Cold Unity #1
Cold Unity #2
Cold Unity #3
全部：
FNV1A64:ec1d3ed67d3035d0
才能继续。
否则：
STOP
Defect = baseline process-repeatability failure
23. Expected ProdSim Exit
由于 WO-02 fixture 全是 supported non-Mastery nodes：
Expected Exit Hash =
FNV1A64:ec1d3ed67d3035d0
04A 不升级 hash schema。
Final Gate 再执行三个独立 Unity process：
Run1 == Run2 == Run3 == ec1d...
如变化：
REJECT / investigate before rebaseline。
24. Mandatory Tests
新增/更新至少覆盖以下行为族：
EveryOnTreeNodeHasDeterministicSupportTruth

StructuralLineDoesNotBlockNodeByItself

ConsumedMeansParserPlusRealRuntimeConsumer

MixedNodeIsBlockedAsWhole

UnsupportedNodeTryAllocateRejectsAtomically

UnsupportedNodeCostsZeroPoints

UnsupportedNodeChangesNoAllocatedState

UnsupportedNodeChangesNoPlayerEffectiveResult

UnsupportedNodeChangesNoSkillEffectiveResult

InjectedBlockedNodeContributesZeroModifiers

SupportedNodeStillAllocatesNormally

SupportedOffensiveNodeChangesActualSkillContext

SupportedDefensiveOrAttributeNodeChangesActualPlayerResult

MasteryTemporarilyCannotAllocate

MasteryImplicitFirstChoiceNoLongerApplies

MasteryTemporaryRejectCostsZero

SpecialWithoutHandlerIsBlocked

UIUsesDomainSupportTruth

NoSecondUnsupportedNodeOracle
名字可调整。
25. PlayMode
至少增加真实 integration coverage：
supported node
→ allocate
→ actual result changes
以及：
unsupported node
→ UI/domain attempt
→ reject
→ point/state/results unchanged
Mastery：
attempt
→ unavailable
→ no effect
若已有 deterministic debug helper，优先使用。
不做脆弱 pixel automation。
26. Test Gate Rule
最终只接受：
ALL DISCOVERED EditMode tests PASS
fail = 0

ALL DISCOVERED PlayMode tests PASS
fail = 0
skip = 0 unless explicitly justified
同时提供：
predecessor contract tests missing = 0
deleted/weakened without explanation = 0
不把具体总数作为未来固定常量。
27. Expected Delta
Gameplay Delta:
YES — passive support/allocatability correctness

Passive Runtime Delta:
YES — single support truth + allocation/consumption gate

Mastery Feature Delta:
NO — only temporary fail-closed state

Parser Delta:
NONE

StatId Delta:
NONE

Canonical Data Delta:
NONE

Content Delta:
NONE

ProdSim Contract Delta:
NONE
28. Acceptance Criteria
AC-01 single Passive support truth owner exists
AC-02 2429 on-tree nodes all receive deterministic runtime status
AC-03 no UNKNOWN allocatability state
AC-04 STRUCTURAL alone does not block

AC-05 CONSUMED requires actual runtime consumer evidence
AC-06 no test-side second modifier interpreter
AC-07 offensive consumer path evidenced
AC-08 player/global consumer path evidenced

AC-09 any blocked gameplay line blocks entire node
AC-10 mixed-node partial application = 0
AC-11 unhandled special interaction blocks allocation

AC-12 unsupported TryAllocate rejects
AC-13 rejected allocation spends zero point
AC-14 rejected allocation changes no allocated state
AC-15 rejected allocation changes no effective player result
AC-16 rejected allocation changes no effective skill result
AC-17 corrupted/injected blocked node contributes zero

AC-18 supported ordinary node remains allocatable
AC-19 supported node's complete effects still apply

AC-20 Mastery currently cannot allocate
AC-21 implicit Mastery choice[0] application = 0
AC-22 Mastery temporary rejection spends zero

AC-23 UI consumes same domain support truth
AC-24 no GUI unsupported-node oracle exists

AC-25 403 unrendered nodes remain source-accounting only
AC-26 blocked domains do not trigger new gameplay implementation

AC-27 PoE source provenance recorded
AC-28 source checksum recorded but excluded from gameplay hash

AC-29 cold-process entry baseline = ec1d... ×3
AC-30 cold-process exit baseline = ec1d... ×3
AC-31 ProdSim canonical hash unchanged

AC-32 all discovered EditMode tests PASS
AC-33 all discovered PlayMode tests PASS
AC-34 predecessor contract tests missing/weakened = 0
AC-35 Content Audit PASS/fresh

AC-36 Parser Delta = NONE
AC-37 StatId Delta = NONE
AC-38 Canonical Data Delta = NONE
AC-39 Content Delta = NONE

AC-40 Drift = 0
AC-41 Source-of-Truth Conflicts = 0
AC-42 Forbidden Expansion Audit = PASS
29. Forbidden Expansion
本单禁止：
Mastery selector
Mastery choice state
Mastery respec

PoeStatParser expansion
new StatId / ModOp
new condition engine

Cold / Lightning / ES
Block / Suppression
Minion / Totem / Warcry
Ailment / Flask
Charge / Leech / DoT

Jewel
Timeless Jewel
Ascendancy

new Skill / Support / Affix

Map/Craft redesign
Passive LOD
texture-memory work

Save/Persistence
new snapshot architecture not already present

ORK
TopDown Engine
vendor framework
30. Required Evidence Pack
Work Order:
S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination

Revision / Working-Tree Fingerprint:

Changed Runtime Files:
Changed UI Files:
Changed Tests:
Changed Tools:
Changed Markdown:
Changed Canonical Data:

Entry Discovered EditMode:
Entry Discovered PlayMode:
Entry Canonical Hash:

Cold Entry Run1:
Cold Entry Run2:
Cold Entry Run3:
Cold Entry Exact:

Passive Source Provenance:
tree_raw SHA-256:

Support Truth Owner:

On-Tree Nodes:
ALLOCATABLE_SUPPORTED:
BLOCKED_CURRENTLY:
SPECIAL_PENDING:
STRUCTURAL/Other:

Mixed Nodes:
Frozen Mixed Node Example:

CONSUMED Lines:
Blocked Lines:
Special Lines:
Structural Lines:

Runtime Consumer Families:
Offensive Skill-Context Evidence:
Player/Global Evidence:

Supported Allocation:
Unsupported Allocation:
Unsupported Point Delta:
Unsupported State Delta:
Unsupported Player Result Delta:
Unsupported Skill Result Delta:

Injected Blocked Node Effect:

Mastery Transitional State:
Mastery Allocation Result:
Implicit First Choice Effect:
Mastery Point Delta:

UI Support Truth Owner:
Second UI Oracle Found:

403 Unrendered Handling:
Blocked Domain Count:

Final Discovered EditMode:
Final EditMode Fail/Skip:
Final Discovered PlayMode:
Final PlayMode Fail/Skip:
Predecessor Tests Missing/Weakened:

Content Audit:

Cold Exit Run1:
Cold Exit Run2:
Cold Exit Run3:
Cold Exit Exact:
Final ProdSim Hash:
Expected Hash:
Hash Unchanged:

Gameplay Delta:
Passive Runtime Delta:
Mastery Feature Delta:
Parser Delta:
StatId Delta:
Canonical Data Delta:
Content Delta:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Defects:
Open Questions:
Recommended Next:
Expected critical values：
Mastery Allocation Result = REJECT
Implicit First Choice Effect = 0
Mastery Point Delta = 0

Parser Delta = NONE
StatId Delta = NONE

Final ProdSim =
FNV1A64:ec1d3ed67d3035d0

Cold Exit Exact = YES

Drift = 0
Source-of-Truth Conflicts = 0
Forbidden Expansion = PASS
31. After WO-04A
若 ACCEPT：
重新释放现有：
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
但增加三条强制 amendment：
Mastery selectable/blocked choice 只能使用 WO-04A support truth；
canonical hash V3 同时清理 stable-key / float encoding / enum-name 依赖；
新 canonical baseline 必须由 3 个独立 Unity cold processes 建立。
WO-03 后：
S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity
它只审计/修复仓库已经存在的：
mask
fixed-width identity
snapshot
clone
map-entry capture
build lock
serializer
reset
不发明 Save/Persistence。
若 04A 证明还有：
“runtime consumer 已存在，但 parser 尚未接上的 effect families”
再插：
S6P-WO-04C — Existing-Consumer Semantic Closure
如果该 bucket = 0，则跳过 04C，直接 WO-05。
32. 双规划渠道治理
从现在起建议固定：
Channel A（本渠道）
= arbitration / executable queue authority

Channel B（第二 GPT）
= independent planning reviewer / second opinion
第二渠道可以：
找漏洞；
提顺序调整；
起草替代 WO；
审历史假设。
但默认输出状态统一视为：
PROPOSED / NOT YET EXECUTABLE
出现冲突：
STOP
→ 把两个方案交给本渠道仲裁
→ 只留下一个 RELEASED queue
不采用“两个渠道并行发令、工作 AI 自己择优执行”。
原因很直接：
无人值守系统最大的敌人不是规划质量稍差，而是同时存在两份都自称 authoritative 的未来状态。
若 Director 明确点名某条第二渠道工作令“立即执行”，则 Director 指令优先；执行后本渠道以真实仓库状态重新 reconciliation。
否则第二渠道就是正式的：
Independent Second Opinion
而不是第二条并行 execution authority。
所以现在状态是：WO-02 仍 ACCEPT；原 WO-03 暂缓但保留；
S6P-WO-04A 正式 RELEASED。 你正在做的 working-tree fingerprint / changed-file inventory 正好可以纳入 04A；另外把“三独立 Unity 进程 ProdSim runner”一起补成 04A 的前置证据工具即可。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：裁决：**改序。采用 `WO-04A → WO-03 → WO-04B → WO-05` 为主序列。** 原已释放的 WO-03 不作废、也不算失败，而是标记为 **`QUEUE-DEFERRED / NOT STARTED`**；先建立唯一 Passive support truth，再让 Mastery 消费这份 truth。第二渠道这次提出的依赖关系判断是成立的。

同时我加两项修正：**WO-02 保持 ACCEPT，不追溯降级**；从现在起 canonical baseline 的最终确认升级为 **3 个独立 Unity 冷启动进程**。另外，原 WO-04 里“现有 consumer 已存在但 parser 仍漏映射”的工作不丢失，后续根据 04A census 决定是否插一个条件性的 `WO-04C`。

:::writing{variant="document" id="28471" title="S6P 执行顺序仲裁与 S6P-WO-04A"}
# S6P Planning Arbitration

## 1. 执行顺序裁决

旧队列：

```text
WO-02
→ WO-03 Mastery
→ WO-04 Semantic Closure
→ WO-05 LOD
```

改为：

```text
WO-02 ACCEPTED
 ↓
WO-04A Passive Support Truth Gate & Silent-Zero Elimination
 ↓
WO-03 Mastery Explicit Selection & Allocation Correctness
 ↓
WO-04B Passive Build Identity / Snapshot / Lock Parity
 ↓
[WO-04C Existing-Consumer Semantic Closure — CONDITIONAL]
 ↓
WO-05 Passive Overview LOD & Texture Residency
```

### 原 WO-03 状态

```text
S6P-WO-03:
RELEASED PREVIOUSLY
→ QUEUE-DEFERRED BEFORE EXECUTION
→ NOT FAILED
→ NOT CANCELLED
```

04A ACCEPT 后重新放行，合同主体保留，但必须使用 04A 建立的 support truth，不得自己建立第二套 Mastery-choice compatibility oracle。

---

# 2. 为什么改序

第二渠道最关键的一点是正确的：

> “能不能完整兑现”是“能不能选择”的前置真相。

现在已经知道：

```text
On-tree nodes = 2429
SUPPORTED = 424
UNSUPPORTED_CURRENTLY = 2005
Mastery = 315
```

如果直接做 WO-03，Mastery selector 必须自行回答：

```text
这个 choice 是否完整支持？
```

随后原 WO-04 又必须回答：

```text
这个 passive/node 是否完整支持？
```

这极易形成：

```text
MasterySupportEvaluator
PassiveSupportEvaluator
```

两套近似但不完全相同的 oracle。

正确依赖应是：

```text
source passive semantics
 ↓
ONE Passive Support Truth
 ↓
normal-node allocation gate
 ↓
Mastery choice gate
 ↓
UI
```

所以 support truth 必须先于 Mastery interaction。

---

# 3. Mastery 过渡态裁定

**授权在 WO-04A 中临时把 Mastery 设为不可分配。**

这是明确的：

```text
Gameplay Delta = YES
```

但属于 bounded correctness/safety correction。

在 WO-03 完成前：

```text
Mastery node
→ SPECIAL_PENDING_MASTERY
→ cannot allocate
→ point cost = 0
→ implicit choice[0] effect = 0
```

允许查看其官方信息，但不得获得 gameplay effect。

这比继续保持：

```text
click
→ spend point
→ silently choice[0]
```

更符合当前已知真相。

## 对 ProdSim hash 的影响

**预期不变。**

WO-02 canonical fixture 明确：

```text
Mastery nodes = 0
Unsupported nodes = 0
```

04A 只会改变这些 fixture 没有触及的非法/未支持 allocation 路径。

因此 04A Exit expected：

```text
FNV1A64:ec1d3ed67d3035d0
```

如果改变：

```text
STOP
→ investigate unexpected canonical truth drift
```

不得直接重新基线。

---

# 4. WO-02 三次独立运行裁定

## 对既有 WO-02

**仍然 ACCEPT。无 retroactive follow-up。**

原合同要求的是：

- 三次 fresh execution；
- 实际重新运行 simulator；
- 非缓存；
- 非复制报告；
- 不得只在同一次计算结果上重复读取。

它没有明确写：

> Unity Editor process 必须退出重启三次。

因此当前三次独立 `run_tests` invocation 在合同原文下有效。

### WO-02 状态不变

```text
S6P-WO-02 = ACCEPT
Canonical =
FNV1A64:ec1d3ed67d3035d0
```

---

## 从现在开始提高标准

第二渠道指出的 static/process contamination 风险成立。

以后凡是：

```text
NEW CANONICAL HASH PROMOTION
```

最终 closure 必须使用：

```text
Unity Process #1 → hash
Unity Process #2 → hash
Unity Process #3 → hash
```

三个独立 Unity 冷启动进程。

优先使用现有可自动化的：

```text
Unity -batchmode -runTests ...
```

或项目等价 runner。

### 04A 的额外 preflight

在任何 gameplay mutation 前，先用新 runner 对当前 WO-02 baseline 做一次：

```text
Cold Process 1 = ec1d...
Cold Process 2 = ec1d...
Cold Process 3 = ec1d...
```

如果不一致：

**停止 04A 产品修改。**

这样既验证旧 baseline，也无需单开 `WO-02-F1`。

---

# 5. Canonical serializer 附加裁定

第二渠道提出的协议风险成立，但不追溯否决 WO-02。

从现在锁定：

## Gameplay hash 的 authority 必须是 stable semantic identity

禁止未来依赖：

```text
Enum.ToString()
localized display name
human-readable stat name
GUI name
dictionary ordering
culture-specific float ToString()
```

作为 canonical identity。

### WO-02 当前冗余名称

如果 `pm/pe/pk` 中同时有：

```text
numeric StatId + readable enum/name
```

现状可以暂时保留，不要求 04A 为此改变 hash。

但是在 **WO-03 本来就要升级 `passive-v2 / V3` 并重新基线** 时：

- hash segment 改为 stable numeric ID / explicit canonical token；
- readable names 移到 report diagnostics；
- float encoding 固定 invariant/canonical representation；
- duplicate modifiers 保留 multiplicity；
- tags/condition identity 明确 canonical sort/encoding。

这样不会额外制造一次纯协议 rebaseline。

---

# 6. Test-count 规则修正

接受第二渠道意见。

以后 Gate 不再使用：

```text
expected exactly 409 tests
```

作为产品门。

正确规则：

```text
all discovered tests PASS
fail = 0
skip = 0 unless explicitly justified
all predecessor contract tests still present
no unexplained deletion/weakening
```

具体：

```text
409/409
422/422
...
```

仍需报告，作为 evidence accounting，但不是固定分母 oracle。

---

# 7. PoE source provenance

04A 开始追加：

```text
tree_raw.json source/version provenance
tree_raw.json checksum
```

推荐 SHA-256。

它属于：

```text
evidence provenance
```

明确**不进入 gameplay FNV hash**。

---

# S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination

**Execution Order:** immediately after accepted WO-02 
**Runtime Gameplay Authority:** `YES — passive allocatability/support gate only` 
**Mastery Feature Authority:** `NO — temporary blocking only` 
**Parser Expansion:** `NO` 
**New StatId / New Gameplay Axis:** `NO` 
**Canonical Data Mutation:** `NO`

## 8. Objective

建立整个 Passive domain 唯一的：

```text
Can this complete passive gameplay promise be fulfilled
by GAME-ZZZ's currently implemented runtime?
```

权威判定。

并消灭：

```text
spend passive point
→ node lights up
→ only supported subset applies
→ unsupported lines silently do nothing
```

这种状态。

核心 invariant：

> **节点只要存在一个不能完整兑现的 gameplay effect，整个节点当前不可分配。**

---

# 9. Support Truth Architecture

建立一个 runtime/domain-owned authority。

名称可按工程风格，例如：

```text
PassiveSupportTruth
PassiveEligibility
PassiveSemanticSupport
```

但只能有**一个规则 owner**。

必须提供至少两类查询：

```text
EvaluateNode(node)
EvaluateChoice(masteryChoice)
```

后者只是给未来 WO-03 消费 support truth。

04A 不实现 Mastery selection。

---

# 10. Line Truth 与 Node Truth 分离

沿用 WO-01 line classification：

```text
CONSUMED
BLOCKED_BY_DOMAIN
SPECIAL_INTERACTION
STRUCTURAL
```

## STRUCTURAL

不属于 gameplay promise。

因此：

```text
STRUCTURAL alone
≠ unsupported
```

括号提示、解释行等不因存在而阻止分配。

但不能使用：

```text
contains '('
```

作为判定规则。

必须根据 semantic classification。

---

# 11. CONSUMED 的严格定义

从 04A 起：

`CONSUMED` 不再仅表示：

```text
PoeStatParser recognized it
```

而必须表示：

```text
source line
→ authoritative parser
→ semantic tuple
→ existing runtime consumer
→ effective gameplay result path exists
```

例如应能证明不同消费链：

```text
AttackSpeed / PhysicalDamage
→ actual Skill context consumer

Life / Strength / Dexterity
→ actual player/global result consumer
```

不得在 test layer 重建第二套 modifier evaluator。

正确链只能是：

```text
source
→ production runtime truth
→ canonical/read-only projection
→ tests/FNV
```

---

# 12. SPECIAL_INTERACTION

若 effect/node 属于 special interaction：

只有存在：

```text
explicit current runtime handler
+
behavior evidence
```

才可视为当前可兑现。

否则：

```text
BLOCKED_SPECIAL_INTERACTION
```

当前至少：

```text
Mastery
Jewel Socket
Timeless/special node
```

不能因为画出来了就算 supported。

---

# 13. Node Allocatability Rule

对于普通 player-allocatable node：

```text
all gameplay effect lines = CONSUMED
AND
all required special interactions handled
→ ALLOCATABLE_SUPPORTED
```

只要存在：

```text
>=1 BLOCKED_BY_DOMAIN
```

或：

```text
unhandled SPECIAL_INTERACTION
```

则：

```text
BLOCKED_CURRENTLY
```

---

# 14. Mixed Node — Mandatory Fail Closed

这是 04A 的核心。

例如：

```text
Line 1 = CONSUMED
Line 2 = CONSUMED
Line 3 = BLOCKED_BY_DOMAIN
```

正确结果：

```text
whole node = BLOCKED_CURRENTLY
```

不得：

```text
apply line1 + line2
ignore line3
```

Evidence 必须找至少一个真实 mixed node 做冻结 regression。

若 authoritative dataset 中客观没有 mixed node，报告 count=0，再以 unit-level semantic fixture 补算法测试；不得伪造它存在。

---

# 15. TryAllocate Gate

`SliceSession.TryAllocate` 或当前唯一 allocation authority 必须调用相同 support truth。

Unsupported node：

```text
TryAllocate
→ deterministic reject
→ point delta = 0
→ allocated-set delta = 0
→ modifier delta = 0
→ player effective-result delta = 0
→ skill effective-result delta = 0
```

必须提供稳定 rejection reason。

---

# 16. Defensive Consumption Gate

除了 allocation gate，还必须防止 corrupted/test-injected state 产生半效果。

如果 allocated collection 中出现一个当前：

```text
BLOCKED_CURRENTLY
```

节点：

它必须：

```text
contribute zero gameplay modifiers
```

并由适合现有架构的 invalid-state evidence 暴露。

不得继续部分消费其中 CONSUMED lines。

不要求新 Persistence/migration。

---

# 17. Mastery Transitional Rule

在 WO-03 前所有 Mastery：

```text
SPECIAL_PENDING_MASTERY
```

### 点击/分配

```text
reject
point delta = 0
```

### Effect

```text
implicit choice[0] = DISABLED
Mastery gameplay modifier = 0
```

### UI

读取相同 domain truth，显示：

```text
currently unavailable / pending explicit mastery selection
```

或等价稳定原因。

不得维护：

```text
masteryBlockedNodeIds[]
```

这样的第二份 GUI truth。

---

# 18. 403 无坐标节点

继续：

```text
SOURCE/CENSUS ACCOUNTING ONLY
```

不得进入：

```text
player allocation eligibility denominator
```

不把它们视为 2429 上树支持率失败。

---

# 19. Missing 70 Domains

继续：

```text
dependency report only
```

不得自动实现：

- Cold；
- Lightning；
- ES；
- Block；
- Suppression；
- Minion；
- Totem；
- Warcry；
- Ailment；
- Flask；
- Charge；
- Leech；
- DoT；
- etc.

这些节点只保持：

```text
BLOCKED_CURRENTLY
```

这不阻塞 04A PASS。

---

# 20. UI Truth

Passive UI 中：

```text
CanClick
Disabled
Reason
```

全部读取 domain authority。

禁止：

```text
UI-specific unsupported list
UI re-runs parser itself
UI checks node text independently
```

视觉只展示真相，不定义真相。

---

# 21. Provenance

Evidence 必须记录：

```text
Passive source file:
tree_raw.json

Source provenance/version:
...

SHA-256:
...
```

Checksum 不进 FNV gameplay hash。

---

# 22. Cold-Process Entry Verification

**必须发生在第一行 production gameplay mutation 之前。**

运行：

```text
Cold Unity #1
Cold Unity #2
Cold Unity #3
```

全部：

```text
FNV1A64:ec1d3ed67d3035d0
```

才能继续。

否则：

```text
STOP
Defect = baseline process-repeatability failure
```

---

# 23. Expected ProdSim Exit

由于 WO-02 fixture 全是 supported non-Mastery nodes：

```text
Expected Exit Hash =
FNV1A64:ec1d3ed67d3035d0
```

04A 不升级 hash schema。

Final Gate 再执行三个独立 Unity process：

```text
Run1 == Run2 == Run3 == ec1d...
```

如变化：

**REJECT / investigate before rebaseline。**

---

# 24. Mandatory Tests

新增/更新至少覆盖以下行为族：

```text
EveryOnTreeNodeHasDeterministicSupportTruth

StructuralLineDoesNotBlockNodeByItself

ConsumedMeansParserPlusRealRuntimeConsumer

MixedNodeIsBlockedAsWhole

UnsupportedNodeTryAllocateRejectsAtomically

UnsupportedNodeCostsZeroPoints

UnsupportedNodeChangesNoAllocatedState

UnsupportedNodeChangesNoPlayerEffectiveResult

UnsupportedNodeChangesNoSkillEffectiveResult

InjectedBlockedNodeContributesZeroModifiers

SupportedNodeStillAllocatesNormally

SupportedOffensiveNodeChangesActualSkillContext

SupportedDefensiveOrAttributeNodeChangesActualPlayerResult

MasteryTemporarilyCannotAllocate

MasteryImplicitFirstChoiceNoLongerApplies

MasteryTemporaryRejectCostsZero

SpecialWithoutHandlerIsBlocked

UIUsesDomainSupportTruth

NoSecondUnsupportedNodeOracle
```

名字可调整。

---

# 25. PlayMode

至少增加真实 integration coverage：

```text
supported node
→ allocate
→ actual result changes
```

以及：

```text
unsupported node
→ UI/domain attempt
→ reject
→ point/state/results unchanged
```

Mastery：

```text
attempt
→ unavailable
→ no effect
```

若已有 deterministic debug helper，优先使用。

不做脆弱 pixel automation。

---

# 26. Test Gate Rule

最终只接受：

```text
ALL DISCOVERED EditMode tests PASS
fail = 0

ALL DISCOVERED PlayMode tests PASS
fail = 0
skip = 0 unless explicitly justified
```

同时提供：

```text
predecessor contract tests missing = 0
deleted/weakened without explanation = 0
```

不把具体总数作为未来固定常量。

---

# 27. Expected Delta

```text
Gameplay Delta:
YES — passive support/allocatability correctness

Passive Runtime Delta:
YES — single support truth + allocation/consumption gate

Mastery Feature Delta:
NO — only temporary fail-closed state

Parser Delta:
NONE

StatId Delta:
NONE

Canonical Data Delta:
NONE

Content Delta:
NONE

ProdSim Contract Delta:
NONE
```

---

# 28. Acceptance Criteria

```text
AC-01 single Passive support truth owner exists
AC-02 2429 on-tree nodes all receive deterministic runtime status
AC-03 no UNKNOWN allocatability state
AC-04 STRUCTURAL alone does not block

AC-05 CONSUMED requires actual runtime consumer evidence
AC-06 no test-side second modifier interpreter
AC-07 offensive consumer path evidenced
AC-08 player/global consumer path evidenced

AC-09 any blocked gameplay line blocks entire node
AC-10 mixed-node partial application = 0
AC-11 unhandled special interaction blocks allocation

AC-12 unsupported TryAllocate rejects
AC-13 rejected allocation spends zero point
AC-14 rejected allocation changes no allocated state
AC-15 rejected allocation changes no effective player result
AC-16 rejected allocation changes no effective skill result
AC-17 corrupted/injected blocked node contributes zero

AC-18 supported ordinary node remains allocatable
AC-19 supported node's complete effects still apply

AC-20 Mastery currently cannot allocate
AC-21 implicit Mastery choice[0] application = 0
AC-22 Mastery temporary rejection spends zero

AC-23 UI consumes same domain support truth
AC-24 no GUI unsupported-node oracle exists

AC-25 403 unrendered nodes remain source-accounting only
AC-26 blocked domains do not trigger new gameplay implementation

AC-27 PoE source provenance recorded
AC-28 source checksum recorded but excluded from gameplay hash

AC-29 cold-process entry baseline = ec1d... ×3
AC-30 cold-process exit baseline = ec1d... ×3
AC-31 ProdSim canonical hash unchanged

AC-32 all discovered EditMode tests PASS
AC-33 all discovered PlayMode tests PASS
AC-34 predecessor contract tests missing/weakened = 0
AC-35 Content Audit PASS/fresh

AC-36 Parser Delta = NONE
AC-37 StatId Delta = NONE
AC-38 Canonical Data Delta = NONE
AC-39 Content Delta = NONE

AC-40 Drift = 0
AC-41 Source-of-Truth Conflicts = 0
AC-42 Forbidden Expansion Audit = PASS
```

---

# 29. Forbidden Expansion

本单禁止：

```text
Mastery selector
Mastery choice state
Mastery respec

PoeStatParser expansion
new StatId / ModOp
new condition engine

Cold / Lightning / ES
Block / Suppression
Minion / Totem / Warcry
Ailment / Flask
Charge / Leech / DoT

Jewel
Timeless Jewel
Ascendancy

new Skill / Support / Affix

Map/Craft redesign
Passive LOD
texture-memory work

Save/Persistence
new snapshot architecture not already present

ORK
TopDown Engine
vendor framework
```

---

# 30. Required Evidence Pack

```text
Work Order:
S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination

Revision / Working-Tree Fingerprint:

Changed Runtime Files:
Changed UI Files:
Changed Tests:
Changed Tools:
Changed Markdown:
Changed Canonical Data:

Entry Discovered EditMode:
Entry Discovered PlayMode:
Entry Canonical Hash:

Cold Entry Run1:
Cold Entry Run2:
Cold Entry Run3:
Cold Entry Exact:

Passive Source Provenance:
tree_raw SHA-256:

Support Truth Owner:

On-Tree Nodes:
ALLOCATABLE_SUPPORTED:
BLOCKED_CURRENTLY:
SPECIAL_PENDING:
STRUCTURAL/Other:

Mixed Nodes:
Frozen Mixed Node Example:

CONSUMED Lines:
Blocked Lines:
Special Lines:
Structural Lines:

Runtime Consumer Families:
Offensive Skill-Context Evidence:
Player/Global Evidence:

Supported Allocation:
Unsupported Allocation:
Unsupported Point Delta:
Unsupported State Delta:
Unsupported Player Result Delta:
Unsupported Skill Result Delta:

Injected Blocked Node Effect:

Mastery Transitional State:
Mastery Allocation Result:
Implicit First Choice Effect:
Mastery Point Delta:

UI Support Truth Owner:
Second UI Oracle Found:

403 Unrendered Handling:
Blocked Domain Count:

Final Discovered EditMode:
Final EditMode Fail/Skip:
Final Discovered PlayMode:
Final PlayMode Fail/Skip:
Predecessor Tests Missing/Weakened:

Content Audit:

Cold Exit Run1:
Cold Exit Run2:
Cold Exit Run3:
Cold Exit Exact:
Final ProdSim Hash:
Expected Hash:
Hash Unchanged:

Gameplay Delta:
Passive Runtime Delta:
Mastery Feature Delta:
Parser Delta:
StatId Delta:
Canonical Data Delta:
Content Delta:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Defects:
Open Questions:
Recommended Next:
```

Expected critical values：

```text
Mastery Allocation Result = REJECT
Implicit First Choice Effect = 0
Mastery Point Delta = 0

Parser Delta = NONE
StatId Delta = NONE

Final ProdSim =
FNV1A64:ec1d3ed67d3035d0

Cold Exit Exact = YES

Drift = 0
Source-of-Truth Conflicts = 0
Forbidden Expansion = PASS
```

---

# 31. After WO-04A

若 ACCEPT：

重新释放现有：

**`S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`**

但增加三条强制 amendment：

1. Mastery selectable/blocked choice **只能使用 WO-04A support truth**；
2. canonical hash V3 同时清理 stable-key / float encoding / enum-name 依赖；
3. 新 canonical baseline 必须由 **3 个独立 Unity cold processes** 建立。

WO-03 后：

**`S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity`**

它只审计/修复仓库已经存在的：

```text
mask
fixed-width identity
snapshot
clone
map-entry capture
build lock
serializer
reset
```

不发明 Save/Persistence。

若 04A 证明还有：

> “runtime consumer 已存在，但 parser 尚未接上的 effect families”

再插：

**`S6P-WO-04C — Existing-Consumer Semantic Closure`**

如果该 bucket = 0，则跳过 04C，直接 WO-05。

---

# 32. 双规划渠道治理

从现在起建议固定：

```text
Channel A（本渠道）
= arbitration / executable queue authority

Channel B（第二 GPT）
= independent planning reviewer / second opinion
```

第二渠道可以：

- 找漏洞；
- 提顺序调整；
- 起草替代 WO；
- 审历史假设。

但默认输出状态统一视为：

```text
PROPOSED / NOT YET EXECUTABLE
```

出现冲突：

```text
STOP
→ 把两个方案交给本渠道仲裁
→ 只留下一个 RELEASED queue
```

**不采用“两个渠道并行发令、工作 AI 自己择优执行”。**

原因很直接：

> 无人值守系统最大的敌人不是规划质量稍差，而是同时存在两份都自称 authoritative 的未来状态。

若 Director **明确点名**某条第二渠道工作令“立即执行”，则 Director 指令优先；执行后本渠道以真实仓库状态重新 reconciliation。

否则第二渠道就是正式的：

**Independent Second Opinion**

而不是第二条并行 execution authority。
:::

所以现在状态是：**WO-02 仍 ACCEPT；原 WO-03 暂缓但保留；`S6P-WO-04A` 正式 RELEASED。** 你正在做的 working-tree fingerprint / changed-file inventory 正好可以纳入 04A；另外把“三独立 Unity 进程 ProdSim runner”一起补成 04A 的前置证据工具即可。
 

 

 

 

 
 
 
 

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