# Art Performance R7 证据 SUMMARY（双 Gate 冻结归档）

- 任务：S3-P5-ART-R7-FORMAL-VISUAL-DENSITY-GATE。日期：2026-09-08。
- sourceCommit（Gate A 与 Gate B 完全一致）：**321d9b42bbd6d0ecf4b5d3f23bf2e47ce88190cf**（feat+tooling 提交后 clean tracked HEAD）。
- 环境：AMD Ryzen 7 5700X3D / NVIDIA GeForce RTX 5070（M7 锁定硬件）、2560×1440 fullscreen、Direct3D12、quality=PC、vSync=0、targetFrameRate=-1、warmup=60、sample=600、castInterval=0.12、预算 8.33ms、3 runs、帧计时必须。
- Profile：`formal-enemy-visual-stress-v1`（selectionMode=DistinctMappedFormalVisuals、assignmentMode=RoundRobinByEnemyKind、gameplayKind=Dummy、useEnemyVisualPresenter=true）。
- Resolved formal visuals：**TrollWarriorVisual;FireLionVisual;BruceVisual**（来自 EnemyVisualCatalog+RuntimeResourcePaths，非本文复制 truth）。
- Visual mix（RoundRobinByEnemyKind）：100 档=34/33/33；200 档=67/67/66；300 档=100/100/100。

## Gate A（snapshot：gate-a/，MANIFEST 校验 OK，15 files）

| 层 | Verdict | 备注 |
|---|---|---|
| Canonical Performance | **PASS** | worst avg=1.884ms、worst p99=2.572ms（Dummy workload 不变） |
| Art Performance | **PASS** | worst avg=6.031ms（72.4%）、worst p99=7.483ms（89.8%）、worst cpu=6.035ms、worst gpu=0.924ms |

## Gate B（snapshot：gate-b/，MANIFEST 校验 OK，15 files；同 sourceCommit）

| 层 | Verdict | 备注 |
|---|---|---|
| Canonical Performance | **PASS** | worst avg=1.874ms、worst p99=2.596ms |
| Art Performance | **PASS** | worst avg=6.056ms（72.7%）、worst p99=7.628ms（91.6%）、worst cpu=6.059ms、worst gpu=0.926ms |

## Art 运行细节（双 Gate 一致性观察）

- visual_instances 每档=density（100/200/300），visual_mix 求和==density（机器可读校验通过）。
- alive≥95%（300 档 285-297/300，kill-then-refill canonical 策略不变）。
- GC/memory 与 renderer/skinned/material slots/triangles 观察值见各 run 证据头（无独立硬门槛；无 OOM/崩溃）。

## 结论边界（§53 措辞约束）

允许表述：**在 M7 锁定硬件 / 2560×1440 canonical 环境下，当前正式 Enemy Visual Stress Mix（Troll / FireLion / Bruce）以 100/200/300 visual instances 完成双 Gate 性能验证**。
禁止表述：「所有未来模型都保证 300@120」；「当前地图会实际刷 100 个 Bruce」。
