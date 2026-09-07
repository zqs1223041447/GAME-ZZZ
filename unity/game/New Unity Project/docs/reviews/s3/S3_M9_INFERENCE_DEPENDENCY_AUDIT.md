# S3-M9 INFERENCE DEPENDENCY AUDIT（零使用审计 → 移除决策）

工作令：S3-M9-INFERENCE-DEPENDENCY-CLOSEOUT（Phase A：任何 Packages 修改之前的审计，本文即其保存结果）。日期：2026-09-08。Baseline HEAD=f27a034。

## Decision

**SAFE_TO_REMOVE**（Branch A 成立——六项 Branch A 条件全部满足，见下）。

## 1. Manifest status（含历史表述纠正）

- `Packages/manifest.json`：`com.unity.ai.inference: 2.6.1` 为 **manifest 直接依赖**；`com.unity.ai.assistant: 2.19.0-pre.2` 为另一**独立**直接依赖。
- **历史表述纠正（如实记录）**：manifest.json 自 S0 初始推送 d5beb7f 起从未被修改（`git log -- Packages/manifest.json` 仅一条=初始提交），即 **Inference 从工程初始就是直接依赖**。M4 复核（S3_M4 第 54 行）「manifest 无 sentis/ml 包」与 M5/M8 时代「间接依赖包」表述**均为失实**——以本文为准纠正；历史 Review 文本按冻结原则不改写。

## 2. Lock depth 与依赖图（Packages/packages-lock.json，移除前）

- `com.unity.ai.inference`：**depth=0**（direct），source=registry，自身依赖：burst 1.8.17 / dt.app-ui 1.3.6 / collections 2.4.3 / nuget.newtonsoft-json 3.2.1 / modules.imageconversion。
- Reverse dependents：全 lock 仅 Inference 自身条目出现该包名，**无任何其它 package 的 dependencies 块引用 `com.unity.ai.inference`**（非 depth 猜测，逐条目核对）。
- 共享依赖预判（最终以 UPM 解析结果为准，不硬编码）：burst 同时被 render-pipelines.core 与 collections 依赖；collections 同时被 render-pipelines.core 依赖 → 两者预计由 URP 保活；dt.app-ui 仅被 Inference 依赖 → 预计随删。

## 3. Runtime / Editor API 使用（代码审计）

搜索范围=整个 Unity 工程 tracked 内容（Assets、ContentData、Packages、tools、docs）：

- `Unity.InferenceEngine` / `Unity.Sentis` / `InferenceEngine`：**0 命中**。
- `ModelAsset` / `BackendType` / `FORCE_SENTIS_ANALYTICS`：**0 命中**（工程代码）。
- 常见词甄别 `Tensor` / `Worker` / `Functional` / `onnx` / `tflite`（词边界）：**0 命中**——不存在「普通单词命中被误判」问题。
- `SENTIS_ANALYTICS_ENABLED` 活跃出现：仅 `ProjectSettings/ProjectSettings.asset` define 行本身；其余全部为历史文档记录（DECISIONS/M4/M5/M8 Review——按工作令二十八允许保留）。
- Runtime API references=**0**；Editor/test API references=**0**。

## 4. Model assets

- tracked 树 `*.onnx / *.sentis / *.nn / *.tflite`：**0**。
- 磁盘 `Assets/`、`ContentData/`（非 Library）：**0**。
- 结论：无任何模型资产可被 scene/prefab/SO/C# 消费；工程外未跟踪素材目录（历轮不动）与本审计无关、未触碰。

## 5. Asmdef references

全部 4 个 asmdef（Game.Runtime.Core / Game.Runtime.Content / Game.Tests.EditMode / Game.Tests.PlayMode）references 仅 Game 程序集 + TestRunner/nunit：**Inference/Sentis assembly 引用=0**；无 defineConstraints/versionDefines 涉及 SENTIS。

## 6. AI Assistant 关系（按 lock 事实，非名称推断）

- `com.unity.ai.assistant`（depth=0）dependencies = 2d.sprite / mathematics / mono-cecil / uielements / newtonsoft-json / unitywebrequest：**Assistant 依赖 Inference=NO**。
- 结论：移除 Inference 不影响 Assistant 解析；Assistant 保留（工作令四：本轮只处理 Inference）。

## 7. Define generator root cause（为什么删包能根治振荡）

- 包：com.unity.ai.inference 2.6.1；文件：`Library/PackageCache/com.unity.ai.inference@9a123aee5df7/Editor/Analytics/AnalyticsDefineManager.cs`（未修改，仅取证）。
- 机制：`static class AnalyticsDefineManager`（namespace `Unity.InferenceEngine.Editor.Analytics.Import`）挂 `[InitializeOnLoadMethod]`——**每个 Unity 实例（测试/构建/编辑器）每次域加载都运行**；逻辑：有 `FORCE_SENTIS_ANALYTICS` 则恒加 define；否则在 `UNITY_2023_2_OR_NEWER && ENABLE_CLOUD_SERVICES_ANALYTICS` 下取 `EditorAnalytics.enabled`（该机器级同意值跨实例类型不稳定=振荡源：测试实例=假→删 define，构建/编辑器实例=真→加 define）；包移除 → 该 `[InitializeOnLoadMethod]` 不再存在 → define 恒保持现状 → **振荡根除**。
- 本机取证的 14 个相关包内文件（Analytics/Visualizer/Importer 系列）全部位于 PackageCache（不提交、不修改）。

## 8. Removal decision 依据汇总（Branch A 六条件）

1. Runtime 零 Inference API 使用 ✓（3 节）
2. Editor 项目代码零使用 ✓（3 节）
3. 无被消费的 .onnx/.sentis/模型资产 ✓（4 节）
4. asmdef 无 Inference assembly reference ✓（5 节）
5. packages-lock 无其它 package 依赖 Inference ✓（2 节）
6. gameplay/tooling 无 SENTIS 条件编译依赖 ✓（3 节；唯一活跃出现=define 本身，随收口删除）

后续执行：最小删除（manifest 仅删 Inference 一条）→ UPM 正常 resolve（lock 由 Unity 更新）→ ProjectSettings canonical define 消失（一次性，工作令十六允许）→ 五步 Gate Stability Matrix 回归。见 Review 文档。
