# DECISIONS

S0 锁定。后续阶段不得在未改本文的情况下推翻这些决定。

## S2P 技术收口（2026-09-07）

- S1 手感门：导演复测通过，**正式关闭**（2026-09-07）。
- S2 换模 + 碰撞：**技术放行**（R 审 f22862e；实现 239142e）；贴地修正 ebbf782；外观「小于敌人/看不清」项已关闭。「模型过」仍是审美终审语，不因技术放行而自称。
- Editor 内嵌基线（6e95c1b）成立，仅作对照，**非 120FPS 证据**。
- 独立包 1080p 全屏基线（b152610）成立：100/200/300 主线程 p99 最高 2.508ms，GPU≈0.33ms，均低于 8.33ms；300 档末帧存活 294/300 接受为接近额定（补怪即时生效）。
- 1440p 被本机 1080p 显示器钳制（全屏/窗口两种方式均被钳回），**待有 1440p 硬件再补测**；不宣布 120FPS@1440p。
- 测量工具 `ArenaPerfHarness`：默认关；仅独立包 `-arenaPerf` 命令行或显式调用启动；**正式游玩路径不得自动开启**。
- 非阻断残留：Eve 回退分支已删除（2026-09-07）——`TryMount` 只加载 `Player/DarkKnight`，缺失即回退胶囊并打日志；`EveView/DriveEve` 命名残留**已改名清零（2026-09-07 改名轮）**：`DarkKnightView`/`DriveDarkKnight`，历史 commit/叙述保留曾用名。
- S3 最小底座（2026-09-07）：Art Bible 初稿路径 `docs/art/ART_BIBLE.md`；内容校验挂 EditMode（`ContentAuditS2Tests`），**只覆盖 S2 切片**（3 Active + 6 Support + 10 词缀 + 16 天赋 + 3 图词缀 + 5 怪），报告 `docs/reviews/s3/CONTENT_AUDIT_S2.md`；音频缺失为已知债不阻断。不加新内容。
- 预留 Tag 钉死（2026-09-07）：Attack / Spell / Projectile / Hit / Physical / Fire / Duration 为**已声明预留**，当前 S2 切片未引用；**S3 内容扩张开启前不得当作「缺实现」去补系统或补技能**；「未使用 Tag ≠ 下一工作项」。
- 玩家视图 Hit/Death 动画（2026-09-07）：自已留档的 401 动画库取 2 条——`pdw_01_01_def_shield_dam_00`（受击后仰，0.63s，全段）与 `pdw_01_01_def_shield_break_00`（裁至跪倒段 0.68s，掐掉恢复站立）；控制器加 Hit/Death 两状态（与 Idle/Run/Attack/Cast 同层）。`DriveDarkKnight` 只挂现有信号：`Session.HitFlash` 上跳沿=Hit（下一击重播即可打断），`MapState.Dead`=Death（播完 `animator.speed=0` 定格跪倒末帧，复活出图恢复）。逻辑状态机未扩、不接音频、命中/死亡公式未动。截图 `docs/reviews/images/07_hit.png`、`08_death.png`。
- 相机跟拍收一帧（2026-09-07）：跟拍按贴地后身高 ≈1.19 重取景——偏移 `(0,17,-15)`→`(0,10.6,-9.3)`（注视点距离 22.7→14.1，俯角不变 ≈48.6°），注视高度 `CamLookY 0.5→0.6`（胸口/身体包围盒中心近似，不锁武器尖），FOV 42 不变。人物约占画面高 1/9（改前约 1/24），全身可辨+近怪在框（截图 `docs/reviews/images/09_camera.png`）。只动常量，无平滑、无新镜头系统；相机恒在地面 y=0 上方 11.2，不穿地；死亡冻帧/进出图无新增摆动源。
- 音频事件挂钩·无素材（2026-09-07）：单一入口 `AudioEvents.Play`（不进 FMOD/Wwise，不生产 wav/ogg）；查找表 `Resources/Audio/<事件名>` 预留 5 键（值可空），无资产=静音+限频日志（5s/键，禁每帧刷屏）。触发点：Cast=`ArenaSim.Resolve` 施放起手；Impact=`ArenaSim.PlayImpact` 技能命中；Hit=玩家受击 HitFlash 上跳沿（`ArenaDirector`，与 Hit 动画同帧；原怪物受击挂点已摘除）；Death=玩家进入 `MapState.Dead`（`ArenaDirector`，与 Death 动画同处；原怪物死亡挂点已摘除）；Loot=**掉落生成**（`SliceSession.DropGear`，AddItem 成功即触发——选生成非拾取，当前无独立拾取动作）。语音 ogg 仍不接；正式游玩路径缺音频不报错不卡死。
- 导演门控待输入表（2026-09-07）：ROADMAP 增「导演门控待输入」节（ART_BIBLE 临时色 / 1440p 补测 / 正式 UI / 语音 ogg / EveView 改名 / 怪物独立 EnemyHit 事件 / 音频 clip 投资源）——每项「无输入则不做」，不得当自动任务开工。
- 门状态总览（2026-09-07）：`docs/reviews/STATUS.md` 一页纸——11 门状态+玩家视图/碰撞/测量/音频/校验快照；总览见 STATUS.md。
- 导演决策集·八项（2026-09-07 导演就位逐项裁定，同日细化）：①三色**仅占位**——现阶段颜色随意，后续按技能敲定配色并配贴图+动效（归未来皮肤/技能表现轮）；②1440p 补测**保留后续测试**（维持挂起、不宣布 120@1440p）；③**重做 UI，参考 PoE/暗黑 3，可使用网络免费资源**（先出方案页给导演过目再实现）；④语音 ogg **全部事件接入并设默认冷却**；⑤**批准** EveView/DriveEve 改名轮——**已执行（2026-09-07 改名轮：`DarkKnightView`/`DriveDarkKnight`）**；⑥怪物独立 EnemyHit 事件**维持现状**（用 Impact 语义，不新增）；⑦5 个音频 clip——**网上搜免费资源**，搜不到则等后续补充；⑧**其余问题全部完成后开启 S3**，S3 内容问 GPT 要规划。门控表（ROADMAP）已同步裁定状态。
- 语音 ogg 接入·第一轮（2026-09-07）：独立人声表 `VoiceCues`（**与 AudioEvents 5 键战斗 SFX 完全隔离**，查找 `Resources/Audio/Voice/<键>`）；3 键=Cast（施放喊招，`ArenaSim.Resolve`，冷却 1.5s）/ Hit（受击呼痛，`ArenaDirector` 上跳沿，冷却 0.6s）/ Death（进入 Dead，`ArenaDirector`，每次一次）。**映射为缺口**：包内 609 条 ogg 为数值 ID 无语义名（fx 208 / voice 401，清点见 `docs/reviews/audio/DK_VOICE_INVENTORY.md`），待导演试听 `Desktop/voice_listen/` 候选（20 条，按 CAST/受击/死亡 前缀）指认后投放 `Resources/Audio/Voice/` 即生效；未指认前=静音。不外购、不硬塞语义。
- 5 键战斗 clip 投放·CC0（2026-09-07）：五键全部接入——来源**单一包 80 CC0 RPG SFX**（rubberduck，OpenGameArt，页面 License=CC0）：Cast=spell_fire_04（黑骑士火系）/ Impact=blade_02 / Hit=creature_hurt_01 / Death=creature_die_01 / Loot=item_coins_01，落 `Assets/Resources/Audio/<键>.ogg`（AudioEvents 查找表零逻辑改动，投放即生效）。来源与许可全录 `docs/reviews/audio/SFX_SOURCES.md`。Play 实证：五键 PlayOneShot 逐一 isPlaying=True。包内 fx 208 条留作备选池。
- UI 方案页已出（2026-09-07）：`docs/ui/UI_PROPOSAL_POE_D3.md`（类 PoE/暗黑 3：深色石质面板+金色细描边+左下双球+底栏技能槽+右侧装备抽屉；含 SliceHud 迁移映射与分轮建议）——**待导演过目点头后才分轮实现**，本轮不改 SliceHud。
- S3 后续规划已出（2026-09-07）：`docs/reviews/s3/S3_PLAN.md`——阶段 0 永关清单 / 1 内容工厂校验扩展 / 2 组合系增产（R1≤3 词缀，R2 +1 Support 变体）/ 3-5 依赖导演输入（UI 过目、人声指认、精模点名）/ 6 后置项单列开启条件 / 7 1440p 非前置。**开工口令=导演明说「开 S3」**；规划存在本身不构成开工；第一批只允许校验扩展+R1。
- S3 第一批已开（2026-09-07，导演口令「开 S3」）：只做 S3_PLAN 允许清单——①内容校验扩展 ②R1 组合系词缀 3 条（灼燃=FireDamage 提高+IgniteChance 固定 / 锐击=Accuracy 固定+CritChanceIncreased 提高 / 熔铸=PhysicalDamage 提高+FireDamage 提高；**全部复用已有 StatId/ModOp，新增 Stat/ModOp/Tag/Effect/Event/运行期系统 = 0**）。组合词缀实现：`AffixDef` 第二行（`Format2` 非空=组合；`Stat2/Op2 + Min2/Max2` 独立掷值），`ItemInstance` 增第二值存储（`SecondValue0..3`）；三词缀进现有 4 槽（武器/胸甲/头盔/靴子）掉落池，且进两步 Craft（随机制作=废料重掷池、定向制作=蚀刻剂写入列表）。
- S3 第一批校验扩展（同日，`ContentAuditS2Tests` 扩展，报告改出 `docs/reviews/s3/CONTENT_AUDIT_S3_BATCH1.md`，S2 报告保留为历史）：①**Support×技能兼容矩阵**（Tag 路径=带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足；机制路径=分裂只接入弹道结算；不兼容必须能失败；钉死 集中×近战/弹道、分裂×近战/范围）；②**运行期消费 Stat 白名单**（27/27 全有读取点，Support/Passive/词缀行全部校验，防「报告绿但运行期未知 Stat」）；③词缀行 ModOp 范围校验；④**未使用 Tag 仍只记录不失败**。测试新增断言：本批新增词缀 ≤3 且 Stat 合法；组合词缀两行都进技能属性包。
- S3-B1-RCLOSE（2026-09-07，规划 AI 工作令「独立复核 + Runtime 兼容门收口」）：①S3 第一批独立复核完成——`docs/reviews/s3/S3_BATCH1_REVIEW.md`，verdict=**PASS**；按真实 diff 核查 A 数据不变量 / B 全消费路径 / C 双值 Roll / D Stat 白名单真实性（27/27 逐一核对非假绿）/ E 审计独立性，无代码级缺陷；补 2 项回归测试（双范围 roll、第二值残留归零）。②**Runtime Support 兼容门已接入**：单一判定入口 `SliceSession.IsSupportCompatible`（Tag 路径=RequiredTags 在技能 Tag 下可满足；机制路径=`SupportDef.MechanicSkill` 新数据字段，分裂=弹道）；`TrySetSupport` 写入前判定，非法组合失败且**无任何半写入**（失败替换保留原合法连接）；装配/配置阶段调用，不进战斗热路径。③职责区分：**golden 矩阵（`SupportCompatGolden`）=独立 oracle**（人工钉死数据），ContentAudit 校验内容、SupportGateTests 校验 Runtime 判定与 golden 全 18 组合一致——Runtime 被放宽即测试红；禁止由 Runtime 反向生成 golden。新增 Stat/ModOp/Tag/Effect/Event/MonoBehaviour/Support/Affix = 0。EditMode 89/89、PlayMode 3/3。
- S3-R2-FIRE-CONVERSION（2026-09-07，规划 AI 工作令）：R2 首个新增 Support 机制变体=**火焰转化**（FireConversion）：50% 物理转火，`Modifier.Tagged(ConvertPhysToFire, Flat, 0.50, Attack|Hit|Physical)`，ChangesMechanism=true、MechanicSkill=None、无 Trigger/Effect——兼容性完全由 Tag 路径自然推导（近战/弹道可接，范围=Spell 无 Attack 拒绝），**Runtime 零专用分支**（grep 全 Runtime 仅目录定义 2 处，战斗代码 0 条 if/switch/case）。**长期决策：Support 与 Passive 共享同一转换轴**——火焰转化 0.50 + 烬心 0.40 经同一 StatBag 自然聚合 0.90，内容来源不拥有自己的战斗公式。golden 扩 3×7（21 组合 parity 全对拍）；内容数量护栏为测试断言（本轮仅 Support +1，其余内容轴全部 +0）；HUD 辅助栏 6→7 最小几何修复（Count 遍历+托盘加高，未扩成 UI 任务）。独立复核 `docs/reviews/s3/S3_R2_REVIEW.md` verdict=**PASS**（1 项测试预期修正：同一 Support 全局唯一为既有规则，非缺陷）。新 Stat/ModOp/Tag/Effect/Event/Condition/MonoBehaviour/Package = 0，新 Support=恰好 1。EditMode 95/95、PlayMode 3/3。报告 `CONTENT_AUDIT_S3_R2.md`（BATCH1 报告保留历史）。
- S3-P12-AUDIT-CLOSEOUT（2026-09-07，规划 AI 工作令「阶段 1/2 收口」）：**S3 阶段 1 与阶段 2 正式收口**（`docs/reviews/s3/S3_PHASE2_CLOSEOUT.md`，Phase 1=COMPLETE、Phase 2=COMPLETE；阶段 3-5 保持导演门控，S3 整体未结束）。①**资源审计从声明文字升级为真实验证**：`ContentResourceAuditContracts` 对 Runtime 全部 Resources 入口（DarkKnightView 玩家预制体 / AudioEvents 5 键 SFX / VoiceCues 3 键人声）按同 key/同类型/同拼接语义真实 `Resources.Load` + AssetDatabase 真实资产路径；分类契约 REQUIRED（缺=审计红：玩家预制体+5 SFX，当前 6/6 PASS）/ GATED（人声 3 键，缺=如实记录不失败，当前 3 GATED-MISSING）/ OPTIONAL（无）；VFX=声明引用 0 → N/A（与资源缺失是两个状态）；同步修掉旧报告「missingAudio 假状态」（硬编码事件名被当缺失打印——已删，改真实结果表）。②**Tag 审计升级**：`SkillTagGolden`（人工钉死 3 技能 mask，独立 oracle）+ `ContentAuditTagRules`（Rule A 未声明 bit / Rule B golden parity / Rule C Attack+Spell 当前形态冲突 / Rule D Melee+Projectile 当前形态冲突 / Rule E 死 Tagged Modifier=全内容库 RequiredTags 必须至少被一个 Active Skill 满足）+ 负向测试（合成坏输入 3 例全被拒绝）；当前死 Tagged Modifier=0（4 条全部可达）。③内容 Count 全轴冻结 delta=0（护栏断言）。Runtime 零改动（仅测试侧 + 文档）。EditMode 96/96、PlayMode 3/3。报告 `CONTENT_AUDIT_S3_CLOSEOUT.md`（BATCH1/R2 保留历史）。
- S3-M1-REPO-TRUTH-CATALOG（2026-09-07，规划 AI 工作令「维护轮」）：①**长期文档规则（稳定契约 vs 当前快照）**：易变化的当前内容数量与资源 present/missing 状态，以最新测试生成 Content Audit / STATUS 为当前快照；RUNTIME 主要保存接口与行为契约，避免在多处复制易漂移数字；历史报告（BATCH1/R2 审计与复核、S2/S2P 报告、DECISIONS 日期条目）是当轮快照，不因当前状态现代化。②**Repository 事实收敛**：RUNTIME 修正 Support（7，含火焰转化机制事实）/Affix（13，单行/双行语义）/音频（S1 历史标注+查找契约+当前 5 SFX REQUIRED PASS、人声 GATED 0/3）；STATUS 音频节、ROADMAP 门控表现状列同步。③**SupportCatalog 不变量硬化**：`SupportId` 增 sentinel `Count`（不算内容、不进 UI/golden/审计清单）；`SupportCatalog.Count=(int)SupportId.Count-1`、`_defs` 容量=`(int)SupportId.Count`——手写 7/8 双 magic number 清零；Get(None/sentinel/非法) 全 default；目录连续、golden 全覆盖、既有身份零变化（4 项不变量测试锁定）。Runtime gameplay 行为零变化（仅计数/容量推导方式）。EditMode 100/100、PlayMode 3/3。复核 `S3_M1_REPO_TRUTH_REVIEW.md` verdict=**PASS**；事实收敛记录 `S3_M1_REPO_TRUTH_AUDIT.md`。
- S3-M2-RESOURCE-CONTRACT-TRUTH（2026-09-08，规划 AI 工作令「维护轮」）：①**Runtime 资源路径拼接单一真相源**——新增 `RuntimeResourcePaths`（纯静态：`PlayerDarkKnight` 常量 + `CombatSfx(key)`/`Voice(key)` 拼接，无管理器/缓存/Addressables）；全 Runtime 恰 3 处 `Resources.Load`（AudioEvents/VoiceCues/DarkKnightView）全部经它取路径；AudioEvents/VoiceCues 查找表由各自 `DeclaredKeys`（只读声明键集）单一键源初始化，`DarkKnightView.ResourcesNameDarkKnight` 保留为 alias 指向 canonical（无第二份 literal）。②**长期规则：资源审计双真相分离**——审计期望键集与 REQUIRED/GATED 分类保持**独立 oracle**（人工钉死，禁止由 Runtime DeclaredKeys 自动生成）；declared-key parity 测试（`ResourceContractParityTests` 5 测含 3 负向）防 Runtime 未审批增删资源入口：私加键/删键/改路径拼接 → parity 红或真实加载红。REQUIRED 真实加载（Resources.Load + AssetDatabase 真实路径）6/6 PASS 未因共享 path builder 削弱；GATED 人声 3 键 0/3 缺失如实记录。③S3_PLAN 历史时态对齐：页首加当前状态（Phase 1/2=COMPLETE、3-5=导演门控、S3 未结束）；阶段 1/2 内联旧状态改「原始依赖（历史）——已执行」；第一批允许清单标注「历史开工范围 · 已执行」。内容全轴 delta=0；gameplay 行为零变化。EditMode 105/105、PlayMode 3/3。复核 `S3_M2_RESOURCE_CONTRACT_REVIEW.md` verdict=**PASS**。
- S3-M3-AUDIT-FAILSAFE-TRUTH（2026-09-08，规划 AI 工作令「维护轮」）：**长期规则：当前测试生成的 Content Audit 必须 failure-safe——语义顺序 Collect → Render → Persist → Assert，报告持久化先于最终测试断言；测试红 ⇒ 仓库当前快照同轮红，失败状态不得遗留上一轮 PASS**。实现：`ContentAuditFailsafe`（ExecuteAudit/FinalizeAudit/AssertAuditResult）+ `ContentAuditResult`（全失败类别集合，HasFailures/FailureCount 单一真值）；Collect 阶段零 NUnit 断言（原 25+ 处 Persist 前直抛断言全部核入集合，新增「结构/契约问题」类别：目录条目 Name/数量护栏/FireConversion 契约/图连通/技能与怪参数/MapAffix）；审计基础设施异常先尽力写 FAIL 快照（Audit completed: NO + Execution error）再上抛，写盘失败直接红不吞异常；报告顶部加 Verdict 块（completed/verdict/failure count，Completed=false 绝不 PASS），渲染纯函数 byte 级确定性（正式报告连跑两遍 sha256 一致实测）。8 条 failure-safe 契约测试（合成输入+临时路径：渲染确定性×2/Synthetic PASS/Synthetic FAIL/Finalize 先写后断言/基础设施快照/写失败红）。文件名保留 `CONTENT_AUDIT_S3_CLOSEOUT.md`=当前测试再生快照职责（BATCH1/R2 为冻结历史 evidence）。Runtime 零改动、内容全轴 delta=0。EditMode 113/113、PlayMode 3/3。复核 `S3_M3_AUDIT_FAILSAFE_REVIEW.md` verdict=**PASS**。
- S3-M4-UNATTENDED-VERIFY-GATE（2026-09-08，规划 AI 工作令「维护轮」）：**长期规则：本地无人值守 Integration Gate 由仓库内 canonical command 执行（`tools/verify_unattended.ps1`，QA 契约 `docs/qa/UNATTENDED_VERIFICATION.md`）；外部 Agent 工具（pipeline CLI 等）可以辅助运行，但不得成为唯一可复现验证入口**。Gate=Unity 定位（显式 -UnityPath→env UNITY_EDITOR→ProjectVersion.txt 精确版本→Hub 默认根+secondaryInstallPath.json，找不到 fail fast 不交互不下载）→ 编辑器锁 fail fast（PROJECT_ALREADY_OPEN，绝不杀外部 Unity 进程）→ EditMode 全套 → 基础设施仍允许时 PlayMode 全套（Edit 测试红≠跳过 Play；仅基础设施崩才跳过并如实记录，不伪造第二套证据）→ XML 动态解析（不硬编码数量、不单独信任 Unity 退出码，exit 与 XML 双证）→ CONTENT_AUDIT freshness+verdict 双检查（fresh=本轮重新落盘，写盘时间判定，hash 不变是预期；旧 PASS snapshot 骗不过 Gate）→ 统一单次判定退出（PASS=0/FAIL=1/Unity 缺失=2/项目占用=3）。套件级超时（默认 20 分钟，只杀自己启动的 child）；结果全部进 `%TEMP%\GAME-ZZZ-UnattendedGate`（仓库零污染，不靠 .gitignore 掩盖）；`-SelfTest` 合成夹具自证（无 Pester 无新依赖）；Gate 只验证不修复（不改 STATUS/ROADMAP/Catalog/代码，不跑 git）。一次性归一化：S0 遗留孤儿 define `SENTIS_ANALYTICS_ENABLED`（无包属、全工程 0 引用）被 Unity 6000.3 批量实例确定性清理（复现实验证实每次批量运行必清理，自定义 define 保留=清理具选择性、终态稳定）——提交该稳定终态使 canonical Gate 零仓库污染。Runtime 零改动、内容全轴 delta=0。Gate canonical 连跑 3 次全 PASS（EditMode 113/113 + PlayMode 3/3，CLOSEOUT sha256 三次一致）。复核 `S3_M4_UNATTENDED_GATE_REVIEW.md` verdict=**PASS**。
- S3-M5-WIN64-PLAYER-BUILD-GATE（2026-09-08，规划 AI 工作令「维护轮」）：**长期规则①：Editor 测试绿 ≠ Player build 绿——Windows-only 项目的完整 Integration evidence 必须包含可重复的 StandaloneWindows64 build validation**；**长期规则②：Player build 产物为 ephemeral，不入仓库**。实现：canonical Gate 增 `-IncludeBuild`（Full=Quick+win64 构建，`-BuildTimeoutMinutes` 默认 30，复用 M4 全部基础设施零第二套）；构建=Unity 官方 CLI `-batchmode -quit -buildTarget win64 -buildWindows64Player`（实测 6000.3.23f1），输出 `%TEMP%\GAME-ZZZ-UnattendedGate\PlayerBuild\`；双证据判定（进程正常+exe 非空+`<exe>_Data` 非空，不硬编码后端产物）；普通测试失败仍构建（一次收齐证据），基础设施失败 Build=NOT RUN 不伪造 PASS；SelfTest 增 5 构件夹具共 14 项；Quick 摘要明示 `PlayerBuild: SKIPPED (use -IncludeBuild)`。**BuildSettings 契约锁定**（`BuildSettingsContractTests` 2 测，EditorBuildSettings API）：Bootstrap=index 0 / Arena=index 1（启用+资产存在、无重复、各恰一次）——ArenaPerfHarness `SceneManager.LoadScene(1)` 的隐藏依赖，防「测试全绿+可 build 但 Harness 启动错场景」；本轮不改 Harness（锁现状，变更需另行工作令同步）。**设置资产归一化与 define 振荡根因修复**（工作令二十五允许的独立记录）：构建实例（-quit 会保存）触发引擎归一化 URP/设置资产——幂等性实证后提交 canonical 态（b79c77c）；随后发现 SENTIS_ANALYTICS_ENABLED 振荡（`com.unity.ai.inference` 间接依赖包 AnalyticsDefineManager 按 EditorAnalytics.enabled 加删，机器级同意值跨实例类型不稳定→任一 HEAD 态都无法双 Gate 同时零污染）→ 根因修复 `submitAnalytics: 1→0`（项目分析提交退出→EditorAnalytics.enabled 恒 false→define 恒缺席，3de5d24）→ 修复后 Quick+Full×2 全部零新增 mutation。Runtime 零改动（ArenaPerfHarness 零触碰）、内容全轴 delta=0、不启动 Player、不做性能阈值。Full Gate 6 次运行全 PASS（115/115+3/3+win64 exe 667136 bytes/Data 162 文件）。复核 `S3_M5_WIN64_BUILD_GATE_REVIEW.md` verdict=**PASS**。
- S3-M6-PLAYER-RUNTIME-TRUTH（2026-09-08，规划 AI 工作令「维护轮」）：**长期规则③：Build success ≠ Player runtime success——涉及 Runtime/Scene/Resources/ProjectSettings 的完整无人值守证据，应能启动本轮刚构建的 Player 并完成最小 Player-run contract**；**长期规则④：Arena harness 分辨率证据必须来自 `Screen.width/height` 实测，不得由测试名/目录名/请求值推断**。实现：canonical Gate 第三层 `-IncludePlayerRun`（自动隐含 -IncludeBuild；Quick 与 Build Gate 行为不变）——TempRoot 每轮清空→本轮构建产物立即以 `-arenaPerf -arenaPerfOut <temp>\PlayerRun` 启动（`-PlayerRunTimeoutMinutes` 默认 10，只杀自己 child）→Player 三档跑完自行退出 exit=0。**证据契约（QA golden 钉死 100/200/300，不从输出枚举）**：三份文件在指定目录（fallback 别处=FAIL）+header density 匹配+`resolution=WxH` 合法且三档一致+`editor=False`（True 即 FAIL=Editor 冒充）+`dx=` 非空+恰 1 行 13 列+dummy==密度+alive>0 且≤dummy（300 档 290/300 为合法 top-up）+数值有限可解析+fto bool；**无性能阈值**，控制台恒输出 `PerformanceVerdict: NOT_EVALUATED`。**Harness 证据真实性修正（仅 2 行元数据，行为零变化）**：标题 `# S2P 1440p harness`→中性 `# ArenaPerfHarness, density=<n>`（不再声明未实测分辨率）；删除 `enteredMap=` 写死常量假证据字段（工作令二十四）。历史 S2P 文件冻结零改动；S2P_CLOSEOUT 补一句消歧注（历史标题 vs resolution= 实测为准）。SelfTest 22 项（+8 证据解析夹具）。实测：Player Runtime Gate×2 全 PASS（115/115+3/3+build+PlayerRun exit=0，实测分辨率 2560x1440/DX12——仅记录，1440p/120 维持未结案）。复核 `S3_M6_PLAYER_RUNTIME_TRUTH_REVIEW.md` verdict=**PASS**。
- S3-M7-1440P-PERFORMANCE-GATE（2026-09-08，规划 AI 工作令「维护轮」，Owner：I=实现/R=A15 复核）：**长期规则⑤：1440p/120 Performance Gate 的 machine-readable contract 归仓库（`docs/qa/PERFORMANCE_GATE.json` 唯一真相源，脚本/文档不得复制第二套 2560/1440/8.33/3-runs）**；**⑥：请求分辨率不是证据，实际 `Screen.width/height` 才是证据**；**⑦：性能 baseline 锁 CPU/GPU（probe 实测回填 contract），硬件变更需正式工作令显式更新、禁止 Gate 自动学习**；**⑧：Performance FAIL（环境正确的真实超预算）时停止 Content Expansion（CONTENT EXPANSION FROZEN BY PERFORMANCE GATE），不得靠降低 workload 让 Gate 变绿**。实现：canonical Gate 第四层 `-IncludePerformance`（隐含 PlayerRun/Build；同一 Build 只构建一次，连续 3 次独立 Player 运行，输出目录逐轮清空）；Player 启动参数 `-screen-width/-height/-fullscreen/-screen-quality/-force-d3d12 -arenaPerf -arenaPerfGate`；`-arenaPerfGate` 唯一作用=运行期钉死 `vSyncCount=0`+`targetFrameRate=-1`（解除帧率限制非作弊）；Harness 增只读证据行 `hardware_cpu/hardware_gpu/perf_env(quality/vsync/targetFps/warmup/sample/castInterval 实测值)`（工作量零变化）。硬指标：main avg+p99≤8.33（p99 为正式硬门）、cpu/gpu avg>0 且≤8.33、frame_timing 必须 available（不可用=EVIDENCE_INCOMPLETE 非 PERF_FAIL）、alive≥95%（300 档≥285）、frames==600；max/GC/memory 只记录。Verdict 语义六态（NOT_EVALUATED/PASS/FAIL/ENV_NOT_MET/EVIDENCE_INCOMPLETE/INFRA），仅 Performance 层允许 PASS；只有 verdict=PASS 才允许总 Gate exit 0。评估器纯函数（contract+parsed results→verdict+reasons），SelfTest 36 项（contract 单一来源加载；评估夹具硬件合成）。**probe→锁定**：实测 CPU=AMD Ryzen 7 5700X3D 8-Core / GPU=NVIDIA GeForce RTX 5070 回填 contract。**实测结果：2 个 canonical Gate × 3 Run × 3 Density=18 次测量全部 PASS**（环境 2560×1440/D3D12/PC/vSync0/targetFps-1 全对齐；worst main avg=1.676ms、worst p99=2.388ms=预算 28.7%、cpu≤1.676、gpu≤0.579、GC=0.0、FrameTiming 18/18）——**S2P 1440p/120 正式 CLOSED（锁定硬件语义）**，冻结证据 `docs/reviews/s2p/1440p-120-m7/`（SUMMARY+contract 快照+gate-b 完整原始文件）；历史 1080p 证据不动；结论边界=仅锁定硬件与 canonical 环境，不构成「所有 Windows PC 都保证 120FPS」。顺带修复两个无人值守健壮性缺口：PlayMode 跳过分支 $playInfra 未赋值导致 $null 绑定 [bool] 参数崩溃（首次暴露）；崩溃残留僵死锁 UnityLockfile 在无 Unity 进程时安全清理（有进程一律 fail fast 不杀）。复核 `S3_M7_1440P_PERFORMANCE_GATE_REVIEW.md` verdict=**PASS**。
- S3-M8-PERF-EVIDENCE-INTEGRITY（2026-09-08，规划 AI 工作令「维护轮」）：**长期规则⑨：Verifier ≠ Historian——canonical Performance Gate 只负责验证与 temp 输出，永不直接写冻结历史证据；正式 closeout/revalidation 的每个 canonical Gate raw evidence 必须在下一次 Gate 清空 temp 之前，经显式 Operator 工具（`tools/snapshot_performance_evidence.ps1`）保存为冻结归档，并以 `MANIFEST.json`（逐文件 SHA-256+bytes+sourceCommit）验证完整性**。实现要点：快照 Operator 拒收非 PASS（FAIL/ENV_NOT_MET/EVIDENCE_INCOMPLETE/INFRA/NOT_EVALUATED）、拒收缺 run/缺密度档/缺 PlayerRun.log、拒收证据头与 contract 不匹配（逐字段 density/resolution/editor=False/dx/hardware_cpu+gpu/quality/vsync/targetFps/warmup/sample/castInterval/13 列）、拒收 destination 已存在（无 -Force，冻结证据 append-by-new-directory）、拒收 Assets/Runtime、ProjectSettings、docs/qa/PERFORMANCE_GATE.json 未提交修改（docs/tools 脏允许）；只复制 12 个原始 run 文件+verification-summary.json+契约快照，永不复制 Player 构建产物（exe/Data/pdb 不入库）；`-VerifyArchive` 只读校验（存在性+字节数+SHA-256+契约快照完整）。仓库真相同步：STATUS 测量段、S3_PLAN Phase 6+7、ROADMAP S2P 行+checklist+导演门控表全部对齐 current-state「1440p/120 已由 M7 CLOSED（锁定硬件）」；M5/M6 时代历史行加「当时状态」标注、不改写历史、不伪造历史 Gate A raw。Snapshot SelfTest 9/9。
- S3-M9-INFERENCE-DEPENDENCY-CLOSEOUT（2026-09-08，规划 AI 工作令「无人值守维护序列最终根因项」）：**长期规则⑩：Package removal 必须先有 usage + reverse-dependency audit（代码 API/模型资产/asmdef/lock 反向依赖逐项实证），不得以「看起来没用」为依据**；并纠正历史失实表述=com.unity.ai.inference 自 S0 d5beb7f 起即为 manifest 直接依赖（M4「manifest 无 sentis 包」与 M5/M8 时代「间接依赖」均失实，历史文本按冻结原则不改写，以 `S3_M9_INFERENCE_DEPENDENCY_AUDIT.md` 为准）。执行：零使用审计全过（API 0 命中含 Tensor/Worker/Functional 词边界甄别/模型资产 0/asmdef 0 引用/lock 反向依赖 0/SENTIS 条件编译依赖 0）→决策 SAFE_TO_REMOVE→最小删除（manifest 仅删一条；UPM 正常 resolve：lock 实际 delta=移除 Inference+dt.app-ui 传递，burst/collections 由 render-pipelines.core 保活、newtonsoft-json 由 Assistant 保活，零意外版本漂移）→ProjectSettings canonical 一次性删 SENTIS define（根因=`AnalyticsDefineManager.cs` [InitializeOnLoadMethod] 按跨实例不稳定的 EditorAnalytics.enabled 加删 define；包移除后不再运行=振荡从机制根除）→五门矩阵（SelfTest 9/9、Quick、Build、Quick-after-Build、PlayerRuntime、Performance 回归）全 PASS+每步 tracked tree clean+define 0 变动；Performance 回归 9/9 PASS（worst avg=1.660ms=19.9%、p99=2.376ms=28.5%，contract/workload 零 diff）；Player Data 162→158 观测性瘦身（非成功标准）；Assistant 及其它「看似未用」依赖一律未动（工作令四十九）；Content delta=0；S2P 维持 CLOSED。**SENTIS define 振荡 CLOSED；无人值守技术维护序列正式结束（GATE-READY / AWAITING NEW PRODUCT DIRECTION），不自行创建 M10；Phase 3/4/5 仍导演门控**。复核 `S3_M9_INFERENCE_DEPENDENCY_REVIEW.md` verdict=**PASS**。
- S3-P3-UI-R3-TOOLTIP-COMPARE（2026-09-08，规划 AI 工作令三十二落实导演裁定）：**长期规则⑪：素材授权问题不再是 Gate——导演已明确（2026-09-08）：当前仅团队内部使用、仓库属内部仓库、现有素材均已取得作者/商业授权或线下合作授权；执行 AI 后续不得再因许可证问题停住素材工作**。素材取舍仍按产品/风格裁定执行（授权解决≠本轮必导素材；R3 因工作范围 Art asset delta=0，与许可无关）。

## 工作方式（导演 2026-09-07 追加）

- 本项目全程无人监管，工作 AI 完全自动根据 GPT（协调席）的指示开展工作；同一 AI 轮流担任 I（实现）与 R（审核）。
- 遇到阻塞项：记录下来（写入当前报告/文档），立即转做其它可推进的工作；**不允许停手等待**。
- 每次向 GPT 汇报时，附带上述两条要求。
- S1 手感门：导演 2026-09-07 人工复测通过（按住走、按住连发、Q/E 施放+命中反馈），**手感门正式关闭**（并入上方 S2P 收口条目）。
- S2 玩家模型外观残留：导演复测确认「身高/缩放有问题——人物模型看不清、明显小于敌人模型」，列为待调项（视图层），调后无需重开全包。**已修正（2026-09-07）**：根因是挂载时一次性贴地补偿在动画播放后失准，身体下沉约 0.70 埋入地下（实际可见仅约 0.49）。修正为按身体包围盒（排除武器）每帧贴地（`DarkKnightView.UpdateGround` + `ArenaDirector` 视图更新调用）；修正后身体净高 1.19（普通怪视觉 0.58、精英监守 1.17），同框截图确认清晰可辨。待复核。

## 工程

- 单机 · Windows · Unity 6000.3 · URP（不改 Built-in / HDRP）。
- 开发根目录：`unity/game/New Unity Project/`。
- 业务程序集只允许：`Game.Runtime.Core`、`Game.Runtime.Content`。不创建 Combat / Skill / Items / Maps / AI / Presentation 空壳程序集。
- 测试程序集：`Game.Tests.EditMode`；S1 增加 `Game.Tests.PlayMode`（非业务）。
- 玩法与内容随机禁止 `UnityEngine.Random`。只用 `SeededRng`。
- 接口以 `docs/RUNTIME.md` 为准；代码与文档不一致则同一次改文档。

## S0 范围

做：目录、Asmdef、Bootstrap + 空 Arena、GameLog、SeededRng、顶层 JSON Import/校验、EditMode 测试。

不做：点地移动、技能、伤害、UI、300 怪、DOTS/Entities、Jobs/Burst、玩法内容。

S0 已放行。S0 模块（SeededRng、GameLog、ContentDatabase、ContentId、ContentRecord、ContentPaths、BootstrapRunner）不得重写。

## 内容数据

- 文本数据在工程根 `ContentData/`，不进 `Assets/`。
- Bootstrap 默认只加载 `ContentData/valid/`。
- `ContentData/invalid/` 只给测试和人工复现。
- Import 是全成或全败：任何错误则 `db` 空，禁止部分成功。

## S1 范围

做：点地移动、朝向、攻击距离、接近、同一套施放流程、取消窗口、输入缓冲、近战/弹道/范围各一、Dummy 100/200/300、简单转向、AI LOD、对象池命中/死亡反馈、占位动画、音频事件名、PerformanceArena 测量。

不做：装备、天赋、Craft、Unique、完整 Combat Math、地图词缀、Jobs/Burst/DOTS、Socket、锁 Build。

- S1 玩法进 `Game.Runtime.Core/Gameplay/`，不新增业务程序集。
- 命中 = 固定扣血。Dummy Hp=2。无护甲/抗性/暴击/转化。
- 弹道运动学 + 对象池，禁止默认 Rigidbody。
- Dummy AI 禁止「300 × 每帧完整 NavMeshAgent」作为方案。
- `COMBAT_MATH.md` 保持未做，禁止假写公式。

## S1 放行

S1 技术已放行。输入、对象池、SeededRng、点地移动、Q/W/E 施放流程不得重写。

## S2 范围

做：Tag/Stat/Modifier/Condition/Effect/Event/Trigger 内核；Combat Math 子集；Q/W/E 同一结算；6 Support + Socket/Link（无孔色）；4 装备槽 Ordinary/Rare ≥10 Affix；随机 Craft + 定向 Craft；16 节点天赋（2 Notable + 1 机制）；3 普通 + 1 Elite；1 图 3 词缀 + Stability + Reward + 进图快照锁；从 Arena 进图。

不做：技能库、大天赋树、Unique、完整 Craft 链、孔色、DOTS、S2P 120FPS、大主菜单、第四个业务程序集（Combat 未拆，公式在 Core/Gameplay）。

机制 Support 锁定为 **Fork**（W 弹道命中后分裂成 2）。天赋机制节点锁定为 **Cinder Heart**（40% 物理转火）。DoT 锁定为 Ignite。Conversion 锁定为 Physical → Fire。

图内不可改 Build。死亡出图免费 Respec。随机只走 SeededRng。内容是数据表（`Catalogs.cs`），不在运行时发明新机制。

## S2 UI

导演打回：补可评价界面，不改数值与结算。玩家可见描述一律中文。死亡后不得卡死。稳定度随死亡/清场下降。

**HUD 未完备**：角色背包、技能、装备、天赋 HUD 后补，当前不得当作已经做完。禁止新技能/词缀/天赋/Unique/Atlas/DOTS/S2P/精模。

## 镜头 / 体型（表现）

高速 ARPG 读图：相机拉远，人/怪视图缩小。不改速度、攻击距离、命中、分离。命中仍是距离 / 圈 / 弹道。数值见 `RUNTIME.md` 碰撞节后的相机行。

## 换模

自制 Eve FBX / 预制体 / Mixamo 导出已从工程剔除（蒙皮损坏、双骨架、单位错误，不可用）。玩家暂回 S1 胶囊。换模等导演提供可用成品后再接。不改按住移动、QWE、结算、掉落。不上 HDRP / DOTS。

2026-09-06 已接导演提供的测试成品：黑色沙漠 Dark Knight 包（`unity/game/测试资源，确认后进入项目`）。只取 `Dark Knight.FBX`（176 骨，BDO Biped）+ DDS 转 PNG 贴图 + 4 条动画 FBX。接入方式：Humanoid 重定向（`DarkKnightAvatar`），预制体 `Assets/Resources/Player/DarkKnight.prefab`，由 `EveView.TryMount` 优先加载（缺预制体回退胶囊视图）。动画只做视图层（Idle/Run/Attack/Cast 四状态控制器，`EveView.DriveEve` 驱动），不进逻辑状态机。401 个动画 FBX 中其余留档未导入。语音 ogg 留档后用（S1/S2 只有 Cast/Impact/Hit/Death 四事件）。

## 碰撞（锁定）

- 玩家与怪物无碰撞，互相穿过，不推开。
- 怪物与怪物无物理挤开；只保留约 1 单位最小间隔，防止叠点。
- 地面 / 墙 / 边界仍要站住（平面 `ClampPlane` + 地面 MeshCollider）。
- 命中继续用距离 / 圈 / 弹道。禁止改成碰到胶囊才算打中。
- 玩家与怪不要物理互推：分层忽略（Player / Monster）。
- 怪与怪禁止 Rigidbody 互撞、禁止 CharacterController 互推；在 `DummyCrowd.Separate`（AI 转向之后）距离 < 1 就水平拨开。
- 无碰撞不等于打不中。走进怪群按 QWE 必须仍能命中。
