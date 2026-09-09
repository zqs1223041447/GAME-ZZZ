using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    public enum SlicePanel : byte
    {
        None = 0,
        Build = 1,
        Map = 2,
        Craft = 3
    }

    public enum MapState : byte
    {
        Town = 0,
        InMap = 1,
        Dead = 2,
        Cleared = 3
    }

    public struct BuildSnapshot
    {
        public bool Locked;
        public int WeaponId;
        public int BodyId;
        public int HelmetId;
        public int BootsId;
        // S4-P2：6 槽快照（旧四槽字段保持原名原序，新增两个只追加）
        public int GlovesId;
        public int BeltId;
        public SupportId Q0, Q1;
        public SupportId W0, W1;
        public SupportId E0;
        public int PassiveMask;
        public int Unspent;
    }

    public sealed class SliceSession
    {
        public readonly StatBag PlayerStats = new StatBag();
        public readonly TriggerSystem Triggers = new TriggerSystem();
        public readonly ItemInstance[] Inventory = new ItemInstance[SliceRules.InventoryCap];
        public int InventoryCount;
        public int NextItemId = 1;

        public int[] Equipped = new int[(int)EquipSlot.Count]; // S4-P2：随 EquipSlot.Count 派生（6 槽），旧四槽 ID 稳定
        public SupportId[] QSupports = new SupportId[2];
        public SupportId[] WSupports = new SupportId[2];
        public SupportId[] ESupports = new SupportId[1];

        public readonly bool[] Allocated = new bool[SliceRules.PassiveCount];
        public int Unspent = SliceRules.StartPoints;
        public int TotalPoints = SliceRules.StartPoints;

        public int Scrap;
        public int Etching;
        public int SelectedInv;
        public int CraftAffixPick;
        public SkillId SelectedSkill = SkillId.Melee;

        public bool[] MapAffixOn = new bool[3];
        public MapState State;
        public BuildSnapshot Snapshot;
        public SlicePanel Panel;

        public float Life;
        public float MaxLife;
        public float Mana;
        public float MaxMana;
        public int Strength;
        public int Dexterity;
        public int Intelligence;
        public bool Alive = true;
        public float HitFlash;
        public string LastMessage = "S2 Town. Tab Build · F6 Map · F8 Craft";
        public string LastLoot = "";
        public int Kills;
        public int LastDamageDealt;
        public bool LastWasFork;
        public bool LastWasIgnite;
        public bool LastWasCrit;
        public float RewardMultiplier = 1f;
        public int Integrity = SliceRules.BaseStability;
        public int Stability = SliceRules.BaseStability;

        public SeededRng LootRng;
        public SeededRng CombatRng;
        public uint SessionSeed = CombatRules.ArenaSeed;

        public int ForkSpawns;
        public int IgniteApplies;

        public SliceSession()
        {
            ResetTown(CombatRules.ArenaSeed);
        }

        public bool BuildLocked
        {
            get { return State == MapState.InMap; }
        }

        public bool OnMap
        {
            get { return State == MapState.InMap || State == MapState.Cleared; }
        }

        public string StatusCopy
        {
            get
            {
                if (State == MapState.InMap)
                    return SliceCopy.CombatLocked;
                if (State == MapState.Cleared)
                    return SliceCopy.Cleared;
                if (State == MapState.Dead)
                    return SliceCopy.DeadRespec;
                return SliceCopy.TownFree;
            }
        }

        public void ResetTown(uint seed)
        {
            SessionSeed = seed;
            LootRng = new SeededRng(seed);
            CombatRng = new SeededRng(seed ^ 0xA5A5u);
            InventoryCount = 0;
            NextItemId = 1;
            for (int i = 0; i < Equipped.Length; i++)
                Equipped[i] = -1;
            QSupports[0] = QSupports[1] = SupportId.None;
            WSupports[0] = WSupports[1] = SupportId.None;
            ESupports[0] = SupportId.None;
            for (int i = 0; i < Allocated.Length; i++)
                Allocated[i] = false;
            Allocated[0] = true;
            Unspent = SliceRules.StartPoints;
            TotalPoints = SliceRules.StartPoints;
            Scrap = 2;
            Etching = 1;
            SelectedInv = 0;
            CraftAffixPick = 0;
            State = MapState.Town;
            Snapshot = default;
            Panel = SlicePanel.None;
            Alive = true;
            Kills = 0;
            ForkSpawns = 0;
            IgniteApplies = 0;
            MapAffixOn[0] = MapAffixOn[1] = MapAffixOn[2] = false;
            Integrity = SliceRules.BaseStability;
            GiveStarterItems();
            RecalcPlayer(true);
            RefreshReward();
            LastMessage = SliceCopy.TownFree;
        }

        void GiveStarterItems()
        {
            var rng = new SeededRng(SessionSeed ^ 0x1111u);
            AddItem(RollItem(EquipSlot.Weapon, Rarity.Ordinary, rng, SliceRules.WeaponSockets, ItemBaseName(EquipSlot.Weapon)));
            AddItem(RollItem(EquipSlot.Body, Rarity.Ordinary, rng, SliceRules.BodySockets, ItemBaseName(EquipSlot.Body)));
            AddItem(RollItem(EquipSlot.Helmet, Rarity.Ordinary, rng, SliceRules.HelmetSockets, ItemBaseName(EquipSlot.Helmet)));
            AddItem(RollItem(EquipSlot.Boots, Rarity.Ordinary, rng, SliceRules.BootsSockets, ItemBaseName(EquipSlot.Boots)));
            Equipped[(int)EquipSlot.Weapon] = 0;
            Equipped[(int)EquipSlot.Body] = 1;
            Equipped[(int)EquipSlot.Helmet] = 2;
            Equipped[(int)EquipSlot.Boots] = 3;
        }

        public ItemInstance RollItem(EquipSlot slot, Rarity rarity, SeededRng rng, int sockets, string baseName)
        {
            ItemInstance it = default;
            it.Id = NextItemId++;
            it.Slot = slot;
            it.Rarity = rarity;
            it.SocketCount = sockets;
            it.BaseName = baseName;
            int n = rarity == Rarity.Rare ? RngUtil.NextInt(rng, 3, 5) : 2;
            it.AffixCount = n;
            // S4-P3：slot-eligible 候选池（canonical catalog 顺序，禁 HashSet 迭代序）；唯一 truth=AffixDef.IsApplicable。
            // 不重复选取=候选池缩池；六槽全局池恒非空（≥13 条不限槽词缀）。
            var eligible = new List<int>((int)AffixId.Count);
            for (int i = 0; i < (int)AffixId.Count; i++)
            {
                if (AffixCatalog.Get((AffixId)i).IsApplicable(slot))
                    eligible.Add(i);
            }
            for (int i = 0; i < n && eligible.Count > 0; i++)
            {
                int pick = RngUtil.NextInt(rng, 0, eligible.Count);
                int id = eligible[pick];
                eligible.RemoveAt(pick);
                AffixDef def = AffixCatalog.Get((AffixId)id);
                float v = RngUtil.Range(rng, def.Min, def.Max);
                float v2 = def.RowCount > 1 ? RngUtil.Range(rng, def.Min2, def.Max2) : 0f;
                it.SetAffix(i, def.Id, v, v2);
            }

            return it;
        }

        public int AddItem(ItemInstance item)
        {
            if (InventoryCount >= Inventory.Length)
                return -1;
            Inventory[InventoryCount] = item;
            InventoryCount++;
            return InventoryCount - 1;
        }

        public bool TryEquip(int invIndex, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            if (invIndex < 0 || invIndex >= InventoryCount)
            {
                error = "无此物品";
                return false;
            }

            ItemInstance it = Inventory[invIndex];
            int slot = (int)it.Slot;
            int old = Equipped[slot];
            Equipped[slot] = invIndex;
            ClampSupportsToSockets();
            RecalcPlayer(false);
            LastMessage = "装备 " + it.BaseName;
            if (old >= 0 && old != invIndex)
                LastMessage += "（替换）";
            return true;
        }

        public bool TrySetSupport(SkillId skill, int index, SupportId support, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            SupportId[] arr = SupportsOf(skill);
            if (arr == null || index < 0 || index >= arr.Length)
            {
                error = "孔位不存在";
                return false;
            }

            int cap = SupportCapacity(skill);
            if (index >= cap)
            {
                error = "该技能孔不足（看装备孔数）";
                return false;
            }

            if (support != SupportId.None && IsSupportUsed(support, skill, index))
            {
                error = SliceCopy.SupportInUse;
                return false;
            }

            // S3-B1-RCLOSE：兼容门在写入前判定，失败不产生任何半写入
            if (support != SupportId.None && !IsSupportCompatible(support, skill))
            {
                error = SupportCatalog.Get(support).Name + " 与 " + SkillDisplayName(skill) + " 不兼容";
                return false;
            }

            arr[index] = support;
            RecalcPlayer(false);
            LastMessage = SkillDisplayName(skill) + " 连接[" + index + "] = " + (support == SupportId.None ? "空" : SupportCatalog.Get(support).Name);
            return true;
        }

        /// <summary>
        /// S3-B1-RCLOSE 运行时兼容判定单一入口（与审计 golden 矩阵同一契约、不同数据源）：
        /// ①Tag 路径——带 RequiredTags 的 Mod 在该技能 Tag 下必须可满足（否则 StatBag 静默跳过=隐形无效）；
        /// ②机制路径——SupportDef.MechanicSkill 限制（分裂只接弹道结算）。装配/配置阶段调用，不进战斗热路径。
        /// </summary>
        public static bool IsSupportCompatible(SupportId support, SkillId skill)
        {
            if (support == SupportId.None)
                return false;
            if (skill != SkillId.Melee && skill != SkillId.Projectile && skill != SkillId.Area)
                return false;
            SupportDef def = SupportCatalog.Get(support);
            if (def.Id != support)
                return false;
            if (def.MechanicSkill != SkillId.None && def.MechanicSkill != skill)
                return false;
            Tag skillTags = SkillTags.Of(skill);
            if (def.Mods == null)
                return true;
            for (int i = 0; i < def.Mods.Length; i++)
            {
                Tag req = def.Mods[i].RequiredTags;
                if (req != Tag.None && (skillTags & req) != req)
                    return false;
            }
            return true;
        }

        public bool TryAllocate(int node, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            if (node < 0 || node >= Allocated.Length)
            {
                error = "无此节点";
                return false;
            }

            if (Allocated[node])
            {
                error = "已点亮";
                return false;
            }

            if (Unspent <= 0)
            {
                error = "没有天赋点";
                return false;
            }

            if (!AdjacentToAllocated(node))
            {
                error = "需与已点亮节点相连";
                return false;
            }

            Allocated[node] = true;
            Unspent--;
            RecalcPlayer(false);
            PassiveNode n = PassiveCatalog.Get(node);
            LastMessage = "点亮 " + n.Name;
            return true;
        }

        public bool TryRespec(out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            int spent = 0;
            for (int i = 1; i < Allocated.Length; i++)
            {
                if (Allocated[i])
                {
                    Allocated[i] = false;
                    spent++;
                }
            }

            Allocated[0] = true;
            Unspent += spent;
            RecalcPlayer(false);
            LastMessage = "免费重构完成，退回 " + spent + " 点";
            return true;
        }

        public bool TryRandomCraft(int invIndex, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            if (Scrap <= 0)
            {
                error = "没有废料";
                return false;
            }

            if (invIndex < 0 || invIndex >= InventoryCount)
            {
                error = "无此物品";
                return false;
            }

            ItemInstance it = Inventory[invIndex];
            if (it.Rarity != Rarity.Rare)
            {
                error = "随机制作只作用于稀有装备";
                return false;
            }

            Scrap--;
            ItemInstance rolled = RollItem(it.Slot, Rarity.Rare, LootRng, it.SocketCount, it.BaseName);
            rolled.Id = it.Id;
            Inventory[invIndex] = rolled;
            RecalcPlayer(false);
            LastMessage = "随机制作：" + DescribeItem(rolled);
            return true;
        }

        public bool TryDirectedCraft(int invIndex, AffixId pick, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }

            if (Etching <= 0)
            {
                error = "没有蚀刻剂";
                return false;
            }

            if (invIndex < 0 || invIndex >= InventoryCount)
            {
                error = "无此物品";
                return false;
            }

            ItemInstance it = Inventory[invIndex];
            AffixDef def = AffixCatalog.Get(pick);
            // S4-P3：定向制作受唯一 applicability 约束——非法组合 deterministic reject（不消耗蚀刻剂、不半写入）
            if (!def.IsApplicable(it.Slot))
            {
                error = def.Name + " 不能出现在" + SlotName(it.Slot);
                return false;
            }
            // S4-P4（Production Simulation 发现项）：物品内重复词缀=非法 duplicate——deterministic reject（不消耗、不半写入）
            for (int a = 0; a < it.AffixCount; a++)
            {
                if (it.AffixIdAt(a) == (int)pick)
                {
                    error = def.Name + " 已在该装备上";
                    return false;
                }
            }
            float v = RngUtil.Range(LootRng, def.Min, def.Max);
            float v2 = def.RowCount > 1 ? RngUtil.Range(LootRng, def.Min2, def.Max2) : 0f;
            if (it.AffixCount < 4)
            {
                it.SetAffix(it.AffixCount, pick, v, v2);
                it.AffixCount++;
                if (it.AffixCount >= 3)
                    it.Rarity = Rarity.Rare;
            }
            else
            {
                it.SetAffix(it.AffixCount - 1, pick, v, v2);
            }

            Inventory[invIndex] = it;
            Etching--;
            RecalcPlayer(false);
            LastMessage = "定向制作写入 " + def.Name;
            return true;
        }

        public void ToggleMapAffix(int i)
        {
            if (OnMap)
            {
                LastMessage = SliceCopy.AffixLocked;
                return;
            }

            if (i < 0 || i >= MapAffixOn.Length)
                return;
            MapAffixOn[i] = !MapAffixOn[i];
            RefreshReward();
            LastMessage = MapAffixCatalog.All[i].Name + (MapAffixOn[i] ? " 开" : " 关") +
                          "  稳定度 " + Stability + "  收益 x" + RewardMultiplier.ToString("0.00");
        }

        public void RefreshReward()
        {
            int cost = 0;
            float reward = 1f;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (!MapAffixOn[i])
                    continue;
                MapAffixDef a = MapAffixCatalog.All[i];
                cost += a.StabilityCost;
                reward += a.RewardAdd;
            }

            int stab = Integrity - cost;
            if (stab < 0)
                stab = 0;
            Stability = stab;
            RewardMultiplier = reward;
        }

        public int EnabledAffixCount()
        {
            int n = 0;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    n++;
            }

            return n;
        }

        public void ApplyStabilityLoss(int amount)
        {
            if (amount < 0)
                amount = 0;
            Integrity -= amount;
            if (Integrity < 0)
                Integrity = 0;
            RefreshReward();
        }

        public bool TryEnterMap(ArenaSim sim, out string error)
        {
            error = null;
            if (OnMap)
            {
                error = State == MapState.Cleared ? SliceCopy.ExitFirst : "已在图内";
                return false;
            }

            RefreshReward();
            Snapshot = Capture();
            Snapshot.Locked = true;
            State = MapState.InMap;
            Alive = true;
            RecalcPlayer(true);
            CombatRng = new SeededRng(MapSeedNow());
            SpawnMap(sim);
            LastMessage = SliceCopy.CombatLocked + "  稳定度 " + Stability + "  收益 x" + RewardMultiplier.ToString("0.00");
            GameLog.Info("Slice", LastMessage);
            return true;
        }

        public void ExitMap(ArenaSim sim, bool death)
        {
            if (State == MapState.Town)
                return;

            State = death ? MapState.Dead : MapState.Town;
            Snapshot.Locked = false;
            sim.Projectiles.Clear();
            sim.Feedback.Clear();
            sim.Dummies.Clear();
            sim.SpawnedCount = 0;
            // 导演 2026-09-08：出图后不再在玩家周围生成木桩群（原 SpawnDummies(8, ...) 已移除）
            if (death)
            {
                ApplyStabilityLoss(SliceRules.DeathStabilityLoss + SliceRules.DeathStabilityPerAffix * EnabledAffixCount());
                string err;
                TryRespec(out err);
                RecalcPlayer(true);
                LastMessage = SliceCopy.DeadRespec + "  稳定度 " + Stability;
            }
            else
            {
                RecalcPlayer(true);
                LastMessage = SliceCopy.TownFree;
            }

            sim.RecoverPlayer();

            GameLog.Info("Slice", LastMessage);
        }

        public uint MapSeedNow()
        {
            uint s = SessionSeed;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    s ^= (uint)(0x9E3779B9u * (i + 1));
            }

            return s;
        }

        public void SpawnMap(ArenaSim sim)
        {
            sim.Projectiles.Clear();
            sim.Feedback.Clear();
            sim.Dummies.Clear();
            var rng = new SeededRng(MapSeedNow());
            SpawnKind(sim, EnemyKind.Brute, 8, rng, 7f);
            SpawnKind(sim, EnemyKind.Stinger, 8, rng, 11f);
            SpawnKind(sim, EnemyKind.Ashling, 6, rng, 15f);
            SpawnKind(sim, EnemyKind.Warden, 1, rng, 18f);
            sim.SpawnedCount = sim.Dummies.AliveCount;
        }

        void SpawnKind(ArenaSim sim, EnemyKind kind, int count, SeededRng rng, float radius)
        {
            EnemyDef def = EnemyCatalog.Get(kind);
            int life = Mathf.RoundToInt(def.Life * (1f + MapMonsterLifeMore()));
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)Mathf.Max(1, count)) * 6.2831853f + rng.NextFloat01() * 0.4f;
                float r = radius + rng.NextFloat01() * 1.6f;
                int id = sim.Dummies.SpawnEnemy(
                    sim.Player.X + Mathf.Cos(ang) * r,
                    sim.Player.Z + Mathf.Sin(ang) * r,
                    kind,
                    life,
                    def);
                if (id < 0)
                    return;
            }
        }

        public float MapMonsterLifeMore()
        {
            float v = 0f;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    v += MapAffixCatalog.All[i].MonsterLifeMore;
            }

            return v;
        }

        public float MapMonsterDamageMore()
        {
            float v = 0f;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    v += MapAffixCatalog.All[i].MonsterDamageMore;
            }

            return v;
        }

        public float MapPlayerFireRes()
        {
            float v = 0f;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    v += MapAffixCatalog.All[i].PlayerFireResFlat;
            }

            return v;
        }

        public float MapMonsterAddedFire()
        {
            float v = 0f;
            for (int i = 0; i < MapAffixOn.Length; i++)
            {
                if (MapAffixOn[i])
                    v += MapAffixCatalog.All[i].MonsterAddedFire;
            }

            return v;
        }

        public void RecalcPlayer(bool refill)
        {
            PlayerStats.Clear();
            PlayerStats.Add(Modifier.Make(StatId.Life, ModOp.Base, SliceRules.PlayerBaseLife), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Mana, ModOp.Base, SliceRules.PlayerBaseMana), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Accuracy, ModOp.Base, SliceRules.PlayerBaseAccuracy), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Armour, ModOp.Base, SliceRules.PlayerBaseArmour), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Evasion, ModOp.Base, SliceRules.PlayerBaseEvasion), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.CritChanceBase, ModOp.Base, SliceRules.BaseCrit), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.MaxFireResistance, ModOp.Base, CombatMath.ResistDefaultMax), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Strength, ModOp.Base, 20f), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Dexterity, ModOp.Base, 20f), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Intelligence, ModOp.Base, 20f), Tag.None, ConditionId.Always);

            for (int i = 0; i < Allocated.Length; i++)
            {
                if (!Allocated[i])
                    continue;
                AddDefensive(PassiveCatalog.Get(i).Mods);
            }

            for (int s = 0; s < Equipped.Length; s++)
            {
                int idx = Equipped[s];
                if (idx < 0 || idx >= InventoryCount)
                    continue;
                AddItemDefensive(Inventory[idx]);
            }

            if (State == MapState.InMap)
                PlayerStats.Add(Modifier.Make(StatId.FireResistance, ModOp.Flat, MapPlayerFireRes()), Tag.None, ConditionId.Always);

            Strength = Mathf.RoundToInt(PlayerStats.Get(StatId.Strength));
            Dexterity = Mathf.RoundToInt(PlayerStats.Get(StatId.Dexterity));
            Intelligence = Mathf.RoundToInt(PlayerStats.Get(StatId.Intelligence));
            PlayerStats.Add(Modifier.Make(StatId.Life, ModOp.Flat, Strength * SliceRules.LifePerStr), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Mana, ModOp.Flat, Intelligence * SliceRules.ManaPerInt), Tag.None, ConditionId.Always);
            PlayerStats.Add(Modifier.Make(StatId.Accuracy, ModOp.Flat, Dexterity * SliceRules.AccuracyPerDex), Tag.None, ConditionId.Always);

            MaxLife = PlayerStats.Get(StatId.Life);
            MaxMana = PlayerStats.Get(StatId.Mana);
            if (MaxLife < 1f)
                MaxLife = 1f;
            if (MaxMana < 1f)
                MaxMana = 1f;
            if (refill || Life <= 0f || Life > MaxLife)
                Life = MaxLife;
            if (refill || Mana > MaxMana)
                Mana = MaxMana;
            RebuildTriggers();
        }

        static bool IsDefensive(StatId stat)
        {
            return stat == StatId.Life || stat == StatId.Mana ||
                   stat == StatId.Strength || stat == StatId.Dexterity || stat == StatId.Intelligence ||
                   stat == StatId.Armour || stat == StatId.Evasion || stat == StatId.Accuracy ||
                   stat == StatId.FireResistance || stat == StatId.MaxFireResistance;
        }

        void AddDefensive(Modifier[] mods)
        {
            if (mods == null)
                return;
            for (int i = 0; i < mods.Length; i++)
            {
                if (IsDefensive(mods[i].Stat))
                    PlayerStats.Add(mods[i], Tag.None, ConditionId.Always);
            }
        }

        void AddItemDefensive(ItemInstance it)
        {
            for (int i = 0; i < it.AffixCount; i++)
            {
                AffixDef def = AffixCatalog.Get((AffixId)it.AffixIdAt(i));
                for (int r = 0; r < def.RowCount; r++)
                {
                    StatId stat = def.RowStat(r);
                    if (!IsDefensive(stat))
                        continue;
                    PlayerStats.Add(Modifier.Make(stat, def.RowOp(r), RowValue(it, i, r)), Tag.None, ConditionId.Always);
                }
            }
        }

        static float RowValue(ItemInstance it, int affixIndex, int row)
        {
            return row == 0 ? it.ValueAt(affixIndex) : it.SecondValueAt(affixIndex);
        }

        void RebuildTriggers()
        {
            Triggers.Clear();
            Triggers.Handler = HandleEffect;
            AddSupportTriggers(SkillId.Melee, QSupports);
            AddSupportTriggers(SkillId.Projectile, WSupports);
            AddSupportTriggers(SkillId.Area, ESupports);
        }

        void AddSupportTriggers(SkillId skill, SupportId[] arr)
        {
            if (arr == null)
                return;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == SupportId.None)
                    continue;
                SupportDef def = SupportCatalog.Get(arr[i]);
                if (def.TriggerEffect == EffectId.None)
                    continue;
                Trigger t;
                t.Event = def.TriggerEvent;
                t.Effect = def.TriggerEffect;
                t.Cooldown = 0f;
                t.MaxDepth = def.TriggerDepth > 0 ? def.TriggerDepth : 1;
                t.Skill = skill;
                Triggers.Add(t);
            }
        }

        public void CollectSkillMods(SkillId skill, StatBag bag)
        {
            bag.Clear();
            Tag tags = SkillTags.Of(skill);
            bag.Add(Modifier.Make(StatId.Accuracy, ModOp.Flat, PlayerStats.Get(StatId.Accuracy)), Tag.None, ConditionId.Always);
            bag.Add(Modifier.Make(StatId.CritChanceBase, ModOp.Base, SliceRules.BaseCrit), Tag.None, ConditionId.Always);

            for (int i = 0; i < Allocated.Length; i++)
            {
                if (!Allocated[i])
                    continue;
                bag.AddAll(PassiveCatalog.Get(i).Mods, tags, ConditionId.Always);
            }

            for (int s = 0; s < Equipped.Length; s++)
            {
                int idx = Equipped[s];
                if (idx < 0 || idx >= InventoryCount)
                    continue;
                ItemInstance it = Inventory[idx];
                for (int a = 0; a < it.AffixCount; a++)
                {
                    AffixDef def = AffixCatalog.Get((AffixId)it.AffixIdAt(a));
                    for (int r = 0; r < def.RowCount; r++)
                    {
                        StatId stat = def.RowStat(r);
                        if (IsDefensive(stat) && stat != StatId.Accuracy)
                            continue;
                        bag.Add(Modifier.Make(stat, def.RowOp(r), RowValue(it, a, r)), tags, ConditionId.Always);
                    }
                }
            }

            SupportId[] arr = SupportsOf(skill);
            if (arr == null)
                return;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == SupportId.None)
                    continue;
                bag.AddAll(SupportCatalog.Get(arr[i]).Mods, tags, ConditionId.Always);
            }
        }

        readonly StatBag _skillBag = new StatBag();

        public SkillDef ResolveSkillDef(SkillId id)
        {
            SkillDef def = SkillCatalog.Get(id);
            CollectSkillMods(id, _skillBag);
            float speed = _skillBag.RawIncreased(StatId.AttackSpeed);
            if (speed > 0f)
            {
                float scale = 1f / (1f + speed);
                def.Recovery *= scale;
                def.Cooldown *= scale;
            }

            float areaMore = _skillBag.RawMore(StatId.AreaRadiusMore);
            if (id == SkillId.Area && Math.Abs(areaMore - 1f) > 1e-5f)
                def.AreaRadius *= areaMore;

            if (State == MapState.InMap || State == MapState.Dead)
            {
                if (id == SkillId.Melee)
                    def.Damage = SliceRules.MeleeBase;
                else if (id == SkillId.Projectile)
                    def.Damage = SliceRules.ProjectileBase;
                else if (id == SkillId.Area)
                    def.Damage = SliceRules.AreaBase;
            }

            return def;
        }

        public HitRequest BuildPlayerHit(SkillId skill, Dummy dummy)
        {
            CollectSkillMods(skill, _skillBag);
            SkillDef def = ResolveSkillDef(skill);
            HitRequest req = default;
            req.PhysFlat = def.Damage + _skillBag.Get(StatId.AddedPhysical);
            req.FireFlat = _skillBag.Get(StatId.AddedFire);
            req.ConvertPhysToFire = _skillBag.Get(StatId.ConvertPhysToFire);
            req.IncDamage = _skillBag.RawIncreased(StatId.Damage);
            req.IncPhys = _skillBag.RawIncreased(StatId.PhysicalDamage);
            req.IncFire = _skillBag.RawIncreased(StatId.FireDamage);
            req.MoreDamage = _skillBag.RawMore(StatId.MoreDamage) * (skill == SkillId.Area ? _skillBag.RawMore(StatId.AreaDamageMore) : 1f);
            req.MorePhys = _skillBag.RawMore(StatId.MorePhysical);
            req.MoreFire = _skillBag.RawMore(StatId.MoreFire);
            req.Accuracy = _skillBag.Get(StatId.Accuracy);
            req.Evasion = dummy.Evasion;
            req.Armour = dummy.Armour;
            req.FireRes = dummy.FireRes;
            req.FireResMax = CombatMath.ResistCap;
            req.BaseCrit = SliceRules.BaseCrit;
            req.AddedCrit = _skillBag.Get(StatId.CritChanceAdded);
            req.IncCrit = _skillBag.RawIncreased(StatId.CritChanceIncreased);
            req.AddedCritMulti = _skillBag.Get(StatId.CritMultiAdded);
            req.IgniteChance = _skillBag.Get(StatId.IgniteChance);
            req.IsAttack = (SkillTags.Of(skill) & Tag.Attack) != 0;
            req.HitRoll = CombatRng.NextFloat01();
            req.CritRoll = CombatRng.NextFloat01();
            req.IgniteRoll = CombatRng.NextFloat01();
            req.Skill = skill;
            req.Tags = SkillTags.Of(skill);
            return req;
        }

        public HitRequest BuildEnemyHit(Dummy dummy)
        {
            float more = 1f + MapMonsterDamageMore();
            HitRequest req = default;
            req.PhysFlat = dummy.AttackPhys * more;
            req.FireFlat = (dummy.AttackFire + MapMonsterAddedFire()) * more;
            req.MoreDamage = 1f;
            req.MorePhys = 1f;
            req.MoreFire = 1f;
            req.Accuracy = dummy.Accuracy;
            req.Evasion = PlayerStats.Get(StatId.Evasion);
            req.Armour = PlayerStats.Get(StatId.Armour);
            req.FireRes = PlayerStats.Get(StatId.FireResistance);
            req.FireResMax = PlayerStats.Get(StatId.MaxFireResistance);
            if (req.FireResMax <= 0f)
                req.FireResMax = CombatMath.ResistDefaultMax;
            req.IsAttack = true;
            req.HitRoll = CombatRng.NextFloat01();
            req.CritRoll = 1f;
            req.IgniteRoll = 1f;
            return req;
        }

        public int ForkCount(SkillId skill)
        {
            CollectSkillMods(skill, _skillBag);
            return _skillBag.Get(StatId.Fork) >= 1f ? 1 : 0;
        }

        public void HandleEffect(EffectId effect, TriggerContext ctx)
        {
            if (effect == EffectId.ForkProjectiles)
            {
                LastWasFork = true;
                return;
            }

            if (effect == EffectId.ApplyIgnite && ctx.Hit.Ignite && ctx.DummyIndex >= 0)
                IgniteApplies++;
        }

        public void OnKill(ArenaSim sim, Dummy dummy, int index)
        {
            Kills++;
            if (State != MapState.InMap)
                return;

            float reward = RewardMultiplier;
            if (RngUtil.Chance(LootRng, SliceRules.DropScrap * Mathf.Min(1.4f, reward)))
            {
                Scrap++;
                LastLoot = "+1 Scrap";
            }

            if (RngUtil.Chance(LootRng, SliceRules.DropEtching * Mathf.Min(1.4f, reward)))
            {
                Etching++;
                LastLoot = "+1 Etching";
            }

            float rareP = SliceRules.DropRare * reward;
            float ordP = SliceRules.DropOrdinary * reward;
            if (dummy.Kind == EnemyKind.Warden)
                rareP += 0.35f;

            if (RngUtil.Chance(LootRng, rareP))
                DropGear(sim, dummy, Rarity.Rare);
            else if (RngUtil.Chance(LootRng, ordP))
                DropGear(sim, dummy, Rarity.Ordinary);

            if (dummy.Kind == EnemyKind.Warden)
            {
                TotalPoints++;
                Unspent++;
                LastMessage = "精英倒下。+1 天赋点。收益 x" + RewardMultiplier.ToString("0.00");
            }

            if (sim.Dummies.AliveCount <= 0)
            {
                State = MapState.Cleared;
                Snapshot.Locked = false;
                ApplyStabilityLoss(SliceRules.ClearStabilityLoss);
                LastMessage = SliceCopy.Cleared + "  稳定度 " + Stability + "  收益 x" + RewardMultiplier.ToString("0.00");
            }
        }

        void DropGear(ArenaSim sim, Dummy dummy, Rarity rarity)
        {
            EquipSlot slot = (EquipSlot)RngUtil.NextInt(LootRng, 0, (int)EquipSlot.Count);
            int sockets = SocketsFor(slot);
            ItemInstance it = RollItem(slot, rarity, LootRng, sockets, ItemBaseName(slot));
            if (AddItem(it) >= 0)
            {
                LastLoot = DescribeItem(it);
                if (sim.Feedback != null)
                    sim.Feedback.Spawn(FeedbackKind.Loot, dummy.X, dummy.Z, 0.6f, 1.1f, 0f);
                AudioEvents.Play(AudioEventId.Loot);
            }
        }

        public void TickRegen(float dt)
        {
            if (!Alive)
                return;
            Mana += SliceRules.ManaRegen * dt;
            if (Mana > MaxMana)
                Mana = MaxMana;
            if (HitFlash > 0f)
            {
                HitFlash -= dt;
                if (HitFlash < 0f)
                    HitFlash = 0f;
            }

            Triggers.Tick(dt);
        }

        public void ApplyPlayerHit(HitResult hit)
        {
            if (!Alive)
                return;
            Life -= hit.TotalTaken;
            HitFlash = CombatRules.HitFlash;
            if (Life <= 0f)
            {
                Life = 0f;
                Alive = false;
            }
        }

        public bool SpendMana(SkillId skill)
        {
            if (State != MapState.InMap)
                return true;
            float cost = skill == SkillId.Area ? 12f : skill == SkillId.Projectile ? 8f : 5f;
            if (Mana < cost)
                return false;
            Mana -= cost;
            return true;
        }

        public SupportId[] SupportsOf(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return QSupports;
            if (skill == SkillId.Projectile)
                return WSupports;
            if (skill == SkillId.Area)
                return ESupports;
            return null;
        }

        /// <summary>S5-WO-02：技能→默认连接槽位硬映射（既有契约，不改）。</summary>
        static EquipSlot MappedSlot(SkillId skill)
        {
            return skill == SkillId.Melee ? EquipSlot.Weapon
                 : skill == SkillId.Projectile ? EquipSlot.Body
                 : EquipSlot.Helmet;
        }

        /// <summary>S5-WO-02：连接槽位→默认技能（映射逆；Gloves/Belt/Boots 无映射=SkillId.None）。</summary>
        static SkillId MappedSlotOfSlot(EquipSlot slot)
        {
            return slot == EquipSlot.Weapon ? SkillId.Melee
                 : slot == EquipSlot.Body ? SkillId.Projectile
                 : slot == EquipSlot.Helmet ? SkillId.Area
                 : SkillId.None;
        }

        /// <summary>
        /// S5-WO-02（BL-021.A2，合同 S5_LINK_CONTRACT.md）：技能被已装备物品 group 1 改挂时的 hosting 库存位。
        /// 返回 -1=无改挂（读路径回退映射槽 legacy）。同技能被多物品改挂=腐败构造态，fail-closed 忽略改挂（禁 first/last-wins）。
        /// </summary>
        int RebindHostIndex(SkillId skill)
        {
            if (skill != SkillId.Melee && skill != SkillId.Projectile && skill != SkillId.Area)
                return -1;
            int found = -1;
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                int idx = Equipped[s];
                if (idx < 0 || idx >= InventoryCount || Inventory[idx].LinkSkill1 != skill)
                    continue;
                if (found >= 0)
                    return -1;
                found = idx;
            }
            return found;
        }

        static int CountSupports(SupportId[] arr)
        {
            if (arr == null)
                return 0;
            int n = 0;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != SupportId.None)
                    n++;
            return n;
        }

        public int SupportCapacity(SkillId skill)
        {
            // S5-WO-02：被改挂技能的唯一连接源=hosting item 的 group 1（末尾 2 孔，容量恒 1）。
            if (RebindHostIndex(skill) >= 0)
                return 1;

            EquipSlot slot = MappedSlot(skill);
            int idx = Equipped[(int)slot];
            if (idx < 0 || idx >= InventoryCount)
                return 0;
            ItemInstance host = Inventory[idx];
            // S5-WO-02：host 自带 group 1（LinkSkill1≠None 且孔数足）时，group 0=前部 SocketCount-2 孔（末尾 2 孔归 group 1）。
            int group0Sockets = host.LinkSkill1 != SkillId.None && host.SocketCount >= 3 ? host.SocketCount - 2 : host.SocketCount;
            int cap = group0Sockets - 1;
            if (cap < 0)
                cap = 0;
            SupportId[] arr = SupportsOf(skill);
            if (arr != null && cap > arr.Length)
                cap = arr.Length;
            return cap;
        }

        /// <summary>
        /// S5-WO-02（BL-021.A2，合同 docs/reviews/S5/S5_LINK_CONTRACT.md）：把一个技能的连接改挂到指定物品的
        /// group 1（末尾 2 孔），或传 SkillId.None 清除改挂。每技能至多一个有效连接源；写入前全量校验
        /// （自改挂/双物品改挂/孔数不足/容量溢出=确定性拒绝，无半写入）——不做静默截断/迁移/重排。
        /// </summary>
        public bool TryReassignLink(int invIndex, SkillId skill, out string error)
        {
            error = null;
            if (BuildLocked)
            {
                error = SliceCopy.LockFail;
                return false;
            }
            if (invIndex < 0 || invIndex >= InventoryCount)
            {
                error = "无此物品";
                return false;
            }

            ItemInstance it = Inventory[invIndex];

            // 清除改挂：被改挂技能回退映射槽 legacy，host group 0 恢复完整孔集
            if (skill == SkillId.None)
            {
                if (it.LinkSkill1 == SkillId.None)
                {
                    error = "该物品无第二连接组";
                    return false;
                }
                SkillId freed = it.LinkSkill1;
                it.LinkSkill1 = SkillId.None;
                Inventory[invIndex] = it;
                ClampSupportsToSockets();
                RecalcPlayer(false);
                LastMessage = SkillDisplayName(freed) + " 连接回归默认";
                return true;
            }

            if (skill != SkillId.Melee && skill != SkillId.Projectile && skill != SkillId.Area)
            {
                error = "非可连接技能";
                return false;
            }

            // 自改挂拒绝：group 1 不能与该物品自带连接同技能
            SkillId hostSkill = MappedSlotOfSlot(it.Slot);
            if (hostSkill != SkillId.None && skill == hostSkill)
            {
                error = "该物品已自带此技能连接";
                return false;
            }

            // 2 组资格：SocketCount >= 3（group 1 固定占末尾 2 孔）
            if (it.SocketCount < 3)
            {
                error = "孔数不足 3，无法承载第二连接组";
                return false;
            }

            // 全局唯一连接源：该技能未被其它已装备物品改挂（禁 first/last-wins）
            for (int s = 0; s < (int)EquipSlot.Count; s++)
            {
                int idx = Equipped[s];
                if (idx == invIndex || idx < 0 || idx >= InventoryCount)
                    continue;
                if (Inventory[idx].LinkSkill1 == skill)
                {
                    error = SkillDisplayName(skill) + " 已被其它装备改挂";
                    return false;
                }
            }

            // 原子容量校验（写入前；任一溢出=拒绝，禁静默截断/迁移/重排）：
            // 拆分后 host 映射技能 group 0 容量 = (SocketCount-2)-1；被改挂技能 group 1 容量 = 1。
            if (CountSupports(SupportsOf(hostSkill)) > it.SocketCount - 3)
            {
                error = SkillDisplayName(hostSkill) + " 现有连接超出拆分后容量，先拆除非空连接";
                return false;
            }
            if (CountSupports(SupportsOf(skill)) > 1)
            {
                error = SkillDisplayName(skill) + " 现有连接超出第二组容量（1），先拆除非空连接";
                return false;
            }

            it.LinkSkill1 = skill;
            Inventory[invIndex] = it;
            ClampSupportsToSockets();
            RecalcPlayer(false);
            LastMessage = SkillDisplayName(skill) + " 连接改挂至 " + CleanBaseName(it.BaseName);
            return true;
        }

        void ClampSupportsToSockets()
        {
            ClampArr(QSupports, SupportCapacity(SkillId.Melee));
            ClampArr(WSupports, SupportCapacity(SkillId.Projectile));
            ClampArr(ESupports, SupportCapacity(SkillId.Area));
        }

        static void ClampArr(SupportId[] arr, int cap)
        {
            for (int i = cap; i < arr.Length; i++)
                arr[i] = SupportId.None;
        }

        bool IsSupportUsed(SupportId id, SkillId exceptSkill, int exceptIndex)
        {
            if (CheckUsed(QSupports, SkillId.Melee, id, exceptSkill, exceptIndex))
                return true;
            if (CheckUsed(WSupports, SkillId.Projectile, id, exceptSkill, exceptIndex))
                return true;
            if (CheckUsed(ESupports, SkillId.Area, id, exceptSkill, exceptIndex))
                return true;
            return false;
        }

        static bool CheckUsed(SupportId[] arr, SkillId skill, SupportId id, SkillId exceptSkill, int exceptIndex)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (skill == exceptSkill && i == exceptIndex)
                    continue;
                if (arr[i] == id)
                    return true;
            }

            return false;
        }

        bool AdjacentToAllocated(int node)
        {
            int[] links = PassiveCatalog.Get(node).Links;
            if (links == null)
                return false;
            for (int i = 0; i < links.Length; i++)
            {
                int n = links[i];
                if (n >= 0 && n < Allocated.Length && Allocated[n])
                    return true;
            }

            return false;
        }

        BuildSnapshot Capture()
        {
            BuildSnapshot s = default;
            s.WeaponId = Equipped[0];
            s.BodyId = Equipped[1];
            s.HelmetId = Equipped[2];
            s.BootsId = Equipped[3];
            s.GlovesId = Equipped[4];
            s.BeltId = Equipped[5];
            s.Q0 = QSupports[0];
            s.Q1 = QSupports[1];
            s.W0 = WSupports[0];
            s.W1 = WSupports[1];
            s.E0 = ESupports[0];
            int mask = 0;
            for (int i = 0; i < Allocated.Length; i++)
            {
                if (Allocated[i])
                    mask |= 1 << i;
            }

            s.PassiveMask = mask;
            s.Unspent = Unspent;
            return s;
        }

        public static int SocketsFor(EquipSlot slot)
        {
            if (slot == EquipSlot.Weapon)
                return SliceRules.WeaponSockets;
            if (slot == EquipSlot.Body)
                return SliceRules.BodySockets;
            if (slot == EquipSlot.Helmet)
                return SliceRules.HelmetSockets;
            if (slot == EquipSlot.Gloves)
                return SliceRules.GlovesSockets;
            if (slot == EquipSlot.Belt)
                return SliceRules.BeltSockets;
            return SliceRules.BootsSockets;
        }

        public static string SlotName(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Weapon: return "武器";
                case EquipSlot.Body: return "胸甲";
                case EquipSlot.Helmet: return "头盔";
                case EquipSlot.Gloves: return "手套";
                case EquipSlot.Belt: return "腰带";
                default: return "靴子";
            }
        }

        public static string SkillName(SkillId skill)
        {
            return SkillDisplayName(skill);
        }

        public static string SkillDisplayName(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return "近战";
            if (skill == SkillId.Projectile)
                return "弹道";
            if (skill == SkillId.Area)
                return "范围";
            return "-";
        }

        public static string SkillHotkey(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return "Q";
            if (skill == SkillId.Projectile)
                return "W";
            if (skill == SkillId.Area)
                return "E";
            return "";
        }

        public static string RarityWord(Rarity rarity)
        {
            return rarity == Rarity.Rare ? "稀有" : "普通";
        }

        public static string ItemBaseName(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Weapon: return "铁刃";
                case EquipSlot.Body: return "皮甲";
                case EquipSlot.Helmet: return "铁盔";
                case EquipSlot.Gloves: return "布手";
                case EquipSlot.Belt: return "皮带";
                default: return "旧靴";
            }
        }

        public static string CleanBaseName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return raw;
            string n = raw.Trim();
            if (n.StartsWith("Rare "))
                n = n.Substring(5);
            else if (n.StartsWith("Ordinary "))
                n = n.Substring(9);
            else if (n.StartsWith("稀有 "))
                n = n.Substring(3);
            else if (n.StartsWith("普通 "))
                n = n.Substring(3);
            return n;
        }

        public static string DescribeItem(ItemInstance it)
        {
            string baseName = CleanBaseName(it.BaseName);
            if (string.IsNullOrEmpty(baseName))
                baseName = ItemBaseName(it.Slot);
            string s = RarityWord(it.Rarity) + " " + baseName + " [" + it.SocketCount + "孔]";
            for (int i = 0; i < it.AffixCount; i++)
                s += " | " + AffixLine(it, i);

            return s;
        }

        public static string AffixLine(ItemInstance it, int i)
        {
            if (i < 0 || i >= it.AffixCount)
                return "";
            AffixDef def = AffixCatalog.Get((AffixId)it.AffixIdAt(i));
            string s = string.Format(def.Format, it.ValueAt(i));
            if (def.RowCount > 1)
                s += "，" + string.Format(def.Format2, it.SecondValueAt(i));
            return s;
        }

        public bool CanAllocate(int node)
        {
            if (BuildLocked)
                return false;
            if (node < 0 || node >= Allocated.Length)
                return false;
            if (Allocated[node])
                return false;
            if (Unspent <= 0)
                return false;
            return AdjacentToAllocated(node);
        }

        public NodeUiState NodeState(int node)
        {
            if (node < 0 || node >= Allocated.Length)
                return NodeUiState.Locked;
            if (Allocated[node])
                return NodeUiState.Allocated;
            if (CanAllocate(node))
                return NodeUiState.Available;
            return NodeUiState.Locked;
        }

        public string SupportLabel(SkillId skill)
        {
            SupportId[] arr = SupportsOf(skill);
            int cap = SupportCapacity(skill);
            string s = SkillDisplayName(skill) + " ";
            if (arr == null)
                return s;
            for (int i = 0; i < arr.Length; i++)
            {
                if (i >= cap)
                    s += "[-]";
                else if (arr[i] == SupportId.None)
                    s += "[ ]";
                else
                    s += "[" + SupportCatalog.Get(arr[i]).Name + "]";
            }

            return s;
        }
    }
}
