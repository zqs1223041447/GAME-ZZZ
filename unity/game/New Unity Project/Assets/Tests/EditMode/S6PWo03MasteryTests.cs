using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-03 — Mastery Explicit Selection & Allocation Correctness.
    /// 04A/04A2 的 NodeTruth 不变；本令只在状态层建立 prerequisite / explicit selection / 原子提交。
    /// </summary>
    public sealed class S6PWo03MasteryTests
    {
        const int MasteryNode = 10;
        const int ExpectedSupported = 367;
        const int ExpectedBlocked = 1660;
        const int ExpectedSpecial = 87;
        const int ExpectedMasteryPending = 315;
        const int ExpectedReachable = 1985;
        const int ExpectedStartConnectedSupported = 325;
        const int ExpectedStartDisconnectedSupported = 42;

        static SliceSession NewSession()
        {
            var s = new SliceSession();
            s.ResetTown(7u);
            return s;
        }

        static int SupportedOrdinal(int mastery)
        {
            int n = PassiveSupport.ChoiceCount(mastery);
            for (int i = 0; i < n; i++)
                if (PassiveSupport.IsChoiceSelectable(PassiveSupport.ChoiceAt(mastery, i)))
                    return i;
            return -1;
        }

        static int ClusterNotable(int mastery)
        {
            int g = PoeTree.Get(mastery).group;
            int found = -1;
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].group == g && nodes[i].Kind == PoeNodeKind.Notable)
                {
                    if (found < 0 || i < found)
                        found = i;
                }
            }
            return found;
        }

        static int UnrelatedNotable(int mastery)
        {
            int g = PoeTree.Get(mastery).group;
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Kind != PoeNodeKind.Notable)
                    continue;
                if (nodes[i].group == g)
                    continue;
                if (PassiveSupport.IsTraversable(PassiveSupport.EvaluateTruth(i).Traversal))
                    return i;
            }
            return -1;
        }

        static bool TryAllocatePath(SliceSession s, int target, out string err)
        {
            err = null;
            if (target < 0)
            {
                err = "无目标";
                return false;
            }
            if (s.Allocated[target])
                return true;

            int n = PoeTree.Count;
            var parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = -2;
            var q = new int[n];
            int head = 0, tail = 0;
            int start = SliceSession.StartNode;
            parent[start] = -1;
            q[tail++] = start;
            while (head < tail)
            {
                int cur = q[head++];
                int[] links = PoeTree.Get(cur).links;
                if (links == null)
                    continue;
                for (int i = 0; i < links.Length; i++)
                {
                    int nb = links[i];
                    if (nb < 0 || nb >= n || parent[nb] != -2)
                        continue;
                    bool dest = nb == target;
                    bool trav = PassiveSupport.IsTraversable(PassiveSupport.EvaluateTruth(nb).Traversal);
                    if (!trav && !dest)
                        continue;
                    parent[nb] = cur;
                    q[tail++] = nb;
                }
            }
            if (parent[target] == -2)
            {
                err = "不可达";
                return false;
            }
            var path = new List<int>();
            for (int x = target; x != start; x = parent[x])
                path.Add(x);
            path.Reverse();
            for (int i = 0; i < path.Count; i++)
            {
                int id = path[i];
                if (s.Allocated[id])
                    continue;
                if (PoeTree.Get(id).Kind == PoeNodeKind.Mastery)
                    continue;
                if (!s.TryAllocate(id, out err))
                    return false;
            }
            return true;
        }

        static bool PrepareMasteryEligible(SliceSession s, int mastery, out string err)
        {
            int notable = ClusterNotable(mastery);
            if (!TryAllocatePath(s, notable, out err))
                return false;
            return s.Allocated[notable];
        }

        static int CountAllocated(SliceSession s)
        {
            int n = 0;
            for (int i = 0; i < s.Allocated.Length; i++)
                if (s.Allocated[i])
                    n++;
            return n;
        }

        static void Snapshot(SliceSession s, out int unspent, out int allocated, out float life, out int masteryOrd)
        {
            unspent = s.Unspent;
            allocated = CountAllocated(s);
            life = s.PlayerStats.Get(StatId.Life);
            masteryOrd = s.MasterySelectedOrdinal(MasteryNode);
        }

        [Test]
        public void ChoiceCensus_MatchesPreflight()
        {
            int masteries = 0, choices = 0, supported = 0, withOne = 0, withTwo = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (PoeTree.Get(i).Kind != PoeNodeKind.Mastery)
                    continue;
                masteries++;
                int n = PassiveSupport.ChoiceCount(i);
                choices += n;
                int ok = PassiveSupport.CountSupportedChoices(i);
                supported += ok;
                if (ok >= 1) withOne++;
                if (ok >= 2) withTwo++;
            }
            Assert.AreEqual(315, masteries);
            Assert.AreEqual(1863, choices);
            Assert.AreEqual(22, supported);
            Assert.AreEqual(22, withOne);
            Assert.AreEqual(0, withTwo);
            Assert.IsFalse(PassiveSupport.SourceChoiceHasStableId, "官方 payload 无 per-choice ID");
            StringAssert.Contains("fallback:MasteryNodeId+sourceOrdinal", PassiveSupport.ChoiceIdentityScheme);
        }

        [Test]
        public void LegacyCensus_Unchanged_367_315()
        {
            int supported = 0, blocked = 0, special = 0, mastery = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                switch (PassiveSupport.EvaluateNode(i))
                {
                    case PassiveSupport.NodeStatus.AllocatableSupported: supported++; break;
                    case PassiveSupport.NodeStatus.BlockedCurrently: blocked++; break;
                    case PassiveSupport.NodeStatus.BlockedSpecialInteraction: special++; break;
                    case PassiveSupport.NodeStatus.SpecialPendingMastery: mastery++; break;
                }
                if (PoeTree.Get(i).Kind == PoeNodeKind.Mastery)
                {
                    var t = PassiveSupport.EvaluateTruth(i);
                    Assert.AreEqual(PassiveSupport.EffectTruth.SpecialPending, t.Effect, "EA-4 静态 Effect 不得改：" + i);
                    Assert.AreEqual(PassiveSupport.TraversalTruth.SpecialBlocked, t.Traversal, "EA-2 静态 Traversal 不得改：" + i);
                }
            }
            Assert.AreEqual(ExpectedSupported, supported, "before=367 after=367 delta=0 reason=EA-4 NodeTruth 不因选择能力移动");
            Assert.AreEqual(ExpectedBlocked, blocked);
            Assert.AreEqual(ExpectedSpecial, special);
            Assert.AreEqual(ExpectedMasteryPending, mastery, "before=315 after=315 delta=0 reason=选择能力是状态层");
        }

        [Test]
        public void TopologyInvariants_Unchanged()
        {
            bool[] reach = PassiveSupport.ReachableSet();
            int reachable = 0, d = 0, disconnected = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (reach[i]) reachable++;
                var t = PassiveSupport.EvaluateTruth(i);
                if (t.Effect == PassiveSupport.EffectTruth.FullySupported
                    && t.Traversal == PassiveSupport.TraversalTruth.Traversable)
                {
                    if (reach[i]) d++;
                    else disconnected++;
                }
            }
            Assert.AreEqual(ExpectedReachable, reachable);
            Assert.AreEqual(ExpectedStartConnectedSupported, d);
            Assert.AreEqual(ExpectedStartDisconnectedSupported, disconnected);
        }

        [Test]
        public void Prerequisite_SameClusterPositive_UnrelatedNegative_NoNotableNegative()
        {
            int notable = ClusterNotable(MasteryNode);
            int unrelated = UnrelatedNotable(MasteryNode);
            Assert.GreaterOrEqual(notable, 0);
            Assert.GreaterOrEqual(unrelated, 0);
            Assert.AreNotEqual(PoeTree.Get(notable).group, PoeTree.Get(unrelated).group);

            var empty = NewSession();
            Assert.IsTrue(PassiveSupport.MasteryInOfficialCluster(MasteryNode));
            Assert.IsFalse(PassiveSupport.MasteryPrerequisiteMet(MasteryNode, empty.Allocated), "no-notable");

            var unrelatedS = NewSession();
            unrelatedS.Allocated[unrelated] = true;
            Assert.IsFalse(PassiveSupport.MasteryPrerequisiteMet(MasteryNode, unrelatedS.Allocated), "unrelated-cluster");

            var same = NewSession();
            same.Allocated[notable] = true;
            Assert.IsTrue(PassiveSupport.MasteryPrerequisiteMet(MasteryNode, same.Allocated), "same-cluster");
            Assert.AreEqual(notable, PassiveSupport.FirstAllocatedClusterNotable(MasteryNode, same.Allocated));
        }

        [Test]
        public void ChoiceSupport_Reuses04A_NoSecondOracle()
        {
            int ord = SupportedOrdinal(MasteryNode);
            Assert.GreaterOrEqual(ord, 0);
            string line = PassiveSupport.ChoiceAt(MasteryNode, ord);
            Assert.AreEqual("+30 to maximum Life", line);
            string domain;
            Assert.AreEqual(PassiveSupport.LineBucket.Consumed, PassiveSupport.ClassifyLine(line, out domain));
            Assert.IsTrue(PassiveSupport.IsChoiceSelectable(line));

            int blocked = 0;
            int n = PassiveSupport.ChoiceCount(MasteryNode);
            for (int i = 0; i < n; i++)
                if (!PassiveSupport.IsChoiceSelectable(PassiveSupport.ChoiceAt(MasteryNode, i)))
                    blocked++;
            Assert.Greater(blocked, 0);

            string src = File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Runtime/Core/Gameplay/PassiveSupport.cs")));
            StringAssert.DoesNotContain("MasterySupportTable", src);
            StringAssert.DoesNotContain("MasteryBlockedKeywords", src);
            StringAssert.DoesNotContain("MasterySpecificParserRules", src);
        }

        [Test]
        public void RegularTryAllocate_StillRejectsMastery()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            int unspent = s.Unspent;
            Assert.IsFalse(s.TryAllocate(MasteryNode, out err));
            Assert.AreEqual(PassiveSupport.ReasonMasteryPending, err);
            Assert.AreEqual(unspent, s.Unspent);
            Assert.IsFalse(s.Allocated[MasteryNode]);
        }

        [Test]
        public void AtomicCommit_SupportedChoice_ExactlyOnce()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            int ord = SupportedOrdinal(MasteryNode);
            float lifeBefore = s.PlayerStats.Get(StatId.Life);
            int unspent = s.Unspent;
            int allocated = CountAllocated(s);

            Assert.IsTrue(s.CanEnterMasterySelection(MasteryNode));
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, ord, out err), err);
            Assert.IsTrue(s.Allocated[MasteryNode]);
            Assert.AreEqual(ord, s.MasterySelectedOrdinal(MasteryNode));
            Assert.AreEqual(unspent - 1, s.Unspent);
            Assert.AreEqual(allocated + 1, CountAllocated(s));
            Assert.AreEqual(lifeBefore + 30f, s.PlayerStats.Get(StatId.Life), 0.0001f);
            Assert.AreEqual(0, s.BlockedAllocatedCount, "合法显式选择不得算 corruption");

            Modifier[] mods = s.EffectivePassiveMods(MasteryNode);
            Assert.AreEqual(1, mods.Length);
            Assert.AreEqual(StatId.Life, mods[0].Stat);
            Assert.AreEqual(ModOp.Flat, mods[0].Op);
            Assert.AreEqual(30f, mods[0].Value, 0.0001f);
            Assert.AreEqual(0, PassiveCatalog.Get(MasteryNode).Mods.Length, "不得烘焙 FirstChoice");
        }

        [Test]
        public void BlockedChoice_AndMissingSelection_ZeroDelta()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            int blockedOrd = -1;
            int n = PassiveSupport.ChoiceCount(MasteryNode);
            for (int i = 0; i < n; i++)
                if (!PassiveSupport.IsChoiceSelectable(PassiveSupport.ChoiceAt(MasteryNode, i)))
                {
                    blockedOrd = i;
                    break;
                }
            Assert.GreaterOrEqual(blockedOrd, 0);

            int unspent, allocated, masteryOrd;
            float life;
            Snapshot(s, out unspent, out allocated, out life, out masteryOrd);

            Assert.IsFalse(s.TryAllocateMastery(MasteryNode, blockedOrd, out err));
            Assert.AreEqual(PassiveSupport.ReasonMasteryChoiceBlocked, err);
            int u2, a2, m2;
            float l2;
            Snapshot(s, out u2, out a2, out l2, out m2);
            Assert.AreEqual(unspent, u2);
            Assert.AreEqual(allocated, a2);
            Assert.AreEqual(life, l2, 0.0001f);
            Assert.AreEqual(-1, m2);

            Assert.IsFalse(s.TryAllocateMastery(MasteryNode, 99, out err));
            Assert.AreEqual(PassiveSupport.ReasonMasteryChoiceRange, err);
            Snapshot(s, out u2, out a2, out l2, out m2);
            Assert.AreEqual(unspent, u2);
            Assert.AreEqual(life, l2, 0.0001f);
        }

        [Test]
        public void InjectedMasteryWithoutSelection_IsCorruption_AndZeroEffect()
        {
            var s = NewSession();
            float life = s.PlayerStats.Get(StatId.Life);
            s.Allocated[MasteryNode] = true;
            s.RecalcPlayer(false);
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f);
            Assert.AreEqual(1, s.BlockedAllocatedCount);
            Assert.AreEqual(0, s.EffectivePassiveMods(MasteryNode).Length);
        }

        [Test]
        public void Respec_ClearsAllocationSelectionAndEffect()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, SupportedOrdinal(MasteryNode), out err), err);
            Assert.Greater(s.PlayerStats.Get(StatId.Life), 0f);
            Assert.IsTrue(s.TryRespec(out err), err);
            Assert.IsFalse(s.Allocated[MasteryNode]);
            Assert.AreEqual(-1, s.MasterySelectedOrdinal(MasteryNode));
            var baseline = NewSession();
            Assert.AreEqual(baseline.PlayerStats.Get(StatId.Life), s.PlayerStats.Get(StatId.Life), 0.0001f);
        }

        [Test]
        public void MasteryIsNotTransitVertex()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, SupportedOrdinal(MasteryNode), out err), err);

            int[] links = PoeTree.Get(MasteryNode).links;
            for (int i = 0; i < links.Length; i++)
            {
                int nb = links[i];
                if (s.Allocated[nb])
                    continue;
                Assert.IsFalse(s.CanAllocate(nb), "专精不得成为后续节点的 transit：" + nb);
            }
        }

        [Test]
        public void SelectorLayout_FitsDesignSpace()
        {
            Rect p1080 = SliceHud.MasterySelectorPanel(1920f, 1080f);
            Assert.GreaterOrEqual(p1080.x, 0f);
            Assert.GreaterOrEqual(p1080.y, 0f);
            Assert.LessOrEqual(p1080.xMax, 1920f + 0.01f);
            Assert.LessOrEqual(p1080.yMax, 1080f + 0.01f);

            float scale = SliceHud.DesignScale(2560f, 1440f);
            float dw = 2560f / scale, dh = 1440f / scale;
            Rect p1440 = SliceHud.MasterySelectorPanel(dw, dh);
            Assert.GreaterOrEqual(p1440.x, 0f);
            Assert.GreaterOrEqual(p1440.y, 0f);
            Assert.LessOrEqual(p1440.xMax, dw + 0.01f);
            Assert.LessOrEqual(p1440.yMax, dh + 0.01f);

            int n = PassiveSupport.ChoiceCount(MasteryNode);
            Assert.AreEqual(n, n, "entry count == source count");
            for (int i = 0; i < n; i++)
            {
                Rect row = SliceHud.MasterySelectorChoiceRow(p1080, i);
                Assert.GreaterOrEqual(row.x, p1080.x);
                Assert.LessOrEqual(row.xMax, p1080.xMax + 0.01f);
            }
            Assert.AreEqual(PassiveSupport.MasteryChoiceKey(MasteryNode, 0),
                PassiveSupport.MasteryChoiceKey(MasteryNode, 0));
        }

        [Test]
        public void ProdSimV3_UnselectedVsSelected_ChangesHashAndLife()
        {
            var eligible = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(eligible, MasteryNode, out err), err);
            PassiveAwareProductionSimulation.ApplyCanonical(eligible);
            string payloadUnselected = PassiveAwareProductionSimulation.StatePayload(eligible);
            StringAssert.StartsWith("pv|passive-v2", payloadUnselected);
            StringAssert.Contains("px|", payloadUnselected);
            Assert.IsFalse(Section(payloadUnselected, "px|").Contains(MasteryNode.ToString(CultureInfo.InvariantCulture) + ":"),
                "未选择时 px| 不得含该专精 identity");

            var selected = NewSession();
            Assert.IsTrue(PrepareMasteryEligible(selected, MasteryNode, out err), err);
            PassiveAwareProductionSimulation.ApplyCanonical(selected);
            int ord = SupportedOrdinal(MasteryNode);
            Assert.IsTrue(selected.TryAllocateMastery(MasteryNode, ord, out err), err);
            string payloadSelected = PassiveAwareProductionSimulation.StatePayload(selected);
            Assert.AreNotEqual(payloadUnselected, payloadSelected);
            StringAssert.Contains("px|" + PassiveSupport.MasteryChoiceKey(MasteryNode, ord), payloadSelected);
            Assert.Greater(selected.PlayerStats.Get(StatId.Life), eligible.PlayerStats.Get(StatId.Life));

            ulong hU = Fnv(payloadUnselected);
            ulong hS = Fnv(payloadSelected);
            Assert.AreNotEqual(hU, hS);

            Assert.AreEqual("pv|passive-v2", PassiveAwareProductionSimulation.ContractVersion);
            StringAssert.DoesNotContain("Enum.ToString", payloadSelected);
        }

        [Test]
        public void NoImplicitFirstChoice()
        {
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (PoeTree.Get(i).Kind != PoeNodeKind.Mastery)
                    continue;
                Assert.AreEqual(0, PassiveCatalog.Get(i).Mods.Length, "FirstChoice 禁止：" + i);
            }
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMasteryEligible(s, MasteryNode, out err), err);
            float life = s.PlayerStats.Get(StatId.Life);
            Assert.IsFalse(s.Allocated[MasteryNode]);
            s.RecalcPlayer(false);
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f);
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
    }
}
