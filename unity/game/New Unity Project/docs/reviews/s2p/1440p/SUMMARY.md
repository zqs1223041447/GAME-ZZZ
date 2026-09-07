# S2P 1440p 独立测量轮 SUMMARY

结论：**1440p 轮已记录**（实际为 1920×1080——见「分辨率阻塞」）；主线程/GPU **均未越过 8.33ms 预算**。不写「S2P 放行」。

## 阻塞记录（无人监管规则：记录后继续）

- **1440p 不可兑现**：本机显示器物理分辨率 1920×1080。全屏 2560×1440 被系统钳到原生 1080p；窗口化 2560×1440 同样被钳回（`resolution=1920x1080`）。已尝试 ` fullscreen 1/0 + -screen-width/height 2560x1440` 两种方式。**待更硬件后补测 1440p**；本轮以「独立包 + 原生 1920×1080 全屏」交付（仍满足「独立 Game 窗口、非 Editor 内嵌视图」）。

## 环境与条件

| 项 | 值 |
|---|---|
| 采样代码 commit | 见文末提交行（基于 6e95c1b 之上：ArenaPerfHarness + ArenaDirector 钩子 + FrameTiming 统计设置） |
| 运行体 | **独立包**（StandaloneWindows64、Mono、DX12、Direct3D12，`editor=False`），非 Editor 内嵌视图 |
| 分辨率 | 1920×1080 全屏（显示器原生；1440p 阻塞见上） |
| 是否进图 | 否（`enteredMap=false`，Arena 本体） |
| 玩家模型 | DarkKnight mounted，每帧包围盒贴地（日志 `[Arena] Eve fitted body height 1.16 from 4.755 scale x0.24`） |
| 施法驱动 | **来自仓库**：`Assets/Runtime/Core/Gameplay/ArenaPerfHarness.cs`，默认关；独立包以 `-arenaPerf` 命令行开启（`RuntimeInitializeOnLoadMethod` 挂常驻 Runner，自动从 Bootstrap 切到 Arena）；编辑器内 `StartManual`。施法 Q/W/E 轮换、间隔 0.12s（≈8.3 次/秒）、目标最近存活实体 |
| 密度策略 | **击杀后立即补到额定**：每帧检查存活数，低于额定即 `SpawnAt` 单只补怪（黄金角确定性散布，禁 `UnityEngine.Random`）。采样窗口内存活保持 ≈额定 |
| 采样窗口 | 每档 60 帧预热 + 600 帧采样；每档独立写 `100/200/300.txt`（后跑不覆盖前跑）+ `player-run.log` 原始副本 |

## 三档结果（独立包 1080p 全屏，带施法）

| 实体 | 存活(末帧) | 主线程 avg | p95 | p99 | p999 | max | GC/帧 | CPU 帧 | GPU | 内存(总分配) |
|---|---|---|---|---|---|---|---|---|---|---|
| 100 | 100/100 | 1.380ms | 1.727 | 1.958 | 2.564 | 3.237 | 0 B | 1.381ms | 0.323ms | 118.9 MB |
| 200 | 200/200 | 1.519ms | 1.866 | 2.168 | 2.432 | 2.457 | 0 B | 1.519ms | 0.322ms | 121.1 MB |
| 300 | 294/300 | 1.753ms | 2.144 | 2.508 | 2.787 | 2.790 | 0 B | 1.753ms | 0.328ms | 119.0 MB |

存活 min/avg 未单独记录：补怪策略即时把存活拉回额定（末帧即常态；294/300 为补怪竞态的瞬时值）。

## 与基线（6e95c1b，Editor 内嵌）对比

主线程 avg：100 档 2.581→1.380ms（-47%）、200 档 2.838→1.519ms（-46%）、300 档 3.112→1.753ms（-44%）——差异为 Editor 常驻开销，独立包数字更接近最终成绩。

## 是否越过 8.33ms（120FPS 预算）

- 主线程 p99：最大 2.508ms（300 档）——**未越过**。
- GPU：0.322–0.328ms——**未越过**。
- 但：1080p 非 1440p；渲染线程独立值/DrawCall/Animator/Separate/物理/VFX 细分未采到（FrameTiming 无独立 render-thread 值；开 Profiler 会改变被测对象）。故只记「已记录」，**120FPS/1440p 结案仍不做**。

## Runtime diff（本轮提交范围）

- 新增 `Assets/Runtime/Core/Gameplay/ArenaPerfHarness.cs`（测量工具，默认关闭，命令行/显式调用启动；含常驻 Runner 与密度补齐）
- `Assets/Runtime/Core/Gameplay/ArenaDirector.cs`：`BuildIfNeeded` 末尾加 3 行测量钩子（默认无操作）
- `ProjectSettings/ProjectSettings.asset`：`enableFrameTimingStats 0→1`（FrameTimingManager 前提，近零开销）；另含构建目标切换写入的 Standalone batching 默认项（静态批=1）
- `Materials/*.mat` 3 处：`_MainTex` 从空引用补上此前已设置的 `_BaseMap` 引用（材质修复的序列化落盘）
- 已还原与本轮无关的构建目标归一化文件（PC_RPAsset / DefaultVolumeProfile / URP GlobalSettings）

## 提交

- `main @ eee3c3d`（ArenaPerfHarness + ArenaDirector 钩子 + enableFrameTimingStats + 材质序列化补全 + 三档原始文件与 player-run.log + 本 SUMMARY）。
