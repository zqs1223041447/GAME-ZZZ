# S6P-WO-03 — Mandatory Preflight（强制前置记录）

**状态：PREFLIGHT PASS — 允许进入产品实现**（2026-09-11）
**合同出处：** `docs/reviews/S6P/S6P_WO_04A_GATE_REVIEW_AND_WO_03_CONTRACT.md` §4（Mandatory Preflight）
**性质：** 本文件覆盖**只读**取证 + 分支判定，**未做任何 production mutation**（本轮未写任何产品代码）。

---

## 1. Entry 记录（§4 要求的第一组字段）

| 项 | 值 |
|---|---|
| Entry Working-Tree Fingerprint | `a85a5e2e23ca5acfae2a96b41a61d12cfdd59fd9fd9ec8a77ab1b43f4a12c926` |
| 明细 | 13765 files / 2,062,028,701 bytes / branch `main` / dirty YES —— `docs/reviews/S6P/_wo03_fingerprint_entry.txt` |
| HEAD | `64b614f2a0effc0e073f5997fa595702effd5bc1`（04A 全程未移动，未提交） |
| Changed-file inventory | 见指纹文件内的 `git status --porcelain` 分区清单（Assets/Runtime 18、Assets/Tests、docs、开发计划、_poe_src 等） |
| Entry Discovered EditMode | **426**（04A 收口态；入口 04A 前为 409） |
| Entry EditMode pass/fail/skip | **426 / 0 / 0** |
| Entry Discovered PlayMode | **17**（04A 前 14） |
| Entry PlayMode pass/fail/skip | **17 / 0 / 0** |
| Entry ProdSim | `FNV1A64:ec1d3ed67d3035d0`（04A 出口冷进程 ×3 EXACT；本前置为只读，hash 不变） |
| Content Audit | PASS / fresh（04A 同轮重生成） |

> 规则升级已生效（规划 AI 03 合同 §3 裁定）：**entry fingerprint → production mutation → tests → final fingerprint**，顺序不可反。
> 本前置已完成第 1 步；后续产品改动的第一行之前不需要再取（同一 entry 态）。

## 2. Mastery Choice Census（§4 第二组字段，用 04A 唯一 support truth 全量统计）

| 指标 | 值 |
|---|---|
| Total Masteries | **315** |
| Total Choices | **1863** |
| Choices Fully Supported | **22** |
| Choices Blocked | **1841** |
| — of which BLOCKED_BY_DOMAIN | 1833 |
| — of which SPECIAL_INTERACTION（无 handler） | 8 |
| Masteries **>= 1** Supported | **22** |
| Masteries **>= 2** Supported | **0** |
| Masteries **== 0** Supported | **293** |

### 分支判定（§4 Conditional branch）

`Masteries with >=1 fully supported choice = 22 ≠ 0` ⇒ **不触发 STOP**，
**允许继续完整 WO-03 产品实现**（不需要回传 `WO-03 PRECONDITION BLOCKED_BY_EXISTING_DOMAIN`，
也不需要 Channel A 裁定是否提前插 04C）。

未做、也不得做的四件事（本轮**一件未做**）：扩 parser、新增 StatId、
把 blocked choice 当 selectable、为满足 WO-03 制造假 supported choice。

## 3. 冻结 fixture 候选（§13）

§13 优先要求「>= 2 个 fully-supported choices 的专精」，实测 **0 个**，
因此按 §13 的 fallback 走「eligible/unselected vs explicitly selected 必须改变 gameplay/hash」。

| 项 | 值 |
|---|---|
| 建议冻结 Mastery | **node 10 = Life Mastery** |
| 官方 cluster/group | `group = 741`（组内 4 个节点，其中 1 个 Notable） |
| 连线 | `links = 1`（单一入口，与官方一致） |
| Supported choice（唯 1 条） | `+30 to maximum Life` ⇒ 语义元组 `Life:Flat=30`（CONSUMED） |
| 其余 choice 状态 | 5 条全部 BLOCKED_BY_DOMAIN（含条件句/Equipped Body Armour/Low Life/Full Life/Skills Cost Life） |

22 个「有可兑现 choice」的专精**每个都恰好只有 1 条**可兑现选项
（node 10 / 96 / 126 / 422 / 449 / 463 …；例：node 463 `group=386`，组内 2 个 Notable、9 个节点、2 条连线）。
⇒ 「同一次分配 + Choice A vs Choice B 产生不同 hash」在本数据上**不可实现**（不存在第二个可兑现选项），
WO-03 的 hash 敏感性必须用 §13 fallback 口径取证：**未选择 vs 显式选择**必须改变 gameplay/hash。

## 4. Prerequisite 规则可行性（只读核对，供实现期直接用）

按合同 §5 的口径（`Mastery.group` 官方簇 + 同簇 >= 1 个已分配的 non-Mastery Notable）：

| 指标 | 值 |
|---|---|
| 同簇存在 Notable 的专精 | **315 / 315** |
| 同簇不存在 Notable 的专精 | **0** |
| 同簇存在 >= 1 Notable 且自身有可兑现 choice | **22** |

⇒ prerequisite 规则在全树 315 个专精上都**可被满足**（不存在「规则写死但数据永远为假」的死规则），
但**只有 22 个专精能在满足 prerequisite 后真正进入 selection**（其余 293 个即使前置满足也没有可兑现选项 ⇒ 仍不可分配）。
这条差别必须在 WO-03 的 UI 与 `CanAllocate` 里如实表达（数据级「无可用选项」 vs 状态级「前置未满足」是两种不同原因）。

## 5. WO-03 实现前必须记录的设计判据（本轮发现的耦合点，留给实现期，不在本前置内改造）

1. **04A 冻结数字会随 WO-03 移动**：04A 口径是 `ALLOCATABLE_SUPPORTED=367 / SPECIAL_PENDING_MASTERY=315`。
   WO-03 引入「数据级专精可选择性」后，22 个专精应从「一律 pending」变为「可选（受 prerequisite 约束）」，
   因此 `PassiveCensusTests` / `PassiveSupportTests` 里 **04A 冻结的 367 / 315 必须由 WO-03 显式更新并说明原因**，
   不得静默改动（且 04A 的证据包保留历史口径，不回填）。
2. **节点资格与状态资格分离**：`PassiveSupport.EvaluateNode` 是**数据级**判定（无状态），
   prerequisite 依赖已分配集合 ⇒ 必须在 `CanAllocate`/`TryAllocateMastery` 层做**状态级**判定，
   两者都要有稳定原因文本，UI 只展示 domain truth 给出的原因（延续 04A §20 口径）。
3. **选择效果消费链**：专精的生效 modifier 必须由「显式选中的那条 choice」经 `PoeStatParser` 产出，
   并走**既有** `RecalcPlayer` / `CollectSkillMods` 消费路径（禁止第二套 Mastery modifier evaluator）；
   `PassiveCatalog.Get(mastery).Mods` 保持 0（04A 已拆掉隐式路径，不得以任何形式恢复 `FirstChoice`/`choices[0]` fallback）。
4. **ProdSim V3 双因归因**：hash 变化必须分别归因于
   A（Mastery gameplay truth 进入 canonical 证据）与 B（`pv|passive-v2` serializer canonical-key 清理），
   且新 baseline 必须由 3 个独立 Unity 冷启动进程 `H1 == H2 == H3` 才能晋升。

## 6. 本轮结论

- **Preflight = PASS**，产品实现被授权继续，**且实现尚未开始**（本前置只读）。
- 04A 的 42/42 AC 与出口证据不受本前置影响（树未变动：Read-only）。
- 实现入口条件已就绪：entry fingerprint 已落库、census 已落库、fixture 候选已定、耦合点已登记。
