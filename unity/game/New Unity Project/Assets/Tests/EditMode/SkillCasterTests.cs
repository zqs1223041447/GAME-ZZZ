using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class SkillCasterTests
    {
        const float Dt = 0.02f;

        [Test]
        public void Melee_WindupActiveRecoveryIdle()
        {
            var sim = NewSim();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);

            TickNone(sim, melee.Windup);
            Assert.AreEqual(CastPhase.Active, sim.Caster.Phase);
            Assert.AreEqual(1, sim.CastEvents);

            TickNone(sim, melee.Active);
            Assert.AreEqual(CastPhase.Recovery, sim.Caster.Phase);

            TickNone(sim, melee.Recovery);
            Assert.AreEqual(CastPhase.Idle, sim.Caster.Phase);
        }

        [Test]
        public void CancelWindow_MoveDuringRecovery_Interrupts()
        {
            var sim = NewSim();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            TickNone(sim, melee.Windup + melee.Active);
            Assert.AreEqual(CastPhase.Recovery, sim.Caster.Phase);

            sim.Tick(Dt, PlayerCommand.MoveTo(5f, 5f));
            Assert.AreEqual(CastPhase.Idle, sim.Caster.Phase);
            Assert.IsTrue(sim.Player.HasDest);
            Assert.AreEqual(5f, sim.Player.DestX, 0.01f);
        }

        [Test]
        public void CancelWindow_MoveDuringWindup_DoesNotInterruptYet()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            sim.Tick(Dt, PlayerCommand.MoveTo(8f, 0f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            Assert.IsTrue(sim.Caster.BufferHas);
            Assert.IsFalse(sim.Player.HasDest);
        }

        [Test]
        public void InputBuffer_FrozenDuringLock_ConsumedOnRecovery()
        {
            var sim = NewSim();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            Assert.AreEqual(SkillId.Melee, sim.Caster.Current);
            Assert.IsTrue(sim.Caster.BufferHas);

            TickNone(sim, melee.Windup + melee.Active);
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            Assert.AreEqual(SkillId.Projectile, sim.Caster.Current);
        }

        [Test]
        public void InputBuffer_OverwriteKeepsLatest()
        {
            var sim = NewSim();
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 4f));
            TickNone(sim, melee.Windup + melee.Active);
            Assert.AreEqual(SkillId.Area, sim.Caster.Current);
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
        }

        [Test]
        public void InputBuffer_ExpiresAfter200msWhenNotFrozen()
        {
            var caster = new SkillCaster();
            caster.Reset();
            var motor = default(PlayerMotorState);
            caster.InjectBufferForTests(PlayerCommand.MoveTo(3f, 0f));
            Assert.IsTrue(caster.BufferHas);

            caster.Tick(CombatRules.BufferWindow + 0.02f, ref motor, null);
            Assert.IsFalse(caster.BufferHas);
            Assert.IsFalse(motor.HasDest);
        }

        [Test]
        public void Cooldown_DoesNotBuffer()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            bool accepted = sim.Caster.TryIssue(
                PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f),
                ref sim.Player,
                sim.Dummies);
            Assert.IsFalse(accepted);
            Assert.IsFalse(sim.Caster.BufferHas);
        }

        [Test]
        public void HoldSkill_RepeatsOnRecoveryViaExistingBuffer()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            for (int i = 0; i < 30; i++)
                sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Assert.GreaterOrEqual(sim.CastEvents, 2, "hold resubmits Cast; recovery cancel/buffer must recast");
            Assert.Less(sim.CastEvents, 12, "must not be a second auto-cast system");
        }

        [Test]
        public void TapOnce_DoesNotRepeatWithoutNewCommand()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            TickNone(sim, 1f);
            Assert.AreEqual(1, sim.CastEvents);
        }

        [Test]
        public void HoldMove_UpdatesDestThenStopClears()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.MoveTo(4f, 0f));
            Assert.AreEqual(4f, sim.Player.DestX, 0.01f);
            sim.Tick(Dt, PlayerCommand.MoveTo(7f, 2f));
            Assert.AreEqual(7f, sim.Player.DestX, 0.01f);
            Assert.AreEqual(2f, sim.Player.DestZ, 0.01f);
            sim.Tick(Dt, PlayerCommand.Stop());
            Assert.IsFalse(sim.Player.HasDest);
            Assert.IsFalse(sim.Player.Moving);
        }

        [Test]
        public void MoveToSelf_DoesNotNaNYaw()
        {
            var sim = NewSim();
            sim.Tick(Dt, PlayerCommand.MoveTo(0f, 0f));
            Assert.IsFalse(float.IsNaN(sim.Player.YawDeg));
            Assert.IsFalse(float.IsInfinity(sim.Player.YawDeg));
            TickNone(sim, 0.2f);
            Assert.IsFalse(float.IsNaN(sim.Player.X));
            Assert.IsFalse(float.IsNaN(sim.Player.YawDeg));
        }

        static ArenaSim NewSim()
        {
            var sim = new ArenaSim();
            sim.Reset();
            return sim;
        }

        static void TickNone(ArenaSim sim, float seconds)
        {
            int n = (int)System.Math.Round(seconds / Dt);
            for (int i = 0; i < n; i++)
                sim.Tick(Dt, PlayerCommand.None());
        }
    }
}
