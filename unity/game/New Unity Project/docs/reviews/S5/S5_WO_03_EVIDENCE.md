# S5_WO_03_EVIDENCE — Evidence Pack（S5-WO-03 Multiple-Link Runtime & UI Integration）

**Work Order**: S5-WO-03 — Multiple-Link Runtime & UI Integration（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 WO-02 Gate Review=ACCEPT 放行 Phase 2）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ 01700b9

## Changed Files

| 类别 | 文件 |
|---|---|
| Changed Runtime Files | `Assets/Runtime/Core/Gameplay/SliceSession.cs`（共享校验器+装备守卫+UI 读路径助手）、`Assets/Runtime/Core/Gameplay/Catalogs.cs`（**零改动**——本单未触碰） |
| Changed UI Files | `Assets/Runtime/Core/Gameplay/SliceHud.cs`（技能行源标注+第二连接配置条+紧凑物品行）、`Assets/Runtime/Core/Gameplay/SliceTooltipModel.cs`（ItemCard 追加连接组划分行） |
| Changed Markdown | `docs/RUNTIME.md`（必改 4 项全覆盖）、`docs/DECISIONS.md`（S5-WO-03 集成语义条目）、`docs/ROADMAP.md`（S5 行 Phase 2 已执行，零 scope 扩张）、`S5_LINK_CONTRACT.md`（状态行+§2b 集成补记）、`S5_SCOPE_LEDGER.md`（BL-021.A2 行实现进度）、`S4R_CAPABILITY_LEDGER.md`（Socket/Link 行 S5 跟踪更新，仍 Partial 不晋升）、`S4R_MECHANIC_MATRIX.md`（Phase-2 证据记录，不晋升）、`规划AI会话.md`（S5 事件回填） |
| Changed Assets/Scenes/Prefabs: **NONE** | COMBAT_MATH.md：**VERIFY ONLY 零公式改动（已核对未触碰）**；词缀目录：零改动（**条目仍 17**） |

## Shared Validation Owner（合同 §3A 单一权威）

- **`SliceSession.ValidateLinkGraphPostState(pendingIdx)` = 唯一规则所有者**；内核 `ValidateHostState(selfIdx, host, bound)`（假设值入参）覆盖全部 7 项：host 资格（映射槽）、SocketCount≥3、自改挂、双物品同改挂（唯一有效源，禁 first/last-wins）、拆分后 host 容量（Supports≤SocketCount−3）、来向组 1 容量（≤1）、唯一有效连接源。
- `TryReassignLink` 重构=**假设性写入 → 共享校验 → 非法原子回滚**（清除=约束放宽恒合法，注释说明不变式论证）；`TryEquip` 重构=**假设性提交 → 同一校验器 → 非法整次回滚**。**零规则副本**（装备路径与改挂路径同走所有者；UI 只读预判 `PreviewReassignError` 亦经同一内核）。
- 全部 5 类拒绝消息保持 WO-02 原文案（tests 逐字断言）：①host 不承载（孔数不足 3/不映射技能连接）②超出拆分后容量 ③超出第二组容量（1）④已被其它装备改挂 ⑤该物品已自带此技能连接。

## Equip/Unequip/Swap Integration（合同 §3B）

- 本仓库**既有装备变更操作=TryEquip 单入口**（equip=替换双语义；无独立 unequip/sell/remove/swap API——保留既有严格行为，未发明新操作）。守卫覆盖：equip（配置 host 入装/替换/同槽交接）、TryDirectedCraft（原位词缀编辑，绑定与划分保持）、TryRandomCraft（整件重铸=新物品 legacy 单组，被改挂技能回默认源=合法转换非静默清除）。
- 「配置物品换装」语义澄清（新增，Evidence 如实申报）：**同槽同技能配置 host 交接=合法移交**——旧 host 随替换离开连接图（入包休眠，绑定保留为物品自身状态），唯一源保持恰一；跨槽重复所有权才构成冲突（B2 拒绝）。

## Atomic Equipment Rejection

- B2（跨槽重复 group-1 所有权装备=拒）/ B3（自改挂绑定入装=拒）/ B7（容量溢出装备=拒）/ B8（两种装备顺序同拒=序无关）——**失败=整次拒绝，Equipped+全部 Inventory[i].LinkSkill1+Q/W/ESupports 快照逐字节相等**（B7 专用字节断言助手）。
- 合法转换：B1（预配置 host 入装）/ B4（host 交接）/ B5（双配置交替装备=唯一性保持）/ B6（host 移除→默认源恢复+Support 列表跟随技能身份）/ B9（原位制作保持绑定+重铸回 legacy）。

## Direct LinkSkill1 UI Writes: **NONE**

UI 全部经 `s.TryReassignLink(...)`（配置条按钮/清除按钮唯一写入口）；`PreviewReassignError`/`RebindCandidates`/`LinkSourceLabel`/`LinkGroupsText`/`SecondaryLinkConfigurable` 均为只读（预判零写入——与写入前校验共用 `ValidateHostState` 内核，非复制规则）。

## Secondary-Link UI（合同 §4）

- **呈现位置**：角色面板（Build）六槽卡内、已装备且 `SecondaryLinkConfigurable(it)`（映射槽 ∧ SocketCount≥3，与域资格同源）的物品卡底部 20px 有界配置条；物品行转紧凑单行（词缀详情看 Tooltip，无新屏/无重构/无拖拽编辑器）。
- **Eligible Visibility Rule**：头盔 2S/手套 1S/腰带 1S/靴子 0S/＜3 孔映射槽物品=无配置条（C1 逐槽断言）；无 Socket Color 控件。
- **Candidate Filtering**：`RebindCandidates`=3 技能−自带映射−已被其它装备改挂者（含当前绑定自身）；配置条不呈现 host 映射技能按钮（比「可选但拒绝」更强）；重复改挂技能不呈现为可用（C3）。
- **Failure Feedback**：不可用候选=灰显+hover Tooltip 原因+点击→LastMessage 确定性可见拒绝（状态零改动）；可用候选点击→TryReassignLink 失败亦回落 LastMessage；「无」按钮=清除（未绑定时灰显）。

## Skill Source Presentation（合同 §5）

- 技能行（底栏三格）新增唯一有效源标注 `LinkSourceLabel`：`武器·组0·容2`（默认）/ `铁刃·组1·容1`（改挂，host 物品名+组号+容量）/ `未装备`；改挂后原默认位**不再呈现为生效源**（D2 断言不含组0/胸甲）；每技能行恰一组标注（D4「组」计数=1，无重复 Skill 行）；标注容量=域值（D3）。

## Tooltip Group Presentation（合同 §6）

- `ItemCard` Body 追加 `LinkGroupsText` 划分行：legacy=`组0 连接：<映射技能> 容<SocketCount−1>`+`第二连接：无`；active split=`组0 … 容0`+`组1 … 容1`（3 孔拆分如实 0+1）；**不暗示免费孔位**（E4 断言无「容3」）；非法/腐败绑定=读路径 fail-closed 按 legacy 呈现（E5）；容量=域 `SupportCapacity`（E3）。

## Stat-Support Group0/Group1 Evidence（合同 §3C/F）

- **数值型**（残暴 Brutal=MorePhysical More 0.40）：组0 位（Body 默认）与组1 位（武器改挂弹道）`BuildPlayerHit(Projectile).MorePhys` 均=1.4 且互相相等——**等价生效恰一次**（F1）。
- **机制型**（分裂 Fork=MechanicSkill 弹道）：同 seed 9u 双 sim，组0/组1 各装 Fork → 真实 ArenaSim 施放命中 → `ForkSpawns==2` 双证明（F2）——**真实 runtime 行为等价，非仅数据模型**。

## Cross-Group Leakage Evidence

- 结构隔离①：拆分后 host 组0 容量=0，host 组0 Support 无法与改挂共存（原子校验拒绝，无泄漏载体，F3）；结构隔离②：改挂技能聚合只来自其唯一组（第三技能 Support 不进入 Projectile 聚合，F3）；组1 Support 不影响 host 组0（F4 BuildPlayerHit(Melee).MorePhys=1）。

## Golden Group0 / Golden Group1

- **21/21 / 21/21**——WO-02 `GoldenCompatibility_PositionIndependent_Group0_And_Group1` 原样保留全绿（位置无关对拍，golden oracle 零改动零复制）；本单未新增兼容路径。

## Gate Results

| 门 | 结果 |
|---|---|
| EditMode | **PASS 291/291**（260 基线 +31 项 MultiLinkIntegrationTests，零删除/零削弱；首跑 289/291 两项失败=**测试前提错误**（B3 同槽同技能交接本为合法移交，改为自改挂绑定入装冲突；C3 胸甲候选排除=自带映射为弹道非近战），**产品代码零改动**，复跑全绿） |
| PlayMode | **PASS 11/11** |
| Content Audit | **PASS / fresh=YES / failures=0** |
| Quick Gate | **PASS**（Gate: PASS；性能门不重跑——共享校验器仅在装备/改挂调用时执行 O(槽位²)=≤36 次比较，非每帧热路径；最终性能复验按计划 Phase 5） |

## Required Tests 对账（合同 §9 A–G）

- **A 域回归**：WO-02 全部 14 项 MultiLinkDomainTests 原样全绿（260 基线零削弱）。
- **B 装备不变量**：B1–B9 恰 9 项（含序无关 B8、字节不变 B7、host 移除 B6、交换 B5）。
- **C UI 配置**：C1–C7 恰 7 项（资格呈现/自带映射不可选/重复改挂不可用/合法赋值更新/A→B/清除回 legacy/失败原因+状态保持）。
- **D 源呈现**：D1–D4（组0/组1/原位不再激活/容量=域值/无重复行）。
- **E Tooltip**：E1–E5（legacy 一组/双组真实/容量=域值/不暗示免费孔/腐败 fail-closed）。
- **F 运行时隔离**：F1 数值型等价/F2 机制型真实分裂等价/F3+F4 零跨组泄漏（golden 21/21 双位维持）。
- **G 负面表面**：G1（ItemInstance 字段+SliceSession 方法名反射：零 Color/GemLevel/Quality/LinkSkill2/ThirdGroup）/G2（LinkGroupsText 全可达态零「组2」）。

## Capability State / Mechanic State

- **Multiple Link Groups — domain + runtime/UI integration implemented**（Ledger/Matrix 已记录）；**Socket/Link 总体保持 Partial**（Socket Color 未支持未授权）；**不晋升**（无独立 Socket 机制晋升）。

## Delta 声明

- **Runtime Delta**: 恰 BL-021.A2 集成（2 个 runtime 文件+2 个 UI 文件+1 个测试文件；授权范围内）
- **Canonical Data Delta: NONE**（ContentData/目录数据零改动）
- **Content Delta: NONE**（Affix Entries Added=0；Skills/Supports 零改动）
- **Authorization Delta: NONE**（本单自身不新增授权）
- **Affix Count = 17**（词缀实现=WO-04 范围，本单零词缀工作）
- **Socket Color Introduced: NO**；**Gem Level/Quality Introduced: NO**（G1 反射断言护栏）

## Drift / Source-of-Truth Conflicts

**0 / 0**（首跑 2 项失败为测试前提错误并当场修正，产品代码零改动，非文档漂移；已复跑全绿）

## Forbidden Expansion Audit

**PASS**——零孔色/零宝石等级品质/零第三组/零任意划分编辑/零新 Skill/Support/槽位/零词缀/零持久化/零新全屏系统；runtime 改动恰在 BL-021.A2 集成范围内；合同 §3B「保留既有严格行为」遵守（未发明 unequip/sell/remove/swap 新操作）。

## Open Questions（供规划 AI）

1. **同槽配置 host 交接语义**：本单实现为「合法移交」（旧 host 离图入包休眠、新 host 接管同一技能、唯一源保持恰一）。这是 TryEquip 同槽替换语义的自然推论；若你裁定交接应拒绝（要求先清除再装备），我可以在下一令改为原子拒绝——语义二选一，当前按合法移交落地并已测试（B4）。
2. **未装备物品赋值语义**：域 API 允许对未装备物品预配置绑定（WO-02 行为保持）；绑定在物品入装时经同一共享校验器复核（B1/B4/B7 实证）。装备时校验=唯一防线，未装备赋值不再提前做容量预检（赋值时仅做绑定合法性+唯一源+「成为 host」容量假设核算）——与 WO-02 内部实现顺序略有差异，可观察行为对既有测试逐位等价，请确认接受。

## Recommended Next WO

**S5-WO-04 — Bounded Affix Breadth**（Phase 3：按已锁定 4 条清单落地=迅疾/铁骨/睿智/坚韧，17→21，含 count-guard 更新；不做清单外任何词缀工作）——由你按 Gate Review 结果下发。
