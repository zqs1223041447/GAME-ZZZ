# S1 PerformanceArena

S1 只要求能测量，不要求 1440p / 120FPS / 300 真实实体达标。

## 如何采

1. 打开 `Assets/Scenes/Arena.unity`，Play。
2. F1 / F2 / F3 生成 100 / 200 / 300 Dummy。
3. F5 对当前数量采样（warmup 20 + 90 帧），写 `Logs/s1-perf-arena.txt`。
4. PlayMode 测试 `PerformanceArena_WritesCostRows` 会连采三档。

列：Dummy 数、存活数、采样帧数、主线程帧时间 avg/p95/max（毫秒）、GC Alloc/帧（托管 `GetAllocatedBytesForCurrentThread` 差值）。

**batchmode `-nographics` 的帧时间不能当 GPU/渲染成绩。** NullGfxDevice 会把主线程时间压到 1ms 附近。席 R 请在 Editor Game 视图或独立播放器里按 F5 覆盖一版。

## 实测（实现席本地，2026-09-06）

### A. 逻辑-only（EditMode，无渲染）

`DummyCrowdTests.SimOnly_CanMeasure300`：300 Dummy × 120 tick = **6.12 ms** 合计（约 0.051 ms/tick）。热路径 60 tick 托管分配 < 2KB。

### B. PlayMode batchmode nographics（有 GO 视图同步，无真渲染）

来自 `Logs/s1-perf-arena.txt`（20 帧，方差大，只证明采样器能写出数字）：

| Dummy | alive | frames | main ms avg | p95 | max | GC alloc/帧 |
|---|---|---|---|---|---|---|
| 100 | 100 | 20 | 0.605 | 0.940 | 1.117 | 0 |
| 200 | 200 | 20 | 0.785 | 0.862 | 2.358 | 0 |
| 300 | 300 | 20 | 0.805 | 0.873 | 1.100 | 0 |

逻辑和托管分配都不是第一瓶颈。真帧时间要在有画面的播放里重采。

## 前 3 个瓶颈

1. **每 Dummy 一个 MeshRenderer + Transform 同步**：300 独立 GO，未 GPU instance / 未合网格。视图 Sync 是密度主成本。
2. **近圈 AI 每帧转向+位移**：LOD 只降远距；玩家站在圈内时大部分 Dummy 仍是每帧 Tick。
3. **URP 前向与方向光**：默认相机 MSAA、方向光阴影对多物体仍贵（Dummy 已关投射阴影；地面与玩家仍吃光照）。

不是 Combat Math、不是装备扫描、不是 300 NavMeshAgent（S1 没有这些路径）。

禁止为冲这张表上 Full DOTS。
