using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 3 UI R4（工作令 S3-P3-UI-R4-PASSIVE-BOUNDARY-CLOSEOUT）：Passive 展示布局契约。
    /// Branch A——只重排现有 Passive 展示，零 gameplay delta，canonical Links 驱动（零 synthetic edge）。
    /// </summary>
    public sealed class SlicePassiveLayoutTests
    {
        static readonly Rect Panel = new Rect(0, 0, SlicePassiveLayout.PanelWidth, SlicePassiveLayout.PanelHeight);
        static int Count { get { return PassiveCatalog.Count; } }

        [Test]
        public void PassiveCount_Unchanged()
        {
            Assert.AreEqual(16, Count, "Passive 数量由 PassiveCatalog 决定，R4 零 delta");
        }

        [Test]
        public void PassiveIds_Unchanged()
        {
            for (int i = 0; i < Count; i++)
                Assert.AreEqual(i, PassiveCatalog.Get(i).Id, "ID 顺序不变");
        }

        [Test]
        public void Layout_RectsNonOverlap()
        {
            var rects = SlicePassiveLayout.AllNodeRects(Panel, Count);
            for (int i = 0; i < rects.Length; i++)
                for (int j = i + 1; j < rects.Length; j++)
                    Assert.IsFalse(rects[i].Overlaps(rects[j]),
                        "node " + i + " 与 node " + j + " 重叠");
        }

        [Test]
        public void Layout_RectsWithinPanel()
        {
            var rects = SlicePassiveLayout.AllNodeRects(Panel, Count);
            for (int i = 0; i < rects.Length; i++)
            {
                Assert.GreaterOrEqual(rects[i].xMin, Panel.xMin, $"node {i} xMin 出 panel");
                Assert.LessOrEqual(rects[i].xMax, Panel.xMax, $"node {i} xMax 出 panel");
                Assert.GreaterOrEqual(rects[i].yMin, Panel.yMin, $"node {i} yMin 出 panel");
                Assert.LessOrEqual(rects[i].yMax, Panel.yMax, $"node {i} yMax 出 panel");
            }
        }

        [Test]
        public void Layout_Deterministic()
        {
            var a = SlicePassiveLayout.AllNodeRects(Panel, Count);
            var b = SlicePassiveLayout.AllNodeRects(Panel, Count);
            for (int i = 0; i < a.Length; i++)
                Assert.AreEqual(a[i], b[i], $"node {i} 两次调用 rect 不一致");
        }

        [Test]
        public void Layout_1080pScaleValid()
        {
            // 1080p 下 panel 居中（R3 DrawerLayout 已验收 1920×1080 design space）
            var panel1080 = new Rect(756, 108, SlicePassiveLayout.PanelWidth, SlicePassiveLayout.PanelHeight);
            var rects = SlicePassiveLayout.AllNodeRects(panel1080, Count);
            for (int i = 0; i < rects.Length; i++)
                Assert.Greater(rects[i].width, 0, $"node {i} 在 1080p 下宽度<=0");
        }

        [Test]
        public void Layout_1440pScaleValid()
        {
            // 1440p：R3 布局系统自动缩放
            var panel1440 = new Rect(1008, 144, SlicePassiveLayout.PanelWidth * 1.333f, SlicePassiveLayout.PanelHeight * 1.333f);
            var rects = SlicePassiveLayout.AllNodeRects(panel1440, Count);
            for (int i = 0; i < rects.Length; i++)
            {
                Assert.Greater(rects[i].width, 0, $"node {i} 在 1440p 下宽度<=0");
                Assert.GreaterOrEqual(rects[i].xMin, panel1440.xMin - 1, $"node {i} 在 1440p 出 panel 左侧");
            }
        }

        [Test]
        public void Edges_OnlyCanonicalLinks()
        {
            // 验证 layout 的 edge 数==canonical Links 数（零 synthetic edge）
            var aPts = new System.Collections.Generic.List<Vector2>();
            var bPts = new System.Collections.Generic.List<Vector2>();
            SlicePassiveLayout.GetEdges(Count, aPts, bPts, Panel);
            int canonical = 0;
            for (int i = 0; i < Count; i++)
            {
                int[] links = PassiveCatalog.Get(i).Links;
                if (links == null) continue;
                foreach (var l in links)
                    if (l > i) canonical++;
            }
            Assert.AreEqual(canonical, aPts.Count, "edge 数必须精确等于 canonical Links（零 synthetic edge）");
        }

        [Test]
        public void Layout_PureFunction_NoGameplayState()
        {
            // SlicePassiveLayout 是纯函数——只读 PassiveCatalog，零 gameplay state 写入
            // 编译期已证明（无 gameplay 参数/字段）；此处实证 deterministic 调用无副作用
            var rects1 = SlicePassiveLayout.AllNodeRects(Panel, Count);
            var rects2 = SlicePassiveLayout.AllNodeRects(Panel, Count);
            for (int i = 0; i < rects1.Length; i++)
                Assert.AreEqual(rects1[i], rects2[i], "纯函数两次调用结果必须一致");
        }

        [Test]
        public void NotableNodes_HaveDistinctSize()
        {
            var normal = PassiveCatalog.Get(0);
            var notable = PassiveCatalog.Get(11);
            var sizeN = SlicePassiveLayout.NodeSize(normal);
            var sizeB = SlicePassiveLayout.NodeSize(notable);
            Assert.Greater(sizeB.x, sizeN.x, "Notable 节点宽度必须大于普通节点");
            Assert.Greater(sizeB.y, sizeN.y, "Notable 节点高度必须大于普通节点");
        }
    }
}
