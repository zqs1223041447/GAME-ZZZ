# S5U_PLAN — Interface Presentation & HUD Renewal（导演插入式呈现周期）

**授权源**：导演 2026-09-09 Arena 实游反馈（原话）：「Bootstrap开启后啥也没有啊，反而arena开启后有页面。另外现在的UI和HUD真的让人看不下去，让规划AI重新规划，插入新一轮的页面内容优化。我要求：参考POE或者暗黑3来制作HUD和UI，如果本机有可用素材也可以使用！ 无人值守模式启动」
**规划裁定**：规划 AI 会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`（2026-09-09 完整重发版，全文由工作 AI 存档于执行会话）。本文件为该计划的仓库化权威版；与规划 AI 原文冲突时以规划 AI 会话原文为准。
**Entry HEAD**：b16d174（S5 收口末位）
**类型**：Director-directed interstitial presentation cycle（**UI presentation only；Gameplay Scope = NONE**）

## §1 S5 处置裁定（规划 AI）

| 项 | 裁定 |
|---|---|
| S5-WO-06 | ACCEPTED |
| S5 Production Closure | PASSED / FROZEN |
| S5 Director Final Gate | **PENDING — DEFERRED BY DIRECTOR UI INTERVENTION** |
| S5 正式状态 | **NOT COMPLETE** |
| S5 Scope | **NOT REOPENED**（导演没有否定 BL-002.A1/BL-021.A2/回归/确定性/性能/收口；也尚未填写 APPROVE——不得推定批准） |
| 新 UI 工作 | **独立插入周期 S5U** |
| 收尾方式 | S5U 完成后：原 S5 Final Gate Packet + S5U presentation addendum 一并重送 Director Final Review |
| **禁止书写（S5U 当时）** | `S5 = COMPLETE`；把 S5U 反写成「S5 scope expanded」 |
| **解除（2026-09-12）** | 导演勾选 APPROVE S5 FINAL GATE 后，`S5 = COMPLETE` 已由 `S5_DIRECTOR_FINAL_GATE.md` 正式书写。S5U 仍不得反写成「S5 scope expanded」。 |

## §2 技术路线（Route A）

**保持 IMGUI，全面视觉重做**。UI Toolkit 迁移=NOT AUTHORIZED；uGUI 迁移=NOT AUTHORIZED；工作 AI 不得自行把 B/C 当 fallback。（完整理由与技术锁见 `S5U_VISUAL_CONTRACT.md` §1。）

## §3 视觉参考原则与自有方向

PoE × Diablo III = REFERENCE ONLY（信息架构/层级/silhouette/密度/交互原则；禁止裁切/rip/描摹）。自有方向=Dark ARPG（黑石/暗氧化金属/做旧铜/哑金/深血红/暗奥蓝/克制 rune 纹饰）；禁止手游亮色/卡通圆角/Material Design/工具灰框/Excel 页面/高饱和霓虹/等权重平铺。详见 `S5U_VISUAL_CONTRACT.md` §3。

## §4 阶段计划

| Phase | 内容 | 运行时授权 |
|---|---|---|
| **0** | **WO-01 Visual Contract & Asset Admission Lock**：表层清单/S5 合同映射/技术锁/7 线框/design tokens/资产矩阵/素材 license 审计/截图基线/回归合同 | **NONE**（仅文档/审计/截图） |
| 1 | Skin Foundation：textured 9-slice 面板框/纹饰角/资源球族/槽框/按钮页签/hover-selected-disabled 态/asset cache/style cache/typography 层级（不重排页面） | UI runtime（首张 UI runtime authority 工作令） |
| 2 | Combat HUD Renewal：Life globe/Mana globe/中央 QWE/Support 视觉/Link 组与源视觉/7 辅助 tray/消息状态/紧凑导航/Tooltip 连接——**导演第一张可验收 vertical slice**（Arena 第一眼从 debug HUD 变正式 ARPG HUD） | UI runtime |
| 3 | Equipment/Inventory/Build/Craft：装备抽屉/六槽/背包网格呈现/物品卡/Build 页/第二连接呈现/制作工作流/选择与 hover 态 | UI runtime |
| 4 | Passive/Map/Tooltip/Polish：统一四页+typography/空态/导航/交互反馈/视觉一致性 | UI runtime |
| 5 | Production Closure：导演截图包/EditMode/PlayMode/Content Audit/S5 ProdSim 回归/canonical 性能/Art 性能/视觉分辨率/layout 分辨率检查/Drift/Forbidden Expansion → **STOP，返回 S5+S5U Director Final Review；不自动启动 S6** | 全门 |

## §5 战斗 HUD 目标结构（权威 silhouette）

Life（左下主锚）— utility — [Q][W][E] 中央动作条（图标优先）— utility — Mana（右下主锚）；Support/Link 降为紧凑 pip/badge + hover tooltip（长文本如 `武器·组1·容1` 退出战斗 HUD 常驻，完整文本真值由 hover/tooltip 提供）。宽屏时 combat-critical HUD 保持相对靠近中央视觉区（PoE Centred UI 原则）。详见 `S5U_VISUAL_CONTRACT.md` WF-1。

## §6 S5 不可破坏的 UI 功能合同（冻结）

Secondary Link configuration / LinkSkill1 None-active 区分 / Group0-Group1 truth / 唯一有效源 / 真实容量 / 3S=0+1 / 候选过滤 / 拒绝原因 / Support 兼容性 / Affix tooltip truth / Q·W·E / Tab / F6 / F8 / Esc / R。**视觉层级可以彻底改变，语义不能改变。**（逐表层映射=`S5U_UI_SURFACE_MAP.md` §2。）

## §7 素材策略（Tier A-D）

Tier A 本机 HD Common Icon Pack 1.2=**QUARANTINED**（license 审计见 `S5U_ASSET_ADMISSION.md` §2）；其它本机 kit=REFERENCE ONLY；Tier B=Kenney CC0 fallback；Tier C=**原创美术=首选最终路线**；Tier D=Asset Store 付费候选=**DIRECTOR INPUT REQUIRED**。无人值守原则：采购未获批不阻塞（原创+CC0 继续）。详见 `S5U_ASSET_ADMISSION.md` §4。

## §8 性能/回归合同

Entry 基线：EditMode=314/314、PlayMode=11/11、Affix=21、Max Link Groups=2、S5 ProdSim=**FNV1A64:9a4c9524d0b3e214**。
S5U 为 UI-only presentation cycle → **Expected S5 ProdSim Hash = UNCHANGED**。
最终要求：predecessor 测试不删除/不削弱；UI 测试可增；gameplay/runtime state 无变化；canonical performance PASS；Art performance PASS（locked hardware/threshold）；Content Audit PASS；ProdSim exact same hash；Drift=0。IMGUI 性能规则见 `S5U_VISUAL_CONTRACT.md` §1。

## §9 Forbidden Expansion（S5U 不授权）

UI Toolkit 迁移 / uGUI 迁移 / inventory Tetris / item footprint / 新 EquipSlot（Ring/Amulet/Offhand/Unique）/ 新 Skill / 新 Support / 新 Affix / Socket Color / Gem Level / Gem Quality / 第三连接组 / 新 Craft 机制 / 新 Craft currency / Progression / Map Tier / Atlas / Boss / Endgame / Persistence / Save system / Controller redesign / input rebinding system / combat math / balance changes / 新 VFX batch / 角色替换 / 敌人视觉替换 / Voice activation / BL-024 / Content Factory。
**「让页面更像 PoE/D3」不能成为实现上述任何系统的理由。**

## §10 治理状态表达（本周期内）

- STATUS（S5U 当时）：S5 Production Closure=PASS；S5 Director Final Gate=PENDING/DEFERRED；**S5≠COMPLETE**；S5U=ACTIVE/DIRECTOR AUTHORIZED；S5U Phase 0=executing。
- STATUS（2026-09-12 起）：S5 Director Final Gate=APPROVED；**S5=COMPLETE**；S5U 为历史插入周期，不倒算进 S5 scope。
- ROADMAP：插入 S5U；不写 S6 active；不把 UI 工作并入 BL-002.A1/BL-021.A2。
- Capability Ledger：UI Presentation quality 建立独立 evidence/tracking；不因视觉重做晋升 gameplay capability。
- Mechanic Matrix / RUNTIME.md / COMBAT_MATH.md：VERIFY ONLY。

## §11 工作令序列

| WO | 名称 | 状态 |
|---|---|---|
| S5U-WO-01 | Visual Contract & Asset Admission Lock | **RELEASED / 本文件随附执行** |
| S5U-WO-02 | Dark ARPG Skin Foundation & Combat HUD Frame（首张 UI runtime authority；预计只做：暗黑皮肤+panel 9-slice+资源球地基+中央 QWE 框+技能槽视觉层级+Support/link 紧凑语言+缓存资产/样式+有界 SliceHud 视觉 helper 抽取；**不同时改** Equipment/Inventory/Craft/Passive/Map） | 待 WO-01 ACCEPT 后由规划 AI 放行 |
| S5U-WO-03..05 | 按 Phase 3/4/5 依次规划 | 未放行 |
