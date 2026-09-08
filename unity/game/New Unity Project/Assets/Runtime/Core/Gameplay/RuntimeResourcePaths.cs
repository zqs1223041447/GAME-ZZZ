namespace Game.Runtime.Core
{
    /// <summary>
    /// Runtime 资源路径单一真相源（S3-M2-RESOURCE-CONTRACT-TRUTH）：只负责路径常量与拼接。
    /// 不做加载/缓存/管理（加载仍在各消费点）；审计侧「应有哪些 key、哪些允许缺失」保持独立 oracle，
    /// 不从此类型生成——Runtime 未审批增删 key 会由 declared-key parity 测试变红。
    /// </summary>
    public static class RuntimeResourcePaths
    {
        public const string PlayerDarkKnight = "Player/DarkKnight";

        /// <summary>敌人视觉预制体（S3-P5-ART-R3）：Brute 槽位的正式怪物视觉，缺失回退基元（降级）。</summary>
        public const string EnemyTrollVisual = "Enemies/TrollWarriorVisual";

        /// <summary>敌人视觉预制体（S3-P5-ART-R4）：Stinger 槽位的正式怪物视觉（火狮），缺失回退基元（降级）。</summary>
        public const string EnemyFireLionVisual = "Enemies/FireLionVisual";

        /// <summary>敌人视觉预制体（S3-P5-ART-R6）：Warden 槽位的正式怪物视觉（Bruce，role-specific 高价值位），缺失回退基元（降级）。</summary>
        public const string EnemyBruceVisual = "Enemies/BruceVisual";

        /// <summary>敌人视觉预制体（S3-P5-ART-R8/R9）：Ashling 槽位候选（石魔 Gargoyle）。R8 曾被 Art Gate 阻断，
        /// R9 经因果隔离与优化后重试接入；若 R9 双 Gate 仍 FAIL 则按令移除并回退 placeholder。</summary>
        public const string EnemyGargoyleVisual = "Enemies/GargoyleVisual";

        public static string CombatSfx(string key)
        {
            return "Audio/" + key;
        }

        public static string Voice(string key)
        {
            return "Audio/Voice/" + key;
        }
    }
}
