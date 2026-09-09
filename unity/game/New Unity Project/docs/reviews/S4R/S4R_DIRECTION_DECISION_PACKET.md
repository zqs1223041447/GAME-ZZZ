# S4R_DIRECTION_DECISION_PACKET — 下一方向决策包（S4R-WO-03 交付）

**性质**：供导演做**下一产品方向选择与 capability delta 授权**的决策材料。五个方向全部 `CANDIDATE — NOT APPROVED`；Planner 建议仅为相对比较输入，不产生授权。终点=「导演已经能做决定」。**本包不启动任何方向的实现。**
**输入**：S4 Frozen Baseline、Capability Ledger（28 行）、Mechanic Matrix、32-item Backlog（`S4R_BACKLOG_NORMALIZATION.md`）、依赖 DAG、3 个 RBC 候选、Hard-Stop/Forbidden 清单、57 个授权原子（`S4R_AUTHORIZATION_ATOMS.md`——权威 atom 位置，本包只引用）。

---

## §1 方向卡（每卡 25 字段）

### DIR-0 — Consolidation & Production Quality（CANDIDATE / NOT APPROVED）

| # | 字段 | 内容 |
|---|---|---|
| 1 | Player value | 可靠性/手感稳定/表现修缮（polish）——无新增玩法价值，降低现有体验的毛刺 |
| 2 | Why now / why not now | Now：S4 后已知限制（BL-030 Death 截断）与合同边界（BL-032）是仅有的未偿质量债，趁 feature 冻结清偿成本最低。Not now：玩家可见价值增量最小 |
| 3 | Included backlog IDs | BL-030、BL-032 |
| 4 | Included authorization atoms | 无需新 atom（两行均单粒度 NOT_AUTHORIZED→需导演批准转入实施）；optional atoms=BL-027（树 UI）、BL-031（三色）——**仅导演显式加入，不得自动纳入** |
| 5 | Explicitly excluded | 一切 BREADTH/DEPTH/CONTENT 项；BL-014/BL-017（属未来验证基建，非本轮 consolidation）；BL-020 路线锁 |
| 6 | Existing reuse | 100%（全复用 S0-S4 全部能力；零新机制） |
| 7 | New capabilities required | **0** |
| 8 | Required Director gates | BL-030/BL-032 从 NOT_AUTHORIZED 转授权；optional：BL-027/BL-031 |
| 9 | Immediate prerequisites | 无 |
| 10 | Blocking prerequisites | 无 |
| 11 | Downstream enabled | 任何后续方向的基座降险（间接） |
| 12 | Canonical-data dependency | 无 |
| 13 | BL-024 dependency | **不需要** |
| 14 | Art dependency | 无（optional BL-031 才涉及） |
| 15 | Audio dependency | 无 |
| 16 | UI dependency | optional（BL-027） |
| 17 | Test/evidence burden | LOW——回归+契约测试既有（理由：零新机制，既有 246+11 套件覆盖） |
| 18 | Performance risk | LOW——表现层小修不触 hot path（理由：Death 视觉/合同流程均为非战斗路径） |
| 19 | Determinism risk | N-A——不动随机与结算 |
| 20 | Save/migration risk | N-A——无存档系统 |
| 21 | Runtime blast radius | LOW——仅表现层与流程文档 |
| 22 | Reference Build impact | 三候选继续有效，覆盖不变 |
| 23 | Long-term Phase compatibility | 兼容（可与 Phase 8+ 任一方向衔接；不改变长期顺序语义） |
| 24 | DoD shape | 已知限制清单缩减（Death 截断关闭）+ 合同变更流程文档化 + 回归全绿；不以新内容/系统为验收 |
| 25 | What becomes possible afterward | 更稳基座：后续任一方向（DIR-1..4）的实施风险与证据负担降低 |

### DIR-1 — Reuse-First Build & Itemization Depth（CANDIDATE / NOT APPROVED；Planner 比较基准第一）

| # | 字段 | 内容 |
|---|---|---|
| 1 | Player value | Build differentiation / itemization chase——同一 3 技能框架下更多有效构筑与装备取舍 |
| 2 | Why now / why not now | Now：复用面最大（3/7/16 节点/6 槽/17 词缀/Combat Math 全部现成）、依赖最短、3 个 RBC 候选可直接用作验收面。Not now：不提供 progression/endgame 层新价值 |
| 3 | Included backlog IDs | BL-002（atoms A1 为主）、BL-021（A1/A2）、BL-022（粒度归 BL-012.A1）；可选：BL-028.A1/A3（Ring/Amulet 需导演显式） |
| 4 | Included authorization atoms | BL-002.A1（现有机制内词缀扩条目）；BL-021.A1 孔色 / BL-021.A2 多 Link（小新机制）；BL-012.A1 Gem Level/Quality 数据模型；**逐 atom 显式授权，不得打包** |
| 5 | Explicitly excluded | BL-002.A2/A3（Prefix-Suffix/Tier 系统改造——留 DIR-3 或后续）、BL-028.A2（Offhand 归 DIR-2 域）、一切 endgame/progression |
| 6 | Existing reuse | HIGH——技能/Support/词缀/掉落/Craft/装备聚合/Tooltip/抽屉全部现成（理由：全部候选输入均为既有系统的数据或小模型扩展） |
| 7 | New capabilities required | 少而明确：孔色兼容系统（BL-021.A1）、Gem Level/Quality 数据模型（BL-012.A1）；其余为内容/数据扩 |
| 8 | Required Director gates | BL-021/BL-012.A1/BL-002.A1 各自解禁（现 NOT_AUTHORIZED）；如含 Ring/Amulet→BL-028.A1/A3 |
| 9 | Immediate prerequisites | 无（复用面已成立） |
| 10 | Blocking prerequisites | 无 |
| 11 | Downstream enabled | 为 DIR-2（Aura 与属性面联动）、DIR-3（craft 目标池变大）提供更宽 itemization 底座 |
| 12 | Canonical-data dependency | 无外部依赖（自设计数据表足够） |
| 13 | BL-024 dependency | **不需要** |
| 14 | Art dependency | 无 |
| 15 | Audio dependency | 无 |
| 16 | UI dependency | 中——孔色/等级质量需 Tooltip/抽屉/制作面板展示扩展（既有 UI 框架内） |
| 17 | Test/evidence burden | MEDIUM——新轴需契约测试+审计扩展（理由：孔色/等级属新数据语义，须进 Production Report 与 audit 失败类别） |
| 18 | Performance risk | LOW——数据轴扩不进 hot path（理由：词缀/孔位均为进图快照聚合，非每帧路径） |
| 19 | Determinism risk | MEDIUM——新增掷值轴须全部走 SeededRng 并入 sim 覆盖（理由：现有 10k sim 契约要求新 roll 源可复现） |
| 20 | Save/migration risk | LOW——会话内数据结构扩展，无跨会话存档 |
| 21 | Runtime blast radius | MEDIUM——Socket/Item 数据模型扩展（理由：触 EquipSlot/Socket 语义但复用既有聚合管线） |
| 22 | Reference Build impact | 三候选直接可用并显著增强（新词缀/孔色直接进 RBC 装备轴）；coverage 不再充分的风险=无 |
| 23 | Long-term Phase compatibility | 高（=长期规划 Phase 8 早期「更多 Affix」+装备语义的复用优先实现；未跳过硬依赖） |
| 24 | DoD shape | 新词缀条目/孔色/Level-Quality 轴全部进 canonical 数据+审计+sim；build 差异肉眼可见（导演 S2 门语义延续）；无 regression |
| 25 | What becomes possible afterward | 更大 build 空间；为 Aura 等新系统的「装备面联动」提供已验证载体；RBC 可升级为更接近正式 Targets 的形态 |

### DIR-2 — Core Build-System Expansion（CANDIDATE / NOT APPROVED）

| # | 字段 | 内容 |
|---|---|---|
| 1 | Player value | 新构筑维度（Aura 取舍/新防御层/新 ailment/大树路线）——build depth 质变 |
| 2 | Why now / why not now | Now：S4 已证明内容可安全规模化，新系统是 BDA 主线（Passive/Aura=Reference Builds 关键轴）。Not now：多个子能力为真新 capability，blast radius 与证据负担最大；多数 atom 为 Director-gated |
| 3 | Included backlog IDs | BL-003（A1 必选/A2/A3 可选）、BL-004（A1-A3）、BL-007（A1-A4 按选）、BL-008（A1-A3 按选）、BL-009、BL-001（A2/A3/A4 可选） |
| 4 | Included authorization atoms | 全部逐 atom：BL-003.A1（**prereq=BL-024**）/A2/A3；BL-004.A1 Reservation 机制/A2 Effect 体系/A3 四 Aura 内容；BL-007.A1..A4；BL-008.A1..A3；BL-009；BL-001.A2..A4 |
| 5 | Explicitly excluded | BL-020 路线锁（不混入）；Phase 8 顺序不得作为批准依据 |
| 6 | Existing reuse | MEDIUM——复用 StatBag/Combat Math/Trigger 基础/6 槽聚合（理由：新机制在其上扩展而非重写） |
| 7 | New capabilities required | 多：Reservation 模型、Aura Emitter/Receiver、Block/ES/Suppression/Leech、Bleed/Poison/Shock、advanced Trigger、大树镜像 |
| 8 | Required Director gates | 上述全部 atoms 现 NOT_AUTHORIZED，均需显式解禁 |
| 9 | Immediate prerequisites | 无硬性（BL-003.A1 除外） |
| 10 | Blocking prerequisites | **BL-003.A1 → BL-024（双门）**；其余子能力无外部 blocking |
| 11 | Downstream enabled | BDA Reference Builds（4 build 差异）成为可能；为 DIR-4 progression 提供 build 面 |
| 12 | Canonical-data dependency | BL-003.A1/A2/A3 需要 PoEDB 镜像数据；其余子能力可自设计 |
| 13 | BL-024 dependency | **部分需要**（仅 Passive 族 atoms；Aura/Defense/Ailment/Trigger 不需要）——选 DIR-2 ≠ 自动批准 BL-024 |
| 14 | Art dependency | 低-中（Aura 表现/新 ailment 表现） |
| 15 | Audio dependency | 低 |
| 16 | UI dependency | 高——大树 UI/Reservation UI/新防御展示（BL-027 若纳入则依赖 BL-003.A1） |
| 17 | Test/evidence burden | HIGH——每个新机制需公式 golden+兼容矩阵扩展+审计类别扩展（理由：现有 Combat Math 契约模式要求每规则 Input/Expected/Source） |
| 18 | Performance risk | HIGH——Aura 体系与多 ailment 进战斗热路径（理由：现有热路径无 per-frame 装备扫描，Aura/ailment tick 是新每帧成本源） |
| 19 | Determinism risk | MEDIUM——新随机轴（ailment roll 等）须全走 SeededRng（理由：同 DIR-1） |
| 20 | Save/migration risk | MEDIUM——build 状态字段大扩（理由：passive/aura/defense 状态进入 Build snapshot 语义） |
| 21 | Runtime blast radius | HIGH——Combat Math/StatBag/状态机多面扩展（理由：命中顺序与聚合轴变更面广） |
| 22 | Reference Build impact | 最强增强（Aura 直接服务于 BDA 4-build 差异目标）；大树/Ascendancy 需要未来新 candidates |
| 23 | Long-term Phase compatibility | 高（=Phase 8 中段+Phase 3 BDA 主线；须保留「顺序≠批准」标注） |
| 24 | DoD shape | 所选 atoms 全部有公式/兼容/性能证据；新机制进 COMBAT_MATH/RUNTIME/Ledger/Matrix；无 baseline regression |
| 25 | What becomes possible afterward | BDA 主线解锁：4 个结构不同 Reference Builds、Reservation 取舍、纵深防御；为 DIR-4 提供 build 前提 |

### DIR-3 — Crafting Depth（CANDIDATE / NOT APPROVED）

| # | 字段 | 内容 |
|---|---|---|
| 1 | Player value | Item construction chase——定向造装/改造/保底的长期目标感 |
| 2 | Why now / why not now | Now：bounded Craft 已成立（随机洗 Rare/定向写入/duplicate reject/确定性）且 10k sim 基建可扩展。Not now：Deep Craft 各操作为独立授权面；无 Tier 语义时升级类操作缺乏数据基础 |
| 3 | Included backlog IDs | BL-013（A1-A9 按导演挑选）、BL-014（**后续验证基建，非首单实现**）、BL-002.A3（Tier 语义，可选） |
| 4 | Included authorization atoms | BL-013.A1..A9 逐个；BL-002.A3（Tier/Mod Group/生成规则）；BL-012.A2（Gem Corruption，如纳入） |
| 5 | Explicitly excluded | BL-014 不在本方向首单实现（sim 是验收基建）；Phase 10 存在≠优先级已定 |
| 6 | Existing reuse | HIGH——Craft/词缀/装备/确定性管线全部现成（理由：新操作在既有 TryCraft 语义上扩展） |
| 7 | New capabilities required | 中：各 craft 操作语义+确定性规则；Tier 语义（若纳入） |
| 8 | Required Director gates | 所选 BL-013 atoms + BL-002.A3 解禁 |
| 9 | Immediate prerequisites | 无 |
| 10 | Blocking prerequisites | BL-013.A6（Upgrade Tier）若需 canonical Tier 数据→BL-024 可选依赖（双门适用） |
| 11 | Downstream enabled | 经济平衡能力（sim）；Unique/高价值物品 chasing（远期） |
| 12 | Canonical-data dependency | 可选（Tier 体系） |
| 13 | BL-024 dependency | **默认不需要**；仅 Tier canonical 化时可选（不得因选 DIR-3 自动开工） |
| 14 | Art dependency | 无 |
| 15 | Audio dependency | 无 |
| 16 | UI dependency | 中——Craft 面板扩展（既有 560×340 面板内） |
| 17 | Test/evidence burden | HIGH——每操作需确定性+非法态负向测试（理由：duplicate guard 先例=每操作一条 deterministic reject 契约） |
| 18 | Performance risk | LOW——craft 为非战斗路径（理由：现有 TryCraft 在装配/城镇语义） |
| 19 | Determinism risk | HIGH——新操作引入新随机消费序列，sim 必须先行扩展（理由：SeededRng 序列变更会影响可复现性契约） |
| 20 | Save/migration risk | LOW-MEDIUM——物品数据结构若加 Tier 字段需迁移语义（会话内） |
| 21 | Runtime blast radius | MEDIUM——Craft/Sim 管线扩展 |
| 22 | Reference Build impact | 强（RBC 的 itemization 维度获得 chase 手段）；不需新 candidates |
| 23 | Long-term Phase compatibility | 高（=Phase 10 的复用优先前段；顺序≠批准） |
| 24 | DoD shape | 所选操作全部：deterministic+非法态拒绝+sim 覆盖+审计；经济类操作（成本相关）在 sim 建立后才可判定 |
| 25 | What becomes possible afterward | 100k Craft Simulator 的验收语义成立；Unique/高价值内容的 item chase 底座 |

### DIR-4 — Progression & Endgame Spine（CANDIDATE / NOT APPROVED）

> **⚠ CURRENTLY DIRECTOR-GATED / NO IMPLEMENTATION AUTHORITY**（硬停止点名族；WO-03 不改变其状态）

| # | 字段 | 内容 |
|---|---|---|
| 1 | Player value | 长线进程/终局目标（Map Tier 梯度/Boss 挑战）——当前完全缺位的 progression 层 |
| 2 | Why now / why not now | Not now（默认）：build-system 深度不足时 progression 内容价值薄（长期规划语义：Phase 11 前置=Phase 8-10）；硬停止显式点名。Now（仅当导演显式 override）：外部无技术硬阻塞（Map/Boss 可自设计） |
| 3 | Included backlog IDs | BL-015（A1-A4）、BL-016（A1-A3）、BL-017、BL-019 |
| 4 | Included authorization atoms | BL-015.A1..A4；BL-016.A1..A3；BL-017（单粒度）；BL-019（单粒度） |
| 5 | Explicitly excluded | BL-018 内容工厂（downstream，见 §2）；BL-020 路线锁 |
| 6 | Existing reuse | MEDIUM——复用图词缀/Stability/Reward/敌人生成/命中/视觉管线（理由：Map Tier 是现有单图语义的参数化扩展，非重写） |
| 7 | New capabilities required | 多：Tier 进程、Monster Package 打包、Boss taxonomy+行为、Special Encounter、Atlas 语义 |
| 8 | Required Director gates | 全部上述 atoms（DEFERRED→显式解禁）；本方向整体 = 硬停止解除令 |
| 9 | Immediate prerequisites | 无技术硬前置（自设计路线）；长期规划语义前置=build-system 深度（DIR-2 族） |
| 10 | Blocking prerequisites | 无（BL-024 不需要——Map/Boss 数据可自设计） |
| 11 | Downstream enabled | 内容工厂（BL-018）的主消费场景；Map Risk Simulator 的语义输入 |
| 12 | Canonical-data dependency | 无外部（自设计） |
| 13 | BL-024 dependency | **不需要** |
| 14 | Art dependency | HIGH——Boss/新怪/场景资产（理由：现 4 正式视觉+1 图，Boss/新场景需新资产批次） |
| 15 | Audio dependency | 中（Boss/新遭遇 SFX） |
| 16 | UI dependency | 中（地图选择/进度 UI） |
| 17 | Test/evidence burden | HIGH——新敌类 AI/遭遇语义/进程数值需全链测试（理由：Boss=新 gameplay taxonomy，现有「无 BossKind」边界反转） |
| 18 | Performance risk | HIGH——Boss 战/新遭遇密度超出现有 canonical workload（理由：现性能门锁 100-200-300 Dummy 语义，Boss 战形态未测） |
| 19 | Determinism risk | MEDIUM——进程随机（地图生成）须全走 SeededRng |
| 20 | Save/migration risk | HIGH——进程状态天然需要持久化语义（理由：tier 进度跨会话存在才完整；触及 BL-025 存档决策） |
| 21 | Runtime blast radius | HIGH——AI/状态机/地图系统多面 |
| 22 | Reference Build impact | 方向会**要求未来新增 candidates**（encounter 验证 build）；且**不得**为 RBC 创建 boss/map-tier clear targets（WO-03 §8 禁止） |
| 23 | Long-term Phase compatibility | 语义一致（=Phase 11）但**位置靠后**；不得把 Phase 11 长期位置解释为已批准立即实施 |
| 24 | DoD shape | Tier 进程可玩可测、Boss taxonomy+行为成立、进程数值有 Risk sim 支撑（BL-017 建立后）；硬停止由本方向批准显式解除 |
| 25 | What becomes possible afterward | Endgame 循环成立；内容工厂获得主场景；长线留存价值 |

## §2 Explicitly Downstream（不是下一方向候选）

**Phase 12 Content Factory（BL-018 / new Content Batch family）= downstream destination**。理由：长期规划要求「所有核心机制稳定后」才大规模内容生产，而当前仍有多组 Partial/Unsupported/Director-Gated 机制（Ledger：4 Partial/5 Unsupported/1 Blocked）。BL-018 **不得**作为 WO-03 默认推荐方向。导演可显式 override 长期规划，但必须是显式 Director override（决策表 Additional Constraints 中写明）。

## §3 Cross-Cutting Director Toggles（独立呈现，不因选向隐式决定）

| Toggle | 当前状态 | 导演须明确 | 默认 |
|---|---|---|---|
| Voice | DEFERRED / BLOCKED（3 键 0/3，等试听指认） | KEEP DEFERRED 或 RESUME GATE（解除试听门） | KEEP DEFERRED |
| BL-024 PoEDB Pipeline | FORBIDDEN_UNTIL_APPROVED（双门） | ELIGIBLE FOR NEXT-CYCLE PLANNING 或 KEEP FORBIDDEN；**选依赖它的方向≠给 BL-024 工作令**——需它的是 DIR-2 的 Passive 族 atoms（BL-003.A1/A2/A3）与 BL-023 全量导入；不需要它的：DIR-0/DIR-1/DIR-3（除 BL-002.A3 Tier 可选）/DIR-4 | KEEP FORBIDDEN |
| New Content Batch | 硬停止点名 | 独立 YES/NO | NO |
| Ring / Offhand / Amulet | BL-028.A1/A2/A3 独立 atoms | 逐槽显式；不得借「Equipment Breadth」隐式解禁 | NO |
| Technical Route | BL-020.A1..A4 独立 gate | 除非所选方向确需，不混入 scope | 继续锁 |

## §4 Cross-Direction Comparison Matrix（一屏比较）

| 比较轴 | DIR-0 | DIR-1 | DIR-2 | DIR-3 | DIR-4 |
|---|---|---|---|---|---|
| Player-facing value axis | Polish/可靠性 | Build differentiation | 新构筑维度 | Item chase | Progression/Endgame |
| Breadth vs Depth | —（质量） | 偏 Breadth（itemization 广度） | Depth（新系统） | Depth（craft 深度） | Breadth（内容进程） |
| Existing capability reuse | 100% | HIGH | MEDIUM | HIGH | MEDIUM |
| 新 capability 族 | 0 | 2 小族（孔色/Gem L-Q 模型） | 5+ 族（Aura/Defense/Ailment/Trigger/大树） | 1 族（craft 操作集）+可选 Tier | 4 族（Tier/Boss/Encounter/Atlas） |
| Dependency depth | 0 | 0 | 深（含 BL-024 双门） | 浅 | 中（长期规划语义前置=build 深度） |
| Director gates required | 2（+2 optional） | 3-5 atoms | 8-20 atoms | 1-10 atoms | 9 atoms+硬停止解除 |
| BL-024 required? | NO | NO | **部分**（仅 Passive 族） | 默认 NO（Tier 可选） | NO |
| Endgame opening? | NO | NO | NO（为将来铺垫） | NO | **YES（本质）** |
| Content Batch required? | NO | NO | 可选（表现配套） | NO | YES（Boss/场景资产） |
| Save-data implications | N-A | LOW | MEDIUM | LOW-MEDIUM | HIGH（触及 BL-025） |
| Performance risk | LOW | LOW | HIGH | LOW | HIGH |
| Determinism risk | N-A | MEDIUM | MEDIUM | HIGH | MEDIUM |
| Test burden | LOW | MEDIUM | HIGH | HIGH | HIGH |
| Art/audio burden | N-A | N-A | LOW-MED | N-A | HIGH |
| Reference Build relevance | 中性（候选不变） | **直接增强三候选** | 最强增强（BDA 4-build 目标） | 强（itemization 维度） | 需未来新 candidates |
| Major irreversible/expensive choice | 无 | 无（可增量） | 大树镜像的 IP 风险承压（Risk Register 既有） | Tier 语义定型 | Boss/taxonomy 定型+存档需求 |
| Earliest blocker | 无（仅需授权） | 无（仅需授权） | BL-024 双门（若选 Passive 族） | 无（仅需授权） | 硬停止显式解除 |

（禁止无证据的天数/周数/story-point 估算——本表只含事实与相对判断。）

## §5 Planner Recommendation Input（相对建议，不产生授权）

**默认比较顺序：DIR-1 → DIR-2 → DIR-3 → DIR-4；DIR-0 作为独立 Consolidation alternative 参与比较（非 fallback）。**
DIR-1 居首依据（仅四条事实）：①复用现有 supported surface 较多；②依赖路径相对较短；③不要求先打开正式 endgame；④可直接利用当前 3 个 Candidate Reference Builds。
**本节不是「Director 已选择 DIR-1」**；DIR-2 的 BDA 主线价值、DIR-0 的降险价值均真实存在，最终选择权完全在导演。

## §6 Director Decision Form（原样回传）

```text
Selected Direction:
[ ] DIR-0  Consolidation & Production Quality
[ ] DIR-1  Reuse-First Build & Itemization Depth
[ ] DIR-2  Core Build-System Expansion
[ ] DIR-3  Crafting Depth
[ ] DIR-4  Progression & Endgame Spine
[ ] CUSTOM （说明：____）

Approved Authorization Atoms:（逐个列，如 BL-021.A1、BL-002.A1……）

Explicitly Deferred Authorization Atoms:

Explicitly Forbidden Authorization Atoms:

Progression Capability:  YES / NO
Endgame Capability:      YES / NO
Boss:                    YES / NO
Unique:                  YES / NO
Ring:                    YES / NO
Offhand:                 YES / NO
Amulet:                  YES / NO
New Content Batch:       YES / NO
Voice:                   KEEP DEFERRED / RESUME GATE
PoEDB Pipeline (BL-024): ELIGIBLE FOR NEXT-CYCLE PLANNING / KEEP FORBIDDEN

Primary Success Axis:
- [ ] Build Diversity
- [ ] Player Progression
- [ ] Encounter Depth
- [ ] Itemization Depth
- [ ] Production Polish
- [ ] Content Breadth
- [ ] Other: ______

Breadth vs Depth Preference: ______

Additional Director Constraints: ______
```

**Default-on-Omission Rule（必须随表生效）**：导演没有明确批准的 capability / authorization atom，继续保持原有 NOT_AUTHORIZED / DEFERRED / BLOCKED / FORBIDDEN 状态；禁止从所选方向名称推导遗漏的授权。

## §7 Reference Build Treatment（AC-08）

- RC-M/RC-P/RC-A 继续 **CANDIDATE / NOT LOCKED**；本包不锁 Concentrated/Combustion 取舍、不锁 Support loadout、不建 DPS gate、不建 boss/map-tier clear target、不把正式 Reference Build Coverage 从 N/A 晋级。
- 方向→RBC 影响：DIR-0=三候选不变；**DIR-1=三候选直接可用并增强**；DIR-2=显著增强且大树/Ascendancy 将要求未来新 candidates；DIR-3=强化 itemization 维度；DIR-4=会使当前候选覆盖不再充分（需 encounter 型新 candidates——在正式锁定后建立）。
