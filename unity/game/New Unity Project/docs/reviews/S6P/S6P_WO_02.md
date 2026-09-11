# S6P-WO-02 — Passive-Aware Production Simulation

周期：S6P — Passive Truth & Deterministic Build Backbone
来源：规划 AI 会话（`game-zzz-planning`）2026-09-11 Gate Review ACCEPT + 完整合同（31 条 AC），
原文见 `docs/reviews/S6P/S6P_WO_01_GATE_REVIEW_AND_WO_02_CONTRACT.md`
性质：**只扩大 evidence surface**。Gameplay Delta = NONE / Passive Runtime Delta = NONE /
Canonical Data Delta = NONE / Content Delta = NONE。授权的改动面只有
`Production Simulation Tool` 与 `Canonical Evidence Surface`。
状态：**Gate Review = ACCEPT（31/31 AC 全满足，Follow-up = NONE）**。新 canonical baseline
`FNV1A64:ec1d3ed67d3035d0` 正式生效；`9a4c…` 降级为 predecessor reference。WO-03 已释放。
Gate Review 原文：`docs/reviews/S6P/S6P_WO_02_GATE_REVIEW_AND_WO_03_CONTRACT.md`；
发出的 Evidence Pack：`docs/reviews/S6P/S6P_WO_02_EVIDENCE_PACK.md`。

---

## §1 本单要解决什么

WO-01 的审计结论是 **`NOT_PASSIVE_SENSITIVE`**：旧哈希只吃「每轮生成的物品内容 + 循环序号」，
改天赋、改专精、把某节点从有效果改成没效果，`FNV1A64:9a4c9524d0b3e214` 都不会动。
于是「hash unchanged」这盏绿灯对天赋域几乎没有保护作用——WO-03 改专精时，它也证明不了任何事。

本单把 Production Simulation 从：

```
loot → craft → equip
```

扩展为：

```
loot → craft → equip → passive allocation → passive modifier → effective gameplay stat
```

**本单不修 Passive、不修专精、不碰 parser、不加 StatId。** 旧 hash 从本单起只是
`PREDECESSOR REFERENCE`，不是 expected oracle。

---

## §2 交付物

| 项 | 位置 | 说明 |
|---|---|---|
| canonical 被动场景 + 哈希负载序列化 | `Assets/Tests/EditMode/PassiveAwareProductionSimulation.cs`（新增） | 纯 tooling 层；fixture 冻结常量；负载构造 |
| 门禁套件 | `Assets/Tests/EditMode/PassiveAwareProductionSimulationTests.cs`（新增） | 15 条断言，覆盖合同 §17 全部 13 项 + 技能属性面 + 报告面 |
| 仿真器改造 | `Assets/Tests/EditMode/ProductionSimulator.cs`（修改） | 每 session 建立 canonical loadout；被动语义状态进 FNV 负载；报告 schema V1→V2 + 敏感性块 |
| census 审计翻转 | `Assets/Tests/EditMode/PassiveCensus.cs` / `PassiveCensusTests.cs`（修改） | 期望结论由 `NOT_PASSIVE_SENSITIVE` 翻为 `PASSIVE_SENSITIVE`；新增"哈希喂点禁用 token"扫描 |
| 机器可读产物 | `docs/qa/PRODUCTION_SIMULATION_REPORT.json`（V2） | 测试再生，禁止手填 |
| census 产物 | `docs/qa/PASSIVE_CENSUS_REPORT.json` | 随门禁再生，`prodSimSurface.verdict = PASSIVE_SENSITIVE` |

**census 的单真值重构**：`ClassifyLine` / `ClassifyNode` 抽出为 `PassiveCensus` 的 internal 唯一实现，
`Collect()`、WO-02 fixture 资格判定、新增断言全部调用同一处——避免出现第二套 blocked/special 规则。

---

## §3 canonical 被动场景（冻结）

```
ScenarioVersion = s6p-wo02-canonical-v1
StartNode       = 2172（"Seven"；= SliceSession.StartNode，树数据漂移即红）
Fixture NodeIds = 559, 1795, 2034        ← 冻结常量，禁止 find-first-supported 动态挑选
Allocated Set   = 559, 1795, 2034, 2172  （起点恒已点亮，一并计入身份）
```

| NodeId | 名字 | 资格（WO-01 census） | 效果行 | 产出 |
|---|---|---|---|---|
| 559 | Attack Speed and Dexterity | `SUPPORTED` | 2 CONSUMED | `AttackSpeed:Increased:0.04`（进攻）、`Dexterity:Flat:5`（属性） |
| 1795 | Physical Damage and Strength | `SUPPORTED` | 2 CONSUMED | `PhysicalDamage:Increased:0.1`（进攻）、`Strength:Flat:5`（属性） |
| 2034 | Life and Strength | `SUPPORTED` | 2 CONSUMED | `Life:Flat:12`（防御）、`Strength:Flat:5`（属性） |

- **Mastery Nodes In Fixture = 0**；**Unsupported Nodes In Fixture = 0**；
  **Special-interaction Nodes In Fixture = 0**（三节点均为 `SUPPORTED`）。
- 三节点都是起点的直接邻居 → 任意加点顺序都满足 connected allocation；
  合法性**不在此处假设**，由 `SliceSession.TryAllocate` 判定（见 §6 反证断言）。
- 覆盖：进攻向 CONSUMED ≥1（实为 2 条）、防御/属性向 CONSUMED ≥1（实为 4 条），
  单一 sub-scenario 即满足，无需拆两个。

---

## §4 canonical 哈希负载（`pv|passive-v1`）

每 session 在 cycle 循环**之前**喂入一次：

```
hash = HashString(hash, "|" + PassiveAwareProductionSimulation.StatePayload(session));
```

负载本体（顺序无关，全部 canonical 化）：

| 段 | 内容 | 排序 |
|---|---|---|
| `pv|passive-v1` | schema marker（合同 §16：显式区分"数据变了"与"evidence surface 升级了"） | — |
| `ps|` | 全部已分配 NodeId | 数值升序 |
| `pm|` | 实际进入 runtime 的 passive modifier 语义元组 `nodeId:statId:Stat:opId:Op:value:tags:condition` | (NodeId, StatId, Op, Value, Tags, Condition) |
| `pe|` | 有效 gameplay 属性：`StatId:Name=有效值/base+flat/increased/more` | StatId 数值序全枚举 |
| `pk|` | 有效技能属性（Melee/Projectile/Area × StatId 同上格式） | skill 序 × StatId 数值序 |

**为什么 `pe|`/`pk|` 要带 raw 分量**：`+10% increased PhysicalDamage` 作用在 0 基底上，
`StatBag.Get()` 仍是 0——只存最终值等于看不见进攻轴。带 `increased/more` 分量后，
`pk|` 里 11=0/0/**0.3035488**/1、26=0/0/**0.04**/1 才是真证据。

**为什么进攻轴要单独建 `pk|`**：进攻被动只进 `CollectSkillMods` 的技能包，不进 `PlayerStats`；
没有 `pk|` 的话，"点亮了但技能侧没生效"这类路径不会被哈希捕获。

**明确不进入哈希**（合同 §6）：节点坐标 / zoom / pan / icon path / texture / sprite UV / 簇视觉 /
tooltip / 本地化文本 / GUI 状态 / hover / 选中节点 / 窗口尺寸 / 分辨率 / 资产与文件时间戳 /
字典迭代序 / `tree_raw.json` 字节 / 2429 个节点定义本身。长度上界由断言守住（< 8192 字符）。

**分配事件面**（合同 §4）：`pa|<nodeId>|<ok|reject>|<remainingPoints>`，本单取值为
`pa|559|ok|122` / `pa|1795|ok|121` / `pa|2034|ok|120`。
事件 trace 进**报告**（`passiveSimulation.allocationEvents`）作为"确实走了 domain API"的证据，
**不进哈希**——合同 §8 要求最终 semantic-state serialization 必须 canonicalized 且对无意义的
顺序不敏感；顺序不敏感这件事因此可以被全量哈希直接证明（见 §6）。

---

## §5 报告 schema

`PRODUCTION_SIMULATION_REPORT.json`：`schemaName` V1→**V2**、`schemaVersion` 1→**2**、
新增 `simulationContractVersion`、`passiveSimulation`、`passiveSensitivity` 三个块：

```json
"passiveSimulation": {
  "scenarioVersion": "s6p-wo02-canonical-v1",
  "allocatedPassiveNodeCount": 4,
  "allocatedPassiveNodeIds": "559,1795,2034,2172",
  "passiveModifierCount": 6,
  "passiveOffensiveTupleCount": 2,
  "passiveDefensiveOrAttributeTupleCount": 4,
  "passiveEffectiveStatSnapshot": "0:Life=10000031/10000031/0/1|…",
  "passiveEffectiveSkillStatSnapshot": "s1:…,11=0/0/0.3035488/1,…,26=0/0/0.04/1,…",
  "allocationEvents": [ "pa|559|ok|122", "pa|1795|ok|121", "pa|2034|ok|120" ],
  "passiveSensitive": true
}
```

报告仍是确定性渲染（无时间戳/GUID/绝对路径）；整棵树不塞进报告。

---

## §6 敏感性证明（合同 §7 必做，不是"跑三次同 hash"）

同一 seed、同一物品生成序列，**只改被动面**：

| 证明 | 变体 | 结果 |
|---|---|---|
| A 分配身份 | canonical `{559,1795,2034}` vs `{559,1795}` | `ec1d3ed67d3035d0` → **`32b23c480e05ec9a`** ≠ |
| B 运行期效果 | canonical vs `{559,2034}`（合法，有效属性不同） | `ec1d3ed67d3035d0` → **`4fc17552175c144a`** ≠ |
| C 恢复 | 回到 canonical | **`ec1d3ed67d3035d0`** 精确复原 = |
| D 顺序不变性 | 同一集合、相反加点顺序 `{2034,1795,559}` | **`ec1d3ed67d3035d0`** = |

- B 不靠改 catalog 数值：两个变体都是 domain API 接受（`|ok|`）的合法加点；
  `pe|` 的 Life 增量与 `pk|` 的 PhysicalDamage/AttackSpeed 增量被逐项断言。
- A/B 同时断言**词缀分布逐项相同** → 差异只能来自被动面，不是物品流分叉。
- 反证"没有绕过 domain"：另取一个与已分配集合不相连的 `SUPPORTED` 节点，
  `TryAllocate` 必须拒绝、不写容器、不扣点。

---

## §7 门

| 门 | 结果 |
|---|---|
| EditMode（最终 HEAD，全量） | **409 / 409 PASS / 0 FAIL**（394 前置全保留 + 本令新增 15） |
| PlayMode（最终 HEAD） | **14 / 14 PASS / 0 FAIL / 0 SKIP** |
| ProdSim 3 次独立全量运行 | **`FNV1A64:ec1d3ed67d3035d0` ×3 exact**（收尾在最终代码状态重跑：04:35:42 / 04:35:49 / 04:35:55 三次独立再生报告，非复制；开发期另有 01:32:52 / 01:33:02 / 01:33:12 三次同值运行） |
| ProdSim invalid / repeat | **invalidCount=0**、**repeatHashMatch=true** |
| 新 canonical hash | **`FNV1A64:ec1d3ed67d3035d0`**（未事前硬编码） |
| Predecessor | `FNV1A64:9a4c9524d0b3e214`（**仅参照**；变化原因 = evidence surface 扩大，非玩法改动） |
| Passive Sensitive | **YES**（census 静态结论也已翻转：`PASSIVE_SENSITIVE`） |
| Mastery Sensitive | **NO / DEFERRED**（fixture 专精数 = 0） |
| Content Audit | **PASS / fresh**（01:33:11 再生） |
| Drift | 0（运行期/被动/数据/内容 delta 全 NONE） |
| Source-of-Truth Conflicts | 0（census 单真值重构后只余一处判定实现） |
| Forbidden Expansion | PASS（无新玩法域、无采购、无换框架、未碰 parser/StatId） |
| Headless Chrome | **NOT REQUIRED**（本单不涉绘制，合同 §20） |

哈希策略总表 6 项：`Before 9a4c…` / `After×3 ec1d…` / `Exact Repeat=YES` /
`Reason=evidence surface 扩大（被动身份+modifier+有效属性首次入哈希）` /
`Changed Canonical Fields=新增 pv|/ps|/pm|/pe|/pk| 负载段` / `Unexpected Hash Inputs=NONE`。

---

## §8 本单明确排除（合同 §10/§11/§23）

| 排除项 | 状态 |
|---|---|
| 专精（315 节点）选择语义 | `EXCLUDED_BY_CONTRACT` —— 不能把 WO-03 即将修正的错误语义冻结成新 oracle |
| 2005 个 `UNSUPPORTED_CURRENTLY` 节点 | 排除；本单只消费 `SUPPORTED` 集合 |
| `PoeStatParser` 扩展 / 新 StatId / 新被动轴 | 未改一行 |
| 药剂 / 珠宝 / 升华 / 新技能 / 新词缀 / 地图制作页 / LOD / 显存优化 / 第三方框架 | 未触碰 |

排除的"非空转"证明：起点邻居里的 `71`（`UnsupportedCurrently`）被冻结为反证常量，
断言它确实被判为 unsupported；树里专精节点数为 315（>0），排除才是有意义的。

---

## §9 环境披露（如实）

- 本单**未发生编辑器挂死**，未使用 §Recovering Scene Backups 恢复配方，无 pipeline 超时。
- 例行操作：跑门禁前先 `editor_stop`（已确认 "Already in edit mode"）；新增 .cs 后
  `recompile` → 轮询 `recompile_status` 至 completed 才跑测试（否则 `run_tests` 会返回 0 tests）。
- PlayMode 走异步：`--async_tests true` + 轮询 `Temp\pipeline_test_status.json`（核对文件时间戳为本次运行）。
- 工作区状态：本单文件与 WO-01 产物同样处于**未提交**状态（沿用当前周期的仓库惯例，未做任何 git 写操作）。
- 中间态 hash 仅存在于开发过程，**未作为任何 oracle**；最终 canonical hash 只由最终代码状态的 3 次独立运行建立。

---

## §10 Evidence Pack（合同 §25 字段）

| 字段 | 值 |
|---|---|
| Work Order | S6P-WO-02 — Passive-Aware Production Simulation |
| Revision/Commit | HEAD `64b614f` + 未提交工作区（与 WO-01 同状态） |
| Changed Runtime Gameplay Files | **NONE** |
| Changed Passive Runtime Files | **NONE** |
| Changed Production Simulation Files | `Assets/Tests/EditMode/ProductionSimulator.cs`（schema V2 + 负载喂点 + 敏感性块）；`Assets/Tests/EditMode/PassiveAwareProductionSimulation.cs`（新增） |
| Changed Test Files | `Assets/Tests/EditMode/PassiveAwareProductionSimulationTests.cs`（新增，15 条）；`PassiveCensus.cs` / `PassiveCensusTests.cs`（审计口径翻转 + 单真值重构） |
| Changed Markdown | 本文件；`开发计划/UNATTENDED_STATE.md`；`开发计划/规划AI会话.md` |
| Changed Canonical Data | **NONE** |
| Entry EditMode | 394 / 394 |
| Entry PlayMode | 14 / 14 |
| Predecessor ProdSim Hash | `FNV1A64:9a4c9524d0b3e214` |
| Canonical Passive Scenario | 见 §3 |
| Scenario Version | `s6p-wo02-canonical-v1` |
| Starting Node | 2172 |
| Allocated Node IDs | 559, 1795, 2034, 2172 |
| Allocated Node Count | 4 |
| Mastery Nodes In Fixture | **0** |
| Unsupported Nodes In Fixture | **0** |
| Offensive Passive Evidence | 559 `AttackSpeed:Increased:0.04`；1795 `PhysicalDamage:Increased:0.1`（`pk|` 里 26=…/**0.04**、11=…/**0.3035488** 实测可见） |
| Defensive/Attribute Passive Evidence | 2034 `Life:Flat:12`；559 `Dexterity:Flat:5`；1795/2034 `Strength:Flat:5`×2（`pe|` 里 Life=10000031、Strength=39.5819969 实测） |
| Passive Allocation API Used | `SliceSession.TryAllocate`（事件 trace `pa|…|ok|…`；无任何容器直写） |
| Hash Payload Added | `pv|passive-v1` / `ps|` / `pm|` / `pe|` / `pk|` |
| Allocated Identity Included | YES（`ps|`） |
| Modifier Semantics Included | YES（`pm|`，含 StatId/ModOp/Value/Tags/Condition） |
| Effective Stats Included | YES（`pe|` + `pk|`，全 StatId 枚举 + raw 分量） |
| UI/Geometry Excluded | YES（行为断言 + 源码 token 扫描，`forbiddenTokensFound = []`） |
| Raw Tree Bytes Excluded | YES（负载 < 8192 字符；只含 4 个已分配节点） |
| Sensitivity Allocation Baseline Hash | `FNV1A64:ec1d3ed67d3035d0` |
| Sensitivity Allocation Mutated Hash | `FNV1A64:32b23c480e05ec9a` |
| Allocation Mutation Changed Hash | **YES** |
| Sensitivity Stat Baseline | `FNV1A64:ec1d3ed67d3035d0` |
| Sensitivity Stat Mutated | `FNV1A64:4fc17552175c144a` |
| Stat Mutation Changed Hash | **YES** |
| Restored Hash | `FNV1A64:ec1d3ed67d3035d0` |
| Restore Exact Match | **YES** |
| Ordering Invariance | **YES**（`allocationOrderingInvariant = true`） |
| Same-State Payload Exact Match | **YES** |
| Production Simulation Run1 | 2026-09-11 04:35:42 → `FNV1A64:ec1d3ed67d3035d0` |
| Production Simulation Run2 | 2026-09-11 04:35:49 → `FNV1A64:ec1d3ed67d3035d0` |
| Production Simulation Run3 | 2026-09-11 04:35:55 → `FNV1A64:ec1d3ed67d3035d0` |
| All Three Exact Match | **YES** |
| New Passive-Aware Canonical Hash | **`FNV1A64:ec1d3ed67d3035d0`** |
| Reason For Hash Change | evidence surface 扩大：被动身份 + 实际生效 modifier + 有效属性/技能属性首次进入 canonical 哈希（**非玩法改动**） |
| Passive Sensitive | **YES** |
| Mastery Sensitive | NO / **DEFERRED to WO-03** |
| EditMode | **409 / 409 PASS** |
| PlayMode | **14 / 14 PASS** |
| Content Audit | PASS |
| Content Audit Fresh | YES |
| Gameplay / Passive Runtime / Canonical Data / Content Delta | **NONE / NONE / NONE / NONE** |
| Drift | 0 |
| Source-of-Truth Conflicts | 0 |
| Forbidden Expansion Audit | **PASS** |
| Environment Incidents | 无 |
| Open Questions | 无（专精缺口已知且已排 WO-03） |
| Recommended Next WO | `S6P-WO-03 — Mastery Explicit Selection Correctness`（合同 §26） |

---

## §11 Gate Review 结论（规划 AI，2026-09-11）

原文：`docs/reviews/S6P/S6P_WO_02_GATE_REVIEW_AND_WO_03_CONTRACT.md`。

| 项 | 裁决 |
|---|---|
| Verdict | **ACCEPT** |
| Acceptance | **31 / 31 AC 全 PASS** |
| Follow-up | **NONE** |
| 新 authoritative baseline | **`FNV1A64:ec1d3ed67d3035d0`** |
| `9a4c…` | 降级为 PREDECESSOR REFERENCE（仅历史参照） |
| Passive-sensitive ProdSim | **SUPPORTED** |
| Mastery-sensitive ProdSim | **NOT YET / DEFERRED to WO-03** |
| 下一令 | **`S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness`（已释放，含完整合同）** |

规划 AI 额外裁定并锁定的三条设计口径：

1. **`pe|` 带 raw 分量（base/flat/increased/more）**：正式接受并锁定。只 hash 最终 `Get()` 有确定性盲区
   ——「modifier 不存在」与「modifier 存在但当前基数为 0」在 final=0 上不可区分。ProdSim 只读取权威结果并规范序列化，**不得复制 modifier 计算**。
2. **`pk|` 技能侧快照**：正式接受并锁定。`pe|` = player/global 有效属性真相，`pk|` = canonical 技能侧有效属性真相，两者都需要，
   否则「被动已分配、`pm|` 正确、PlayerStats 未变、技能消费者悄悄坏了」这条路径会被漏过。
3. **allocation event（`pa|…`）不进哈希**：接受。事件保留在报告作 provenance，canonical hash 以最终 gameplay state 为核心，
   因此 `559→1795→2034` 与 `2034→1795→559` 只要最终状态相同即同 hash（已被 mutation test 实证，非设计声明）。

**关于 git 未提交状态**：本单 31 条 AC 未把「必须 git commit」列为 acceptance criterion，故不构成 failure、不产生 follow-up。
但从 **WO-03 起** Evidence 必须给出 `Entry revision / working-tree fingerprint` + `Changed-file inventory` +
`Gate 执行对应的最终文件状态`，保证 Gate 与 Evidence 唯一对应同一 working-tree state（不允许「测 A 状态 → 改文件 → 用 A 结果申报 B 通过」）。

**WO-03 入口基线**：`EditMode 409/409`、`PlayMode 14/14`、`ProdSim FNV1A64:ec1d3ed67d3035d0`、
`Passive Sensitive=YES`、`Mastery Sensitive=NO`、`Mastery Nodes=315`、`Implicit First Choice=PRESENT`、`Mastery Prerequisite=ABSENT`。
