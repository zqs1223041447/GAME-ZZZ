using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// Phase 3 R3 统一 Tooltip 纯表现模型（工作令 S3-P3-UI-R3-TOOLTIP-COMPARE）。
    /// 只读转换已有 canonical 数据（ItemInstance/AffixCatalog/SupportCatalog/SliceSession.IsSupportCompatible），
    /// 不绘制、不修改 session、不成为第二规则源、不复制装备数值算法。
    /// 装备比较 key=(StatId,ModOp)（hybrid 词缀两行全展开），union 含 equipped-only 损失，仅展示 delta!=0。
    /// </summary>
    public static class SliceTooltipModel
    {
        public struct Card
        {
            public string Title;        // Header：名称（物品=CleanBaseName / 辅助=Name / 纯文本卡可空）
            public string Subtitle;     // Header：稀有度·槽位·孔数 / 辅助类型标注
            public string Badge;        // 状态标注（「已装备」）
            public string[] Body;       // 词缀行 / 描述行（canonical AffixLine / Def.Desc）
            public string ContextTitle; // 对比区标题（「与当前装备相比」/「兼容」）；null=无对比区
            public string[] Context;    // 对比行（可空）
            public string Footer;       // 操作提示（可空）
            public Rarity Rarity;       // 标题着色（物品卡；其余 Ordinary）
        }

        public struct StatRow
        {
            public StatId Stat;
            public ModOp Op;
            public float Value;
        }

        /// <summary>展开装备全部词缀行（hybrid 第二行一并进入），同 (StatId,ModOp) 求和聚合；顺序=词缀定义序（首次出现）。</summary>
        public static List<StatRow> Aggregate(ItemInstance it)
        {
            List<StatRow> rows = new List<StatRow>();
            for (int i = 0; i < it.AffixCount; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)it.AffixIdAt(i));
                for (int r = 0; r < def.RowCount; r++)
                    Merge(rows, def.RowStat(r), def.RowOp(r), RowValue(it, i, r));
            }
            return rows;
        }

        static float RowValue(ItemInstance it, int affixIndex, int row)
        {
            return row == 0 ? it.ValueAt(affixIndex) : it.SecondValueAt(affixIndex);
        }

        static void Merge(List<StatRow> rows, StatId stat, ModOp op, float value)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Stat == stat && rows[i].Op == op)
                {
                    StatRow m = rows[i];
                    m.Value += value;
                    rows[i] = m;
                    return;
                }
            }
            rows.Add(new StatRow { Stat = stat, Op = op, Value = value });
        }

        /// <summary>union 比较：candidate - equipped；只保留 delta!=0；排序=StatId enum 序，同 Stat 按 ModOp 序（确定性）。</summary>
        public static List<StatRow> Compare(ItemInstance candidate, ItemInstance equipped)
        {
            List<StatRow> union = Aggregate(candidate);
            List<StatRow> eq = Aggregate(equipped);
            for (int i = 0; i < eq.Count; i++)
                Merge(union, eq[i].Stat, eq[i].Op, -eq[i].Value);
            List<StatRow> delta = new List<StatRow>(union.Count);
            for (int i = 0; i < union.Count; i++)
            {
                if (Mathf.Abs(union[i].Value) > 1e-4f)
                    delta.Add(union[i]);
            }
            delta.Sort(CompareOrder);
            return delta;
        }

        static int CompareOrder(StatRow a, StatRow b)
        {
            if (a.Stat != b.Stat)
                return (int)a.Stat - (int)b.Stat;
            return (int)a.Op - (int)b.Op;
        }

        /// <summary>带符号行文本：按 (StatId,ModOp) 反查 AffixCatalog 既有 Format 模板（同 key 取 AffixId 序首个），正负号统一前置。</summary>
        public static string FormatStat(StatId stat, ModOp op, float delta)
        {
            string tpl = null;
            for (int i = 0; i < AffixCatalog.Count && tpl == null; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                if (def.Stat == stat && def.Op == op)
                    tpl = def.Format;
                else if (def.RowCount > 1 && def.Stat2 == stat && def.Op2 == op)
                    tpl = def.Format2;
            }
            if (tpl == null)
                tpl = "{0:0.##} " + stat; // 防御回退（当前目录词缀不产生该情况）
            if (tpl.Length > 0 && tpl[0] == '+')
                tpl = tpl.Substring(1);
            return (delta < 0f ? "-" : "+") + string.Format(tpl, Mathf.Abs(delta));
        }

        /// <summary>
        /// 物品卡。对比区仅在「候选对应某槽位、该槽已有不同装备」时出现（canonical equipped 同槽）；
        /// 槽空=只显示候选本身；同物品=「已装备」标注、无对比区。
        /// </summary>
        public static Card ItemCard(ItemInstance item, SliceSession session)
        {
            Card c = default;
            string baseName = SliceSession.CleanBaseName(item.BaseName);
            if (string.IsNullOrEmpty(baseName))
                baseName = SliceSession.ItemBaseName(item.Slot);
            c.Title = baseName;
            c.Subtitle = SliceSession.RarityWord(item.Rarity) + " · " + SliceSession.SlotName(item.Slot) +
                " · " + item.SocketCount + "孔";
            c.Rarity = item.Rarity;
            string[] body = new string[item.AffixCount];
            for (int i = 0; i < item.AffixCount; i++)
                body[i] = SliceSession.AffixLine(item, i);
            c.Body = body;

            int eq = session != null ? session.Equipped[(int)item.Slot] : -1;
            bool hasEq = eq >= 0 && session != null && eq < session.InventoryCount;
            if (!hasEq)
            {
                c.Footer = "点击装备";
                return c;
            }
            ItemInstance equipped = session.Inventory[eq];
            if (equipped.Id == item.Id)
            {
                c.Badge = "已装备";
                return c;
            }
            List<StatRow> delta = Compare(item, equipped);
            if (delta.Count == 0)
                return c; // 全零差异不刷屏：无对比区
            c.ContextTitle = "与当前装备相比";
            string[] lines = new string[delta.Count];
            for (int i = 0; i < delta.Count; i++)
                lines[i] = FormatStat(delta[i].Stat, delta[i].Op, delta[i].Value);
            c.Context = lines;
            c.Footer = "点击替换同槽装备";
            return c;
        }

        /// <summary>辅助卡：描述 + 逐技能兼容行（全部经 canonical IsSupportCompatible）+ 当前技能放置结论（Footer）。</summary>
        public static Card SupportCard(SupportId support, SliceSession session)
        {
            SupportDef def = SupportCatalog.Get(support);
            Card c = default;
            c.Title = def.Name;
            c.Subtitle = def.MechanicSkill != SkillId.None
                ? "机制型 · 限 " + SliceSession.SkillDisplayName(def.MechanicSkill)
                : def.ChangesMechanism ? "机制型" : "增强型";
            c.Rarity = Rarity.Ordinary;
            c.Body = new[] { def.Desc };

            SkillId[] skills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };
            string[] lines = new string[skills.Length];
            for (int i = 0; i < skills.Length; i++)
                lines[i] = (SliceSession.IsSupportCompatible(support, skills[i]) ? "✓ " : "× ") +
                    SliceSession.SkillHotkey(skills[i]) + " " + SliceSession.SkillDisplayName(skills[i]);
            c.ContextTitle = "兼容";
            c.Context = lines;

            SkillId cur = session != null ? session.SelectedSkill : SkillId.None;
            if (cur == SkillId.Melee || cur == SkillId.Projectile || cur == SkillId.Area)
                c.Footer = (SliceSession.IsSupportCompatible(support, cur) ? "可装配到当前技能（" : "与当前技能不兼容（") +
                    SliceSession.SkillHotkey(cur) + " " + SliceSession.SkillDisplayName(cur) + "）";
            return c;
        }

        /// <summary>纯文本卡（被动节点/截断提示等既有文字提示的统一出口）。</summary>
        public static Card TextCard(string title, string body)
        {
            Card c = default;
            c.Title = title;
            c.Rarity = Rarity.Ordinary;
            c.Body = string.IsNullOrEmpty(body) ? new string[0] : new[] { body };
            return c;
        }
    }
}
