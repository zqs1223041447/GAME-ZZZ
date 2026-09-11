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
    /// S6P-WO-01 被动域 census + 门禁覆盖审计（纯 tooling 层：不改 runtime，不改 canonical 数据）。
    /// S6P-WO-04A 起：**分类规则不再由本文件拥有** —— 行分类与节点资格一律调用 runtime 唯一 owner
    /// <see cref="PassiveSupport"/>；本文件只做分母统计与产物渲染（"census 消费 truth"）。
    ///
    /// 要回答三件事，且必须可机械验证（禁止"看起来对"）：
    ///   1. 节点集合对账：官方数据 3390 / 升华 558 / 非升华 2832 / 上树 2429 / 未上树 403 的包含关系。
    ///   2. 效果行四分类：CONSUMED / BLOCKED_BY_DOMAIN / SPECIAL_INTERACTION / STRUCTURAL，UNKNOWN 必须为 0。
    ///   3. ProdSim 面审计：确定性哈希到底吃不吃被动构筑真相。
    /// </summary>
    internal static class PassiveCensus
    {
        internal const string SchemaName = "PASSIVE_CENSUS_REPORT_V2";
        internal const int SchemaVersion = 2;
        internal const string GeneratedBy =
            "Game.Tests.EditMode.PassiveCensusTests（EditMode 测试再生；source=Resources/UI/PoE/passive_tree.json + PoeStatParser + ProductionSimulator 源码审计）";

        /// <summary>docs/qa 下的机器可读产物路径（与 ProductionSimulator/ProductionContentReport 同一约定）。</summary>
        internal static string ArtifactPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/PASSIVE_CENSUS_REPORT.json")); }
        }

        // ================= 源数据事实（engine 内不可重算，故记录出处；恒等式在测试里校验） =================

        /// <summary>出处：官方天赋树内嵌数据 `tree_raw.json` 的 `nodes` 键总数。验证命令见 S6P_WO_01 记录。</summary>
        internal const int SourceTotalNodes = 3390;
        /// <summary>出处：`tree_raw.json` 中带 `ascendancyName` 的节点数（升华，本域不纳入）。</summary>
        internal const int SourceAscendancyNodes = 558;
        /// <summary>出处：`tree_raw.json` 中非升华但 `groups[node.group]` 不存在的节点数（无坐标，官方页面也不绘制）。</summary>
        internal const int SourceUnplottedNonAscendancyNodes = 403;
        /// <summary>导演口径「2832 节点」。出处：非升华节点总数 = 上树 2429 + 未上树 403。</summary>
        internal const int DirectorStatedNodeCount = 2832;

        // ================= 分类（实现在 runtime：PassiveSupport） =================

        /// <summary>节点级资格（S6P-WO-04A：由 <see cref="PassiveSupport.EvaluateNode(int)"/> 唯一判定）。</summary>
        internal enum NodeEligibility
        {
            /// <summary>全部效果行都可被现有引擎语义兑现。</summary>
            Supported = 0,
            /// <summary>需要当前不存在的 bespoke handler（珠宝孔 / 时光珠宝类节点）。</summary>
            Special = 1,
            /// <summary>含现有引擎无法兑现的效果行 —— 整节点不可分配（合同 §14）。</summary>
            UnsupportedCurrently = 2,
            /// <summary>专精：WO-03 建立显式选择前不可分配（合同 §17）。</summary>
            MasteryPending = 3
        }

        internal sealed class LineRow
        {
            public int NodeIndex;
            public string NodeName;
            public int Kind;
            public bool Locked;
            /// <summary>"stats"（节点本体词条）或 "choices"（专精可选效果）。</summary>
            public string Source;
            public string Text;
            public PassiveSupport.LineBucket Bucket;
            /// <summary>仅 BlockedByDomain 有值。</summary>
            public string Domain;
        }

        internal sealed class Data
        {
            // 1. 节点集合
            public int SourceTotal;
            public int SourceAscendancy;
            public int SourceUnplottedNonAscendancy;
            public int DirectorStated;
            public int Plotted;
            public int Groups;
            public int Locked;
            public int KindNormal, KindNotable, KindKeystone, KindMastery, KindJewel, KindStart;
            public int NodesWithStatsText;
            public int NodesWithoutStatsText;
            public int NodesWithChoices;

            // 2. 效果行
            public int LinesTotal;
            public int LinesConsumed;
            public int LinesBlockedByDomain;
            public int LinesSpecialInteraction;
            public int LinesStructural;
            public int LinesUnknown;
            public List<string> Domains = new List<string>();
            public List<int> DomainCounts = new List<int>();
            public List<string> UnknownSamples = new List<string>();
            public List<string> StructuralSamples = new List<string>();

            // 节点级资格
            public int NodesSupported;
            public int NodesSpecial;
            public int NodesUnsupportedCurrently;
            /// <summary>专精过渡态（WO-03 前不可分配；S6P-WO-04A §17）。</summary>
            public int NodesMasteryPending;

            // 3. ProdSim 面
            public List<string> ProdSimHashInputSites = new List<string>();
            public bool ProdSimHashPayloadMentionsPassive;
            public bool ProdSimReadsAllocatedState;
            public bool ProdSimHashDependsOnPassiveResult;
            public bool ProdSimMasterySensitive;
            public string ProdSimVerdict;
            /// <summary>哈希喂点里出现的禁用内容 token（合同 §6 口径；非空即 NEEDS_MANUAL_REVIEW）。</summary>
            public List<string> ProdSimForbiddenTokensFound = new List<string>();

            // 4. 分配不变量覆盖
            public List<string> InvariantNames = new List<string>();
            public List<bool> InvariantCovered = new List<bool>();
        }

        // ================= 收集 =================

        internal static Data Collect()
        {
            var d = new Data();
            d.SourceTotal = SourceTotalNodes;
            d.SourceAscendancy = SourceAscendancyNodes;
            d.SourceUnplottedNonAscendancy = SourceUnplottedNonAscendancyNodes;
            d.DirectorStated = DirectorStatedNodeCount;

            PoeNode[] nodes = PoeTree.Nodes;
            PoeGroup[] groups = PoeTree.Groups;
            d.Plotted = nodes == null ? 0 : nodes.Length;
            d.Groups = groups == null ? 0 : groups.Length;

            var domainCount = new Dictionary<string, int>(StringComparer.Ordinal);
            var unknownSeen = new HashSet<string>(StringComparer.Ordinal);
            var structuralSeen = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < d.Plotted; i++)
            {
                PoeNode n = nodes[i];
                if (n.locked != 0) d.Locked++;

                switch (n.Kind)
                {
                    case PoeNodeKind.Notable: d.KindNotable++; break;
                    case PoeNodeKind.Keystone: d.KindKeystone++; break;
                    case PoeNodeKind.Mastery: d.KindMastery++; break;
                    case PoeNodeKind.Jewel: d.KindJewel++; break;
                    case PoeNodeKind.Start: d.KindStart++; break;
                    default: d.KindNormal++; break;
                }

                bool hasStats = !string.IsNullOrEmpty(n.stats);
                if (hasStats) d.NodesWithStatsText++; else d.NodesWithoutStatsText++;
                if (!string.IsNullOrEmpty(n.choices)) d.NodesWithChoices++;

                int nodeBlocked = 0, nodeSpecial = 0, nodeConsumed = 0;
                ClassifyText(d, i, n, "stats", n.stats, domainCount, unknownSeen, structuralSeen,
                    ref nodeConsumed, ref nodeBlocked, ref nodeSpecial);
                ClassifyText(d, i, n, "choices", n.choices, domainCount, unknownSeen, structuralSeen,
                    ref nodeConsumed, ref nodeBlocked, ref nodeSpecial);

                // 资格判定走 runtime 唯一实现（禁止在 Collect 里再写一套 blocked/special 规则）
                int c2, b2, s2, st2;
                switch (ClassifyNode(n, out c2, out b2, out s2, out st2))
                {
                    case NodeEligibility.UnsupportedCurrently: d.NodesUnsupportedCurrently++; break;
                    case NodeEligibility.Special: d.NodesSpecial++; break;
                    case NodeEligibility.MasteryPending: d.NodesMasteryPending++; break;
                    default: d.NodesSupported++; break;
                }
            }

            d.Domains.AddRange(domainCount.Keys);
            d.Domains.Sort(StringComparer.Ordinal);
            for (int i = 0; i < d.Domains.Count; i++)
                d.DomainCounts.Add(domainCount[d.Domains[i]]);

            d.UnknownSamples.AddRange(unknownSeen);
            d.UnknownSamples.Sort(StringComparer.Ordinal);
            if (d.UnknownSamples.Count > 300)
                d.UnknownSamples.RemoveRange(300, d.UnknownSamples.Count - 300);
            d.StructuralSamples.AddRange(structuralSeen);
            d.StructuralSamples.Sort(StringComparer.Ordinal);
            if (d.StructuralSamples.Count > 20)
                d.StructuralSamples.RemoveRange(20, d.StructuralSamples.Count - 20);

            CollectProdSimSurface(d);
            CollectInvariants(d);
            return d;
        }

        static void ClassifyText(Data d, int index, PoeNode n, string source, string text,
            Dictionary<string, int> domainCount, HashSet<string> unknownSeen, HashSet<string> structuralSeen,
            ref int consumed, ref int blocked, ref int special)
        {
            if (string.IsNullOrEmpty(text))
                return;

            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line;
                if (nl < 0) { line = text.Substring(start); start = text.Length; }
                else { line = text.Substring(start, nl - start); start = nl + 1; }

                line = line.Trim();
                if (line.Length == 0)
                    continue;   // 空行不是效果行

                d.LinesTotal++;

                string domain;
                PassiveSupport.LineBucket bucket = PassiveSupport.ClassifyLine(line, out domain);

                if (bucket == PassiveSupport.LineBucket.Structural && structuralSeen.Count < 200) structuralSeen.Add(line);
                else if (bucket == PassiveSupport.LineBucket.BlockedByDomain)
                {
                    int c;
                    domainCount.TryGetValue(domain, out c);
                    domainCount[domain] = c + 1;
                }
                else if (bucket == PassiveSupport.LineBucket.Unknown && unknownSeen.Count < 400) unknownSeen.Add(line);

                if (bucket == PassiveSupport.LineBucket.Consumed) { d.LinesConsumed++; consumed++; }
                else if (bucket == PassiveSupport.LineBucket.BlockedByDomain) { d.LinesBlockedByDomain++; blocked++; }
                else if (bucket == PassiveSupport.LineBucket.SpecialInteraction) { d.LinesSpecialInteraction++; special++; }
                else if (bucket == PassiveSupport.LineBucket.Structural) { d.LinesStructural++; }
                else d.LinesUnknown++;
            }
        }

        /// <summary>
        /// 节点级资格：**委托** runtime 唯一 owner（<see cref="PassiveSupport.EvaluateNode(PoeNode)"/>）。
        /// 行计数只为报告用，分类也一律走 <see cref="PassiveSupport.ClassifyLine"/>。
        /// </summary>
        internal static NodeEligibility ClassifyNode(PoeNode n, out int consumed, out int blocked, out int special, out int structural)
        {
            consumed = blocked = special = structural = 0;
            ClassifyNodeText(n.stats, ref consumed, ref blocked, ref special, ref structural);
            ClassifyNodeText(n.choices, ref consumed, ref blocked, ref special, ref structural);
            switch (PassiveSupport.EvaluateNode(n))
            {
                case PassiveSupport.NodeStatus.AllocatableSupported: return NodeEligibility.Supported;
                case PassiveSupport.NodeStatus.BlockedSpecialInteraction: return NodeEligibility.Special;
                case PassiveSupport.NodeStatus.SpecialPendingMastery: return NodeEligibility.MasteryPending;
                default: return NodeEligibility.UnsupportedCurrently;
            }
        }

        static void ClassifyNodeText(string text, ref int consumed, ref int blocked, ref int special, ref int structural)
        {
            if (string.IsNullOrEmpty(text))
                return;
            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line;
                if (nl < 0) { line = text.Substring(start); start = text.Length; }
                else { line = text.Substring(start, nl - start); start = nl + 1; }
                line = line.Trim();
                if (line.Length == 0)
                    continue;

                string domain;
                PassiveSupport.LineBucket b = PassiveSupport.ClassifyLine(line, out domain);
                if (b == PassiveSupport.LineBucket.Consumed) consumed++;
                else if (b == PassiveSupport.LineBucket.BlockedByDomain) blocked++;
                else if (b == PassiveSupport.LineBucket.SpecialInteraction) special++;
                else if (b == PassiveSupport.LineBucket.Structural) structural++;
            }
        }

        // ================= ProdSim 面审计（静态：读源码里真正的哈希喂点） =================

        const string ProdSimSourceRelative = "Assets/Tests/EditMode/ProductionSimulator.cs";

        internal static void CollectProdSimSurface(Data d)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ProdSimSourceRelative));
            if (!File.Exists(path))
            {
                d.ProdSimVerdict = "SOURCE_MISSING";
                return;
            }

            string[] lines = File.ReadAllLines(path);
            bool mentionsPassive = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                int at = l.IndexOf("HashString(hash", StringComparison.Ordinal);
                if (at < 0)
                    continue;

                string site = (i + 1).ToString(CultureInfo.InvariantCulture) + ": " + l.Trim();
                if (d.ProdSimHashInputSites.Count < 40)
                    d.ProdSimHashInputSites.Add(site);

                string lower = l.ToLowerInvariant();
                if (lower.Contains("passive") || lower.Contains("allocated") || lower.Contains("stat")
                    || lower.Contains("modifier"))
                    mentionsPassive = true;

                for (int t = 0; t < ForbiddenHashTokens.Length; t++)
                {
                    if (lower.Contains(ForbiddenHashTokens[t]) && !d.ProdSimForbiddenTokensFound.Contains(ForbiddenHashTokens[t]))
                        d.ProdSimForbiddenTokensFound.Add(ForbiddenHashTokens[t]);
                }
            }

            d.ProdSimHashPayloadMentionsPassive = mentionsPassive;
            // 被动语义状态被喂进哈希：喂点里出现 canonical payload 序列化器；payload 由已分配状态构造。
            d.ProdSimReadsAllocatedState = ContainsAny(lines, "AllocatedNodeIds") || ContainsAny(lines, "Allocated[");
            d.ProdSimHashDependsOnPassiveResult = ContainsAny(lines, "StatePayload");
            d.ProdSimMasterySensitive = ContainsAny(lines, "Mastery") || ContainsAny(lines, "choices");

            if (d.ProdSimMasterySensitive || d.ProdSimForbiddenTokensFound.Count > 0)
                d.ProdSimVerdict = "NEEDS_MANUAL_REVIEW";
            else if (mentionsPassive && d.ProdSimReadsAllocatedState && d.ProdSimHashDependsOnPassiveResult)
                d.ProdSimVerdict = "PASSIVE_SENSITIVE";      // S6P-WO-02 之后的期望结论
            else if (!mentionsPassive && !d.ProdSimReadsAllocatedState && !d.ProdSimHashDependsOnPassiveResult)
                d.ProdSimVerdict = "NOT_PASSIVE_SENSITIVE"; // S6P-WO-01 的结论（历史）
            else
                d.ProdSimVerdict = "NEEDS_MANUAL_REVIEW";
        }

        /// <summary>canonical gameplay hash 明确禁止包含的内容（合同 §6）。</summary>
        internal static readonly string[] ForbiddenHashTokens =
        {
            "icon", "texture", "sprite", "tooltip", "zoom", "pan", "resolution", "coordinate", "guid", "timestamp"
        };

        static bool ContainsAny(string[] lines, string token)
        {
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].IndexOf(token, StringComparison.Ordinal) >= 0)
                    return true;
            return false;
        }

        // ================= 分配不变量覆盖 census =================
        // 只记录"是否已有权威测试"的事实；缺的登记给 WO-03/04 补，不在本轮改行为。

        static readonly string[] InvariantNamesAll =
        {
            "ValidStart",              // 起点合法（只能从职业起点开始）
            "ConnectedAllocation",     // 只能加到与已点亮节点相连的节点
            "NoIllegalJump",           // 不可跨图跳点
            "PointAccounting",         // 点数守恒（未花点 = 总点 - 已花）
            "DuplicateAllocationNoop", // 重复加点幂等
            "ResetExact",              // R 重构精确清空并归还点数
            "MasteryPrerequisite"      // 专精置前/选择规则
        };

        internal static void CollectInvariants(Data d)
        {
            // 黑盒探测：只经 TryAllocate/TryRespec 的对外行为取证，不复制 AdjacentToAllocated 的判定逻辑。
            // 用 links 只做**候选挑选**，真值仍由 TryAllocate 的接受/拒绝给出。
            bool startValid = false, connected = false, noJump = false, points = false, dupe = false, reset = false;
            const bool mastery = false;   // 专精前置/选择当前不存在任何状态或规则 —— 登记给 WO-03 建立

            try
            {
                var s = new SliceSession();
                s.ResetTown(12345u);

                int start = SliceSession.StartNode;
                startValid = start >= 0 && start < s.Allocated.Length && s.Allocated[start];

                int near = -1, far = -1;
                int linkedFallback = -1, farFallback = -1;
                int[] startLinks = start >= 0 && start < SliceRules.PassiveCount ? PoeTree.Get(start).links : null;
                var linkedToStart = new HashSet<int>();
                if (startLinks != null)
                    for (int i = 0; i < startLinks.Length; i++) linkedToStart.Add(startLinks[i]);

                for (int i = 0; i < SliceRules.PassiveCount; i++)
                {
                    if (i == start || PoeTree.Get(i).locked != 0)
                        continue;
                    // 04A2 起"相连"由通行维度判定、"真生效"由效果维度判定，二者不再共用布尔值。
                    // 优先挑 FULLY_SUPPORTED 节点，好让这两条不变量证的仍是"连线/点数"而不是兑现率。
                    bool supported = PassiveSupport.YieldsModifiers(PassiveSupport.EvaluateTruth(i).Effect);
                    if (linkedToStart.Contains(i))
                    {
                        if (near < 0 && supported) near = i;
                        else if (near < 0 && linkedFallback < 0) linkedFallback = i;
                    }
                    else
                    {
                        if (far < 0 && supported) far = i;
                        else if (far < 0 && farFallback < 0) farFallback = i;
                    }
                    if (near >= 0 && far >= 0) break;
                }
                if (near < 0) near = linkedFallback;
                if (far < 0) far = farFallback;

                string err;
                if (near >= 0)
                    connected = s.TryAllocate(near, out err);

                int unspentAfterConnected = s.Unspent;

                if (far >= 0)
                {
                    bool acceptedFar = s.TryAllocate(far, out err);
                    noJump = !acceptedFar;
                    points = s.Unspent == unspentAfterConnected;
                }

                if (near >= 0)
                {
                    int before = s.Unspent;
                    s.TryAllocate(near, out err);            // 重复点亮同一节点
                    dupe = s.Unspent == before;
                }

                s.TryRespec(out err);
                reset = s.Unspent == s.TotalPoints && s.Allocated[start];
            }
            catch { }

            d.InvariantNames.AddRange(InvariantNamesAll);
            d.InvariantCovered.Add(startValid);
            d.InvariantCovered.Add(connected);
            d.InvariantCovered.Add(noJump);
            d.InvariantCovered.Add(points);
            d.InvariantCovered.Add(dupe);
            d.InvariantCovered.Add(reset);
            d.InvariantCovered.Add(mastery);
        }

        // ================= 渲染（确定性：同 Data 两次渲染 byte 级一致） =================

        internal static string Render(Data d)
        {
            var sb = new StringBuilder(32 * 1024);
            sb.Append("{\n");
            sb.Append("  \"schema\": \"").Append(SchemaName).Append("\",\n");
            sb.Append("  \"schemaVersion\": ").Append(SchemaVersion).Append(",\n");
            sb.Append("  \"generatedBy\": \"").Append(GeneratedBy).Append("\",\n");

            sb.Append("  \"nodeSets\": {\n");
            sb.Append("    \"sourceTotal\": ").Append(d.SourceTotal).Append(",\n");
            sb.Append("    \"sourceAscendancy\": ").Append(d.SourceAscendancy).Append(",\n");
            sb.Append("    \"sourceUnplottedNonAscendancy\": ").Append(d.SourceUnplottedNonAscendancy).Append(",\n");
            sb.Append("    \"directorStated\": ").Append(d.DirectorStated).Append(",\n");
            sb.Append("    \"plotted\": ").Append(d.Plotted).Append(",\n");
            sb.Append("    \"groups\": ").Append(d.Groups).Append(",\n");
            sb.Append("    \"locked\": ").Append(d.Locked).Append(",\n");
            sb.Append("    \"kinds\": { \"normal\": ").Append(d.KindNormal)
              .Append(", \"notable\": ").Append(d.KindNotable)
              .Append(", \"keystone\": ").Append(d.KindKeystone)
              .Append(", \"mastery\": ").Append(d.KindMastery)
              .Append(", \"jewel\": ").Append(d.KindJewel)
              .Append(", \"start\": ").Append(d.KindStart).Append(" },\n");
            sb.Append("    \"nodesWithStatsText\": ").Append(d.NodesWithStatsText).Append(",\n");
            sb.Append("    \"nodesWithoutStatsText\": ").Append(d.NodesWithoutStatsText).Append(",\n");
            sb.Append("    \"nodesWithChoices\": ").Append(d.NodesWithChoices).Append("\n");
            sb.Append("  },\n");

            sb.Append("  \"effectLines\": {\n");
            sb.Append("    \"total\": ").Append(d.LinesTotal).Append(",\n");
            sb.Append("    \"consumed\": ").Append(d.LinesConsumed).Append(",\n");
            sb.Append("    \"blockedByDomain\": ").Append(d.LinesBlockedByDomain).Append(",\n");
            sb.Append("    \"specialInteraction\": ").Append(d.LinesSpecialInteraction).Append(",\n");
            sb.Append("    \"structural\": ").Append(d.LinesStructural).Append(",\n");
            sb.Append("    \"unknown\": ").Append(d.LinesUnknown).Append(",\n");
            sb.Append("    \"domains\": {");
            for (int i = 0; i < d.Domains.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(d.Domains[i]).Append("\": ").Append(d.DomainCounts[i]);
            }
            sb.Append("},\n");
            sb.Append("    \"unknownSamples\": [");
            for (int i = 0; i < d.UnknownSamples.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(Escape(d.UnknownSamples[i])).Append('"');
            }
            sb.Append("]\n");
            sb.Append("  },\n");

            sb.Append("  \"nodeEligibility\": {\n");
            sb.Append("    \"supported\": ").Append(d.NodesSupported).Append(",\n");
            sb.Append("    \"special\": ").Append(d.NodesSpecial).Append(",\n");
            sb.Append("    \"unsupportedCurrently\": ").Append(d.NodesUnsupportedCurrently).Append(",\n");
            sb.Append("    \"masteryPending\": ").Append(d.NodesMasteryPending).Append("\n");
            sb.Append("  },\n");

            sb.Append("  \"prodSimSurface\": {\n");
            sb.Append("    \"hashInputSites\": [");
            for (int i = 0; i < d.ProdSimHashInputSites.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(Escape(d.ProdSimHashInputSites[i])).Append('"');
            }
            sb.Append("],\n");
            sb.Append("    \"hashPayloadMentionsPassive\": ").Append(Bool(d.ProdSimHashPayloadMentionsPassive)).Append(",\n");
            sb.Append("    \"readsAllocatedState\": ").Append(Bool(d.ProdSimReadsAllocatedState)).Append(",\n");
            sb.Append("    \"hashDependsOnPassiveResult\": ").Append(Bool(d.ProdSimHashDependsOnPassiveResult)).Append(",\n");
            sb.Append("    \"masterySensitive\": ").Append(Bool(d.ProdSimMasterySensitive)).Append(",\n");
            sb.Append("    \"forbiddenTokensFound\": [");
            for (int i = 0; i < d.ProdSimForbiddenTokensFound.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(Escape(d.ProdSimForbiddenTokensFound[i])).Append('"');
            }
            sb.Append("],\n");
            sb.Append("    \"verdict\": \"").Append(d.ProdSimVerdict).Append("\"\n");
            sb.Append("  },\n");

            sb.Append("  \"allocationInvariants\": [\n");
            for (int i = 0; i < d.InvariantNames.Count; i++)
            {
                sb.Append("    { \"name\": \"").Append(d.InvariantNames[i])
                  .Append("\", \"covered\": ").Append(Bool(d.InvariantCovered[i])).Append(" }");
                sb.Append(i == d.InvariantNames.Count - 1 ? "\n" : ",\n");
            }
            sb.Append("  ]\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static string Bool(bool b) { return b ? "true" : "false"; }

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 8);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c == '\n') sb.Append("\\n");
                else if (c == '\r') sb.Append("\\r");
                else if (c == '\t') sb.Append("\\t");
                else if (c < ' ') sb.Append(' ');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        internal static void Write(Data d)
        {
            string path = ArtifactPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Render(d), new UTF8Encoding(false));
        }
    }
}
