using System.Collections.Generic;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Skill Tag golden（S3-P12-AUDIT-CLOSEOUT）：人工钉死当前 3 个 Active Skill 的完整预期 Tag mask。
    /// 独立 oracle——不得调用 SkillTags.Of() 生成 expected；parity 测试验证 Runtime 与本表一致，
    /// Runtime Tag 被意外删改（如丢 Physical/Hit）时测试变红。
    /// </summary>
    public static class SkillTagGolden
    {
        public static readonly Dictionary<SkillId, Tag> Masks = new Dictionary<SkillId, Tag>
        {
            { SkillId.Melee, Tag.Attack | Tag.Melee | Tag.Hit | Tag.Physical },
            { SkillId.Projectile, Tag.Attack | Tag.Projectile | Tag.Hit | Tag.Physical },
            { SkillId.Area, Tag.Spell | Tag.Area | Tag.Hit | Tag.Physical }
        };

        /// <summary>当前已声明 Tag 全集（与 Kernel.Tag 枚举一致）。</summary>
        public const Tag DeclaredTags =
            Tag.Attack | Tag.Spell | Tag.Melee | Tag.Projectile | Tag.Area |
            Tag.Hit | Tag.Physical | Tag.Fire | Tag.Duration;
    }

    /// <summary>
    /// Tag 组合规则（当前 Slice invariant，非永久架构哲学；每条规则可独立否决，供负向测试用合成输入）。
    /// Rule A 未声明 bit 禁止；Rule C Attack+Spell 当前形态冲突；Rule D Melee+Projectile 当前形态冲突；
    /// Rule E RequiredTags 必须至少被一个当前 Active Skill 满足（否则=死 Tagged Modifier）。
    /// </summary>
    public static class ContentAuditTagRules
    {
        /// <summary>校验技能 Tag mask（Rule A/C/D）。返回 null=通过，否则为违规原因。</summary>
        public static string ValidateSkillMask(SkillId skill, Tag mask)
        {
            if (((uint)mask & ~(uint)SkillTagGolden.DeclaredTags) != 0)
                return skill + " 含未声明 Tag bit";
            if ((mask & Tag.Attack) != 0 && (mask & Tag.Spell) != 0)
                return skill + " Attack+Spell 当前形态冲突";
            if ((mask & Tag.Melee) != 0 && (mask & Tag.Projectile) != 0)
                return skill + " Melee+Projectile 当前形态冲突";
            return null;
        }

        /// <summary>校验内容 RequiredTags（Rule A/E）。返回 null=通过，否则为违规原因（死 Tagged Modifier 等）。</summary>
        public static string ValidateRequiredTags(Tag required)
        {
            if (required == Tag.None)
                return null;
            if (((uint)required & ~(uint)SkillTagGolden.DeclaredTags) != 0)
                return "RequiredTags 含未声明 Tag bit：" + required;
            foreach (var pair in SkillTagGolden.Masks)
            {
                if ((pair.Value & required) == required)
                    return null;
            }
            return "死 Tagged Modifier：当前无任何 Active Skill 可满足 " + required;
        }

        /// <summary>可满足该 RequiredTags 的当前 Active Skill 列表（报告/可达性用）。</summary>
        public static List<SkillId> SatisfyingSkills(Tag required)
        {
            var list = new List<SkillId>();
            foreach (var pair in SkillTagGolden.Masks)
            {
                if ((pair.Value & required) == required)
                    list.Add(pair.Key);
            }
            return list;
        }
    }
}
