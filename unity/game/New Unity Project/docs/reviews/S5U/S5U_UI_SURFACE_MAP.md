# S5U_UI_SURFACE_MAP — 当前 UI 表层清单 + S5 合同映射（WO-01/B+C）

**周期**：S5U — Interface Presentation & HUD Renewal（导演 2026-09-09 Arena 实游授权：参考 PoE/暗黑3 重做 HUD/UI）
**工作令**：S5U-WO-01 — Visual Contract & Asset Admission Lock（Phase 0；**运行时改动授权=NONE**）
**本文性质**：当前产品 UI 表层的权威清单（Surface Inventory）+ S5 冻结合同在此表层的承载映射。S5U 后续工作令改造视觉呈现时，本文所列「Must Preserve」语义逐条不可丢失。
**代码真相源**：`Assets/Runtime/Core/Gameplay/SliceHud.cs`（1013 行）+ `SliceDrawerLayout.cs` + `SlicePassiveLayout.cs` + `SliceTooltipLayout.cs` + `SliceTooltipModel.cs` + `SliceSession.cs`（读路径）。
**截图基线**：`docs/reviews/S5U/screenshots/01..08_*.png`（10 张，见 `S5U_WO_01_EVIDENCE.md` §1）。

## 0. 运行时几何事实（2026-09-09 编辑器实测，unity-cli eval 反射）

| 项 | 值 | 说明 |
|---|---|---|
| GameView 后备缓冲 | **3840×2160（精确 16:9）** | 编辑器 Game 视图（导演实游所见） |
| `DesignScale(w,h)` | **=1.4**（`Mathf.Clamp(Min(w/1920,h/1080),0.4,1.4)` 触发上夹取） | 源码 `SliceHud.DesignScale` |
| 有效设计空间 | **2742.9 × 1542.9**（非 1920×1080） | 右锚元素（导航/抽屉）随 Dw 外扩；中央底栏保持设计居中 |
| 派生事实 | 1920×1080 基准的布局函数在任何视口都成立（右锚=Dw-312 等），但**当视口宽高比≠16:9 或 scale 触发 [0.4,1.4] 夹取时，设计空间宽≠1920**，各表面间距显著变化 | 基线截图均在 16:9 缓冲下捕获（无纵横失真），但 scale=1.4 夹取态；canonical 2560×1440 无夹取态（scale=1.333）为 S5U Final Gate 复验标准 |

> 换算系数（本基线）：capture_px = design_px × (1.4/3840×2560) = design_px × 0.9333。

## 1. Surface Inventory（23 表层）

每项：Surface / Current Presentation（现状呈现）/ Authoritative Behavior（权威行为=代码事实）/ Must Preserve（S5U 不得丢失的语义）/ Visual Problem（导演不可接受点）/ Target Presentation（S5U 目标，引用 Visual Contract 线框）/ Owner / Tests。

### S-01 顶部状态栏（Top Status）
- **Current**：设计空间 `(12,10,560,84)` 深底金边板；三行纯文本：StatusCopy 标题 / `稳定度·收益·废料·蚀刻剂·天赋点` / `LastMessage·LastLoot`。
- **Behavior**：StatusCopy 随 MapState 切换（TownFree/CombatLocked/Cleared/DeadRespec）；资源行随 Session 字段实时刷新。
- **Must Preserve**：四态文案语义；五项资源数值；消息/拾取反馈通道。
- **Visual Problem**：纯文本工程条；与战斗 HUD 无视觉层级关系；占位左上角割裂。
- **Target**：WF-Combat（VISUAL_CONTRACT §2）——状态徽章化收编进顶部极简条，资源收编进 HUD 角标。
- **Owner**：SliceHud.DrawTop。**Tests**：无直接（经 Surface 快照）；S5U Phase 2 起新增 HUD 文本契约测试。

### S-02 导航按钮（Nav）
- **Current**：右上角 `(Dw-296,10,284,44)`，三枚文本按钮 角色/地图/制作，选中态金色高亮。
- **Behavior**：点击切换 `Session.Panel`（Build/Map/Craft 三态互斥）。
- **Must Preserve**：三面板入口 + 选中态反馈；Esc 关闭面板。
- **Visual Problem**：文本按钮无图标；位置远离视线热点。
- **Target**：WF-Combat 右上紧凑图标导航（可保留文字标签）。
- **Owner**：DrawNav。**Tests**：同 S-01。

### S-03 生命球（Life Orb）
- **Current**：底栏左端 92px 圆；底部按比例填充（Group 裁剪）+程序化石环金边；球心文本 `cur/max`。
- **Behavior**：`Life/MaxLife`；RecalcPlayer refill。
- **Must Preserve**：数值真值（cur/max）；死亡=0 关联 DeadRespec 横幅。
- **Visual Problem**：**调试神力基线 `PlayerBaseLife=9999999` 使文本 `99999999/9…` 截断不可读**（基线截图 01/02 可证）；纯色填充无质感。
- **Target**：WF-Combat 左下主锚——D3 式 globe frame+mask+fill；数值压缩显示（万进制或省略）+hover 精确值。**注意：`PlayerBaseLife` 数值本身是 Runtime 调试值，S5U-WO-01 不改运行时；仅记录呈现问题。**
- **Owner**：DrawSkillHud/DrawOrb。**Tests**：S5U Phase 2 增加球体文本格式纯函数测试。

### S-04 法力球（Mana Orb）
- **Current**：底栏左二 92px 圆，蓝色，同 S-03 结构；`Mana/MaxMana`（55/55 可读）。
- **Behavior**：SpendMana 扣减 / TickRegen 回复。
- **Must Preserve**：数值真值；技能耗蓝关联（不足时不可施放）。
- **Visual Problem**：同 S-03 质感问题。
- **Target**：WF-Combat 右下主锚（与生命球对称）。
- **Owner**：DrawOrb。**Tests**：同 S-03。

### S-05 技能单元格 Q/W/E（Skill Cells）
- **Current**：底栏中央三格 170×86；每格：技能名（_title）+ 热键（右上小字）+ **连接源标注行**（`LinkSourceLabel`：`武器·组0·容1` / `裂石巨刃·组1·容1` / `头盔·组0·容1`）+ 辅助孔行（最多 2 孔，76×40）。
- **Behavior**：点击格上半区选技能（SelectedSkill）；`SupportsOf(skill)` 渲染；`SupportCapacity(skill)` 定孔数；closed 孔=`无孔`（灰显+tooltip）。
- **Must Preserve**（S5 合同直承）：**唯一有效连接源标注（LinkSourceLabel 全语义）**；改挂后原默认位不再呈现为生效源；组号+host+容量三要素；closed/empty/filled 三态；点击选择。
- **Visual Problem**：长文本标注行（`裂石巨刃·组1·容1`）占据格内主要空间=「工程文字说明」常驻战斗 HUD；无技能图标；无 usable 状态视觉（可施放/禁用）。
- **Target**：WF-Combat 中央动作条：图标优先技能格（icon/hotkey/usable 态），连接源降为紧凑徽章（G0/G1 pip）+ hover 展开；完整文本由 tooltip 提供（文本真值不删）。
- **Owner**：DrawSkillCell。**Tests**：`MultiLinkIntegrationTests`（域/集成已覆盖语义）；S5U 增呈现层快照测试。

### S-06 辅助孔（Support Sockets）
- **Current**：格内 76×40 孔；三态：closed（灰+无孔 tooltip）/empty（空+tooltip）/filled（辅助名+SupportCard tooltip）。
- **Behavior**：拖放落点（HandleSocketInput）；点击已填充孔=摘下（TrySetSupport None）并拾起；容量外拒绝。
- **Must Preserve**：三态语义；拖放+点击两种配置流；拒绝提示（LastMessage）。
- **Visual Problem**：孔=纯文本框；无 gem 图形。
- **Target**：gem 图标化孔位+空/闭态框线（VISUAL_CONTRACT tokens SlotNormal/Hover/Disabled）。
- **Owner**：DrawSocket/HandleSocketInput。**Tests**：同 S-05。

### S-07 辅助 tray（Support Tray）
- **Current**：底栏右侧 4 列 gem 网格（84×44×7）；已用辅助名后缀 `*`；机制型左侧 Cinder 色条；hover=SupportCard tooltip；点击=拾起（可拖放/直点放置）。
- **Behavior**：`SupportCatalog.Count=7`；IsLinked 标记；ChangesMechanism 徽记。
- **Must Preserve**：7 辅助全集；已用标记；机制徽记；拾起/拖放输入流；tooltip 兼容性 truth。
- **Visual Problem**：文本 gem；无图标；与技能孔无视觉连线。
- **Target**：图标 gem + 已用态灰化/角标；保留机制徽记。
- **Owner**：DrawSupportTray/DrawGem。**Tests**：同 S-05。

### S-08 装备抽屉（Equipment Drawer 列）
- **Current**：右缘常驻列 `(Dw-312,64,300,326)`：标题「装备」+ 2×3 迷你槽（DisplayOrder：武器/头盔/胸甲/手套/鞋子/腰带）+ 底部 角色/制作 两 tab。
- **Behavior**：常驻可见（任何 Panel 态）；点击槽=开 Build 面板；hover=ItemCard tooltip（TipPriDrawer）。
- **Must Preserve**：六槽=EquipSlot 六槽 canonical；DisplayOrder 人体顺序；点击入口；hover truth。
- **Visual Problem**：槽=文字块；稀有度仅文字着色；无物品图标。
- **Target**：WF-Equipment 右列——六图标槽+稀有度框色+空槽剪影。
- **Owner**：DrawDrawer/DrawMiniSlot/SliceDrawerLayout。**Tests**：几何测试已存在（SliceDrawerLayout 纯函数）；S5U 增图标适配测试。

### S-09 Build 面板（角色页容器）
- **Current**：`(BuildPanel(Dw))` 760×430 抽屉左侧板；标题 `角色 · <StatusCopy>`；内含 GearRow + Inventory + Tree。
- **Behavior**：Panel==Build 时绘制；ShouldBlockWorld=true。
- **Must Preserve**：三区布局语义（装备行/背包/天赋）；图内锁定标题联动。
- **Visual Problem**：三区同权重平铺；无层级。
- **Target**：WF-Equipment（§2）。
- **Owner**：DrawBuild。**Tests**：几何测试。

### S-10 六槽装备卡（Gear Row）
- **Current**：3×2 卡片 241×56；左竖条=稀有度色；卡身=槽名+物品行（稀有度词+基名+孔数）；**第二连接条**（卡底 20px，仅 SecondaryLinkConfigurable 物品）；hover=ItemCard tooltip（TipPriPanel）；点击=SelectedInv。
- **Behavior**：只读 canonical Equipped；`SecondaryLinkConfigurable`（映射槽+SocketCount≥3）同源判定。
- **Must Preserve**：六槽卡完整信息；稀有度区分；点击选择；tooltip truth。
- **Visual Problem**：卡=纯文本块；第二连接条与卡身同质化。
- **Target**：WF-Equipment 装备卡=框体+图标位+名称+词缀摘要+独立第二连接控件。
- **Owner**：DrawGearRow/DrawSlotCard。**Tests**：`S5BuildInteractionTests`（语义）+几何测试。

### S-11 第二连接配置条（Rebind Strip）【S5 核心】
- **Current**：卡底条：`第二连接` 标签+候选按钮（`RebindCandidates`：排除映射技能与已被他挂者，含当前绑定自身）+ `无` 按钮（有改挂时）。
- **Behavior**：可用候选=NavBtn（当前绑定高亮），点击走 `TryReassignLink`（唯一写入口）；不可用候选=灰显+hover 原因 tooltip+**点击=确定性可见拒绝**（LastMessage=原因，状态零改动）；`无`=清除改挂。
- **Must Preserve**（**S5 合同 §7 直承，逐条冻结**）：候选过滤语义（映射槽排除/已被其它装备改挂排除）；PreviewReassignError 与写前校验同内核；**拒绝必须可见且原因明确**；LinkSkill1=None/active 双态；组0/组1 truth（LinkGroupsText：`组0 连接：X 容N;组1 连接：Y 容1`）；3S=0+1 划分呈现；`无` 清除入口。
- **Visual Problem**：纯文本按钮条；拒绝原因在顶栏消息行而非控件附近。
- **Target**：WF-Equipment 卡内「第二连接」控件组：候选=技能图标 chip（可用/灰/激活三态）+ 拒绝原因就地浮层；语义全保留。
- **Owner**：DrawRebindStrip。**Tests**：`MultiLinkIntegrationTests` E/F/G（拒绝矩阵）+ 域核 `MultiLinkDomainTests`。

### S-12 背包列表（Inventory）
- **Current**：Build 面板左下 360×260 滚动列表；行=46px（稀有度边条+`稀有度词 基名`+词缀摘要两行）；hover=ItemCard tooltip；**点击行=TryEquip（含失败原因进 LastMessage）**；SelectedInv 高亮。
- **Behavior**：`InventoryCap` 上限；点击=装备尝试（canonical TryEquip 合同）；滚动视图。
- **Must Preserve**：点击装备流（含拒绝文案）；候选 vs canonical tooltip（工作令 七-十）；选择态。
- **Visual Problem**：**列表而非网格**（导演预期 ARPG 网格）；行=纯文本。
- **Target**：WF-Equipment 背包**网格呈现**（cell/frame/empty/hover）——**仅呈现**：格子容量语义、无 Tetris/占格 mechanics（Forbidden Expansion）。
- **Owner**：DrawInventory。**Tests**：TryEquip 语义已覆盖；S5U 增网格几何测试。

### S-13 天赋树（Passive Tree）
- **Current**：Build 面板右下 368×260；16 节点（TreePos 归一化）+20 edges（PassiveCatalog.Links 驱动，零 synthetic）；节点=色条文本块（Notable/Mechanic 86×40，普通 70×32）；三态色（Allocated 金/Cinder、Available 绿、Lock 灰）；hover=TextCard tooltip；点击=TryAllocate（拒绝原因进 LastMessage）。
- **Behavior**：`NodeState(i)` 单一真值；`Allocated[]`；Unspent 计数。
- **Must Preserve**：16 节点/20 edges canonical；三态语义；Notable/Mechanic 分级；分配流+拒绝反馈；interaction contract（current）。
- **Visual Problem**：文本节点；edges=2px 深色线（低可读）；无选中态视觉（只有 hover）；面板无框。
- **Target**：WF-Passive：节点=icon/socket 图形分级、edges 高亮（active 路径亮）、selected 独立态、面板框线；**不改 gameplay**（Phase 4）。
- **Owner**：DrawTree/SlicePassiveLayout。**Tests**：几何测试+NodeState 语义已覆盖。

### S-14 制作面板（Craft）
- **Current**：`CraftPanel(Dw)` 560×340；顶部资源行（废料/蚀刻剂）；`_craftResult` 物品体区（SelectedInv 物品：稀有度边条+DrawItemBody 全词缀）或「在角色面板点选一件装备」；随机制作/定向制作按钮（含资源计数）；「定向写入」21 词缀按钮（5 列网格，CraftAffixPick 高亮）。
- **Behavior**：TryRandomCraft/TryDirectedCraft（含拒绝原因）；词缀选择=CraftAffixPick；hover 结果区=ItemCard tooltip。
- **Must Preserve**：选择→当前词缀→动作→成本→结果/拒绝 信息流语义；21 词缀全集选择器；两类制作入口；资源计数真值。
- **Visual Problem**：全部塞进同色文字面板（导演点名）；无 item/affix 区分层级；无成本/错误分区。
- **Target**：WF-Craft：Selected Item / Current Affixes / Available Action / Cost / Result-Rejection 五区明确分区（§2）。
- **Owner**：DrawCraft。**Tests**：CraftRegression（语义）已覆盖。

### S-15 地图面板（Map）
- **Current**：`(12,140,480,300)`：标题 `地图 灰烬庭院 · 状态`；提示行；3 行 MapAffix（复选框 16px+名称+稳定-成本+收益+加成+描述，选中行金底）；`稳定度/收益` 汇总行；进入地图/出图按钮（Return 键同效）。
- **Behavior**：ToggleMapAffix；RefreshReward 即时更新；TryEnterMap/ExitMap；进入后 Panel=None。
- **Must Preserve**：3 地图词缀全集+开关语义；稳定/收益实时计算；进出图双入口（按钮+Enter）；出图免费重铸语义（StatusCopy）。
- **Visual Problem**：复选框=像素方块；行=纯文本；无地图主题视觉。
- **Target**：WF-Map：词缀=勾选卡（图标+代价徽章）；进图按钮=大 CTA；面板框线。
- **Owner**：DrawMap。**Tests**：MapAffix 语义已覆盖（S4 系）。

### S-16 Tooltip 系统
- **Current**：单渲染器（DrawTooltip 唯一出口）；优先级 TipPriPanel(3)>Drawer(2)>BottomBar(1)>TopNav(0)，同级先到先得；位置=SliceTooltipLayout.Place（右下→左翻→上翻→视口钳制）；卡片结构=Title(稀有度着色)/Subtitle/Badge(金)/Body 行/ContextTitle+Context/Footer；石底金边 9-slice。
- **Behavior**：RequestTip 单一请求口；每帧重置；三种卡（ItemCard/SupportCard/TextCard）。
- **Must Preserve**：ItemCard 的**连接组划分行（LinkGroupsText）+ 第二连接行 + 已装备标注 + 对比区语义**；SupportCard 兼容性 truth；优先级次序（面板>抽屉>底栏>顶栏/导航）；Place 翻转/钳制几何合同。
- **Visual Problem**：纯文本行；无稀有度色带/图标位；宽度 360 固定。
- **Target**：WF-Tooltip 层级：NAME / type·slot·context / 分隔 / Primary values / Affixes / 分隔 / Support·Link / Secondary explanation（§2）——**现有卡语义全部保留为新层级的槽位**。
- **Owner**：DrawTooltip/SliceTooltipModel/SliceTooltipLayout。**Tests**：SliceTooltipLayout 几何测试已存在。

### S-17 拖放系统（Drag & Drop）
- **Current**：tray gem 点击=拾起（_picked）；MouseDrag>16px=_dragging（DrawDragGhost 跟随指针显示辅助名）；MouseUp 于孔上=PlaceSupport；点击空孔直接放置 _picked。
- **Behavior**：EndDragIfNeeded 状态机；TrySetSupport 唯一写入。
- **Must Preserve**：拾起/拖放/直点三输入流；ghost 反馈；失败走 LastMessage。
- **Visual Problem**：ghost=纯文本条。
- **Target**：gem 图标 ghost+目标孔高亮。
- **Owner**：HandleSocketInput/EndDragIfNeeded/DrawDragGhost。**Tests**：TrySetSupport 语义已覆盖。

### S-18 选择态（Selection）
- **Current**：SelectedSkill=技能格金色高亮（_slotSel）；SelectedInv=背包行金底/卡体选择。
- **Behavior**：技能选择影响施放；物品选择影响 Craft 结果区。
- **Must Preserve**：两选择域语义与联动（Craft 读 SelectedInv）。
- **Visual Problem**：高亮=纯色填充。
- **Target**：SlotSelected token+角标。
- **Owner**：DrawSkillCell/DrawInventory。**Tests**：现有。

### S-19 悬停态（Hover）
- **Current**：所有可交互件 hover=SlotHover 背景（程序化石纹）；tooltip 同帧请求。
- **Behavior**：Pointer 统一（DebugHover 钉点支持编辑器视觉验证——本次基线截图即用此通道）。
- **Must Preserve**：hover↔tooltip 同帧耦合；DebugHover 验证通道（测试/截图基建）。
- **Visual Problem**：hover 与 normal 差异弱。
- **Target**：tokens SlotNormal/Hover 差异化（金边内亮）。
- **Owner**：EnsureStyles。**Tests**：几何测试。

### S-20 禁用/锁定态（Disabled / Locked）
- **Current**：closed 孔=灰化+`无孔`；BuildLocked（图内）=全部 Try* 拒绝（`图内锁定构筑`）+StatusCopy=CombatLocked；面板标题随状态。
- **Behavior**：RejectReason 用户可读。
- **Must Preserve**：锁定语义与文案；灰显规则（不可用候选=灰+原因）。
- **Visual Problem**：锁定=只有文案；无视觉态。
- **Target**：SlotDisabled token+面板锁定徽记。
- **Owner**：DrawSocket/DrawRebindStrip。**Tests**：拒绝矩阵已覆盖。

### S-21 失败消息面（Failure Message）
- **Current**：`s.LastMessage` 顶栏第三行（含 LastLoot 拼接）；rebind 拒绝/装备失败/分配失败/制作失败/支持不兼容全部落此通道。
- **Behavior**：唯一用户可读拒绝通道（除 tooltip 原因）。
- **Must Preserve**：**所有拒绝原因的可见性**（确定性可见拒绝合同）。
- **Visual Problem**：消息藏在顶栏，与触发控件无空间关联。
- **Target**：消息徽章化（顶栏保留）+关键控件就地浮层（Phase 3/4 渐进）。
- **Owner**：DrawTop/各 Try* 调用点。**Tests**：拒绝矩阵已覆盖。

### S-22 横幅（Banner）
- **Current**：Dead（深红）/Cleared（深绿）居中横幅 440×36@y86。
- **Behavior**：MapState 驱动。
- **Must Preserve**：两态文案与时机。
- **Visual Problem**：色块+文本。
- **Target**：主题化横幅（V 徽记/框线）。
- **Owner**：DrawBanner。**Tests**：现有。

### S-23 点击反馈（Click Flash）
- **Current**：金色高亮闪 0.16s 衰减叠加命中元素。
- **Behavior**：ClickFlash/DrawFlash。
- **Must Preserve**：点击反馈存在性。
- **Visual Problem**：与 hover 色混淆。
- **Target**：保留（token 化）。
- **Owner**：DrawFlash。**Tests**：现有。

## 2. S5 合同映射（冻结语义 × 承载表层）

| S5 冻结合同项 | 当前承载表层 | Must-Preserve 断言 |
|---|---|---|
| LinkSkill1 = None / active 双态 | S-11（无 按钮 vs 候选按钮）；S-05（LinkSourceLabel 组源标注） | 双态在任何改版中可辨识；None 时组1 不呈现为生效源 |
| Secondary Link 配置（唯一写入口 TryReassignLink） | S-11 | 任何视觉改版仍唯一经 TryReassignLink 写入；UI 不得旁路 |
| Group0 / Group1 truth（3S=0+1 划分） | S-05（`X·组N·容C` 标注）；S-16（ItemCard 组划分行） | 组号+host+容量三要素完整 |
| effective source exactly once（唯一有效源） | S-05 LinkSourceLabel（RebindHostIndex fail-closed） | 腐败构造态 fail-closed 回默认组呈现 |
| true capacity（真实容量） | S-05 孔数+closed 孔 `无孔`；S-16 组行 `容N` | 容量=SupportCapacity 单一真值 |
| 3S split = 0+1 | S-16 ItemCard（`组0 连接：近战 容1;组1 连接：弹道 容1`）；S-10 卡身 `N孔` | 3 孔=组0容0+组1容1 不暗示免费孔 |
| candidate filtering（候选过滤） | S-11 RebindCandidates | 映射技能排除+被他挂排除+含自身 |
| rejection reason（拒绝原因） | S-11 灰候选 hover tooltip+点击 LastMessage；S-21 | 确定性可见拒绝，状态零改动 |
| support compatibility | S-07 SupportCard tooltip（canonical runtime 兼容性）；写入前门 IsSupportCompatible | 兼容性 golden 唯一真值不变 |
| support isolation（S5 隔离语义） | S-05/S-06/S-07（支持配置仅作用于技能） | 不跨技能漂移 |
| Affix tooltip truth | S-16 ItemCard 词缀行 | 词缀行=AffixLine/ItemAffixSummary 真值 |
| Q/W/E 技能选择+施放入口 | S-05 | 三技能恒在 HUD |
| Tab/F6/F8/Esc/R 键位 | HandleKeys | 键位语义不动（视觉可改） |
| 六槽 canonical（EquipSlot 0-5） | S-08/S-10 | DisplayOrder 仅为 UI 顺序，ID 不漂移 |

**裁决基线**：`docs/reviews/S5/S5_LINK_CONTRACT.md` + `S5_AFFIX_ADMISSION.md` + `S5_SCOPE_LEDGER.md`；S5 Final Gate=PENDING/DEFERRED（2026-09-09 导演 UI 干预后由 S5U 承接 presentation 层；S5 语义层冻结不动）。

## 3. WO-01 范围声明

本文只做**记录**（Current/Behavior/Problem/Target），不实现任何呈现改动。Runtime Product Delta=NONE；所有「Target」列为 S5U Phase 1-4 的实现规格锚点，经后续工作令逐项落地与测试。

## 4. WO-02 实现真值（战斗表层 S-01..S-07，2026-09-09）

> 下列表层已由 S5U-WO-02 落地「Target」首轮（Dark ARPG Skin Foundation & Combat HUD Frame）；Must Preserve 全部保留且有测试锚定。证据=`S5U_WO_02_EVIDENCE.md`（7 张 canonical After 截图）。

| Surface | WO-02 实现 | Must Preserve 校验 |
|---|---|---|
| S-01 顶部状态栏 | 三行结构保留；标题/数据行间加原创金饰线（SliceHudIcons.Separator 256×6） | 四态文案/五资源/消息通道不变（代码同源） |
| S-02 导航 | 未重绘（WO-03 范围） | 三面板入口+选中态不变 |
| S-03 生命球 | **已换装**：128² 原创暗铁金圈球环（GlobeFrame）+紧凑文本（10.0M/10.0M）+hover 精确值 tooltip | cur/max 真值；死亡=0 关联不变 |
| S-04 法力球 | **已换装**：同 S-03 对称 | 数值真值/关联不变 |
| S-05 技能单元格 | **已换装**：96² 图标优先格（原创符文+热键角标 18×14+连接徽章 `G{组}·容{N}` 84×14+pip 行+弱化名注+点击选择） | LinkSourceLabel 全语义降级为徽章+tooltip 承载（文本真值不删，tooltip 三行 Body）；点击选择流不变 |
| S-06 支持孔 | **已换装**：14×14 中性金属 pip 三态（closed/empty/filled=旧 DrawSocket 语义 1:1；PipFor 恒等映射测试锁死）；pip 悬停=独立 SupportCard（不与框卡互顶） | 三态语义/拖放+点击流/拒绝提示不变 |
| S-07 辅助 tray | 未重绘（WO-03 范围）；几何并入战斗底栏单一来源（CombatBarRects） | 7 辅助全集/已用标记/机制徽记/输入流不变 |

**几何单一来源**：`SliceHud.CombatBarRects(dw,dh)`（public 纯函数）——S-03..S-07 全部元素矩形出自同一函数，`S5UHudTests` 三设计空间（1920×1080 共同空间 / 1.4 上夹取空间 / 1280×720 小窗）断言不重叠不出界。
**连接徽章真值映射**：`SliceSession.LinkBadgeText(skill)` = `"G"+组+"·容"+SupportCapacity(skill)`；与 `LinkSourceLabel` 同源（组/容量逐字符一致有测试）。
**§0 几何事实补充（canonical 通道）**：WO-02 起所有 After 截图经 canonical 无夹取通道采集——GameView `m_TargetSize`=2560×1440（GAME-ZZZ-CANON FixedResolution 注册）→ Screen=2560×1440、DesignScale=1.3333、**设计空间=1920×1080**；§0 表内 1.4 夹取态换算系数仅适用于 WO-01 历史基线。

## 5. WO-03 实现真值（装备/背包呈现 S-08..S-16，2026-09-09）

> 规划 AI 2026-09-09 放行令落地：抽屉壳+装备 2×3+背包呈现网格+ItemCard 层级+第二连接条带+紧凑格式化升位修正。证据=`S5U_WO_03_EVIDENCE.md`（7 张 canonical After）。

| Surface | WO-03 实现 | 契约校验 |
|---|---|---|
| S-08 抽屉壳 | **已换装**：全壳面板（头部「角色 / 背包」+饰线 / Build-Craft tab（语义不变）/ 装备区+背包区 / 底部反馈条 LastMessage）；`SliceDrawerLayout.Shell 系`纯函数=单一来源；壳底缘恒在战斗栏顶上方 8px（测试断言三设计空间） | 不与战斗栏重叠；2560/1920 双分辨率完整可见 |
| S-09 装备 2×3 | **已换装**：146×86 槽=槽位类型符文（6 件原创 EqGlyph，generic 不伪装物品）+槽名+物品名（稀有度真值着色）/空槽暗符文+「空」；hover=ItemCard；点击=开角色页（语义不变） | **恒 6 槽**（禁 Ring/Amulet/Offhand）；EquipSlot ID 不漂移 |
| S-10 背包网格 | **已换装**：92×70×3 列滚动呈现网格（1 物品=1 恰一格；顺序=InventoryCount 真值顺序；零 width/height/占位/旋转/容量机制） | 网格=纯呈现（ShellInvContentHeight=ceil(count/3) 行高，测试锁定） |
| S-11 物品格层级 | 符文+稀有度左缘（既有真值）+名称+词缀摘要+已装备徽记；**禁项全未添加**（无 item level/quality/socket 色/新稀有度层/unidentified/vendor value） | 既有真值零增删 |
| S-12 ItemCard | 层级=标题（稀有度着色）/副题（稀有·槽·孔）/**金分隔线×2（渲染层新增）**/正文（基础+词缀+连接组真值）/对比区/操作提示；模型（SliceTooltipModel）零改动 | Affix 通用渲染不变；无占位泄漏 |
| S-13 状态语言 | Normal/Hover/Selected（石质框四态沿用）+已装备徽记+空槽暗符文；优先级 selected > hover > normal（GUI 框态选择序） | 视觉状态不改变选择逻辑 |
| S-14 装备/拖放合同 | 点击网格=既有 `SelectedInv=i + TryEquip(i)`（逐字保留）；装备经既有 post-validator；**零直接写 LinkSkill1/零自动迁移** | 换装/替换路径不变（SixSlot PlayMode 测试绿） |
| S-15 第二连接条带 | 既有条带保留（资格=SecondaryLinkConfigurable 同源；候选=RebindCandidates；写入唯一 TryReassignLink；拒绝=确定性可见）；本 Wo 仅随装备卡符文化呈现 | 容量/候选/拒绝语义精确（MultiLink 套件绿） |
| S-16 物品 Tooltip | 与 WO-02 tooltip 同族（石底金边卡+分隔线）；**修复：网格 cell 悬停坐标换算（内容坐标→设计空间）——旧列表悬停检测从未对齐，本 Wo 修正后 cell hover→ItemCard 可靠触发；容器（壳/面板）结构性不请求 tooltip，无同优先级抢占** | 视口钳制/翻转沿用 SliceTooltipLayout |

**WO-03 合同修正（规划 AI 非阻塞项）**：`SliceHudFormat.Compact` 升位——999999→`1.0M`（K/M/T 段舍入达上段阈值即升位；禁 1000.0K/1000.0M 伪影）；formatter 测试同步（含全窗口扫描测试）。
**Build 页调整**：背包列表自 Build 面板**移除**（网格移交常驻抽屉壳=单一背包表面）；天赋树放宽至面板整幅（SlicePassiveLayout 按矩形布局，语义零改动）；装备明细行保留（符文化+既有连接/条带）。
