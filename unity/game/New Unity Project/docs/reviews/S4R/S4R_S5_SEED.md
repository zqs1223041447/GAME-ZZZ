# S4R_S5_SEED — S5 Implementation-Cycle Seed（Phase 4 产物；SEEDED / NOT ACTIVATED）

**来源**：规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09 Direction Gate Review（导演 Form：仅选 DIR-1，其余未填 → Default-on-Omission）。**性质**：planning seed，**不是工作授权**——S5 不得开始，直到导演显式批准至少一个 Authorization Atom 并完成 S4R Final Director Gate。

## 1. Direction Gate Review 结果

- **Product Direction Gate: PASS** —— 导演已解决 S4 硬停止中「等待导演下一产品方向」：下一产品方向 = **DIR-1 Reuse-First Build & Itemization Depth**。
- **Implementation Authorization Gate: BLOCKED** —— Approved Authorization Atoms = 0（导演未逐项点名；按 F1 授权优先级，DIR-1 选择≠实现授权；合法的 partial gate outcome）。
- Cross-Cutting Gates（未答→默认）：Progression=NO / Endgame=NO / Boss=NO / Unique=NO / Ring=NO / Offhand=NO / Amulet=NO / New Content Batch=NO / Voice=KEEP DEFERRED / BL-024=KEEP FORBIDDEN。

## 2. S5 Seed

| 项 | 值 |
|---|---|
| Cycle ID | **S5** |
| Working Title | Build Identity & Itemization Depth |
| Product Direction | DIR-1（Reuse-First Build & Itemization Depth） |
| 状态 | **SEEDED / NOT ACTIVATED** |
| 激活前置 | ≥1 个 DIR-1 atom 显式批准 + S4R Final Director Gate |

**S5 Product Mission**：在不进入 Progression Spine / Endgame / 大规模内容工厂的前提下，优先复用已验证的 S4 面（3 Skills / 7 Supports / 6 槽 / 17 词缀 / 单一 applicability / Passive 基础 / Combat Math / 确定性工具链 / RC-M·RC-P·RC-A 候选覆盖），使现有 build/itemization 产生明显可区分的选择。

**Frozen S5 Exclusions（当前 Form 直接列为 OUT / NOT AUTHORIZED）**：Progression Spine；Map Tier/progression；Boss；Atlas；Unique；Ring；Offhand；Amulet；New Content Batch；Voice activation；BL-024 PoEDB Pipeline；Curse；Flask；Jewel；New Class；Deep Craft；Persistence；Mastery；Ascendancy；Aura/Reservation；未批准的新 Defense/Ailment/Trigger；Phase 12 Content Factory。只有导演新的显式决定才能改变。

**Candidate Capability Delta（当前=NONE；以下仅为候选非 scope）**：BL-002.A1 / BL-002.A2 / BL-002.A3 / BL-021.A1 / BL-021.A2 / BL-012.A1。

## 3. 待导演决定的 4 个原子（Required Director Supplemental Decision）

| 原子 | 内容 | 规划 AI 建议 |
|---|---|---|
| BL-002.A1 | **现有机制内有上限的新词缀扩展**（bounded affix breadth；不加新系统、不碰 Prefix-Suffix/Tier） | **APPROVE FIRST**（复用最大、blast radius 最低、不需要 BL-024/Progression/Endgame） |
| BL-021.A1 | 装备孔颜色（真新 socket 机制，radius 高于 BL-002.A1；不需要 BL-024/endgame） | 可独立考虑；首批非必须 |
| BL-021.A2 | 多组连接/扩展 Socket-Link 模型（数据模型/UI/测试面明显更大） | 首批不优先 |
| BL-012.A1 | 技能/辅助宝石 等级/品质（成长轴；导演当前 Progression=NO，需显式点名才覆盖默认） | 暂不作最小首批 |

规划 AI 推荐的最小首批：**仅 BL-002.A1 = APPROVE，其余三个 KEEP NOT AUTHORIZED**（仅为建议，非导演决定）。批准 BL-002.A1 ≠ 自动批准 BL-002.A2/A3、BL-029.A3（新词缀族）、Phase 12、无限制 Content Batch；新增词缀数量上限由 S5 正式计划设定。

## 4. 拟定 S5 Gate 形态（atom 批准后细化为可执行计划）

Phase 0 = Approved-scope contract lock + test oracle lock → Phase 1 = Data/domain 实现 → Phase 2 = Runtime 集成 → Phase 3 = UI/observability/audit 集成 → Phase 4 = Reference Build validation → Phase 5 = Production simulation/determinism/regression/performance gate（按最终批准 atoms 调整）。

**S5 证据标准（不因 reuse-first 降低）**：EditMode/PlayMode 回归 + Content Audit + 确定性 Production Simulation + 受影响的 support 兼容 oracle parity 与 applicability 单一真相校验 + 受影响的 Reference Build 证据 + 显式 capability/mechanic delta + 与 runtime 影响成比例的性能验证。

**Reference Build Policy**：RC-M/RC-P/RC-A 继续 CANDIDATE / NOT LOCKED；S5 正式计划可在 atom 批准后决定是否锁定；本 seed 不锁定。

## 5. 当前停机条件

导演补原子批准前：**不下发 S5-WO-01**；继续禁止 runtime change / prototype / implementation spike / canonical-data expansion / 新词缀条目 / 孔色实现 / Level-Quality 实现 / Reference Build lock。当前允许：本 Gate Review 与 S5 seed 落库；把 4 原子问题转导演；回答导演对四项区别的询问。

## 6. S4R 候选事实（正式 Phase 状态仍待 S4R Final Director Gate 统一晋级）

Phase 0 对账工作 accepted；Phase 1 正规化工作 accepted；Phase 2 决策包 accepted（含 F1）；导演产品方向输入已收到（DIR-1）；Phase 4 seed 已生成（本文件）。**S4R 整体 COMPLETE 仍不成立。**
