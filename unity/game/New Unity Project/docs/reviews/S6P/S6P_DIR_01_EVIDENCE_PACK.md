# S6P-DIR-01 · Evidence Pack

> 工作令：S6P-DIR-01（导演 2026-09-11 补充要求 4/5/6）。实现记录=`S6P_DIR_01.md`；美术来源=`S6P_DIR_01_POEDB_SOURCING.md`；
> 同批导演要求 1/2/3=`S6P_DIRECTOR_FEEDBACK_2026_09_11.md`。

## 1. 工作令 ID 与结论

| 项 | 值 |
|---|---|
| Work Order | **S6P-DIR-01**（导演插入令） |
| 结论 | **IMPLEMENTED — 待规划 AI / 导演验收** |
| 未完成项 | 无「本令范围内」未完成项；§7 列出 4 条**已知限制**（其中 L1/L3 需更高层级裁定） |
| 冲突登记 | 天赋树可达性（14/2429）与 WO-04A fail-closed 门的冲突 → `S6P_DIRECTOR_FEEDBACK_2026_09_11.md` §5（本令未处置，不自选放宽） |

## 2. 实现摘要

| # | 要求（导演原话） | 实现 | 关键落点 |
|---|---|---|---|
| 4 | 「装备图标现在是纯2D，我需你从POEDB.TW上扒取装备图标来用」 | 30 个 poedb 资源落盘；六槽 × 两稀有度共 12 张真实物品图接入背包格与装备卡 | `SlicePoeArt`、`SliceHudIcons.ItemIcon`、`SliceHud.DrawInventoryGrid/DrawSlotCard`、`tools/poedb/fetch_art.py` |
| 5 | 「技能特效顺别也从POEDB.TW上扒取，制作冰茅和火球术」 | poedb 技能/辅助宝石图接入技能槽与辅助托盘；新增 5 条主动技能（含冰矛 R / 火球术 T）并接入弹体特效美术 | `SkillId.IceSpear/Fireball`、`SkillCatalog`、`ArenaSim.ResolveProjectile`、`ArenaDirector.BuildProjectileArt` |
| 6 | 「辅助技能做一个投射物返回和狙击印记的效果试试」 | 两条机制型辅助 + 四项机制（穿透 / 返回 / 命中点爆炸 / 单体印记增伤） | `ProjectilePool`（返回与不重复命中位图）、`ArenaSim.ResolveProjectileHit/ResolveImpactArea`、`DummyCrowd.ApplyMark/TickMarks` |

## 3. 变更文件

### 3.1 运行时（`Assets/Runtime/Core/Gameplay/`）

| 文件 | 变更 |
|---|---|
| `SlicePoeArt.cs` | **新增**：poedb 美术加载层（懒加载 + 缓存 + 缺失回退 null；`DeclaredKeys` 单一真相；`ItemArt/SkillArt/SupportArt/ProjectileArt` 映射） |
| `Catalogs.cs` | `SupportId` +2（`ReturningProjectiles=8`、`SnipersMark=9`，`Count=10`）；两条 `SupportDef`；`SliceRules` +4 常量（新技能进图基础伤害、狙击印记数值/时长） |
| `CombatTypes.cs` | `SkillId` +2（`IceSpear=4`、`Fireball=5`，`Count=6`）；`SkillDef` +3 字段（`Pierce`/`ImpactAreaRadius`/`BaseDamageIsFire`）；`SkillCatalog.Defs` 随 `SkillId.Count` 派生 + 两条新技能定义 |
| `Kernel.cs` | `SkillTags.Of` 覆盖 5 技能（冰矛=法术弹道物理轴；火球术=纯火焰法术弹道+范围） |
| `SliceSession.cs` | 冰矛/火球术共享弹道连接组（`SupportsOf`/`MappedSlot`/`ConnectionGroupRep`）；支持查重按组代表；`IsSupportCompatible` 机制轴改为 Tag 蕴含；新技能进图伤害与范围缩放泛化；`BuildPlayerHit` 元素基底由 `BaseDamageIsFire` 决定；`HasSupport` 读路径；技能名/热键 |
| `SkillCaster.cs` | `CooldownRemain` 随 `SkillId.Count` 派生（原硬编码 4） |
| `ProjectilePool.cs` | **重写**：`PierceLeft`/`CanReturn`/`Returning`/`HitSlotBase`/`ImpactAreaRadius`；每投射物×每目标已命中位图（穿透与返程共用的唯一「不重复结算」真值）；掉头返程（返程自带距离预算，到玩家即消失） |
| `ArenaSim.cs` | `ResolveProjectile(skill, def)` 三弹道共用；`ResolveProjectileHit` 印记加成 + 打标 + 命中点爆炸；新增 `ResolveImpactArea`；`Tick` 增加 `TickMarks` |
| `DummyCrowd.cs` | `Dummy.MarkRemain`；`ApplyMark`（唯一写入）/`TickMarks`（唯一衰减） |
| `EnemyVisualFeedback.cs` | `EnemyFeedbackState.Mark`（优先级最低）；`Compute/Apply` 增加 4 参重载（3 参重载=既有行为逐位不变） |
| `EnemyVisualPresenter.cs` | `ApplyFeedback` 4 参重载 |
| `ArenaDirector.cs` | R/T 键位与持键连发；投射物美术材质（URP/Unlit 贴图球体 + 每技能底色/尺度，冰矛非等比拉长）；标记态 tint（基元与 presenter 两条呈现路径） |
| `SliceHudIcons.cs` | `SkillGlyph`/`SkillGlyphIsArt`/`ItemIcon`/`SupportGem`（poedb 优先，逐级回退） |
| `SliceHud.cs` | R/T 技能槽接入；槽位/背包格用真实物品图（含等比缩放 `FitAspect`）；辅助托盘绘制宝石图 |

### 3.2 资源（`Assets/Resources/UI/PoE/`）

| 目录 | 数量 | 内容 |
|---|---|---|
| `Items/` | 12 | 六槽 × 普通/稀有 |
| `Skills/` | 5 | 五条主动技能的宝石图 |
| `Supports/` | 9 | 九条辅助的宝石图 |
| `Vfx/` | 4 | 冰矛/火球弹体特效（摄取期抠背景）+ 两条冰矛 MTX 特效（登记未消费） |

### 3.3 工具与测试

| 文件 | 变更 |
|---|---|
| `tools/poedb/fetch_art.py` | **新增**：清单驱动的 poedb 摄取脚本（webp→png、`.meta` 生成、GUID 保留、可选背景抠除；幂等，`--force` 全量重下） |
| `Assets/Tests/EditMode/S6PDir01Tests.cs` | **新增 27 条**：美术契约、技能/辅助身份与定义、连接组语义、兼容矩阵、穿透/返回/印记/爆炸行为、印记呈现态优先级 |
| `Assets/Tests/EditMode/ContentAuditTagRules.cs` | `SkillTagGolden` +冰矛/火球术，新增 `All` 全集 |
| `Assets/Tests/EditMode/SupportCompatGolden.cs` | 矩阵 `3×7` → `5×9`（逐条人工钉死，含负例） |
| `Assets/Tests/EditMode/ContentAuditS2Tests.cs` | 技能枚举改用 `SkillTagGolden.All`；Support 护栏 `基线+1`→`基线+3`；Skill 护栏 `Area=3`→`Fireball=5` |
| `Assets/Tests/EditMode/SupportCatalogInvariantTests.cs` | `SupportId.Count` 8→10、`SupportCatalog.Count` 7→9；矩阵覆盖断言改为按技能全集派生 |
| `Assets/Tests/EditMode/S3R2FireConversionTests.cs` | 总数 7→9；矩阵对拍扩到全部技能（火焰转化自身定义断言全不变） |
| `Assets/Tests/EditMode/ProductionContentReportTests.cs` | 仅断言文案去硬编码（计数已由 audit 派生） |

## 4. 运行时行为变更清单（可复核点）

| 变更 | 旧 → 新 |
|---|---|
| 可用技能 | 3 → **5**（冰矛 R / 火球术 T） |
| 弹体机制 | 命中即消失 → 可**穿透**（冰矛 3）/ 可**返回**（投射物返回辅助） |
| 命中点结算 | 无 → **火球术命中点范围爆炸**（排除已直接命中的目标，避免双算） |
| 单体状态 | 点燃 → 点燃 + **狙击印记**（被印记目标承受投射物伤害 ×(1+0.35)） |
| 元素基底 | 所有弹道=物理基底+附加火焰 → 火球术=**纯火焰基底**（`BaseDamageIsFire`，物理分量为 0） |
| 范围缩放 | 只对 `SkillId.Area` 生效 → 对**任何带半径的技能**生效（否则火球术爆炸静默忽略范围词条） |
| 支持兼容判定 | 技能身份枚举（硬编码 1..3）→ Tag 蕴含（机制轴=`Tag.Projectile`） |
| 连接组 | 3 技能各一组 → 弹道/冰矛/火球术**共享弹道组**（见 WO-05 §4.3 理由） |
| 敌人反馈态 | Normal/Ignite/Hit → +Mark（最低优先级） |
| HUD 图标来源 | 程序化/Aria → **poedb 真实美术优先**（逐级回退不变） |

## 5. 测试与验证证据

### 5.1 门禁

| 门 | 结果 | 命令 |
|---|---|---|
| 编译 | 0 error | `unity command recompile` + `recompile_status` |
| EditMode | **460 / 460 PASS** | `unity command run_tests --mode EditMode` |
| PlayMode | **17 / 17 PASS** | `unity command run_tests --mode PlayMode --async_tests true` + `test_status` |
| ProdSim canonical | `FNV1A64:ec1d3ed67d3035d0`（**未变**，`docs/qa/PRODUCTION_SIMULATION_REPORT.json` verdict=PASS） | EditMode 内含 `ProductionSimulatorTests` |

> 新增 27 条测试的分布：美术契约 6、技能身份/定义 6、辅助身份/兼容 5、投射物返回 2、穿透 2、印记 4、火球爆炸/元素 2、印记呈现态 1（另有 1 条连接组语义，合计 27）。

### 5.2 行为证据（测试内的可观测量，非观感描述）

| 行为 | 断言 |
|---|---|
| 穿透不重复结算 | 同一目标一次穿透飞行内 `HitEvents == 1`；双排目标依次结算 `HitEvents == 2` |
| 返回 | 到射程尽头不消失且 `Returning == true`、`DirX` 反向；到玩家处消失；无辅助时按原规则在射程尽头消失 |
| 印记施加/衰减 | 命中后 `MarkRemain == SnipersMarkDuration`；`TickMarks` 线性递减至 0；单体（旁观者 `MarkRemain == 0`） |
| 印记增伤 | 同会话同自造 packet（基础 100、无 inc/more/crit），唯一差异=目标是否已带印记 → 印记伤害严格更高 |
| 火球爆炸 | 半径内目标受伤、半径外目标 HP 不变；弹体携带 `ImpactAreaRadius > 0` |
| 元素基底 | 火球术 `FireFlat > 0 且 PhysFlat == 0 且 IsAttack == false`；冰矛 `PhysFlat > 0 且 IsAttack == false` |
| 兼容负例 | 集中不接冰矛；火焰转化不接冰矛/火球术（需 Attack+Physical） |
| 连接组共享 | 装在弹道组的辅助对冰矛/火球术可见；对近战不可见 |

### 5.3 视觉取证（play mode 实拍，`docs/_dirshots/s6pwo05/`）

| 图 | 内容 | 同时返回的运行时读数 |
|---|---|---|
| `01_bag_open_poedb_icons.png` | 背包开：装备卡 + 背包格为 poedb 物品图（等比缩放）；辅助托盘 9 个 poedb 宝石图；底栏 Q/W/E/R/T 五个宝石图 | `bag=True inv=4 supports=ReturningProjectiles/SnipersMark` |
| `02_ice_spear_in_flight.png` | 冰矛弹体（冷青细长，poedb 冰矛特效图 + Unlit 贴图球体） | `projAlive=1 fbAlive=0` |
| `03_fireball_impact_explosion.png` | 火球术弹体（poedb 火焰特效图贴图球体） | `alive=3 feedback=0` |
| `04_snipers_mark_and_return.png` | 3 只蛮兵（真实 Troll 视觉）+ 被印记目标（命中闪优先于印记 tint）+ 返程冰矛 | `marked=1 markRemain=7.93 returning=1 hits=4 targetHp=3922` |

**弹体形态的取舍记录**（同一次拍摄四轮迭代，全部实测）：平面广告牌+不抠图=深色方块；平面广告牌+亮度抠图=残余边缘方晕；加色混合=饱和方片；**贴图球体（Unlit）=无方晕且美术本色**，故定为最终形态。这一条连同「poedb 只发布图标级美术」写进已知限制 L2。

## 6. 规范 / 目录 / 能力 / 机制矩阵变更

| 轴 | 变更 | 说明 |
|---|---|---|
| canonical 数据 schema | `SkillId` 4/5、`SupportId` 8/9（追加，旧值零漂移） | 无字段重排、无 payload 变化 |
| `StatId` / `ModOp` / `Tag` / `EffectId` / `EventId` / `ConditionId` / `AffixId` | **全部不变** | 护栏钉死值原样保留 |
| Support 兼容矩阵 golden | `3×7` → `5×9`（45 组合） | 逐条人工钉死，runtime parity 全绿 |
| ProdSim canonical payload | **不变**（`MappedSlot`/连接组设计使然） | canonical hash 仍为 `ec1d3ed67d3035d0` |
| 能力/限制 | 新增 L1（无冷/电轴）、L2（poedb 无粒子）、L3（共享连接组）、L4（印记仅单体增伤） | 见 `S6P_DIR_01.md` §7 |

## 7. 已知限制 / 阻塞项

| # | 项 | 状态 |
|---|---|---|
| L1 | 引擎无冰冷伤害轴 → 冰矛基础伤害走物理轴 | **需裁定**：解除需新增 StatId（会移动 canonical 基线）→ 归 WO-03 ProdSim V3 或专门冻结令 |
| L2 | poedb 不发布粒子系统，只有图标级美术 | 永久事实；弹体=贴图球体，粒子行为引擎自绘 |
| L3 | 冰矛/火球术与弹道共享连接组（不新增 arrays） | **需裁定**：独立孔位需扩 payload（同上移动基线） |
| L4 | 狙击印记只实现单体 + 增伤 | 计划弹道改写（拉向印记）未做，属后续机制令 |
| — | 天赋树可达性 14/2429 vs WO-04A fail-closed | **需裁定**，见 `S6P_DIRECTOR_FEEDBACK_2026_09_11.md` §5 |

## 8. 非阻塞遗留

| 项 | 说明 |
|---|---|
| `Vfx/IceSpearMtx`、`Vfx/IceSpearMtxAlt` | 登记在册、当前未消费（供后续特效档位切换）；资源契约测试要求「声明的必须可加载、目录内不得有未声明文件」，两张均已声明 |
| `S3R2FireConversionTests.SupportAndPassiveConversion_ComposeOnSameAxis` 命名 | WO-04A 遗留的非阻塞改名项（未在本令顺带做，避免与 WO-03 触碰同一文件时冲突） |
| `SliceHud.DrawEmptySkillCell` | 5 槽全部接技能后暂无调用方（保留给后续药剂/空槽复用，不再是死代码路径） |

## 9. 文档更新

| 文件 | 状态 |
|---|---|
| `docs/reviews/S6P/S6P_DIR_01.md` | 新增（实现记录） |
| `docs/reviews/S6P/S6P_DIR_01_POEDB_SOURCING.md` | 新增（逐条来源台账） |
| `docs/reviews/S6P/S6P_DIR_01_EVIDENCE_PACK.md` | 本文件 |
| `docs/reviews/S6P/S6P_DIRECTOR_FEEDBACK_2026_09_11.md` | 新增（导演要求 1/2/3 + 真值冲突登记） |
| `docs/RUNTIME.md` / `docs/COMBAT_MATH.md` | 追加本令运行时真值（技能/支持/机制/弹体结算/印记） |
| `开发计划/UNATTENDED_STATE.md` / `开发计划/规划AI会话.md` | 更新（当前单、进度日志） |

## 10. Git 状态与建议

| 项 | 值 |
|---|---|
| 提交 | 本令**未提交**（沿用无人值守模式的「不擅自 commit」约定） |
| 新文件 | `SlicePoeArt.cs`、`S6PDir01Tests.cs`、`tools/poedb/fetch_art.py`、30 个美术 + `.meta`、4 份 S6P 文档、4 张取证截图 |
| 建议规划 AI 重点读 | `S6P_DIR_01.md` §4.3（共享连接组的不变量理由）、§7（L1/L3）、`S6P_DIRECTOR_FEEDBACK_2026_09_11.md` §5（可达性冲突） |
