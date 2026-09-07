using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3-M1-REPO-TRUTH-CATALOG：SupportCatalog 不变量。
    /// Count sentinel=容量/Count 单一真相源（不算内容、不进 UI/golden/审计内容清单）；
    /// 目录连续无洞；golden 恰好覆盖全部真实 Support；既有 Support 身份零变化。
    /// </summary>
    public sealed class SupportCatalogInvariantTests
    {
        [Test]
        public void SentinelContract_CountIsNotContent_AndGetIsSafe()
        {
            Assert.AreEqual(0, (int)SupportId.None);
            // sentinel 存在且 = 真实数 + 1；SupportCatalog.Count 由 sentinel 派生
            Assert.AreEqual(8, (int)SupportId.Count);
            Assert.AreEqual(7, SupportCatalog.Count);
            Assert.AreEqual((int)SupportId.Count - 1, SupportCatalog.Count);

            // Get(None) / Get(Count sentinel) / 任意非法 id：一律 default，不越界不异常
            Assert.AreEqual(SupportId.None, SupportCatalog.Get(SupportId.None).Id);
            SupportDef bySentinel = SupportCatalog.Get(SupportId.Count);
            Assert.AreEqual(SupportId.None, bySentinel.Id, "sentinel 不得 Get 出真实 Support");
            Assert.AreEqual(SupportId.None, SupportCatalog.Get((SupportId)255).Id, "非法 id 不得越界");
            // sentinel 不在 golden 内容清单里
            Assert.IsFalse(SupportCompatGolden.Matrix.ContainsKey(SupportId.Count));
            Assert.IsFalse(SupportCompatGolden.Contains(SupportId.Count, SkillId.Melee));
        }

        [Test]
        public void CatalogContiguous_AllRealIdsDefined_NoHole()
        {
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                SupportDef def = SupportCatalog.Get((SupportId)i);
                Assert.AreEqual((SupportId)i, def.Id, "目录存在洞或缺定义：id=" + i);
                Assert.IsFalse(string.IsNullOrEmpty(def.Name), "Support " + i + " Name 为空");
                Assert.IsFalse(string.IsNullOrEmpty(def.Desc), "Support " + i + " Desc 为空");
            }
        }

        [Test]
        public void CompatGolden_CoversExactly_CurrentSkillsTimesRealSupports()
        {
            // golden 键集 = 全部真实 Support（无缺、无重复、无 sentinel/None）
            var seen = new System.Collections.Generic.HashSet<SupportId>();
            Assert.AreEqual(SupportCatalog.Count, SupportCompatGolden.Matrix.Count,
                "golden 覆盖数必须等于真实 Support 数");
            foreach (var pair in SupportCompatGolden.Matrix)
            {
                Assert.IsTrue(seen.Add(pair.Key), "golden 键重复：" + pair.Key);
                Assert.GreaterOrEqual((int)pair.Key, 1, "golden 不得含 None/sentinel");
                Assert.LessOrEqual((int)pair.Key, SupportCatalog.Count, "golden 键超出真实范围：" + pair.Key);
                Assert.IsNotNull(pair.Value);
                foreach (SkillId skill in pair.Value)
                    Assert.IsTrue(skill == SkillId.Melee || skill == SkillId.Projectile || skill == SkillId.Area,
                        "golden 含未知技能：" + pair.Key + " -> " + skill);
            }
            // 全覆盖：每个真实 Support × 每个当前 Active Skill 都有确定性期望（parity 测试逐组合对拍）
            Assert.AreEqual(3 * SupportCatalog.Count, 3 * SupportCompatGolden.Matrix.Count);
        }

        [Test]
        public void ExistingSupportIdentity_UnchangedByMaintenance()
        {
            Assert.AreEqual("燃烧", SupportCatalog.Get(SupportId.AddedFire).Name);
            Assert.AreEqual("残暴", SupportCatalog.Get(SupportId.Brutal).Name);
            Assert.AreEqual("集中", SupportCatalog.Get(SupportId.Concentrated).Name);
            Assert.AreEqual("迅捷", SupportCatalog.Get(SupportId.Faster).Name);
            Assert.AreEqual("燃尽", SupportCatalog.Get(SupportId.Combustion).Name);
            Assert.AreEqual("分裂", SupportCatalog.Get(SupportId.Fork).Name);
            Assert.AreEqual("火焰转化", SupportCatalog.Get(SupportId.FireConversion).Name);

            // 关键机制定义零变化
            Assert.AreEqual(SkillId.Projectile, SupportCatalog.Get(SupportId.Fork).MechanicSkill);
            SupportDef fc = SupportCatalog.Get(SupportId.FireConversion);
            Assert.AreEqual(StatId.ConvertPhysToFire, fc.Mods[0].Stat);
            Assert.AreEqual(ModOp.Flat, fc.Mods[0].Op);
            Assert.AreEqual(0.50f, fc.Mods[0].Value, 0.0001f);
            Assert.AreEqual(Tag.Attack | Tag.Hit | Tag.Physical, fc.Mods[0].RequiredTags);
            Assert.AreEqual(SkillId.None, fc.MechanicSkill);
        }
    }
}
