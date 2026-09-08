using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Phase 5 Art Trial R6（工作令 S3-P5-ART-R6-WARDEN-BRUCE-INTEGRATION）：第三正式敌人视觉（Warden→BruceVisual）
    /// 真实运行验证：挂载/placeholder 不重复显示/移动与攻击 truth 触发/R5 反馈 parity/gameplay 真相不变。
    /// </summary>
    public sealed class BruceVisualPlayModeTests
    {
        static int FindWardenSlot(ArenaDirector director)
        {
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
                if (items[i].Occupied && items[i].Alive && items[i].Kind == EnemyKind.Warden)
                    return i;
            return -1;
        }

        static IEnumerator TickUntil(ArenaDirector director, System.Func<bool> done, int maxFrames)
        {
            for (int i = 0; i < maxFrames; i++)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
                if (done()) break;
            }
        }

        [UnityTest]
        public IEnumerator Warden_BruceVisual_Integration_AndFeedbackParity()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            string err;
            Assert.IsTrue(director.Sim.Session.TryEnterMap(director.Sim, out err), "进图失败：" + err);
            yield return null;

            int wardenSlot = FindWardenSlot(director);
            Assert.GreaterOrEqual(wardenSlot, 0, "地图必须生成 Warden（每图 1 只）");

            // ① 正式视觉挂载：Bruce presenter + placeholder 基元渲染器隐藏（一实体一视觉身份）
            var presenter = director.EnemyVisualFor(wardenSlot);
            Assert.IsNotNull(presenter, "Warden 槽位必须挂 BruceVisual（R6 第三正式视觉）");
            Assert.IsTrue(presenter.Root.activeInHierarchy, "Bruce 视觉 root 必须激活");
            var primitiveRenderer = presenter.Root.transform.parent.GetComponent<MeshRenderer>();
            if (primitiveRenderer != null)
                Assert.IsFalse(primitiveRenderer.enabled, "placeholder 基元渲染器必须隐藏（不得 Bruce+placeholder 重叠）");
            Assert.AreEqual(Vector3.one, presenter.Root.transform.parent.localScale, "gameplay root scale=1");

            // ② Move truth → Run 视觉状态（Warden 逼近玩家）
            yield return TickUntil(director, () =>
            {
                var p = director.EnemyVisualFor(wardenSlot);
                return p != null && p.LastRequestedState == "Run";
            }, 600);
            var runPresenter = director.EnemyVisualFor(wardenSlot);
            Assert.IsNotNull(runPresenter, "Run 等待期间 presenter 不得丢失");
            Assert.AreEqual("Run", runPresenter.LastRequestedState, "Warden 移动必须出现 Run 视觉状态");

            // ③ 清场（canonical ApplyHit，QA 同款确定性路径）：Warden 是唯一 melee 威胁后再验证攻击 truth；
            //    不清场时 Warden 会被先到怪群挡在停步距离外（真实行为，非缺陷）
            int culled = 0;
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Alive && i != wardenSlot) { director.Sim.Dummies.ApplyHit(i, 999999, null, false); culled++; }
            }
            Assert.Greater(culled, 0, "必须存在可清场的其他敌人");
            Assert.IsTrue(director.Sim.Dummies.Items[wardenSlot].Alive, "Warden 自身必须存活");

            // ④ R5 反馈 parity 自动生效于 Bruce（无 Bruce 特例分支）
            int hp0 = director.Sim.Dummies.Items[wardenSlot].Hp;
            director.Sim.Dummies.ApplyIgnite(wardenSlot, 1f, 60f);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(EnemyFeedbackState.Ignite, runPresenter.LastFeedbackState, "canonical Ignite truth 必须到达 Bruce 反馈");

            director.Sim.Dummies.ApplyHit(wardenSlot, 1, null, false);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(EnemyFeedbackState.Hit, runPresenter.LastFeedbackState, "Hit 必须优先于 Ignite");
            Assert.Greater(director.Sim.Dummies.Items[wardenSlot].Hp, 0, "非致死命中保持存活真相");

            yield return TickUntil(director, () => director.Sim.Dummies.Items[wardenSlot].HitFlash <= 0.02f, 200);
            Assert.AreEqual(EnemyFeedbackState.Ignite, runPresenter.LastFeedbackState, "Hit 结束回 Ignite");

            director.Sim.Dummies.ApplyIgnite(wardenSlot, 1f, 0.15f);
            yield return TickUntil(director, () => director.Sim.Dummies.Items[wardenSlot].IgniteRemain <= 0f, 200);
            Assert.AreEqual(EnemyFeedbackState.Normal, runPresenter.LastFeedbackState, "Ignite 过期回精确 Normal");
            Assert.IsTrue(director.Sim.Dummies.Items[wardenSlot].Alive, "反馈全程存活真相不变");

            // ⑤ Attack truth → Attack 视觉状态（AttackExecutions 观察计数递增）
            int attack0 = director.Sim.Dummies.Items[wardenSlot].AttackExecutions;
            yield return TickUntil(director, () =>
            {
                var p = director.EnemyVisualFor(wardenSlot);
                return p != null && director.Sim.Dummies.Items[wardenSlot].Alive
                    && director.Sim.Dummies.Items[wardenSlot].AttackExecutions > attack0
                    && p.LastRequestedState == "Attack";
            }, 1500);
            var attackPresenter = director.EnemyVisualFor(wardenSlot);
            Assert.IsNotNull(attackPresenter, "Attack 等待期间 presenter 不得丢失");
            Assert.Greater(director.Sim.Dummies.Items[wardenSlot].AttackExecutions, attack0, "Warden 必须真实执行攻击");
            Assert.AreEqual("Attack", attackPresenter.LastRequestedState, "真实攻击执行必须触发 Attack 视觉状态");

            // ⑥ Death truth：gameplay 真相（AliveCount-1）不变，视觉播 Death
            int aliveBefore = director.Sim.Dummies.AliveCount;
            director.Sim.Dummies.ApplyHit(wardenSlot, 999999, null, false);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(aliveBefore - 1, director.Sim.Dummies.AliveCount, "死亡真相不得因视觉接入改变");
            var deadPresenter = director.EnemyVisualFor(wardenSlot);
            if (deadPresenter != null)
                Assert.AreEqual("Death", deadPresenter.LastRequestedState, "死亡必须由真实死亡状态驱动");

            Object.Destroy(go);
            yield return null;
        }
    }
}
