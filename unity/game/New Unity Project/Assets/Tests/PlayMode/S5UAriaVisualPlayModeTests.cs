using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// S5U 导演输入（Aria）HUD 视觉验证：启动 ArenaDirector → 打开 Build 抽屉（装备槽可见）→
    /// ScreenCapture 落盘（gate 产物目录），断言文件生成。人类评审/自动化读图核对：
    /// 生命/法力球金环覆层 + 装备槽 Aria 图标 + 整体无布局回归。
    /// </summary>
    public sealed class S5UAriaVisualPlayModeTests
    {
        [UnityTest]
        public IEnumerator Aria_Hud_WithDrawer_Renders_And_Captures()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            yield return null;

            Assert.IsNotNull(director.Sim, "ArenaDirector 启动失败");
            // 多件同类装备演示（导演指令 2026-09-10：拥挤网格排布验证）：
            // 18 件注入（每槽 3 件、Rare/Ordinary 混合）+ 初始 4 件 → 22 件=3 行满幅；装备 2 件验证"已装备"角标
            var session = director.Sim.Session;
            var rng = new SeededRng(20260910u);
            var slots = new[] { EquipSlot.Weapon, EquipSlot.Helmet, EquipSlot.Body, EquipSlot.Gloves, EquipSlot.Boots, EquipSlot.Belt };
            for (int i = 0; i < 18; i++)
            {
                var slot = slots[i % 6];
                var rarity = (i % 3 == 0) ? Rarity.Rare : Rarity.Ordinary;
                session.AddItem(session.RollItem(slot, rarity, rng,
                    SliceSession.SocketsFor(slot), SliceSession.ItemBaseName(slot)));
            }
            string equipErr;
            Assert.IsTrue(session.TryEquip(4, out equipErr), "演示装备武器失败: " + equipErr); // 第 5 件=首个注入 Weapon
            Assert.IsTrue(session.TryEquip(5, out equipErr), "演示装备头盔失败: " + equipErr);
            Assert.GreaterOrEqual(session.InventoryCount, 22, "演示注入后库存数不足");
            session.Panel = SlicePanel.Build; // 抽屉（装备槽+背包网格）入画
            yield return null;
            yield return null;
            yield return null;
            Assert.GreaterOrEqual(director.Sim.Session.InventoryCount, 22, "帧渲染后库存数回退（Session 被重置?）");
            var all = Object.FindObjectsByType<ArenaDirector>(FindObjectsSortMode.None);
            foreach (var d in all)
                Debug.Log($"[S5UAriaTest] director={d.GetInstanceID()} count={d.Sim.Session.InventoryCount} session={d.Sim.Session.GetHashCode()}");
            Assert.AreEqual(1, all.Length, "场景内应只有本测试的 ArenaDirector，实际 " + all.Length);

            var dir = Path.Combine(Path.GetTempPath(), "GAME-ZZZ-UnattendedGate");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "s5u_aria_hud.png");
            if (File.Exists(path))
                File.Delete(path);
            ScreenCapture.CaptureScreenshot(path, 1);
            // CaptureScreenshot 在帧末执行；批处理禁 WaitForEndOfFrame，用整帧等待让 OnGUI 绘制并被捕获
            for (int i = 0; i < 12; i++)
                yield return null;

            // 批处理无 Game View（Screen=640×480、OnGUI 不入后缓冲）时截图不可得——如实跳过而非判红；
            // 一旦落盘则仍按“非空画面”硬断言。见 S5U_WO_04 §环境披露。
            if (!File.Exists(path))
                Assert.Ignore("批处理环境不产出 HUD 截图（无 Game View）：" + path);
            Assert.Greater(new FileInfo(path).Length, 10240, "HUD 截图过小，疑似空画面");

            Object.Destroy(go);
            yield return null;
        }
    }
}
