# S5_PLAN — GAME-ZZZ S5：Build Identity & Itemization Depth

**来源**：规划 AI（ChatGPT 镜像站，会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09）在导演原子批准后下发的 S5 短期计划与 S5-WO-01 全文。工作 AI 落库为 Repository-as-Memory，不得重新解释为别的周期。
**基线**：main @ bd4b914（S4R WO-01..03+F1 全 ACCEPTED；Director Direction Gate=DIR-1 APPROVED）。
**Cycle State at Issue**：**AUTHORIZED FOR PHASE 0**。Formal Phase State Rule：正式阶段状态仍只在最终 Director Gate 后统一更新（当前全部为候选事实）。
**Cycle State（2026-09-12 Director Final Gate）**：**COMPLETE**。导演勾选 `[x] APPROVE S5 FINAL GATE`。授权原子仍且仅仍是 `{BL-002.A1, BL-021.A2}`。本 APPROVE 不授权 Flask / Jewel / Ascendancy / Timeless / 新 Stat 轴 / 战役扩张。记录=`S5_DIRECTOR_FINAL_GATE.md`。

## 1. Direction Gate 结果（原子级）

- 导演显式批准：**BL-002.A1**（现有机制内有上限的新词缀扩展）、**BL-021.A2**（多组连接 / 扩展 Socket-Link 模型）。
- 导演显式拒绝（本周期不做）：**BL-021.A1**（孔颜色）、**BL-012.A1**（宝石等级/品质）。
- 其余全部 Default-on-Omission 维持原状态。**S5 实现授权 = 恰好 { BL-002.A1, BL-021.A2 }，不含任何其它能力**；旧「S5 硬停止」只对这两个原子解除，不得外溢。

## 2. Cycle Mission

S5 以复用已验证的 S4 面为主，提升构筑差异与词条广度。两个产品变更：①把当前单连接模型扩展为**严格有界的多连接组模型**；②**只用已支持词缀/机制语义**新增有界数量的词缀条目。S5 不开 progression、endgame、新装备槽、孔颜色、宝石成长、新词缀族、大规模内容生产。

## 3. Frozen Entry Baseline

| Domain | Entry Baseline |
|---|---|
| Equip Slots | 6 |
| Affixes | 17 |
| Canonical Skills | 3 |
| Canonical Supports | 7 |
| Passive | 16 节点 / 20 边 / 0 断连 |
| Enemy production visuals | 4 |
| Combat SFX | 5 |
| Voice | 3 GATED |
| EditMode | 246/246 |
| PlayMode | 11/11 |
| Runtime drift | 0 |
| Existing link model | 固定单连接组模型 |
| Socket color | unsupported / 显式不授权 |
| Gem Level / Quality | unsupported / 显式不授权 |

S4 性能与生产模拟证据为前代基线。

## 4. Hard Scope Limits

### 4.1 词缀广度硬上限
S5 最多新增 **+6** 条词缀目录条目（17 → 最多 23）。Phase 0 锁定精确 N（1≤N≤6）；N 事后变更需新的规划 AI 工作令。每条新词缀必须：唯一外部 canonical 来源=PoEDB（人工溯源，不触发 BL-024）；复用已支持的 runtime stat/modifier 语义；走现有单一 applicability truth；用现有装备槽语义；可经现有掉落/制作路径获得。不授权：新词缀族；新 Modifier 运算语义；Prefix/Suffix 架构；Tier 系统；ModGroup 系统；新生成规则框架；新装备槽；新 progression 资源。**BL-002.A1 批准 ≠ BL-002.A2/A3 批准 ≠ BL-029.A3 批准。**

### 4.2 多连接组硬上限
已具备连接能力的物品可从当前单连接组扩展为**每件最多 2 个连接组**。硬规则：组数 ∈ {0,1,2}；第三组必须被确定性拒绝；哪些物品/槽位具备连接能力不变；每组的孔位/容量规则沿用当前单连接语义；不因存在第二组而增加组内孔数；现有 Support 兼容真相继续权威；每组独立解析兼容性；Support/效果不得跨组泄漏；连接组顺序必须确定性；既有单连接数据保持有效并映射为 group 0（语义零变化）。**不引入**：孔颜色字段/匹配/UI/需求；宝石等级/品质字段或缩放；持久化/存档能力。这是连接组基数扩展，不是通用 Socket 系统重写。

## 5. Explicit Out of Scope

Progression Spine；Map Tier/progression；Boss；Atlas；Unique；Ring；Offhand；Amulet；New Content Batch；Phase 12 Content Factory；Voice activation；BL-024 PoEDB ingestion pipeline；Curse；Flask；Jewel；New Class；Deep Craft；Persistence/save；Aura/Reservation；Mastery；Ascendancy；新增 Defense 族；新增 Ailment 族；高级 Trigger；Socket Color；Gem Level/Quality；Prefix/Suffix；Affix Tier/ModGroup；新 Skill；新 Support；新词缀族；新 currency/resource。PoEDB 可作为已批准的有界词缀的人工 canonical 溯源，这不授权 BL-024。

## 6. Phase Structure

- **Phase 0 — Scope & Contract Lock**（=S5-WO-01）：把两个批准原子变成实现就绪合同。产出：精确 N（1-6）+ 词缀候选清单（PoEDB 溯源）；Multi-Link 领域合同；legacy 单连接映射；资格不变量；兼容隔离规则；UI 边界；验证/测试矩阵；禁区断言。Exit Gate：N 锁定；每条候选词缀过准入规则；MaxLinkGroupsPerItem=2 合同锁定；组隔离/legacy 行为无歧义；**尚无新 gameplay runtime 行为**；Quick Gate 保持绿。
- **Phase 1 — Multiple-Link Domain Core**（=S5-WO-02，首个改 runtime 的工作令）：域/数据层实现 BL-021.A2。行为：表示至多 2 组；确定性校验/顺序；legacy 单组数据等价；第三组拒绝；资格不变；无孔色；无宝石成长；无存档依赖。Gate：域级 EditMode 测试 PASS + 既有回归保持绿。
- **Phase 2 — Multiple-Link Runtime & UI Integration**：两组正确接入既有 runtime/loadout 与 UI 路径。行为：每组独立使用既有 Support 兼容入口；无跨组 Support 继承；非法组合在任一组仍非法；既有单连接行为零变化；UI 仅扩到能表示两组；不引入颜色可视化/匹配。兼容证据：golden oracle 继续权威；尽量在两个组位都跑既有兼容矩阵，证明组位不改变兼容语义。
- **Phase 3 — Bounded Affix Breadth**：精确新增 Phase 0 锁定的 N 条（N≤6）。每条：PoEDB 溯源；既有机制/stat 路径；现有 applicability truth；无新族语义；可经既有生成/制作路径获得；不产生死声明 stat/modifier。Gate：目录数=17+N；无额外未批准词缀；Content Audit fresh PASS；applicability audit PASS；有界生成/可达证据 PASS。
- **Phase 4 — Build & Interaction Validation**：验证两个变更带来有意义选择且未隐性扩 scope。RC-M/RC-P/RC-A 可作验证场景（不因此自动锁定为正式 Reference Builds）。覆盖至少：legacy 单连接；合法两连接；非法第三连接；两组不同合法 Support 组合；跨组隔离；新词缀在合法槽位生效/非法槽位拒绝；无任何需要孔颜色/宝石等级品质的交互。正式 Reference Build Coverage 仅在后续显式规划门锁定目标后才可晋级，否则保持候选/非权威。
- **Phase 5 — Production Closure**：全部 EditMode/PlayMode PASS（测试数变化逐项解释）；Content Audit PASS fresh；无死声明 stat/modifier；兼容 oracle parity PASS；新词缀可达 PASS；确定性 Production Simulation 可复现 PASS；既有锁定硬件 1440p/120 性能合同 PASS；canonical 性能路径 PASS；-IncludeArtPerformance PASS；production visual 分辨率健康；Drift=0；Forbidden Expansion Audit=PASS。
- **Determinism Rule**：S5 有意改变 gameplay/content 数据，S4 hash `FNV1A64:a1f075f251ec1070` 为前代参照而非 S5 期望输出。S5 Final Gate 必须建立新 canonical hash 并在 ≥3 次独立等价 Production Simulation 运行中精确一致。

## 7. Test & Evidence Standard

回归入口基线 EditMode 246/246、PlayMode 11/11；**S5 预期新增测试**——各门使用「全 PASS+精确计数+每个新增/删除测试逐项解释」；删除/削弱既有测试需显式理由与单独评审。
Multi-Link 证据必须演示：1 组 legacy parity；2 组合法构造；第三组确定性拒绝；资格不变；每组容量不变；兼容性组无关；跨组隔离；确定性组顺序；零孔色行为；零宝石等级/品质行为。
词缀证据必须包含：精确 N；旧数 17；新数 17+N；每条溯源；复用机制/stat 路径；applicability 结果；生成/制作可达；无死 stat 审计；无新族；无 Prefix/Suffix/Tier/ModGroup 行为。

## 8. Performance Policy

S5 改变 runtime loadout 行为与 UI 表示，最终性能复验必需。沿用现有 S4 锁定硬件 1440p/120 合同不变；不为 S5 发明新阈值。最终证据用与 S4 相同指标族报告 canonical 与 Art 路径以便直接对比；任何「技术上 PASS 但明显变差」的回归必须在 Evidence Pack 中披露。

## 9. Capability / Mechanic Accounting

预期最终 delta：Capability Ledger 中 Socket/Link 总体可保持 **PARTIAL**（孔色仍 unsupported），但其子能力状态必须显式记录：**Multiple Link Groups = SUPPORTED；Socket Color = UNSUPPORTED / NOT AUTHORIZED**。词缀广度不创建新 capability 族。Mechanic Support Matrix 必须独立表示：单连接 legacy 支持；多连接支持；孔色 unsupported；宝石等级/品质 unsupported；有界词缀广度（既有语义实现）。任何聚合行都不得掩盖导演对孔色/宝石成长的显式拒绝。

## 10. Markdown Synchronization

DECISIONS（更新：精确批准/拒绝原子；最多 2 连接组；词缀上限 +6；无隐性原子扩张）；RUNTIME（runtime 行为实际落地时更新：多连接 runtime 合同/legacy 行为/组隔离/当前词缀 applicability 行为；易变计数按治理指向审计/报告真相）；COMBAT_MATH（仅 verify，除非发现意外战斗数学依赖；不授权公式变更）；Capability Ledger（阶段落地时更新子能力证据）；Mechanic Matrix（更新多连接机制证据与有界词缀 linkage）；STATUS（执行期 LIMITED 跟踪更新；不提前标 Phase COMPLETE；正式阶段状态仍待最终 Director Gate）；ROADMAP（S5 以活跃已授权方向呈现，scope 恰=BL-002.A1+BL-021.A2，保留全部显式排除）。

## 11. Evidence Pack Rule Per Work Order

每张 S5 工作令回传：WO ID；revision/commit；changed files；精确测试与计数；Capability/Mechanic/Runtime/Canonical/Content/Authorization Delta（零写 NONE）；link-group cardinality before/after；affix count before/after（适用时）；drift；SoT 冲突；Forbidden Expansion audit；未决问题；建议下一 WO。

## 12. Final S5 Acceptance

呈交导演 Final Gate 仅当：只实现了 BL-002.A1 与 BL-021.A2；新增词缀 N≤6；总词缀 ≤23；每条新词缀 PoEDB 有据；无新词缀族；最大连接组=2；既有每组孔容量不变；既有槽位/物品连接资格不变；孔颜色缺席；宝石等级/品质缺席；legacy 单连接行为过回归；多连接组隔离；兼容真相单一；全部测试 PASS；Content Audit PASS；确定性精确重复 PASS；既有性能合同 PASS；Drift=0；Forbidden Expansion Audit=PASS；所需 Markdown 真相已同步。**正式 S5 阶段/状态完成只在导演 Final Gate 之后。**

---

# 附：S5-WO-01 — Approved Scope & Contract Lock（Phase 0）

**Cycle**: S5；**Phase**: Phase 0；**Type**: Primary Work Order；**Runtime Behavior Authority For This WO: NONE；Documentation/Analysis Authority: YES；Gameplay Implementation: NOT YET。**

**Objective**：在改 runtime 之前，为两个显式批准原子（BL-021.A2 多连接、BL-002.A1 有界词缀广度）产出实现就绪、可测试的合同，消除语义歧义；完成后下一工作令能够「不发明产品行为」地实现 Multi-Link Domain Core。

**In Scope**：
- **A. Multi-Link Contract Lock**：文档化 MaxLinkGroupsPerEligibleItem=2 权威合同；指认以下各项的当前真相源：连接物品资格；当前单连接表示；每组孔/容量规则；Support 兼容；连接顺序；runtime 消费路径；UI 表示路径。锁定行为：legacy 单连接→group 0；可选 group 1；最大组数=2；第三组拒绝；每组孔/容量沿用当前语义；资格不变；确定性组顺序；独立 Support 兼容评估；无跨组 Support/效果泄漏。显式声明 S5 不引入：孔颜色/颜色匹配/宝石等级/宝石品质/持久化/新增带孔槽位/新增技能与辅助。
- **B. Data-Model Decision**：选定**唯一**多连接组实现表示并文档化：权威数据所有者；组身份/顺序；合法基数；非法态处理；legacy 数据映射；Unity 序列化资产兼容性考量；runtime 读路径；UI 读路径。不得要求持久化/存档能力。本单不实现。
- **C. Multi-Link Test Matrix**：覆盖至少 12 场景：legacy 1 组；合法 2 组；非法 3 组；group 0 合法/group 1 合法；group 0 非法 Support 配对；group 1 非法 Support 配对；无跨组兼容泄漏；确定性顺序；物品/槽资格不变；每组容量不变；无孔色依赖；无等级/品质依赖。golden 兼容 oracle 适用的地方指认精确复用策略。
- **D. Affix Admission Manifest**：选精确 N（1≤N≤6）；恰好 N 条的权威候选清单；每条记录：稳定候选 ID；项目内命名；PoEDB canonical 溯源；复用的既有 stat/modifier 语义；适用既有装备槽；既有 applicability predicate/路径；既有生成/制作路径；确认无新词缀族；确认无 Prefix/Suffix 语义；确认无 Tier/ModGroup 语义。任一要求不满足的候选不得入 N。
- **E. Exact Scope Ledger**：一页实现 scope 台账：APPROVED={BL-021.A2, BL-002.A1}；EXPLICITLY REJECTED={BL-021.A1 孔色, BL-012.A1 宝石等级/品质}；NOT AUTHORIZED=其余全部现有原子。后续 S5 工作令必须引用本台账。

**Out of Scope**：无 `.cs` gameplay 行为实现；LinkGroup runtime 实现；UI 实现；词缀目录新增；prefab/scene 行为变更；资产内容变更；战斗数学变更；Support 兼容逻辑变更；PoEDB pipeline；依赖未实现 runtime 才能通过的新测试；Reference Build lock。可检视既有测试并文档化未来测试矩阵。

**Forbidden Expansion**：Socket Color；Gem Level/Quality；Prefix/Suffix；Tier/ModGroup；新词缀族；新 Skill；新 Support；新 EquipSlot；Ring；Offhand；Amulet；Unique；Boss；Progression；Endgame；Atlas；Voice；BL-024；Persistence；New Content Batch；Phase 12；prototype/spike 实现。

**Acceptance Criteria**：AC-01 导演 scope 对账（APPROVED=恰 BL-002.A1+BL-021.A2）；AC-02 连接基数锁定（唯一权威合同=MaxLinkGroupsPerEligibleItem=2，无文档暗示任意/无界组）；AC-03 资格冻结（连接资格规则指认并声明不变；不新增带连接槽位）；AC-04 每组语义冻结（既有单连接孔/容量合同指认；第二组复用该合同；不增加组内容量）；AC-05 legacy 映射锁定（既有单连接→group 0 语义零变化）；AC-06 隔离合同锁定（每组独立兼容评估；无跨组泄漏；组顺序确定性）；AC-07 显式拒绝护栏（合同与测试矩阵含护栏证明实现不要求孔色/宝石等级/宝石品质）；AC-08 数据模型选定（唯一实现表示；无未决等权威备选；tradeoff 可记录）；AC-09 测试矩阵完整（12 场景全表示；无验收条件依赖未指定行为）；AC-10 精确词缀数锁定（N 整数 1-6；计划目录数=17+N）；AC-11 每条候选可准入（溯源/语义路径/applicability/生成路径/无新族/无 PS/无 Tier-ModGroup；不合格候选移除或替换）；AC-12 无 BL-024 依赖（人工溯源即可）；AC-13 Runtime Delta=NONE（零 gameplay .cs/prefab/scene/资产行为变更；仅文档 helper 数据且不影响 runtime）；AC-14 基线保持绿（Quick Gate：EditMode/PlayMode 全 PASS、Content Audit fresh PASS、exit 0；计数若≠246/11 须逐项解释；Phase 0 正常应零测试数变化）；AC-15 Drift=0 且 SoT 冲突=0 且 Forbidden Audit=PASS。

**Markdown Sync**：DECISIONS（更新：批准/拒绝原子；最多 2 连接组；词缀上限 +6；Phase 0 选定精确 N）；RUNTIME（VERIFY ONLY；不得把 Multi-Link 描述为已实现；引用未来合同必须标注 planned/authorized-not-implemented）；COMBAT_MATH（VERIFY ONLY）；Capability Ledger（仅授权/跟踪更新；不标 Multiple Links 为 supported）；Mechanic Matrix（仅授权/跟踪更新；不晋升）；STATUS（LIMITED：S5 授权 scope/WO-01 issued/predecessor 导演决定；不在最终治理更新前标 Phase 0 COMPLETE）；ROADMAP（更新：S5 scope 恰=BL-002.A1+BL-021.A2，保留全部显式排除）。

**Recommended Documents**：`docs/reviews/S5/S5_WO_01.md`、`S5_LINK_CONTRACT.md`、`S5_AFFIX_ADMISSION.md`、`S5_WO_01_EVIDENCE.md`（link 合同与词缀清单权威位置唯一）。

**Non-Blocking Tasks**：NB-1 既有 Link 数据流审计（item/data→link 表示→compatibility→runtime→UI，纯文档）；NB-2 兼容 oracle 复用矩阵（golden 用例→group 0/1 执行映射）；NB-3 词缀溯源筛选（筛更多复用既有语义的 PoEDB 候选；只有最终锁定 N 入选）；NB-4 legacy 资产兼容清单（依赖单连接表示的资产/数据→group 0 映射；本单不迁移资产）；NB-5 禁区断言文档（color 字段/level-quality 字段/第三连接组/新槽资格/未批准词缀语义的捕获检查）。

**Evidence Pack 模板**：Work Order / Revision-Commit / Changed Markdown / Changed Runtime Files / Director Scope（Approved Atoms / Explicitly Rejected Atoms）/ Multi-Link（Chosen Data Model / Max Link Groups / Eligible Item-Slot Rule / Per-Group Capacity Rule / Legacy Mapping / Compatibility Authority / Cross-Group Isolation Rule / Third-Group Handling）/ Affix（Locked N / Entry Count Before / Planned Entry Count After / Candidate IDs / PoEDB Provenance Complete / New Semantic Families / Prefix-Suffix Introduced / Tier-ModGroup Introduced / BL-024 Required）/ Test Matrix Cases / Quick Gate（EditMode/PlayMode/Content Audit）/ 六 Delta（本单预期全 NONE；Authorization tracking 只反映已发行导演批准，本单自身不新增授权）/ Drift / SoT Conflicts / Forbidden Audit / Open Questions / Recommended Next WO。

**Expected Next Work Order**：S5-WO-01 获 ACCEPT 后，下一 Primary = **S5-WO-02 — Multiple-Link Domain Core**（首个授权改 runtime 的工作令，实现 BL-021.A2；不加词缀、除显式包含外不做 UI）。

---

**执行结论（规划 AI 同轮批复）**：Atom-level Gate Review=**PASS，S5 从 Phase 0 正式开始**。实现授权严格=BL-002.A1+BL-021.A2；孔色与宝石等级/品质作为负向验收条件贯穿全周期。工作 AI 可自动执行 S5-WO-01；完成后回传 Evidence，规划 AI 审合同严密性后决定是否放行 S5-WO-02。
