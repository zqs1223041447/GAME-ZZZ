# GAME-ZZZ 新短期规划讨论 QA

> 用途：记录在正式编写“指导规划 AI 工作的新短期规划”之前的全部关键讨论、推荐与 Director 决策。
> 状态：讨论中，尚未形成最终短期规划。
> 原则：正式规划只在关键问题全部确认后编写。

## 已确认的总体方向

### Q0-1：新版短期计划走多远？
**候选：**
- A：装备 + 天赋成熟后验收
- B：装备 + 天赋 + Aura / Reservation，正式验证 Build Diversity
- C：一路推进到第一次 Endgame Alpha

**推荐：** B

**Director 回答：** B

**已确认结论：**
新版短期计划以 **Build Diversity Alpha** 为核心，目标至少覆盖完整装备结构、构筑深化、中型/大型 Passive Tree 验证，以及 Aura / Reservation 第一版；完成后做正式 Build Diversity 验收，而不是直接一路扩到 Endgame Alpha。

---

### Q0-2：下一阶段优先“更多内容”还是“更深构筑”？
**候选：**
1. 优先增加 Active Skill 数量
2. 优先增加装备、Affix、Support、Passive、Aura 等组合维度

**推荐：** 2

**Director 回答：** 2

**已确认结论：**
优先做 **构筑深度与组合维度**，暂不以增加 Active Skill 列表长度作为主要目标。

---

### Q0-3：是否补齐装备槽？
**Director 回答：** 要补，并且同技能进行联调。

**已确认结论：**
新版短期计划必须继续补齐装备体系；新增装备槽不能作为孤立数据扩张，必须与现有/新增技能、Support、Affix 和 Build 结果进行联调验证。

---

### Q0-4：Passive Tree 采用什么方向？
**Director 回答：**
直接以现成的 Path of Exile 天赋树为模板，属性、功能、特性均作为实现参考；本项目因技术或系统前置条件暂时无法实现的节点，在天赋树中明确标注“未实装”。

角色在当前阶段默认具有 50 天赋点，后续正式游戏将通过升级或消耗品等方式获得天赋点。

**待继续细化：**
- 采用哪一版本/哪一赛季的 POE Passive Tree 作为基准
- “直接抄”具体包括：拓扑、数值、节点名称、节点文本、Keystone/Notable 机制中的哪些层级
- 暂未实装节点在运行时的行为
- 与本项目尚不存在系统的依赖管理

---

### Q0-5：规划 AI / 工作 AI 自动推进到什么程度？
**Director 回答：**
按照“已批准阶段范围内、硬 Gate 全绿即可自动进入下一工作令”的方向调整；但必须特别设计 **非阻塞项目**。

**已确认结论：**
规划 AI 可以在已批准的短期计划范围内持续安排工作 AI，不应因单个阻塞项停摆。必须建立明确的：
- 阻塞项记录
- 非阻塞任务池
- 可并行/可替代工作
- 重新进入阻塞项的触发条件

涉及新游戏设计语义、新核心系统方向或“是否好玩”的判断，仍回到 Director 决策。

---

## Grill-me 讨论

### Q1：当前默认 50 天赋点，是正式产品规则还是 Alpha 测试规则？
**问题：**
“角色默认具有 50 天赋”是：
- A：正式游戏中新角色出生即拥有 50 点
- B：当前 Build Diversity Alpha 为测试构筑而默认提供 50 点，正式成长曲线以后再决定

**推荐：** B

**Director 回答：** B

**已确认结论：**
在 **Build Diversity Alpha** 中，角色默认获得 **50 个可分配 Passive Points**，用于立即验证大规模天赋路径、Build 分化和装备/技能联动。

该数字 **不是正式产品的新角色初始点数承诺**。正式游戏的初始点数、升级获取、任务奖励、消耗品获取等成长曲线，在未来角色成长阶段单独决定。

---

## 讨论记录规则

从此版本开始：
1. 所有规划讨论中的关键 Q&A 都写入本文档。
2. 每个问题至少记录：问题、候选/语境、推荐、Director 回答、最终结论。
3. 未决问题明确标记为“待确认”，不得被规划 AI 当作既定设计。
4. 每次讨论回复同步更新本文档，并提供最新版下载。
5. 最终短期规划完成后，此 QA 文档作为规划决策依据随规划一并保留。


### Q2：POE Passive Tree 是否锁定一个固定版本作为 Canonical Snapshot？
**问题：**
新版短期规划需要明确 POE 天赋树的数据来源策略。可选方向：

- **A：动态跟随 POE 最新版本。**  
  规划 AI 每次发现 POE 更新后，都可以把本项目天赋树同步到最新版。

- **B：锁定一个明确版本作为 Canonical Snapshot。**  
  在本轮 Build Diversity Alpha 内，拓扑、节点属性、功能与特性均以该固定版本为基准；除非 Director 明确批准升级基准，否则 POE 后续改版不得自动改变本项目天赋树。

- **C：不锁版本，只抽取 POE 天赋树中与当前项目已经实装系统相关的部分。**

**推荐：** B

**推荐理由：**
1. 防止外部游戏更新导致本项目 Runtime、测试与平衡基线持续漂移。
2. 让规划 AI / 工作 AI 拥有稳定、可复现的数据源。
3. 尚未实现的节点可以保留在固定树中并标记“未实装”，不会因为当前系统不支持而破坏完整树结构。
4. 以后若决定同步新 POE 版本，可以作为一次显式 Migration 工作，而不是隐式自动更新。

**当前可验证候选基准：**
Path of Exile 1 — **3.28.0 Mirage**。

**Director 回答：** B

**已确认结论：**
本轮 Build Diversity Alpha 锁定 **Path of Exile 1 — 3.28.0 Mirage** 作为 Passive Tree 的 **Canonical Snapshot**。

规划 AI 与工作 AI 不得因为 POE 后续版本更新而自动迁移本项目的天赋树。任何版本升级必须由 Director 明确批准，并作为一次独立的 Passive Tree Migration 工作处理。

外部基准版本一经锁定，本轮所有树拓扑、节点数据、机制映射与“未实装”清单，都应能够追溯到该快照。


---

## 讨论节奏新增规则

Director 要求：从 Q3 开始，**每轮固定提出 4 个关键决策分支**，允许 Director 一次回答四项。  
每个分支都必须给出推荐项和理由；不得把能够从仓库、已锁定设计或权威外部基准中直接确定的事实伪装成问题。

---

### Q3：POE Passive Tree 的“复制”边界如何定义？
**背景：**
Director 已确认属性、功能、特性直接以 POE 3.28.0 为实现基准。但还需要区分“玩法数据/机制”与“表现资产/文本”，否则规划 AI 可能把图标、背景、美术、原始文案也视为必须复制。

**选项：**
- **A：完全镜像。** 拓扑、数值、功能、节点名称、节点说明、美术图标、树背景等全部直接复用。
- **B：机制高保真，表现原创。** 拓扑、数值、属性、功能、节点依赖和机制语义按 3.28.0 实现；玩家可见的美术、图标、背景、品牌化文本与必要的命名采用本项目自己的表现层。机械说明从结构化数据生成。
- **C：仅参考机制。** POE 只作为灵感，不要求数值或拓扑一致。
- **D：仅内部完全镜像，发布前再统一替换表现层。**

**推荐：** B

**推荐理由：**
保持 Director 要求的玩法与机制高保真，同时让“数据层/规则层”和“表现层”天然解耦，避免后续为了替换表现资产再重构 Runtime。对于一个未来可能公开发布的项目，这也比直接把第三方美术和品牌化文案带入工程更稳妥。

**Director 回答：** A

**已确认结论：**
本轮对 POE 3.28.0 Passive Tree 采用 **完全镜像** 口径：拓扑、数值、属性、功能、节点名称、节点说明以及相关表现资产均纳入镜像目标。

**显式风险：**
该决定会引入第三方知识产权与公开发布风险。此风险不得被规划 AI 静默忽略，后续必须单独确认“开发阶段镜像”与“公开发布版本”的边界。

---

### Q4：暂未实装的 Passive 节点在树上如何工作？
**背景：**
完整 POE 树必然包含本项目当前没有的机制。如果简单禁用节点，可能直接切断路径；如果隐藏节点，则树拓扑不再等同于 Canonical Snapshot。

**选项：**
- **A：隐藏未实装节点。** 不显示、不可分配。
- **B：显示但锁定。** 标“未实装”，不可分配；可能导致部分路径暂时不可达。
- **C：保留并允许分配。** 已支持的 modifier 正常生效；不支持的 modifier 行明确显示“未实装/无效果”。如果整个节点都未支持，仍可消耗点数作为路径节点，但不给未实现效果。
- **D：用临时近似效果替代。** 保持节点可玩性，等正式机制完成后再替换。

**推荐：** C

**推荐理由：**
这样最能保持 POE 3.28.0 原始拓扑和 50 点路径测试的真实性，也不会迫使工作 AI 为了“让树连通”提前实现 Aura、Block、ES、Charge 等所有系统。未实装效果必须在 UI、测试报告和支持矩阵中同时显式标识，禁止静默失效。

**Director 回答：** C

**已确认结论：**
暂未实装 Passive 节点 **保留、显示并允许分配**，用于保持 Canonical Tree 的路径连续性。
- 已实现的 modifier/effect 正常生效。
- 未实现的 modifier/effect 必须明确标记“未实装/无效果”。
- 若整个节点都尚未支持，仍允许消耗点数作为路径节点，但不得伪造临时效果。
- UI、测试报告、支持矩阵必须一致标识，禁止静默失效。

---

### Q5：第一轮导入整棵树后，角色从哪里开始？
**背景：**
长期目标是“职业只是起点 + 共享天赋树”，但当前游戏只有一个正式角色。如果同时实现所有 POE 职业语义，会把本轮从 Build Diversity 扩大到 Character/Class System。

**选项：**
- **A：完整导入树，并立即开放所有 POE 起始位置给玩家选择。**
- **B：完整导入整棵树和所有起始锚点数据，但当前角色只启用一个指定起点；其他起点保留为 dormant metadata，不在本轮引入职业系统。**
- **C：只导入当前起点附近约 50～80 点范围，后面逐步扩树。**
- **D：完整导入，但当前角色统一从树中央或自定义中立起点开始，不采用 POE 职业起点。**

**推荐：** B

**推荐理由：**
数据层一次建立完整 Canonical Tree，避免以后重新导入和迁移；Gameplay 层却仍然只服务一个角色，不提前制造职业系统。以后增加职业时，可以直接激活已有起始锚点。

**Director 回答：** B

**已确认结论：**
完整导入 POE 3.28.0 Passive Tree 及所有起始锚点数据，但当前角色只启用一个指定起点。
其他起始点保留为 dormant metadata，本轮不因此启动完整职业系统；未来新增职业/角色时可显式激活。

---

### Q6：Build Diversity Alpha 中 50 点的洗点规则如何定义？
**背景：**
本轮 50 点是测试预算，不是正式成长曲线。为了真正比较多个 Build，需要决定测试过程中是否允许无成本快速重构。

**选项：**
- **A：50 点 + 完全免费无限洗点/整树重置。** 本轮只验证 Build，不模拟正式 Respec Economy。
- **B：50 点，但已分配点不能回退。** 想测试新 Build 必须重新开局。
- **C：从 0 点逐步获得到 50 点，同时验证升级成长过程。**
- **D：50 点 + 免费洗点，并额外要求 Build Preset/快照切换系统作为本轮硬性功能。**

**推荐：** A

**推荐理由：**
当前目标是验证树、装备、Support、技能与 Aura 的组合空间，而不是验证升级或洗点经济。A 的实现最小，又能最大幅度降低人工测试不同 Build 的成本。Build Preset 很有价值，但更适合作为非阻塞测试工具，而不是阻塞核心 Gameplay 的硬 Gate。

**Director 回答：** A

**已确认结论：**
Build Diversity Alpha 中采用 **50 点 + 完全免费、无限洗点/整树重置**。
该规则服务于快速测试构筑，不代表正式游戏 Respec Economy。
Build Preset / 快照切换属于高价值 **非阻塞测试工具**，可以在不影响主 Gate 的前提下安排。


---

### Q7：Q3“完全镜像”与未来公开发布之间如何划界？
**背景：**
Q3 已选择完全镜像 POE 3.28.0 的树，包括名称、说明和表现资产。这个选择适合作为高保真内部研发基准，但若未来公开发布/商业化，第三方 IP 风险会显著提高。

**选项：**
- **A：仅内部研发镜像。** 任何公开演示前必须全部替换第三方表现与受保护内容。
- **B：开发和未来公开版本都保持完全镜像。**
- **C：本轮允许完全镜像以提高研发效率，但把“Public Release IP Scrub”设为未来公开发布前的强制 Gate；在该 Gate 之前不阻塞 Build Diversity Alpha。**
- **D：暂不处理发布边界，直到游戏接近发布。**

**推荐：** C

**主要权衡：**
保留当前研发速度与高保真基准，同时把公开发布风险显式隔离成未来 Gate；不会让法律/IP 清理阻塞当前短期研发。

**Director 回答：** B

**已确认结论：**
Director 明确决定：**开发阶段与未来公开版本均保持 POE 3.28.0 Passive Tree 的完全镜像口径**，不设置发布前表现层替换 Gate。

这是对推荐方案 C 的明确覆盖。规划 AI 不得自行将其改写为“仅内部研发镜像”或自动加入替换任务。

**显式风险（保留）：**
该决定具有显著第三方知识产权、授权与公开发布风险。风险记录必须持续保留；除非 Director 后续修改决策，否则该风险本身不构成本轮 Build Diversity Alpha 的阻塞项。

---

### Q8：本轮“补齐装备槽”的目标形态是什么？
**背景：**
当前正式槽位已有 Weapon / Body / Helmet / Boots / Gloves / Belt。Director 已要求补齐，并与技能联调。

**选项：**
- **A：补到 10 个标准核心槽位：Weapon、Offhand、Helmet、Body、Gloves、Boots、Belt、Amulet、Ring1、Ring2；本轮不做 Weapon Swap。两手武器/副手占用、武器标签与技能装备要求一并联调。**
- **B：在 A 基础上同时加入 Weapon Swap、Quiver 等更完整武器配置语义。**
- **C：只补 Amulet + Ring1 + Ring2，Offhand 继续延后。**
- **D：数据层支持完整槽位，但本轮 UI 和 Gameplay 只启用一部分。**

**推荐：** A

**主要权衡：**
能真正验证装备—技能—Affix 联动，又避免 Weapon Swap 等次级复杂度把短期阶段扩大成完整装备系统重构。

**Director 回答：** A

**已确认结论：**
本轮补齐到 10 个核心装备槽：
Weapon、Offhand、Helmet、Body、Gloves、Boots、Belt、Amulet、Ring1、Ring2。

同时必须联调：
- 两手武器/副手占用规则
- 武器标签
- 技能装备要求
- Affix applicability
- 装备变更对技能与 Build 的实际影响

本轮 **不把 Weapon Swap 作为硬性范围**。

---

### Q9：Build Diversity Alpha 最终至少要证明多少个“真正不同的 Build”？
**背景：**
如果没有明确验收数量，规划 AI 很容易把“多几个数值变体”当成 Build Diversity 完成。

**选项：**
- **A：至少 3 个 Reference Builds；每个只需一个核心维度不同。**
- **B：至少 4 个 Reference Builds；每个必须在至少 3 个维度上形成实质差异，例如主伤害机制/技能行为、Passive 路线、装备 Affix 优先级、Aura/Reservation、关键防御或资源取舍。**
- **C：至少 6 个 Reference Builds，追求更强展示效果。**
- **D：不固定人工 Build，完全靠组合模拟器证明多样性。**

**推荐：** B

**主要权衡：**
4 个足以暴露“看似有树其实只有一种最优路线”的问题，同时还不至于为了凑 6 个 Build 过早增加大量技能和系统。

**Director 回答：** B

**已确认结论：**
Build Diversity Alpha 最终至少需要证明 **4 个 Reference Builds**。

每个 Reference Build 必须至少在 **3 个实质维度**上与其他 Build 形成差异，不能仅靠数值微调冒充不同构筑。可计入差异的维度包括：
- 主伤害/技能行为
- Passive 路线与关键节点
- 装备与 Affix 优先级
- Aura / Reservation 组合
- 关键防御或资源取舍
- 其他由 Director 批准的机制差异

自动模拟可以提供证据，但不能完全取代这 4 个人工可解释的 Reference Builds。

---

### Q10：这一短期阶段要不要新增 Active Skill？
**背景：**
已确认优先“构筑深度”而非“技能数量”。但装备、Passive 和 Aura 联调可能暴露现有 3 个 Active 无法覆盖某类关键机制的情况。

**选项：**
- **A：严格冻结现有 3 个 Active，本阶段绝不新增。**
- **B：默认冻结；只有当某个已批准 Build Diversity 验收目标无法由现有技能表达时，规划 AI 才能提出/实现必要的新 Active，且本阶段最多新增 2 个。**
- **C：主动扩到 6～8 个 Active，再做构筑验证。**
- **D：直接开始按 POE 技能体系批量导入 Active。**

**推荐：** B

**主要权衡：**
把技能扩张变成“由验证需求驱动”的例外，而不是内容生产主线；既不锁死测试能力，也能防止 AI 用加技能掩盖底层构筑深度不足。

**Director 回答：** B

**已确认结论：**
本阶段默认冻结 Active Skill 数量，不主动把“技能数量扩张”作为主线。

仅当某个已批准的 Build Diversity 验收目标无法由现有 3 个 Active Skill 表达时，规划 AI 才能批准必要的新 Active Skill；整个阶段最多新增 **2 个**。

新增技能必须由验证缺口驱动，并与装备、Support、Passive、Aura/Reservation 联调，禁止为了内容数量而扩张。


---

### Q11：Aura / Reservation 第一版要做到多深？
**背景：**
新版短期目标 B 已确认 Aura / Reservation 是本阶段第一个新增核心构筑系统。若范围不锁定，工作 AI 很容易直接扩成完整 POE Aura 生态。

**选项：**
- **A：只做最小系统。** 统一 Reservation 百分比 + 2 个 Aura，证明“占用资源换持续收益”即可。
- **B：做可验证的第一版体系。** 支持百分比 Reservation、多个 Aura 同时启用、Reservation Efficiency、Aura Effect、作用范围/自身效果边界，并至少提供 4 个具有不同 Build 取舍的 Aura；暂不做复杂触发型 Reservation 或大量特殊例外。
- **C：直接镜像 POE 3.28.0 的全部 Aura / Reservation 相关技能与机制。**
- **D：只建立底层接口和测试，不做玩家可用 Aura。**

**推荐：** B

**推荐理由：**
本阶段需要的是让 Aura 真正成为构筑维度，而不是打勾式最小实现；但直接全量镜像会把短期计划拉成内容搬运阶段。

**Director 回答：** B

**已确认结论：**
Aura / Reservation 第一版必须成为真正的构筑维度，而不是仅完成最小接口。
本轮至少支持：
- 百分比 Reservation
- 多个 Aura 同时启用
- Reservation Efficiency
- Aura Effect
- 作用范围/自身效果边界
- 至少 4 个具有明显不同 Build 取舍的 Aura

暂不因此自动扩张到复杂触发型 Reservation 或 POE 全量 Aura 生态。

---

### Q12：Affix 扩张采用“数量目标”还是“构筑覆盖目标”？
**背景：**
当前 Affix 只有十几条。进入 Build Diversity Alpha 后，单纯规定“扩到 30/50 条”容易产生很多低价值同质词缀。

**选项：**
- **A：设固定数量，例如至少 40 条 Affix。**
- **B：以构筑覆盖矩阵为主。** 每个 Reference Build 必须拥有若干明确的进攻、防御、资源/效用装备追求；数量只设下限，不作为完成标准。
- **C：直接镜像 POE 3.28.0 的装备词缀池。**
- **D：暂不扩 Affix，只依赖 Passive 和 Aura 做差异。**

**推荐：** B

**推荐理由：**
能迫使工作 AI 证明“装备真的参与 Build”，而不是为了达成数量 KPI 批量生产 +X% 数值词缀。可在正式规划中再给一个最低数量底线。

**Director 回答：** B

**已确认结论：**
Affix 扩张采用 **Build Coverage Matrix** 驱动，而非纯数量 KPI。
每个 Reference Build 都必须拥有明确的：
- 进攻向装备追求
- 防御向装备追求
- 资源/效用向装备追求

Affix 总量可以设置最低底线，但“达到多少条”不能单独构成阶段完成条件。

---

### Q13：非阻塞项目机制采用什么强制规则？
**背景：**
Director 已明确要求“特别注意设置非阻塞项目”。需要把这件事变成规划 AI 的硬协议，而不是一句口号。

**选项：**
- **A：每个阶段只维护一个主任务；阻塞时规划 AI 自由选择别的工作。**
- **B：每个工作包都必须同时定义 Primary Path + 至少 2 个 Non-Blocking Tasks + Blocked Resume Trigger。主任务阻塞后，工作 AI 自动切换非阻塞项；达到恢复条件后再回主任务。**
- **C：建立一个全局 Backlog，阻塞时随机取最高优先级任务。**
- **D：任何阻塞都暂停并请求 Director。**

**推荐：** B

**推荐理由：**
它最符合你当前的无人监管 AI 工作模式，而且可审计、可恢复，不会因为“自由选择”让工作 AI 越做越偏。

**Director 回答：** B

**已确认结论：**
所有正式工作包必须强制包含：
1. Primary Path
2. 至少 2 个 Non-Blocking Tasks
3. Blocked Resume Trigger

主任务阻塞时，工作 AI 不得停摆等待；必须记录阻塞原因、证据与恢复条件，然后自动切换到非阻塞任务。
当恢复触发条件满足后，规划 AI 应重新把主任务拉回执行队列。

---

### Q14：阶段 Gate 中“好不好玩”的人工验收怎么设置？
**背景：**
自动测试、性能 Gate、数据验证可以无人执行，但 Build Diversity 和战斗体验最终仍需要 Director 判断。若人工 Gate 太频繁，会让项目阻塞；太少又可能让 AI 长时间在错误方向上扩张。

**选项：**
- **A：每个小阶段都必须等 Director 试玩确认后才能继续。**
- **B：只在 3 个关键体验节点设置 Director Gate：装备/技能联调完成、Passive Tree + 50 点完整可玩、4 Reference Builds + Aura 最终验收；其他阶段硬 Gate 全绿即可自动推进，并继续非阻塞工作。**
- **C：整个短期计划只有最终一次 Director Gate。**
- **D：完全取消人工 Gate，由测试和模拟决定是否完成。**

**推荐：** B

**推荐理由：**
既保留真正需要人的“构筑是否有意义/是否好玩”判断，又不会让规划 AI 每完成一个小任务就停下来等你。

**Director 回答：** C

**已确认结论：**
整个 Build Diversity Alpha **只在最终收口设置一次 Director 体验 Gate**。
装备/技能联调、Passive Tree 完整可玩等中间里程碑不等待 Director 人工试玩，硬 Gate、自动测试和 Reviewer 通过后即可继续推进。

规划 AI 必须避免因“等待人工体验确认”造成中途停摆；若存在体验风险，应记录为风险/待最终验收项，并继续执行不依赖该判断的工作。


---

### Q15：Passive Tree 的外部数据应以什么形式进入仓库？
**背景：**
已锁定 POE1 3.28.0 为 Canonical Snapshot，并要求整棵树高保真镜像。如果让工作 AI 直接把外部网页/第三方结构当运行时依赖，会破坏 Repository-as-Memory 和可复现性。

**选项：**
- **A：运行时或每次构建时在线读取 POE 数据。**
- **B：把 3.28.0 树解析成仓库内固定、版本化的 Canonical Data Snapshot，并保存来源、解析器版本、哈希与导入报告；Runtime 只读取仓库内数据。**
- **C：直接手工录入树节点，不保留原始快照。**
- **D：由规划 AI 每次需要时重新抓取和生成。**

**推荐：** B

**推荐理由：**
保证离线、可复现、可审计；以后迁移新 POE 版本也可以明确做 diff，而不会发生数据源漂移。

**Director 回答：** B

**已确认结论：**
POE1 3.28.0 Passive Tree 必须以 **仓库内固定、版本化 Canonical Data Snapshot** 的形式保存。
快照至少保存：
- 数据来源
- 基准版本
- 解析器/导入器版本
- 内容哈希
- 导入报告
- 差异/异常记录

Runtime 只依赖仓库内快照，不依赖外部网站或构建时在线抓取。未来升级 POE 版本必须走显式 Migration。

---

### Q16：未实装 Passive 效果的“完成进度”如何管理？
**背景：**
Q4 已确认未实装节点可分配、未实现部分明确无效果。随着整棵树导入，未实现机制会非常多；如果没有统一支持矩阵，规划 AI 很容易重复实现或误判完成。

**选项：**
- **A：只在节点 UI 上显示“未实装”，不额外维护清单。**
- **B：建立 Passive Mechanic Support Matrix：每类机制标记 Supported / Partial / Unsupported / Blocked，并关联测试、Runtime owner、依赖系统与可启用节点数。**
- **C：每个节点单独建任务追踪。**
- **D：先全部视为 Unsupported，等以后一次性处理。**

**推荐：** B

**推荐理由：**
按“机制族”追踪比几千个节点逐个管理更可维护，也更适合规划 AI 判断下一批实现什么能最大化 Build Coverage。

**Director 回答：** B

**已确认结论：**
建立 **Passive Mechanic Support Matrix**，按机制族而不是逐节点追踪实现状态。
每类机制至少标记：
- Supported
- Partial
- Unsupported
- Blocked

并关联：
- Runtime/系统依赖
- 自动测试
- 受影响节点数量
- 当前可启用节点数
- 阻塞原因与恢复条件

规划 AI 必须以该矩阵作为 Passive 实现推进和非阻塞任务选择的重要依据。

---

### Q17：装备 + 技能联调的武器规则，本轮要覆盖到什么程度？
**背景：**
Q8 已确认补齐 10 个核心槽并处理 Offhand、两手占用、武器标签和技能装备要求。还需要决定“武器类型”本轮是不是只做够测试，还是正式建立长期骨架。

**选项：**
- **A：只实现当前 3 个 Active Skill 用得到的最少武器类型。**
- **B：建立一套稳定的 Weapon Family / Handedness / Requirement 数据模型，并实现一批足以覆盖现有技能和 4 Reference Builds 的代表性武器类型；不追求 POE 全武器库。**
- **C：直接完整镜像 POE 3.28.0 全武器类型与基础物品。**
- **D：暂时取消技能的武器限制，只验证槽位。**

**推荐：** B

**推荐理由：**
这样能真正验证“装备决定技能可用性/构筑路线”的长期设计，同时避免把 Build Diversity Alpha 变成 Base Item 内容搬运阶段。

**Director 回答：** B

**已确认结论：**
建立稳定的 **Weapon Family / Handedness / Requirement** 数据模型。
本轮只实现足够覆盖现有技能与 4 个 Reference Builds 的代表性武器类型，不追求 POE 全武器库。

装备—技能联调必须验证：
- 武器类型要求
- 单手/双手/副手占用
- Skill 可用性
- Affix applicability
- 更换武器后的 Build 结果
- 非法组合的验证与 UI 反馈

---

### Q18：Build Diversity Alpha 的最终硬性验收是否要求“自动可重复证明”？
**背景：**
Q9 已确认至少 4 个 Reference Builds，Q14 又把 Director Gate 压到最终一次。因此在最终人工试玩前，需要规划 AI 能客观证明每个 Build 确实成立，而不是只保存一套配置截图。

**选项：**
- **A：只要 4 个 Build 能手工配置并进入地图即可。**
- **B：每个 Reference Build 都必须有版本化 Build Definition + 固定 Seed 验证场景 + 自动战斗/数值测试 + 关键指标报告；每次相关 Runtime 改动都能重跑回归。**
- **C：只记录 DPS / EHP 两个数字。**
- **D：完全由最终 Director 试玩判断，不建立自动 Build 验证。**

**推荐：** B

**推荐理由：**
这是“只设最终一次人工 Gate”能够安全成立的前提。自动证据不能判断好不好玩，但能阻止 AI 在中途悄悄把某个 Reference Build 做坏。

**Director 回答：** B

**已确认结论：**
4 个 Reference Builds 都必须具备 **自动、可重复、可版本化验证**：
- Build Definition
- 固定 Seed 验证场景
- 自动战斗/数值测试
- 关键指标报告
- Runtime 相关变更后的回归能力

自动证据用于证明 Build 没有被技术回归破坏；“是否有趣、取舍是否成立”仍由最终唯一一次 Director Gate 判断。


---

### Q19：当前唯一角色在 POE 3.28.0 Passive Tree 上启用哪个起始锚点？
**背景：**
Q5 已确认整棵树与全部职业起始锚点都导入，但当前只有一个角色，只启用一个起点。Build Diversity Alpha 又要求默认 50 点并证明 4 个不同 Reference Builds。

**选项：**
- **A：Marauder 起点。** 强化当前偏近战/力量角色身份，但 50 点范围内跨区成本较高。
- **B：Duelist 起点。** 在力量/敏捷、近战、攻击与防御之间较均衡。
- **C：Scion 起点。** 位于树中心，更适合用同一个角色和 50 点预算快速验证不同方向的 Build；其他职业起点保持 dormant。
- **D：不预先指定，由规划 AI 根据现有 3 个 Active Skill 的实际兼容性自动选择最能覆盖 4 个 Reference Builds 的起点，并把选择结果记录为阶段决策。**

**推荐：** C

**推荐理由：**
本轮目标是验证 Build Diversity，而不是验证职业身份。中央起点能显著降低“50 点全花在赶路上”的概率，也最适合一个角色覆盖四种构筑。正式职业起点以后仍可重新决定。

**Director 回答：** C

**已确认结论：**
当前唯一角色在本轮 Build Diversity Alpha 中启用 **Scion 中央起点**。
其他 POE 3.28.0 职业起始锚点仍完整保存在 Canonical Snapshot 中，但保持 dormant，不因此启动职业系统。
该选择服务于“50 点快速覆盖多种构筑路线”的测试目标，不自动定义未来正式角色的出生位置。

---

### Q20：Affix Coverage Matrix 之外，本轮设多少条 Affix 作为最低规模底线？
**背景：**
Q12 已确认 Affix 以构筑覆盖为完成标准，但仍需要一个最低内容量，避免工作 AI 用极少数高度定制词缀“刚好拼出四个测试 Build”。

**选项：**
- **A：至少 24 条。** 增幅较小，最快完成。
- **B：至少 40 条。** 在当前基础上形成足够的装备选择空间，同时仍属于短期可控规模。
- **C：至少 60 条。** 更接近第一次真正的装备池，但内容与平衡成本明显上升。
- **D：完全不设数量底线，只看 Coverage Matrix。**

**推荐：** B

**推荐理由：**
40 条不是“完成 KPI”，只是防止过拟合 4 个 Reference Builds 的最低广度。真正 Gate 仍是每个 Build 都有进攻、防御、资源/效用追求，并且不同装备槽存在有意义的选择。

**Director 回答：** B

**已确认结论：**
Affix 除 Build Coverage Matrix 外，设置 **至少 40 条** 的最低规模底线。
40 条仅用于防止测试内容过拟合 4 个 Reference Builds；达到 40 条本身不能构成完成条件。
最终仍必须证明每个 Build 都有进攻、防御、资源/效用方向的真实装备选择。

---

### Q21：第一批至少 4 个 Aura 的内容来源如何定义？
**背景：**
Q11 已确认 Aura / Reservation 第一版至少提供 4 个具有不同取舍的 Aura，但还没规定这些 Aura 是原创还是继续采用 POE 高保真策略。

**选项：**
- **A：从 POE1 3.28.0 选择 4 个与本项目当前已支持 Combat Math 最匹配的代表性 Aura，高保真实现其数值/功能/名称/表现；优先覆盖物理、火焰、Armour、Evasion 等当前已有机制。**
- **B：只参考 POE 机制，由本项目原创 4 个 Aura。**
- **C：直接导入 POE1 3.28.0 全部 Aura 与 Reservation Skill。**
- **D：由规划 AI 自由选择原创或镜像，只要通过测试即可。**

**推荐：** A

**推荐理由：**
与已经锁定的 Passive Tree 高保真策略一致，同时只选当前系统能真正支撑的 4 个代表性 Aura，不会因为一个 Aura 阶段被迫提前实现 Cold、Energy Shield、Minion 等整套外部机制。

**Director 回答：** A

**已确认结论：**
首批至少 4 个 Aura 从 **POE1 3.28.0** 中选择与当前项目已支持 Combat Math 最匹配的代表性 Aura，高保真实现其数值、功能、名称与表现。
优先使用当前已有的 Physical、Fire、Armour、Evasion 等机制，避免为了 Aura 本身提前拉入大量尚不存在的核心系统。

---

### Q22：Build Diversity Alpha 扩张完成后，性能 Gate 如何处理？
**背景：**
现有项目已经建立 1440p / 120 FPS 的锁定硬件性能基线。新增完整 Passive Tree UI、10 装备槽、更多 Affix、Aura 与 Build Runtime 后，需要决定是否把“不得倒退”继续设为硬 Gate。

**选项：**
- **A：只重跑现有战斗 Performance Gate；只要原场景继续通过即可。**
- **B：保留现有锁定硬件 1440p / 120 FPS 战斗 Gate，同时新增 Passive Tree UI/大数据加载与 Build 切换的性能预算；出现回归时冻结相关扩张并优化，但其他不依赖该阻塞的任务继续执行。**
- **C：本阶段不设性能硬 Gate，等内容更多后统一优化。**
- **D：把目标临时降低到 60 FPS，以便更快增加系统。**

**推荐：** B

**推荐理由：**
完整 Passive Tree 最容易引入的未必是战斗 CPU 问题，也可能是 UI、数据加载、重算与 GC 问题。B 能守住已经证明过的性能成果，又与已确认的非阻塞协议兼容。

**Director 回答：** C

**已确认结论：**
Build Diversity Alpha **不设置性能硬 Gate**。
已有性能基线、性能测量和回归信息仍应保留并可持续记录，但性能回归不得自动冻结整个 Build Diversity 主线。

若出现性能问题：
- 记录问题、证据、影响范围与建议修复；
- 可进入 Non-Blocking / Technical Debt 工作池；
- 不因未达到既有 1440p/120 FPS 目标而阻塞本短期计划的系统与构筑验证；
- 未来重新进入专门性能阶段时再恢复性能硬 Gate。


---

### Q23：首批 4 个 Aura 是否直接锁定为 Pride / Anger / Determination / Grace？
**背景：**
Q21 已确认从 POE1 3.28.0 选择至少 4 个、与当前 Combat Math 相匹配的 Aura。当前项目已经具备 Physical、Fire、Armour、Evasion 等基础，因此这四个 Aura 可以分别形成进攻与防御方向的明显构筑取舍。

**选项：**
- **A：锁定 Pride、Anger、Determination、Grace。** 数值与规则按 3.28.0 Canonical Snapshot 高保真实现。
- **B：只锁定“物理进攻 / 火焰进攻 / Armour 防御 / Evasion 防御”四个功能位，具体 Aura 名称由规划 AI 从 3.28.0 中选择。**
- **C：让规划 AI 完全自由选择 4 个 POE Aura，只要求当前系统可实现。**
- **D：一次扩到 6 个 Aura，再覆盖更多方向。**

**推荐：** A

**推荐理由：**
这四个定位清晰、与当前 Combat Math 的重合度高，也能直接服务 4 个 Reference Builds；锁死内容可以减少规划 AI 在 Aura 选型上的反复决策。

**Director 回答：** A

**已确认结论：**
首批 Aura 直接锁定为 **Pride / Anger / Determination / Grace**。
数值、功能、名称与表现按 POE1 3.28.0 Canonical Snapshot 的高保真口径实现。

它们分别服务于：
- Physical 进攻
- Fire 进攻
- Armour 防御
- Evasion 防御

规划 AI 不再对首批 Aura 选型进行二次决策。

---

### Q24：4 个 Reference Builds 的具体构筑方案由谁决定？
**背景：**
已经锁定至少 4 个 Reference Builds，且每个至少在 3 个实质维度上不同。但目前仓库已有 3 个 Active Skill，规划还允许在确有验证缺口时最多新增 2 个。如果现在强行指定四套 Build，可能与现有技能实际能力错位。

**选项：**
- **A：Director 现在逐个指定四套完整 Build。**
- **B：规划 AI 在完成现有技能/Support/Combat Math 审计后提出并锁定 4 个 Reference Builds；必须覆盖明显不同的伤害/防御/装备/Passive/Aura 路线，且遵守“最多新增 2 个 Active”的限制。无需中途等 Director 审批，只要规则满足即可执行，最终统一由 Director 验收。**
- **C：直接选择 4 个经典 POE Build 进行复刻。**
- **D：不预定义 Build，让随机模拟器自己发现四个表现最好的组合。**

**推荐：** B

**推荐理由：**
让 Reference Builds 从仓库实际能力长出来，同时保持最终可解释、可回归；也符合已经确认的“全阶段只有最终一次 Director Gate”。

**Director 回答：** B

**已确认结论：**
4 个 Reference Builds 由 **规划 AI** 在审计现有 Active Skill、Support、Combat Math、装备与 Passive 支持状态后提出并锁定。
要求：
- 至少 4 个；
- 每个至少在 3 个实质维度上与其他 Build 不同；
- 优先复用现有 3 个 Active Skill；
- 只有验证缺口确实无法覆盖时才允许新增 Active Skill，且本阶段最多新增 2 个；
- 无需中途等待 Director 审批；
- 最终统一进入唯一一次 Director 体验 Gate。

---

### Q25：装备 Base Item 是否也采用 POE 全量镜像策略？
**背景：**
Passive Tree 已明确完全镜像 POE 3.28.0，但装备体系目前只确认补齐 10 个槽位、建立 Weapon Family / Handedness / Requirement 和至少 40 条 Affix。尚未决定 Base Item 是否也要在本轮全量镜像 POE。

**选项：**
- **A：本轮直接全量镜像 POE 3.28.0 Base Items、名称、基础属性、需求和武器类型。**
- **B：机制结构参考 POE，但本轮只建立能覆盖 10 个槽位、现有技能与 4 个 Reference Builds 的代表性 Base Item 集合；不追求全量。**
- **C：武器 Base Items 全量镜像，护甲与首饰只做代表性集合。**
- **D：只保留现有装备 Base，不新增 Base Item 内容，只增加槽位。**

**推荐：** B

**推荐理由：**
本轮核心是验证装备—技能—Passive—Aura 的 Build 联动；全量 Base Item 会把短期目标明显推向内容搬运，而不会成比例提高构筑验证质量。

**Director 回答：** B

**已确认结论：**
Base Item 不在本轮全量镜像 POE。
本轮建立：
- 覆盖 10 个核心装备槽位；
- 覆盖现有技能；
- 覆盖 4 个 Reference Builds；
- 符合 Weapon Family / Handedness / Requirement 模型

所需的代表性 Base Item 集合。

Base Item 的目标是支持 Build 验证，而不是完成 POE 全量内容迁移。

---

### Q26：现有 S4 未收口工作如何并入新版短期计划？
**背景：**
仓库当前仍处于 S4，前面阶段已经完成，尚有生产规模模拟/收口类工作。新版短期规划将替代旧“简化版”，需要防止规划 AI 同时维护两套互相竞争的执行路线。

**选项：**
- **A：必须完整完成现有 S4 全部剩余工作并正式关闭 S4，之后才能启动新短期规划。**
- **B：新版规划生效后立即废止 S4 剩余任务，不再处理。**
- **C：把 S4 尚未完成但对 Build Diversity 有价值的验证/生产模拟吸收为新版计划的 Entry/Non-Blocking 工作；纯粹为了旧阶段收口、且不再提供新证据的工作取消。新版规划成为唯一执行路线。**
- **D：S4 与新版短期规划并行，由规划 AI 自由选择。**

**推荐：** C

**推荐理由：**
既不丢掉已有投入和有价值的生产验证，也避免为了“阶段形式完整”继续做已经失去优先级的旧任务；最重要的是保证规划 AI 只有一份当前执行真相。

**Director 回答：** C

**已确认结论：**
新版短期规划生效后，成为 **唯一当前执行路线**。
现有 S4 尚未完成的工作按以下规则处理：
- 对 Build Diversity、生产规模、数据正确性仍有价值的验证/模拟 → 吸收到新版计划的 Entry 或 Non-Blocking 工作；
- 仅为了旧阶段形式收口、且不再提供新证据的工作 → 取消；
- 禁止 S4 与新版计划长期并行成为两套竞争的任务源。


---

### Q27：完整 Passive Tree 的玩家交互 UI 本轮做到什么程度？
**背景：**
既然整棵 POE 3.28.0 树都会导入，树本身会非常大。如果只“能显示”，50 点构筑测试会非常难用；如果完全复刻全部高级交互，又容易让 UI 工程膨胀。

**选项：**
- **A：只要求完整显示 + 点选/退点。**
- **B：要求完整显示、缩放/平移、节点详情、当前/未实装状态、路径高亮、搜索节点/属性、剩余点数、整树重置；暂不要求复杂规划器、导入导出和多套预设。**
- **C：直接实现接近 POE 完整天赋树 UI，包括搜索、路径规划、预设、导入导出、对比等高级功能。**
- **D：本轮只做开发者调试 UI，正式玩家 UI 后置。**

**推荐：** B

**推荐理由：**
这是“整棵树 + 50 点免费洗点”真正可用的最低体验层，足以支持最终 Director 验收，同时避免把 Build Planner 做成新的独立项目。

**Director 回答：** B

**已确认结论：**
完整 Passive Tree 玩家 UI 本轮要求：
- 完整树显示
- 缩放 / 平移
- 节点详情
- Supported / Partial / Unsupported / Blocked 或“未实装”状态可见
- 路径高亮
- 节点/属性搜索
- 剩余 Passive Points 显示
- 单点退点与整树重置

本轮暂不把复杂 Build Planner、导入导出、多套预设等高级功能设为硬性范围。

---

### Q28：Aura / Reservation 第一版是否引入完整 Mana Resource 语义？
**背景：**
Pride、Anger、Determination、Grace 都需要 Reservation。若项目当前资源系统还没有完整对齐 POE，就需要决定第一版是只做“占用比例”概念，还是正式建立长期资源骨架。

**选项：**
- **A：建立正式 Mana / Reservation 基础模型：Maximum Mana、Current Mana、Reserved Mana、Reservation %、Reservation Efficiency；Aura 默认占用 Mana。暂不做 Life Reservation 和复杂资源替代。**
- **B：只做一个抽象 Reservation Budget，不引入 Mana。**
- **C：直接镜像 POE 的 Mana/Life Reservation、Blood Magic 等完整体系。**
- **D：Aura 暂时不消耗资源，只用于验证效果。**

**推荐：** A

**推荐理由：**
Aura 的核心价值就是资源取舍。A 足够接近长期目标，也不会把 Blood Magic、Life Reservation 等高级机制提前拉进来。

**Director 回答：** A

**已确认结论：**
Aura / Reservation 第一版正式建立 Mana / Reservation 基础模型：
- Maximum Mana
- Current Mana
- Reserved Mana
- Reservation %
- Reservation Efficiency
- Aura 默认占用 Mana

本轮暂不因此启动 Life Reservation、Blood Magic 或复杂资源替代体系。

---

### Q29：Reference Build 的“成立”是否需要量化最低战斗标准？
**背景：**
Q18 已确认 Build 必须自动可回归，但如果没有统一的最低性能门槛，规划 AI 可能生成 4 个“机制不同但明显不能打”的 Build。

**选项：**
- **A：只要求机制差异，不要求战斗强度。**
- **B：为统一验证场景设置 Build Viability Floor：4 个 Build 都必须完成同一固定 Seed 的战斗/地图目标，并达到最低生存与输出门槛；不同 Build 之间允许明显强弱差异，不要求数值平衡。**
- **C：要求 4 个 Build 的 DPS / 生存能力控制在 ±10% 内。**
- **D：完全不设自动强度标准，只在最终 Director Gate 判断。**

**推荐：** B

**推荐理由：**
本轮要证明的是“不同路线都能成立”，不是证明平衡完成。统一最低门槛能防止拿一个残废 Build 凑数量，又不会过早陷入精细平衡。

**Director 回答：** A

**已确认结论：**
4 个 Reference Builds 本轮 **只要求机制与构筑路径形成实质差异，不设置统一战斗强度或通关能力硬门槛**。

自动验证仍需保证基础技术正确性，例如：
- Build Definition 可加载
- Passive 分配合法
- 装备/技能/Aura 配置有效
- 无异常、NaN、非法状态或崩溃
- 关键已支持机制按定义生效

但 DPS、生存能力、固定场景完成度不作为本阶段硬性 Gate。最终“是否真的成立/是否好玩”仍交给唯一一次 Director 体验验收。

---

### Q30：Crafting 在本轮 Build Diversity Alpha 中处于什么位置？
**背景：**
长期规划把深度 Craft 放在更后面的独立阶段，但本轮装备槽、Affix 与 Reference Builds 都会明显扩张。需要区分“构筑测试需要快速组装装备”与“正式 Craft 深化”。

**选项：**
- **A：本轮同步扩展正式 Craft 系统，让玩家可以通过 Craft 自然做出 4 个 Reference Builds。**
- **B：不扩深度 Craft。保留/修补现有基础 Craft；Reference Build 测试允许使用确定性测试装备、Build Definition 或开发工具直接生成目标装备。Craft 只要不因新槽位/Affix 发生回归即可。**
- **C：完全冻结 Craft，相关功能本轮不可使用。**
- **D：直接推进长期规划中的深度 Craft 阶段。**

**推荐：** B

**推荐理由：**
Build Diversity Alpha 的任务是证明“这些装备与 Affix 能产生不同 Build”，不是证明玩家已经能通过完整经济系统稳定做出它们。这样能避免提前把短期计划拖入深度 Craft。

**Director 回答：** B

**已确认结论：**
本轮不进入深度 Craft。
要求：
- 保留并修补现有基础 Craft
- 新装备槽、新 Affix 与数据结构不得破坏现有 Craft
- Reference Build 测试允许通过 Build Definition、确定性测试装备或开发工具直接生成目标装备
- 不要求玩家通过正式 Craft 经济自然制作出 4 个 Reference Builds

深度 Craft 继续留在后续独立阶段。


---

### Q31：Passive 点分配是否合法，是否严格按 POE 原始连通规则？
**背景：**
当前角色从 Scion 起点开始、默认 50 点、可无限免费洗点。完整树导入后，必须明确玩家是否能“跨区点亮”不相连节点，否则路径成本这一核心设计会失真。

**选项：**
- **A：严格采用 POE 原始连通规则。** 新节点必须与已分配路径相连；退点后剩余树仍必须保持从当前起点可达，禁止制造悬空分支。
- **B：Build Diversity Alpha 允许任意节点自由分配，不要求连通，以提高测试速度。**
- **C：正常玩家严格连通，但开发者/自动测试工具可以绕过连通规则直接注入 Build Definition。**
- **D：只要求分配时连通，退点时不验证剩余结构。**

**推荐：** C

**推荐理由：**
玩家体验保持 POE 路径成本真实性；自动测试和 Reference Build 回归又可以快速构造任意状态，不必每次模拟 50 次点击。

**Director 回答：** C

**已确认结论：**
玩家正常操作必须严格遵守 POE 风格 Passive 连通规则：
- 新分配节点必须从当前有效路径可达；
- 退点后剩余已分配节点仍需从启用起点保持可达；
- 禁止制造悬空分支。

开发者工具、自动测试与 Reference Build 回归允许绕过交互式连通校验，直接注入版本化 Build Definition，以提高验证效率。

---

### Q32：POE Passive Tree 中的 Mastery 节点，本轮怎么处理？
**背景：**
3.28.0 树中 Mastery 是大量构筑的重要机制。完整镜像树时，Mastery 如果全部无效，会让不少分支失真；如果全量实现，又可能拉入大量尚未支持的机制。

**选项：**
- **A：Mastery UI 与选择结构完整实现；只实现当前 Combat Math / Aura / Weapon / Defence 已支持机制对应的 Mastery 效果，其余明确标“未实装”。**
- **B：本轮所有 Mastery 全部可见但不可使用。**
- **C：直接全量实现 3.28.0 所有 Mastery 效果。**
- **D：从树中暂时隐藏 Mastery。**

**推荐：** A

**推荐理由：**
保留原始树的核心交互语义，同时继续遵守“按当前机制支持范围渐进实现”的原则。

**Director 回答：** A

**已确认结论：**
Mastery 的结构与交互 UI 在本轮完整保留。
实现策略：
- 当前 Runtime 已支持机制对应的 Mastery 效果正常实现并测试；
- Partial / Unsupported 机制对应的 Mastery 仍可见；
- 尚未实现的具体效果明确标注“未实装/无效果”；
- 不因 Mastery 的存在强制提前实现与本轮 Reference Builds 无关的系统。

---

### Q33：Passive Tree 里的 Jewel Socket 本轮怎么处理？
**背景：**
长期规划把 Jewel 放在后续系统扩张中，但完整 POE 树包含 Jewel Socket。如果处理方式不清楚，规划 AI 可能因为“树完整镜像”提前启动整个 Jewel 系统。

**选项：**
- **A：Jewel Socket 保留、可分配，但明确标记 Jewel 功能未实装；占用 Passive Point 后暂时不提供 Jewel 插槽效果。**
- **B：本轮同步实现基础 Jewel 系统，以保证这些节点具有实际价值。**
- **C：Jewel Socket 显示但不可分配。**
- **D：从 Canonical Runtime Tree 中移除 Jewel Socket，未来再加回来。**

**推荐：** A

**推荐理由：**
与 Q4“未实装节点仍可作为路径节点”完全一致，不让 Jewel 系统提前成为 Build Diversity Alpha 的阻塞项。

**Director 回答：** A

**已确认结论：**
Jewel Socket 节点保留在 Canonical Tree 中并允许正常分配，用于保持原始拓扑与路径成本。
本轮不启动 Jewel 系统；Jewel 插槽效果明确标记为“未实装/无效果”。
Jewel 相关系统不得成为 Build Diversity Alpha 的阻塞项。

---

### Q34：规划 AI 应如何决定“下一批优先实现哪些未支持 Passive 机制”？
**背景：**
完整树导入后 Unsupported/Partial 机制会很多。若没有优先级原则，工作 AI 很容易按节点顺序机械搬运，偏离 Build Diversity 目标。

**选项：**
- **A：按 POE 树节点数量最多的机制优先。**
- **B：按 4 个 Reference Builds 的 Coverage Gap 优先：哪个未支持机制能同时解锁更多 Reference Build 路线、Aura/装备联动或关键节点，就优先实现哪个；与 4 个 Build 无关的机制保持未实装并进入后续 Backlog。**
- **C：按实现难度从简单到困难。**
- **D：完全按照 POE 官方树的数据顺序逐项实现。**

**推荐：** B

**推荐理由：**
它能把“完整树镜像”和“短期不无限扩张”统一起来：数据可以全量存在，Runtime 机制只为当前 Build Diversity 目标按价值展开。

**Director 回答：** B

**已确认结论：**
未支持 Passive 机制的实现优先级由 **4 个 Reference Builds 的 Coverage Gap** 驱动。
规划 AI 应优先选择能：
- 解锁更多 Reference Build 路线；
- 增强装备 / Affix / Aura 联动；
- 让关键 Passive / Mastery / Keystone 从“未实装”转为可用；
- 同时服务多个 Reference Builds

的机制。

与本轮 4 个 Reference Builds 无直接关系的 Unsupported 机制继续保留在后续 Backlog，不得因“整棵树已导入”而机械全量实现。


---

### Q35：POE 的 Ascendancy（升华）是否纳入本轮短期计划？
**背景：**
主 Passive Tree 已锁定为 POE1 3.28.0 完整 Canonical Snapshot，但 POE 的 Ascendancy 属于角色职业进阶体系，会显著扩大 Build Diversity，同时也会把当前阶段带入职业系统。

**选项：**
- **A：本轮完整纳入 Ascendancy，并为当前角色开放一个或多个升华体系。**
- **B：只把 Ascendancy 数据作为外部参考/未来 Backlog，不纳入本轮 Runtime、UI 与 Reference Build 验收。**
- **C：只实现 Scion 对应的 Ascendant，其他 Ascendancy 后置。**
- **D：规划 AI 可以在 4 个 Reference Builds 确实缺少构筑差异时，自行启动部分 Ascendancy。**

**推荐：** B

**推荐理由：**
当前已经有完整 Passive、装备、Affix、Aura 四个主要构筑维度。再引入 Ascendancy 会把短期目标从“证明 Build Diversity”扩成“职业系统 Alpha”，容易失焦。

**Director 回答：** A

**已确认结论：**
Ascendancy 正式纳入本轮 Build Diversity Alpha。
这意味着新版短期规划必须把 Ascendancy 作为构筑维度之一，而不是仅作为未来 Backlog。

但具体需要实现：
- 哪些 Ascendancy
- 当前 Scion 角色如何接入
- 是否需要全部职业升华数据
- Ascendancy Points 如何发放

仍需后续讨论确认。

---

### Q36：Affix 系统本轮是否正式引入 Prefix / Suffix、Tier 与 Mod Group 约束？
**背景：**
Q12 已确认至少 40 条 Affix，并以 Build Coverage Matrix 驱动。如果只做“40 条可随机属性”，装备系统仍然很难体现 POE 式取舍，也会妨碍未来 Craft。

**选项：**
- **A：本轮正式建立 Prefix / Suffix、Tier、Mod Group / Mutual Exclusion、Item Slot Applicability；Item Level 与高级掉落权重后置。**
- **B：只做 Prefix / Suffix，不做 Tier 与 Mod Group。**
- **C：直接镜像 POE 完整 Affix 生成规则，包括 Item Level、权重、Influence 等。**
- **D：继续保持当前简单 Affix 模型，等 Craft 阶段再升级。**

**推荐：** A

**推荐理由：**
这是装备构筑深度真正成立的最低长期骨架，同时不会把本轮拖入完整 POE 掉落经济与高级 Craft。

**Director 回答：** A

**已确认结论：**
Affix 系统本轮正式建立以下长期骨架：
- Prefix / Suffix
- Tier
- Mod Group / Mutual Exclusion
- Item Slot Applicability

Item Level、Influence、完整高级权重/掉落生成规则等更深层内容不自动进入本轮，除非后续另行批准。

---

### Q37：Aura 的作用对象与范围语义，本轮做到什么程度？
**背景：**
首批锁定 Pride / Anger / Determination / Grace。当前是单机单角色项目，尚未确认 Minion / Ally 体系。如果完全照 POE 行为实现 Aura，可能需要提前建立单位阵营、盟友筛选与范围传播。

**选项：**
- **A：建立通用 Aura Emitter / Receiver 与范围筛选模型，支持 Self / Ally / Enemy 三类目标语义；当前没有的 Ally/Minion 内容不额外生产，但底层可承载。**
- **B：只实现当前玩家自身能感知到的效果，不建立通用 Aura 目标模型。**
- **C：为了高保真 Aura，同步加入基础 Minion / Ally 测试单位体系。**
- **D：所有 Aura 暂时都当成玩家自身 Buff 处理。**

**推荐：** A

**推荐理由：**
Pride 本身就涉及对附近敌人的作用语义；如果只做玩家 Buff，很快会产生错误抽象。A 建底层语义但不扩内容，最符合长期可扩张目标。

**Director 回答：** A

**已确认结论：**
Aura 系统建立通用 **Aura Emitter / Receiver** 与范围筛选模型，至少支持：
- Self
- Ally
- Enemy

当前不因这一抽象强制生产 Minion / Ally 内容，但 Runtime 必须能够承载未来单位。
Pride 等对敌 Aura 不得被简化成普通 Self Buff。

---

### Q38：规划 AI 的“任务颗粒度”采用什么强制上限？
**背景：**
新版短期规划会比旧简化版复杂很多。如果规划 AI 每次给工作 AI 一整个阶段，容易造成大批改动、难审计；如果颗粒太小，又会制造大量管理开销。

**选项：**
- **A：每个工作令只允许 1 个主要功能目标 + 明确验收 + 至少 2 个 Non-Blocking Tasks；原则上一次工作令不得同时跨越超过 2 个核心系统。**
- **B：允许工作 AI 一次承包整个阶段，只要最终测试通过。**
- **C：每个 commit 都必须对应一个独立工作令。**
- **D：不设颗粒度规则，由规划 AI 自行判断。**

**推荐：** A

**推荐理由：**
它能维持无人监管开发的可审计性，又不会细化到“一个 commit 一个工作令”的高管理成本；也和你已经确认的 Primary + Non-Blocking + Resume Trigger 协议天然兼容。

**Director 回答：** A

**已确认结论：**
规划 AI 给工作 AI 的每个正式工作令强制遵守：
- 1 个主要功能目标
- 明确验收条件
- 至少 2 个 Non-Blocking Tasks
- 明确 Blocked Resume Trigger
- 原则上一次工作令不得同时跨越超过 2 个核心系统

禁止把完整阶段一次性交给工作 AI 无边界实现，也不要求“一个 commit 一个工作令”。


---

### Q39：既然 Ascendancy 纳入本轮，具体实现范围选哪一种？
**背景：**
当前角色在主 Passive Tree 上采用 Scion 起点，但 Q35 已确认 Ascendancy 正式进入本轮。如果范围不锁，规划 AI 可能直接开始复制所有职业的完整升华体系。

**选项：**
- **A：完整镜像 POE1 3.28.0 全部 Ascendancy 数据与树结构，但本轮当前角色只开放 Ascendant；其他 Ascendancy 作为 dormant data，不进入玩家流程。**
- **B：只实现 Ascendant，不导入其他 Ascendancy 数据。**
- **C：完整镜像全部 Ascendancy，并允许当前角色自由选择任意 Ascendancy。**
- **D：由规划 AI 根据 4 个 Reference Builds 选择需要的 2～4 个 Ascendancy 实现。**

**推荐：** A

**推荐理由：**
和主 Passive Tree 的策略一致：数据层完整镜像，Gameplay 层只激活当前真正需要的部分。这样既保留未来扩展空间，又不会立刻把职业系统全部拉进玩家流程。

**Director 回答：** A

**已确认结论：**
完整镜像 POE1 3.28.0 全部 Ascendancy 数据与树结构。
本轮当前角色只开放 **Ascendant**，其他 Ascendancy 保留为 dormant data，不进入玩家流程。

这与主 Passive Tree 的策略保持一致：数据层完整、Gameplay 层只启用当前角色实际需要的部分。

---

### Q40：Ascendancy Points 在本轮怎么给？
**背景：**
当前普通 Passive Points 已确定为 Alpha 默认 50 点并免费重置。Ascendancy 如果纳入 Build Diversity 验证，也必须有明确测试预算，否则 Reference Builds 很难稳定复现。

**选项：**
- **A：Build Diversity Alpha 默认提供 8 个 Ascendancy Points，并允许免费无限重置；正式获取方式以后再决定。**
- **B：默认 4 点，只验证半套升华。**
- **C：通过临时 Lab/任务流程获取，顺便验证进阶成长。**
- **D：规划 AI 根据具体 Reference Build 自由设置点数。**

**推荐：** A

**推荐理由：**
和“50 普通天赋点用于 Alpha 验证”逻辑完全一致。先验证完整升华构筑价值，不提前做 Lab、任务链或正式成长曲线。

**Director 回答：** A

**已确认结论：**
Build Diversity Alpha 默认提供 **8 个 Ascendancy Points**，并允许免费无限重置。
该规则仅服务于 Alpha 构筑验证，不代表正式游戏的 Ascendancy 获取方式。
Lab、任务链或正式成长曲线不在本轮自动进入范围。

---

### Q41：Affix Tier 本轮是否需要与 Item Level 绑定？
**背景：**
Q36 已确认正式引入 Tier，但如果 Tier 只是一个标签而没有生成约束，其长期意义有限；如果完整复刻 POE Item Level 又会引入更多掉落和内容复杂度。

**选项：**
- **A：Tier 暂时只作为数值层级与互斥结构存在，不要求 Item Level 解锁规则。**
- **B：建立最小 Item Level 规则：每个 Tier 可以声明最低 Item Level，掉落/生成器据此限制；不做 Influence 等高级来源体系。**
- **C：直接完整镜像 POE 3.28.0 的 Item Level / Affix Tier / Weight 生成规则。**
- **D：本轮取消 Tier，等 Craft 阶段再做。**

**推荐：** B

**推荐理由：**
能让 Tier 真正参与装备生成和未来 Craft，又不会立刻扩到 Influence、Essence、Fossil 等高级来源。

**Director 回答：** C

**已确认结论：**
Affix Tier / Item Level / Weight 生成规则采用 **POE1 3.28.0 高保真完整镜像口径**。
这意味着装备生成器需要能够表达并遵循 POE 3.28.0 对应的：
- Item Level gating
- Affix Tier
- Spawn Weight / Weighting
- 相关生成约束

但“是否因此必须全量导入全部 POE Affix 内容池”仍需下一轮单独确认，避免把“规则完整镜像”和“内容全量镜像”混为一谈。

---

### Q42：Build Diversity Alpha 的最终“完成”是达到一组功能清单，还是需要形成一个可重复玩的整体验证场景？
**背景：**
目前已经会加入完整 Passive Tree、Ascendancy、10 装备槽、≥40 Affix、4 Aura、4 Reference Builds。如果只按功能逐项打勾，最终可能出现“所有系统都存在，但组合起来不好用”。

**选项：**
- **A：只要所有系统和自动测试完成即可，最终 Director 自由试玩。**
- **B：必须形成一个固定的 Build Diversity Validation Scenario：玩家可以在同一入口完成装备配置、50 Passive + Ascendancy 分配、Aura 配置、技能/Support 调整，并快速切换/重建 4 个 Reference Builds，然后进入同一个标准战斗场景体验差异。该场景是最终 Director Gate 的唯一标准入口。**
- **C：做 4 个独立角色存档，每个 Build 一个，不需要统一场景。**
- **D：做完整 Campaign/升级流程后再验收。**

**推荐：** B

**推荐理由：**
这样最终验收不是“到处点系统”，而是一个真正可重复的 Build Laboratory。它也特别适合未来规划 AI 在每轮改动后做 smoke/regression。

**Director 回答：** B

**已确认结论：**
建立统一的 **Build Diversity Validation Scenario**，作为本轮唯一最终 Director Gate 的标准入口。

该场景必须能在同一入口完成：
- 装备配置
- 50 Passive Points 分配/重置
- 8 Ascendancy Points 分配/重置
- Aura / Reservation 配置
- Active / Support 配置
- 快速重建 4 个 Reference Builds
- 进入统一标准战斗场景体验差异

该场景也应成为后续 smoke/regression 的稳定入口。


---

### Q43：Q41 的“完整 Affix 生成规则镜像”是否同时意味着全量 Affix 内容池？
**背景：**
此前已经确认 Affix 至少 40 条、按 Build Coverage Matrix 驱动；Q41 又选择完整镜像 POE 3.28.0 的 Item Level / Tier / Weight 生成规则。需要明确“规则完整”与“内容完整”的关系。

**选项：**
- **A：规则全量镜像，但内容仍只导入本轮需要的代表性 Affix 集合（至少 40 条）；这些 Affix 的 ilvl/tier/weight 必须严格遵守 POE 3.28.0。**
- **B：规则和 Affix 内容池都全量镜像 POE 3.28.0。**
- **C：先全量导入所有 Affix 数据，但 Runtime 只启用本轮需要的至少 40 条，其余 dormant。**
- **D：由规划 AI 根据实现成本自行决定。**

**推荐：** C

**推荐理由：**
数据层一次拿到完整权威快照，便于未来扩张和 diff；Gameplay 层仍只启用当前 Build Diversity 所需内容，避免把平衡与测试范围一次拉满。

**Director 回答：** A

**已确认结论：**
POE1 3.28.0 的 Affix **生成规则**采用高保真完整镜像，但本轮 Affix **内容池不全量导入**。

本轮只导入并启用满足以下条件的代表性 Affix 集合：
- 总量至少 40 条；
- 满足 Build Coverage Matrix；
- 覆盖 10 个核心装备槽位；
- 能服务 4 个 Reference Builds；
- 每条 Affix 的 Item Level / Tier / Weight / Mod Group 等规则严格遵守 POE1 3.28.0 对应基准。

其余 POE Affix 内容不因“规则完整镜像”而自动进入本轮。

---

### Q44：Ascendant 的具体效果是否也采用“完整结构 + 渐进实装”策略？
**背景：**
Q39 已确认全部 Ascendancy 数据镜像，但当前只开放 Ascendant。Ascendant 本身会引用其他 Ascendancy/职业方向，部分效果可能依赖当前未实装系统。

**选项：**
- **A：Ascendant 树全部可见可分配；当前 Runtime 支持的效果正常生效，未支持效果明确标“未实装/无效果”，但仍可作为路径/选择存在。**
- **B：只显示并允许选择当前已完全支持的 Ascendant 节点。**
- **C：为了 Ascendant 全部有效，同步实现它依赖的所有机制。**
- **D：本轮 Ascendant 只做静态展示，不参与 Reference Build。**

**推荐：** A

**推荐理由：**
与主 Passive Tree / Mastery / Jewel Socket 的处理原则一致，避免为追求 100% 节点有效而无限扩系统。

**Director 回答：** A

**已确认结论：**
Ascendant 树全部可见、可分配。
- 当前 Runtime 已支持的效果正常生效并测试；
- 未支持效果明确标记“未实装/无效果”；
- 未支持节点仍可作为路径/选择存在；
- 禁止为了让 Ascendant 100% 有效而自动提前实现与本轮无关的全部依赖系统。

该策略与主 Passive Tree、Mastery、Jewel Socket 的渐进实现原则保持一致。

---

### Q45：4 个 Reference Builds 是否必须体现不同 Ascendant 选择？
**背景：**
Ascendancy 已正式进入本轮。如果 4 个 Reference Builds 全部用同一组 Ascendant 节点，Ascendancy 可能“实现了但没真正参与 Build Diversity”。

**选项：**
- **A：4 个 Reference Builds 必须使用 4 套不同 Ascendant 分配。**
- **B：至少 2 套不同 Ascendant 分配必须被 4 个 Reference Builds 实际使用；不要求 4 套全部唯一。**
- **C：Ascendant 只需技术可用，不要求进入 Reference Builds。**
- **D：规划 AI 自由决定，不设覆盖要求。**

**推荐：** B

**推荐理由：**
能证明 Ascendancy 确实参与构筑差异，又不会为了“凑四套升华”强迫规划 AI 制造不自然 Build。

**Director 回答：** B

**已确认结论：**
4 个 Reference Builds 中，至少必须实际使用 **2 套不同的 Ascendant 分配方案**。

不要求 4 个 Build 各自拥有完全唯一的 Ascendant 路线，但 Ascendancy 必须真正参与 Build Diversity，而不能只是“技术上存在但无人使用”。

---

### Q46：Build Diversity Validation Scenario 中，装备获取采用什么方式？
**背景：**
Q30 已确认本轮不深化 Craft，Reference Build 可以通过确定性装备/开发工具生成。最终 Director Gate 的入口需要统一，避免试玩时还要手工刷装备或依赖随机掉落。

**选项：**
- **A：Validation Scenario 提供 Build Loadout 面板，可一键生成/装备 4 个 Reference Builds 的确定性装备；同时允许手动调整 Affix、Passive、Aura 等。随机 Loot/Craft 不作为进入验证的前置条件。**
- **B：必须通过正常 Loot + Craft 获取验证装备，尽量模拟真实游戏。**
- **C：只提供控制台命令生成装备，不做可视化入口。**
- **D：每个 Reference Build 使用独立预制存档，不允许现场修改。**

**推荐：** A

**推荐理由：**
最终验收目标是比较 Build 差异，而不是测试经济获取过程。A 最适合快速重复试玩，也最适合作为自动回归工具入口。

**Director 回答：** A

**已确认结论：**
Build Diversity Validation Scenario 提供可视化 **Build Loadout 面板**：
- 可一键生成并装备 4 个 Reference Builds 的确定性装备；
- 可快速恢复对应 Passive / Ascendancy / Aura / Skill / Support 配置；
- 允许现场继续手动调整 Affix、Passive、Aura 等构筑维度；
- 随机 Loot 与正式 Craft 不作为进入最终验证的前置条件。

该入口同时服务最终 Director Gate 与自动/半自动回归验证。


---

### Q47：Support Skill 本轮如何扩张？
**背景：**
当前项目已有 7 个 Support，短期目标又明确“优先构筑深度而非 Active Skill 数量”。Support 是最直接的技能行为改造维度之一，如果完全冻结，4 个 Reference Builds 可能主要靠 Passive/Aura/装备形成差异；如果大量扩张，又会进入内容生产。

**选项：**
- **A：冻结当前 Support 数量，本轮不新增。**
- **B：默认冻结；规划 AI 只有在 Reference Build Coverage Gap 表明现有 Support 无法形成足够技能行为差异时，才允许新增 Support；本轮最多新增 4 个，并必须至少服务 2 个 Reference Builds 或解决一个关键机制缺口。**
- **C：主动扩到至少 12～16 个 Support。**
- **D：直接开始高保真导入 POE 3.28.0 Support Gem。**

**推荐：** B

**推荐理由：**
和 Active Skill 的“需求驱动扩张”保持一致，但给 Support 更高一点弹性，因为它本身就是 Build Diversity 的高杠杆系统。

**Director 回答：** D

**已确认结论：**
本轮不再采用“按 Coverage Gap 少量新增 Support”的保守策略，而是启动 **POE1 3.28.0 Support Gem 高保真导入**。

但 Q47 只确认了方向，尚未自动等价为：
- 所有 Support 数据必须本轮全部启用；
- 所有 Support 依赖机制必须立即实装；
- Skill Gem 等级/品质/获取系统必须同步完整实现。

这些边界由后续 Q51～Q54 继续确认。

---

### Q48：规划 AI 遇到“旧文档与当前 Runtime/测试冲突”时，以谁为准？
**背景：**
仓库已经出现 STATUS / ROADMAP 历史信息混杂的问题。新版规划会成为新的执行真相，如果规划 AI没有固定优先级，后续可能因为读到旧段落而重新打开已经关闭的任务。

**选项：**
- **A：文档永远优先于代码和测试。**
- **B：采用 Source-of-Truth 优先级：Director 已确认决策 > 新短期规划 > DECISIONS > 当前可运行 Runtime + 自动测试证据 > STATUS/ROADMAP 当前摘要 > 历史阶段文档 > 旧简化版/长期规划中的旧执行细节。发现冲突时必须记录 Drift，并修正文档，但不得静默改产品决策。**
- **C：Runtime 永远优先，文档只作参考。**
- **D：遇到任何冲突都停止并询问 Director。**

**推荐：** B

**推荐理由：**
这能让 Repository-as-Memory 真正稳定，又不会让陈旧 Markdown 覆盖已经验证过的 Runtime 事实。

**Director 回答：** B

**已确认结论：**
仓库采用固定 Source-of-Truth 优先级：

1. Director 已确认决策
2. 新短期规划
3. DECISIONS
4. 当前可运行 Runtime + 自动测试证据
5. STATUS / ROADMAP 当前摘要
6. 历史阶段文档
7. 旧简化版 / 长期规划中的旧执行细节

发现冲突时必须：
- 记录 Drift；
- 修正文档；
- 不得静默改变 Director 产品决策；
- 不得因为陈旧 Markdown 自动重开已关闭工作。

---

### Q49：Non-Blocking Tasks 是否允许做“未来系统”预研？
**背景：**
Q13 已要求每个工作包至少 2 个 Non-Blocking Tasks。若定义太宽，主任务一阻塞，工作 AI 可能趁机开始做 Curse、Flask、Jewel、Atlas 等未来系统，导致范围失控。

**选项：**
- **A：允许，只要属于长期规划，阻塞时都可以提前做。**
- **B：Non-Blocking Tasks 只能来自当前短期规划已批准范围、测试/工具/文档/数据清理/技术债，或明确列出的未来“预研但不可落 Runtime”项；禁止未经批准实现未来核心系统。**
- **C：Non-Blocking 只能写测试和文档，不能写 Gameplay。**
- **D：由工作 AI 自由判断。**

**推荐：** B

**推荐理由：**
既保证阻塞时有足够事情可做，又防止“非阻塞”变成偷偷扩 Scope 的通道。

**Director 回答：** B

**已确认结论：**
Non-Blocking Tasks 只能来自：
- 当前新短期规划已批准范围；
- 测试 / 工具 / 文档；
- 数据清理；
- 技术债；
- 明确列出的“仅预研、不落 Runtime”未来事项。

禁止工作 AI 借 Non-Blocking 名义擅自启动 Curse、Flask、Jewel、Atlas 等未批准未来核心系统。

---

### Q50：规划 AI 什么时候可以重排阶段和 Reference Build？
**背景：**
本轮很多内容由规划 AI在审计后自行决定，例如 4 个 Reference Builds、未支持机制优先级。如果它可以随时改目标，工作 AI 可能一直追逐移动靶；如果完全不允许调整，又会在新证据出现后僵化。

**选项：**
- **A：规划 AI 可以随时重排和替换 Reference Builds，只需记录。**
- **B：采用“阶段内稳定、Gate 间可重排”：一个工作包启动后不得无证据改目标；只有在硬失败、依赖事实变化、Coverage Gap 证明原方案不可行或阶段边界切换时，规划 AI 才可重排。替换 Reference Build 必须记录原因、影响和迁移结果。**
- **C：一旦规划写出，直到最终 Director Gate 前都不得调整。**
- **D：任何重排都必须找 Director。**

**推荐：** B

**推荐理由：**
能兼顾自主推进和目标稳定，避免无人监管开发陷入“规划 AI 每轮都重新发明路线图”。

**Director 回答：** B

**已确认结论：**
采用“**阶段内稳定、Gate 间可重排**”原则。

工作包启动后，不得无证据随意改变目标。
只有出现以下情况时规划 AI 才可重排：
- 硬失败；
- 依赖事实变化；
- Coverage Gap 证明原方案不可行；
- 阶段边界切换。

替换 Reference Build 或重大重排时必须记录：
- 原因；
- 证据；
- 影响范围；
- 数据/测试迁移；
- 对后续工作包的影响。


---

### Q51：POE 3.28.0 Support Gem 的“高保真导入”采用哪种激活策略？
**背景：**
Q47 已选择直接启动 POE 3.28.0 Support Gem 高保真导入。现在必须明确数据全量与 Runtime 全量是否绑定，否则本轮可能因大量未存在机制而无限扩张。

**选项：**
- **A：数据全量镜像 + 渐进 Runtime。** 全量导入 3.28.0 Support Gem 定义、标签、需求、数值、描述与兼容规则；Runtime 只实现当前系统可承载及 Reference Builds 优先需要的效果，其余 Support 显示“未实装/部分实装”。
- **B：数据和 Runtime 都要求本轮全量实装，所有 3.28.0 Support Gem 必须实际可用。**
- **C：只导入当前 3～5 个 Active Skill 能直接使用的 Support。**
- **D：只导入数据，不允许任何新 Support Runtime 机制进入本轮。**

**推荐：** A

**推荐理由：**
和 Passive Tree / Ascendancy 的策略一致：外部基准数据完整存在，但 Runtime 机制按当前阶段价值逐步解锁。这样既满足“高保真导入”，又不会被 Minion、Totem、Trap、Mine、Cold、Lightning 等尚未存在系统拖住。

**Director 回答：** A

**已确认结论：**
POE1 3.28.0 Support Gem 采用 **数据全量镜像 + Runtime 渐进实装**：
- 全量导入 Support Gem 定义；
- 全量导入标签、需求、数值、描述与兼容规则；
- Runtime 只实现当前系统可承载及 Reference Builds 优先需要的效果；
- 未支持或部分支持的 Support 必须明确显示 Unsupported / Partial；
- 不得因为数据已存在就自动启动所有依赖系统。

---

### Q52：Support Gem 的等级与品质（Level / Quality）本轮是否纳入？
**背景：**
POE Support Gem 的数值会随等级变化，Quality 也会改变效果。如果只固定一个数值快照，机制能用，但并非完整 Gem 语义；如果连经验成长与品质获取一起做，会把本轮拉入 Skill Gem Progression。

**选项：**
- **A：完整导入 Level 1～20 与 Quality 0～20 的数值表和规则，但 Build Diversity Alpha 中允许直接设置等级/品质，不实现经验升级与品质获取流程。**
- **B：只使用一个固定测试等级（例如 Level 20 / 0 Quality），成长系统全部后置。**
- **C：同步实现 Support Gem 经验、升级、品质获取与完整成长流程。**
- **D：只导入 Level 1 数据。**

**推荐：** A

**推荐理由：**
保留高保真数据与未来成长基础，同时不把“怎么练 Gem”变成本轮阻塞项。Validation Scenario 可以直接切换等级/品质用于构筑测试。

**Director 回答：** A

**已确认结论：**
完整导入 Support Gem：
- Level 1～20 数值表与规则；
- Quality 0～20 数值表与规则。

Build Diversity Alpha 中允许直接设置 Gem Level / Quality，用于快速测试。
本轮不因此实现：
- Gem 经验成长；
- 自动升级流程；
- Quality 获取流程；
- 完整 Skill Gem Progression。

---

### Q53：为了高保真 Support 兼容性，Active Skill 的 Tag / Requirement 系统做到什么程度？
**背景：**
大量 Support 是否能连接某个技能，取决于 Attack、Spell、Projectile、AoE、Melee、Fire、Duration 等标签和额外限制。Q10 仍限制 Active Skill 内容扩张，但 Support 全量数据导入会要求稳定的兼容判定层。

**选项：**
- **A：建立完整、数据驱动的 Skill Tag / Support Compatibility 规则模型，并把 POE 3.28.0 所需标签全集导入；当前 Active Skill 只标记其真实拥有的标签。未实现机制标签可存在但不要求产生 Runtime 效果。**
- **B：只为当前 Active Skill 手工写 Support 白名单。**
- **C：Support 默认都能连所有 Active Skill，遇到明显错误再修。**
- **D：为了完整兼容性，同步全量导入 POE Active Skill。**

**推荐：** A

**推荐理由：**
Support 高保真最依赖的不是数量，而是兼容规则。如果用白名单，后面每增加一个技能都会产生维护债；A 能形成长期可扩张骨架，同时不要求 Active Skill 内容全量搬运。

**Director 回答：** A

**已确认结论：**
建立完整、数据驱动的 **Skill Tag / Support Compatibility** 规则模型，并导入 POE1 3.28.0 所需标签全集。

当前 Active Skill：
- 只赋予其真实拥有的 Tag；
- Compatibility 由数据规则计算；
- 未实现机制对应 Tag 可以存在；
- Tag 的存在不自动要求对应 Runtime 机制实现。

禁止使用长期不可维护的手工 Support 白名单作为正式架构。

---

### Q54：Support 依赖当前未实装核心系统时，规划 AI 是否允许自动启动那个系统？
**背景：**
全量 Support 数据中必然出现 Minion、Totem、Trap、Mine、Charge、Cold、Lightning、Chaos、DoT、Trigger 等当前未完整支持方向。需要防止“为了让一个 Support 变绿”不断扩大 Scope。

**选项：**
- **A：允许。只要某 Support 数据已导入，规划 AI 可以自动启动它依赖的任何系统。**
- **B：不允许自动扩 Scope。未支持 Support 进入 Support Mechanic Matrix；只有当该机制属于当前短期规划已批准范围，或能显著填补 4 个 Reference Builds 的 Coverage Gap 且不违反已锁定系统边界时才实现。否则保持未实装。**
- **C：所有 Support 依赖机制本轮一律禁止实现。**
- **D：每遇到一种新机制都必须询问 Director。**

**推荐：** B

**推荐理由：**
这是让“全量 Support 数据导入”与“短期计划仍然有限”能够同时成立的关键规则，也和 Q34、Q49 的 Coverage/Non-Blocking 原则一致。

**Director 回答：** B

**已确认结论：**
Support 依赖当前未实装核心系统时，规划 AI **不得自动扩大 Scope**。

只有满足以下之一才允许推进依赖机制：
1. 该机制已经属于当前新短期规划批准范围；
2. 该机制能显著填补 4 个 Reference Builds 的 Coverage Gap，且不违反其他已锁定系统边界。

否则：
- Support 数据继续存在；
- 标记 Unsupported / Partial；
- 写入 Support Mechanic Matrix / Backlog；
- 不阻塞 Build Diversity Alpha。


---

### Q55：Support Gem 在玩家侧是否正式接入 POE 风格 Socket / Link 体系？
**背景：**
长期目标已经包含 Active + Support、Socket / Link。现在 Support 数据开始全量镜像，如果玩家仍通过简单“Support 列表”直接绑定技能，会与长期装备构筑方向脱节；但一次性全量复刻 POE 的装备孔色、链接与重铸经济又会明显扩大范围。

**选项：**
- **A：本轮正式建立 Socket / Link 基础体系：Active 与 Support 必须通过同一 Linked Group 才发生支持关系；装备可以承载 Socket Group。先实现足够支撑当前 4 个 Reference Builds 的槽位/链接结构，不要求完整 Socket Craft Economy。**
- **B：继续保持抽象 Skill + Support 配置，本轮不引入 Socket / Link。**
- **C：完整镜像 POE1 3.28.0 的 Socket Color / Link / 装备孔位 / 重铸相关规则。**
- **D：只在 Validation Scenario 中模拟 Linked Group，正式装备系统暂不接入。**

**推荐：** A

**推荐理由：**
能让 Support 真正进入装备—技能联动，而不会把 Chromatic / Jeweller / Fusing 等获取与 Craft 经济一起拉进本轮。

**Director 回答：** A

**已确认结论：**
本轮正式建立基础 **Socket / Link** 体系：
- Active 与 Support 必须位于同一 Linked Group 才发生支持关系；
- 装备可以承载 Socket Group；
- Socket / Link 成为装备—技能—Support 联调的一部分；
- 本轮不因此进入完整 Socket Craft Economy。

---

### Q56：Gem 的装配与切换入口，本轮采用哪种玩家流程？
**背景：**
Build Diversity Validation Scenario 需要快速切换 Active / Support，Support 又有 Level / Quality。若全部依赖掉落和背包获取，会让最终验收被内容获取流程干扰。

**选项：**
- **A：Validation Scenario 提供 Gem Library / Loadout 面板，可直接选择已导入 Active/Support、设置 Level/Quality、装入 Socket/Link；正式 Loot 获取流程不作为本轮前置。**
- **B：所有 Gem 必须先通过正常 Loot 获取后才能装配。**
- **C：只通过开发者控制台生成 Gem。**
- **D：每个 Reference Build 固定 Gem 配置，不允许现场替换。**

**推荐：** A

**推荐理由：**
和确定性装备 Loadout 的原则一致：这轮验证组合深度，不验证 Gem 获取经济。

**Director 回答：** A

**已确认结论：**
Build Diversity Validation Scenario 提供 **Gem Library / Loadout**：
- 可直接选择当前可用 Active Skill；
- 可浏览已导入 Support Gem；
- 可设置 Support Gem Level / Quality；
- 可装入 Socket / Link；
- 不要求先通过 Loot 获得 Gem；
- 不把 Gem 获取经济作为本轮前置条件。

---

### Q57：是否建立 Support Mechanic Support Matrix？
**背景：**
Passive 已确认使用 Passive Mechanic Support Matrix。Support 全量导入后同样会出现大量 Supported / Partial / Unsupported 机制。如果不统一追踪，规划 AI 很容易重复判断或误把“数据已导入”当“效果已实现”。

**选项：**
- **A：建立 Support Mechanic Support Matrix，按机制族记录 Supported / Partial / Unsupported / Blocked，并关联受影响 Support、依赖系统、测试、Reference Build Coverage 与恢复条件。**
- **B：只在每个 Support Gem UI 上显示状态，不额外维护矩阵。**
- **C：每个 Support Gem 独立建任务。**
- **D：与 Passive Matrix 合并成一个巨型通用表，不区分系统。**

**推荐：** A

**推荐理由：**
这会成为规划 AI 判断“下一批 Support 机制实现价值”的核心依据，也和 Passive 侧形成一致工作法。

**Director 回答：** A

**已确认结论：**
建立独立 **Support Mechanic Support Matrix**，按机制族追踪：
- Supported
- Partial
- Unsupported
- Blocked
- 受影响 Support
- 依赖系统
- 自动测试
- Reference Build Coverage
- 阻塞原因
- Resume Trigger

规划 AI 必须利用该矩阵判断下一批 Support 机制的实现价值。

---

### Q58：Support Compatibility 的自动回归测试做到什么程度？
**背景：**
全量 Support 数据 + Tag Compatibility 很容易出现“理论规则正确，但某个 Support 错误支持/拒绝某技能”的回归。当前 Active Skill 数量仍很少，但未来会增长。

**选项：**
- **A：建立 Compatibility Matrix 自动测试：对所有当前可用 Active × 已导入 Support 计算兼容状态，并与 Canonical Rule/Expected Snapshot 比对；新增/修改 Skill Tag 或 Support Rule 时必须重跑。**
- **B：只为 4 个 Reference Builds 用到的 Skill/Support 组合写测试。**
- **C：只做人工 UI 检查。**
- **D：等 Active Skill 数量扩大后再建设兼容性自动测试。**

**推荐：** A

**推荐理由：**
当前技能少时建立矩阵成本最低，未来新增技能时会直接受益，也能防止工作 AI 用手工例外慢慢污染兼容规则。

**Director 回答：** A

**已确认结论：**
建立自动 **Active × Support Compatibility Matrix** 回归测试。
所有当前可用 Active Skill 与已导入 Support Gem 都必须计算兼容状态，并与 Canonical Expected Snapshot / Rule 结果比对。

以下改动必须触发矩阵重跑：
- Active Skill Tag 变化
- Support Tag / Compatibility Rule 变化
- Socket / Link 规则变化
- 新增 Active Skill
- Support Runtime 状态变化

禁止长期依赖人工白名单与人工 UI 检查作为兼容性真相。


---

### Q59：Socket Color 是否纳入本轮正式规则？
**背景：**
POE Support/Active Gem 不只有 Link 关系，还受 Red / Green / Blue 等 Socket Color 限制。现在已经决定高保真导入 Support Gem，如果忽略颜色，很多装备—Gem 取舍会与 POE 原规则明显不同；但若把改色货币和随机重铸一起做，又会提前进入 Craft Economy。

**选项：**
- **A：正式实现 Red / Green / Blue / White Socket Color 与 Gem Color Requirement；Validation Scenario 可直接编辑 Socket Color，不实现 Chromatic 等改色经济。**
- **B：本轮所有 Socket 都视为通用色，只验证 Link。**
- **C：完整实现 POE Socket Color + 装备属性偏置 + Chromatic/Jeweller/Fusing 等相关改造规则。**
- **D：只有 Support Gem 检查颜色，Active Gem 不检查。**

**推荐：** A

**推荐理由：**
能保留装备与 Gem 的真实兼容约束，同时把“如何随机洗出颜色”留到后续 Craft 阶段，不阻塞当前 Build Diversity。

**Director 回答：** B

**已确认结论：**
本轮 **不启用 Socket Color 作为装备/宝石兼容约束**。
所有 Socket 在 Gameplay 规则上视为通用，不因为 Red / Green / Blue / White 阻止 Active / Support 装配。

这意味着：
- Socket / Link 仍正式存在；
- Gem Compatibility 仍由 Skill Tag / Support Rule 决定；
- Socket Color 不作为 Build Diversity Alpha 的构筑门槛；
- 不启动 Chromatic 或改色经济。

---

### Q60：装备上的 Socket 数量与最大 Link 是否采用 POE 风格容量规则？
**背景：**
Q55 已确认装备承载 Socket Group。如果所有装备都能任意 6-Link，会严重破坏装备槽位价值；如果全量复刻所有历史 Socket 生成细节，又会扩大内容范围。

**选项：**
- **A：采用稳定的 POE 风格容量模型：不同 Base Item / 装备类型声明 Max Sockets / Max Links；当前代表性 Base Items 按 POE1 3.28.0 对应规则配置。Validation Scenario 可直接生成合法 Socket/Link，不做随机打孔/连线经济。**
- **B：所有装备统一最多 4 Socket / 4 Link。**
- **C：所有装备都允许最多 6 Link，方便测试。**
- **D：完整实现 POE 的 Socket 数量生成、属性门槛、货币修改概率与全部边界规则。**

**推荐：** A

**推荐理由：**
保留“装备类型影响技能链接上限”的核心结构，又不会把随机打孔与连线货币系统拖进来。

**Director 回答：** A

**已确认结论：**
采用稳定的 POE 风格 Socket 容量模型：
- 不同 Base Item / 装备类型声明 Max Sockets；
- 不同 Base Item / 装备类型声明 Max Links；
- 当前代表性 Base Items 按 POE1 3.28.0 对应规则配置；
- Validation Scenario 可以直接生成合法 Socket / Link；
- 本轮不实现随机打孔、随机连线与相关货币经济。

---

### Q61：规划 AI 是否要维护一个“阶段能力清单（Capability Ledger）”？
**背景：**
当前会同时存在 Passive / Ascendancy / Support 的 Supported / Partial / Unsupported 状态，还有装备、Aura、Socket/Link 等系统。如果规划 AI 只靠多个分散 Matrix，很容易不知道“当前项目究竟已经能表达哪些构筑能力”。

**选项：**
- **A：建立统一 Capability Ledger，只记录高层能力状态，例如 Fire Conversion、Crit、Armour、Evasion、Aura Reservation、Projectile Modification、Socket Colors 等；各系统 Matrix 仍保留细节。规划 AI 每次阶段切换前必须刷新 Ledger。**
- **B：不建立统一 Ledger，只依赖各自 Matrix。**
- **C：把所有细节都塞进一个超级表，取代各系统 Matrix。**
- **D：只在最终报告里人工汇总一次。**

**推荐：** A

**推荐理由：**
这能给规划 AI 一个“当前游戏能力地图”，非常适合用来判断 Reference Build Coverage Gap，同时又不破坏各系统矩阵的细粒度追踪。

**Director 回答：** A

**已确认结论：**
建立统一 **Capability Ledger**，用于给规划 AI 提供当前项目的高层能力地图。

Ledger 记录例如：
- Damage / Conversion
- Crit
- Armour / Evasion
- Aura / Reservation
- Weapon / Handedness
- Socket / Link
- Support Compatibility
- Passive / Ascendancy
- 其他关键构筑能力

各系统自己的 Mechanic Support Matrix 继续保留细节。
规划 AI 在阶段切换前必须刷新 Capability Ledger，并用它评估 Reference Build Coverage Gap。

---

### Q62：每个阶段/工作包完成后，规划 AI 是否必须执行“仓库事实收口”再发下一单？
**背景：**
你希望规划 AI 能长期、无人监管地连续安排工作 AI。最大的风险之一是代码已变、测试已变，但 STATUS / DECISIONS / Matrix / Roadmap 没同步，导致下一轮基于旧事实规划。

**选项：**
- **A：是。每个正式工作包完成后，规划 AI 必须先完成 Repository Reconciliation：确认 Runtime、测试、Matrix、DECISIONS/STATUS/ROADMAP、阻塞项和 Non-Blocking 状态一致，再生成下一工作令。**
- **B：只有阶段结束时做一次文档收口。**
- **C：完全依赖工作 AI 自己更新文档，规划 AI 不复核。**
- **D：只在发现冲突时再收口。**

**推荐：** A

**推荐理由：**
这是让 Repository-as-Memory 在长期 AI 开发里不漂移的核心保障；虽然增加少量管理成本，但能显著降低后续规划错误。

**Director 回答：** B

**已确认结论：**
Repository Reconciliation 改为 **阶段结束时统一强制执行**，而不是每个工作包结束都必须完整收口。

工作包级别允许只更新其直接相关的测试/状态记录，不要求每次都全面核对所有文档。
每个阶段结束前必须统一核对并修正：
- Runtime
- 自动测试
- Capability Ledger
- Passive / Support 等 Mechanic Matrix
- DECISIONS
- STATUS / ROADMAP
- Blocked / Non-Blocking 状态
- 当前阶段完成证据

未完成阶段级 Reconciliation 时，不得进入下一阶段。


---

### Q63：虽然本轮 Socket 不限制颜色，Gem Color 数据是否仍保留？
**背景：**
Q59 选择所有 Socket Gameplay 上通用，但 POE 3.28.0 Active/Support 数据本身仍包含颜色/属性倾向信息。如果直接丢弃，以后恢复 Socket Color 会需要重新迁移数据。

**选项：**
- **A：Gem Color / Attribute Color 数据继续完整导入并保留为 dormant metadata；本轮 UI 可显示，但 Compatibility 不检查颜色。**
- **B：本轮完全删除/忽略 Gem Color 数据，以后需要时再重新导入。**
- **C：颜色只用于 UI 分类和筛选，不保存原始 POE 字段。**
- **D：规划 AI 自行决定是否保留。**

**推荐：** A

**推荐理由：**
几乎不增加 Runtime 复杂度，又保留未来恢复 POE 风格 Socket Color 的迁移路径。

**Director 回答：** A

**已确认结论：**
Gem Color / Attribute Color 数据继续完整导入并保留为 **dormant metadata**。
本轮：
- UI 可以显示颜色信息；
- Compatibility 不检查颜色；
- Socket Gameplay 视为通用色；
- 未来若恢复 POE 风格 Socket Color，可直接复用现有 Canonical Data，不需重新迁移。

---

### Q64：新版短期规划采用“明确阶段序列”还是“动态目标池”？
**背景：**
现在系统范围已经很清楚，但规划 AI 仍需要知道总体推进次序。若只给一个大 Backlog，可能出现先做 Aura、再回头补装备基础结构的依赖倒置。

**选项：**
- **A：采用明确阶段序列，每阶段有 Entry / Scope / Hard Gate / Non-Blocking / Exit / Reconciliation；规划 AI 只能在阶段内自主排序，阶段切换按 Gate 进行。**
- **B：只给一个按优先级排序的动态目标池，规划 AI 自由穿插所有系统。**
- **C：只规定最终目标，不规定阶段或优先级。**
- **D：由工作 AI 自己决定阶段顺序。**

**推荐：** A

**推荐理由：**
这最适合“规划 AI 管工作 AI”的模式：既有长期秩序，又保留阶段内自主调度和非阻塞切换。

**Director 回答：** A

**已确认结论：**
新版短期规划采用 **明确阶段序列**。
每个阶段必须定义：
- Entry
- Scope
- Hard Gate
- Non-Blocking Tasks
- Exit
- Repository Reconciliation

规划 AI 可以在阶段内部自主调整工作包顺序，但不得跳过未通过的阶段 Gate，也不得把所有系统混成一个无边界动态 Backlog。

---

### Q65：规划 AI 是否可以在已批准系统范围内自动新增测试、验证工具和数据审计工具？
**背景：**
随着 Passive、Support、Affix、Socket、Ascendancy 数据规模增大，实际工作中经常会发现原计划没写到的 Validator / Import Audit / Snapshot Diff / Smoke Test 等工具需求。

**选项：**
- **A：可以。只要工具直接服务当前批准范围、不会改变 Gameplay 设计语义，规划 AI 可自动安排，不需要 Director 逐项批准。**
- **B：只有自动测试可以新增，编辑器工具/导入工具必须询问 Director。**
- **C：所有新工具都必须先询问 Director。**
- **D：禁止本轮新增工具，尽量复用现有。**

**推荐：** A

**推荐理由：**
这些工具属于实现保障而不是产品设计；如果每个 Validator 都等人工批准，会破坏无人监管推进。

**Director 回答：** A

**已确认结论：**
规划 AI 可以在当前已批准范围内自动新增：
- 自动测试
- Validator
- Import Audit
- Snapshot Diff
- 数据审计工具
- Smoke / Regression 工具
- 编辑器辅助工具
- 其他不改变 Gameplay 产品语义的实现保障工具

无需逐项请求 Director 批准。

---

### Q66：规划 AI 在什么情况下才允许中途打断并请求 Director 决策？
**背景：**
Q14 已确认只有最终一次正式体验 Gate，Q13 又要求阻塞时自动切换 Non-Blocking Tasks。但仍需要定义少数真正必须人工介入的情况。

**选项：**
- **A：只有出现“已确认 Director 决策彼此冲突 / 新问题必然改变核心产品语义 / 所有当前批准范围内 Primary 与 Non-Blocking 工作都被同一依赖阻塞”时，才允许中途请求 Director；普通技术失败、测试失败、数据缺口、实现困难均由规划 AI 自主处理。**
- **B：任何硬 Gate 失败都立即请求 Director。**
- **C：任何需求解释存在歧义都请求 Director。**
- **D：永不中途请求 Director，全部留到最终验收。**

**推荐：** A

**推荐理由：**
能最大程度维持无人监管推进，同时保留真正涉及产品方向或全局死锁时的人类决策权。

**Director 回答：** A

**已确认结论：**
规划 AI 只有在以下情况下才允许中途请求 Director 决策：
1. 已确认的 Director 决策彼此产生无法自行消解的冲突；
2. 新问题必然改变核心产品语义或已锁定设计方向；
3. 当前 Primary Path 与所有合法 Non-Blocking Tasks 都被同一依赖彻底阻塞，形成全局死锁。

普通技术失败、测试失败、数据缺口、实现困难、局部设计细节与可恢复阻塞均由规划 AI 自主处理。


---

### Q67：Active Skill 数据是否也采用“全量镜像数据 + 渐进 Runtime”？
**背景：**
Support Gem 已经确定全量导入 POE1 3.28.0 数据，但 Active Skill 目前仍只有现有技能为主、必要时最多新增 2 个 Runtime 技能。如果 Support Compatibility 只面对少数当前 Active，数据层仍是不完整的。

**选项：**
- **A：全量导入 POE1 3.28.0 Active Skill Gem 数据、标签、需求、等级/品质表与描述，作为 dormant canonical data；Runtime 仍只启用现有技能及最多新增 2 个，未实现 Active 标记 Unsupported。**
- **B：只保留当前已实现 Active Skill 数据，不导入其余 POE Active。**
- **C：数据和 Runtime 都全量镜像 POE Active Skill。**
- **D：只导入会被 4 个 Reference Builds 用到的 Active Skill。**

**推荐：** A

**推荐理由：**
这样 Support Compatibility、Tag Schema 和未来扩张的数据基线一次建立完整，但不会把本轮拖成全量 Active Skill Runtime 搬运。

**Director 回答：** A

**已确认结论：**
POE1 3.28.0 Active Skill Gem 采用 **全量 Canonical Data 导入 + Runtime 渐进启用**：
- 全量导入 Active Skill Gem 定义；
- 标签；
- 需求；
- Level / Quality 数据；
- 描述与其他必要 Canonical Metadata。

Runtime 本轮仍只启用：
- 当前已存在 Active Skill；
- 因 Reference Build Coverage Gap 确有必要时最多新增 2 个 Active Skill。

其余 Active Skill 保留为 dormant / Unsupported data，不因数据全量存在而自动进入 Runtime。

---

### Q68：最终 Build Diversity Alpha 是否要求“支持率百分比”达标？
**背景：**
完整 Passive Tree、Ascendancy、Support/Active 数据导入后，必然会存在大量 Unsupported / Partial。若用“全体支持率 ≥ X%”作为 Gate，很可能逼着项目实现大量与 4 个 Reference Builds 无关的系统；但完全不设支持度要求，又可能让树看起来完整、实际大部分没效果。

**选项：**
- **A：不设全局百分比；只要求 4 个 Reference Builds 的关键路径、关键 Passive/Mastery/Ascendant、所用 Support、Aura、装备和 Affix 全部 Supported，外围内容可以继续 Unsupported。**
- **B：要求 Passive / Support / Ascendancy 等整体至少 50% Supported。**
- **C：要求整体至少 80% Supported。**
- **D：要求所有导入内容 100% Supported 才算完成。**

**推荐：** A

**推荐理由：**
最符合“数据完整、Runtime 按 Build Coverage 渐进实现”的整体原则，也能避免用无意义的百分比把短期计划无限扩大。

**Director 回答：** A

**已确认结论：**
Build Diversity Alpha **不设置全局 Supported 百分比 Gate**。

最终必须 100% Supported 的范围仅限 4 个 Reference Builds 的关键依赖路径，包括其实际使用的：
- Passive / Mastery
- Ascendant
- Active Skill
- Support
- Aura / Reservation
- Weapon / Equipment
- Affix
- Socket / Link
- 其他关键 Build 机制

外围 Canonical Data 可以继续处于 Partial / Unsupported / dormant 状态。

---

### Q69：阶段推进顺序是否固定为“基础数据 → 装备/Socket → Passive/Ascendancy → Support/Aura → Reference Builds/Validation”？
**背景：**
Q64 已确认要有明确阶段序列，现在需要锁总体顺序，避免规划 AI 在依赖尚未建立时先做上层 Build 内容。

**选项：**
- **A：固定为：
  1. Entry / S4 吸收与仓库真相收口
  2. Canonical Data & Capability Foundation
  3. Equipment / Affix / Base Item / Socket-Link
  4. Passive Tree / Mastery / Ascendancy
  5. Support / Skill Compatibility / Aura-Reservation
  6. Reference Builds / Validation Scenario / Final Director Gate**
- **B：Passive/Ascendancy 放到装备之前。**
- **C：Support/Aura 放到 Passive 之前。**
- **D：只规定依赖关系，不固定阶段顺序，让规划 AI自由选择。**

**推荐：** A

**推荐理由：**
它从数据基础到构筑载体，再到构筑规则，最后才组合验证；依赖最清晰，也最方便阶段级 Reconciliation。

**Director 回答：** A

**已确认结论：**
新版短期规划采用固定总体阶段顺序：

1. Entry / 旧 S4 有价值工作吸收与执行真相切换
2. Canonical Data & Capability Foundation
3. Equipment / Affix / Base Item / Socket-Link
4. Passive Tree / Mastery / Ascendancy
5. Support / Skill Compatibility / Aura-Reservation
6. Reference Builds / Build Diversity Validation Scenario / Final Director Gate

阶段内部规划 AI 可自主排序工作包；阶段之间必须通过对应 Gate 与阶段级 Repository Reconciliation。

---

### Q70：最终 Director Gate 失败后，规划 AI 是否可以自动进入修正循环？
**背景：**
Q14 已确认只有最终一次正式 Director 体验 Gate。但如果你最终试玩后指出“Build 差异不明显”“某套不好玩”之类问题，需要决定是否每次修正都重新进入人工审批。

**选项：**
- **A：Director 给出问题清单后，规划 AI 自动把反馈转成新的有限修正阶段，继续安排工作 AI；修正完成后再次进入 Director Gate，循环直到通过。**
- **B：最终 Gate 一旦失败，整个规划停止，必须由 Director 重新写新规划。**
- **C：规划 AI 可以自行判断是否忽略部分 Director 反馈。**
- **D：最终 Gate 只记录意见，不要求修正。**

**推荐：** A

**推荐理由：**
这样“最终一次 Gate”实际上是一个可重复收口环，而不是失败一次就把无人监管流程打断；同时修正范围仍由你的反馈约束。

**Director 回答：** A

**已确认结论：**
最终 Director Gate 未通过时，不废止整份短期规划。

流程为：
1. Director 提供问题/体验反馈清单；
2. 规划 AI 把反馈转化为边界明确的 Correction Phase；
3. 自动拆分工作包并安排工作 AI；
4. 使用原有 Hard Gate / Non-Blocking / Reconciliation 机制执行；
5. 修正完成后再次进入 Director Gate；
6. 循环直到 Director 明确通过。

规划 AI 不得自行忽略 Director 在 Gate 中提出的正式反馈。


---

### Q71：规划 AI 给工作 AI 的标准 Work Order 必须包含哪些字段？
**背景：**
Q38 已限制每个工作令只设 1 个主要功能目标、至少 2 个 Non-Blocking Tasks，并且原则上不跨超过 2 个核心系统。为了让规划 AI 的输出稳定可审计，需要决定是否进一步锁死工作令模板。

**选项：**
- **A：采用强制 Work Order 模板，至少包含：Objective、Why Now、In Scope、Out of Scope、Inputs/Source of Truth、Expected Files/Systems、Acceptance Criteria、Tests/Gates、Primary Path、≥2 Non-Blocking Tasks、Blocked Resume Trigger、Evidence Required、Docs/Matrix Updates、Forbidden Expansion、Completion Report Format。**
- **B：只要求 Objective / Scope / Acceptance Criteria 三项，其余自由。**
- **C：规划 AI 每次根据任务类型自行决定格式。**
- **D：继续沿用旧简化版的 5 字段任务格式。**

**推荐：** A

**推荐理由：**
新版规划已经比旧简化版复杂得多。如果工作令仍过于简略，Non-Blocking、Canonical Source、禁止扩 Scope 等关键约束无法稳定传递给工作 AI。

**Director 回答：** A

**已确认结论：**
规划 AI 必须使用强制 **Work Order Template**。每个正式工作令至少包含：
- Objective
- Why Now
- In Scope
- Out of Scope
- Inputs / Source of Truth
- Expected Files / Systems
- Acceptance Criteria
- Tests / Gates
- Primary Path
- 至少 2 个 Non-Blocking Tasks
- Blocked Resume Trigger
- Evidence Required
- Docs / Matrix Updates
- Forbidden Expansion
- Completion Report Format

规划 AI 不得退回到旧简化版的极简 5 字段格式。

---

### Q72：规划 AI 同一时间应该下发多少个“正式主工作令”？
**背景：**
无人监管推进需要持续性，但如果同时下发多个 Primary Work Orders，工作 AI 可能交叉改动多个系统；如果严格一次一个，又需要依赖 Non-Blocking Tasks 解决阻塞。

**选项：**
- **A：始终只允许 1 个 Active Primary Work Order；其内部自带至少 2 个 Non-Blocking Tasks。只有完成/正式阻塞转移/取消当前主单后，规划 AI 才激活下一主单。**
- **B：允许同时激活最多 3 个 Primary Work Orders，让工作 AI 自由切换。**
- **C：一个阶段的所有 Work Orders 一次性全部下发。**
- **D：不限制，由工作 AI 自己选。**

**推荐：** A

**推荐理由：**
和“单写者、可审计、阶段内稳定”原则最一致。非阻塞机制已经解决了单主线卡住的问题，没有必要再制造多主线并发。

**Director 回答：** A

**已确认结论：**
任意时刻只允许 **1 个 Active Primary Work Order**。
当前主工作令阻塞时：
- 不启动第二个并行主工作令；
- 使用当前工作令内预先定义的 Non-Blocking Tasks；
- 只有当前主工作令完成、被正式转移、取消或进入明确阻塞状态后，规划 AI 才能激活下一主工作令。

---

### Q73：工作 AI 完成一个 Work Order 后，谁负责最终技术验收？
**背景：**
当前仓库历史上允许同一个工作 AI 轮流担任 Implementer / Reviewer。新版计划需要明确规划 AI 在接收完成报告时，是否只相信工作 AI 自报完成。

**选项：**
- **A：工作 AI 必须先执行 Self-Review / Tests / Evidence Pack；随后规划 AI 根据 Acceptance Criteria、测试证据、diff/状态文档进行独立复核。规划 AI 不写功能代码，但有权判定未完成并发回修正。**
- **B：工作 AI 自己声明完成即视为通过。**
- **C：必须启动一个独立第三 AI Reviewer 才能验收。**
- **D：只有阶段结束时才统一技术验收。**

**推荐：** A

**推荐理由：**
不需要额外增加常驻 Agent，但也不会让“实现者说完成”直接等于“事实完成”。规划 AI 真正承担协调/验收职责。

**Director 回答：** A

**已确认结论：**
Work Order 的技术完成采用两层验收：
1. 工作 AI 必须先完成 Self-Review / Tests / Evidence Pack；
2. 规划 AI 再依据 Acceptance Criteria、测试证据、Diff、Matrix/状态文档进行独立复核。

规划 AI：
- 不承担正常功能实现代码；
- 有权判定“未完成”；
- 有权发回有限修正；
- 不得仅凭工作 AI 自报完成就关闭工作令。

---

### Q74：POE Canonical Data 的外部来源出现冲突时，采用什么权威顺序？
**背景：**
Passive Tree、Active/Support Gem、Ascendancy、Affix 等数据不一定都能从同一个官方页面完整取得。未来工作 AI 可能同时看到官方 Patch Notes、游戏数据提取、Path of Building、PoEDB 等来源，并产生差异。

**选项：**
- **A：建立固定 Source Hierarchy：GGG 官方发布/客户端数据或可验证游戏数据 > 官方 Patch Notes > 已锁定的可信数据提取/社区工具源 > 其他社区资料。任何非官方来源都必须记录来源版本与哈希；发生冲突时不得静默拼接。**
- **B：PoEDB 作为唯一来源，最方便。**
- **C：Path of Building 作为唯一来源。**
- **D：由工作 AI 每次自行选择看起来最完整的来源。**

**推荐：** A

**推荐理由：**
完整镜像要求“可追溯”，不只是“有数据”。固定来源优先级能防止多个社区源之间的小差异慢慢污染 Canonical Snapshot。

**Director 回答：** B

**已确认结论：**
本轮 POE Canonical Data 的唯一外部数据来源锁定为 **PoEDB**。

规划 AI / 工作 AI：
- 不得把 GGG Patch Notes、Path of Building 或其他社区来源与 PoEDB 数据静默拼接成 Canonical Snapshot；
- 所有导入数据必须记录 PoEDB 对应版本/页面/抓取时间/内容哈希或等价可追溯信息；
- 如果 PoEDB 缺失字段、页面不可用或数据自相矛盾，必须显式记录为 Data Gap / Blocked，不得自行切换外部来源补齐；
- 如需修改唯一来源策略，必须由 Director 重新决策。


---

### Q75：PoEDB 出现缺失字段或页面不可用时，当前阶段怎么处理？
**背景：**
Q74 已锁定 PoEDB 为唯一外部数据源，因此不能再用 GGG / PoB 等来源补齐。需要决定“数据缺口”是否阻塞整个阶段。

**选项：**
- **A：任何 PoEDB 数据缺口都阻塞当前阶段，必须等数据恢复。**
- **B：按影响分级。若缺口影响当前 4 个 Reference Builds、Canonical Schema 或当前阶段 Hard Gate，则标记 Blocked；否则记录 Data Gap，相关内容保持 dormant / Unsupported，并继续其他 Primary 或 Non-Blocking 工作。**
- **C：规划 AI 可以根据常识猜测缺失数据，之后再修。**
- **D：允许临时切换到其他数据源补齐，但最终仍标 PoEDB。**

**推荐：** B

**推荐理由：**
既遵守“唯一数据源”，又不会因为一个与当前 Build 无关的缺字段让整个短期规划停摆。

**Director 回答：** B

**已确认结论：**
PoEDB 数据缺口采用 **影响分级** 处理：
- 若缺口影响当前 4 个 Reference Builds；
- 影响 Canonical Schema；
- 或影响当前阶段 Hard Gate；

则标记为 Blocked。

否则：
- 记录 Data Gap；
- 相关内容保持 dormant / Unsupported；
- 继续执行其他 Primary 或 Non-Blocking 工作；
- 不允许用其他外部来源静默补齐。

---

### Q76：每个 Work Order 是否强制要求“可回滚点”？
**背景：**
工作 AI 无人监管连续执行，如果某个工作令跨多个文件并引入错误，需要有明确恢复方式。当前又限制一个工作令原则上不跨超过两个核心系统。

**选项：**
- **A：每个 Work Order 开始前记录 Baseline Commit / Snapshot；完成时形成可识别的完成提交或等价变更边界。若验收失败且修复成本高，规划 AI 可以回退到 Baseline 后重做。**
- **B：不要求显式回滚点，只依赖 Git 历史。**
- **C：每改一个文件都建立备份副本。**
- **D：只有阶段开始时做一次大 Snapshot。**

**推荐：** A

**推荐理由：**
对无人监管 AI 开发非常重要，而且不会像逐文件备份那样制造噪音。

**Director 回答：** D

**已确认结论：**
不要求每个 Work Order 单独建立 Baseline / Rollback Point。

采用 **阶段级 Snapshot**：
- 每个阶段开始时建立一次明确 Baseline Snapshot；
- 阶段内依赖正常 Git 历史与工作令证据追踪；
- 不强制每个工作令形成独立可回滚基线；
- 若阶段内出现严重不可恢复问题，可回到阶段级 Snapshot 重新组织执行。

规划 AI 不得把“每工作令独立回滚点”重新设为硬性流程要求。

---

### Q77：Evidence Pack 是否要使用固定最低内容？
**背景：**
Q73 已确认工作 AI 必须交 Evidence Pack。如果证据格式自由，规划 AI 很难稳定判断“完成”与否。

**选项：**
- **A：固定最低 Evidence Pack：变更摘要、涉及文件/系统、自动测试结果、关键截图/运行证据（适用时）、数据/Schema Diff、Matrix/Capability 变化、已知限制、阻塞项、Non-Blocking 完成情况、回滚点/提交标识。**
- **B：只提交测试通过截图或日志。**
- **C：只写一段自然语言总结。**
- **D：由工作 AI 自由决定证据形式。**

**推荐：** A

**推荐理由：**
这会让规划 AI 的独立验收真正可重复，而不是每轮重新猜“这个工作 AI 到底交了什么”。

**Director 回答：** A

**已确认结论：**
Evidence Pack 使用固定最低格式，至少包含：
- 变更摘要
- 涉及文件 / 系统
- 自动测试结果
- 关键截图 / 运行证据（适用时）
- Data / Schema Diff
- Matrix / Capability 变化
- 已知限制
- 阻塞项
- Non-Blocking 完成情况
- 当前提交 / Git 变更标识
- 与阶段级 Snapshot 的关系（适用时）

Evidence Pack 是规划 AI 技术验收的正式输入。

---

### Q78：阶段级 Repository Reconciliation 完成后，是否生成“阶段快照文档”？
**背景：**
Q62 已确认阶段结束时统一做 Repository Reconciliation。为了避免 STATUS/ROADMAP 越来越长，最好决定是否每阶段额外生成一份稳定快照，作为未来规划 AI 快速恢复上下文的入口。

**选项：**
- **A：每个阶段结束生成一份 Phase Snapshot，固定记录：已完成能力、未支持能力、关键决策、Canonical Data 版本、测试状态、Reference Build 影响、阻塞/技术债、下一阶段 Entry 条件。STATUS 只保留当前摘要与链接。**
- **B：继续把全部历史都累加进 STATUS.md，不生成阶段快照。**
- **C：只更新 ROADMAP，不保留阶段历史。**
- **D：每个 Work Order 都生成一个独立快照文档。**

**推荐：** A

**推荐理由：**
能明显减轻你现在已经看到的“当前事实埋在历史事实里”的 Repository-as-Memory 问题。

**Director 回答：** A

**已确认结论：**
每个阶段结束后生成稳定 **Phase Snapshot**。

Phase Snapshot 至少记录：
- 已完成能力
- 未支持 / Partial / Blocked 能力
- 关键 Director / 技术决策
- Canonical Data 版本
- 测试状态
- Capability Ledger 摘要
- Passive / Support 等 Matrix 状态
- Reference Build 影响
- 阻塞 / 技术债
- 下一阶段 Entry 条件

STATUS.md 只保留“当前项目摘要 + 当前阶段 + Phase Snapshot 链接/索引”，避免继续无限累加历史正文。


---

### Q79：每个阶段开始时的 Baseline Snapshot 必须包含什么？
**背景：**
Q76 已确认只做阶段级 Snapshot，不为每个 Work Order 单独建回滚点。因此阶段开始时的 Baseline 必须足够完整，否则后续无法判断哪些问题是阶段新引入的。

**选项：**
- **A：阶段 Baseline 强制包含 Git commit/tag（或等价代码快照）、全量当前自动测试结果、Capability Ledger、各 Mechanic Matrix、Canonical Data 版本/哈希、已知失败清单、当前阻塞项与性能记录（若有）。**
- **B：只记录 Git commit/tag。**
- **C：只记录测试结果和 STATUS。**
- **D：由规划 AI 自由决定 Baseline 内容。**

**推荐：** A

**推荐理由：**
既然没有 Work Order 级回滚点，就必须让阶段基线足够强，才能区分“旧债”和“新回归”。

**Director 回答：** A

**已确认结论：**
阶段 Baseline Snapshot 强制包含：
- Git commit / tag 或等价代码基线
- 全量当前自动测试结果
- Capability Ledger
- 各 Mechanic Support Matrix
- Canonical Data 版本 / 哈希
- 已知失败清单
- 当前阻塞项
- 适用时的性能记录

该 Baseline 用于区分历史问题与当前阶段新引入回归。

---

### Q80：阶段内出现自动测试失败时，如何判断是否阻塞？
**背景：**
仓库未来可能存在已知 Unsupported/Blocked 项，也可能出现历史遗留失败。不能把所有红灯都视为“新工作导致”，也不能允许新回归混进旧失败里。

**选项：**
- **A：采用 Baseline Diff Gate：阶段 Baseline 已知失败允许继续存在并必须跟踪；任何由当前阶段新引入的测试失败、原有通过测试变红、Canonical Snapshot/Matrix 不一致都视为回归并阻塞相关 Primary Path。与该回归无关的 Non-Blocking Tasks 可继续。**
- **B：只要总测试通过率没有下降超过 5% 就可以继续。**
- **C：任何测试失败都阻塞整个阶段。**
- **D：只看工作 AI 自己新增的测试，旧测试失败忽略。**

**推荐：** A

**推荐理由：**
这是阶段级 Snapshot 模式下最可靠的回归判断方法，也与非阻塞机制兼容。

**Director 回答：** A

**已确认结论：**
阶段内采用 **Baseline Diff Gate**：
- Baseline 已知失败可继续存在，但必须持续记录；
- 任何原本通过的测试变红；
- 新增失败；
- Canonical Snapshot 与 Runtime/Matrix 不一致；
- 或阶段改动导致已支持能力回退；

均视为新回归，并阻塞相关 Primary Path。

与该回归无关的 Non-Blocking Tasks 仍可继续执行。

---

### Q81：规划 AI 是否可以批准架构重构？
**背景：**
后续会引入 Canonical Data、完整 Passive Tree、Support Compatibility、Aura、Socket/Link 等长期骨架，现有两程序集结构可能遇到压力。需要决定规划 AI 能不能以“为了实现新系统”为由大改架构。

**选项：**
- **A：允许有限重构。只要重构直接服务当前批准能力、不会引入第二套平行架构、不会无必要新增核心程序集、并且有回归证据，规划 AI 可自动批准；涉及根本 Runtime 架构方向、核心程序集边界大改或替换已锁定技术路线时必须请求 Director。**
- **B：本轮完全禁止架构重构，只能在现有结构里加功能。**
- **C：规划 AI 可以自由重构，只要最终测试通过。**
- **D：任何重构都必须请求 Director。**

**推荐：** A

**推荐理由：**
完全禁止重构会让新系统被迫塞进旧形态；完全放开又容易出现 AI 过度抽象。A 给“必要重构”留空间，但锁住根本方向。

**Director 回答：** A

**已确认结论：**
规划 AI 可以批准 **有限必要架构重构**，前提是：
- 直接服务当前批准能力；
- 不制造第二套平行架构；
- 不无必要新增核心程序集；
- 有明确回归证据；
- 不改变已锁定产品语义。

如果涉及：
- 根本 Runtime 架构方向；
- 核心程序集边界大改；
- 替换已锁定技术路线；
- 引入新的根本性平台方案；

则必须请求 Director 决策。

---

### Q82：这份新短期规划什么时候正式宣告完成？
**背景：**
Q70 已确认最终 Director Gate 可以失败并进入 Correction Phase 循环。需要明确“规划 AI 什么时候停止继续扩张”，否则最终通过后它可能自动进入 Curse、Flask、Jewel、Atlas 等下一个长期阶段。

**选项：**
- **A：只有在 6 个正式阶段全部 Exit + Reconciliation 完成、4 个 Reference Builds 关键路径全部 Supported、Validation Scenario 完成、Director 最终明确通过，并且所有 Correction Phase 关闭后，本短期规划才 Completed。Completed 后规划 AI 必须停止新增本规划之外的 Gameplay 工作，等待 Director 提供下一份目标。**
- **B：最终自动测试全绿就算完成，不需要 Director 明确通过。**
- **C：Director 通过后规划 AI 可以自行从长期规划选择下一系统继续开发。**
- **D：只要阶段 6 开始就视为基本完成。**

**推荐：** A

**推荐理由：**
这是防止“规划 AI 顺着长期规划无限自动扩张”的最终保险丝。

**Director 回答：** C

**已确认结论：**
当本轮 6 个阶段完成、最终 Director Gate 明确通过、Correction Phase 关闭后，规划 AI **不停止整个项目推进**。

规划 AI 可以继续从长期规划中选择下一项系统/阶段，并进入新的短期执行周期。

因此，本轮新短期规划不仅要定义当前 Build Diversity Alpha，还需要形成可被后续短期周期继承的滚动治理规则。

“下一系统如何自动选择、是否需要生成新的短期计划、后续计划继承哪些规则”由 Q83～Q86 继续确认。


---

### Q83：当前短期规划完成后，规划 AI 应如何产生“下一份短期计划”？
**背景：**
Q82 已确认最终通过后规划 AI 可以继续从长期规划选择下一系统，不能停下来等待新指示。需要明确它是直接开工，还是先生成下一周期计划。

**选项：**
- **A：规划 AI 必须先基于长期规划、当前 Capability Ledger、未完成 Backlog、依赖关系与风险，自动生成一份新的“下一周期短期计划”，再按该计划继续安排工作 AI；无需等待 Director 预审批，但仍保留该周期最终 Director Gate。**
- **B：规划 AI 不需要生成新短期计划，直接从长期规划挑一个系统开工即可。**
- **C：每次完成后都必须等 Director 亲自指定下一短期目标。**
- **D：永远沿用当前这份计划，不再生成新周期计划，只不断追加阶段。**

**推荐：** A

**推荐理由：**
这能让“自动继续”仍然有清晰边界和阶段结构，不会退化成规划 AI 从长期规划里自由抓任务。

**Director 回答：** A

**已确认结论：**
当前短期周期完成并通过最终 Director Gate 后，规划 AI 必须基于：
- 长期规划
- 当前 Capability Ledger
- 未完成 Backlog
- 依赖关系
- 风险
- 当前 Runtime / 测试事实

自动生成 **下一周期短期计划**，再继续安排工作 AI。

无需等待 Director 对每个新周期进行预审批，但新周期仍保留最终 Director Gate，以及 Q66 / Q86 规定的中途请求条件。

---

### Q84：后续自动生成的短期计划，是否自动继承本次讨论形成的治理协议？
**背景：**
本次已确认大量通用治理规则：单 Primary Work Order、Non-Blocking、Evidence Pack、Phase Snapshot、Baseline Diff、Source-of-Truth、Director 中断条件等。若每个新周期都重新讨论，会失去自动连续推进的意义。

**选项：**
- **A：自动继承。后续所有短期周期默认继承本次已确认的“规划 AI 治理协议”，除非新周期的产品系统天然冲突，规划 AI 才能提出显式变更。**
- **B：每个新周期重新定义全部治理规则。**
- **C：只继承 Work Order 格式，其余重新决定。**
- **D：由规划 AI 自由判断继承哪些规则。**

**推荐：** A

**推荐理由：**
把本次成果真正变成“规划 AI 操作系统”，而不是只服务 Build Diversity Alpha 的一次性文档。

**Director 回答：** A

**已确认结论：**
后续所有短期周期默认继承本次讨论形成的 **规划 AI 通用治理协议**，包括但不限于：
- 单 Active Primary Work Order
- Work Order 强制模板
- Non-Blocking Tasks
- Blocked Resume Trigger
- Evidence Pack
- 阶段 Baseline
- Baseline Diff Gate
- Phase Snapshot
- Repository Reconciliation
- Source-of-Truth 顺序
- Capability Ledger
- Mechanic Support Matrix
- 中途请求 Director 条件
- 最终 Director Gate / Correction Loop

只有当新周期的产品/技术特性与现有治理规则天然冲突时，规划 AI 才能提出显式变更。

---

### Q85：规划 AI 从长期规划选下一个系统时，是否必须严格按长期规划原始阶段顺序？
**背景：**
长期规划 v1 有明确 Phase 顺序，但实际仓库已经经过简化版和 S4 演进，未来的依赖、风险和价值可能与最初顺序不同。

**选项：**
- **A：严格按长期规划原始 Phase 顺序，不允许跳过或重排。**
- **B：允许证据驱动重排。规划 AI 可以根据 Capability Ledger、依赖、风险、当前内容缺口和玩家价值调整顺序，但必须记录为什么偏离长期规划；不得跳过仍是硬依赖的前置能力。**
- **C：规划 AI 完全自由选择任何长期系统。**
- **D：每次重排都必须请求 Director。**

**推荐：** B

**推荐理由：**
既保留长期规划作为 North Star，又允许真实工程状态修正早期纸面顺序。

**Director 回答：** B

**已确认结论：**
规划 AI 不必机械遵循长期规划 v1 的原始 Phase 顺序。

允许 **证据驱动重排**，依据包括：
- Capability Ledger
- 系统依赖
- 当前风险
- 内容缺口
- Build / Gameplay 价值
- 当前工程事实

任何偏离长期规划原始顺序都必须记录原因。
仍属于硬依赖的前置能力不得被跳过。

---

### Q86：规划 AI 自动进入“下一周期”时，什么时候必须再次中途请求 Director？
**背景：**
Q66 已定义当前周期中途请求 Director 的条件；现在要确认跨周期是否有额外约束，避免规划 AI 自动开启一个事实上改变产品方向的新阶段。

**选项：**
- **A：沿用 Q66，并增加一条：如果下一周期候选系统会引入此前未批准的重大产品语义（例如联网、多角色职业重构、商业化经济、根本战斗模式变化等），必须先请求 Director；如果只是长期规划里已明确批准的系统，则可自动生成下一短期计划并执行。**
- **B：所有下一周期都无需再请求 Director，长期规划中出现过就自动做。**
- **C：每个新周期开始前都必须 Director 批准。**
- **D：只有技术架构变化才请求 Director，Gameplay 语义变化也可自动。**

**推荐：** A

**推荐理由：**
能保持自动滚动，又保留真正的产品方向控制权。

**Director 回答：** A

**已确认结论：**
跨短期周期沿用 Q66 的中途请求 Director 规则，并增加：
- 如果下一周期候选系统会引入此前未批准的重大产品语义，必须先请求 Director；
- 如果候选系统已经明确存在于长期规划并且不改变核心产品方向，规划 AI 可以自动生成下一短期周期并执行。

“长期规划中出现过”不等于可以修改任何已锁定 Director 决策；冲突时仍以 Director 决策优先。


---

### Q87：新版短期规划在仓库中如何替代旧“简化版_v0”？
**背景：**
目标是让新版成为规划 AI 的当前执行依据。如果旧简化版仍以“当前计划”身份留在根目录，规划 AI 未来可能读取到两套冲突执行规则。

**选项：**
- **A：保留 `简化版_v0.md` 作为历史归档，但在文件顶部明确标记 Superseded，并指向新版短期规划；新版规划成为唯一当前执行计划。**
- **B：直接删除旧简化版文件，只保留新版。**
- **C：两个文件并存且都有效，由规划 AI按情况选择。**
- **D：继续修改旧简化版原文件，不新建新版文件。**

**推荐：** A

**推荐理由：**
既保留历史决策脉络，又不会制造两个“当前真相”。

**Director 回答：** B

**已确认结论：**
新版短期规划正式生效时，旧 `简化版_v0.md` **直接删除**。
不再保留为历史归档，也不得继续作为规划 AI 的任何执行依据。

历史决策追溯由：
- QA Director Decision Record
- Git 历史
- Phase Snapshot
- 新版短期规划中的 Supersedes / Created From 信息

承担。

---

### Q88：新版短期规划是否采用版本化文件名与状态头？
**背景：**
Q83 已确认未来会自动滚动生成下一周期短期计划。如果每次都叫“短期规划.md”，历史追踪和跨周期恢复会很混乱。

**选项：**
- **A：采用版本化命名和固定状态头，例如 `SHORT_TERM_PLAN_BDA_v1.md`，头部记录 Status、Cycle ID、Canonical Data Version、Created From、Supersedes、Director Decisions Ref、Entry Commit、Current Phase。后续周期生成新的独立文件。**
- **B：永远只有一个 `SHORT_TERM_PLAN.md`，每轮直接覆盖。**
- **C：只按日期命名，不设 Cycle ID。**
- **D：由规划 AI 每次自由命名。**

**推荐：** A

**推荐理由：**
最适合滚动规划和 Repository-as-Memory，也能让规划 AI 清楚“当前周期”和“历史周期”的边界。

**Director 回答：** A

**已确认结论：**
新版短期规划采用版本化文件名与固定状态头。

建议命名形态：
`SHORT_TERM_PLAN_BDA_v1.md`

状态头至少记录：
- Status
- Cycle ID
- Canonical Data Version
- Created From
- Supersedes
- Director Decisions Ref
- Entry Commit / Baseline
- Current Phase

后续滚动周期生成新的独立短期规划文件，不直接覆盖旧周期文件。

---

### Q89：本次 QA 文档在正式规划中的地位是什么？
**背景：**
你已经要求所有 Q&A 持续记录。本次 QA 里包含大量 Director 覆盖推荐项的明确决策，如果最终规划里出现歧义，需要决定是否可以回查 QA 作为权威解释。

**选项：**
- **A：QA 作为正式 Director Decision Record 保存。正式短期规划必须引用它；若规划正文与 QA 中明确 Director 回答冲突，以 QA 中较新的明确 Director 决策为准，并要求修正规划正文。**
- **B：QA 只是讨论记录，正式规划写完后不再具有约束力。**
- **C：把 QA 全部复制进正式规划正文，之后删除 QA。**
- **D：只保留最终结论，不保留问题和推荐过程。**

**推荐：** A

**推荐理由：**
它能保存“为什么这样定”和“Director 明确覆盖过哪些推荐”，非常适合未来规划 AI 处理歧义和追溯设计意图。

**Director 回答：** A

**已确认结论：**
本 QA 文档作为正式 **Director Decision Record** 保存。

正式短期规划必须引用该 QA。
若规划正文与 QA 中明确 Director 回答冲突：
- 以较新的明确 Director 决策为准；
- 规划 AI 必须要求修正规划正文；
- 不得用“规划文档写的是另一套”覆盖 Director 已确认决策。

QA 保留完整问题、推荐、Director 回答与结论，不仅保留最终答案。

---

### Q90：规划 AI 是否负责主动维护这些治理文档？
**背景：**
如果规划 AI 只下工作令、不维护计划/状态/阶段快照，那么长期运行后 Repository-as-Memory 仍会漂移。

**选项：**
- **A：是。规划 AI 负责维护短期计划状态、Phase Snapshot、Capability Ledger、Mechanic Matrix 索引、STATUS/ROADMAP 当前摘要、Drift/Blocked 记录；工作 AI 负责提供实现侧证据和必要文档更新，规划 AI 在阶段 Reconciliation 时统一校正。**
- **B：所有文档都由工作 AI 自己维护，规划 AI 只下单。**
- **C：规划 AI 只维护 SHORT_TERM_PLAN，其他文档不管。**
- **D：文档只在最终 Director Gate 前统一补一次。**

**推荐：** A

**推荐理由：**
“规划 AI 负责当前真相，工作 AI 负责实现证据”这一分工最适合你现在的长期无人监管模式。

**Director 回答：** B

**已确认结论：**
治理文档由 **工作 AI 负责实际维护和落盘**。

规划 AI 的主要职责仍是：
- 生成 Work Order；
- 定义验收；
- 独立复核 Evidence Pack；
- 检查文档是否与当前事实一致；
- 在阶段 Reconciliation 时判定文档是否合格。

规划 AI 不直接承担日常文档写入，但有权要求工作 AI 修正文档后才通过阶段 Gate。


---

### Q91：既然工作 AI 负责治理文档，规划 AI 是否必须把“文档更新”写成每个 Work Order 的硬验收项？
**背景：**
Q90 已确认工作 AI 负责文档落地。如果文档更新只是“建议”，很容易再次出现 Runtime 已变化、Repository-as-Memory 没同步的问题。

**选项：**
- **A：是。凡是会改变 Runtime、Canonical Data、Capability、Matrix、阶段状态、阻塞状态或 Director 决策映射的 Work Order，都必须把对应文档更新写进 Acceptance Criteria；缺文档即未完成。纯内部重构且不改变外部事实时可不更新高层文档。**
- **B：文档更新只作为 Evidence Pack 的可选项。**
- **C：只在阶段末统一补文档，Work Order 内不要求。**
- **D：工作 AI 自己判断要不要更新。**

**推荐：** A

**推荐理由：**
这样既保留“工作 AI 负责写”，又确保规划 AI 能通过硬验收防止文档漂移。

**Director 回答：** A

**已确认结论：**
凡 Work Order 会改变 Runtime、Canonical Data、Capability、Mechanic Matrix、阶段事实、阻塞事实或 Director 决策映射，必须把对应文档更新写入 Acceptance Criteria。
缺少必要文档同步即视为该 Work Order 未完成。
纯内部重构且不改变外部事实时，可不更新高层治理文档。

---

### Q92：规划 AI 是否可以直接修改“计划状态字段”，还是也必须让工作 AI 改？
**背景：**
Q90 选择所有治理文档由工作 AI 维护，但规划 AI 自己最清楚哪个 Work Order 已通过、哪个阶段已完成。如果连状态字段都不能改，可能出现验收完成但状态尚未同步。

**选项：**
- **A：规划 AI 不直接修改任何仓库文件；所有状态更新都通过一个专门的 Documentation/State Work Order 交给工作 AI 执行，随后规划 AI 复核。**
- **B：规划 AI 允许直接修改纯治理状态字段（如 Current Phase、Work Order 状态、Gate 结果、Blocked 状态），但不修改实现文档和 Gameplay 代码。**
- **C：规划 AI 可以随意修改任何文档，只是不写代码。**
- **D：阶段状态只在最终 Director Gate 后更新。**

**推荐：** B

**推荐理由：**
这样可以避免“验收者无法更新自己判定的状态”这一摩擦，同时仍把实现侧文档维护留给工作 AI。

**Director 回答：** D

**已确认结论：**
阶段状态字段只在 **最终 Director Gate 后** 更新。
规划 AI 不直接通过日常验收去改写正式阶段状态；阶段内实际推进由 Work Order、Evidence Pack、Phase Snapshot 与执行记录证明。
最终 Director Gate 完成后，再由工作 AI 按验收结果统一更新正式状态字段。

---

### Q93：删除旧 `简化版_v0.md` 后，是否要在新版计划中保留一段“迁移说明”？
**背景：**
Q87 已确认直接删除旧文件。未来规划 AI 如果只看到新版，不知道为什么旧路线消失，可能无法理解 S0～S4 与新周期的关系。

**选项：**
- **A：是。新版计划开头保留简短 Migration Note：说明旧简化版已完成使命并被删除，新计划接管执行；列出从 S4 吸收哪些有效工作、哪些旧规则被废止。**
- **B：不需要，删除就是删除，不再提旧计划。**
- **C：把旧简化版全文复制到新版附录。**
- **D：只在 Git commit message 里说明。**

**推荐：** A

**推荐理由：**
不会保留两套计划，却能让未来 AI 明白为什么执行模型发生变化。

**Director 回答：** B

**已确认结论：**
新版计划正文中 **不保留旧简化版迁移说明**。
旧文件按 Q87 直接删除；新版计划不再解释旧文件为什么消失，也不复制旧内容。
历史追溯依赖 Git、QA Director Decision Record 与阶段证据。

---

### Q94：本轮 QA 讨论什么时候可以结束并进入正式规划编写？
**背景：**
我们已经确认 90+ 个决策。继续无限追问会出现边际价值快速下降。需要明确“问题已经足够完整”的退出条件。

**选项：**
- **A：当剩余问题只涉及实现细节，且不再影响范围、阶段、权限、Gate、数据来源、核心玩法语义时，就停止 grill-me，直接编写正式新版短期规划；实现细节交给规划 AI 在 Work Order 中解决。**
- **B：继续讨论，直到所有可能的系统细节都提前决定。**
- **C：现在立即停止讨论并开始写正式规划。**
- **D：至少再固定讨论 20 个问题再判断。**

**推荐：** A

**推荐理由：**
目前核心框架已经非常完整。接下来应该只继续追问真正会改变计划结构的问题，而不是把 Work Order 层面的实现选择也全部提前锁死。

**Director 回答：** C

**已确认结论：**
Director 明确决定：**立即结束 grill-me 讨论并进入正式新版短期规划编写。**

从此点开始：
- 不再继续追加前置设计问题；
- 已确认 Q&A 作为正式 Director Decision Record；
- 尚未锁定的纯实现细节由规划 AI 在 Work Order 层基于本规划自主解决；
- 若未来触发 Q66 / Q86 所定义的重大语义冲突或全局死锁，再重新请求 Director 决策。


---

# 讨论封口

**状态：CLOSED / READY FOR FORMAL PLAN**

本轮 grill-me 讨论已经由 Director 在 Q94 明确结束。  
后续 `SHORT_TERM_PLAN_BDA_v1.md` 为正式执行规划，本 QA 为其 Director Decision Record。

若二者出现冲突：
1. 较新的明确 Director 决策优先；
2. 工作 AI 必须修正规划或实现侧文档；
3. 不允许用旧计划、旧 STATUS 段落或历史实现解释覆盖本 QA 中的明确决策。
