using System.Collections.Generic;
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
            // S6P-WO-05 追加 2 条机制型 Support 后总数 9；本测试只锚定火焰转化自身的定义不变
            Assert.AreEqual(9, SupportCatalog.Count, "S3-R2 新增 1 条 + S6P-WO-05 新增 2 条 = 9");
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
            // S6P-WO-05 起矩阵为 Active 技能全集 × 真实 Support 全集（5 × 9）
            Assert.AreEqual(SupportCatalog.Count, SupportCompatGolden.Matrix.Count, "golden 必须覆盖全部真实 Support");
            Assert.IsTrue(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Melee));
            Assert.IsTrue(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Projectile));
            Assert.IsFalse(SupportCompatGolden.Contains(SupportId.FireConversion, SkillId.Area));
            // Runtime parity：全组合（含新技能列）——沿用统一入口与 golden 对拍
            foreach (var pair in SupportCompatGolden.Matrix)
                foreach (SkillId skill in SkillTagGolden.All)
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
        public void SupportAndPassiveConversion_ComposeOnSameAxis()
        {
            var s = new SliceSession();
            string err;

            // S6P-WO-04A2（本轮行为更正）：真实转火基石（Avatar of Fire）的词条里含
            // "Deal no Non-Fire Damage" 这类引擎兑现不了的行 ⇒ EffectTruth=UNFULFILLED、整节点 0 效果；
            // 但它**仍是合法路径节点**（TraversalTruth=TRAVERSABLE）—— 04A 的"含 blocked 行即不可分配"
            // 已被本令拆成两个维度：能不能走 ≠ 能不能生效。
            int avatar = FindByStat("Converted to Fire Damage");
            Assert.GreaterOrEqual(avatar, 0, "真实数据里必须存在物理转火的天赋点");
            PassiveSupport.NodeTruth tr = PassiveSupport.EvaluateTruth(avatar);
            Assert.AreEqual(PassiveSupport.EffectTruth.Unfulfilled, tr.Effect,
                "转火基石含当前兑现不了的效果行 ⇒ 效果维度必须是 UNFULFILLED");
            Assert.AreEqual(PassiveSupport.TraversalTruth.Traversable, tr.Traversal,
                "普通节点即便效果未兑现也必须是合法路径（本令核心）");

            int unspentBefore = s.Unspent;
            Assert.IsFalse(s.TryAllocate(avatar, out err), "该基石离起点很远，分配门应报相连性");
            Assert.AreEqual("需与已点亮节点相连", err, "拒绝原因必须来自通行维度，不再是支持门");
            Assert.AreEqual(unspentBefore, s.Unspent, "被拒绝的加点不得扣点");

            // 效果门：即便把它塞进损坏/注入状态的 Allocated，它也**一条 modifier 都不许出**。
            float convertBefore = s.PlayerStats.Get(StatId.ConvertPhysToFire);
            s.Allocated[avatar] = true;
            s.RecalcPlayer(false);
            Assert.AreEqual(convertBefore, s.PlayerStats.Get(StatId.ConvertPhysToFire), 0.0001f,
                "route-only 节点必须整节点零效果（不得只吃它可识别的那一条转火行）");
            s.Allocated[avatar] = false;

            // 同一条转换轴上的聚合仍然成立：取**权威 parser** 对该节点真实文本的产出，
            // 与 Support 的 modifier 一起走同一个生产 StatBag 聚合路径（不复制任何解释器）。
            Modifier[] passive = PoeStatParser.ParseCached(PoeTree.Get(avatar).stats);
            Assert.AreEqual(1, passive.Length, "该文本必须映射出唯一一条转换 modifier");
            Assert.AreEqual(StatId.ConvertPhysToFire, passive[0].Stat);
            Assert.AreEqual(0.50f, passive[0].Value, 0.0001f);

            var bag = new StatBag();
            Tag tags = SkillTags.Of(SkillId.Melee);
            bag.AddAll(passive, tags, ConditionId.Always);
            bag.AddAll(SupportCatalog.Get(SupportId.FireConversion).Mods, tags, ConditionId.Always);
            // 0.50（Avatar of Fire）+ 0.50（火焰转化）经同一 StatBag 轴自然聚合，无专属叠加规则
            Assert.AreEqual(1.00f, bag.Get(StatId.ConvertPhysToFire), 0.0001f);
        }

        /// <summary>真实天赋域按索引找第一个含指定词条的节点。</summary>
        static int FindByStat(string needle)
        {
            var nodes = PoeTree.Nodes;
            if (nodes == null)
                return -1;
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].locked == 0 && !string.IsNullOrEmpty(nodes[i].stats) && nodes[i].stats.Contains(needle))
                    return i;
            return -1;
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
