using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 背包面板几何契约（2026-09-10 贴右通顶；2026-09-12 导演：宝石与物品共用背包格网，取消托盘分区）。
    /// 全部为纯几何断言，与 SliceHud 共用同一组布局函数。
    /// </summary>
    public sealed class SliceBagPanelTests
    {
        static readonly Vector2[] Spaces =
        {
            new Vector2(1920f, 1080f),
            new Vector2(2560f, 1440f),   // 同一设计空间（DesignScale 折算后）
            new Vector2(2580f, 1080f)
        };

        [Test]
        public void Panel_IsFlushRight_FullHeight([ValueSource(nameof(Spaces))] Vector2 space)
        {
            var s = SliceDrawerLayout.Shell(space.x, space.y);
            Assert.AreEqual(space.x, s.xMax, 0.01f, "面板右缘必须完全贴合屏幕右缘");
            Assert.AreEqual(0f, s.y, 0.01f, "面板上缘必须通到屏幕顶");
            Assert.AreEqual(space.y, s.yMax, 0.01f, "面板下缘必须通到屏幕底");
            Assert.AreEqual(SliceDrawerLayout.PanelW, s.width, 0.01f);
        }

        [Test]
        public void PanelSections_NeverOverlap_AndStayInside([ValueSource(nameof(Spaces))] Vector2 space)
        {
            float dw = space.x, dh = space.y;
            var shell = SliceDrawerLayout.Shell(dw, dh);
            var header = SliceDrawerLayout.ShellHeader(dw, dh);
            var equipLabel = SliceDrawerLayout.EquipLabel(dw, dh);
            var invLabel = SliceDrawerLayout.InvLabel(dw, dh);
            var view = SliceDrawerLayout.ShellInvView(dw, dh);
            var footer = SliceDrawerLayout.ShellFooter(dw, dh);

            var ordered = new[] { header, equipLabel, invLabel, view, footer };
            for (int i = 0; i < ordered.Length; i++)
            {
                Assert.IsTrue(Encloses(shell, ordered[i]), "区块 " + i + " 必须落在面板内");
                if (i > 0)
                    Assert.LessOrEqual(ordered[i - 1].yMax, ordered[i].y + 0.01f,
                        "区块 " + (i - 1) + " 与 " + i + " 纵向不得重叠");
            }

            // 装备六卡：3 列 × 2 行，互不重叠且在面板内
            var rects = new Rect[6];
            for (int i = 0; i < 6; i++)
            {
                rects[i] = SliceDrawerLayout.ShellSlot(i, dw, dh);
                Assert.IsTrue(Encloses(shell, rects[i]), "装备卡 " + i + " 越出面板");
            }
            for (int i = 0; i < rects.Length; i++)
                for (int k = i + 1; k < rects.Length; k++)
                    Assert.IsFalse(rects[i].Overlaps(rects[k]), "装备卡 " + i + " 与 " + k + " 重叠");
            Assert.LessOrEqual(rects[5].yMax, invLabel.y + 0.01f, "装备区之下直接是背包，不得再插托盘");
        }

        static bool Encloses(Rect outer, Rect inner)
        {
            return inner.x >= outer.x - 0.01f && inner.y >= outer.y - 0.01f &&
                   inner.xMax <= outer.xMax + 0.01f && inner.yMax <= outer.yMax + 0.01f;
        }

        [Test]
        public void DisplayOrder_IsBijectionOverAllSixSlots()
        {
            var seen = new HashSet<EquipSlot>();
            foreach (var slot in SliceDrawerLayout.DisplayOrder)
                Assert.IsTrue(seen.Add(slot), "UI 展示顺序不得重复槽：" + slot);
            Assert.AreEqual((int)EquipSlot.Count, SliceDrawerLayout.DisplayOrder.Length, "六槽必须全部呈现");
        }

        [Test]
        public void Grid_IsFullWidth_AndHoldsWholeCapacity()
        {
            int perRow = SliceDrawerLayout.InvColumns;
            Assert.GreaterOrEqual(perRow, 12, "导演要求扩大格子显示量：至少 12 列");
            Assert.AreEqual(0, SliceRules.InventoryCap % perRow, "满幅格网必须整除容量（无残缺末行）");

            float gridW = perRow * SliceDrawerLayout.CellW + (perRow - 1) * SliceDrawerLayout.GridGap;
            float innerW = SliceDrawerLayout.PanelW - 2f * SliceDrawerLayout.PadX;
            Assert.LessOrEqual(gridW, innerW + 0.01f, "12 列必须装得进面板内衬宽度");

            int rows = SliceRules.InventoryCap / perRow;
            Assert.AreEqual(rows * (SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap),
                SliceDrawerLayout.ShellInvContentHeight(SliceRules.InventoryCap), 0.01f);
            int shared = SliceDrawerLayout.SharedBagCellCount;
            int sharedRows = (shared + perRow - 1) / perRow;
            Assert.AreEqual(sharedRows * (SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap),
                SliceDrawerLayout.ShellInvContentHeight(shared), 0.01f);
        }

        [Test]
        public void GridCells_TileWithoutOverlap()
        {
            var first = SliceDrawerLayout.ShellInvCell(0);
            var last = SliceDrawerLayout.ShellInvCell(SliceRules.InventoryCap - 1);
            Assert.AreEqual(0f, first.x, 0.01f);
            Assert.AreEqual(0f, first.y, 0.01f);
            var second = SliceDrawerLayout.ShellInvCell(1);
            Assert.AreEqual(SliceDrawerLayout.CellW + SliceDrawerLayout.GridGap, second.x, 0.01f);
            var nextRow = SliceDrawerLayout.ShellInvCell(SliceDrawerLayout.InvColumns);
            Assert.AreEqual(0f, nextRow.x, 0.01f);
            Assert.AreEqual(SliceDrawerLayout.CellH + SliceDrawerLayout.GridGap, nextRow.y, 0.01f);
            Assert.Greater(last.xMax, 0f);
        }

        [Test]
        public void InvCellScreenRect_AppliesViewOffsetAndScroll()
        {
            var view = new Rect(100f, 200f, 800f, 400f);
            var scroll = new Vector2(0f, 37f);
            Rect a = SliceHud.InvCellScreenRect(view, Vector2.zero, 0);
            Rect b = SliceHud.InvCellScreenRect(view, scroll, 0);
            Assert.AreEqual(view.x, a.x, 0.01f);
            Assert.AreEqual(view.y, a.y, 0.01f);
            Assert.AreEqual(a.y - 37f, b.y, 0.01f, "滚动偏移必须体现在屏幕坐标上");
            Rect cell5 = SliceDrawerLayout.ShellInvCell(5);
            Rect screen5 = SliceHud.InvCellScreenRect(view, scroll, 5);
            Assert.AreEqual(view.x + cell5.x, screen5.x, 0.01f);
            Assert.AreEqual(view.y + cell5.y - 37f, screen5.y, 0.01f);
        }

        [Test]
        public void SharedGrid_HoldsEverySupportGem()
        {
            int n = SupportCatalog.Count;
            Assert.Greater(n, 0);
            Assert.AreEqual(n, SliceDrawerLayout.GemOccupantCount);
            for (int i = 0; i < n; i++)
            {
                Assert.IsTrue(SliceDrawerLayout.CellIsGem(i));
                Rect cell = SliceDrawerLayout.ShellInvCell(i);
                Assert.Greater(cell.width, 0f);
                Assert.Greater(cell.height, 0f);
            }
        }
    }
}
