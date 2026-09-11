using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// S6P-WO-02 — canonical 被动场景 + canonical 哈希负载（纯 tooling 层：不改 passive runtime / 不改 canonical 数据）。
    ///
    /// 目的：把 Production Simulation 从 loot→craft→equip 扩到
    /// loot→craft→equip→passive allocation→passive modifier→effective gameplay stat，
    /// 使以后任何真实 Passive/Mastery gameplay truth 变化都能被 FNV1A64 evidence 捕获。
    ///
    /// 硬性边界（合同 §1/§10/§11）：
    ///   · fixture 只用 WO-01 census 判定为 SUPPORTED 且非专精的节点；
    ///   · 加点只走 <see cref="SliceSession.TryAllocate"/>（domain API），禁止直接写 Allocated 容器；
    ///   · 专精明确排除（EXCLUDED_BY_CONTRACT）——不能把 WO-03 即将修正的错误语义冻结成新 oracle；
    ///   · 2005 个 unsupported 节点明确排除。
    ///
    /// 哈希负载只含游戏真相（身份 / modifier 语义 / 有效属性），不含坐标、图标、贴图、UI 状态、
    /// 原始 JSON 字节或整棵 2429 节点定义（合同 §6）。
    /// </summary>
    internal static class PassiveAwareProductionSimulation
    {
        /// <summary>哈希负载 schema marker（合同 §16：显式区分"数据变了"与"evidence surface 升级了"）。</summary>
        internal const string ContractVersion = "pv|passive-v1";

        internal const string ScenarioVersion = "s6p-wo02-canonical-v1";

        /// <summary>
        /// canonical fixture —— 官方 NodeId，冻结（禁止 find-first-supported 动态挑选；合同 §3）。
        /// 出处：WO-01 census `SUPPORTED=424` 集合内的主树节点，全部是起点 2172 的直接邻居，
        /// 因此任意加点顺序都满足 connected allocation（合法性仍由 domain API 判定，不在此处假设）。
        ///   559  "Attack Speed and Dexterity"  → 进攻 CONSUMED（AttackSpeed:Increased）+ 属性 CONSUMED（Dexterity:Flat）
        ///   1795 "Physical Damage and Strength" → 进攻 CONSUMED（PhysicalDamage:Increased）+ 属性 CONSUMED（Strength:Flat）
        ///   2034 "Life and Strength"           → 防御 CONSUMED（Life:Flat）+ 属性 CONSUMED（Strength:Flat）
        /// </summary>
        internal static readonly int[] CanonicalNodeIds = { 559, 1795, 2034 };

        /// <summary>同一集合、相反加点顺序 —— 只用于证明"枚举/操作顺序不改变哈希"（合同 §8）。</summary>
        internal static readonly int[] CanonicalReverseOrderNodeIds = { 2034, 1795, 559 };

        /// <summary>敏感性 A：去掉一个 canonical 加点（仍合法、仍连通）。</summary>
        internal static readonly int[] SensitivityAllocationNodeIds = { 559, 1795 };

        /// <summary>敏感性 B：另一组合法加点，最终有效 gameplay 属性不同（少 Life / Strength / PhysicalDamage）。</summary>
        internal static readonly int[] SensitivityStatNodeIds = { 559, 2034 };

        /// <summary>起点节点（= SliceSession.StartNode），冻结值用于断言树数据未漂移。</summary>
        internal const int StartNodeId = 2172;

        /// <summary>
        /// 起点邻居里的 unsupported 反证节点（WO-01 census 判定 UnsupportedCurrently，本单禁用）。
        /// 冻结值用于断言"排除口径不是空转"。
        /// </summary>
        internal const int UnsupportedNeighborNodeId = 71;

        /// <summary>
        /// 把 fixture 加进 session（只走 domain API）。返回确定性事件 trace：
        /// `pa|&lt;nodeId&gt;|&lt;ok|reject&gt;|&lt;remainingPoints&gt;`（合同 §4 的事件面；进报告作证据，
        /// 不进 canonical 哈希 —— 哈希用下面顺序无关的语义状态，见 §8）。
        /// </summary>
        internal static List<string> Apply(SliceSession session, int[] nodeIds)
        {
            var trace = new List<string>(nodeIds.Length);
            for (int i = 0; i < nodeIds.Length; i++)
            {
                string err;
                bool ok = session.TryAllocate(nodeIds[i], out err);
                trace.Add("pa|" + nodeIds[i].ToString(CultureInfo.InvariantCulture)
                    + "|" + (ok ? "ok" : "reject")
                    + "|" + session.Unspent.ToString(CultureInfo.InvariantCulture));
            }
            return trace;
        }

        internal static List<string> ApplyCanonical(SliceSession session)
        {
            return Apply(session, CanonicalNodeIds);
        }

        /// <summary>全部已分配 NodeId，数值升序（不依赖 Allocated[] 迭代之外任何容器顺序）。</summary>
        internal static int[] AllocatedNodeIds(SliceSession session)
        {
            var list = new List<int>();
            for (int i = 0; i < session.Allocated.Length; i++)
                if (session.Allocated[i])
                    list.Add(i);
            list.Sort();
            return list.ToArray();
        }

        /// <summary>实际进入 runtime 的 passive modifier 语义元组，按 (NodeId, StatId, Op, Value) 稳定排序。</summary>
        internal static List<string> ModifierTuples(SliceSession session)
        {
            int[] ids = AllocatedNodeIds(session);
            var rows = new List<ModRow>();
            for (int i = 0; i < ids.Length; i++)
            {
                Modifier[] mods = PassiveCatalog.Get(ids[i]).Mods;
                if (mods == null)
                    continue;
                for (int m = 0; m < mods.Length; m++)
                    rows.Add(new ModRow(ids[i], mods[m]));
            }
            rows.Sort();
            var outRows = new List<string>(rows.Count);
            for (int i = 0; i < rows.Count; i++)
                outRows.Add(rows[i].Format());
            return outRows;
        }

        /// <summary>
        /// 有效 gameplay 属性快照（StatId 数值序全枚举 —— 合同 §5C 优先方案）。
        /// 每项含 有效值 / base+flat / increased / more：只给最终值会丢掉"基数为 0 的进攻轴"
        /// （+10% increased PhysicalDamage 作用在 0 基底上，Get() 仍是 0，等于看不见）。
        /// </summary>
        internal static string StatSnapshot(SliceSession session)
        {
            var sb = new StringBuilder(1024);
            for (int i = 0; i < (int)StatId.Count; i++)
            {
                if (i > 0)
                    sb.Append('|');
                sb.Append(i.ToString(CultureInfo.InvariantCulture)).Append(':')
                  .Append(((StatId)i).ToString()).Append('=');
                AppendStatComponents(sb, session.PlayerStats, (StatId)i);
            }
            return sb.ToString();
        }

        /// <summary>canonical 技能集合（q/w/e 三条真实 skill 路径）。</summary>
        internal static readonly SkillId[] CanonicalSkills = { SkillId.Melee, SkillId.Projectile, SkillId.Area };

        /// <summary>
        /// 有效技能属性快照（canonical 技能 × StatId 数值序）。
        /// 进攻向被动（AttackSpeed / PhysicalDamage 等）只进技能包、不进 PlayerStats，
        /// 所以必须单独取证，否则"点亮了但没生效"的技能路径不会被哈希看到。
        /// </summary>
        internal static string SkillStatSnapshot(SliceSession session)
        {
            var bag = new StatBag();
            var sb = new StringBuilder(2048);
            for (int k = 0; k < CanonicalSkills.Length; k++)
            {
                session.CollectSkillMods(CanonicalSkills[k], bag);
                if (k > 0)
                    sb.Append('|');
                sb.Append('s').Append(((int)CanonicalSkills[k]).ToString(CultureInfo.InvariantCulture)).Append(':');
                for (int i = 0; i < (int)StatId.Count; i++)
                {
                    if (i > 0)
                        sb.Append(',');
                    sb.Append(i.ToString(CultureInfo.InvariantCulture)).Append('=');
                    AppendStatComponents(sb, bag, (StatId)i);
                }
            }
            return sb.ToString();
        }

        static void AppendStatComponents(StringBuilder sb, StatBag bag, StatId stat)
        {
            sb.Append(Fmt(bag.Get(stat))).Append('/')
              .Append(Fmt(bag.RawFlat(stat))).Append('/')
              .Append(Fmt(bag.RawIncreased(stat))).Append('/')
              .Append(Fmt(bag.RawMore(stat)));
        }

        /// <summary>
        /// canonical 语义状态负载：顺序无关（身份升序 / modifier 排序 / 属性按 StatId 序）。
        /// 这是进 canonical 哈希的被动面；不含操作顺序、UI 状态、树几何、贴图或原始 JSON 字节。
        /// </summary>
        internal static string StatePayload(SliceSession session)
        {
            var sb = new StringBuilder(2048);
            sb.Append(ContractVersion).Append('\n');

            int[] ids = AllocatedNodeIds(session);
            sb.Append("ps|");
            for (int i = 0; i < ids.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(ids[i].ToString(CultureInfo.InvariantCulture));
            }
            sb.Append('\n');

            List<string> mods = ModifierTuples(session);
            sb.Append("pm|");
            for (int i = 0; i < mods.Count; i++)
            {
                if (i > 0)
                    sb.Append('|');
                sb.Append(mods[i]);
            }
            sb.Append('\n');

            sb.Append("pe|").Append(StatSnapshot(session));
            sb.Append('\n');
            sb.Append("pk|").Append(SkillStatSnapshot(session));
            return sb.ToString();
        }

        /// <summary>本轮 fixture 覆盖的被动影响统计（证据用；不是判据）。</summary>
        internal static int OffensiveTupleCount(SliceSession session)
        {
            int n = 0;
            List<string> rows = ModifierTuples(session);
            for (int i = 0; i < rows.Count; i++)
                if (IsOffensive(rows[i]))
                    n++;
            return n;
        }

        internal static int DefensiveOrAttributeTupleCount(SliceSession session)
        {
            int n = 0;
            List<string> rows = ModifierTuples(session);
            for (int i = 0; i < rows.Count; i++)
                if (!IsOffensive(rows[i]))
                    n++;
            return n;
        }

        static readonly StatId[] OffensiveStats =
        {
            StatId.Damage, StatId.PhysicalDamage, StatId.FireDamage, StatId.MoreDamage,
            StatId.MorePhysical, StatId.MoreFire, StatId.AddedPhysical, StatId.AddedFire,
            StatId.ConvertPhysToFire, StatId.CritChanceBase, StatId.CritChanceAdded,
            StatId.CritChanceIncreased, StatId.CritMultiAdded, StatId.IgniteChance,
            StatId.AreaRadiusMore, StatId.AreaDamageMore, StatId.AttackSpeed, StatId.Fork
        };

        static bool IsOffensive(string tuple)
        {
            // tuple 形如 "<nodeId>:<statId>:<StatName>:<opId>:<OpName>:<value>:<tags>:<condition>"
            int first = tuple.IndexOf(':');
            int second = first < 0 ? -1 : tuple.IndexOf(':', first + 1);
            if (second < 0)
                return false;
            int stat;
            string statIdText = tuple.Substring(first + 1, second - first - 1);
            if (!int.TryParse(statIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out stat))
                return false;
            for (int i = 0; i < OffensiveStats.Length; i++)
                if ((int)OffensiveStats[i] == stat)
                    return true;
            return false;
        }

        static string Fmt(float v)
        {
            return v.ToString("R", CultureInfo.InvariantCulture);
        }

        struct ModRow : IComparable<ModRow>
        {
            readonly int _nodeId;
            readonly Modifier _mod;

            internal ModRow(int nodeId, Modifier mod)
            {
                _nodeId = nodeId;
                _mod = mod;
            }

            public int CompareTo(ModRow o)
            {
                int c = _nodeId.CompareTo(o._nodeId);
                if (c != 0) return c;
                c = ((int)_mod.Stat).CompareTo((int)o._mod.Stat);
                if (c != 0) return c;
                c = ((int)_mod.Op).CompareTo((int)o._mod.Op);
                if (c != 0) return c;
                c = _mod.Value.CompareTo(o._mod.Value);
                if (c != 0) return c;
                c = ((uint)_mod.RequiredTags).CompareTo((uint)o._mod.RequiredTags);
                if (c != 0) return c;
                return ((int)_mod.Condition).CompareTo((int)o._mod.Condition);
            }

            internal string Format()
            {
                return _nodeId.ToString(CultureInfo.InvariantCulture) + ":"
                    + ((int)_mod.Stat).ToString(CultureInfo.InvariantCulture) + ":" + _mod.Stat + ":"
                    + ((int)_mod.Op).ToString(CultureInfo.InvariantCulture) + ":" + _mod.Op + ":"
                    + Fmt(_mod.Value) + ":"
                    + ((uint)_mod.RequiredTags).ToString(CultureInfo.InvariantCulture) + ":"
                    + ((int)_mod.Condition).ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
