# ROADMAP

当前阶段：**S3 = COMPLETE（Phase 1/2/3 UI/5 Art 全 COMPLETE；Phase 4 人声=DEFERRED BY DIRECTOR，排除出当前完成范围——总收口 `docs/reviews/s3/S3_OVERALL_CLOSEOUT_REVIEW.md` PASS，2026-09-08）；S4 = IN PROGRESS（Production Scale & Itemization Breadth，规划 `docs/reviews/s4/S4_PLAN.md`）：Phase 0=COMPLETE、Phase 1=COMPLETE（Production Tooling v1）、Phase 2（Gloves+Belt 装备槽扩容）=IN PROGRESS、Phase 3-5 NOT STARTED；Stage0 锁全部延续；1440p/120 已于 M7 在锁定硬件下 CLOSED（结论只适用锁定硬件/canonical contract，非所有 Windows PC 保证 120FPS）**（S2 玩法循环已由导演确认，2026-09-06。自制 Eve FBX **已剔除**，换模 Dark Knight 技术放行。碰撞规则仍在。I 不自放行）。

## 阶段

| 阶段 | 目标 | 状态 |
|---|---|---|
| S0 | 工程能打开，测试能跑，合法数据能进，坏数据失败，Seed 可复现 | **已放行** |
| S1 | 能打 + 能量测 | **已放行**（手感门 2026-09-07 导演复测关闭） |
| S2 | 微循环能玩 | **已完成**（导演 2026-09-06 确认）；换模 Dark Knight 技术放行（贴地修正 ebbf782）；碰撞仍在（HUD 未完备） |
| S2P | 真实系统密度报告 | **1080p 已证（b152610）+ 1440p/120 已于 M7 在锁定硬件下 CLOSED（2026-09-08，证据 `docs/reviews/s2p/1440p-120-m7/`；边界=锁定硬件，非所有 Windows PC 保证）** |
| S3 | 按队列扩内容 | **S3=COMPLETE（2026-09-08 总收口 `S3_OVERALL_CLOSEOUT_REVIEW.md` PASS）**：Phase 1（内容工厂）+ Phase 2（R1 3 组合词缀/R2 火焰转化）+ Phase 3 Formal UI（R1-R4）+ Phase 5 Art（四正式视觉+Art Gate）全 COMPLETE；**Phase 4 人声=DEFERRED BY DIRECTOR（排除出当前完成范围）** |
| S4 | Production Scale & Itemization Breadth（扩产工具→装备槽→词缀→生产模拟） | **IN PROGRESS（导演 2026-09-09「开启 S4」，规划 `docs/reviews/s4/S4_PLAN.md`）**：Phase 0=COMPLETE、Phase 1=COMPLETE（Production Content Report=`docs/qa/CONTENT_PRODUCTION_REPORT.json`）；**Phase 2 Gloves+Belt=IN PROGRESS**；Phase 3-5 NOT STARTED；Stage0 锁延续、Voice 继续 DEFERRED |

## Gate S0

- [x] 项目能打开且无编译错误
- [x] EditMode 测试一次跑完
- [x] 合法 Dummy 能 Import
- [x] 缺 ID 明确失败
- [x] 固定 Seed 序列可复现

## Gate S1

- [x] 导演认为移动和基础攻击「可以继续」（已放行，下令开 S2）
- [x] 密度场景可复现测量
- [x] 瓶颈清单存在（`docs/S1_PERF.md`）
- [x] 未引入「每个 Buff 一个 GameObject / 每个弹道一个 Rigidbody / 每怪每帧完整寻路」

## Gate S2（玩法门，导演拍板。2026-09-06 导演确认通过）

- [x] 改 Support 或词缀或天赋后，技能差异肉眼可见
- [x] 更高风险地图的收益可理解
- [x] 死亡后可免费重构再挑战（图内仍锁 Build）
- [x] 10～20 分钟内能完整走完两轮循环

失败：修循环或修可见反馈，不开始 S2P / S3。

## Gate S2P

**技术收口已记录（2026-09-07，详见 `docs/reviews/s2p/S2P_CLOSEOUT.md`）**：

- [x] Editor 内嵌基线（6e95c1b）：100/200/300 可复核，非 120FPS 证据
- [x] 独立包 1080p 全屏基线（b152610，ArenaPerfHarness 可复现）：主线程 p99 最高 2.508ms、GPU≈0.33ms，均低于 8.33ms 预算
- [x] 1440p 分辨率证据：**已完成**（M7 locked hardware 真实 2560×1440；历史：2026-09-07 曾因本机 1080p 显示器钳制挂起，后条件变化补测）
- [x] 120FPS@1440p 结案：**CLOSED（M7 locked hardware，2026-09-08）**——2 Gate ×3 Run ×3 密度=18 测量全 PASS，worst p99=2.388ms / 预算 8.33

## 下一步

S2P 技术收口已记录。**S3 最小底座已开工（2026-09-07）**：`docs/art/ART_BIBLE.md` 初稿 + 内容校验（`ContentAuditS2Tests`，EditMode）。**S3 内容工厂 Phase 1 + 组合增产 Phase 2 已完成（2026-09-07，工作令 S3-P12 收口）**：R1=3 组合系词缀（`S3_BATCH1_REVIEW.md` PASS）；R2=火焰转化 Support（`S3_R2_REVIEW.md` PASS）；Runtime Support 兼容门（golden=独立 oracle）；资源路径级真实验证（REQUIRED 缺失即红；人声=GATED 记录；VFX=无声明引用 N/A）+ Tag 组合规则/golden/死内容扫描（`CONTENT_AUDIT_S3_CLOSEOUT.md`、`S3_PHASE2_CLOSEOUT.md`）。**仍禁止**——大天赋树、Unique 库、Atlas、完整 Craft、新运行期系统。**未使用 Tag ≠ 下一工作项**。后续：**阶段 3 UI=导演门控、阶段 4 人声=导演门控、阶段 5 精模=导演门控（GATED / NOT STARTED，非阻塞）；后置系统仍关**——S3 整体未结束，下一步等规划 AI 逐令。

## 导演门控待输入

（**2026-09-07 导演已就位并逐项裁定**，见下表「导演裁定」列。裁定前原规则为「无输入则不做」。裁定后新状态：已下令项进 GPT 流水排令执行；关闭项归档不再等输入。）

| 项 | 现状 | 导演裁定（2026-09-07） |
|---|---|---|
| ART_BIBLE 友方/敌方/危险色 | 临时值（Rare 金实测） | **占位**——现阶段颜色随意；后续按技能敲定配色并配贴图+动效（归技能表现/皮肤轮） |
| 1440p@120 补测 | 1080p 已证；1440p 硬件钳制（当时） | 历史：曾裁定「保留后续测试/挂起（不补测、不结案）」；**2026-09-08 M7 建立锁定硬件 Performance Gate 后条件变化，已完成补测并 CLOSED**（证据 `docs/reviews/s2p/1440p-120-m7/`）——不再是待输入项，不自动启动其它任何门控项 |
| 正式 UI | IMGUI 简易可读 | **方案页已出**（2026-09-07，`docs/ui/UI_PROPOSAL_POE_D3.md`：石质面板+金描边+双球+底栏+右侧抽屉）——**待导演过目点头后才分轮实现** |
| 语音 ogg 接入 | `VoiceCues` 3 键已接+冷却；映射 GATED（0/3 present） | **已执行·第一轮**（2026-09-07）：`VoiceCues` 独立表 3 键+冷却已接（Cast 1.5s/Hit 0.6s/Death 每次一次）；**映射缺口待导演试听指认**（清单 `docs/reviews/audio/DK_VOICE_INVENTORY.md`，候选在 `Desktop/voice_listen/`） |
| EveView/DriveEve 改名 | **已执行**（2026-09-07 改名轮：`DarkKnightView`/`DriveDarkKnight`，行为零变化） | 指向改名轮 commit；历史叙述保留曾用名 |
| 怪物独立 EnemyHit 事件 | 无此名，用 Impact 语义 | **维持现状**——不新增 |
| 5 个音频 clip | **已投放（REQUIRED 资源审计 6/6 PASS）** | **已执行**（2026-09-07）：五键全 CC0 投放（来源 80 CC0 RPG SFX / rubberduck / OGA，许可与映射见 `docs/reviews/audio/SFX_SOURCES.md`）；Play 实证五键 PlayOneShot 全响 |

表外项 **S3 内容扩张**：导演裁定（2026-09-07）——其他问题全部完成后开启 S3；S3 内容问 GPT 要规划。
