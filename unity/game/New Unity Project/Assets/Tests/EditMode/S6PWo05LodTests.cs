using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>S6P-WO-05 AC-05 / AC-06：LOD 按物理投影切档，边界精确。</summary>
    public sealed class S6PWo05LodTests
    {
        [Test]
        public void Boundaries_AreExact()
        {
            Assert.AreEqual(PassiveTreeLod.Level.Overview, PassiveTreeLod.FromProjectedPx(7.999f));
            Assert.AreEqual(PassiveTreeLod.Level.Mid, PassiveTreeLod.FromProjectedPx(8.000f));
            Assert.AreEqual(PassiveTreeLod.Level.Mid, PassiveTreeLod.FromProjectedPx(17.999f));
            Assert.AreEqual(PassiveTreeLod.Level.Detail, PassiveTreeLod.FromProjectedPx(18.000f));
        }

        [Test]
        public void SamePhysicalProjection_SameLod_On1080And1440()
        {
            float d1080 = SliceHud.DesignScale(1920f, 1080f);
            float d1440 = SliceHud.DesignScale(2560f, 1440f);
            Assert.AreEqual(1f, d1080, 0.0001f);
            Assert.AreEqual(2560f / 1920f, d1440, 0.0001f);

            float z1080 = PassiveTreeLod.ZoomForProjectedPx(12f, d1080);
            float z1440 = PassiveTreeLod.ZoomForProjectedPx(12f, d1440);
            Assert.AreEqual(PassiveTreeLod.Level.Mid, PassiveTreeLod.FromZoom(z1080, d1080));
            Assert.AreEqual(PassiveTreeLod.Level.Mid, PassiveTreeLod.FromZoom(z1440, d1440));
            Assert.AreEqual(12f, PassiveTreeLod.NormalProjectedPx(z1080, d1080), 0.0001f);
            Assert.AreEqual(12f, PassiveTreeLod.NormalProjectedPx(z1440, d1440), 0.0001f);
            Assert.AreNotEqual(z1080, z1440, "同样投影像素在不同 DesignScale 下 zoom 必须不同（禁止 raw-zoom LOD）");
        }

        [Test]
        public void RepresentativeFixtures_LandInExpectedLod()
        {
            float ds = SliceHud.DesignScale(1920f, 1080f);
            Assert.AreEqual(PassiveTreeLod.Level.Overview,
                PassiveTreeLod.FromZoom(PassiveTreeLod.ZoomForProjectedPx(6f, ds), ds));
            Assert.AreEqual(PassiveTreeLod.Level.Mid,
                PassiveTreeLod.FromZoom(PassiveTreeLod.ZoomForProjectedPx(12f, ds), ds));
            Assert.AreEqual(PassiveTreeLod.Level.Detail,
                PassiveTreeLod.FromZoom(PassiveTreeLod.ZoomForProjectedPx(24f, ds), ds));
        }

        [Test]
        public void FullIconPolicy_OverviewNone_MidImportantOnly()
        {
            Assert.IsFalse(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Overview, PoeNodeKind.Normal));
            Assert.IsFalse(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Overview, PoeNodeKind.Notable));
            Assert.IsFalse(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Normal));
            Assert.IsFalse(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Jewel));
            Assert.IsTrue(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Notable));
            Assert.IsTrue(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Keystone));
            Assert.IsTrue(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Mastery));
            Assert.IsTrue(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Mid, PoeNodeKind.Start));
            Assert.IsTrue(PassiveTreeLod.AllowsFullIcon(PassiveTreeLod.Level.Detail, PoeNodeKind.Normal));
        }

        [Test]
        public void GroupDecoration_OverviewOff_MidByProjectedDiameter()
        {
            Assert.IsFalse(PassiveTreeLod.AllowsGroupDecoration(PassiveTreeLod.Level.Overview, 400f));
            Assert.IsFalse(PassiveTreeLod.AllowsGroupDecoration(PassiveTreeLod.Level.Mid, 159.9f));
            Assert.IsTrue(PassiveTreeLod.AllowsGroupDecoration(PassiveTreeLod.Level.Mid, 160f));
            Assert.IsTrue(PassiveTreeLod.AllowsGroupDecoration(PassiveTreeLod.Level.Detail, 10f));
        }

        [Test]
        public void HitRadius_HasSixPxFloor()
        {
            float tiny = PassiveTreeLod.HitRadiusPx((int)PoeNodeKind.Normal, 0.02f, 1f);
            Assert.AreEqual(6f, tiny, 0.0001f);
            float big = PassiveTreeLod.HitRadiusPx((int)PoeNodeKind.Keystone, 1f, 1f);
            Assert.Greater(big, 6f);
        }

        [Test]
        public void Lod0_PlanRequestsNoIndividualIconsOrChrome()
        {
            float ds = SliceHud.DesignScale(1920f, 1080f);
            float z = PassiveTreeLod.ZoomForProjectedPx(6f, ds);
            var view = new Rect(0f, 0f, 1920f, 1080f);
            Vector2 pan = PassiveTreeLod.FocusPan(view, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(view, pan, z, ds);
            Assert.AreEqual(PassiveTreeLod.Level.Overview, plan.Lod);
            Assert.AreEqual(0, plan.RequiredIconCount);
            Assert.IsFalse(plan.NeedFrameAtlas);
            Assert.IsFalse(plan.NeedGroupAtlas);
            Assert.Greater(plan.VisibleNodeCount, 0);
        }

        [Test]
        public void HitNodeId_Fixtures_MatchAcrossThreeLods()
        {
            int keystone = FirstKind(PoeNodeKind.Keystone);
            int jewel = FirstKind(PoeNodeKind.Jewel);
            Assert.GreaterOrEqual(keystone, 0);
            Assert.GreaterOrEqual(jewel, 0);
            int[] ids = { 2172, 71, 183, 10, 1006, keystone, jewel };
            float ds = SliceHud.DesignScale(1920f, 1080f);
            float[] px = { 6f, 12f, 24f };
            var view = new Rect(0f, 0f, 1920f, 1080f);
            for (int f = 0; f < ids.Length; f++)
            {
                int id = ids[f];
                for (int L = 0; L < px.Length; L++)
                {
                    float z = PassiveTreeLod.ZoomForProjectedPx(px[L], ds);
                    Vector2 pan = PassiveTreeLod.FocusPan(view, z, id);
                    Vector2 c = PoeTreeView.ScreenOf(new Vector2(PoeTree.Get(id).x, PoeTree.Get(id).y), pan, z);
                    Assert.AreEqual(id, PassiveTreeLod.HitNodeId(c, pan, z, ds), "centre lodPx=" + px[L] + " node=" + id);
                    float hr = PassiveTreeLod.HitRadiusPx(PoeTree.Get(id).kind, z, ds) / ds;
                    float o = 0.4f * hr;
                    Assert.AreEqual(id, PassiveTreeLod.HitNodeId(c + new Vector2(o, 0f), pan, z, ds));
                    Assert.AreEqual(id, PassiveTreeLod.HitNodeId(c + new Vector2(-o, 0f), pan, z, ds));
                    Assert.AreEqual(id, PassiveTreeLod.HitNodeId(c + new Vector2(0f, o), pan, z, ds));
                    Assert.AreEqual(id, PassiveTreeLod.HitNodeId(c + new Vector2(0f, -o), pan, z, ds));
                }
            }
        }

        [Test]
        public void VisibleSet_DoesNotDependOnLodFunction()
        {
            float ds = 1f;
            float z = PassiveTreeLod.ZoomForProjectedPx(12f, ds);
            var view = new Rect(0f, 0f, 1920f, 1080f);
            Vector2 pan = PassiveTreeLod.FocusPan(view, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(view, pan, z, ds);
            int vis = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                bool v = PoeTreeView.Visible(PoeTree.Get(i), pan, z, view, PassiveTreeRenderPlan.NodeVisiblePad);
                if (plan.VisibleNodes[i]) vis++;
                Assert.AreEqual(v, plan.VisibleNodes[i], "visible 不得吃 LOD：" + i);
            }
            Assert.AreEqual(vis, plan.VisibleNodeCount);
        }

        static int FirstKind(PoeNodeKind k)
        {
            for (int i = 0; i < PoeTree.Count; i++)
                if (PoeTree.Get(i).Kind == k)
                    return i;
            return -1;
        }
    }
}
