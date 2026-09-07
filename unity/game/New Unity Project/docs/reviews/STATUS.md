# STATUS（门状态一页纸）

日期：2026-09-07。本轮提交：见下方「本轮 commit」行（由紧随的回填提交写入，格式 A=STATUS 主体 / B=回填）。

本轮 commit：A=（S3-M1 主体：sentinel 目录硬化 + RUNTIME/STATUS/ROADMAP 事实收敛，由紧随的回填提交写入）/ B=回填本行

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
| S2P 1440p@120 | 未结案 | 硬件钳制（本机 1080p 显示器）；不宣布 120FPS@1440p |
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

- `ArenaPerfHarness` 默认关；正式游玩路径不得自动开启。1080p 独立包三档 p99 最高 2.508ms；**不宣布 120FPS@1440p**；1440p 补测挂门控表。

## 音频

- 当前事实（收口审计 `CONTENT_AUDIT_S3_CLOSEOUT.md` 资源契约为准）：**战斗 SFX 5 键已投放全 CC0（Cast/Impact/Hit/Death/Loot，REQUIRED 6/6 真实加载 PASS，含玩家预制体）**；**人声 VoiceCues 3 键（Cast/Hit/Death）映射仍 GATED（0/3 present，缺失=静音，待导演试听指认）**。单一入口 `AudioEvents.Play` + 独立 `VoiceCues`，无中间件；缺资产=静音+限频日志（降级模式）。

## 校验

- EditMode 100/100、PlayMode 3/3（pipeline run_tests，运行中编辑器；M1 维护轮 +4 目录不变量测试）。报告 **`CONTENT_AUDIT_S3_CLOSEOUT.md`**（测试再生；BATCH1/R2 保留历史）：StatId/ModOp/运行期未知 Stat=0；非法 Tag=0；Effect-Event=0；链接=0；词缀行=0；兼容矩阵=0（3×7 parity 21 组合）；**Skill Tag golden parity=3/3**；**死 Tagged Modifier=0**（4 条全可达）；**资源契约=REQUIRED 6/6 真实加载 PASS（玩家预制体+5 SFX）、GATED 人声 3 键缺失如实记录、VFX 声明引用 0=N/A**。负向 Tag 规则测试通过。阶段 1/2 收口=`S3_PHASE2_CLOSEOUT.md`（Phase 1/2=COMPLETE）；M1 事实收敛=`S3_M1_REPO_TRUTH_AUDIT.md`/`S3_M1_REPO_TRUTH_REVIEW.md`（PASS）。未使用 Tag 4 个（Spell/Projectile/Fire/Duration）=预留，不是任务。

## 导演门控待输入

- 表在 `docs/ROADMAP.md`「导演门控待输入」节（7 项，每项「无输入则不做」，不得当自动任务开工）。
- **S3 规划已出**（`docs/reviews/s3/S3_PLAN.md`）、内容扩张仍关；开工口令=导演明说「开 S3」。

## 已对齐（本轮只改文档）

- `docs/art/ART_BIBLE.md` 相机行 (0,17,-15)→(0,10.6,-9.3)：76381c0 改跟拍后 ART_BIBLE 未同步——本页轮对齐。
- 1.16 / 1.19 并存确认**非矛盾**：1.16=胶囊碰撞体高度（`DarkKnightView.TargetHeight`），1.19=贴地后可见身高；各文档口径一致（RUNTIME / DECISIONS / ART_BIBLE 同）。

## 残留

- 除门控表挂起项外无新增矛盾。

## 最近一次绿灯

| 日期 | HEAD | EditMode | PlayMode | 失败项 |
|---|---|---|---|---|
| 2026-09-07 | S3-M1（回填写入） | 100/100 | 3/3 | 无 |
| 2026-09-07 | 2c8e5bc+f40b5ef | 96/96 | 3/3 | 无 |
| 2026-09-07 | a3572f8+b8a6e2a | 95/95 | 3/3 | 无 |
| 2026-09-07 | 3a45983 | 89/89 | 3/3 | 无 |
| 2026-09-07 | ba87b8a | 83/83 | 3/3 | 无 |
| 2026-09-07 | f35586c | 82/82 | 3/3 | 无 |

心跳：2026-09-07 | HEAD=cd16e50 | 工作区干净 | 门控 7 项未开工
心跳#2：2026-09-07 | HEAD=f80057a | 工作区干净 | 门控 7 项未开工
心跳#3：2026-09-07 | HEAD=004539d | 工作区干净 | 门控 7 项未开工
