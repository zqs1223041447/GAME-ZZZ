# S3_M7_1440P_PERFORMANCE_GATE_REVIEW（S3 维护轮 M7 独立复核）

日期：2026-09-08。工作令：`S3-M7-1440P-PERFORMANCE-GATE`。Owner：Phase I=实现 / Phase R=A15 Integration/Reviewer（本文）。Baseline HEAD=d25d5fd。

## Baseline HEAD / Performance contract

- Baseline=d25d5fd（M6 回填）；验证链 Quick/Audit/Build/Player-run truth 全部就位；`PerformanceVerdict=NOT_EVALUATED`；S2P 1440p/120=NOT CLOSED。
- Contract（机器可读唯一真相源）：`docs/qa/PERFORMANCE_GATE.json`（schemaVersion=1；2560×1440/fullscreen/Direct3D12/PC/vSync=0/targetFrameRate=-1/densities 100-200-300/warmup 60/sample 600/castInterval 0.12/frameBudgetMs 8.33/requiredRuns=3/requireFrameTiming=true/minAliveRatio=0.95/hardware CPU+GPU）。脚本从 JSON 读取，无第二套硬编码真相；预算语义文档=`docs/qa/PERFORMANCE_BUDGET.md`。

## Hardware probe → Locked CPU/GPU

- 流程（工作令七）：Harness 先增只读硬件/环境元数据 → probe（Player Runtime Gate）实测 → 回填 contract。
- 实测（probe 证据，`PlayerRun/100.txt`）：`hardware_cpu=AMD Ryzen 7 5700X3D 8-Core Processor`、`hardware_gpu=NVIDIA GeForce RTX 5070`、`perf_env quality=PC vsync=0 targetFps=-1 warmup=60 sample=600 castInterval=0.12`、resolution=2560×1440、editor=False、dx=Direct3D12。
- 回填后 contract 硬件锁定；`Get-PerformanceContract` 拒绝 PENDING_HARDWARE_PROBE 状态（防未锁硬件就评性能）。

## Environment enforcement

每轮×每密度文件实测（Runtime API，非预期值打印）对拍 contract：editor=False / resolution==2560×1440（`Screen.width/height`，请求值不是证据）/ fullscreen=True / dx==Direct3D12 / quality==PC（`QualitySettings.names[GetQualityLevel()]`）/ vSync==0 / targetFps==-1 / warmup==60 / sample==600 / castInterval==0.12 / CPU+GPU==locked。任一不符 → ENV_NOT_MET（不评性能）。

## Anti-cheat diff（Reviewer 必查）

`git diff` 审计 ArenaPerfHarness.cs：**仅**新增 `-arenaPerfGate` 解析、vSync/targetFrameRate 钉死、3 行只读元数据（hardware_cpu/hardware_gpu/perf_env）+1 处修复（`QualitySettings.name`→`names[GetQualityLevel()]`，静态上下文编译错误）。**Densities/WarmupFrames/SampleFrames/CastInterval/kill-refill/QWE 轮换/Spawn strategy/Arena seed/Combat/PerfSampler/质量/分辨率全部未动**；未用 nographics、未用 batchmode Player、未换渲染路径、未删 workload。任何一项改动=Reviewer FAIL——全部清白。

## Gate A / Gate B（每轮三档数据，ms）

**Gate A**（canonical 第 1 次；temp ephemeral，数字如下记录）：

| Run | 100（avg/p99/cpu/gpu/alive） | 200 | 300 |
|---|---|---|---|
| 1 | 1.337/2.130/1.338/0.564/100 | 1.471/2.090/1.470/0.564/200 | 1.663/2.326/1.663/0.562/290 |
| 2 | 1.334/2.076/1.335/0.565/100 | 1.443/2.098/1.442/0.565/200 | 1.675/2.354/1.676/0.562/293 |
| 3 | 1.358/2.054/1.358/0.564/100 | 1.448/2.169/1.448/0.564/200 | 1.664/2.349/1.666/0.561/290 |

**Gate B**（canonical 第 2 次；原始文件冻结于 `docs/reviews/s2p/1440p-120-m7/gate-b/Run1-3/`）：

| Run | 100（avg/p99/cpu/gpu/alive） | 200 | 300 |
|---|---|---|---|
| 1 | 1.367/2.087/1.367/0.578/100 | 1.467/2.087/1.467/0.578/200 | 1.647/2.305/1.647/0.577/290 |
| 2 | 1.327/2.075/1.328/0.561/100 | 1.476/2.117/1.476/0.561/200 | 1.661/2.388/1.662/0.561/291 |
| 3 | 1.343/2.125/1.342/0.579/100 | 1.450/2.078/1.449/0.579/200 | 1.659/2.261/1.657/0.578/290 |

## Hard metric result

- 18/18 测量（2 Gate×3 Run×3 Density）：main avg ≤1.676（≤8.33 ✓）、main p99 ≤2.388（≤8.33 ✓，p99 硬门）、cpu avg ∈[1.328,1.676]（>0 ≤8.33 ✓）、gpu avg ∈[0.561,0.579]（>0 ≤8.33 ✓）、frame_timing_ok=true ×18、alive=100/100、200/200、290-293/300（≥95% ✓）、frames=600 ×18。**全部硬指标 PASS**。

## GC/memory observations

- gc_alloc_bytes_avg=0.0（全采样）；mem_total_mb≈121.8——观测记录，未设门（工作令二十二）；无异常。

## Environment verdict / Performance verdict / S2P closeout verdict

- Environment：Gate A=PASS、Gate B=PASS（2560×1440/D3D12/PC/vSync0/targetFps-1/CPU+GPU locked 全对齐）。
- PerformanceVerdict：Gate A=**PASS**、Gate B=**PASS**（非挑最好——两 Gate 每轮每密度全过）。
- **S2P closeout verdict：1440p/120 = CLOSED（M7 locked hardware）**——S2P_CLOSEOUT.md 增 M7 Final Closeout 节；STATUS 门状态行更新为「已证 / CLOSED（M7 locked hardware）」；结论边界=仅锁定硬件与 canonical 环境，不构成「所有 Windows PC 都保证 120FPS」。历史 1080p 证据未动。

## SelfTest

36/36 PASS exit 0（XML 5 + Audit 3 + freshness + 构件 5 + Player 证据 8 + **Performance 15**：contract schema/合法三轮 PASS/p99 超预算/avg 超预算/GPU 超预算/分辨率不符 ENV_NOT_MET/API 不符/Quality 不符/vSync 不符/targetFps 不符/硬件不符 ENV_NOT_MET/FrameTiming 不可用 EVIDENCE_INCOMPLETE/alive 比例无效 FAIL/frames 无效 FAIL/文件级硬件与环境行捕获）。contract 单一来源加载（SelfTest 不复制第二套 8.33）。

## Repository pollution / Content delta

- 实时性能输出全在 `%TEMP%\GAME-ZZZ-UnattendedGate\Performance\Run1-3\`（逐轮清空隔离）；仓库内仅本轮工作文件（harness 元数据 + gate 脚本 + contract + docs + 冻结证据包 `1440p-120-m7/`——按工作令五十三由执行 AI 明确复制，Gate 脚本自身不写该目录）。
- Content delta 全 0（Support/Affix/Skill/Passive/Enemy/Resource/Scene/Combat 机制不变）；Phase 3-5 未启动（M7 只关 S2P 技术性能门，不自动开启 UI/Voice/Art）。

## 顺带修复（无人值守健壮性）

1. PlayMode 跳过分支 `$playInfra` 未赋值 → $null 绑定 [bool] 参数崩溃（M4 潜伏 bug，本轮回归时首次暴露——编辑器意外开着导致 EditMode 基础设施失败触发该路径）→ 跳过分支显式置 $false。
2. 崩溃/强杀残留僵死锁 `Temp/UnityLockfile`（无 Unity 进程存活）会永久阻塞无人值守流程 → Gate 检测：锁存在且无任何 Unity 进程 → 警告并移除后继续；有 Unity 进程一律 fail fast（不杀任何进程，工作令六契约不变）。
3. Harness 静态上下文 `QualitySettings.name` 编译错误（CS0120）→ 修正为 `QualitySettings.names[QualitySettings.GetQualityLevel()]`（由 probe 即时抓出——Gate 自证有效）。

## Remaining risks

- 低：性能结论绑定锁定硬件（contract 硬件变更需正式工作令重测——设计意图）；其它机器/环境未测。
- 低：max/p95/p999/GC/memory 仅观测（frame hitch budget 另立规则）。
- 低：`-screen-quality PC` 依赖项目存在名为 PC 的质量档（QualitySettings.asset 实证存在；档名变更需同步 contract）。
- 低：Gate A 原始文件按 temp 契约被后续运行清理（ephemeral）；其完整数字记录于冻结包 SUMMARY.md，Gate B 原始文件完整冻结。

## Verdict

**PASS**
