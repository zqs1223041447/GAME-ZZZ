using UnityEngine;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase3 正式 UI 换皮层契约（SliceSkin 程序化贴图）：
    /// 合成确定性/幂等、九宫格边框不变量（面板/槽位）、球环 alpha 语义（外内透明+环带不透明）。
    /// 像素断言全部基于固定种子整数哈希噪声=确定性成立。
    /// </summary>
    public class SliceSkinTests
    {
        [Test]
        public void Ensure_CreatesAllTextures_AndIsIdempotent()
        {
            SliceSkin.Ensure();
            Assert.IsNotNull(SliceSkin.Panel);
            Assert.IsNotNull(SliceSkin.SlotNormal);
            Assert.IsNotNull(SliceSkin.SlotHover);
            Assert.IsNotNull(SliceSkin.SlotPress);
            Assert.IsNotNull(SliceSkin.SlotSelected);
            Assert.IsNotNull(SliceSkin.OrbRing);
            Assert.IsNotNull(SliceSkin.OrbFill);
            Assert.AreEqual(256, SliceSkin.Panel.width);
            Assert.AreEqual(96, SliceSkin.SlotNormal.width);
            Assert.AreEqual(192, SliceSkin.OrbRing.width);

            Texture2D before = SliceSkin.Panel;
            SliceSkin.Ensure();
            Assert.AreSame(before, SliceSkin.Panel, "Ensure 必须幂等（合成一次静态缓存）");
        }

        [Test]
        public void Panel_HasDarkBevelCorner_AndGoldTrimLine()
        {
            Texture2D t = SliceSkin.Panel;
            Color corner = t.GetPixel(0, 0);
            Assert.Less(corner.r, 0.05f, "外缘必须为暗凿边（BevelDark）");
            Color trim = t.GetPixel(t.width / 2, 5); // inset 5 的金线（底边中点）
            Assert.Greater(trim.r, 0.5f, "面板必须含暗金内描线");
        }

        [Test]
        public void Slot_VariantsTrimBrightness_Ordered()
        {
            int y = 3; // inset 3 描线
            float normal = SliceSkin.SlotNormal.GetPixel(48, y).r;
            float hover = SliceSkin.SlotHover.GetPixel(48, y).r;
            float press = SliceSkin.SlotPress.GetPixel(48, y).r;
            Assert.Greater(hover, normal, "hover 描线必须比常态亮（Rare 金）");
            Assert.Less(press, normal, "press 描线必须比常态暗（暗金）");
        }

        [Test]
        public void Slot_Selected_HasInnerGlowLine()
        {
            Texture2D t = SliceSkin.SlotSelected;
            Color inner = t.GetPixel(48, 5); // inset 3+2 的内晕圈
            Assert.Greater(inner.a, 0f, "选中态必须有内晕圈线");
            Assert.Greater(inner.r, 0.5f, "内晕圈为金系");
        }

        [Test]
        public void OrbRing_CornerAndHoleTransparent_BandOpaque()
        {
            Texture2D t = SliceSkin.OrbRing;
            Assert.Less(t.GetPixel(2, 2).a, 0.02f, "环外角必须透明");
            Assert.Less(t.GetPixel(96, 96).a, 0.02f, "环内孔必须透明");
            Assert.Greater(t.GetPixel(96, 8).a, 0.5f, "环带必须不透明（石质环）");
        }

        [Test]
        public void OrbFill_CenterOpaqueGrey_CornerTransparent()
        {
            Texture2D t = SliceSkin.OrbFill;
            Color c = t.GetPixel(96, 96);
            Assert.Greater(c.a, 0.98f, "球液中心必须不透明");
            Assert.AreEqual(c.r, c.g, "球液必须为灰阶（供运行期染色）");
            Assert.AreEqual(c.g, c.b, "球液必须为灰阶");
            Assert.Less(t.GetPixel(2, 2).a, 0.02f, "球外角必须透明");
        }
    }
}
