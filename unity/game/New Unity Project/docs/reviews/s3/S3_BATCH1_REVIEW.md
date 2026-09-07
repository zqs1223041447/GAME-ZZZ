# S3_BATCH1_REVIEW（S3 第一批独立复核）

日期：2026-09-07。工作令：`S3-B1-RCLOSE`（规划 AI 下达）。复核角色：A15 Integration / Reviewer。以本地仓库真实 commit/diff 为准，未沿用实现轮汇报结论。

## Reviewed commits

- 主提交 `ba87b8a`：S3 第一批（校验扩展 + 3 组合系词缀 + 报告改出 BATCH1）
- 回填 `43eb331`：STATUS 纯文档回填

## Reviewed scope

- `Assets/Runtime/Core/Gameplay/Catalogs.cs`（AffixId/AffixDef 第二行/ItemInstance 第二值/3 词缀定义）
- `Assets/Runtime/Core/Gameplay/SliceSession.cs`（RollItem/TryDirectedCraft/AddItemDefensive/CollectSkillMods/AffixLine/DescribeItem）
- `Assets/Tests/EditMode/ContentAuditS2Tests.cs`、`Assets/Tests/EditMode/SliceLoopTests.cs`
- `docs/reviews/s3/CONTENT_AUDIT_S3_BATCH1.md`（测试再生）、DECISIONS/ROADMAP/STATUS
- 全 Runtime 范围搜索：`SetAffix`/`AffixIdAt`/`ValueAt`/`SecondValueAt`/`AffixCount`/`Affix0..3`/`Value0..3`/`AffixCatalog`/`ItemInstance` 全部引用点

## Findings（按严重度）

| # | 项 | 严重度 | 结论 |
|---|---|---|---|
| 1 | A. Hybrid 数据不变量：`SetAffix` 总是成对写第一/第二值，单行覆写组合词缀时第二值归零；槽 0–3 索引映射平行一致；第二值仅在 `RowCount==2` 时被读 | 低 | 无缺陷 |
| 2 | B. 消费路径：运行期 Affix 字段访问完全集中（Catalogs 结构体自身 + SliceSession 两消费点 + HUD 只以 `AffixCount` 做行界、文案经 `AffixLine`）；无绕开双行机制的旧路径；无独立 clear/reset 路径（整体替换走 `RollItem` 新结构体） | 低 | 无缺陷 |
| 3 | C. 双值 Roll：掉落与定向制作均对两行独立掷值（各自 Min/Max）；原测试未覆盖「两行范围差异明显」的用例 | 低 | **测试缺口，已补**（锐击 20–50 vs 0.15–0.30） |
| 4 | D. Stat 白名单真实性：27 个 Stat 逐一 grep 核对，全部落在真实运行期读取文件（`SliceSession.cs`：RecalcPlayer/BuildPlayerHit/ResolveSkillDef/ForkCount；CombatMath 消费 HitRequest 字段）；非「报告自证」 | 低 | 无假绿 |
| 5 | E. 兼容矩阵独立性：golden 为测试内字面数据，钉死不兼容/兼容组合双向防放宽/收紧；实现轮未自证 | 低 | 无缺陷；本轮继续强化（见下） |
| 6 | 文档滞后：审计报告文本「运行时暂不阻断」在兼容门落地后失效 | 低 | 随阶段二按测试机制再生修正（非手工改报告） |

## Fixes applied

- 无代码级 Bug 修复（未发现 Batch 1 引入的缺陷）。
- 测试补齐 2 项（C 项缺口 + 第二值残留回归）：
  - `DirectedCraft_AccCrit_DualRangeIndependent`：两行范围差异明显的组合词缀，第一行范围不污染第二行。
  - `DirectedCraft_SingleRowOverHybrid_NoSecondValueResidue`：组合词缀被单行覆写后第二值归零。
- 剩余风险（低，不修）：`AffixDef.RowStat(1)/RowOp(1)` 对单行词缀会返回无效行数据，契约是「调用方必须以 `RowCount` 为上界循环」——当前两处调用（防御/技能聚合）均满足；已由双行单测锁定。

## Tests added/changed

- 新增 `SupportGateTests`（4）：矩阵 parity（Runtime 判定 == golden，全 18 组合）、非法连接拒绝且无写入、失败替换保留原状态、合法组合不变。
- 新增 `SupportCompatGolden`（golden oracle 单一来源，ContentAuditS2Tests 与 SupportGateTests 共用，禁止由 Runtime 推导）。
- SliceLoopTests 补 2（见上）。EditMode 83→89。

## Reviewer verdict

**PASS**（无缺陷修复项；审计独立性维持；阶段二运行时兼容门按工作令实施，见 DECISIONS/STATUS）。
