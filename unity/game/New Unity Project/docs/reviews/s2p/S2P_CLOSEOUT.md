# S2P 技术收口

日期：2026-09-07。状态：**技术收口已记录**；S2P 的 1440p/120 目标**未结案**。

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

- **1440p 硬件阻塞**：本机显示器 1920×1080，全屏/窗口 2560×1440 均被钳回。待有 1440p 硬件后补测（不是 S3 前置阻断）。
- 非阻断残留（已清，2026-09-07 改名轮）：代码名已由曾用名 `EveView / DriveEve` 改为 `DarkKnightView / DriveDarkKnight`（历史 commit/记录保留曾用名）。`Player/Eve` 回退分支已删除（2026-09-07）——`TryMount` 只加载 `Player/DarkKnight`，缺失即回退胶囊并打日志；仓库内无 Player/Eve 预制体、无运行时 .blend 加载。

## 结论

1080p 独立包路径已证；1440p/120 未结案；**不宣布 120FPS@1440p**。
