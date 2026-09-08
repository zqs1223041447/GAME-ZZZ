# S3-PHASE5-ART-R1 BRUCE TRIAL — 复核报告

工作令：S3-P5-ART-R1-BRUCE-TRIAL（Phase 5 Art Trial R1：Dragon Warlord Bruce 单模型导入、试装与九轴验收）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**ACCEPT WITH FOLLOW-UP**（九轴：8 项 PASS + Style Fit=CONDITIONAL；技术导入/集成/契约全部达标：EditMode 148/148（+10 Bruce 契约）、Gate 全 PASS exit=0、Runtime/Gameplay/Content/Audio delta=0、UI 零改动。Style Fit 的风格混用裁定归导演/规划 AI，见 §12）。

## 1. Baseline HEAD

- Baseline=d4ecf9f（Phase 3 UI R3 STATUS 回填后 HEAD；R1/R2/R3 COMPLETE，R4 NOT STARTED 未动，Phase 4 人声继续不做）。
- 授权前提：导演 2026-09-08 裁定（DECISIONS 规则⑪）——许可证不再是任何 Gate；本轮资源取舍只看风格/技术/动画/性能/可读性/玩法适配。

## 2. Source asset inventory（工作令 四）

- 来源：导演本地 `素材初筛，等待验收\03_角色怪物\已拷贝\UNITY龙领主模型\Dragon Warlord Bruce.rar`（3.2MB）→ 解包为 **Dragon Warlord Bruce.unitypackage**（3.66MB）+ vendor 网站链接/junk（不入库）。
- source format=unitypackage（内含 FBX）；package 条目共 10（8 资产+2 目录占位）；vendor 自述=「Dragon Warlord Bruce，dragon/monster，918 polys（quad 口径）/1,015 verts，TIF 512²，rigged，20 animations（bip 时间线 0–855 帧）」。
- 盘点明细（解包后逐条目 pathname 核对）：bruce.FBX（9,244KB 源）/ warlord_complete_map.tif（432KB）/ warlord_spear_complete.tif（768KB）/ 2×.mat（Built-in 老 shader）/ Bruce_Prefab.prefab（29.8KB，vendor demo）/ bruce_notes.txt / bruce_animationList.txt。
- **无 demo scripts、无示例场景、无 vendor shader、无 sample UI**——五的排除清单在本包天然为空。

## 3. Imported subset（工作令 五/六/七）

- 导入目录（遵循仓库既有 `Assets/Art/Player/DarkKnight/{...}` 惯例，语义组=Enemies）：`Assets/Art/Enemies/DragonWarlordBruce/{Source,Textures,Materials,Prefabs}`。
- 导入集（GUID 保持式：包内缺 asset.meta，按 pathname 第二行原始 GUID 手写最小 meta，Unity 导入时保留 guid 并归一化 importer 设置→材质/预制体跨文件引用零断链）：
  - Source/：bruce.FBX（+归一化 ModelImporter meta）+ bruce_notes.txt + bruce_animationList.txt（vendor 文档=溯源证据）
  - Textures/：warlord_complete_map.tif（512²）、warlord_spear_complete.tif（512²）
  - Materials/：warlord_complete_map.mat、warlord_spear_complete.mat（已迁 URP/Lit）
  - Prefabs/：BruceTrial.prefab（项目侧 wrapper）+ BruceTrial.controller（项目侧，含内嵌 Generic Avatar）
- **剔除**：Bruce_Prefab.prefab（vendor demo 预制体：Unity 6 下加载为多根空壳+一个 Light 组件——五/三十/三十一达标；原件保留在导演 RAR，未销毁）。
- 总量≈10.5MB（FBX 占 9.2MB）。未触碰：4.8GB PBR 包/POLYGON/critterpack/21 组 PNG（三）。

## 4. Rig（工作令 八）

- **RigType=Generic**（ModelImporter.animationType=Generic）。骨架=44 骨 biped 系（龙形武将+双矛），非人形语义不满足 Humanoid 映射——按令「不得为 Avatar validation 变绿强行人形」选 Generic；20 clips 在 Generic 下同骨架直播无重定向需求。

## 5. Animations（工作令 九/二十四）

- 源=单一 Take 001（855 帧 bip 时间线，vendor 动画清单 20 段）。已按清单在 ModelImporter.clipAnimations 切为 **20 个具名 clip**（combat_mode/windmill_blow/left_straight_blow/right_straight_blow/scissors_blow/double_blow/two_straight_blows/two_straight_jump_blows/two_stabbing_attacks/looking_left/looking_right/taking_hit/move_back/move_front/move_left/move_right/defending/dying/run/walk；loop 标记=combat_mode/looking×2/move×4/defending/run/walk；vendor 清单第 6 条「165-255」按前段衔接修正为 210-255，如实记录）。
- 五类映射：**Idle=combat_mode ✓ / Move=walk+run ✓ / Attack=8 种 blow ✓ / Hit=taking_hit ✓ / Death=dying ✓——零 MISSING**。
- Root motion：**关闭**（applyRootMotion=false，二十九）；wrapper controller 为 6 状态最小集（Idle/Walk/Run/Attack/Hit/Death，默认 Idle；QA 用 `Animator.Play()` 直驱），**状态间过渡接线=正式集成 follow-up**（本原型控制器不含 trigger/float 条件链）。

## 6. Materials / URP conversion（工作令 十/十一）

- 源材质=Built-in（Nature/Tree Soft Occlusion Leaves + Legacy Shaders/Diffuse）→ 全部迁 **URP/Lit**：_BaseMap=对应 512² TIF、_BaseColor=白、_Metallic=0、_Smoothness=0.15；无 normal/emission/alpha（TIF 实测 alpha=False——十二「alpha 是否实际需要」=不需要，保持 Opaque）。未引入 HDRP/vendor pipeline/整套 shader framework（十达标）；未美化改变源辨识度（十一）。

## 7. Texture observations（工作令 十二）

- 2×512² TIF（与 vendor 自述一致）；导入修正过程=TextureImporter textureShape 曾因最小 meta 缺省被判 TextureCube→显式 textureType=Default+textureShape=Texture2D 重导入（详见 §18）；无 8K 异常、无预算问题；运行时内存≈2×512² 压缩贴图（可忽略量级）。**未发明全项目 texture budget**（按令仅观察记录）。

## 8. Prefab component audit（工作令 十三/三十/三十一/三十二）

- **BruceTrial.prefab 全组件清单**：Transform×N（骨骼层级）+ Animator×1（根，Generic avatar+controller+applyRootMotion=false）+ SkinnedMeshRenderer×1（主体）+ MeshFilter/MeshRenderer×4（spear_01/spear_02=双矛挂 Bip01 L/R Hand、left_shoulder/right_shoulder=肩甲挂 Clavicle——全部挂骨骼下，随动画移动）。
- **vendor MonoBehaviour=0；Collider/Rigidbody/Camera/Light=0**（ vendor demo Light 随 vendor prefab 剔除）；无第二个 gameplay physics；美术 prefab 不含 HP/AI/Damage/Loot（十三达标）。
- 测试 `BruceTrial_NoGameplayComponents_NoVendorScripts` 逐 transform 钉住（三十六）。

## 9. Scale（工作令 十五/十六）

- 实测：FBX 原始世界高=1012.9 单位（1 SMR localBounds×importer 换算）→ wrapper 等比 scale=**0.00257** → 可见高 **2.60m**。
- DarkKnight-relative：DarkKnight 可见高≈1.19m（renderer 口径实测 1.03）→ **Bruce/DarkKnight≈2.2×（renderer 口径 2.5×）**；对普通木桩（≈0.58）≈4.5×——Boss 体量感明确，俯视镜头不遮画面。
- **视觉 scale 与 gameplay collider 解耦（十六）**：本轮零 gameplay 改动（hitbox/attack range/nav radius 未动——Runtime delta=0 实证）。

## 10. Ground contact（工作令 十四）

- wrapper=根（scale 0.00257）+Model 子级（localPosition.y 修正贴地；首版因「localPosition 在根缩放坐标系」的换算差沉 7.8cm，已按世界量补偿修正并在 play 实例验证）。Play 实测：站立贴地（脚/尾触地）、Walk 迈步贴地、Death 倒地不穿地不漂浮（截图 bruce_coexist/bruce_close_walk/bruce_death）。

## 11. Gameplay camera / readability（工作令 二十/二十一）

- 全部截图用**当前实际 gameplay 相机**（Game View capture，非 Scene View）：常态同屏（bruce_coexist）、近战距离（bruce_close_walk——Bruce 与 DarkKnight 近战距并立）、Attack 态（bruce_attack）、Death 态（bruce_death）。
- 判定：silhouette（角冠+双矛+长尾）独特清晰；近/常距下武器/肢体/材质分离清楚；色彩鲜明（橙红甲+青绿鳞 vs DarkKnight 暗黑）——同屏信息可读性高；Hit 态由 controller 状态与 taking_hit clip 侧验证（未单独截图，如实记录）。

## 12. DarkKnight style comparison / Nine-axis verdict（工作令 二十二/二十三）

| 轴 | 判定 | 依据 |
|---|---|---|
| 1. Style Fit | **CONDITIONAL** | Bruce=手绘低模（鲜明色块/金属细节/512² 手绘贴图），DarkKnight=暗黑写实（低亮度盔甲）。同屏并存可读、色彩分离好，但风格谱系不同；工作令二十三要求按「较写实、暗黑」方向评估——该方向性裁定（是否接受与 DarkKnight 混用）归导演/规划 AI，执行 AI 不代拍 |
| 2. Silhouette | PASS | 角冠/双矛/长尾轮廓独特，俯视一眼可辨 |
| 3. Rig | PASS | 44 骨 Generic 稳定导入，装配/引用零断链（GUID 保持式导入） |
| 4. Animation | PASS | 五类全齐零 MISSING；20 clips 全部可播；walk 帧序交替正常无明显滑步 |
| 5. Scale | PASS | 2.60m≈2.2×DarkKnight，Boss 感明确不遮镜头；等比缩放 |
| 6. Materials/URP | PASS | 2 材质成功迁 URP/Lit，无错误 shader；无 alpha 需求如实记录 |
| 7. Ground Contact | PASS | 站/走/倒三态贴地正常 |
| 8. Gameplay Readability | PASS | 攻击/行走/死亡俯视可读；视觉与 gameplay 解耦零干扰 |
| 9. Performance Cost | PASS | 1738 verts/1822 tris/5 renderers/2 mats/2×512²——成本极低 |
| **Final** | **ACCEPT WITH FOLLOW-UP** | 技术轴全 PASS；follow-up=①风格混用导演/规划裁定 ②controller 过渡接线 ③Hit 态游戏内验证 ④Boss gameplay 位定义（本轮禁止创建） |

## 13. Test result（工作令 三十六）

- `BruceArtTrialTests` 10 项（EditMode 138→**148**）：PrefabLoads / NoMissingScripts / RequiredRenderers_Present（1 SMR+4 MR、空材质槽 0）/ WrapperScale_FinitePositive_BossHeight（2.6±0.2m 断言）/ NoGameplayComponents_NoVendorScripts / Materials_UrpLit_NoErrorShader（BaseMap 必挂）/ AnimatorReferences_Valid（6 状态+默认 Idle+root motion off）/ SourceHierarchy_StableClips（Generic+≥20 clips+五类代表 clip+骨骼>40）/ Textures_Default2D_512 / NoResourcesFolder_NoVendorPrefab。
- 适应实际资产结构：未硬编码 vendor 层级名（按组件类型/角色断言）。

## 14. PlayerRuntime result（工作令 三十九）

- Bruce **未接入默认玩家可见 Runtime 路径**（QA 实例=play 模式运行时 spawn，未存场景；无场景/预制体引用→不入构建）。仍执行 canonical Gate：SelfTest PASS → `-IncludePlayerRun` **PASS exit=0**：EditMode 148/148 + PlayMode 3/3 + ContentAudit fresh + PlayerBuild win64（exe 667136B/158 files，与 R3 轮完全一致=Bruce 资产未进构建）+ PlayerRun PASS exit=0 三档 100/200/300。
- 如实记录：本轮 PlayerRun 实测分辨率=1920×1080（此前几轮=2560×1440）。该层启动参数本就不带分辨率（仅 Performance 层带 -screen-width/-height 2560/1440），实测值随系统默认窗口而变；PASS 判定不依赖具体值（契约=三档一致+合法+editor=False，均满足）。

## 15. Performance result / reason not formal（工作令 四十/四十一）

- **非 formal Performance Gate**（Bruce 仅存在于非默认 Art Trial 预览，未进默认 Arena/benchmark Runtime——四十条款命中「不需要 formal」分支）。
- 记录：renderer count=5（1 SMR+4 MR）；material/submesh=2 材质；vertices=1738 / triangles=1822（超低模，比现有任一实体低一个量级）；frame observation=play 预览 60fps 视觉流畅无掉帧（编辑器观察，非正式证据）。无任何为性能作弊的动作（四十一）。

## 16. Runtime delta / Gameplay delta / Content delta / Audio delta（工作令 二十六/二十七）

- Runtime delta=0（Runtime/Core 零文件改动——git status 实证）；Gameplay delta=0（EnemyId/Boss AI/loot/skill/spawn 均未创建）；Content delta=0（Content Audit fresh PASS——Art 文件未被误判，三十八达标，无需改 Audit）；Audio delta=0（无 roar/SFX，Phase 4 继续 GATED）。
- 允许的 delta=Art asset/prefab（Assets/Art/Enemies/** + 测试）。

## 17. UI delta / R4（工作令 四十四）

- UI 零改动（本轮不碰 Tooltip/HUD）；R3 保持 COMPLETE；R4 继续 NOT STARTED / PLANNING REVIEW REQUIRED。

## 18. 本轮过程事实（如实记录）

- 导入链三处修复：①包内无 asset.meta→按 pathname 第二行原始 GUID 手写最小 meta（跨文件引用零断链）；②TIF 因缺 importer 设置被判 TextureCube→显式 textureType=Default+textureShape=Texture2D；③vendor prefab 为损坏 demo（多根+Light）→剔除并自建 wrapper。
- 贴地首版沉 7.8cm（child localPosition 忘除以根缩放）→按世界量修正。
- 交互式编辑器在 FBX 20-clip 重导入期间主线程长阻塞（eval 超时）→强关后由 canonical Gate 批处理完整重导（全部通过），视觉 QA 在重开后完成。
- Bruce 未在本轮接入任何默认 Runtime 路径、未创建 Boss system/第二只怪/解压 4.8GB 包（四十五达标）。

## 19. 下一步（按工作令 四十五，待规划 AI 决策）

- Bruce 判定已写入 `MODEL_ASSET_SCREENING_R1.md`（ACCEPT WITH FOLLOW-UP）。若规划 AI 采纳：正式集成（controller 过渡接线+Boss 位定义+入默认 Runtime 路径时补 -IncludePerformance Gate）或据此定普通怪精模批量策略/下一候选；若导演否决风格：REJECT 路径附九轴失败原因供筛选。
