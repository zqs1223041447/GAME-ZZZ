using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S6P-WO-05：被动树 LOD 切档与命中半径（纯函数，tests 与 renderer 共用）。
    /// 切档只看物理屏幕投影，不看 raw zoom。
    /// </summary>
    public static class PassiveTreeLod
    {
        public const float NormalNodeDesignSize = 46f;
        public const float Lod1MinPx = 8f;
        public const float Lod2MinPx = 18f;
        public const float MinHitRadiusPx = 6f;
        public const float HitRadiusMul = 1.2f;

        public enum Level
        {
            Overview = 0,
            Mid = 1,
            Detail = 2
        }

        public static float NormalProjectedPx(float treeZoom, float designScale)
        {
            return NormalNodeDesignSize * treeZoom * designScale;
        }

        public static Level FromProjectedPx(float normalProjectedPx)
        {
            if (normalProjectedPx < Lod1MinPx)
                return Level.Overview;
            if (normalProjectedPx < Lod2MinPx)
                return Level.Mid;
            return Level.Detail;
        }

        public static Level FromZoom(float treeZoom, float designScale)
        {
            return FromProjectedPx(NormalProjectedPx(treeZoom, designScale));
        }

        /// <summary>G1/G2/G3 代表档：给定目标投影像素反推 zoom。</summary>
        public static float ZoomForProjectedPx(float projectedPx, float designScale)
        {
            float ds = designScale <= 0f ? 1f : designScale;
            return projectedPx / (NormalNodeDesignSize * ds);
        }

        public static float HitRadiusPx(int kind, float treeZoom, float designScale)
        {
            float projected = PoeTreeView.NodeSize(kind) * treeZoom * designScale * 0.5f;
            float r = projected * HitRadiusMul;
            return r < MinHitRadiusPx ? MinHitRadiusPx : r;
        }

        public static bool AllowsFullIcon(Level lod, PoeNodeKind kind)
        {
            if (lod == Level.Overview)
                return false;
            if (lod == Level.Mid)
                return kind == PoeNodeKind.Notable || kind == PoeNodeKind.Keystone
                    || kind == PoeNodeKind.Mastery || kind == PoeNodeKind.Start;
            return true;
        }

        public static bool AllowsGroupDecoration(Level lod, float groupProjectedDiameterPx)
        {
            if (lod == Level.Overview)
                return false;
            if (lod == Level.Mid)
                return groupProjectedDiameterPx >= 160f;
            return true;
        }

        public static float GroupProjectedDiameterPx(int groupIndex, float treeZoom, float designScale)
        {
            return 2f * PoeTreeView.GroupRadius(groupIndex) * treeZoom * designScale;
        }

        /// <summary>
        /// 命中 NodeId。probe 与 pan 同为设计空间（GUI 组内）。LOD 不是输入。
        /// 物理半径 = max(NodeSize*zoom*designScale*0.5*1.2, 6px)；平局取较小 NodeId。
        /// </summary>
        public static int HitNodeId(Vector2 probeDesign, Vector2 pan, float zoom, float designScale)
        {
            if (designScale <= 0f)
                designScale = 1f;
            PoeNode[] nodes = PoeTree.Nodes;
            if (nodes == null)
                return -1;
            int best = -1;
            float bestD2 = float.MaxValue;
            for (int i = 0; i < nodes.Length; i++)
            {
                Vector2 c = PoeTreeView.ScreenOf(new Vector2(nodes[i].x, nodes[i].y), pan, zoom);
                float hrDesign = HitRadiusPx(nodes[i].kind, zoom, designScale) / designScale;
                float dx = probeDesign.x - c.x;
                float dy = probeDesign.y - c.y;
                float d2 = dx * dx + dy * dy;
                if (d2 > hrDesign * hrDesign)
                    continue;
                if (best < 0 || d2 < bestD2 - 1e-10f || (d2 <= bestD2 + 1e-10f && i < best))
                {
                    best = i;
                    bestD2 = d2;
                }
            }
            return best;
        }

        public static Vector2 FocusPan(Rect viewport, float zoom, int nodeId)
        {
            PoeNode n = PoeTree.Get(nodeId);
            return new Vector2(viewport.x + viewport.width * 0.5f - n.x * zoom,
                viewport.y + viewport.height * 0.5f - n.y * zoom);
        }
    }
}
