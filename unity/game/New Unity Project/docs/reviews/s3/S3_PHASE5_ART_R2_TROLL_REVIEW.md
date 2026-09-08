# S3-PHASE5-ART-R2 TROLL TRIAL — 复核报告

工作令：S3-P5-ART-R2-PBR-STYLE-ANCHOR（Phase 5 Art Trial R2：代表怪第一只——Dexsoft Troll Warrior（Troll_2）单模型导入、试装与九轴验收）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**ACCEPT WITH FOLLOW-UP**（九轴：8 项 PASS + Style Fit=CONDITIONAL；技术导入/集成/契约全部达标：EditMode 158/158（+10 Troll 契约）、Gate 全 PASS exit=0、Runtime/Gameplay/Content/Audio delta=0、UI 零改动。Style Fit 的风格混用裁定归导演/规划 AI，见 §12）。

## 1. Baseline HEAD

- Baseline=6ba2ace（Phase 5 Art Trial R1 Bruce STATUS 回填后 HEAD；R1=ACCEPT WITH FOLLOW-UP 未正式集成；Phase 3 UI R1-R3 COMPLETE 未动；Phase 4 人声继续不做）。
- 授权前提：导演 2026-09-08 裁定（DECISIONS 规则⑪）——许可证不再是任何 Gate；本轮资源取舍只看风格/技术/动画/性能/可读性/玩法适配。

## 2. Source asset inventory（工作令 四）

- 来源：导演本地 `素材初筛，等待验收\03_角色怪物\已拷贝\巨魔战士 模型\巨魔战士.rar`（39.4MB）→ 内含 **Dexsoft Troll Warrior/troll2.unitypackage**（42.6MB，vendor=Dexsoft-Games；cgjoy 转载包，附论坛 url/推广 txt——不入库）。
- source format=unitypackage（vendor 正式包，含 asset.meta 与原始 GUID——与 Bruce 包「无 meta 需手写」不同，本包直导即 GUID 保持）；vendor=Dexsoft-Games「Troll Warrior」（Troll_2 系，含 L 低模变体）。
- 盘点要点：模型 FBX（36 骨标准 Biped 人形+武器网格）、动画源 FBX（厂商已按 take 预切）、贴图 5 张 TGA（身体 2048² D/N/S + 武器 512×1024 D/N）、vendor 演示场景/脚本未进包内资产面（无 demo prefab 需剔除——比 Bruce 包干净）。
- 另一包 `巨魔 模型\巨魔 Unity 4.2.0.rar`（TheTroll/The_Troll.unitypackage，27MB）本轮未触碰（初筛报告「巨魔×2」中的另一条，留给后续候选）。

## 3. Imported subset（工作令 五/六/七）

- 导入目录（遵循 `Assets/Art/Enemies/{...}` 既有惯例）：`Assets/Art/Enemies/TrollWarrior/{Source,Textures,Materials,Prefabs}`。
- 导入集：
  - Source/：Troll_2_L.FBX（19.9MB，模型+rig，Humanoid）+ Troll_2_L_anims_split.fbx（21.7MB，厂商预切 take 动画源，Humanoid）
  - Textures/：Troll_2_D.tga（2048²）、Troll_2_N.tga（2048²，NormalMap 导入型）、Troll_2_S.tga（2048²，spec）、Sword_D.tga（512×1024）、Sword_N.tga（512×1024，NormalMap 导入型）
  - Materials/：Troll_2_D.mat、Sword_D.mat（项目侧迁移 URP/Lit）
  - Prefabs/：TrollWarriorTrial.prefab（项目侧 wrapper）+ TrollWarriorTrial.controller（项目侧，6 状态）
- 剔除项=无（包内无 demo 预制体/demo 脚本/vendor 场景——五的排除清单天然为空；vendor 推广 txt/url 不入库）。
- 总量≈47MB（FBX 占 41.6MB；贴图≈5.4MB TGA）。

## 4. Rig（工作令 八/十五）

- **RigType=Humanoid**（ModelImporter.animationType=3，双侧 FBX 一致）。36 骨标准 Biped 人形——按真实结构判断，非强行转换；`avatar.isHuman=true` 由测试钉住。Humanoid 选择的收益=未来与 DarkKnight/Bruce 共用人形动画重定向通道。

## 5. Animations（工作令 九/二十四）

- 源=厂商已按 take 预切（`anims_split` FBX 内部 take 表，无需 clipAnimations 覆写）；测试断言 **≥20 clips** 且五类代表 clip 必在。
- 五类映射：**Idle=Standby ✓ / Move=Walk+Run ✓ / Attack=Attack1 ✓ / Hit=Beaten ✓ / Death=Death1 ✓——零 MISSING**。
- Root motion：**关闭**（applyRootMotion=false，二十九）；wrapper controller 为 6 状态最小集（Idle/Walk/Run/Attack/Hit/Death，默认 Idle；状态→motion 接线实测：Idle→Standby / Walk→Walk / Run→Run / Attack→Attack1 / Hit→Beaten / Death→Death1）；QA 用 `Animator.Play()` 直驱。**状态间过渡接线=正式集成 follow-up**（与 Bruce R1 同）。

## 6. Materials / URP conversion（工作令 十/十一）

- 项目侧 2 材质全迁 **URP/Lit**：Troll_2_D=_BaseMap 身体 2048² D + _BumpMap N + **_MetallicGlossMap=S spec 图**（keyword _METALLICSPECGLOSSMAP）+ _Metallic=0 + _Smoothness=0.35；Sword_D=_BaseMap 512×1024 + _BumpMap N（同构）。未引入 HDRP/vendor pipeline/整套 shader framework（十达标）；未美化改变源辨识度（十一）。
- PBR 贴图组挂接完整性（工作令 十八）由测试逐材质钉住：BaseMap+Normal 必挂、法线图必须 NormalMap 导入型、不得残留 FBX 内嵌 vendor 默认材质。

## 7. Texture observations（工作令 十二）

- 3×2048²（D/N/S）+2×512×1024（D/N）；法线/武器法线显式 NormalMap 型（防止被当普通颜色图）；无 8K 异常。运行时显存量级≈3×2048² 压缩贴图+2×512×1024（中低）。**未发明全项目 texture budget**（按令仅观察记录）。

## 8. Prefab component audit（工作令 十三/三十/三十一/三十二）

- **TrollWarriorTrial.prefab 全组件清单**：根（Transform+Animator，applyRootMotion=false，Humanoid avatar+controller）+ 骨骼层级 + **SkinnedMeshRenderer×3**（body+weapon_left+weapon_right——武器为 skinned 挂骨网格，随动画手部位移；实测静态 MeshRenderer=0）。
- **vendor MonoBehaviour=0；Collider/Rigidbody/Camera/Light=0**；缺脚本=0；无 gameplay 组件（十三达标，测试逐 transform 钉住）。

## 9. Scale（工作令 十五/十六）

- wrapper 等比 scale=**2.5047**。高度多口径（如实并列）：测试 bind 口径（sharedMesh 本地 AABB×scale）**≈2.15m**（断言 2.15±0.15）；实例化实测世界 bounds（含 child y 0.034 贴地修正）**=2.350m**；动画播放口径（Walk 中 SMR bounds）≈2.43m；视觉渲染口径≈2.35m（bind×1.09）。
- DarkKnight-relative：DarkKnight 可见高≈1.19 → **Troll/DarkKnight≈1.8×（bind 口径）-2.0×（世界口径）**——**重量级精英**定位，低于 Bruce Boss 2.60m（≈2.2×）；对普通木桩（≈0.58）≈3.7-4.0×。俯视镜头体量感明确不遮画面。
- **视觉 scale 与 gameplay collider 解耦（十六）**：本轮零 gameplay 改动（hitbox/attack range/nav radius 未动——Runtime delta=0 实证）。

## 10. Ground contact（工作令 十四）

- **数值验证**：实例置于原点（root pos=0,0,0），渲染器世界 bounds 合并 **MINY=0.000**（脚底正好在地面；child localPosition.y=0.034 为微修正）。
- **视觉验证**（平地摆位、无遮挡、gameplay 相机+HUD 实拍）：站立（qa1）贴地、Walk 迈步贴地（qa2）、Attack 跨步挥砍贴地（qa3）、Death 倒地身体平贴地面不穿地不漂浮（qa4）、近战距与 DarkKnight 并立双脚落地面（qa5）。

## 11. Gameplay camera / readability（工作令 二十/二十一）

- 全部截图用**当前实际 gameplay 相机 + HUD 合成画面**（Game View screen capture，非 Scene View）：远景常态（qa1_troll_far_idle）、Walk（qa2）、Attack 态（qa3）、Death 态（qa4）、近战距并立（qa5_troll_close_darkknight）、三方同屏（qa7_threeway：DarkKnight+Troll+Bruce）。
- 判定：silhouette（驼背巨体+肩刺+双持刃）独特清晰；近/常距下武器/肢体/材质分离清楚；色彩（苍白皮肤+金饰暗甲 vs DarkKnight 暗黑 vs Bruce 橙红甲青绿鳞）——三方同屏信息可读性高（qa7 实证）；Hit 态由 controller 状态与 Beaten clip 侧验证（未单独截图，如实记录）。

## 12. DarkKnight style comparison / Nine-axis verdict（工作令 二十二/二十三）

| 轴 | 判定 | 依据 |
|---|---|---|
| 1. Style Fit | **CONDITIONAL** | Troll_2=带 Normal/Spec 的 2048² 手绘 PBR 风（低饱和土色+金属暗甲），谱系上**比 Bruce（纯手绘低模 512²）更接近 DarkKnight 的暗黑写实向**；三方同屏可读、色彩分离好；是否接受混用的方向性裁定归导演/规划 AI，执行 AI 不代拍 |
| 2. Silhouette | PASS | 驼背巨体/肩刺/双持刃轮廓独特，俯视一眼可辨 |
| 3. Rig | PASS | 36 骨标准 Biped=Humanoid，avatar 有效，引用零断链（vendor 正式包 GUID 保持式直导） |
| 4. Animation | PASS | 厂商预切 takes≥20 测试钉住；五类全齐零 MISSING；walk 帧序交替正常无明显滑步 |
| 5. Scale | PASS | bind≈2.15m/世界实测 2.35m≈1.8-2.0×DarkKnight，重量级精英定位明确且低于 Bruce Boss；等比缩放 |
| 6. Materials/URP | PASS | 2 材质迁 URP/Lit，PBR 组（BaseMap+Normal+Spec）挂接齐全，无错误 shader |
| 7. Ground Contact | PASS | 数值 MINY=0.000 + 站/走/攻/死/近距五态截图贴地 |
| 8. Gameplay Readability | PASS | 攻/走/死俯视可读；三方同框色彩/轮廓分离 |
| 9. Performance Cost | PASS | 合计≈1579 verts/1625 tris/3 SMR/2 mats/3×2048²+2×512×1024——成本低 |
| **Final** | **ACCEPT WITH FOLLOW-UP** | 技术轴全 PASS；follow-up=①风格混用导演/规划裁定 ②controller 过渡接线 ③Hit 态游戏内验证 ④正式集成时代表怪位定义（本轮禁止创建） |

## 13. Test result（工作令 三十六）

- `TrollWarriorArtTrialTests` 10 项（EditMode 148→**158**）：PrefabLoads / NoMissingScripts / RequiredRenderers_Present（3 SMR=body+weapon_left+weapon_right、静态 MR=0、空材质槽 0）/ WrapperScale_FinitePositive_HeavyHeight（有限为正+等比+bind 2.15±0.15m 断言）/ NoGameplayComponents_NoVendorScripts / Materials_UrpLit_PbrMapsAssigned（BaseMap+Normal 必挂+仅项目材质）/ AnimatorReferences_Valid（avatar isHuman+6 状态+默认 Idle+root motion off）/ SourceAssets_HumanoidRig_StableClips（双侧 FBX Humanoid+≥20 takes+五类代表 clip+骨骼>20）/ Textures_PbrMapTypes（3×2048²+2×512×1024+法线导入型）/ NoResourcesFolder_NoVendorJunk（无 Resources 泄漏+无 vendor 场景+唯一 prefab）。
- 适应实际资产结构：按组件类型断言（3 SMR、武器 skinned），未硬编码 vendor 层级名。

## 14. PlayerRuntime result（工作令 三十九）

- Troll **未接入默认玩家可见 Runtime 路径**（QA 实例=play 模式运行时 spawn，未存场景；无场景/预制体引用→不入构建）。canonical Gate：SelfTest PASS → `-IncludePlayerRun` **PASS exit=0**：EditMode 158/158 + PlayMode 3/3 + ContentAudit fresh + PlayerBuild win64 + PlayerRun PASS exit=0 三档 100/200/300。

## 15. Performance result / reason not formal（工作令 四十/四十一）

- **非 formal Performance Gate**（Troll 仅存在于非默认 Art Trial 预览，未进默认 Arena/benchmark Runtime——四十条款命中「不需要 formal」分支）。
- 记录：renderer count=3（全 SMR）；material/submesh=2 材质；vertices≈1579 / triangles≈1625（超低模）；贴图≈3×2048²+2×512×1024；frame observation=play 预览视觉流畅（编辑器观察，非正式证据）。无任何为性能作弊的动作（四十一）。

## 16. Runtime delta / Gameplay delta / Content delta / Audio delta（工作令 二十六/二十七）

- Runtime delta=0（Runtime/Core 零文件改动——git status 实证）；Gameplay delta=0（EnemyId/AI/loot/skill/spawn 均未创建）；Content delta=0（Content Audit fresh PASS——Art 文件未被误判，三十八达标）；Audio delta=0（无 roar/SFX，Phase 4 继续 GATED）。
- 允许的 delta=Art asset/prefab（Assets/Art/Enemies/TrollWarrior/** + 测试）。

## 17. UI delta / R4（工作令 四十四）

- UI 零改动；Phase 3 UI R1-R3 保持 COMPLETE；R4 继续 NOT STARTED / PLANNING REVIEW REQUIRED。

## 18. 本轮过程事实（如实记录）

- **崩溃续接**：上一会话在素材落地+测试编写完成、验证未开始时中断（API 故障）；本会话续接完成验证/收口，未重做已完成部分。
- **QA 截图全部重拍**：崩溃前会话已产 7 张 QA 图但其摆位脚本未保存、多张显示巨魔站位在木桩方块顶面（无法自证是否刻意摆位）；本轮以①数值接地验证（MINY=0.000）②全套平地重拍（摆位脚本落盘、自动避让 1.5m 内木桩）重建证据链；旧图已删（qa5_close_idle/qa6_darkknight 冗余重复一并去除）。
- **后台编辑器渲染停摆假象**：后台编辑器 autotick 只推进 tick 不触发重绘，截图捕获到陈旧绑定姿势（T-pose）；诊断链=normT 推进+SMR bounds 变化（CPU 在动）vs 截图字节不变（GPU 画面冻结）→ `editor_focus` 前台化后恢复正常；教训=截图前必须确认编辑器前台渲染活性（连拍字节变化）。
- **eval 对象名两处笔误**（DarkKnight(Clone)→实际为 Player）与 spawn 重复执行造成的一次双实例重叠 → 全部作废重拍；成品 6 张经逐张人工验图。
- Troll 未在本轮接入任何默认 Runtime 路径、未创建普通怪 system、未解压另一巨魔包（四十五达标）。

## 19. 下一步（按工作令 四十五，待规划 AI 决策）

- Troll_2 判定已写入 `MODEL_ASSET_SCREENING_R1.md`（代表怪行首个 ACCEPT WITH FOLLOW-UP）。若规划 AI 采纳：正式集成（controller 过渡接线+代表怪位定义+入默认 Runtime 路径时补 -IncludePerformance Gate）或据此定普通怪精模批量策略/下一候选（Lion Head/狼/蝙蝠×2/TheTroll 等）；若导演否决风格：REJECT 路径附九轴失败原因供筛选。
