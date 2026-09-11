# S6P-WO-04C — Existing-Consumer Semantic Closure

**状态：** CLOSED / ACCEPT WITH FOLLOW-UP（Channel A 2026-09-12；Follow-up = 纯证据 disposition 表，不改 parser）  
**入口：** HEAD `8959a60` / fingerprint `7310214b…` dirty=NO  
**ProdSim：** 同进程 + 冷进程 ×3 均为 `FNV1A64:99f1bfd3f81c4fe6` EXACT（V3，未 rebaseline）  
**门：** EditMode **512/512**、PlayMode **21/21**、Content Audit PASS

## 做了什么

1. 入口 census：松散关键词桶 48 行（`docs/qa/WO_04C_EXISTING_CONSUMER_GAP.json`）。含 Minion / Iron Reflexes 假阳性。
2. `PoeStatParser` 只加 `^...$` 无条件句式：increased max Life/Mana、increased Str/Dex/Int、flat Armour/Evasion/Accuracy、max Fire Resistance。
3. 故意不接 `increased Area Damage`（`AreaDamageMore` 只走 `RawMore`）。
4. remaining 桶 = AreaDamageMore × 2（`docs/qa/WO_04C_EXISTING_CONSUMER_REMAINING.json`）。

## census before / after / delta / reason

| 量 | before | after | delta | reason |
|---|---|---|---|---|
| FULLY_SUPPORTED | 367 | 453 | +86 | 整节点原先唯一阻塞行就是本次接线句式 |
| UNFULFILLED | 1660 | 1574 | -86 | 同上 |
| special / mastery | 87 / 315 | 87 / 315 | 0 | 不属本令 |
| CONSUMED 行 | 607 | 798 | +191 | 同一句式出现在多个节点；入口 48 是去重后的 unique line |
| 可达 | 1985 | 1985 | 0 | TraversalTruth 不动 |
| \|D\| | 325 | 411 | +86 | 新增 supported 全部在 start-connected 域 |
| start-disconnected supported | 42 | 42 | 0 | 未解孤岛 |
| mixed（c>0 且 b>0） | 211 | 259 | +48 | 原先全 blocked 的节点现含 CONSUMED 子行，仍 UNFULFILLED |
| 04A2 候选集 | 311 | 44 | -267 | 新增 FULLY_SUPPORTED 扩大「仅 effective 可 transit」的 legacy 可达 |
| effective-only 可达（04A2 R_legacy） | 14 | 367 | +353 | 同一原因；runtime 可达仍 1985 |
| 04A2 fixture | 183 / [2172, 71, 183] | 同 | 0 | DiscoverFixture 仍返回同一 target |

## 没做什么

新 StatId、AreaDamageMore Increased、Minion、Jewel/Timeless、Mastery 选择语义、LOD、存档。
