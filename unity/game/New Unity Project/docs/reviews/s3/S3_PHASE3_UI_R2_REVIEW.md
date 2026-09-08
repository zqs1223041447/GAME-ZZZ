# S3-P3-UI-R2 EQUIPMENT DRAWER — 复核报告

工作令：S3-P3-UI-R2-EQUIPMENT-DRAWER（Phase 3 第 2 轮：右侧装备抽屉 + Build/Craft 迁移 + 常驻 4 槽）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**PASS**（EditMode 121→127 全绿；抽屉几何/四槽映射/面板迁移/输入阻挡全部达标；旧 UI 零重复=单一 canonical 渲染器+s.Panel 唯一状态；Player Runtime Gate 全 PASS exit=0）。

## 1. Baseline

- Baseline=a90f34a（Phase 3 第 1 轮 STATUS 回填后 HEAD；R1=SliceSkin 程序化换皮+双球+点击效+全局缩放，Player Runtime Gate PASS 121/121）。
- 规划 AI 已验收 R1，随本工作令下达 5 项素材裁定（许可硬规则、Bruce 仅本地试模、POLYGON 不批为最终风格混用、21 组 GUI PNG 暂不入库、critterpack 仅记录）。
- 导演裁定（本轮入库 `docs/art/MODEL_ASSET_SCREENING_R1.md`）：**许可证问题已由导演解决，不再作为 gate**；素材不再因许可阻塞，规划 AI 风格/顺序裁定仍有效。

## 2. 交付结构

- 新文件 `SliceDrawerLayout.cs`：纯几何层（设计空间 1920×1080 基准纯函数，零状态零绘制，供 SliceHud 与 EditMode 几何测试共用）——Column/Slot/Tab/BuildPanel/CraftPanel/ColumnX；常量 ColumnW=300/ColumnH=232/ColumnTop=64/ScreenMargin=12/Slot=138×82/Gap=6/TabH=26/BuildPanel=760×430/CraftPanel=560×340/PanelGap=12。
- `SliceHud.cs`：
  - `DrawDrawer`：常驻抽屉列（右缘留 12px）=标题「装备」+2×2 迷你槽+底部两 tab（「角色」「制作」，toggle 语义）。
  - `DrawMiniSlot`：只读现有 canonical 装备状态（EquipSlot/Inventory），文字+程序化槽位皮肤，无新图标资源。
  - `DrawBuild`/`DrawCraft`：外框矩形改由 SliceDrawerLayout 右锚供给（迁移而非复制）。
  - `ShouldBlockWorld` 改薄封装 → 纯函数 `BlocksWorldInput(..., Rect drawer, ...)`；鼠标坐标沿用 R1 的 `_scale` 换算。
- 数据层零改动：EquipSlot/Inventory/SliceSession/`s.Panel` 路由（Tab/F6/F8/Escape/QWE/拖拽装 Support）原样。

## 3. 四槽映射（canonical，不新增 schema）

- 0=Weapon、1=Body、2=Helmet、3=Boots（EquipSlot 既有顺序；`Count=4` 哨兵不进 UI，测试单独钉住）。
- 槽内：`SliceSession.SlotName(slot)` 标签 + 已装备物按 Rarity 着色 `CleanBaseName`（Clipped+DescribeItem tooltip）/ 未装备显示「空」。
- 交互：点击槽=打开 Build 面板（`s.Panel=Build`）+ClickFlash；不直接穿戴/卸下（装备编辑仍唯一入口=Build 面板，避免双写路径）。

## 4. Build/Craft 迁移（移动而非复制）

- `DrawBuild`：`_panel = SliceDrawerLayout.BuildPanel(Dw())`（760×430，右锚=抽屉列左侧-12px）；`DrawCraft`：`CraftPanel(Dw())`（560×340 同右锚）。
- 单一渲染器：两面板内容函数体零复制，仅外框矩形来源从居中常量改为 SliceDrawerLayout。
- `s.Panel` 仍为唯一 canonical 面板状态；Tab/F6/F8/Escape 与抽屉 tab 操作同一状态机（同面板再点=关闭）。
- 几何断言（测试）：Build/Craft `xMax ≤ column.x`（不覆盖抽屉列）、`yMax ≤ 1080-118`（不压底栏关键区）、尺寸精确 760×430 / 560×340。

## 5. 旧 UI 重复检查

- 面板旧居中矩形已移除：矩形来源唯一（SliceDrawerLayout），无残留居中路径（grep 无第二赋值点）。
- 抽屉 tab 与顶部 Nav 的「角色/制作」共用同一 NavBtn 绘制与同一 `s.Panel` 状态，无第二状态副本。
- `DrawMiniSlot` 不写装备数据（只读+跳转），无并行装备逻辑。

## 6. ShouldBlockWorld 验证

- 纯函数语义：`panelOpen || dragging` → 吞；否则 topBar/nav/skillHud/tray/**drawer** Contains → 吞。
- 测试：抽屉内点=true、战场中心=false、`panelOpen=true`（吞）全绿。
- 抽屉列与 Build/Craft 右锚面板互不重叠（面板 xMax ≤ column.x），吞区不含面板区由 `panelOpen` 兜底（任一面板开=全吞，语义与 R1 一致）。

## 7. 视觉验证（编辑器 Play 截图，Assets/Temp=gitignore，不入库）

- 紧凑窗口 880×377（DesignScale 触底夹取 0.4）：抽屉列右缘正确（「装备」+4 空槽+2 tab）；Build 打开=右锚于列左、无重叠；Craft 打开=同右锚、无重叠。三张截图逐张目检通过。
- 1080p/1440p：几何由 EditMode 测试断言（1080p 布局入界；`DesignScale` 1080p=1.0、1440p=2560/1920≈1.333、夹取下限 0.4/上限 1.4）。本轮未在 1440p 真机截图（PlayerRun 仅产出密度证据，工作令未要求性能/分辨率截图）。

## 8. 测试与 Gate

- EditMode 121→**127**（+6）：`Column_FitsDesignSpace_At1080p` / `FourSlots_NonOverlapping_AndInsideColumnHeader` / `BuildAndCraftPanels_LeftOfColumn_AndNotCoveringBottomBar` / `DesignScale_At1080p_And1440p` / `BlocksWorldInput_DrawerInsideTrue_OutsideFalse` / `EquipSlot_CanonicalFourSlots_Unchanged`（含 EquipSlot 哨兵钉住：`(int)Count==4`、命名成员 0..3）。
- Player Runtime Gate（`-IncludePlayerRun`）**PASS exit=0**：EditMode 127/127（failed 0）+ PlayMode 3/3 + ContentAudit PASS fresh + PlayerBuild PASS win64（GAME-ZZZ.exe 667136 bytes，Data 158 files）+ PlayerRun PASS exit=0 @2560×1440 密度 100/200/300；PerformanceVerdict=NOT_EVALUATED（本工作令不要求性能层）。
- 前置 SelfTest 28/28 PASS。Gate 运行后 `git status` 树态与进门前一致（无 ProjectSettings/契约污染）。

## 9. Delta 清单

- 新增：`SliceDrawerLayout.cs`(+meta)、`SliceDrawerTests.cs`(+meta)。
- 修改：`SliceHud.cs`（抽屉+迁移+BlocksWorldInput+DesignScale public 化）、`docs/art/MODEL_ASSET_SCREENING_R1.md`（导演许可证裁定记录）。
- 内容 delta=0：无外部资源、无 Resources 契约变更、无场景/预制改动；数据/输入层零改动。

## 10. 结论与下一步

- Verdict=**PASS**。
- 按工作令：R3（Tooltip 词缀对比）**不自行开始**，等规划 AI 安排。
