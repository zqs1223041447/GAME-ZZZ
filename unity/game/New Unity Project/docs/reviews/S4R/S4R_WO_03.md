# S4R_WO_03 — Next-Direction Decision Packet（Primary Work Order）

**来源**：规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09 下发（WO-02 Gate Review 随附）。工作 AI 于本轮正式落库并执行。**Cycle**: S4R；**Phase**: Phase 2 — Next-Direction Decision Packet；**Work Order Type**: Primary；**Runtime Authority: NONE；Gameplay Implementation Authority: NONE；Canonical Data Expansion Authority: NONE。**

## 前置：WO-02 Gate Review 结论（规划 AI 2026-09-09）

**ACCEPT WITH FOLLOW-UP → S4R-WO-03**（非返工）。13 项 PASS（Candidate Backlog 32 闭合 / 分类 32-32 / 授权 32-32 / 来源覆盖 / 依赖图 DAG 0 环 / RBC 3-3 / RBC unsupported=NONE / Ledger 18-4-5-1=28 / 六 delta=NONE / 新授权=0 / Drift=0 / SoT=0 / Forbidden=PASS）；Deduplication=PASS WITH FOLLOW-UP。
**Follow-up：Authorization Atom Granularity**——导演决策不能以聚合 Backlog 行为授权粒度：BL-003（大树/Mastery/Ascendancy 分别可授权）、BL-016（Monster Package/Boss/Special Encounter）、BL-001（slot breadth/Weapon family/Handedness/Requirements）、BL-013（九种 craft 操作）、Gem Corruption（BL-012 域）与 Item Corruption（BL-013 域）保持领域区分；无需重拆 32 行，WO-03 建立 Atoms 即可。
**裁定①**：BL-024 双门——导演批准方向 → 规划 AI 下一周期计划显式纳入 → 下发明确 Primary Work Order → 才获 implementation authority；S4R 内维持 FORBIDDEN_UNTIL_APPROVED / authority NONE。
**裁定②**：RBC 细节继续 NOT LOCKED；正式锁定发生在导演选定方向后的新 implementation cycle。

## 1. Objective

基于已闭合的：S4 Frozen Baseline；Capability Ledger；Mechanic Support Matrix；32-item normalized backlog；dependency DAG；3 个 Candidate Reference Builds；Director-Gated/Forbidden inventory——生成一个足够让 Director **明确选择下一产品方向和明确授权 capability delta** 的 Decision Packet。终点是「导演已经能做决定」，不是「工作 AI 替导演做决定」。本单不得启动任何候选方向的 runtime implementation。

## 2. Mandatory Follow-up — Authorization Atom Audit

为聚合 Backlog 行建立 `BL-xxx.A1/A2/...` 授权原子（仅决策粒度，不改 32 行 authoritative ID）。至少审计 BL-001（slot breadth/Weapon family/Handedness/Requirements）、BL-003（大树/Mastery/Ascendancy）、BL-012（Level-Quality/Corruption/Ascension/Alternate Form/Final Upgrade）、BL-013（Add/Remove/Reroll/Lock/Targeted Reroll/Upgrade Tier/Special/Final/Item Corruption）、BL-016（Monster Package/Boss/Special Encounter）；其它聚合行同规则补建。验收：不存在「批准宽泛 ID 即隐式批准多个独立 gated 机制」。

## 3. 五方向（均 CANDIDATE — NOT APPROVED）

- **DIR-0 Consolidation & Production Quality**：不加主要 gameplay capability，强化 S0-S4 完成度/可靠性/合同/UX。输入 BL-030/BL-032/TECH_QUALITY 项；导演门控 UI/路线项仅 optional atom。最低 blast radius；不以新 progression/endgame/Content Batch 为成功条件。是「继续冻结 feature surface」的正式产品选项，非 fallback。
- **DIR-1 Reuse-First Build & Itemization Depth**：优先复用 3 Skills/7 Supports/passive 基础/6 槽/17 词缀/现有 Combat Math 增加 build differentiation，暂不进 Endgame Spine。候选族 BL-002/BL-021/BL-022 及可复用项。**依赖路径最短的默认比较基准**（Planner comparison prior，非批准非授权）。
- **DIR-2 Core Build-System Expansion**：增加新 build-system capability（Passive depth atoms/Aura-Reservation/Additional Defense/Additional Ailment/Advanced Trigger/装备-武器系统 atoms）。必须区分：现有机制 depth vs 真新 capability vs 需 BL-024 vs 本就 Director-gated；不得以 Phase 8 顺序代替 Director approval。
- **DIR-3 Crafting Depth**：把 bounded Craft 扩展为系统性 item-construction。依据 BL-013/BL-014。必须展示：现有 Craft 已支持什么/新增什么/各 atom/Craft Sim 为何属后续验证基建/何时需要 100k sim gate。Phase 10 存在≠优先级已定。
- **DIR-4 Progression & Endgame Spine**：BL-015/BL-016 atoms/BL-017/BL-019 及真实 prerequisites。**Mandatory Warning：CURRENTLY DIRECTOR-GATED / NO IMPLEMENTATION AUTHORITY**。必须说明：Map Tier/Progression=新产品能力；Boss 不因 Elite 存在视为已支持；Atlas 不因现有 Map System 视为小增量；Map Risk Sim=downstream 基建；Phase 11 位置≠已批准立即实施。

## 4. Explicitly Downstream（不得包装成当前可启动）

**Phase 12 Content Factory（BL-018/new Content Batch family）= downstream destination**：长期规划要求核心机制稳定后才大规模生产，而当前仍有多组 Partial/Unsupported/Director-Gated。不得作为 WO-03 默认推荐方向；导演可显式 override 覆盖长期规划，但必须显式。

## 5. Cross-Cutting Director Toggles（独立呈现，不因选向隐式决定）

Voice（DEFERRED/BLOCKED——导演须明确 keep deferred 或解除试听门；不能默认 release）；BL-024 PoEDB Pipeline（当前 forbidden；说明哪些方向需要/不需要；选向≠给 BL-024 工作令）；New Content Batch（独立 YES/NO，默认 NO）；Ring/Offhand/Amulet（独立 atoms，不得借「Equipment Breadth」隐式解禁）；Technical Route（DOTS/HDRP/FMOD/捏脸继续独立 gate，不混入 scope）。

## 6. Direction Card 字段（25 项，每 DIR 必答）

Player value；Why now / why not now；Included backlog IDs；Included authorization atoms；Explicitly excluded items；Existing Supported capability reuse；New capabilities required；Required Director gates；Immediate prerequisites；Blocking prerequisites；Downstream capabilities enabled；Canonical-data dependency；BL-024 dependency；Art dependency；Audio dependency；UI dependency；Test/evidence burden；Performance risk；Determinism risk；Save/migration risk；Runtime blast radius；Reference Build impact；Compatibility with long-term Phase ordering；Definition of Done shape；What becomes possible afterward。风险用 LOW/MEDIUM/HIGH/N-A 且附一行事实理由，禁止无依据数字化评分。

## 7. Cross-Direction Comparison Matrix

一屏比较 DIR-0~4，至少含：Player-facing value axis；Breadth vs Depth；Existing capability reuse；新 capability 族数量/类型；Dependency depth；Director gates required；BL-024 required?；Endgame opening?；Content Batch required?；Save-data implications；Performance risk；Determinism risk；Test burden；Art/audio burden；Reference Build relevance；Major irreversible/expensive choice；Earliest blocker。禁止无证据的天数/周数/story-point 估算。

## 8. Reference Build Treatment

RC-M/RC-P/RC-A 继续 CANDIDATE/NOT LOCKED。WO-03 可回答：哪个方向能继续用三候选；哪个方向要求未来新增 candidate；哪个方向使当前 coverage 不再充分。不得：锁定 Concentrated/Combustion 取舍；锁定最终 Support loadout；创建 DPS gate；创建 boss/map-tier clear target；把正式 Reference Build Coverage 从 N/A 晋级。

## 9. Planner Recommendation Section

只允许相对建议，不产生授权。默认比较顺序：1. DIR-1 → 2. DIR-2 → 3. DIR-3 → 4. DIR-4；DIR-0 作为独立 Consolidation alternative 参与比较（非「失败才做」的 fallback）。DIR-1 居首依据只能写：复用现有 supported surface 较多；依赖路径相对较短；不要求先打开正式 endgame；可直接利用当前 3 个 Candidate Reference Builds。不得写成「Director 已选择 DIR-1」。

## 10. Director Decision Form + Default-on-Omission Rule

表单见 `S4R_DIRECTION_DECISION_PACKET.md` §6（原样可回传：Selected Direction[DIR-0..4/CUSTOM] / Approved·Deferred·Forbidden Atoms / Progression / Endgame / Boss / Unique / Ring / Offhand / Amulet / New Content Batch / Voice[KEEP DEFERRED|RESUME GATE] / PoEDB Pipeline[ELIGIBLE FOR NEXT-CYCLE PLANNING|KEEP FORBIDDEN] / Primary Success Axis / Breadth vs Depth / Additional Constraints）。**Default-on-Omission Rule：导演未明确批准的 capability/atom 保持原 NOT_AUTHORIZED/DEFERRED/BLOCKED/FORBIDDEN 状态；禁止从方向名称推导遗漏授权。**

## 11. Out of Scope

修改 runtime gameplay；改 scene/prefab/asset；新增 canonical content；做 PoEDB Pipeline；改 combat math；balance tuning；锁 Reference Builds；实现 Prototype；feature spike；开 S5；创建正式新周期 implementation tasks；开始任何 DIR-x 的 Phase 0。

## 12. Forbidden Expansion

完整继承（S5/Progression Spine/Map Tier/Boss/Unique/Ring/Offhand/Amulet/New Content Batch/Voice activation/Curse/Flask/Jewel/Atlas/New Class/Deep Craft/unapproved Endgame/new EquipSlot/new Skill/new Support/new Affix family/new currency/Aura-Reservation implementation/Mastery/Ascendancy/Persistence implementation/Weapon-Handedness-Requirement implementation/PoEDB bulk pipeline/Craft Simulator implementation/Map Risk Simulator implementation）——可分析可候选，不可实现。

## 13. Acceptance Criteria

AC-01 五卡完整统一字段；AC-02 原子审计 PASS（无隐式解禁）；AC-03 无隐藏批准（全 CANDIDATE/NOT APPROVED，新授权=0）；AC-04 对比矩阵完整（导演无需自查 32 行）；AC-05 依赖真相保留（DAG≠schedule；长期顺序≠commitment；downstream≠ready root）；AC-06 Phase 12 正确 downstream 处理；AC-07 BL-024 双门保留；AC-08 RBC 保持候选、正式 Coverage 不晋级；AC-09 决策表无歧义（填表即可生成 implementation-cycle seed）；AC-10 六 delta=NONE；AC-11 Drift=0/逐项报告 + SoT=0/逐项报告；AC-12 Forbidden Audit PASS。

## 14. Markdown Sync

DECISIONS（UPDATE REQUIRED：Atom 语义/方向候选≠导演批准/default-on-omission/BL-024 双门；不得登记导演未做的决定）；RUNTIME/COMBAT_MATH（VERIFY ONLY：expected verified; no change required）；Ledger（UPDATE ONLY IF NEEDED FOR ATOM LINKAGE，不得升降级）；Matrix（UPDATE ONLY IF NEEDED，正式 Coverage 不变）；STATUS（LIMITED：WO-02 Review=ACCEPT WITH FOLLOW-UP/WO-03 issued/Decision Packet awaiting Director Gate；禁把 Phase 2 标 COMPLETE）；ROADMAP（UPDATE REQUIRED：链接 Decision Packet；DIR-0~4=alternatives；下一正式周期=Awaiting Director Selection；禁把任何 DIR 写成 committed roadmap）。

## 15. 交付物

`docs/reviews/S4R/S4R_WO_03.md`、`S4R_DIRECTION_DECISION_PACKET.md`、`S4R_AUTHORIZATION_ATOMS.md`（atoms 权威位置唯一）、`S4R_WO_03_EVIDENCE.md`。

## 16. Non-Blocking Tasks

Atom Audit / DIR→Backlog traceability / Dependency-path condensation / Cross-direction risk matrix / RBC→DIR relevance map / Director Decision Form / Phase 8-12→Direction traceability；无法判断的产品价值写 `DIRECTOR INPUT REQUIRED`，不得自行创造产品需求。

## 17. Evidence 回传 + 循环规则

按模板回传（含 Atoms Total / Bundled Backlogs Audited / Hidden-Approval Findings·Resolved / 五卡完成度 / 比较顺序 / Backlog 覆盖：Covered-By-Directions·Explicitly-Downstream·Not-Represented·理由 / RBC 状态 / BL-024·Voice·Content-Factory 三门保留 / 六 delta / Drift / SoT / Forbidden / Form Ready / Open Questions / Recommended Next Step）+ 三摘要（Comparison Matrix / Atom Summary / Director Decision Form）。
**本单完成后不自动启动 WO-04 或任何 implementation work。WO-03 通过后：若导演已回填 Decision Form → 规划 AI 做 Direction Gate Review 并生成下一正式 implementation-cycle seed；若导演未输入 → 保持 S4R runtime freeze，不以「规划已完成」为由自行启动任何候选系统。**
