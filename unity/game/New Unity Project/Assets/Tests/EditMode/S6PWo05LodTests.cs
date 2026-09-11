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
    }
}
