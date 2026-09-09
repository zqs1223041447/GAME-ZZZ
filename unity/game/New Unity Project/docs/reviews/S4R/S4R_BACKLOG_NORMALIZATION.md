# S4R_BACKLOG_NORMALIZATION — 候选 Backlog 正规化（S4R Phase 1 交付）

**Work Order**: S4R-WO-02（规划 AI 2026-09-09 下发）。**性质**：documentation-only 候选池。**本文件任何条目都不构成实施授权**——`Candidate ≠ Approved`、`dependency order ≠ product commitment`（DECISIONS 同轮登记）。正式阶段状态与产品方向选择权在 Director Direction Gate（S4R Phase 3）。

## 0. 统计口径（AC-03/AC-09 对齐）

- **Backlog Total = 32**。
- **By Primary Classification**：ALREADY_SUPPORTED=0；BREADTH_EXTENSION=3；DEPTH_EXTENSION=9；CONTENT_ONLY=1；TECH_QUALITY=4；DIRECTOR_GATED=13；FORBIDDEN_UNTIL_APPROVED=1；DEPENDENCY_BLOCKED=1。
- **By Authorization State**：AUTHORIZED=0（**New gameplay authorizations = 0**）；NOT_AUTHORIZED=18；DEFERRED=13；BLOCKED=1。
- ALREADY_SUPPORTED=0 的原因：已支持能力（Ledger Supported 18 项）本身不构成 backlog item；其「合理 breadth/depth 后续」已按具体扩张项拆入 BL-001/002/021/022/023 等。此为 AC-01 要求的「明确标记为什么不构成」。
- 风险轴速记（AC-06，非 ALREADY_SUPPORTED 项不得空缺）：**P**=performance / **D**=determinism / **S**=save-migration / **T**=test burden / **C**=canonical-data dependency / **B**=runtime blast radius；取值 低/中/高/N-A。
- 依赖速记：`prereq:`=immediate/blocking prerequisite；`down:`=downstream dependents；`择`=与其它候选仅为产品选择关系、无技术依赖。

## 1. 候选池总表（32 项，去重后）

| ID | 名称 | 来源 | 族 / 现状 | Primary | 授权 | 玩家价值 | 依赖 | 风险 P/D/S/T/C/B | 关键备注 |
|---|---|---|---|---|---|---|---|---|---|
| BL-001 | 完整装备槽（10 槽 / Weapon Family / Handedness / Requirement / Base Items） | 长期规划 Phase 8①；BDA Phase 2 | Equipment / Unsupported | BREADTH_EXTENSION | NOT_AUTHORIZED | build breadth | prereq:无（复用 6 槽架构）；down:BL-002/028 | 中/中/中/高/中/高 | 10 槽需导演逐项解禁；Requirement 模型为新 capability |
| BL-002 | 更多 Affix（≥40 / prefix-suffix / Tier / Mod Group / Item Level·Weight 生成） | 长期规划 Phase 8②；BDA Phase 2 | Equipment-Affix / 17 条已成立 | BREADTH_EXTENSION | NOT_AUTHORIZED | build breadth | prereq:无（Tier 生成需 BL-024）；down:BL-018 | 低/中/中/高/中/中 | 新词缀族=导演冻结（BL-029）；现有 17 条全复用已有 Stat/ModOp |
| BL-003 | 完整 Passive Tree（PoEDB 镜像：大树/Scion/50 点/UI 全套） | 长期规划 Phase 8③；BDA Phase 3；Stage0 锁「大天赋树」 | Passive / Unsupported（现 16 节点自设计） | DEPTH_EXTENSION | NOT_AUTHORIZED | build depth | prereq:**BL-024**；down:BL-027 | 中/高/中/高/高/高 | IP 风险已在 Risk Register（BDA §18）；大树 UI=BL-027 前置 |
| BL-004 | Aura / Reservation（Pride/Anger/Determination/Grace + Mana Reservation 体系） | 长期规划 Phase 8④；BDA Phase 5 | Aura / Unsupported | DEPTH_EXTENSION | NOT_AUTHORIZED | combat depth+资源取舍 | prereq:Mana 模型扩展（新 capability）；down:BL-018 | 中/中/低/高/中/高 | S4R 明确禁实施；BDA Reference Builds 的关键取舍轴 |
| BL-005 | Curse | 长期规划 Phase 8⑤ | Ailment/Enemy / Unsupported | DIRECTOR_GATED | DEFERRED | combat depth | prereq:无；择 BL-006/007 | 低/低/低/中/中/中 | 导演未批；协议 §10 点名禁区 |
| BL-006 | Flask | 长期规划 Phase 8⑥ | Resource / Unsupported | DIRECTOR_GATED | DEFERRED | 防御/循环 | prereq:无；择 BL-005 | 低/低/低/中/低/中 | 导演未批 |
| BL-007 | 更多 Defense（Block/ES/Suppression/Leech 等） | 长期规划 Phase 8⑦；COMBAT_MATH「未做」 | Defense / Unsupported（现仅 Armour+Fire Res） | DEPTH_EXTENSION | NOT_AUTHORIZED | survivability | prereq:无；择 BL-005/006 | 中/低/低/高/中/高 | COMBAT_MATH 未做清单如实保留 |
| BL-008 | 更完整 Ailment（Bleed/Poison/Shock 等） | 长期规划 Phase 8⑧ | Ailment / Unsupported（现仅 Ignite） | DEPTH_EXTENSION | NOT_AUTHORIZED | combat depth | prereq:无；择 BL-007 | 中/低/低/高/中/高 | 现有 Ignite 单实例取高 DPS 语义可作模板 |
| BL-009 | 高级 Trigger | 长期规划 Phase 8⑨ | Trigger / Partial（TriggerSystem DepthCap=8 基础版） | DEPTH_EXTENSION | NOT_AUTHORIZED | build depth | prereq:现有 TriggerSystem 扩展授权；down:BL-018 | 中/中/低/高/中/高 | advanced Trigger implementation 列 WO-02 Forbidden |
| BL-010 | Unique（系统+内容库） | 长期规划 Phase 8⑩；Stage0 锁 | Item / Unsupported | DIRECTOR_GATED | DEFERRED | build identity | prereq:BL-001/002；down:BL-018 | 中/中/中/高/高/高 | 导演未批；协议 §10 点名 |
| BL-011 | Jewel | 长期规划 Phase 8⑪；Stage0（BDA Phase 3 内 Jewel Socket 先 Unsupported 保留） | Item/Passive / Unsupported | DIRECTOR_GATED | DEFERRED | build depth | prereq:BL-003（Jewel Socket 挂树）；down:BL-018 | 低/低/中/中/高/中 | 导演未批 |
| BL-012 | Skill Gem 高级成长（Quality/Corruption/Ascension/Alternate Form/Final Upgrade） | 长期规划 Phase 9 | Skill / Unsupported（SkillInstance 已稳定） | DEPTH_EXTENSION | NOT_AUTHORIZED | long-term growth | prereq:核心 SkillInstance 稳定（已满足）；down:BL-013 | 中/中/中/高/中/高 | 长期规划「核心稳定后才加」已满足前置，仍需导演方向 |
| BL-013 | Crafting Deepening（Add/Remove/Reroll/Lock/Targeted Reroll/Upgrade Tier/Special/Final/Corruption） | 长期规划 Phase 10；Stage0 锁「deep Craft」 | Craft / Partial（随机+定向+duplicate reject 已成立） | DEPTH_EXTENSION | NOT_AUTHORIZED | item chase | prereq:BL-002（Tier 语义）；down:BL-014 | 中/高（确定性经济）/中/高/高/高 | 现有 Craft 边界=洗 Rare/写入词缀，不扩 |
| BL-014 | Craft Simulator（100k 次成本分析 P50/P90/P99） | 长期规划 Phase 10 配套 | Tooling / Unsupported（现有 10k 生产 sim 是另一物） | TECH_QUALITY | NOT_AUTHORIZED | tooling（间接） | prereq:BL-013 语义集；down:BL-013 平衡 | 低/低/低/中/中/低 | WO-02 明确不开发；与 Production Simulator 不得混同 |
| BL-015 | Map Tier / Progression Spine（地图进程/Modifier Pool 扩张/Synergy/Stability/Reward Multiplier） | 长期规划 Phase 11；硬停止点名 | Map-Endgame / Unsupported（1 图 3 词缀） | DIRECTOR_GATED | DEFERRED | progression | prereq:Phase 8-10 族；down:BL-016/017/018 | 高/中/中/高/高/高 | **硬停止点名**；决定地图进程形态过早（S4_PLAN §2 同语义） |
| BL-016 | Boss 系统（Boss taxonomy/内容/Special Encounter/Monster Package） | 长期规划 Phase 11；硬停止点名 | Enemy / Unsupported（3 普通+1 Elite） | DIRECTOR_GATED | DEFERRED | encounter depth | prereq:BL-015 同族；down:BL-018 | 高/中/低/高/中/高 | 现有「无 BossKind」边界（Bruce=presentation 不改 taxonomy） |
| BL-017 | Map Risk Simulator | 长期规划 Phase 11 配套 | Tooling / Unsupported | TECH_QUALITY | NOT_AUTHORIZED | tooling（间接） | prereq:BL-015 语义集 | 低/低/低/中/高/低 | WO-02 明确不开发 |
| BL-018 | 内容工厂（技能/Support/Affix/Unique/Passive/Jewel/Monster/Map Mod/Boss Mod 大规模生产） | 长期规划 Phase 12 | Content / Unsupported | CONTENT_ONLY | NOT_AUTHORIZED | content volume | prereq:**核心机制稳定（BL-004/008/009 等先行）**；down:无 | 中/中/低/高/高/中 | 80% 组合已有机制/20% 提新需求；比例颠倒=架构失控 |
| BL-019 | Atlas | Stage0 锁 | Endgame / Unsupported | DIRECTOR_GATED | DEFERRED | endgame | prereq:BL-015 | 高/中/中/高/高/高 | 导演未批；协议 §10 点名 |
| BL-020 | 技术路线锁包（DOTS/Entities、HDRP、FMOD、捏脸换装） | Stage0 锁 | Tech / 锁定（反向决策=不做） | DIRECTOR_GATED | DEFERRED | —（约束项） | prereq:无 | 高/中/低/高/低/高 | 每项需导演单独解禁；当前 DECISIONS 明令禁止第二套架构 |
| BL-021 | Socket 孔色 / 多 Link / 扩孔（Gloves/Belt 词缀位→带孔） | RUNTIME 契约边界；S2 锁「无孔色」 | Socket / Partial（固定孔位一条 Link） | DEPTH_EXTENSION | NOT_AUTHORIZED | build depth | prereq:无；择 BL-022 | 中/低/低/中/中/中 | Gem Color metadata 保留语义（BDA Phase 2 边界） |
| BL-022 | Support Level 1–20 / Quality 0–20 | BDA Phase 4 | Support / Unsupported（Level/Quality 无数据模型） | DEPTH_EXTENSION | NOT_AUTHORIZED | build depth | prereq:BL-023 数据模型；择 BL-021 | 低/低/低/中/高/中 | BDA Phase 4 范围 |
| BL-023 | 全量 Active/Support canonical 导入 + Validation Gem Library/Loadout | BDA Phase 4 | Content-Pipeline / Unsupported（现 3+7 自设计） | BREADTH_EXTENSION | NOT_AUTHORIZED | content breadth | prereq:**BL-024**；down:BL-022 | 中/中/中/高/高/高 | 兼容体系已有 Tag 路径+golden oracle，可承接扩张 |
| BL-024 | PoEDB Canonical Import Pipeline（版本化快照/哈希/Import Report/Data Gap） | BDA Phase 1；S4R 禁令 | Tooling / Unsupported | **FORBIDDEN_UNTIL_APPROVED** | NOT_AUTHORIZED | 前置基建 | prereq:导演方向授权；**down:BL-003/023（blocking prerequisite）** | 低/高（可重复生成）/低/高/高/中 | WO-02 Forbidden 点名「PoEDB bulk ingestion pipeline implementation」；PoEDB=唯一来源不变 |
| BL-025 | Persistence / 存档与迁移 | Ledger Unsupported；BDA migration-risk 轴 | Meta / Unsupported（当前设计无跨会话存档） | DIRECTOR_GATED | DEFERRED | meta | prereq:导演产品语义决策 | 低/中/高/中/低/高 | 若做=新存档字段，属导演级产品决策 |
| BL-026 | Voice 解禁（人声映射指认→投放） | Voice GATED；DEFERRED BY DIRECTOR | Audio / **Blocked**（3 键 0/3 present，缺失=静音） | DEPENDENCY_BLOCKED | BLOCKED | presentation | **Resume Trigger=导演试听指认**（候选清单 `DK_VOICE_INVENTORY.md`） | 低/低/低/低/低/低 | 基础设施已就绪（VoiceCues+冷却+parity 测试），仅缺导演输入 |
| BL-027 | 天赋树 UI 重排（UI_PROPOSAL 分轮建议第 4 轮） | UI_PROPOSAL_POE_D3.md；导演门控表 | UI / Unsupported（现 16 节点固定布局） | DIRECTOR_GATED | DEFERRED | UX | prereq:BL-003（大树成立才有意义） | 低/低/低/中/低/中 | Stage0 禁区待规划审查 |
| BL-028 | 新装备槽点名项：Ring（×2 语义）/ Offhand 双持 / Amulet | 硬停止点名；导演门控 | Equipment / Unsupported | DIRECTOR_GATED | DEFERRED | build breadth | prereq:BL-001 槽位框架（子项）；down:BL-018 | 中/中/中/中/低/中 | 每槽需导演单独解禁；与 BL-001 去重说明=BL-001 记总体方向，本项记录点名槽位授权粒度 |
| BL-029 | 新内容轴扩张授权门槛（新 Skill/新 Support/新 Affix family/新 progression currency） | 硬停止+WO-02 Forbidden | Content / 冻结 | DIRECTOR_GATED | DEFERRED | —（门槛项） | prereq:导演逐项批准 | 低/低/低/中/中/中 | 与 BL-002 去重：BL-002=词缀广度方向本身；本项=其授权门槛记录 |
| BL-030 | Death 视觉 0.40s 回收截断修复 | S3 Phase 5 收口 known limitation | Presentation / Partial（non-blocking） | TECH_QUALITY | NOT_AUTHORIZED | polish | prereq:表现层工作令 | 低/低/低/低/低/低 | 不改 gameplay lifetime（契约边界） |
| BL-031 | ART_BIBLE 三色定稿（按技能配色+贴图+动效） | 导演门控表（裁定=占位） | Presentation / Partial（临时色） | DIRECTOR_GATED | DEFERRED | presentation | prereq:技能表现/皮肤轮立项 | 低/低/低/中/低/低 | 导演原话：现阶段颜色随意，归技能表现/皮肤轮 |
| BL-032 | 性能合同/锁定硬件变更流程 | 契约边界（其它机器须显式更新 contract+重建 baseline） | Perf-Gate / Supported（合同锁定有效） | TECH_QUALITY | NOT_AUTHORIZED | tooling（间接） | prereq:条件触发（硬件/合同变更） | 中/中/低/高/低/中 | 当前无需动作；记录触发条件防误触 |

## 2. 依赖图（AC-05）

```
【工具/基建层】
BL-024 PoEDB Pipeline (FORBIDDEN_UNTIL_APPROVED)
  ├─ blocking prerequisite of → BL-003（Passive Tree 镜像）
  ├─ blocking prerequisite of → BL-023（全量 Skill/Support 导入）
  └─ down → BL-002（若做 Tier/生成规则 canonical 化）

【长期规划 Phase 8 链】（语义保留；非已批准执行顺序）
BL-001 完整装备槽 → BL-002 更多 Affix → BL-003 完整 Passive Tree → BL-004 Aura/Reservation
  → BL-005 Curse → BL-006 Flask → BL-007 更多 Defense → BL-008 更完整 Ailment
  → BL-009 高级 Trigger → BL-010 Unique → BL-011 Jewel

【Phase 9/10】
BL-012 Skill Gem 高级成长（prereq=SkillInstance 稳定 ✓ 已满足） → BL-013 Craft Deepening
BL-013 → BL-014 Craft Simulator

【Phase 11 Endgame】
BL-015 Map Tier/Progression ←（endgame prerequisite 链：Phase 8-10 族先行）
BL-016 Boss（与 BL-015 同族）；BL-017 Map Risk Simulator ← BL-015；BL-019 Atlas ← BL-015

【Phase 12 内容工厂】
BL-018 ← BL-004/BL-008/BL-009/BL-010/BL-016 等（核心机制稳定后）

【独立/低耦合】
BL-021 ⊥ BL-022（互不依赖，产品选择）
BL-026 Voice（仅等导演输入）；BL-030/BL-031/BL-032（表现/合同小项）
BL-025 存档（独立产品语义决策）；BL-027 ← BL-003；BL-028 ← BL-001；BL-029 = 授权门槛记录
```

**Dependency Cycles Found = 0**（全图 DAG；BL-013↔BL-014 为语义前置非循环依赖：sim 依赖 craft 语义，craft 平衡消费 sim 输出，非同一令内循环）。

## 3. 导演输入派生表（AC-12：不读 runtime 代码即可回答）

| 导演问题 | 由本 backlog 直接可答 |
|---|---|
| 哪些候选技术上最近（复用现有 S4 capability） | BL-002（现有 Stat/ModOp 即可扩）、BL-021/BL-022、BL-030、BL-031——均低 blast radius |
| 哪些必须先新增 capability | BL-001（Weapon/Handedness/Requirement）、BL-004（Mana Reservation）、BL-007/008/009（新战斗机制）、BL-012/013（新数据模型）、BL-023/024（canonical pipeline） |
| 哪些属于真正 endgame | BL-015/016/017/019（+BL-018 后期阶段） |
| 哪些是 breadth | BL-001/002/023/028 |
| 哪些是 depth | BL-003/004/007/008/009/012/013/021/022/025 |
| 哪些仍被 Director Gate 阻塞 | BL-005/006/010/011/015/016/019/020/025/027/028/029/031（DEFERRED）+ BL-026（BLOCKED）+ BL-024（FORBIDDEN_UNTIL_APPROVED） |

## 4. 来源覆盖核对（AC-01）

- 长期规划 Phase 8 ①-⑪ → BL-001…BL-011 ✓；Phase 9 → BL-012 ✓；Phase 10 → BL-013/014 ✓；Phase 11 → BL-015/016/017 ✓；Phase 12 → BL-018 ✓。
- Stage0 锁 → 大天赋树=BL-003、Unique library=BL-010、Atlas=BL-019、deep Craft=BL-013、DOTS/HDRP/FMOD/捏脸=BL-020 ✓。
- S4R Director-Gated Inputs（11 问）→ 分别映射 BL-015/016/019/028/029/026/010 等 ✓（每问的默认 NO 与对应项授权状态一致）。
- Ledger Partial/Unsupported/Blocked → Socket=BL-021、Map=BL-015、Enemy=BL-016、Aura=BL-004、Weapon=BL-001、Mastery/Ascendancy=BL-003（大树镜像族内含，其 Mastery/Ascendancy 子语义不另立 ID 以满足 AC-02 去重——备注于 BL-003）、Persistence=BL-025、Canonical Pipeline=BL-024、Voice=BL-026 ✓。
- Matrix 未支持 mechanics → Support Level/Quality=BL-022、全量导入=BL-023、Craft Deepening=BL-013、Craft Sim=BL-014、Map Tier=BL-015、Map Risk Sim=BL-017、孔色=BL-021、10 槽/Weapon 族=BL-001 ✓。
- Voice GATED → BL-026 ✓。
- 已知技术/质量债 → BL-030（Death 截断）、BL-032（合同变更流程）；**Harness 代表性缺口不构成 backlog item**（已被 DECISIONS 规则⑭以「独立 Art Gate」方案正式关闭，属已解决事实非未来事项）；**狼/蝙蝠选模否决存档不构成**（已关闭裁定，非待办）。
- 已支持能力的 breadth/depth 后续 → BL-001/002/021/022/023/028 ✓。

## 5. 不矛盾声明（AC-04）

全部 32 项逐一核对：无任何项同时处于「Forbidden/Gated/Blocked」与「Ready/Active/Approved/NEXT IMPLEMENTATION」表述；依赖图中的「先行关系」均为 prerequisite 语义，不含执行承诺；长期规划顺序仅作为依赖语义保留（每处均标注「非已批准执行顺序」）。
