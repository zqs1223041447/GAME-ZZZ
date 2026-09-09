# STATUS（门状态一页纸）

日期：2026-09-09。本轮提交：见下方「本轮 commit」行（由紧随的回填提交写入，格式 A=STATUS 主体 / B=回填）。

本轮 commit（S4R-WO-02-BACKLOG-NORMALIZATION）：**S4R Phase 1 候选池正规化（纯文档轮，六项 delta=0、New gameplay authorizations=0）**。WO-01 Gate Review=**ACCEPT**（规划 AI 12 项全 PASS；非阻塞 follow-up=Ledger 计数口径，已并入本单）。本单：WO-02 工作令落库（`S4R_WO_02.md`）；**Backlog 正规化**（`S4R_BACKLOG_NORMALIZATION.md`：候选池 32 项——BREADTH 3/DEPTH 9/CONTENT 1/TECH 4/DIRECTOR_GATED 13/FORBIDDEN_UNTIL_APPROVED 1/DEPENDENCY_BLOCKED 1，AUTHORIZED=0，依赖图 DAG 无环，Phase 8→12 顺序仅作依赖语义非执行承诺）；**Reference Build 候选信封**（`S4R_REFERENCE_BUILD_CANDIDATES.md`：RC-M/RC-P/RC-A 覆盖全部 3 canonical Skill，全 Supported mechanics、组合对 golden oracle 合法、unsupported requirement=NONE、CANDIDATE/NOT LOCKED）；**Ledger 计数对账**（计数单位=总览行；Supported 18/Partial 4/Unsupported 5/Blocked 1=28；WO-01 摘要「14」=统计笔误，不改行状态）+ Matrix Backlog linkage 与非权威 Candidate Coverage Mapping；DECISIONS 登记 Candidate≠Approved/RBC≠Locked Target/依赖顺序≠产品承诺；**Candidate≠Approved——候选与依赖先行关系均不构成实施授权**；S4 硬停止与 Voice DEFERRED 原样保留；下一实施周期=Director-Gated。（本轮提交：A=3609515 主体 / B=本行所在回填提交）

## 门状态

| 门 | 状态 | 关键 commit / 说明 |
|---|---|---|
| S0 能跑 | 放行 | 2ee479d / d5beb7f——工程可开、测试可跑、坏数据失败、Seed 可复现 |
| S1 技术 | 放行 | 点地移动、三技能、对象池、密度可测 |
| S1 手感 | 放行（导演复测 2026-09-07 关门） | 手感门已关闭 |
| S2 微循环技术 | 已完成（导演 2026-09-06 确认） | 10-20 分钟完整两轮循环 |
| S2 UI | 简易可读（IMGUI SliceHud）；HUD 未完备已登记 | 正式 UI 挂导演门控表，无输入则不做 |
| S2 换模碰撞 | 技术放行 | f22862e / 239142e 换模；ebbf782 贴地修正；碰撞规则仍在 |
| S2 外观「小于敌人」 | 已修正 | ebbf782——身体净高 ≈1.19，同框可辨 |
| S2P 1080p | 已证 | b152610 独立包：p99 最高 2.508ms（预算 8.33ms） |
| S2P 1440p@120 | **已证 / CLOSED（M7 locked hardware）** | 2026-09-08：AMD Ryzen 7 5700X3D / RTX 5070、真实 2560×1440、D3D12、PC Quality 通过 canonical 120FPS Gate（worst p99=2.388ms / 预算 8.33）；证据 `docs/reviews/s2p/1440p-120-m7/`；结论仅适用锁定硬件，其它机器需按 contract 显式更新 |
| S3 最小底座 | 已开工 | c2689be ART_BIBLE+内容校验；8f2de35 预留 Tag 钉死 |
| S3 内容扩张 | **S3=COMPLETE（Voice=DEFERRED BY DIRECTOR）** | Phase 1/2/3(UI)/5(Art)=COMPLETE；Phase 4 人声=导演延期排除出当前完成范围；总收口 `S3_OVERALL_CLOSEOUT_REVIEW.md`（PASS）；Stage0 全锁延续 |
| S4 Production Scale & Itemization Breadth | **COMPLETE（2026-09-09）** | 规划=`docs/reviews/s4/S4_PLAN.md`；Phase 0/1/2/3/4/5 全 COMPLETE；总收口=`docs/reviews/s4/S4_OVERALL_CLOSEOUT_REVIEW.md`（§11b Recertification：ENV blocker 经系统正常模式切换解除；Final Gate 双 PASS——Canonical worst p99=4.840ms=58.1% / Art worst avg=4.085ms=49.0%、resolvedVisuals=4/fallback=0/clones=0）；Simulation hash exact match=FNV1A64:a1f075f251ec1070；**硬停止：不启动 S5/新内容，等待导演/协调席下一产品方向** |
| S4R Direction Readiness & Baseline Lock | **IN PROGRESS（2026-09-09，规划 AI 下发）** | **inter-cycle 治理周期：无 gameplay capability 变更授权，Runtime feature surface 冻结**；计划=`docs/reviews/S4R/S4R_PLAN.md`（Phase 0 对账→Backlog 分类→导演决策包→**Director Direction Gate**→下一周期 seed）；WO-01=Frozen Baseline & Governance Reconciliation（**Gate Review=ACCEPT**；Ledger/Matrix/SoT Index 首次正式建立）；WO-02=Backlog & Dependency Normalization（候选池 32 项+依赖图+RBC 候选 3 例=CANDIDATE/NOT LOCKED；**候选≠承诺**）；**硬停止延续，下一实施周期=Director-Gated** |

## 玩家视图

| 项 | 值 |
|---|---|
| 模型 | DarkKnight（BDO 包，预制体缩放 0.24，Humanoid 重定向） |
| 贴地后身高 | ≈1.19（胶囊碰撞体 1.16 = `DarkKnightView.TargetHeight`；武器长刀不参与包围盒） |
| 动画 | 六态 Idle / Run / Attack / Cast / Hit / Death——Hit=受击 HitFlash 上跳沿（可被下一击打断重播）；Death=进入 `MapState.Dead` 播完冻结末帧（843011c） |
| 相机 | 偏移 (0,10.6,-9.3)、注视斜距 14.1、注视 y 0.6、FOV 42（76381c0） |
| 动画源 | 401 库两条：def_shield_dam（Hit 0.63s）/ def_shield_break 裁跪段（Death 0.68s）；截图 07_hit / 08_death / 09_camera |

## 碰撞与命中

- 分层穿过（IgnoreLayerCollision Player×Monster、Monster×Monster）+ `DummyCrowd.Separate(1)`；命中=距离 / 圈 / 弹道 + `ResolveHit`（COMBAT_MATH 公式未动）。

## 测量

- `ArenaPerfHarness` 默认关；正式游玩路径不得自动开启。1080p 独立包三档 p99 最高 2.508ms（历史证据保留，`docs/reviews/s2p/`）。**1440p/120 已于 M7 在锁定硬件下 CLOSED（2026-09-08）**：真实 2560×1440 / fullscreen / D3D12 / PC / vSync=0 / targetFps=-1，2 个 canonical Gate ×3 Run ×3 Density=18 测量全 PASS，worst p99=2.388ms=预算 8.33 的 28.7%；证据 `docs/reviews/s2p/1440p-120-m7/`。结论只适用锁定 M7 硬件（Ryzen 7 5700X3D / RTX 5070）+ canonical 契约，**不代表所有 Windows PC 保证 120FPS**；其它机器须按 `docs/qa/PERFORMANCE_GATE.json` 显式更新并重建 baseline。**R7 新增第五层 Formal Art Performance Gate（`-IncludeArtPerformance`）**：正式敌人渲染/动画成本由独立 Art Gate 覆盖（canonical 仍 Dummy baseline），双 Gate 同 HEAD 全 PASS（300 正式 visual stress worst p99=7.628ms=91.6%），证据 `docs/reviews/s3/art-performance-r7/`。

## 音频

- 当前事实（收口审计 `CONTENT_AUDIT_S3_CLOSEOUT.md` 资源契约为准）：**战斗 SFX 5 键已投放全 CC0（Cast/Impact/Hit/Death/Loot，REQUIRED 6/6 真实加载 PASS，含玩家预制体）**；**人声 VoiceCues 3 键（Cast/Hit/Death）映射仍 GATED（0/3 present，缺失=静音，待导演试听指认）**。单一入口 `AudioEvents.Play` + 独立 `VoiceCues`，无中间件；缺资产=静音+限频日志（降级模式）。路径拼接单一真相源 `RuntimeResourcePaths`（S3-M2）：审计期望键集与 REQUIRED/GATED 分类保持独立 oracle，declared-key parity 防未审批增删资源入口。

## 校验

- EditMode 246/246、PlayMode 11/11（canonical Gate `.\tools\verify_unattended.ps1`；M5 维护轮 +2；Phase 3 UI 三轮 +23；Phase 5 Art R1 +10/R2 +10/R3 +13/R4 +8/R5 +10/R6 +8；R7 art 工具 SelfTest 层；R9 AshlingVisualContractTests +8 wired 断言——R8 集成态版本历史可溯；S4-P0P1 +12：ProductionContentReportTests 契约；S4-P2 +11 EditMode（SixSlotEquipmentTests 9+SliceDrawerTests 六槽契约）+1 PlayMode（六槽 loop）；S4-P3 +10 EditMode（AffixApplicabilityTests）+1 PlayMode（词缀 applicability session loop）；**S4-P4 +4（ProductionSimulatorTests 10k/同 seed/异 seed+duplicate reject 回归）**）。报告 **`CONTENT_AUDIT_S3_CLOSEOUT.md`**（测试再生；BATCH1/R2 保留历史）：**顶部 Verdict 块=Audit completed: YES / Verdict: PASS / Failure count: 0**；StatId/ModOp/运行期未知 Stat=0；非法 Tag=0；Effect-Event=0；链接=0；词缀行=0；兼容矩阵=0（3×7 parity 21 组合）；**Skill Tag golden parity=3/3**；**死 Tagged Modifier=0**（4 条全可达）；**结构/契约问题=0**；**资源契约=REQUIRED 8/8 真实加载 PASS（玩家预制体+5 SFX+TrollWarriorVisual+FireLionVisual）、GATED 人声 3 键缺失如实记录、VFX 声明引用 0=N/A**。负向 Tag 规则测试通过。阶段 1/2 收口=`S3_PHASE2_CLOSEOUT.md`（Phase 1/2=COMPLETE）；M1 事实收敛（`S3_M1_REPO_TRUTH_REVIEW.md` PASS）；M2 资源路径单一真相源（`S3_M2_RESOURCE_CONTRACT_REVIEW.md` PASS）；M3 failure-safe 审计（`S3_M3_AUDIT_FAILSAFE_REVIEW.md` PASS）；M4 无人值守 Quick Gate（`S3_M4_UNATTENDED_GATE_REVIEW.md` PASS）；**M5 Full Integration Gate=Quick+win64 Player Build（`.\tools\verify_unattended.ps1 -IncludeBuild`；`-batchmode -quit -buildTarget win64 -buildWindows64Player` 实测；双证据判定；6 次运行全 PASS，exe 667136 bytes/Data 162 文件；Build Settings 场景契约锁定 Bootstrap=0/Arena=1；设置资产归一化收口+define 振荡根因修复 `submitAnalytics: 0`），复核=`S3_M5_WIN64_BUILD_GATE_REVIEW.md`（PASS）**；**M6 Player Runtime Gate=`-IncludePlayerRun`（隐含 Build；本轮刚构建 Player 以 `-arenaPerf` 启动三档跑完自退 exit=0；证据 QA golden 100/200/300+editor=False+实测分辨率一致+13 列可解析；无性能阈值恒输出 `PerformanceVerdict: NOT_EVALUATED`；Harness 证据标题中性化+删 enteredMap 假字段；实测分辨率 2560x1440/DX12 仅记录——1440p/120 维持未结案、性能未判定（M6 当时状态；后由 M7 CLOSED）），复核=`S3_M6_PLAYER_RUNTIME_TRUTH_REVIEW.md`（PASS）**；**M7 Performance Gate=`-IncludePerformance`（contract=`docs/qa/PERFORMANCE_GATE.json` 锁 2560×1440/D3D12/PC/vSync0/targetFps-1/CPU+GPU；同一 Build 连续 3 次 Player 锁定环境运行；硬指标 main avg+p99≤8.33+cpu/gpu≤8.33+FrameTiming 必须+alive≥95%+frames=600；**2 个 canonical Gate ×3 Run×3 Density=18 测量全 PASS，worst p99=2.388ms=预算 28.7%→S2P 1440p/120 CLOSED（锁定硬件）**，冻结证据 `docs/reviews/s2p/1440p-120-m7/`；预算语义=`docs/qa/PERFORMANCE_BUDGET.md`），复核=`S3_M7_1440P_PERFORMANCE_GATE_REVIEW.md`（PASS）**；**M8 Evidence Integrity Gate（工作令 S3-M8-PERF-EVIDENCE-INTEGRITY）：新增显式快照 Operator `tools/snapshot_performance_evidence.ps1`（Verifier≠Operator 职责分离=DECISIONS 规则⑨；拒非 PASS/不完整/契约不匹配/脏树/已存在目标；MANIFEST 逐文件 SHA-256；-VerifyArchive 防篡改），M7「Gate A 只有抄表」证据不对称以**未来可重复链**修复=Gate PASS→立即快照→下一 Gate→VerifyArchive；**M8 provenance-complete revalidation=PASS**（Evidence HEAD=a11aa87，两 canonical Gate 各 9/9 PASS 且同 HEAD；18/18 预算内 worst p99=4.380ms=52.6%；gate-a/gate-b 原始证据各 14 文件冻结+MANIFEST 校验双 OK；ProjectSettings define 振荡矩阵实测=Quick 门移除/Performance·Build 门回写，Evidence HEAD 取 Performance 门终态，Gate A 首尝试因脏树被快照拒收而作废未冻结——如实记录），复核=`S3_M8_PERF_EVIDENCE_INTEGRITY_REVIEW.md`（PASS）**；**M9 Inference Dependency Closeout（工作令 S3-M9-INFERENCE-DEPENDENCY-CLOSEOUT，维护序列最终根因项）：零使用审计全过后移除 com.unity.ai.inference 直接依赖（lock UPM 解析 -Inference/-dt.app-ui；burst/collections 由 URP 保活），ProjectSettings canonical 一次性删 SENTIS define（根因=Inference 包 AnalyticsDefineManager 跨实例加删）；**五门矩阵 Quick/Build/Quick-after-Build/PlayerRuntime/Performance 全 PASS+每步树净+define 0 变动=振荡 CLOSED**；Performance 回归 9/9 PASS（worst p99=2.376ms=28.5%，contract/workload 零 diff）；DECISIONS 规则⑩（Package removal 须先审计）；Content delta=0；**维护序列正式结束 GATE-READY**），复核=`S3_M9_INFERENCE_DEPENDENCY_REVIEW.md`（PASS）**。未使用 Tag 4 个（Spell/Projectile/Fire/Duration）=预留，不是任务。**S4-P0P1 新增 Production Content Report**：`docs/qa/CONTENT_PRODUCTION_REPORT.json`（EditMode `ProductionContentReportTests` 再生；schema=CONTENT_PRODUCTION_REPORT_V1；当前=verdict PASS、counts 3/7/17/16/5/3/6（S4-P3 起 affixes=17）、unconsumedDeclaredStats=0、affixApplicability unrestricted=13/restricted=4/bySlot 13×4+15+15、Support 组合 21 中 16 兼容 parity OK、Passive 16 节点 20 边 0 断连、REQUIRED 10/10、GATED 人声 0/3 如实记录、formal visuals 4/4）。

## 导演门控待输入

- 表在 `docs/ROADMAP.md`「导演门控待输入」节（7 项，每项「无输入则不做」，不得当自动任务开工）。
- **S3 已开工（历史「开 S3」口令已于 2026-09-07 消费）**：Phase 1/2=COMPLETE；**Phase 3 Formal UI=COMPLETE（R1 底栏双球/R2 右侧装备抽屉/R3 统一 Tooltip/R4 Passive 有界重排=Branch A COMPLETE（bounded existing-passive presentation only）；Phase 3 UI Closeout Audit=PASS；Phase 3 UI R4 Review=`docs/reviews/s3/S3_PHASE3_UI_R4_PASSIVE_REVIEW.md`）**；Phase 4 人声=「本阶段暂不搞」GATED；**Phase 5 精模=进行中 · Art Trial R8（授权已由导演解决不再作为 Gate=DECISIONS 规则⑪；导演 2026-09-08 核心规则追加=无人值守自动执行 GPT 下发的工作安排；R1 Bruce=Role-Specific 候选；R2 巨魔 Troll_2=**默认视觉锚点（规则⑫）**；R3=Troll 正式接入 Brute 视觉位=INTEGRATED；R4=Lion Head 接入 Stinger 视觉位=INTEGRATED；R5=Hit/Ignite 反馈统一=Formal enemy visual feedback parity established（规则⑬）；R6=Warden→Bruce 第三正式视觉=INTEGRATED；R7=Formal Art Density Performance Gate=ESTABLISHED（规则⑭，双 Gate 同 HEAD 全 PASS，300 正式 visual stress worst p99=7.628ms=91.6%）；**R8=Ashling 定向选模：SELECTED=SFB Gargoyle（石魔，资产已验收保留于 Art/Enemies/Gargoyle/）但 Formal Art Performance Gate 四视觉 mix 实测 FAIL（200/300 档超预算）→ 按工作令恢复 Ashling=placeholder、不降质修绿=ASSET ACCEPTED — ASHLING RUNTIME INTEGRATION BLOCKED BY ART PERFORMANCE**；**R9=根因隔离（gpuSkinning=0 全项目 CPU 蒙皮）+无损优化（gpuSkinning=1+optimizeGameObjects+scale 曲线剥离）→双 Gate 同 HEAD=6572fbd 全 PASS（四视觉 mix worst p99=6.168ms=74.0%）→ Ashling→GargoyleVisual 正式接入=PASS — GARGOYLE OPTIMIZED AND ASHLING FORMAL VISUAL INTEGRATED——all current non-benchmark EnemyKinds have formal visuals（规则⑯）；R10 Closeout Audit=PASS — PHASE 5 ART COMPLETE（四非 benchmark EnemyKind 全正式视觉/共享管线零特例/反馈一致/MPB 零 clone/契约 10/10/四门全 PASS/R7·R9 归档校验 OK/导演四项真相保留；Death 截断=non-blocking known limitation）**；导演同日四项指示随 R6 入库；狼=真实 mismatch 否决存档、蝙蝠×2 关闭**）；**新一批内容（含第 8 Support/新词缀/新怪）不得自行启动**——需规划 AI/导演新立工作令；规划见 `docs/reviews/s3/S3_PLAN.md`（S3=COMPLETE; Voice DEFERRED BY DIRECTOR（Phase 4 导演延期排除出当前阶段完成范围；S3 Phase 1/2/3/5 全 COMPLETE））。
- **S4 = COMPLETE（2026-09-09，导演 2026-09-09「开启 S4」→协调席 S4 Master Plan→Phase 0-5 全 COMPLETE；无人值守自动执行）**：S4=Production Scale & Itemization Breadth；收口=6 EquipSlots canonical（Gloves/Belt）/17 Affixes canonical（4 条 Gloves/Belt 专属）/单一 applicability truth/Production Report PASS/10k Simulation PASS（hash exact match）/四门+Art Gate 全 PASS（Recertification：活动显示恢复 2560×1440 后 `-IncludeArtPerformance` 双 PASS）；**完成后硬停止（§四十九）：不启动 S5/Progression Spine/Map Tier/Boss/Unique/Ring/Offhand/Amulet/新 Content Batch/Voice——等待导演/协调席下一产品方向**；Stage0 锁全部延续；Voice 继续 DEFERRED；规划=`docs/reviews/s4/S4_PLAN.md`。

## 已对齐（本轮只改文档）

- `docs/RUNTIME.md` S2 节装备陈旧事实对齐（S4R-WO-01）：4 槽/「当前 13 条」/抽屉 2×2 四槽 → 6 槽 canonical EquipSlot/易变计数改为指向最新 Content Audit·Production Report 快照（S3-M1 规则）/抽屉 2×3 六槽（展示顺序 Weapon/Helmet/Body/Gloves/Boots/Belt）——S4-P2/P3 改变事实后 RUNTIME 未同步的文档漂移；只改文档，Runtime 零改动。
- `docs/art/ART_BIBLE.md` 相机行 (0,17,-15)→(0,10.6,-9.3)：76381c0 改跟拍后 ART_BIBLE 未同步——本页轮对齐。
- 1.16 / 1.19 并存确认**非矛盾**：1.16=胶囊碰撞体高度（`DarkKnightView.TargetHeight`），1.19=贴地后可见身高；各文档口径一致（RUNTIME / DECISIONS / ART_BIBLE 同）。

## 残留

- 除门控表挂起项外无新增矛盾。

## 最近一次绿灯

| 日期 | HEAD | EditMode | PlayMode | 失败项 |
|---|---|---|---|---|
| 2026-09-09 | 本轮 S4R-WO-02-BACKLOG-NORMALIZATION（纯文档轮：Backlog 候选池 32 项去重分类+依赖图 DAG 无环+RBC 候选 3 例（全 Supported、golden 合法、CANDIDATE/NOT LOCKED）+Ledger 计数对账（18/4/5/1=28；WO-01「14」=统计笔误不改行）+Matrix linkage/非权威 Coverage Mapping+DECISIONS Candidate≠Approved；**Capability/Mechanic/Runtime/Canonical/Content/New Authorization delta 全=0**；Quick Gate 复跑 PASS） | 246/246 | 11/11 | 无 |
| 2026-09-09 | 本轮 S4R-WO-01-GOVERNANCE-RECONCILIATION（治理对账轮：S4R 计划落库 `docs/reviews/S4R/S4R_PLAN.md`+Frozen Baseline+Capability Ledger/Mechanic Matrix/SoT Index 首次正式建立+RUNTIME 装备节漂移修复（4 槽/13 词缀/2×2→6 槽/快照指针/2×3 六槽）；**Runtime/Canonical/Content delta=0**；Quick Gate PASS；按 S4R Test Policy 性能门不重跑、引用 S4 Final 证据；S4 硬停止与 Voice DEFERRED 原样保留） | 246/246 | 11/11 | 无 |
| 2026-09-09 | 本轮 S4-P5R-LOCKED-ENV-RECERTIFICATION（Display Audit=Branch A（EDID 1440 可用）；系统正常模式切换恢复活动显示 2560×1440@144；**Final Gate `-IncludeArtPerformance` 双 PASS**：Canonical worst avg=1.903/worst p99=4.840=58.1%、Art worst avg=4.085=49.0%/worst p99=6.671=80.1%/resolvedVisuals=4/fallback=0/clones=0；加上 Gate 1/2/3+hash exact match+归档 OK——**S4 = COMPLETE**；硬停止等待导演下一方向） | 246/246 | 11/11 | 无 |
| 2026-09-09 | 本轮 S4-P5-INTEGRATED-CLOSEOUT（Branch B：Gate 1/2/3 全 PASS+Simulation hash exact match a1f075f251ec1070+R7/R9 归档 OK×4+snapshot 全对齐+零代码 diff；**Gate 4/Art Gate=ENV_NOT_MET**（活动显示=虚拟 1920×1080@144≠锁定 2560×1440，性能余量健康不放行）；S4 仍 IN PROGRESS——Phase 5 blocked，待导演恢复 1440p 或正式合同变更令） | 246/246 | 11/11 | Gate4/Art=ENV |
| 2026-09-09 | 本轮 S4-P4-PRODUCTION-SIMULATION（真实 Drop/Craft/Equip 路径 10k cycles：invalidCount=0+同 seed hash 一致+六槽全 reachable+新 4 词缀可达；**发现并修复 TryDirectedCraft 重复词缀缺口=duplicate guard deterministic reject**；产物 PRODUCTION_SIMULATION_REPORT.json 工具再生；EditMode 246/246/PlayMode 11/11；Gate 全 PASS；Performance=NO/Art N/A；Phase 4=COMPLETE、Phase 5 NOT STARTED） | 246/246 | 11/11 | 无 |
| 2026-09-09 | 本轮 S4-P3-AFFIX-APPLICABILITY-BREADTH（applicability 单一 predicate 收口（Branch B：AllowedSlots 0=不限槽，Drop/Craft×2/Audit/Report 全复用）；词缀+4（迅握/锋锐=Gloves、壁垒/韧脉=Belt 专属，全部已有 Stat/ModOp）；RollItem eligible 池+Directed Craft deterministic reject；Audit 扩展+护栏 S4-P3 化；Report affixApplicability 17/13/4 bySlot 13×4/15/15；EditMode 242/242+PlayMode 11/11+生成 smoke 零非法对；Gate 全 PASS；Performance=NO/Art N/A；Phase 3=COMPLETE、Phase 4 NOT STARTED） | 242/242 | 11/11 | 无 |
| 2026-09-09 | 本轮 S4-P2-EQUIPMENT-BREADTH-GLOVES-BELT（Truth Drift=Branch T1（canonical 3 Skills/7 Supports；历史「7/8」=口径漂移）；EquipSlot 4→6 旧 ID 零漂移；6 槽 Drop/Craft/Equip/Replace/聚合/快照/Tooltip/抽屉 2×3 全闭环；零第二套装备系统零新 gameplay 语义；Production Report equipmentSlots=6；Gate 全 PASS，Performance=NO/Art N/A；Phase 2=COMPLETE） | 232/232 | 10/10 | 无 |
| 2026-09-09 | 本轮 S4-P0P1-PRODUCTION-READINESS（协调席 S4 Master Plan 入库=Production Scale & Itemization Breadth；Production Content Report v1=`docs/qa/CONTENT_PRODUCTION_REPORT.json` machine-readable 确定性/禁止手填/verdict 继承 audit；Branch A 复用 Collect seam 零第二套 scanner；Gate=SelfTest+EditMode+PlayMode+Audit fresh+Build+PlayerRun exit=0 全 PASS；Performance/Art N/A；Content/Gameplay/UI/Art/Audio delta=0；Phase 0/1=COMPLETE、S4=IN PROGRESS） | 221/221 | 9/9 | 无 |
| 2026-09-08 | 本轮 R9（gpuSkinning 根因修复（全项目 CPU 蒙皮→GPU）+Ashling→GargoyleVisual 正式接入=PASS — GARGOYLE OPTIMIZED AND ASHLING FORMAL VISUAL INTEGRATED；双 Gate 同 HEAD=6572fbd 全 PASS，canonical 9/9+Art 9/9，四视觉 mix worst p99=6.168ms=74.0%；证据冻结 docs/reviews/s3/art-performance-r9-gargoyle/） | 199/199 | 9/9 | 无 |
| 2026-09-08 | 本轮 R8（Ashling 定向选模=Gargoyle SELECTED+资产保留；Formal Art Performance Gate 四视觉 mix 实测 FAIL→按工作令恢复 Ashling placeholder 不降质修绿=ASSET ACCEPTED — INTEGRATION BLOCKED；恢复后全门 PASS 198/198+9/9+Build+canonical 9/9+Art 9/9 无回归） | 198/198 | 9/9 | 无 |
| 2026-09-08 | 本轮 R7（Formal Art Density Performance Gate=ESTABLISHED：-IncludeArtPerformance 第五层，双 Gate 同 sourceCommit=321d9b4 全 PASS，canonical 9/9+Art 9/9，art worst p99=7.628ms=91.6%；证据冻结 docs/reviews/s3/art-performance-r7/；SelfTest art 15+3 项全绿；ART_VISUAL_RENDER_COST_NOT_REPRESENTED_BY_CANONICAL_HARNESS 关闭并替换为精确表述） | 191/191 | 9/9 | 无 |
| 2026-09-08 | 本轮 R6（Warden→Bruce 第三正式视觉=INTEGRATED，three formal enemy visuals integrated；导演四项指示入库；Gate PASS 191/191+9/9+Build+PlayerRun exit=0+Performance 9/9 PASS worst p99=5.403ms=64.8%，Harness 代表性=NO 如实记录） | 191/191 | 9/9 | 无 |
| 2026-09-08 | 本轮 R5（Troll/FireLion 正式视觉 Hit/Ignite 反馈统一=Formal enemy visual feedback parity established；MaterialPropertyBlock 零 gameplay 写；Gate PASS 184/184+8/8+Build+PlayerRun exit=0+Performance 9/9 PASS worst p99=2.778ms=33.4%，Harness 代表性=NO 如实记录） | 184/184 | 8/8 | 无 |
| 2026-09-08 | 本轮 R4（第二敌人视觉 Lion Head 经通用 Catalog 接入 Stinger 位=INTEGRATED；修复 R3 遗留 Mount scale 覆写；Gate PASS 176/176+6/6+Build（resources.assets 检索实证 FireLionVisual 入构建）+PlayerRun exit=0+Performance 9/9 PASS worst p99=2.458ms=29.5%，Harness 代表性=NO 如实记录） | 176/176 | 6/6 | 无 |
| 2026-09-08 | 本轮 R2 后续 R3（Troll 正式接入默认 Runtime Brute 视觉位=INTEGRATED；Gate PASS 169/169+5/5+Build（resources.assets 检索实证 Troll 入构建）+PlayerRun exit=0+Performance 9/9 PASS worst p99=2.615ms=31.4%，Harness 代表性=NO 如实记录） | 169/169 | 5/5 | 无 |
| 2026-09-08 | d488650+d9c02bc+e3c250f（Phase 5 Art Trial R2：Dexsoft 巨魔战士 Troll_2 单模型导入+试装+九轴=ACCEPT WITH FOLLOW-UP（8 PASS+风格 CONDITIONAL）；Gate PASS 158/158+3/3+Build+PlayerRun exit=0） | 158/158 | 3/3 | 无 |
| 2026-09-08 | e5f646e+ed5c4a8+415ae41（Phase 5 Art Trial R1：Bruce 单模型导入+试装+九轴=ACCEPT WITH FOLLOW-UP（8 PASS+风格 CONDITIONAL）；Gate PASS 148/148+3/3+Build+PlayerRun exit=0） | 148/148 | 3/3 | 无 |
| 2026-09-08 | 490ff69+0226c38+ab1db82（Phase 3 UI 第 3 轮：统一 Tooltip+装备同槽 union 对比+Support canonical 兼容提示；Player Runtime Gate PASS 138/138+3/3+Build+PlayerRun exit=0 @2560×1440） | 138/138 | 3/3 | 无 |
| 2026-09-08 | b165e3b+5848857+14b39ca（Phase 3 UI 第 2 轮：右侧装备抽屉 SliceDrawerLayout+2×2 迷你槽+Build/Craft 右锚迁移；Player Runtime Gate PASS 127/127+3/3+Build+PlayerRun exit=0 @2560×1440） | 127/127 | 3/3 | 无 |
| 2026-09-08 | 9c28889+d8bc144（Phase 3 UI 第 1 轮：SliceSkin 程序化石质贴图+底栏双球+点击效；Player Runtime Gate PASS 121/121+3/3+Build+PlayerRun exit=0 @1920×1080；STATUS 回填=a90f34a） | 121/121 | 3/3 | 无 |
| 2026-09-08 | e9d681e（hotfix：ROADMAP 页首 S2P/1440p CLOSED 对齐；SelfTest 9/9+Quick Gate PASS） | 115/115 | 3/3 | 无 |
| 2026-09-08 | 0f5908b（M9：Inference 移除后五门矩阵 Quick/Build/Quick2/PlayerRuntime/Performance 全 PASS，每步树净+define 0 变动；worst p99=2.376ms；振荡 CLOSED） | 115/115 | 3/3 | 无 |
| 2026-09-08 | ead815c+3a577d3+a11aa87+9889998（M8 revalidation Gate A+B：18 测量全 PASS，worst p99=4.380ms=预算 52.6%；gate-a/gate-b raw 14 文件冻结+MANIFEST 校验，同 HEAD=a11aa87） | 115/115 | 3/3 | 无 |
| 2026-09-08 | b3999cd+0eeeea8+422d356（Performance Gate A+B：18 测量全 PASS，worst p99=2.388ms；S2P 1440p/120 CLOSED） | 115/115 | 3/3 | 无 |
| 2026-09-08 | dfb33bd（Player Runtime Gate×2：115/115+3/3+build+PlayerRun PASS；性能=NOT_EVALUATED） | 115/115 | 3/3 | 无 |
| 2026-09-08 | 938d697（Full Gate×6：115/115+3/3+win64 build PASS） | 115/115 | 3/3 | 无 |
| 2026-09-08 | 58544a3+71601fd+61813ac（canonical Gate×3） | 113/113 | 3/3 | 无 |
| 2026-09-08 | 6ad5679+f2673da | 113/113 | 3/3 | 无 |
| 2026-09-08 | 9697f65+368ed36 | 105/105 | 3/3 | 无 |
| 2026-09-07 | 6014bc5+a56d11b | 100/100 | 3/3 | 无 |
| 2026-09-07 | 2c8e5bc+f40b5ef | 96/96 | 3/3 | 无 |
| 2026-09-07 | a3572f8+b8a6e2a | 95/95 | 3/3 | 无 |
| 2026-09-07 | 3a45983 | 89/89 | 3/3 | 无 |
| 2026-09-07 | ba87b8a | 83/83 | 3/3 | 无 |
| 2026-09-07 | f35586c | 82/82 | 3/3 | 无 |

心跳：2026-09-07 | HEAD=cd16e50 | 工作区干净 | 门控 7 项未开工
心跳#2：2026-09-07 | HEAD=f80057a | 工作区干净 | 门控 7 项未开工
心跳#3：2026-09-07 | HEAD=004539d | 工作区干净 | 门控 7 项未开工
