using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Phase 5 Art Trial R3（S3-P5-ART-R3）：Troll 正式敌人视觉的真实运行验证。
    /// 只验证「状态被正确触发/选择」，不做脆弱的逐帧断言；gameplay 真相（存活数/死亡回收）不变是硬断言。
    /// </summary>
    public sealed class TrollRuntimeVisualPlayModeTests
    {
        static int FindFirstBruteSlot(ArenaDirector director)
        {
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
                if (items[i].Occupied && items[i].Alive && items[i].Kind == EnemyKind.Brute)
                    return i;
            return -1;
        }

        [UnityTest]
        public IEnumerator EnemyVisual_BruteSlots_PresentTrollDrivenByGameplayStates()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            string err;
            Assert.IsTrue(director.Sim.Session.TryEnterMap(director.Sim, out err), "进图失败：" + err);
            int aliveAfterSpawn = director.Sim.Dummies.AliveCount;
            int bruteSlot = FindFirstBruteSlot(director);
            Assert.GreaterOrEqual(bruteSlot, 0, "地图必须生成 Brute（默认敌人视觉目标）");
            yield return null; // SyncViews 先跑一帧

            // ① Brute 槽位挂上正式视觉；基元 placeholder 隐藏；gameplay root scale 不被美术 scale 污染
            EnemyVisualPresenter presenter = director.EnemyVisualFor(bruteSlot);
            Assert.IsNotNull(presenter, "Brute 槽位必须挂 EnemyVisualPresenter");
            Assert.IsTrue(presenter.Root.activeInHierarchy, "视觉 root 必须激活");
            Assert.AreEqual(Vector3.one, presenter.Root.transform.parent.localScale, "gameplay root scale 必须保持 1");
            MeshRenderer placeholder = presenter.Root.transform.parent.GetComponent<MeshRenderer>();
            Assert.IsNotNull(placeholder);
            Assert.IsFalse(placeholder.enabled, "基元 placeholder 必须隐藏");

            // ② 移动 → Run 视觉状态（真实 TickAi 信号驱动，非逐帧精确断言）
            bool sawRun = false;
            for (int i = 0; i < 120 && !sawRun; i++)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                if (director.EnemyVisualFor(bruteSlot) != null &&
                    director.EnemyVisualFor(bruteSlot).LastRequestedState == "Run")
                    sawRun = true;
            }
            Assert.IsTrue(sawRun, "Brute 逼近阶段必须出现 Run 视觉状态");

            // ③ 攻击 → Attack 视觉状态（唯一 canonical 信号=AttackExecutions 执行序号，进图后才结算）
            bool sawAttack = false;
            for (int i = 0; i < 400 && !sawAttack; i++)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                for (int s = 0; s < CombatRules.DummyPoolSize && !sawAttack; s++)
                {
                    EnemyVisualPresenter p = director.EnemyVisualFor(s);
                    if (p != null && p.Root != null && p.Root.activeInHierarchy &&
                        p.LastRequestedState == "Attack")
                        sawAttack = true;
                }
            }
            Assert.IsTrue(sawAttack, "Brute 到达攻击距离后必须出现 Attack 视觉状态（真实攻击执行驱动）");

            // ④ 死亡：gameplay 真相照旧（ApplyHit → AliveCount-1），视觉播 Death，回收后视觉隐藏+placeholder 恢复
            int aliveBefore = director.Sim.Dummies.AliveCount;
            director.Sim.Dummies.ApplyHit(bruteSlot, 999999, null, false);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(aliveBefore - 1, director.Sim.Dummies.AliveCount, "死亡真相不得因视觉接入改变");
            EnemyVisualPresenter deadPresenter = director.EnemyVisualFor(bruteSlot);
            if (deadPresenter != null)
                Assert.AreEqual("Death", deadPresenter.LastRequestedState, "死亡必须由真实死亡状态驱动");

            double recycleEnd = Time.realtimeSinceStartup + 5.0;
            while (Time.realtimeSinceStartup < recycleEnd)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                if (!director.Sim.Dummies.TryGet(bruteSlot, out _))
                    break;
            }
            Assert.IsFalse(director.Sim.Dummies.TryGet(bruteSlot, out _), "DeathRecycle 0.40s 后必须照常回收");
            Assert.IsFalse(deadPresenter.Root.activeInHierarchy, "回收后视觉必须隐藏");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyVisual_DummyKind_KeepsPrimitiveVisual()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            // 竞技场模式（EnemyKind.Dummy）维持基元视觉：Harness 密度档路径不因本轮改变
            director.Sim.SpawnDummies(4, CombatRules.ArenaSeed);
            yield return null;
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].Occupied)
                    continue;
                Assert.IsNull(director.EnemyVisualFor(i), "Dummy kind 不得挂 presenter（§8 只换一个 archetype）");
            }

            Object.Destroy(go);
            yield return null;
        }
    }
}
