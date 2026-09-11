# S6P — Passive Truth & Deterministic Build Backbone

来源：规划 AI 会话（`game-zzz-planning`）2026-09-11 回复，全文见
`docs/reviews/S6P/S6P_PLANNER_REPLY_WO_RELEASE.md`。
本文件是**可执行摘要**；冲突时以规划 AI 原文与仓库工作令为准。

## 周期定位

S5U（UI/HUD 改版 + 真天赋域接入）已收口。S6P 不开新玩法域，只做一件事：

> **让门禁真正覆盖天赋域，然后把天赋域的真话说清楚、说到做到。**

药剂 / 珠宝 / 升华 / Timeless Jewel / 新技能 / 新 Stat 轴（冰冷、闪电、ES、格挡、压制、
召唤物、图腾、战吼、异常、充能、吸取、DoT）**全部排除出无人值守队列**——它们需要导演的玩法授权，
只能出现在 census / dependency report 里，不得作为阻塞项、不得自动实现。

## 工作令序列

| 令 | 标题 | 预期 hash |
|---|---|---|
| S6P-WO-01 | Passive Truth Census & Gate Coverage Audit | **必须保持 `FNV1A64:9a4c9524d0b3e214`** |
| S6P-WO-02 | Passive-Aware Production Simulation | 允许变化，3× 重基线 |
| S6P-WO-03 | Mastery Explicit Selection Correctness | 预期变化，3× 重基线 |
| S6P-WO-04 | Supported Passive Semantic Closure & No-Silent-Noop Guard | 预期变化，3× 重基线 |
| S6P-WO-05 | Passive Tree Overview LOD & Texture Residency | **必须等于 WO-04 基线** |

**一轮一令**：每张 Evidence Pack 先经规划 AI Gate Review，上一单 ACCEPT 后才释放下一张。

---

## S6P-WO-01 — Passive Truth Census & Gate Coverage Audit

**性质**：无 runtime 变更（域层 NONE / 绘制层 NONE）。

**为什么现在做**：这是所有后续工作的分母。当前同时存在 2429 主树节点 / 315 专精 /
57 珠宝孔 / 30 时光珠宝类 / 558 升华 / 导演口径 2832 / 531 节点已产出 modifier。
集合包含关系没定死之前，不能用「78% 未覆盖」指导代码工作。
同时必须先确认 ProdSim 到底有没有消费被动状态。

**改动面**：新增 `S6PassiveCensusTests`（或同族）+ census/report 生成器 + ProdSim 覆盖审计；
文档三份：passive census、semantic coverage matrix、ProdSim surface map。

**完成口径（机械判定）**

- 权威节点集合对账完成，且 2429 / 2832 / 315 / 57 / 30 / 558 六个数字的含义**逐条显式写明**；
  源数据证明不了的关系写 `UNRESOLVED_WITH_SOURCE`，**禁止用加法凑数**。
- 每个主树节点分类完成；每条被动效果行分类完成；`UNKNOWN` 语义行 = 0。
- 效果行只能落入四类：`CONSUMED` / `BLOCKED_BY_DOMAIN` / `SPECIAL_INTERACTION` / `STRUCTURAL`。
- 必须给出三个是非：`ProdSim reads allocated passive state` / `ProdSim output depends on
  passive modifier result` / `ProdSim mastery-sensitive`。**不能猜。**
- 顺带 census 被动分配不变量当前是否已有权威测试：合法起点、连通加点、禁止跳点、
  点数守恒、重复加点幂等、R 重置精确、专精前置。缺哪条就登记进 WO-03/04 补回归。

**证据**：EditMode 全 PASS + 新增 census 断言（节点 ID 唯一 / 分类总数对账 / 类别重叠对账 /
效果行无 UNKNOWN / parser 结果与 consumer 清单可关联）；PlayMode 全 PASS（不要求新增）；
ProdSim `9a4c9524d0b3e214` unchanged。

**止损线**：为了把数字对上而擅自解释 2429/2832 —— 一律写 `UNRESOLVED_WITH_SOURCE`。

---

## S6P-WO-02 — Passive-Aware Production Simulation

**为什么第二**：这是本轮最高优先级的**被漏掉的骨架项**。如果 ProdSim 不消费被动状态，
那么在 WO-03 改专精时，「hash 不变」根本证明不了任何事。

**改动面**：ProdSim 场景 + canonical 被动 loadout fixture + hash payload + 确定性报告。
**不得为了测试改 passive runtime。**

**canonical 场景最低覆盖**：确定性初始态 / 确定性加点 ID 序列 / 至少一个当前 `CONSUMED` 的
进攻向被动 / 至少一个 `CONSUMED` 的防御或属性向被动 / 结果玩家有效属性 / 已分配节点 ID /
产出的被动 modifier。**暂时不要把「专精默认首条生效」这个已知错误行为写进基线**（那是 WO-03）。

**完成口径**：必须证明两个 mutation —— A：改 canonical 被动分配 → hash **必须**变；
B：恢复分配 → hash **必须**精确复原。然后跑 ≥3 次建立新 canonical hash。

**哈希规则**：允许变化，理由不是玩法变了，而是 evidence surface 扩大、终于把被动构筑真相纳入 hash。

**止损线**：禁止把 UI 状态、整张 2429 节点 JSON、texture/path、字典迭代顺序放进 gameplay hash；
禁止为了凑稳定 hash 去固定错误状态。hash 只纳入游戏真相。

---

## S6P-WO-03 — Mastery Explicit Selection Correctness

**为什么第三**：这是当前**已知的明确 gameplay correctness defect** —— 315 个专精节点有多个
可选效果，系统却默认第一条自动生效。优先级高于 LOD、地图页、制作页。

**改动面**：域层新增类似 `MasterySelection`（nodeId → selectedEffectId）；Session 的分配 /
选择 / 重置 / modifier 聚合；绘制层点击专精弹**有界**选择框（呈现全部候选 + 当前选择 +
confirm/cancel）。

**完成口径（机械证明）**：未选专精效果 = NONE；分配专精必须显式选择；选中后恰好一个效果生效；
改选 → 旧效果移除 + 新效果生效 + **绝不同时**；R 重置 → 分配与专精选择一起清空；
同一轮内 session 重建 → 状态确定。**禁止 `selected = choices[0] when missing`。**

**UI 验收不看「弹窗漂亮」**：弹窗矩形在 1080p/1440p 屏内；N 个候选 = N 个命中矩形；
候选 ID 与官方数据 ID 一致；点击坐标 → 确定性选中对应效果；Esc/cancel 不改状态。截图仅为辅助。

**最大坑**：UI 选的是 B，但聚合器偷偷还在用 A。所有测试必须端到端：
`点击/选择状态 → 有效 modifier/属性`。

---

## S6P-WO-04 — Supported Passive Semantic Closure & No-Silent-Noop Guard

**为什么排专精后**：到这里才适合处理「531/2429 之外还有什么能马上支持」。
但**不是**去实现冰冷/闪电/ES/召唤物/药剂等新域——只处理「runtime 已有真实 consumer，
但 parser 还漏掉的被动效果」。是把已有发动机接完整，不是新造发动机。

**最重要的新增规则**：任何节点只要含有 runtime 无法真实兑现的效果，
**不得静默只吃其中支持的那部分数字**。资格判定：

```
全部 gameplay 效果行可消费            → SUPPORTED
特殊交互另行处理                      → SPECIAL
否则                                  → UNSUPPORTED_CURRENTLY
```

`UNSUPPORTED_CURRENTLY`：明确视觉标记 + hover 说明当前工程未支持 + **禁止花点分配**。

**完成口径**：真正的 Gate 不是 `coverage >= 50%`，而是
`仅需现有引擎语义的效果行：Parsed = 100%，Runtime-consumed = 100%`，
且 `Partial semantic application = 0`、`Condition dropped = 0`、
`Silent ignored allocated effects = 0`、`Dead new StatIds = 0`。
`BLOCKED_BY_DOMAIN` 可以很多，不算失败。

**最危险的坑**：`"10% X while Y"` 被实现成 `+10% X always`。
发现条件引擎不支持时，**整条 BLOCKED**，而不是删掉条件。

---

## S6P-WO-05 — Passive Tree Overview LOD & Texture Residency

**为什么第五**：高价值可用性工程，但不是玩法语义前置。前四单先保证
「点什么、选什么、算什么都是真的」，再解决「整树缩小时看不懂」。

**LOD 合同**：按**实际屏幕投影大小**切档，不是只看 zoom 值。

- LOD0 Overview —— 普通节点画点；显著/基石/专精各有区分符号；已点亮强高亮。
  关闭：完整 512² 图标、簇装饰底衬、细节框。
- LOD1 Mid —— 显著/专精/基石用美术；普通节点简化；簇装饰减量。
- LOD2 Detail —— 完整图标/框/悬停/标签。

**完成口径**：适配全屏缩放下普通节点完整图标绘制数 = 0；命中测试 NodeId 在三档之间一致；
世界↔屏幕变换在容差内；已点亮路径在三档之间一致。

**Headless Chrome 对拍**：继续用现有官方数据渲染器，固定 viewport/树心/zoom/pan，
输出 overview/mid/detail，自动比较节点中心、边的端点、可见类别掩码、已点亮路径、裁剪。
**不要求 Chrome 与 Unity 像素级美术一致**，只要求 geometry/state oracle 一致。

**显存 Gate**：先测「树关闭/俯瞰/细节三态下的常驻纹理、独立图标加载数、图集纹理数、峰值纹理内存」。
目标：LOD0 不因 2429 节点加载全部完整图标；同一纹理不重复实例化；优先用官方图集 UV；
树关闭后非必要动态资源可释放；无逐帧纹理分配。

**止损线**：禁止借显存优化改官方节点坐标、改 node ID、改分配语义、改 modifier、重采样 canonical data。

---

## 候选清单重分类（规划 AI 裁定）

| 项 | 裁定 |
|---|---|
| 专精选择交互 | **现在就做** —— 不是 UX，是 correctness |
| 词条映射（现有语义补齐） | **现在做** |
| 词条映射（新 Stat 轴） | **不做** —— 没有 runtime consumer 就是新 gameplay domain，不能靠扩大 parser 自行启动 |
| 药剂系统 | 玩法骨架，但需导演授权，**不进无人值守队列** |
| 普通珠宝孔 | 构筑骨架，属 Item/Passive interaction 域，需导演授权，**不进本轮** |
| Ascendancy | 玩法骨架但更晚，且在 558 节点导入之外还要 class/ascendancy identity、点数获取、分配重置、特殊机制 |
| Timeless Jewel | 最晚，依赖 普通 Jewel → 稳定 passive 语义 → radius/transformation → 确定性 seed |
| 整树俯瞰可读性 | **重要可用性基础设施**，排在语义正确性之后、页面 polish 之前 → 所以进 WO-05 |
| 512² 图标显存 | **production scalability**，不是单纯 polish，与 LOD 同单处理 |
| 地图页 / 制作页新版 | **后置** —— 功能已存在，UI 丑不影响 gameplay truth |
| 滚动条皮肤 | 最低优先级 |
| R/T 空槽 | **保持空置**，正确表现为 `EMPTY / RESERVED`，不为填 UI 新造技能 |
| 四个药剂槽 | 药剂系统获授权前明确表现为 `RESERVED / NO FLASK EQUIPPED`，不能让玩家点了没反应 |

## 被漏掉但更该先做的三件事（规划 AI 指出）

1. **ProdSim 对被动是否真的敏感** —— 第一优先。FNV1A64 的绿灯现在是局部绿灯。
2. **Unsupported passive 的「静默空效果」** —— 花点点亮但 runtime 效果为空，属错误产品语义。
3. **被动分配不变量** —— 合法起点 / 连通加点 / 禁止跳点 / 点数守恒 / 重复加点幂等 /
   R 重置精确 / 专精前置。WO-01 顺带 census，缺的权威测试在 WO-03/04 补回归。

## 若无人值守只能做三件

1. Passive Census + Passive-aware ProdSim（即 WO-01 → WO-02 连续完成）
2. Mastery Explicit Selection
3. Existing-Domain Semantic Closure + No-Silent-Noop Guard

整树 LOD 排第四；地图/制作页重皮、滚动条更后。

## 哈希策略总表

每次允许变化的 WO，Evidence 里必须同时给：
`Before Hash` / `After Hash × 3` / `Exact Repeat = YES` / `Reason For Change` /
`Changed Canonical Fields` / `Unexpected Hash Inputs = NONE`。

## 立即释放

**`S6P-WO-01 — Passive Truth Census & Gate Coverage Audit`**。本轮不碰 runtime，故预期：

```
EditMode            : ALL PASS
PlayMode            : ALL PASS
ProdSim             : FNV1A64:9a4c9524d0b3e214
Drift               : 0
Forbidden Expansion : PASS
```

## 这条计划最关键的转向

> **不要再把「词条映射覆盖率」当 KPI。**
> 下一阶段的判据是：**当前声称支持的节点 100% 真生效，当前不支持的节点 0% 假生效。**
