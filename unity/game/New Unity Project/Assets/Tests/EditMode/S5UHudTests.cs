using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5U-WO-02 战斗 HUD 换装契约（docs/reviews/S5U/S5U_VISUAL_CONTRACT.md §4/§5）：
    /// 紧凑数值格式化器 / 战斗底栏布局单一来源（双球+QWE 槽+tray 不重叠不出界，1920×1080 与 2560×1440 同设计空间）/
    /// 连接徽章真值映射（LinkBadgeText=LinkSourceLabel 同源）/容量真值映射 / 大数值适配球内文本 /
    /// 图标缓存幂等复用 / 禁 Socket Color 语义（PipFor 恒等映射）。语义冻结项零削弱：支持孔 closed/empty/filled 三态与旧 DrawSocket 1:1。
    /// </summary>
    public sealed class S5UHudTests
    {
        // ---------- L1 紧凑数值格式化器 ----------

        [TestCase(999f, "999")]
        [TestCase(1000f, "1.0K")]
        [TestCase(9999f, "10.0K")]
        [TestCase(10000f, "10.0K")]
        [TestCase(999940f, "999.9K")]
        [TestCase(999999f, "1.0M")]
        [TestCase(1000000f, "1.0M")]
        [TestCase(9999999f, "10.0M")]
        [TestCase(999999999999f, "1.0T")]
        public void Compact_Boundaries_FormatsExactly(float value, string expected)
        {
            Assert.AreEqual(expected, SliceHudFormat.Compact(value));
        }

        [Test]
        public void Compact_NeverEmitsThousandBoundaryArtifact()
        {
            // WO-03 合同修正（规划 AI）：K/M 段舍入达上段阈值=升位；禁 1000.0K/1000.0M 伪影
            for (float v = 999900f; v <= 1000000f; v += 10f)
                StringAssert.DoesNotContain("1000.0K", SliceHudFormat.Compact(v), "v=" + v);
            for (float v = 999900000000f; v <= 1000000000000f; v += 10000000f)
                StringAssert.DoesNotContain("1000.0M", SliceHudFormat.Compact(v), "v=" + v);
        }

        [Test]
        public void Compact_SmallAndNegative_RoundToInt()
        {
            Assert.AreEqual("0", SliceHudFormat.Compact(0f));
            Assert.AreEqual("54", SliceHudFormat.Compact(54f));
            Assert.AreEqual("-5", SliceHudFormat.Compact(-5f));
        }

        [Test]
        public void Compact_LargeLife_FitsOrbTextBox()
        {
            // 生命球文本区 112px（128−16）；12pt bold 数字约 9px/字符 → 上限 9 字符
            Assert.AreEqual("10.0M", SliceHudFormat.Compact(10000040f));
            Assert.AreEqual("10.0M", SliceHudFormat.Compact(10000040f), "调试点：10000040/10000040 必须显示 10.0M/10.0M");
            foreach (float v in new[] { 123456f, 999999f, 10000040f, 123456789f, 99999999999f })
                Assert.LessOrEqual(SliceHudFormat.Compact(v).Length, 9, "球内文本不得溢出 112px 文本区");
        }

        // ---------- L4 战斗底栏布局（单一来源 CombatBarRects） ----------

        static readonly Vector2[] DesignSpaces =
        {
            new Vector2(1920f, 1080f),  // 2560×1440 与 1920×1080 的共同 no-clamp 设计空间
            new Vector2(2743.2f, 1542.6f), // 4K 缓冲 3840×2160 触发 1.4 上钳的设计空间
            new Vector2(2580f, 1080f),  // 超宽 3440×1440 的设计空间
            new Vector2(1920f, 1600f)   // 窄高窗口（DesignScale 下界=1920 宽的设计空间）
        };

        [Test]
        public void CombatBar_ElementsNeverOverlap_AndStayInViewport([ValueSource(nameof(DesignSpaces))] Vector2 space)
        {
            var L = SliceHud.CombatBarRects(space.x, space.y);
            var bar = L.Bar;
            var frame = L.Frame;
            // S5U-F1 V-01：统一外框出界约束
            Assert.GreaterOrEqual(frame.x, 2f, "外框不得贴出左缘");
            Assert.LessOrEqual(frame.xMax, space.x - SliceDrawerLayout.PanelW + 0.01f, "外框不得进入背包面板区域");
            Assert.LessOrEqual(frame.yMax, space.y + 0.01f, "外框不得溢出下缘");
            Assert.GreaterOrEqual(frame.y, 0f);
            Assert.GreaterOrEqual(bar.x, 12f, "底栏不得贴出左缘");
            // 2026-09-10：背包面板常驻贴右，底栏在面板左侧的剩余宽度内居中，不得被面板压住
            Assert.LessOrEqual(bar.xMax, space.x - SliceDrawerLayout.PanelW + 0.01f, "底栏不得进入背包面板区域");
            Assert.LessOrEqual(bar.yMax, space.y, "底栏不得溢出下缘");
            Assert.GreaterOrEqual(bar.y, 0f);
            // 外框包络全部元素（assembly 一体；2026-09-10：tray 已迁出底栏，改为 5 主动技能槽 + 4 药剂槽）
            for (int i = 0; i < SliceHud.SkillSlots; i++)
                Assert.IsTrue(Encloses(frame, L.SkillSlot(i)), "外框必须包络技能槽 " + SliceHud.SlotHotkeys[i]);
            for (int i = 0; i < SliceHud.FlaskSlots; i++)
                Assert.IsTrue(Encloses(frame, L.FlaskSlot(i)), "外框必须包络药剂槽 " + SliceHud.FlaskHotkeys[i]);
            Assert.IsTrue(Encloses(frame, L.LifeOrb) && Encloses(frame, L.ManaOrb),
                "V-01：外框必须包络双球");

            // 水平排序（Life | Q | W | E | R | T | 药剂 1-4 | Mana）+ 最小间隙
            Assert.LessOrEqual(L.LifeOrb.xMax + 10f, L.SlotQ.x, "LIFE 球与 Q 槽不得重叠");
            for (int i = 1; i < SliceHud.SkillSlots; i++)
                Assert.LessOrEqual(L.SkillSlot(i - 1).xMax + 4f, L.SkillSlot(i).x,
                    "技能槽 " + SliceHud.SlotHotkeys[i - 1] + "/" + SliceHud.SlotHotkeys[i] + " 不得重叠");
            Assert.LessOrEqual(L.SlotT.xMax + 10f, L.Flask1.x, "T 槽与药剂槽不得重叠");
            for (int i = 1; i < SliceHud.FlaskSlots; i++)
                Assert.LessOrEqual(L.FlaskSlot(i - 1).xMax + 4f, L.FlaskSlot(i).x,
                    "药剂槽 " + i + "/" + (i + 1) + " 不得重叠");
            Assert.LessOrEqual(L.FlaskSlot(SliceHud.FlaskSlots - 1).xMax + 10f, L.ManaOrb.x, "药剂槽与 MANA 球不得重叠");
        }

        [Test]
        public void CombatBar_Hotkeys_MatchPoelikeSlotRow()
        {
            Assert.AreEqual(new[] { "Q", "W", "E", "R", "T" }, SliceHud.SlotHotkeys, "主动技能槽必须为 Q/W/E/R/T 五槽");
            Assert.AreEqual(5, SliceHud.SkillSlots);
            Assert.AreEqual(4, SliceHud.FlaskSlots, "为后续药剂预留 4 个槽位");
            Assert.AreEqual(2, SliceSession.SkillHotkey(SkillId.Area) == "E" ? 2 : -1, "既有 E 绑定不得改变");
        }

        [Test]
        public void CombatBar_CarriesNoSupportTray()
        {
            // 导演指令：底栏只保留主动技能——辅助宝石托盘已迁至背包面板，底栏不再有 tray 字段
            var fields = typeof(SliceHud.CombatBarLayout).GetFields();
            foreach (var f in fields)
                Assert.AreNotEqual("Tray", f.Name, "底栏不得再持有辅助宝石托盘");
        }

        static bool Encloses(Rect outer, Rect inner)
        {
            return inner.x >= outer.x - 0.01f && inner.y >= outer.y - 0.01f &&
                   inner.xMax <= outer.xMax + 0.01f && inner.yMax <= outer.yMax + 0.01f;
        }

        [Test]
        public void CombatBar_ElementSizes_FixedContract()
        {
            var L = SliceHud.CombatBarRects(1920f, 1080f);
            Assert.AreEqual(128f, L.LifeOrb.width, "生命球 128²");
            Assert.AreEqual(128f, L.LifeOrb.height, "生命球 128²");
            Assert.AreEqual(128f, L.ManaOrb.width, "法力球 128²");
            Assert.AreEqual(92f, L.SlotQ.width, "技能槽 92²");
            Assert.AreEqual(92f, L.SlotE.height, "技能槽 92²");
            Assert.AreEqual(92f, L.SlotR.width, "R 槽 92²");
            Assert.AreEqual(92f, L.SlotT.width, "T 槽 92²");
            Assert.AreEqual(new Vector2(54f, 78f), new Vector2(L.Flask1.width, L.Flask1.height), "预留药剂槽 54×78");
            Assert.AreEqual(0f, L.FlaskRow.height - L.Flask1.height, "药剂槽行高=单槽高");
            // S5U-F1 V-01：外框=total+20 × barH+28，包络 Bar
            Assert.AreEqual(new Vector2(L.Bar.width + 20f, L.Bar.height + 28f), new Vector2(L.Frame.width, L.Frame.height),
                "外框 = 内栏 + 20 × +28");
            Assert.IsTrue(Encloses(L.Frame, L.Bar), "外框包络内栏");
            // 2560×1440 与 1920×1080 同设计空间 → 布局逐字节一致（no-clamp 通道）
            var L2 = SliceHud.CombatBarRects(1920f, 1080f);
            Assert.AreEqual(L.Bar, L2.Bar);
            Assert.AreEqual(L.Frame, L2.Frame);
            Assert.AreEqual(L.SlotW, L2.SlotW);
        }

        // ---------- 连接徽章/容量真值映射（S5 合同承载点） ----------

        static SliceSession NewSession(out ArenaSim sim)
        {
            sim = new ArenaSim();
            sim.Reset();
            var s = new SliceSession();
            sim.Session = s;
            s.ResetTown(20260909u);
            sim.Caster.Defs = s.ResolveSkillDef;
            return s;
        }

        static string GroupOf(string label)
        {
            var parts = label.Split('·');
            return parts[parts.Length - 2].Replace("组", "");
        }

        static string CapOf(string label)
        {
            var parts = label.Split('·');
            return parts[parts.Length - 1].Replace("容", "");
        }

        [Test]
        public void LinkBadge_MatchesLinkSourceLabel_OnEverySkill()
        {
            ArenaSim sim;
            var s = NewSession(out sim);
            var rng = new SeededRng(4242u);
            string err;
            int iw = s.AddItem(s.RollItem(EquipSlot.Weapon, Rarity.Rare, rng, 4, "裂石巨刃"));
            Assert.IsTrue(s.TryEquip(iw, out err), "装备测试武器失败: " + err);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), "重绑弹道连接失败: " + err);

            foreach (SkillId skill in new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area })
            {
                string label = s.LinkSourceLabel(skill);
                string expected = "G" + GroupOf(label) + "·容" + CapOf(label);
                Assert.AreEqual(expected, s.LinkBadgeText(skill),
                    skill + " 徽章必须与连接源标签同源（组/容量逐字符一致）");
            }
            // 确定性种子锁定：二段连接真值（裂石巨刃·组1）
            StringAssert.Contains("裂石巨刃·组1", s.LinkSourceLabel(SkillId.Projectile));
            StringAssert.Contains("组0", s.LinkSourceLabel(SkillId.Melee));
        }

        [Test]
        public void SupportCapacity_MatchesLinkSourceCapacity()
        {
            ArenaSim sim;
            var s = NewSession(out sim);
            var rng = new SeededRng(4242u);
            string err;
            int iw = s.AddItem(s.RollItem(EquipSlot.Weapon, Rarity.Rare, rng, 4, "裂石巨刃"));
            s.TryEquip(iw, out err);
            s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err);

            foreach (SkillId skill in new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area })
            {
                int cap = int.Parse(CapOf(s.LinkSourceLabel(skill)));
                Assert.AreEqual(cap, s.SupportCapacity(skill), skill + " 容量必须取连接源组的容量（禁槽位推导）");
            }
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "确定性种子下 弹道=容1");
        }

        // ---------- L2/L3 图标缓存与禁 Socket Color ----------

        [Test]
        public void Icons_Ensure_IsIdempotent_SingleSynthesis()
        {
            Texture2D globe = SliceHudIcons.GlobeFrame;
            Texture2D slot = SliceHudIcons.SlotFrame;
            Texture2D sep = SliceHudIcons.Separator;
            Texture2D sword = SliceHudIcons.GlyphMelee;
            Texture2D arrow = SliceHudIcons.GlyphProjectile;
            Texture2D nova = SliceHudIcons.GlyphArea;
            SliceHudIcons.Ensure();
            Assert.AreSame(globe, SliceHudIcons.GlobeFrame, "球环必须合成一次静态缓存");
            Assert.AreSame(slot, SliceHudIcons.SlotFrame, "槽框必须合成一次静态缓存");
            Assert.AreSame(sep, SliceHudIcons.Separator, "饰线必须合成一次静态缓存");
            Assert.AreSame(sword, SliceHudIcons.GlyphMelee, "近战符文必须静态缓存");
            Assert.AreSame(arrow, SliceHudIcons.GlyphProjectile, "弹道符文必须静态缓存");
            Assert.AreSame(nova, SliceHudIcons.GlyphArea, "范围符文必须静态缓存");
            Assert.AreEqual(256, globe.width, "球环 256²");
            Assert.AreEqual(128, slot.width, "槽框 128²");
            Assert.AreEqual(64, sword.width, "符文 64²");
            Assert.AreEqual(6, sep.height, "饰线 256×6");
        }

        [Test]
        public void PipFor_IdentityMapping_NoSocketColorSemantics()
        {
            Assert.AreNotSame(SliceHudIcons.PipOn, SliceHudIcons.PipOff, "空/填充必须是可区分的两态");
            Assert.AreNotSame(SliceHudIcons.PipOn, SliceHudIcons.PipClosed, "填充/封闭必须是可区分的两态");
            Assert.AreNotSame(SliceHudIcons.PipOff, SliceHudIcons.PipClosed, "空/封闭必须是可区分的两态");
            Assert.AreEqual(20, SliceHudIcons.PipOn.width, "pip 20²");

            // 恒等映射：任何 SupportId 在同 (filled,closed) 下必须返回同一贴图（禁按辅助类型着色=Socket Color 语义回归）
            for (int id = 1; id <= SupportCatalog.Count; id++)
            {
                var sid = (SupportId)id;
                Assert.AreSame(SliceHudIcons.PipOn, SliceHudIcons.PipFor(sid, true, false), sid + " 填充态必须恒等（中性金属）");
                Assert.AreSame(SliceHudIcons.PipOff, SliceHudIcons.PipFor(sid, false, false), sid + " 空态必须恒等");
                Assert.AreSame(SliceHudIcons.PipClosed, SliceHudIcons.PipFor(sid, false, true), sid + " 封闭态必须恒等");
                Assert.AreSame(SliceHudIcons.PipClosed, SliceHudIcons.PipFor(sid, true, true), "封闭优先于填充（容量外孔不可用）");
            }
        }

        [Test]
        public void GlyphFor_CoversAllCastableSkills()
        {
            Assert.AreSame(SliceHudIcons.GlyphMelee, SliceHudIcons.GlyphFor(SkillId.Melee));
            Assert.AreSame(SliceHudIcons.GlyphProjectile, SliceHudIcons.GlyphFor(SkillId.Projectile));
            Assert.AreSame(SliceHudIcons.GlyphArea, SliceHudIcons.GlyphFor(SkillId.Area));
        }
    }
}
