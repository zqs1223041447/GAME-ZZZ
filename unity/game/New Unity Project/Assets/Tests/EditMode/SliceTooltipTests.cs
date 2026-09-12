using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 3 R3 统一 Tooltip 契约（工作令 S3-P3-UI-R3-TOOLTIP-COMPARE 二十八节）：
    /// hybrid 词缀两行全进模型、比较 key=(StatId,ModOp)、union 含 equipped-only 损失、
    /// 槽空无对比区、同物品=「已装备」零噪音、排序确定性、Support 兼容展示与 canonical
    /// IsSupportCompatible 全量 parity、右缘翻转/钳制、1440p 设计换算、模型纯读不改 session。
    /// 兼容性 golden=直接对齐 canonical 方法（不造第二套兼容逻辑）。
    /// </summary>
    public class SliceTooltipTests
    {
        const float Dw = 1920f;
        const float Dh = 1080f;

        static SliceSession Session()
        {
            var s = new SliceSession();
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;
            return s;
        }

        static ItemInstance Item(int id, EquipSlot slot, Rarity rar, string name, int affixCount)
        {
            ItemInstance it = default;
            it.Id = id;
            it.Slot = slot;
            it.Rarity = rar;
            it.SocketCount = 3;
            it.BaseName = name;
            it.AffixCount = affixCount;
            return it;
        }

        [Test]
        public void HybridAffix_BothRows_EnterModel()
        {
            ItemInstance it = Item(1, EquipSlot.Weapon, Rarity.Rare, "灼燃", 1);
            it.SetAffix(0, AffixId.IgniteFire, 0.20f, 0.15f);

            List<SliceTooltipModel.StatRow> rows = SliceTooltipModel.Aggregate(it);
            Assert.AreEqual(2, rows.Count, "hybrid 词缀两行必须全部进入模型");
            Assert.AreEqual(StatId.FireDamage, rows[0].Stat);
            Assert.AreEqual(ModOp.Increased, rows[0].Op);
            Assert.AreEqual(0.20f, rows[0].Value, 1e-4f);
            Assert.AreEqual(StatId.IgniteChance, rows[1].Stat);
            Assert.AreEqual(ModOp.Flat, rows[1].Op);
            Assert.AreEqual(0.15f, rows[1].Value, 1e-4f);

            // 已装备侧的 hybrid 第二行同样进 union（候选没有该 key ⇒ 必须显示损失）
            ItemInstance eq = Item(2, EquipSlot.Weapon, Rarity.Rare, "灼燃旧", 1);
            eq.SetAffix(0, AffixId.IgniteFire, 0.10f, 0.10f);
            List<SliceTooltipModel.StatRow> delta = SliceTooltipModel.Compare(it, eq);
            Assert.AreEqual(2, delta.Count);
            Assert.AreEqual(StatId.FireDamage, delta[0].Stat);
            Assert.AreEqual(StatId.IgniteChance, delta[1].Stat);
            Assert.AreEqual(0.05f, delta[1].Value, 1e-4f, "hybrid 第二行损失必须可见（0.15-0.10）");
        }

        [Test]
        public void Comparison_KeyIsStatIdPlusModOp()
        {
            // 候选 IncPhys(+20%)；已装备 PhysFire(12%,12%)——不同词缀名、同 key (PhysicalDamage,Increased)
            ItemInstance cand = Item(1, EquipSlot.Weapon, Rarity.Rare, "新刃", 1);
            cand.SetAffix(0, AffixId.IncPhys, 0.20f, 0f);
            ItemInstance eq = Item(2, EquipSlot.Weapon, Rarity.Rare, "熔铸旧刃", 1);
            eq.SetAffix(0, AffixId.PhysFire, 0.12f, 0.12f);

            List<SliceTooltipModel.StatRow> delta = SliceTooltipModel.Compare(cand, eq);
            Assert.AreEqual(2, delta.Count, "按 (StatId,ModOp) 聚合：物理提高合并、火焰提高=equipped-only 损失");
            Assert.AreEqual(StatId.PhysicalDamage, delta[0].Stat);
            Assert.AreEqual(ModOp.Increased, delta[0].Op);
            Assert.AreEqual(0.08f, delta[0].Value, 1e-4f);
            Assert.AreEqual(StatId.FireDamage, delta[1].Stat);
            Assert.AreEqual(ModOp.Increased, delta[1].Op);
            Assert.AreEqual(-0.12f, delta[1].Value, 1e-4f);

            string physLine = SliceTooltipModel.FormatStat(delta[0].Stat, delta[0].Op, delta[0].Value);
            string fireLine = SliceTooltipModel.FormatStat(delta[1].Stat, delta[1].Op, delta[1].Value);
            StringAssert.Contains("+8%", physLine);
            StringAssert.Contains("物理伤害", physLine);
            StringAssert.StartsWith("-", fireLine);
            StringAssert.Contains("火焰伤害", fireLine);
        }

        [Test]
        public void UnionComparison_IncludesEquippedOnlyLoss()
        {
            ItemInstance cand = Item(1, EquipSlot.Body, Rarity.Rare, "新甲", 1);
            cand.SetAffix(0, AffixId.IncPhys, 0.20f, 0f);
            ItemInstance eq = Item(2, EquipSlot.Body, Rarity.Rare, "旧甲", 2);
            eq.SetAffix(0, AffixId.IncPhys, 0.12f, 0f);
            eq.SetAffix(1, AffixId.Life, 20f, 0f);

            List<SliceTooltipModel.StatRow> delta = SliceTooltipModel.Compare(cand, eq);
            Assert.AreEqual(2, delta.Count);
            bool lifeLoss = false;
            foreach (SliceTooltipModel.StatRow r in delta)
                if (r.Stat == StatId.Life && r.Op == ModOp.Flat && r.Value < 0f)
                    lifeLoss = true;
            Assert.IsTrue(lifeLoss, "equipped-only 属性必须以负 delta 进入 union 比较");
        }

        [Test]
        public void NoEquippedItem_NoComparisonSection()
        {
            var s = Session();
            ItemInstance cand = Item(1, EquipSlot.Helmet, Rarity.Rare, "新盔", 1);
            cand.SetAffix(0, AffixId.Armour, 30f, 0f);

            var card = SliceTooltipModel.ItemCard(cand, s);
            Assert.IsNull(card.ContextTitle, "槽空=只显示候选本身，不伪造对比");
            Assert.IsNull(card.Context);
            StringAssert.Contains("点击", card.Footer);
        }

        [Test]
        public void SameEquippedItem_Badge_NoZeroDeltaSpam()
        {
            var s = Session();
            ItemInstance it = Item(1, EquipSlot.Weapon, Rarity.Rare, "在装", 2);
            it.SetAffix(0, AffixId.Life, 20f, 0f);
            it.SetAffix(1, AffixId.Armour, 30f, 0f);
            int idx = s.AddItem(it);
            s.Equipped[(int)EquipSlot.Weapon] = idx;

            var card = SliceTooltipModel.ItemCard(s.Inventory[idx], s);
            Assert.AreEqual("已装备", card.Badge);
            Assert.IsNull(card.ContextTitle, "同物品不得出现全零对比区");
            Assert.IsNull(card.Context);
        }

        [Test]
        public void Comparison_Ordering_Deterministic()
        {
            ItemInstance cand = Item(1, EquipSlot.Body, Rarity.Rare, "排序候选", 3);
            cand.SetAffix(0, AffixId.Life, 20f, 0f);
            cand.SetAffix(1, AffixId.IncFire, 0.25f, 0f);
            cand.SetAffix(2, AffixId.AccCrit, 30f, 0.25f);
            ItemInstance eq = Item(2, EquipSlot.Body, Rarity.Rare, "排序已装", 3);
            eq.SetAffix(0, AffixId.AddFire, 6f, 0f);
            eq.SetAffix(1, AffixId.Evasion, 30f, 0f);
            eq.SetAffix(2, AffixId.Crit, 0.20f, 0f);

            List<SliceTooltipModel.StatRow> a = SliceTooltipModel.Compare(cand, eq);
            List<SliceTooltipModel.StatRow> b = SliceTooltipModel.Compare(cand, eq);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Stat, b[i].Stat, "重复调用顺序必须稳定");
                Assert.AreEqual(a[i].Op, b[i].Op);
                Assert.AreEqual(a[i].Value, b[i].Value, 1e-4f);
            }
            for (int i = 1; i < a.Count; i++)
            {
                int ka = (int)a[i - 1].Stat * 8 + (int)a[i - 1].Op;
                int kb = (int)a[i].Stat * 8 + (int)a[i].Op;
                Assert.Less(ka, kb, "排序=StatId enum 序，同 Stat 按 ModOp 序");
            }
        }

        [Test]
        public void SupportPresentation_MatchesCanonicalMethod()
        {
            var s = Session();
            // 分裂（机制型，限弹道）：canonical=近战✗/弹道✓/范围✗
            var fork = SliceTooltipModel.SupportCard(SupportId.Fork, s);
            Assert.AreEqual(3, fork.Context.Length);
            StringAssert.StartsWith("× Q", fork.Context[0]);
            StringAssert.StartsWith("✓ W", fork.Context[1]);
            StringAssert.StartsWith("× E", fork.Context[2]);
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.Fork, SkillId.Melee));
            Assert.IsTrue(SliceSession.IsSupportCompatible(SupportId.Fork, SkillId.Projectile));

            // 燃烧（增强型）：canonical 三技能全兼容
            var fire = SliceTooltipModel.SupportCard(SupportId.AddedFire, s);
            for (int i = 0; i < 3; i++)
                StringAssert.StartsWith("✓", fire.Context[i]);
        }

        [Test]
        public void FullSkillSupportParity_ThroughPresentation()
        {
            var s = Session();
            SkillId[] skills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };
            for (int i = 1; i <= SupportCatalog.Count; i++) // 从目录 sentinel 取真实数量，不硬编码 7
            {
                SupportId support = (SupportId)i;
                var card = SliceTooltipModel.SupportCard(support, s);
                Assert.AreEqual(3, card.Context.Length);
                for (int k = 0; k < skills.Length; k++)
                {
                    bool canonical = SliceSession.IsSupportCompatible(support, skills[k]);
                    bool shown = card.Context[k].StartsWith("✓");
                    Assert.AreEqual(canonical, shown,
                        SupportCatalog.Get(support).Name + " × " + skills[k] + " 展示必须与 canonical 兼容判定一致");
                }
            }
        }

        [Test]
        public void DescriptionStyle_DoesNotClipWrappedGlyphs()
        {
            GUIStyle st = SliceTooltipLayout.DescriptionStyle();
            Assert.IsTrue(st.wordWrap);
            Assert.AreEqual(TextClipping.Overflow, st.clipping);
            string line = "增加 20% 最大生命，并附加足够长的描述以在 tooltip 内宽处换行，避免被单行高度裁掉";
            float need = SliceTooltipLayout.BodyLineHeight(line);
            Assert.Greater(need, SliceTooltipLayout.BodyFont + 4f);
            var card = SliceTooltipModel.TextCard("装备描述", line);
            Assert.GreaterOrEqual(SliceTooltipLayout.CardHeight(card), need);
        }

        [Test]
        public void Placement_FlipsLeft_AtRightEdge()
        {
            // 抽屉槽中心（设计空间 x≈1827）+ 指针右下默认 → 必须翻到指针左侧且不出视口
            Rect r = SliceTooltipLayout.Place(new Vector2(1827f, 131f), SliceTooltipLayout.BaseW, 300f, Dw, Dh);
            Assert.LessOrEqual(r.xMax, Dw - SliceTooltipLayout.Margin);
            Assert.Less(r.x, 1827f, "右缘必须翻到指针左侧");
            Assert.GreaterOrEqual(r.x, SliceTooltipLayout.Margin);
            Assert.GreaterOrEqual(r.y, SliceTooltipLayout.Margin);
            Assert.LessOrEqual(r.yMax, Dh - SliceTooltipLayout.Margin);
        }

        [Test]
        public void Placement_ValidAt1440p_DesignScale()
        {
            float scale = SliceHud.DesignScale(2560f, 1440f);
            Assert.AreEqual(2560f / 1920f, scale, 0.0001f);
            Rect r = SliceTooltipLayout.Place(new Vector2(1600f, 900f), SliceTooltipLayout.BaseW, 400f, 1920f, 1080f);
            Assert.GreaterOrEqual(r.x, 0f);
            Assert.LessOrEqual(r.xMax, 1920f + 0.01f);
            Assert.GreaterOrEqual(r.y, 0f);
            Assert.LessOrEqual(r.yMax, 1080f + 0.01f);
            // 屏幕空间换算后仍须在 2560×1440 内
            Assert.LessOrEqual(r.xMax * scale, 2560f + 0.01f);
            Assert.LessOrEqual(r.yMax * scale, 1440f + 0.01f);
        }

        [Test]
        public void ModelCreation_DoesNotMutateSession()
        {
            var s = Session();
            ItemInstance eqW = Item(1, EquipSlot.Weapon, Rarity.Rare, "武器", 2);
            eqW.SetAffix(0, AffixId.IncPhys, 0.20f, 0f);
            eqW.SetAffix(1, AffixId.Life, 20f, 0f);
            int eqIdx = s.AddItem(eqW);
            s.Equipped[(int)EquipSlot.Weapon] = eqIdx;
            ItemInstance cand = Item(2, EquipSlot.Weapon, Rarity.Rare, "候选", 2);
            cand.SetAffix(0, AffixId.AddFire, 8f, 0f);
            cand.SetAffix(1, AffixId.AccCrit, 30f, 0.20f);
            s.AddItem(cand);
            ItemInstance eqB = Item(3, EquipSlot.Body, Rarity.Ordinary, "甲", 1);
            eqB.SetAffix(0, AffixId.Armour, 30f, 0f);
            int bodyIdx = s.AddItem(eqB);
            s.Equipped[(int)EquipSlot.Body] = bodyIdx;
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.AddedFire, out err), err);

            int[] equippedBefore = (int[])s.Equipped.Clone();
            int invBefore = s.InventoryCount;
            SkillId skillBefore = s.SelectedSkill;
            SupportId[] qBefore = (SupportId[])s.SupportsOf(SkillId.Melee).Clone();
            SupportId[] wBefore = (SupportId[])s.SupportsOf(SkillId.Projectile).Clone();
            SupportId[] eBefore = (SupportId[])s.SupportsOf(SkillId.Area).Clone();
            int nextIdBefore = s.NextItemId;
            int scrapBefore = s.Scrap;

            SliceTooltipModel.ItemCard(cand, s);
            SliceTooltipModel.ItemCard(s.Inventory[eqIdx], s);
            SliceTooltipModel.SupportCard(SupportId.Fork, s);
            SliceTooltipModel.TextCard("名", "描述");

            CollectionAssert.AreEqual(equippedBefore, s.Equipped);
            Assert.AreEqual(invBefore, s.InventoryCount);
            Assert.AreEqual(skillBefore, s.SelectedSkill);
            CollectionAssert.AreEqual(qBefore, s.SupportsOf(SkillId.Melee));
            CollectionAssert.AreEqual(wBefore, s.SupportsOf(SkillId.Projectile));
            CollectionAssert.AreEqual(eBefore, s.SupportsOf(SkillId.Area));
            Assert.AreEqual(nextIdBefore, s.NextItemId);
            Assert.AreEqual(scrapBefore, s.Scrap);
        }
    }
}
