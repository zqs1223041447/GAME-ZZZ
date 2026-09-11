using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-04A2 门禁：被动**通行资格**与**效果兑现资格**分离。
    ///
    /// 本令修的是一个架构错误：04A 把「能不能作为树路径」和「承诺的效果能不能兑现」
    /// 压成了一个布尔值（<c>AllocatableSupported</c>），导致全树可达节点只有 14/2429。
    /// 本令后两者是两个独立维度，普通节点即便效果未兑现也能作为路径（route-only），
    /// 但**整节点贡献 0 modifier**；专精 / 珠宝孔 / 时光珠宝类仍然 fail-closed。
    ///
    /// AC 对应见表现在每个测试的注释里（AC-x）。
    /// </summary>
    public sealed class S6PWo04A2Tests
    {
        // ---- 04A 冻结 census（本令不得改动任何一个数字） ----
        const int ExpectedFullySupported = 367;
        const int ExpectedUnfulfilled = 1660;
        const int ExpectedSpecialBlocked = 402;      // 87 无 handler 特殊 + 315 专精过渡
        const int ExpectedLegacyReachable = 14;      // 旧行为参考可达数

        // ---- §8.3 确定性发现后冻结的恢复 fixture（数据未变则逐字不变） ----
        // 2026-09-11 发现：从 2172 出发，最短(距离 2)的「legacy 到不了、新规则到得了、
        // 且路径上真的经过 route-only 节点」的真实 supported 节点 = 183，
        // canonical 路径 [2172, 71, 183]，其中 71 是唯一 route-only 节点（必须先点它才能到 183）。
        const int FrozenSupportedDenominator = 325;
        const int FrozenCandidateCount = 311;
        const int FrozenTarget = 183;
        const int FrozenDistance = 2;
        const int FrozenFirstRouteOnly = 71;
        static readonly int[] FrozenPath = { 2172, 71, 183 };
        static readonly int[] FrozenRouteOnlyOnPath = { 71 };

        static readonly int[] FrozenMixedNodes = { 71, 12, 255 };
        const int MasteryNode = 10;
        const int JewelNode = 78;

        // ==================== 1. 两个独立维度 / 单一 owner ====================

        /// <summary>AC-2 / AC-3 / AC-4：两个维度存在、同属一个 owner、且不再有共用布尔 authority。</summary>
        [Test]
        public void TruthOwner_ExposesTwoIndependentDimensions_AndNoSharedBooleanAuthority()
        {
            Type t = typeof(PassiveSupport);
            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var names = new List<string>(methods.Length);
            for (int i = 0; i < methods.Length; i++)
                names.Add(methods[i].Name);

            Assert.IsTrue(names.Contains("EvaluateTruth"), "canonical truth API 必须存在");
            Assert.IsTrue(names.Contains("IsTraversable"), "通行维度判据必须独立暴露");
            Assert.IsTrue(names.Contains("YieldsModifiers"), "效果维度判据必须独立暴露");
            Assert.IsTrue(names.Contains("ReachableSet"), "可达性真值必须由同一 owner 产出");
            Assert.IsFalse(names.Contains("IsAllocatable"),
                "04A 的 IsAllocatable 必须拆除：不得存在同时决定通行与生效的布尔 authority");
        }

        /// <summary>AC-4：分配门与消费门不得再读四值诊断投影（源码级取证，防"注释说改了、代码没改"）。</summary>
        [Test]
        public void GatesDoNotReadTheDiagnosticProjection()
        {
            string path = Path.Combine(Application.dataPath, "Runtime/Core/Gameplay/SliceSession.cs");
            Assert.IsTrue(File.Exists(path), "运行时源码必须可读：" + path);
            string src = File.ReadAllText(path);

            Assert.IsFalse(src.Contains("EvaluateNode("),
                "SliceSession 不得再消费四值诊断投影（它只服务 census/证据）");
            Assert.IsFalse(src.Contains("IsAllocatable"),
                "SliceSession 不得再存在共用布尔 authority");
            Assert.IsTrue(src.Contains("YieldsModifiers"), "消费门必须读效果维度");
            Assert.IsTrue(src.Contains("IsTraversable"), "分配门必须读通行维度");
        }

        /// <summary>AC-5 / AC-6 / AC-7 / AC-16：全树真值矩阵与 04A 冻结数字。</summary>
        [Test]
        public void TruthMatrix_MatchesContract_And04ACensusPreserved()
        {
            int[][] cross = new int[3][];
            for (int i = 0; i < 3; i++)
                cross[i] = new int[3];

            for (int i = 0; i < PoeTree.Count; i++)
            {
                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(i);
                Assert.IsTrue(Enum.IsDefined(typeof(PassiveSupport.EffectTruth), t.Effect), "未定义的效果真值：" + i);
                Assert.IsTrue(Enum.IsDefined(typeof(PassiveSupport.TraversalTruth), t.Traversal), "未定义的通行真值：" + i);

                if (PassiveSupport.IsTraversable(t.Traversal))
                {
                    // 普通节点不得被标成 OUT_OF_DOMAIN 来规避可达性门；也不得是 SPECIAL_PENDING。
                    Assert.AreNotEqual(PassiveSupport.EffectTruth.SpecialPending, t.Effect,
                        "可通行的普通节点不得是 SPECIAL_PENDING：" + i);
                }
                else
                {
                    Assert.AreEqual(PassiveSupport.EffectTruth.SpecialPending, t.Effect,
                        "不可通行节点（特殊交互 / 域外）的效果维度必须是 SPECIAL_PENDING：" + i);
                }
                cross[(int)t.Effect][(int)t.Traversal]++;

                // 四值诊断投影必须与两个维度逐节点一致（它是派生视图，不是第二套 oracle）
                switch (PassiveSupport.EvaluateNode(i))
                {
                    case PassiveSupport.NodeStatus.AllocatableSupported:
                        Assert.AreEqual(PassiveSupport.EffectTruth.FullySupported, t.Effect);
                        Assert.AreEqual(PassiveSupport.TraversalTruth.Traversable, t.Traversal);
                        break;
                    case PassiveSupport.NodeStatus.BlockedCurrently:
                        Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled, t.Effect);
                        Assert.AreEqual(PassiveSupport.TraversalTruth.Traversable, t.Traversal);
                        break;
                    case PassiveSupport.NodeStatus.BlockedSpecialInteraction:
                    case PassiveSupport.NodeStatus.SpecialPendingMastery:
                        Assert.AreEqual(PassiveSupport.EffectTruth.SpecialPending, t.Effect);
                        Assert.AreEqual(PassiveSupport.TraversalTruth.SpecialBlocked, t.Traversal);
                        break;
                    default:
                        Assert.Fail("上树节点不得是 OutOfDomain：" + i);
                        break;
                }
            }

            int E0 = (int)PassiveSupport.EffectTruth.FullySupported;
            int E1 = (int)PassiveSupport.EffectTruth.Unfulfilled;
            int E2 = (int)PassiveSupport.EffectTruth.SpecialPending;
            int T0 = (int)PassiveSupport.TraversalTruth.Traversable;
            int T1 = (int)PassiveSupport.TraversalTruth.SpecialBlocked;
            int T2 = (int)PassiveSupport.TraversalTruth.OutOfDomain;

            Assert.AreEqual(ExpectedFullySupported, cross[E0][T0], "FULLY_SUPPORTED × TRAVERSABLE");
            Assert.AreEqual(ExpectedUnfulfilled, cross[E1][T0], "UNFULFILLED × TRAVERSABLE（route-only）");
            Assert.AreEqual(ExpectedSpecialBlocked, cross[E2][T1], "SPECIAL_PENDING × SPECIAL_BLOCKED");
            Assert.AreEqual(0, cross[E0][T1] + cross[E1][T1], "可兑现/未兑现不得与 SPECIAL_BLOCKED 交叉");
            Assert.AreEqual(0, cross[E2][T0], "SPECIAL_PENDING 不得可通行");
            Assert.AreEqual(0, cross[0][T2] + cross[1][T2] + cross[2][T2], "上树节点不得是 OUT_OF_DOMAIN");
            Assert.AreEqual(PoeTree.Count,
                cross[E0][T0] + cross[E1][T0] + cross[E2][T1], "真值矩阵必须划分全部上树节点");
        }

        /// <summary>AC-9：mixed 节点整节点零贡献（身份变、gameplay 不变）。</summary>
        [Test]
        public void MixedNodes_AreRouteOnly_AndContributeNothing()
        {
            for (int k = 0; k < FrozenMixedNodes.Length; k++)
            {
                int id = FrozenMixedNodes[k];
                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(id);
                Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled, t.Effect, "mixed 节点效果维度：" + id);
                Assert.AreEqual(PassiveSupport.TraversalTruth.Traversable, t.Traversal, "mixed 节点通行维度：" + id);
                AssertContributesNothing(id);
            }
        }

        // ==================== 2. 可达性门 ====================

        /// <summary>AC-15：start-connected ordinary supported nodes 必须 100% 可达，缺失集必须为空。</summary>
        [Test]
        public void ReachabilityGate_SupportedDenominatorIsFullyReachable()
        {
            List<int> d = PassiveTraversal04A2.SupportedDenominator();
            bool[] runtime = PassiveSupport.ReachableSet();
            int[] missing = PassiveTraversal04A2.Missing(runtime, d);

            Assert.AreEqual(FrozenSupportedDenominator, d.Count, "supported denominator 冻结值");
            Assert.AreEqual(0, missing.Length,
                "start-connected ordinary supported nodes 必须 100% 可达，缺失：" +
                PassiveTraversal04A2.FormatPath(missing));
        }

        /// <summary>AC-15 的非平凡性凭据：旧行为参考可达 14，通行/生效分离后必须实质恢复。</summary>
        [Test]
        public void ReachabilityGate_IsNonTrivial_AndProductionMatchesReference()
        {
            bool[] legacy = PassiveTraversal04A2.LegacyReach();
            bool[] reference = PassiveTraversal04A2.ReferenceReach();
            bool[] runtime = PassiveSupport.ReachableSet();

            int l = 0, r = 0, p = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (legacy[i]) l++;
                if (reference[i]) r++;
                if (runtime[i]) p++;
                Assert.AreEqual(reference[i], runtime[i],
                    "生产可达集与参考遍历必须逐节点一致（互为 oracle）：" + i);
            }
            Assert.AreEqual(ExpectedLegacyReachable, l, "旧行为参考可达数必须是 14（gate 非平凡性凭据）");
            Assert.Greater(p, l, "可达规模必须实质提升，否则 gate 是空转");
            Assert.IsTrue(runtime[PassiveTraversal04A2.StartNodeId], "seed 本身必须可达");
        }

        /// <summary>AC-14：恢复 fixture 必须与冻结的 ID / 路径 / 长度逐字一致。</summary>
        [Test]
        public void RecoveryFixture_IsFrozen()
        {
            PassiveTraversal04A2.Fixture f = PassiveTraversal04A2.DiscoverFixture();
            Assert.AreEqual(FrozenCandidateCount, PassiveTraversal04A2.CandidateCount(), "候选集大小");
            Assert.GreaterOrEqual(PassiveTraversal04A2.CandidateCount(), 1, "候选集不得为空（§8.2）");

            Assert.AreEqual(FrozenTarget, f.Target, "冻结 target 不得漂移");
            Assert.AreEqual(FrozenDistance, f.Distance, "冻结距离不得漂移");
            Assert.AreEqual(PassiveTraversal04A2.FormatPath(FrozenPath), PassiveTraversal04A2.FormatPath(f.Path),
                "canonical 最短路径不得漂移");
            Assert.AreEqual(PassiveTraversal04A2.FormatPath(FrozenRouteOnlyOnPath),
                PassiveTraversal04A2.FormatPath(f.RouteOnlyOnPath), "路径上的 route-only 集合不得漂移");
            Assert.AreEqual(FrozenFirstRouteOnly, f.FirstRouteOnly, "第一个 route-only 节点不得漂移");

            // §8.4 必须同时成立的三条
            bool[] legacy = PassiveTraversal04A2.LegacyReach();
            bool[] runtime = PassiveSupport.ReachableSet();
            Assert.IsFalse(legacy[f.Target], "冻结 target 在旧行为下必须不可达");
            Assert.IsTrue(runtime[f.Target], "冻结 target 在新通行规则下必须可达");
            Assert.GreaterOrEqual(f.RouteOnlyOnPath.Length, 1, "冻结路径必须真的经过 route-only 节点");
            Assert.AreEqual(PassiveTraversal04A2.StartNodeId, f.Path[0], "路径必须从 seed 开始");
            Assert.AreEqual(f.Target, f.Path[f.Path.Length - 1], "路径必须终止于 target");
            Assert.AreEqual(PassiveTraversal04A2.StartNodeId, SliceSession.StartNode, "起点不得被本令改变");
        }

        /// <summary>AC-8：冻结路径全程经 domain API 可分配、正常扣点。</summary>
        [Test]
        public void FrozenPath_IsAllocatableThroughDomainApi()
        {
            PassiveTraversal04A2.Fixture f = PassiveTraversal04A2.DiscoverFixture();
            Assert.Greater(f.Path.Length, 1);
            Assert.LessOrEqual(f.Path.Length - 1, SliceRules.StartPoints, "路径长度必须在天赋点预算内");

            var s = new SliceSession();
            s.ResetTown(20260911u);
            string err;
            for (int i = 1; i < f.Path.Length; i++)
            {
                Assert.IsTrue(s.TryAllocate(f.Path[i], out err),
                    "冻结路径节点必须可分配：" + f.Path[i] + "（" + err + "）");
            }
            Assert.AreEqual(SliceRules.StartPoints - (f.Path.Length - 1), s.Unspent, "必须按普通节点正常扣点");
            Assert.AreEqual(f.Path.Length, CountAllocated(s), "起点 + 路径节点全部分配成功");
        }

        // ==================== 3. 负向对照 ====================

        /// <summary>AC-10 / AC-11 / AC-12：专精 / 珠宝孔 / 时光珠宝类拒绝且零扣点、零状态变化。</summary>
        [Test]
        public void SpecialNodes_Reject_WithZeroPointAndZeroStateMutation()
        {
            int[] specials = { MasteryNode, JewelNode, FirstLockedNode() };
            for (int k = 0; k < specials.Length; k++)
            {
                int id = specials[k];
                Assert.GreaterOrEqual(id, 0, "特殊节点样例必须存在");

                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(id);
                Assert.AreEqual(PassiveSupport.EffectTruth.SpecialPending, t.Effect, "效果维度：" + id);
                Assert.AreEqual(PassiveSupport.TraversalTruth.SpecialBlocked, t.Traversal, "通行维度：" + id);

                var s = new SliceSession();
                s.ResetTown(3u);
                // 让相连性成立，确保测的是通行门而不是相连门
                int[] links = PoeTree.Get(id).links;
                if (links != null && links.Length > 0)
                    s.Allocated[links[0]] = true;

                int unspent = s.Unspent;
                string before = PassiveAwareProductionSimulation.StatePayload(s);
                string err;
                Assert.IsFalse(s.TryAllocate(id, out err), "不可通行节点必须被拒绝：" + id);
                Assert.AreEqual(unspent, s.Unspent, "拒绝必须零扣点：" + id);
                Assert.IsFalse(s.Allocated[id], "拒绝不得写入 selected 集：" + id);
                Assert.AreEqual(before, PassiveAwareProductionSimulation.StatePayload(s), "拒绝必须零状态变化：" + id);
                Assert.IsFalse(s.CanAllocate(id), "UI 不得显示为可点：" + id);
                Assert.IsNotNull(s.NodeBlockReason(id), "必须给出稳定原因：" + id);
            }
        }

        /// <summary>AC-13：不相连与点数不足两条既有负向对照必须继续拒绝。</summary>
        [Test]
        public void TraversalNegativeControls_NonAdjacentAndNoPoints()
        {
            var s = new SliceSession();
            s.ResetTown(5u);
            bool[] reach = PassiveSupport.ReachableSet();
            int far = -1;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (i == SliceSession.StartNode)
                    continue;
                if (!PassiveSupport.IsTraversable(PassiveSupport.EvaluateTruth(i).Traversal))
                    continue;
                if (!reach[i]) { far = i; break; }
            }
            Assert.GreaterOrEqual(far, 0, "必须存在'可通行但与起点不连通'的普通节点");

            string err;
            int unspent = s.Unspent;
            Assert.IsFalse(s.TryAllocate(far, out err), "不相连节点必须被拒绝");
            Assert.AreEqual("需与已点亮节点相连", err);
            Assert.AreEqual(unspent, s.Unspent);
            Assert.IsFalse(s.Allocated[far]);

            var s2 = new SliceSession();
            s2.ResetTown(5u);
            s2.Unspent = 0;
            int adjacent = FirstTraversableNeighbour();
            Assert.GreaterOrEqual(adjacent, 0);
            Assert.IsFalse(s2.TryAllocate(adjacent, out err), "点数不足必须被拒绝");
            Assert.AreEqual("没有天赋点", err);
            Assert.IsFalse(s2.Allocated[adjacent]);
        }

        // ==================== 4. ProdSim 敏感性 ====================

        /// <summary>
        /// AC-19：分配 route-only 节点必须改变 passive-selection 身份（ps|），
        /// 但 pm / pe / pk 必须 EXACT MATCH（即 0 gameplay 效果）。
        /// </summary>
        [Test]
        public void ProdSimSensitivity_RouteOnlyAllocationChangesIdentityOnly()
        {
            PassiveTraversal04A2.Fixture f = PassiveTraversal04A2.DiscoverFixture();
            Assert.GreaterOrEqual(f.FirstRouteOnly, 0, "冻结路径必须含 route-only 节点");
            int r = f.FirstRouteOnly;

            var prefix = new List<int>();
            for (int i = 1; i < f.Path.Length; i++)
            {
                if (f.Path[i] == r)
                    break;
                prefix.Add(f.Path[i]);
            }

            var a = new SliceSession();
            a.ResetTown(20260911u);
            var b = new SliceSession();
            b.ResetTown(20260911u);
            string err;
            for (int i = 0; i < prefix.Count; i++)
            {
                Assert.IsTrue(a.TryAllocate(prefix[i], out err), "前缀必须可分配：" + err);
                Assert.IsTrue(b.TryAllocate(prefix[i], out err), "前缀必须可分配（B）：" + err);
            }
            Assert.AreNotEqual(a.Unspent, b.Unspent + 1, "构造前置：B 比 A 少一点之前状态必须一致");
            Assert.AreEqual(a.Unspent, b.Unspent, "构造前置：A / B 前缀一致");
            Assert.IsTrue(b.TryAllocate(r, out err), "route-only 节点必须可分配：" + err);
            Assert.AreEqual(a.Unspent - 1, b.Unspent, "route-only 分配必须扣 1 点");

            string pa = PassiveAwareProductionSimulation.StatePayload(a);
            string pb = PassiveAwareProductionSimulation.StatePayload(b);
            Assert.AreNotEqual(Section(pa, "ps|"), Section(pb, "ps|"), "ps: DIFFERENT（身份必须对 route-only 分配敏感）");
            Assert.AreEqual(Section(pa, "pm|"), Section(pb, "pm|"), "pm: EXACT MATCH");
            Assert.AreEqual(Section(pa, "pe|"), Section(pb, "pe|"), "pe: EXACT MATCH");
            Assert.AreEqual(Section(pa, "pk|"), Section(pb, "pk|"), "pk: EXACT MATCH");
        }

        /// <summary>
        /// 诚实取证：367 个 supported 节点里有 42 个连参考图都到不了（被不可通行的特殊节点围住）。
        /// 合同 §7.2 的 D 明确不含它们，所以它们不参与「100% 可达」验收 ——
        /// 但必须被显式钉死并登记，不得因为落在分母之外就当它们不存在。
        /// </summary>
        [Test]
        public void SupportedNodesOutsideStartConnectedRegion_AreRegistered()
        {
            int[] outside = PassiveTraversal04A2.NotStartConnectedSupported();
            Assert.AreEqual(ExpectedFullySupported - FrozenSupportedDenominator, outside.Length,
                "supported 总数 − start-connected supported = 孤立区 supported");
            bool[] rref = PassiveTraversal04A2.ReferenceReach();
            for (int i = 0; i < outside.Length; i++)
                Assert.IsFalse(rref[outside[i]], "被登记为孤立区的节点不得出现在参考可达集里：" + outside[i]);
        }

        // ==================== 5. 证据产物 ====================

        /// <summary>把可达性测量与 fixture 写成机器可读产物（tooling 层；判据仍是上面的断言）。</summary>
        [Test]
        public void WritesTraversalArtifact()
        {
            string path = PassiveTraversal04A2.ArtifactPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, PassiveTraversal04A2.RenderJson(), new UTF8Encoding(false));
            Assert.IsTrue(File.Exists(path), "产物必须落盘：" + path);
        }

        // ==================== helpers ====================

        /// <summary>
        /// 把节点直接塞进 selected 集（绕过分配门，模拟损坏/注入状态），
        /// 要求身份（ps|）变化、而 pm|/pe|/pk| 一字不变。
        /// </summary>
        static void AssertContributesNothing(int id)
        {
            var s = new SliceSession();
            s.ResetTown(7u);
            string before = PassiveAwareProductionSimulation.StatePayload(s);
            s.Allocated[id] = true;
            s.RecalcPlayer(false);
            string after = PassiveAwareProductionSimulation.StatePayload(s);

            Assert.AreNotEqual(Section(before, "ps|"), Section(after, "ps|"), "身份必须变化：" + id);
            Assert.AreEqual(Section(before, "pm|"), Section(after, "pm|"), "不得污染 pm|：" + id);
            Assert.AreEqual(Section(before, "pe|"), Section(after, "pe|"), "不得污染 pe|：" + id);
            Assert.AreEqual(Section(before, "pk|"), Section(after, "pk|"), "不得污染 pk|：" + id);
        }

        static string Section(string payload, string tag)
        {
            int at = payload.IndexOf(tag, StringComparison.Ordinal);
            if (at < 0)
                return "";
            int end = payload.IndexOf('\n', at);
            return end < 0 ? payload.Substring(at) : payload.Substring(at, end - at);
        }

        static int CountAllocated(SliceSession s)
        {
            int n = 0;
            for (int i = 0; i < s.Allocated.Length; i++)
                if (s.Allocated[i])
                    n++;
            return n;
        }

        static int FirstLockedNode()
        {
            for (int i = 0; i < PoeTree.Count; i++)
                if (PoeTree.Get(i).locked != 0)
                    return i;
            return -1;
        }

        static int FirstTraversableNeighbour()
        {
            int[] links = PoeTree.Get(SliceSession.StartNode).links;
            if (links == null)
                return -1;
            for (int i = 0; i < links.Length; i++)
                if (PassiveSupport.IsTraversable(PassiveSupport.EvaluateTruth(links[i]).Traversal))
                    return links[i];
            return -1;
        }
    }
}
