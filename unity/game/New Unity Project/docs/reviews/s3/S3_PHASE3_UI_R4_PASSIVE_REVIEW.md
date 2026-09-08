# S3-PHASE3-UI-R4-PASSIVE-BOUNDARY 复核与收口报告

工作令：S3-P3-UI-R4-PASSIVE-BOUNDARY-CLOSEOUT（规划 AI/GPT 协调席 2026-09-08 下发）。Owner：I=实现 / R=复核（同一 AI 分轮担任）。日期：2026-09-08。Baseline=a65267c（R10 Phase 5 Art COMPLETE HEAD）。

## 0. Verdict

**PASS — R4 EXISTING PASSIVE UI REORDER COMPLETE**（§42 Branch A：只重排现有 Passive 展示，零 gameplay delta，canonical Links 驱动零 synthetic edge；Phase 3 Formal UI = COMPLETE）

## 1. Baseline HEAD

a65267c（R10 Phase 5 Art COMPLETE HEAD，已 push）。

## 2. Passive Truth Audit（§3/§4，代码当前真相）

| Item | Current Truth |
|---|---|
| Passive count | **16**（PassiveCatalog） |
| Existing IDs | 0-15（心脉/蛮力/敏足/灵思/铁骨/影蔽/血脉/粉碎/余烬/精准/冲击/残暴打击/火葬/烬心/厚皮/搏动） |
| Unlock mechanism | `SliceSession.TryAllocate(i)` canonical 写路径（金/已点/可点/锁住 三态） |
| Prerequisites | Links[] 数组（canonical 语义边）——真实 graph edge 存在 |
| Graph edges | Links 数组（**有 canonical edge**） |
| Currency/cost | 已点金（TryAllocate 内部判定，无独立 currency system） |
| Respec | `TryRespec` 已存在（canonical） |
| Gameplay mutation path | TryAllocate→RecalcPlayer（唯一写路径，R4 未触碰） |
| Existing UI form | SliceHud.DrawTree（IMGUI：NodeCenter/NodeRect 内联计算+Links 画线+三态颜色+Tooltip+Click） |

**审计结论：系统已有真实 16 节点小型图+Links 语义边+canonical 三态+Tooltip**——属于 Branch A（§6）。

## 3. Branch Decision（§5）

- Branch A：当前系统已有真实小型 16 节点图+Links 语义边+canonical TryAllocate 路径。可**只重排展示**完成（不依赖任何 Stage0 禁止能力）。
- Branch B：不触发（无需新 Passive/新 edge/新 respec/新大型树系统）。

## 4. Implemented Layout（§22-§27，Branch A）

- 新增 `SlicePassiveLayout`（纯 presentation 静态类，零 gameplay 状态/写 API）：Normalized 16 位置（手工设计不重叠）+NodeRect（普通 56×26/Notable·Mechanic 72×34）+NodeCenter+GetEdges（仅 canonical Links，零 synthetic edge，§10/§11 合规）。
- SliceHud.DrawTree 改用 SlicePassiveLayout（替代内联 TreePos 计算）；Tooltip 仍走 SliceTooltipModel（§15）；Click 仍走 TryAllocate canonical（§13/§14 合规）；ShouldBlockWorld 仍覆盖 panel 内区域（§27）。
- 命名：使用当前项目名称（天赋/心脉/蛮力等，§12 合规——未强加「天赋树」）。
- 视觉语言：沿用 R1-R3 已有正式 UI（dark stone/Rare Gold/ivory text/existing hover/click feedback，§17 合规）。
- 布局基线：1920×1080 design space（§19 合规）；R1-R3 已锁项未动（§20 合规——Life/Mana orbs/bottom bar/QWE/tray/drawer/Tooltip 零重排）。
- Passive 容器：沿用已有 panel 内区域（§21 合规——未创造新全屏 gameplay mode）。
- 状态样式：仅使用 canonical 已有 Allocated/Available/Locked 三态（§25 合规——未发明第三种 gameplay 状态）。

## 5. Skipped Reasons（§7/§8，Branch B 不适用）

无需 Branch B——系统已有真实小型树+Links 边+TryAllocate。不存在「需要 Stage0 禁止能力才能成立」的情况。

## 6. Canonical Mutation Path（§13/§14）

TryAllocate（未变）→ RecalcPlayer（未变）→ StatBag AddAll（未变）。零新写路径、零 UI 判断复制（§14 合规）。

## 7. Tooltip Reuse（§15/§16）

SliceTooltipModel.TextCard(n.Name, n.Desc)——R3 已验收统一 tooltip 路径；仅显示当前已有 name/desc（§16 合规——无 hypothetical upgrade/next level/path bonus）。

## 8. Tests（§32）

- EditMode **+10（199→209）**（`SlicePassiveLayoutTests`）：PassiveCount unchanged/IDs unchanged/rects non-overlap/rects within panel/deterministic/1080p valid/1440p scale valid/only canonical Links edges（零 synthetic edge）/layout pure function no gameplay state/Notable distinct size。
- PlayMode：**0 new**（§33：现有 CatalogPlayModeTests 已覆盖三 kind 同图+death truth——无真实 coverage hole）。

## 9. Gates（§38-§39）

- SelfTest：**PASS**。
- Player Runtime Gate：**PASS**——EditMode **209/209** + PlayMode **9/9** + Audit fresh + Build PASS + PlayerRun exit=0。
- Performance：不要求（§40 合规——本轮仅 UI presentation，零 Runtime/ProjectSettings/rendering/hot gameplay loop 改动）。

## 10. Scope（§45-§48）

- Passive delta=0 / StatId delta=0 / Modifier delta=0 / Condition delta=0 / Effect delta=0 / Trigger delta=0 / PassiveId delta=0 / Gameplay delta=0 / Content delta=0 / Art delta=0 / Audio delta=0 / Voice delta=0 / Phase 5 delta=0 / ProjectSettings delta=0。
- R1-R3 已锁项（orbs/bottom bar/QWE/tray/drawer/Tooltip）零重排（§20 合规）。

## 11. Phase 状态（§44-§48）

- **Phase 3 Formal UI = COMPLETE**（R1 COMPLETE/R2 COMPLETE/R3 COMPLETE/**R4 COMPLETE — bounded existing-passive presentation only**；无其它 UI blocker）。
- 含义：formal bottom HUD/dual orbs/skill-support bar/equipment drawer/Build-Craft migration/unified Tooltip/existing Passive UI handling 已收口。**不表示**大天赋树完成/inventory overhaul/map UI/atlas/character customization/future UI 永久冻结。
- Phase 4 Voice：GATED（导演指令继续）。
- Phase 5 Art：COMPLETE（R10 收口，零 diff 本轮）。
- **S3 overall 仍 NOT COMPLETE**（Phase 4 Voice 正式排除+S3 overall milestone 需要 S3-CLOSEOUT 工作令单独审计）。

## 12. Stage0 禁止项（§49）

全部继续锁：big passive tree / Unique library / Atlas / deep Craft / DOTS / HDRP / FMOD / character customization。

## 13. Reviewer Findings / Fixes

- Findings：①Branch A 正确选定——现有系统已有真实 16 节点小型图+Links 语义边+TryAllocate canonical 路径，属于「只重排展示」范围；②SlicePassiveLayout 提取后 TreePos 旧数组废弃（DrawTree 直接消费 SlicePassiveLayout 纯函数）；③初次实现非重叠测试捕获 node 12/15 重叠——修 Normalized 坐标后通过（**测试先行实际捕获布局缺陷，非形式断言**）。
- Fixes：如上，无遗留。
- Final verdict：**PASS — R4 EXISTING PASSIVE UI REORDER COMPLETE**。

## 14. 提交（§53 Branch A）

- feat(ui): reorder existing passive presentation → test(ui): cover bounded passive layout contract → docs(ui): close formal ui phase；普通 push 禁 force。
