using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5U-WO-03 装备/背包呈现契约（docs/reviews/S5U/S5U_WO_02_EVIDENCE.md §7 建议单 + 规划 AI 2026-09-09 放行令）：
    /// 背包面板壳几何单一来源（2026-09-10 导演重排：贴右/通顶，与战斗栏横向永不相交）/恰 6 装备槽/
    /// 呈现网格非机制（1 物品=1 格、顺序=库存真值、满幅列数）/装备槽符文缓存与恒等映射/
    /// 展示顺序=人体逻辑且 EquipSlot ID 不漂移。
    /// </summary>
    public sealed class S5UDrawerTests
    {
        static readonly Vector2[] DesignSpaces =
        {
            new Vector2(1920f, 1080f),
            new Vector2(2743.2f, 1542.6f),
            new Vector2(1920f, 1600f)
        };

        [Test]
        public void Shell_FlushRightFullHeight_AndClearOfCombatBar([ValueSource(nameof(DesignSpaces))] Vector2 space)
        {
            var shell = SliceDrawerLayout.Shell(space.x, space.y);
            var bar = SliceHud.CombatBarRects(space.x, space.y).Bar;
            Assert.AreEqual(space.x, shell.xMax, 0.01f, "壳右缘必须贴合屏幕右缘");
            Assert.AreEqual(0f, shell.y, 0.01f, "壳上缘必须通到屏幕顶");
            Assert.AreEqual(space.y, shell.yMax, 0.01f, "壳下缘必须通到屏幕底（上下通顶）");
            // 底栏在面板左侧的剩余宽度内居中：二者横向永不相交
            Assert.LessOrEqual(bar.xMax, shell.x + 0.01f, "战斗栏不得进入面板区域（否则被面板压住）");
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
        public void Shell_ExactlySixSlots_Paperdoll_NonOverlap()
        {
            Assert.AreEqual(6, SliceDrawerLayout.DisplayOrder.Length, "恒 6 槽（禁 Ring/Amulet/Offhand/双武器位）");
            var expected = new[]
            {
                EquipSlot.Weapon, EquipSlot.Helmet, EquipSlot.Body,
                EquipSlot.Gloves, EquipSlot.Boots, EquipSlot.Belt
            };
            for (int i = 0; i < 6; i++)
                Assert.AreEqual(expected[i], SliceDrawerLayout.DisplayOrder[i], "展示顺序=人体逻辑（EquipSlot ID 不漂移）");

            // 3 列 × 2 行等大卡；两两不重叠；全部落在装备区内
            var slots = new Rect[6];
            for (int i = 0; i < 6; i++)
            {
                slots[i] = SliceDrawerLayout.ShellSlot(i, 1920f, 1080f);
                Assert.AreEqual(SliceDrawerLayout.CardW, slots[i].width, 0.01f, "槽 " + i + " 宽=装备卡宽");
                Assert.AreEqual(SliceDrawerLayout.CardH, slots[i].height, 0.01f, "槽 " + i + " 高=装备卡高");
            }
            for (int a = 0; a < 6; a++)
                for (int b = a + 1; b < 6; b++)
                    Assert.IsFalse(slots[a].Overlaps(slots[b]), "槽 " + a + " 与 " + b + " 不重叠");
            var shell = SliceDrawerLayout.Shell(1920f, 1080f);
            for (int i = 0; i < 6; i++)
                Assert.IsTrue(RectMinMaxInside(shell, slots[i]), "槽 " + i + " 在弹窗内");
        }

        [Test]
        public void InventoryGrid_PresentationOnly_UniformCells()
        {
            // 1 物品=1 恰一格；rows=ceil(count/列数)；单元同尺寸不重叠；内容高仅由行数决定
            int cols = SliceDrawerLayout.InvColumns;
            for (int count = 0; count <= cols * 2; count++)
            {
                int rows = (count + cols - 1) / cols;
                Assert.AreEqual(rows * (SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap),
                    SliceDrawerLayout.ShellInvContentHeight(count), "count=" + count);
            }
            var c0 = SliceDrawerLayout.ShellInvCell(0);
            var c1 = SliceDrawerLayout.ShellInvCell(1);
            var cLast = SliceDrawerLayout.ShellInvCell(cols - 1);
            var cNext = SliceDrawerLayout.ShellInvCell(cols);
            Assert.AreEqual(new Vector2(SliceDrawerLayout.CellW, SliceDrawerLayout.CellH), new Vector2(c0.width, c0.height));
            Assert.AreEqual(c0.y, c1.y, "同排同高");
            Assert.AreEqual(c0.y, cLast.y, "每排恰 InvColumns 列");
            Assert.LessOrEqual(c0.xMax + 0.01f, c1.x, "横向不重叠");
            Assert.Greater(cNext.y, cLast.yMax, "第二行与首行纵向不重叠");
            Assert.LessOrEqual(cLast.xMax,
                cols * SliceDrawerLayout.CellW + (cols - 1) * SliceDrawerLayout.GridGap + 0.05f, "首行恰满列收口");
            // 视口恰容纳整幅格子（导演指令：扩大格子显示量）
            var view = SliceDrawerLayout.ShellInvView(1920f, 1080f);
            Assert.GreaterOrEqual(view.width + 0.01f,
                cols * SliceDrawerLayout.CellW + (cols - 1) * SliceDrawerLayout.GridGap, "满幅格网装得下");
        }

        [Test]
        public void ScrollHover_ScreenRectMapping_NeverDrifts()
        {
            // S5U-F1 回归（规划 AI 硬合同）：inventory scroll≠0 + 指针在视觉显示的 cell N 上 → 悬停必须解析为 N；
            // 内容坐标/视口坐标/指针坐标不得再漂移（单一来源 InvCellScreenRect）。
            int cols = SliceDrawerLayout.InvColumns;
            float rowStep = SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap;
            var view = SliceDrawerLayout.ShellInvView(1920f, 1080f);
            // scroll=0：第一行
            var c0 = SliceHud.InvCellScreenRect(view, Vector2.zero, 0);
            Assert.AreEqual(view.x, c0.x, "scroll=0 时首格与视口同 x");
            Assert.AreEqual(view.y, c0.y, "scroll=0 时首格与视口同 y");
            Assert.IsTrue(c0.Contains(new Vector2(view.x + 20f, view.y + 20f)), "scroll=0 指针在首格内可命中");
            // non-zero scroll：第二行首格（内容 y=rowStep）；scroll.y=rowStep → 屏幕位置=视口顶
            var scroll = new Vector2(0f, rowStep);
            var cRow1 = SliceHud.InvCellScreenRect(view, scroll, cols);
            Assert.AreEqual(view.y, cRow1.y, "scroll=rowStep 时第二行首格对齐视口顶");
            Assert.AreEqual(view.x, cRow1.x, "第二行首格仍是第 0 列");
            Assert.IsTrue(cRow1.Contains(new Vector2(view.x + 20f, view.y + 20f)), "scroll 后同屏幕点命中第二行首格");
            Assert.AreNotEqual(SliceHud.InvCellScreenRect(view, Vector2.zero, cols).y, cRow1.y,
                "scroll=0 时第二行仍在视口下方（映射随滚动唯一变化）");
            // 第三行首格随 scroll=2*rowStep 对齐视口顶；横向仍第 0 列
            var cRow2 = SliceHud.InvCellScreenRect(view, new Vector2(0f, rowStep * 2f), cols * 2);
            Assert.AreEqual(view.y, cRow2.y);
            Assert.AreEqual(view.x, cRow2.x);
            // 第一行第 6 列：横向无滚动偏移
            var c6 = SliceHud.InvCellScreenRect(view, Vector2.zero, 6);
            Assert.AreEqual(view.y, c6.y);
            Assert.AreEqual(view.x + 6f * (SliceDrawerLayout.CellW + SliceDrawerLayout.GridGap), c6.x, "第 6 列横向偏移不变");
            // 换算与内容坐标一致性：屏幕矩形尺寸恒等于 cell 尺寸
            var raw = SliceDrawerLayout.ShellInvCell(4);
            var scr = SliceHud.InvCellScreenRect(view, new Vector2(0f, 37f), 4);
            Assert.AreEqual(raw.width, scr.width);
            Assert.AreEqual(raw.height, scr.height);
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
            // 源=Aria 高清模板（2026-09-10 起 512²，早前 128²）或程序化 64² 回退；绘制端缩放，皆合法
            Assert.That(weapon.width, Is.EqualTo(512).Or.EqualTo(128).Or.EqualTo(64),
                "装备符文=Aria 512²/128² 或程序化 64²，实际 " + weapon.width);
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
