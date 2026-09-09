# S4R_WO_02 — Backlog & Dependency Normalization（Primary Work Order）

**来源**：规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09 下发（Gate Review of WO-01 随附）。工作 AI 于本轮正式落库并执行。**Phase**: S4R Phase 1。**Runtime Authority: NONE；Implementation Authority: NONE。**

## 前置：WO-01 Gate Review 结论（规划 AI 2026-09-09）

**ACCEPT → S4R-WO-02**。12 项全部 PASS（Frozen baseline / STATUS consistency / hard stop preserved / Capability reconciliation delta=NONE / Mechanic reconciliation delta=NONE / Runtime NONE / Canonical-Content NONE / Test baseline 246+11 / Drift PASS（R-04 属授权内文档 drift，修复后=0）/ SoT conflicts 0 / Forbidden Expansion PASS / Director-gated inventory PASS）。R-04 处置被确认为正确（修 S4 已成立事实未同步 RUNTIME 的 documentation drift，不构成 scope creep）。
**非阻塞 Follow-up（并入 WO-02 任务 E）**：WO-01 Evidence 摘要「Supported（14）」与逐项列举数量不一致——WO-02 中统一 Ledger 统计口径：明确计数单位；表头总计由实际行状态得出；Evidence 给出 reconciled count；不得为凑数改变行状态。

## Objective

把目前分散在长期规划 Phase 8–12、Stage0 锁、Capability Ledger、Mechanic Matrix、Director Gate 清单及 Voice Gate 中的未来事项，整理为一个**去重、可追踪、具依赖关系且不会被误认为已批准实施的正式 Candidate Backlog**；同时建立 **Reference Build Candidate Envelope**（CANDIDATE / NON-AUTHORITATIVE / NOT LOCKED）作为以后判断 build coverage 和产品方向的输入。Open Question ① 裁定：**是，Phase 1 应建立 Reference Build 候选草案**，但不得在本单锁定正式 Targets，更不得为实现候选去补机制。

## In Scope

- **A. 统一 Candidate Backlog**：来源至少覆盖：长期规划 Phase 8–12；Stage0/当前锁定清单；S4R Director-Gated Inputs；Ledger 中 Partial/Unsupported/Blocked；Matrix 未支持 mechanics；Voice GATED；已知技术/质量债务；已支持能力的合理 breadth/depth 后续项。每项唯一 ID（`S4R-BL-001` 起）。
- **B. 字段**：ID / Name / Origin / Capability family / Current implementation state / Primary classification / Player-facing value category / Required capability delta / Required mechanic delta / Canonical-data dependency / Prerequisites / Downstream dependents / Art dependency / Audio dependency / Test-evidence burden / Performance risk / Determinism risk / Save-migration risk / Runtime blast radius / 是否改变 player-facing progression / Director approval requirement / Current authorization state / Reference-build relevance / Notes。
- **Primary Classification（唯一）**：`ALREADY_SUPPORTED / BREADTH_EXTENSION / DEPTH_EXTENSION / CONTENT_ONLY / TECH_QUALITY / DIRECTOR_GATED / FORBIDDEN_UNTIL_APPROVED / DEPENDENCY_BLOCKED`；flag 不得绕过 Primary；`DIRECTOR_GATED / FORBIDDEN_UNTIL_APPROVED / DEPENDENCY_BLOCKED` 不得同时被描述为 READY/ACTIVE/APPROVED/NEXT IMPLEMENTATION。
- **C. Dependency Normalization**：候选依赖图，回答：哪些只复用 S4 capability；哪些要求新增 capability；哪些要求 canonical pipeline 扩展；哪些必须在 progression 前；哪些属 endgame prerequisite；哪些属内容工厂前置；哪些互不依赖只是产品选择。**保留长期规划依赖语义 Phase 8 → Phase 9/10 → Phase 11 → Phase 12；不得因 Phase 11/12 价值高而误分类为当前可执行。**
- **D. Reference Build Candidate Envelope**：documentation-only；≥覆盖全部 3 个 canonical Skill（建议一 Skill 一候选）；只用 S4 当前 Supported mechanics；Partial 机制只可作 coverage observation；Unsupported/Director-gated 不得进入必需组成。每候选写：Candidate Build ID / Core skill / Intended mechanical identity / Support combination / Passive-mechanic families exercised / Equipment-affix families exercised / Damage-defense mechanics exercised / Expected coverage purpose / Currently unsupported mechanics required=必须 NONE / Status=CANDIDATE NOT LOCKED。不要求 balance/DPS/通关 target。
- **E. Ledger Count Reconciliation**：固定计数单位；四状态总数与实际行一致；不改行语义凑数；记录 count-rule。

## Out of Scope

改 gameplay runtime；新增 canonical data；实现候选所缺机制；balance tuning；锁定正式 Reference Build Targets；给 backlog 做最终产品优先级承诺；宣布下一周期即 Phase 8/9/10/11/12 任何一项；创建新 progression/endgame implementation plan；PoEDB 全量导入；开发 Craft Simulator；开发 Map Risk Simulator；开启新 Content Batch。

## Forbidden Expansion

完整继承硬停止清单（S5/Progression Spine/Map Tier/Boss/Unique/Ring/Offhand/Amulet/New Content Batch/Voice activation/Curse/Flask/Jewel/Atlas/New Class/Deep Craft/Unapproved Endgame）以及：Aura/Reservation implementation、Mastery implementation、Ascendancy implementation、Persistence/save system、Weapon/Handedness/Requirement system、new Skill/Support、new Affix family、new EquipSlot、new progression currency、advanced Trigger implementation、**PoEDB bulk ingestion pipeline implementation**。**注意：这些内容允许出现在 backlog 中——「记录候选项」不是「授权实施」。**

## Acceptance Criteria

- **AC-01** Candidate Pool Complete：上述 10 类来源全部纳入或明确标记「为什么不构成 backlog item」；不得静默遗漏。
- **AC-02** Unique & Deduplicated：语义相同只有一个 authoritative ID（如 Unique 同时出现在长期规划与 Director Gate → 一个 ID）。
- **AC-03** Authorization Is Explicit：每项 AUTHORIZED/NOT AUTHORIZED/DEFERRED/BLOCKED；本单结束 **New gameplay authorizations = 0**。
- **AC-04** Classification Integrity：唯一 Primary；不存在 Forbidden+Ready / Gated+Active / Blocked+Executable 矛盾态。
- **AC-05** Dependency Graph Complete：要求新增 capability 的项至少有 immediate prerequisite / blocking prerequisite / downstream dependency（如存在）；长期规划顺序≠已批准执行顺序。
- **AC-06** Risk Coverage：非 ALREADY_SUPPORTED 项至少评估 performance/determinism/save-migration/test burden/canonical-data dependency/runtime blast radius（可 N/A，不可空缺）。
- **AC-07** Reference Build Candidates Established：覆盖全部 3 canonical Skill；仅依赖 Supported mechanics；unsupported requirement=NONE；标记 CANDIDATE/NOT LOCKED；不形成正式产品 gate。
- **AC-08** Reference Build Coverage Remains Non-Authoritative：正式 Coverage 不得从 N/A 晋级 PASS/SUPPORTED；可增 Candidate Coverage Mapping，但与正式 Locked Targets 分开。
- **AC-09** Ledger Counts Reconciled：统计口径写明；summary 与 rows 一致；无凑数 promotion。
- **AC-10** Zero Runtime/Product Delta：Capability/Mechanic/Runtime/Canonical/Content/New Authorization Delta 全 NONE。
- **AC-11** Forbidden Expansion Audit PASS：全部硬停止继续有效。
- **AC-12** Director Decision Inputs Can Be Derived：仅凭 backlog/dependency 文档即可回答：哪些候选技术上最近；哪些须先新增 capability；哪些属真正 endgame；哪些 breadth/depth；哪些仍被 Director Gate 阻塞。**但本单不得替导演选择方向。**

## Markdown Sync Checklist

DECISIONS（UPDATE REQUIRED：Backlog 分类语义；Candidate≠Approved；Reference Build Candidate≠Locked Target；dependency order≠product commitment；不得登记新 gameplay 产品决定）；RUNTIME（NO CHANGE EXPECTED，仅 verify）；COMBAT_MATH（NO CHANGE EXPECTED，候选 Build 不得改公式）；`S4R_CAPABILITY_LEDGER.md`（UPDATE REQUIRED：count reconciliation + backlog links + classification/reference metadata；Capability states 原则不变）；`S4R_MECHANIC_MATRIX.md`（UPDATE REQUIRED：Backlog ID linkage + Candidate Reference Build coverage mapping；正式 Coverage 保持未锁定）；STATUS（LIMITED：WO-02 Evidence 引用 + S4R tracking；禁止把 Phase 1 标 COMPLETE/清 Director Gate/启动 S5）；ROADMAP（UPDATE REQUIRED：链接 authoritative normalized backlog；可表达 dependency families；不得把 Phase 8–12 顺序写成 committed roadmap）。

## 建议产出

`docs/reviews/S4R/S4R_BACKLOG_NORMALIZATION.md`（依赖图可并入，不强制拆分）、`docs/reviews/S4R/S4R_REFERENCE_BUILD_CANDIDATES.md`、`docs/reviews/S4R/S4R_WO_02_EVIDENCE.md`。

## Non-Blocking Tasks

主路径某候选来源/依赖无法确认时：Backlog Source Index（source→IDs 映射）；Capability Dependency Vocabulary（prereq/blocker/downstream/product-choice 定义统一）；Reference Build Candidate Draft（只 Supported）；PoEDB Dependency Audit（只标记，不开发）；Long-Term Phase Traceability（Phase 8–12↔backlog IDs 双向 trace）。需原计划原文才能判断的标 `UNRESOLVED-SOURCE`，不猜不实现。

## Evidence Pack 回传模板

Work Order / Revision-Commit / Changed Markdown / Changed Runtime Files / Backlog Total / By Primary Classification / By Authorization State / Sources Covered / Deduplicated Items / Unresolved Source Items / Dependency Edges / Dependency Cycles Found / Reference Build Candidates / Canonical Skills Covered / Unsupported Mechanics Required By Candidates / Capability Ledger Count Rule（Supported/Partial/Unsupported/Blocked 分计数）/ 六项 Delta 全 NONE / Drift Found / SoT Conflicts / Forbidden Expansion Audit / Director-Gated Items / Open Questions / Recommended Next WO。另附三个小摘要：Backlog 分类表（ID/名称/Primary/Authorization/主要 blocker）；Dependency roots；Reference Build candidates（每候选一行覆盖目的）。

## 预告

WO-02 通过后，预计下一主单 = **S4R-WO-03 — Next-Direction Decision Packet**（把标准化 backlog 压缩成少数可供导演明确选择的产品方向，仍不实现任何方向）。
