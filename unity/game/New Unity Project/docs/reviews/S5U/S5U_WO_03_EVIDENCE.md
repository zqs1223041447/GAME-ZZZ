# S5U_WO_03_EVIDENCE — Equipment & Inventory Presentation Renewal（证据包）

**周期**：S5U — Interface Presentation & HUD Renewal
**工作令**：S5U-WO-03 — Equipment & Inventory Presentation Renewal（规划 AI 2026-09-09 放行，紧随 WO-02=ACCEPT）
**执行日期**：2026-09-09（无人值守模式）
**授权边界**：presentation-only（IMGUI；gameplay/inventory-system/craft-system/bootstrap authority=NONE）
**HEAD 状态**：门与截图全部采集于最终代码态（cell 布局打磨+网格悬停坐标修复+重编译 clean 后）。

## §1 交付摘要

| 项 | 交付 |
|---|---|
| 抽屉壳（S-08） | `SliceDrawerLayout` 新增 Shell 系纯函数（Shell/Header/ShellTab/ShellSlot/ShellInvView/ShellInvCell/ShellInvContentHeight/ShellFooter）；壳 312 宽全高（底缘 dh-160，恒在战斗栏顶上方 8px）；头部标题+饰线/Build-Craft tab（语义不变）/装备区+背包区/底部反馈条 |
| 装备 2×3（S-09） | 146×86 槽=6 件原创装备槽符文（EqWeapon/Helmet/Body/Gloves/Boots/Belt，generic 类型+恒等映射 EquipGlyph）+槽名+物品名（稀有度真值着色）；空槽=暗符文+「空」；hover=ItemCard；点击=开角色页（既有语义） |
| 背包呈现网格（S-10/S-11） | 92×70×3 列滚动网格：1 物品=1 恰一格、顺序=InventoryCount 真值、零网格机制（无宽高/占位/旋转/容量系统）；格=槽位符文+稀有度左缘+名称+词缀摘要+已装备徽记 |
| ItemCard 层级（S-12/S-16） | 渲染层金分隔线×2（标题块|正文|对比区）；模型（SliceTooltipModel）零改动；tooltip 与战斗 HUD 同族；cell 悬停触发（本 Wo 修复坐标换算，见 §5） |
| 状态语言（S-13） | Normal/Hover/Selected 四态石质框沿用+已装备徽记+空槽暗符文；selected > hover 优先 |
| 装备/拖放合同（S-14/S-15） | 网格点击=既有 `SelectedInv=i + TryEquip(i)` 逐字保留；第二连接条带保留（资格/候选/写入/拒绝四口径同源）；零直接 LinkSkill1 写入 |
| Compact 升位修正（规划 AI 合同修正） | `SliceHudFormat.Compact`：K/M/T 段舍入达上段阈值=升位（999999→**1.0M**；禁 1000.0K/1000.0M）；测试同步+全窗口扫描测试 |
| Build 页调整 | 背包列表自 Build 面板移除（单一背包表面=常驻抽屉壳）；天赋树放宽至面板整幅（SlicePassiveLayout 按矩形布局，语义零改动）；装备明细行符文化保留 |

## §2 截图证据（7 张 canonical After，全部采集于最终 HEAD）

| # | 文件 | 状态 | 验证要点 |
|---|---|---|---|
| 1 | `after_wo3_combat_drawer_2560x1440.png` | 战斗+常驻抽屉同屏 | 壳（头部/tab/六槽符文/网格/反馈条）与战斗底栏并存不重叠 |
| 2 | `after_wo3_grid_selected_2560x1440.png` | SelectedInv=烬星长杖 | 选中金框独立可辨（selected 态） |
| 3 | `after_wo3_grid_hover_itemcard_2560x1440.png` | 悬停烬星长杖 | hover 态+ItemCard 完整（稀有·武器·4孔/+词缀×4/组0·组1 连接真值/对比区/点击替换提示）；右缘左翻不越屏 |
| 4 | `after_wo3_minislot_tooltip_2560x1440.png` | 悬停装备武器槽 | 已装备卡（Badge=已装备+分隔线+双组连接真值） |
| 5 | `after_wo3_build_page_2560x1440.png` | 角色页 | 6 明细卡（符文+稀有度+孔数+第一连接行）+第二连接条带（裂石巨刃=弹道/范围/无）+天赋树整幅 |
| 6 | `after_wo3_gearcard_hover.png→after_wo3_gearcard_hover_2560x1440.png` | 悬停武器明细卡 | ItemCard 完整真值+对比零干扰 |
| 7 | `after_wo3_1920x1080.png` | 1080 兼容 | 壳+网格 1:1 逐像素等价，无裁剪/无重叠 |

## §3 验收断言（规划 AI 27 项 AC）

| AC | 结果 | 证据 |
|---|---|---|
| AC-01 呈现 only | PASS | §4 Delta；ProdSim hash 不变 |
| AC-02 Gameplay/Content/Balance=NONE | PASS | 同上 |
| AC-03 恰 6 EquipSlots | PASS | S5UDrawerTests.Shell_ExactlySixSlots（DisplayOrder 断言=人体顺序+ID 不漂移） |
| AC-04 列表→呈现网格（无网格机制） | PASS | InventoryGrid_PresentationOnly_UniformCells（内容高=ceil 行高） |
| AC-05 库存顺序/存储真值不变 | PASS | 网格顺序=InventoryCount 真值；零写入路径 |
| AC-06 装备/替换/拖动走既有 API | PASS | 点击流逐字保留；SixSlot PlayMode 绿 |
| AC-07 零直接 LinkSkill1 UI 写入 | PASS | grep 无新写入；条带唯一走 TryReassignLink |
| AC-08 Secondary Link UI 可用 | PASS | 截图 #5（弹道/范围/无 候选+当前态） |
| AC-09 容量/候选/拒绝语义精确 | PASS | MultiLink/SupportGate 套件绿 |
| AC-10 ItemCard 层级清晰 | PASS | 截图 #3/#4（分隔线层级） |
| AC-11 Item tooltip 完整真值 | PASS | 截图 #3（词缀+双组+对比+操作） |
| AC-12 hover/select/equipped 视觉可辨 | PASS | 截图 #2（selected）/ #3（hover+徽记）/#4（已装备徽记） |
| AC-13 绘制=命中几何同一 layout authority | PASS | Shell 系纯函数；DrawInventoryGrid 用 ShellInvCell+ScrollView（GUI 输入由 ScrollView 变换） |
| AC-14 2560 无裁剪/无重叠 | PASS | 截图 #1-6 + 三空间几何测试 |
| AC-15 1920×1080 兼容 | PASS | 截图 #7 |
| AC-16 Combat HUD 完好 | PASS | 截图 #1（战斗底栏未动）；WO-02 套件全绿 |
| AC-17 Compact 无 1000.0K 伪影 | PASS | Compact_NeverEmitsThousandBoundaryArtifact（999.9K..1e6 与 999990M..1e12 全窗口扫描）+ 999999→1.0M |
| AC-18 零每帧纹理/图标生成 | PASS | 图标=Ensure 静态缓存（幂等测试）；零每帧 GUIStyle 重建（沿用 WO-02 缓存样式）；绘制循环无 LINQ/临时集合（网格循环仅局部 struct+既有字符串） |
| AC-19 EditMode 全 PASS（前置 332 保留） | PASS | **344/344**（332+12 新增：S5UDrawerTests 9 + formatter 修正 3；零删除零削弱） |
| AC-20 PlayMode 全 PASS | PASS | **11/11**（最终 HEAD 重跑） |
| AC-21 Content Audit PASS/fresh | PASS | CONTENT_PRODUCTION_REPORT 再持久化 verdict=PASS |
| AC-22 ProdSim hash 精确 | PASS | **FNV1A64:9a4c9524d0b3e214**（repeatHashMatch=True、invalidCount=0、21:47 再生） |
| AC-23 Affix=21 | PASS | Audit counts（零内容改动） |
| AC-24 Max link groups=2 | PASS | 域核零改动 |
| AC-25 Drift=0 | PASS | — |
| AC-26 SoT 冲突=0 | PASS | — |
| AC-27 Forbidden Expansion=PASS | PASS | 未触 Bootstrap/Craft/Passive/Map 重设计；未触 Socket Color/Gem 等级品质/第三组/新槽位 |

## §4 Delta 声明

- **Runtime Delta（呈现层，授权内）**：SliceDrawerLayout（+Shell 系纯函数，旧 Column 系原样保留供既有测试）；SliceHudIcons（+6 装备符文）；SliceHudFormat（升位修正）；SliceHud（DrawDrawer/DrawMiniSlot 重做、DrawInventoryGrid 新增、DrawInventory 列表移除、DrawBuild 调整、DrawSlotCard 符文化、DrawTooltip 分隔线）。
- **Gameplay/Content/Balance Delta=NONE**（ProdSim hash 精确不变实证）；SliceSession 零改动。
- **Drift=0；SoT 冲突=0；Forbidden=PASS。**

## §5 环境与过程披露（如实）

1. **网格悬停坐标缺陷（自查修复）**：网格 cell 的 hover/tooltip 检测原用内容坐标与设计空间 Pointer 比较——**旧背包列表实现同样存在此缺陷且从未被悬停验证暴露**；本 Wo 改为显式换算（view+cell−scroll）后 cell 悬停→ItemCard 可靠触发；GUI.Button 输入由 ScrollView 自行变换，点击流不受影响（截图 #3 为修复后重采）。
2. **Build 页背包列表移除决定**：背包表面唯一化（常驻抽屉壳网格），Build 页=装备明细+天赋树（放宽整幅）；装备/替换/选择功能全部经抽屉网格承载（同一 TryEquip 路径），无功能净损失；如规划 AI 认为需要恢复 Build 页内嵌背包，可回退该小项（呈现层独立）。
3. **编辑器外部关闭事件**：执行中途编辑器被有序关闭（Editor.log 显示正常退出序列，非崩溃；疑似外部/人工操作）；重启后重编译+全门重跑通过，仓库无损。
4. **批处理 canonical 门 tier**：仍留 S5U 收口全量重跑（编辑器占用，WO-01 先例延续）。
5. **截图时序**：7 张全部在最终 HEAD 重采（cell 打磨+悬停修复之后）。

## §6 交付物清单（本提交）

- 代码：`SliceDrawerLayout.cs`、`SliceHudIcons.cs`、`SliceHudFormat.cs`、`SliceHud.cs`、`S5UDrawerTests.cs`(+meta 新增)
- 凭证：`docs/reviews/S5U/asset-source/equip_*_glyph.png`（6 PNG）
- 截图：`docs/reviews/S5U/screenshots/after_wo3_*`（7 张）
- 文档：`S5U_VISUAL_CONTRACT.md`（§7）、`S5U_ASSET_ADMISSION.md`（台账 15 项）、`S5U_UI_SURFACE_MAP.md`（§5）、本文、STATUS/ROADMAP/DECISIONS 同步

## §7 建议下一令（按规划 AI 预授权）

- **S5U-WO-04 — Bootstrap Entry Affordance**（方案 B 已原则授权：Bootstrap 校验有效态后显式「进入竞技场」按钮；禁自动进场/禁复用 -arenaPerf 流程；无可复用普通进图路径则 STOP 上报）。WO-03 Gate 通过后执行。
- 其后：Build/Craft/Passive/Map/Tooltip 后续呈现轮（规划 AI 序列裁定）。
