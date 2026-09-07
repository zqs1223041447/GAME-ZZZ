# S3_R2_REVIEW（S3 R2 转换型 Support 独立复核）

日期：2026-09-07。工作令：`S3-R2-FIRE-CONVERSION`（规划 AI 下达）。Phase R（A15 Reviewer）：在 Phase I 实现全绿后，从真实 diff 重新复核，不以「刚写的所以没问题」为依据。

## Reviewed commit/diff

- 本轮实现（Phase I）：`SupportId.FireConversion`（SupportId 7 / Count 6→7 / `_defs[8]`）+ `SupportCompatGolden` 扩 3×7 + `S3R2FireConversionTests`（A–F 六测）+ 审计报告改出 `CONTENT_AUDIT_S3_R2.md` + HUD 辅助栏最小几何修复（6→7 格）。
- 以上一轮已收口基线（3a45983/6d12a90）为对照。

## Scope（Reviewer A：是否真的只新增 1 Support）

- Support +1（火焰转化，唯一）；内容数量护栏为**测试断言**（ContentAuditS2Tests 内）：词缀=13、StatId=28、ModOp/Tag/Effect/Event/Condition/Skill/图词缀轴全部 +0。**无其它内容轴变动。**

## Compatibility result（Reviewer B）

- golden 3×7 全 21 组合人工钉死（禁止从 Runtime 反推）；`FireConversion` 列：Melee ✓ / Projectile ✓ / Area ✗。
- Runtime parity：SupportGateTests + S3R2 测试内对拍，21/21 一致（Runtime 由 RequiredTags 自然推导，无本 Support 特例）。

## Conversion composition result（Reviewer C）

- Support 单独：基线 0 → +0.50（`StatAggregation_SupportAddsExactlyHalfConversion`）。
- 与烬心组合：0.40 + 0.50 = **0.90**（`SupportAndCinderHeart_ComposeNinetyPercent`）——两来源经同一 StatBag 轴聚合，无专属叠加规则。

## Consumer（Reviewer D）

- `ActualHit_CompositionConvertsPhysToFire`：BuildPlayerHit 结算前 `ConvertPhysToFire=0.50` 进入 HitRequest；同输入对比 ResolveHit——物理成分 8→4 下降、火焰成分 0→4 上升（护甲/抗性置零稳定观察）。转换真实进入现有伤害结算，非「StatBag 里未被消费的数字」。

## Special-case branch audit（Reviewer E：Architecture）

- 全 Runtime grep `FireConversion`：**仅 Catalogs.cs（枚举定义 + 目录数据）2 处**；ArenaSim / CombatMath / TriggerSystem / 命中结算 / 每帧 Update / 弹道命中 / 敌人命中 **0 条专用分支**。该 Support 是内容组合，非伪数据驱动。

## Regression result（Reviewer F：现有 6 Support）

- AddedFire / Brutal / Faster / Combustion：数值路径测试通过（ValidCombinations_Unchanged、StatAggregation 等）。
- Concentrated：范围缩半径测试通过（`Concentrated_ShrinksAreaRadius`）。
- Fork：弹道分裂测试通过（`ForkSupport_SplitsProjectileOnHit`），MechanicSkill 限制与兼容门行为未变。
- 兼容门契约回归：InvalidAttach / FailedReplacement 全过。

## Findings（Reviewer 期间发现与处置）

| # | 发现 | 处置 |
|---|---|---|
| 1 | 首版 RuntimeAttach 测试试图把同一 Support 同时装到近战与弹道，被**既有规则**「同一 Support 全局唯一」（IsSupportUsed）正确拒绝——测试预期错误，非产品缺陷 | 修正测试（先卸下再装另一技能）；既有规则保持不变 |
| 2 | HUD 辅助栏原硬编码 6 格（3 行），7 个 Support 会溢出 | 最小几何修复：改用 `SupportCatalog.Count` 遍历 + 托盘 104→132 高（4 行），未扩成 UI 任务 |
| 3 | 无 Combat Math 修改需求（现有转换公式直接成立，无 Bug） | 无修复；未动公式/顺序/clamp |

## Fixes applied

- 测试预期修正（上表 #1）+ HUD 托盘最小几何修复（#2）。均为本轮直接引入问题的最小修复。

## Remaining risks

- 低：`SupportCatalog.Count`/`_defs` 容量仍为手工维护（7/8），下轮新增 Support 时需同步——内容数量护栏断言会在漏改时变红（防静默遗漏）。
- 低：MechanicSkill 语义当前仅「单技能限制」；若未来出现多技能机制 Support 需扩展表达（R2 未需要，未预扩）。

## Verdict

**PASS**
