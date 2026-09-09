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
        // Sentinel（S3-M1-REPO-TRUTH-CATALOG）：真实 Support 数 = (int)Count - 1（None 不算内容）；
        // 仅作目录容量/Count 真相源，不进 UI/golden/审计内容清单，不计作内容 +1
        Count = 8
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
        Count = 17
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
        public const int InventoryCap = 24;
        public const int MaxAffixesOrdinary = 2;
        public const int MaxAffixesRare = 4;
        public const int PassiveCount = 16;
        public const int StartPoints = 8;
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

    public static class PassiveCatalog
    {
        static PassiveNode[] _nodes;

        public static PassiveNode Get(int id)
        {
            Ensure();
            if (id < 0 || id >= _nodes.Length)
                return default;
            return _nodes[id];
        }

        public static int Count
        {
            get
            {
                Ensure();
                return _nodes.Length;
            }
        }

        static void Ensure()
        {
            if (_nodes != null)
                return;
            _nodes = new PassiveNode[16];
            _nodes[0] = Node(0, "心脉", false, false, new[] { 1, 2, 3, 6 }, "+20 生命", Modifier.Make(StatId.Life, ModOp.Flat, 20f));
            _nodes[1] = Node(1, "蛮力", false, false, new[] { 0, 4, 7, 11 }, "+12 力量", Modifier.Make(StatId.Strength, ModOp.Flat, 12f));
            _nodes[2] = Node(2, "敏足", false, false, new[] { 0, 5, 9 }, "+12 敏捷", Modifier.Make(StatId.Dexterity, ModOp.Flat, 12f));
            _nodes[3] = Node(3, "灵思", false, false, new[] { 0, 8, 14 }, "+12 智力", Modifier.Make(StatId.Intelligence, ModOp.Flat, 12f));
            _nodes[4] = Node(4, "铁骨", false, false, new[] { 1, 6 }, "+30 护甲", Modifier.Make(StatId.Armour, ModOp.Flat, 30f));
            _nodes[5] = Node(5, "影蔽", false, false, new[] { 2, 9 }, "+30 闪避", Modifier.Make(StatId.Evasion, ModOp.Flat, 30f));
            _nodes[6] = Node(6, "血脉", false, false, new[] { 0, 4, 10 }, "+25 生命", Modifier.Make(StatId.Life, ModOp.Flat, 25f));
            _nodes[7] = Node(7, "粉碎", false, false, new[] { 1, 11 }, "16% 物理伤害", Modifier.Make(StatId.PhysicalDamage, ModOp.Increased, 0.16f));
            _nodes[8] = Node(8, "余烬", false, false, new[] { 3, 12, 13 }, "16% 火焰伤害", Modifier.Make(StatId.FireDamage, ModOp.Increased, 0.16f));
            _nodes[9] = Node(9, "精准", false, false, new[] { 2, 5, 10 }, "+50 命中", Modifier.Make(StatId.Accuracy, ModOp.Flat, 50f));
            _nodes[10] = Node(10, "冲击", false, false, new[] { 6, 9 }, "30% 暴击率", Modifier.Make(StatId.CritChanceIncreased, ModOp.Increased, 0.30f));
            _nodes[11] = Node(11, "残暴打击", true, false, new[] { 1, 7 }, "近战物理更多 25%", Modifier.Tagged(StatId.MorePhysical, ModOp.More, 0.25f, Tag.Melee));
            _nodes[12] = Node(12, "火葬", true, false, new[] { 8, 13 }, "30% 火焰伤害，+25% 点燃", Modifier.Make(StatId.FireDamage, ModOp.Increased, 0.30f), Modifier.Make(StatId.IgniteChance, ModOp.Flat, 0.25f));
            _nodes[13] = Node(13, "烬心", false, true, new[] { 8, 12 }, "40% 物理转火", Modifier.Make(StatId.ConvertPhysToFire, ModOp.Flat, 0.40f));
            _nodes[14] = Node(14, "厚皮", false, false, new[] { 3, 15 }, "+12% 火焰抗性", Modifier.Make(StatId.FireResistance, ModOp.Flat, 0.12f));
            _nodes[15] = Node(15, "搏动", false, false, new[] { 14 }, "12% 攻击速度", Modifier.Make(StatId.AttackSpeed, ModOp.Increased, 0.12f));
        }

        static PassiveNode Node(int id, string name, bool notable, bool mechanic, int[] links, string desc, params Modifier[] mods)
        {
            PassiveNode n;
            n.Id = id;
            n.Name = name;
            n.Notable = notable;
            n.Mechanic = mechanic;
            n.Links = links;
            n.Mods = mods;
            n.Desc = desc;
            return n;
        }
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
