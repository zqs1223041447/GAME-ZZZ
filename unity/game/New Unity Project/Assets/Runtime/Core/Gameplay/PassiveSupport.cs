using System;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S6P-WO-04A — 被动域**唯一** support truth：回答"这条被动承诺，当前引擎能不能完整兑现"。
    ///
    /// 唯一 owner 的含义（合同 §9/§11/§20）：
    ///   · 行四分类（CONSUMED / BLOCKED_BY_DOMAIN / SPECIAL_INTERACTION / STRUCTURAL）只在本文件实现；
    ///   · 节点资格只由 <see cref="EvaluateNode(int)"/> 给出；
    ///   · "哪些 StatId 真有 runtime consumer" 由本域拥有（<see cref="RuntimeConsumedStats"/>），
    ///     census / ContentAudit 等测试程序集**反过来消费它**——生产规则不得由测试程序集定义；
    ///   · 分配门（SliceSession.TryAllocate）、消费门（RecalcPlayer / CollectSkillMods）与 UI 读同一份 truth。
    ///
    /// 本类只判"能不能完整兑现"，不做数值，也不产生第二套 modifier 解释器。
    /// </summary>
    public static class PassiveSupport
    {
        // ================= 行分类 =================

        public enum LineBucket : byte
        {
            /// <summary>parser 命中，且产出的每个 StatId 都有真实 runtime consumer。</summary>
            Consumed = 0,
            /// <summary>命中"当前引擎没有的玩法轴"关键词表 —— 引擎兑现不了这一行。</summary>
            BlockedByDomain = 1,
            /// <summary>需要 bespoke 交互，而当前没有对应 runtime handler。</summary>
            SpecialInteraction = 2,
            /// <summary>官方数据里的注释/提示行；语义角色不是 gameplay effect。</summary>
            Structural = 3,
            /// <summary>既非可消费、也非已知缺失轴、还有数字 —— 分类器没覆盖到（门禁要求必须为 0）。</summary>
            Unknown = 4
        }

        // ================= 节点资格 =================

        public enum NodeStatus : byte
        {
            /// <summary>全部 gameplay 效果行都可兑现 ⇒ 可分配。</summary>
            AllocatableSupported = 0,
            /// <summary>存在至少一条引擎兑现不了的 gameplay 效果行 ⇒ 整节点不可分配（§14 fail closed）。</summary>
            BlockedCurrently = 1,
            /// <summary>需要当前不存在的 bespoke handler（珠宝孔 / 时光珠宝类节点）⇒ 不可分配（§12）。</summary>
            BlockedSpecialInteraction = 2,
            /// <summary>专精：WO-03 建立显式选择前一律不可分配（§17）。</summary>
            SpecialPendingMastery = 3,
            /// <summary>不在上树节点索引域内（403 未上树节点根本不在 PoeTree.Nodes 里）。</summary>
            OutOfDomain = 4
        }

        /// <summary>
        /// S6P-WO-04A2 —— **效果维度**：这个节点承诺的 gameplay effect 当前能不能完整兑现。
        /// 与 <see cref="TraversalTruth"/> 相互独立：路由资格不看它，效果兑现不看路由。
        /// </summary>
        public enum EffectTruth : byte
        {
            /// <summary>全部 gameplay 效果行都可兑现 ⇒ 分配后应用其完整 modifier 集。</summary>
            FullySupported = 0,
            /// <summary>含当前兑现不了的 gameplay promise（含 mixed node）⇒ 整节点 0 modifier。</summary>
            Unfulfilled = 1,
            /// <summary>需要本令不涉及的专门机制（专精 / 珠宝孔 / 时光珠宝类）⇒ 不作普通节点式兑现。</summary>
            SpecialPending = 2
        }

        /// <summary>
        /// S6P-WO-04A2 —— **通行维度**：这个节点能不能作为被动树路径。
        /// </summary>
        public enum TraversalTruth : byte
        {
            /// <summary>属当前普通被动分配域：可分配、扣点、进 selected 集、可作为后续邻接节点的路径。</summary>
            Traversable = 0,
            /// <summary>在树数据里，但分配/通行需要本令禁止涉及的特殊机制 ⇒ 不可分配、不可作为 transit。</summary>
            SpecialBlocked = 1,
            /// <summary>已有结构事实认定它不属当前普通被动分配域。</summary>
            OutOfDomain = 2
        }

        /// <summary>一个节点的**两个独立真值**（canonical owner 只在这里产出）。</summary>
        public struct NodeTruth
        {
            public EffectTruth Effect;
            public TraversalTruth Traversal;
        }

        // 稳定的拒绝/展示原因（UI 与 TryAllocate 共用同一份文本，禁止各自维护一套）。
        public const string ReasonBlockedCurrently = "该节点含当前引擎无法完整兑现的效果";
        public const string ReasonBlockedSpecial = "该节点需要当前尚未实现的特殊交互";
        public const string ReasonMasteryPending = "专精暂未开放显式选择";
        public const string ReasonOutOfDomain = "无此节点";

        /// <summary>
        /// 真实 runtime consumer 的 StatId 全清单（**单一 owner**）。
        /// 出处（S4/S5/S6P-WO-02 逐条取证）：
        ///   · 面板/防御：SliceSession.RecalcPlayer -> PlayerStats
        ///   · 攻击结算：SliceSession.BuildPlayerHit -> HitRequest -> CombatMath.ResolveHit
        ///   · 技能形态：SliceSession.ResolveSkillDef
        ///   · 机制：ArenaSim.ResolveProjectile -> SliceSession.ForkCount
        /// census / ContentAuditS2Tests / ProductionSimulator / ProductionContentReport 只消费本数组，
        /// 不得各自复制一份（否则"能否兑现"会出现第二套判定）。
        /// </summary>
        public static readonly StatId[] RuntimeConsumedStats =
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

        public static bool IsRuntimeConsumed(StatId stat)
        {
            for (int i = 0; i < RuntimeConsumedStats.Length; i++)
                if (RuntimeConsumedStats[i] == stat)
                    return true;
            return false;
        }

        /// <summary>节点/词条级资格（唯一实现；UI、分配门、消费门、census 都调它）。</summary>
        /// <summary>
        /// 节点**两个维度的 canonical 真值**（分配门只读 Traversal、消费门只读 Effect）。
        /// 索引域外（含 403 未上树节点）返回 SPECIAL_PENDING / OUT_OF_DOMAIN。
        /// </summary>
        public static NodeTruth EvaluateTruth(int nodeId)
        {
            if (nodeId < 0 || nodeId >= PoeTree.Count)
            {
                NodeTruth ood;
                ood.Effect = EffectTruth.SpecialPending;
                ood.Traversal = TraversalTruth.OutOfDomain;
                return ood;
            }
            EnsureCache();
            if (!_known[nodeId])
            {
                PoeNode n = PoeTree.Get(nodeId);
                NodeTruth t = ClassifyTruth(n);
                _truth[nodeId] = t;
                _status[nodeId] = Project(n, t);
                _known[nodeId] = true;
            }
            return _truth[nodeId];
        }

        public static NodeTruth EvaluateTruth(PoeNode n)
        {
            return ClassifyTruth(n);
        }

        /// <summary>
        /// 04A 的四值资格。本令后**降级为 census / 诊断投影**：由 <see cref="EvaluateTruth(int)"/> 唯一派生，
        /// 不再驱动分配门，也不再驱动消费门 —— 那两个门各自只读一个维度。
        /// 保留它是为了让 04A 冻结 census（367/1660/87/315）继续可机械复核。
        /// </summary>
        public static NodeStatus EvaluateNode(int nodeId)
        {
            if (nodeId < 0 || nodeId >= PoeTree.Count)
                return NodeStatus.OutOfDomain;
            EnsureCache();
            if (!_known[nodeId])
            {
                PoeNode n = PoeTree.Get(nodeId);
                NodeTruth t = ClassifyTruth(n);
                _truth[nodeId] = t;
                _status[nodeId] = Project(n, t);
                _known[nodeId] = true;
            }
            return _status[nodeId];
        }

        public static NodeStatus EvaluateNode(PoeNode n)
        {
            return Project(n, ClassifyTruth(n));
        }

        /// <summary>通行资格（canonical 判据）：只有 TRAVERSABLE 可分配、可扣点、可作为路径。</summary>
        public static bool IsTraversable(TraversalTruth traversal)
        {
            return traversal == TraversalTruth.Traversable;
        }

        /// <summary>效果兑现资格（canonical 判据）：只有 FULLY_SUPPORTED 可贡献 modifier。</summary>
        public static bool YieldsModifiers(EffectTruth effect)
        {
            return effect == EffectTruth.FullySupported;
        }

        static void EnsureCache()
        {
            if (_truth == null || _truth.Length != PoeTree.Count)
            {
                _truth = new NodeTruth[PoeTree.Count];
                _status = new NodeStatus[PoeTree.Count];
                _known = new bool[PoeTree.Count];
            }
        }

        /// <summary>本节点不可通行时的稳定原因文本（可通行返回 null）。分配门与 UI 共用。</summary>
        public static string TraversalReason(int nodeId)
        {
            NodeTruth t = EvaluateTruth(nodeId);
            if (IsTraversable(t.Traversal))
                return null;
            return Reason(EvaluateNode(nodeId));
        }

        /// <summary>可分配返回 null；否则返回稳定原因（UI 展示与分配拒绝共用）。</summary>
        public static string Reason(NodeStatus status)
        {
            switch (status)
            {
                case NodeStatus.AllocatableSupported: return null;
                case NodeStatus.BlockedSpecialInteraction: return ReasonBlockedSpecial;
                case NodeStatus.SpecialPendingMastery: return ReasonMasteryPending;
                case NodeStatus.OutOfDomain: return ReasonOutOfDomain;
                default: return ReasonBlockedCurrently;
            }
        }

        /// <summary>
        /// 节点级判定（唯一实现）。产出的两个维度互相独立：
        ///   专精 / 珠宝孔 / 时光珠宝类 → SPECIAL_PENDING + SPECIAL_BLOCKED（§12/§17）；
        ///   只要出现一条 BLOCKED_BY_DOMAIN 行，整节点 UNFULFILLED（§14 整节点 fail closed），
        ///     但**通行不受影响**（本令的核心：路由资格与效果兑现分离）；
        ///   无 handler 的 SPECIAL_INTERACTION → SPECIAL_PENDING + SPECIAL_BLOCKED；
        ///   STRUCTURAL 行既不阻止通行也不产生承诺（§10）。
        /// </summary>
        static NodeTruth ClassifyTruth(PoeNode n)
        {
            NodeTruth t;
            if (n.Kind == PoeNodeKind.Mastery || n.Kind == PoeNodeKind.Jewel || n.locked != 0)
            {
                t.Effect = EffectTruth.SpecialPending;
                t.Traversal = TraversalTruth.SpecialBlocked;
                return t;
            }

            bool consumed = false, blocked = false, special = false, structural = false;
            ClassifyText(n.stats, ref consumed, ref blocked, ref special, ref structural);
            ClassifyText(n.choices, ref consumed, ref blocked, ref special, ref structural);

            // mixed（部分可兑现 + 部分不可兑现）必须整节点 UNFULFILLED：一条都不许漏出（§4/§14）。
            if (blocked)
            {
                t.Effect = EffectTruth.Unfulfilled;
                t.Traversal = TraversalTruth.Traversable;
                return t;
            }
            if (special)
            {
                t.Effect = EffectTruth.SpecialPending;
                t.Traversal = TraversalTruth.SpecialBlocked;
                return t;
            }
            t.Effect = EffectTruth.FullySupported;
            t.Traversal = TraversalTruth.Traversable;
            return t;
        }

        /// <summary>两个真值 → 04A 诊断四值（仅供 census/证据；不是任何门的判据）。</summary>
        static NodeStatus Project(PoeNode n, NodeTruth t)
        {
            if (t.Traversal == TraversalTruth.OutOfDomain)
                return NodeStatus.OutOfDomain;
            if (t.Traversal == TraversalTruth.SpecialBlocked)
                return n.Kind == PoeNodeKind.Mastery
                    ? NodeStatus.SpecialPendingMastery : NodeStatus.BlockedSpecialInteraction;
            return t.Effect == EffectTruth.FullySupported
                ? NodeStatus.AllocatableSupported : NodeStatus.BlockedCurrently;
        }

        static void ClassifyText(string text, ref bool consumed, ref bool blocked, ref bool special, ref bool structural)
        {
            if (string.IsNullOrEmpty(text))
                return;
            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line;
                if (nl < 0) { line = text.Substring(start); start = text.Length; }
                else { line = text.Substring(start, nl - start); start = nl + 1; }

                string domain;
                switch (ClassifyLine(line, out domain))
                {
                    case LineBucket.Consumed: consumed = true; break;
                    case LineBucket.BlockedByDomain: blocked = true; break;
                    case LineBucket.SpecialInteraction: special = true; break;
                    case LineBucket.Structural: structural = true; break;
                }
            }
        }

        /// <summary>
        /// 单行判定（唯一实现）。判定顺序：空/括号提示 → CONSUMED → 缺失轴 → 无数字的特殊交互 → UNKNOWN。
        /// 注意 STRUCTURAL 的判据是**行首就是 '('**（官方数据标记注释行的格式约定），
        /// 不是"含括号就算"：`+20% increased Damage (some note)` 行首是 '+'，必须仍按效果行判定。
        /// </summary>
        public static LineBucket ClassifyLine(string line, out string domain)
        {
            domain = null;
            if (line == null)
                return LineBucket.Structural;
            line = line.Trim();
            if (line.Length == 0)
                return LineBucket.Structural;
            if (line[0] == '(')
                return LineBucket.Structural;

            if (IsFullyConsumed(line))
                return LineBucket.Consumed;

            domain = MatchDomain(line);
            if (domain != null)
                return LineBucket.BlockedByDomain;
            if (!HasDigit(line))
                return LineBucket.SpecialInteraction;
            return LineBucket.Unknown;
        }

        /// <summary>CONSUMED = parser 命中 **且** 产出 StatId 全都有真实 runtime consumer（合同 §11）。</summary>
        static bool IsFullyConsumed(string line)
        {
            Modifier[] mods = PoeStatParser.Parse(line);
            if (mods.Length == 0)
                return false;
            for (int i = 0; i < mods.Length; i++)
                if (!IsRuntimeConsumed(mods[i].Stat))
                    return false;
            return true;
        }

        static bool HasDigit(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (s[i] >= '0' && s[i] <= '9')
                    return true;
            return false;
        }

        static string MatchDomain(string line)
        {
            for (int i = 0; i < DomainTable.GetLength(0); i++)
                if (line.IndexOf(DomainTable[i, 0], StringComparison.OrdinalIgnoreCase) >= 0)
                    return DomainTable[i, 1];
            return null;
        }

        static NodeTruth[] _truth;
        static NodeStatus[] _status;
        static bool[] _known;
        static bool[] _reach;

        /// <summary>测试注入/数据漂移后用（缓存必须与当前树数据同尺寸）。</summary>
        internal static void InvalidateCache()
        {
            _truth = null;
            _status = null;
            _known = null;
            _reach = null;
        }

        /// <summary>
        /// S6P-WO-04A2 可达性真值：从 <see cref="PoeTree.StartIndex"/> 出发、
        /// **只以 TRAVERSABLE 节点为 transit** 的图可达集合（seed 本身无条件入集）。
        ///
        /// 边只取 canonical 数据里的 <see cref="PoeNode.links"/>（无向、已去重升序），
        /// 邻接遍历按 nodeId 升序 —— 结果与枚举顺序、渲染顺序、坐标无关。
        /// 严禁用屏幕坐标 / orbit / group 归属 / 视觉连线推导邻接（§7.1）。
        /// </summary>
        public static bool[] ReachableSet()
        {
            int n = PoeTree.Count;
            if (_reach != null && _reach.Length == n)
                return _reach;
            _reach = new bool[n];
            if (n <= 0)
                return _reach;

            var queue = new int[n];
            int head = 0, tail = 0;
            int seed = PoeTree.StartIndex;
            if (seed >= 0 && seed < n)
            {
                _reach[seed] = true;
                queue[tail++] = seed;
            }
            while (head < tail)
            {
                int[] links = PoeTree.Get(queue[head++]).links;
                if (links == null)
                    continue;
                for (int i = 0; i < links.Length; i++)
                {
                    int nb = links[i];
                    if (nb < 0 || nb >= n || _reach[nb])
                        continue;
                    if (!IsTraversable(EvaluateTruth(nb).Traversal))
                        continue;
                    _reach[nb] = true;
                    queue[tail++] = nb;
                }
            }
            return _reach;
        }

        // ================= 缺失玩法轴关键词表（顺序敏感） =================
        // 越具体的轴越靠前；宽泛词（while/per/against…）放最后兜底。命中即 BLOCKED_BY_DOMAIN，
        // 轴标签是给后续周期的待办清单，不是本轮失败。
        static readonly string[,] DomainTable = new string[,]
        {
            { "energy shield", "EnergyShield" },
            { "suppress", "Suppression" },
            { "spell suppression", "Suppression" },
            { "block", "Block" },
            { "summon", "Minion" },
            { "zombie", "Minion" },
            { "skeleton", "Minion" },
            { "spectre", "Minion" },
            { "golem", "Minion" },
            { "raised", "Minion" },
            { "animated", "Minion" },
            { "minion", "Minion" },
            { "totem", "Totem" },
            { "warcry", "Warcry" },
            { "warcries", "Warcry" },
            { "tincture", "Tincture" },
            { "flask", "Flask" },
            { "retaliation", "Retaliation" },
            { "impale", "Impale" },
            { "fortif", "Fortify" },
            { "rage", "Rage" },
            { "valour", "Valour" },
            { "trap", "Trap" },
            { "mine", "Mine" },
            { "link skill", "Link" },
            { "level of all", "GemLevel" },
            { "gem", "GemLevel" },
            { "melee", "Melee" },
            { "strike range", "Range" },
            { "light radius", "Radius" },
            { "chaining", "Chaining" },
            { "chain", "Chaining" },
            { "consecrated ground", "Ground" },
            { "ground", "Ground" },
            { "phasing", "Buff" },
            { "arcane surge", "Buff" },
            { "herald", "Buff" },
            { "frozen", "Ailment" },
            { "chilled", "Ailment" },
            { "shocked", "Ailment" },
            { "reservation", "Reservation" },
            { "area damage", "AreaOfEffect" },
            { "avoid", "Avoidance" },
            { "slam", "Slam" },
            { "channel", "Channelling" },
            { "trigger", "Trigger" },
            { "cooldown", "Cooldown" },
            { "vaal", "Vaal" },
            { "manifest", "Minion" },
            { "companion", "Minion" },
            { "beast", "Minion" },
            { "withered", "Debuff" },
            { "debuff", "Debuff" },
            { "marks", "Mark" },
            { "mark effect", "Mark" },
            { "double damage", "DamageModifier" },
            { "triple damage", "DamageModifier" },
            { "cruelty", "Buff" },
            { "ghost shroud", "Buff" },
            { "plague", "Buff" },
            { "seal", "Seal" },
            { "support", "Support" },
            { "reflected", "Reflected" },
            { "reflect", "Reflected" },
            { "steel", "Steel" },
            { "corpse", "Corpse" },
            { "exert", "Exert" },
            { "damage reduction", "DamageReduction" },
            { "movement", "Movement" },
            { "fire", "Fire" },
            { "cold", "Cold" },
            { "lightning", "Lightning" },
            { "chaos", "Chaos" },
            { "poison", "Ailment" },
            { "bleed", "Ailment" },
            { "chill", "Ailment" },
            { "freeze", "Ailment" },
            { "shock", "Ailment" },
            { "scorch", "Ailment" },
            { "brittle", "Ailment" },
            { "sap", "Ailment" },
            { "ailment", "Ailment" },
            { "ignite", "Ailment" },
            { "damage over time", "DamageOverTime" },
            { "over time", "DamageOverTime" },
            { "degeneration", "DamageOverTime" },
            { "leech", "Leech" },
            { "recoup", "Recovery" },
            { "regen", "Recovery" },
            { "recover", "Recovery" },
            { "regeneration", "Recovery" },
            { "recharge", "Recovery" },
            { "charge", "Charge" },
            { "aura", "Aura" },
            { "curse", "Curse" },
            { "hex", "Curse" },
            { "brand", "Brand" },
            { "banner", "Banner" },
            { "guard skill", "GuardSkill" },
            { "stance", "Stance" },
            { "offering", "Offering" },
            { "onslaught", "Buff" },
            { "elusive", "Buff" },
            { "tailwind", "Buff" },
            { "unholy might", "Buff" },
            { "fortification", "Fortify" },
            { "stun", "Stun" },
            { "knockback", "Knockback" },
            { "taunt", "Taunt" },
            { "blind", "Blind" },
            { "maim", "Maim" },
            { "hinder", "Hinder" },
            { "corrupted blood", "Ailment" },
            { "exposure", "Exposure" },
            { "cast speed", "CastSpeed" },
            { "spell damage", "Spell" },
            { "spell critical", "Spell" },
            { "spell", "Spell" },
            { "projectile", "Projectile" },
            { "arrow", "Projectile" },
            { "bow", "WeaponType" },
            { "wand", "WeaponType" },
            { "claw", "WeaponType" },
            { "dagger", "WeaponType" },
            { "axe", "WeaponType" },
            { "mace", "WeaponType" },
            { "sceptre", "WeaponType" },
            { "sword", "WeaponType" },
            { "staff", "WeaponType" },
            { "spear", "WeaponType" },
            { "flail", "WeaponType" },
            { "unarmed", "WeaponType" },
            { "two handed", "WeaponType" },
            { "one handed", "WeaponType" },
            { "dual wield", "WeaponType" },
            { "wielding", "WeaponType" },
            { "shield", "Offhand" },
            { "quiver", "Offhand" },
            { "elemental", "Elemental" },
            { "physical", "Physical" },
            { "attack speed", "AttackSpeed" },
            { "accuracy", "Accuracy" },
            { "evasion", "Evasion" },
            { "armour", "Armour" },
            { "critical", "Critical" },
            { "resist", "Resistance" },
            { "life", "Life" },
            { "mana", "Mana" },
            { "attribute", "Attribute" },
            { "strength", "Attribute" },
            { "dexterity", "Attribute" },
            { "intelligence", "Attribute" },
            { "skill effect duration", "Duration" },
            { "duration", "Duration" },
            { "area of effect", "AreaOfEffect" },
            { "movement speed", "Movement" },
            { "skill cost", "Cost" },
            { "cost", "Cost" },
            { "reserve", "Reservation" },
            { "while", "Conditional" },
            { "per ", "Conditional" },
            { "against", "Conditional" },
            { "if you", "Conditional" },
            { "when you", "Conditional" },
            { "recently", "Conditional" },
            { "with ", "Conditional" },
            { "on hit", "Conditional" },
            { "nearby", "Conditional" },
            { "enemies", "Conditional" },
            { "enemy", "Conditional" }
        };
    }
}
