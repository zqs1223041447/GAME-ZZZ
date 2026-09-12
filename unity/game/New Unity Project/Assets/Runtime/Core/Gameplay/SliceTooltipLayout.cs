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
        public const float InnerPad = 12f;
        public const float TitleH = 20f;
        public const float LineH = 16f;
        public const float BodyFont = 14f;

        public static float InnerW { get { return BaseW - InnerPad * 2f; } }

        /// <summary>描述正文行高：含 word-wrap，不得按单行 15px 裁字。</summary>
        public static float BodyLineHeight(string line)
        {
            return SliceHud.EstimateWrappedHeight(line, InnerW, BodyFont);
        }

        public static float CardHeight(SliceTooltipModel.Card card)
        {
            const float pad = 10f;
            bool hasHead = !string.IsNullOrEmpty(card.Title) || !string.IsNullOrEmpty(card.Subtitle) ||
                !string.IsNullOrEmpty(card.Badge);
            bool hasBody = card.Body != null && card.Body.Length > 0;
            bool hasCtx = !string.IsNullOrEmpty(card.ContextTitle) || (card.Context != null && card.Context.Length > 0);
            float h = pad * 2f;
            if (hasHead && hasBody) h += 8f;
            if (hasBody && hasCtx) h += 8f;
            if (!string.IsNullOrEmpty(card.Title)) h += TitleH;
            if (!string.IsNullOrEmpty(card.Subtitle)) h += LineH;
            if (!string.IsNullOrEmpty(card.Badge)) h += LineH;
            if (card.Body != null)
            {
                for (int i = 0; i < card.Body.Length; i++)
                    h += BodyLineHeight(card.Body[i]);
            }
            if (!string.IsNullOrEmpty(card.ContextTitle)) h += 18f;
            if (card.Context != null)
            {
                for (int i = 0; i < card.Context.Length; i++)
                    h += BodyLineHeight(card.Context[i]);
            }
            if (!string.IsNullOrEmpty(card.Footer)) h += LineH;
            return h;
        }

        public static GUIStyle DescriptionStyle()
        {
            var st = new GUIStyle();
            st.fontSize = (int)BodyFont;
            st.wordWrap = true;
            st.clipping = TextClipping.Overflow;
            st.richText = false;
            return st;
        }

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
