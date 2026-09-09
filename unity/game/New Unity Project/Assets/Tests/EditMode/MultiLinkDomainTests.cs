using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5-WO-02（BL-021.A2，合同 docs/reviews/S5/S5_LINK_CONTRACT.md）Multi-Link 域核契约：
    /// legacy 单组 parity / 2 组推导与确定性划分 / 总容量取舍（有意设计）/ 资格不变 /
    /// 全局唯一连接源（自改挂/双物品改挂/腐败态 fail-closed）/ 原子容量校验（溢出拒绝、无静默截断）/
    /// 改挂清除与 A→B 变更 / golden 兼容组位无关（21 组合双位置对拍）/ 禁区表面（无颜色/等级品质/第三组字段）。
    /// 随机零使用（纯域逻辑）；UI 不在本单范围。
    /// </summary>
    public sealed class MultiLinkDomainTests
    {
        static readonly SupportId[] AllSupports =
        {
            SupportId.AddedFire, SupportId.Brutal, SupportId.Concentrated,
            SupportId.Faster, SupportId.Combustion, SupportId.Fork, SupportId.FireConversion
        };

        static int WeaponIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Weapon]; }
        static int BodyIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Body]; }
        static int HelmetIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Helmet]; }

        // ---------- legacy parity（场景 1/9） ----------

        [Test]
        public void LegacyParity_DefaultNoRebind_CapacitiesAndBehaviorUnchanged()
        {
            var s = new SliceSession();
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1, "既有物品默认=单连接 legacy");
            Assert.AreEqual(SkillId.None, s.Inventory[BodyIdx(s)].LinkSkill1, "既有物品默认=单连接 legacy");

            // 映射槽容量与现状逐位等价（Weapon 3S→2 / Body 3S→2 / Helmet 2S→1）
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee));
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Projectile));
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area));

            // 默认行为回归：兼容 Support 正常装配
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.Faster, out err), err);
            Assert.AreEqual(SupportId.Faster, s.QSupports[0]);
        }

        [Test]
        public void LegacyParity_DisplaySockets_GlovesBelt_NotLinkEligible()
        {
            var s = new SliceSession();
            // 新手套/腰带初始未装备（Equipped=-1）——先制造并装备，验证其 1 孔仅展示、不映射技能
            string err;
            ItemInstance gloves = s.RollItem(EquipSlot.Gloves, Rarity.Ordinary, new SeededRng(7u),
                SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
            int gIdx = s.AddItem(gloves);
            Assert.IsTrue(s.TryEquip(gIdx, out err), err);
            ItemInstance belt = s.RollItem(EquipSlot.Belt, Rarity.Ordinary, new SeededRng(9u),
                SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
            int bIdx = s.AddItem(belt);
            Assert.IsTrue(s.TryEquip(bIdx, out err), err);

            Assert.IsFalse(s.TryReassignLink(gIdx, SkillId.Melee, out err), err);
            Assert.IsTrue(err.Contains("孔数不足"), err);
            Assert.IsFalse(s.TryReassignLink(bIdx, SkillId.Area, out err), err);
            Assert.IsTrue(err.Contains("孔数不足"), err);
            // Boots 0 孔同拒（初始已装备）
            Assert.IsFalse(s.TryReassignLink(s.Equipped[(int)EquipSlot.Boots], SkillId.Melee, out err), err);
        }

        // ---------- 2 组推导与总容量取舍（场景 2/10） ----------

        [Test]
        public void TwoGroupDerivation_PartitionAndCapacities()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);

            Assert.AreEqual(SkillId.Projectile, s.Inventory[WeaponIdx(s)].LinkSkill1);
            // 改挂技能唯一源=hosting group 1（容量恒 1）；host 映射技能 group 0=前部 1 孔（容量 0）；Area 不受影响
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile));
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee));
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area));

            // group 1 单 Support 位：index 0 可用、index 1 超 cap 拒绝
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.IsFalse(s.TrySetSupport(SkillId.Projectile, 1, SupportId.Faster, out err), err);
            Assert.IsTrue(err.Contains("孔不足"), err);
        }

        [Test]
        public void TotalCapacityTradeoff_ThreeSocket_Legacy2VsSplit0And1()
        {
            var s = new SliceSession();
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee), "legacy：3S 武器=2 Support 位");
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee), "拆分后 group 0=1 孔→0 Support（有意取舍，非回归）");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "拆分后 group 1=2 孔→1 Support（有意取舍，非回归）");
        }

        // ---------- 自改挂 / 全局唯一连接源（场景 7 + 唯一源测试） ----------

        [Test]
        public void SelfReassign_Rejected_StateUnchanged()
        {
            var s = new SliceSession();
            string err;
            Assert.IsFalse(s.TryReassignLink(WeaponIdx(s), SkillId.Melee, out err), err);
            Assert.IsTrue(err.Contains("已自带"), err);
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1, "拒绝后零半写入");
        }

        [Test]
        public void DuplicateRebind_SecondHost_Rejected()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            Assert.IsFalse(s.TryReassignLink(BodyIdx(s), SkillId.Area, out err), err);
            Assert.IsTrue(err.Contains("已被其它装备改挂"), err);
            Assert.AreEqual(SkillId.None, s.Inventory[BodyIdx(s)].LinkSkill1, "拒绝后零半写入");
        }

        [Test]
        public void Rebind_SuppressesDefaultSource_Clear_Restores()
        {
            var s = new SliceSession();
            string err;
            // 改挂前：Projectile 默认源=Body（cap 2）
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Projectile));
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "默认源被抑制，唯一源=Weapon group 1");
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Faster, out err), err);
            // 清除：默认源恢复，Support 列表跟随技能身份保留
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.None, out err), err);
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1);
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Projectile), "默认源恢复");
            Assert.AreEqual(SupportId.Faster, s.WSupports[0], "Support 列表跟随技能身份，不被迁移/丢弃");
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee), "host group 0 恢复完整孔集");
        }

        [Test]
        public void ChangeRebind_AtoB_SingleTransaction()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            // A 恢复默认、B 接管 group 1（无双源中间态）
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "A 回退默认源");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "B 唯一源=group 1");
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee));
            Assert.AreEqual(SkillId.Projectile, s.Inventory[WeaponIdx(s)].LinkSkill1);
        }

        [Test]
        public void CorruptDuplicateState_FailClosed_FallsBackToLegacy_NoFirstWins()
        {
            var s = new SliceSession();
            // 直接构造腐败态（绕过 API）：两件物品同挂 Area —— 读路径必须 fail-closed（忽略改挂、回退 legacy），禁止 first-wins
            ItemInstance w = s.Inventory[WeaponIdx(s)];
            w.LinkSkill1 = SkillId.Area;
            s.Inventory[WeaponIdx(s)] = w;
            ItemInstance h = s.Inventory[HelmetIdx(s)];
            h.LinkSkill1 = SkillId.Area;
            s.Inventory[HelmetIdx(s)] = h;
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "腐败态=忽略改挂回退映射槽 legacy（Helmet 2S→1），不按装备顺序择一");
        }

        // ---------- 原子容量校验（场景：溢出拒绝、零半写入） ----------

        [Test]
        public void AtomicCapacity_HostGroup0Overflow_Rejected()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.Faster, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 1, SupportId.Combustion, out err), err);
            Assert.IsFalse(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.IsTrue(err.Contains("超出拆分后容量"), err);
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1, "拒绝后零半写入");
            Assert.AreEqual(SupportId.Faster, s.QSupports[0], "既有 Support 不被静默截断/迁移");
            Assert.AreEqual(SupportId.Combustion, s.QSupports[1]);
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee), "容量保持 legacy");
        }

        [Test]
        public void AtomicCapacity_IncomingSkillOverflow_Rejected()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Faster, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 1, SupportId.Brutal, out err), err);
            Assert.IsFalse(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.IsTrue(err.Contains("超出第二组容量"), err);
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1, "拒绝后零半写入");
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Projectile), "容量保持 legacy");
        }

        // ---------- golden 兼容组位无关（场景 4/5/6） ----------

        [Test]
        public void GoldenCompatibility_PositionIndependent_Group0_And_Group1()
        {
            for (int si = 0; si < 3; si++)
            {
                var skill = (SkillId)(si + 1); // Melee/Projectile/Area
                for (int i = 0; i < AllSupports.Length; i++)
                {
                    var support = AllSupports[i];
                    bool expected = SliceSession.IsSupportCompatible(support, skill);

                    // group 0 位（映射槽 0 号 Support 位）
                    var s0 = new SliceSession();
                    string err;
                    bool ok0 = s0.TrySetSupport(skill, 0, support, out err);
                    Assert.AreEqual(expected, ok0,
                        "group 0 位不一致：" + skill + "×" + support + " expected=" + expected + " got=" + ok0 + " err=" + err);

                    // group 1 位（改挂到 3S host 后 0 号 Support 位）
                    var s1 = new SliceSession();
                    int hostIdx = skill == SkillId.Melee ? BodyIdx(s1) : WeaponIdx(s1); // Melee 改挂 Body，Projectile/Area 改挂 Weapon
                    Assert.IsTrue(s1.TryReassignLink(hostIdx, skill, out err), err);
                    bool ok1 = s1.TrySetSupport(skill, 0, support, out err);
                    Assert.AreEqual(expected, ok1,
                        "group 1 位不一致：" + skill + "×" + support + " expected=" + expected + " got=" + ok1 + " err=" + err);
                }
            }
        }

        // ---------- 禁区表面（场景 11/12 + 第三组） ----------

        [Test]
        public void ForbiddenSurface_NoColorLevelQualityOrThirdGroupFields()
        {
            var fields = typeof(ItemInstance).GetFields();
            foreach (var f in fields)
            {
                string n = f.Name;
                Assert.IsFalse(n.Contains("Color"), "禁止孔颜色字段：" + n);
                Assert.IsFalse(n.Contains("Level") || n.Contains("Quality"), "禁止宝石等级/品质字段：" + n);
                Assert.IsFalse(n.Contains("LinkSkill2") || n.Contains("LinkGroup"), "禁止第三连接组表示：" + n);
            }
            bool hasLinkSkill1 = System.Array.Exists(fields, f => f.Name == "LinkSkill1");
            Assert.IsTrue(hasLinkSkill1, "有界表示必须恰含 LinkSkill1 字段");
        }

        [Test]
        public void Eligibility_Helmet_TwoSockets_CannotHostSecondGroup()
        {
            var s = new SliceSession();
            string err;
            Assert.IsFalse(s.TryReassignLink(HelmetIdx(s), SkillId.Melee, out err), err);
            Assert.IsTrue(err.Contains("孔数不足"), err);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Helmet legacy 容量不变");
        }
    }
}
