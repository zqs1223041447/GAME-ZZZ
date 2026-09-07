# S3_M1_REPO_TRUTH_AUDIT（S3 维护轮 M1 · Repository 事实收敛）

日期：2026-09-07。工作令：`S3-M1-REPO-TRUTH-CATALOG`。Baseline HEAD=2ba4daa（工作树干净）。本记录为人工整理，全部事实数字来自测试与扫描验证（EditMode 100/100 / PlayMode 3/3 通过后的最终代码态）。

## 当前事实快照（与测试断言一致）

| 轴 | 值 | 验证 |
|---|---|---|
| Support | 7（真实，sentinel 不计） | 支栏护栏断言 + 目录连续性测试 |
| Active Skill | 3 | 护栏断言 |
| Affix | 13（S2 基线 10 + R1 三条组合系） | 护栏断言 |
| Passive / Enemy / MapAffix | 16 / 5 / 3 | 既有审计 |
| StatId / ModOp / Tag / Effect / Event / Condition | 28 / 5 / 9 声明 / 3 / 4 / 5 | 护栏断言 |
| 战斗 SFX | 5 键 REQUIRED 全部真实加载 PASS | 资源契约（真实 Resources.Load + Asset 路径） |
| 人声 Voice | 3 键 GATED，0/3 present（缺失=静音，待导演指认） | 资源契约 |
| VFX | 声明引用 0 → N/A（与资源缺失区分） | Reviewer 独立扫描 |
| EditMode / PlayMode | 100/100、3/3 | pipeline run_tests |

## Docs 漂移扫描与处置（历史 vs 当前分类）

| 文件 | 发现 | 分类 | 处置 |
|---|---|---|---|
| `docs/RUNTIME.md`（Support 节） | 「6 Support」清单（缺火焰转化） | current-state 漂移 | **已修**：7 Support + 火焰转化机制事实（50% 物转火/RequiredTags 驱动兼容/无专用 Combat 分支/MechanicSkill=弹道的分裂标注）+ 兼容门契约一句 + 数量指向最新审计 |
| `docs/RUNTIME.md`（装备/掉落节） | 「10 Affix」 | current-state 漂移 | **已修**：指向 Catalog（当前 13）+ 单行/双行语义一句（独立掷值、第二值、RowCount 消费） |
| `docs/RUNTIME.md`（音频节） | 「预留 5 键，无资产=静音」与「已投放」同段并置易误读；S1 日志行为无历史标注 | current/history 混排 | **已修**：S1 日志行为标 **S1 historical baseline**；查找规则改述为契约（缺资产=降级模式）；当前事实明确（5 键 REQUIRED 6/6 PASS；人声 GATED 0/3） |
| `docs/reviews/STATUS.md`（音频节） | 「查找表 5 键全空=静音」「语音 ogg 未接」 | current-state 漂移 | **已修**：改为当前事实（5 SFX REQUIRED PASS；人声 GATED 0/3） |
| `docs/ROADMAP.md`（门控表现状列） | 音频两行「现状」列停留在投放前 | current-state 漂移 | **已修**：现状列更新（5 键已投放 6/6 PASS；人声 3 键已接映射 GATED） |
| `docs/DECISIONS.md` 日期条目（S2 底座 6 Support/10 词缀、音频挂钩·无素材等） | 描述当时状态 | **历史快照** | 保留不改（每条日期决策描述当时事实；后续条目已覆盖演进） |
| `CONTENT_AUDIT_S2/BATCH1/R2.md`、`S2_PLAYER_MODEL_R.md`、`S3_R2_REVIEW.md（95/95）` | 当轮历史快照 | **历史快照** | 保留不改（未因当前 100/100 被现代化） |

## SupportCatalog 不变量（本轮硬化结果）

- `SupportId.Count` sentinel=8（真实 7+1）；`SupportCatalog.Count=(int)SupportId.Count-1`；`_defs` 容量=`(int)SupportId.Count`——**手写 7/8 双 magic number 清零**（grep 复核，残留 7/8 均为无关常量 MeleeBase/ProjectileBase/StartPoints）。
- Get 边界：None / sentinel / 255 全部返回 default 不越界（测试锁定）。
- 目录连续：1..7 无洞、Id/Name/Desc 完整（测试锁定）。
- Golden 覆盖：键集=全部真实 Support（无缺/无重复/无 sentinel），合法技能⊆3 Active（测试锁定）；Runtime parity 21 组合全绿。
- 内容 delta：全轴 0（sentinel 不计作 Support +1，护栏断言验证）。

## 测试

- 新增 `SupportCatalogInvariantTests` 4 测：sentinel 契约 / 连续覆盖 / golden 完整性 / 既有身份零变化。
- 最终态：EditMode 100/100、PlayMode 3/3、Compile 0 error。
