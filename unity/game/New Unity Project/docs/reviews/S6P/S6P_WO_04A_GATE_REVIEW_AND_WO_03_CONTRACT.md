# S6P-WO-04A Gate Review & S6P-WO-03 Re-Release（规划 AI 原文存档）

**渠道：** Channel A（`game-zzz-planning` / `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`）
**时间：** 2026-09-11 08:0x（本地）
**本轮输入：** 工作 AI 的 04A Evidence Pack 回传（`%LOCALAPPDATA%\Temp\game_zzz_wo04a_evidence_prompt.md`）
**回复全文（含 HTML 快照）：** `%LOCALAPPDATA%\Temp\game_zzz_wo04a_gate_review.md`、`%LOCALAPPDATA%\Temp\chatgpt-web-page.html`
**会话复用：** `reused: true`，`session_id` 与登记一致（未发生 fallback）

## 裁定摘要（工作 AI 摘录，原文见下）

- **S6P-WO-04A Gate Review = ACCEPT WITH FOLLOW-UP**：技术/产品 AC **42 / 42 PASS**，
  **Blocking Follow-up = NONE**，Evidence Exception = 1（入口 working-tree fingerprint 缺失，
  记为 `EVIDENCE_EXCEPTION_ACCEPTED`，不得事后伪造，不要求 04A 返工）。
- **珠宝孔第三处 Delta 裁定 = 接受**：57 个 Jewel Socket 由「无普通 effect line ⇒ 旧算法误判可点」
  改为 `BLOCKED_SPECIAL_INTERACTION`，正是 §12 / AC-11 要求；`424 − 57 = 367` 是合理的 truth refinement，
  不构成实现 Jewel system，也没有扩大玩法域。
- **S3R2 适配裁定 = 接受，不判为测试削弱**：旧 E2E 路径被产品规则合法关闭；现形成三层互补证据
  （基石被拒 + 权威 parser→生产 StatBag 聚合 + Support 侧 actual-hit E2E），且 04A 另有
  supported 进攻被动 → 真实技能上下文的正向端到端。
  非阻塞整理要求：WO-03 动到附近代码时把测试名改成不误导后续 AI 的名字（如
  `SupportAndParsedPassiveConversion_ComposeOnSameAxis`），**不属于 04A 返工**。
- **S6P-WO-03 = RE-RELEASED / EXECUTE NOW**，含强制 preflight 与条件分支（见原文 §4）。

---

## 规划 AI 回复原文

Gate 结论：
ACCEPT WITH FOLLOW-UP，Follow-up 为非阻塞证据治理项；
S6P-WO-03 正式重新释放，可继续无人值守执行。
技术/产品层面的 42 条 AC 我判定 42/42 PASS。唯一没有完整满足的是 Amendment 要求的“首次 gameplay mutation 前精确入口 working-tree fingerprint”；它不属于 42 条产品 AC，而且现在无法诚实补造历史指纹，所以记为 Evidence Exception，不要求返工 04A。WO-03 起必须严格先取入口 fingerprint。
S6P-WO-04A Gate Review & S6P-WO-03 Re-Release
S6P-WO-04A Gate Review
Verdict: 
ACCEPT WITH FOLLOW-UP
Technical AC: 
42 / 42 PASS
Blocking Follow-up: 
NONE
Evidence Exception: 
1
Next: 
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
Gate Summary
项
裁定
单一 Passive support truth
PASS
Runtime 不反向依赖 Tests
PASS
2429 节点 deterministic truth
PASS
UNKNOWN
0 / PASS
Mixed node fail-closed
PASS
Unsupported allocation atomic reject
PASS
Defensive consumption gate
PASS
Player/global 正对照
PASS
Skill-context 正对照
PASS
Mastery temporary fail-closed
PASS
implicit 
choice[0]
0 / PASS
UI 使用同一 domain truth
PASS
Parser / StatId expansion
NONE / PASS
Content / canonical data
NONE / PASS
EditMode
426/426, fail=0, skip=0
PlayMode
17/17, fail=0, skip=0
Content Audit
PASS / fresh
Cold exit ×3
exact
ProdSim
FNV1A64:ec1d3ed67d3035d0 unchanged
Drift
0
SoT Conflict
0
Forbidden Expansion
PASS
1. 珠宝孔 Delta 裁定
接受，属于 WO-04A 原合同明确授权范围，不需要单独立项。
57 个 Jewel Socket 从：
无普通 effect line → 被旧算法误判为可点
改为：
BLOCKED_SPECIAL_INTERACTION
正是 §12 / AC-11 所要求的结果。
当前没有 Jewel interaction handler，因此它们不能被归为“完整兑现”。
所以：
旧表面 supported = 424
- 57 Jewel Socket
= 367 ALLOCATABLE_SUPPORTED
是合理的 truth refinement。
这不是实现 Jewel system，也没有扩大玩法域。
后续真正实现 Jewel 时，这 57 个节点才由：
BLOCKED_SPECIAL_INTERACTION
迁移为有真实 handler 的 special interaction。
2. S3R2FireConversionTests 适配裁定
接受，不判为测试削弱。
原因是原测试依赖的产品路径：
allocate Keystone 1650 → passive fire-conversion effect
经过 04A 后已经被产品规则合法关闭：
该节点含当前无法兑现的 
Deal no Non-Fire Damage，因此整个节点必须 fail-closed。
继续强迫旧 E2E 测试通过反而会要求产品违反 04A 的新 invariant。
你现在形成了三层互补证据：
Keystone 1650
→ real allocation rejection evidence

真实 passive 文本
→ authoritative PoeStatParser
→ production StatBag composition evidence

Support
→ existing actual-hit E2E evidence
再加上 04A 已经另有：
SUPPORTED offensive passive → actual Skill context
正向端到端测试，因此整体覆盖没有被掏空。
非阻塞整理要求
测试名如果仍叫：
SupportAndPassiveConversion_ComposeOnSameAxis
但实际上不再执行 Passive allocation，WO-03 修改测试附近代码时顺手改成不会误导后续 AI 的名称，例如：
SupportAndParsedPassiveConversion_ComposeOnSameAxis
或同义准确名称。
这不是 04A rework。
3. Entry Fingerprint 缺失裁定
记为：
EVIDENCE_EXCEPTION_ACCEPTED
不得事后伪造。
已有以下事实足以让我接受 04A：
HEAD 已披露；
mutation 前 cold baseline ×3 已真实完成；
entry Edit/Play 门已知；
final/gate fingerprints 已存在；
exit cold ×3 exact；
canonical hash 未漂移；
changed-file inventory 可审计。
从 WO-03 起规则升级为：
entry fingerprint
→ production mutation
→ tests
→ final fingerprint
顺序不可反。
S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness
Status: 
RE-RELEASED / EXECUTE NOW
04A 已完成，因此 WO-03 不再拥有定义“支持不支持”的权力。
它只负责：
在 04A 的 PassiveSupport truth 上建立 Mastery prerequisite、explicit selection、atomic allocation 与 deterministic evidence。
4. Mandatory Preflight — 在第一行 runtime mutation 前
先记录：
Entry Working-Tree Fingerprint
HEAD
Changed-file inventory
EditMode discovered/pass/fail/skip
PlayMode discovered/pass/fail/skip
Entry ProdSim = ec1d3ed67d3035d0
然后使用 04A 的唯一 support truth 全量统计 315 个 Mastery：
Total Masteries
Total Choices
Choices Fully Supported
Choices Blocked
Masteries >=1 supported choice
Masteries >=2 supported choices
Masteries =0 supported choices
Conditional branch
若：
Masteries with >=1 fully supported choice = 0
则：
STOP BEFORE PRODUCT MUTATION。
不得：
扩 parser；
新增 StatId；
临时把 blocked choice 当 selectable；
为满足 WO-03 制造假 supported choice。
回传：
WO-03 PRECONDITION BLOCKED_BY_EXISTING_DOMAIN
以及 choice census。
此时 canonical hash 必须仍为：
FNV1A64:ec1d3ed67d3035d0
由 Channel A 再裁定是否提前插入 04C。
若 
>=1，继续下面完整 WO-03。
5. Mastery prerequisite
建立单一 domain owner。
Mastery 可进入 selection 阶段必须证明：
Mastery belongs to authoritative official cluster/group
AND
same cluster has >=1 allocated non-Mastery Notable
如果 source data 存在更直接官方 relation，使用更直接 relation。
禁止使用：
GUI 几何；
最近节点；
名称；
localized text；
UI order。
必须有：
same-cluster positive = PASS
unrelated-cluster negative = PASS
no-notable negative = PASS
6. Choice support truth
Mastery choice 的 selectable/blocked 状态：
只能调用 WO-04A PassiveSupport truth。
禁止新增：
MasterySupportTable
MasteryBlockedKeywords
MasterySpecificParserRules
GUIChoiceSupportLogic
完整 choice：
所有 gameplay effect 都可兑现
→ selectable

任何 gameplay effect blocked
→ whole choice blocked
继续 fail-closed。
7. Selection identity
优先使用 source stable ID。
若官方 choice 没 ID：
MasteryNodeId + authoritative source ordinal
作为稳定 identity。
不得使用：
localized text；
filtered GUI index；
GetHashCode()；
runtime object identity。
8. Atomic behavior
打开 selector：
point delta = 0
allocation delta = 0
modifier delta = 0
Cancel/Esc：
all gameplay delta = 0
选择一个 fully-supported choice：
revalidate prerequisite
revalidate passive point
revalidate choice ownership
revalidate PassiveSupport
→ allocate Mastery
→ write explicit choice identity
→ spend exactly 1 point
→ apply selected choice exactly once
失败：
point = unchanged
allocation = unchanged
selection = unchanged
player result = unchanged
skill result = unchanged
严禁恢复：
FirstChoice / choices[0] fallback
9. R reset / reselection
当前没有 refund economy。
因此：
allocated Mastery choice
→ cannot freely switch
当前唯一重新选择方式：
R global reset
→ allocation removed
→ selection removed
→ effects removed
→ allocate again
WO-03 不创造 Mastery respec currency 或免费重选。
10. UI
Selector 只是 domain truth 的视图。
必须显示：
all official choices
supported/blocked state
blocked reason
current selected state
Blocked choice 可见但不能 commit。
机械验证：
entry count == source count
1920×1080 bounds PASS
2560×1440 bounds PASS
long text contained
choice-key ↔ source identity deterministic
视觉美观不是 Gate。
11. ProdSim V3
WO-03 将首次真正纳入 Mastery gameplay identity。
同时完成此前批准的 serializer protocol cleanup：
simulationContractVersion = V3
pv|passive-v2
Canonical identity 禁止依赖：
Enum.ToString()
localized names
culture-dependent float formatting
dictionary iteration
GUI ordering
使用：
stable numeric/token IDs；
invariant canonical float encoding；
deterministic sorting；
duplicate modifier multiplicity preserved；
canonical tags/condition identity。
Readable name 可继续存在报告中，但不作为 hash identity。
12. Hash attribution
WO-03 最终 hash 允许并预期变化，原因可以有两部分：
A. Mastery gameplay truth enters canonical evidence
B. V3 serializer canonical-key cleanup
Evidence 必须分别说明。
不要把中间开发 hash 提升为 baseline。
最终 baseline 只在产品状态冻结后建立。
13. Mastery-sensitive fixture
冻结一个真实 Mastery：
Mastery NodeId
Prerequisite path
Prerequisite Notable
Selected ChoiceKey
Selected semantic tuples
优先选择：
>=2 fully-supported choices
这样必须证明：
same allocation + Choice A → H_A
same allocation + Choice B → H_B
H_A != H_B

restore A → exact H_A
如果只有一个 supported choice：
eligible/unselected
vs
explicitly selected
必须改变 gameplay/hash。
UI open/hover/cancel：
hash unchanged
14. Runtime consumption
选择效果必须走已有 Passive production consumer。
禁止新写第二套：
Mastery modifier evaluator
必须证明：
selected choice effects = exactly once
non-selected choices = zero
blocked choice = zero
missing selection = zero
其中至少实际检查最终：
player/global result，或
skill-context result，
取决于该 canonical Mastery effect 的真实消费链。
只看 modifier list 不够。
15. Required deterministic evidence
必须覆盖：
Mastery prerequisite positive/negative
selector no-mutation
cancel no-mutation
no implicit first choice
supported atomic commit
blocked atomic reject
exact one-point cost
selected effect exactly once
non-selected zero
reset exact
choice identity stable
04A support truth reused
no second Mastery support oracle
UI-only hash invariance
Mastery hash sensitivity
PlayMode 至少有一个稳定的 UI/domain integration flow；优先使用已有 deterministic helper，不强造像素鼠标自动化。
16. Exit Gate
必须：
ALL discovered EditMode PASS
fail = 0

ALL discovered PlayMode PASS
fail = 0
skip = 0 unless explicitly justified

predecessor tests missing = 0
unexplained weakening = 0

Content Audit = PASS / fresh
Canonical Data Delta = NONE
Content Delta = NONE
Parser expansion = NONE
StatId expansion = NONE
Drift = 0
SoT conflicts = 0
Forbidden Expansion = PASS
最终新 canonical baseline 必须由：
Unity cold process #1
Unity cold process #2
Unity cold process #3
三个独立冷启动运行得到：
H1 == H2 == H3
才可 promotion。
17. WO-03 Forbidden Expansion
仍禁止：
general passive semantic expansion
Cold / Lightning / ES
Block / Suppression
Minion / Totem / Warcry
Ailment / Flask
Charge / Leech / DoT

Jewel / Timeless Jewel
Ascendancy

new Skill / Support / Affix

refund economy
Save/Persistence

Map/Craft redesign
Passive LOD
texture-memory work

ORK / TopDown / vendor framework
18. Evidence Pack 增补字段
除原 WO-03 Evidence Pack 外，本次必须额外包含：
Entry Working-Tree Fingerprint:
Entry Changed-File Inventory:

Mastery Choice Census:
Total Choices:
Fully Supported Choices:
Blocked Choices:
Masteries >=1 Supported:
Masteries >=2 Supported:
Masteries Zero Supported:

PassiveSupport Owner Used:
Second Mastery Support Oracle Found:

Serializer Stable-Key Scheme:
Float Canonical Encoding:
Enum Display Names In Hash:
Duplicate Multiplicity Preserved:

Final Working-Tree Fingerprint:

Cold Process Run1:
Cold Process Run2:
Cold Process Run3:
New Canonical Hash:
19. Expected Next
若 WO-03 完整 ACCEPT：
S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity
它将审计所有已有：
fixed-width masks
allocation identity
snapshot
clone
map-entry capture
build lock
serializer
reset
但不发明 Save/Persistence。
如果 WO-03 preflight 证明 
0 fully-supported Mastery choices，则不执行上述 product mutation，返回 blocker evidence，由 Channel A 决定是否把条件性 
WO-04C — Existing-Consumer Semantic Closure 提前。
所以这轮最重要的裁定有三个：57 个珠宝孔被封锁是正确且已授权；S3R2 的变化属于产品语义改变后的合法测试迁移，不算削弱；入口 fingerprint 缺失记一次不可追补的 evidence exception，不阻塞继续。
S6P-WO-03 现在正式进入 RELEASED。
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
ChatGPT 说：**Gate 结论：`ACCEPT WITH FOLLOW-UP`，Follow-up 为非阻塞证据治理项；`S6P-WO-03` 正式重新释放，可继续无人值守执行。**

技术/产品层面的 42 条 AC 我判定 **42/42 PASS**。唯一没有完整满足的是 Amendment 要求的“首次 gameplay mutation 前精确入口 working-tree fingerprint”；它不属于 42 条产品 AC，而且现在无法诚实补造历史指纹，所以记为 **Evidence Exception**，不要求返工 04A。WO-03 起必须严格先取入口 fingerprint。

:::writing{variant="document" id="84326" title="S6P-WO-04A Gate Review & S6P-WO-03 Re-Release"}
# S6P-WO-04A Gate Review

**Verdict:** `ACCEPT WITH FOLLOW-UP` 
**Technical AC:** `42 / 42 PASS` 
**Blocking Follow-up:** `NONE` 
**Evidence Exception:** `1` 
**Next:** `S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`

## Gate Summary

| 项 | 裁定 |
|---|---|
| 单一 Passive support truth | **PASS** |
| Runtime 不反向依赖 Tests | **PASS** |
| 2429 节点 deterministic truth | **PASS** |
| UNKNOWN | **0 / PASS** |
| Mixed node fail-closed | **PASS** |
| Unsupported allocation atomic reject | **PASS** |
| Defensive consumption gate | **PASS** |
| Player/global 正对照 | **PASS** |
| Skill-context 正对照 | **PASS** |
| Mastery temporary fail-closed | **PASS** |
| implicit `choice[0]` | **0 / PASS** |
| UI 使用同一 domain truth | **PASS** |
| Parser / StatId expansion | **NONE / PASS** |
| Content / canonical data | **NONE / PASS** |
| EditMode | **426/426, fail=0, skip=0** |
| PlayMode | **17/17, fail=0, skip=0** |
| Content Audit | **PASS / fresh** |
| Cold exit ×3 | **exact** |
| ProdSim | **`FNV1A64:ec1d3ed67d3035d0` unchanged** |
| Drift | **0** |
| SoT Conflict | **0** |
| Forbidden Expansion | **PASS** |

---

## 1. 珠宝孔 Delta 裁定

**接受，属于 WO-04A 原合同明确授权范围，不需要单独立项。**

57 个 Jewel Socket 从：

`无普通 effect line → 被旧算法误判为可点`

改为：

`BLOCKED_SPECIAL_INTERACTION`

正是 §12 / AC-11 所要求的结果。

当前没有 Jewel interaction handler，因此它们不能被归为“完整兑现”。

所以：

```text
旧表面 supported = 424
- 57 Jewel Socket
= 367 ALLOCATABLE_SUPPORTED
```

是合理的 truth refinement。

这不是实现 Jewel system，也没有扩大玩法域。

后续真正实现 Jewel 时，这 57 个节点才由：

`BLOCKED_SPECIAL_INTERACTION`

迁移为有真实 handler 的 special interaction。

---

## 2. S3R2FireConversionTests 适配裁定

**接受，不判为测试削弱。**

原因是原测试依赖的产品路径：

`allocate Keystone 1650 → passive fire-conversion effect`

经过 04A 后已经被产品规则**合法关闭**：

该节点含当前无法兑现的 `Deal no Non-Fire Damage`，因此整个节点必须 fail-closed。

继续强迫旧 E2E 测试通过反而会要求产品违反 04A 的新 invariant。

你现在形成了三层互补证据：

```text
Keystone 1650
→ real allocation rejection evidence

真实 passive 文本
→ authoritative PoeStatParser
→ production StatBag composition evidence

Support
→ existing actual-hit E2E evidence
```

再加上 04A 已经另有：

`SUPPORTED offensive passive → actual Skill context`

正向端到端测试，因此整体覆盖没有被掏空。

### 非阻塞整理要求

测试名如果仍叫：

`SupportAndPassiveConversion_ComposeOnSameAxis`

但实际上不再执行 Passive allocation，WO-03 修改测试附近代码时顺手改成不会误导后续 AI 的名称，例如：

`SupportAndParsedPassiveConversion_ComposeOnSameAxis`

或同义准确名称。

这不是 04A rework。

---

## 3. Entry Fingerprint 缺失裁定

记为：

`EVIDENCE_EXCEPTION_ACCEPTED`

不得事后伪造。

已有以下事实足以让我接受 04A：

- HEAD 已披露；
- mutation 前 cold baseline ×3 已真实完成；
- entry Edit/Play 门已知；
- final/gate fingerprints 已存在；
- exit cold ×3 exact；
- canonical hash 未漂移；
- changed-file inventory 可审计。

从 WO-03 起规则升级为：

```text
entry fingerprint
→ production mutation
→ tests
→ final fingerprint
```

顺序不可反。

---

# S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness

**Status:** `RE-RELEASED / EXECUTE NOW`

04A 已完成，因此 WO-03 不再拥有定义“支持不支持”的权力。

它只负责：

> **在 04A 的 PassiveSupport truth 上建立 Mastery prerequisite、explicit selection、atomic allocation 与 deterministic evidence。**

## 4. Mandatory Preflight — 在第一行 runtime mutation 前

先记录：

```text
Entry Working-Tree Fingerprint
HEAD
Changed-file inventory
EditMode discovered/pass/fail/skip
PlayMode discovered/pass/fail/skip
Entry ProdSim = ec1d3ed67d3035d0
```

然后使用 **04A 的唯一 support truth** 全量统计 315 个 Mastery：

```text
Total Masteries
Total Choices
Choices Fully Supported
Choices Blocked
Masteries >=1 supported choice
Masteries >=2 supported choices
Masteries =0 supported choices
```

### Conditional branch

若：

```text
Masteries with >=1 fully supported choice = 0
```

则：

**STOP BEFORE PRODUCT MUTATION。**

不得：

- 扩 parser；
- 新增 StatId；
- 临时把 blocked choice 当 selectable；
- 为满足 WO-03 制造假 supported choice。

回传：

`WO-03 PRECONDITION BLOCKED_BY_EXISTING_DOMAIN`

以及 choice census。

此时 canonical hash 必须仍为：

`FNV1A64:ec1d3ed67d3035d0`

由 Channel A 再裁定是否提前插入 04C。

若 `>=1`，继续下面完整 WO-03。

---

## 5. Mastery prerequisite

建立单一 domain owner。

Mastery 可进入 selection 阶段必须证明：

```text
Mastery belongs to authoritative official cluster/group
AND
same cluster has >=1 allocated non-Mastery Notable
```

如果 source data 存在更直接官方 relation，使用更直接 relation。

禁止使用：

- GUI 几何；
- 最近节点；
- 名称；
- localized text；
- UI order。

必须有：

```text
same-cluster positive = PASS
unrelated-cluster negative = PASS
no-notable negative = PASS
```

---

## 6. Choice support truth

Mastery choice 的 selectable/blocked 状态：

**只能调用 WO-04A PassiveSupport truth。**

禁止新增：

```text
MasterySupportTable
MasteryBlockedKeywords
MasterySpecificParserRules
GUIChoiceSupportLogic
```

完整 choice：

```text
所有 gameplay effect 都可兑现
→ selectable

任何 gameplay effect blocked
→ whole choice blocked
```

继续 fail-closed。

---

## 7. Selection identity

优先使用 source stable ID。

若官方 choice 没 ID：

```text
MasteryNodeId + authoritative source ordinal
```

作为稳定 identity。

不得使用：

- localized text；
- filtered GUI index；
- `GetHashCode()`；
- runtime object identity。

---

## 8. Atomic behavior

打开 selector：

```text
point delta = 0
allocation delta = 0
modifier delta = 0
```

Cancel/Esc：

```text
all gameplay delta = 0
```

选择一个 fully-supported choice：

```text
revalidate prerequisite
revalidate passive point
revalidate choice ownership
revalidate PassiveSupport
→ allocate Mastery
→ write explicit choice identity
→ spend exactly 1 point
→ apply selected choice exactly once
```

失败：

```text
point = unchanged
allocation = unchanged
selection = unchanged
player result = unchanged
skill result = unchanged
```

严禁恢复：

`FirstChoice / choices[0] fallback`

---

## 9. R reset / reselection

当前没有 refund economy。

因此：

```text
allocated Mastery choice
→ cannot freely switch
```

当前唯一重新选择方式：

```text
R global reset
→ allocation removed
→ selection removed
→ effects removed
→ allocate again
```

WO-03 不创造 Mastery respec currency 或免费重选。

---

## 10. UI

Selector 只是 domain truth 的视图。

必须显示：

```text
all official choices
supported/blocked state
blocked reason
current selected state
```

Blocked choice 可见但不能 commit。

机械验证：

```text
entry count == source count
1920×1080 bounds PASS
2560×1440 bounds PASS
long text contained
choice-key ↔ source identity deterministic
```

视觉美观不是 Gate。

---

## 11. ProdSim V3

WO-03 将首次真正纳入 Mastery gameplay identity。

同时完成此前批准的 serializer protocol cleanup：

```text
simulationContractVersion = V3
pv|passive-v2
```

Canonical identity 禁止依赖：

```text
Enum.ToString()
localized names
culture-dependent float formatting
dictionary iteration
GUI ordering
```

使用：

- stable numeric/token IDs；
- invariant canonical float encoding；
- deterministic sorting；
- duplicate modifier multiplicity preserved；
- canonical tags/condition identity。

Readable name 可继续存在报告中，但不作为 hash identity。

---

## 12. Hash attribution

WO-03 最终 hash **允许并预期变化**，原因可以有两部分：

```text
A. Mastery gameplay truth enters canonical evidence
B. V3 serializer canonical-key cleanup
```

Evidence 必须分别说明。

不要把中间开发 hash 提升为 baseline。

最终 baseline 只在产品状态冻结后建立。

---

## 13. Mastery-sensitive fixture

冻结一个真实 Mastery：

```text
Mastery NodeId
Prerequisite path
Prerequisite Notable
Selected ChoiceKey
Selected semantic tuples
```

优先选择：

`>=2 fully-supported choices`

这样必须证明：

```text
same allocation + Choice A → H_A
same allocation + Choice B → H_B
H_A != H_B

restore A → exact H_A
```

如果只有一个 supported choice：

```text
eligible/unselected
vs
explicitly selected
```

必须改变 gameplay/hash。

UI open/hover/cancel：

`hash unchanged`

---

## 14. Runtime consumption

选择效果必须走已有 Passive production consumer。

禁止新写第二套：

`Mastery modifier evaluator`

必须证明：

```text
selected choice effects = exactly once
non-selected choices = zero
blocked choice = zero
missing selection = zero
```

其中至少实际检查最终：

- player/global result，或
- skill-context result，

取决于该 canonical Mastery effect 的真实消费链。

只看 modifier list 不够。

---

## 15. Required deterministic evidence

必须覆盖：

```text
Mastery prerequisite positive/negative
selector no-mutation
cancel no-mutation
no implicit first choice
supported atomic commit
blocked atomic reject
exact one-point cost
selected effect exactly once
non-selected zero
reset exact
choice identity stable
04A support truth reused
no second Mastery support oracle
UI-only hash invariance
Mastery hash sensitivity
```

PlayMode 至少有一个稳定的 UI/domain integration flow；优先使用已有 deterministic helper，不强造像素鼠标自动化。

---

## 16. Exit Gate

必须：

```text
ALL discovered EditMode PASS
fail = 0

ALL discovered PlayMode PASS
fail = 0
skip = 0 unless explicitly justified

predecessor tests missing = 0
unexplained weakening = 0

Content Audit = PASS / fresh
Canonical Data Delta = NONE
Content Delta = NONE
Parser expansion = NONE
StatId expansion = NONE
Drift = 0
SoT conflicts = 0
Forbidden Expansion = PASS
```

最终新 canonical baseline 必须由：

```text
Unity cold process #1
Unity cold process #2
Unity cold process #3
```

三个独立冷启动运行得到：

`H1 == H2 == H3`

才可 promotion。

---

## 17. WO-03 Forbidden Expansion

仍禁止：

```text
general passive semantic expansion
Cold / Lightning / ES
Block / Suppression
Minion / Totem / Warcry
Ailment / Flask
Charge / Leech / DoT

Jewel / Timeless Jewel
Ascendancy

new Skill / Support / Affix

refund economy
Save/Persistence

Map/Craft redesign
Passive LOD
texture-memory work

ORK / TopDown / vendor framework
```

---

## 18. Evidence Pack 增补字段

除原 WO-03 Evidence Pack 外，本次必须额外包含：

```text
Entry Working-Tree Fingerprint:
Entry Changed-File Inventory:

Mastery Choice Census:
Total Choices:
Fully Supported Choices:
Blocked Choices:
Masteries >=1 Supported:
Masteries >=2 Supported:
Masteries Zero Supported:

PassiveSupport Owner Used:
Second Mastery Support Oracle Found:

Serializer Stable-Key Scheme:
Float Canonical Encoding:
Enum Display Names In Hash:
Duplicate Multiplicity Preserved:

Final Working-Tree Fingerprint:

Cold Process Run1:
Cold Process Run2:
Cold Process Run3:
New Canonical Hash:
```

---

## 19. Expected Next

若 WO-03 完整 ACCEPT：

`S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity`

它将审计所有已有：

```text
fixed-width masks
allocation identity
snapshot
clone
map-entry capture
build lock
serializer
reset
```

但不发明 Save/Persistence。

如果 WO-03 preflight 证明 `0 fully-supported Mastery choices`，则不执行上述 product mutation，返回 blocker evidence，由 Channel A 决定是否把条件性 `WO-04C — Existing-Consumer Semantic Closure` 提前。
:::

所以这轮最重要的裁定有三个：**57 个珠宝孔被封锁是正确且已授权；S3R2 的变化属于产品语义改变后的合法测试迁移，不算削弱；入口 fingerprint 缺失记一次不可追补的 evidence exception，不阻塞继续。**

`S6P-WO-03` 现在正式进入 **RELEASED**。
 

 

 

 

 
 
 
 

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
标题 1
要求更改
CtrlK
文本
