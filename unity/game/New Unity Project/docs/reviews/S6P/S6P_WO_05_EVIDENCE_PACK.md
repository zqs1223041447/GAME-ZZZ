# S6P-WO-05 Evidence Pack

**Work Order:** S6P-WO-05 — Passive Tree Overview LOD & Texture Residency  
**Base:** `ef11cae`（04C CLOSED）  
**Implementation:** `2411250`（VRAM + 1920 LOD shots；前序 `22de41e` / `0edbe95` / `c4959d9` / `74dc522`）  
**状态：** CLOSED / ACCEPT WITH FOLLOW-UP（Channel A；下一令 NONE / STOP）

## EP-A Entry / identity

| 项 | 值 |
|---|---|
| Entry HEAD | `ef11cae` |
| Unity | 6000.3 |
| ProdSim 入口/出口 | `FNV1A64:99f1bfd3f81c4fe6` V3 |

## EP-B Semantic freeze

453 / 1574 / 87 / 315；CONSUMED 798；reachable 1985；\|D\|=411；disconnected 42；Mastery 22。WO-05 delta=0。

## EP-C LOD

`NormalProjectedPx = 46 * zoom * DesignScale`。&lt;8 Overview / [8,18) Mid / ≥18 Detail。7.999/8/17.999/18 测试 PASS。

## EP-F Chrome

`tools/evidence/wo05-chrome-geom.ps1` + `wo05-geom-oracle.html` + `tree_raw.json`。G0–G3 × 1920/2560：setMismatch=0，max centre/edge ≤ 0.0038 px。

## EP-G Hit

2429 centre probes mismatch=0。Fixtures 2172/71/183/10/1006 × 三档 5 probe PASS。

## EP-H Path

[2172,71,183] 三档 NodeState/Effect/Traversal 不变。

## EP-J Memory

见 `docs/qa/wo05/VRAM_FOUR_STATE.json`。CLOSED/LOD0 bytes=0。

**测量方法：** `PassiveTreeTextures.ResidentBytes` = 各 resident `Texture2D` 的 `width × height × 4`（按 RGBA32 估算）。**不是** GPU driver 真实 VRAM。unique Texture2D 数 = `ResidentIconCount + ResidentChromeCount`。load/unload 计数见 `ResourceLoadCalls` / `OwnerUnloadCalls`，由 `S6PWo05ResidencyTests` 机械核对。

## EP-K Nine-point

resident == required last station；无九站 union。

## EP-M 120 frames

同 plan Sync ×120：Load/unload 增量 0。

## EP-O Visual

| 视口 | LOD0 | LOD1 | LOD2 |
|---|---|---|---|
| 1920 | `lod0_1920.png` | `lod1_1920.png` | `lod2_1920.png` |
| 2560 | **NOT CAPTURED — machine geometry evidence available** | 同左 | 同左 |

2560 机械 geometry oracle（G0–G3_2560）已运行并 PASS；截图缺口不构成产品失败（Gate follow-up）。

## EP-Q ProdSim cold

```
Run1 | 2026-09-12 09:11:38 | 17.9s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run2 | 2026-09-12 09:11:52 | 14.2s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run3 | 2026-09-12 09:12:06 | 14.5s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Distinct hashes : 1
EXACT MATCH     : YES
```

## EP-R

未自行宣布 S6P CLOSED。successor 等 Channel A。
