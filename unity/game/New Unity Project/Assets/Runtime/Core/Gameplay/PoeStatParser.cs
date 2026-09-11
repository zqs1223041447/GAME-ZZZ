using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Game.Runtime.Core
{
    /// <summary>
    /// PoE 原始词条文本 -> 引擎可结算 Modifier 的保守映射（真实天赋树接入）。
    /// 只承认 Kernel.StatId/ModOp 已有语义的固定句式；任何带附加限定词的句子
    /// （召唤物/图腾/战吼/异常状态/药剂/充能/吸取/持续伤害/条件句/括号提示文本等）一律静默跳过——
    /// 错配一个数值比漏掉一行更糟。永不抛异常：空/垃圾输入返回空数组。
    /// </summary>
    public static class PoeStatParser
    {
        static readonly Modifier[] Empty = new Modifier[0];

        static readonly Dictionary<string, Modifier[]> Cache = new Dictionary<string, Modifier[]>();

        static readonly Rule[] Rules = BuildRules();

        /// <summary>PoE stat text -> canonical modifiers the engine can apply. Unmappable lines are skipped.</summary>
        public static Modifier[] Parse(string statText)
        {
            var sink = new List<Modifier>(4);
            int mapped = MapLines(statText, sink);
            return mapped == 0 ? Empty : sink.ToArray();
        }

        /// <summary>Allocated node count that actually produced at least one modifier (diagnostics).</summary>
        public static int MappedLineCount(string statText)
        {
            return MapLines(statText, null);
        }

        /// <summary>Cache keyed by stat text; never allocates twice for the same node text.</summary>
        public static Modifier[] ParseCached(string statText)
        {
            if (string.IsNullOrEmpty(statText))
                return Empty;

            Modifier[] cached;
            if (Cache.TryGetValue(statText, out cached))
                return cached;

            Modifier[] parsed = Parse(statText);
            Cache[statText] = parsed;
            return parsed;
        }

        /// <summary>逐行映射；sink 为 null 时只计数不收集（诊断路径无分配）。</summary>
        static int MapLines(string statText, List<Modifier> sink)
        {
            if (string.IsNullOrEmpty(statText))
                return 0;

            int mapped = 0;
            int start = 0;
            int len = statText.Length;
            while (start < len)
            {
                int nl = statText.IndexOf('\n', start);
                string line;
                if (nl < 0)
                {
                    line = statText.Substring(start);
                    start = len;
                }
                else
                {
                    line = statText.Substring(start, nl - start);
                    start = nl + 1;
                }

                Modifier mod;
                if (!TryMapLine(line, out mod))
                    continue;

                mapped++;
                if (sink != null)
                    sink.Add(mod);
            }
            return mapped;
        }

        static bool TryMapLine(string raw, out Modifier mod)
        {
            mod = default;
            if (raw == null)
                return false;

            string line = raw.Trim();
            if (line.Length == 0 || line[0] == '(')
                return false;

            for (int i = 0; i < Rules.Length; i++)
            {
                Rule rule = Rules[i];
                Match m = rule.Pattern.Match(line);
                if (!m.Success)
                    continue;

                float number;
                if (!TryParseNumber(m.Groups[1].Value, out number))
                    continue;

                mod = Modifier.Make(rule.Stat, rule.Op, number * rule.Scale);
                return true;
            }
            return false;
        }

        static bool TryParseNumber(string s, out float value)
        {
            value = 0f;
            if (string.IsNullOrEmpty(s))
                return false;
            if (s.IndexOf(',') >= 0)
                s = s.Replace(",", "");
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        struct Rule
        {
            public Regex Pattern;
            public StatId Stat;
            public ModOp Op;
            public float Scale;
        }

        static Rule R(string pattern, StatId stat, ModOp op, float scale)
        {
            return new Rule
            {
                Pattern = new Regex(pattern, RegexOptions.CultureInvariant),
                Stat = stat,
                Op = op,
                Scale = scale
            };
        }

        /// <summary>
        /// 句式表：^...$ 全行锚定，附加限定词（while/if/with/per/against...）必然落空 -> 跳过。
        /// Percent 数值统一 /100（引擎以 0.12 表示 12%）；flat 属性/生命/魔力保持原值。
        /// </summary>
        static Rule[] BuildRules()
        {
            const string N = "([0-9][0-9,]*(?:\\.[0-9]+)?)";
            return new[]
            {
                // 属性 / 资源 flat
                R("^\\+?" + N + " to Strength$", StatId.Strength, ModOp.Flat, 1f),
                R("^\\+?" + N + " to Dexterity$", StatId.Dexterity, ModOp.Flat, 1f),
                R("^\\+?" + N + " to Intelligence$", StatId.Intelligence, ModOp.Flat, 1f),
                R("^\\+?" + N + " to maximum Life$", StatId.Life, ModOp.Flat, 1f),
                R("^\\+?" + N + " to maximum Mana$", StatId.Mana, ModOp.Flat, 1f),
                // S6P-WO-04C：无条件句式接到已有 consumer（Get() 已消费 Increased/Flat）
                R("^\\+?" + N + "% increased maximum Life$", StatId.Life, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased maximum Mana$", StatId.Mana, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Strength$", StatId.Strength, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Dexterity$", StatId.Dexterity, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Intelligence$", StatId.Intelligence, ModOp.Increased, 0.01f),
                R("^\\+?" + N + " to Armour$", StatId.Armour, ModOp.Flat, 1f),
                R("^\\+?" + N + " to Evasion Rating$", StatId.Evasion, ModOp.Flat, 1f),
                R("^\\+?" + N + " to Accuracy Rating$", StatId.Accuracy, ModOp.Flat, 1f),

                // 防御 increased
                R("^\\+?" + N + "% increased Armour$", StatId.Armour, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Evasion Rating$", StatId.Evasion, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Accuracy Rating$", StatId.Accuracy, ModOp.Increased, 0.01f),

                // 抗性（引擎无冰冷/闪电轴：全元素抗性只记火焰，不虚构）
                R("^\\+?" + N + "% to Fire Resistance$", StatId.FireResistance, ModOp.Flat, 0.01f),
                R("^\\+?" + N + "% to all Elemental Resistances$", StatId.FireResistance, ModOp.Flat, 0.01f),
                R("^\\+?" + N + "% to maximum Fire Resistance$", StatId.MaxFireResistance, ModOp.Flat, 0.01f),

                // 攻击速度
                R("^\\+?" + N + "% increased Attack Speed$", StatId.AttackSpeed, ModOp.Increased, 0.01f),

                // 伤害 increased（先具体后泛化；全行锚定使 "Global"/"Attack" 修饰不冲突）
                R("^\\+?" + N + "% increased Global Physical Damage$", StatId.PhysicalDamage, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Physical Damage$", StatId.PhysicalDamage, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Fire Damage$", StatId.FireDamage, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Attack Damage$", StatId.Damage, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% increased Damage$", StatId.Damage, ModOp.Increased, 0.01f),

                // 暴击
                R("^\\+?" + N + "% increased Critical Strike Chance$", StatId.CritChanceIncreased, ModOp.Increased, 0.01f),
                R("^\\+?" + N + "% to Critical Strike Multiplier$", StatId.CritMultiAdded, ModOp.Flat, 0.01f),

                // 范围半径（AreaDamageMore 只走 RawMore：禁止把 "increased Area Damage" 接到 Increased，那是静默空转）
                R("^\\+?" + N + "% increased Area of Effect$", StatId.AreaRadiusMore, ModOp.Increased, 0.01f),

                // 点燃
                R("^\\+?" + N + "% increased Ignite Chance$", StatId.IgniteChance, ModOp.Flat, 0.01f),
                R("^\\+?" + N + "% chance to Ignite$", StatId.IgniteChance, ModOp.Flat, 0.01f),

                // 物理转火焰（引擎唯一有语义的转换轴；Avatar of Fire 的多元素写法并入同一条）
                R("^\\+?" + N + "% of Physical, Cold and Lightning Damage Converted to Fire Damage$",
                    StatId.ConvertPhysToFire, ModOp.Flat, 0.01f),
                R("^\\+?" + N + "% of Physical Damage Converted to Fire Damage$",
                    StatId.ConvertPhysToFire, ModOp.Flat, 0.01f),

                // more 乘算（仅限引擎已有 More 轴的伤害类型）
                R("^\\+?" + N + "% more Physical Damage$", StatId.MorePhysical, ModOp.More, 0.01f),
                R("^\\+?" + N + "% more Fire Damage$", StatId.MoreFire, ModOp.More, 0.01f),
                R("^\\+?" + N + "% more Attack Damage$", StatId.MoreDamage, ModOp.More, 0.01f),
                R("^\\+?" + N + "% more Damage$", StatId.MoreDamage, ModOp.More, 0.01f)
            };
        }
    }
}
