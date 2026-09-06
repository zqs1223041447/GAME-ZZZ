using UnityEngine;

namespace Game.Runtime.Core
{
    public static class SliceCopy
    {
        public const string CombatLocked = "战斗中锁定";
        public const string LockFail = "图内锁定构筑";
        public const string Cleared = "清场可改构筑，可出图";
        public const string TownFree = "出图后可免费重构";
        public const string DeadRespec = "死亡出图，已免费重构";
        public const string AffixLocked = "进图后不能改词缀";
        public const string ExitFirst = "请先出图";
        public const string SupportInUse = "该辅助已装在其他技能上";
    }

    public static class SlicePalette
    {
        public static readonly Color Panel = new Color(0.09f, 0.10f, 0.12f, 0.94f);
        public static readonly Color PanelEdge = new Color(0.78f, 0.64f, 0.28f, 1f);
        public static readonly Color Text = new Color(0.92f, 0.90f, 0.84f, 1f);
        public static readonly Color Dim = new Color(0.62f, 0.60f, 0.55f, 1f);
        public static readonly Color Ordinary = new Color(0.78f, 0.78f, 0.80f, 1f);
        public static readonly Color Rare = new Color(0.95f, 0.78f, 0.22f, 1f);
        public static readonly Color NodeOn = new Color(0.93f, 0.80f, 0.32f, 1f);
        public static readonly Color NodeAvail = new Color(0.38f, 0.72f, 0.46f, 1f);
        public static readonly Color NodeLock = new Color(0.28f, 0.28f, 0.30f, 1f);
        public static readonly Color Notable = new Color(1f, 0.86f, 0.40f, 1f);
        public static readonly Color Cinder = new Color(1f, 0.42f, 0.12f, 1f);
        public static readonly Color Brute = new Color(0.55f, 0.22f, 0.16f, 1f);
        public static readonly Color Stinger = new Color(0.32f, 0.78f, 0.30f, 1f);
        public static readonly Color Ashling = new Color(0.95f, 0.42f, 0.12f, 1f);
        public static readonly Color Warden = new Color(0.52f, 0.24f, 0.78f, 1f);
        public static readonly Color Life = new Color(0.75f, 0.18f, 0.16f, 1f);
        public static readonly Color Mana = new Color(0.22f, 0.40f, 0.82f, 1f);
    }

    public enum NodeUiState : byte
    {
        Allocated = 0,
        Available = 1,
        Locked = 2
    }
}
