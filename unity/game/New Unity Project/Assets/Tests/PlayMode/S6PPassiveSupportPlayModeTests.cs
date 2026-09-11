using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// S6P-WO-04A 集成对照（PlayMode，真实 ArenaSim/SliceSession，不用像素自动化）：
    ///   · supported 节点 → 真实分配 → 玩家/技能有效结果真的变化（正对照）
    ///   · unsupported 节点 → 分配被拒 → 点数/状态/结果零变化（负对照）
    ///   · 专精 → 不可用 → 零效果
    /// </summary>
    public sealed class S6PPassiveSupportPlayModeTests
    {
        const int SupportedDefensiveNode = 2034;   // +12 to maximum Life / +5 to Strength
        const int SupportedOffensiveNode = 559;    // 4% increased Attack Speed / +5 to Dexterity
        const int UnsupportedNeighbour = 71;       // 起点邻居；含 blocked 行（Mana Regeneration）
        const int MasteryNode = 10;

        static ArenaSim NewSim()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            sim.Session.ResetTown(12345u);
            sim.Caster.Defs = sim.Session.ResolveSkillDef;
            return sim;
        }

        [UnityTest]
        public IEnumerator SupportedNode_Allocate_ChangesActualPlayerAndSkillResult()
        {
            var sim = NewSim();
            yield return null;
            var s = sim.Session;

            var baseline = new ArenaSim();
            baseline.Reset();
            baseline.Session = new SliceSession();
            baseline.Session.ResetTown(12345u);
            baseline.Caster.Defs = baseline.Session.ResolveSkillDef;

            float lifeBefore = s.PlayerStats.Get(StatId.Life);
            var bagBefore = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagBefore);
            float speedBefore = bagBefore.RawIncreased(StatId.AttackSpeed);
            float recoveryBefore = s.ResolveSkillDef(SkillId.Melee).Recovery;

            string err;
            Assert.IsTrue(s.TryAllocate(SupportedDefensiveNode, out err), err);
            Assert.Greater(s.PlayerStats.Get(StatId.Life), lifeBefore, "supported 防御节点必须真的改变玩家有效结果");
            Assert.Greater(s.PlayerStats.Get(StatId.Strength), baseline.Session.PlayerStats.Get(StatId.Strength));
            Assert.Greater(s.MaxLife, baseline.Session.MaxLife);

            Assert.IsTrue(s.TryAllocate(SupportedOffensiveNode, out err), err);
            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            Assert.Greater(bagAfter.RawIncreased(StatId.AttackSpeed), speedBefore,
                "supported 进攻节点必须真的进入技能上下文");
            Assert.Less(s.ResolveSkillDef(SkillId.Melee).Recovery, recoveryBefore,
                "技能形态消费者必须真的看到该变化");
        }

        [UnityTest]
        public IEnumerator UnsupportedNode_Reject_LeavesPointsStateAndResultsUnchanged()
        {
            var sim = NewSim();
            yield return null;
            var s = sim.Session;

            var bagBefore = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagBefore);
            float life = s.PlayerStats.Get(StatId.Life);
            float str = s.PlayerStats.Get(StatId.Strength);
            int unspent = s.Unspent;
            int allocatedBefore = CountAllocated(s);

            string err;
            Assert.IsFalse(s.TryAllocate(UnsupportedNeighbour, out err), "unsupported 节点必须被拒绝");
            Assert.AreEqual(PassiveSupport.ReasonBlockedCurrently, err);
            Assert.AreEqual(unspent, s.Unspent, "点数零变化");
            Assert.AreEqual(allocatedBefore, CountAllocated(s), "已分配集合零变化");
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f, "玩家结果零变化");
            Assert.AreEqual(str, s.PlayerStats.Get(StatId.Strength), 0.0001f);

            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            for (int i = 0; i < (int)StatId.Count; i++)
                Assert.AreEqual(bagBefore.Get((StatId)i), bagAfter.Get((StatId)i), 0.0001f, "技能结果零变化：" + (StatId)i);

            Assert.IsFalse(s.CanAllocate(UnsupportedNeighbour), "UI 不得把 unsupported 节点显示为可点");
            Assert.AreEqual(NodeUiState.Locked, s.NodeState(UnsupportedNeighbour));
            Assert.IsNotNull(s.NodeBlockReason(UnsupportedNeighbour), "UI 必须能取到稳定原因");
        }

        [UnityTest]
        public IEnumerator Mastery_Unavailable_And_NoEffect()
        {
            var sim = NewSim();
            yield return null;
            var s = sim.Session;

            int neighbour = PoeTree.Get(MasteryNode).links[0];
            s.Allocated[neighbour] = true;      // 相连性成立（不走生产分配 API）

            float lifeBefore = s.PlayerStats.Get(StatId.Life);
            int unspent = s.Unspent;
            string err;
            Assert.IsFalse(s.TryAllocate(MasteryNode, out err), "专精在 WO-03 前必须不可分配");
            Assert.AreEqual(PassiveSupport.ReasonMasteryPending, err);
            Assert.AreEqual(unspent, s.Unspent, "专精拒绝必须零消耗");
            Assert.IsFalse(s.Allocated[MasteryNode]);

            // 即便损坏状态里分配了专精，也不得有任何效果
            s.Allocated[MasteryNode] = true;
            s.RecalcPlayer(false);
            Assert.AreEqual(lifeBefore, s.PlayerStats.Get(StatId.Life), 0.0001f, "专精不得产生任何效果");
            Assert.AreEqual(0, PassiveCatalog.Get(MasteryNode).Mods.Length, "专精不得烘焙隐式 modifier");
        }

        static int CountAllocated(SliceSession s)
        {
            int n = 0;
            for (int i = 0; i < s.Allocated.Length; i++)
                if (s.Allocated[i])
                    n++;
            return n;
        }
    }
}
