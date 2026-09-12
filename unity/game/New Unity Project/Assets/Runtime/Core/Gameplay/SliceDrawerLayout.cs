using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 背包面板的纯几何布局（2026-09-10 导演指令：完全贴右边、上下通顶、扩大格子显示量）。
    /// 全部为设计空间（1920×1080 基准）纯函数，供 SliceHud 与 EditMode 几何测试共用；
    /// 不含任何状态与绘制。槽位映射=现有 canonical EquipSlot（六槽，不新增 schema）。
    /// 面板右缘贴合屏幕右缘、上缘贴合屏幕顶、下缘贴合屏幕底（零外边距）。
    /// </summary>
    public static class SliceDrawerLayout
    {
        /// <summary>面板宽（12 列满幅格子 812 + 两侧 18 内衬 + 滚动条余量）。</summary>
        public const float PanelW = 850f;
        public const float PadX = 18f;
        public const float HeaderH = 40f;
        public const float TabH = 24f;
        public const float SectionLabelH = 18f;
        public const float FooterH = 26f;

        /// <summary>装备卡（3 列 × 2 行 = 六槽，人体顺序见 DisplayOrder）。</summary>
        public const float CardGap = 8f;
        public const float CardW = (PanelW - 2f * PadX - 2f * CardGap) / 3f;
        public const float CardH = 96f;

        /// <summary>背包网格：12 列；宝石与物品共用同一滚动区（不再单独托盘）。</summary>
        public const int InvColumns = 12;
        public const float CellW = 64f;
        public const float CellH = 60f;
        public const float GridGap = 4f;

        /// <summary>Build 内容面板（天赋树为全屏表面，此处仅保留旧调用方的右锚参照）。</summary>
        public const float BuildPanelW = 760f;
        public const float BuildPanelH = 430f;
        public const float CraftPanelW = 560f;
        public const float CraftPanelH = 340f;
        public const float PanelGap = 12f;

        // ---- 纵向节奏（设计像素；自顶向下累加，全部派生自常量以免漂移） ----
        public const float EquipLabelY = HeaderH + 6f;
        public const float EquipY = EquipLabelY + SectionLabelH + 2f;
        public const float EquipZoneH = CardH * 2f + CardGap;
        public const float InvLabelY = EquipY + EquipZoneH + 12f;
        public const float InvViewY = InvLabelY + SectionLabelH + 2f;

        public static int GemOccupantCount { get { return SupportCatalog.Count; } }
        public static int SharedBagCellCount { get { return GemOccupantCount + SliceRules.InventoryCap; } }
        public static bool CellIsGem(int cell)
        {
            return cell >= 0 && cell < GemOccupantCount;
        }
        public static SupportId GemInCell(int cell)
        {
            return CellIsGem(cell) ? (SupportId)(cell + 1) : SupportId.None;
        }
        public static int ItemIndexForCell(int cell)
        {
            return cell - GemOccupantCount;
        }

        /// <summary>背包面板外框：右缘/上缘/下缘全部贴合屏幕边（导演指令）。</summary>
        public static Rect Shell(float dw, float dh)
        {
            return new Rect(dw - PanelW, 0f, PanelW, dh);
        }

        public static float ColumnX(float dw)
        {
            return dw - PanelW;
        }

        /// <summary>窗内标题条（标题 + 关闭 X）。</summary>
        public static Rect ShellHeader(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x, s.y, s.width, HeaderH);
        }

        /// <summary>窗内 角色/制作 两 tab（语义=现有 Panel 切换）。</summary>
        public static Rect ShellTab(int index, float dw, float dh)
        {
            var s = Shell(dw, dh);
            float w = 76f;
            return new Rect(s.xMax - PadX - (2 - index) * (w + 6f), s.y + 8f, w, TabH);
        }

        /// <summary>“装备”节题行。</summary>
        public static Rect EquipLabel(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x + PadX, s.y + EquipLabelY, s.width - 2f * PadX, SectionLabelH);
        }

        /// <summary>装备卡（index 0..5 = DisplayOrder 顺序；3 列 × 2 行）。</summary>
        public static Rect ShellSlot(int index, float dw, float dh)
        {
            var s = Shell(dw, dh);
            int col = index % 3;
            int row = index / 3;
            return new Rect(s.x + PadX + col * (CardW + CardGap),
                s.y + EquipY + row * (CardH + CardGap), CardW, CardH);
        }

        /// <summary>“背包”节题行。</summary>
        public static Rect InvLabel(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x + PadX, s.y + InvLabelY, s.width - 2f * PadX, SectionLabelH);
        }

        /// <summary>背包网格可视区（12 列滚动视口；内容坐标见 ShellInvContentHeight/ShellInvCell）。</summary>
        public static Rect ShellInvView(float dw, float dh)
        {
            var s = Shell(dw, dh);
            float y0 = s.y + InvViewY;
            float bottom = s.yMax - FooterH - 6f;
            return new Rect(s.x + PadX, y0, s.width - 2f * PadX, Mathf.Max(CellH, bottom - y0));
        }

        /// <summary>网格内容总高（rows=ceil(count/12)；内容坐标，供 ScrollView）。</summary>
        public static float ShellInvContentHeight(int count)
        {
            int rows = (count + InvColumns - 1) / InvColumns;
            return rows * (CellH + GridGap);
        }

        /// <summary>网格单元矩形（内容坐标：col=i%12, row=i/12；调用方叠加滚动偏移）。</summary>
        public static Rect ShellInvCell(int index)
        {
            int col = index % InvColumns;
            int row = index / InvColumns;
            return new Rect(col * (CellW + GridGap), row * (CellH + GridGap), CellW, CellH);
        }

        /// <summary>窗底反馈条（LastMessage/拾取反馈）。</summary>
        public static Rect ShellFooter(float dw, float dh)
        {
            var s = Shell(dw, dh);
            return new Rect(s.x + PadX, s.yMax - FooterH - 2f, s.width - 2f * PadX, FooterH);
        }

        /// <summary>UI 展示顺序（人体逻辑）与 EquipSlot stable numeric ID 分离（工作令 十九）：不为 UI 顺序改枚举 ID。</summary>
        public static readonly EquipSlot[] DisplayOrder =
        {
            EquipSlot.Weapon, EquipSlot.Helmet,
            EquipSlot.Body, EquipSlot.Gloves,
            EquipSlot.Boots, EquipSlot.Belt
        };

        /// <summary>Build 内容面板（现有 760×430 渲染器右锚迁移；列左侧）。</summary>
        public static Rect BuildPanel(float dw)
        {
            return new Rect(ColumnX(dw) - PanelGap - BuildPanelW, 64f, BuildPanelW, BuildPanelH);
        }

        /// <summary>Craft 内容面板（现有 560×340 渲染器右锚迁移；列左侧）。</summary>
        public static Rect CraftPanel(float dw)
        {
            return new Rect(ColumnX(dw) - PanelGap - CraftPanelW, 64f, CraftPanelW, CraftPanelH);
        }
    }
}
