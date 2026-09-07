# S3_M1_REPO_TRUTH_REVIEW（S3 维护轮 M1 独立复核）

日期：2026-09-07。工作令：`S3-M1-REPO-TRUTH-CATALOG`。Phase R（A15 Reviewer）：以真实 diff 复核，不以实现轮汇报为依据。

## Baseline HEAD

2ba4daa（P12 收口提交，工作树干净）。

## Reviewed files

- `Assets/Runtime/Core/Gameplay/Catalogs.cs`（SupportId sentinel + Count/容量派生）
- `Assets/Tests/EditMode/SupportCatalogInvariantTests.cs`（新增 4 测）
- `docs/RUNTIME.md`、`docs/reviews/STATUS.md`、`docs/ROADMAP.md`（事实修正）
- `docs/reviews/s3/S3_M1_REPO_TRUTH_AUDIT.md`（新增）

## Drift findings（Phase A 扫描结论）

- current-state 漂移 3 处：RUNTIME Support 数（6→7 缺火焰转化）、RUNTIME Affix 数（10→13）、STATUS 音频节（「查找表全空」/「语音未接」与已投放事实相悖）+ ROADMAP 门控表现状列 2 行。
- historical/current 混排 1 处：RUNTIME 音频节 S1 日志行为未标历史（已标 S1 historical baseline）。
- historical snapshot 保持未改：DECISIONS 日期条目、CONTENT_AUDIT_S2/BATCH1/R2、S2_PLAYER_MODEL_R、S3_R2_REVIEW（95/95 为当轮正确历史）——git diff 复核未触碰。

## Documentation fixes

见 `S3_M1_REPO_TRUTH_AUDIT.md` 处置表：RUNTIME Support/Affix/音频、STATUS 音频、ROADMAP 现状列；「稳定契约 vs 当前快照」规则已记入 DECISIONS（易变数字以最新 Content Audit/STATUS 为快照，RUNTIME 存接口与行为契约）。

## Catalog maintenance diff

- `SupportId` 增 sentinel `Count=8`（注释明确：不算内容、不进 UI/golden/审计内容清单）。
- `SupportCatalog.Count`=(int)SupportId.Count-1；`_defs` 容量=(int)SupportId.Count——双 magic number 清零。
- 其余 Catalog（Affix/Passive/MapAffix/Enemy）未动（遵守「不顺便重构」）；未建通用目录框架。

## Sentinel leakage audit

- UI（SliceHud 辅助栏）：遍历 1..SupportCatalog.Count → 只渲染 7 真实 Support，sentinel 不显示。
- Audit：Support 循环 1..Count；护栏断言 Support==7（sentinel 不计内容）。
- CompatGolden：键集无 sentinel（测试断言）。
- Runtime Get：None/sentinel/255 全部 default 不越界（测试断言）。

## Test result

- EditMode 100/100（96→100，+4 不变量测试）、PlayMode 3/3、Compile 0 error。
- 含兼容门 parity 21 组合、资源契约 REQUIRED 6/6、Tag parity 3/3、死 Tagged Modifier=0 全部保持。

## Content delta

全轴 0（Support 仍 7 真实——sentinel 不计；Skill/Affix/Passive/Enemy/MapAffix/Stat/Tag/ModOp/Effect/Event/Condition 均不变）。

## Gameplay behavior

- 唯一 Runtime 逻辑变化=SupportCatalog 计数/容量推导方式；CombatMath / TrySetSupport 行为 / loot / craft / skill / enemy 零变化（diff 复核）。
- Fork / FireConversion 关键定义零变化（不变量测试锁定：MechanicSkill=Projectile；ConvertPhysToFire Flat 0.50 RequiredTags=Attack|Hit|Physical MechanicSkill=None）。

## Remaining risks

- 其它 enum（EnemyKind 等）无 sentinel——按工作令记录不重构；未来如需同模式逐个立项。
- 资源契约与 Runtime 查找规则同源复制（P12 已登记，维持）。

## Verdict

**PASS**
