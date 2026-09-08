# S3-PHASE5-ART-R8 ASHLING VISUAL 复核与收口报告

工作令：S3-P5-ART-R8-ASHLING-FORMAL-VISUAL（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。Baseline=f2f6d16（R7 收口 HEAD，已 push）。

## 0. Verdict

**ASSET ACCEPTED — ASHLING RUNTIME INTEGRATION BLOCKED BY ART PERFORMANCE**（§52 规定分支：候选资产通过全部功能/角色/风格/技术验证并完成临时接入，但 Formal Art Performance Gate 四视觉 stress mix 实测超预算；按工作令 §40/§44/§45 不优化、不降质、恢复 Ashling→placeholder，资产保留待未来 Art Performance 优化轮）。

## 1. Baseline HEAD

- f2f6d16（R7 Formal Art Density Gate 收口 HEAD）。

## 2. Ashling Gameplay Truth（§3，只读）

6@r15 / HP24 / 甲20 / 闪避40 / 速2.2 / cd1.05 / 物3+**火5** / 近战直击（无敌方弹道）/ 中型威胁（>Stinger、<Brute<Warden）/ placeholder=中圆柱。代码当前真相复核一致。

## 3. Ashling Role Profile（§4，Art planning，不写回 schema）

Combat Role=中距近战火系扰袭（DoT 点燃）；Typical Distance=中距；Mobility=中速；Damage Language=轻击+火（灼烧/余烬语义，不必全身火焰）；Threat Level=中型；Visual Weight=1.5–1.8m；Silhouette Needs=与火狮/巨魔/Bruce 区分；Animation Needs=Idle/Move/Attack/Death 必备、Hit 优先。

## 4. Source Screening（§5-§15）

- Search scope：导演本地池全量（`素材初筛，等待验收`+主素材库 `游戏素材文件夹\...资源大合集`），未联网/未购新。
- Candidates inspected：PBR Monster Pack 3（石像鬼/骨龙/蛇战士/**Weeper** 实测解出）+ R4 已盘候选（Wolf/TheTroll/Bat×2）+ POLYGON 系/虫包/Orc/Dwarfs/卡通/NPC 通用。
- Rejected：Weeper（施法系视觉与近战直击 truth 冲突）、Bone Dragon（大体型误传 Boss 威胁）、Serpent Warrior（人形剪影与 Troll 家族相近，主题弱于 Gargoyle）、Wolf（动画失传维持）、TheTroll（角色重复维持）、Bat×2（飞行+缺 clip 维持）、POLYGON 系（导演风格裁定维持）、虫包（体型过小）、Orc/Dwarfs/卡通/NPC（角色/风格不符）。
- **Selected：SFB Gargoyle（带翼石魔）**——Role/Style/Silhouette/Animation/Cost 五轴全 PASS；vendor 30 段命名分段动画自 meta 破解（Idle 110f/Walk 30f/Attack01 60f/Hit 85f/DeathStanding 40f 地面五态全齐零缺失+飞行/施法余段未用）。

## 5. Integration（临时态，§17-§31 全过）

- 导入子集：FBX 17.4MB+2048² albedo/normal/metallicRoughness（最小化，无 demo/vendor 脚本/音效/粒子）。
- 目录：`Assets/Art/Enemies/Gargoyle/`（沿用惯例）。
- 正式 prefab（当时）：`GargoyleVisual.prefab` wrapper scale=0.537→**世界高 1.700m**、minY=0.000 贴地；控制器五态（Move=Walk clip 挂 Run 状态名）；材质 URP/Lit 双渲染器（Body+Wings）；组件审计全零；root motion off。
- Catalog 绑定（当时）：Ashling→GargoyleVisual；资源契约 9/9→10/10（当时）。
- R5 反馈 parity：Ignite/Hit 优先/恢复 自动生效，零候选特例；Element≠Ignite（§30：仅 IgniteRemain>0 触发，已断言）。
- EditMode 7 项+PlayMode 集成 1 项+QA 七张（五方同屏/Ignite/Hit-while-Ignite/恢复/6 只 Ashling 同屏/近距攻击/死亡）全部 PASS（临时态证据，`Assets/Temp/qa/r8_*`）。

## 6. Formal Art Performance Gate 实测（§40/§42/§43）——**FAIL**

- 四视觉 stress mix（RoundRobinByEnemyKind：100 档 25×4、200 档 50×4、300 档 75×4；resolvedVisuals=4，visual_instances==density，无 fallback）。
- 3 runs 全 FAIL：
  - 200 档：main p99=10.767/10.78/10.798 > 8.33（3/3）
  - 300 档：main avg=9.055/9.095/9.14 > 8.33、p99=14.486/14.625/14.911 > 8.33、cpu 超预算、alive=278/287/296<95%
  - 100 档：PASS（avg=4.48-4.56、p99=6.36-6.57、gpu≤0.97）
- canonical Performance 同轮 PASS（Dummy 基线无回归）。

## 7. 裁定执行（§40/§44/§45）

- **禁止项全部遵守**：未降纹理/未关阴影/未加 LOD/未减质量/未减密度/未放宽 8.33ms/未改 profile/未改 density。
- 候选资产保留：`Assets/Art/Enemies/Gargoyle/`（FBX+贴图+材质+控制器+**GargoyleVisual.prefab 移至 Prefabs/ 子目录——非 Resources Runtime 入口**）。
- Runtime mapping 恢复：**Ashling→placeholder**（Catalog null 断言钉住；Resources 无入口断言钉住）。
- 恢复后回归：`-IncludeArtPerformance` 复跑全 PASS——EditMode **198/198**（含 AshlingVisualContractTests 7 项保留资产健康测试）、PlayMode **9/9**、Audit fresh、Build PASS、canonical 9/9 PASS、**Art 9/9 PASS**（worst avg=6.078ms、worst p99=7.603ms=91.2%，与 R7 基线一致）。

## 8. Director Truths（§31，全部保留）

Player base HP=9999999 ✓ / Town·出图后不自动 SpawnDummies(8) ✓ / runInBackground=true ✓ / RequestState 同状态不重播修复 ✓（护栏测试维持）。

## 9. Scope（§49/§50）

- EnemyKind delta=0（Ashling 未换模回退）；Gameplay delta=0（本轮）；Content/UI/Audio delta=0；新无关模型=0（仅 Gargoyle 子集）；第五正式视觉/LOD/纹理压缩/Phase 3 R4/Audio 未启动。

## 10. Verification（恢复后终态）

- SelfTest：PASS；EditMode：198/198；PlayMode：9/9；Audit：fresh PASS（REQUIRED 9/9——GargoyleVisual 契约行随接线回退移除）；Build：PASS；PlayerRuntime：PASS（双 Gate 内 3 locked-env runs）；ArtPerformance：PASS（三视觉基线回归）。

## 11. Reviewer Findings / Fixes

- Findings：①四视觉 Art Gate FAIL 是本轮**核心事实**（数据见 §6，证据 `%TEMP%\GAME-ZZZ-UnattendedGate` 当轮产物，未冻结——工作令 §46 本轮不要求双 Gate 冻结）；②vendor 分段破解复用 R4 方法（meta 命名 clipAnimations 直接可读，成本低）；③控制器 Move 状态名须为 "Run"（FBX clip 名 Walk→状态改名，R4 Move=Run 语义一致）。
- Fixes：如上，无遗留。
- Final verdict：**ASSET ACCEPTED — ASHLING RUNTIME INTEGRATION BLOCKED BY ART PERFORMANCE**。

## 12. Phase 状态（§47/§50）

- Phase 5：仍 IN PROGRESS——三套正式视觉（Troll/FireLion/Bruce）+Formal Art Performance Gate（R7）维持；Ashling=placeholder（保留已验收候选资产待优化轮）；Dummy=benchmark placeholder（§48 永不算缺口）。
- 未自行启动：第五正式视觉、Art optimization、LOD/纹理压缩、Phase 3 R4、Audio、新 Boss。

## 13. 提交（§53）

- docs(art) screen models for ashling role（本筛选报告）→ art(s3) import selected ashling visual（保留资产入库）→ feat(art) 恢复 placeholder+保留资产接线回退 → test(art) 保留资产健康测试 → docs(art) R8 review+STATUS 回填；普通 push 禁 force。
