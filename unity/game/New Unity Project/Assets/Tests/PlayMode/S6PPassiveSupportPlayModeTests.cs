using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// S6P-WO-04A 集成对照（PlayMode，真实 ArenaSim/SliceSession，不用像素自动化）；
    /// S6P-WO-04A2 起改为**通行 / 生效分离**的两条对照：
    ///   · supported 节点 → 真实分配 → 玩家/技能有效结果真的变化（正对照）
    ///   · route-only 节点 → 分配合法、扣点、进 selected 集，但 gameplay 结果零变化
    ///   · 专精 / 珠宝孔 → 不可通行 → 拒绝且零扣点（负对照）
    /// </summary>
    public sealed class S6PPassiveSupportPlayModeTests
    {
        const int SupportedDefensiveNode = 2034;   // +12 to maximum Life / +5 to Strength
        const int SupportedOffensiveNode = 559;    // 4% increased Attack Speed / +5 to Dexterity
        const int UnsupportedNeighbour = 71;       // 起点邻居；含 blocked 行 ⇒ route-only
        const int MasteryNode = 10;
        const int JewelNode = 78;                  // 珠宝孔：kind=Jewel，非通行

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
        public IEnumerator RouteOnlyNode_Allocate_SpendsPoint_ButYieldsNoEffect()
        {
            var sim = NewSim();
            yield return null;
            var s = sim.Session;

            // S6P-WO-04A2：route-only 节点（通行合法、效果未兑现）在 UI 上就是"可点"，且没有不可点原因。
            Assert.IsTrue(s.CanAllocate(UnsupportedNeighbour), "route-only 节点必须显示为可点（通行维度）");
            Assert.AreEqual(NodeUiState.Available, s.NodeState(UnsupportedNeighbour));
            Assert.IsNull(s.NodeBlockReason(UnsupportedNeighbour), "不可点原因只由通行维度产生");
            Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled,
                PassiveSupport.EvaluateTruth(UnsupportedNeighbour).Effect, "它的效果维度必须是 UNFULFILLED");

            var bagBefore = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagBefore);
            float life = s.PlayerStats.Get(StatId.Life);
            float str = s.PlayerStats.Get(StatId.Strength);
            float intel = s.PlayerStats.Get(StatId.Intelligence);
            int unspent = s.Unspent;
            int allocatedBefore = CountAllocated(s);

            string err;
            Assert.IsTrue(s.TryAllocate(UnsupportedNeighbour, out err), "route-only 节点必须可作为路径点亮：" + err);
            Assert.AreEqual(unspent - 1, s.Unspent, "route-only 加点必须正常扣 1 点");
            Assert.AreEqual(allocatedBefore + 1, CountAllocated(s), "route-only 加点必须进入已分配集合");
            Assert.IsTrue(s.Allocated[UnsupportedNeighbour], "route-only 加点必须写入 selected 集");

            // 但它的一切 gameplay 效果必须是 0（整节点，含它那几条"看起来可识别"的行）
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f, "route-only 不得改变玩家结果");
            Assert.AreEqual(str, s.PlayerStats.Get(StatId.Strength), 0.0001f);
            Assert.AreEqual(intel, s.PlayerStats.Get(StatId.Intelligence), 0.0001f,
                "该节点含可识别的 +Intelligence 行，也必须被整节点挡住（不得半消费）");

            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            for (int i = 0; i < (int)StatId.Count; i++)
                Assert.AreEqual(bagBefore.Get((StatId)i), bagAfter.Get((StatId)i), 0.0001f, "route-only 不得改变技能结果：" + (StatId)i);
        }

        [UnityTest]
        public IEnumerator JewelSocket_Reject_LeavesPointsStateAndResultsUnchanged()
        {
            var sim = NewSim();
            yield return null;
            var s = sim.Session;

            // 负向对照：非通行节点（珠宝孔）必须被拒绝，且零扣点、零状态变化。
            int neighbour = PoeTree.Get(JewelNode).links == null || PoeTree.Get(JewelNode).links.Length == 0
                ? -1 : PoeTree.Get(JewelNode).links[0];
            Assert.GreaterOrEqual(neighbour, 0, "珠宝孔样例必须至少有一条连线");
            s.Allocated[neighbour] = true;     // 相连性成立（不走生产分配 API）

            var bagBefore = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagBefore);
            float life = s.PlayerStats.Get(StatId.Life);
            int unspent = s.Unspent;
            int allocatedBefore = CountAllocated(s);

            string err;
            Assert.IsFalse(s.TryAllocate(JewelNode, out err), "珠宝孔必须被拒绝");
            Assert.AreEqual(PassiveSupport.ReasonBlockedSpecial, err);
            Assert.AreEqual(unspent, s.Unspent, "点数零变化");
            Assert.AreEqual(allocatedBefore, CountAllocated(s), "已分配集合零变化");
            Assert.IsFalse(s.Allocated[JewelNode]);
            Assert.IsFalse(s.CanAllocate(JewelNode), "UI 不得把珠宝孔显示为可点");
            Assert.AreEqual(NodeUiState.Locked, s.NodeState(JewelNode));
            Assert.IsNotNull(s.NodeBlockReason(JewelNode), "UI 必须能取到稳定原因");
            Assert.AreEqual(life, s.PlayerStats.Get(StatId.Life), 0.0001f, "玩家结果零变化");

            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            for (int i = 0; i < (int)StatId.Count; i++)
                Assert.AreEqual(bagBefore.Get((StatId)i), bagAfter.Get((StatId)i), 0.0001f, "技能结果零变化：" + (StatId)i);
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
