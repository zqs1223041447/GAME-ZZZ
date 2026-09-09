using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S4-P3 Affix applicability 契约（工作令 S4-P3-AFFIX-AFFIX-APPLICABILITY-BREADTH §三十二/§三十三）：
    /// 旧 13 Affix stable ID 与全槽可用行为保持 / 新词缀行合法且只用已被 Runtime 消费的 Stat / 唯一 predicate=AffixDef.IsApplicable /
    /// Gloves/Belt 池互斥新词缀 / RollItem 走 eligible 池且确定性 / Directed Craft 非法组合 deterministic reject /
    /// 六槽池非空 / 合成坏 mask 被审计校验拒绝 / 几百次生成 smoke 零非法对+可复现 / Production Report per-slot 派生。
    /// 随机全部走 SeededRng。
    /// </summary>
    public sealed class AffixApplicabilityTests
    {
        static readonly AffixId[] OldAffixes =
        {
            AffixId.AddPhys, AffixId.IncPhys, AffixId.AddFire, AffixId.IncFire, AffixId.Life,
            AffixId.Armour, AffixId.Evasion, AffixId.FireRes, AffixId.Accuracy, AffixId.Crit,
            AffixId.IgniteFire, AffixId.AccCrit, AffixId.PhysFire
        };

        [Test]
        public void OldAffixes_StableIds_AndAllSlotsEligible_Preserved()
        {
            Assert.AreEqual(13, OldAffixes.Length);
            for (int i = 0; i < OldAffixes.Length; i++)
            {
                Assert.AreEqual((int)OldAffixes[i], i, "旧 Affix stable ID 不得漂移（位 " + i + "）");
                AffixDef def = AffixCatalog.Get(OldAffixes[i]);
                Assert.AreEqual(OldAffixes[i], def.Id);
                Assert.AreEqual(0, def.AllowedSlots, "旧词缀默认不限槽（Branch B 向后兼容语义）");
                for (int s = 0; s < (int)EquipSlot.Count; s++)
                    Assert.IsTrue(def.IsApplicable((EquipSlot)s), "旧词缀必须六槽全可用：" + def.Name);
            }
            Assert.GreaterOrEqual((int)AffixId.Count, 21, "S5-WO-04 落地后 21（不得缩水）");
            Assert.LessOrEqual((int)AffixId.Count, 21, "S5-WO-04 追加上限 4（总 21，BL-002.A1 锁定清单，硬上限）");
        }

        [Test]
        public void NewAffixes_RowsValid_AndSlotRestriction()
        {
            Assert.AreEqual(EquipSlot.Gloves, RestrictedTo(AffixCatalog.Get(AffixId.SwiftGrip)), "迅握=Gloves 专属");
            Assert.AreEqual(EquipSlot.Gloves, RestrictedTo(AffixCatalog.Get(AffixId.KeenEdge)), "锋锐=Gloves 专属");
            Assert.AreEqual(EquipSlot.Belt, RestrictedTo(AffixCatalog.Get(AffixId.Bulwark)), "壁垒=Belt 专属");
            Assert.AreEqual(EquipSlot.Belt, RestrictedTo(AffixCatalog.Get(AffixId.VitalWeave)), "韧脉=Belt 专属");

            AffixDef swift = AffixCatalog.Get(AffixId.SwiftGrip);
            Assert.AreEqual(StatId.AttackSpeed, swift.Stat);
            Assert.AreEqual(ModOp.Increased, swift.Op);
            Assert.AreEqual(1, swift.RowCount);
            AffixDef keen = AffixCatalog.Get(AffixId.KeenEdge);
            Assert.AreEqual(2, keen.RowCount);
            Assert.AreEqual(StatId.CritChanceAdded, keen.Stat);
            Assert.AreEqual(StatId.Accuracy, keen.Stat2);
            AffixDef bulwark = AffixCatalog.Get(AffixId.Bulwark);
            Assert.AreEqual(StatId.Armour, bulwark.Stat);
            Assert.AreEqual(ModOp.Increased, bulwark.Op);
            AffixDef vital = AffixCatalog.Get(AffixId.VitalWeave);
            Assert.AreEqual(2, vital.RowCount);
            Assert.AreEqual(StatId.Life, vital.Stat);
            Assert.AreEqual(StatId.FireResistance, vital.Stat2);
        }

        static EquipSlot? RestrictedTo(AffixDef def)
        {
            if (def.AllowedSlots == 0)
                return null;
            for (int s = 0; s < (int)EquipSlot.Count; s++)
                if ((def.AllowedSlots & (ushort)(1 << s)) != 0)
                    return (EquipSlot)s;
            return null;
        }

        [Test]
        public void AllAffixes_OnlyConsumedStats_NoNewModOp()
        {
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                for (int r = 0; r < def.RowCount; r++)
                {
                    StatId stat = def.RowStat(r);
                    bool consumed = false;
                    foreach (var s in ContentAuditS2Tests.RuntimeConsumedStats)
                        if (s == stat) { consumed = true; break; }
                    Assert.IsTrue(consumed, "词缀 Stat 必须属于 canonical runtime-consumed truth：" + def.Name + " -> " + stat);
                    Assert.LessOrEqual((int)def.RowOp(r), (int)ModOp.Override, "ModOp 不得越界：" + def.Name);
                }
            }
        }

        [Test]
        public void EligiblePools_PerSlot_DerivedFromPredicate()
        {
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                var slot = (EquipSlot)s;
                var pool = EligibleIds(slot);
                Assert.GreaterOrEqual(pool.Count, 13, "每槽 eligible 池必须 ≥13（不限槽词缀）：" + slot);
                foreach (var old in OldAffixes)
                    Assert.IsTrue(pool.Contains((int)old), "不限槽旧词缀必须六槽可用：" + slot);
            }
            var gloves = EligibleIds(EquipSlot.Gloves);
            var belt = EligibleIds(EquipSlot.Belt);
            Assert.IsTrue(gloves.Contains((int)AffixId.SwiftGrip) && gloves.Contains((int)AffixId.KeenEdge), "Gloves 池必须含新 Gloves 词缀");
            Assert.IsFalse(gloves.Contains((int)AffixId.Bulwark) || gloves.Contains((int)AffixId.VitalWeave), "Gloves 池不得含 Belt 专属");
            Assert.IsTrue(belt.Contains((int)AffixId.Bulwark) && belt.Contains((int)AffixId.VitalWeave), "Belt 池必须含新 Belt 词缀");
            Assert.IsFalse(belt.Contains((int)AffixId.SwiftGrip) || belt.Contains((int)AffixId.KeenEdge), "Belt 池不得含 Gloves 专属");
        }

        static List<int> EligibleIds(EquipSlot slot)
        {
            var pool = new List<int>();
            for (int i = 0; i < AffixCatalog.Count; i++)
                if (AffixCatalog.Get((AffixId)i).IsApplicable(slot))
                    pool.Add(i);
            return pool;
        }

        [Test]
        public void NoAffix_WithZeroApplicableSlots()
        {
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                bool any = false;
                for (int s = 0; s < (int)EquipSlot.Count; s++)
                    if (def.IsApplicable((EquipSlot)s)) { any = true; break; }
                Assert.IsTrue(any, "词缀不得零槽可用：" + def.Name);
            }
        }

        [Test]
        public void RollItem_RespectsApplicability_AndDeterministic()
        {
            var seenGloves = new HashSet<int>();
            var seenBelt = new HashSet<int>();
            for (uint seed = 1u; seed <= 80u; seed++)
            {
                var s = new SliceSession();
                ItemInstance g = s.RollItem(EquipSlot.Gloves, Rarity.Rare, new SeededRng(seed),
                    SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
                for (int a = 0; a < g.AffixCount; a++)
                {
                    Assert.IsTrue(AffixCatalog.Get((AffixId)g.AffixIdAt(a)).IsApplicable(EquipSlot.Gloves),
                        "Gloves 掉落不得出现非法词缀：" + g.AffixIdAt(a));
                    seenGloves.Add(g.AffixIdAt(a));
                }
                ItemInstance b = s.RollItem(EquipSlot.Belt, Rarity.Rare, new SeededRng(seed ^ 0x5A5Au),
                    SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
                for (int a = 0; a < b.AffixCount; a++)
                {
                    Assert.IsTrue(AffixCatalog.Get((AffixId)b.AffixIdAt(a)).IsApplicable(EquipSlot.Belt),
                        "Belt 掉落不得出现非法词缀：" + b.AffixIdAt(a));
                    seenBelt.Add(b.AffixIdAt(a));
                }
                ItemInstance w = s.RollItem(EquipSlot.Weapon, Rarity.Rare, new SeededRng(seed ^ 0xA5A5u),
                    SliceSession.SocketsFor(EquipSlot.Weapon), SliceSession.ItemBaseName(EquipSlot.Weapon));
                for (int a = 0; a < w.AffixCount; a++)
                {
                    int id = w.AffixIdAt(a);
                    Assert.IsFalse(id == (int)AffixId.SwiftGrip || id == (int)AffixId.KeenEdge ||
                                   id == (int)AffixId.Bulwark || id == (int)AffixId.VitalWeave,
                        "旧四槽不得 roll 新专属词缀：" + id);
                }
            }
            Assert.IsTrue(seenGloves.Contains((int)AffixId.SwiftGrip) && seenGloves.Contains((int)AffixId.KeenEdge),
                "新 Gloves 词缀必须可达（种子扫描）");
            Assert.IsTrue(seenBelt.Contains((int)AffixId.Bulwark) && seenBelt.Contains((int)AffixId.VitalWeave),
                "新 Belt 词缀必须可达（种子扫描）");

            // 确定性重放：同 seed 两次 roll 结果逐字段一致
            var s1 = new SliceSession();
            var s2 = new SliceSession();
            ItemInstance r1 = s1.RollItem(EquipSlot.Gloves, Rarity.Rare, new SeededRng(777u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            ItemInstance r2 = s2.RollItem(EquipSlot.Gloves, Rarity.Rare, new SeededRng(777u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            Assert.AreEqual(r1.AffixCount, r2.AffixCount);
            for (int a = 0; a < r1.AffixCount; a++)
            {
                Assert.AreEqual(r1.AffixIdAt(a), r2.AffixIdAt(a));
                Assert.AreEqual(r1.ValueAt(a), r2.ValueAt(a), 0.0001f);
                Assert.AreEqual(r1.SecondValueAt(a), r2.SecondValueAt(a), 0.0001f);
            }
        }

        [Test]
        public void DirectedCraft_RejectsIllegalCombination_Deterministic()
        {
            var s = new SliceSession();
            s.Etching = 2;
            ItemInstance gloves = s.RollItem(EquipSlot.Gloves, Rarity.Ordinary, new SeededRng(5u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            int idx = s.AddItem(gloves);
            string err;
            Assert.IsFalse(s.TryDirectedCraft(idx, AffixId.Bulwark, out err), "Belt 专属词缀写入手套必须 deterministic reject");
            Assert.IsFalse(string.IsNullOrEmpty(err));
            Assert.AreEqual(2, s.Etching, "拒绝时不得消耗蚀刻剂");
            for (int a = 0; a < s.Inventory[idx].AffixCount; a++)
                Assert.IsTrue(AffixCatalog.Get((AffixId)s.Inventory[idx].AffixIdAt(a)).IsApplicable(EquipSlot.Gloves),
                    "拒绝后物品必须保持原样合法");

            ItemInstance belt = s.RollItem(EquipSlot.Belt, Rarity.Ordinary, new SeededRng(6u),
                SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
            int beltIdx = s.AddItem(belt);
            Assert.IsTrue(s.TryDirectedCraft(beltIdx, AffixId.Bulwark, out err), err);
            bool found = false;
            for (int a = 0; a < s.Inventory[beltIdx].AffixCount; a++)
                if (s.Inventory[beltIdx].AffixIdAt(a) == (int)AffixId.Bulwark)
                    found = true;
            Assert.IsTrue(found, "合法组合必须写入成功");
            Assert.AreEqual(1, s.Etching);
        }

        [Test]
        public void Audit_Validator_CatchesSyntheticInvalidMask()
        {
            var bad = new AffixDef { Id = AffixId.SwiftGrip, Name = "合成坏mask", AllowedSlots = (ushort)(1 << 15) };
            Assert.IsNotNull(ContentAuditTagRules.ValidateAffixSlots(bad), "未知 EquipSlot bit 必须被拒绝");
            var good = new AffixDef { Id = AffixId.SwiftGrip, Name = "合成好mask", AllowedSlots = AffixCatalog.SlotsMask(EquipSlot.Gloves, EquipSlot.Belt) };
            Assert.IsNull(ContentAuditTagRules.ValidateAffixSlots(good));
            var unrestricted = new AffixDef { Id = AffixId.Life, Name = "不限槽", AllowedSlots = 0 };
            Assert.IsNull(ContentAuditTagRules.ValidateAffixSlots(unrestricted));
        }

        [Test]
        public void GenerationSmoke_HundredsRolls_NoIllegalPair_RepeatDeterministic()
        {
            // §三十三 小型确定性 smoke：几百次生成（Drop/Craft 共用的 RollItem 路径），零非法 slot-affix 对 + 同 seed 可复现。
            var first = GenerateSequence(911u, 40);
            var second = GenerateSequence(911u, 40);
            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].slot, second[i].slot);
                Assert.AreEqual(first[i].affix, second[i].affix);
                Assert.AreEqual(first[i].value, second[i].value, 0.0001f);
            }
            foreach (var row in first)
                Assert.IsTrue(AffixCatalog.Get((AffixId)row.affix).IsApplicable(row.slot), "非法 slot-affix 对：" + row.slot + " <- " + row.affix);
        }

        static List<(EquipSlot slot, int affix, float value)> GenerateSequence(uint seed, int rounds)
        {
            var rows = new List<(EquipSlot, int, float)>();
            for (int r = 0; r < rounds; r++)
            {
                var s = new SliceSession();
                for (int slot = 0; slot < (int)EquipSlot.Count; slot++)
                {
                    var es = (EquipSlot)slot;
                    ItemInstance it = s.RollItem(es, (r % 2 == 0) ? Rarity.Rare : Rarity.Ordinary, new SeededRng((uint)(seed + r * 10 + slot)),
                        SliceSession.SocketsFor(es), SliceSession.ItemBaseName(es));
                    for (int a = 0; a < it.AffixCount; a++)
                        rows.Add((es, it.AffixIdAt(a), it.ValueAt(a)));
                }
            }
            return rows;
        }

        [Test]
        public void ProductionReport_AffixApplicability_DerivedFromPredicate()
        {
            var audit = ContentAuditS2Tests.CollectCurrentRepositoryAudit();
            audit.Completed = true; // seam 语义（同 ProductionContentReportTests）
            var data = ProductionContentReport.Build(audit);
            Assert.AreEqual((int)AffixId.Count, data.AffixCount);
            int unrestricted = 0, restricted = 0;
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                if (AffixCatalog.Get((AffixId)i).AllowedSlots == 0) unrestricted++; else restricted++;
            }
            Assert.AreEqual(unrestricted, data.AffixUnrestricted);
            Assert.AreEqual(restricted, data.AffixRestricted);
            Assert.AreEqual((int)EquipSlot.Count, data.AffixBySlot.Count);
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                int expected = EligibleIds((EquipSlot)s).Count;
                Assert.AreEqual(expected, data.AffixBySlot[s].EligibleAffixCount,
                    "per-slot eligibleAffixCount 必须由 canonical predicate 派生：" + (EquipSlot)s);
            }
        }
    }
}
