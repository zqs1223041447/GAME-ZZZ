using UnityEngine;

namespace Game.Runtime.Core
{
    public enum SkillId : byte
    {
        None = 0,
        Melee = 1,
        Projectile = 2,
        Area = 3
    }

    public enum CastPhase : byte
    {
        Idle = 0,
        Windup = 1,
        Active = 2,
        Recovery = 3
    }

    public enum AnimState : byte
    {
        Idle = 0,
        Run = 1,
        Attack = 2,
        Cast = 3,
        Hit = 4,
        Death = 5
    }

    public enum AudioEventId : byte
    {
        Cast = 0,
        Impact = 1,
        Hit = 2,
        Death = 3,
        Loot = 4
    }

    public enum CommandKind : byte
    {
        None = 0,
        Move = 1,
        Cast = 2,
        Stop = 3
    }

    public enum FeedbackKind : byte
    {
        Hit = 0,
        Death = 1,
        Cast = 2,
        MeleeSwing = 3,
        Area = 4,
        Loot = 5,
        Ignite = 6
    }

    public struct PlayerCommand
    {
        public CommandKind Kind;
        public SkillId Skill;
        public int TargetDummy;
        public float AimX;
        public float AimZ;

        public static PlayerCommand None()
        {
            PlayerCommand c;
            c.Kind = CommandKind.None;
            c.Skill = SkillId.None;
            c.TargetDummy = -1;
            c.AimX = 0f;
            c.AimZ = 0f;
            return c;
        }

        public static PlayerCommand MoveTo(float x, float z)
        {
            PlayerCommand c;
            c.Kind = CommandKind.Move;
            c.Skill = SkillId.None;
            c.TargetDummy = -1;
            c.AimX = x;
            c.AimZ = z;
            return c;
        }

        public static PlayerCommand CastAt(SkillId skill, int targetDummy, float aimX, float aimZ)
        {
            PlayerCommand c;
            c.Kind = CommandKind.Cast;
            c.Skill = skill;
            c.TargetDummy = targetDummy;
            c.AimX = aimX;
            c.AimZ = aimZ;
            return c;
        }

        public static PlayerCommand Stop()
        {
            PlayerCommand c;
            c.Kind = CommandKind.Stop;
            c.Skill = SkillId.None;
            c.TargetDummy = -1;
            c.AimX = 0f;
            c.AimZ = 0f;
            return c;
        }
    }

    public delegate SkillDef SkillDefResolver(SkillId id);

    public struct SkillDef
    {
        public SkillId Id;
        public float Range;
        public float Windup;
        public float Active;
        public float Recovery;
        public float Cooldown;
        public int Damage;
        public float ProjectileSpeed;
        public float ProjectileRadius;
        public float ProjectileMaxDistance;
        public float AreaRadius;
    }

    public struct PlayerMotorState
    {
        public float X;
        public float Z;
        public float YawDeg;
        public bool HasDest;
        public float DestX;
        public float DestZ;
        public float StopDistance;
        public int ChaseDummy;
        public SkillId PendingSkill;
        public bool Moving;
        public AnimState Anim;
    }

    public static class CombatRules
    {
        public const uint ArenaSeed = 0xC0FFEE;
        public const float BufferWindow = 0.20f;

        public const float PlayerSpeed = 6.5f;
        public const float PlayerTurnSpeedDeg = 720f;
        public const float ArriveEpsilon = 0.18f;
        public const float PlaneLimit = 38f;
        public const float DistEpsilon = 1e-5f;

        public const float HoldMoveEngage = 0.04f;
        public const float HoldSkillRepeat = 0.08f;

        public const int DummyHp = 2;
        public const float DummyRadius = 0.45f;
        public const float DummySpeed = 2.2f;
        public const float DummyTurnSpeedDeg = 360f;
        public const float DummyStopDistance = 1.1f;
        public const float DummyMinSeparation = 1f;
        public const float PickRadius = 0.85f;

        public const float LodNear = 16f;
        public const float LodFar = 32f;
        public const int LodMidPeriod = 3;
        public const int LodFarPeriod = 10;

        public const float DeathRecycle = 0.40f;
        public const float HitFlash = 0.20f;

        public const int DummyPoolSize = 300;
        public const int ProjectilePoolSize = 32;
        public const int FeedbackPoolSize = 48;

        public const float MeleeConeHalfDeg = 60f;
        public const int AudioHitDeathBudget = 16;

        public const float ApproachRangeFactor = 0.92f;
    }

    public static class SkillCatalog
    {
        static readonly SkillDef[] Defs = new SkillDef[4];

        static SkillCatalog()
        {
            Defs[(int)SkillId.Melee] = new SkillDef
            {
                Id = SkillId.Melee,
                Range = 2.4f,
                Windup = 0.06f,
                Active = 0.08f,
                Recovery = 0.10f,
                Cooldown = 0.04f,
                Damage = 1
            };
            Defs[(int)SkillId.Projectile] = new SkillDef
            {
                Id = SkillId.Projectile,
                Range = 12f,
                Windup = 0.08f,
                Active = 0.06f,
                Recovery = 0.12f,
                Cooldown = 0.10f,
                Damage = 1,
                ProjectileSpeed = 18f,
                ProjectileRadius = 0.35f,
                ProjectileMaxDistance = 16f
            };
            Defs[(int)SkillId.Area] = new SkillDef
            {
                Id = SkillId.Area,
                Range = 7f,
                Windup = 0.08f,
                Active = 0.10f,
                Recovery = 0.14f,
                Cooldown = 0.14f,
                Damage = 2,
                AreaRadius = 3.2f
            };
        }

        public static SkillDef Get(SkillId id)
        {
            int i = (int)id;
            if (i <= 0 || i >= Defs.Length)
                return default;
            return Defs[i];
        }

        public static float DurationOf(CastPhase phase, SkillDef def)
        {
            if (phase == CastPhase.Windup)
                return def.Windup;
            if (phase == CastPhase.Active)
                return def.Active;
            if (phase == CastPhase.Recovery)
                return def.Recovery;
            return 0f;
        }

        public static AnimState AnimFor(SkillId id)
        {
            return id == SkillId.Melee ? AnimState.Attack : AnimState.Cast;
        }
    }

    public static class AudioEvents
    {
        public static string Name(AudioEventId id)
        {
            switch (id)
            {
                case AudioEventId.Cast: return "Cast";
                case AudioEventId.Impact: return "Impact";
                case AudioEventId.Hit: return "Hit";
                case AudioEventId.Death: return "Death";
                case AudioEventId.Loot: return "Loot";
                default: return "Cast";
            }
        }

        public static void Play(AudioEventId id)
        {
            GameLog.Info("Audio", Name(id));
        }
    }

    public static class CombatMathUtil
    {
        public static float ClampPlane(float v)
        {
            if (v > CombatRules.PlaneLimit)
                return CombatRules.PlaneLimit;
            if (v < -CombatRules.PlaneLimit)
                return -CombatRules.PlaneLimit;
            return v;
        }

        public static float DistSq(float ax, float az, float bx, float bz)
        {
            float dx = ax - bx;
            float dz = az - bz;
            return dx * dx + dz * dz;
        }

        public static float Dist(float ax, float az, float bx, float bz)
        {
            return Mathf.Sqrt(DistSq(ax, az, bx, bz));
        }

        public static void DirTo(float fromX, float fromZ, float toX, float toZ, out float dirX, out float dirZ, out float yawDeg)
        {
            float dx = toX - fromX;
            float dz = toZ - fromZ;
            float len = Mathf.Sqrt(dx * dx + dz * dz);
            if (len < 1e-5f)
            {
                dirX = 0f;
                dirZ = 1f;
                yawDeg = 0f;
                return;
            }

            dirX = dx / len;
            dirZ = dz / len;
            yawDeg = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
        }

        public static float MoveTowardsAngleDeg(float current, float target, float maxDelta)
        {
            float delta = Mathf.DeltaAngle(current, target);
            if (Mathf.Abs(delta) <= maxDelta)
                return target;
            return current + Mathf.Sign(delta) * maxDelta;
        }
    }
}
