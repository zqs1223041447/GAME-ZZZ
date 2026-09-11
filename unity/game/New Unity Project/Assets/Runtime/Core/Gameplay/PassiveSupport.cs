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
        public static NodeStatus EvaluateNode(int nodeId)
        {
            if (nodeId < 0 || nodeId >= PoeTree.Count)
                return NodeStatus.OutOfDomain;

            if (_status == null || _status.Length != PoeTree.Count)
            {
                _status = new NodeStatus[PoeTree.Count];
                _known = new bool[PoeTree.Count];
            }
            if (!_known[nodeId])
            {
                _status[nodeId] = ClassifyNode(PoeTree.Get(nodeId));
                _known[nodeId] = true;
            }
            return _status[nodeId];
        }

        public static NodeStatus EvaluateNode(PoeNode n)
        {
            return ClassifyNode(n);
        }

        public static bool IsAllocatable(NodeStatus status)
        {
            return status == NodeStatus.AllocatableSupported;
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
        /// 节点级判定（唯一实现）：
        ///   专精 → 过渡态不可分配；珠宝孔 / 时光珠宝类节点 → 无 handler 的特殊交互；
        ///   只要出现一条 BLOCKED_BY_DOMAIN 行，整节点 fail closed（§14）；
        ///   出现无 handler 的 SPECIAL_INTERACTION 同样整节点拒绝（§12）；
        ///   STRUCTURAL 行不阻止分配（§10）。
        /// </summary>
        static NodeStatus ClassifyNode(PoeNode n)
        {
            if (n.Kind == PoeNodeKind.Mastery)
                return NodeStatus.SpecialPendingMastery;
            if (n.Kind == PoeNodeKind.Jewel)
                return NodeStatus.BlockedSpecialInteraction;
            if (n.locked != 0)
                return NodeStatus.BlockedSpecialInteraction;

            bool consumed = false, blocked = false, special = false, structural = false;
            ClassifyText(n.stats, ref consumed, ref blocked, ref special, ref structural);
            ClassifyText(n.choices, ref consumed, ref blocked, ref special, ref structural);

            if (blocked) return NodeStatus.BlockedCurrently;
            if (special) return NodeStatus.BlockedSpecialInteraction;
            return NodeStatus.AllocatableSupported;
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

        static NodeStatus[] _status;
        static bool[] _known;

        /// <summary>测试注入/数据漂移后用（缓存必须与当前树数据同尺寸）。</summary>
        internal static void InvalidateCache()
        {
            _status = null;
            _known = null;
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
