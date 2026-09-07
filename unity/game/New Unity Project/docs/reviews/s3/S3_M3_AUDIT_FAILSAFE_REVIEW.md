# S3_M3_AUDIT_FAILSAFE_REVIEW（S3 维护轮 M3 独立复核）

日期：2026-09-08。工作令：`S3-M3-AUDIT-FAILSAFE-TRUTH`。Phase R（A15 Reviewer）：以真实 diff 复核。Baseline HEAD=1a358e5。

## Baseline

- HEAD=1a358e5（M2 回填）；EditMode 105/105、PlayMode 3/3；工作树净（仅工作区外未跟踪素材目录，不触碰）。
- `CONTENT_AUDIT_S3_CLOSEOUT.md` baseline=6073 bytes，sha256=9f14fed46b7a68fe0836e95e18e940319f44adebba88b48c4769e66b6ddbc1fb。
- 内容 Count 全轴：Support 7 / Affix 13 / StatId 28 / ModOp 4 / Tag max Duration=256 / Effect max=2 / Event 4 / Condition 4 / Skill 3 / MapAffix 3 / Passive 16 / Enemy 5。

## 原有 failure ordering（风险确认）

旧主流程 `ContentAuditS2_Passes` 实际顺序 = **Collect 与 Assert 交织 → Assert.IsEmpty 批量断言 → WriteReport**。目录条目 Name、第一批 ≤3、10 条数量护栏、FireConversion 契约、Passive 数量/Name/无链接、图连通、技能 timing、怪 Life/Name、MapAffix StabilityCost/Id 共 **25+ 处 NUnit 直抛全部发生在写报告之前**；最后 9 条 Assert.IsEmpty 也在 WriteReport 之前。风险成立：任何内容违规/基础设施异常 → 测试红但仓库当前报告仍保留上一轮 PASS——违反 Repository-as-Memory。另：旧报告无总 Verdict，接棒 AI 需自行数表判断绿否。

## 新顺序（Collect → Render → Persist → Assert）

`ContentAuditFailsafe.ExecuteAudit(path, collect, render)`：Stage 1 collect（零断言，全部问题入 `ContentAuditResult` 集合）→ Stage 2 `render(result)` 纯渲染 → Stage 3 `File.WriteAllText` 落盘 → Stage 4 `AssertAuditResult(result)`。生产入口 `ContentAuditS2_Passes => ExecuteAudit(ReportPath, CollectCurrentRepositoryAudit, RenderReport)`，语义等价于工作令要求的生产流程。

## Direct pre-persist Assert audit（Reviewer 必查①）

rg 复核 `Assert\.`：`ContentAuditS2Tests.cs` 仅存 6 处，**全部位于合成负向测试 `ContentAuditTagRules_NegativeCases_AreRejected`（工作令明示豁免）**；Collect/Render 主流程 0 处。`ContentAuditFailsafe.cs` 的断言全部集中在 `AssertAuditResult`（Stage 4），唯一调用点是 `FinalizeAudit` 内 `File.WriteAllText` **之后**（rg 行序 127→129）。

## 不允许「先写 PASS 再发现 FAIL」（必查②）

Verdict 由 `RenderReport` 内已完成 Result 计算（`Completed && FailureCount == 0`）；所有校验在 Collect 阶段进 Result，渲染后无任何额外 validation（rg 复核渲染函数无校验调用）。

## FAIL 不能被后续 PASS 覆盖（必查③）

8 条 failure-safe 测试全部使用 `%TEMP%/gamezzz_audit_failsafe_tests` 临时路径，SetUp/TearDown 自清理；正式 CLOSEOUT 报告只由 `ContentAuditS2_Passes`（正式 current-state Audit）写入。`Execute_ReportWriteFails_TestReds_NoSwallow` 用「父级为文件」制造真实写失败——异常上抛（非断言失败、不吞），正式报告路径不被触碰。

## Synthetic PASS proof

`Render_SyntheticPass_CompletedYes_VerdictPass_ZeroFailures`：Completed=YES / Verdict: PASS / Failure count: 0 / 无 FAIL 字样。`Finalize_PassResult_WritesGreenReport_NoThrow`：临时路径写盘成功且文件含 Verdict: PASS。

## Synthetic FAIL proof

`Render_SyntheticFail_ShowsFailVerdict_AndDetail_NotPass`：Verdict: FAIL / Failure count: 2 / Audit completed: YES / 类别+明细（REQUIRED 缺失、结构/契约）均在 Markdown 内，且不含 "Verdict: PASS"。

## Finalize-write-before-fail proof（必查④）

`Finalize_FailResult_WritesRedReport_BeforeAssertThrows`：捕获 AssertionException 后检查临时文件——**文件已存在、Verdict=FAIL、明细已写入**；即失败异常发生之前红报告已落盘。失败路径用测试临时目录，未覆盖正式报告。

## Renderer determinism proof

合成级：PASS/FAIL Result 各渲染两遍完全一致（2 测）。仓库级实测：正式 Audit 连跑两遍，报告均 6585 bytes，sha256 两遍同为 0707e2b82f2ff555af9d416875eb124e15fb5b334dd94af31de31e3a0d25be66——**byte-equivalent**。渲染无时间戳/GUID/用户名/绝对路径/会话 id。

## Infrastructure exception behavior

`Execute_CollectThrows_WritesFailSnapshot_ThenPropagates`：collect 抛 NullReferenceException → 捕获后先写快照（Audit completed: NO / Verdict: FAIL / Execution error: NullReferenceException…）→ 原异常上抛（非 AssertionException）。写快照本身再失败则直接上抛 → 测试红。生产路径同函数（同一 catch 代码路径），契约被测试锁定。

## Current real audit verdict

正式 `CONTENT_AUDIT_S3_CLOSEOUT.md`（测试再生）：**Audit completed: YES / Verdict: PASS / Failure count: 0**；结构/契约问题=0；REQUIRED 6/6 真实加载 PASS（玩家预制体+5 SFX）；GATED 人声 3 键 0/3 如实记录不失败；死 Tagged Modifier=0（4/4 可达）；兼容矩阵违规=0；Tag golden parity 3/3；未使用 Tag=预留记录。GATED/未使用 Tag/VFX N/A/1440p/Phase 3-5 均未被计为失败（只记状态）。

## 功能覆盖不变（必查⑤）

重构前后逐项对位：StatId 合法性/运行期白名单/ModOp 范围、Support 兼容矩阵+golden 钉死（Pin）、词缀双行、R1 ≤3、R2 FireConversion 契约、Passive 链接对称/无链接/连通、Skill profile、Enemy、MapAffix、SkillTagGolden parity、Tag 当前形态规则、死 Tagged Modifier、REQUIRED/GATED 资源、未使用 Tag、内容冻结护栏——全部保留（现入 StructuralProblems/BadAffix/CompatProblems 等类别）。无通用 validation framework，无第三方包。

## Content delta

全轴 0（护栏断言全绿；Support=0/Affix=0/Skill=0/Passive=0/Enemy=0/Stat=0/Tag=0/Effect/Event/Condition=0/Resources=0）。

## Runtime delta

**0 文件修改**（rg `git status` 复核：仅 Assets/Tests/EditMode 3 文件 + 2 新文件/meta + 报告 + 文档）。Hot path changed=no；Update added=0；per-Hit changed=no；allocation runtime changed=no。无需 Arena。

## Remaining risks

- 低：Stage 4 断言逐类 Assert.IsEmpty——首类失败即抛，后续类别断言不再执行（报告已完整落盘，不受影响；失败明细在报告内完整可见）。
- 低：基础设施快照写盘若与正式报告同路径冲突（理论上 collect 异常时正式路径可用性未知）——当前 catch 内写失败直接红，符合「写失败→测试红」契约。
- 低：`Mathf.Abs` 容差比较 FireConversion 0.50 与原 `Assert.AreEqual(delta)` 等价（0.0001f）。

## Verdict

**PASS**
