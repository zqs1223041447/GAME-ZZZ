using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-04A2 —— 可达性门与恢复 fixture 的**纯计算**层（测试专用：无缓存、无 gameplay 状态）。
    ///
    /// 合同 §7.1 规定连通性只能来自 canonical 图边（<see cref="PoeNode.links"/>，无向、已去重升序），
    /// 严禁用坐标 / orbit / group / 视觉连线推导。这里的 reference traversal 与生产
    /// <see cref="PassiveSupport.ReachableSet"/> 是**两份独立实现**，测试要求两者逐节点一致：
    /// 不一致就说明其中一份错了（互为 oracle）。
    ///
    /// §7.2 的 D（supported denominator）= 「start-connected 的普通、且效果可完整兑现的节点」，
    /// 这是「100% 可达」的正式分母，不允许用「比 14 大很多」代替。
    /// </summary>
    internal static class PassiveTraversal04A2
    {
        /// <summary>可达性测量的固定 seed（合同 §6：起点语义本身不得因本令改变）。</summary>
        internal const int StartNodeId = 2172;

        internal static bool IsOrdinaryTraversable(int id)
        {
            return PassiveSupport.IsTraversable(PassiveSupport.EvaluateTruth(id).Traversal);
        }

        internal static bool IsEffective(int id)
        {
            return PassiveSupport.YieldsModifiers(PassiveSupport.EvaluateTruth(id).Effect);
        }

        /// <summary>route-only = 可通行但效果未兑现（本令的核心类别）。</summary>
        internal static bool IsRouteOnly(int id)
        {
            PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(id);
            return PassiveSupport.IsTraversable(t.Traversal) && !PassiveSupport.YieldsModifiers(t.Effect);
        }

        /// <summary>
        /// 通用图遍历。邻接一律按 <see cref="PoeNode.links"/> 的既有升序访问，
        /// 因此 parent 链就是**唯一的 canonical 最短路径**（BFS 树路径）。
        /// </summary>
        internal static bool[] Reach(int seed, Func<int, bool> canTransit, out int[] parent)
        {
            int n = PoeTree.Count;
            var seen = new bool[n];
            parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = -1;
            if (n <= 0 || seed < 0 || seed >= n)
                return seen;

            var queue = new int[n];
            int head = 0, tail = 0;
            seen[seed] = true;
            queue[tail++] = seed;
            while (head < tail)
            {
                int cur = queue[head++];
                int[] links = PoeTree.Get(cur).links;
                if (links == null)
                    continue;
                for (int i = 0; i < links.Length; i++)
                {
                    int nb = links[i];
                    if (nb < 0 || nb >= n || seen[nb])
                        continue;
                    if (!canTransit(nb))
                        continue;
                    seen[nb] = true;
                    parent[nb] = cur;
                    queue[tail++] = nb;
                }
            }
            return seen;
        }

        /// <summary>R_legacy：旧行为参考可达集 —— 只有「可通行且效果可兑现」的节点可作 transit。</summary>
        internal static bool[] LegacyReach()
        {
            int[] parent;
            return Reach(StartNodeId, id => IsOrdinaryTraversable(id) && IsEffective(id), out parent);
        }

        /// <summary>R_ref：合同 §7.2 的参考图可达集（普通节点无论效果都可作 transit）。</summary>
        internal static bool[] ReferenceReach()
        {
            int[] parent;
            return Reach(StartNodeId, IsOrdinaryTraversable, out parent);
        }

        /// <summary>D = start-connected ordinary supported nodes（合同 §7.2 唯一正式定义），升序。</summary>
        internal static List<int> SupportedDenominator()
        {
            bool[] rref = ReferenceReach();
            var list = new List<int>();
            for (int i = 0; i < PoeTree.Count; i++)
                if (rref[i] && IsOrdinaryTraversable(i) && IsEffective(i))
                    list.Add(i);
            return list;
        }

        internal static int[] Ascending(IList<int> ids)
        {
            int[] a = new int[ids.Count];
            for (int i = 0; i < ids.Count; i++)
                a[i] = ids[i];
            Array.Sort(a);
            return a;
        }

        internal static int[] Missing(bool[] reachable, IList<int> required)
        {
            var miss = new List<int>();
            for (int i = 0; i < required.Count; i++)
            {
                int id = required[i];
                if (id < 0 || id >= reachable.Length || !reachable[id])
                    miss.Add(id);
            }
            miss.Sort();
            return miss.ToArray();
        }

        internal static int[] PathTo(int[] parent, int node)
        {
            var rev = new List<int>();
            int cur = node;
            int guard = parent.Length + 2;
            while (cur != StartNodeId && cur >= 0 && guard-- > 0)
            {
                rev.Add(cur);
                cur = parent[cur];
            }
            if (cur != StartNodeId)
                return new int[0];
            rev.Add(StartNodeId);
            rev.Reverse();
            return rev.ToArray();
        }

        /// <summary>§8.3 冻结的恢复 fixture。</summary>
        internal struct Fixture
        {
            public int Target;
            public int Distance;
            public int[] Path;                 // [2172, ..., Target]
            public int[] RouteOnlyOnPath;
            public int FirstRouteOnly;
        }

        /// <summary>
        /// §7/§8.3 的确定性发现：从 D 中挑「legacy 到不了、参考图到得了、且 canonical 最短路径上
        /// 至少含一个 route-only 节点」的节点，按 (距离, nodeId) 升序取唯一一个。
        /// </summary>
        internal static Fixture DiscoverFixture()
        {
            bool[] rref = ReferenceReach();
            bool[] rlegacy = LegacyReach();
            List<int> d = SupportedDenominator();

            int[] parent;
            Reach(StartNodeId, IsOrdinaryTraversable, out parent);

            int best = -1, bestDist = int.MaxValue;
            for (int i = 0; i < d.Count; i++)
            {
                int t = d[i];
                if (t < 0 || t >= rref.Length || !rref[t] || rlegacy[t])
                    continue;
                int[] path = PathTo(parent, t);
                if (path.Length == 0)
                    continue;
                bool hasRouteOnly = false;
                for (int k = 1; k < path.Length; k++)
                    if (IsRouteOnly(path[k])) { hasRouteOnly = true; break; }
                if (!hasRouteOnly)
                    continue;
                int dist = path.Length - 1;
                if (best < 0 || dist < bestDist || (dist == bestDist && t < best))
                {
                    best = t;
                    bestDist = dist;
                }
            }

            Fixture f = new Fixture();
            f.Target = best;
            if (best < 0)
                return f;

            f.Path = PathTo(parent, best);
            f.Distance = f.Path.Length - 1;
            var ro = new List<int>();
            for (int k = 1; k < f.Path.Length; k++)
                if (IsRouteOnly(f.Path[k]))
                    ro.Add(f.Path[k]);
            f.RouteOnlyOnPath = ro.ToArray();
            f.FirstRouteOnly = ro.Count > 0 ? ro[0] : -1;
            return f;
        }

        /// <summary>候选集大小（合同 §8.2 的 C）：legacy 到不了、参考图到得了、路径含 route-only。</summary>
        internal static int CandidateCount()
        {
            bool[] rref = ReferenceReach();
            bool[] rlegacy = LegacyReach();
            List<int> d = SupportedDenominator();
            int[] parent;
            Reach(StartNodeId, IsOrdinaryTraversable, out parent);
            int n = 0;
            for (int i = 0; i < d.Count; i++)
            {
                int t = d[i];
                if (t < 0 || t >= rref.Length || !rref[t] || rlegacy[t])
                    continue;
                int[] path = PathTo(parent, t);
                if (path.Length == 0)
                    continue;
                for (int k = 1; k < path.Length; k++)
                    if (IsRouteOnly(path[k])) { n++; break; }
            }
            return n;
        }

        internal static string FormatPath(int[] path)
        {
            var sb = new StringBuilder(128);
            sb.Append('[');
            for (int i = 0; i < path.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(path[i].ToString(CultureInfo.InvariantCulture));
            }
            sb.Append(']');
            return sb.ToString();
        }

        internal static string ArtifactPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/WO_04A2_TRAVERSAL_REPORT.json")); }
        }

        /// <summary>把可达性测量与 fixture 写成机器可读产物（证据用；不是判据）。</summary>
        internal static string RenderJson()
        {
            bool[] rref = ReferenceReach();
            bool[] rlegacy = LegacyReach();
            bool[] runtime = PassiveSupport.ReachableSet();
            List<int> d = SupportedDenominator();
            int[] missing = Missing(runtime, d);
            Fixture f = DiscoverFixture();

            int traversable = 0, specialBlocked = 0, outOfDomain = 0;
            int effSupported = 0, effUnfulfilled = 0, effSpecial = 0;
            int legacyCount = 0, refCount = 0, runtimeCount = 0, edgeless = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(i);
                if (PassiveSupport.IsTraversable(t.Traversal)) traversable++;
                else if (t.Traversal == PassiveSupport.TraversalTruth.SpecialBlocked) specialBlocked++;
                else outOfDomain++;
                if (t.Effect == PassiveSupport.EffectTruth.FullySupported) effSupported++;
                else if (t.Effect == PassiveSupport.EffectTruth.Unfulfilled) effUnfulfilled++;
                else effSpecial++;
                if (rlegacy[i]) legacyCount++;
                if (rref[i]) refCount++;
                if (runtime[i]) runtimeCount++;
                int[] lk = PoeTree.Get(i).links;
                if (lk == null || lk.Length == 0) edgeless++;
            }

            var sb = new StringBuilder(4096);
            sb.Append("{\n");
            sb.Append("  \"schema\": \"WO_04A2_TRAVERSAL_REPORT_V1\",\n");
            sb.Append("  \"generatedBy\": \"Game.Tests.EditMode.S6PWo04A2Tests（测试再生；source=Resources/UI/PoE/passive_tree.json + PassiveSupport）\",\n");
            sb.Append("  \"startNode\": ").Append(StartNodeId).Append(",\n");
            sb.Append("  \"nodeCount\": ").Append(PoeTree.Count).Append(",\n");
            sb.Append("  \"edgeCount\": ").Append(EdgeCount()).Append(",\n");
            sb.Append("  \"edgelessNodes\": ").Append(edgeless).Append(",\n");
            sb.Append("  \"traversable\": ").Append(traversable).Append(",\n");
            sb.Append("  \"specialBlocked\": ").Append(specialBlocked).Append(",\n");
            sb.Append("  \"outOfDomain\": ").Append(outOfDomain).Append(",\n");
            sb.Append("  \"effectFullySupported\": ").Append(effSupported).Append(",\n");
            sb.Append("  \"effectUnfulfilled\": ").Append(effUnfulfilled).Append(",\n");
            sb.Append("  \"effectSpecialPending\": ").Append(effSpecial).Append(",\n");
            sb.Append("  \"legacyReachable\": ").Append(legacyCount).Append(",\n");
            sb.Append("  \"referenceReachable\": ").Append(refCount).Append(",\n");
            sb.Append("  \"runtimeReachable\": ").Append(runtimeCount).Append(",\n");
            sb.Append("  \"supportedDenominator\": ").Append(d.Count).Append(",\n");
            sb.Append("  \"supportedReachable\": ").Append(d.Count - missing.Length).Append(",\n");
            sb.Append("  \"supportedNotStartConnected\": ").Append(effSupported - d.Count).Append(",\n");
            sb.Append("  \"supportedNotStartConnectedIds\": ").Append(FormatPath(NotStartConnectedSupported())).Append(",\n");
            sb.Append("  \"missingSupported\": ").Append(FormatPath(missing)).Append(",\n");
            sb.Append("  \"fixtureCandidateCount\": ").Append(CandidateCount()).Append(",\n");
            sb.Append("  \"frozenTarget\": ").Append(f.Target).Append(",\n");
            sb.Append("  \"frozenTargetDistance\": ").Append(f.Distance).Append(",\n");
            sb.Append("  \"frozenPath\": ").Append(FormatPath(f.Path ?? new int[0])).Append(",\n");
            sb.Append("  \"frozenRouteOnlyOnPath\": ").Append(FormatPath(f.RouteOnlyOnPath ?? new int[0])).Append(",\n");
            sb.Append("  \"frozenFirstRouteOnly\": ").Append(f.FirstRouteOnly).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// 效果可完整兑现、但**连参考图都到不了**的节点（升序）。
        /// 合同 §7.2 的 D 明确不含它们，所以它们不参与「100% 可达」验收；
        /// 但它们是真实的玩家不可达 supported 节点，必须如实登记（不允许藏进分母之外当无事发生）。
        /// </summary>
        internal static int[] NotStartConnectedSupported()
        {
            bool[] rref = ReferenceReach();
            var list = new List<int>();
            for (int i = 0; i < PoeTree.Count; i++)
                if (!rref[i] && IsEffective(i))
                    list.Add(i);
            return list.ToArray();
        }

        internal static int EdgeCount()        {
            int edges = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                int[] lk = PoeTree.Get(i).links;
                if (lk == null)
                    continue;
                for (int k = 0; k < lk.Length; k++)
                    if (lk[k] > i)
                        edges++;
            }
            return edges;
        }
    }
}
