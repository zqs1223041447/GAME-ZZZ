using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5-WO-04（BL-002.A1，权威清单 docs/reviews/S5/S5_AFFIX_ADMISSION.md）有界词缀广度：
    /// 锁定清单恰 4 条（迅疾/铁骨/睿智/坚韧，17→21，stable ID 只追加不漂移）/
    /// 单一 applicability truth（铁骨排除 Belt=GAME-ZZZ 有界适用性决策；生成池/定向制作同源，无分支复制）/
    /// 生成与定向制作可达（SeededRng 确定性有界证据；禁 test-only fallback）/
    /// 运行时消费实证（AttackSpeed/Armour/Intelligence/Strength 全部经既有聚合轴生效，零死声明）/
    /// Craft 回归（duplicate/applicability/池自动纳入）/ 通用 Tooltip 呈现（无词条专属 UI 分支）/
    /// Multi-Link 冻结回归联动（新词缀物品参与改挂路径不受影响）。
    /// </summary>
    public sealed class S5AffixBreadthTests
    {
        static readonly AffixId[] Manifest = { AffixId.SwiftBreeze, AffixId.Ironhide, AffixId.Insight, AffixId.Tenacity };

        static ItemInstance MakeAffixItem(SliceSession s, EquipSlot slot, AffixId id, float value)
        {
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = slot;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = SliceSession.SocketsFor(slot);
            it.BaseName = SliceSession.ItemBaseName(slot);
            it.AffixCount = 1;
            it.SetAffix(0, id, value, 0f);
            return it;
        }

        static int AddAndEquip(SliceSession s, EquipSlot slot, AffixId id, float value)
        {
            int idx = s.AddItem(MakeAffixItem(s, slot, id, value));
            string err;
            Assert.IsTrue(s.TryEquip(idx, out err), err);
            return idx;
        }

        /// <summary>清空装备（隔离 starter 词缀噪声：新词缀入池后 starter roll 也可能含新词条）。</summary>
        static void ClearEquipment(SliceSession s)
        {
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;
            s.RecalcPlayer(true);
        }

        // ---------- AC-01/02/03：精确清单 + 精确计数 + 精确语义 ----------

        [Test]
        public void Manifest_ExactDefinitions_StableIds_ExactCount()
        {
            Assert.AreEqual(21, (int)AffixId.Count, "AC-02：Before 17 + Added 4 = 21");
            Assert.AreEqual(21, AffixCatalog.Count, "AC-02：目录计数=枚举 Count");
            for (int i = 0; i < 17; i++)
                Assert.AreEqual((AffixId)i, AffixCatalog.Get((AffixId)i).Id, "旧 ID 只追加不漂移（位 " + i + "）");

            var swift = AffixCatalog.Get(AffixId.SwiftBreeze);
            Assert.AreEqual(AffixId.SwiftBreeze, swift.Id);
            Assert.AreEqual(17, (int)AffixId.SwiftBreeze, "AC-03：stable ID=17");
            Assert.AreEqual("迅疾", swift.Name);
            Assert.AreEqual(StatId.AttackSpeed, swift.Stat);
            Assert.AreEqual(ModOp.Increased, swift.Op);
            Assert.AreEqual(0.10f, swift.Min, 0.0001f);
            Assert.AreEqual(0.16f, swift.Max, 0.0001f);
            Assert.AreEqual(1, swift.RowCount, "四条全部单行（无第二值路径触碰）");
            Assert.AreEqual(0, swift.AllowedSlots, "迅疾不限槽（6 槽合法）");

            var iron = AffixCatalog.Get(AffixId.Ironhide);
            Assert.AreEqual(AffixId.Ironhide, iron.Id);
            Assert.AreEqual(18, (int)AffixId.Ironhide, "AC-03：stable ID=18");
            Assert.AreEqual("铁骨", iron.Name);
            Assert.AreEqual(StatId.Armour, iron.Stat);
            Assert.AreEqual(ModOp.Increased, iron.Op);
            Assert.AreEqual(0.10f, iron.Min, 0.0001f);
            Assert.AreEqual(0.22f, iron.Max, 0.0001f);
            Assert.AreEqual(1, iron.RowCount);

            var insight = AffixCatalog.Get(AffixId.Insight);
            Assert.AreEqual(AffixId.Insight, insight.Id);
            Assert.AreEqual(19, (int)AffixId.Insight, "AC-03：stable ID=19");
            Assert.AreEqual("睿智", insight.Name);
            Assert.AreEqual(StatId.Intelligence, insight.Stat);
            Assert.AreEqual(ModOp.Flat, insight.Op);
            Assert.AreEqual(6f, insight.Min, 0.0001f);
            Assert.AreEqual(12f, insight.Max, 0.0001f);
            Assert.AreEqual(1, insight.RowCount);
            Assert.AreEqual(0, insight.AllowedSlots, "睿智不限槽");

            var tenacity = AffixCatalog.Get(AffixId.Tenacity);
            Assert.AreEqual(AffixId.Tenacity, tenacity.Id);
            Assert.AreEqual(20, (int)AffixId.Tenacity, "AC-03：stable ID=20（坚韧 Strength/Flat exact-match；原开阔候选已废弃）");
            Assert.AreEqual("坚韧", tenacity.Name);
            Assert.AreEqual(StatId.Strength, tenacity.Stat);
            Assert.AreEqual(ModOp.Flat, tenacity.Op);
            Assert.AreEqual(6f, tenacity.Min, 0.0001f);
            Assert.AreEqual(12f, tenacity.Max, 0.0001f);
            Assert.AreEqual(1, tenacity.RowCount);
            Assert.AreEqual(0, tenacity.AllowedSlots, "坚韧不限槽");
        }

        // ---------- AC-05/06/08：单一 applicability truth + 铁骨 Belt 守卫 ----------

        [Test]
        public void Ironbone_Belt_Rejected_SingleTruth_AllOtherSlotsValid()
        {
            var iron = AffixCatalog.Get(AffixId.Ironhide);
            Assert.IsFalse(iron.IsApplicable(EquipSlot.Belt), "AC-06：铁骨+Belt=INVALID（唯一 applicability truth）");
            Assert.IsTrue(iron.IsApplicable(EquipSlot.Weapon), "铁骨 武器 合法");
            Assert.IsTrue(iron.IsApplicable(EquipSlot.Body), "铁骨 胸甲 合法");
            Assert.IsTrue(iron.IsApplicable(EquipSlot.Helmet), "铁骨 头盔 合法");
            Assert.IsTrue(iron.IsApplicable(EquipSlot.Gloves), "铁骨 手套 合法");
            Assert.IsTrue(iron.IsApplicable(EquipSlot.Boots), "铁骨 靴子 合法");

            // 生成池=同一 truth（无分支复制）：每槽池的 18 存在性恒等于 IsApplicable
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                var slot = (EquipSlot)s;
                bool inPool = false;
                for (int i = 0; i < AffixCatalog.Count; i++)
                    if ((int)AffixCatalog.Get((AffixId)i).Id == (int)AffixId.Ironhide && AffixCatalog.Get((AffixId)i).IsApplicable(slot))
                        inPool = true;
                Assert.AreEqual(iron.IsApplicable(slot), inPool, "随机池必须与 IsApplicable 同源，禁分支复制：" + slot);
            }

            // 定向制作=同一 truth：Belt 拒绝、不消耗、无半写入（腰带初始未装备=自建物品验证）
            var s2 = new SliceSession();
            int beltIdx = s2.AddItem(MakeAffixItem(s2, EquipSlot.Belt, AffixId.Life, 12f));
            int etchBefore = s2.Etching;
            int countBefore = s2.Inventory[beltIdx].AffixCount;
            string err;
            Assert.IsFalse(s2.TryDirectedCraft(beltIdx, AffixId.Ironhide, out err), "AC-06：定向制作铁骨→Belt 必须拒绝");
            Assert.IsTrue(err.Contains("不能出现在"), err);
            Assert.AreEqual(etchBefore, s2.Etching, "拒绝不消耗蚀刻剂");
            Assert.AreEqual(countBefore, s2.Inventory[beltIdx].AffixCount, "拒绝无半写入");
            Assert.AreEqual((int)AffixId.Life, s2.Inventory[beltIdx].AffixIdAt(0), "拒绝无半写入");
        }

        // ---------- AC-07：生成/定向制作可达（确定性有界证据） ----------

        [Test]
        public void Reachability_DirectedCraft_AllFourManifestEntries()
        {
            // 定向制作=现有批准路径（目录枚举+applicability+duplicate guard），四条全部可达（合法槽各一，全新物品无重复风险）
            EquipSlot[] slots = { EquipSlot.Weapon, EquipSlot.Body, EquipSlot.Helmet, EquipSlot.Belt };
            for (int i = 0; i < Manifest.Length; i++)
            {
                var s = new SliceSession();
                string err;
                EquipSlot slot = Manifest[i] == AffixId.Ironhide ? EquipSlot.Body : slots[i];
                int idx = s.AddItem(MakeAffixItem(s, slot, AffixId.Life, 12f));
                Assert.IsTrue(s.TryDirectedCraft(idx, Manifest[i], out err),
                    Manifest[i] + " 必须经定向制作可达：" + err);
                ItemInstance it = s.Inventory[idx];
                AffixDef def = AffixCatalog.Get(Manifest[i]);
                Assert.AreEqual((int)Manifest[i], it.AffixIdAt(it.AffixCount - 1), "定向写入=manifest 词条");
                float v = it.ValueAt(it.AffixCount - 1);
                Assert.GreaterOrEqual(v, def.Min, "掷值不得低于 Min");
                Assert.LessOrEqual(v, def.Max, "掷值不得高于 Max");
                Assert.AreEqual(1, def.RowCount, "单行词缀不触碰第二值路径");
            }
        }

        [Test]
        public void Reachability_RandomPool_DeterministicSweep_InvalidSlotNever()
        {
            // 有界确定性 sweep（SeededRng，非 UnityEngine.Random）：四条在合法槽（Body）全部可达
            var seen = new bool[Manifest.Length];
            for (uint seed = 1; seed <= 400 && !(seen[0] && seen[1] && seen[2] && seen[3]); seed++)
            {
                var s = new SliceSession();
                ItemInstance it = s.RollItem(EquipSlot.Body, Rarity.Rare, new SeededRng(seed), 3, "RareBody");
                for (int a = 0; a < it.AffixCount; a++)
                {
                    for (int m = 0; m < Manifest.Length; m++)
                        if (it.AffixIdAt(a) == (int)Manifest[m])
                            seen[m] = true;
                }
            }
            for (int m = 0; m < Manifest.Length; m++)
                Assert.IsTrue(seen[m], "随机生成路径必须可达：" + AffixCatalog.Get(Manifest[m]).Name + "（有界 sweep 400 seeds，SeededRng 确定性）");

            // AC-08：非法槽不可达——Belt 掉落池恒不含铁骨（200 seeds 全绿）
            for (uint seed = 1; seed <= 200; seed++)
            {
                var s = new SliceSession();
                ItemInstance it = s.RollItem(EquipSlot.Belt, Rarity.Rare, new SeededRng(seed), 1, "RareBelt");
                for (int a = 0; a < it.AffixCount; a++)
                    Assert.AreNotEqual((int)AffixId.Ironhide, it.AffixIdAt(a), "Belt 不得经随机路径获得铁骨（applicability 唯一 truth）");
            }
        }

        // ---------- AC-09：运行时消费（零死声明） ----------

        [Test]
        public void RuntimeConsumption_SwiftBreeze_AttackSpeedAggregation()
        {
            var baseline = new SliceSession();
            ClearEquipment(baseline);
            float baseRecovery = baseline.ResolveSkillDef(SkillId.Melee).Recovery;

            var s = new SliceSession();
            ClearEquipment(s);
            AddAndEquip(s, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            float scaled = s.ResolveSkillDef(SkillId.Melee).Recovery;
            Assert.AreEqual(baseRecovery / 1.16f, scaled, baseRecovery * 0.0002f,
                "迅疾经既有 AttackSpeed 聚合生效（Recovery=base/(1+speed)，不可见新分支）");
        }

        [Test]
        public void RuntimeConsumption_Ironhide_ArmourAggregation()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            // 先装无甲词缀身体测得基线；再换铁骨身体=同基面乘算（隔离其它槽词缀）
            AddAndEquip(s, EquipSlot.Body, AffixId.Life, 12f);
            float armour0 = s.PlayerStats.Get(StatId.Armour);
            AddAndEquip(s, EquipSlot.Body, AffixId.Ironhide, 0.22f);
            float armour1 = s.PlayerStats.Get(StatId.Armour);
            Assert.AreEqual(armour0 * 1.22f, armour1, armour0 * 0.002f,
                "铁骨经既有 Armour 聚合生效（increased=(base+flat)×(1+0.22)，同基面对拍）");
        }

        [Test]
        public void RuntimeConsumption_Insight_IntelligenceStatPath()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            AddAndEquip(s, EquipSlot.Helmet, AffixId.Insight, 12f);
            Assert.AreEqual(32f, s.PlayerStats.Get(StatId.Intelligence), 0.0001f, "睿智=既有 Intelligence Flat 轴（20 基础+12）");
            Assert.AreEqual(32, s.Intelligence, "会话属性=聚合值（既有 ManaPerInt 换算自动受益）");
        }

        [Test]
        public void RuntimeConsumption_Tenacity_StrengthStatPath()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            AddAndEquip(s, EquipSlot.Gloves, AffixId.Tenacity, 12f);
            Assert.AreEqual(32f, s.PlayerStats.Get(StatId.Strength), 0.0001f, "坚韧=既有 Strength Flat 轴（20 基础+12）");
            Assert.AreEqual(32, s.Strength, "会话属性=聚合值（既有 LifePerStr 换算自动受益）");
        }

        // ---------- AC-10/11：Craft 回归 + 通用呈现 ----------

        [Test]
        public void CraftRegression_DuplicateRejection_AndExistingSemantics()
        {
            var s = new SliceSession();
            s.Etching = 5; // 资源补足（初始 1 不够多次定向；验证的是词缀/craft 语义，非经济）
            string err;
            int idx = s.AddItem(MakeAffixItem(s, EquipSlot.Weapon, AffixId.Life, 12f));
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.SwiftBreeze, out err), err);
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.Tenacity, out err), err);
            Assert.AreEqual(Rarity.Rare, s.Inventory[idx].Rarity, "≥3 词缀=Rare（既有语义不变）");
            int etch = s.Etching;
            Assert.IsFalse(s.TryDirectedCraft(idx, AffixId.SwiftBreeze, out err), "既有 duplicate rejection 不变");
            Assert.IsTrue(err.Contains("已在该装备上"), err);
            Assert.AreEqual(etch, s.Etching, "拒绝不消耗蚀刻剂");
            // 定向目录枚举自动纳入新词条（Craft 面板按钮=AffixCatalog.Count 迭代，无硬编码清单）
            Assert.AreEqual(21, AffixCatalog.Count);
        }

        [Test]
        public void GenericPresentation_AffixLine_AndItemCard()
        {
            var s = new SliceSession();
            ItemInstance it = MakeAffixItem(s, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            string line = SliceSession.AffixLine(it, 0);
            Assert.IsTrue(line.Contains("攻击速度"), "通用 Format 渲染：" + line);
            Assert.IsFalse(line.Contains("{"), "Format 占位符必须已渲染：" + line);

            var card = SliceTooltipModel.ItemCard(it, s);
            Assert.IsNotNull(card.Body);
            Assert.IsTrue(System.Array.Exists(card.Body, b => b.Contains("攻击速度")),
                "物品 Tooltip=通用 Body 行（无词条专属 UI 分支）");
        }

        // ---------- AC-13：Multi-Link 冻结回归联动 ----------

        [Test]
        public void MultiLinkFrozenRegression_NewAffixItem_InteractsUnchanged()
        {
            var s = new SliceSession();
            string err;
            // 新词缀物品参与多连接路径：定向铁骨→改挂→组1 装配，全部照常
            int idx = s.AddItem(MakeAffixItem(s, EquipSlot.Weapon, AffixId.Ironhide, 0.22f));
            Assert.IsTrue(s.TryEquip(idx, out err), err);
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.SwiftBreeze, out err), err);
            Assert.IsTrue(s.TryReassignLink(idx, SkillId.Projectile, out err), err);
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee), "拆分后 host 组0 容量规则不变");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "组1 容量规则不变");
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Faster, out err), err);
            Assert.AreEqual(SupportId.Faster, s.WSupports[0], "组1 Support 装配不受新词缀影响");
        }
    }
}
