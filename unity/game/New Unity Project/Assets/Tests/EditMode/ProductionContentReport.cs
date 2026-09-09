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
    /// S4-P1 Production Content Report v1（工作令 S4-P0P1-PRODUCTION-READINESS，Branch A）。
    /// 单一真值约束（工作令 §12「不复制真相」）：本报告只汇总 canonical truth——
    ///   审计 verdict/计数/runtime coverage 来自 ContentAuditS2Tests.CollectCurrentRepositoryAudit()（既有 Collect→Render→Persist→Assert seam，本类不建第二套 Catalog scanner）；
    ///   兼容组合=SupportCompatGolden oracle；Passive 边=PassiveCatalog.Links（只读）；装备槽=EquipSlot；
    ///   正式敌人视觉=EnemyVisualCatalog（从 catalog 派生，不硬编码数量）；资源契约经 audit result（REQUIRED 缺失/UNKNOWN Stat 等已由 audit 判定）。
    /// Verdict 只继承 audit 真相：unused reserved Tag / GATED Voice（人声）/ Stage0 内容不存在 ≠ FAIL（工作令 §11）。
    /// 渲染为纯函数：同一 Data 两次渲染 byte 级一致（无时间戳/GUID/用户名/绝对路径）。
    /// </summary>
    internal static class ProductionContentReport
    {
        internal const string SchemaName = "CONTENT_PRODUCTION_REPORT_V1";
        internal const int SchemaVersion = 1;
        /// <summary>generatedBy 记录生成器与收集 seam；不记录 commit/时间戳——报告必须可由同 HEAD 重复再生（确定性）。</summary>
        internal const string GeneratedBy = "Game.Tests.EditMode.ProductionContentReportTests（EditMode 测试再生；collector=ContentAuditS2Tests.CollectCurrentRepositoryAudit）";

        internal static string ArtifactPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/CONTENT_PRODUCTION_REPORT.json")); }
        }

        internal sealed class VisualRow
        {
            public string EnemyKind;
            public string ResourceKey;
            public bool Formal;
        }

        internal sealed class SlotAffixCount
        {
            public string Slot;
            public int EligibleAffixCount;
        }

        internal sealed class Data
        {
            public string Verdict;
            public bool AuditCompleted;
            public string AuditExecutionError;
            public int AuditFailureCount;

            public int SkillCount;
            public int SupportCount;
            public int AffixCount;
            public int PassiveCount;
            public int EnemyKindCount;
            public int MapModCount;
            public int EquipmentSlotCount;

            public List<string> DeclaredStats;
            public List<string> ConsumedStats;
            public List<string> UnconsumedDeclaredStats;
            public List<string> DeclaredTags;
            public List<string> UnusedTags;

            public int ComboTotal;
            public int ComboCompatible;
            public int ComboIncompatible;
            public string OracleParityStatus;

            // S4-P3：Affix applicability 汇总（全部派生 canonical predicate IsApplicable，禁止手写 expected counts）
            public int AffixUnrestricted;
            public int AffixRestricted;
            public List<SlotAffixCount> AffixBySlot;

            public int PassiveEdgeCount;
            public int PassiveDisconnectedCount;

            public int RequiredPresent;
            public int RequiredMissing;
            public int GatedPresent;
            public int GatedMissing;
            public List<string> MissingRequired;
            public string VoiceStatus;

            public List<VisualRow> Visuals;
            public int FormalVisualCount;
            public int MissingFormalNonBenchmark;
        }

        /// <summary>从 canonical audit result 构建报告数据。不重新校验内容（校验真相只有 audit 一份）。</summary>
        internal static Data Build(ContentAuditResult audit)
        {
            var d = new Data();
            d.AuditCompleted = audit.Completed;
            d.AuditExecutionError = audit.ExecutionError;
            d.AuditFailureCount = audit.FailureCount;
            d.Verdict = audit.Completed && audit.FailureCount == 0 ? "PASS" : "FAIL";

            d.SkillCount = audit.ActiveSkillCount;
            d.SupportCount = audit.SupportCount;
            d.AffixCount = audit.AffixCount;
            d.PassiveCount = audit.PassiveCount;
            d.EnemyKindCount = audit.EnemyCount;
            d.MapModCount = audit.MapAffixCount;
            d.EquipmentSlotCount = (int)EquipSlot.Count;

            d.DeclaredStats = new List<string>(audit.DeclaredStats);
            d.DeclaredStats.Sort(StringComparer.Ordinal);
            d.ConsumedStats = new List<string>();
            foreach (var s in ContentAuditS2Tests.RuntimeConsumedStats)
                d.ConsumedStats.Add(s.ToString());
            var consumedSet = new HashSet<string>(d.ConsumedStats);
            d.UnconsumedDeclaredStats = new List<string>();
            foreach (var s in d.DeclaredStats)
                if (!consumedSet.Contains(s))
                    d.UnconsumedDeclaredStats.Add(s);

            d.DeclaredTags = new List<string>();
            foreach (Tag t in Enum.GetValues(typeof(Tag)))
                if (t != Tag.None && ((uint)SkillTagGolden.DeclaredTags & (uint)t) == (uint)t)
                    d.DeclaredTags.Add(t.ToString());
            d.UnusedTags = new List<string>(audit.UnusedTags);

            var skills = new List<SkillId>(SkillTagGolden.Masks.Keys);
            skills.Sort();
            int total = 0, compatible = 0;
            for (int i = 1; i <= audit.SupportCount; i++)
            {
                SupportId id = SupportCatalog.Get((SupportId)i).Id;
                foreach (var s in skills)
                {
                    total++;
                    if (SupportCompatGolden.Contains(id, s))
                        compatible++;
                }
            }
            d.ComboTotal = total;
            d.ComboCompatible = compatible;
            d.ComboIncompatible = total - compatible;
            d.OracleParityStatus = audit.CompatProblems.Count == 0 ? "OK" : "VIOLATED";

            // S4-P3：Affix applicability（canonical predicate 派生）
            d.AffixBySlot = new List<SlotAffixCount>();
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                int eligible = 0;
                for (int i = 0; i < AffixCatalog.Count; i++)
                    if (AffixCatalog.Get((AffixId)i).IsApplicable((EquipSlot)s))
                        eligible++;
                d.AffixBySlot.Add(new SlotAffixCount { Slot = ((EquipSlot)s).ToString(), EligibleAffixCount = eligible });
            }
            d.AffixUnrestricted = 0;
            d.AffixRestricted = 0;
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                if (AffixCatalog.Get((AffixId)i).AllowedSlots == 0)
                    d.AffixUnrestricted++;
                else
                    d.AffixRestricted++;
            }

            // Passive：只读 canonical Links（越界/单向由 audit MissingLinks 判定，此处只汇总）。
            int edgeCount = 0;
            for (int i = 0; i < PassiveCatalog.Count; i++)
                foreach (var l in PassiveCatalog.Get(i).Links)
                    if (l >= 0 && l < PassiveCatalog.Count && i < l)
                        edgeCount++;
            d.PassiveEdgeCount = edgeCount;
            d.PassiveDisconnectedCount = CountDisconnected();

            d.RequiredPresent = audit.RequiredPassed;
            d.RequiredMissing = audit.RequiredTotal - audit.RequiredPassed;
            d.GatedPresent = audit.GatedPresent;
            d.GatedMissing = audit.GatedMissing;
            d.MissingRequired = new List<string>(audit.RequiredResourceMissing);
            d.VoiceStatus = "GATED / DEFERRED BY DIRECTOR（人声 3 键缺失不失败，如实记录）";

            d.Visuals = new List<VisualRow>();
            int formal = 0, missingFormal = 0;
            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                string path = EnemyVisualCatalog.VisualResourcePath(kind);
                // Dummy=gameplay 木桩（非正式美术对象），按工作令 §10 排除出 formal 统计
                bool eligible = kind != EnemyKind.Dummy;
                bool isFormal = eligible && path != null;
                d.Visuals.Add(new VisualRow { EnemyKind = kind.ToString(), ResourceKey = path, Formal = isFormal });
                if (eligible)
                {
                    if (path != null) formal++; else missingFormal++;
                }
            }
            d.FormalVisualCount = formal;
            d.MissingFormalNonBenchmark = missingFormal;
            return d;
        }

        static int CountDisconnected()
        {
            if (PassiveCatalog.Count == 0)
                return 0;
            var seen = new bool[PassiveCatalog.Count];
            var queue = new Queue<int>();
            queue.Enqueue(0);
            seen[0] = true;
            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                foreach (var l in PassiveCatalog.Get(cur).Links)
                    if (l >= 0 && l < PassiveCatalog.Count && !seen[l]) { seen[l] = true; queue.Enqueue(l); }
            }
            int disconnected = 0;
            for (int i = 0; i < seen.Length; i++)
                if (!seen[i])
                    disconnected++;
            return disconnected;
        }

        /// <summary>确定性 JSON 渲染：固定字段顺序、Ordinal 排序集合、无环境相关值。</summary>
        internal static string RenderJson(Data d)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"schema\": "); AppendString(sb, SchemaName); sb.Append(",\n");
            sb.Append("  \"schemaVersion\": ").Append(SchemaVersion).Append(",\n");
            sb.Append("  \"generatedBy\": "); AppendString(sb, GeneratedBy); sb.Append(",\n");
            sb.Append("  \"verdict\": "); AppendString(sb, d.Verdict); sb.Append(",\n");
            sb.Append("  \"audit\": { \"completed\": ").Append(d.AuditCompleted ? "true" : "false");
            sb.Append(", \"failureCount\": ").Append(d.AuditFailureCount);
            sb.Append(", \"executionError\": "); AppendNullableString(sb, d.AuditExecutionError);
            sb.Append(" },\n");
            sb.Append("  \"counts\": {\n");
            sb.Append("    \"skills\": ").Append(d.SkillCount).Append(",\n");
            sb.Append("    \"supports\": ").Append(d.SupportCount).Append(",\n");
            sb.Append("    \"affixes\": ").Append(d.AffixCount).Append(",\n");
            sb.Append("    \"passives\": ").Append(d.PassiveCount).Append(",\n");
            sb.Append("    \"enemyKinds\": ").Append(d.EnemyKindCount).Append(",\n");
            sb.Append("    \"mapMods\": ").Append(d.MapModCount).Append(",\n");
            sb.Append("    \"equipmentSlots\": ").Append(d.EquipmentSlotCount).Append("\n");
            sb.Append("  },\n");
            sb.Append("  \"runtimeCoverage\": {\n");
            sb.Append("    \"declaredStats\": "); AppendArray(sb, d.DeclaredStats); sb.Append(",\n");
            sb.Append("    \"consumedStats\": "); AppendArray(sb, d.ConsumedStats); sb.Append(",\n");
            sb.Append("    \"unconsumedDeclaredStats\": "); AppendArray(sb, d.UnconsumedDeclaredStats); sb.Append(",\n");
            sb.Append("    \"declaredTags\": "); AppendArray(sb, d.DeclaredTags); sb.Append(",\n");
            sb.Append("    \"unusedTags\": "); AppendArray(sb, d.UnusedTags); sb.Append("\n");
            sb.Append("  },\n");
            sb.Append("  \"supportCompatibility\": { \"total\": ").Append(d.ComboTotal);
            sb.Append(", \"compatible\": ").Append(d.ComboCompatible);
            sb.Append(", \"incompatible\": ").Append(d.ComboIncompatible);
            sb.Append(", \"oracleParityStatus\": "); AppendString(sb, d.OracleParityStatus);
            sb.Append(" },\n");
            sb.Append("  \"affixApplicability\": { \"total\": ").Append(d.AffixCount);
            sb.Append(", \"unrestricted\": ").Append(d.AffixUnrestricted);
            sb.Append(", \"restricted\": ").Append(d.AffixRestricted);
            sb.Append(", \"bySlot\": [\n");
            for (int i = 0; i < d.AffixBySlot.Count; i++)
            {
                var sc = d.AffixBySlot[i];
                sb.Append("      { \"slot\": "); AppendString(sb, sc.Slot);
                sb.Append(", \"eligibleAffixCount\": ").Append(sc.EligibleAffixCount);
                sb.Append(" }");
                if (i < d.AffixBySlot.Count - 1)
                    sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("    ] },\n");
            sb.Append("  \"passiveGraph\": { \"nodeCount\": ").Append(d.PassiveCount);
            sb.Append(", \"edgeCount\": ").Append(d.PassiveEdgeCount);
            sb.Append(", \"disconnectedCount\": ").Append(d.PassiveDisconnectedCount);
            sb.Append(" },\n");
            sb.Append("  \"resources\": {\n");
            sb.Append("    \"requiredPresent\": ").Append(d.RequiredPresent).Append(",\n");
            sb.Append("    \"requiredMissing\": ").Append(d.RequiredMissing).Append(",\n");
            sb.Append("    \"gatedPresent\": ").Append(d.GatedPresent).Append(",\n");
            sb.Append("    \"gatedMissing\": ").Append(d.GatedMissing).Append(",\n");
            sb.Append("    \"missingRequired\": "); AppendArray(sb, d.MissingRequired); sb.Append(",\n");
            sb.Append("    \"voiceStatus\": "); AppendString(sb, d.VoiceStatus); sb.Append("\n");
            sb.Append("  },\n");
            sb.Append("  \"enemyVisuals\": {\n");
            sb.Append("    \"formalCount\": ").Append(d.FormalVisualCount).Append(",\n");
            sb.Append("    \"missingFormalNonBenchmark\": ").Append(d.MissingFormalNonBenchmark).Append(",\n");
            sb.Append("    \"dummyExcluded\": true,\n");
            sb.Append("    \"mappings\": [\n");
            for (int i = 0; i < d.Visuals.Count; i++)
            {
                var v = d.Visuals[i];
                sb.Append("      { \"enemyKind\": "); AppendString(sb, v.EnemyKind);
                sb.Append(", \"resourceKey\": "); AppendNullableString(sb, v.ResourceKey);
                sb.Append(", \"formal\": ").Append(v.Formal ? "true" : "false");
                sb.Append(" }");
                if (i < d.Visuals.Count - 1)
                    sb.Append(",");
                sb.Append("\n");
            }
            sb.Append("    ]\n");
            sb.Append("  },\n");
            sb.Append("  \"failPolicyNote\": ");
            AppendString(sb, "verdict 只继承 canonical audit 真相；unused reserved Tag / GATED Voice / Stage0 内容不存在不构成 FAIL；REQUIRED 资源缺失、运行期未知 Stat、兼容矩阵违规、canonical Passive 边破坏、正式视觉缺失均 FAIL（经 audit 失败类别）");
            sb.Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static void AppendString(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"') sb.Append("\\\"");
                else if (c == '\\') sb.Append("\\\\");
                else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else sb.Append(c);
            }
            sb.Append('"');
        }

        static void AppendNullableString(StringBuilder sb, string s)
        {
            if (s == null)
                sb.Append("null");
            else
                AppendString(sb, s);
        }

        static void AppendArray(StringBuilder sb, List<string> items)
        {
            sb.Append('[');
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                AppendString(sb, items[i]);
            }
            sb.Append(']');
        }
    }
}
