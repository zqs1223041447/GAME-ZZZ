# S6P-WO-04C Evidence Pack

**Work Order:** S6P-WO-04C — Existing-Consumer Semantic Closure  
**Base commit:** `8959a60`  
**Implementation commit:** `e1d8eea`  
**Channel A 合同正文：** 未补发（镜像站登录失效）。实现按 04B RELEASED + 原 WO-04「只接已有 consumer」开工。

## EP-A Entry

| 项 | 值 |
|---|---|
| Entry fingerprint | `7310214b9657450a81281496048d22949e4b64a232b17bcdb3782621c097e95e` dirty=NO @ `8959a60` |
| Entry EditMode/PlayMode | 506/506 · 21/21（WO-04B ACCEPT 态） |
| Entry ProdSim | `FNV1A64:99f1bfd3f81c4fe6` V3 |

## EP-B Census bucket

入口松散关键词桶 **48** unique lines（`docs/qa/WO_04C_EXISTING_CONSUMER_GAP.json`）。含必须排除的 Minion / Iron Reflexes。

接线后 remaining = **AreaDamageMore × 2**（故意不接 Increased；`docs/qa/WO_04C_EXISTING_CONSUMER_REMAINING.json`）。

## EP-C Parser wiring（无新 StatId）

| 句式 | StatId | Op | Scale |
|---|---|---|---|
| `N% increased maximum Life` | Life | Increased | 0.01 |
| `N% increased maximum Mana` | Mana | Increased | 0.01 |
| `N% increased Strength/Dexterity/Intelligence` | 对应属性 | Increased | 0.01 |
| `+N to Armour` | Armour | Flat | 1 |
| `+N to Evasion Rating` | Evasion | Flat | 1 |
| `+N to Accuracy Rating` | Accuracy | Flat | 1 |
| `+N% to maximum Fire Resistance` | MaxFireResistance | Flat | 0.01 |

负向：Minion 前缀、`while` 条件、Converts 转换、`increased Area Damage` 全部 0 modifiers。

## EP-D Census delta（显式，非静默）

见 `docs/qa/WO_04C_CENSUS_DELTA.json` 与 `S6P_WO_04C.md` 表。

| 量 | before | after | delta |
|---|---|---|---|
| FULLY_SUPPORTED | 367 | 453 | +86 |
| UNFULFILLED | 1660 | 1574 | -86 |
| special / mastery | 87 / 315 | 87 / 315 | 0 |
| CONSUMED 行 | 607 | 798 | +191 |
| 可达 | 1985 | 1985 | 0 |
| \|D\| | 325 | 411 | +86 |
| start-disconnected | 42 | 42 | 0 |
| mixed | 211 | 259 | +48 |
| 04A2 候选集 | 311 | 44 | -267 |
| R_legacy | 14 | 367 | +353 |
| fixture 183 / [2172,71,183] | 同 | 同 | 0 |

reason：exact-uncond 已有 consumer 接入；mixed 整节点仍 UNFULFILLED；TraversalTruth 不动。

## EP-E Consumption proof

`FullySupportedLifeIncreased_RaisesMaxLife`：FULLY_SUPPORTED 且含 Life Increased 的节点注入 Allocated 后 `MaxLife` 上升。  
Mastery +30 断言改为 `RawFlat +30`（Get 会再乘路径上新接入的 Life Increased）。

## EP-F Forbidden

新 StatId / AreaDamageMore Increased / Minion 域 / Jewel / Timeless / Mastery 选择语义 / LOD / Save / rebaseline = NONE。

## EP-G Gates（同进程）

| 项 | 值 |
|---|---|
| 编译 | 0 error |
| EditMode | **512 / 512** |
| PlayMode | **21 / 21** |
| Content Audit | PASS fresh |
| ProdSim 同进程 | `FNV1A64:99f1bfd3f81c4fe6` **UNCHANGED**（canonical 559/1795/2034/2172 原已 FULLY_SUPPORTED） |

## EP-H Cold ProdSim ×3

EXACT MATCH YES。未 rebaseline。

```
Run1 | 2026-09-11 23:41:14 | 15.7s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run2 | 2026-09-11 23:41:28 | 14.2s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Run3 | 2026-09-11 23:41:42 | 14.4s | exit=0 | contract=V3 | invalid=0 | FNV1A64:99f1bfd3f81c4fe6
Distinct hashes : 1
EXACT MATCH     : YES
```

## EP-I Handoff

`S6P-WO-04C = READY_FOR_GATE_REVIEW`  
`successor WO-05 = BLOCKED_PENDING_CHANNEL_A_GATE_REVIEW`  
规划渠道登录当前失效：已 `chatgpt_ask.py open` + `ask` 两次，均「未检测到已登录状态」。登录恢复后把本 Evidence Pack 发回 Channel A。
