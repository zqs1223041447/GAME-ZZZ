using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 导演 2026-09-12 实测四项：多行描述换行高度、天赋树点击=绘制、宝石与背包同区、主城枢纽+功能型 NPC+进图。
    /// 全部驱动 shipped 函数，禁止第二套几何/进图。
    /// </summary>
    public sealed class DirectorPlaytestTests
    {
        const float Dw = 1920f;
        const float Dh = 1080f;

        [Test]
        public void DescriptionStyle_OverflowAndWraps()
        {
            GUIStyle st = SliceTooltipLayout.DescriptionStyle();
            Assert.IsTrue(st.wordWrap, "描述必须 wordWrap");
            Assert.AreEqual(TextClipping.Overflow, st.clipping, "描述不得 Clip 到单行高度");
        }

        [Test]
        public void EquipmentCard_HeightContainsEveryBodyLine()
        {
            var s = new SliceSession();
            ItemInstance it = default;
            it.Id = 7;
            it.Slot = EquipSlot.Weapon;
            it.Rarity = Rarity.Rare;
            it.SocketCount = 3;
            it.BaseName = "烬星长杖";
            it.AffixCount = 3;
            it.SetAffix(0, AffixId.IgniteFire, 0.20f, 0.15f);
            it.SetAffix(1, AffixId.PhysFire, 0.12f, 0.12f);
            it.SetAffix(2, AffixId.AccCrit, 30f, 0.25f);
            var card = SliceTooltipModel.ItemCard(it, s);
            Assert.Greater(card.Body.Length, 0);
            AssertCardContainsBody(card);
        }

        [Test]
        public void TreeNodeCard_WrappedStats_FitInsideCardHeight()
        {
            var s = new SliceSession();
            int wrapped = 0;
            for (int i = 0; i < PoeTree.Count && wrapped == 0; i++)
            {
                var card = SliceHud.TreeNodeCard(s, i);
                if (card.Body == null)
                    continue;
                for (int b = 0; b < card.Body.Length; b++)
                {
                    float need = SliceTooltipLayout.BodyLineHeight(card.Body[b]);
                    if (need > SliceTooltipLayout.BodyFont + 4f + 0.01f)
                    {
                        AssertCardContainsBody(card);
                        GUIStyle st = SliceTooltipLayout.DescriptionStyle();
                        Assert.AreEqual(TextClipping.Overflow, st.clipping);
                        wrapped++;
                        break;
                    }
                }
            }
            Assert.Greater(wrapped, 0, "真实天赋树必须存在至少一条会在 tooltip 内宽换行的描述");
        }

        [Test]
        public void EstimateWrappedHeight_ExceedsSingleLine_ForLongCopy()
        {
            string line = "该节点当前无法兑现任何效果（0 效果）并且这是一段足够长的装备或天赋描述用来强制换行显示完整字形不得裁切到单行";
            float single = SliceTooltipLayout.BodyFont + 4f;
            float need = SliceHud.EstimateWrappedHeight(line, SliceTooltipLayout.InnerW, SliceTooltipLayout.BodyFont);
            Assert.Greater(need, single, "长描述必须算出多于一行的高度");
            Assert.AreEqual(need, SliceTooltipLayout.BodyLineHeight(line), 0.01f);
            var card = SliceTooltipModel.TextCard("描述", line);
            Assert.GreaterOrEqual(SliceTooltipLayout.CardHeight(card), need);
        }

        static void AssertCardContainsBody(SliceTooltipModel.Card card)
        {
            float sum = 0f;
            if (card.Body != null)
            {
                for (int i = 0; i < card.Body.Length; i++)
                {
                    float lineH = SliceTooltipLayout.BodyLineHeight(card.Body[i]);
                    Assert.GreaterOrEqual(lineH, SliceTooltipLayout.BodyFont + 4f - 0.01f,
                        "每行高度不得小于单行字形高：" + card.Body[i]);
                    sum += lineH;
                }
            }
            float h = SliceTooltipLayout.CardHeight(card);
            Assert.GreaterOrEqual(h, sum, "卡片高度必须容纳全部正文换行");
            Assert.Greater(h, 15f * (card.Body == null ? 0 : card.Body.Length),
                "不得再按 15px 单行裁正文");
        }

        [Test]
        public void TreeClick_DrawnCentreHitsSameNode_AtDefaultMidAndMaxZoom()
        {
            int nodeId = PoeTree.StartIndex;
            Rect view = SliceHud.PoeTreeViewport(Dw, Dh);
            Assert.Greater(view.y, 0f, "视口必须让出标题条（这正是旧二次减原点的偏移）");
            float[] zooms =
            {
                SliceHud.DefaultTreeZoom,
                (SliceHud.DefaultTreeZoom + PoeTreeView.MaxZoom) * 0.5f,
                PoeTreeView.MaxZoom
            };
            PoeNode n = PoeTree.Get(nodeId);
            float ds = SliceHud.DesignScale(Dw, Dh);
            for (int z = 0; z < zooms.Length; z++)
            {
                float zoom = zooms[z];
                Vector2 pan = new Vector2(view.width * 0.5f - n.x * zoom, view.height * 0.5f - n.y * zoom);
                Vector2 drawn = SliceHud.TreeDrawnCentreDesign(nodeId, view, pan, zoom);
                Vector2 probe = SliceHud.TreeClickProbeFromDesign(drawn, view);
                Rect local = PoeTreeView.NodeRect(n, pan, zoom);
                Assert.AreEqual(local.x + local.width * 0.5f, probe.x, 0.01f, "探针必须等于绘制中心（组内） zoom=" + zoom);
                Assert.AreEqual(local.y + local.height * 0.5f, probe.y, 0.01f, "探针必须等于绘制中心（组内） zoom=" + zoom);
                Assert.AreNotEqual(probe.y, local.y + local.height * 0.5f - view.y, "禁止再减一次视口原点 zoom=" + zoom);
                int hit = SliceHud.TreeHitFromDesignPointer(drawn, view, pan, zoom, ds);
                Assert.AreEqual(nodeId, hit, "绘制中心必须命中同一节点 zoom=" + zoom);
            }
        }

        [Test]
        public void SharedBag_GemsOccupyInventoryCells_NoTrayBand()
        {
            Assert.Greater(SliceDrawerLayout.GemOccupantCount, 0);
            Assert.AreEqual(SliceDrawerLayout.GemOccupantCount + SliceRules.InventoryCap,
                SliceDrawerLayout.SharedBagCellCount);
            for (int i = 0; i < SliceDrawerLayout.GemOccupantCount; i++)
            {
                Assert.IsTrue(SliceDrawerLayout.CellIsGem(i), "前缀格必须是宝石 " + i);
                Assert.AreEqual((SupportId)(i + 1), SliceDrawerLayout.GemInCell(i));
            }
            Assert.IsFalse(SliceDrawerLayout.CellIsGem(SliceDrawerLayout.GemOccupantCount));
            Assert.AreEqual(0, SliceDrawerLayout.ItemIndexForCell(SliceDrawerLayout.GemOccupantCount));
            Assert.AreEqual(SupportId.None, SliceDrawerLayout.GemInCell(SliceDrawerLayout.GemOccupantCount));

            float dw = Dw, dh = Dh;
            var equip = SliceDrawerLayout.ShellSlot(5, dw, dh);
            var inv = SliceDrawerLayout.InvLabel(dw, dh);
            var view = SliceDrawerLayout.ShellInvView(dw, dh);
            Assert.LessOrEqual(equip.yMax, inv.y + 0.01f, "装备区之下直接是背包，不得再插托盘");
            Assert.LessOrEqual(inv.yMax, view.y + 0.01f);
            string src = ReadHudSource();
            Assert.IsFalse(src.Contains("DrawSupportTray"), "不得再绘制独立辅助宝石托盘");
            Assert.IsFalse(src.Contains("TrayArea"), "HUD 不得再引用托盘分区");
        }

        [Test]
        public void TownHub_IdentityAndNpcBinds_EnterMapViaTryEnterMap()
        {
            Assert.AreEqual("ember-town", TownHub.Id);
            Assert.AreEqual("烬城", TownHub.DisplayName);
            Assert.AreEqual("ash-court", TownHub.MapId);
            Assert.AreEqual("灰烬庭院", TownHub.MapDisplayName);
            Assert.AreEqual(3, TownHub.NpcCount);
            Assert.AreEqual(TownNpcKind.Stash, TownHub.NpcKind(0));
            Assert.AreEqual(TownNpcKind.Crafter, TownHub.NpcKind(1));
            Assert.AreEqual(TownNpcKind.Waystone, TownHub.NpcKind(2));

            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            SliceSession s = sim.Session;
            Assert.IsTrue(TownHub.IsHubActive(s));
            Assert.AreEqual(MapState.Town, s.State);

            s.BagOpen = false;
            string err;
            Assert.IsTrue(TownHub.ApplyNpc(s, 0, out err), err);
            Assert.IsTrue(s.BagOpen, "仓库管事必须打开背包");
            Assert.AreEqual(MapState.Town, s.State);

            Assert.IsTrue(TownHub.ApplyNpc(s, 1, out err), err);
            Assert.AreEqual(SlicePanel.Craft, s.Panel, "工匠必须打开制作");
            Assert.AreEqual(MapState.Town, s.State);

            s.Panel = SlicePanel.None;
            Assert.IsTrue(TownHub.ApplyNpc(s, 2, out err), err);
            Assert.AreEqual(SlicePanel.Map, s.Panel, "地图官必须打开地图面板");
            Assert.AreEqual(MapState.Town, s.State, "打开地图面板不得直接进图");

            Vector2 stash = TownHub.NpcRect(0, Dw, Dh).center;
            Assert.AreEqual(0, TownHub.HitNpc(stash, Dw, Dh));
            Assert.AreEqual(-1, TownHub.HitNpc(new Vector2(4f, 4f), Dw, Dh));

            int spawnedBefore = sim.SpawnedCount;
            Assert.IsTrue(TownHub.TryEnterMapFromHub(s, sim, out err), err);
            Assert.AreEqual(MapState.InMap, s.State);
            Assert.IsTrue(s.BuildLocked);
            Assert.Greater(sim.Dummies.AliveCount, 0, "进图必须走既有 TryEnterMap 生成灰烬庭院");
            Assert.Greater(sim.SpawnedCount, spawnedBefore);
            Assert.IsFalse(TownHub.IsHubActive(s));
            Assert.IsFalse(TownHub.TryEnterMapFromHub(s, sim, out err), "图内不得再从枢纽进图");
        }

        static string ReadHudSource()
        {
            return System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Runtime/Core/Gameplay/SliceHud.cs"));
        }
    }
}
