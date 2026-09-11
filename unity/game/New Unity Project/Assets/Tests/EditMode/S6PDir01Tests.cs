using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-05（导演 2026-09-11 补充要求 4/5/6）：
    /// ①装备图标 / 技能特效改由 poedb.tw 真实美术承载（SlicePoeArt 资源契约 + 消费点真值）；
    /// ②新增两条主动技能：冰矛（SkillId=4）与火球术（SkillId=5），身份/定义逐条钉死；
    /// ③新增两条机制型辅助：投射物返回（SupportId=8）与狙击印记（SupportId=9），
    ///    行为在 ProjectilePool / ArenaSim / DummyCrowd 上可观测断言。
    /// </summary>
    public sealed class S6PDir01Tests
    {
        const string ArtRoot = "Assets/Resources/UI/PoE";

        // ===================== ① poedb 美术契约 =====================

        static readonly string[] ArtFolders = { "Items", "Skills", "Supports", "Vfx" };

        [Test]
        public void PoeArt_DeclaredKeys_AllLoadable()
        {
            var missing = new List<string>();
            foreach (var key in SlicePoeArt.DeclaredKeys)
                if (SlicePoeArt.Get(key) == null)
                    missing.Add(key);
            Assert.IsEmpty(missing, "SlicePoeArt 声明 key 加载失败: " + string.Join(", ", missing));
        }

        [Test]
        public void PoeArt_DeclaredKeys_NoDuplicates()
        {
            var seen = new HashSet<string>();
            var dupes = new List<string>();
            foreach (var key in SlicePoeArt.DeclaredKeys)
                if (!seen.Add(key))
                    dupes.Add(key);
            Assert.IsEmpty(dupes, "SlicePoeArt 声明 key 重复: " + string.Join(", ", dupes));
        }

        [Test]
        public void PoeArt_Folders_HaveNoUndeclaredTextures()
        {
            var declared = new HashSet<string>(SlicePoeArt.DeclaredKeys);
            var extra = new List<string>();
            foreach (var folder in ArtFolders)
            {
                var root = Path.Combine(Application.dataPath, "Resources/UI/PoE", folder).Replace('\\', '/');
                if (!Directory.Exists(root))
                    continue;
                foreach (var png in Directory.GetFiles(root, "*.png", SearchOption.TopDirectoryOnly))
                {
                    var rel = folder + "/" + Path.GetFileNameWithoutExtension(png);
                    if (!declared.Contains(rel))
                        extra.Add(rel);
                }
            }
            Assert.IsEmpty(extra, "poedb 美术目录出现未声明 png（违反选材摄取）: " + string.Join(", ", extra));
        }

        [Test]
        public void PoeArt_MissingPath_ReturnsNull_NoThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                Assert.IsNull(SlicePoeArt.Get("Items/DoesNotExist"), "不存在路径必须回退 null");
                Assert.IsNull(SlicePoeArt.Get(null), "空路径必须回退 null");
                Assert.IsNull(SlicePoeArt.SkillArt(SkillId.None), "None 无美术");
            });
        }

        [Test]
        public void ItemIcons_AllSlotsBothRarities_AreRealArt()
        {
            // 导演原话「装备图标现在是纯2D」——此断言锁定：六槽 × 两稀有度全部拿到 poedb 真实物品图，
            // 且消费点 SliceHudIcons.ItemIcon 返回的就是该美术（不是程序化槽位符号）。
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                foreach (var rarity in new[] { Rarity.Ordinary, Rarity.Rare })
                {
                    EquipSlot slot = (EquipSlot)s;
                    Texture2D art = SlicePoeArt.ItemArt(slot, rarity);
                    Assert.IsNotNull(art, slot + " " + rarity + " 缺 poedb 物品图");
                    Assert.AreSame(art, SliceHudIcons.ItemIcon(slot, rarity),
                        slot + " 消费点未使用 poedb 物品图");
                }
            }
        }

        [Test]
        public void SkillIcons_AllActiveSkills_AreRealArt()
        {
            foreach (SkillId skill in SkillTagGolden.All)
            {
                Texture2D art = SlicePoeArt.SkillArt(skill);
                Assert.IsNotNull(art, skill + " 缺 poedb 技能宝石图");
                Assert.AreSame(art, SliceHudIcons.SkillGlyph(skill), skill + " 技能槽未使用 poedb 宝石图");
                Assert.IsTrue(SliceHudIcons.SkillGlyphIsArt(skill));
            }
        }

        [Test]
        public void SupportIcons_AllSupports_AreRealArt()
        {
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                var id = (SupportId)i;
                Assert.IsNotNull(SlicePoeArt.SupportArt(id), id + " 缺 poedb 辅助宝石图");
            }
        }

        [Test]
        public void ProjectileVfx_AllProjectileSkills_Covered()
        {
            foreach (var skill in new[] { SkillId.Projectile, SkillId.Melee, SkillId.Area, SkillId.IceSpear, SkillId.Fireball })
                Assert.IsNotNull(SlicePoeArt.ProjectileArt(skill), skill + " 缺投射物特效图");
        }

        [Test]
        public void ProjectileArt_TwoNewSkills_UseDedicatedVfx()
        {
            // 两条新技能的弹体必须用各自的特效图（不是退回宝石图/通用弹道图）
            Assert.AreSame(SlicePoeArt.IceSpearVfx, SlicePoeArt.ProjectileArt(SkillId.IceSpear), "冰矛弹体必须用冰矛特效图");
            Assert.AreSame(SlicePoeArt.FireballVfx, SlicePoeArt.ProjectileArt(SkillId.Fireball), "火球弹体必须用火球特效图");
            Assert.AreNotSame(SlicePoeArt.ProjectileArt(SkillId.IceSpear), SlicePoeArt.ProjectileArt(SkillId.Fireball),
                "两条技能的弹体美术必须可区分");
            // 既有投射物仍走宝石图（回退链 intact）
            Assert.AreSame(SlicePoeArt.SkillArt(SkillId.Projectile), SlicePoeArt.ProjectileArt(SkillId.Projectile));
        }

        // ===================== ② 新技能身份与定义 =====================

        [Test]
        public void SkillIdentity_StableNumericIds()
        {
            Assert.AreEqual(0, (int)SkillId.None);
            Assert.AreEqual(1, (int)SkillId.Melee);
            Assert.AreEqual(2, (int)SkillId.Projectile);
            Assert.AreEqual(3, (int)SkillId.Area);
            Assert.AreEqual(4, (int)SkillId.IceSpear, "冰矛=4（既有 1-3 不漂移）");
            Assert.AreEqual(5, (int)SkillId.Fireball, "火球术=5");
            Assert.AreEqual(6, (int)SkillId.Count);
        }

        [Test]
        public void SkillTags_MatchGolden_AllFive()
        {
            foreach (var pair in SkillTagGolden.Masks)
                Assert.AreEqual(pair.Value, SkillTags.Of(pair.Key), pair.Key + " Tag mask 与 golden 不一致");
            Assert.AreEqual(SkillTagGolden.Masks.Count, SkillTagGolden.All.Length, "All 集必须覆盖全部技能");
        }

        [Test]
        public void IceSpear_IsFastPiercingProjectileSpell()
        {
            SkillDef def = SkillCatalog.Get(SkillId.IceSpear);
            Assert.AreEqual(SkillId.IceSpear, def.Id);
            Assert.Greater(def.Pierce, 0, "冰矛必须穿透");
            Assert.Greater(def.ProjectileSpeed, SkillCatalog.Get(SkillId.Projectile).ProjectileSpeed,
                "冰矛弹速必须快于基础弹道");
            Assert.Less(def.ProjectileRadius, SkillCatalog.Get(SkillId.Projectile).ProjectileRadius,
                "冰矛弹径必须细于基础弹道");
            Assert.IsFalse(def.BaseDamageIsFire, "冰矛基础伤害不是火焰（引擎无冰冷轴，走物理轴）");
            Assert.IsTrue(SliceSession.SkillHotkey(SkillId.IceSpear) == "R");
            Assert.AreEqual("冰矛", SliceSession.SkillDisplayName(SkillId.IceSpear));
        }

        [Test]
        public void Fireball_IsFireProjectileWithImpactExplosion()
        {
            SkillDef def = SkillCatalog.Get(SkillId.Fireball);
            Assert.AreEqual(SkillId.Fireball, def.Id);
            Assert.IsTrue(def.BaseDamageIsFire, "火球术基础伤害必须是火焰（不虚构物理分量）");
            Assert.Greater(def.ImpactAreaRadius, 0f, "火球术必须有命中点爆炸");
            Assert.IsTrue((SkillTags.Of(SkillId.Fireball) & Tag.Area) != 0, "火球术必须带范围 Tag");
            Assert.IsTrue((SkillTags.Of(SkillId.Fireball) & Tag.Fire) != 0);
            Assert.IsFalse((SkillTags.Of(SkillId.Fireball) & Tag.Physical) != 0, "火球术不得带物理 Tag");
            Assert.IsTrue(SliceSession.SkillHotkey(SkillId.Fireball) == "T");
            Assert.AreEqual("火球术", SliceSession.SkillDisplayName(SkillId.Fireball));
        }

        [Test]
        public void NewSkills_ShareProjectileConnectionGroup()
        {
            var s = new SliceSession();
            Assert.AreSame(s.WSupports, s.SupportsOf(SkillId.IceSpear), "冰矛必须与弹道同连接组");
            Assert.AreSame(s.WSupports, s.SupportsOf(SkillId.Fireball), "火球术必须与弹道同连接组");
            Assert.AreEqual(SkillId.Projectile, SliceSession.ConnectionGroupRep(SkillId.IceSpear));
            Assert.AreEqual(SkillId.Projectile, SliceSession.ConnectionGroupRep(SkillId.Fireball));
            Assert.AreEqual(SkillId.Melee, SliceSession.ConnectionGroupRep(SkillId.Melee));
            // 组内共享：装配在弹道上的辅助，同组技能必须读到同一真值
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.ReturningProjectiles, out err), err);
            Assert.IsTrue(s.HasSupport(SkillId.IceSpear, SupportId.ReturningProjectiles), "同组技能必须共享辅助");
            Assert.IsTrue(s.HasSupport(SkillId.Fireball, SupportId.ReturningProjectiles));
            Assert.IsFalse(s.HasSupport(SkillId.Melee, SupportId.ReturningProjectiles), "跨组不得串味");
        }

        // ===================== ③ 新辅助身份与兼容 =====================

        [Test]
        public void SupportIdentity_StableNumericIds()
        {
            Assert.AreEqual(8, (int)SupportId.ReturningProjectiles);
            Assert.AreEqual(9, (int)SupportId.SnipersMark);
            Assert.AreEqual(10, (int)SupportId.Count);
            Assert.AreEqual("投射物返回", SupportCatalog.Get(SupportId.ReturningProjectiles).Name);
            Assert.AreEqual("狙击印记", SupportCatalog.Get(SupportId.SnipersMark).Name);
        }

        [Test]
        public void NewSupports_OnlyProjectileDelivery()
        {
            foreach (var support in new[] { SupportId.ReturningProjectiles, SupportId.SnipersMark })
            {
                Assert.IsTrue(support == SupportId.ReturningProjectiles || support == SupportId.SnipersMark);
                Assert.IsFalse(SliceSession.IsSupportCompatible(support, SkillId.Melee), "近战不得接 " + support);
                Assert.IsFalse(SliceSession.IsSupportCompatible(support, SkillId.Area), "范围不得接 " + support);
                Assert.IsTrue(SliceSession.IsSupportCompatible(support, SkillId.Projectile));
                Assert.IsTrue(SliceSession.IsSupportCompatible(support, SkillId.IceSpear));
                Assert.IsTrue(SliceSession.IsSupportCompatible(support, SkillId.Fireball));
            }
        }

        [Test]
        public void NewSupports_MechanismFlag_AndNoFabricatedStatAxis()
        {
            foreach (var support in new[] { SupportId.ReturningProjectiles, SupportId.SnipersMark })
            {
                SupportDef d = SupportCatalog.Get(support);
                Assert.IsTrue(d.ChangesMechanism, support + " 必须标记为机制型");
                Assert.AreEqual(SkillId.Projectile, d.MechanicSkill);
                Assert.AreEqual(EffectId.None, d.TriggerEffect, "不得为此新增 Effect");
                Assert.IsNotNull(d.Mods);
                Assert.AreEqual(0, d.Mods.Length, "纯机制 Support 不得虚构 Stat 轴");
                Assert.IsFalse(string.IsNullOrEmpty(d.Desc));
            }
            // 数值唯一真相源=SliceRules（Desc 由它派生，防两处数字漂移）
            Assert.IsTrue(SupportCatalog.Get(SupportId.SnipersMark).Desc.Contains("35%"),
                "狙击印记 Desc 必须由 SliceRules.SnipersMarkMoreDamage 派生");
        }

        [Test]
        public void Fireball_TakesAreaSupport_IceSpearDoesNot()
        {
            Assert.IsTrue(SliceSession.IsSupportCompatible(SupportId.Concentrated, SkillId.Fireball),
                "火球术有范围分量，应可接集中");
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.Concentrated, SkillId.IceSpear),
                "冰矛无范围分量，不得接集中");
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.FireConversion, SkillId.Fireball),
                "火焰转化需 Attack+Physical，火球术二者皆无");
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.FireConversion, SkillId.IceSpear),
                "火焰转化需 Attack，冰矛是无 Attack 的法术");
        }

        // ===================== ④ 投射物返回行为 =====================

        static ArenaSim NewSim(uint seed)
        {
            var sim = new ArenaSim();
            sim.Reset();
            var session = new SliceSession();
            session.ResetTown(seed);
            sim.Session = session;
            return sim;
        }

        static void KillDummiesFarAway(ArenaSim sim)
        {
            // 隔离：把木桩挪到射程外，避免「掉头」断言被命中分支干扰
            for (int i = 0; i < sim.Dummies.Items.Length; i++)
                sim.Dummies.Items[i] = default;
            sim.Dummies.OccupiedCount = 0;
            sim.Dummies.AliveCount = 0;
        }

        [Test]
        public void ReturningProjectile_TurnsAroundAtMaxRange_AndDiesAtPlayer()
        {
            var sim = NewSim(7u);
            KillDummiesFarAway(sim);
            sim.Player.X = 0f;
            sim.Player.Z = 0f;

            SkillDef def = SkillCatalog.Get(SkillId.IceSpear);
            def.ProjectileSpeed = 10f;
            def.ProjectileMaxDistance = 3f;
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, default, false, 0, false,
                SkillId.IceSpear, false, 0, true);
            Assert.GreaterOrEqual(idx, 0);
            Assert.AreEqual(1, sim.Projectiles.AliveCount);

            for (int i = 0; i < 20 && !sim.Projectiles.Items[idx].Returning; i++)
                sim.Projectiles.Tick(0.1f, sim.Dummies, sim.Feedback, sim);

            Projectile p = sim.Projectiles.Items[idx];
            Assert.IsTrue(p.Alive, "到射程尽头必须掉头而不是消失");
            Assert.IsTrue(p.Returning, "到射程尽头必须进入返程");
            Assert.Less(p.DirX, 0f, "返程必须朝玩家方向");

            for (int i = 0; i < 60 && sim.Projectiles.Items[idx].Alive; i++)
                sim.Projectiles.Tick(0.1f, sim.Dummies, sim.Feedback, sim);
            Assert.IsFalse(sim.Projectiles.Items[idx].Alive, "返程到达玩家后必须消失");
            Assert.AreEqual(0, sim.Projectiles.AliveCount);
        }

        [Test]
        public void ReturningProjectile_WithoutSupport_DiesAtMaxRange()
        {
            var sim = NewSim(7u);
            KillDummiesFarAway(sim);
            SkillDef def = SkillCatalog.Get(SkillId.Projectile);
            def.ProjectileSpeed = 10f;
            def.ProjectileMaxDistance = 3f;
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, default, false, 0, false,
                SkillId.Projectile, false, 0, false);
            for (int i = 0; i < 20 && sim.Projectiles.Items[idx].Alive; i++)
                sim.Projectiles.Tick(0.1f, sim.Dummies, sim.Feedback, sim);
            Assert.IsFalse(sim.Projectiles.Items[idx].Alive, "无返回支持：射程尽头消失（既有行为不变）");
        }

        [Test]
        public void PiercingProjectile_HitsEachEnemyOnce_AndKeepsFlying()
        {
            var sim = NewSim(11u);
            sim.Dummies.Clear();
            sim.Dummies.Items[0].Occupied = true;
            sim.Dummies.Items[0].Alive = true;
            sim.Dummies.Items[0].Hp = 100000;
            sim.Dummies.Items[0].MaxHp = 100000;
            sim.Dummies.Items[0].X = 2f;
            sim.Dummies.Items[0].Z = 0f;
            sim.Dummies.OccupiedCount = 1;
            sim.Dummies.AliveCount = 1;

            SkillDef def = SkillCatalog.Get(SkillId.IceSpear);
            def.ProjectileSpeed = 10f;
            def.ProjectileMaxDistance = 6f;
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, default, false, 0, false,
                SkillId.IceSpear, false, 3, false);

            int hits = 0;
            for (int i = 0; i < 40 && sim.Projectiles.Items[idx].Alive; i++)
            {
                sim.Projectiles.Tick(0.1f, sim.Dummies, sim.Feedback, sim);
                hits = sim.Dummies.HitEvents;
            }

            Assert.AreEqual(1, hits, "同一目标在一次穿透飞行中只能结算一次");
            Assert.Less(sim.Dummies.Items[0].Hp, 100000, "穿透命中必须造成伤害");
        }

        [Test]
        public void PiercingProjectile_HitsTwoEnemiesInLine()
        {
            var sim = NewSim(13u);
            sim.Dummies.Clear();
            for (int k = 0; k < 2; k++)
            {
                sim.Dummies.Items[k].Occupied = true;
                sim.Dummies.Items[k].Alive = true;
                sim.Dummies.Items[k].Hp = 100000;
                sim.Dummies.Items[k].MaxHp = 100000;
                sim.Dummies.Items[k].X = 1.5f + k * 1.5f;
                sim.Dummies.Items[k].Z = 0f;
            }
            sim.Dummies.OccupiedCount = 2;
            sim.Dummies.AliveCount = 2;

            SkillDef def = SkillCatalog.Get(SkillId.IceSpear);
            def.ProjectileSpeed = 10f;
            def.ProjectileMaxDistance = 6f;
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, default, false, 0, false,
                SkillId.IceSpear, false, 3, false);
            for (int i = 0; i < 40 && sim.Projectiles.Items[idx].Alive; i++)
                sim.Projectiles.Tick(0.1f, sim.Dummies, sim.Feedback, sim);

            Assert.AreEqual(2, sim.Dummies.HitEvents, "穿透必须依次结算两个目标");
        }

        // ===================== ⑤ 狙击印记行为 =====================

        [Test]
        public void SnipersMark_AppliedOnProjectileHit_AndDecays()
        {
            var sim = NewSim(17u);
            sim.Dummies.Clear();
            sim.Dummies.Items[0].Occupied = true;
            sim.Dummies.Items[0].Alive = true;
            sim.Dummies.Items[0].Hp = 100000;
            sim.Dummies.Items[0].MaxHp = 100000;
            sim.Dummies.Items[0].X = 1f;
            sim.Dummies.Items[0].Z = 0f;
            sim.Dummies.OccupiedCount = 1;
            sim.Dummies.AliveCount = 1;

            sim.Session.WSupports[0] = SupportId.SnipersMark;
            Dummy probe = sim.Dummies.Items[0];
            HitRequest req = sim.Session.BuildPlayerHit(SkillId.Projectile, probe);
            SkillDef def = SkillCatalog.Get(SkillId.Projectile);
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, req, true, 0, false, SkillId.Projectile, false);
            Assert.GreaterOrEqual(idx, 0);
            sim.ResolveProjectileHit(idx, 0);

            Assert.Greater(sim.Dummies.Items[0].MarkRemain, 0f, "命中必须施加印记");
            Assert.AreEqual(SliceRules.SnipersMarkDuration, sim.Dummies.Items[0].MarkRemain, 1e-4f);

            sim.Dummies.TickMarks(1f);
            Assert.AreEqual(SliceRules.SnipersMarkDuration - 1f, sim.Dummies.Items[0].MarkRemain, 1e-4f);
            sim.Dummies.TickMarks(999f);
            Assert.AreEqual(0f, sim.Dummies.Items[0].MarkRemain, 1e-4f);
        }

        [Test]
        public void SnipersMark_OnlyHitsTarget_NotBystanders()
        {
            var sim = NewSim(19u);
            sim.Dummies.Clear();
            for (int k = 0; k < 2; k++)
            {
                sim.Dummies.Items[k].Occupied = true;
                sim.Dummies.Items[k].Alive = true;
                sim.Dummies.Items[k].Hp = 100000;
                sim.Dummies.Items[k].MaxHp = 100000;
                sim.Dummies.Items[k].X = 1f + k * 3f;
                sim.Dummies.Items[k].Z = 0f;
            }
            sim.Dummies.OccupiedCount = 2;
            sim.Dummies.AliveCount = 2;
            sim.Session.WSupports[0] = SupportId.SnipersMark;

            HitRequest req = sim.Session.BuildPlayerHit(SkillId.Projectile, sim.Dummies.Items[0]);
            SkillDef def = SkillCatalog.Get(SkillId.Projectile);
            int idx = sim.Projectiles.Spawn(0f, 0f, 1f, 0f, def, req, true, 0, false, SkillId.Projectile, false);
            sim.ResolveProjectileHit(idx, 0);

            Assert.Greater(sim.Dummies.Items[0].MarkRemain, 0f);
            Assert.AreEqual(0f, sim.Dummies.Items[1].MarkRemain, "印记是单体：旁观者不得被标记");
        }

        [Test]
        public void SnipersMark_RaisesProjectileDamage_OnMarkedTarget()
        {
            // 同一会话、同一自造 HitRequest（唯一差异=目标是否已带印记）：RNG 消耗模式完全一致，
            // 掷值逐一相同，因此伤害差只能来自印记倍率（确定性，非统计）。
            int plain = DamageOfOneHit(false);
            int marked = DamageOfOneHit(true);
            Assert.Greater(marked, plain,
                "被印记目标承受的投射物伤害必须更高（未印记=" + plain + "，印记=" + marked + "）");
        }

        /// <summary>单次投射物命中伤害（自造 packet：基础 100、无 inc/more/crit，排除掷值干扰）。</summary>
        static int DamageOfOneHit(bool preMarked)
        {
            var sim = NewSim(23u);
            sim.Dummies.Clear();
            sim.Dummies.Items[0].Occupied = true;
            sim.Dummies.Items[0].Alive = true;
            sim.Dummies.Items[0].Hp = 10000000;
            sim.Dummies.Items[0].MaxHp = 10000000;
            sim.Dummies.Items[0].X = 1f;
            sim.Dummies.Items[0].Z = 0f;
            sim.Dummies.OccupiedCount = 1;
            sim.Dummies.AliveCount = 1;
            sim.Session.WSupports[0] = SupportId.SnipersMark;
            if (preMarked)
                sim.Dummies.ApplyMark(0, SliceRules.SnipersMarkDuration);

            HitRequest req = default;
            req.PhysFlat = 100f;
            req.MoreDamage = 1f;
            req.MorePhys = 1f;
            req.MoreFire = 1f;
            req.Accuracy = 1000f;
            req.Skill = SkillId.Projectile;
            req.Tags = SkillTags.Of(SkillId.Projectile);

            SkillDef def = SkillCatalog.Get(SkillId.Projectile);
            int idx = sim.Projectiles.Spawn(1f, 0f, 1f, 0f, def, req, true, 0, false, SkillId.Projectile, false);
            sim.ResolveProjectileHit(idx, 0);
            return sim.Session.LastDamageDealt;
        }

        [Test]
        public void Fireball_ImpactExplosion_DamagesTargetsAroundImpactOnly()
        {
            var sim = NewSim(29u);
            sim.Dummies.Clear();
            // 目标 0 被直接命中；目标 1/2 在爆炸半径内；目标 3 在半径外
            float[] xs = { 1f, 1.6f, 2.4f, 12f };
            for (int k = 0; k < xs.Length; k++)
            {
                sim.Dummies.Items[k].Occupied = true;
                sim.Dummies.Items[k].Alive = true;
                sim.Dummies.Items[k].Hp = 100000;
                sim.Dummies.Items[k].MaxHp = 100000;
                sim.Dummies.Items[k].X = xs[k];
                sim.Dummies.Items[k].Z = 0f;
            }
            sim.Dummies.OccupiedCount = xs.Length;
            sim.Dummies.AliveCount = xs.Length;

            SkillDef def = SkillCatalog.Get(SkillId.Fireball);
            HitRequest req = sim.Session.BuildPlayerHit(SkillId.Fireball, sim.Dummies.Items[0]);
            int idx = sim.Projectiles.Spawn(1f, 0f, 1f, 0f, def, req, true, 0, false, SkillId.Fireball, true);
            Assert.Greater(sim.Projectiles.Items[idx].ImpactAreaRadius, 0f, "火球术投射物必须携带命中点爆炸半径");
            sim.ResolveProjectileHit(idx, 0);

            Assert.Less(sim.Dummies.Items[1].Hp, 100000, "爆炸半径内目标必须受伤");
            Assert.AreEqual(100000, sim.Dummies.Items[3].Hp, "爆炸半径外目标不得受伤");
        }

        [Test]
        public void MarkFeedbackState_LowestPriority_AndThreeArgOverloadUnchanged()
        {
            // 新状态优先级最低：被印记是持续态，不得盖过命中/点燃
            Assert.AreEqual(EnemyFeedbackState.Hit, EnemyVisualFeedback.Compute(true, 0.5f, true, true));
            Assert.AreEqual(EnemyFeedbackState.Ignite, EnemyVisualFeedback.Compute(true, 0f, true, true));
            Assert.AreEqual(EnemyFeedbackState.Mark, EnemyVisualFeedback.Compute(true, 0f, false, true));
            Assert.AreEqual(EnemyFeedbackState.Normal, EnemyVisualFeedback.Compute(true, 0f, false, false));
            Assert.AreEqual(EnemyFeedbackState.Normal, EnemyVisualFeedback.Compute(false, 0.5f, true, true), "死亡恒 Normal");
            // 3 参重载=既有行为（无印记概念）：逐位不变
            Assert.AreEqual(EnemyFeedbackState.Hit, EnemyVisualFeedback.Compute(true, 0.5f, true));
            Assert.AreEqual(EnemyFeedbackState.Ignite, EnemyVisualFeedback.Compute(true, 0f, true));
            Assert.AreEqual(EnemyFeedbackState.Normal, EnemyVisualFeedback.Compute(true, 0f, false));
        }

        [Test]
        public void Fireball_BaseDamage_IsFireOnly()
        {
            var sim = NewSim(31u);
            HitRequest req = sim.Session.BuildPlayerHit(SkillId.Fireball, default);
            Assert.Greater(req.FireFlat, 0f, "火球术基础伤害必须是火焰");
            Assert.AreEqual(0f, req.PhysFlat, "火球术不得有物理基础伤害");
            Assert.IsFalse(req.IsAttack, "火球术是法术（不吃命中/闪避判定）");

            HitRequest ice = sim.Session.BuildPlayerHit(SkillId.IceSpear, default);
            Assert.Greater(ice.PhysFlat, 0f, "冰矛基础伤害走物理轴（引擎无冰冷轴的已知限制）");
            Assert.IsFalse(ice.IsAttack, "冰矛是法术");
        }
    }
}
