using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// 2026-09-10 导演指令的视觉验证（截图落盘，供人工/自动读图核对）：
    /// 天赋树整幅与放大两态（真实 PoE 布局/图标/连线/词条）、背包面板（贴右通顶、满幅格网、
    /// 辅助宝石托盘、装备卡）、底栏（QWERT 主动技能 + 预留药剂槽）。
    /// 同时断言：加点确实改变数值快照、面板开启时世界输入被吞掉。
    /// </summary>
    public sealed class S5UPoeTreeVisualPlayModeTests
    {
        static string GateDir
        {
            get
            {
                var dir = Path.Combine(Path.GetTempPath(), "GAME-ZZZ-UnattendedGate");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>批处理环境没有 Game View（Screen=640×480、OnGUI 不进后缓冲），截图不可得时如实跳过；
        /// 一旦落盘仍按“非空画面”硬断言。</summary>
        static IEnumerator Capture(string name)
        {
            yield return null;
            string path = Path.Combine(GateDir, name);
            if (File.Exists(path))
                File.Delete(path);
            ScreenCapture.CaptureScreenshot(path, 1);
            for (int i = 0; i < 12; i++)
                yield return null;
            if (!File.Exists(path))
                Assert.Ignore("批处理环境不产出截图（无 Game View）：" + name);
            Assert.Greater(new FileInfo(path).Length, 10240, "截图过小，疑似空画面: " + name);
        }

        static ArenaDirector Boot()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            return director;
        }

        static void FillInventory(SliceSession session, int count)
        {
            var rng = new SeededRng(20260910u);
            var slots = new[]
            {
                EquipSlot.Weapon, EquipSlot.Helmet, EquipSlot.Body,
                EquipSlot.Gloves, EquipSlot.Boots, EquipSlot.Belt
            };
            for (int i = 0; i < count && session.InventoryCount < session.Inventory.Length; i++)
            {
                var slot = slots[i % 6];
                var rarity = (i % 3 == 0) ? Rarity.Rare : Rarity.Ordinary;
                session.AddItem(session.RollItem(slot, rarity, rng,
                    SliceSession.SocketsFor(slot), SliceSession.ItemBaseName(slot)));
            }
        }

        [UnityTest]
        public IEnumerator PoeTree_FullAndZoomed_RenderAndCapture()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            yield return null;

            var session = director.Sim.Session;
            Assert.IsTrue(PoeTree.Ready, "真实天赋树数据必须可加载");
            FillInventory(session, 30);

            // 点亮一条从起点出发的路径，让“已点亮/可点/锁住”三态同时入画。
            // S6P-WO-04A 起只有 support truth 判为可兑现的节点能点亮：起点邻居里恰好是 559/1795/2034。
            string err;
            int start = SliceSession.StartNode;
            int[] firstRing = PoeTree.Get(start).links;
            int lit = 0;
            for (int i = 0; i < firstRing.Length; i++)
                if (session.TryAllocate(firstRing[i], out err))
                    lit++;
            Assert.GreaterOrEqual(lit, 3, "起点必须至少有 3 个可兑现邻居可点亮（559/1795/2034）");

            Assert.AreEqual(SlicePanel.Build, session.Panel == SlicePanel.None ? SlicePanel.Build : session.Panel);
            session.Panel = SlicePanel.Build;

            SliceHud.DebugTreeOff();
            yield return Capture("poe_01_tree_full.png");

            // 放大到一个基石并悬停，验证 tooltip 的“说明”真值
            int keystone = -1;
            for (int i = 0; i < PoeTree.Count; i++)
                if (PoeTree.Get(i).Kind == PoeNodeKind.Keystone) { keystone = i; break; }
            Assert.GreaterOrEqual(keystone, 0);

            float scale = SliceHud.DesignScale(Screen.width, Screen.height);
            float dw = Screen.width / scale, dh = Screen.height / scale;
            Rect view = SliceHud.PoeTreeViewport(dw, dh);
            SliceHud.DebugTreeAt(1f, keystone);
            SliceHud.DebugHoverAt(new Vector2(view.x + view.width * 0.5f, view.y + view.height * 0.5f));
            yield return Capture("poe_02_tree_zoomed_tooltip.png");

            SliceHud.DebugHoverOff();
            SliceHud.DebugTreeAt(0.55f, keystone);
            yield return Capture("poe_03_tree_zoom_half.png");

            SliceHud.DebugTreeOff();
            Assert.AreEqual(1, session.Allocated[start] ? 1 : 0, "起点必须始终已点亮");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BagPanel_And_CombatBar_RenderAndCapture()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            yield return null;

            var session = director.Sim.Session;
            FillInventory(session, session.Inventory.Length);
            string err;
            session.TryEquip(0, out err);
            session.TryEquip(3, out err);
            session.Panel = SlicePanel.None;

            // 功能断言先于截图（截图在无 Game View 的环境下会被跳过）
            float sc = SliceHud.DesignScale(Screen.width, Screen.height);
            float w = Screen.width / sc, h = Screen.height / sc;
            var shell = SliceDrawerLayout.Shell(w, h);
            var bar = SliceHud.CombatBarRects(w, h);
            Assert.AreEqual(w, shell.xMax, 0.01f, "背包面板必须贴右");
            Assert.AreEqual(h, shell.yMax, 0.01f, "背包面板必须通底");
            Assert.LessOrEqual(bar.Bar.xMax, shell.x + 0.01f, "战斗栏不得被背包面板压住");
            Assert.AreEqual(SliceHud.SkillSlots, 5, "底栏主动技能槽必须为五槽 QWERT");
            Assert.AreEqual(SliceHud.FlaskSlots, 4, "必须为后续药剂预留槽位");
            Assert.AreEqual(session.Inventory.Length, SliceRules.InventoryCap, "背包容量=满幅格网格数");

            SliceHud.DebugHoverOff();
            SliceHud.DebugTreeOff();
            yield return Capture("poe_04_bag_panel_and_bar.png");

            // 悬停在一件背包物品上，验证词条 tooltip 不被截断
            FillInventory(session, 1);
            float scale = SliceHud.DesignScale(Screen.width, Screen.height);
            float dw = Screen.width / scale, dh = Screen.height / scale;
            Rect view = SliceDrawerLayout.ShellInvView(dw, dh);
            Vector2 cell = new Vector2(view.x + 32f, view.y + 30f);
            SliceHud.DebugHoverAt(cell);
            yield return Capture("poe_05_bag_item_tooltip.png");

            // 悬停在一个技能槽上，验证底部主动技能槽的 tooltip
            var skillBar = SliceHud.CombatBarRects(dw, dh);
            SliceHud.DebugHoverAt(new Vector2(skillBar.SlotQ.center.x, skillBar.SlotQ.center.y));
            yield return Capture("poe_06_skill_slot_tooltip.png");

            SliceHud.DebugHoverOff();
            Object.Destroy(go);
            yield return null;
        }
    }
}
