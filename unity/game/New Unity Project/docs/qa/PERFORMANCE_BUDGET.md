# PERFORMANCE_BUDGET（1440p/120 性能预算 · QA 操作契约）

S3-M7 建立。机器可读唯一真相源：`docs/qa/PERFORMANCE_GATE.json`（脚本从 JSON 读取，禁止在脚本/文档里复制第二套 2560/1440/8.33/3-runs 真相）。

## Target

- **1440p / 120 FPS**（工程帧预算 **8.33 ms**）。
- 表述边界：通过=「在仓库锁定的 M7 测试硬件、真实 StandaloneWindows64、真实 2560×1440、D3D12、PC Quality 下通过 120FPS 工程门」；**不得写「所有 Windows PC 都保证 120FPS」**。

## Locked Environment（来自 JSON）

| 项 | 值 |
|---|---|
| Resolution | 2560×1440（actual `Screen.width/height` 为证据；请求值不是证据） |
| Fullscreen | true |
| Graphics API | Direct3D12 |
| Quality | PC（实际 `QualitySettings.name` 为证据） |
| vSyncCount | 0（`-arenaPerfGate` 运行期钉死） |
| targetFrameRate | -1（`-arenaPerfGate` 运行期钉死） |
| Hardware | CPU/GPU 锁定字符串（probe 实测回填 JSON；变更需正式工作令显式更新，禁止自动学习） |

## Hard Metrics（每一轮 × 每一密度全部满足）

| 项 | 门 |
|---|---|
| main_ms_avg | ≤ 8.33 |
| main_ms_p99 | ≤ 8.33（p99 是正式硬门，只看 avg 不够） |
| cpu_ms_avg | > 0 且 ≤ 8.33 |
| gpu_ms_avg | > 0 且 ≤ 8.33 |
| frame_timing_ok | 必须 = true（FrameTiming 不可用 → EVIDENCE_INCOMPLETE，非 PERF_FAIL） |
| alive / dummy_count | ≥ 0.95（100→≥95 / 200→≥190 / 300→≥285；测量有效性规则） |
| frames | == 600（严格等于 sampleFrames，非仅 >0） |
| 运行数 | requiredRuns=3，同一 Build 连续 3 次独立 Player 运行，**全部 PASS**（不挑最好） |

## Observational Metrics（只记录不设门）

- main_ms_max（单点 OS/scheduler spike 不作正式门槛；未来 frame hitch budget 另立规则）
- main_ms_p95 / p999、gc_alloc_bytes_avg、mem_total_mb（进入 Performance Evidence；明显异常记风险，不自动放宽/收紧其它门）

## Verdict Semantics

| 状态 | 含义 |
|---|---|
| NOT_EVALUATED | 未请求性能 Gate |
| PASS | 环境正确且三轮全部硬指标通过 |
| FAIL | 环境正确但任一硬性能指标/测量有效性失败（PERF_FAIL） |
| ENV_NOT_MET | 实际环境不满足 locked contract（分辨率/全屏/API/质量/vSync/targetFps/硬件任一不符）——不得评性能 |
| EVIDENCE_INCOMPLETE | 环境看似正确但正式测量证据不足（如 FrameTiming unavailable） |
| INFRA | Player/构建/进程等基础设施失败 |

## R3 Content Expansion 契约

- Performance Gate **FAIL**（环境正确、真超预算）→ 记录 `CONTENT EXPANSION FROZEN BY PERFORMANCE GATE`，禁止自行扩内容；由规划 AI 另下 Performance Optimization 工作令。
- **ENV_NOT_MET** → 记录 `PERFORMANCE NOT EVALUATED`，不等于游戏性能失败。

## Anti-Cheating（Reviewer 必查）

禁止为让数字变绿：减怪、降 quality、关 shadow/renderer、关技能、减 SampleFrames、增 CastInterval、改 top-up/kill-refill、降分辨率、用 nographics、用 batchmode Player、换更快渲染路径、删 gameplay workload。任何一项 = Reviewer FAIL。`-arenaPerfGate` 的唯一允许作用 = 运行期钉死 `vSyncCount=0` + `targetFrameRate=-1`（解除帧率限制，非性能作弊模式）。

## Hardware Update Policy

换机器/换显卡 → 禁止 Gate 自动学习新硬件；必须正式工作令更新 `PERFORMANCE_GATE.json` 硬件字段并重新建立 baseline。
