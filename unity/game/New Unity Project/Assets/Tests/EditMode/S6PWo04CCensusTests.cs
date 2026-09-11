using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-04C 入口 census：无条件词条里，语义属于已有 RuntimeConsumedStats，但 parser 尚未映射。
    /// </summary>
    public sealed class S6PWo04CCensusTests
    {
        [Test]
        public void ExistingConsumerUnmapped_BucketIsDocumented()
        {
            // 入口 census（parser 接线前）已冻结为 48 行；本测试不再覆盖它。
            string entry = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/WO_04C_EXISTING_CONSUMER_GAP.json"));
            Assert.IsTrue(File.Exists(entry), "入口 gap JSON 必须保留：" + entry);
            StringAssert.Contains("\"total\": 48", File.ReadAllText(entry, Encoding.UTF8));

            var rows = Collect();
            string remaining = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/WO_04C_EXISTING_CONSUMER_REMAINING.json"));
            File.WriteAllText(remaining, Render(rows), Encoding.UTF8);
            TestContext.WriteLine("remaining-existing-consumer lines=" + rows.Count);

            var families = new HashSet<string>();
            for (int i = 0; i < rows.Count; i++)
                families.Add(rows[i].Family);
            foreach (string f in families)
                Assert.AreEqual("AreaDamageMore", f,
                    "04C 接线后剩余桶只能是故意不接的 AreaDamageMore Increased（RawMore-only consumer）");
            Assert.Greater(rows.Count, 0, "AreaDamageMore increased 仍必须留在 remaining 桶（禁止静默 Increased）");
        }

        [Test]
        public void WiredExactUncond_IsConsumed_NegativesStayBlocked()
        {
            string domain;
            Assert.AreEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("5% increased maximum Life", out domain));
            Assert.AreEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("+50 to Armour", out domain));
            Assert.AreEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("+1% to maximum Fire Resistance", out domain));
            Assert.AreEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("12% increased Strength", out domain));

            Assert.AreNotEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("Minions have 12% increased maximum Life", out domain));
            Assert.AreNotEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine("20% increased Area Damage", out domain));
            Assert.AreNotEqual(PassiveSupport.LineBucket.Consumed,
                PassiveSupport.ClassifyLine(
                    "Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating",
                    out domain));
        }

        [Test]
        public void CensusDelta_WritesCurrentCounts()
        {
            int supported = 0, blocked = 0, special = 0, mastery = 0, consumed = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                switch (PassiveSupport.EvaluateNode(i))
                {
                    case PassiveSupport.NodeStatus.AllocatableSupported: supported++; break;
                    case PassiveSupport.NodeStatus.BlockedCurrently: blocked++; break;
                    case PassiveSupport.NodeStatus.BlockedSpecialInteraction: special++; break;
                    case PassiveSupport.NodeStatus.SpecialPendingMastery: mastery++; break;
                }
                consumed += CountConsumed(PoeTree.Get(i).stats);
                consumed += CountConsumed(PoeTree.Get(i).choices);
            }

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

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"before\": {\"supported\":367,\"blocked\":1660,\"special\":87,\"mastery\":315,\"consumedLines\":607,\"reachable\":1985,\"D\":325,\"disconnectedSupported\":42},\n");
            sb.Append("  \"after\": {\"supported\":").Append(supported)
              .Append(",\"blocked\":").Append(blocked)
              .Append(",\"special\":").Append(special)
              .Append(",\"mastery\":").Append(mastery)
              .Append(",\"consumedLines\":").Append(consumed)
              .Append(",\"reachable\":").Append(reachable)
              .Append(",\"D\":").Append(d)
              .Append(",\"disconnectedSupported\":").Append(disconnected).Append("},\n");
            sb.Append("  \"delta\": {\"supported\":").Append(supported - 367)
              .Append(",\"blocked\":").Append(blocked - 1660)
              .Append(",\"consumedLines\":").Append(consumed - 607)
              .Append(",\"D\":").Append(d - 325)
              .Append(",\"disconnectedSupported\":").Append(disconnected - 42).Append("},\n");
            sb.Append("  \"reason\": \"WO-04C exact-uncond parser rules for existing consumers; mixed nodes stay UNFULFILLED\"\n");
            sb.Append("}\n");
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/WO_04C_CENSUS_DELTA.json"));
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TestContext.WriteLine(sb.ToString());

            Assert.AreEqual(87, special, "Jewel/Timeless 不属 04C");
            Assert.AreEqual(315, mastery, "Mastery 静态资格不属 04C");
            Assert.AreEqual(1985, reachable, "TraversalTruth 不因 parser 接线移动");
            Assert.AreEqual(453, supported, "before=367 after=453 delta=+86");
            Assert.AreEqual(1574, blocked, "before=1660 after=1574 delta=-86");
            Assert.AreEqual(411, d, "before=325 after=411 delta=+86");
            Assert.AreEqual(42, disconnected, "start-disconnected supported 未动");
            Assert.AreEqual(798, consumed, "before=607 after=798 delta=+191");
        }

        [Test]
        public void EntryDisposition_All48_NoAmbiguousStop()
        {
            var rows = CollectEntryLoose();
            Assert.AreEqual(48, rows.Count, "入口 unique lines 必须仍为 48");

            int wired = 0, skipped = 0, excluded = 0, ambiguous = 0;
            var byFamily = new SortedDictionary<string, int[]>();
            var sb = new StringBuilder();
            sb.Append("{\n  \"total\": ").Append(rows.Count).Append(",\n  \"OUT_AMBIGUOUS_STOP\": 0,\n  \"lines\": [\n");
            for (int i = 0; i < rows.Count; i++)
            {
                string disp = Disposition(rows[i].Line, rows[i].Family);
                if (disp == "WIRED_EXISTING_CONSUMER") wired++;
                else if (disp == "INTENTIONAL_SKIP_SEMANTIC_MISMATCH") skipped++;
                else if (disp.StartsWith("EXCLUDED_")) excluded++;
                else ambiguous++;

                int[] c;
                if (!byFamily.TryGetValue(rows[i].Family, out c))
                {
                    c = new int[4];
                    byFamily[rows[i].Family] = c;
                }
                if (disp == "WIRED_EXISTING_CONSUMER") c[0]++;
                else if (disp == "INTENTIONAL_SKIP_SEMANTIC_MISMATCH") c[1]++;
                else if (disp.StartsWith("EXCLUDED_")) c[2]++;
                else c[3]++;

                if (i > 0) sb.Append(",\n");
                sb.Append("    {\"family\":\"").Append(rows[i].Family)
                  .Append("\",\"disposition\":\"").Append(disp)
                  .Append("\",\"node\":").Append(rows[i].Node)
                  .Append(",\"line\":\"").Append(rows[i].Line.Replace("\"", "'")).Append("\"}");
            }
            sb.Append("\n  ],\n  \"families\": {\n");
            bool first = true;
            foreach (var kv in byFamily)
            {
                if (!first) sb.Append(",\n");
                first = false;
                sb.Append("    \"").Append(kv.Key).Append("\": {\"wired\":").Append(kv.Value[0])
                  .Append(",\"skipped\":").Append(kv.Value[1])
                  .Append(",\"excluded\":").Append(kv.Value[2])
                  .Append(",\"ambiguous\":").Append(kv.Value[3]).Append("}");
            }
            sb.Append("\n  },\n  \"counts\": {\"wired\":").Append(wired)
              .Append(",\"skipped\":").Append(skipped)
              .Append(",\"excluded\":").Append(excluded)
              .Append(",\"ambiguous\":").Append(ambiguous).Append("}\n}\n");

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/WO_04C_DISPOSITION.json"));
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Assert.AreEqual(0, ambiguous, "OUT_AMBIGUOUS_STOP 必须为 0");
            Assert.AreEqual(2, skipped, "AreaDamageMore Increased ×2 必须 INTENTIONAL_SKIP");
            Assert.Greater(wired, 0);
            Assert.Greater(excluded, 0);
        }

        [Test]
        public void MasteryChoiceCensus_Unchanged_22_AndNoSecondChoice()
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
            Assert.AreEqual(315, masteries, "before=315 after=315 delta=0");
            Assert.AreEqual(1863, choices, "before=1863 after=1863 delta=0");
            Assert.AreEqual(22, supported, "before=22 after=22 delta=0 reason=04C 不改 Mastery choice support");
            Assert.AreEqual(22, withOne);
            Assert.AreEqual(0, withTwo, ">=2 可兑现 choice 仍为 0");
        }

        [Test]
        public void FullySupportedLifeIncreased_RaisesMaxLife()
        {
            int id = FirstFullySupportedWith(StatId.Life, ModOp.Increased);
            Assert.GreaterOrEqual(id, 0, "必须存在 FULLY_SUPPORTED 且含 Life Increased 的节点");
            var s = new SliceSession();
            s.ResetTown(7u);
            s.RecalcPlayer(true);
            float before = s.MaxLife;
            s.Allocated[id] = true;
            s.RecalcPlayer(false);
            Assert.Greater(s.MaxLife, before, "04C 接线后 Life Increased 必须进入 RecalcPlayer.Get(Life)：" + id);
        }

        static int FirstFullySupportedWith(StatId stat, ModOp op)
        {
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (PassiveSupport.EvaluateTruth(i).Effect != PassiveSupport.EffectTruth.FullySupported)
                    continue;
                Modifier[] mods = PoeStatParser.Parse(PoeTree.Get(i).stats);
                for (int m = 0; m < mods.Length; m++)
                    if (mods[m].Stat == stat && mods[m].Op == op)
                        return i;
            }
            return -1;
        }

        static int CountConsumed(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            int n = 0, start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line = nl < 0 ? text.Substring(start) : text.Substring(start, nl - start);
                start = nl < 0 ? text.Length : nl + 1;
                string domain;
                if (PassiveSupport.ClassifyLine(line, out domain) == PassiveSupport.LineBucket.Consumed)
                    n++;
            }
            return n;
        }

        static string Disposition(string line, string family)
        {
            if (family == "AreaDamageMore")
                return "INTENTIONAL_SKIP_SEMANTIC_MISMATCH";
            string lower = line.ToLowerInvariant();
            if (lower.IndexOf("minions") >= 0)
                return "EXCLUDED_NOT_PLAYER_CONSUMER";
            if (line.IndexOf("Converts ", StringComparison.OrdinalIgnoreCase) >= 0)
                return "EXCLUDED_CONVERSION_KEYSTONE";
            if (!HasLeadingNumber(line))
                return "EXCLUDED_NOT_EXACT_UNCOND";
            string domain;
            if (PassiveSupport.ClassifyLine(line, out domain) == PassiveSupport.LineBucket.Consumed)
                return "WIRED_EXISTING_CONSUMER";
            return "OUT_AMBIGUOUS_STOP";
        }

        static bool HasLeadingNumber(string line)
        {
            int i = 0;
            if (i < line.Length && line[i] == '+') i++;
            return i < line.Length && line[i] >= '0' && line[i] <= '9';
        }

        /// <summary>入口 48 行重建：用接线前的松散关键词，且不因现已 CONSUMED 而丢掉。</summary>
        internal static List<Gap> CollectEntryLoose()
        {
            var list = new List<Gap>();
            var seen = new HashSet<string>();
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                AddLoose(list, seen, i, nodes[i].name, "stats", nodes[i].stats);
                AddLoose(list, seen, i, nodes[i].name, "choices", nodes[i].choices);
            }
            return list;
        }

        static void AddLoose(List<Gap> list, HashSet<string> seen, int node, string name, string src, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line = nl < 0 ? text.Substring(start) : text.Substring(start, nl - start);
                start = nl < 0 ? text.Length : nl + 1;
                line = line.Trim();
                if (line.Length == 0 || line[0] == '(')
                    continue;
                string family = LooseEntryFamily(line);
                if (family == null)
                    continue;
                if (!seen.Add(line))
                    continue;
                string domain;
                Gap g;
                g.Node = node;
                g.Name = name;
                g.Source = src;
                g.Line = line;
                g.Family = family;
                g.Bucket = PassiveSupport.ClassifyLine(line, out domain).ToString();
                list.Add(g);
            }
        }

        static string LooseEntryFamily(string line)
        {
            if (ContainsAny(line, "while ", "if ", "when ", "per ", "against ", "recently", "with ", "during "))
                return null;
            if (line.IndexOf("maximum Fire Resistance") >= 0)
                return "MaxFireResistance";
            if (line.IndexOf("Area Damage") >= 0)
                return "AreaDamageMore";
            if (line.EndsWith(" to Armour") || line.EndsWith(" to Armour Rating"))
                return "ArmourFlat";
            if (line.EndsWith(" to Evasion Rating") || line.EndsWith(" to Evasion"))
                return "EvasionFlat";
            if (line.EndsWith(" to Accuracy Rating"))
                return "AccuracyFlat";
            if (line.IndexOf("increased maximum Life") >= 0)
                return "LifeIncreased";
            if (line.IndexOf("increased maximum Mana") >= 0)
                return "ManaIncreased";
            if (line.IndexOf("increased Strength") >= 0)
                return "StrengthIncreased";
            if (line.IndexOf("increased Dexterity") >= 0)
                return "DexterityIncreased";
            if (line.IndexOf("increased Intelligence") >= 0)
                return "IntelligenceIncreased";
            return null;
        }

        internal static List<Gap> Collect()
        {
            var list = new List<Gap>();
            var seen = new HashSet<string>();
            PoeNode[] nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                AddText(list, seen, i, nodes[i].name, "stats", nodes[i].stats);
                AddText(list, seen, i, nodes[i].name, "choices", nodes[i].choices);
            }
            return list;
        }

        static void AddText(List<Gap> list, HashSet<string> seen, int node, string name, string src, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line = nl < 0 ? text.Substring(start) : text.Substring(start, nl - start);
                start = nl < 0 ? text.Length : nl + 1;
                line = line.Trim();
                if (line.Length == 0 || line[0] == '(')
                    continue;
                string domain;
                PassiveSupport.LineBucket b = PassiveSupport.ClassifyLine(line, out domain);
                if (b == PassiveSupport.LineBucket.Consumed)
                    continue;
                string family = GuessExistingFamily(line);
                if (family == null)
                    continue;
                if (!seen.Add(line))
                    continue;
                Gap g;
                g.Node = node;
                g.Name = name;
                g.Source = src;
                g.Line = line;
                g.Family = family;
                g.Bucket = b.ToString();
                list.Add(g);
            }
        }

        /// <summary>
        /// 只认无条件、且能落到现有 StatId 家族的句式。
        /// Minion 前缀 / Converts 转换 / 条件句一律排除（不是「已有玩家 consumer 漏接」）。
        /// </summary>
        internal static string GuessExistingFamily(string line)
        {
            if (ContainsAny(line, "while ", "if ", "when ", "per ", "against ", "recently", "with ", "during ", "minions "))
                return null;
            if (line.IndexOf("Converts ", StringComparison.OrdinalIgnoreCase) >= 0)
                return null;
            if (ExactNumberThen(line, "% increased maximum Life")) return "LifeIncreased";
            if (ExactNumberThen(line, "% increased maximum Mana")) return "ManaIncreased";
            if (ExactNumberThen(line, "% increased Strength")) return "StrengthIncreased";
            if (ExactNumberThen(line, "% increased Dexterity")) return "DexterityIncreased";
            if (ExactNumberThen(line, "% increased Intelligence")) return "IntelligenceIncreased";
            if (ExactNumberThen(line, " to Armour")) return "ArmourFlat";
            if (ExactNumberThen(line, " to Evasion Rating")) return "EvasionFlat";
            if (ExactNumberThen(line, " to Accuracy Rating")) return "AccuracyFlat";
            if (ExactNumberThen(line, "% to maximum Fire Resistance")) return "MaxFireResistance";
            if (ExactNumberThen(line, "% increased Area Damage")) return "AreaDamageMore";
            return null;
        }

        static bool ExactNumberThen(string line, string rest)
        {
            int i = 0;
            if (i < line.Length && line[i] == '+') i++;
            if (i >= line.Length || line[i] < '0' || line[i] > '9')
                return false;
            while (i < line.Length && ((line[i] >= '0' && line[i] <= '9') || line[i] == ',' || line[i] == '.'))
                i++;
            if (i >= line.Length)
                return false;
            return string.CompareOrdinal(line, i, rest, 0, rest.Length) == 0 && i + rest.Length == line.Length;
        }

        static bool ContainsAny(string line, params string[] tokens)
        {
            string lower = line.ToLowerInvariant();
            for (int i = 0; i < tokens.Length; i++)
                if (lower.IndexOf(tokens[i]) >= 0)
                    return true;
            return false;
        }

        static string Render(List<Gap> rows)
        {
            var by = new SortedDictionary<string, int>();
            for (int i = 0; i < rows.Count; i++)
            {
                int n;
                by.TryGetValue(rows[i].Family, out n);
                by[rows[i].Family] = n + 1;
            }
            var sb = new StringBuilder();
            sb.Append("{\n  \"total\": ").Append(rows.Count).Append(",\n  \"families\": {\n");
            bool first = true;
            foreach (var kv in by)
            {
                if (!first) sb.Append(",\n");
                first = false;
                sb.Append("    \"").Append(kv.Key).Append("\": ").Append(kv.Value);
            }
            sb.Append("\n  },\n  \"samples\": [\n");
            int cap = System.Math.Min(40, rows.Count);
            for (int i = 0; i < cap; i++)
            {
                if (i > 0) sb.Append(",\n");
                sb.Append("    {\"family\":\"").Append(rows[i].Family)
                  .Append("\",\"node\":").Append(rows[i].Node)
                  .Append(",\"line\":\"").Append(rows[i].Line.Replace("\"", "'")).Append("\"}");
            }
            sb.Append("\n  ]\n}\n");
            return sb.ToString();
        }

        internal struct Gap
        {
            public int Node;
            public string Name;
            public string Source;
            public string Line;
            public string Family;
            public string Bucket;
        }
    }
}
