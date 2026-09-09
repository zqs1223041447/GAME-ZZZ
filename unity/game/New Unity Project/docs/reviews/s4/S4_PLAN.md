# S4_PLAN — Production Scale & Itemization Breadth

**来源**：协调席（GPT）S4 Master Plan（2026-09-09，经导演「开启 S4 工作 + 读取仓库 https://github.com/zqs1223041447/GAME-ZZZ 」指令下发）。工作 AI 将其正式写入 Repository-as-Memory，不得重新解释为别的 S4。
**基线**：main @ 7edbb6f（S3 = COMPLETE; Voice DEFERRED BY DIRECTOR）。

> **Milestone 状态回填（2026-09-09，不改目标定义）**：Phase 0/1/2/3/4 = COMPLETE；Phase 5（Integrated Closeout）= IN PROGRESS —— Gate 1/2/3 全 PASS + Simulation hash exact match + R7/R9 归档 OK，但 Gate 4/Art Gate=ENV_NOT_MET（机器活动显示=远程虚拟显示 1920×1080@144 ≠ 合同锁定 2560×1440；规则⑦ 不自动放宽，待导演恢复 1440p 或正式合同变更令）。总收口评审=`S4_OVERALL_CLOSEOUT_REVIEW.md`（Branch B）。S4=IN PROGRESS。

## 0. S4 定义

**S4 = Production Scale & Itemization Breadth**——不是 S3 再来一遍，也不是直接跳 Atlas/Boss/深 Craft。在不解锁 Stage0 大系统的前提下，把 S3 已经成立的 Vertical Slice 从「功能完整、表现完整、性能通过」推进到「可以安全扩大装备/词缀内容，并且由生产工具自动证明不会把项目写坏」。

### S4 核心原则

- **S4 不重做**：S3 Formal UI、S3 Formal Art、Combat Feel、Enemy visual pipeline、Performance infrastructure。
- **S4 重点**：Production Tooling / Equipment Breadth / Affix & Itemization Breadth / Deterministic Production Simulation / Integrated Performance & Closeout。

### 上位路线依据（仓库原始总计划）

首个完整 Vertical Slice 之后：**真实系统 Performance Gate → Production Tooling → 再做系统扩张**；系统扩张推荐从「装备槽 → 更多 Affix」开始。业务程序集继续只允许 Game.Runtime.Core / Game.Runtime.Content；随机只走 SeededRng。

## 1. Stage0 锁定边界（全部延续）

导演本次只说「开启 S4」，**没有逐条解禁**。以下继续锁，不得因「进入 S4」自动解锁（每项仍需导演单独明确解禁）：

- 大天赋树
- Unique library
- Atlas
- full/deep Craft
- DOTS
- HDRP
- FMOD
- character customization / 捏脸换装系统

另外：**Phase 4 Voice 仍 DEFERRED BY DIRECTOR**；S4 不自动恢复 Voice 工作，不修改 VoiceCues / voice 资源。

## 2. S4 额外 Out-of-Scope（非 Stage0 永久锁，但不属于本轮主线）

除非规划 AI 另立明确工作令，S4 默认不启动：Boss gameplay / 新 Boss taxonomy / Environment Art 大制作 / 新地图生成系统 / Aura-Reservation / Curse / Flask / Jewel / 新 Defense layer / 高级 Ailment / Advanced Trigger / Offhand-dual-wield / 双 Ring slot 语义 / 新技能机制引擎。

**Map Tier / Progression Spine 特意未放入 S4**：Stage0 仍锁 Atlas，导演未给 S4 新的 Endgame 指令；现在决定地图进程形态过早。等 S4 证明「内容能安全规模化」后，S5 再规划 Progression Spine。

## 3. Phase 结构

### Phase 0 — S4 Baseline / Production Readiness
建立 S4 Repository-as-Memory 入口，审计当前生产工具到底已经做到什么。输出：本文件 + current Catalog/count baseline + current tool inventory + Content Audit capability matrix + missing production-tooling gaps + Stage0 locks + Gate policy。完成标准：S4 plan 与当前 repo truth 一致；不复制历史 truth；明确哪些能力已有、哪些缺；当前全 Gate 绿状态。**S4 Phase 0 = COMPLETE**。

### Phase 1 — Production Tooling v1
内容规模增长以前，机器可以回答：项目有哪些内容、哪些引用有效、哪些组合非法、哪些资源缺失、哪些 gameplay Stat 没消费。**优先复用现有 Content Audit Collect → Render → Persist → Assert；不得平行造第二套 Catalog scanner**。最低输出：Production Content Report（machine-readable、确定性），至少包含 Skills/Supports/Affixes/Passives/EnemyKinds/MapMods/equipmentSlots 计数、referenced Stats、runtime-consumed Stats、unused Tags（信息项不 FAIL）、Support compatibility coverage、Passive graph connectivity、Resources required/gated/missing、formal enemy visual coverage、content validation verdict。Voice GATED=信息项不 FAIL。完成标准：single source collector / machine-readable / deterministic / missing-invalid 能 FAIL / current valid repo PASS / 零 gameplay-content delta。**S4 Phase 1 = COMPLETE — Production Tooling v1**。

### Phase 2 — Equipment Breadth v1
S3 正式装备槽：Weapon/Body/Helmet/Boots。S4 第一轮只扩两个低歧义槽：**Gloves、Belt**（4 → 6 formal slots）。不一次开完整 PoE 槽位（Offhand 双持/盾/双手语义、Ring1/Ring2 重复槽语义、Jewelry 后续独立扩）。必须复用：EquipSlot canonical definition、item instance、Affix system、Drop、Craft、Tooltip、Compare、Equipment drawer、Build snapshot。禁止：新装备 gameplay engine、GlovesSystem/BeltSystem、第二套 crafting、新 inventory backend。完成标准：Gloves/Belt 均能掉落/装备替换/比较/Craft/进 Build stats/UI/tooltip/build snapshot；旧四槽零回归。

### Phase 3 — Affix / Applicability Breadth v1
Phase 2 成立后扩词缀深度，**只用已有 Stat/ModOp/Modifier 语义**。最大范围新增 ≤4 个 Affix；优先让 Gloves/Belt 有有意义选择。允许（若架构确实需要）最小 data-driven slot applicability——只决定某 Affix 能出现在哪些 EquipSlot，不得决定伤害计算/技能行为。禁止：新 StatId、新 ModOp、新 Effect type、新 Trigger、Unique/Tier 大系统、deep Craft、prefix/suffix 大改造（除非 repo 已有这些数据字段）。完成标准：≤4 新词缀全部由已有机制组合；drop/craft 同一 canonical pool；incompatible slots 不 roll；SeededRng 确定性；Content Audit PASS；无 dead Stat。

### Phase 4 — Production Simulation / Scale Proof
不再加玩法。证明 S4 新装备/词缀架构可被机器批量生产而不产生非法状态。建立或**扩展现有** deterministic simulator（已有则扩展，禁止再建一套）。最低执行 10,000 seeded item/craft cycles；至少验证：deterministic replay / no NaN / no invalid slot / no incompatible affix / no stale second-value data / no impossible item / no invalid duplicate-conflict / craft result always valid / all generated affix Stats consumed by Runtime / fixed seed output reproducible。输出 machine-readable summary：seed/iterations/slot distribution/rarity distribution/affix distribution/invalid count/deterministic hash。完成标准：10,000 cycles invalid=0；同 seed 结果 hash 一致。

### Phase 5 — S4 Integrated Closeout
把 S4 所有新增内容放回完整游戏循环验证：6 equipment slots + item/craft/drop + UI + compare + Build + Content Audit + Production Report + Simulator + canonical gameplay + formal enemy visuals 全部共存。成功后 **S4 = COMPLETE**。

## 4. Milestone Matrix

| Milestone | Deliverable | Hard completion |
|---|---|---|
| M0 | S4 baseline + tooling gap audit | Repo truth frozen |
| M1 | Production Tooling v1 | Machine-readable deterministic report |
| M2 | Gloves + Belt | 6-slot full loop |
| M3 | ≤4 bounded Affixes | Existing mechanisms only |
| M4 | 10k production simulation | 0 invalid + deterministic hash |
| M5 | Integrated closeout | Four Gates + Art Gate PASS |

## 5. 四门 + Art Gate（S4 延续框架）

- **Gate 1 Static/Unit**：`.\tools\verify_unattended.ps1 -SelfTest` + EditMode PASS。重点：pure contracts / deterministic logic。
- **Gate 2 Integration/Content**：PlayMode PASS + Content Audit fresh PASS。任何 Content delta 都必须重新生成 Audit。
- **Gate 3 Build/Player**：`.\tools\verify_unattended.ps1 -IncludePlayerRun`。Win64 Build PASS + Player exit=0 + Runtime required resources PASS。
- **Gate 4 Canonical Performance**：`-IncludePerformance`。触发条件：gameplay hot path changed / equipment stat resolution changed / loot-craft runtime changed materially / phase closeout。Phase 0/1 纯 tooling 正常 N/A；Phase 2/3 如仅低频装备/城镇路径，可由 Reviewer 判断是否本轮必须跑；**S4 Closeout 强制跑**。Contract 继续锁当前 M7：2560×1440 / D3D12 / locked hardware / 8.33ms / 100-200-300 / existing warmup+samples / no workload cheating（`docs/qa/PERFORMANCE_GATE.json` 唯一真相源）。
- **Art Gate**：`-IncludeArtPerformance`。强制触发：formal visual set / renderer-material-Animator / ProjectSettings affecting rendering-skinning / new runtime art dependency 变化。S4 正常不改正式敌人美术，Phase 0–4 正常不要求；**S4 Closeout 强制跑一次 Art Gate regression**，确保 S4 没把 S3 Art closure 悄悄破坏。

### Gate Failure Policy
任何 hard Gate FAIL：不得放宽预算、改测试让它绿、删除合法 workload、跳过 invalid evidence。必须：停止该方向扩张；因果定位；修复；原 Gate 重跑。

## 6. S4 内容增长规则

S4 不是「大批量内容工厂」。**Phase 4 生产模拟通过前禁止**：大批技能 / 大批 Affix / 大批装备 base / 新怪批量 / 新地图批量。S4 首先证明：扩大内容不会破坏架构。

## 7. Voice

保持 **DEFERRED BY DIRECTOR**。S4 完成条件不要求 Voice；S4 不修改 VoiceCues / voice 资源。

## 8. S4 完成定义

以下全部成立才允许 **S4 = COMPLETE**：
Phase 0/1/2/3/4 全 COMPLETE；Gate 1–4 全 PASS；final Art Gate PASS；Stage0 locks preserved；Voice remains correctly deferred；no unresolved S4 hard blocker。

**完成 S4 后不自动启动 S5。**
