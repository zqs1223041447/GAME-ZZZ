using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-02 门禁：Passive-Aware Production Simulation。
    ///
    /// 本单只扩大 evidence surface，不改 passive runtime。这些断言要证明的是：
    ///   · canonical fixture 只用 SUPPORTED、非专精节点，且只经 domain API 加点；
    ///   · 已分配身份 / 实际生效 modifier / 有效 gameplay 属性都真的进了 canonical 哈希；
    ///   · 改加点 / 换合法加点使有效属性变化 → 哈希必变；恢复 canonical → 精确复原；
    ///   · 枚举/操作顺序、UI 状态、树几何、贴图都不影响哈希。
    ///
    /// 专精与 2005 个 unsupported 节点在本单明确排除（EXCLUDED_BY_CONTRACT）。
    /// </summary>
    public sealed class PassiveAwareProductionSimulationTests
    {
        static readonly int[] Canonical = PassiveAwareProductionSimulation.CanonicalNodeIds;
        static readonly int[] Reversed = PassiveAwareProductionSimulation.CanonicalReverseOrderNodeIds;
        static readonly int[] MutationAllocation = PassiveAwareProductionSimulation.SensitivityAllocationNodeIds;
        static readonly int[] MutationStat = PassiveAwareProductionSimulation.SensitivityStatNodeIds;

        static SliceSession NewSession(uint seed)
        {
            var s = new SliceSession();
            s.ResetTown(seed);
            return s;
        }

        static SliceSession NewCanonicalSession(uint seed)
        {
            var s = NewSession(seed);
            PassiveAwareProductionSimulation.Apply(s, Canonical);
            return s;
        }

        static string Payload(SliceSession s)
        {
            return PassiveAwareProductionSimulation.StatePayload(s);
        }

        static string Section(string payload, string tag)
        {
            int at = payload.IndexOf(tag);
            if (at < 0)
                return "";
            int end = payload.IndexOf('\n', at);
            return end < 0 ? payload.Substring(at) : payload.Substring(at, end - at);
        }

        static ulong Fnv(string s)
        {
            ulong h = 14695981039346656037UL;
            for (int i = 0; i < s.Length; i++)
            {
                h ^= s[i];
                h *= 1099511628211UL;
            }
            return h;
        }

        // ---------- 1. fixture 合法性与资格 ----------

        [Test]
        public void CanonicalFixtureUsesOnlySupportedNonMasteryNodes()
        {
            Assert.AreEqual(PassiveAwareProductionSimulation.StartNodeId, SliceSession.StartNode,
                "canonical fixture 假设的起点必须与运行时起点一致（树数据漂移即报红）");
            Assert.Greater(Canonical.Length, 0);

            for (int i = 0; i < Canonical.Length; i++)
            {
                PoeNode n = PoeTree.Get(Canonical[i]);
                Assert.AreNotEqual(PoeNodeKind.Mastery, n.Kind, "fixture 不得含专精节点：" + Canonical[i]);
                Assert.AreEqual(0, n.locked, "fixture 不得含树上不可点节点：" + Canonical[i]);

                int consumed, blocked, special, structural;
                PassiveCensus.NodeEligibility elig =
                    PassiveCensus.ClassifyNode(n, out consumed, out blocked, out special, out structural);
                Assert.AreEqual(PassiveCensus.NodeEligibility.Supported, elig,
                    "fixture 节点必须全部效果行可消费：" + Canonical[i] + " " + n.name);
                Assert.Greater(consumed, 0, "fixture 节点必须至少产出一条 CONSUMED 效果行：" + Canonical[i]);
            }

            var s = NewCanonicalSession(1u);
            Assert.Greater(PassiveAwareProductionSimulation.OffensiveTupleCount(s), 0, "必须覆盖至少一条进攻向 CONSUMED 效果");
            Assert.Greater(PassiveAwareProductionSimulation.DefensiveOrAttributeTupleCount(s), 0,
                "必须覆盖至少一条防御/属性/资源向 CONSUMED 效果");
        }

        [Test]
        public void CanonicalFixtureAllocatesThroughDomainAPI()
        {
            var s = NewSession(4242u);
            List<string> events = PassiveAwareProductionSimulation.ApplyCanonical(s);

            Assert.AreEqual(Canonical.Length, events.Count);
            for (int i = 0; i < events.Count; i++)
            {
                StringAssert.StartsWith("pa|", events[i]);
                StringAssert.Contains("|ok|", events[i], "canonical 加点必须被 domain 接受：" + events[i]);
            }

            int[] allocated = PassiveAwareProductionSimulation.AllocatedNodeIds(s);
            CollectionAssert.AreEquivalent(new[] { 2172, 559, 1795, 2034 }, allocated,
                "已分配集合 = 起点 + canonical fixture");
            Assert.AreEqual(SliceRules.StartPoints - Canonical.Length, s.Unspent, "点数守恒（缺一即说明绕过 domain）");
            Assert.IsTrue(s.Allocated[SliceSession.StartNode], "起点恒已点亮");

            // 不得绕过 domain：不相连节点必须仍被拒绝，且拒绝不扣点、不写容器
            var allocatedSet = new HashSet<int>(allocated);
            int far = FindNonAdjacentSupported(allocatedSet);
            Assert.GreaterOrEqual(far, 0, "必须能找到一个与已分配集合不相连的 SUPPORTED 节点作为反证");
            int before = s.Unspent;
            string err;
            Assert.IsFalse(s.TryAllocate(far, out err), "不相连节点必须被 domain 拒绝（" + far + "）");
            Assert.IsFalse(s.Allocated[far], "被拒绝的加点不得写入已分配容器");
            Assert.AreEqual(before, s.Unspent, "被拒绝的加点不得扣点");
        }

        static int FindNonAdjacentSupported(HashSet<int> allocated)
        {
            for (int i = 0; i < SliceRules.PassiveCount; i++)
            {
                if (allocated.Contains(i) || PoeTree.Get(i).locked != 0)
                    continue;
                int consumed, blocked, special, structural;
                if (PassiveCensus.ClassifyNode(PoeTree.Get(i), out consumed, out blocked, out special, out structural)
                    != PassiveCensus.NodeEligibility.Supported)
                    continue;
                int[] links = PoeTree.Get(i).links;
                bool adjacent = false;
                if (links != null)
                    for (int k = 0; k < links.Length; k++)
                        if (allocated.Contains(links[k])) { adjacent = true; break; }
                if (!adjacent)
                    return i;
            }
            return -1;
        }

        // ---------- 2. 哈希必须包含被动真相 ----------

        [Test]
        public void PassiveAllocatedNodeIdsEnterHash()
        {
            var baseline = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical);
            var mutated = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, MutationAllocation);

            Assert.AreNotEqual(baseline.Hash, mutated.Hash, "去掉一个 canonical 加点后哈希必须变");

            // 差异只能来自被动面：物品生成序列逐项相同
            for (int i = 0; i < baseline.AffixDistribution.Length; i++)
                Assert.AreEqual(baseline.AffixDistribution[i], mutated.AffixDistribution[i], "改动面只能是被动面");
            Assert.AreEqual(baseline.RareCount, mutated.RareCount);
            Assert.AreEqual(baseline.Iterations, mutated.Iterations);

            string psBaseline = Section(Payload(NewCanonicalSession(9u)), "ps|");
            var s2 = NewSession(9u);
            PassiveAwareProductionSimulation.Apply(s2, MutationAllocation);
            string psMutated = Section(Payload(s2), "ps|");
            Assert.AreNotEqual(psBaseline, psMutated, "已分配身份段必须真的进负载");
            StringAssert.Contains("559,1795,2034,2172", psBaseline);
        }

        [Test]
        public void PassiveEffectiveModifiersEnterHash()
        {
            var s = NewCanonicalSession(21u);
            string payload = Payload(s);
            string pm = Section(payload, "pm|");
            Assert.IsNotEmpty(pm, "canonical 场景必须产出 modifier 语义元组");
            List<string> tuples = PassiveAwareProductionSimulation.ModifierTuples(s);
            Assert.Greater(tuples.Count, 0);
            StringAssert.StartsWith("559:3:1:5", tuples[0],
                "V3 负载只含稳定数值 token（nodeId:statId:opId:value）：" + tuples[0]);

            // 单段扰动必须改变同一 FNV 负载哈希（证明 modifier 段真的参与，不是摆设）
            ulong whole = Fnv(payload);
            ulong identityOnly = Fnv(payload.Replace(Section(payload, "ps|"), Section(payload, "ps|") + ",99999"));
            ulong modifierOnly = Fnv(payload.Replace(pm, pm + "|99999:0:1:1:0:0"));
            Assert.AreNotEqual(whole, identityOnly);
            Assert.AreNotEqual(whole, modifierOnly);
        }

        [Test]
        public void PassiveEffectiveStatsEnterHash()
        {
            var baseline = NewCanonicalSession(31u);
            var mutated = NewSession(31u);
            PassiveAwareProductionSimulation.Apply(mutated, MutationStat);

            string peBaseline = Section(Payload(baseline), "pe|");
            string peMutated = Section(Payload(mutated), "pe|");
            Assert.AreNotEqual(peBaseline, peMutated, "有效 gameplay 属性段必须随合法加点变化");
            Assert.AreNotEqual(baseline.PlayerStats.Get(StatId.Life), mutated.PlayerStats.Get(StatId.Life),
                "合法替代加点必须产生不同的有效属性（敏感性 B 的前提）");
            Assert.AreNotEqual(
                ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical).Hash,
                ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, MutationStat).Hash,
                "有效属性变化必须传导到 canonical 哈希");
        }

        [Test]
        public void OffensivePassiveEntersSkillStatSnapshot()
        {
            var canonical = NewCanonicalSession(27u);
            StringAssert.Contains("pk|", Payload(canonical), "有效技能属性段必须在负载里");

            // 同一 seed → 起始装备相同；只差一个进攻节点时，技能包上的增量必须恰为该被动值。
            // （只看 Get() 会被 0 基底吃掉：+10% increased 作用在 0 上仍是 0。）
            var withPhys = NewSession(27u);
            PassiveAwareProductionSimulation.Apply(withPhys, new[] { 1795 });
            var withoutPhys = NewSession(27u);
            double physDelta = IncreasedOf(PassiveAwareProductionSimulation.SkillStatSnapshot(withPhys), 0, StatId.PhysicalDamage)
                             - IncreasedOf(PassiveAwareProductionSimulation.SkillStatSnapshot(withoutPhys), 0, StatId.PhysicalDamage);
            Assert.AreEqual(0.1, physDelta, 1e-5, "节点 1795 的 +10% increased PhysicalDamage 必须出现在有效技能属性上");

            var withSpeed = NewSession(27u);
            PassiveAwareProductionSimulation.Apply(withSpeed, new[] { 559 });
            var withoutSpeed = NewSession(27u);
            double speedDelta = IncreasedOf(PassiveAwareProductionSimulation.SkillStatSnapshot(withSpeed), 0, StatId.AttackSpeed)
                              - IncreasedOf(PassiveAwareProductionSimulation.SkillStatSnapshot(withoutSpeed), 0, StatId.AttackSpeed);
            Assert.AreEqual(0.04, speedDelta, 1e-5, "节点 559 的 +4% increased Attack Speed 必须出现在有效技能属性上");
        }

        /// <summary>解析技能属性快照：第 skillOrdinal 个技能、指定 StatId 的 increased 分量。</summary>
        static double IncreasedOf(string snapshot, int skillOrdinal, StatId stat)
        {
            string scope = snapshot.Split('|')[skillOrdinal];
            int colon = scope.IndexOf(':');
            string[] entries = scope.Substring(colon + 1).Split(',');
            string prefix = ((int)stat).ToString(System.Globalization.CultureInfo.InvariantCulture) + "=";
            for (int i = 0; i < entries.Length; i++)
            {
                if (!entries[i].StartsWith(prefix, System.StringComparison.Ordinal))
                    continue;
                string[] comps = entries[i].Substring(entries[i].IndexOf('=') + 1).Split('/');
                return double.Parse(comps[2], System.Globalization.CultureInfo.InvariantCulture);
            }
            return double.NaN;
        }

        [Test]
        public void PassiveAllocationMutationChangesHash()
        {
            var a = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical);
            var b = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, MutationAllocation);
            Assert.AreNotEqual(a.Hash, b.Hash);
            Assert.AreEqual(0, a.InvalidCount);
            Assert.AreEqual(0, b.InvalidCount, "两个变体都必须是干净运行（invalid=0）");
        }

        [Test]
        public void PassiveStatMutationViaLegalAlternateAllocationChangesHash()
        {
            var s = NewSession(77u);
            List<string> events = PassiveAwareProductionSimulation.Apply(s, MutationStat);
            for (int i = 0; i < events.Count; i++)
                StringAssert.Contains("|ok|", events[i], "替代加点也必须是合法加点（不靠改 catalog 造差异）");

            var a = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical);
            var b = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, MutationStat);
            Assert.AreEqual(0, a.InvalidCount);
            Assert.AreEqual(0, b.InvalidCount);
            Assert.AreNotEqual(a.Hash, b.Hash, "合法加点换组（有效属性不同）必须改变哈希");

            // 差异只来自被动面
            for (int i = 0; i < a.AffixDistribution.Length; i++)
                Assert.AreEqual(a.AffixDistribution[i], b.AffixDistribution[i]);
        }

        [Test]
        public void RestoringCanonicalFixtureRestoresExactHash()
        {
            string h1 = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical).Hash;
            string h2 = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, MutationAllocation).Hash;
            string h3 = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical).Hash;
            Assert.AreNotEqual(h1, h2);
            Assert.AreEqual(h1, h3, "恢复 canonical fixture 必须精确复原原哈希（无隐藏状态污染）");

            string p1 = Payload(NewCanonicalSession(101u));
            var s = NewSession(101u);
            PassiveAwareProductionSimulation.Apply(s, MutationStat);
            var s2 = NewSession(101u);
            PassiveAwareProductionSimulation.Apply(s2, Canonical);
            Assert.AreEqual(p1, Payload(s2), "重建 session 的 canonical 负载必须逐字符一致");
        }

        [Test]
        public void PassiveCollectionOrderingDoesNotChangeHash()
        {
            var canonical = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical);
            var reversed = ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Reversed);
            Assert.AreEqual(canonical.Hash, reversed.Hash, "同一集合、不同加点顺序不得改变 canonical 哈希");

            var a = NewSession(55u);
            PassiveAwareProductionSimulation.Apply(a, Canonical);
            var b = NewSession(55u);
            PassiveAwareProductionSimulation.Apply(b, Reversed);
            Assert.AreEqual(Payload(a), Payload(b), "canonical 序列化必须顺序无关（身份升序 / modifier 排序）");
        }

        // ---------- 3. 明确不得进入哈希的内容 ----------

        [Test]
        public void UIAndTreeGeometryDoNotEnterHash()
        {
            var a = NewCanonicalSession(63u);
            var b = NewCanonicalSession(63u);
            b.Panel = SlicePanel.Craft;
            b.SelectedInv = 5;
            b.CraftAffixPick = 2;
            b.HitFlash = 0.33f;
            b.LastMessage = "ui noise";
            b.LastLoot = "ui noise";
            Assert.AreEqual(Payload(a), Payload(b), "UI/交互状态不得进入被动负载");

            string payload = Payload(a).ToLowerInvariant();
            for (int i = 0; i < PassiveCensus.ForbiddenHashTokens.Length; i++)
                Assert.IsFalse(payload.Contains(PassiveCensus.ForbiddenHashTokens[i]),
                    "负载不得含非游戏真相 token：" + PassiveCensus.ForbiddenHashTokens[i]);

            Assert.Less(Payload(a).Length, 8192,
                "负载必须是有界的（canonical 节点 + 28 属性 × 3 技能），不得把整棵树/原始 JSON 塞进来");
            int[] ids = PassiveAwareProductionSimulation.AllocatedNodeIds(a);
            Assert.AreEqual(Canonical.Length + 1, ids.Length, "只有起点 + canonical fixture 进入负载");
            var allowed = new HashSet<int>(Canonical);
            allowed.Add(PassiveAwareProductionSimulation.StartNodeId);
            for (int i = 0; i < ids.Length; i++)
                Assert.IsTrue(allowed.Contains(ids[i]), "负载不得引用未分配节点：" + ids[i]);
        }

        [Test]
        public void MasteryExcludedFromWO02CanonicalFixture()
        {
            for (int i = 0; i < Canonical.Length; i++)
            {
                PoeNode n = PoeTree.Get(Canonical[i]);
                Assert.AreNotEqual(PoeNodeKind.Mastery, n.Kind);
                Assert.IsTrue(string.IsNullOrEmpty(n.choices), "fixture 节点不得带可选效果：" + Canonical[i]);
            }

            int masteryNodes = 0;
            for (int i = 0; i < PoeTree.Count; i++)
                if (PoeTree.Get(i).Kind == PoeNodeKind.Mastery)
                    masteryNodes++;
            Assert.Greater(masteryNodes, 0, "树里存在专精节点，排除才是有意义的");

            var s = NewCanonicalSession(8u);
            int[] ids = PassiveAwareProductionSimulation.AllocatedNodeIds(s);
            for (int i = 0; i < ids.Length; i++)
                Assert.AreNotEqual(PoeNodeKind.Mastery, PoeTree.Get(ids[i]).Kind, "已分配集合不得含专精：" + ids[i]);
        }

        [Test]
        public void UnsupportedNodesExcludedFromWO02CanonicalFixture()
        {
            int consumed, blocked, special, structural;
            PoeNode neighbor = PoeTree.Get(PassiveAwareProductionSimulation.UnsupportedNeighborNodeId);
            Assert.AreEqual(PassiveCensus.NodeEligibility.UnsupportedCurrently,
                PassiveCensus.ClassifyNode(neighbor, out consumed, out blocked, out special, out structural),
                "起点旁边的 unsupported 节点必须被判为 UnsupportedCurrently（排除口径非空转）");

            for (int i = 0; i < Canonical.Length; i++)
            {
                Assert.AreEqual(PassiveCensus.NodeEligibility.Supported,
                    PassiveCensus.ClassifyNode(PoeTree.Get(Canonical[i]), out consumed, out blocked, out special, out structural));
            }

            var s = NewCanonicalSession(3u);
            int[] ids = PassiveAwareProductionSimulation.AllocatedNodeIds(s);
            for (int i = 0; i < ids.Length; i++)
                Assert.AreNotEqual(PassiveCensus.NodeEligibility.UnsupportedCurrently,
                    PassiveCensus.ClassifyNode(PoeTree.Get(ids[i]), out consumed, out blocked, out special, out structural),
                    "已分配集合不得含 unsupported 节点：" + ids[i]);
        }

        // ---------- 4. 确定性与报告 ----------

        [Test]
        public void SameScenarioProducesExactSamePayload()
        {
            Assert.AreEqual(Payload(NewCanonicalSession(11u)), Payload(NewCanonicalSession(11u)));
            Assert.AreEqual(
                ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical).Hash,
                ProductionSimulator.RunWithScenario(ProductionSimulator.Seed, Canonical).Hash);
        }

        [Test]
        public void Report_DeclaresPassiveSurfaceAndSensitivity()
        {
            var r = ProductionSimulator.Run(ProductionSimulator.Seed, true);

            Assert.IsTrue(r.VerdictPass, "canonical 运行必须干净");
            Assert.AreEqual(PassiveAwareProductionSimulation.ScenarioVersion, r.PassiveScenarioVersion);
            Assert.AreEqual(Canonical.Length + 1, r.AllocatedPassiveNodeCount);
            Assert.AreEqual("559,1795,2034,2172", r.AllocatedPassiveNodeIds);
            Assert.Greater(r.PassiveModifierCount, 0);
            Assert.Greater(r.PassiveOffensiveTupleCount, 0);
            Assert.Greater(r.PassiveDefensiveOrAttributeTupleCount, 0);
            Assert.IsNotEmpty(r.PassiveEffectiveStatSnapshot);
            Assert.IsNotEmpty(r.PassiveEffectiveSkillStatSnapshot);
            Assert.AreEqual(Canonical.Length, r.PassiveAllocationEvents.Count);

            Assert.IsTrue(r.PassiveSensitive, "合同 §7：被动敏感性必须为 TRUE");
            Assert.IsTrue(r.AllocationMutationChangedHash, "A：改一个 canonical 加点 → 哈希必变");
            Assert.IsTrue(r.StatMutationChangedHash, "B：合法换组使有效属性不同 → 哈希必变");
            Assert.IsTrue(r.RestoreExactMatch, "C：恢复 canonical → 精确复原");
            Assert.IsTrue(r.OrderingInvariant, "§8：加点顺序不影响哈希");
            Assert.AreNotEqual(ProductionSimulator.PredecessorHash, r.Hash,
                "本单有意扩大 evidence surface；旧 hash 只能作 predecessor reference");

            Assert.IsTrue(System.IO.File.Exists(ProductionSimulator.ArtifactPath));
            string json = System.IO.File.ReadAllText(ProductionSimulator.ArtifactPath);
            StringAssert.Contains(PassiveAwareProductionSimulation.SimulationContractVersion, json);
            StringAssert.Contains(PassiveAwareProductionSimulation.ContractVersion, json);
            StringAssert.Contains("\"passiveSensitive\": true", json);
            StringAssert.Contains("\"allocatedPassiveNodeCount\": 4", json);
            StringAssert.Contains(ProductionSimulator.PredecessorHash, json);
        }
    }
}
