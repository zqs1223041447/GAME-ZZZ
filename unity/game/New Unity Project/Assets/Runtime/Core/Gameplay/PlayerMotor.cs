using UnityEngine;

namespace Game.Runtime.Core
{
    public static class PlayerMotor
    {
        public static void Tick(ref PlayerMotorState s, DummyCrowd dummies, float dt, bool canMove)
        {
            if (!canMove)
            {
                s.Moving = false;
                return;
            }

            if (s.ChaseDummy >= 0)
            {
                Dummy dummy;
                if (dummies == null || !dummies.TryGet(s.ChaseDummy, out dummy) || !dummy.Alive)
                {
                    s.ChaseDummy = -1;
                    s.PendingSkill = SkillId.None;
                    s.HasDest = false;
                    s.Moving = false;
                    return;
                }

                s.DestX = dummy.X;
                s.DestZ = dummy.Z;
                s.HasDest = true;
            }

            if (!s.HasDest)
            {
                s.Moving = false;
                return;
            }

            float dx = s.DestX - s.X;
            float dz = s.DestZ - s.Z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            float stop = s.StopDistance;
            if (stop < CombatRules.ArriveEpsilon)
                stop = CombatRules.ArriveEpsilon;

            if (dist < CombatRules.DistEpsilon)
            {
                s.Moving = false;
                if (s.ChaseDummy < 0)
                    s.HasDest = false;
                return;
            }

            float dirX = dx / dist;
            float dirZ = dz / dist;
            s.YawDeg = CombatMathUtil.MoveTowardsAngleDeg(
                s.YawDeg,
                Mathf.Atan2(dirX, dirZ) * Mathf.Rad2Deg,
                CombatRules.PlayerTurnSpeedDeg * dt);

            float step = CombatRules.PlayerSpeed * dt;
            bool arrive = dist <= stop || step >= dist - stop;
            if (arrive)
            {
                if (dist > stop)
                {
                    float remain = dist - stop;
                    s.X += dirX * remain;
                    s.Z += dirZ * remain;
                    s.X = CombatMathUtil.ClampPlane(s.X);
                    s.Z = CombatMathUtil.ClampPlane(s.Z);
                }

                s.Moving = false;
                if (s.ChaseDummy < 0)
                    s.HasDest = false;
                return;
            }

            s.X = CombatMathUtil.ClampPlane(s.X + dirX * step);
            s.Z = CombatMathUtil.ClampPlane(s.Z + dirZ * step);
            s.Moving = true;
        }

        public static void SetMove(ref PlayerMotorState s, float x, float z)
        {
            s.HasDest = true;
            s.DestX = CombatMathUtil.ClampPlane(x);
            s.DestZ = CombatMathUtil.ClampPlane(z);
            s.StopDistance = CombatRules.ArriveEpsilon;
            s.ChaseDummy = -1;
            s.PendingSkill = SkillId.None;
        }

        public static void SetApproach(ref PlayerMotorState s, int target, SkillId skill, float destX, float destZ, float range)
        {
            s.HasDest = true;
            s.DestX = destX;
            s.DestZ = destZ;
            s.StopDistance = range * CombatRules.ApproachRangeFactor;
            s.ChaseDummy = target;
            s.PendingSkill = skill;
        }

        public static void Stop(ref PlayerMotorState s)
        {
            s.HasDest = false;
            s.ChaseDummy = -1;
            s.PendingSkill = SkillId.None;
            s.Moving = false;
        }
    }
}
