# 精模素材初筛报告 R1（Phase 5 前置 · 供导演/规划 AI 决策）

日期：2026-09-08。来源：`G:\GAME-ZZZ\素材初筛，等待验收\`（导演指示：从该文件夹找可用素材，拿不准的问 GPT）。基线：Phase 5 范围锁死=**玩家 / 核心技能 / Boss / 代表怪 / 关键地标**（`S3_PLAN.md` 阶段 8）；现有玩家=DarkKnight（BDO 包，Humanoid 重定向，已入库在用）。

> **导演裁定更新（2026-09-08）**：素材**许可证问题已由导演解决，不再作为 gate**。规划 AI 同日给出的风格/顺序裁定（Dragon Warlord Bruce 优先候选审查、POLYGON 不批作正式最终同框风格仅可内部 placeholder、4.8GB 包暂缓解压、21 组图标包暂不接入、critterpack 仅记录）仍然有效——那些是产品/风格决策，不是许可 gate。后续素材入库按导演已解决的许可授权执行。

## 一、素材库现状（初筛说明+实际盘点）

- 总量 8.0 GB；四大类：01_技能特效（32 包 unitypackage）/ 02_场景环境（已拷贝+仅预览大件）/ 03_角色怪物（已拷贝+仅预览大件）/ 04_UI与工具。
- 03_角色怪物·已拷贝（可直接试装的）：Dragon Warlord Bruce（龙领主）、Lion Head Monster、巨魔×2、原始部族、狼、野生动物昆虫、critterpack 小动物、蝙蝠×2（RoamingBats）、女性战士角色与装备、POLYGON 系（Dungeons / Knights / Samurai / MINI Fantasy / Fantasy Horde Dwarfs / Orc Pack Bundle 等 9+ 包）、NPC 人体通用动画（42MB）。
- 03_仅预览（原件在库，未解压）：**PBR 石像鬼/骨龙/蛇战士（4.8GB，初筛说明标注「最值得优先解压验收的怪物包」）**、PBR Characters Orcs（647MB）、LIVING DEAD PACK（545MB 僵尸系）、All Star Character Collection（847MB）。
- 04_UI与工具：21 组 PNG 透明 GUI 图标（56.7MB .rar 未解压）、ActionGameStarterKit / ORK Kit / Dungeon Breaker（源码参考）。

## 二、对 Phase 5 范围的初选映射（建议，未导入）

| Phase 5 需求 | 初选候选 | 状态 | 理由 |
|---|---|---|---|
| Boss | **Dragon Warlord Bruce**（龙领主） | **已试装（Art Trial R1，2026-09-08）→ ACCEPT WITH FOLLOW-UP**：九轴=8 PASS+Style Fit CONDITIONAL（手绘低模 vs DarkKnight 暗黑写实的风格混用待导演/规划裁定）；技术全达标（Generic 44 骨/20 clips 五类齐/URP-Lit 2 材质/2.60m≈2.2×DarkKnight/1738 verts-1822 tris 极低成本）；follow-up=风格裁定+controller 过渡接线+Hit 游戏内验证+Boss 位定义。详见 `docs/reviews/s3/S3_PHASE5_ART_R1_BRUCE_REVIEW.md` | 体量小、名字直接对应 Boss 位；与「监守」精英现有位置衔接 |
| Boss 备选 | PBR 石像鬼/骨龙/蛇战士包 | 仅预览（4.8GB） | 初筛说明自荐优先验收；但需解压+导入评估成本高 |
| 代表怪 | **Lion Head Monster / 巨魔×2 / 狼 / 蝙蝠×2** | **R3+R4 已裁定并接入（2026-09-08）**：Brute→Troll_2（R3，默认视觉锚点=规则⑫）；**Ashling→Lion Head Monster 经 R4 正式接入为 Ashling（烬灵）视觉（INTEGRATED，fire_lion 贴图与火主题契合，vendor 分段帧表已从旧版 meta 二进制破解：Idle 1-100/Run 450-494/Attack 714-749/GetHit 945-1000/Die 1001-1175@60fps，五类齐零缺失）**；**狼（STANDARD WOLF）=真实 mismatch 如实记录**——FBX 仅含单 AnimationStack（Take 001），vendor 分段帧表在旧版 meta 二进制中不可恢复，Take 001 实测中段（6.5-12.5s）静止无 walk/run 内容，动画映射无法诚实建立（非技术偷懒）；TheTroll=与 Brute 角色重复不优先；蝙蝠（RoamingBats）=仅 flap 单动画，Attack/Death 无专 clip 且飞行语义需额外解释，不适配；剩余候选全部 SCREENED/CLOSED，无未决精模 | Troll_2 覆盖「近战重甲」位（Brute）、FireLion 覆盖「元素火」位（Ashling）；两席均已正式 Runtime 集成 |
| 批量普通怪 | **POLYGON Knights / Samurai / Dwarfs / Orc Pack Bundle** | 已拷贝 | 低模统一风格、可批量出普通怪（现普通怪=色块基元） |
| 玩家外观备选 | 女性战士角色与装备 | 已拷贝 | 备用；现有 DarkKnight 不动 |
| 动画 | NPC 人体通用动画（42MB） | 已拷贝 | 与 Humanoid 重定向管线（现 DarkKnight 六态）同路数，可扩怪物动作 |

## 三、不确定项（按导演指示，提请 GPT/导演裁定）

1. **PBR 石像鬼/骨龙/蛇战士（4.8GB，仅预览）**：是否现在就解压验收？解压+导入评估成本高（4.8GB），若 Boss 只选 Dragon Warlord 则可暂缓。
2. **风格混用**：POLYGON 系为低模卡通向，与现玩家 DarkKnight（写实向）同框风格差明显——普通怪用 POLYGON 是否可接受？或代表怪/Boss 只从写实包（PBR/Lion Head/巨魔）里选？
3. **许可确认（最重要）**：素材包为导演个人汇集下载（含 Synty POLYGON 等商业资产）。现有先例=DarkKnight（BDO 包）已入库；新增模型资产**是否允许入库仓库**需要导演明确授权范围（只进本地工程不入仓库 vs 入仓库）。涉及重分发风险，不能由执行 AI 自行决定。
4. **UI 图标包（21 组 PNG，56.7MB 未解压）**：本轮 UI 换皮未用图标（技能槽沿用文字+按键角标，符合方案页第一轮范围）。若要上真实技能/物品图标，需先解压并确认许可；是否列入下一轮？
5. **动物/小动物包（critterpack 等）**：当前无对应玩法位（无宠物/中立生物），建议仅记录不导入。

## 四、执行边界声明

- 本报告只盘点与建议，**未导入任何模型资产、未动 Packages/工程文件**；所有已拷贝包在工程外目录（历轮惯例不动）。
- 导入任何候选前需导演/规划 AI 明确第 3 项许可边界，然后走既有「入库检查 + 换模挂载 + 贴地验证」管线。
