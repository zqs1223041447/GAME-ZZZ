using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 天赋树视口几何契约（纯函数）：世界↔屏幕变换、节点尺寸随缩放、锚点缩放不漂移、
    /// 整树适配居中、视口裁剪。2429 节点必须裁剪，否则每帧绘制不可控。
    /// </summary>
    public sealed class PoeTreeViewTests
    {
        static readonly Rect View1080 = new Rect(1920f / 2 - 600f, 46f, 1200f, 700f);

        [Test]
        public void ScreenAndWorld_RoundTrip()
        {
            var pan = new Vector2(37f, -11f);
            const float zoom = 0.42f;
            var world = new Vector2(-1234.5f, 678.25f);
            Vector2 screen = PoeTreeView.ScreenOf(world, pan, zoom);
            Vector2 back = PoeTreeView.WorldOf(screen, pan, zoom);
            Assert.AreEqual(world.x, back.x, 0.01f);
            Assert.AreEqual(world.y, back.y, 0.01f);
        }

        [Test]
        public void NodeRect_IsCentredOnNode_AndScalesWithZoom()
        {
            var node = PoeTree.Get(PoeTree.StartIndex);
            var pan = new Vector2(100f, 200f);
            Rect a = PoeTreeView.NodeRect(node, pan, 1f);
            Rect b = PoeTreeView.NodeRect(node, pan, 0.5f);
            Assert.AreEqual(a.width * 0.5f, b.width, 0.01f, "节点框必须随缩放等比");
            Vector2 centre = PoeTreeView.ScreenOf(new Vector2(node.x, node.y), pan, 1f);
            Assert.AreEqual(centre.x, a.x + a.width * 0.5f, 0.01f);
            Assert.AreEqual(centre.y, a.y + a.height * 0.5f, 0.01f);
        }

        [Test]
        public void NodeSize_OrdersByKind()
        {
            Assert.Less(PoeTreeView.NodeSize((int)PoeNodeKind.Normal), PoeTreeView.NodeSize((int)PoeNodeKind.Notable));
            Assert.Less(PoeTreeView.NodeSize((int)PoeNodeKind.Notable), PoeTreeView.NodeSize((int)PoeNodeKind.Keystone));
        }

        [Test]
        public void ZoomAround_KeepsAnchorWorldPointFixed()
        {
            var pan = new Vector2(300f, 90f);
            const float oldZoom = 0.5f;
            const float newZoom = 1.1f;
            var anchor = new Vector2(700f, 400f);
            Vector2 worldBefore = PoeTreeView.WorldOf(anchor, pan, oldZoom);
            Vector2 newPan = PoeTreeView.ZoomAround(pan, oldZoom, newZoom, anchor);
            Vector2 worldAfter = PoeTreeView.WorldOf(anchor, newPan, newZoom);
            Assert.AreEqual(worldBefore.x, worldAfter.x, 0.01f, "锚点缩放不得漂移");
            Assert.AreEqual(worldBefore.y, worldAfter.y, 0.01f, "锚点缩放不得漂移");
        }

        [Test]
        public void FitPan_CentresWholeTree()
        {
            float zoom = 0.1f;
            Vector2 pan = PoeTreeView.FitPan(View1080, zoom);
            Rect bounds = PoeTreeView.WorldBounds();
            Vector2 treeCentre = new Vector2(bounds.x + bounds.width * 0.5f, bounds.y + bounds.height * 0.5f);
            Vector2 onScreen = PoeTreeView.ScreenOf(treeCentre, pan, zoom);
            Assert.AreEqual(View1080.x + View1080.width * 0.5f, onScreen.x, 0.5f);
            Assert.AreEqual(View1080.y + View1080.height * 0.5f, onScreen.y, 0.5f);
        }

        [Test]
        public void FitZoom_IsClampedAndFitsInsideViewport()
        {
            float zoom = PoeTreeView.FitZoom(View1080, 40f);
            Assert.GreaterOrEqual(zoom, PoeTreeView.MinZoom);
            Assert.LessOrEqual(zoom, PoeTreeView.MaxZoom);
            Rect bounds = PoeTreeView.WorldBounds();
            Assert.LessOrEqual(bounds.width * zoom, View1080.width + 1f, "整树宽度必须放进视口");
            Assert.LessOrEqual(bounds.height * zoom, View1080.height + 1f, "整树高度必须放进视口");
        }

        [Test]
        public void Visible_CullsOffscreenNodes()
        {
            var node = PoeTree.Get(PoeTree.StartIndex);
            // 把该节点放到视口正中的平移
            var pan = new Vector2(View1080.x + View1080.width * 0.5f - node.x, View1080.y + View1080.height * 0.5f - node.y);
            Assert.IsTrue(PoeTreeView.Visible(node, pan, 1f, View1080, 8f), "正中节点必须可见");
            var far = new Vector2(pan.x + 100000f, pan.y);
            Assert.IsFalse(PoeTreeView.Visible(node, far, 1f, View1080, 8f), "远推出去的节点必须被裁掉");
        }

        [Test]
        public void ViewportOrigin_IsTheOnlyOffsetBetweenLocalAndDesignSpace()
        {
            // 天赋树在 GUI.BeginGroup(viewport) 内绘制：节点矩形是「局部」坐标，
            // 命中判定必须用「指针 − 视口原点」——本测试钉死这个唯一偏移量。
            Rect view = SliceHud.PoeTreeViewport(1920f, 1080f);
            Assert.AreEqual(0f, view.x, 0.01f, "视口横向从 0 开始（全宽）");
            Assert.Greater(view.y, 0f, "视口顶部让出标题条");
            Assert.Greater(view.height, 0f);
            Assert.LessOrEqual(view.yMax, 1080f, "视口不得越出屏幕（底栏保持可见）");

            // 把起点节点摆到视口正中：局部矩形中心 + 视口原点 == 设计空间视口中心
            var node = PoeTree.Get(PoeTree.StartIndex);
            const float zoom = 0.5f;
            var pan = new Vector2(view.width * 0.5f - node.x * zoom, view.height * 0.5f - node.y * zoom);
            Rect local = PoeTreeView.NodeRect(node, pan, zoom);
            Vector2 designCentre = new Vector2(view.x + local.x + local.width * 0.5f,
                view.y + local.y + local.height * 0.5f);
            Assert.AreEqual(view.x + view.width * 0.5f, designCentre.x, 0.01f);
            Assert.AreEqual(view.y + view.height * 0.5f, designCentre.y, 0.01f);
        }

        static int CountVisible(Rect view, float zoom, Vector2 pan)
        {
            int visible = 0;
            var nodes = PoeTree.Nodes;
            for (int i = 0; i < nodes.Length; i++)
                if (PoeTreeView.Visible(nodes[i], pan, zoom, view, 12f))
                    visible++;
            return visible;
        }

        [Test]
        public void FitView_ShowsWholeTree_ZoomedIn_IsCulled()
        {
            Rect view = new Rect(0f, 0f, 1920f, 700f);
            float fit = PoeTreeView.FitZoom(view, 40f);
            Assert.AreEqual(PoeTree.Count, CountVisible(view, fit, PoeTreeView.FitPan(view, fit)),
                "整树适配视图必须把全部节点纳入视口");

            // 放大到 1:1 时，可视节点数必须被裁到极小（否则每帧 2429 次绘制不可接受）
            Vector2 zoomedPan = PoeTreeView.FitPan(view, 1f);
            int visible = CountVisible(view, 1f, zoomedPan);
            Assert.Greater(visible, 0, "放大后仍应看得到节点");
            Assert.Less(visible, PoeTree.Count / 4, "放大视图必须真的裁剪（可视节点应远少于总数）");
        }
    }
}
