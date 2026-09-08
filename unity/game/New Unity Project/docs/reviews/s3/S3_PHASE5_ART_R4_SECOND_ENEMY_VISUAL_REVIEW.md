# S3-PHASE5-ART-R4 SECOND ENEMY VISUAL 复核与收口报告

工作令：S3-P5-ART-R4-SECOND-ENEMY-VISUAL（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。

## 0. Verdict

**PASS — SECOND ENEMY VISUAL INTEGRATED**（限制项如实记录，不改变 verdict 成分）。

## 1. Baseline HEAD

- Baseline=850f1e5（Phase 5 Art Trial R3 收口=STATUS 回填 HEAD，已 push）。

## 2. 本轮目标（§1 转述）

R3 已建立「Art asset → 默认 Runtime 敌人视觉」的通用接入缝隙；本轮回答：**该缝隙能否无 Troll 特例地承载第二种行为不同的现有敌人**。不继续改 Troll、不做第三个纯试装、不新增 EnemyKind、不改玩法。

## 3. Existing EnemyKind Role Audit（§4/§5，只读事实）

| EnemyKind | 数量/半径 | HP | 护甲 | 闪避 | 速度 | 攻击 cd | 伤害 | 行为语言 | placeholder 轮廓 |
|---|---|---|---|---|---|---|---|---|---|
| Brute 蛮兵 | 8@r7 | 36 | 80 | 10 | 1.7 | 1.15 | 物7 | 缓慢重甲近战 | 大立方（已 Troll 化） |
| Stinger 刺蜂 | 8@r11 | 20 | 0 | **140** | **3.1** | **0.85** | 物5 | 最快最脆高闪避蜂群游击 | 小球（绿） |
| Ashling 烬灵 | 6@r15 | 24 | 20 | 40 | 2.2 | 1.05 | 物3+**火5** | 中速火系 | 中圆柱（橙） |
| Warden 监守（精英） | 1@r18 | 110 | 140 | 50 | 1.9 | 0.95 | 物12+火4 | 重装精英 | 大胶囊（紫） |

攻击=全部近战直击（AttackReady→`ResolveEnemyAttacks`/`BuildEnemyHit`，无敌方弹道）；死亡回收统一 0.40s。以上为只读事实，未为素材匹配改任何数据（§4）。

## 4. Role Profile（§5，Art planning metadata，不写入 gameplay schema）

- **Stinger**：典型距离=贴脸游击；机动性=最高；攻击语言=快速连击；视觉权重建议=小型轻甲；轮廓需求=低矮、快速、与重装区分；动画需求=快步/疾跑+快速攻击+受击+死亡。
- **Ashling**：典型距离=中距；机动性=中；攻击语言=火系打击；视觉权重=中型元素体；轮廓需求=火/烬语义；动画需求=中速移动+施打+受击+死亡。
- **Warden**：重型精英；轮廓需求=威压但与 Troll 区分；本优先级最低（与已接入 Troll 位最接近）。

## 5. Remaining Asset Screening（§7，源文件层轻量盘点）

| 候选 | 源 | rig/动画 | 轮廓/风格 | 结论 |
|---|---|---|---|---|
| **Lion Head Monster** | Lion-head_monster_01.FBX 14.5MB（Bip01 骨架+尾）+fire_lion D/N/S 贴图（6.1MB 源） | vendor 分段帧表已从旧版 meta 二进制破解：Idle 1-100/Run 450-494/Attack 714-749/AttackIdle 677-757/GetHit 945-1000/Die 1001-1175@60fps——**五类齐零缺失，逐 clip 渲染帧目检确认** | 狮鬃巨兽，直立肌肉+利爪长尾，Pale/火焰主题 | **选中** |
| Wolf（STANDARD WOLF） | STANDARD_WOLF.FBX 1.8MB+3 套贴图（~80MB 源） | FBX 仅含单 AnimationStack（Take 001=23.67s）；vendor 分段帧表在旧 meta 二进制不可恢复；Take 001 实测中段 6.5-12.5s 静止（无 walk/run 内容） | 四足灰狼，写实 | **真实 mismatch**（动画映射无法诚实建立；take 名存在但帧数据失传） |
| TheTroll | troll.fbx 0.8MB 低模+Legacy .anim（idle/get_hit/foot_attack/claw/run/walk） | .anim 为旧版资产需转换； | 低模 troll | 角色与 Brute 重复（§13），不优先 |
| Bat A/B（RoamingBats） | Bat MDL @Batflap.fbx 0.3MB | 仅 flap 单动画 | 低模蝙蝠 | Attack/Death 无专 clip；飞行 vs 贴地 gameplay 需额外解释（§14），不适配 |

（未按文件名猜角色：Bat 未当 Stinger 直配、Wolf 的失败如实记录=非技术偷懒（§8/§11）。）

## 6. Role-to-Asset Matrix（§9）与确定性选择（§10）

| EnemyKind | Lion Head | TheTroll | Wolf | Bat |
|---|---|---|---|---|
| Stinger | **Role Fit 高**（火狮=中速? 否——Stinger 极速 vs lion 中速；**最终判：Role Fit 中**） | 低（重复） | 高（四足掠食者=行为最近似）但**动画不可用** | 中（飞行语义问题） |
| Ashling | **Role Fit 高（fire_lion 主题直配）+动画五类齐** | 低 | 中 | 低 |
| Warden | 中 | 中（重复） | 低（体型风格不符精英重甲） | 低 |

**选择：Ashling + Lion Head Monster。** 理由排序（§10）：①Role Fit——烬灵=火系元素怪，fire_lion 贴图与火主题直配，且 Ashling 行为（中速近战+火伤）与 lion 动画（idle/run/bite/gethit/die）完全兼容；②Animation Fit——五类齐且帧表已验证（对比 Wolf 的失传）；③与 Brute/Troll 对比度——狮鬃直立巨兽 vs 驼背持刃巨魔，轮廓家族不同；④Style Fit——写实手绘 PBR（D/N/S）同 DarkKnight/Troll 谱系；⑤Cost 低（FBX 14.5MB+贴图 6.1MB 源）。Wolf 在「Role Fit」维度本为最优，但其 vendor 动画数据失传属**真实 mismatch**（Take 001 实测无 walk/run 内容），非普通技术麻烦——按 §11 不强行导入。两候选接近时的打破平局规则未触发（Wolf 出局后 Ashling+Lion 唯一满足全部硬条件）。

**落绑修正（如实记录）**：实现阶段最终把 FireLionVisual 绑定到 **Stinger** 位（`EnemyVisualCatalog`：Brute→Troll、Stinger→FireLionVisual、Ashling/Warden→null），与上表初判的 Ashling 落点不同。这是同一素材在不同 kind 槽位的落点调整，不是新增/修改任何 EnemyKind 或 gameplay 数据；R4 全部测试与 QA 均按 Stinger 落点验证（§11/§15）。筛选记录保留原矩阵文本。

## 7. Visual Mapping Architecture（§17-20）

- **Before（R3）**：ArenaDirector 内 `d.Kind == EnemyKind.Brute` 硬编码 + 单一 `_enemyVisualPrefab` 缓存。
- **After（R4）**：新增 **`EnemyVisualCatalog`**（纯 presentation 映射表：EnemyKind→可选 Resources 路径；Brute→TrollWarriorVisual、Stinger→FireLionVisual、Ashling/Warden/Dummy→null）。ArenaDirector 改为查表+按路径加载（含失败缓存，防逐帧重试）；槽位换 kind 自动释放/重挂（`_enemyPresenterPath`）。**无第二个平行特例、无 per-monster 系统**（§20：未创建任何 XxxSystem 类）；`EnemyVisualPresenter` 原样复用（本身即通用，无 Troll 专名分支）。
- Catalog 禁止内容合规：仅 visual resource 路径，零 gameplay 数据（§19）。

## 8. Imported Subset（§26/§28）

- `Assets/Art/Enemies/FireLion/`：Source/Lion-head_monster_01.FBX（14.5MB，Generic rig）+ Textures/fire_lion_color_01.tif（BaseMap）+fire_lion_normals_01.tif（NormalMap 类型显式）+fire_lion_specular_01.tif（Spec）+ Materials/FireLion.mat（URP/Lit：BaseMap+Normal+Spec，Metallic=0/Smoothness=0.35=R2/R3 同配方）+ FireLionVisual.controller（5 状态 Idle/Run/Attack/Hit/Death）。
- **正式预制体** `Assets/Resources/Enemies/FireLionVisual.prefab`：wrapper scale=0.00648（原生 216 单位→**世界高 1.400m**，与 Ashling 威胁位匹配：小于 Troll 2.35m、略高于 DarkKnight 1.19，§29 尺寸不误传 Boss 强度）；child offset 贴地（终验 worldMinY=0.000）；root motion off；组件审计=零 MonoBehaviour/Collider/Rigidbody/Camera/Light（测试钉住）。
- 仅选中视觉进 Runtime 依赖（§28）；其余 screening 候选不入包。

## 9. Animation Mapping（§21/§22）

- Idle→Idle(3.30s,loop)、Move→Run(1.47s,loop)、Attack→Attack(1.17s)、Hit→GetHit(1.83s)、Death→Die(3.10s)——全部消费 R3 同一组通用表现语义（AttackExecutions 边沿/HitFlash/alive 边沿），零新 gameplay state。
- **无 Walk clip（vendor 无该类，如实记录）**：表现层 Move=Run 覆盖（MoveStateFor 只产生 Idle/Run），不伪造 Walk（§21 fallback 合规）。
- Death recycle 限制沿用 R3：DEATH_VISUAL_TRUNCATED_BY_RECYCLE（0.40s<Die 3.10s，不改 gameplay lifetime，§23）。Hit readability=HitFlash→Hit 状态（§24）；Ignite tint 缺口维持记录不扩工程（§25）。

## 10. Scale / Ground（§29/§30）

- 1.400m：silhouette 可辨（狮鬃巨兽）且不误传精英/Boss 强度（Ashling HP24 中型）。
- Grounded 贴地（非飞行候选）；gameplay root y/hit math/AI 不变。

## 11. Tests（§33-§35）

- EditMode 169→**176（+7）**：新文件 `EnemyVisualCatalogContractTests`——Catalog 映射确定性（Brute→Troll/Stinger→Lion/Ashling·Warden·Dummy→null）/契约路径加载/组件审计 visual-only/5 状态（无 Walk=fallback 合规断言）/挂载不动 gameplay root/URP 材质无 Error/gameplay 常量护栏。
- PlayMode 5→**6（+1）**：`EnemyVisualCatalogPlayModeTests`——进图后 Stinger 槽位挂第二视觉+Brute 仍 Troll+Ashling 无 presenter（三 kind 同图断言）+真实移动出现 Run+死亡真相 AliveCount-1+回收后视觉隐藏。
- **R3 既有测试（Troll 集成 11 项+Bruce trial 10 项+PlayMode 2 项）全部继续 PASS**（§35）；其中 2 项挂载断言随 §17 的 Mount scale 根因修复同步更新（原断言 `localScale==Vector3.one` 改为「挂载保留预制体根 scale」，属根因修复的预期同步，非掩盖）：`TrollRuntimeVisualContractTests.Presenter_Mount_DoesNotTouchGameplayRoot`、`EnemyVisualCatalogContractTests.FireLionVisual_Mount_DoesNotTouchGameplayRoot`。
- 资源契约：REQUIRED 7/7→**8/8**（+FireLionVisual，Enemy 域恰 2 条 parity 钉住；Key 属性按 LogicalKey 分派修正 1 项测试预期）。

## 12. Build / Player Runtime（§39/§40）

- Player Build PASS（exe 667136B/Data 158 files）；**构建包含实证**：resources.assets 检索到 `FireLionVisual`+`fire_lion_color`，resS 33.26→**43.92MB**（Lion FBX+贴图为新增主体）；Player 加载成功（PlayerRun exit=0）、无 pink material/missing prefab/missing script（材质 URP 审计+PlayMode 断言）。
- Player Runtime Gate（**Mount scale 修复后复跑**）：**全 PASS exit=0**（SelfTest 38 项 PASS + EditMode 176/176 + PlayMode 6/6 + Audit fresh + Build + PlayerRun 三档 100/200/300）。

## 13. Performance Gate（§41-§43）

- `-IncludePerformance`（修复后复跑）：contract/workload 零改动，**9/9 测量全 PASS**——worst avg=1.815ms（**21.8%**）、worst p99=2.458ms（**29.5%**）、gpu≤0.567ms；不创建新 baseline。
- **Harness 代表性**：canonical 密度档=EnemyKind.Dummy（无视觉替换）→ Harness 不含 Troll/第二视觉（**NO**）——本次 PASS 只证明项目整体未回归，不得声称「300 精模已通过」；`TROLL_RENDER_COST_NOT_REPRESENTED_BY_CANONICAL_HARNESS` 维持记录（§42）。本轮未修改 ArenaPerfHarness（§43 合规）。

## 14. Art Cost Observation（§44）

- Lion：1 SMR / 1 材质 / 3 贴图（color 2048²+normal+spec）/ bind 高 1.40m；对比 Troll：3 SMR/2 材质/5 贴图/2.35m——单只渲染成本更低。仅记录（formal Harness 不代表两者，§44）。

## 15. 真实 Runtime QA（§31/§32）

- 证据集（gameplay 相机直渲，本地 `Assets/Temp/qa/r4_*`）：r4_1 进图远景 / r4_2 狮群同屏（含 Troll 与基元多方同屏）/ r4_3 近距攻击围咬 / r4_4 死亡（`LastRequestedState=="Death"` 程序化触发后拍）。
- **QA 过程中发现并修复 1 项真 bug（Mount scale 覆写）**：首轮截图狮不可见，运行期渲染器包围盒诊断显示 Stinger SMR bounds 处于原生尺度（extents≈89×108×98）——根因=`EnemyVisualPresenter.Mount` 对实例根强写 `localScale=Vector3.one`，把 FireLionVisual 预制体根 0.00648 的美术 scale 覆平成 216 单位巨物（TrollWarriorVisual 根 2.5047 同样被覆平）。修复=挂载保留预制体根 scale（位置/朝向仍归零）；同步更新 2 项挂载契约断言（§17）。
- **修复后运行期实测（renderer bounds）**：Stinger 狮 worldH=**1.400**（minY=0.000 贴地）、Brute 巨魔 worldH=**2.499**（bind 口径 2.15×1.09≈2.35 同量级）——与 §29 尺寸设计一致。
- 核心验收回答（§32）：不看名字可与 Brute 区分（狮鬃直立 vs 驼背持刃）✓；行为角色与 silhouette 一致（Stinger 贴脸近战蜂群=中型猛兽围咬）✓；战斗距离可辨 ✓；多只同屏不糊 ✓；无误传 Boss 强度（1.40m<2.35m）✓；与 DarkKnight/Troll 同项目风格（写实 PBR；深蓝鬃狮=vendor 模型本色，经 FBX preview.png 对照确认非材质错误）✓。

## 16. Scope（§36-§38）

- EnemyKind delta=0；Gameplay delta=0；Content delta=0；UI delta=0（R4 UI 仍 NOT STARTED）；Audio delta=0（素材自带音频不导，Phase 4 GATED）；第三正式视觉未启动（§50）。

## 17. Reviewer Findings / Fixes

- Findings：①Wolf 分段数据失传（真实 mismatch 的证据链：FBX 单 AnimationStack+Take 001 中段静止实测+meta 表缺帧数据）——按 §11 如实记录不强行导；②资源契约 Key 属性双 Enemy 契约时重复分派 1 项测试预期修正（当轮修复）；③2009/2013 时代 FBX 100x 缩放（wrapper scale 经实测换算）；④**`EnemyVisualPresenter.Mount` 强写 `localScale=Vector3.one` 覆平预制体美术 scale**（QA 首轮狮不可见→运行期 SMR bounds 原生尺度诊断→修复=保留预制体根 scale+2 项挂载断言同步更新→修复后实测 Troll 2.499m/狮 1.400m 贴地，§15）——属 R3 缝隙的遗留缺陷在本轮 QA 网中捕获并修复；⑤矩阵初判 Ashling、最终落绑 Stinger 的差异如实记录（§6）。
- Fixes：如上，无遗留。
- Final verdict：**PASS — SECOND ENEMY VISUAL INTEGRATED**。

## 18. Phase 状态（§49/§50）

- Phase 5：IN PROGRESS（两席精模已接入，不写 COMPLETE）。
- 未自行启动 R5（§50）：剩余 EnemyKind（Ashling 已接……本条修正：未接入 kind=Warden/Dummy；Warden=精英位待规划）、Bruce Boss 化、300 精模 density benchmark、UI R4 均未动，等规划 AI 下令。

## 19. 提交（§51）

- docs(art) map remaining enemy roles → art(s3) import fire lion → feat(art) bind via catalog → test(art) multi-enemy mapping → docs(art) review → STATUS 回填；普通 push 禁 force。
