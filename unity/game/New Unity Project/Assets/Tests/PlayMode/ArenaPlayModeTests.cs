using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    public sealed class ArenaPlayModeTests
    {
        [UnityTest]
        public IEnumerator ArenaDirector_BootsAndThreeSkillsFire()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            Assert.IsNotNull(director.Sim);
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            SkillDef proj = SkillCatalog.Get(SkillId.Projectile);
            SkillDef area = SkillCatalog.Get(SkillId.Area);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Step(director.Sim, melee.Windup + melee.Active + melee.Recovery);
            Assert.GreaterOrEqual(director.Sim.CastEvents, 1);

            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            Step(director.Sim, proj.Windup + proj.Active + proj.Recovery);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 4f));
            Step(director.Sim, area.Windup);
            Assert.GreaterOrEqual(director.Sim.CastEvents, 3);

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProjectilesAndDummies_HaveNoRigidbodyOrNavMeshAgent()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            director.Sim.SpawnDummies(20, CombatRules.ArenaSeed);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 8f));
            Step(director.Sim, SkillCatalog.Get(SkillId.Projectile).Windup);
            yield return null;

            Rigidbody[] bodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            Assert.AreEqual(0, bodies.Length, "S1 禁止弹道/Dummy Rigidbody");

            NavMeshAgent[] agents = Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            Assert.AreEqual(0, agents.Length, "S1 禁止 Dummy NavMeshAgent");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerformanceArena_WritesCostRows()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            yield return director.SampleDensity(100, 8, 20);
            yield return director.SampleDensity(200, 8, 20);
            yield return director.SampleDensity(300, 8, 20);

            Assert.IsFalse(string.IsNullOrEmpty(director.LastPerfPath));
            Assert.IsTrue(File.Exists(director.LastPerfPath), director.LastPerfPath);
            string text = File.ReadAllText(director.LastPerfPath);
            StringAssert.Contains("100,", text);
            StringAssert.Contains("200,", text);
            StringAssert.Contains("300,", text);
            StringAssert.Contains("top_bottlenecks", text);

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SixSlot_EquipReplaceLoop_NoException()
        {
            // S4-P2 §28：真实 loop——取/建手套→穿戴→聚合变化→替换移除旧词缀；腰带同理；旧四槽保持；无异常。
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            if (director.Sim.Session == null)
            {
                director.Sim.Session = new SliceSession();
                director.Sim.Caster.Defs = director.Sim.Session.ResolveSkillDef;
            }

            var s = director.Sim.Session;
            Assert.AreEqual(6, s.Equipped.Length, "六槽装备数组（随 EquipSlot.Count 派生）");
            for (int i = 0; i <= 3; i++)
                Assert.GreaterOrEqual(s.Equipped[i], 0, "旧四槽初始装备保持");

            float baseLife = s.MaxLife;
            ItemInstance g = MakeItem(s, EquipSlot.Gloves, 100f);
            string err;
            Assert.IsTrue(s.TryEquip(s.AddItem(g), out err), err);
            Assert.Greater(s.MaxLife, baseLife, "手套词缀进 canonical 聚合");

            ItemInstance g2 = MakeItem(s, EquipSlot.Gloves, 300f);
            Assert.IsTrue(s.TryEquip(s.AddItem(g2), out err), err);
            Assert.Greater(s.MaxLife, baseLife + 150f, "替换后新手套生效");

            ItemInstance b = MakeItem(s, EquipSlot.Belt, 50f);
            int beltIdx = s.AddItem(b);
            Assert.IsTrue(s.TryEquip(beltIdx, out err), err);
            Assert.Greater(s.MaxLife, baseLife + 150f, "腰带词缀进 canonical 聚合");
            Assert.AreEqual(beltIdx, s.Equipped[(int)EquipSlot.Belt], "腰带装备槽指向新物品");

            ItemInstance b2 = MakeItem(s, EquipSlot.Belt, 10f);
            Assert.IsTrue(s.TryEquip(s.AddItem(b2), out err), err);
            Assert.Less(s.MaxLife, baseLife + 300f + 50f, "替换后旧腰带词缀移除（新=300+10 < 旧=300+50）");

            // 原四槽语义保持：仍可替换（武器）
            ItemInstance w = MakeItem(s, EquipSlot.Weapon, 500f);
            Assert.IsTrue(s.TryEquip(s.AddItem(w), out err), err);

            // 战斗 tick 不因六槽状态异常
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AffixApplicability_SessionLoop_NoIllegalAffix()
        {
            // S4-P3 §三十四：真实 session path——生成 Gloves 出现合法新词缀→equip 后 Modifier 生效（AttackSpeed 缩短 recovery）→
            // craft 后仍合法；Belt 定向制作合法写入、非法组合 deterministic reject（不消耗）；旧槽 gameplay 正常。
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            if (director.Sim.Session == null)
            {
                director.Sim.Session = new SliceSession();
                director.Sim.Caster.Defs = director.Sim.Session.ResolveSkillDef;
            }
            var s = director.Sim.Session;

            ItemInstance picked = default;
            bool hasSwift = false;
            for (uint seed = 1u; seed <= 120u && !hasSwift; seed++)
            {
                ItemInstance g = s.RollItem(EquipSlot.Gloves, Rarity.Rare, new SeededRng(seed),
                    SliceSession.SocketsFor(EquipSlot.Gloves), SliceSession.ItemBaseName(EquipSlot.Gloves));
                for (int a = 0; a < g.AffixCount; a++)
                    if (g.AffixIdAt(a) == (int)AffixId.SwiftGrip) { picked = g; hasSwift = true; break; }
            }
            Assert.IsTrue(hasSwift, "真实 RollItem 路径必须可达新 Gloves 词缀（迅握）");
            for (int a = 0; a < picked.AffixCount; a++)
                Assert.IsTrue(AffixCatalog.Get((AffixId)picked.AffixIdAt(a)).IsApplicable(EquipSlot.Gloves),
                    "手套物品词缀必须合法");

            float baseRecovery = s.ResolveSkillDef(SkillId.Melee).Recovery;
            string err;
            Assert.IsTrue(s.TryEquip(s.AddItem(picked), out err), err);
            float withSwift = s.ResolveSkillDef(SkillId.Melee).Recovery;
            Assert.Less(withSwift, baseRecovery, "AttackSpeed 词缀必须经 canonical 聚合生效（recovery 缩短）");

            // craft 后仍合法（随机制作=同槽 eligible 池重掷）
            s.Scrap = 5;
            int glovesIdx = s.Equipped[(int)EquipSlot.Gloves];
            Assert.IsTrue(s.TryRandomCraft(glovesIdx, out err), err);
            for (int a = 0; a < s.Inventory[glovesIdx].AffixCount; a++)
                Assert.IsTrue(AffixCatalog.Get((AffixId)s.Inventory[glovesIdx].AffixIdAt(a)).IsApplicable(EquipSlot.Gloves),
                    "随机制作后不得出现非法词缀");

            // Belt：定向制作合法写入；Gloves 专属词缀永不注入 Belt
            ItemInstance belt = s.RollItem(EquipSlot.Belt, Rarity.Ordinary, new SeededRng(11u),
                SliceSession.SocketsFor(EquipSlot.Belt), SliceSession.ItemBaseName(EquipSlot.Belt));
            int beltIdx = s.AddItem(belt);
            Assert.IsTrue(s.TryDirectedCraft(beltIdx, AffixId.Bulwark, out err), err);
            Assert.IsFalse(s.TryDirectedCraft(glovesIdx, AffixId.Bulwark, out err), "Belt 专属词缀不得注入手套");
            director.Sim.Tick(0.02f, PlayerCommand.None());
            yield return null;

            // 旧槽 gameplay 正常：武器仍可替换
            ItemInstance w = s.RollItem(EquipSlot.Weapon, Rarity.Rare, new SeededRng(13u),
                SliceSession.SocketsFor(EquipSlot.Weapon), SliceSession.ItemBaseName(EquipSlot.Weapon));
            Assert.IsTrue(s.TryEquip(s.AddItem(w), out err), err);

            Object.Destroy(go);
            yield return null;
        }

        static ItemInstance MakeItem(SliceSession s, EquipSlot slot, float lifeValue)
        {
            ItemInstance it = default;
            it.Id = s.NextItemId++;
            it.Slot = slot;
            it.Rarity = Rarity.Ordinary;
            it.SocketCount = SliceSession.SocketsFor(slot);
            it.BaseName = SliceSession.ItemBaseName(slot);
            it.AffixCount = 1;
            it.SetAffix(0, AffixId.Life, lifeValue, 0f);
            return it;
        }

        static void Step(ArenaSim sim, float seconds)
        {
            const float dt = 0.02f;
            int n = Mathf.RoundToInt(seconds / dt);
            for (int i = 0; i < n; i++)
                sim.Tick(dt, PlayerCommand.None());
        }
    }
}
