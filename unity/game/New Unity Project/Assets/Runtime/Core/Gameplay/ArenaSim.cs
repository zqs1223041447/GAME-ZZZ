using UnityEngine;

namespace Game.Runtime.Core
{
    public sealed class ArenaSim
    {
        public PlayerMotorState Player;
        public readonly SkillCaster Caster = new SkillCaster();
        public readonly DummyCrowd Dummies = new DummyCrowd();
        public readonly ProjectilePool Projectiles = new ProjectilePool();
        public readonly FeedbackPool Feedback = new FeedbackPool();
        public SliceSession Session;

        public int CastEvents;
        public int ImpactEvents;
        public int SpawnedCount;
        int _hitLogLeft;

        public int AliveDummyCount
        {
            get { return Dummies.AliveCount; }
        }

        public void Reset()
        {
            Player = default;
            Player.YawDeg = 0f;
            Player.ChaseDummy = -1;
            Player.PendingSkill = SkillId.None;
            Player.Anim = AnimState.Idle;
            Caster.Reset();
            Dummies.Bind(this);
            Dummies.Clear();
            Projectiles.Clear();
            Feedback.Clear();
            CastEvents = 0;
            ImpactEvents = 0;
            SpawnedCount = 0;
            _hitLogLeft = 0;
        }

        public void SpawnDummies(int count, uint seed)
        {
            Projectiles.Clear();
            Feedback.Clear();
            var rng = new SeededRng(seed);
            Dummies.SpawnRing(count, rng, Player.X, Player.Z);
            SpawnedCount = count;
            _hitLogLeft = CombatRules.AudioHitDeathBudget;
        }

        public void Tick(float dt, PlayerCommand command)
        {
            if (dt < 0f)
                dt = 0f;

            if (Session != null)
            {
                Caster.Defs = Session.ResolveSkillDef;
                Session.TickRegen(dt);
            }

            Caster.Tick(dt, ref Player, Dummies);
            if (Caster.ResolveThisTick && PlayerCanMove())
                Resolve(Caster.ResolveSkill);

            command = FilterCommand(command);
            if (command.Kind != CommandKind.None)
                Caster.TryIssue(command, ref Player, Dummies);

            Caster.TryBeginPending(ref Player, Dummies);
            PlayerMotor.Tick(ref Player, Dummies, dt, Caster.CanMove && PlayerCanMove());
            Caster.TryBeginPending(ref Player, Dummies);

            Projectiles.Tick(dt, Dummies, Feedback, this);
            Dummies.TickAi(dt, Player.X, Player.Z);
            Dummies.Separate(CombatRules.DummyMinSeparation);
            ResolveEnemyAttacks();
            Dummies.TickDots(dt, Feedback);
            Dummies.TickDeath(dt);
            Feedback.Tick(dt);
            UpdatePlayerAnim();
            CheckPlayerDeath();
        }

        bool PlayerCanMove()
        {
            if (Session == null)
                return true;
            return Session.Alive;
        }

        PlayerCommand FilterCommand(PlayerCommand command)
        {
            if (command.Kind != CommandKind.Cast)
                return command;
            if (Session == null)
                return command;
            if (!Session.Alive)
                return PlayerCommand.None();
            return command;
        }

        void CheckPlayerDeath()
        {
            if (Session == null || Session.Alive)
                return;
            if (Session.State == MapState.InMap)
                Session.ExitMap(this, true);
        }

        public void RecoverPlayer()
        {
            Caster.Reset();
            if (Session != null)
                Caster.Defs = Session.ResolveSkillDef;
            PlayerMotor.Stop(ref Player);
            Player.Anim = AnimState.Idle;
            Player.Moving = false;
            if (Session != null)
                Session.Alive = true;
        }

        public bool TryConsumeHitLog()
        {
            if (_hitLogLeft <= 0)
                return false;
            _hitLogLeft--;
            return true;
        }

        public void PlayImpact()
        {
            ImpactEvents++;
            AudioEvents.Play(AudioEventId.Impact);
        }

        void Resolve(SkillId skill)
        {
            if (Session != null && Session.State == MapState.InMap && !Session.SpendMana(skill))
                return;

            SkillDef def = Caster.Def(skill);
            AudioEvents.Play(AudioEventId.Cast);
            CastEvents++;
            SpawnCastFeedback(skill, def);

            if (skill == SkillId.Melee)
                ResolveMelee(def);
            else if (skill == SkillId.Projectile)
                ResolveProjectile(def);
            else if (skill == SkillId.Area)
                ResolveArea(def);
        }

        void SpawnCastFeedback(SkillId skill, SkillDef def)
        {
            if (skill == SkillId.Melee)
            {
                float x = Player.X + Caster.LockedDirX * 0.95f;
                float z = Player.Z + Caster.LockedDirZ * 0.95f;
                Feedback.Spawn(FeedbackKind.MeleeSwing, x, z, 0.14f, 1.8f, Player.YawDeg);
                return;
            }

            if (skill == SkillId.Area)
            {
                Feedback.Spawn(
                    FeedbackKind.Area,
                    Caster.LockedAimX,
                    Caster.LockedAimZ,
                    0.22f,
                    def.AreaRadius * 2f,
                    0f);
                return;
            }

            Feedback.Spawn(FeedbackKind.Cast, Player.X, Player.Z, 0.14f, 0.8f, Player.YawDeg);
        }

        void ResolveMelee(SkillDef def)
        {
            PlayImpact();
            float rangeSq = def.Range * def.Range;
            float minDot = Mathf.Cos(CombatRules.MeleeConeHalfDeg * Mathf.Deg2Rad);
            for (int i = 0; i < Dummies.Items.Length; i++)
            {
                if (!Dummies.Items[i].Occupied || !Dummies.Items[i].Alive)
                    continue;
                float dx = Dummies.Items[i].X - Player.X;
                float dz = Dummies.Items[i].Z - Player.Z;
                float sq = dx * dx + dz * dz;
                if (sq > rangeSq)
                    continue;
                float len = Mathf.Sqrt(sq);
                float nx = len > 1e-5f ? dx / len : Caster.LockedDirX;
                float nz = len > 1e-5f ? dz / len : Caster.LockedDirZ;
                float dot = nx * Caster.LockedDirX + nz * Caster.LockedDirZ;
                if (dot < minDot)
                    continue;
                ApplySkillHit(SkillId.Melee, i, def.Damage);
            }
        }

        void ResolveProjectile(SkillDef def)
        {
            float x = Player.X + Caster.LockedDirX * 0.6f;
            float z = Player.Z + Caster.LockedDirZ * 0.6f;
            int forks = Session != null ? Session.ForkCount(SkillId.Projectile) : 0;
            bool packet = Session != null;
            HitRequest req = default;
            bool fire = false;
            if (packet)
            {
                Dummy probe = default;
                req = Session.BuildPlayerHit(SkillId.Projectile, probe);
                fire = req.FireFlat + req.ConvertPhysToFire > 0f;
            }

            Projectiles.Spawn(
                x, z, Caster.LockedDirX, Caster.LockedDirZ, def,
                req, packet, forks, false, SkillId.Projectile, fire);
        }

        void ResolveArea(SkillDef def)
        {
            PlayImpact();
            float r2 = def.AreaRadius * def.AreaRadius;
            float cx = Caster.LockedAimX;
            float cz = Caster.LockedAimZ;
            for (int i = 0; i < Dummies.Items.Length; i++)
            {
                if (!Dummies.Items[i].Occupied || !Dummies.Items[i].Alive)
                    continue;
                if (CombatMathUtil.DistSq(cx, cz, Dummies.Items[i].X, Dummies.Items[i].Z) > r2)
                    continue;
                ApplySkillHit(SkillId.Area, i, def.Damage);
            }
        }

        public void ResolveProjectileHit(int projIndex, int dummyIndex)
        {
            Projectile p = Projectiles.Items[projIndex];
            bool log = TryConsumeHitLog();
            int damage = p.Damage;
            HitResult result = default;
            if (p.UsePacket && Session != null)
            {
                Dummy dummy = Dummies.Items[dummyIndex];
                HitRequest req = p.Packet;
                req.Evasion = dummy.Evasion;
                req.Armour = dummy.Armour;
                req.FireRes = dummy.FireRes;
                req.HitRoll = Session.CombatRng.NextFloat01();
                req.CritRoll = Session.CombatRng.NextFloat01();
                req.IgniteRoll = Session.CombatRng.NextFloat01();
                result = CombatMath.ResolveHit(req);
                damage = result.Hit ? result.TotalTaken : 0;
                Session.LastDamageDealt = damage;
                Session.LastWasCrit = result.Crit;
                Session.LastWasIgnite = result.Ignite;
                if (result.Ignite)
                {
                    Dummies.ApplyIgnite(dummyIndex, result.IgniteDps, CombatMath.IgniteDuration);
                    Feedback.Spawn(FeedbackKind.Ignite, dummy.X, dummy.Z, 0.28f, 1.2f, 0f);
                    TriggerContext ctx = default;
                    ctx.Skill = p.Skill;
                    ctx.DummyIndex = dummyIndex;
                    ctx.X = p.X;
                    ctx.Z = p.Z;
                    ctx.DirX = p.DirX;
                    ctx.DirZ = p.DirZ;
                    ctx.Hit = result;
                    ctx.Request = req;
                    ctx.Speed = p.Speed;
                    ctx.Radius = p.Radius;
                    ctx.MaxDistance = p.MaxDistance;
                    ctx.Traveled = p.Traveled;
                    Session.Triggers.Fire(EventId.OnHit, ctx);
                }
                else if (result.Hit)
                {
                    TriggerContext ctx = default;
                    ctx.Skill = p.Skill;
                    ctx.DummyIndex = dummyIndex;
                    ctx.X = p.X;
                    ctx.Z = p.Z;
                    ctx.DirX = p.DirX;
                    ctx.DirZ = p.DirZ;
                    ctx.Hit = result;
                    ctx.Request = req;
                    ctx.Speed = p.Speed;
                    ctx.Radius = p.Radius;
                    ctx.MaxDistance = p.MaxDistance;
                    ctx.Traveled = p.Traveled;
                    Session.Triggers.Fire(EventId.OnHit, ctx);
                }

                if (p.Forks > 0 && result.Hit && !p.FromFork)
                    ForkFrom(p);
            }

            if (damage > 0)
            {
                bool died = Dummies.ApplyHit(dummyIndex, damage, Feedback, log);
                PlayImpact();
                if (died && Session != null)
                    Session.OnKill(this, Dummies.Items[dummyIndex], dummyIndex);
            }
        }

        void ForkFrom(Projectile p)
        {
            SkillDef def = Caster.Def(SkillId.Projectile);
            def.ProjectileSpeed = p.Speed;
            def.ProjectileRadius = p.Radius;
            float remain = p.MaxDistance - p.Traveled;
            if (remain < 2f)
                remain = 2f;
            def.ProjectileMaxDistance = remain;
            float ang = 0.55f;
            SpawnFork(p, def, ang);
            SpawnFork(p, def, -ang);
            if (Session != null)
            {
                Session.ForkSpawns += 2;
                Session.LastWasFork = true;
            }
        }

        void SpawnFork(Projectile p, SkillDef def, float ang)
        {
            float c = Mathf.Cos(ang);
            float s = Mathf.Sin(ang);
            float dx = p.DirX * c - p.DirZ * s;
            float dz = p.DirX * s + p.DirZ * c;
            Projectiles.Spawn(
                p.X, p.Z, dx, dz, def,
                p.Packet, p.UsePacket, 0, true, p.Skill, p.HasFire);
        }

        void ApplySkillHit(SkillId skill, int dummyIndex, int fallbackDamage)
        {
            int damage = fallbackDamage;
            if (Session != null)
            {
                Dummy dummy = Dummies.Items[dummyIndex];
                HitRequest req = Session.BuildPlayerHit(skill, dummy);
                HitResult result = CombatMath.ResolveHit(req);
                Session.LastDamageDealt = result.Hit ? result.TotalTaken : 0;
                Session.LastWasCrit = result.Crit;
                Session.LastWasIgnite = result.Ignite;
                if (!result.Hit)
                    return;
                damage = result.TotalTaken;
                if (result.Ignite)
                {
                    Dummies.ApplyIgnite(dummyIndex, result.IgniteDps, CombatMath.IgniteDuration);
                    Feedback.Spawn(FeedbackKind.Ignite, dummy.X, dummy.Z, 0.28f, 1.2f, 0f);
                }

                TriggerContext ctx = default;
                ctx.Skill = skill;
                ctx.DummyIndex = dummyIndex;
                ctx.X = dummy.X;
                ctx.Z = dummy.Z;
                ctx.Hit = result;
                ctx.Request = req;
                Session.Triggers.Fire(result.Crit ? EventId.OnCrit : EventId.OnHit, ctx);
                if (result.Crit)
                    Session.Triggers.Fire(EventId.OnHit, ctx);
            }

            bool died = Dummies.ApplyHit(dummyIndex, damage, Feedback, TryConsumeHitLog());
            if (died && Session != null)
                Session.OnKill(this, Dummies.Items[dummyIndex], dummyIndex);
        }

        void ResolveEnemyAttacks()
        {
            if (Session == null || !Session.Alive || Session.State != MapState.InMap)
                return;

            for (int i = 0; i < Dummies.Items.Length; i++)
            {
                if (!Dummies.Items[i].Occupied || !Dummies.Items[i].Alive || !Dummies.Items[i].AttackReady)
                    continue;
                Dummies.Items[i].AttackReady = false;
                HitRequest req = Session.BuildEnemyHit(Dummies.Items[i]);
                HitResult result = CombatMath.ResolveHit(req);
                if (!result.Hit)
                    continue;
                Session.ApplyPlayerHit(result);
                Feedback.Spawn(FeedbackKind.Hit, Player.X, Player.Z, 0.16f, 0.7f, 0f);
            }
        }

        void UpdatePlayerAnim()
        {
            if (Caster.Phase != CastPhase.Idle)
                Player.Anim = SkillCatalog.AnimFor(Caster.Current);
            else if (Player.Moving)
                Player.Anim = AnimState.Run;
            else
                Player.Anim = AnimState.Idle;
        }
    }
}
