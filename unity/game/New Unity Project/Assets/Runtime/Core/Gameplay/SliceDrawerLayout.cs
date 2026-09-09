using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// Phase 3 R2 右侧装备抽屉的纯几何布局（工作令 S3-P3-UI-R2-EQUIPMENT-DRAWER）。
    /// 全部为设计空间（1920×1080 基准）纯函数，供 SliceHud 与 EditMode 几何测试共用；
    /// 不含任何状态与绘制。槽位映射=现有 canonical EquipSlot（Weapon/Body/Helmet/Boots），不新增 schema。
    /// </summary>
    public static class SliceDrawerLayout
    {
        public const float ColumnW = 300f;
        /// <summary>S4-P2：六槽 2×3（3 行）后加高列；旧 232=2 行，新 326=26 头 + 3×(82+6) + tab 行。</summary>
        public const float ColumnH = 326f;
        public const float ColumnTop = 64f;
        public const float ScreenMargin = 12f;
        public const float SlotW = 138f;
        public const float SlotH = 82f;
        public const float Gap = 6f;
        public const float TabH = 26f;
        public const float BuildPanelW = 760f;
        public const float BuildPanelH = 430f;
        public const float CraftPanelW = 560f;
        public const float CraftPanelH = 340f;
        public const float PanelGap = 12f;

        /// <summary>常驻抽屉列（右缘留 ScreenMargin）。</summary>
        public static Rect Column(float dw)
        {
            return new Rect(dw - ScreenMargin - ColumnW, ColumnTop, ColumnW, ColumnH);
        }

        /// <summary>迷你装备槽（S4-P2 起 2 列 × 3 行；index 0..5 = UI 展示顺序，见 DisplayOrder）。</summary>
        public static Rect Slot(int index, float dw)
        {
            int col = index % 2;
            int row = index / 2;
            Rect c = Column(dw);
            return new Rect(c.x + 6f + col * (SlotW + Gap), c.y + 26f + row * (SlotH + Gap), SlotW, SlotH);
        }

        /// <summary>UI 展示顺序（人体逻辑）与 EquipSlot stable numeric ID 分离（工作令 十九）：不为 UI 顺序改枚举 ID。</summary>
        public static readonly EquipSlot[] DisplayOrder =
        {
            EquipSlot.Weapon, EquipSlot.Helmet,
            EquipSlot.Body, EquipSlot.Gloves,
            EquipSlot.Boots, EquipSlot.Belt
        };

        /// <summary>抽屉底部 Build/Craft 两个 tab 按钮（各占一半宽）。</summary>
        public static Rect Tab(int index, float dw)
        {
            Rect c = Column(dw);
            float w = (ColumnW - 12f - Gap) * 0.5f;
            return new Rect(c.x + 6f + index * (w + Gap), c.y + ColumnH - TabH - 6f, w, TabH);
        }

        /// <summary>Build 内容面板（现有 760×430 渲染器右锚迁移；列左侧）。</summary>
        public static Rect BuildPanel(float dw)
        {
            return new Rect(ColumnX(dw) - PanelGap - BuildPanelW, ColumnTop, BuildPanelW, BuildPanelH);
        }

        /// <summary>Craft 内容面板（现有 560×340 渲染器右锚迁移；列左侧）。</summary>
        public static Rect CraftPanel(float dw)
        {
            return new Rect(ColumnX(dw) - PanelGap - CraftPanelW, ColumnTop, CraftPanelW, CraftPanelH);
        }

        public static float ColumnX(float dw)
        {
            return dw - ScreenMargin - ColumnW;
        }
    }
}
