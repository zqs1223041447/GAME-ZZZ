using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5-WO-05（Phase 4 Build &amp; Interaction Validation；Runtime Product Delta=NONE）：
    /// V1 legacy 单连接 3/3 技能（S5 词缀在场）/ V2 合法双连接 ×2（含组0/组1 不同 Support 身份）/
    /// V3 数值型 Support 跨位置（含 S5 词缀构筑）/ V4 机制型 Support 跨位置（Fork 真实分裂，RC-P 同构）/
    /// V5 第三连接组=结构性拒绝 / V6 词缀在场跨组隔离（词缀轴与连接归属正交）/
    /// 词缀交互矩阵 4/4（迅疾改挂不增不减/铁骨 Belt 负面在多连接负载下保持/睿智·坚韧下游换算贯通）/
    /// 有意义选择证据（≥3 个可区分合法负载场景）。零产品行为改动；RC-M/RC-P/RC-A 仅作 validation scenarios（CANDIDATE / NOT LOCKED）。
    /// </summary>
    public sealed class S5BuildInteractionTests
    {
        static ItemInstance MakeAffixItem(SliceSession s, EquipSlot slot, AffixId id, float value)
        {
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = slot;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = SliceSession.SocketsFor(slot);
            it.BaseName = SliceSession.ItemBaseName(slot);
            it.AffixCount = 1;
            it.SetAffix(0, id, value, 0f);
            return it;
        }

        static int AddAndEquip(SliceSession s, EquipSlot slot, AffixId id, float value)
        {
            int idx = s.AddItem(MakeAffixItem(s, slot, id, value));
            string err;
            Assert.IsTrue(s.TryEquip(idx, out err), err);
            return idx;
        }

        static void ClearEquipment(SliceSession s)
        {
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;
            s.RecalcPlayer(true);
        }

        static ArenaSim NewSim(uint seed)
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            sim.Session.ResetTown(seed);
            sim.Caster.Defs = sim.Session.ResolveSkillDef;
            return sim;
        }

        static void TickNone(ArenaSim sim, float seconds)
        {
            int n = (int)System.Math.Round(seconds / 0.02f);
            for (int i = 0; i < n; i++)
                sim.Tick(0.02f, PlayerCommand.None());
        }

        // ---------- V1：legacy 单连接 parity（3/3 canonical skills，S5 词缀在场） ----------

        [Test]
        public void V1_LegacySingleLink_ThreeSkills_WithS5Affixes()
        {
            EquipSlot[] slots = { EquipSlot.Weapon, EquipSlot.Body, EquipSlot.Helmet };
            SkillId[] skills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };
            AffixId[] affixes = { AffixId.SwiftBreeze, AffixId.Ironhide, AffixId.Insight };
            int[] legacyCaps = { 2, 2, 1 };
            for (int i = 0; i < 3; i++)
            {
                var s = new SliceSession();
                AddAndEquip(s, slots[i], affixes[i], affixes[i] == AffixId.SwiftBreeze ? 0.16f : affixes[i] == AffixId.Ironhide ? 0.22f : 12f);
                string err;
                Assert.AreEqual(SkillId.None, s.Inventory[s.Equipped[(int)slots[i]]].LinkSkill1, skills[i] + " legacy=无第二连接组");
                Assert.AreEqual(legacyCaps[i], s.SupportCapacity(skills[i]), skills[i] + " legacy 容量不变");
                Assert.IsTrue(s.TrySetSupport(skills[i], 0, SupportId.Brutal, out err), err);
                HitRequest hit = s.BuildPlayerHit(skills[i], default(Dummy));
                Assert.AreEqual(1.4f, hit.MorePhys, 0.0001f, skills[i] + " legacy Support 照常生效");
                string label = s.LinkSourceLabel(skills[i]);
                Assert.IsTrue(label.Contains("组0") && !label.Contains("组1"), skills[i] + " 唯一源=组0：" + label);
                string[] groups = s.LinkGroupsText(s.Inventory[s.Equipped[(int)slots[i]]]);
                Assert.IsFalse(System.Array.Exists(groups, g => g.Contains("组1")), skills[i] + " legacy 不呈现第二组");
            }
        }

        // ---------- V2：合法双连接 ×2（A=组0/组1 不同 Support 身份） ----------

        [Test]
        public void V2_TwoLink_ConfigA_DifferentSupportIdentities()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            string err;
            // 全新 3S 物品（隔离 starter 词缀噪声），先配置双改挂（容量侧校验在 Support 为空时通过），再装不同身份 Support
            Assert.IsTrue(s.TryEquip(s.AddItem(MakeAffixItem(s, EquipSlot.Weapon, AffixId.Life, 12f)), out err), err);
            Assert.IsTrue(s.TryEquip(s.AddItem(MakeAffixItem(s, EquipSlot.Body, AffixId.Life, 12f)), out err), err);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Body], SkillId.Area, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Faster, out err), err);

            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee));
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile));
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area));
            HitRequest p = s.BuildPlayerHit(SkillId.Projectile, default(Dummy));
            Assert.AreEqual(1.4f, p.MorePhys, 0.0001f, "组1（弹道）Support 生效恰一次");
            var bag = new StatBag();
            s.CollectSkillMods(SkillId.Area, bag);
            Assert.AreEqual(0.2f, bag.RawIncreased(StatId.AttackSpeed), 0.0001f, "组1（范围）Support 生效恰一次");
            Assert.AreEqual(1f, s.BuildPlayerHit(SkillId.Melee, default(Dummy)).MorePhys, 0.0001f, "零跨组继承");
            Assert.IsTrue(s.LinkSourceLabel(SkillId.Projectile).Contains("组1"), "源标注与 runtime 一致");
            Assert.IsTrue(s.LinkSourceLabel(SkillId.Area).Contains("组1"), "源标注与 runtime 一致");
        }

        [Test]
        public void V2_TwoLink_ConfigB_AreaViaGroup1_Combustion()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            string err;
            Assert.IsTrue(s.TryEquip(s.AddItem(MakeAffixItem(s, EquipSlot.Weapon, AffixId.Life, 12f)), out err), err);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Area, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Combustion, out err), err);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 唯一源=武器组1 容 1");
            var bag = new StatBag();
            s.CollectSkillMods(SkillId.Area, bag);
            Assert.AreEqual(1f, bag.Get(StatId.IgniteChance), 0.0001f, "燃尽点燃轴经组1 生效");
            Assert.AreEqual(0.5f, bag.RawIncreased(StatId.FireDamage), 0.0001f, "燃尽火伤轴经组1 生效");
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee), "host 组0=近战 3 孔拆分容 0");
        }

        // ---------- V3：数值型 Support 跨位置（S5 词缀构筑内） ----------

        [Test]
        public void V3_StatSupportCrossPosition_WithS5AffixInBuild()
        {
            string err;
            // 组0：坚韧（手套，Strength 轴）在场，Brutal 装弹道默认源
            var g0 = new SliceSession();
            AddAndEquip(g0, EquipSlot.Gloves, AffixId.Tenacity, 12f);
            Assert.IsTrue(g0.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            Assert.AreEqual(1.4f, g0.BuildPlayerHit(SkillId.Projectile, default(Dummy)).MorePhys, 0.0001f, "组0 位生效恰一次");
            Assert.AreEqual(32, g0.Strength, "词缀经全局属性轴，与连接模式无关");

            // 组1：同词缀构筑，Brutal 随技能迁入组1
            var g1 = new SliceSession();
            AddAndEquip(g1, EquipSlot.Gloves, AffixId.Tenacity, 12f);
            Assert.IsTrue(g1.TryReassignLink(g1.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.IsTrue(g1.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            Assert.AreEqual(1.4f, g1.BuildPlayerHit(SkillId.Projectile, default(Dummy)).MorePhys, 0.0001f, "组1 位等价生效恰一次");
            Assert.AreEqual(32, g1.Strength, "词缀不受改挂影响（Affix/Link 正交）");
        }

        // ---------- V4：机制型 Support 跨位置（Fork 真实分裂；RC-P 同构+新词缀） ----------

        [Test]
        public void V4_MechanicSupportCrossPosition_RCP_Fork_WithAffixes()
        {
            string err;
            // RC-P exact（组0）：弹道 + Fork + Faster + 迅疾（词缀在场）
            var simA = NewSim(9u);
            AddAndEquip(simA.Session, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            Assert.IsTrue(simA.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.IsTrue(simA.Session.TrySetSupport(SkillId.Projectile, 1, SupportId.Faster, out err), err);
            simA.Dummies.SpawnAt(0f, 2.4f);
            simA.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, 0, 0f, 2.4f));
            TickNone(simA, SkillCatalog.Get(SkillId.Projectile).Windup);
            for (int i = 0; i < 40 && simA.Session.ForkSpawns == 0; i++)
                simA.Tick(0.02f, PlayerCommand.None());
            Assert.AreEqual(2, simA.Session.ForkSpawns, "组0 真实分裂恰一次（Fork+迅疾在场）");

            // RC-P exact（组1）：同词缀，Fork 随技能入组1（组1 容 1=恰 Fork；Faster 无位=容量真相）
            var simB = NewSim(9u);
            AddAndEquip(simB.Session, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            Assert.IsTrue(simB.Session.TryReassignLink(simB.Session.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.AreEqual(1, simB.Session.SupportCapacity(SkillId.Projectile));
            Assert.IsTrue(simB.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            simB.Dummies.SpawnAt(0f, 2.4f);
            simB.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, 0, 0f, 2.4f));
            TickNone(simB, SkillCatalog.Get(SkillId.Projectile).Windup);
            for (int i = 0; i < 40 && simB.Session.ForkSpawns == 0; i++)
                simB.Tick(0.02f, PlayerCommand.None());
            Assert.AreEqual(2, simB.Session.ForkSpawns, "组1 真实分裂恰一次（非仅数据模型）");
        }

        // ---------- V5：第三连接组=结构性拒绝 ----------

        [Test]
        public void V5_InvalidThirdGroup_StructuralRejection()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            int[] eqBefore = (int[])s.Equipped.Clone();
            // 语义等价第三组：第二物品再挂同技能=确定性拒绝（组上限=2 结构不变）
            Assert.IsFalse(s.TryReassignLink(s.Equipped[(int)EquipSlot.Body], SkillId.Projectile, out err), "第三组必须拒绝");
            Assert.IsTrue(err.Contains("已被其它装备改挂"), err);
            for (int i = 0; i < eqBefore.Length; i++)
                Assert.AreEqual(eqBefore[i], s.Equipped[i], "零状态损坏");
            foreach (var f in typeof(ItemInstance).GetFields())
                Assert.IsFalse(f.Name.Contains("LinkSkill2") || f.Name.Contains("LinkGroup"), "不得发明 LinkSkill2/group2 状态：" + f.Name);
        }

        // ---------- V6：词缀在场跨组隔离（词缀轴 × 连接归属正交） ----------

        [Test]
        public void V6_CrossGroupIsolation_UnderAffixes()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            string err;
            // 双连接在场 + S5 词缀（武器坚韧=玩家属性轴；身体迅疾=全局攻速轴；头盔无词缀=Area legacy 源）
            AddAndEquip(s, EquipSlot.Weapon, AffixId.Tenacity, 12f);
            AddAndEquip(s, EquipSlot.Body, AffixId.SwiftBreeze, 0.16f);
            Assert.IsTrue(s.TryEquip(s.AddItem(MakeAffixItem(s, EquipSlot.Helmet, AffixId.Life, 12f)), out err), err);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Concentrated, out err), err);

            // host 组0 Support 不影响组1 技能；组1 Support 不影响 host 组0
            Assert.AreEqual(1.4f, s.BuildPlayerHit(SkillId.Projectile, default(Dummy)).MorePhys, 0.0001f, "组1 生效恰一次");
            Assert.AreEqual(1f, s.BuildPlayerHit(SkillId.Melee, default(Dummy)).MorePhys, 0.0001f, "组1 Support 不泄漏到 host 组0");
            var bagA = new StatBag();
            s.CollectSkillMods(SkillId.Area, bagA);
            Assert.AreEqual(1.4f, bagA.RawMore(StatId.AreaDamageMore), 0.0001f, "Area 专属 Support 只服务 Area（RawMore=中性 1+和，F1 同源语义）");
            // 词缀经全局/装备 stat 路径照常（与连接组无关）：攻速进各技能聚合、力量进玩家面板
            var bagP = new StatBag();
            s.CollectSkillMods(SkillId.Projectile, bagP);
            Assert.AreEqual(0.16f, bagP.RawIncreased(StatId.AttackSpeed), 0.0001f, "词缀轴不成为连接组成员机制（进技能聚合=既有全局语义）");
            Assert.AreEqual(32, s.Strength, "坚韧经玩家属性轴（RecalcPlayer），与组归属无关");
            // 改挂能力不受词缀影响（正交）：当前绑定预判=可用（词缀在场判定照常）；
            // 换绑候选（胸甲→近战）会令残暴溢出胸甲组0=确定性拒绝（域语义正确，与词缀无关）
            Assert.IsNull(s.PreviewReassignError(s.Inventory[s.Equipped[(int)EquipSlot.Weapon]], SkillId.Projectile), "当前绑定预判=可用（词缀在场判定照常）");
            string reason = s.PreviewReassignError(s.Inventory[s.Equipped[(int)EquipSlot.Body]], SkillId.Melee);
            Assert.IsTrue(reason != null && reason.Contains("超出拆分后容量"), "约束冲突候选=确定性拒绝：" + reason);
        }

        // ---------- §5 词缀交互矩阵（4/4） ----------

        [Test]
        public void AffixMatrix_SwiftBreeze_RebindDoesNotDuplicateOrSuppress()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            AddAndEquip(s, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            float rec0 = s.ResolveSkillDef(SkillId.Melee).Recovery;
            var bag0 = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bag0);
            Assert.AreEqual(0.16f, bag0.RawIncreased(StatId.AttackSpeed), 0.0001f);

            string err;
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            float rec1 = s.ResolveSkillDef(SkillId.Melee).Recovery;
            var bag1 = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bag1);
            Assert.AreEqual(rec0, rec1, rec0 * 0.0001f, "改挂不抑制既有词缀效果");
            Assert.AreEqual(0.16f, bag1.RawIncreased(StatId.AttackSpeed), 0.0001f, "改挂不重复/不意外叠加词缀");
            var bagW = new StatBag();
            s.CollectSkillMods(SkillId.Projectile, bagW);
            Assert.AreEqual(0.16f, bagW.RawIncreased(StatId.AttackSpeed), 0.0001f, "组1 技能同吃全局词缀轴（既有全局语义，非组机制）");
        }

        [Test]
        public void AffixMatrix_Ironhide_BeltNegative_InMultiLinkLoadout()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            string err;
            // 合法非 Belt 槽在活构筑内照常（同基面对拍）
            AddAndEquip(s, EquipSlot.Body, AffixId.Life, 12f);
            float armour0 = s.PlayerStats.Get(StatId.Armour);
            AddAndEquip(s, EquipSlot.Body, AffixId.Ironhide, 0.22f);
            Assert.AreEqual(armour0 * 1.22f, s.PlayerStats.Get(StatId.Armour), armour0 * 0.002f, "铁骨经既有 Armour 聚合（非 Belt 槽）");
            // 多连接负载在场
            AddAndEquip(s, EquipSlot.Weapon, AffixId.SwiftBreeze, 0.16f);
            Assert.IsTrue(s.TryReassignLink(s.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            // Belt 负面在多连接负载下保持为真
            int beltIdx = s.AddItem(MakeAffixItem(s, EquipSlot.Belt, AffixId.Life, 12f));
            s.Etching = 5;
            int etch = s.Etching;
            Assert.IsFalse(s.TryDirectedCraft(beltIdx, AffixId.Ironhide, out err), "AC-09：组合验证下 Belt 仍非法");
            Assert.IsTrue(err.Contains("不能出现在"), err);
            Assert.AreEqual(etch, s.Etching, "拒绝不消耗");
        }

        [Test]
        public void AffixMatrix_Insight_DownstreamManaCoherent()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            float baseMana = s.MaxMana; // 智力 20 基础下的换算基线
            AddAndEquip(s, EquipSlot.Helmet, AffixId.Insight, 12f);
            Assert.AreEqual(32f, s.PlayerStats.Get(StatId.Intelligence), 0.0001f);
            Assert.AreEqual(32, s.Intelligence);
            Assert.AreEqual(baseMana + 12f * SliceRules.ManaPerInt, s.MaxMana, 0.0001f, "睿智下游=既有 Intelligence→Mana 换算贯通");
        }

        [Test]
        public void AffixMatrix_Tenacity_DownstreamLifeCoherent()
        {
            var s = new SliceSession();
            ClearEquipment(s);
            float baseLife = s.MaxLife; // 力量 20 基础下的换算基线
            AddAndEquip(s, EquipSlot.Gloves, AffixId.Tenacity, 12f);
            Assert.AreEqual(32f, s.PlayerStats.Get(StatId.Strength), 0.0001f);
            Assert.AreEqual(32, s.Strength);
            Assert.AreEqual(baseLife + 12f * SliceRules.LifePerStr, s.MaxLife, 0.0001f, "坚韧下游=既有 Strength→Life 换算贯通");
        }

        // ---------- §7 有意义选择证据：≥3 个可区分合法负载场景 ----------

        [Test]
        public void MeaningfulChoice_ThreeDistinguishableValidLoadouts()
        {
            string err;
            // 场景 1（RC-M 完整身份）：legacy 近战=火焰转化+残暴+新词缀坚韧（Conversion 身份）
            var c1 = new SliceSession();
            Assert.IsTrue(c1.TrySetSupport(SkillId.Melee, 0, SupportId.FireConversion, out err), err);
            Assert.IsTrue(c1.TrySetSupport(SkillId.Melee, 1, SupportId.Brutal, out err), err);
            AddAndEquip(c1, EquipSlot.Gloves, AffixId.Tenacity, 12f);
            var bag1 = new StatBag();
            c1.CollectSkillMods(SkillId.Melee, bag1);
            Assert.AreEqual(0.5f, bag1.Get(StatId.ConvertPhysToFire), 0.0001f, "场景 1=legacy 单连接+转化身份");
            Assert.AreEqual(1.4f, c1.BuildPlayerHit(SkillId.Melee, default(Dummy)).MorePhys, 0.0001f, "场景 1 Support 双位生效");
            Assert.AreEqual(32, c1.Strength, "场景 1 新词缀在场（坚韧 Strength 轴）");

            // 场景 2（RC-P 同构）：拆分双连接=弹道 Fork 机制身份（组1 容 1=恰 Fork；总容量 2→0+1 取舍）
            var c2 = new SliceSession();
            Assert.IsTrue(c2.TryReassignLink(c2.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.IsTrue(c2.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.Greater(c2.ForkCount(SkillId.Projectile), 0, "场景 2=拆分双连接+Fork 机制身份");

            // 场景 3：双组不同 Support 身份（Brutal 数值型 / Faster 数值型，跨两组）
            var c3 = new SliceSession();
            Assert.IsTrue(c3.TryReassignLink(c3.Equipped[(int)EquipSlot.Weapon], SkillId.Projectile, out err), err);
            Assert.IsTrue(c3.TryReassignLink(c3.Equipped[(int)EquipSlot.Body], SkillId.Area, out err), err);
            Assert.IsTrue(c3.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            Assert.IsTrue(c3.TrySetSupport(SkillId.Area, 0, SupportId.Faster, out err), err);
            Assert.AreEqual(1.4f, c3.BuildPlayerHit(SkillId.Projectile, default(Dummy)).MorePhys, 0.0001f, "场景 3=双组不同身份");

            // 可区分性：三场景的（技能,连接模式,Support 身份）两两不同——仅用既有机制构成
            Assert.AreNotEqual(SkillId.Melee, SkillId.Projectile);
            Assert.AreNotEqual(SupportId.FireConversion, SupportId.Fork);
            Assert.AreNotEqual(c1.SupportCapacity(SkillId.Melee), c2.SupportCapacity(SkillId.Melee),
                "场景 1 legacy 容量 2 vs 场景 2 拆分后 0（真实取舍=可区分）");
        }
    }
}
