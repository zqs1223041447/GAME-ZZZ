using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 右侧背包面板契约（2026-09-10 导演指令重排：贴右/通顶；原「居中列」几何已由
    /// SliceBagPanelTests 接管细部断言）。此处保留与呈现格局无关的稳定合同：
    /// 展示顺序双射、内容面板不压面板/底栏、设计缩放、世界输入阻挡、EquipSlot canonical 数值。
    /// </summary>
    public class SliceDrawerTests
    {
        const float Dw = 1920f;
        const float Dh = 1080f;

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
        public void BuildAndCraftPanels_LeftOfPanel_AndNotCoveringBottomBar()
        {
            float panelX = SliceDrawerLayout.Shell(Dw, Dh).x;
            Rect build = SliceDrawerLayout.BuildPanel(Dw);
            Rect craft = SliceDrawerLayout.CraftPanel(Dw);
            Assert.LessOrEqual(build.xMax, panelX, "Build 面板不得覆盖背包面板");
            Assert.LessOrEqual(craft.xMax, panelX, "Craft 面板不得覆盖背包面板");
            // 底栏关键区：底栏外框顶 = dh - (orbD+16) - 28
            float bottomBarTop = Dh - 172f;
            Assert.LessOrEqual(build.yMax, bottomBarTop, "Build 面板不得压底栏关键区");
            Assert.LessOrEqual(craft.yMax, bottomBarTop, "Craft 面板不得压底栏关键区");
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
        public void BlocksWorldInput_PanelInsideTrue_OutsideFalse()
        {
            Rect panel = SliceDrawerLayout.Shell(Dw, Dh);
            Rect topBar = new Rect(12, 10, 620, 84);
            Rect nav = new Rect(642, 10, 268, 40);
            Rect skillHud = new Rect(200, Dh - 172, 1028, 172);
            Rect tray = SliceDrawerLayout.TrayArea(Dw, Dh);
            Vector2 inside = panel.center;
            Vector2 outside = new Vector2(Dw * 0.4f, Dh * 0.5f); // 战斗区中心（面板之外）
            Assert.IsTrue(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, tray, panel, inside),
                "面板内点必须吞掉世界点击");
            Assert.IsFalse(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, tray, panel, outside),
                "面板外（战斗区中心）不得吞掉世界点击");
            Assert.IsTrue(SliceHud.BlocksWorldInput(true, false, topBar, nav, skillHud, tray, panel, outside),
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
