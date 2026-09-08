# STATUS（门状态一页纸）

日期：2026-09-08。本轮提交：见下方「本轮 commit」行（由紧随的回填提交写入，格式 A=STATUS 主体 / B=回填）。

本轮 commit（Phase 3 UI 第 2 轮）：A=b165e3b（feat(ui)：右侧装备抽屉 SliceDrawerLayout 纯几何+常驻 2×2 迷你槽+Build/Craft 右锚迁移+BlocksWorldInput 抽屉吞区）+ 5848857（test(ui)：SliceDrawerTests +6=127/127）+ 14b39ca（docs(ui)：R2 复核 PASS+方案页/RUNTIME 同步+导演许可证裁定入库）；上轮第 1 轮 A=9c28889+d8bc144 已回填于下方绿灯表 / B=回填本行

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
| S3 内容扩张 | **阶段 1+2 已收口（COMPLETE）** | 第一批/R1=PASS（`S3_BATCH1_REVIEW.md`）；R2 火焰转化=PASS（`S3_R2_REVIEW.md`）；兼容门+资源真实验证+Tag 规则（`S3_PHASE2_CLOSEOUT.md`）；**阶段 3-5=导演门控 GATED/NOT STARTED，后置系统仍关，S3 整体未结束** |

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

- `ArenaPerfHarness` 默认关；正式游玩路径不得自动开启。1080p 独立包三档 p99 最高 2.508ms（历史证据保留，`docs/reviews/s2p/`）。**1440p/120 已于 M7 在锁定硬件下 CLOSED（2026-09-08）**：真实 2560×1440 / fullscreen / D3D12 / PC / vSync=0 / targetFps=-1，2 个 canonical Gate ×3 Run ×3 Density=18 测量全 PASS，worst p99=2.388ms=预算 8.33 的 28.7%；证据 `docs/reviews/s2p/1440p-120-m7/`。结论只适用锁定 M7 硬件（Ryzen 7 5700X3D / RTX 5070）+ canonical 契约，**不代表所有 Windows PC 保证 120FPS**；其它机器须按 `docs/qa/PERFORMANCE_GATE.json` 显式更新并重建 baseline。

## 音频

- 当前事实（收口审计 `CONTENT_AUDIT_S3_CLOSEOUT.md` 资源契约为准）：**战斗 SFX 5 键已投放全 CC0（Cast/Impact/Hit/Death/Loot，REQUIRED 6/6 真实加载 PASS，含玩家预制体）**；**人声 VoiceCues 3 键（Cast/Hit/Death）映射仍 GATED（0/3 present，缺失=静音，待导演试听指认）**。单一入口 `AudioEvents.Play` + 独立 `VoiceCues`，无中间件；缺资产=静音+限频日志（降级模式）。路径拼接单一真相源 `RuntimeResourcePaths`（S3-M2）：审计期望键集与 REQUIRED/GATED 分类保持独立 oracle，declared-key parity 防未审批增删资源入口。

## 校验

- EditMode 127/127、PlayMode 3/3（canonical Gate `.\tools\verify_unattended.ps1`；M5 维护轮 +2 Build Settings 场景契约测试；Phase 3 UI 第 1 轮 +6 SliceSkin 贴图契约测试；Phase 3 UI 第 2 轮 +6 装备抽屉几何/输入阻挡测试）。报告 **`CONTENT_AUDIT_S3_CLOSEOUT.md`**（测试再生；BATCH1/R2 保留历史）：**顶部 Verdict 块=Audit completed: YES / Verdict: PASS / Failure count: 0**；StatId/ModOp/运行期未知 Stat=0；非法 Tag=0；Effect-Event=0；链接=0；词缀行=0；兼容矩阵=0（3×7 parity 21 组合）；**Skill Tag golden parity=3/3**；**死 Tagged Modifier=0**（4 条全可达）；**结构/契约问题=0**；**资源契约=REQUIRED 6/6 真实加载 PASS（玩家预制体+5 SFX）、GATED 人声 3 键缺失如实记录、VFX 声明引用 0=N/A**。负向 Tag 规则测试通过。阶段 1/2 收口=`S3_PHASE2_CLOSEOUT.md`（Phase 1/2=COMPLETE）；M1 事实收敛（`S3_M1_REPO_TRUTH_REVIEW.md` PASS）；M2 资源路径单一真相源（`S3_M2_RESOURCE_CONTRACT_REVIEW.md` PASS）；M3 failure-safe 审计（`S3_M3_AUDIT_FAILSAFE_REVIEW.md` PASS）；M4 无人值守 Quick Gate（`S3_M4_UNATTENDED_GATE_REVIEW.md` PASS）；**M5 Full Integration Gate=Quick+win64 Player Build（`.\tools\verify_unattended.ps1 -IncludeBuild`；`-batchmode -quit -buildTarget win64 -buildWindows64Player` 实测；双证据判定；6 次运行全 PASS，exe 667136 bytes/Data 162 文件；Build Settings 场景契约锁定 Bootstrap=0/Arena=1；设置资产归一化收口+define 振荡根因修复 `submitAnalytics: 0`），复核=`S3_M5_WIN64_BUILD_GATE_REVIEW.md`（PASS）**；**M6 Player Runtime Gate=`-IncludePlayerRun`（隐含 Build；本轮刚构建 Player 以 `-arenaPerf` 启动三档跑完自退 exit=0；证据 QA golden 100/200/300+editor=False+实测分辨率一致+13 列可解析；无性能阈值恒输出 `PerformanceVerdict: NOT_EVALUATED`；Harness 证据标题中性化+删 enteredMap 假字段；实测分辨率 2560x1440/DX12 仅记录——1440p/120 维持未结案、性能未判定（M6 当时状态；后由 M7 CLOSED）），复核=`S3_M6_PLAYER_RUNTIME_TRUTH_REVIEW.md`（PASS）**；**M7 Performance Gate=`-IncludePerformance`（contract=`docs/qa/PERFORMANCE_GATE.json` 锁 2560×1440/D3D12/PC/vSync0/targetFps-1/CPU+GPU；同一 Build 连续 3 次 Player 锁定环境运行；硬指标 main avg+p99≤8.33+cpu/gpu≤8.33+FrameTiming 必须+alive≥95%+frames=600；**2 个 canonical Gate ×3 Run×3 Density=18 测量全 PASS，worst p99=2.388ms=预算 28.7%→S2P 1440p/120 CLOSED（锁定硬件）**，冻结证据 `docs/reviews/s2p/1440p-120-m7/`；预算语义=`docs/qa/PERFORMANCE_BUDGET.md`），复核=`S3_M7_1440P_PERFORMANCE_GATE_REVIEW.md`（PASS）**；**M8 Evidence Integrity Gate（工作令 S3-M8-PERF-EVIDENCE-INTEGRITY）：新增显式快照 Operator `tools/snapshot_performance_evidence.ps1`（Verifier≠Operator 职责分离=DECISIONS 规则⑨；拒非 PASS/不完整/契约不匹配/脏树/已存在目标；MANIFEST 逐文件 SHA-256；-VerifyArchive 防篡改），M7「Gate A 只有抄表」证据不对称以**未来可重复链**修复=Gate PASS→立即快照→下一 Gate→VerifyArchive；**M8 provenance-complete revalidation=PASS**（Evidence HEAD=a11aa87，两 canonical Gate 各 9/9 PASS 且同 HEAD；18/18 预算内 worst p99=4.380ms=52.6%；gate-a/gate-b 原始证据各 14 文件冻结+MANIFEST 校验双 OK；ProjectSettings define 振荡矩阵实测=Quick 门移除/Performance·Build 门回写，Evidence HEAD 取 Performance 门终态，Gate A 首尝试因脏树被快照拒收而作废未冻结——如实记录），复核=`S3_M8_PERF_EVIDENCE_INTEGRITY_REVIEW.md`（PASS）**；**M9 Inference Dependency Closeout（工作令 S3-M9-INFERENCE-DEPENDENCY-CLOSEOUT，维护序列最终根因项）：零使用审计全过后移除 com.unity.ai.inference 直接依赖（lock UPM 解析 -Inference/-dt.app-ui；burst/collections 由 URP 保活），ProjectSettings canonical 一次性删 SENTIS define（根因=Inference 包 AnalyticsDefineManager 跨实例加删）；**五门矩阵 Quick/Build/Quick-after-Build/PlayerRuntime/Performance 全 PASS+每步树净+define 0 变动=振荡 CLOSED**；Performance 回归 9/9 PASS（worst p99=2.376ms=28.5%，contract/workload 零 diff）；DECISIONS 规则⑩（Package removal 须先审计）；Content delta=0；**维护序列正式结束 GATE-READY**），复核=`S3_M9_INFERENCE_DEPENDENCY_REVIEW.md`（PASS）**。未使用 Tag 4 个（Spell/Projectile/Fire/Duration）=预留，不是任务。

## 导演门控待输入

- 表在 `docs/ROADMAP.md`「导演门控待输入」节（7 项，每项「无输入则不做」，不得当自动任务开工）。
- **S3 已开工（历史「开 S3」口令已于 2026-09-07 消费）**：Phase 1/2=COMPLETE；Phase 3 UI / 4 人声 / 5 精模=导演门控 GATED/NOT STARTED；**新一批内容（含第 8 Support/新词缀/新怪）不得自行启动**——需规划 AI/导演新立工作令；规划见 `docs/reviews/s3/S3_PLAN.md`（S3 整体未结束，不写成 COMPLETE）。

## 已对齐（本轮只改文档）

- `docs/art/ART_BIBLE.md` 相机行 (0,17,-15)→(0,10.6,-9.3)：76381c0 改跟拍后 ART_BIBLE 未同步——本页轮对齐。
- 1.16 / 1.19 并存确认**非矛盾**：1.16=胶囊碰撞体高度（`DarkKnightView.TargetHeight`），1.19=贴地后可见身高；各文档口径一致（RUNTIME / DECISIONS / ART_BIBLE 同）。

## 残留

- 除门控表挂起项外无新增矛盾。

## 最近一次绿灯

| 日期 | HEAD | EditMode | PlayMode | 失败项 |
|---|---|---|---|---|
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
