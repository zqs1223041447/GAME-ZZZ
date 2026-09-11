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
    }
}
