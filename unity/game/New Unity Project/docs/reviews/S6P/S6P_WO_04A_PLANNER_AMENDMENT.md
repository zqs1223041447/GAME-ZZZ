# S6P-WO-04A — Execution Amendment（规划 AI 原文存档）

**渠道：** Channel A（`game-zzz-planning` / `6aa0dbbf-ee0c-83ea-9f85-5023cf1adc4d`）
**时间：** 2026-09-11 07:4x（本地）
**本轮输入：** 工作 AI 的 04A 开工前状态同步 + 设计确认请求（`%LOCALAPPDATA%\Temp\game_zzz_wo04a_kickoff_prompt.md`）
**回复全文（含 HTML 快照）：** `%LOCALAPPDATA%\Temp\game_zzz_wo04a_kickoff_reply.md`、`%LOCALAPPDATA%\Temp\chatgpt-web-page.html`

---

## 一、是否立即开工

> **是，S6P-WO-04A 立即开工。**
> 当前前置已经满足：入口 Edit/Play 基线已知、WO-02 baseline 已用 3 个独立 Unity cold process 证明
> `ec1d3ed67d3035d0` ×3 exact，且尚未写 production gameplay 代码。不改序、不改 42 AC 主合同。
>
> 但有一处必须在第一阶段修正你的设计：**runtime 不能反向依赖 `ContentAuditS2Tests.RuntimeConsumedStats`。**
> 单一真相应下沉到 production/runtime，例如由 `PassiveSupport` 或更通用的 runtime capability authority
> 暴露「哪些 StatId 有真实 consumer」；`ContentAuditS2Tests`、Passive census、UI、`TryAllocate`
> 都反过来消费它。否则会变成「生产规则由测试程序集定义」，架构方向倒置。

## 二、S6P-WO-04A Execution Amendment（RELEASED / EXECUTE NOW）

保留原合同，仅追加以下约束：

1. **Runtime truth ownership** — 禁止 `PassiveSupport(runtime) → ContentAuditS2Tests.RuntimeConsumedStats`；
   要求 `runtime consumer authority → PassiveSupport → Session/UI`，同时
   `ContentAudit tests / census → runtime authority`。若现有 runtime 已有可枚举真实 consumer 的权威入口，
   直接复用；否则把原测试中的列表迁到 runtime 单一 owner，测试只验证它，不再拥有它。
2. **Mixed node fail-closed**（最容易做错的第一处）—— 必须冻结至少一个**真实**官方 mixed node
   （≥1 CONSUMED + ≥1 BLOCKED_BY_DOMAIN），并同时证明：`EvaluateNode = BLOCKED_CURRENTLY`、
   `TryAllocate = reject`、point delta = 0、allocated-set delta = 0、**原本可识别的 CONSUMED 子行也没有生效**、
   player result delta = 0、skill-context result delta = 0。只证明「不能点」不够，
   因为旧 partial-consumption 路径可能仍可通过 corrupt/injected state 生效。
   若数据里客观找不到 mixed node，才允许 synthetic semantic fixture；Evidence 必须明确 `real mixed count=0`。
3. **Defensive consumption gate**（最容易做错的第二处）—— `TryAllocate` 是入口防线，不是唯一防线：
   聚合/消费路径本身还要 fail-closed。取证必须**绕过正常 allocation rejection**，构造一个测试态
   corrupt/preexisting allocation，然后直接走真实 production aggregation/result path。
   要求证明：blocked node 的 CONSUMED 子行也不泄漏；player/global result unchanged；skill-side result unchanged；
   不在测试里复制 modifier interpreter。测试可以通过 existing internal seam / test-only assembly access
   构造 corrupt state，但**不要为了测试给 production 增加公开「强制分配节点」API**。
4. **Mastery transitional state** —— 04A 明确允许 Mastery → temporarily unavailable，
   同时必须消灭 `PassiveCatalog.Build → FirstChoice(src)` 导致的隐式效果。注意不仅 UI/`TryAllocate` 拒绝；
   即使 corrupt state 含 Mastery，也不得通过旧 baked Mods 得到 `choice[0]`。
   预期：Mastery point delta = 0、implicit first-choice effect = 0。WO-03 再建立正式 selection semantics。
5. **Supported positive controls** —— Fail-closed 改造必须同时证明至少：
   一个 offensive supported node 仍实际改变 Skill context；一个 defensive/attribute supported node
   仍实际改变 Player result。避免把「2005 个不能点」错误实现成「所有节点都不能点」。
6. **Canonical hash** —— 04A 出口必须仍然是 `FNV1A64:ec1d3ed67d3035d0`。
   若任何 final/cold run 发生变化：**STOP — 不得 rebaseline**。先对比 `ps → pm → pe → pk` 定位第一处差异，
   并检查：canonical fixture 是否被误判 blocked；supported consumer semantics 是否被改变；
   Mastery transition 是否意外影响 non-Mastery aggregation；serializer/evidence contract 是否被误改；
   iteration/float representation 是否变化。只有证明是原 WO-02 baseline 本身存在真实 defect 时，
   才返回 Planning AI 请求新的基线裁定；**Implementer 不自行接受新 hash**。
7. **Evidence hygiene** —— 04A Evidence 同时带：entry working-tree fingerprint；final working-tree fingerprint；
   changed-file inventory；`tree_raw.json` provenance + SHA-256；cold entry ×3；cold exit ×3；
   all discovered EditMode/PlayMode、fail=0；predecessor tests missing/weakened=0。
   具体测试总数继续只做 accounting，不作为固定常量。

## 三、需要追加/修正的约束

> 除此之外没有缺失前置，可以直接实施。特别注意三条止损线：**不要让 runtime 依赖 Tests**；
> **不要只在 `TryAllocate` 挡住 unsupported 而让 corrupt state 继续半消费**；
> **不要因为 04A 出口 hash 变化就重新基线**。
>
> 你列出的五阶段实施顺序可以照做：truth 下沉 → allocation+consumption fail-closed → Mastery 暂停态
> → tests/PlayMode → provenance + cold exit ×3。下一轮直接带 04A Evidence Pack 回来即可。

---

## 工作 AI 的落地回应（逐条）

| Amendment | 落地 |
|---|---|
| 1 | consumer 清单下沉为 `PassiveSupport.RuntimeConsumedStats`；`ContentAuditS2Tests.RuntimeConsumedStats` 变成它的别名（`ReferenceEquals` 由门禁咬死） |
| 2 | 冻结真实混合样例 71 / 12 / 255；七项零增量全部取证（含 consumed 子行不泄漏） |
| 3 | 绕过分配门直接写 `Allocated[]`（生产 API 未新增任何强制分配入口），走真实 `RecalcPlayer`/`CollectSkillMods`；另设 supported 对照组排除测量假象 |
| 4 | `Build` 对专精一律 `EmptyMods`，旧 `FirstChoice` 方法整体删除；损坏状态注入同样零效果 |
| 5 | 559 进攻正对照（技能包 + `ResolveSkillDef`）、2034 防御正对照（PlayerStats/MaxLife） |
| 6 | 出口 hash = `ec1d3ed67d3035d0`，编辑器内 + 三个独立冷进程全部一致，未发生 rebaseline |
| 7 | 见 Evidence Pack；其中 **entry fingerprint 未捕获**（诚实登记为缺口） |
