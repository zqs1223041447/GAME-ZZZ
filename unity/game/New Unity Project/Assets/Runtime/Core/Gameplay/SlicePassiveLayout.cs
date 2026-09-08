using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// Passive 展示布局（S3-P3-UI-R4-BOUNDARY，Branch A）：纯 presentation 函数——
    /// 只从 PassiveCatalog.Count/Notable/Mechanic/Links 推导 node rect 与 edge 线段，
    /// 零 gameplay 状态、零写 API、零新增 edge（仅消费 canonical Links）。
    /// 16 节点在 368×260 panel 内不重叠、1080p/1440p 有效。
    /// </summary>
    public static class SlicePassiveLayout
    {
        /// <summary>规范化布局区域尺寸（R3 已建立的角色面板内天赋区域）。</summary>
        public static float PanelWidth { get { return 368f; } }
        public static float PanelHeight { get { return 260f; } }

        /// <summary>16 节点归一化坐标（0-1 相对面板），手工设计确保非重叠+可读。</summary>
        static readonly Vector2[] Normalized =
        {
            new Vector2(0.50f, 0.08f),   // 0 心脉
            new Vector2(0.18f, 0.26f),   // 1 蛮力
            new Vector2(0.50f, 0.26f),   // 2 敏足
            new Vector2(0.82f, 0.26f),   // 3 灵思
            new Vector2(0.10f, 0.50f),   // 4 铁骨
            new Vector2(0.50f, 0.50f),   // 5 影蔽
            new Vector2(0.30f, 0.50f),   // 6 血脉
            new Vector2(0.08f, 0.72f),   // 7 粉碎
            new Vector2(0.72f, 0.72f),   // 8 余烬
            new Vector2(0.90f, 0.50f),   // 9 精准
            new Vector2(0.50f, 0.72f),   // 10 冲击
            new Vector2(0.22f, 0.90f),   // 11 残暴打击（Notable）
            new Vector2(0.72f, 0.92f),   // 12 火葬（Notable）
            new Vector2(0.50f, 0.90f),   // 13 烬心（Mechanic）
            new Vector2(0.88f, 0.72f),   // 14 厚皮
            new Vector2(0.92f, 0.92f)    // 15 搏动
        };
        public static Vector2 NodeSize(PassiveNode n)
        {
            return n.Mechanic || n.Notable ? new Vector2(72f, 34f) : new Vector2(56f, 26f);
        }

        /// <summary>节点 rect（绝对坐标，在 panel 内）。</summary>
        public static Rect NodeRect(Rect panel, int index, PassiveNode n)
        {
            var p = Normalized[index];
            var size = NodeSize(n);
            return new Rect(
                panel.x + p.x * panel.width - size.x * 0.5f,
                panel.y + p.y * panel.height - size.y * 0.5f,
                size.x, size.y);
        }

        /// <summary>节点中心。</summary>
        public static Vector2 NodeCenter(Rect panel, int index)
        {
            var p = Normalized[index];
            return new Vector2(panel.x + p.x * panel.width, panel.y + p.y * panel.height);
        }

        /// <summary>所有节点 rect（panel 内，确保不重叠）。</summary>
        public static Rect[] AllNodeRects(Rect panel, int count)
        {
            var rects = new Rect[count];
            for (int i = 0; i < count; i++)
                rects[i] = NodeRect(panel, i, PassiveCatalog.Get(i));
            return rects;
        }

        /// <summary>规范化 edge 线段列表（仅 canonical Links，零 synthetic edge）。</summary>
        public static void GetEdges(int count, System.Collections.Generic.List<Vector2> aOut, System.Collections.Generic.List<Vector2> bOut, Rect panel)
        {
            aOut.Clear();
            bOut.Clear();
            for (int i = 0; i < count; i++)
            {
                int[] links = PassiveCatalog.Get(i).Links;
                if (links == null) continue;
                var a = NodeCenter(panel, i);
                for (int k = 0; k < links.Length; k++)
                {
                    if (links[k] <= i) continue;
                    if (links[k] >= count) continue;
                    aOut.Add(a);
                    bOut.Add(NodeCenter(panel, links[k]));
                }
            }
        }
    }
}
