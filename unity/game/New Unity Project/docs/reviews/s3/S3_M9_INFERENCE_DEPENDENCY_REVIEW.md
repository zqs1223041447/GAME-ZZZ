# S3-M9 INFERENCE DEPENDENCY CLOSEOUT — 复核报告

工作令：S3-M9-INFERENCE-DEPENDENCY-CLOSEOUT（Unity Inference Dependency Root-Cause Removal + Unattended Gate Stability Closeout——无人值守维护序列最终根因项）。Owner：I=实现 / R=本复核（同轮自审）。日期：2026-09-08。

## Verdict

**PASS — Branch A（SAFE_TO_REMOVE，移除成功）**。Q1-Q8 全达标；Gate Stability Matrix 五行全 PASS+每步 tracked tree clean+SENTIS define 变动 0；维护序列正式结束（GATE-READY / AWAITING NEW PRODUCT DIRECTION）。

## 1. Baseline HEAD

- Baseline=f27a034（M8 STATUS 回填，M8 收口后 HEAD）。
- 审计提交（Branch A 原子 chore）=0f5908b：manifest+packages-lock+ProjectSettings（canonical define 消失）+审计文档 `S3_M9_INFERENCE_DEPENDENCY_AUDIT.md` 四文件原子入库。

## 2. Manifest direct dependencies（移除前）

`com.unity.ai.assistant 2.19.0-pre.2`、`com.unity.ai.inference 2.6.1`、`com.unity.multiplayer.center 1.0.1`、`com.unity.render-pipelines.universal 17.3.0`、`com.unity.test-framework 1.6.0`、`com.unity.pipeline 0.6.0-exp.1`、+builtin modules。

## 3. Inference depth 与 Reverse dependents

- Lock：`com.unity.ai.inference` depth=0（direct，source=registry）。**Q1 答案：Direct**（并纠正 M4「manifest 无 sentis 包」、M5/M8「间接依赖」两处历史失实表述——manifest 自 S0 d5beb7f 起未变，Inference 始终为直接依赖；历史文本按冻结原则不改写，以审计文档为准）。
- Reverse dependents=**0**（全 lock 仅 Inference 自身条目出现该包名；Q3 答案由此得出，非 depth 猜测）。

## 4. API usage audit（移除前后两次搜索一致）

- `Unity.InferenceEngine` / `Unity.Sentis` / `InferenceEngine`：**0**（Q2 答案：项目业务代码不使用它）。
- `ModelAsset` / `BackendType` / `Tensor` / `Worker` / `Functional` / `onnx` / `tflite`（词边界甄别）：**0**。
- 移除后 `git grep -i -E 'Unity\.InferenceEngine|Unity\.Sentis|InferenceEngine' -- Assets ContentData tools`：**NONE**。

## 5. Model asset audit

tracked `*.onnx/*.sentis/*.nn/*.tflite`=0；磁盘 Assets/ContentData=0；无 scene/prefab/SO/C# 消费路径存在；工程外未跟踪素材目录未触碰。

## 6. Asmdef audit

4 个 asmdef（Core/Content/EditMode/PlayMode）references 仅 Game 程序集+TestRunner/nunit：Inference/Sentis assembly reference=**0**。

## 7. Define generator root cause（「为什么删包能根治」的直接证据）

- `Library/PackageCache/com.unity.ai.inference@9a123aee5df7/Editor/Analytics/AnalyticsDefineManager.cs`：`static class AnalyticsDefineManager` + `[InitializeOnLoadMethod]`——每个实例（测试/构建/编辑器）每次域加载都执行；`FORCE_SENTIS_ANALYTICS` 恒加；否则按 `EditorAnalytics.enabled`（跨实例类型不稳定的机器级同意值）加/删 `SENTIS_ANALYTICS_ENABLED`。**该 Manager 即振荡唯一源头**；包移除 → `[InitializeOnLoadMethod]` 不存在 → define 恒保持现状。未修改/未提交任何 Library 文件（仅取证）。

## 8. AI Assistant relation

`com.unity.ai.assistant` lock dependencies = 2d.sprite/mathematics/mono-cecil/uielements/newtonsoft-json/unitywebrequest：**Assistant depends on Inference = NO**（Q4 答案，按 lock 事实非名称推断）。Assistant 保留未动；不受移除影响。

## 9. Decision

**SAFE_TO_REMOVE**（六条件全过，逐条见审计文档第 8 节）→ Branch A 执行。

## 10. Manifest diff

仅删除一行：`"com.unity.ai.inference": "2.6.1",`（Intentional；无其它 manifest 变化）。

## 11. Lock diff（UPM 实际解析结果，非手工）

- **Intentional**：移除 direct `com.unity.ai.inference`。
- **UPM Resolution Consequence**：移除传递 `com.unity.dt.app-ui`（唯一依赖方=Inference）。
- **Retained（共享包按解析保留）**：burst（render-pipelines.core 保活）、collections（render-pipelines.core 保活）、nuget.newtonsoft-json（Assistant 保活）、modules.imageconversion（builtin）。
- Unexpected package version churn：**无**（diff 3 insertions/28 deletions，仅上述两包块）；无 missing/manifest-lock mismatch；lock JSON 有效（54 entries）。

## 12. ProjectSettings diff

一次 canonical diff（0f5908b 内）：`scriptingDefineSymbols.Standalone: APP_UI_EDITOR_ONLY;SENTIS_ANALYTICS_ENABLED` → `APP_UI_EDITOR_ONLY`。此后五门零加删（见矩阵）。`APP_UI_EDITOR_ONLY` 保留（未扩成 define 清理轮）。

## 13-17. Gate Stability Matrix（本轮核心验收证据）

执行序：clean tracked tree（0f5908b）起步，每步门后检查 `git status` + define：

| # | Gate | PASS | tracked tree clean | SENTIS define mutation |
|---|---|---|---|---|
| 1 | SelfTest（快照工具） | ✓（9/9） | ✓ | 0 |
| 2 | Quick | ✓（115/115+3/3+Audit） | ✓ | 0 |
| 3 | Build（-IncludeBuild） | ✓（exe 667136B，Data **158**） | ✓ | 0（构建实例不再回写=关键根因证明） |
| 4 | Quick after Build | ✓（115/115+3/3） | ✓ | 0（Build→Quick 切换无振荡） |
| 5 | Player Runtime（-IncludePlayerRun） | ✓（PlayerRun exit=0，2560×1440，100/200/300 证据有效） | ✓ | 0 |
| 6 | Performance（-IncludePerformance） | ✓（9/9） | ✓ | 0 |

## 18. Performance Regression（post-package-removal proof，非新 baseline）

- Environment=PASS：2560×1440 fullscreen/D3D12/PC/vSync=0/targetFps=-1，CPU/GPU=锁定值（Ryzen 7 5700X3D / RTX 5070）不变。
- Worst avg=**1.660ms**（Run2/300）=预算 19.9%；worst p99=**2.376ms**（Run3/100）=预算 28.5%；worst CPU=1.660ms；worst GPU=0.581ms；alive：300 档 290/300（96.7%）+100/200 满员；FrameTiming 9/9。
- Verdict=PASS（exit=0）。
- **Contract changed：NO**（`docs/qa/PERFORMANCE_GATE.json` 对 0f5908b `git diff`=0 行；未重新 probe/未改硬件锁定）。
- **Workload changed：NO**（ArenaPerfHarness 零 diff；Densities/Warmup/Sample/CastInterval/Quality/resolution/frame budget 未动）。
- S2P 1440p/120 维持 CLOSED（无新冻结包，按工作令三十二只记录本 PASS）。

## 19. Define search after gates（工作令二十八）

`SENTIS_ANALYTICS_ENABLED` 在 tracked 文件中活跃出现=**0**（ProjectSettings 干净）；剩余命中全部为历史文档（DECISIONS/M4/M5/M8 Review+本审计文档）——按工作令允许保留，不为搜索「0」删除历史记录。

## 20. Lock health（工作令二十七）

lock JSON 有效；Inference 条目已消失；无 dangling dependency（app-ui 残留引用=0）；Unity resolve exit=0 无 package errors；无 missing package；manifest-lock 一致。

## 21. Tree pollution / Runtime delta / Content delta

- 每个 Gate 后 tracked tree clean（矩阵列 3）；无 Gate 自动 revert/白名单/忽略 hack（Q6=NO；snapshot Operator 0 diff，Gate 工具 0 diff）。
- Runtime C# delta=**0**；Tooling delta=**0**；Gameplay 0。
- Content delta：Skill/Support/Affix/Passive/Enemy/Resource/Scene/Stat/Tag/Effect-Event-Condition 全=**0**；资源审计维持（REQUIRED 6/6、Voice GATED 未变）。
- S2P 状态未破坏；Phase 3/4/5 未启动（仍 GATED）。

## 22. Remaining risks

- 低：`com.unity.ai.assistant`/`multiplayer.center`/`pipeline` 等其它「看起来未用」依赖按工作令四十九一律未动（记录在案，未来有明确需求时另行评估）。
- 低：Player build Data 文件 162→158（Inference 运行时片移除的观测性瘦身；非成功标准）。
- 无其它新增风险。

## 23. Q1-Q8 必查

- **Q1** Inference 是 direct 还是 indirect？→ **Direct**（depth=0；纠正历史失实表述）。
- **Q2** 项目业务代码真的使用它吗？→ **NO**（0 命中，扫描证据）。
- **Q3** 其它 package 是否依赖它？→ **NO**（lock reverse dependent=0）。
- **Q4** Assistant 是否依赖它？→ **NO**（lock 事实）。
- **Q5** 移除后 Quick→Build→Quick 是否还会改 ProjectSettings？→ **NO**（矩阵实测：Quick/Build/Quick-after-Build define 变动均 0，树净）。
- **Q6** Gate 是否通过自动 revert 隐藏污染？→ **NO**（工具零改动；无 revert/白名单）。
- **Q7** Performance contract/workload 是否变化？→ **NO**（diff=0 行 / Harness 零 diff）。
- **Q8** Package removal 是否造成 Player/Performance 回归？→ **NO**（Build PASS、PlayerRuntime PASS、Performance 9/9 PASS 优于既有 worst 记录带内）。

## 24. 结论

根因包（com.unity.ai.inference 2.6.1，自 S0 即为直接依赖但零使用）已安全移除；`AnalyticsDefineManager` 不再存在，SENTIS define 跨 Gate 振荡从机制上根除；五门矩阵全绿、树净、内容零改动、性能契约零改动且回归 PASS。**无人值守技术维护序列到此正式结束：GATE-READY / AWAITING NEW PRODUCT DIRECTION**（不自行创建 M10；Phase 3/4/5 仍导演门控；新 Content Batch 仍不得自行启动）。
