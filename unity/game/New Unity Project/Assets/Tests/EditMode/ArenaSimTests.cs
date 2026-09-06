using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class ArenaSimTests
    {
        const float Dt = 0.02f;

        [Test]
        public void ClickMove_ReachesDestination()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Tick(Dt, PlayerCommand.MoveTo(4f, 0f));
            Assert.IsTrue(sim.Player.HasDest);

            for (int i = 0; i < 180; i++)
                sim.Tick(Dt, PlayerCommand.None());

            Assert.IsFalse(sim.Player.HasDest);
            Assert.IsFalse(sim.Player.Moving);
            Assert.Less(Mathf.Abs(sim.Player.X - 4f), 0.25f);
            Assert.Less(Mathf.Abs(sim.Player.Z), 0.25f);
        }

        [Test]
        public void Approach_ThenMeleeWhenInRange()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int id = sim.Dummies.SpawnAt(8f, 0f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, id, 8f, 0f));
            Assert.AreEqual(CastPhase.Idle, sim.Caster.Phase);
            Assert.AreEqual(id, sim.Player.ChaseDummy);
            Assert.AreEqual(SkillId.Melee, sim.Player.PendingSkill);

            for (int i = 0; i < 200; i++)
                sim.Tick(Dt, PlayerCommand.None());

            Assert.AreEqual(1, sim.Dummies.Items[id].Hp);
            Assert.AreEqual(1, sim.Dummies.HitEvents);
        }

        [Test]
        public void Melee_MissesOutOfRangeIfForcedResolve()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int id = sim.Dummies.SpawnAt(10f, 0f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            TickNone(sim, SkillCatalog.Get(SkillId.Melee).Windup);
            Assert.AreEqual(CombatRules.DummyHp, sim.Dummies.Items[id].Hp);
            Assert.AreEqual(0, sim.Dummies.HitEvents);
        }

        [Test]
        public void OverlapDummy_MeleeStillHits()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int id = sim.Dummies.SpawnAt(0f, 0f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, id, 0f, 0f));
            TickNone(sim, SkillCatalog.Get(SkillId.Melee).Windup);
            Assert.Greater(sim.Dummies.HitEvents, 0);
            Assert.Less(sim.Dummies.Items[id].Hp, CombatRules.DummyHp);
        }

        [Test]
        public void PlayerWalksOntoDummyCell()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Dummies.SpawnAt(2f, 0f);
            sim.Tick(Dt, PlayerCommand.MoveTo(2f, 0f));
            for (int i = 0; i < 120; i++)
                sim.Tick(Dt, PlayerCommand.None());
            Assert.Less(Mathf.Abs(sim.Player.X - 2f), 0.25f);
            Assert.IsFalse(float.IsNaN(sim.Player.YawDeg));
        }

        [Test]
        public void Melee_HitsInFrontCone()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int front = sim.Dummies.SpawnAt(0f, 1.6f);
            int side = sim.Dummies.SpawnAt(1.6f, 0f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            TickNone(sim, SkillCatalog.Get(SkillId.Melee).Windup);
            Assert.AreEqual(1, sim.Dummies.Items[front].Hp);
            Assert.AreEqual(CombatRules.DummyHp, sim.Dummies.Items[side].Hp);
        }

        [Test]
        public void Projectile_MovesKinematicAndHits()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int id = sim.Dummies.SpawnAt(0f, 3f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, id, 0f, 3f));
            TickNone(sim, SkillCatalog.Get(SkillId.Projectile).Windup);
            Assert.Greater(sim.Projectiles.AliveCount, 0);

            for (int i = 0; i < 40; i++)
                sim.Tick(Dt, PlayerCommand.None());

            Assert.AreEqual(0, sim.Projectiles.AliveCount);
            Assert.AreEqual(1, sim.Dummies.Items[id].Hp);
            Assert.AreEqual(1, sim.ImpactEvents);
        }

        [Test]
        public void Area_SingleEvaluationHitsClusterOnly()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int a = sim.Dummies.SpawnAt(0f, 3f);
            int b = sim.Dummies.SpawnAt(0.6f, 3.1f);
            int c = sim.Dummies.SpawnAt(20f, 20f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 3f));
            TickNone(sim, SkillCatalog.Get(SkillId.Area).Windup);
            Assert.AreEqual(CastPhase.Active, sim.Caster.Phase);
            Assert.IsFalse(sim.Dummies.Items[a].Alive);
            Assert.IsFalse(sim.Dummies.Items[b].Alive);
            Assert.IsTrue(sim.Dummies.Items[c].Alive);
            Assert.AreEqual(2, sim.Dummies.HitEvents);

            int hits = sim.Dummies.HitEvents;
            TickNone(sim, 0.24f);
            Assert.AreEqual(hits, sim.Dummies.HitEvents);
        }

        [Test]
        public void Hit_FixedDamageThenDeathRecycles()
        {
            var sim = new ArenaSim();
            sim.Reset();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            int id = sim.Dummies.SpawnAt(0f, 1.5f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, id, 0f, 1.5f));
            TickNone(sim, melee.Windup);
            Assert.AreEqual(1, sim.Dummies.Items[id].Hp);
            Assert.IsTrue(sim.Dummies.Items[id].Occupied);

            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, id, 0f, 1.5f));
            TickNone(sim, melee.Windup + melee.Active + melee.Recovery + melee.Windup);
            Assert.IsFalse(sim.Dummies.Items[id].Alive);

            TickNone(sim, CombatRules.DeathRecycle + 0.04f);
            Assert.IsFalse(sim.Dummies.Items[id].Occupied);
            Assert.AreEqual(0, sim.Dummies.OccupiedCount);
        }

        [Test]
        public void Melee_CastAndHitFeedbackBothPresent()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Dummies.SpawnAt(0f, 1.6f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            TickNone(sim, SkillCatalog.Get(SkillId.Melee).Windup);
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.MeleeSwing), 1);
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.Hit), 1);
        }

        [Test]
        public void Area_TelegraphAndHitFeedbackBothPresentOnLethal()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Dummies.SpawnAt(0f, 3f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 3f));
            TickNone(sim, SkillCatalog.Get(SkillId.Area).Windup);
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.Area), 1, "E 施放范围圈");
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.Hit), 1, "E 命中闪白，死亡不能顶替");
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.Death), 1);
            Assert.Greater(sim.Dummies.Items[0].HitFlash, 0f);
        }

        [Test]
        public void Projectile_HitSpawnsFeedback()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int id = sim.Dummies.SpawnAt(0f, 3f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, id, 0f, 3f));
            TickNone(sim, SkillCatalog.Get(SkillId.Projectile).Windup);
            for (int i = 0; i < 40 && sim.Projectiles.AliveCount > 0; i++)
                sim.Tick(Dt, PlayerCommand.None());
            Assert.GreaterOrEqual(CountKind(sim, FeedbackKind.Hit), 1);
            Assert.AreEqual(1, sim.Dummies.HitEvents);
        }

        [Test]
        public void FeedbackPool_ReusesSlots()
        {
            var pool = new FeedbackPool();
            for (int i = 0; i < CombatRules.FeedbackPoolSize + 8; i++)
                pool.Spawn(FeedbackKind.Hit, i, 0f, 0.16f);
            Assert.LessOrEqual(pool.AliveCount, CombatRules.FeedbackPoolSize);

            for (int i = 0; i < 20; i++)
                pool.Tick(0.02f);
            Assert.AreEqual(0, pool.AliveCount);
        }

        [Test]
        public void ThreeSkills_ShareCastPipeline()
        {
            var sim = new ArenaSim();
            sim.Reset();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            SkillDef proj = SkillCatalog.Get(SkillId.Projectile);
            SkillDef area = SkillCatalog.Get(SkillId.Area);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            TickNone(sim, melee.Windup + melee.Active + melee.Recovery);

            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            TickNone(sim, proj.Windup + proj.Active + proj.Recovery);

            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 4f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            TickNone(sim, area.Windup);
            Assert.AreEqual(3, sim.CastEvents);
        }

        static int CountKind(ArenaSim sim, FeedbackKind kind)
        {
            int n = 0;
            for (int i = 0; i < sim.Feedback.Items.Length; i++)
            {
                if (sim.Feedback.Items[i].Alive && sim.Feedback.Items[i].Kind == kind)
                    n++;
            }

            return n;
        }

        static void TickNone(ArenaSim sim, float seconds)
        {
            int n = (int)System.Math.Round(seconds / Dt);
            for (int i = 0; i < n; i++)
                sim.Tick(Dt, PlayerCommand.None());
        }
    }
}
