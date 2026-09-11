# S6P-WO-01 — Passive Truth Census & Gate Coverage Audit

周期：S6P — Passive Truth & Deterministic Build Backbone
来源：规划 AI 会话（`game-zzz-planning`）2026-09-11 释放
性质：**无 runtime 变更**（域层 NONE / 绘制层 NONE）
产物：`docs/qa/PASSIVE_CENSUS_REPORT.json`（机器可读，测试再生，禁止手填）

---

## §1 为什么先做这个

这是后续所有被动工作的**分母**。当前同时流传着 2429 / 2832 / 315 / 57 / 30 / 558 六个数字，
但它们的集合包含关系从未写死；在写死之前，「78% 词条未覆盖」这种说法无法指导代码工作。

同时有一个更要命的问题要就地查清：**ProdSim 的确定性哈希到底看不看得到被动构筑真相。**
如果看不到，那么「hash unchanged」这盏绿灯对天赋域几乎没有保护作用。

---

## §2 交付物

| 项 | 位置 | 说明 |
|---|---|---|
| census 收集器 + 报告渲染 | `Assets/Tests/EditMode/PassiveCensus.cs` | 纯 tooling 层；单一真值：节点/词条取 `PoeTree.Data`，命中判定取 `PoeStatParser` 本体（不复制正则） |
| 门禁套件 | `Assets/Tests/EditMode/PassiveCensusTests.cs` | 12 条断言 |
| 机器可读产物 | `docs/qa/PASSIVE_CENSUS_REPORT.json` | 确定性渲染（无时间戳/GUID/绝对路径），同一仓库状态两次渲染 byte 级一致 |

---

## §3 节点集合对账（六个数字的包含关系）

| 集合 | 值 | 出处 |
|---|---|---|
| 官方天赋树数据节点总数 | **3390** | `tree_raw.json` 的 `nodes` 键总数（源数据，引擎内不可重算） |
| 升华（ascendancy）节点 | **558** | 带 `ascendancyName` 的节点（本域不纳入） |
| 非升华节点 | **2832** | = 上树 2429 + 未上树 403 —— **这就是导演口径的那个 2832** |
| 上树（本域实际驱动） | **2429** | `passive_tree.json` 节点数，引擎内可重算 |
| 未上树（无坐标） | **403** | 非升华但 `groups[node.group]` 不存在；官方页面同样不绘制（多为涂油/触媒类显著点） |
| 簇 | **797** | |

**关键澄清：「2832 节点全部驱动数值」在本域的可落实口径是 2429。** 剩下 403 个在官方数据里
没有坐标、官方页面也不绘制，**不可能被任何玩家点亮** —— 它们不是「漏驱动的节点」，而是不在树上的节点。

上树 2429 的分类（合计恒等式已断言）：

| 类别 | 数量 |
|---|---|
| 普通点 | 1518 |
| 显著点（notable） | 483 |
| 基石（keystone） | 49 |
| 专精（mastery） | **315** |
| 珠宝孔 | **57** |
| 职业起点 | 7 |
| 其中「可见但无连线、不可点」（时光珠宝类显著点） | **30** |

词条文本覆盖：2008 个节点带 `stats` 文本，**421 个不带**（315 个专精的全部效果在 `choices` 里，其余为纯枢纽点）。
带可选效果的节点数 = 315，与专精数一致。

---

## §4 效果行四分类（UNKNOWN = 0）

对全部上树节点的 `stats` 行与专精 `choices` 行逐行分类。判定顺序：括号提示 → 引擎可解析（CONSUMED）
→ 命中缺失轴关键词（BLOCKED_BY_DOMAIN）→ 无数字且无已知轴（SPECIAL_INTERACTION）→ 否则 UNKNOWN。

| 桶 | 行数 |
|---|---|
| `CONSUMED`（引擎真实消费） | **607** |
| `BLOCKED_BY_DOMAIN`（引擎没有该轴） | **4475** |
| `SPECIAL_INTERACTION`（bespoke 行为描述，无数字） | **12** |
| `STRUCTURAL`（括号内提示文本） | **582** |
| **合计** | **5676** |
| `UNKNOWN` | **0** |

`CONSUMED` 与 `PoeStatParser.MappedLineCount` 的逐节点汇总做了交叉断言 —— 不允许存在第二套判定。

### 4.1 缺失轴分布（= S6P-WO-04 的待办清单，非失败）

| 轴 | 行数 | 轴 | 行数 | 轴 | 行数 |
|---|---|---|---|---|---|
| Ailment | 425 | Block | 184 | EnergyShield | 214 |
| WeaponType | 313 | Minion | 192 | Recovery | 190 |
| Life | 186 | Melee | 151 | Flask | 109 |
| Mana | 102 | Stun | 93 | Leech | 91 |
| Spell | 83 | Warcry | 80 | Impale | 74 |
| Charge | 73 | Projectile | 72 | Fire | 71 |
| Mine | 71 | Totem | 70 | Critical | 67 |
| Cold | 62 | Retaliation | 58 | Trap | 58 |
| DamageOverTime | 56 | Suppression | 56 | Aura | 55 |
| CastSpeed | 55 | Lightning | 54 | Rage | 53 |
| Stance | 53 | Accuracy | 52 | Curse | 52 |
| Buff | 51 | Chaos | 46 | Armour | 44 |
| Reservation | 44 | Movement | 42 | Conditional | 41 |
| Elemental | 39 | Tincture | 39 | Cooldown | 37 |
| Evasion | 37 | Offhand | 32 | Avoidance | 32 |
| Brand | 30 | Attribute | 28 | Physical | 28 |
| Reflected | 27 | AreaOfEffect | 7 | 其余 20 个轴 | ≤23 各 |

### 4.2 节点级资格（S6P-WO-04 的判据，本轮只登记不改行为）

| 资格 | 节点数 |
|---|---|
| `SUPPORTED`（全部效果行都能被现有引擎兑现） | **424** |
| `SPECIAL` | 0 |
| `UNSUPPORTED_CURRENTLY` | **2005** |

> **这就是本次审计最重要的发现：现在有 2005 个节点是「可以点亮、但包含引擎兑现不了的效果行」的。**
> 按 S6P-WO-04 的口径，这属于「花点买到假效果」的产品语义缺陷；本轮不改行为，只把它变成可计量的事实。

---

## §5 ProdSim 面审计：确定性哈希**看不到**被动构筑真相

读 `Assets/Tests/EditMode/ProductionSimulator.cs`，抓出全部哈希喂点（源码行号为证据）：

| 行 | 喂点 |
|---|---|
| 98 | `hash = HashString(hash, "\|cy\|" + done)` —— 仅 cycle 序号 |
| 119 | `hash = HashString(hash, ItemKey(it))` —— 掉落物品键 |
| 141 | `hash = HashString(hash, "rc\|" + ItemKey(it))` —— 随机制作物品键 |
| 172 | `hash = HashString(hash, "dc\|" + ItemKey(...))` —— 定向制作物品键 |
| 187 | `hash = HashString(hash, "eq\|" + (int)it.Slot + "\|" + ItemKey(it))` —— 装备物品键 |

| 是非题 | 答案 |
|---|---|
| ProdSim 的哈希负载提到 passive / allocated / node / stat / modifier | **否** |
| ProdSim 读 `Allocated[]` | **否** |
| 哈希依赖被动 modifier 结果 | **否** |
| ProdSim 对专精敏感 | **否** |
| 审计结论 | **`NOT_PASSIVE_SENSITIVE`** |

**结论（有据）：ProdSim 会建 `SliceSession` 并 `ResetTown`，但它的哈希只覆盖「每轮生成的物品内容 + 循环序号」。
改天赋、改专精、把某个节点从有效果改成没效果，哈希都不会变。**
这正是规划 AI 指出的「局部绿灯」，也是 S6P-WO-02 必须解决的问题。

---

## §6 被动分配不变量覆盖 census（黑盒探测，不复制判定逻辑）

只经 `TryAllocate` / `TryRespec` 的对外行为取证，用 `links` 仅做候选挑选。

| 不变量 | 当前是否成立 |
|---|---|
| ValidStart（起点恒已点亮） | ✅ |
| ConnectedAllocation（相连节点可点亮） | ✅ |
| NoIllegalJump（不相连节点被拒绝） | ✅ |
| PointAccounting（被拒绝的加点不扣点） | ✅ |
| DuplicateAllocationNoop（重复加点不二次扣点） | ✅ |
| ResetExact（R 重构精确清空并归还点数） | ✅ |
| **MasteryPrerequisite（专精前置 / 显式选择）** | ❌ **不存在** —— 登记给 S6P-WO-03 |

测试里对前六条断言为真（红了=加点语义被改坏），对专精一条断言为假
（**WO-03 建立后必须把该断言翻转为 true**，这是一个刻意留的"翻转哨兵"）。

---

## §7 门

| 门 | 结果 |
|---|---|
| EditMode | **394 / 394 PASS / 0 FAIL**（原 382 + 本令新增 12） |
| PlayMode | **14 / 14 PASS / 0 FAIL / 0 SKIP** |
| ProdSim | **`FNV1A64:9a4c9524d0b3e214` invalid=0 —— UNCHANGED**（与 S5 基线一致） |
| Drift | 0 |
| Forbidden Expansion | PASS（未引入新玩法域、未采购、未换框架） |

哈希策略符合规划 AI 的总表：本轮「Gameplay truth 不变 → 必须保持 `9a4c…`」。

---

## §8 环境披露（如实）

1. **本轮遇到并修复了一次编辑器挂死**。现象：pipeline 全部命令超时（连 `editor_stop` /
   `test_status` 都超时），进程 CPU 不再增长，`Editor.log` 被
   `IPCStream (hubIPCService): IPC stream failed to write (Timed out)` 刷屏。
2. **真因**：强杀编辑器后，下次启动会停在模态对话框 **"Recovering Scene Backups"**
   （`MainWindowTitle` 可查），该对话框不响应 pipeline，于是所有命令超时。
   `unity close` 在这种状态下无效（返回 `closed=false method=none`）。
3. **恢复配方**（已写入 `开发计划/UNATTENDED_STATE.md`）：
   杀掉 `Unity*` 全部进程 → 删 `Temp/__Backupscenes` 与 `Temp/UnityLockfile` → 重新 `unity open`。
4. **另一个坑**：编辑器处于 **Play 模式时 `run_tests` 会一直挂住**（不报错、不返回）。
   跑 EditMode 门禁前必须先 `editor_stop`。
5. 本令的 ProdSim 数字来自真跑（`Simulation_10kCycles_ZeroInvalid_AndReproducible` 与
   `Simulation_SameSeed_HashIdentical` 均 PASS，控制台两次都打印同一 hash）。

---

## §9 交给 S6P-WO-02 的接口

WO-02 需要建立 canonical 被动 loadout 并把它纳入哈希。本令已经为它准备好：

- `PassiveCensus.Collect()` 给出**分类真值**（哪些行 CONSUMED、哪些节点 SUPPORTED），
  WO-02 选 canonical 节点时应从 `SUPPORTED` 里挑，而不是随手挑。
- `ProdSimSurface.ProdSimHashInputSites` 记录了当前全部哈希喂点 —— WO-02 改完之后，
  这几处必须出现被动输入，`verdict` 必须从 `NOT_PASSIVE_SENSITIVE` 翻走。
- 所有本轮断言都是"翻转哨兵"：WO-02/03/04 一旦把行为改对，相关断言会红，
  强制同步更新口径而不是悄悄漂移。
