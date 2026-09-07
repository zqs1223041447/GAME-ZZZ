# S2P 技术收口

日期：2026-09-07。状态：**技术收口已记录**；S2P 的 1440p/120 目标最初未结案，**2026-09-08（M7）已在锁定硬件上正式关闭**（见下方 M7 Final Closeout）。

注（S3-M6 补记）：历史结果文件首行沿用早期「S2P 1440p harness」命名，**实际分辨率以每份文件的 `resolution=` 行为准**（该轮为 1920×1080）；历史数据文件保持原样不现代化（S3-M6 起新证据标题已中性化为 `# ArenaPerfHarness, density=<n>`）。

## M7 1440p/120 Final Closeout（2026-09-08）

**S2P 1440p/120 = CLOSED**：2026-09-08 已在 M7 锁定测试硬件（AMD Ryzen 7 5700X3D 8-Core / NVIDIA GeForce RTX 5070）、真实 StandaloneWindows64、真实 2560×1440、D3D12、PC Quality（vSync=0 / targetFrameRate=-1）环境通过 canonical 120FPS Performance Gate（预算 8.33ms）。

- 证据：2 个 canonical Gate × 3 Run × 3 Density = 18 次测量全部 PASS；worst main avg=1.676ms / worst p99=2.388ms（预算的 28.7%）；cpu/gpu avg 全部 ≤8.33 且 >0；FrameTiming 18/18 available；alive 96.7%-100%（≥95%）；frames=600×18。完整冻结证据包见 `1440p-120-m7/`（SUMMARY.md + PERFORMANCE_GATE.json 快照 + gate-b/Run1-3 原始文件）。
- contract：`docs/qa/PERFORMANCE_GATE.json`（机器可读唯一真相源）；预算语义：`docs/qa/PERFORMANCE_BUDGET.md`。
- 边界：此结论仅适用于锁定硬件与 canonical 环境；其它机器需按 contract 显式更新硬件字段并重新建立 baseline，**不构成「所有 Windows PC 都保证 120FPS」**。
- 历史 1080p 证据（`1440p/`）与本表三档数字保持原样（当时环境与当前环境不同，不互相覆盖）。

## 证据包

| 轮 | commit | 环境 | 路径 |
|---|---|---|---|
| Editor 内嵌基线 | 6e95c1b | Editor 内嵌 Game 视图、DX12、带施法（会话内注入） | `docs/reviews/s2p/baseline/`（SUMMARY + 100/200/300.txt） |
| 独立包 1080p 全屏 | b152610 | StandaloneWindows64、DX12、1920×1080 全屏（显示器原生）、带施法（仓库 ArenaPerfHarness） | `docs/reviews/s2p/1440p/`（SUMMARY + 100/200/300.txt + player-run.log） |

## 三档数字（独立包 1080p，主线程 avg/p99，GPU）

| 实体 | 存活(末帧) | 主线程 avg/p99 | GPU |
|---|---|---|---|
| 100 | 100/100 | 1.380 / 1.958 ms | 0.323 ms |
| 200 | 200/200 | 1.519 / 2.168 ms | 0.322 ms |
| 300 | 294/300 | 1.753 / 2.508 ms | 0.328 ms |

全部低于 120FPS 预算 8.33ms。Editor 基线（2.581/2.838/3.112 avg）保留作对照。

## 未采到字段

渲染线程独立值（FrameTiming 无此数据）、DrawCall、Animator / Separate / 转向 / 物理 / VFX 细分耗时（无 Profiler 计数；开深 Profiler 会改变被测对象）。1440p 分辨率证据缺失。

## 密度与施法可复现性

- 密度策略：击杀后 `SpawnAt` 即时补到额定（黄金角确定性散布，未用 `UnityEngine.Random`）；300 档末帧 294/300 接受为接近额定。
- 施法驱动：`Assets/Runtime/Core/Gameplay/ArenaPerfHarness.cs`，默认关；独立包 `-arenaPerf` 命令行开启；Q/W/E 轮换 0.12s。

## 阻塞与残留

- **1440p 硬件阻塞**：本机显示器 1920×1080，全屏/窗口 2560×1440 均被钳回。待有 1440p 硬件后补测（不是 S3 前置阻断）。（M6 当时状态；后由 M7 在锁定硬件下 CLOSED，见上方 Final Closeout）
- 非阻断残留（已清，2026-09-07 改名轮）：代码名已由曾用名 `EveView / DriveEve` 改为 `DarkKnightView / DriveDarkKnight`（历史 commit/记录保留曾用名）。`Player/Eve` 回退分支已删除（2026-09-07）——`TryMount` 只加载 `Player/DarkKnight`，缺失即回退胶囊并打日志；仓库内无 Player/Eve 预制体、无运行时 .blend 加载。

## 结论

1080p 独立包路径已证；1440p/120 未结案；**不宣布 120FPS@1440p**。（本段为 M5/M6 时代结论；1440p/120 后由 M7 CLOSED——以本文头部状态与「M7 Final Closeout」章节为准）
