using System.Collections.Generic;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Support×技能兼容 golden 期望（S3-B1-RCLOSE）。
    /// 独立 oracle：人工钉死的数据，不得由 Runtime 判定推导（防自证循环）。
    /// ContentAuditS2Tests 用它校验内容与钉死组合；SupportGateTests 用它校验 Runtime 判定与其完全一致。
    /// 判定依据（与 Runtime 契约同源但数据独立）：①Tag 路径=带 RequiredTags 的 Mod 在该技能 Tag 下可满足；
    /// ②机制路径=分裂（ForkProjectiles）只接入弹道结算。
    /// </summary>
    public static class SupportCompatGolden
    {
        public static readonly Dictionary<SupportId, SkillId[]> Matrix = new Dictionary<SupportId, SkillId[]>
        {
            { SupportId.AddedFire, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area } },
            { SupportId.Brutal, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area } },
            { SupportId.Concentrated, new[] { SkillId.Area } },
            { SupportId.Faster, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area } },
            { SupportId.Combustion, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area } },
            { SupportId.Fork, new[] { SkillId.Projectile } }
        };

        public static bool Contains(SupportId support, SkillId skill)
        {
            SkillId[] skills;
            if (!Matrix.TryGetValue(support, out skills) || skills == null)
                return false;
            for (int i = 0; i < skills.Length; i++)
                if (skills[i] == skill)
                    return true;
            return false;
        }
    }
}
