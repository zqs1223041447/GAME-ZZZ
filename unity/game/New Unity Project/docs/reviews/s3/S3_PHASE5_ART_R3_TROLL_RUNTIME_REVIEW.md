# S3-PHASE5-ART-R3 TROLL RUNTIME INTEGRATION 复核与收口报告

工作令：S3-P5-ART-R3-TROLL-RUNTIME-INTEGRATION（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。

## 0. Verdict

**PASS — TROLL DEFAULT ENEMY VISUAL INTEGRATED**（若 Performance 或 QA 环节有保留，见 §17 与 §24 如实记录，不改本 verdict 的成分）。

## 1. Baseline HEAD

- Baseline=59229be（Phase 5 Art Trial R2 收口=STATUS 回填 HEAD）。
- 开工授权：导演 2026-09-08 核心规则追加「无人值守，自动执行 GPT 下发的工作安排」（DECISIONS 工作方式节）；工作令由协调席对话直接下发（memofun 会话 6a9ed34a，回复存档 `.grok/tmp/memofun_next_plan15.txt`）。

## 2. Planner Art-Direction Verdict（转述工作令）

- R2 判定 PASS；**Troll_2 所代表的 PBR 较写实方向=普通/精英敌人后续选模的默认视觉锚点**；Bruce=高价值/Boss/特殊怪角色化候选（不删除不返工）。此为美术角色分层，**不新增 gameplay 敌人分类**。

## 3. Existing Enemy Visual Architecture（§4 审计，先审后改）

- **敌人=纯数据**：`DummyCrowd.Items[]`（`Dummy` 结构池，`DummyPoolSize=300`），无 GameObject；AI=`TickAi`（LOD 分带），动画状态=枚举字段 `d.Anim`（Idle/Run/Hit/Death）。
- **视图=ArenaDirector**：Build 时预建 300 个基元 GameObject（Cube/Sphere/Cylinder/Capsule 按 kind，MeshFilter+MeshRenderer 直接在根上）；`SyncViews()` 每帧数据→transform/颜色挤压动画（`ApplyAnimVisual`）。**gameplay root 与视觉在同一 GO（无分离）**。
- **可观察状态**：`d.Anim`（Idle/Run/Hit/Death）、`d.HitFlash`（0.20s）、死亡回收 `DeathRecycle=0.40s`、攻击=「`AttackReady` 置位→`ArenaSim.ResolveEnemyAttacks` 消费并结算」（canonical attack event，此前无视图可观察点）。
- **Harness 关系**：`ArenaPerfHarness.SampleDensity` 与正式敌人共用同一 Sim+SyncViews+基元视觉池（kind=Dummy）。
- **玩家先例**：`DarkKnightView.TryMount`（prefab 挂 gameplay root 下、剥物理、贴地）+`DriveDarkKnight`（读逻辑状态/HitFlash 上跳沿/MapState.Dead 驱动 Animator）——本轮敌人视觉按同构思路落地。

## 4. Branch B：新增通用表现层（§6）

- 仓库当前无「visual child / serialized prefab renderer attachment」类敌人视觉挂点（基元渲染与 gameplay root 同 GO）→ 走 Branch B：新增**最小、通用、无 gameplay 权限**的表现层。
- **`EnemyVisualPresenter`**（纯 C# 类，非 MonoBehaviour，不注册 Update）：持有视觉预制体实例与 Animator；输入=视图层读到的通用 gameplay 状态 `(alive, AnimState, HitFlash, AttackExecutions, time)`；输出=Animator.Play 状态选择。禁止/不具备：AI、伤害、速度、死亡条件、目标选择、攻击冷却、任何 gameplay 写 API。
- **攻击信号**：`Dummy` 新增观察字段 `AttackExecutions`（ArenaSim 消费 AttackReady 时 +1，单调、不参与任何 gameplay 判定）——攻击动画由真实攻击执行事件驱动，表现层不自判距离/时机（§16）。

## 5. Runtime Visual Target（§7/§8）

- **目标=EnemyKind.Brute（普通近战怪，SpawnMap 每图 8 只，无特殊机制）**：地图默认敌人中最基础、最常见、无特殊机制的一类；与初筛报告「Troll_2 覆盖近战重甲代表怪位」一致。
- gameplay 身份保持 Brute：EnemyId/数值/AI/掉落/波次零改动；**不为 Troll 创造新敌人类型**。
- 其余 kind（Stinger/Ashling/Warden/Dummy）保持基元视觉；**Dummy（Harness 密度档）不换装**（有 PlayMode 测试钉住）。

## 6. Visual Prefab（§9）

- `Assets/Resources/Enemies/TrollWarriorVisual.prefab`：由 R2 已验收的 TrollWarriorTrial.prefab 复制派生（Trial 本就满足「Transform+Animator+3 SMR、vendor script=0/collider=0/Rigidbody=0」）；Trial 历史资产原样保留。资源路径经 `RuntimeResourcePaths.EnemyTrollVisual` 单一真相源（S3-M2 契约延伸）。
- 组件审计测试钉住：MonoBehaviour=0、Collider=0、Rigidbody=0、Camera/Light=0、3 SMR 网格+材质全挂、Animator 6 状态（Idle/Walk/Run/Attack/Hit/Death）。

## 7. Gameplay Root 与 Visual Root 分离（§10）

- 表现层实例以 `localPosition/localRotation/localScale=恒等` 挂在 Dummy gameplay root 下（`VisualRoot`）；**美术 scale（预制体自带 2.5047→2.35m）不作用于 gameplay root**（root scale 恒 1，挂载与运行时均有断言）。
- 基元 placeholder 在表现层激活期间隐藏（MeshRenderer.enabled=false），槽位换 kind/回收时恢复——Dummy 池槽位复用语义不变。

## 8. Scale（§11）

- 沿用 R2 已验收视觉尺寸：**世界可见高 ≈2.35m（≈DarkKnight 1.19 的 1.8-2.0×）**；本轮未做 visual scale 调整（如 QA 发现问题再按 before/after/reason 记录）。

## 9. Ground Contact（§12）

- 贴地=R2 验证的预制体自带 offset（child y=0.034、MINY≈0.000）；gameplay root y=0，无逐帧贴地（300 槽位逐帧 bounds 开销不成比例；R2 静态 offset 在跑/攻/死动画下已实测贴地）。正式相机五态截图复验见 §20。

## 10. Animation Hookup（§13-§21）

- Root Motion=off（预制体继承 R2 配置+挂载时再强制）。
- 状态映射（全部来自已有 gameplay 信号）：
  - **Idle**：`d.Anim==Idle`；
  - **Move→Run**：`d.Anim==Run`（真实移动状态，表现层不自算速度）；
  - **Attack**：`d.AttackExecutions` 序号边沿（=ArenaSim 真实攻击执行）；动作窗口=Attack clip 长，窗口内不被移动姿态覆盖；
  - **Hit**：`d.HitFlash>0 且 d.Anim==Hit`（既有受击信号）；
  - **Death**：`alive=false` 边沿（既有死亡状态）。
- **新 gameplay 状态引入=0**（仅新增只读观察计数）。

## 11. Runtime 限制如实记录（§17/§18）

- **HIT 动画窗口**：HitFlash 0.20s < Beaten clip 时长，无迭代问题；但若 HitFlash 跨越 Hit 窗口边缘存在一次重启可能（表现层纯视觉细节，不影响判定）。
- **DEATH_VISUAL_TRUNCATED_BY_RECYCLE**：gameplay 死亡回收 `DeathRecycle=0.40s` 远短于 Death clip——视觉只播 0.40s 即随槽位回收隐藏；**按工作令不改 gameplay lifetime**，Dead clip 保持有效、测试继续钉住。
- **IGNITE_TINT_NOT_PRESENTED**：基元视觉的点燃染色（AshColor lerp）未迁移到 PBR 视觉（SMR 材质染色留待表现增强轮，非本轮范围）。

## 12. Tests（§29）

- EditMode 158→**169（+11）**：新文件 `TrollRuntimeVisualContractTests`——预制体契约路径加载/组件审计（零 MonoBehaviour·Collider·Rigidbody·Camera·Light+3 SMR 全挂）/Animator 6 状态/挂载不动 gameplay root（scale/pos/rot 断言）/移动映射纯函数/Idle·Run 跟随/攻击边沿=执行序号（基线不误报+窗口保持+窗口外恢复）/Hit 边沿/Death 对齐回收/gameplay 常量护栏（PoolSize=300·DeathRecycle=0.40·HitFlash=0.20）/URP 材质无 Error shader。
- 既有 oracle 同步：`RuntimeResourcePaths.EnemyTrollVisual` 进路径契约测试；资源审计契约+1（Enemy 域 REQUIRED 恰 1 条，parity 测试钉住）。
- PlayMode 3→**5（+2）**：新文件 `TrollRuntimeVisualPlayModeTests`——①进图后 Brute 槽位挂正式视觉+placeholder 隐藏+root scale=1+移动出现 Run+攻击执行出现 Attack+ApplyHit 后 AliveCount-1 且视觉播 Death+0.40s 回收后视觉隐藏；②Dummy kind（Harness 路径）保持基元视觉不挂 presenter。
- 既有 Bruce/Troll trial 测试全部继续 PASS（158/158 内含）。

## 13. Content Audit（§31）

- PASS fresh（本轮 Gate 真实再生）；**Content delta=0**（新增 prefab/材质属 Art integration，非新 Enemy 内容定义）；资源契约 REQUIRED 6/6→**7/7**（+敌人视觉预制体，真实 Resources.Load PASS）。

## 14. Build Inclusion（§32）

- Player Build PASS（win64，exe 667136 bytes / Data 158 files——文件数与 R2 轮一致）。
- **Troll 进入构建硬证据**：`resources.assets` 内检索到 `TrollWarriorVisual`（预制体）、`Troll_2_L`（网格）、`Troll_2_D`/`Sword_D`（材质）资产名；`resources.assets.resS`=33.26MB（Troll FBX 网格+压缩贴图为新增主体）。
- Player 实际渲染 Troll 的最强证据见 §20（真实 gameplay 相机截图）。

## 15. Player Runtime Gate（§33）

- `verify_unattended.ps1 -SelfTest`：38 项 PASS。
- `-IncludePlayerRun`：**全 PASS exit=0**——EditMode 169/169 + PlayMode 5/5 + Audit fresh PASS + win64 Build + PlayerRun 三档 100/200/300 exit=0（分辨率 2560x1440 实测）；tracked tree clean（提交后）。

## 16. Performance Gate（§34-§38）

- `-IncludePerformance`：contract/workload 零改动（2560×1440/D3D12/PC/vSync0/targetFps-1/锁定硬件 Ryzen 7 5700X3D + RTX 5070/3 canonical runs/预算 8.33ms）。
- **结果：PASS，3 Runs × 3 Density=9/9 测量全 PASS**——worst main avg=1.908ms（预算 22.9%）、**worst p99=2.615ms（预算 31.4%）**、worst cpu=1.908ms、worst gpu=0.572ms、alive 合规（300 档 293-295/300=top-up 合法）；不创建新 baseline（§37）。
- **Harness 代表性（§35 如实回答）**：canonical ArenaPerfHarness 密度档生成 `EnemyKind.Dummy`，本轮视觉绑定 Brute——**Harness 未使用 Troll 视觉路径（NO）**。本次 Performance PASS 证明项目全局未回归，**不得声称「300 Troll 已通过」**；记录 `TROLL_RENDER_COST_NOT_REPRESENTED_BY_CANONICAL_HARNESS`，未来如需单独立 Art density 工作令。
- §36 合规：未为 Performance PASS 做任何绕开（无 if arenaPerf 分支/无隐藏/无降质——Troll 视觉只在 Brute 槽位按真实地图规则出现，Harness 的 Dummy 假人本就非本轮目标）。

## 17. 正式 Runtime QA（§25/§26）

- 场景=真实 gameplay 相机（进图真实 SpawnMap 敌群，Troll AI 真实走近/攻击）。证据集（本地 QA 集 `Assets/Temp/qa/r3_*`，按仓库惯例不入库、评审按名引用；本轮以 gameplay 相机直渲 RenderTexture 存 PNG，含 IMGUI HUD 的合成画面为 r3_1/r3_3/r3_4 三张）：
  - **r3_1_map_entry_multitroll**：进图远景+HUD——8 只 Brute 全部 Troll 化同屏，轮廓/武器可辨，DarkKnight 居中可读，Stinger（绿球）/Ashling（橙柱）/Warden（紫胶囊）基元视觉原样保持；
  - **r3_3_darkknight_troll_close**：近距合围——巨魔≈玩家 1.8-2.0×，为大而不 Boss 化（普通怪基元 0.58 同屏对照），玩家 110/110 存活；
  - **r3_4_brutes_approach**：逼近段 Run 姿势清晰（真实 TickAi 移动信号驱动）；
  - **r3_5_attack_close**：近距攻击突进姿势+玩家受击反馈白球（Attack 状态选择另由 PlayMode 测试程序化钉住）；
  - **r3_6_death**：被击杀 Brute 的 Death 坍塌姿势（玩家身旁暗色坍塌体；0.40s 回收窗内，截断限制见 §11）；
  - **r3_5_attack**：远景战斗群（相机直渲，无 HUD 合成）。
- Hit 态未单独截图（如实记录）：Hit 与 Attack/Move 同用 0.20s HitFlash 窗口，画面区分度低；Hit 状态选择由 EditMode/PlayMode 测试断言钉住。
- QA 过程事实（如实）：①玩家会被 8 Brute 真实围杀（约 4-6s），多轮会话用于抓拍——gameplay 真实性的副产品，非回归；②Game View 在编辑器失焦后停止重绘、截到陈旧画面（R2 教训复现），本轮改用 gameplay 相机直渲 RenderTexture 绕开（合成 HUD 的三张均产生于重绘正常窗口）；③`unity command eval` 上下文把 `FindObjectOfType` obsolete 警告按编译错误处理且错误信包回显脚本文本——初期轮询的「命中」是字符串匹配假阳性，改 `FindFirstObjectByType`+done 文件标记后消除。

## 18. Scope（§48 对账）

- EnemyId delta=0；Gameplay delta=0（行为零变化，仅 +1 只读观察计数与视图层新增）；Content delta=0；UI delta=0；Audio delta=0（§40）；第三模型（Lion/TheTroll/狼/蝙蝠）未动（§46）；UI R4 未动（§41）。

## 19. Reviewer Findings

- 实现过程 1 项编译错误（新字段漏赋值 CS0165）当轮修复，无遗留。
- PlayMode 攻击断言依赖 Brute 走近+攻击 CD 的真实 AI 时间（等待上界 8s 模拟时间内出现），非逐帧脆弱断言。

## 20. Final Verdict

**PASS — TROLL DEFAULT ENEMY VISUAL INTEGRATED**（五类动画由真实 gameplay 信号驱动；Performance Gate PASS 且如实标注 Harness 代表性=NO；Death 视觉截断为 gameplay 回收语义，已记录不改）。

## 21. Phase 状态（§44/§45/§46）

- Phase 5：**IN PROGRESS — Troll default enemy visual integrated**（不写 COMPLETE：剩余角色/怪物视觉范围未规划）。
- Phase 3 UI：R1/R2/R3 COMPLETE、R4 NOT STARTED（Stage0 大天赋树禁令不变）。
- 剩余精模 Lion/TheTroll/Wolf/Bats：SCREENED / NOT STARTED。

## 22. 提交

- feat(art) integrate troll visual → test(art) cover troll runtime visual contract → docs(art) close troll runtime integration review → STATUS 回填（仓库惯例四笔；普通 push）。
