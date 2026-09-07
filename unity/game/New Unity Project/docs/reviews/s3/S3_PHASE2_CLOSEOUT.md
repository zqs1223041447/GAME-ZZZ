# S3_PHASE2_CLOSEOUT（S3 阶段 1 + 阶段 2 收口）

日期：2026-09-07。工作令：`S3-P12-AUDIT-CLOSEOUT`（规划 AI 下达）。阶段 1（内容工厂校验扩展）与阶段 2（R1/R2 组合系增产）正式收口。

## Scope

### 阶段 1 · 内容工厂（本轮完成最后两项 + 此前已落地项）

- **缺资源报告路径级细化 + 真实验证**（本轮）：`ContentResourceAuditContracts`——对 Runtime 全部 Resources 入口逐项真实加载（同 key/同类型/同拼接语义）+ `AssetDatabase` 真实资产路径；REQUIRED 缺失=审计红；GATED 缺失=如实记录；「无声明引用」与「资源缺失」区分为两个状态（VFX=Declared 0 → N/A）。
- **Tag 合法性细则 / 组合规则**（本轮）：`SkillTagGolden`（人工钉死 3 技能完整 mask，独立 oracle）+ `ContentAuditTagRules`（Rule A 未声明 bit / Rule B golden parity / Rule C Attack+Spell 当前形态冲突 / Rule D Melee+Projectile 当前形态冲突 / Rule E 死 Tagged Modifier）+ 负向测试证明规则能抓坏合成输入。
- Support compatibility matrix（RCLOSE 已落地：golden oracle + Runtime 门 + 21 组合 parity）。
- 未使用 Tag 持续记录（预留≠失败，当前未使用 4 个：Spell/Projectile/Fire/Duration）。
- Stat consumer whitelist（RCLOSE 已落地，27/27 逐核）——此为额外保护，不替代资源/Tag 两项。

### 阶段 2 · 组合系增产

- **R1**：3 条组合系 Affix（灼燃 FireDamage+IgniteChance / 锐击 Accuracy+CritChanceIncreased / 熔铸 PhysicalDamage+FireDamage）；双行 Affix 数据模型（AffixDef 第二行 + ItemInstance 第二值，独立掷值）；全部走现有 4 槽掉落与两步 Craft；掉落/随机制作/定向制作/防御/技能聚合/Tooltip 全路径按行支持。
- **R2**：火焰转化 Support（ConvertPhysToFire Flat 0.50，RequiredTags=Attack|Hit|Physical）；Tag 驱动兼容（近战/弹道可接、范围拒绝）；Support 与 Passive 共用 ConvertPhysToFire（0.50+0.40=0.90 自然聚合）；**零 Support 专用 Runtime 分支**。

## Evidence（仓库内证据链）

| 证据 | 文件 |
|---|---|
| R1 审计 | `docs/reviews/s3/CONTENT_AUDIT_S3_BATCH1.md`（保留历史） |
| R1 复核 | `docs/reviews/s3/S3_BATCH1_REVIEW.md`（verdict PASS） |
| R2 审计 | `docs/reviews/s3/CONTENT_AUDIT_S3_R2.md`（保留历史） |
| R2 复核 | `docs/reviews/s3/S3_R2_REVIEW.md`（verdict PASS） |
| Runtime 兼容门 | `SliceSession.IsSupportCompatible` + `SupportGateTests`（3×7 parity） |
| 收口审计 | `docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`（本轮，测试再生） |

## Architecture Conclusions

1. Hybrid Affix 可以只用已有 StatId/ModOp 增产（R1 三条全部为既有轴组合）。
2. 新 Support 机制可以只靠已有 Modifier/Tag/Stat 进入 CombatMath（R2 火焰转化零专用分支，全 Runtime grep 仅目录定义）。
3. Support compatibility 已有 Runtime gate（TrySetSupport 写前拒绝、失败无半写入）。
4. Audit 与 Runtime 有独立 oracle（SupportCompatGolden / SkillTagGolden 均人工钉死，禁止 Runtime 反推）。
5. 内容来源不拥有独立战斗公式（Support/Passive 共享同一 StatBag 轴）。
6. 两轮均没有新 MonoBehaviour / 新战斗引擎路径（RCLOSE/R2 REVIEW 均已核查）。

## Phase 1 逐项对照（S3_PLAN 阶段 1）

| 原 目标 | 状态 |
|---|---|
| 缺资源报告细化（VFX/预制体/音频路径级） | **COMPLETE**（本轮：真实加载 + 分类契约 + 假状态清理） |
| Tag 合法性细则（Tag 组合规则表） | **COMPLETE**（本轮：组合规则 + golden + 死内容扫描 + 负向测试） |
| Support 兼容矩阵校验（Q/W/E × Support 非法组合提前报） | **COMPLETE**（BATCH1 落地审计 + RCLOSE 落地 Runtime 门） |
| 未用 Tag 持续钉死 | **COMPLETE**（持续记录，从不作为失败项） |

## Remaining Risks（如实记录，非本轮阻塞）

- `SupportCatalog.Count`/`_defs` 容量手工维护（护栏断言兜底，漏改即红）——低风险债。
- `MechanicSkill` 当前仅表达单技能限制；多技能机制 Support 出现时需扩展。
- 正式 UI / 人声映射 / 精模：**导演门控（GATED / NOT STARTED）**，非阻塞。
- 1440p 补测：硬件挂起（非 S3 前置）。
- 资源契约与 Runtime 查找规则为「同源复制」（key/拼接方式在测试侧复刻）——Runtime 改路径时需同步契约（有 REQUIRED 加载失败兜底）。

## Verdict

**S3 PHASE 1 = COMPLETE**
**S3 PHASE 2 = COMPLETE**（R1/R2 均通过 I/R 双角色；阶段 3-5 保持导演门控，S3 整体未结束）
