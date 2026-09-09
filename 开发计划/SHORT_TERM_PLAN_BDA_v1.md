# SHORT_TERM_PLAN_BDA_v1

> GAME-ZZZ — Build Diversity Alpha  
> 本文件用于指导“规划 AI”持续安排“工作 AI”执行工作。  
> 本文件是当前短期执行周期的规范，不是工作 AI 的单次任务清单。

## 0. 状态头

- **Status:** APPROVED / READY FOR REPOSITORY ADOPTION
- **Cycle ID:** BDA-1
- **Cycle Name:** Build Diversity Alpha
- **Canonical POE Baseline:** Path of Exile 1 — 3.28.0
- **Canonical External Data Source:** PoEDB only
- **Director Decision Record:** `GAME-ZZZ_short_term_planning_QA.md`
- **Long-Term North Star:** 仓库长期规划 v1
- **Entry Commit / Baseline:** 由 Phase 1 工作 AI 在阶段开始时记录
- **Current Phase:** 由执行证据追踪；正式阶段状态字段仅在最终 Director Gate 后统一更新
- **Final Human Gate:** 仅在 Phase 6 最终收口时进行；失败则进入 Correction Loop

---

# 1. 本周期的唯一核心目标

本周期不再验证“Micro ARPG Loop 能否存在”。

本周期要证明：

> **当前已经成立的 Micro ARPG 基础，能否通过装备、Affix、Socket/Link、Passive Tree、Ascendancy、Support 与 Aura/Reservation，形成至少 4 个具有明确结构差异的 Reference Builds，并让这些构筑能够被规划 AI / 工作 AI 持续、可重复、可审计地扩张。**

本周期的重点是 **Build Diversity + Production-Ready System Breadth**，不是：
- 大量 Active Skill 内容生产；
- Campaign；
- Endgame / Atlas；
- 深度 Craft；
- Jewel 完整系统；
- Curse / Flask；
- 性能专项冲刺；
- 完整职业成长流程。

---

# 2. Director 已锁定的产品规则

以下内容为本周期的硬约束。规划 AI 不得自行更改。

## 2.1 Passive Tree

1. 以 **POE1 3.28.0** 为固定 Canonical Snapshot。
2. 外部数据来源只允许 **PoEDB**。
3. Passive Tree 采用完整镜像方向：
   - 拓扑；
   - 数值；
   - 节点功能；
   - 节点名称与说明；
   - 相关表现内容；
   - Mastery；
   - Jewel Socket；
   - Ascendancy 数据。
4. 当前角色使用 **Scion 起点**。
5. Build Diversity Alpha 默认提供：
   - **50 Passive Points**
   - 免费无限退点 / 整树重置
6. 50 点只服务 Alpha 验证，不定义正式游戏的角色成长曲线。
7. 正常玩家分配必须遵守连通规则；自动测试和开发工具可以直接注入 Build Definition。
8. 未实装节点：
   - 继续显示；
   - 允许分配；
   - 保持路径连续性；
   - 已支持效果生效；
   - 未支持效果明确标记“未实装 / 无效果”；
   - 禁止静默失效。
9. Mastery：
   - 结构与 UI 完整存在；
   - 已支持效果生效；
   - 其余效果显式 Unsupported。
10. Jewel Socket：
    - 保留；
    - 可分配；
    - 本周期 Jewel 功能未实装；
    - 不作为阻塞项。

## 2.2 Ascendancy

1. 全量镜像 POE1 3.28.0 Ascendancy 数据与树结构。
2. 当前角色仅开放 **Ascendant**。
3. 其他 Ascendancy 作为 dormant data。
4. Alpha 默认提供 **8 Ascendancy Points**。
5. 免费无限重置。
6. Ascendant 未支持效果与 Passive 相同：
   - 可见；
   - 可分配；
   - 明确标记未实装。
7. 4 个 Reference Builds 中至少出现 **2 套不同 Ascendant 分配方案**。

## 2.3 Equipment

本周期补齐 10 个核心装备槽：

1. Weapon
2. Offhand
3. Helmet
4. Body
5. Gloves
6. Boots
7. Belt
8. Amulet
9. Ring1
10. Ring2

本周期必须建立：

- Weapon Family
- Handedness
- Weapon / Skill Requirement
- 双手武器与 Offhand 占用规则
- Item Slot Applicability
- 代表性 Base Item 集合
- 装备与技能实际联调

本周期不要求 Weapon Swap 成为硬性范围。

Base Item 不全量搬运 POE 内容，只实现足够覆盖：
- 10 个装备槽；
- 当前 Active Skill；
- 4 个 Reference Builds；
- 武器/装备规则验证

的代表性集合。

## 2.4 Affix

本周期至少 **40 条 Affix**。

完成标准不是“达到 40 条”，而是 Build Coverage Matrix 必须证明每个 Reference Build 都拥有明确的：

- 进攻装备追求；
- 防御装备追求；
- 资源 / 效用装备追求。

Affix 系统正式包含：

- Prefix / Suffix
- Tier
- Mod Group / Mutual Exclusion
- Item Slot Applicability
- Item Level gating
- Tier 规则
- Spawn Weight / Weighting

Item Level / Tier / Weight 生成规则以 POE1 3.28.0 / PoEDB 为高保真基准。

**规则完整不等于 Affix 内容池全量。**
本周期只导入并启用至少 40 条代表性 Affix；其数值与生成规则必须遵守 Canonical Data。

## 2.5 Active Skill

1. 全量导入 POE1 3.28.0 Active Skill Gem 的 Canonical Data：
   - Tag
   - Requirement
   - Level 数据
   - Quality 数据
   - 描述与必要元数据
2. Runtime 不全量实现。
3. 默认继续使用现有 Active Skill。
4. 只有 Reference Build Coverage Gap 明确证明现有技能无法表达批准目标时，才允许增加 Runtime Active Skill。
5. 本周期新增 Active Skill 上限：**最多 2 个**。
6. 其余 Active Skill 作为 dormant / Unsupported data。

## 2.6 Support Gem

1. 全量导入 POE1 3.28.0 Support Gem Canonical Data。
2. 完整导入：
   - Tag
   - Requirement
   - Compatibility
   - Level 1–20
   - Quality 0–20
   - 描述与必要元数据
3. Runtime 采用渐进实装：
   - 当前系统可承载、Reference Build 优先需要的 Support 正常实装；
   - 其余标记 Partial / Unsupported。
4. Support 数据存在不得自动触发未来系统扩张。
5. 只有：
   - 已属于当前批准范围；
   - 或明显填补 4 个 Reference Builds 的 Coverage Gap 且不违反边界
   的 Support 依赖机制，才允许进入 Runtime。
6. 建立独立 **Support Mechanic Support Matrix**。
7. 建立完整数据驱动 **Skill Tag / Support Compatibility** 模型。
8. 禁止以手写白名单作为正式兼容架构。
9. 建立 Active × Support Compatibility Matrix 自动回归测试。

## 2.7 Socket / Link

本周期正式引入基础 Socket / Link：

- Active 与 Support 必须位于同一 Linked Group 才发生支持关系；
- 装备承载 Socket Group；
- Base Item / 装备类型声明 Max Sockets / Max Links；
- 当前代表性 Base Item 按 POE1 3.28.0 对应容量规则配置；
- Validation Scenario 可直接生成合法 Socket / Link。

本周期 **不启用 Socket Color 兼容限制**。

Gem Color / Attribute Color：
- 继续作为 Canonical dormant metadata 导入；
- UI 可显示；
- Gameplay Compatibility 不检查颜色。

本周期不进入：
- Chromatic；
- Jeweller；
- Fusing；
- 随机打孔 / 连线经济。

## 2.8 Aura / Reservation

首批固定 4 个 Aura：

- Pride
- Anger
- Determination
- Grace

按 POE1 3.28.0 Canonical Data 高保真实现。

Aura / Reservation 第一版必须支持：

- Maximum Mana
- Current Mana
- Reserved Mana
- Reservation %
- Reservation Efficiency
- Aura Effect
- 多 Aura 同时启用
- 通用 Aura Emitter / Receiver
- Self / Ally / Enemy 目标语义

本周期不自动进入：
- Life Reservation
- Blood Magic
- 完整资源替代体系
- Minion / Ally 内容生产

## 2.9 Craft

本周期不进入深度 Craft。

要求：
- 现有基础 Craft 不因新装备槽 / Affix 回归；
- Reference Build 可通过确定性测试装备、Build Definition 或 Loadout 工具直接获得目标装备；
- 不要求玩家通过随机 Loot + Craft 自然做出 4 个 Reference Builds。

## 2.10 Performance

本周期 **性能不是 Hard Gate**。

允许：
- 持续测量；
- 记录回归；
- 形成 Technical Debt / Non-Blocking 工作；
- 保留既有性能证据。

不得：
- 因未达到既有 1440p / 120 FPS 目标而冻结 Build Diversity 主线。

未来进入专门性能周期时再恢复性能硬 Gate。

---

# 3. Reference Build 验收模型

规划 AI 必须在早期 Runtime / Skill / Support / Combat Math 审计后，自动提出并锁定 **4 个 Reference Build Targets**。

无需中途请求 Director。

最终 4 个 Reference Builds 必须：

1. 至少 4 个；
2. 每个至少在 **3 个实质维度**上与其他 Build 不同；
3. 至少覆盖 2 套不同 Ascendant 分配；
4. 实际依赖的关键路径必须全部 Supported；
5. 能被 Build Definition 稳定重建；
6. 能被自动回归验证技术正确性。

可作为“实质维度”的项目包括：

- 主伤害机制；
- Skill 行为；
- Passive 路线；
- Mastery / Ascendant 选择；
- Equipment / Base Item；
- Affix 优先级；
- Support 组合；
- Aura / Reservation；
- 防御方向；
- 资源取舍；
- Weapon / Handedness。

本周期 **不要求**：
- 4 个 Build DPS 接近；
- 统一生存强度；
- 固定 Seed 通关门槛；
- 数值平衡完成。

自动验证只负责证明：
- Build Definition 可加载；
- 分配合法；
- 配置有效；
- Runtime 不产生异常 / NaN / 非法状态；
- 已标记 Supported 的关键机制确实生效。

“是否有趣、是否形成真实取舍、是否值得继续扩张”由最终 Director Gate 判断。

---

# 4. Capability / Mechanic 管理

规划 AI 必须要求工作 AI 维护以下结构：

## 4.1 Capability Ledger

统一记录当前项目“已经能表达哪些高层构筑能力”。

至少覆盖：

- Damage Types
- Conversion
- Crit
- Accuracy / Evasion
- Armour
- Aura / Reservation
- Weapon / Handedness
- Socket / Link
- Skill Tag
- Support Compatibility
- Passive
- Mastery
- Ascendancy
- Equipment / Affix
- 其他本周期关键机制

状态至少使用：

- Supported
- Partial
- Unsupported
- Blocked

## 4.2 Passive Mechanic Support Matrix

按机制族追踪：
- 状态；
- 依赖系统；
- 测试；
- 影响节点；
- 可启用节点；
- Reference Build Coverage；
- Blocked 原因；
- Resume Trigger。

未支持 Passive 的实现优先级由 **Reference Build Coverage Gap** 驱动。

## 4.3 Support Mechanic Support Matrix

同样记录：
- 状态；
- 受影响 Support；
- 依赖；
- 自动测试；
- Reference Build Coverage；
- Resume Trigger。

不得因为“数据已经导入”机械追求全量 Runtime 支持。

## 4.4 Supported 百分比

本周期不设置全局支持率 KPI。

最终硬要求是：

> 4 个 Reference Builds 的关键依赖路径必须 100% Supported。

外围 Canonical Data 可继续 Partial / Unsupported / dormant。

---

# 5. Canonical Data 协议

## 5.1 唯一外部来源

本周期外部 Canonical Data **只允许 PoEDB**。

禁止规划 AI / 工作 AI：
- 使用 GGG Patch Notes 补 Canonical 缺口；
- 使用 Path of Building 补 Canonical 缺口；
- 使用其他社区数据静默混源；
- 根据常识猜测缺失数值。

每个 Canonical Snapshot 至少记录：

- POE version；
- PoEDB 对应来源；
- 抓取 / 导入时间；
- Importer / Parser 版本；
- Data Schema 版本；
- 内容哈希；
- Import Report；
- Data Gap；
- 异常 / 冲突。

## 5.2 PoEDB 数据缺口

若缺口影响：
- 4 个 Reference Builds；
- Canonical Schema；
- 当前阶段 Hard Gate；

则该依赖标记 **Blocked**。

否则：
- 记录 Data Gap；
- 相关内容保持 dormant / Unsupported；
- 继续 Primary 或 Non-Blocking 工作；
- 禁止切换数据源补齐。

## 5.3 Runtime 依赖

Runtime 只读取仓库内版本化 Canonical Snapshot。

禁止：
- Runtime 在线抓取 PoEDB；
- Build 时临时抓取外部数据；
- 每次运行重新生成不同 Canonical Data。

---

# 6. 规划 AI 的 Source of Truth

遇到冲突时，按以下顺序判断：

1. **较新的明确 Director Decision**
2. **本短期规划**
3. `DECISIONS.md`
4. 当前可运行 Runtime + 自动测试证据
5. `STATUS.md` / `ROADMAP.md` 当前摘要
6. 历史 Phase Snapshot / 历史阶段文档
7. 长期规划中的旧执行细节

长期规划仍然是产品 North Star，但不能覆盖本周期已经明确锁定的执行规则。

出现 Drift 时：
- 记录冲突；
- 要求工作 AI 修正文档；
- 不得静默改产品语义。

---

# 7. 规划 AI 的权限

规划 AI可以自主：

- 拆阶段；
- 排工作令；
- 在阶段内重排；
- 建立测试；
- 建立 Validator；
- 建立 Import Audit；
- 建立 Snapshot Diff；
- 建立数据审计工具；
- 建立 Smoke / Regression 工具；
- 建立必要编辑器辅助工具；
- 批准有限、必要架构重构；
- 依据 Coverage Gap 调整机制实现优先级；
- 在 Gate 间基于证据重排 Reference Build；
- 完成当前周期后自动生成下一周期短期计划。

规划 AI不得自主：

- 改变 Director 已锁定产品规则；
- 通过 Non-Blocking 偷跑未来核心系统；
- 因一个 Unsupported 数据项自动启动完整新系统；
- 无证据频繁替换 Reference Build；
- 静默混用外部 Canonical Source；
- 无必要新增第二套平行 Runtime 架构；
- 随意大改核心程序集边界；
- 替换已锁定技术路线；
- 把性能回归变成本周期全局阻塞；
- 自报工作 AI 完成即自动关闭工作令。

若出现根本 Runtime 架构方向变化、核心程序集边界大改或替换已锁定技术路线，必须请求 Director。

---

# 8. 中途何时允许请求 Director

本周期只有以下情况允许中途打断 Director：

1. 两个已确认 Director 决策发生无法自行消解的冲突；
2. 新问题必然改变核心产品语义；
3. Primary Path 与所有合法 Non-Blocking Tasks 被同一依赖彻底阻塞，形成全局死锁；
4. 跨周期候选系统会引入此前未批准的重大产品语义。

普通情况不得中断：
- 编译错误；
- 测试失败；
- 数据缺口；
- 局部实现困难；
- 局部重构；
- 可恢复阻塞；
- 文档 Drift；
- 单个 Unsupported 机制。

---

# 9. Work Order 协议

任意时刻只允许 **1 个 Active Primary Work Order**。

工作令必须使用固定模板。

## 9.1 强制字段

每个 Work Order 至少包含：

1. **Objective**
2. **Why Now**
3. **In Scope**
4. **Out of Scope**
5. **Inputs / Source of Truth**
6. **Expected Files / Systems**
7. **Acceptance Criteria**
8. **Tests / Gates**
9. **Primary Path**
10. **Non-Blocking Task 1**
11. **Non-Blocking Task 2**
12. **Blocked Resume Trigger**
13. **Evidence Required**
14. **Docs / Matrix Updates**
15. **Forbidden Expansion**
16. **Completion Report Format**

原则上一个 Work Order：
- 只有 1 个主要功能目标；
- 不跨超过 2 个核心系统。

## 9.2 Non-Blocking 强制规则

每个 Work Order 至少提供 2 个 Non-Blocking Tasks。

允许的 Non-Blocking 类型：

- 当前短期范围内的独立功能；
- 测试；
- Validator；
- Import Audit；
- Snapshot Diff；
- 数据清理；
- 文档；
- 技术债；
- 编辑器工具；
- 已明确允许的“只预研、不落 Runtime”项目。

禁止：
- Curse；
- Flask；
- Jewel Runtime；
- Atlas；
- 未批准职业系统；
- 其他未来核心 Gameplay
被当作“顺手非阻塞任务”提前实现。

主任务阻塞后：
1. 记录 Blocked 原因；
2. 保存证据；
3. 写明 Resume Trigger；
4. 自动切到 Non-Blocking；
5. Trigger 满足后重新拉回 Primary。

---

# 10. 工作 AI 完成与规划 AI 验收

## 10.1 工作 AI Self-Review

工作 AI 必须先完成：

- 编译；
- 自动测试；
- 数据验证；
- 自审；
- 文档更新；
- Evidence Pack。

## 10.2 固定 Evidence Pack

至少包含：

- 变更摘要；
- 涉及文件 / 系统；
- 自动测试结果；
- 关键运行证据 / 截图（适用时）；
- Data / Schema Diff；
- Capability / Matrix 变化；
- 已知限制；
- Blocked 项；
- Non-Blocking 完成情况；
- Git 变更标识；
- 与阶段 Baseline 的关系。

## 10.3 规划 AI 独立复核

规划 AI必须：
- 对照 Acceptance Criteria；
- 检查测试证据；
- 检查 Capability / Matrix；
- 检查文档；
- 检查 Scope；
- 检查是否偷跑未来系统；
- 检查是否产生新回归。

规划 AI不写正常 Gameplay 功能代码，但可以：
- 判定未完成；
- 发回有限修正；
- 要求补证据；
- 要求补文档；
- 要求消除 Scope Drift。

---

# 11. 文档责任

治理文档由 **工作 AI 实际维护和落盘**。

凡 Work Order 改变：

- Runtime；
- Canonical Data；
- Capability；
- Mechanic Matrix；
- 阻塞事实；
- Director 决策映射；
- 阶段事实；

对应文档更新必须写入 Acceptance Criteria。

**缺少必要文档 = Work Order 未完成。**

纯内部重构且不改变外部事实时，可不更新高层文档。

规划 AI的责任是：
- 审核文档是否正确；
- 在阶段 Reconciliation 时拒绝不一致状态；
- 要求工作 AI 修复。

---

# 12. 阶段 Baseline / 回归协议

不要求每个 Work Order 建立独立回滚点。

每个阶段开始时必须建立 **Stage Baseline Snapshot**，至少包含：

- Git commit / tag；
- 全量当前自动测试结果；
- Capability Ledger；
- 各 Mechanic Matrix；
- Canonical Data 版本 / 哈希；
- 已知失败；
- 当前 Blocked 项；
- 适用时性能记录。

## 12.1 Baseline Diff Gate

允许 Baseline 已知失败继续存在，但必须明确记录。

以下情况视为本阶段新回归：

- 原本绿色测试变红；
- 新增异常失败；
- Canonical Snapshot 与 Runtime 不一致；
- Mechanic Matrix 与 Runtime 不一致；
- 原有 Supported 能力退回 Partial / Unsupported；
- 新阶段改动破坏旧功能。

新回归：
- 阻塞相关 Primary Path；
- 无关 Non-Blocking Tasks 继续执行。

---

# 13. 阶段级 Repository Reconciliation

完整 Reconciliation **只在阶段结束时强制进行**，不要求每个 Work Order 全量收口。

阶段结束前必须统一核对：

- Runtime；
- 自动测试；
- Canonical Data；
- Capability Ledger；
- Passive / Support 等 Mechanic Matrix；
- DECISIONS；
- STATUS / ROADMAP 当前事实；
- Blocked / Non-Blocking；
- 当前阶段证据。

未通过 Reconciliation：
- 不得进入下一阶段。

每个阶段结束后生成 **Phase Snapshot**，至少记录：

- 已完成能力；
- Partial / Unsupported / Blocked；
- 关键决策；
- Canonical Data 版本；
- 测试状态；
- Capability Ledger 摘要；
- Matrix 摘要；
- Reference Build 影响；
- 技术债 / 阻塞；
- 下一阶段 Entry 条件。

`STATUS.md` 应保持当前摘要，不继续堆积完整历史正文。

---

# 14. 正式阶段序列

---

## Phase 1 — Entry / Canonical & Capability Foundation

### Objective

把当前仓库状态切换成 Build Diversity Alpha 的稳定执行基线，并建立之后所有阶段依赖的数据与能力事实层。

### Scope

1. 建立 Stage Baseline Snapshot。
2. 审计当前 Runtime / Tests / S4 剩余有效工作。
3. 只吸收仍然对：
   - Build Diversity；
   - Production Scale；
   - 数据正确性；
   - 测试可信度
   有价值的未完成事项。
4. 建立 PoEDB-only Canonical Import Pipeline。
5. 建立版本化 Canonical Snapshot：
   - Passive；
   - Ascendancy；
   - Active Skill；
   - Support Gem；
   - Affix generation rules。
6. 建立 Capability Ledger。
7. 建立 Passive Mechanic Support Matrix。
8. 建立 Support Mechanic Support Matrix。
9. 审计现有 Skill / Support / Combat Math。
10. 规划 AI 基于审计结果锁定 4 个 **Reference Build Targets**。

### Hard Gate

- Stage Baseline 完整；
- Canonical Data 可重复生成；
- Runtime 不依赖在线数据；
- PoEDB 来源与哈希可追溯；
- Data Gap 被显式记录；
- Capability Ledger 建立；
- Passive / Support Matrix 建立；
- 4 个 Reference Build Targets 已明确；
- 无新 Baseline regression。

### Non-Blocking Pool

- Import Audit；
- Snapshot Diff；
- Schema Validator；
- 数据清理；
- STATUS / Matrix 整理；
- 现有测试补强。

### Exit

完成 Phase Snapshot + Repository Reconciliation。

---

## Phase 2 — Equipment / Affix / Base Item / Socket-Link

### Objective

让装备真正成为 Build Diversity 的主要构筑载体，而不是简单数值容器。

### Scope

1. 补齐 10 个装备槽。
2. Weapon Family / Handedness / Requirement。
3. 双手武器 / Offhand 占用。
4. 代表性 Base Item。
5. Prefix / Suffix。
6. Tier。
7. Mod Group / Mutual Exclusion。
8. Item Slot Applicability。
9. Item Level / Tier / Weight 生成规则。
10. 至少 40 条 Affix。
11. Build Coverage Matrix。
12. Socket / Link 基础。
13. Max Sockets / Max Links。
14. 与当前 Skill / Support 联调。
15. 现有 Craft 回归保护。

### Hard Gate

- 10 槽可用；
- 装备合法性校验通过；
- Weapon / Skill Requirement 生效；
- 至少 40 Affix；
- Reference Build Targets 均拥有进攻 / 防御 / 资源或效用装备追求；
- Affix 生成遵守 Canonical rules；
- Socket / Link 可构造合法配置；
- 无 Socket Color Compatibility；
- Gem Color metadata 保留；
- 现有 Craft 无新回归；
- 无 Baseline regression。

### Non-Blocking Pool

- Affix Validator；
- Base Item Audit；
- 装备测试数据生成器；
- Loadout 工具骨架；
- UI 数据展示；
- 文档 / Matrix 整理。

### Exit

完成 Phase Snapshot + Repository Reconciliation。

---

## Phase 3 — Passive Tree / Mastery / Ascendancy

### Objective

把 POE1 3.28.0 Passive / Ascendancy 结构变成可实际用于构筑验证的 Runtime 与 UI。

### Scope

1. 完整 Passive Tree Canonical Snapshot。
2. Scion 起点。
3. 50 Passive Points。
4. 免费无限洗点。
5. 连通规则。
6. Passive UI：
   - 完整显示；
   - 缩放 / 平移；
   - 节点详情；
   - 状态标识；
   - 路径高亮；
   - 搜索；
   - 剩余点数；
   - 单点退点；
   - 整树重置。
7. Mastery。
8. Jewel Socket 以 Unsupported 方式保留。
9. 全量 Ascendancy data。
10. 当前开放 Ascendant。
11. 8 Ascendancy Points。
12. Ascendant 免费无限重置。
13. 按 Reference Build Coverage Gap 实装关键 Passive / Mastery / Ascendant 机制。

### Hard Gate

- Canonical Tree 可稳定加载；
- Scion 起点有效；
- 正常分配严格连通；
- 测试工具可直接注入 Build Definition；
- 50 / 8 点预算可用；
- 免费重置可用；
- Unsupported 节点无静默失效；
- Mastery 状态正确；
- Jewel Socket 可分配但明确 Unsupported；
- Reference Build Targets 所需关键 Passive / Ascendant 路线达到 Supported；
- 无 Baseline regression。

### Non-Blocking Pool

- Tree search/index；
- Passive importer audit；
- Matrix 自动生成；
- UI 可用性修正；
- 额外 Supported 机制（仅 Coverage Gap 驱动）；
- 文档整理。

### Exit

完成 Phase Snapshot + Repository Reconciliation。

---

## Phase 4 — Support / Skill Compatibility / Socket Integration

### Objective

让全量 Support Canonical Data 与当前 Runtime 形成可扩展、可测试、非手工白名单的技能改造体系。

### Scope

1. Active Canonical Data 全量导入。
2. Support Canonical Data 全量导入。
3. Support Level 1–20。
4. Quality 0–20。
5. Data-driven Skill Tag model。
6. Support Compatibility rules。
7. Active × Support Compatibility Matrix。
8. Support Mechanic Support Matrix。
9. Socket / Link 与 Active / Support 真正联动。
10. Validation Gem Library / Loadout。
11. 必要时最多新增 2 个 Runtime Active Skill。
12. Reference Build Targets 关键 Support 机制优先 Supported。

### Hard Gate

- Compatibility 不依赖长期白名单；
- Compatibility Matrix 自动测试可重跑；
- Tag / Rule 改动会触发回归；
- Gem Library 可设置 Level / Quality；
- Linked Group 正确控制 Support 生效；
- Reference Build Targets 所需 Support 全部 Supported；
- 其他 Support 明确 Partial / Unsupported；
- 不因未支持 Support 自动启动未来系统；
- 无 Baseline regression。

### Non-Blocking Pool

- Compatibility snapshot diff；
- Support importer audit；
- Tag validator；
- Gem Library UX；
- 支持状态 UI；
- 文档 / Matrix。

### Exit

完成 Phase Snapshot + Repository Reconciliation。

---

## Phase 5 — Aura / Reservation

### Objective

引入本周期第一个新增核心构筑系统，并让进攻、防御与 Mana Reservation 形成真实取舍。

### Scope

固定实现：

- Pride
- Anger
- Determination
- Grace

实现：

- Maximum Mana
- Current Mana
- Reserved Mana
- Reservation %
- Reservation Efficiency
- Aura Effect
- 多 Aura
- Aura Emitter / Receiver
- Self / Ally / Enemy
- 与 Passive / Ascendant / Equipment / Affix 联动
- 与 4 个 Reference Build Targets 联动

### Hard Gate

- 4 Aura 均可实际使用；
- Reservation 正确影响可用 Mana；
- Reservation Efficiency 生效；
- Aura Effect 生效；
- Pride 等 Enemy Aura 不被错误简化为 Self Buff；
- Self / Ally / Enemy 目标模型稳定；
- Reference Build Targets 可产生不同 Aura 选择；
- 不自动进入 Life Reservation / Blood Magic；
- 无 Baseline regression。

### Non-Blocking Pool

- Aura debug UI；
- Reservation visualizer；
- Aura target tests；
- Aura matrix；
- 数据审计；
- 文档更新。

### Exit

完成 Phase Snapshot + Repository Reconciliation。

---

## Phase 6 — Reference Builds / Validation Scenario / Director Gate

### Objective

把前 5 阶段的系统组合成一个可重复、可对比、可回归的 Build Diversity 实验环境，并交给 Director 做唯一正式体验验收。

### Scope

1. 最终锁定 4 个 Reference Builds。
2. 每个 Build 至少 3 个实质构筑差异。
3. 至少 2 套 Ascendant 分配。
4. 关键依赖全部 Supported。
5. 建立版本化 Build Definition。
6. 建立一键 Loadout。
7. Build Loadout 面板支持：
   - Equipment；
   - deterministic Affix；
   - Passive；
   - Ascendancy；
   - Active；
   - Support；
   - Level / Quality；
   - Socket / Link；
   - Aura。
8. 建立统一 Build Diversity Validation Scenario。
9. 同一标准战斗场景体验 4 个 Build。
10. 建立自动技术回归。
11. 不要求固定战斗强度 / DPS 平衡。

### Hard Gate

在进入 Director Gate 前：

- 4 Build Definition 全部可重复加载；
- 4 Build 关键依赖路径 100% Supported；
- 至少 2 套 Ascendant；
- 每个 Build 至少 3 个实质差异维度；
- Loadout 可一键重建；
- Validation Scenario 可稳定进入；
- 自动技术回归通过；
- 无新 Baseline regression；
- Phase 1–5 的 Reconciliation 已完成；
- 本周期文档与事实一致。

### Final Director Gate

Director 只在这里进行正式体验判断。

Director 判断：

- 4 个 Build 是否真的不同；
- 装备选择是否有意义；
- Passive 路线是否形成路径成本；
- Ascendancy 是否参与构筑；
- Support 是否改变 Skill 行为；
- Aura 是否形成资源取舍；
- 整体是否值得继续扩张。

---

# 15. Correction Loop

如果 Final Director Gate 未通过：

1. Director 给出问题清单；
2. 规划 AI 不废止本计划；
3. 自动创建边界明确的 Correction Phase；
4. 把反馈转成有限 Work Orders；
5. 继续使用：
   - 单 Primary；
   - Non-Blocking；
   - Evidence Pack；
   - Baseline Diff；
   - Reconciliation；
6. 修正完成后再次进入 Final Director Gate；
7. 循环直到 Director 明确通过。

规划 AI不得忽略 Director 的正式 Gate 反馈。

---

# 16. 正式阶段状态更新规则

阶段内部执行进度通过以下证据追踪：

- Work Order；
- Evidence Pack；
- Blocked Record；
- Phase Snapshot；
- Reconciliation。

**正式阶段状态字段只在最终 Director Gate 后统一更新。**

规划 AI 不直接以日常验收修改正式阶段状态。

由工作 AI 按最终 Director Gate 结果统一更新对应治理文件，规划 AI负责复核。

---

# 17. 本周期通过后的自动滚动规则

当：

- Phase 1–6 全部实际完成；
- Final Director Gate 通过；
- Correction Loop 关闭；

规划 AI 自动进入“下一周期生成”。

无需等待 Director 预审批。

## 17.1 下一周期输入

规划 AI必须读取：

- 长期规划；
- 最新 Capability Ledger；
- Backlog；
- 技术债；
- Unsupported / Partial；
- Phase Snapshots；
- Runtime / Tests；
- 当前风险；
- 玩家价值；
- 系统依赖。

## 17.2 下一周期规划

规划 AI必须自动生成新的版本化短期规划，例如：

`SHORT_TERM_PLAN_<CYCLE>_v1.md`

新周期：
- 自动继承本文件的通用治理协议；
- 可以证据驱动重排长期规划的原始顺序；
- 必须记录为什么偏离长期规划；
- 不得跳过硬依赖。

## 17.3 何时必须找 Director

如果下一周期只是长期规划中已经批准的系统：
- 自动生成；
- 自动执行。

如果候选周期会引入此前未批准的重大产品语义，例如：
- 根本战斗模式变化；
- 联网；
- 商业化经济；
- 根本职业结构变化；
- 其他会改变产品定义的系统；

必须先请求 Director。

---

# 18. 风险记录

## 18.1 POE 高保真镜像

Director 已明确选择：
- Passive Tree 完整镜像；
- Ascendancy 完整数据镜像；
- Active / Support Canonical Data 全量镜像；
- 部分系统高保真实现；
- 未来公开版本仍保持相关镜像方向。

该决定存在显著第三方知识产权 / 授权风险。

按照 Director 决策：
- 风险必须保留在 Risk Register；
- 风险不构成本周期 Build Diversity 的自动阻塞项；
- 规划 AI不得擅自将产品设计改为原创替代；
- 若 Director 后续重新决定，以新决策为准。

---

# 19. 规划 AI 每次开始工作的读取顺序

每次重新进入仓库，规划 AI按以下顺序恢复上下文：

1. Director Decision Record / 最新 Director 决策
2. 当前 `SHORT_TERM_PLAN_*`
3. 最新 Phase Snapshot
4. Capability Ledger
5. Passive Mechanic Support Matrix
6. Support Mechanic Support Matrix
7. DECISIONS
8. 当前 Runtime / Tests
9. STATUS / ROADMAP 当前摘要
10. 必要时回溯长期规划

不要从历史长文第一行开始机械执行。

先确定：
- 当前实际阶段；
- Active Primary Work Order；
- Blocked 项；
- Non-Blocking 项；
- Reference Build Coverage Gap；
- 当前 Baseline 与新增回归。

再下工作令。

---

# 20. 规划 AI 的最终行为准则

规划 AI必须始终遵守以下优先级：

> **先守住当前真相 → 再守住构筑目标 → 再扩能力 → 最后扩内容。**

不得把：
- 数据导入完成；
- 测试数量增加；
- 文档变多；
- 节点数量增加；
- Support 数量增加；

本身视为“游戏进展”。

本周期真正的成功标准是：

> **玩家可以在同一个验证入口，用 50 Passive + 8 Ascendancy + 10 装备槽 + ≥40 Affix + Socket/Link + Support + Pride/Anger/Determination/Grace，构造并快速切换至少 4 个结构上真正不同的 Build；这些 Build 的关键路径都已 Supported，技术状态可自动回归，最终由 Director 确认差异具有实际游戏价值。**
