using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S5U-WO-02（规划 AI 授权的 presentation-only 通用数值格式化器）：
    /// HUD 大数值压缩显示（999→999 / 12,450→12.5K / 9,999,999→10.0M）。
    /// S5U-WO-03 合同修正（规划 AI）：rounded-unit promotion——按段舍入后达上段阈值则升位
    /// （999,999→"1.0M"，禁止出现 1000.0K / 1000.0M 型边界伪影；T 段同理）。
    /// 底层值零改动、零写回；通用阈值（不得对 PlayerBaseLife 或任意特定值写特殊分支）；
    /// 纯函数、零分配路径（string.Format 一次）。
    /// </summary>
    public static class SliceHudFormat
    {
        /// <summary>K 段起点（≥1000 进 K）。</summary>
        public const float KThreshold = 1000f;
        /// <summary>M 段起点（≥1,000,000 进 M）。</summary>
        public const float MThreshold = 1000000f;
        /// <summary>T 段起点（≥1,000,000,000,000 进 T；M 段上界=升位锚）。</summary>
        public const float TThreshold = 1000000000000f;

        /// <summary>HUD 紧凑格式：&lt;K 原值整数；K/M/T 段保留 1 位小数；舍入升位；负数原样整数。</summary>
        public static string Compact(float value)
        {
            float v = Mathf.Abs(value);
            if (value < 0f || v < KThreshold)
                return Mathf.RoundToInt(value).ToString();
            if (v < MThreshold)
                return FormatScaled(value, KThreshold, "K", MThreshold, "M");
            if (v < TThreshold)
                return FormatScaled(value, MThreshold, "M", TThreshold, "T");
            return FormatScaled(value, TThreshold, "T", float.PositiveInfinity, null);
        }

        /// <summary>段格式化：1 位小数；舍入后达到上一段阈值=升位（禁 1000.0K/1000.0M/1000.0T 伪影）。</summary>
        static string FormatScaled(float value, float div, string suffix, float nextThreshold, string nextSuffix)
        {
            float scaled = value / div;
            if (nextSuffix != null && Mathf.RoundToInt(scaled * 10f) >= Mathf.RoundToInt(nextThreshold / div * 10f))
            {
                // 999.96K → 舍入显示 1000.0K → 提升到 M 段（1.0M）
                return (value / nextThreshold).ToString("0.0", CultureInfoInvariant) + nextSuffix;
            }
            return scaled.ToString("0.0", CultureInfoInvariant) + suffix;
        }

        /// <summary>恒定不变文化（小数点=‘.’，中文环境不漂移）。</summary>
        static System.Globalization.CultureInfo CultureInfoInvariant
        {
            get { return System.Globalization.CultureInfo.InvariantCulture; }
        }
    }
}
