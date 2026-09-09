# S4R_SOURCE_OF_TRUTH_INDEX — 真相源索引（S4R-WO-01 建立）

**用途**：任何 AI 复核「某项事实以哪个文件为准、证据在哪、当前是否一致」。冲突裁决按 BDA 计划 §6 优先级：①较新的明确 Director Decision → ②当前 SHORT_TERM_PLAN → ③DECISIONS → ④Runtime+测试证据 → ⑤STATUS/ROADMAP 摘要 → ⑥Phase Snapshot/历史 → ⑦长期规划旧执行细节。
**本轮核查结论**：发现并修复 1 处文档漂移（RUNTIME 装备节，见下表 R-04）；无第二 canonical 相互冲突证据；无语义冲突。

| # | Claim | Authoritative Source | Evidence Source | 当前状态 | Mismatch |
|---|---|---|---|---|---|
| R-01 | S4 = COMPLETE（Phase 0-5 全 COMPLETE） | STATUS 门状态表 + ROADMAP 阶段表 | `docs/reviews/s4/S4_OVERALL_CLOSEOUT_REVIEW.md`（§11b Recertification PASS） | 一致 | 无 |
| R-02 | S4 硬停止原文（不启动 S5/.../Voice） | STATUS 门状态表 S4 行 | S4_PLAN §8 | 一致（S4R 计划 §5/冻结基线 §2 同语义转写，未弱化） | 无 |
| R-03 | Voice = DEFERRED BY DIRECTOR | STATUS 门状态表 | S3/S4 收口评审 + Production Report `gatedMissing=3` | 一致 | 无 |
| R-04 | 装备 6 槽 / 17 词缀 / 抽屉 2×3 | RUNTIME.md（本轮修订后）+ Production Report | `SixSlotEquipmentTests`、`AffixApplicabilityTests`、`SliceDrawerLayout.cs`（ColumnH 326、2×3、DisplayOrder 六槽） | **修复前 RUNTIME 写「4 槽/当前 13 条/2×2 迷你槽」——S4-P2/P3 轮改变事实后 RUNTIME 未同步（文档漂移，非 runtime 缺陷）→ 本轮 S4R-WO-01 对齐** | **已修复（本单）** |
| R-05 | 3 Skills / 7 Supports | RUNTIME.md Support 节 + Production Report | `SupportGateTests`（golden 21 组合 parity OK） | 一致 | 无 |
| R-06 | Passive 16/20/0 | RUNTIME.md 天赋节 + Production Report | ProductionContentReportTests（graph 再生） | 一致 | 无 |
| R-07 | 单一 applicability truth（AllowedSlots 位掩码） | RUNTIME.md 装备节（本轮对齐后） | `AffixApplicabilityTests` + `Catalogs.cs` S4-P3 注释 | 一致 | 无 |
| R-08 | 正式敌人视觉 4 / fallback 0 / clones 0 | Capability Ledger + STATUS | Production Report `enemyVisuals` + Art Gate 证据 | 一致 | 无 |
| R-09 | 战斗 SFX 5 键 PASS / 人声 3 键 GATED 0/3 | RUNTIME.md 音频节 + STATUS 音频节 | 资源契约审计（REQUIRED 10/10；GATED 0/3 如实记录）+ `SFX_SOURCES.md`/`DK_VOICE_INVENTORY.md` | 一致 | 无 |
| R-10 | S4 Final 性能证据（canonical worst p99=4.840ms=58.1% / Art worst avg=4.085ms=49.0% 等） | STATUS 门状态表 + 最近一次绿灯表 | S4 Final Gate `-IncludeArtPerformance` @ b3eb9fe（冻结证据引用） | 一致（S4R 不重跑，不重解释） | 无 |
| R-11 | Production Simulation hash=FNV1A64:a1f075f251ec1070 | STATUS + S4_PLAN milestone 回填 | `docs/qa/PRODUCTION_SIMULATION_REPORT.json`（工具再生） | 一致 | 无 |
| R-12 | 测试基线 EditMode 246/246 + PlayMode 11/11 | STATUS 最近一次绿灯表 | 2026-09-09 S4R-WO-01 Quick Gate（exit 0，PASS 246/246+11/11+audit fresh） | 一致 | 无 |
| R-13 | 测试计数 246/11 的构成 | 各轮 STATUS 校验节（逐轮 +N 记录） | 各轮 Evidence Pack | 一致（246=246 个 EditMode 测试，无未解释增删） | 无 |
| R-14 | Capability Ledger / Mechanic Matrix 的权威位置 | **本目录**：`S4R_CAPABILITY_LEDGER.md`、`S4R_MECHANIC_MATRIX.md`（首次正式建立；此前仅 BDA §4 定义） | 同文件 | 一致（新建立，无旧副本冲突） | 无 |
| R-15 | 规划 AI 会话渠道 | `开发计划/规划AI会话.md`（唯一登记） | gpt-web 注册表 `%LOCALAPPDATA%\chatgpt-web-debug\sessions.json` topic=game-zzz-planning | 一致 | 无 |
| R-16 | 1440p/120 CLOSED（锁定硬件语义） | STATUS 测量节 + ROADMAP S2P 行 | `docs/reviews/s2p/1440p-120-m7/`（冻结证据）+ contract | 一致 | 无 |
| R-17 | PoEDB = 唯一 canonical 外部来源 | S4R_PLAN §5 + BDA §5 | —（流水线未建立为当前事实，见 Ledger） | 一致 | 无 |
| R-18 | COMBAT_MATH 公式现状 | `docs/COMBAT_MATH.md` | `CombatMathTests` golden 向量 | 本轮 VERIFY：一致，no change required | 无 |

## 冲突裁决适用例（本轮实际执行）

- RUNTIME 装备节的「当前 13 条」类易变计数违反 S3-M1 长期规则（RUNTIME 避免复制易漂移数字）→ 修复方式=改写为契约描述 + 指向最新 Content Audit/Production Report 快照，而不是在 RUNTIME 里硬编码 17。历史 S3-M1 条目与 S4 事实无语义冲突，按「历史快照不现代化」保留原文。
