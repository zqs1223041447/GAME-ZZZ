# S5U_ASSET_ADMISSION — 资产需求矩阵 + 本机素材审计 + 外部策略 + IP 契约（WO-01/G+H+I+J+K）

**周期**：S5U — Interface Presentation & HUD Renewal
**准入合同**：每个 production UI asset 必须登记完整 provenance（Asset Name / Source / Author·Provider / License / License Evidence / Acquisition Date / Original Package / Unity Import Path / Modified / Purpose / Native Resolution / Alpha / Nine-Slice Border / Status）。**Status 只能 = ADMITTED / REJECTED / QUARANTINED；license 未知 → 只能 QUARANTINED。不能因「就在本机」推定可商用。**

## §1 资产需求矩阵（G — Asset Requirement Matrix）

| # | 资产槽 | Needed | Candidate Source | Fallback | License Status | 实现阶段 |
|---|---|---|---|---|---|---|
| 1 | panel 9-slice（面板石底框） | YES | 原创程序化石纹升级（SliceSkin 延伸）+ 原创绘制 | Kenney CC0 结构件改色 | CLEAN（原创）/ CC0 | Phase 1 |
| 2 | ornamental corner（角花） | YES | 原创绘制 | 程序化 rune 笔刷 | CLEAN（原创） | Phase 1 |
| 3 | resource globe frame（生命/法力球外环） | YES | 原创绘制（D3 式 silhouette 参考） | 程序化圆环 | CLEAN（原创） | Phase 2 |
| 4 | resource globe mask/fill | YES | 原创绘制（液体/血/奥术两套） | 程序化渐变 | CLEAN（原创） | Phase 2 |
| 5 | skill frame（技能格框） | YES | 原创绘制 | Kenney CC0 框改色 | CLEAN（原创）/ CC0 | Phase 2 |
| 6 | support/link pip（G0/G1 徽标） | YES | 原创绘制（rune pip） | 文字徽章 | CLEAN（原创） | Phase 2 |
| 7 | inventory cell（背包格） | YES | 原创绘制 | Kenney CC0 cell | CLEAN（原创）/ CC0 | Phase 3 |
| 8 | equipment slot（六槽框） | YES | 原创绘制 | Kenney CC0 slot | CLEAN（原创）/ CC0 | Phase 3 |
| 9 | tab（页签） | YES | 原创绘制 | Kenney CC0 tab | CLEAN（原创）/ CC0 | Phase 3 |
| 10 | button ×4（normal/hover/selected/disabled） | YES | 原创绘制 4 态 | Kenney CC0 button 4 态 | CLEAN（原创）/ CC0 | Phase 1 |
| 11 | separator（分隔纹） | YES | 原创绘制 | 程序化线条 | CLEAN（原创） | Phase 1 |
| 12 | tooltip frame（tooltip 框） | YES | 原创绘制（PanelRaised 变体） | panel 9-slice 复用 | CLEAN（原创） | Phase 1 |
| 13 | navigation icon family（角色/地图/制作/关闭） | YES | 原创绘制 3-4 枚 | Kenney CC0 图标 | CLEAN（原创）/ CC0 | Phase 2 |
| 14 | utility icons（稳定/收益/废料/蚀刻剂/天赋点） | YES | 原创绘制 5 枚 | Kenney CC0 资源图标 | CLEAN（原创）/ CC0 | Phase 2 |
| 15 | skill icon ×3（斩击/弹道/技圈） | YES | 原创绘制 | 几何剪影 | CLEAN（原创） | Phase 2 |
| 16 | support gem icon ×7 | YES | 原创绘制 7 枚 | 色块 gem 剪影 | CLEAN（原创） | Phase 2 |
| 17 | life/mana 数值字体压缩规则 | YES | 纯代码（万进制格式化） | — | CLEAN（代码） | Phase 2 |

**本机 HD Common Icon Pack 不在任何生产槽位的 Candidate Source 里**（§2 QUARANTINED）；Kenney 只作 license-clean fallback/primitive source，不是最终暗黑视觉方向（不得让游戏变成 Kenney 默认风格）。

## §2 本机 HD Common Icon Pack 1.2 审计（H）

| 项 | 实测事实（2026-09-09 复核） |
|---|---|
| 来源包名 | `21款游戏PNG透明背景GUI图标Unity游戏图形资源`（rar，人人素材分发页 www.rrcg.cn / www.rr-sc.com） |
| Archive Contents | 1 个图标包（名为「21组图标」实为 1 组）：HD Common Icon Pack 1.2 |
| Actual PNG Count | **23 张**（22 张图标 + 1 张 .icon.png；另有各图标 preview 缩略） |
| Dimensions | **128px 级**（128×67 至 128×128 不等，非正方形） |
| Alpha | 有（PNG 透明底） |
| Visual Style | 通用写实物料图标（法力/金币/宝箱/钥匙/卷轴/火/水等 generic resource icons），**非暗黑 ARPG 风格，分辨率过低（128px），无边框/九宫格组件** |
| License Metadata | 包内仅 `免责声明.txt`（GBK 原文）：「本资源仅供个人学习 仅供学习研究之用，不得用于商业用途，请24小时内删除，版权归原创者或相关公司所有，如果你喜欢该资源，请购买正版支持」 |
| Source Metadata | 无作者署名、无原始出处页快照、无 EULA 副本；分发站为人人素材（二传站） |
| Recommended Uses | **无**（不得用于任何生产用途） |
| Rejected Uses | 全部生产用途（含参考裁切进游戏） |
| **Admission State** | **QUARANTINED — DO NOT IMPORT INTO PRODUCTION**（学习/研究免责声明 + 不可再分发/商用 + 作者链断裂；不能因「就在本机」推定可商用） |
| 处置 | 解压副本仅存于 `素材初筛，等待验收\04_UI与工具\_解压_21组图标\`（仓库外/不进 Assets）；不 import、不裁切、不溯源复用 |

## §3 其它本机 kit 审计（I）

| 包 | Identity | License located? | Relevant UI content? | 处置 |
|---|---|---|---|---|
| ORK Okashi RPG Kit 源码 1.2.5 | unity3d 游戏源码 分发包（人人素材系） | NO（无 license 文件） | 有完整 UI 框架代码 | **REFERENCE ONLY**；Production use=NOT AUTHORIZED；**禁止整包源码合入 runtime** |
| Action Game Starter Kit | unity3d 游戏源码 分发包 | NO | 有移动端 UI 资产 | **REFERENCE ONLY**；同上 |
| Dungeon Breaker Starter Kit | unity3d 游戏源码 分发包 | NO | 有 RPG UI/图标 | **REFERENCE ONLY**；同上 |

「参考」边界：仅允许**看**（布局思路/交互模式/组件清单），禁止复制贴入任何代码/图集/切片。

## §4 外部资产策略（J）

| 层 | 策略 |
|---|---|
| **Primary** | **GAME-ZZZ 原创美术**（§1 矩阵全部槽位的首选；原创绘制 + 程序化石纹延伸；asset provenance 全登记） |
| **Fallback** | 已验证 CC0——Kenney 官方 UI Pack / RPG Expansion（官方明确标示 CC0）：placeholder / utility icon / structural component / rapid prototype 用；不得决定最终风格 |
| Optional | Unity Asset Store 付费候选清单（S5U 内允许**建立候选清单**） |
| **付费采购** | **DIRECTOR INPUT REQUIRED — PAID ASSET PURCHASE**：工作 AI 不得假定导演愿意付费；每一付费候选须逐项 license review（Unity Standard EULA / Non-standard / Restricted 逐个核对；许可资产按条款嵌入 Licensed Product，不得再分发原始素材） |
| 无人值守原则 | 采购未获批**不阻塞** S5U：没有付费输入就继续 Original + verified local（无）+ verified CC0 路径 |

**本工作令**：不购买素材（无采购动作）；无 Director 采购输入即推进原创+CC0。

## §5 IP / 版权契约（K）

| 项 | 裁定 |
|---|---|
| PoE / Diablo III | **REFERENCE ONLY**——只可参考：layout / hierarchy / silhouette / information density / interaction pattern / dark-fantasy visual principles |
| Screenshot crop（截 HUD 图裁用） | **FORBIDDEN** |
| Asset extraction（rip 图标/框体/globe/字体） | **FORBIDDEN** |
| Logo copy | **FORBIDDEN** |
| Icon copy（直接复制图标构图） | **FORBIDDEN** |
| Frame tracing（描框） | **FORBIDDEN** |
| near-pixel-perfect reconstruction | **FORBIDDEN** |
| Proprietary-font extraction | **FORBIDDEN** |

## §6 Provenance 台账

**当前 ADMITTED production UI assets：15 项（WO-02 登记 9 + WO-03 登记 6，全部=GAME-ZZZ ORIGINAL 运行时程序化合成）。**

| # | 资产 | 用途（表层） | 源 | 依据 | 存放 |
|---|---|---|---|---|---|
| 1 | GlobeFrame（球环 256²） | S-03/S-04 双球框 | GAME-ZZZ 原创 | SliceHudIcons 运行时合成（SliceSkin 同范式：一次合成静态缓存；确定性整数哈希噪声）；零 Resources 契约=无 import 无裁切 | 运行时合成；凭证渲染 `docs/reviews/S5U/asset-source/globe_frame.png` |
| 2 | SlotFrame（槽框 128² 九宫格） | S-05 技能槽框 | GAME-ZZZ 原创 | 同上 | 凭证 `asset-source/skill_slot_frame.png` |
| 3 | GlyphMelee（64²） | S-05 Q 符文 | GAME-ZZZ 原创 | 同上（斜置巨剑+横护手+柄尾） | 凭证 `asset-source/skill_melee_glyph.png` |
| 4 | GlyphProjectile（64²） | S-05 W 符文 | GAME-ZZZ 原创 | 同上（金镞箭+尾羽） | 凭证 `asset-source/skill_projectile_glyph.png` |
| 5 | GlyphArea（64²） | S-05 E 符文 | GAME-ZZZ 原创 | 同上（新星环+八芒+金核） | 凭证 `asset-source/skill_area_glyph.png` |
| 6 | PipOn（20²） | S-06 支持孔=填充态 | GAME-ZZZ 原创 | 同上（中性金属，恒等映射） | 凭证 `asset-source/support_pip_on.png` |
| 7 | PipOff（20²） | S-06 支持孔=空态 | GAME-ZZZ 原创 | 同上 | 凭证 `asset-source/support_pip_off.png` |
| 8 | PipClosed（20²） | S-06 支持孔=封闭态（容量外） | GAME-ZZZ 原创 | 同上 | 凭证 `asset-source/support_pip_closed.png` |
| 9 | Separator（256×6） | S-01 顶部标题/数据饰线 | GAME-ZZZ 原创 | 同上（金线+菱形节点） | 凭证 `asset-source/separator.png` |
| 10 | EqWeapon（64²） | S-09 武器槽/网格物品符文 | GAME-ZZZ 原创 | WO-03 登记同范式（立式巨剑+金铆；generic 槽位类型，不伪装具体物品） | 凭证 `asset-source/equip_weapon_glyph.png` |
| 11 | EqHelmet（64²） | S-09 头盔槽/网格 | GAME-ZZZ 原创 | 同上（圆顶+帽檐+护鼻） | 凭证 `asset-source/equip_helmet_glyph.png` |
| 12 | EqBody（64²） | S-09 胸甲槽/网格 | GAME-ZZZ 原创 | 同上（甲身+中脊金线） | 凭证 `asset-source/equip_body_glyph.png` |
| 13 | EqGloves（64²） | S-09 手套槽/网格 | GAME-ZZZ 原创 | 同上（掌+指+护腕） | 凭证 `asset-source/equip_gloves_glyph.png` |
| 14 | EqBoots（64²） | S-09 靴子槽/网格 | GAME-ZZZ 原创 | 同上（靴筒+厚底） | 凭证 `asset-source/equip_boots_glyph.png` |
| 15 | EqBelt（64²） | S-09 腰带槽/网格 | GAME-ZZZ 原创 | 同上（横带+金扣） | 凭证 `asset-source/equip_belt_glyph.png` |

**台账约束重申**：PNG 凭证仅作 provenance 记录，**不进入 Assets/ 不 import**；运行时唯一真相=`SliceHudIcons` 程序化合成（改样式=改代码=改凭证，单向同步）。审计断言 `PipFor 恒等映射` 禁止出现任何 Socket Color 语义回归（S5 负向条件）。

---

### 历史预登记（WO-01，仍生效）
CC0 fallback 预登记（未下载、未 import）：Kenney UI Pack（kenney.nl，官方页标示 CC0）——若后续 Phase 需要 placeholder 结构件，下载后按准入合同补全台账（含 Acquisition Date + License Evidence 截图/链接）后再 import。本轮 WO-02 未使用任何外部素材（原创合成全部覆盖战斗 HUD 需求）。
