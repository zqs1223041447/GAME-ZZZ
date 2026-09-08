# S3-P3-UI-R3 TOOLTIP COMPARE — 复核报告

工作令：S3-P3-UI-R3-TOOLTIP-COMPARE（Phase 3 第 3 轮：统一 Tooltip + 装备词缀对比 + Support 兼容提示）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**PASS**（EditMode 127→138 全绿（+11）；单一渲染器/纯读模型/canonical 兼容路径/union 比较/确定性排序/右缘翻转全部达标；R1/R2 结构零重排；Content/Audio/Art delta=0；Player Runtime Gate 全 PASS exit=0）。

## 1. Baseline

- Baseline=62ff456（Phase 3 R2 STATUS 回填后 HEAD；R2=右侧装备抽屉+Build/Craft 右锚迁移，规划 AI 判定 PASS）。
- 规划 AI 随工作令确认：R1/R2 COMPLETE；授权（许可证）问题不再是 Gate（导演 2026-09-08 裁定，见 §17）；R3 不得混入精模导入、不得自动开 R4。

## 2. Tooltip architecture（canonical sources）

- **纯表现模型 `SliceTooltipModel`**（新文件）：Card 结构（Title/Subtitle/Badge/Body/ContextTitle/Context/Footer/Rarity）+ 纯函数——`Aggregate`（展开装备全部词缀行=hybrid 两行全进，同 (StatId,ModOp) 求和聚合）、`Compare`（union：candidate−equipped，仅 delta!=0，排序=StatId enum 序再 ModOp 序）、`FormatStat`（按 (StatId,ModOp) 反查 AffixCatalog 既有 Format/Format2 模板，正负号统一前置；防御回退仅备而不用）、`ItemCard`/`SupportCard`/`TextCard`。不绘制、不修改 session、不是第二规则源、不复制装备数值算法。
- **纯几何 `SliceTooltipLayout`**（新文件）：`Place(pointer,w,h,vw,vh)`——默认 pointer 右下（+14/+16）、右溢翻左、下溢上翻、最终钳视口（Margin=8）；BaseW=360（工作令推荐 340–380 带内）。
- **渲染 `SliceHud.DrawTooltip`（单一正式出口）**：SliceSkin.PanelBg 石底金边九宫格 + 稀有度着色标题（Rare 金/普通米白）+ 对比区标题（PanelEdge 金）+ dim footer；每帧至多一卡（RequestTip 确定性优先级：面板 TipPriPanel=3 > 抽屉=2 > 底栏=1 > 顶栏/导航=0；同优先级先到先得=稳定）。不可点击、不进入第二输入态、不新增世界吞区（ShouldBlockWorld 零改动）。
- 复用不动：`DescribeItem`/`AffixLine`/`CleanBaseName`/`RarityWord`/`SlotName`/`SkillHotkey`/`SkillDisplayName` 等数据 helper 原样，被新表现层调用。

## 3. Item tooltip 覆盖（工作令 六）

- Build 面板四张装备卡（已装备本体）、Craft 面板结果区（候选/已装备）、背包行（候选）、右侧抽屉四槽（已装备本体）。卡内容：名称（CleanBaseName）、稀有度·槽位·孔数、词缀行（canonical AffixLine，hybrid 两行全显）。

## 4. Hybrid affix handling（九）

- `Aggregate` 按 `AffixDef.RowCount` 展开（row0=Stat/Op+Value，row1=Stat2/Op2+SecondValue）；现有三枚双行词缀（灼燃/锐击/熔铸）两行全部进模型与比较。测试 `HybridAffix_BothRows_EnterModel` 钉住 IgniteFire 双行进聚合 + 已装备侧 hybrid 第二行损失可见。

## 5. Equipment comparison algorithm（七/八/十）

- 对象：仅当候选对应某 EquipSlot 且该槽已有装备；比较目标=canonical 同槽已装备（`session.Equipped[(int)item.Slot]`）。不与任意 inventory 比、无自动最佳、无 DPS。
- key=(StatId, ModOp)；每件按 Aggregate 聚合后 candidate−equipped。
- 展示：标题「与当前装备相比」，仅 delta!=0 行（十）；带符号文本复用 AffixCatalog 模板（如 `+8% 物理伤害`/`-20 生命`），无 gear score/DPS%/推荐装备（十一）。

## 6. equipped-only loss handling（十三）

- union 合并：equipped 独有 key 以负 delta 进入比较（`Merge(union, stat, op, -value)`）。测试 `UnionComparison_IncludesEquippedOnlyLoss` 钉住 Life 损失行；截图证据 r3_compare 含 `-20 生命`/`-12% 火焰伤害` 损失行。

## 7. Support compatibility canonical proof（十五/十六/十七）

- 全部经 `SliceSession.IsSupportCompatible`（运行时兼容判定单一入口）：逐技能行（✓/× + hotkey + 名）与「可装配到当前技能 / 与当前技能不兼容」footer（SelectedSkill）。零硬编码兼容表（FireConversion 等特例由 Tag/MechanicSkill 路径自然推导）；不复制 TrySetSupport 前置判断（容量/占用不是本轮语义）。
- 测试：`SupportPresentation_MatchesCanonicalMethod`（分裂=×Q/✓W/×E，燃烧=3×✓）+ `FullSkillSupportParity_ThroughPresentation`（全目录 SupportCatalog.Count × 3 技能展示与 canonical 全量 parity；不硬编码 7=二十九达标）。

## 8. 缺失与同装行为（十二）

- 槽空：只显示候选（Footer「点击装备」），无伪造对比。同物品（Id 相等）：Badge「已装备」、无对比区（零噪音）。全零差异（不同物品同数值）：同样无对比区（不刷屏）。测试 `NoEquippedItem_NoComparisonSection` / `SameEquippedItem_Badge_NoZeroDeltaSpam`。

## 9. 排序确定性（十四）

- 候选自身词缀行=定义序（AffixLine）；比较区=StatId enum 序、同 Stat 按 ModOp 序。测试 `Comparison_Ordering_Deterministic`（两次调用逐行相等 + 全序断言）。

## 10. single-tooltip ownership / World input（二十/二十一/二十二/二十三）

- 每帧至多一张（`_tipSet/_tipPri` 状态机，帧首重置）；确定性优先级（同优先级先到先得）——`grep` 全部 RequestTip 调用点=9 处，各自唯一优先级常量；卡片不可交互（纯 Label/Box，无按钮/滚动/pin）；ShouldBlockWorld 未改（tooltip 不新增世界吞区）；click-flash 技术债按令保持未动。

## 11. R1/R2 结构锁定（二十四）

- 球/底栏/QWE/Support 2×4/抽屉几何/Build-Craft 迁移/4 槽 schema 零重排：本轮 Runtime diff 仅=SliceHud（tooltip 管线与接线）+SliceSkin（Ensure 自愈一行，见 §18）+两个新文件；SliceDrawerLayout 零改动。

## 12. 视觉验证（二十五/二十六/二十七）

- 方式：编辑器 Play + `SliceHud.DebugHoverAt`（public 验证钩子：钉 GUI 指针到设计空间点，正常输入零影响）+ `capture_game_view --source screen`；游戏视图经浮动窗口精确设置（DPI 125% 换算：逻辑 1536×885→实测 Screen 1920×1080；2048×1173→实测 2560×1440；1024×597→实测 1280×720，均为逐次 eval 实测值）。
- **1080p（实测 1920×1080，scale=1.0）**：①Item Compare（Craft 结果区悬停碎星重刃）：候选词缀+「与当前装备相比」五行（-20 生命 / +30 命中 / +8% 物理伤害 / -12% 火焰伤害 / +20% 暴击率，损失与增益同屏）+「点击替换同槽装备」；②Support：分裂卡（✓W 弹道 + ×Q 近战/×E 范围 + 「与当前技能不兼容（Q 近战）」）与燃烧卡（3×✓ + 「可装配到当前技能（Q 近战）」）——兼容与不兼容真实案例齐备；③Drawer：抽屉武器槽悬停卡正确翻转到抽屉左侧（熔铸长刀 · 稀有 · 武器 · 3孔 · 已装备 + 词缀两行）。
- **1440p（实测 2560×1440，scale≈1.333）**：双球 + 底栏 + 装备抽屉 + tooltip 同屏，tooltip 正确左翻——顺带关闭 R1/R2 记录的「1440p UI visual review」遗留（性能契约未动）。
- **窄窗口（实测 1280×720，scale≈0.667，16:9）**：tooltip 钳制完整不出屏、header/body 可读、抽屉源 hover 可用。极端低于 0.4 夹取窗口维持 R2 记录（不为它重写 HUD）。
- 截图=Assets/Temp（gitignore，不入库）；六状态截图逐张目检通过后已清理。

## 13. tests（二十八/二十九）

- `SliceTooltipTests` 11 项（EditMode 127→**138**）：HybridAffix_BothRows_EnterModel / Comparison_KeyIsStatIdPlusModOp / UnionComparison_IncludesEquippedOnlyLoss / NoEquippedItem_NoComparisonSection / SameEquippedItem_Badge_NoZeroDeltaSpam / Comparison_Ordering_Deterministic / SupportPresentation_MatchesCanonicalMethod / FullSkillSupportParity_ThroughPresentation / Placement_FlipsLeft_AtRightEdge / Placement_ValidAt1440p_DesignScale / ModelCreation_DoesNotMutateSession。
- 兼容 golden=直接对齐 canonical `IsSupportCompatible`（未造第二套兼容逻辑）；Support 遍历取 `SupportCatalog.Count`（sentinel 派生，非硬编码 7）。

## 14. PlayerRuntime Gate（三十六）

- SelfTest 28/28 PASS → `.\tools\verify_unattended.ps1 -IncludePlayerRun` **PASS exit=0**：EditMode 138/138（failed 0）+ PlayMode 3/3 + ContentAudit fresh + PlayerBuild win64 + PlayerRun exit=0 @2560×1440 密度 100/200/300；PerformanceVerdict=NOT_EVALUATED（本工作令不要求性能层）。

## 15. Content delta / Audio delta（三十/三十一）

- Content delta=0：Skill/Support/Affix/Passive/Enemy/资源契约零改动（tooltip 只读展示已有内容）。
- Audio delta=0：AudioEvents/VoiceCues/音频资源零 diff；无 tooltip 音效；Phase 4 保持 GATED。

## 16. Art asset delta（三十三/三十四）

- Art asset delta=0（理由=工作范围，不因许可证）：无新模型/贴图/图标；Bruce 未导入（下一轮优先候选=Phase 5 Art Trial R1，届时无需授权裁定）。

## 17. Director licensing rule status（三十二）

- 导演裁定（2026-09-08）已按工作令最小条目入 `docs/DECISIONS.md`=**长期规则⑪**：素材授权问题不再是 Gate（内部仓库、素材均已获授权）；执行 AI 后续不得因许可停住素材工作。此前同日已记入 `docs/art/MODEL_ASSET_SCREENING_R1.md`（R2 轮）。规划 AI 风格/顺序裁定（Bruce 优先、POLYGON 限制等）继续有效。

## 18. 本轮附带修复（如实记录）

- `SliceSkin.Ensure` 一行加固：静态缓存贴图被销毁（交互式编辑器无域重载的 teardown 残留）时按 Unity null 语义重建（Gate 批处理新域行为不变；R1 幂等测试不受影响）。起因=本轮交互式编辑器内 run_tests 观测到 6 项 SliceSkin 测试 MissingReferenceException，修复后交互式 138/138 全绿。
- 测试夹具：DebugHoverAt/DebugHoverOff（public 静态验证钩子）——编辑器视觉验证用；默认关闭，正常输入与 Player 行为零影响。

## 19. R4 boundary（三十五）

- R4（天赋树重排）**未自行开始**：Stage 0 长期禁止「大天赋树」；即使方案页写了 R4，也必须由规划 AI 重新区分「现有小型 passive UI 整理」vs「新增/扩大 passive tree」，执行 AI 不自行解释为已解锁。

## 20. Verdict 复述

- **PASS**。Phase 3 保持 IN PROGRESS（R1/R2/R3 COMPLETE，R4 待规划审查）；Phase 4 保持 GATED；Bruce 未在本轮混入。
