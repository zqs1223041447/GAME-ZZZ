namespace Game.Runtime.Core
{
    public struct Projectile
    {
        public bool Alive;
        public float X;
        public float Z;
        public float DirX;
        public float DirZ;
        public float Speed;
        public float Radius;
        public float MaxDistance;
        public float Traveled;
        public int Damage;
        public bool UsePacket;
        public HitRequest Packet;
        public int Forks;
        public bool FromFork;
        public SkillId Skill;
        public bool HasFire;
        // S6P-WO-05：穿透剩余次数（>0 = 命中后继续飞行）
        public int PierceLeft;
        // S6P-WO-05：投射物返回（CanReturn=支持装配；Returning=已在返程）
        public bool CanReturn;
        public bool Returning;
        // S6P-WO-05：本投射物已命中目标位图基址（穿透/返程不得对同一目标重复结算）
        public int HitSlotBase;
        // S6P-WO-05：命中点爆炸半径（>0 = 命中时在命中点做范围结算，火球术用）
        public float ImpactAreaRadius;
    }

    public sealed class ProjectilePool
    {
        public readonly Projectile[] Items = new Projectile[CombatRules.ProjectilePoolSize];
        public int AliveCount;

        // 每投射物 × 每目标 1 bit 的已命中位图（S6P-WO-05 穿透/返程的唯一「不重复结算」真值）
        readonly bool[] _hits = new bool[CombatRules.ProjectilePoolSize * CombatRules.DummyPoolSize];

        /// <summary>返程投射物与玩家的判定半径（到达即消失）。</summary>
        const float ReturnArrive = 0.85f;

        public void Clear()
        {
            for (int i = 0; i < Items.Length; i++)
                Items[i] = default;
            for (int i = 0; i < _hits.Length; i++)
                _hits[i] = false;
            AliveCount = 0;
        }

        public bool WasHit(int projIndex, int dummyIndex)
        {
            int at = Items[projIndex].HitSlotBase + dummyIndex;
            if (at < 0 || at >= _hits.Length)
                return false;
            return _hits[at];
        }

        void MarkHit(int projIndex, int dummyIndex)
        {
            int at = Items[projIndex].HitSlotBase + dummyIndex;
            if (at >= 0 && at < _hits.Length)
                _hits[at] = true;
        }

        public int Spawn(float x, float z, float dirX, float dirZ, SkillDef def)
        {
            return Spawn(x, z, dirX, dirZ, def, default, false, 0, false, SkillId.Projectile, false, 0, false);
        }

        public int Spawn(
            float x,
            float z,
            float dirX,
            float dirZ,
            SkillDef def,
            HitRequest packet,
            bool usePacket,
            int forks,
            bool fromFork,
            SkillId skill,
            bool hasFire)
        {
            return Spawn(x, z, dirX, dirZ, def, packet, usePacket, forks, fromFork, skill, hasFire,
                def.Pierce, false);
        }

        public int Spawn(
            float x,
            float z,
            float dirX,
            float dirZ,
            SkillDef def,
            HitRequest packet,
            bool usePacket,
            int forks,
            bool fromFork,
            SkillId skill,
            bool hasFire,
            int pierce,
            bool canReturn)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].Alive)
                    continue;

                float lenSq = dirX * dirX + dirZ * dirZ;
                if (lenSq < 1e-8f)
                {
                    dirX = 0f;
                    dirZ = 1f;
                }

                Projectile p;
                p.Alive = true;
                p.X = x;
                p.Z = z;
                p.DirX = dirX;
                p.DirZ = dirZ;
                p.Speed = def.ProjectileSpeed;
                p.Radius = def.ProjectileRadius;
                p.MaxDistance = def.ProjectileMaxDistance;
                p.Traveled = 0f;
                p.Damage = def.Damage;
                p.UsePacket = usePacket;
                p.Packet = packet;
                p.Forks = forks;
                p.FromFork = fromFork;
                p.Skill = skill;
                p.HasFire = hasFire;
                p.PierceLeft = pierce > 0 ? pierce : 0;
                p.CanReturn = canReturn;
                p.Returning = false;
                p.HitSlotBase = i * CombatRules.DummyPoolSize;
                p.ImpactAreaRadius = def.ImpactAreaRadius;
                for (int h = 0; h < CombatRules.DummyPoolSize; h++)
                    _hits[p.HitSlotBase + h] = false;
                Items[i] = p;
                AliveCount++;
                return i;
            }

            return -1;
        }

        public void Tick(float dt, DummyCrowd dummies, FeedbackPool feedback, ArenaSim sim)
        {
            float dummyR = CombatRules.DummyRadius;
            for (int i = 0; i < Items.Length; i++)
            {
                ref Projectile p = ref Items[i];
                if (!p.Alive)
                    continue;

                float step = p.Speed * dt;
                p.X += p.DirX * step;
                p.Z += p.DirZ * step;
                p.Traveled += step;

                bool hit = false;
                int hitDummy = -1;
                if (dummies != null)
                {
                    float reach = p.Radius + dummyR;
                    float reachSq = reach * reach;
                    for (int d = 0; d < dummies.Items.Length; d++)
                    {
                        if (!dummies.Items[d].Occupied || !dummies.Items[d].Alive)
                            continue;
                        if (WasHit(i, d))
                            continue;
                        if (CombatMathUtil.DistSq(p.X, p.Z, dummies.Items[d].X, dummies.Items[d].Z) > reachSq)
                            continue;
                        hitDummy = d;
                        hit = true;
                        break;
                    }
                }

                if (hit)
                {
                    if (sim != null)
                        sim.ResolveProjectileHit(i, hitDummy);
                    else
                    {
                        bool log = false;
                        dummies.ApplyHit(hitDummy, p.Damage, feedback, log);
                    }
                    MarkHit(i, hitDummy);

                    if (p.PierceLeft > 0)
                        p.PierceLeft--; // 穿透：继续飞行（同一目标已入位图，不会重复结算）
                    else if (p.CanReturn && !p.Returning)
                    {
                        if (!TurnAround(ref p, sim))
                        {
                            p = default;
                            AliveCount--;
                        }
                    }
                    else
                    {
                        p = default;
                        AliveCount--;
                    }
                    continue;
                }

                if (p.Traveled >= p.MaxDistance)
                {
                    if (p.CanReturn && !p.Returning)
                    {
                        if (!TurnAround(ref p, sim))
                        {
                            p = default;
                            AliveCount--;
                        }
                        continue;
                    }

                    p = default;
                    AliveCount--;
                    continue;
                }

                if (p.Returning && sim != null)
                {
                    if (CombatMathUtil.Dist(p.X, p.Z, sim.Player.X, sim.Player.Z) <= ReturnArrive)
                    {
                        p = default;
                        AliveCount--;
                    }
                }
            }
        }

        /// <summary>掉头返回玩家：返程自带距离预算（= 掉头时到玩家的距离）。无玩家上下文时返回 false（=按原规则消失）。</summary>
        bool TurnAround(ref Projectile p, ArenaSim sim)
        {
            if (sim == null)
                return false;
            float dx;
            float dz;
            float yaw;
            CombatMathUtil.DirTo(p.X, p.Z, sim.Player.X, sim.Player.Z, out dx, out dz, out yaw);
            float dist = CombatMathUtil.Dist(p.X, p.Z, sim.Player.X, sim.Player.Z);
            if (dist <= ReturnArrive)
                return false;
            p.DirX = dx;
            p.DirZ = dz;
            p.Returning = true;
            p.Traveled = 0f;
            p.MaxDistance = dist;
            p.Speed = p.Speed > 0f ? p.Speed : 1f;
            return true;
        }
    }
}
