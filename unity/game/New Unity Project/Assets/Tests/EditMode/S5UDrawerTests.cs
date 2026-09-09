using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5U-WO-03 装备/背包呈现契约（docs/reviews/S5U/S5U_WO_02_EVIDENCE.md §7 建议单 + 规划 AI 2026-09-09 放行令）：
    /// 抽屉壳几何单一来源（不重叠战斗栏/双分辨率出界）/恰 6 装备槽/呈现网格非机制（1 物品=1 格、顺序=库存真值）/
    /// 装备槽符文缓存与恒等映射/展示顺序=人体逻辑且 EquipSlot ID 不漂移。
    /// 既有断言零削弱：SliceDrawerTests（旧 Column 系）原样保留。
    /// </summary>
    public sealed class S5UDrawerTests
    {
        static readonly Vector2[] DesignSpaces =
        {
            new Vector2(1920f, 1080f),
            new Vector2(2743.2f, 1542.6f),
            new Vector2(1280f, 720f)
        };

        [Test]
        public void Shell_StaysInViewport_AboveCombatBar([ValueSource(nameof(DesignSpaces))] Vector2 space)
        {
            var shell = SliceDrawerLayout.Shell(space.x, space.y);
            var bar = SliceHud.CombatBarRects(space.x, space.y).Bar;
            Assert.LessOrEqual(shell.xMax, space.x, "壳不得溢出右缘");
            Assert.GreaterOrEqual(shell.x, 12f, "壳左缘不越界");
            Assert.GreaterOrEqual(shell.y, 0f);
            Assert.LessOrEqual(shell.yMax, space.y, "壳不得溢出下缘");
            Assert.LessOrEqual(shell.yMax + 0.01f, bar.y, "壳底缘必须位于战斗栏顶之上（永不重叠）");
        }

        [Test]
        public void Shell_SubRects_InsideShell_AndConsistent([ValueSource(nameof(DesignSpaces))] Vector2 space)
        {
            var shell = SliceDrawerLayout.Shell(space.x, space.y);
            Assert.IsTrue(RectMinMaxInside(shell, SliceDrawerLayout.ShellHeader(space.x, space.y)), "头部在壳内");
            var tab0 = SliceDrawerLayout.ShellTab(0, space.x, space.y);
            var tab1 = SliceDrawerLayout.ShellTab(1, space.x, space.y);
            Assert.IsTrue(RectMinMaxInside(shell, tab0) && RectMinMaxInside(shell, tab1), "tab 在壳内");
            Assert.LessOrEqual(tab0.xMax + 0.01f, tab1.x, "两 tab 不重叠");
            var foot = SliceDrawerLayout.ShellFooter(space.x, space.y);
            Assert.IsTrue(RectMinMaxInside(shell, foot), "反馈条在壳内");
            for (int i = 0; i < 6; i++)
                Assert.IsTrue(RectMinMaxInside(shell, SliceDrawerLayout.ShellSlot(i, space.x, space.y)), "槽 " + i + " 在壳内");
            var view = SliceDrawerLayout.ShellInvView(space.x, space.y);
            Assert.IsTrue(RectMinMaxInside(shell, view), "背包视口在壳内");
            Assert.LessOrEqual(view.yMax + 0.01f, foot.y - 0.01f, "背包视口不得压反馈条");
        }

        static bool RectMinMaxInside(Rect outer, Rect inner)
        {
            return inner.x >= outer.x - 0.01f && inner.y >= outer.y - 0.01f &&
                   inner.xMax <= outer.xMax + 0.01f && inner.yMax <= outer.yMax + 0.01f;
        }

        [Test]
        public void Shell_ExactlySixSlots_Uniform_NonOverlap()
        {
            Assert.AreEqual(6, SliceDrawerLayout.DisplayOrder.Length, "恒 6 槽（禁 Ring/Amulet/Offhand/双武器位）");
            var expected = new[]
            {
                EquipSlot.Weapon, EquipSlot.Helmet, EquipSlot.Body,
                EquipSlot.Gloves, EquipSlot.Boots, EquipSlot.Belt
            };
            for (int i = 0; i < 6; i++)
                Assert.AreEqual(expected[i], SliceDrawerLayout.DisplayOrder[i], "展示顺序=人体逻辑（EquipSlot ID 不漂移）");

            var a = SliceDrawerLayout.ShellSlot(0, 1920f, 1080f);
            Assert.AreEqual(new Vector2(SliceDrawerLayout.ShellSlotW, SliceDrawerLayout.ShellSlotH), new Vector2(a.width, a.height));
            for (int i = 1; i < 6; i++)
            {
                var b = SliceDrawerLayout.ShellSlot(i, 1920f, 1080f);
                Assert.AreEqual(a.width, b.width);
                Assert.AreEqual(a.height, b.height);
            }
            var s01 = SliceDrawerLayout.ShellSlot(0, 1920f, 1080f);
            var s10 = SliceDrawerLayout.ShellSlot(1, 1920f, 1080f);
            Assert.LessOrEqual(s01.xMax + 0.01f, s10.x, "同排槽不重叠");
            var s03 = SliceDrawerLayout.ShellSlot(0, 1920f, 1080f);
            var s12 = SliceDrawerLayout.ShellSlot(2, 1920f, 1080f);
            Assert.LessOrEqual(s03.yMax + 0.01f, s12.y, "跨排槽不重叠");
        }

        [Test]
        public void InventoryGrid_PresentationOnly_UniformCells()
        {
            // 1 物品=1 恰一格；rows=ceil(count/3)；单元同尺寸不重叠；无 width/height/占多格/旋转（内容高仅由行数决定）
            for (int count = 0; count <= 15; count++)
            {
                int rows = (count + 2) / 3;
                Assert.AreEqual(rows * (SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap),
                    SliceDrawerLayout.ShellInvContentHeight(count), "count=" + count);
            }
            var c0 = SliceDrawerLayout.ShellInvCell(0);
            var c1 = SliceDrawerLayout.ShellInvCell(1);
            var c2 = SliceDrawerLayout.ShellInvCell(2);
            var c3 = SliceDrawerLayout.ShellInvCell(3);
            Assert.AreEqual(new Vector2(SliceDrawerLayout.CellW, SliceDrawerLayout.CellH), new Vector2(c0.width, c0.height));
            Assert.AreEqual(c0.y, c1.y, "同排同高");
            Assert.AreEqual(c0.y, c2.y, "每排恰 3 列");
            Assert.LessOrEqual(c0.xMax + 0.01f, c1.x, "横向不重叠");
            Assert.LessOrEqual(c1.xMax + 0.01f, c2.x, "横向不重叠");
            Assert.LessOrEqual(c0.yMax + 0.01f, c3.y, "纵向不重叠");
            // 单元必须装进视口宽（296 内容宽 ≥ 3×92+2×4）
            var view = SliceDrawerLayout.ShellInvView(1920f, 1080f);
            Assert.LessOrEqual(SliceDrawerLayout.ShellW - 16f, view.width + 0.01f);
            Assert.GreaterOrEqual(view.width, 3f * SliceDrawerLayout.CellW + 2f * SliceDrawerLayout.GridGap, "3 列装得下");
        }

        [Test]
        public void EquipGlyphs_Ensure_Idempotent_AllSixDistinct()
        {
            var weapon = SliceHudIcons.EqWeapon;
            var helmet = SliceHudIcons.EqHelmet;
            var body = SliceHudIcons.EqBody;
            var gloves = SliceHudIcons.EqGloves;
            var boots = SliceHudIcons.EqBoots;
            var belt = SliceHudIcons.EqBelt;
            SliceHudIcons.Ensure();
            Assert.AreSame(weapon, SliceHudIcons.EqWeapon, "装备符文必须一次合成静态缓存");
            Assert.AreSame(helmet, SliceHudIcons.EqHelmet);
            Assert.AreSame(body, SliceHudIcons.EqBody);
            Assert.AreSame(gloves, SliceHudIcons.EqGloves);
            Assert.AreSame(boots, SliceHudIcons.EqBoots);
            Assert.AreSame(belt, SliceHudIcons.EqBelt);
            Assert.AreEqual(64, weapon.width, "装备符文 64²");
            // 六槽互异 + 恒等映射稳定
            var all = new[] { weapon, helmet, body, gloves, boots, belt };
            for (int i = 0; i < all.Length; i++)
                for (int j = i + 1; j < all.Length; j++)
                    Assert.AreNotSame(all[i], all[j], "六槽符文必须互异（槽位类型可辨识）");
            Assert.AreSame(weapon, SliceHudIcons.EquipGlyph(EquipSlot.Weapon));
            Assert.AreSame(helmet, SliceHudIcons.EquipGlyph(EquipSlot.Helmet));
            Assert.AreSame(body, SliceHudIcons.EquipGlyph(EquipSlot.Body));
            Assert.AreSame(gloves, SliceHudIcons.EquipGlyph(EquipSlot.Gloves));
            Assert.AreSame(boots, SliceHudIcons.EquipGlyph(EquipSlot.Boots));
            Assert.AreSame(belt, SliceHudIcons.EquipGlyph(EquipSlot.Belt));
        }
    }
}
