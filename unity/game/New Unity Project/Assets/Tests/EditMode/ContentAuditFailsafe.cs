using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 内容审计结果对象（S3-M3 failure-safe）：全部失败类别 + 报告渲染所需数据。
    /// 单一真值入口：HasFailures / FailureCount。Completed=false（基础设施异常）时绝不 PASS。
    /// </summary>
    internal sealed class ContentAuditResult
    {
        /// <summary>Collect 是否完整执行（未被基础设施异常打断）。false 时 Verdict 必须 FAIL。</summary>
        public bool Completed;

        /// <summary>审计基础设施异常（NullReference/IO/编程错误），非内容违规。Completed=false 时非空。</summary>
        public string ExecutionError;

        // ---- 失败类别（每一类为空 = 该轴绿）----
        /// <summary>结构/契约：目录条目缺 Name、内容数量护栏、FireConversion 契约、Passive 图连通、技能/怪参数、MapAffix Id。</summary>
        public readonly List<string> StructuralProblems = new List<string>();
        /// <summary>StatId 越界 / 运行期未知 Stat / 非法 ModOp。</summary>
        public readonly List<string> MissingStat = new List<string>();
        /// <summary>未声明 Tag bit。</summary>
        public readonly List<string> IllegalTag = new List<string>();
        /// <summary>Effect/Event 引用非法。</summary>
        public readonly List<string> BadEffect = new List<string>();
        /// <summary>天赋链接：无链接/越界/单向。</summary>
        public readonly List<string> MissingLinks = new List<string>();
        /// <summary>词缀行非法（含运行期未知 Stat）。</summary>
        public readonly List<string> BadAffix = new List<string>();
        /// <summary>Support×技能兼容矩阵违规。</summary>
        public readonly List<string> CompatProblems = new List<string>();
        /// <summary>Skill Tag 规则/golden parity 违规。</summary>
        public readonly List<string> TagProblems = new List<string>();
        /// <summary>死 Tagged Modifier（Rule E）。</summary>
        public readonly List<string> DeadTagged = new List<string>();
        /// <summary>REQUIRED 资源缺失/路径异常。</summary>
        public readonly List<string> RequiredResourceMissing = new List<string>();

        // ---- 报告数据（非失败判据）----
        public readonly List<string> ResourceRows = new List<string>();
        public readonly List<string> TagProfileRows = new List<string>();
        public readonly List<string> ReachabilityRows = new List<string>();
        public readonly List<string> MatrixRows = new List<string>();
        public readonly List<string> R2SupportRows = new List<string>();
        public readonly List<string> NewAffixRows = new List<string>();
        /// <summary>已声明但当前内容未引用的 Tag（预留记录，不是失败项）。</summary>
        public readonly List<string> UnusedTags = new List<string>();
        /// <summary>内容引用的 Stat 全集（Support/Passive Mod + 词缀行收集；Production Report 汇总用，不参与失败判定）。</summary>
        public readonly HashSet<string> DeclaredStats = new HashSet<string>();

        public int ActiveSkillCount;
        public int SupportCount;
        public int AffixCount;
        public int PassiveCount;
        public int MapAffixCount;
        public int EnemyCount;
        public int ModRefCount;
        public int NewAffixCount;
        public int TaggedModCount;
        public int ReachableTaggedMods;
        public int RequiredPassed;
        public int RequiredTotal;
        public int GatedPresent;
        public int GatedMissing;

        public int FailureCount
        {
            get
            {
                int n = StructuralProblems.Count + MissingStat.Count + IllegalTag.Count + BadEffect.Count +
                        MissingLinks.Count + BadAffix.Count + CompatProblems.Count + TagProblems.Count +
                        DeadTagged.Count + RequiredResourceMissing.Count;
                if (!Completed)
                    n++;
                return n;
            }
        }

        public bool HasFailures { get { return FailureCount > 0; } }
    }

    /// <summary>
    /// Failure-safe 审计管线（S3-M3 工作令）：语义顺序 Collect → Render → Persist → Assert。
    /// Persist 必须先于 Assert：内容违规时红报告已落盘，仓库当前快照不留上一轮 PASS。
    /// - Stage 4 内容违规断言直接上抛 AssertionException（红报告已在盘上）；
    /// - 基础设施异常（Collect/Render/写盘前异常）：先尽力写 FAIL 快照再上抛；
    /// - 写盘本身失败：Debug.Log 记录目标路径与异常后原样上抛（测试红，不吞异常）。
    /// </summary>
    internal static class ContentAuditFailsafe
    {
        /// <summary>主入口。collect 不得含普通内容违规断言（只入 Result 集合）。</summary>
        public static void ExecuteAudit(string reportPath, Func<ContentAuditResult> collect, Func<ContentAuditResult, string> render)
        {
            ContentAuditResult result = null;
            try
            {
                result = collect();                                  // Stage 1
                result.Completed = true;
                FinalizeAudit(result, reportPath, render);           // Stage 2/3/4
            }
            catch (AssertionException)
            {
                throw; // 内容违规：Stage 3 已把红报告落盘
            }
            catch (Exception ex)
            {
                if (result == null)
                    result = new ContentAuditResult();
                result.Completed = false;
                result.ExecutionError = ex.GetType().Name + ": " + ex.Message;
                Debug.Log("[ContentAudit] 审计基础设施异常，先写 FAIL 快照再上抛（不吞异常）。intended report path: " +
                          reportPath + " exception: " + result.ExecutionError);
                File.WriteAllText(reportPath, render(result));       // 此写再失败则直接上抛 → 测试红
                throw;
            }
        }

        /// <summary>Stage 2+3+4：渲染 → 写盘 → 内容断言。断言只发生在报告已持久化之后。</summary>
        public static void FinalizeAudit(ContentAuditResult result, string reportPath, Func<ContentAuditResult, string> render)
        {
            string markdown = render(result);
            string dir = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(reportPath, markdown);
            Debug.Log("[ContentAudit] wrote " + reportPath);
            AssertAuditResult(result);
        }

        /// <summary>Stage 4：只基于已完成 Result 的内容断言；由报告落盘后调用。</summary>
        public static void AssertAuditResult(ContentAuditResult result)
        {
            Assert.IsTrue(result.Completed && string.IsNullOrEmpty(result.ExecutionError),
                "Audit 未完整执行，绝不能 PASS：" + (string.IsNullOrEmpty(result.ExecutionError) ? "Completed=false" : result.ExecutionError));
            Assert.IsEmpty(result.StructuralProblems, "结构/契约问题：" + string.Join("; ", result.StructuralProblems));
            Assert.IsEmpty(result.MissingStat, "StatId/ModOp 引用缺失或运行期未知：" + string.Join("; ", result.MissingStat));
            Assert.IsEmpty(result.IllegalTag, "非法 Tag：" + string.Join("; ", result.IllegalTag));
            Assert.IsEmpty(result.BadEffect, "Effect/Event 引用非法：" + string.Join("; ", result.BadEffect));
            Assert.IsEmpty(result.MissingLinks, "天赋链接缺失/单向：" + string.Join("; ", result.MissingLinks));
            Assert.IsEmpty(result.BadAffix, "词缀行非法：" + string.Join("; ", result.BadAffix));
            Assert.IsEmpty(result.CompatProblems, "Support×技能兼容矩阵：" + string.Join("; ", result.CompatProblems));
            Assert.IsEmpty(result.TagProblems, "Skill Tag 规则/golden parity：" + string.Join("; ", result.TagProblems));
            Assert.IsEmpty(result.DeadTagged, "死 Tagged Modifier：" + string.Join("; ", result.DeadTagged));
            Assert.IsEmpty(result.RequiredResourceMissing, "REQUIRED 资源缺失：" + string.Join("; ", result.RequiredResourceMissing));
        }
    }
}
