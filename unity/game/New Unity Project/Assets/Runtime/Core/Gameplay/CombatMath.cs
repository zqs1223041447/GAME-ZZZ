using UnityEngine;

namespace Game.Runtime.Core
{
    public struct HitRequest
    {
        public float PhysFlat;
        public float FireFlat;
        public float ConvertPhysToFire;
        public float IncDamage;
        public float IncPhys;
        public float IncFire;
        public float MoreDamage;
        public float MorePhys;
        public float MoreFire;
        public float Accuracy;
        public float Evasion;
        public float Armour;
        public float FireRes;
        public float FireResMax;
        public float BaseCrit;
        public float AddedCrit;
        public float IncCrit;
        public float AddedCritMulti;
        public float IgniteChance;
        public bool IsAttack;
        public float HitRoll;
        public float CritRoll;
        public float IgniteRoll;
        public SkillId Skill;
        public Tag Tags;
    }

    public struct HitResult
    {
        public bool Hit;
        public bool Crit;
        public bool Ignite;
        public float HitChance;
        public float CritChance;
        public float PhysPreMit;
        public float FirePreMit;
        public float PhysTaken;
        public float FireTaken;
        public float IgniteDps;
        public int TotalTaken;
    }

    public static class CombatMath
    {
        public const float CritBaseMultiplier = 1.50f;
        public const float HitChanceMin = 0.05f;
        public const float HitChanceMax = 1.00f;
        public const float ArmourDivisor = 5f;
        public const float PdrCap = 0.90f;
        public const float ResistCap = 0.90f;
        public const float ResistDefaultMax = 0.75f;
        public const float IgniteDpsFactor = 0.50f;
        public const float IgniteDuration = 4f;
        public const float AccNumerator = 1.25f;
        public const float EvasionDivisor = 5f;
        public const float EvasionExponent = 0.9f;

        public static float CombineStat(float basePlusFlat, float increased, float moreProduct)
        {
            float incMul = 1f + increased;
            if (incMul < 0f)
                incMul = 0f;
            if (moreProduct < 0f)
                moreProduct = 0f;
            float v = basePlusFlat * incMul * moreProduct;
            if (v < 0f)
                v = 0f;
            return v;
        }

        public static float ChanceToHit(float accuracy, float evasion)
        {
            if (evasion <= 0f)
                return HitChanceMax;
            if (accuracy <= 0f)
                return HitChanceMin;

            float evadeTerm = Mathf.Pow(evasion / EvasionDivisor, EvasionExponent);
            float uncapped = AccNumerator * accuracy / (accuracy + evadeTerm);
            if (uncapped < HitChanceMin)
                return HitChanceMin;
            if (uncapped > HitChanceMax)
                return HitChanceMax;
            return uncapped;
        }

        public static float CritChance(float baseCrit, float additional, float increased)
        {
            float chance = (baseCrit + additional) * (1f + increased);
            if (chance < 0f)
                return 0f;
            return chance;
        }

        public static float CritMultiplier(float additional)
        {
            float m = CritBaseMultiplier + additional;
            if (m < 1f)
                return 1f;
            return m;
        }

        public static float PhysicalDamageReduction(float armour, float rawPhys)
        {
            if (armour <= 0f || rawPhys <= 0f)
                return 0f;
            float dr = armour / (armour + ArmourDivisor * rawPhys);
            if (dr > PdrCap)
                return PdrCap;
            if (dr < 0f)
                return 0f;
            return dr;
        }

        public static float AfterArmour(float rawPhys, float armour)
        {
            float taken = rawPhys * (1f - PhysicalDamageReduction(armour, rawPhys));
            if (taken < 0f)
                return 0f;
            return taken;
        }

        public static float ClampResistance(float resist, float maxResist)
        {
            float max = maxResist;
            if (max > ResistCap)
                max = ResistCap;
            if (max < 0f)
                max = 0f;
            if (resist > max)
                return max;
            return resist;
        }

        public static float AfterResistance(float damage, float resist, float maxResist)
        {
            float r = ClampResistance(resist, maxResist);
            float taken = damage * (1f - r);
            if (taken < 0f)
                return 0f;
            return taken;
        }

        public static float IgniteDpsFromFire(float firePreMit)
        {
            if (firePreMit <= 0f)
                return 0f;
            return firePreMit * IgniteDpsFactor;
        }

        public static void ConvertPhysToFire(
            float physFlat,
            float fireFlat,
            float convert,
            out float physLeft,
            out float converted,
            out float fireNative)
        {
            if (convert < 0f)
                convert = 0f;
            if (convert > 1f)
                convert = 1f;
            converted = physFlat * convert;
            physLeft = physFlat * (1f - convert);
            fireNative = fireFlat;
        }

        public static int RoundDamage(float value)
        {
            if (value <= 0f)
                return 0;
            return Mathf.RoundToInt(value);
        }

        public static HitResult ResolveHit(in HitRequest req)
        {
            HitResult r = default;

            float moreDamage = req.MoreDamage > 0f ? req.MoreDamage : 1f;
            float morePhys = req.MorePhys > 0f ? req.MorePhys : 1f;
            float moreFire = req.MoreFire > 0f ? req.MoreFire : 1f;

            float physLeft;
            float converted;
            float fireNative;
            ConvertPhysToFire(req.PhysFlat, req.FireFlat, req.ConvertPhysToFire, out physLeft, out converted, out fireNative);

            r.PhysPreMit = CombineStat(physLeft, req.IncDamage + req.IncPhys, moreDamage * morePhys);
            float fireFromNative = CombineStat(fireNative, req.IncDamage + req.IncFire, moreDamage * moreFire);
            float fireFromConv = CombineStat(
                converted,
                req.IncDamage + req.IncPhys + req.IncFire,
                moreDamage * morePhys * moreFire);
            r.FirePreMit = fireFromNative + fireFromConv;

            r.HitChance = req.IsAttack ? ChanceToHit(req.Accuracy, req.Evasion) : HitChanceMax;
            r.Hit = req.HitRoll < r.HitChance;
            if (!r.Hit)
                return r;

            r.CritChance = CritChance(req.BaseCrit, req.AddedCrit, req.IncCrit);
            r.Crit = req.CritRoll < r.CritChance;
            if (r.Crit)
            {
                float multi = CritMultiplier(req.AddedCritMulti);
                r.PhysPreMit *= multi;
                r.FirePreMit *= multi;
            }

            r.PhysTaken = AfterArmour(r.PhysPreMit, req.Armour);
            float resMax = req.FireResMax > 0f ? req.FireResMax : ResistDefaultMax;
            r.FireTaken = AfterResistance(r.FirePreMit, req.FireRes, resMax);

            float total = r.PhysTaken + r.FireTaken;
            r.TotalTaken = RoundDamage(total);
            if (r.TotalTaken < 1)
                r.TotalTaken = 1;

            r.IgniteDps = IgniteDpsFromFire(r.FirePreMit);
            r.Ignite = r.FirePreMit > 0f && req.IgniteRoll < req.IgniteChance && r.IgniteDps > 0f;
            return r;
        }
    }

    public static class RngUtil
    {
        public static int NextInt(SeededRng rng, int minInclusive, int maxExclusive)
        {
            if (rng == null || maxExclusive <= minInclusive)
                return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(rng.NextUInt() % span);
        }

        public static float Range(SeededRng rng, float minInclusive, float maxInclusive)
        {
            if (rng == null || maxInclusive <= minInclusive)
                return minInclusive;
            return minInclusive + rng.NextFloat01() * (maxInclusive - minInclusive);
        }

        public static bool Chance(SeededRng rng, float p)
        {
            if (rng == null)
                return false;
            if (p >= 1f)
                return true;
            if (p <= 0f)
                return false;
            return rng.NextFloat01() < p;
        }
    }
}
