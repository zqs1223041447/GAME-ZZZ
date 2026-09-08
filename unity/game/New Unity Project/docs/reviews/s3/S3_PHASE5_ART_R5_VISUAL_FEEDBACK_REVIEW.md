# S3-PHASE5-ART-R5 VISUAL FEEDBACK PARITY 复核与收口报告

工作令：S3-P5-ART-R5-VISUAL-FEEDBACK-PARITY（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。Baseline=6ca46f0（R4 收口 HEAD，已 push）。

## 0. Verdict

**PASS — FORMAL ENEMY VISUAL FEEDBACK PARITY ESTABLISHED**（Troll/FireLion 两套正式 PBR 视觉的 Hit/Ignite 反馈统一收口；限制项如实记录，不改变 verdict 成分）。

## 1. R3/R4 Known Gaps（本轮动机）

- R3/R4 连续记录 `IGNITE_TINT_NOT_PRESENTED`：Ignite DoT 是核心战斗 truth，但正式 PBR 视觉未表现。
- 若先接第三只模型，同一缺口会被复制；本轮先把共享管线补完整。

## 2. Canonical Gameplay Truth（§5，只读事实）

- **Hit**：`Dummy.HitFlash`（`CombatRules.HitFlash`=0.20s，`ApplyHit` 写入、`DummyCrowd.Tick` 衰减）——R3 起已消费。
- **Ignite**：`Dummy.IgniteRemain`（+`IgniteDps`），canonical 入口 `DummyCrowd.ApplyIgnite(index, dps, duration)`（ArenaSim 真实点燃成功后调用），`DummyCrowd.Tick` 递减并经 `AfterResistance` 产 DoT 伤害。**该字段本就对 view 层只读可读**（ArenaDirector 基元路径已有 `d.IgniteRemain > 0f` 消费先例），故本轮**零新增 observation 字段、零新增 gameplay state**。
- 死亡回收/HitFlash 时长等常量全部未动（护栏测试钉住）。

## 3. Read-only Observation Changes（§7）

- 无新增字段。ArenaDirector 视图循环以 `d.IgniteRemain > 0f` 派生只读 bool 传入表现层；不复制 timer、不倒计时、不参与 gameplay 判定、presentation 无写回。

## 4. Presenter Feedback Architecture（§8/§11/§17/§18）

- 新增**纯 presentation helper `EnemyVisualFeedback`**（普通 C# 类，非 MonoBehaviour）：职责仅为 {Normal/Ignite/Hit} → {渲染器 tint} 映射；不拥有 timers/conditions/gameplay 引用。
- 状态映射纯函数 `Compute(alive, hitFlash, ignited)`：死亡→Normal；`hitFlash>0.02`→Hit；`ignited`→Ignite；否则 Normal——确定性、可单测。
- 优先级 **Hit > Ignite > Normal**（§9）：Hit+Ignite 同存显示 Hit；Hit 结束自动回 Ignite；Ignite 结束精确恢复原色。
- tint 与 Animator 正交（§10）：Attack+Ignite、Hit 动画+Hit tint 均合法；Ignite 未加入 Animator 状态机。
- **仅状态变化时写 renderer**（§18）：`_written/_last` 追踪，Normal↔Ignite↔Hit 变化帧才写 property block，无每帧重复写。

## 5. MaterialPropertyBlock Design（§11-§14/§16）

- 实现=`MaterialPropertyBlock` 对 URP/Lit `_BaseColor` 做 tint（`Shader.PropertyToID` 缓存 id）；**全链路零 `renderer.material` 实例化**（§26 护栏测试+静态 grep 双证：新增代码 `.material` 命中=0）。
- **原色缓存**（§12）：挂载时对每 Renderer×材质槽从 canonical sharedMaterial 读 `_BaseColor` 缓存（不假定白色）；tint 以缓存原色为基底 Lerp；Normal 以缓存原色写回（精确恢复）。
- **多 Renderer/多材质槽**（§13）：Troll 3 SMR、FireLion 1 SMR 全覆盖；逐 index 独立 property block（`SetPropertyBlock(block, index)`）。
- **Shader Property 安全**（§14）：`HasProperty(_BaseColor)` 不成立的槽安全跳过（空槽/null 材质同样跳过），无 exception 路径（负向测试覆盖）。
- Tint 常量（§15，presentation-only）：Ignite=Lerp(原色, (0.95,0.42,0.12), 0.55)（与基元 placeholder AshColor 同族同强度，保留 texture/PBR 可读性、不「红色塑料」）；Hit=Lerp(原色, (1.00,0.97,0.92), 0.95)（短促更强，与 placeholder HitColor 语义一致，无高频刺眼白闪）。
- 未使用 Emission（§16）：仅 BaseColor tint，不动 shader keyword，零材质变体风险。
- 每帧分配（§17）：所有 MPB/颜色数组在挂载时一次性构建；无每帧 new、无 LINQ、无热路径集合。

## 6. Presenter 无 Gameplay Write（§19）

- `EnemyVisualPresenter`/`EnemyVisualFeedback` 新 API 仅 `ApplyFeedback(alive, hitFlash, ignited)`（presentation update）与只读观察 `LastFeedbackState`；无 damage/condition/timer 写 API。**反射护栏测试**对两类型全部公共方法名断言禁用集（ApplyHit/ApplyIgnite/SetIgnite/AddIgnite/RemoveIgnite/SetHitFlash/ApplyDamage/Kill）=0 命中。

## 7. Placeholder（§20）

- Ashling/Warden 基元 placeholder 原有 hit/tint 表现未动、未回归（本轮零改动其路径）；未为统一而重写 placeholder。

## 8. Tests（§24-§27/§30）

- **EditMode +8（169→184 套件含 R4；新增 `EnemyVisualFeedbackTests`）**：状态映射确定性+优先级（Hit>Ignite>Normal、死亡→Normal）/多 Renderer×多材质槽 Normal 精确恢复/Ignite 与 Hit tint 互异+Hit 结束回 Ignite+Ignite 结束精确 Normal/非白原色为 tint 基底/sharedMaterials 引用前后不变+零 runtime clone（`(Instance)` 名称=0）/无 `_BaseColor` 槽与空槽安全跳过/Presenter·Feedback 反射无 gameplay 写 API/Troll+FireLion 真实预制体经 presenter 进入 Ignite 且退出精确恢复。
- **PlayMode +2（6→8，新增 `EnemyVisualFeedbackPlayModeTests`）**：
  - Test A — Troll Ignite（真实 Brute 实体）：canonical `ApplyIgnite` 触发→presenter=Ignite→跨帧维持→DoT 照常扣血（hp 下降）且 alive 真相不变→收窄时长至到期→精确 Normal。
  - Test B — FireLion Hit/Ignite（真实 Stinger）：Ignite→非致死 ApplyHit→**Hit 优先**→HitFlash 衰减完成（Ignite 仍在燃烧）→回 Ignite→到期→Normal。
  - 时间窗全部按模拟值判定（`IgniteRemain`/`HitFlash` 阈值），不按真实秒数假设（过程教训：测试环境 sim 时钟≈4×真实时钟，真实秒数断言不可靠——已根除）。
- **Gameplay Constants Guard**（§28）：R4 契约测试继续钉住 DummyPoolSize=300/DeathRecycle=0.40/HitFlash=0.20 等。
- **Death 截断边界**（§29）：KNOWN PRESENTATION LIMIT — GAMEPLAY LIFETIME PRESERVED 维持，本轮未触碰 DeathRecycle。
- **DoT 不变性**（§30）：Test A 直接断言 hp 下降/alive 不变/时长由 gameplay 到期——相同场景视觉开闭前后 gameplay truth 唯一来源。

## 9. 真实 Visual QA（§31/§32，gameplay 相机直渲，本地 `Assets/Temp/qa/r5_*`）

- r5_1 双视 Normal / r5_2 Troll Ignite（暖灼 tint 可辨、texture 仍可读）/ r5_3 Troll Hit-while-Ignite（Hit 白闪更强）/ r5_4 FireLion Ignite / r5_5 FireLion Hit / r5_6 双视到期恢复原色（银白/深蓝无残留）。
- Ignite 经 canonical `ApplyIgnite`（ArenaSim 真实点燃同款入口，deterministic setup path §23 合规；无 production Debug setter）。
- 验收回答（§32）：①Ignite 正常战斗距离可辨（暖色 vs 银白/深蓝原色，非永久红皮——到期即恢复）✓；②不会误认为永久红色皮肤（恢复画面实证）✓；③Hit 比 Ignite 更瞬时更强 ✓；④PBR texture/normal 可读性保留（Lerp 0.55 非全替）✓；⑤Troll/FireLion 两不同模型表现一致（同一共享 helper）✓；⑥多敌同屏噪声可控（仅受击/燃烧者变色）✓。
- Lion QA 截图距离较远（Stinger 游击走位），状态判定以程序化钩子（`LastFeedbackState`）+与 Troll 完全相同共享路径为准，如实记录。

## 10. 默认地图观察（§41，非硬门）

- InMap 会话实测：presenters=16（Troll+FireLion 多只）、renderers=68、**runtime 材质实例=0**（property block 路径零克隆实证）；多 Troll+FireLion 正常、Ignite 多目标无卡顿迹象、状态切换正常（QA 会话全程 observation）。

## 11. Build / Gates（§36-§38）

- Player Build PASS（exe 667136B/Data 158 files，Troll/FireLion 照常入包，无 pink/error，property block 在 Player 生效——PlayerRun exit=0）。
- **Player Runtime Gate**：SelfTest PASS + EditMode **184/184** + PlayMode **8/8** + Audit fresh PASS + Build PASS + PlayerRun PASS exit=0（100/200/300 三档）。
- **Performance Gate**：contract/workload 零改动，**9/9 PASS**——worst avg=1.946ms（**23.4%**）、worst p99=2.778ms（**33.4%**）、gpu≤0.572ms；不创建新 baseline。
- **Harness 代表性**（§39/§40）：canonical 密度档=Dummy 无视觉替换 → `ART_VISUAL_RENDER_COST_NOT_REPRESENTED_BY_CANONICAL_HARNESS` 维持如实记录；本轮未修改 ArenaPerfHarness/contract；PASS 只证明全项目 canonical gate 未回归，不得声称「300 个 Ignite Troll 已证」。

## 12. Scope（§33-§35）

- 新模型：0（Lion Head 不续接正式位、TheTroll/Wolf/Bats/Bruce 不动）；Art asset delta=0。
- Content delta=0；Gameplay delta=0（零新字段/零写回/常量未动）；UI delta=0；Audio/Voice delta=0；Particle/VFX system=0（未为「火焰冒火」建粒子）；资源契约 REQUIRED 8/8 保持、0 新 Resources key。

## 13. Reviewer Findings / Fixes

- Findings：①PlayMode 测试初版以真实秒数假设时间窗，测试环境 sim 时钟≈4×真实时钟导致 ignite 提前到期（**表现层行为正确**，测试假设错误）——根因修复=全部时间窗改按模拟值（IgniteRemain/HitFlash）判定；②DoT 跳伤经 ApplyHit 天然产生 HitFlash→Hit 优先级在真实路径自证（初版断言把该帧误判为非 Ignite，修正为「跳伤帧∈{Hit,Ignite} 且 flash 衰减后=Ignite」）；③Stage 采样受后台编辑器节流影响（EditorApplication.update 与 MonoBehaviour.Update 频率分离）——以正式 PlayMode 测试为最终证据。
- Fixes：如上，无遗留。
- Final verdict：**PASS — FORMAL ENEMY VISUAL FEEDBACK PARITY ESTABLISHED**。

## 14. DECISIONS（§43）

- 长期规则⑬（最小新增）：正式敌人 visual 必须消费已有 canonical gameplay truth 呈现关键战斗反馈；presentation 不复制/不决定 gameplay 状态；动态材质反馈走共享材质安全路径（MaterialPropertyBlock），不以 `Renderer.material` 实例化为默认实现。

## 15. Phase 状态与边界（§44/§46）

- Phase 5：仍 **IN PROGRESS**；状态可写 **Formal enemy visual feedback parity established for Troll/FireLion**（不写 COMPLETE）。
- 未自行启动：第三正式模型、300-art density benchmark、Death corpse system、VFX system、Phase 3 R4、Voice、Boss integration——完成后交规划 AI。

## 16. 提交（§47）

- feat(art) canonical enemy visual status feedback → test(art) hit/ignite visual parity → docs(art) R5 review+DECISIONS⑬ → STATUS 回填；普通 push 禁 force。
