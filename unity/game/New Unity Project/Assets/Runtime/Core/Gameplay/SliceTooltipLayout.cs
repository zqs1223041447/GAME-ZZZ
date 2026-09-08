using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>R3 Tooltip 纯几何定位（工作令 十九）：设计空间视口内——默认 pointer 右下；
    /// 右侧越界翻到指针左侧；下方越界向上；最后钳到视口。供 SliceHud 与 EditMode 几何测试共用。</summary>
    public static class SliceTooltipLayout
    {
        public const float BaseW = 360f;   // 工作令推荐 340–380 带内
        public const float OffsetX = 14f;
        public const float OffsetY = 16f;
        public const float Margin = 8f;

        public static Rect Place(Vector2 pointer, float w, float h, float vw, float vh)
        {
            float x = pointer.x + OffsetX;
            float y = pointer.y + OffsetY;
            if (x + w > vw - Margin)
                x = pointer.x - w - OffsetX;
            if (y + h > vh - Margin)
                y = pointer.y - h - OffsetY;
            float maxX = Mathf.Max(Margin, vw - Margin - w);
            float maxY = Mathf.Max(Margin, vh - Margin - h);
            return new Rect(Mathf.Clamp(x, Margin, maxX), Mathf.Clamp(y, Margin, maxY), w, h);
        }
    }
}
