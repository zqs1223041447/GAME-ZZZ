using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S4 Phase 4 Production Simulation 契约（S4_PLAN §八；协调席 2026-09-09 工作指令）：
    /// 10,000 cycles（真实 Drop/Craft/Equip 路径）invalid=0 / 同 seed 结果 hash 一致 / 异 seed hash 不同（sanity）。
    /// 报告再生（docs/qa/PRODUCTION_SIMULATION_REPORT.json）在本测试内执行——工具生成，禁止手填。
    /// </summary>
    public sealed class ProductionSimulatorTests
    {
        [Test]
        public void Simulation_10kCycles_ZeroInvalid_AndReproducible()
        {
            var r = ProductionSimulator.Run(ProductionSimulator.Seed, true);
            Debug.Log("[ProductionSimulation] hash=" + r.Hash + " invalid=" + r.InvalidCount);
            Assert.AreEqual(ProductionSimulator.TotalIterations, r.Iterations);
            Assert.AreEqual(0, r.InvalidCount, "10,000 cycles 不得产生非法状态：" + string.Join(" | ", r.Invalids.ToArray()));
            Assert.IsTrue(r.RepeatHashMatch, "同 seed 重复运行 hash 必须一致");
            Assert.IsTrue(r.VerdictPass);
            Assert.Greater(r.DropCycles, 0);
            Assert.Greater(r.RandomCraftCycles, 0);
            Assert.Greater(r.DirectedCraftCycles, 0);
            Assert.Greater(r.EquipCycles, 0);
            // 新词缀在大样本必须可达（Gloves/Belt 专属 4 条）
            Assert.Greater(r.AffixDistribution[(int)AffixId.SwiftGrip], 0, "迅握必须可达");
            Assert.Greater(r.AffixDistribution[(int)AffixId.KeenEdge], 0, "锋锐必须可达");
            Assert.Greater(r.AffixDistribution[(int)AffixId.Bulwark], 0, "壁垒必须可达");
            Assert.Greater(r.AffixDistribution[(int)AffixId.VitalWeave], 0, "韧脉必须可达");
            // 旧四槽不得出现新专属词缀（分布级证明：新专属词缀只出现在 Gloves/Belt 槽分布里由 applicability 保证）
            // rejectedDirectedCraft=预期 duplicate guard 生效次数（信息项，非失败）；非法组合拒绝必须为 0（由 invalidCount 覆盖）
        }

        [Test]
        public void DirectedCraft_RejectsDuplicateAffix_WithoutConsuming()
        {
            // S4-P4 发现项回归测试：物品内重复词缀=非法 duplicate——deterministic reject（不消耗蚀刻剂）
            var s = new SliceSession();
            s.Etching = 3;
            ItemInstance it = s.RollItem(EquipSlot.Weapon, Rarity.Ordinary, new SeededRng(5u),
                SliceSession.SocketsFor(EquipSlot.Weapon), SliceSession.ItemBaseName(EquipSlot.Weapon));
            int idx = s.AddItem(it);
            AffixId existing = (AffixId)s.Inventory[idx].AffixIdAt(0);
            string err;
            Assert.IsFalse(s.TryDirectedCraft(idx, existing, out err), "重复词缀必须被拒绝");
            Assert.IsFalse(string.IsNullOrEmpty(err));
            Assert.AreEqual(3, s.Etching, "拒绝时不得消耗蚀刻剂");
            int count = s.Inventory[idx].AffixCount;
            int occurrences = 0;
            for (int a = 0; a < count; a++)
                if (s.Inventory[idx].AffixIdAt(a) == (int)existing)
                    occurrences++;
            Assert.AreEqual(1, occurrences, "拒绝后物品内该词缀仍只有一次");
        }

        [Test]
        public void Simulation_SameSeed_HashIdentical()
        {
            var a = ProductionSimulator.Run(ProductionSimulator.Seed, false);
            var b = ProductionSimulator.Run(ProductionSimulator.Seed, false);
            Assert.AreEqual(a.Hash, b.Hash, "同 seed 输出必须可复现");
            for (int i = 0; i < a.AffixDistribution.Length; i++)
                Assert.AreEqual(a.AffixDistribution[i], b.AffixDistribution[i], "同 seed 词缀分布必须一致");
            Assert.AreEqual(a.RareCount, b.RareCount);
            Assert.AreEqual(a.OrdinaryCount, b.OrdinaryCount);
        }

        [Test]
        public void Simulation_DifferentSeed_HashDiffers()
        {
            var a = ProductionSimulator.Run(ProductionSimulator.Seed, false);
            var b = ProductionSimulator.Run(ProductionSimulator.Seed ^ 0xBEEFu, false);
            Assert.AreNotEqual(a.Hash, b.Hash, "不同 seed 输出应当不同（64-bit FNV 碰撞概率可忽略）");
        }
    }
}
