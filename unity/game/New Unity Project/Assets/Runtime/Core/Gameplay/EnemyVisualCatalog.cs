namespace Game.Runtime.Core
{
    /// <summary>
    /// 敌人视觉映射表（S3-P5-ART-R4-SECOND-ENEMY-VISUAL）：EnemyKind → 可选正式视觉资源路径。
    /// 纯 presentation data——禁止存放 HP/伤害/速度/掉落/AI 等任何 gameplay 数据；
    /// null=该 kind 未接入正式视觉（保持基元 placeholder）。消费端仅 EnemyVisualPresenter 管线。
    /// </summary>
    public static class EnemyVisualCatalog
    {
        public static string VisualResourcePath(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Brute:
                    return RuntimeResourcePaths.EnemyTrollVisual;
                case EnemyKind.Stinger:
                    return RuntimeResourcePaths.EnemyFireLionVisual;
                case EnemyKind.Warden:
                    return RuntimeResourcePaths.EnemyBruceVisual;
                case EnemyKind.Ashling:
                    // R9：优化后重试接入（R8 曾被 Art Gate 阻断；若 R9 双 Gate 仍 FAIL 则按令回退 placeholder）
                    return RuntimeResourcePaths.EnemyGargoyleVisual;
                default:
                    return null;
            }
        }
    }
}
