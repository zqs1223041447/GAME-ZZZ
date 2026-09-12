# S6P-WO-05 Evidence Pack

**Work Order:** S6P-WO-05 — Passive Tree Overview LOD & Texture Residency  
**Base:** `ef11cae`（04C CLOSED）  
**Implementation:** `2411250`（VRAM + 1920 LOD shots；前序 `22de41e` / `0edbe95` / `c4959d9` / `74dc522`）  
**状态：** READY_FOR_GATE_REVIEW

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

## EP-K Nine-point

resident == required last station；无九站 union。

## EP-M 120 frames

同 plan Sync ×120：Load/unload 增量 0。

## EP-O Visual

`lod0_1920.png` `lod1_1920.png` `lod2_1920.png`。2560 Game View 未切到。

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
