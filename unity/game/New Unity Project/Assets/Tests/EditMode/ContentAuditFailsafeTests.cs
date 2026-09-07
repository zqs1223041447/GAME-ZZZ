using System;
using System.IO;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Failure-safe 审计契约测试（S3-M3）：全部使用 synthetic ContentAuditResult + 测试临时路径——
    /// 不修改正式 Catalog，不触碰正式 CLOSEOUT 报告（正式报告只由正式 current-state Audit 写）。
    /// 锁定契约：渲染确定性 / Synthetic PASS-FAIL 渲染正确 / Finalize 先写盘后断言 /
    /// 基础设施异常先落 FAIL 快照再上抛 / 写盘失败必须红（不吞异常）。
    /// </summary>
    public sealed class ContentAuditFailsafeTests
    {
        static readonly string TempDir = Path.Combine(Path.GetTempPath(), "gamezzz_audit_failsafe_tests");

        [SetUp]
        public void CleanTemp()
        {
            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);
            Directory.CreateDirectory(TempDir);
        }

        [TearDown]
        public void RemoveTemp()
        {
            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);
        }

        static ContentAuditResult PassResult()
        {
            var r = new ContentAuditResult { Completed = true };
            r.ActiveSkillCount = 3;
            r.SupportCount = 7;
            r.AffixCount = 13;
            r.PassiveCount = 16;
            r.MapAffixCount = 3;
            r.EnemyCount = 5;
            r.ModRefCount = 20;
            r.NewAffixCount = 3;
            r.TaggedModCount = 4;
            r.ReachableTaggedMods = 4;
            r.RequiredPassed = 6;
            r.RequiredTotal = 6;
            r.GatedPresent = 0;
            r.GatedMissing = 3;
            return r;
        }

        static ContentAuditResult FailResult()
        {
            var r = PassResult();
            r.RequiredResourceMissing.Add("Test missing（synthetic key）：synthetic REQUIRED 缺失");
            r.StructuralProblems.Add("Test structural：synthetic 越界条目");
            return r;
        }

        [Test]
        public void Render_Deterministic_PassResult_TwoRendersIdentical()
        {
            string first = ContentAuditS2Tests.RenderReport(PassResult());
            string second = ContentAuditS2Tests.RenderReport(PassResult());
            Assert.AreEqual(first, second, "同一 PASS Result 两次渲染必须完全一致（byte 级确定性）");
        }

        [Test]
        public void Render_Deterministic_FailResult_TwoRendersIdentical()
        {
            string first = ContentAuditS2Tests.RenderReport(FailResult());
            string second = ContentAuditS2Tests.RenderReport(FailResult());
            Assert.AreEqual(first, second, "同一 FAIL Result 两次渲染必须完全一致（byte 级确定性）");
        }

        [Test]
        public void Render_SyntheticFail_ShowsFailVerdict_AndDetail_NotPass()
        {
            string text = ContentAuditS2Tests.RenderReport(FailResult());
            StringAssert.Contains("Verdict: FAIL", text);
            StringAssert.Contains("Failure count: 2", text);
            StringAssert.Contains("Audit completed: YES", text);
            StringAssert.Contains("Test missing", text, "失败明细必须进入报告，不能只写 FAIL 请看 Console");
            StringAssert.Contains("Test structural", text, "结构/契约失败明细必须进入报告");
            StringAssert.DoesNotContain("Verdict: PASS", text, "FAIL 报告不得出现 PASS");
        }

        [Test]
        public void Render_SyntheticPass_CompletedYes_VerdictPass_ZeroFailures()
        {
            string text = ContentAuditS2Tests.RenderReport(PassResult());
            StringAssert.Contains("Audit completed: YES", text);
            StringAssert.Contains("Verdict: PASS", text);
            StringAssert.Contains("Failure count: 0", text);
            StringAssert.DoesNotContain("Verdict: FAIL", text, "PASS 报告不得出现 FAIL");
        }

        [Test]
        public void Finalize_FailResult_WritesRedReport_BeforeAssertThrows()
        {
            var path = Path.Combine(TempDir, "fail_report.md");
            AssertionException thrown = null;
            try
            {
                ContentAuditFailsafe.FinalizeAudit(FailResult(), path, ContentAuditS2Tests.RenderReport);
            }
            catch (AssertionException ex)
            {
                thrown = ex;
            }
            Assert.IsNotNull(thrown, "FAIL result 必须在 Stage 4 抛断言失败");
            Assert.IsTrue(File.Exists(path), "断言抛出之前红报告必须已写盘（Persist 先于 Assert）");
            var text = File.ReadAllText(path);
            StringAssert.Contains("Verdict: FAIL", text);
            StringAssert.Contains("Test missing", text);
            StringAssert.DoesNotContain("Verdict: PASS", text);
        }

        [Test]
        public void Finalize_PassResult_WritesGreenReport_NoThrow()
        {
            var path = Path.Combine(TempDir, "pass_report.md");
            Assert.DoesNotThrow(() => ContentAuditFailsafe.FinalizeAudit(PassResult(), path, ContentAuditS2Tests.RenderReport));
            var text = File.ReadAllText(path);
            StringAssert.Contains("Audit completed: YES", text);
            StringAssert.Contains("Verdict: PASS", text);
        }

        [Test]
        public void Execute_CollectThrows_WritesFailSnapshot_ThenPropagates()
        {
            var path = Path.Combine(TempDir, "infra_snapshot.md");
            Func<ContentAuditResult> boom = () => { throw new NullReferenceException("synthetic infra boom"); };
            var caught = Assert.Catch<Exception>(() => ContentAuditFailsafe.ExecuteAudit(path, boom, ContentAuditS2Tests.RenderReport));
            Assert.IsFalse(caught is AssertionException, "基础设施异常不得伪装成内容断言失败");
            Assert.IsTrue(File.Exists(path), "审计基础设施炸掉时必须先落 FAIL 快照，不得遗留上一轮 PASS");
            var text = File.ReadAllText(path);
            StringAssert.Contains("Audit completed: NO", text);
            StringAssert.Contains("Verdict: FAIL", text);
            StringAssert.Contains("NullReferenceException", text);
            StringAssert.Contains("Execution error", text);
        }

        [Test]
        public void Execute_ReportWriteFails_TestReds_NoSwallow()
        {
            // 父级「目录」实际是一个文件 → 建目录/写文件必然失败，模拟写盘异常
            var blocker = Path.Combine(TempDir, "blocker.md");
            File.WriteAllText(blocker, "not a directory");
            var badPath = Path.Combine(blocker, "report.md");
            var caught = Assert.Catch<Exception>(() => ContentAuditFailsafe.ExecuteAudit(badPath, PassResult, ContentAuditS2Tests.RenderReport));
            Assert.IsFalse(caught is AssertionException, "写盘失败必须以真实异常上抛（测试红），不得吞异常");
            Assert.IsTrue(File.Exists(blocker), "原有文件不得被审计改动");
        }
    }
}
