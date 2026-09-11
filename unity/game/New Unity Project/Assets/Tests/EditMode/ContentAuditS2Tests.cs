using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S3 内容工厂校验（在 ContentAuditS2Tests 上扩展，类名保留以维持文档指向）。
    /// 只扫现行切片：3 Active + 7 Support（含 R2 火焰转化）+ 13 词缀（含第一批 3 条组合系）+ 16 天赋 + 3 图词缀 + 怪表。
    /// 失败条件：①REQUIRED 资源缺失 ②Support×技能兼容矩阵违规 ③词缀/Mod 行引用运行期未知 Stat / 非法 ModOp
    /// ④Tag 规则违规（未声明 bit / Skill profile 偏离 golden / 死 Tagged Modifier）⑤内容数量护栏 ⑥结构/契约问题。
    /// 未使用 Tag 与 GATED 资源（人声）只记录，不得当作失败项。
    /// S3-M3 failure-safe：语义顺序 **Collect → Render → Persist → Assert**——全部内容问题先收集进
    /// ContentAuditResult（Collect 阶段零 NUnit 断言），报告写盘后才断言；测试红 ⇒ 当前快照报告同轮红，
    /// 绝不遗留上一轮 PASS。审计基础设施异常先落 FAIL 快照再上抛；写盘失败直接红。
    /// 拆分：ContentResourceAuditContracts（资源契约）/ ContentAuditTagRules + SkillTagGolden（Tag 规则与 golden）。
    /// 报告写入 docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md（BATCH1/R2 报告保留冻结历史）。
    /// </summary>
    public sealed class ContentAuditS2Tests
    {
        /// <summary>S2 收口时词缀池基线（10 条）。S3 第一批新增 = AffixId.Count - 基线，必须 ≤3。</summary>
        const int S2BaselineAffixCount = 10;

        /// <summary>S2 收口时 Support 基线（6 条）。R2 新增 = SupportCatalog.Count - 基线，工作令要求恰好 1（火焰转化）。</summary>
        const int S2BaselineSupportCount = 6;

        /// <summary>运行期消费 Stat 白名单（读取点见注释）。内容引用白名单之外的 Stat = 「报告绿但运行期未知 Stat」，必须失败。
        /// internal：Production Content Report（S4-P1）引用同一白名单汇总 runtime coverage，不建第二份 truth。</summary>
        /// <summary>
        /// runtime consumer 白名单。**单一 owner 已下沉到 runtime**（<see cref="PassiveSupport.RuntimeConsumedStats"/>，
        /// S6P-WO-04A）：生产规则不能由测试程序集定义。本字段只是别名，禁止在此处再补第二份清单。
        /// </summary>
        internal static readonly StatId[] RuntimeConsumedStats = PassiveSupport.RuntimeConsumedStats;

        struct TaggedModEntry
        {
            public string Owner;
            public Tag Tags;
        }

        static string ReportPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/reviews/s3/CONTENT_AUDIT_S3_CLOSEOUT.md")); }
        }

        [Test]
        public void ContentAuditS2_Passes()
        {
            ContentAuditFailsafe.ExecuteAudit(ReportPath, CollectCurrentRepositoryAudit, RenderReport);
        }

        /// <summary>Stage 1：收集当前仓库审计状态。普通内容违规只入 Result 集合，绝不在此抛 NUnit 断言。</summary>
        internal static ContentAuditResult CollectCurrentRepositoryAudit()
        {
            var result = new ContentAuditResult();
            var usedTags = new List<Tag>();
            var taggedMods = new List<TaggedModEntry>();
            int modCount = 0;

            void ScanMods(IEnumerable<Modifier> mods, string owner)
            {
                foreach (var m in mods)
                {
                    modCount++;
                    result.DeclaredStats.Add(m.Stat.ToString());
                    if ((int)m.Stat < 0 || (int)m.Stat >= (int)StatId.Count)
                        result.MissingStat.Add(owner + " -> StatId " + m.Stat);
                    if (!IsRuntimeConsumed(m.Stat))
                        result.MissingStat.Add(owner + " -> 运行期未知 Stat " + m.Stat);
                    if ((int)m.Op > (int)ModOp.Override)
                        result.MissingStat.Add(owner + " -> ModOp " + m.Op);
                    if (m.RequiredTags != Tag.None)
                    {
                        if (((uint)m.RequiredTags & ~(uint)SkillTagGolden.DeclaredTags) != 0)
                            result.IllegalTag.Add(owner + " -> RequiredTags " + m.RequiredTags);
                        usedTags.Add(m.RequiredTags);
                        taggedMods.Add(new TaggedModEntry { Owner = owner, Tags = m.RequiredTags });
                    }
                }
            }

            // Supports（7）+ 兼容矩阵
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                var def = SupportCatalog.Get((SupportId)i);
                if (string.IsNullOrEmpty(def.Name))
                    result.StructuralProblems.Add("Support " + i + " Name 为空");
                ScanMods(def.Mods, "Support." + def.Name);
                if (def.TriggerEffect != EffectId.None)
                {
                    if ((int)def.TriggerEffect > (int)EffectId.ApplyIgnite)
                        result.BadEffect.Add("Support." + def.Name + " -> EffectId " + def.TriggerEffect);
                    if ((int)def.TriggerEvent >= (int)EventId.Count)
                        result.BadEffect.Add("Support." + def.Name + " -> EventId " + def.TriggerEvent);
                }
                CheckSupportCompat(def, result.CompatProblems);
                SkillId[] matrix;
                if (SupportCompatGolden.Matrix.TryGetValue(def.Id, out matrix))
                    result.MatrixRows.Add("| " + def.Name + " | " + Mark(matrix, SkillId.Melee) + " | " + Mark(matrix, SkillId.Projectile) + " | " + Mark(matrix, SkillId.Area) + " |");
                else
                    result.MatrixRows.Add("| " + def.Name + " | 未声明 | 未声明 | 未声明 |");
            }

            // Affixes（13 = S2 基线 10 + 第一批 3）
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                var def = AffixCatalog.Get((AffixId)i);
                if (string.IsNullOrEmpty(def.Name))
                    result.StructuralProblems.Add("Affix " + i + " Name 为空");
                if (string.IsNullOrEmpty(def.Format))
                    result.StructuralProblems.Add("Affix " + i + " Format 为空");
                for (int r = 0; r < def.RowCount; r++)
                {
                    StatId stat = def.RowStat(r);
                    result.DeclaredStats.Add(stat.ToString());
                    string owner = "Affix." + def.Name + " 行" + (r + 1);
                    if ((int)stat < 0 || (int)stat >= (int)StatId.Count)
                        result.BadAffix.Add(owner + " -> StatId " + stat);
                    if (!IsRuntimeConsumed(stat))
                        result.BadAffix.Add(owner + " -> 运行期未知 Stat " + stat);
                    ModOp op = def.RowOp(r);
                    if ((int)op > (int)ModOp.Override)
                        result.BadAffix.Add(owner + " -> ModOp " + op);
                    if (r == 0 && def.Max < def.Min)
                        result.BadAffix.Add(owner + " Max < Min");
                    if (r == 1 && def.Max2 < def.Min2)
                        result.BadAffix.Add(owner + " Max < Min");
                }
                if (def.RowCount == 2 && string.IsNullOrEmpty(def.Format2))
                    result.BadAffix.Add("Affix." + def.Name + " 组合第二行缺 Format2");
                if (def.RowCount == 1 && !string.IsNullOrEmpty(def.Format2))
                    result.BadAffix.Add("Affix." + def.Name + " 单行词缀却带 Format2");
                // S4-P3：applicability mask + stable ID（位=Id 一致，防目录 reorder 导致旧 ID 漂移）
                string maskErr = ContentAuditTagRules.ValidateAffixSlots(def);
                if (maskErr != null)
                    result.StructuralProblems.Add("Affix." + def.Name + "：" + maskErr);
                if (def.Id != (AffixId)i)
                    result.StructuralProblems.Add("Affix stable ID 漂移：目录位 " + i + " 的 Id=" + def.Id);
            }

            // S4-P3：六槽 eligible 池非空（canonical IsApplicable 派生；任一槽空池=结构/契约失败）
            var slotPool = new int[(int)EquipSlot.Count];
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                for (int s = 0; s < (int)EquipSlot.Count; s++)
                    if (def.IsApplicable((EquipSlot)s))
                        slotPool[s]++;
            }
            for (int s = 0; s < slotPool.Length; s++)
                if (slotPool[s] == 0)
                    result.StructuralProblems.Add("装备槽 eligible 词缀池为空：" + SliceSession.SlotName((EquipSlot)s));

            // S3 第一批护栏（≤3）由 S4-P3 取代，S4-P3（≤4，总 17）由 S5-WO-04 取代：旧 13 ID 只追加不缩水；
            // S5-WO-04（BL-002.A1 锁定清单，docs/reviews/S5/S5_AFFIX_ADMISSION.md）追加恰 4（总上限 21，仍为硬上限）
            int newAffixes = (int)AffixId.Count - S2BaselineAffixCount;
            result.NewAffixCount = newAffixes;
            if (newAffixes < 0)
                result.StructuralProblems.Add("词缀池缩水：AffixId.Count < " + S2BaselineAffixCount);
            if ((int)AffixId.Count > S2BaselineAffixCount + 3 + 4 + 4)
                result.StructuralProblems.Add("S5-WO-04 词缀追加必须 ≤4（总上限 21，BL-002.A1）：当前 " + (int)AffixId.Count);

            // S3-R2 内容数量护栏；S6P-WO-05 再追加 2 条机制型 Support（投射物返回/狙击印记），护栏随之 +3
            PinCount(result, SupportCatalog.Count, S2BaselineSupportCount + 3, "Support（R2 +1 火焰转化；S6P-WO-05 +2 投射物返回/狙击印记）");
            PinCount(result, (int)StatId.Count, 28, "StatId（不得新增）");
            PinCount(result, (int)ModOp.Override, 4, "ModOp（最大仍为 Override=4）");
            PinCount(result, (int)Tag.Duration, 256, "Tag（最大仍为 Duration=1<<8）");
            PinCount(result, (int)EffectId.ApplyIgnite, 2, "EffectId（最大仍为 ApplyIgnite=2）");
            PinCount(result, (int)EventId.Count, 4, "EventId（不得新增）");
            PinCount(result, (int)ConditionId.IsSpell, 4, "ConditionId（最大仍为 IsSpell=4）");
            PinCount(result, (int)SkillId.Fireball, 5, "Active 技能（S6P-WO-05 追加冰矛=4/火球术=5）");
            PinCount(result, MapAffixCatalog.All.Length, 3, "图词缀（不得新增）");

            // R2 Support 定义契约（S3-R2-FIRE-CONVERSION）
            SupportDef fc = SupportCatalog.Get(SupportId.FireConversion);
            if (fc.Id != SupportId.FireConversion)
                result.StructuralProblems.Add("FireConversion Id 必须有效");
            if (!fc.ChangesMechanism)
                result.StructuralProblems.Add("火焰转化必须 ChangesMechanism=true");
            if (fc.MechanicSkill != SkillId.None)
                result.StructuralProblems.Add("火焰转化兼容性必须完全由 Tag 路径推导，不得绑死技能");
            if (fc.TriggerEffect != EffectId.None)
                result.StructuralProblems.Add("火焰转化不得使用 Trigger Effect");
            if (fc.Mods == null)
            {
                result.StructuralProblems.Add("火焰转化 Mods 缺失");
            }
            else
            {
                if (fc.Mods.Length != 1)
                    result.StructuralProblems.Add("火焰转化只允许 1 条核心 Modifier，实际 " + fc.Mods.Length);
                else
                {
                    Modifier m0 = fc.Mods[0];
                    if (m0.Stat != StatId.ConvertPhysToFire)
                        result.StructuralProblems.Add("火焰转化核心 Stat 必须 ConvertPhysToFire，实际 " + m0.Stat);
                    if (m0.Op != ModOp.Flat)
                        result.StructuralProblems.Add("火焰转化核心 Op 必须 Flat，实际 " + m0.Op);
                    if (Mathf.Abs(m0.Value - 0.50f) > 0.0001f)
                        result.StructuralProblems.Add("火焰转化核心值必须 0.50，实际 " + m0.Value);
                    if (m0.RequiredTags != (Tag.Attack | Tag.Hit | Tag.Physical))
                        result.StructuralProblems.Add("火焰转化 RequiredTags 必须 Attack|Hit|Physical，实际 " + m0.RequiredTags);
                }
            }

            // Passives（真实 PoE 天赋域）+ 链接对称与连通
            if (PassiveCatalog.Count != SliceRules.PassiveCount)
                result.StructuralProblems.Add("Passive 数量 " + PassiveCatalog.Count + " ≠ SliceRules.PassiveCount " + SliceRules.PassiveCount);
            var linkSet = new HashSet<long>();
            for (int i = 0; i < PassiveCatalog.Count; i++)
            {
                var n = PassiveCatalog.Get(i);
                if (string.IsNullOrEmpty(n.Name))
                    result.StructuralProblems.Add("Passive " + i + " Name 为空");
                ScanMods(n.Mods, "Passive." + n.Name);
                // 时光珠宝类显著点在官方数据里本就没有连线（locked）：树上可见但不可点，不算缺链。
                if (n.Links.Length == 0 && PoeTree.Get(i).locked == 0)
                    result.MissingLinks.Add("Passive " + i + " 无链接");
                foreach (var l in n.Links)
                {
                    if (l < 0 || l >= PassiveCatalog.Count)
                        result.MissingLinks.Add(n.Name + " -> 越界链接 " + l);
                    else
                        linkSet.Add(LinkKey(i, l));
                }
            }
            foreach (var key in linkSet)
            {
                long rev = (key >> 32) | ((key & 0xffffffffL) << 32);
                if (!linkSet.Contains(rev))
                    result.MissingLinks.Add("单向链接 " + (int)(key >> 32) + " -> " + (int)(key & 0xffffffffL));
            }
            // 连通性：从 0 出发 BFS 覆盖全部节点（链接已先核入 MissingLinks，BFS 只走合法边保证 Collect 完整）
            if (PassiveCatalog.Count > 0)
            {
                var seen = new bool[PassiveCatalog.Count];
                var queue = new Queue<int>();
                queue.Enqueue(0);
                seen[0] = true;
                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    foreach (var l in PassiveCatalog.Get(cur).Links)
                        if (l >= 0 && l < PassiveCatalog.Count && !seen[l]) { seen[l] = true; queue.Enqueue(l); }
                }
                for (int i = 0; i < seen.Length; i++)
                    if (!seen[i] && PoeTree.Get(i).locked == 0)
                        result.StructuralProblems.Add("天赋图不连通：节点 " + i + " 不可达");
            }

            // Skills（S6P-WO-05 起 5 Active：原 3 + 冰矛/火球术）——枚举真值=SkillTagGolden.All，
            // 但每个技能的定义校验仍是独立的（不因新增而跳过任何一条）
            SkillId[] activeSkills = SkillTagGolden.All;
            result.ActiveSkillCount = activeSkills.Length;
            foreach (SkillId id in activeSkills)
            {
                var def = SkillCatalog.Get(id);
                if (def.Range <= 0f) result.StructuralProblems.Add(id + " Range 必须 > 0");
                if (def.Windup <= 0f) result.StructuralProblems.Add(id + " Windup 必须 > 0");
                if (def.Active <= 0f) result.StructuralProblems.Add(id + " Active 必须 > 0");
                if (def.Recovery <= 0f) result.StructuralProblems.Add(id + " Recovery 必须 > 0");
                if (def.Cooldown < 0f) result.StructuralProblems.Add(id + " Cooldown 必须 ≥ 0");
            }

            // Enemies（5）与 MapAffix（3）
            var enemyKinds = new[] { EnemyKind.Dummy, EnemyKind.Brute, EnemyKind.Stinger, EnemyKind.Ashling, EnemyKind.Warden };
            result.EnemyCount = enemyKinds.Length;
            foreach (EnemyKind kind in enemyKinds)
            {
                var e = EnemyCatalog.Get(kind);
                if (e.Life <= 0)
                    result.StructuralProblems.Add(kind + " Life 必须 > 0");
                if (string.IsNullOrEmpty(e.Name))
                    result.StructuralProblems.Add(kind + " Name 为空");
            }
            var mapIds = new HashSet<int>();
            foreach (var m in MapAffixCatalog.All)
            {
                if (m.StabilityCost <= 0)
                    result.StructuralProblems.Add("MapAffix " + m.Name + " StabilityCost 必须 > 0");
                if (!mapIds.Add(m.Id))
                    result.StructuralProblems.Add("MapAffix Id 重复 " + m.Id);
            }

            // Skill Tag golden parity（Rule B）+ 当前形态规则（Rule C/D）+ 死 Tagged Modifier（Rule E）
            // 必须在全部 ScanMods（Support+Passive）之后执行，保证 taggedMods 覆盖整个内容库
            foreach (var pair in SkillTagGolden.Masks)
            {
                Tag runtime = SkillTags.Of(pair.Key);
                result.TagProfileRows.Add("| " + pair.Key + " | " + runtime + " | " + pair.Value + " | " + (runtime == pair.Value ? "✓" : "✗") + " |");
                if (runtime != pair.Value)
                    result.TagProblems.Add(pair.Key + " Runtime Tag " + runtime + " ≠ golden " + pair.Value);
                string ruleErr = ContentAuditTagRules.ValidateSkillMask(pair.Key, runtime);
                if (ruleErr != null)
                    result.TagProblems.Add(ruleErr);
            }
            int reachable = 0;
            foreach (var t in taggedMods)
            {
                string err = ContentAuditTagRules.ValidateRequiredTags(t.Tags);
                if (err != null)
                    result.DeadTagged.Add(t.Owner + " -> " + t.Tags + "（" + err + "）");
                else
                    reachable++;
                var skills = ContentAuditTagRules.SatisfyingSkills(t.Tags);
                result.ReachabilityRows.Add("| " + t.Owner + " | " + t.Tags + " | " + JoinSkills(skills) + " | " + (skills.Count > 0 ? "✓" : "✗") + " |");
            }
            result.TaggedModCount = taggedMods.Count;
            result.ReachableTaggedMods = reachable;

            // 资源契约：真实加载验证（与 Runtime 同 key/同类型/同拼接语义）。REQUIRED 缺失=失败；GATED 缺失=只记录。
            int requiredPassed = 0, requiredTotal = 0, gatedPresent = 0, gatedMissing = 0;
            foreach (var contract in ContentResourceAuditContracts.All)
            {
                var r = ContentResourceAuditContracts.Verify(contract);
                string resultWord = ContentResourceAuditContracts.ResultWord(r.Loaded, contract.Class);
                result.ResourceRows.Add("| " + contract.Logical + " | " + contract.Key + " | " + contract.TypeName + " | " + contract.Class + " | " +
                                 (r.Loaded ? r.AssetPath : "—") + " | " + resultWord + " |");
                if (contract.Class == ResourceClass.Required)
                {
                    requiredTotal++;
                    if (r.Loaded && !string.IsNullOrEmpty(r.AssetPath) && r.AssetPath.StartsWith("Assets/Resources/"))
                        requiredPassed++;
                    else if (r.Loaded)
                        result.RequiredResourceMissing.Add(contract.Logical + "（" + contract.Key + "）：已加载但资产路径异常 " + r.AssetPath);
                    else
                        result.RequiredResourceMissing.Add(contract.Logical + "（" + contract.Key + "）：" + r.Error);
                }
                else if (contract.Class == ResourceClass.Gated)
                {
                    if (r.Loaded)
                        gatedPresent++;
                    else
                        gatedMissing++;
                }
            }
            result.RequiredPassed = requiredPassed;
            result.RequiredTotal = requiredTotal;
            result.GatedPresent = gatedPresent;
            result.GatedMissing = gatedMissing;

            // 未使用 Tag（声明了但没有任何内容引用）——预留记录，不是失败项
            foreach (Tag t in new[] { Tag.Attack, Tag.Spell, Tag.Melee, Tag.Projectile, Tag.Area, Tag.Hit, Tag.Physical, Tag.Fire, Tag.Duration })
            {
                bool used = false;
                foreach (var u in usedTags)
                    if ((u & t) == t) { used = true; break; }
                if (!used)
                    result.UnusedTags.Add(t.ToString());
            }

            // 数量快照（渲染用；护栏违规时如实渲染实际值）
            result.SupportCount = SupportCatalog.Count;
            result.AffixCount = AffixCatalog.Count;
            result.PassiveCount = PassiveCatalog.Count;
            result.MapAffixCount = MapAffixCatalog.All.Length;
            result.ModRefCount = modCount;

            // R2 新增 Support 与第一批新增词缀的表格行
            for (int i = S2BaselineSupportCount + 1; i <= SupportCatalog.Count; i++)
            {
                SupportDef def = SupportCatalog.Get((SupportId)i);
                result.R2SupportRows.Add("| " + def.Id + " | " + def.Name + " | " + JoinMods(def.Mods) + " | " + TagsText(def.Mods) + " | " +
                             (def.ChangesMechanism ? "true" : "false") + " | " + (def.MechanicSkill == SkillId.None ? "无（Tag 路径推导）" : def.MechanicSkill.ToString()) + " | " +
                             (def.TriggerEffect == EffectId.None ? "无" : def.TriggerEffect.ToString()) + " |");
            }
            for (int i = S2BaselineAffixCount; i < AffixCatalog.Count; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)i);
                result.NewAffixRows.Add("| " + def.Id + " | " + def.Name + " | " + def.Stat + " " + ModOpName(def.Op) + "（" + def.Format + "） | " +
                             def.Stat2 + " " + ModOpName(def.Op2) + "（" + def.Format2 + "） | " + SlotText(def) + " | 随机池 + 定向列表 |");
            }
            return result;
        }

        /// <summary>Stage 2：纯渲染——同一 Result 两次渲染 byte 级一致（无时间戳/GUID/用户名/绝对路径/会话 id）。</summary>
        internal static string RenderReport(ContentAuditResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# CONTENT_AUDIT_S3_CLOSEOUT");
            sb.AppendLine("");
            sb.AppendLine("**当前 Content Audit snapshot（测试再生）**。文件名保留 CLOSEOUT 是因为它源自 S3 Phase 1/2 收口；**BATCH1 / R2 报告才是冻结历史 evidence**。");
            sb.AppendLine("");
            sb.AppendLine("生成：EditMode 测试 `ContentAuditS2Tests`，failure-safe 顺序 **Collect → Render → Persist → Assert**（S3-M3：报告先于测试断言落盘——测试红 ⇒ 本快照同轮红，不遗留上一轮 PASS）。只覆盖现行切片：3 Active + " + result.SupportCount + " Support + " + result.AffixCount + " 词缀 + " + result.PassiveCount + " 天赋 + " + result.MapAffixCount + " 图词缀 + " + result.EnemyCount + " 怪。");
            sb.AppendLine("");
            sb.AppendLine("## Verdict");
            sb.AppendLine("");
            sb.AppendLine("- Audit completed: " + (result.Completed ? "YES" : "NO"));
            sb.AppendLine("- Verdict: " + (result.Completed && result.FailureCount == 0 ? "PASS" : "FAIL"));
            sb.AppendLine("- Failure count: " + result.FailureCount);
            if (!string.IsNullOrEmpty(result.ExecutionError))
                sb.AppendLine("- Execution error: " + result.ExecutionError);
            sb.AppendLine("");
            sb.AppendLine("## 总数");
            sb.AppendLine("");
            sb.AppendLine("| 类别 | 数量 |");
            sb.AppendLine("|---|---|");
            sb.AppendLine("| Active 技能 | " + result.ActiveSkillCount + " |");
            sb.AppendLine("| Support | " + result.SupportCount + " |");
            sb.AppendLine("| 词缀 | " + result.AffixCount + " |");
            sb.AppendLine("| 天赋节点 | " + result.PassiveCount + "（含 2 Notable + 1 机制烬心） |");
            sb.AppendLine("| 图词缀 | " + result.MapAffixCount + " |");
            sb.AppendLine("| 怪 | " + result.EnemyCount + "（3 普通 + Elite 监守 + 木桩） |");
            sb.AppendLine("| Modifier 引用（Support+Passive） | " + result.ModRefCount + " |");
            sb.AppendLine("");
            sb.AppendLine("内容数量护栏（Collect 阶段核入「结构/契约问题」）：Support=7 / StatId=28 / ModOp、Tag、Effect、Event、Condition、Skill、图词缀轴全部 +0；词缀=13 基线 + S4-P3 追加 ≤4（旧 ID 只追加不缩水，S4-P3 前基线冻结）。");
            sb.AppendLine("");
            sb.AppendLine("## 资源契约（真实加载验证，非声明文字）");
            sb.AppendLine("");
            sb.AppendLine("REQUIRED 缺失=审计失败；GATED 缺失=如实记录不失败（导演门控）；「无声明引用」与「有引用但缺资源」是两种状态——VFX 当前 Declared references = 0（Runtime 无任何 VFX 资源路径，Reviewer 已独立扫描复核）→ Result: N/A，与资源缺失不同。");
            sb.AppendLine("");
            sb.AppendLine("| Logical | Resource Key | Type | Class | Asset Path | Result |");
            sb.AppendLine("|---|---|---|---|---|---|");
            foreach (var row in result.ResourceRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("REQUIRED 通过 " + result.RequiredPassed + "/" + result.RequiredTotal + "；GATED present " + result.GatedPresent + " / missing " + result.GatedMissing + "（缺失不失败）。");
            sb.AppendLine("");
            sb.AppendLine("## Skill Tag Profiles（golden parity + 当前形态规则）");
            sb.AppendLine("");
            sb.AppendLine("| Skill | Runtime Tags | Golden | Result |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var row in result.TagProfileRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("## Tagged Modifier Reachability（Rule E：死 Tagged Modifier = 0）");
            sb.AppendLine("");
            sb.AppendLine("| Owner | RequiredTags | 可满足 Skill | 结果 |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var row in result.ReachabilityRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("Tagged Modifier 总数 " + result.TaggedModCount + "，可满足 " + result.ReachableTaggedMods + "，不可满足 " + (result.TaggedModCount - result.ReachableTaggedMods) + "（期望 0）。负向测试 `ContentAuditTagRules_NegativeCases_AreRejected` 证明规则能抓坏合成输入。");
            sb.AppendLine("");
            sb.AppendLine("## 缺失 / 非法");
            sb.AppendLine("");
            sb.AppendLine("| 项 | 条数 | 明细 |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| 结构/契约问题（目录条目 Name / 数量护栏 / FireConversion 契约 / 图连通 / 技能与怪参数 / MapAffix） | " + result.StructuralProblems.Count + " | " + JoinOrEmpty(result.StructuralProblems) + " |");
            sb.AppendLine("| StatId/ModOp 引用缺失或运行期未知 | " + result.MissingStat.Count + " | " + JoinOrEmpty(result.MissingStat) + " |");
            sb.AppendLine("| 非法 Tag | " + result.IllegalTag.Count + " | " + JoinOrEmpty(result.IllegalTag) + " |");
            sb.AppendLine("| Effect/Event 引用非法 | " + result.BadEffect.Count + " | " + JoinOrEmpty(result.BadEffect) + " |");
            sb.AppendLine("| 天赋链接缺失/单向 | " + result.MissingLinks.Count + " | " + JoinOrEmpty(result.MissingLinks) + " |");
            sb.AppendLine("| 词缀行非法（含运行期未知 Stat） | " + result.BadAffix.Count + " | " + JoinOrEmpty(result.BadAffix) + " |");
            sb.AppendLine("| Support×技能兼容矩阵违规 | " + result.CompatProblems.Count + " | " + JoinOrEmpty(result.CompatProblems) + " |");
            sb.AppendLine("| Skill Tag 规则/golden parity 违规 | " + result.TagProblems.Count + " | " + JoinOrEmpty(result.TagProblems) + " |");
            sb.AppendLine("| 死 Tagged Modifier | " + result.DeadTagged.Count + " | " + JoinOrEmpty(result.DeadTagged) + " |");
            sb.AppendLine("| REQUIRED 资源缺失 | " + result.RequiredResourceMissing.Count + " | " + JoinOrEmpty(result.RequiredResourceMissing) + " |");
            sb.AppendLine("");
            sb.AppendLine("## Support × 技能兼容矩阵");
            sb.AppendLine("");
            sb.AppendLine("判定依据：①带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足；②机制路径（SupportDef.MechanicSkill）。**运行时已接入同一契约**：TrySetSupport 写入前调用 SliceSession.IsSupportCompatible 拒绝非法连接；golden 矩阵保持独立 oracle（SupportCompatGolden），Runtime parity 由 SupportGateTests 单独校验。");
            sb.AppendLine("");
            sb.AppendLine("| Support | Q 近战 | W 弹道 | E 范围 |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var row in result.MatrixRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("已知不兼容（钉死，不得放宽）：集中×近战、集中×弹道（Tag.Area 仅范围技能满足）；分裂×近战、分裂×范围（ForkProjectiles 只进弹道结算）；火焰转化×范围（RequiredTags=Attack|Hit|Physical，范围=Spell 无 Attack）。");
            sb.AppendLine("");
            sb.AppendLine("## S3 R2 新增 Support（工作令 S3-R2-FIRE-CONVERSION）");
            sb.AppendLine("");
            sb.AppendLine("机制型转换 Support：无新 Effect/Trigger/Stat/ModOp/Tag，无 Support 专用 Runtime 分支（Reviewer 零分支审计见 `S3_R2_REVIEW.md`）。");
            sb.AppendLine("");
            sb.AppendLine("| ID | 名称 | Modifier | RequiredTags | ChangesMechanism | MechanicSkill | Trigger |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var row in result.R2SupportRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("## S3 第一批 + S4-P3 追加词缀");
            sb.AppendLine("");
            sb.AppendLine("全部由**已有 StatId/ModOp** 组成。S4-P3 起带 AllowedSlots（0=不限槽）；可出现在其 eligible 槽位掉落池（不限槽=全部 6 槽）。第二行独立掷值，存 `ItemInstance` 第二值。定向制作对非法槽位组合 deterministic reject。");
            sb.AppendLine("");
            sb.AppendLine("| ID | 名称 | 行 1（Stat/Op） | 行 2（Stat/Op） | 槽位 | 两步 Craft |");
            sb.AppendLine("|---|---|---|---|---|---|");
            foreach (var row in result.NewAffixRows)
                sb.AppendLine(row);
            sb.AppendLine("");
            sb.AppendLine("## 未使用 Tag（已声明、当前内容未引用）");
            sb.AppendLine("");
            sb.AppendLine("**预留 Tag 不是失败项**：以下 Tag 为已声明预留，当前切片未引用；后续批次未立令前不得当作「缺实现」去补系统或补技能。");
            sb.AppendLine("");
            sb.AppendLine(result.UnusedTags.Count == 0 ? "无" : "- " + string.Join("\n- ", result.UnusedTags));
            sb.AppendLine("");
            return sb.ToString();
        }

        static void PinCount(ContentAuditResult result, int actual, int expected, string axis)
        {
            if (actual != expected)
                result.StructuralProblems.Add("内容数量护栏违规：" + axis + " 期望 " + expected + " 实际 " + actual);
        }

        /// <summary>S4-P3：AllowedSlots 的报告文字（0=不限槽=全部 6 槽）。</summary>
        static string SlotText(AffixDef def)
        {
            if (def.AllowedSlots == 0)
                return "6 槽全部";
            var names = new List<string>();
            for (int s = 0; s < (int)EquipSlot.Count; s++)
                if ((def.AllowedSlots & (ushort)(1 << s)) != 0)
                    names.Add(SliceSession.SlotName((EquipSlot)s));
            return "仅 " + string.Join("/", names.ToArray());
        }

        /// <summary>Tag 规则负向测试：合成坏输入必须被 validator 拒绝（证明规则本身有效，而非当前内容碰巧合法）。</summary>
        [Test]
        public void ContentAuditTagRules_NegativeCases_AreRejected()
        {
            // Negative 1：Attack|Spell 当前无任何技能可满足（Rule E）
            Assert.IsNotNull(ContentAuditTagRules.ValidateRequiredTags(Tag.Attack | Tag.Spell),
                "Attack|Spell 应被判为死 Tagged Modifier");
            // Negative 2：技能 profile 同时 Melee|Projectile（Rule D）
            Tag badProfile = Tag.Melee | Tag.Projectile | Tag.Hit | Tag.Physical;
            Assert.IsNotNull(ContentAuditTagRules.ValidateSkillMask(SkillId.Melee, badProfile),
                "Melee+Projectile 当前形态冲突应被拒绝");
            // Negative 3：未声明 bit
            Tag bogus = (Tag)0x4000u;
            Assert.IsNotNull(ContentAuditTagRules.ValidateRequiredTags(bogus), "未声明 bit 应被拒绝");
            Assert.IsNotNull(ContentAuditTagRules.ValidateSkillMask(SkillId.Melee, SkillTagGolden.Masks[SkillId.Melee] | bogus),
                "技能 mask 含未声明 bit 应被拒绝");
            // 正例对照：合法 mask 与可满足 RequiredTags 通过
            Assert.IsNull(ContentAuditTagRules.ValidateSkillMask(SkillId.Melee, SkillTagGolden.Masks[SkillId.Melee]));
            Assert.IsNull(ContentAuditTagRules.ValidateRequiredTags(Tag.Attack | Tag.Hit | Tag.Physical));
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
                if (SkillTags.Of(skill) == Tag.None)
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
            PinCompatExcludes(def.Id, SupportId.Concentrated, SkillId.Melee, problems);
            PinCompatExcludes(def.Id, SupportId.Concentrated, SkillId.Projectile, problems);
            PinCompatExcludes(def.Id, SupportId.Fork, SkillId.Melee, problems);
            PinCompatExcludes(def.Id, SupportId.Fork, SkillId.Area, problems);
            PinCompatIncludes(def.Id, SupportId.Concentrated, SkillId.Area, problems);
            PinCompatIncludes(def.Id, SupportId.Fork, SkillId.Projectile, problems);
        }

        static void PinCompatExcludes(SupportId defId, SupportId key, SkillId skill, List<string> problems)
        {
            if (defId != key)
                return;
            if (ContainsSkill(SupportCompatGolden.Matrix[key], skill))
                problems.Add("已知不兼容被放宽：" + SupportCatalog.Get(key).Name + " × " + SliceSession.SkillDisplayName(skill));
        }

        static void PinCompatIncludes(SupportId defId, SupportId key, SkillId skill, List<string> problems)
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

        static string JoinSkills(List<SkillId> skills)
        {
            if (skills == null || skills.Count == 0)
                return "—";
            var names = new List<string>();
            foreach (var s in skills)
                names.Add(SliceSession.SkillDisplayName(s));
            return string.Join("/", names.ToArray());
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
