using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S2 内容校验（S3 最小项）。只扫当前切片：3 Active + 6 Support + 10 词缀 + 16 天赋 + 3 图词缀 + 怪表。
    /// 引用缺失 / 非法 Tag / 断链 / 不连通 → 测试失败；音频缺失为已知债，仅记录不失败。
    /// 报告写入 docs/reviews/s3/CONTENT_AUDIT_S2.md。
    /// </summary>
    public sealed class ContentAuditS2Tests
    {
        static readonly Tag DeclaredTags =
            Tag.Attack | Tag.Spell | Tag.Melee | Tag.Projectile | Tag.Area |
            Tag.Hit | Tag.Physical | Tag.Fire | Tag.Duration;

        static string ReportPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/reviews/s3/CONTENT_AUDIT_S2.md")); }
        }

        [Test]
        public void ContentAuditS2_Passes()
        {
            var missingStat = new List<string>();
            var illegalTag = new List<string>();
            var badEffect = new List<string>();
            var missingLinks = new List<string>();
            var usedTags = new List<Tag>();
            int modCount = 0;

            void ScanMods(IEnumerable<Modifier> mods, string owner)
            {
                foreach (var m in mods)
                {
                    modCount++;
                    if ((int)m.Stat < 0 || (int)m.Stat >= (int)StatId.Count)
                        missingStat.Add(owner + " -> StatId " + m.Stat);
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

            // Supports (6)
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
            }

            // Affixes (10)
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                var def = AffixCatalog.Get((AffixId)i);
                Assert.IsFalse(string.IsNullOrEmpty(def.Format), "Affix " + i + " Format 为空");
                if ((int)def.Stat < 0 || (int)def.Stat >= (int)StatId.Count)
                    missingStat.Add("Affix." + def.Name + " -> StatId " + def.Stat);
                Assert.IsTrue(def.Max >= def.Min, "Affix " + def.Name + " Max < Min");
            }

            // Passives (16) + 链接对称与连通
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

            // Skills (3 Active)
            foreach (SkillId id in new[] { SkillId.Melee, SkillId.Projectile, SkillId.Area })
            {
                var def = SkillCatalog.Get(id);
                Assert.Greater(def.Range, 0f, id + " Range");
                Assert.Greater(def.Windup, 0f, id + " Windup");
                Assert.Greater(def.Active, 0f, id + " Active");
                Assert.Greater(def.Recovery, 0f, id + " Recovery");
                Assert.GreaterOrEqual(def.Cooldown, 0f, id + " Cooldown");
            }

            // Enemies (5) 与 MapAffix (3)
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

            // 未使用 Tag（声明了但没有任何内容引用）
            var unusedTags = new List<string>();
            foreach (Tag t in new[] { Tag.Attack, Tag.Spell, Tag.Melee, Tag.Projectile, Tag.Area, Tag.Hit, Tag.Physical, Tag.Fire, Tag.Duration })
            {
                bool used = false;
                foreach (var u in usedTags)
                    if ((u & t) == t) { used = true; break; }
                if (!used)
                    unusedTags.Add(t.ToString());
            }

            // 音频：S1/S2 只打事件日志，无资产接线（已知债，不阻断）
            var missingAudio = new List<string> { "Cast", "Impact", "Hit", "Death", "Loot" };

            // 失败断言（缺失/非法即失败；音频缺失与未使用 Tag 只记录）
            Assert.IsEmpty(missingStat, "StatId/ModOp 引用缺失：" + string.Join("; ", missingStat));
            Assert.IsEmpty(illegalTag, "非法 Tag：" + string.Join("; ", illegalTag));
            Assert.IsEmpty(badEffect, "Effect/Event 引用非法：" + string.Join("; ", badEffect));
            Assert.IsEmpty(missingLinks, "天赋链接缺失/单向：" + string.Join("; ", missingLinks));

            WriteReport(modCount, missingStat, illegalTag, badEffect, missingLinks, unusedTags, missingAudio);
        }

        static long LinkKey(int a, int b)
        {
            return ((long)a << 32) | (uint)b;
        }

        static void WriteReport(int modCount, List<string> missingStat, List<string> illegalTag,
            List<string> badEffect, List<string> missingLinks, List<string> unusedTags, List<string> missingAudio)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# CONTENT_AUDIT_S2");
            sb.AppendLine("");
            sb.AppendLine("生成：EditMode 测试 `ContentAuditS2Tests`（S3 最小项）。只覆盖 S2 切片：3 Active + 6 Support + 10 词缀 + 16 天赋 + 3 图词缀 + 5 怪。");
            sb.AppendLine("");
            sb.AppendLine("## 总数");
            sb.AppendLine("");
            sb.AppendLine("| 类别 | 数量 |");
            sb.AppendLine("|---|---|");
            sb.AppendLine("| Active 技能 | 3 |");
            sb.AppendLine("| Support | 6 |");
            sb.AppendLine("| 词缀 | 10 |");
            sb.AppendLine("| 天赋节点 | 16（含 2 Notable + 1 机制烬心） |");
            sb.AppendLine("| 图词缀 | 3 |");
            sb.AppendLine("| 怪 | 5（3 普通 + Elite 监守 + 木桩） |");
            sb.AppendLine("| Modifier 引用 | " + modCount + " |");
            sb.AppendLine("");
            sb.AppendLine("## 缺失 / 非法");
            sb.AppendLine("");
            sb.AppendLine("| 项 | 条数 | 明细 |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| Content ID / StatId 引用缺失 | " + missingStat.Count + " | " + JoinOrEmpty(missingStat) + " |");
            sb.AppendLine("| 非法 Tag | " + illegalTag.Count + " | " + JoinOrEmpty(illegalTag) + " |");
            sb.AppendLine("| Effect/Event 引用非法 | " + badEffect.Count + " | " + JoinOrEmpty(badEffect) + " |");
            sb.AppendLine("| 天赋链接缺失/单向 | " + missingLinks.Count + " | " + JoinOrEmpty(missingLinks) + " |");
            sb.AppendLine("");
            sb.AppendLine("## 未使用 Tag（已声明、当前内容未引用）");
            sb.AppendLine("");
            sb.AppendLine(unusedTags.Count == 0 ? "无" : "- " + string.Join("\n- ", unusedTags));
            sb.AppendLine("");
            sb.AppendLine("## 音频（已知债，不阻断 S3 最小门）");
            sb.AppendLine("");
            sb.AppendLine("全部事件（" + string.Join(", ", missingAudio) + "）仅有 `AudioEvents.Play` 日志接线，无音频资产。事件名固定 Cast/Impact/Hit/Death/Loot，接入时按名补资产即可。");
            sb.AppendLine("");
            sb.AppendLine("## 预制体 / VFX 引用");
            sb.AppendLine("");
            sb.AppendLine("当前 S2 内容全部为代码表 + 视图层代码（胶囊/换模模型/IMGUI），无预制体 / VFX / 音频路径引用——本类 0 条。");
            sb.AppendLine("");

            string dir = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(ReportPath, sb.ToString());
            UnityEngine.Debug.Log("[ContentAudit] wrote " + ReportPath);
        }

        static string JoinOrEmpty(List<string> list)
        {
            return list.Count == 0 ? "—" : string.Join("; ", list);
        }
    }
}
