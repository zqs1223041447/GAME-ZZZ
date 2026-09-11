using System.Collections.Generic;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Support×技能兼容 golden 期望（S3-B1-RCLOSE）。
    /// 独立 oracle：人工钉死的数据，不得由 Runtime 判定推导（防自证循环）。
    /// ContentAuditS2Tests 用它校验内容与钉死组合；SupportGateTests 用它校验 Runtime 判定与其完全一致。
    /// 判定依据（与 Runtime 契约同源但数据独立）：①Tag 路径=带 RequiredTags 的 Mod 在该技能 Tag 下可满足；
    /// ②机制路径=机制轴只接具备该轴 Tag 的技能（投射物投送=Tag.Projectile）。
    /// S6P-WO-05：冰矛/火球术两条新技能逐条钉死（含「不兼容」的负例：火焰转化需 Attack+Physical，
    /// 冰矛无 Attack、火球术无 Physical，故两者都不接）。
    /// </summary>
    public static class SupportCompatGolden
    {
        public static readonly Dictionary<SupportId, SkillId[]> Matrix = new Dictionary<SupportId, SkillId[]>
        {
            { SupportId.AddedFire, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area, SkillId.IceSpear, SkillId.Fireball } },
            { SupportId.Brutal, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area, SkillId.IceSpear, SkillId.Fireball } },
            { SupportId.Concentrated, new[] { SkillId.Area, SkillId.Fireball } },
            { SupportId.Faster, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area, SkillId.IceSpear, SkillId.Fireball } },
            { SupportId.Combustion, new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area, SkillId.IceSpear, SkillId.Fireball } },
            { SupportId.Fork, new[] { SkillId.Projectile, SkillId.IceSpear, SkillId.Fireball } },
            // S3 R2（S3-R2-FIRE-CONVERSION）：RequiredTags=Attack|Hit|Physical——近战/弹道满足，范围=Spell 无 Attack 不满足
            { SupportId.FireConversion, new[] { SkillId.Melee, SkillId.Projectile } },
            // S6P-WO-05：两条机制型 Support 只接投射物投送（MechanicSkill=Projectile → Tag.Projectile）
            { SupportId.ReturningProjectiles, new[] { SkillId.Projectile, SkillId.IceSpear, SkillId.Fireball } },
            { SupportId.SnipersMark, new[] { SkillId.Projectile, SkillId.IceSpear, SkillId.Fireball } }
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
