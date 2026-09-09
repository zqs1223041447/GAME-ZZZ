# S4R_AUTHORIZATION_ATOMS — 授权原子审计（S4R-WO-03 Mandatory Follow-up 交付）

**性质**：授权原子=导演决策的最小授权粒度。**Atom 仅用于决策粒度，不改变 32-item Backlog 的 authoritative ID 数量**（WO-02 Gate Review follow-up）。**任何 atom 的存在都不构成批准**；授权状态只能由导演在 Decision Form 中显式给出。
**Default-on-Omission Rule**：导演未明确批准的 atom 保持其继承状态（NOT_AUTHORIZED / DEFERRED / BLOCKED / FORBIDDEN）；禁止从所选方向名称推导遗漏授权。
**Atoms Total = 57**（覆盖 15 个聚合行；其余 17 行为单粒度行无需拆分，见 §3）。

## 1. 必审聚合行的 Atoms（规划 AI 点名）

### BL-001 完整装备槽（4 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-001.A1 | Equip-slot breadth（除点名槽外的槽位广度框架：扩至 10 槽的骨架语义） | NOT_AUTHORIZED | DIR-1/DIR-2 |
| BL-001.A2 | Weapon family（武器族/基础类型） | NOT_AUTHORIZED | DIR-1/DIR-2 |
| BL-001.A3 | Handedness（双手/单手/Offhand 占用语义） | NOT_AUTHORIZED | DIR-2 |
| BL-001.A4 | Requirements（等级/属性需求模型） | NOT_AUTHORIZED | DIR-2 |

### BL-003 完整 Passive Tree（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-003.A1 | Larger Passive Tree（大树镜像/Scion/50 点/连通规则/基础 Tree UI） | NOT_AUTHORIZED（**blocking prereq=BL-024**） | DIR-2 |
| BL-003.A2 | Mastery | **NOT_AUTHORIZED（显式未批准，独立于 A1）** | DIR-2（可选） |
| BL-003.A3 | Ascendancy（数据+开放 Ascendant+8 点） | **NOT_AUTHORIZED（显式未批准，独立于 A1/A2）** | DIR-2（可选） |

### BL-012 Skill Gem 高级成长（5 atoms；与 BL-022 域合并说明见 A1）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-012.A1 | Gem Level/Quality 数据模型（**权威原子；BL-022 Support Level/Quality 的授权粒度归并于此，不另立**） | NOT_AUTHORIZED | DIR-1 |
| BL-012.A2 | Gem Corruption（**Gem 域——与 BL-013.A9 Item Corruption 严格领域区分**） | NOT_AUTHORIZED | DIR-1/DIR-3 |
| BL-012.A3 | Gem Ascension | NOT_AUTHORIZED | 远期 |
| BL-012.A4 | Alternate Form | NOT_AUTHORIZED | 远期 |
| BL-012.A5 | Final Upgrade | NOT_AUTHORIZED | 远期 |

### BL-013 Deep Craft（9 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-013.A1 | Add Affix | NOT_AUTHORIZED | DIR-3 |
| BL-013.A2 | Remove Affix | NOT_AUTHORIZED | DIR-3 |
| BL-013.A3 | Reroll（重掷既有行） | NOT_AUTHORIZED | DIR-3 |
| BL-013.A4 | Lock | NOT_AUTHORIZED | DIR-3 |
| BL-013.A5 | Targeted Reroll | NOT_AUTHORIZED | DIR-3 |
| BL-013.A6 | Upgrade Tier | NOT_AUTHORIZED（若需 canonical Tier 数据→BL-024 可选依赖） | DIR-3 |
| BL-013.A7 | Special Craft | NOT_AUTHORIZED | DIR-3 |
| BL-013.A8 | Final Craft | NOT_AUTHORIZED | DIR-3 |
| BL-013.A9 | Item Corruption（**Item/Craft 域——与 BL-012.A2 Gem Corruption 严格领域区分**） | NOT_AUTHORIZED | DIR-3 |

### BL-016 Encounter 族（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-016.A1 | Monster Package（打包刷怪语义） | NOT_AUTHORIZED | DIR-4 |
| BL-016.A2 | Boss（独立 gameplay taxonomy+行为+内容） | NOT_AUTHORIZED | DIR-4 |
| BL-016.A3 | Special Encounter | NOT_AUTHORIZED | DIR-4 |

## 2. 其它聚合行补建 Atoms（同规则自查）

### BL-002 更多 Affix（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-002.A1 | 现有 Stat/ModOp 机制内新增词缀条目（S4-P3 同型低门槛扩张） | NOT_AUTHORIZED | DIR-1 |
| BL-002.A2 | Prefix/Suffix 系统改造 | NOT_AUTHORIZED | DIR-1（可选） |
| BL-002.A3 | Tier / Mod Group（互斥）/ Item Level·Weight 生成规则系统 | NOT_AUTHORIZED（Tier 数据若 canonical 化→BL-024 可选依赖） | DIR-1（可选）/DIR-3 |

### BL-004 Aura / Reservation（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-004.A1 | Reservation 机制（Max/Current/Reserved Mana、%、Efficiency） | NOT_AUTHORIZED | DIR-2 |
| BL-004.A2 | Aura Effect 体系（Emitter/Receiver、Self/Ally/Enemy 目标模型） | NOT_AUTHORIZED | DIR-2 |
| BL-004.A3 | 四 Aura 内容（Pride/Anger/Determination/Grace） | NOT_AUTHORIZED（内容轴，依赖 A1/A2） | DIR-2 |

### BL-007 更多 Defense（4 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-007.A1 | Block | NOT_AUTHORIZED | DIR-2 |
| BL-007.A2 | Energy Shield | NOT_AUTHORIZED | DIR-2 |
| BL-007.A3 | Suppression | NOT_AUTHORIZED | DIR-2 |
| BL-007.A4 | Leech（复杂 Leech） | NOT_AUTHORIZED | DIR-2 |

### BL-008 更完整 Ailment（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-008.A1 | Bleed | NOT_AUTHORIZED | DIR-2 |
| BL-008.A2 | Poison | NOT_AUTHORIZED | DIR-2 |
| BL-008.A3 | Shock | NOT_AUTHORIZED | DIR-2 |

### BL-015 Map Tier / Progression Spine（4 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-015.A1 | Map Tier 进程（tier 梯度/难度进程） | NOT_AUTHORIZED | DIR-4 |
| BL-015.A2 | Map Modifier Pool 扩张 | NOT_AUTHORIZED | DIR-4 |
| BL-015.A3 | Modifier Synergy | NOT_AUTHORIZED | DIR-4 |
| BL-015.A4 | Stability / Reward Multiplier 语义扩展（基础语义已存在，此为扩展） | NOT_AUTHORIZED | DIR-4 |

### BL-020 技术路线锁包（4 atoms，各自独立 gate）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-020.A1 | DOTS / Entities 解禁 | DEFERRED | 无（除非所选方向确需） |
| BL-020.A2 | HDRP 解禁 | DEFERRED | 无 |
| BL-020.A3 | FMOD（中间件）解禁 | DEFERRED | 无 |
| BL-020.A4 | 捏脸换装（character customization）解禁 | DEFERRED | 无 |

### BL-021 Socket 孔色 / 多 Link（2 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-021.A1 | 孔色与颜色兼容（Gem Color metadata 语义启用） | NOT_AUTHORIZED | DIR-1 |
| BL-021.A2 | 多 Link / 扩孔（Gloves/Belt 词缀位→带孔） | NOT_AUTHORIZED | DIR-1 |

### BL-023 全量 Active/Support 导入 + Gem Library（3 atoms）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-023.A1 | Active Skill 全量导入 | NOT_AUTHORIZED（prereq=BL-024） | 远期 |
| BL-023.A2 | Support Gem 全量导入 | NOT_AUTHORIZED（prereq=BL-024） | 远期 |
| BL-023.A3 | Validation Gem Library / Loadout UI | NOT_AUTHORIZED | 远期 |

### BL-028 点名装备槽（3 atoms——与 BL-028 行一一对应，独立于 BL-001.A1）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-028.A1 | Ring（含 Ring1/Ring2 重复槽语义） | NOT_AUTHORIZED | DIR-1/DIR-2（可选） |
| BL-028.A2 | Offhand / 双持语义 | NOT_AUTHORIZED | DIR-2（可选） |
| BL-028.A3 | Amulet | NOT_AUTHORIZED | DIR-1/DIR-2（可选） |

### BL-029 新内容轴授权门槛（4 atoms——门槛记录，每轴独立批准）

| Atom | 子能力 | 状态 | 归属方向 |
|---|---|---|---|
| BL-029.A1 | 新 Active Skill | NOT_AUTHORIZED | 按所选方向显式批准 |
| BL-029.A2 | 新 Support | NOT_AUTHORIZED | 同上 |
| BL-029.A3 | 新 Affix family | NOT_AUTHORIZED | 同上 |
| BL-029.A4 | 新 progression currency/resource | NOT_AUTHORIZED | 同上 |

## 3. 单粒度行（无需拆分的 17 行及理由）

BL-005 Curse、BL-006 Flask、BL-009 高级 Trigger（单机制族，扩展即整体）、BL-010 Unique（系统框架与内容一体；内容生产归 BL-018 downstream）、BL-011 Jewel、BL-014 Craft Simulator、BL-017 Map Risk Simulator、BL-018 内容工厂（**downstream destination，Phase 12 整体性**）、BL-019 Atlas、BL-022（授权粒度归并 **BL-012.A1**，不重复授权）、BL-024 PoEDB Pipeline（**双门**：FORBIDDEN_UNTIL_APPROVED；方向选择≠工作令）、BL-025 Persistence、BL-026 Voice（BLOCKED，Resume=导演指认）、BL-027 树 UI（依赖 BL-003.A1）、BL-030 Death 截断、BL-031 三色定稿、BL-032 合同变更流程。

## 4. Hidden-Approval 审计结论（AC-02）

- 审计前风险点 5 处（BL-001/BL-003/BL-012/BL-013/BL-016 宽行隐式解禁）→ 全部拆分 ✓。
- 补建审计发现并拆分：BL-002/BL-004/BL-007/BL-008/BL-015/BL-020/BL-021/BL-023/BL-028/BL-029 ✓。
- 域区分保留：BL-012.A2（Gem Corruption）≠ BL-013.A9（Item Corruption）✓；BL-022 并入 BL-012.A1 唯一权威 ✓。
- **结论：不存在「批准宽泛 Backlog ID 即隐式批准多个独立 gated 机制」的路径；DIR-1 复用优先路径（BL-002.A1/BL-021/BL-012.A1）同样逐 atom 显式授权，不得打包。**
