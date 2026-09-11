using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// PoeStatParser：PoE 原始词条文本 -> 引擎 Modifier 的保守映射契约（真实 2429 节点天赋树接入）。
    /// 逐条覆盖映射表 / 多行聚合 / 不可映射静默跳过 / 垃圾输入不抛异常 / ParseCached 同文本同实例。
    /// </summary>
    public sealed class PoeStatParserTests
    {
        [Test]
        public void AttributeFlat_MapsMatchingStat()
        {
            var str = PoeStatParser.Parse("+10 to Strength");
            Assert.AreEqual(1, str.Length);
            Assert.AreEqual(StatId.Strength, str[0].Stat);
            Assert.AreEqual(ModOp.Flat, str[0].Op);
            Assert.AreEqual(10f, str[0].Value, 0.0001f);

            Assert.AreEqual(StatId.Dexterity, PoeStatParser.Parse("+12 to Dexterity")[0].Stat);
            Assert.AreEqual(StatId.Intelligence, PoeStatParser.Parse("+13 to Intelligence")[0].Stat);
        }

        [Test]
        public void LifeAndManaFlat()
        {
            var life = PoeStatParser.Parse("+40 to maximum Life");
            Assert.AreEqual(1, life.Length);
            Assert.AreEqual(StatId.Life, life[0].Stat);
            Assert.AreEqual(ModOp.Flat, life[0].Op);
            Assert.AreEqual(40f, life[0].Value, 0.0001f);

            var mana = PoeStatParser.Parse("+25 to maximum Mana");
            Assert.AreEqual(1, mana.Length);
            Assert.AreEqual(StatId.Mana, mana[0].Stat);
            Assert.AreEqual(ModOp.Flat, mana[0].Op);
            Assert.AreEqual(25f, mana[0].Value, 0.0001f);
        }

        [Test]
        public void ArmourAndEvasionIncreased()
        {
            var armour = PoeStatParser.Parse("24% increased Armour");
            Assert.AreEqual(StatId.Armour, armour[0].Stat);
            Assert.AreEqual(ModOp.Increased, armour[0].Op);
            Assert.AreEqual(0.24f, armour[0].Value, 0.0001f);

            var evasion = PoeStatParser.Parse("14% increased Evasion Rating");
            Assert.AreEqual(StatId.Evasion, evasion[0].Stat);
            Assert.AreEqual(ModOp.Increased, evasion[0].Op);
            Assert.AreEqual(0.14f, evasion[0].Value, 0.0001f);

            var accuracy = PoeStatParser.Parse("20% increased Accuracy Rating");
            Assert.AreEqual(StatId.Accuracy, accuracy[0].Stat);
            Assert.AreEqual(ModOp.Increased, accuracy[0].Op);
        }

        [Test]
        public void AllElementalResistances_MapsFireOnly()
        {
            var all = PoeStatParser.Parse("+12% to all Elemental Resistances");
            Assert.AreEqual(1, all.Length, "不得虚构冰冷/闪电抗性轴");
            Assert.AreEqual(StatId.FireResistance, all[0].Stat);
            Assert.AreEqual(ModOp.Flat, all[0].Op);
            Assert.AreEqual(0.12f, all[0].Value, 0.0001f);

            var fire = PoeStatParser.Parse("+15% to Fire Resistance");
            Assert.AreEqual(1, fire.Length);
            Assert.AreEqual(StatId.FireResistance, fire[0].Stat);
            Assert.AreEqual(0.15f, fire[0].Value, 0.0001f);
        }

        [Test]
        public void AttackSpeedAndCrit()
        {
            var speed = PoeStatParser.Parse("8% increased Attack Speed");
            Assert.AreEqual(StatId.AttackSpeed, speed[0].Stat);
            Assert.AreEqual(ModOp.Increased, speed[0].Op);
            Assert.AreEqual(0.08f, speed[0].Value, 0.0001f);

            var crit = PoeStatParser.Parse("30% increased Critical Strike Chance");
            Assert.AreEqual(StatId.CritChanceIncreased, crit[0].Stat);
            Assert.AreEqual(ModOp.Increased, crit[0].Op);
            Assert.AreEqual(0.30f, crit[0].Value, 0.0001f);

            var multi = PoeStatParser.Parse("+15% to Critical Strike Multiplier");
            Assert.AreEqual(StatId.CritMultiAdded, multi[0].Stat);
            Assert.AreEqual(ModOp.Flat, multi[0].Op);
            Assert.AreEqual(0.15f, multi[0].Value, 0.0001f);
        }

        [Test]
        public void DamageAreaIgniteAndMore()
        {
            Assert.AreEqual(StatId.Damage, PoeStatParser.Parse("16% increased Damage")[0].Stat);
            Assert.AreEqual(StatId.Damage, PoeStatParser.Parse("16% increased Attack Damage")[0].Stat);
            Assert.AreEqual(StatId.PhysicalDamage, PoeStatParser.Parse("16% increased Physical Damage")[0].Stat);
            Assert.AreEqual(StatId.PhysicalDamage, PoeStatParser.Parse("16% increased Global Physical Damage")[0].Stat);
            Assert.AreEqual(StatId.FireDamage, PoeStatParser.Parse("16% increased Fire Damage")[0].Stat);

            var area = PoeStatParser.Parse("10% increased Area of Effect");
            Assert.AreEqual(StatId.AreaRadiusMore, area[0].Stat);
            Assert.AreEqual(ModOp.Increased, area[0].Op);

            var ignite = PoeStatParser.Parse("15% increased Ignite Chance");
            Assert.AreEqual(StatId.IgniteChance, ignite[0].Stat);
            Assert.AreEqual(ModOp.Flat, ignite[0].Op);
            Assert.AreEqual(0.15f, ignite[0].Value, 0.0001f);

            var chance = PoeStatParser.Parse("20% chance to Ignite");
            Assert.AreEqual(StatId.IgniteChance, chance[0].Stat);
            Assert.AreEqual(ModOp.Flat, chance[0].Op);

            var more = PoeStatParser.Parse("20% more Fire Damage");
            Assert.AreEqual(StatId.MoreFire, more[0].Stat);
            Assert.AreEqual(ModOp.More, more[0].Op);
            Assert.AreEqual(0.20f, more[0].Value, 0.0001f);
        }

        [Test]
        public void MultiLineBlock_ProducesSeveralModifiers()
        {
            const string block =
                "+10 to Strength\n" +
                "+40 to maximum Life\n" +
                "14% increased Evasion Rating\n" +
                "8% increased Attack Speed\n" +
                "(reminder text that must be ignored)";
            var mods = PoeStatParser.Parse(block);
            Assert.AreEqual(4, mods.Length);
            Assert.AreEqual(4, PoeStatParser.MappedLineCount(block));
            Assert.AreEqual(StatId.Strength, mods[0].Stat);
            Assert.AreEqual(StatId.Life, mods[1].Stat);
            Assert.AreEqual(StatId.Evasion, mods[2].Stat);
            Assert.AreEqual(StatId.AttackSpeed, mods[3].Stat);
        }

        [Test]
        public void UnmappableLines_SkippedSilently()
        {
            const string block =
                "Minions deal 20% increased Damage\n" +
                "15% increased Warcry Cooldown Recovery Rate\n" +
                "Gain 15 Life per Enemy Killed\n" +
                "4% of Damage taken Recouped as Life\n" +
                "100% increased Attack Critical Strike Chance while Dual Wielding\n" +
                "Regenerate 2% of Life per second\n" +
                "(Only Damage from Hits can be Recouped, over 4 seconds following the Hit)";
            var mods = PoeStatParser.Parse(block);
            Assert.AreEqual(0, mods.Length);
            Assert.AreEqual(0, PoeStatParser.MappedLineCount(block));

            // 混排：可映射行仍被取出，其余静默丢弃
            var mixed = PoeStatParser.Parse("Minions deal 20% increased Damage\n+10 to Strength");
            Assert.AreEqual(1, mixed.Length);
            Assert.AreEqual(StatId.Strength, mixed[0].Stat);
        }

        [Test]
        public void RobustInput_ThousandsSeparatorAndCrLf()
        {
            var mods = PoeStatParser.Parse("+1,234 to maximum Life\r\n10% increased Attack Speed");
            Assert.AreEqual(2, mods.Length);
            Assert.AreEqual(1234f, mods[0].Value, 0.0001f);
            Assert.AreEqual(0.10f, mods[1].Value, 0.0001f);
        }

        [Test]
        public void GarbageInput_ReturnsEmpty_DoesNotThrow()
        {
            Assert.IsNotNull(PoeStatParser.Parse(null));
            Assert.AreEqual(0, PoeStatParser.Parse(null).Length);
            Assert.AreEqual(0, PoeStatParser.Parse("").Length);
            Assert.AreEqual(0, PoeStatParser.Parse("   ").Length);
            Assert.AreEqual(0, PoeStatParser.Parse("???").Length);
            Assert.AreEqual(0, PoeStatParser.Parse("Grant nothing 29161").Length);
            Assert.AreEqual(0, PoeStatParser.Parse("Reminder text in parentheses").Length);
            Assert.AreEqual(0, PoeStatParser.MappedLineCount(null));
            Assert.AreEqual(0, PoeStatParser.ParseCached(null).Length);
        }

        [Test]
        public void ParseCached_SameText_ReturnsSameInstance()
        {
            const string text = "+10 to Strength\n14% increased Evasion Rating";
            var a = PoeStatParser.ParseCached(text);
            var b = PoeStatParser.ParseCached(text);
            Assert.AreSame(a, b, "同节点文本必须命中缓存，不重复分配");
            Assert.AreEqual(2, a.Length);
        }

        // ---------- S6P-WO-04C：已有 consumer 的无条件句式 ----------

        [Test]
        public void Wo04C_ExistingConsumerExactUncond_Maps()
        {
            AssertMod("5% increased maximum Life", StatId.Life, ModOp.Increased, 0.05f);
            AssertMod("10% increased maximum Life", StatId.Life, ModOp.Increased, 0.10f);
            AssertMod("8% increased maximum Mana", StatId.Mana, ModOp.Increased, 0.08f);
            AssertMod("12% increased Strength", StatId.Strength, ModOp.Increased, 0.12f);
            AssertMod("12% increased Dexterity", StatId.Dexterity, ModOp.Increased, 0.12f);
            AssertMod("12% increased Intelligence", StatId.Intelligence, ModOp.Increased, 0.12f);
            AssertMod("+50 to Armour", StatId.Armour, ModOp.Flat, 50f);
            AssertMod("+30 to Evasion Rating", StatId.Evasion, ModOp.Flat, 30f);
            AssertMod("+150 to Accuracy Rating", StatId.Accuracy, ModOp.Flat, 150f);
            AssertMod("+1% to maximum Fire Resistance", StatId.MaxFireResistance, ModOp.Flat, 0.01f);
        }

        [Test]
        public void Wo04C_MinionConditionalConversionAndAreaIncreased_StayUnmapped()
        {
            Assert.AreEqual(0, PoeStatParser.Parse("Minions have 12% increased maximum Life").Length,
                "召唤物生命不是玩家 Life consumer");
            Assert.AreEqual(0, PoeStatParser.Parse("Minions have 15% increased maximum Life").Length);
            Assert.AreEqual(0, PoeStatParser.Parse("5% increased maximum Life while on Low Life").Length,
                "条件句不得剥掉 while");
            Assert.AreEqual(0, PoeStatParser.Parse(
                "Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating").Length,
                "Iron Reflexes 转换不是 +Evasion Flat");
            Assert.AreEqual(0, PoeStatParser.Parse("20% increased Area Damage").Length,
                "AreaDamageMore 只走 RawMore；Increased 会静默空转，禁止接入");
            Assert.AreEqual(0, PoeStatParser.Parse("10% increased Area Damage").Length);
        }

        static void AssertMod(string line, StatId stat, ModOp op, float value)
        {
            var mods = PoeStatParser.Parse(line);
            Assert.AreEqual(1, mods.Length, line);
            Assert.AreEqual(stat, mods[0].Stat, line);
            Assert.AreEqual(op, mods[0].Op, line);
            Assert.AreEqual(value, mods[0].Value, 0.0001f, line);
        }
    }
}
