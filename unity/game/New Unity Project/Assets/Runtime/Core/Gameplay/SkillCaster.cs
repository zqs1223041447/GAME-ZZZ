namespace Game.Runtime.Core
{
    public sealed class SkillCaster
    {
        public CastPhase Phase;
        public SkillId Current;
        public float PhaseElapsed;
        public readonly float[] CooldownRemain = new float[4];
        public int LockedTarget;
        public float LockedAimX;
        public float LockedAimZ;
        public float LockedDirX;
        public float LockedDirZ;
        public bool Resolved;

        public bool BufferHas;
        public PlayerCommand BufferCmd;
        public float BufferAge;
        public bool BufferFrozen;

        public bool ResolveThisTick;
        public SkillId ResolveSkill;
        public SkillDefResolver Defs;

        public void Reset()
        {
            Phase = CastPhase.Idle;
            Current = SkillId.None;
            PhaseElapsed = 0f;
            for (int i = 0; i < CooldownRemain.Length; i++)
                CooldownRemain[i] = 0f;
            LockedTarget = -1;
            LockedAimX = 0f;
            LockedAimZ = 0f;
            LockedDirX = 0f;
            LockedDirZ = 1f;
            Resolved = false;
            BufferHas = false;
            BufferAge = 0f;
            BufferFrozen = false;
            ResolveThisTick = false;
            ResolveSkill = SkillId.None;
        }

        public SkillDef Def(SkillId id)
        {
            if (Defs != null)
                return Defs(id);
            return SkillCatalog.Get(id);
        }

        public bool InCancelWindow
        {
            get { return Phase == CastPhase.Recovery || Phase == CastPhase.Idle; }
        }

        public bool CanMove
        {
            get { return Phase == CastPhase.Idle; }
        }

        public void Tick(float dt, ref PlayerMotorState motor, DummyCrowd dummies)
        {
            ResolveThisTick = false;
            ResolveSkill = SkillId.None;

            for (int i = 1; i < CooldownRemain.Length; i++)
            {
                if (CooldownRemain[i] > 0f)
                    CooldownRemain[i] -= dt;
                if (CooldownRemain[i] < 0f)
                    CooldownRemain[i] = 0f;
            }

            if (BufferHas && !BufferFrozen)
            {
                BufferAge += dt;
                if (BufferAge > CombatRules.BufferWindow)
                    BufferHas = false;
            }

            if (Phase == CastPhase.Idle)
            {
                TryConsumeBuffer(ref motor, dummies);
                return;
            }

            SkillDef def = Def(Current);
            float duration = SkillCatalog.DurationOf(Phase, def);
            PhaseElapsed += dt;
            if (PhaseElapsed + 1e-6f >= duration)
                AdvancePhase(ref motor, dummies);
        }

        public bool TryIssue(PlayerCommand cmd, ref PlayerMotorState motor, DummyCrowd dummies)
        {
            if (cmd.Kind == CommandKind.None)
                return false;

            if (cmd.Kind == CommandKind.Cast)
            {
                if (cmd.Skill == SkillId.None)
                    return false;
                if (CooldownRemain[(int)cmd.Skill] > 0f)
                    return false;
            }

            if (Phase == CastPhase.Windup || Phase == CastPhase.Active)
            {
                BufferHas = true;
                BufferCmd = cmd;
                BufferAge = 0f;
                BufferFrozen = true;
                return true;
            }

            if (Phase == CastPhase.Recovery)
                CancelToIdle();

            return Execute(cmd, ref motor, dummies);
        }

        public void InjectBufferForTests(PlayerCommand cmd)
        {
            BufferHas = true;
            BufferCmd = cmd;
            BufferAge = 0f;
            BufferFrozen = false;
        }

        public bool TryBeginPending(ref PlayerMotorState motor, DummyCrowd dummies)
        {
            if (Phase != CastPhase.Idle)
                return false;
            if (motor.PendingSkill == SkillId.None)
                return false;

            SkillId skill = motor.PendingSkill;
            int target = motor.ChaseDummy;
            float aimX = motor.DestX;
            float aimZ = motor.DestZ;
            if (!InRangeWith(Def(skill), skill, motor.X, motor.Z, target, aimX, aimZ, dummies))
                return false;

            return BeginCast(skill, target, aimX, aimZ, ref motor, dummies);
        }

        bool TryConsumeBuffer(ref PlayerMotorState motor, DummyCrowd dummies)
        {
            if (!BufferHas)
                return false;

            PlayerCommand cmd = BufferCmd;
            BufferHas = false;
            BufferFrozen = false;
            BufferAge = 0f;

            if (cmd.Kind == CommandKind.Cast && cmd.Skill != SkillId.None && CooldownRemain[(int)cmd.Skill] > 0f)
                return false;

            if (Phase == CastPhase.Recovery)
                CancelToIdle();

            return Execute(cmd, ref motor, dummies);
        }

        void AdvancePhase(ref PlayerMotorState motor, DummyCrowd dummies)
        {
            if (Phase == CastPhase.Windup)
            {
                EnterActive();
                return;
            }

            if (Phase == CastPhase.Active)
            {
                EnterRecovery(ref motor, dummies);
                return;
            }

            Phase = CastPhase.Idle;
            Current = SkillId.None;
            PhaseElapsed = 0f;
            TryConsumeBuffer(ref motor, dummies);
        }

        void EnterActive()
        {
            Phase = CastPhase.Active;
            PhaseElapsed = 0f;
            Resolved = true;
            ResolveThisTick = true;
            ResolveSkill = Current;
        }

        void EnterRecovery(ref PlayerMotorState motor, DummyCrowd dummies)
        {
            Phase = CastPhase.Recovery;
            PhaseElapsed = 0f;
            BufferFrozen = false;
            TryConsumeBuffer(ref motor, dummies);
        }

        void CancelToIdle()
        {
            Phase = CastPhase.Idle;
            Current = SkillId.None;
            PhaseElapsed = 0f;
            Resolved = false;
            ResolveThisTick = false;
        }

        bool Execute(PlayerCommand cmd, ref PlayerMotorState motor, DummyCrowd dummies)
        {
            if (cmd.Kind == CommandKind.Stop)
            {
                PlayerMotor.Stop(ref motor);
                return true;
            }

            if (cmd.Kind == CommandKind.Move)
            {
                PlayerMotor.SetMove(ref motor, cmd.AimX, cmd.AimZ);
                return true;
            }

            SkillId skill = cmd.Skill;
            int target = cmd.TargetDummy;
            float aimX = cmd.AimX;
            float aimZ = cmd.AimZ;
            RefreshAimFromTarget(ref target, ref aimX, ref aimZ, dummies);

            SkillDef def = Def(skill);
            if (!InRangeWith(def, skill, motor.X, motor.Z, target, aimX, aimZ, dummies))
            {
                PlayerMotor.SetApproach(ref motor, target, skill, aimX, aimZ, def.Range);
                return true;
            }

            return BeginCast(skill, target, aimX, aimZ, ref motor, dummies);
        }

        bool BeginCast(SkillId skill, int target, float aimX, float aimZ, ref PlayerMotorState motor, DummyCrowd dummies)
        {
            RefreshAimFromTarget(ref target, ref aimX, ref aimZ, dummies);
            PlayerMotor.Stop(ref motor);

            float dirX;
            float dirZ;
            float yaw;
            CombatMathUtil.DirTo(motor.X, motor.Z, aimX, aimZ, out dirX, out dirZ, out yaw);
            motor.YawDeg = yaw;

            Current = skill;
            Phase = CastPhase.Windup;
            PhaseElapsed = 0f;
            LockedTarget = target;
            LockedAimX = aimX;
            LockedAimZ = aimZ;
            LockedDirX = dirX;
            LockedDirZ = dirZ;
            Resolved = false;
            CooldownRemain[(int)skill] = Def(skill).Cooldown;
            return true;
        }

        public static bool InRange(SkillId skill, float px, float pz, int target, float aimX, float aimZ, DummyCrowd dummies)
        {
            return InRangeWith(SkillCatalog.Get(skill), skill, px, pz, target, aimX, aimZ, dummies);
        }

        public static bool InRangeWith(SkillDef def, SkillId skill, float px, float pz, int target, float aimX, float aimZ, DummyCrowd dummies)
        {
            float tx = aimX;
            float tz = aimZ;
            if (target >= 0 && dummies != null)
            {
                Dummy dummy;
                if (!dummies.TryGet(target, out dummy) || !dummy.Alive)
                {
                    if (skill == SkillId.Melee)
                        return true;
                    return CombatMathUtil.Dist(px, pz, aimX, aimZ) <= def.Range;
                }

                tx = dummy.X;
                tz = dummy.Z;
            }
            else if (skill == SkillId.Melee)
            {
                return true;
            }

            return CombatMathUtil.Dist(px, pz, tx, tz) <= def.Range;
        }

        static void RefreshAimFromTarget(ref int target, ref float aimX, ref float aimZ, DummyCrowd dummies)
        {
            if (target < 0 || dummies == null)
                return;
            Dummy dummy;
            if (!dummies.TryGet(target, out dummy) || !dummy.Alive)
            {
                target = -1;
                return;
            }

            aimX = dummy.X;
            aimZ = dummy.Z;
        }
    }
}
