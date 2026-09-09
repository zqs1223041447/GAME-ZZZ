# S5U_WO_01_EVIDENCE — Visual Contract & Asset Admission Lock（证据包）

**Work Order**：S5U-WO-01 — Visual Contract & Asset Admission Lock（S5U Phase 0）
**执行日期**：2026-09-09
**Runtime Product Authority**：NONE（AC-01：Changed Runtime Product Files = NONE）
**Gameplay Authority**：NONE（AC-02：Gameplay Delta = NONE）
**UI Runtime Modification**：NONE
**Asset Audit Authority**：YES

## §1 截图基线（A — Current Screenshot Baseline）

**来源**：`Assets/Scenes/Arena.unity` 真实运行路径（编辑器 Play 模式 + 确定性状态注入 via unity-cli eval_file；**零运行时代码改动**，全部状态经 canonical 公共 API：ResetTown/RollItem/AddItem/TryEquip/TrySetSupport/TryReassignLink/TryEnterMap/ExitMap/Panel/SliceHud.DebugHoverAt）。
**捕获**：`unity command capture_game_view --source screen`（合成后备缓冲，含 IMGUI HUD）。
**目录**：`docs/reviews/S5U/screenshots/`。

| # | 文件 | 状态 | 分辨率 |
|---|---|---|---|
| 01 | `01_combat_clean_1440.png` | 进图战斗·无面板·无 hover（六槽已装备、辅助空、连接=默认映射槽） | 2560×1440 |
| 02 | `02_combat_supports_links_1440.png` | 进图战斗·辅助×3（点燃/分裂/燃烧）+ **武器第二连接改挂生效**（`srcW=裂石巨刃·组1·容1`） | 2560×1440 |
| 03 | `03_equipment_drawer_hover_1440.png` | 装备抽屉列+武器迷你槽 hover=ItemCard tooltip（TipPriDrawer） | 2560×1440 |
| 04 | `04_inventory_build_1440.png` | Build 面板全开（六槽卡+第二连接条+背包列表+天赋树） | 2560×1440 |
| 05 | `05_craft_1440.png` | Craft 面板（选中烬星长杖+随机制作/定向制作+21 词缀网格） | 2560×1440 |
| 06 | `06_passive_hover_1440.png` | 天赋树 hover 影袭节点（TextCard tooltip） | 2560×1440 |
| 07 | `07_map_town_1440.png` | 城镇+地图面板（3 词缀勾选+稳定/收益+进入地图 CTA） | 2560×1440 |
| 08 | `08_tooltip_item_1440.png` | Build 面板武器卡 hover=完整 ItemCard（词缀+组0/组1 连接真值+第二连接行） | 2560×1440 |
| sanity-1 | `02_combat_supports_links_1080.png` | 状态02 的 1920×1080 layout sanity（16:9 缓冲等比，布局语义同源） | 1920×1080 |
| sanity-2 | `04_inventory_build_1080.png` | 状态04 的 1920×1080 layout sanity | 1920×1080 |

**AC-20 = PASS（2560×1440 before 基线 8 张齐）；AC-21 = PASS（1080 sanity 2 张齐）。**

### 运行时几何事实（本次基线的测量记录）

| 项 | 值 | 证据 |
|---|---|---|
| GameView 后备缓冲 | **3840×2160（精确 16:9）**，捕获下采样至 2560×1440（无纵横失真） | capture 结果 + 像素扫描（面板/抽屉边框位置与设计换算一致） |
| `SliceHud.DesignScale` | **=1.4（上夹取生效）** → 有效设计空间 **2742.9×1542.9** | 反射探针：`Screen=3840x2160 scale=0.822→(改尺寸后)…`；`DesignScale=Mathf.Clamp(Min(w/1920,h/1080),0.4,1.4)` |
| 布局函数行为 | 右锚（导航/抽屉/面板右缘=Dw-偏移）与居中（底栏）在 dw=2743 下全部按源码语义成立；UI 元素无一缺失/错位 | 反射字段（_topBar/_nav/_skillHud/_tray/_drawer）与 theory 全等 |
| canonical 2560×1440 无夹取态 | scale=1.333、设计空间=1920×1080——**S5U Final Gate 复验标准态**（编辑器面板尺寸≠该态；正式 canonical 复验需 2560×1440 游戏视图/Player，Phase 5 执行） | 本记录 |

**执行注记（对后续工作令的基建沉淀）**：
1. `SliceHud.DebugHoverAt/DebugHoverOff`（编辑器视觉验证辅助，S5 已入产品）= hover/tooltip 截图的确定性通道；hover 点必须按**运行时 dw**（`Screen.width/DesignScale(...)`）计算，不得硬编码 1920 空间坐标（本会话实测教训：dw≠1920 时钉点落空，`tipSet=False`）。
2. 截图状态注入脚本存档于执行会话（eval_file 模式，`%TEMP%\game-zzz-evals\`）；可复用为 S5U 各 Phase 的截图协议。
3. 「游离斜线」鉴定=木桩近战挥砍世界特效（Feedback MeleeSwing），非 UI 缺陷（04/06/08 同位可复现，02 全开态同样可见）。

## §2 表层清单与合同映射（B+C）

- **Surface Inventory**：23 表层（S-01..S-23），每项 7 列（Current/Behavior/Must Preserve/Visual Problem/Target/Owner/Tests）→ `S5U_UI_SURFACE_MAP.md` §1。**AC-03 = PASS。**
- **S5 Contract Map**：14 项冻结合同（LinkSkill1 双态/唯一写入口/组0组1/唯一有效源/真实容量/3S=0+1/候选过滤/拒绝原因/兼容性/隔离/词缀 truth/QWE/键位/六槽）× 承载表层映射，无一项被视觉重做计划移除 → `S5U_UI_SURFACE_MAP.md` §2。**AC-04 = PASS。**

## §3 技术锁（D）

- Selected=**IMGUI**（Route A）；UI Toolkit=NOT AUTHORIZED；uGUI=NOT AUTHORIZED；工作 AI 不得自行换路线 → `S5U_VISUAL_CONTRACT.md` §1。**AC-05/06/07 = PASS。**

## §4 线框（E — 7/7）

WF-1 Combat HUD / WF-2 Equipment+Inventory / WF-3 Build 层级 / WF-4 Craft 五区 / WF-5 Passive / WF-6 Map / WF-7 Tooltip 层级 → `S5U_VISUAL_CONTRACT.md` §2。**AC-08..14 = PASS（全部锁定，未实现）。**

## §5 Design Tokens（F）

色彩 17 tokens（Background..Error+稀有度锚）/ Slot 状态 4 / Spacing 4（4/8/12/24）/ Frame 3（1/2/4px）/ Typography 4（15B/13/11/11B）→ `S5U_VISUAL_CONTRACT.md` §4。WO-01 锁家族与语义，RGB 终值 Phase 1 随资产锁定；此后禁止 uncontrolled magic styles。**AC-15 = PASS。**

## §6 资产矩阵与素材审计（G+H+I+J+K）

- **Asset Requirement Matrix**：17 槽位（panel 9-slice..数值字体规则），每槽 Needed/Candidate/Fallback/License/Phase → `S5U_ASSET_ADMISSION.md` §1。**AC-16 = PASS。**
- **HD Common Icon Pack 1.2**：Archive=1 包；PNG=23 张（128px 级）；Alpha=有；风格=通用写实物料图标（非暗黑 ARPG、分辨率不足）；License=包内免责声明（人人素材分发，「仅供学习研究之用，不得用于商业用途，请24小时内删除」）；Production Admission=**QUARANTINED**；Approved Uses=无；Rejected Uses=全部生产用途 → `S5U_ASSET_ADMISSION.md` §2。**AC-17 = PASS（license/provenance 状态全登记）；AC-18 = PASS（未知/受限 license 未准入）。**
- **其它本机 kit**（ORK Okashi RPG Kit 1.2.5 / Action Game Starter Kit / Dungeon Breaker Starter Kit）：license 未定位 → 全部 **REFERENCE ONLY**；Production use=NOT AUTHORIZED；禁止整包源码合入 runtime → §3。
- **外部策略**：Primary=原创；Fallback=Kenney CC0；Optional=Asset Store 候选清单；付费采购=**DIRECTOR INPUT REQUIRED**（本 WO 无采购动作）→ §4。
- **PoE/D3 边界**：REFERENCE ONLY；截图裁切/素材抽取/logo/icon 复制/描框/近像素重建/专有字体提取=全部 FORBIDDEN → §5。**AC-19 = PASS。**

## §7 门与回归（AC-22..27）

| 门 | 结果 | 证据 |
|---|---|---|
| EditMode | **PASS 314/314**（入口基线 314；delta=0；含 ProductionSimulatorTests） | 活动编辑器 run_tests（async），duration 1.59s，failed=0 |
| PlayMode | **PASS 11/11** | 同上，duration 3.89s，failed=0 |
| Content Audit | **PASS / fresh=YES**（`CONTENT_AUDIT_S3_CLOSEOUT.md` 于本轮 EditMode 运行再持久化，mtime=2026-09-09 18:37:57） | 文件时间戳 |
| Quick Gate | **PASS**（EditMode+PlayMode+Audit 三件套全过；canonical 批处理门因编辑器占用未跑批处理 tier——编辑器内等效执行同一测试集） | 本节 |
| S5 ProdSim Hash | **FNV1A64:9a4c9524d0b3e214**（fresh，`docs/qa/PRODUCTION_SIMULATION_REPORT.json` 本轮再生：verdict=PASS，invalid=0，repeatHashMatch=true，repeatHash 同值） | qa 报告 JSON |
| S5 Expected Hash | FNV1A64:9a4c9524d0b3e214（S5 canonical，`docs/reviews/s5/final-gate/` 冻结） | S5 冻结证据 |
| **Hash Unchanged** | **YES（exact match）** | 上述两行 |
| Canonical/Art Performance | 本 WO 未重跑（Runtime Product Delta=NONE；S5 final-gate 冻结证据继续有效；Phase 5 Production Closure 全量重跑） | S5 冻结证据 |

## §8 Delta 与漂移声明

| 项 | 值 |
|---|---|
| Runtime Product Delta | **NONE**（git diff：零 `.cs` 运行时/测试/catalog 改动） |
| Gameplay Delta | **NONE** |
| Canonical Data Delta | **NONE** |
| Content Delta | **NONE** |
| UI Technology Delta | **NONE** |
| Asset Production Delta | **NONE**（零 asset import；截图 PNG 为证据产物） |
| Authorization Delta | Director 授权的 S5U 规划/呈现范围（Phase 0 文档+审计+基线） |
| Drift | **0** |
| Source-of-Truth Conflicts | **0**（S5U 四文档相互引用一致；与 S5 冻结合同零冲突） |
| Forbidden Expansion Audit | **PASS**（WO-01 交付物仅 Markdown + 截图 PNG；无 runtime/asset/测试改动；无 Tetris/新槽/新词缀/框架迁移等任何禁项触碰） |

## §9 Open Questions

1. **Canonical 2560×1440 无夹取态的复验通道**：编辑器 Game 视图面板像素≠精确 2560×1440（当前 3840×2160 缓冲触发 scale=1.4 夹取）。S5U Final Gate 需要真 canonical（scale=1.333/设计空间 1920×1080）。建议 Phase 5 用 Player（2560×1440 全屏，如 S5 性能门）或固定分辨率视图采集；Phase 2 起的中间验收可用当前 16:9 缓冲态（aspect-true，布局语义同源）+ 布局纯函数单测（1440p DesignScale 无夹取断言已有）补足。**不阻塞 WO-01。**
2. PlayerBaseLife=9999999（调试神力值）使生命球文本截断——S5U 不改 runtime；该值是否改为正式数值属 gameplay/balance，**超出 S5U 授权**，登记为 Director Input Item（呈现层在 Phase 2 用压缩格式化规避不可读）。
3. 付费素材（Asset Store）：等 Director 输入；未获批不阻塞（原创+CC0 继续）。

## §10 Recommended Next WO

**S5U-WO-02 — Dark ARPG Skin Foundation & Combat HUD Frame**（首张 UI runtime authority 工作令；待规划 AI Gate=ACCEPT 后放行）。

## §11 交付物清单（本提交）

```
docs/reviews/S5U/S5U_PLAN.md                    （新）
docs/reviews/S5U/S5U_VISUAL_CONTRACT.md         （新）
docs/reviews/S5U/S5U_UI_SURFACE_MAP.md          （新）
docs/reviews/S5U/S5U_ASSET_ADMISSION.md         （新）
docs/reviews/S5U/S5U_WO_01_EVIDENCE.md          （新，本文）
docs/reviews/S5U/screenshots/*.png              （新，10 张）
docs/qa/PRODUCTION_SIMULATION_REPORT.json       （测试再生，hash 不变）
docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md    （测试再生，fresh）
docs/DECISIONS.md / docs/ROADMAP.md / docs/reviews/STATUS.md （治理同步，GBK 追加节）
```
