using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S5-WO-03（BL-021.A2 Phase 2，合同 docs/reviews/S5/S5_LINK_CONTRACT.md）多连接运行时与 UI 集成：
    /// B 装备状态不变量（唯一来源守卫=共享后置校验器；装备/替换/换装原子拒绝，字节不变）/
    /// C UI 配置模型（资格呈现=域同源；候选过滤；预判=写入同一内核；失败原因可见）/
    /// D 技能行唯一源呈现 / E Tooltip 连接组划分真实性 / F 运行时 Support 隔离（组0/组1 等价生效恰一次、零跨组泄漏）/
    /// G 负面表面（零孔色/零宝石等级品质/零第三组）。WO-02 域核 260 基线零削弱。
    /// </summary>
    public sealed class MultiLinkIntegrationTests
    {
        const float Dt = 0.02f;

        static int WeaponIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Weapon]; }
        static int BodyIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Body]; }
        static int HelmetIdx(SliceSession s) { return s.Equipped[(int)EquipSlot.Helmet]; }

        /// <summary>构造物品（可带绑定：模拟外部配置态/背包含绑定物品——域 API 对重复赋值本身已拒=第一道防线）。</summary>
        static ItemInstance MakeItem(SliceSession s, EquipSlot slot, SkillId bound)
        {
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = slot;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = SliceSession.SocketsFor(slot);
            it.BaseName = SliceSession.ItemBaseName(slot);
            it.LinkSkill1 = bound;
            return it;
        }

        static ItemInstance Bare(EquipSlot slot, int sockets)
        {
            ItemInstance it = default;
            it.Slot = slot;
            it.SocketCount = sockets;
            return it;
        }

        static void Snapshot(SliceSession s, out int[] eq, out SkillId[] links, out SupportId[] q, out SupportId[] w, out SupportId[] es)
        {
            eq = (int[])s.Equipped.Clone();
            links = new SkillId[s.InventoryCount];
            for (int i = 0; i < s.InventoryCount; i++)
                links[i] = s.Inventory[i].LinkSkill1;
            q = (SupportId[])s.QSupports.Clone();
            w = (SupportId[])s.WSupports.Clone();
            es = (SupportId[])s.ESupports.Clone();
        }

        static void AssertSnapshotUnchanged(SliceSession s, int[] eq, SkillId[] links, SupportId[] q, SupportId[] w, SupportId[] es)
        {
            for (int i = 0; i < eq.Length; i++)
                Assert.AreEqual(eq[i], s.Equipped[i], "装备位字节不变");
            Assert.AreEqual(links.Length, s.InventoryCount, "库存数量不变");
            for (int i = 0; i < links.Length; i++)
                Assert.AreEqual(links[i], s.Inventory[i].LinkSkill1, "LinkSkill1 字节不变（禁静默清除/改写）");
            for (int i = 0; i < q.Length; i++)
                Assert.AreEqual(q[i], s.QSupports[i], "Support 数组不变（禁静默截断/迁移）");
            for (int i = 0; i < w.Length; i++)
                Assert.AreEqual(w[i], s.WSupports[i], "Support 数组不变（禁静默截断/迁移）");
            for (int i = 0; i < es.Length; i++)
                Assert.AreEqual(es[i], s.ESupports[i], "Support 数组不变（禁静默截断/迁移）");
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
            int n = (int)System.Math.Round(seconds / Dt);
            for (int i = 0; i < n; i++)
                sim.Tick(Dt, PlayerCommand.None());
        }

        // ---------- B. 装备状态不变量（换装不得绕过唯一有效连接源） ----------

        [Test]
        public void B1_Equip_ConfiguredHost_Succeeds_GroupsResolved()
        {
            var s = new SliceSession();
            string err;
            ItemInstance body = MakeItem(s, EquipSlot.Body, SkillId.None);
            int bi = s.AddItem(body);
            Assert.IsTrue(s.TryReassignLink(bi, SkillId.Area, out err), err);
            Assert.IsTrue(s.TryEquip(bi, out err), err);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 唯一源=Body 组1 容1");
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Projectile), "Projectile=Body 组0（3 孔拆分后容 0）");
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee), "Melee=Weapon 组0 不受影响");
        }

        [Test]
        public void B2_Equip_CreatingDuplicateGroup1_RejectsAtomically()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            ItemInstance body = MakeItem(s, EquipSlot.Body, SkillId.Area); // 直接构造（域 API 对重复赋值本身已拒=第一道防线）
            int bi = s.AddItem(body);
            int oldBody = s.Equipped[(int)EquipSlot.Body];
            Assert.IsFalse(s.TryEquip(bi, out err), "装备不得创建重复 group-1 所有权");
            Assert.IsTrue(err.Contains("已被其它装备改挂"), err);
            Assert.AreEqual(oldBody, s.Equipped[(int)EquipSlot.Body], "原子回滚：装备位零改动");
            Assert.AreEqual(SkillId.Area, s.Inventory[bi].LinkSkill1, "禁静默清除候选物品绑定");
            Assert.AreEqual(SkillId.Area, s.Inventory[WeaponIdx(s)].LinkSkill1, "原 host 绑定不变");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 仍由原 host 组1 供给");
        }

        [Test]
        public void B3_ReplaceSlotItem_WithSelfReassignedBinding_Rejects()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            // 替换冲突：入装物品绑定=其自身映射技能（自改挂态入装即冲突；绑定由外部构造，域 API 对该赋值本身已拒）
            ItemInstance w2 = MakeItem(s, EquipSlot.Weapon, SkillId.Melee);
            int wi = s.AddItem(w2);
            int oldW = WeaponIdx(s);
            Assert.IsFalse(s.TryEquip(wi, out err), "替换为自改挂绑定物品必须原子拒绝");
            Assert.IsTrue(err.Contains("已自带"), err);
            Assert.AreEqual(oldW, s.Equipped[(int)EquipSlot.Weapon], "原子回滚：装备位零改动");
            Assert.AreEqual(SkillId.Area, s.Inventory[oldW].LinkSkill1, "原 host 绑定不被清除/改写");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 仍由原 host 组1 供给");
        }

        [Test]
        public void B4_ReplaceHost_WithValidConfiguredHost_Commits()
        {
            var s = new SliceSession();
            string err;
            ItemInstance w2 = MakeItem(s, EquipSlot.Weapon, SkillId.None);
            int wi = s.AddItem(w2);
            Assert.IsTrue(s.TryReassignLink(wi, SkillId.Area, out err), err);
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            int oldW = WeaponIdx(s);
            Assert.IsTrue(s.TryEquip(wi, out err), err);
            Assert.AreEqual(wi, s.Equipped[(int)EquipSlot.Weapon]);
            Assert.AreEqual(SkillId.Area, s.Inventory[wi].LinkSkill1);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 唯一源=新 host 组1");
            Assert.AreEqual(SkillId.Area, s.Inventory[oldW].LinkSkill1, "被替换物品入包（未装备=不参与连接图，绑定保留为物品自身状态）");
        }

        [Test]
        public void B5_SwapConfiguredEligibleItems_PreservesUniqueness()
        {
            var s = new SliceSession();
            string err;
            ItemInstance body = MakeItem(s, EquipSlot.Body, SkillId.None);
            int bi = s.AddItem(body);
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.IsTrue(s.TryReassignLink(bi, SkillId.Area, out err), err);
            Assert.IsTrue(s.TryEquip(bi, out err), err);
            Assert.AreEqual(SkillId.Projectile, s.Inventory[WeaponIdx(s)].LinkSkill1);
            Assert.AreEqual(SkillId.Area, s.Inventory[bi].LinkSkill1);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile), "弹道→武器组1");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "范围→胸甲组1");
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee), "近战→武器组0（3 孔拆分容 0）");
        }

        [Test]
        public void B6_ReplaceConfiguredHost_WithUnconfigured_RestoresDefaultSource()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Faster, out err), err);
            ItemInstance w2 = MakeItem(s, EquipSlot.Weapon, SkillId.None);
            int wi = s.AddItem(w2);
            Assert.IsTrue(s.TryEquip(wi, out err), err);
            Assert.AreEqual(SkillId.None, s.Inventory[wi].LinkSkill1);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "host 移除→Area 默认源=头盔 组0 容1");
            Assert.AreEqual(SupportId.Faster, s.ESupports[0], "Support 列表跟随技能身份，不被迁移/丢弃");
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee), "新武器 legacy 组0 容量恢复");
        }

        [Test]
        public void B7_FailedTransition_LeavesStateByteUnchanged()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Faster, out err), err);
            ItemInstance body = MakeItem(s, EquipSlot.Body, SkillId.Area);
            int bi = s.AddItem(body);
            int[] eq; SkillId[] links; SupportId[] q, w, es;
            Snapshot(s, out eq, out links, out q, out w, out es);
            Assert.IsFalse(s.TryEquip(bi, out err), "host 拆分后容量溢出=整次装备拒绝");
            Assert.IsTrue(err.Contains("超出拆分后容量"), err);
            AssertSnapshotUnchanged(s, eq, links, q, w, es);
        }

        [Test]
        public void B8_DuplicateTransitionRejection_OrderIndependent()
        {
            for (int order = 0; order < 2; order++)
            {
                var s = new SliceSession();
                string err;
                EquipSlot firstSlot = order == 0 ? EquipSlot.Weapon : EquipSlot.Body;
                EquipSlot secondSlot = order == 0 ? EquipSlot.Body : EquipSlot.Weapon;
                ItemInstance first = MakeItem(s, firstSlot, SkillId.None);
                int fi = s.AddItem(first);
                Assert.IsTrue(s.TryReassignLink(fi, SkillId.Area, out err), err);
                Assert.IsTrue(s.TryEquip(fi, out err), err);
                ItemInstance second = MakeItem(s, secondSlot, SkillId.Area);
                int si = s.AddItem(second);
                Assert.IsFalse(s.TryReassignLink(si, SkillId.Area, out err), "域 API 第一道防线：重复赋值拒（两种装备顺序一致）");
                Assert.IsTrue(err.Contains("已被其它装备改挂"), err);
                Assert.IsFalse(s.TryEquip(si, out err), "装备守卫第二道防线：重复 group-1 拒（两种装备顺序一致）");
                Assert.IsTrue(err.Contains("已被其它装备改挂"), err);
                Assert.AreEqual(fi, s.Equipped[(int)firstSlot], "已装备 host 不被顶替");
            }
        }

        [Test]
        public void B9_Craft_OnEquippedHost_BindingSemantics()
        {
            var s = new SliceSession();
            string err;
            ItemInstance w = MakeItem(s, EquipSlot.Weapon, SkillId.None);
            w.Rarity = Rarity.Rare; // 随机制作前置=稀有
            int wi = s.AddItem(w);
            Assert.IsTrue(s.TryEquip(wi, out err), err);
            Assert.IsTrue(s.TryReassignLink(wi, SkillId.Area, out err), err);
            // 定向制作=原位词缀编辑：绑定与划分保持
            Assert.IsTrue(s.TryDirectedCraft(wi, AffixId.Life, out err), err);
            Assert.AreEqual(SkillId.Area, s.Inventory[wi].LinkSkill1, "原位制作不改写绑定");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area));
            // 随机制作=整件重铸为新物品：legacy 单组（合法转换，非静默清除存活物品）→ 被改挂技能回默认源
            Assert.IsTrue(s.TryRandomCraft(wi, out err), err);
            Assert.AreEqual(SkillId.None, s.Inventory[wi].LinkSkill1, "重铸物品=legacy 单组");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "Area 默认源恢复=头盔 组0");
            Assert.AreEqual(2, s.SupportCapacity(SkillId.Melee));
        }

        // ---------- C. UI 配置模型（读路径助手=写入同一校验内核） ----------

        [Test]
        public void C1_SecondaryLinkControlEligibility_MatchesDomainTruth()
        {
            Assert.IsTrue(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Weapon, 3)), "武器 3 孔=有配置");
            Assert.IsTrue(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Body, 3)), "胸甲 3 孔=有配置");
            Assert.IsFalse(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Helmet, 2)), "头盔 2 孔=无配置");
            Assert.IsFalse(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Gloves, 1)), "手套 1 孔=无配置");
            Assert.IsFalse(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Belt, 1)), "腰带 1 孔=无配置");
            Assert.IsFalse(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Boots, 0)), "靴子 0 孔=无配置");
            Assert.IsFalse(SliceSession.SecondaryLinkConfigurable(Bare(EquipSlot.Weapon, 2)), "<3 孔映射槽物品=无配置");
        }

        [Test]
        public void C2_HostSkill_CannotBeSelected()
        {
            var s = new SliceSession();
            SkillId[] cands = s.RebindCandidates(s.Inventory[WeaponIdx(s)]);
            Assert.AreEqual(2, cands.Length, "武器候选=弹道/范围（近战=自带映射被排除）");
            Assert.IsFalse(System.Array.Exists(cands, k => k == SkillId.Melee), "自带映射技能不得成为候选");
        }

        [Test]
        public void C3_DuplicateSecondarySkill_CannotBecomeValid()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            ItemInstance body = MakeItem(s, EquipSlot.Body, SkillId.None);
            int bi = s.AddItem(body);
            SkillId[] cands = s.RebindCandidates(s.Inventory[bi]);
            Assert.AreEqual(1, cands.Length, "胸甲候选仅剩近战（自带映射=弹道；范围=已被武器改挂）");
            Assert.AreEqual(SkillId.Melee, cands[0]);
            string reason = s.PreviewReassignError(s.Inventory[bi], SkillId.Area);
            Assert.IsTrue(reason != null && reason.Contains("已被其它装备改挂"), "重复改挂预判=不可用+原因：" + reason);
            Assert.IsFalse(s.TryReassignLink(bi, SkillId.Area, out err), err);
        }

        [Test]
        public void C4_ValidAssign_UpdatesUIModel_AndDomain()
        {
            var s = new SliceSession();
            Assert.IsNull(s.PreviewReassignError(s.Inventory[WeaponIdx(s)], SkillId.Projectile), "合法候选预判=可用");
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.AreEqual(SkillId.Projectile, s.Inventory[WeaponIdx(s)].LinkSkill1);
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile));
            Assert.AreEqual(0, s.SupportCapacity(SkillId.Melee));
        }

        [Test]
        public void C5_Change_AtoB_PreviewThenCommit()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            Assert.IsNull(s.PreviewReassignError(s.Inventory[WeaponIdx(s)], SkillId.Projectile), "A→B 预判=可用");
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.AreEqual(SkillId.Projectile, s.Inventory[WeaponIdx(s)].LinkSkill1, "B 接管组1（单事务无中间双源）");
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Projectile));
            Assert.AreEqual(1, s.SupportCapacity(SkillId.Area), "A 回默认源");
        }

        [Test]
        public void C6_Clear_ReturnsLegacyPresentation()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            Assert.IsNull(s.PreviewReassignError(s.Inventory[WeaponIdx(s)], SkillId.None), "清除预判恒合法");
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.None, out err), err);
            string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
            Assert.AreEqual(2, groups.Length);
            Assert.IsTrue(groups[0].Contains("组0") && groups[0].Contains("容2"), groups[0]);
            Assert.IsTrue(groups[1].Contains("第二连接：无"), groups[1]);
            Assert.IsFalse(System.Array.Exists(groups, g => g.Contains("组1")), "清除后不呈现组1");
        }

        [Test]
        public void C7_FailedAssignment_ReasonVisible_StatePreserved()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.Faster, out err), err);
            string reason = s.PreviewReassignError(s.Inventory[WeaponIdx(s)], SkillId.Projectile);
            Assert.IsTrue(reason != null && reason.Contains("超出拆分后容量"), "容量不可用候选必须带可读原因：" + reason);
            Assert.IsFalse(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.AreEqual(SkillId.None, s.Inventory[WeaponIdx(s)].LinkSkill1, "失败后状态保持");
            Assert.AreEqual(SupportId.Faster, s.QSupports[0], "Support 不被截断/迁移");
        }

        // ---------- D. 技能行唯一源呈现 ----------

        [Test]
        public void D1_DefaultSkill_ShowsGroup0Source()
        {
            var s = new SliceSession();
            string label = s.LinkSourceLabel(SkillId.Melee);
            Assert.IsTrue(label.Contains("武器") && label.Contains("组0") && label.Contains("容2"), label);
        }

        [Test]
        public void D2_ReassignedSkill_ShowsGroup1Host_OriginalNotActive()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            string p = s.LinkSourceLabel(SkillId.Projectile);
            Assert.IsTrue(p.Contains("组1") && p.Contains("铁刃"), "改挂技能=host 物品+组1：" + p);
            Assert.IsFalse(p.Contains("组0") || p.Contains("胸甲"), "原默认位不得同时呈现为生效源：" + p);
            string m = s.LinkSourceLabel(SkillId.Melee);
            Assert.IsTrue(m.Contains("武器") && m.Contains("组0") && m.Contains("容0"), m);
        }

        [Test]
        public void D3_LabelCapacity_MatchesDomainValues()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            SkillId[] skills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };
            for (int i = 0; i < skills.Length; i++)
            {
                string label = s.LinkSourceLabel(skills[i]);
                Assert.IsTrue(label.Contains("容" + s.SupportCapacity(skills[i])),
                    skills[i] + " 标注容量必须=域值：" + label);
            }
        }

        [Test]
        public void D4_OneSourceLabelPerSkill_NoDuplicateRows()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            SkillId[] skills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };
            for (int i = 0; i < skills.Length; i++)
            {
                string label = s.LinkSourceLabel(skills[i]);
                Assert.IsFalse(string.IsNullOrEmpty(label));
                int marks = 0, at = label.IndexOf("组");
                while (at >= 0)
                {
                    marks++;
                    at = label.IndexOf("组", at + 1);
                }
                Assert.AreEqual(1, marks, "每技能行恰一个连接源标注（禁第二 Skill 行/重复行）：" + label);
            }
        }

        // ---------- E. Tooltip 连接组划分真实性 ----------

        [Test]
        public void E1_ItemCard_Legacy_OneGroupAndNoneSecondary()
        {
            var s = new SliceSession();
            string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
            Assert.IsNotNull(groups);
            Assert.AreEqual(2, groups.Length);
            Assert.IsTrue(groups[0].Contains("组0") && groups[0].Contains("近战") && groups[0].Contains("容2"), groups[0]);
            Assert.IsTrue(groups[1].Contains("第二连接：无"), groups[1]);
        }

        [Test]
        public void E2_ItemCard_ActiveSplit_TwoGroupsTruthful()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
            Assert.IsTrue(groups[0].Contains("组0") && groups[0].Contains("近战") && groups[0].Contains("容0"), groups[0]);
            Assert.IsTrue(groups[1].Contains("组1") && groups[1].Contains("弹道") && groups[1].Contains("容1"), groups[1]);
        }

        [Test]
        public void E3_ItemCard_CapacitiesEqualDomainValues()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
            Assert.IsTrue(groups[0].Contains("容" + s.SupportCapacity(SkillId.Melee)), groups[0]);
            Assert.IsTrue(groups[1].Contains("容" + s.SupportCapacity(SkillId.Projectile)), groups[1]);
        }

        [Test]
        public void E4_ThreeSocketSplit_DoesNotImplyFreeSupports()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Area, out err), err);
            string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
            for (int i = 0; i < groups.Length; i++)
                Assert.IsFalse(groups[i].Contains("容3"), "3 孔拆分不得呈现为 2 遗留 Support+额外组（真实=0+1）：" + groups[i]);
            Assert.IsTrue(System.Array.Exists(groups, l => l.Contains("容0")), "组0 真实容量 0 必须如实呈现");
            Assert.IsTrue(System.Array.Exists(groups, l => l.Contains("容1")), "组1 容量 1 必须如实呈现");
        }

        [Test]
        public void E5_CorruptBinding_FailClosedDisplay_AsLegacy()
        {
            var s = new SliceSession();
            ItemInstance w = s.Inventory[WeaponIdx(s)];
            w.LinkSkill1 = SkillId.Melee; // 自改挂腐败态（绕过 API 构造）
            s.Inventory[WeaponIdx(s)] = w;
            string[] groups = s.LinkGroupsText(w);
            Assert.IsTrue(System.Array.Exists(groups, l => l.Contains("第二连接：无")), "非法绑定读路径 fail-closed=按 legacy 一组呈现");
            Assert.IsFalse(System.Array.Exists(groups, l => l.Contains("组1")), "不呈现非法组1");
        }

        // ---------- F. 运行时 Support 隔离（组0/组1 等价、零泄漏） ----------

        [Test]
        public void F1_StatSupport_EquivalentEffect_Group0_vs_Group1()
        {
            string err;
            var g0 = new SliceSession();
            Assert.IsTrue(g0.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            HitRequest r0 = g0.BuildPlayerHit(SkillId.Projectile, default(Dummy));
            Assert.AreEqual(1.4f, r0.MorePhys, 0.0001f, "组0 位生效恰一次（残暴 40% 更多物理）");

            var g1 = new SliceSession();
            Assert.IsTrue(g1.TryReassignLink(WeaponIdx(g1), SkillId.Projectile, out err), err);
            Assert.IsTrue(g1.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            HitRequest r1 = g1.BuildPlayerHit(SkillId.Projectile, default(Dummy));
            Assert.AreEqual(1.4f, r1.MorePhys, 0.0001f, "组1 位等价生效恰一次");
            Assert.AreEqual(r0.MorePhys, r1.MorePhys, 0.0001f, "组0/组1 数值等价");
        }

        [Test]
        public void F2_MechanicSupport_Fork_RealRuntime_Group0_vs_Group1()
        {
            string err;
            // 组0（弹道默认源=Body）
            var simA = NewSim(9u);
            Assert.IsTrue(simA.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.Greater(simA.Session.ForkCount(SkillId.Projectile), 0);
            simA.Dummies.SpawnAt(0f, 2.4f);
            simA.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, 0, 0f, 2.4f));
            TickNone(simA, SkillCatalog.Get(SkillId.Projectile).Windup);
            for (int i = 0; i < 40 && simA.Session.ForkSpawns == 0; i++)
                simA.Tick(Dt, PlayerCommand.None());
            Assert.AreEqual(2, simA.Session.ForkSpawns, "组0 位分裂真实生效恰一次");

            // 组1（武器改挂弹道，同 seed 同操作=确定性等价）
            var simB = NewSim(9u);
            Assert.IsTrue(simB.Session.TryReassignLink(WeaponIdx(simB.Session), SkillId.Projectile, out err), err);
            Assert.AreEqual(1, simB.Session.SupportCapacity(SkillId.Projectile));
            Assert.IsTrue(simB.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.Greater(simB.Session.ForkCount(SkillId.Projectile), 0);
            simB.Dummies.SpawnAt(0f, 2.4f);
            simB.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, 0, 0f, 2.4f));
            TickNone(simB, SkillCatalog.Get(SkillId.Projectile).Windup);
            for (int i = 0; i < 40 && simB.Session.ForkSpawns == 0; i++)
                simB.Tick(Dt, PlayerCommand.None());
            Assert.AreEqual(2, simB.Session.ForkSpawns, "组1 位分裂真实生效恰一次（真实 runtime，非仅数据模型）");
        }

        [Test]
        public void F3_Isolation_HostGroup0_CannotReachReassignedSkill()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            // 结构隔离①：拆分后 host 组0 容量 0——host 组0 Support 无法与改挂共存（原子校验拒绝，无泄漏载体）
            Assert.IsFalse(s.TrySetSupport(SkillId.Melee, 0, SupportId.Brutal, out err), err);
            Assert.IsTrue(err.Contains("孔不足"), err);
            // 结构隔离②：改挂技能聚合只来自其唯一组（他技能 Support 不进入）
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Brutal, out err), err);
            var bag = new StatBag();
            s.CollectSkillMods(SkillId.Projectile, bag);
            Assert.AreEqual(0f, bag.Get(StatId.MorePhysical), 0.0001f, "host 组0/第三技能 Support 不得进入改挂技能聚合");
        }

        [Test]
        public void F4_Isolation_Group1Supports_CannotReachHostGroup0()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.Projectile, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Brutal, out err), err);
            HitRequest m = s.BuildPlayerHit(SkillId.Melee, default(Dummy));
            Assert.AreEqual(1f, m.MorePhys, 0.0001f, "组1 Support 不得跨组泄漏到 host 组0");
            HitRequest p = s.BuildPlayerHit(SkillId.Projectile, default(Dummy));
            Assert.AreEqual(1.4f, p.MorePhys, 0.0001f, "组1 Support 生效恰一次");
        }

        // ---------- G. 负面表面 ----------

        [Test]
        public void G1_NoColorLevelQualityOrThirdGroup_StateOrControl()
        {
            var fields = typeof(ItemInstance).GetFields();
            foreach (var f in fields)
            {
                Assert.IsFalse(f.Name.Contains("Color"), "禁止孔颜色状态：" + f.Name);
                Assert.IsFalse(f.Name.Contains("Level") || f.Name.Contains("Quality"), "禁止宝石等级/品质状态：" + f.Name);
                Assert.IsFalse(f.Name.Contains("LinkSkill2") || f.Name.Contains("LinkGroup"), "禁止第三组状态：" + f.Name);
            }
            var methods = typeof(SliceSession).GetMethods();
            foreach (var m in methods)
            {
                string n = m.Name;
                Assert.IsFalse(n.Contains("Color"), "禁止孔颜色控制路径：" + n);
                Assert.IsFalse(n.Contains("GemLevel") || n.Contains("Quality"), "禁止宝石等级/品质控制路径：" + n);
                Assert.IsFalse(n.Contains("LinkSkill2") || n.Contains("ThirdGroup"), "禁止第三组控制路径：" + n);
            }
        }

        [Test]
        public void G2_LinkGroupsText_NeverEmitsThirdGroup()
        {
            var s = new SliceSession();
            string err;
            SkillId[] states = { SkillId.None, SkillId.Projectile, SkillId.Area };
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] != SkillId.None)
                {
                    Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), states[i], out err), err);
                }
                string[] groups = s.LinkGroupsText(s.Inventory[WeaponIdx(s)]);
                for (int g = 0; g < groups.Length; g++)
                    Assert.IsFalse(groups[g].Contains("组2"), "呈现层禁第三组：" + groups[g]);
                if (states[i] != SkillId.None)
                {
                    Assert.IsTrue(s.TryReassignLink(WeaponIdx(s), SkillId.None, out err), err);
                }
            }
        }
    }
}
