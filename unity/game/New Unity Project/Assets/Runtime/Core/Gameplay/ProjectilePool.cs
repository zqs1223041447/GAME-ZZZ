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
    }

    public sealed class ProjectilePool
    {
        public readonly Projectile[] Items = new Projectile[CombatRules.ProjectilePoolSize];
        public int AliveCount;

        public void Clear()
        {
            for (int i = 0; i < Items.Length; i++)
                Items[i] = default;
            AliveCount = 0;
        }

        public int Spawn(float x, float z, float dirX, float dirZ, SkillDef def)
        {
            return Spawn(x, z, dirX, dirZ, def, default, false, 0, false, SkillId.Projectile, false);
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
                }

                if (hit || p.Traveled >= p.MaxDistance)
                {
                    p = default;
                    AliveCount--;
                }
            }
        }
    }
}
