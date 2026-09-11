using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-04B — Passive Build Identity / Snapshot / Lock Parity。
    /// 只治理已有 snapshot / lock / reset 边界；不发明存档。
    /// </summary>
    public sealed class S6PWo04BIdentityTests
    {
        const int MasteryNode = 10;
        const int CanonicalA = 559;
        const int CanonicalB = 1795;
        const int CanonicalC = 2034;

        static SliceSession NewSession()
        {
            var s = new SliceSession();
            s.ResetTown(11u);
            return s;
        }

        static ArenaSim NewSim(SliceSession s)
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = s;
            sim.Caster.Defs = s.ResolveSkillDef;
            return sim;
        }

        static int SupportedOrdinal(int mastery)
        {
            int n = PassiveSupport.ChoiceCount(mastery);
            for (int i = 0; i < n; i++)
                if (PassiveSupport.IsChoiceSelectable(PassiveSupport.ChoiceAt(mastery, i)))
                    return i;
            return -1;
        }

        static bool TryAllocatePath(SliceSession s, int target, out string err)
        {
            err = null;
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

        static bool PrepareMastery(SliceSession s, out string err)
        {
            int notable = -1;
            int g = PoeTree.Get(MasteryNode).group;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (PoeTree.Get(i).group == g && PoeTree.Get(i).Kind == PoeNodeKind.Notable)
                {
                    if (notable < 0 || i < notable)
                        notable = i;
                }
            }
            return TryAllocatePath(s, notable, out err);
        }

        [Test]
        public void IdentityScan_NoCompactMask_NoSaveSeam()
        {
            string session = File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Runtime/Core/Gameplay/SliceSession.cs")));
            StringAssert.DoesNotContain("PassiveMask", session, "现役代码不得再有 32 位 PassiveMask");
            StringAssert.Contains("long PassiveHash", session);
            StringAssert.Contains("bool[] Allocated", session);
            StringAssert.Contains("int[] MasteryChoice", session);
            StringAssert.DoesNotContain("File.WriteAll", session);
            StringAssert.DoesNotContain("BinaryFormatter", session);
            Assert.IsFalse(session.Contains("JsonUtility.ToJson(this)"), "不得把 session 当存档序列化");
        }

        [Test]
        public void CanonicalIdentity_IsSortedAndOrderIndependent()
        {
            var a = NewSession();
            var b = NewSession();
            string err;
            Assert.IsTrue(a.TryAllocate(CanonicalA, out err), err);
            Assert.IsTrue(a.TryAllocate(CanonicalB, out err), err);
            Assert.IsTrue(a.TryAllocate(CanonicalC, out err), err);
            Assert.IsTrue(b.TryAllocate(CanonicalC, out err), err);
            Assert.IsTrue(b.TryAllocate(CanonicalB, out err), err);
            Assert.IsTrue(b.TryAllocate(CanonicalA, out err), err);
            Assert.AreEqual(a.CanonicalAllocatedIds(), b.CanonicalAllocatedIds());
            Assert.AreEqual(a.ComputePassiveHash(), b.ComputePassiveHash());
            StringAssert.Contains("2172", a.CanonicalAllocatedIds());
            Assert.IsTrue(IsSortedCsv(a.CanonicalAllocatedIds()));
        }

        [Test]
        public void BuildSnapshot_SchemaMatchesCurrentContract()
        {
            var names = new List<string>();
            foreach (var f in typeof(BuildSnapshot).GetFields())
                names.Add(f.Name);
            names.Sort(System.StringComparer.Ordinal);
            CollectionAssert.AreEqual(new[]
            {
                "BeltId", "BodyId", "BootsId", "E0", "GlovesId", "HelmetId", "Locked",
                "PassiveHash", "Q0", "Q1", "Unspent", "W0", "W1", "WeaponId"
            }, names.ToArray(), "不得给 snapshot 加 NodeId 列表/存档字段");
            Assert.AreEqual(typeof(long), typeof(BuildSnapshot).GetField("PassiveHash").FieldType);
        }

        [Test]
        public void LiveToSnapshot_FingerprintMatchesLive_NotASaveObject()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(s.TryAllocate(CanonicalA, out err), err);
            Assert.IsTrue(s.TryAllocate(CanonicalB, out err), err);
            Assert.IsTrue(s.TryAllocate(CanonicalC, out err), err);
            Assert.IsTrue(PrepareMastery(s, out err), err);
            int ord = SupportedOrdinal(MasteryNode);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, ord, out err), err);

            long liveHash = s.ComputePassiveHash();
            string liveIds = s.CanonicalAllocatedIds();
            int unspent = s.Unspent;
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            Assert.AreEqual(liveHash, s.Snapshot.PassiveHash, "snapshot fingerprint = 进图前 live");
            Assert.AreEqual(unspent, s.Snapshot.Unspent);
            Assert.AreEqual(s.Equipped[0], s.Snapshot.WeaponId);
            Assert.AreEqual(s.Equipped[1], s.Snapshot.BodyId);
            Assert.AreEqual(s.Equipped[2], s.Snapshot.HelmetId);
            Assert.AreEqual(s.Equipped[3], s.Snapshot.BootsId);
            Assert.AreEqual(s.Equipped[4], s.Snapshot.GlovesId);
            Assert.AreEqual(s.Equipped[5], s.Snapshot.BeltId);
            Assert.AreEqual(s.QSupports[0], s.Snapshot.Q0);
            Assert.AreEqual(s.WSupports[0], s.Snapshot.W0);
            Assert.AreEqual(s.ESupports[0], s.Snapshot.E0);
            Assert.IsTrue(s.Snapshot.Locked);
            Assert.AreEqual(liveIds, s.CanonicalAllocatedIds(), "进图不得改 live allocation");
            Assert.AreEqual(liveHash, s.ComputePassiveHash());
        }

        [Test]
        public void PassiveHash_RouteOnly71_IsIdentitySensitive()
        {
            var s = NewSession();
            long h0 = s.ComputePassiveHash();
            string err;
            Assert.IsTrue(s.TryAllocate(71, out err), err);
            Assert.IsTrue(s.Allocated[71]);
            Assert.AreNotEqual(h0, s.ComputePassiveHash());
            StringAssert.Contains("71", s.CanonicalAllocatedIds());
            Assert.AreEqual(0, s.EffectivePassiveMods(71).Length);
        }

        [Test]
        public void PassiveHash_Mastery10Choice_IsIdentitySensitive()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMastery(s, out err), err);
            long before = s.ComputePassiveHash();
            string pxBefore = s.CanonicalMasterySelections();
            int ord = SupportedOrdinal(MasteryNode);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, ord, out err), err);
            Assert.AreNotEqual(before, s.ComputePassiveHash());
            Assert.AreNotEqual(pxBefore, s.CanonicalMasterySelections());
            StringAssert.Contains("10", s.CanonicalAllocatedIds());
            StringAssert.Contains(PassiveSupport.MasteryChoiceKey(MasteryNode, ord), s.CanonicalMasterySelections());
        }

        [Test]
        public void FailedMapEntry_DoesNotReplaceSnapshot()
        {
            var s = NewSession();
            string err;
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            long hash = s.Snapshot.PassiveHash;
            int weapon = s.Snapshot.WeaponId;
            Assert.IsFalse(s.TryEnterMap(sim, out err));
            Assert.AreEqual(hash, s.Snapshot.PassiveHash);
            Assert.AreEqual(weapon, s.Snapshot.WeaponId);
            Assert.AreEqual(MapState.InMap, s.State);
        }

        [Test]
        public void Cleared_UnlocksAndCannotReenterUntilExit()
        {
            var s = NewSession();
            string err;
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            s.State = MapState.Cleared;
            s.Snapshot.Locked = false;
            Assert.IsFalse(s.BuildLocked);
            Assert.IsFalse(s.Snapshot.Locked);
            Assert.IsTrue(s.OnMap);
            Assert.IsFalse(s.TryEnterMap(sim, out err), "Cleared 必须先 Exit");
            s.ExitMap(sim, false);
            Assert.AreEqual(MapState.Town, s.State);
            Assert.IsFalse(s.BuildLocked);
        }

        [Test]
        public void ProdSim_DoesNotConsumeBuildSnapshot()
        {
            string src = File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Tests/EditMode/ProductionSimulator.cs")));
            string aware = File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath,
                "Tests/EditMode/PassiveAwareProductionSimulation.cs")));
            Assert.IsFalse(src.Contains("BuildSnapshot") || src.Contains(".Snapshot"),
                "ProdSim 不得读 BuildSnapshot");
            Assert.IsFalse(aware.Contains("BuildSnapshot") || aware.Contains(".Snapshot"));
            Assert.AreEqual("V3", PassiveAwareProductionSimulation.SimulationContractVersion);
            Assert.AreEqual("pv|passive-v2", PassiveAwareProductionSimulation.ContractVersion);
        }

        [Test]
        public void Serializer_RouteOnlyAndMasteryParity()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(s.TryAllocate(71, out err), err);
            string payload = PassiveAwareProductionSimulation.StatePayload(s);
            StringAssert.Contains("ps|", payload);
            StringAssert.Contains("71", payload);
            Assert.AreEqual(0, s.EffectivePassiveMods(71).Length);

            Assert.IsTrue(PrepareMastery(s, out err), err);
            int ord = SupportedOrdinal(MasteryNode);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, ord, out err), err);
            payload = PassiveAwareProductionSimulation.StatePayload(s);
            StringAssert.Contains("10", payload);
            StringAssert.Contains("px|" + PassiveSupport.MasteryChoiceKey(MasteryNode, ord), payload);
        }

        [Test]
        public void ResetTown_SameSeed_EqualsFresh()
        {
            var fresh = NewSession();
            var dirty = NewSession();
            string err;
            Assert.IsTrue(dirty.TryAllocate(CanonicalA, out err), err);
            dirty.ResetTown(11u);
            Assert.AreEqual(fresh.CanonicalAllocatedIds(), dirty.CanonicalAllocatedIds());
            Assert.AreEqual(fresh.CanonicalMasterySelections(), dirty.CanonicalMasterySelections());
            Assert.AreEqual(fresh.ComputePassiveHash(), dirty.ComputePassiveHash());
            Assert.AreEqual(fresh.Unspent, dirty.Unspent);
            Assert.AreEqual(default(BuildSnapshot).PassiveHash, dirty.Snapshot.PassiveHash);
            Assert.IsFalse(dirty.Snapshot.Locked);
        }

        [Test]
        public void DeathRespec_ClearsMasteryIdentity()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMastery(s, out err), err);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, SupportedOrdinal(MasteryNode), out err), err);
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            s.ExitMap(sim, true);
            Assert.IsFalse(s.Allocated[MasteryNode]);
            Assert.AreEqual(-1, s.MasterySelectedOrdinal(MasteryNode));
            Assert.AreEqual("", s.CanonicalMasterySelections());
        }

        [Test]
        public void BuildLock_RejectsAllocateRespecMastery_NoDrift()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(s.TryAllocate(CanonicalA, out err), err);
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            Assert.IsTrue(s.BuildLocked);

            string ids = s.CanonicalAllocatedIds();
            string mx = s.CanonicalMasterySelections();
            long hash = s.Snapshot.PassiveHash;
            int unspent = s.Unspent;
            float life = s.PlayerStats.Get(StatId.Life);

            Assert.IsFalse(s.TryAllocate(CanonicalB, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryRespec(out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryAllocateMastery(MasteryNode, 0, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryEquip(0, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TrySetSupport(SkillId.Melee, 0, SupportId.Fork, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryRandomCraft(0, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryDirectedCraft(0, AffixId.Life, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(s.TryReassignLink(0, SkillId.Melee, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);

            Assert.AreEqual(ids, s.CanonicalAllocatedIds());
            Assert.AreEqual(mx, s.CanonicalMasterySelections());
            Assert.AreEqual(hash, s.ComputePassiveHash());
            Assert.AreEqual(unspent, s.Unspent);
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f);
            Assert.AreEqual(hash, s.Snapshot.PassiveHash);
        }

        [Test]
        public void Unlock_RestoresAllocate()
        {
            var s = NewSession();
            string err;
            var sim = NewSim(s);
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);
            Assert.IsFalse(s.TryAllocate(CanonicalA, out err));
            s.ExitMap(sim, false);
            Assert.IsFalse(s.BuildLocked);
            Assert.IsTrue(s.TryAllocate(CanonicalA, out err), err);
            Assert.IsTrue(s.Allocated[CanonicalA]);
        }

        [Test]
        public void Respec_ClearsAllocationAndMasteryResidue()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(PrepareMastery(s, out err), err);
            Assert.IsTrue(s.TryAllocateMastery(MasteryNode, SupportedOrdinal(MasteryNode), out err), err);
            Assert.IsTrue(s.TryRespec(out err), err);
            Assert.IsTrue(s.Allocated[SliceSession.StartNode]);
            for (int i = 0; i < s.Allocated.Length; i++)
            {
                if (i == SliceSession.StartNode)
                    continue;
                Assert.IsFalse(s.Allocated[i], "respec 残留分配：" + i);
            }
            for (int i = 0; i < s.MasteryChoice.Length; i++)
                Assert.AreEqual(-1, s.MasteryChoice[i], "respec 残留专精选择：" + i);
            Assert.AreEqual("", s.CanonicalMasterySelections());
        }

        [Test]
        public void CloneAndPersistenceSeams_AreNotApplicable()
        {
            string runtimeDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Runtime/Core/Gameplay"));
            string[] files = Directory.GetFiles(runtimeDir, "*.cs");
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                if (name == "ArenaPerfHarness.cs" || name == "PerfSampler.cs")
                    continue;
                string src = File.ReadAllText(files[i]);
                Assert.IsFalse(src.Contains("ISerializable"), name);
                Assert.IsFalse(src.Contains("BinaryFormatter"), name);
            }
        }

        static bool IsSortedCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return true;
            string[] parts = csv.Split(',');
            int prev = int.MinValue;
            for (int i = 0; i < parts.Length; i++)
            {
                int v;
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                    return false;
                if (v < prev)
                    return false;
                prev = v;
            }
            return true;
        }
    }
}
