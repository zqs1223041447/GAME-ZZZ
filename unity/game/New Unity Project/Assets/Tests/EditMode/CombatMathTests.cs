using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class CombatMathTests
    {
        [Test]
        public void GV_STAT_001_IncreasedAndMore()
        {
            float v = CombatMath.CombineStat(120f, 0.50f, 1.20f);
            Assert.AreEqual(216f, v, 0.001f);
        }

        [Test]
        public void GV_STAT_002_IncreasedFloor()
        {
            float v = CombatMath.CombineStat(100f, -2f, 1f);
            Assert.AreEqual(0f, v, 0.001f);
        }

        [Test]
        public void GV_HIT_001_NoEvasionAlwaysHits()
        {
            Assert.AreEqual(1f, CombatMath.ChanceToHit(10f, 0f), 0.0001f);
        }

        [Test]
        public void GV_HIT_002_NoAccuracyMinFive()
        {
            Assert.AreEqual(0.05f, CombatMath.ChanceToHit(0f, 1000f), 0.0001f);
        }

        [Test]
        public void GV_HIT_003_Acc1000_Eva5000()
        {
            Assert.AreEqual(0.832674f, CombatMath.ChanceToHit(1000f, 5000f), 0.00002f);
        }

        [Test]
        public void GV_CRIT_001_Chance()
        {
            Assert.AreEqual(0.10f, CombatMath.CritChance(0.05f, 0f, 1.0f), 0.0001f);
        }

        [Test]
        public void GV_CRIT_002_Damage()
        {
            Assert.AreEqual(2.0f, CombatMath.CritMultiplier(0.50f), 0.0001f);
            Assert.AreEqual(150f, 100f * CombatMath.CritMultiplier(0f), 0.001f);
        }

        [Test]
        public void GV_ARMOUR_001_Half()
        {
            Assert.AreEqual(0.50f, CombatMath.PhysicalDamageReduction(5000f, 1000f), 0.0001f);
            Assert.AreEqual(500f, CombatMath.AfterArmour(1000f, 5000f), 0.001f);
        }

        [Test]
        public void GV_ARMOUR_002_Cap90()
        {
            Assert.AreEqual(0.90f, CombatMath.PhysicalDamageReduction(45000f, 1000f), 0.0001f);
            Assert.AreEqual(100f, CombatMath.AfterArmour(1000f, 45000f), 0.001f);
        }

        [Test]
        public void GV_ARMOUR_003_ZeroArmour()
        {
            Assert.AreEqual(100f, CombatMath.AfterArmour(100f, 0f), 0.001f);
        }

        [Test]
        public void GV_RES_001_SeventyFive()
        {
            Assert.AreEqual(25f, CombatMath.AfterResistance(100f, 0.75f, 0.75f), 0.001f);
        }

        [Test]
        public void GV_RES_002_Negative()
        {
            Assert.AreEqual(120f, CombatMath.AfterResistance(100f, -0.20f, 0.75f), 0.001f);
        }

        [Test]
        public void GV_RES_003_Cap90()
        {
            Assert.AreEqual(10f, CombatMath.AfterResistance(100f, 1.20f, 0.90f), 0.001f);
        }

        [Test]
        public void GV_CONV_001_FiftyNoInc()
        {
            HitRequest req = BaseReq();
            req.PhysFlat = 100f;
            req.ConvertPhysToFire = 0.50f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.AreEqual(50f, r.PhysPreMit, 0.001f);
            Assert.AreEqual(50f, r.FirePreMit, 0.001f);
            Assert.AreEqual(100, r.TotalTaken);
        }

        [Test]
        public void GV_CONV_002_ConvertedGetsBothIncreased()
        {
            HitRequest req = BaseReq();
            req.PhysFlat = 100f;
            req.ConvertPhysToFire = 0.50f;
            req.IncPhys = 1.0f;
            req.IncFire = 1.0f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.AreEqual(100f, r.PhysPreMit, 0.001f);
            Assert.AreEqual(150f, r.FirePreMit, 0.001f);
            Assert.AreEqual(250, r.TotalTaken);
        }

        [Test]
        public void GV_CONV_003_FullConvert()
        {
            HitRequest req = BaseReq();
            req.PhysFlat = 80f;
            req.ConvertPhysToFire = 1f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.AreEqual(0f, r.PhysPreMit, 0.001f);
            Assert.AreEqual(80f, r.FirePreMit, 0.001f);
        }

        [Test]
        public void GV_IGNITE_001_HalfFireFourSeconds()
        {
            Assert.AreEqual(50f, CombatMath.IgniteDpsFromFire(100f), 0.001f);
            Assert.AreEqual(4f, CombatMath.IgniteDuration, 0.001f);
        }

        [Test]
        public void GV_FULL_001_PhysHitVsArmour()
        {
            HitRequest req = BaseReq();
            req.PhysFlat = 1000f;
            req.Armour = 5000f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.IsTrue(r.Hit);
            Assert.AreEqual(500, r.TotalTaken);
        }

        [Test]
        public void GV_FULL_002_Miss()
        {
            HitRequest req = BaseReq();
            req.IsAttack = true;
            req.Accuracy = 0f;
            req.Evasion = 1000f;
            req.HitRoll = 0.50f;
            req.PhysFlat = 50f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.IsFalse(r.Hit);
            Assert.AreEqual(0, r.TotalTaken);
        }

        [Test]
        public void GV_FULL_003_CritFireIgnite()
        {
            HitRequest req = BaseReq();
            req.FireFlat = 100f;
            req.BaseCrit = 0.05f;
            req.IncCrit = 1f;
            req.CritRoll = 0.01f;
            req.IgniteChance = 1f;
            req.IgniteRoll = 0f;
            req.FireRes = 0.50f;
            req.FireResMax = 0.75f;
            HitResult r = CombatMath.ResolveHit(req);
            Assert.IsTrue(r.Crit);
            Assert.AreEqual(150f, r.FirePreMit, 0.001f);
            Assert.AreEqual(75f, r.FireTaken, 0.001f);
            Assert.AreEqual(75, r.TotalTaken);
            Assert.IsTrue(r.Ignite);
            Assert.AreEqual(75f, r.IgniteDps, 0.001f);
        }

        [Test]
        public void StatBag_TagFilter()
        {
            var bag = new StatBag();
            bag.Add(Modifier.Tagged(StatId.MorePhysical, ModOp.More, 0.25f, Tag.Melee), Tag.Attack | Tag.Projectile, ConditionId.Always);
            Assert.AreEqual(1f, bag.RawMore(StatId.MorePhysical), 0.001f);
            bag.Add(Modifier.Tagged(StatId.MorePhysical, ModOp.More, 0.25f, Tag.Melee), Tag.Attack | Tag.Melee, ConditionId.Always);
            Assert.AreEqual(1.25f, bag.RawMore(StatId.MorePhysical), 0.001f);
        }

        [Test]
        public void Trigger_DepthCapAndCooldown()
        {
            var ts = new TriggerSystem();
            ts.Add(new Trigger { Event = EventId.OnHit, Effect = EffectId.ForkProjectiles, Cooldown = 1f, MaxDepth = 1, Skill = SkillId.None });
            int runs = 0;
            ts.Handler = (e, ctx) =>
            {
                runs++;
                ts.Fire(EventId.OnHit, ctx);
            };
            ts.Fire(EventId.OnHit, default);
            Assert.AreEqual(1, runs);
            Assert.Greater(ts.DepthBlocked, 0);
            ts.Fire(EventId.OnHit, default);
            Assert.AreEqual(1, runs);
            Assert.Greater(ts.CooldownBlocked, 0);
        }

        static HitRequest BaseReq()
        {
            HitRequest req = default;
            req.MoreDamage = 1f;
            req.MorePhys = 1f;
            req.MoreFire = 1f;
            req.IsAttack = false;
            req.HitRoll = 0f;
            req.CritRoll = 1f;
            req.IgniteRoll = 1f;
            req.FireResMax = 0.75f;
            return req;
        }
    }
}
