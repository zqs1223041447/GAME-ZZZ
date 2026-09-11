# Evidence Pack — S6P-WO-02 执行完毕，请 Gate Review

按你的规矩「一轮一令、上一单 ACCEPT 才释放下一单」，本单到此停下等你裁决。
以下全部数字均来自真跑，不是推定。这是**跟进你上一条 WO-02 完整合同（31 条 AC）**的交付评审。

## 令号

`S6P-WO-02 — Passive-Aware Production Simulation`
性质：**只扩大 evidence surface**。Gameplay Delta = NONE / Passive Runtime Delta = NONE /
Canonical Data Delta = NONE / Content Delta = NONE。授权的改动面只有
`Production Simulation Tool` 与 `Canonical Evidence Surface`。
工作令记录：`docs/reviews/S6P/S6P_WO_02.md`。

## 1. canonical 被动场景（冻结，合同 §3）

```
ScenarioVersion = s6p-wo02-canonical-v1
StartNode       = 2172（"Seven"；= SliceSession.StartNode）
Fixture NodeIds = 559, 1795, 2034   ← 冻结常量，禁止 find-first-supported 动态挑选
Allocated Set   = 559, 1795, 2034, 2172（起点恒已点亮，一并计入身份）
```

| NodeId | 名字 | 资格（WO-01 census） | 效果行 | 产出 |
|---|---|---|---|---|
| 559 | Attack Speed and Dexterity | `SUPPORTED` | 2 CONSUMED | `AttackSpeed:Increased:0.04`（进攻）、`Dexterity:Flat:5`（属性） |
| 1795 | Physical Damage and Strength | `SUPPORTED` | 2 CONSUMED | `PhysicalDamage:Increased:0.1`（进攻）、`Strength:Flat:5`（属性） |
| 2034 | Life and Strength | `SUPPORTED` | 2 CONSUMED | `Life:Flat:12`（防御）、`Strength:Flat:5`（属性） |

- **Mastery Nodes In Fixture = 0**；**Unsupported Nodes In Fixture = 0**；**Special Nodes = 0**。
- 三节点均为起点 2172 的直接邻居 → 任意加点顺序都满足 connected allocation。
- 覆盖：进攻向 CONSUMED = 2 条；防御/属性向 CONSUMED = 4 条（单 sub-scenario 即达标）。
- 加点只走 `SliceSession.TryAllocate`（domain API），无任何容器直写。事件 trace：
  `pa|559|ok|122` / `pa|1795|ok|121` / `pa|2034|ok|120`（进报告作证据，不进哈希）。

## 2. 哈希负载（`pv|passive-v1`，合同 §5/§6/§16）

新增段：`pv|passive-v1`（schema marker）/ `ps|`（已分配 NodeId，数值升序）/
`pm|`（实际生效 modifier 语义元组 nodeId:statId:op:value:tags:condition，稳定排序）/
`pe|`（有效 gameplay 属性，StatId 全枚举，含 raw flat/increased/more 分量）/
`pk|`（有效技能属性，3 canonical 技能 × StatId 全枚举，含 raw 分量）。

- `pe|`/`pk|` 带 raw 分量：`+10% increased PhysicalDamage` 落在 0 基底上时 `Get()` 仍是 0，
  只存最终值看不见进攻轴；`pk|` 里 `11=0/0/0.3035488/1`、`26=0/0/0.04/1` 才是真证据。
- **明确不进哈希**：坐标 / zoom / pan / icon / texture / UV / 簇视觉 / tooltip / 本地化 /
  GUI 状态 / hover / 选中节点 / 窗口尺寸 / 分辨率 / 资产与文件时间戳 / 字典迭代序 /
  `tree_raw.json` 字节 / 2429 节点定义本身。负载长度上界 < 8192 字符（断言守住）。
- 报告 schema 升 **V2**，新增 `simulationContractVersion` / `passiveSimulation` / `passiveSensitivity`。

## 3. 敏感性证明（合同 §7 必做，非「跑三次同 hash」）

同一 seed、同一物品生成序列，只改被动面：

| 证明 | 变体 | 结果 |
|---|---|---|
| A 分配身份 | canonical `{559,1795,2034}` vs `{559,1795}` | `ec1d3ed67d3035d0` → **`32b23c480e05ec9a`** ≠ |
| B 运行期效果 | canonical vs `{559,2034}`（合法，有效属性不同） | `ec1d3ed67d3035d0` → **`4fc17552175c144a`** ≠ |
| C 恢复 | 回到 canonical | **`ec1d3ed67d3035d0`** 精确复原 = |
| D 顺序不变性 | 同一集合、相反加点顺序 `{2034,1795,559}` | **`ec1d3ed67d3035d0`** = |

- A/B 同时断言**词缀分布逐项相同** → 差异只来自被动面，不是物品流分叉。
- B 不靠改 catalog 数值：两个变体都被 domain API 接受（`|ok|`），Life 与 PhysicalDamage/AttackSpeed 增量逐项断言。
- 反证「没绕过 domain」：取一个与已分配集合不相连的 `SUPPORTED` 节点，`TryAllocate` 必须拒绝、不写容器、不扣点。

## 4. 门（本次收尾在最终代码状态重跑）

| 门 | 结果 |
|---|---|
| EditMode（全量） | **409 / 409 PASS / 0 FAIL**（394 前置全保留 + 本令新增 15） |
| PlayMode | **14 / 14 PASS / 0 FAIL / 0 SKIP** |
| ProdSim invalid / repeat | **invalidCount=0**、**repeatHashMatch=true** |
| Content Audit | **PASS / fresh**（affixes 21 / passives 2429；本次 EditMode 运行再生） |
| Drift | 0 |
| Source-of-Truth Conflicts | 0（census 单真值重构后只余一处判定实现） |
| Forbidden Expansion | PASS（无新玩法域、无采购、无换框架、未碰 parser/StatId） |
| Headless Chrome | **NOT REQUIRED**（本单不涉绘制，合同 §20） |

## 5. 三次独立 ProdSim 全量运行（合同 §14，收尾重跑）

| Run | 时间戳 | Hash |
|---|---|---|
| Run1 | 2026-09-11 04:35:42 | `FNV1A64:ec1d3ed67d3035d0` |
| Run2 | 2026-09-11 04:35:49 | `FNV1A64:ec1d3ed67d3035d0` |
| Run3 | 2026-09-11 04:35:55 | `FNV1A64:ec1d3ed67d3035d0` |

**All Three Exact Match = YES**（三次独立进程内重放，非复制报告、非读缓存）。

## 6. 哈希策略总表（合同 §13）

```
Before              : FNV1A64:9a4c9524d0b3e214（PREDECESSOR REFERENCE，仅参照）
After × 3           : FNV1A64:ec1d3ed67d3035d0（exact）
Exact Repeat        : YES
Reason For Change   : evidence surface 扩大 —— 被动身份 + 实际生效 modifier + 有效属性/技能属性
                      首次进入 canonical 哈希（非玩法改动）
Changed Canonical Fields : 新增 pv| / ps| / pm| / pe| / pk| 负载段
Unexpected Hash Inputs   : NONE
```

## 7. Delta 与排除（合同 §10/§11/§12/§23）

| 项 | 值 |
|---|---|
| Gameplay / Passive Runtime / Canonical Data / Content Delta | **NONE / NONE / NONE / NONE** |
| Changed Runtime Gameplay Files | **NONE**（复核：WO-02 时间窗内 `Assets/Runtime` 下 .cs 零改动） |
| Mastery Sensitive | NO / **DEFERRED to WO-03**（fixture 专精数 = 0） |
| Unsupported Nodes In Fixture | **0**（"非空转"证明：起点旁 unsupported 节点 71 被冻结为反证常量，断言其确被判为 UnsupportedCurrently） |
| PoeStatParser / 新 StatId / 新被动轴 | 未改一行 |

## 8. 环境披露（如实）

- 本单**未发生编辑器挂死**，未使用 Recovering Scene Backups 恢复配方，无 pipeline 超时。
- 例行：跑门禁前 `editor_stop`；新增 .cs 后 `recompile` 轮询至 completed；PlayMode 走异步 + 核对状态文件时间戳为本次运行。
- 工作区状态：本单文件与 WO-01 产物同样处于**未提交**状态（沿用本周期仓库惯例，未做任何 git 写操作）。

## 9. 我要你回答的

1. **Gate Review 裁决**：ACCEPT / ACCEPT WITH FOLLOW-UP / REJECT-REWORK？
2. 逐条核对上方 31 条 AC（AC-01 ~ AC-31）是否全部满足；如有不满足项请指出。
3. 若 ACCEPT：**释放 `S6P-WO-03 — Mastery Explicit Selection Correctness`**？
   该单将首次获得 Passive gameplay behavior authority（implicit first-choice 消除、显式 selected effect、
   prerequisite、reset、aggregation、Passive-aware ProdSim 敏感性、新 hash 3× 重基线）。

请输出中文。
