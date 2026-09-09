using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S4-P2 六槽装备契约（工作令 S4-P2-EQUIPMENT-BREADTH-GLOVES-BELT §26）：
    /// 槽位元数据（无 Boots 兜底错名）/ 初始镇=旧四槽在穿、新手套腰带空 / RollItem 可产手套腰带 /
    /// 掉落路径经 canonical NextInt(0,Count) 可达新槽（OnKill 真实 DropGear 驱动，种子确定性）/
    /// 穿戴与替换聚合进出 / Craft 闭环同槽不变 / 同槽 union 对比 / 6 槽快照 / Production Report 派生 6。
    /// 随机全部走 SeededRng（无 UnityEngine.Random）。
    /// </summary>
    public sealed class SixSlotEquipmentTests
    {
        [Test]
        public void SlotMetadata_AllSixSlots_NoBootFallback()
        {
            Assert.AreEqual(3, SliceSession.SocketsFor(EquipSlot.Weapon));
            Assert.AreEqual(3, SliceSession.SocketsFor(EquipSlot.Body));
            Assert.AreEqual(2, SliceSession.SocketsFor(EquipSlot.Helmet));
            Assert.AreEqual(0, SliceSession.SocketsFor(EquipSlot.Boots));
            Assert.AreEqual(1, SliceSession.SocketsFor(EquipSlot.Gloves));
            Assert.AreEqual(1, SliceSession.SocketsFor(EquipSlot.Belt));

            Assert.AreEqual("武器", SliceSession.SlotName(EquipSlot.Weapon));
            Assert.AreEqual("胸甲", SliceSession.SlotName(EquipSlot.Body));
            Assert.AreEqual("头盔", SliceSession.SlotName(EquipSlot.Helmet));
            Assert.AreEqual("靴子", SliceSession.SlotName(EquipSlot.Boots));
            Assert.AreEqual("手套", SliceSession.SlotName(EquipSlot.Gloves), "新槽不得落进 Boots default 兜底");
            Assert.AreEqual("腰带", SliceSession.SlotName(EquipSlot.Belt), "新槽不得落进 Boots default 兜底");

            Assert.AreEqual("铁刃", SliceSession.ItemBaseName(EquipSlot.Weapon));
            Assert.AreEqual("皮甲", SliceSession.ItemBaseName(EquipSlot.Body));
            Assert.AreEqual("铁盔", SliceSession.ItemBaseName(EquipSlot.Helmet));
            Assert.AreEqual("旧靴", SliceSession.ItemBaseName(EquipSlot.Boots));
            Assert.AreEqual("布手", SliceSession.ItemBaseName(EquipSlot.Gloves), "新槽不得落进 Boots default 兜底");
            Assert.AreEqual("皮带", SliceSession.ItemBaseName(EquipSlot.Belt), "新槽不得落进 Boots default 兜底");
        }

        [Test]
        public void StarterTown_OldFourEquipped_NewSlotsStartEmpty()
        {
            var s = new SliceSession();
            Assert.AreEqual(6, s.Equipped.Length, "装备数组必须随 EquipSlot.Count 派生");
            for (int i = 0; i <= 3; i++)
                Assert.GreaterOrEqual(s.Equipped[i], 0, "旧四槽初始装备行为保持（槽 " + (EquipSlot)i + "）");
            Assert.AreEqual(-1, s.Equipped[(int)EquipSlot.Gloves], "新手套=空（不改变镇重置 RNG 流）");
            Assert.AreEqual(-1, s.Equipped[(int)EquipSlot.Belt], "新腰带=空（不改变镇重置 RNG 流）");
        }

        [Test]
        public void RollItem_CanProduceGlovesAndBelt()
        {
            var s = new SliceSession();
            ItemInstance g = s.RollItem(EquipSlot.Gloves, Rarity.Rare, new SeededRng(7u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            Assert.AreEqual(EquipSlot.Gloves, g.Slot);
            Assert.AreEqual(1, g.SocketCount);
            Assert.AreEqual("布手", g.BaseName);
            ItemInstance b = s.RollItem(EquipSlot.Belt, Rarity.Ordinary, new SeededRng(9u),
                SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
            Assert.AreEqual(EquipSlot.Belt, b.Slot);
            Assert.AreEqual(1, b.SocketCount);
            Assert.AreEqual("皮带", b.BaseName);
        }

        [Test]
        public void Loot_DropPath_ReachesGlovesAndBelt()
        {
            // 真实 DropGear 路径：OnKill → NextInt(LootRng, 0, EquipSlot.Count)。种子确定性扫描，
            // 断言 6 槽均可达（新槽不是死内容）且掉落槽位全部合法。
            var seen = new HashSet<EquipSlot>();
            for (uint seed = 1u; seed <= 120u && (seen.Count < (int)EquipSlot.Count); seed++)
            {
                var sim = NewSliceSim(seed);
                string err;
                Assert.IsTrue(sim.Session.TryEnterMap(sim, out err), err);
                var dummy = sim.Dummies.Items[0];
                for (int k = 0; k < 6; k++)
                    sim.Session.OnKill(sim, dummy, 0);
                for (int i = 4; i < sim.Session.InventoryCount; i++) // 前 4=初始装备（Weapon/Body/Helmet/Boots）
                    seen.Add(sim.Session.Inventory[i].Slot);
            }
            for (int i = 0; i < (int)EquipSlot.Count; i++)
                Assert.IsTrue(seen.Contains((EquipSlot)i), "掉落池必须覆盖全部 6 槽（缺 " + (EquipSlot)i + "）");
        }

        [Test]
        public void Equip_Replace_Gloves_UpdatesAggregationBothWays()
        {
            var s = new SliceSession();
            // 清空初始装备，隔离 MaxLife 断言（导演真相：基础血量 9999999）
            ClearAll(s);
            float baseLife = LifeOf(s);

            ItemInstance a = MakeItem(s, EquipSlot.Gloves, 100f);
            int idxA = s.AddItem(a);
            string err;
            Assert.IsTrue(s.TryEquip(idxA, out err), err);
            Assert.AreEqual(idxA, s.Equipped[(int)EquipSlot.Gloves]);
            float withA = LifeOf(s);
            Assert.Greater(withA, baseLife, "手套 Life 词缀必须进 canonical 聚合");

            ItemInstance b = MakeItem(s, EquipSlot.Gloves, 300f);
            int idxB = s.AddItem(b);
            Assert.IsTrue(s.TryEquip(idxB, out err), err);
            Assert.AreEqual(idxB, s.Equipped[(int)EquipSlot.Gloves], "同槽替换必须覆盖");
            float withB = LifeOf(s);
            Assert.Greater(withB, withA, "替换后新词缀生效");
            Assert.AreNotEqual(withB, baseLife, "替换后旧词缀不得残留");
        }

        [Test]
        public void Equip_Replace_Belt_UpdatesAggregation_BothWays()
        {
            var s = new SliceSession();
            ClearAll(s);
            float baseLife = LifeOf(s);

            ItemInstance a = MakeItem(s, EquipSlot.Belt, 150f);
            int idxA = s.AddItem(a);
            string err;
            Assert.IsTrue(s.TryEquip(idxA, out err), err);
            Assert.AreEqual(idxA, s.Equipped[(int)EquipSlot.Belt]);
            Assert.Greater(LifeOf(s), baseLife, "腰带 Life 词缀必须进 canonical 聚合");

            ItemInstance b = MakeItem(s, EquipSlot.Belt, 50f);
            int idxB = s.AddItem(b);
            Assert.IsTrue(s.TryEquip(idxB, out err), err);
            Assert.AreEqual(idxB, s.Equipped[(int)EquipSlot.Belt]);
            Assert.Less(LifeOf(s), baseLife + 150f, "替换后旧腰带词缀必须移除（50 < 150）");
        }

        [Test]
        public void Craft_WorksOnGlovesAndBelt_SlotPreserved()
        {
            foreach (var slot in new[] { EquipSlot.Gloves, EquipSlot.Belt })
            {
                var s = new SliceSession();
                s.Scrap = 5;
                ItemInstance it = s.RollItem(slot, Rarity.Rare, new SeededRng(21u),
                    SliceSession.SocketsFor(slot), SliceSession.ItemBaseName(slot));
                int idx = s.AddItem(it);
                string err;
                Assert.IsTrue(s.TryRandomCraft(idx, out err), err);
                ItemInstance crafted = s.Inventory[idx];
                Assert.AreEqual(slot, crafted.Slot, "随机制作=废料池同槽重掷，槽位不得漂移");
                Assert.Greater(crafted.AffixCount, 0);
            }
        }

        [Test]
        public void SameSlotComparison_WorksWithNewSlots()
        {
            var s = new SliceSession();
            ClearAll(s);
            ItemInstance eq = MakeItem(s, EquipSlot.Gloves, 100f);
            int idx = s.AddItem(eq);
            string err;
            Assert.IsTrue(s.TryEquip(idx, out err), err);
            ItemInstance candidate = MakeItem(s, EquipSlot.Gloves, 260f);
            var rows = SliceTooltipModel.Compare(candidate, s.Inventory[s.Equipped[(int)EquipSlot.Gloves]]);
            Assert.Greater(rows.Count, 0, "手套同槽 union 对比必须产出 delta 行");
            var card = SliceTooltipModel.ItemCard(candidate, s);
            Assert.IsNotNull(card, "手套物品卡（含同槽已装备对比路径）不得抛异常");
        }

        [Test]
        public void BuildSnapshot_IncludesGlovesAndBelt()
        {
            var s = new SliceSession();
            ItemInstance g = s.RollItem(EquipSlot.Gloves, Rarity.Ordinary, new SeededRng(31u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            ItemInstance b = s.RollItem(EquipSlot.Belt, Rarity.Ordinary, new SeededRng(33u),
                SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
            string err;
            Assert.IsTrue(s.TryEquip(s.AddItem(g), out err), err);
            Assert.IsTrue(s.TryEquip(s.AddItem(b), out err), err);

            var sim = NewSliceSim(5u);
            sim.Session = s;
            sim.Caster.Defs = s.ResolveSkillDef;
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            Assert.AreEqual(s.Equipped[(int)EquipSlot.Gloves], s.Snapshot.GlovesId, "快照必须含手套槽");
            Assert.AreEqual(s.Equipped[(int)EquipSlot.Belt], s.Snapshot.BeltId, "快照必须含腰带槽");
            // 旧四槽快照语义保持
            Assert.AreEqual(s.Equipped[0], s.Snapshot.WeaponId);
            Assert.AreEqual(s.Equipped[1], s.Snapshot.BodyId);
            Assert.AreEqual(s.Equipped[2], s.Snapshot.HelmetId);
            Assert.AreEqual(s.Equipped[3], s.Snapshot.BootsId);
        }

        [Test]
        public void ProductionReport_DerivesSixSlotsCanonically()
        {
            var audit = ContentAuditS2Tests.CollectCurrentRepositoryAudit();
            audit.Completed = true; // seam 语义（同 ProductionContentReportTests）
            var data = ProductionContentReport.Build(audit);
            Assert.AreEqual(6, data.EquipmentSlotCount);
            Assert.AreEqual((int)EquipSlot.Count, data.EquipmentSlotCount, "equipmentSlots 必须=canonical EquipSlot.Count 派生");
        }

        // ---- helpers ----

        static ArenaSim NewSliceSim(uint seed)
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            sim.Session.ResetTown(seed);
            sim.Caster.Defs = sim.Session.ResolveSkillDef;
            return sim;
        }

        static void ClearAll(SliceSession s)
        {
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;
            s.RecalcPlayer(true);
        }

        static float LifeOf(SliceSession s)
        {
            s.RecalcPlayer(true);
            return s.MaxLife;
        }

        static ItemInstance MakeItem(SliceSession s, EquipSlot slot, float lifeValue)
        {
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = slot;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = SliceSession.SocketsFor(slot);
            it.BaseName = SliceSession.ItemBaseName(slot);
            it.AffixCount = 1;
            it.SetAffix(0, AffixId.Life, lifeValue, 0f);
            return it;
        }
    }
}
