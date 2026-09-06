using System;

namespace Game.Runtime.Core
{
    [Flags]
    public enum Tag : uint
    {
        None = 0,
        Attack = 1 << 0,
        Spell = 1 << 1,
        Melee = 1 << 2,
        Projectile = 1 << 3,
        Area = 1 << 4,
        Hit = 1 << 5,
        Physical = 1 << 6,
        Fire = 1 << 7,
        Duration = 1 << 8
    }

    public enum StatId : byte
    {
        Life = 0,
        Mana = 1,
        Strength = 2,
        Dexterity = 3,
        Intelligence = 4,
        Armour = 5,
        Evasion = 6,
        Accuracy = 7,
        FireResistance = 8,
        MaxFireResistance = 9,
        Damage = 10,
        PhysicalDamage = 11,
        FireDamage = 12,
        MoreDamage = 13,
        MorePhysical = 14,
        MoreFire = 15,
        AddedPhysical = 16,
        AddedFire = 17,
        ConvertPhysToFire = 18,
        CritChanceBase = 19,
        CritChanceAdded = 20,
        CritChanceIncreased = 21,
        CritMultiAdded = 22,
        IgniteChance = 23,
        AreaRadiusMore = 24,
        AreaDamageMore = 25,
        AttackSpeed = 26,
        Fork = 27,
        Count = 28
    }

    public enum ModOp : byte
    {
        Base = 0,
        Flat = 1,
        Increased = 2,
        More = 3,
        Override = 4
    }

    public enum ConditionId : byte
    {
        Always = 0,
        Hit = 1,
        Crit = 2,
        IsAttack = 3,
        IsSpell = 4
    }

    public enum EffectId : byte
    {
        None = 0,
        ForkProjectiles = 1,
        ApplyIgnite = 2
    }

    public enum EventId : byte
    {
        OnHit = 0,
        OnKill = 1,
        OnCrit = 2,
        OnCast = 3,
        Count = 4
    }

    public struct Modifier
    {
        public StatId Stat;
        public ModOp Op;
        public float Value;
        public Tag RequiredTags;
        public ConditionId Condition;

        public static Modifier Make(StatId stat, ModOp op, float value)
        {
            Modifier m;
            m.Stat = stat;
            m.Op = op;
            m.Value = value;
            m.RequiredTags = Tag.None;
            m.Condition = ConditionId.Always;
            return m;
        }

        public static Modifier Tagged(StatId stat, ModOp op, float value, Tag tags)
        {
            Modifier m = Make(stat, op, value);
            m.RequiredTags = tags;
            return m;
        }
    }

    public sealed class StatBag
    {
        public const int StatCount = (int)StatId.Count;

        readonly float[] _base = new float[StatCount];
        readonly float[] _flat = new float[StatCount];
        readonly float[] _inc = new float[StatCount];
        readonly float[] _more = new float[StatCount];
        readonly bool[] _hasOverride = new bool[StatCount];
        readonly float[] _override = new float[StatCount];

        public StatBag()
        {
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < StatCount; i++)
            {
                _base[i] = 0f;
                _flat[i] = 0f;
                _inc[i] = 0f;
                _more[i] = 1f;
                _hasOverride[i] = false;
                _override[i] = 0f;
            }
        }

        public void Add(Modifier mod, Tag skillTags, ConditionId now)
        {
            if (mod.RequiredTags != Tag.None && (skillTags & mod.RequiredTags) != mod.RequiredTags)
                return;
            if (mod.Condition != ConditionId.Always && mod.Condition != now)
                return;

            int i = (int)mod.Stat;
            if (i < 0 || i >= StatCount)
                return;

            switch (mod.Op)
            {
                case ModOp.Base:
                    _base[i] += mod.Value;
                    break;
                case ModOp.Flat:
                    _flat[i] += mod.Value;
                    break;
                case ModOp.Increased:
                    _inc[i] += mod.Value;
                    break;
                case ModOp.More:
                    _more[i] *= 1f + mod.Value;
                    break;
                case ModOp.Override:
                    _hasOverride[i] = true;
                    _override[i] = mod.Value;
                    break;
            }
        }

        public void AddAll(Modifier[] mods, Tag skillTags, ConditionId now)
        {
            if (mods == null)
                return;
            for (int i = 0; i < mods.Length; i++)
                Add(mods[i], skillTags, now);
        }

        public float Get(StatId id)
        {
            int i = (int)id;
            if (i < 0 || i >= StatCount)
                return 0f;
            if (_hasOverride[i])
                return _override[i];
            return CombatMath.CombineStat(_base[i] + _flat[i], _inc[i], _more[i]);
        }

        public float RawIncreased(StatId id)
        {
            return _inc[(int)id];
        }

        public float RawMore(StatId id)
        {
            return _more[(int)id];
        }

        public float RawFlat(StatId id)
        {
            return _base[(int)id] + _flat[(int)id];
        }
    }

    public struct Trigger
    {
        public EventId Event;
        public EffectId Effect;
        public float Cooldown;
        public int MaxDepth;
        public SkillId Skill;
    }

    public struct TriggerContext
    {
        public EventId Event;
        public int Depth;
        public SkillId Skill;
        public int DummyIndex;
        public float X;
        public float Z;
        public float DirX;
        public float DirZ;
        public HitResult Hit;
        public HitRequest Request;
        public float Radius;
        public float Speed;
        public float MaxDistance;
        public float Traveled;
    }

    public sealed class TriggerSystem
    {
        public const int DepthCap = 8;
        public const int MaxTriggers = 16;

        readonly Trigger[] _triggers = new Trigger[MaxTriggers];
        readonly float[] _remain = new float[MaxTriggers];
        int _count;
        int _depth;
        public int FireCount;
        public int DepthBlocked;
        public int CooldownBlocked;
        public Action<EffectId, TriggerContext> Handler;

        public int Depth
        {
            get { return _depth; }
        }

        public void Clear()
        {
            _count = 0;
            _depth = 0;
            FireCount = 0;
            DepthBlocked = 0;
            CooldownBlocked = 0;
            for (int i = 0; i < MaxTriggers; i++)
            {
                _triggers[i] = default;
                _remain[i] = 0f;
            }
        }

        public void Add(Trigger trigger)
        {
            if (_count >= MaxTriggers)
                return;
            if (trigger.MaxDepth <= 0)
                trigger.MaxDepth = 1;
            _triggers[_count] = trigger;
            _remain[_count] = 0f;
            _count++;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_remain[i] > 0f)
                {
                    _remain[i] -= dt;
                    if (_remain[i] < 0f)
                        _remain[i] = 0f;
                }
            }
        }

        public void Fire(EventId ev, TriggerContext ctx)
        {
            if (_depth >= DepthCap)
            {
                DepthBlocked++;
                return;
            }

            _depth++;
            ctx.Depth = _depth;
            try
            {
                for (int i = 0; i < _count; i++)
                {
                    Trigger t = _triggers[i];
                    if (t.Event != ev)
                        continue;
                    if (t.Skill != SkillId.None && t.Skill != ctx.Skill)
                        continue;
                    if (ctx.Depth > t.MaxDepth)
                    {
                        DepthBlocked++;
                        continue;
                    }

                    if (_remain[i] > 0f)
                    {
                        CooldownBlocked++;
                        continue;
                    }

                    FireCount++;
                    _remain[i] = t.Cooldown;
                    ctx.Event = ev;
                    if (Handler != null)
                        Handler(t.Effect, ctx);
                }
            }
            finally
            {
                _depth--;
            }
        }
    }

    public static class SkillTags
    {
        public static Tag Of(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return Tag.Attack | Tag.Melee | Tag.Hit | Tag.Physical;
            if (skill == SkillId.Projectile)
                return Tag.Attack | Tag.Projectile | Tag.Hit | Tag.Physical;
            if (skill == SkillId.Area)
                return Tag.Spell | Tag.Area | Tag.Hit | Tag.Physical;
            return Tag.None;
        }
    }
}
