using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-04A 门禁：被动支持真相（唯一 owner）与"静默半效果"消除。
    ///
    /// 冻结样例（全部取自权威数据，禁止 find-first 动态挑选）：
    ///   起点邻居：71 blocked / 559,1795,2034 supported（WO-02 canonical fixture）
    ///   真实混合节点：71（+5 Intelligence 可消费 + Mana Regeneration blocked）
    ///                 12（10% increased PhysicalDamage 可消费 + Minions blocked）—— 技能侧泄漏对照
    ///                 255（8% increased AttackSpeed 可消费 + Block/Melee Strike Range blocked + structural）
    ///   珠宝孔：78（kind=Jewel，无词条但有连线）  专精：10  时光珠宝类：locked != 0
    /// </summary>
    public sealed class PassiveSupportTests
    {
        // ---------------- 冻结样例 ----------------
        const int MixedNodeId = 71;
        const int OffensiveMixedNodeId = 12;
        const int StructuralMixedNodeId = 255;
        const int JewelNodeId = 78;
        const int MasteryNodeId = 10;
        static readonly int[] SupportedStartNeighbours = { 559, 1795, 2034 };

        // 04A 产品真相快照（与 census 报告同源；变红=可分配性被改动）
        const int ExpectedSupported = 367;
        const int ExpectedBlocked = 1660;
        const int ExpectedSpecial = 87;
        const int ExpectedMasteryPending = 315;
        // 排除专精/珠宝孔/时光珠宝类（它们由更高优先级的特殊状态接管）
        const int ExpectedRealMixedNodes = 211;
        const int ExpectedConsumedLines = 607;

        // ---------------- 1. 全树确定性 ----------------

        [Test]
        public void EveryOnTreeNodeHasDeterministicSupportTruth()
        {
            PoeNode[] nodes = PoeTree.Nodes;
            Assert.Greater(nodes.Length, 0, "天赋树数据必须可加载");

            int supported = 0, blocked = 0, special = 0, mastery = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                PassiveSupport.NodeStatus a = PassiveSupport.EvaluateNode(i);
                Assert.AreEqual(a, PassiveSupport.EvaluateNode(i), "同一节点两次判定必须一致：" + i);
                Assert.IsTrue(Enum.IsDefined(typeof(PassiveSupport.NodeStatus), a),
                    "不得存在 UNKNOWN 可分配性状态：" + i);
                switch (a)
                {
                    case PassiveSupport.NodeStatus.AllocatableSupported: supported++; break;
                    case PassiveSupport.NodeStatus.BlockedCurrently: blocked++; break;
                    case PassiveSupport.NodeStatus.BlockedSpecialInteraction: special++; break;
                    case PassiveSupport.NodeStatus.SpecialPendingMastery: mastery++; break;
                }
            }
            Assert.AreEqual(nodes.Length, supported + blocked + special + mastery, "资格必须划分全部上树节点");
            Assert.AreEqual(ExpectedSupported, supported, "ALLOCATABLE_SUPPORTED");
            Assert.AreEqual(ExpectedBlocked, blocked, "BLOCKED_CURRENTLY");
            Assert.AreEqual(ExpectedSpecial, special, "BLOCKED_SPECIAL_INTERACTION");
            Assert.AreEqual(ExpectedMasteryPending, mastery, "SPECIAL_PENDING_MASTERY");

            Assert.AreEqual(PassiveSupport.NodeStatus.OutOfDomain, PassiveSupport.EvaluateNode(-1));
            Assert.AreEqual(PassiveSupport.NodeStatus.OutOfDomain, PassiveSupport.EvaluateNode(nodes.Length),
                "403 未上树节点不在 PoeTree.Nodes 内 ⇒ 索引域外，只做源数据记账");
        }

        [Test]
        public void EveryEffectLineHasKnownBucket()
        {
            PoeNode[] nodes = PoeTree.Nodes;
            int unknown = 0, consumed = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                unknown += CountUnknown(nodes[i].stats);
                unknown += CountUnknown(nodes[i].choices);
                consumed += CountBucket(nodes[i].stats, PassiveSupport.LineBucket.Consumed);
                consumed += CountBucket(nodes[i].choices, PassiveSupport.LineBucket.Consumed);
            }
            Assert.AreEqual(0, unknown, "UNKNOWN 行分类必须为 0（门禁）");
            Assert.AreEqual(ExpectedConsumedLines, consumed, "CONSUMED 行数（04A 严格化后不变）");
        }

        // ---------------- 2. STRUCTURAL 单独不阻止分配 ----------------

        [Test]
        public void StructuralLineDoesNotBlockNodeByItself()
        {
            string domain;
            Assert.AreEqual(PassiveSupport.LineBucket.Structural,
                PassiveSupport.ClassifyLine("(Ailments that deal Damage are Bleeding, Ignited, and Poisoned)", out domain));
            Assert.AreEqual(PassiveSupport.LineBucket.Structural, PassiveSupport.ClassifyLine("(some note)", out domain));
            // 钉死"含括号 ≠ STRUCTURAL"：行首不是 '(' 的注释尾巴必须仍按效果行判定
            Assert.AreNotEqual(PassiveSupport.LineBucket.Structural,
                PassiveSupport.ClassifyLine("+20% increased Damage (some note)", out domain),
                "判定依据是行首格式约定，不是'含括号'");

            // 合成语义样例（真实数据里没有"只有括号行"的节点：structuralOnlyNodes=0）
            PoeNode n = default;
            n.kind = (int)PoeNodeKind.Normal;
            n.stats = "(only a note)";
            Assert.AreEqual(PassiveSupport.NodeStatus.AllocatableSupported, PassiveSupport.EvaluateNode(n),
                "STRUCTURAL 行不构成 gameplay promise ⇒ 不阻止分配");
            n.stats = "(note one)\n(note two)";
            Assert.AreEqual(PassiveSupport.NodeStatus.AllocatableSupported, PassiveSupport.EvaluateNode(n));

            // 同一判据下：可消费行 + blocked 行必须整体 fail closed
            n.stats = "+10 to Strength\n12% increased Movement Speed";
            Assert.AreEqual(PassiveSupport.NodeStatus.BlockedCurrently, PassiveSupport.EvaluateNode(n),
                "混合节点必须整体不可分配");
        }

        // ---------------- 3. CONSUMED = parser + 真实 runtime consumer ----------------

        [Test]
        public void ConsumedMeansParserPlusRealRuntimeConsumer()
        {
            // 单一 owner：清单在 runtime，测试只消费（禁止生产规则由测试程序集定义）
            Assert.IsTrue(ReferenceEquals(ContentAuditS2Tests.RuntimeConsumedStats, PassiveSupport.RuntimeConsumedStats),
                "runtime consumer 清单必须只有一个 owner");

            foreach (StatId s in Enum.GetValues(typeof(StatId)))
            {
                if (s == StatId.Count)
                    continue;
                Assert.IsTrue(PassiveSupport.IsRuntimeConsumed(s),
                    "每个 StatId 都必须有真实 runtime consumer 站点（否则 parser 承认也不算可兑现）：" + s);
            }
            Assert.IsFalse(PassiveSupport.IsRuntimeConsumed(StatId.Count), "Count 不是 StatId");

            // 树上每条 CONSUMED 行都必须同时满足"parser 命中"与"产出 StatId 全部有真实消费者"
            PoeNode[] nodes = PoeTree.Nodes;
            int lines = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                lines += CheckConsumed(nodes[i].stats);
                lines += CheckConsumed(nodes[i].choices);
            }
            Assert.AreEqual(ExpectedConsumedLines, lines);
        }

        static int CheckConsumed(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            int n = 0;
            string[] parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                string domain;
                if (PassiveSupport.ClassifyLine(parts[i], out domain) != PassiveSupport.LineBucket.Consumed)
                    continue;
                Modifier[] mods = PoeStatParser.Parse(parts[i]);
                Assert.Greater(mods.Length, 0, "CONSUMED 行必须被权威 parser 命中：" + parts[i]);
                for (int m = 0; m < mods.Length; m++)
                    Assert.IsTrue(PassiveSupport.IsRuntimeConsumed(mods[m].Stat),
                        "CONSUMED 行产出的 StatId 必须有真实消费者：" + parts[i] + " -> " + mods[m].Stat);
                n++;
            }
            return n;
        }

        // ---------------- 4. 混合节点整体 fail closed ----------------

        [Test]
        public void MixedNodeIsBlockedAsWhole()
        {
            int realMixed = 0;
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                // 专精 / 珠宝孔 / 时光珠宝类由更高优先级的特殊状态接管（§12/§17），不参与"混合 ⇒ blocked"这条判据
                if (nodes[i].Kind == PoeNodeKind.Mastery || nodes[i].Kind == PoeNodeKind.Jewel || nodes[i].locked != 0)
                    continue;
                int c, b, s, st;
                CountLines(i, out c, out b, out s, out st);
                if (c > 0 && b > 0)
                {
                    realMixed++;
                    Assert.AreEqual(PassiveSupport.NodeStatus.BlockedCurrently, PassiveSupport.EvaluateNode(i),
                        "任何含 blocked 行的节点都必须整体不可分配：" + i);
                }
            }
            Assert.AreEqual(ExpectedRealMixedNodes, realMixed,
                "权威数据里存在真实混合节点（不是合成样例）");

            // 冻结样例的客观形状
            int mc, mb, ms, mst;
            CountLines(MixedNodeId, out mc, out mb, out ms, out mst);
            Assert.AreEqual(1, mc, "冻结混合节点必须含 1 条可消费行：" + PoeTree.Get(MixedNodeId).stats);
            Assert.AreEqual(1, mb, "冻结混合节点必须含 1 条 blocked 行");
            CountLines(StructuralMixedNodeId, out mc, out mb, out ms, out mst);
            Assert.AreEqual(1, mst, "255 号样例额外含 structural 行（证明 structural 不救场）");
            Assert.AreEqual(PassiveSupport.NodeStatus.BlockedCurrently, PassiveSupport.EvaluateNode(StructuralMixedNodeId));
        }

        // ---------------- 5. 分配门：原子拒绝 + 零增量 ----------------

        [Test]
        public void RouteOnlyNodeTryAllocateSucceeds_AndSpendsExactlyOnePoint()
        {
            var s = NewSession();
            int unspent = s.Unspent;
            string err;
            Assert.IsTrue(s.CanAllocate(SupportedStartNeighbours[0]), "前置：起点邻居里的 supported 节点本来可点");
            // S6P-WO-04A2：71 与起点相连、且是普通节点 ⇒ 通行维度合法 ⇒ 分配必须成功（唯一变化是扣 1 点）。
            // 04A 的"含 blocked 行即原子拒绝"已被本令拆开：效果未兑现 ≠ 不能作为路径。
            Assert.IsTrue(s.TryAllocate(MixedNodeId, out err), "含 blocked 行的相连节点必须可作为路径点亮：" + err);
            Assert.AreEqual(unspent - 1, s.Unspent, "route-only 节点必须花掉恰好 1 点");
            Assert.IsTrue(s.Allocated[MixedNodeId], "route-only 节点必须进入 selected 集");
            Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled,
                PassiveSupport.EvaluateTruth(MixedNodeId).Effect, "它的效果维度必须是 UNFULFILLED");
            Assert.AreEqual(NodeUiState.Allocated, s.NodeState(MixedNodeId), "UI 状态必须与 domain truth 一致");
        }

        [Test]
        public void UnsupportedNodeChangesNoPlayerEffectiveResult()
        {
            var s = NewSession();
            var baseline = NewSession();
            string err;
            s.TryAllocate(MixedNodeId, out err);
            s.RecalcPlayer(false);
            AssertPlayerResultsEqual(baseline, s, "route-only 加点（分配合法）不得改变玩家有效结果");
        }

        [Test]
        public void UnsupportedNodeChangesNoSkillEffectiveResult()
        {
            var s = NewSession();
            var baseline = NewSession();
            string err;
            s.TryAllocate(MixedNodeId, out err);
            AssertSkillResultsEqual(baseline, s, "route-only 加点（分配合法）不得改变技能有效结果");
        }

        // ---------------- 6. 消费门：损坏/注入状态也不得半消费 ----------------

        [Test]
        public void InjectedBlockedNodeContributesZeroModifiers()
        {
            var s = NewSession();
            var control = NewSession();
            Assert.AreEqual(0, s.BlockedAllocatedCount, "正常会话不得有不可通行节点被点亮");

            float lifeBefore = s.PlayerStats.Get(StatId.Life);
            float manaBefore = s.PlayerStats.Get(StatId.Mana);
            var bagBefore = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagBefore);
            float physBefore = bagBefore.RawIncreased(StatId.PhysicalDamage);
            float accBefore = bagBefore.Get(StatId.Accuracy);

            // 绕过分配门构造损坏状态（生产 API 不提供"强制分配"）
            s.Allocated[OffensiveMixedNodeId] = true;   // 12：可消费子行 = 10% increased PhysicalDamage
            s.Allocated[MixedNodeId] = true;            // 71：可消费子行 = +5 Intelligence
            s.RecalcPlayer(false);

            // S6P-WO-04A2：route-only 节点本来就是**合法**分配，不再算损坏状态 ⇒ 该计数恒为 0。
            // 真正要钉的是"它们贡献 0 modifier"（下面几条）。
            Assert.AreEqual(0, s.BlockedAllocatedCount, "route-only 分配是合法状态，不算损坏");
            Assert.AreEqual(lifeBefore, s.PlayerStats.Get(StatId.Life), 0.0001f, "blocked 节点不得贡献防御 modifier");
            Assert.AreEqual(manaBefore, s.PlayerStats.Get(StatId.Mana), 0.0001f, "+5 Intelligence 子行也不得泄漏");

            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            Assert.AreEqual(physBefore, bagAfter.RawIncreased(StatId.PhysicalDamage), 0.0001f,
                "blocked 节点的可消费子行不得泄漏进技能包");
            Assert.AreEqual(accBefore, bagAfter.Get(StatId.Accuracy));

            // 真正损坏：把**不可通行**节点（专精）塞进 selected 集 ⇒ invalid-state evidence 必须暴露
            s.Allocated[MasteryNodeId] = true;
            Assert.AreEqual(1, s.BlockedAllocatedCount, "不可通行节点被点亮必须由 invalid-state evidence 暴露");
            s.Allocated[MasteryNodeId] = false;

            // 对照：同一注入手法放一个 supported 节点，效果必须真的出现（证明"零"不是测量假象）
            control.Allocated[559] = true;
            control.RecalcPlayer(false);
            var bagControl = new StatBag();
            control.CollectSkillMods(SkillId.Melee, bagControl);
            Assert.Greater(bagControl.RawIncreased(StatId.AttackSpeed), 0f, "对照：supported 节点注入后必须生效");
        }

        // ---------------- 7. 正对照：supported 节点照常生效 ----------------

        [Test]
        public void SupportedNodeStillAllocatesNormally()
        {
            var s = NewSession();
            string err;
            Assert.IsTrue(s.CanAllocate(SupportedStartNeighbours[0]));
            for (int i = 0; i < SupportedStartNeighbours.Length; i++)
            {
                int id = SupportedStartNeighbours[i];
                Assert.AreEqual(PassiveSupport.NodeStatus.AllocatableSupported, PassiveSupport.EvaluateNode(id));
                int before = s.Unspent;
                Assert.IsTrue(s.TryAllocate(id, out err), "supported 节点必须可点亮：" + id + " " + err);
                Assert.AreEqual(before - 1, s.Unspent, "正常加点必须恰好消耗 1 点");
                Assert.IsTrue(s.Allocated[id]);
                Assert.AreEqual(NodeUiState.Allocated, s.NodeState(id));
            }
        }

        [Test]
        public void SupportedOffensiveNodeChangesActualSkillContext()
        {
            // 559 = "4% increased Attack Speed / +5 to Dexterity"：进攻 modifier 只进技能包与技能形态
            var baseline = NewSession();
            var s = NewSession();
            string err;
            Assert.IsTrue(s.TryAllocate(559, out err), err);

            var b0 = new StatBag();
            var b1 = new StatBag();
            baseline.CollectSkillMods(SkillId.Melee, b0);
            s.CollectSkillMods(SkillId.Melee, b1);
            Assert.Greater(b1.RawIncreased(StatId.AttackSpeed), b0.RawIncreased(StatId.AttackSpeed),
                "supported 进攻节点必须真的改变技能上下文");
            Assert.Less(s.ResolveSkillDef(SkillId.Melee).Recovery, baseline.ResolveSkillDef(SkillId.Melee).Recovery,
                "技能形态消费者（ResolveSkillDef）必须真的看到该变化");
        }

        [Test]
        public void SupportedDefensiveOrAttributeNodeChangesActualPlayerResult()
        {
            // 2034 = "+12 to maximum Life / +5 to Strength"
            var baseline = NewSession();
            var s = NewSession();
            string err;
            Assert.IsTrue(s.TryAllocate(2034, out err), err);

            Assert.Greater(s.PlayerStats.Get(StatId.Life), baseline.PlayerStats.Get(StatId.Life),
                "supported 防御/属性节点必须真的改变玩家有效结果");
            Assert.Greater(s.PlayerStats.Get(StatId.Strength), baseline.PlayerStats.Get(StatId.Strength));
            Assert.Greater(s.MaxLife, baseline.MaxLife);
        }

        // ---------------- 8. 专精过渡态 ----------------

        [Test]
        public void MasteryTemporarilyCannotAllocate()
        {
            var s = NewSession();
            int neighbour = PoeTree.Get(MasteryNodeId).links[0];
            s.Allocated[neighbour] = true;          // 让相连性成立：被拒的唯一原因只能是过渡态
            Assert.IsFalse(s.CanAllocate(MasteryNodeId));
            int unspent = s.Unspent;
            string err;
            Assert.IsFalse(s.TryAllocate(MasteryNodeId, out err), "专精不得走普通 TryAllocate（须显式选择）");
            Assert.AreEqual(PassiveSupport.ReasonMasteryPending, err, "必须有稳定原因（UI 与门共用一个真值）");
            Assert.AreEqual(unspent, s.Unspent, "MasteryTemporaryRejectCostsZero");
            Assert.IsFalse(s.Allocated[MasteryNodeId]);
            Assert.AreEqual(PassiveSupport.NodeStatus.SpecialPendingMastery, PassiveSupport.EvaluateNode(MasteryNodeId));

            // 全树 315 个专精一律如此
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].Kind == PoeNodeKind.Mastery)
                    Assert.AreEqual(PassiveSupport.NodeStatus.SpecialPendingMastery, PassiveSupport.EvaluateNode(i),
                        "专精必须一律为过渡态：" + i);
        }

        [Test]
        public void MasteryImplicitFirstChoiceNoLongerApplies()
        {
            PoeNode[] nodes = PoeTree.Nodes;
            int masteries = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Kind != PoeNodeKind.Mastery)
                    continue;
                masteries++;
                Assert.AreEqual(0, PassiveCatalog.Get(i).Mods.Length,
                    "专精不得再烘焙任何隐式 modifier（旧 FirstChoice 路径已拆）：" + i);
            }
            Assert.AreEqual(ExpectedMasteryPending, masteries);

            // 该专精确有"可被 parser 兑现"的可选效果（否则本对照无效）——它绝不能因为损坏状态而生效
            Modifier[] choices = PoeStatParser.ParseCached(PoeTree.Get(MasteryNodeId).choices);
            Assert.Greater(choices.Length, 0, "node 10 的 choices 里存在可映射效果（+30 to maximum Life）");

            var s = NewSession();
            var baseline = NewSession();
            float lifeBefore = s.PlayerStats.Get(StatId.Life);
            var bagBefore = new StatBag();
            baseline.CollectSkillMods(SkillId.Melee, bagBefore);
            s.Allocated[MasteryNodeId] = true;      // 损坏/注入状态
            s.RecalcPlayer(false);
            Assert.AreEqual(lifeBefore, s.PlayerStats.Get(StatId.Life), 0.0001f,
                "损坏状态下专精也不得产生任何效果");
            Assert.AreEqual(1, s.BlockedAllocatedCount);

            var bagAfter = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bagAfter);
            for (int i = 0; i < (int)StatId.Count; i++)
            {
                Assert.AreEqual(bagBefore.Get((StatId)i), bagAfter.Get((StatId)i), 0.0001f,
                    "专精不得进入技能包：" + (StatId)i);
                Assert.AreEqual(bagBefore.RawIncreased((StatId)i), bagAfter.RawIncreased((StatId)i), 0.0001f,
                    "专精不得进入技能包（increased）：" + (StatId)i);
            }
        }

        // ---------------- 9. 无 handler 的特殊交互 ----------------

        [Test]
        public void SpecialWithoutHandlerIsBlocked()
        {
            Assert.AreEqual(PoeNodeKind.Jewel, PoeTree.Get(JewelNodeId).Kind, "样例必须是珠宝孔");
            Assert.AreEqual(PassiveSupport.NodeStatus.BlockedSpecialInteraction, PassiveSupport.EvaluateNode(JewelNodeId),
                "珠宝孔没有 runtime handler ⇒ 不可分配（不能因为画出来了就算 supported）");
            Assert.AreEqual(PassiveSupport.ReasonBlockedSpecial, PassiveSupport.Reason(PassiveSupport.EvaluateNode(JewelNodeId)));

            var s = NewSession();
            s.Allocated[PoeTree.Get(JewelNodeId).links[0]] = true;   // 相连性成立
            string err;
            int unspent = s.Unspent;
            Assert.IsFalse(s.TryAllocate(JewelNodeId, out err), "珠宝孔必须被拒绝");
            Assert.AreEqual(PassiveSupport.ReasonBlockedSpecial, err);
            Assert.AreEqual(unspent, s.Unspent);
            Assert.IsFalse(s.Allocated[JewelNodeId]);

            // 时光珠宝类（官方数据无连线）恒不可点
            PoeNode[] nodes = PoeTree.Nodes;
            int locked = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].locked == 0)
                    continue;
                locked++;
                Assert.AreEqual(PassiveSupport.NodeStatus.BlockedSpecialInteraction, PassiveSupport.EvaluateNode(i),
                    "时光珠宝类节点必须为 blocked special：" + i);
                Assert.IsFalse(s.CanAllocate(i));
            }
            Assert.Greater(locked, 0, "官方数据存在时光珠宝类节点");
        }

        // ---------------- 10. UI 读同一 truth ----------------

        [Test]
        public void UIUsesDomainSupportTruth()
        {
            var s = NewSession();
            int traversable = 0, blocked = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(i);
                string reason = s.NodeBlockReason(i);
                if (PassiveSupport.IsTraversable(t.Traversal))
                {
                    traversable++;
                    // S6P-WO-04A2：通行与生效分离 ⇒ "为什么不能点"只由通行维度产生。
                    // 效果未兑现（route-only）**不是**不可点理由。
                    Assert.IsNull(reason, "可通行节点不得有禁用原因：" + i);
                }
                else
                {
                    blocked++;
                    Assert.IsNotNull(reason, "不可通行节点必须有稳定原因：" + i);
                }

                if (s.CanAllocate(i))
                    Assert.IsTrue(PassiveSupport.IsTraversable(t.Traversal), "UI 可点必须蕴含 domain truth 可通行：" + i);
                if (s.NodeState(i) == NodeUiState.Available)
                {
                    bool masterySel = PoeTree.Get(i).Kind == PoeNodeKind.Mastery && s.CanEnterMasterySelection(i);
                    Assert.IsTrue(masterySel || PassiveSupport.IsTraversable(t.Traversal),
                        "UI Available 必须是可通行普通节点或可进入选择器的专精：" + i);
                }
                if (!PassiveSupport.IsTraversable(t.Traversal) && PoeTree.Get(i).Kind != PoeNodeKind.Mastery)
                    Assert.AreNotEqual(NodeUiState.Available, s.NodeState(i), "不可通行非专精节点不得显示为可点：" + i);
            }
            Assert.AreEqual(ExpectedSupported + ExpectedBlocked, traversable,
                "可通行 = 全部普通上树节点（367 可兑现 + 1660 route-only）");
            Assert.AreEqual(ExpectedSpecial + ExpectedMasteryPending, blocked,
                "不可通行 = 无 handler 特殊交互 87 + 专精过渡 315");
        }

        // ---------------- 11. 不得存在第二套 oracle ----------------

        [Test]
        public void NoSecondUnsupportedNodeOracle()
        {
            Assert.IsTrue(ReferenceEquals(ContentAuditS2Tests.RuntimeConsumedStats, PassiveSupport.RuntimeConsumedStats),
                "consumer 清单唯一 owner = runtime");

            string support = ReadSource("Assets/Runtime/Core/Gameplay/PassiveSupport.cs");
            StringAssert.Contains("DomainTable", support);

            // runtime 里只有 PassiveSupport 允许拥有分类规则与原因文本
            string[] runtime = Directory.GetFiles(DataRoot("Assets/Runtime"), "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < runtime.Length; i++)
            {
                if (runtime[i].EndsWith("PassiveSupport.cs"))
                    continue;
                string text = File.ReadAllText(runtime[i]);
                Assert.IsFalse(text.Contains("DomainTable"),
                    "runtime 出现了第二套缺失轴表：" + runtime[i]);
                Assert.IsFalse(text.Contains("该节点含当前引擎无法完整兑现的效果"),
                    "runtime 出现了第二套原因文本：" + runtime[i]);
            }

            // 测试程序集不得再复制缺失轴关键词表
            string[] tests = Directory.GetFiles(DataRoot("Assets/Tests"), "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < tests.Length; i++)
            {
                string text = File.ReadAllText(tests[i]);
                Assert.IsFalse(text.Contains("\"energy shield\""),
                    "测试程序集出现了第二套缺失轴表：" + tests[i]);
            }

            // UI 不得自己跑 parser / 自查节点文本
            string hud = ReadSource("Assets/Runtime/Core/Gameplay/SliceHud.cs");
            StringAssert.Contains("NodeBlockReason(", hud, "UI 必须经 domain truth 取原因");
            Assert.IsFalse(hud.Contains("PoeStatParser"), "UI 不得自己跑 parser");
        }

        // ---------------- 工具 ----------------

        static SliceSession NewSession()
        {
            var s = new SliceSession();
            s.ResetTown(12345u);
            return s;
        }

        static void AssertPlayerResultsEqual(SliceSession a, SliceSession b, string message)
        {
            for (int i = 0; i < (int)StatId.Count; i++)
                Assert.AreEqual(a.PlayerStats.Get((StatId)i), b.PlayerStats.Get((StatId)i), 0.0001f,
                    message + "：" + (StatId)i);
            Assert.AreEqual(a.MaxLife, b.MaxLife, 0.0001f, message + "：MaxLife");
            Assert.AreEqual(a.MaxMana, b.MaxMana, 0.0001f, message + "：MaxMana");
        }

        static void AssertSkillResultsEqual(SliceSession a, SliceSession b, string message)
        {
            var ba = new StatBag();
            var bb = new StatBag();
            for (int s = 0; s <= (int)SkillId.Area; s++)
            {
                SkillId skill = (SkillId)s;
                a.CollectSkillMods(skill, ba);
                b.CollectSkillMods(skill, bb);
                for (int i = 0; i < (int)StatId.Count; i++)
                {
                    Assert.AreEqual(ba.Get((StatId)i), bb.Get((StatId)i), 0.0001f,
                        message + "：" + skill + "/" + (StatId)i);
                    Assert.AreEqual(ba.RawIncreased((StatId)i), bb.RawIncreased((StatId)i), 0.0001f,
                        message + "：raw inc " + skill + "/" + (StatId)i);
                    Assert.AreEqual(ba.RawMore((StatId)i), bb.RawMore((StatId)i), 0.0001f,
                        message + "：raw more " + skill + "/" + (StatId)i);
                }
                Assert.AreEqual(a.ResolveSkillDef(skill).Recovery, b.ResolveSkillDef(skill).Recovery, 0.0001f,
                    message + "：Recovery " + skill);
            }
        }

        static void CountLines(int id, out int consumed, out int blocked, out int special, out int structural)
        {
            PoeNode n = PoeTree.Get(id);
            consumed = blocked = special = structural = 0;
            CountText(n.stats, ref consumed, ref blocked, ref special, ref structural);
            CountText(n.choices, ref consumed, ref blocked, ref special, ref structural);
        }

        static void CountText(string text, ref int consumed, ref int blocked, ref int special, ref int structural)
        {
            if (string.IsNullOrEmpty(text))
                return;
            string[] parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().Length == 0)
                    continue;
                string domain;
                switch (PassiveSupport.ClassifyLine(parts[i], out domain))
                {
                    case PassiveSupport.LineBucket.Consumed: consumed++; break;
                    case PassiveSupport.LineBucket.BlockedByDomain: blocked++; break;
                    case PassiveSupport.LineBucket.SpecialInteraction: special++; break;
                    case PassiveSupport.LineBucket.Structural: structural++; break;
                }
            }
        }

        static int CountBucket(string text, PassiveSupport.LineBucket want)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            int n = 0;
            string[] parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().Length == 0)
                    continue;
                string domain;
                if (PassiveSupport.ClassifyLine(parts[i], out domain) == want)
                    n++;
            }
            return n;
        }

        static int CountUnknown(string text)
        {
            return CountBucket(text, PassiveSupport.LineBucket.Unknown);
        }

        static string DataRoot(string relative)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
        }

        static string ReadSource(string relative)
        {
            string path = DataRoot(relative);
            Assert.IsTrue(File.Exists(path), "源码缺失：" + path);
            return File.ReadAllText(path);
        }
    }
}
