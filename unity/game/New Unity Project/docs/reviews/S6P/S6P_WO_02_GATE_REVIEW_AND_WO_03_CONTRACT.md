结论：
S6P-WO-02 = ACCEPT，31/31 AC 全部满足，Follow-up = NONE。

新的 authoritative ProdSim baseline 正式变为 
FNV1A64:ec1d3ed67d3035d0；
9a4c… 从现在起只保留 predecessor reference。正式释放 
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness。
S6P-WO-02 Gate Review
S6P-WO-02 Gate Review — Passive-Aware Production Simulation
Verdict: 
ACCEPT
Acceptance: 
31 / 31
Follow-up: 
NONE
Next Primary: 
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
1. AC-01 ～ AC-31 逐项核对
AC
要求
Evidence Review
结果
AC-01
Gameplay Delta = NONE
Runtime gameplay files 未改
PASS
AC-02
Passive Runtime Delta = NONE
Passive domain/runtime 未改
PASS
AC-03
Canonical Data Delta = NONE
tree_raw/节点定义未改
PASS
AC-04
Content Delta = NONE
Skill/Support/Affix/Passive source 无内容变化
PASS
AC-05
ProdSim 有显式 Passive scenario
s6p-wo02-canonical-v1
PASS
AC-06
Fixture 仅使用 SUPPORTED nodes
559/1795/2034 全为 SUPPORTED
PASS
AC-07
Mastery=0
明确为 0
PASS
AC-08
Unsupported=0
明确为 0；71 仅作 rejection sentinel
PASS
AC-09
经 domain allocation path
全部走 
SliceSession.TryAllocate
PASS
AC-10
allocated identity 入 hash
`ps
`
PASS
AC-11
effective modifiers 入 hash
`pm
`
PASS
AC-12
effective gameplay stats 入 hash
`pe
+pk
`
PASS
AC-13
合法 allocation mutation 改 hash
ec1d… → 32b2…
PASS
AC-14
合法 effective-stat mutation 改 hash
ec1d… → 4fc1…
PASS
AC-15
恢复 canonical 精确恢复 hash
回到 
ec1d3ed67d3035d0
PASS
AC-16
collection/order 不影响 hash
逆序 allocation 同 hash
PASS
AC-17
UI/tree geometry 不进 hash
forbidden surface 明确排除并守卫
PASS
AC-18
不 indiscriminately hash 原树 bytes
tree_raw.json/2429 definitions 均排除
PASS
AC-19
Passive sensitivity = TRUE
census sentinel 已翻转
PASS
AC-20
Mastery sensitivity deferred
NO / DEFERRED TO WO-03
PASS
AC-21
3 次 fresh independent ProdSim
三次独立重跑、报告重生、非缓存
PASS
AC-22
三次 hash exact match
3/3 
ec1d…
PASS
AC-23
新 canonical hash 建立
FNV1A64:ec1d3ed67d3035d0
PASS
AC-24
9a4c… 仅 predecessor
Evidence 正确降级
PASS
AC-25
EditMode all PASS
409/409
PASS
AC-26
PlayMode all PASS
14/14，0 skip
PASS
AC-27
Content Audit fresh PASS
PASS / fresh
PASS
AC-28
predecessor tests 未删除/削弱
394 全保留 +15
PASS
AC-29
Drift=0
0
PASS
AC-30
SoT conflicts=0
census classification 已单真值化
PASS
AC-31
Forbidden Expansion PASS
parser/StatId/Mastery/新域均未越界
PASS
Final: 
31 / 31 PASS.
2. 
pe| raw components 裁定
正式接受并锁定。
只 hash 最终 
Get() 的方案存在确定性盲区，例如：
base PhysicalDamage = 0
increased = +10%
final = 0
如果只记录 
final=0，就无法区分：
modifier absent
与：
modifier present but current base is zero
因此 
base / flat / increased / more 等权威组合分量属于 gameplay semantic state，允许进入 canonical evidence。
ProductionSimulator 的职责仍然只是读取权威结果并规范序列化，不能复制 modifier calculation。
3. 
pk| Skill-side snapshot 裁定
正式接受并锁定。
当前部分进攻 Passive 通过 
CollectSkillMods 进入技能侧，而不是 
PlayerStats。
所以：
pe = player/global effective stat truth
pk = canonical Skill-side effective stat truth
两者都需要。
否则未来完全可能出现：
Passive allocated
pm tuple correct
PlayerStats unchanged
Skill consumer accidentally broken
ProdSim still green
加入 
pk| 正好关闭这个缺口。
4. Allocation events 不进入 hash
接受。
pa|... 保留在 report 作为 provenance/evidence 是正确的，但最终 canonical hash 以最终 gameplay state为核心。
这使：
559 → 1795 → 2034
与：
2034 → 1795 → 559
只要合法且最终状态相同，就能得到相同结果。
这个性质已经被实际 mutation test 证明，不只是设计声明。
5. Canonical Baseline Promotion
旧：
FNV1A64:9a4c9524d0b3e214
状态改为：
PREDECESSOR REFERENCE
新 authoritative S6P baseline：
FNV1A64:ec1d3ed67d3035d0
Evidence：
Run1 = ec1d3ed67d3035d0
Run2 = ec1d3ed67d3035d0
Run3 = ec1d3ed67d3035d0

Exact Match = YES
Passive Sensitive = YES
Mastery Sensitive = NO / DEFERRED
后续任何不改变 Passive gameplay truth 或 canonical evidence contract的 WO 都应保持 
ec1d…。
WO-03 是例外：它将修正 Mastery gameplay truth，所以允许并预期再次建立新 hash。
6. Git 未提交状态
本单 31 条 AC 没有把“必须 git commit”列为 acceptance criterion，因此：
不构成 WO-02 failure，也不产生 follow-up。
但从 WO-03 起 Evidence 必须至少明确给出：
Entry revision / working-tree fingerprint
Changed-file inventory
Gate execution对应的最终文件状态
如果周期仍采用“不自动 git write”的策略，可以继续不 commit；但必须保证 Gate 与 Evidence 能唯一对应到同一 working-tree state。
不允许测试 A 状态、随后修改文件，再用 A 状态结果申报 B 状态通过。
7. Final State
S6P-WO-02 = ACCEPT
Follow-up = NONE

Passive-sensitive ProdSim = SUPPORTED
Mastery-sensitive ProdSim = NOT YET

Canonical Hash =
FNV1A64:ec1d3ed67d3035d0

Next =
S6P-WO-03
下面正式放行下一令。WO-03 的重点不是“做一个 Mastery 弹窗”，而是一次 domain correctness correction：消灭默认首条、建立 prerequisite、显式 choice state、原子点数事务，并让 ProdSim 第一次真正对 Mastery choice 敏感。
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
Cycle: S6P — Passive Truth & Build Backbone
Primary: Mastery gameplay correctness
Passive Gameplay Authority: 
YES — Mastery only
General Passive Semantic Expansion: 
NO
Parser Expansion: 
NO
New StatId / ModOp: 
NO
Canonical Data Mutation: 
NO
1. Objective
移除当前：
Mastery
→ choices 存在
→ choice[0] 隐式生效
改成：
满足 Mastery prerequisite
→ 点击 Mastery
→ 显式选择一个完整可消费 choice
→ 原子分配 Mastery
→ 消耗 exactly 1 passive point
→ only selected effect 生效
核心 invariant：
没有显式 Mastery selection，就绝对没有 Mastery gameplay effect。
2. Entry Baseline
EditMode = 409/409
PlayMode = 14/14

Canonical ProdSim =
FNV1A64:ec1d3ed67d3035d0

Passive Sensitive = YES
Mastery Sensitive = NO

Mastery Nodes = 315
Implicit First Choice = PRESENT
Mastery Prerequisite = ABSENT
3. Mastery Prerequisite
必须建立单一 domain authority。
默认规则：
Mastery M 可进入选择阶段
IFF
同一 authoritative official cluster/group
至少一个非 Mastery Notable 已合法分配
若官方 source data 有比 group membership 更精确的直接关联字段，则优先使用官方关系，并在 Evidence 披露。
禁止根据：
屏幕距离；
node name；
localized text；
最近 notable；
UI 顺序；
推断 prerequisite。
必须证明：
same-cluster allocated notable → eligible
unrelated-cluster notable → not eligible
no notable → not eligible
4. Choice State
建立 session/domain-owned 状态：
MasteryNodeId → SelectedChoiceKey
不得存在于纯 UI state。
Choice identity 优先级：
source-provided stable ID；
若无，则 
MasteryNodeId + authoritative source ordinal。
禁止使用：
localized text；
render index after filtering；
object hash code；
GUI control ID。
5. Eliminate Implicit Choice 0
必须删除所有等价行为：
missing selection → choices[0]
First()
FirstOrDefault()
index 0 fallback
正确结果：
selection missing
→ mastery allocation/effect = NONE
不得为了旧 fixture compatibility 静默写回 first choice。
6. Allocation Transaction
点击 eligible Mastery：
open selector
point delta = 0
allocation delta = 0
effect delta = 0
选择 supported choice 后执行单次原子事务：
validate prerequisite
validate point
validate choice ownership
validate complete choice semantics consumable
→ allocate
→ store selection
→ spend exactly 1
→ apply selected choice exactly once
任何步骤失败：
allocation delta = 0
selection delta = 0
point delta = 0
modifier delta = 0
7. Unsupported Choices
315 个 Mastery 的官方 choices 必须全部保持可见真相。
Choice classification 使用现有 census/parser authority：
fully CONSUMED
→ selectable

contains BLOCKED_BY_DOMAIN
→ disabled

contains incomplete/unsupported condition
→ disabled
禁止：
隐藏 unsupported choice；
只执行其中支持的行；
丢掉 condition 后执行数字；
为了让 choice 可选新增 StatId/parser rule。
8. Zero-supported Mastery
如果某 Mastery：
supported choices = 0
正确行为：
selector may show all official choices
all choices disabled
point spend = 0
Mastery cannot commit
不是 fallback 到首条。
Evidence 必须统计：
Masteries with >=1 supported choice
Masteries with >=2 supported choices
Masteries with zero supported choices
9. Reselection / Reset
本 WO 不引入 refund economy。
已分配 Mastery：
可查看 current selected choice；
不允许免费重新选择。
改变 choice 的唯一当前路径：
R global reset
→重新分配
→重新选择
R 必须清除：
Mastery allocation
Mastery selection map
Mastery effects
且恢复当前既有 point-reset truth。
10. UI
在现有 fullscreen Passive surface 中增加 bounded deterministic selector。
要求显示：
Mastery identity
all official choices
supported / blocked state
selected state
blocked reason
机械交互：
supported choice click → atomic commit
blocked choice click → no mutation
Esc/cancel → no mutation
必须保留：
P/Tab；
zoom；
pan；
hover；
normal allocation；
R reset。
11. UI Mechanical Gate
不以“看着漂亮”为通过条件。
必须测试：
choice entry count == official source choice count

choice-key mapping stable

popup inside 1920×1080
popup inside 2560×1440

long text contained/wrapped
no choice hit rect overlaps illegally

supported/disabled mapping deterministic
若现有 IMGUI debug interaction helper 可用，优先用 deterministic helper，不做脆弱鼠标像素脚本。
12. Runtime Consumption
Mastery effect 必须复用普通 Passive 的既有 semantic consumer。
禁止另写：
MasteryStatsAggregator
去复制规则。
对一个 selected fully-supported choice：
selected lines → exactly once
non-selected → zero
多行 choice：
所有 gameplay effect line 全部支持才允许 commit。
否则整 choice blocked。
13. Sentinel Flip
WO-01 当前：
MasteryPrerequisite = FALSE
WO-03 Exit：
MasteryPrerequisite = TRUE
必须是行为测试翻转，不只是新增 helper。
14. ProdSim V3 / Passive-v2
升级 canonical evidence contract，例如：
simulationContractVersion = V3
pv|passive-v2
Mastery gameplay truth 必须进入 hash：
Mastery NodeId
Selected Choice Key
selected semantic tuples
resulting effective pe/pk truth
明确排除：
popup open
hover
GUI control
visual selection highlight
scroll position
localized text
choice rendering order
15. Canonical Mastery Fixture
从真实官方树 + 当前 semantic census 中确定一个固定 Mastery。
要求：
legal reachable prerequisite path
same-cluster notable
>=1 fully supported choice
优先选：
>=2 fully supported choices
以便直接证明 A/B choice sensitivity。
一旦选定，冻结：
Mastery NodeId
Prerequisite Path
Notable NodeId
Choice Key
Choice semantic tuples
禁止运行时 find-first-supported。
16. No Suitable Mastery Fallback
如果 census 真跑证明：
fully supported Mastery choices = 0
不得扩 parser 强行制造一个。
仍完成：
prerequisite；
explicit state；
no fallback；
zero-supported handling；
reset；
并 Evidence 标：
ProdSim Mastery semantic fixture =
BLOCKED_BY_EXISTING_DOMAIN
这种情况下不得伪造 AC-23/24/25 的 gameplay sensitivity PASS；Planning Gate 将据真实数据重新裁定 WO-04 顺序。
17. Mastery Sensitivity
若 canonical Mastery 有 >=2 supported choices：
same allocations + Choice A = H_A
same allocations + Choice B = H_B

H_A != H_B

restore A
→ exact H_A
同时：
open/cancel/hover UI only
→ hash unchanged
若只有一个 supported choice：
eligible but unselected
vs
selected supported choice
→ hash changes
18. New Canonical Hash
ec1d3ed67d3035d0 作为 predecessor reference。
完成所有 gameplay change 后：
Run1
Run2
Run3
三次 fresh independent run：
Hash1 == Hash2 == Hash3
建立新的 S6P canonical baseline。
Evidence 必须同时报告 invocation/run timestamp，避免“同一缓存读取”歧义。
19. Mandatory Tests
至少新增覆盖：
MasteryRequiresSameClusterAllocatedNotable
UnrelatedNotableDoesNotUnlockMastery
OpeningSelectorDoesNotSpendPoint
CancelSelectorDoesNotMutateState
MissingSelectionProducesNoEffect
NoImplicitFirstChoiceFallback
SupportedChoiceCommitsAtomically
SuccessfulMasteryCostsExactlyOnePoint
UnsupportedChoiceRejectsAtomically
SelectedChoiceAppliesExactlyOnce
NonSelectedChoicesApplyZeroTimes
PartialChoiceCannotApply
DuplicateAllocationDoesNotDoubleSpend
GlobalResetClearsSelection
GlobalResetRemovesEffect
ChoiceIdentityStable
MasteryPrerequisiteSentinelTrue
PopupChoiceCountMatchesOfficialData
PopupBounds1920
PopupBounds2560
ProdSimMasterySensitive
UIOnlyStateHashInvariant
可以多于上述数量。
不得删除/削弱 predecessor tests。
20. PlayMode
若已有稳定 deterministic UI helper，新增至少一个：
open tree
→ allocate prerequisite notable
→ open Mastery
→ select supported choice
→ verify point
→ verify effect
→ R reset
→ effect removed
若当前 IMGUI harness 不适合可靠点击，可以用既有 debug interaction path。
禁止为了满足 PlayMode 数字造 fragile pixel automation。
现有 14 条 predecessor PlayMode 必须全部继续通过。
21. Expected Delta
Gameplay Delta =
YES — Mastery correctness only

Passive Runtime Delta =
Mastery prerequisite
explicit selection state
selected-choice consumption

Canonical Data Delta = NONE
Content Delta = NONE
Parser Delta = NONE
StatId Delta = NONE
New Gameplay Domain = NONE
22. Forbidden Expansion
禁止本单实现：
general unsupported-node rejection
general semantic closure

Cold / Lightning
Energy Shield
Block / Suppression
Ailment
Minion / Totem / Warcry
Flask / Charge / Leech / DoT

Jewel
Timeless Jewel
Ascendancy

new Skill
new Support
new Affix

Map/Craft UI overhaul
Passive LOD
texture-memory optimization

refund currency
partial respec economy

ORK
TopDown Engine
other vendor framework
23. Acceptance Criteria
必须全部满足，除 §16 的“官方数据客观无 supported Mastery”特殊阻塞路径外：
AC-01 prerequisite 有单一 domain owner
AC-02 same-cluster Notable required
AC-03 unrelated Notable cannot unlock
AC-04 implicit choice[0] fallback = 0

AC-05 selector open mutation = 0
AC-06 cancel mutation = 0
AC-07 supported choice atomic commit
AC-08 successful point cost = exactly 1
AC-09 failed point cost = 0
AC-10 missing selection effect = 0

AC-11 allocated Mastery selected choices = exactly 1
AC-12 selected effects apply exactly once
AC-13 non-selected effects apply zero times
AC-14 unsupported complete choice cannot allocate
AC-15 partial semantic application = 0

AC-16 R clears allocation/selection/effect
AC-17 no refund economy introduced
AC-18 MasteryPrerequisite sentinel = TRUE

AC-19 official choices represented
AC-20 supported/blocked mapping exact
AC-21 choice identity stable/source-derived
AC-22 1080p + 1440p popup geometry PASS

AC-23 ProdSim Mastery-sensitive
AC-24 choice identity enters hash
AC-25 effective Mastery result enters hash
AC-26 UI-only Mastery state hash-invariant

AC-27 three fresh final ProdSim runs exact-match
AC-28 new canonical hash established

AC-29 EditMode ALL PASS
AC-30 PlayMode ALL PASS
AC-31 Content Audit PASS/fresh

AC-32 Canonical Data Delta = NONE
AC-33 Content Delta = NONE
AC-34 Parser/Stat expansion = NONE

AC-35 Drift = 0
AC-36 Source-of-Truth Conflicts = 0
AC-37 Forbidden Expansion = PASS
24. Evidence Pack
Work Order: S6P-WO-03
Revision / Working-Tree Fingerprint:

Changed Passive Runtime Files:
Changed UI Files:
Changed Tests:
Changed Simulation Files:
Changed Markdown:
Changed Canonical Data:

Entry EditMode:
Entry PlayMode:
Entry Canonical Hash:

Mastery Nodes:
Choice Identity Scheme:

Prerequisite Owner:
Prerequisite Source Relation:
Same-Cluster Positive:
Unrelated-Cluster Negative:

Implicit First Choice Before:
Implicit First Choice After:

Masteries With >=1 Supported Choice:
Masteries With >=2 Supported Choices:
Zero-Supported Masteries:

Canonical Mastery Node:
Prerequisite Path:
Prerequisite Notable:
Canonical Choice:
Alternative Supported Choice:

Open Selector Point Delta:
Cancel Point Delta:
Success Point Delta:
Failed Point Delta:

Missing Selection Effect:
Selected Choice Count:
Selected Effect Count:
NonSelected Effect Count:
Partial Choice Application:

R Reset:
Selection State After Reset:
Effect After Reset:

Reselection Rule:

MasteryPrerequisite Sentinel:

ProdSim Contract Version:
Predecessor Hash:

Choice A Hash:
Choice B Hash:
Choice Sensitivity:
Restore Exact:
UI-Only Hash Invariance:

ProdSim Run1 Timestamp/Hash:
ProdSim Run2 Timestamp/Hash:
ProdSim Run3 Timestamp/Hash:
All Three Exact Match:
New Canonical Hash:

EditMode:
PlayMode:
Content Audit:
Content Audit Fresh:

Gameplay Delta:
Passive Runtime Delta:
Canonical Data Delta:
Content Delta:
Parser Delta:
StatId Delta:
New Gameplay Domain:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Environment Incidents:
Defects:
Open Questions:
Recommended Next WO:
Expected next, if ACCEPT:
S6P-WO-04 — Supported Passive Semantic Closure & No-Silent-Noop Guard
无人值守授权现在正式切换到 
S6P-WO-03。 当前 
ec1d3ed67d3035d0 是 WO-03 的入口基线；本单允许它因为 Mastery gameplay truth 被正确化而变化，但最终仍必须以 3 次独立 exact-repeat 建立新的 canonical hash。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：结论：**`S6P-WO-02 = ACCEPT`，31/31 AC 全部满足，Follow-up = NONE。** 
新的 authoritative ProdSim baseline 正式变为 **`FNV1A64:ec1d3ed67d3035d0`**；`9a4c…` 从现在起只保留 predecessor reference。正式释放 **`S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`**。

:::writing{variant="document" id="41763" title="S6P-WO-02 Gate Review"}
# S6P-WO-02 Gate Review — Passive-Aware Production Simulation

**Verdict:** `ACCEPT` 
**Acceptance:** `31 / 31` 
**Follow-up:** `NONE` 
**Next Primary:** `S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`

## 1. AC-01 ～ AC-31 逐项核对

| AC | 要求 | Evidence Review | 结果 |
|---|---|---|---|
| AC-01 | Gameplay Delta = NONE | Runtime gameplay files 未改 | **PASS** |
| AC-02 | Passive Runtime Delta = NONE | Passive domain/runtime 未改 | **PASS** |
| AC-03 | Canonical Data Delta = NONE | `tree_raw`/节点定义未改 | **PASS** |
| AC-04 | Content Delta = NONE | Skill/Support/Affix/Passive source 无内容变化 | **PASS** |
| AC-05 | ProdSim 有显式 Passive scenario | `s6p-wo02-canonical-v1` | **PASS** |
| AC-06 | Fixture 仅使用 SUPPORTED nodes | 559/1795/2034 全为 SUPPORTED | **PASS** |
| AC-07 | Mastery=0 | 明确为 0 | **PASS** |
| AC-08 | Unsupported=0 | 明确为 0；71 仅作 rejection sentinel | **PASS** |
| AC-09 | 经 domain allocation path | 全部走 `SliceSession.TryAllocate` | **PASS** |
| AC-10 | allocated identity 入 hash | `ps|` | **PASS** |
| AC-11 | effective modifiers 入 hash | `pm|` | **PASS** |
| AC-12 | effective gameplay stats 入 hash | `pe|` + `pk|` | **PASS** |
| AC-13 | 合法 allocation mutation 改 hash | `ec1d… → 32b2…` | **PASS** |
| AC-14 | 合法 effective-stat mutation 改 hash | `ec1d… → 4fc1…` | **PASS** |
| AC-15 | 恢复 canonical 精确恢复 hash | 回到 `ec1d3ed67d3035d0` | **PASS** |
| AC-16 | collection/order 不影响 hash | 逆序 allocation 同 hash | **PASS** |
| AC-17 | UI/tree geometry 不进 hash | forbidden surface 明确排除并守卫 | **PASS** |
| AC-18 | 不 indiscriminately hash 原树 bytes | `tree_raw.json`/2429 definitions 均排除 | **PASS** |
| AC-19 | Passive sensitivity = TRUE | census sentinel 已翻转 | **PASS** |
| AC-20 | Mastery sensitivity deferred | `NO / DEFERRED TO WO-03` | **PASS** |
| AC-21 | 3 次 fresh independent ProdSim | 三次独立重跑、报告重生、非缓存 | **PASS** |
| AC-22 | 三次 hash exact match | 3/3 `ec1d…` | **PASS** |
| AC-23 | 新 canonical hash 建立 | `FNV1A64:ec1d3ed67d3035d0` | **PASS** |
| AC-24 | `9a4c…` 仅 predecessor | Evidence 正确降级 | **PASS** |
| AC-25 | EditMode all PASS | 409/409 | **PASS** |
| AC-26 | PlayMode all PASS | 14/14，0 skip | **PASS** |
| AC-27 | Content Audit fresh PASS | PASS / fresh | **PASS** |
| AC-28 | predecessor tests 未删除/削弱 | 394 全保留 +15 | **PASS** |
| AC-29 | Drift=0 | 0 | **PASS** |
| AC-30 | SoT conflicts=0 | census classification 已单真值化 | **PASS** |
| AC-31 | Forbidden Expansion PASS | parser/StatId/Mastery/新域均未越界 | **PASS** |

**Final: `31 / 31 PASS`.**

---

# 2. `pe|` raw components 裁定

**正式接受并锁定。**

只 hash 最终 `Get()` 的方案存在确定性盲区，例如：

```text
base PhysicalDamage = 0
increased = +10%
final = 0
```

如果只记录 `final=0`，就无法区分：

```text
modifier absent
```

与：

```text
modifier present but current base is zero
```

因此 `base / flat / increased / more` 等权威组合分量属于 gameplay semantic state，允许进入 canonical evidence。

ProductionSimulator 的职责仍然只是**读取权威结果并规范序列化**，不能复制 modifier calculation。

---

# 3. `pk|` Skill-side snapshot 裁定

**正式接受并锁定。**

当前部分进攻 Passive 通过 `CollectSkillMods` 进入技能侧，而不是 `PlayerStats`。

所以：

```text
pe = player/global effective stat truth
pk = canonical Skill-side effective stat truth
```

两者都需要。

否则未来完全可能出现：

```text
Passive allocated
pm tuple correct
PlayerStats unchanged
Skill consumer accidentally broken
ProdSim still green
```

加入 `pk|` 正好关闭这个缺口。

---

# 4. Allocation events 不进入 hash

**接受。**

`pa|...` 保留在 report 作为 provenance/evidence 是正确的，但最终 canonical hash 以**最终 gameplay state**为核心。

这使：

```text
559 → 1795 → 2034
```

与：

```text
2034 → 1795 → 559
```

只要合法且最终状态相同，就能得到相同结果。

这个性质已经被实际 mutation test 证明，不只是设计声明。

---

# 5. Canonical Baseline Promotion

旧：

```text
FNV1A64:9a4c9524d0b3e214
```

状态改为：

```text
PREDECESSOR REFERENCE
```

新 authoritative S6P baseline：

```text
FNV1A64:ec1d3ed67d3035d0
```

Evidence：

```text
Run1 = ec1d3ed67d3035d0
Run2 = ec1d3ed67d3035d0
Run3 = ec1d3ed67d3035d0

Exact Match = YES
Passive Sensitive = YES
Mastery Sensitive = NO / DEFERRED
```

后续任何**不改变 Passive gameplay truth 或 canonical evidence contract**的 WO 都应保持 `ec1d…`。

WO-03 是例外：它将修正 Mastery gameplay truth，所以允许并预期再次建立新 hash。

---

# 6. Git 未提交状态

本单 31 条 AC 没有把“必须 git commit”列为 acceptance criterion，因此：

**不构成 WO-02 failure，也不产生 follow-up。**

但从 WO-03 起 Evidence 必须至少明确给出：

```text
Entry revision / working-tree fingerprint
Changed-file inventory
Gate execution对应的最终文件状态
```

如果周期仍采用“不自动 git write”的策略，可以继续不 commit；但必须保证 Gate 与 Evidence 能唯一对应到同一 working-tree state。

不允许测试 A 状态、随后修改文件，再用 A 状态结果申报 B 状态通过。

---

# 7. Final State

```text
S6P-WO-02 = ACCEPT
Follow-up = NONE

Passive-sensitive ProdSim = SUPPORTED
Mastery-sensitive ProdSim = NOT YET

Canonical Hash =
FNV1A64:ec1d3ed67d3035d0

Next =
S6P-WO-03
```
:::

下面正式放行下一令。WO-03 的重点不是“做一个 Mastery 弹窗”，而是一次 **domain correctness correction**：消灭默认首条、建立 prerequisite、显式 choice state、原子点数事务，并让 ProdSim 第一次真正对 Mastery choice 敏感。

:::writing{variant="document" id="56218" title="S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness"}
# S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness

**Cycle:** S6P — Passive Truth & Build Backbone 
**Primary:** Mastery gameplay correctness 
**Passive Gameplay Authority:** `YES — Mastery only` 
**General Passive Semantic Expansion:** `NO` 
**Parser Expansion:** `NO` 
**New StatId / ModOp:** `NO` 
**Canonical Data Mutation:** `NO`

## 1. Objective

移除当前：

```text
Mastery
→ choices 存在
→ choice[0] 隐式生效
```

改成：

```text
满足 Mastery prerequisite
→ 点击 Mastery
→ 显式选择一个完整可消费 choice
→ 原子分配 Mastery
→ 消耗 exactly 1 passive point
→ only selected effect 生效
```

核心 invariant：

> **没有显式 Mastery selection，就绝对没有 Mastery gameplay effect。**

---

# 2. Entry Baseline

```text
EditMode = 409/409
PlayMode = 14/14

Canonical ProdSim =
FNV1A64:ec1d3ed67d3035d0

Passive Sensitive = YES
Mastery Sensitive = NO

Mastery Nodes = 315
Implicit First Choice = PRESENT
Mastery Prerequisite = ABSENT
```

---

# 3. Mastery Prerequisite

必须建立**单一 domain authority**。

默认规则：

```text
Mastery M 可进入选择阶段
IFF
同一 authoritative official cluster/group
至少一个非 Mastery Notable 已合法分配
```

若官方 source data 有比 group membership 更精确的直接关联字段，则优先使用官方关系，并在 Evidence 披露。

禁止根据：

- 屏幕距离；
- node name；
- localized text；
- 最近 notable；
- UI 顺序；

推断 prerequisite。

必须证明：

```text
same-cluster allocated notable → eligible
unrelated-cluster notable → not eligible
no notable → not eligible
```

---

# 4. Choice State

建立 session/domain-owned 状态：

```text
MasteryNodeId → SelectedChoiceKey
```

不得存在于纯 UI state。

Choice identity 优先级：

1. source-provided stable ID；
2. 若无，则 `MasteryNodeId + authoritative source ordinal`。

禁止使用：

- localized text；
- render index after filtering；
- object hash code；
- GUI control ID。

---

# 5. Eliminate Implicit Choice 0

必须删除所有等价行为：

```text
missing selection → choices[0]
First()
FirstOrDefault()
index 0 fallback
```

正确结果：

```text
selection missing
→ mastery allocation/effect = NONE
```

不得为了旧 fixture compatibility 静默写回 first choice。

---

# 6. Allocation Transaction

点击 eligible Mastery：

```text
open selector
point delta = 0
allocation delta = 0
effect delta = 0
```

选择 supported choice 后执行单次原子事务：

```text
validate prerequisite
validate point
validate choice ownership
validate complete choice semantics consumable
→ allocate
→ store selection
→ spend exactly 1
→ apply selected choice exactly once
```

任何步骤失败：

```text
allocation delta = 0
selection delta = 0
point delta = 0
modifier delta = 0
```

---

# 7. Unsupported Choices

315 个 Mastery 的官方 choices 必须全部保持可见真相。

Choice classification 使用现有 census/parser authority：

```text
fully CONSUMED
→ selectable

contains BLOCKED_BY_DOMAIN
→ disabled

contains incomplete/unsupported condition
→ disabled
```

禁止：

- 隐藏 unsupported choice；
- 只执行其中支持的行；
- 丢掉 condition 后执行数字；
- 为了让 choice 可选新增 StatId/parser rule。

---

# 8. Zero-supported Mastery

如果某 Mastery：

```text
supported choices = 0
```

正确行为：

```text
selector may show all official choices
all choices disabled
point spend = 0
Mastery cannot commit
```

不是 fallback 到首条。

Evidence 必须统计：

```text
Masteries with >=1 supported choice
Masteries with >=2 supported choices
Masteries with zero supported choices
```

---

# 9. Reselection / Reset

本 WO 不引入 refund economy。

已分配 Mastery：

- 可查看 current selected choice；
- 不允许免费重新选择。

改变 choice 的唯一当前路径：

```text
R global reset
→重新分配
→重新选择
```

`R` 必须清除：

```text
Mastery allocation
Mastery selection map
Mastery effects
```

且恢复当前既有 point-reset truth。

---

# 10. UI

在现有 fullscreen Passive surface 中增加 bounded deterministic selector。

要求显示：

```text
Mastery identity
all official choices
supported / blocked state
selected state
blocked reason
```

机械交互：

```text
supported choice click → atomic commit
blocked choice click → no mutation
Esc/cancel → no mutation
```

必须保留：

- P/Tab；
- zoom；
- pan；
- hover；
- normal allocation；
- R reset。

---

# 11. UI Mechanical Gate

不以“看着漂亮”为通过条件。

必须测试：

```text
choice entry count == official source choice count

choice-key mapping stable

popup inside 1920×1080
popup inside 2560×1440

long text contained/wrapped
no choice hit rect overlaps illegally

supported/disabled mapping deterministic
```

若现有 IMGUI debug interaction helper 可用，优先用 deterministic helper，不做脆弱鼠标像素脚本。

---

# 12. Runtime Consumption

Mastery effect 必须复用普通 Passive 的既有 semantic consumer。

禁止另写：

```text
MasteryStatsAggregator
```

去复制规则。

对一个 selected fully-supported choice：

```text
selected lines → exactly once
non-selected → zero
```

多行 choice：

> 所有 gameplay effect line 全部支持才允许 commit。

否则整 choice blocked。

---

# 13. Sentinel Flip

WO-01 当前：

```text
MasteryPrerequisite = FALSE
```

WO-03 Exit：

```text
MasteryPrerequisite = TRUE
```

必须是行为测试翻转，不只是新增 helper。

---

# 14. ProdSim V3 / Passive-v2

升级 canonical evidence contract，例如：

```text
simulationContractVersion = V3
pv|passive-v2
```

Mastery gameplay truth 必须进入 hash：

```text
Mastery NodeId
Selected Choice Key
selected semantic tuples
resulting effective pe/pk truth
```

明确排除：

```text
popup open
hover
GUI control
visual selection highlight
scroll position
localized text
choice rendering order
```

---

# 15. Canonical Mastery Fixture

从真实官方树 + 当前 semantic census 中确定一个固定 Mastery。

要求：

```text
legal reachable prerequisite path
same-cluster notable
>=1 fully supported choice
```

优先选：

```text
>=2 fully supported choices
```

以便直接证明 A/B choice sensitivity。

一旦选定，冻结：

```text
Mastery NodeId
Prerequisite Path
Notable NodeId
Choice Key
Choice semantic tuples
```

禁止运行时 find-first-supported。

---

# 16. No Suitable Mastery Fallback

如果 census 真跑证明：

```text
fully supported Mastery choices = 0
```

不得扩 parser 强行制造一个。

仍完成：

- prerequisite；
- explicit state；
- no fallback；
- zero-supported handling；
- reset；

并 Evidence 标：

```text
ProdSim Mastery semantic fixture =
BLOCKED_BY_EXISTING_DOMAIN
```

这种情况下不得伪造 AC-23/24/25 的 gameplay sensitivity PASS；Planning Gate 将据真实数据重新裁定 WO-04 顺序。

---

# 17. Mastery Sensitivity

若 canonical Mastery 有 >=2 supported choices：

```text
same allocations + Choice A = H_A
same allocations + Choice B = H_B

H_A != H_B

restore A
→ exact H_A
```

同时：

```text
open/cancel/hover UI only
→ hash unchanged
```

若只有一个 supported choice：

```text
eligible but unselected
vs
selected supported choice
→ hash changes
```

---

# 18. New Canonical Hash

`ec1d3ed67d3035d0` 作为 predecessor reference。

完成所有 gameplay change 后：

```text
Run1
Run2
Run3
```

三次 fresh independent run：

```text
Hash1 == Hash2 == Hash3
```

建立新的 S6P canonical baseline。

Evidence 必须同时报告 invocation/run timestamp，避免“同一缓存读取”歧义。

---

# 19. Mandatory Tests

至少新增覆盖：

```text
MasteryRequiresSameClusterAllocatedNotable
UnrelatedNotableDoesNotUnlockMastery
OpeningSelectorDoesNotSpendPoint
CancelSelectorDoesNotMutateState
MissingSelectionProducesNoEffect
NoImplicitFirstChoiceFallback
SupportedChoiceCommitsAtomically
SuccessfulMasteryCostsExactlyOnePoint
UnsupportedChoiceRejectsAtomically
SelectedChoiceAppliesExactlyOnce
NonSelectedChoicesApplyZeroTimes
PartialChoiceCannotApply
DuplicateAllocationDoesNotDoubleSpend
GlobalResetClearsSelection
GlobalResetRemovesEffect
ChoiceIdentityStable
MasteryPrerequisiteSentinelTrue
PopupChoiceCountMatchesOfficialData
PopupBounds1920
PopupBounds2560
ProdSimMasterySensitive
UIOnlyStateHashInvariant
```

可以多于上述数量。

不得删除/削弱 predecessor tests。

---

# 20. PlayMode

若已有稳定 deterministic UI helper，新增至少一个：

```text
open tree
→ allocate prerequisite notable
→ open Mastery
→ select supported choice
→ verify point
→ verify effect
→ R reset
→ effect removed
```

若当前 IMGUI harness 不适合可靠点击，可以用既有 debug interaction path。

**禁止为了满足 PlayMode 数字造 fragile pixel automation。**

现有 14 条 predecessor PlayMode 必须全部继续通过。

---

# 21. Expected Delta

```text
Gameplay Delta =
YES — Mastery correctness only

Passive Runtime Delta =
Mastery prerequisite
explicit selection state
selected-choice consumption

Canonical Data Delta = NONE
Content Delta = NONE
Parser Delta = NONE
StatId Delta = NONE
New Gameplay Domain = NONE
```

---

# 22. Forbidden Expansion

禁止本单实现：

```text
general unsupported-node rejection
general semantic closure

Cold / Lightning
Energy Shield
Block / Suppression
Ailment
Minion / Totem / Warcry
Flask / Charge / Leech / DoT

Jewel
Timeless Jewel
Ascendancy

new Skill
new Support
new Affix

Map/Craft UI overhaul
Passive LOD
texture-memory optimization

refund currency
partial respec economy

ORK
TopDown Engine
other vendor framework
```

---

# 23. Acceptance Criteria

必须全部满足，除 §16 的“官方数据客观无 supported Mastery”特殊阻塞路径外：

```text
AC-01 prerequisite 有单一 domain owner
AC-02 same-cluster Notable required
AC-03 unrelated Notable cannot unlock
AC-04 implicit choice[0] fallback = 0

AC-05 selector open mutation = 0
AC-06 cancel mutation = 0
AC-07 supported choice atomic commit
AC-08 successful point cost = exactly 1
AC-09 failed point cost = 0
AC-10 missing selection effect = 0

AC-11 allocated Mastery selected choices = exactly 1
AC-12 selected effects apply exactly once
AC-13 non-selected effects apply zero times
AC-14 unsupported complete choice cannot allocate
AC-15 partial semantic application = 0

AC-16 R clears allocation/selection/effect
AC-17 no refund economy introduced
AC-18 MasteryPrerequisite sentinel = TRUE

AC-19 official choices represented
AC-20 supported/blocked mapping exact
AC-21 choice identity stable/source-derived
AC-22 1080p + 1440p popup geometry PASS

AC-23 ProdSim Mastery-sensitive
AC-24 choice identity enters hash
AC-25 effective Mastery result enters hash
AC-26 UI-only Mastery state hash-invariant

AC-27 three fresh final ProdSim runs exact-match
AC-28 new canonical hash established

AC-29 EditMode ALL PASS
AC-30 PlayMode ALL PASS
AC-31 Content Audit PASS/fresh

AC-32 Canonical Data Delta = NONE
AC-33 Content Delta = NONE
AC-34 Parser/Stat expansion = NONE

AC-35 Drift = 0
AC-36 Source-of-Truth Conflicts = 0
AC-37 Forbidden Expansion = PASS
```

---

# 24. Evidence Pack

```text
Work Order: S6P-WO-03
Revision / Working-Tree Fingerprint:

Changed Passive Runtime Files:
Changed UI Files:
Changed Tests:
Changed Simulation Files:
Changed Markdown:
Changed Canonical Data:

Entry EditMode:
Entry PlayMode:
Entry Canonical Hash:

Mastery Nodes:
Choice Identity Scheme:

Prerequisite Owner:
Prerequisite Source Relation:
Same-Cluster Positive:
Unrelated-Cluster Negative:

Implicit First Choice Before:
Implicit First Choice After:

Masteries With >=1 Supported Choice:
Masteries With >=2 Supported Choices:
Zero-Supported Masteries:

Canonical Mastery Node:
Prerequisite Path:
Prerequisite Notable:
Canonical Choice:
Alternative Supported Choice:

Open Selector Point Delta:
Cancel Point Delta:
Success Point Delta:
Failed Point Delta:

Missing Selection Effect:
Selected Choice Count:
Selected Effect Count:
NonSelected Effect Count:
Partial Choice Application:

R Reset:
Selection State After Reset:
Effect After Reset:

Reselection Rule:

MasteryPrerequisite Sentinel:

ProdSim Contract Version:
Predecessor Hash:

Choice A Hash:
Choice B Hash:
Choice Sensitivity:
Restore Exact:
UI-Only Hash Invariance:

ProdSim Run1 Timestamp/Hash:
ProdSim Run2 Timestamp/Hash:
ProdSim Run3 Timestamp/Hash:
All Three Exact Match:
New Canonical Hash:

EditMode:
PlayMode:
Content Audit:
Content Audit Fresh:

Gameplay Delta:
Passive Runtime Delta:
Canonical Data Delta:
Content Delta:
Parser Delta:
StatId Delta:
New Gameplay Domain:

Drift:
Source-of-Truth Conflicts:
Forbidden Expansion Audit:

Environment Incidents:
Defects:
Open Questions:
Recommended Next WO:
```

**Expected next, if ACCEPT:**

`S6P-WO-04 — Supported Passive Semantic Closure & No-Silent-Noop Guard`
:::

**无人值守授权现在正式切换到 `S6P-WO-03`。** 当前 `ec1d3ed67d3035d0` 是 WO-03 的入口基线；本单允许它因为 Mastery gameplay truth 被正确化而变化，但最终仍必须以 **3 次独立 exact-repeat** 建立新的 canonical hash。
 

 

 

 

 
 
 
 

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
文本
要求更改
CtrlK
文本