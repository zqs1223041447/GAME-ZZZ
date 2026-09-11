# S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination

**状态：IMPLEMENTED / GATE REVIEW PENDING**（2026-09-11）
**合同出处：** `docs/reviews/S6P/S6P_ORDER_ARBITRATION_AND_WO_04A_CONTRACT.md`（§8–§31，42 条 AC + §24 的 19 个测试族）
**执行修订：** 规划 AI 于 2026-09-11 心跳以「04A Execution Amendment」确认「立即开工」，追加 7 条约束
（runtime 拥有 consumer 清单 / mixed node 完整零增量取证 / 消费门必须绕开分配门取证 /
专精连 baked Mods 也要拆 / supported 正对照 / 出口 hash 必须不变 / Evidence hygiene）。
Amendment 原文见 `docs/reviews/S6P/S6P_WO_04A_PLANNER_AMENDMENT.md`。

---

## 1. Objective

建立整个 Passive domain **唯一**的「这条被动承诺当前引擎能不能完整兑现」权威判定，
并消灭「花点 → 点亮 → 只有一部分生效 → 其余静默无事发生」这条路径。

核心 invariant（§14）：

> 节点只要存在一条无法完整兑现的 gameplay effect，**整节点当前不可分配**。

---

## 2. 落地结构（唯一 owner）

| 位置 | 角色 |
|---|---|
| `Assets/Runtime/Core/Gameplay/PassiveSupport.cs`（新增） | **唯一** support truth：行四分类 + 节点资格 + 真实 consumer 清单 + 稳定原因文本 |
| `SliceSession.TryAllocate` | 支持门：不可兑现场景原子拒绝（点/状态/结果零变化） |
| `SliceSession.RecalcPlayer` / `CollectSkillMods` | 消费门：已分配集合里的不可分配节点贡献 **0** modifier |
| `SliceSession.CanAllocate` / `NodeState` / `NodeBlockReason` | UI 真相接口（UI 不再自己判定） |
| `SliceHud` 节点 tooltip | 只展示 domain truth 给出的原因 |
| `PassiveCensus`（测试侧） | 反向**消费** runtime truth（分类规则已从测试程序集删除） |
| `ContentAuditS2Tests.RuntimeConsumedStats` | 改为 runtime 数组的**别名**（测试不再拥有生产规则） |

分类规则的唯一性由门禁咬死：`PassiveSupportTests.NoSecondUnsupportedNodeOracle` 扫描
`Assets/Runtime`，除 `PassiveSupport.cs` 外任何文件出现 `DomainTable` 或原因文本即失败；
扫描 `Assets/Tests` 出现 `"energy shield"` 关键词表即失败；`SliceHud.cs` 出现 `PoeStatParser` 即失败。

---

## 3. 实际行为变化（三处，全部是有界更正）

1. **blocked 节点整体原子拒绝**（1660 个上树节点）：`TryAllocate` 稳定拒绝，
   点数/已分配集合/玩家结果/技能结果零增量。
2. **专精过渡态**（315 个）：WO-03 建立显式选择前不可分配（消耗 0），
   且 `PassiveCatalog.Build` 不再把 `choices` 首条烘焙成默认 Mods（旧 `FirstChoice` 路径已删除）。
3. **无 handler 的特殊交互显式封锁**（87 个 = 57 珠宝孔 + 30 时光珠宝类无连线节点）：
   合同 §12 明确「Jewel Socket / Timeless-special 不能因为画出来了就算 supported」。
   珠宝孔在旧口径下是「无词条 ⇒ 真空 supported」，因此这一条是本轮**新增的第三处**行为更正，
   不在事前预估的两条之内，已在 Evidence Pack 的 Delta 一节单列。

未被触碰：parser 规则表、StatId/ModOp、canonical 数据、内容、哈希协议、专精选择语义（留给 WO-03）。

---

## 4. 数据事实（04A 口径，census V2）

| 指标 | 值 | 备注 |
|---|---|---|
| 上树节点 | 2429 | 未上树 403 仍只做源数据记账（§18） |
| ALLOCATABLE_SUPPORTED | **367** | 424 → 367 的差 = 57 珠宝孔（真空 supported 已封锁） |
| BLOCKED_CURRENTLY | **1660** | 含 211 个普通域内真实混合节点 |
| BLOCKED_SPECIAL_INTERACTION | **87** | 57 珠宝孔 + 30 时光珠宝类（原本按词条算进 blocked） |
| SPECIAL_PENDING_MASTERY | **315** | WO-03 前一律不可分配 |
| 效果行 | consumed 607 / blocked 4475 / special 12 / structural 582 / unknown **0** | 与 WO-01 完全一致：严格化 CONSUMED **一条都没翻** |
| 真实混合节点 | 236（含特殊类别） / 211（普通域内） | 合同 §14 要求的是真实样例，不是合成样例 |

`CONSUMED` 严格化（parser 命中 **且** 产出 StatId 全部有真实 runtime consumer）在当前数据上
**不产生任何行位移**：`StatId` 全枚举 28 项 100% 都在 consumer 清单内，
因此 607 行、424 个原 supported 主树节点、WO-02 canonical fixture（559/1795/2034 + 起点 2172）
全部保持可分配 —— 出口 hash 因此理应不变（实测不变）。

---

## 5. 门禁结果

| 门 | 结果 |
|---|---|
| EditMode | **426 / 426 PASS**，fail=0，skip=0（入口 409/409，+17 = 新增 `PassiveSupportTests`） |
| PlayMode | **17 / 17 PASS**，fail=0，skip=0（入口 14/14，+3 = 新增 `S6PPassiveSupportPlayModeTests`） |
| ProdSim（编辑器内） | `FNV1A64:ec1d3ed67d3035d0`，invalid=0，verdict PASS |
| ProdSim（出口冷进程 ×3） | Run1=Run2=Run3=`FNV1A64:ec1d3ed67d3035d0`，EXACT MATCH = YES |
| Content Audit | PASS（本轮重新生成，`docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`） |
| Census | V1 → **V2**（新增 `masteryPending`），`docs/qa/PASSIVE_CENSUS_REPORT.json` |
| 前置测试缺失/削弱 | 0 个**未解释**的删除或削弱；4 处解释性适配见 §7 |

---

## 6. 关键证据（谁在证明「不是空的」）

- **混合节点整体 fail closed**：冻结真实样例 `71`（可消费 `+5 to Intelligence` + blocked `Mana Regeneration`，
  且是起点邻居 ⇒ 相连性成立，被拒的唯一原因只能是支持门）、`12`（可消费 `10% increased PhysicalDamage`
  + blocked Minions）、`255`（可消费 AttackSpeed + blocked Block/Melee + structural 行）。
- **消费门不是摆设**：绕过分配门直接把 `Allocated[12] / Allocated[71]` 写成 true（生产 API 不提供强制分配），
  `RecalcPlayer` 与 `CollectSkillMods` 必须给出 0 增量，且 `BlockedAllocatedCount` 暴露 2；
  同一手法注入 supported 节点 `559` 必须真的生效（对照组，排除「测量假象」）。
- **专精零效果**：全树 315 个专精 `PassiveCatalog.Get(i).Mods.Length == 0`；
  损坏状态注入后玩家属性与技能包与基线逐 StatId 相等。
- **UI 同一真相**：2429 个节点逐个断言 `CanAllocate`/`NodeState(Available)` ⇒ support truth 可分配，
  且不可分配节点必有稳定原因、绝不显示为可点。

---

## 7. 前置测试的解释性适配（§26 要求披露）

| 测试 | 旧前提 | 适配 | 是否削弱 |
|---|---|---|---|
| `PoeTreeTests.StartNode_IsAllocatedOnReset_…` | 起点 `links[0]`(=71) 可点 | 改为「第一个可兑现邻居可点」，**并新增**「不可兑现邻居必须被拒 + 零扣点」断言 | 否，强度提升 |
| `PoeTreeTests.Respec_KeepsOnlyStartNode` | 同上 | 同上（改用可兑现邻居） | 否 |
| `SliceLoopTests.InMap_BuildChangeFails_DeathRespecUnlocks` | 出图后点 `links[0]` | 改用第一个可兑现邻居 | 否 |
| `S3R2FireConversionTests.SupportAndPassiveConversion_ComposeOnSameAxis` | 沿路径点亮转火基石（node 1650）后取技能包 | 基石含 blocked 行 ⇒ 04A 起**必须**不可分配；改为「断言其被拒 + `NodeBlockReason` 为 blocked」+「用权威 parser 对该真实文本的产出与 Support modifier 一起走生产 StatBag 聚合」 | **部分**：不再有「真实被动 → 分配 → 技能上下文」的端到端路径（该路径被本轮产品真相合法关闭）；同轴聚合算术仍被证明。转换机制的端到端路径另有 `ActualHit_CompositionConvertsPhysToFire`（Support 侧）继续覆盖 |
| `S5UPoeTreeVisualPlayModeTests.PoeTree_FullAndZoomed_…` | 点亮整个第一环 | 只点亮可兑现邻居（559/1795/2034），并断言 ≥3 | 否（截图态三态仍在） |
| `PassiveCensusTests.NodeEligibility_PartitionsAllNodes` | 三分类 | 四分类（含 masteryPending）+ 冻结真相数字 | 否，强度提升 |

---

## 8. 未做（禁止扩张边界，§29 保持 PASS）

不改 parser、不加 StatId/ModOp、不建专精选择器、不碰珠宝/药剂/升华/Timeless/新玩法域、
不引入第三方框架、不做存档、不动 canonical 数据与哈希协议。

---

## 9. 遗留缺口（诚实登记）

1. **入口 working-tree fingerprint 未捕获**：本单在读取合同后即开始实施，未在第一次写文件前跑
   `tools/evidence/working-tree-fingerprint.ps1`。入口侧只能给出 HEAD `64b614f` 与入口门基线数字，
   无法给出精确的入口内容指纹。出口指纹已按合同提供。**下次执行规则：任何 gameplay 改动前先落指纹。**
2. `PoeStatParser` 当前能产出的 StatId 已 100% 被消费，因此「严格化 CONSUMED」在今天**不可观测**
   （不产生行位移）。这是事实，不是省略；一旦未来有 parser 新增产出而无消费者，
   `ConsumedMeansParserPlusRealRuntimeConsumer` 会立刻变红。
3. 专精的隐式首条效果在当前数据上**本来就解析不出来**（315 个专精的 `choices` 首行全部落空），
   所以拆掉预热路径在今天也是行为中性的；这是消除隐患，不是修复可见 bug。

---

## 10. 建议规划 AI 下一轮优先核对的仓库事实

1. `Assets/Runtime/Core/Gameplay/PassiveSupport.cs`（唯一 owner 是否真的唯一）
2. `Assets/Tests/EditMode/PassiveSupportTests.cs` 的 17 个族与冻结样例数字
3. `docs/qa/PASSIVE_CENSUS_REPORT.json`（V2 分布）与 `docs/reviews/S6P/_wo04a_cold_exit.txt`
4. 本单 §7 的 5 处测试适配是否可接受（尤其 S3R2 的端到端路径关闭）
5. 若 ACCEPT：按合同 §31 重新放行 WO-03（并带三条 amendment）
