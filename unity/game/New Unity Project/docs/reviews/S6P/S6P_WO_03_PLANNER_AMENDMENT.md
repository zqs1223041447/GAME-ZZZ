# S6P-WO-03 Execution Amendment（Channel A 原文摘录）

**渠道：** Channel A `game-zzz-planning-2` / `6aa368c5-7478-83ea-a2d3-95003ee4e6ab`
**时间：** 2026-09-11
**输入：** 开工简报（远端 `c16d129`，仓库 https://github.com/zqs1223041447/GAME-ZZZ）
**回复全文：** `%LOCALAPPDATA%\Temp\game_zzz_wo03_start_reply.md`

## 裁定

- **S6P-WO-03 = EXECUTE NOW**（继续有效）。不插新 WO、不改队列：`WO-03 → WO-04B → [WO-04C] → WO-05`。
- 下一次只收 WO-03 Evidence Pack 做 Gate Review。

## Execution Amendment（4 条，一次性）

### EA-1 ChoiceKey 优先级

不能无条件使用 `MasteryNodeId + ordinal`。合同原文：source stable ID 优先；只有官方 choice 没有 stable ID 时，才 fallback 到 `MasteryNodeId + authoritative source ordinal`。

GUI index、localized text、`GetHashCode()`、runtime object identity 全禁。

**实现取证：** `passive_tree.json` 的 `choices` 字段只有换行文本，没有 per-choice ID。因此 `PassiveSupport.SourceChoiceHasStableId = false`，scheme = `fallback:MasteryNodeId+sourceOrdinal (source choice IDs=NONE)`。

### EA-2 Mastery 不升级成普通 traversal node

不得把 Mastery 的静态 `TraversalTruth` 改成 `TRAVERSABLE`。它继续是 `SPECIAL_BLOCKED`。打开 selector / explicit commit 是专门的 Mastery transaction，不是普通树路径分配。

**即使成功选中，Mastery 也不得成为后续节点的 transit vertex。** `reachable=1985`、`|D|=325`、42 个 start-disconnected supported nodes 是 topology invariant；任一变化 = STOP / investigate，禁止静默更新。

### EA-3 BlockedAllocatedCount 合法例外

成功 Mastery commit 会形成合法 allocation，但静态 TraversalTruth 仍为 SPECIAL_BLOCKED。计数必须只把「没有合法 explicit-selection authority 却出现在 allocated state 的 special-blocked node」算异常。

测试注入的裸 Mastery / Jewel / Timeless corruption 仍必须能被抓到。不得借此给 Jewel/Timeless 开例外。

### EA-4 367/315 不是实施目标值

不得为了旧 preflight 曾预测「22 个会移动」而强行改 NodeTruth/census。若新实现自然导致 legacy diagnostic census 变化，就按 before/after/delta/reason 报；若仍为 367/315，也完全合法。

选择能力属于 Mastery choice/state truth，不能为了 census 漂亮而破坏 04A2 的静态 Effect/Traversal truth。

**本轮选择：** 保持 367/315（delta=0，reason=EA-4）。

## §13 fallback

是当前数据下唯一合法敏感性口径。禁止伪造 Choice A/B。冻结 node 10：eligible+unselected vs explicitly selected（+30 Life）必须改变 gameplay/hash；selector open/hover/cancel 必须 hash 不变。
