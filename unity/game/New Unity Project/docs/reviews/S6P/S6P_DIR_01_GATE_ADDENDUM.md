# S6P-DIR-01 · Gate Addendum（规划 AI 裁定要求的补充证据）

> 来源：规划 AI（Channel A）2026-09-11 对本批导演插单的回复（会话 `game-zzz-planning` / `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`）。
> 裁定要点：① 可达性冲突 → 批准「通行/生效分离」，但另立 **S6P-WO-04A2** 先做，不并入原令；
> ② 共享连接组 → **暂时接受**（附 5 条不变量）；③ 冰冷轴暂缓且**明确不属 WO-03 职权**；
> ④ 本批**不得记作 S6P-WO-05**（该编号＝Passive Overview LOD & Texture Residency），改记 **S6P-DIR-01**；
> ⑤ 收口需补本文件所列 6 项证据。
>
> 本文件只补证据，不重做已完成的工作。

## 1. 怪物攻击动画实机播放证据（裁定第 5 项①②）

**要求**：不能只证明 Run/Walk Loop 修好；要证明 **attack clip/state 真正在实机播放** —— 进入 attack state、normalized time 推进、退出回 locomotion。

**做法**：play mode 实机（不手动 Tick，让游戏自己跑），进图后把一只蛮兵（`EnemyKind.Brute`，Troll 视觉）生成在玩家身边，反复读它的 `Animator`：

| 采样 | gameplay 请求 | Animator state hash | `normalizedTime` | `Bip01_R_Forearm` 世界坐标 |
|---|---|---|---|---|
| 1 | `Attack` | 1080829965 | 0.051 | 0.914, 1.353, 1.062 |
| 2 | `Idle` | 2081823275 | 0.184 | 1.040, 1.348, 1.059 |
| 3 | `Attack` | 1080829965 | 0.311 | 0.610, 1.158, 0.842 |
| 4 | `Attack` | 1080829965 | 0.096 | 0.936, 1.438, 1.103 |
| 5 | `Idle` | 2081823275 | 0.211 | 1.051, 1.349, 1.056 |
| 6 | `Attack` | 1080829965 | 0.328 | 0.641, 1.052, 0.800 |

结论（逐条对应裁定要求）：

| 裁定要求 | 证据 |
|---|---|
| attack state 被进入 | `req=Attack` 时 state hash = **1080829965**（与 `req=Idle` 的 2081823275 不同、且 `length=1.77` 与国际待机 1.67 不同） |
| 推进 | 攻击采样之间**前臂世界坐标位移最大 0.4 单位**（如 0.610,1.158,0.842 → 0.936,1.438,1.103）——骨骼在动，不是冻结姿态 |
| 退出回 locomotion | 采样序列呈 `Attack → Idle → Attack → Idle` 交替；`AttackExecutions` 单调递增（30→31→32→33） |
| 实机而非手动驱动 | 采样期间不调用 `Sim.Tick`，由 play mode 自身帧循环推进；攻击执行门（`ArenaSim.ResolveEnemyAttacks` 需 `State == InMap`）已在 `InMap` 下满足 |

> 备注：攻击 clip 的 `normalizedTime` 采样值不是单调递增（0.051→0.311→0.096→0.328），因为它是**循环 clip**
> 且采样间隔与 clip 长度不成整数倍 —— 因此本节用**骨骼位移**而不是 `normalizedTime` 单调性作为「在动」的判据。

## 2. 四项新机制的分域证据（裁定第 5 项③）

**要求**：冰矛穿透 / 火球命中点爆炸 / 投射物返回 / 狙击印记必须各有 domain 或 PlayMode 级证据，不能只靠特效截图。

| 机制 | 证据（`Assets/Tests/EditMode/S6P_DIR_01Tests.cs` 内的域级断言） |
|---|---|
| 冰矛穿透 | `PiercingProjectile_HitsEachEnemyOnce_AndKeepsFlying`：同一目标一次飞行内 `HitEvents == 1`；`PiercingProjectile_HitsTwoEnemiesInLine`：一线两目标依次结算 `HitEvents == 2` |
| 火球命中点爆炸 | `Fireball_ImpactExplosion_DamagesTargetsAroundImpactOnly`：半径内目标 HP 下降、半径外（x=12）HP 不变；并断言弹体 `ImpactAreaRadius > 0` |
| 投射物返回 | `ReturningProjectile_TurnsAroundAtMaxRange_AndDiesAtPlayer`：到射程尽头 `Returning == true`、`DirX` 反向、抵达玩家后消失；`ReturningProjectile_WithoutSupport_DiesAtMaxRange`：无辅助时行为不变 |
| 狙击印记 | `SnipersMark_AppliedOnProjectileHit_AndDecays`（施加 + 衰减 + 时长精确）、`SnipersMark_OnlyHitsTarget_NotBystanders`（单体）、`SnipersMark_RaisesProjectileDamage_OnMarkedTarget`（同会话同自造 packet，唯一差异=是否已带印记 ⇒ 伤害严格更高） |
| 印记呈现态 | `MarkFeedbackState_LowestPriority_AndThreeArgOverloadUnchanged`（Hit > Ignite > Mark；3 参重载逐位不变） |
| 元素基底 | `Fireball_BaseDamage_IsFireOnly`（`FireFlat > 0 且 PhysFlat == 0 且 IsAttack == false`）、`IceSpear_IsFastPiercingProjectileSpell` |

> 全部为 EditMode 域级/集成级测试（`ArenaSim` + `ProjectilePool` + `SliceSession` 真实路径），非截图。
> 视觉取证（`docs/_dirshots/s6pwo05/`）只作为**呈现层**附加证据，不作为机制证据。

## 3. Multi-Link 不变量证据（裁定第 5 项④：L3 必须证明「不是第三连接组」）

| 裁定条件 | 现状证据 |
|---|---|
| `MaxLinkGroups = 2` | 未新增连接组概念；组划分仍只有 `LinkGroupsText` 的「组0 / 组1」；`TryReassignLink` 仍只接受 `LinkSkill1` 一个改挂位（`ItemInstance.LinkSkill1` 单字段，无 `LinkSkill2`） |
| `LinkSkill2` 不存在 | `ItemInstance` 无该字段（本令未触碰 `Catalogs.cs` 的 `ItemInstance` 定义） |
| 第三连接组不存在 | `SliceSession.SupportsOf` 只有三条返回路径（`QSupports` / `WSupports` / `ESupports`），**数组数量与长度零变化**；冰矛/火球术返回的就是既有 `WSupports` 引用（`NewSkills_ShareProjectileConnectionGroup` 用 `Assert.AreSame` 钉死「同一个数组实例」） |
| Support capacity 不增加 | `SupportCapacity` 实现未改；冰矛/火球术经 `MappedSlot → Body` 得到与弹道**相同**的容量（胸甲孔数−1），不叠加、不新增 |
| existing G0/G1 truth 不改变 | `RebindHostIndex` 对非 1..3 技能仍返回 −1；`RebindCandidates` 仍只枚举 1..3；`LinkGroupsText`/`LinkSourceLabel` 对其余技能行为不变；既有 S5 连接契约测试全绿（EditMode 462/462 内含 `MultiLinkDomainTests` / `MultiLinkIntegrationTests` / `S5BuildInteractionTests`） |

**结论**：冰矛/火球术是**复用既有 G0（弹道）连接上下文**，**不是第三连接组、不新增容量、不改变 G0/G1 真值**。符合裁定条件，裁定「暂时接受」成立。
唯一行为差异（同组三技能显示同一徽章/同一孔位）已登记为 L3；若裁决要求独立孔位，按裁定另开 Multi-Link expansion WO，**不并入 WO-03**。

## 4. 资产清单与授权状态（裁定第 5 项⑤）

见 `S6P_DIR_01_POEDB_SOURCING.md` §7：30 个文件的 **Unity 路径 + 字节数 + SHA-256 + 用途**，逐条来源 URL 见同文件 §1–§4；
授权状态统一标注 **PROTOTYPE / REFERENCE ONLY / NOT PRODUCTION-ADMITTED**（未收到可发行的授权文件；可下载 ≠ 已授权），
解除条件＝导演/法务给出商用授权或替换方案。

## 5. 内容增量与 ProdSim 覆盖面声明（裁定第 5 项⑥⑦）

| 声明 | 内容 |
|---|---|
| 这是**明确的导演授权内容增量** | `SkillId.IceSpear=4`、`SkillId.Fireball=5`、`SupportId.ReturningProjectiles=8`、`SupportId.SnipersMark=9`、以及 30 项外部视觉素材 —— 不得再表述为「无内容变化」。 |
| ProdSim `ec1d3ed67d3035d0` 的**准确含义** | 它证明的是**旧 canonical 场景未被破坏**（既有 3 技能 / 7 辅助 / 物品 / 天赋 fixture 的结算与哈希不变），**不**证明新四条技能/辅助已被 ProdSim 覆盖。 |
| 新轴的覆盖状态 | **未覆盖**：canonical fixture（`PassiveAwareProductionSimulation.CanonicalSkills`）仍只有 Melee/Projectile/Area，未含冰矛/火球术；新技能/新支持的 canonical 覆盖属于后续工作令。 |
| 术语澄清 | 「共享连接组」= 复用既有 G0 组，非新增组（见 §3）。 |

## 6. 本批状态

| 项 | 值 |
|---|---|
| 令号 | **S6P-DIR-01 — Director Supplemental Gameplay & Presentation Intervention**（原误记的 `S6P-WO-05` 已改名，存根见 `S6P_WO_05.md`） |
| 状态 | **IMPLEMENTED / GATE PENDING**（本 Addendum 补齐后等待 Gate Review） |
| 门禁 | 编译 0 error；EditMode **462/462**；PlayMode **17/17**；ProdSim `FNV1A64:ec1d3ed67d3035d0`（未变，含义见 §5） |
| 队列 | 从本文件起调整为：**S6P-DIR-01 Gate Addendum ＋ S6P-WO-04A2 → WO-03 → WO-04B → [WO-04C 条件] → WO-05**（WO-05 仍指 Passive Overview LOD & Texture Residency） |
