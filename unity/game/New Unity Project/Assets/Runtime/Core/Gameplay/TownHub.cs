using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 主城第一增量（导演 2026-09-12）：Town 不再只是 MapState.Town 空场。
    /// 纯函数：身份 / NPC 绑定 / 进图入口；绘制在 SliceHud，进图仍走 SliceSession.TryEnterMap。
    /// 不改战斗生成、不改 loot、不新增地图。
    /// </summary>
    public enum TownNpcKind : byte
    {
        Stash = 0,
        Crafter = 1,
        Waystone = 2
    }

    public static class TownHub
    {
        public const string Id = "ember-town";
        public const string DisplayName = "烬城";
        public const string MapId = "ash-court";
        public const string MapDisplayName = "灰烬庭院";
        public const int NpcCount = 3;

        public static bool IsHubActive(SliceSession s)
        {
            return s != null && s.State == MapState.Town;
        }

        public static Rect Plaza(float dw, float dh)
        {
            float right = SliceDrawerLayout.ColumnX(dw) - 16f;
            float top = 96f;
            float bottom = dh - 180f;
            return new Rect(20f, top, Mathf.Max(280f, right - 20f), Mathf.Max(160f, bottom - top));
        }

        public static TownNpcKind NpcKind(int index)
        {
            if (index == 1) return TownNpcKind.Crafter;
            if (index == 2) return TownNpcKind.Waystone;
            return TownNpcKind.Stash;
        }

        public static string NpcName(int index)
        {
            switch (NpcKind(index))
            {
                case TownNpcKind.Crafter: return "工匠";
                case TownNpcKind.Waystone: return "地图官";
                default: return "仓库管事";
            }
        }

        public static string NpcFunctionLabel(int index)
        {
            switch (NpcKind(index))
            {
                case TownNpcKind.Crafter: return "制作";
                case TownNpcKind.Waystone: return "进入 " + MapDisplayName;
                default: return "打开背包";
            }
        }

        public static Rect NpcRect(int index, float dw, float dh)
        {
            Rect plaza = Plaza(dw, dh);
            float w = 200f;
            float h = 72f;
            float gap = 16f;
            float rowW = NpcCount * w + (NpcCount - 1) * gap;
            float x0 = plaza.x + Mathf.Max(12f, (plaza.width - rowW) * 0.5f);
            float y = plaza.y + plaza.height * 0.42f;
            return new Rect(x0 + index * (w + gap), y, w, h);
        }

        public static int HitNpc(Vector2 designPointer, float dw, float dh)
        {
            if (!Plaza(dw, dh).Contains(designPointer))
                return -1;
            for (int i = 0; i < NpcCount; i++)
            {
                if (NpcRect(i, dw, dh).Contains(designPointer))
                    return i;
            }
            return -1;
        }

        /// <summary>NPC 绑定：仓库=开背包；工匠=制作面板；地图官=地图面板（进图仍走 TryEnterMapFromHub）。</summary>
        public static bool ApplyNpc(SliceSession s, int npcIndex, out string error)
        {
            error = null;
            if (s == null)
            {
                error = "无会话";
                return false;
            }
            if (!IsHubActive(s))
            {
                error = "仅主城可与 NPC 交互";
                return false;
            }
            if (npcIndex < 0 || npcIndex >= NpcCount)
            {
                error = "未知 NPC";
                return false;
            }
            switch (NpcKind(npcIndex))
            {
                case TownNpcKind.Stash:
                    s.BagOpen = true;
                    s.LastMessage = "仓库管事：背包已打开";
                    return true;
                case TownNpcKind.Crafter:
                    s.Panel = SlicePanel.Craft;
                    s.LastMessage = "工匠：制作台已打开";
                    return true;
                default:
                    s.Panel = SlicePanel.Map;
                    s.LastMessage = "地图官：选择词缀后进入" + MapDisplayName;
                    return true;
            }
        }

        /// <summary>主城侧进图：唯一走既有 TryEnterMap（灰烬庭院），禁止第二套进图。</summary>
        public static bool TryEnterMapFromHub(SliceSession s, ArenaSim sim, out string error)
        {
            error = null;
            if (s == null)
            {
                error = "无会话";
                return false;
            }
            if (!IsHubActive(s) && s.State != MapState.Dead)
            {
                error = "仅主城可从枢纽进图";
                return false;
            }
            return s.TryEnterMap(sim, out error);
        }
    }
}
