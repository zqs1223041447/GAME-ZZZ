# S6P-WO-04A — Evidence Pack

**Work Order:** S6P-WO-04A — Passive Support Truth Gate & Silent-Zero Elimination
**Revision / Working-Tree Fingerprint（gate 态）：** `c5dfe500aaefcbd70a6f35ed770a17a7c60bbda6c538c5135ed892922b5d8071`
（13758 files / 2,061,958,708 bytes / branch `main` / HEAD `64b614f2a0effc0e073f5997fa595702effd5bc1` / dirty YES）
详见 `docs/reviews/S6P/_wo04a_fingerprint_gate.txt`。终态指纹见 §Final。
**Git State:** 未提交（沿用 S6P-WO-01/02 口径：工作区 + 内容指纹作为 change identifier）；HEAD 未移动。

---

## 1. Changed files

**Changed Runtime Files**
- `Assets/Runtime/Core/Gameplay/PassiveSupport.cs`（新增，唯一 support truth）
- `Assets/Runtime/Core/Gameplay/SliceSession.cs`（支持门 + 消费门 + UI 真相接口 + invalid-state 计数）
- `Assets/Runtime/Core/Gameplay/Catalogs.cs`（专精不再烘焙隐式 `choice[0]`；删除死方法 `FirstChoice`）

**Changed UI Files**
- `Assets/Runtime/Core/Gameplay/SliceHud.cs`（节点 tooltip 只读 domain truth；移除「默认生效首条」表述）

**Changed Tests**
- `Assets/Tests/EditMode/PassiveSupportTests.cs`（新增，17 个测试族）
- `Assets/Tests/PlayMode/S6PPassiveSupportPlayModeTests.cs`（新增，3 个集成对照）
- `Assets/Tests/EditMode/PassiveCensus.cs`（分类规则改为消费 runtime；schema V1→V2 + `masteryPending`）
- `Assets/Tests/EditMode/PassiveCensusTests.cs`（四分类划分 + 冻结真相数字）
- `Assets/Tests/EditMode/ContentAuditS2Tests.cs`（consumer 清单改为 runtime 单一 owner 的别名）
- `Assets/Tests/EditMode/PoeTreeTests.cs`、`SliceLoopTests.cs`、`S3R2FireConversionTests.cs`、
  `Assets/Tests/PlayMode/S5UPoeTreeVisualPlayModeTests.cs`（5 处解释性适配，见 WO 记录 §7）

**Changed Tools** — 无（沿用 `tools/evidence/working-tree-fingerprint.ps1`、`cold-process-prodsim.ps1`）
**Changed Canonical Data** — **NONE**（`Assets/Resources/UI/PoE/passive_tree.json` 内容未变，SHA-256 见 §4）
**Changed Markdown** — 本文件、`docs/reviews/S6P/S6P_WO_04A.md`、`S6P_WO_04A_PLANNER_AMENDMENT.md`、
`docs/qa/PASSIVE_CENSUS_REPORT.json`(V2)、`docs/qa/PRODUCTION_SIMULATION_REPORT.json`(重生成)、
`docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`(重生成)、`开发计划/UNATTENDED_STATE.md`、`开发计划/规划AI会话.md`

---

## 2. Gates

| 项 | 值 |
|---|---|
| Entry Discovered EditMode | 409 |
| Entry Discovered PlayMode | 14 |
| Entry Canonical Hash | `FNV1A64:ec1d3ed67d3035d0`（3 个独立冷进程已证，2026-09-11 04:51–04:52） |
| Final Discovered EditMode | **426**（+17） |
| Final EditMode Fail/Skip | **0 / 0** |
| Final Discovered PlayMode | **17**（+3） |
| Final PlayMode Fail/Skip | **0 / 0** |
| Predecessor tests missing/weakened | 0 个未解释；5 处适配全部披露（WO 记录 §7） |
| Content Audit | **PASS**（本轮重新生成 `docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md`，07:54:40） |

**Cold Entry Run1/2/3:** `FNV1A64:ec1d3ed67d3035d0` ×3，EXACT = YES（工具 `tools/evidence/cold-process-prodsim.ps1`）
**Cold Exit Run1/2/3:** `FNV1A64:ec1d3ed67d3035d0` ×3，**EXACT = YES**
（2026-09-11 07:58:42 / 07:58:56 / 07:59:10，各 15.9s/14.1s/14.2s，exit=0，`contract=pv|passive-v1`，`invalid=0`；
原文 `docs/reviews/S6P/_wo04a_cold_exit.txt`）

**Final ProdSim Hash:** `FNV1A64:ec1d3ed67d3035d0`
**Expected Hash:** `FNV1A64:ec1d3ed67d3035d0`
**Hash Unchanged:** **YES**（无 rebaseline；`passiveSensitivity` 的 A/B/C 对照全部仍按旧值变化，见报告 JSON）

---

## 3. Passive source provenance（§21，只入证据、不进 gameplay hash）

| 项 | 值 |
|---|---|
| Passive source file | `_poe_src/tree_raw.json`（6,666,937 bytes；官方天赋树页面内嵌 tree 数据） |
| Source provenance/version | 官方页面内嵌数据，2026-09-10 21:56（本地）抓取；生成链 `_poe_src/fetch.js`（美术）→ `_poe_src/gen.js`（结构/坐标/连线）→ 运行时 `passive_tree.json`；美术源 = poedb.tw 同源 CDN |
| `tree_raw.json` SHA-256 | `C3B14448401FF432BF6D306B0E099FD24F57920CC06E4702220D25FE8F936826` |
| Runtime data `Assets/Resources/UI/PoE/passive_tree.json` SHA-256 | `2907524F072DDC2C965B02446BA0B3A8D161C8F057B063F5E4C7BE9BC458ED7F` |
| Checksum in gameplay hash? | **NO**（仅作 evidence provenance） |

---

## 4. Support truth 结果面

**Support Truth Owner:** `Game.Runtime.Core.PassiveSupport`（单一 owner；census/ContentAudit/UI/SliceSession 全部消费它）
**Second UI Oracle Found:** **NO**（门禁 `NoSecondUnsupportedNodeOracle` 扫描 runtime 目录无第二套表/原因文本、
测试目录无第二套关键词表、`SliceHud.cs` 无 `PoeStatParser`）

| 指标 | 值 |
|---|---|
| On-Tree Nodes | 2429（未上树 403 = 源数据记账，不入可分配分母） |
| ALLOCATABLE_SUPPORTED | **367** |
| BLOCKED_CURRENTLY | **1660** |
| BLOCKED_SPECIAL_INTERACTION | **87**（57 珠宝孔 + 30 时光珠宝类无连线节点） |
| SPECIAL_PENDING_MASTERY | **315** |
| STRUCTURAL/Other（索引域外） | 0（`OutOfDomain` 只对索引越界返回） |
| Mixed Nodes（含特殊类别） | **236** |
| Mixed Nodes（普通域内，判据使用） | **211** |
| Frozen Mixed Node Example | **71** = `20% increased Mana Regeneration Rate`(BLOCKED/Recovery) + `+5 to Intelligence`(CONSUMED)；**起点直接邻居** ⇒ 相连性成立，被拒唯一原因只能是支持门。另冻结 12（consumed 进攻行）与 255（consumed + blocked + structural） |
| CONSUMED Lines | 607（与 WO-01 相同：严格化未翻任何一行） |
| Blocked Lines | 4475 |
| Special Lines | 12 |
| Structural Lines | 582 |
| Unknown Lines | **0** |

**Runtime Consumer Families:**
① 面板/防御 `RecalcPlayer → PlayerStats`；② 攻击结算 `BuildPlayerHit → HitRequest → CombatMath.ResolveHit`；
③ 技能形态 `ResolveSkillDef`；④ 机制 `ArenaSim.ResolveProjectile → ForkCount`。
`StatId` 全枚举 28/28 均在 `PassiveSupport.RuntimeConsumedStats` 内（清单 owner = runtime，测试改持别名）。

**Offensive Skill-Context Evidence:** `SupportedOffensiveNodeChangesActualSkillContext` —— 559 点亮后
`CollectSkillMods(Melee).RawIncreased(AttackSpeed)` 上升，且 `ResolveSkillDef(Melee).Recovery` 下降（真消费者）。
**Player/Global Evidence:** `SupportedDefensiveOrAttributeNodeChangesActualPlayerResult` —— 2034 点亮后
`PlayerStats.Get(Life)`/`Get(Strength)`/`MaxLife` 全部上升。

**Supported Allocation:** 559/1795/2034 正常可点、恰好扣 1 点、`NodeState = Allocated`
**Unsupported Allocation:** 拒绝，原因 = `该节点含当前引擎无法完整兑现的效果`（稳定文本）
**Unsupported Point Delta:** 0 ｜ **State Delta:** 0（`Allocated[]` 未写入）｜ **Player Result Delta:** 0 ｜ **Skill Result Delta:** 0
（`UnsupportedNode*` 四个族 + PlayMode `UnsupportedNode_Reject_LeavesPointsStateAndResultsUnchanged`）

**Injected Blocked Node Effect:** 绕过分配门直接写 `Allocated[12]=true; Allocated[71]=true` 后：
`BlockedAllocatedCount == 2`（invalid-state 自证）、玩家 Life/Mana 与技能包 PhysicalDamage/Accuracy 与基线**逐项相等**
（consumed 子行不泄漏）；同一手法注入 supported 559 则**确实生效**（对照组）。
生产 API 未新增任何「强制分配」入口。

**Mastery Transitional State:** 315 个专精全部 `SPECIAL_PENDING_MASTERY`
**Mastery Allocation Result:** **REJECT**（原因 `专精暂未开放显式选择`；PlayMode 亦证）
**Implicit First Choice Effect:** **0**（全树 315 个专精 `PassiveCatalog.Get(i).Mods.Length == 0`；
旧 `FirstChoice` 烘焙路径整体删除；损坏状态注入后玩家/技能结果与基线逐 StatId 相等）
**Mastery Point Delta:** **0**

**403 Unrendered Handling:** 仅源数据记账（`PoeTree.Nodes` 只含 2429 上树节点；索引越界返回 `OutOfDomain`，不进 UI/分配分母）
**Blocked Domain Count:** 与 WO-01 相同的缺失轴关键词表（72 条轴标签），本轮**未新增任何玩法域**

---

## 5. Delta 声明

| 项 | 结果 |
|---|---|
| Gameplay Delta | **YES** — 被动可分配性/支持真相（有界更正） |
| Passive Runtime Delta | **YES** — 单一 support truth + 分配门 + 消费门 + UI 真相 |
| Mastery Feature Delta | **NO** — 只有临时 fail-closed（未建立选择语义） |
| Parser Delta | **NONE** |
| StatId Delta | **NONE** |
| Canonical Data Delta | **NONE**（`passive_tree.json` 字节未变，SHA-256 同上） |
| Content Delta | **NONE** |
| ProdSim Contract Delta | **NONE**（`pv\|passive-v1` 与 canonical hash 均未变） |
| 未事前预估的行为变化 | **一处**：57 个珠宝孔由「无词条 ⇒ 真空 supported」改为 BLOCKED_SPECIAL_INTERACTION（合同 §12 的明确要求，已单列于 WO 记录 §3） |

**Drift:** 0 ｜ **Source-of-Truth Conflicts:** 0 ｜ **Forbidden Expansion Audit:** PASS

---

## 6. Defects / Open questions

1. **Entry working-tree fingerprint 未捕获**（流程缺口，非产品缺陷）。入口侧可给的事实：HEAD `64b614f`、
   入口门 409/409 + 14/14、入口 cold ×3 `ec1d…`。出口指纹已提供。**已写入执行规则：gameplay 改动前先落指纹。**
2. CONSUMED 严格化在当前数据上不可观测（28/28 StatId 都有消费者）——已在 WO 记录 §9 单列，非省略。
3. 专精隐式首条效果在当前数据上本来就解析不出（315 个专精 `choices` 首行全落空），拆路径属消除隐患。
4. 开放问题（供规划 AI 裁定）：`S3R2FireConversionTests` 的「真实被动 → 技能上下文」端到端路径
   被本轮产品真相合法关闭，改为「被拒 + 同轴 StatBag 聚合」是否接受。

---

## 7. Recommended Planner Read

1. `Assets/Runtime/Core/Gameplay/PassiveSupport.cs`
2. `Assets/Tests/EditMode/PassiveSupportTests.cs`
3. `docs/qa/PASSIVE_CENSUS_REPORT.json`（V2）
4. `docs/reviews/S6P/_wo04a_cold_exit.txt`、`docs/reviews/S6P/_wo04a_fingerprint_gate.txt`
5. `docs/reviews/S6P/S6P_WO_04A.md` §7（测试适配）与 §9（遗留缺口）
6. 若 ACCEPT → 按合同 §31 释放 WO-03（+ 三条 amendment）

---

## Final — 终态工作区指纹（Amendment 7）

```text
Working-Tree Fingerprint (content-addressed, SHA-256)
branch                : main
HEAD                  : 64b614f2a0effc0e073f5997fa595702effd5bc1
tracked+untracked files: 13763
total bytes           : 2061992877
WORKING-TREE HASH     : f96824ecc0237beb1939f308e03f2264f80754e377f01fa84a57ab07675d8a20
dirty                 : YES
```

说明：`gate` 指纹（§头部 `c5dfe500…`，13758 files）是**门禁实际运行过的代码/测试状态**；
终态指纹（13763 files）在其之上增加了 5 个证据/文档文件（本包、WO 记录、amendment、cold exit、gate 指纹原文）。
两者之差**只在文档**，代码与测试零差异（可用 `git status --porcelain` 的 `docs` 区与 `Assets` 区核对）。
逐文件明细：`docs/reviews/S6P/_wo04a_fingerprint_final.txt`、`_wo04a_fingerprint_gate.txt`。

