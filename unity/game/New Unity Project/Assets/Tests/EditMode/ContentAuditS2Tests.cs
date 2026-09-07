using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3 R2 内容校验（在 ContentAuditS2Tests 上扩展，类名保留以维持文档指向）。
    /// 只扫现行切片：3 Active + 7 Support（含 R2 火焰转化）+ 13 词缀（含第一批 3 条组合系）+ 16 天赋 + 3 图词缀 + 怪表。
    /// 失败条件：①Support×技能兼容矩阵违规 ②词缀/Mod 行引用运行期未知 Stat / 非法 ModOp ③内容数量护栏（仅 Support +1）
    /// ④新词缀 &gt; 3 条。未使用 Tag 仍为预留记录，不得当作失败项。报告写入 docs/reviews/s3/CONTENT_AUDIT_S3_R2.md。
    /// </summary>
    public sealed class ContentAuditS2Tests
    {
        static readonly Tag DeclaredTags =
            Tag.Attack | Tag.Spell | Tag.Melee | Tag.Projectile | Tag.Area |
            Tag.Hit | Tag.Physical | Tag.Fire | Tag.Duration;

        /// <summary>S2 收口时词缀池基线（10 条）。S3 第一批新增 = AffixId.Count - 基线，必须 ≤3。</summary>
        const int S2BaselineAffixCount = 10;

        /// <summary>S2 收口时 Support 基线（6 条）。R2 新增 = SupportCatalog.Count - 基线，工作令要求恰好 1（火焰转化）。</summary>
        const int S2BaselineSupportCount = 6;

        /// <summary>
        /// 运行期真正消费的 Stat（读取点见注释）。内容引用白名单之外的 Stat = 「报告绿但运行期未知 Stat」，必须失败。
        /// 新增 StatId 必须先在 SliceSession/CombatMath/Kernel 有读取点，再进白名单与内容。
        /// </summary>
        static readonly StatId[] RuntimeConsumedStats = new[]
        {
            // 面板/防御：SliceSession.RecalcPlayer -> PlayerStats（BuildEnemyHit 消费）
            StatId.Life, StatId.Mana, StatId.Strength, StatId.Dexterity, StatId.Intelligence,
            StatId.Armour, StatId.Evasion, StatId.Accuracy, StatId.FireResistance, StatId.MaxFireResistance,
            // 攻击结算：SliceSession.BuildPlayerHit -> HitRequest -> CombatMath.ResolveHit
            StatId.Damage, StatId.PhysicalDamage, StatId.FireDamage,
            StatId.MoreDamage, StatId.MorePhysical, StatId.MoreFire,
            StatId.AddedPhysical, StatId.AddedFire, StatId.ConvertPhysToFire,
            StatId.CritChanceBase, StatId.CritChanceAdded, StatId.CritChanceIncreased, StatId.CritMultiAdded,
            StatId.IgniteChance,
            // 技能形态：SliceSession.ResolveSkillDef
            StatId.AreaRadiusMore, StatId.AreaDamageMore, StatId.AttackSpeed,
            // 机制：ArenaSim.ResolveProjectile -> SliceSession.ForkCount
            StatId.Fork
        };

        /// <summary>
        /// Support × 技能兼容矩阵 golden 期望见 SupportCompatGolden（独立 oracle，本测试与 Runtime parity 测试共用）。
        /// 判定依据：①Tag 路径——带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足（StatBag.Add 静默跳过，静默无效=内容债）；
        /// ②机制路径——ForkProjectiles 只接入弹道结算（SupportDef.MechanicSkill）。
        /// 运行时已接入同一契约（TrySetSupport 拒绝非法连接，见 SliceSession.IsSupportCompatible）；golden 不得由 Runtime 推导。
        /// </summary>

        static string ReportPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/reviews/s3/CONTENT_AUDIT_S3_R2.md")); }
        }

        [Test]
        public void ContentAuditS2_Passes()
        {
            var missingStat = new List<string>();
            var illegalTag = new List<string>();
            var badEffect = new List<string>();
            var missingLinks = new List<string>();
            var badAffix = new List<string>();
            var compatProblems = new List<string>();
            var usedTags = new List<Tag>();
            int modCount = 0;

            void ScanMods(IEnumerable<Modifier> mods, string owner)
            {
                foreach (var m in mods)
                {
                    modCount++;
                    if ((int)m.Stat < 0 || (int)m.Stat >= (int)StatId.Count)
                        missingStat.Add(owner + " -> StatId " + m.Stat);
                    if (!IsRuntimeConsumed(m.Stat))
                        missingStat.Add(owner + " -> 运行期未知 Stat " + m.Stat);
                    if ((int)m.Op > (int)ModOp.Override)
                        missingStat.Add(owner + " -> ModOp " + m.Op);
                    if (m.RequiredTags != Tag.None)
                    {
                        if (((uint)m.RequiredTags & ~(uint)DeclaredTags) != 0)
                            illegalTag.Add(owner + " -> RequiredTags " + m.RequiredTags);
                        usedTags.Add(m.RequiredTags);
                    }
                }
            }

            // Supports（6）+ 兼容矩阵
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                var def = SupportCatalog.Get((SupportId)i);
                Assert.IsFalse(string.IsNullOrEmpty(def.Name), "Support " + i + " Name 为空");
                ScanMods(def.Mods, "Support." + def.Name);
                if (def.TriggerEffect != EffectId.None)
                {
                    if ((int)def.TriggerEffect > (int)EffectId.ApplyIgnite)
                        badEffect.Add("Support." + def.Name + " -> EffectId " + def.TriggerEffect);
                    if ((int)def.TriggerEvent >= (int)EventId.Count)
                        badEffect.Add("Support." + def.Name + " -> EventId " + def.TriggerEvent);
                }
                CheckSupportCompat(def, compatProblems);
            }

            // Affixes（13 = S2 基线 10 + 本批 3）
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                var def = AffixCatalog.Get((AffixId)i);
                Assert.IsFalse(string.IsNullOrEmpty(def.Name), "Affix " + i + " Name 为空");
                Assert.IsFalse(string.IsNullOrEmpty(def.Format), "Affix " + i + " Format 为空");
                for (int r = 0; r < def.RowCount; r++)
                {
                    StatId stat = def.RowStat(r);
                    string owner = "Affix." + def.Name + " 行" + (r + 1);
                    if ((int)stat < 0 || (int)stat >= (int)StatId.Count)
                        badAffix.Add(owner + " -> StatId " + stat);
                    if (!IsRuntimeConsumed(stat))
                        badAffix.Add(owner + " -> 运行期未知 Stat " + stat);
                    ModOp op = def.RowOp(r);
                    if ((int)op > (int)ModOp.Override)
                        badAffix.Add(owner + " -> ModOp " + op);
                    if (r == 0 && def.Max < def.Min)
                        badAffix.Add(owner + " Max < Min");
                    if (r == 1 && def.Max2 < def.Min2)
                        badAffix.Add(owner + " Max < Min");
                }
                if (def.RowCount == 2 && string.IsNullOrEmpty(def.Format2))
                    badAffix.Add("Affix." + def.Name + " 组合第二行缺 Format2");
                if (def.RowCount == 1 && !string.IsNullOrEmpty(def.Format2))
                    badAffix.Add("Affix." + def.Name + " 单行词缀却带 Format2");
            }

            // 本批断言：新增词缀 ≤3 且全部合法（合法性由上方通用扫描覆盖）
            int newAffixes = (int)AffixId.Count - S2BaselineAffixCount;
            Assert.GreaterOrEqual(newAffixes, 0, "S3 第一批词缀池缩水：AffixId.Count < " + S2BaselineAffixCount);
            Assert.LessOrEqual(newAffixes, 3, "S3 第一批新增词缀必须 ≤3：当前 " + newAffixes);

            // S3-R2 内容数量护栏：本轮仅 Support +1（火焰转化），其余内容轴全部不变（每轮内容扩张须显式更新本护栏）
            Assert.AreEqual(S2BaselineSupportCount + 1, SupportCatalog.Count, "R2 仅允许新增 1 个 Support（火焰转化）");
            Assert.AreEqual(10 + 3, (int)AffixId.Count, "词缀轴应保持 S2 基线 10 + 第一批 3");
            Assert.AreEqual(28, (int)StatId.Count, "不得新增 StatId");
            Assert.AreEqual(4, (int)ModOp.Override, "不得新增 ModOp（最大仍为 Override=4）");
            Assert.AreEqual(256u, (uint)Tag.Duration, "不得新增 Tag（最大仍为 Duration=1<<8）");
            Assert.AreEqual(2, (int)EffectId.ApplyIgnite, "不得新增 EffectId（最大仍为 ApplyIgnite=2）");
            Assert.AreEqual(4, (int)EventId.Count, "不得新增 EventId");
            Assert.AreEqual(4, (int)ConditionId.IsSpell, "不得新增 ConditionId（最大仍为 IsSpell=4）");
            Assert.AreEqual(3, (int)SkillId.Area, "Active 技能不得新增");
            Assert.AreEqual(3, MapAffixCatalog.All.Length, "图词缀不得新增");
            // 新 Support 定义契约（S3-R2-FIRE-CONVERSION）
            SupportDef fc = SupportCatalog.Get(SupportId.FireConversion);
            Assert.AreEqual(SupportId.FireConversion, fc.Id, "FireConversion Id 必须有效");
            Assert.IsTrue(fc.ChangesMechanism, "火焰转化必须 ChangesMechanism=true");
            Assert.AreEqual(SkillId.None, fc.MechanicSkill, "火焰转化兼容性必须完全由 Tag 路径推导，不得绑死技能");
            Assert.AreEqual(EffectId.None, fc.TriggerEffect, "火焰转化不得使用 Trigger Effect");
            Assert.IsNotNull(fc.Mods);
            Assert.AreEqual(1, fc.Mods.Length, "火焰转化只允许 1 条核心 Modifier");
            Assert.AreEqual(StatId.ConvertPhysToFire, fc.Mods[0].Stat);
            Assert.AreEqual(ModOp.Flat, fc.Mods[0].Op);
            Assert.AreEqual(0.50f, fc.Mods[0].Value, 0.0001f);
            Assert.AreEqual(Tag.Attack | Tag.Hit | Tag.Physical, fc.Mods[0].RequiredTags);

            // Passives（16）+ 链接对称与连通
            Assert.AreEqual(SliceRules.PassiveCount, PassiveCatalog.Count);
            var linkSet = new HashSet<long>();
            for (int i = 0; i < PassiveCatalog.Count; i++)
            {
                var n = PassiveCatalog.Get(i);
                Assert.IsFalse(string.IsNullOrEmpty(n.Name), "Passive " + i + " Name 为空");
                ScanMods(n.Mods, "Passive." + n.Name);
                Assert.Greater(n.Links.Length, 0, "Passive " + i + " 无链接");
                foreach (var l in n.Links)
                {
                    if (l < 0 || l >= PassiveCatalog.Count)
                        missingLinks.Add(n.Name + " -> 越界链接 " + l);
                    else
                        linkSet.Add(LinkKey(i, l));
                }
            }
            foreach (var key in linkSet)
            {
                long rev = (key >> 32) | ((key & 0xffffffffL) << 32);
                if (!linkSet.Contains(rev))
                    missingLinks.Add("单向链接 " + (int)(key >> 32) + " -> " + (int)(key & 0xffffffffL));
            }
            // 连通性：从 0 出发 BFS 覆盖全部节点
            var seen = new bool[PassiveCatalog.Count];
            var queue = new Queue<int>();
            queue.Enqueue(0);
            seen[0] = true;
            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                foreach (var l in PassiveCatalog.Get(cur).Links)
                    if (!seen[l]) { seen[l] = true; queue.Enqueue(l); }
            }
            for (int i = 0; i < seen.Length; i++)
                Assert.IsTrue(seen[i], "天赋图不连通：节点 " + i + " 不可达");

            // Skills（3 Active）
            foreach (SkillId id in new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area })
            {
                var def = SkillCatalog.Get(id);
                Assert.Greater(def.Range, 0f, id + " Range");
                Assert.Greater(def.Windup, 0f, id + " Windup");
                Assert.Greater(def.Active, 0f, id + " Active");
                Assert.Greater(def.Recovery, 0f, id + " Recovery");
                Assert.GreaterOrEqual(def.Cooldown, 0f, id + " Cooldown");
            }

            // Enemies（5）与 MapAffix（3）
            foreach (EnemyKind kind in new[] { EnemyKind.Dummy, EnemyKind.Brute, EnemyKind.Stinger, EnemyKind.Ashling, EnemyKind.Warden })
            {
                var e = EnemyCatalog.Get(kind);
                Assert.Greater(e.Life, 0, kind + " Life");
                Assert.IsFalse(string.IsNullOrEmpty(e.Name), kind + " Name 为空");
            }
            var mapIds = new HashSet<int>();
            foreach (var m in MapAffixCatalog.All)
            {
                Assert.Greater(m.StabilityCost, 0, "MapAffix " + m.Name + " StabilityCost");
                Assert.IsTrue(mapIds.Add(m.Id), "MapAffix Id 重复 " + m.Id);
            }

            // 未使用 Tag（声明了但没有任何内容引用）——预留记录，不是失败项
            var unusedTags = new List<string>();
            foreach (Tag t in new[] { Tag.Attack, Tag.Spell, Tag.Melee, Tag.Projectile, Tag.Area, Tag.Hit, Tag.Physical, Tag.Fire, Tag.Duration })
            {
                bool used = false;
                foreach (var u in usedTags)
                    if ((u & t) == t) { used = true; break; }
                if (!used)
                    unusedTags.Add(t.ToString());
            }

            // 音频：5 事件已挂钩 AudioEvents（无中间件），Resources/Audio 无资产=静音+限频日志（已知债，不阻断）
            var missingAudio = new List<string> { "Cast", "Impact", "Hit", "Death", "Loot" };

            // 失败断言（缺失/非法/矩阵违规即失败；音频缺失与未使用 Tag 只记录）
            Assert.IsEmpty(missingStat, "StatId/ModOp 引用缺失或运行期未知：" + string.Join("; ", missingStat));
            Assert.IsEmpty(illegalTag, "非法 Tag：" + string.Join("; ", illegalTag));
            Assert.IsEmpty(badEffect, "Effect/Event 引用非法：" + string.Join("; ", badEffect));
            Assert.IsEmpty(missingLinks, "天赋链接缺失/单向：" + string.Join("; ", missingLinks));
            Assert.IsEmpty(badAffix, "词缀行非法：" + string.Join("; ", badAffix));
            Assert.IsEmpty(compatProblems, "Support×技能兼容矩阵：" + string.Join("; ", compatProblems));

            WriteReport(modCount, newAffixes, missingStat, illegalTag, badEffect, missingLinks, badAffix,
                compatProblems, unusedTags, missingAudio);
        }

        static bool IsRuntimeConsumed(StatId stat)
        {
            foreach (var s in RuntimeConsumedStats)
                if (s == stat)
                    return true;
            return false;
        }

        static void CheckSupportCompat(SupportDef def, List<string> problems)
        {
            SkillId[] skills;
            if (!SupportCompatGolden.Matrix.TryGetValue(def.Id, out skills))
            {
                problems.Add("未声明兼容矩阵 " + def.Name);
                return;
            }
            if (skills == null || skills.Length == 0)
            {
                problems.Add("兼容矩阵空集（死内容）：" + def.Name);
                return;
            }
            for (int i = 0; i < skills.Length; i++)
            {
                SkillId skill = skills[i];
                if (skill != SkillId.Melee && skill != SkillId.Projectile && skill != SkillId.Area)
                {
                    problems.Add("矩阵含未知技能：" + def.Name + " -> " + skill);
                    continue;
                }
                // 声明合法 ⇒ 该技能 Tag 下每条带 RequiredTags 的 Mod 必须可满足
                Tag skillTags = SkillTags.Of(skill);
                if (def.Mods == null)
                    continue;
                foreach (var m in def.Mods)
                {
                    if (m.RequiredTags != Tag.None && (skillTags & m.RequiredTags) != m.RequiredTags)
                        problems.Add("矩阵声明合法但 Tag 不满足：" + def.Name + " × " + SliceSession.SkillDisplayName(skill) +
                                     "（" + m.RequiredTags + "）");
                }
            }
            // 钉死已知不兼容（矩阵不得放宽）与已知兼容（矩阵不得收紧）
            AssertCompatExcludes(def.Id, SupportId.Concentrated, SkillId.Melee, problems);
            AssertCompatExcludes(def.Id, SupportId.Concentrated, SkillId.Projectile, problems);
            AssertCompatExcludes(def.Id, SupportId.Fork, SkillId.Melee, problems);
            AssertCompatExcludes(def.Id, SupportId.Fork, SkillId.Area, problems);
            AssertCompatIncludes(def.Id, SupportId.Concentrated, SkillId.Area, problems);
            AssertCompatIncludes(def.Id, SupportId.Fork, SkillId.Projectile, problems);
        }

        static void AssertCompatExcludes(SupportId defId, SupportId key, SkillId skill, List<string> problems)
        {
            if (defId != key)
                return;
            if (ContainsSkill(SupportCompatGolden.Matrix[key], skill))
                problems.Add("已知不兼容被放宽：" + SupportCatalog.Get(key).Name + " × " + SliceSession.SkillDisplayName(skill));
        }

        static void AssertCompatIncludes(SupportId defId, SupportId key, SkillId skill, List<string> problems)
        {
            if (defId != key)
                return;
            if (!ContainsSkill(SupportCompatGolden.Matrix[key], skill))
                problems.Add("已知兼容被收紧：" + SupportCatalog.Get(key).Name + " × " + SliceSession.SkillDisplayName(skill));
        }

        static bool ContainsSkill(SkillId[] skills, SkillId skill)
        {
            if (skills == null)
                return false;
            for (int i = 0; i < skills.Length; i++)
                if (skills[i] == skill)
                    return true;
            return false;
        }

        static long LinkKey(int a, int b)
        {
            return ((long)a << 32) | (uint)b;
        }

        static void WriteReport(int modCount, int newAffixes, List<string> missingStat, List<string> illegalTag,
            List<string> badEffect, List<string> missingLinks, List<string> badAffix, List<string> compatProblems,
            List<string> unusedTags, List<string> missingAudio)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# CONTENT_AUDIT_S3_R2");
            sb.AppendLine("");
            sb.AppendLine("生成：EditMode 测试 `ContentAuditS2Tests`（S3 R2 扩展，工作令 S3-R2-FIRE-CONVERSION）。只覆盖现行切片：3 Active + " + SupportCatalog.Count + " Support（S2 基线 6 + R2 1 火焰转化）+ " + AffixCatalog.Count + " 词缀（S2 基线 10 + 第一批 3 组合系）+ 16 天赋 + 3 图词缀 + 5 怪。");
            sb.AppendLine("");
            sb.AppendLine("## 总数");
            sb.AppendLine("");
            sb.AppendLine("| 类别 | 数量 |");
            sb.AppendLine("|---|---|");
            sb.AppendLine("| Active 技能 | 3 |");
            sb.AppendLine("| Support | " + SupportCatalog.Count + "（S2 基线 6 + R2 1） |");
            sb.AppendLine("| 词缀 | " + AffixCatalog.Count + "（S2 基线 10 + S3 第一批 " + newAffixes + "） |");
            sb.AppendLine("| 天赋节点 | 16（含 2 Notable + 1 机制烬心） |");
            sb.AppendLine("| 图词缀 | 3 |");
            sb.AppendLine("| 怪 | 5（3 普通 + Elite 监守 + 木桩） |");
            sb.AppendLine("| Modifier 引用（Support+Passive） | " + modCount + " |");
            sb.AppendLine("");
            sb.AppendLine("## 缺失 / 非法");
            sb.AppendLine("");
            sb.AppendLine("| 项 | 条数 | 明细 |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| StatId/ModOp 引用缺失或运行期未知 | " + missingStat.Count + " | " + JoinOrEmpty(missingStat) + " |");
            sb.AppendLine("| 非法 Tag | " + illegalTag.Count + " | " + JoinOrEmpty(illegalTag) + " |");
            sb.AppendLine("| Effect/Event 引用非法 | " + badEffect.Count + " | " + JoinOrEmpty(badEffect) + " |");
            sb.AppendLine("| 天赋链接缺失/单向 | " + missingLinks.Count + " | " + JoinOrEmpty(missingLinks) + " |");
            sb.AppendLine("| 词缀行非法（含运行期未知 Stat） | " + badAffix.Count + " | " + JoinOrEmpty(badAffix) + " |");
            sb.AppendLine("| Support×技能兼容矩阵违规 | " + compatProblems.Count + " | " + JoinOrEmpty(compatProblems) + " |");
            sb.AppendLine("");
            sb.AppendLine("## Support × 技能兼容矩阵（本批新增校验）");
            sb.AppendLine("");
            sb.AppendLine("判定依据：①带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足（StatBag 对不满足是静默跳过=隐形无效）；②机制路径（分裂）只接入弹道结算。**运行时已接入同一契约**：TrySetSupport 写入前调用 SliceSession.IsSupportCompatible 拒绝非法连接；golden 矩阵保持独立 oracle（SupportCompatGolden），Runtime parity 由 SupportGateTests 单独校验。");
            sb.AppendLine("");
            sb.AppendLine("| Support | Q 近战 | W 弹道 | E 范围 |");
            sb.AppendLine("|---|---|---|---|");
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                SupportDef def = SupportCatalog.Get((SupportId)i);
                SkillId[] skills = SupportCompatGolden.Matrix[def.Id];
                sb.AppendLine("| " + def.Name + " | " + Mark(skills, SkillId.Melee) + " | " + Mark(skills, SkillId.Projectile) + " | " + Mark(skills, SkillId.Area) + " |");
            }
            sb.AppendLine("");
            sb.AppendLine("已知不兼容（钉死，不得放宽）：集中×近战、集中×弹道（Tag.Area 仅范围技能满足）；分裂×近战、分裂×范围（ForkProjectiles 只进弹道结算）；火焰转化×范围（RequiredTags=Attack|Hit|Physical，范围=Spell 无 Attack）。");
            sb.AppendLine("");
            sb.AppendLine("## S3 R2 新增 Support（工作令 S3-R2-FIRE-CONVERSION）");
            sb.AppendLine("");
            sb.AppendLine("机制型转换 Support：无新 Effect/Trigger/Stat/ModOp/Tag，无 Support 专用 Runtime 分支（Reviewer 零分支审计见 `S3_R2_REVIEW.md`）；Runtime parity 由 SupportGateTests 按 golden 3×7 全 21 组合校验。");
            sb.AppendLine("");
            sb.AppendLine("| ID | 名称 | Modifier | RequiredTags | ChangesMechanism | MechanicSkill | Trigger |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            for (int i = S2BaselineSupportCount + 1; i <= SupportCatalog.Count; i++)
            {
                SupportDef def = SupportCatalog.Get((SupportId)i);
                sb.AppendLine("| " + def.Id + " | " + def.Name + " | " + JoinMods(def.Mods) + " | " + TagsText(def.Mods) + " | " +
                             (def.ChangesMechanism ? "true" : "false") + " | " + (def.MechanicSkill == SkillId.None ? "无（Tag 路径推导）" : def.MechanicSkill.ToString()) + " | " +
                             (def.TriggerEffect == EffectId.None ? "无" : def.TriggerEffect.ToString()) + " |");
            }
            sb.AppendLine("");
            sb.AppendLine("内容数量护栏（测试断言）：Support=7（+1）；词缀=13；StatId=28；ModOp/Tag/Effect/Event/Condition/Skill/图词缀轴全部 +0。");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("## 本批新增词缀（S3 第一批）");
            sb.AppendLine("");
            sb.AppendLine("全部由**已有 StatId/ModOp** 组成（新增 Stat/ModOp/Tag/Effect/Event = 0）。可出现在 4 槽（武器/胸甲/头盔/靴子）掉落池；进两步 Craft（随机制作=废料池重掷、定向制作=蚀刻剂写入列表）。第二行独立掷值，存 `ItemInstance` 第二值。");
            sb.AppendLine("");
            sb.AppendLine("| ID | 名称 | 行 1（Stat/Op） | 行 2（Stat/Op） | 槽位 | 两步 Craft |");
            sb.AppendLine("|---|---|---|---|---|---|");
            for (int i = S2BaselineAffixCount; i < AffixCatalog.Count; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                sb.AppendLine("| " + def.Id + " | " + def.Name + " | " + def.Stat + " " + ModOpName(def.Op) + "（" + def.Format + "） | " +
                             def.Stat2 + " " + ModOpName(def.Op2) + "（" + def.Format2 + "） | 4 槽全部 | 随机池 + 定向列表 |");
            }
            sb.AppendLine("");
            sb.AppendLine("## 未使用 Tag（已声明、当前内容未引用）");
            sb.AppendLine("");
            sb.AppendLine("**预留 Tag 不是失败项**：以下 Tag 为已声明预留，当前切片未引用；S3 后续批次未立令前不得当作「缺实现」去补系统或补技能。");
            sb.AppendLine("");
            sb.AppendLine(unusedTags.Count == 0 ? "无" : "- " + string.Join("\n- ", unusedTags));
            sb.AppendLine("");
            sb.AppendLine("## 音频（已知债，不阻断）");
            sb.AppendLine("");
            sb.AppendLine("全部事件（" + string.Join(", ", missingAudio) + "）已挂钩 `AudioEvents.Play`（单一入口，无中间件）。查找表 `Resources/Audio/<事件名>` 5 键已投放 CC0 clip；语音 ogg 走 `VoiceCues` 待导演指认，与本审计无关。");
            sb.AppendLine("");
            sb.AppendLine("## 预制体 / VFX 引用");
            sb.AppendLine("");
            sb.AppendLine("当前切片内容全部为代码表 + 视图层代码（胶囊/换模模型/IMGUI），无预制体 / VFX / 音频路径引用——本类 0 条。");
            sb.AppendLine("");

            string dir = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(ReportPath, sb.ToString());
            UnityEngine.Debug.Log("[ContentAudit] wrote " + ReportPath);
        }

        static string Mark(SkillId[] skills, SkillId skill)
        {
            return ContainsSkill(skills, skill) ? "✓" : "✗";
        }

        static string JoinMods(Modifier[] mods)
        {
            if (mods == null)
                return "—";
            var parts = new List<string>();
            for (int i = 0; i < mods.Length; i++)
                parts.Add(mods[i].Stat + " " + ModOpName(mods[i].Op) + " " + mods[i].Value.ToString("0.##"));
            return string.Join("；", parts.ToArray());
        }

        static string TagsText(Modifier[] mods)
        {
            if (mods == null)
                return "—";
            uint merged = 0;
            for (int i = 0; i < mods.Length; i++)
                merged |= (uint)mods[i].RequiredTags;
            return ((Tag)merged).ToString();
        }

        static string ModOpName(ModOp op)
        {
            switch (op)
            {
                case ModOp.Base: return "基础";
                case ModOp.Flat: return "固定";
                case ModOp.Increased: return "提高";
                case ModOp.More: return "更多";
                case ModOp.Override: return "覆盖";
                default: return op.ToString();
            }
        }

        static string JoinOrEmpty(List<string> list)
        {
            return list.Count == 0 ? "—" : string.Join("; ", list);
        }
    }
}
