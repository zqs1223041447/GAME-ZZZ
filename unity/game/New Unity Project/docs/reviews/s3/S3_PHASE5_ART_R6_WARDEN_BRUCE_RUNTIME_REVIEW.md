# S3-PHASE5-ART-R6 WARDEN BRUCE RUNTIME 复核与收口报告

工作令：S3-P5-ART-R6-WARDEN-BRUCE-INTEGRATION（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。Baseline=93bb08a（R5 收口 HEAD，已 push）。

## 0. Verdict

**PASS — WARDEN BRUCE VISUAL INTEGRATED**（第三正式敌人视觉经共享表现链接入；导演 2026-09-08 四项指示（玩家血量/出图地图内容/runInBackground/移动动画根因修复）同轮落实并如实记录为 Gameplay delta；限制项如实记录不改变 verdict 成分）。

## 1. Planner Warden/Bruce rationale（§2 转述）

Warden=每图 1 只、HP110/甲140/重装精英位；Bruce=R1 已验收的 Role-Specific 高价值候选（≈2.60m、双武器/长尾/高辨识 silhouette）。两者自然匹配：**已有 Warden gameplay identity → Bruce visual identity**，非新 Boss/Elite 系统/新 EnemyKind。

## 2. Catalog before/after（§7）

- Before（R5）：Brute→TrollWarriorVisual、Stinger→FireLionVisual、Ashling/Warden/Dummy→null。
- After（R6）：+ **Warden→BruceVisual**（`EnemyVisualCatalog` 单一映射表内新增，无第二套 dictionary）；Ashling/Dummy 仍 null。

## 3. Formal Prefab（§5/§6）

- `Assets/Resources/Enemies/BruceVisual.prefab`：由 R1 已验收 `BruceTrial.prefab` 派生（Trial 历史资产原样保留）；根 wrapper scale=**0.0025668899**（R1 验证值，世界高≈2.60m）、root motion off、组件审计=零 MonoBehaviour/Collider/Rigidbody/CharacterController/Camera/Light（测试钉住）。
- Resource path=`Enemies/BruceVisual`（`RuntimeResourcePaths.EnemyBruceVisual` 单一真相源）；缺失回退基元（R4 失败缓存沿用）。
- 正式 Runtime 不依赖 Trial 命名 prefab（§5 合规）。

## 4. Scale / Ground / Weapon-Bone（§9/§10/§11/§19）

- Scale：R1 值 2.60m 作为起点，正式 Runtime 镜头验证（r6_5 近距攻击同框：Bruce 明显高于 DarkKnight≈1.19、低于误传「全新 Boss 机制」的夸张度）——**无需 visual-only 调整**。
- Ground：由预制体自带 offset（R1 验证）+Mount 位置归零承接；Death 后 0.40s 回收隐藏。
- 武器/肩甲 child 跟骨（R1 已确认）；正式挂载 move 不脱骨、attack 不飞武器（QA 实拍）。

## 5. Animation Mapping（§12-§16）

- 控制器六态：Idle/Walk/Run/Attack/Hit/Death 全部有 Motion 绑定（YAML 级验证）；表现层消费 Idle/Run/Attack/Hit/Death 五类通用语义（Move=Run；Walk 不伪造进表现状态机）。
- 信号源全部既有：Move=AnimState.Run、Attack=AttackExecutions 边沿、Hit=HitFlash+AnimState.Hit、Death=alive 边沿、Ignite=IgniteRemain>0（R5 通用反馈自动生效，零 Bruce 特例分支）。
- New gameplay state：0。
- **通用根因修复（导演指示④）**：`EnemyVisualPresenter.RequestState` 原先每帧 `Play(state,0,0f)` 把循环 clip 从头重启——表现为「所有怪物没有移动动作」。修复=同状态不重播（攻击/受击/死亡边沿 force 重播）。运行期实证：Run 态 animator normalizedTime 连续推进至 30+ 周（不再钉在首帧）。

## 6. R5 Feedback Parity on Bruce（§15-§17）

- Ignite→暖灼 tint、Hit+Ignite→Hit 优先、Hit 结束→Ignite、Ignite 结束→原色（PlayMode 断言）；多 Renderer×多材质槽全覆盖、原色缓存精确恢复、sharedMaterial 引用不变、runtime clone=0。

## 7. 真实 Runtime QA（§24/§25，gameplay 相机直渲，本地 `Assets/Temp/qa/r6_*`）

- r6_1 三方同屏（Troll+FireLion+Bruce+DarkKnight）/ r6_2 Bruce Ignite / r6_3 Bruce Hit-while-Ignite / r6_4 Bruce 恢复原色 / r6_5_bruce_attack（近距攻击，攻击执行 exec=1 程序化触发）/ r6_5_bruce_melee_0..7（逼近连拍）/ r6_6 狮 Ignite+Bruce 同屏 / r6_7 狮恢复 / r6_8 Bruce 死亡坍塌。
- Threat Readability（§25）：①不看名字可与 Brute/Stinger 区分（龙鬃双武器人形 vs 驼背持刃/四足狮）✓；②尺寸符合高 HP/高甲角色（2.6m vs Troll 2.5m/狮 1.4m/玩家 1.19）✓；③不误传「新 Boss 机制」（无 Boss 演出/Aura，纯体型+剪影）✓；④每图 1 只形成视觉重点 ✓；⑤DarkKnight 近战仍可读 ✓。
- QA 过程事实：Warden 在未清场时会被先到怪群挡在停步距离外（真实行为）——QA 与 PlayMode 测试均采用「清场后单挑」确定性路径；曾出现会话级 prefab 加载失败缓存致全员回退基元（重启编辑器后不复现，属编辑器长会话累积状态，已如实记录）。

## 8. 导演 2026-09-08 四项指示（超出工作令范围，如实记录）

1. **玩家血量**：`SliceRules.PlayerBaseLife` 80→**9999999**（QA/游玩期不再被围杀打断表现验证）；死亡路径可测性以测试致死量同步抬升保持（`SliceLoopTests` 9999→99999999，行为断言不变）。
2. **出图后地图内容**：移除 Town 默认与死亡/回城路径的 `SpawnDummies(8)`（黄色基元木桩围城）；F1/F2/F3 手动基准生成保留；Harness 不受影响（`-arenaPerf` 自行 spawn）。
3. **后台冻结**：`ProjectSettings.runInBackground` 0→1（项目设置）+`Application.runInBackground=true`（运行时强制）——切后台不再冻结。
4. **移动动画缺失**：§5 根因修复（每帧重启→同状态不重播）；「部分模型没有攻击动画」同源于此（状态被连续重启淹没），修复后攻击/受击边沿 force 重播。
- 以上为**导演指令的 Gameplay delta**（第 1/2 项改变 gameplay 数值与出图内容），非工作令授权范围——由导演直接下令，规划 AI 知情处理（本轮汇报如实上报）。

## 9. Tests（§27-§29）

- EditMode +7（`BruceVisualContractTests`）：契约路径加载/visual-only 组件审计/六态+root motion off（控制器层枚举；prefab 未实例化时 `Animator.HasState` 不可靠，如实记录并改用 AnimatorController 枚举）/wrapper scale 有限为正≈2.6m/Mount 保留预制体根 scale+gameplay root 零改动（R4 根因不回归）/R5 反馈支持 Bruce 多材质槽+Hit 优先+精确恢复+sharedMaterial 不变/gameplay 常量护栏。
- PlayMode +1（`BruceVisualPlayModeTests`）：真实 Warden——Bruce 挂载+placeholder 不重复显示/Run 真实触发/R5 反馈 parity（Ignite/Hit 优先/恢复）/清场后真实攻击 exec 递增→Attack 状态/死亡真相 AliveCount-1→Death 状态。
- 既有测试全绿：BruceArtTrialTests/Troll 各套/Catalog/Feedback/LoopTests 全 PASS（唯一预期同步=`SliceLoopTests` 致死量随 HP 指示抬升，属根因同步非掩盖）。
- 资源契约：REQUIRED 8/8→**9/9**（+BruceVisual，declared-key parity+真实加载 PASS）。

## 10. Build / Gates（§30-§33）

- Build：PASS win64（exe 667136B/Data 158 files——Bruce 网格/贴图/材质与上轮同包体系，PlayerRun 实证 Bruce 在 Player 中可见）。
- Player Runtime Gate：SelfTest PASS + EditMode **191/191** + PlayMode **9/9** + Audit fresh PASS + Build PASS + PlayerRun PASS exit=0。
- Performance Gate：contract/workload 零改动，**9/9 PASS**——worst avg=1.898ms（**22.8%**）、worst p99=5.403ms（**64.8%**，Run3-100 单尖峰，仍在预算内）、gpu≤0.587ms；不创建新 baseline。
- **Harness 代表性**：canonical 密度档=Dummy → **Harness represents Troll/FireLion/Bruce = NO**（如实记录）；PASS 只证明全局未回归，不声称「300 Bruce 已证」；未改 Harness/contract/budget（§34 合规）。

## 11. Scope（§33-§40）

- EnemyKind delta=0（无新 kind/Boss/Elite）；Gameplay delta=**导演 4 项指示（§8：PlayerBaseLife/出图木桩/runInBackground/动画根因修复）**；Content delta=0（数量护栏不变）；UI delta=0；Audio delta=0（Phase 4 GATED）；新素材导入=0（Bruce 用已入库 R1 资产）；其余素材（TheTroll/Wolf/Bats）不动。

## 12. Resource Contract / Audit（§20/§21/§37）

- REQUIRED 9/9 真实加载 PASS；0 手写独立 Bruce loader（共用 catalog+失败缓存）；缺资源=回退基元不 crash 不逐帧重试。
- Content Audit fresh PASS；内容数量 0 delta。

## 13. Phase 状态（§44/§46）

- Phase 5：仍 **IN PROGRESS**——three formal enemy visuals integrated（Troll/FireLion/Bruce）；Ashling 仍 placeholder（是否补第四视觉待规划 AI 裁定）。
- 未自行启动：第四视觉、300-art density benchmark、Boss system、Death corpse system、VFX、Phase 3 R4、Voice。

## 14. Reviewer Findings / Fixes

- Findings：①prefab 未实例化时 `Animator.HasState` 返回假阴性（改控制器层枚举断言，运行期实例上正常）；②Warden 未清场时被怪群挡在停步距离外（真实行为，QA/测试以清场确定性路径处理）；③编辑器长会话累积状态导致会话级 prefab 加载失败缓存（全员回退基元一次，重启编辑器不复现，如实记录）；④`RequestState` 每帧重启 clip 的通用表现缺陷（导演观察④，本轮修复并实证）。
- Fixes：如上，无遗留。
- Final verdict：**PASS — WARDEN BRUCE VISUAL INTEGRATED**。

## 15. DECISIONS / MODEL_ASSET_SCREENING（§42/§43）

- DECISIONS：R3 条目（规则⑫）下追加 R6 事实行：**Warden 使用 Bruce 作为正式 role-specific high-value visual——是既有 EnemyKind 的 presentation binding，不新增 Boss/Elite gameplay taxonomy，不写成 BossKind**。
- MODEL_ASSET_SCREENING：Bruce 行更新为 **ACCEPTED ROLE-SPECIFIC — FORMALLY INTEGRATED AS WARDEN VISUAL**（保留未来高价值/Boss 候选的历史定位）。

## 16. 提交（§47）

- art(s3): promote bruce to formal warden visual → feat(art): bind warden visual through shared catalog → test(art): cover bruce runtime and feedback parity → docs(art): review warden bruce integration → STATUS 回填；普通 push 禁 force。
