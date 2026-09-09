# S4R_WO_01_EVIDENCE — Evidence Pack（S4R-WO-01 Frozen Baseline & Governance Reconciliation）

**Work Order**: S4R-WO-01 — Frozen Baseline & Governance Reconciliation（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 下发）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交（同一行登记）；基线 main @ 64eb9fe

## Changed Markdown（全部 9 个文件）

| 文件 | 变更类型 |
|---|---|
| `docs/reviews/S4R/S4R_PLAN.md` | 新建：规划 AI S4R 周期计划正文落库（含 WO-01/NB 令与同步协议） |
| `docs/reviews/S4R/S4R_FROZEN_BASELINE.md` | 新建：S4 冻结基线快照（18 项 claim→authoritative→evidence）+ Hard-Stop Registry + Forbidden Expansion Registry + 已知限制 |
| `docs/reviews/S4R/S4R_CAPABILITY_LEDGER.md` | 新建：Capability Ledger 首次正式建立（28 能力项，状态+证据锚点+门控标注） |
| `docs/reviews/S4R/S4R_MECHANIC_MATRIX.md` | 新建：Mechanic Support Matrix 首次正式建立（Passive/Support/装备-Craft/战斗-地图-表现-基础设施 4 族） |
| `docs/reviews/S4R/S4R_SOURCE_OF_TRUTH_INDEX.md` | 新建：SoT Index（18 项 claim→权威源→证据源→现状→mismatch；含裁决适用例） |
| `docs/reviews/S4R/S4R_WO_01_EVIDENCE.md` | 新建：本 Evidence Pack |
| `docs/RUNTIME.md` | 修订（2 处，见 Drift R-04） |
| `docs/DECISIONS.md` | 追加 1 条：S4R 周期定位（inter-cycle/未授权扩张/硬停止延续/Direction Gate 前置/候选方向不得写成已批准） |
| `docs/ROADMAP.md` | 页首当前阶段摘要加 S4R + 阶段表加 S4R 行（IN PROGRESS） |
| `docs/reviews/STATUS.md` | LIMITED：本轮 commit 行 + 门状态表加 S4R 行 + 最近一次绿灯加行 + 已对齐节加 RUNTIME 对齐记录 |
| `开发计划/规划AI会话.md` | 新建：规划 AI 会话渠道登记（会话 ID/URL/复用规则/fallback 约定/已通过事实） |

## Tests / Gates

- **Quick Gate**（`.\tools\verify_unattended.ps1`，2026-09-09，exit 0 = PASS）：**EditMode 246/246（failed 0, skipped 0）；PlayMode 11/11（failed 0, skipped 0）；Content Audit PASS fresh=YES failures=0**。测试计数与基线一致，无未解释增删。
- Performance Gate：按 S4R Test Policy **不重跑**（无 runtime/performance drift），引用 S4 Final `-IncludeArtPerformance` 证据 @ b3eb9fe（canonical worst p99=4.840ms=58.1% / Art worst avg=4.085ms=49.0%）。
- 附带观察：Gate 重再生的 `CONTENT_AUDIT_S3_CLOSEOUT.md` 与仓库版本字节级一致（未出现在 git diff）——确定性渲染契约在实测中成立。

## Delta 声明（逐项）

- **Capability Delta: NONE**（Ledger 为首次建立=文档事实；无能力新增/晋升/降级）
- **Mechanic Support Delta: NONE**（同上；无 mechanic promotion）
- **Runtime Delta: NONE**（零代码/预制体/数据行为改动；`git diff` 不含任何 `.cs`/场景/资产）
- **Canonical Data Delta: NONE**（ContentData/ 与 Catalog 零改动；无第二数据源）
- **Content Delta: NONE**（3/7/17/16/5/3/6 全轴不变，与 Production Report 一致）

## Frozen Baseline Reconciled

**YES** — `S4R_FROZEN_BASELINE.md` 18 项冻结事实全部有 authoritative reference + evidence pointer；规划 AI 下发的第 3 节 13 行基线表逐项对上（6 槽/17 词缀/单一 applicability/3 Skills/7 Supports/Passive 16-20-0/DarkKnight/4 视觉/SFX 5/Voice 3 GATED/性能值/sim hash/测试基线/门状态）。

## STATUS Consistent / ROADMAP Consistent

- STATUS：S4=COMPLETE 原样保留；S3 Voice=DEFERRED BY DIRECTOR 原样保留；S4 硬停止原文未被删除/弱化/解释为已授权 S5（仅新增 S4R 行=治理周期+冻结语义）。LIMITED 更新合规（未把 S4R 任何 Phase 标 COMPLETE）。
- ROADMAP：S4R=inter-cycle/direction readiness；下一实施周期=Director-Gated；downstream forbidden 项标注「仍未授权，候选的存在不构成承诺实施」。

## Drift Found（1 项，已在本单修复）

**R-04（文档漂移，severity=低，非 runtime 缺陷）**：`docs/RUNTIME.md` S2 装备节写「4 槽」「当前 13 条（词缀）」、UI 节写「2×2 迷你槽 Weapon/Body/Helmet/Boots」——与 S4-P2/P3 后的 runtime 事实（6 槽 / 17 词缀 / 抽屉 2×3 六槽）不符。原因：S4-P2/P3 轮更新了 STATUS/报告但未同步 RUNTIME。
**冲突双方摘录**：RUNTIME 原文「4 槽。Ordinary 2 Affix，Rare 3–4。词缀见 `AffixCatalog`（当前 13 条…）」vs Production Report `"counts": {"affixes": 17, "equipmentSlots": 6}` 与 `SliceDrawerLayout.cs`（`ColumnH=326`、注释「S4-P2：六槽 2×3（3 行）后加高列；旧 232=2 行」）。
**处置**：符合 WO-01「RUNTIME: UPDATE ONLY IF BASELINE DOCUMENTATION IS STALE」授权→已修复（契约描述+易变计数改快照指针，遵守 S3-M1 长期规则）；Runtime 零改动。**修复后 Drift = 0。**

## Source-of-Truth Conflicts

**0** 语义冲突（唯一 mismatch 即上述 R-04 文档漂移，已修复；历史快照文本按冻结原则不现代化）。

## Forbidden Expansion Audit

**PASS** — 本单零 gameplay/代码改动；Forbidden Registry（`S4R_FROZEN_BASELINE.md` §3）逐项对照：未触碰 S5/Progression/Map Tier/Boss/Unique/Ring/Offhand/Amulet/Content Batch/Voice/Curse/Flask/Jewel/Atlas/New Class/Deep Craft/新槽位/新技能/新词缀族/新货币/dormant 系统/替代数据源。

## Director-Gated Items Confirmed（保持 NOT AUTHORIZED / DEFERRED）

S5 与一切等价实施周期；Progression Spine；Map Tier；Boss；Unique；Ring/Offhand/Amulet；New Content Batch；Voice 解禁（含试听指认仍待导演）；Curse/Flask/Jewel/Atlas/New Class/Deep Craft；新 EquipSlot/Skill/Support/Affix family/currency。决策包待 S4R Phase 2 产出、Phase 3 由导演裁定。

## Open Questions（供规划 AI）

1. Reference Build Targets 尚未锁定（BDA Phase 1 范围）——S4R Phase 1 的 Backlog 分类将按「Candidate 周期」维度展开，是否需要在本周期内先建立候选 Reference Build 草案（纯文档）作为决策包输入？
2. 长期规划 Phase 8-12 候选方向的摘录已随本 Evidence Pack 附上（见回传消息）。

## Recommended Next WO

建议按 S4R 计划推进 **S4R-WO-02 = Phase 1 Backlog & Dependency Normalization**（分类不实现），由规划 AI 下发并确认范围。

## Self-Review 清单（协议 §6 十项）

编译（Quick Gate 含）/自动测试 246+11 全绿 ✓；Validator/Audit fresh PASS ✓；Baseline Regression 无（测试计数与门状态零变化）✓；Canonical 一致性 ✓；Ledger ✓；Matrix ✓；Markdown 与 Runtime 同步（R-04 修复后）✓；In Scope ✓（唯一代码邻接动作=零）；Forbidden Expansion 未触碰 ✓。
