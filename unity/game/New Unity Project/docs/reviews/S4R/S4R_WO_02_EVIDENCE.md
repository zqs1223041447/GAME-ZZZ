# S4R_WO_02_EVIDENCE — Evidence Pack（S4R-WO-02 Backlog & Dependency Normalization）

**Work Order**: S4R-WO-02 — Backlog & Dependency Normalization（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 WO-01 Gate Review=ACCEPT 下发）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ da3db41

## Changed Markdown（10 个文件）

| 文件 | 变更类型 |
|---|---|
| `docs/reviews/S4R/S4R_WO_02.md` | 新建：工作令原文落库 + WO-01 Gate Review=ACCEPT 结论与 follow-up 登记 |
| `docs/reviews/S4R/S4R_BACKLOG_NORMALIZATION.md` | 新建：候选池 32 项（去重/唯一主分类/授权显式/依赖图/风险轴/来源覆盖核对/导演输入派生表） |
| `docs/reviews/S4R/S4R_REFERENCE_BUILD_CANDIDATES.md` | 新建：Reference Build 候选信封（RC-M/RC-P/RC-A；对 golden oracle 合法组合自查） |
| `docs/reviews/S4R/S4R_CAPABILITY_LEDGER.md` | 更新：Count Rule 固化（单位=总览行；18/4/5/1=28）+ WO-01「14」笔误对账声明 + Backlog 链接节 |
| `docs/reviews/S4R/S4R_MECHANIC_MATRIX.md` | 更新：新增 §6 Backlog ID Linkage + §7 非权威 Candidate Reference Build Coverage Mapping |
| `docs/DECISIONS.md` | 追加 1 条：S4R-WO-02 Backlog 分类语义（Candidate≠Approved / RBC≠Locked Target / 依赖顺序≠产品承诺 / 新授权=0） |
| `docs/ROADMAP.md` | S4R 行更新：WO-01=ACCEPT + WO-02 交付与链接（候选≠承诺） |
| `docs/reviews/STATUS.md` | LIMITED：本轮 commit 行 + 门状态 S4R 行更新 + 最近一次绿灯加行 |
| `docs/RUNTIME.md` | **verify only — verified; no change required**（本单零改动；WO-01 轮已对齐，本单无新 drift） |
| `docs/COMBAT_MATH.md` | **verify only — verified; no change required**（候选 Build 未触碰任何公式） |

## Changed Runtime Files

**NONE**（零 .cs/场景/预制体/资产/ContentData diff）

## Backlog 统计（按回传模板）

- **Backlog Total: 32**
- **By Primary Classification**: ALREADY_SUPPORTED=0（已支持能力本身不入池，原因已在 §0/§4 声明）；BREADTH_EXTENSION=3（BL-001/002/023）；DEPTH_EXTENSION=9（BL-003/004/007/008/009/012/013/021/022）；CONTENT_ONLY=1（BL-018）；TECH_QUALITY=4（BL-014/017/030/032）；DIRECTOR_GATED=13（BL-005/006/010/011/015/016/019/020/025/027/028/029/031）；FORBIDDEN_UNTIL_APPROVED=1（BL-024）；DEPENDENCY_BLOCKED=1（BL-026）
- **By Authorization State**: AUTHORIZED=0（**New gameplay authorizations = 0**）；NOT_AUTHORIZED=18；DEFERRED=13；BLOCKED=1
- **Sources Covered**: 10 类来源全纳入（长期规划 Phase 8-12 / Stage0 锁 / Director-Gated Inputs / Ledger 未支持行 / Matrix 未支持机制 / Voice GATED / 技术·质量债 / 已支持能力后续）——逐项映射见 Backlog §4；不构成 item 的例外亦显式声明（Harness 代表性=已被规则⑭关闭；狼/蝙蝠=已关闭裁定）
- **Deduplicated Items**: 5 处语义合并（大天赋树→BL-003；Unique library→BL-010；deep Craft→BL-013；Jewel Socket 保留语义→BL-011；Mastery/Ascendancy→BL-003 族内不另立 ID；Boss/Special Encounter/Monster Package→BL-016）
- **Unresolved Source Items: 0**（无 UNRESOLVED-SOURCE；全部来源可从仓库文档定位）

## Dependency / Reference Build

- **Dependency Edges**: 长期规划链 Phase 8（BL-001→…→BL-011）+ Phase 9/10（BL-012→BL-013→BL-014）+ Phase 11（BL-015/016/017/019）+ Phase 12（BL-018 ←核心机制稳定）+ 基建前置（**BL-024 → BL-003/BL-023 blocking**）+ 独立项（BL-021⊥BL-022 等产品选择关系）——每处均标注「非已批准执行顺序」
- **Dependency Cycles Found: 0**（DAG；BL-013↔BL-014 为语义前置非循环）
- **Reference Build Candidates: 3**（S4R-RBC-M 烈刃转火 / S4R-RBC-P 分裂弹幕 / S4R-RBC-A 灰烬领域）
- **Canonical Skills Covered: 3/3**（Melee/Projectile/Area 各一候选）
- **Unsupported Mechanics Required By Candidates: 全部 NONE**（Support 组合逐一对 `SupportCompatGolden` 合法：RC-M=FireConversion+Brutal；RC-P=Fork+Faster；RC-A=Concentrated+Combustion，Area 2S 取一以 canonical 为准——两支持均合法）

## Capability Ledger Count Rule（AC-09）

- 计数单位=Ledger 状态总览行（capability/domain 行）；**Supported=18 / Partial=4 / Unsupported=5 / Blocked=1 / 合计 28**；WO-01 摘要「Supported（14）」=统计笔误（逐项实为 18），**不改变任何行状态、无凑数 promotion**；规则与总数已固化进 Ledger 文件。

## Delta 声明（AC-10，逐项）

Capability Delta: **NONE**；Mechanic Support Delta: **NONE**；Runtime Delta: **NONE**；Canonical Data Delta: **NONE**；Content Delta: **NONE**；**New Authorization Delta: NONE**（Backlog/RBC 全部候选与依赖关系均显式标注「非授权/非承诺」）。

## Tests / Gates

- **Quick Gate 复跑（本单当轮，exit 0=PASS）：EditMode 246/246、PlayMode 11/11、Content Audit fresh PASS failures=0**；Performance Gate 不重跑（无 runtime/performance drift，引用 S4 Final 证据）。

## Drift Found

**0**（本单无新增文档漂移；RUNTIME/COMBAT_MATH verify 均为 no change required）

## Source-of-Truth Conflicts

**0**

## Forbidden Expansion Audit

**PASS** — 硬停止全清单原样；PoEDB bulk pipeline=FORBIDDEN_UNTIL_APPROVED（BL-024 显式分类）；Aura/Mastery/Ascendancy/Persistence/Weapon-Handedness/新 Skill-Support/Affix family/EquipSlot/currency/advanced Trigger 全部仅以候选形式存在于 Backlog，未实施；候选≠授权（DECISIONS 登记）。

## Director-Gated Items

13 项 DEFERRED（BL-005/006/010/011/015/016/019/020/025/027/028/029/031）+ 1 项 BLOCKED（BL-026 Voice，Resume=导演试听指认）+ 1 项 FORBIDDEN_UNTIL_APPROVED（BL-024）。AC-12 派生表已就绪（Backlog §3）：导演可不读 runtime 代码直接得到「技术最近/需新 capability/真 endgame/breadth/depth/Gate 阻塞」五类答案。

## Open Questions（供规划 AI）

1. BL-024（PoEDB Pipeline）分类为 FORBIDDEN_UNTIL_APPROVED——若下一正式周期（导演批准方向后）需要它作为 blocking prerequisite，是否届时由规划 AI 以新周期工作令显式解禁？
2. RBC 三候选的 Support 细节取舍（如 RC-A 的 Concentrated vs Combustion）留待正式 Targets 锁定令裁决——是否同意保持 NOT LOCKED 进入 WO-03 决策包？

## Recommended Next WO

按计划推进 **S4R-WO-03 — Next-Direction Decision Packet**（把 32 项标准化 backlog 压缩为少数可供导演明确选择的候选方向；不实现任何方向）。

## Self-Review 对照 AC-01..AC-12

AC-01 ✓（§4 逐源核对+例外声明）；AC-02 ✓（去重 5 处+唯一 authoritative ID）；AC-03 ✓（AUTHORIZED=0）；AC-04 ✓（§5 矛盾态核查）；AC-05 ✓（依赖图+顺序非承诺标注）；AC-06 ✓（32 项风险轴无空缺，N-A 显式）；AC-07 ✓（3/3 Skill、golden 合法、NONE unsupported、NOT LOCKED）；AC-08 ✓（正式 Coverage 维持 N/A，候选映射分区）；AC-09 ✓（Count Rule 固化）；AC-10 ✓（六 delta=0）；AC-11 ✓（Forbidden Audit PASS）；AC-12 ✓（派生表，不替导演选择）。
