# 导演实测四项（2026-09-12）

导演原话：装备/天赋树多行描述被挡；天赋树缩小后点击与绘制错位；辅助宝石与背包分区；开始制作主城地图、功能型 NPC、游戏内地图。

ProdSim 未动。Canonical hash 仍为 `FNV1A64:99f1bfd3f81c4fe6`（V3 `pv|passive-v2`）。无新 StatId / 无新 parser 族 / 无 WO-05 LOD 改写。第 4 项是第一增量，不是战役。

## 1. 多行描述被裁

根因：Tooltip 正文按 15px 单行 `Clip` 绘制，`CardHeight` 也不含换行。

修法：`SliceTooltipLayout.BodyLineHeight` = `SliceHud.EstimateWrappedHeight`；`CardHeight` 按每行换行高累加；`DescriptionStyle` = wordWrap + `TextClipping.Overflow`。`DrawTooltip` / `DrawItemBody` 用该样式与行高。专精选择器既有 Overflow+wrap-height 未改。

测试：`DirectorPlaytestTests` + `SliceTooltipTests.DescriptionStyle_DoesNotClipWrappedGlyphs`。

## 2. 天赋树点击与绘制错位

根因：`DrawPoeTree` 在 `GUI.BeginGroup(view)` 内绘制。IMGUI 已把鼠标换成组内坐标，`TreePointer` 再减一次视口原点（标题条 46px）。默认缩放 0.45 时节点命中半径小于该偏移，点不中；放大到最大后大节点半径盖过偏移，看起来「必须放到最大才能点」。

修法：组内 `TreePointer` 直接用 `Pointer`。`TreeClickProbeFromDesign` 只用于设计空间指针（DebugHover）。`TreeHitFromDesignPointer` / `TreeDrawnCentreDesign` 与绘制同一换算。

测试：`DefaultTreeZoom` 0.45、中档、`MaxZoom` 三档，绘制中心经 HUD 探针必须命中同一 NodeId。

## 3. 辅助宝石与背包同区

根因：装备六槽 / 辅助宝石托盘 / 背包网格三区堆叠。导演要求去掉托盘分区。

修法：删除 `TrayLabel` / `TrayArea` / `DrawSupportTray`。`SharedBagCellCount` = 宝石数 + `InventoryCap`；前缀格是宝石（点选后仍走既有孔位安装），其后是物品。`SelectedInv` / `TryEquip` 用物品下标，不是格子下标。纸娃娃六槽与 Q/W/E 安装规则未改。

测试：`SliceBagPanelTests` 不再冻结三区不重叠；`DirectorPlaytestTests.SharedBag_GemsOccupyInventoryCells_NoTrayBand`。

## 4. 主城第一增量

`TownHub`（`ember-town` / 烬城）：`MapState.Town` 时画枢纽广场 + 3 个功能型 NPC。

| NPC | 绑定 |
|---|---|
| 仓库管事 | `BagOpen=true` |
| 工匠 | `Panel=Craft` |
| 地图官 | `Panel=Map`（灰烬庭院） |

进图唯一走 `TownHub.TryEnterMapFromHub` → `SliceSession.TryEnterMap`（既有灰烬庭院，含词缀/稳定度）。不改 `SpawnMap`、不刷木桩、不加 Atlas / 多城 / 对话树。

测试：`DirectorPlaytestTests.TownHub_IdentityAndNpcBinds_EnterMapViaTryEnterMap`。

## Gate

- EditMode 全量 **540/540**。导演相关类两遍全绿。
- ProdSim `FNV1A64:99f1bfd3f81c4fe6` invalid=0。
- PlayMode：首轮 Pipeline 7801 断连未记 PASS；恢复后 **23/23** fail=0 skip=0。
- Channel A（`game-zzz-planning-2`）：**ACCEPT WITH FOLLOW-UP**；后继 **NONE / STOP**。Follow-up=补 PlayMode（已补）。`TownHub.TryEnterMapFromHub` 允许 `MapState.Dead` 走既有 `TryEnterMap` 为非阻塞卫生项，本轮不改。
