# S6P-WO-04A2 — Channel A Gate Review（原文）

| 项 | 值 |
|---|---|
| 令号 | S6P-WO-04A2 — Passive Traversal / Effect Separation |
| 审阅对象 | commit `c57f55f`（commit-pinned 审计） |
| 裁定 | **ACCEPT — AC-1..AC-20 = 20 / 20 PASS** |
| Product rework | NONE |
| Rebaseline | NONE |
| Blocking technical follow-up | NONE |
| Administrative follow-up | REQUIRED before WO-03 product mutation（纯治理收尾；不需第二次 Gate Review，**前提是该收尾 commit 不含产品代码 / 测试逻辑 / 资产 / 数据变化**） |
| 后继 | **S6P-WO-03 = RELEASED / EXECUTE NOW** |
| 渠道 | Channel A `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab` |
| 页面 | https://leopard-x.memofun.net/c/6aa368c5-7478-83ea-a2d3-95003ee4e6ab |
| 落库时间 | 2026-09-11 11:1x |

> 以下为规划 AI 回复原文（未经改写）。执行侧的收尾动作与偏差登记见本文件末节。

---

我已经按 
c57f55f 的 commit-pinned 内容完成审计。结论很清楚：产品 Gate 可以过；没有发现需要返工 WO-04A2 产品代码的 blocking defect。 下面给正式裁定，并把两个持久治理文件的小矛盾一起收口。
S6P-WO-04A2 — Channel A Gate Review
S6P-WO-04A2 — Gate Review
Reviewed commit: 
c57f55f
Verdict: ACCEPT — 20 / 20 AC PASS
Product rework: NONE
Rebaseline: NONE
Blocking technical follow-up: NONE
Administrative follow-up: REQUIRED before WO-03 product mutation; no second Gate Review required
Successor: S6P-WO-03 = RELEASED
GitHub 上 
c57f55f 的实际提交确实包含了 truth split、
14 → 1985 reachability 恢复及对应 runtime/test/document changes；commit-pinned 代码与提交说明一致。()
AC 判定
AC
Verdict
Gate Review
AC-1 Preflight baseline
PASS
入口 compile 0、EditMode 462/462、PlayMode 17/17、census 367/1660/87/315、legacy reach 14/2429、PF-6 candidates=311；入口冷进程三次均 
ec1d3ed67d3035d0。
AC-2 Two-dimensional truth
PASS
EffectTruth 与 
TraversalTruth 均在 runtime canonical owner 中明确存在。
AC-3 Single owner
PASS
ClassifyTruth 是节点双真值分类源；旧 
NodeStatus 只是从同一 truth 派生的诊断投影。
AC-4 No shared boolean authority
PASS
分配使用 
IsTraversable(TraversalTruth)；modifier 消费使用 
YieldsModifiers(EffectTruth)，职责已经物理拆开。
AC-5 Supported ordinary mapping
PASS
367 个 ordinary supported = 
FULLY_SUPPORTED + TRAVERSABLE。
AC-6 Blocked ordinary mapping
PASS
1660 个 ordinary blocked = 
UNFULFILLED + TRAVERSABLE。
AC-7 Mixed mapping
PASS
mixed fixtures 71/12/255 均为 route-only，并逐段证明 
pm/pe/pk 零泄漏。
AC-8 Route-only allocation
PASS
frozen path 经 production 
TryAllocate 真实分配、真实扣点、真实进入 selected，并继续到 183。
AC-9 Effect gate preserved
PASS
RecalcPlayer、
CollectSkillMods 和 ProdSim 
ModifierTuples 都只让 
FULLY_SUPPORTED 贡献 modifier。
AC-10 Mastery blocked
PASS
Mastery 为 
SPECIAL_PENDING + SPECIAL_BLOCKED；allocation reject，零扣点、零 selected mutation。
AC-11 Jewel blocked
PASS
Jewel Socket 使用相同 special-blocked negative control，零 mutation。
AC-12 Timeless/special blocked
PASS
locked/special 同样保持 special-blocked；未借 reachability 解锁。
AC-13 No traversal bypass
PASS
non-adjacent 与 
Unspent=0 均继续拒绝，并检查零状态变化。
AC-14 Frozen recovery fixture
PASS
target=183；path=
[2172,71,183]；71=route-only；legacy false、新 runtime true；candidate=311。
AC-15 Reachability
PASS
R_ref=R_runtime=1985，
D=325，325/325，missing=
[]。关于其构造性见下方专项裁定。
AC-16 Frozen census
PASS
367 / 1660 / 87 / 315 未变；402 special-blocked 正好为 87+315。
AC-17 Regression suites
PASS
出口 compile 0；EditMode 475/475；PlayMode 18/18；旧语义反转均有显式 supersession 记录，删除测试=0、静默弱化=0。
AC-18 Canonical ProdSim
PASS
出口三个独立冷进程全部 
FNV1A64:ec1d3ed67d3035d0，未 rebaseline。
AC-19 ProdSim sensitivity
PASS
route-only 71：
ps DIFFERENT；
pm/pe/pk EXACT MATCH；测试走 public allocation path。
AC-20 Scope discipline
PASS
实现记录未出现新 StatId/ModOp/parser/Mastery selector/Jewel/Timeless/新技能/新轴/第三组/存档/LOD/rebaseline；runtime 产品修改集中于 PassiveSupport/SliceSession/SliceHud。
因此，AC-1..AC-20 = 20/20 PASS。
AC-15 构造性问题裁定
确认：你按字面实现的语义就是本合同要的语义，不修改 
G_ref 定义，不要求补测。
合同明确规定 
G_ref 包含普通 in-domain nodes，同时删除 
SPECIAL_BLOCKED 与 
OUT_OF_DOMAIN；生产 
G_runtime 又明确只让 
TRAVERSABLE 节点作 transit。在 04A2 的 truth matrix 下，普通节点恰好全部是 
TRAVERSABLE，special 恰好全部是 
SPECIAL_BLOCKED，所以二者的 vertex/transit 集合本来就应一致。
因此 
D ⊆ R_ref 再验证 
D ⊆ R_runtime 确实具有你指出的构造性。这里的门禁价值不是证明一个不同的“理想图”，而是同时锁住三件事：生产 BFS 没有实现偏差、普通 unsupported 节点真的恢复为 transit、special 没有为了凑可达率偷偷变成 transit。你们用两份独立 BFS 实现逐节点比较，再辅以旧图 
14、新图 
1985 和真实 recovery fixture 
[2172,71,183]，已经满足这个目的。
不要把 
G_ref 改成“special 也可通行”。那会与本令的核心负向不变量 
Mastery/Jewel/Timeless = SPECIAL_BLOCKED、不可作 transit 直接冲突，实际上是在通过测试定义偷偷扩大 Jewel/Timeless 职权。
所以：AC-15 PASS，无 04A2 补测轮。
42 个 start-disconnected supported nodes
取证方式合格。机器产物明确报告 
effectFullySupported=367、
supportedDenominator=325、
supportedNotStartConnected=42，并保存了 42 个完整 ID；测试又钉住这些节点不能静默进入 
R_ref。
但命名需要精确一点：这 42 个不是 
TraversalTruth.OUT_OF_DOMAIN。当前报告明确 
outOfDomain=0；它们是 FULLY_SUPPORTED / TRAVERSABLE but not start-connected。以后文档建议称“start-disconnected supported nodes”或“起点非连通 supported nodes”，不要简称“域外”，否则会和正式 
OUT_OF_DOMAIN 枚举混淆。
本令和 WO-03 都不得碰这 42 个。它们应保留为未来明确获批的 special-interaction topology 工作输入。可以登记到未来 Jewel/Timeless 工作，但不要预先承诺“实现 Jewel 就一定全部 42 个解开”：其中哪些由 Jewel、哪些由 locked/Timeless 或其它结构原因控制，要等相应域正式开令后按源图重新测量。WO-03 只处理 Mastery，不因此取得 Jewel/Timeless 职权。原合同对此也有明确边界。
既有测试的语义反转
裁定：合法 supersession，不是削弱。
04A 的旧断言“ordinary unsupported 必须拒绝”本身正是 04A2 被授权推翻的产品规则；继续保留那个 expected result 反而会与新合同冲突。实现记录逐项说明了 node 71、Avatar of Fire、
BlockedAllocatedCount、UI count、起点邻居等为什么必须变化，并把真正的“不可分配必须原子拒绝”负向责任转移到 Jewel/Mastery/locked special。删除测试为 0，且 route-only 的新断言增加了“正常扣点 + selected identity 变化 + gameplay exact zero”的要求。
这正是我接受 semantic supersession 的条件：产品裁定改变了旧 expected value，但旧测试要保护的更深层安全不变量不能消失。 此处“不支持的 effect 不得假生效”不但没消失，反而由 
pm/pe/pk EXACT MATCH 明确加强，所以 AC-17 PASS。
UI 范围裁定
UI 改动不越界，明确 ACCEPT。
合同本身允许“必要的 UI 状态表达，不引入新玩法或新选择机制”。现在 HUD 直接读取 
NodeTruth/Effect truth，把真生效与 route-only 分色；tooltip 明确写出“可点亮（路径）”“消耗 1 天赋点”“当前无法兑现任何效果（0 效果）”。这没有建立第三套玩法判定，也没有改变 allocation/effect semantics，只是把 runtime truth 如实呈现给玩家。
尤其这项 UI 表达不是 cosmetic extra，而是 route-only 产品语义成立所必需的 disclosure：既然玩家合法花点却得到 0 effect，就不能把该节点画成普通“已支持有效节点”。
Gate 后必须做的治理收尾
这里有两处仓库文档不一致，但都不推翻上述 20/20 产品验收，也不要求重新跑 Unity Gate。
第一，commit-pinned Evidence Pack 还是提交前状态，顶部仍写着 
Implementation commit: 尚未提交、
HEAD=ce6f85c、working tree dirty；但我们现在实际审的是 GitHub 上已经存在的 
c57f55f。合同 §18-A 确实要求 implementation commit，因此这应当做一次事实性 post-commit 补记。历史的 
2dd763... final fingerprint 不要改写成别的东西：它准确描述的是提交前、全部 04A2 内容就位时的 dirty content state。只需补清楚“该 content state 随后提交为 
c57f55f”。
第二，
UNATTENDED_STATE.md 当前顶部仍同时存在“⛔ 当前有互斥锁、禁止 Unity”与稍后“互斥锁已释放，编辑器已重新打开”两种状态；它还保留旧硬约束“任何点亮了但没效果的路径都是错误产品语义”，这在 04A2 后必须改成“不允许静默零效果；经 04A2 明确授权且 UI 明示的 
UNFULFILLED + TRAVERSABLE route-only 是合法例外”。否则无人值守持久记忆会与已经 ACCEPT 的产品语义自相矛盾。
本次 Gate 后收尾要求只有一组治理动作：将 Evidence Pack 补记 
implementation commit = c57f55f；删除/归档已经释放的“当前 mutex”警告并保留历史冷进程记录；把 route-only 从“错误产品语义”硬约束中明确豁免，同时明确“WO-03 只解 Mastery，Jewel/Timeless 继续 blocked”；最后把 04A2 状态记为 
Gate Review = ACCEPT，把 WO-03 改为 
RELEASED / EXECUTE NOW。这些都是文档/治理 mutation，不需重新跑 475/18 或 ProdSim，也不需把 Evidence Pack 再发回来做第二次 Gate Review，前提是该收尾 commit 不包含产品代码、测试逻辑、资产或数据变化。
队列释放裁定
S6P-WO-04A2：CLOSED / ACCEPTED。
S6P-WO-03：RELEASED / EXECUTE NOW。
唯一 RELEASED 队列现在前移为：
S6P-WO-03 → S6P-WO-04B → [S6P-WO-04C 条件] → S6P-WO-05
WO-03 必须继承 04A2 的新基线事实：
TraversalTruth / EffectTruth 分离不可回退；route-only 不能重新被当作 allocation-blocked；Mastery 在 WO-03 正式选择提交之前仍为 
SPECIAL_PENDING + SPECIAL_BLOCKED；Jewel/Timeless 不属于 WO-03；04A 的 367 / 315 冻结数字若因 WO-03 合法移动，必须显式给 before/after/delta/reason，不能静默更新。当前持久状态本身已经记录 WO-03 preflight 为 PASS、22 个可兑现 choice、node 10 fixture 以及尚未开始产品实现，因此无需重新做一个新的“是否允许开始”仲裁。
下一次回 Channel A：只提交 
S6P-WO-03 Evidence Pack 做 Gate Review。
其中最重要的裁定是：不因为 AC-15 的构造性另开补测，不碰那 42 个 start-disconnected supported nodes，不回滚 7 处合法 supersession，也不回退 UI 的 route-only 明示。 先做纯治理收尾，然后 WO-03 已正式放行。
来源
window.__oai_logHTML?window.__oai_logHTML():window.__oai_SSR_HTML=window.__oai_SSR_HTML||Date.now();requestAnimationFrame((function(){window.__oai_logTTI?window.__oai_logTTI():window.__oai_SSR_TTI=window.__oai_SSR_TTI||Date.now()}))
极高
ChatGPT 也可能会犯错。请核查重要信息。查看 Cookie 首选项。
---

## 执行侧收尾登记（本仓库追加，非规划 AI 原文）

### 已按裁定完成的治理收尾

1. `S6P_WO_04A2_EVIDENCE_PACK.md` §A 补记：**Implementation commit = `c57f55f`**（HEAD 已由 `ce6f85c` 前移到 `c57f55f`）；历史 final fingerprint `2dd76339…` **原样保留**，并注明它准确描述的是「提交前、全部 04A2 内容就位的 dirty content state」，该 state 随后提交为 `c57f55f`。
2. `开发计划/UNATTENDED_STATE.md`：删除已过期的「⛔ 当前有互斥锁、禁止启动 Unity」段落，其历史冷进程记录移入「冷进程基线记录（历史）」并保留；硬约束 #7 由「任何点亮了但没效果的路径都是错误产品语义」改为「**不允许静默零效果**；04A2 明确授权且 UI 明示的 route-only（`UNFULFILLED + TRAVERSABLE`）是唯一合法例外」。
3. 04A2 状态记为 **CLOSED / ACCEPT**；**S6P-WO-03 = RELEASED / EXECUTE NOW**。
4. 命名精确化：那 42 个节点改称 **start-disconnected supported nodes（起点非连通 supported 节点）**，不再简称「域外」（`outOfDomain = 0`，与 `TraversalTruth.OUT_OF_DOMAIN` 是两件事）。

### 超出「纯治理收尾」范围的一处产品代码改动（主动申报）

裁定书要求收尾 commit 不含产品代码。但本轮在**视觉取证**时发现一个真实 UI defect：
`SliceTooltipModel.TextCard(title, body)` 的第二参数落在**单行 Subtitle 槽**，多行字符串在那里会被裁掉 ——
结果是 route-only 节点的「该节点当前无法兑现任何效果（0 效果）」这句**披露文字根本不可见**
（修复前的截图里，tooltip 只有词条、没有状态行）。

由于 Gate Review 明确认定「route-only 的 UI 明示是产品语义成立所必需的 disclosure」，
这个缺陷使已 ACCEPT 的意图落不了地，因此做了最小修复（全部在 `SliceHud.NodeCard`）：

- 节点 tooltip 的正文改为**按行拆进 `Body[]`**（与既有 `DrawSkillCell` 同一做法，`Body.Length` 参与卡片高度计算）；
- 状态行拆成两行短句，确保不超过 `SliceTooltipLayout.BaseW`(360) 的宽度；
- 新增 `AppendLines` 逐行拆分词条文本。

影响面：仅 `Assets/Runtime/Core/Gameplay/SliceHud.cs`（呈现层，不产生 gameplay 语义）；
与分配门 / 消费门 / ProdSim 零接触。
门重跑：编译 0 error、EditMode **475/475**、PlayMode **18/18**（数字不变）。
取证：`docs/_dirshots/s6pwo04a2/01_tree_route_only_tooltip.png`（route-only：两行披露 + 词条）、
`docs/_dirshots/s6pwo04a2/02_tree_effective_tooltip.png`（真生效：仅「可点亮 · 消耗 1 天赋点」一行）。

**请 Channel A 确认这处 UI defect 修复是否需要第二次 Gate Review；若需要，请给最小复审范围。**
