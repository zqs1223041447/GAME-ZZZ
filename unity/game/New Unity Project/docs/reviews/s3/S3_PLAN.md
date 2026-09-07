# S3_PLAN（S3 后续工作规划 · 纯文档）

日期：2026-09-07。**当前状态（2026-09-07 收口后）**：Phase 1=COMPLETE、Phase 2=COMPLETE（R1/R2 均通过 I/R）；**Phase 3 UI / Phase 4 人声 / Phase 5 精模=导演门控 GATED/NOT STARTED**；新一批内容不得自行开始；后置系统仍关；**S3 整体未结束**。下文「开工口令=导演明说『开 S3』」为**历史开工门规**（当时未开 S3 时的规则），不再是当前状态。

## 阶段 0 · 永关清单（除非导演单条解禁）

大天赋树、Unique 库、Atlas、完整 Craft、DOTS、HDRP、FMOD、换装/捏脸。
状态：**禁止**——每条解禁都需导演单独立令，不随「开 S3」口令连带解禁。

## 阶段 1 · 内容工厂最小工具（现在可做，纯校验扩展）

**状态：COMPLETE（2026-09-07，工作令 S3-P12 收口；证据 `docs/reviews/s3/S3_PHASE2_CLOSEOUT.md` + `CONTENT_AUDIT_S3_CLOSEOUT.md`——资源路径级真实验证 / Tag 组合规则+golden / 兼容矩阵 / 未用 Tag 持续记录 全部落地）**

- 依托现有 `ContentAuditS2Tests` 扩展：①缺资源报告细化（VFX/预制体/音频路径级）②Tag 合法性细则（Tag 组合规则表）③**Support 兼容矩阵校验**（Q/W/E × Support 非法组合提前报）④未用 Tag 持续钉死。
- 原始依赖（历史）：无（现有校验框架）；原始状态（历史）：导演点「开 S3」后第一批可含——**已执行**。
- 产出红线：只报问题不加内容；报告仍落 `docs/reviews/s3/`。

## 阶段 2 · 技能/词缀/怪「组合已有机制」增产（R1/R2 轮）

**状态：COMPLETE（2026-09-07；R1=3 组合词缀 `S3_BATCH1_REVIEW.md` PASS，R2=火焰转化 `S3_R2_REVIEW.md` PASS；证据 `S3_PHASE2_CLOSEOUT.md`）**

- 规则：**只用数据 + 已有 Effect/Trigger/Stat 组合**产新条目（如新词缀=现有 Modifier 组合、新怪=现有 Kind 换参数+现动画）；**禁止新 MonoBehaviour 堆系统**、禁止新 Effect 引擎路径。
- 批次建议：R1=+少量 Affix（≤3 条，全部由既有 ModOp 组合）；R2=+1 Support 机制变体（复用现有 Support 框架字段）。
- **执行状态（2026-09-07）**：R1 已落地（第一批 3 组合词缀 + 校验扩展，BATCH1 报告）；R2 已落地（工作令 S3-R2-FIRE-CONVERSION，方向由规划 AI 指定=物理转火：火焰转化，零专用分支，R2 报告 + `S3_R2_REVIEW.md` PASS）。
- 原始依赖（历史）：阶段 1 校验先行；R2 当时需规划 AI 指定变体方向（**R2 不再待开工**）。每批次仍走 I/R 全流程 + CONTENT_AUDIT 再生。

## 阶段 3 · UI 实现轮（依赖导演过目方案页）

- ①底栏+双球 → ②右侧装备抽屉 → ③Tooltip 升级（方案见 `docs/ui/UI_PROPOSAL_POE_D3.md`）。
- 依赖：**导演对方案页点头**（布局/基调）。状态：**等导演**。
- 约束：仍 IMGUI 换皮、不上 UI Toolkit；每轮微循环可玩、测试全绿。

## 阶段 4 · 音频人声映射（依赖导演指认）

- 依赖：**导演试听 `Desktop/voice_listen/` 20 条候选并指认**（Cast/Hit/Death）。
- 状态：**等导演**。指认后投放 `Resources/Audio/Voice/<键>`（零代码）+ 实机验证。
- 5 键战斗 SFX 已全 CC0 投放（`docs/reviews/audio/SFX_SOURCES.md`），本阶段不重复。

## 阶段 5 · Art Bible 精模投资（依赖导演给优先级）

- 范围锁死：**玩家 / 核心技能 / Boss / 代表怪 / 关键地标**；普通箱墙树不单做（Kitbash）。
- 依赖：导演点名先做哪件 + 素材来源（同 Dark Knight 采购路径）。状态：**等导演**。
- 每件精模走「入库检查 + 换模挂载 + 贴地验证」既有管线。

## 阶段 6 · 后置项单列（开启条件，不是排期）

| 项 | 开启条件（全部需导演单独立令） |
|---|---|
| Unique 库 | 大树之后；且已有 ≥2 轮组合增产经验 |
| 大天赋树 | 导演给树形规模与节点数上限 |
| Atlas | 完整 Craft 之后 |
| 深度 Craft | 废料/蚀刻剂经济有实际循环数据 |
| Boss 哲学（遭遇设计） | 精模 Boss 落地至少 1 只 |
| 1440p@120 补测 | **已完成并 CLOSED（M7 locked hardware，2026-09-08）**——不再是待开启项（历史：导演曾裁定「以后再说」挂起；M7 后硬件/门条件变化并完成补测，证据 `docs/reviews/s2p/1440p-120-m7/`） |
| DOTS/HDRP/FMOD | **永不上**（除非导演推翻 DECISIONS 基线） |

## 阶段 7 · 1440p@120（非本阶段前置）

- **状态：COMPLETE / CLOSED（M7 locked hardware，2026-09-08）**——真实 2560×1440/D3D12/PC/vSync=0/targetFps=-1 下 18 测量全 PASS（worst p99=2.388ms / 预算 8.33），证据 `docs/reviews/s2p/1440p-120-m7/`。
- 历史演进：导演曾裁定「以后再说」（维持挂起、不补测、不宣布 120@1440p）；M7 建立锁定硬件 canonical Performance Gate 后条件变化，完成补测并收口。
- **它仍不是 S3 Phase 3-5 的自动前置**：完成不自动启动 UI / 人声 / 精模（各自仍等导演立令）。
- 锁定硬件边界不变：结论仅适用 M7 锁定 CPU/GPU + canonical 契约，其它机器须按 `docs/qa/PERFORMANCE_GATE.json` 显式更新并重建 baseline。

## 第一批 S3 开工允许清单（**历史开工范围 · 已执行**）

只允许：**阶段 1 内容工厂校验扩展 + R1（≤3 条组合系 Affix）**。
仍禁止：新机制引擎、新 MonoBehaviour 系统、Unique/大树/Atlas/深度 Craft、任何阶段 0 项。
（本清单为当时第一批的范围证据，**不是下一轮的待办**——Phase 1/2 均已收口。）

## 汇报与门规

- 每阶段仍走 I/R 全流程；CONTENT_AUDIT 随内容变更再生；STATUS 的「最近一次绿灯」随每轮追加。
- 导演两件待办（不阻塞维护轮）：①UI 方案页过目（点头/批改）②人声 20 候选试听指认。
