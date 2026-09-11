using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S4-P1 Production Content Report 契约测试（工作令 S4-P0P1-PRODUCTION-READINESS §17）。
    /// 覆盖：报告确定性 / 计数=canonical Catalog / Support 组合=golden oracle / 运行期 Stat 消费 parity /
    /// unused reserved Tag 不 FAIL / REQUIRED 资源缺失必须 FAIL / GATED 人声不 FAIL / Passive 边=canonical Links /
    /// 正式视觉从 catalog 派生 / audit block 与源 Result parity（不复制真相守卫）。
    /// 报告再生（写盘 docs/qa/CONTENT_PRODUCTION_REPORT.json）也在此测试内执行——工具生成，禁止手填。
    /// </summary>
    public sealed class ProductionContentReportTests
    {
        [Test]
        public void ProductionContentReport_RegeneratesArtifact_AndVerdictPass()
        {
            var audit = ContentAuditS2Tests.CollectCurrentRepositoryAudit();
            audit.Completed = true; // seam 语义：ContentAuditFailsafe.ExecuteAudit 在 collect 完整返回后置位（Collect→Render→Persist→Assert）
            var data = ProductionContentReport.Build(audit);
            string json = ProductionContentReport.RenderJson(data);
            string path = ProductionContentReport.ArtifactPath;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, json);
            Debug.Log("[ProductionReport] wrote " + path);
            Assert.IsTrue(audit.Completed && audit.FailureCount == 0, "当前仓库应为绿；内容违规时本报告同轮红");
            Assert.AreEqual("PASS", data.Verdict);
            Assert.IsTrue(data.Verdict == "PASS" || data.Verdict == "FAIL", "verdict 无第三种值");
        }

        [Test]
        public void Report_Render_IsDeterministic()
        {
            string first = ProductionContentReport.RenderJson(ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit()));
            string second = ProductionContentReport.RenderJson(ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit()));
            Assert.AreEqual(first, second, "同 HEAD 两次收集+渲染必须 byte 级一致");
        }

        [Test]
        public void Report_Counts_MatchCanonicalCatalogs()
        {
            var data = ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit());
            Assert.AreEqual(SkillTagGolden.Masks.Count, data.SkillCount, "Active 技能计数必须=SkillTagGolden（canonical Active 技能全集）");
            Assert.AreEqual(SupportCatalog.Count, data.SupportCount);
            Assert.AreEqual((int)AffixId.Count, data.AffixCount);
            Assert.AreEqual(PassiveCatalog.Count, data.PassiveCount);
            Assert.AreEqual(MapAffixCatalog.All.Length, data.MapModCount);
            Assert.AreEqual((int)EquipSlot.Count, data.EquipmentSlotCount);
            Assert.AreEqual(Enum.GetValues(typeof(EnemyKind)).Length, data.EnemyKindCount);
        }

        [Test]
        public void Report_SupportCombos_MatchGoldenOracle()
        {
            var data = ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit());
            int goldenCompatible = 0;
            foreach (var pair in SupportCompatGolden.Matrix)
                goldenCompatible += pair.Value.Length;
            Assert.AreEqual(goldenCompatible, data.ComboCompatible, "兼容组合必须=SupportCompatGolden oracle 汇总");
            Assert.AreEqual(SupportCatalog.Count * SkillTagGolden.Masks.Count, data.ComboTotal);
            Assert.AreEqual(data.ComboTotal - data.ComboCompatible, data.ComboIncompatible);
            Assert.AreEqual("OK", data.OracleParityStatus, "golden 矩阵与内容 parity 由 audit 判定，此处必须 OK");
        }

        [Test]
        public void Report_UnconsumedDeclaredStats_ParityWithAuditTruth()
        {
            var audit = ContentAuditS2Tests.CollectCurrentRepositoryAudit();
            var data = ProductionContentReport.Build(audit);
            Assert.IsEmpty(audit.MissingStat, "audit 真相：运行期未知 Stat 必须 0");
            Assert.IsEmpty(data.UnconsumedDeclaredStats, "已声明 Stat 必须全部被 runtime 消费（白名单=ContentAuditS2Tests.RuntimeConsumedStats 单一 truth）");
            // declared ⊆ StatId 枚举名（非法 StatId 由 audit 判定，报告侧只做引用完整性）
            foreach (var s in data.DeclaredStats)
                Assert.IsTrue(Enum.IsDefined(typeof(StatId), s), "declared Stat 必须是 StatId 枚举成员：" + s);
        }

        [Test]
        public void Report_Verdict_FailsOnSyntheticRequiredResourceMissing()
        {
            var r = new ContentAuditResult { Completed = true };
            r.RequiredResourceMissing.Add("synthetic（key=x）：Resources.Load 返回空");
            Assert.AreEqual("FAIL", ProductionContentReport.Build(r).Verdict, "REQUIRED 资源缺失必须 FAIL");
        }

        [Test]
        public void Report_Verdict_FailsOnIncompleteCollect()
        {
            var r = new ContentAuditResult { Completed = false, ExecutionError = "NullReferenceException: synthetic" };
            Assert.AreEqual("FAIL", ProductionContentReport.Build(r).Verdict, "Collect 未完整执行绝不能 PASS");
        }

        [Test]
        public void Report_UnusedReservedTag_DoesNotFail()
        {
            var r = new ContentAuditResult { Completed = true };
            r.UnusedTags.Add("Fire");
            Assert.AreEqual("PASS", ProductionContentReport.Build(r).Verdict, "unused reserved Tag 是信息项，不构成 FAIL");
            Assert.AreEqual(1, ProductionContentReport.Build(r).UnusedTags.Count);
        }

        [Test]
        public void Report_GatedVoiceMissing_DoesNotFail()
        {
            var r = new ContentAuditResult { Completed = true, RequiredPassed = 10, RequiredTotal = 10 };
            r.GatedPresent = 0;
            r.GatedMissing = 3;
            var data = ProductionContentReport.Build(r);
            Assert.AreEqual("PASS", data.Verdict, "GATED 人声（DEFERRED BY DIRECTOR）缺失不失败，如实记录");
            Assert.AreEqual(0, data.GatedPresent);
            Assert.AreEqual(3, data.GatedMissing);
            Assert.IsTrue(!string.IsNullOrEmpty(data.VoiceStatus));
        }

        [Test]
        public void Report_PassiveSummary_MatchesCanonicalLinks()
        {
            var data = ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit());
            // 测试侧独立计数（parity）：无向唯一边
            var pairs = new HashSet<long>();
            for (int i = 0; i < PassiveCatalog.Count; i++)
                foreach (var l in PassiveCatalog.Get(i).Links)
                    if (l >= 0 && l < PassiveCatalog.Count)
                        pairs.Add(((long)Math.Min(i, l) << 32) | (uint)Math.Max(i, l));
            Assert.AreEqual(pairs.Count, data.PassiveEdgeCount, "边汇总必须=canonical Links 无向唯一边");
            Assert.Greater(data.PassiveEdgeCount, 0, "canonical 天赋图必须有边");
            Assert.AreEqual(0, data.PassiveDisconnectedCount, "canonical 天赋图从节点 0 必须全连通");
        }

        [Test]
        public void Report_Visuals_DerivedFromCatalog()
        {
            var data = ProductionContentReport.Build(ContentAuditS2Tests.CollectCurrentRepositoryAudit());
            int expectedFormal = 0;
            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                string path = EnemyVisualCatalog.VisualResourcePath(kind);
                var row = data.Visuals.Find(v => v.EnemyKind == kind.ToString());
                Assert.IsNotNull(row, "每个 EnemyKind 必须有一行：" + kind);
                Assert.AreEqual(path, row.ResourceKey, "resourceKey 必须逐 kind 来自 EnemyVisualCatalog");
                if (kind == EnemyKind.Dummy)
                    Assert.IsFalse(row.Formal, "Dummy 排除出 formal 统计");
                else if (path != null)
                {
                    Assert.IsTrue(row.Formal);
                    expectedFormal++;
                }
            }
            Assert.AreEqual(expectedFormal, data.FormalVisualCount, "formalCount 从 catalog 派生，不硬编码");
            Assert.AreEqual(0, data.MissingFormalNonBenchmark, "当前全部非木桩 EnemyKind 均已接入正式视觉");
        }

        [Test]
        public void Report_AuditBlock_MatchesSourceResult()
        {
            var r = new ContentAuditResult { Completed = true };
            r.MissingStat.Add("synthetic -> 运行期未知 Stat");
            r.SupportCount = 7;
            r.AffixCount = 13;
            var data = ProductionContentReport.Build(r);
            Assert.AreEqual("FAIL", data.Verdict, "源 audit 有失败类别时报告必须同 verdict（不复制真相守卫：verdict 只继承 audit）");
            Assert.AreEqual(1, data.AuditFailureCount);
            Assert.IsTrue(data.AuditCompleted);
            Assert.IsNull(data.AuditExecutionError);
            Assert.AreEqual(7, data.SupportCount);
            Assert.AreEqual(13, data.AffixCount);
        }
    }
}
