using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S4 Phase 4 — Production Simulation / Scale Proof（S4_PLAN §八；协调席 2026-09-09 工作指令）。
    /// 用**真实 Drop/Craft/Equip 路径**（RollItem/TryRandomCraft/TryDirectedCraft/TryEquip/RecalcPlayer，零第二套生成引擎）
    /// 跑 10,000 seeded cycles，验证：deterministic replay / no NaN / no invalid slot / no incompatible affix /
    /// no stale second-value data / no impossible item / no duplicate affix / craft result always valid /
    /// all generated affix Stats consumed / fixed seed output reproducible（FNV-1a 64 hash）。
    /// 产物：docs/qa/PRODUCTION_SIMULATION_REPORT.json（machine-readable；确定性渲染，禁止手填）。
    /// 纯 tooling/测试层（Game.Tests.EditMode），零 runtime hot path。
    ///
    /// S6P-WO-02：canonical 面从 loot→craft→equip 扩到 loot→craft→equip→passive allocation→
    /// passive modifier→effective gameplay stat（见 <see cref="PassiveAwareProductionSimulation"/>）。
    /// 每个 session 先经 domain API 建立 canonical 被动 loadout，再把顺序无关的被动语义状态喂进 FNV 负载。
    /// 旧 hash FNV1A64:9a4c9524d0b3e214 从本单起只是 predecessor reference，不是 expected oracle。
    /// </summary>
    internal static class ProductionSimulator
    {
        internal const string SchemaName = "PRODUCTION_SIMULATION_REPORT_V2";
        internal const int SchemaVersion = 2;
        internal const string GeneratedBy = "Game.Tests.EditMode.ProductionSimulatorTests（EditMode 测试再生；真实 Drop/Craft/Equip + canonical Passive allocation 路径驱动，禁止手填）";
        /// <summary>本单建立新 canonical hash 前的 predecessor（仅参照，不是期望值）。</summary>
        internal const string PredecessorHash = "FNV1A64:9a4c9524d0b3e214";
        internal const uint Seed = 20260909u;
        internal const int SessionCycleSize = 40;
        internal const int TotalIterations = 10000;

        internal static string ArtifactPath
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/PRODUCTION_SIMULATION_REPORT.json")); }
        }

        internal sealed class Result
        {
            public bool VerdictPass;
            public int Iterations;
            public int InvalidCount;
            public int RejectedDirectedCraft;
            public int DropCycles, RandomCraftCycles, DirectedCraftCycles, EquipCycles;
            public int[] SlotDistribution = new int[(int)EquipSlot.Count];
            public int OrdinaryCount, RareCount;
            public int[] AffixDistribution = new int[(int)AffixId.Count];
            public List<string> Invalids = new List<string>();
            public string Hash;             // FNV-1a 64 hex
            public bool RepeatHashMatch;
            public string RepeatHash;

            // ---- S6P-WO-02：被动面证据（canonical session 的状态，不是判据本身） ----
            public string PassiveScenarioVersion;
            public int AllocatedPassiveNodeCount;
            public string AllocatedPassiveNodeIds;
            public int PassiveModifierCount;
            public int PassiveOffensiveTupleCount;
            public int PassiveDefensiveOrAttributeTupleCount;
            public string PassiveEffectiveStatSnapshot;
            public string PassiveEffectiveSkillStatSnapshot;
            public bool PassiveSensitive;
            public List<string> PassiveAllocationEvents = new List<string>();

            // ---- 敏感性证明（临时构造的合法变体；不进 canonical hash） ----
            public string SensitivityAllocationBaselineHash;
            public string SensitivityAllocationMutatedHash;
            public bool AllocationMutationChangedHash;
            public string SensitivityStatBaselineHash;
            public string SensitivityStatMutatedHash;
            public bool StatMutationChangedHash;
            public string SensitivityRestoredHash;
            public bool RestoreExactMatch;
            public bool OrderingInvariant;
        }

        /// <summary>跑 TotalIterations 个 cycle（SessionCycleSize/会话 × N 会话）；decisionRng 决定 cycle 类别。</summary>
        internal static Result Run(uint seed, bool writeArtifact)
        {
            var r = RunCore(seed, PassiveAwareProductionSimulation.CanonicalNodeIds);
            var repeat = RunCore(seed, PassiveAwareProductionSimulation.CanonicalNodeIds);
            r.RepeatHash = repeat.Hash;
            r.RepeatHashMatch = repeat.Hash == r.Hash && repeat.InvalidCount == 0;
            r.VerdictPass = r.InvalidCount == 0 && r.RepeatHashMatch;
            if (writeArtifact)
            {
                ProvePassiveSensitivity(r, seed);
                WriteArtifact(r, seed);
            }
            return r;
        }

        /// <summary>用显式被动 loadout 跑一次完整 simulation（工具/测试层：合同 §7 敏感性证明用）。</summary>
        internal static Result RunWithScenario(uint seed, int[] nodeIds)
        {
            var r = RunCore(seed, nodeIds);
            r.VerdictPass = r.InvalidCount == 0;
            return r;
        }

        /// <summary>
        /// 合同 §7 必做证明：A 改一个 canonical 加点 → hash 必须变；B 合法换一组加点使有效属性不同 → hash 必须变；
        /// C 恢复 canonical → 必须精确复原；另证加点顺序不影响 hash（§8）。
        /// </summary>
        static void ProvePassiveSensitivity(Result r, uint seed)
        {
            string canonical = r.Hash;
            var idsA = PassiveAwareProductionSimulation.SensitivityAllocationNodeIds;
            var idsB = PassiveAwareProductionSimulation.SensitivityStatNodeIds;
            var reversed = PassiveAwareProductionSimulation.CanonicalReverseOrderNodeIds;

            r.SensitivityAllocationBaselineHash = canonical;
            r.SensitivityAllocationMutatedHash = RunCore(seed, idsA).Hash;
            r.AllocationMutationChangedHash = r.SensitivityAllocationMutatedHash != canonical;

            r.SensitivityStatBaselineHash = canonical;
            r.SensitivityStatMutatedHash = RunCore(seed, idsB).Hash;
            r.StatMutationChangedHash = r.SensitivityStatMutatedHash != canonical;

            r.SensitivityRestoredHash = RunCore(seed, PassiveAwareProductionSimulation.CanonicalNodeIds).Hash;
            r.RestoreExactMatch = r.SensitivityRestoredHash == canonical;

            r.OrderingInvariant = RunCore(seed, reversed).Hash == canonical;
            r.PassiveSensitive = r.AllocationMutationChangedHash && r.StatMutationChangedHash && r.RestoreExactMatch;
        }

        static Result RunCore(uint seed, int[] passiveNodeIds)
        {
            var r = new Result();
            ulong hash = 14695981039346656037UL;
            int sessions = TotalIterations / SessionCycleSize;
            int done = 0;
            for (int s = 0; s < sessions && done < TotalIterations; s++)
            {
                var session = new SliceSession();
                session.ResetTown(seed + (uint)s * 7919u);
                session.Etching = 999;   // 定向制作不因资源枯竭产生伪失败
                // S6P-WO-02：canonical 被动 loadout（只走 domain API；事件 trace 进报告，语义状态进哈希）
                var events = PassiveAwareProductionSimulation.Apply(session, passiveNodeIds);
                if (s == 0)
                    CapturePassiveEvidence(r, session, events);
                hash = HashString(hash, "|" + PassiveAwareProductionSimulation.StatePayload(session));
                var decision = new SeededRng(seed ^ (0x9E3779B9u + (uint)s));
                for (int c = 0; c < SessionCycleSize && done < TotalIterations; c++, done++)
                {
                    int roll = RngUtil.NextInt(decision, 0, 100);
                    if (roll < 40)
                    {
                        r.DropCycles++;
                        CycleDrop(session, decision, r, ref hash);
                    }
                    else if (roll < 65)
                    {
                        r.RandomCraftCycles++;
                        CycleRandomCraft(session, r, ref hash);
                    }
                    else if (roll < 85)
                    {
                        r.DirectedCraftCycles++;
                        CycleDirectedCraft(session, decision, r, ref hash);
                    }
                    else
                    {
                        r.EquipCycles++;
                        CycleEquip(session, r, ref hash);
                    }
                    hash = HashString(hash, "|cy|" + done.ToString(CultureInfo.InvariantCulture));
                }
            }
            r.Iterations = done;
            r.Hash = string.Format(CultureInfo.InvariantCulture, "FNV1A64:{0:x16}", hash);
            return r;
        }

        static void CapturePassiveEvidence(Result r, SliceSession session, List<string> events)
        {
            r.PassiveScenarioVersion = PassiveAwareProductionSimulation.ScenarioVersion;
            int[] ids = PassiveAwareProductionSimulation.AllocatedNodeIds(session);
            r.AllocatedPassiveNodeCount = ids.Length;
            var sb = new StringBuilder();
            for (int i = 0; i < ids.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(ids[i].ToString(CultureInfo.InvariantCulture));
            }
            r.AllocatedPassiveNodeIds = sb.ToString();
            r.PassiveModifierCount = PassiveAwareProductionSimulation.ModifierTuples(session).Count;
            r.PassiveOffensiveTupleCount = PassiveAwareProductionSimulation.OffensiveTupleCount(session);
            r.PassiveDefensiveOrAttributeTupleCount = PassiveAwareProductionSimulation.DefensiveOrAttributeTupleCount(session);
            r.PassiveEffectiveStatSnapshot = PassiveAwareProductionSimulation.StatSnapshot(session);
            r.PassiveEffectiveSkillStatSnapshot = PassiveAwareProductionSimulation.SkillStatSnapshot(session);
            r.PassiveAllocationEvents = events;
        }

        // ---- cycles ----

        static void CycleDrop(SliceSession session, SeededRng decision, Result r, ref ulong hash)
        {
            EquipSlot slot = (EquipSlot)RngUtil.NextInt(decision, 0, (int)EquipSlot.Count);
            Rarity rarity = RngUtil.Chance(decision, 0.5f) ? Rarity.Rare : Rarity.Ordinary;
            ItemInstance it = session.RollItem(slot, rarity, session.LootRng,
                SliceSession.SocketsFor(slot), SliceSession.ItemBaseName(slot));
            Validate(session, it, "drop", r);
            r.SlotDistribution[(int)it.Slot]++;
            if (it.Rarity == Rarity.Rare) r.RareCount++; else r.OrdinaryCount++;
            for (int a = 0; a < it.AffixCount; a++)
                r.AffixDistribution[it.AffixIdAt(a)]++;
            hash = HashString(hash, ItemKey(it));
        }

        static void CycleRandomCraft(SliceSession session, Result r, ref ulong hash)
        {
            int idx = FirstRare(session);
            if (idx < 0)
                return; // 无稀有物（早期 cycle 可能）——如实跳过，不算失败
            session.Scrap++;
            string err;
            if (!session.TryRandomCraft(idx, out err))
            {
                r.InvalidCount++;
                r.Invalids.Add("randomCraft FAIL: idx=" + idx + " " + err);
                return;
            }
            ItemInstance it = session.Inventory[idx];
            if (it.Rarity != Rarity.Rare)
                Fail(r, "randomCraft 产物非稀有");
            Validate(session, it, "randomCraft", r);
            for (int a = 0; a < it.AffixCount; a++)
                r.AffixDistribution[it.AffixIdAt(a)]++;
            hash = HashString(hash, "rc|" + ItemKey(it));
        }

        static void CycleDirectedCraft(SliceSession session, SeededRng decision, Result r, ref ulong hash)
        {
            if (session.InventoryCount <= 0)
                return;
            int idx = RngUtil.NextInt(decision, 0, session.InventoryCount);
            ItemInstance target = session.Inventory[idx];
            var pool = new List<int>();
            for (int i = 0; i < (int)AffixId.Count; i++)
                if (AffixCatalog.Get((AffixId)i).IsApplicable(target.Slot))
                    pool.Add(i);
            AffixId pick = (AffixId)pool[RngUtil.NextInt(decision, 0, pool.Count)]; // 先从 eligible 池选（合法组合）
            string err;
            if (!session.TryDirectedCraft(idx, pick, out err))
            {
                // S4-P4：物品已有同词缀=duplicate guard 的预期 reject（fail-safe 生效），如实计数；
                // 其它拒绝（applicability 等）在 eligible 池下不应发生 → invalid。
                if (err != null && err.Contains("已在"))
                {
                    r.RejectedDirectedCraft++;
                    Validate(session, session.Inventory[idx], "directedCraft-reject", r);
                    return;
                }
                r.RejectedDirectedCraft++;
                r.InvalidCount++;
                r.Invalids.Add("directedCraft 拒绝合法组合: idx=" + idx + " affix=" + pick + " " + err);
                return;
            }
            Validate(session, session.Inventory[idx], "directedCraft", r);
            hash = HashString(hash, "dc|" + ItemKey(session.Inventory[idx]));
        }

        static void CycleEquip(SliceSession session, Result r, ref ulong hash)
        {
            for (int idx = 0; idx < session.InventoryCount; idx++)
            {
                ItemInstance it = session.Inventory[idx];
                if (session.Equipped[(int)it.Slot] == idx)
                    continue;
                string err;
                if (session.TryEquip(idx, out err))
                {
                    Validate(session, it, "equip", r);
                    CheckAggregation(session, r);
                    hash = HashString(hash, "eq|" + (int)it.Slot + "|" + ItemKey(it));
                    return;
                }
            }
        }

        // ---- invariants ----

        static void Validate(SliceSession session, ItemInstance it, string origin, Result r)
        {
            string tag = origin + ": id=" + it.Id;
            if ((int)it.Slot < 0 || (int)it.Slot >= (int)EquipSlot.Count)
                Fail(r, tag + " 非法槽位 " + it.Slot);
            if (it.SocketCount != SliceSession.SocketsFor(it.Slot))
                Fail(r, tag + " 孔数 " + it.SocketCount + " ≠ 槽位契约 " + SliceSession.SocketsFor(it.Slot));
            if (it.AffixCount < 0 || it.AffixCount > SliceRules.MaxAffixesRare)
                Fail(r, tag + " 非法词缀数 " + it.AffixCount);
            var seen = new HashSet<int>();
            for (int a = 0; a < it.AffixCount; a++)
            {
                int id = it.AffixIdAt(a);
                if (id < 0 || id >= (int)AffixId.Count)
                {
                    Fail(r, tag + " 词缀 ID 越界 " + id);
                    continue;
                }
                if (!seen.Add(id))
                    Fail(r, tag + " 物品内重复词缀 " + id);
                AffixDef def = AffixCatalog.Get((AffixId)id);
                if (!def.IsApplicable(it.Slot))
                    Fail(r, tag + " 非法 slot-affix 对：" + it.Slot + " <- " + (AffixId)id);
                if (!IsConsumed(def.Stat) || (def.RowCount > 1 && !IsConsumed(def.Stat2)))
                    Fail(r, tag + " 词缀引用非运行期消费 Stat：" + (AffixId)id);
                float v = it.ValueAt(a);
                if (!IsFinite(v) || v < def.Min - 0.0001f || v > def.Max + 0.0001f)
                    Fail(r, tag + " 行 1 值越界/NaN：" + v);
                if (!IsFinite(it.SecondValueAt(a)))
                    Fail(r, tag + " 第二值 NaN");
                if (def.RowCount == 1 && Mathf.Abs(it.SecondValueAt(a)) > 1e-6f)
                    Fail(r, tag + " 单行词缀残留第二值 " + it.SecondValueAt(a) + "（stale second-value）");
                if (def.RowCount > 1 && (it.SecondValueAt(a) < def.Min2 - 0.0001f || it.SecondValueAt(a) > def.Max2 + 0.0001f))
                    Fail(r, tag + " 行 2 值越界：" + it.SecondValueAt(a));
            }
        }

        static void CheckAggregation(SliceSession session, Result r)
        {
            if (!IsFinite(session.MaxLife) || session.MaxLife < 1f)
                Fail(r, "聚合产物 MaxLife 非法：" + session.MaxLife);
            if (!IsFinite(session.MaxMana) || session.MaxMana < 1f)
                Fail(r, "聚合产物 MaxMana 非法：" + session.MaxMana);
        }

        static bool IsFinite(float v)
        {
            return !float.IsNaN(v) && !float.IsInfinity(v);
        }

        static bool IsConsumed(StatId stat)
        {
            foreach (var s in ContentAuditS2Tests.RuntimeConsumedStats)
                if (s == stat)
                    return true;
            return false;
        }

        static int FirstRare(SliceSession session)
        {
            for (int i = 0; i < session.InventoryCount; i++)
                if (session.Inventory[i].Rarity == Rarity.Rare)
                    return i;
            return -1;
        }

        static void Fail(Result r, string msg)
        {
            r.InvalidCount++;
            if (r.Invalids.Count < 64)
                r.Invalids.Add(msg);
        }

        static string ItemKey(ItemInstance it)
        {
            var sb = new StringBuilder();
            sb.Append((int)it.Slot).Append('|').Append((int)it.Rarity).Append('|').Append(it.SocketCount)
              .Append('|').Append(it.AffixCount);
            for (int a = 0; a < it.AffixCount; a++)
                sb.Append('|').Append(it.AffixIdAt(a))
                  .Append(',').Append(it.ValueAt(a).ToString("0.######", CultureInfo.InvariantCulture))
                  .Append(',').Append(it.SecondValueAt(a).ToString("0.######", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        static ulong HashString(ulong hash, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= 1099511628211UL;
            }
            return hash;
        }

        static void WriteArtifact(Result r, uint seed)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"schema\": "); AppendStr(sb, SchemaName); sb.Append(",\n");
            sb.Append("  \"schemaVersion\": ").Append(SchemaVersion).Append(",\n");
            sb.Append("  \"simulationContractVersion\": "); AppendStr(sb, PassiveAwareProductionSimulation.ContractVersion); sb.Append(",\n");
            sb.Append("  \"generatedBy\": "); AppendStr(sb, GeneratedBy); sb.Append(",\n");
            sb.Append("  \"verdict\": "); AppendStr(sb, r.VerdictPass ? "PASS" : "FAIL"); sb.Append(",\n");
            sb.Append("  \"seed\": ").Append(seed).Append(",\n");
            sb.Append("  \"iterations\": ").Append(r.Iterations).Append(",\n");
            sb.Append("  \"sessionCycleSize\": ").Append(SessionCycleSize).Append(",\n");
            sb.Append("  \"cycleKindDistribution\": { \"drop\": ").Append(r.DropCycles)
              .Append(", \"randomCraft\": ").Append(r.RandomCraftCycles)
              .Append(", \"directedCraft\": ").Append(r.DirectedCraftCycles)
              .Append(", \"equip\": ").Append(r.EquipCycles).Append(" },\n");
            sb.Append("  \"slotDistribution\": { ");
            for (int s = 0; s < r.SlotDistribution.Length; s++)
            {
                sb.Append('"').Append(((EquipSlot)s).ToString()).Append("\": ").Append(r.SlotDistribution[s]);
                if (s < r.SlotDistribution.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(" },\n");
            sb.Append("  \"rarityDistribution\": { \"ordinary\": ").Append(r.OrdinaryCount)
              .Append(", \"rare\": ").Append(r.RareCount).Append(" },\n");
            sb.Append("  \"affixDistribution\": { ");
            for (int i = 0; i < r.AffixDistribution.Length; i++)
            {
                sb.Append('"').Append(((AffixId)i).ToString()).Append("\": ").Append(r.AffixDistribution[i]);
                if (i < r.AffixDistribution.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(" },\n");
            sb.Append("  \"invalidCount\": ").Append(r.InvalidCount).Append(",\n");
            sb.Append("  \"invalidSamples\": [ ");
            for (int i = 0; i < r.Invalids.Count; i++)
            {
                AppendStr(sb, r.Invalids[i]);
                if (i < r.Invalids.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(" ],\n");
            sb.Append("  \"rejectedDirectedCraft\": ").Append(r.RejectedDirectedCraft).Append(",\n");
            sb.Append("  \"deterministicHash\": "); AppendStr(sb, r.Hash); sb.Append(",\n");
            sb.Append("  \"repeatHashMatch\": ").Append(r.RepeatHashMatch ? "true" : "false").Append(",\n");
            sb.Append("  \"repeatHash\": "); AppendStr(sb, r.RepeatHash); sb.Append(",\n");

            sb.Append("  \"passiveSimulation\": {\n");
            sb.Append("    \"scenarioVersion\": "); AppendStr(sb, r.PassiveScenarioVersion ?? ""); sb.Append(",\n");
            sb.Append("    \"allocatedPassiveNodeCount\": ").Append(r.AllocatedPassiveNodeCount).Append(",\n");
            sb.Append("    \"allocatedPassiveNodeIds\": "); AppendStr(sb, r.AllocatedPassiveNodeIds ?? ""); sb.Append(",\n");
            sb.Append("    \"passiveModifierCount\": ").Append(r.PassiveModifierCount).Append(",\n");
            sb.Append("    \"passiveOffensiveTupleCount\": ").Append(r.PassiveOffensiveTupleCount).Append(",\n");
            sb.Append("    \"passiveDefensiveOrAttributeTupleCount\": ").Append(r.PassiveDefensiveOrAttributeTupleCount).Append(",\n");
            sb.Append("    \"passiveEffectiveStatSnapshot\": "); AppendStr(sb, r.PassiveEffectiveStatSnapshot ?? ""); sb.Append(",\n");
            sb.Append("    \"passiveEffectiveSkillStatSnapshot\": "); AppendStr(sb, r.PassiveEffectiveSkillStatSnapshot ?? ""); sb.Append(",\n");
            sb.Append("    \"allocationEvents\": [ ");
            for (int i = 0; i < r.PassiveAllocationEvents.Count; i++)
            {
                AppendStr(sb, r.PassiveAllocationEvents[i]);
                if (i < r.PassiveAllocationEvents.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(" ],\n");
            sb.Append("    \"passiveSensitive\": ").Append(r.PassiveSensitive ? "true" : "false").Append("\n");
            sb.Append("  },\n");

            sb.Append("  \"passiveSensitivity\": {\n");
            sb.Append("    \"predecessorHash\": "); AppendStr(sb, PredecessorHash); sb.Append(",\n");
            sb.Append("    \"allocationMutationBaselineHash\": "); AppendStr(sb, r.SensitivityAllocationBaselineHash ?? ""); sb.Append(",\n");
            sb.Append("    \"allocationMutationHash\": "); AppendStr(sb, r.SensitivityAllocationMutatedHash ?? ""); sb.Append(",\n");
            sb.Append("    \"allocationMutationChangedHash\": ").Append(r.AllocationMutationChangedHash ? "true" : "false").Append(",\n");
            sb.Append("    \"statMutationBaselineHash\": "); AppendStr(sb, r.SensitivityStatBaselineHash ?? ""); sb.Append(",\n");
            sb.Append("    \"statMutationHash\": "); AppendStr(sb, r.SensitivityStatMutatedHash ?? ""); sb.Append(",\n");
            sb.Append("    \"statMutationChangedHash\": ").Append(r.StatMutationChangedHash ? "true" : "false").Append(",\n");
            sb.Append("    \"restoredHash\": "); AppendStr(sb, r.SensitivityRestoredHash ?? ""); sb.Append(",\n");
            sb.Append("    \"restoreExactMatch\": ").Append(r.RestoreExactMatch ? "true" : "false").Append(",\n");
            sb.Append("    \"allocationOrderingInvariant\": ").Append(r.OrderingInvariant ? "true" : "false").Append("\n");
            sb.Append("  }\n");
            sb.Append("}\n");
            string path = ArtifactPath;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log("[ProductionSimulation] wrote " + path);
        }

        static void AppendStr(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"') sb.Append("\\\"");
                else if (c == '\\') sb.Append("\\\\");
                else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
