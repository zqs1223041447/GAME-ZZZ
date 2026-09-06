namespace Game.Runtime.Core
{
    public struct Feedback
    {
        public bool Alive;
        public FeedbackKind Kind;
        public float X;
        public float Z;
        public float Age;
        public float Duration;
        public float Scale;
        public float YawDeg;
    }

    public sealed class FeedbackPool
    {
        public readonly Feedback[] Items = new Feedback[CombatRules.FeedbackPoolSize];
        public int AliveCount;

        public void Clear()
        {
            for (int i = 0; i < Items.Length; i++)
                Items[i] = default;
            AliveCount = 0;
        }

        public int Spawn(FeedbackKind kind, float x, float z, float duration)
        {
            return Spawn(kind, x, z, duration, DefaultScale(kind), 0f);
        }

        public int Spawn(FeedbackKind kind, float x, float z, float duration, float scale, float yawDeg)
        {
            int free = -1;
            int oldest = 0;
            float oldestAge = -1f;
            for (int i = 0; i < Items.Length; i++)
            {
                if (!Items[i].Alive)
                {
                    free = i;
                    break;
                }

                if (Items[i].Age > oldestAge)
                {
                    oldestAge = Items[i].Age;
                    oldest = i;
                }
            }

            if (free < 0)
            {
                free = oldest;
                if (Items[free].Alive)
                    AliveCount--;
            }

            Feedback f;
            f.Alive = true;
            f.Kind = kind;
            f.X = x;
            f.Z = z;
            f.Age = 0f;
            f.Duration = duration;
            f.Scale = scale;
            f.YawDeg = yawDeg;
            Items[free] = f;
            AliveCount++;
            return free;
        }

        static float DefaultScale(FeedbackKind kind)
        {
            if (kind == FeedbackKind.Area)
                return SkillCatalog.Get(SkillId.Area).AreaRadius * 2f;
            if (kind == FeedbackKind.MeleeSwing)
                return 1.8f;
            if (kind == FeedbackKind.Hit)
                return 0.95f;
            if (kind == FeedbackKind.Death)
                return 1.2f;
            if (kind == FeedbackKind.Loot)
                return 1.1f;
            if (kind == FeedbackKind.Ignite)
                return 1.2f;
            return 0.8f;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                ref Feedback f = ref Items[i];
                if (!f.Alive)
                    continue;
                f.Age += dt;
                if (f.Age >= f.Duration)
                {
                    f = default;
                    AliveCount--;
                }
            }
        }
    }
}
