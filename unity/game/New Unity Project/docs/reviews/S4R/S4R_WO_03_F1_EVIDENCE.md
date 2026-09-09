# S4R_WO_03_F1_EVIDENCE — Evidence Pack（S4R-WO-03-F1 Director-Gate Semantic Clarification）

**Follow-up**: S4R-WO-03-F1 — Director-Gate Semantic Clarification（documentation-only；规划 AI 2026-09-09 WO-03 Gate Review=ACCEPT WITH FOLLOW-UP 随附；不重开 Phase 2、不构成新 Primary Work Order）
**Revision/Commit**: A=主体提交（hash 由 STATUS「本轮 commit」回填行登记）/ B=STATUS 回填提交；基线 main @ 7c75cbe

## Changed Markdown（4 个文件）

| 文件 | 变更类型 |
|---|---|
| `docs/reviews/S4R/S4R_DIRECTION_DECISION_PACKET.md` | F1-A：对比矩阵行 + DIR-4 卡 Art dependency 字段澄清；F1-B：DIR-4 卡 Save/migration 字段重写；§3 Toggles 增 BL-025 行；§6 增授权优先级裁定 |
| `docs/DECISIONS.md` | 追加 1 条：F1 三项语义（Content Batch 解耦 / DIR-4↔BL-025=downstream / 授权优先级+原子级 Review） |
| `docs/reviews/STATUS.md` | LIMITED：本轮 commit 行 + 最近一次绿灯加行 |
| 本文件 | 新建：F1 Evidence |

（RUNTIME/COMBAT_MATH 本轮 verify 结论不变：verified; no change required——零改动。）

## F1 模板回传内容

```
Follow-up: S4R-WO-03-F1
Revision/Commit: A=（主体，hash 见 STATUS 回填行）/ B=（回填）

Decision Matrix Content-Batch Row:
Before: | Content Batch required? | NO | NO | 可选（表现配套） | NO | YES（Boss/场景资产） |
After:  | New Content Batch required by direction? | NO | NO | NO | NO | NO（任何 DIR-0~4 均不因方向本身自动要求或批准 New Content Batch——独立 YES/NO 导演门，默认 NO；DIR-4 验证内容归显式 atom/bounded validation scope；单独批准 Content Batch≠激活 Phase 12，除非显式声明） |

DIR-4 ↔ BL-025 Relationship: DOWNSTREAM SEPARATE（Option 2，真实依赖结论：首周期可在无持久化下以会话内 Tier 进程成立；BL-025=downstream / separately gated；DIR-4 selection does not authorize it；不从 Progression=YES/Endgame=YES 推导；seed 阶段需持久化必须显式列入 Required Gates 单独批准）

Director Form Changed: NO（决策表字段与 Default-on-Omission Rule 原样；仅新增 §6 授权优先级裁定注=Explicit Atom Decision > Explicit individual toggle > Selected Direction > Planner recommendation，CUSTOM/部分授权做原子级 Gate Review）

Authorization States Changed: NONE
Runtime Delta: NONE
Capability Delta: NONE
Mechanic Support Delta: NONE
Canonical Data Delta: NONE
Content Delta: NONE

Drift: 0
Source-of-Truth Conflicts: 0
Forbidden Expansion Audit: PASS（57 atoms 状态全部不变；无 prototype/Phase 0/BL-024 动作）
```

## F1 Acceptance 核对（规划 AI 十条）

1. 矩阵不再表达「DIR-4 requires New Content Batch」✓；2. New Content Batch 仍为独立 YES/NO 门 ✓；3. Phase 12/BL-018 仍 downstream ✓；4. BL-025 关系明确=downstream/separate ✓；5. 不从 Progression/Endgame=YES 推导 Persistence ✓；6. Decision Form 与 Default-on-Omission Rule 不变 ✓；7. 57 atoms 状态全部不变 ✓；8. Runtime/capability/mechanic/canonical/content delta 全 NONE ✓；9. Forbidden Expansion Audit=PASS ✓；10. Drift=0 ✓。

## Tests / Gates

- Quick Gate 复跑（当轮，exit 0=PASS）：EditMode 246/246、PlayMode 11/11、Content Audit fresh PASS。Performance 不重跑（零 runtime drift）。
