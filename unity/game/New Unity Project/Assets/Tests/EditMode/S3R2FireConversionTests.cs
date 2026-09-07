using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3-R2-FIRE-CONVERSION：转换型 Support「火焰转化」行为契约。
    /// 证明机制型内容仅靠已有 Tag → Modifier → StatBag → HitRequest → CombatMath 数据链进入游戏：
    /// 无新 Effect/Stat/Trigger，无 Support 专用 Runtime 分支；与烬心共享同一转换轴自然聚合。
    /// </summary>
    public sealed class S3R2FireConversionTests
    {
        [Test]
        public void CatalogDefinition_SingleConversionSupport()
        {
            Assert.AreEqual(7, SupportCatalog.Count, "本轮仅新增 1 个 Support");
            SupportDef def = SupportCatalog.Get(SupportId.FireConversion);
            Assert.AreEqual(SupportId.FireConversion, def.Id);
            Assert.AreEqual("火焰转化", def.Name);
            Assert.IsTrue(def.ChangesMechanism);
            Assert.AreEqual(SkillId.None, def.MechanicSkill, "兼容性必须由 Tag 路径推导，不得绑死技能");
            Assert.AreEqual(EffectId.None, def.TriggerEffect, "不得使用 Trigger");
            Assert.AreEqual(EventId.OnHit, def.TriggerEvent); // 未用触发时字段默认值，无 Effect 即不进 TriggerSystem
            Assert.IsNotNull(def.Mods);
            Assert.AreEqual(1, def.Mods.Length);
            Assert.AreEqual(StatId.ConvertPhysToFire, def.Mods[0].Stat);
            Assert.AreEqual(ModOp.Flat, def.Mods[0].Op);
            Assert.AreEqual(0.50f, def.Mods[0].Value, 0.0001f);
            Assert.AreEqual(Tag.Attack | Tag.Hit | Tag.Physical, def.Mods[0].RequiredTags);
        }

        [Test]
        public void GoldenMatrix_ThreeBySeven_FireConversionPinned()
        {
            Assert.AreEqual(7, SupportCompatGolden.Matrix.Count, "golden 必须 3×7 全覆盖");
            Assert.IsTrue(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Melee));
            Assert.IsTrue(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Projectile));
            Assert.IsFalse(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Area));
            // Runtime parity：21 组合（含新列）——沿用统一入口与 golden 对拍
            foreach (var pair in SupportCompatGolden.Matrix)
                foreach (SkillId skill in new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area })
                    Assert.AreEqual(SupportCompatGolden.Contains(pair.Key, skill),
                        SliceSession.IsSupportCompatible(pair.Key, skill),
                        "Runtime 与 golden 不一致：" + pair.Key + " × " + skill);
        }

        [Test]
        public void RuntimeAttach_MeleeProjectileOk_AreaRejected_NoWrites()
        {
            var s = new SliceSession();
            string err;
            // 既有规则：同一 Support 全局只能装在一处——先装近战验证合法，再卸下装弹道
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.FireConversion, out err), err);
            Assert.AreEqual(SupportId.FireConversion, s.QSupports[0]);
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.None, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.FireConversion, out err), err);
            Assert.AreEqual(SupportId.FireConversion, s.WSupports[0]);

            // 范围（Spell）不满足 Attack|Hit|Physical → 拒绝且无半写入
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Concentrated, out err), err); // 先装合法 Support
            Assert.IsFalse(s.TrySetSupport(SkillId.Area, 0, SupportId.FireConversion, out err));
            Assert.AreEqual(SupportId.Concentrated, s.ESupports[0], "失败连接不得改变已有合法 Support");
            Assert.IsFalse(string.IsNullOrEmpty(err), err);
        }

        [Test]
        public void StatAggregation_SupportAddsExactlyHalfConversion()
        {
            var baseline = new SliceSession();
            var bag = new StatBag();
            baseline.CollectSkillMods(SkillId.Melee, bag);
            float before = bag.Get(StatId.ConvertPhysToFire);
            Assert.AreEqual(0f, before, 0.0001f, "基线（无烬心无 Support）转换为 0");

            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.FireConversion, out err), err);
            var bag2 = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bag2);
            Assert.AreEqual(before + 0.50f, bag2.Get(StatId.ConvertPhysToFire), 0.0001f);
        }

        [Test]
        public void SupportAndCinderHeart_ComposeNinetyPercent()
        {
            var s = new SliceSession();
            string err;
            // 烬心（节点 13）需要路径点亮：0 → 3 → 8 → 13
            Assert.IsTrue(s.TryAllocate(3, out err), err);
            Assert.IsTrue(s.TryAllocate(8, out err), err);
            Assert.IsTrue(s.TryAllocate(13, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.FireConversion, out err), err);

            var bag = new StatBag();
            s.CollectSkillMods(SkillId.Melee, bag);
            // 0.40（烬心）+ 0.50（火焰转化）经同一 StatBag 轴自然聚合，无专属叠加规则
            Assert.AreEqual(0.90f, bag.Get(StatId.ConvertPhysToFire), 0.0001f);
        }

        [Test]
        public void ActualHit_CompositionConvertsPhysToFire()
        {
            var sim = NewSliceSim();
            var s = sim.Session;
            string err;
            // 先连接（城镇有武器孔位），再清空装备隔离掉落词缀噪声
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.FireConversion, out err), err);
            s.InventoryCount = 0;
            for (int i = 0; i < s.Equipped.Length; i++)
                s.Equipped[i] = -1;
            Assert.IsTrue(s.TryEnterMap(sim, out err), err);

            // 结算前稳定观察：转换率 +0.50 进入 HitRequest
            HitRequest conv = s.BuildPlayerHit(SkillId.Melee, default(Dummy));
            Assert.AreEqual(0.50f, conv.ConvertPhysToFire, 0.0001f);

            // 同一固定输入对比结算：物理成分下降、火焰成分从 0 上升（护甲/抗性置零保证可稳定观察）
            HitRequest baseline = conv;
            baseline.ConvertPhysToFire = 0f;
            baseline.HitRoll = conv.HitRoll = 0f;   // 必命中
            baseline.CritRoll = conv.CritRoll = 1f; // 不暴击
            baseline.IgniteRoll = conv.IgniteRoll = 1f;
            baseline.Armour = conv.Armour = 0f;
            baseline.FireRes = conv.FireRes = 0f;

            HitResult rb = CombatMath.ResolveHit(baseline);
            HitResult rc = CombatMath.ResolveHit(conv);
            Assert.Greater(rb.PhysTaken, rc.PhysTaken, "物理成分必须明显下降");
            Assert.AreEqual(0f, rb.FireTaken, 0.0001f, "基线无火焰成分");
            Assert.Greater(rc.FireTaken, 0f, "火焰成分必须实际进入结算");
        }

        static ArenaSim NewSliceSim()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.Session = new SliceSession();
            sim.Caster.Defs = sim.Session.ResolveSkillDef;
            return sim;
        }
    }
}
