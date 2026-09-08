# S4_P1_TOOLING_GAP_REVIEW — Production Readiness（工作令 S4-P0P1-PRODUCTION-READINESS）

**日期**：2026-09-09　**Baseline**：main @ 7edbb6f（clean，S3=COMPLETE; Voice DEFERRED BY DIRECTOR）
**本轮范围**：S4 规划入库（`S4_PLAN.md`）+ Production Tooling 能力审计 + 补齐第一个缺口（Production Content Report v1）。**不开始 Gloves/Belt**（Phase 2 未开工）。

## 1. Repo Truth Audit（本地 clean HEAD 重读）

| 项 | 状态 |
|---|---|
| docs/DECISIONS.md | 规则①-⑭+⑯ 在库（无⑮，编号历史跳空）；无人值守自动执行为核心规则（导演 2026-09-08）；Stage0 锁延续 |
| docs/RUNTIME.md | 程序集=Game.Runtime.Core / Game.Runtime.Content（业务）；SeededRng 唯一随机源；测试程序集 Game.Tests.EditMode / PlayMode（非业务） |
| docs/reviews/STATUS.md | S3=COMPLETE；Phase 4 Voice=DEFERRED BY DIRECTOR；Phase 3 UI / Phase 5 Art=COMPLETE |
| docs/reviews/s3/S3_OVERALL_CLOSEOUT_REVIEW.md | PASS — S3 COMPLETE; VOICE DEFERRED BY DIRECTOR（Stage0 全锁；四门+Art Gate 全 PASS） |
| docs/qa/ | PERFORMANCE_GATE.json（M7 唯一真相源）/ ART_PERFORMANCE_PROFILE.json / PERFORMANCE_BUDGET.md / UNATTENDED_VERIFICATION.md |
| Content Audit 实现 | EditMode `ContentAuditS2Tests.CollectCurrentRepositoryAudit`（Collect→Render→Persist→Assert，failure-safe S3-M3） |
| Catalogs | SkillCatalog/SupportCatalog/AffixCatalog/PassiveCatalog/MapAffixCatalog/EnemyCatalog（Core/Gameplay/Catalogs.cs） |
| RuntimeResourcePaths | 单一拼接契约（REQUIRED 10/10） |
| Support compatibility oracle | `SupportCompatGolden.Matrix`（独立 oracle；Runtime parity 由 SupportGateTests 校验） |
| Passive Links | PassiveCatalog.Links canonical；audit 校验对称+连通 |
| Enemy visual catalog | `EnemyVisualCatalog`（EnemyKind→可选 Resources 路径；Dummy→null） |
| verify_unattended tooling | `-SelfTest`（合成夹具，不启 Unity）/ 默认 Quick Gate（EditMode+PlayMode+Audit）/ `-IncludeBuild` / `-IncludePlayerRun` / `-IncludePerformance` / `-IncludeArtPerformance` |

**Public GitHub cache mismatch**：公开仓库（https://github.com/zqs1223041447/GAME-ZZZ）页面可访问，但网页缓存落后于 7edbb6f（首页历史很浅；docs/reviews/s3/ 缓存列表只到早期文件；最新 Closeout 文件直抓 raw 会 cache miss）。协调席已将「公开索引滞后」记入 S4 repo-truth 风险。**处理原则：本地 clean HEAD 真相优先；公开缓存不作为 truth 来源。**（另注：docs/ROADMAP.md 页首阶段行仍是 S3 中期旧态「Phase 3/4/5=GATED/NOT STARTED」，与 STATUS/closeout 存在时间差——已记录为 finding，不擅改历史路线图正文。）

## 2. Tooling Capability Matrix

| Capability | Existing | Canonical source | Gap |
|---|---|---|---|
| Skill validation | ✓ | ContentAuditS2Tests（参数契约）+ SkillTagGolden | 无 |
| Support compatibility | ✓ | SupportCompatGolden.Matrix + audit CompatProblems + SupportGateTests（Runtime parity） | 无 |
| Affix validation | ✓ | audit BadAffix（StatId/ModOp/行契约/Min-Max） | 无 |
| Passive graph validation | ✓ | audit MissingLinks + BFS 连通 | 无 |
| Map mod validation | ✓ | audit（Id 唯一 + StabilityCost） | 无 |
| Runtime Stat consumption | ✓ | `RuntimeConsumedStats` 白名单（unknown Stat=audit FAIL） | 无 |
| Resource validation | ✓ | ContentResourceAuditContracts（真实加载；REQUIRED FAIL / GATED 记录） | 无 |
| Content counts | ✓ | audit PinCount 护栏 + Catalog 计数 | 无 |
| Missing asset report | ✓ | audit RequiredResourceMissing | 无 |
| **Production summary（machine-readable 单文件汇总）** | **✗ 缺** | — | **本轮补齐：Production Content Report v1** |

结论：收集/校验能力已全覆盖；唯一缺口是「机器可读的单一生产汇总报告」。原总计划列过的 Tool 不自动再造成同名平行工具——本缺口经 Branch 判定后由现有 seam 补齐。

## 3. Branch Decision（工作令 §5-§8）

**选 Branch A — Existing Collector Reusable。**

理由：`ContentAuditS2Tests` 已有完整 canonical collection seam（Collect → Render → Persist → Assert，failure-safe 语义 S3-M3 冻结），且全部 §10 要求字段（计数/Stat 消费/Tag/兼容/Passive/资源/视觉）都有 canonical 来源可直接汇总。无需 Branch C 的 read-only adapter，也不满足 Branch B（当前仓库没有任何 machine-readable production report）。

**实现边界（不建第二套 scanner，不复制 truth）**：
- 生产报告 = 既有 seam 上的 **renderer/persistence**：`ProductionContentReport.Build(ContentAuditResult)` 只汇总 audit result + canonical oracle/enum（SupportCompatGolden / PassiveCatalog.Links 只读 / EquipSlot / EnemyVisualCatalog / SkillTagGolden.DeclaredTags）。
- collector 最小**加法**扩展：`ContentAuditResult.DeclaredStats`（ScanMods + 词缀行收集 Stat 名，不参与任何失败判定）+ `RuntimeConsumedStats` 改 internal 供报告引用同一白名单（不建第二份 Stat truth）。校验逻辑零改动。
- **Verdict 只继承 audit 真相**：PASS ⇔ audit.Completed && FailureCount==0。不重复校验、不自建失败类别。
- 产物：`docs/qa/CONTENT_PRODUCTION_REPORT.json`（工具=EditMode 测试再生，禁止手填；确定性渲染，无时间戳/commit/GUID/绝对路径）。
- **New parallel truth introduced：无**（parity 测试守卫：audit block 与源 Result 逐字段一致；计数=canonical Catalog；Support 组合=golden oracle；Passive 边=canonical Links；视觉逐 kind=EnemyVisualCatalog）。

## 4. Report 字段覆盖（工作令 §10）

| 要求字段 | 实现位置 |
|---|---|
| Provenance（schema/schemaVersion/generatedBy/collector） | JSON 头部（不含 commit/时间戳——同 HEAD 可重复再生） |
| Counts（skills/supports/affixes/passives/enemyKinds/mapMods/equipmentSlots） | `counts`（全部读 audit/Catalog，不硬编码） |
| Runtime Coverage（declared/consumed/unconsumed Stats + declared/unused Tags） | `runtimeCoverage`（白名单单一 truth 引用） |
| Compatibility（skill×support 组合 + oracle/parity status） | `supportCompatibility` |
| Passive（node/edge/disconnected，只读 canonical Links） | `passiveGraph` |
| Resources（required/gated/present/missing；Voice=GATED/DEFERRED 不算 missing failure） | `resources` |
| Visuals（formal mapping count / missing formal non-benchmark；Dummy 排除） | `enemyVisuals`（从 EnemyVisualCatalog 派生） |
| Verdict | `verdict`（继承 audit） |

## 5. Fail / Non-Fail 语义（工作令 §11）

- FAIL（经 audit 失败类别继承）：missing required runtime resource / unknown Stat / unconsumed active Stat / Support 兼容 parity 违规 / invalid Affix row / canonical Passive 边破坏 / duplicate-stable ID / formal required visual missing / audit 任何 hard failure / Collect 未完整执行。
- 不 FAIL：unused reserved Tag（信息项）/ Voice deferred（GATED 缺失如实记录）/ Stage0 内容不存在 / unused screened art candidate。

## 6. Scope Delta（本轮）

- Gameplay delta：**0**　Content delta：**0**（无新 Skill/Support/Affix/Passive/Enemy/MapMod/EquipSlot）　UI delta：**0**　Art delta：**0**　Audio delta：**0**
- 新增=纯 tooling/tests/docs：`ProductionContentReport.cs`、`ProductionContentReportTests.cs`、`ContentAuditFailsafe.cs`/`ContentAuditS2Tests.cs` 加法扩展（DeclaredStats 捕获 + 白名单 internal，校验逻辑零改动）、`docs/reviews/s4/` 两文档、`docs/qa/CONTENT_PRODUCTION_REPORT.json`（测试再生）。
- Phase 3 UI COMPLETE 不动；Phase 5 Art COMPLETE 不动；Voice 继续 deferred。
