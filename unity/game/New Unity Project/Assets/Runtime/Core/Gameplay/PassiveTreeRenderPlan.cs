using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S6P-WO-05：被动树当帧 render plan（presentation only，不写 gameplay）。
    /// </summary>
    public sealed class PassiveTreeRenderPlan
    {
        public const float NodeVisiblePad = 20f;
        public const float EdgeVisiblePad = 60f;

        public PassiveTreeLod.Level Lod;
        public float DesignScale;
        public float Zoom;
        public Vector2 Pan;
        public Rect Viewport;
        public bool[] VisibleNodes;
        public int VisibleNodeCount;
        public int VisibleEdgeCount;
        public int RequiredIconCount;
        public bool NeedFrameAtlas;
        public bool NeedGroupAtlas;
        public HashSet<string> RequiredIconStems = new HashSet<string>();

        public static PassiveTreeRenderPlan Build(Rect viewport, Vector2 pan, float zoom, float designScale)
        {
            var p = new PassiveTreeRenderPlan();
            p.Viewport = viewport;
            p.Pan = pan;
            p.Zoom = zoom;
            p.DesignScale = designScale <= 0f ? 1f : designScale;
            p.Lod = PassiveTreeLod.FromZoom(zoom, p.DesignScale);
            PoeNode[] nodes = PoeTree.Nodes;
            int n = nodes == null ? 0 : nodes.Length;
            p.VisibleNodes = new bool[n];
            for (int i = 0; i < n; i++)
            {
                if (!PoeTreeView.Visible(nodes[i], pan, zoom, viewport, NodeVisiblePad))
                    continue;
                p.VisibleNodes[i] = true;
                p.VisibleNodeCount++;
                if (PassiveTreeLod.AllowsFullIcon(p.Lod, nodes[i].Kind) && !string.IsNullOrEmpty(nodes[i].icon))
                {
                    if (p.RequiredIconStems.Add(nodes[i].icon))
                        p.RequiredIconCount++;
                }
            }
            for (int i = 0; i < n; i++)
            {
                int[] links = nodes[i].links;
                if (links == null)
                    continue;
                Vector2 a = PoeTreeView.ScreenOf(new Vector2(nodes[i].x, nodes[i].y), pan, zoom);
                for (int k = 0; k < links.Length; k++)
                {
                    int j = links[k];
                    if (j <= i)
                        continue;
                    Vector2 b = PoeTreeView.ScreenOf(new Vector2(nodes[j].x, nodes[j].y), pan, zoom);
                    if (IsEdgeVisible(a, b, viewport, EdgeVisiblePad))
                        p.VisibleEdgeCount++;
                }
            }
            p.NeedFrameAtlas = p.Lod == PassiveTreeLod.Level.Detail
                || p.Lod == PassiveTreeLod.Level.Mid;
            PoeGroup[] groups = PoeTree.Groups;
            if (groups != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    float diam = PassiveTreeLod.GroupProjectedDiameterPx(i, zoom, p.DesignScale);
                    if (PassiveTreeLod.AllowsGroupDecoration(p.Lod, diam))
                    {
                        p.NeedGroupAtlas = true;
                        break;
                    }
                }
            }
            if (p.Lod == PassiveTreeLod.Level.Overview)
            {
                p.NeedFrameAtlas = false;
                p.NeedGroupAtlas = false;
            }
            return p;
        }

        public static bool IsEdgeVisible(Vector2 a, Vector2 b, Rect viewport, float pad)
        {
            float minX = Mathf.Min(a.x, b.x) - pad, maxX = Mathf.Max(a.x, b.x) + pad;
            float minY = Mathf.Min(a.y, b.y) - pad, maxY = Mathf.Max(a.y, b.y) + pad;
            return maxX >= viewport.x && minX <= viewport.xMax && maxY >= viewport.y && minY <= viewport.yMax;
        }
    }
}
