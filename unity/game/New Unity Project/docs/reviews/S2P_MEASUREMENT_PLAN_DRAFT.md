# S2P 测量方案草稿（等门期间文档，不实施）

状态：草稿。换模门（R + 导演）放行前**不开跑、不改性能代码**。放行后按本清单开工。

## 目标

真实系统（Dark Knight + 真实 QWE / Support / 池 / 动画视图层）在 100 / 200 / 300 实体下仍有接近最终性能目标的路径。1440p / 120FPS 在本门追。失败 = 停扩内容、找瓶颈、重新过门。

## 测量环境要求

- 场景必须是当前 Dark Knight 玩家模型 + 真实技能/池/动画路径；**禁止空胶囊**、禁止 `-nographics` 空壳采样当 GPU 成绩。
- 分辨率 1440p；窗口全屏渲染，非 Game 视图缩放滑条。
- 采样前热身帧不计入。

## 操作

1. F1 / F2 / F3 生成 100 / 200 / 300 Dummy。
2. 按住 QWE 连发（真实施放负载），同时按住走。
3. F5 对当前密度采样一截；每档各采一截。
4. 记录是否进图（F6 流程不影响本测量）。

## 必出字段

| 类 | 字段 |
|---|---|
| 帧 | CPU 帧时 avg / p95 / max（ms）、主线程、渲染线程 |
| GPU | GPU 帧时（需 GPU 采样器；无则标 NOT_MEASURED 并说明） |
| GC | 每帧托管分配（`GetAllocatedBytesForCurrentThread` 差值） |
| 动画 | Animator 更新耗时（Profiler 标记） |
| AI | 转向 / Separate 间隔计算耗时 |
| 物理 | 碰撞矩阵与查询耗时 |
| 渲染 | VFX、Draw Call、SetPass、内存（Texture/Mesh/总计） |

输出：`Logs/s2p-density.txt`（沿用 s1-perf-arena.txt 的行格式，新增上述列）。

## 优化顺序（预写死，不得跳序）

AI Tick → Animator LOD（按距离降频/裁剪）→ 转向 → 弹道 → 阴影 → 灯 → VFX → 池。
仍失败才局部 Jobs / Burst。**禁止第一反应 Full DOTS**；禁止每对象 Update、每 Buff 一个 GO、弹道刚体、满员 NavMeshAgent。

## 顺带复验 S1 手感（不绑进本包，可打回）

按住走、按住连发、Q/E 施放 + 命中反馈。「输入重写」不做独立项目。

## 通过标准

真实路径下系统复杂度仍有接近最终性能目标的路径 → 过门，才谈 S3 / Art Bible / 正经套材质。

## 明确不做

技能库、大天赋树、Unique、Atlas、完整 Craft 链、Aura/Curse/Flask/ES/Block、DOTS、HDRP、FMOD、换装捏脸、武器战斗、程序化大地图、16 席流水线。
