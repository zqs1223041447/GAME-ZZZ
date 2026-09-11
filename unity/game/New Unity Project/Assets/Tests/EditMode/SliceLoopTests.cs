using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class SliceLoopTests
    {
        const float Dt = 0.02f;

        [Test]
        public void ForkSupport_SplitsProjectileOnHit()
        {
            var sim = NewSliceSim();
            string err;
            Assert.IsTrue(sim.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.Greater(sim.Session.ForkCount(SkillId.Projectile), 0);
            sim.Dummies.SpawnAt(0f, 2.4f);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, 0, 0f, 2.4f));
            TickNone(sim, SkillCatalog.Get(SkillId.Projectile).Windup);
            Assert.Greater(sim.Projectiles.AliveCount, 0);

            for (int i = 0; i < 40 && sim.Session.ForkSpawns == 0; i++)
                sim.Tick(Dt, PlayerCommand.None());

            Assert.AreEqual(2, sim.Session.ForkSpawns);
            Assert.GreaterOrEqual(sim.Projectiles.AliveCount, 2);
            int forks = 0;
            for (int i = 0; i < sim.Projectiles.Items.Length; i++)
            {
                if (sim.Projectiles.Items[i].Alive && sim.Projectiles.Items[i].FromFork)
                    forks++;
            }

            Assert.AreEqual(2, forks);
        }

        [Test]
        public void InMap_BuildChangeFails_DeathRespecUnlocks()
        {
            var sim = NewSliceSim();
            string err;
            Assert.IsTrue(sim.Session.TryEnterMap(sim, out err), err);
            Assert.IsTrue(sim.Session.BuildLocked);
            Assert.IsFalse(sim.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(sim.Session.TryEquip(0, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(sim.Session.TryAllocate(1, out err));
            Assert.AreEqual(SliceCopy.LockFail, err);
            Assert.IsFalse(sim.Session.TryRandomCraft(0, out err));
            Assert.IsFalse(sim.Session.TryDirectedCraft(0, AffixId.Life, out err));

            sim.Session.ExitMap(sim, true);
            Assert.IsTrue(sim.Session.Alive);
            Assert.AreEqual(CastPhase.Idle, sim.Caster.Phase);
            Assert.IsFalse(sim.Session.BuildLocked);
            // 真实天赋域：起点（Scion）恒已点亮，其可兑现的邻居在出图后可加点
            // （S6P-WO-04A：起点邻居里含 blocked 行的节点不再可点，这里取第一个可兑现邻居）
            int first = FirstAllocatableNeighbour();
            Assert.GreaterOrEqual(first, 0, "起点必须至少有一个可兑现邻居");
            Assert.IsFalse(sim.Session.Allocated[first]);
            Assert.IsTrue(sim.Session.TryAllocate(first, out err), err);
            Assert.IsTrue(sim.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
        }

        static int FirstAllocatableNeighbour()
        {
            int[] links = PoeTree.Get(SliceSession.StartNode).links;
            if (links == null)
                return -1;
            for (int i = 0; i < links.Length; i++)
                if (PassiveSupport.IsAllocatable(PassiveSupport.EvaluateNode(links[i])))
                    return links[i];
            return -1;
        }

        [Test]
        public void DropAndCraft_SameSeed_Reproducible()
        {
            ItemInstance a = RollWith(42u, 7u);
            ItemInstance b = RollWith(42u, 7u);
            Assert.AreEqual(a.Rarity, b.Rarity);
            Assert.AreEqual(a.AffixCount, b.AffixCount);
            Assert.AreEqual(a.Affix0, b.Affix0);
            Assert.AreEqual(a.Value0, b.Value0, 0.0001f);
            Assert.AreEqual(a.Affix1, b.Affix1);
            Assert.AreEqual(a.Value1, b.Value1, 0.0001f);
            Assert.AreEqual(a.Affix2, b.Affix2);
            Assert.AreEqual(a.Value2, b.Value2, 0.0001f);

            ItemInstance c = CraftRandomWith(11u, 3u);
            ItemInstance d = CraftRandomWith(11u, 3u);
            Assert.AreEqual(c.Affix0, d.Affix0);
            Assert.AreEqual(c.Value0, d.Value0, 0.0001f);
            Assert.AreEqual(c.Affix1, d.Affix1);
            Assert.AreEqual(c.Value1, d.Value1, 0.0001f);
        }

        [Test]
        public void MapLayout_SameSeedAndAffixes_Stable()
        {
            var simA = NewSliceSim();
            var simB = NewSliceSim();
            simA.Session.MapAffixOn[0] = true;
            simA.Session.MapAffixOn[2] = true;
            simB.Session.MapAffixOn[0] = true;
            simB.Session.MapAffixOn[2] = true;
            string err;
            Assert.IsTrue(simA.Session.TryEnterMap(simA, out err), err);
            Assert.IsTrue(simB.Session.TryEnterMap(simB, out err), err);
            Assert.AreEqual(simA.Dummies.AliveCount, simB.Dummies.AliveCount);
            Assert.Greater(simA.Dummies.AliveCount, 10);
            for (int i = 0; i < simA.Dummies.Items.Length; i++)
            {
                Assert.AreEqual(simA.Dummies.Items[i].Occupied, simB.Dummies.Items[i].Occupied);
                if (!simA.Dummies.Items[i].Occupied)
                    continue;
                Assert.AreEqual(simA.Dummies.Items[i].Kind, simB.Dummies.Items[i].Kind);
                Assert.AreEqual(simA.Dummies.Items[i].X, simB.Dummies.Items[i].X, 0.0001f);
                Assert.AreEqual(simA.Dummies.Items[i].Z, simB.Dummies.Items[i].Z, 0.0001f);
            }

            Assert.AreEqual(simA.Session.Stability, simB.Session.Stability);
            Assert.AreEqual(simA.Session.RewardMultiplier, simB.Session.RewardMultiplier, 0.0001f);
            Assert.Greater(simA.Session.RewardMultiplier, 1.8f);
        }

        [Test]
        public void Concentrated_ShrinksAreaRadius()
        {
            var sim = NewSliceSim();
            float before = sim.Caster.Def(SkillId.Area).AreaRadius;
            string err;
            Assert.IsTrue(sim.Session.TrySetSupport(SkillId.Area, 0, SupportId.Concentrated, out err), err);
            float after = sim.Caster.Def(SkillId.Area).AreaRadius;
            Assert.Less(after, before * 0.8f);
        }

        [Test]
        public void RewardUiReadable_FromAffixes()
        {
            var s = new SliceSession();
            s.ToggleMapAffix(0);
            s.ToggleMapAffix(1);
            s.ToggleMapAffix(2);
            Assert.AreEqual(40, s.Stability);
            Assert.AreEqual(2.30f, s.RewardMultiplier, 0.001f);
        }

        [Test]
        public void ItemName_RarityAppearsOnce()
        {
            ItemInstance it = default;
            it.Slot = EquipSlot.Weapon;
            it.Rarity = Rarity.Rare;
            it.BaseName = "Rare Weapon";
            it.SocketCount = 3;
            it.AffixCount = 0;
            string d = SliceSession.DescribeItem(it);
            Assert.IsFalse(d.Contains("稀有 稀有"), d);
            Assert.IsTrue(d.StartsWith("稀有 "), d);
            Assert.AreEqual("铁盔", SliceSession.ItemBaseName(EquipSlot.Helmet));
            Assert.AreEqual("Weapon", SliceSession.CleanBaseName("Rare Weapon"));
            Assert.AreEqual("铁盔", SliceSession.CleanBaseName("普通 铁盔"));
        }

        [Test]
        public void SkillHud_UsesDisplayNameNotRepeatedKey()
        {
            Assert.AreEqual("近战", SliceSession.SkillDisplayName(SkillId.Melee));
            Assert.AreEqual("弹道", SliceSession.SkillDisplayName(SkillId.Projectile));
            Assert.AreEqual("范围", SliceSession.SkillDisplayName(SkillId.Area));
            Assert.AreEqual("Q", SliceSession.SkillHotkey(SkillId.Melee));
            Assert.AreNotEqual(SliceSession.SkillDisplayName(SkillId.Melee), SliceSession.SkillHotkey(SkillId.Melee));
            string label = new SliceSession().SupportLabel(SkillId.Melee);
            Assert.IsFalse(label.StartsWith("Q Q"), label);
            Assert.IsTrue(label.StartsWith("近战"), label);
        }

        [Test]
        public void Cleared_UnlocksBuild_PrimaryExit()
        {
            var sim = NewSliceSim();
            string err;
            Assert.IsTrue(sim.Session.TryEnterMap(sim, out err), err);
            Assert.IsTrue(sim.Session.BuildLocked);
            Assert.AreEqual(SliceCopy.CombatLocked, sim.Session.StatusCopy);
            sim.Session.State = MapState.Cleared;
            sim.Session.Snapshot.Locked = false;
            Assert.IsFalse(sim.Session.BuildLocked);
            Assert.IsTrue(sim.Session.OnMap);
            Assert.AreEqual(SliceCopy.Cleared, sim.Session.StatusCopy);
            Assert.IsTrue(sim.Session.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.IsFalse(sim.Session.TryEnterMap(sim, out err));
            Assert.AreEqual(SliceCopy.ExitFirst, err);
            sim.Session.ExitMap(sim, false);
            Assert.IsFalse(sim.Session.OnMap);
            Assert.AreEqual(SliceCopy.TownFree, sim.Session.StatusCopy);
        }

        [Test]
        public void StatusCopy_FourStates()
        {
            var s = new SliceSession();
            Assert.AreEqual(SliceCopy.TownFree, s.StatusCopy);
            s.State = MapState.InMap;
            Assert.AreEqual(SliceCopy.CombatLocked, s.StatusCopy);
            s.State = MapState.Cleared;
            Assert.AreEqual(SliceCopy.Cleared, s.StatusCopy);
            s.State = MapState.Dead;
            Assert.AreEqual(SliceCopy.DeadRespec, s.StatusCopy);
        }

        [Test]
        public void Death_UnsticksPlayer_AndLowersStability()
        {
            var sim = NewSliceSim();
            sim.Session.ToggleMapAffix(0);
            sim.Session.ToggleMapAffix(1);
            sim.Session.ToggleMapAffix(2);
            Assert.AreEqual(40, sim.Session.Stability);
            string err;
            Assert.IsTrue(sim.Session.TryEnterMap(sim, out err), err);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);

            HitResult kill = default;
            // 导演 2026-09-08 玩家基础血量=9999999，致死量同步抬升（死亡路径行为断言不变）
            kill.TotalTaken = 99999999;
            sim.Session.ApplyPlayerHit(kill);
            Assert.IsFalse(sim.Session.Alive);
            sim.Tick(Dt, PlayerCommand.None());

            Assert.IsTrue(sim.Session.Alive);
            Assert.AreEqual(MapState.Dead, sim.Session.State);
            Assert.AreEqual(CastPhase.Idle, sim.Caster.Phase);
            Assert.AreEqual(AnimState.Idle, sim.Player.Anim);
            Assert.Less(sim.Session.Stability, 40);
            Assert.AreEqual(15, sim.Session.Stability);

            sim.Tick(Dt, PlayerCommand.MoveTo(4f, 0f));
            Assert.IsTrue(sim.Player.HasDest);
            for (int i = 0; i < 40; i++)
                sim.Tick(Dt, PlayerCommand.None());
            Assert.Greater(sim.Player.X, 1f);
        }

        [Test]
        public void PlayerFacingText_IsChinese()
        {
            Assert.AreEqual("燃烧", SupportCatalog.Get(SupportId.AddedFire).Name);
            Assert.AreEqual("分裂", SupportCatalog.Get(SupportId.Fork).Name);
            // 真实天赋域：目录直接由 PoE 数据驱动，抽查一个基石名字
            int keystone = -1;
            for (int i = 0; i < PassiveCatalog.Count; i++)
                if (PassiveCatalog.Get(i).Mechanic) { keystone = i; break; }
            Assert.GreaterOrEqual(keystone, 0, "真实天赋域必须含基石节点");
            Assert.IsNotEmpty(PassiveCatalog.Get(keystone).Name);
            Assert.AreEqual("壮硕", MapAffixCatalog.All[0].Name);
            Assert.AreEqual("普通", SliceSession.RarityWord(Rarity.Ordinary));
            Assert.AreEqual("稀有", SliceSession.RarityWord(Rarity.Rare));
            Assert.AreEqual("铁刃", SliceSession.ItemBaseName(EquipSlot.Weapon));
        }

        [Test]
        public void Qwe_StillShareCastPipeline_WithSession()
        {
            var sim = NewSliceSim();
            SkillDef melee = sim.Caster.Def(SkillId.Melee);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            TickNone(sim, melee.Windup + melee.Active + melee.Recovery);
            sim.Tick(Dt, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            Assert.AreEqual(CastPhase.Windup, sim.Caster.Phase);
            Assert.GreaterOrEqual(sim.CastEvents, 1);
        }

        [Test]
        public void DirectedCraft_HybridAffix_WritesBothRows()
        {
            var s = new SliceSession();
            // 清空初始装备，隔离断言
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;

            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = EquipSlot.Weapon;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = 3;
            it.BaseName = "Test";
            int idx = s.AddItem(it);
            s.Equipped[(int)EquipSlot.Weapon] = idx;

            string err;
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.IgniteFire, out err), err);
            ItemInstance crafted = s.Inventory[idx];
            Assert.AreEqual(1, crafted.AffixCount);
            Assert.AreEqual((int)AffixId.IgniteFire, crafted.AffixIdAt(0));

            AffixDef def = AffixCatalog.Get(AffixId.IgniteFire);
            Assert.GreaterOrEqual(crafted.ValueAt(0), def.Min);
            Assert.LessOrEqual(crafted.ValueAt(0), def.Max);
            Assert.GreaterOrEqual(crafted.SecondValueAt(0), def.Min2);
            Assert.LessOrEqual(crafted.SecondValueAt(0), def.Max2);

            // 两行都必须进技能属性包（行 1 = FireDamage 提高，行 2 = IgniteChance 固定）
            var bag = new StatBag();
            s.CollectSkillMods(SkillId.Projectile, bag);
            Assert.AreEqual(crafted.ValueAt(0), bag.RawIncreased(StatId.FireDamage), 0.0001f);
            Assert.AreEqual(crafted.SecondValueAt(0), bag.RawFlat(StatId.IgniteChance), 0.0001f);
        }

        [Test]
        public void DirectedCraft_SingleRowOverHybrid_NoSecondValueResidue()
        {
            var s = new SliceSession();
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = EquipSlot.Weapon;
            it.Rarity = Rarity.Rare;
            it.SocketCount = 3;
            it.BaseName = "Test";
            it.AffixCount = 4;
            // S4-P4：词缀唯一性契约（物品内不得重复）——slot 0 改用 IncPhys，使定向制作 Life 覆写 slot 3 不触发 duplicate guard
            it.SetAffix(0, AffixId.IncPhys, 0.20f, 0f);
            it.SetAffix(1, AffixId.Armour, 30f, 0f);
            it.SetAffix(2, AffixId.Evasion, 30f, 0f);
            it.SetAffix(3, AffixId.IgniteFire, 0.20f, 0.15f); // 组合词缀带第二值
            int idx = s.AddItem(it);
            s.Equipped[(int)EquipSlot.Weapon] = idx;

            string err;
            // 满槽定向制作：覆写最后一个槽（3）为单行词缀
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.Life, out err), err);
            ItemInstance crafted = s.Inventory[idx];
            Assert.AreEqual((int)AffixId.Life, crafted.AffixIdAt(3));
            Assert.AreEqual(0f, crafted.SecondValueAt(3), 0.0001f, "单行词缀覆写后第二值必须归零（不得泄漏）");
            Assert.GreaterOrEqual(crafted.ValueAt(3), AffixCatalog.Get(AffixId.Life).Min);
            Assert.LessOrEqual(crafted.ValueAt(3), AffixCatalog.Get(AffixId.Life).Max);
        }

        [Test]
        public void DirectedCraft_AccCrit_DualRangeIndependent()
        {
            var s = new SliceSession();
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = EquipSlot.Body;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = 3;
            it.BaseName = "Test";
            int idx = s.AddItem(it);

            string err;
            Assert.IsTrue(s.TryDirectedCraft(idx, AffixId.AccCrit, out err), err);
            ItemInstance crafted = s.Inventory[idx];
            AffixDef def = AffixCatalog.Get(AffixId.AccCrit);
            // 两行范围差异明显（20–50 vs 0.15–0.30）：第一行不得污染第二行
            Assert.GreaterOrEqual(crafted.ValueAt(0), def.Min);
            Assert.LessOrEqual(crafted.ValueAt(0), def.Max);
            Assert.GreaterOrEqual(crafted.SecondValueAt(0), def.Min2);
            Assert.LessOrEqual(crafted.SecondValueAt(0), def.Max2);
            Assert.Less(crafted.SecondValueAt(0), 1f, "第二行必须使用自身百分比范围");
        }

        static ItemInstance RollWith(uint sessionSeed, uint rollSeed)
        {
            var s = new SliceSession();
            s.SessionSeed = sessionSeed;
            return s.RollItem(EquipSlot.Weapon, Rarity.Rare, new SeededRng(rollSeed), 3, "Test");
        }

        static ItemInstance CraftRandomWith(uint lootSeed, uint baseSeed)
        {
            var s = new SliceSession();
            s.LootRng = new SeededRng(lootSeed);
            s.Scrap = 5;
            ItemInstance rare = s.RollItem(EquipSlot.Body, Rarity.Rare, new SeededRng(baseSeed), 3, "RareBody");
            int idx = s.AddItem(rare);
            string err;
            Assert.IsTrue(s.TryRandomCraft(idx, out err), err);
            return s.Inventory[idx];
        }

        static ArenaSim NewSliceSim()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            sim.Caster.Defs = sim.Session.ResolveSkillDef;
            return sim;
        }

        static void TickNone(ArenaSim sim, float seconds)
        {
            int n = (int)System.Math.Round(seconds / Dt);
            for (int i = 0; i < n; i++)
                sim.Tick(Dt, PlayerCommand.None());
        }
    }
}
