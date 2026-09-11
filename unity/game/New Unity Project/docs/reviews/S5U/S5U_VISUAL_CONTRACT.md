# S5U_VISUAL_CONTRACT — 技术锁 + 线框 + 设计 Tokens（WO-01/D+E+F）

**周期**：S5U — Interface Presentation & HUD Renewal
**权威来源**：规划 AI S5U 计划（会话 `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`，2026-09-09 完整重发版）；导演授权源=2026-09-09 Arena 实游反馈（「参考 PoE 或暗黑 3 来制作 HUD 和 UI，如果本机有可用素材也可以使用」）。
**本文效力**：S5U 全周期视觉呈现的唯一 authoritative contract；后续工作令的视觉实现不得偏离本文；偏离需规划 AI 修订令。

## §1 技术锁（Technology Lock — D）

| 项 | 裁定 |
|---|---|
| **Selected** | **IMGUI（OnGUI）——全面视觉重做 Route A** |
| UI Toolkit 迁移 | **NOT AUTHORIZED** |
| uGUI 迁移 | **NOT AUTHORIZED** |
| 工作 AI 自行换路线 | **禁止**（B/C 不是 fallback） |

理由（规划 AI 已裁定）：当前要解决的是 presentation quality failure，不是 UI framework failure。现有 IMGUI 已承载并验证 Q/W/E、Tab/F6/F8/Esc/R、Drawer、Build、Craft、Passive、Map、Tooltip、Drag/drop、第二连接配置、Link source、Support slot、group/capacity truth、314 EditMode、11 PlayMode、既有 layout 纯函数、1920×1080 design-space scaling。迁移框架会把 presentation/interaction/geometry/input/automated tests/performance 绑进同一次高风险重写，与导演「尽快把 UI 做到能看」相反。未来若有明确证据证明 IMGUI 阻止正式产品目标，单独开 Architecture Gate。

IMGUI 性能规则（全周期有效）：
- 禁止：OnGUI 每帧生成 texture / 每帧创建大批 GUIStyle / 每帧 load resource / 无界 tooltip allocation / 热渲染无界 LINQ allocation / UI asset 重复解码 / 不受控 layout GC。
- 要求：texture 缓存（SliceSkin 程序化石纹模式延续）、GUIStyle 缓存（EnsureStyles 单次）、GUIContent 合理复用、atlas/asset lookup 有界、视觉装饰不改变 gameplay sim。

## §2 七个权威线框（Wireframes — E）

> 全部为 ASCII 布局规格（设计空间 1920×1080 基准书写；右锚/居中规则随 Dw 换算，见 SURFACE_MAP §0 几何事实）。标注 = [方块: 呈现元素]；`↳` = hover 交互；**禁止在本工作令实现**。

### WF-1 Combat HUD（战斗底栏）
```
┌─────────────────────────── 视口（Arena 世界层在最底） ───────────────────────────┐
│ [Top 极简条: 状态徽章 | 稳定/收益微章]                    [右上 Nav: ⚔ 🗺 ⚗ 图标] │
│                                                                                  │
│                       （Target / temporary status 仅在有意义时出现）              │
│                                                                                  │
│   ╭──────╮                                          ╭──────╮                     │
│   │ LIFE │  [ut] [ut]  ┌────┐┌────┐┌────┐  [ut] [ut] │ MANA │   ← 底部中央聚合    │
│   │ globe│  utility   │ Q  ││ W  ││ E  │  utility  │ globe│                     │
│   ╰──────╯  ring     │icon││icon││icon│   ring    ╰──────╯                     │
│     86px             │ Q  ││ W  ││ E  │                                        │
│   ▓ life fill        └────┘└────┘└────┘   ← 每格: icon+hotkey+usable 态       │
│                              [G0●][G1●]  ← 连接组紧凑 pip（G0/G1 角标）          │
│                              ↳ hover: 连接源完整文本（host·组·容）=tooltip      │
│       [support tray: 7 gem 图标, 已用=灰+角标]  ↳ hover: SupportCard           │
└──────────────────────────────────────────────────────────────────────────────────┘
```
- 双球对称（左 LIFE / 右 MANA）包夹中央 Q/W/E——经典 ARPG silhouette（D3 参考）。
- combat-critical HUD 保持相对靠近中央视觉区（宽屏不无限拉极端；PoE Centred UI 原则）。
- S5 信息不删：LinkSourceLabel 完整文本降为 hover tooltip；G0/G1 pip 常驻。
- Q/W/E 可施放态（蓝足/冷却/锁定）必须可见。

### WF-2 Equipment / Inventory（角色页）
```
┌ Build 面板（抽屉左侧） ────────────────────────────────┐ [抽屉列常驻]          │
│ 角色 · <状态>                                           │ ┌──装备─────────┐    │
│ ┌─六槽装备卡 3×2────────────────────────────────────┐  │ │ ⚔武器  🪖头盔 │    │
│ │ [slot icon][name + rarity 框色]                    │  │ │ 🛡胸甲  🧤手套 │    │
│ │  词缀摘要 ≤2 行                                    │  │ │ 👟鞋子  📜腰带 │    │
│ │  [第二连接: (近战)(弹道)(范围)(无) chip 组]         │  │ │  (icon+框色)  │    │
│ │  ↳ 不可用 chip=灰+hover 原因；点击=确定性拒绝浮层   │  │ ├───────────────┤    │
│ └────────────────────────────────────────────────────┘  │ │ [角色] [制作] │    │
│ ┌─背包网格 6×4──────────────┐ ┌─天赋（WF-5）────────┐  │ └───────────────┘    │
│ │ [□][□][□][□]              │ │                     │  │                      │
│ │ [□][ITEM][□][□] ...       │ │                     │  │                      │
│ │  ↳ hover: ItemCard        │ │                     │  │                      │
│ │  点击=TryEquip（语义不变）│ │                     │  │                      │
│ └───────────────────────────┘ └─────────────────────┘  │                      │
└─────────────────────────────────────────────────────────┘                      │
```
- 背包**网格化=仅呈现**：cell/frame/empty/hover；容量语义不变；**禁止 Tetris/物品占格 mechanics**（Forbidden Expansion）。
- 第二连接 chip 组=icon 化候选（可用/灰/激活三态），拒绝原因就地浮层（保留顶栏消息兜底）。

### WF-3 Build 页信息层级（六槽卡优先级）
```
[icon/silhouette] → [item name (rarity 色)] → [stats/词缀 ≤3 行] → [第二连接控件]
                                                   ↳ 更多词缀: tooltip
```
- equipment-frame first；icon first；name second；stats third；Secondary Link 与装备绑定显示。

### WF-4 Craft（制作页五区流）
```
┌ 制作 · <状态> ────────────────────────────┐
│ [资源: 废料 N · 蚀刻剂 M]（角标化）        │
│ ┌─① Selected Item────┐                    │
│ │ icon+name+稀有度框  │                    │
│ ├─② Current Affixes──┤                    │
│ │ 词缀行（分区背景）  │                    │
│ ├─③ Available Action─┤                    │
│ │ [随机制作] [定向制作]│  ← 按钮+成本徽章  │
│ ├─④ Cost/Requirement─┤                    │
│ │ 成本行（不足=红）   │                    │
│ ├─⑤ Result/Rejection─┤                    │
│ │ 结果卡 / 拒绝原因   │                    │
│ └─定向写入: 21 affix chip 网格（选中高亮）─│
└────────────────────────────────────────────┘
```
- item information / action / cost / error 四类信息**不得**再塞同一色文字面板。

### WF-5 Passive（天赋）
```
┌ 天赋面板（框线+标题栏: 已点 N/M）──────────────┐
│   node──edge──node      edge: active=亮金      │
│    ╲          ╱         inactive=暗            │
│     node       node     node: [icon/孔图形]    │
│                         分级: 普通/Notable/Mechanic 尺寸+框 |
│  ↳ hover: TextCard；selected=独立描边（新增）  │
└────────────────────────────────────────────────┘
```
- 保留 16 节点/20 edges/interaction contract；**本周期不重做 passive gameplay**。

### WF-6 Map（地图页）
```
┌ 地图 · <状态> ──────────────┐
│ [进图前勾选词缀提示]         │
│ ┌──────────────────────┐   │
│ │ ☑ affix card          │   │
│ │   icon + 稳定-2 收益+.5│  │
│ │   描述                 │   │
│ └──────────────────────┘×3 │
│ 稳定 96 · 收益 x1.50（实时）│
│ [   进 入 地 图 (大 CTA)  ] │
└─────────────────────────────┘
```
- 不新增 Map Tier/Atlas/progression。

### WF-7 Tooltip 层级
```
┌─────────────────────────┐
│ ITEM / SKILL NAME（稀有度色）│
│ type · slot · context（孔数/已装备/对比源）│
│ ────────────────────────│
│ Primary values（主属性） │
│ Affixes / effects（词缀行）│
│ ────────────────────────│
│ Support / Link information│
│   （组0/组1 划分行+第二连接行+容）│
│ Secondary explanation     │
│ Footer（操作提示）        │
└─────────────────────────┘
```
- 必须保留：group 0 / group 1；actual capacity；Secondary Link None/active；S5 3S split truth；rejection information（现状卡全部槽位语义平移进新层级）。

## §3 Art 方向（导演授权范围）

**参考原则（REFERENCE ONLY）**：PoE × Diablo III——信息架构、视觉层级、HUD silhouette、信息密度、交互呈现；**不复制**任何受版权保护资产（见 ASSET_ADMISSION §IP）。
- PoE：战斗信息向玩家视野集中、底部聚合、support/link 紧凑 visual indicator（pip/badge）而非常驻长文本。
- D3：经典 silhouette（左 Life orb — 中央 action bar — 右 Mana orb）、暗黑框体、低重心、图标优先、临时/常驻信息分层。

**GAME-ZZZ 自有方向（Dark ARPG）**：Black stone + Dark oxidized metal + Worn bronze + Muted gold + Deep blood red + Dark arcane blue + Restrained rune ornament。观感=沉、硬、古旧、战斗化。
**禁止**：手游亮色面板 / 卡通圆角 / Material Design / 开发工具灰框 / Excel 式页面 / 高饱和霓虹 / 全区域等视觉权重。

## §4 Design Tokens（F — 家族契约）

> WO-01 锁 token **家族与语义**；最终 RGB 像素值在 Phase 1（Skin Foundation）随资产落地锁死。此后**禁止**散布 uncontrolled magic styles——所有颜色/间距/框宽/字号必须经 token 取值。

### 色彩（Color）
| Token | 语义 | 初值锚（待 Phase 1 锁定） |
|---|---|---|
| Background | 全屏最底 | 近黑（#0B0A09 系） |
| Panel | 面板石底 | 黑石（#14120F 系） |
| PanelRaised | 抬升面（tooltip/卡） | 深氧化金属（#1C1915 系） |
| Border | 常规框线 | 暗铜（#5A4A2E 系） |
| BorderHighlight | 强调框线（选中/hover） | 亮金（#C9A227 系） |
| Ornament | 纹饰（角花/分隔纹） | 做旧铜绿（#6E5B36 系） |
| TextPrimary | 主文本 | 米白（#E8DCC0 系，延续 SliceSkin.TextCream） |
| TextSecondary | 次文本 | 灰米（#A89878 系） |
| TextMuted | 弱文本 | 暗灰（#6B6252 系） |
| TextDisabled | 禁用文本 | 灰化（#4A453C 系） |
| Health | 生命/血液 | 深血红（#8A1A12 系） |
| Mana | 法力 | 暗奥蓝（#1E3A5C 系） |
| Accent | 交互强调 | 哑金（#C9A227 系=BorderHighlight 复用） |
| Warning | 警示 | 锈橙（#B06A1E 系） |
| Success | 成功 | 苔绿（#4A7A3A 系） |
| Error | 错误/拒绝 | 血红（#A02418 系） |
| Rare / Ordinary | 稀有度色 | 现有 SlicePalette.Rare/Ordinary 迁移锚 |

### 稀有度/状态框（Slot States）
| Token | 语义 |
|---|---|
| SlotNormal | 默认槽/孔框（暗框+石底） |
| SlotHover | hover 态（金边内亮） |
| SlotSelected | 选中态（金边+角标） |
| SlotDisabled | 禁用/锁定态（灰化+无交互） |

### 间距（Spacing）
| Token | 值（设计空间 px） |
|---|---|
| SpacingXS | 4 |
| SpacingS | 8 |
| SpacingM | 12 |
| SpacingL | 24 |

### 框宽（Frame）
| Token | 值 |
|---|---|
| FrameThin | 1px（分隔线/次级框） |
| FrameNormal | 2px（面板/槽） |
| FrameHeavy | 4px（主面板/双球外环） |

### 字体（Typography）
| Token | 语义 | 锚 |
|---|---|---|
| FontTitle | 标题/物品名 | 15 Bold（现状 _title 迁移） |
| FontBody | 正文 | 13（_body） |
| FontSmall | 次级/摘要 | 11（_small） |
| FontHotkey | 热键角标 | 11 Bold（格内右上） |

### 字体资产策略
中文字体：沿用引擎默认（现状）至 Phase 4 typography polish；如需商用字体=**Director Input Required**（付费/授权输入点）。

## §5 视觉验收标准（吸收规划 AI ⑫节）

S5U Final Gate 至少提供：Arena clean combat HUD / two-link active / item tooltip / Equipment+Inventory / Build+Passive / Craft / Map。Canonical Director screenshot=**2560×1440**（无夹取态，scale=1.333，设计空间=1920×1080）；同时必须检查 1920×1080。检查项：无 clipping / 无 overlap / 无 unreadable microtext / 无 inaccessible button / 无 tooltip offscreen / 无 action bar collision。

## §6 Phase-1 实现真值（S5U-WO-02 落地记录，2026-09-09）

**范围**：本节记录 WF-1 战斗 HUD（S-01..S-07）的实际实现真值；其余表层（S-08..S-23）尚未换装，线框与 token 继续生效为待实现目标。

### 6.1 战斗底栏已实现结构（=WF-1 首轮落地）
- 布局单一来源：`SliceHud.CombatBarRects(dw,dh)`（公开纯函数，测试锚点 `S5UHudTests`）——LIFE 球128² ─ QWE 槽各96² ─ MANA 球128² ─ tray 354×94，总宽960 居中，栏高144，底缘8px 边距；2560×1440 与 1920×1080 同设计空间（scale=1.3333/1.0），布局逐字节一致（无夹取）。
- 双球：`SliceHudIcons.GlobeFrame`（256² 暗铁环+双金圈+8铆钉）+ 球心紧凑文本 `SliceHudFormat.Compact`（999→`999`，1000→`1.0K`，≥1e6→`1.0M`）；hover=精确整数值 tooltip。
- 技能槽：`SliceHudIcons.GlyphFor(skill)` 64² 原创符文（巨剑/箭/新星环）；热键角标 18×14；连接徽章 84×14 `G{组}·容{N}`（=LinkSourceLabel 同源紧凑态，真值映射有测试）；pip 行 14×14@20px（closed/empty/filled 三态=旧 DrawSocket 语义 1:1，`PipFor` 恒等映射测试锁死禁 Socket Color）；弱化名注（11px dim）。
- Tooltip：框悬停=连接真值卡（连接源/辅助清单/操作提示三行 Body）；pip 悬停=SupportCard（不与框卡互顶——同优先级后写胜出已显式排除）；球悬停=精确数值。
- 顶部：标题/数据行间加 `SliceHudIcons.Separator`（256×6 金饰线+菱形）。
- 导航/tray：沿用 S5 石底金边件（本轮未重绘，WO-03+ 范围）。

### 6.2 Token 落地状态（Phase 1 部分）
- **已锁**（战斗 HUD 子集）：Slot 状态四态沿用 SliceSkin 程序化石质（Normal/Hover/Press/Selected）；Health/Mana=SlicePalette.Life/Mana；文字三阶=TextCream/Dim；金饰=做旧铜+亮金双圈。
- **未锁**（WO-03+ 表层）：完整 RGB 值仍以「家族语义」为契约；Equipment/Build/Craft/Map/Passive 换装时逐表锁定并回填本表。

### 6.3 编辑器验证通道（S5U 基础设施，非产品功能）
- **Canonical GameView 通道**：GameViewSizes 反射注册 `GAME-ZZZ-CANON` 2560×1440 FixedResolution → `m_SelectedSizes[0]`+`m_TargetSize` SerializedObject 设定 → Screen=2560×1440、DesignScale=1.3333、dw=1920 无夹取（编辑器重启后需重注册；已验证可重复建立）。
- **DebugHover 通道**：`SliceHud.DebugHoverAt(设计空间点)/DebugHoverOff()`——指针钉扎截图通道，正常输入零影响；hover 点必须由运行时 dw 动态换算（禁止硬编码 1920 空间点）。
- **capture_game_view**：`--source screen --width/--height`；存至 `Assets/TempShots/` 后须移出 Assets 并删除 .meta。

## §7 WO-03 实现真值（装备/背包呈现，2026-09-09）

- **抽屉壳**：`SliceDrawerLayout.Shell 系`（Shell/Header/ShellTab/ShellSlot/ShellInvView/ShellInvCell/ShellInvContentHeight/ShellFooter）=纯函数单一来源；壳 312 宽、底缘 dh-160（战斗栏顶 dh-152 上方 8px）；三设计空间（1920×1080/1.4 夹取/1280×720）不重叠不出界有测试（`S5UDrawerTests`）。
- **装备槽符文**：EqWeapon/EqHelmet/EqBody/EqGloves/EqBoots/EqBelt（64² 原创合成；generic 槽位类型；恒等映射 `EquipGlyph(slot)`；互异+幂等测试）。
- **呈现网格**：92×70×3 列；1 物品=1 恰一格；顺序=InventoryCount 真值；零网格机制（内容高=ceil(count/3)×行高，测试锁定）。
- **Tooltip 层级**：金分隔线×2（标题块|正文|对比区）为渲染层视觉（模型零改动）；cell 悬停坐标换算修复（内容→设计空间；旧列表缺陷一并修正）。
- **Compact 升位**：K/M/T 段舍入达上段阈值=升位（999999→1.0M；禁 1000.0K/1000.0M）——规划 AI WO-02 Gate 合同修正项，随 WO-03 落地。

---

## 修订（2026-09-10，Director Input 无人值守批次）

| 项 | 修订 |
|---|---|
| 背包窗口（原右侧抽屉壳） | **D2 式居中独立弹窗**（导演参考图驱动）：上=装备纸娃娃三列（武器高槽 200×240 / 头盔 92 / 胸甲 108 / 腰带 74 / 手套 92 / 靴子 108），下=8 列满幅格子网格（78×70，5 行可见，滚动溢出）；几何单一来源不变（SliceDrawerLayout.Shell*，几何测试全量更新重跑） |
| 窗框 | Aria FrameGold 描金九宫（NineSlice b=12；缺失→程序化 PanelBg 独立成立） |
| 生命/法力球 | Aria FrameRoundGold 金环覆层叠加程序化球体之上（缺失→程序化球框独立成立）；截图实测=铆钉金环包络红/蓝球，风格成立 |
| 装备槽图标 | Aria 128² 白模板（Sword/Helmet/ChestArmor/Gloves/Boots/Belt），绘制端缩放+GUI.color 染色；程序化 64² 符文保留为回退 |
| 许可基础 | Director Attestation 2026-09-10（本机素材包全部已授权）；准入台账 S5U_ASSET_ADMISSION §7/§8 |
| 验收 | EditMode 350/350 + PlayMode 12/12（含 S5UAriaVisualPlayModeTests 截图锚）；canonical 截图 `screenshots/09_bag_modal_aria_1440.png` |
### 修订 2（2026-09-10，Director 指令"多件同类装备并排"）

| 项 | 修订 |
|---|---|
| 背包格子渲染 | **D2 图标优先格**：稀有度染底（Rare=暗金雾/Ordinary=中性极弱）+ 同色 1px 内框 + 居中 44² 槽位类型图标；名称/词缀摘要从格内移除，唯一承载=悬停 ItemCard tooltip（既有）；已装备=右上 37×14 角标（三字不换行） |
| 多件同类辨识 | 格底/框色=稀有度第一辨识层，居中图标=槽位类型第二辨识层；同类并排（实测 3 把剑/3 件胸甲同屏）按列分布可逐一辨认 |
| 拥挤演示 | PlayMode 截图锚注入 22 件（每槽 3-4 件、混合稀有度、2 件已装备）：`screenshots/10_bag_modal_crowded_1440.png` |
| 附带修复 | `EnemyVisualFeedbackPlayModeTests.FireLion_*` 泄漏 ArenaDirector（未 Destroy）→ 滞留场景双 HUD 叠画污染后续截图；已补 Destroy。S5UAriaVisualPlayModeTests 加"场景恰 1 个 ArenaDirector"防回归断言 |