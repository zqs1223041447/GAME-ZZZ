using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-01 门禁：被动域 census 与覆盖审计。
    /// 这些断言是后续所有被动工作的**分母**——它们红了，说明"改天赋会不会被门禁发现"这件事本身就不成立。
    /// 本套件不改 runtime 行为，ProdSim 哈希必须保持 S5 基线不变。
    /// </summary>
    public sealed class PassiveCensusTests
    {
        static PassiveCensus.Data _cached;

        static PassiveCensus.Data Census()
        {
            if (_cached == null)
                _cached = PassiveCensus.Collect();
            return _cached;
        }

        // ---------- 1. 节点集合对账 ----------

        [Test]
        public void NodeSets_ReconcileExactly_ToDirectorStatedCount()
        {
            var d = Census();

            // 官方数据四个集合必须自洽：总数 = 升华 + 非升华；非升华 = 上树 + 未上树。
            Assert.AreEqual(PassiveCensus.SourceTotalNodes, d.SourceAscendancy + d.SourceUnplottedNonAscendancy + d.Plotted,
                "3390 必须 = 升华 558 + 未上树 403 + 上树 2429");
            Assert.AreEqual(PassiveCensus.DirectorStatedNodeCount, d.Plotted + d.SourceUnplottedNonAscendancy,
                "导演口径 2832 必须 = 上树 2429 + 未上树 403（非升华节点总数）");
            Assert.AreEqual(2429, d.Plotted, "上树节点数");
            Assert.AreEqual(797, d.Groups, "簇数");
        }

        [Test]
        public void NodeSets_KindBreakdown_IsStable()
        {
            var d = Census();
            Assert.AreEqual(1518, d.KindNormal, "普通节点");
            Assert.AreEqual(483, d.KindNotable, "显著节点");
            Assert.AreEqual(49, d.KindKeystone, "基石节点");
            Assert.AreEqual(315, d.KindMastery, "专精节点");
            Assert.AreEqual(57, d.KindJewel, "珠宝孔");
            Assert.AreEqual(7, d.KindStart, "职业起点");
            Assert.AreEqual(d.Plotted, d.KindNormal + d.KindNotable + d.KindKeystone + d.KindMastery + d.KindJewel + d.KindStart,
                "分类合计必须等于上树节点数");
            Assert.AreEqual(30, d.Locked, "无连线的时光珠宝类显著点");
            Assert.AreEqual(315, d.NodesWithChoices, "带可选效果的节点数（专精）必须与专精数一致");
        }

        [Test]
        public void NodeSets_StatsTextCoverage_IsPartitioned()
        {
            var d = Census();
            Assert.AreEqual(d.Plotted, d.NodesWithStatsText + d.NodesWithoutStatsText,
                "有词条文本 / 无词条文本 必须构成划分");
            Assert.Greater(d.NodesWithStatsText, 0, "主树必须有带词条的节点");
        }

        // ---------- 2. 效果行四分类 ----------

        [Test]
        public void EffectLines_NoUnknown()
        {
            var d = Census();
            if (d.LinesUnknown > 0)
            {
                var sb = new StringBuilder("UNKNOWN 效果行必须为 0，未覆盖样本：\n");
                for (int i = 0; i < d.UnknownSamples.Count && i < 40; i++)
                    sb.Append("  · ").Append(d.UnknownSamples[i]).Append('\n');
                Assert.Fail(sb.ToString());
            }
        }

        [Test]
        public void EffectLines_FourWayPartitionIsComplete()
        {
            var d = Census();
            Assert.AreEqual(d.LinesTotal,
                d.LinesConsumed + d.LinesBlockedByDomain + d.LinesSpecialInteraction + d.LinesStructural + d.LinesUnknown,
                "四分类（+UNKNOWN）必须恰好划分全部效果行");
            Assert.Greater(d.LinesTotal, 0);
            Assert.Greater(d.LinesConsumed, 0, "必须存在被引擎真实消费的效果行");
            Assert.Greater(d.LinesStructural, 0, "括号提示文本应落入 STRUCTURAL");
        }

        [Test]
        public void EffectLines_ConsumedCount_MatchesParserItself()
        {
            // 单一真值交叉验：census 的 CONSUMED 计数必须等于 PoeStatParser 自己逐节点统计的映射行数。
            PoeNode[] nodes = PoeTree.Nodes;
            int parserMapped = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                parserMapped += PoeStatParser.MappedLineCount(nodes[i].stats);
                parserMapped += PoeStatParser.MappedLineCount(nodes[i].choices);
            }

            var d = Census();
            Assert.AreEqual(parserMapped, d.LinesConsumed,
                "census 的 CONSUMED 行数必须与 PoeStatParser.MappedLineCount 汇总一致（不得出现第二套判定）");
        }

        [Test]
        public void EffectLines_EveryBlockedDomain_IsNamed()
        {
            var d = Census();
            Assert.AreEqual(d.Domains.Count, d.DomainCounts.Count, "域清单与计数必须一一对应");
            for (int i = 0; i < d.Domains.Count; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(d.Domains[i]), "域标签不得为空");
                Assert.Greater(d.DomainCounts[i], 0, "域 " + d.Domains[i] + " 的计数必须为正");
            }
        }

        [Test]
        public void NodeEligibility_PartitionsAllNodes()
        {
            var d = Census();
            Assert.AreEqual(d.Plotted,
                d.NodesSupported + d.NodesSpecial + d.NodesUnsupportedCurrently + d.NodesMasteryPending,
                "节点级资格必须恰好划分全部上树节点（S6P-WO-04A 起含专精过渡态）");
            // S6P-WO-04A 的产品真相快照（支持门 / 消费门生效后的真实分布；变红=被动可分配性被改动）
            Assert.AreEqual(367, d.NodesSupported, "ALLOCATABLE_SUPPORTED");
            Assert.AreEqual(1660, d.NodesUnsupportedCurrently, "BLOCKED_CURRENTLY（含真实混合节点）");
            Assert.AreEqual(87, d.NodesSpecial, "BLOCKED_SPECIAL_INTERACTION（57 珠宝孔 + 30 时光珠宝类无连线节点）");
            Assert.AreEqual(315, d.NodesMasteryPending, "SPECIAL_PENDING_MASTERY（WO-03 前专精一律不可分配）");
        }

        // ---------- 3. ProdSim 面审计 ----------

        [Test]
        public void ProdSimSurface_HashPayloadIsPassiveSensitive()
        {
            // S6P-WO-01 的结论是 NOT_PASSIVE_SENSITIVE（历史）。WO-02 把 canonical 被动状态喂进哈希后，
            // 本断言翻转——翻转是刻意的：不翻转就说明 WO-02 没做到。
            var d = Census();
            Assert.AreEqual("PASSIVE_AND_MASTERY_SENSITIVE", d.ProdSimVerdict,
                "审计结论：ProdSim 的确定性哈希必须能看到被动 + 专精选择真相（S6P-WO-03）");
            Assert.IsTrue(d.ProdSimHashPayloadMentionsPassive, "哈希喂点必须出现 passive/allocated/stat/modifier 输入");
            Assert.IsTrue(d.ProdSimReadsAllocatedState, "ProductionSimulator 必须经被动负载读到已分配节点身份");
            Assert.IsTrue(d.ProdSimHashDependsOnPassiveResult, "哈希必须依赖 canonical 被动状态负载");
            Assert.IsTrue(d.ProdSimMasterySensitive, "WO-03 起专精选择 identity 必须进入哈希（px|）");
            Assert.AreEqual(0, d.ProdSimForbiddenTokensFound.Count,
                "哈希喂点不得含坐标/图标/贴图/tooltip/zoom/时间戳等非游戏真相：" + string.Join(",", d.ProdSimForbiddenTokensFound.ToArray()));
            Assert.Greater(d.ProdSimHashInputSites.Count, 0, "必须抓到哈希喂点作为证据");
        }

        // ---------- 4. 分配不变量覆盖 ----------

        [Test]
        public void AllocationInvariants_AreEnumeratedAndObservable()
        {
            var d = Census();
            Assert.AreEqual(7, d.InvariantNames.Count, "七条分配不变量必须逐条列出");
            Assert.AreEqual(d.InvariantNames.Count, d.InvariantCovered.Count);

            var byName = new Dictionary<string, bool>();
            for (int i = 0; i < d.InvariantNames.Count; i++)
                byName[d.InvariantNames[i]] = d.InvariantCovered[i];

            // 这六条当前已由运行时行为保证，必须为真（红了=被动加点语义被改坏）
            Assert.IsTrue(byName["ValidStart"], "起点必须恒为已点亮");
            Assert.IsTrue(byName["ConnectedAllocation"], "相连节点必须可点亮");
            Assert.IsTrue(byName["NoIllegalJump"], "不相连的节点必须被拒绝");
            Assert.IsTrue(byName["PointAccounting"], "被拒绝的加点不得扣点");
            Assert.IsTrue(byName["DuplicateAllocationNoop"], "重复加点不得二次扣点");
            Assert.IsTrue(byName["ResetExact"], "R 重构必须精确清空并归还点数");

            // 专精前置/选择当前**不存在**——这是已知缺口，登记给 S6P-WO-03。
            Assert.IsFalse(byName["MasteryPrerequisite"],
                "专精选择规则尚未建立；WO-03 建立后本断言需同步翻转为 true");
        }

        // ---------- 5. 产物 ----------

        [Test]
        public void Report_RenderIsDeterministic()
        {
            var first = PassiveCensus.Render(Census());
            var second = PassiveCensus.Render(PassiveCensus.Collect());
            Assert.AreEqual(first, second, "同一仓库状态两次渲染必须 byte 级一致（禁时间戳/GUID/绝对路径）");
        }

        [Test]
        public void Report_IsWrittenToQaArtifact()
        {
            var d = Census();
            PassiveCensus.Write(d);
            Assert.IsTrue(File.Exists(PassiveCensus.ArtifactPath), "census 产物必须落盘：" + PassiveCensus.ArtifactPath);
            string text = File.ReadAllText(PassiveCensus.ArtifactPath);
            StringAssert.Contains(PassiveCensus.SchemaName, text);
            StringAssert.Contains("\"unknown\": 0", text);
        }
    }
}
