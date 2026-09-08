# Art Performance R9 证据 SUMMARY（Gargoyle 优化双 Gate 冻结归档）

- 任务：S3-P5-ART-R9-GARGOYLE-PERFORMANCE-OPTIMIZATION。日期：2026-09-08。
- sourceCommit（Gate A 与 Gate B 完全一致）：**6572fbd432baed883023adf8c66cd4f4a87263cc**（perf+feat+test+audit 提交后 clean tracked HEAD）。
- 环境：M7 锁定硬件（Ryzen 7 5700X3D / RTX 5070）、2560×1440 fullscreen、D3D12、PC、vSync=0、targetFps=-1、warmup=60、sample=600、cast=0.12、预算 8.33ms、3 runs。
- Profile：formal-enemy-visual-stress-v1（0 semantic diff）。**Resolved formal visuals=4**（Troll/FireLion/Gargoyle/Bruce）；mix=25×4/50×4/75×4；fallback=0；material clones=0。

## Root Cause 与修复（§22 因果隔离结论）

| Suspect | 证据 | A/B Result | Verdict |
|---|---|---|---|
| **全项目 CPU 蒙皮（gpuSkinning=0）** | R8 FAIL：+75 Gargoyle（1.29M 蒙皮顶点，占总 145 万的 89%）→ main cpu +7.2ms；GPU 仅 +0.4ms；GC=0 | gpuSkinning 1 后 avg 9.1→3.8ms（**-58%**）、p99 14.9→6.2ms（**-58%**） | **PRIMARY ROOT CAUSE** |
| Rig 变换层级（optimizeGameObjects） | A/B avg 无变化（9.07-9.19 vs 9.06-9.14） | 无效（变换占比较小） | Secondary/no-op（保留=资产 hygiene） |
| 曲线密度（Optimal 压缩） | A/B avg 无变化（9.01-9.10） | 无效（按 §13 回退 KeyframeReduction 保留原始保真） | 排除 |
| Material/texture（GPU） | GPU 全程 ≤1.36ms | — | 排除（非首要） |

无 Secondary contributor 需处理（修复后余量充足）。

## Gate A（snapshot：gate-a/，MANIFEST 校验 OK，15 files）

| 层 | Verdict | 数据 |
|---|---|---|
| Canonical Performance | **PASS** | worst avg=1.54ms、worst p99=4.823ms（100 档偶发采样尖峰，p99 avg 档 2.3-2.5ms） |
| Art Performance | **PASS** | worst avg=2.542ms（30.5%）、worst p99=6.168ms（**74.0%**）、worst cpu=3.933ms、worst gpu=1.239ms；alive 289-295/300、95-100/100 |

## Gate B（snapshot：gate-b/，同 sourceCommit；MANIFEST 校验 OK）

| 层 | Verdict | 数据 |
|---|---|---|
| Canonical Performance | **PASS** | worst avg=1.497ms、worst p99=4.522ms |
| Art Performance | **PASS** | worst avg=2.552ms（30.6%）、worst p99=6.093ms（**73.1%**）、worst cpu=3.945ms、worst gpu=1.239ms；alive 292-295/300、99-100/100 |

Headroom：worst art p99=6.168ms=74.0% 预算——余量充足，非 PASS WITH LOW HEADROOM（阈值语义不变）。

## 结论边界

允许表述：在 M7 锁定硬件 / 2560×1440 canonical 环境下，四正式视觉 mix（Troll/FireLion/Gargoyle/Bruce）以 100/200/300 visual instances 完成双 Gate 性能验证（gpuSkinning 修复后）。
禁止表述：「所有未来模型都保证 300@120」「当前地图会刷 75 个 Gargoyle」。

## 视觉 QA

R9 改动均为引擎配置（gpuSkinning）+ 数据剥离（恒定 scale 曲线数学上零输出差异）+ optimizeGameObjects（骨骼层级合并，蒙皮矩阵不变）——视觉输出与 R8 QA 七张（Assets/Temp/qa/r8_*：五方同屏/Ignite/Hit-while-Ignite/恢复/6 只 Ashling 同屏/近距攻击/死亡）一致，无可见退化；待机姿态/翅膀/攻击/死亡帧目检正常（Gate 后目检）。
