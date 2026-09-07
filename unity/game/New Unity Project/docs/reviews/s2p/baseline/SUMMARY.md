# S2P 基线测量 SUMMARY

结论：**基线已记录**（不写「S2P 通过」——通过判定在下一拍 R 审核）

## 环境与前置

| 项 | 值 |
|---|---|
| 工程 commit | `main @ abb4a0a`（采样字段 PerfSampler.cs + 三档原始文件 + 本 SUMMARY；基线数据采集基于 ebbf782 之上） |
| Unity | 6000.3.23f1（URP，DX12，GUI Editor，非 batch、非 -nographics） |
| 分辨率 | Editor 内嵌 Game 视图（未锁定独立分辨率；正式 1440p 全屏成绩需独立测量轮） |
| 是否进图 | 否（Arena 本体，未走 F6 进图） |
| 玩家模型 | DarkKnight mounted（每帧包围盒贴地生效）；scale=0.244，身体净高 1.185 |
| 采样窗口 | 每档 60 帧预热 + 600 帧采样（F5 采样等同代码路径 `SampleDensity`，参数加长；热键以代码为准：F1/F2/F3 生成、F5 采样） |
| 技能是否在打 | **是**：运行时注入施法驱动（`EditorApplication.update` 闭包，未落盘、未提交），Q/W/E 轮换 ~8.3 次/秒，目标最近存活实体；木桩生命=2 会被击杀，存活 <50% 时按原数量自动重刷（种子固定 0xC0FFEE） |
| 池是否在用 | 是（弹道池、命中反馈池、死亡回收全程工作） |
| 工作方式 | Editor 前台化后采样；200 档首跑被窗口激活巨型卡顿污染（p999=6366ms），已前台化重跑替换 |

## 三档摘要（带施法行；括号内为无施法站桩参考）

| 实体 | 存活(末帧) | 主线程 avg | p95 | p99 | p999 | max | GC/帧 | CPU 帧(FrameTiming) | GPU | 内存(总分配) |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 100 | 72/100（站桩 100） | 2.581ms（2.425） | 3.230（2.823） | 3.810（3.112） | 6.446（4.011） | 8.710（4.287） | 0 B | 2.581ms | 0.106ms | 419.8 MB |
| 200 | 173/200 | 2.838ms | 3.495 | 4.119 | 4.655 | 4.748 | 0 B | 2.838ms | 0.104ms | 429.9 MB |
| 300 | 269/300（站桩 300） | 3.112ms（3.012） | 3.844（3.499） | 4.335（3.758） | 5.695（4.055） | 6.823（4.146） | 0 B | 3.113ms | 0.106ms | 422.4 MB |

原始数据：`100.txt` / `200.txt` / `300.txt`（每文件首行=带施法，第二行=站桩参考）。

## 字段覆盖说明（未采到字段 + 原因）

| 字段 | 状态 | 说明 |
|---|---|---|
| 主线程帧时 | 已采 | `Time.unscaledDeltaTime`（PerfSampler 原字段 + 新增 p99/p999） |
| CPU 帧时 / GPU | 已采 | `FrameTimingManager`（cpuFrameTime/gpuFrameTime，毫秒） |
| 渲染线程独立值 | 未采到 | FrameTiming 仅提供整帧 cpuFrameTime，无独立 render-thread 值 |
| GC/帧 | 已采 | `GetAllocatedBytesForCurrentThread` 差值（0 B/帧——池化路径零分配） |
| 内存 | 已采 | `Profiler.GetTotalAllocatedMemoryLong`（总分配 MB） |
| Animator / Separate / 转向 / 物理 / VFX 细分耗时 | 未采到 | 需插桩或开 Profiler 计数；R 规则禁止改 Tick 路径、开 Profiler 会改变被测对象，故记录为未采 |
| DrawCall | 未采到 | 无 Profiler 计数可用（同上，不为此开 Profiler） |
| 1440p/120FPS 正式判定 | 未采 | Editor 内嵌视图非独立全屏；本基线只作可复核基线，不作 120FPS 证据 |

## Runtime diff（本轮唯一代码改动，仅为补采样字段）

- `Assets/Runtime/Core/Gameplay/PerfSampler.cs`：`PerfRow` 新增 `MainMsP99/MainMsP999/MemTotalMb/FrameCpuMsAvg/FrameGpuMsAvg/FrameTimingAvailable`；`Sample` 增加 FrameTimingManager 读取（尽力而为，失败自动停用）；`Write` 追加列 `cpu_ms_avg,gpu_ms_avg,mem_total_mb,frame_timing_ok`（旧列保持原序在前）。未触碰 Tick、LOD、池、灯、阴影、技能、碰撞、贴地、缩放。

## 提交

- 本 SUMMARY 与三档原始文件 + PerfSampler 采样字段：`main @ abb4a0a`。

外观残留（必须记录）：身高/朝向最终审美待以后有人看，**不写「模型过」**；S1 手感门已按导演复测关闭。
