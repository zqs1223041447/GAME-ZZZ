using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Phase 5 Art Trial R5（工作令 S3-P5-ART-R5-VISUAL-FEEDBACK-PARITY）：真实运行验证
    /// canonical gameplay Ignite/Hit truth 到达 Presenter 并驱动反馈 tint。
    /// Ignite 经 canonical 入口 ApplyIgnite（ArenaSim 真实点燃路径同款，deterministic setup path）触发；
    /// 所有时间窗按模拟值（IgniteRemain/HitFlash）判定，不按真实秒数假设；
    /// 硬断言：gameplay 真相（DoT 伤害/存活/时长）不因表现层改变。
    /// </summary>
    public sealed class EnemyVisualFeedbackPlayModeTests
    {
        static int FindFirstKindSlot(ArenaDirector director, EnemyKind kind)
        {
            Dummy[] items = director.Sim.Dummies.Items;
            for (int i = 0; i < items.Length; i++)
                if (items[i].Occupied && items[i].Alive && items[i].Kind == kind)
                    return i;
            return -1;
        }

        static IEnumerator TickFrames(ArenaDirector director, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                director.Sim.Tick(0.02f, PlayerCommand.None());
                yield return null;
            }
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
        public IEnumerator Troll_Ignite_ReachesPresenter_AndDoTUnchanged()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            string err;
            Assert.IsTrue(director.Sim.Session.TryEnterMap(director.Sim, out err), "进图失败：" + err);
            yield return null;

            int bruteSlot = FindFirstKindSlot(director, EnemyKind.Brute);
            Assert.GreaterOrEqual(bruteSlot, 0, "地图必须生成 Brute");
            var presenter = director.EnemyVisualFor(bruteSlot);
            Assert.IsNotNull(presenter, "Brute 槽位必须挂 Troll 视觉");
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "初始=Normal（原色）");

            // canonical Ignite 入口（与 ArenaSim 真实点燃成功后同一条 ApplyIgnite 路径），长时长保证持续窗口
            int hp0 = director.Sim.Dummies.Items[bruteSlot].Hp;
            director.Sim.Dummies.ApplyIgnite(bruteSlot, 1f, 60f);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "canonical Ignite truth 必须到达 Presenter");
            Assert.Greater(director.Sim.Dummies.Items[bruteSlot].IgniteRemain, 0f, "gameplay Ignite 状态必须真实存在");

            // 持续窗口：等首跳 DoT（1dmg/模拟秒）——DoT 经 ApplyHit 造成 HitFlash，Hit 优先级应真实出现
            yield return TickUntil(director, () => director.Sim.Dummies.Items[bruteSlot].Hp < hp0, 400);
            Assert.Greater(director.Sim.Dummies.Items[bruteSlot].IgniteRemain, 0f, "长时长 Ignite 期间必须保持 IgniteRemain>0");
            Assert.AreNotEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "DoT 跳伤瞬间必须表现为 Hit（优先级）或 Ignite，不得回 Normal");
            Assert.Less(director.Sim.Dummies.Items[bruteSlot].Hp, hp0, "Ignite DoT 必须照常扣血（表现层不得改变 DoT）");
            Assert.IsTrue(director.Sim.Dummies.Items[bruteSlot].Alive, "DoT 不得因表现层加速致死");

            // HitFlash 衰减完成后（Ignite 仍在燃烧）：必须回到 Ignite
            yield return TickUntil(director, () => director.Sim.Dummies.Items[bruteSlot].HitFlash <= 0.02f, 200);
            Assert.Greater(director.Sim.Dummies.Items[bruteSlot].IgniteRemain, 0f, "长时长 Ignite 仍应燃烧");
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "HitFlash 衰减后必须回到 Ignite");

            // 过期恢复：以 canonical ApplyIgnite 收窄时长，等待 IgniteRemain 由 gameplay 归零 → 精确 Normal
            director.Sim.Dummies.ApplyIgnite(bruteSlot, 1f, 0.15f);
            yield return TickUntil(director, () => director.Sim.Dummies.Items[bruteSlot].IgniteRemain <= 0f, 200);
            Assert.LessOrEqual(director.Sim.Dummies.Items[bruteSlot].IgniteRemain, 0f, "Ignite 时长由 gameplay 决定，必须照常到期");
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "Ignite 过期必须回 Normal");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FireLion_HitPriority_ThenIgnite_ThenNormal()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            string err;
            Assert.IsTrue(director.Sim.Session.TryEnterMap(director.Sim, out err), "进图失败：" + err);
            yield return null;

            int stingerSlot = FindFirstKindSlot(director, EnemyKind.Stinger);
            Assert.GreaterOrEqual(stingerSlot, 0, "地图必须生成 Stinger");
            var presenter = director.EnemyVisualFor(stingerSlot);
            Assert.IsNotNull(presenter, "Stinger 槽位必须挂 FireLion 视觉");

            director.Sim.Dummies.ApplyIgnite(stingerSlot, 1f, 60f);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState);

            // 非致死真实命中（ApplyHit=canonical HitFlash 来源）：Hit 必须优先于 Ignite
            director.Sim.Dummies.ApplyHit(stingerSlot, 1, null, false);
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;
            Assert.AreEqual(EnemyFeedbackState.Hit, presenter.LastFeedbackState, "Hit+Ignite 同时存在必须显示 Hit");
            Assert.Greater(director.Sim.Dummies.Items[stingerSlot].Hp, 0, "非致死命中必须保持存活真相");
            Assert.IsTrue(director.Sim.Dummies.Items[stingerSlot].Alive);

            // HitFlash 0.20s 窗口结束后（flash 由 gameplay 衰减到阈值内）：Ignite 仍存在 → 回 Ignite
            yield return TickUntil(director, () => director.Sim.Dummies.Items[stingerSlot].HitFlash <= 0.02f, 200);
            Assert.LessOrEqual(director.Sim.Dummies.Items[stingerSlot].HitFlash, 0.02f, "HitFlash 必须照常衰减");
            Assert.Greater(director.Sim.Dummies.Items[stingerSlot].IgniteRemain, 0f, "长时长 Ignite 必须仍在燃烧");
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "Hit 结束后必须自动回到 Ignite");

            // Ignite 到期 → Normal
            director.Sim.Dummies.ApplyIgnite(stingerSlot, 1f, 0.15f);
            yield return TickUntil(director, () => director.Sim.Dummies.Items[stingerSlot].IgniteRemain <= 0f, 200);
            Assert.LessOrEqual(director.Sim.Dummies.Items[stingerSlot].IgniteRemain, 0f, "Ignite 时长照常到期");
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "Ignite 过期必须回 Normal");
            Assert.IsTrue(director.Sim.Dummies.Items[stingerSlot].Alive, "全程存活真相不变");
        }
    }
}
