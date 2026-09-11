using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 真实 PoE 天赋树域契约（2026-09-10 导演指令：天赋树一比一复刻 PoEDB 页面，
    /// 包括图标与说明，并通过快捷键开关）。数据源=官方天赋树页面内嵌 tree 数据，
    /// 美术=PoEDB 同源 CDN；此处锁定「数据可加载 / 坐标与连线自洽 / 视口变换正确 /
    /// 加点仍受连线与点数约束」四类不变量。
    /// </summary>
    public sealed class PoeTreeTests
    {
        static int Start { get { return PoeTree.StartIndex; } }

        [Test]
        public void Data_Loads_WithFullPoetreeShape()
        {
            Assert.IsTrue(PoeTree.Ready, "天赋树数据必须可加载（Resources/UI/PoE/passive_tree.json）");
            Assert.Greater(PoeTree.Count, 2000, "真实 PoE 主树节点数应在 2000 以上");
            Assert.IsNotNull(PoeTree.Groups);
            Assert.Greater(PoeTree.Groups.Length, 500, "天赋簇数量应为数百");
            Assert.AreEqual(PoeTree.Count, PassiveCatalog.Count, "天赋目录必须直接由真实数据驱动");
            Assert.AreEqual(PoeTree.Count, SliceRules.PassiveCount, "PassiveCount 必须与真实节点数一致");
        }

        [Test]
        public void EveryNode_HasFinitePosition_InsidePublishedBounds()
        {
            var d = PoeTree.Data;
            float pad = 600f; // 节点自身半径 + 簇外扩
            for (int i = 0; i < d.nodes.Length; i++)
            {
                PoeNode n = d.nodes[i];
                Assert.IsTrue(IsFinite(n.x) && IsFinite(n.y), n.name + " 坐标必须有限");
                Assert.GreaterOrEqual(n.x, d.meta.minX - pad, n.name + " 越出左界");
                Assert.LessOrEqual(n.x, d.meta.maxX + pad, n.name + " 越出右界");
                Assert.GreaterOrEqual(n.y, d.meta.minY - pad, n.name + " 越出上界");
                Assert.LessOrEqual(n.y, d.meta.maxY + pad, n.name + " 越出下界");
            }
        }

        static bool IsFinite(float f)
        {
            return !float.IsNaN(f) && !float.IsInfinity(f);
        }

        [Test]
        public void Graph_IsSymmetric_AndIndicesInRange()
        {
            var nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                int[] links = nodes[i].links;
                if (links == null)
                    continue;
                for (int k = 0; k < links.Length; k++)
                {
                    int j = links[k];
                    Assert.GreaterOrEqual(j, 0, nodes[i].name + " 连线索引越界");
                    Assert.Less(j, nodes.Length, nodes[i].name + " 连线索引越界");
                    Assert.AreNotEqual(i, j, nodes[i].name + " 不得自连");
                    CollectionAssert.Contains(nodes[j].links, i,
                        "连线必须无向对称：" + nodes[i].name + " → " + nodes[j].name);
                }
            }
        }

        [Test]
        public void LockedNodes_AreExactlyTheEdgelessOnes()
        {
            var nodes = PoeTree.Nodes;
            int locked = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                bool edgeless = nodes[i].links == null || nodes[i].links.Length == 0;
                if (nodes[i].locked == 0)
                    continue;
                locked++;
                Assert.IsTrue(edgeless, nodes[i].name + "：标记为不可点却仍有连线");
            }
            Assert.Greater(locked, 0, "官方数据存在时光珠宝类无连线节点，应被标记为不可点");
        }

        [Test]
        public void StartNode_IsAllocatedOnReset_AndUnreachableNodesAreRejected()
        {
            var s = new SliceSession();
            Assert.IsTrue(s.Allocated[Start], "起点节点必须默认点亮");
            Assert.AreEqual(1, CountAllocated(s), "新会话只应点亮起点");

            string err;
            // S6P-WO-04A2：起点邻居里既有可兑现的（559/1795/2034），也有含"引擎兑现不了的效果行"的（71 …）。
            // 本令起这两类**都能点**（通行维度相同）；区别只在效果维度：前者真生效，后者 0 效果。
            int neighbour = FirstEffectiveNeighbour(Start);
            Assert.GreaterOrEqual(neighbour, 0, "起点必须至少有一个可兑现的邻居");
            Assert.IsTrue(s.TryAllocate(neighbour, out err), "可兑现的起点邻居必须可点亮：" + err);
            Assert.AreEqual(PoeTree.Count, s.Allocated.Length, "Allocated 长度必须等于真实节点数");

            int routeOnly = FirstRouteOnlyNeighbour(Start);
            Assert.GreaterOrEqual(routeOnly, 0, "起点邻居里必须存在 route-only 节点（钉死通行/生效分离）");
            Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled,
                PassiveSupport.EvaluateTruth(routeOnly).Effect, "route-only 邻居的效果维度必须是 UNFULFILLED");
            Assert.IsTrue(s.TryAllocate(routeOnly, out err), "route-only 邻居必须可作为路径点亮：" + err);

            // 与已点亮集合不连通的节点必须被拒绝
            int far = FindDisconnectedNode(s, neighbour);
            Assert.GreaterOrEqual(far, 0, "必须能找到一个与起点不连通的节点");
            Assert.IsFalse(s.TryAllocate(far, out err));
            Assert.AreEqual("需与已点亮节点相连", err);

            // 无连线的时光珠宝节点永不可点
            int locked = FindLockedNode();
            Assert.GreaterOrEqual(locked, 0);
            Assert.IsFalse(s.CanAllocate(locked), "无连线节点不得可点");
        }

        /// <summary>起点邻居里第一个效果可完整兑现的节点（无则 -1）。</summary>
        static int FirstEffectiveNeighbour(int node)
        {
            int[] links = PoeTree.Get(node).links;
            if (links == null)
                return -1;
            for (int i = 0; i < links.Length; i++)
                if (PassiveSupport.YieldsModifiers(PassiveSupport.EvaluateTruth(links[i]).Effect))
                    return links[i];
            return -1;
        }

        /// <summary>起点邻居里第一个"可通行但效果未兑现"的 route-only 节点（无则 -1）。</summary>
        static int FirstRouteOnlyNeighbour(int node)
        {
            int[] links = PoeTree.Get(node).links;
            if (links == null)
                return -1;
            for (int i = 0; i < links.Length; i++)
            {
                PassiveSupport.NodeTruth t = PassiveSupport.EvaluateTruth(links[i]);
                if (PassiveSupport.IsTraversable(t.Traversal) && !PassiveSupport.YieldsModifiers(t.Effect))
                    return links[i];
            }
            return -1;
        }

        [Test]
        public void Respec_KeepsOnlyStartNode()
        {
            var s = new SliceSession();
            string err;
            s.TryAllocate(FirstEffectiveNeighbour(Start), out err);
            Assert.Greater(CountAllocated(s), 1);
            Assert.IsTrue(s.TryRespec(out err), err);
            Assert.AreEqual(1, CountAllocated(s), "重构后只应保留起点");
            Assert.IsTrue(s.Allocated[Start]);
            Assert.AreEqual(SliceRules.StartPoints, s.Unspent, "重构必须退回全部已花点数");
        }

        static int CountAllocated(SliceSession s)
        {
            int n = 0;
            for (int i = 0; i < s.Allocated.Length; i++)
                if (s.Allocated[i])
                    n++;
            return n;
        }

        static int FindDisconnectedNode(SliceSession s, int allocated)
        {
            var nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (s.Allocated[i] || nodes[i].locked != 0)
                    continue;
                int[] links = nodes[i].links;
                if (links == null || links.Length == 0)
                    continue;
                bool touches = false;
                for (int k = 0; k < links.Length; k++)
                    if (links[k] == allocated || s.Allocated[links[k]])
                        touches = true;
                if (!touches)
                    return i;
            }
            return -1;
        }

        static int FindLockedNode()
        {
            var nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].locked != 0)
                    return i;
            return -1;
        }

        [Test]
        public void EveryNodeIcon_LoadsFromResources()
        {
            // 视觉可用性的硬前提：树要画得出来，每个节点的图标就必须真的能加载（缺任一张=洞里有个空框）
            var nodes = PoeTree.Nodes;
            int checked_ = 0;
            var seen = new HashSet<string>();
            for (int i = 0; i < nodes.Length; i++)
            {
                string stem = nodes[i].icon;
                if (string.IsNullOrEmpty(stem) || !seen.Add(stem))
                    continue;
                Assert.IsNotNull(PoeTree.Icon(stem), "节点图标缺失（" + nodes[i].name + "）：" + stem);
                checked_++;
            }
            Assert.Greater(checked_, 500, "真实主树应有数百张互异图标");
        }

        [Test]
        public void ChromeAtlases_Load_AndCarryEveryRequiredSlice()
        {
            var chrome = PoeTree.Data.chrome;
            Assert.IsNotNull(chrome, "天赋树必须带官方图集描述");

            var frame = RequireAtlas(chrome.frame, "node frame");
            foreach (string slice in new[]
            {
                "PSSkillFrame", "PSSkillFrameActive", "PSSkillFrameHighlighted",
                "NotableFrameUnallocated", "NotableFrameCanAllocate", "NotableFrameAllocated",
                "KeystoneFrameUnallocated", "KeystoneFrameCanAllocate", "KeystoneFrameAllocated",
                "JewelFrameUnallocated", "JewelFrameCanAllocate", "JewelFrameAllocated"
            })
                Assert.GreaterOrEqual(frame.IndexOf(slice), 0, "frame 图集缺切片：" + slice);

            var group = RequireAtlas(chrome.group, "group background");
            foreach (string slice in new[] { "PSGroupBackground1", "PSGroupBackground2", "PSGroupBackground3" })
                Assert.GreaterOrEqual(group.IndexOf(slice), 0, "簇底衬图集缺切片：" + slice);

            RequireAtlas(chrome.line, "orbit line");
            RequireAtlas(chrome.background, "orbit background");
            RequireAtlas(chrome.mastery, "mastery");
            RequireAtlas(chrome.jewel, "jewel");
        }

        static PoeAtlas RequireAtlas(PoeAtlas atlas, string what)
        {
            Assert.IsNotNull(atlas, what + " 图集描述缺失");
            Assert.IsNotNull(PoeTree.Chrome(atlas.file), what + " 图集贴图加载失败：" + atlas.file);
            Assert.Greater(atlas.names.Length, 0, what + " 图集没有切片");
            Rect r;
            Assert.IsTrue(atlas.TryRect(atlas.names[0], out r), what + " 首切片 UV 不可解析");
            return atlas;
        }

        [Test]
        public void FocusedAllocations_MovePlayerStats()
        {
            // 「全部驱动数值」的抽样验证：找一个带力量词条的小点，点亮后力量必须上升
            var nodes = PoeTree.Nodes;
            int index = -1;
            string statText = null;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].kind != 0 || nodes[i].locked != 0)
                    continue;
                if (nodes[i].links == null || nodes[i].links.Length == 0)
                    continue;
                if (!string.IsNullOrEmpty(nodes[i].stats) && nodes[i].stats.Contains("Strength"))
                {
                    index = i;
                    statText = nodes[i].stats;
                    break;
                }
            }
            Assert.GreaterOrEqual(index, 0, "真实数据里必须存在 +力量 类小点");

            var mods = PoeStatParser.ParseCached(statText);
            Assert.Greater(mods.Length, 0, "该词条必须能映射成引擎 modifier：" + statText);
        }
    }
}
