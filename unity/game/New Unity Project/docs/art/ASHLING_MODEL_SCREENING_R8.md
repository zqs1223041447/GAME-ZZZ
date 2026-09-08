# Ashling 定向选模报告（S3-P5-ART-R8-ASHLING-FORMAL-VISUAL §16）

日期：2026-09-08。工作令允许范围：导演本地素材池全量（许可证不设 Gate）。

## 一、Ashling Gameplay Truth（只读审计，代码当前真相）

| 项 | 值 |
|---|---|
| Spawn | 6 只 @ r15（每图第三批） |
| HP / Armor / Evasion | 24 / 20 / 40 |
| 速度 / 攻击 cd | 2.2（中速）/ 1.05 |
| 伤害 | 物理 3 + **火 5**（近战直击，无敌方弹道） |
| 行为语言 | 中距离近战 + 火系 DoT；非远程/非施法（无弹道） |
| placeholder 轮廓 | 中圆柱（橙），~0.55 缩放 |
| 威胁层级 | >Stinger（HP20/甲0）与 <Brute（HP36/甲80）<Warden（HP110 精英）→ **中型** |

## 二、Role Profile（Art planning，不写回 schema）

- **Combat Role**：中距近战火系扰袭（DoT 点燃制造持续威胁）
- **Typical Distance**：r15 环→逼近停步（中距）
- **Mobility**：中速（2.2）
- **Damage Language**：轻击+火（视觉需要「灼烧/余烬」语义但不必全身火焰）
- **Threat Level**：中型（不是精英/Boss）
- **Visual Weight**：1.5–1.8m 区间（>FireLion 1.40、<Troll 2.5/Bruce 2.6）
- **Silhouette Needs**：与火狮（四足狮）、巨魔（持刃驼背）、Bruce（龙鬃双武器）明确区分
- **Animation Needs**：Idle/Move/Attack/Death 必备，Hit 优先（缺省可用 R5 tint 保可读）

## 三、Search Scope（§5/§6）

导演本地池全量盘点：`G:\GAME-ZZZ\素材初筛，等待验收\`（01 特效 32 包 / 02 场景 / 03 角色怪物 / 04 UI）+ 主素材库 `G:\游戏素材文件夹\3D游戏...资源大合集\角色模型\`。未联网下载、未临时购买。此前 R4 已盘点并留档：TheTroll（角色重复+Legacy 动画）、Wolf（动画数据失传）、Bat×2（飞行语义+缺 Attack/Death clip）。

## 四、Candidates Inspected & Screened

| 候选 | 来源 | Role Fit | Style Fit | Silhouette | Animation Fit | Cost | Verdict |
|---|---|---|---|---|---|---|---|
| **SFB Gargoyle（石魔，带翼石魔人）** | PBR Monster Pack 3（主库 PBR石像鬼骨龙蛇战士 4.9GB 双卷 RAR，实测解出 PBR Monster Pack 3-10.unitypackage） | PASS（石魔+火伤=「烬」语义合理；中速地面近战） | PASS（PBR、与 DarkKnight/Troll/FireLion/Bruce 同写实谱系） | PASS（翼+角驼背石魔，与火狮四足/巨魔持刃/龙鬃 Bruce 明确区分） | PASS（vendor 30 段命名分段自 meta 破解：Idle 110f/Walk 30f/Attack01 60f/Hit 85f/DeathStanding 40f+飞行/施法等；地面五态全齐零缺失） | CONDITIONAL→实导通过（FBX 17.4MB+2048² D/N/MR 三贴图+2 渲染器，极低成本） | **SELECTED** |
| SFB Weeper（披布哭泣者） | 同包 | CONDITIONAL（烬灵语义极佳：石质 tex_Stone 存在） | CONDITIONAL | PASS（与全部现有敌区分） | **FAIL 倾向**（施法系视觉：Weeper Spell 粒子=远程语义，与近战直击 truth 冲突；走位/攻击 clip 待验但风险高） | 低 | 淘汰（功能风险） |
| SFB Bone Dragon（骨龙） | 同包 | FAIL（大体型→误传 Warden/Boss 威胁，§10 淘汰轴） | PASS | FAIL | 低估 | 淘汰 |
| SFB Serpent Warrior（蛇战士） | 同包 | PASS | CONDITIONAL | CONDITIONAL（人形持械，与 Troll/DarkKnight 剪影家族相近） | PASS（Attack1-3/Tail Whip 齐） | 中 | 备选落选（Gargoyle 主题/剪影更优） |
| Wolf（STANDARD WOLF） | R4 已盘 | 高 | PASS | PASS | **FAIL（动画数据失传，R4 证据链维持）** | — | 维持淘汰 |
| TheTroll | R4 已盘 | FAIL（角色与 Brute/Troll 位重复） | PASS | FAIL | CONDITIONAL | — | 维持淘汰 |
| Bat×2 | R4 已盘 | FAIL（飞行 vs 地面近战 + 缺 Attack/Death clip） | — | — | FAIL | — | 维持淘汰 |
| POLYGON 系 9 包 | 已拷贝 | — | **FAIL（导演裁定：不批作正式最终同框风格）** | — | — | — | 淘汰 |
| 野生动物昆虫（critterpack） | 已解压 Wildlife Insects 1.2 | FAIL（体型过小，无法承载中型威胁且与「昆虫系」轮廓不匹配） | — | — | — | — | 淘汰 |
| Orc Pack / Dwarfs / 9 卡通 / NPC 通用 | 已拷贝 | FAIL（人形装备系与 Ashling 元素体角色不符/风格不符） | — | — | — | — | 淘汰 |

## 五、SELECTED：SFB Gargoyle

- 理由排序：①Role Fit——石魔=「烬灵」石/烬主题自然映射，地面中速近战与 Ashling 行为完全兼容；②Animation Fit——地面五态全齐（唯一零缺失候选）；③Silhouette——与现有三套正式视觉明确区分（四足狮/持刃驼背/龙鬃双武器 → 带翼石魔）；④Style Fit——PBR 写实同谱系；⑤Cost 低。
- 导入子集（§18 最小化）：FBX+3 贴图（albedo/normal/metallicRoughness）——未导 demo 场景/vendor 脚本/音效/粒子（§18）。
- 实导结果：wrapper scale 0.537→**世界高 1.700m**（minY=0.000 贴地）；控制器五态 Idle/Run(Walk clip)/Attack/Hit/Death；材质 URP/Lit（同 R4 配方）双渲染器（Body+Wings）。

## 六、Runtime 集成结果与最终裁定

- 已完成临时验证（集成态）：EditMode 7 项+PlayMode 集成测试+QA 七张（五方同屏/Ignite/Hit/恢复/6 只 Ashling 同屏/近距攻击/死亡）全部通过。
- **Formal Art Performance Gate（四视觉 stress mix）实测 FAIL**：200 档 p99=10.78-10.80ms、300 档 avg=9.06-9.14ms/p99=14.49-14.91ms/cpu 超预算、300 档 alive 278-287/300<95%（3 runs 全 FAIL，`%TEMP%\GAME-ZZZ-UnattendedGate` 当轮证据）。
- 按 §40/§44/§45：**不做任何优化/降质/改预算**；候选资产保留，Runtime 映射恢复 **Ashling→placeholder**；GargoyleVisual.prefab 移出 Resources（资产保留于 `Assets/Art/Enemies/Gargoyle/Prefabs/`，非 Runtime 入口）。
- 最终状态：**ASSET ACCEPTED — ASHLING RUNTIME INTEGRATION BLOCKED BY ART PERFORMANCE**。

## 七、恢复后回归验证

- -IncludeArtPerformance 复跑（三视觉基线）：EditMode 198/198（含保留资产 7 测）、PlayMode 9/9、Audit fresh、Build PASS、canonical 9/9 PASS、**Art 9/9 PASS**（worst avg=6.078ms、worst p99=7.603ms=91.2%）——R7 基线无回归。
- Catalog：Ashling→null/Dummy→null 断言钉住；保留资产非 Resources 入口断言钉住（防未来误接线）。
