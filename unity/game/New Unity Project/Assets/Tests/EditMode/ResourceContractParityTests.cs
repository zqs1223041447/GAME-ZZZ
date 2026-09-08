using System.Collections.Generic;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3-M2-RESOURCE-CONTRACT-TRUTH：Runtime declared keys × Audit 契约 parity。
    /// 边界：路径拼接共享 RuntimeResourcePaths（如何加载）；期望键集/分类为独立 oracle（人工声明，
    /// 不从 Runtime declared keys 自动生成）——Runtime 未审批增删资源入口时本文件必须变红。
    /// </summary>
    public sealed class ResourceContractParityTests
    {
        // 独立 golden 期望（人工钉死，不从 Runtime 生成）
        static readonly string[] ExpectedSfxKeys = { "Cast", "Impact", "Hit", "Death", "Loot" };
        static readonly string[] ExpectedVoiceKeys = { "Cast", "Hit", "Death" };

        /// <summary>覆盖校验（纯函数，供负向测试合成输入）：返回 null=恰好一致，否则为原因。</summary>
        internal static string ValidateCoverage(IReadOnlyList<string> runtimeKeys, IReadOnlyList<string> auditKeys)
        {
            var runtimeSet = new HashSet<string>();
            foreach (var k in runtimeKeys)
                if (!runtimeSet.Add(k))
                    return "Runtime declared key 重复：" + k;
            var auditSet = new HashSet<string>();
            foreach (var k in auditKeys)
                if (!auditSet.Add(k))
                    return "Audit 期望 key 重复：" + k;
            var missing = new List<string>();
            var extra = new List<string>();
            foreach (var k in auditSet)
                if (!runtimeSet.Contains(k))
                    missing.Add(k);
            foreach (var k in runtimeSet)
                if (!auditSet.Contains(k))
                    extra.Add(k);
            if (missing.Count > 0)
                return "Audit 缺契约（Runtime 已声明但审计未批准）：" + string.Join(", ", missing.ToArray());
            if (extra.Count > 0)
                return "Audit 多契约（审计批准但 Runtime 未声明）：" + string.Join(", ", extra.ToArray());
            return null;
        }

        [Test]
        public void RuntimePathContract_ProducesExactKeys()
        {
            Assert.AreEqual("Player/DarkKnight", RuntimeResourcePaths.PlayerDarkKnight);
            Assert.AreEqual("Enemies/TrollWarriorVisual", RuntimeResourcePaths.EnemyTrollVisual);
            Assert.AreEqual("Audio/Cast", RuntimeResourcePaths.CombatSfx("Cast"));
            Assert.AreEqual("Audio/Loot", RuntimeResourcePaths.CombatSfx("Loot"));
            Assert.AreEqual("Audio/Voice/Cast", RuntimeResourcePaths.Voice("Cast"));
            Assert.AreEqual("Audio/Voice/Death", RuntimeResourcePaths.Voice("Death"));
        }

        [Test]
        public void AudioEvents_DeclaredKeys_MatchGolden()
        {
            Assert.IsNull(ValidateCoverage(AudioEvents.DeclaredKeys, ExpectedSfxKeys),
                "AudioEvents declared keys 与审计 golden 期望不一致");
            Assert.AreEqual(5, AudioEvents.DeclaredKeys.Count);
        }

        [Test]
        public void VoiceCues_DeclaredKeys_MatchGolden()
        {
            Assert.IsNull(ValidateCoverage(VoiceCues.DeclaredKeys, ExpectedVoiceKeys),
                "VoiceCues declared keys 与审计 golden 期望不一致");
            Assert.AreEqual(3, VoiceCues.DeclaredKeys.Count);
        }

        [Test]
        public void AuditContracts_ExactCoverage_NoMissingExtraDuplicate()
        {
            var sfxExpected = new List<string>();
            var voiceExpected = new List<string>();
            int playerCount = 0;
            int enemyCount = 0;
            foreach (var c in ContentResourceAuditContracts.All)
            {
                if (c.Domain == ResourceDomain.CombatSfx)
                    sfxExpected.Add(c.LogicalKey);
                else if (c.Domain == ResourceDomain.Voice)
                    voiceExpected.Add(c.LogicalKey);
                else if (c.Domain == ResourceDomain.Enemy)
                {
                    enemyCount++;
                    Assert.AreEqual(ResourceClass.Required, c.Class);
                    Assert.AreEqual(RuntimeResourcePaths.EnemyTrollVisual, c.Key);
                }
                else
                {
                    playerCount++;
                    Assert.AreEqual(ResourceClass.Required, c.Class);
                    Assert.AreEqual("Player/DarkKnight", c.Key);
                }
            }
            Assert.AreEqual(1, playerCount, "玩家预制体契约必须恰好 1 条");
            Assert.AreEqual(1, enemyCount, "敌人视觉预制体契约必须恰好 1 条（S3-P5-ART-R3）");
            Assert.IsNull(ValidateCoverage(AudioEvents.DeclaredKeys, sfxExpected), "战斗 SFX 契约覆盖不一致");
            Assert.IsNull(ValidateCoverage(VoiceCues.DeclaredKeys, voiceExpected), "人声契约覆盖不一致");
        }

        [Test]
        public void CoverageValidator_NegativeCases_AreRejected()
        {
            // Negative A — Runtime 声明了审计未批准的 key（如私加 Taunt）：必须红
            Assert.IsNotNull(ValidateCoverage(new[] { "Cast", "Taunt" }, new[] { "Cast" }));
            // Negative B — 审计批准了 Runtime 未声明的 key（如 Loot 被删）：必须红
            Assert.IsNotNull(ValidateCoverage(new[] { "Cast" }, new[] { "Cast", "Loot" }));
            // Negative C — 重复：必须红
            Assert.IsNotNull(ValidateCoverage(new[] { "Cast", "Cast" }, new[] { "Cast" }));
            Assert.IsNotNull(ValidateCoverage(new[] { "Cast" }, new[] { "Cast", "Cast" }));
            // 正例对照
            Assert.IsNull(ValidateCoverage(ExpectedSfxKeys, ExpectedSfxKeys));
        }
    }
}
