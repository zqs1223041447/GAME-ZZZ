# S5_WO_01_EVIDENCE — Evidence Pack（S5-WO-01 Approved Scope & Contract Lock）

**Work Order**: S5-WO-01 — Approved Scope & Contract Lock（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 S5 计划下发）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ bd4b914

## Changed Markdown（12 个文件）

| 文件 | 变更类型 |
|---|---|
| `docs/reviews/S5/S5_PLAN.md` | 新建：S5 短期计划全文 + S5-WO-01 工作令原文（唯一落库位置） |
| `docs/reviews/S5/S5_WO_01.md` | 新建：工作令指针/状态记录（全文指向 S5_PLAN 附录，避免双权威） |
| `docs/reviews/S5/S5_LINK_CONTRACT.md` | 新建：**Multi-Link 唯一权威合同**（MaxLinkGroupsPerEligibleItem=2；数据模型唯一选定；真相源指认；12 场景测试矩阵；golden 复用策略；禁区护栏） |
| `docs/reviews/S5/S5_AFFIX_ADMISSION.md` | 新建：**词缀准入清单**（Locked N=4；候选 17-20 迅疾/铁骨/睿智/开阔；Phase 3 登记事项含计数护栏更新；未入选候选记录） |
| `docs/reviews/S5/S5_SCOPE_LEDGER.md` | 新建：一页 scope 台账（APPROVED 2 / EXPLICITLY REJECTED 2 / NOT AUTHORIZED 其余） |
| `docs/DECISIONS.md` | 追加 1 条：S5 授权合同（批准/拒绝原子、max 2 连接组、词缀上限 +6 与 N=4、Phase 0 合同要点）；同轮恢复上轮被误替换的 F1 条目（见 Drift/勘误节） |
| `docs/RUNTIME.md` | **1 处勘误修正（自纠漂移）+ 其余 verify only**：S4R 轮误写「Gloves/Belt 为词缀位无 Socket」→ 修正「各 1 孔仅展示计数、不映射技能孔位」（`SliceRules.GlovesSockets/BeltSockets=1` + Tooltip「N孔」实证）；Multi-Link 不描述为已实现（合同仅标注 AUTHORIZED NOT IMPLEMENTED 于 S5 文档） |
| `docs/reviews/S4R/S4R_CAPABILITY_LEDGER.md` | Socket/Link 行：Gloves/Belt 事实修正 + **S5 授权跟踪**（Multiple Link Groups=AUTHORIZED NOT IMPLEMENTED；Socket Color/Gem Level-Quality=导演显式拒绝；状态保持 Partial 不晋升） |
| `docs/reviews/S4R/S4R_MECHANIC_MATRIX.md` | §3 表后增 S5 授权跟踪块（仅 tracking 不晋升）；修正表格被注打断的格式 |
| `docs/ROADMAP.md` | S5 行=AUTHORIZED FOR PHASE 0（scope 恰=两原子，保留全部显式排除）+ 页首摘要更新（DIR-1 已批准/S5 开工） |
| `docs/reviews/STATUS.md` | LIMITED：本轮 commit 行 + S5 门状态行更新 + 最近一次绿灯加行 |
| `docs/reviews/S4R/S4R_S5_SEED.md` | 无改动（seed 历史，已被 S5_PLAN 取代为活跃计划——保留为 Phase 4 产物记录） |

## Changed Runtime Files

**NONE**（零 .cs/prefab/scene/资产 diff——Phase 0 纯文档；AC-13 ✓）

## Director Scope

- **Approved Atoms**: BL-002.A1（bounded affix breadth）+ BL-021.A2（multiple links / expanded socket-link model）
- **Explicitly Rejected Atoms**: BL-021.A1（Socket Color）、BL-012.A1（Gem Level / Quality）——贯穿 S5 全周期的负向验收条件
- 其余全部 Default-on-Omission 维持原状态；S5 授权=恰好 {BL-002.A1, BL-021.A2}，无外溢（AC-01 ✓）

## Multi-Link 合同要点

- **Chosen Data Model**: `ItemInstance` 单字段 `LinkSkill1`（SkillId，None=0 ⇒ default 即 legacy 单组）；组划分=确定性规则派生（group 1=末尾 2 孔；group 0=其余前部孔）；Support 存储复用既有 per-skill 会话数组（零新存档字段）
- **Max Link Groups**: 2（第三组确定性拒绝——表示层+校验层双护栏）
- **Eligible Item/Slot Rule**: 连接资格集合不变（SupportCapacity 只走 Weapon/Body/Helmet 硬映射）；2 组资格由 `SocketCount ≥ 3` 推导（当前孔数下=Weapon/Body）；无新增带连接槽位
- **Per-Group Capacity Rule**: 不变（容量=组孔数−1；group 1 恒=1）；启用 group 1 为显式取舍（3S 物品：group 0 剩 0 Support + group 1 得 1 Support）
- **Legacy Mapping**: 既有连接 ≡ group 0，语义零变化（`SkillId.None=0` 使既有数据/构造零迁移）
- **Compatibility Authority**: golden oracle（`SupportCompatGolden` 21 组合）不变不复制；判定唯一走 `IsSupportCompatible`；测试在 group 0/1 两位置各跑 golden 用例证明组位无关
- **Cross-Group Isolation Rule**: 每技能恰一个连接源（映射槽 或 恰一个 group 1 改挂）；改挂语义=被改挂技能的连接整体迁移至 hosting item group 1（结构性排除泄漏）
- **Third-Group Handling**: 表示层无第三槽位+写入前校验 ⇒ 确定性拒绝，无半写入

## Affix 要点

- **Locked N: 4**（1≤4≤6）；**Entry Count Before: 17 → Planned After: 21**
- **Candidate IDs**: AffixId 17「迅疾」（AttackSpeed/Increased 0.10-0.16，不限槽）、18「铁骨」（Armour/Increased 0.10-0.22，非 Belt 五槽——保留壁垒 Belt 生态位）、19「睿智」（Intelligence/Flat 6-12，不限槽）、20「开阔」（AreaRadiusMore/More 0.10-0.20，不限槽）
- **PoEDB Provenance Complete**: YES（人工溯源：攻速 increased 族 / %increased Armour 族 / +Intelligence 属性族 / increased AoE 族——均对应既有消费 stat）
- **New Semantic Families**: NONE（全部既有 StatId+ModOp；ModOp.More≤Override 经词缀行校验允许——`Kernel.cs` 枚举序实证）
- **Prefix/Suffix Introduced**: NONE；**Tier/ModGroup Introduced**: NONE；**BL-024 Required**: NO
- **可达性**：随机掉落/洗炼池（IsApplicable 自动过滤）+ 定向制作（`TryDirectedCraft` 按目录枚举——已核无硬编码清单）
- **Phase 3 登记事项**：stable ID 追加（Count 17→21）；**S4-P3 计数护栏必须同步更新**（`AffixId.Count > S2BaselineAffixCount+3+4` → +4/上限 21——护栏更新引用本清单与导演批准，非语义放宽）

## Test Matrix Cases

12 场景全部表示于 `S5_LINK_CONTRACT.md` §4（legacy 1 组 / 合法 2 组 / 第三组拒绝 / 双组合法 / group 0 非法配对 / group 1 非法配对 / 跨组隔离 / 确定性顺序 / 资格不变 / 每组容量不变 / 零孔色 / 零等级品质）；S5-WO-02 起逐条落地为测试。

## Quick Gate

**PASS（当轮复跑，exit 0）**：**EditMode 246/246、PlayMode 11/11、Content Audit fresh PASS failures=0**——与基线 246/11 完全一致（Phase 0 零测试数变化，AC-14 ✓）；Performance 不重跑（零 runtime 改动）。

## Delta 声明

Capability Delta: **NONE**（Ledger/Matrix 仅授权跟踪，未晋升）；Mechanic Support Delta: **NONE**；Runtime Delta: **NONE**；Canonical Data Delta: **NONE**；Content Delta: **NONE**；Authorization Delta: **NONE**（本单自身不新增授权——授权只反映既有导演批准）。

## Drift Found（1 项自纠 + 1 项流程勘误，均已修复）

1. **自纠（自引漂移，severity=低）**：S4R-WO-01 轮我在 RUNTIME 装备节写入「Gloves/Belt 为词缀位无 Socket」——与代码事实不符（`SliceRules.GlovesSockets/BeltSockets=1`；Tooltip/物品名显示「N孔」）。本轮做 Link 合同核查时发现并修正：「各 1 孔——仅作孔数展示，不映射技能孔位，SupportCapacity 仍只走 Weapon/Body/Helmet」。Ledger 同步修正。原因：S4R 轮未核 `SliceRules` 即从 RUNTIME S2 时代插座表（未列 Gloves/Belt）推断为 0 孔。修复后 RUNTIME/Ledger/代码三方一致。
2. **流程勘误（已即时恢复）**：本轮 DECISIONS 追加时误将上轮 F1 条目整条替换，随即发现并恢复——DECISIONS 现含完整 F1 + S5-WO-01 两条目，历史零丢失。

## Source-of-Truth Conflicts

**0**（修正后 RUNTIME=Ledger=代码一致；Link 合同=唯一权威、RUNTIME 未描述未实现行为）

## Forbidden Expansion Audit

**PASS**——零代码实现；scope 台账三区明确；孔色/宝石成长=负向条件；无 prototype/spike/BL-024/RB lock。

## Open Questions（供规划 AI）

1. **改挂（rebind）语义确认**：group 1 = 第二技能连接「整体迁移」到 hosting item（被改挂技能原映射槽连接失效）——这是隔离规则下唯一自洽解释；若你预期的是其它语义（如叠加），请在放行 S5-WO-02 前纠正合同。
2. **铁骨槽位排除 Belt**（保留壁垒 Belt 专属生态位）是否同意；若你希望不受限，Phase 3 前可改 mask（清单内一行变更）。

## Recommended Next WO

**S5-WO-02 — Multiple-Link Domain Core**（首个 runtime 工作令：BL-021.A2 域/数据层实现，依 `S5_LINK_CONTRACT.md`；不加词缀、不做 UI——按计划 Phase 1 边界）。

## Self-Review 对照 AC-01..AC-15

AC-01 ✓（台账三区）；AC-02 ✓（唯一合同数值 2）；AC-03 ✓（资格指认+不变声明）；AC-04 ✓（每组容量规则指认）；AC-05 ✓（None=0 零迁移）；AC-06 ✓（隔离三条款）；AC-07 ✓（§5 护栏+场景 11/12）；AC-08 ✓（唯一表示+备选 tradeoff 记录）；AC-09 ✓（12/12）；AC-10 ✓（N=4；21=17+4）；AC-11 ✓（逐条准入自查+未入选记录）；AC-12 ✓（人工溯源）；AC-13 ✓（零 runtime diff）；AC-14 ✓（246/11+fresh PASS）；AC-15 ✓（Drift 0 项未决/自纠已修/SoT 0/Forbidden PASS）。
