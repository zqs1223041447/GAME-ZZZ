using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3-B1-RCLOSE：Runtime Support 兼容门测试。
    /// golden 期望取自 SupportCompatGolden（独立 oracle）；本文件验证 Runtime 判定/行为与其一致，
    /// 不修改 golden——Runtime 被意外放宽时这里必须变红。
    /// </summary>
    public sealed class SupportGateTests
    {
        [Test]
        public void RuntimeMatrix_ParityWithGolden()
        {
            foreach (var pair in SupportCompatGolden.Matrix)
            {
                foreach (SkillId skill in SkillTagGolden.All)
                {
                    bool expected = SupportCompatGolden.Contains(pair.Key, skill);
                    bool actual = SliceSession.IsSupportCompatible(pair.Key, skill);
                    Assert.AreEqual(expected, actual,
                        "Runtime 与 golden 矩阵不一致：" + pair.Key + " × " + skill);
                }
            }
        }

        [Test]
        public void InvalidAttach_Rejected_NoWrites()
        {
            var s = new SliceSession();
            string err;
            // 集中（Tag.Area 路径）对近战非法；分裂（机制路径）对近战非法
            Assert.IsFalse(s.TrySetSupport(SkillId.Melee, 0, SupportId.Concentrated, out err));
            Assert.IsFalse(string.IsNullOrEmpty(err), err);
            Assert.AreEqual(SupportId.None, s.QSupports[0], "非法连接不得写入孔位");
            Assert.IsFalse(s.TrySetSupport(SkillId.Melee, 0, SupportId.Fork, out err));
            Assert.AreEqual(SupportId.None, s.QSupports[0], "非法连接不得写入孔位");
            // 其它技能孔不受影响
            Assert.AreEqual(SupportId.None, s.WSupports[0]);
            Assert.AreEqual(SupportId.None, s.ESupports[0]);
            // 非法判定入口本身与 golden 一致
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.Concentrated, SkillId.Melee));
            Assert.IsFalse(SliceSession.IsSupportCompatible(SupportId.Fork, SkillId.Melee));
        }

        [Test]
        public void FailedReplacement_PreservesOldState()
        {
            var s = new SliceSession();
            string err;
            Assert.IsTrue(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Fork, out err), err);
            Assert.AreEqual(SupportId.Fork, s.WSupports[0]);

            // 用非法组合替换同一孔位：必须失败且原 Support 原样保留（防"先写后校验"）
            Assert.IsFalse(s.TrySetSupport(SkillId.Projectile, 0, SupportId.Concentrated, out err));
            Assert.AreEqual(SupportId.Fork, s.WSupports[0], "失败的替换不得改变已有合法连接");
        }

        [Test]
        public void ValidCombinations_Unchanged()
        {
            var s = new SliceSession();
            string err;
            // 数值 Support：通用合法
            Assert.IsTrue(s.TrySetSupport(SkillId.Melee, 0, SupportId.AddedFire, out err), err);
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Faster, out err), err);
            // 机制 Support：集中只接范围、分裂只接弹道
            Assert.IsTrue(s.TrySetSupport(SkillId.Area, 0, SupportId.Concentrated, out err), err);
            Assert.AreEqual(SupportId.Concentrated, s.ESupports[0]);
            // 运行时判定与 golden 一致（合法组合全绿）
            Assert.IsTrue(SliceSession.IsSupportCompatible(SupportId.Fork, SkillId.Projectile));
            Assert.IsTrue(SliceSession.IsSupportCompatible(SupportId.Combustion, SkillId.Melee));
        }
    }
}
