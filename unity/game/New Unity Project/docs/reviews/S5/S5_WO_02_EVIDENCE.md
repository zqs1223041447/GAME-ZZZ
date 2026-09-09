# S5_WO_02_EVIDENCE — Evidence Pack（S5-WO-02 Multiple-Link Domain Core）

**Work Order**: S5-WO-02 — Multiple-Link Domain Core（规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d` 2026-09-09 随 WO-01 Gate Review=ACCEPT WITH FOLLOW-UP 放行）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ 901f26d

## Pre-Code Contract Patch（代码前强制收口，WO-01 评审裁定条款）

- `S5_LINK_CONTRACT.md` 新增 §2a：改挂语义获批（整体迁移不叠加；每技能有效连接源≤1；Support 列表跟随技能身份；清除恢复默认）；必须拒绝的写入三类（自改挂/双物品同技能/双源态）+ 禁 first/last-wins + 腐败态 fail-closed；缺组 parity 明确条款；改挂生效前提与划分；总容量取舍示例（3S：legacy 2 → 拆分 0+1，有意设计非回归）；**原子容量校验**（写入前两组容量校验，溢出=原子拒绝，禁静默截断/移除/迁移/重排）；生命周期（Assign/Change A→B 单事务/Clear）。

## Affix Candidate 20 Resolution（documentation-only；WO-02 零词缀实现）

- 按裁定 **Option B 替换**：原候选 20「开阔（AreaRadiusMore/More，溯源 increased AoE——运算语义未证匹配）」→ 新候选 20「**坚韧**（`Strength`/`Flat` 6-12，不限槽，PoE "+X to Strength" exact flat 语义匹配）」。N=4 不变；总量 21 不变；无新族/无 Prefix-Suffix/无 Tier-ModGroup/无 BL-024 全部保持；原候选留档（若未来出现 more-AoE 实证可另行立令）。
- 铁骨（候选 18）按裁定补溯源/适用性区分表述：PoEDB 溯源仅证词条语义；「Belt 不可出」=GAME-ZZZ 有界适用性决策，非 PoEDB 事实。

## Changed Runtime Files（恰 BL-021.A2 域核）

| 文件 | 变更 |
|---|---|
| `Assets/Runtime/Core/Gameplay/Catalogs.cs` | `ItemInstance` 追加 `LinkSkill1`（SkillId；None=0 ⇒ default 即 legacy 单组，既有数据零迁移；唯一写入口=TryReassignLink） |
| `Assets/Runtime/Core/Gameplay/SliceSession.cs` | 新增 `MappedSlot`/`MappedSlotOfSlot`（既有硬映射提为 helper，语义零变化）、`RebindHostIndex`（改挂查询；腐败双改挂 fail-closed 忽略改挂回退 legacy，禁 first/last-wins）、`CountSupports`、`TryReassignLink`（唯一写入口：BuildLocked/无效索引/自改挂/孔数<3/双物品同技能改挂/两组容量溢出=确定性拒绝无半写入；清除/变更=事务性）；`SupportCapacity` 派生扩展（改挂技能恒 1；host 自带 group 1 时 group 0=SocketCount−2 孔）——**无改挂时与既有行为逐位等价** |
| `Assets/Tests/EditMode/MultiLinkDomainTests.cs` | 新建：14 项域核契约测试（见下） |

**Changed Assets/Scenes/Prefabs: NONE**；UI：NONE（显式 Out of Scope）；词缀目录：零改动（条目仍 17）。

## Data Model / 行为要点

- Legacy Default：`LinkSkill1=None` ⇒ 单组=全部 SocketCount 孔，容量/兼容/顺序与既有逐位等价（EditMode 260/260 含全部既有回归）
- Max Groups：2；Partition：group 1=末尾 2 孔、group 0=前部其余（确定性）
- Eligibility Rule：不变（SupportCapacity 硬映射三槽；2 组资格=SocketCount≥3 ⇒ 当前=Weapon/Body；Helmet 2S/Boots 0S/Gloves·Belt 1S=显式拒绝）
- Unique-Link-Source Rule：每技能至多一个有效连接源；自改挂拒；双物品改挂拒；**无 first/last-wins**（腐败态读路径 fail-closed 回退 legacy）
- Capacity Validation：写入前原子校验（host 映射技能 ≤ SocketCount−3；被改挂技能 ≤1）；溢出=拒绝且零半写入；无静默截断/迁移/重排
- Third-Group Handling：表示层无第三槽位 ⇒ 不可表示+API 拒绝

## Tests（逐项对账：EditMode 246 → 260，+14 全部为 MultiLinkDomainTests，零删除/零削弱）

1. LegacyParity_DefaultNoRebind_CapacitiesAndBehaviorUnchanged（legacy parity：容量 2/2/1+默认装配回归）
2. LegacyParity_DisplaySockets_GlovesBelt_NotLinkEligible（手套/腰带 1 孔仅展示——制造+装备后仍拒；Boots 0 孔拒）
3. TwoGroupDerivation_PartitionAndCapacities（2 组推导：1/0/1 容量+group 1 单 Support 位）
4. TotalCapacityTradeoff_ThreeSocket_Legacy2VsSplit0And1（总容量取舍 2-0-1 显式断言=有意设计）
5. SelfReassign_Rejected_StateUnchanged（自改挂拒+零半写入）
6. DuplicateRebind_SecondHost_Rejected（双物品同技能改挂拒+零半写入）
7. Rebind_SuppressesDefaultSource_Clear_Restores（默认源抑制/清除恢复/Support 列表跟随技能身份）
8. ChangeRebind_AtoB_SingleTransaction（A→B 单事务无中间双源）
9. CorruptDuplicateState_FailClosed_FallsBackToLegacy_NoFirstWins（腐败态 fail-closed 回退 legacy，禁 first-wins）
10. AtomicCapacity_HostGroup0Overflow_Rejected（host 溢出拒+Support 不被截断/迁移）
11. AtomicCapacity_IncomingSkillOverflow_Rejected（来向溢出拒+零半写入）
12. GoldenCompatibility_PositionIndependent_Group0_And_Group1（golden 7×3=21 组合在 group 0/group 1 两位置全对拍，oracle 零复制）
13. ForbiddenSurface_NoColorLevelQualityOrThirdGroupFields（反射断言：无 Color/Level/Quality/LinkSkill2/LinkGroup 字段；恰含 LinkSkill1）
14. Eligibility_Helmet_TwoSockets_CannotHostSecondGroup（Helmet 2S 资格不变）

**结果**：EditMode **260/260 PASS**（failed 0, skipped 0）；PlayMode **11/11 PASS**；Content Audit **PASS fresh=YES failures=0**；Quick Gate **PASS exit 0**（首跑曾 259/260——测试自身 bug：手套/腰带初始未装备（Equipped=-1）传参致「无此物品」断言不符，修正测试（先制造+装备）后全绿；产品代码零改动）。性能门不重跑（Phase 1 域核零热路径变化——RebindHostIndex 仅在装备/聚合/取容量时执行，非每帧；最终性能复验按计划在 Phase 5）。

## Capability State Promotion

**NONE（不晋升）**——Ledger/Matrix 记候选事实：「Multiple Link Groups=域核已实现（S5-WO-02），用户侧集成待 Phase 2」；Socket/Link 总体保持 Partial（孔色缺席）；Socket Color/Gem Level-Quality=导演显式拒绝（负向条件）。

## Delta 声明

- **Capability Delta**: NONE（不晋升，候选事实跟踪）
- **Mechanic Support Delta**: NONE（同上）
- **Runtime Delta**: 恰 BL-021.A2 域核（2 个 .cs 文件+1 个测试文件；授权范围内）
- **Canonical Data Delta**: NONE（ContentData/Catalogs 目录数据零改动——LinkSkill1 为结构字段非数据条目）
- **Content Delta**: NONE（Affix Entries Added = 0；Skills/Supports 零改动）
- **Authorization Delta**: NONE（本单自身不新增授权）
- **Socket Color Introduced: NO**；**Gem Level/Quality Introduced: NO**

## Drift / Source-of-Truth Conflicts

**0 / 0**（首跑失败为测试 bug，非文档漂移；已修复并复跑全绿）

## Forbidden Expansion Audit

**PASS**——零 UI/零词缀/零孔色/零宝石成长/零持久化/零新槽位；runtime 改动恰在授权域核内；WO-01 评审裁定（改挂语义/原子校验/fail-closed/候选 20 替换）全部落地。

## Open Questions（供规划 AI）

1. 域核 API 已就绪但无 UI 入口（Phase 2 范围）——Phase 2 的 UI 边界请按计划「仅扩到能表示两组」下发时给出精确呈现需求（技能行连接来源标注/物品 Tooltip「2 连接组」），我将按合同实现。
2. 候选 20 已替换为「坚韧 Strength/Flat」——是否认可（若你希望保留 AoE 轴，可在 Phase 3 前给出 exact-match 的 more-AoE PoEDB 实证，我将恢复开阔候选）。

## Recommended Next WO

**S5-WO-03 — Multiple-Link Runtime & UI Integration**（Phase 2：两组接入既有 loadout/UI 路径；golden 双组位集成证据）——由你按 Gate Review 结果下发。
