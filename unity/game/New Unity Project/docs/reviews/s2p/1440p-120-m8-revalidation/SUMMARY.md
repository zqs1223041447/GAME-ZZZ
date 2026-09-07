# S2P 1440p/120 M8 PROVENANCE-COMPLETE REVALIDATION EVIDENCE（冻结历史证据）

日期：2026-09-08。状态：**M8 Gate A = PASS、Gate B = PASS（同一 Evidence HEAD）→ M8 provenance-complete revalidation = PASS**。

## 语义边界（先读）

- **M8 不重新定义性能预算，不替代 M7 历史 closure**。M7（2026-09-08，locked hardware）的 1440p/120 CLOSED 结论保持不变。
- M8 目的：在**相同 locked contract**（`docs/qa/PERFORMANCE_GATE.json`，未改动）下，用**两个 canonical Performance Gate 的完整原始证据**（3 Run × {100/200/300.txt + PlayerRun.log} + verification-summary.json + 契约快照 + MANIFEST.json SHA-256 逐文件）对 M7 的当前性能结论做 **provenance-complete revalidation**——消除 M7「Gate A 只有抄表数字、Gate B 才有原始文件」的证据不对称（该历史边界如实保留于 `1440p-120-m7/SUMMARY.md`，不补造 M7 Gate A 原件）。
- 数字波动说明：M8 不要求与 M7 数字一致（自然波动），只要求两个 canonical Gate 都 PASS。

## Evidence Source HEAD（两 Gate 共同）

| 项 | 值 |
|---|---|
| sourceCommit（MANIFEST 双 Gate 一致） | a11aa8737669e188f453345b18414855c36d0955 |
| Unity | 6000.3.23f1 |
| OS | Windows 11 Pro（10.0.26100） |
| CPU（locked，未变） | AMD Ryzen 7 5700X3D 8-Core Processor |
| GPU（locked，未变） | NVIDIA GeForce RTX 5070 |
| 实测环境（两 Gate 一致） | 2560×1440 fullscreen / Direct3D12 / PC / vSync=0 / targetFps=-1 |
| Contract | `PERFORMANCE_GATE.json` 快照在每个归档内（schemaVersion 1，与 M7 锁定值逐字段一致） |

HEAD 说明：a11aa87 之前的同日提交链 = 92a71a9（ProjectSettings define 移除，被实测证伪）→ ead815c（snapshot Operator 工具）→ 3a577d3（docs 真相修正）→ a11aa87（define 振荡矩阵实测收敛：**Quick 门=移除 define / Performance·Build 门=回写 define**，单 HEAD 态无法同时满足两类门零污染；Performance 门终态=含 define，故 Evidence HEAD 取含 define 态）。

## Gate A（HEAD=a11aa87，canonical `-IncludePerformance`，exit=0，PerformanceVerdict=PASS）

| Run | 100（avg / p99 / cpu / gpu / alive） | 200（同） | 300（同） |
|---|---|---|---|
| 1 | 1.343 / 1.990 / 1.343 / 0.561 / 100/100 | 1.443 / 2.105 / 1.442 / 0.564 / 200/200 | 1.671 / 2.355 / 1.670 / 0.557 / 290/300 |
| 2 | 1.348 / 1.941 / 1.347 / 0.578 / 100/100 | 1.467 / 2.111 / 1.467 / 0.580 / 200/200 | 1.657 / 2.458 / 1.657 / 0.574 / 291/300 |
| 3 | 1.342 / 4.319 / 1.342 / 0.577 / 100/100 | 1.467 / 2.049 / 1.467 / 0.580 / 200/200 | 1.640 / 2.223 / 1.639 / 0.575 / 290/300 |

Verdict：Environment=PASS；PerformanceVerdict=**PASS**（9/9）。完整原始证据冻结于 `gate-a/`（MANIFEST.json + PERFORMANCE_GATE.json + verification-summary.json + Run1-3/{100,200,300.txt,PlayerRun.log}，SHA-256 逐文件校验通过）。

## Gate B（同一 HEAD=a11aa87，canonical `-IncludePerformance`，exit=0，PerformanceVerdict=PASS）

| Run | 100（avg / p99 / cpu / gpu / alive） | 200（同） | 300（同） |
|---|---|---|---|
| 1 | 1.348 / 1.997 / 1.347 / 0.561 / 100/100 | 1.454 / 2.092 / 1.451 / 0.561 / 200/200 | 1.678 / 2.263 / 1.678 / 0.561 / 290/300 |
| 2 | 1.366 / 4.380 / 1.367 / 0.561 / 100/100 | 1.455 / 2.153 / 1.456 / 0.563 / 200/200 | 1.643 / 2.274 / 1.641 / 0.559 / 290/300 |
| 3 | 1.344 / 2.053 / 1.344 / 0.577 / 100/100 | 1.471 / 2.052 / 1.472 / 0.578 / 200/200 | 1.640 / 2.254 / 1.640 / 0.574 / 291/300 |

Verdict：Environment=PASS；PerformanceVerdict=**PASS**（9/9）。完整原始证据冻结于 `gate-b/`（同结构，VerifyArchive OK）。

## Aggregate（2 Gate × 3 Run × 3 Density = 18 次测量，全部 PASS）

- Worst main avg：1.678 ms（Gate B Run1/300）= 预算 8.33 的 20.1%
- Worst main p99：4.380 ms（Gate B Run2/100，单帧尖峰；仍 = 预算 52.6%）
- Worst cpu avg：1.678 ms；Worst gpu avg：0.580 ms（均 >0 且 ≤8.33）
- Alive validity：300 档 290-291/300（96.7%-97.0% ≥95%）；100/200 档满员
- FrameTiming：18/18 available
- **当前状态：S2P 1440p/120 = CLOSED（M7 locked hardware）不变；M8 provenance-complete revalidation = PASS**

## 快照链与操作记录

1. Gate A 第一次尝试（HEAD=3a577d3）**作废未冻结**：门结束后 ProjectSettings.asset 出现 define 回写（ProjectSettings 振荡矩阵实测，快照 Operator 按 M8 契约拒收脏树）——该尝试的数字未进入任何冻结证据。
2. define 终态收敛提交 a11aa87 后重跑 Gate A（干净树起步、终态树干净）→ `gate-a` 快照 14 文件 + VerifyArchive OK。
3. Gate B 同 HEAD → `gate-b` 快照 14 文件 + VerifyArchive OK。
4. `MANIFEST.json`（每 Gate）：schemaVersion=1、sourceCommit=a11aa87…、contractSha256、verificationSummarySha256、expectedRuns=3、expectedDensities=[100,200,300]、files[] 逐文件 SHA-256+bytes；`-VerifyArchive` 篡改可检（SelfTest 1c 夹具证明）。
5. 快照 Operator= `tools/snapshot_performance_evidence.ps1`（SelfTest 9/9）；Verifier（`verify_unattended.ps1`）与 Operator 职责分离=DECISIONS 长期规则⑨（Verifier≠Historian）。

## 边界说明

- 结论仍仅适用锁定硬件（Ryzen 7 5700X3D / RTX 5070）与 canonical 契约（2560×1440/D3D12/PC/vSync0/targetFps-1）；**不构成「所有 Windows PC 都保证 120FPS」**；其它机器须按 contract 显式更新并重建 baseline。
- 历史 1080p 证据、M7 冻结包（含其 Gate A raw 缺失的历史边界）零改动。
