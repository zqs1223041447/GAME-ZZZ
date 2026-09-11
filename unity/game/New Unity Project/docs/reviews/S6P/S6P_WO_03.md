# S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness

**状态：** 产品实现完毕，待 Channel A Gate Review  
**时间：** 2026-09-11  
**合同：** `S6P_WO_04A_GATE_REVIEW_AND_WO_03_CONTRACT.md` §4–§19 + Execution Amendment `S6P_WO_03_PLANNER_AMENDMENT.md`  
**入口 HEAD：** `c16d129`（生产树基线 `a91e391`）

## 做了什么

在 04A `PassiveSupport` 唯一 truth 上建立专精的 **prerequisite / 显式选择 / 原子提交 / 确定性证据**。不重新定义「支不支持」。

1. **Prerequisite owner**（`PassiveSupport`）：官方 `PoeNode.group` 簇 + 同簇 >=1 已分配 non-Mastery Notable。禁止 GUI 几何 / 名称 / 序。
2. **Choice support**：只调用 `ClassifyLine`。整行 CONSUMED 才可提交。
3. **Selection identity（EA-1）**：官方 `choices` 无 per-choice ID ⇒ fallback `MasteryNodeId + source ordinal`。scheme 字符串钉死。
4. **原子提交** `SliceSession.TryAllocateMastery`：成功扣恰好 1 点、写入 identity、所选 choice 经既有 parser 生效恰好一次。失败零变化。普通 `TryAllocate` 仍拒绝专精。
5. **EA-2**：静态 `TraversalTruth` 保持 `SPECIAL_BLOCKED`。已选择专精 **不是 transit**（`AdjacentToAllocated` 跳过 Mastery 邻居）。
6. **EA-3**：`BlockedAllocatedCount` 只把没有合法 explicit-selection authority 的 special-blocked 已分配节点算 corruption。合法选中专精不算。Jewel/Timeless 无例外。
7. **消费**：`EffectivePassiveMods` 是 RecalcPlayer / CollectSkillMods / ProdSim 的同一入口。未选择 / blocked / 注入 = 空。`PassiveCatalog` 专精 Mods 仍为 0。
8. **UI**：点击专精打开选择器（零 gameplay 增量）；Esc 先关选择器；全部官方 choice 可见；blocked 不可提交。布局纯函数可测 1920/2560。
9. **ProdSim V3**：`simulationContractVersion=V3`，负载 `pv|passive-v2`，`px|` 选择 identity，pm/pe/pk 去掉 `Enum.ToString()`。
10. **S3R2 改名**：`SupportAndParsedPassiveConversion_ComposeOnSameAxis`（非阻塞 follow-up）。

## 没做什么

Jewel / Timeless / 升华 / 药剂 / 新 Stat 轴 / refund / 存档 / 第三连接组 / parser 扩张。未把 Mastery 改成 TRAVERSABLE。未为 census 漂亮而改 NodeTruth。

## 数字

| 项 | 值 |
|---|---|
| 04A 诊断 census | **367 / 1660 / 87 / 315**（before=after，delta=0，reason=EA-4） |
| 可达 / \|D\| / start-disconnected supported | **1985 / 325 / 42**（topology invariant，未动） |
| Choice census | 315 专精 / 1863 choice / 可兑现 **22** / >=2 = **0** |
| 冻结 fixture | node **10** Life Mastery，group 741，choice `+30 to maximum Life` |
| EditMode | **489/489**（475 前置 + 14 新） |
| PlayMode | **19/19**（18 前置 + 1 新） |
| Content Audit | PASS / fresh |
| ProdSim 入口 | `FNV1A64:ec1d3ed67d3035d0`（`pv\|passive-v1`） |
| ProdSim 出口冷进程 ×3 | `FNV1A64:99f1bfd3f81c4fe6` EXACT MATCH（V3 / `pv\|passive-v2`） |

## Hash 归因

- **B（serializer cleanup）**：canonical fixture 仍 0 专精。hash 变化来自 `pv|passive-v2` + 空 `px|` 行 + 去掉 enum 名。这是默认场景 hash 从 `ec1d…` 变成 `99f1…` 的原因。
- **A（Mastery gameplay）**：同一 prerequisite 下 unselected vs explicit `+30 Life` 改变 `px|`、`pm|`、`pe|` Life 与 FNV。selector 打开/取消不进 session，故不进 hash。
