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

        // ---------------- S5U-WO-03：抽屉壳（Shell）单一来源（角色/背包页；旧 Column 系保留供 Build 面板与既有测试） ----------------

        /// <summary>壳宽（较旧列 300 加宽 12 供 3 列网格）。</summary>
        public const float ShellW = 312f;
        /// <summary>壳顶（与旧列同高起点）。</summary>
        public const float ShellTop = 64f;
        /// <summary>壳底距设计底 =224：底缘 dh-160，在战斗栏顶（dh-152）上方 8px，两层永不重叠。</summary>
        public const float ShellBottomGap = 224f;
        public const float HeaderH = 26f;
        public const float ShellTabH = 26f;
        public const float ShellSlotW = 146f;
        public const float ShellSlotH = 86f;
        public const float CellW = 92f;
        public const float CellH = 70f;
        public const float GridGap = 4f;
        public const float FooterH = 20f;
        /// <summary>装备区/背包区段标签行高。</summary>
        public const float SectionLabelH = 16f;

        /// <summary>抽屉壳外框（右缘 ScreenMargin；底缘避开战斗栏）。</summary>
        public static Rect Shell(float dw, float dh)
        {
            return new Rect(dw - ScreenMargin - ShellW, ShellTop, ShellW, dh - ShellBottomGap - ShellTop);
        }

        /// <summary>壳内标题条。</summary>
        public static Rect ShellHeader(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x, s.y, s.width, HeaderH);
        }

        /// <summary>壳内 Build/Craft 两 tab（语义=现有 Panel 切换，仅视觉重做）。</summary>
        public static Rect ShellTab(int index, float dw, float dh)
        {
            var s = Shell(dw, dh);
            float w = (ShellW - 12f - Gap) * 0.5f;
            return new Rect(s.x + 6f + index * (w + Gap), s.y + HeaderH + 4f, w, ShellTabH);
        }

        /// <summary>装备区 2×3 槽（index 0..5=DisplayOrder；恒 6 槽，不新增槽位）。</summary>
        public static Rect ShellSlot(int index, float dw, float dh)
        {
            var s = Shell(dw, dh);
            float y0 = s.y + HeaderH + 4f + ShellTabH + 4f + SectionLabelH;
            int col = index % 2;
            int row = index / 2;
            return new Rect(s.x + 6f + col * (ShellSlotW + Gap), y0 + row * (ShellSlotH + Gap), ShellSlotW, ShellSlotH);
        }

        /// <summary>背包网格可视区（滚动视口；内容坐标见 ShellInvContentSize/ShellInvCell）。
        /// 小视口（dh&lt;~880）时壳高压到下限 24px 仍保持滚动可用，且永不压反馈条。</summary>
        public static Rect ShellInvView(float dw, float dh)
        {
            var s = Shell(dw, dh);
            float y0 = s.y + HeaderH + 4f + ShellTabH + 4f + SectionLabelH + 3f * (ShellSlotH + Gap) + SectionLabelH + 2f;
            float bottom = s.yMax - FooterH - 6f;
            return new Rect(s.x + 8f, y0, ShellW - 16f, Mathf.Max(24f, bottom - y0));
        }

        /// <summary>网格内容总高（rows=ceil(count/3)；内容坐标，供 ScrollView）。</summary>
        public static float ShellInvContentHeight(int count)
        {
            int rows = (count + 2) / 3;
            return rows * (CellH + GridGap);
        }

        /// <summary>网格单元矩形（内容坐标：col=i%3, row=i/3；调用方叠加滚动偏移）。</summary>
        public static Rect ShellInvCell(int index)
        {
            int col = index % 3;
            int row = index / 3;
            return new Rect(col * (CellW + GridGap), row * (CellH + GridGap), CellW, CellH);
        }

        /// <summary>壳底反馈条（LastMessage/拾取反馈）。</summary>
        public static Rect ShellFooter(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x + 6f, s.yMax - FooterH - 2f, ShellW - 12f, FooterH);
        }
    }
}
