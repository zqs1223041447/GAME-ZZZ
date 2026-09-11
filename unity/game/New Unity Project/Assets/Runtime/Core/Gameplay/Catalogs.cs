namespace Game.Runtime.Core
{
    public enum SupportId : byte
    {
        None = 0,
        AddedFire = 1,
        Brutal = 2,
        Concentrated = 3,
        Faster = 4,
        Combustion = 5,
        Fork = 6,
        // S3 R2（工作令 S3-R2-FIRE-CONVERSION）：机制型转换 Support，复用 ConvertPhysToFire，无新 Effect/Stat
        FireConversion = 7,
        // S6P-WO-05（导演 2026-09-11「辅助技能做一个投射物返回和狙击印记的效果」）：两条机制型 Support，
        // 追加在既有正式 Support 后、Count 前；既有数值 1-7 不得漂移。行为常数见 SliceRules。
        ReturningProjectiles = 8,
        SnipersMark = 9,
        // Sentinel（S3-M1-REPO-TRUTH-CATALOG）：真实 Support 数 = (int)Count - 1（None 不算内容）；
        // 仅作目录容量/Count 真相源，不进 UI/golden/审计内容清单，不计作内容 +1
        Count = 10
    }

    public enum AffixId : byte
    {
        AddPhys = 0,
        IncPhys = 1,
        AddFire = 2,
        IncFire = 3,
        Life = 4,
        Armour = 5,
        Evasion = 6,
        FireRes = 7,
        Accuracy = 8,
        Crit = 9,
        // S3 第一批：组合系（全部复用已有 StatId/ModOp，≤3 条）
        IgniteFire = 10,
        AccCrit = 11,
        PhysFire = 12,
        // S4-P3（工作令 S4-P3-AFFIX-APPLICABILITY-BREADTH）：Gloves/Belt 定向词缀（全部复用已有 StatId/ModOp，2-4 条；旧 ID 只追加不漂移）
        SwiftGrip = 13,
        KeenEdge = 14,
        Bulwark = 15,
        VitalWeave = 16,
        // S5-WO-04（工作令 S5-WO-04-BOUNDED-AFFIX-BREADTH，BL-002.A1）：锁定清单恰 4 条（权威清单 docs/reviews/S5/S5_AFFIX_ADMISSION.md；
        // 17→21；全部复用既有 StatId/ModOp 单行词缀；旧 ID 只追加不漂移）
        SwiftBreeze = 17, // 迅疾 AttackSpeed/Increased 0.10-0.16 不限槽
        Ironhide = 18,    // 铁骨 Armour/Increased 0.10-0.22 排除 Belt（GAME-ZZZ 有界适用性决策，非 PoEDB 禁令）
        Insight = 19,     // 睿智 Intelligence/Flat 6-12 不限槽
        Tenacity = 20,    // 坚韧 Strength/Flat 6-12 不限槽（原开阔 AreaRadiusMore/More 候选已按裁定废弃）
        Count = 21
    }

    public enum EquipSlot : byte
    {
        Weapon = 0,
        Body = 1,
        Helmet = 2,
        Boots = 3,
        // S4-P2（工作令 S4-P2-EQUIPMENT-BREADTH-GLOVES-BELT）：追加在既有正式槽后、Count 前；既有数值 0-3 不得漂移
        Gloves = 4,
        Belt = 5,
        Count = 6
    }

    public enum Rarity : byte
    {
        Ordinary = 0,
        Rare = 1
    }

    public enum EnemyKind : byte
    {
        Dummy = 0,
        Brute = 1,
        Stinger = 2,
        Ashling = 3,
        Warden = 4
    }

    public struct SupportDef
    {
        public SupportId Id;
        public string Name;
        public string Desc;
        public bool ChangesMechanism;
        public Modifier[] Mods;
        public EffectId TriggerEffect;
        public EventId TriggerEvent;
        public int TriggerDepth;
        // 机制类 Support 的技能限制（SkillId.None = 不限）；S3-B1-RCLOSE 运行时兼容门据此判定（分裂只接弹道）
        public SkillId MechanicSkill;
    }

    public struct AffixDef
    {
        public AffixId Id;
        public string Name;
        public StatId Stat;
        public ModOp Op;
        public float Min;
        public float Max;
        public string Format;
        // 第二行（组合词缀）：Format2 为空 = 单行词缀；非空 = 组合第二行（Stat2/Op2 + Min2/Max2 独立掷值）
        public StatId Stat2;
        public ModOp Op2;
        public float Min2;
        public float Max2;
        public string Format2;
        // S4-P3（工作令 S4-P3-AFFIX-APPLICABILITY-BREADTH，Branch B）：槽位适用性 bit mask——bit i = EquipSlot i。
        // 0 = 不限槽（既有 13 词缀行为完全不变的默认语义）。唯一 eligibility truth = IsApplicable()；
        // 只决定「Affix 能否出现在该 EquipSlot 的 roll 池」，不得决定数值/结算/rarity/穿戴资格。
        public ushort AllowedSlots;

        public int RowCount
        {
            get { return string.IsNullOrEmpty(Format2) ? 1 : 2; }
        }

        public StatId RowStat(int row)
        {
            return row == 0 ? Stat : Stat2;
        }

        public ModOp RowOp(int row)
        {
            return row == 0 ? Op : Op2;
        }

        /// <summary>唯一 canonical applicability 判定（Drop / Random Craft / Directed Craft / 审计 / 报告全部复用，禁止各自再判一遍）。</summary>
        public bool IsApplicable(EquipSlot slot)
        {
            return AllowedSlots == 0 || (AllowedSlots & (ushort)(1 << (int)slot)) != 0;
        }
    }

    public struct PassiveNode
    {
        public int Id;
        public string Name;
        public bool Notable;
        public bool Mechanic;
        public int[] Links;
        public Modifier[] Mods;
        public string Desc;
    }

    public struct EnemyDef
    {
        public EnemyKind Kind;
        public string Name;
        public int Life;
        public float Armour;
        public float Evasion;
        public float FireRes;
        public float Accuracy;
        public int AttackPhys;
        public int AttackFire;
        public float AttackCd;
        public float Speed;
        public float Scale;
        public bool Elite;
    }

    public struct MapAffixDef
    {
        public int Id;
        public string Name;
        public string Desc;
        public int StabilityCost;
        public float RewardAdd;
        public float MonsterLifeMore;
        public float MonsterDamageMore;
        public float PlayerFireResFlat;
        public float MonsterAddedFire;
    }

    public struct ItemInstance
    {
        public int Id;
        public EquipSlot Slot;
        public Rarity Rarity;
        public int SocketCount;
        public string BaseName;
        public int AffixCount;
        public AffixId Affix0;
        public float Value0;
        public AffixId Affix1;
        public float Value1;
        public AffixId Affix2;
        public float Value2;
        public AffixId Affix3;
        public float Value3;
        // 组合词缀第二行值（与 Affix0..3 一一对应；单行词缀恒为 0）
        public float SecondValue0;
        public float SecondValue1;
        public float SecondValue2;
        public float SecondValue3;
        // S5-WO-02（BL-021.A2，合同 docs/reviews/S5/S5_LINK_CONTRACT.md）：可选第二连接组绑定技能。
        // None（=0）= 单连接 legacy（全部 SocketCount 孔归 group 0，行为与既有逐位等价）；非 None 且 SocketCount>=3 时
        // group 0=前部 SocketCount-2 孔、group 1=末尾 2 孔（改挂该技能的连接；每技能至多一个有效连接源）。
        // 写入唯一入口=SliceSession.TryReassignLink（映射技能自改挂/同技能双物品改挂/容量溢出=原子拒绝，无半写入）。
        public SkillId LinkSkill1;

        public int AffixIdAt(int i)
        {
            if (i == 0) return (int)Affix0;
            if (i == 1) return (int)Affix1;
            if (i == 2) return (int)Affix2;
            return (int)Affix3;
        }

        public float ValueAt(int i)
        {
            if (i == 0) return Value0;
            if (i == 1) return Value1;
            if (i == 2) return Value2;
            return Value3;
        }

        public float SecondValueAt(int i)
        {
            if (i == 0) return SecondValue0;
            if (i == 1) return SecondValue1;
            if (i == 2) return SecondValue2;
            return SecondValue3;
        }

        public void SetAffix(int i, AffixId id, float value, float secondValue)
        {
            if (i == 0) { Affix0 = id; Value0 = value; SecondValue0 = secondValue; }
            else if (i == 1) { Affix1 = id; Value1 = value; SecondValue1 = secondValue; }
            else if (i == 2) { Affix2 = id; Value2 = value; SecondValue2 = secondValue; }
            else { Affix3 = id; Value3 = value; SecondValue3 = secondValue; }
        }
    }

    public static class SliceRules
    {
        /// <summary>2026-09-10 导演：背包满幅格网（12 列 × 8 行）——容量随呈现格局扩大。</summary>
        public const int InventoryCap = 96;
        public const int MaxAffixesOrdinary = 2;
        public const int MaxAffixesRare = 4;
        /// <summary>天赋域大小 = 真实 PoE 天赋树节点数（数据缺失时退化为 1，保证不崩）。</summary>
        public static int PassiveCount
        {
            get { return PoeTree.Count > 0 ? PoeTree.Count : 1; }
        }
        /// <summary>起始天赋点 = 官方天赋树总点数（123）。旧 16 节点域用 8；2429 节点域下 8 点点不动任何基石。</summary>
        public const int StartPoints = 123;
        // 导演 2026-09-08：QA/游玩期玩家血量提升至 9999999（原 80）——玩家不再被围杀打断表现验证
        public const float PlayerBaseLife = 9999999f;
        public const float PlayerBaseMana = 40f;
        public const float PlayerBaseAccuracy = 120f;
        public const float PlayerBaseArmour = 10f;
        public const float PlayerBaseEvasion = 20f;
        public const float LifePerStr = 0.5f;
        public const float ManaPerInt = 0.5f;
        public const float AccuracyPerDex = 2f;
        public const float ManaRegen = 8f;
        public const float BaseCrit = 0.05f;
        public const int WeaponSockets = 3;
        public const int BodySockets = 3;
        public const int HelmetSockets = 2;
        public const int BootsSockets = 0;
        // S4-P2：新槽孔数（不映射任何技能孔位——SupportCapacity 仍只走 Weapon/Body/Helmet）
        public const int GlovesSockets = 1;
        public const int BeltSockets = 1;
        public const int MapSeed = unchecked((int)0xC0FFEE);
        public const int BaseStability = 100;
        public const int DeathStabilityLoss = 10;
        public const int DeathStabilityPerAffix = 5;
        public const int ClearStabilityLoss = 3;
        public const float DropOrdinary = 0.55f;
        public const float DropRare = 0.22f;
        public const float DropScrap = 0.40f;
        public const float DropEtching = 0.18f;
        public const int MeleeBase = 8;
        public const int ProjectileBase = 7;
        public const int AreaBase = 10;
        // S6P-WO-05：新技能进图基础伤害（与既有三技能同轴，不新增伤害类型）
        public const int IceSpearBase = 6;
        public const int FireballBase = 8;
        // S6P-WO-05：狙击印记数值（唯一真相源=此处；SupportDef.Desc 由它派生，避免两处数字漂移）
        public const float SnipersMarkMoreDamage = 0.35f;
        public const float SnipersMarkDuration = 8f;
        public const int SandboxMelee = 1;
        public const int SandboxProjectile = 1;
        public const int SandboxArea = 2;
    }

    public static class SupportCatalog
    {
        static SupportDef[] _defs;

        public static SupportDef Get(SupportId id)
        {
            Ensure();
            int i = (int)id;
            if (i <= 0 || i >= _defs.Length)
                return default;
            return _defs[i];
        }

        public static int Count
        {
            get { return (int)SupportId.Count - 1; } // sentinel 派生：真实 Support 数（None 不算内容）
        }

        static void Ensure()
        {
            if (_defs != null)
                return;
            _defs = new SupportDef[(int)SupportId.Count];
            _defs[(int)SupportId.AddedFire] = new SupportDef
            {
                Id = SupportId.AddedFire,
                Name = "燃烧",
                Desc = "+10 附加火焰",
                Mods = new[] { Modifier.Make(StatId.AddedFire, ModOp.Flat, 10f) }
            };
            _defs[(int)SupportId.Brutal] = new SupportDef
            {
                Id = SupportId.Brutal,
                Name = "残暴",
                Desc = "40% 更多物理",
                Mods = new[] { Modifier.Make(StatId.MorePhysical, ModOp.More, 0.40f) }
            };
            _defs[(int)SupportId.Concentrated] = new SupportDef
            {
                Id = SupportId.Concentrated,
                Name = "集中",
                Desc = "范围缩小 30%，范围伤害提高 40%",
                Mods = new[]
                {
                    Modifier.Tagged(StatId.AreaRadiusMore, ModOp.More, -0.30f, Tag.Area),
                    Modifier.Tagged(StatId.AreaDamageMore, ModOp.More, 0.40f, Tag.Area)
                }
            };
            _defs[(int)SupportId.Faster] = new SupportDef
            {
                Id = SupportId.Faster,
                Name = "迅捷",
                Desc = "20% 攻击速度",
                Mods = new[] { Modifier.Make(StatId.AttackSpeed, ModOp.Increased, 0.20f) }
            };
            _defs[(int)SupportId.Combustion] = new SupportDef
            {
                Id = SupportId.Combustion,
                Name = "燃尽",
                Desc = "50% 火焰伤害，必定点燃",
                Mods = new[]
                {
                    Modifier.Make(StatId.FireDamage, ModOp.Increased, 0.50f),
                    Modifier.Make(StatId.IgniteChance, ModOp.Flat, 1f)
                },
                TriggerEffect = EffectId.ApplyIgnite,
                TriggerEvent = EventId.OnHit,
                TriggerDepth = 1
            };
            _defs[(int)SupportId.Fork] = new SupportDef
            {
                Id = SupportId.Fork,
                Name = "分裂",
                Desc = "弹道命中后分裂为 2",
                ChangesMechanism = true,
                Mods = new[] { Modifier.Make(StatId.Fork, ModOp.Flat, 1f) },
                TriggerEffect = EffectId.ForkProjectiles,
                TriggerEvent = EventId.OnHit,
                TriggerDepth = 1,
                MechanicSkill = SkillId.Projectile
            };
            _defs[(int)SupportId.FireConversion] = new SupportDef
            {
                Id = SupportId.FireConversion,
                Name = "火焰转化",
                Desc = "50% 物理伤害转换为火焰伤害",
                ChangesMechanism = true,
                // 唯一核心 Modifier：转换走既有 ConvertPhysToFire 轴；RequiredTags 让兼容性由 Tag 路径自然推导（近战/弹道可接，范围=Spell 无 Attack 不可接）
                Mods = new[] { Modifier.Tagged(StatId.ConvertPhysToFire, ModOp.Flat, 0.50f, Tag.Attack | Tag.Hit | Tag.Physical) }
            };
            // S6P-WO-05：投射物返回（PoE Returning Projectiles Support）——命中或飞完全程后掉头返回，
            // 返程可再命中（同一目标不重复结算），回到玩家处消失。纯机制，无 Stat 轴 Mod。
            _defs[(int)SupportId.ReturningProjectiles] = new SupportDef
            {
                Id = SupportId.ReturningProjectiles,
                Name = "投射物返回",
                Desc = "投射物命中或到达射程尽头后返回，返程可再次命中（同一目标不重复）",
                ChangesMechanism = true,
                Mods = new Modifier[0],
                MechanicSkill = SkillId.Projectile
            };
            // S6P-WO-05：狙击印记（PoE Sniper's Mark）——支持的投射物命中时给目标打上印记，
            // 被印记的敌人受到该玩家投射物的伤害提高（+SliceRules.SnipersMarkMoreDamage），持续 SliceRules.SnipersMarkDuration。
            _defs[(int)SupportId.SnipersMark] = new SupportDef
            {
                Id = SupportId.SnipersMark,
                Name = "狙击印记",
                Desc = "投射物命中时施加印记，被印记敌人受到的投射物伤害提高 " +
                       (int)(SliceRules.SnipersMarkMoreDamage * 100f) + "%",
                ChangesMechanism = true,
                Mods = new Modifier[0],
                MechanicSkill = SkillId.Projectile
            };
        }
    }

    public static class AffixCatalog
    {
        static AffixDef[] _defs;

        public static AffixDef Get(AffixId id)
        {
            Ensure();
            return _defs[(int)id];
        }

        public static int Count
        {
            get { return (int)AffixId.Count; }
        }

        static void Ensure()
        {
            if (_defs != null)
                return;
            _defs = new AffixDef[(int)AffixId.Count];
            _defs[(int)AffixId.AddPhys] = new AffixDef { Id = AffixId.AddPhys, Name = "附加物理", Stat = StatId.AddedPhysical, Op = ModOp.Flat, Min = 3f, Max = 8f, Format = "+{0:0} 物理" };
            _defs[(int)AffixId.IncPhys] = new AffixDef { Id = AffixId.IncPhys, Name = "物理伤害", Stat = StatId.PhysicalDamage, Op = ModOp.Increased, Min = 0.12f, Max = 0.28f, Format = "{0:0%} 物理伤害" };
            _defs[(int)AffixId.AddFire] = new AffixDef { Id = AffixId.AddFire, Name = "附加火焰", Stat = StatId.AddedFire, Op = ModOp.Flat, Min = 4f, Max = 10f, Format = "+{0:0} 火焰" };
            _defs[(int)AffixId.IncFire] = new AffixDef { Id = AffixId.IncFire, Name = "火焰伤害", Stat = StatId.FireDamage, Op = ModOp.Increased, Min = 0.12f, Max = 0.28f, Format = "{0:0%} 火焰伤害" };
            _defs[(int)AffixId.Life] = new AffixDef { Id = AffixId.Life, Name = "生命", Stat = StatId.Life, Op = ModOp.Flat, Min = 12f, Max = 28f, Format = "+{0:0} 生命" };
            _defs[(int)AffixId.Armour] = new AffixDef { Id = AffixId.Armour, Name = "护甲", Stat = StatId.Armour, Op = ModOp.Flat, Min = 15f, Max = 45f, Format = "+{0:0} 护甲" };
            _defs[(int)AffixId.Evasion] = new AffixDef { Id = AffixId.Evasion, Name = "闪避", Stat = StatId.Evasion, Op = ModOp.Flat, Min = 15f, Max = 45f, Format = "+{0:0} 闪避" };
            _defs[(int)AffixId.FireRes] = new AffixDef { Id = AffixId.FireRes, Name = "火焰抗性", Stat = StatId.FireResistance, Op = ModOp.Flat, Min = 0.08f, Max = 0.18f, Format = "+{0:0%} 火焰抗性" };
            _defs[(int)AffixId.Accuracy] = new AffixDef { Id = AffixId.Accuracy, Name = "命中", Stat = StatId.Accuracy, Op = ModOp.Flat, Min = 20f, Max = 60f, Format = "+{0:0} 命中" };
            _defs[(int)AffixId.Crit] = new AffixDef { Id = AffixId.Crit, Name = "暴击率", Stat = StatId.CritChanceIncreased, Op = ModOp.Increased, Min = 0.20f, Max = 0.50f, Format = "{0:0%} 暴击率" };
            // S3 第一批组合词缀：全部由已有 StatId/ModOp 组成，无新 Stat/ModOp/Effect/Tag
            _defs[(int)AffixId.IgniteFire] = new AffixDef
            {
                Id = AffixId.IgniteFire, Name = "灼燃",
                Stat = StatId.FireDamage, Op = ModOp.Increased, Min = 0.12f, Max = 0.28f, Format = "{0:0%} 火焰伤害",
                Stat2 = StatId.IgniteChance, Op2 = ModOp.Flat, Min2 = 0.10f, Max2 = 0.20f, Format2 = "点燃几率 {0:0%}"
            };
            _defs[(int)AffixId.AccCrit] = new AffixDef
            {
                Id = AffixId.AccCrit, Name = "锐击",
                Stat = StatId.Accuracy, Op = ModOp.Flat, Min = 20f, Max = 50f, Format = "+{0:0} 命中",
                Stat2 = StatId.CritChanceIncreased, Op2 = ModOp.Increased, Min2 = 0.15f, Max2 = 0.30f, Format2 = "暴击率 {0:0%}"
            };
            _defs[(int)AffixId.PhysFire] = new AffixDef
            {
                Id = AffixId.PhysFire, Name = "熔铸",
                Stat = StatId.PhysicalDamage, Op = ModOp.Increased, Min = 0.12f, Max = 0.28f, Format = "{0:0%} 物理伤害",
                Stat2 = StatId.FireDamage, Op2 = ModOp.Increased, Min2 = 0.12f, Max2 = 0.28f, Format2 = "火焰伤害 {0:0%}"
            };
            // S4-P3 第一批（Gloves/Belt 定向，AllowedSlots 限定；全部复用已有 StatId/ModOp——运行期消费实证见 ContentAuditS2Tests.RuntimeConsumedStats）
            _defs[(int)AffixId.SwiftGrip] = new AffixDef
            {
                Id = AffixId.SwiftGrip, Name = "迅握",
                Stat = StatId.AttackSpeed, Op = ModOp.Increased, Min = 0.08f, Max = 0.14f, Format = "{0:0%} 攻击速度",
                AllowedSlots = SlotsMask(EquipSlot.Gloves)
            };
            _defs[(int)AffixId.KeenEdge] = new AffixDef
            {
                Id = AffixId.KeenEdge, Name = "锋锐",
                Stat = StatId.CritChanceAdded, Op = ModOp.Flat, Min = 0.03f, Max = 0.07f, Format = "+{0:0%} 暴击率",
                Stat2 = StatId.Accuracy, Op2 = ModOp.Flat, Min2 = 15f, Max2 = 35f, Format2 = "+{0:0} 命中",
                AllowedSlots = SlotsMask(EquipSlot.Gloves)
            };
            _defs[(int)AffixId.Bulwark] = new AffixDef
            {
                Id = AffixId.Bulwark, Name = "壁垒",
                Stat = StatId.Armour, Op = ModOp.Increased, Min = 0.10f, Max = 0.22f, Format = "{0:0%} 护甲",
                AllowedSlots = SlotsMask(EquipSlot.Belt)
            };
            _defs[(int)AffixId.VitalWeave] = new AffixDef
            {
                Id = AffixId.VitalWeave, Name = "韧脉",
                Stat = StatId.Life, Op = ModOp.Flat, Min = 15f, Max = 30f, Format = "+{0:0} 生命",
                Stat2 = StatId.FireResistance, Op2 = ModOp.Flat, Min2 = 0.05f, Max2 = 0.10f, Format2 = "+{0:0%} 火焰抗性",
                AllowedSlots = SlotsMask(EquipSlot.Belt)
            };
            // S5-WO-04 第一批（BL-002.A1 锁定清单 N=4；全部复用既有 StatId/ModOp；单一 applicability truth；
            // PoEDB 人工溯源与逐条准入自查见 docs/reviews/S5/S5_AFFIX_ADMISSION.md）
            _defs[(int)AffixId.SwiftBreeze] = new AffixDef
            {
                Id = AffixId.SwiftBreeze, Name = "迅疾",
                Stat = StatId.AttackSpeed, Op = ModOp.Increased, Min = 0.10f, Max = 0.16f, Format = "{0:0%} 攻击速度"
            };
            _defs[(int)AffixId.Ironhide] = new AffixDef
            {
                Id = AffixId.Ironhide, Name = "铁骨",
                Stat = StatId.Armour, Op = ModOp.Increased, Min = 0.10f, Max = 0.22f, Format = "{0:0%} 护甲",
                AllowedSlots = SlotsMask(EquipSlot.Weapon, EquipSlot.Body, EquipSlot.Helmet, EquipSlot.Gloves, EquipSlot.Boots)
            };
            _defs[(int)AffixId.Insight] = new AffixDef
            {
                Id = AffixId.Insight, Name = "睿智",
                Stat = StatId.Intelligence, Op = ModOp.Flat, Min = 6f, Max = 12f, Format = "+{0:0} 智力"
            };
            _defs[(int)AffixId.Tenacity] = new AffixDef
            {
                Id = AffixId.Tenacity, Name = "坚韧",
                Stat = StatId.Strength, Op = ModOp.Flat, Min = 6f, Max = 12f, Format = "+{0:0} 力量"
            };
        }

        /// <summary>S4-P3：EquipSlot 组合转 AllowedSlots bit mask（canonical 构造入口，避免手写位运算出错）。</summary>
        public static ushort SlotsMask(params EquipSlot[] slots)
        {
            ushort mask = 0;
            if (slots == null)
                return mask;
            for (int i = 0; i < slots.Length; i++)
                mask |= (ushort)(1 << (int)slots[i]);
            return mask;
        }
    }

    /// <summary>
    /// 天赋目录 = 真实 PoE 天赋树（S5U 导演插入周期 2026-09-10 换域）。
    /// 节点身份/坐标/连线/词条来自 PoeTree 数据层；Mods 由 PoeStatParser 把 PoE 词条文本
    /// 映射到 canonical StatId（映射不上的词条只做展示，不产生数值）。
    /// 逐节点惰性构造 + 缓存：2429 个节点不会在启动时全部解析。
    /// </summary>
    public static class PassiveCatalog
    {
        static PassiveNode[] _cache;

        public static PassiveNode Get(int id)
        {
            if (PoeTree.Count <= 0 || id < 0 || id >= PoeTree.Count)
                return default;
            if (_cache == null || _cache.Length != PoeTree.Count)
                _cache = new PassiveNode[PoeTree.Count];
            if (_cache[id].Links == null)
                _cache[id] = Build(PoeTree.Get(id));
            return _cache[id];
        }

        public static int Count
        {
            get { return PoeTree.Count; }
        }

        static PassiveNode Build(PoeNode src)
        {
            PassiveNode n;
            n.Id = src.index;
            n.Name = src.name;
            n.Notable = src.Kind == PoeNodeKind.Notable;
            n.Mechanic = src.Kind == PoeNodeKind.Keystone;
            n.Links = src.links ?? EmptyLinks;
            // S6P-WO-04A §17：专精在 WO-03 建立显式选择前**不得**有任何隐式效果 ——
            // 旧行为（把 choices 首条当默认 Mods 烘焙）就是"点亮了但玩家没选过"的静默半效果，已拆除。
            n.Mods = src.Kind == PoeNodeKind.Mastery ? EmptyMods : PoeStatParser.ParseCached(src.stats);
            n.Desc = src.Kind == PoeNodeKind.Mastery ? src.choices : src.stats;
            return n;
        }

        static readonly Modifier[] EmptyMods = new Modifier[0];

        /// <summary>专精节点的全部可选效果（换行分隔；tooltip 展示用，无则空）。</summary>
        public static string Choices(int id)
        {
            return PoeTree.Get(id).choices ?? "";
        }

        static readonly int[] EmptyLinks = new int[0];
    }

    public static class EnemyCatalog
    {
        public static EnemyDef Get(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Brute:
                    return new EnemyDef { Kind = kind, Name = "蛮兵", Life = 36, Armour = 80, Evasion = 10, FireRes = 0f, Accuracy = 90, AttackPhys = 7, AttackFire = 0, AttackCd = 1.15f, Speed = 1.7f, Scale = 1.05f };
                case EnemyKind.Stinger:
                    return new EnemyDef { Kind = kind, Name = "刺蜂", Life = 20, Armour = 0, Evasion = 140, FireRes = 0f, Accuracy = 110, AttackPhys = 5, AttackFire = 0, AttackCd = 0.85f, Speed = 3.1f, Scale = 0.85f };
                case EnemyKind.Ashling:
                    return new EnemyDef { Kind = kind, Name = "烬灵", Life = 24, Armour = 20, Evasion = 40, FireRes = 0.40f, Accuracy = 95, AttackPhys = 3, AttackFire = 5, AttackCd = 1.05f, Speed = 2.2f, Scale = 0.95f };
                case EnemyKind.Warden:
                    return new EnemyDef { Kind = kind, Name = "监守", Life = 110, Armour = 140, Evasion = 50, FireRes = 0.20f, Accuracy = 120, AttackPhys = 12, AttackFire = 4, AttackCd = 0.95f, Speed = 1.9f, Scale = 1.55f, Elite = true };
                default:
                    return new EnemyDef { Kind = EnemyKind.Dummy, Name = "木桩", Life = CombatRules.DummyHp, AttackCd = 99f, Scale = 1f };
            }
        }
    }

    public static class MapAffixCatalog
    {
        public static readonly MapAffixDef[] All =
        {
            new MapAffixDef { Id = 0, Name = "壮硕", Desc = "怪物生命 +40%", StabilityCost = 15, RewardAdd = 0.35f, MonsterLifeMore = 0.40f },
            new MapAffixDef { Id = 1, Name = "凶残", Desc = "怪物伤害 +30%", StabilityCost = 20, RewardAdd = 0.40f, MonsterDamageMore = 0.30f },
            new MapAffixDef { Id = 2, Name = "灰幕", Desc = "玩家火抗 -20%，怪物附加火焰", StabilityCost = 25, RewardAdd = 0.55f, PlayerFireResFlat = -0.20f, MonsterAddedFire = 6f }
        };
    }
}
