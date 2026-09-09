using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 3 R2 右侧装备抽屉契约（工作令 S3-P3-UI-R2-EQUIPMENT-DRAWER 第十五节）：
    /// 抽屉几何（设计空间内/槽位不重叠/槽位在列头区）、内容面板不压底栏关键区、
    /// 1080p/1440p 缩放换算、ShouldBlockWorld 抽屉内外点、EquipSlot canonical 数值稳定（S4-P2 六槽）。
    /// </summary>
    public class SliceDrawerTests
    {
        const float Dw = 1920f;
        const float Dh = 1080f;

        [Test]
        public void Column_FitsDesignSpace_At1080p()
        {
            Rect c = SliceDrawerLayout.Column(Dw);
            Assert.GreaterOrEqual(c.x, 0f);
            Assert.LessOrEqual(c.xMax, Dw);
            Assert.GreaterOrEqual(c.y, 0f);
            Assert.LessOrEqual(c.yMax, Dh);
            Assert.AreEqual(SliceDrawerLayout.ColumnW, c.width);
            Assert.AreEqual(SliceDrawerLayout.ColumnH, c.height);
        }

        [Test]
        public void SixSlots_NonOverlapping_AndInsideColumnHeader()
        {
            Rect c = SliceDrawerLayout.Column(Dw);
            Assert.AreEqual(6, SliceDrawerLayout.DisplayOrder.Length, "UI 展示顺序必须覆盖 6 槽");
            Rect[] slots = new Rect[6];
            for (int i = 0; i < 6; i++)
            {
                slots[i] = SliceDrawerLayout.Slot(i, Dw);
                Assert.IsTrue(c.Contains(slots[i].center), $"槽 {i} 中心必须在抽屉列内");
                Assert.LessOrEqual(slots[i].yMax, c.yMax - SliceDrawerLayout.TabH - 6f + 2f,
                    "槽不得侵入 tab 行");
            }
            for (int a = 0; a < 6; a++)
            {
                for (int b = a + 1; b < 6; b++)
                {
                    Assert.IsFalse(slots[a].Overlaps(slots[b]), $"槽 {a} 与槽 {b} 不得重叠");
                }
            }
        }

        [Test]
        public void DisplayOrder_CoversAllSlots_ExactlyOnce()
        {
            // S4-P2 十九：UI 人体顺序与 stable ID 分离——顺序可变，但必须是 6 槽的双射
            var seen = new HashSet<EquipSlot>();
            foreach (var slot in SliceDrawerLayout.DisplayOrder)
                Assert.IsTrue(seen.Add(slot), "UI 展示顺序不得重复槽：" + slot);
            for (int i = 0; i < (int)EquipSlot.Count; i++)
                Assert.IsTrue(seen.Contains((EquipSlot)i), "UI 展示顺序缺槽：" + (EquipSlot)i);
        }

        [Test]
        public void BuildAndCraftPanels_LeftOfColumn_AndNotCoveringBottomBar()
        {
            Rect column = SliceDrawerLayout.Column(Dw);
            Rect build = SliceDrawerLayout.BuildPanel(Dw);
            Rect craft = SliceDrawerLayout.CraftPanel(Dw);
            Assert.LessOrEqual(build.xMax, column.x, "Build 面板不得覆盖抽屉列");
            Assert.LessOrEqual(craft.xMax, column.x, "Craft 面板不得覆盖抽屉列");
            // 底栏关键区（设计空间）：底栏顶=1080-118，双球/技能槽/辅助条都在其中
            float bottomBarTop = Dh - 118f;
            Assert.LessOrEqual(build.yMax, bottomBarTop, "Build 面板不得压底栏关键区");
            Assert.LessOrEqual(craft.yMax, bottomBarTop, "Craft 面板不得压底栏关键区");
            // 面板尺寸保持既有渲染器原尺寸（move, not duplicate：不改内容布局）
            Assert.AreEqual(760f, build.width);
            Assert.AreEqual(430f, build.height);
            Assert.AreEqual(560f, craft.width);
            Assert.AreEqual(340f, craft.height);
        }

        [Test]
        public void DesignScale_At1080p_And1440p()
        {
            Assert.AreEqual(1f, SliceHud.DesignScale(1920f, 1080f), 0.0001f, "1080p=设计基准 1.0");
            Assert.AreEqual(2560f / 1920f, SliceHud.DesignScale(2560f, 1440f), 0.0001f, "1440p=1.333 同比放大");
            Assert.AreEqual(0.4f, SliceHud.DesignScale(600f, 400f), 0.0001f, "极小窗口夹取下限 0.4");
            Assert.AreEqual(1.4f, SliceHud.DesignScale(4000f, 2200f), 0.0001f, "超大窗口夹取上限 1.4");
        }

        [Test]
        public void BlocksWorldInput_DrawerInsideTrue_OutsideFalse()
        {
            Rect column = SliceDrawerLayout.Column(Dw);
            Rect topBar = new Rect(12, 10, 560, 84);
            Rect nav = new Rect(Dw - 296, 10, 284, 44);
            Rect skillHud = new Rect(394, Dh - 118, 1132, 104);
            Rect tray = new Rect(skillHud.x + skillHud.width - 364, skillHud.y + 5, 354, 94);
            Vector2 inside = column.center;
            Vector2 outside = new Vector2(Dw * 0.5f, Dh * 0.5f); // 战斗区中心
            Assert.IsTrue(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, tray, column, inside),
                "抽屉内点必须吞掉世界点击");
            Assert.IsFalse(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, tray, column, outside),
                "抽屉外（战斗区中心）不得吞掉世界点击");
            Assert.IsTrue(SliceHud.BlocksWorldInput(true, false, topBar, nav, skillHud, tray, column, outside),
                "面板开启时全屏阻挡（既有行为保持）");
        }

        [Test]
        public void EquipSlot_CanonicalNumericIDs_Stable()
        {
            // S4-P2：旧四槽 numeric ID 不得漂移；Gloves/Belt 追加在 Count 前（工作令 三）
            Assert.AreEqual(6, (int)EquipSlot.Count);
            Assert.AreEqual(EquipSlot.Weapon, (EquipSlot)0);
            Assert.AreEqual(EquipSlot.Body, (EquipSlot)1);
            Assert.AreEqual(EquipSlot.Helmet, (EquipSlot)2);
            Assert.AreEqual(EquipSlot.Boots, (EquipSlot)3);
            Assert.AreEqual(EquipSlot.Gloves, (EquipSlot)4);
            Assert.AreEqual(EquipSlot.Belt, (EquipSlot)5);
        }
    }
}
