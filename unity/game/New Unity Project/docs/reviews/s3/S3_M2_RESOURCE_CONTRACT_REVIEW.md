# S3_M2_RESOURCE_CONTRACT_REVIEW（S3 维护轮 M2 独立复核）

日期：2026-09-07。工作令：`S3-M2-RESOURCE-CONTRACT-TRUTH`。Phase R（A15 Reviewer）：以真实 diff 复核。Baseline HEAD=38a1118。

## Runtime resource callsites（Phase A 重扫）

全 Runtime `Resources.Load` 调用点恰 3 处（rg 复核，无漏项）：`AudioEvents.ClipFor`（战斗 SFX）、`VoiceCues.ClipFor`（人声）、`DarkKnightView.TryMount`（玩家预制体）。

## Before duplication model → After single-source model

- Before：路径拼接 3 处独立字面量（`"Audio/"+name`、`"Audio/Voice/"+key`、`"Player/DarkKnight"`）+ key 集合在查找表初始化里重复一份（Runtime 内部双份真相）+ 测试侧契约手写完整路径字符串。
- After：`RuntimeResourcePaths`（纯静态：常量+拼接，无管理器/缓存/Addressables）单一真相源；三消费点全部经它取路径；AudioEvents/VoiceCues 查找表由各自 `DeclaredKeys` 单一键源初始化；测试侧契约经同一 builder 拼 key（共享「如何加载」）。

## Runtime declared key sets / Audit expected sets

- AudioEvents.DeclaredKeys = Cast/Impact/Hit/Death/Loot（只读）↔ Audit golden 期望（人工钉死）同集合 → parity ✓（5/5）。
- VoiceCues.DeclaredKeys = Cast/Hit/Death（只读）↔ Audit golden 期望同集合 → parity ✓（3/3）。
- Player：恰好 1 条 REQUIRED 契约（canonical key=Player/DarkKnight）✓。
- parity 测试逻辑由「当前键集对拍」驱动，不硬编码 5/3 计数为唯一判据。

## Audit independence（Reviewer B）

- Audit 期望键集（`ExpectedSfxKeys`/`ExpectedVoiceKeys`/契约条目 LogicalKey）全部人工声明，**未**从 AudioEvents/VoiceCues.DeclaredKeys 自动生成；REQUIRED/GATED 分类与 Gate 理由仍独立。
- 场景推演（架构成立，未改正式 Runtime 验证）：VoiceCues 私加 Taunt → parity 红（Audit 未批准）；删除 Loot → parity 红；`Audio/` 误改 `Sounds/` → REQUIRED 真实加载红。三个方向都有失败路径。

## Negative validator proof（Reviewer synthetic）

`CoverageValidator_NegativeCases_AreRejected`：missing（Runtime {Cast,Taunt} vs Audit {Cast}）红；extra（Runtime {Cast} vs Audit {Cast,Loot}）红；duplicate（双侧重复）红；正例对照绿。

## Required / Gated load result

- REQUIRED：DarkKnight + 5 SFX 真实加载 **6/6 PASS**（AssetDatabase 真实路径）——真实验证保留，未因共享 path builder 削弱。
- GATED：人声 3 键 0/3 present（GATED-MISSING，缺失不失败，如实报告）。
- VFX：声明引用 0 → N/A。

## S3_PLAN drift findings / fixes（Reviewer F：新 AI 视角通读）

- 发现：页首「规划，非开工令/内容扩张整体仍关」、阶段 1「状态：现在做」、阶段 2「等导演开 S3 口令/R2 另需点名」、第一批允许清单无历史标注——历史时态冒充当前状态。
- 修正：页首加当前状态（Phase 1/2=COMPLETE、3-5=GATED、新内容不得自行开始、S3 未结束）并标注「开 S3 口令」为历史门规；阶段 1/2 内联旧状态改为「原始依赖（历史）——已执行」；第一批允许清单标注「历史开工范围 · 已执行」。
- 新 AI 通读五问：Phase 1 完了吗=是；Phase 2 完了吗=是；R2 还在等方向吗=否；可自行做 Phase 3 吗=否；可自行做新内容批次吗=否。全部无歧义。

## Runtime behavior diff（Reviewer E）

- 三消费点加载相同 key/相同类型/相同资源；冷却、fallback（静音/胶囊）、日志、gameplay 零变化（diff 复核：仅路径拼接与键源初始化方式；CombatMath/SliceSession/ArenaSim/SkillCaster/ProjectilePool/DummyCrowd/兼容门零触碰）。
- DarkKnight 旧公开常量 `ResourcesNameDarkKnight` 保留为 alias 指向 canonical（无第二份 literal）。

## Content delta

全轴 0（护栏断言验证；sentinel 不计）。

## Remaining risks

- 低：Runtime 新增资源入口时需同步测试侧 golden（parity 红提示，防未审批增删——这是设计目标而非缺陷）。
- 低：`Name(AudioEventId)` 的键映射与 DeclaredSfxKeys 仍为两处文本（一个 enum→键 switch、一个键数组）——语义不同（事件 id→键 vs 声明键集），未合并；如未来增加事件需两处同步（parity 测试会抓漏）。

## Verdict

**PASS**
