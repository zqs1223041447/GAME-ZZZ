using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 真实 PoE 天赋树的纯几何层（无状态、无绘制）：世界坐标→屏幕坐标的平移/缩放变换、
    /// 节点尺寸、簇底衬缩放、视口裁剪。供 SliceHud 与 EditMode 几何测试共用同一真相源。
    /// 世界坐标 = 官方天赋树坐标（y 向下为正，与屏幕一致）；zoom=1 时 1 世界单位=1 设计像素。
    /// </summary>
    public static class PoeTreeView
    {
        public const float MinZoom = 0.02f;
        public const float MaxZoom = 1.6f;

        /// <summary>节点框绘制边长（zoom=1 的设计像素；与官方 frame 图集切片尺寸同源比例）。</summary>
        public static float NodeSize(int kind)
        {
            switch ((PoeNodeKind)kind)
            {
                case PoeNodeKind.Keystone: return 104f;
                case PoeNodeKind.Notable: return 72f;
                case PoeNodeKind.Mastery: return 84f;
                case PoeNodeKind.Jewel: return 72f;
                case PoeNodeKind.Start: return 104f;
                default: return 46f;
            }
        }

        public static Vector2 ScreenOf(Vector2 world, Vector2 pan, float zoom)
        {
            return new Vector2(pan.x + world.x * zoom, pan.y + world.y * zoom);
        }

        public static Vector2 WorldOf(Vector2 screen, Vector2 pan, float zoom)
        {
            if (zoom <= 0f)
                zoom = MinZoom;
            return new Vector2((screen.x - pan.x) / zoom, (screen.y - pan.y) / zoom);
        }

        public static Rect NodeRect(PoeNode n, Vector2 pan, float zoom)
        {
            float s = NodeSize(n.kind) * zoom;
            Vector2 c = ScreenOf(new Vector2(n.x, n.y), pan, zoom);
            return new Rect(c.x - s * 0.5f, c.y - s * 0.5f, s, s);
        }

        /// <summary>世界坐标包围盒（整棵树的尺寸；初始视图与缩放下限由此推导）。</summary>
        public static Rect WorldBounds()
        {
            var d = PoeTree.Data;
            if (d == null || d.meta == null)
                return new Rect(-1000f, -1000f, 2000f, 2000f);
            var m = d.meta;
            return new Rect(m.minX, m.minY, m.maxX - m.minX, m.maxY - m.minY);
        }

        /// <summary>让整棵树刚好充满视口所需的缩放（留边距）。</summary>
        public static float FitZoom(Rect viewport, float padding)
        {
            Rect b = WorldBounds();
            if (b.width <= 0f || b.height <= 0f || viewport.width <= 0f || viewport.height <= 0f)
                return 1f;
            float z = Mathf.Min((viewport.width - padding * 2f) / b.width, (viewport.height - padding * 2f) / b.height);
            return Mathf.Clamp(z, MinZoom, MaxZoom);
        }

        /// <summary>把整棵树居中放进视口的平移量。</summary>
        public static Vector2 FitPan(Rect viewport, float zoom)
        {
            Rect b = WorldBounds();
            Vector2 center = new Vector2(b.x + b.width * 0.5f, b.y + b.height * 0.5f);
            return new Vector2(viewport.x + viewport.width * 0.5f - center.x * zoom,
                viewport.y + viewport.height * 0.5f - center.y * zoom);
        }

        /// <summary>以某屏幕点为锚缩放（滚轮缩放保持指针下的世界点不动）。</summary>
        public static Vector2 ZoomAround(Vector2 pan, float oldZoom, float newZoom, Vector2 anchor)
        {
            newZoom = Mathf.Clamp(newZoom, MinZoom, MaxZoom);
            if (oldZoom <= 0f)
                return pan;
            return new Vector2(anchor.x - (anchor.x - pan.x) * (newZoom / oldZoom),
                anchor.y - (anchor.y - pan.y) * (newZoom / oldZoom));
        }

        /// <summary>节点是否落在视口内（含外扩，供绘制裁剪；2429 节点必须裁剪）。</summary>
        public static bool Visible(PoeNode n, Vector2 pan, float zoom, Rect viewport, float pad)
        {
            Vector2 c = ScreenOf(new Vector2(n.x, n.y), pan, zoom);
            float r = NodeSize(n.kind) * zoom * 0.5f + pad;
            return c.x + r >= viewport.x && c.x - r <= viewport.xMax &&
                   c.y + r >= viewport.y && c.y - r <= viewport.yMax;
        }

        /// <summary>簇底衬的绘制矩形（把官方底衬贴图缩放到刚好包住簇内节点）。</summary>
        public static Rect GroupRect(PoeGroup g, float spriteW, float spriteH, Vector2 pan, float zoom)
        {
            float r = g.radius * zoom;
            float w = r * 2f;
            // 按宽度贴合并保持贴图长宽比：圆形底衬（1/2 档）得到圆，宽底衬（3 档）得到扁椭圆。
            float h = spriteW > 0f ? w * (spriteH / spriteW) : w;
            Vector2 c = ScreenOf(new Vector2(g.x, g.y), pan, zoom);
            return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
        }

        // ================= 簇几何（从成员节点推导，不用 canonical 的 group.x/y） =================
        // 实测：canonical 数据里 `group.x/y` 与节点 `x/y` **不在同一坐标系**——同一簇两者差值从数百到
        // 上万世界单位且逐簇不同（例：start 簇 group.y=899.8 vs 节点质心 6.8；group 42 差 6073）。
        // 节点坐标才与官方渲染对得上，因此底衬中心/半径一律由成员节点推导；无成员节点的簇回退到数据值。

        static Vector2[] _groupCentre;
        static float[] _groupRadius;

        static void EnsureGroupGeometry()
        {
            PoeGroup[] groups = PoeTree.Groups;
            if (groups == null || groups.Length == 0)
                return;
            if (_groupCentre != null && _groupCentre.Length == groups.Length)
                return;

            _groupCentre = new Vector2[groups.Length];
            _groupRadius = new float[groups.Length];
            var memberCount = new int[groups.Length];
            for (int i = 0; i < groups.Length; i++)
            {
                _groupCentre[i] = new Vector2(groups[i].x, groups[i].y);   // 无成员时的回退
                _groupRadius[i] = groups[i].radius;
            }

            PoeNode[] nodes = PoeTree.Nodes;
            if (nodes == null)
                return;

            for (int i = 0; i < nodes.Length; i++)
            {
                int g = nodes[i].group;
                if (g < 0 || g >= groups.Length)
                    continue;
                _groupCentre[g].x += nodes[i].x;
                _groupCentre[g].y += nodes[i].y;
                memberCount[g]++;
            }
            for (int i = 0; i < groups.Length; i++)
            {
                if (memberCount[i] == 0)
                    continue;
                _groupCentre[i] /= memberCount[i];
                _groupRadius[i] = 0f;
            }
            for (int i = 0; i < nodes.Length; i++)
            {
                int g = nodes[i].group;
                if (g < 0 || g >= groups.Length || memberCount[g] == 0)
                    continue;
                float dx = nodes[i].x - _groupCentre[g].x;
                float dy = nodes[i].y - _groupCentre[g].y;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > _groupRadius[g])
                    _groupRadius[g] = d;
            }
        }

        /// <summary>簇中心（成员节点质心；无成员节点时回退 canonical 值）。</summary>
        public static Vector2 GroupCentre(int groupIndex)
        {
            EnsureGroupGeometry();
            PoeGroup[] groups = PoeTree.Groups;
            if (groups == null || groupIndex < 0 || groupIndex >= groups.Length)
                return Vector2.zero;
            return _groupCentre != null ? _groupCentre[groupIndex] : new Vector2(groups[groupIndex].x, groups[groupIndex].y);
        }

        /// <summary>簇半径（成员节点最大环半径；无成员节点时回退 canonical 值）。</summary>
        public static float GroupRadius(int groupIndex)
        {
            EnsureGroupGeometry();
            PoeGroup[] groups = PoeTree.Groups;
            if (groups == null || groupIndex < 0 || groupIndex >= groups.Length)
                return 0f;
            return _groupRadius != null ? _groupRadius[groupIndex] : 0f;
        }

        /// <summary>簇底衬绘制矩形（中心/半径来自成员节点；10% 外扩让节点落在底衬之内）。</summary>
        public static Rect GroupRect(int groupIndex, float spriteW, float spriteH, Vector2 pan, float zoom)
        {
            PoeGroup[] groups = PoeTree.Groups;
            if (groups == null || groupIndex < 0 || groupIndex >= groups.Length)
                return default;
            float r = Mathf.Max(8f, GroupRadius(groupIndex) * 1.10f) * zoom;
            float w = r * 2f;
            float h = spriteW > 0f ? w * (spriteH / spriteW) : w;
            Vector2 c = ScreenOf(GroupCentre(groupIndex), pan, zoom);
            return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
        }
    }
}
