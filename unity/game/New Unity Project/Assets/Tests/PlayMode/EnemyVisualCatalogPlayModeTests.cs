using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Phase 5 Art Trial R4（S3-P5-ART-R4）：第二敌人视觉（Stinger→FireLion）真实运行验证。
    /// 只验证「状态被正确触发/选择」；gameplay 真相（存活数/死亡回收）不变是硬断言。
    /// </summary>
    public sealed class EnemyVisualCatalogPlayModeTests
    {
        static int FindFirstKindSlot(ArenaDirector director, EnemyKind kind)
        {
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
                if (items[i].Occupied && items[i].Alive && items[i].Kind == kind)
                    return i;
            return -1;
        }

        [UnityTest]
        public IEnumerator EnemyVisual_StingerSlots_PresentLion_AndBruteStaysTroll()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            string err;
            Assert.IsTrue(director.Sim.Session.TryEnterMap(director.Sim, out err), "进图失败：" + err);
            yield return null; // SyncViews 先跑一帧

            int stingerSlot = FindFirstKindSlot(director, EnemyKind.Stinger);
            int bruteSlot = FindFirstKindSlot(director, EnemyKind.Brute);
            int ashlingSlot = FindFirstKindSlot(director, EnemyKind.Ashling);
            Assert.GreaterOrEqual(stingerSlot, 0, "地图必须生成 Stinger");
            Assert.GreaterOrEqual(bruteSlot, 0, "地图必须生成 Brute");
            Assert.GreaterOrEqual(ashlingSlot, 0, "地图必须生成 Ashling");

            // ① Stinger 槽位挂第二视觉（FireLion）；Brute 槽位仍是 Troll；Ashling 槽位保持 placeholder
            //    （R8：候选 GargoyleVisual 被 Formal Art Performance Gate 阻断，按工作令恢复 placeholder）
            var stingerPresenter = director.EnemyVisualFor(stingerSlot);
            var brutePresenter = director.EnemyVisualFor(bruteSlot);
            Assert.IsNotNull(stingerPresenter, "Stinger 槽位必须挂 presenter（R4 第二视觉）");
            Assert.IsNotNull(brutePresenter, "Brute 槽位必须挂 presenter（R3 契约不变）");
            Assert.IsNull(director.EnemyVisualFor(ashlingSlot), "Ashling 保持基元 placeholder（R8 Art Gate 阻断后回退）");
            Assert.IsTrue(stingerPresenter.Root.activeInHierarchy, "Stinger 视觉 root 必须激活");
            Assert.AreEqual(Vector3.one, stingerPresenter.Root.transform.parent.localScale, "gameplay root scale 必须保持 1");

            // ② 移动 → Run 视觉状态（真实 TickAi 信号，非逐帧精确断言）
            bool sawRun = false;
            for (int i = 0; i < 120 && !sawRun; i++)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                var p = director.EnemyVisualFor(stingerSlot);
                if (p != null && p.LastRequestedState == "Run")
                    sawRun = true;
            }
            Assert.IsTrue(sawRun, "Stinger 逼近阶段必须出现 Run 视觉状态");

            // ③ 死亡：gameplay 真相照旧，视觉播 Death，回收后视觉隐藏
            int aliveBefore = director.Sim.Dummies.AliveCount;
            director.Sim.Dummies.ApplyHit(stingerSlot, 999999, null, false);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(aliveBefore - 1, director.Sim.Dummies.AliveCount, "死亡真相不得因视觉接入改变");
            var deadPresenter = director.EnemyVisualFor(stingerSlot);
            if (deadPresenter != null)
                Assert.AreEqual("Death", deadPresenter.LastRequestedState, "死亡必须由真实死亡状态驱动");

            double recycleEnd = Time.realtimeSinceStartup + 5.0;
            while (Time.realtimeSinceStartup < recycleEnd)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                if (!director.Sim.Dummies.TryGet(stingerSlot, out _))
                    break;
            }
            Assert.IsFalse(director.Sim.Dummies.TryGet(stingerSlot, out _), "DeathRecycle 0.40s 后必须照常回收");
            Assert.IsFalse(deadPresenter.Root.activeInHierarchy, "回收后视觉必须隐藏");

            Object.Destroy(go);
            yield return null;
        }
    }
}
