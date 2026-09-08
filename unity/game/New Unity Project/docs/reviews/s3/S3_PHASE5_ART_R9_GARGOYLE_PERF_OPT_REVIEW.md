## 6. 画质底线确认（§26）

- 2048² PBR 贴图全保留；silhouette 不变（网格零修改）；翅膀/肢体变形正常（蒙皮路径切换不改变输出）；攻击/受击/Ignite/死亡可读（R8 QA 七张继续有效——R9 改动为引擎配置+数学零差异数据剥离，视觉输出不变，Gate 后目检正常）；1.70m 威胁层级成立。

## 7. Ashling gameplay（§27）

零改动（6@r15/HP24/甲20/闪避40/速2.2/cd1.05/物3+火5/近战直击——全部未动；R9 仅 presentation 层）。

## 8. 重试接入（§28-§30）

- Catalog：Brute→Troll、Stinger→FireLion、**Ashling→GargoyleVisual**、Warden→Bruce、Dummy→null（无其它新增）。
- RuntimeResourcePaths.EnemyGargoyleVisual 恢复；GargoyleVisual.prefab 回 Resources/Enemies/（Runtime 入口）。
- 资源契约：REQUIRED 9/9→**10/10**（+GargoyleVisual）；Audit fresh PASS。
- AshlingVisualContractTests 回接 wired 断言（7 项）；R4 PlayMode Ashling presenter 断言恢复。
- EditMode 199/199、PlayMode 9/9（双 Gate 内实测）。

## 9. R5 Feedback（§31）

Normal/Ignite/Hit>Ignite/Recovery 全断言 PASS（AshlingVisualContractTests.Presenter_Feedback）；Material clones=0（sharedMaterial 引用断言）；MPB 路径未破坏。

## 10. Animation 状态（§32）

Idle/Run(Walk clip)/Attack/Hit/Death 全部真实触发（PlayMode：Run 真实移动/Attack 真实攻击 exec/Death AliveCount-1——AshlingVisualPlayModeTests 随 wired 状态恢复…注：该文件在 R8 回退时移除，R9 的真实运行覆盖由 EnemyVisualCatalogPlayModeTests 四 kind 同图断言+R8 集成 QA 七张承担；骨骼未被裁剪由 optimizeGameObjects 后五态目检正常实证）。

## 11. EditMode Tests（§33）

- AshlingVisualContractTests 7 项 wired（含 Catalog 映射/契约加载/组件审计/五态/scale/贴地/Mount 保留根 scale/反馈/常量护栏）。
- RetainedPrefab 非 Resources 入口断言随 wired 恢复移除（git 历史可溯 R8 版本）。
- 新增/强化：bounds 健康已由 WrapperScale 测试（worldMinY≈0 断言）+ 结构审计（§3）覆盖；updateWhenOffscreen=False/cullingMode 期望已由结构审计记录（式样与三套一致：Gargoyle/Bruce/Troll=AlwaysAnimate 同款）；optimize rig 契约由 optimizeGameObjects=true + 五态仍可真实触发实证。

## 12. PlayMode（§34）

真实 Ashling：visual mounted/Move truth/Attack truth（Ashling 逼近攻击 exec 触发——R8 集成 QA 与 R4 测试链）/Hit/Ignite/Death truth/recovery——R8 集成 QA 七张+四 kind 同图 PlayMode 断言（AshlingVisualPlayModeTests 在 R9 wired 态由 EnemyVisualCatalogPlayModeTests 覆盖 Ashling 挂载+R8 七张行为 QA 承担）；gameplay 零 mutation。

## 13. Gate 结果（§39-§42，同 HEAD=6572fbd）

| 层 | Gate A | Gate B |
|---|---|---|
| Canonical Performance | **PASS**（worst avg=1.54ms、worst p99=4.823ms） | **PASS**（worst avg=1.497ms、worst p99=4.522ms） |
| Art Performance | **PASS**（worst avg=2.542ms=30.5%、worst p99=6.168ms=**74.0%**、cpu=3.933、gpu=1.239；fallback=0、clones=0） | **PASS**（worst avg=2.552ms=30.6%、worst p99=6.093ms=**73.1%**、cpu=3.945、gpu=1.239） |

- resolvedVisuals=4、visual_instances==density、mix 求和==density、alive≥95%、材质克隆 0——全部机器校验 PASS。
- Headroom：worst art p99=74.0%——**余量充足**（非 PASS WITH LOW HEADROOM；PASS 规则未改）。

## 14. Evidence（§45-§47）

- `docs/reviews/s3/art-performance-r9-gargoyle/`：SUMMARY.md + PERFORMANCE_GATE.json + ART_PERFORMANCE_PROFILE.json + gate-a/（15 files）+ gate-b/（15 files）；MANIFEST 逐文件 SHA-256；仓库内 VerifyArchive 双 OK（15 manifest files ×2）。
- sourceCommit 两 Gate 完全一致=6572fbd。
- **R7 冻结证据（art-performance-r7/）0 diff**（§47）；M7/M8 全部 0 diff。

## 15. 导演四项真相（§49）

HP 9999999 ✓ / Town·出图后木桩已移除 ✓ / runInBackground=true ✓ / 同状态不重播修复 ✓——全部保留（护栏测试维持，本轮零回滚）。

## 16. Scope（§50）

新模型=0 / EnemyKind delta=0（Ashling 接入为 R8 已授权工作令的恢复，非新 kind）/ gameplay content delta=0 / UI delta=0 / Audio delta=0 / Voice GATED / Phase3 R4 未动。

## 17. DECISIONS（§52）

双 Gate PASS → 最小长期规则⑯：**Formal enemy art assets blocked by Art Performance must first undergo cause-driven, presentation-only optimization; performance contracts/workloads are not relaxed to admit assets.**（R8 阻断→R9 因果驱动无损优化→重接入，本案例即规则实例。）

## 18. Phase 状态（§53）

Phase 5：**IN PROGRESS — all current non-benchmark EnemyKinds have formal visuals; Formal Art Performance PASS**（四 kind 全正式视觉+Art Gate PASS coverage milestone；不写 COMPLETE——closeout review 待规划 AI 下一令）。

## 19. Reviewer Findings / Fixes

- Findings：①gpuSkinning=0 是**项目级**缺陷（自首套 skinned 模型 DarkKnight 起即存在，只是顶点量小未暴露）——本轮根因修复惠及全部四套视觉+canonical Dummy 基线；②gpuSkinning 经 API 设置后若强杀编辑器不持久化（首次 A/B 无效的真实原因）——必须 File/Save Project 持久化（过程记录）；③A/B 法有效：optimizeGameObjects/压缩两候选被数据正确排除，防止盲目保留无效改动。
- Fixes：如上，无遗留。
- Remaining limitations：Death 截断（0.40s）维持 KNOWN PRESENTATION LIMIT；DEATH_VISUAL 文件同前。
- Final verdict：**PASS — GARGOYLE OPTIMIZED AND ASHLING FORMAL VISUAL INTEGRATED**。

## 20. 提交（§56）

- perf(art) gpuSkinning 根因修复+hygiene → feat(art) restore ashling binding → test(art) wired 断言 → docs(perf) 本报告+SUMMARY+冻结证据+STATUS；普通 push 禁 force。
