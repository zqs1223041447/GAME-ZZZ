# S4R_PLAN — GAME-ZZZ S4R：Direction Readiness & Baseline Lock

**来源**：规划 AI（ChatGPT 镜像站，会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09）下发的 S4R 周期计划全文。工作 AI 在工作令 **S4R-WO-01** 中将其正式落库为 Repository-as-Memory，不得重新解释为别的周期。
**基线**：main @ b3eb9fe（S4 = COMPLETE；硬停止延续）。
**周期类型**：Inter-cycle / 治理与产品方向准备。**Runtime Capability Expansion: NOT AUTHORIZED。**

> **Milestone 状态回填**：本文件落库时 Phase 0（Baseline Reconciliation）由 S4R-WO-01 执行中；正式阶段状态字段仍按规则只在 S4R Final Director Gate 后统一更新。

---

## 0. 关键治理判断

S4R 不是 S5，也不授权任何新的 gameplay capability。它是 S4 与下一正式产品周期之间的受控过渡周期。理由：当前 STATUS 硬停止要求等待导演下一产品方向，规划 AI 不能用「自动规划下一周期」的权限绕过这条显式 Director Gate。

## 1. Cycle Mission

S4 已证明当前生产基线可以在锁定硬件、正式视觉、正式物品广度以及 Production Simulation 条件下闭合。S4R 的任务不是继续堆叠系统或内容，而是：

1. 锁定并可追溯地描述 S4 最终产品/技术基线。
2. 重新核对 Capability Ledger、Mechanic Support Matrix、Backlog、ROADMAP 与运行时事实，确保不存在「文档声称支持但运行时不支持」或反向 drift。
3. 将下一产品周期的候选方向转换为导演可以明确批准/拒绝的决策包。
4. 在导演未授权前保持 runtime feature surface 不变。
5. 导演方向确定后，产出下一正式实施周期的 planning seed；不得在 S4R 内提前实现该方向。

## 2. Entry Conditions（全部满足，冻结为 entry baseline；S4R 不重新解释已 CLOSED 的 gate）

- S4 = COMPLETE；S4 Phase 0–5 全 COMPLETE。
- Final `-IncludeArtPerformance` Gate 双 PASS：canonical worst avg=1.903ms（22.8%）/ worst p99=4.840ms（58.1%）；Art worst avg=4.085ms（49.0%）/ worst p99=6.671ms（80.1%）；Art visuals=resolvedVisuals 4 / fallback 0 / clones 0。
- Production Simulation = exact deterministic hash（FNV1A64:a1f075f251ec1070）。
- EditMode = 246/246；PlayMode = 11/11。
- S0–S2P gates CLOSED；S3 COMPLETE；Voice remains DEFERRED BY DIRECTOR。
- 1440p/120 locked-hardware performance gate CLOSED。
- Repository-reported Drift = 0。

## 3. Frozen Product Baseline

| Domain | Frozen S4 Baseline |
|---|---|
| Equip Slots | 6（新增 Gloves / Belt） |
| Affixes | 17（4 条 Gloves/Belt 专属） |
| Applicability | Single applicability truth |
| Canonical Skills | 3 |
| Canonical Supports | 7 |
| Passive Graph | 16 nodes / 20 edges / 0 disconnected |
| Player Visual | DarkKnight |
| Production Enemy Visuals | Troll / FireLion / Gargoyle / Bruce |
| Combat SFX | 5 keys deployed |
| Voice | 3 keys, GATED |
| Production Simulation | deterministic exact hash |
| Runtime Drift | none reported |

这张表是 baseline assertion，不是本周期新增 capability。扩展快照（含证据指针）见 `S4R_FROZEN_BASELINE.md`。

## 4. S4R Non-Goals

S4R 明确不承担：新玩法系统开发；新 progression loop；新 endgame loop；新装备类别；新职业；新 canonical skill/support 内容；新敌人正式 Content Batch；新 Boss；Unique item 系统或 Unique 内容；Voice release；深度 Craft；大规模 runtime 架构重写；为「下一周期可能需要」而提前埋入未经批准的 gameplay capability。

允许修复的唯一代码类事项：已有 S0–S4 capability 的明确 regression / documentation-vs-runtime defect。这类修复如出现，也需单独 Work Order，不得顺手扩 scope。

## 5. Forbidden Expansion（S4R 内全部 FORBIDDEN / NOT AUTHORIZED）

S5 gameplay implementation；Progression Spine；Map Tier；Boss implementation/content；Unique；Ring；Offhand；Amulet；New Content Batch；Voice activation/release；Curse；Flask；Jewel；Atlas；New Class；Deep Craft；Any unapproved Endgame；未经导演明确授权的新 EquipSlot；未经导演明确授权的新 Skill / Support；未经导演明确授权的新 Affix family；未经导演明确授权的新 progression currency/resource；「为了以后用」而加入的 dormant gameplay system；对 PoEDB canonical data 的选择性替代来源。**PoEDB 继续是唯一 canonical external data source。**

## 6. Phase Structure

### Phase 0 — Baseline Reconciliation
目标：将 S4 最终事实与治理文档重新对齐。输出：Frozen S4 Baseline Snapshot、Capability Ledger reconciliation、Mechanic Support Matrix reconciliation、Source-of-Truth map、Drift report、Hard-stop registry。
Exit Criteria：所有已实现 capability 都有明确 ledger 状态；已支持/部分支持/不支持状态与 runtime evidence 一致；无未经解释的 UNKNOWN；无隐性 capability expansion；Drift=0 或所有 drift 被显式列成 defect 不得静默改口径。

### Phase 1 — Backlog & Dependency Normalization
目标：把 Backlog/长期规划候选事项按治理规则重新分类，不实现。分类轴：Already Supported / Breadth Extension / Depth Extension / Content-only / Tech-Quality / Director-Gated / Forbidden Until Explicit Approval / Dependency Blocked。每项记录：capability 依赖、mechanic 依赖、canonical-data 依赖、art/audio 依赖、测试影响、性能风险、确定性风险、migration/save-data 风险（如适用）、blast radius、是否改变 player-facing progression。
Exit Criteria：不存在一个 backlog item 同时处于「可直接执行」与「Director-Gated/Forbidden」。

### Phase 2 — Next-Direction Decision Packet
目标：形成导演决策材料，不实现候选方向。每个候选方向至少给出：Player value、为什么现在做、Existing capability reuse、Required new capability、Explicit exclusions、Major dependency chain、Performance implications、Test/evidence burden、Expected cycle boundary、Principal risks、Definition of Done、What becomes possible afterward。
禁止为证明候选方案而提前实现 prototype gameplay system；允许 documentation-only analysis。

### Phase 3 — Director Direction Gate（S4R 核心产品门）
导演必须明确选择：下一正式产品目标；明确允许新增的 capability；明确继续 defer 的 capability；内容广度预算；Voice 是否继续 deferred；是否开放 progression/endgame 类 capability；下一周期的主要成功指标。
导演没有作出选择时：S4R 可以继续执行治理、证据和规划类 Non-Blocking Work，但 runtime feature surface 保持 frozen。

### Phase 4 — Implementation-Cycle Seed
导演批准产品方向后，仅产出下一正式周期的规划 seed：proposed cycle ID、objective、capability delta、mechanic delta、dependency graph、risk register、preliminary phase decomposition、preliminary gate design、proposed first implementation Work Order。Phase 4 本身不执行该 Work Order；下一正式周期由规划 AI 根据 Director Gate 结果单独下发。

## 7. S4R Final Gate

同时满足才关闭：Frozen S4 baseline fully reconciled；Capability Ledger 无未解释 drift；Mechanic Support Matrix 无未解释 drift；Source-of-Truth hierarchy 明确；Forbidden Expansion registry 明确；Backlog 已重新分类；Director 已给出下一产品方向；下一 implementation cycle planning seed 已生成；S4R 期间没有未经授权的 runtime capability expansion。**Director 对 S4R Final Gate 作最终批准；只有此时才把 S4R 正式阶段状态统一更新为 COMPLETE。**

## 8. Source-of-Truth Order

延续现有治理规则；文档冲突按既有 QA 定义执行，不由 Implementer 临时决定。特别规则继续有效：Canonical external data → PoEDB only；Runtime implementation claims → 必须有 runtime/test evidence；Gate completion → Evidence Pack + Director Gate；STATUS → formal phase state authority；Planning documents 不得自行覆盖已经 CLOSED 的 gate 事实。若既有 QA 文件规定了更精确的优先级，以 QA 原文为准。

## 9. Runtime Change Policy

S4R 默认 Runtime Change Budget = 0 gameplay capability changes。允许的改动：Markdown/governance/documentation；测试运行及 evidence generation；已批准测试工具的非行为性维护；经后续独立 Work Order 批准的 S0–S4 regression fix。
禁止利用「清理/refactor/为未来准备」等名义引入：新接口语义；新 gameplay 状态；新存档字段；新掉落逻辑；新装备逻辑；新 progression hooks；inactive-but-functional future systems。

## 10. Test Policy

Phase 0 baseline reconciliation 至少重新确认 EditMode baseline 与 PlayMode baseline，预期仍为 EditMode 246/246、PlayMode 11/11。若测试计数改变（即使全 PASS）必须解释：哪些测试增删、对应哪个已批准 Work Order、是否意味着 capability surface 改变。
S4 Final Performance Gate 默认不要求重跑；只有 S4R 发生影响 runtime/performance 的批准修复、原 Evidence 无法复现、或已记录性能 drift 时才考虑重跑；否则引用 S4 最终 canonical / Art evidence。

## 11. Evidence-Pack Standard for S4R

每张 Work Order 回传至少包括：Work Order ID、commit/revision identifier、changed-file list、diff summary、tests executed、exact pass/fail counts、capability delta、mechanic-support delta、runtime delta、content delta、newly discovered drift、unresolved questions、source-of-truth conflicts、Forbidden Expansion check、required follow-up、candidate STATUS/ROADMAP patch（若涉及）。delta 为零时明确写 NONE，不省略。

## 12. Director-Gated Inputs（当前需要导演输入；未产生前不授权 S5 或任何等价实施周期）

1. 下一周期的首要玩家价值？（build depth / content breadth / progression / encounter depth / polish 等方向分类，不代表已批准任何系统）
2. 是否授权进入 Progression 类能力？（当前继续视为 NO / NOT AUTHORIZED，直到导演明确改变）
3. 是否授权任何 Endgame 能力？（含 Map Tier / Atlas 类方向，默认 NO）
4. 是否授权 Boss 方向？（默认 NO）
5. 是否开放新的装备 slot？（Ring / Amulet / Offhand 当前全部 NO）
6. 是否开放 Unique？（默认 NO）
7. 是否允许下一 Content Batch？（默认 NO）
8. Voice 的 DEFERRED 状态是否改变？（默认保持 deferred）
9. 下一周期更偏 breadth 还是 depth？
10. 下一周期的关键 gate 优先优化什么？（player-facing completion / deterministic correctness / build diversity / content volume / performance margin / production polish）
11. 是否存在新的导演级禁区或产品约束？

---

# 13. 第一个 Primary Work Order

## S4R-WO-01 — Frozen Baseline & Governance Reconciliation

**Objective**：建立一个能够由规划 AI、工作 AI 和 Director 共同引用的 S4 Frozen Baseline + Governance Reconciliation Pack，确认：S4 最终事实；当前 capability surface；当前 mechanic support surface；current hard stops；source-of-truth linkage；当前 drift 是否确实为 0。本单不得增加或改变玩家可见 gameplay capability。

**In Scope**：核对 `docs/reviews/STATUS.md`、`开发计划/SHORT_TERM_PLAN_BDA_v1.md`、`开发计划/GAME-ZZZ_short_term_planning_QA.md`、当前 ROADMAP、DECISIONS、RUNTIME、COMBAT_MATH、Capability Ledger、Mechanic Support Matrix；建立 S4 Frozen Baseline Snapshot；对齐当前事实（6 EquipSlots / 17 Affixes / single applicability truth / 3 Skills / 7 Supports / Passive 16-20-0 / DarkKnight / 4 production enemy visuals / combat SFX 5 / Voice 3 GATED / S4 perf gate values / Production Simulation hash / EditMode-PlayMode baseline / all gate states）；建立 Hard Stop / Forbidden Expansion 清单；检查治理文档对 S4 状态的冲突描述；Ledger/Matrix reconciliation；建立 Source-of-Truth Index（claim → authoritative source → evidence source → current state → mismatch status）；发现 drift 时记录、分类、给出建议后续 Work Order，不得在本单顺手修 gameplay。

**Out of Scope**：gameplay code changes；content implementation；art replacement；audio expansion；canonical data expansion；balance pass；progression work；endgame work；new item types；new affixes；new skills/supports；new enemies；new Boss；Voice enablement；large refactor。

**Forbidden Expansion**：S5 / Progression Spine / Map Tier / Boss / Unique / Ring / Offhand / Amulet / New Content Batch / Voice / Curse / Flask / Jewel / Atlas / New Class / Deep Craft / Unapproved Endgame 及任何语义等价的隐性实现。

**Acceptance Criteria**（全部满足才可提交 Evidence Pack）：
1. S4 Baseline Snapshot 完整：所有冻结事实均有对应 authoritative/evidence reference。
2. STATUS consistency：S4 仍明确 COMPLETE；S3 Voice 仍明确 DEFERRED BY DIRECTOR；S4 hard stop 未被删除、弱化或解释为已授权 S5。
3. Capability Ledger reconciled：每项当前实现 capability 都有明确状态；无未解释 UNKNOWN；无 unsupported 被误标 supported；本单 capability delta = NONE。
4. Mechanic Support Matrix reconciled：支持状态与实际 runtime/test evidence 一致；无未经批准的 mechanic 晋级；本单 mechanic-support delta = NONE。
5. Runtime delta = NONE：不修改 gameplay runtime source/prefab/data behavior；必须修复的 defect 只登记不在本单实施。
6. Canonical-data delta = NONE：不新增 PoEDB canonical entries；不引入第二 canonical data source。
7. Drift result 明确：报告 Drift: NONE 或逐条登记 mismatch、severity、source conflict 和建议 WO。
8. Test baseline confirmed：EditMode 246/246、PlayMode 11/11；计数不同须逐项解释；不要求重跑 S4 performance gate（除非发现 runtime/performance drift）。
9. Forbidden Expansion audit = PASS。
10. Director-gated decision list 已落库：所有未批准方向保持 NOT AUTHORIZED / DEFERRED，不得标为 planned implementation。

**Markdown Sync Checklist**：DECISIONS（UPDATE REQUIRED：S4R 是 inter-cycle、Runtime capability expansion 未授权、S4 hard-stop 延续、Director Direction Gate 是下一正式实施周期前置；不得把候选方向写成已批准决策）；RUNTIME（VERIFY; UPDATE ONLY IF BASELINE DOCUMENTATION IS STALE——不得借本单修改 runtime 行为；一致则写 verified, no change required）；COMBAT_MATH（VERIFY; NORMALLY NO CHANGE——本单不授权公式/系数/balance 改动；发现 drift 登记不自行重定义）；Capability Ledger（UPDATE REQUIRED：reconciliation、frozen S4 state、evidence linkage、director-gated/not-authorized 区分；不新增实际 capability）；Mechanic Support Matrix（UPDATE REQUIRED：reconciliation、evidence linkage、unsupported/forbidden 状态清晰化；不做 mechanic promotion）；STATUS（LIMITED UPDATE：保留 S4=COMPLETE 与硬停止语义；允许加入 S4R planning/reference note；不把 S4R 内各 Phase 提前标 COMPLETE；正式 phase 状态仍等待最终 Director Gate）；ROADMAP（UPDATE REQUIRED：加入 S4R=inter-cycle / direction readiness；next implementation cycle=Director-Gated；downstream forbidden items 仍未授权；不得把候选项的「存在」写成「承诺实施」）。

**Primary WO Evidence Pack 回传要求**：Work Order / Revision-Commit / Changed Markdown / Changed Runtime Files / EditMode / PlayMode / Capability Delta / Mechanic Support Delta / Runtime Delta / Canonical Data Delta / Content Delta / Frozen Baseline Reconciled / STATUS Consistent / ROADMAP Consistent / Drift Found / Source-of-Truth Conflicts / Forbidden Expansion Audit / Director-Gated Items Confirmed / Open Questions / Recommended Next WO。另附：git diff --stat 摘要；所有修改 Markdown 的文件名；Capability Ledger reconciliation 新增/变更行摘要；Mechanic Matrix reconciliation 新增/变更行摘要；若存在 drift，把原文冲突双方各贴足够判断的短摘录。不需要贴整个文件全文。

**Non-Blocking Tasks for S4R-WO-01**（主路径因治理文档缺失、格式不清或 ledger/matrix schema 无法判断而阻塞时可执行）：S4 Evidence Index（只整理现有 S4 Phase 0–5 / Final Gate evidence 路径、结论、hash、测试结果，不重跑性能测试）；Source-of-Truth Index；Hard-Stop Audit（搜索规划文档中与当前 hard-stop 相冲突的 active/planned 表述；只报告不实现）；Backlog Classification Draft（仅分类）；Evidence Hygiene（检查 S4 关键 evidence 的孤儿引用、旧路径或重复 source-of-truth）。这些工作不能转化为 gameplay implementation。

## S4R-NB-01 — S4 Evidence & Source-of-Truth Index（等待导演期间的第一个 Non-Blocking Work Order）

Classification: NON-BLOCKING；Runtime Authority: NONE。
Objective：把已完成的 S4 证据整理成稳定索引，使后续规划不需要重新解释 S4 gate。
In Scope：仅建立 Markdown evidence index（phase / gate / evidence artifact-path / decisive metric / pass result / governing source / superseded evidence（若有）/ final authoritative evidence）；重点覆盖 canonical performance、art performance、visual-resolution result、Production Simulation hash、EditMode、PlayMode、gate closure、Voice defer、hard-stop。
Acceptance：不修改 runtime；不修改 canonical data；不增加 capability；不更改 S4 gate result；所有 S4 Final Gate 核心结论均能从索引定位到 authoritative evidence；不存在两个同时被标成 canonical 的相互冲突 evidence；Forbidden Expansion Audit=PASS。
Evidence 回传：新增/更新文件；索引条目数；orphaned evidence count；conflicting evidence count；superseded evidence count；runtime diff 必须 NONE。

---

# 14. 下一步同步协议

完成 S4R-WO-01 后把 Evidence Pack 摘要发回规划 AI 会话（`6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`），规划 AI 据此做 Gate Review：ACCEPT → S4R-WO-02 / ACCEPT WITH FOLLOW-UP / REJECT-REWORK。在导演产品方向尚未到位之前，后续工作令只停留在 baseline、ledger/matrix、backlog、decision packet、evidence 范围内，不越过当前硬停止。
为把 Phase 1/2 做得更精确，WO-01 之后下一次回传时一并附：长期规划中 S4 之后的章节摘录、当前 Capability Ledger、当前 Mechanic Support Matrix/Backlog。
