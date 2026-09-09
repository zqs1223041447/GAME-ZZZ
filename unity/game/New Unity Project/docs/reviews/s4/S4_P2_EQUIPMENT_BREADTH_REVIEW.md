# S4_P2_EQUIPMENT_BREADTH_REVIEW — 6 槽装备闭环（工作令 S4-P2-EQUIPMENT-BREADTH-GLOVES-BELT）

**日期**：2026-09-09　**Baseline**：main @ af5287c（S4 Phase 0/1 COMPLETE）→ 本轮 HEAD 见文末
**Verdict**：**PASS — S4 PHASE 2 EQUIPMENT BREADTH COMPLETE**（S4 Phase 2 = COMPLETE — 6-slot Equipment Breadth）

## 1. Preflight Truth Drift（§一）

- Current canonical counts：**Skills=3 / Supports=7**（`docs/qa/CONTENT_PRODUCTION_REPORT.json`，EditMode 测试再生；SupportId.Count=8 为含 `None` 哨兵，真实 Support 数=Count-1=7——`Catalogs.cs` 注释与 M1 sentinel 重构 commit 6014bc5 双实证）。
- Historical 7/8 source：**仅** `docs/reviews/s3/S3_OVERALL_CLOSEOUT_REVIEW.md`（行 14/25/62，commit 7edbb6f 引入）；全部同期审计文档（CONTENT_AUDIT_S2/S3_BATCH1/S3_R2/S3_CLOSEOUT、RUNTIME.md、STATUS）均为 **3 Active + 6→7 Support**。
- **Branch T1 — Historical/documentation count drift**：内容时间线只有加入（S3-R2 时 6→7 Support），从未有 Skill/Support 移除；护栏测试（PinCount、S2BaselineSupportCount=6+1、golden 3×7=21）从未变红——真实回退不可能穿过这些护栏。**Regression：无**（T2 不触发，无 CONTENT_COUNT_REGRESSION_BLOCKER）。
- 处理：不回写历史 review verdict；不新增内容凑数；canonical truth 以 Production Report 为准。历史 closeout 文件的 7/8 措辞按工作令保留原样（本节记录为 known reporting defect）。

## 2. Roadmap Truth（§二）

- `docs/ROADMAP.md` 页首阶段行 + 阶段表 S3/S4 行已更新为当前态：S3 COMPLETE / Voice DEFERRED BY DIRECTOR / S4 IN PROGRESS（Phase 0/1 COMPLETE、Phase 2 IN PROGRESS）。历史计划段落与历史 review verdict **未重写**。

## 3. EquipSlot（§三）

- Before：Weapon=0/Body=1/Helmet=2/Boots=3/Count=4（显式稳定值）。
- After：**追加 Gloves=4、Belt=5、Count=6**；旧四槽数值零漂移（`SliceDrawerTests.EquipSlot_CanonicalNumericIDs_Stable` 钉死 0-5 全表）。
- 孔数：GlovesSockets=1 / BeltSockets=1（新槽不映射任何技能孔位——SupportCapacity 仍只走 Weapon/Body/Helmet，零 gameplay 语义溢出）。

## 4. Architecture（§五/§六/§七）

- **4-slot assumptions found 及处理**（全仓 75 处 EquipSlot / 37 处 Equipped 引用逐条核对）：
  ①`Equipped=new int[4]`→`new int[(int)EquipSlot.Count]`（先于掉落启用，防越界）；②`SocketsFor`/`SlotName`/`ItemBaseName` 的 Boots default 兜底→补显式 Gloves/Belt 分支（否则新槽静默显示"靴子/旧靴"）；③`BuildSnapshot` 四字段→追加 GlovesId/BeltId（Capture 同步）；④抽屉 2×2→2×3（ColumnH 232→326，纯几何）；⑤Build 面板头部 4 卡单行→6 卡双行 3 列；⑥`SliceDrawerTests` 四槽钉死断言→六槽。
  天然适配零改动（实证）：ResetTown 初始化循环、TryEquip（`Equipped[(int)it.Slot]`）、RecalcPlayer/CollectSkillMods 聚合（按 `Equipped.Length` 遍历）、TryRandomCraft/TryDirectedCraft（槽位透传）、Tooltip 同槽对比 key=`(int)item.Slot`、DropGear 槽选取=`NextInt(LootRng,0,(int)EquipSlot.Count)`（canonical，自动扩 6）。
- Parallel equipment system：**0**（无 GlovesSystem/BeltSystem/EquipmentV2/第二套 crafting/新 inventory backend；全部复用 ItemInstance/EquipSlot/SliceSession 单一装备存储）。
- Base item changes：slot-driven 程序化 base（`ItemBaseName` 映射）→ 新槽仅 +2 条 base 映射（布手/皮带），**未建 base catalog**。
- Starter 装备：新槽起始为空（-1）——镇重置 RNG 流零变化，旧四槽首发行为零回归。

## 5. Loop（§十-§十四）

- Drop：canonical `NextInt(LootRng,0,(int)EquipSlot.Count)`，EditMode `Loot_DropPath_ReachesGlovesAndBelt` 经真实 OnKill→DropGear 种子扫描证明 **6 槽全部可达**（无 %4/Next(4)/硬编码四槽数组残留——审计已全仓排除）；RNG 取值序列变化=合法 content-set change（§27 如实申报，非 accidental drift）。
- Craft：TryRandomCraft（同槽重掷）/TryDirectedCraft 对新槽物品工作（`Craft_WorksOnGlovesAndBelt_SlotPreserved`）。
- Equip/Replace：空手套/腰带可穿、同槽替换覆盖、旧物按现有语义留背包（`Equip_Replace_Gloves/Belt_UpdatesAggregation*`）。
- Stat aggregation：新槽 Modifier 进 canonical RecalcPlayer/CollectSkillMods 聚合，替换后旧词缀移除（正反双向断言）。
- Build snapshot：**6 槽全含**（Capture 读 Equipped[0..5]；无持久化格式→§十五 **NO PERSISTENCE MIGRATION REQUIRED**，全仓无 Equipped 存档读写——审计实证）。
- Tooltip/Compare：R3 统一 tooltip/union 对比经 `Equipped[(int)item.Slot]` 天然覆盖新槽（`SameSlotComparison_WorksWithNewSlots`）；**无 GlovesTooltip/BeltTooltip**（§十六）。

## 6. UI（§十七-§二十二）

- Drawer：2 列 × 3 行六槽（ColumnH 232→326），`SliceDrawerLayout` 纯几何承载全部 rect（**零坐标回硬编码**）；tab 行不侵入（几何测试断言）。
- UI 展示顺序（§十九）：`SliceDrawerLayout.DisplayOrder`（Weapon|Helmet / Body|Gloves / Boots|Belt 人体逻辑）与 stable numeric ID 分离——`DisplayOrder_CoversAllSlots_ExactlyOnce` 证明 6 槽双射；未为 UI 顺序改枚举 ID。
- Drawer 交互（§二十）：六槽 hover/tooltip/click/occupied 全走既有 DrawMiniSlot 单渲染器；mini-slot click=打开现有 Build 面板，无装备旁路。
- World blocking（§二十一）：新槽 rect 全部在既有抽屉列内（几何测试），`BlocksWorldInput` 列级遮挡语义不变，无穿透。
- Responsive（§二十二）：设计空间 1920×1080 基准内 6 槽全可见（几何测试）；1440p=DesignScale 同比放大（既有测试锁定）；极窄窗口 clamp 下限 0.4（既有测试锁定）。本轮未跑像素级截图（IMGUI 布局为纯函数几何，测试级证据已覆盖）。

## 7. Production Tooling（§二十三/§二十四/§二十五）

- Production Report 已再生：**equipmentSlots: 4→6**；其它 canonical counts 零变化（skills=3/supports=7/affixes=13/passives=16/enemyKinds=5/mapMods=3——非 T1 修复需要，本来就没变）。
- Schema 未大改（§二十四）：slot count 已足够本轮审计价值，未加 slot names 列表（避免为 Phase 2 动 schema）。
- Content Audit fresh PASS（本轮再生）；Audit 无 slot-count parity 钉死（无手填第二份 expected=6 truth；报告由 `(int)EquipSlot.Count` 派生——`ProductionReport_DerivesSixSlotsCanonically` 钉死）。

## 8. Tests（§二十六/§二十八）

- EditMode +11（232/232）：
  - `SixSlotEquipmentTests`（新文件 9 项）：槽位元数据全 6 槽（无 Boots 兜底错名）/ 初始镇旧四槽在穿+新槽空 / RollItem 产手套腰带 / **掉落路径经真实 OnKill→DropGear 6 槽全覆盖** / 手套穿戴-替换-聚合双向 / 腰带同 / Craft 新槽闭环同槽不变 / 同槽 union 对比 / 6 槽快照 / Production Report 派生 6。
  - `SliceDrawerTests`：四槽几何断言→六槽（不重叠/列内/不侵 tab）；Enum ID 稳定 0-5；DisplayOrder 双射。
- PlayMode +1（10/10）：`SixSlot_EquipReplaceLoop_NoException`——ArenaDirector 真实会话内取建手套→穿→聚合→替换移除旧词缀→腰带同理→旧四槽保持→战斗 tick 无异常。

## 9. Verification（§三十三-§三十六）

- SelfTest：**PASS**（全项）。
- EditMode **232/232**；PlayMode **10/10**；Audit **fresh PASS failures=0**；Build **PASS win64**；PlayerRuntime **PASS exit=0 @2560×1440 densities=100/200/300**；tracked clean。
- Performance required：**NO**——装备操作=低频 item/UI path；未改 player per-frame stat aggregation 热路径结构（聚合仍按 Equipped.Length 单遍遍历，唯数组 4→6，量级不变）、未改 Arena 热循环/战斗结算。按 §三十五 不跑 canonical Performance。
- Art Gate：**N/A**——formal visual set / renderer / Animator / ProjectSettings rendering 全 0 diff（§三十六）。

## 10. Scope（§八/§三十）

- EquipSlot delta：**+2（Gloves/Belt）**；base 映射 +2。Skill/Support/Affix/Passive/Enemy/MapMod/StatId/ModOp delta：**0**。UI delta：抽屉 2×3+Build 头部 6 卡（布局内改版，无新屏）。Art delta：0。Audio delta：0。
- No New Gameplay Semantics（§三十）：手套/腰带=纯 Modifier 容器/装备位；无手套攻击系统/无 Flask belt/无护甲分区。Voice 继续 DEFERRED（§三十九）。Stage0 全锁（§三十八）。导演真相全保持（§三十七）。

## 11. Phase Status（§三十一）

- S4 Phase 0=COMPLETE；Phase 1=COMPLETE；**Phase 2=COMPLETE（6-slot Equipment Breadth）**；Phase 3=NOT STARTED（Affix breadth 不自行启动，§四十）；S4 overall=IN PROGRESS。S3=COMPLETE；Voice=DEFERRED BY DIRECTOR。

## 12. Reviewer Findings / Fixes

- Findings：①首跑 PlayMode 新测试断言自伤（把槽位号当背包索引用）——测试侧修正为 AddItem 索引对照，零产品代码改动；②EditMode 首跑 232/232 一次绿（六槽扩容未破坏任何既有契约）。
- Fixes：如上，无遗留。
- Final verdict：**PASS — S4 PHASE 2 EQUIPMENT BREADTH COMPLETE**

## 13. Remote Sync

- Commits：本轮 feat/test/docs 三笔（见 git log）；普通 push，未 force；Local HEAD=Remote HEAD（见完成汇报）。
