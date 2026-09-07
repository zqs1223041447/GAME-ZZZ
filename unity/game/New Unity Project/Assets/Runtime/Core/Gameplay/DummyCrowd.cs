using UnityEngine;

namespace Game.Runtime.Core
{
    public struct Dummy
    {
        public bool Occupied;
        public bool Alive;
        public float X;
        public float Z;
        public float YawDeg;
        public int Hp;
        public int MaxHp;
        public float HitFlash;
        public float DeathT;
        public AnimState Anim;
        public byte LodBand;
        public bool MovedThisTick;
        public EnemyKind Kind;
        public float Armour;
        public float Evasion;
        public float FireRes;
        public float Accuracy;
        public float AttackPhys;
        public float AttackFire;
        public float AttackCd;
        public float AttackTimer;
        public bool CanAttack;
        public bool AttackReady;
        public float Speed;
        public float Scale;
        public float IgniteDps;
        public float IgniteRemain;
        public float DotAcc;
    }

    public sealed class DummyCrowd
    {
        public readonly Dummy[] Items = new Dummy[CombatRules.DummyPoolSize];
        public int OccupiedCount;
        public int AliveCount;
        public int FrameIndex;
        public int HitEvents;
        public int DeathEvents;
        ArenaSim _killSim;

        public void Bind(ArenaSim sim)
        {
            _killSim = sim;
        }

        public void Clear()
        {
            for (int i = 0; i < Items.Length; i++)
                Items[i] = default;
            OccupiedCount = 0;
            AliveCount = 0;
            FrameIndex = 0;
        }

        public bool TryGet(int index, out Dummy dummy)
        {
            if (index < 0 || index >= Items.Length)
            {
                dummy = default;
                return false;
            }

            dummy = Items[index];
            return dummy.Occupied;
        }

        public int SpawnAt(float x, float z)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].Occupied)
                    continue;

                Dummy d;
                d.Occupied = true;
                d.Alive = true;
                d.X = CombatMathUtil.ClampPlane(x);
                d.Z = CombatMathUtil.ClampPlane(z);
                d.YawDeg = 0f;
                d.Hp = CombatRules.DummyHp;
                d.MaxHp = CombatRules.DummyHp;
                d.HitFlash = 0f;
                d.DeathT = 0f;
                d.Anim = AnimState.Idle;
                d.LodBand = 0;
                d.MovedThisTick = false;
                d.Kind = EnemyKind.Dummy;
                d.Armour = 0f;
                d.Evasion = 0f;
                d.FireRes = 0f;
                d.Accuracy = 0f;
                d.AttackPhys = 0f;
                d.AttackFire = 0f;
                d.AttackCd = 99f;
                d.AttackTimer = 0f;
                d.CanAttack = false;
                d.AttackReady = false;
                d.Speed = CombatRules.DummySpeed;
                d.Scale = 1f;
                d.IgniteDps = 0f;
                d.IgniteRemain = 0f;
                d.DotAcc = 0f;
                Items[i] = d;
                OccupiedCount++;
                AliveCount++;
                return i;
            }

            return -1;
        }

        public int SpawnEnemy(float x, float z, EnemyKind kind, int life, EnemyDef def)
        {
            int id = SpawnAt(x, z);
            if (id < 0)
                return -1;

            ref Dummy d = ref Items[id];
            d.Kind = kind;
            d.Hp = life;
            d.MaxHp = life;
            d.Armour = def.Armour;
            d.Evasion = def.Evasion;
            d.FireRes = def.FireRes;
            d.Accuracy = def.Accuracy;
            d.AttackPhys = def.AttackPhys;
            d.AttackFire = def.AttackFire;
            d.AttackCd = def.AttackCd;
            d.CanAttack = true;
            d.Speed = def.Speed > 0.1f ? def.Speed : CombatRules.DummySpeed;
            d.Scale = def.Scale > 0.1f ? def.Scale : 1f;
            return id;
        }

        public void SpawnRing(int count, SeededRng rng, float originX, float originZ)
        {
            Clear();
            if (count < 0)
                count = 0;
            if (count > CombatRules.DummyPoolSize)
                count = CombatRules.DummyPoolSize;

            int spawned = 0;
            float radius = 5f;
            while (spawned < count)
            {
                int ringCount = (int)(radius * 2.6f);
                if (ringCount < 8)
                    ringCount = 8;
                for (int i = 0; i < ringCount && spawned < count; i++)
                {
                    float jitter = rng.NextFloat01() * 0.45f;
                    float ang = (i / (float)ringCount) * 6.2831853f + jitter * 0.25f;
                    float r = radius + jitter;
                    float x = originX + Mathf.Cos(ang) * r;
                    float z = originZ + Mathf.Sin(ang) * r;
                    SpawnAt(x, z);
                    spawned++;
                }

                radius += 2.2f;
            }
        }

        public int FindNearestAlive(float x, float z, float maxDist)
        {
            float maxSq = maxDist * maxDist;
            int best = -1;
            float bestSq = maxSq;
            for (int i = 0; i < Items.Length; i++)
            {
                if (!Items[i].Occupied || !Items[i].Alive)
                    continue;
                float sq = CombatMathUtil.DistSq(x, z, Items[i].X, Items[i].Z);
                if (sq <= bestSq)
                {
                    bestSq = sq;
                    best = i;
                }
            }

            return best;
        }

        public bool ApplyHit(int index, int damage, FeedbackPool feedback, bool logAudio)
        {
            if (index < 0 || index >= Items.Length)
                return false;
            ref Dummy d = ref Items[index];
            if (!d.Occupied || !d.Alive)
                return false;

            d.Hp -= damage;
            d.HitFlash = CombatRules.HitFlash;
            d.Anim = AnimState.Hit;
            HitEvents++;
            if (feedback != null)
                feedback.Spawn(FeedbackKind.Hit, d.X, d.Z, 0.18f, 0.95f, 0f);
            // Hit 音频已重挂到玩家受击（ArenaDirector.HitFlash 上跳沿），怪物受击不再是 Hit 语义

            if (d.Hp <= 0)
            {
                d.Alive = false;
                d.DeathT = 0f;
                d.Anim = AnimState.Death;
                AliveCount--;
                DeathEvents++;
                if (feedback != null)
                    feedback.Spawn(FeedbackKind.Death, d.X, d.Z, CombatRules.DeathRecycle);
                // Death 音频已重挂到玩家进入 MapState.Dead（ArenaDirector）
                return true;
            }

            return false;
        }

        public void ApplyIgnite(int index, float dps, float duration)
        {
            if (index < 0 || index >= Items.Length)
                return;
            ref Dummy d = ref Items[index];
            if (!d.Occupied || !d.Alive)
                return;
            if (dps + 1e-4f >= d.IgniteDps)
            {
                d.IgniteDps = dps;
                d.IgniteRemain = duration;
            }
        }

        public void TickDots(float dt, FeedbackPool feedback)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                ref Dummy d = ref Items[i];
                if (!d.Occupied || !d.Alive)
                    continue;
                if (d.IgniteRemain <= 0f || d.IgniteDps <= 0f)
                    continue;

                d.IgniteRemain -= dt;
                float taken = CombatMath.AfterResistance(d.IgniteDps, d.FireRes, CombatMath.ResistCap);
                d.DotAcc += taken * dt;
                while (d.DotAcc >= 1f && d.Alive)
                {
                    d.DotAcc -= 1f;
                    Dummy copy = d;
                    bool died = ApplyHit(i, 1, feedback, false);
                    if (died && _killSim != null && _killSim.Session != null)
                        _killSim.Session.OnKill(_killSim, copy, i);
                }

                if (d.IgniteRemain <= 0f)
                {
                    d.IgniteDps = 0f;
                    d.IgniteRemain = 0f;
                }
            }
        }

        public void TickAi(float dt, float playerX, float playerZ)
        {
            FrameIndex++;
            float nearSq = CombatRules.LodNear * CombatRules.LodNear;
            float farSq = CombatRules.LodFar * CombatRules.LodFar;

            for (int i = 0; i < Items.Length; i++)
            {
                ref Dummy d = ref Items[i];
                d.MovedThisTick = false;
                if (!d.Occupied || !d.Alive)
                    continue;

                float sq = CombatMathUtil.DistSq(d.X, d.Z, playerX, playerZ);
                int period;
                if (sq > farSq)
                {
                    d.LodBand = 2;
                    period = CombatRules.LodFarPeriod;
                }
                else if (sq > nearSq)
                {
                    d.LodBand = 1;
                    period = CombatRules.LodMidPeriod;
                }
                else
                {
                    d.LodBand = 0;
                    period = 1;
                }

                if (((FrameIndex + i) % period) != 0)
                    continue;

                if (d.LodBand == 2)
                    continue;

                float stepDt = dt * period;
                float targetYaw = Mathf.Atan2(playerX - d.X, playerZ - d.Z) * Mathf.Rad2Deg;
                d.YawDeg = CombatMathUtil.MoveTowardsAngleDeg(
                    d.YawDeg,
                    targetYaw,
                    CombatRules.DummyTurnSpeedDeg * stepDt);

                float dist = Mathf.Sqrt(sq);
                if (dist <= CombatRules.DummyStopDistance)
                {
                    d.Anim = d.HitFlash > 0f ? AnimState.Hit : AnimState.Idle;
                    if (d.CanAttack)
                    {
                        d.AttackTimer += stepDt;
                        if (d.AttackTimer >= d.AttackCd)
                        {
                            d.AttackTimer = 0f;
                            d.AttackReady = true;
                        }
                    }

                    continue;
                }

                float rad = d.YawDeg * Mathf.Deg2Rad;
                float spd = d.Speed > 0.1f ? d.Speed : CombatRules.DummySpeed;
                float move = spd * stepDt;
                float nx = d.X + Mathf.Sin(rad) * move;
                float nz = d.Z + Mathf.Cos(rad) * move;
                d.X = CombatMathUtil.ClampPlane(nx);
                d.Z = CombatMathUtil.ClampPlane(nz);
                d.MovedThisTick = true;
                d.Anim = d.HitFlash > 0f ? AnimState.Hit : AnimState.Run;
            }
        }

        public void Separate(float minDist)
        {
            if (minDist <= 0f)
                return;

            float minSq = minDist * minDist;
            Dummy[] items = Items;
            int n = items.Length;
            for (int i = 0; i < n; i++)
            {
                if (!items[i].Occupied || !items[i].Alive)
                    continue;
                for (int j = i + 1; j < n; j++)
                {
                    if (!items[j].Occupied || !items[j].Alive)
                        continue;

                    float dx = items[j].X - items[i].X;
                    float dz = items[j].Z - items[i].Z;
                    float sq = dx * dx + dz * dz;
                    if (sq >= minSq)
                        continue;

                    float dist = Mathf.Sqrt(sq);
                    float nx;
                    float nz;
                    if (dist < CombatRules.DistEpsilon)
                    {
                        nx = 1f;
                        nz = 0f;
                        dist = 0f;
                    }
                    else
                    {
                        nx = dx / dist;
                        nz = dz / dist;
                    }

                    float push = (minDist - dist) * 0.5f;
                    items[i].X = CombatMathUtil.ClampPlane(items[i].X - nx * push);
                    items[i].Z = CombatMathUtil.ClampPlane(items[i].Z - nz * push);
                    items[j].X = CombatMathUtil.ClampPlane(items[j].X + nx * push);
                    items[j].Z = CombatMathUtil.ClampPlane(items[j].Z + nz * push);
                }
            }
        }

        public void TickDeath(float dt)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                ref Dummy d = ref Items[i];
                if (!d.Occupied)
                    continue;

                if (d.HitFlash > 0f)
                {
                    d.HitFlash -= dt;
                    if (d.HitFlash < 0f)
                        d.HitFlash = 0f;
                }

                if (d.Alive)
                {
                    if (d.HitFlash <= 0f && !d.MovedThisTick && d.Anim == AnimState.Hit)
                        d.Anim = AnimState.Idle;
                    continue;
                }

                d.Anim = AnimState.Death;
                d.DeathT += dt;
                if (d.DeathT >= CombatRules.DeathRecycle)
                {
                    d = default;
                    OccupiedCount--;
                }
            }
        }
    }
}
