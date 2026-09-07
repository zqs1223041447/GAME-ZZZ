# S2P 1440p/120 M7 PERFORMANCE GATE EVIDENCE（冻结历史证据）

日期：2026-09-08。状态：**PerformanceVerdict = PASS（两个 canonical Gate 全部通过）→ S2P 1440p/120 = CLOSED（M7 locked hardware）**。

## Locked Environment（contract 快照见 PERFORMANCE_GATE.json）

| 项 | 值 |
|---|---|
| commit | 本轮收口 commit（见 STATUS/最近一次绿灯行） |
| Unity | 6000.3.23f1 |
| OS | Windows 11 Pro（10.0.26100） |
| CPU（locked） | AMD Ryzen 7 5700X3D 8-Core Processor |
| GPU（locked） | NVIDIA GeForce RTX 5070 |
| 实测分辨率 | 2560×1440（`Screen.width/height`，三档一致，fullscreen=True） |
| Graphics API | Direct3D12（`SystemInfo.graphicsDeviceType`） |
| Quality | PC（`QualitySettings.names[GetQualityLevel()]` 实测） |
| vSync / targetFrameRate | 0 / -1（`-arenaPerfGate` 运行期钉死，metadata 实测值） |
| Frame budget | 8.33 ms（120 FPS） |
| Harness workload | 密度 100/200/300、warmup 60、sample 600、castInterval 0.12、kill-then-refill、Q/W/E 轮换——与 S2P 以来完全一致（未为性能改动任何工作量） |

## Gate A（canonical Performance Gate 第 1 次；输出为 temp ephemeral，完整数字如下）

| Run | 100（avg / p99 / cpu / gpu / alive） | 200（同） | 300（同） |
|---|---|---|---|
| 1 | 1.337 / 2.130 / 1.338 / 0.564 / 100/100 | 1.471 / 2.090 / 1.470 / 0.564 / 200/200 | 1.663 / 2.326 / 1.663 / 0.562 / 290/300 |
| 2 | 1.334 / 2.076 / 1.335 / 0.565 / 100/100 | 1.443 / 2.098 / 1.442 / 0.565 / 200/200 | 1.675 / 2.354 / 1.676 / 0.562 / 293/300 |
| 3 | 1.358 / 2.054 / 1.358 / 0.564 / 100/100 | 1.448 / 2.169 / 1.448 / 0.564 / 200/200 | 1.664 / 2.349 / 1.666 / 0.561 / 290/300 |

Verdict：Environment=PASS；PerformanceVerdict=**PASS**（9/9 密度×轮 全过）。

## Gate B（canonical Performance Gate 第 2 次；原始文件冻结于 `gate-b/Run1-3/`）

| Run | 100（avg / p99 / cpu / gpu / alive） | 200（同） | 300（同） |
|---|---|---|---|
| 1 | 1.367 / 2.087 / 1.367 / 0.578 / 100/100 | 1.467 / 2.087 / 1.467 / 0.578 / 200/200 | 1.647 / 2.305 / 1.647 / 0.577 / 290/300 |
| 2 | 1.327 / 2.075 / 1.328 / 0.561 / 100/100 | 1.476 / 2.117 / 1.476 / 0.561 / 200/200 | 1.661 / 2.388 / 1.662 / 0.561 / 291/300 |
| 3 | 1.343 / 2.125 / 1.342 / 0.579 / 100/100 | 1.450 / 2.078 / 1.449 / 0.579 / 200/200 | 1.659 / 2.261 / 1.657 / 0.578 / 290/300 |

Verdict：Environment=PASS；PerformanceVerdict=**PASS**（9/9 全过）。`gate-b/Performance/Run1-3/` 为完整原始证据（100/200/300.txt + PlayerRun.log ×3）。

## Aggregate（2 Gate × 3 Run × 3 Density = 18 次测量）

- Worst main avg：1.676 ms（预算 8.33 的 20.1%）
- Worst main p99：2.388 ms（预算 8.33 的 28.7%）
- Worst cpu avg：1.676 ms；Worst gpu avg：0.579 ms（均 ≤8.33 且 >0）
- GC observation：gc_alloc_bytes_avg = 0.0（全部采样）
- Memory observation：mem_total_mb ≈ 121.8（观测记录，未设门）
- Alive validity：290-293/300（96.7%-97.7% ≥ 95%）与 100/100、200/200——全部有效
- FrameTiming：18/18 available（frame_timing_ok=true，cpu/gpu avg 均有效）
- 最终结论：**PerformanceVerdict = PASS**；**S2P 1440p/120 在 M7 锁定测试硬件（AMD Ryzen 7 5700X3D / RTX 5070）、真实 StandaloneWindows64、真实 2560×1440、D3D12、PC Quality 环境下通过 120FPS 工程门**。此结论仅适用于锁定硬件与 canonical 环境，**不构成「所有 Windows PC 都保证 120FPS」**；其它机器需按 contract 显式更新并重新建立 baseline。

## 边界说明

- Gate A 的原始文件按 temp 契约被下一次运行清理（ephemeral），其完整数字由本表记录（来自 canonical 控制台输出）；Gate B 的原始文件完整冻结于本目录。
- 历史 1080p 证据（`docs/reviews/s2p/1440p/`）保持不动；本目录不覆盖任何历史文件。
