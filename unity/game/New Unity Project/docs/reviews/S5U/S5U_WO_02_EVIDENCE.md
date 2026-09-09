# S5U_WO_02_EVIDENCE — Dark ARPG Skin Foundation & Combat HUD Frame（证据包）

**周期**：S5U — Interface Presentation & HUD Renewal（导演 2026-09-09 授权：参考 PoE/暗黑3 重做 HUD/UI）
**工作令**：S5U-WO-02 — Dark ARPG Skin Foundation & Combat HUD Frame（首个 UI runtime authority 工作令）
**执行日期**：2026-09-09（无人值守模式）
**授权边界**：presentation-only（IMGUI 强制；gameplay/content/balance=NONE；Forbidden Expansion 不越界）
**HEAD 状态**：本证据所列截图与门数据全部采集于最终代码态（重编译 clean 后、编辑器重启+复验后）。

## §1 交付摘要（L1-L4 全落地）

| 层 | 交付 | 文件 |
|---|---|---|
| L1 | 紧凑数值格式化器（`Compact`：<1K 整数/<1M 一位小数 K/≥1M 一位小数 M；不变文化） | `Assets/Runtime/Core/Gameplay/SliceHudFormat.cs`（新增） |
| L2 | 原创图标合成（SliceSkin 同范式：运行时一次性合成→静态缓存；零 Resources 契约；9 件） | `Assets/Runtime/Core/Gameplay/SliceHudIcons.cs`（新增）+ 凭证 `docs/reviews/S5U/asset-source/`（9 PNG，不进 Assets） |
| L3 | 会话读路径（连接徽章紧凑真值） | `Assets/Runtime/Core/Gameplay/SliceSession.cs`（**新增公开方法 LinkBadgeText**，其余零改动） |
| L4 | 战斗 HUD 重做（双球+QWE 图标槽+pip/徽章+tray 几何单一来源） | `Assets/Runtime/Core/Gameplay/SliceHud.cs`（DrawSkillHud/DrawSkillCell/DrawSocketPip/DrawOrb 重做；`CombatBarRects` 公开纯函数=布局单一来源；其余面板零改动） |
| 测试 | 18 条新 EditMode（S5UHudTests） | `Assets/Tests/EditMode/S5UHudTests.cs`（新增） |

## §2 截图证据（7 张 canonical After，全部采集于最终 HEAD）

存放：`docs/reviews/S5U/screenshots/`（与 §1 章节同目录）。采集通道=canonical 无夹取（GameView m_TargetSize=2560×1440，Screen=2560×1440，DesignScale=1.3333，**设计空间=1920×1080**）；状态注入=确定性（ResetTown(20260909)+SeededRng(4242)+裂石巨刃重绑弹道组1+Q=燃烧/E=燃尽 支持）；悬停=DebugHover 编辑器验证通道（点由运行时 dw 动态换算）。

| # | 文件 | 状态 | 验证要点 |
|---|---|---|---|
| 1 | `after_combat_full_2560x1440.png` | 满血满蓝 Q 选中 | 双球 10.0M/10.0M+54/54 紧凑真值；Q 金框选中；G0·容1/G1·容1/G0·容1 徽章；Q/E pip=填充+封闭、W pip=空+封闭；tray 星标（燃烧*/燃尽*）+机制色条（分裂/火焰转化） |
| 2 | `after_combat_partial_2560x1440.png` | 34% 血 / 2 蓝 | 3.4M/10.0M+3/54；填充比例真实（Group 裁剪机制不变） |
| 3 | `after_combat_group1_2560x1440.png` | W 选中（组1 态） | W 金框选中；组1 二段连接视觉成立 |
| 4 | `after_tooltip_skill_q_2560x1440.png` | Q 框悬停 | 连接真值卡四行：近战 Q / 武器·组0·容1 / 辅助：燃烧 / 操作提示；下溢上翻不越屏 |
| 5 | `after_tooltip_pip_2560x1440.png` | Q 支持孔悬停 | SupportCard 完整（燃烧/增强型/+10 附加火烬/兼容 √Q√W√F/可装配到当前技能）；**修复验证：不再被框卡顶掉** |
| 6 | `after_tooltip_skill_w_2560x1440.png` | W 框悬停（组1） | 弹道 W / **裂石巨刃·组1·容1** / 无辅助 / 提示——S5 唯一连接源真值完整保留 |
| 7 | `after_combat_1920x1080.png` | 1080 兼容 | Screen=1920×1080、scale=1.0、布局 1:1 逐像素等价；无 clipping/overlap |

**对照基线**：`docs/reviews/S5U/screenshots/before_*_2560x1440.png`（WO-01 follow-up 三张，HEAD=1c4061b 视觉态）——旧 HUD=扁条+文字槽+溢出球文本；新 HUD=框体化/图标化/紧凑化，语义零丢失。

## §3 验收断言（AC 全表）

| AC | 断言 | 证据 | 结果 |
|---|---|---|---|
| AC-01 | Compact 七边界值精确（999/1K/9999/10K/999999/1M/9999999） | S5UHudTests.Compact_Boundaries_FormatsExactly | PASS |
| AC-02 | Compact 小值/负值=RoundToInt（0/54/−5） | Compact_SmallAndNegative_RoundToInt | PASS |
| AC-03 | 大生命值适配球内文本（10.0M=5 字符≤9 上限；1e5..1e11 全采样） | Compact_LargeLife_FitsOrbTextBox | PASS |
| AC-04 | 三个设计空间（1920×1080 共同空间/1.4 上夹取空间/1280×720）底栏元素不重叠、不出界、≥12 左缘 | CombatBar_ElementsNeverOverlap_AndStayInViewport（3 参数化） | PASS |
| AC-05 | 元素尺寸契约（球 128²/槽 96²/tray 354×94；2560×1440 与 1920×1080 布局逐字节一致） | CombatBar_ElementSizes_FixedContract | PASS |
| AC-06 | 连接徽章真值映射（三技能 badge=「G{组}·容{N}」与 LinkSourceLabel 逐字符同源；组1 重绑确定性锁定） | LinkBadge_MatchesLinkSourceLabel_OnEverySkill | PASS |
| AC-07 | 容量真值映射（SupportCapacity=连接源组容量，禁槽位推导；弹道=容1） | SupportCapacity_MatchesLinkSourceCapacity | PASS |
| AC-08 | 图标缓存幂等（9 件 Ensure 二次调用同引用；尺寸 256/128/64/20/6） | Icons_Ensure_IsIdempotent_SingleSynthesis | PASS |
| AC-09 | **禁 Socket Color 语义**：全 Support 目录 × 三态 PipFor 恒等映射；三态互异；封闭优先于填充 | PipFor_IdentityMapping_NoSocketColorSemantics | PASS |
| AC-10 | 符文全覆盖三技能（GlyphFor 恒等） | GlyphFor_CoversAllCastableSkills | PASS |
| AC-11 | 重编译 clean（failed=false errors=[]） | recompile_status ×4 | PASS |
| AC-12 | EditMode 全量 **332/332**（=S5 基线 314 + 新增 18；零削弱零跳过） | run_tests EditMode（本轮两次运行：329→修复断言语义后 332） | PASS |
| AC-13 | PlayMode 全量 **11/11** | run_tests PlayMode async | PASS |
| AC-14 | Audit fresh：CONTENT_PRODUCTION_REPORT.json 本轮再持久化 verdict=PASS | docs/qa 时间戳 2026-09-09 20:25 | PASS |
| AC-15 | ProdSim hash 精确不变：**FNV1A64:9a4c9524d0b3e214**（repeatHashMatch=True、invalidCount=0、10k cycles、verdict=PASS） | PRODUCTION_SIMULATION_REPORT.json（20:25 再生） | PASS |
| AC-16 | canonical After 7 张（≥3 要求超额） | §2 表 | PASS |
| AC-17 | 1080p 兼容（scale=1.0 逐像素等价） | §2 #7 | PASS |
| AC-18 | Tooltip 三态真值（框卡/pip 支持卡/组1 连接真值） | §2 #4/5/6 | PASS |
| AC-19 | 运行时几何真值：dw=1920.0 无夹取（canonical 通道） | 探针 st_geom_probe/gv_targetsize 输出 | PASS |
| AC-20 | Runtime Product Delta=presentation-only（ProdSim hash 不变=内容/数值零漂移实证） | AC-15 | PASS |
| AC-21 | Forbidden Expansion=无（未触 S5 冻结语义/未动 uGUI/Toolkit/未动 gameplay 文件除 LinkBadgeText 纯新增） | §4 Delta | PASS |

## §4 Delta 声明

- **Runtime Product Delta（呈现层，授权内）**：SliceHud 战斗底栏（DrawSkillHud/DrawSkillCell/DrawSocketPip/DrawOrb/DrawTop 饰线/EnsureStyles 两样式）；新增 SliceHudFormat/SliceHudIcons；SliceSession 新增 `LinkBadgeText`（纯只读映射）。
- **Gameplay/Content/Balance Delta=NONE**：未动 ArenaSim/CombatRules/Catalogs/RollItem/数值；ProdSim hash 与 S5 canonical 精确一致（AC-15）。
- **S5 冻结合同**：连接源唯一真值（LinkSourceLabel 全语义迁移至 tooltip 承载，文本未删）/容量/支持孔三态 1:1/拖放与点击流/Esc/R 等键位——全部保留（AC-06/07/09 + 既有 MultiLink/Support 套件全绿）。
- **Drift=0；SoT=0；Forbidden=PASS。**

## §5 环境与过程披露（如实）

1. **编辑器重启事件**：本轮曾在 Play 模式存活时启动 EditMode 测试批（Unity Test Framework 模式切换过渡态）后被终止，编辑器进入每帧 NRE 风暴（ArenaDirector.ReleaseEnemyVisual，被杀运行留下的半切换态）且命令管线死锁（editor_stop 120s 超时）→ 强杀进程重启编辑器（新 pid 45648）→ **全部门在重启后重跑**（EditMode 332/332、PlayMode 11/11、Audit、ProdSim 均为重启后数据）；仓库状态无损。过程纪律入账：**测试批禁止与存活 Play 会话混跑**。
2. **自查修复两缺陷（提交前）**：(a) pip 悬停 SupportCard 被同优先级框卡顶掉（后写胜出）→ pip 行悬停显式排除框卡；(b) 多行真值误入 TextCard 单行 Subtitle 槽被 15px 裁剪 → 显式拆入 Body[]（未改 SliceTooltipModel 冻结 API）。
3. **截图时序披露**：首批 After 截图产生于中间代码态；最终提交版 7 张**全部在最终 HEAD 重采**（§2）。
4. **门 tier 披露**：批处理 canonical 门因编辑器占用未跑（WO-01 先例）；in-editor 等效门已全过；S5U 收口轮全量重跑批处理 tier。
5. **Bootstrap→Arena 入口缺陷（导演原始抱怨之一）尚未修复**：现有桥仅 `-arenaPerf`（测量+自动退出），普通启动仍停在 Bootstrap 空场景。修复涉及启动流程（gameplay-adjacent），**未获授权不实施**——已列为 S5U-WO-03+ 候选授权点（规划 AI 裁定）。
6. **canonical 通道易失**：编辑器重启后 GAME-ZZZ-CANON 注册丢失，需按合同 §6.3 重注册（已验证可重复建立）；1080 兼容图捕获时 sel[0]=7 固定分辨率下 m_TargetSize 瞬时生效后回落 canonical（捕获本身 1920×1080 PNG 有效，随后已还原 2560×1440）。

## §6 交付物清单（本提交）

- 代码：`SliceHudFormat.cs`(+meta)、`SliceHudIcons.cs`(+meta)、`SliceSession.cs`、`SliceHud.cs`、`S5UHudTests.cs`(+meta)
- 凭证：`docs/reviews/S5U/asset-source/`（9 PNG + .meta）
- 截图：`docs/reviews/S5U/screenshots/after_*`（7 张）
- 文档：`S5U_VISUAL_CONTRACT.md`（§6 实现真值）、`S5U_ASSET_ADMISSION.md`（§6 台账 9 项）、`S5U_UI_SURFACE_MAP.md`（§4 实现真值）、`S5U_WO_02_EVIDENCE.md`（本文）、STATUS/ROADMAP/DECISIONS 同步

## §7 建议下一令（规划 AI 裁定）

- **S5U-WO-03 — Equipment & Inventory Presentation Renewal**（S-08..S-16：角色页/背包/抽屉/ItemCard 换装；复用 §4 token 家族与 SliceHudIcons 范式）。
- 授权点候选：Bootstrap→Arena 普通启动桥（修复导演「Bootstrap 开启后啥也没有」；最小方案=BootstrapRunner 校验通过后自动 TryEnterMap 或提供一键入口按钮——需要规划 AI 划定边界：启动流程是否算 presentation-adjacent）。
