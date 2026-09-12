# S5_SCOPE_LEDGER — S5 实现 scope 台账（S5-WO-01 交付物 E；一页；后续 S5 工作令必须引用）

**周期**：S5 — Build Identity & Itemization Depth（DIR-1；`S5_PLAN.md`）。

## APPROVED（导演显式批准的实现授权，恰好两项）

| 原子 | 内容 | 硬边界 | 权威合同 |
|---|---|---|---|
| BL-021.A2 | Multiple Links / 扩展 Socket-Link 模型 | MaxLinkGroupsPerEligibleItem=**2**；第三组确定性拒绝；连接资格集合不变；每组容量规则不变（容量=组孔数−1）；group 1=末尾 2 孔改挂语义；组隔离/确定性顺序；无孔色/无宝石成长/无存档。**实现进度：域核=S5-WO-02；运行时+UI 集成=S5-WO-03（共享后置校验器/装备守卫/第二连接配置条/技能行源标注/Tooltip 划分行/运行时隔离实证）** | `S5_LINK_CONTRACT.md` |
| BL-002.A1 | Bounded Affix Breadth（现有机制内新词缀） | 精确 **N=4**（上限 6）；总目录数 17→21；每条复用既有 stat/modop 语义 + 单一 applicability + 现有生成路径；PoEDB 人工溯源（无 BL-024）；无新族/无 Prefix-Suffix/无 Tier-ModGroup。**实现进度：S5-WO-04=IMPLEMENTED（迅疾/铁骨/睿智/坚韧 17→21；候选 20 权威=坚韧 Strength/Flat；铁骨 Belt 排除=GAME-ZZZ 有界适用性决策）** | `S5_AFFIX_ADMISSION.md` |

## EXPLICITLY REJECTED（导演本周期明确「不做」；作为负向验收条件贯穿 S5 全周期）

| 原子 | 内容 | 处置 |
|---|---|---|
| BL-021.A1 | Socket Color（孔颜色） | 全周期不实现；合同/测试矩阵含零颜色护栏（`S5_LINK_CONTRACT.md` §4 场景 11、§5） |
| BL-012.A1 | Gem Level / Quality（宝石等级/品质） | 全周期不实现；零 level/quality 护栏（§4 场景 12、§5） |

## NOT AUTHORIZED（其余全部现有原子，维持原状态）

BL-002.A2（Prefix/Suffix）、BL-002.A3（Tier/ModGroup/生成规则）、BL-003.*（大树/Mastery/Ascendancy）、BL-004.*（Aura/Reservation）、BL-005 Curse、BL-006 Flask、BL-007.*（Defense 族）、BL-008.*（Ailment 族）、BL-009 高级 Trigger、BL-010 Unique、BL-011 Jewel、BL-012.A2-A5（Gem 其它成长）、BL-013.*（Deep Craft）、BL-014 Craft Sim、BL-015.*（Map Tier）、BL-016.*（Encounter）、BL-017 Map Risk Sim、BL-018 内容工厂（downstream）、BL-019 Atlas、BL-020.*（技术路线锁）、BL-023.*（全量导入）、BL-024 PoEDB Pipeline（双门）、BL-025 Persistence、BL-026 Voice（GATED）、BL-027 树 UI、BL-028.*（Ring/Offhand/Amulet）、BL-029.*（新内容轴门槛）、BL-030/031/032（质量/表现/合同项）。

**Default-on-Omission Rule 全程生效**：本台账未列 APPROVED 的一切能力保持原状态；禁止从「S5 已开工」推导任何额外授权。

## Phase-5 Final Evidence Linkage（S5-WO-06；零新增授权）

Production Closure（S5-WO-06，2026-09-09）全门 PASS 候选：EditMode 314/314 / PlayMode 11/11 / Audit fresh / Affix=21 / 组 max=2 / golden 21/21 双位 / 词缀可达 4/4+铁骨 Belt 负面 / **ProdSim×3 hash exact=FNV1A64:9a4c9524d0b3e214（S5 canonical hash；S4 参照仅历史）** / canonical+Art 双性能门锁定硬件 PASS（worst p99=4.925ms(59.1%) 与 6.689ms(80.3%)）/ 视觉解析 4/4 无 fallback/clone / Drift=0 / Forbidden=PASS。**本节仅链接证据，不新增授权**——显式拒绝项（孔色/宝石等级品质）与 Default-on-Omission 全部保持。**Director Final Gate = APPROVED（2026-09-12）**（`S5_FINAL_GATE_PACKET.md` §10；正式记录=`S5_DIRECTOR_FINAL_GATE.md`）。正式 **S5=COMPLETE**。本批准不新增授权、不授权 Flask/Jewel/Ascendancy/Timeless/新 Stat 轴/战役扩张。
